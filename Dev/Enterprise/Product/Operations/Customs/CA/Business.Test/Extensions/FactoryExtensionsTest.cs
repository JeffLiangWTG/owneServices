using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class FactoryExtensionsTest : TestCaseWithFactory
	{
		public void TestCurrentCompanyOrgProxyNull()
		{
			GlbCompany.CurrentCompany.GC_OH_OrgProxy = ZGuid.Empty;
			AssertEquals(ZString.Empty, Factory.CanadianCarrierCode());
		}

		public void TestCurrentCompanyHasCustomsRegNo()
		{
			var currentCompanyOrgProxy = GlbCompany.CurrentCompany.OrgProxy;
			AssertNotNull(currentCompanyOrgProxy);
			AssertEquals(ZString.Empty, Factory.CanadianCarrierCode());
			Factory.ClearCachedValue<ZString>("CA.Business.CandianCarrierCode");
			currentCompanyOrgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "CA1VALUE", Core.Constants.CountryCodes.Canada);
			AssertEquals("CA1VALUE", Factory.CanadianCarrierCode());
		}

		public void TestFallBackToActiveCanadianCompany()
		{
			var secondCACompany = Factory.NewWithValidTestData<GlbCompany>();
			secondCACompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			var secondCABranch = Factory.NewWithValidTestData<GlbBranch>();
			secondCACompany.Branches.Add(secondCABranch);
			Factory.Save();
			AssertEquals(ZString.Empty, Factory.CanadianCarrierCode());

			var secondCAOrgProxy = Factory.NewWithValidTestData<OrgHeader>();
			secondCAOrgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "CA2VALUE", Core.Constants.CountryCodes.Canada);
			secondCACompany.GC_OH_OrgProxy = secondCAOrgProxy.PK;
			Factory.Save();
			Factory.ClearCachedValue<ZString>("CA.Business.CandianCarrierCode");
			AssertEquals("CA2VALUE", Factory.CanadianCarrierCode());
		}

		public void TestFallBackActiveCompaniesOrder()
		{
			var secondCAOrgProxy = Factory.NewWithValidTestData<OrgHeader>();
			secondCAOrgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "CA2VALUE", Core.Constants.CountryCodes.Canada);

			var thirdCAOrgProxy = Factory.NewWithValidTestData<OrgHeader>();
			thirdCAOrgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "CA3VALUE", Core.Constants.CountryCodes.Canada);

			var secondCACompany = Factory.NewWithValidTestData<GlbCompany>();
			secondCACompany.GC_Code = "ABA";
			secondCACompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			secondCACompany.GC_OH_OrgProxy = secondCAOrgProxy.PK;
			var secondCABranch = Factory.NewWithValidTestData<GlbBranch>();
			secondCACompany.Branches.Add(secondCABranch);
			Factory.Save();
			AssertEquals("CA2VALUE", Factory.CanadianCarrierCode());

			var thirdCACompany = Factory.NewWithValidTestData<GlbCompany>();
			thirdCACompany.GC_Code = "AAA";
			thirdCACompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			thirdCACompany.GC_OH_OrgProxy = thirdCAOrgProxy.PK;
			var thirdCABranch = Factory.NewWithValidTestData<GlbBranch>();
			thirdCACompany.Branches.Add(thirdCABranch);
			Factory.Save();
			Factory.ClearCachedValue<ZString>("CA.Business.CandianCarrierCode");
			AssertEquals("CA3VALUE", Factory.CanadianCarrierCode());
		}
	}
}
