using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.Interfaces;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	class SupplierBuyerLinkRFLChargeRuleTest : TestCaseWithFactory
	{
		public void TestChargeCode()
		{
			AssertEquals("ChargeCode should be RFL.", UCCCustomsChargeTypeList.Codes.RoyaltiesLicenseFeeCharge, testItem.GetChargeCode(null));
		}

		public void TestGetPercentage()
		{
			var link = Factory.NewWithValidTestData<OrgSupplierBuyerLink>();
			AssertNoExceptionThrown("Should not throw exception for new OrgSupplierBuyerLink.", () => testItem.GetPercentage(link));

			link.OL_BuyingCommissionPercentage = 1.22;
			link.OL_RoyaltyPercentage = 3.44;
			AssertEquals("Should get correct OL_RoyaltyPercentage.", 3.44m, testItem.GetPercentage(link));
		}

		public void TestShouldApplyRule()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("Should not apply for EXP jobs.", true, testItem.ShouldApplyRule(declaration));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("Should not apply for IMP jobs.", true, testItem.ShouldApplyRule(declaration));
		}

		ISupplierBuyerLinkChargeRule testItem;

		protected override void SetUp()
		{
			base.SetUp();
			testItem = new SupplierBuyerLinkRFLChargeRule();
		}
	}
}
