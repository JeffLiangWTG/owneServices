using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(CompanyNameChina))]
	sealed class CompanyNameChinaTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should not match <Company Name>", !ValueProviderToTest.IsResponsibleForReplacing("<Company Name>", Passes.FirstPass));
			Assert("should not match <copmany Name China>", !ValueProviderToTest.IsResponsibleForReplacing("<copmany Name China>", Passes.FirstPass));
			Assert("should match < company    name China       >", ValueProviderToTest.IsResponsibleForReplacing("< company    name China       >", Passes.FirstPass));
			Assert("should match <CompanyNameChina>", ValueProviderToTest.IsResponsibleForReplacing("<CompanyNameChina>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			AssertEquals(LocalCompanyName.GetCurrentCompanyLocalName(), ValueProviderToTest.GetReplacement("<Company Name China>", Report));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new CompanyNameChina();
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			CreateOrgProxy();
		}

		void CreateOrgProxy()
		{
			var factory = new BusinessObjectFactory();

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.China);
			var testOrgHeader = factory.NewWithValidTestData(typeof(OrgHeader)) as OrgHeader;

			var testAddress1 = testOrgHeader.Addresses.AddNew();
			var testAddress2 = testOrgHeader.Addresses.AddNew();
			var testAddress3 = testOrgHeader.Addresses.AddNew();
			var testAddress4 = testOrgHeader.Addresses.AddNew();
			var testAddress5 = testOrgHeader.Addresses.AddNew();

			testAddress1.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Payables);
			testAddress2.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Receivables);
			testAddress3.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Miscellaneous);
			testAddress4.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Receivables);
			testAddress5.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Receivables);

			testAddress1.OA_CompanyNameOverride = "AussieCOMPANY";
			testAddress2.OA_CompanyNameOverride = "";
			testAddress3.OA_CompanyNameOverride = "USCompany";
			testAddress4.OA_CompanyNameOverride = "CHINACOMPANYNonDefault";
			testAddress5.OA_CompanyNameOverride = "CHINACOMPANY";

			testAddress1.OA_Address1 = "Address1";
			testAddress2.OA_Address1 = "EmptyName";
			testAddress3.OA_Address1 = "Address3";
			testAddress4.OA_Address1 = "Address4";
			testAddress5.OA_Address1 = "Address5";

			testAddress1.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.Payables);
			testAddress2.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.Receivables);
			testAddress3.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.Miscellaneous);
			testAddress4.AddressCapability.SetIsNotMainAddress(OrgConstants.AddressType.Receivables);
			testAddress5.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.Receivables);
			factory.Save();

			GlbCompany.CurrentCompany.GC_OH_OrgProxy = testOrgHeader.PK;
		}

		#endregion
	}
}
