namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	class ImportInvoiceApportionChargeLookupsTest : EU.Business.Declaration.Testing.InvoiceApportionChargeLookupsTest
	{
		public void TestChargeTypeList_Import()
		{
			var chargeTypeList = invoiceApportionCharge.Lookups.ChargeTypeList;

			CombineAssertions(() =>
			{
				AssertEquals("Codes", "1X, 2X, AB, AD, AE, AF, AG, AH, AI, AJ, AK, AL, AN, BA, BB, BC, BD, BE, BF, BG, OFT, ONS", chargeTypeList.CodesAsString);
				var newDeclaration = Factory.New<JobDeclaration>();
				newDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				AssertSame("Cached", chargeTypeList, newDeclaration.Invoices.AddNew().GroupCharges.AddNew().Lookups.ChargeTypeList);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			invoiceApportionCharge = declaration.Invoices.AddNew().GroupCharges.AddNew();
		}

		JobDeclaration declaration;
		InvoiceApportionCharge invoiceApportionCharge;
	}
}
