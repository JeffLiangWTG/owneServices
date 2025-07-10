using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using CargoWise.DbUpgrader.Foundation;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformations.PreUpgrade;

namespace Enterprise.DbUpgrader.Schema.OnlineUpgrade
{
	public partial class TablePreSynchroniser
	{
		public TablePreSynchroniser(IUpgradeManager manager, string dbBeingUpgraded, string templateDb)
		{
			this.targetDb = dbBeingUpgraded;
			this.templateDb = templateDb;
			this.preAddTablePrefix = (dbBeingUpgraded == Db.DatabaseName) ? "" : dbBeingUpgraded;
			this.Manager = manager;
			this.newColumnCreatorFactory = new ColumnCreatorWithValueFactory(manager);

			CheckStmExtendedProperty();
		}

		void CheckStmExtendedProperty()
		{
			if (!DataUtils.ObjectExists(Db.Connection, ExtProperty.TableName))
			{
				Db.Connection.ExecuteNonQuery(ExtProperty.TableDefinition);
			}
		}

		static readonly string preAddDb = UpgUtils.UpgraderPrefix + "PreSchemaUpgradeDb_" + Db.DatabaseName;

		protected string TargetDb
		{
			get { return targetDb; }
		}
		readonly string targetDb;

		protected string TemplateDB
		{
			get { return templateDb; }
		}
		readonly string templateDb;

		protected readonly string preAddTablePrefix;
		protected IUpgradeManager Manager { get; }
		readonly ColumnCreatorWithValueFactory newColumnCreatorFactory;

		readonly PopulateTargetColumnsConfiguration populateTargetColumnsConfiguration = new PopulateTargetColumnsConfiguration();

		public void AddAndPopulateAuditAndNaturalKeyColumns()
		{
			var tables = GetColumnsToPopulate(GetColumnsToAdd(), CreatePopulatedColumns);

			var totalTableCount = tables.Count;
			var totalColumnCount = 0;
			long totalTableSize = 0;
			tables.ForEach(t =>
			{
				totalColumnCount += t.ColumnList.Count;
				totalTableSize += t.TargetTableSize;

				RecordHighWatermarkIfPopulatingAuditInfo(t);
			});

			var headerMessage = String.Format(CultureInfo.InvariantCulture, "Add new columns ({0})", totalColumnCount);
			DoPopulate(tables, totalTableCount, totalTableSize, headerMessage, "(+)", OnlineUpgradePopulatingStatusName, OnlineUpgradePopulatingWatermarkName);

			void RecordHighWatermarkIfPopulatingAuditInfo(PopulatedTable table)
			{
				if (table.ColumnList.Any(x =>
					x.ColumnName.IndexOf("SystemCreate", StringComparison.OrdinalIgnoreCase) > -1
					|| x.ColumnName.IndexOf("SystemLastEdit", StringComparison.OrdinalIgnoreCase) > -1))
				{
					ExtProperty.Table.Update(
						Db.Connection,
						table.TargetTableSchema,
						table.TargetTableName,
						HighWatermarks.PopulateAuditTimeAndUserHighWatermark,
						DateTime.UtcNow.ToString("s"));
				}
			}
		}

		public void ConvertCharFlagsToBit()
		{
			Manager.StartTask("Get fields to convert char flags to bit");

			var tables = GetColumnsToPopulate(GetAllColumnsToConvertCharToBit(), CreateConvertedColumnsCharToBit);
			var totalTableCount = tables.Count;
			var totalColumnCount = 0;
			long totalTableSize = 0;
			tables.ForEach(t =>
			{
				totalColumnCount += t.ColumnList.Count;
				totalTableSize += t.TargetTableSize;
			});

			string headerMessage = String.Format(CultureInfo.InvariantCulture, "Convert char flags to bit ({0})", totalColumnCount);

			DoPopulate(tables, totalTableCount, totalTableSize, headerMessage, "(~)", OnlineUpgradeConvertToBitStatusName, OnlineUpgradeConvertToBitWatermarkName);
		}

		public void ConvertCharToChar()
		{
			Manager.StartTask("Get fields to convert [n][var]char to [n][var]char");

			var tables = GetColumnsToPopulate(GetAllColumnsToConvertCharToChar(), CreateConvertedColumnsCharToChar);
			var totalTableCount = tables.Count;
			var totalColumnCount = 0;
			long totalTableSize = 0;
			tables.ForEach(t =>
			{
				totalColumnCount += t.ColumnList.Count;
				totalTableSize += t.TargetTableSize;
			});

			string headerMessage = String.Format(CultureInfo.InvariantCulture, "Convert [n][var]char to [n][var]char ({0})", totalColumnCount);

			DoPopulate(tables, totalTableCount, totalTableSize, headerMessage, "(~)", OnlineUpgradeConvertCharToCharStatusName, OnlineUpgradeConvertCharToCharWatermarkName);
		}

		public void ConvertDecimalToDecimal()
		{
			Manager.StartTask("Get fields to convert decimal to decimal");

			var tables = GetColumnsToPopulate(GetAllColumnsToConvertDecimalToDecimal(), CreateConvertedColumnsDecimalToDecimal);
			var totalTableCount = tables.Count;
			var (totalColumnCount, totalTableSize) = tables
				.Select(t => (t.ColumnList.Count, t.TargetTableSize))
				.Aggregate((0, 0L), (a, b) => (a.Item1 + b.Count, a.Item2 + b.TargetTableSize));

			var headerMessage = $"Convert decimal to decimal ({totalColumnCount})";

			DoPopulate(tables, totalTableCount, totalTableSize, headerMessage, "(~)", OnlineUpgradeConvertDecimalToDecimalStatusName, OnlineUpgradeConvertDecimalToDecimalWatermarkName);
		}

		public void ConvertDateTimesToDate()
		{
			Manager.StartTask("Get fields to convert [small]datetime[2] to date");

			var tables = GetColumnsToPopulate(GetAllColumnsToConvertDateTimesToDate(), CreateConvertedColumnsDateTimesToDate);
			var totalTableCount = tables.Count;
			var totalColumnCount = 0;
			long totalTableSize = 0;
			tables.ForEach(t =>
			{
				totalColumnCount += t.ColumnList.Count;
				totalTableSize += t.TargetTableSize;
			});

			string headerMessage = String.Format(CultureInfo.InvariantCulture, "Convert [small]datetime[2] to date ({0})", totalColumnCount);

			DoPopulate(tables, totalTableCount, totalTableSize, headerMessage, "(~)", OnlineUpgradeConvertDateTimesToDateStatusName, OnlineUpgradeConvertDateTimesToDateWatermarkName);
		}

		public void DoPopulate(List<PopulatedTable> tables, int totalTableCount, long totalTableSize, string headerMessage, string infoMark, string statusName, string watermarkName)
		{
			if (totalTableCount > 0)
			{
				Manager.StartTask(headerMessage);

				Manager.ShowInfoMessage("Creating Pre-Add database.");
#if DEBUG
				// We should not be creating Big DBs on DAT or for local DEBUG build DBs
				TablePreSynchroniser.CreatePreAddDb_ForTest();
#else
				TablePreSynchroniser.CreatePreAddDb(dataInitialSizeMb: 1024, dataGrowthMb: 1024, logInitialSizeMb: 1024, logGrowthMb: 512);
#endif

				int currentTable = 1;
				long currentTableSize = 0;
				foreach (var table in tables.OrderBy(t => t.TargetTableFullName))
				{
					currentTableSize += table.TargetTableSize;
					Manager.ShowInfoMessage(String.Format(CultureInfo.InvariantCulture,
						"table {0}/{1} ({2:N0} MB/{3:N0} MB) {4}",
						currentTable++,				// 0
						totalTableCount,			// 1
						currentTableSize,			// 2
						totalTableSize,				// 3
						table.TargetTableFullName	// 4
						));
					foreach (var col in table.ColumnList)
					{
						Manager.ShowInfoMessage(FormattableString.Invariant($"    {infoMark} {col.ColumnName} {col.ColumnType} {col.Sparse} {col.ColumnNullable} {col.ColumnDefaultConstraint}"));
					}

					if (table.TargetTableStatus.Value == TargetTableStatus.None)
					{
						CreateTargetColumnsWithStatus(table, statusName, TargetTableStatus.Added);
					}

					if (table.TargetTableStatus.Value == TargetTableStatus.Added || table.TargetTableStatus.Value == TargetTableStatus.Populating && !PreAddTableExists(table))
					{
						CreatePreAddColumnsWithStatus(table, statusName, watermarkName, TargetTableStatus.Populating);
					}

					if (table.TargetTableStatus.Value == TargetTableStatus.Populating)
					{
						PopulateTargetColumns(table, statusName, watermarkName, TargetTableStatus.Populated);
					}

					if (table.TargetTableStatus.Value == TargetTableStatus.Populated)
					{
						DropPreAddTableAndStatuses(table, statusName);
					}
				}
			}
		}

		#region Add and populate new columns

		static string OnlineUpgradePopulatingStatusName
		{
			get { return "OnlineUpgradePopulatingStatus"; }
		}

		static string OnlineUpgradePopulatingWatermarkName
		{
			get { return "OnlineUpgradePopulatingWatermark"; }
		}

		#region Get Columns to Add Scripts

