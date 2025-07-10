using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.DbUpgrader.Foundation;
using CargoWise.DbUpgrader.Schema.Synchronisers;

namespace Enterprise.DbUpgrader.Schema
{
	public class SchemaSynchroniser
	{
		public SchemaSynchroniser(DbConnection upgConnection, IUpgradeTaskWorkflowLogger taskLogger, string dbToSynchronise, string templateDb)
		{
			this.dbBeingSynchronised = dbToSynchronise;
			this.templateDb = templateDb;
			this.upgConnection = upgConnection;
			this.taskLogger = taskLogger;

			this.userSchemaSynchroniser = new UserSchemaSynchroniser(upgConnection, dbToSynchronise, templateDb, taskLogger);
			this.tableMetadataRunner = new TableScriptRunner(upgConnection, dbToSynchronise, templateDb);
			IndexMetadataRunner = new IndexScriptRunner(upgConnection, dbToSynchronise, templateDb, taskLogger);
			this.columnstoreIndexSynchroniser = new ColumnstoreIndexSynchroniser(upgConnection, dbToSynchronise, templateDb);
			this.spatialIndexSynchroniser = new SpatialIndexSynchroniser(upgConnection, dbToSynchronise, templateDb, taskLogger);
			this.constraintMetadataRunner = new ConstraintScriptRunner(upgConnection, dbToSynchronise, templateDb, taskLogger);
			this.pkAndUkSynchroniser = new PrimaryAndUniqueKeySynchroniser(upgConnection, dbToSynchronise, templateDb, this.tableMetadataRunner, this.taskLogger);
		}

		public void SynchroniseDatabaseSchema()
		{
			taskLogger.StartTask("Create new user schemas");
			userSchemaSynchroniser.CreateNewUserSchemas();

			// Synchronise objects
			SynchroniseDatabaseSchemaObjects();

			taskLogger.StartTask("Drop old user schemas");
			userSchemaSynchroniser.DropOldUserSchemas();
		}

		public void CreateAndValidateCheckConstraints()
		{
			taskLogger.StartTask("Synchronising Check Constraints");
			taskLogger.ActivateSubtaskProgress(2);

			taskLogger.StartSubtask("Create check constraints (WITH CHECK)");
			constraintMetadataRunner.CreateCheckConstraints();

			taskLogger.StartSubtask("Ensure check constraints are enabled and trusted");
			constraintMetadataRunner.EnsureCheckConstraintsAreEnabled();
		}

		void SynchroniseDatabaseSchemaObjects()
		{
			taskLogger.StartTask("Synchronising XML Schemas");
			SynchroniseXmlSchemas();
			taskLogger.StartTask("Synchronising Tables");
			SynchroniseTables();
			taskLogger.StartTask("Synchronising Constraints and Indexes");
			SynchroniseAllConstraintsAndIndexes();
			taskLogger.StartTask("Synchronising Table Valued Types");
			SynchroniseTableValuedTypes();
		}

		#region Implementation

		#region Constants and Attributes

		protected readonly string dbBeingSynchronised;
		protected readonly string templateDb;
		protected readonly DbConnection upgConnection;
		protected readonly IUpgradeTaskWorkflowLogger taskLogger;
		protected readonly TableScriptRunner tableMetadataRunner;
		readonly UserSchemaSynchroniser userSchemaSynchroniser;
		public IndexScriptRunner IndexMetadataRunner { get; }
		readonly ColumnstoreIndexSynchroniser columnstoreIndexSynchroniser;
		readonly SpatialIndexSynchroniser spatialIndexSynchroniser;
		readonly ConstraintScriptRunner constraintMetadataRunner;
		readonly PrimaryAndUniqueKeySynchroniser pkAndUkSynchroniser;

		#endregion

		#region Upgrade Scripts

		#region Synchronise XML Schemas

		void SynchroniseXmlSchemas()
		{
			try
			{
				XmlSchemaSynchroniser xsdSynchroniser = new XmlSchemaSynchroniser(upgConnection, taskLogger, dbBeingSynchronised, templateDb);
				xsdSynchroniser.SynchroniseXmlSchemas();
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				taskLogger.ShowTaskError(e.ToString());
				throw new Exception("Failed to upgrade XML schemas.\r\n" + e.Message, e);
			}
		}

