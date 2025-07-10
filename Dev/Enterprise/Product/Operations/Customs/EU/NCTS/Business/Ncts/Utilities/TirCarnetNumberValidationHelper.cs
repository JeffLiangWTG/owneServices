using System;
using System.Text.RegularExpressions;

namespace Enterprise.Customs.EU.NCTS.Business
{
	sealed class TirCarnetNumberValidationHelper
	{
		static readonly long LastTirCarnetNumberWithoutControlChars = 25_000_000L;

		static readonly Regex TirCarnetNumberPattern = new Regex("^([A-Z]{2})?(\\d+)$", RegexOptions.Compiled);

		public static bool NumberIsValid(string tirCarnetNumber)
		{
			var result = false;
			if (!string.IsNullOrEmpty(tirCarnetNumber))
			{
				var tirCarnetNumberInUpperCase = tirCarnetNumber.ToUpper();
				var match = TirCarnetNumberPattern.Match(tirCarnetNumberInUpperCase);
				if (match.Success)
				{
					if (long.TryParse(match.Groups[2].Value, out var number))
					{
						var checkCode = match.Groups[1].Value;
						var checkCodeIsNeeded = number > LastTirCarnetNumberWithoutControlChars;

						result = (checkCodeIsNeeded && !string.IsNullOrEmpty(checkCode) && checkCode == GetCheckCode(number))
							|| (!checkCodeIsNeeded && string.IsNullOrEmpty(checkCode));
					}
				}
			}
			return result;
		}

		static string GetCheckCode(long tirCarnetNumber)
		{
			var result = string.Empty;
			if (tirCarnetNumber > LastTirCarnetNumberWithoutControlChars)
			{
				long carnetModulo23 = tirCarnetNumber % 23;
				var checkValue = 'A' + (int)((3 * carnetModulo23 + 17) % 26);
				var checkCharacter = Convert.ToChar(checkValue);

				result = carnetModulo23 < 12 ? $"{checkCharacter}X" : $"X{checkCharacter}";
			}
			return result;
		}
	}
}
