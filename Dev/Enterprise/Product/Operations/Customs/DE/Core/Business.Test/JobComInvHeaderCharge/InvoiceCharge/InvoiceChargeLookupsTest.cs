namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	public class InvoiceChargeLookupsTest : EU.Business.Declaration.Testing.InvoiceChargeLookupsTest
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

		public void TestChargeTypeList_Export()
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;

			AssertType<ChargeCodeList>(lookups.ChargeTypeList);
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceCharge = invoice.Charges.AddNew();
			lookups = invoiceCharge.Lookups;
		}
		JobDeclaration declaration;
		InvoiceChargeLookups lookups;
	}
}
