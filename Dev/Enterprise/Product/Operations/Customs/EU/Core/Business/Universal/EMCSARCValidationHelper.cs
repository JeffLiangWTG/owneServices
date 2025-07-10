using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.EU.Business
{
	public static class EMCSARCValidationHelper
	{
		public static ZString ValidateACRNumberIsValid(ZString arc, BusinessObjectFactory factory)
		{
			var regex = new Regex("^[0-9]{2}(?<CountryCode>[A-Z]{2})(?<Reference>[A-Z0-9]{16})(?<CheckDigit>[0-9])$");
			var match = regex.Match(arc);
			var errors = new ZStringBuilder();
			if (!match.Success)
			{
				errors.AppendIfNotEmpty(errorFormatMessageError);
			}
			else
			{
				var countryCode = match.Groups["CountryCode"].Value;
				if (countryCode.Equals(Core.Constants.CountryCodes.Germany))
				{
					var reference = match.Groups["Reference"].Value;
					var regex1 = new Regex("^[0-9]{16}$");
					var regex2 = new Regex("^[0-9]{15}(P|S)$");

					var numberFromFirstTwoLetters = int.Parse(arc.Substring(0, 2));
					var isValidReference = (numberFromFirstTwoLetters <= 23 && regex1.IsMatch(reference)) || (numberFromFirstTwoLetters >= 22 && regex2.IsMatch(reference));
					if (!isValidReference)
					{
						errors.AppendIfNotEmpty(errorFormatMessageError);
					}
				}
				else
				{
					errors.AppendIfNotEmpty(CheckCountryCode(countryCode, factory));
				}
				errors.AppendIfNotEmpty(CheckARCCheckDigit(arc, match.Groups["CheckDigit"].Value));
			}

			return errors.ToStringWithNewLineBetweenAppends();
		}

		static ZString CheckCountryCode(ZString countryCode, BusinessObjectFactory factory)
		{
			var result = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(factory, countryCode, Env.CurrentCompany.Country.Code, UniversalReferenceConstants.RefCusCodeListType.Code.Code_NC010, ZDate.Today);
			var isValidCountryCode = result != null;

			if (!isValidCountryCode)
			{
				return ResString.GetMultilingualString("38C62856-13AB-48F0-8C5B-1526C7212941", "Please enter a valid country/region code.");
			}
			return ZString.Empty;
		}

		static ZString CheckARCCheckDigit(ZString arc, ZString expectedCheckDigit)
		{
			var calculatedResult = CalculatedTotalARCValue(arc);

			calculatedResult %= 11;
			if (calculatedResult == 10)
			{
				calculatedResult = 0;
			}

			if (expectedCheckDigit != calculatedResult.ToString(CultureInfo.InvariantCulture))
			{
				return ResString.GetMultilingualString("672DC88A-C75E-41F5-9C9D-4E0F37392C53", "ARC does not have a valid check (last) digit. The check digit should be {0}", calculatedResult);
			}

			return ZString.Empty;
		}

		static int CalculatedTotalARCValue(ZString arc)
		{
			var totalCalculatedResult = 0;
			var position = 0d;
			var acrWithoutCheckDigit = arc.SubstringSafe(0, 20);

			foreach (var c in acrWithoutCheckDigit)
			{
				var charValue = Dictionary[c];
				var factor = Math.Pow(2.0, position);

				totalCalculatedResult += (charValue * Convert.ToInt32(factor));
				position++;
			}

			return totalCalculatedResult;
		}

		#region Dictionary

		static IReadOnlyDictionary<char, int> Dictionary => dictionary ?? (dictionary = new Dictionary<char, int>()
		{
			{ '0', 0 },
			{ '1', 1 },
			{ '2', 2 },
			{ '3', 3 },
			{ '4', 4 },
			{ '5', 5 },
			{ '6', 6 },
			{ '7', 7 },
			{ '8', 8 },
			{ '9', 9 },
			{ 'A', 10 },
			{ 'B', 12 },
			{ 'C', 13 },
			{ 'D', 14 },
			{ 'E', 15 },
			{ 'F', 16 },
			{ 'G', 17 },
			{ 'H', 18 },
			{ 'I', 19 },
			{ 'J', 20 },
			{ 'K', 21 },
			{ 'L', 23 },
			{ 'M', 24 },
			{ 'N', 25 },
			{ 'O', 26 },
			{ 'P', 27 },
			{ 'Q', 28 },
			{ 'R', 29 },
			{ 'S', 30 },
			{ 'T', 31 },
			{ 'U', 32 },
			{ 'V', 34 },
			{ 'W', 35 },
			{ 'X', 36 },
			{ 'Y', 37 },
			{ 'Z', 38 }
		});

		[ThreadStatic]
		static IReadOnlyDictionary<char, int> dictionary;

		#endregion

		[ThreadSafe]
		static readonly ResourceString errorFormatMessageError = ResString.GetMultilingualString("42BFC912-9461-4C2B-8DFB-76D411A83580", @"Structure does not correspond to an EMCS Administrative Reference Code (ARC). Please enter the ARC in the following format with only numbers and uppercase letters:
• two numbers for the year of issue,
• two letters for the ISO country/region code for country/region of issue,
• sixteen alphanumeric characters for unique identification and
• one number check digit");
	}
}
