using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	sealed class CopyDocumentsSelectionHeaderValidationTest : TestCaseWithFactory
	{
		public void TestCheckInvoiceNumber()
		{
			var invoiceLine = Factory.New<JobDeclaration>().Invoices.AddNew().InvoiceLines.AddNew();
			var header = new CopyDocumentsSelectionHeader(invoiceLine);

			CombineAssertions(() =>
			{
				ValidationTestHelper.AssertErrorIfNotEntered(header.InvoiceNumberInfo);
				ValidationTestHelper.AssertErrorIfInvalidCode(header.InvoiceNumberInfo, "INV", header.Lookups.AllCode);
			});
		}
	}
}
