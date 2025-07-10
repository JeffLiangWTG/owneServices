using Enterprise.Core;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(CompanyPostalAddress1))]
	sealed class CompanyPostalAddress1Test : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should match < CompanyPostalAddress1 >", ValueProviderToTest.IsResponsibleForReplacing("< CompanyPostalAddress1 >", Passes.FirstPass));
			Assert("should match < Company Address 1 >", ValueProviderToTest.IsResponsibleForReplacing("< Company Postal Address 1 >", Passes.FirstPass));
			Assert("should match < Company Address1 >", ValueProviderToTest.IsResponsibleForReplacing("< Company Postal Address1 >", Passes.FirstPass));
			Assert("should match < CompanyAddress 1>", ValueProviderToTest.IsResponsibleForReplacing("< CompanyPostalAddress 1>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			GlbCompany.CurrentCompany.OrgProxy.Addresses.RemoveAndDeleteAll();
			AssertEquals("An empty string should be returned if there is no postal address", "", ValueProviderToTest.GetReplacement("<CompanyPostalAddress1>", Report));

			var postalAddress = GlbCompany.CurrentCompany.OrgProxy.Addresses.AddNew();
			postalAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Postal.Code);
			postalAddress.OA_Address1 = "PostalAddress1";
			AssertEquals("Postal OA_Address1 should be returned if it exists", postalAddress.OA_Address1, ValueProviderToTest.GetReplacement("<CompanyPostalAddress1>", Report));

			Report.Parent.Language = SharedConstants.Languages.ChineseSimplified;
			var postalAddressChinese = GlbCompany.CurrentCompany.OrgProxy.Addresses.AddNew();
			postalAddressChinese.OA_Language = SharedConstants.Languages.ChineseSimplified;
			postalAddressChinese.AddressCapability.SetCapabilityEnabled(OrgAddressType.Postal.Code);
			postalAddressChinese.AddressCapability.SetIsMainAddress(OrgAddressType.Postal.Code);
			postalAddressChinese.OA_Address1 = "天王盖地虎";
			using (Res.TemporarilySwitchLanguage(SharedConstants.Languages.ChineseSimplified))
			{
				AssertEquals("Postal OA_Address1 should be returned if it exists", postalAddressChinese.OA_Address1, ValueProviderToTest.GetReplacement("<CompanyPostalAddress1>", Report));
			}
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new CompanyPostalAddress1();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			GlbCompany.CurrentCompany.OrgProxy.Addresses.RemoveAndDeleteAll();
			var postalAddress = GlbCompany.CurrentCompany.OrgProxy.Addresses.AddNew();
			postalAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Postal.Code);
			postalAddress.OA_Address1 = "Postal Address Line 1";
		}
	}
}
