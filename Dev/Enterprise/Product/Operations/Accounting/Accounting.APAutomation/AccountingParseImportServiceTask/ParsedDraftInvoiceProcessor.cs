using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Dash.Business;
using Enterprise.Dash.Business.Services;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using TimeProvider = Enterprise.ZArchitecture.Core.TimeProvider;

namespace Enterprise.Accounting.APAutomation.AccountingParseImportServiceTask
{
	public class ParsedDraftInvoiceProcessor
	{
		public ParsedDraftInvoiceProcessor(ILogger logger, IDashEntitiesService dashEntitiesService, ITimeProvider timeProvider = null, int batchSize = 10, int maxRetryCount = 4, TimeSpan? minimumRetryDelay = null)
		{
			this.logger = logger;
			this.dashEntitiesService = dashEntitiesService;
			this.timeProvider = timeProvider ?? new TimeProvider();
			this.batchSize = batchSize;
			this.maxRetryCount = maxRetryCount;
			this.minimumRetryDelay = minimumRetryDelay ?? TimeSpan.FromMinutes(9);
		}

		readonly ILogger logger;
		readonly IDashEntitiesService dashEntitiesService;
		readonly ITimeProvider timeProvider;
		readonly int batchSize;
		readonly int maxRetryCount;
		readonly TimeSpan minimumRetryDelay;

		public void Run(CancellationToken cancellationToken)
		{
			while (!cancellationToken.IsCancellationRequested)
			{
				var successfulItemCount = 0;
				var processingCancelledDueToFailure = false;
				var factory = new BusinessObjectFactory();
				var invoiceBatch = LoadInvoiceBatch(factory);
				if (invoiceBatch.Count == 0)
				{
					break;
				}

				logger.Log(LogType.Information, FormattableString.Invariant($"Found {invoiceBatch.Count} draft invoice(s) to process"));

				foreach (var (message, draftInvoice, dashInvoice) in invoiceBatch)
				{
					if (cancellationToken.IsCancellationRequested)
					{
						logger.Log(LogType.Debug, (NoResString)"Cancellation requested");
						break;
					}

					logger.Log(LogType.Debug, FormattableString.Invariant($"Processing EDIMessage (PK): {message.PK}, Dash Invoice (PK): {dashInvoice.PK}, Draft Invoice (PK): {draftInvoice.PK}"));

					try
					{
						if (draftInvoice.AIH_Status == AccDraftInvoiceHeaderStatus.Analyzing)
						{
							DraftInvoiceImporter.ImportToDraftInvoice(factory, dashInvoice, draftInvoice);
							QueueForAutoReconciliationAndPosting(draftInvoice);
						}
						else
						{
							logger.Log(LogType.Debug, FormattableString.Invariant($"Draft invoice (internal reference: {draftInvoice.AIH_InternalReference}) was not updated as the status was set to {draftInvoice.AIH_Status}"));
						}

						if (!dashEntitiesService.UpdateStatusToComplete(dashInvoice.DashDocument))
						{
							logger.Log(LogType.Debug, FormattableString.Invariant($"Dash doc: {dashInvoice.DPI_DDD_DashDocID} failed to update status to complete"));
						}

						message.EM_Status = EDIMessageStatusList.Codes.ProcessedOK;
						factory.Save();
						logger.Log(LogType.Debug, FormattableString.Invariant($"Processing draft invoice (internal reference: {draftInvoice.AIH_InternalReference}) was successful"));

						successfulItemCount++;
					}
					catch (SqlException sqlEx) when (!sqlEx.IsCriticalException())
					{
						var dbErrorHandler = new DbErrorHandler(sqlEx, ((IDbConnected)factory).Connection);
						HandleProcessingException(message, draftInvoice, dashInvoice, sqlEx, isEligibleForRetry: dbErrorHandler.ShouldBeRetried);
						processingCancelledDueToFailure = true;
						break;
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						HandleProcessingException(message, draftInvoice, dashInvoice, ex, isEligibleForRetry: false);
						processingCancelledDueToFailure = true;
						break;
					}
				}

				if (processingCancelledDueToFailure)
				{
					logger.Log(LogType.Information, FormattableString.Invariant($"Processing cancelled after {successfulItemCount}/{invoiceBatch.Count} draft invoice(s) completed successfully. 1 item failed, {invoiceBatch.Count - successfulItemCount - 1} still to be processed"));
				}
				else
				{
					logger.Log(LogType.Information, FormattableString.Invariant($"Processing finished after {successfulItemCount}/{invoiceBatch.Count} draft invoice(s) completed successfully"));
				}
			}
		}

		void HandleProcessingException(IEDIMessage originalMessage, AccDraftInvoiceHeader originalDraftInvoice, DashAPInvoice dashInvoice, Exception ex, bool isEligibleForRetry)
		{
			var factory = new BusinessObjectFactory();
			var reloadedMessage = factory.Load<IEDIMessage>(originalMessage.PK);
			var reloadedDraftInvoice = factory.Load<AccDraftInvoiceHeader>(originalDraftInvoice.PK);

			if (!isEligibleForRetry)
			{
				logger.Log(LogType.Error, FormattableString.Invariant($"Processing draft invoice (internal reference: {reloadedDraftInvoice.AIH_InternalReference}) failed. Error was not eligible for retry\r\nError message: {ex.Message}"));
				reloadedMessage.EM_Status = EDIMessageStatusList.Codes.Failed;
				reloadedDraftInvoice.AIH_Status = AccDraftInvoiceHeaderStatus.Draft;
			}
			else if (reloadedMessage.EM_RetryCount >= maxRetryCount)
			{
				logger.Log(LogType.Error, FormattableString.Invariant($"Processing draft invoice (internal reference: {reloadedDraftInvoice.AIH_InternalReference}) failed. Retry limit reached - message marked as failed\r\nError message: {ex.Message}"));
				reloadedMessage.EM_Status = EDIMessageStatusList.Codes.Failed;
				reloadedDraftInvoice.AIH_Status = AccDraftInvoiceHeaderStatus.Draft;
			}
			else
			{
				reloadedMessage.EM_RetryCount++;
				reloadedMessage.EM_HeldUntilDate = timeProvider.GetCurrentUtcDateTime().Add(minimumRetryDelay);
				logger.Log(LogType.Error, FormattableString.Invariant($"Processing draft invoice (internal reference: {reloadedDraftInvoice.AIH_InternalReference}) failed. Queued for attempt {reloadedMessage.EM_RetryCount + 1}/{maxRetryCount + 1} sometime after {reloadedMessage.EM_HeldUntilDate} UTC\r\nError message: {ex.Message}"));
			}

			factory.Save();
		}

