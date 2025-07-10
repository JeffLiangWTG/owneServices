using System;
using Enterprise.Accounting.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.ServiceTasks.JobCostingReport.DBObjects
{
	class RptDt_JobCostingDataIndexByOH : JCDDependentDBObject
	{
		public RptDt_JobCostingDataIndexByOH()
			: base(JCDDBObjectInfoList.Instance["RptDt_NI_JCD_OH_JCD_GC_JCD_JH"])
		{
		}

		public override string Name => FormattableString.Invariant($"INDEX {base.Name}");

		// This object has been tomb-stoned and will not be created.
		public override string CreateSQLText => string.Empty;

		public override string CheckAndDropSQLText => (NoResString)"DROP INDEX IF EXISTS RptDt_ViewJobCostingDataAmountByJob.RptDt_NI_JCD_OH_JCD_GC_JCD_JH;";
	}
}
