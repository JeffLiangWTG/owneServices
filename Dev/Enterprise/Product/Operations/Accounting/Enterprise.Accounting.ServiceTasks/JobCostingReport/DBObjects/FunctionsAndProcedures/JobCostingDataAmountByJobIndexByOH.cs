using System;
using Enterprise.Accounting.Business;

namespace Enterprise.Accounting.ServiceTasks.JobCostingReport.DBObjects
{
	class JobCostingDataAmountByJobIndexByOH : JCDDependentDBObject
	{
		public JobCostingDataAmountByJobIndexByOH()
			: base(JCDDBObjectInfoList.Instance["RptDt_NX_JCA_OH_JCA_GC_JCA_JH"])
		{
		}

		public override string Name => FormattableString.Invariant($"INDEX {base.Name}");

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "SQL command")]
		public override string CreateSQLText
			=> @"
CREATE INDEX RptDt_NX_JCA_OH_JCA_GC_JCA_JH
	ON dbo.RptDtJobCostingDataAmountByJob (JCA_OH_DebtorOrCreditor, JCA_GC, JCA_JH)
	INCLUDE (JCA_PostPeriod, JCA_RX_NKLocalCurrency, JCA_Revenue, JCA_Cost);
";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "SQL command")]
		public override string CheckAndDropSQLText => @"
DROP INDEX IF EXISTS RptDt_NX_JCA_OH_JCA_GC_JCA_JH
	ON dbo.RptDtJobCostingDataAmountByJob;
";
	}
}