		void QueueForAutoReconciliationAndPosting(AccDraftInvoiceHeader draftInvoice)
		{
			var message = draftInvoice.Factory.New<IEDIMessage>();
			message.EM_GB = draftInvoice.AIH_GB_Branch;
			message.EM_GE = draftInvoice.AIH_GE_Department;
			message.EM_ApplicationCode = ApplicationCodeList.Codes.AccountsPayableAutomation;
			message.EM_MessageType = EDIMessageTypeList.Codes.PIN;
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Internal;
			message.EM_Status = EDIMessageStatusList.Codes.Queued;
			message.EM_LinkUniqueID = draftInvoice.PK;
			message.EM_LinkTable = draftInvoice.TableName;
			message.EM_IsActive = true;
		}

		List<(IEDIMessage message, AccDraftInvoiceHeader draftInvoice, DashAPInvoice dashInvoice)> LoadInvoiceBatch(BusinessObjectFactory factory)
		{
			var result = new List<(IEDIMessage message, AccDraftInvoiceHeader draftInvoice, DashAPInvoice dashInvoice)>();
			var messageBatch = LoadMessages(factory);
			if (messageBatch.Length == 0)
			{
				logger.Log(LogType.Debug, (NoResString)"No messages to process.");
				return result;
			}

			var dashDocumentPks = messageBatch
				.Where(m => !m.EM_LinkUniqueID.IsDefault)
				.Select(m => m.EM_LinkUniqueID);
			var dashApInvoices = dashEntitiesService.LoadAPInvoices(dashDocumentPks, factory);
			var dashApInvoicesByDocPk = dashApInvoices.ToDictionary(d => d.DPI_DDD_DashDocID);

			var apInvoicePks = dashApInvoices.Select(d => d.DashDocument.DDD_RelatedEntityID);
			var draftInvoices = factory.Load<AccDraftInvoiceHeader>(new ZQuery(AccDraftInvoiceHeaderSchema.PK, apInvoicePks));
			var draftInvoicesByPk = draftInvoices.ToDictionary(d => d.PK);

			foreach (var message in messageBatch)
			{
				if (!dashApInvoicesByDocPk.TryGetValue(message.EM_LinkUniqueID, out var dashInvoice))
				{
					ErrorReporter.ReportOnce($"{GetType()}_NoDashAPInvoice", $"The EDI Message {message.PK} is not linked to a Dash AP Invoice.");
					message.EM_Status = EDIMessageStatusList.Codes.Error;
					continue;
				}

				if (!draftInvoicesByPk.TryGetValue(dashInvoice.DashDocument.DDD_RelatedEntityID, out var draftInvoice))
				{
					ErrorReporter.ReportOnce($"{GetType()}_NoAccDraftInvoiceHeader", $"The Dash Document {dashInvoice.DashDocument.PK} is not linked to a Draft Invoice.");
					message.EM_Status = EDIMessageStatusList.Codes.Error;
					continue;
				}

				result.Add(new (message, draftInvoice, dashInvoice));
			}

			if (result.Count != messageBatch.Length)
			{
				//Save any messages that have been updated with EM_Status = Error above (which is not expected to ever happen,
				//but we need to make sure these messages are kicked off the queue if it does).
				factory.Save();
			}

			return result;
		}

		IEDIMessage[] LoadMessages(BusinessObjectFactory factory)
		{
			var query = new ZDBOnlyQuery(typeof(IEDIMessage)) { MaximumRows = batchSize }
				.AddToFilter(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.DashAccountingImport)
				.AddToFilter(EDIMessageSchema.EM_MessageType, EDIMessageTypeList.Codes.API)
				.AddToFilter(EDIMessageSchema.EM_IsActive, true)
				.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Internal)
				.AddToFilter(EDIMessageSchema.EM_Status, EDIMessageStatusList.Codes.Queued);
			var validDateTimeSubQuery = new ZQuery(EDIMessageSchema.EM_HeldUntilDate, SQLComparisonOperator.Equal, null);
			validDateTimeSubQuery.AddToFilter(JoinCondition.Or, EDIMessageSchema.EM_HeldUntilDate, SQLComparisonOperator.LessThanOrEqualTo, timeProvider.GetCurrentUtcDateTime());
			query.AddToFilter(validDateTimeSubQuery);

			query.TableIndexHints.Add(new TableIndexHint(EDIMessageSchema.Constants.Indexes.NR_RX__EM_ApplicationCode_EM_ReceiveTransmit_EM_MessageNum_EM_SystemCreateTimeUtc));

			return factory.Load<IEDIMessage>(query);
		}
	}
}
