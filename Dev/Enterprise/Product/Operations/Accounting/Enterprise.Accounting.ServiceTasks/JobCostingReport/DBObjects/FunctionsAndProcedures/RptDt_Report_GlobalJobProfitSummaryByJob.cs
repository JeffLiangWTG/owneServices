using System;
using Enterprise.Accounting.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.ServiceTasks.JobCostingReport.DBObjects
{
	public class RptDt_Report_GlobalJobProfitSummaryByJob : JCDDependentDBObject
	{
		public RptDt_Report_GlobalJobProfitSummaryByJob()
			: base(JCDDBObjectInfoList.Instance["RptDt_Report_GlobalJobProfitSummaryByJob"])
		{
		}

		public override string Name => FormattableString.Invariant($"FUNCTION {base.Name}");

		public override string CreateSQLText => GetEmbeddedResourceText("Enterprise.Accounting.ServiceTasks.JobCostingReport.DBObjects.FunctionsAndProcedures.RptDt_Report_GlobalJobProfitSummaryByJob.sql");

		public override string CheckAndDropSQLText => (NoResString)@" IF OBJECT_ID(N'RptDt_Report_GlobalJobProfitSummaryByJob') IS NOT NULL
														BEGIN
															DROP FUNCTION RptDt_Report_GlobalJobProfitSummaryByJob
														END";
	}
}
