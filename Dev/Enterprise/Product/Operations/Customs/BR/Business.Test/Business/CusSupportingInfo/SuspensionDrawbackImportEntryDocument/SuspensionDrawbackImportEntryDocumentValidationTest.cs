using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class SuspensionDrawbackImportEntryDocumentValidationTest : BusinessObjectValidationTestCase
	{
		public void TestParent()
		{
			var parent = Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew().SuspensionDrawbackCollection.AddNew().SuspensionDrawbackInvoiceCollection.AddNew();
			AssertEquals("Parent", parent, parent.Validation.Parent);
		}
	}
}
