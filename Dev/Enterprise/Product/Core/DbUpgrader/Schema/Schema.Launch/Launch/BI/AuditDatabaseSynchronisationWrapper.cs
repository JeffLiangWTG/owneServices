namespace Enterprise.DbUpgrader.Schema
{
	using System;
	using System.Data;
	using System.Globalization;
	using System.Linq;
	using CargoWise.Bi.Common;
	using CargoWise.Data;
	using CargoWise.Types;
	using Enterprise.DbUpgrader.Schema.Template;
	using Enterprise.DbUpgrader.Shared;

	class AuditDatabaseSynchronisationWrapper : BusinessIntelligenceSynchronisationWrapper
	{
		public static AuditDatabaseSynchronisationWrapper New(IUpgradeManager manager, DbConnection upgConnection)
		{
			return new AuditDatabaseSynchronisationWrapper(manager, Db.AuditDatabaseName, upgConnection);
		}

		protected AuditDatabaseSynchronisationWrapper(IUpgradeManager manager, string dbToUpgrade, DbConnection upgConnection)
			: base(manager, dbToUpgrade, upgConnection)
		{
		}

		protected override IAuxiliaryDbCreator GetTemplateDbCreator()
		{
			return new AuditDbTemplate(Manager, TemplateDb, UpgConnection.ServerName);
		}

		protected override void RunSynchronisationActions()
		{
			RenameOldColumns();
			base.RunSynchronisationActions();
			PopulatesBiSystemTables();
		}

		#region Rename Old Audit Columns

		/// <summary>
		/// Renames columns removed from the latest Audit schema so they can be preserved.
		/// </summary>
		internal void RenameOldColumns()
		{
			Manager.StartNonEstimatedTask("Rename old audit columns");

			using (((ICurrentDbControl)UpgConnection).UseDatabase(DbBeingUpgraded))
			{
				var columnsToRename = DataUtils.GetDataTableFromQuery(UpgConnection, ColumnsToRenameSql);
				var columnsToRecreate = DataUtils.GetDataTableFromQuery(UpgConnection, ColumnsToRecreateSql);

				if (columnsToRename.Rows.Count > 0)
				{
					InitializeSchemaMappingValues();
					DateTime utcTimeStamp = Convert.ToDateTime(UpgConnection.ExecuteScalar("SELECT GETUTCDATE()"), CultureInfo.InvariantCulture);
					string renamedColumnPrefix =
						ColumnChangeRetriever.WtgPreservedColumnPrefix
						+ RenamedColumnDeletedFlag
						+ utcTimeStamp.ToString(RenamedColumnTimeStampFormat, CultureInfo.InvariantCulture)
						+ "!";

					foreach (DataRow row in columnsToRename.Rows)
					{
						var columnName = row["ColName"].ToString().Trim();
						var columnId = Convert.ToString(UpgConnection.ExecuteScalar("SELECT ISNULL((SELECT TOP 1 column_id FROM sys.columns WHERE name ='" + columnName + "'), 0 )"), CultureInfo.InvariantCulture);
						var renamedColumnPrefixFull = renamedColumnPrefix + columnId + "!";
						RenameColumn(row, renamedColumnPrefixFull);

						var columnToRecreateRow = columnsToRecreate.Rows.OfType<DataRow>().FirstOrDefault(r =>
							r["TabSchema"].ToString().Trim() == row["TabSchema"].ToString().Trim() &&
							r["TabName"].ToString().Trim() == row["TabName"].ToString().Trim() &&
							r["ColName"].ToString().Trim() == row["ColName"].ToString().Trim());
						if (columnToRecreateRow != null)
						{
							AddNewColumns(columnToRecreateRow);
						}
					}
				}
			}
		}

		byte[] currentMaxLsn;
		ZDateTime? currentMaxLsnTimeUtc;
		string effectiveSchemaVersion = string.Empty;

		void InitializeSchemaMappingValues()
		{
			using (var cmd = UpgConnection.Command("select top 1 StartLsn, TranEndTimeUtc from biadmin.LsnTimeMapping order by StartLsn desc"))
			using (var reader = cmd.ExecuteReader())
			{
				if (reader.Read())
				{
					var startLsnObj = reader["StartLsn"];
					if (startLsnObj != DBNull.Value)
					{
						currentMaxLsn = (byte[])startLsnObj;
					}
					var tranEndTimeObj = reader["TranEndTimeUtc"];
					if (tranEndTimeObj != DBNull.Value)
					{
						currentMaxLsnTimeUtc = new ZDateTime(tranEndTimeObj);
					}
				}
			}
			effectiveSchemaVersion = DataUtils.LoadDbExtendedProperty(UpgConnection, BiConstants.MainDbSchemaVersionExtPtyName, DbBeingUpgraded);
			if (effectiveSchemaVersion == null)
			{
				effectiveSchemaVersion = string.Empty;
			}
		}

		void RenameColumn(DataRow columnToRenameInfoRow, string renamedColumnPrefix)
		{
			string tableSchema = columnToRenameInfoRow["TabSchema"].ToString().Trim();
			string tableName = columnToRenameInfoRow["TabName"].ToString().Trim();
			string oldColumnName = columnToRenameInfoRow["ColName"].ToString().Trim();
			string newColumnName = renamedColumnPrefix + oldColumnName;

			Manager.StartSubtask(
				String.Format(CultureInfo.InvariantCulture,
					"(~) {0}.{1}.{2} -> [{3}]",
					/*0*/tableSchema,
					/*1*/tableName,
					/*2*/oldColumnName,
					/*3*/newColumnName)
			);

			var dependencyRemover = new DbColumnDependencyRemover(tableSchema, tableName, oldColumnName);
			dependencyRemover.DropRelateObjectsBeforeRenamingColumn(UpgConnection, newColumnName);

			DbObjectCreator.RenameColumn(UpgConnection, tableSchema, tableName, oldColumnName, newColumnName);
			SaveMappingToAuditDatabase(tableSchema, tableName, oldColumnName, newColumnName);
		}

		public virtual void AddNewColumns(DataRow columnToRecreateInfoRow)
		{
			var columnMetaData = new ColumnChangeMetadata(columnToRecreateInfoRow);

			Manager.StartSubtask(
				String.Format(CultureInfo.InvariantCulture,
					"(+) {0}.{1}.{2}",
					/*0*/columnMetaData.TableSchema,
					/*1*/columnMetaData.TableName,
					/*2*/columnMetaData.ColumnName)
			);

			UpgConnection.ExecuteNonQuery($"ALTER TABLE [{DbBeingUpgraded}].[{columnMetaData.TableSchema}].[{columnMetaData.TableName}] ADD {columnMetaData.FullAddColumnDeclaration}");
		}

		void SaveMappingToAuditDatabase(string tableSchema, string tableName, string oldColumnName, string newColumnName)
		{
			if (currentMaxLsn != null)
			{
				try
				{
					var sqlText = string.Format(CultureInfo.InvariantCulture,
						@"
IF NOT EXISTS (SELECT NULL FROM [{0}].[SchemaMappingSummary] WHERE TableName = @TableName AND MappedColumn = @MappedColumn AND MaxLsn = @MaxLsn)
BEGIN
	INSERT INTO [{0}].[SchemaMappingSummary] (MaxLsn, MaxLsnTimeUTC, EffectiveSchemaVersion, TableName, RenamedColumn, MappedColumn)
	VALUES (@MaxLsn, @MaxLsnTimeUTC, @EffectiveSchemaVersion, @TableName, @RenamedColumn, @MappedColumn)
END

UPDATE [{0}].SchemaMappingHistory
SET MappedName = @RenamedColumn
WHERE ColumnName = @MappedColumn AND MappedName IS NULL",
						BiConstants.BiAdminSchemaName);

					using (var cmd = UpgConnection.Command(sqlText))
					{
						cmd.AddParameter("@MaxLsn", SqlDbType.Binary, currentMaxLsn);
						cmd.AddParameter("@MaxLsnTimeUTC", SqlDbType.DateTime, currentMaxLsnTimeUtc);
						cmd.AddParameter("@EffectiveSchemaVersion", SqlDbType.VarChar, effectiveSchemaVersion);
						cmd.AddParameter("@TableName", SqlDbType.VarChar, $"{tableSchema}.{tableName}");
						cmd.AddParameter("@RenamedColumn", SqlDbType.VarChar, newColumnName);
						cmd.AddParameter("@MappedColumn", SqlDbType.VarChar, oldColumnName);

						cmd.ExecuteNonQuery();
					}
				}
				catch (SqlException ex)
				{
					var dbErrorType = new DbErrorMatch(ex).ExceptionType;
					if (dbErrorType != DbErrorType.InvalidObjectName && dbErrorType != DbErrorType.InvalidColumnName)
					{
						throw;
					}
				}
			}
		}

		/// <summary>
		/// Select audit columns removed from latest schema which should be renamed and preserved.
		/// -- Exclude microsoft-shipped tables (table is_ms_shipped = 0)
		/// -- Only columns no longer in the database schema (NewCol name is null)
		/// -- Exclude columns in the BI admin schema (schema name != BiAdminSchemaName)
		/// -- Exclude CDC system columns (column name !starts with CdcSystemColumnPrefix)
		/// -- Exclude CW preserved columns (column name !starts with CargoWisePreservedColumnPrefix)
		/// -- Only nullable columns (column is_nullable = 1)
		/// </summary>
		string ColumnsToRenameSql
		{
			get
			{
				return String.Format(CultureInfo.InvariantCulture, @"
					SELECT
						TabSchema = CurSch.name,
						TabName = CurTab.name,
						ColName = CurCol.name
					FROM
						[{0}].sys.tables CurTab
						INNER JOIN [{0}].sys.schemas CurSch ON CurSch.schema_id = CurTab.schema_id
						INNER JOIN [{0}].sys.columns CurCol ON CurCol.object_id = CurTab.object_id
						INNER JOIN [{0}].sys.types CurColDataType ON CurCol.user_type_id = CurColDataType.user_type_id
						INNER JOIN [{1}].sys.tables NewTab ON NewTab.name = CurTab.name
						INNER JOIN [{1}].sys.schemas NewSch ON NewSch.schema_id = NewTab.schema_id AND NewSch.name = CurSch.name
						LEFT JOIN [{1}].sys.columns NewCol ON NewCol.object_id = NewTab.object_id AND NewCol.name = CurCol.name
						LEFT JOIN [{1}].sys.types NewColDataType ON NewCol.user_type_id = NewColDataType.user_type_id
					WHERE
						CurTab.is_ms_shipped = 0
						AND NewTab.is_ms_shipped = 0
						AND (NewCol.name IS NULL
							OR NewColDataType.name <> CurColDataType.name
							OR NewCol.max_length <> CurCol.max_length
							OR NewCol.precision <> CurCol.precision
							OR NewCol.scale <> CurCol.scale)
						AND CurSch.name <> '{2}'
						AND CurCol.name not like '{3}%'
						AND CurCol.name not like '{4}%'
						AND CurCol.is_nullable = 1
					ORDER BY
						CurSch.name,
						CurTab.name,
						CurCol.name",
					/*0*/DbBeingUpgraded,
					/*1*/TemplateDb,
					/*2*/BiConstants.BiAdminSchemaName,
					/*3*/DataUtils.ReplaceSqlLikeWildcard(CdcSystemColumnPrefix),
					/*4*/ColumnChangeRetriever.WtgPreservedColumnPrefix
				);
			}
		}

		string ColumnsToRecreateSql
		{
			get
			{
				return String.Format(CultureInfo.InvariantCulture, @"
					SELECT
						TabSchema = NewSch.name,
						TabName = NewTab.name,
						ColName = NewCol.name,
						ColType = NewTyp.name,
						ColLength = IIF(NewCol.max_length > 0 AND NewTyp.name in (N'nchar', N'nvarchar'), NewCol.max_length / 2, NewCol.max_length),
						ColNullOrNotNull = IIF(NewCol.is_nullable = 1, 'NULL', 'NOT NULL'),
						ColSparse = NewCol.is_sparse,
						ColPrecision = NewCol.precision,
						ColScale = NewCol.scale,
						ColDefault = NewDefault.definition,
						ColXmlSchema = NewXmlSchema.name,
						IdentitySeed = NewIdCol.seed_value,
						IdentityIncrement = NewIdCol.increment_value,
						IsComputedColumnChange = NewCol.is_computed,
						ComputedColumnDefinition = NewCptCol.definition,
						IsPersistedComputedColumn = NewCptCol.is_persisted
					FROM
						[{1}].sys.schemas AS NewSch
						JOIN [{1}].sys.tables AS NewTab ON NewTab.schema_id = NewSch.schema_id
						JOIN [{1}].sys.columns AS NewCol ON NewCol.object_id = NewTab.object_id
						JOIN [{1}].sys.types AS NewTyp ON NewTyp.user_type_id = NewCol.user_type_id
						LEFT JOIN [{1}].sys.default_constraints AS NewDefault ON NewDefault.object_id = NewCol.default_object_id
						LEFT JOIN [{1}].sys.identity_columns AS NewIdCol ON NewIdCol.object_id = NewCol.object_id AND NewIdCol.column_id = NewCol.column_id
						LEFT JOIN [{1}].sys.computed_columns AS NewCptCol ON NewCptCol.object_id = NewCol.object_id AND NewCptCol.column_id = NewCol.column_id
						LEFT JOIN [{1}].sys.xml_schema_collections AS NewXmlSchema ON NewXmlSchema.xml_collection_id = NewCol.xml_collection_id AND NewCol.xml_collection_id <> 0

						JOIN [{0}].sys.schemas AS CurSch ON 1=1 AND CurSch.name = NewSch.name
						JOIN [{0}].sys.tables AS CurTab ON CurTab.schema_id = CurSch.schema_id AND CurTab.name = NewTab.name

						LEFT JOIN [{0}].sys.columns AS CurCol ON CurCol.object_id = CurTab.object_id AND CurCol.name = NewCol.name
						LEFT JOIN [{0}].sys.types CurColDataType ON CurCol.user_type_id = CurColDataType.user_type_id
					WHERE
						CurTab.is_ms_shipped = 0
						AND NewTab.is_ms_shipped = 0
						AND (NewTyp.name <> CurColDataType.name
							OR NewCol.max_length <> CurCol.max_length
							OR NewCol.precision <> CurCol.precision
							OR NewCol.scale <> CurCol.scale)
						AND CurSch.name <> '{2}'
						AND CurCol.name not like '{3}%'
						AND CurCol.name not like '{4}%'
						AND CurCol.is_nullable = 1
					ORDER BY
						CurSch.name,
						CurTab.name,
						CurCol.name",
					/*0*/DbBeingUpgraded,
					/*1*/TemplateDb,
					/*2*/BiConstants.BiAdminSchemaName,
					/*3*/DataUtils.ReplaceSqlLikeWildcard(CdcSystemColumnPrefix),
					/*4*/ColumnChangeRetriever.WtgPreservedColumnPrefix
				);
			}
		}

		internal const string CdcSystemColumnPrefix = "__$";
		internal const string RenamedColumnDeletedFlag = "D-";
		internal const string RenamedColumnTimeStampFormat = "yyyyMMdd-HHmm";

		#endregion

		void PopulatesBiSystemTables()
		{
			Manager.ShowInfoMessage("Populating Audit control tables");
			new AuditDatabasePopulator(UpgConnection).Run();
		}
	}
}
