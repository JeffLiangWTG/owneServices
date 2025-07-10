namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	sealed class ImportInvoiceChargeLookupsTest : EU.Business.Declaration.Testing.InvoiceChargeLookupsTest
	{
		public void TestChargeTypeList_Import()
		{
			var chargeTypeList = invoiceCharge.Lookups.ChargeTypeList;

			CombineAssertions(() =>
			{
				AssertEquals("Codes", "1X, 2X, AB, AD, AE, AF, AG, AH, AI, AJ, AK, AL, AN, BA, BB, BC, BD, BE, BF, BG, OFT, ONS", chargeTypeList.CodesAsString);
				var newDeclaration = Factory.New<JobDeclaration>();
				newDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				AssertSame("Cached", chargeTypeList, newDeclaration.Invoices.AddNew().Charges.AddNew().Lookups.ChargeTypeList);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			invoiceCharge = declaration.Invoices.AddNew().Charges.AddNew();
		}

		JobDeclaration declaration;
		InvoiceCharge invoiceCharge;
	}
}