		DataTable GetColumnsToAdd()
		{
			string renamedColumns = newColumnCreatorFactory.GetPreUpgradeTransformationRenamedColumnList();
			string renamedJoin = "";
			string renamedColumn = "";
			if (!String.IsNullOrWhiteSpace(renamedColumns))
			{
				renamedJoin = String.Format(CultureInfo.InvariantCulture, @"
	LEFT JOIN
	(
		VALUES
			{0}
	) AS Renamed(schemaName, tableName, oldName, newName) ON Renamed.schemaName = NewSchema.name AND Renamed.tableName = NewTab.name AND Renamed.newName = NewCol.name"
					, renamedColumns
					);

				renamedColumn = ", Renamed.oldName";
			}

			var sql = FormattableString.Invariant($@"-- Get list of the columns to populate
SELECT
	TabSchema        = NewSchema.name,
	TabName          = NewTab.name,
	ColId            = NewCol.column_id,
	ColName          = NewCol.name,
	ColType          = NewType.name,
	ColLength        = NewCol.max_length,
	ColPrecision     = NewCol.precision,
	ColScale         = NewCol.scale,
	ColNullOrNotNull = CASE NewCol.is_nullable WHEN 1 THEN 'NULL' ELSE 'NOT NULL' END,
	ColSparse        = NewCol.is_sparse,
	ColDefault       = NewDefault.definition,
	ColExtProp       = ISNULL(CurColStatus.SEP_Value, 'None'),
	TabExtProp       = ISNULL(CurTabWatermark.SEP_Value, '0')
FROM
	[{templateDb}].sys.schemas                       AS NewSchema
	JOIN [{templateDb}].sys.tables                   AS NewTab     ON NewTab.schema_id = NewSchema.schema_id
	JOIN [{templateDb}].sys.columns                  AS NewCol     ON NewCol.object_id = NewTab.object_id
	JOIN [{templateDb}].sys.types                    AS NewType    ON NewType.user_type_id = NewCol.user_type_id
	LEFT JOIN [{templateDb}].sys.default_constraints AS NewDefault ON NewDefault.object_id = NewCol.default_object_id
{renamedJoin}

	JOIN [{targetDb}].sys.schemas      AS CurSchema ON CurSchema.name = NewSchema.name
	JOIN [{targetDb}].sys.tables       AS CurTab    ON CurSchema.schema_id = CurTab.schema_id AND CurTab.name = NewTab.name
	LEFT JOIN [{targetDb}].sys.columns AS CurCol    ON CurCol.object_id = CurTab.object_id AND CurCol.name in (NewCol.name{renamedColumn})

	LEFT JOIN [{targetDb}].dbo.StmExtendedProperty AS CurColStatus ON 1=1
		AND CurColStatus.SEP_Class = 'Column'
		AND CurColStatus.SEP_DatabaseNameSuffix = ''
		AND CurColStatus.SEP_SchemaName = CurSchema.name
		AND CurColStatus.SEP_MajorObjectName = CurTab.name
		AND CurColStatus.SEP_MinorObjectName = CurCol.name
		AND CurColStatus.SEP_Name = '{OnlineUpgradePopulatingStatusName}'

	LEFT JOIN [{targetDb}].dbo.StmExtendedProperty AS CurTabWatermark ON 1=1
		AND CurTabWatermark.SEP_Class = 'Table'
		AND CurTabWatermark.SEP_DatabaseNameSuffix = ''
		AND CurTabWatermark.SEP_SchemaName = CurSchema.name
		AND CurTabWatermark.SEP_MajorObjectName = CurTab.name
		AND CurTabWatermark.SEP_MinorObjectName = ''
		AND CurTabWatermark.SEP_Name = '{OnlineUpgradePopulatingWatermarkName}'

WHERE 1=1
	-- Ignore microsoft-shipped tables
	AND CurTab.is_ms_shipped = 0
	AND NewTab.is_ms_shipped = 0

	-- Only columns added in the new schema OR columns for that populating was started
	AND
	(
		CurCol.name is NULL
		OR CurColStatus.SEP_Value is NOT NULL
	)

	-- Ignore columns with a special rename handling in the upgrade (linked to big clustered indexes)
	AND NewCol.name NOT in ('SL_PostedTimeUtc', 'SJ_PostedTimeUtc', 'SY_PostedTimeUtc', 'SW_PostedTimeUtc')
OPTION (RECOMPILE);
");

			StoreQueryForTest(sql);

			return DataUtils.GetDataTableFromQuery(Db.Connection, sql);
		}

		#endregion

		void CreatePopulatedColumns(IGrouping<string, ColumnChangeMetadata> table, PopulatedTable populatedTable)
		{
			foreach (var col in table.OrderBy(c => c.GetColumnId()))
			{
				var columnWithValue = newColumnCreatorFactory.GetColumnCreator(Manager, col, targetDb);
				if (columnWithValue != null)
				{
					var populatedColumn = new PopulatedColumn();

					populatedColumn.ColumnId = int.Parse(col.GetColumnId(), CultureInfo.InvariantCulture);
					populatedColumn.ColumnName = col.ColumnName;
					populatedColumn.ColumnType = col.GetFullTypeDeclaration();
					populatedColumn.ColumnNullable = (col.IsNullable) ? "NULL" : "NOT NULL";

					// Audit columns should be non-nullable, then aren't supposed to be sparse either, see TheTest rules: UserHasConstraintWhenCreateTimeIsNonNullable, UserHasConstraintWhenLastEditTimeIsNonNullable, and AuditColumnIsNonNullable
					populatedColumn.IsSparse = false;
					populatedColumn.ColumnDefaultConstraint = (col.GetDefaultClause() == null)
						? null
						: String.Format(CultureInfo.InvariantCulture, " CONSTRAINT {0} DEFAULT {1}", col.CalculatedDefaultConstraintName.QuoteName(), col.GetDefaultClause());

					populatedColumn.PopulateSource = columnWithValue.PopulateSource;
					populatedColumn.PopulateExpression = columnWithValue.PopulateExpression;

					if (!String.IsNullOrWhiteSpace(columnWithValue.OrderByExpression) && populatedTable.OrderByExpression == null)
					{
						populatedTable.OrderByExpression = columnWithValue.OrderByExpression;
					}

					populatedColumn.TriggerExpression = columnWithValue.PopulateTriggerExpression;
					populatedColumn.TriggerSource = columnWithValue.PopulateTriggerSource;

					populatedColumn.PopulatePreAddWhereClause = "OR 1=1";
					populatedColumn.PopulateTargetWhereClause = null;

					if (!populatedTable.TargetTableStatus.HasValue)
					{
						populatedTable.TargetTableStatus = (TargetTableStatus)Enum.Parse(typeof(TargetTableStatus), col.GetColumnExtendedProperty(), true);
					}

					populatedTable.ColumnList.Add(populatedColumn);
				}
			}

			PopulateTableProperties(populatedTable);
		}

		protected void PopulateTableProperties(PopulatedTable populatedTable)
		{
			if (populatedTable.OrderByExpression == null)
			{
				var defaultOrderBy = GetDefaultOrderByExpression(populatedTable.TargetTableFullName);
				if (!String.IsNullOrWhiteSpace(defaultOrderBy))
				{
					populatedTable.OrderByExpression = defaultOrderBy;
				}
			}

			if (populatedTable.ColumnList.Select(c => (String.IsNullOrWhiteSpace(c.PopulateSource)) ? c.ColumnId.ToString(CultureInfo.InvariantCulture) : c.PopulateSource).Distinct().Count() > 1)
			{
				populatedTable.ColumnList.Where(c => !String.IsNullOrWhiteSpace(c.PopulateSource)).ToList().ForEach(c => c.PopulateSource = "LEFT " + c.PopulateSource);
			}

			var triggerExpression = String.Join(",\r\n\t\t", populatedTable.ColumnList.Where(c => !String.IsNullOrWhiteSpace(c.TriggerExpression)).Select(c => c.TriggerExpression));
			if (!String.IsNullOrWhiteSpace(triggerExpression))
			{
				populatedTable.ColumnList.Where(c => !String.IsNullOrWhiteSpace(c.TriggerExpression)).ToList().ForEach(c => c.TriggerSource = String.Format(CultureInfo.InvariantCulture, c.TriggerSource, triggerExpression));
			}
		}

		#endregion // Add and populate new columns

		#region Convert Char flags to Bit

		static string OnlineUpgradeConvertToBitStatusName
		{
			get { return "OnlineUpgradeConvertToBitStatus"; }
		}

		static string OnlineUpgradeConvertToBitWatermarkName
		{
			get { return "OnlineUpgradeConvertToBitWatermark"; }
		}

		DataTable GetAllColumnsToConvertCharToBit()
		{
			var newColumnPrefix = ColumnSynchroniser.ConvertCharToBitColumnPrefix;
			var sql = String.Format(CultureInfo.InvariantCulture, @"-- Get list of the columns to populate
SELECT
	TabSchema        = NewSchema.name,
	TabName          = NewTab.name,
	ColId            = NewCol.column_id,
	ColName          = NewCol.name,
	ColType          = NewType.name,
	ColLength        = NewCol.max_length,
	ColPrecision     = NewCol.precision,
	ColScale         = NewCol.scale,
	ColNullOrNotNull = CASE NewCol.is_nullable WHEN 1 THEN 'NULL' ELSE 'NOT NULL' END,
	ColSparse        = NewCol.is_sparse,
	ColDefault       = NewDefault.definition,
	ColExtProp       = ISNULL(CurColStatus.SEP_Value, 'None'),
	TabExtProp       = ISNULL(CurTabWatermark.SEP_Value, '0')
FROM
	[{1}].sys.schemas                       AS NewSchema
	JOIN [{1}].sys.tables                   AS NewTab     ON NewTab.schema_id = NewSchema.schema_id
	JOIN [{1}].sys.columns                  AS NewCol     ON NewCol.object_id = NewTab.object_id
	JOIN [{1}].sys.types                    AS NewType    ON NewType.user_type_id = NewCol.user_type_id
	LEFT JOIN [{1}].sys.default_constraints AS NewDefault ON NewDefault.object_id = NewCol.default_object_id

	JOIN [{0}].sys.schemas      AS CurSchema ON CurSchema.name = NewSchema.name
	JOIN [{0}].sys.tables       AS CurTab    ON CurTab.schema_id = CurSchema.schema_id AND CurTab.name = NewTab.name
	JOIN [{0}].sys.columns      AS CurCol    ON CurCol.object_id = CurTab.object_id AND CurCol.name = NewCol.name
	JOIN [{0}].sys.types        AS CurType   ON CurType.user_type_id = CurCol.user_type_id
	LEFT JOIN [{0}].sys.columns AS CurNewCol ON CurNewCol.object_id = CurCol.object_id AND CurNewCol.name = '{4}' + CurCol.name

	LEFT JOIN [{0}].dbo.StmExtendedProperty AS CurColStatus ON 1=1
		AND CurColStatus.SEP_Class = 'Column'
		AND CurColStatus.SEP_DatabaseNameSuffix = ''
		AND CurColStatus.SEP_SchemaName = CurSchema.name
		AND CurColStatus.SEP_MajorObjectName = CurTab.name
		AND CurColStatus.SEP_MinorObjectName = CurNewCol.name
		AND CurColStatus.SEP_Name = '{2}'

	LEFT JOIN [{0}].dbo.StmExtendedProperty AS CurTabWatermark ON 1=1
		AND CurTabWatermark.SEP_Class = 'Table'
		AND CurTabWatermark.SEP_DatabaseNameSuffix = ''
		AND CurTabWatermark.SEP_SchemaName = CurSchema.name
		AND CurTabWatermark.SEP_MajorObjectName = CurTab.name
		AND CurTabWatermark.SEP_MinorObjectName = ''
		AND CurTabWatermark.SEP_Name = '{3}'

WHERE 1=1
	-- Ignore microsoft-shipped tables
	AND CurTab.is_ms_shipped = 0
	AND NewTab.is_ms_shipped = 0

	-- Only columns to be converted from char to bit OR columns for that populating was started
	AND CurType.name in ('char', 'varchar')
	AND CurCol.max_length = 1
	AND NewType.name = 'bit'
	AND
	(1=2
		OR CurNewCol.name is NULL -- new columns were not created
		OR CurColStatus.SEP_Value is NOT NULL -- new columns were created but not completely populated
	)

OPTION (RECOMPILE);
"
				, targetDb                               // 0
				, templateDb                             // 1
				, OnlineUpgradeConvertToBitStatusName    // 2
				, OnlineUpgradeConvertToBitWatermarkName // 3
				, newColumnPrefix                        // 4
				);

			StoreQueryForTest(sql);

			return DataUtils.GetDataTableFromQuery(Db.Connection, sql);
		}

		void CreateConvertedColumnsCharToBit(IGrouping<string, ColumnChangeMetadata> table, PopulatedTable populatedTable)
		{
			var newColumnPrefix = ColumnSynchroniser.ConvertCharToBitColumnPrefix;
			foreach (var col in table.OrderBy(c => c.GetColumnId()))
			{
				var populatedColumn = new PopulatedColumn();
				var newColumnName = newColumnPrefix + col.ColumnName;

				populatedColumn.ColumnId = int.Parse(col.GetColumnId(), CultureInfo.InvariantCulture);
				populatedColumn.ColumnName = newColumnPrefix + col.ColumnName;
				populatedColumn.ColumnType = col.GetFullTypeDeclaration();
				populatedColumn.IsSparse = col.IsSparse;

				if (!col.IsSparse)
				{
					var fakeDefault = DataUtils.GetTheMostPopularValueForTheColumn(Db.Connection, targetDb, col.TableSchema, col.TableName, col.ColumnName);
					populatedColumn.ColumnDefaultConstraint = String.Format(CultureInfo.InvariantCulture,
						" CONSTRAINT {0} DEFAULT {1}"
						, (newColumnPrefix + col.CalculatedDefaultConstraintName).QuoteName() // 0
						, (String.Equals(fakeDefault, "Y", StringComparison.OrdinalIgnoreCase) ? "1" : "0") // 1
						);
				}

				populatedColumn.ColumnNullable = (col.IsNullable) ? "NULL" : "NOT NULL";

				populatedColumn.PopulateSource = null;

				if (col.IsNullable)
				{
					populatedColumn.PopulateExpression = String.Format(CultureInfo.InvariantCulture,
						"CASE {0} WHEN 'Y' THEN 1 WHEN 'N' THEN 0 END"
						, col.ColumnName.QuoteName() // 0
						);
					populatedColumn.PopulatePreAddWhereClause = String.Format(CultureInfo.InvariantCulture,
						"OR {0} is NULL AND NULLIF({1}, '') is NOT NULL OR {0} <> CASE {1} WHEN 'Y' THEN 1 WHEN 'N' THEN 0 END"
						, newColumnName.QuoteName()  // 0
						, col.ColumnName.QuoteName() // 1
						);
				}
				else
				{
					populatedColumn.PopulateExpression = String.Format(CultureInfo.InvariantCulture,
						"CASE {0} WHEN 'Y' THEN 1 ELSE 0 END"
						, col.ColumnName.QuoteName() // 0
						);
					populatedColumn.PopulatePreAddWhereClause = String.Format(CultureInfo.InvariantCulture,
						"OR {0} <> CASE {1} WHEN 'Y' THEN 1 ELSE 0 END"
						, newColumnName.QuoteName()  // 0
						, col.ColumnName.QuoteName() // 1
						);
				}

				populatedColumn.PopulateTargetWhereClause = populatedColumn.PopulatePreAddWhereClause;

				populatedColumn.TriggerSource = null;
				populatedColumn.TriggerExpression = null;

				if (!populatedTable.TargetTableStatus.HasValue)
				{
					populatedTable.TargetTableStatus = (TargetTableStatus)Enum.Parse(typeof(TargetTableStatus), col.GetColumnExtendedProperty(), true);
				}

				populatedTable.ColumnList.Add(populatedColumn);
			}

			if (populatedTable.OrderByExpression == null)
			{
				var defaultOrderBy = GetDefaultOrderByExpression(populatedTable.TargetTableFullName);
				if (!String.IsNullOrWhiteSpace(defaultOrderBy))
				{
					populatedTable.OrderByExpression = defaultOrderBy;
				}
			}
		}

		#endregion // Convert Char flags to Bit

		#region Convert [n][var]char to [n][var]char

		static string OnlineUpgradeConvertCharToCharStatusName
		{
			get { return "OnlineUpgradeConvertCharToCharStatus"; }
		}

		static string OnlineUpgradeConvertCharToCharWatermarkName
		{
			get { return "OnlineUpgradeConvertCharToCharWatermark"; }
		}

		DataTable GetAllColumnsToConvertCharToChar()
		{
			var newColumnPrefix = ColumnSynchroniser.ConvertCharToCharColumnPrefix;
			var sql = FormattableString.Invariant($@"-- Get list of the columns to populate
SELECT
	TabSchema        = NewSch.name,
	TabName          = NewTab.name,
	ColId            = NewCol.column_id,
	ColName          = NewCol.name,
	ColType          = NewTyp.name,
	ColLength        = IIF(NewCol.max_length > 0 AND NewTyp.name in (N'nchar', N'nvarchar'), NewCol.max_length / 2, NewCol.max_length),
	ColPrecision     = NewCol.precision,
	ColScale         = NewCol.scale,
	ColNullOrNotNull = IIF(NewCol.is_nullable = 1, 'NULL', 'NOT NULL'),
	ColSparse        = NewCol.is_sparse,
	ColDefault       = NewDef.definition,
	ColExtProp       = ISNULL(CurColStatus.SEP_Value, 'None'),
	TabExtProp       = ISNULL(CurTabWatermark.SEP_Value, '0')
FROM
	[{templateDb}].sys.schemas                       AS NewSch
	JOIN [{templateDb}].sys.tables                   AS NewTab ON NewTab.schema_id = NewSch.schema_id
	JOIN [{templateDb}].sys.columns                  AS NewCol ON NewCol.object_id = NewTab.object_id
	JOIN [{templateDb}].sys.types                    AS NewTyp ON NewTyp.user_type_id = NewCol.user_type_id
	LEFT JOIN [{templateDb}].sys.default_constraints AS NewDef ON NewDef.object_id = NewCol.default_object_id

	JOIN [{targetDb}].sys.schemas      AS CurSch    ON 1=1
		AND CurSch.name = NewSch.name
	JOIN [{targetDb}].sys.tables       AS CurTab    ON CurTab.schema_id = CurSch.schema_id
		AND CurTab.name = NewTab.name
	JOIN [{targetDb}].sys.columns      AS CurCol    ON CurCol.object_id = CurTab.object_id
		AND CurCol.name = NewCol.name
	JOIN [{targetDb}].sys.types        AS CurTyp    ON CurTyp.user_type_id = CurCol.user_type_id
	LEFT JOIN [{targetDb}].sys.columns AS CurNewCol ON CurNewCol.object_id = CurCol.object_id
		AND CurNewCol.name = '{newColumnPrefix}' + CurCol.name

	LEFT JOIN [{targetDb}].dbo.StmExtendedProperty AS CurColStatus ON 1=1
		AND CurColStatus.SEP_Class = 'Column'
		AND CurColStatus.SEP_DatabaseNameSuffix = ''
		AND CurColStatus.SEP_SchemaName = CurSch.name
		AND CurColStatus.SEP_MajorObjectName = CurTab.name
		AND CurColStatus.SEP_MinorObjectName = CurNewCol.name
		AND CurColStatus.SEP_Name = '{OnlineUpgradeConvertCharToCharStatusName}'

	LEFT JOIN [{targetDb}].dbo.StmExtendedProperty AS CurTabWatermark ON 1=1
		AND CurTabWatermark.SEP_Class = 'Table'
		AND CurTabWatermark.SEP_DatabaseNameSuffix = ''
		AND CurTabWatermark.SEP_SchemaName = CurSch.name
		AND CurTabWatermark.SEP_MajorObjectName = CurTab.name
		AND CurTabWatermark.SEP_MinorObjectName = ''
		AND CurTabWatermark.SEP_Name = '{OnlineUpgradeConvertCharToCharWatermarkName}'

	CROSS APPLY
	(
		SELECT
			Value = CONVERT(bit, IIF(EXISTS(
				SELECT NULL
				FROM
					[{templateDb}].sys.indexes            AS ind
					JOIN [{templateDb}].sys.index_columns AS icol ON icol.object_id = ind.object_id AND icol.index_id = ind.index_id
				WHERE 1=1
					AND ind.object_id = NewCol.object_id
					AND
					(1=2
						OR icol.column_id = NewCol.column_id
						OR ind.has_filter = 1 AND CHARINDEX(QUOTENAME(NewCol.name), ind.filter_definition) > 0
					)
				), 1, 0))
	) AS Indexed

WHERE 1=1
	-- Ignore microsoft-shipped tables
	AND CurTab.is_ms_shipped = 0
	AND NewTab.is_ms_shipped = 0

	-- Only columns to be converted from [n][var]char to [n][var]char
	AND CurTyp.name in (N'char', N'varchar', N'nchar', N'nvarchar')
	AND NewTyp.name in (N'char', N'varchar', N'nchar', N'nvarchar')

	-- Ignore computed columns
	AND CurCol.is_computed = 0
	AND NewCol.is_computed = 0

	-- Ignore decreasing size
	AND
		IIF(CurCol.max_length = -1
			, 32767
			, IIF(CurTyp.name in (N'nchar', N'nvarchar')
				, CurCol.max_length / 2
				, CurCol.max_length
				)
			)
		<=
		IIF(NewCol.max_length = -1
			, 32767
			, IIF(NewTyp.name in (N'nchar', N'nvarchar')
				, NewCol.max_length / 2
				, NewCol.max_length
				)
			)

	-- Ignore metadata-only operations
	AND
	(1=2
		OR CurTyp.name <> NewTyp.name
		OR
		(
			CurTyp.name = NewTyp.name
			AND CurCol.max_length <> NewCol.max_length
			AND
			(1=2
				OR CurTyp.name in (N'char', N'nchar')
				OR Indexed.Value = 1
			)
		)
	)

	-- Ignore Offline-only operations
	-- NOT NULL Columns of type varchar(max), nvarchar(max), varbinary(max), xml, text, ntext, image, hierarchyid, geometry, geography, or CLR UDTS, can't be added in an online operation
	AND NOT
	(
		NewTyp.name in (N'varchar', N'nvarchar')
		AND NewCol.max_length = -1
		AND NewCol.is_nullable = 0
	)

	-- Check Populate status
	AND
	(1=2
		OR CurNewCol.name is NULL -- new columns were not created
		OR CurColStatus.SEP_Value is NOT NULL -- new columns were created but not completely populated
	)

OPTION (RECOMPILE)

"
				);

			StoreQueryForTest(sql);

			return DataUtils.GetDataTableFromQuery(Db.Connection, sql);
		}

		void CreateConvertedColumnsCharToChar(IGrouping<string, ColumnChangeMetadata> table, PopulatedTable populatedTable)
		{
			var newColumnPrefix = ColumnSynchroniser.ConvertCharToCharColumnPrefix;
			foreach (var col in table.OrderBy(c => c.GetColumnId()))
			{
				var populatedColumn = new PopulatedColumn();
				var newColumnName = newColumnPrefix + col.ColumnName;

				populatedColumn.ColumnId = int.Parse(col.GetColumnId(), CultureInfo.InvariantCulture);
				populatedColumn.ColumnName = newColumnPrefix + col.ColumnName;
				populatedColumn.ColumnType = col.GetFullTypeDeclaration();
				populatedColumn.IsSparse = col.IsSparse;

				var defaultClause = col.GetDefaultClause() ?? "''";

				if (!col.IsSparse)
				{
					var fakeDefault = DataUtils.GetTheMostPopularValueForTheColumn(Db.Connection, targetDb, col.TableSchema, col.TableName, col.ColumnName);
					if (fakeDefault == null)
					{
						fakeDefault = (col.IsNullable) ? "NULL" : defaultClause;
					}
					else
					{
						fakeDefault = String.Format(CultureInfo.InvariantCulture, "'{0}'", DataUtils.EscapeSingleQuotes(fakeDefault.TrimEnd(' ')));
					}

					populatedColumn.ColumnDefaultConstraint = String.Format(CultureInfo.InvariantCulture,
						" CONSTRAINT {0} DEFAULT {1}"
						, (newColumnPrefix + col.CalculatedDefaultConstraintName).QuoteName() // 0
						, fakeDefault                                                         // 1
						);
				}

				populatedColumn.ColumnNullable = (col.IsNullable) ? "NULL" : "NOT NULL";

				populatedColumn.PopulateSource = null;

				if (col.IsNullable)
				{
					populatedColumn.PopulateExpression = String.Format(CultureInfo.InvariantCulture,
						"RTRIM(CONVERT({0}, {1}))"
						, populatedColumn.ColumnType // 0
						, col.ColumnName.QuoteName() // 1
						);
				}
				else
				{
					populatedColumn.PopulateExpression = String.Format(CultureInfo.InvariantCulture,
						"ISNULL(RTRIM(CONVERT({0}, {1})), {2})"
						, populatedColumn.ColumnType // 0
						, col.ColumnName.QuoteName() // 1
						, defaultClause              // 2
						);
				}

				populatedColumn.PopulatePreAddWhereClause = String.Format(CultureInfo.InvariantCulture,
					"OR {0} is NULL AND {1} is NOT NULL OR {0} is NOT NULL AND {1} is NULL OR {0} <> {1}"
					, newColumnName.QuoteName()  // 0
					, col.ColumnName.QuoteName() // 1
					);
				populatedColumn.PopulateTargetWhereClause = populatedColumn.PopulatePreAddWhereClause;

				populatedColumn.TriggerSource = null;
				populatedColumn.TriggerExpression = null;

				if (!populatedTable.TargetTableStatus.HasValue)
				{
					populatedTable.TargetTableStatus = (TargetTableStatus)Enum.Parse(typeof(TargetTableStatus), col.GetColumnExtendedProperty(), true);
				}

				populatedTable.ColumnList.Add(populatedColumn);
			}

			if (populatedTable.OrderByExpression == null)
			{
				var defaultOrderBy = GetDefaultOrderByExpression(populatedTable.TargetTableFullName);
				if (!String.IsNullOrWhiteSpace(defaultOrderBy))
				{
					populatedTable.OrderByExpression = defaultOrderBy;
				}
			}
		}

		#endregion // Convert [n][var]char to [n][var]char

		#region Convert Decimal to Decimal

		static string OnlineUpgradeConvertDecimalToDecimalStatusName => "OnlineUpgradeConvertDecimalToDecimalStatus";

		static string OnlineUpgradeConvertDecimalToDecimalWatermarkName => "OnlineUpgradeConvertDecimalToDecimalWatermark";

		DataTable GetAllColumnsToConvertDecimalToDecimal()
		{
			var newColumnPrefix = ColumnSynchroniser.ConvertDecimalToDecimalColumnPrefix;
			var sql = $@"-- {nameof(GetAllColumnsToConvertDecimalToDecimal)}
SELECT
	TabSchema        = NewSchema.name,
	TabName          = NewTab.name,
	ColId            = NewCol.column_id,
	ColName          = NewCol.name,
	ColType          = NewType.name,
	ColLength        = NewCol.max_length,
	ColPrecision     = NewCol.precision,
	ColScale         = NewCol.scale,
	ColNullOrNotNull = IIF(NewCol.is_nullable = 1, 'NULL', 'NOT NULL'),
	ColSparse        = NewCol.is_sparse,
	ColDefault       = NewDefault.definition,
	ColExtProp       = ISNULL(CurColStatus.SEP_Value, 'None'),
	TabExtProp       = ISNULL(CurTabWatermark.SEP_Value, '0')
FROM
	[{templateDb}].sys.schemas                       AS NewSchema
	JOIN [{templateDb}].sys.tables                   AS NewTab     ON NewTab.schema_id = NewSchema.schema_id
	JOIN [{templateDb}].sys.columns                  AS NewCol     ON NewCol.object_id = NewTab.object_id
	JOIN [{templateDb}].sys.types                    AS NewType    ON NewType.user_type_id = NewCol.user_type_id
	LEFT JOIN [{templateDb}].sys.default_constraints AS NewDefault ON NewDefault.object_id = NewCol.default_object_id

	JOIN [{targetDb}].sys.schemas      AS CurSchema ON CurSchema.name = NewSchema.name
	JOIN [{targetDb}].sys.tables       AS CurTab    ON CurTab.schema_id = CurSchema.schema_id AND CurTab.name = NewTab.name
	JOIN [{targetDb}].sys.columns      AS CurCol    ON CurCol.object_id = CurTab.object_id AND CurCol.name = NewCol.name
	JOIN [{targetDb}].sys.types        AS CurType   ON CurType.user_type_id = CurCol.user_type_id
	LEFT JOIN [{targetDb}].sys.columns AS CurNewCol ON CurNewCol.object_id = CurCol.object_id AND CurNewCol.name = '{newColumnPrefix}' + CurCol.name

	LEFT JOIN [{targetDb}].dbo.StmExtendedProperty AS CurColStatus ON 1=1
		AND CurColStatus.SEP_Class = 'Column'
		AND CurColStatus.SEP_DatabaseNameSuffix = ''
		AND CurColStatus.SEP_SchemaName = CurSchema.name
		AND CurColStatus.SEP_MajorObjectName = CurTab.name
		AND CurColStatus.SEP_MinorObjectName = CurNewCol.name
		AND CurColStatus.SEP_Name = '{OnlineUpgradeConvertDecimalToDecimalStatusName}'

	LEFT JOIN [{targetDb}].dbo.StmExtendedProperty AS CurTabWatermark ON 1=1
		AND CurTabWatermark.SEP_Class = 'Table'
		AND CurTabWatermark.SEP_DatabaseNameSuffix = ''
		AND CurTabWatermark.SEP_SchemaName = CurSchema.name
		AND CurTabWatermark.SEP_MajorObjectName = CurTab.name
		AND CurTabWatermark.SEP_MinorObjectName = ''
		AND CurTabWatermark.SEP_Name = '{OnlineUpgradeConvertDecimalToDecimalWatermarkName}'
WHERE 1=1
	-- Ignore microsoft-shipped tables
	AND CurTab.is_ms_shipped = 0
	AND NewTab.is_ms_shipped = 0

	-- Ignore computed columns
	AND CurCol.is_computed = 0
	AND NewCol.is_computed = 0

	-- Only columns to be converted from decimal to decimal OR columns for that populating was started
	AND CurType.name = 'decimal'
	AND NewType.name = 'decimal'

	-- Exclude columns that are staying the same, losing precision, losing scale, or losing leading digits
	AND (NewCol.precision <> CurCol.precision OR NewCol.scale <> CurCol.scale)
	AND NewCol.precision >= CurCol.precision
	AND NewCol.scale >= CurCol.scale
	AND NewCol.precision - NewCol.scale >= CurCol.precision - CurCol.scale

	-- Check Populate status
	AND
	(1=2
		OR CurNewCol.name is NULL -- new columns were not created
		OR CurColStatus.SEP_Value is NOT NULL -- new columns were created but not completely populated
	)

OPTION (RECOMPILE);
";

			StoreQueryForTest(sql);

			return DataUtils.GetDataTableFromQuery(Db.Connection, sql);
		}

		void CreateConvertedColumnsDecimalToDecimal(IEnumerable<ColumnChangeMetadata> table, PopulatedTable populatedTable)
		{
			var newColumnPrefix = ColumnSynchroniser.ConvertDecimalToDecimalColumnPrefix;
			foreach (var col in table.OrderBy(c => c.GetColumnId()))
			{
				var newColumnName = newColumnPrefix + col.ColumnName;
				var defaultClause = col.GetDefaultClause() ?? "0.";

				var populatedColumn = new PopulatedColumn
				{
					ColumnId = int.Parse(col.GetColumnId(), CultureInfo.InvariantCulture),
					ColumnName = newColumnPrefix + col.ColumnName,
					ColumnType = col.GetFullTypeDeclaration(),
					IsSparse = col.IsSparse,
					ColumnNullable = col.IsNullable ? "NULL" : "NOT NULL",
					PopulateSource = null,
					TriggerSource = null,
					TriggerExpression = null
				};

				if (!col.IsSparse)
				{
					var fakeDefault = DataUtils.GetTheMostPopularValueForTheColumn(Db.Connection, targetDb, col.TableSchema, col.TableName, col.ColumnName);
					if (fakeDefault is null)
					{
						fakeDefault = col.IsNullable ? "NULL" : defaultClause;
					}
					else
					{
						fakeDefault = DataUtils.EscapeSingleQuotes(fakeDefault.TrimEnd(' '));
					}
					populatedColumn.ColumnDefaultConstraint = $" CONSTRAINT {(newColumnPrefix + col.CalculatedDefaultConstraintName).QuoteName()} DEFAULT {fakeDefault}";
				}

				populatedColumn.PopulateExpression = string.Format(CultureInfo.InvariantCulture,
					col.IsNullable ? "CONVERT({0}, {1})" : "ISNULL(CONVERT({0}, {1}), {2})",
					populatedColumn.ColumnType,
					col.ColumnName.QuoteName(),
					defaultClause
				);

				populatedColumn.PopulateTargetWhereClause = populatedColumn.PopulatePreAddWhereClause = string.Format(CultureInfo.InvariantCulture,
					"OR {0} is NULL AND {1} is NOT NULL OR {1} is NOT NULL AND {1} is NULL OR {0} <> {1}",
					newColumnName.QuoteName(),
					col.ColumnName.QuoteName()
				);

				populatedTable.TargetTableStatus ??= (TargetTableStatus)Enum.Parse(typeof(TargetTableStatus), col.GetColumnExtendedProperty(), ignoreCase: true);

				populatedTable.ColumnList.Add(populatedColumn);
			}

			if (populatedTable.OrderByExpression != null) { return; }
			var defaultOrderBy = GetDefaultOrderByExpression(populatedTable.TargetTableFullName);
			if (!string.IsNullOrWhiteSpace(defaultOrderBy))
			{
				populatedTable.OrderByExpression = defaultOrderBy;
			}
		}

		#endregion // Convert Decimal to Decimal

		#region Convert DateTimes to Date

		static string OnlineUpgradeConvertDateTimesToDateStatusName => "OnlineUpgradeConvertDateTimesToDateStatus";

		static string OnlineUpgradeConvertDateTimesToDateWatermarkName => "OnlineUpgradeConvertDateTimesToDateWatermarkName";

		DataTable GetAllColumnsToConvertDateTimesToDate()
		{
			var newColumnPrefix = ColumnSynchroniser.ConvertDateTimesToDateColumnPrefix;
			var sql = $@"-- {nameof(GetAllColumnsToConvertDateTimesToDate)}
SELECT
	TabSchema        = NewSchema.name,
	TabName          = NewTab.name,
	ColId            = NewCol.column_id,
	ColName          = NewCol.name,
	ColType          = NewType.name,
	ColLength        = NewCol.max_length,
	ColPrecision     = NewCol.precision,
	ColScale         = NewCol.scale,
	ColNullOrNotNull = IIF(NewCol.is_nullable = 1, 'NULL', 'NOT NULL'),
	ColSparse        = NewCol.is_sparse,
	ColDefault       = NewDefault.definition,
	ColExtProp       = ISNULL(CurColStatus.SEP_Value, 'None'),
	TabExtProp       = ISNULL(CurTabWatermark.SEP_Value, '0')
FROM
	[{templateDb}].sys.schemas                       AS NewSchema
	JOIN [{templateDb}].sys.objects                  AS NewTab     ON NewTab.schema_id = NewSchema.schema_id
	JOIN [{templateDb}].sys.columns                  AS NewCol     ON NewCol.object_id = NewTab.object_id
	JOIN [{templateDb}].sys.types                    AS NewType    ON NewType.user_type_id = NewCol.user_type_id
	LEFT JOIN [{templateDb}].sys.default_constraints AS NewDefault ON NewDefault.object_id = NewCol.default_object_id

	JOIN [{targetDb}].sys.schemas      AS CurSchema ON CurSchema.name = NewSchema.name
	JOIN [{targetDb}].sys.objects      AS CurTab    ON CurTab.schema_id = CurSchema.schema_id AND CurTab.name = NewTab.name
	JOIN [{targetDb}].sys.columns      AS CurCol    ON CurCol.object_id = CurTab.object_id AND CurCol.name = NewCol.name
	JOIN [{targetDb}].sys.types        AS CurType   ON CurType.user_type_id = CurCol.user_type_id
	LEFT JOIN [{targetDb}].sys.columns AS CurNewCol ON CurNewCol.object_id = CurCol.object_id AND CurNewCol.name = N{newColumnPrefix.QuoteName('\'')} + CurCol.name

	LEFT JOIN [{targetDb}].dbo.StmExtendedProperty AS CurColStatus ON 1=1
		AND CurColStatus.SEP_Class = 'Column'
		AND CurColStatus.SEP_DatabaseNameSuffix = ''
		AND CurColStatus.SEP_SchemaName = CurSchema.name
		AND CurColStatus.SEP_MajorObjectName = CurTab.name
		AND CurColStatus.SEP_MinorObjectName = CurNewCol.name
		AND CurColStatus.SEP_Name = '{OnlineUpgradeConvertDateTimesToDateStatusName}'

	LEFT JOIN [{targetDb}].dbo.StmExtendedProperty AS CurTabWatermark ON 1=1
		AND CurTabWatermark.SEP_Class = 'Table'
		AND CurTabWatermark.SEP_DatabaseNameSuffix = ''
		AND CurTabWatermark.SEP_SchemaName = CurSchema.name
		AND CurTabWatermark.SEP_MajorObjectName = CurTab.name
		AND CurTabWatermark.SEP_MinorObjectName = ''
		AND CurTabWatermark.SEP_Name = '{OnlineUpgradeConvertDateTimesToDateWatermarkName}'
WHERE 1=1
	AND CurTab.type = 'U'
	AND NewTab.type = 'U'

	-- Ignore microsoft-shipped tables
	AND CurTab.is_ms_shipped = 0
	AND NewTab.is_ms_shipped = 0

	-- Ignore computed columns
	AND CurCol.is_computed = 0
	AND NewCol.is_computed = 0

	-- Only columns to be converted from smalldatetime, datetime or datetime2 to date OR columns for that populating was started
	AND CurType.name IN (N'smalldatetime', N'datetime', N'datetime2')
	AND NewType.name = N'date'
	AND
		(1=2
			OR CurNewCol.name is NULL -- new columns were not created
			OR CurColStatus.SEP_Value is NOT NULL -- new columns were created but not completely populated
		)

OPTION (RECOMPILE);
";

			StoreQueryForTest(sql);

			return DataUtils.GetDataTableFromQuery(Db.Connection, sql);
		}

		void CreateConvertedColumnsDateTimesToDate(IEnumerable<ColumnChangeMetadata> table, PopulatedTable populatedTable)
		{
			var newColumnPrefix = ColumnSynchroniser.ConvertDateTimesToDateColumnPrefix;
			foreach (var col in table.OrderBy(c => c.GetColumnId()))
			{
				var newColumnName = newColumnPrefix + col.ColumnName;
				var utcSuffix = "Utc";
				var fallbackDefaultClause = col.ColumnName.EndsWith(utcSuffix, StringComparison.OrdinalIgnoreCase) ? "GETUTCDATE()" : "GETDATE()";
				var defaultClause = col.GetDefaultClause() ?? fallbackDefaultClause;

				var populatedColumn = new PopulatedColumn
				{
					ColumnId = int.Parse(col.GetColumnId(), CultureInfo.InvariantCulture),
					ColumnName = newColumnName,
					ColumnType = col.GetFullTypeDeclaration(),
					ColumnNullable = col.IsNullable ? "NULL" : "NOT NULL",
					IsSparse = col.IsSparse,
					PopulateSource = null,
					TriggerSource = null,
					TriggerExpression = null
				};

				if (!col.IsSparse)
				{
					var fakeDefault = col.IsNullable ? "NULL" : defaultClause;
					populatedColumn.ColumnDefaultConstraint = $" CONSTRAINT {(newColumnPrefix + col.CalculatedDefaultConstraintName).QuoteName()} DEFAULT {fakeDefault}";
				}

				populatedColumn.PopulateExpression = string.Format(CultureInfo.InvariantCulture,
					col.IsNullable ? "CONVERT({0}, {1})" : "ISNULL(CONVERT({0}, {1}), {2})",
					populatedColumn.ColumnType,
					col.ColumnName.QuoteName(),
					defaultClause
				);

				populatedColumn.PopulateTargetWhereClause = populatedColumn.PopulatePreAddWhereClause = string.Format(CultureInfo.InvariantCulture,
					"OR {0} is NULL AND {1} is NOT NULL OR {0} is NOT NULL AND {1} is NULL OR {0} <> {2}",
					newColumnName.QuoteName(),
					col.ColumnName.QuoteName(),
					populatedColumn.PopulateExpression
				);

				var setTargetWithAuditColumnsExpression = $"{populatedColumn.ColumnName.QuoteName()} = {populatedColumn.PopulateExpression}";
				var systemLastEditColumns = GetSystemLastEditColumns(populatedTable, new List<string>()
				{
					setTargetWithAuditColumnsExpression
				});

				if (systemLastEditColumns.Count > 0)
				{
					setTargetWithAuditColumnsExpression += $", {String.Join(",\r\n\t", systemLastEditColumns)}";
				}

				populatedColumn.TriggerSource = @$"
CREATE TRIGGER {populatedColumn.ColumnName}_DateTimesToDateTrigger
    ON {populatedTable.TargetTableFullName}
    AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON
    
    if (NOT EXISTS(SELECT NULL FROM inserted)) RETURN

    if (UPDATE ({col.ColumnName.QuoteName()}))
    begin
        UPDATE target SET
        {setTargetWithAuditColumnsExpression}
		FROM {populatedTable.TargetTableFullName} AS target
        WHERE 1=0 {populatedColumn.PopulateTargetWhereClause};
    END

END
";

				populatedTable.TargetTableStatus ??= (TargetTableStatus)Enum.Parse(typeof(TargetTableStatus), col.GetColumnExtendedProperty(), ignoreCase: true);

				populatedTable.ColumnList.Add(populatedColumn);
			}

			if (populatedTable.OrderByExpression == null)
			{
				var defaultOrderBy = GetDefaultOrderByExpression(populatedTable.TargetTableFullName);
				if (!String.IsNullOrWhiteSpace(defaultOrderBy))
				{
					populatedTable.OrderByExpression = defaultOrderBy;
				}
			}
		}

		#endregion

		void CreateTargetColumnsWithStatus(PopulatedTable table, string statusName, TargetTableStatus statusValue)
		{
			using (var transactionManager = Db.Connection.BeginTransactionWithManager())
			{
				CreateTargetColumns(table);
				CreateTargetTriggers(table);
				SetStatus(table, statusName, statusValue);
				transactionManager.CommitTransaction();
			}

			RefreshDependentScripts(Db.Connection, table.TargetTableDb, table.TargetTableSchema, table.TargetTableName, Manager);
		}

		void CreatePreAddColumnsWithStatus(PopulatedTable table, string statusName, string watermarkName, TargetTableStatus statusValue)
		{
			Manager.ShowInfoMessage("    : Recording PKs");

			using (var transactionManager = Db.Connection.BeginTransactionWithManager())
			{
				DropPopulatingWatermark(table, watermarkName);
				CreatePreAddTable(table);
				PopulatePreAddColumns(table);
				SetStatus(table, statusName, statusValue);

				transactionManager.CommitTransaction();
			}
		}

		void DropPreAddTableAndStatuses(PopulatedTable table, string statusName)
		{
			using (var transactionManager = Db.Connection.BeginTransactionWithManager())
			{
				DropPreAddTable(table);
				foreach (var col in table.ColumnList)
				{
					ExtProperty.Column.Delete(Db.Connection, table.TargetTableSchema, table.TargetTableName, col.ColumnName, statusName);
				}

				transactionManager.CommitTransaction();
			}
		}

		#region Populated tables

		#region HelperClasses

		class PopulateTargetColumnsConfiguration
		{
			public int TopRowCount { get; set; } = 3000;
			public TimeSpan TimeConsumingThreshold { get; set; } = TimeSpan.FromMinutes(1d);
		}

		#endregion //HelperClasses

		public List<PopulatedTable> GetColumnsToPopulate(DataTable columnsToPopulate, Action<IGrouping<string, ColumnChangeMetadata>, PopulatedTable> createColumns)
		{
			var tables = new List<PopulatedTable>();

			if (columnsToPopulate.Rows.Count > 0)
			{
				CreatePopulatedTables(columnsToPopulate, tables, createColumns);
			}

			return tables;
		}

		void CreatePopulatedTables(DataTable columnsToAdd, List<PopulatedTable> tables, Action<IGrouping<string, ColumnChangeMetadata>, PopulatedTable> createColumns)
		{
			var columns = columnsToAdd.Rows.Cast<DataRow>()
				.Select(row => new ColumnChangeMetadata(row))
				.GroupBy(col => col.TableSchema, col => col, StringComparer.OrdinalIgnoreCase);

			foreach (var schema in columns.OrderBy(s => s.Key))
			{
				foreach (var table in schema.GroupBy(s => s.TableName, col => col, StringComparer.OrdinalIgnoreCase).OrderBy(t => t.Key))
				{
					var populatedTable = new PopulatedTable(targetDb, schema.Key, table.Key, preAddDb, GetPreAddTableName(table.Key));
					if (GetSinglePrimaryKeyColumnInfo(schema.Key, table.Key, out var tablePKColumnName, out var tablePKColumnType))
					{
						populatedTable.TargetTablePKColumnName = tablePKColumnName;
						populatedTable.TargetTablePKColumnType = tablePKColumnType;
						populatedTable.PopulatingWatermark = int.Parse(table.FirstOrDefault().GetTableExtendedProperty(), CultureInfo.InvariantCulture);

						createColumns(table, populatedTable);

						if (populatedTable.ColumnList.Count > 0)
						{
							populatedTable.TargetTableSize = GetTableSize_MB(populatedTable);
							tables.Add(populatedTable);
						}
					}
				}
			}
		}

		string GetDefaultOrderByExpression(string tableFullName)
		{
			var sql = String.Format(CultureInfo.InvariantCulture, @"-- Get SystemCreateTime field name if exist
SELECT TOP(1)
	name
FROM
	[{0}].sys.columns
WHERE
	object_id = OBJECT_ID(N'{1}', N'U')
	AND name LIKE '%[_]SystemCreateTime%'
;"
				, targetDb      // 0
				, tableFullName // 1
				);

			StoreQueryForTest(sql);

			var orderByExpression = String.Empty;
			var createTimeColumn = Db.Connection.ExecuteScalar(sql);
			if (createTimeColumn != null && createTimeColumn != DBNull.Value)
			{
				orderByExpression = createTimeColumn.ToString();
			}

			return orderByExpression;
		}

		bool GetSinglePrimaryKeyColumnInfo(string tableSchema, string tableName, out string columnName, out string columnDataType)
		{
			columnName = null;
			columnDataType = null;

			var sql = String.Format(CultureInfo.InvariantCulture, @"-- Get PK column name and type
SELECT TOP(2)
	col.name,
	typ.name
FROM
	[{0}].sys.key_constraints    AS kc
	JOIN [{0}].sys.tables        AS tab ON tab.object_id = kc.parent_object_id
	JOIN [{0}].sys.schemas       AS sch ON sch.schema_id = tab.schema_id
	JOIN [{0}].sys.index_columns AS ixc ON ixc.object_id = tab.object_id AND ixc.index_id = kc.unique_index_id
	JOIN [{0}].sys.columns       AS col ON col.object_id = tab.object_id AND col.column_id = ixc.column_id
	JOIN [{0}].sys.types         AS typ ON typ.system_type_id = col.system_type_id
WHERE
	kc.type = 'PK'
	AND sch.name = '{1}'
	AND tab.name = '{2}'
OPTION (RECOMPILE);
"
				, targetDb    // 0
				, tableSchema // 1
				, tableName   // 2
				);

			//StoreQueryForTest(sql);

			using (var cmd = Db.Connection.Command(sql))
			using (var reader = cmd.ExecuteReader())
			{
				if (reader.Read())
				{
					columnName = (string)reader[0];
					columnDataType = (string)reader[1];

					if (reader.Read())
					{
						columnName = null;
						columnDataType = null;
					}
				}
			}

			return (columnName != null && columnDataType != null);
		}

		#endregion //Populated tables

		#region Pre-add table

		protected virtual string GetPreAddTableName(string tableName)
		{
			return String.Format(CultureInfo.InvariantCulture, "{0}__{1}", preAddTablePrefix, tableName);
		}

		static void CreatePreAddDb(int dataInitialSizeMb, int dataGrowthMb, int logInitialSizeMb, int logGrowthMb)
		{
			using (var connection = Db.NewAdminConnection())
			{
				if (!DbObjectCreator.DatabaseExists(connection, preAddDb))
				{
					connection.CreateDatabase(databaseName: preAddDb
						, dataPath: null
						, logPath: null
						, dataInitialSizeMb: dataInitialSizeMb
						, logInitialSizeMb: logInitialSizeMb
						, dataGrowthMb: dataGrowthMb
						, logGrowthMb: logGrowthMb
						);

					SetDbSettings(connection
						, dbName: preAddDb
						, autoCreateStatistics: false
						, autoUpdateStatistics: false
						, autoUpdateStatisticsAsynchronously: false
						, allowSnapshotIsolation: true
						, parameterization: DbParameterization.SIMPLE
						);
				}

				CreateDbSchemas(connection, preAddDb);
			}
		}

		static void CreateDbSchemas(DbConnection connection, string dbName)
		{
			using (((ICurrentDbControl)connection).UseDatabase(dbName))
			{
				foreach (var schemaName in Db.CW1AdditionalSchemas)
				{
					if (!SchemaExists(connection, schemaName))
					{
						connection.ExecuteNonQuery(FormattableString.Invariant($"CREATE SCHEMA {schemaName.QuoteName()}"));
					}
				}
			}
		}

		static bool SchemaExists(DbConnection connection, string name)
			=> connection.Exists("FROM sys.schemas WHERE name = @schemaName", cmd => cmd.AddParameter("@schemaName", SqlDbType.NVarChar, 128, name));

		#region PreAddDbSettings

		enum DbParameterization
		{
			SIMPLE,
			FORCED,
		}

		static void SetDbSettings(AdminConnection connection, string dbName, bool autoCreateStatistics, bool autoUpdateStatistics, bool autoUpdateStatisticsAsynchronously, bool allowSnapshotIsolation, DbParameterization parameterization)
		{
			var sql = String.Format(CultureInfo.InvariantCulture, @"
ALTER DATABASE [{0}] SET RECOVERY                     {1} WITH NO_WAIT;
ALTER DATABASE [{0}] SET AUTO_CREATE_STATISTICS       {2} WITH NO_WAIT;
ALTER DATABASE [{0}] SET AUTO_UPDATE_STATISTICS       {3} WITH NO_WAIT;
ALTER DATABASE [{0}] SET AUTO_UPDATE_STATISTICS_ASYNC {4} WITH NO_WAIT;
ALTER DATABASE [{0}] SET ALLOW_SNAPSHOT_ISOLATION     {5};
ALTER DATABASE [{0}] SET READ_COMMITTED_SNAPSHOT      {5} WITH NO_WAIT;
ALTER DATABASE [{0}] SET PARAMETERIZATION             {6} WITH NO_WAIT;
"
				, dbName                                            // 0
				, "SIMPLE"                                          // 1
				, autoCreateStatistics ? "ON" : "OFF"               // 2
				, autoUpdateStatistics ? "ON" : "OFF"               // 3
				, autoUpdateStatisticsAsynchronously ? "ON" : "OFF" // 4
				, allowSnapshotIsolation ? "ON" : "OFF"             // 5
				, parameterization.ToString()                       // 6
				);

			using (((ICurrentDbControl)connection).UseDatabase(Db.SqlMasterDb))
			{
				connection.ExecuteNonQuery(sql);
			}
		}

		#endregion // PreAddDbSettings

		bool PreAddTableExists(PopulatedTable table)
		{
			var sql = String.Format(CultureInfo.InvariantCulture, @"-- Does table exist?
SELECT CONVERT(bit, ISNULL(OBJECT_ID(N'{0}', N'U'), 0));
"
				, table.PreAddTableFullName // 0
				);

			StoreQueryForTest(sql);
			return (bool)Db.Connection.ExecuteScalar(sql);
		}

		void CreatePreAddTable(PopulatedTable table)
		{
			var preAddColumnList = table.ColumnList
				.Where(c => !String.IsNullOrWhiteSpace(c.PopulateSource))
				.Select(c => String.Format(CultureInfo.InvariantCulture, "{0} {1} {2} NULL", c.ColumnName.QuoteName(), c.ColumnType, c.Sparse))
				.ToList();
			var preAddColumns = (preAddColumnList.Count == 0) ? String.Empty : String.Format(CultureInfo.InvariantCulture, ",\r\n\t\t{0}", String.Join(",\r\n\t\t", preAddColumnList));

			var sql = String.Format(CultureInfo.InvariantCulture, @"-- Create pre-add table if not exist
if (OBJECT_ID(N'{0}', N'U') is NULL)
begin
	CREATE TABLE {0}
	(
		Id int IDENTITY(1, 1) NOT NULL PRIMARY KEY CLUSTERED,
		PK {1} NOT NULL{2}
	);
end
"
				, table.PreAddTableFullName     // 0
				, table.TargetTablePKColumnType // 1
				, preAddColumns                 // 2
				);

			StoreQueryForTest(sql);
			Db.Connection.ExecuteNonQuery(sql);
		}

		protected virtual void PopulatePreAddColumns(PopulatedTable table)
		{
			var columnNameList = table.ColumnList.Where(c => !String.IsNullOrWhiteSpace(c.PopulateSource)).Select(c => c.ColumnName.QuoteName()).ToList();
			var columnNames = (columnNameList.Count == 0) ? String.Empty : String.Format(CultureInfo.InvariantCulture, ", {0}", String.Join(", ", columnNameList));

			var selectColumnList = table.ColumnList.Where(c => !String.IsNullOrWhiteSpace(c.PopulateSource)).Select(c => String.Format(CultureInfo.InvariantCulture, "{0} = {1}", c.ColumnName.QuoteName(), c.PopulateExpression)).ToList();
			var selectColumns = (selectColumnList.Count == 0) ? String.Empty : String.Format(CultureInfo.InvariantCulture, ",\r\n\t{0}", String.Join(",\r\n\t", selectColumnList));

			var populateSourceList = table.ColumnList.Where(c => !String.IsNullOrWhiteSpace(c.PopulateSource)).Select(c => c.PopulateSource).Distinct().ToList();
			var populateSources = (populateSourceList.Count == 0) ? String.Empty : String.Format(CultureInfo.InvariantCulture, "\r\n\t{0}", String.Join("\r\n\t", populateSourceList));

			var whereClauseList = table.ColumnList.Where(c => !String.IsNullOrWhiteSpace(c.PopulatePreAddWhereClause)).Select(c => c.PopulatePreAddWhereClause).ToList();
			var whereClause = (whereClauseList.Count == 0) ? String.Empty : String.Join("\r\n\t", whereClauseList);

			var orderByExpression = (String.IsNullOrWhiteSpace(table.OrderByExpression)) ? String.Empty : String.Format(CultureInfo.InvariantCulture, "\r\nORDER BY\r\n\t{0}", table.OrderByExpression);

			var sql = String.Format(CultureInfo.InvariantCulture, @"-- Populating pre-add table
SET NOCOUNT ON;

INSERT {0} WITH (TABLOCKX) (PK{1})
SELECT
	PK = target.[{2}]{3}
FROM
	{4} AS target WITH(READPAST, READCOMMITTEDLOCK){5}
WHERE 1=2
	{6}{7}
OPTION (RECOMPILE);
"
				, table.PreAddTableFullName     // 0
				, columnNames                   // 1
				, table.TargetTablePKColumnName // 2
				, selectColumns                 // 3
				, table.TargetTableFullName     // 4
				, populateSources               // 5
				, whereClause                   // 6
				, orderByExpression             // 7
				);

			StoreQueryForTest(sql);
			Db.Connection.ExecuteNonQuery(sql);
		}

		void DropPreAddTable(PopulatedTable table)
		{
			var sql = String.Format(CultureInfo.InvariantCulture, @"-- Dropping pre-add table
if (OBJECT_ID(N'{0}', N'U') is NOT NULL) DROP TABLE {0};
"
				, table.PreAddTableFullName // 0
				);

			StoreQueryForTest(sql);
			Db.Connection.ExecuteNonQuery(sql);
		}

		#endregion //Pre-add table

		#region Target table

		protected void CreateTargetColumns(PopulatedTable table)
		{
			var columns = String.Join(",\r\n\t",
				table.ColumnList.Where(c => !ColumnExists(table, c.ColumnName))
					.Select(c => String.Format(CultureInfo.InvariantCulture,
						"{0} {1} {2} {3} {4}",
						c.ColumnName.QuoteName(),     // 0
						c.ColumnType,                 // 1
						c.Sparse,                     // 2
						c.ColumnNullable,             // 3
						c.ColumnDefaultConstraint))); // 4

			if (!string.IsNullOrEmpty(columns))
			{
				var sql = String.Format(CultureInfo.InvariantCulture, @"-- Create new columns
ALTER TABLE {0} ADD
	{1}
;"
					, table.TargetTableFullName // 0
					, columns                   // 1
				);

				StoreQueryForTest(sql);
				Db.Connection.ExecuteNonQuery(sql);
			}
		}

		bool ColumnExists(PopulatedTable table, string columnName) => DbObjectCreator.ColumnExists(Db.Connection, TargetDb, table.TargetTableSchema, table.TargetTableName, columnName);

		void CreateTargetTriggers(PopulatedTable table)
		{
			foreach (var sql in table.ColumnList.Where(c => !String.IsNullOrWhiteSpace(c.TriggerSource)).Select(c => c.TriggerSource).Distinct())
			{
				StoreQueryForTest(sql);
				Db.Connection.ExecuteNonQuery(sql);
			}
		}

		protected virtual IEnumerable<string> GetCustomUpdateColumns(PopulatedTable table)
		{
			return Enumerable.Empty<string>();
		}

		void PopulateTargetColumns(PopulatedTable table, string statusName, string watermarkName, TargetTableStatus statusValue)
		{
			Manager.ShowInfoMessage("    : Updating with default value");

			RunUserAction_ForTest(table.TargetTableFullName);

			var id_FromParameterName = "@id_from";
			var id_ToParameterName = "@id_to";
			var sql = CreatePopulateTargetColumns(table, id_FromParameterName, id_ToParameterName);

			StoreQueryForTest(sql);

			int currentWatermark = Math.Max(table.PopulatingWatermark, 1);
			var maximumWatermark = GetMaximumWatermark(preAddDb, table.TargetTableSchema, GetPreAddTableName(table.TargetTableName));
			LogRecordsRemaining();

			var logFullnessProvider = new CargoWise.Data.SqlServer.LogFullnessProvider();
			var backlogWaiter = new CargoWise.Data.SqlServer.BacklogWaiter(new[] { logFullnessProvider });
			var sw = Stopwatch.StartNew();
			while (currentWatermark <= maximumWatermark)
			{
				backlogWaiter.WaitUntilBacklogIsAcceptable((elapsed, nextWaitTime, success, failureReason, bytesBacklog) =>
				{
					Manager.ShowInfoMessage("\t      waiting for backlog to clear, " + (success ? bytesBacklog.BacklogDescription : failureReason) + ". Please run the LBK service task to reduce log fullness.");
				});

				if (sw.Elapsed >= populateTargetColumnsConfiguration.TimeConsumingThreshold)
				{
					LogRecordsRemaining();
					ExtProperty.Table.Update(Db.Connection, table.TargetTableSchema, table.TargetTableName, watermarkName, currentWatermark.ToString(CultureInfo.InvariantCulture));
					sw.Restart();
				}

				using (var transactionManager = Db.Connection.BeginTransactionWithManager())
				{
					var nextWatermark = currentWatermark + populateTargetColumnsConfiguration.TopRowCount;
					using (var cmd = Db.Connection.Command(sql))
					{
						cmd.AddParameter(id_FromParameterName, SqlDbType.Int, currentWatermark);
						cmd.AddParameter(id_ToParameterName, SqlDbType.Int, nextWatermark - 1);
						cmd.ExecuteNonQuery();
					}

					transactionManager.CommitTransaction();
					currentWatermark = nextWatermark;

#if DEBUG
					if (PopulateTargetColumnsBreak)
					{
						return;
					}
#endif
				}
			}

			Db.Connection.RunInTransaction(() =>
			{
				SetStatus(table, statusName, statusValue);
				ExtProperty.Table.Delete(Db.Connection, table.TargetTableSchema, table.TargetTableName, watermarkName);
			});

			void LogRecordsRemaining() => Manager.ShowInfoMessage(String.Format(CultureInfo.InvariantCulture, "\t    {0,11:N0} records remaining", maximumWatermark - currentWatermark + 1));
		}

		protected internal List<string> GetSystemLastEditColumns(PopulatedTable table, IEnumerable<string> updateStatements)
		{
			var result = new List<string>();
			var prefix = CargoWise.Schema.Schema.GetPrefixFromColumnName(table.TargetTablePKColumnName);

			var nonComparableType = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "image", "ntext", "text", "xml" };
			var ignoreColumnComparison = !updateStatements.Any() || table.ColumnList.Any(x => nonComparableType.Contains(x.ColumnType));

			var lastEditTimeColumn = (!string.IsNullOrEmpty(prefix) ? prefix + "_" : string.Empty) + "SystemLastEditTimeUtc";
			string columnChangeComparison = null;

			if (!table.ColumnList.Any(x => x.ColumnName == lastEditTimeColumn) && ColumnExists(table, lastEditTimeColumn))
			{
				var lastEditTimeValue = "GETUTCDATE()";
				if (!ignoreColumnComparison)
				{
					columnChangeComparison = GetColumnChangeComparison(updateStatements);
					lastEditTimeValue = GenerateAuditCheck(lastEditTimeColumn.QuoteName(), lastEditTimeValue, columnChangeComparison, useNullIf: false);
				}

				result.Add($"{lastEditTimeColumn.QuoteName()} = {lastEditTimeValue}");
			}

			var lastEditUserColumn = (!string.IsNullOrEmpty(prefix) ? prefix + "_" : string.Empty) + "SystemLastEditUser";
			if (!table.ColumnList.Any(x => x.ColumnName == lastEditUserColumn) && ColumnExists(table, lastEditUserColumn))
			{
				var lastEditUserValue = "'~BP'";
				if (!ignoreColumnComparison)
				{
					if (string.IsNullOrEmpty(columnChangeComparison))
					{
						columnChangeComparison = GetColumnChangeComparison(updateStatements);
					}
					lastEditUserValue = GenerateAuditCheck(lastEditUserColumn.QuoteName(), lastEditUserValue, columnChangeComparison, useNullIf: true);
				}

				result.Add($"{lastEditUserColumn.QuoteName()} = {lastEditUserValue}");
			}

			return result;
		}

