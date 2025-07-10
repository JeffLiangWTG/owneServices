using System;
using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CANValidation : ValidationProvider
	{
		public void ValidateCAN(ZPropertyInfo property)
		{
			string messageError = "";
			ZString cANNumber = property.Value.ToString();
			if (!cANNumber.IsEmpty)
			{
				if (!LengthValid(cANNumber))
				{
					messageError = "Entry Number is not the correct length - should be 9 characters.";
				}
				else
				{
					string invalidCharacters = GetInvalidCharacters(cANNumber);
					if (invalidCharacters.Length != 0)
					{
						messageError = String.Format("Entry Number contains invalid characters : '{0}'", invalidCharacters);
					}
					else
					{
						char expectedCheckDigit = CalculateCheckDigit(cANNumber);
						if (expectedCheckDigit != cANNumber[checkDigitIndex])
						{
							messageError = "Check Digit incorrect.  Should be '" + expectedCheckDigit + "'";
						}
					}
				}
			}

			if (!string.IsNullOrEmpty(messageError))
			{
				property.AddMessageError(messageError);
			}
		}

		#region Implementation

		readonly int checkDigitIndex = 8;

		bool LengthValid(string aString)
		{
			return aString.Length == 9;
		}

		string GetInvalidCharacters(string aString)
		{
			string result = "";
			for (int i = 0; i < aString.Length; i++)
			{
				if (characterWeights[aString[i]] == null)
				{
					result += aString[i];
				}
			}
			return result;
		}

		readonly Hashtable characterWeights = CreateCharacterWeightHashtable();

		readonly Hashtable positionWeight = CreatePositionWeightHashtable();

		char CalculateCheckDigit(string cANNumber)
		{
			char result = '~';
			int sum = 0;
			for (int i = 0; i < cANNumber.Length - 1; i++)
			{
				int wv = (int)characterWeights[cANNumber[i]];
				int pi = (int)positionWeight[i + 1];
				sum += (wv * pi);
			}
			int weightCheckDigit = 22 - sum % 23;
			if (weightCheckDigit < 0 && weightCheckDigit > 22)
			{
				throw new OdysseyException("Failed to calculate check digit");
			}
			foreach (DictionaryEntry anEntry in characterWeights)
			{
				if ((int)anEntry.Value == weightCheckDigit)
				{
					result = (char)anEntry.Key;
					break;
				}
			}
			return result;
		}

		static Hashtable CreateCharacterWeightHashtable()
		{
			Hashtable result = new Hashtable();
			result['A'] = 14;
			result['C'] = 9;
			result['E'] = 4;
			result['F'] = 16;
			result['G'] = 2;
			result['H'] = 22;
			result['J'] = 11;
			result['K'] = 6;
			result['L'] = 19;
			result['M'] = 1;
			result['N'] = 21;
			result['P'] = 13;
			result['R'] = 7;
			result['T'] = 15;
			result['W'] = 12;
			result['X'] = 3;
			result['Y'] = 18;
			result['3'] = 10;
			result['4'] = 20;
			result['6'] = 5;
			result['7'] = 17;
			result['9'] = 8;
			result['S'] = 0;
			return result;
		}

		static Hashtable CreatePositionWeightHashtable()
		{
			Hashtable result = new Hashtable();
			result[1] = 3;
			result[2] = 2;
			result[3] = 6;
			result[4] = 8;
			result[5] = 5;
			result[6] = 9;
			result[7] = 4;
			result[8] = 7;
			result[9] = 1;
			return result;
		}

		#endregion
	}
}
