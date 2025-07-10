using System;
using Enterprise.Accounting.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.ServiceTasks.JobCostingReport.DBObjects
{
	class RptDt_GlobalJobProfitReportCore : JCDDependentDBObject
	{
		public RptDt_GlobalJobProfitReportCore()
			: base(JCDDBObjectInfoList.Instance["RptDt_GlobalJobProfitReportCore"])
		{
		}

		public override string Name => FormattableString.Invariant($"FUNCTION {base.Name}");

		public override string CreateSQLText => GetEmbeddedResourceText(resourceName);

		public override string CheckAndDropSQLText => (NoResString)@" IF OBJECT_ID(N'RptDt_GlobalJobProfitReportCore') IS NOT NULL
														BEGIN
															DROP FUNCTION RptDt_GlobalJobProfitReportCore
														END";

		const string resourceName = "Enterprise.Accounting.ServiceTasks.JobCostingReport.DBObjects.FunctionsAndProcedures.RptDt_GlobalJobProfitReportCore.sql";
	}
}
