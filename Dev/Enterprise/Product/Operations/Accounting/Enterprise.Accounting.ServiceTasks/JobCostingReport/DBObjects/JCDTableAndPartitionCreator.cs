using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using Enterprise.Accounting.ServiceTasks.JobCostingReport;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.ServiceTasks
{
	public static class JCDTableAndPartitionCreator
	{
		#region Create Table And Partition

		public static bool Create(DbConnection connection, int startingPeriod, Action<string> logStatus)
		{
			bool result = false;

			logStatus((NoResString)"Creating Job Costing Data Queue service task related tables and partition");

			if (Drop(connection, x => true))
			{
				result = RunSql(connection, JCDTableAndPartitionObject.GetTableAndPartitionInfo(startingPeriod).OrderBy(x => x.Sequence).Select(x => x.CreateSQLText).ToList());
			}

			if (result)
			{
				logStatus((NoResString)"Job Costing Data Queue service task related tables and partition have been created successfully");
			}

			return result;
		}

		#endregion

		#region Drop Main Report Table And Partition

		public static bool DeleteMainReportTableAndPartition(DbConnection connection, Action<string> logStatus)
		{
			bool result = false;

			logStatus("Dropping database objects : RptDtJobCostingData, PS_AccountingPeriodCompany, PF_AccountingPeriodCompany");

			result = RunSql(connection, JCDTableAndPartitionObject.GetTableAndPartitionInfo().Where(x => !x.IsTemporary).OrderByDescending(x => x.Sequence).Select(x => x.CheckAndDropSQLText).ToList());

			if (result)
			{
				logStatus("Dropped database objects : RptDtJobCostingData, PS_AccountingPeriodCompany, PF_AccountingPeriodCompany");
			}

			return result;
		}

		#endregion

		#region Drop Temporary Tables And Index

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Log Message. Not displayed in UI")]
		public static bool DeleteTempTableAndIndex(DbConnection connection, Action<string> addToLog)
		{
			bool result = false;

			addToLog("Dropping database objects : RptDtUnprocessedAccTransactionLines, RptDtUnprocessedReversedAL");

			result = Drop(connection, x => x.IsTemporary);

			if (result)
			{
				addToLog("Dropped database objects : RptDtUnprocessedAccTransactionLines, RptDtUnprocessedReversedAL");
			}

			return result;
		}

		static bool Drop(DbConnection connection, Func<JCDTableAndPartitionObject, bool> filter)
		{
			return RunSql(connection, JCDTableAndPartitionObject.GetTableAndPartitionInfo().Where(filter).OrderByDescending(x => x.Sequence).Select(x => x.CheckAndDropSQLText).ToList());
		}

		#endregion

		static bool RunSql(DbConnection connection, List<string> sqls)
		{
			bool result = false;

			if (connection != null)
			{
				sqls.ForEach(sql => connection.ExecuteNonQuery(sql));
				result = true;
			}

			return result;
		}
	}
}
