using System;
using Enterprise.Accounting.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.ServiceTasks.JobCostingReport.DBObjects
{
	class RptDt_JobCostingDataClusteredIndex : JCDDependentDBObject
	{
		public RptDt_JobCostingDataClusteredIndex()
			: base(JCDDBObjectInfoList.Instance["RptDt_CI_JCD_PostPeriod_JCD_GC_JCD_OH_JCD_JH"])
		{
		}

		public override string Name => FormattableString.Invariant($"UNIQUE CLUSTERED INDEX {base.Name}");

		// This object has been tomb-stoned and will not be created.
		public override string CreateSQLText => string.Empty;

		// RptDt_CI_JCD_JH is a legacy name for this index
		public override string CheckAndDropSQLText => (NoResString)@"
DROP INDEX IF EXISTS RptDt_ViewJobCostingDataAmountByJob.RptDt_CI_JCD_JH;
DROP INDEX IF EXISTS RptDt_ViewJobCostingDataAmountByJob.RptDt_CI_JCD_PostPeriod_JCD_GC_JCD_OH_JCD_JH;
";
	}
}
