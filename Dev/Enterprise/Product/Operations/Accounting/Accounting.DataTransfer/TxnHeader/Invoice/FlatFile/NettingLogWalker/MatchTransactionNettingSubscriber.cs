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
	public class MatchTransactionNettingSubscriber : LogSubscriber
	{
		public override string Name => "MatchTransactionNettingTransmitter";

		public override string[] TableNames => new string[] { AccTransactionHeaderSchema.Constants.TableName };

		public override string[] EventTypes => new string[] { Events.EditedARecord.Code };

		protected override void ProcessLogQueueItems(IQueuedLog[] queuedLogs)
		{
			if (queuedLogs.Any())
			{
				var candidateLogs = queuedLogs.Where(log => HelperMethods.GetStatus(log.SJ_Reference) == AccountingConstants.InvoiceAdditionalReference.FullyMatched
																					|| HelperMethods.GetStatus(log.SJ_Reference) == AccountingConstants.InvoiceAdditionalReference.UndoFullyMatched);
				foreach (var log in candidateLogs)
				{
					var factory = log.Factory;
					if (factory.Load<TransactionHeader>(log.SJ_ParentID) is InvoicingBase transaction && transaction.Company.FirstActiveBranch != null)
					{
						UniversalTransactionTransmitter.UniversalTransmitForNettingSystem(GetINotificationsWrapperAroundILogger(), factory, transaction, HelperMethods.GetStatus(log.SJ_Reference));
					}
				}
			}
		}

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
	}
}
