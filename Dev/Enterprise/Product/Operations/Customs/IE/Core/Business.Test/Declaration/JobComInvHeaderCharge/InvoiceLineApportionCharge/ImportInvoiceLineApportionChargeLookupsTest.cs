using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	class ImportInvoiceLineApportionChargeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestChargeTypeList_Import()
		{
			var chargeTypeList = invoiceLineApportionCharge.Lookups.ChargeTypeList;

			CombineAssertions(() =>
			{
				AssertEquals("Codes", "1X, 2X, AB, AD, AE, AF, AG, AH, AI, AJ, AK, AL, AN, BA, BB, BC, BD, BE, BF, BG, OFT, ONS", chargeTypeList.CodesAsString);
				var newDeclaration = Factory.New<JobDeclaration>();
				newDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				AssertSame("Cached", chargeTypeList, newDeclaration.Invoices.AddNew().InvoiceLines.AddNew().ApportionedCharges.AddNew().Lookups.ChargeTypeList);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			invoiceLineApportionCharge = declaration.Invoices.AddNew().InvoiceLines.AddNew().ApportionedCharges.AddNew();
		}

		JobDeclaration declaration;
		InvoiceLineApportionCharge invoiceLineApportionCharge;
	}
}
