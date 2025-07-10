using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.FR.Business.Interfaces;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	public class SupplierBuyerLinkInsuranceChargeRuleTest : TestCaseWithFactory
	{
		public void TestChargeCode()
		{
			AssertEquals(FRCustomsChargeTypeList.Codes.InsuranceCostsCharge, testItem.GetChargeCode(declaration));

			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals(FRCustomsChargeTypeList.Codes.AirInsuranceCostsCharge, testItem.GetChargeCode(declaration));
		}

		public void TestGetPercentage()
		{
			var link = Factory.NewWithValidTestData<OrgSupplierBuyerLink>();
			AssertNoExceptionThrown("Should not throw exception for new OrgSupplierBuyerLink.", () => testItem.GetPercentage(link));

			link.OL_InsuranceUplift = 10m;
			AssertEquals("Should get correct OL_InsuranceUplift.", 10m, testItem.GetPercentage(link));
		}

		public void TestShouldApplyRule()
		{
			AssertEquals(true, testItem.ShouldApplyRule(declaration));
		}

		protected override void SetUp()
		{
			base.SetUp();
			testItem = new SupplierBuyerLinkInsuranceChargeRule();
			declaration = Factory.New<JobDeclaration>();
		}
		ISupplierBuyerLinkChargeRule testItem;
		JobDeclaration declaration;
	}
}
