using System;
using System.Diagnostics;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.APAutomation.APReconciliation;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using TimeProvider = Enterprise.ZArchitecture.Core.TimeProvider;

namespace Enterprise.Accounting.APAutomation.AccountsPayableAutomationServiceTask
{
	public class AccountsPayableAutomationProcessor
	{
		public AccountsPayableAutomationProcessor(IAPReconciliationPoster poster, ILogger logger, ITimeProvider timeProvider = null,
			int maximumRetryAttempts = 1, TimeSpan? retryWaitPeriod = null, TimeSpan? progressReportPeriod = null)
		{
			this.poster = poster;
			this.logger = logger;
			this.timeProvider = timeProvider ?? new TimeProvider();
			this.maximumRetryAttempts = maximumRetryAttempts;
			this.retryWaitPeriod = retryWaitPeriod ?? TimeSpan.FromMinutes(10);
			this.progressReportPeriod = progressReportPeriod ?? TimeSpan.FromSeconds(30);
		}

		readonly IAPReconciliationPoster poster;
		readonly ILogger logger;
		readonly ITimeProvider timeProvider;
		readonly int maximumRetryAttempts;
		readonly TimeSpan retryWaitPeriod;
		readonly TimeSpan progressReportPeriod;

		public void Run(CancellationToken cancellationToken)
		{
			while (!cancellationToken.IsCancellationRequested)
			{
				var factory = new BusinessObjectFactory();
				var messageBatch = LoadMessages(factory);
				if (messageBatch.Length == 0)
				{
					logger.Log(LogType.Debug, "Nothing to process.");
					break;
				}

				logger.Log(LogType.Information, FormattableString.Invariant($"{messageBatch.Length} draft invoice/s ready to be processed."));

				var stopWatch = Stopwatch.StartNew();
				var processedCount = 0;
				foreach (var message in messageBatch)
				{
					if (cancellationToken.IsCancellationRequested)
					{
						logger.Log(LogType.Information, "Cancellation requested.");
						break;
					}

					try
					{
						ProcessMessage(message.PK);
					}
					catch (SqlException sqlException) when (!sqlException.IsCriticalException())
					{
						var dbErrorHandler = new DbErrorHandler(sqlException, ((IDbConnected)factory).Connection);
						HandleFailureToProcessMessage(message.PK, sqlException, isEligibleForRetry: dbErrorHandler.ShouldBeRetried);
					}
					catch (Exception exception) when (!exception.IsCriticalException())
					{
						HandleFailureToProcessMessage(message.PK, exception, isEligibleForRetry: false);
					}

					processedCount++;
					if (stopWatch.Elapsed > progressReportPeriod && processedCount != messageBatch.Length)
					{
						logger.Log(LogType.Information, FormattableString.Invariant($"Processed {processedCount}/{messageBatch.Length} draft invoice/s."));
						stopWatch.Reset();
					}
				}
				logger.Log(LogType.Information, FormattableString.Invariant($"Finished processing {processedCount} draft invoice/s."));
			}
		}

