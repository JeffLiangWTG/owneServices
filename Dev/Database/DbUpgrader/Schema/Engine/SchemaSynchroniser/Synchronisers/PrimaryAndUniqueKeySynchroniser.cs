using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.DbUpgrader.Foundation;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.Schema
{
	class PrimaryAndUniqueKeySynchroniser : MetadataScriptRunner
	{
		public PrimaryAndUniqueKeySynchroniser(DbConnection upgConnection, string dbBeingUpgraded, string templateDb, TableScriptRunner tableMetadataRunner, IUpgradeTaskWorkflowLogger taskLogger)
			: base(upgConnection, dbBeingUpgraded, templateDb, taskLogger)
		{
			this.tableMetadataRunner = tableMetadataRunner;
		}

		readonly TableScriptRunner tableMetadataRunner;

		/// <summary>
		/// Drop, Create or Modify(Recreate) Primary Key or Unique Constraints (Dropping Ref FKs and XML indexes)
		/// Note: Constraints of tables that are not in the new schema (client specific tables) are IGNORED
		/// </summary>
		public void SynchronisePrimaryAndUniqueConstraints()
		{
			taskLogger.ShowInfoMessage("\tDrop old constraints");
			DropOldOrModifiedPksAndUqs();

			taskLogger.ShowInfoMessage("\tCreate new constraints");
			CeateNewPksAndUqs();
		}

		#region Drop Old Primary Key and Unique constraints

		/// <summary>
		///	--------------------------------------------------------------------------------------------------------
		///	-- DROP PrimaryKey AND Unique CONSTRAINTS THAT HAVE BEEN REMOVED OR MODIFIED IN THE TEMPLATE SCHEMA
		///	--
		///	-- Note 1: Assumes that all constraints in the template are single-key ones.
		///	--         I.E. are composed by only one column (this is enforced by unit tests).
		///	--
		/// -- Note 2: PKs and UQs of tables that are not in the new schema are IGNORED.
		///	--------------------------------------------------------------------------------------------------------
		/// </summary>
		void DropOldOrModifiedPksAndUqs()
		{
			string constraintsToDropScript = GetScriptReplacingDbNames(SelectOldPksAndUqsRaw);
			var constraintsToDrop = DataUtils.GetDataTableFromQuery(upgConnection, constraintsToDropScript);

			foreach (DataRow row in constraintsToDrop.Rows)
			{
				string schemaName = row["SchemaName"].ToString().Trim();
				string tableName = row["TableName"].ToString().Trim();
				string constraintName = row["ConstraintName"].ToString().Trim();

				taskLogger.ShowInfoMessage("\t  (-) " + schemaName + "." + tableName + "." + constraintName);

				tableMetadataRunner.DropReferencingFKs(schemaName, tableName);
				tableMetadataRunner.DropPrimaryXmlIndexes(schemaName, tableName);
				tableMetadataRunner.DropSpatialIndexes(schemaName, tableName);
				DbColumnDependencyRemover.DisableTableChangeTracking(upgConnection, schemaName, tableName);

				string dropScript = "ALTER TABLE [" + schemaName + "].[" + tableName + "] DROP CONSTRAINT [" + constraintName + "];";
				upgConnection.ExecuteNonQuery(dropScript);
			}
		}

		const string SelectOldPksAndUqsRaw = @"
			SELECT DISTINCT
				CurConstraint.SchemaName SchemaName,
				CurConstraint.TableName TableName,
				CurConstraint.IndexName ConstraintName

			FROM

				(SELECT
					sch.name SchemaName,
					tab.name TableName, 
					kc.name IndexName,
					kc.type ConstraintType,
					ind.type IndexType,
					col.name ColumnName
				FROM
					[{0}].sys.key_constraints kc
					INNER JOIN [{0}].sys.tables tab
						ON tab.object_id = kc.parent_object_id
					INNER JOIN [{0}].sys.schemas sch
						ON sch.schema_id = tab.schema_id
					INNER JOIN [{0}].sys.indexes ind
						ON ind.object_id = kc.parent_object_id and ind.index_id = kc.unique_index_id
					INNER JOIN [{0}].sys.index_columns ikey
						ON ikey.object_id = kc.parent_object_id AND ikey.index_id = kc.unique_index_id
					INNER JOIN [{0}].sys.columns col
						ON col.object_id = ikey.object_id AND col.column_id = ikey.column_id
				WHERE
					tab.is_ms_shipped = 0
					AND kc.is_ms_shipped = 0
					-- Ignore TABLES that are not in the template schema (client tables)
					AND EXISTS (
						SELECT newtab.name
						FROM [{1}].sys.tables newtab
						INNER JOIN [{1}].sys.schemas newsch ON newsch.schema_id = newtab.schema_id
						WHERE newsch.name = sch.name
						AND newtab.name = tab.name)
				) CurConstraint

				LEFT JOIN

				(SELECT
					sch.name SchemaName,
					tab.name TableName, 
					kc.name IndexName,
					kc.type ConstraintType,
					ind.type IndexType,
					col.name ColumnName
				FROM
					[{1}].sys.key_constraints kc
					INNER JOIN [{1}].sys.tables tab
						ON tab.object_id = kc.parent_object_id
					INNER JOIN [{1}].sys.schemas sch
						ON sch.schema_id = tab.schema_id
					INNER JOIN [{1}].sys.indexes ind
						ON ind.object_id = kc.parent_object_id and ind.index_id = kc.unique_index_id
					INNER JOIN [{1}].sys.index_columns ikey
						ON ikey.object_id = kc.parent_object_id AND ikey.index_id = kc.unique_index_id
					INNER JOIN [{1}].sys.columns col
						ON col.object_id = ikey.object_id AND col.column_id = ikey.column_id
				WHERE
					tab.is_ms_shipped = 0
					AND kc.is_ms_shipped = 0
				) NewConstraint

				-- Constraints match if Schema, Table, Name, Type, IndexType and KeyColumn all match
				ON CurConstraint.SchemaName = NewConstraint.SchemaName
				AND CurConstraint.TableName = NewConstraint.TableName
				AND CurConstraint.IndexName = NewConstraint.IndexName
				AND CurConstraint.ConstraintType = NewConstraint.ConstraintType
				AND CurConstraint.IndexType = NewConstraint.IndexType
				AND CurConstraint.ColumnName = NewConstraint.ColumnName

			WHERE
				NewConstraint.TableName is null
			OPTION (USE HINT ('FORCE_LEGACY_CARDINALITY_ESTIMATION'))";

		#endregion

		#region Create New Primary Key and Unique constraints

		/// <summary>
		///	--------------------------------------------------------------------------------------------------------
		///	-- CREATE PrimaryKey AND Unique CONSTRAINTS THAT HAVE BEEN ADDED IN THE TEMPLATE SCHEMA
		///	-- OR THAT HAVE BEEN MODIFIED AND HENCE WERE DROPPED IN DropOldOrModifiedPksAndUqs
		///	--------------------------------------------------------------------------------------------------------
		/// </summary>
		void CeateNewPksAndUqs()
		{
			CheckConflictingConstraints();

			string constraintsToCreateScript = GetScriptReplacingDbNames(SelectNewPksAndUqsRaw);
			var constraintsToCreate = DataUtils.GetDataTableFromQuery(upgConnection, constraintsToCreateScript);

			var columnNamesInReverseOrder = new List<string>();
			foreach (DataRow row in constraintsToCreate.Rows)
			{
				string columnName = row["ColumnName"].ToString().Trim();
				int columnNumber = Convert.ToInt32(row["ColumnNumber"]);
				string sortDirection = (Convert.ToInt32(row["IsDescendingKey"]) == 0) ? "ASC" : "DESC";

				columnNamesInReverseOrder.Add(columnName.QuoteName() + " " + sortDirection);
				if (columnNumber == 1)
				{
					string schemaName = row["SchemaName"].ToString().Trim();
					string tableName = row["TableName"].ToString().Trim();
					string constraintName = row["ConstraintName"].ToString().Trim();
					string constraintType = row["ConstraintType"].ToString().Trim();
					string constraintKeyword = (String.Compare(constraintType, PkType, true) == 0) ? PkKeyword : UqKeyword;
					string isClustered = (Convert.ToInt32(row["IsClustered"]) == 0) ? "NONCLUSTERED" : "CLUSTERED";

					taskLogger.ShowInfoMessage("\t  (+) " + schemaName + "." + tableName + "." + constraintName);
					if (isClustered == "CLUSTERED")
					{
						DropClusteredIndex(schemaName, tableName);
					}

					columnNamesInReverseOrder.Reverse();
					string createScript = String.Format(
						"ALTER TABLE {0}.{1} ADD CONSTRAINT {2} {3} {4} ({5});",
						schemaName.QuoteName(), tableName.QuoteName(), constraintName.QuoteName(), constraintKeyword, isClustered, string.Join(", ", columnNamesInReverseOrder));
					upgConnection.ExecuteNonQuery(createScript);

					columnNamesInReverseOrder.Clear();
				}
			}
		}

		void CheckConflictingConstraints()
		{
			string conflictingConstraintsScript = GetScriptReplacingDbNames(SelectConflictingConstraintsRaw);
			var conflictingConstraints = DataUtils.GetDataTableFromQuery(upgConnection, conflictingConstraintsScript);

			if (conflictingConstraints.Rows.Count > 0)
			{
				var errorMsgBuilder = new StringBuilder();
				errorMsgBuilder.AppendLine();
				errorMsgBuilder.AppendLine("The following constraints on client-defined tables conflict with our database schema.");
				errorMsgBuilder.AppendLine("They must be removed/renamed before an upgrade can be applied.");

				foreach (DataRow row in conflictingConstraints.Rows)
				{
					errorMsgBuilder.AppendLine().AppendFormat(
						"\t{0}.{1} ({2}) - conflicts with {3}",
						row["ConflictingTableName"].ToString().Trim(),
						row["ConstraintName"].ToString().Trim(),
						row["ConstraintType"].ToString().Trim(),
						row["TableName"].ToString().Trim());
				}

				throw new Exception(errorMsgBuilder.ToString(), null);
			}
		}

		void DropClusteredIndex(string schemaName, string tableName)
		{
			var fullTableName = schemaName.QuoteName() + "." + tableName.QuoteName();
			var indexName = upgConnection.ExecuteScalar(string.Format(CultureInfo.InvariantCulture,
				"SELECT name FROM sys.indexes WHERE object_id = OBJECT_ID(N{0}) AND index_id = 1 OPTION (RECOMPILE);"
				, fullTableName.QuoteName('\'') // 0
				)) as string;

			if (!string.IsNullOrWhiteSpace(indexName))
			{
				upgConnection.ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture,
					"DROP INDEX {2} ON {0}.{1};"
					, schemaName.QuoteName() // 0
					, tableName.QuoteName()  // 1
					, indexName.QuoteName()  // 2
					));
			}
		}

		const string PkType = "PK";
		const string PkKeyword = "PRIMARY KEY";
		const string UqKeyword = "UNIQUE";

		const string SelectConflictingConstraintsRaw = @"
			SELECT
				existingClientTab.name ConflictingTableName,
				existingClientKc.name ConstraintName,
				existingClientKc.type ConstraintType,
				tab.name TableName
			FROM
				[{1}].sys.key_constraints kc
				INNER JOIN [{1}].sys.tables tab ON tab.object_id = kc.parent_object_id
				INNER JOIN [{0}].sys.key_constraints existingClientKc ON existingClientKc.name = kc.name
				INNER JOIN [{0}].sys.tables existingClientTab ON existingClientTab.object_id = existingClientKc.parent_object_id
			WHERE
				tab.is_ms_shipped = 0
				AND kc.is_ms_shipped = 0
				AND existingClientTab.name != tab.name
			;";

		const string SelectNewPksAndUqsRaw = @"
			SELECT
				sch.name SchemaName,
				tab.name TableName,
				kc.name ConstraintName,
				kc.type ConstraintType,
				col.name ColumnName,
				iKey.key_Ordinal ColumnNumber,
				ikey.is_descending_key IsDescendingKey,
				(CASE ind.type WHEN 1 THEN 1 else 0 END) IsClustered
			FROM
				[{1}].sys.key_constraints kc
				INNER JOIN [{1}].sys.tables tab
					ON tab.object_id = kc.parent_object_id
				INNER JOIN [{1}].sys.schemas sch
					ON tab.schema_id = sch.schema_id
				INNER JOIN [{1}].sys.indexes ind
					ON ind.object_id = kc.parent_object_id and ind.index_id = kc.unique_index_id
				INNER JOIN [{1}].sys.index_columns ikey
					ON ikey.object_id = kc.parent_object_id AND ikey.index_id = kc.unique_index_id
				INNER JOIN [{1}].sys.columns col
					ON col.object_id = ikey.object_id AND col.column_id = ikey.column_id
				LEFT JOIN [{0}].sys.key_constraints curKc
					ON curKc.name = kc.name
			WHERE
				tab.is_ms_shipped = 0
				AND kc.is_ms_shipped = 0
				AND curKc.name is null
			order by kc.name, ColumnNumber desc
			;";

		#endregion

	}
}
