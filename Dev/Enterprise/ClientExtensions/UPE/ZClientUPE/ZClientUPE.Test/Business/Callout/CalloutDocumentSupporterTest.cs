using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(CalloutDocumentSupporter))]
	sealed class CalloutDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestCalloutProperty()
		{
			var cusHAWB = Factory.New<UPECusHAWB>();
			var documentSupporter = new CalloutDocumentSupporter(cusHAWB);
			AssertEquals("Should re-Factory.Load the Callout object when we pass a CusHAWB into the constructor", typeof(Callout), documentSupporter.Callout.GetType());
		}

		public void TestGetDocBusinessObjects()
		{
			var callout = Factory.NewWithValidTestData<TestCallout>();
			var documentSupporter = (CalloutDocumentSupporter)callout.DocumentSupporter;
			var wrappers = documentSupporter.GetDocumentWrappers(Core.Constants.DataContext.CusHAWB, null);
			AssertEquals("Should return 1 Callout doc wrapper", 1, wrappers.Length);
			AssertEquals("Should return 1 Callout doc wrapper", true, wrappers[0] is DocCallout);
		}

		public void TestGetContactOrganisation()
		{
			var billTo = Factory.New<OrgHeader>();
			var billToAccountNumber = billTo.CustomsCodes.AddNew();
			var callout = Factory.New<TestCallout>();
			var documentSupporter = (CalloutDocumentSupporter)callout.DocumentSupporter;
			var deliveryContact = documentSupporter.GetContactOrganisation("", null, DocumentDirection.ARV);
			callout.BillToAccountNumber = "BillToAccountNumber";
			billToAccountNumber.OK_CustomsRegNo = callout.BillToAccountNumber;
			AssertEquals("BillTo should be used as the delivery organisation", callout.BillTo, deliveryContact.OrgHeader);
		}

		public void TestQueueInvoiceForBatchPrintAndSave_OnDocumentMenuItemClicked()
		{
			TestCaseWithClientSpecificDocuments.LoadUPESpecificDocuments();
			var callout = Factory.NewWithValidTestData<TestCallout>();
			var documentSupporter = (CalloutDocumentSupporter)callout.DocumentSupporter;
			var eventSource = new MockDocumentEventSource();
			documentSupporter.Initialise(eventSource);
			var e = new DocumentCancelEventArgs(new UPEDocumentMenuItemLoader(Factory).LoadUPSTaxInvoiceQueueForBatchPrint());
			eventSource.FireDocumentPrintRequested(e);
			var taxInvoicePrintItem = new UPEPrintBatchItem.Loader(Factory).LoadPrintBatchItem(UPEPrintBatchTypes.Codes.TaxInvoice, callout.PK, new UPEDocumentMenuItemLoader(Factory).LoadUPSTaxInvoice().PK);
			AssertNotNull("Tax invoice from the callout item should be queued for batch print", taxInvoicePrintItem);
			AssertEquals("The document delivery form should be cancelled because we're not delivering it with doc engine now", true, e.Cancel);
		}

		public void TestQueueTaxAndCommercialInvoiceForBatchPrintAndSave_OnDocumentMenuItemClicked()
		{
			TestCaseWithClientSpecificDocuments.LoadUPESpecificDocuments();
			var callout = Factory.NewWithValidTestData<TestCallout>();
			var documentSupporter = (CalloutDocumentSupporter)callout.DocumentSupporter;
			var eventSource = new MockDocumentEventSource();
			documentSupporter.Initialise(eventSource);
			var e = new DocumentCancelEventArgs(new UPEDocumentMenuItemLoader(Factory).LoadUPSTaxInvoiceAndCommercialInvoiceQueueForBatchPrint());
			eventSource.FireDocumentPrintRequested(e);
			var taxInvoiceAndCommercialInvoicePrintItem = new UPEPrintBatchItem.Loader(Factory).LoadPrintBatchItem(UPEPrintBatchTypes.Codes.TaxInvoice, callout.PK, new UPEDocumentMenuItemLoader(Factory).LoadUPSTaxInvoiceAndCommercialInvoice().PK);
			AssertNotNull("Tax invoice from the callout item should be queued for batch print", taxInvoiceAndCommercialInvoicePrintItem);
			AssertEquals("The document delivery form should be cancelled because we're not delivering it with doc engine now", true, e.Cancel);
		}

		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject() => Factory.New<Callout>();

		protected override bool ExcludeDocumentCommandTest(Integration.DocumentEngine.IDocumentCommand documentCommand) => documentCommand.SU_MenuName == "Tax Invoice - Queue for Print" || documentCommand.SU_MenuName == "Tax + Commercial Inv. - Queue for Print";

		sealed class MockDocumentEventSource : IDocumentEvents
		{
			public event DocumentCancelEventHandler DocumentPrintRequested;
			public event DocumentPrintedEventHandler DocumentPrePreviewed;
			public event DocumentPrintedEventHandler DocumentPrePrinted;
			public event DocumentPrintedEventHandler DocumentPrinted;
			public void FireDocumentPrintRequested(DocumentCancelEventArgs e)
			{
				DocumentPrintRequested(this, e);
			}

			public void FireDocumentPrePreviewed(DocumentPrintedEventArgs e)
			{
				DocumentPrePreviewed(this, e);
			}

			public void FireDocumentPrePrinted(DocumentPrintedEventArgs e)
			{
				DocumentPrePrinted(this, e);
			}

			public void FireDocumentPrinted(DocumentPrintedEventArgs e)
			{
				DocumentPrinted(this, e);
			}
		}

		sealed class TestCallout : Callout
		{
			public TestCallout(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			internal new CusHAWBDocumentSupporter DocumentSupporter => base.DocumentSupporter;
		}
	}
}
