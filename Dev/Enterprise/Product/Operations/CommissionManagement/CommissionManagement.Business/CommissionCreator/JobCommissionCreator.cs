using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.CommissionManagement.Business
{
	public class JobCommissionCreator : CommissionCreator
	{
		public JobCommissionCreator(JobHeader job, DataTable effectiveDateCacheTable = null)
			: base(job.Factory)
		{
			this.job = job;
			this.effectiveDateCacheTable = effectiveDateCacheTable;
		}

		readonly JobHeader job;
		readonly DataTable effectiveDateCacheTable;

		#region Create Commissions

		public override void CreateCommissions(CreateCommissionContext context)
		{
			var jobCloseLogsQuery = new ZQuery(StmALogSchema.SL_Parent, job.PK);
			jobCloseLogsQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, AutoEvents.JobCloseCode);
			jobCloseLogsQuery.AddToFilter(StmALogSchema.SL_EventTime, SQLComparisonOperator.GreaterThanOrEqualTo, context.FromDate);
			var jobCloseLogs = Factory.Load<StmALog>(jobCloseLogsQuery);
			var jobCloseLogsByDate = jobCloseLogs.Select(x => Tuple.Create(x.SL_EventTime, (BusinessObject)x));

			var jobTransactionsQuery = JobRelatedTransactionCommissionCreator.GetJobRelatedTransactionsQuery(job);
			jobTransactionsQuery.AddToFilter(AccTransactionHeaderSchema.AH_PostDate, SQLComparisonOperator.GreaterThanOrEqualTo, context.FromDate);
			jobTransactionsQuery.AddToFilter(AccTransactionHeaderSchema.AH_GC, job.JH_GC);
			var jobTransactions = Factory.Load<TransactionHeader>(jobTransactionsQuery).OfType<ICommissionableTransaction>();
			var jobTransactionsByDate = jobTransactions.Select(x => Tuple.Create(x.AH_PostDate, (BusinessObject)x));

			ReversalTransactionCommissionCreator.AddReversalTransactionFetchHints(Factory, jobTransactions);

			var snapshotEventsOrderedByTime = jobCloseLogsByDate.Union(jobTransactionsByDate).OrderBy(x => x.Item1);
			foreach (var snapshotEvent in snapshotEventsOrderedByTime)
			{
				var jobCloseLog = snapshotEvent.Item2 as StmALog;
				if (jobCloseLog != null)
				{
					CreateJobClosedCommissions(job, jobCloseLog, context);
				}
				else
				{
					var jobTransaction = snapshotEvent.Item2 as ICommissionableTransaction;
					if (jobTransaction != null)
					{
						CreateJobRelatedTransactionCommissions(jobTransaction, context);
					}
				}
			}
		}

		protected virtual void CreateJobClosedCommissions(JobHeader job, StmALog jobCloseLog, CreateCommissionContext context)
		{
			new JobClosedCommissionCreator(job, jobCloseLog.SL_EventTime, effectiveDateCacheTable).CreateCommissions(context);
		}

		protected virtual void CreateJobRelatedTransactionCommissions(ICommissionableTransaction transaction, CreateCommissionContext context)
		{
			new JobRelatedTransactionCommissionCreator(transaction, null, effectiveDateCacheTable).CreateCommissions(context);
		}

		#endregion
	}
}
