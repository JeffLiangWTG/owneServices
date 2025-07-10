using System.Collections.Immutable;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Common.AU.CMR
{
	public class CANValidation
	{
		public ZString GetInvalidReason(ZString cAN)
		{
			cAN = cAN.ToUpper();
			ZString result = ZString.Empty;
			if (cAN.Length != 9)
			{
				result = Res.GetString("5101445C-F20C-45A3-80C5-F79EF96523B1", "CAN must be 9 characters.");
			}
			else
			{
				ZString dataPart = cAN.Substring(0, 8);
				ZString checkDigitPart = cAN.Substring(8, 1);
				ZString invalidCharacters = GetInvalidCharacters(dataPart);
				if (!(invalidCharacters.Length == 0))
				{
					result = Res.GetString("04CC3000-555B-4DF2-8ABD-D3F8774029CF", "CAN contains the invalid character(s): '{0}'.", invalidCharacters);
				}
				else
				{
					ZString expectedCheckDigit = GetExpectedCheckDigit(dataPart);
					if (checkDigitPart != expectedCheckDigit)
					{
						result = Res.GetString("D7D609F3-81C3-49F6-B756-5A133704268B", "CAN has an invalid check digit.");
#if DEBUG
						result = Res.GetString("EBDBCD54-6C5A-4D4A-BCCD-F47F08D291BA", "  Expected digit: '{0}'.", expectedCheckDigit);
#endif
					}
				}
			}
			return result;
		}

		public void ValidateCANField(ZPropertyInfo cANInfo)
		{
			if (cANInfo.Value is ZString)
			{
				ZString invalidReason = GetInvalidReason((ZString)cANInfo.Value);
				if (!invalidReason.IsEmpty)
				{
					cANInfo.AddMessageError(invalidReason);
				}
			}
		}

		#region Implementation

		protected ZString GetInvalidCharacters(ZString data)
		{
			var invalidCharacters = data.ExcludeChars(ValidDataCharacters);
			if (data.Contains(" "))
			{
				invalidCharacters += " ";
			}

			return invalidCharacters;
		}
		protected ZString GetExpectedCheckDigit(ZString data)
		{
			int sumOfWeightedCharacters = 0;
			for (int i = 0; i < 8; i++)
			{
				sumOfWeightedCharacters += CharacterWeights[ValidDataCharacters.IndexOf(data[i])] * PositionWeights[i];
			}
			int checkDigitWeight = 22 - sumOfWeightedCharacters % 23;
			return CalidCharacersByWeight.Substring(checkDigitWeight, 1);
		}

		protected const string ValidDataCharacters = "ACEFGHJKLMNPRTWXY34679";
		protected const string CalidCharacersByWeight = "SMGXE6KR9C3JWPATF7YL4NH";

		protected static readonly ImmutableArray<int> CharacterWeights = new int[22] { 14, 9, 4, 16, 2, 22, 11, 6, 19, 1, 21, 13, 7, 15, 12, 3, 18, 10, 20, 5, 17, 8 }.ToImmutableArray();
		protected static readonly ImmutableArray<int> PositionWeights = new int[8] { 3, 2, 6, 8, 5, 9, 4, 7 }.ToImmutableArray();

		#endregion
	}
}