		#endregion

		#region Synchronise Tables

		/// <summary>
		/// Compares the Tables in the current DB to the template ones
		///   - Old tables should be removed (found only in the Current DB)
		///   - Modified tables should be upgraded (found in both with differences)
		///   - New tables should be created (found only in the Template DB)
		/// </summary>
		/// <returns>
		/// bool Successful (True/False)
		/// </returns>
		void SynchroniseTables()
		{
			try
			{
				taskLogger.StartTask("Removing Old Tables");
				DropUnsupportedSchemaTables();
				DropAllOldTables();
				taskLogger.StartTask("Dropping Old/Modified Sensitivity Classifications");
				DropOldAndModifiedSensitivityClassifications();
				DropTablesWhichAllExistingColumnsAreToBeRemoved();
				taskLogger.StartTask("Updating Modified Tables");
				UpdateAllModifiedTables();
				FixTableNameCasing();
				taskLogger.StartTask("Creating New Tables");
				CreateAllNewTables();
				taskLogger.StartTask("Creating New/Modified Sensitivity Classifications");
				AddNewAndModifiedSensitivityClassifications();
				SynchroniseTableLockEscalation();
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				taskLogger.ShowTaskError(e.ToString());
				throw new Exception("Failed to upgrade tables." + System.Environment.NewLine + e.Message, e);
			}
		}

		void DropUnsupportedSchemaTables()
		{
			var tablesToDrop = tableMetadataRunner.GetListOfTablesFromUnsupportedSchemas();
			taskLogger.ActivateSubtaskProgress(tablesToDrop.Rows.Count);

			foreach (DataRow row in tablesToDrop.Rows)
			{
				string schemaName = row["SchemaName"].ToString().Trim();
				string tableName = row["TableName"].ToString().Trim();
				taskLogger.StartSubtask(schemaName + "." + tableName);
				DropTable(schemaName, tableName);
			}
		}

		void DropAllOldTables()
		{
			var tablesToDrop = tableMetadataRunner.GetOldTablesDataTable();
			taskLogger.ActivateSubtaskProgress(tablesToDrop.Rows.Count);

			foreach (DataRow row in tablesToDrop.Rows)
			{
				string schemaName = row["SchemaName"].ToString().Trim();
				string tableName = row["TableName"].ToString().Trim();
				taskLogger.StartSubtask(schemaName + "." + tableName);
				DropTable(schemaName, tableName);
			}
		}

		void DropOldAndModifiedSensitivityClassifications()
		{
			var classificationsToDrop = tableMetadataRunner.GetSensitivityClassificationsToDrop();

			using (((ICurrentDbControl)upgConnection).UseDatabase(dbBeingSynchronised))
			{
				foreach (DataRow row in classificationsToDrop.Rows)
				{
					var schemaName = row["TabSchema"].ToString().QuoteName();
					var tableName = row["TabName"].ToString().QuoteName();
					var columnName = row["ColName"].ToString().QuoteName();
					taskLogger.ShowInfoMessage($"    (-) {schemaName}.{tableName}.{columnName}");
					upgConnection.ExecuteNonQuery($"DROP SENSITIVITY CLASSIFICATION FROM {schemaName}.{tableName}.{columnName}");
				}
			}
		}

		void DropTablesWhichAllExistingColumnsAreToBeRemoved()
		{
			var tablesToDrop = tableMetadataRunner.GetTablesWhichAllExistingColumnsAreBeingRemovedDataTable();
			taskLogger.ActivateSubtaskProgress(tablesToDrop.Rows.Count);

			foreach (DataRow row in tablesToDrop.Rows)
			{
				string schemaName = row["SchemaName"].ToString().Trim();
				string tableName = row["TableName"].ToString().Trim();
				taskLogger.StartSubtask(String.Format(CultureInfo.InvariantCulture, "[{0}].[{1}] (all existing columns removed)", schemaName, tableName));
				DropTable(schemaName, tableName);
			}
		}

