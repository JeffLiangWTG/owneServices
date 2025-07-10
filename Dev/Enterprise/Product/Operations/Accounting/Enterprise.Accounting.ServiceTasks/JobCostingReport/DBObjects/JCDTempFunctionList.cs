using System;
using System.Collections.Generic;
using Enterprise.Accounting.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.ServiceTasks.JobCostingReport
{
	public class JCDTempFunctionList
	{
		IEnumerable<JCDDependentDBObject> Get()
		{
			yield return new DBObjects.RptDt_TG_AccTransactionLines_InsertToReversedLinesTable();
			yield return new DBObjects.RptDtTransformAccTransactionLineToJobCostingQueueRecord();
		}

		public static JCDDBObjectVersionManager GetVersionManager()
		{
			var currentVersion = AccountingConfigurationRegistry.Instance.JobCostingQueueDBObjectVersion.Value;
			Action<int> changeVersion = (versionNo) => AccountingConfigurationRegistry.Instance.JobCostingQueueDBObjectVersion.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, versionNo);
			return new JCDDBObjectVersionManager((NoResString)"Temporary DB Objects", currentVersion, changeVersion, LATEST_VERSION, new JCDTempFunctionList().Get());
		}

		public const int LATEST_VERSION = 2;
	}
}
