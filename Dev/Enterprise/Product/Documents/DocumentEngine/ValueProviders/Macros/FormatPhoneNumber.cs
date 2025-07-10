using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	public class FormatPhoneNumber : ValueProvider
	{
		public override Regex Regex
		{
			get { return regex; }
		}

		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<FormatPhoneNumber(\"{phoneNumber}\",\"{format}\")>",
				ResString.GetMultilingualString("b6bbadde-9690-4ed1-965b-7e20c836e0f6", @"Transforms a phone number into a specific format.
- {0}: The phone number.
- {1}: The format of the phone number. Value can be {2}, {3} or {4}.
Note: the original phone Number will be returned if the transformation fails.",
"phoneNumber", "format", "E164", "INTERNATIONAL", "NATIONAL"),
				new List<(string example, object expectedResult)> {
					((NoResString)"<FormatPhoneNumber(\"+61 426 829 924\",\"E164\")>", "+61426829924"),
					((NoResString)"<FormatPhoneNumber(\"+61 426 829 924\",\"INTERNATIONAL\")>", "+61 426 829 924"),
					((NoResString)"<FormatPhoneNumber(\"+61 426 829 924\",\"NATIONAL\")>", "0426 829 924")
				});
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			var result = string.Empty;
			var match = Regex.Match(macro);

			if (match.Success)
			{
				var phoneNumber = match.Groups["phoneNumber"].Value;
				var format = match.Groups["format"].Value.ToUpper(CultureInfo.InvariantCulture);
				switch (format)
				{
					case "E164":
						result = phoneNumberFormatterAndValidator.Normalize(phoneNumber, ZString.Empty);
						break;
					case "INTERNATIONAL":
						result = phoneNumberFormatterAndValidator.FormatInternational(phoneNumber, ZString.Empty);
						break;
					case "NATIONAL":
						result = phoneNumberFormatterAndValidator.FormatLocal(phoneNumber, ZString.Empty);
						break;
				}
				if (string.IsNullOrEmpty(result))
				{
					result = phoneNumber;
				}
			}

			return result;
		}

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "It's readonly and ititializes right away.")]
		static readonly PhoneNumberFormatterAndValidator phoneNumberFormatterAndValidator = new PhoneNumberFormatterAndValidator();
		static readonly Regex regex = new Regex(@"^<\s*FormatPhoneNumber\s*\(\s*""(?<phoneNumber>[^""]*?)"",\s*""(?<format>[^""]*?)""\s*\)\s*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
