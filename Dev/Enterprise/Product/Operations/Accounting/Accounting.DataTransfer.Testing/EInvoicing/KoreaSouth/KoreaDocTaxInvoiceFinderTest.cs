using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.DataTransfer.EInvoicing.KoreaSouth;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;

namespace Enterprise.Accounting.DataTransfer.eInvoicing.KoreaSouth.Testing
{
	class KoreaDocTaxInvoiceFinderTest : TestCaseWithFactory
	{
		public void TestConstructor_WhenAccEInvoicingBatchIsNull()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new KoreaDocTaxInvoiceFinder(null));
		}

		public void TestGetXml_ReturnNull_WhenMissingLog()
		{
			var invoice = PrepareInvoice();

			var finder = new KoreaDocTaxInvoiceFinder(invoice.EInvoicingProxy?.MostRecentPivot?.Batch);
			finder.GetXml("TEST001");

			AssertNullOrEmpty(finder.XMLStr);
			AssertNull(finder.XMLDocument);
		}

		public void TestGetXml_FromInterchangeAcknowledgedLog()
		{
			AssertGetXml(AutoEvents.InterchangeAcknowledged, EInvoicingKoreaSouthConstants.StatusCodes.SubmitSuccess);
		}

		public void TestGetXml_FromInterchangeRejectedLog()
		{
			AssertGetXml(AutoEvents.InterchangeRejected, EInvoicingKoreaSouthConstants.StatusCodes.SubmitFail);
		}

		public void TestGetXml_FromTheLatestLog_WhenExistMultipleLogs()
		{
			var xml1 = @"<TaxInvoice>
	<TaxInvoiceDocument>
		<IssueID>TEST001</IssueID>
		<IssueDateTime>20240729</IssueDateTime>
	</TaxInvoiceDocument>
</TaxInvoice>";

			var xml2 = @"<TaxInvoice>
	<TaxInvoiceDocument>
		<IssueID>TEST001</IssueID>
		<IssueDateTime>20240730</IssueDateTime>
	</TaxInvoiceDocument>
</TaxInvoice>";

			var invoice = PrepareInvoice();

			var universalEvent1 = GetUniversalEvent(new[] { xml1 }, EInvoicingKoreaSouthConstants.StatusCodes.SubmitSuccess, AutoEvents.InterchangeAcknowledged);
			TestObjectCreator.CreateAndLinkUniversalEvent(invoice.EInvoicingProxy?.MostRecentPivot?.Batch, universalEvent1, AutoEvents.InterchangeAcknowledged);

			var universalEvent2 = GetUniversalEvent(new[] { xml2 }, EInvoicingKoreaSouthConstants.StatusCodes.SubmitFail, AutoEvents.InterchangeRejected);
			TestObjectCreator.CreateAndLinkUniversalEvent(invoice.EInvoicingProxy?.MostRecentPivot?.Batch, universalEvent2, AutoEvents.InterchangeRejected);

			var universalEvent3 = GetUniversalEvent(null, EInvoicingKoreaSouthConstants.StatusCodes.SubmitFail, AutoEvents.InterchangeRejected);
			TestObjectCreator.CreateAndLinkUniversalEvent(invoice.EInvoicingProxy?.MostRecentPivot?.Batch, universalEvent3, AutoEvents.InterchangeRejected);

			var finder = new KoreaDocTaxInvoiceFinder(invoice.EInvoicingProxy?.MostRecentPivot?.Batch);

			finder.GetXml("TEST001");
			AssertEquals(xml2, finder.XMLStr);
			AssertNotNull(finder.XMLDocument);
		}

		void AssertGetXml(Event eventType, string statusCode)
		{
			var xml1 = @"<TaxInvoice>
	<TaxInvoiceDocument>
		<IssueID>TEST001</IssueID>
	</TaxInvoiceDocument>
</TaxInvoice>";

			var xml2 = @"<TaxInvoice>
	<TaxInvoiceDocument>
		<IssueID>TEST002</IssueID>
	</TaxInvoiceDocument>
</TaxInvoice>";

			var universalEvent = GetUniversalEvent(new[] { xml1, xml2 }, statusCode, eventType);
			var invoice = PrepareInvoice(eventType, universalEvent);

			var finder = new KoreaDocTaxInvoiceFinder(invoice.EInvoicingProxy?.MostRecentPivot?.Batch);

			finder.GetXml("TEST001");
			AssertEquals(xml1, finder.XMLStr);
			AssertNotNull(finder.XMLDocument);

			finder.GetXml("TEST002");
			AssertEquals(xml2, finder.XMLStr);
			AssertNotNull(finder.XMLDocument);

			finder.GetXml("TEST003");
			AssertNullOrEmpty(finder.XMLStr);
			AssertNull(finder.XMLDocument);
		}

		InvoicingBase PrepareInvoice(Event eventType = null, string universalEvent = "")
		{
			var invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.KRW, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			var batch = TestObjectCreator.CreateEInvoicingBatch(1, Core.Constants.EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
			var pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(batch, invoice, Core.Constants.EInvoicingPivotState.Sent);

			var referenceKRI = Factory.New<AccTransactionHeaderReference>();
			referenceKRI.AH1_AH = invoice.PK;
			referenceKRI.AH1_Type = AccTransactionHeaderReferenceTypes.KRI;
			referenceKRI.AH1_Reference = "TEST001";

			if (eventType != null && !universalEvent.IsNullOrEmpty())
			{
				TestObjectCreator.CreateAndLinkUniversalEvent(batch, universalEvent, eventType);
			}

			return invoice;
		}

		string GetUniversalEvent(IEnumerable<string> taxInvoiceXMLs, string koreaStatusCode, Event eventType)
		{
			return $@"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>AccEInvoicingBatch</Type>
					<Key>1</Key>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2018-01-09T09:30:10</EventTime>
		<EventType>{eventType.Code}</EventType>
		<EventParameters>
			<MessageType>KR</MessageType>
			<MessageSubType>GEN</MessageSubType>
		</EventParameters>
		<ContextCollection>
			<Context>
				<Type>{EInvoicingKoreaSouthConstants.DataContext.KoreaStatusCode}</Type>
				<Value>{koreaStatusCode}</Value>
			</Context>
			<Context>
				<Type>{EInvoicingKoreaSouthConstants.DataContext.KoreaReceiptID}</Type>
				<Value>NTS-20240617110351-27529</Value>
			</Context>
{GetContextOfKoreaDocTaxInvoice()}
		</ContextCollection>
	</Event>
</UniversalEvent>";

			string GetContextOfKoreaDocTaxInvoice()
			{
				return taxInvoiceXMLs == null
					? ""
					: string.Join(System.Environment.NewLine, taxInvoiceXMLs.Select(xml => @$"
			<Context>
				<Type>EINV_DocTaxInvoice</Type>
				<Value>{Convert.ToBase64String(MessageEncoding.UTF8WithoutBOM.GetBytes(xml))}</Value>
			</Context>"))
			+
			@$"
			<Context>
				<Type>EINV_DocTaxInvoiceCount</Type>
				<Value>{taxInvoiceXMLs.Count()}</Value>
			</Context>";
			}
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
