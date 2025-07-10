using System;
using Enterprise.Accounting.Business;

namespace Enterprise.Accounting.ServiceTasks.JobCostingReport.DBObjects
{
	class JobCostingDataAmountByJobIndexByJH : JCDDependentDBObject
	{
		public JobCostingDataAmountByJobIndexByJH()
			: base(JCDDBObjectInfoList.Instance["RptDt_NX_JCA_JH_JCA_GC_JCA_OH"])
		{
		}

		public override string Name => FormattableString.Invariant($"INDEX {base.Name}");

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "SQL command")]
		public override string CreateSQLText
			=> @"
CREATE INDEX RptDt_NX_JCA_JH_JCA_GC_JCA_OH
	ON dbo.RptDtJobCostingDataAmountByJob (JCA_JH, JCA_GC, JCA_OH_DebtorOrCreditor)
	INCLUDE (JCA_PostPeriod, JCA_RX_NKLocalCurrency, JCA_Revenue, JCA_Cost);
";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "SQL command")]
		public override string CheckAndDropSQLText => @"
DROP INDEX IF EXISTS RptDt_NX_JCA_JH_JCA_GC_JCA_OH
	ON dbo.RptDtJobCostingDataAmountByJob;
";
	}
}
