using System;
using Enterprise.Accounting.Business;

namespace Enterprise.Accounting.ServiceTasks.JobCostingReport.DBObjects
{
	class JobCostingDataAmountByJobClusteredIndex_Obsolete : JCDDependentDBObject
	{
		public JobCostingDataAmountByJobClusteredIndex_Obsolete()
			: base(JCDDBObjectInfoList.Instance["RptDt_UC_JCA_PostPeriod_JCA_GC_JCA_OH_JCA_JH"])
		{
		}

		public override string Name => FormattableString.Invariant($"UNIQUE CLUSTERED INDEX {base.Name}");

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "SQL command")]
		public override string CreateSQLText
			=> @"
CREATE UNIQUE CLUSTERED INDEX RptDt_UC_JCA_PostPeriod_JCA_GC_JCA_OH_JCA_JH
	ON dbo.RptDtJobCostingDataAmountByJob (JCA_PostPeriod, JCA_GC, JCA_OH_DebtorOrCreditor, JCA_JH);
";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "SQL command")]
		public override string CheckAndDropSQLText => @"
DROP INDEX IF EXISTS RptDt_UC_JCA_PostPeriod_JCA_GC_JCA_OH_JCA_JH
	ON dbo.RptDtJobCostingDataAmountByJob;
";
	}
}
