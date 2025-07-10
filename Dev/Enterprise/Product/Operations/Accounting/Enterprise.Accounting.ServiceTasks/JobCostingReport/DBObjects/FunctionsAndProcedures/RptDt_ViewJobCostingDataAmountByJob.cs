using System;
using Enterprise.Accounting.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.ServiceTasks.JobCostingReport.DBObjects
{
	class RptDt_ViewJobCostingDataAmountByJob : JCDDependentDBObject
	{
		public RptDt_ViewJobCostingDataAmountByJob()
			: base(JCDDBObjectInfoList.Instance["RptDt_ViewJobCostingDataAmountByJob"])
		{
		}

		public override string Name => FormattableString.Invariant($"VIEW {base.Name}");

		// This object has been tomb-stoned and will not be created.
		public override string CreateSQLText => string.Empty;

		// Change to drop index and drop view
		public override string CheckAndDropSQLText => (NoResString)@" IF OBJECT_ID(N'RptDt_ViewJobCostingDataAmountByJob') IS NOT NULL
														BEGIN
															DROP VIEW RptDt_ViewJobCostingDataAmountByJob
														END";
	}
}
