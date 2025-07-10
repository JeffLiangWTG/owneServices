using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using UniversalEventDataObject = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Accounting.ElectronicMessaging.TaxCore.Testing
{
	public class TaxCoreEInvoicingEventMessageProcessorTest : TestCaseWithFactoryAndMessagingHelpers
	{
		[TestDate(2019, 8, 1, 17, 51, 31)]
		public void TestProcessAcknowledgement_Success()
		{
			EventDataObject.EventParameters = new EventParameters
			{
				MessageType = CountryCode,
				MessageSubType = CountryResponseFactory.ResponseMessageSubTypeCode,
			};

			var processor = new TaxCoreEInvoicingEventMessageProcessor(logger, message, EventDataObject, Batch, CountryResponseFactory);

			AssertEquals("Precondition", 0, logger.Logs.Count());
			AssertEquals("Precondition", ZDateTime.Empty, Pivot.AIP_LastResponseReceivedUtc);
			AssertEquals("Precondition", EInvoicingPivotState.Sent, Pivot.AIP_Status);
			AssertEquals("Precondition", ZString.Empty, Pivot.AIP_ErrorDescription);
			AssertEquals("Precondition", 0, Invoice.DocManagerInfo.AllEDocs.Count);

			processor.Process();

			AssertEquals("Postcondition", 0, logger.Logs.Count());
			AssertEquals("Postcondition", ZDateTime.Now, Pivot.AIP_LastResponseReceivedUtc);
			AssertEquals("Postcondition", EInvoicingPivotState.Succeed, Pivot.AIP_Status);
			AssertEquals("Postcondition", ZString.Empty, Pivot.AIP_ErrorDescription);

			var authorisation = Factory.BOFactory.LoadTop1<AccTransactionHeaderAuthorisationRecord>(new ZQuery(AccTransactionHeaderAuthorisationRecordSchema.AHF_ParentId, Invoice.PK));
			AssertNotNull("Authorisation record should be created", authorisation);
			AssertEquals("One eDoc added", 1, Invoice.DocManagerInfo.AllEDocs.Count);
			AssertContains("Fiji Invoice Response_", Invoice.DocManagerInfo.AllEDocs[0].FileName);
			AssertContains(".json", Invoice.DocManagerInfo.AllEDocs[0].FileName);
			AssertEquals("Response should be stored in eDoc", MinimumJson, Invoice.DocManagerInfo.AllEDocs[0].ImageData.ToUTF8());
		}

		[TestDate(2019, 8, 1, 17, 51, 31)]
		public void TestProcessAcknowledgement_Failure()
		{
			const string malformedResponse = "You have received a non-JSON response from the server.";
			const string expectedWarning = """
				Reading Fiji E-Invoice Response ended with an exception:
				Exception Type: Enterprise.Accounting.ElectronicMessaging.Common.DeserializationException.
				""";

			EventDataObject.EventParameters = new EventParameters
			{
				MessageType = CountryCode,
				MessageSubType = CountryResponseFactory.ResponseMessageSubTypeCode,
			};

			EventDataObject.ContextCollection.First(c => c.Type.Type.ToString() == "ResponseMessage").Value = Convert.ToBase64String(Encoding.UTF8.GetBytes(malformedResponse));

			var processor = new TaxCoreEInvoicingEventMessageProcessor(logger, message, EventDataObject, Batch, CountryResponseFactory);

			AssertEquals("Precondition", 0, logger.Logs.Count());
			AssertEquals("Precondition", ZDateTime.Empty, Pivot.AIP_LastResponseReceivedUtc);
			AssertEquals("Precondition", EInvoicingPivotState.Sent, Pivot.AIP_Status);
			AssertEquals("Precondition", ZString.Empty, Pivot.AIP_ErrorDescription);
			AssertEquals("Precondition", 0, Invoice.DocManagerInfo.AllEDocs.Count);

			processor.Process();

			AssertEquals("Postcondition", 2, logger.Logs.Count());
			var log = logger.Logs.First();
			AssertEquals("Log type", LogType.Warning, log.Type);
			AssertContains(expectedWarning, log.Message);

			AssertEquals("Postcondition", ZDateTime.Now, Pivot.AIP_LastResponseReceivedUtc);
			AssertEquals("Postcondition", EInvoicingPivotState.Failed, Pivot.AIP_Status);
			AssertEquals("Postcondition", "Unreadable response from web service", Pivot.AIP_ErrorDescription);

			var authorisation = Factory.BOFactory.LoadTop1<AccTransactionHeaderAuthorisationRecord>(new ZQuery(AccTransactionHeaderAuthorisationRecordSchema.AHF_ParentId, Invoice.PK));
			AssertNull("Authorisation record should NOT be created", authorisation);
			AssertEquals("One eDoc added", 1, Invoice.DocManagerInfo.AllEDocs.Count);
			AssertContains("Fiji Invoice Response_", Invoice.DocManagerInfo.AllEDocs[0].FileName);
			AssertContains(".json", Invoice.DocManagerInfo.AllEDocs[0].FileName);
			AssertEquals("Response should be stored in eDoc", malformedResponse, Invoice.DocManagerInfo.AllEDocs[0].ImageData.ToUTF8());
		}

		public void TestProcessAcknowledgement_DoesNothingOnDiscardedBatch()
		{
			Batch.AIB_Status = EInvoicingBatchState.Discarded;
			Factory.SaveForTesting();

			EventDataObject.EventParameters = new EventParameters
			{
				MessageType = CountryCode,
				MessageSubType = CountryResponseFactory.ResponseMessageSubTypeCode
			};

			var countryFactory = TaxCoreEInvoicingResponseObjectFactory.GetICountryEInvoicingResponseObjectFactory(CountryCode);
			var processor = new TaxCoreEInvoicingEventMessageProcessor(logger, message, EventDataObject, Batch, countryFactory);

			AssertEquals("Precondition", 0, logger.Logs.Count());
			AssertEquals("Precondition", ZDateTime.Empty, Pivot.AIP_LastResponseReceivedUtc);
			AssertEquals("Precondition", EInvoicingPivotState.Sent, Pivot.AIP_Status);
			AssertEquals("Precondition", ZString.Empty, Pivot.AIP_ErrorDescription);
			AssertEquals("Precondition", 0, Invoice.DocManagerInfo.AllEDocs.Count);

			processor.Process();

			AssertEquals("Precondition", 0, logger.Logs.Count());
			AssertEquals("Precondition", ZDateTime.Empty, Pivot.AIP_LastResponseReceivedUtc);
			AssertEquals("Precondition", EInvoicingPivotState.Sent, Pivot.AIP_Status);
			AssertEquals("Precondition", ZString.Empty, Pivot.AIP_ErrorDescription);
			AssertEquals("Precondition", 0, Invoice.DocManagerInfo.AllEDocs.Count);
		}

		public void TestProcessAcknowledgement_NoUpdateIfNoPivot()
		{
			Pivot.Delete();
			Factory.SaveForTesting();

			EventDataObject.EventParameters = new EventParameters
			{
				MessageType = CountryCode,
				MessageSubType = CountryResponseFactory.ResponseMessageSubTypeCode,
			};

			var processor = new TaxCoreEInvoicingEventMessageProcessor(logger, message, EventDataObject, Batch, CountryResponseFactory);

			AssertEquals("Precondition", 0, logger.Logs.Count());
			ErrorReporter.Clear();

			processor.Process();

			AssertEquals("Precondition", 1, logger.Logs.Count());
			var log = logger.Logs.First();
			AssertEquals("Log type", LogType.Error, log.Type);
			var expectedError = "No update performed due to transaction pivot not found for invoice batch 1 in Eagle Datamation International.";
			AssertContains(expectedError, log.Message);
			AssertNotNull(ErrorReporter.LastMessageReported);
			AssertEquals(expectedError, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestProcessAcknowledgement_NoUpdateOnSucceededPivot()
		{
			Pivot.AIP_Status = EInvoicingPivotState.Succeed;
			Factory.SaveForTesting();

			var processor = new TaxCoreEInvoicingEventMessageProcessor(logger, message, EventDataObject, Batch, CountryResponseFactory);

			AssertEquals("Precondition", ZDateTime.Empty, Pivot.AIP_LastResponseReceivedUtc);
			AssertEquals("Precondition", EInvoicingPivotState.Succeed, Pivot.AIP_Status);
			AssertEquals("Precondition", ZString.Empty, Pivot.AIP_ErrorDescription);
			AssertEquals("Precondition", 0, logger.Logs.Count());

			processor.Process();

			AssertEquals("Postcondition", ZDateTime.Empty, Pivot.AIP_LastResponseReceivedUtc);
			AssertEquals("Postcondition", EInvoicingPivotState.Succeed, Pivot.AIP_Status);
			AssertEquals("Postcondition", ZString.Empty, Pivot.AIP_ErrorDescription);
			AssertEquals("Postcondition", 1, logger.Logs.Count());
			var log = logger.Logs.First();
			AssertEquals("Log type", LogType.Warning, log.Type);
			var expectedWarning = "No update performed due to transaction pivot having 'SUC' status for invoice batch 1 in Eagle Datamation International.";
			AssertContains(expectedWarning, log.Message);
			Assert(string.IsNullOrEmpty(ErrorReporter.LastMessageReported));
		}

		public void TestProcessAcknowledgement_NoUpdateIfEmptyMessageSubtype()
		{
			EventDataObject.EventParameters = new EventParameters
			{
				MessageType = CountryCode,
				MessageSubType = "",
			};

			var processor = new TaxCoreEInvoicingEventMessageProcessor(logger, message, EventDataObject, Batch, CountryResponseFactory);

			AssertEquals("Precondition", ZDateTime.Empty, Pivot.AIP_LastResponseReceivedUtc);
			AssertEquals("Precondition", EInvoicingPivotState.Sent, Pivot.AIP_Status);
			AssertEquals("Precondition", ZString.Empty, Pivot.AIP_ErrorDescription);
			AssertEquals("Precondition", 0, logger.Logs.Count());
			ErrorReporter.Clear();

			processor.Process();

			AssertEquals("Postcondition", ZDateTime.Empty, Pivot.AIP_LastResponseReceivedUtc);
			AssertEquals("Postcondition", EInvoicingPivotState.Sent, Pivot.AIP_Status);
			AssertEquals("Postcondition", ZString.Empty, Pivot.AIP_ErrorDescription);
			AssertEquals("Postcondition", 1, logger.Logs.Count());
			var log = logger.Logs.First();
			AssertEquals("Log type", LogType.Error, log.Type);
			var expectedError = "No update performed due to universal event not containing message sub type for invoice batch 1 in Eagle Datamation International.";
			AssertContains(expectedError, log.Message);
			AssertEquals(expectedError, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestProcessAcknowledgement_NoUpdateIfInvalidMessageSubtype()
		{
			EventDataObject.EventParameters = new EventParameters
			{
				MessageType = CountryCode,
				MessageSubType = "Invalid Subtype",
			};

			var processor = new TaxCoreEInvoicingEventMessageProcessor(logger, message, EventDataObject, Batch, CountryResponseFactory);

			AssertEquals("Precondition", ZDateTime.Empty, Pivot.AIP_LastResponseReceivedUtc);
			AssertEquals("Precondition", EInvoicingPivotState.Sent, Pivot.AIP_Status);
			AssertEquals("Precondition", ZString.Empty, Pivot.AIP_ErrorDescription);
			AssertEquals("Precondition", 0, logger.Logs.Count());
			ErrorReporter.Clear();

			processor.Process();

			AssertEquals("Postcondition", ZDateTime.Empty, Pivot.AIP_LastResponseReceivedUtc);
			AssertEquals("Postcondition", EInvoicingPivotState.Sent, Pivot.AIP_Status);
			AssertEquals("Postcondition", ZString.Empty, Pivot.AIP_ErrorDescription);
			AssertEquals("Postcondition", 1, logger.Logs.Count());
			var log = logger.Logs.First();
			AssertEquals("Log type", LogType.Error, log.Type);
			var expectedError = "No update performed due to invalid message sub type [Invalid Subtype] found for invoice batch 1 in Eagle Datamation International.";
			AssertContains(expectedError, log.Message);
			AssertEquals(expectedError, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		[TestDate(2019, 8, 1, 17, 51, 31)]
		public void TestProcessRejection()
		{
			var helper = new EInvoicingTestHelper(new TestObjectCreator(new BusinessObjectFactory()));

			EventDataObject.EventType = AutoEvents.InterchangeRejectedCode;
			EventDataObject.EventParameters = new EventParameters
			{
				MessageType = CountryCode,
				Reason = "This is the rejection reason from eHub."
			};

			var processor = new TaxCoreEInvoicingEventMessageProcessor(logger, message, EventDataObject, Batch, CountryResponseFactory);

			AssertEquals("Precondition", 0, logger.Logs.Count());
			AssertEquals("Precondition", ZDateTime.Empty, Pivot.AIP_LastResponseReceivedUtc);
			AssertEquals("Precondition", EInvoicingPivotState.Sent, Pivot.AIP_Status);
			AssertEquals("Precondition", ZString.Empty, Pivot.AIP_ErrorDescription);

			var notificationGroupPK = helper.CreateNotificationGroup("Test User 2", "company2user@abc.com");
			using (AccountingConfigurationRegistry.Instance.EInvoicingErrorNotificationGroup.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, notificationGroupPK.ToGuid()))
			{
				processor.Process();
			}

			AssertEquals("Postcondition", 1, logger.Logs.Count());
			var log = logger.Logs.First();
			AssertEquals("Log type", LogType.Information, log.Type);
			var expectedLog = """
				Email Notification was sent successfully for Eagle Datamation International.
				E-Reporting Email Notification task completed.
				""";
			AssertContains(expectedLog, log.Message);
			AssertEquals("Postcondition", ZDateTime.Now, Pivot.AIP_LastResponseReceivedUtc);
			AssertEquals("Postcondition", EInvoicingPivotState.Failed, Pivot.AIP_Status);
			AssertEquals("Postcondition", "This is the rejection reason from eHub.", Pivot.AIP_ErrorDescription);

			var authorisation = Factory.LoadTop1<AccTransactionHeaderAuthorisationRecord>(new ZQuery(AccTransactionHeaderAuthorisationRecordSchema.AHF_ParentId, Invoice.PK));
			AssertNull("Authorisation record should not be created", authorisation);
		}

		ZString CountryCode => CountryCodes.Fiji;

		ITaxCoreCountryEInvoicingResponseObjectFactory CountryResponseFactory => TaxCoreEInvoicingResponseObjectFactory.GetICountryEInvoicingResponseObjectFactory(CountryCode);

		#region Implementation

		TestObjectCreator ObjectCreator;
		InvoicingBase Invoice;
		AccEInvoicingBatch Batch;
		AccEInvoicingTransactionPivot Pivot;
		UniversalEventDataObject EventDataObject;
		IEDIMessage message;
		IXmlSessionTracker logger;

		const string MinimumJson = """
			{"RequestedBy":"","SignedBy":"","DT":"2019-07-16T15:26:00Z","IC":"0","InvoiceCounterExtension":"","IN":"12345678798","VerificationUrl":"","TotalCounter":0,"TransactionTypeCounter":0,"TotalAmount":0,"ID":"","S":"", "TaxItems":[]}
			""";

		protected override void SetUp()
		{
			base.SetUp();
			ObjectCreator = new TestObjectCreator(Factory.BOFactory);
			Invoice = ObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", ObjectCreator.TWD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			Batch = ObjectCreator.CreateEInvoicingBatch(1, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
			Pivot = ObjectCreator.CreateEInvoicingTransactionPivot(Batch, Invoice, EInvoicingPivotState.Sent);
			Factory.SaveForTesting();

			EventDataObject = new UniversalEventDataObject();
			EventDataObject.DataContext = new UniversalDataBuss.DataObjects.Universal._2012_11.DataContext();
			EventDataObject.DataContext.AddDataTarget(DataContextType.AccEInvoicingBatch, string.Format("{0}", Batch.AIB_BatchNumber));
			EventDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			EventDataObject.EventType = AutoEvents.InterchangeAcknowledgedCode;
			EventDataObject.EventTime = ZDateTimeOffset.Now;

			EventDataObject.ContextCollection = new List<Context>();

			var companyCodeContextType = new ContextType() { Type = "CompanyCode" };
			EventDataObject.ContextCollection.Add(new Context() { Type = companyCodeContextType, Value = GlbCompany.CurrentCompany.GC_Code });
			EventDataObject.ContextCollection.Add(new Context() { Type = "ResponseMessage", Value = Convert.ToBase64String(Encoding.UTF8.GetBytes(MinimumJson)) });

			message = GetQueuedUniversalEventMessage(EventDataObject, UniversalXmlInfo.Namespace_2012_11);
			logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
		}

		#endregion
	}
}
