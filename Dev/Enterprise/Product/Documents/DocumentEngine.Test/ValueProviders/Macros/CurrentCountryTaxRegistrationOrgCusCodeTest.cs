using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(CurrentCountryTaxRegistrationOrgCusCode))]
	sealed class CurrentCountryTaxRegistrationOrgCusCodeTest : ValueProviderWithLoadControlFactoryTest<CurrentCountryTaxRegistrationOrgCusCode>
	{
		public override void TestIsResponsibleForReplacing()
		{
			Assert("Should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.SecondPass));
			Assert("Should not match <CurrentCountryTaxRegistrationOrgCusCode()>", !ValueProviderToTest.IsResponsibleForReplacing("<CurrentCountryTaxRegistrationOrgCusCode()>", Passes.SecondPass));
			Assert("Should not match < CurrentCountryTaxRegistrationOrgCusCode ( ) >", !ValueProviderToTest.IsResponsibleForReplacing("< CurrentCountryTaxRegistrationOrgCusCode ( ) >", Passes.SecondPass));
			Assert("Should not match <CurrentCountryTaxRegistrationOrgCusCode(AALSHI)>", !ValueProviderToTest.IsResponsibleForReplacing("<CurrentCountryTaxRegistrationOrgCusCode(AALSHI)>", Passes.SecondPass));
			Assert("Should not match < CurrentCountryTaxRegistrationOrgCusCode ( AALSHI ) >", !ValueProviderToTest.IsResponsibleForReplacing("< CurrentCountryTaxRegistrationOrgCusCode ( AALSHI ) >", Passes.SecondPass));
			Assert("Should match <CurrentCountryTaxRegistrationOrgCusCode>", ValueProviderToTest.IsResponsibleForReplacing("<CurrentCountryTaxRegistrationOrgCusCode>", Passes.SecondPass));
		}

		public override void TestReplacement()
		{
			AssertEquals(Country.GetConsumptionTaxRegistrationOrgCusCode(GlbCompany.CurrentCompany.GC_RN_NKCountryCode), ValueProviderToTest.GetReplacement("<CurrentCountryTaxRegistrationOrgCusCode>", Report));
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = "AU";
		}
	}
}
