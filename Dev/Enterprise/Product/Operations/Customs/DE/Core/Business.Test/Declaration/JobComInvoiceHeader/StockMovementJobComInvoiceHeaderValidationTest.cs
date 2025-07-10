using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	class StockMovementJobComInvoiceHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckJZ_Incoterm()
		{
			invoice.Validation.ValidateJZ_IncoTerm();
			AssertNoMessageErrors(invoice.JZ_IncoTermInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.LUZ;
			invoice = declaration.Invoices.AddNew();
		}
		JobComInvoiceHeader invoice;
	}
}
