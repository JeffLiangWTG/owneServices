using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.Common
{
	internal enum MrnTypes
	{
		Unknown,
		Transit,
		Import,
		Export
	}
	static class MovementReferenceNumberValidator
	{
		public static string ApplyAdditionalValidationOnMrn(string customsEntryNumbertoValidate, MrnTypes mrnTypes)
		{
			MovementReferenceNumberValidatorTools tools = new MovementReferenceNumberValidatorTools();
			string countryMRN = tools.GetCountryMrn(customsEntryNumbertoValidate);
			string warningMrnTransit = Res.GetString("5051F271-DBD6-4F3F-AE48-C7DD371ABD84", "This is not a valid Transit MRN for country ");
			string warningMrnExport = Res.GetString("E4DA3BEC-3495-4482-BD80-6D955390B94E", "This is not a valid Export MRN for country ");
			string warningMrnImport = Res.GetString("661F86D7-F6F2-4009-B3CB-7505E12A261D", "This is not a valid Import MRN for country ");
			string warningBadCountry = Res.GetString("C5C0B965 -E1E3-495A-9819-D12F7D2352E2", "Country of issue is not recognized, cannot validate this MRN");
			string warningUnknow = Res.GetString("08D767F7-D0B6-4794-B873-023E4DD3E860", "This is not a valid MRN for ");
			if (!tools.CountryIsValid(countryMRN))
			{
				return warningBadCountry;
			}

			if (mrnTypes.Equals(MrnTypes.Transit))
			{
				if (!ProcessTransit(countryMRN, customsEntryNumbertoValidate, tools))
				{
					return warningMrnTransit + countryMRN;
				}
				return ZString.Empty;
			}
			else if (mrnTypes.Equals(MrnTypes.Export))
			{
				if (!ProcessExport(countryMRN, customsEntryNumbertoValidate, tools))
				{
					return warningMrnExport + countryMRN;
				}
				return ZString.Empty;
			}
			else if (mrnTypes.Equals(MrnTypes.Import))
			{
				if (!ProcessImport(countryMRN, customsEntryNumbertoValidate, tools))
				{
					return warningMrnImport + countryMRN;
				}
				return ZString.Empty;
			}
			else if (mrnTypes.Equals(MrnTypes.Unknown))
			{
				if (ProcessImport(countryMRN, customsEntryNumbertoValidate, tools) || ProcessExport(countryMRN, customsEntryNumbertoValidate, tools) || ProcessTransit(countryMRN, customsEntryNumbertoValidate, tools))
				{
					return ZString.Empty;
				}
				else
				{
					return warningUnknow + countryMRN;
				}
			}
			return ZString.Empty;
		}

		static bool ProcessTransit(string countryMRN, string customsEntryNumbertoValidate, MovementReferenceNumberValidatorTools tools)
		{
			var result = false;

			if (tools.countryTransitWith14DigitAtEnd.Contains(countryMRN))
			{
				result = tools.Validate14Digit(customsEntryNumbertoValidate);
			}
			else if (countryMRN.Equals(Core.Constants.CountryCodes.Austria))
			{
				result = tools.ValidateSymbolAtANumber(customsEntryNumbertoValidate, "TN", 11) || tools.ValidateSymbolAtANumber(customsEntryNumbertoValidate, "TV", 11)
						|| tools.ValidateSymbolAtANumber(customsEntryNumbertoValidate, "CT", 11);
			}
			else if (tools.countryTransitWith1atpos11.Contains(countryMRN))
			{
				result = tools.ValidateSymbolAtANumber(customsEntryNumbertoValidate, "1", 11);
			}
			else if (countryMRN.Equals(Core.Constants.CountryCodes.CzechRepublic))
			{
				result = tools.ValidateSymbolAtANumber(customsEntryNumbertoValidate, "9", 11);
			}
			else if (countryMRN.Equals(Core.Constants.CountryCodes.Germany))
			{
				var valid17thChars = new[] { "J", "K", "L", "M", "T" };
				result = valid17thChars.Any(c => tools.ValidateSymbolAtANumber(customsEntryNumbertoValidate, c, 17));
			}
			else if (countryMRN.Equals(Core.Constants.CountryCodes.Estonia) || countryMRN.Equals(Core.Constants.CountryCodes.Poland))
			{
				result = tools.ValidateSymbolAtANumber(customsEntryNumbertoValidate, "N", 11) || tools.ValidateSymbolAtANumber(customsEntryNumbertoValidate, "T", 11);
			}
			else if (countryMRN.Equals(Core.Constants.CountryCodes.Greece))
			{
				result = tools.ValidateSymbolAtANumber(customsEntryNumbertoValidate, Core.Constants.CountryCodes.Turkey, 5) || tools.ValidateSymbolAtANumber(customsEntryNumbertoValidate, "RT", 5);
			}
			else if (countryMRN.Equals(Core.Constants.CountryCodes.Spain))
			{
				result = tools.ValidateSymbolAtANumber(customsEntryNumbertoValidate, "5", 11);
			}
			else if (countryMRN.Equals(Core.Constants.CountryCodes.Finland) || countryMRN.Equals(Core.Constants.CountryCodes.Italy) || countryMRN.Equals(Core.Constants.CountryCodes.SanMarino))
			{
				result = tools.ValidateSymbolAtANumber(customsEntryNumbertoValidate, "T", 17);
			}
			else if (countryMRN.Equals(Core.Constants.CountryCodes.Slovakia))
			{
				result = tools.ValidateSymbolAtANumber(customsEntryNumbertoValidate, Core.Constants.CountryCodes.Turkey, 9);
			}
			else if (countryMRN.Equals(Core.Constants.CountryCodes.Turkey))
			{
				result = true;
			}

			return result;
		}
		static bool ProcessExport(string countryMRN, string customsEntryNumbertoValidate, MovementReferenceNumberValidatorTools tools)
		{
			return tools.ExportCountryFirstPart(countryMRN, customsEntryNumbertoValidate) || tools.ExportCountrySecondPart(countryMRN, customsEntryNumbertoValidate);
		}

		static bool ProcessImport(string countryMRN, string customsEntryNumbertoValidate, MovementReferenceNumberValidatorTools tools)
		{
			return tools.ImportCountryFirstPart(countryMRN, customsEntryNumbertoValidate) || tools.ImportCountrySecondPart(countryMRN, customsEntryNumbertoValidate);
		}
	}

	#region process
	internal class MovementReferenceNumberValidatorTools
	{
		readonly string[] countriesAuthorizedToMRN = new string[] {
			Core.Constants.CountryCodes.Andorra,
			Core.Constants.CountryCodes.Austria,
			Core.Constants.CountryCodes.Belgium,
			Core.Constants.CountryCodes.Bulgaria,
			Core.Constants.CountryCodes.Switzerland,
			Core.Constants.CountryCodes.Cyprus,
			Core.Constants.CountryCodes.CzechRepublic,
			Core.Constants.CountryCodes.Germany,
			Core.Constants.CountryCodes.Denmark,
			Core.Constants.CountryCodes.Estonia,
			Core.Constants.CountryCodes.Greece,
			Core.Constants.CountryCodes.Spain,
			Core.Constants.CountryCodes.Finland,
			Core.Constants.CountryCodes.France,
			Core.Constants.CountryCodes.Croatia,
			Core.Constants.CountryCodes.Hungary,
			Core.Constants.CountryCodes.Ireland,
			Core.Constants.CountryCodes.Italy,
			Core.Constants.CountryCodes.Lithuania,
			Core.Constants.CountryCodes.Luxembourg,
			Core.Constants.CountryCodes.Latvia,
			Core.Constants.CountryCodes.Malta,
			Core.Constants.CountryCodes.Netherlands,
			Core.Constants.CountryCodes.Norway,
			Core.Constants.CountryCodes.Poland,
			Core.Constants.CountryCodes.Portugal,
			Core.Constants.CountryCodes.Romania,
			Core.Constants.CountryCodes.Sweden,
			Core.Constants.CountryCodes.Slovenia,
			Core.Constants.CountryCodes.Slovakia,
			Core.Constants.CountryCodes.SanMarino,
			Core.Constants.CountryCodes.UnitedKingdom
		};

		internal string[] countryTransitWith1atpos11 = new string[] {
			Core.Constants.CountryCodes.Cyprus,
			Core.Constants.CountryCodes.Denmark,
			Core.Constants.CountryCodes.Hungary,
			Core.Constants.CountryCodes.Ireland,
			Core.Constants.CountryCodes.Lithuania,
			Core.Constants.CountryCodes.Luxembourg,
			Core.Constants.CountryCodes.Latvia,
			Core.Constants.CountryCodes.Malta,
			Core.Constants.CountryCodes.Netherlands,
			Core.Constants.CountryCodes.Norway,
			Core.Constants.CountryCodes.Portugal,
			Core.Constants.CountryCodes.Sweden,
			Core.Constants.CountryCodes.Slovenia,
			Core.Constants.CountryCodes.UnitedKingdom
		};

		internal string[] countryTransitWith14DigitAtEnd = new string[] {
			Core.Constants.CountryCodes.Andorra,
			Core.Constants.CountryCodes.Belgium,
			Core.Constants.CountryCodes.Bulgaria,
			Core.Constants.CountryCodes.Switzerland,
			Core.Constants.CountryCodes.France,
			Core.Constants.CountryCodes.Romania
		};

		internal string[] countryExportWith2atpos11 = new string[] {
			Core.Constants.CountryCodes.Cyprus,
			Core.Constants.CountryCodes.CzechRepublic,
			Core.Constants.CountryCodes.Denmark,
			Core.Constants.CountryCodes.Hungary,
			Core.Constants.CountryCodes.Luxembourg,
			Core.Constants.CountryCodes.Latvia,
			Core.Constants.CountryCodes.Malta,
			Core.Constants.CountryCodes.Netherlands,
			Core.Constants.CountryCodes.Portugal,
			Core.Constants.CountryCodes.Slovenia
		};

		internal string GetCountryMrn(ZString customsEntryNumbertoValidate) => customsEntryNumbertoValidate.SubstringSafe(2, 2);

		internal bool ImportCountryFirstPart(string countryMRN, string customsEntryNumbertoValidate)
		{
			var result = false;

			if (countryMRN.Equals(Core.Constants.CountryCodes.Austria))
			{
				result = ValidateSymbolAtANumber(customsEntryNumbertoValidate, "IS", 11);
			}
			else if (countryMRN.Equals(Core.Constants.CountryCodes.Belgium))
			{
				result = ValidateSymbolAtANumber(customsEntryNumbertoValidate, "I", 5) || ValidateSymbolAtANumber(customsEntryNumbertoValidate, "N", 5);
			}
			else if (countryMRN.Equals(Core.Constants.CountryCodes.Bulgaria))
			{
				result = ValidateSymbolAtANumber(customsEntryNumbertoValidate, "I", 5);
			}
			else if (countryMRN.Equals(Core.Constants.CountryCodes.Cyprus) || countryMRN.Equals(Core.Constants.CountryCodes.Denmark) || countryMRN.Equals(Core.Constants.CountryCodes.Latvia) || countryMRN.Equals(Core.Constants.CountryCodes.Slovenia))
			{
				result = ValidateSymbolAtANumber(customsEntryNumbertoValidate, "3", 11);
			}
			else if (countryMRN.Equals(Core.Constants.CountryCodes.CzechRepublic))
			{
				result = ValidateSymbolAtANumber(customsEntryNumbertoValidate, "1", 11);
			}
			else if (countryMRN.Equals(Core.Constants.CountryCodes.Germany))
			{
				var valid17thChars = new[] { "I", "R", "Z" };
				result = valid17thChars.Any(c => ValidateSymbolAtANumber(customsEntryNumbertoValidate, c, 17));
			}
			else if (countryMRN.Equals(Core.Constants.CountryCodes.Estonia))
			{
				result = ValidateSymbolAtANumber(customsEntryNumbertoValidate, "C", 11) || ValidateSymbolAtANumber(customsEntryNumbertoValidate, "N", 11) || ValidateSymbolAtANumber(customsEntryNumbertoValidate, "T", 11);
			}
			else if (countryMRN.Equals(Core.Constants.CountryCodes.Greece))
			{
				result = ValidateSymbolAtANumber(customsEntryNumbertoValidate, "EN", 5) && ValidateNumericAtDigit(customsEntryNumbertoValidate, 7, 11);
			}
			else if (countryMRN.Equals(Core.Constants.CountryCodes.Spain))
			{
				result = ValidateSymbolAtANumber(customsEntryNumbertoValidate, "7", 11) || ValidateSymbolAtANumber(customsEntryNumbertoValidate, "B", 11) || ValidateSymbolAtANumber(customsEntryNumbertoValidate, "C", 11) || ValidateSymbolAtANumber(customsEntryNumbertoValidate, "D", 11)
					|| ValidateSymbolAtANumber(customsEntryNumbertoValidate, "E", 11) || ValidateSymbolAtANumber(customsEntryNumbertoValidate, "F", 11);
			}
			else if (countryMRN.Equals(Core.Constants.CountryCodes.Finland))
			{
				result = ValidateSymbolAtANumber(customsEntryNumbertoValidate, "I", 13) || ValidateSymbolAtANumber(customsEntryNumbertoValidate, "I", 17);
			}

			return result;
		}

		internal bool ImportCountrySecondPart(string countryMRN, string customsEntryNumbertoValidate)
		{
			var result = false;

			if (countryMRN.Equals(Core.Constants.CountryCodes.France))
			{
				result = ValidateSymbolAtANumber(customsEntryNumbertoValidate, "S", 5);
			}
			else if (countryMRN.Equals(Core.Constants.CountryCodes.Croatia))
			{
				result = true;
			}
			else if (countryMRN.Equals(Core.Constants.CountryCodes.Hungary) || countryMRN.Equals(Core.Constants.CountryCodes.Romania))
			{
				result = ValidateSymbolAtANumber(customsEntryNumbertoValidate, "I", 11);
			}
			else if (countryMRN.Equals(Core.Constants.CountryCodes.Ireland))
			{
				result = true;
			}
			else if (countryMRN.Equals(Core.Constants.CountryCodes.Italy))
			{
				result = ValidateSymbolAtANumber(customsEntryNumbertoValidate, "I", 17) || ValidateSymbolAtANumber(customsEntryNumbertoValidate, "N", 17);
			}
			else if (countryMRN.Equals(Core.Constants.CountryCodes.Lithuania))
			{
				result = ValidateSymbolAtANumber(customsEntryNumbertoValidate, "IV", 11);
			}
			else if (countryMRN.Equals(Core.Constants.CountryCodes.Luxembourg))
			{
				result = ValidateSymbolAtANumber(customsEntryNumbertoValidate, "ENS", 5);
			}
			else if (countryMRN.Equals(Core.Constants.CountryCodes.Malta))
			{
				result = ValidateSymbolAtANumber(customsEntryNumbertoValidate, "3", 9);
			}
			else if (countryMRN.Equals(Core.Constants.CountryCodes.Netherlands) || countryMRN.Equals(Core.Constants.CountryCodes.Portugal))
			{
				result = ValidateSymbolAtANumber(customsEntryNumbertoValidate, "4", 11);
			}
			else if (countryMRN.Equals(Core.Constants.CountryCodes.Poland))
			{
				result = ((ValidateSymbolAtANumber(customsEntryNumbertoValidate, "I", 11) || ValidateSymbolAtANumber(customsEntryNumbertoValidate, "D", 11) || ValidateSymbolAtANumber(customsEntryNumbertoValidate, "M", 11))
					&& ValidateNumericAtDigit(customsEntryNumbertoValidate, 5, 10) && ValidateNumericAtDigit(customsEntryNumbertoValidate, 12, 18));
			}
			else if (countryMRN.Equals(Core.Constants.CountryCodes.Sweden))
			{
				result = ValidateSymbolAtANumber(customsEntryNumbertoValidate, "SID", 5);
			}
			else if (countryMRN.Equals(Core.Constants.CountryCodes.Slovakia))
			{
				result = ValidateSymbolAtANumber(customsEntryNumbertoValidate, "IM", 9) || ValidateSymbolAtANumber(customsEntryNumbertoValidate, "PD", 9);
			}
			else if (countryMRN.Equals(Core.Constants.CountryCodes.UnitedKingdom))
			{
				result = ValidateSymbolAtANumber(customsEntryNumbertoValidate, "I", 7);
			}

			return result;
		}

		internal bool ExportCountryFirstPart(string countryMRN, string customsEntryNumbertoValidate)
		{
			var result = false;

			if (countryMRN.Equals(Core.Constants.CountryCodes.Austria))
			{
				result = ValidateSymbolAtANumber(customsEntryNumbertoValidate, "EN", 11) || ValidateSymbolAtANumber(customsEntryNumbertoValidate, "EV", 11)
						|| ValidateSymbolAtANumber(customsEntryNumbertoValidate, "EA", 11) || ValidateSymbolAtANumber(customsEntryNumbertoValidate, Core.Constants.CountryCodes.Spain, 11);
			}
			else if (countryMRN.Equals(Core.Constants.CountryCodes.Belgium))
			{
				result = ValidateSymbolAtANumber(customsEntryNumbertoValidate, "E", 5);
			}
			else if (countryMRN.Equals(Core.Constants.CountryCodes.Bulgaria))
			{
				result = ValidateSymbolAtANumber(customsEntryNumbertoValidate, "A", 11) || ValidateSymbolAtANumber(customsEntryNumbertoValidate, "C", 11) || ValidateSymbolAtANumber(customsEntryNumbertoValidate, "D", 11);
			}
			else if (countryExportWith2atpos11.Contains(countryMRN))
			{
				result = ValidateSymbolAtANumber(customsEntryNumbertoValidate, "2", 11);
			}
			else if (countryMRN.Equals(Core.Constants.CountryCodes.Germany))
			{
				var valid17thChars = new[] { "A", "B", "E", "X" };
				result = valid17thChars.Any(c => ValidateSymbolAtANumber(customsEntryNumbertoValidate, c, 17));
			}
			else if (countryMRN.Equals(Core.Constants.CountryCodes.Estonia))
			{
				result = ValidateSymbolAtANumber(customsEntryNumbertoValidate, Core.Constants.CountryCodes.Estonia, 9) && ValidateNumericAtDigit(customsEntryNumbertoValidate, 11, 11);
			}
			else if (countryMRN.Equals(Core.Constants.CountryCodes.Greece))
			{
				result = ValidateSymbolAtANumber(customsEntryNumbertoValidate, "EX", 5);
			}
			else if (countryMRN.Equals(Core.Constants.CountryCodes.Spain))
			{
				result = ValidateSymbolAtANumber(customsEntryNumbertoValidate, "1", 11) || ValidateSymbolAtANumber(customsEntryNumbertoValidate, "2", 11);
			}
			else if (countryMRN.Equals(Core.Constants.CountryCodes.Finland) || countryMRN.Equals(Core.Constants.CountryCodes.Italy))
			{
				result = ValidateSymbolAtANumber(customsEntryNumbertoValidate, "E", 17);
			}
			else if (countryMRN.Equals(Core.Constants.CountryCodes.France))
			{
				result = ValidateSymbolAtANumber(customsEntryNumbertoValidate, "C", 5) || ValidateSymbolAtANumber(customsEntryNumbertoValidate, "D", 5);
			}

			return result;
		}

		internal bool ExportCountrySecondPart(string countryMRN, string customsEntryNumbertoValidate)
		{
			var result = false;

			if (countryMRN.Equals(Core.Constants.CountryCodes.Croatia))
			{
				result = true;
			}
			else if (countryMRN.Equals(Core.Constants.CountryCodes.Ireland))
			{
				result = ValidateSymbolAtANumber(customsEntryNumbertoValidate, "EU", 8) || ValidateSymbolAtANumber(customsEntryNumbertoValidate, "CO", 8) || ValidateSymbolAtANumber(customsEntryNumbertoValidate, "EX", 8);
			}
			else if (countryMRN.Equals(Core.Constants.CountryCodes.Lithuania))
			{
				result = ValidateSymbolAtANumber(customsEntryNumbertoValidate, "E", 11) || ValidateSymbolAtANumber(customsEntryNumbertoValidate, "IS", 11);
			}
			else if (countryMRN.Equals(Core.Constants.CountryCodes.Poland))
			{
				result = ValidateSymbolAtANumber(customsEntryNumbertoValidate, "E", 11) || ValidateSymbolAtANumber(customsEntryNumbertoValidate, "W", 11) || ValidateSymbolAtANumber(customsEntryNumbertoValidate, "S", 11);
			}
			else if (countryMRN.Equals(Core.Constants.CountryCodes.Romania))
			{
				result = ValidateSymbolAtANumber(customsEntryNumbertoValidate, "E", 11);
			}
			else if (countryMRN.Equals(Core.Constants.CountryCodes.Sweden))
			{
				result = ValidateSymbolAtANumber(customsEntryNumbertoValidate, "E", 5) || ValidateSymbolAtANumber(customsEntryNumbertoValidate, "SUD", 5);
			}
			else if (countryMRN.Equals(Core.Constants.CountryCodes.Slovakia))
			{
				result = ValidateSymbolAtANumber(customsEntryNumbertoValidate, "EX", 9) || ValidateSymbolAtANumber(customsEntryNumbertoValidate, "PV", 9);
			}
			else if (countryMRN.Equals(Core.Constants.CountryCodes.UnitedKingdom))
			{
				result = ValidateSymbolAtANumber(customsEntryNumbertoValidate, "X", 7);
			}

			return result;
		}

		internal bool Validate14Digit(ZString customsEntryNumbertoValidate) => ZLong.TryParse(customsEntryNumbertoValidate.SubstringSafe(customsEntryNumbertoValidate.Length - 14, 14), out _);

		internal bool ValidateNumericAtDigit(ZString customsEntryNumbertoValidate, int indexStart, int indexEnd) => ZInt.TryParse(customsEntryNumbertoValidate.SubstringSafe(indexStart - 1, indexEnd - indexStart + 1), out _);

		internal bool ValidateSymbolAtANumber(ZString customsEntryNumbertoValidate, string digitToFind, int indexFirstChar) => customsEntryNumbertoValidate.SubstringSafe(indexFirstChar - 1, digitToFind.Length).Equals(digitToFind);

		internal bool CountryIsValid(string country) => countriesAuthorizedToMRN.Contains(country);
	}
	#endregion
}
