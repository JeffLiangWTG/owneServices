using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.EventMessageProcessor;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing
{
	/// <summary>
	/// Generic class for processing XUE message with single transaction.
	/// This will copy data from XUE context fields to database fields in CW1 based on the naming convention defined in ContextTypeCode class.
	/// </summary>
	internal class GlobalEInvoicingEventMessageProcessor : TransactionBatchEventMessageProcessor
	{
		public GlobalEInvoicingEventMessageProcessor(EventMessageProcessorData eventMessageProcessorData, ICountryEInvoicingObjectFactory countryEInvoicingObjectFactory)
			: base(eventMessageProcessorData, countryEInvoicingObjectFactory)
		{
		}

		#region Transactions

		protected override List<EventAndDatabaseTransaction> LoadTransactions()
		{
			var eventAndDatabaseTransaction = new EventAndDatabaseTransaction()
			{
				ParentType = typeof(GlobalEInvoicingEventMessageProcessor),
				AuthRecordType = CountryFactory.AuthorizationRecordType,
				CompanyName = companyName,
				UniversalEvent = universalEvent,
				Batch = invoiceBatch,
				EventData = UniversalEventTransactionDataObject.FromUniversalEvent(universalEvent),
				Pivot = invoiceBatch?.TransactionPivots?.Cast<AccEInvoicingTransactionPivot>().FirstOrDefault(),
			};
			if (eventAndDatabaseTransaction.Pivot != null)
			{
				eventAndDatabaseTransaction.Transaction = invoiceBatch.Factory.Load<InvoicingBase>(eventAndDatabaseTransaction.Pivot.AIP_ParentID);
			}
			if (eventAndDatabaseTransaction.Transaction != null)
			{
				var filter = new ZQuery(AccTransactionHeaderAuthorisationRecordSchema.AHF_ParentTableCode, AccTransactionHeaderSchema.Constants.Prefix);
				filter.AddToFilter(JoinCondition.And, AccTransactionHeaderAuthorisationRecordSchema.AHF_ParentId, SQLComparisonOperator.Equal, eventAndDatabaseTransaction.Transaction.PK);
				filter.AddToFilter(JoinCondition.And, AccTransactionHeaderAuthorisationRecordSchema.AHF_RecordType, SQLComparisonOperator.Equal, eventAndDatabaseTransaction.AuthRecordType);
				eventAndDatabaseTransaction.AuthRecord = invoiceBatch.Factory.LoadTop1<AccTransactionHeaderAuthorisationRecord>(filter);
			}
			return new List<EventAndDatabaseTransaction>() { eventAndDatabaseTransaction };
		}

		protected EventAndDatabaseTransaction EventAndDatabaseTransaction => Transactions.Single();

		#endregion Transactions

		public override void Process()
		{
			// Different validation rules apply for a single transaction, which are applied before the usual processing logic in base class.
			if (!EventAndDatabaseTransaction.ValidatePivotAndBatch(logger))
			{
				return;
			}

			base.Process();
		}

		protected override void ProcessIAKEventMessage()
		{
			// 🚩🚩🚩
			// All changes to XUE structure must be compatible with XUE wiki pages.
			// https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki/13092/XUE-Reference
			// https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki/13099/XUE-Message-Many-Transactions
			// If you need to extend the functionality of the XUE message, make changes on the wiki and ask Murray for a review!
			// 🚩🚩🚩

			EventAndDatabaseTransaction.MapGovernmentAllocatedIdToDatabase(logger);
			EventAndDatabaseTransaction.MapComplianceNumberToDatabase(logger);
			EventAndDatabaseTransaction.MapComplianceDateToDatabase(logger);
			EventAndDatabaseTransaction.MapComplianceSubTypeToDatabase(logger);
			EventAndDatabaseTransaction.MapComplianceDocumentStatusToDatabase();
			EventAndDatabaseTransaction.MapAuthorisationRecordToDatabase(logger);

			invoiceBatch.UpdateBatchAndPivotStatusAndErrorDescription(new[] { EventAndDatabaseTransaction.Transaction.PK }, EventAndDatabaseTransaction.EventData.FailureReason, PivotStatusForIAK, lastResponseReceivedUtc: LastResponseReceivedTime);
		}

		protected virtual ZString PivotStatusForIAK => EventAndDatabaseTransaction.EventData.PivotStatus;

		protected override ZString PivotStatusForIRJ => EventAndDatabaseTransaction.EventData.PivotStatus;

		protected override bool ShouldSendEmail => EventAndDatabaseTransaction.Transaction != null;

		protected override string LogErrorNotFoundKey => "TransactionNotFound"; // Error Message Key for Developers Only

		protected override GEIEmailNotificationCreator GetEmailNotificationCreator(ZString errorMessage, ILogger logCollector)
		{
			return new GEIEmailNotificationCreator(ediMessage, EventAndDatabaseTransaction.Transaction, new [] { errorMessage }, logCollector);
		}
	}
}
