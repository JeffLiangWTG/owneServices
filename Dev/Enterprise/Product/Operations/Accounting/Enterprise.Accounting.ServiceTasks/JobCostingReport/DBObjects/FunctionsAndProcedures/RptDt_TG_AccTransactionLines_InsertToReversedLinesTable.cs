using System;
using Enterprise.Accounting.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.ServiceTasks.JobCostingReport.DBObjects
{
	public class RptDt_TG_AccTransactionLines_InsertToReversedLinesTable : JCDDependentDBObject
	{
		public RptDt_TG_AccTransactionLines_InsertToReversedLinesTable()
			: base(JCDDBObjectInfoList.Instance["RptDt_TG_AccTransactionLines_InsertToReversedLinesTable"])
		{
		}

		public override string Name => FormattableString.Invariant($"TRIGGER {base.Name}");

		public override string CreateSQLText => GetEmbeddedResourceText("Enterprise.Accounting.ServiceTasks.JobCostingReport.DBObjects.FunctionsAndProcedures.RptDt_TG_AccTransactionLines_InsertToReversedLinesTable.sql");

		public override string CheckAndDropSQLText => (NoResString)@" IF OBJECT_ID(N'RptDt_TG_AccTransactionLines_InsertToReversedLinesTable') IS NOT NULL
														BEGIN
															DROP TRIGGER RptDt_TG_AccTransactionLines_InsertToReversedLinesTable
														END";
	}
}
