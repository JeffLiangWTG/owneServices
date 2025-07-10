using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	class AddInfoJobComInvoiceLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckZG_ReimportDate_EntryReleaseDate()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			var entry2 = declaration.CustomsEntryHeaders.AddNew();

			CombineAssertions(() =>
			{
				invoiceLine.ZG_ReimportDate = ZDateTime.Today.AddDays(-5);
				AssertHasMessageError("All customsEntryHeaders have empty CH_EntryReleaseDate", invoiceLine.ZG_ReimportDateInfo, "The date of Re-Importation must not be earlier than the current date.");

				entry1.CH_EntryReleaseDate = new ZDateTime(2020, 1, 19);
				invoiceLine.ZG_ReimportDate = ZDateTime.Today.AddDays(-5);
				AssertNoMessageError("Some customsEntryHeaders have non-empty CH_EntryReleaseDate", invoiceLine.ZG_ReimportDateInfo, "The date of Re-Importation must not be earlier than the current date.");
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.InvoiceLines.AddNew();
		}
		JobDeclaration declaration;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;
	}
}