		string GetColumnChangeComparison(IEnumerable<string> updateStatements)
		{
			var prefixLength = ColumnSynchroniser.ConvertCharToCharColumnPrefix.Length + 1;
			return string.Join(" AND ", updateStatements.Select(x => x.StartsWith($"[{ColumnSynchroniser.ConvertCharToCharColumnPrefix}") ? $"(target.[{x.Substring(prefixLength)})" : $"(target.{x})"));
		}

		string GenerateAuditCheck(string auditColumn, string newValue, string columnComparison, bool useNullIf)
		{
			var convertedAuditColum = useNullIf ? $"NULLIF({auditColumn}, '')" : $"{auditColumn}";

			var result = $"(CASE WHEN ({columnComparison}) THEN ISNULL({convertedAuditColum}, {newValue}) ELSE {newValue} END)";

			return result;
		}

		protected virtual string CreatePopulateTargetColumns(PopulatedTable table, string id_FromParameterName, string id_ToParameterName)
		{
			var customUpdateColumns = GetCustomUpdateColumns(table);
			var updateColumns = new List<string>(customUpdateColumns.Any()
				? customUpdateColumns
				: table.ColumnList.Select(c =>
					(String.IsNullOrWhiteSpace(c.PopulateSource))
							? String.Format(CultureInfo.InvariantCulture, "{0} = {1}", c.ColumnName.QuoteName(), c.PopulateExpression)
							: (String.IsNullOrWhiteSpace(c.PopulateExpression))
								? String.Format(CultureInfo.InvariantCulture, "{0} = {1}", c.ColumnName.QuoteName(), c.PopulateExpression)
								: c.ColumnType == "uniqueidentifier"
									? c.ColumnNullable == "NOT NULL"
										? String.Format(CultureInfo.InvariantCulture, "{0} = tmp.{0}", c.ColumnName.QuoteName())
										: String.Format(CultureInfo.InvariantCulture, "{0} = COALESCE(target.{0}, tmp.{0})", c.ColumnName.QuoteName())
									: String.Format(CultureInfo.InvariantCulture, "{0} = COALESCE(NULLIF(target.{0}, ''), tmp.{0})", c.ColumnName.QuoteName())));

			var whereClauseList = table.ColumnList.Where(c => !String.IsNullOrWhiteSpace(c.PopulateTargetWhereClause)).Select(c => c.PopulateTargetWhereClause).ToList();
			var whereClause = (whereClauseList.Count == 0) ? String.Empty : String.Format(CultureInfo.InvariantCulture, "\r\nWHERE 1=2\r\n\t{0}", String.Join("\r\n\t", whereClauseList));

			updateColumns.AddRange(GetSystemLastEditColumns(table, updateColumns));

			var sql = String.Format(CultureInfo.InvariantCulture, @"-- Populating target table
SET NOCOUNT ON;

UPDATE TOP({0}) target WITH (ROWLOCK) SET
	{1}
{8}
FROM
	{2} AS target WITH (READPAST, READCOMMITTEDLOCK)
	JOIN {3} AS tmp ON tmp.PK = target.[{4}] AND Id BETWEEN {6} AND {7}{5}
OPTION (MAXDOP 1)
;
"
				, populateTargetColumnsConfiguration.TopRowCount // 0
				, String.Join(",\r\n\t", updateColumns)          // 1
				, table.TargetTableFullName                      // 2
				, table.PreAddTableFullName                      // 3
				, table.TargetTablePKColumnName                  // 4
				, whereClause                                    // 5
				, id_FromParameterName                           // 6
				, id_ToParameterName                             // 7
				, OutputClause // 8
				);
			return sql;
		}

