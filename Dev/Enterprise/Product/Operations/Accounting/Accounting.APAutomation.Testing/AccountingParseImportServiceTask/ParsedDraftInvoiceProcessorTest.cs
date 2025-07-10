using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.APAutomation.AccountingParseImportServiceTask;
using Enterprise.Accounting.Business;
using Enterprise.Dash.Business;
using Enterprise.Dash.Business.Services;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using Moq;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.APAutomation.Testing.AccountingParseImportServiceTask
{
	public class ParsedDraftInvoiceProcessorTest : TestCaseWithFactory
	{
		public void TestNoMessagesToProcess()
		{
			var loggerMock = new Mock<ILogger>();
			var dashEntitiesServiceMock = new Mock<IDashEntitiesService>();
			var cancellationTokenSource = new CancellationTokenSource();

			var processor = new ParsedDraftInvoiceProcessor(loggerMock.Object, dashEntitiesServiceMock.Object);
			processor.Run(cancellationTokenSource.Token);

			AssertNothingToProcess(loggerMock);
			loggerMock.VerifyNoOtherCalls();
		}

		public void TestNoDashDocumentLinkedToMessage()
		{
			var message = CreateEdiMessage(dashDocument: null);
			Factory.Save();

			var loggerMock = new Mock<ILogger>();
			var dashEntitiesServiceMock = new Mock<IDashEntitiesService>();
			var cancellationTokenSource = new CancellationTokenSource();

			var processor = new ParsedDraftInvoiceProcessor(loggerMock.Object, dashEntitiesServiceMock.Object);
			processor.Run(cancellationTokenSource.Token);

			AssertEquals(1, ErrorReporter.TotalErrorCount);
			AssertEquals(ErrorReporter.LastMessageReported, $"The EDI Message {message.PK} is not linked to a Dash AP Invoice.");
			ErrorReporter.Clear();

			loggerMock.VerifyNoOtherCalls();

			AssertEquals(EDIMessageStatusList.Codes.Error, message.EM_Status);
		}

		public void TestNoDraftInvoiceLinkedToDashDocument()
		{
			var dashInvoice = CreateDashInvoice();
			var dashDoc = CreateDashDocument(draftInvoice: null, dashInvoice);
			dashInvoice.DPI_DDD_DashDocID = dashDoc.PK;
			var message = CreateEdiMessage(dashDoc);
			Factory.Save();

			var loggerMock = new Mock<ILogger>();
			var dashEntitiesServiceMock = new Mock<IDashEntitiesService>();
			dashEntitiesServiceMock
				.Setup(m => m.LoadAPInvoices(
					new[] { dashDoc.PK },
					It.IsAny<BusinessObjectFactory>()))
				.Returns(new[] { dashInvoice });
			var cancellationTokenSource = new CancellationTokenSource();

			var processor = new ParsedDraftInvoiceProcessor(loggerMock.Object, dashEntitiesServiceMock.Object);
			processor.Run(cancellationTokenSource.Token);

			AssertEquals(1, ErrorReporter.TotalErrorCount);
			AssertEquals(ErrorReporter.LastMessageReported, $"The Dash Document {dashDoc.PK} is not linked to a Draft Invoice.");
			ErrorReporter.Clear();

			loggerMock.VerifyNoOtherCalls();

			AssertEquals(EDIMessageStatusList.Codes.Error, message.EM_Status);
		}

		public void TestDraftInvoiceDiscarded()
		{
			const string parsedInvoiceNumber = "INV-1234";
			const string internalReferenceNumber = "00001000";
			var draftInvoice = objectCreator.CreateUploadedDraftInvoice("AUD");
			draftInvoice.AIH_InternalReference = internalReferenceNumber;
			draftInvoice.AIH_Status = AccDraftInvoiceHeaderStatus.Discarded;
			var dashInvoice = CreateDashInvoice(invoiceNumber: parsedInvoiceNumber);
			var dashDoc = CreateDashDocument(draftInvoice, dashInvoice);
			dashInvoice.DPI_DDD_DashDocID = dashDoc.PK;
			var message = CreateEdiMessage(dashDoc);
			Factory.Save();

			AssertEquals("Pre-condition: Status is DSC", AccDraftInvoiceHeaderStatus.Discarded, draftInvoice.AIH_Status);
			AssertEquals("Pre-condition: Transaction Number is blank", ZString.Empty, draftInvoice.AIH_TransactionNumber);

			var loggerMock = new Mock<ILogger>();
			var dashEntitiesServiceMock = new Mock<IDashEntitiesService>();
			dashEntitiesServiceMock
				.Setup(m => m.LoadAPInvoices(
					new[] { dashDoc.PK },
					It.IsAny<BusinessObjectFactory>()))
				.Returns(new[] { dashInvoice });
			dashEntitiesServiceMock
				.Setup(m => m.UpdateStatusToComplete(dashDoc, false))
				.Returns(true);
			var cancellationTokenSource = new CancellationTokenSource();

			var processor = new ParsedDraftInvoiceProcessor(loggerMock.Object, dashEntitiesServiceMock.Object);
			processor.Run(cancellationTokenSource.Token);

			AssertLoggedMessage(loggerMock, LogType.Information, "Found 1 draft invoice(s) to process");
			AssertLoggedMessage(loggerMock, LogType.Debug,		$"Processing EDIMessage (PK): {message.PK}, Dash Invoice (PK): {dashInvoice.PK}, Draft Invoice (PK): {draftInvoice.PK}");
			AssertLoggedMessage(loggerMock, LogType.Debug,		$"Draft invoice (internal reference: {internalReferenceNumber}) was not updated as the status was set to DSC");
			AssertLoggedMessage(loggerMock, LogType.Debug,		$"Processing draft invoice (internal reference: {internalReferenceNumber}) was successful");
			AssertLoggedMessage(loggerMock, LogType.Information, "Processing finished after 1/1 draft invoice(s) completed successfully");
			AssertNothingToProcess(loggerMock);
			loggerMock.VerifyNoOtherCalls();

			AssertEquals(AccDraftInvoiceHeaderStatus.Discarded, draftInvoice.AIH_Status);
			AssertEquals("Transaction Number should not be imported", ZString.Empty, draftInvoice.AIH_TransactionNumber);
			AssertTriedToUpdateDashDocumentToComplete("The dash doc should be complete", dashEntitiesServiceMock, dashDoc, Times.Once());
			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
			Assert("The draft invoice should NOT be queued for auto-rec & posting", !IsQueuedForAutoRecAndPosting(draftInvoice));
		}

		public void TestProcessingCancelled()
		{
			const string internalReferenceNumber = "00001000";
			var item1 = CreateBatchItem();
			item1.DraftInvoice.AIH_InternalReference = internalReferenceNumber;
			var item2 = CreateBatchItem();
			Factory.Save();

			var loggerMock = new Mock<ILogger>();
			var dashEntitiesServiceMock = new Mock<IDashEntitiesService>();
			dashEntitiesServiceMock
				.Setup(m => m.LoadAPInvoices(
					It.IsAny<IEnumerable<ZGuid>>(),
					It.IsAny<BusinessObjectFactory>()))
				.Returns(new[] {
					item1.DashInvoice,
					item2.DashInvoice });
			var cancellationTokenSource = new CancellationTokenSource();

			//Force the cancellation to happen while the first item is being processed.
			dashEntitiesServiceMock.Setup(svc => svc.UpdateStatusToComplete(item1.DashDoc, false))
				.Returns(() =>
				{
					cancellationTokenSource.Cancel();
					return true;
				});

			var processor = new ParsedDraftInvoiceProcessor(loggerMock.Object, dashEntitiesServiceMock.Object, batchSize: 2);
			processor.Run(cancellationTokenSource.Token);

			AssertLoggedMessage(loggerMock, LogType.Information, "Found 2 draft invoice(s) to process");
			AssertLoggedMessage(loggerMock, LogType.Debug,		$"Processing EDIMessage (PK): {item1.Message.PK}, Dash Invoice (PK): {item1.DashInvoice.PK}, Draft Invoice (PK): {item1.DraftInvoice.PK}");
			AssertLoggedMessage(loggerMock, LogType.Debug,		$"Processing draft invoice (internal reference: {internalReferenceNumber}) was successful");
			AssertLoggedMessage(loggerMock, LogType.Debug,		 "Cancellation requested");
			AssertLoggedMessage(loggerMock, LogType.Information, "Processing finished after 1/2 draft invoice(s) completed successfully");
			loggerMock.VerifyNoOtherCalls();

			CombineAssertions(() =>
			{
				AssertEquals("The first message should be processed", EDIMessageStatusList.Codes.ProcessedOK, item1.Message.EM_Status);
				AssertEquals("The first draft invoice should still be 'Analyzing'", AccDraftInvoiceHeaderStatus.Analyzing, item1.DraftInvoice.AIH_Status);
				AssertTriedToUpdateDashDocumentToComplete("The first dash doc should be complete", dashEntitiesServiceMock, item1.DashDoc, Times.Once());
				Assert("The first draft invoice should be sent for auto-rec & posting", IsQueuedForAutoRecAndPosting(item1.DraftInvoice));
				AssertEquals("The second message should NOT be processed", EDIMessageStatusList.Codes.Queued, item2.Message.EM_Status);
				AssertEquals("The second draft invoice should still be 'Analyzing'", AccDraftInvoiceHeaderStatus.Analyzing, item2.DraftInvoice.AIH_Status);
				AssertTriedToUpdateDashDocumentToComplete("The second dash doc should NOT be complete", dashEntitiesServiceMock, item2.DashDoc, Times.Never());
				Assert("The second draft invoice should NOT be sent for auto-rec & posting", !IsQueuedForAutoRecAndPosting(item2.DraftInvoice));
			});
		}

		public void TestUpdateDashDocumentStatusToCompleteFailed()
		{
			const string internalReferenceNumber = "00001000";
			var draftInvoice = objectCreator.CreateUploadedDraftInvoice("AUD");
			draftInvoice.AIH_InternalReference = internalReferenceNumber;
			var dashInvoice = CreateDashInvoice();
			var dashDoc = CreateDashDocument(draftInvoice, dashInvoice);
			dashInvoice.DPI_DDD_DashDocID = dashDoc.PK;
			var message = CreateEdiMessage(dashDoc);
			Factory.Save();

			var loggerMock = new Mock<ILogger>();
			var dashEntitiesServiceMock = new Mock<IDashEntitiesService>();
			dashEntitiesServiceMock
				.Setup(m => m.LoadAPInvoices(
					new[] { dashDoc.PK },
					It.IsAny<BusinessObjectFactory>()))
				.Returns(new[] { dashInvoice });
			dashEntitiesServiceMock
				.Setup(m => m.UpdateStatusToComplete(dashDoc, false))
				.Returns(false);
			var cancellationTokenSource = new CancellationTokenSource();

			var processor = new ParsedDraftInvoiceProcessor(loggerMock.Object, dashEntitiesServiceMock.Object);
			processor.Run(cancellationTokenSource.Token);

			AssertLoggedMessage(loggerMock, LogType.Information, "Found 1 draft invoice(s) to process");
			AssertLoggedMessage(loggerMock, LogType.Debug,		$"Processing EDIMessage (PK): {message.PK}, Dash Invoice (PK): {dashInvoice.PK}, Draft Invoice (PK): {draftInvoice.PK}");
			AssertLoggedMessage(loggerMock, LogType.Debug,		$"Dash doc: {dashDoc.PK} failed to update status to complete");
			AssertLoggedMessage(loggerMock, LogType.Debug,		$"Processing draft invoice (internal reference: {internalReferenceNumber}) was successful");
			AssertLoggedMessage(loggerMock, LogType.Information, "Processing finished after 1/1 draft invoice(s) completed successfully");
			AssertNothingToProcess(loggerMock);
			loggerMock.VerifyNoOtherCalls();

			AssertEquals(AccDraftInvoiceHeaderStatus.Analyzing, draftInvoice.AIH_Status);
			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
			AssertTriedToUpdateDashDocumentToComplete("The dash doc should be complete", dashEntitiesServiceMock, dashDoc, Times.Once());
			Assert("The draft invoice should be queued for auto-rec & posting", IsQueuedForAutoRecAndPosting(draftInvoice));
		}

		public void TestProcessSingleMessage()
		{
			const string parsedInvoiceNumber = "INV-1234";
			const string internalReferenceNumber = "00001000";
			var draftInvoice = objectCreator.CreateUploadedDraftInvoice("AUD");
			draftInvoice.AIH_InternalReference = internalReferenceNumber;
			var dashInvoice = CreateDashInvoice(invoiceNumber: parsedInvoiceNumber);
			var dashDoc = CreateDashDocument(draftInvoice, dashInvoice);
			dashInvoice.DPI_DDD_DashDocID = dashDoc.PK;
			var message = CreateEdiMessage(dashDoc);
			Factory.Save();

			AssertEquals("Pre-condition: Status is 'Analyzing'", AccDraftInvoiceHeaderStatus.Analyzing, draftInvoice.AIH_Status);
			AssertEquals("Pre-condition: Transaction Number is blank", ZString.Empty, draftInvoice.AIH_TransactionNumber);

			var loggerMock = new Mock<ILogger>();
			var dashEntitiesServiceMock = new Mock<IDashEntitiesService>();
			dashEntitiesServiceMock
				.Setup(m => m.LoadAPInvoices(
					new[] { dashDoc.PK },
					It.IsAny<BusinessObjectFactory>()))
				.Returns(new[] { dashInvoice });
			dashEntitiesServiceMock
				.Setup(m => m.UpdateStatusToComplete(dashDoc, false))
				.Returns(true);
			var cancellationTokenSource = new CancellationTokenSource();

			var processor = new ParsedDraftInvoiceProcessor(loggerMock.Object, dashEntitiesServiceMock.Object);
			processor.Run(cancellationTokenSource.Token);

			AssertLoggedMessage(loggerMock, LogType.Information, "Found 1 draft invoice(s) to process");
			AssertLoggedMessage(loggerMock, LogType.Debug,		 $"Processing EDIMessage (PK): {message.PK}, Dash Invoice (PK): {dashInvoice.PK}, Draft Invoice (PK): {draftInvoice.PK}");
			AssertLoggedMessage(loggerMock, LogType.Debug,		$"Processing draft invoice (internal reference: {internalReferenceNumber}) was successful");
			AssertLoggedMessage(loggerMock, LogType.Information, "Processing finished after 1/1 draft invoice(s) completed successfully");
			AssertNothingToProcess(loggerMock);
			loggerMock.VerifyNoOtherCalls();

			AssertEquals(AccDraftInvoiceHeaderStatus.Analyzing, draftInvoice.AIH_Status);
			AssertEquals(parsedInvoiceNumber, draftInvoice.AIH_TransactionNumber);
			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
			AssertTriedToUpdateDashDocumentToComplete("The dash doc should be complete", dashEntitiesServiceMock, dashDoc, Times.Once());
			Assert("The draft invoice should be queued for auto-rec & posting", IsQueuedForAutoRecAndPosting(draftInvoice));
		}

		public void TestProcessMessageInBatches()
		{
			const string internalReferenceNumber1 = "00001001";
			var item1 = CreateBatchItem();
			item1.DraftInvoice.AIH_InternalReference = internalReferenceNumber1;
			const string internalReferenceNumber2 = "00001002";
			var item2 = CreateBatchItem();
			item2.DraftInvoice.AIH_InternalReference = internalReferenceNumber2;
			const string internalReferenceNumber3 = "00001003";
			var item3 = CreateBatchItem();
			item3.DraftInvoice.AIH_InternalReference = internalReferenceNumber3;
			Factory.Save();

			var loggerMock = new Mock<ILogger>();
			var dashEntitiesServiceMock = new Mock<IDashEntitiesService>();
			dashEntitiesServiceMock
				.Setup(m => m.LoadAPInvoices(
					It.IsAny<IEnumerable<ZGuid>>(),
					It.IsAny<BusinessObjectFactory>()))
				.Returns(new [] {
					item1.DashInvoice,
					item2.DashInvoice,
					item3.DashInvoice });
			dashEntitiesServiceMock
				.Setup(m => m.UpdateStatusToComplete(It.IsAny<DashDocument>(), false))
				.Returns(true);
			var cancellationTokenSource = new CancellationTokenSource();

			var processor = new ParsedDraftInvoiceProcessor(loggerMock.Object, dashEntitiesServiceMock.Object, batchSize: 2);
			processor.Run(cancellationTokenSource.Token);

			AssertLoggedMessage(loggerMock, LogType.Information, "Found 2 draft invoice(s) to process");
			AssertLoggedMessage(loggerMock, LogType.Debug,		$"Processing EDIMessage (PK): {item1.Message.PK}, Dash Invoice (PK): {item1.DashInvoice.PK}, Draft Invoice (PK): {item1.DraftInvoice.PK}");
			AssertLoggedMessage(loggerMock, LogType.Debug,		$"Processing draft invoice (internal reference: {internalReferenceNumber1}) was successful");
			AssertLoggedMessage(loggerMock, LogType.Debug,		$"Processing EDIMessage (PK): {item2.Message.PK}, Dash Invoice (PK): {item2.DashInvoice.PK}, Draft Invoice (PK): {item2.DraftInvoice.PK}");
			AssertLoggedMessage(loggerMock, LogType.Debug,		$"Processing draft invoice (internal reference: {internalReferenceNumber2}) was successful");
			AssertLoggedMessage(loggerMock, LogType.Information, "Processing finished after 2/2 draft invoice(s) completed successfully");
			AssertLoggedMessage(loggerMock, LogType.Information, "Found 1 draft invoice(s) to process");
			AssertLoggedMessage(loggerMock, LogType.Debug,		$"Processing EDIMessage (PK): {item3.Message.PK}, Dash Invoice (PK): {item3.DashInvoice.PK}, Draft Invoice (PK): {item3.DraftInvoice.PK}");
			AssertLoggedMessage(loggerMock, LogType.Debug,		$"Processing draft invoice (internal reference: {internalReferenceNumber3}) was successful");
			AssertLoggedMessage(loggerMock, LogType.Information, "Processing finished after 1/1 draft invoice(s) completed successfully");
			AssertNothingToProcess(loggerMock);
			loggerMock.VerifyNoOtherCalls();

			AssertBatchProgress("First item in batch", item1.Message, item1.DraftInvoice, item1.DashDoc);
			AssertBatchProgress("Second item in batch", item2.Message, item2.DraftInvoice, item2.DashDoc);
			AssertBatchProgress("Third item in batch", item3.Message, item3.DraftInvoice, item3.DashDoc);

			void AssertBatchProgress(string testMessage, IEDIMessage message, AccDraftInvoiceHeader draftInvoice, DashDocument dashDoc)
			{
				CombineAssertions(testMessage, () =>
				{
					AssertEquals("The message should be processed", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
					AssertEquals("The draft invoice should still be 'Analyzing'", AccDraftInvoiceHeaderStatus.Analyzing, draftInvoice.AIH_Status);
					AssertTriedToUpdateDashDocumentToComplete("The dash doc should be complete", dashEntitiesServiceMock, dashDoc, Times.Once());
					Assert("The draft invoice should be queued for auto-rec & posting", IsQueuedForAutoRecAndPosting(draftInvoice));
				});
			}
		}

		public void TestMessageQueuedForRetry()
		{
			const string internalReferenceNumber = "00001000";
			var draftInvoice = objectCreator.CreateUploadedDraftInvoice("AUD");
			draftInvoice.AIH_InternalReference = internalReferenceNumber;
			var dashInvoice = CreateDashInvoice();
			var dashDoc = CreateDashDocument(draftInvoice, dashInvoice);
			dashInvoice.DPI_DDD_DashDocID = dashDoc.PK;
			var message = CreateEdiMessage(dashDoc);
			Factory.Save();

			var loggerMock = new Mock<ILogger>();
			var dashEntitiesServiceMock = new Mock<IDashEntitiesService>();
			dashEntitiesServiceMock
				.Setup(m => m.LoadAPInvoices(
					new[] { dashDoc.PK },
					It.IsAny<BusinessObjectFactory>()))
				.Returns(new[] { dashInvoice });
			// Force exception to happen during processing
			dashEntitiesServiceMock
				.Setup(m => m.UpdateStatusToComplete(dashDoc, false))
				.Returns(() => { throw SqlExceptionBuilder.CreateSqlException(errorNumber: 1205, "Deadlock exception occured"); });
			var timeProviderMock = new Mock<ITimeProvider>();
			timeProviderMock
				.Setup(m => m.GetCurrentUtcDateTime())
				.Returns(new DateTime(2025, 6, 2, 8, 0, 0));
			var cancellationTokenSource = new CancellationTokenSource();

			var processor = new ParsedDraftInvoiceProcessor(loggerMock.Object, dashEntitiesServiceMock.Object, timeProvider: timeProviderMock.Object, maxRetryCount: 3);
			processor.Run(cancellationTokenSource.Token);

			AssertLoggedMessage(loggerMock, LogType.Information, "Found 1 draft invoice(s) to process");
			AssertLoggedMessage(loggerMock, LogType.Debug,		$"Processing EDIMessage (PK): {message.PK}, Dash Invoice (PK): {dashInvoice.PK}, Draft Invoice (PK): {draftInvoice.PK}");
			AssertLoggedMessage(loggerMock, LogType.Error,		$"Processing draft invoice (internal reference: {internalReferenceNumber}) failed. Queued for attempt 2/4 sometime after 02-Jun-25 08:09:00 UTC\r\n" +
																 "Error message: Deadlock exception occured");
			AssertLoggedMessage(loggerMock, LogType.Information, "Processing cancelled after 0/1 draft invoice(s) completed successfully. 1 item failed, 0 still to be processed");
			AssertNothingToProcess(loggerMock);
			loggerMock.VerifyNoOtherCalls();

			AssertEquals("Retry count should be set to 1", (int)message.EM_RetryCount, 1);
			AssertDateTimeWithinOneSecond("Held until date not set correctly", message.EM_HeldUntilDate.ToDateTime(), new DateTime(2025, 6, 2, 8, 9, 0));
		}

		public void TestRetryLimitReached()
		{
			const string internalReferenceNumber = "00001000";
			var draftInvoice = objectCreator.CreateUploadedDraftInvoice("AUD");
			draftInvoice.AIH_InternalReference = internalReferenceNumber;
			var dashInvoice = CreateDashInvoice();
			var dashDoc = CreateDashDocument(draftInvoice, dashInvoice);
			dashInvoice.DPI_DDD_DashDocID = dashDoc.PK;
			var message = CreateEdiMessage(dashDoc);
			message.EM_RetryCount = 3;
			Factory.Save();

			var loggerMock = new Mock<ILogger>();
			var dashEntitiesServiceMock = new Mock<IDashEntitiesService>();
			dashEntitiesServiceMock
				.Setup(m => m.LoadAPInvoices(
					new[] { dashDoc.PK },
					It.IsAny<BusinessObjectFactory>()))
				.Returns(new[] { dashInvoice });
			// Force exception to happen during processing
			dashEntitiesServiceMock
				.Setup(m => m.UpdateStatusToComplete(dashDoc, false))
				.Returns(() => { throw SqlExceptionBuilder.CreateSqlException(errorNumber: 1205, "Deadlock exception occured"); });
			var cancellationTokenSource = new CancellationTokenSource();

			var processor = new ParsedDraftInvoiceProcessor(loggerMock.Object, dashEntitiesServiceMock.Object, maxRetryCount: 3);
			processor.Run(cancellationTokenSource.Token);

			AssertLoggedMessage(loggerMock, LogType.Information, "Found 1 draft invoice(s) to process");
			AssertLoggedMessage(loggerMock, LogType.Debug,		$"Processing EDIMessage (PK): {message.PK}, Dash Invoice (PK): {dashInvoice.PK}, Draft Invoice (PK): {draftInvoice.PK}");
			AssertLoggedMessage(loggerMock, LogType.Error,		$"Processing draft invoice (internal reference: {internalReferenceNumber}) failed. Retry limit reached - message marked as failed\r\n" +
																 "Error message: Deadlock exception occured");
			AssertLoggedMessage(loggerMock, LogType.Information, "Processing cancelled after 0/1 draft invoice(s) completed successfully. 1 item failed, 0 still to be processed");
			AssertNothingToProcess(loggerMock);
			loggerMock.VerifyNoOtherCalls();

			AssertEquals("Retry count should not go past the maximum", 3, (int)message.EM_RetryCount);
			AssertEquals("Message not marked as failed", EDIMessageStatusList.Codes.Failed, message.EM_Status);
			AssertEquals("Invoice not marked as draft", AccDraftInvoiceHeaderStatus.Draft, draftInvoice.AIH_Status);
		}

		public void TestSQLExceptionNotElegibleForRetry()
		{
			const string internalReferenceNumber = "00001000";
			var draftInvoice = objectCreator.CreateUploadedDraftInvoice("AUD");
			draftInvoice.AIH_InternalReference = internalReferenceNumber;
			var dashInvoice = CreateDashInvoice();
			var dashDoc = CreateDashDocument(draftInvoice, dashInvoice);
			dashInvoice.DPI_DDD_DashDocID = dashDoc.PK;
			var message = CreateEdiMessage(dashDoc);
			message.EM_RetryCount = 3;
			Factory.Save();

			var loggerMock = new Mock<ILogger>();
			var dashEntitiesServiceMock = new Mock<IDashEntitiesService>();
			dashEntitiesServiceMock
				.Setup(m => m.LoadAPInvoices(
					new[] { dashDoc.PK },
					It.IsAny<BusinessObjectFactory>()))
				.Returns(new[] { dashInvoice });
			// Force exception to happen during processing
			dashEntitiesServiceMock
				.Setup(m => m.UpdateStatusToComplete(dashDoc, false))
				.Returns(() => { throw SqlExceptionBuilder.CreateSqlException(errorNumber: 3702, "Database Offline"); });
			var cancellationTokenSource = new CancellationTokenSource();
			var processor = new ParsedDraftInvoiceProcessor(loggerMock.Object, dashEntitiesServiceMock.Object, maxRetryCount: 3);
			processor.Run(cancellationTokenSource.Token);

			AssertLoggedMessage(loggerMock, LogType.Information, "Found 1 draft invoice(s) to process");
			AssertLoggedMessage(loggerMock, LogType.Debug,		$"Processing EDIMessage (PK): {message.PK}, Dash Invoice (PK): {dashInvoice.PK}, Draft Invoice (PK): {draftInvoice.PK}");
			AssertLoggedMessage(loggerMock, LogType.Error,		$"Processing draft invoice (internal reference: {internalReferenceNumber}) failed. Error was not eligible for retry\r\n" +
																 "Error message: Database Offline");
			AssertLoggedMessage(loggerMock, LogType.Information, "Processing cancelled after 0/1 draft invoice(s) completed successfully. 1 item failed, 0 still to be processed");
			AssertNothingToProcess(loggerMock);
			loggerMock.VerifyNoOtherCalls();

			AssertEquals("Message not marked as failed", EDIMessageStatusList.Codes.Failed, message.EM_Status);
			AssertEquals("Invoice not marked as draft", AccDraftInvoiceHeaderStatus.Draft, draftInvoice.AIH_Status);
		}

		public void TestNonSQLExceptionNotElegibleForRetry()
		{
			const string internalReferenceNumber = "00001000";
			var draftInvoice = objectCreator.CreateUploadedDraftInvoice("AUD");
			draftInvoice.AIH_InternalReference = internalReferenceNumber;
			var dashInvoice = CreateDashInvoice();
			var dashDoc = CreateDashDocument(draftInvoice, dashInvoice);
			dashInvoice.DPI_DDD_DashDocID = dashDoc.PK;
			var message = CreateEdiMessage(dashDoc);
			message.EM_RetryCount = 3;
			Factory.Save();

			var loggerMock = new Mock<ILogger>();
			var dashEntitiesServiceMock = new Mock<IDashEntitiesService>();
			dashEntitiesServiceMock
				.Setup(m => m.LoadAPInvoices(
					new[] { dashDoc.PK },
					It.IsAny<BusinessObjectFactory>()))
				.Returns(new[] { dashInvoice });
			// Force exception to happen during processing
			dashEntitiesServiceMock
				.Setup(m => m.UpdateStatusToComplete(dashDoc, false))
				.Returns(() => { throw new Exception("Example Exception"); });
			var cancellationTokenSource = new CancellationTokenSource();
			var processor = new ParsedDraftInvoiceProcessor(loggerMock.Object, dashEntitiesServiceMock.Object, maxRetryCount: 3);
			processor.Run(cancellationTokenSource.Token);

			AssertLoggedMessage(loggerMock, LogType.Information, "Found 1 draft invoice(s) to process");
			AssertLoggedMessage(loggerMock, LogType.Debug,		$"Processing EDIMessage (PK): {message.PK}, Dash Invoice (PK): {dashInvoice.PK}, Draft Invoice (PK): {draftInvoice.PK}");
			AssertLoggedMessage(loggerMock, LogType.Error,		$"Processing draft invoice (internal reference: {internalReferenceNumber}) failed. Error was not eligible for retry\r\n" +
																 "Error message: Example Exception");
			AssertLoggedMessage(loggerMock, LogType.Information, "Processing cancelled after 0/1 draft invoice(s) completed successfully. 1 item failed, 0 still to be processed");
			AssertNothingToProcess(loggerMock);
			loggerMock.VerifyNoOtherCalls();

			AssertEquals("Message not marked as failed", EDIMessageStatusList.Codes.Failed, message.EM_Status);
			AssertEquals("Invoice not marked as draft", AccDraftInvoiceHeaderStatus.Draft, draftInvoice.AIH_Status);
		}

		public void TestFailingItemAtStartOfBatch()
		{
			const string internalReferenceNumber1 = "00001001";
			var item1 = CreateBatchItem();
			item1.DraftInvoice.AIH_InternalReference = internalReferenceNumber1;
			const string internalReferenceNumber2 = "00001002";
			var item2 = CreateBatchItem();
			item2.DraftInvoice.AIH_InternalReference = internalReferenceNumber2;
			Factory.Save();

			var loggerMock = new Mock<ILogger>();
			var dashEntitiesServiceMock = new Mock<IDashEntitiesService>();
			dashEntitiesServiceMock
				.Setup(m => m.LoadAPInvoices(
					It.IsAny<IEnumerable<ZGuid>>(),
					It.IsAny<BusinessObjectFactory>()))
				.Returns(new[] {
					item1.DashInvoice,
					item2.DashInvoice });
			// Force exception to happen during processing for item 1
			dashEntitiesServiceMock
				.Setup(m => m.UpdateStatusToComplete(item1.DashDoc, false))
				.Returns(() => { throw new Exception("Example Exception"); });
			dashEntitiesServiceMock
				.Setup(m => m.UpdateStatusToComplete(item2.DashDoc, false))
				.Returns(true);
			var timeProviderMock = new Mock<ITimeProvider>();
			timeProviderMock
				.Setup(m => m.GetCurrentUtcDateTime())
				.Returns(new DateTime(2025, 6, 2, 8, 0, 0));
			var cancellationTokenSource = new CancellationTokenSource();

			var processor = new ParsedDraftInvoiceProcessor(loggerMock.Object, dashEntitiesServiceMock.Object, timeProvider: timeProviderMock.Object, maxRetryCount: 3);
			processor.Run(cancellationTokenSource.Token);

			AssertLoggedMessage(loggerMock, LogType.Information, "Found 2 draft invoice(s) to process");
			AssertLoggedMessage(loggerMock, LogType.Debug,		$"Processing EDIMessage (PK): {item1.Message.PK}, Dash Invoice (PK): {item1.DashInvoice.PK}, Draft Invoice (PK): {item1.DraftInvoice.PK}");
			AssertLoggedMessage(loggerMock, LogType.Error,		$"Processing draft invoice (internal reference: {internalReferenceNumber1}) failed. Error was not eligible for retry\r\n" +
																 "Error message: Example Exception");
			AssertLoggedMessage(loggerMock, LogType.Information, "Processing cancelled after 0/2 draft invoice(s) completed successfully. 1 item failed, 1 still to be processed");
			AssertLoggedMessage(loggerMock, LogType.Information, "Found 1 draft invoice(s) to process");
			AssertLoggedMessage(loggerMock, LogType.Debug,		$"Processing draft invoice (internal reference: {internalReferenceNumber2}) was successful");
			AssertLoggedMessage(loggerMock, LogType.Debug,		$"Processing EDIMessage (PK): {item2.Message.PK}, Dash Invoice (PK): {item2.DashInvoice.PK}, Draft Invoice (PK): {item2.DraftInvoice.PK}");
			AssertLoggedMessage(loggerMock, LogType.Information, "Processing finished after 1/1 draft invoice(s) completed successfully");
			AssertNothingToProcess(loggerMock);
			loggerMock.VerifyNoOtherCalls();

			AssertEquals(AccDraftInvoiceHeaderStatus.Analyzing, item2.DraftInvoice.AIH_Status);
			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, item2.Message.EM_Status);
			AssertTriedToUpdateDashDocumentToComplete("The dash doc should be complete", dashEntitiesServiceMock, item2.DashDoc, Times.Once());
			Assert("The draft invoice should be queued for auto-rec & posting", IsQueuedForAutoRecAndPosting(item2.DraftInvoice));
		}

		public void TestMessageProcessedAfterHeldUntilDate()
		{
			const string internalReferenceNumber = "00001000";
			var draftInvoice = objectCreator.CreateUploadedDraftInvoice("AUD");
			draftInvoice.AIH_InternalReference = internalReferenceNumber;
			var dashInvoice = CreateDashInvoice();
			var dashDoc = CreateDashDocument(draftInvoice, dashInvoice);
			dashInvoice.DPI_DDD_DashDocID = dashDoc.PK;
			var message = CreateEdiMessage(dashDoc);
			message.EM_HeldUntilDate = new DateTime(2025, 6, 2, 7, 59, 0);
			Factory.Save();

			var loggerMock = new Mock<ILogger>();
			var dashEntitiesServiceMock = new Mock<IDashEntitiesService>();
			dashEntitiesServiceMock
				.Setup(m => m.LoadAPInvoices(
					new[] { dashDoc.PK },
					It.IsAny<BusinessObjectFactory>()))
				.Returns(new[] { dashInvoice });
			dashEntitiesServiceMock
				.Setup(m => m.UpdateStatusToComplete(dashDoc, false))
				.Returns(true);
			var timeProviderMock = new Mock<ITimeProvider>();
			timeProviderMock
				.Setup(m => m.GetCurrentUtcDateTime())
				.Returns(new DateTime(2025, 6, 2, 8, 0, 0));
			var cancellationTokenSource = new CancellationTokenSource();

			var processor = new ParsedDraftInvoiceProcessor(loggerMock.Object, dashEntitiesServiceMock.Object, timeProvider: timeProviderMock.Object);
			processor.Run(cancellationTokenSource.Token);

			AssertLoggedMessage(loggerMock, LogType.Information, "Found 1 draft invoice(s) to process");
			AssertLoggedMessage(loggerMock, LogType.Debug, $"Processing EDIMessage (PK): {message.PK}, Dash Invoice (PK): {dashInvoice.PK}, Draft Invoice (PK): {draftInvoice.PK}");
			AssertLoggedMessage(loggerMock, LogType.Debug, $"Processing draft invoice (internal reference: {internalReferenceNumber}) was successful");
			AssertLoggedMessage(loggerMock, LogType.Information, "Processing finished after 1/1 draft invoice(s) completed successfully");
			AssertNothingToProcess(loggerMock);
			loggerMock.VerifyNoOtherCalls();

			AssertEquals(AccDraftInvoiceHeaderStatus.Analyzing, draftInvoice.AIH_Status);
			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
			Assert("The draft invoice should be queued for auto-rec & posting", IsQueuedForAutoRecAndPosting(draftInvoice));
		}

		public void TestMessageIgnoredBeforeHeldUntilDate()
		{
			var draftInvoice = objectCreator.CreateUploadedDraftInvoice("AUD");
			var dashInvoice = CreateDashInvoice();
			var dashDoc = CreateDashDocument(draftInvoice, dashInvoice);
			dashInvoice.DPI_DDD_DashDocID = dashDoc.PK;
			var message = CreateEdiMessage(dashDoc);
			message.EM_HeldUntilDate = new DateTime(2025, 6, 2, 8, 1, 0);
			Factory.Save();

			var loggerMock = new Mock<ILogger>();
			var dashEntitiesServiceMock = new Mock<IDashEntitiesService>();
			dashEntitiesServiceMock
				.Setup(m => m.LoadAPInvoices(
					new[] { dashDoc.PK },
					It.IsAny<BusinessObjectFactory>()))
				.Returns(new[] { dashInvoice });
			var timeProviderMock = new Mock<ITimeProvider>();
			timeProviderMock
				.Setup(m => m.GetCurrentUtcDateTime())
				.Returns(new DateTime(2025, 6, 2, 8, 0, 0));
			var cancellationTokenSource = new CancellationTokenSource();

			var processor = new ParsedDraftInvoiceProcessor(loggerMock.Object, dashEntitiesServiceMock.Object, timeProvider: timeProviderMock.Object);
			processor.Run(cancellationTokenSource.Token);

			AssertNothingToProcess(loggerMock);
		}

		(AccDraftInvoiceHeader DraftInvoice, IEDIMessage Message, DashDocument DashDoc, DashAPInvoice DashInvoice) CreateBatchItem()
		{
			var draftInvoice = objectCreator.CreateUploadedDraftInvoice("AUD");
			var dashInvoice = CreateDashInvoice();
			var dashDoc = CreateDashDocument(draftInvoice, dashInvoice);
			dashInvoice.DPI_DDD_DashDocID = dashDoc.PK;
			var message = CreateEdiMessage(dashDoc);
			return (draftInvoice, message, dashDoc, dashInvoice);
		}

		IEDIMessage CreateEdiMessage(DashDocument dashDocument = null)
		{
			var message = Factory.New<IEDIMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.DashAccountingImport;
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Internal;
			message.EM_MessageType = EDIMessageTypeList.Codes.API;
			message.EM_Status = EDIMessageStatusList.Codes.Queued;
			message.EM_IsActive = true;
			if(dashDocument != null)
			{
				message.EM_LinkUniqueID = dashDocument.PK;
				message.EM_LinkTable = dashDocument.TablePrefix;
			}
			return message;
		}

		DashDocument CreateDashDocument(AccDraftInvoiceHeader draftInvoice = null, DashAPInvoice dashInvoice = null)
		{
			var dashDocument = Factory.New<DashDocument>();
			dashDocument.DDD_ParseType = "PIN";
			dashDocument.DDD_ParseStatus = "RTP";
			if(draftInvoice != null)
			{
				dashDocument.DDD_RelatedEntityID = draftInvoice.PK;
				dashDocument.DDD_RelatedEntityTableCode = draftInvoice.TablePrefix;
			}
			if (dashInvoice != null)
			{
				dashDocument.DDD_DocID = dashInvoice.PK;
			}

			return dashDocument;
		}

		DashAPInvoice CreateDashInvoice(ZGuid? matchedIssuerPk = null, ZGuid? matchedIssuerAddressPk = null,
			ZString? invoiceNumber = null, ZDate? invoiceDate = null, ZString? currencyCode = null,
			ZDecimal? grossTotal = null, ZDecimal? vatTotal = null, ZDecimal? netTotal = null)
		{
			var dashInvoice = Factory.New<DashAPInvoice>();
			dashInvoice.DPI_OH_MatchedIssuerID = matchedIssuerPk ?? ZGuid.Empty;
			dashInvoice.DPI_OA_MatchedIssuerAddressID = matchedIssuerAddressPk ?? ZGuid.Empty;
			dashInvoice.DPI_RX_NKInvoiceCurrency = currencyCode ?? ZString.Empty;
			dashInvoice.DPI_InvoiceNumber = invoiceNumber ?? ZString.Empty;
			dashInvoice.DPI_InvoiceDate = invoiceDate ?? ZDate.Empty;
			dashInvoice.DPI_GrossTotal = grossTotal ?? ZDecimal.Zero;
			dashInvoice.DPI_VatTotal = vatTotal ?? ZDecimal.Zero;
			dashInvoice.DPI_NetTotal = netTotal ?? ZDecimal.Zero;

			return dashInvoice;
		}

		static void AssertNothingToProcess(Mock<ILogger> loggerMock)
		{
			AssertNoExceptionThrown(() =>
			{
				loggerMock.Verify(logger => logger.Log(LogType.Debug, "No messages to process."), Times.Once);
			});
		}

		static void AssertLoggedMessage(Mock<ILogger> loggerMock, LogType expectedLogLevel, string expectedLogMessage, Times? expectedTimes = null)
		{
			AssertNoExceptionThrown(() =>
			{
				loggerMock.Verify(logger => logger.Log(expectedLogLevel, expectedLogMessage), expectedTimes ?? Times.Once());
			});
		}

		static void AssertTriedToUpdateDashDocumentToComplete(string testMessage, Mock<IDashEntitiesService> dashEntitiesServiceMock, DashDocument dashDoc, Times expectedTimes)
		{
			AssertNoExceptionThrown(testMessage, () =>
			{
				dashEntitiesServiceMock.Verify(svc => svc.UpdateStatusToComplete(It.Is<DashDocument>(i => i.PK == dashDoc.PK), false), expectedTimes);
			});
		}

		bool IsQueuedForAutoRecAndPosting(AccDraftInvoiceHeader draftInvoice)
		{
			var query = new ZQuery();
			query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.AccountsPayableAutomation);
			query.AddToFilter(EDIMessageSchema.EM_MessageType, EDIMessageTypeList.Codes.PIN);
			query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Internal);
			query.AddToFilter(EDIMessageSchema.EM_Status, EDIMessageStatusList.Codes.Queued);
			query.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, draftInvoice.PK);
			query.AddToFilter(EDIMessageSchema.EM_LinkTable, draftInvoice.TableName);
			query.AddToFilter(EDIMessageSchema.EM_GB, draftInvoice.AIH_GB_Branch);
			query.AddToFilter(EDIMessageSchema.EM_GE, draftInvoice.AIH_GE_Department);
			query.AddToFilter(EDIMessageSchema.EM_IsActive, true);
			query.TableIndexHints.Add(new TableIndexHint(EDIMessageSchema.Constants.Indexes.NR_RX__EM_ApplicationCode_EM_ReceiveTransmit_EM_SystemCreateTimeUtc_EM_MessageNum));

			return Factory.Load<IEDIMessage>(query).Length == 1;
		}

		protected override void SetUp()
		{
			base.SetUp();
			objectCreator = new TestObjectCreator(Factory);
		}

		TestObjectCreator objectCreator;
	}
}
