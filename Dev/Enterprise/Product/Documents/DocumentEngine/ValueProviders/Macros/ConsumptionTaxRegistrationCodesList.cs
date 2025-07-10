using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	internal class ConsumptionTaxRegistrationCodesList : ValueProviderWithLoadControlFactory
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<ConsumptionTaxRegistrationCodesList({organisation code})>",
				ResString.GetMultilingualString("f12959eb-fa4b-4d07-b1db-ed9c9bf35a5a", @"For a given Organization, returns the comma-separated list of its registered Consumption Tax codes."),
				new List<(string example, object expectedResult)> { ("<ConsumptionTaxRegistrationCodesList(<OrgCode>)>", (NoResString)"SG SGGST, AU AUABN, DE DEUST") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			string result = string.Empty;
			OrgHeader header = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, Regex.Match(macro).Groups[1].Value);

			if (header != null)
			{
				List<string> regCodes = new List<string>();

				foreach (OrgCusCode orgCusCode in header.CustomsCodes)
				{
					if (orgCusCode.OK_CodeType == Country.GetConsumptionTaxRegistrationOrgCusCode(orgCusCode.OK_RN_NKCodeCountry))
					{
						regCodes.Add(string.Format("{0} {1}", orgCusCode.OK_RN_NKCodeCountry, orgCusCode.OK_CustomsRegNo));
					}
				}

				result = string.Join(", ", regCodes.ToArray());
			}

			return result;
		}

		public override Passes PassToStartReplacingOn
		{
			get { return Passes.SecondPass; }
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}

		static readonly Regex fRegex = new Regex(@"^<\s*ConsumptionTaxRegistrationCodesList\s*\(\s*(.+)\s*\)\s*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