		protected virtual string OutputClause => string.Empty;

		void DropPopulatingWatermark(PopulatedTable table, string watermarkName)
		{
			if (table.PopulatingWatermark > 0)
			{
				ExtProperty.Table.Delete(Db.Connection, table.TargetTableSchema, table.TargetTableName, watermarkName);
				table.PopulatingWatermark = 0;
			}
		}

		#endregion //Target table

		#region Online upgrade status

		void SetStatus(PopulatedTable table, string statusName, TargetTableStatus statusValue)
		{
			if (table.TargetTableStatus.Value != statusValue)
			{
				foreach (var col in table.ColumnList)
				{
					ExtProperty.Column.Update(Db.Connection, table.TargetTableSchema, table.TargetTableName, col.ColumnName, statusName, statusValue.ToString());
				}

				table.TargetTableStatus = statusValue;
			}
		}

		#endregion // Online upgrade status
		int GetMaximumWatermark(string preAddDb, string preAddSchema, string preAddTable)
		{
			string sql = String.Format(CultureInfo.InvariantCulture, @"-- Get count of the remaining columns
SELECT ISNULL(MAX(ID), 0)
FROM
	[{0}].[{1}].[{2}];
"
				, preAddDb // 0
				, preAddSchema // 1
				, preAddTable // 2
				);

			StoreQueryForTest(sql);

			return Convert.ToInt32(Db.Connection.ExecuteScalar(sql), CultureInfo.InvariantCulture);
		}

