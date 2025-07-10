using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.Accounting.Registry.Business;
using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Accounting.Business
{
	public class JCDDBObjectsChecker
	{
		public JCDDBObjectsChecker(DbConnection connection)
		{
			Argument.NotNull(connection, "Connection");
			this.connection = connection;
		}
		readonly DbConnection connection;

		public bool DoesAllJCDDBObjectExist()
		{
			return JCDDBObjects.All(x => x.Exist || x.IsObsolete);
		}

		public bool DoesAllPermanentJCDDBObjectExist()
		{
			return JCDDBObjects.Where(x => !x.IsTemporary).All(x => x.Exist || x.IsObsolete);
		}

		public bool DoesAnyTempJCDDBObjectExist()
		{
			return JCDDBObjects.Where(x => x.IsTemporary).Any(x => x.Exist);
		}

		public bool DoesAnyJCDDBObjectExist()
		{
			return JCDDBObjects.Where(x => x.CreatedBy == JCDDBObjectInfo.JCD).Any(x => x.Exist);
		}

		public bool DoesAllTempJCDTableExist()
		{
			return JCDDBObjects.Where(x => x.IsTemporary && (x.Type == JCDDBObjectInfo.TABLE || x.Type == JCDDBObjectInfo.INDEX)).All(x => x.Exist || x.IsObsolete);
		}

		public bool DoesAllPermanentJCDTableAndPartitionExist()
		{
			return JCDDBObjects.Where(x => !x.IsTemporary && (x.Type == JCDDBObjectInfo.TABLE || x.Type == JCDDBObjectInfo.PARTITION_SCHEME)).All(x => x.Exist || x.IsObsolete);
		}

		public bool IsThereAnyUnprocessedOldALRecord()
		{
			var result = false;
			if (DataUtils.ObjectExists(Connection, "RptDtUnprocessedAccTransactionLines") && Connection.Exists((NoResString)"FROM RptDtUnprocessedAccTransactionLines"))
			{
				var maxRowNumber = Connection.ExecuteScalar<long>("SELECT ISNULL(MAX(UL_RowNumber), 0) FROM RptDtUnprocessedAccTransactionLines");
				result = maxRowNumber >= AccountingConfigurationRegistry.Instance.JCDQueueHighWaterMark.Value;
			}
			return result;
		}

		public bool IsThereAnyALRecord()
		{
			return Connection.Exists((NoResString)"FROM dbo.AccTransactionLines");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Table Name")]
		public bool IsQueueEmpty()
		{
			return DataUtils.ObjectExists(Connection, "JobCostingDataQueue") && !Connection.Exists("FROM dbo.JobCostingDataQueue");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Table Name")]
		public bool IsReportTableEmpty()
		{
			return DataUtils.ObjectExists(Connection, "RptDtJobCostingData") && !Connection.Exists("FROM RptDtJobCostingData");
		}

		public string GetMissingDBObjectNames()
		{
			var missingObjectNameString = string.Empty;
			var missingObjectNames = JCDDBObjects
										.Where(x => !x.Exist && !x.IsObsolete)
										.OrderBy(x => x.DependencySequence)
										.Select(x => x.Name);
			if (missingObjectNames.Any())
			{
				missingObjectNameString = string.Join(", ", missingObjectNames);
			}
			return missingObjectNameString;
		}

		IReadOnlyCollection<JCDDBObjectInfoWithExists> JCDDBObjects => (jcdDBObjects ??= PopulateExistProperty());
		IReadOnlyCollection<JCDDBObjectInfoWithExists> jcdDBObjects;

		IReadOnlyCollection<JCDDBObjectInfoWithExists> PopulateExistProperty()
		{
			var dbObjectInfo = JCDDBObjectInfoList.Instance;
			var sql = GetSql(dbObjectInfo);
			var objectNamesInDatabase = DataUtils.GetListOfValuesFromQuery(Connection, sql).ToHashSet();
			return dbObjectInfo
				.Where(dbObj => dbObj.ObjectCategory == JCDDBObjectInfo.DbObjectCategory.Definition)
				.Select(dbObj =>
			{
				var existsInDb = objectNamesInDatabase.Contains(dbObj.Name);
				return dbObj.WithExists(existsInDb);
			}).ToList();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Part of SQL")]
		string GetSql(IEnumerable<JCDDBObjectInfo> dbObjectInfo)
		{
			var sqlBuilder = new List<string>();

			var systemObjects = dbObjectInfo.Where(dbObj => new[] { JCDDBObjectInfo.TABLE, JCDDBObjectInfo.DEPENDENT_TABLE, JCDDBObjectInfo.PROCEDURE, JCDDBObjectInfo.FUNCTION, JCDDBObjectInfo.TABLE_FUNCTION, JCDDBObjectInfo.VIEW }.Contains(dbObj.Type));
			if (systemObjects.Any())
			{
				sqlBuilder.Add(string.Format(CultureInfo.InvariantCulture, @"--This is the query for loading JCD DB Object Info
SELECT [name] FROM sys.objects where [name] IN ({0}) AND [type] IN ('{1}', '{2}', '{3}', '{4}', '{5}')", getNameAsCSV(systemObjects), JCDDBObjectInfo.TABLE, JCDDBObjectInfo.PROCEDURE, JCDDBObjectInfo.FUNCTION, JCDDBObjectInfo.TABLE_FUNCTION, JCDDBObjectInfo.VIEW));
			}

			var sysIndex = dbObjectInfo.Where(dbObj => dbObj.Type == JCDDBObjectInfo.INDEX);
			if (sysIndex.Any())
			{
				sqlBuilder.Add(string.Format(CultureInfo.InvariantCulture, "SELECT [name] FROM sys.indexes where [name] in ({0})", getNameAsCSV(sysIndex)));
			}

			var sysTriggers = dbObjectInfo.Where(dbObj => dbObj.Type == JCDDBObjectInfo.TRIGGER);
			if (sysTriggers.Any())
			{
				sqlBuilder.Add(string.Format(CultureInfo.InvariantCulture, "SELECT [name] FROM sys.triggers where [name] in ({0})", getNameAsCSV(sysTriggers)));
			}

			var sysPF = dbObjectInfo.Where(dbObj => dbObj.Type == JCDDBObjectInfo.PARTITION_FUNCTION);
			if (sysPF.Any())
			{
				sqlBuilder.Add(string.Format(CultureInfo.InvariantCulture, "SELECT [name] FROM sys.partition_functions where [name] in ({0})", getNameAsCSV(sysPF)));
			}

			var sysPS = dbObjectInfo.Where(dbObj => dbObj.Type == JCDDBObjectInfo.PARTITION_SCHEME);
			if (sysPS.Any())
			{
				sqlBuilder.Add(string.Format(CultureInfo.InvariantCulture, "SELECT [name] FROM sys.partition_schemes where [name] in ({0})", getNameAsCSV(sysPS)));
			}

			var sql = string.Join("\n UNION \n", sqlBuilder);
			return sql;

			string getNameAsCSV(IEnumerable<JCDDBObjectInfo> selObjectInfo)
			{
				return string.Join(", ", selObjectInfo.Select(x => $"'{x.Name}'"));
			}
		}

		DbConnection Connection
		{
			get
			{
				return connection;
			}
		}
	}

	[Immutable]
	public class JCDDBObjectInfoList : IEnumerable<JCDDBObjectInfo>
	{
		public static JCDDBObjectInfoList Instance { get; } = new JCDDBObjectInfoList();

		JCDDBObjectInfoList()
		{
			dbObjects = new[]
			{
				new JCDDBObjectInfo(name: "PeriodEndExchangeRate",                          type: JCDDBObjectInfo.FUNCTION,  isTemporary: false, isObsolete: false, dependencySequence: 1, createdBy: JCDDBObjectInfo.SCHEMA),
				new JCDDBObjectInfo(name: "CreateJobCostingDataTableAndCCI",                type: JCDDBObjectInfo.PROCEDURE, isTemporary: false, isObsolete: false, dependencySequence: 2, createdBy: JCDDBObjectInfo.SCHEMA),
				new JCDDBObjectInfo(name: "SwitchNonEmptyPartitionFromJobCostingDataTable", type: JCDDBObjectInfo.PROCEDURE, isTemporary: false, isObsolete: false, dependencySequence: 3, createdBy: JCDDBObjectInfo.SCHEMA),
				new JCDDBObjectInfo(name: "CreatePeriodCompanyPartitionKeys",               type: JCDDBObjectInfo.PROCEDURE, isTemporary: false, isObsolete: false, dependencySequence: 4, createdBy: JCDDBObjectInfo.SCHEMA),
				new JCDDBObjectInfo(name: "JobCostingDataQueue",                            type: JCDDBObjectInfo.TABLE,     isTemporary: false, isObsolete: false, dependencySequence: 5, createdBy: JCDDBObjectInfo.SCHEMA),
				new JCDDBObjectInfo(name: "NR_RC__JCQ_HasNoPeriod_JCQ_RowNumber",           type: JCDDBObjectInfo.INDEX,     isTemporary: false, isObsolete: false, dependencySequence: 6, createdBy: JCDDBObjectInfo.SCHEMA),

				new JCDDBObjectInfo(name: "RptDtUnprocessedAccTransactionLines",                      type: JCDDBObjectInfo.TABLE,     isTemporary: true,  isObsolete: false, dependencySequence: 7,  createdBy: JCDDBObjectInfo.JCD),
				new JCDDBObjectInfo(name: "NR_RC__Clustered_UL_RowNumber",                            type: JCDDBObjectInfo.INDEX,     isTemporary: true,  isObsolete: false, dependencySequence: 8,  createdBy: JCDDBObjectInfo.JCD),
				new JCDDBObjectInfo(name: "RptDtUnprocessedReversedAL",                               type: JCDDBObjectInfo.TABLE,     isTemporary: true,  isObsolete: false, dependencySequence: 9,  createdBy: JCDDBObjectInfo.JCD),
				new JCDDBObjectInfo(name: "FK_UC__Clustered_URL_ALPK",                                type: JCDDBObjectInfo.INDEX,     isTemporary: true,  isObsolete: false, dependencySequence: 10, createdBy: JCDDBObjectInfo.JCD),
				new JCDDBObjectInfo(name: "RptDt_TG_AccTransactionLines_InsertToReversedLinesTable",  type: JCDDBObjectInfo.TRIGGER,   isTemporary: true,  isObsolete: false, dependencySequence: 11, createdBy: JCDDBObjectInfo.JCD),
				new JCDDBObjectInfo(name: "RptDt_TG_AccTransactionLines_InsertToJobCostingDataQueue", type: JCDDBObjectInfo.TRIGGER,   isTemporary: false, isObsolete: false, dependencySequence: 12, createdBy: JCDDBObjectInfo.JCD),
				new JCDDBObjectInfo(name: "RptDtTransformAccTransactionLineToJobCostingQueueRecord",  type: JCDDBObjectInfo.PROCEDURE, isTemporary: true,  isObsolete: false, dependencySequence: 13, createdBy: JCDDBObjectInfo.JCD),

				new JCDDBObjectInfo(name: "PF_AccountingPeriodCompany",           type: JCDDBObjectInfo.PARTITION_FUNCTION, isTemporary: false, isObsolete: false, dependencySequence: 20, createdBy: JCDDBObjectInfo.JCD),
				new JCDDBObjectInfo(name: "PS_AccountingPeriodCompany",           type: JCDDBObjectInfo.PARTITION_SCHEME,   isTemporary: false, isObsolete: false, dependencySequence: 21, createdBy: JCDDBObjectInfo.JCD),
				new JCDDBObjectInfo(name: "RptDtJobCostingData",                  type: JCDDBObjectInfo.TABLE,              isTemporary: false, isObsolete: false, dependencySequence: 22, createdBy: JCDDBObjectInfo.JCD),
				new JCDDBObjectInfo(name: "CI_JCD_PostDate",                      type: JCDDBObjectInfo.INDEX,              isTemporary: false, isObsolete: false, dependencySequence: 23, createdBy: JCDDBObjectInfo.JCD),
				new JCDDBObjectInfo(name: "NCI_JCD_OH",                           type: JCDDBObjectInfo.INDEX,              isTemporary: false, isObsolete: false, dependencySequence: 24, createdBy: JCDDBObjectInfo.JCD),

				new JCDDBObjectInfo(name: "RptDtJobCostingDataAmountByJob",                                     type: JCDDBObjectInfo.DEPENDENT_TABLE,    isTemporary: false, isObsolete: false, dependencySequence: 30, createdBy: JCDDBObjectInfo.JCD),
				new JCDDBObjectInfo(name: "RptDt_UC_JCA_PostPeriod_JCA_GC_JCA_OH_JCA_JH",                       type: JCDDBObjectInfo.INDEX,              isTemporary: false, isObsolete: true, dependencySequence: 31, createdBy: JCDDBObjectInfo.JCD),
				new JCDDBObjectInfo(name: "RptDt_UC_JCA_PostPeriod_JCA_GC_JCA_OH_JCA_JH_JCA_RX_NKLocalCurrency",type: JCDDBObjectInfo.INDEX,              isTemporary: false, isObsolete: false, dependencySequence: 31, createdBy: JCDDBObjectInfo.JCD),
				new JCDDBObjectInfo(name: "PopulateJobCostingDataAmountByJob",                                  type: JCDDBObjectInfo.INSERT,             isTemporary: false, isObsolete: false, dependencySequence: 32, createdBy: JCDDBObjectInfo.JCD),
				new JCDDBObjectInfo(name: "RptDt_NX_JCA_JH_JCA_GC_JCA_OH",                                      type: JCDDBObjectInfo.INDEX,              isTemporary: false, isObsolete: false, dependencySequence: 33, createdBy: JCDDBObjectInfo.JCD),
				new JCDDBObjectInfo(name: "RptDt_NX_JCA_OH_JCA_GC_JCA_JH",                                      type: JCDDBObjectInfo.INDEX,              isTemporary: false, isObsolete: false, dependencySequence: 34, createdBy: JCDDBObjectInfo.JCD),

				new JCDDBObjectInfo(name: "RptDtPopulateJobCostingDataFromQueue", type: JCDDBObjectInfo.PROCEDURE, isTemporary: false, isObsolete: false, dependencySequence: 40, createdBy: JCDDBObjectInfo.JCD),

				new JCDDBObjectInfo(name: "RptDt_GetPeriodKeys",                            type: JCDDBObjectInfo.TABLE_FUNCTION, isTemporary: false, isObsolete: false, dependencySequence: 50, createdBy: JCDDBObjectInfo.JCD),
				new JCDDBObjectInfo(name: "RptDt_GlobalJobProfitReportCore",                type: JCDDBObjectInfo.FUNCTION,       isTemporary: false, isObsolete: false, dependencySequence: 51, createdBy: JCDDBObjectInfo.JCD),
				new JCDDBObjectInfo(name: "RptDt_Report_GlobalJobProfitSummaryByJob",       type: JCDDBObjectInfo.FUNCTION,       isTemporary: false, isObsolete: false, dependencySequence: 52, createdBy: JCDDBObjectInfo.JCD),
				new JCDDBObjectInfo(name: "RptDt_Report_GlobalForwardingAndCustomsSummary", type: JCDDBObjectInfo.FUNCTION,       isTemporary: false, isObsolete: false, dependencySequence: 53, createdBy: JCDDBObjectInfo.JCD),

				new JCDDBObjectInfo(name: "RptDt_ViewJobCostingDataAmountByJob",            type: JCDDBObjectInfo.VIEW,           isTemporary: false, isObsolete: true,  dependencySequence: 60, createdBy: JCDDBObjectInfo.JCD),
				new JCDDBObjectInfo(name: "RptDt_CI_JCD_PostPeriod_JCD_GC_JCD_OH_JCD_JH",   type: JCDDBObjectInfo.INDEX,          isTemporary: false, isObsolete: true,  dependencySequence: 61, createdBy: JCDDBObjectInfo.JCD),
				new JCDDBObjectInfo(name: "RptDt_NI_JCD_JH_JCD_GC_JCD_OH",                  type: JCDDBObjectInfo.INDEX,          isTemporary: false, isObsolete: true,  dependencySequence: 62, createdBy: JCDDBObjectInfo.JCD),
				new JCDDBObjectInfo(name: "RptDt_NI_JCD_OH_JCD_GC_JCD_JH",                  type: JCDDBObjectInfo.INDEX,          isTemporary: false, isObsolete: true,  dependencySequence: 63, createdBy: JCDDBObjectInfo.JCD),
			}.ToImmutableDictionary(x => x.Name, x => x);
		}
		readonly ImmutableDictionary<string, JCDDBObjectInfo> dbObjects;

		public IEnumerator<JCDDBObjectInfo> GetEnumerator()
		{
			return dbObjects.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public JCDDBObjectInfo this[string name] => dbObjects[name];
	}

	[Immutable]
	public class JCDDBObjectInfo
	{
		public JCDDBObjectInfo(string name, string type, int dependencySequence, bool isTemporary, bool isObsolete, string createdBy)
		{
			Name = name;
			Type = type;
			DependencySequence = dependencySequence;
			IsTemporary = isTemporary;
			IsObsolete = isObsolete;
			CreatedBy = createdBy;
		}

		#region Properties

		public string Name { get; }
		public string Type { get; }
		public DbObjectCategory ObjectCategory
			=> Type == JCDDBObjectInfo.INSERT ? DbObjectCategory.Manipulation : DbObjectCategory.Definition;
		public int DependencySequence { get; }
		public bool IsTemporary { get; }
		public bool IsObsolete { get; }  // Obsolete objects should be dropped and never re-created
		public string CreatedBy { get; }

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Not displayed in UI")]
		public override string ToString()
			=> FormattableString.Invariant($"{Name}-{Type}-{(IsTemporary ? "Temporary" : "Permanent")}-{(IsObsolete ? "Obsolete" : "Live")}-{DependencySequence}-{CreatedBy}");

		internal JCDDBObjectInfoWithExists WithExists(bool exist)
			=> new JCDDBObjectInfoWithExists(this, exist);

		#region Constants

		public enum DbObjectCategory
		{
			Manipulation,
			Definition,
		}

		// Classified as data definition statements (DDL)
		public const string TABLE = "U";
		public const string DEPENDENT_TABLE = "DU";  // A table which depends on RptDtJobCostingData. Used to replace indexed views.
		public const string PROCEDURE = "P";
		public const string FUNCTION = "IF";
		public const string TABLE_FUNCTION = "TF";
		public const string INDEX = "I";
		public const string TRIGGER = "T";
		public const string PARTITION_FUNCTION = "PF";
		public const string PARTITION_SCHEME = "PS";
		public const string SCHEMA = "S";
		public const string JCD = "J";
		public const string VIEW = "V";

		// Classified as data manipulation statements (DML)
		public const string INSERT = "IN";

		#endregion
	}

	[Immutable]
	class JCDDBObjectInfoWithExists
	{
		public JCDDBObjectInfoWithExists(JCDDBObjectInfo parent, bool exist)
		{
			fParent = parent;
			Exist = exist;
		}

		#region Properties

		readonly JCDDBObjectInfo fParent;

		public string Name => fParent.Name;
		public string Type => fParent.Type;
		public JCDDBObjectInfo.DbObjectCategory ObjectCategory => fParent.ObjectCategory;
		public bool Exist { get; }
		public int DependencySequence => fParent.DependencySequence;
		public bool IsTemporary => fParent.IsTemporary;
		public bool IsObsolete => fParent.IsObsolete;
		public string CreatedBy => fParent.CreatedBy;

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Not displayed in UI")]
		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}: {1}", fParent.ToString(), Exist ? "Exists in DB" : "Not in DB");
		}
	}
}
