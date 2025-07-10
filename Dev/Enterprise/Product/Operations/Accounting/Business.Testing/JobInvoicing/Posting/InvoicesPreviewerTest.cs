using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Posting.Testing
{
	[TestedType(typeof(InvoicesPreviewer))]
	public class InvoicesPreviewerTest : NonPersistentBusinessObjectTestCase
	{
		public void TestInvoicesPreviewer()
		{
			var creator = new TestObjectCreator(Factory);
			var invoice = Factory.NewWithValidTestData<APInvoice>();
			var transactions = new TransactionCreatorHashtable();
			transactions.AddAPInvoice(invoice, creator.AALSHI.OH_Code, invoice.InvoiceNumber);

			var previewer = new InvoicesPreviewer(transactions, JobInvoicingPostingOption.Costs);
			AssertEquals(1, previewer.PreviewInvoices.Count);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new InvoicesPreviewer((TransactionCreatorHashtable)null, JobInvoicingPostingOption.All);
		}
	}
}