		long GetTableSize_MB(PopulatedTable table)
		{
			var sql = String.Format(CultureInfo.InvariantCulture, @"-- Get size of a table in MB
SELECT
	table_size = SUM(used_page_count * 8) / 1024 -- MB
FROM
	[{0}].sys.dm_db_partition_stats
WHERE
	object_id = OBJECT_ID(N'{1}', N'U')
;"
				, targetDb                  // 0
				, table.TargetTableFullName // 1
				);

			StoreQueryForTest(sql);
			return (long)Db.Connection.ExecuteScalar(sql);
		}

		public static void CleanupAllResources(DbConnection connection)
		{
			if (!connection.IsInTransaction)
			{
				throw new InvalidOperationException("A connection with a valid transaction context must be passed");
			}

			if (DbObjectCreator.DatabaseExists(connection, preAddDb))
			{
				var sql = String.Format(CultureInfo.InvariantCulture, @"
SELECT
	'DROP TABLE [{0}]..[' + tab.name + '];'
FROM
	[{0}].sys.tables tab
WHERE
	tab.is_ms_shipped = 0
;"
					, preAddDb // 0
					);

				var cmdRunner = new BatchRunner();
				cmdRunner.RunCommandsGeneratedByQuery(connection, sql);

				sql = String.Format(CultureInfo.InvariantCulture, @"
DELETE
	dbo.StmExtendedProperty
WHERE 1=1
	AND SEP_Class              = 'Table'
	AND SEP_DatabaseNameSuffix = ''
	AND SEP_Name               = '{0}'
OPTION (RECOMPILE);

DELETE
	dbo.StmExtendedProperty
WHERE 1=1
	AND SEP_Class              = 'Column'
	AND SEP_DatabaseNameSuffix = ''
	AND SEP_Name               = '{1}'
OPTION (RECOMPILE);
"
					, OnlineUpgradePopulatingWatermarkName // 0
					, OnlineUpgradePopulatingStatusName    // 1
					);

				connection.ExecuteNonQuery(sql);
			}
		}

