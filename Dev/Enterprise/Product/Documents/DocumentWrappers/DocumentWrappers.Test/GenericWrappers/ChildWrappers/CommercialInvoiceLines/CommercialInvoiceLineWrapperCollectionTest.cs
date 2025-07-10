using Enterprise.Customs.Business;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(CommercialInvoiceLineWrapperCollection))]
	sealed class CommercialInvoiceLineWrapperCollectionTest : Base.Testing.GenericWrapperCollectionTest<CommercialInvoiceLineWrapperCollection>
	{
		public void TestLoadFromShipment()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoiceHeader1 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader1.JZ_InvoiceNumber = "INVOICE_2";
			BaseJobComInvoiceHeader invoiceHeader2 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader2.JZ_InvoiceNumber = "INVOICE_1";

			BaseJobComInvoiceLine invoiceLine1 = invoiceHeader1.JobComInvoiceLines.AddNew();
			BaseJobComInvoiceLine invoiceLine2 = invoiceHeader2.JobComInvoiceLines.AddNew();
			BaseJobComInvoiceLine invoiceLine3 = invoiceHeader2.JobComInvoiceLines.AddNew();

			declaration.InvoiceLines.Sort(JobComInvoiceLineSchema.JI_LineNo.Name, System.ComponentModel.ListSortDirection.Descending);

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			CommercialInvoiceLineWrapperCollection collection = new CommercialInvoiceLineWrapperCollection(shipment, Factory);
			AssertEquals("collection.Count", 0, collection.Count);

			declaration.JE_JS = shipment.PK;
			collection = new CommercialInvoiceLineWrapperCollection(shipment, Factory);
			AssertEquals("collection.Count", 3, collection.Count);
			AssertEquals("collection[0].Invoice.InvoiceNumber", "INVOICE_1", collection[0].Invoice.InvoiceNumber);
			AssertEquals("collection[0].LineNo", 1, collection[0].LineNo);
			AssertEquals("collection[1].Invoice.InvoiceNumber", "INVOICE_1", collection[1].Invoice.InvoiceNumber);
			AssertEquals("collection[1].LineNo", 2, collection[1].LineNo);
			AssertEquals("collection[2].Invoice.InvoiceNumber", "INVOICE_2", collection[2].Invoice.InvoiceNumber);
			AssertEquals("collection[2].LineNo", 1, collection[2].LineNo);
		}

		public void TestLoadFromDeclaration()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoiceHeader1 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader1.JZ_InvoiceNumber = "INVOICE_2";
			BaseJobComInvoiceHeader invoiceHeader2 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader2.JZ_InvoiceNumber = "INVOICE_1";

			CommercialInvoiceLineWrapperCollection collection = new CommercialInvoiceLineWrapperCollection(declaration, Factory);
			AssertEquals("collection.Count", 0, collection.Count);

			BaseJobComInvoiceLine invoiceLine1 = invoiceHeader1.JobComInvoiceLines.AddNew();
			BaseJobComInvoiceLine invoiceLine2 = invoiceHeader2.JobComInvoiceLines.AddNew();
			BaseJobComInvoiceLine invoiceLine3 = invoiceHeader2.JobComInvoiceLines.AddNew();

			declaration.InvoiceLines.Sort(JobComInvoiceLineSchema.JI_LineNo.Name, System.ComponentModel.ListSortDirection.Descending);

			collection = new CommercialInvoiceLineWrapperCollection(declaration, Factory);
			AssertEquals("collection.Count", 3, collection.Count);
			AssertEquals("collection[0].Invoice.InvoiceNumber", "INVOICE_1", collection[0].Invoice.InvoiceNumber);
			AssertEquals("collection[0].LineNo", 1, collection[0].LineNo);
			AssertEquals("collection[1].Invoice.InvoiceNumber", "INVOICE_1", collection[1].Invoice.InvoiceNumber);
			AssertEquals("collection[1].LineNo", 2, collection[1].LineNo);
			AssertEquals("collection[2].Invoice.InvoiceNumber", "INVOICE_2", collection[2].Invoice.InvoiceNumber);
			AssertEquals("collection[2].LineNo", 1, collection[2].LineNo);
		}

		public void TestLoadFromInvoiceHeader()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoiceHeader1 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader1.JZ_InvoiceNumber = "INVOICE_2";
			BaseJobComInvoiceHeader invoiceHeader2 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader2.JZ_InvoiceNumber = "INVOICE_1";

			CommercialInvoiceLineWrapperCollection collection = new CommercialInvoiceLineWrapperCollection(invoiceHeader1, Factory);
			AssertEquals("collection.Count", 0, collection.Count);
			collection = new CommercialInvoiceLineWrapperCollection(invoiceHeader2, Factory);
			AssertEquals("collection.Count", 0, collection.Count);

			BaseJobComInvoiceLine invoiceLine1 = invoiceHeader1.JobComInvoiceLines.AddNew();
			BaseJobComInvoiceLine invoiceLine2 = invoiceHeader2.JobComInvoiceLines.AddNew();
			BaseJobComInvoiceLine invoiceLine3 = invoiceHeader2.JobComInvoiceLines.AddNew();

			invoiceHeader1.JobComInvoiceLines.Sort(JobComInvoiceLineSchema.JI_LineNo.Name, System.ComponentModel.ListSortDirection.Descending);
			collection = new CommercialInvoiceLineWrapperCollection(invoiceHeader1, Factory);
			AssertEquals("collection.Count", 1, collection.Count);
			AssertEquals("collection[0].Invoice.InvoiceNumber", "INVOICE_2", collection[0].Invoice.InvoiceNumber);
			AssertEquals("collection[0].LineNo", 1, collection[0].LineNo);

			invoiceHeader2.JobComInvoiceLines.Sort(JobComInvoiceLineSchema.JI_LineNo.Name, System.ComponentModel.ListSortDirection.Descending);
			collection = new CommercialInvoiceLineWrapperCollection(invoiceHeader2, Factory);
			AssertEquals("collection.Count", 2, collection.Count);
			AssertEquals("collection[0].Invoice.InvoiceNumber", "INVOICE_1", collection[0].Invoice.InvoiceNumber);
			AssertEquals("collection[0].LineNo", 1, collection[0].LineNo);
			AssertEquals("collection[1].Invoice.InvoiceNumber", "INVOICE_1", collection[1].Invoice.InvoiceNumber);
			AssertEquals("collection[1].LineNo", 2, collection[1].LineNo);
		}

		#region Imeplementation
		protected override CommercialInvoiceLineWrapperCollection GetNewDocumentWrapperCollection()
		{
			return new CommercialInvoiceLineWrapperCollection((BaseJobComInvoiceHeader)null, Factory);
		}

		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			return new CommercialInvoiceLineWrapper(null, Factory);
		}
		#endregion
	}
}