		void CreateAllNewTables()
		{
			var tablesToCreate = tableMetadataRunner.GetNewTablesDataTable();
			taskLogger.ActivateSubtaskProgress(tablesToCreate.Rows.Count);

			foreach (DataRow row in tablesToCreate.Rows)
			{
				string schemaName = row["SchemaName"].ToString().Trim();
				string tableName = row["TableName"].ToString().Trim();
				taskLogger.StartSubtask(String.Format("[{0}].[{1}]", schemaName, tableName));
				tableMetadataRunner.CreateNewTable(schemaName, tableName);
			}
		}

		void AddNewAndModifiedSensitivityClassifications()
		{
			var classificationsToCreate = tableMetadataRunner.GetSensitivityClassificationsToCreate();

			using (((ICurrentDbControl)upgConnection).UseDatabase(dbBeingSynchronised))
			{
				foreach (DataRow row in classificationsToCreate.Rows)
				{
					var schemaName = row["TabSchema"].ToString().QuoteName();
					var tableName = row["TabName"].ToString().QuoteName();
					var columnName = row["ColName"].ToString().QuoteName();
					taskLogger.ShowInfoMessage($"    (+) {schemaName}.{tableName}.{columnName}");

					var sqlText = GetAddingSensitivityClassificationSql(row, schemaName, tableName, columnName);
					upgConnection.ExecuteNonQuery(sqlText);
				}
			}

			string GetAddingSensitivityClassificationSql(DataRow row, string targetSchemaName, string targetTableName, string targetColumnName)
			{
				var sqlCommandBuilder = new StringBuilder();
				sqlCommandBuilder.AppendLine($@"ADD SENSITIVITY CLASSIFICATION TO {targetSchemaName}.{targetTableName}.{targetColumnName}
WITH (");
				if (!row["Label"].ToString().IsNullOrEmpty())
				{
					sqlCommandBuilder.Append($"LABEL = N'{row["Label"]}',");
				}
				if (!row["LabelId"].ToString().IsNullOrEmpty())
				{
					sqlCommandBuilder.Append($"LABEL_ID = N'{row["LabelId"]}',");
				}
				if (!row["InfoType"].ToString().IsNullOrEmpty())
				{
					sqlCommandBuilder.Append($"INFORMATION_TYPE = N'{row["InfoType"]}',");
				}
				if (!row["InfoTypeId"].ToString().IsNullOrEmpty())
				{
					sqlCommandBuilder.Append($"INFORMATION_TYPE_ID = N'{row["InfoTypeId"]}',");
				}
				if (!row["Rank"].ToString().IsNullOrEmpty())
				{
					sqlCommandBuilder.Append($"RANK = {row["Rank"]},");
				}

				sqlCommandBuilder.Remove(sqlCommandBuilder.Length - 1, 1);
				sqlCommandBuilder.AppendLine(")");
				return sqlCommandBuilder.ToString();
			}
		}

		protected virtual void UpdateAllModifiedTables()
		{
			colSynchroniser = new ColumnSynchroniser(upgConnection, taskLogger, dbBeingSynchronised, templateDb);
			colSynchroniser.DropAlterAndAddColumns();
		}
		ColumnSynchroniser colSynchroniser;

		public IEnumerable<SchemaTableNamePair> DistinctModifiedTablesIgnoringCase
			=> colSynchroniser?.DistinctModifiedTablesIgnoringCase ?? Enumerable.Empty<SchemaTableNamePair>();

		void FixTableNameCasing()
		{
			var tablesToFixCase = tableMetadataRunner.GetTablesToFixCaseDataTable();
			taskLogger.ActivateSubtaskProgress(tablesToFixCase.Rows.Count);

			foreach (DataRow row in tablesToFixCase.Rows)
			{
				string schemaName = row["SchemaName"].ToString().Trim();
				string oldTableName = row["OldTableName"].ToString().Trim();
				string newTableName = row["NewTableName"].ToString().Trim();
				taskLogger.StartSubtask("(#) " + schemaName + "." + oldTableName + " => " + newTableName);
				tableMetadataRunner.RenameTable(schemaName, oldTableName, newTableName);
			}
		}

		void SynchroniseTableLockEscalation()
		{
			taskLogger.ShowInfoMessage("(Synchronising lock escalation settings)");
			tableMetadataRunner.SynchroniseLockEscalationSettings();
		}

		void DropTable(string schemaName, string tableName)
		{
			tableMetadataRunner.DropReferencingFKs(schemaName, tableName);
			tableMetadataRunner.DropSchemaBoundReferencingObjects(schemaName, tableName);
			string sqlText = String.Format("DROP TABLE [{0}].[{1}].[{2}]", dbBeingSynchronised, schemaName, tableName);
			upgConnection.ExecuteNonQuery(sqlText);
		}

		#endregion

		#region Synchronise Constraints and Indexes

		void SynchroniseAllConstraintsAndIndexes()
		{
			try
			{
				taskLogger.ActivateSubtaskProgress(6);

				taskLogger.StartSubtask("Primary Key and Unique constraints");
				SynchroniseAllPrimaryKeyAndUniqueConstraints();
				taskLogger.StartSubtask("Indexes");
				SynchroniseAllIndexes();
				taskLogger.StartSubtask("Drop check constraints");
				DropCheckConstraints();
				taskLogger.StartSubtask("Default constraints");
				SynchroniseAllDefaultConstraints();
				taskLogger.StartSubtask("Relationships");
				SynchroniseAllFKs();
				taskLogger.StartSubtask("Ensure Foreign Key constraints are enabled and trusted");
				EnsureForeignKeyConstraintsAreEnabledAndTrusted();
			}
			catch (Exception e)
			{
				throw new Exception("Failed to synchronise constraints and indexes.\r\n" + e.Message, e);
			}
		}

		void SynchroniseAllIndexes()
		{
			taskLogger.ShowInfoMessage("\tSynchronising on-line indexes");
			IndexMetadataRunner.SynchroniseOnlineIndexes();

			taskLogger.ShowInfoMessage("\tRemoving Old Indexes");
			IndexMetadataRunner.DropOldIndexes();

			taskLogger.ShowInfoMessage("\tRecreating Modified Indexes");
			IndexMetadataRunner.RecreateModifiedIndexes();

			taskLogger.ShowInfoMessage("\tCreating New Indexes");
			IndexMetadataRunner.CreateNewIndexes();

			taskLogger.ShowInfoMessage("\tSynchronising Columnstore Indexes");
			columnstoreIndexSynchroniser.SynchroniseAll();

			taskLogger.ShowInfoMessage("\tSynchronising Spatial Indexes");
			spatialIndexSynchroniser.SynchroniseAll();

			taskLogger.ShowInfoMessage("\tSynchronising Index Options");
			IndexMetadataRunner.SynchroniseIndexOptions();
		}

		/// <summary>
		/// Drop, Create or Modify(Recreate) Primary Key or Unique Constraints
		/// </summary>
		void SynchroniseAllPrimaryKeyAndUniqueConstraints()
		{
			pkAndUkSynchroniser.SynchronisePrimaryAndUniqueConstraints();
		}

		/// <summary>
		/// Drop, Create or Modify(Recreate) CHECK constraints
		/// Note: CHECK constraints of tables that are not in the new schema (unmanaged tables) are IGNORED
		/// </summary>
		void DropCheckConstraints()
		{
			constraintMetadataRunner.DropCheckConstraints();
		}

		/// <summary>
		/// Drop, Create or Modify(Recreate) DEFAULT constraints
		/// Note: DEFAULT constraints of tables that are not in the new schema (unmanaged tables) are IGNORED
		/// </summary>
		void SynchroniseAllDefaultConstraints()
		{
			constraintMetadataRunner.SynchroniseAllDefaultConstraints();
		}

		void SynchroniseAllFKs()
		{
			constraintMetadataRunner.SynchroniseAllForeignKeys();
		}

		void EnsureForeignKeyConstraintsAreEnabledAndTrusted()
		{
			constraintMetadataRunner.EnsureForeignKeyConstraintsAreEnabledAndTrusted();
		}

		#endregion

		#region Synchronise Table Valued Types

		void SynchroniseTableValuedTypes()
		{
			try
			{
				var synchroniser = new TableValuedTypesSynchroniser(
					upgConnection,
					taskLogger);
				synchroniser.SynchroniseTableValuedTypes();
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				taskLogger.ShowTaskError(e.ToString());
				throw new Exception("Failed to Synchronise Table Valued Types.\r\n" + e.Message, e);
			}
		}

		#endregion // Synchronise Table Valued Types

		#endregion // Upgrade Scripts

		#endregion // Implementation
	}
}
