namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	class ImportInvoiceLineChargeLookupsTest : EU.Business.Declaration.Testing.InvoiceLineChargeLookupsTest
	{
		public void TestChargeTypeList_Import()
		{
			var chargeTypeList = invoiceLineCharge.Lookups.ChargeTypeList;

			CombineAssertions(() =>
			{
				AssertEquals("Codes", "2X, AB, AD, AE, AF, AG, AH, AI, AJ, AL, AN, BB, BC, BD, BE, BF, BG, OFT, ONS", chargeTypeList.CodesAsString);
				var newDeclaration = Factory.New<JobDeclaration>();
				newDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				AssertSame("Cached", chargeTypeList, newDeclaration.Invoices.AddNew().InvoiceLines.AddNew().Charges.AddNew().Lookups.ChargeTypeList);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			invoiceLineCharge = declaration.Invoices.AddNew().InvoiceLines.AddNew().Charges.AddNew();
		}

		JobDeclaration declaration;
		InvoiceLineCharge invoiceLineCharge;
	}
}
