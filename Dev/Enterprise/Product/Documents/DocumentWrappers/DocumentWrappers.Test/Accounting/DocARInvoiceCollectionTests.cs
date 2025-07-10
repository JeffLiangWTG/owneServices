using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocARBatchInvoiceLineCollection))]
	public class DocARInvoiceCollectionTests : NonPersistentBusinessObjectCollectionTestCase<DocARBatchInvoiceLineCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var invoice = Factory.New<ARInvoice>();
			return DocARBatchInvoiceLine.New(invoice, Factory);
		}

		protected override DocARBatchInvoiceLineCollection GetCollectionToTest()
		{
			return new DocARBatchInvoiceLineCollection(Factory);
		}
	}
}
