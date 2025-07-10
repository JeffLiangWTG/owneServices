using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(InvoicingBaseConsolCollectionForImporting))]
	public class InvoicingBaseConsolCollectionForImportingTest : APInvoiceConsolCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			APInvoice invoice = Factory.New<APInvoice>();
			APInvoiceConsolCosting costing = new APInvoiceConsolCosting(Factory, invoice);
			InvoicingBaseBulkConsolCostImporter importer = new InvoicingBaseBulkConsolCostImporter(costing);
			return new InvoicingBaseConsolCollectionForImporting(importer, Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New(typeof(InvoicingBaseConsolForImporting));
		}
	}
}
