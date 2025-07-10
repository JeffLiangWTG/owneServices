using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Bi.Common;
using CargoWise.Bi.Configuration.DataSets;
using CargoWise.Bi.Developement.SchemaSync;
using CargoWise.Bi.Development.Common;
using CargoWise.Bi.Development.SchemaSync.DataSets;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.Client.EDI;
using Enterprise.ZArchitecture.Environment;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Bi.Development.SchemaSync
{
	public static class SchemaSynchroniser
	{
		#region SuppressResourceStringsCheckRegion

		#region Static Members
		[ThreadSafe]
		static readonly string[] attributeNames = new string[] { "DataType", "MaxLength", "Precision", "Scale", "Nullable", "IsPrimaryKey", "ReferenceSchema", "ReferenceTable" };
		[ThreadSafe]
		static readonly SchemaDataSet schema = new SchemaDataSet();

		[ThreadStatic]
		public static Action<DbConnection, string> BeforeBiConfigurationLoad;

		[ThreadStatic]
		public static Action<DbConnection, string> AfterBiConfigurationLoad;

		#endregion

		public static void SyncConfiguration(bool parseSchema = false)
		{
			LoadBiConfiguration();
			SyncSchemaConfiguration(parseSchema);
			BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.Validate();
			SaveConfiguration();
			TestQueryForAllEdwTables();
		}

		public static void LoadBiConfiguration()
		{
			if (!BiAutomationConfigLoaderForDevelopment.Instance.Loaded)
			{
				BiLogger.StartTask("Loading BI Configuration XML");
				BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.Validate();

				if (!Globals.IsTest)
				{
					new ColumnMapper().MapCdcColumnsToModelView();
				}
			}
		}

		public static void SyncSchemaConfiguration(bool parseSchema = false)
		{
			using (Db.DisableSchemaVersionCheck())
			using (var connection = Db.NewAdminConnection(BiDatabase.ServerName, Db.SqlMasterDb))
			{
				BiLogger.StartSubtask("Ensuring connection settings");
				connection.EnsureIsOpen();
				try
				{
					if (parseSchema)
					{
						BiDatabase.LoadSchemaFromSQL(schema, useBinaries: false);
					}
					else
					{
						BiDatabase.CreateTemplateDatabase(connection);
						CallBeforeBiConfigurationLoad(connection);
						LoadDatabaseSchema(connection);
					}
					BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.SortModelDependencyOrder();
					CompareConfigurations();
					MarkEdwTablesAndColumns();
					MarkEdiTablesAndColumns();
					MarkAuditTablesAndColumns();
					MarkColumnsAsCdcEnabled();

					if (!Globals.IsTest)
					{
						new ColumnMapper().MapCdcColumnsToModelView();
					}

					CallAfterBiConfigurationLoad(connection);
				}
				finally
				{
					BiDatabase.DropTemplateDatabase(connection);
					BiDatabase.DropBiDatabases(connection);
				}
			}
		}

		public static void LoadDatabaseSchema(AdminConnection connection)
		{
			BiDatabase.LoadSchemaFromDatabase(connection, schema);
		}

		static void MarkColumnsAsCdcEnabled()
		{
			var cdcEnabledColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			var columnRegex = new Regex(@"\w*_\w*");

			foreach (var table in BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.CdcTableConfig.Where(t => !string.IsNullOrEmpty(t.AuditFilter)))
			{
				foreach (Match match in columnRegex.Matches(table.AuditFilter))
				{
					if (!cdcEnabledColumns.Contains(match.Value))
					{
						cdcEnabledColumns.Add(match.Value);
					}
				}
			}

			foreach (var table in BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.CdcTableConfig.Where(t => !string.IsNullOrEmpty(t.EdwFilter)))
			{
				foreach (Match match in columnRegex.Matches(table.EdwFilter))
				{
					if (!cdcEnabledColumns.Contains(match.Value))
					{
						cdcEnabledColumns.Add(match.Value);
					}
				}
			}

			foreach (var column in BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.CdcColumnConfig)
			{
				if (SourceTableIsEnabledForCdc(column) && MeetsCdcEnabledRequirements(cdcEnabledColumns, column))
				{
					column.CdcEnabled = true;
				}
				else
				{
					column.CdcEnabled = false;
				}
			}
		}
		public static bool MeetsCdcEnabledRequirements(HashSet<string> cdcEnabledColumns, BiAutomationConfigDataSet.CdcColumnConfigRow column)
		{
			return cdcEnabledColumns.Contains(column.SourceColumn) ||
				column.ColumnInAudit || column.ColumnInEdw;
		}

		static bool SourceTableIsEnabledForCdc(BiAutomationConfigDataSet.CdcColumnConfigRow column)
		{
			var table = column.GetCdcTable();
			return table.TableInAudit || table.TableInEdw;
		}

		static void MarkEdwTablesAndColumns()
		{
			var columnsInEdw = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			var columnRegex = new Regex(@"\w*_\w*");

			foreach (var edwTable in BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwTableConfig)
			{
				var stagingTable = edwTable.StagingTable;
				foreach (var edwColumn in edwTable.GetEdwColumnConfigRows())
				{
					foreach (Match match in columnRegex.Matches(edwColumn.Expression ?? ""))
					{
						var key = stagingTable + "." + match.Value;
						if (!columnsInEdw.Contains(key))
						{
							columnsInEdw.Add(key);
						}
					}
					foreach (Match match in columnRegex.Matches(edwColumn.Condition ?? ""))
					{
						var key = stagingTable + "." + match.Value;
						if (!columnsInEdw.Contains(key))
						{
							columnsInEdw.Add(key);
						}
					}
				}
			}

			foreach (var table in BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.CdcTableConfig)
			{
				foreach (var column in table.GetCdcColumnConfigRows())
				{
					column.ColumnInEdw = columnsInEdw.Contains(table.SourceTable + "." + column.SourceColumn);
					if (column.ColumnInEdw)
					{
						column.ColumnInAudit = true;
					}
				}

				table.TableInEdw = table.GetCdcColumnConfigRows().Any(c => c.ColumnInEdw);
				if (table.TableInEdw && string.IsNullOrEmpty(table.IndexedColumn))
				{
					table.IndexedColumn = table.GetCdcColumnConfigRows().FirstOrDefault(c => c.IsPrimaryKey)?.SourceColumn;
				}
			}
		}

		static void MarkEdiTablesAndColumns()
		{
			var allTables = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.CdcTableConfig.OrderBy(x => x.SourceTable);
			foreach (var table in allTables)
			{
				table.IsEdiClient = false;
			}

			var scriptNames = EDIClientDbSchemaUpgradeInfo.TableCreationScripts.Value.Select(x => x.ObjectName);
			foreach (var cdcTable in BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.CdcTableConfig)
			{
				if (scriptNames.ToList().Contains(cdcTable.SourceTable, StringComparer.OrdinalIgnoreCase))
				{
					cdcTable.IsEdiClient = true;
				}
			}
		}

		static void MarkAuditTablesAndColumns()
		{
			foreach (var table in BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.CdcTableConfig)
			{
				table.TableInAudit = table.GetCdcColumnConfigRows().Any(c => c.ColumnInAudit);
			}
		}

		public static void SaveConfiguration()
		{
			BiLogger.StartSubtask("Saving BI Configuration");
			edwDatabaseCreated = false;
			SaveBiAutomationConfiguration();
			TsqlScripts.GenerateSchemaFiles();
		}

		static void CallBeforeBiConfigurationLoad(DbConnection connection)
		{
			var handler = BeforeBiConfigurationLoad;

			if (handler != null)
			{
				try
				{
					handler(connection, BiDatabase.TemplateDatabaseName);
				}
				finally
				{
					BeforeBiConfigurationLoad = null;
				}
			}
		}

		static void CallAfterBiConfigurationLoad(DbConnection connection)
		{
			var handler = AfterBiConfigurationLoad;

			if (handler != null)
			{
				try
				{
					handler(connection, BiDatabase.TemplateDatabaseName);
				}
				finally
				{
					AfterBiConfigurationLoad = null;
				}
			}
		}

		static void CompareConfigurations()
		{
			try
			{
				BiLogger.StartSubtask("Comparing configuration with database schema");
				var definitionList = schema.Definition.Select(definition => new Definition(definition.SourceSchema, definition.SourceTable)).GroupBy(d => d.SourceTable).Select(d => d.First());

				if (definitionList.Any())
				{
					MarkAddedTables(definitionList.OrderBy(x => x.SourceTable));
					MarkRemovedTablesAsDeleted(definitionList);
					MarkTablesWithChangedCasing(definitionList);
					MarkAddedAndChangedColumns();
					MarkRemovedColumnAsDeleted();
					MarkColumnsAsChangedCasing();
					MarkUnchangedTablesWithChangedColumns();
				}

				BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.AcceptChanges();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				BiLogger.Fail("Configuration comparison failed");
				throw new BiDatabaseSyncException(ex.Message, ex);
			}
		}

		static void MarkUnchangedTablesWithChangedColumns()
		{
			var changedColumnOnlyTableQuery = from existingCdcTableConfig in BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.CdcTableConfig
											  join existingCdcColumnConfig in BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.CdcColumnConfig
												  on new { existingCdcTableConfig.SourceTable }
												  equals new { existingCdcColumnConfig.SourceTable }
											  where
											  existingCdcTableConfig.Action == "Unchanged"
											  && existingCdcColumnConfig.Action != "Unchanged"
											  select existingCdcTableConfig;

			foreach (var changeColumnOnlyTable in changedColumnOnlyTableQuery.Distinct())
			{
				BiLogger.StartSubtask(string.Format(CultureInfo.InvariantCulture, "Marking table [{0}] as Changed Column Only", changeColumnOnlyTable.SourceTable));
				changeColumnOnlyTable.Action = "ChangedColumnOnly";
			}
		}

		public static void MarkRemovedColumnAsDeleted()
		{
			var columnConfigOuterQuery = from existingCdcColumnConfig in BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.CdcColumnConfig
										 join existingCdcTableConfig in BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.CdcTableConfig
											 on new { SourceTable = existingCdcColumnConfig.SourceTable.ToUpperInvariant() }
											 equals new { SourceTable = existingCdcTableConfig.SourceTable.ToUpperInvariant() }
										 join currentDefinitionRow in schema.Definition
											 on new { SourceSchema = existingCdcTableConfig.SourceSchema.ToUpperInvariant(), SourceTable = existingCdcTableConfig.SourceTable.ToUpperInvariant(), SourceColumn = existingCdcColumnConfig.SourceColumn.ToUpperInvariant() }
											 equals new { SourceSchema = currentDefinitionRow.SourceSchema.ToUpperInvariant(), SourceTable = currentDefinitionRow.SourceTable.ToUpperInvariant(), SourceColumn = currentDefinitionRow.SourceColumn.ToUpperInvariant() }
											 into outerJoinRows
										 from currentDefinition in outerJoinRows.DefaultIfEmpty()
										 where
											 currentDefinition == null
										 select existingCdcColumnConfig;

			foreach (var cdcColumnConfigRow in columnConfigOuterQuery.Distinct())
			{
				BiLogger.StartSubtask(string.Format(CultureInfo.InvariantCulture, "Marking column [{0}] as Deleted", cdcColumnConfigRow.SourceColumn));
				cdcColumnConfigRow.Action = "Deleted";

				foreach (var edwColumn in BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwColumnConfig.GetDependentEdwColumns(cdcColumnConfigRow))
				{
					BiLogger.StartSubtask(string.Format(CultureInfo.InvariantCulture, "Setting expression for EDW column [{0}].[{1}] to NULL", edwColumn.GetEdwTable().Name, edwColumn.Name));
					edwColumn.Expression = "NULL";
				}
			}
		}

		static void MarkColumnsAsChangedCasing()
		{
			var columnConfigOuterQuery = from currentDefinitionRow in schema.Definition
												  join cdcColumnConfigRow in BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.CdcColumnConfig
												  on new { SourceTable = currentDefinitionRow.SourceTable.ToUpperInvariant(), SourceColumn = currentDefinitionRow.SourceColumn.ToUpperInvariant() }
												  equals new { SourceTable = cdcColumnConfigRow.SourceTable.ToUpperInvariant(), SourceColumn = cdcColumnConfigRow.SourceColumn.ToUpperInvariant() }
												  into outerJoinRows
												  from row in outerJoinRows.DefaultIfEmpty()
												  where row.SourceColumn != currentDefinitionRow.SourceColumn
												  select new { SourceTable = currentDefinitionRow.SourceTable, SourceColumn = currentDefinitionRow.SourceColumn };

			foreach (var definitionRow in columnConfigOuterQuery.Distinct())
			{
				var cdcColumnConfigRow = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.CdcColumnConfig.FirstOrDefault(t => t.SourceColumn.Equals(definitionRow.SourceColumn, StringComparison.OrdinalIgnoreCase));
				if (cdcColumnConfigRow != null)
				{
					BiLogger.StartSubtask(string.Format(CultureInfo.InvariantCulture, "Marking column [{0}] as Changed Casing", definitionRow.SourceColumn));
					cdcColumnConfigRow.SourceColumn = definitionRow.SourceColumn;
					cdcColumnConfigRow.Action = "Changed Casing";

					var regex = new Regex(string.Format(CultureInfo.InvariantCulture, @"\b{0}\b", cdcColumnConfigRow.SourceColumn), RegexOptions.IgnoreCase);
					foreach (var edwColumn in BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwColumnConfig.GetDependentEdwColumns(cdcColumnConfigRow))
					{
						edwColumn.Expression = regex.Replace(edwColumn.Expression, definitionRow.SourceColumn);
					}
				}
			}
		}

		static void MarkAddedAndChangedColumns()
		{
			var columns = GetNewAndChangedColumns();

			foreach (var definitionOuterQueryRow in columns.Distinct())
			{
				if (definitionOuterQueryRow.Item2 == null)
				{
					BiAutomationConfigDataSet.CdcColumnConfigRow newCdcColumnConfigRow = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.CdcColumnConfig.NewCdcColumnConfigRow();
					var firstElement = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.CdcTableConfig.First(cdcTableConfig =>
										cdcTableConfig.SourceSchema == definitionOuterQueryRow.Item1.SourceSchema
										&& cdcTableConfig.SourceTable == definitionOuterQueryRow.Item1.SourceTable);

					newCdcColumnConfigRow.SourceTable = firstElement.SourceTable;
					SetCdcColumnAttributes(definitionOuterQueryRow.Item1, newCdcColumnConfigRow, "Added");
					if (firstElement.Action == "Added")
					{
						newCdcColumnConfigRow.CdcEnabled = false;
					}
					else if (firstElement.GetCdcColumnConfigRows().Any(c => c.ColumnInAudit)
						&& newCdcColumnConfigRow.DataType != "geography" && !definitionOuterQueryRow.Item1.SourceColumn.Contains("ClusterKey"))
					{
						newCdcColumnConfigRow.CdcEnabled = true;
						newCdcColumnConfigRow.ColumnInAudit = true;
					}

					BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.CdcColumnConfig.Rows.Add(newCdcColumnConfigRow);
				}
				else
				{
					SetCdcColumnAttributes(definitionOuterQueryRow.Item1, definitionOuterQueryRow.Item2, "Changed");
				}
			}
		}

		static IEnumerable<Tuple<SchemaDataSet.DefinitionRow, BiAutomationConfigDataSet.CdcColumnConfigRow>> GetNewAndChangedColumns()
		{
			return from column in GetColumns()
				   where column.Item2 == null ||
				   !(
					  column.Item2.DataType == column.Item1.DataType
					  && column.Item2.MaxLength == column.Item1.MaxLength
					  && (column.Item2.IsPrimaryKey == column.Item1.IsPrimaryKey || column.Item2.IsPrimaryKey)
					  && column.Item2.Nullable == column.Item1.Nullable
					  && column.Item2.Precision == column.Item1.Precision
					  && column.Item2.Scale == column.Item1.Scale
					  && string.Equals(column.Item2.ReferenceSchema, column.Item1.ReferenceSchema, StringComparison.OrdinalIgnoreCase)
					  && string.Equals(column.Item2.ReferenceTable, column.Item1.ReferenceTable, StringComparison.OrdinalIgnoreCase)
						)
				   select column;
		}

		static IEnumerable<Tuple<SchemaDataSet.DefinitionRow, BiAutomationConfigDataSet.CdcColumnConfigRow>> GetColumns()
		{
			return from currentDefinitionRow in schema.Definition
				   join cdcTableConfig in BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.CdcTableConfig
				   on new { SourceSchema = currentDefinitionRow.SourceSchema.ToUpperInvariant(), SourceTable = currentDefinitionRow.SourceTable.ToUpperInvariant() }
				   equals new { SourceSchema = cdcTableConfig.SourceSchema.ToUpperInvariant(), SourceTable = cdcTableConfig.SourceTable.ToUpperInvariant() }
				   join cdcColumnConfig in BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.CdcColumnConfig
				   on new { SourceTable = cdcTableConfig.SourceTable.ToUpperInvariant(), SourceColumn = currentDefinitionRow.SourceColumn.ToUpperInvariant() }
				   equals new { SourceTable = cdcColumnConfig.SourceTable.ToUpperInvariant(), SourceColumn = cdcColumnConfig.SourceColumn.ToUpperInvariant() }
				   into outerJoinRows
				   from existingCdcColumnConfig in outerJoinRows.DefaultIfEmpty()
				   select new Tuple<SchemaDataSet.DefinitionRow, BiAutomationConfigDataSet.CdcColumnConfigRow>(currentDefinitionRow, existingCdcColumnConfig);
		}

		static void SetCdcColumnAttributes(SchemaDataSet.DefinitionRow definitionRow, BiAutomationConfigDataSet.CdcColumnConfigRow cdcColumnConfigRow, string action)
		{
			if (cdcColumnConfigRow != null)
			{
				switch (action)
				{
					case "Added":
						BiLogger.StartSubtask(string.Format(CultureInfo.InvariantCulture, "Marking column [{0}] as Added", definitionRow.SourceColumn));
						cdcColumnConfigRow.SourceColumn = definitionRow.SourceColumn;
						cdcColumnConfigRow.Action = action;

						foreach (string attributeName in attributeNames)
						{
							cdcColumnConfigRow[attributeName] = definitionRow[attributeName];
						}
						break;

					case "Changed":
						foreach (string attributeName in attributeNames)
						{
							if (cdcColumnConfigRow[attributeName].ToString() != definitionRow[attributeName].ToString())
							{
								cdcColumnConfigRow[attributeName] = definitionRow[attributeName];
								if (cdcColumnConfigRow.Action != "Changed")
								{
									BiLogger.StartSubtask(string.Format(CultureInfo.InvariantCulture, "Marking column [{0}] as Changed", cdcColumnConfigRow.SourceColumn));
									cdcColumnConfigRow.Action = "Changed";
								}
							}
						}

						if (cdcColumnConfigRow.Action == "Changed")
						{
							foreach (var edwTable in BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwTableConfig.Where(t => string.Equals(t.StagingTable, cdcColumnConfigRow.SourceTable, StringComparison.OrdinalIgnoreCase)))
							{
								foreach (var edwColumn in
									edwTable.GetEdwColumnConfigRows().Where(c =>
										string.Equals(cdcColumnConfigRow.SourceColumn, c.Expression, StringComparison.OrdinalIgnoreCase) &&
										(!c.Name.EndsWith("Key", StringComparison.OrdinalIgnoreCase) ||
										 !c.DataType.Equals("bigint", StringComparison.OrdinalIgnoreCase))))
								{
									edwColumn.DataType = cdcColumnConfigRow.DataType;
									edwColumn.MaxLength = cdcColumnConfigRow.MaxLength;
									edwColumn.Scale = cdcColumnConfigRow.Scale;
									edwColumn.Precision = cdcColumnConfigRow.Precision;
								}
							}
						}
						break;
				}
			}
		}

		static void MarkRemovedTablesAsDeleted(IEnumerable<Definition> definitionList)
		{
			var tableConfigOuterQuery = from cdcTableConfigRow in BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.CdcTableConfig
										join definition in definitionList
										on new { SourceSchema = cdcTableConfigRow.SourceSchema.ToUpperInvariant(), SourceTable = cdcTableConfigRow.SourceTable.ToUpperInvariant() }
										equals new { SourceSchema = definition.SourceSchema.ToUpperInvariant(), SourceTable = definition.SourceTable.ToUpperInvariant() }
										into outerJoinRows
										from row in outerJoinRows.DefaultIfEmpty()
										where row == null
										select cdcTableConfigRow;

			foreach (var cdcTableConfig in tableConfigOuterQuery.Distinct())
			{
				BiLogger.StartSubtask(string.Format(CultureInfo.InvariantCulture, "Marking table [{0}] as Deleted", cdcTableConfig.SourceTable));
				cdcTableConfig.Action = "Deleted";
			}
		}

		static void MarkAddedTables(IEnumerable<Definition> definitionList)
		{
			var tableDefinitionOuterQuery = from definition in definitionList
											join cdcTableConfigRow in BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.CdcTableConfig
											on new { SourceSchema = definition.SourceSchema.ToUpperInvariant(), SourceTable = definition.SourceTable.ToUpperInvariant() }
											equals new { SourceSchema = cdcTableConfigRow.SourceSchema.ToUpperInvariant(), SourceTable = cdcTableConfigRow.SourceTable.ToUpperInvariant() }
											into outerJoinRows
											from row in outerJoinRows.DefaultIfEmpty()
											where row == null
											select new { SourceSchema = definition.SourceSchema, SourceTable = definition.SourceTable };

			foreach (var definitionRow in tableDefinitionOuterQuery.Distinct())
			{
				BiAutomationConfigDataSet.CdcTableConfigRow newCdcTableConfigRow = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.CdcTableConfig.NewCdcTableConfigRow();

				BiLogger.StartSubtask(string.Format(CultureInfo.InvariantCulture, "Marking table [{0}] as Added", definitionRow.SourceTable));
				newCdcTableConfigRow.SourceSchema = definitionRow.SourceSchema;
				newCdcTableConfigRow.SourceTable = definitionRow.SourceTable;
				newCdcTableConfigRow.Action = "Added";

				BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.CdcTableConfig.Rows.Add(newCdcTableConfigRow);
			}
		}

		static void MarkTablesWithChangedCasing(IEnumerable<Definition> definitionList)
		{
			var tableDefinitionOuterQuery = from definition in definitionList
											join cdcTableConfigRow in BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.CdcTableConfig
											on new { SourceSchema = definition.SourceSchema.ToUpperInvariant(), SourceTable = definition.SourceTable.ToUpperInvariant() }
											equals new { SourceSchema = cdcTableConfigRow.SourceSchema.ToUpperInvariant(), SourceTable = cdcTableConfigRow.SourceTable.ToUpperInvariant() }
											into outerJoinRows
											from row in outerJoinRows.DefaultIfEmpty()
											where row.SourceTable != definition.SourceTable
											select new { SourceSchema = definition.SourceSchema, SourceTable = definition.SourceTable };

			foreach (var definitionRow in tableDefinitionOuterQuery.Distinct())
			{
				var cdcTableConfigRow = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.CdcTableConfig.FirstOrDefault(t => t.SourceTable.Equals(definitionRow.SourceTable, StringComparison.OrdinalIgnoreCase));
				if (cdcTableConfigRow != null)
				{
					BiLogger.StartSubtask(string.Format(CultureInfo.InvariantCulture, "Marking table [{0}] as Changed Casing", definitionRow.SourceTable));

					BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.CdcTableConfig.SuspendValidation();
					BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.CdcColumnConfig.SuspendValidation();
					BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwTableConfig.SuspendValidation();

					cdcTableConfigRow.SourceTable = definitionRow.SourceTable;
					cdcTableConfigRow.Action = "Changed Casing";

					foreach (var edwTable in BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwTableConfig.Where(t => t.SourceSchema.Equals(definitionRow.SourceSchema, StringComparison.OrdinalIgnoreCase) && t.StagingTable.Equals(definitionRow.SourceTable, StringComparison.OrdinalIgnoreCase)))
					{
						edwTable.SourceSchema = definitionRow.SourceSchema;
						edwTable.StagingTable = definitionRow.SourceTable;
					}

					BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.CdcTableConfig.ResumeValidation();
					BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.CdcColumnConfig.ResumeValidation();
					BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwTableConfig.ResumeValidation();
				}
			}
		}

		public static void SaveBiAutomationConfiguration()
		{
			BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.Validate();
			DeleteOldRows();
			HideExcludedColumns();
			SaveConfigurationToFiles();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:Do Not Use DataSet", Justification = "Baseline")]
		static void SaveConfigurationToFiles()
		{
			var configFileTypes = Enum.GetValues(typeof(BiConfigFileType));

			foreach (BiConfigFileType configFileType in configFileTypes)
			{
				var configFileTypeString = configFileType.ToString();
				BiLogger.StartSubtask($"Saving [{configFileTypeString}]");

				var dt = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.Tables[configFileTypeString];
				using (var ds = CreateDataSet(dt))
				{
					var fileContent = BiConfigurationModification.GetConfigurationFileContent(ds);
					BiConfigurationFileHandler.SplitBiConfigFile(configFileType, fileContent);
				}
			}

			BiConfigurationFileHandler.AddConfigFilesToProject();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		static DataSet CreateDataSet(DataTable dt) // This 'BusinessObject' is not in the database.
		{
			var ds = new DataSet(); // This 'BusinessObject' is not in the database.
			ds.Locale = CultureInfo.InvariantCulture;
			var parentTable = dt.Copy();
			ds.DataSetName = parentTable.TableName;
			ds.Tables.Add(parentTable);

			foreach (DataRelation relation in dt.ChildRelations)
			{
				var childTable = relation.ChildTable.Copy();
				if (childTable.Columns.Contains("TabularModelLogicName"))
				{
					childTable.DefaultView.Sort = "Schema, Name";
				}
				else if (childTable.Columns.Contains("Name"))
				{
					childTable.DefaultView.Sort = "Name";
				}
				else if (childTable.Columns.Contains("SourceColumn"))
				{
					childTable.DefaultView.Sort = "SourceColumn";
				}
				else if (childTable.Columns.Contains("SsasModelLogicalName"))
				{
					childTable.DefaultView.Sort = "TableName";
				}
				else if (childTable.Columns.Contains("ReportName"))
				{
					childTable.DefaultView.Sort = "SourceSchema,StagingTable";
				}
				childTable = childTable.DefaultView.ToTable();
				ds.Tables.Add(childTable);

				var parentColumnList = new List<DataColumn>();
				foreach (var parentColumn in relation.ParentColumns)
				{
					parentColumnList.Add(parentTable.Columns[parentColumn.ColumnName]);
				}

				var childColumnList = new List<DataColumn>();
				foreach (var childColumn in relation.ChildColumns)
				{
					childColumnList.Add(childTable.Columns[childColumn.ColumnName]);
				}

				foreach (DataTable table in ds.Tables)
				{
					table.CaseSensitive = false;
					table.Locale = CultureInfo.InvariantCulture;
				}

				ds.Relations.Add(relation.RelationName, parentColumnList.ToArray(), childColumnList.ToArray());
				ds.Relations[relation.RelationName].Nested = relation.Nested;
			}

			return ds;
		}

		static void HideExcludedColumns()
		{
			foreach (DataTable table in BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.Tables)
			{
				foreach (DataColumn column in table.Columns)
				{
					if (column.ColumnName == "DependencyOrder" && table.TableName == "EdwCustomTableConfig")
					{
						column.ColumnMapping = MappingType.Attribute;
					}
					else
					{
						var hiddenColumns = new[] { "Action", "Message", "DependencyOrder", "ModelViewColumn", "SsasTableID", "EdwColumnConfig_ID", "EdwModelViewColumnID", "EdwDenormalizedColumnConfigID", "CdcTableConfig_Id", "SsasCubeID", "EdwTableConfig_ID", "EdwModelViewTableID", "EdwDenormalizedTableConfigID" };

						if (hiddenColumns.Contains(column.ColumnName))
						{
							column.ColumnMapping = MappingType.Hidden;
						}
						else
						{
							column.ColumnMapping = MappingType.Attribute;
						}
					}
				}
			}
		}

		static void DeleteOldRows()
		{
			foreach (BiAutomationConfigDataSet.CdcTableConfigRow cdcTableConfigRow in BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.CdcTableConfig)
			{
				var columnConfigRows = cdcTableConfigRow.GetCdcColumnConfigRows();
				foreach (BiAutomationConfigDataSet.CdcColumnConfigRow cdcColumnConfigRow in columnConfigRows)
				{
					if (cdcColumnConfigRow.Action == "Deleted" && cdcColumnConfigRow.RowState != DataRowState.Deleted)
					{
						cdcColumnConfigRow.Delete();
					}
				}

				if (cdcTableConfigRow.Action == "Deleted" && cdcTableConfigRow.RowState != DataRowState.Deleted)
				{
					cdcTableConfigRow.Delete();
				}
			}
			BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.AcceptChanges();
		}

		#region Transform Queries

		public static string TestSingleQuery(string source, string query, string operation)
		{
			string outputMsg;
			try
			{
				outputMsg = TestQueryUnsafe(source, query, operation);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				outputMsg = ex.Message;
			}
			return outputMsg;
		}

		static string TestQueryUnsafe(string source, string query, string operation, AdminConnection connection = null)
		{
			string outputMsg = null;
			if (string.IsNullOrEmpty(query) || string.IsNullOrEmpty(query.Trim(' ', '\r', '\n')))
			{
				if (operation != "Partition Query")
				{
					outputMsg = string.Format(CultureInfo.InvariantCulture, "{0} ({1})\r\nMissing query.", source, operation);
				}
			}
			else
			{
				if (connection is null)
				{
					outputMsg = CreateDatabaseIfNotCreatedAndRunQuery(query);
				}
				else
				{
					outputMsg = RunQuery(query, connection);
				}

				if (!string.IsNullOrEmpty(outputMsg))
				{
					outputMsg = string.Format(CultureInfo.InvariantCulture, "{0} ({1})\r\n{2}", source, operation, outputMsg);
				}
			}
			return outputMsg;
		}

		[ThreadSafe]
		static bool edwDatabaseCreated = false;

		public static string CreateDatabaseIfNotCreatedAndRunQuery(string query)
		{
			using (Db.DisableSchemaVersionCheck())
			using (var connection = Db.NewAdminConnection(BiDatabase.ServerName, Db.SqlMasterDb))
			{
				if (!edwDatabaseCreated)
				{
					BiDatabase.CreateEdwDatabase(connection);
					edwDatabaseCreated = true;
				}

				var result = RunQuery(query, connection);
				return result;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public static string RunQuery(string query, AdminConnection connection)
		{
			string outputMsg = null;
			try
			{
				using (((ICurrentDbControl)connection).UseDatabase(BiDatabase.EdwDatabaseName))
				{
					connection.BeginTransaction();
					var regex = new Regex(@"(\s|\r|\n|^)GO(\s|\r|\n|$)", RegexOptions.IgnoreCase);
					foreach (var subquery in regex.Split(query).ToList().Where(q => !string.IsNullOrWhiteSpace(q)))
					{
						connection.ExecuteNonQuery(subquery);
					}
				}
			}
			catch (SqlException ex)
			{
				outputMsg = string.Format(CultureInfo.InvariantCulture, "Query executed with errors.\r\n{0}", ex.Message);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				outputMsg = ex.Message;
			}
			finally
			{
				connection.RollbackTransaction();
			}

			return outputMsg;
		}

		public static void TestQueryForAllEdwTables()
		{
			BiLogger.StartSubtask("Testing generated queries for EDW tables");

			using (Db.DisableSchemaVersionCheck())
			using (var adminConnectionForTest = Db.NewAdminConnection(BiDatabase.ServerName, Db.SqlMasterDb))
			{
				if (!edwDatabaseCreated)
				{
					BiDatabase.CreateEdwDatabase(adminConnectionForTest);
					edwDatabaseCreated = true;
				}

				using (((ICurrentDbControl)adminConnectionForTest).UseDatabase(BiDatabase.EdwDatabaseName))
				{
					var resultMsgList = new List<string>();

					var excludedEdwTables = new List<string>();
					if (Globals.IsTest)
					{
						// Exclude EDI base tables
						excludedEdwTables.AddRange(new List<string> { "BAS__LicenceCompany", "BAS__LicenceDatabase", "BAS__IncidentMain", "BAS__ReleaseBuild", "BAS__LicenceHeader" });
					}

					foreach (var edwTable in BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwTableConfig.Where(t => !excludedEdwTables.Contains(t.Name)))
					{
						var source = string.Format(CultureInfo.InvariantCulture, "[{0}].[{1}]", edwTable.Schema, edwTable.Name);
						CheckForQueryErrorsWithConnection(adminConnectionForTest, source, BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.GetInitialLoadQueryForEdwTableTesting(edwTable), "Initial Load", resultMsgList);
						CheckForQueryErrorsWithConnection(adminConnectionForTest, source, BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.GetIncrementalInsertQueryForEdwTableTesting(edwTable), "Incremental Insert", resultMsgList);
						CheckForQueryErrorsWithConnection(adminConnectionForTest, source, BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.GetIncrementalDeleteQueryForEdwTable(edwTable), "Incremental Delete", resultMsgList);
					}

					foreach (var denormTable in BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwDenormalizedTableConfig)
					{
						var source = string.Format(CultureInfo.InvariantCulture, "[{0}].[{1}]", denormTable.Schema, denormTable.Name);
						CheckForQueryErrorsWithConnection(adminConnectionForTest, source, BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.GetDropAndCreateDenormalizedTableViewQuery(denormTable), "Create View", resultMsgList);
						CheckForQueryErrorsWithConnection(adminConnectionForTest, source, BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.GetDropAndCreateInitialLoadQueryForDenormalizedTable(denormTable), "Initial Load", resultMsgList);
						CheckForQueryErrorsWithConnection(adminConnectionForTest, source, BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.GetDropAndCreateIncrementalLoadQueryForDenormalizedTable(denormTable), "Incremental Load", resultMsgList);
					}

					foreach (var customTable in BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwCustomTableConfig)
					{
						var source = string.Format(CultureInfo.InvariantCulture, "[{0}].[{1}]", customTable.Schema, customTable.Name);
						if (!string.IsNullOrWhiteSpace(customTable.CreateViewQuery))
						{
							var query = string.Format(CultureInfo.InvariantCulture,
		@"IF EXISTS
	(SELECT NULL FROM sys.views v INNER JOIN sys.schemas s ON v.schema_id = s.schema_id
		WHERE v.name = '{0}' AND s.name = '{1}')
	DROP VIEW [{1}].[{0}]

GO

{2}

GO", customTable.ViewName, customTable.Schema, customTable.CreateViewQuery);
							CheckForQueryErrorsWithConnection(adminConnectionForTest, source, query, "Create View", resultMsgList);
						}
						if (!string.IsNullOrWhiteSpace(customTable.InitialLoadQuery))
						{
							CheckForQueryErrorsWithConnection(adminConnectionForTest, source, customTable.InitialLoadQuery, "Initial Load", resultMsgList);
						}
						if (!string.IsNullOrWhiteSpace(customTable.IncrementalLoadQuery))
						{
							CheckForQueryErrorsWithConnection(adminConnectionForTest, source, customTable.IncrementalLoadQuery, "Incremental Load", resultMsgList);
						}
					}

					foreach (var modelView in BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwModelViewTableConfig)
					{
						var source = string.Format(CultureInfo.InvariantCulture, "[{0}].[{1}]", modelView.Schema, modelView.Name);
						CheckForQueryErrorsWithConnection(adminConnectionForTest, source, BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.GetDropAndCreateModelViewQuery(modelView), "Create View", resultMsgList);
					}

					foreach (var ssasTable in BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.SsasTables)
					{
						var ssasCube = ssasTable.GetSsasCube();
						var source = string.Format(CultureInfo.InvariantCulture, "[{0}].[{1}]", ssasCube.SsasModelLogicalName, ssasTable.TableName);
						if (!ssasTable.IsCalculated)
						{
							CheckForQueryErrorsWithConnection(adminConnectionForTest, source, ssasTable.Query, "Partition Query", resultMsgList);
						}
					}

					if (!resultMsgList.Any())
					{
						BiLogger.StartSubtask("All queries executed successfully.");
						BiLogger.Complete();
					}
					else
					{
						var outputMsg = string.Join("\r\n", resultMsgList);
						throw new BiDatabaseSyncException(string.Format(CultureInfo.InvariantCulture, "EDW transformation query execution failed. Check BI config files for incompatible configuration, or contact any of the Business Intelligence team.\r\n{0}", outputMsg));
					}
				}
				BiDatabase.DropEdwDatabases(adminConnectionForTest);
				edwDatabaseCreated = false;
			}
		}

		static void CheckForQueryErrorsWithConnection(AdminConnection connection, string source, string query, string operation, List<string> resultMsgList)
		{
			string resultMsg = TestQueryUnsafe(source, query, operation, connection);
			if (!string.IsNullOrEmpty(resultMsg))
			{
				resultMsgList.Add(resultMsg);
			}
		}

		#endregion

		#region Populate Columns

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public static void PopulateEdwColumns(BiAutomationConfigDataSet.EdwTableConfigRow edwTable)
		{
			try
			{
				var stagingTable = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.CdcTableConfig
					.Where(t => new Regex(string.Format(CultureInfo.InvariantCulture, @"\b{0}\b", t.SourceTable), RegexOptions.IgnoreCase).Match(edwTable.StagingTable).Success)
					.FirstOrDefault();

				if (stagingTable != null)
				{
					var cdcColumns = stagingTable.GetCdcColumnConfigRows();
					CheckReferenceTables(edwTable, cdcColumns);
					CreateEdwKeyColumns(edwTable, cdcColumns);

					var excludedDataTypes = new[] { "xml" };

					foreach (var column in cdcColumns.Where(c => !c.IsPrimaryKey &&
						!excludedDataTypes.Contains(c.DataType) &&
						!BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwColumnConfig.GetDependentEdwColumns(c).Any()))
					{
						if (!string.IsNullOrEmpty(column.ReferenceTable))
						{
							var parentTable = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwTableConfig.Where(t => new Regex(string.Format(CultureInfo.InvariantCulture, @"\b{0}\b", t.StagingTable), RegexOptions.IgnoreCase).Match(column.ReferenceTable).Success).FirstOrDefault();
							var parentColumn = parentTable.GetEdwColumnConfigRows().Where(c => c.Name == parentTable.BaseName + "ID").FirstOrDefault();

							var columnName = string.Format(CultureInfo.InvariantCulture, "{0}Key", parentTable.BaseName);
							int counter = 2;
							while (edwTable.GetEdwColumnConfigRows().Any(c => c.Name == columnName))
							{
								columnName = string.Format(CultureInfo.InvariantCulture, "{0}Key{1}", parentTable.BaseName, counter++);
							}

							BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwColumnConfig.AddEdwColumnConfigRow(edwTable.Name, edwTable.TransformId, "bigint", 16, 0, 0, columnName, column.SourceColumn, false, false, "", parentTable.Name, parentColumn.Name, "", "", "", false);
						}
						else
						{
							BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwColumnConfig.AddEdwColumnConfigRow(edwTable.Name, edwTable.TransformId, column.DataType, column.MaxLength, column.Precision, column.Scale, column.SourceColumn, column.SourceColumn, false, false, "", "", "", "", "", "", false);
						}
					}
				}
				else
				{
					throw new BiDatabaseSyncException(string.Format(CultureInfo.InvariantCulture, "Cannot find staging table [{0}].", edwTable.StagingTable));
				}
			}
			catch (BiDatabaseSyncException ex)
			{
				BiLogger.Fail(ex.Message);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				BiLogger.Fail(ex);
			}
		}

		static void CheckReferenceTables(BiAutomationConfigDataSet.EdwTableConfigRow edwTable, BiAutomationConfigDataSet.CdcColumnConfigRow[] cdcColumns)
		{
			var referenceTables = cdcColumns.Where(c => !string.IsNullOrEmpty(c.ReferenceTable)).Select(c => c.ReferenceTable).Distinct();
			var stagingTablesAvailable = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwTableConfig.Select(t => t.StagingTable);
			foreach (var referenceTable in referenceTables)
			{
				if (referenceTable != edwTable.StagingTable && !stagingTablesAvailable.Contains(referenceTable))
				{
					throw new BiDatabaseSyncException(string.Format(CultureInfo.InvariantCulture, "Source table has a reference to [{0}], which does not have a base EDW table yet. Create this table and re-populate columns.", referenceTable));
				}
			}
		}

		static void CreateEdwKeyColumns(BiAutomationConfigDataSet.EdwTableConfigRow edwTable, BiAutomationConfigDataSet.CdcColumnConfigRow[] cdcColumns)
		{
			var pkColumn = cdcColumns.Where(c => c.IsPrimaryKey).Select(c => c.SourceColumn).FirstOrDefault();
			var edwColumns = edwTable.GetEdwColumnConfigRows();

			var keyName = string.Format(CultureInfo.InvariantCulture, "{0}Key", edwTable.BaseName);
			if (!edwColumns.Any(c => c.Name == keyName))
			{
				BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwColumnConfig.AddEdwColumnConfigRow(edwTable.Name, edwTable.TransformId, "bigint", 0, 0, 0, keyName, "", false, false, "", "", "", "", "", "", false);
			}

			var idName = string.Format(CultureInfo.InvariantCulture, "{0}ID", edwTable.BaseName);
			if (!edwColumns.Any(c => c.Name == idName))
			{
				BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwColumnConfig.AddEdwColumnConfigRow(edwTable.Name, edwTable.TransformId, "uniqueidentifier", 0, 0, 0, idName, pkColumn, false, false, "", "", "", "", "", "", false);
			}
		}

		public static BiAutomationConfigDataSet.EdwTableConfigRow CloneEdwTable(BiAutomationConfigDataSet.EdwTableConfigRow edwTable)
		{
			BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EnforceConstraints = false;

			var newTransformId = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwTableConfig.Where(t => t.Name == edwTable.Name).Max(t => t.TransformId) + 1;
			var cloneEdwTable = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwTableConfig.AddEdwTableConfigRow(edwTable.Schema, edwTable.Name, "", edwTable.SourceSchema, edwTable.StagingTable, edwTable.WhereClause, newTransformId, edwTable.IsPartitioned, edwTable.DependencyOrder, edwTable.CustomIndex);

			foreach (var column in edwTable.GetEdwColumnConfigRows())
			{
				BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwColumnConfig.AddEdwColumnConfigRow(cloneEdwTable.Name, cloneEdwTable.TransformId, column.DataType, column.MaxLength, column.Precision, column.Scale, column.Name, column.Expression, column.IsPartitionKey, column.UsesFunction, "", column.ParentTable, column.ParentColumn, column.TargetColumn, "", column.KeepAs, column.EnableIndex);
			}

			BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EnforceConstraints = true;

			return cloneEdwTable;
		}

		public static void PopulateDenormColumns(BiAutomationConfigDataSet.EdwDenormalizedTableConfigRow denormTable)
		{
			try
			{
				var denormColumns = denormTable.GetEdwDenormalizedColumnConfigRows();
				var sourceTables = ParseSourceTables(denormTable.Expression);

				foreach (var sourceTable in sourceTables)
				{
					if (!BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwTableConfig.Any(t => DoesTableMatch(sourceTable.Schema, sourceTable.Table, t.Schema, t.Name)) &&
						!BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwDenormalizedTableConfig.Any(t => DoesTableMatch(sourceTable.Schema, sourceTable.Table, t.Schema, t.Name)))
					{
						throw new BiDatabaseSyncException(string.Format(CultureInfo.InvariantCulture, "Cannot find EDW table [{0}].[{1}].", sourceTable.Schema, sourceTable.Table));
					}
				}

				var keyName = string.Format(CultureInfo.InvariantCulture, "{0}Key", denormTable.BaseName);
				if (!denormColumns.Any(c => c.Name == keyName))
				{
					BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwDenormalizedColumnConfig.AddEdwDenormalizedColumnConfigRow(denormTable, keyName, "bigint", 0, 0, 0, "", false, "", false);
				}

				foreach (var sourceTable in sourceTables)
				{
					if (!PopulateDenormTableWithBaseSourceTable(denormTable, sourceTable))
					{
						PopulateDenormTableWithDenormSourceTable(denormTable, sourceTable);
					}
				}
			}
			catch (BiDatabaseSyncException ex)
			{
				BiLogger.Fail(ex.Message);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				BiLogger.Fail(ex);
			}
		}

		static bool PopulateDenormTableWithBaseSourceTable(BiAutomationConfigDataSet.EdwDenormalizedTableConfigRow denormTable, SourceTableDefinition sourceTable)
		{
			bool result = false;
			var baseTable = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwTableConfig.Where(t => DoesTableMatch(sourceTable.Schema, sourceTable.Table, t.Schema, t.Name)).FirstOrDefault();
			if (baseTable != null)
			{
				result = true;
				var edwColumns = baseTable.GetEdwColumnConfigRows();

				foreach (var column in edwColumns.Where(c => c.Name != baseTable.BaseName + "ID"))
				{
					var columnName = column.Name;

					if (columnName != sourceTable.Table.Replace(BiConstants.EdwBaseTablePrefix, "") + "Key")
					{
						columnName = sourceTable.Table.Replace(BiConstants.EdwBaseTablePrefix, "") + column.Name;
					}
					var expression = column.Name;
					if (!string.IsNullOrEmpty(sourceTable.Alias))
					{
						expression = string.Format(CultureInfo.InvariantCulture, "[{0}].[{1}]", sourceTable.Alias, column.Name);
					}
					BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwDenormalizedColumnConfig.AddEdwDenormalizedColumnConfigRow(denormTable, columnName, column.DataType, column.MaxLength, column.Precision, column.Scale, expression, false, "", column.EnableIndex);
				}
			}
			return result;
		}

		static void PopulateDenormTableWithDenormSourceTable(BiAutomationConfigDataSet.EdwDenormalizedTableConfigRow denormTable, SourceTableDefinition sourceTable)
		{
			var aggTable = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwDenormalizedTableConfig.Where(t => DoesTableMatch(sourceTable.Schema, sourceTable.Table, t.Schema, t.Name)).FirstOrDefault();
			if (aggTable != null)
			{
				foreach (var column in aggTable.GetEdwDenormalizedColumnConfigRows())
				{
					var columnName = column.Name;
					if (columnName != sourceTable.Table.Replace(BiConstants.EdwAggregateTablePrefix, "") + "Key")
					{
						columnName = sourceTable.Table.Replace(BiConstants.EdwAggregateTablePrefix, "") + column.Name;
					}

					var expression = column.Name;
					if (!string.IsNullOrEmpty(sourceTable.Alias))
					{
						expression = string.Format(CultureInfo.InvariantCulture, "[{0}].[{1}]", sourceTable.Alias, column.Name);
					}
					BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwDenormalizedColumnConfig.AddEdwDenormalizedColumnConfigRow(denormTable, columnName, column.DataType, column.MaxLength, column.Precision, column.Scale, expression, false, "", column.EnableIndex);
				}
			}
		}

		static bool DoesTableMatch(string sourceSchema, string sourceTable, string targetSchema, string targetTable)
		{
			return sourceSchema.Equals(targetSchema, StringComparison.OrdinalIgnoreCase) &&
				sourceTable.Equals(targetTable, StringComparison.OrdinalIgnoreCase);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public static void PopulateModelViewColumns(BiAutomationConfigDataSet.EdwModelViewTableConfigRow modelView)
		{
			try
			{
				var sourceTables = ParseSourceTables(modelView.Expression);
				if (sourceTables.Count() > 1)
				{
					throw new BiDatabaseSyncException("Cannot populate from multiple source tables.");
				}

				var sourceTable = sourceTables.FirstOrDefault();
				if (sourceTable != null)
				{
					if (sourceTable.Table.StartsWith(BiConstants.EdwBaseTablePrefix, StringComparison.OrdinalIgnoreCase))
					{
						PopulateModelViewWithBaseTable(modelView, sourceTable);
					}
					else if (sourceTable.Table.StartsWith(BiConstants.EdwAggregateTablePrefix, StringComparison.OrdinalIgnoreCase))
					{
						PopulateModelViewWithDenormTable(modelView, sourceTable);
					}
					else if (BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwCustomTableConfig.Any(t => t.Name == sourceTable.Table))
					{
						PopulateModelViewWithCustomTable(modelView, sourceTable);
					}
					else
					{
						throw new BiDatabaseSyncException(string.Format(CultureInfo.InvariantCulture, "Cannot find EDW table [{0}].[{1}].", sourceTable.Schema, sourceTable.Table));
					}
				}
			}
			catch (BiDatabaseSyncException ex)
			{
				BiLogger.Fail(ex.Message);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				BiLogger.Fail(ex);
			}
		}

		static void PopulateModelViewWithBaseTable(BiAutomationConfigDataSet.EdwModelViewTableConfigRow modelView, SourceTableDefinition sourceTable)
		{
			var baseTable = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwTableConfig
				.Where(t =>
					new Regex(string.Format(CultureInfo.InvariantCulture, @"\b{0}\b", t.Schema), RegexOptions.IgnoreCase).Match(sourceTable.Schema).Success &&
					new Regex(string.Format(CultureInfo.InvariantCulture, @"\b{0}\b", t.Name), RegexOptions.IgnoreCase).Match(sourceTable.Table).Success)
				.FirstOrDefault();
			if (baseTable != null)
			{
				var edwColumns = baseTable.GetEdwColumnConfigRows();

				foreach (var column in edwColumns.Where(c => c.Name != baseTable.BaseName + "ID" &&
					!BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwModelViewColumnConfig.GetDependentModelViewColumns(c).Any()))
				{
					var expression = column.Name;
					if (!string.IsNullOrEmpty(sourceTable.Alias))
					{
						expression = string.Format(CultureInfo.InvariantCulture, "[{0}].[{1}]", sourceTable.Alias, column.Name);
					}
					BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwModelViewColumnConfig.AddEdwModelViewColumnConfigRow(modelView, column.Name, expression, "", "");
				}
			}
		}

		static void PopulateModelViewWithDenormTable(BiAutomationConfigDataSet.EdwModelViewTableConfigRow modelView, SourceTableDefinition sourceTable)
		{
			var denormTable = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwDenormalizedTableConfig
				.Where(t =>
					new Regex(string.Format(CultureInfo.InvariantCulture, @"\b{0}\b", t.Schema), RegexOptions.IgnoreCase).Match(sourceTable.Schema).Success &&
					new Regex(string.Format(CultureInfo.InvariantCulture, @"\b{0}\b", t.Name), RegexOptions.IgnoreCase).Match(sourceTable.Table).Success)
				.FirstOrDefault();
			if (denormTable != null)
			{
				var denormColumns = denormTable.GetEdwDenormalizedColumnConfigRows();

				foreach (var column in denormColumns.Where(c =>
					!BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwModelViewColumnConfig.GetDependentModelViewColumns(c).Any()))
				{
					var expression = column.Name;
					if (!string.IsNullOrEmpty(sourceTable.Alias))
					{
						expression = string.Format(CultureInfo.InvariantCulture, "[{0}].[{1}]", sourceTable.Alias, column.Name);
					}
					BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwModelViewColumnConfig.AddEdwModelViewColumnConfigRow(modelView, column.Name, expression, "", "");
				}
			}
		}

		static void PopulateModelViewWithCustomTable(BiAutomationConfigDataSet.EdwModelViewTableConfigRow modelView, SourceTableDefinition sourceTable)
		{
			var customTable = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwCustomTableConfig
				.Where(t =>
					new Regex(string.Format(CultureInfo.InvariantCulture, @"\b{0}\b", t.Schema), RegexOptions.IgnoreCase).Match(sourceTable.Schema).Success &&
					new Regex(string.Format(CultureInfo.InvariantCulture, @"\b{0}\b", t.Name), RegexOptions.IgnoreCase).Match(sourceTable.Table).Success)
				.FirstOrDefault();
			if (customTable != null)
			{
				var edwCustomColumns = customTable.GetEdwCustomColumnConfigRows();

				foreach (var column in edwCustomColumns)
				{
					var expression = column.Name;
					if (!string.IsNullOrEmpty(sourceTable.Alias))
					{
						expression = string.Format(CultureInfo.InvariantCulture, "[{0}].[{1}]", sourceTable.Alias, column.Name);
					}
					BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwModelViewColumnConfig.AddEdwModelViewColumnConfigRow(modelView, column.Name, expression, "", "");
				}
			}
		}

		static IEnumerable<SourceTableDefinition> ParseSourceTables(string expression)
		{
			var result = new List<SourceTableDefinition>();

			if (!string.IsNullOrEmpty(expression))
			{
				var regex = new Regex(@"(^|JOIN\s+)(\[*(?<Schema>\b.+?\b)\]*)\.(\[*(?<Table>\b.+?\b)\]*)($|\s+(AS\s+)*(\[*(?<Alias>\b.+?\b)\]*))", RegexOptions.IgnoreCase);
				foreach (Match match in regex.Matches(expression))
				{
					var schema = match.Groups["Schema"].Value;
					var table = match.Groups["Table"].Value;
					var alias = match.Groups["Alias"].Value;

					if (!result.Any(d => d.Schema == schema && d.Table == table))
					{
						result.Add(new SourceTableDefinition(schema, table, alias));
					}
				}
			}

			return result;
		}

		class SourceTableDefinition
		{
			public SourceTableDefinition(string schema, string table, string alias)
			{
				Schema = schema;
				Table = table;
				Alias = alias;
			}

			public string Schema { get; }
			public string Table { get; }
			public string Alias { get; }
		}

		#endregion

		#region Load Model View Columns

		public static DataTable LoadModelViewColumns(string modelViewName)
		{
			var modelView = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwModelViewTableConfig.FirstOrDefault(mv => mv.Name == modelViewName);

			if (modelView != null)
			{
				using (Db.DisableSchemaVersionCheck())
				using (var connection = Db.NewAdminConnection(BiDatabase.ServerName, Db.SqlMasterDb))
				{
					if (!edwDatabaseCreated)
					{
						BiDatabase.CreateEdwDatabase(connection);
						edwDatabaseCreated = true;
					}

					using (((ICurrentDbControl)connection).UseDatabase(BiDatabase.EdwDatabaseName))
					{
						var dataTable = DataUtils.GetDataTableFromQuery(connection, $"SELECT * FROM [{modelView.Schema}].[{modelView.Name}]");
						dataTable.TableName = modelViewName;
						return dataTable;
					}
				}
			}
			else
			{
				return null;
			}
		}

		#endregion

		#endregion

		class Definition
		{
			public Definition(string sourceSchema, string sourceTable)
			{
				SourceSchema = sourceSchema;
				SourceTable = sourceTable;
			}

			public override bool Equals(object obj)
			{
				var defObj = (Definition)obj;
				return SourceSchema == defObj.SourceSchema && SourceTable == defObj.SourceTable;
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("Decruftification", "WTG3007:Overrides should not simply call base.", Justification = "Required because Equals is overridden")]
			public override int GetHashCode()
			{
				return base.GetHashCode();
			}

			public string SourceSchema;
			public string SourceTable;
		}
	}
}
