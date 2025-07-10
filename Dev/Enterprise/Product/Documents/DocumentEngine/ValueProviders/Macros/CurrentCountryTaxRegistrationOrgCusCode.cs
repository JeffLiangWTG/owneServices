using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	internal class CurrentCountryTaxRegistrationOrgCusCode : ValueProviderWithLoadControlFactory
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<CurrentCountryTaxRegistrationOrgCusCode>",
				ResString.GetMultilingualString("340e8e05-747a-440a-946e-6c2c40c9a6c8", @"returns Current Company's registered Consumption Tax code."),
				new List<(string example, object expectedResult)> { ("<CurrentCountryTaxRegistrationOrgCusCode>", "ABN") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			return Country.GetConsumptionTaxRegistrationOrgCusCode(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}

		static readonly Regex fRegex = new Regex(@"^<\s*CurrentCountryTaxRegistrationOrgCusCode\s*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
