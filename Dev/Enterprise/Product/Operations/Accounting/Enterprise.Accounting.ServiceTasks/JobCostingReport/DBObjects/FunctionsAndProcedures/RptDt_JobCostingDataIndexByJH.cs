using System;
using Enterprise.Accounting.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.ServiceTasks.JobCostingReport.DBObjects
{
	class RptDt_JobCostingDataIndexByJH : JCDDependentDBObject
	{
		public RptDt_JobCostingDataIndexByJH()
			: base(JCDDBObjectInfoList.Instance["RptDt_NI_JCD_JH_JCD_GC_JCD_OH"])
		{
		}

		public override string Name => FormattableString.Invariant($"INDEX {base.Name}");

		// This object has been tomb-stoned and will not be created.
		public override string CreateSQLText => string.Empty;

		public override string CheckAndDropSQLText => (NoResString)"DROP INDEX IF EXISTS RptDt_ViewJobCostingDataAmountByJob.RptDt_NI_JCD_JH_JCD_GC_JCD_OH;";
	}
}
