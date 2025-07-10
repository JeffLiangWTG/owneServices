using Enterprise.Customs.Business;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	public class GroupInvoiceChargeLookupsTest : EU.Business.Declaration.Testing.GroupInvoiceChargeLookupsTest
	{
		public void TestChargeTypeList_Import_NoAIRType()
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;

			var chargeTypeList = lookups.ChargeTypeList;

			CombineAssertions(() =>
			{
				AssertEquals("No Charge Type AIR", false, chargeTypeList.ContainsCode(ImportChargeCodeList.Codes.AIR));
				AssertSame("Cached", chargeTypeList, lookups.ChargeTypeList);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			declaration.Invoices.AddNew();
			var groupCharge = declaration.TopGroupInvoice.Charges.AddNew();
			lookups = groupCharge.Lookups;
		}
		JobDeclaration declaration;
		JobComInvHeaderChargeLookups lookups;
	}
}
