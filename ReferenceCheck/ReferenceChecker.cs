using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace ReferenceCheck
{

    /// <summary>
    /// Base class for running checks on reference numbers
    /// </summary>
    public abstract class ReferenceChecker
    {

        //DMBTools.GeneralTools MyTools = new DMBTools.GeneralTools();

        /// <summary>
        /// Checks if a character is a numeric
        /// </summary>
        /// <param name="passedCharacter">The character(s) to be checked for a numerical value</param>
        /// <returns></returns>
        public bool checkNumeric(string passedCharacter)
        {
            double dummyValue = new double();
            System.Globalization.CultureInfo myCultureInfo = new System.Globalization.CultureInfo("en-US", true);
            return double.TryParse(passedCharacter, System.Globalization.NumberStyles.Any, myCultureInfo.NumberFormat, out dummyValue);
        }

        /// <summary>
        /// Checks whether a reference number is valid
        /// </summary>
        /// <param name="reference">The reference to be checked</param>
        /// <returns>Either the validated reference number or an empty string</returns>
        public string checkReference(string reference)
        {
            if (reference == "")
            {
                return "";
            }
            if (reference.Length != referenceLength())
            {
                return "";
            }
            Regex objEndCharacters = new Regex(regularExpression());
            if (objEndCharacters.IsMatch(reference) == false)
            {
                return "";
            }
            if (reference.Substring(ccPosition(), 1).ToUpper() != checkCharacter(weightedValue(reference)))
            {
                return "";
            }
            return reference.Substring(0,referenceLength());
        }

        /// <summary>
        /// Calculate the value of the reference being checked. The weightable value is effectively converting
        /// each character into a numerical value then producing a total of those values
        /// </summary>
        /// <param name="reference">The reference to be checked</param>
        /// <returns>The total value of the characters in the reference</returns>
        private Int32 weightedValue(string reference)
        {
            Int32 result = 0;
            int[] weight = weightedValues();
            for (int counter = 0; counter <= weight.GetUpperBound(0); counter++)
            {
                if (weight[counter] != 0)
                {
                    if (checkNumeric(reference.Substring(counter, 1)) == true)
                    {
                        result = result + int.Parse(reference.Substring(counter, 1)) * weight[counter];
                    }
                    else
                    {
                        //result = result + (ConvertedNumeric(reference.Substring(counter, 1)) * weight[counter]);
                        result = result + ConvertedNumeric(reference.Substring(counter, 1), weight[counter]);
                        //* weight[counter]
                    }
                }
            }
            return result + initialWeight();
        }

        /// <summary>
        /// Calculates a check character based on the total value of the characters in the reference
        /// </summary>
        /// <param name="value">The total value of the characters in the reference</param>
        /// <returns>The check chracter which should apply to the reference</returns>
        private string checkCharacter(int value)
        {
            string result = possibleCCs().Substring(value % modValue(), 1);
            return result;
        }

        /// <summary>
        /// Returns the regular expression that determines the format of the reference
        /// </summary>
        /// <returns>The regular expression relevant to the reference type</returns>
        public abstract string regularExpression();
        
        /// <summary>
        /// Returns the value to be applied against each character in the reference
        /// </summary>
        /// <returns>An integer array with the values to be applied </returns>
        public abstract int[] weightedValues();
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public abstract string possibleCCs();
        
        /// <summary>
        /// Returns the list of the possible check characters for the reference type
        /// </summary>
        /// <returns>A string containing the possible check characters</returns>
        public abstract int ccPosition();
        
        /// <summary>
        /// Returns the expected length of the reference number
        /// </summary>
        /// <returns>An integer showing the expected length of the reference</returns>
        public abstract int referenceLength();
        
        /// <summary>
        /// Returns the tyep of modulus check that applies to a specified reference type
        /// </summary>
        /// <returns>An integer showing the modulus value to apply</returns>
        public abstract int modValue();
        
        /// <summary>
        /// Returns any initial value that should be included when calculating the weight of a reference
        /// </summary>
        /// <returns></returns>
        public abstract int initialWeight();

        /// <summary>
        /// Converts an alpha character in the reference to a numeric - for reference types where alpha
        /// characters form part of the weighted value for a reference number
        /// </summary>
        /// <param name="AlphaChar">The character to be converted to a numeric</param>
        /// <param name="Multiplier">The weighted value to be applied to the alpha character</param>
        /// <returns>An integer representing the associated value for the character</returns>
        public abstract int ConvertedNumeric(string AlphaChar,int Multiplier); 

    }

    /// <summary>
    /// Checks the validity of a COTAX reference
    /// </summary>
    public class CotaxReferenceChecker : ReferenceChecker
    {

        /// <summary>
        /// Returns the expected structure for a COTAX reference
        /// </summary>
        /// <returns>A string containing a regular expression with the expected COTAX structure</returns>
        public override string regularExpression()
        {
            return @"\b\d{10}A001\b";
        }

        /// <summary>
        /// Returns weight that applies to each character in the COTAX reference
        /// </summary>
        /// <returns>An integer array</returns>
        public override int[] weightedValues()
        {
            return new int[] { 0, 6, 7, 8, 9, 10, 5, 4, 3,2 };
        }

        /// <summary>
        /// Returns the check characters that apply to a COTAX reference
        /// </summary>
        /// <returns>A string - the order of the check characters are based on the associated remainder
        /// following a modulus check</returns>
        public override string possibleCCs()
        {
            return "21987654321";
        }

        /// <summary>
        /// Returns the position of the check character within the reference
        /// </summary>
        /// <returns>An integer</returns>
        public override int ccPosition()
        {
            return 0;
        }

        /// <summary>
        /// Returns the expected length of a COTAX reference number
        /// </summary>
        /// <returns>An integer</returns>
        public override int referenceLength()
        {
            return 14;
        }

        /// <summary>
        /// Returns the nodulus value that applies when calculating the check character
        /// </summary>
        /// <returns>An integer</returns>
        public override int modValue()
        {
            return 11;
        }

        /// <summary>
        /// Returns the initial value to be used when calculating the 'weight' of the characters in the
        /// reference number</summary>
        /// <returns>An integer - 0 (there is no initial value required for this reference type)</returns>
        public override int initialWeight()
        {
            return 0;
        }

        /// <summary>
        /// Converts an alpha character in the reference to a numeric - for reference types where alpha
        /// characters form part of the weighted value for a reference number
        /// </summary>
        /// <param name="AlphaChar">The character to be converted to a numeric</param>
        /// <param name="Multiplier">The weighted value to be applied to the alpha character</param>
        /// <returns>An integer representing the associated value for the character</returns>
        public override int ConvertedNumeric(string AlphaChar, int Multiplier)
        {
            return 0;
        }

    }

    /// <summary>
    /// Checks the validity of a SA reference
    /// </summary>
    public class SAReferenceChecker : ReferenceChecker
    {

        /// <summary>
        /// Returns the expected structure for a SA reference
        /// </summary>
        /// <returns>A string containing a regular expression with the expected SA structure</returns>
        public override string regularExpression()
        {
            return @"\b\d{10}[kK]\b";
        }

        /// <summary>
        /// Returns weight that applies to each character in the SA reference
        /// </summary>
        /// <returns>An integer array - the order of the values match the order of the characters in the
        /// reference</returns>
        public override int[] weightedValues()
        {
            return new int[] { 0, 6, 7, 8, 9, 10, 5, 4, 3, 2 ,1 };
        }

        /// <summary>
        /// Returns the check characters that apply to a SAFE reference
        /// </summary>
        /// <returns>A string - the order of the check characters are based on the associated remainder
        /// following a modulus check</returns>
        public override string possibleCCs()
        {
            return "21987654321";
        }

        /// <summary>
        /// Returns the position of the check character within the reference
        /// </summary>
        /// <returns>An integer</returns>
        public override int ccPosition()
        {
            return 0;
        }

        /// <summary>
        /// Returns the expected length of a SA reference number
        /// </summary>
        /// <returns></returns>
        public override int referenceLength()
        {
            return 11;
        }

        /// <summary>
        /// Returns the nodulus value that applies when calculating the check character
        /// </summary>
        /// <returns>An integer</returns>
        public override int modValue()
        {
            return 11;
        }

        /// <summary>
        /// Returns the initial value to be used when calculating the 'weight' of the characters in the
        /// reference number
        /// </summary>
        /// <returns>An integer - 0 (there is no initial value required for this reference type) </returns>
        public override int initialWeight()
        {
            return 0;
        }

        /// <summary>
        /// Converts an alpha character in the reference to a numeric - for reference types where alpha
        /// characters form part of the weighted value for a reference number
        /// </summary>
        /// <param name="AlphaChar">The character to be converted to a numeric</param>
        /// <param name="Multiplier">The weighted value to be applied to the alpha character</param>
        /// <returns>An integer representing the associated value for the character</returns>
        public override int ConvertedNumeric(string AlphaChar, int Multiplier)
        {
            return 0;
        }

    }

    /// <summary>
    /// Checks the validity of a PAYE reference
    /// </summary>
    public class PAYERReference : ReferenceChecker
    {

        //DMBTools.GeneralTools MyTools = new DMBTools.GeneralTools();

        /// <summary>
        /// Returns the expected structure for a PAYE reference
        /// </summary>
        /// <returns>A string containing a regular expression with the expected PAYE structure</returns>
        public override string regularExpression()
        {
            return @"\b\d{3}[pP][a-zA-Z]\d{7}[0-9xX]\b";
        }

        /// <summary>
        /// Returns weight that applies to each character in the PAYE reference
        /// </summary>
        /// <returns>An integer array - the order of the values match the order of the characters in the
        /// reference</returns>
        public override int[] weightedValues()
        {
            return new int[] { 9, 10, 11, 0, 0, 8, 7, 6, 5, 4, 3, 2, 1 };
        }

        /// <summary>
        /// Returns the check characters that apply to a PAYE reference
        /// </summary>
        /// <returns>A string - the order of the check characters are based on the associated remainder
        /// following a modulus check</returns>
        public override string possibleCCs()
        {
            return "ABCDEFGHXJKLMNYPQRSTZVW";
        }

        /// <summary>
        /// Returns the position of the check character within the reference
        /// </summary>
        /// <returns>An integer</returns>
        public override int ccPosition()
        {
            return 4;
        }

        /// <summary>
        /// Returns the expected length of a PAYE reference number
        /// </summary>
        /// <returns>An integer</returns>
        public override int referenceLength()
        {
            return 13;
        }

        /// <summary>
        /// Returns the nodulus value that applies when calculating the check character
        /// </summary>
        /// <returns>An integer</returns>
        public override int modValue()
        {
            return 23;
        }

        /// <summary>
        /// Returns the initial value to be used when calculating the 'weight' of the characters in the
        /// reference number
        /// </summary>
        /// <returns>An integer - 0 (there is no initial value required for this reference type)</returns>
        public override int initialWeight()
        {
            return 576;
        }

        /// <summary>
        /// Converts an alpha character in the reference to a numeric - for reference types where alpha
        /// characters form part of the weighted value for a reference number
        /// </summary>
        /// <param name="AlphaChar">The character to be converted to a numeric</param>
        /// <param name="Multiplier">The weighted value to be applied to the alpha character</param>
        /// <returns>An integer representing the associated value for the character</returns>
        public override int ConvertedNumeric(string AlphaChar,int Multiplier)
        {
            return 41;
        }

    }

    /// <summary>
    /// Checks the validity of a SAFE reference
    /// </summary>
    public class SAFEChargeReferenceChecker : ReferenceChecker
    {

        //DMBTools.GeneralTools MyTools = new DMBTools.GeneralTools();

        /// <summary>
        /// Returns the expected structure for a SAFE reference
        /// </summary>
        /// <returns>A string containing a regular expression with the expected SAFE structure</returns>
        public override string regularExpression()
        {
            return @"\b[xX][a-zA-Z]\w\d{11}\b";
        }

        /// <summary>
        /// Returns weight that applies to each character in the SAFE reference
        /// </summary>
        /// <returns>An integer array - the order of the values match the order of the characters in the
        /// reference</returns>
        public override int[] weightedValues()
        {
            return new int[] { 0, 0, 9, 10, 11, 12, 13, 8, 7, 6, 5, 4, 3, 2 };
        }

        /// <summary>
        /// Returns the check characters that apply to a SAFE reference
        /// </summary>
        /// <returns>A string - the order of the check characters are based on the associated remainder
        /// following a modulus check</returns>
        public override string possibleCCs()
        {
            return "ABCDEFGHXJKLMNYPQRSTZVW";
        }

        /// <summary>
        /// Returns the position of the check character within the reference
        /// </summary>
        /// <returns>An integer</returns>
        public override int ccPosition()
        {
            return 1;
        }

        /// <summary>
        /// Returns the expected length of a SAFE reference number
        /// </summary>
        /// <returns></returns>
        public override int referenceLength()
        {
            return 14;
        }

        /// <summary>
        /// Returns the nodulus value that applies when calculating the check character
        /// </summary>
        /// <returns>An integer</returns>
        public override int modValue()
        {
            return 23;
        }

        /// <summary>
        /// Returns the initial value to be used when calculating the 'weight' of the characters in the
        /// reference number
        /// </summary>
        /// <returns>An integer - 0 (there is no initial value required for this reference type) </returns>
        public override int initialWeight()
        {
            return 0;
        }

        /// <summary>
        /// Converts an alpha character in the reference to a numeric - for reference types where alpha
        /// characters form part of the weighted value for a reference number
        /// </summary>
        /// <param name="AlphaChar">The character to be converted to a numeric</param>
        /// <param name="Multiplier">The weighted value to be applied to the alpha character</param>
        /// <returns>An integer representing the associated value for the character</returns>
        public override int ConvertedNumeric(string AlphaChar,int Multiplier)
        {
            if (checkNumeric(AlphaChar.ToString()) == false)
            {
                return AlphaToNumeric(AlphaChar.ToString())* Multiplier;
            }
            else
            {
                return 0;
            }
            //throw new NotImplementedException();
        }

        private int AlphaToNumeric(string Alpha)
        {
            int localValue = 0;
            switch (Alpha.ToUpper())
            {
                case "A":
                    localValue = 33;
                    break;
                case "B":
                    localValue = 34;
                    break;
                case "C":
                    localValue = 35;
                    break;
                case "D":
                    localValue = 36;
                    break;
                case "E":
                    localValue = 37;
                    break;
                case "F":
                    localValue = 38;
                    break;
                case "G":
                    localValue = 39;
                    break;
                case "H":
                    localValue = 40;
                    break;
                case "I":
                    localValue = 41;
                    break;
                case "J":
                    localValue = 42;
                    break;
                case "K":
                    localValue = 43;
                    break;
                case "L":
                    localValue = 44;
                    break;
                case "M":
                    localValue = 45;
                    break;
                case "N":
                    localValue = 46;
                    break;
                case "O":
                    localValue = 47;
                    break;
                case "P":
                    localValue = 48;
                    break;
                case "Q":
                    localValue = 49;
                    break;
                case "R":
                    localValue = 50;
                    break;
                case "S":
                    localValue = 51;
                    break;
                case "T":
                    localValue = 52;
                    break;
                case "U":
                    localValue = 53;
                    break;
                case "V":
                    localValue = 54;
                    break;
                case "W":
                    localValue = 55;
                    break;
                case "X":
                    localValue = 56;
                    break;
                case "Y":
                    localValue = 57;
                    break;
                case "Z":
                    localValue = 58;
                    break;
                default:
                    localValue = 0;
                    break;
            }
            return localValue;
        }
    }

    /// <summary>
    /// Checks the validity of a SAFE reference
    /// </summary>
    public class SAFECARReferenceChecker : ReferenceChecker
    {

        //DMBTools.GeneralTools MyTools = new DMBTools.GeneralTools();

        /// <summary>
        /// Returns the expected structure for a SAFE reference
        /// </summary>
        /// <returns>A string containing a regular expression with the expected SAFE structure</returns>
        public override string regularExpression()
        {
            return @"\b[xX][a-zA-Z]\w\d{12}\b";
        }

        /// <summary>
        /// Returns weight that applies to each character in the SAFE reference
        /// </summary>
        /// <returns>An integer array - the order of the values match the order of the characters in the
        /// reference</returns>
        public override int[] weightedValues()
        {
            return new int[] { 0, 0, 9, 10, 11, 12, 13, 8, 7, 6, 5, 4, 3, 2, 1 };
        }

        /// <summary>
        /// Returns the check characters that apply to a SAFE reference
        /// </summary>
        /// <returns>A string - the order of the check characters are based on the associated remainder
        /// following a modulus check</returns>
        public override string possibleCCs()
        {
            return "ABCDEFGHXJKLMNYPQRSTZVW";
        }

        /// <summary>
        /// Returns the position of the check character within the reference
        /// </summary>
        /// <returns>An integer</returns>
        public override int ccPosition()
        {
            return 1;
        }

        /// <summary>
        /// Returns the expected length of a SAFE reference number
        /// </summary>
        /// <returns></returns>
        public override int referenceLength()
        {
            return 15;
        }

        /// <summary>
        /// Returns the nodulus value that applies when calculating the check character
        /// </summary>
        /// <returns>An integer</returns>
        public override int modValue()
        {
            return 23;
        }

        /// <summary>
        /// Returns the initial value to be used when calculating the 'weight' of the characters in the
        /// reference number
        /// </summary>
        /// <returns>An integer - 0 (there is no initial value required for this reference type) </returns>
        public override int initialWeight()
        {
            return 0;
        }

        /// <summary>
        /// Converts an alpha character in the reference to a numeric - for reference types where alpha
        /// characters form part of the weighted value for a reference number
        /// </summary>
        /// <param name="AlphaChar">The character to be converted to a numeric</param>
        /// <param name="Multiplier">The weighted value to be applied to the alpha character</param>
        /// <returns>An integer representing the associated value for the character</returns>
        public override int ConvertedNumeric(string AlphaChar, int Multiplier)
        {
            if (checkNumeric(AlphaChar.ToString()) == false)
            {
                return AlphaToNumeric(AlphaChar.ToString()) * Multiplier;
            }
            else
            {
                return 0;
            }
        }

        private int AlphaToNumeric(string Alpha)
        {
            int localValue = 0;
            switch (Alpha.ToUpper())
            {
                case "A":
                    localValue = 33;
                    break;
                case "B":
                    localValue = 34;
                    break;
                case "C":
                    localValue = 35;
                    break;
                case "D":
                    localValue = 36;
                    break;
                case "E":
                    localValue = 37;
                    break;
                case "F":
                    localValue = 38;
                    break;
                case "G":
                    localValue = 39;
                    break;
                case "H":
                    localValue = 40;
                    break;
                case "I":
                    localValue = 41;
                    break;
                case "J":
                    localValue = 42;
                    break;
                case "K":
                    localValue = 43;
                    break;
                case "L":
                    localValue = 44;
                    break;
                case "M":
                    localValue = 45;
                    break;
                case "N":
                    localValue = 46;
                    break;
                case "O":
                    localValue = 47;
                    break;
                case "P":
                    localValue = 48;
                    break;
                case "Q":
                    localValue = 49;
                    break;
                case "R":
                    localValue = 50;
                    break;
                case "S":
                    localValue = 51;
                    break;
                case "T":
                    localValue = 52;
                    break;
                case "U":
                    localValue = 53;
                    break;
                case "V":
                    localValue = 54;
                    break;
                case "W":
                    localValue = 55;
                    break;
                case "X":
                    localValue = 56;
                    break;
                case "Y":
                    localValue = 57;
                    break;
                case "Z":
                    localValue = 58;
                    break;
                default:
                    localValue = 0;
                    break;
            }
            return localValue;
        }
    }

    /// <summary>
    /// Checks the validity of an IT-CP reference
    /// </summary>
    public class ITCPReference : ReferenceChecker
    {

        /// <summary>
        /// Returns the expected structure for a IT-CP reference
        /// </summary>
        /// <returns>A string containing a regular expression with the expected IT-CP structure</returns>
        public override string regularExpression()
        {
            return @"\b\d{3}[fF][a-zA-Z]\d{6}\b";
        }

        /// <summary>
        /// Returns weight that applies to each character in the IT-CP reference
        /// </summary>
        /// <returns>An integer array - the order of the values match the order of the characters in the
        /// reference</returns>
        public override int[] weightedValues()
        {
            return new int[] { 9, 10, 11, 0, 0, 8, 7, 6, 5, 4, 3};
        }

        /// <summary>
        /// Returns the check characters that apply to a IT-CP reference
        /// </summary>
        /// <returns>A string - the order of the check characters are based on the associated remainder
        /// following a modulus check</returns>
        public override string possibleCCs()
        {
            return "ABCDEFGHXJKLMNYPQRSTZVW";
        }

        /// <summary>
        /// Returns the position of the check character within the reference
        /// </summary>
        /// <returns>An integer</returns>
        public override int ccPosition()
        {
            return 4;
        }

        /// <summary>
        /// Returns the expected length of a IT-CP reference number
        /// </summary>
        /// <returns>An integer</returns>
        public override int referenceLength()
        {
            return 11;
        }

        /// <summary>
        /// Returns the nodulus value that applies when calculating the check character
        /// </summary>
        /// <returns>An integer</returns>
        public override int modValue()
        {
            return 23;
        }

        /// <summary>
        /// Returns the initial value to be used when calculating the 'weight' of the characters in the
        /// reference number
        /// </summary>
        /// <returns>An integer - 0 (there is no initial value required for this reference type)</returns>
        public override int initialWeight()
        {
            return 420;
        }

        /// <summary>
        /// Converts an alpha character in the reference to a numeric - for reference types where alpha
        /// characters form part of the weighted value for a reference number
        /// </summary>
        /// <param name="AlphaChar">The character to be converted to a numeric</param>
        /// <param name="Multiplier">The weighted value to be applied to the alpha character</param>
        /// <returns>An integer representing the associated value for the character</returns>
        public override int ConvertedNumeric(string AlphaChar, int Multiplier)
        {
            return 0;
        }

    }

    /// <summary>
    /// Checks the validity of a CT reference
    /// </summary>
    public class CTReferenceChecker : ReferenceChecker
    {

        /// <summary>
        /// Returns the expected structure for a CT reference
        /// </summary>
        /// <returns>A string containing a regular expression with the expected CT structure</returns>
        public override string regularExpression()
        {
            return @"\b\d{3}[cC][a-zA-Z]\d{6}\b";
        }

        /// <summary>
        /// Returns weight that applies to each character in the CT reference
        /// </summary>
        /// <returns>An integer array - the order of the values match the order of the characters in the
        /// reference</returns>
        public override int[] weightedValues()
        {
            return new int[] { 9, 10, 11, 0, 0, 8, 7, 6, 5, 4, 3 };
        }

        /// <summary>
        /// Returns the check characters that apply to a CT reference
        /// </summary>
        /// <returns>A string - the order of the check characters are based on the associated remainder
        /// following a modulus check</returns>
        public override string possibleCCs()
        {
            return "ABCDEFGHXJKLMNYPQRSTZVW";
        }

        /// <summary>
        /// Returns the position of the check character within the reference
        /// </summary>
        /// <returns>An integer</returns>
        public override int ccPosition()
        {
            return 4;
        }

        /// <summary>
        /// Returns the expected length of a CT reference number
        /// </summary>
        /// <returns>An integer</returns>
        public override int referenceLength()
        {
            return 11;
        }

        /// <summary>
        /// Returns the nodulus value that applies when calculating the check character
        /// </summary>
        /// <returns>An integer</returns>
        public override int modValue()
        {
            return 23;
        }

        /// <summary>
        /// Returns the initial value to be used when calculating the 'weight' of the characters in the
        /// reference number
        /// </summary>
        /// <returns>An integer - 0 (there is no initial value required for this reference type)</returns>
        public override int initialWeight()
        {
            return 420;
        }

        /// <summary>
        /// Converts an alpha character in the reference to a numeric - for reference types where alpha
        /// characters form part of the weighted value for a reference number
        /// </summary>
        /// <param name="AlphaChar">The character to be converted to a numeric</param>
        /// <param name="Multiplier">The weighted value to be applied to the alpha character</param>
        /// <returns>An integer representing the associated value for the character</returns>
        public override int ConvertedNumeric(string AlphaChar, int Multiplier)
        {
            return 0;
        }

    }

    /// <summary>
    /// Checks the validity of a NTC reference
    /// </summary>
    public class NTCRReference : ReferenceChecker
    {

        //DMBTools.GeneralTools MyTools = new DMBTools.GeneralTools();

        /// <summary>
        /// Returns the expected structure for a NTC reference
        /// </summary>
        /// <returns>A string containing a regular expression with the expected NTC structure</returns>
        public override string regularExpression()
        {
            return @"\b[^dfioquvDFIOQUV0-9]{2}(?<!GB|gb|NK|nk|TN|tn|ZZ|zz)\d{12}[nN][a-zA-Z]\b";
        }

        /// <summary>
        /// Returns weight that applies to each character in the NTC reference
        /// </summary>
        /// <returns>An integer array - the order of the values match the order of the characters in the
        /// reference</returns>
        public override int[] weightedValues()
        {
            return new int[] { 256, 128, 64, 32, 16, 8, 4, 2, 0, 0, 0, 0, 0, 0, 0, 0 };
        }

        /// <summary>
        /// Returns the check characters that apply to a NTC reference
        /// </summary>
        /// <returns>A string - the order of the check characters are based on the associated remainder
        /// following a modulus check</returns>
        public override string possibleCCs()
        {
            return "ABCDEFGHJKLMNPQRSTVWXYZ";
        }

        /// <summary>
        /// Returns the position of the check character within the reference
        /// </summary>
        /// <returns>An integer</returns>
        public override int ccPosition()
        {
            return 15;
        }

        /// <summary>
        /// Returns the expected length of a NTC reference number
        /// </summary>
        /// <returns>An integer</returns>
        public override int referenceLength()
        {
            return 16;
        }

        /// <summary>
        /// Returns the nodulus value that applies when calculating the check character
        /// </summary>
        /// <returns>An integer</returns>
        public override int modValue()
        {
            return 23;
        }

        /// <summary>
        /// Returns the initial value to be used when calculating the 'weight' of the characters in the
        /// reference number
        /// </summary>
        /// <returns>An integer - 0 (there is no initial value required for this reference type)</returns>
        public override int initialWeight()
        {
            return 0;
        }

        /// <summary>
        /// Verifies the NTC reference including ensuring that the date is earlier than today
        /// </summary>
        /// <param name="passedReference">The reference to be checked</param>
        /// <returns>Either the validated reference or an empty string</returns>
        /// <remarks>This should </remarks>
        public string CheckReferenceIncludingDate(string passedReference)
        {
            string result = "";
            if (passedReference == null || passedReference == "")
            {
                return "";
            }
            result = checkReference(passedReference);
            if (result != "")
            {
                string date = passedReference.Substring(8, 6);
                try
                {
                    if (DateTime.Parse(date.Substring(0, 2) + @"/" + date.Substring(2, 2) + @"/" + date.Substring(4, 2)) > DateTime.Now)
                    {
                        result = "";
                    }
                }
                catch
                {
                    result = "";
                    return result;
                }
            }
            else
            {
                result = "";
            }
            return result;
        }

        /// <summary>
        /// Converts an alpha character in the reference to a numeric - for reference types where alpha
        /// characters form part of the weighted value for a reference number
        /// </summary>
        /// <param name="AlphaChar">The character to be converted to a numeric</param>
        /// <param name="Multiplier">The weighted value to be applied to the alpha character</param>
        /// <returns>An integer representing the associated value for the character</returns>
        public override int ConvertedNumeric(string AlphaChar, int Multiplier)
        {
            if (checkNumeric(AlphaChar.ToString()) == false)
            {
                return AlphaToNumeric(AlphaChar.ToString()) * Multiplier;
            }
            else
            {
                return 0;
            }
        }

        private int AlphaToNumeric(string Alpha)
        {
            int localValue = 0;
            switch (Alpha.ToUpper())
            {
                case "A":
                    localValue = 33;
                    break;
                case "B":
                    localValue = 34;
                    break;
                case "C":
                    localValue = 35;
                    break;
                case "E":
                    localValue = 37;
                    break;
                case "G":
                    localValue = 39;
                    break;
                case "H":
                    localValue = 40;
                    break;
                case "J":
                    localValue = 42;
                    break;
                case "K":
                    localValue = 43;
                    break;
                case "L":
                    localValue = 44;
                    break;
                case "M":
                    localValue = 45;
                    break;
                case "N":
                    localValue = 46;
                    break;
                case "P":
                    localValue = 48;
                    break;
                case "R":
                    localValue = 50;
                    break;
                case "S":
                    localValue = 51;
                    break;
                case "T":
                    localValue = 52;
                    break;
                case "W":
                    localValue = 55;
                    break;
                case "X":
                    localValue = 56;
                    break;
                case "Y":
                    localValue = 57;
                    break;
                case "Z":
                    localValue = 58;
                    break;
                default:
                    localValue = 0;
                    break;
            }
            return localValue;
        }

    }

    /// <summary>
    /// Checks the validity of a NINO
    /// </summary>
    public class NINOReference : ReferenceChecker
    {

        //DMBTools.GeneralTools MyTools = new DMBTools.GeneralTools();

        /// <summary>
        /// Returns the expected structure for a NTC reference
        /// </summary>
        /// <returns>A string containing a regular expression with the expected NTC structure</returns>
        public override string regularExpression()
        {
            return @"[a-zA-Z]{2}\d{6}[a-zA-Z]";
        }

        /// <summary>
        /// Returns weight that applies to each character in the NTC reference
        /// </summary>
        /// <returns>An integer array - the order of the values match the order of the characters in the
        /// reference</returns>
        public override int[] weightedValues()
        {
            return new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        }

        /// <summary>
        /// Returns the check characters that apply to a NTC reference
        /// </summary>
        /// <returns>A string - the order of the check characters are based on the associated remainder
        /// following a modulus check</returns>
        public override string possibleCCs()
        {
            return "ABCDEFGHJKLMNPQRSTVWXYZ";
        }

        /// <summary>
        /// Returns the position of the check character within the reference
        /// </summary>
        /// <returns>An integer</returns>
        public override int ccPosition()
        {
            return 0;
        }

        /// <summary>
        /// Returns the expected length of a NTC reference number
        /// </summary>
        /// <returns>An integer</returns>
        public override int referenceLength()
        {
            return 9;
        }

        /// <summary>
        /// Returns the nodulus value that applies when calculating the check character
        /// </summary>
        /// <returns>An integer</returns>
        public override int modValue()
        {
            return 0;
        }

        /// <summary>
        /// Returns the initial value to be used when calculating the 'weight' of the characters in the
        /// reference number
        /// </summary>
        /// <returns>An integer - 0 (there is no initial value required for this reference type)</returns>
        public override int initialWeight()
        {
            return 0;
        }

        ///Placeholder for minimal checking of a NINO until an EPID is located
        /// <summary>
        /// Verifies the NINO reference including ensuring that the date is earlier than today
        /// </summary>
        /// <param name="passedReference">The reference to be checked</param>
        /// <returns>Either the validated reference or an empty string</returns>
        /// <remarks>This should </remarks>
        public string CheckNINO(string passedReference)
        {
            string result = "";
            Regex objEndCharacters = new Regex(regularExpression());
            if (objEndCharacters.IsMatch(passedReference) == false || passedReference.Length != referenceLength())
            {
                return "";
            }
            else
            {
                return passedReference;
            }
        }

        /// <summary>
        /// Converts an alpha character in the reference to a numeric - for reference types where alpha
        /// characters form part of the weighted value for a reference number
        /// </summary>
        /// <param name="AlphaChar">The character to be converted to a numeric</param>
        /// <param name="Multiplier">The weighted value to be applied to the alpha character</param>
        /// <returns>An integer representing the associated value for the character</returns>
        public override int ConvertedNumeric(string AlphaChar, int Multiplier)
        {
            if (checkNumeric(AlphaChar.ToString()) == false)
            {
                return AlphaToNumeric(AlphaChar.ToString()) * Multiplier;
            }
            else
            {
                return 0;
            }
        }

        private int AlphaToNumeric(string Alpha)
        {
            int localValue = 0;
            switch (Alpha.ToUpper())
            {
                case "A":
                    localValue = 33; ; ;
                    break;
                case "B":
                    localValue = 34;
                    break;
                case "C":
                    localValue = 35;
                    break;
                case "E":
                    localValue = 37;
                    break;
                case "G":
                    localValue = 39;
                    break;
                case "H":
                    localValue = 40;
                    break;
                case "J":
                    localValue = 42;
                    break;
                case "K":
                    localValue = 43;
                    break;
                case "L":
                    localValue = 44;
                    break;
                case "M":
                    localValue = 45;
                    break;
                case "N":
                    localValue = 46;
                    break;
                case "P":
                    localValue = 48;
                    break;
                case "R":
                    localValue = 50;
                    break;
                case "S":
                    localValue = 51;
                    break;
                case "T":
                    localValue = 52;
                    break;
                case "W":
                    localValue = 55;
                    break;
                case "X":
                    localValue = 56;
                    break;
                case "Y":
                    localValue = 57;
                    break;
                case "Z":
                    localValue = 58;
                    break;
                default:
                    localValue = 0;
                    break;
            }
            return localValue;
        }

    }


    





/// <summary>
/// Checks the validity of a SDLT reference
/// </summary>
public class SDLTReferenceChecker : ReferenceChecker
    {

        /// <summary>
        /// Returns the expected structure for a SDLT reference
        /// </summary>
        /// <returns>A string containing a regular expression with the expected SDLT structure</returns>
        public override string regularExpression()
        {
            return @"\b\d{9}[M][A-Z]\b";
        }

        /// <summary>
        /// Returns weight that applies to each character in the SDLT reference
        /// </summary>
        /// <returns>An integer array - the order of the values match the order of the characters in the
        /// reference</returns>
        public override int[] weightedValues()
        {
            return new int[] { 6, 7, 8, 9, 10, 5, 4, 3, 2, 0, 0 };
        }

        /// <summary>
        /// Returns the check characters that apply to a CT reference
        /// </summary>
        /// <returns>A string - the order of the check characters are based on the associated remainder
        /// following a modulus check</returns>
        public override string possibleCCs()
        {
            return "ABCDEFGHXJKLMNYPQRSTZVW";
        }

        /// <summary>
        /// Returns the position of the check character within the reference
        /// </summary>
        /// <returns>An integer</returns>
        public override int ccPosition()
        {
            return 10;
        }

        /// <summary>
        /// Returns the expected length of a CT reference number
        /// </summary>
        /// <returns>An integer</returns>
        public override int referenceLength()
        {
            return 11;
        }

        /// <summary>
        /// Returns the nodulus value that applies when calculating the check character
        /// </summary>
        /// <returns>An integer</returns>
        public override int modValue()
        {
            return 23;
        }

        /// <summary>
        /// Returns the initial value to be used when calculating the 'weight' of the characters in the
        /// reference number
        /// </summary>
        /// <returns>An integer - 0 (there is no initial value required for this reference type)</returns>
        public override int initialWeight()
        {
            return 0;
        }

        /// <summary>
        /// Converts an alpha character in the reference to a numeric - for reference types where alpha
        /// characters form part of the weighted value for a reference number
        /// </summary>
        /// <param name="AlphaChar">The character to be converted to a numeric</param>
        /// <param name="Multiplier">The weighted value to be applied to the alpha character</param>
        /// <returns>An integer representing the associated value for the character</returns>
        public override int ConvertedNumeric(string AlphaChar, int Multiplier)
        {
            return 0;
        }

    }

       /// <summary>
    /// Checks the validity of a 18 character MOSS reference
    /// </summary>
    public class MOSS18ReferenceChecker : ReferenceChecker
    {

        /// <summary>
        /// Returns the expected structure for an 18 character MOSS reference
        /// </summary>
        /// <returns>A string containing a regular expression with the expected COTAX structure</returns>
        public override string regularExpression()
        {
            return @"[a-z A-Z]{2}[0-9]{12}[/][0-9]{3}";
        }

        /// <summary>
        /// Returns weight that applies to each character in the COTAX reference
        /// </summary>
        /// <returns>An integer array</returns>
        public override int[] weightedValues()
        {
            //TODO Awaiting confirmation of whether VAT ref has a check character calculation
            return new int[] { 0, 6, 7, 8, 9, 10, 5, 4, 3, 2 };
        }

        /// <summary>
        /// Returns the check characters that apply to a COTAX reference
        /// </summary>
        /// <returns>A string - the order of the check characters are based on the associated remainder
        /// following a modulus check</returns>
        public override string possibleCCs()
        {
            //TODO Awaiting confirmation of whether VAT ref has a check character calculation
            return "21987654321";
        }

        /// <summary>
        /// Returns the position of the check character within the reference
        /// </summary>
        /// <returns>An integer</returns>
        public override int ccPosition()
        {
            //TODO Awaiting confirmation of whether VAT ref has a check character calculation
            return 0;
        }

        /// <summary>
        /// Returns the expected length of a MOSS reference number
        /// </summary>
        /// <returns>An integer</returns>
        public override int referenceLength()
        {
            return 18;
        }

        /// <summary>
        /// Returns the nodulus value that applies when calculating the check character
        /// </summary>
        /// <returns>An integer</returns>
        public override int modValue()
        {
            //TODO Awaiting confirmation of whether VAT ref has a check character calculation
            return 11;
        }

        /// <summary>
        /// Returns the initial value to be used when calculating the 'weight' of the characters in the
        /// reference number</summary>
        /// <returns>An integer - 0 (there is no initial value required for this reference type)</returns>
        public override int initialWeight()
        {
            return 0;
        }

        /// <summary>
        /// Converts an alpha character in the reference to a numeric - for reference types where alpha
        /// characters form part of the weighted value for a reference number
        /// </summary>
        /// <param name="AlphaChar">The character to be converted to a numeric</param>
        /// <param name="Multiplier">The weighted value to be applied to the alpha character</param>
        /// <returns>An integer representing the associated value for the character</returns>
        public override int ConvertedNumeric(string AlphaChar, int Multiplier)
        {
            return 0;
        }
    }

    /// <summary>
    /// Checks the validity of a 17 character MOSS reference
    /// </summary>
    public class MOSS17ReferenceChecker : ReferenceChecker
    {

        /// <summary>
        /// Returns the expected structure for a 17 character MOSS reference
        /// </summary>
        /// <returns>A string containing a regular expression with the expected MOSS structure</returns>
        public override string regularExpression()
        {
            return @"[a-z A-Z]{2}[EU][0-9]{9}[/][0-9]{3}";
        }

        /// <summary>
        /// Returns weight that applies to each character in the COTAX reference
        /// </summary>
        /// <returns>An integer array</returns>
        public override int[] weightedValues()
        {
            //TODO Awaiting confirmation of whether VAT ref has a check character calculation
            return new int[] { 0, 6, 7, 8, 9, 10, 5, 4, 3, 2 };
        }

        /// <summary>
        /// Returns the check characters that apply to a COTAX reference
        /// </summary>
        /// <returns>A string - the order of the check characters are based on the associated remainder
        /// following a modulus check</returns>
        public override string possibleCCs()
        {
            //TODO Awaiting confirmation of whether VAT ref has a check character calculation
            return "21987654321";
        }

        /// <summary>
        /// Returns the position of the check character within the reference
        /// </summary>
        /// <returns>An integer</returns>
        public override int ccPosition()
        {
            //TODO Awaiting confirmation of whether VAT ref has a check character calculation
            return 0;
        }

        /// <summary>
        /// Returns the expected length of a MOSS reference number
        /// </summary>
        /// <returns>An integer</returns>
        public override int referenceLength()
        {
            return 17;
        }

        /// <summary>
        /// Returns the nodulus value that applies when calculating the check character
        /// </summary>
        /// <returns>An integer</returns>
        public override int modValue()
        {
            //TODO Awaiting confirmation of whether VAT ref has a check character calculation
            return 11;
        }

        /// <summary>
        /// Returns the initial value to be used when calculating the 'weight' of the characters in the
        /// reference number</summary>
        /// <returns>An integer - 0 (there is no initial value required for this reference type)</returns>
        public override int initialWeight()
        {
            return 0;
        }

        /// <summary>
        /// Converts an alpha character in the reference to a numeric - for reference types where alpha
        /// characters form part of the weighted value for a reference number
        /// </summary>
        /// <param name="AlphaChar">The character to be converted to a numeric</param>
        /// <param name="Multiplier">The weighted value to be applied to the alpha character</param>
        /// <returns>An integer representing the associated value for the character</returns>
        public override int ConvertedNumeric(string AlphaChar, int Multiplier)
        {
            return 0;
        }

        /// <summary>
        /// Checks whether a reference number is valid
        /// </summary>
        /// <param name="reference">The reference to be checked</param>
        /// <returns>Either the validated reference number or an empty string</returns>
        public string checkReference(string reference)
        {
            if (reference == "")
            {
                return "";
            }
            if (reference.Length != referenceLength())
            {
                return "";
            }
            Regex objEndCharacters = new Regex(regularExpression());
            if (objEndCharacters.IsMatch(reference) == false)
            {
                return "";
            }
            ReferenceCheck.VAT11ReferenceChecker myRefCheck = new ReferenceCheck.VAT11ReferenceChecker();
            string validatedRef = string.Empty;
            string formattedVATNo = reference.Substring(4, 3) + " " + reference.Substring(7, 4) + " " + reference.Substring(11, 2);
            validatedRef = myRefCheck.checkReference(formattedVATNo);
            if (string.IsNullOrEmpty(validatedRef) == true)
            {
                ReferenceCheck.VAT1155ReferenceChecker myRef55Check = new ReferenceCheck.VAT1155ReferenceChecker();
                validatedRef = myRef55Check.checkReference(formattedVATNo);
                if (string.IsNullOrEmpty(validatedRef) == true)
                {
                    return "";
                }
            }
            return reference.Substring(0, referenceLength());
        }

    }

    /// <summary>
    /// Checks the validity of a 18 character NICO reference
    /// </summary>
    public class NICOReferenceChecker : ReferenceChecker
    {

        public string CheckReference(string reference)
        {
            if (reference.Length != referenceLength())
            {
                return "";
            }
            Regex objEndCharacters = new Regex(regularExpression());
            if (objEndCharacters.IsMatch(reference) == false)
            {
                return "";
            }
            return reference;
        }
        /// <summary>
        /// Returns the expected structure for a 18 character NICO reference
        /// </summary>
        /// <returns>A string containing a regular expression with the expected MOSS structure</returns>
        public override string regularExpression()
        {
            return @"\d{18}";
        }

        /// <summary>
        /// Returns weight that applies to each character in the COTAX reference
        /// </summary>
        /// <returns>An integer array</returns>
        public override int[] weightedValues()
        {
            //TODO Awaiting confirmation of whether VAT ref has a check character calculation
            return new int[] {};
        }

        /// <summary>
        /// Returns the check characters that apply to a COTAX reference
        /// </summary>
        /// <returns>A string - the order of the check characters are based on the associated remainder
        /// following a modulus check</returns>
        public override string possibleCCs()
        {
            //TODO Awaiting confirmation of whether VAT ref has a check character calculation
            return "";
        }

        /// <summary>
        /// Returns the position of the check character within the reference
        /// </summary>
        /// <returns>An integer</returns>
        public override int ccPosition()
        {
            //TODO Awaiting confirmation of whether VAT ref has a check character calculation
            return 0;
        }

        /// <summary>
        /// Returns the expected length of a MOSS reference number
        /// </summary>
        /// <returns>An integer</returns>
        public override int referenceLength()
        {
            return 18;
        }

        /// <summary>
        /// Returns the nodulus value that applies when calculating the check character
        /// </summary>
        /// <returns>An integer</returns>
        public override int modValue()
        {
            //TODO Awaiting confirmation of whether VAT ref has a check character calculation
            return 11;
        }

        /// <summary>
        /// Returns the initial value to be used when calculating the 'weight' of the characters in the
        /// reference number</summary>
        /// <returns>An integer - 0 (there is no initial value required for this reference type)</returns>
        public override int initialWeight()
        {
            return 0;
        }

        /// <summary>
        /// Converts an alpha character in the reference to a numeric - for reference types where alpha
        /// characters form part of the weighted value for a reference number
        /// </summary>
        /// <param name="AlphaChar">The character to be converted to a numeric</param>
        /// <param name="Multiplier">The weighted value to be applied to the alpha character</param>
        /// <returns>An integer representing the associated value for the character</returns>
        public override int ConvertedNumeric(string AlphaChar, int Multiplier)
        {
            return 0;
        }

        /// <summary>
        /// Checks whether a reference number is valid
        /// </summary>
        /// <param name="reference">The reference to be checked</param>
        /// <returns>Either the validated reference number or an empty string</returns>
        public string checkReference(string reference)
        {
            if (reference == "")
            {
                return "";
            }
            if (reference.Length != referenceLength())
            {
                return "";
            }
            Regex objEndCharacters = new Regex(regularExpression());
            if (objEndCharacters.IsMatch(reference) == false)
            {
                return "";
            }
            return reference;
 
        }

    }



    /// <summary>
    /// Checks the validity of a 15 character MOSS reference
    /// </summary>
    public class MOSS15ReferenceChecker : ReferenceChecker
    {

        /// <summary>
        /// Returns the expected structure for a 17 character MOSS reference
        /// </summary>
        /// <returns>A string containing a regular expression with the expected MOSS structure</returns>
        public override string regularExpression()
        {
            return @"[GB][0-9]{9}[/][0-9]{3}";
        }

        /// <summary>
        /// Returns weight that applies to each character in the COTAX reference
        /// </summary>
        /// <returns>An integer array</returns>
        public override int[] weightedValues()
        {
            //TODO Awaiting confirmation of whether VAT ref has a check character calculation
            return new int[] { 0, 6, 7, 8, 9, 10, 5, 4, 3, 2 };
        }

        /// <summary>
        /// Returns the check characters that apply to a COTAX reference
        /// </summary>
        /// <returns>A string - the order of the check characters are based on the associated remainder
        /// following a modulus check</returns>
        public override string possibleCCs()
        {
            //TODO Awaiting confirmation of whether VAT ref has a check character calculation
            return "21987654321";
        }

        /// <summary>
        /// Returns the position of the check character within the reference
        /// </summary>
        /// <returns>An integer</returns>
        public override int ccPosition()
        {
            //TODO Awaiting confirmation of whether VAT ref has a check character calculation
            return 0;
        }

        /// <summary>
        /// Returns the expected length of a MOSS reference number
        /// </summary>
        /// <returns>An integer</returns>
        public override int referenceLength()
        {
            return 15;
        }

        /// <summary>
        /// Returns the nodulus value that applies when calculating the check character
        /// </summary>
        /// <returns>An integer</returns>
        public override int modValue()
        {
            //TODO Awaiting confirmation of whether VAT ref has a check character calculation
            return 11;
        }

        /// <summary>
        /// Returns the initial value to be used when calculating the 'weight' of the characters in the
        /// reference number</summary>
        /// <returns>An integer - 0 (there is no initial value required for this reference type)</returns>
        public override int initialWeight()
        {
            return 0;
        }

        /// <summary>
        /// Converts an alpha character in the reference to a numeric - for reference types where alpha
        /// characters form part of the weighted value for a reference number
        /// </summary>
        /// <param name="AlphaChar">The character to be converted to a numeric</param>
        /// <param name="Multiplier">The weighted value to be applied to the alpha character</param>
        /// <returns>An integer representing the associated value for the character</returns>
        public override int ConvertedNumeric(string AlphaChar, int Multiplier)
        {
            return 0;
        }

        /// <summary>
        /// Checks whether a reference number is valid
        /// </summary>
        /// <param name="reference">The reference to be checked</param>
        /// <returns>Either the validated reference number or an empty string</returns>
        public string checkReference(string reference)
        {
            if (reference == "")
            {
                return "";
            }
            if (reference.Length != referenceLength())
            {
                return "";
            }
            Regex objEndCharacters = new Regex(regularExpression());
            if (objEndCharacters.IsMatch(reference) == false)
            {
                return "";
            }
            ReferenceCheck.VAT11ReferenceChecker myRefCheck = new ReferenceCheck.VAT11ReferenceChecker();
            string validatedRef = string.Empty;
            string formattedVATNo = reference.Substring(2, 3) + " " + reference.Substring(5, 4) + " " + reference.Substring(9, 2);
            validatedRef = myRefCheck.checkReference(formattedVATNo);
            if (string.IsNullOrEmpty(validatedRef) == true)
            {
                ReferenceCheck.VAT1155ReferenceChecker myRef55Check = new ReferenceCheck.VAT1155ReferenceChecker();
                validatedRef = myRef55Check.checkReference(formattedVATNo);
                if (string.IsNullOrEmpty(validatedRef) == true)
                {
                    return "";
                }
            }
            return reference.Substring(0, referenceLength());
        }

    }

    /// <summary>
    /// Checks the validity of a 11 character VAT reference
    /// </summary>
    public class VAT11ReferenceChecker : ReferenceChecker
    {

        //DMBTools.GeneralTools MyTools = new DMBTools.GeneralTools();

        /// <summary>
        /// Returns the expected structure for a 11 character VAT reference
        /// </summary>
        /// <returns>A string containing a regular expression with the expected VAT structure</returns>
        public override string regularExpression()
        {
            return @"[0-9]{3}[ ][0-9]{4}[ ][0-9]{2}";
        }

        /// <summary>
        /// Returns weight that applies to each character in the COTAX reference
        /// </summary>
        /// <returns>An integer array</returns>
        public override int[] weightedValues()
        {
            //TODO Awaiting confirmation of whether VAT ref has a check character calculation
            return new int[] { 8, 7, 6, 5, 4, 3, 2, 0, 0 };
        }

        /// <summary>
        /// Returns the check characters that apply to a COTAX reference
        /// </summary>
        /// <returns>A string - the order of the check characters are based on the associated remainder
        /// following a modulus check</returns>
        public override string possibleCCs()
        {
            //TODO Awaiting confirmation of whether VAT ref has a check character calculation
            return "21987654321";
        }

        /// <summary>
        /// Returns the position of the check character within the reference
        /// </summary>
        /// <returns>An integer</returns>
        public override int ccPosition()
        {
            //TODO Awaiting confirmation of whether VAT ref has a check character calculation
            return 7;
        }

        /// <summary>
        /// Returns the expected length of a VAT reference number
        /// </summary>
        /// <returns>An integer</returns>
        public override int referenceLength()
        {
            return 11;
        }

        /// <summary>
        /// Returns the nodulus value that applies when calculating the check character
        /// </summary>
        /// <returns>An integer</returns>
        public override int modValue()
        {
            //TODO Awaiting confirmation of whether VAT ref has a check character calculation
            return 97;
        }

        /// <summary>
        /// Returns the initial value to be used when calculating the 'weight' of the characters in the
        /// reference number</summary>
        /// <returns>An integer - 0 (there is no initial value required for this reference type)</returns>
        public override int initialWeight()
        {
            return 0;
        }

        /// <summary>
        /// Converts an alpha character in the reference to a numeric - for reference types where alpha
        /// characters form part of the weighted value for a reference number
        /// </summary>
        /// <param name="AlphaChar">The character to be converted to a numeric</param>
        /// <param name="Multiplier">The weighted value to be applied to the alpha character</param>
        /// <returns>An integer representing the associated value for the character</returns>
        public override int ConvertedNumeric(string AlphaChar, int Multiplier)
        {
            return 0;
        }

        /// <summary>
        /// Checks whether a reference number is valid
        /// </summary>
        /// <param name="reference">The reference to be checked</param>
        /// <returns>Either the validated reference number or an empty string</returns>
        /// <remarks>any spaces will be removed from the reference prior to checking the check character</remarks>
        public string checkReference(string reference)
        {
            if (reference == "")
            {
                return "";
            }
            if (reference.Length != referenceLength())
            {
                return "";
            }
            Regex objEndCharacters = new Regex(regularExpression());
            if (objEndCharacters.IsMatch(reference) == false)
            {
                return "";
            }
            string testReference = reference.Replace(" ", "");
            if (testReference.Substring(ccPosition(), 2).ToUpper() != checkCharacter(weightedValue(testReference)))
            {
                return "";
            }
            return reference;
        }

        /// <summary>
        /// Calculate the value of the reference being checked. The weightable value is effectively converting
        /// each character into a numerical value then producing a total of those values
        /// </summary>
        /// <param name="reference">The reference to be checked</param>
        /// <returns>The total value of the characters in the reference</returns>
        private Int32 weightedValue(string reference)
        {
            Int32 result = 0;
            int[] weight = weightedValues();
            for (int counter = 0; counter <= weight.GetUpperBound(0); counter++)
            {
                if (weight[counter] != 0)
                {
                    if (checkNumeric(reference.Substring(counter, 1)) == true)
                    {
                        result = result + int.Parse(reference.Substring(counter, 1)) * weight[counter];
                    }
                    else
                    {
                        //result = result + (ConvertedNumeric(reference.Substring(counter, 1)) * weight[counter]);
                        result = result + ConvertedNumeric(reference.Substring(counter, 1), weight[counter]);
                        //* weight[counter]
                    }
                }
            }
            return result + initialWeight();
        }

        /// <summary>
        /// Calculates a check character based on the total value of the characters in the reference
        /// </summary>
        /// <param name="value">The total value of the characters in the reference</param>
        /// <returns>The check chracter which should apply to the reference</returns>
        private string checkCharacter(int value)
        {
            string result = (97 -(value % modValue())).ToString("00");
            return result;
        }

    }

    /// <summary>
    /// Checks the validity of an 11 character VAT reference
    /// </summary>
    public class VAT1155ReferenceChecker : ReferenceChecker
    {

        //DMBTools.GeneralTools MyTools = new DMBTools.GeneralTools();

        /// <summary>
        /// Returns the expected structure for a 11 character VAT reference
        /// </summary>
        /// <returns>A string containing a regular expression with the expected VAT structure</returns>
        public override string regularExpression()
        {
            return @"[0-9]{3}[ ][0-9]{4}[ ][0-9]{2}";
        }

        /// <summary>
        /// Returns weight that applies to each character in the COTAX reference
        /// </summary>
        /// <returns>An integer array</returns>
        public override int[] weightedValues()
        {
            //TODO Awaiting confirmation of whether VAT ref has a check character calculation
            return new int[] { 8, 7, 6, 5, 4, 3, 2, 0, 0 };
        }

        /// <summary>
        /// Returns the check characters that apply to a COTAX reference
        /// </summary>
        /// <returns>A string - the order of the check characters are based on the associated remainder
        /// following a modulus check</returns>
        public override string possibleCCs()
        {
            //TODO Awaiting confirmation of whether VAT ref has a check character calculation
            return "21987654321";
        }

        /// <summary>
        /// Returns the position of the check character within the reference
        /// </summary>
        /// <returns>An integer</returns>
        public override int ccPosition()
        {
            //TODO Awaiting confirmation of whether VAT ref has a check character calculation
            return 7;
        }

        /// <summary>
        /// Returns the expected length of a VAT reference number
        /// </summary>
        /// <returns>An integer</returns>
        public override int referenceLength()
        {
            return 11;
        }

        /// <summary>
        /// Returns the nodulus value that applies when calculating the check character
        /// </summary>
        /// <returns>An integer</returns>
        public override int modValue()
        {
            //TODO Awaiting confirmation of whether VAT ref has a check character calculation
            return 97;
        }

        /// <summary>
        /// Returns the initial value to be used when calculating the 'weight' of the characters in the
        /// reference number</summary>
        /// <returns>An integer - 0 (there is no initial value required for this reference type)</returns>
        public override int initialWeight()
        {
            return 55;
        }

        /// <summary>
        /// Converts an alpha character in the reference to a numeric - for reference types where alpha
        /// characters form part of the weighted value for a reference number
        /// </summary>
        /// <param name="AlphaChar">The character to be converted to a numeric</param>
        /// <param name="Multiplier">The weighted value to be applied to the alpha character</param>
        /// <returns>An integer representing the associated value for the character</returns>
        public override int ConvertedNumeric(string AlphaChar, int Multiplier)
        {
            return 0;
        }

        /// <summary>
        /// Checks whether a reference number is valid
        /// </summary>
        /// <param name="reference">The reference to be checked</param>
        /// <returns>Either the validated reference number or an empty string</returns>
        /// <remarks>any spaces will be removed from the reference prior to checking the check character</remarks>
        public string checkReference(string reference)
        {
            if (reference == "")
            {
                return "";
            }
            if (reference.Length != referenceLength())
            {
                return "";
            }
            Regex objEndCharacters = new Regex(regularExpression());
            if (objEndCharacters.IsMatch(reference) == false)
            {
                return "";
            }
            string testReference = reference.Replace(" ", "");
            if (testReference.Substring(ccPosition(), 2).ToUpper() != checkCharacter(weightedValue(testReference)))
            {
                return "";
            }
            return reference;
        }

        /// <summary>
        /// Calculate the value of the reference being checked. The weightable value is effectively converting
        /// each character into a numerical value then producing a total of those values
        /// </summary>
        /// <param name="reference">The reference to be checked</param>
        /// <returns>The total value of the characters in the reference</returns>
        private Int32 weightedValue(string reference)
        {
            Int32 result = 0;
            int[] weight = weightedValues();
            for (int counter = 0; counter <= weight.GetUpperBound(0); counter++)
            {
                if (weight[counter] != 0)
                {
                    if (checkNumeric(reference.Substring(counter, 1)) == true)
                    {
                        result = result + int.Parse(reference.Substring(counter, 1)) * weight[counter];
                    }
                    else
                    {
                        //result = result + (ConvertedNumeric(reference.Substring(counter, 1)) * weight[counter]);
                        result = result + ConvertedNumeric(reference.Substring(counter, 1), weight[counter]);
                        //* weight[counter]
                    }
                }
            }
            return result + initialWeight();
        }

        /// <summary>
        /// Calculates a check character based on the total value of the characters in the reference
        /// </summary>
        /// <param name="value">The total value of the characters in the reference</param>
        /// <returns>The check chracter which should apply to the reference</returns>
        private string checkCharacter(int value)
        {
            string result = (97 - (value % modValue())).ToString("00");
            return result;
        }

    }

    /// <summary>
    /// Checks the validity of an 11 character VAT reference
    /// </summary>
    public class VAT16ReferenceChecker : ReferenceChecker
    {

        //DMBTools.GeneralTools MyTools = new DMBTools.GeneralTools();

        /// <summary>
        /// Returns the expected structure for a 11 character VAT reference
        /// </summary>
        /// <returns>A string containing a regular expression with the expected VAT structure</returns>
        public override string regularExpression()
        {
            return @"[0-9]{3}[ ][0-9]{4}[ ][0-9]{2}[ ][0-9]{3,4}";
        }

        /// <summary>
        /// Returns weight that applies to each character in the COTAX reference
        /// </summary>
        /// <returns>An integer array</returns>
        public override int[] weightedValues()
        {
            //TODO Awaiting confirmation of whether VAT ref has a check character calculation
            return new int[] { 8, 7, 6, 5, 4, 3, 2, 0, 0 };
        }

        /// <summary>
        /// Returns the check characters that apply to a COTAX reference
        /// </summary>
        /// <returns>A string - the order of the check characters are based on the associated remainder
        /// following a modulus check</returns>
        public override string possibleCCs()
        {
            //TODO Awaiting confirmation of whether VAT ref has a check character calculation
            return "21987654321";
        }

        /// <summary>
        /// Returns the position of the check character within the reference
        /// </summary>
        /// <returns>An integer</returns>
        public override int ccPosition()
        {
            //TODO Awaiting confirmation of whether VAT ref has a check character calculation
            return 7;
        }

        /// <summary>
        /// Returns the expected length of a VAT reference number
        /// </summary>
        /// <returns>An integer</returns>
        public override int referenceLength()
        {
            return 16;
        }

        /// <summary>
        /// Returns the nodulus value that applies when calculating the check character
        /// </summary>
        /// <returns>An integer</returns>
        public override int modValue()
        {
            //TODO Awaiting confirmation of whether VAT ref has a check character calculation
            return 97;
        }

        /// <summary>
        /// Returns the initial value to be used when calculating the 'weight' of the characters in the
        /// reference number</summary>
        /// <returns>An integer - 0 (there is no initial value required for this reference type)</returns>
        public override int initialWeight()
        {
            return 0;
        }

        /// <summary>
        /// Converts an alpha character in the reference to a numeric - for reference types where alpha
        /// characters form part of the weighted value for a reference number
        /// </summary>
        /// <param name="AlphaChar">The character to be converted to a numeric</param>
        /// <param name="Multiplier">The weighted value to be applied to the alpha character</param>
        /// <returns>An integer representing the associated value for the character</returns>
        public override int ConvertedNumeric(string AlphaChar, int Multiplier)
        {
            return 0;
        }

        /// <summary>
        /// Checks whether a reference number is valid
        /// </summary>
        /// <param name="reference">The reference to be checked</param>
        /// <returns>Either the validated reference number or an empty string</returns>
        /// <remarks>any spaces will be removed from the reference prior to checking the check character</remarks>
        public string checkReference(string reference)
        {
            if (reference == "")
            {
                return "";
            }
            if (reference.Length != 15 && reference.Length != 16)
            {
                return "";
            }
            Regex objEndCharacters = new Regex(regularExpression());
            if (objEndCharacters.IsMatch(reference) == false)
            {
                return "";
            }
            string testReference = reference.Replace(" ", "");
            if (testReference.Substring(ccPosition(), 2).ToUpper() != checkCharacter(weightedValue(testReference)))
            {
                return "";
            }
            return reference;
        }

        /// <summary>
        /// Calculate the value of the reference being checked. The weightable value is effectively converting
        /// each character into a numerical value then producing a total of those values
        /// </summary>
        /// <param name="reference">The reference to be checked</param>
        /// <returns>The total value of the characters in the reference</returns>
        private Int32 weightedValue(string reference)
        {
            Int32 result = 0;
            int[] weight = weightedValues();
            for (int counter = 0; counter <= weight.GetUpperBound(0); counter++)
            {
                if (weight[counter] != 0)
                {
                    if (checkNumeric(reference.Substring(counter, 1)) == true)
                    {
                        result = result + int.Parse(reference.Substring(counter, 1)) * weight[counter];
                    }
                    else
                    {
                        //result = result + (ConvertedNumeric(reference.Substring(counter, 1)) * weight[counter]);
                        result = result + ConvertedNumeric(reference.Substring(counter, 1), weight[counter]);
                        //* weight[counter]
                    }
                }
            }
            return result + initialWeight();
        }

        /// <summary>
        /// Calculates a check character based on the total value of the characters in the reference
        /// </summary>
        /// <param name="value">The total value of the characters in the reference</param>
        /// <returns>The check chracter which should apply to the reference</returns>
        private string checkCharacter(int value)
        {
            string result = (97 - (value % modValue())).ToString("00");
            return result;
        }

    }

    /// <summary>
    /// Checks the validity of a 15-16 character VAT reference
    /// </summary>
    public class VAT1655ReferenceChecker : ReferenceChecker
    {

        //DMBTools.GeneralTools MyTools = new DMBTools.GeneralTools();

        /// <summary>
        /// Returns the expected structure for a 11 character VAT reference
        /// </summary>
        /// <returns>A string containing a regular expression with the expected VAT structure</returns>
        public override string regularExpression()
        {
            return @"[0-9]{3}[ ][0-9]{4}[ ][0-9]{2}[ ][0-9]{3,4}";
        }

        /// <summary>
        /// Returns weight that applies to each character in the COTAX reference
        /// </summary>
        /// <returns>An integer array</returns>
        public override int[] weightedValues()
        {
            //TODO Awaiting confirmation of whether VAT ref has a check character calculation
            return new int[] { 8, 7, 6, 5, 4, 3, 2, 0, 0 };
        }

        /// <summary>
        /// Returns the check characters that apply to a COTAX reference
        /// </summary>
        /// <returns>A string - the order of the check characters are based on the associated remainder
        /// following a modulus check</returns>
        public override string possibleCCs()
        {
            //TODO Awaiting confirmation of whether VAT ref has a check character calculation
            return "21987654321";
        }

        /// <summary>
        /// Returns the position of the check character within the reference
        /// </summary>
        /// <returns>An integer</returns>
        public override int ccPosition()
        {
            //TODO Awaiting confirmation of whether VAT ref has a check character calculation
            return 7;
        }

        /// <summary>
        /// Returns the expected length of a VAT reference number
        /// </summary>
        /// <returns>An integer</returns>
        public override int referenceLength()
        {
            return 16;
        }

        /// <summary>
        /// Returns the nodulus value that applies when calculating the check character
        /// </summary>
        /// <returns>An integer</returns>
        public override int modValue()
        {
            //TODO Awaiting confirmation of whether VAT ref has a check character calculation
            return 97;
        }

        /// <summary>
        /// Returns the initial value to be used when calculating the 'weight' of the characters in the
        /// reference number</summary>
        /// <returns>An integer - 0 (there is no initial value required for this reference type)</returns>
        public override int initialWeight()
        {
            return 0;
        }

        /// <summary>
        /// Converts an alpha character in the reference to a numeric - for reference types where alpha
        /// characters form part of the weighted value for a reference number
        /// </summary>
        /// <param name="AlphaChar">The character to be converted to a numeric</param>
        /// <param name="Multiplier">The weighted value to be applied to the alpha character</param>
        /// <returns>An integer representing the associated value for the character</returns>
        public override int ConvertedNumeric(string AlphaChar, int Multiplier)
        {
            return 0;
        }

        /// <summary>
        /// Checks whether a reference number is valid
        /// </summary>
        /// <param name="reference">The reference to be checked</param>
        /// <returns>Either the validated reference number or an empty string</returns>
        /// <remarks>any spaces will be removed from the reference prior to checking the check character</remarks>
        public string checkReference(string reference)
        {
            if (reference == "")
            {
                return "";
            }
            if (reference.Length != 15 && reference.Length != 16)
            {
                return "";
            }
            Regex objEndCharacters = new Regex(regularExpression());
            if (objEndCharacters.IsMatch(reference) == false)
            {
                return "";
            }
            string testReference = reference.Replace(" ", "");
            if (testReference.Substring(ccPosition(), 2).ToUpper() != checkCharacter(weightedValue(testReference)))
            {
                return "";
            }
            return reference;
        }

        /// <summary>
        /// Calculate the value of the reference being checked. The weightable value is effectively converting
        /// each character into a numerical value then producing a total of those values
        /// </summary>
        /// <param name="reference">The reference to be checked</param>
        /// <returns>The total value of the characters in the reference</returns>
        private Int32 weightedValue(string reference)
        {
            Int32 result = 0;
            int[] weight = weightedValues();
            for (int counter = 0; counter <= weight.GetUpperBound(0); counter++)
            {
                if (weight[counter] != 0)
                {
                    if (checkNumeric(reference.Substring(counter, 1)) == true)
                    {
                        result = result + int.Parse(reference.Substring(counter, 1)) * weight[counter];
                    }
                    else
                    {
                        //result = result + (ConvertedNumeric(reference.Substring(counter, 1)) * weight[counter]);
                        result = result + ConvertedNumeric(reference.Substring(counter, 1), weight[counter]);
                        //* weight[counter]
                    }
                }
            }
            return result + initialWeight();
        }

        /// <summary>
        /// Calculates a check character based on the total value of the characters in the reference
        /// </summary>
        /// <param name="value">The total value of the characters in the reference</param>
        /// <returns>The check chracter which should apply to the reference</returns>
        private string checkCharacter(int value)
        {
            string result = (97 - (value % modValue())).ToString("00");
            return result;
        }

    }

    /// <summary>
    /// Checks the validity of an IHT reference
    /// </summary>
    public class IHTReferenceChecker : ReferenceChecker
    {

        /// <summary>
        /// Returns the expected structure for a COTAX reference
        /// </summary>
        /// <returns>A string containing a regular expression with the expected COTAX structure</returns>
        public override string regularExpression()
        {
            return @"(A|F|L|N|EN|ET|SS|ST)\d{6}\/\d{2}[A-Z]";
        }

        /// <summary>
        /// Checks that the reference matches the pattern for inheritance tax (no check character checks)
        /// </summary>
        /// <param name="reference">The reference to be checked</param>
        /// <returns></returns>
        public string checkReference(string reference)
        {
            if(reference.Length >12)
            {
                return "";
            }
            Regex objEndCharacters = new Regex(regularExpression());
            if (objEndCharacters.IsMatch(reference) == false)
            {
                return "";
            }
            else
            {
                return reference;
            }
        }
          
        /// <summary>
        /// Returns weight that applies to each character in the COTAX reference
        /// </summary>
        /// <returns>An integer array</returns>
        public override int[] weightedValues()
        {
            return new int[] { 0};
        }

        /// <summary>
        /// Returns the check characters that apply to a COTAX reference
        /// </summary>
        /// <returns>A string - the order of the check characters are based on the associated remainder
        /// following a modulus check</returns>
        public override string possibleCCs()
        {
            return "0";
        }

        /// <summary>
        /// Returns the position of the check character within the reference
        /// </summary>
        /// <returns>An integer</returns>
        public override int ccPosition()
        {
            return 0;
        }

        /// <summary>
        /// Returns the expected length of a COTAX reference number
        /// </summary>
        /// <returns>An integer</returns>
        public override int referenceLength()
        {
            return 0;
        }

        /// <summary>
        /// Returns the nodulus value that applies when calculating the check character
        /// </summary>
        /// <returns>An integer</returns>
        public override int modValue()
        {
            return 0;
        }

        /// <summary>
        /// Returns the initial value to be used when calculating the 'weight' of the characters in the
        /// reference number</summary>
        /// <returns>An integer - 0 (there is no initial value required for this reference type)</returns>
        public override int initialWeight()
        {
            return 0;
        }

        /// <summary>
        /// Converts an alpha character in the reference to a numeric - for reference types where alpha
        /// characters form part of the weighted value for a reference number
        /// </summary>
        /// <param name="AlphaChar">The character to be converted to a numeric</param>
        /// <param name="Multiplier">The weighted value to be applied to the alpha character</param>
        /// <returns>An integer representing the associated value for the character</returns>
        public override int ConvertedNumeric(string AlphaChar, int Multiplier)
        {
            return 0;
        }

    }

    /// <summary>
    /// Checks the validity of a VAT reference
    /// </summary>
    public class UKVATReferenceChecker : ReferenceChecker
    {

        /// <summary>
        /// Returns the expected structure for a COTAX reference
        /// </summary>
        /// <returns>A string containing a regular expression with the expected COTAX structure</returns>
        public override string regularExpression()
        {
            return @"^(GBGD|GBHA)\d{3}|^GB\d{3}[ ]\d{4}[ ]\d{2}([ ]\d{3})?";
        }

        /// <summary>
        /// Checks that the reference matches the pattern for inheritance tax (no check character checks)
        /// </summary>
        /// <param name="reference">The reference to be checked</param>
        /// <returns></returns>
        public string checkReference(string reference)
        {
            string regValue;
            switch(reference.Length)
            {
                case 7:
                    regValue = @"(GBGD|GBHA)\d{3}";
                    break;
                case 13:
                    regValue = @"GB\d{3}[ ]\d{4}[ ]\d{2}";
                    break;
                case 17:
                    regValue = @"GB\d{3}[ ]\d{4}[ ]\d{2}[ ]\d{3}";
                    break;
                default:
                    return "";
            }
            Regex objEndCharacters = new Regex(regValue);
            if (objEndCharacters.IsMatch(reference) == false)
            {
                return "";
            }
            else
            {
                return reference;
            }
        }

        /// <summary>
        /// Returns weight that applies to each character in the COTAX reference
        /// </summary>
        /// <returns>An integer array</returns>
        public override int[] weightedValues()
        {
            return new int[] { 0 };
        }

        /// <summary>
        /// Returns the check characters that apply to a COTAX reference
        /// </summary>
        /// <returns>A string - the order of the check characters are based on the associated remainder
        /// following a modulus check</returns>
        public override string possibleCCs()
        {
            return "0";
        }

        /// <summary>
        /// Returns the position of the check character within the reference
        /// </summary>
        /// <returns>An integer</returns>
        public override int ccPosition()
        {
            return 0;
        }

        /// <summary>
        /// Returns the expected length of a COTAX reference number
        /// </summary>
        /// <returns>An integer</returns>
        public override int referenceLength()
        {
            return 0;
        }

        /// <summary>
        /// Returns the nodulus value that applies when calculating the check character
        /// </summary>
        /// <returns>An integer</returns>
        public override int modValue()
        {
            return 0;
        }

        /// <summary>
        /// Returns the initial value to be used when calculating the 'weight' of the characters in the
        /// reference number</summary>
        /// <returns>An integer - 0 (there is no initial value required for this reference type)</returns>
        public override int initialWeight()
        {
            return 0;
        }

        /// <summary>
        /// Converts an alpha character in the reference to a numeric - for reference types where alpha
        /// characters form part of the weighted value for a reference number
        /// </summary>
        /// <param name="AlphaChar">The character to be converted to a numeric</param>
        /// <param name="Multiplier">The weighted value to be applied to the alpha character</param>
        /// <returns>An integer representing the associated value for the character</returns>
        public override int ConvertedNumeric(string AlphaChar, int Multiplier)
        {
            return 0;
        }

    }

    /// <summary>
    /// Checks the validity of a VAT reference
    /// </summary>
    public class ExciseIdReferenceChecker : ReferenceChecker
    {

        /// <summary>
        /// Returns the expected structure for a COTAX reference
        /// </summary>
        /// <returns>A string containing a regular expression with the expected COTAX structure</returns>
        public override string regularExpression()
        {
            return @"GBTC\d{9}";
        }

        /// <summary>
        /// Checks that the reference matches the pattern for inheritance tax (no check character checks)
        /// </summary>
        /// <param name="reference">The reference to be checked</param>
        /// <returns></returns>
        public string checkReference(string reference)
        {
            if (reference.Length != referenceLength())
            {
                return "";
            }
            Regex objEndCharacters = new Regex(regularExpression());
            if (objEndCharacters.IsMatch(reference) == false)
            {
                return "";
            }
            else
            {
                return reference;
            }
        }

        /// <summary>
        /// Returns weight that applies to each character in the COTAX reference
        /// </summary>
        /// <returns>An integer array</returns>
        public override int[] weightedValues()
        {
            return new int[] { 0 };
        }

        /// <summary>
        /// Returns the check characters that apply to a COTAX reference
        /// </summary>
        /// <returns>A string - the order of the check characters are based on the associated remainder
        /// following a modulus check</returns>
        public override string possibleCCs()
        {
            return "0";
        }

        /// <summary>
        /// Returns the position of the check character within the reference
        /// </summary>
        /// <returns>An integer</returns>
        public override int ccPosition()
        {
            return 0;
        }

        /// <summary>
        /// Returns the expected length of a COTAX reference number
        /// </summary>
        /// <returns>An integer</returns>
        public override int referenceLength()
        {
            return 13;
        }

        /// <summary>
        /// Returns the nodulus value that applies when calculating the check character
        /// </summary>
        /// <returns>An integer</returns>
        public override int modValue()
        {
            return 0;
        }

        /// <summary>
        /// Returns the initial value to be used when calculating the 'weight' of the characters in the
        /// reference number</summary>
        /// <returns>An integer - 0 (there is no initial value required for this reference type)</returns>
        public override int initialWeight()
        {
            return 0;
        }

        /// <summary>
        /// Converts an alpha character in the reference to a numeric - for reference types where alpha
        /// characters form part of the weighted value for a reference number
        /// </summary>
        /// <param name="AlphaChar">The character to be converted to a numeric</param>
        /// <param name="Multiplier">The weighted value to be applied to the alpha character</param>
        /// <returns>An integer representing the associated value for the character</returns>
        public override int ConvertedNumeric(string AlphaChar, int Multiplier)
        {
            return 0;
        }
    }
        ///NINO check added on 13/04/17
        /// Checks the validity of a NINO reference
        /// </summary>
        public class NINOReferenceChecker : ReferenceChecker
        {

            /// <summary>
            /// Returns the expected structure for a NINO reference
            /// </summary>
            /// <returns>A string containing a regular expression with the expected NINO structure</returns>
            public override string regularExpression()
            {
                return @"[a-zA-Z]{2}\d{6}[a-dA-D]";
            }
      
            /// <summary>
            /// Checks that the reference matches the pattern for NINO (no check character checks)
            /// </summary>
            /// <param name="reference">The reference to be checked</param>
            /// <returns></returns>
            public string checkReference(string reference)
            {
                if (reference.Length != referenceLength())
                {
                    return "";
                }
                Regex objEndCharacters = new Regex(regularExpression());
                if (objEndCharacters.IsMatch(reference) == false)
                {
                    return "";
                }
                else
                {
                    return reference;
                }
            }

            /// <summary>
            /// Returns weight that applies to each character in the NINO reference
            /// </summary>
            /// <returns>An integer array</returns>
            public override int[] weightedValues()
            {
                return new int[] { 0 };
            }

            /// <summary>
            /// Returns the check characters that apply to a NINO reference
            /// </summary>
            /// <returns>A string - the order of the check characters are based on the associated remainder
            /// following a modulus check</returns>
            public override string possibleCCs()
            {
                return "0";
            }

            /// <summary>
            /// Returns the position of the check character within the reference
            /// </summary>
            /// <returns>An integer</returns>
            public override int ccPosition()
            {
                return 0;
            }

            /// <summary>
            /// Returns the expected length of a NINO reference number
            /// </summary>
            /// <returns>An integer</returns>
            public override int referenceLength()
            {
                return 9;
            }

            /// <summary>
            /// Returns the nodulus value that applies when calculating the check character
            /// </summary>
            /// <returns>An integer</returns>
            public override int modValue()
            {
                return 0;
            }

            /// <summary>
            /// Returns the initial value to be used when calculating the 'weight' of the characters in the
            /// reference number</summary>
            /// <returns>An integer - 0 (there is no initial value required for this reference type)</returns>
            public override int initialWeight()
            {
                return 0;
            }

            /// <summary>
            /// Converts an alpha character in the reference to a numeric - for reference types where alpha
            /// characters form part of the weighted value for a reference number
            /// </summary>
            /// <param name="AlphaChar">The character to be converted to a numeric</param>
            /// <param name="Multiplier">The weighted value to be applied to the alpha character</param>
            /// <returns>An integer representing the associated value for the character</returns>
            public override int ConvertedNumeric(string AlphaChar, int Multiplier)
            {
                return 0;
            }





        }
    }