		void ProcessMessage(ZGuid messagePk)
		{
			var factory = new BusinessObjectFactory();
			var message = factory.Load<IEDIMessage>(messagePk);

			if (message.EM_LinkUniqueID.IsDefault || message.EM_LinkTable != AccDraftInvoiceHeader.Schema.TableName)
			{
				ErrorReporter.ReportOnce($"{GetType()}_NoAccDraftInvoiceHeader",
					$"The EDI Message {message.PK} is not linked to a draft invoice. Table: {message.EM_LinkTable}, PK: {message.EM_LinkUniqueID}");
				message.EM_Status = EDIMessageStatusList.Codes.Error;
				factory.Save();
				return;
			}

			var factoryForPosting = new BusinessObjectFactory();
			var draftInvoice = factoryForPosting.Load<AccDraftInvoiceHeader>(message.EM_LinkUniqueID);
			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, draftInvoice.AIH_GB_Branch.ToGuid(), draftInvoice.AIH_GE_Department.ToGuid()))
			{
				LogDiagnosticsForDraft(draftInvoice, FormattableString.Invariant($"User: {Env.CurrentUser.LoginName}, Company: {Env.CurrentCompany.Code}, Branch: {Env.CurrentBranch.Code}, Department: {Env.CurrentDepartment.Code}."));

				var postedInvoice = AutoReconcileAndPost(draftInvoice);
				message.EM_Status = EDIMessageStatusList.Codes.ProcessedOK;

				if (postedInvoice == null)
				{
					UpdateDraftInvoiceOnFailure(factory, message.EM_LinkUniqueID);
					factory.Save();
				}
				else
				{
					BusinessObjectFactory.SaveTogether(factory, factoryForPosting);
				}
			}
		}

		void HandleFailureToProcessMessage(ZGuid messagePk, Exception exception, bool isEligibleForRetry)
		{
			var factory = new BusinessObjectFactory();
			var message = factory.Load<IEDIMessage>(messagePk);

			if (!isEligibleForRetry)
			{
				logger.Log(LogType.Error, FormattableString.Invariant(@$"Unable to process EDI Message '{messagePk}'.
{exception.GetType()}:
{exception.Message}"));

				message.EM_Status = EDIMessageStatusList.Codes.Failed;
				UpdateDraftInvoiceOnFailure(factory, message.EM_LinkUniqueID);
			}
			else if (message.EM_RetryCount >= maximumRetryAttempts)
			{
				logger.Log(LogType.Error, FormattableString.Invariant(@$"Unable to process EDI Message '{messagePk}' after {message.EM_RetryCount + 1} attempts.
{exception.GetType()}:
{exception.Message}"));

				message.EM_Status = EDIMessageStatusList.Codes.Failed;
				message.EM_HeldUntilDate = ZDateTime.Empty;
				UpdateDraftInvoiceOnFailure(factory, message.EM_LinkUniqueID);
			}
			else
			{
				var heldUntilDate = new ZDateTime(timeProvider.GetCurrentUtcDateTime().Add(retryWaitPeriod));
				logger.Log(LogType.Error, FormattableString.Invariant(@$"Unable to process EDI Message '{messagePk}'. It will be re-processed in the next run after {heldUntilDate.ToBestReadableDateTimeString()} (UTC).
{exception.GetType()}:
{exception.Message}"));

				message.EM_RetryCount++;
				message.EM_HeldUntilDate = heldUntilDate;
			}
			factory.Save();
		}

		static void UpdateDraftInvoiceOnFailure(BusinessObjectFactory factory, ZGuid draftInvoicePK)
		{
			var draftInvoice = factory.Load<AccDraftInvoiceHeader>(draftInvoicePK);
			if (draftInvoice != null && draftInvoice.AIH_Status == AccDraftInvoiceHeaderStatus.Analyzing)
			{
				draftInvoice.AIH_Status = AccDraftInvoiceHeaderStatus.Draft;
			}
		}

		InvoicingBase AutoReconcileAndPost(AccDraftInvoiceHeader draftInvoice)
		{
			switch ((string)draftInvoice.AIH_TransactionType)
			{
				case TransactionTypes.CreditNote:
					return AutoReconcileAndPost<APCreditNote>(draftInvoice);
				case TransactionTypes.Invoice:
					return AutoReconcileAndPost<APInvoice>(draftInvoice);
				default:
					LogDiagnosticsForDraft(draftInvoice,
						FormattableString.Invariant($"Invalid transaction type '{draftInvoice.AIH_TransactionType}'."));
					return null;
			}
		}

		T AutoReconcileAndPost<T>(AccDraftInvoiceHeader draftInvoice) where T : InvoicingBase
		{
			LogDiagnosticsForDraft(draftInvoice, (NoResString)"Attempting to auto-reconcile and post.");

			var postedInvoice = poster.AutoReconcileAndPost<T>(draftInvoice, out var errorMessage);

			if (postedInvoice == null)
			{
				LogDiagnosticsForDraft(draftInvoice,
					FormattableString.Invariant($"Could not be auto-reconciled/posted. Reason:\r\n{errorMessage}"));
				return null;
			}

			LogDiagnosticsForDraft(draftInvoice, (NoResString)"Successfully posted.");

			return postedInvoice;
		}

		void LogDiagnosticsForDraft(AccDraftInvoiceHeader draftInvoice, string message) => logger.Log(LogType.Debug,
			FormattableString.Invariant(
				$"[{draftInvoice.AIH_TransactionType}|{draftInvoice.AIH_InternalReference}] {message}"));

		IEDIMessage[] LoadMessages(BusinessObjectFactory factory)
		{
			var heldUntilDateQuery = new ZQuery(EDIMessageSchema.EM_HeldUntilDate, SQLComparisonOperator.Equal, null);
			heldUntilDateQuery.AddToFilter(JoinCondition.Or, EDIMessageSchema.EM_HeldUntilDate, SQLComparisonOperator.LessThanOrEqualTo, timeProvider.GetCurrentUtcDateTime());

			var query = new ZDBOnlyQuery(typeof(IEDIMessage))
				.AddToFilter(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.AccountsPayableAutomation)
				.AddToFilter(EDIMessageSchema.EM_MessageType, EDIMessageTypeList.Codes.PIN)
				.AddToFilter(EDIMessageSchema.EM_IsActive, true)
				.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Internal)
				.AddToFilter(EDIMessageSchema.EM_Status, EDIMessageStatusList.Codes.Queued)
				.AddToFilter(heldUntilDateQuery);

			query.TableIndexHints.Add(new TableIndexHint(EDIMessageSchema.Constants.Indexes.NR_RX__EM_ApplicationCode_EM_ReceiveTransmit_EM_MessageNum_EM_SystemCreateTimeUtc));

			return factory.Load<IEDIMessage>(query);
		}
	}
}
