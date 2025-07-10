using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.EventMessageProcessor;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing
{
	/// <summary>
	/// Generic class for processing XUE message with multiple transactions in the batch.
	/// This will copy data from XUE context fields to database fields in CW1 based on the naming convention defined in ContextTypeCode class.
	/// </summary>
	internal class TransactionBatchEventMessageProcessor : EInvoicingEventMessageProcessor
	{
		protected readonly ICountryEInvoicingObjectFactory CountryFactory;

		public TransactionBatchEventMessageProcessor(EventMessageProcessorData eventMessageProcessorData, ICountryEInvoicingObjectFactory countryEInvoicingObjectFactory)
			: base(eventMessageProcessorData.Logger, eventMessageProcessorData.Message, eventMessageProcessorData.UniversalEvent, eventMessageProcessorData.InvoiceBatch)
		{
			CountryFactory = Argument.NotNull(countryEInvoicingObjectFactory, nameof(countryEInvoicingObjectFactory));
		}

		#region Transactions

		protected List<EventAndDatabaseTransaction> Transactions
			=> fTransactions ?? (fTransactions = LoadTransactions());

		List<EventAndDatabaseTransaction> fTransactions;

		protected virtual List<EventAndDatabaseTransaction> LoadTransactions()
		{
			var eventTransactions = UniversalEventTransactionDataObject.FromJsonInContextCollection(universalEvent, out var hasJsonError);
			if (hasJsonError)
			{
				LogError(Res.GetString("GlobalEInvoicingEventMessageProcessor|JsonParseError", "JSON parse error detected, one or more {0} values will not be processed for invoice batch {1} in {2}.", EventContextTypeCode.BatchResponseObject, invoiceBatch.AIB_BatchNumber, companyName));
			}

			var transactionPks = invoiceBatch.TransactionPivots.Cast<AccEInvoicingTransactionPivot>().Select(x => x.AIP_ParentID).ToArray();
			var dbTransactions = invoiceBatch.Factory.Load<InvoicingBase>(new ZQuery(AccTransactionHeaderSchema.PK, transactionPks));

			var authRecordQuery = new ZQuery(AccTransactionHeaderAuthorisationRecordSchema.AHF_ParentId, transactionPks);
			authRecordQuery.AddToFilter(AccTransactionHeaderAuthorisationRecordSchema.AHF_ParentTableCode, AccTransactionHeaderSchema.Constants.Prefix);
			authRecordQuery.AddToFilter(AccTransactionHeaderAuthorisationRecordSchema.AHF_RecordType, CountryFactory.AuthorizationRecordType);
			var authRecords = invoiceBatch.Factory.Load<AccTransactionHeaderAuthorisationRecord>(authRecordQuery);

			if (invoiceBatch.TransactionPivots.Count != dbTransactions.Length)
			{
				ReportAndLogError("GlobalEInvoicingEventMessageProcessor|BatchPivotAndTransactionCountUnequal", FormattableString.Invariant($"For batch {invoiceBatch.AIB_BatchNumber} in company {companyName}, there are {invoiceBatch.TransactionPivots.Count:N0} pivots, but {dbTransactions.Length:N0} transactions found in database."));  // Error Message for Developers Only
			}

			var pivotsByAHPK = invoiceBatch.TransactionPivots.Cast<AccEInvoicingTransactionPivot>().ToDictionary(p => p.AIP_ParentID, p => p);
			var uniqueIdentifier = (ITransactionInfoHelper)new TransactionInfoHelper();
			var transactionsByUniqueId = dbTransactions.ToDictionary(uniqueIdentifier.GetUniqueIdentifierWithinBatch, t => t);
			var authRecordsByAHPK = authRecords.ToDictionary(t => t.AHF_ParentId, t => t);

			var result = new List<EventAndDatabaseTransaction>(dbTransactions.Length);
			var correlatedTransactionKeys = new HashSet<string>(eventTransactions.Count);
			foreach (var et in eventTransactions)
			{
				var transactionKey = uniqueIdentifier.GetUniqueIdentifierWithinBatch(et);
				transactionsByUniqueId.TryGetValue(transactionKey, out var transaction);
				pivotsByAHPK.TryGetValue(transaction?.PK ?? ZGuid.Empty, out var pivot);
				if (transaction != null && pivot != null)
				{
					correlatedTransactionKeys.Add(transactionKey);
				}
				authRecordsByAHPK.TryGetValue(transaction?.PK ?? ZGuid.Empty, out var authRecord);

				result.Add(new EventAndDatabaseTransaction()
				{
					ParentType = typeof(TransactionBatchEventMessageProcessor),
					AuthRecordType = CountryFactory.AuthorizationRecordType,
					CompanyName = companyName,
					UniversalEvent = universalEvent,
					Batch = invoiceBatch,
					EventData = et,
					Pivot = pivot,
					Transaction = transaction,
					AuthRecord = authRecord,
				});
			}

			foreach (var t in dbTransactions)
			{
				var transactionKey = uniqueIdentifier.GetUniqueIdentifierWithinBatch(t);
				if (!correlatedTransactionKeys.Contains(transactionKey))
				{
					pivotsByAHPK.TryGetValue(t.PK, out var pivot);
					authRecordsByAHPK.TryGetValue(t.PK, out var authRecord);

					result.Add(new EventAndDatabaseTransaction()
					{
						ParentType = typeof(TransactionBatchEventMessageProcessor),
						AuthRecordType = CountryFactory.AuthorizationRecordType,
						CompanyName = companyName,
						UniversalEvent = universalEvent,
						Batch = invoiceBatch,
						EventData = null,
						Pivot = pivot,
						Transaction = t,
						AuthRecord = authRecord,
					});
				}
			}

			return result;
		}

		#endregion Transactions

		public override void Process()
		{
			if (invoiceBatch.AIB_Status == EInvoicingBatchState.Discarded)
			{
				return;
			}

			MapBatchLevelContextValues();

			if (universalEvent.EventType.Value == AutoEvents.InterchangeAcknowledgedCode)
			{
				ProcessIAKEventMessage();
			}
			else
			{
				ProcessIRJEventMessage();
			}

			CountryFactory.GetGlobalXUEFunctionalityProvider().AfterEventMessageProcessed(universalEvent, invoiceBatch, logger);
		}

		protected void MapBatchLevelContextValues()
		{
			var contextCollection = UniversalEventTransactionDataObject.GetContextCollectionSafe(universalEvent);
#pragma warning disable CS0618 // Type or member is obsolete: backward compatibility of Legacy_GovernmentAllocatedNumber / EINV_GovtAllocatedRefNumber
			if (contextCollection.TryGetValue(EventContextTypeCode.Legacy_GovernmentAllocatedNumber, out var governmentAllocatedNumber))
			{
				invoiceBatch.AIB_GovernmentAllocatedNumber = governmentAllocatedNumber;
			}
#pragma warning restore CS0618 // Type or member is obsolete
			if (contextCollection.TryGetValue(EventContextTypeCode.AIB_GovernmentAllocatedNumber_Explicit, out governmentAllocatedNumber))
			{
				invoiceBatch.AIB_GovernmentAllocatedNumber = governmentAllocatedNumber;
			}

			if (contextCollection.TryGetValue(EventContextTypeCode.AIB_EHubAllocatedNumber, out var ehubAllocatedNumber))
			{
				invoiceBatch.AIB_EHubAllocatedNumber = ehubAllocatedNumber;
			}
		}

		protected virtual void ProcessIRJEventMessage()
		{
			var errorMessage = universalEvent.EventParameters?.Reason ?? ZString.Empty;
			errorMessage = EventProcessorHelper.CheckLengthAndTruncIfNeeded(invoiceBatch.Company, errorMessage);

			foreach (AccEInvoicingTransactionPivot pivot in invoiceBatch.TransactionPivots)
			{
				pivot.AIP_Status = PivotStatusForIRJ;
				pivot.AIP_ErrorDescription = errorMessage;
				pivot.AIP_LastResponseReceivedUtc = LastResponseReceivedTime;
			}

			SendErrorNotificationEmail(errorMessage);
		}

		protected virtual void ProcessIAKEventMessage()
		{
			// 🚩🚩🚩
			// All changes to XUE structure must be compatible with XUE wiki pages.
			// https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki/13092/XUE-Reference
			// https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki/13099/XUE-Message-Many-Transactions
			// If you need to extend the functionality of the XUE message, make changes on the wiki and ask Murray for a review!
			// 🚩🚩🚩

			foreach (var t in Transactions)
			{
				if (t.ValidatePivotAndBatch(logger))
				{
					t.MapGovernmentAllocatedIdToDatabase(logger);
					t.MapComplianceNumberToDatabase(logger);
					t.MapAuthorisationRecordToDatabase(logger);

					t.Pivot.AIP_Status = GetPivotStatusForIAK(t);
					t.Pivot.AIP_ErrorDescription = EventProcessorHelper.CheckLengthAndTruncIfNeeded(invoiceBatch.Company, t.EventData.FailureReason);
					t.Pivot.AIP_LastResponseReceivedUtc = LastResponseReceivedTime;
				}
			}

			if (AnyTransactionsWithError())
			{
				SendErrorNotificationEmail(ZString.Empty);
			}
		}

		protected virtual ZString GetPivotStatusForIAK(EventAndDatabaseTransaction eventAndDatabaseTransaction) => eventAndDatabaseTransaction.EventData.PivotStatus;

		protected virtual ZString PivotStatusForIRJ => EInvoicingPivotState.Failed;

		protected override bool ShouldSendEmail
			=> universalEvent.EventType.Value == AutoEvents.InterchangeRejectedCode
			|| AnyTransactionsWithError();

		protected override string LogErrorNotFoundKey => "TransactionNotFound"; // Error Message Key for Developers Only

		protected override GEIEmailNotificationCreator GetEmailNotificationCreator(ZString errorMessage, ILogger logCollector)
		{
			if (!errorMessage.IsEmpty)
			{
				var relatedTransactions = invoiceBatch.TransactionPivots.Cast<AccEInvoicingTransactionPivot>().Select(p => p.ParentTransactionHeader).ToArray();
				return new GEIEmailNotificationCreator(ediMessage, invoiceBatch, new [] { errorMessage }, relatedTransactions, logCollector);
			}
			else
			{
				var errorTransactions = Transactions
									.Where(TransactionsWithErrorPredicate)
									.Select(t => IncorrectTransactionDetails.FromBizo(t.Transaction, new[] { (ZString)t.EventData.FailureReason }))
									.ToList();
				if (errorTransactions.Count == 0)
				{
					return null;
				}
				return new GEIEmailNotificationCreator(ediMessage, errorTransactions, invoiceBatch.Company, logCollector);
			}
		}

		bool AnyTransactionsWithError()
			=> Transactions.Any(TransactionsWithErrorPredicate);

		static bool TransactionsWithErrorPredicate(EventAndDatabaseTransaction t)
			=> t.Transaction != null && t.EventData?.PivotStatus == EInvoicingPivotState.Failed;
	}
}