		partial void StoreQueryForTest(string query);
		partial void RunUserAction_ForTest(string tableFullName);

		public const string GetRefreshScriptFormat = @"-- TablePreSynchroniser.RefreshDependentScripts(Get)
WITH
	cte AS (
			SELECT
				obj_level = 1,
				obj_id    = d.referencing_id
			FROM
				sys.sql_expression_dependencies AS d
				JOIN sys.objects                AS o ON o.object_id = d.referencing_id
			WHERE 1=1
				AND d.is_schema_bound_reference = 0
				AND d.referenced_id = OBJECT_ID(@TableName, N'U')
				AND d.referenced_minor_id = 0
				AND o.type in ('P', 'V', 'TR', 'FN', 'IF', 'TF')
				AND o.name NOT LIKE 'Client%'
				AND o.name NOT LIKE 'RptDt%'

			UNION ALL

			SELECT
				obj_level = cte.obj_level + 1,
				obj_id    = d.referencing_id
			FROM
				cte
				JOIN sys.sql_expression_dependencies AS d ON d.referenced_id = cte.obj_id
				JOIN sys.objects                     AS o ON o.object_id = d.referencing_id
			WHERE 1=1
				AND d.is_schema_bound_reference = 0
				AND d.referenced_minor_id = 0
				AND o.type in ('P', 'V', 'TR', 'FN', 'IF', 'TF')
				AND o.name NOT LIKE 'Client%'
				AND o.name NOT LIKE 'RptDt%'
		)
	, cte_distinct AS (
			SELECT
				obj_id
				, obj_level = MAX(obj_level)
			FROM
				cte
			GROUP BY
				obj_id
		)
SELECT
	SchemaName = OBJECT_SCHEMA_NAME(obj_id),
	ObjectName = OBJECT_NAME(obj_id)
FROM
	cte_distinct
ORDER BY
	obj_level;
";

