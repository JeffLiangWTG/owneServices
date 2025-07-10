using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Common.EU
{
	public static class MRNFormatValidator
	{
		public static ZString CheckMRNFormat(ZString mrn, BusinessObjectFactory factory, ZString preRequisteMsg)
		{
			var errors = new ZStringBuilder();
			if (!mrn.IsEmpty)
			{
				var mrnRegex = new Regex("^[0-9]{2}(?<CountryCode>[A-Z]{2})[A-Z0-9]{13}(?<CheckDigit>[0-9])$");
				var match = mrnRegex.Match(mrn);

				if (!match.Success)
				{
					errors.AppendIfNotEmpty(Res.GetString("14876346-C839-47F7-BC42-874FA5577A52", "{0} {1}", preRequisteMsg, MRNFormatInvalidMsg));
				}
				else
				{
					errors.AppendIfNotEmpty(CheckCountryCode(match.Groups["CountryCode"].Value, factory));
					errors.AppendIfNotEmpty(CheckMRNCheckDigit(mrn));
				}
			}
			return errors.ToStringWithNewLineBetweenAppends();
		}

		static ZString CheckCountryCode(ZString countryCode, BusinessObjectFactory factory)
		{
			return MRNAndGRNFormatValidatorHelper.IsCountryCodeValid(countryCode, factory) ? string.Empty : Res.GetString("495E4FAD-1333-49D5-8615-42B16BFDF16E", "MRN does not contain a valid country/region code");
		}

		static ZString CheckMRNCheckDigit(ZString mrn)
		{
			(var isValidDigit, var result) = MRNAndGRNFormatValidatorHelper.IsMRNDigitValid(mrn);
			return isValidDigit ? string.Empty : Res.GetString("958AD919-88C2-4F57-AF0E-E10C3B73343D", "MRN does not have a valid last digit. The last digit should be {0}", result);
		}

		static string MRNFormatInvalidMsg => Res.GetString("6a3d2e4e-44db-4770-adbb-02a39885c250", @"a MRN structure is required (18 alphanumeric characters). Please enter a MRN in the following format with only numbers and upper case letters:
• two numbers for the year of issue,
• two letters for the ISO country/region code for country/region of issue,
• thirteen alphanumeric characters for unique identification and
• one number check digit");
	}
}
