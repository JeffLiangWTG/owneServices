using System;
using Enterprise.Accounting.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.ServiceTasks.JobCostingReport.DBObjects
{
	class RptDt_GetPeriodKeys : JCDDependentDBObject
	{
		public RptDt_GetPeriodKeys()
			: base(JCDDBObjectInfoList.Instance["RptDt_GetPeriodKeys"])
		{
		}

		public override string Name => FormattableString.Invariant($"FUNCTION {base.Name}");

		public override string CreateSQLText => GetEmbeddedResourceText(resourceName);

		public override string CheckAndDropSQLText => (NoResString)@" IF OBJECT_ID(N'RptDt_GetPeriodKeys') IS NOT NULL
														BEGIN
															DROP FUNCTION RptDt_GetPeriodKeys
														END";

		const string resourceName = "Enterprise.Accounting.ServiceTasks.JobCostingReport.DBObjects.FunctionsAndProcedures.RptDt_GetPeriodKeys.sql";
	}
}
