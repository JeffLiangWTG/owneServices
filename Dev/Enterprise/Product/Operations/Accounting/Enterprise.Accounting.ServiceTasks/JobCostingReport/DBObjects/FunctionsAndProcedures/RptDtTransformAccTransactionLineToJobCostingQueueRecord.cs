using System;
using Enterprise.Accounting.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.ServiceTasks.JobCostingReport.DBObjects
{
	public class RptDtTransformAccTransactionLineToJobCostingQueueRecord : JCDDependentDBObject
	{
		public RptDtTransformAccTransactionLineToJobCostingQueueRecord()
			: base(JCDDBObjectInfoList.Instance["RptDtTransformAccTransactionLineToJobCostingQueueRecord"])
		{
		}

		public override string Name => FormattableString.Invariant($"PROCEDURE {base.Name}");

		public override string CreateSQLText => GetEmbeddedResourceText("Enterprise.Accounting.ServiceTasks.JobCostingReport.DBObjects.FunctionsAndProcedures.RptDtTransformAccTransactionLineToJobCostingQueueRecord.sql");

		public override string CheckAndDropSQLText => (NoResString)@" IF OBJECT_ID(N'RptDtTransformAccTransactionLineToJobCostingQueueRecord') IS NOT NULL
														BEGIN
															DROP PROCEDURE RptDtTransformAccTransactionLineToJobCostingQueueRecord
														END";
	}
}
