using System;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.APAutomation.AccountsPayableAutomationServiceTask;
using Enterprise.Accounting.APAutomation.APReconciliation;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;
using Moq;
using Moq.Language.Flow;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.APAutomation.Testing.AccountsPayableAutomationServiceTask
{
	public class AccountsPayableAutomationProcessorTest : TestCaseWithFactory
	{
		public void TestNoMessagesToProcess()
		{
			var loggerMock = new Mock<ILogger>();
			var posterMock = new Mock<IAPReconciliationPoster>();
			var cancellationTokenSource = new CancellationTokenSource();

			var processor = new AccountsPayableAutomationProcessor(posterMock.Object, loggerMock.Object);
			processor.Run(cancellationTokenSource.Token);

			AssertLoggedMessage(loggerMock, LogType.Debug, "Nothing to process.");
			loggerMock.VerifyNoOtherCalls();
		}

		public void TestNoDraftInvoiceLinkedToMessage()
		{
			var message = CreateEdiMessage();
			Factory.Save();

			var loggerMock = new Mock<ILogger>();
			var posterMock = new Mock<IAPReconciliationPoster>();
			var cancellationTokenSource = new CancellationTokenSource();

			var processor = new AccountsPayableAutomationProcessor(posterMock.Object, loggerMock.Object);
			processor.Run(cancellationTokenSource.Token);

			AssertLoggedMessage(loggerMock, LogType.Information, "1 draft invoice/s ready to be processed.");

			AssertEquals("An error should be reported", 1, ErrorReporter.TotalErrorCount);
			AssertEquals(ErrorReporter.LastMessageReported, $"The EDI Message {message.PK} is not linked to a draft invoice. Table: {message.EM_LinkTable}, PK: {message.EM_LinkUniqueID}");
			ErrorReporter.Clear();

			AssertLoggedMessage(loggerMock, LogType.Information, "Finished processing 1 draft invoice/s.");
			AssertLoggedMessage(loggerMock, LogType.Debug, "Nothing to process.");
			loggerMock.VerifyNoOtherCalls();

			AssertEquals(EDIMessageStatusList.Codes.Error, message.EM_Status);
		}

		public void TestDraftInvoiceReconciledAndPosted()
		{
			var draftInvoice = objectCreator.CreateUploadedDraftInvoice("AUD");
			var message = CreateEdiMessage(draftInvoice);
			Factory.Save();

			var loggerMock = new Mock<ILogger>();
			var posterMock = new Mock<IAPReconciliationPoster>();
			SetupPoster<APInvoice>(posterMock, draftInvoice)
				.Returns(Factory.New<APInvoice>());
			var cancellationTokenSource = new CancellationTokenSource();

			var processor = new AccountsPayableAutomationProcessor(posterMock.Object, loggerMock.Object);
			processor.Run(cancellationTokenSource.Token);

			AssertAutoReconcileAndPostCalled(posterMock, draftInvoice);

			AssertLoggedMessage(loggerMock, LogType.Information, "1 draft invoice/s ready to be processed.");
			AssertLoggedMessage(loggerMock, LogType.Debug, "[INV|001] User: CWSupport, Company: EDI, Branch: BNE, Department: BRN.");
			AssertLoggedMessage(loggerMock, LogType.Debug, "[INV|001] Attempting to auto-reconcile and post.");
			AssertLoggedMessage(loggerMock, LogType.Debug, "[INV|001] Successfully posted.");
			AssertLoggedMessage(loggerMock, LogType.Information, "Finished processing 1 draft invoice/s.");
			AssertLoggedMessage(loggerMock, LogType.Debug, "Nothing to process.");
			loggerMock.VerifyNoOtherCalls();

			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
		}

		public void TestMultipleDraftInvoicesReconciledAndPosted()
		{
			var draftInvoice = objectCreator.CreateUploadedDraftInvoice("AUD");
			draftInvoice.AIH_InternalReference = "001";
			var message1 = CreateEdiMessage(draftInvoice);
			var draftCreditNote = objectCreator.CreateDraftTransaction(TransactionTypes.CreditNote, AccDraftInvoiceHeaderStatus.Analyzing, "AUD", internalReferenceNumber: "002");
			draftCreditNote.AIH_GC_Company = objectCreator.NonCurrentCompany.PK;
			draftCreditNote.AIH_GB_Branch = objectCreator.NonCurrentBranch.PK;
			draftCreditNote.AIH_GE_Department = objectCreator.NonCurrentDepartment.PK;
			var message2 = CreateEdiMessage(draftCreditNote);
			Factory.Save();

			var loggerMock = new Mock<ILogger>();
			var posterMock = new Mock<IAPReconciliationPoster>();
			SetupPoster<APInvoice>(posterMock, draftInvoice)
				.Returns(Factory.New<APInvoice>());
			SetupPoster<APCreditNote>(posterMock, draftCreditNote)
				.Returns(Factory.New<APCreditNote>());
			var cancellationTokenSource = new CancellationTokenSource();

			var processor = new AccountsPayableAutomationProcessor(posterMock.Object, loggerMock.Object, progressReportPeriod: TimeSpan.FromSeconds(0));
			processor.Run(cancellationTokenSource.Token);

			AssertLoggedMessage(loggerMock, LogType.Information, "2 draft invoice/s ready to be processed.");
			AssertLoggedMessage(loggerMock, LogType.Debug, "[INV|001] User: CWSupport, Company: EDI, Branch: BNE, Department: BRN.");
			AssertLoggedMessage(loggerMock, LogType.Debug, "[INV|001] Attempting to auto-reconcile and post.");
			AssertLoggedMessage(loggerMock, LogType.Debug, "[INV|001] Successfully posted.");
			AssertLoggedMessage(loggerMock, LogType.Information, "Processed 1/2 draft invoice/s.");
			AssertLoggedMessage(loggerMock, LogType.Debug, "[CRD|002] User: CWSupport, Company: EDI, Branch: SYD, Department: CEA.");
			AssertLoggedMessage(loggerMock, LogType.Debug, "[CRD|002] Attempting to auto-reconcile and post.");
			AssertLoggedMessage(loggerMock, LogType.Debug, "[CRD|002] Successfully posted.");
			AssertLoggedMessage(loggerMock, LogType.Information, "Finished processing 2 draft invoice/s.");
			AssertLoggedMessage(loggerMock, LogType.Debug, "Nothing to process.");
			loggerMock.VerifyNoOtherCalls();

			AssertEquals("The first message should be processed", EDIMessageStatusList.Codes.ProcessedOK, message1.EM_Status);
			AssertEquals("The second message should be processed", EDIMessageStatusList.Codes.ProcessedOK, message2.EM_Status);
		}

		public void TestDraftInvoiceCouldNotBePosted_Analyzing()
		{
			var draftInvoice = objectCreator.CreateUploadedDraftInvoice("AUD");
			var message = CreateEdiMessage(draftInvoice);
			Factory.Save();

			AssertEquals("Pre-condition: Draft invoice has status 'Analyzing'", AccDraftInvoiceHeaderStatus.Analyzing, draftInvoice.AIH_Status);

			var loggerMock = new Mock<ILogger>();
			var posterMock = new Mock<IAPReconciliationPoster>();
			SetupPoster<APInvoice>(posterMock, draftInvoice, errorMessage: "Some error.");
			var cancellationTokenSource = new CancellationTokenSource();

			var processor = new AccountsPayableAutomationProcessor(posterMock.Object, loggerMock.Object);
			processor.Run(cancellationTokenSource.Token);

			AssertLoggedMessage(loggerMock, LogType.Information, "1 draft invoice/s ready to be processed.");
			AssertLoggedMessage(loggerMock, LogType.Debug, "[INV|001] User: CWSupport, Company: EDI, Branch: BNE, Department: BRN.");
			AssertLoggedMessage(loggerMock, LogType.Debug, "[INV|001] Attempting to auto-reconcile and post.");
			AssertLoggedMessage(loggerMock, LogType.Debug, "[INV|001] Could not be auto-reconciled/posted. Reason:\r\nSome error.");
			AssertLoggedMessage(loggerMock, LogType.Information, "Finished processing 1 draft invoice/s.");
			AssertLoggedMessage(loggerMock, LogType.Debug, "Nothing to process.");
			loggerMock.VerifyNoOtherCalls();

			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
			AssertEquals(AccDraftInvoiceHeaderStatus.Draft, draftInvoice.AIH_Status);
		}

		public void TestDraftInvoiceCouldNotBePosted_ApprovedForPosting()
		{
			var draftInvoice = objectCreator.CreateUploadedDraftInvoice("AUD");
			draftInvoice.AIH_Status = AccDraftInvoiceHeaderStatus.Draft;
			draftInvoice.HasReconciliationRun = true;
			draftInvoice.AIH_Status = AccDraftInvoiceHeaderStatus.ApprovedForPosting;
			var message = CreateEdiMessage(draftInvoice);
			Factory.Save();

			AssertEquals("Pre-condition: Draft invoice has status 'ApprovedForPosting'", AccDraftInvoiceHeaderStatus.ApprovedForPosting, draftInvoice.AIH_Status);

			var loggerMock = new Mock<ILogger>();
			var posterMock = new Mock<IAPReconciliationPoster>();
			SetupPoster<APInvoice>(posterMock, draftInvoice, errorMessage: "Some error.");
			var cancellationTokenSource = new CancellationTokenSource();

			var processor = new AccountsPayableAutomationProcessor(posterMock.Object, loggerMock.Object);
			processor.Run(cancellationTokenSource.Token);

			AssertLoggedMessage(loggerMock, LogType.Information, "1 draft invoice/s ready to be processed.");
			AssertLoggedMessage(loggerMock, LogType.Debug, "[INV|001] User: CWSupport, Company: EDI, Branch: BNE, Department: BRN.");
			AssertLoggedMessage(loggerMock, LogType.Debug, "[INV|001] Attempting to auto-reconcile and post.");
			AssertLoggedMessage(loggerMock, LogType.Debug, "[INV|001] Could not be auto-reconciled/posted. Reason:\r\nSome error.");
			AssertLoggedMessage(loggerMock, LogType.Information, "Finished processing 1 draft invoice/s.");
			AssertLoggedMessage(loggerMock, LogType.Debug, "Nothing to process.");
			loggerMock.VerifyNoOtherCalls();

			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
			AssertEquals(AccDraftInvoiceHeaderStatus.ApprovedForPosting, draftInvoice.AIH_Status);
		}

		public void TestCancellationToken()
		{
			var draftInvoice1 = objectCreator.CreateUploadedDraftInvoice("AUD");
			draftInvoice1.AIH_InternalReference = "001";
			var message1 = CreateEdiMessage(draftInvoice1);
			var draftInvoice2 = objectCreator.CreateUploadedDraftInvoice("AUD");
			draftInvoice2.AIH_InternalReference = "002";
			var message2 = CreateEdiMessage(draftInvoice2);
			Factory.Save();

			var cancellationTokenSource = new CancellationTokenSource();
			var loggerMock = new Mock<ILogger>();
			var posterMock = new Mock<IAPReconciliationPoster>();
			SetupPoster<APInvoice>(posterMock, draftInvoice1)
				.Returns(() =>
				{
					//Force the cancellation to happen while the first draft invoice is being reconciled.
					cancellationTokenSource.Cancel();
					return Factory.New<APInvoice>();
				});

			var processor = new AccountsPayableAutomationProcessor(posterMock.Object, loggerMock.Object, progressReportPeriod: TimeSpan.FromSeconds(0));
			processor.Run(cancellationTokenSource.Token);

			AssertLoggedMessage(loggerMock, LogType.Information, "2 draft invoice/s ready to be processed.");
			AssertLoggedMessage(loggerMock, LogType.Debug, "[INV|001] User: CWSupport, Company: EDI, Branch: BNE, Department: BRN.");
			AssertLoggedMessage(loggerMock, LogType.Debug, "[INV|001] Attempting to auto-reconcile and post.");
			AssertLoggedMessage(loggerMock, LogType.Debug, "[INV|001] Successfully posted.");
			AssertLoggedMessage(loggerMock, LogType.Information, "Processed 1/2 draft invoice/s.");
			AssertLoggedMessage(loggerMock, LogType.Information, "Cancellation requested.");
			AssertLoggedMessage(loggerMock, LogType.Information, "Finished processing 1 draft invoice/s.");
			loggerMock.VerifyNoOtherCalls();

			AssertEquals("The first message should be processed", EDIMessageStatusList.Codes.ProcessedOK, message1.EM_Status);
			AssertEquals("The second message should not be processed", EDIMessageStatusList.Codes.Queued, message2.EM_Status);
		}

		public void TestExceptionQueuedForRetry()
		{
			var draftInvoice1 = objectCreator.CreateUploadedDraftInvoice("AUD");
			draftInvoice1.AIH_InternalReference = "001";
			var message1 = CreateEdiMessage(draftInvoice1);
			var draftInvoice2 = objectCreator.CreateUploadedDraftInvoice("AUD");
			draftInvoice2.AIH_InternalReference = "002";
			var message2 = CreateEdiMessage(draftInvoice2);
			Factory.Save();

			var cancellationTokenSource = new CancellationTokenSource();
			var loggerMock = new Mock<ILogger>();
			var posterMock = new Mock<IAPReconciliationPoster>();
			SetupPoster<APInvoice>(posterMock, draftInvoice1)
				.Throws(SqlExceptionBuilder.CreateSqlException(6005, "Some GeneralNetworkError processing draft invoice 001."));
			SetupPoster<APInvoice>(posterMock, draftInvoice2)
				.Returns(Factory.New<APInvoice>());
			var timeProviderMock = new Mock<ITimeProvider>();
			var currentDateTimeUtc = new DateTime(2025, 1, 7, 10, 33, 56);
			timeProviderMock.Setup(c => c.GetCurrentUtcDateTime()).Returns(currentDateTimeUtc);

			var processor = new AccountsPayableAutomationProcessor(posterMock.Object, loggerMock.Object, timeProvider: timeProviderMock.Object);
			processor.Run(cancellationTokenSource.Token);

			AssertLoggedMessage(loggerMock, LogType.Information, "2 draft invoice/s ready to be processed.");
			AssertLoggedMessage(loggerMock, LogType.Debug, "[INV|001] User: CWSupport, Company: EDI, Branch: BNE, Department: BRN.");
			AssertLoggedMessage(loggerMock, LogType.Debug, "[INV|001] Attempting to auto-reconcile and post.");
			AssertLoggedMessage(loggerMock,
				expectedLogLevel: LogType.Error,
				expectedLogMessage: @$"Unable to process EDI Message '{message1.PK}'. It will be re-processed in the next run after 07 Jan 2025 10:43 (UTC).
{SqlClientSqlException}:
Some GeneralNetworkError processing draft invoice 001.");
			AssertLoggedMessage(loggerMock, LogType.Debug, "[INV|002] User: CWSupport, Company: EDI, Branch: BNE, Department: BRN.");
			AssertLoggedMessage(loggerMock, LogType.Debug, "[INV|002] Attempting to auto-reconcile and post.");
			AssertLoggedMessage(loggerMock, LogType.Debug, "[INV|002] Successfully posted.");
			AssertLoggedMessage(loggerMock, LogType.Information, "Finished processing 2 draft invoice/s.");
			AssertLoggedMessage(loggerMock, LogType.Debug, "Nothing to process.");
			loggerMock.VerifyNoOtherCalls();

			CombineAssertions("The first message should be queued for retry", () =>
			{
				AssertEquals(EDIMessageStatusList.Codes.Queued, message1.EM_Status);
				AssertEquals((byte)1, message1.EM_RetryCount);
				AssertEquals(currentDateTimeUtc.AddMinutes(10), message1.EM_HeldUntilDate);
				AssertEquals(AccDraftInvoiceHeaderStatus.Analyzing, draftInvoice1.AIH_Status);
			});

			CombineAssertions("The second message should be processed", () =>
			{
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message2.EM_Status);
				AssertEquals((byte)0, message2.EM_RetryCount);
				AssertEquals(ZDateTime.Empty, message2.EM_HeldUntilDate);
				//If PostFromDraftInvoice wasn't being mocked then AIH_Status would be "Processed" here.
				AssertEquals(AccDraftInvoiceHeaderStatus.Analyzing, draftInvoice1.AIH_Status);
			});
		}

		public void TestExceptionsNotQueuedForRetry()
		{
			var draftInvoice1 = objectCreator.CreateUploadedDraftInvoice("AUD");
			draftInvoice1.AIH_InternalReference = "001";
			var message1 = CreateEdiMessage(draftInvoice1);
			var draftInvoice2 = objectCreator.CreateUploadedDraftInvoice("AUD");
			draftInvoice2.AIH_InternalReference = "002";
			var message2 = CreateEdiMessage(draftInvoice2);
			Factory.Save();

			var cancellationTokenSource = new CancellationTokenSource();
			var loggerMock = new Mock<ILogger>();
			var posterMock = new Mock<IAPReconciliationPoster>();
			SetupPoster<APInvoice>(posterMock, draftInvoice1)
				.Throws(SqlExceptionBuilder.CreateSqlException(5011, "Some DatabaseDoesNotExist processing draft invoice 001."));
			SetupPoster<APInvoice>(posterMock, draftInvoice2)
				.Throws(new Exception("Some other exception processing draft invoice 002."));

			var processor = new AccountsPayableAutomationProcessor(posterMock.Object, loggerMock.Object);
			processor.Run(cancellationTokenSource.Token);

			AssertLoggedMessage(loggerMock, LogType.Information, "2 draft invoice/s ready to be processed.");
			AssertLoggedMessage(loggerMock, LogType.Debug, "[INV|001] User: CWSupport, Company: EDI, Branch: BNE, Department: BRN.");
			AssertLoggedMessage(loggerMock, LogType.Debug, "[INV|001] Attempting to auto-reconcile and post.");
			AssertLoggedMessage(loggerMock,
				expectedLogLevel: LogType.Error,
				expectedLogMessage: @$"Unable to process EDI Message '{message1.PK}'.
{SqlClientSqlException}:
Some DatabaseDoesNotExist processing draft invoice 001.");
			AssertLoggedMessage(loggerMock, LogType.Debug, "[INV|002] User: CWSupport, Company: EDI, Branch: BNE, Department: BRN.");
			AssertLoggedMessage(loggerMock, LogType.Debug, "[INV|002] Attempting to auto-reconcile and post.");
			AssertLoggedMessage(loggerMock,
				expectedLogLevel: LogType.Error,
				expectedLogMessage: @$"Unable to process EDI Message '{message2.PK}'.
System.Exception:
Some other exception processing draft invoice 002.");
			AssertLoggedMessage(loggerMock, LogType.Information, "Finished processing 2 draft invoice/s.");
			AssertLoggedMessage(loggerMock, LogType.Debug, "Nothing to process.");
			loggerMock.VerifyNoOtherCalls();

			CombineAssertions("The first message should have failed and NOT queued for retry", () =>
			{
				AssertEquals(EDIMessageStatusList.Codes.Failed, message1.EM_Status);
				AssertEquals((byte)0, message1.EM_RetryCount);
				AssertEquals(ZDateTime.Empty, message1.EM_HeldUntilDate);
				AssertEquals(AccDraftInvoiceHeaderStatus.Draft, draftInvoice1.AIH_Status);
			});

			CombineAssertions("The second message should have failed and NOT queued for retry", () =>
			{
				AssertEquals(EDIMessageStatusList.Codes.Failed, message2.EM_Status);
				AssertEquals((byte)0, message2.EM_RetryCount);
				AssertEquals(ZDateTime.Empty, message2.EM_HeldUntilDate);
				AssertEquals(AccDraftInvoiceHeaderStatus.Draft, draftInvoice2.AIH_Status);
			});
		}

		public void TestRetryMessage_Succeeds()
		{
			var currentDateTimeUtc = new DateTime(2025, 1, 7, 10, 33, 0);
			var draftInvoice = objectCreator.CreateUploadedDraftInvoice("AUD");
			draftInvoice.AIH_InternalReference = "001";
			var message = CreateEdiMessage(draftInvoice);
			message.EM_RetryCount = 1;
			var retryTimeUtc = currentDateTimeUtc.AddMinutes(-1);
			message.EM_HeldUntilDate = retryTimeUtc;
			Factory.Save();

			var cancellationTokenSource = new CancellationTokenSource();
			var loggerMock = new Mock<ILogger>();
			var posterMock = new Mock<IAPReconciliationPoster>();
			SetupPoster<APInvoice>(posterMock, draftInvoice)
				.Returns(Factory.New<APInvoice>());
			var timeProviderMock = new Mock<ITimeProvider>();
			timeProviderMock.Setup(c => c.GetCurrentUtcDateTime()).Returns(currentDateTimeUtc);

			var processor = new AccountsPayableAutomationProcessor(posterMock.Object, loggerMock.Object, timeProvider: timeProviderMock.Object);
			processor.Run(cancellationTokenSource.Token);

			AssertLoggedMessage(loggerMock, LogType.Information, "1 draft invoice/s ready to be processed.");
			AssertLoggedMessage(loggerMock, LogType.Debug, "[INV|001] User: CWSupport, Company: EDI, Branch: BNE, Department: BRN.");
			AssertLoggedMessage(loggerMock, LogType.Debug, "[INV|001] Attempting to auto-reconcile and post.");
			AssertLoggedMessage(loggerMock, LogType.Debug, "[INV|001] Successfully posted.");
			AssertLoggedMessage(loggerMock, LogType.Information, "Finished processing 1 draft invoice/s.");
			AssertLoggedMessage(loggerMock, LogType.Debug, "Nothing to process.");
			loggerMock.VerifyNoOtherCalls();

			AssertAutoReconcileAndPostCalled(posterMock, draftInvoice);

			CombineAssertions("The message should be processed", () =>
			{
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
				AssertEquals((byte)1, message.EM_RetryCount);
				AssertEquals(retryTimeUtc, message.EM_HeldUntilDate);
				//If PostFromDraftInvoice wasn't being mocked then AIH_Status would be "Processed" here.
				AssertEquals(AccDraftInvoiceHeaderStatus.Analyzing, draftInvoice.AIH_Status);
			});
		}

		public void TestRetryMessage_ExceedsLimit()
		{
			var currentDateTimeUtc = new DateTime(2025, 1, 7, 10, 33, 0);
			var draftInvoice1 = objectCreator.CreateUploadedDraftInvoice("AUD");
			draftInvoice1.AIH_InternalReference = "001";
			var message1 = CreateEdiMessage(draftInvoice1);
			message1.EM_RetryCount = 1;
			var retryTimeUtc = currentDateTimeUtc.AddMinutes(-1);
			message1.EM_HeldUntilDate = retryTimeUtc;
			var draftInvoice2 = objectCreator.CreateUploadedDraftInvoice("AUD");
			draftInvoice2.AIH_InternalReference = "002";
			var message2 = CreateEdiMessage(draftInvoice2);
			Factory.Save();

			var cancellationTokenSource = new CancellationTokenSource();
			var loggerMock = new Mock<ILogger>();
			var posterMock = new Mock<IAPReconciliationPoster>();
			SetupPoster<APInvoice>(posterMock, draftInvoice1)
				.Throws(SqlExceptionBuilder.CreateSqlException(1205, "Some DeadlockError processing draft invoice 001."));
			SetupPoster<APInvoice>(posterMock, draftInvoice2)
				.Returns(Factory.New<APInvoice>());
			var timeProviderMock = new Mock<ITimeProvider>();
			timeProviderMock.Setup(c => c.GetCurrentUtcDateTime()).Returns(currentDateTimeUtc);

			var processor = new AccountsPayableAutomationProcessor(posterMock.Object, loggerMock.Object, timeProvider: timeProviderMock.Object);
			processor.Run(cancellationTokenSource.Token);

			AssertLoggedMessage(loggerMock, LogType.Information, "2 draft invoice/s ready to be processed.");
			AssertLoggedMessage(loggerMock, LogType.Debug, "[INV|001] User: CWSupport, Company: EDI, Branch: BNE, Department: BRN.");
			AssertLoggedMessage(loggerMock, LogType.Debug, "[INV|001] Attempting to auto-reconcile and post.");
			AssertLoggedMessage(loggerMock, LogType.Error, @$"Unable to process EDI Message '{message1.PK}' after 2 attempts.
{SqlClientSqlException}:
Some DeadlockError processing draft invoice 001.");
			AssertLoggedMessage(loggerMock, LogType.Debug, "[INV|002] User: CWSupport, Company: EDI, Branch: BNE, Department: BRN.");
			AssertLoggedMessage(loggerMock, LogType.Debug, "[INV|002] Attempting to auto-reconcile and post.");
			AssertLoggedMessage(loggerMock, LogType.Debug, "[INV|002] Successfully posted.");
			AssertLoggedMessage(loggerMock, LogType.Information, "Finished processing 2 draft invoice/s.");
			AssertLoggedMessage(loggerMock, LogType.Debug, "Nothing to process.");
			loggerMock.VerifyNoOtherCalls();

			AssertAutoReconcileAndPostCalled(posterMock, draftInvoice1);
			AssertAutoReconcileAndPostCalled(posterMock, draftInvoice2);

			CombineAssertions("The first message should be queued for retry", () =>
			{
				AssertEquals(EDIMessageStatusList.Codes.Failed, message1.EM_Status);
				AssertEquals((byte)1, message1.EM_RetryCount);
				AssertEquals(ZDateTime.Empty, message1.EM_HeldUntilDate);
			});

			CombineAssertions("The second message should be processed", () =>
			{
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message2.EM_Status);
				AssertEquals((byte)0, message2.EM_RetryCount);
				AssertEquals(ZDateTime.Empty, message2.EM_HeldUntilDate);
			});
		}

		IEDIMessage CreateEdiMessage(AccDraftInvoiceHeader draftInvoice = null)
		{
			var message = Factory.New<IEDIMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.AccountsPayableAutomation;
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Internal;
			message.EM_MessageType = EDIMessageTypeList.Codes.PIN;
			message.EM_Status = EDIMessageStatusList.Codes.Queued;
			message.EM_IsActive = true;
			if (draftInvoice != null)
			{
				message.EM_LinkUniqueID = draftInvoice.PK;
				message.EM_LinkTable = draftInvoice.TableName;
			}
			return message;
		}

		static void AssertLoggedMessage(Mock<ILogger> loggerMock, LogType expectedLogLevel, string expectedLogMessage, Times? expectedTimes = null)
		{
			AssertNoExceptionThrown(() =>
			{
				loggerMock.Verify(logger => logger.Log(expectedLogLevel, expectedLogMessage), expectedTimes ?? Times.Once());
			});
		}

		static void AssertAutoReconcileAndPostCalled(Mock<IAPReconciliationPoster> posterMock, AccDraftInvoiceHeader draftInvoice)
		{
			posterMock.Verify(
				p => p.AutoReconcileAndPost<APInvoice>(It.Is<AccDraftInvoiceHeader>(d => d.PK == draftInvoice.PK),
					out It.Ref<string>.IsAny), Times.Once());
		}

		ISetup<IAPReconciliationPoster, T> SetupPoster<T>(
			Mock<IAPReconciliationPoster> posterMock,
			AccDraftInvoiceHeader draftInvoice,
			string errorMessage = default
			) where T : InvoicingBase
		{
			return posterMock.Setup(
				p => p.AutoReconcileAndPost<T>(It.Is<AccDraftInvoiceHeader>(d => d.PK == draftInvoice.PK),
					out errorMessage));
		}

		protected override void SetUp()
		{
			base.SetUp();
			objectCreator = new TestObjectCreator(Factory);
		}

		TestObjectCreator objectCreator;

#if NETFRAMEWORK
		const string SqlClientSqlException = "System.Data.SqlClient.SqlException";
#else
		const string SqlClientSqlException = "Microsoft.Data.SqlClient.SqlException";
#endif
	}
}
