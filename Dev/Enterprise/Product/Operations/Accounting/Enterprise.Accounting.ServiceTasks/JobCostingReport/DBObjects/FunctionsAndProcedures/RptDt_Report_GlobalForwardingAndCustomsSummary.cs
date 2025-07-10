using System;
using Enterprise.Accounting.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.ServiceTasks.JobCostingReport.DBObjects
{
	class RptDt_Report_GlobalForwardingAndCustomsSummary : JCDDependentDBObject
	{
		public RptDt_Report_GlobalForwardingAndCustomsSummary()
			: base(JCDDBObjectInfoList.Instance["RptDt_Report_GlobalForwardingAndCustomsSummary"])
		{
		}

		public override string Name => FormattableString.Invariant($"FUNCTION {base.Name}");

		public override string CreateSQLText => GetEmbeddedResourceText(resourceName);

		public override string CheckAndDropSQLText => (NoResString)@" IF OBJECT_ID(N'RptDt_Report_GlobalForwardingAndCustomsSummary') IS NOT NULL
														BEGIN
															DROP FUNCTION RptDt_Report_GlobalForwardingAndCustomsSummary
														END";

		const string resourceName = "Enterprise.Accounting.ServiceTasks.JobCostingReport.DBObjects.FunctionsAndProcedures.RptDt_Report_GlobalForwardingAndCustomsSummary.sql";
	}
}