		/// <summary>
		/// Calls SQL sproc sys.sp_refreshsqlmodule for modules that depend on the given table.
		/// Will drill down the dependency hiearchy.
		/// For example, a function that depends on a view that depends on the table will get refreshed.
		/// Will refresh modules in the same database and in the UserRepository database if it exists.
		/// 
		/// When called in production, the current database is always the main CW database.
		/// </summary>
		/// <param name="connection">Connection to use. Current database must be table database.</param>
		/// <param name="tableDb">database name containing the table</param>
		/// <param name="tableSchemaName">table schema name</param>
		/// <param name="tableName">table name</param>
		/// <param name="manager">for logging</param>
		public static void RefreshDependentScripts(DbConnection connection, string tableDb, string tableSchemaName, string tableName, IUpgradeManager manager)
		{
			var allObjects = RefreshDependentScriptsInSameDb(connection, tableSchemaName, tableName, manager);
			RefreshDependentScriptsInUserRepository(connection, tableDb, allObjects, manager);
		}

		/// <summary>
		/// Refresh and return a list with the given object and all its dependent objects in the same db.
		/// </summary>
		internal static List<(string SchemaName, string ObjectName)> RefreshDependentScriptsInSameDb(DbConnection connection, string tableSchemaName, string tableName, IUpgradeTaskWorkflowLogger manager)
		{
			var targetTableFullName = tableSchemaName.QuoteName() + '.' + tableName.QuoteName();

			var skippedCount = 0;
			manager.ShowInfoMessage("    : Refreshing dependent scripts of " + targetTableFullName);
			var allObjects = new List<(string SchemaName, string ObjectName)>();
			allObjects.Add((tableSchemaName, tableName));
			var commandsToRun = DataUtils.GetDataTableFromQuery(
				connection,
				GetRefreshScriptFormat,
				("@TableName", SqlDbType.NVarChar, 4000, (object)targetTableFullName));
			foreach (DataRow row in commandsToRun.Rows)
			{
				try
				{
					connection.ExecuteNonQuery(
						@"-- TablePreSynchroniser.RefreshDependentScripts(Execute)
EXEC sys.sp_refreshsqlmodule @FullName;
",
						cmd =>
						{
							var schemaName = (string)row[0];
							var objectName = (string)row[1];
							allObjects.Add((schemaName, objectName));
							var fullName = schemaName.QuoteName() + "." + objectName.QuoteName();
							cmd.AddParameter("@FullName", SqlDbType.NVarChar, 257, fullName);
						});
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					skippedCount++;
				}
			}

			if (skippedCount > 0)
			{
				manager.ShowInfoMessage(String.Format(CultureInfo.InvariantCulture, "        Skipped {0} objects", skippedCount));
			}

			return allObjects;
		}

