using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(ExportInvoiceLineWrapper))]
	sealed class ExportInvoiceLineWrapperTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var invoiceLine = new ExportInvoiceLine();
			return new ExportInvoiceLineWrapper(invoiceLine, Factory);
		}

		public void TestExportInvoiceLineFull()
		{
			var entry = new TestDataSetupHelper(Factory).GetExportEntryWithFullData();
			var wrapper = new EntryDocumentWrapper(entry, Factory).ExportEntryWrapper;

			var exportInvoiceLine1 = wrapper.EntryLineItems[0].InvoiceLineItems[0].InvoiceLine;
			AssertEquals("2007 N81011141", exportInvoiceLine1.DetailDescription);
			AssertEquals("dfewe", exportInvoiceLine1.Ingredient);
			AssertEquals(1m, exportInvoiceLine1.QtyOrWeight);
			AssertEquals("U", exportInvoiceLine1.QtyOrWeightUnit);
			AssertEquals(27670m, exportInvoiceLine1.UnitPrice);
			AssertEquals(27670m, exportInvoiceLine1.Amount);

			var exportInvoiceLine2 = wrapper.EntryLineItems[0].InvoiceLineItems[1].InvoiceLine;
			AssertEquals("모델규격2", exportInvoiceLine2.DetailDescription);
			AssertEquals("성분2", exportInvoiceLine2.Ingredient);
			AssertEquals(1234m, exportInvoiceLine2.QtyOrWeight);
			AssertEquals("KG", exportInvoiceLine2.QtyOrWeightUnit);
			AssertEquals(729.334887m, exportInvoiceLine2.UnitPrice);
			AssertEquals(899999.25m, exportInvoiceLine2.Amount);

			var exportInvoiceLine3 = wrapper.EntryLineItems[1].InvoiceLineItems[0].InvoiceLine;
			AssertEquals("모델규격3", exportInvoiceLine3.DetailDescription);
			AssertEquals("성분3", exportInvoiceLine3.Ingredient);
			AssertEquals(12345m, exportInvoiceLine3.QtyOrWeight);
			AssertEquals("KG", exportInvoiceLine3.QtyOrWeightUnit);
			AssertEquals(16.200830m, exportInvoiceLine3.UnitPrice);
			AssertEquals(199999.25m, exportInvoiceLine3.Amount);
		}
	}
}
