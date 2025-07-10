using System;
using Enterprise.Accounting.Business;

namespace Enterprise.Accounting.ServiceTasks.JobCostingReport.DBObjects
{
	class RptDtJobCostingDataAmountByJob : JCDDependentDBObject
	{
		public RptDtJobCostingDataAmountByJob()
			: base(JCDDBObjectInfoList.Instance["RptDtJobCostingDataAmountByJob"])
		{
		}

		public override string Name => FormattableString.Invariant($"TABLE {base.Name}");

		public override string CreateSQLText
			=> GetEmbeddedResourceText("Enterprise.Accounting.ServiceTasks.JobCostingReport.DBObjects.FunctionsAndProcedures.RptDtJobCostingDataAmountByJob.sql");

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "SQL command")]
		public override string CheckAndDropSQLText => @"
DROP TABLE IF EXISTS dbo.RptDtJobCostingDataAmountByJob
";
	}
}
