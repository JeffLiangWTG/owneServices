using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class OverCreditLimit : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<OverCreditLimit({OrganizationPK})>",
				ResString.GetMultilingualString("2f8c681b-8422-44c8-ac61-fbb12c7edfa8", "Check if a organization is over credit limit."),
				new List<(string example, object expectedResult)> {
					("<OverCreditLimit(<OH_PK>)>", "N") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			string macroMatch = Regex.Match(macro).Groups[1].Value;
			ZGuid organizationPK;
			OrgHeader orgHeader = null;

			if (ZGuid.TryParse(macroMatch, out organizationPK))
			{
				orgHeader = report.Factory.Load<OrgHeader>(organizationPK);
			}
			if (orgHeader != null)
			{
				return (orgHeader.CreditChecker?.DoesExceedCreditLimit() ?? false) ? "Y" : "N";
			}
			else
			{
				return ZString.Empty;
			}
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<(?:[\s]*)OverCreditLimit(?:[\s]*)\((?:[\s]*)([^\s]+)(?:[\s]*)\)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
