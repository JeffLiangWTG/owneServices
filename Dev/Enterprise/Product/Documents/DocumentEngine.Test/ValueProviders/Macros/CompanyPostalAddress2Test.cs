using Enterprise.Core;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(CompanyPostalAddress2))]
	sealed class CompanyPostalAddress2Test : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should match < CompanyPostalAddress2 >", ValueProviderToTest.IsResponsibleForReplacing("< CompanyPostalAddress2 >", Passes.FirstPass));
			Assert("should match < Company Address 1 >", ValueProviderToTest.IsResponsibleForReplacing("< Company Postal Address 2 >", Passes.FirstPass));
			Assert("should match < Company Address1 >", ValueProviderToTest.IsResponsibleForReplacing("< Company Postal Address2 >", Passes.FirstPass));
			Assert("should match < CompanyAddress 1>", ValueProviderToTest.IsResponsibleForReplacing("< CompanyPostalAddress 2>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			GlbCompany.CurrentCompany.OrgProxy.Addresses.RemoveAndDeleteAll();
			AssertEquals("An empty string should be returned if there is no postal address", "", ValueProviderToTest.GetReplacement("<CompanyPostalAddress2>", Report));

			OrgAddress postalAddress = GlbCompany.CurrentCompany.OrgProxy.Addresses.AddNew();
			postalAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Postal.Code);
			postalAddress.OA_Address2 = "PostalAddress1";
			AssertEquals("Postal OA_Address1 should be returned if it exists", postalAddress.OA_Address2, ValueProviderToTest.GetReplacement("<CompanyPostalAddress2>", Report));

			Report.Parent.Language = SharedConstants.Languages.ChineseSimplified;
			OrgAddress postalAddressChinese = GlbCompany.CurrentCompany.OrgProxy.Addresses.AddNew();
			postalAddressChinese.OA_Language = SharedConstants.Languages.ChineseSimplified;
			postalAddressChinese.AddressCapability.SetCapabilityEnabled(OrgAddressType.Postal.Code);
			postalAddressChinese.AddressCapability.SetIsMainAddress(OrgAddressType.Postal.Code);
			postalAddressChinese.OA_Address2 = "宝塔镇河妖";
			using (Res.TemporarilySwitchLanguage(SharedConstants.Languages.ChineseSimplified))
			{
				AssertEquals("Postal OA_Address1 should be returned if it exists", postalAddressChinese.OA_Address2, ValueProviderToTest.GetReplacement("<CompanyPostalAddress2>", Report));
			}
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new CompanyPostalAddress2();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			GlbCompany.CurrentCompany.OrgProxy.Addresses.RemoveAndDeleteAll();
			var postalAddress = GlbCompany.CurrentCompany.OrgProxy.Addresses.AddNew();
			postalAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Postal.Code);
			postalAddress.OA_Address2 = "Postal Address Line 2";
		}
	}
}
