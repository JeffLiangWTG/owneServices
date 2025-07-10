using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class LocalBusinessNumber : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<LocalBusinessNumber({orgheaderpk})>",
				ResString.GetMultilingualString("04d6b680-30bb-489d-bc2a-c53bd7703a85", @"Returns the local Business Registration Number of the Organization represented by the {0}.", "orgheaderpk"),
				new List<(string example, object expectedResult)> { ("<LocalBusinessNumber(<OH_PK>)>", "123 456 789 0") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			// Arg 1: orgPK
			string orgPK = regex.Match(macro).Groups[1].Value;

			if (!ZGuid.TryParse(orgPK, out var pK))
			{
				ReportMacroError(report, Res.GetString("b5fa14dc-ba66-486f-90a3-597b61470a9f", "Unable to obtain Local Business Number. Please check {0} is a valid {1}.", orgPK, "orgheaderpk")); // orgheaderpk is the table name from which the Local Business Number is obtained. Should not be translated - is in documentation.
				return string.Empty;
			}

			var orgHeader = (new BusinessObjectFactory()).Load<OrgHeader>(pK);
			if (orgHeader == null)
			{
				ReportMacroError(report, Res.GetString("9a577fe5-e15a-4c6e-9baf-9622f22766c5", "Unable to obtain Local Business Number. No organization matching the {0} '{1}' could be found.", "orgheaderpk", orgPK));
				return string.Empty;
			}

			return orgHeader.PrimaryRegistrationNumber.Number;
		}

		public override Regex Regex => regex;
		static readonly Regex regex = new Regex(@"^<(?:[\s]*)localbusinessnumber(?:[\s]*)\((?:[\s]*)([^\s]+)(?:[\s]*)\)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
