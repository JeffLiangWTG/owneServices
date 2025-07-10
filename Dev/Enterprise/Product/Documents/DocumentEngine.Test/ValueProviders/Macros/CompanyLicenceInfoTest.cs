using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(CompanyLicenceInfo))]
	sealed class CompanyLicenceInfoTest : ValueProviderWithLoadControlFactoryTest<CompanyLicenceInfo>
	{
		public override void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should not match <CompanyLicenceInfo>", !ValueProviderToTest.IsResponsibleForReplacing("<CompanyLicenceInfo>", Passes.FirstPass));
			Assert("should not match <CompanyLicenceInfo.LicenceServerID>", !ValueProviderToTest.IsResponsibleForReplacing("<CompanyLicenceInfo.LicenceServerID>", Passes.FirstPass));
			Assert("should match <CompanyLicenceInfo().LicenceServerID>", ValueProviderToTest.IsResponsibleForReplacing("<CompanyLicenceInfo().LicenceServerID>", Passes.FirstPass));
			Assert("should match <CompanyLicenceInfo(0D784091-0C42-450A-A2A2-4A8995124112).LicenceServerID>", ValueProviderToTest.IsResponsibleForReplacing("<CompanyLicenceInfo(0D784091-0C42-450A-A2A2-4A8995124112).LicenceServerID>", Passes.FirstPass));
			Assert("should match <CompanyLicenceInfo(0D784091-0C42-450A-A2A2-4A8995124112).LicenceEnterpriseCode>", ValueProviderToTest.IsResponsibleForReplacing("<CompanyLicenceInfo(0D784091-0C42-450A-A2A2-4A8995124112).LicenceEnterpriseCode>", Passes.FirstPass));
			Assert("should match <CompanyLicenceInfo(0D784091-0C42-450A-A2A2-4A8995124112).LicenceCompanyCode>", ValueProviderToTest.IsResponsibleForReplacing("<CompanyLicenceInfo(0D784091-0C42-450A-A2A2-4A8995124112).LicenceCompanyCode>", Passes.FirstPass));
			Assert("should not match <CompanyLicenceInfo(0D784091-0C42-450A-A2A2-4A8995124112).Whatever>", !ValueProviderToTest.IsResponsibleForReplacing("<CompanyLicenceInfo(0D784091-0C42-450A-A2A2-4A8995124112).Whatever>", Passes.FirstPass));
			Assert("should still match bad guid <CompanyLicenceInfo(0D784091-0W42-450A-A2A2-4A8995124112).LicenceServerID>", ValueProviderToTest.IsResponsibleForReplacing("<CompanyLicenceInfo(0D784091-0W42-450A-A2A2-4A8995124112).LicenceServerID>", Passes.FirstPass));

			Assert("should match <CompanyLicenceInfo(<Another.Macro>).LicenceServerID>", ValueProviderToTest.IsResponsibleForReplacing("<CompanyLicenceInfo(<Another.Macro>).LicenceServerID>", Passes.FirstPass));
		}

		public override void TestReplacement()
		{
			BusinessObjectFactory factory = ((ValueProviderWithLoadControlFactory)ValueProviderToTest).FactoryForTesting;
			GlbCompany anotherCompany = factory.NewWithValidTestData<GlbCompany>();
			anotherCompany.GC_Code = "AAA";

			GlbCompany company = GlbCompany.CurrentCompany;

			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			var enterpriseCode = registrationKey.EnterpriseCode;
			var serverCode = registrationKey.ServerCode;

			AssertEquals(serverCode, ValueProviderToTest.GetReplacement(String.Format("<CompanyLicenceInfo({0}).LicenceServerID>", company.PK.ToString()), Report));
			AssertEquals(enterpriseCode, ValueProviderToTest.GetReplacement(String.Format("<CompanyLicenceInfo({0}).LicenceEnterpriseCode>", company.PK.ToString()), Report));
			AssertEquals(company.GC_Code.ToString(), ValueProviderToTest.GetReplacement(String.Format("<CompanyLicenceInfo({0}).LicenceCompanyCode>", company.PK.ToString()), Report));

			AssertEquals(serverCode, ValueProviderToTest.GetReplacement("<CompanyLicenceInfo().LicenceServerID>", Report));
			AssertEquals(enterpriseCode, ValueProviderToTest.GetReplacement("<CompanyLicenceInfo().LicenceEnterpriseCode>", Report));
			AssertEquals(company.GC_Code.ToString(), ValueProviderToTest.GetReplacement("<CompanyLicenceInfo().LicenceCompanyCode>", Report));

			AssertEquals(company.GC_Code, ValueProviderToTest.GetReplacement("<CompanyLicenceInfo(WHATEVER OTHER CRAP THEY PUT IN).LicenceCompanyCode>", Report));
			AssertEquals("AAA", ValueProviderToTest.GetReplacement(String.Format("<CompanyLicenceInfo({0}).LicenceCompanyCode>", anotherCompany.PK.ToString()), Report));
		}
	}
}
