using System;
using Enterprise.Accounting.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.ServiceTasks.JobCostingReport.DBObjects
{
	internal class RptDtPopulateJobCostingDataFromQueue : JCDDependentDBObject
	{
		public RptDtPopulateJobCostingDataFromQueue()
			: base(JCDDBObjectInfoList.Instance["RptDtPopulateJobCostingDataFromQueue"])
		{
		}

		public override string Name => FormattableString.Invariant($"PROCEDURE {base.Name}");

		public override string CreateSQLText => GetEmbeddedResourceText("Enterprise.Accounting.ServiceTasks.JobCostingReport.DBObjects.FunctionsAndProcedures.RptDtPopulateJobCostingDataFromQueue.sql");

		public override string CheckAndDropSQLText => (NoResString)@" IF OBJECT_ID(N'RptDtPopulateJobCostingDataFromQueue') IS NOT NULL
														BEGIN
															DROP PROCEDURE RptDtPopulateJobCostingDataFromQueue
														END";
	}
}
