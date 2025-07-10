using System;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.DataTransfer.Testing;

namespace Enterprise.Accounting.DataTransfer.Invoices.Testing
{
	public class JobRelatedARInvoicesExportFilterTest : TransactionExportFilterTestBase
	{
		public new void TestGetFilterPks()
		{
			Assert("The test is not suitable here.", true);
		}

		public void TestFilterProvider()
		{
			JobRelatedARInvoicesExportFilter filter = new JobRelatedARInvoicesExportFilter(Factory, FilterProvider, ObjectCreator.Job1.JH_JobNum, false);

			ARInvoice testARInvoice1 = Factory.NewWithValidTestData<ARInvoice>();
			AddInvoiceLineWithTestValues(testARInvoice1, ObjectCreator.Job1.PK);
			testARInvoice1.AH_ConsolidatedInvoiceRef = ObjectCreator.Job1.JH_JobNum + "/E";
			Factory.Save();

			ARInvoice testARConsolInvoice1 = Factory.NewWithValidTestData<ARInvoice>();
			AddInvoiceLineWithTestValues(testARConsolInvoice1, ObjectCreator.Job1.PK);
			testARConsolInvoice1.AH_ConsolidatedInvoiceRef = ObjectCreator.Job1.JH_JobNum + "/E";
			testARConsolInvoice1.AH_JH = ZGuid.Empty;
			Factory.Save();

			InvoicingBaseCollection invoices = new InvoicingBaseCollection(Factory);
			invoices.Load(filter.Filter);

			Assert("Invoice Collection contains ARInvoice1", BusinessObjectIsInCollectionByPK(invoices, ARInvoice1));
			Assert("Invoice Collection contains TestARInvoice1", BusinessObjectIsInCollectionByPK(invoices, testARInvoice1));
			Assert("Invoice Collection does not contain TestARConsolInvoice1", !BusinessObjectIsInCollectionByPK(invoices, testARConsolInvoice1));
			AssertEquals("Invoice Collection Count", 2, invoices.Count);

			ARInvoice testARInvoice2 = Factory.NewWithValidTestData<ARInvoice>();
			AddInvoiceLineWithTestValues(testARInvoice2, ObjectCreator.Job1.PK);
			testARInvoice2.AH_ConsolidatedInvoiceRef = ObjectCreator.Job1.JH_JobNum + "01";
			Factory.Save();

			ARInvoice testARConsolInvoice2 = Factory.NewWithValidTestData<ARInvoice>();
			AddInvoiceLineWithTestValues(testARConsolInvoice2, ObjectCreator.Job1.PK);
			testARConsolInvoice2.AH_ConsolidatedInvoiceRef = ObjectCreator.Job1.JH_JobNum + "01";
			testARConsolInvoice2.AH_JH = ZGuid.Empty;
			Factory.Save();

			invoices = new InvoicingBaseCollection(Factory);
			invoices.Load(filter.Filter);

			Assert("Invoice Collection contains ARInvoice1", BusinessObjectIsInCollectionByPK(invoices, ARInvoice1));
			Assert("Invoice Collection does not TestARInvoice2", !BusinessObjectIsInCollectionByPK(invoices, testARInvoice2));
			Assert("Invoice Collection does not contain TestARConsolInvoice2", !BusinessObjectIsInCollectionByPK(invoices, testARConsolInvoice2));
			AssertEquals("Invoice Collection Count", 2, invoices.Count);

			filter = new JobRelatedARInvoicesExportFilter(Factory, FilterProvider, ObjectCreator.Job1.JH_JobNum, true);

			ARInvoice testARInvoice3 = Factory.NewWithValidTestData<ARInvoice>();
			AddInvoiceLineWithTestValues(testARInvoice3, ObjectCreator.Job1.PK);
			testARInvoice3.AH_ConsolidatedInvoiceRef = ObjectCreator.Job1.JH_JobNum + "/E";
			Factory.Save();

			ARInvoice testARConsolInvoice3 = Factory.NewWithValidTestData<ARInvoice>();
			AddInvoiceLineWithTestValues(testARConsolInvoice3, ObjectCreator.Job1.PK);
			testARConsolInvoice3.AH_ConsolidatedInvoiceRef = ObjectCreator.Job1.JH_JobNum + "/E";
			testARConsolInvoice3.AH_JH = ZGuid.Empty;
			Factory.Save();

			invoices = new InvoicingBaseCollection(Factory);
			invoices.Load(filter.Filter);

			Assert("Invoice Collection does not contain ARInvoice1", !BusinessObjectIsInCollectionByPK(invoices, ARInvoice1));
			Assert("Invoice Collection does not contain TestARInvoice3", !BusinessObjectIsInCollectionByPK(invoices, testARInvoice3));
			Assert("Invoice Collection contains TestARConsolInvoice3", BusinessObjectIsInCollectionByPK(invoices, testARConsolInvoice3));
			AssertEquals("Invoice Collection Count", 2, invoices.Count);

			ARInvoice testARInvoice4 = Factory.NewWithValidTestData<ARInvoice>();
			AddInvoiceLineWithTestValues(testARInvoice4, ObjectCreator.Job1.PK);
			testARInvoice4.AH_ConsolidatedInvoiceRef = ObjectCreator.Job1.JH_JobNum + "01";
			Factory.Save();

			ARInvoice testARConsolInvoice4 = Factory.NewWithValidTestData<ARInvoice>();
			AddInvoiceLineWithTestValues(testARConsolInvoice4, ObjectCreator.Job1.PK);
			testARConsolInvoice4.AH_ConsolidatedInvoiceRef = ObjectCreator.Job1.JH_JobNum + "01";
			testARConsolInvoice4.AH_JH = ZGuid.Empty;
			Factory.Save();

			invoices = new InvoicingBaseCollection(Factory);
			invoices.Load(filter.Filter);

			Assert("Invoice Collection does not contain ARInvoice1", !BusinessObjectIsInCollectionByPK(invoices, ARInvoice1));
			Assert("Invoice Collection does not TestARInvoice4", !BusinessObjectIsInCollectionByPK(invoices, testARInvoice4));
			Assert("Invoice Collection does not contain TestARConsolInvoice4", !BusinessObjectIsInCollectionByPK(invoices, testARConsolInvoice4));
			AssertEquals("Invoice Collection Count", 2, invoices.Count);
		}

		public new void TestSystemLastEditTimeColumn()
		{
			Assert("The test is not suitable here.", true);
		}

		protected override CargoWise.Schema.SchemaDateTimeColumn ExpectedSystemLastEditTimeColumn
		{
			get { throw new NotImplementedException(); }
		}

		protected override TransactionExportFilter GetNewExportFilter()
		{
			return new JobRelatedARInvoicesExportFilter(Factory, FilterProvider, ObjectCreator.Job1.JH_JobNum, false);
		}
	}
}