		static void RefreshDependentScriptsInUserRepository(DbConnection connection, string tableDb, List<(string SchemaName, string ObjectName)> allObjects, IUpgradeTaskWorkflowLogger logger)
		{
			try
			{
				var userRepositoryDbName = tableDb + DbUserRepository.RepositoryDbSuffix;
				if (connection.DatabaseExists(userRepositoryDbName))
				{
					RefreshDependentScriptsInAnotherDbOnSameServer(connection, userRepositoryDbName, tableDb, allObjects, logger);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				logger.ShowTaskError(ex.ToString());
			}
		}

		static void RefreshDependentScriptsInAnotherDbOnSameServer(
			DbConnection connection,
			string otherDbName,
			string tableDb,
			List<(string SchemaName, string ObjectName)> allObjects,
			IUpgradeTaskWorkflowLogger logger)
		{
			var skippedCount = 0;
			logger.ShowInfoMessage("    : Refreshing dependent scripts in " + otherDbName);
			var sql = string.Format(GetOtherDbRefreshScriptFormat, otherDbName);
			var dependentFullNames = new HashSet<string>();
			foreach (var mainObject in allObjects)
			{
				var commandsToRun = DataUtils.GetDataTableFromQuery(
					connection,
					sql,
					("@DbName", SqlDbType.NVarChar, 256, (object)tableDb),
					("@SchemaName", SqlDbType.NVarChar, 256, (object)mainObject.SchemaName),
					("@ObjectName", SqlDbType.NVarChar, 256, (object)mainObject.ObjectName));
				foreach (DataRow row in commandsToRun.Rows)
				{
					var fullName = (string)row[0];
					dependentFullNames.Add(fullName);
				}
			}

			foreach (var fullName in dependentFullNames)
			{
				try
				{
					connection.ExecuteNonQuery(
						$@"-- TablePreSynchroniser.RefreshOtherDbDependentScripts(Execute)
EXEC {otherDbName.QuoteName()}.sys.sp_refreshsqlmodule @FullName;
",
						cmd =>
						{
							cmd.AddParameter("@FullName", SqlDbType.NVarChar, 257, fullName);
						});
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					skippedCount++;
				}
			}

			if (skippedCount > 0)
			{
				logger.ShowInfoMessage(String.Format(CultureInfo.InvariantCulture, "        Skipped {0} objects", skippedCount));
			}
		}

		internal const string GetOtherDbRefreshScriptFormat = @"-- TablePreSynchroniser.RefreshOtherDbDependentScripts(Get)
WITH
	objectSynonyms AS (
		select synonymId = object_id from {0}.sys.synonyms where base_object_name = QUOTENAME(@DbName) + '.' + QUOTENAME(@SchemaName) + '.' + QUOTENAME(@ObjectName)
	),
	cte AS (
			SELECT
				obj_level = 1,
				obj_id    = d.referencing_id
			FROM
				{0}.sys.sql_expression_dependencies AS d
				JOIN {0}.sys.objects                AS o ON o.object_id = d.referencing_id
			WHERE 1=1
				AND d.referenced_database_name = @DbName
				AND d.referenced_schema_name = @SchemaName
				AND d.referenced_entity_name = @ObjectName
				AND d.is_schema_bound_reference = 0
				AND d.referenced_minor_id = 0
				AND o.type in ('P', 'V', 'TR', 'FN', 'IF', 'TF')

			UNION ALL

			SELECT
				obj_level = 1,
				obj_id    = d.referencing_id
			FROM
				{0}.sys.sql_expression_dependencies AS d
				JOIN {0}.sys.objects                AS o ON o.object_id = d.referencing_id
			WHERE 1=1
				AND d.referenced_database_name is null
				AND d.referenced_id in (select synonymId from objectSynonyms)
				AND d.is_schema_bound_reference = 0
				AND d.referenced_minor_id = 0
				AND o.type in ('P', 'V', 'TR', 'FN', 'IF', 'TF')

			UNION ALL

			SELECT
				obj_level = cte.obj_level + 1,
				obj_id    = d.referencing_id
			FROM
				cte
				JOIN {0}.sys.sql_expression_dependencies AS d ON d.referenced_id = cte.obj_id
				JOIN {0}.sys.objects                     AS o ON o.object_id = d.referencing_id
			WHERE 1=1
				AND d.is_schema_bound_reference = 0
				AND d.referenced_minor_id = 0
				AND o.type in ('P', 'V', 'TR', 'FN', 'IF', 'TF')
		)
	, cte_distinct AS (
			SELECT
				obj_id
				, obj_level = MAX(obj_level)
			FROM
				cte
			GROUP BY
				obj_id
		)
SELECT
	FullName = QUOTENAME(s.name) + '.' + QUOTENAME(o.name)
FROM
	cte_distinct
JOIN {0}.sys.objects AS o ON o.object_id = obj_id
JOIN {0}.sys.schemas AS s ON o.schema_id = s.schema_id
ORDER BY
	obj_level;
";
	}
}

#region Test
#if DEBUG

#region partial class

namespace Enterprise.DbUpgrader.Schema.OnlineUpgrade
{
	public partial class TablePreSynchroniser
	{
		public static string PreAddDb_Exposed => preAddDb;

		public TablePreSynchroniser(IUpgradeManager manager, string dbBeingUpgraded, string templateDb, ColumnCreatorWithValueFactory columnCreatorFactory)
			: this(manager, dbBeingUpgraded, templateDb)
		{
			this.newColumnCreatorFactory = columnCreatorFactory;
		}

		public bool PopulateTargetColumnsBreak;

		public static void CreatePreAddDb_ForTest()
		{
			TablePreSynchroniser.CreatePreAddDb(dataInitialSizeMb: 5, dataGrowthMb: 5, logInitialSizeMb: 5, logGrowthMb: 5);
		}

		public static void DropPreAddDb_ForTest()
		{
			using (AdminConnection adminConnection = Db.NewAdminConnection())
			{
				var sql = String.Format(CultureInfo.InvariantCulture, @"-- TablePreSynchroniser: DropPreAddDb
if (DB_ID(N'{0}') is NOT NULL) DROP DATABASE [{0}];
"
					, preAddDb
					);

				adminConnection.ExecuteNonQuery(sql);
			}
		}

		public int TopRowCount_ForTest
		{
			get { return populateTargetColumnsConfiguration.TopRowCount; }
			set { populateTargetColumnsConfiguration.TopRowCount = value; }
		}

		public bool StoreQueryToManager { get; set; }

		partial void StoreQueryForTest(string query)
		{
			if (StoreQueryToManager)
			{
				Manager.ShowInfoMessage(query);
			}
		}

		public List<PopulatedTable> GetCharToCharColumnsToPopulate() => GetColumnsToPopulate(GetAllColumnsToConvertCharToChar(), CreateConvertedColumnsCharToChar);

		public void DoPopulateForTest(List<PopulatedTable> tables, int totalTableCount, long totalTableSize, string headerMessage, string infoMark, string statusName, string watermarkName) => DoPopulate(tables, totalTableCount, totalTableSize, headerMessage, infoMark, statusName, watermarkName);

		public TimeSpan TimeConsumingThresholdForTest
		{
			get { return populateTargetColumnsConfiguration.TimeConsumingThreshold; }
			set { populateTargetColumnsConfiguration.TimeConsumingThreshold = value; }
		}

		public static string CharToCharStatusNameForTest => OnlineUpgradeConvertCharToCharStatusName;
		public static string CharToCharWatermarkNameForTest => OnlineUpgradeConvertCharToCharWatermarkName;
		public Overridable<Action<string>> UserAction_ForTest = new Overridable<Action<string>>(null);
		partial void RunUserAction_ForTest(string tableFullName)
		{
			UserAction_ForTest.Value?.Invoke(tableFullName);
		}
	}
}

#endregion // partial class

#endif
#endregion //Test
