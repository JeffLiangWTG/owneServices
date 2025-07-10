using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.DataTransfer.Universal.Netting;
using Enterprise.Environment;
using Enterprise.LogWalker;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BatchProcessor.Accounting
{
	[Serializable]
	public class TransactionNettingSubscriber : LogSubscriber
	{
		public const string TransactionNettingTransmitter = "TransactionNettingTransmitter";

		public override string Name => TransactionNettingTransmitter;

		public override string[] EventTypes => new string[] { Events.AddedARecordToTheSystem.Code, Events.EditedARecord.Code };

		public override string[] TableNames => new string[] { AccTransactionHeaderSchema.Constants.TableName };

		protected override ILogBatcher GetLogBatcher() => new LogBatcher();

		class LogBatcher : LogBatcher<ZGuid>
		{
			protected override void AddGroupingFetchHints(IEnumerable<IQueuedLog> enumberable) { }
			protected override ZGuid GetGroupLogKey(IQueuedLog log) => log.SJ_ParentID;
			protected override LogsGroupContext SetContextForLogsGroup(ZGuid groupKey, IEnumerable<IQueuedLog> queuedLogs)
			{
				var firstLog = queuedLogs.First(); // Guaranteed to exist.
				var factory = firstLog.Factory;
				if (factory.Load<TransactionHeader>(queuedLogs.First().SJ_ParentID) is InvoicingBase transaction && transaction.Company.FirstActiveBranch != null)
				{
					return new LogsGroupContext(false, new TemporaryUserContext() { BranchPK = transaction.Company.FirstActiveBranch.PK.ToGuid(), DepartmentPK = GlbDepartment.CurrentDepartment.PK.ToGuid() }.Set());
				}
				else
				{
					return new LogsGroupContext(false);
				}
			}
		}

		protected override void ProcessLogQueueItems(IQueuedLog[] queuedLogs)
		{
			var transactionsWithReference = (from log in queuedLogs
											 where log.SJ_ParentTableCode == AccTransactionHeaderSchema.Constants.Prefix && IsAllowedReference(log.SJ_Reference)
											 select new { log.SJ_ParentID, Reference = HelperMethods.GetStatus(log.SJ_Reference) }).ToList();
			var factory = queuedLogs[0].Factory;
			foreach (var transactionWithReference in transactionsWithReference)
			{
				if (factory.Load<TransactionHeader>(transactionWithReference.SJ_ParentID) is InvoicingBase transaction && transaction.Company.FirstActiveBranch != null)
				{
					UniversalTransactionTransmitter.UniversalTransmitForNettingSystem(GetINotificationsWrapperAroundILogger(), factory, transaction, transactionWithReference.Reference);
				}
			}
		}

		bool IsAllowedReference(ZString fullReferenceText)
		{
			var reference = HelperMethods.GetStatus(fullReferenceText);
			return new ZString[]
			{
				AccountingConstants.InvoiceAdditionalReference.Posted,
				AccountingConstants.InvoiceAdditionalReference.PostedAndFullyMatched,
				AccountingConstants.InvoiceAdditionalReference.Reversed
			}.Contains(reference);
		}
	}
}
