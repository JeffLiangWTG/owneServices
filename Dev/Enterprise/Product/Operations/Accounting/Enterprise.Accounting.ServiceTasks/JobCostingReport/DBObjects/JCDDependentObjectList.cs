using System;
using System.Collections.Generic;
using Enterprise.Accounting.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.ServiceTasks.JobCostingReport
{
	public class JCDDependentObjectList
	{
		IEnumerable<JCDDependentDBObject> Get()
		{
			yield return new DBObjects.RptDt_ViewJobCostingDataAmountByJob();
			yield return new DBObjects.RptDt_JobCostingDataClusteredIndex();
			yield return new DBObjects.RptDt_JobCostingDataIndexByJH();
			yield return new DBObjects.RptDt_JobCostingDataIndexByOH();

			yield return new DBObjects.RptDtJobCostingDataAmountByJob();
			yield return new DBObjects.JobCostingDataAmountByJobClusteredIndex();
			yield return new DBObjects.JobCostingDataAmountByJobClusteredIndex_Obsolete();
			yield return new DBObjects.PopulateJobCostingDataAmountByJob();
			yield return new DBObjects.JobCostingDataAmountByJobIndexByJH();
			yield return new DBObjects.JobCostingDataAmountByJobIndexByOH();

			yield return new DBObjects.RptDt_GetPeriodKeys();
			yield return new DBObjects.RptDt_GlobalJobProfitReportCore();
			yield return new DBObjects.RptDt_TG_AccTransactionLines_InsertToJobCostingDataQueue();
			yield return new DBObjects.RptDtPopulateJobCostingDataFromQueue();
			yield return new DBObjects.RptDt_Report_GlobalJobProfitSummaryByJob();
			yield return new DBObjects.RptDt_Report_GlobalForwardingAndCustomsSummary();
		}

		public static JCDDBObjectVersionManager GetVersionManager()
		{
			var currentversion = AccountingConfigurationRegistry.Instance.JobCostingReportRelatedDBObjectVersion.Value;
			Action<int> changeVersion = (versionNo) => AccountingConfigurationRegistry.Instance.JobCostingReportRelatedDBObjectVersion.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, versionNo);
			return new JCDDBObjectVersionManager((NoResString)"Functions, Stored Procedures and Dependent Tables", currentversion, changeVersion, LATEST_VERSION, new JCDDependentObjectList().Get());
		}

		public const int LATEST_VERSION = 12;
	}
}
