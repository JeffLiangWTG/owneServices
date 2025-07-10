using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Bi.Common;
using CargoWise.Common;

[assembly: WTG.StaticAnalysis.Annotation.UsesConstants(typeof(BiConstants))]

namespace CargoWise.Bi.Configuration.DataSets
{
	#region SuppressResourceStringsCheckRegion

	public class SourceTableDefinition
	{
		public SourceTableDefinition(string schema, string table, string alias)
		{
			Schema = schema;
			Table = table;
			Alias = alias;
		}

		public SourceTableDefinition(string schema, string table, string alias, string filter)
			: this(schema, table, alias)
		{
			Filter = filter;
		}

		public string Schema { get; }
		public string Table { get; }
		public string Alias { get; }
		public string Filter { get; }
	}

	[System.CodeDom.Compiler.GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
	public partial class BiAutomationConfigDataSet
	{
		public static IEnumerable<SourceTableDefinition> ParseSourceTables(string expression)
		{
			var result = new List<SourceTableDefinition>();

			if (!string.IsNullOrEmpty(expression))
			{
				foreach (Match match in sourceTableRegex.Matches(expression))
				{
					var schema = match.Groups["Schema"].Value;
					var table = match.Groups["Table"].Value;
					var alias = match.Groups["Alias"].Value;

					if (!schema.Equals(BiConstants.BiAdminSchemaName) && !schema.Contains(" "))
					{
						result.Add(new SourceTableDefinition(schema, table, alias));
					}
				}
			}

			return result;
		}

		static readonly Regex sourceTableRegex = new Regex(@"(^|JOIN\s+)(\[*(?<Schema>\b.+?\b)\]*)\.(\[*(?<Table>\b.+?\b)\]*)($|\s*\n|\s+(AS\s+)*(\[*(?<Alias>\b.+?\b)\]*))", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		#region Validation

		public void Validate()
		{
			var errorMessages = new StringBuilder();

			try
			{
				SortModelDependencyOrder();
			}
			catch (BiConfigurationException ex)
			{
				errorMessages.AppendLine(ex.Message);
			}

			ValidateCdcConfiguration(errorMessages);
			ValidateEdwTables(errorMessages);
			ValidateEdwDenormalizedTables(errorMessages);
			ValidateEdwCustomTables(errorMessages);
			ValidateEdwModelViews(errorMessages);
			ValidateSsasConfiguration(errorMessages);
			ValidateTabularConfiguration(errorMessages);
			ValidateReportConfiguration(errorMessages);

			if (errorMessages.Length > 0)
			{
				throw new BiConfigurationException("One or more errors have been detected in the configuration data.\r\n" + errorMessages.ToString().TrimEnd());
			}
		}

		void ValidateCdcConfiguration(StringBuilder errorMessages)
		{
			if (CdcTableConfig.Any())
			{
				var tableNonDeleted = CdcTableConfig.Where(cdcTableConfig => cdcTableConfig.Action != "Deleted");
				foreach (CdcTableConfigRow cdcTableConfigRow in tableNonDeleted)
				{
					cdcTableConfigRow.Validate();
					if (cdcTableConfigRow.HasErrors)
					{
						errorMessages.AppendLine(String.Format(CultureInfo.InvariantCulture, "Table: {0}", cdcTableConfigRow.SourceTable));
						errorMessages.AppendLine(cdcTableConfigRow.RowError);
					}

					var column = cdcTableConfigRow.GetCdcColumnConfigRows();
					var columnNotDeleted = column.Where(cdcColumnConfig => cdcColumnConfig.Action != "Deleted");
					foreach (CdcColumnConfigRow cdcColumnConfigRow in columnNotDeleted)
					{
						cdcColumnConfigRow.Validate();
						if (cdcColumnConfigRow.HasErrors)
						{
							errorMessages.AppendLine(String.Format(CultureInfo.InvariantCulture, "Table: {0}, Column: {1}", cdcTableConfigRow.SourceTable, cdcColumnConfigRow.SourceColumn));
							errorMessages.AppendLine(cdcColumnConfigRow.RowError);
						}
					}
				}
			}
		}

		void ValidateEdwTables(StringBuilder errorMessages)
		{
			if (EdwTableConfig.Any())
			{
				EdwTableConfig.SuspendValidation();
				EdwColumnConfig.SuspendValidation();
				foreach (var edwTable in EdwTableConfig)
				{
					edwTable.Schema = FormatMember(edwTable.Schema);
					edwTable.Name = FormatMember(edwTable.Name);
					edwTable.SourceSchema = FormatMember(edwTable.SourceSchema);
					edwTable.StagingTable = FormatMember(edwTable.StagingTable);
					edwTable.SetIsPartitioned();

					edwTable.Validate();

					if (edwTable.HasErrors)
					{
						errorMessages.AppendLine(String.Format(CultureInfo.InvariantCulture, "Model Table: {0}", edwTable.Name));
						errorMessages.AppendLine(edwTable.RowError);
					}

					foreach (var edwColumn in edwTable.GetEdwColumnConfigRows())
					{
						edwColumn.Name = FormatMember(edwColumn.Name);
						edwColumn.Expression = FormatMember(edwColumn.Expression);
						edwColumn.ParentTable = FormatMember(edwColumn.ParentTable);
						edwColumn.ParentColumn = FormatMember(edwColumn.ParentColumn);
						edwColumn.Condition = FormatMember(edwColumn.Condition);

						edwColumn.Validate();

						if (edwColumn.HasErrors)
						{
							errorMessages.AppendLine(String.Format(CultureInfo.InvariantCulture, "Model Table: {0}, Model Column: {1}", edwTable.Name, edwColumn.Name));
							errorMessages.AppendLine(edwColumn.RowError);
						}
					}
				}

				EdwTableConfig.ResumeValidation();
				EdwColumnConfig.ResumeValidation();
			}
		}

		void ValidateEdwDenormalizedTables(StringBuilder errorMessages)
		{
			if (EdwDenormalizedTableConfig.Any())
			{
				EdwDenormalizedTableConfig.SuspendValidation();
				EdwDenormalizedColumnConfig.SuspendValidation();

				foreach (var denormTable in EdwDenormalizedTableConfig)
				{
					denormTable.Schema = FormatMember(denormTable.Schema);
					denormTable.Name = FormatMember(denormTable.Name);
					denormTable.Expression = FormatMember(denormTable.Expression);
					denormTable.SetIsPartitioned();

					denormTable.Validate();

					if (denormTable.HasErrors)
					{
						errorMessages.AppendLine(String.Format(CultureInfo.InvariantCulture, "Denormalized Model Table: {0}", denormTable.Name));
						errorMessages.AppendLine(denormTable.RowError);
					}

					foreach (var column in denormTable.GetEdwDenormalizedColumnConfigRows())
					{
						column.Name = FormatMember(column.Name);
						column.Expression = FormatMember(column.Expression);

						column.Validate();

						if (column.HasErrors)
						{
							errorMessages.AppendLine(String.Format(CultureInfo.InvariantCulture, "Denormalized Model Table: {0}, Column: {1}", denormTable.Name, column.Name));
							errorMessages.AppendLine(column.RowError);
						}
					}
				}

				EdwDenormalizedTableConfig.ResumeValidation();
				EdwDenormalizedColumnConfig.ResumeValidation();
			}
		}

		void ValidateEdwCustomTables(StringBuilder errorMessages)
		{
			if (EdwCustomTableConfig.Any())
			{
				EdwCustomTableConfig.SuspendValidation();
				EdwCustomColumnConfig.SuspendValidation();

				foreach (var customTable in EdwCustomTableConfig)
				{
					customTable.Schema = FormatMember(customTable.Schema);
					customTable.Name = FormatMember(customTable.Name);

					customTable.Validate();

					if (customTable.HasErrors)
					{
						errorMessages.AppendLine(String.Format(CultureInfo.InvariantCulture, "Custom Model Table: {0}", customTable.Name));
						errorMessages.AppendLine(customTable.RowError);
					}

					int numOfColumns = customTable.GetEdwCustomColumnConfigRows().Length;
					if (numOfColumns == 0)
					{
						errorMessages.AppendLine(String.Format(CultureInfo.InvariantCulture, "Custom Model Table: {0}", customTable.Name));
						errorMessages.AppendLine("Custom table cannot be created without any columns.");
					}

					foreach (var column in customTable.GetEdwCustomColumnConfigRows())
					{
						column.Name = FormatMember(column.Name);

						column.Validate();

						if (column.HasErrors)
						{
							errorMessages.AppendLine(String.Format(CultureInfo.InvariantCulture, "Custom Model Table: {0}, Column: {1}", customTable.Name, column.Name));
							errorMessages.AppendLine(column.RowError);
						}
					}
				}

				EdwCustomTableConfig.ResumeValidation();
				EdwCustomColumnConfig.ResumeValidation();
			}
		}

		void ValidateEdwModelViews(StringBuilder errorMessages)
		{
			if (EdwModelViewTableConfig.Any())
			{
				EdwModelViewTableConfig.SuspendValidation();
				EdwModelViewColumnConfig.SuspendValidation();

				foreach (var viewTable in EdwModelViewTableConfig)
				{
					viewTable.Schema = FormatMember(viewTable.Schema);
					viewTable.Name = FormatMember(viewTable.Name);
					viewTable.Comment = FormatMember(viewTable.Comment);
					viewTable.Expression = FormatMember(viewTable.Expression);

					viewTable.Validate();

					if (viewTable.HasErrors)
					{
						errorMessages.AppendLine(String.Format(CultureInfo.InvariantCulture, "Model View: {0}", viewTable.Name));
						errorMessages.AppendLine(viewTable.RowError);
					}

					foreach (var viewColumn in viewTable.GetEdwModelViewColumnConfigRows())
					{
						viewColumn.Name = FormatMember(viewColumn.Name);
						viewColumn.Comment = FormatMember(viewColumn.Comment);
						viewColumn.Expression = FormatMember(viewColumn.Expression);

						viewColumn.Validate();

						if (viewColumn.HasErrors)
						{
							errorMessages.AppendLine(String.Format(CultureInfo.InvariantCulture, "Model View: {0}, Column: {1}", viewTable.Name, viewColumn.Name));
							errorMessages.AppendLine(viewColumn.RowError);
						}
					}
				}

				EdwModelViewTableConfig.ResumeValidation();
				EdwModelViewColumnConfig.ResumeValidation();
			}
		}

		void ValidateSsasConfiguration(StringBuilder errorMessages)
		{
			if (SsasTables.Any())
			{
				SsasTables.SuspendValidation();

				foreach (var ssasTable in SsasTables)
				{
					ssasTable.Validate();

					if (ssasTable.HasErrors)
					{
						errorMessages.AppendLine(String.Format(CultureInfo.InvariantCulture, "Model Table: {0}", ssasTable.TableName));
						errorMessages.AppendLine(ssasTable.RowError);
					}
				}
				SsasTables.ResumeValidation();
			}
		}

		void ValidateTabularConfiguration(StringBuilder errorMessages)
		{
			if (LinkedTable.Any())
			{
				LinkedTable.SuspendValidation();

				foreach (var linkedTable in LinkedTable)
				{
					linkedTable.Validate();

					if (linkedTable.HasErrors)
					{
						errorMessages.AppendLine(String.Format(CultureInfo.InvariantCulture, "Linked Table: {0}", linkedTable.Name));
						errorMessages.AppendLine(linkedTable.RowError);
					}
				}
				SsasTables.ResumeValidation();
			}
		}

		void ValidateReportConfiguration(StringBuilder errorMessages)
		{
			if (ReportMappingTableConfig.Any())
			{
				ReportMappingTableConfig.SuspendValidation();

				foreach (var mappingTable in ReportMappingTableConfig)
				{
					mappingTable.Validate();

					if (mappingTable.HasErrors)
					{
						errorMessages.AppendLine(String.Format(CultureInfo.InvariantCulture, "Report Mapping Source Schema: {0} Staging Table: {1}", mappingTable.SourceSchema, mappingTable.StagingTable));
						errorMessages.AppendLine(mappingTable.RowError);
					}
				}
				SsasTables.ResumeValidation();
			}
		}

		string FormatMember(string value)
		{
			var result = value;

			if (!string.IsNullOrEmpty(value))
			{
				result = result.Trim('\r', '\n', '\t', ' ');
				var spaceRegex = new Regex("( )+", RegexOptions.IgnoreCase);
				result = spaceRegex.Replace(result, " ");
			}

			return result;
		}

		#endregion

		#region EDW Model Table Dependency Order

		public void SortModelDependencyOrder()
		{
			SortBaseTableDependencyOrder();
			SortAggregateTableDependencyOrder();
			SortCustomTableDependencyOrder();
		}

		#region Base Table

		public void SortBaseTableDependencyOrder()
		{
			ResetBaseTableDependencyOrder();
			int dependencyOrderId = 1;

			var baseTablesWithUnsortedDependenciesCount = EdwTableConfig.Count(t => t.DependencyOrder == -1);
			while (baseTablesWithUnsortedDependenciesCount > 0)
			{
				var independentTables = EdwTableConfig.Where(t => t.DependencyOrder == -1 && !BaseTableHasColumnsWithUnsortedDependencies(t.Name, t.Name)).Select(t => t.Name).OrderBy(t => t).Distinct();
				foreach (var tableName in independentTables)
				{
					var dependencyOrder = dependencyOrderId++;
					foreach (var edwTable in EdwTableConfig.Where(t => t.Name == tableName))
					{
						edwTable.DependencyOrder = dependencyOrder;
					}
				}

				var newCount = EdwTableConfig.Count(t => t.DependencyOrder == -1);
				if (newCount == baseTablesWithUnsortedDependenciesCount && newCount > 0)
				{
					throw new BiConfigurationException("Circular dependencies detected. Check the parent tables to resolve the issue.");
				}
				else
				{
					baseTablesWithUnsortedDependenciesCount = newCount;
				}
			}
		}

		void ResetBaseTableDependencyOrder()
		{
			foreach (var baseTable in EdwTableConfig)
			{
				baseTable.DependencyOrder = -1;
			}
		}

		bool BaseTableHasColumnsWithUnsortedDependencies(string tableName, string parentTableName)
		{
			var result = false;
			foreach (var refTable in GetReferenceBaseTables(tableName))
			{
				if (refTable.Name == parentTableName)
				{
					var sourceTable = EdwTableConfig.Where(t => t.Name == parentTableName).FirstOrDefault();
					if (sourceTable != null)
					{
						throw new BiConfigurationException(String.Format(CultureInfo.InvariantCulture, "[{0}] and [{1}] have circular dependencies. Check the parent tables to resolve the issue.", sourceTable.Name, tableName));
					}
				}
				else if (refTable.DependencyOrder == -1 || BaseTableHasColumnsWithUnsortedDependencies(refTable.Name, parentTableName))
				{
					result = true;
					break;
				}
			}
			return result;
		}

		IEnumerable<EdwTableConfigRow> GetReferenceBaseTables(string tableName)
		{
			var result = new List<EdwTableConfigRow>();

			foreach (var edwTable in EdwTableConfig.Where(t => t.Name == tableName))
			{
				var edwColumns = edwTable.GetEdwColumnConfigRows().Where(c => !string.IsNullOrEmpty(c.ParentColumn) && c.ParentTable != tableName);

				foreach (var edwColumn in edwColumns)
				{
					var refTable = EdwTableConfig.Where(et => et.Name == edwColumn.ParentTable).FirstOrDefault();
					if (refTable != null)
					{
						if (!result.Any(t => t.Name == refTable.Name))
						{
							result.Add(refTable);
						}
					}
				}
			}
			return result;
		}

		#endregion

		#region Aggregate Table

		public void SortAggregateTableDependencyOrder()
		{
			ResetAggregateTableDependencyOrder();
			int dependencyOrderId = 1;
			while (DenormalizedTablesHaveUnsortedDependencies())
			{
				var independentTables = EdwDenormalizedTableConfig.Where(t => t.DependencyOrder == -1 && !DenormalizedTableHasColumnsWithUnsortedDependencies(t, t.Name)).OrderBy(t => t.Name).Distinct();
				foreach (var tableName in independentTables)
				{
					foreach (var edwTable in independentTables)
					{
						edwTable.DependencyOrder = dependencyOrderId++;
					}
				}
			}
		}

		void ResetAggregateTableDependencyOrder()
		{
			foreach (var aggTable in EdwDenormalizedTableConfig)
			{
				aggTable.DependencyOrder = -1;
			}
		}

		bool DenormalizedTablesHaveUnsortedDependencies()
		{
			return EdwDenormalizedTableConfig.Any(t => t.DependencyOrder == -1);
		}

		bool DenormalizedTableHasColumnsWithUnsortedDependencies(EdwDenormalizedTableConfigRow aggTable, string parentTableName)
		{
			var result = false;
			foreach (var refTable in GetReferenceDenormalizedTables(aggTable))
			{
				if (refTable.Name == parentTableName)
				{
					var sourceTable = EdwDenormalizedTableConfig.Where(t => t.Name == parentTableName).FirstOrDefault();
					if (sourceTable != null)
					{
						throw new BiConfigurationException(String.Format(CultureInfo.InvariantCulture, "[{0}] and [{1}] have circular dependencies. Check the parent tables to resolve the issue.", sourceTable.Name, aggTable));
					}
				}
				else if (refTable.DependencyOrder == -1 || DenormalizedTableHasColumnsWithUnsortedDependencies(refTable, parentTableName))
				{
					result = true;
					break;
				}
			}
			return result;
		}

		IEnumerable<EdwDenormalizedTableConfigRow> GetReferenceDenormalizedTables(EdwDenormalizedTableConfigRow aggTable)
		{
			var result = new List<EdwDenormalizedTableConfigRow>();

			var sourceTables = ParseSourceTables(aggTable.Expression);

			foreach (var refTable in sourceTables)
			{
				var refAggTable = EdwDenormalizedTableConfig.Where(et => et.Name == refTable.Table && et.Schema == refTable.Schema).FirstOrDefault();
				if (refAggTable != null)
				{
					if (!result.Any(t => t.Name == refAggTable.Name))
					{
						result.Add(refAggTable);
					}
				}
			}
			return result;
		}

		#endregion

		#region Custom Table

		public void SortCustomTableDependencyOrder()
		{
			ResetCustomTableDependencyOrder();
			int dependencyOrderId = 1;
			while (CustomTablesHaveUnsortedDependencies())
			{
				var independentTables = EdwCustomTableConfig.Where(t => t.DependencyOrder == -1 && !CustomTableHasUnsortedDependencies(t, t.Name)).OrderBy(t => t.Name).Distinct();
				foreach (var edwTable in independentTables)
				{
					edwTable.DependencyOrder = dependencyOrderId++;
				}
			}
		}

		void ResetCustomTableDependencyOrder()
		{
			foreach (var customTable in EdwCustomTableConfig.Where(t => t.RunBeforeTransform))
			{
				customTable.DependencyOrder = 0;
			}
			foreach (var customTable in EdwCustomTableConfig.Where(t => !t.RunBeforeTransform))
			{
				customTable.DependencyOrder = -1;
			}
		}

		bool CustomTablesHaveUnsortedDependencies()
		{
			return EdwCustomTableConfig.Any(t => t.DependencyOrder == -1);
		}

		bool CustomTableHasUnsortedDependencies(EdwCustomTableConfigRow customTable, string parentTableName)
		{
			var result = false;

			var dependencyList = customTable.ParseCustomTablesFromQueries();
			foreach (var refTable in dependencyList)
			{
				if (refTable.Name == parentTableName)
				{
					throw new BiConfigurationException(String.Format(CultureInfo.InvariantCulture, "[{0}] and [{1}] have circular dependencies. Check the parent tables to resolve the issue.", parentTableName, customTable.Name));
				}

				if (CustomTableHasUnsortedDependencies(refTable, parentTableName) || refTable.DependencyOrder == -1)
				{
					result = true;
					break;
				}
			}

			return result;
		}

		#endregion

		#endregion

		#region EDW Transform Queries

		public string GetInitialLoadQueryForEdwTableTesting(EdwTableConfigRow edwTable)
		{
			return String.Format(CultureInfo.InvariantCulture,
@"
CREATE TABLE #Keys 
(
		PK_GUID UNIQUEIDENTIFIER NOT NULL, 
		PK_INT BIGINT NOT NULL, 
		TransformID INT NOT NULL,
		PRIMARY KEY CLUSTERED (PK_GUID ASC, TransformID ASC)
)

DECLARE @MaxID BIGINT = 0

{0}",
				GetInitialLoadQueryForEdwTable(edwTable));
		}

		public string GetInitialLoadQueryForEdwTable(EdwTableConfigRow edwTable)
		{
			string sqlText = null;
			if (!edwTable.HasErrors)
			{
				try
				{
					var columnList = edwTable.GetEdwColumnConfigRows().Where(c => !string.IsNullOrEmpty(c.Expression));
					if (columnList.Any())
					{
						var columnQueryList = new List<string>();
						var joinQueryList = new List<string>();
						var crossApplyQueryList = new List<string>();
						var referenceKeyDictionary = new Dictionary<string, string>();
						var stagingTable = CdcTableConfig.Where(t => t.SourceSchema == edwTable.SourceSchema && t.SourceTable == edwTable.StagingTable).FirstOrDefault();
						var pkColumn = stagingTable.GetCdcColumnConfigRows().Where(c => c.IsPrimaryKey).FirstOrDefault().SourceColumn;

						int counter = 1;
						foreach (var edwColumn in columnList)
						{
							if (edwColumn.UsesFunction)
							{
								columnQueryList.Add(String.Format(CultureInfo.InvariantCulture, @"/*{0}*/ [{0}].value AS [{0}]", edwColumn.Name));
								crossApplyQueryList.Add(String.Format(CultureInfo.InvariantCulture, @"CROSS APPLY {0} AS {1}", edwColumn.Expression, edwColumn.Name));
							}
							else if (string.IsNullOrEmpty(edwColumn.ParentColumn) || edwColumn.Expression == "NULL")
							{
								columnQueryList.Add(String.Format(CultureInfo.InvariantCulture, @"/*{0}*/ {1}", edwColumn.Name, AddTableAlias(stagingTable, edwColumn.Expression, "st")));
							}
							else
							{
								var refTableKey = $"Expression:{edwColumn.Expression} ParentTable:{edwColumn.ParentTable} ParentColumn:{edwColumn.ParentColumn}";
								var shouldCreateReference = !referenceKeyDictionary.ContainsKey(refTableKey);
								string refTableCounterName;
								if (shouldCreateReference)
								{
									refTableCounterName = "RefTable" + counter++;
									referenceKeyDictionary.Add(refTableKey, refTableCounterName);
								}
								else
								{
									refTableCounterName = referenceKeyDictionary[refTableKey];
								}

								var referenceTable = EdwTableConfig.Where(t => t.Name == edwColumn.ParentTable).FirstOrDefault();
								string condition = FormatCondition(stagingTable, edwColumn);

								if ((EdwTableConfig.IsSelfReferenced(edwTable)) && (referenceTable != null) && (referenceTable.Name == edwTable.Name))
								{
									columnQueryList.Add(String.Format(CultureInfo.InvariantCulture, @"/*{0}*/ [{1}].[PK_INT]", edwColumn.Name, refTableCounterName));
									if (shouldCreateReference)
									{
										joinQueryList.Add(String.Format(CultureInfo.InvariantCulture, "LEFT JOIN #Keys [{0}] ON st.[{1}] = [{0}].[PK_GUID] AND [{0}].TransformID = {2}",
											refTableCounterName,
											edwColumn.Expression,
											edwTable.TransformId
										));
									}
								}
								else if (referenceTable == null)
								{
									columnQueryList.Add(String.Format(CultureInfo.InvariantCulture, @"/*{0}*/ [{1}].[{2}]", edwColumn.Name, refTableCounterName, edwColumn.TargetColumn));
									if (shouldCreateReference)
									{
										var refCustomTable = EdwCustomTableConfig.Where(t => t.Name == edwColumn.ParentTable).FirstOrDefault();
										joinQueryList.Add(String.Format(CultureInfo.InvariantCulture, "LEFT JOIN [{0}].[{1}] [{2}] ON {3}st.[{4}] = [{2}].[{5}]",
											refCustomTable.Schema,
											refCustomTable.Name,
											refTableCounterName,
											condition,
											edwColumn.Expression,
											edwColumn.ParentColumn
										));
									}
								}
								else
								{
									columnQueryList.Add(String.Format(CultureInfo.InvariantCulture, @"/*{0}*/ [{1}].[{2}]", edwColumn.Name, refTableCounterName, referenceTable.BaseName + "Key"));
									if (shouldCreateReference)
									{
										joinQueryList.Add(String.Format(CultureInfo.InvariantCulture, "LEFT JOIN [{0}].[{1}] [{2}] ON {3}{4} = [{2}].[{5}]",
											referenceTable.Schema,
											referenceTable.Name,
											refTableCounterName,
											condition,
											AddTableAlias(stagingTable, edwColumn.Expression, "st"),
											edwColumn.ParentColumn
										));
									}
								}
							}
						}

						string whereClause = null;
						if (!string.IsNullOrEmpty(edwTable.WhereClause))
						{
							whereClause = edwTable.WhereClause;
							foreach (var cdcColumn in stagingTable.GetCdcColumnConfigRows())
							{
								var regex = new Regex(String.Format(CultureInfo.InvariantCulture, @"\[*\b{0}\b\]*", cdcColumn.SourceColumn), RegexOptions.IgnoreCase);
								whereClause = regex.Replace(whereClause, String.Format(CultureInfo.InvariantCulture, "[st].[{0}]", cdcColumn.SourceColumn));
							}
						}

						string insertQuery = null;
						if ((EdwTableConfig.HasChildTables(edwTable)) && (!EdwTableConfig.IsSelfReferenced(edwTable)))
						{
							insertQuery = String.Format(CultureInfo.InvariantCulture,
@"/*for updated rows*/
DECLARE @row_count BIGINT

INSERT INTO [{0}].[{1}] WITH (TABLOCK) /* Table:Ini_{0}.{1} */
([__$transform_id], [{2}], [{3}])
 SELECT
	{9},
	k.PK_INT,
	{4}
FROM [Staging].[{5}] [st]{6}{7}
		INNER JOIN #Keys k ON k.PK_GUID = [st].[{8}] AND k.TransformID = {9} {10}

SET @row_count = @@ROWCOUNT

SELECT @MaxID = ISNULL(MAX([{2}]), 0) FROM [{0}].[{1}]

/*for new rows*/
INSERT INTO [{0}].[{1}] WITH (TABLOCK) /* Table:Ini_{0}.{1} */
([__$transform_id], [{2}], [{3}])
SELECT
	{9},
	@MaxID + ROW_NUMBER() OVER (ORDER BY (SELECT 1)),
	{4}
FROM [Staging].[{5}] [st]{6}{7}
LEFT JOIN #Keys k ON k.[PK_GUID] = [st].[{8}] AND k.TransformID = {9}
WHERE k.PK_GUID IS NULL{11}

SET @row_count = @row_count + @@ROWCOUNT
",
								edwTable.Schema,
								edwTable.Name,
								edwTable.BaseName + "Key",
								String.Join("], [", columnList.Select(c => c.Name)),
								String.Join(",\r\n\t", columnQueryList),
								edwTable.StagingTable,
								joinQueryList.Any() ? "\r\n" + String.Join("\r\n", joinQueryList) : "",
								crossApplyQueryList.Any() ? "\r\n" + String.Join("\r\n", crossApplyQueryList) : "",
								pkColumn,
								edwTable.TransformId,
								!string.IsNullOrEmpty(whereClause) ? "\r\nWHERE " + whereClause : "",
								!string.IsNullOrEmpty(whereClause) ? " AND\r\n\t(" + whereClause + ")" : "");
						}
						else if (EdwTableConfig.IsSelfReferenced(edwTable))
						{
							insertQuery = String.Format(CultureInfo.InvariantCulture,
@"
DECLARE @row_count BIGINT

/*for all rows*/
INSERT INTO [{0}].[{1}] WITH (TABLOCK) /* Table:Ini_{0}.{1} */
([__$transform_id], [{2}], [{3}])
 SELECT
	{9},
	k.PK_INT,
	{4}
FROM [Staging].[{5}] [st]{6}{7}
INNER JOIN #Keys k ON k.PK_GUID = [st].[{8}] AND k.TransformID = {9} {10}

SET @row_count = @@ROWCOUNT
",
								edwTable.Schema,
								edwTable.Name,
								edwTable.BaseName + "Key",
								String.Join("], [", columnList.Select(c => c.Name)),
								String.Join(",\r\n\t", columnQueryList),
								edwTable.StagingTable,
								joinQueryList.Any() ? "\r\n" + String.Join("\r\n", joinQueryList) : "",
								crossApplyQueryList.Any() ? "\r\n" + String.Join("\r\n", crossApplyQueryList) : "",
								pkColumn,
								edwTable.TransformId,
								!string.IsNullOrEmpty(whereClause) ? "\r\nWHERE " + whereClause : "");
						}
						else
						{
							insertQuery = String.Format(CultureInfo.InvariantCulture,
@"
DECLARE @row_count BIGINT
SELECT @MaxID = ISNULL(MAX([{9}]), 0) FROM [{0}].[{1}]

INSERT INTO [{0}].[{1}] WITH (TABLOCK) /* Table:Ini_{0}.{1} */
([__$transform_id], [{9}], [{2}])
SELECT
	{8},
	@MaxID + ROW_NUMBER() OVER (ORDER BY (SELECT 1)),
	{3}
FROM [Staging].[{4}] [st]{5}{6}{7}

SET @row_count = @@ROWCOUNT
",
								edwTable.Schema,
								edwTable.Name,
								String.Join("], [", columnList.Select(c => c.Name)),
								String.Join(",\r\n\t", columnQueryList),
								edwTable.StagingTable,
								joinQueryList.Any() ? "\r\n" + String.Join("\r\n", joinQueryList) : "",
								crossApplyQueryList.Any() ? "\r\n" + String.Join("\r\n", crossApplyQueryList) : "",
								!string.IsNullOrEmpty(whereClause) ? "\r\nWHERE " + whereClause : "",
								edwTable.TransformId,
								edwTable.BaseName + "Key"
								);
						}

						string unprocessedDateQuery = null;
						if (edwTable.TransformId == 1)
						{
							unprocessedDateQuery = BiAutomationConfigDataSet.GetUnProcessedDataQuery(edwTable.Schema, edwTable.Name, BiConstants.BiAdminSchemaName);
						}

						sqlText = String.Format(CultureInfo.InvariantCulture, @"{0}{1}", unprocessedDateQuery, insertQuery);
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
				}
			}
			return sqlText;
		}

		public static string GetUnProcessedDataQuery(string schema, string table, string adminSchema)
		{
			return String.Format(CultureInfo.InvariantCulture,
@"DELETE
FROM [{2}].[SsasPartitionUnprocessedDate]
WHERE SchemaName = '{0}' and TableName = '{1}'

INSERT INTO [{2}].[SsasPartitionUnprocessedDate]
(SchemaName, TableName, CreateDate)
VALUES('{0}', '{1}', NULL)
",
				schema,
				table,
				adminSchema
			);
		}

		public string GetIncrementalInsertQueryForEdwTableTesting(EdwTableConfigRow edwTable)
		{
			return String.Format(CultureInfo.InvariantCulture,
@"
CREATE TABLE #Keys 
(
		PK_GUID UNIQUEIDENTIFIER NOT NULL, 
		PK_INT BIGINT NOT NULL, 
		TransformID INT NOT NULL,
		PRIMARY KEY CLUSTERED (PK_GUID ASC, TransformID ASC)
)

DECLARE @MaxID BIGINT = 0

{0}",
				GetIncrementalInsertQueryForEdwTable(edwTable));
		}

		public string GetIncrementalInsertQueryForEdwTable(EdwTableConfigRow edwTable)
		{
			string sqlText = null;
			string sqlMergePartitionDateText = null;
			if (!edwTable.HasErrors)
			{
				var columnList = edwTable.GetEdwColumnConfigRows().Where(c => !string.IsNullOrEmpty(c.Expression));
				if (columnList.Any())
				{
					try
					{
						var columnQueryList = new List<string>();
						var joinQueryList = new List<string>();
						var crossApplyQueryList = new List<string>();
						var referenceKeyDictionary = new Dictionary<string, string>();
						int counter = 1;

						var stagingTable = CdcTableConfig.Where(t => t.SourceSchema == edwTable.SourceSchema && t.SourceTable == edwTable.StagingTable).FirstOrDefault();
						var stagingColumns = stagingTable.GetCdcColumnConfigRows().Where(c => c.ColumnInEdw);
						var pkColumn = stagingColumns.FirstOrDefault(c => c.IsPrimaryKey).SourceColumn;
						var partitionKey = columnList.Where(c => c.IsPartitionKey).Select(c => c.Name).FirstOrDefault();

						string whereClause = null;
						if (!string.IsNullOrEmpty(edwTable.WhereClause))
						{
							whereClause = edwTable.WhereClause;
							foreach (var cdcColumn in stagingTable.GetCdcColumnConfigRows())
							{
								var regex = new Regex(String.Format(CultureInfo.InvariantCulture, @"\[*\b{0}\b\]*", cdcColumn.SourceColumn), RegexOptions.IgnoreCase);
								whereClause = regex.Replace(whereClause, String.Format(CultureInfo.InvariantCulture, "[st].[{0}]", cdcColumn.SourceColumn));
							}
						}

						int selfReferenceCounter = 0;
						var keysInsertQueryForSelfReferencedTables = "";

						foreach (var edwColumn in columnList)
						{
							if (edwColumn.UsesFunction)
							{
								columnQueryList.Add(String.Format(CultureInfo.InvariantCulture, @"/*{0}*/ [{0}].value AS [{0}]", edwColumn.Name));
								crossApplyQueryList.Add(String.Format(CultureInfo.InvariantCulture, @"CROSS APPLY {0} AS {1}", edwColumn.Expression, edwColumn.Name));
							}
							else if (string.IsNullOrEmpty(edwColumn.ParentColumn) || edwColumn.Expression == "NULL")
							{
								columnQueryList.Add(String.Format(CultureInfo.InvariantCulture, "/*{0}*/ {1}", edwColumn.Name, AddTableAlias(stagingTable, edwColumn.Expression, "st")));
							}
							else
							{
								var refTableKey = $"Expression:{edwColumn.Expression} ParentTable:{edwColumn.ParentTable} ParentColumn:{edwColumn.ParentColumn}";
								var shouldCreateReference = !referenceKeyDictionary.ContainsKey(refTableKey);
								if (shouldCreateReference)
								{
									referenceKeyDictionary.Add(refTableKey, "");
								}

								var referenceTable = EdwTableConfig.Where(t => t.Name == edwColumn.ParentTable).FirstOrDefault();
								string condition = FormatCondition(stagingTable, edwColumn);

								if ((EdwTableConfig.IsSelfReferenced(edwTable)) && (referenceTable != null) && (referenceTable.Name == edwTable.Name))
								{
									selfReferenceCounter++;
									if (selfReferenceCounter == 1)
									{
										keysInsertQueryForSelfReferencedTables = String.Format(CultureInfo.InvariantCulture,
@"CREATE TABLE #ForeignKeys (FK_GUID UNIQUEIDENTIFIER, TransformID INT)

INSERT INTO #ForeignKeys (FK_GUID, TransformID) /* Table:Inc_{4}.{5} */
SELECT DISTINCT Q.FK_GUID, {3} AS TransformID
FROM
(SELECT [{1}] AS FK_GUID
FROM [Staging].[{0}]
WHERE {2} [{1}] IS NOT NULL
",
										edwTable.StagingTable,
										edwColumn.Expression,
										condition,
										edwTable.TransformId,
										edwTable.Schema,
										edwTable.Name
										);
									}
									else
									{
										keysInsertQueryForSelfReferencedTables = String.Format(CultureInfo.InvariantCulture,
@"{3}
UNION
SELECT [{1}]
FROM [Staging].[{0}]
WHERE {2} [{1}] IS NOT NULL
",
										edwTable.StagingTable,
										edwColumn.Expression,
										condition,
										keysInsertQueryForSelfReferencedTables
										);
									}

									var refTableCounterName = shouldCreateReference ? "Keys" + selfReferenceCounter : referenceKeyDictionary[refTableKey];
									columnQueryList.Add(String.Format(CultureInfo.InvariantCulture, @"/*{0}*/ [{1}].[PK_INT]", edwColumn.Name, refTableCounterName));
									if (shouldCreateReference)
									{
										referenceKeyDictionary[refTableKey] = refTableCounterName;
										joinQueryList.Add(String.Format(CultureInfo.InvariantCulture, "LEFT JOIN #Keys [{0}] ON st.[{1}] = [{0}].[PK_GUID] AND [{0}].TransformID = {2}",
											refTableCounterName,
											edwColumn.Expression,
											edwTable.TransformId
										));
									}
								}
								else if (referenceTable == null)
								{
									var refTableCounterName = shouldCreateReference ? "RefTable" + counter++ : referenceKeyDictionary[refTableKey];
									var refCustomTable = EdwCustomTableConfig.Where(t => t.Name == edwColumn.ParentTable).FirstOrDefault();
									columnQueryList.Add(String.Format(CultureInfo.InvariantCulture, @"/*{0}*/ [{1}].[{2}]", edwColumn.Name, refTableCounterName, edwColumn.TargetColumn));
									if (shouldCreateReference)
									{
										referenceKeyDictionary[refTableKey] = refTableCounterName;
										joinQueryList.Add(String.Format(CultureInfo.InvariantCulture, "LEFT JOIN [{0}].[{1}] [{2}]\r\n\tON {3}{4} = [{2}].[{5}]",
											refCustomTable.Schema,
											refCustomTable.Name,
											refTableCounterName,
											condition,
											AddTableAlias(stagingTable, edwColumn.Expression, "st"),
											edwColumn.ParentColumn
										));
									}
								}
								else
								{
									var refTableCounterName = shouldCreateReference ? "RefTable" + counter++ : referenceKeyDictionary[refTableKey];
									columnQueryList.Add(String.Format(CultureInfo.InvariantCulture, @"/*{0}*/ [{1}].[{2}]", edwColumn.Name, refTableCounterName, referenceTable.BaseName + "Key"));
									if (shouldCreateReference)
									{
										referenceKeyDictionary[refTableKey] = refTableCounterName;
										joinQueryList.Add(String.Format(CultureInfo.InvariantCulture, "LEFT JOIN [{0}].[{1}] [{2}]\r\n\tON {3}{4} = {2}.[{5}]",
											referenceTable.Schema,
											referenceTable.Name,
											refTableCounterName,
											condition,
											AddTableAlias(stagingTable, edwColumn.Expression, "st"),
											edwColumn.ParentColumn
										));
									}
								}
							}
						}

						if (selfReferenceCounter > 0)
						{
							keysInsertQueryForSelfReferencedTables = String.Format(CultureInfo.InvariantCulture,
@"{0}
) Q

CREATE CLUSTERED INDEX CX_ForeignKeys_TEMP ON #ForeignKeys (FK_GUID, TransformID)

INSERT INTO #Keys  WITH (TABLOCK) (PK_GUID, PK_INT, TransformID)
SELECT fk.FK_GUID, mt.[{4}], fk.TransformID
FROM [{1}].[{2}] mt
INNER JOIN #ForeignKeys fk ON fk.FK_GUID = mt.[{3}] AND mt.[__$transform_id] = fk.TransformID
LEFT JOIN #Keys k ON k.PK_GUID = fk.FK_GUID AND fk.TransformID = k.TransformID
WHERE k.PK_GUID IS NULL
",
							keysInsertQueryForSelfReferencedTables,
							edwTable.Schema,
							edwTable.Name,
							edwTable.BaseName + "ID",
							edwTable.BaseName + "Key"
							);
						}

						if (columnQueryList.Any())
						{
							if (string.IsNullOrEmpty(partitionKey))
							{
								sqlMergePartitionDateText = String.Format(CultureInfo.InvariantCulture,
@"IF NOT EXISTS
	(SELECT 1 
	FROM [{2}].[SsasPartitionUnprocessedDate] 
	WHERE SchemaName = '{0}' and TableName = '{1}' AND CreateDate IS NULL)
	AND
	EXISTS
	(SELECT 1
	FROM [{2}].[TransformedRow]
	WHERE SchemaName = '{0}' and TableName = '{1}')
BEGIN
	INSERT INTO [{2}].[SsasPartitionUnprocessedDate] (SchemaName, TableName, CreateDate)
	VALUES('{0}', '{1}', NULL)
END",
			edwTable.Schema,
			edwTable.Name,
			BiConstants.BiAdminSchemaName);
							}
							else
							{
								sqlMergePartitionDateText = String.Format(CultureInfo.InvariantCulture,
@"IF NOT EXISTS
	(SELECT 1 
	FROM [{2}].[SsasPartitionUnprocessedDate] 
	WHERE SchemaName = '{0}' and TableName = '{1}' AND CreateDate IS NULL)
BEGIN
	INSERT INTO [{2}].[SsasPartitionUnprocessedDate] (SchemaName, TableName, CreateDate)
	SELECT DISTINCT tr.SchemaName, tr.TableName, tr.CreateDate
	FROM [{2}].[TransformedRow] tr
	LEFT JOIN [{2}].[SsasPartitionUnprocessedDate] ud ON tr.SchemaName = ud.SchemaName AND tr.TableName = ud.TableName AND tr.CreateDate = ud.CreateDate
	WHERE tr.SchemaName = '{0}' AND tr.TableName = '{1}' AND ud.SchemaName IS NULL
END",
			edwTable.Schema,
			edwTable.Name,
			BiConstants.BiAdminSchemaName);
							}

							if (EdwTableConfig.IsSelfReferenced(edwTable))
							{
								sqlText = String.Format(CultureInfo.InvariantCulture,
@"{15}
DECLARE @inserts BIGINT = 0

/*<topNdeclare>*/

INSERT INTO [{0}].[{1}] WITH (TABLOCK) /* Table:Inc_{0}.{1} */
([__$transform_id], [{7}], [{2}])
OUTPUT '{0}', '{1}', INSERTED.[{7}]{8} INTO [{9}].[TransformedRow] (SchemaName, TableName, KeyValue{10})
SELECT /*<topNselect*/
	{13},
	Keys.PK_INT,
	{3}
FROM [Staging].[{4}] [st]{5}{6}
INNER JOIN #Keys Keys on Keys.PK_GUID = st.[{11}] AND Keys.TransformId = {13}
WHERE ([st].[OP_TYPE] = 0 OR [st].[OP_TYPE] = 9){14}

SET @inserts = @@ROWCOUNT

{12}",
									edwTable.Schema,
									edwTable.Name,
									String.Join("], [", columnList.Select(c => c.Name)),
									String.Join(",\r\n\t", columnQueryList),
									edwTable.StagingTable,
									joinQueryList.Any() ? "\r\n" + String.Join("\r\n", joinQueryList) : "",
									crossApplyQueryList.Any() ? "\r\n" + String.Join("\r\n", crossApplyQueryList) : "",
									edwTable.BaseName + "Key",
									!string.IsNullOrEmpty(partitionKey) ? String.Format(CultureInfo.InvariantCulture, ", ISNULL(INSERTED.[{0}], '1900-01-01')", partitionKey) : "",
									BiConstants.BiAdminSchemaName,
									!string.IsNullOrEmpty(partitionKey) ? ", CreateDate" : "",
									columnList.Where(c => c.Name == edwTable.BaseName + "ID").Select(c => c.Expression).FirstOrDefault(),
									sqlMergePartitionDateText,
									edwTable.TransformId,
									!string.IsNullOrEmpty(whereClause) ? " AND\r\n\t(" + whereClause + ")" : "",
									keysInsertQueryForSelfReferencedTables
								);
							}
							else
							{
								sqlText = String.Format(CultureInfo.InvariantCulture,
@"DECLARE @inserts BIGINT = 0

/*<topNdeclare>*/

INSERT INTO [{0}].[{1}] WITH (TABLOCK) /* Table:Inc_{0}.{1} */
([__$transform_id], [{7}], [{2}])
OUTPUT '{0}', '{1}', INSERTED.[{7}]{8} INTO [{9}].[TransformedRow] (SchemaName, TableName, KeyValue{10})
SELECT /*<topNselect>*/
	{13},
	CASE WHEN tr.KeyValue IS NULL THEN @MaxID + ROW_NUMBER() OVER (ORDER BY (SELECT 1)) ELSE tr.KeyValue END,
	{3}
FROM [Staging].[{4}] [st]{5}{6}
LEFT JOIN [{9}].[TransformedRow] tr on tr.SchemaName = '{0}' AND tr.TableName = '{1}' AND tr.PKValue = st.[{11}] AND tr.TransformId = {13}
WHERE ([st].[OP_TYPE] = 0 OR [st].[OP_TYPE] = 9){14}

SET @inserts = @@ROWCOUNT
{12}",
								edwTable.Schema,
								edwTable.Name,
								String.Join("], [", columnList.Select(c => c.Name)),
								String.Join(",\r\n\t", columnQueryList),
								edwTable.StagingTable,
								joinQueryList.Any() ? "\r\n" + String.Join("\r\n", joinQueryList) : "",
									crossApplyQueryList.Any() ? "\r\n" + String.Join("\r\n", crossApplyQueryList) : "",
								edwTable.BaseName + "Key",
								!string.IsNullOrEmpty(partitionKey) ? String.Format(CultureInfo.InvariantCulture, ", ISNULL(INSERTED.[{0}], '1900-01-01')", partitionKey) : "",
								BiConstants.BiAdminSchemaName,
								!string.IsNullOrEmpty(partitionKey) ? ", CreateDate" : "",
								columnList.Where(c => c.Name == edwTable.BaseName + "ID").Select(c => c.Expression).FirstOrDefault(),
								sqlMergePartitionDateText,
								edwTable.TransformId,
								!string.IsNullOrEmpty(whereClause) ? " AND\r\n\t" + whereClause : ""
							);
							}
						}
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
					}
				}
			}
			return sqlText;
		}

		public string GetIncrementalDeleteQueryForEdwTable(EdwTableConfigRow edwTable)
		{
			string sqlText = null;
			if (!edwTable.HasErrors)
			{
				try
				{
					var stagingTable = CdcTableConfig.Where(t => t.SourceSchema == edwTable.SourceSchema && t.SourceTable == edwTable.StagingTable).FirstOrDefault();
					var stagingColumns = stagingTable.GetCdcColumnConfigRows().Where(c => c.ColumnInEdw);
					var pkColumn = stagingColumns.FirstOrDefault(c => c.IsPrimaryKey).SourceColumn;
					var partitionKey = edwTable.GetEdwColumnConfigRows().Where(c => c.IsPartitionKey).Select(c => c.Name).FirstOrDefault();
					var keepAsColumns = edwTable.GetEdwColumnConfigRows().Where(c => !string.IsNullOrWhiteSpace(c.KeepAs));

					sqlText = String.Format(CultureInfo.InvariantCulture,
@"DELETE mt
OUTPUT '{0}', '{1}', DELETED.{5}, DELETED.[{3}]{6}, {9}{10}
INTO [{7}].[TransformedRow] (SchemaName, TableName, KeyValue, PKValue{8}, TransformId{11})
FROM [{0}].[{1}] [mt] WITH(INDEX(IX_{0}_{1}_{3}))
INNER JOIN [Staging].[{2}] [st] ON [mt].[{3}] = [st].[{4}] AND [mt].[__$transform_id] = {9}
/*PartitionEliminationQuery*/",
							edwTable.Schema,
							edwTable.Name,
							edwTable.StagingTable,
							edwTable.BaseName + "ID",
							pkColumn,
							edwTable.BaseName + "Key",
							!string.IsNullOrEmpty(partitionKey) ? String.Format(CultureInfo.InvariantCulture, ", ISNULL(DELETED.[{0}], '1900-01-01')", partitionKey) : "",
							BiConstants.BiAdminSchemaName,
							!string.IsNullOrEmpty(partitionKey) ? ", CreateDate" : "",
							edwTable.TransformId,
							keepAsColumns.Any() ? ", " + String.Join(", ", keepAsColumns.Select(c => String.Format(CultureInfo.InvariantCulture, "DELETED.[{0}]", c.Name))) : "",
							keepAsColumns.Any() ? ", " + String.Join(", ", keepAsColumns.Select(c => c.KeepAs)) : ""
						);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
				}
			}
			return sqlText;
		}

		public string GetEditableCustomIndexScriptQueryForEdwTable(EdwTableConfigRow edwTable)
		{
			var sqlText = string.Empty;
			if (!edwTable.HasErrors && edwTable.CustomIndex != null)
			{
				sqlText = edwTable.CustomIndex + "\r\n\r\n";
			}
			return sqlText;
		}

		public string GetCustomIndexScriptQueryForEdwTable(EdwTableConfigRow edwTable)
		{
			string sqlText = null;
			if (!edwTable.HasErrors)
			{
				try
				{
					var indexes = new StringBuilder();

					foreach (var column in edwTable.GetEdwColumnConfigRows().Where(x => x.EnableIndex))
					{
						var includedColumn = column.Name == edwTable.BaseName + "ID" ? $" INCLUDE({edwTable.BaseName}Key)" : "";
						var transformId = column.Name == edwTable.BaseName + "ID" ? ", [__$transform_id]" : "";
						indexes.AppendLine($"CREATE NONCLUSTERED INDEX [IX_{edwTable.Schema}_{edwTable.Name}_{column.Name}] ON [{edwTable.Schema}].[{edwTable.Name}] ({column.Name}{transformId}){includedColumn} WITH (DATA_COMPRESSION = PAGE, DROP_EXISTING = OFF);\r\n");
					}

					sqlText = indexes.ToString();
				}
				catch (Exception ex) when (ex.IsCriticalException())
				{
				}
			}
			return sqlText;
		}

		public string GetEditableCustomIndexScriptQueryForDenormalizedTable(EdwDenormalizedTableConfigRow edwDenormTable)
		{
			var sqlText = string.Empty;
			if (!edwDenormTable.HasErrors && edwDenormTable.CustomIndex != null)
			{
				sqlText = edwDenormTable.CustomIndex + "\r\n\r\n";
			}
			return sqlText;
		}

		public string GetCustomIndexScriptQueryForDenormalizedTable(EdwDenormalizedTableConfigRow edwDenormTable)
		{
			string sqlText = null;
			if (!edwDenormTable.HasErrors)
			{
				try
				{
					var indexes = new List<string>();

					foreach (var column in edwDenormTable.GetEdwDenormalizedColumnConfigRows().Where(x => x.EnableIndex))
					{
						indexes.Add("CREATE NONCLUSTERED INDEX [IX_" + edwDenormTable.Schema + "_" + edwDenormTable.Name + "_" + column.Name + "] ON [" + edwDenormTable.Schema + "].[" + edwDenormTable.Name + "] (" + column.Name + ") WITH (DATA_COMPRESSION = PAGE, DROP_EXISTING = OFF);\r\n\r\n");
					}

					sqlText = String.Join("", indexes);
				}
				catch (Exception ex) when (ex.IsCriticalException())
				{
				}
			}
			return sqlText;
		}

		public string GetEditableCustomIndexScriptQueryForCustomTable(EdwCustomTableConfigRow edwCustomTable)
		{
			string sqlText = string.Empty;
			if (!edwCustomTable.HasErrors && edwCustomTable.CustomIndex != null)
			{
				sqlText = edwCustomTable.CustomIndex + "\r\n\r\n";
			}
			return sqlText;
		}

		string AddTableAlias(CdcTableConfigRow stagingTable, string expression, string tableAlias)
		{
			string result = expression;

			if (!string.IsNullOrEmpty(expression))
			{
				foreach (var columnName in stagingTable.GetCdcColumnConfigRows().Where(c => c.ColumnInEdw).Select(c => c.SourceColumn))
				{
					var regex = new Regex(String.Format(CultureInfo.InvariantCulture, @"\[?\b{0}\b\]?(?!')", columnName), RegexOptions.IgnoreCase);
					result = regex.Replace(result, String.Format(CultureInfo.InvariantCulture, @"[{0}].[{1}]", tableAlias, columnName));
				}
			}

			return result;
		}

		string FormatCondition(CdcTableConfigRow stagingTable, EdwColumnConfigRow edwColumn)
		{
			var condition = !string.IsNullOrEmpty(edwColumn.Condition) ? edwColumn.Condition + " AND " : "";
			if (!string.IsNullOrEmpty(condition))
			{
				foreach (var column in stagingTable.GetCdcColumnConfigRows().Select(c => c.SourceColumn))
				{
					var conditionRegex = new Regex(String.Format(CultureInfo.InvariantCulture, @"\[?\b{0}\b\]?(?!')", column), RegexOptions.IgnoreCase);
					condition = conditionRegex.Replace(condition, String.Format(CultureInfo.InvariantCulture, "[st].[{0}]", column));
				}
			}

			return condition;
		}

		#endregion

		#region CDC Tables

		public CdcTableConfigRow[] GetOrderedCdcTableConfigRows()
		{
			return CdcTableConfig.OrderBy(cdcTable => cdcTable.SourceSchema).ThenBy(cdcTable => cdcTable.SourceTable).ToArray();
		}

		partial class CdcTableConfigDataTable
		{
			protected override void OnColumnChanged(DataColumnChangeEventArgs e)
			{
				base.OnColumnChanged(e);
				if (validationRequired && e.Row.RowState != DataRowState.Detached && e.Column.ColumnName != "Message")
				{
					((CdcTableConfigRow)e.Row).Validate();
				}
			}

			public void SuspendValidation()
			{
				validationRequired = false;
			}

			public void ResumeValidation()
			{
				validationRequired = true;
			}

			bool validationRequired = true;
		}

		partial class CdcColumnConfigDataTable
		{
			protected override void OnColumnChanged(DataColumnChangeEventArgs e)
			{
				base.OnColumnChanged(e);
				if (validationRequired && e.Row.RowState != DataRowState.Detached && e.Column.ColumnName != "Message")
				{
					((CdcColumnConfigRow)e.Row).Validate();
				}
			}

			public void SuspendValidation()
			{
				validationRequired = false;
			}

			public void ResumeValidation()
			{
				validationRequired = true;
			}

			bool validationRequired = true;
		}

		partial class CdcTableConfigRow
		{
			public CdcColumnConfigRow[] GetOrderedCdcColumnConfigRows()
			{
				if ((Table.ChildRelations["FK_CdcTableConfig_CdcColumnConfig"] == null))
				{
					return Array.Empty<CdcColumnConfigRow>();
				}
				else
				{
					return ((IEnumerable<CdcColumnConfigRow>)base.GetChildRows(Table.ChildRelations["FK_CdcTableConfig_CdcColumnConfig"])).Where(cdcColumnRow => cdcColumnRow.RowState != DataRowState.Detached).OrderBy(cdcColumnRow => cdcColumnRow.SourceColumn).ToArray();
				}
			}

			public void Validate()
			{
				if (Action != "Deleted" && RowState != DataRowState.Detached)
				{
					var tableMessage = new StringBuilder();

					if (!TableInAudit && !string.IsNullOrEmpty(AuditFilter))
					{
						tableMessage.AppendLine("Audit Filter should only be for tables enabled for Audit.");
					}

					if (!TableInEdw && !string.IsNullOrEmpty(EdwFilter))
					{
						tableMessage.AppendLine("EDW Filter should only be for tables enabled for EDW.");
					}

					var cdcColumns = GetCdcColumnConfigRows();
					var primaryKeyCount = cdcColumns.Count(c => c.IsPrimaryKey);
					if (TableInEdw)
					{
						if (primaryKeyCount > 1)
						{
							tableMessage.AppendLine("Only one Primary Key allowed for each table.");
						}

						if (string.IsNullOrEmpty(IndexedColumn))
						{
							tableMessage.AppendLine("Missing Indexed Column");
						}
						else
						{
							var indexedColumn = cdcColumns.FirstOrDefault(c => c.SourceColumn.Equals(IndexedColumn, StringComparison.OrdinalIgnoreCase));
							if (indexedColumn == null)
							{
								tableMessage.AppendLine("Indexed column is not part of the table schema.");
							}
							else if (indexedColumn.Nullable)
							{
								tableMessage.AppendLine("Indexed column cannot be nullable.");
							}
							else if (indexedColumn.DataType != "uniqueidentifier" && indexedColumn.DataType != "datetime")
							{
								tableMessage.AppendLine("Indexed column data type should be uniqueidentifier or datetime.");
							}
						}
					}

					if ((TableInAudit || TableInEdw) && !IsEdiClient)
					{
						var pk = cdcColumns.FirstOrDefault(c => c.IsPrimaryKey);
						if (pk is null || !pk.CdcEnabled)
						{
							tableMessage.AppendLine("Columns used to uniquely identify a row for net change tracking must be included in the list of captured columns. Enable CDC for the primary key of the source table.");
						}
					}

					if (cdcColumns.Any(c => c.HasErrors))
					{
						tableMessage.AppendLine("*See column error message");
					}

					if (tableMessage.Length > 0)
					{
						Message = tableMessage.ToString().TrimEnd();
						RowError = tableMessage.ToString().TrimEnd();
					}
					else
					{
						Message = null;
						RowError = null;
					}
				}
			}
		}

		partial class CdcColumnConfigRow
		{
			public CdcTableConfigRow GetCdcTable()
			{
				return (CdcTableConfigRow)base.GetParentRow(Table.ParentRelations["FK_CdcTableConfig_CdcColumnConfig"]);
			}

			public void Validate()
			{
				if (Action != "Deleted" && RowState != DataRowState.Detached)
				{
					var columnMessage = new StringBuilder();

					if (CdcEnabled)
					{
						if (string.IsNullOrWhiteSpace(DataType))
						{
							columnMessage.AppendLine("Missing 'Data Type' value");
						}
						else if (IsPrimaryKey && DataType != "uniqueidentifier")
						{
							columnMessage.AppendLine("Primary key should be of data type 'uniqueidentifier'.");
						}

						if (string.IsNullOrWhiteSpace(MaxLength.ToString(CultureInfo.InvariantCulture)))
						{
							columnMessage.AppendLine("Missing 'Max Length' value");
						}

						if ((string.IsNullOrWhiteSpace(ReferenceSchema) && !string.IsNullOrWhiteSpace(ReferenceTable)) ||
							(!string.IsNullOrWhiteSpace(ReferenceSchema) && string.IsNullOrWhiteSpace(ReferenceTable)))
						{
							columnMessage.AppendLine("Missing 'Reference schema' or 'Reference Table' value");
						}
					}

					if (ColumnInAudit)
					{
						switch (DataType)
						{
							case "geography":
								columnMessage.AppendLine("Geography data cannot be in Audit.");
								break;
							default:
								break;
						}
					}

					if (columnMessage.Length > 0)
					{
						Message = columnMessage.ToString().TrimEnd();
						RowError = columnMessage.ToString().TrimEnd();
					}
					else
					{
						Message = null;
						RowError = null;
					}

					CdcTableConfigRow.Validate();
				}
			}
		}

		#endregion

		#region EDW Base Tables

		partial class EdwTableConfigDataTable
		{
			protected override void OnColumnChanged(DataColumnChangeEventArgs e)
			{
				base.OnColumnChanged(e);
				if (validationRequired && e.Row.RowState != DataRowState.Detached && e.Column.ColumnName != "Message")
				{
					SuspendValidation();
					((EdwTableConfigRow)e.Row).Validate();
					ValidateUniqueness(e);
					ResumeValidation();
				}
			}

			void ValidateUniqueness(DataColumnChangeEventArgs e)
			{
				var row = ((EdwTableConfigRow)e.Row);
				if (e.Column.ColumnName == "Name" && this.Any(r => r.Name == (string)e.ProposedValue && r.DependencyOrder != row.DependencyOrder))
				{
					row.Message = "Name must be unique." + (string.IsNullOrEmpty(row.Message) ? "" : "\r\n" + row.Message);
					row.RowError = "Name must be unique." + (string.IsNullOrEmpty(row.RowError) ? "" : "\r\n" + row.RowError);
				}
			}

			public bool IsLastTransform(EdwTableConfigRow edwTable)
			{
				var maxTransformId = this.Where(t => t.Name == edwTable.Name).Max(t => t.TransformId);
				return edwTable.TransformId == maxTransformId;
			}

			public bool HasChildTables(EdwTableConfigRow edwTable)
			{
				var columnConfigDataTable = (EdwColumnConfigDataTable)this.ChildRelations[0].ChildTable;
				var hasChildTables = columnConfigDataTable.Any(c => c.ParentTable == edwTable.Name && c.TableName != edwTable.Name);
				return hasChildTables;
			}

			public bool IsSelfReferenced(EdwTableConfigRow edwTable)
			{
				return edwTable.GetEdwColumnConfigRows().Any(c => c.ParentTable == edwTable.Name);
			}

			public void SuspendValidation()
			{
				validationRequired = false;
			}

			public void ResumeValidation()
			{
				validationRequired = true;
			}

			bool validationRequired = true;
		}

		partial class EdwColumnConfigDataTable
		{
			public IEnumerable<EdwColumnConfigRow> GetDependentEdwColumns(CdcColumnConfigRow cdcColumn)
			{
				var configData = (BiAutomationConfigDataSet)DataSet;
				var result = new List<EdwColumnConfigRow>();

				var cdcTable = cdcColumn.GetCdcTable();
				if (cdcTable != null)
				{
					var cdcTableName = cdcTable.SourceTable;
					var edwTables = configData.EdwTableConfig.Where(
						t =>
							string.Equals(t.SourceSchema, cdcTable.SourceSchema, StringComparison.OrdinalIgnoreCase)
							&& string.Equals(t.StagingTable, cdcTable.SourceTable, StringComparison.OrdinalIgnoreCase)
						);

					if (edwTables.Any())
					{
						var regex = new Regex(String.Format(CultureInfo.InvariantCulture, @"\b{0}\b", cdcColumn.SourceColumn), RegexOptions.IgnoreCase);
						foreach (var edwTable in edwTables)
						{
							var edwColumns = edwTable.GetEdwColumnConfigRows().Where(c => !string.IsNullOrEmpty(c.Expression) && regex.Match(c.Expression).Success);
							if (edwColumns.Any())
							{
								result.AddRange(edwColumns);
							}
						}
					}
				}

				return result;
			}

			protected override void OnColumnChanged(DataColumnChangeEventArgs e)
			{
				base.OnColumnChanged(e);
				if (validationRequired && e.Row.RowState != DataRowState.Detached && e.Column.ColumnName != "Message")
				{
					SuspendValidation();
					((EdwColumnConfigRow)e.Row).Validate();
					ResumeValidation();
				}
			}

			public void SuspendValidation()
			{
				validationRequired = false;
			}

			public void ResumeValidation()
			{
				validationRequired = true;
			}

			bool validationRequired = true;
		}

		partial class EdwTableConfigRow
		{
			public string BaseName
			{
				get
				{
					return Name.Replace(BiConstants.EdwBaseTablePrefix, "");
				}
			}

			public void SetIsPartitioned()
			{
				IsPartitioned = GetEdwColumnConfigRows().Any(c => c.IsPartitionKey);
			}

			public void Validate()
			{
				var configData = (BiAutomationConfigDataSet)Table.DataSet;
				var tableMessage = new StringBuilder();
				var regex = new Regex("[^a-zA-Z0-9_]");

				var edwColumns = GetEdwColumnConfigRows();

				if (string.IsNullOrEmpty(Schema))
				{
					tableMessage.AppendLine("Schema cannot be empty.");
				}
				else if (regex.Match(Schema).Success)
				{
					tableMessage.AppendLine("Schema can only contain letters, numbers, and underscores.");
				}

				if (string.IsNullOrEmpty(Name))
				{
					tableMessage.AppendLine("Name cannot be empty.");
				}
				else if (!Name.StartsWith(BiConstants.EdwBaseTablePrefix))
				{
					tableMessage.AppendLine(String.Format(CultureInfo.InvariantCulture, "Name should start with '{0}'.", BiConstants.EdwBaseTablePrefix));
				}
				else if (regex.Match(Name).Success)
				{
					tableMessage.AppendLine("Name can only contain letters, numbers, and underscores.");
				}
				else if (tableEdwTableConfig.Count(t => t.Name == Name && t.TransformId == TransformId) > 1)
				{
					tableMessage.AppendLine("Name and Transform ID need to be unique.");
				}

				if (string.IsNullOrEmpty(SourceSchema))
				{
					tableMessage.AppendLine("Source Schema cannot be empty.");
				}
				else if (string.IsNullOrEmpty(StagingTable))
				{
					tableMessage.AppendLine("Staging Table cannot be empty.");
				}
				else if (!configData.CdcTableConfig.Any(t => t.SourceSchema == SourceSchema && t.SourceTable == StagingTable))
				{
					tableMessage.AppendLine("Staging Table does not exist.");
				}

				if (!edwColumns.Any(c => c.Name == BaseName + "ID"))
				{
					tableMessage.AppendLine(String.Format(CultureInfo.InvariantCulture, "Table should contain a '{0}ID' column.", BaseName));
				}

				if (!edwColumns.Any(c => c.Name == BaseName + "Key"))
				{
					tableMessage.AppendLine(String.Format(CultureInfo.InvariantCulture, "Table should contain a '{0}Key' column.", BaseName));
				}

				if (!string.IsNullOrEmpty(WhereClause) &&
					(WhereClause.ToLower(CultureInfo.InvariantCulture).Contains("select") ||
					 WhereClause.ToLower(CultureInfo.InvariantCulture).Contains("from")))
				{
					tableMessage.AppendLine(String.Format(CultureInfo.InvariantCulture, "Subqueries are not supported by the ETL.", BaseName));
				}

				if (edwColumns.Where(c => c.IsPartitionKey).Count() > 1)
				{
					tableMessage.AppendLine("Table can only have one partition key.");
				}

				var keepAsValue = edwColumns.Select(c => c.KeepAs).Where(v => !string.IsNullOrWhiteSpace(v));
				if (keepAsValue.Count() != keepAsValue.Distinct().Count())
				{
					tableMessage.AppendLine("Keep As value should be unique for a column in the table.");
				}

				if (edwColumns.Any(c => c.HasErrors))
				{
					tableMessage.AppendLine("*See column error message");
				}

				if (TransformId != 1)
				{
					var mainEdwTable = tableEdwTableConfig.Where(t => t.Name == Name && t.TransformId == 1).FirstOrDefault();
					if (mainEdwTable != null)
					{
						if (mainEdwTable.Schema != Schema)
						{
							tableMessage.AppendLine("Schema should be the same as the first transform table.");
						}

						var columns = GetEdwColumnConfigRows();
						var mainColumns = mainEdwTable.GetEdwColumnConfigRows();

						bool matchingColumns = columns.Length == mainColumns.Length;
						if (matchingColumns)
						{
							foreach (var column in columns)
							{
								if (!mainColumns.Any(c => c.Name == column.Name &&
									c.DataType == column.DataType &&
									c.MaxLength == column.MaxLength &&
									c.Precision == column.Precision &&
									c.Scale == column.Scale &&
									c.IsPartitionKey == column.IsPartitionKey))
								{
									matchingColumns = false;
									break;
								}
							}
						}

						if (!matchingColumns)
						{
							tableMessage.AppendLine("Columns does not match with the first transform table.");
						}
					}
					else
					{
						tableMessage.AppendLine("No table with Transform ID = 1.");
					}
				}

				if (tableMessage.Length > 0)
				{
					Message = tableMessage.ToString().TrimEnd();
					RowError = tableMessage.ToString().TrimEnd();
				}
				else
				{
					Message = null;
					RowError = null;
				}
			}
		}

		partial class EdwColumnConfigRow
		{
			public EdwTableConfigRow GetEdwTable()
			{
				return (EdwTableConfigRow)base.GetParentRow(Table.ParentRelations["FK_EdwTableConfig_EdwColumnConfig"]);
			}

			public void Validate()
			{
				var configData = (BiAutomationConfigDataSet)Table.DataSet;
				var columnMessage = new StringBuilder();
				var regex = new Regex("[^a-zA-Z0-9_]");

				if (string.IsNullOrEmpty(Name))
				{
					columnMessage.AppendLine("Name cannot be empty.");
				}
				else if (regex.Match(Name).Success)
				{
					columnMessage.AppendLine("Name can only contain letters, numbers, and underscores.");
				}
				else if (Name == GetEdwTable().BaseName + "ID")
				{
					if (DataType != "uniqueidentifier")
					{
						columnMessage.AppendLine("ID column should be a uniqueidentifier.");
					}

					if (!EnableIndex)
					{
						columnMessage.AppendLine("ID column should have index enabled.");
					}
				}
				else if (Name == GetEdwTable().BaseName + "Key")
				{
					if (DataType != "bigint")
					{
						columnMessage.AppendLine("Key column should be a bigint.");
					}

					if (!EnableIndex)
					{
						columnMessage.AppendLine("Key column should have index enabled.");
					}
				}

				var numericTypes = new[] { "bigint", "binary", "bit", "date", "datetime", "datetimeoffset", "decimal", "int", "money", "smalldatetime", "smallint", "tinyint" };
				var nonNumericTypes = new[] { "char", "image", "nvarchar", "uniqueidentifier", "varbinary", "varchar", "xml" };

				if (numericTypes.Contains(DataType))
				{
					if (Precision < 0 || Precision > 38)
					{
						columnMessage.AppendLine("Precision must range between 0 and 38.");
					}
					if (Scale < 0 || Scale > 38)
					{
						columnMessage.AppendLine("Scale must range between 0 and 38.");
					}
				}

				if (string.IsNullOrEmpty(Expression) && Name != GetEdwTable().BaseName + "Key")
				{
					columnMessage.AppendLine("Expression cannot be empty.");
				}
				else if (!string.IsNullOrEmpty(Expression) && Name == GetEdwTable().BaseName + "Key")
				{
					columnMessage.AppendLine("Surrogate Key column cannot have an expression.");
				}

				if (string.IsNullOrEmpty(ParentTable) && !string.IsNullOrEmpty(ParentColumn))
				{
					columnMessage.AppendLine(@"Missing 'Parent Table'.");
				}
				else if (!string.IsNullOrEmpty(ParentTable) && string.IsNullOrEmpty(ParentColumn))
				{
					columnMessage.AppendLine(@"Missing 'Parent Column'.");
				}
				else if (!string.IsNullOrEmpty(ParentTable) && !string.IsNullOrEmpty(ParentColumn))
				{
					var baseTable = configData.EdwTableConfig.FirstOrDefault(t => t.Name == ParentTable);
					var customTable = configData.EdwCustomTableConfig.FirstOrDefault(t => t.Name == ParentTable);

					if (baseTable != null)
					{
						if (!configData.EdwColumnConfig.Any(c => c.Name == ParentColumn && c.GetEdwTable().Name == ParentTable))
						{
							columnMessage.AppendLine(@"Parent Column does not exist.");
						}

						if (ParentTable.Replace(BiConstants.EdwBaseTablePrefix, "") + "ID" == ParentColumn && DataType != "bigint")
						{
							columnMessage.AppendLine(@"Data type should be bigint for soft key.");
						}

						if (!string.IsNullOrEmpty(TargetColumn))
						{
							columnMessage.AppendLine(@"Target Column should be empty for references to base tables.");
						}
					}
					else if (customTable != null)
					{
						if (!customTable.RunBeforeTransform)
						{
							columnMessage.AppendLine(@"Parent table is not a pre-transform custom table.");
						}

						if (!configData.EdwCustomColumnConfig.Any(c => c.Name == ParentColumn && c.GetEdwCustomTable().Name == ParentTable))
						{
							columnMessage.AppendLine(@"Parent Column does not exist.");
						}

						if (string.IsNullOrEmpty(TargetColumn))
						{
							columnMessage.AppendLine(@"Target Column should not be empty for references to custom tables.");
						}
						else if (!configData.EdwCustomColumnConfig.Any(c => c.Name == TargetColumn && c.GetEdwCustomTable().Name == ParentTable))
						{
							columnMessage.AppendLine(@"Target Column does not exist.");
						}
					}
					else
					{
						columnMessage.AppendLine(@"Parent Table does not exist.");
					}

					if (UsesFunction)
					{
						columnMessage.AppendLine(@"Column should not use function if it has a Parent Column.");
					}
				}

				if (!string.IsNullOrEmpty(Condition) && (string.IsNullOrEmpty(ParentTable) || string.IsNullOrEmpty(ParentColumn)))
				{
					columnMessage.AppendLine(@"Condition only applies for soft key columns.");
				}

				if (IsPartitionKey && DataType != "date")
				{
					columnMessage.AppendLine(@"Partition key should be of type date.");
				}

				var validKeepAsValues = new[] { "RefValue1", "RefValue2", "RefValue3", "RefValue4", "RefValue5" };
				if (!string.IsNullOrWhiteSpace(KeepAs) && !validKeepAsValues.Contains(KeepAs))
				{
					columnMessage.AppendLine(@"Invalid Keep As value for the column.");
				}

				if (string.IsNullOrEmpty(ParentTable) && string.IsNullOrEmpty(ParentColumn))
				{
					var edwTable = GetEdwTable();
					var cdcTable = configData.CdcTableConfig.FirstOrDefault(t => t.SourceSchema == edwTable.SourceSchema && t.SourceTable == edwTable.StagingTable);
					if (cdcTable != null)
					{
						var trimmedExpression = Expression.Trim(new[] { ' ', ']', '[' });
						var cdcColumn = cdcTable.GetCdcColumnConfigRows().FirstOrDefault(c => c.SourceColumn == trimmedExpression);
						if (cdcColumn != null)
						{
							if (DataType.Equals("xml", StringComparison.OrdinalIgnoreCase))
							{
								columnMessage.AppendLine(@"XML data cannot be in EDW.");
							}
							else if (cdcColumn.DataType != DataType)
							{
								columnMessage.AppendLine(@"Data type does not match with the source column.");
							}
							else if (DataType == "varchar" || DataType == "nvarchar" || DataType == "binary" || DataType == "varbinary")
							{
								if (cdcColumn.MaxLength != MaxLength)
								{
									columnMessage.AppendLine(@"Max length does not match with the source column.");
								}
							}
							else if (DataType == "decimal")
							{
								if (cdcColumn.Precision != Precision)
								{
									columnMessage.AppendLine(@"Precision does not match with the source column.");
								}
								if (cdcColumn.Scale != Scale)
								{
									columnMessage.AppendLine(@"Scale does not match with the source column.");
								}
							}
						}
					}
				}

				if (columnMessage.Length > 0)
				{
					Message = columnMessage.ToString().TrimEnd();
					RowError = columnMessage.ToString().TrimEnd();
				}
				else
				{
					Message = null;
					RowError = null;
				}

				GetEdwTable().Validate();
			}
		}

		#endregion

		#region EDW Aggregate Tables

		partial class EdwDenormalizedTableConfigDataTable
		{
			protected override void OnColumnChanged(DataColumnChangeEventArgs e)
			{
				base.OnColumnChanged(e);
				if (validationRequired && e.Row.RowState != DataRowState.Detached && e.Column.ColumnName != "Message")
				{
					SuspendValidation();
					((EdwDenormalizedTableConfigRow)e.Row).Validate();
					ResumeValidation();
				}
			}

			public void SuspendValidation()
			{
				validationRequired = false;
			}

			public void ResumeValidation()
			{
				validationRequired = true;
			}

			bool validationRequired = true;
		}

		partial class EdwDenormalizedColumnConfigDataTable
		{
			protected override void OnColumnChanged(DataColumnChangeEventArgs e)
			{
				base.OnColumnChanged(e);
				if (validationRequired && e.Row.RowState != DataRowState.Detached && e.Column.ColumnName != "Message")
				{
					SuspendValidation();
					((EdwDenormalizedColumnConfigRow)e.Row).Validate();
					ResumeValidation();
				}
			}

			public void SuspendValidation()
			{
				validationRequired = false;
			}

			public void ResumeValidation()
			{
				validationRequired = true;
			}

			bool validationRequired = true;
		}

		partial class EdwDenormalizedTableConfigRow
		{
			public string BaseName
			{
				get
				{
					return Name.Replace(BiConstants.EdwAggregateTablePrefix, "");
				}
			}

			public void SetIsPartitioned()
			{
				IsPartitioned = GetEdwDenormalizedColumnConfigRows().Any(c => c.IsPartitionKey);
			}

			public void Validate()
			{
				var tableMessage = new StringBuilder();
				var columns = GetEdwDenormalizedColumnConfigRows();
				var regex = new Regex("[^a-zA-Z0-9_]");

				if (string.IsNullOrEmpty(Schema))
				{
					tableMessage.AppendLine("Schema cannot be empty.");
				}
				else if (regex.Match(Schema).Success)
				{
					tableMessage.AppendLine("Schema can only contain letters, numbers, and underscores.");
				}

				if (string.IsNullOrEmpty(Name))
				{
					tableMessage.AppendLine("Name cannot be empty.");
				}
				else if (regex.Match(Name).Success)
				{
					tableMessage.AppendLine("Name can only contain letters, numbers, and underscores.");
				}
				else if (!Name.StartsWith(BiConstants.EdwAggregateTablePrefix))
				{
					tableMessage.AppendLine(String.Format(CultureInfo.InvariantCulture, "Name should start with '{0}'.", BiConstants.EdwAggregateTablePrefix));
				}
				else if (!string.IsNullOrEmpty(Expression))
				{
					var nameRegex = new Regex(String.Format(CultureInfo.InvariantCulture, @"\b{0}{1}\b", BiConstants.EdwBaseTablePrefix, BaseName), RegexOptions.IgnoreCase);
					if (nameRegex.Match(Expression).Success)
					{
						tableMessage.AppendLine("Base Name should not be the same as a source table.");
					}
				}

				if (!columns.Any(c => c.Name == BaseName + "Key"))
				{
					tableMessage.AppendLine(String.Format(CultureInfo.InvariantCulture, "Table should contain a '{0}Key' column.", BaseName));
				}

				if (!BiScriptHelper.ContainsIllegalQueryElements(tableMessage, Expression))
				{
					var sourceTables = ParseSourceTables(Expression);

					if (sourceTables.Count() < 2)
					{
						tableMessage.AppendLine("Aggregate Table should have more than 1 source table.");
					}
					else if (sourceTables.Any(t => !((BiAutomationConfigDataSet)Table.DataSet).EdwTableConfig.Any(et => et.Name == t.Table)))
					{
						tableMessage.AppendLine("Aggregate Table expression can only use base tables.");
					}

					foreach (var sourceTable in sourceTables)
					{
						if (!string.IsNullOrEmpty(sourceTable.Alias))
						{
							var baseName = sourceTable.Table.Replace(BiConstants.EdwBaseTablePrefix, "").Replace(BiConstants.EdwAggregateTablePrefix, "");
							var expRegex = new Regex(String.Format(CultureInfo.InvariantCulture, @"^\[?{0}\]?\.?\[?{1}Key\]?$", sourceTable.Alias, baseName));
							var softKey = columns.FirstOrDefault(c => expRegex.Match(c.Expression).Success);
							if (softKey == null)
							{
								tableMessage.AppendLine(String.Format(CultureInfo.InvariantCulture, @"Missing soft key for table [{0}].[{1}] [{2}] with expression ""[{2}].[{3}Key]"".", sourceTable.Schema, sourceTable.Table, sourceTable.Alias, baseName));
							}
						}
						else
						{
							tableMessage.AppendLine(String.Format(CultureInfo.InvariantCulture, @"Missing alias for table [{0}].[{1}].", sourceTable.Schema, sourceTable.Table));
						}
					}
				}

				if (columns.Where(c => c.IsPartitionKey).Count() > 1)
				{
					tableMessage.AppendLine("Table can only have one partition key.");
				}

				if (columns.Any(c => c.HasErrors))
				{
					tableMessage.AppendLine("*See column error message");
				}

				if (tableMessage.Length > 0)
				{
					Message = tableMessage.ToString().TrimEnd();
					RowError = tableMessage.ToString().TrimEnd();
				}
				else
				{
					Message = null;
					RowError = null;
				}
			}
		}

		partial class EdwDenormalizedColumnConfigRow
		{
			public EdwDenormalizedTableConfigRow GetDenormalizedTable()
			{
				return (EdwDenormalizedTableConfigRow)base.GetParentRow(Table.ParentRelations["FK_EdwDenormalizedTableConfig_EdwDenormalizedColumnConfig"]);
			}

			public void Validate()
			{
				var configData = (BiAutomationConfigDataSet)Table.DataSet;
				var columnMessage = new StringBuilder();
				var denormTable = GetDenormalizedTable();
				var regex = new Regex("[^a-zA-Z0-9_]");

				if (string.IsNullOrEmpty(Name))
				{
					columnMessage.AppendLine("Name cannot be empty.");
				}
				else if (regex.Match(Name).Success)
				{
					columnMessage.AppendLine("Name can only contain letters, numbers, and underscores.");
				}

				if (Name == denormTable.BaseName + "Key" && DataType != "bigint")
				{
					columnMessage.AppendLine("Key should be of type bigint.");
				}

				if (string.IsNullOrEmpty(Expression) && Name != denormTable.BaseName + "Key")
				{
					columnMessage.AppendLine("Expression cannot be empty.");
				}
				else if (!string.IsNullOrEmpty(Expression))
				{
					var castRegex = new Regex(@"\bCAST\b\s*\(.+?AS\s+\bDATE\b\s*\)", RegexOptions.IgnoreCase);
					var convertRegex = new Regex(@"\bCONVERT\b\s*\(\s*\bDATE\b\s*,.+?\)", RegexOptions.IgnoreCase);
					var ltrimRegex = new Regex(@"\bLTRIM\b\s*\(.+?\)", RegexOptions.IgnoreCase);
					var rtrimRegex = new Regex(@"\bRTRIM\b\s*\(.+?\)", RegexOptions.IgnoreCase);

					if (castRegex.Match(Expression).Success ||
						convertRegex.Match(Expression).Success ||
						ltrimRegex.Match(Expression).Success ||
						rtrimRegex.Match(Expression).Success)
					{
						columnMessage.AppendLine("Column expression should be created in base table level.");
					}
				}

				foreach (var sourceTable in ParseSourceTables(denormTable.Expression))
				{
					var edwTable = configData.EdwTableConfig.FirstOrDefault(t => t.Name == sourceTable.Table);
					if (edwTable != null)
					{
						var edwColumn = edwTable.GetEdwColumnConfigRows().FirstOrDefault(c => new Regex(String.Format(CultureInfo.InvariantCulture, @"^\[*\b{0}\b\]*\.\[*\b{1}\b\]*$", sourceTable.Alias, c.Name), RegexOptions.IgnoreCase).IsMatch(Expression));
						if (edwColumn != null)
						{
							if (edwColumn.DataType != DataType)
							{
								columnMessage.AppendLine(@"Data type does not match with the source column.");
							}
							else if (DataType == "varchar" || DataType == "nvarchar" || DataType == "binary" || DataType == "varbinary")
							{
								if (edwColumn.MaxLength != MaxLength)
								{
									columnMessage.AppendLine(@"Max length does not match with the source column.");
								}
							}
							else if (DataType == "decimal")
							{
								if (edwColumn.Precision != Precision)
								{
									columnMessage.AppendLine(@"Precision does not match with the source column.");
								}
								if (edwColumn.Scale != Scale)
								{
									columnMessage.AppendLine(@"Scale does not match with the source column.");
								}
							}
						}
					}
				}

				if (IsPartitionKey && DataType != "date")
				{
					columnMessage.AppendLine(@"Partition key should be of type date.");
				}

				if (columnMessage.Length > 0)
				{
					Message = columnMessage.ToString().TrimEnd();
					RowError = columnMessage.ToString().TrimEnd();
				}
				else
				{
					Message = null;
					RowError = null;
				}

				denormTable.Validate();
			}
		}

		#region Denormalized Table Queries

		public string GetDropAndCreateDenormalizedTableViewQuery(EdwDenormalizedTableConfigRow denormTable)
		{
			return String.Format(CultureInfo.InvariantCulture,
@"IF EXISTS
	(SELECT NULL FROM sys.views v INNER JOIN sys.schemas s ON v.schema_id = s.schema_id
		WHERE v.name = '{3}{0}' AND s.name = '{1}')
	DROP VIEW [{1}].[{3}{0}]

GO

{2}

GO",
				denormTable.Name,
				denormTable.Schema,
				GetCreateDenormalizedTableViewQuery(denormTable),
				BiConstants.EdwAggregateViewPrefix
			);
		}

		public string GetCreateDenormalizedTableViewQuery(EdwDenormalizedTableConfigRow denormTable)
		{
			string sqlText = null;
			if (!denormTable.HasErrors)
			{
				try
				{
					var columnList = denormTable.GetEdwDenormalizedColumnConfigRows().Where(c => c.Name != denormTable.BaseName + "Key").OrderBy(c => c.Name);
					if (columnList.Any())
					{
						var columnQueryList = new List<string>();
						foreach (var column in columnList)
						{
							columnQueryList.Add(String.Format(CultureInfo.InvariantCulture, @"[{0}] = {1}", column.Name, column.Expression));
						}

						sqlText = String.Format(CultureInfo.InvariantCulture,
@"CREATE VIEW [{0}].[{4}{1}]
WITH SCHEMABINDING AS
SELECT
	{2}
FROM {3}",
								denormTable.Schema,
								denormTable.Name,
								String.Join(",\r\n\t", columnQueryList),
								denormTable.Expression,
								BiConstants.EdwAggregateViewPrefix
							);
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
				}
			}
			return sqlText;
		}

		public string GetDropAndCreateInitialLoadQueryForDenormalizedTable(EdwDenormalizedTableConfigRow denormTable)
		{
			return String.Format(CultureInfo.InvariantCulture,
@"IF EXISTS
	(SELECT NULL FROM sys.procedures p INNER JOIN sys.schemas s ON p.schema_id = s.schema_id
		WHERE p.name = '{3}{0}' AND s.name = '{1}')
	DROP PROCEDURE [{1}].[{3}{0}]

GO

{2}

GO",
				denormTable.Name,
				denormTable.Schema,
				GetInitialLoadQueryForDenormalizedTable(denormTable),
				BiConstants.EdwAggregateInitialLoadProcPrefix
			);
		}

		public string GetInitialLoadQueryForDenormalizedTable(EdwDenormalizedTableConfigRow denormTable)
		{
			string sqlText = null;

			if (!denormTable.HasErrors)
			{
				try
				{
					var columnList = denormTable.GetEdwDenormalizedColumnConfigRows().Where(c => c.Name != denormTable.BaseName + "Key").Select(c => c.Name).OrderBy(c => c);
					if (columnList.Any())
					{
						sqlText = String.Format(CultureInfo.InvariantCulture,
@"CREATE PROCEDURE [{0}].[{6}{1}]

@number_of_future_periods_for_partitioning INT = 6

AS

BEGIN
	SET NOCOUNT ON

	DECLARE @RowCount BIGINT = 0, @DT1 DATETIME, @DT2 DATETIME

	DECLARE @temp INT = (SELECT TOP (1) null FROM [{0}].[{1}] WITH (UPDLOCK, TABLOCK));

	UPDATE [{5}].ModelTableState
	SET 
		CurrentState = 'Transforming - Initial Load',
		StateModifiedTimestamp = GETDATE(),
		SqlErrorMessage = NULL
	WHERE ModelTableName = '{1}' and ModelSchemaName = '{0}'

	TRUNCATE TABLE [{0}].[{1}]

	{8}

	DROP INDEX IF EXISTS [cci_{0}_{1}] ON [{0}].[{1}]

	CREATE CLUSTERED INDEX CX_{0}_{1}  --Table:Ini_{0}_{1}
	ON [{0}].[{1}] ([{7}]) ON [PRIMARY];

	DROP INDEX CX_{0}_{1} ON [{0}].[{1}]

	SET @DT1 = GETDATE()

	INSERT INTO [{0}].[{1}] WITH (TABLOCK) --Table:Ini_{0}_{1}
		([{7}], [{2}])
	SELECT
		ROW_NUMBER() OVER (ORDER BY (SELECT 1)),	--[{7}]
		[{3}]
	FROM
		[{0}].[{4}]

	SET @RowCount = @@ROWCOUNT
	SET @DT2 = GETDATE()

	UPDATE [{5}].ModelTableState
	SET 
		InitialLoadRecordCount = @RowCount,
		InitialLoadDurationMs = DATEDIFF(MS, @DT1, @DT2),
		CurrentState = 'Transforming - CCI Create',
		StateModifiedTimestamp = GETDATE()
	WHERE ModelTableName = '{1}' and ModelSchemaName = '{0}'

	DECLARE @EdwPartitionColumnName VARCHAR(128)
	SELECT @EdwPartitionColumnName = ISNULL(tc.EdwPartitionColumnName,'')
	FROM biadmin.ModelTableConfiguration tc
	WHERE ModelTableName = '{1}' and ModelSchemaName = '{0}'

	IF @EdwPartitionColumnName = ''
		CREATE CLUSTERED COLUMNSTORE INDEX [cci_{0}_{1}]  --Table:Ini_{0}_{1}
		ON [{0}].[{1}]
	ELSE
	BEGIN
		DECLARE @ResultCode INT, @ErrorMessage NVARCHAR(MAX), @ErrorCode INT
		EXEC Transform.usp_PartitionTable @LoadType = 1, @SchemaName = '{0}', @TableName = '{1}', @PartitionColumnName = @EdwPartitionColumnName, 
				@ErrorMessage = @ErrorMessage OUTPUT, @ResultCode = @ResultCode OUTPUT, @ErrorCode = @ErrorCode OUTPUT, @NumberOfFuturePeriods = @number_of_future_periods_for_partitioning
		IF @ResultCode NOT IN (1, 7) BEGIN SET @ErrorMessage = '[' + CAST(@ResultCode AS VARCHAR(5)) + ',PartitionTable(1)] ' + @ErrorMessage; RAISERROR (@ErrorMessage, 16, 2); RETURN; END
		IF @ResultCode = 7
			CREATE CLUSTERED COLUMNSTORE INDEX [cci_{0}_{1}]  --Table:Ini_{0}_{1}
			ON [{0}].[{1}]
	END

	{9}

	UPDATE [{5}].ModelTableState
	SET 
		CurrentState = 'Transforming - Clean Up of Partition Unprocessed Data',
		StateModifiedTimestamp = GETDATE()
	WHERE ModelTableName = '{1}' and ModelSchemaName = '{0}'

	DELETE
	FROM [{5}].[SsasPartitionUnprocessedDate]
	WHERE SchemaName = '{0}' and TableName = '{1}'

	INSERT INTO [{5}].[SsasPartitionUnprocessedDate]
	(SchemaName, TableName, CreateDate)
	VALUES('{0}', '{1}', NULL)

END",
								denormTable.Schema,
								denormTable.Name,
								String.Join("], [", columnList),
								String.Join("],\r\n\t\t[", columnList),
								String.Format(CultureInfo.InvariantCulture, "{0}{1}", BiConstants.EdwAggregateViewPrefix, denormTable.Name),
								BiConstants.BiAdminSchemaName,
								BiConstants.EdwAggregateInitialLoadProcPrefix,
								denormTable.BaseName + "Key",
								GetDropCustomIndexQueryForDenormalizedTable(denormTable),
								GetCreateIndexQueryForDenormalizedTable(denormTable)
							);
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
				}
			}

			return sqlText;
		}

		public string GetCreateIndexQueryForDenormalizedTable(EdwDenormalizedTableConfigRow denormTable)
		{
			var createQueries = new List<string>();

			foreach (var columnConfig in denormTable.GetEdwDenormalizedColumnConfigRows().Where(x => x.EnableIndex))
			{
				createQueries.Add(String.Format(CultureInfo.InvariantCulture,
				@"CREATE NONCLUSTERED INDEX [IX_{0}_{1}_{2}]  --Table:Ini_{0}_{1}
	ON [{0}].[{1}] ({2}) WITH (DATA_COMPRESSION = PAGE, DROP_EXISTING = OFF);",
				denormTable.Schema,
				denormTable.Name,
				columnConfig.Name));
			}

			if (!string.IsNullOrEmpty(denormTable.CustomIndex))
			{
				createQueries.Add(denormTable.CustomIndex);
			}

			return String.Join("\r\n\t", createQueries);
		}

		public string GetDropCustomIndexQueryForDenormalizedTable(EdwDenormalizedTableConfigRow denormTable)
		{
			var dropQueries = new List<string>();

			foreach (var columnConfig in denormTable.GetEdwDenormalizedColumnConfigRows().Where(x => x.EnableIndex))
			{
				dropQueries.Add(String.Format(CultureInfo.InvariantCulture, @"DROP INDEX IF EXISTS [IX_{0}_{1}_{2}] ON [{0}].[{1}]",
				denormTable.Schema,
				denormTable.Name,
				columnConfig.Name));
			}

			if (!string.IsNullOrEmpty(denormTable.CustomIndex))
			{
				var pattern = @"CREATE\s.*\sINDEX\s\[?(?<indexName>[^\[\]]+)\]?\sON\s\[?(?<schemaName>[^\[\]]+)\]?\.\[?(?<tableName>[^\[\]]+)\]?\s";
				var result = Regex.Match(denormTable.CustomIndex, pattern, RegexOptions.IgnoreCase);

				dropQueries.Add(String.Format(CultureInfo.InvariantCulture, $@"DROP INDEX IF EXISTS {result.Result("[${indexName}]")} ON {result.Result("[${schemaName}].[${tableName}]")}"));
			}

			return String.Join("\r\n", dropQueries);
		}

		public string GetDropAndCreateIncrementalLoadQueryForDenormalizedTable(EdwDenormalizedTableConfigRow denormTable)
		{
			return String.Format(CultureInfo.InvariantCulture,
@"IF EXISTS
	(SELECT NULL FROM sys.procedures p INNER JOIN sys.schemas s ON p.schema_id = s.schema_id
		WHERE p.name = '{3}{0}' AND s.name = '{1}')
	DROP PROCEDURE [{1}].[{3}{0}]

GO

{2}

GO",
				denormTable.Name,
				denormTable.Schema,
				GetIncrementalLoadQueryForDenormalizedTable(denormTable),
				BiConstants.EdwAggregateIncrementalLoadProcPrefix
			);
		}

		string GetTableListForSchema(string schemaName, IEnumerable<SourceTableDefinition> sourceTableList)
		{
			return string.Join("', '", sourceTableList.Where(table => table.Schema == schemaName).Select(table => table.Table));
		}

		string GetSqlTextForIncParitioning(EdwDenormalizedTableConfigRow denormTable)
		{
			var partitionKey = denormTable.GetEdwDenormalizedColumnConfigRows().Where(k => k.IsPartitionKey).Select(k => k.Name).FirstOrDefault();
			if (partitionKey == null)
			{
				return "";
			}
			var result = @$"
	DECLARE @ResultCode INT, @ErrorMessage NVARCHAR(MAX), @ErrorCode INT
	EXEC Transform.usp_PartitionTable @LoadType = 2, @SchemaName = '{denormTable.Schema}', @TableName = '{denormTable.Name}', @PartitionColumnName = '{partitionKey}', 
			@ErrorMessage = @ErrorMessage OUTPUT, @ResultCode = @ResultCode OUTPUT, @ErrorCode = @ErrorCode OUTPUT, @NumberOfFuturePeriods = @number_of_future_periods_for_partitioning
	IF @ResultCode NOT IN (1, 9) BEGIN SET @ErrorMessage = '[' + CAST(@ResultCode AS VARCHAR(5)) + ',PartitionTable(2)] ' + @ErrorMessage; RAISERROR (@ErrorMessage, 16, 2); RETURN; END
";
			return result;
		}

		public string GetIncrementalLoadQueryForDenormalizedTable(EdwDenormalizedTableConfigRow denormTable)
		{
			string sqlText = null;

			if (!denormTable.HasErrors)
			{
				try
				{
					var sourceTableList = ParseSourceTables(denormTable.Expression);
					if (sourceTableList.Any())
					{
						var foreignKeyTableReference = GetForeignKeyTableReference(denormTable, sourceTableList);
						var sqlTextForPartitioning = GetSqlTextForIncParitioning(denormTable);
						var relevantSchemas = sourceTableList.Select(x => x.Schema).Distinct();
						var transformedRowPredicate = String.Join("\r\n\t\t\tOR ", relevantSchemas.Select(schemaName => $"(SchemaName = '{schemaName}' AND TableName IN ('{GetTableListForSchema(schemaName, sourceTableList)}'))"));

						sqlText = @$"CREATE PROCEDURE [{denormTable.Schema}].[{BiConstants.EdwAggregateIncrementalLoadProcPrefix}{denormTable.Name}]

@number_of_future_periods_for_partitioning INT = 6

AS

BEGIN
	SET NOCOUNT ON

	--Do nothing if there are no any changes in underlying base tables
	IF NOT EXISTS
		(SELECT 1 FROM [biadmin].[TransformedRow] WHERE
			{transformedRowPredicate}
		)
	BEGIN
		UPDATE [biadmin].[ModelTableState]
		SET 
			CurrentState = 'Transforming (skipped)',
			IncrementalDeleteRecordCount = 0,
			IncrementalDeleteDurationMs = 0,
			IncrementalInsertRecordCount = 0,
			IncrementalInsertDurationMs = 0,
			StateModifiedTimestamp = GETDATE()
		WHERE ModelTableName = '{denormTable.Name}' and ModelSchemaName = '{denormTable.Schema}'

		RETURN
	END
{sqlTextForPartitioning}
	DECLARE @temp INT = (SELECT TOP (1) null FROM [{denormTable.Schema}].[{denormTable.Name}] WITH (UPDLOCK, TABLOCK));

	DECLARE @DelRowCount BIGINT = 0, @InsRowCount BIGINT = 0, @DT1 DATETIME, @DT2 DATETIME, @DelDateDiff BIGINT
	DECLARE @MaxID BIGINT = 0

	SELECT @MaxID = ISNULL(MAX([{denormTable.BaseName}Key]), 0) FROM [{denormTable.Schema}].[{denormTable.Name}]

{GetIncLoadQueries(denormTable, foreignKeyTableReference)}

END";
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
				}
			}

			return sqlText;
		}

		string GetIncLoadQueries(EdwDenormalizedTableConfigRow denormTable, Dictionary<string, SourceTableDefinition> foreignKeyTableReference)
		{
			return string.Join("\r\n", new string[]
			{
				GetChangeKeysTableQuery(denormTable, foreignKeyTableReference),
				GetPrepareNullKeysTableQuery(denormTable, foreignKeyTableReference),
				GetCleanNullKeysTableQuery(denormTable, foreignKeyTableReference),
				GetDeleteModifiedRowsQuery(denormTable, foreignKeyTableReference),
				GetReinsertModifiedAndNewRowsQuery(denormTable, foreignKeyTableReference),
				GetPrepareKeysTableQuery(denormTable, foreignKeyTableReference),
				GetDeleteNullNeighborsQuery(denormTable, foreignKeyTableReference),
				GetMergePartitionDatesQuery(denormTable)
			});
		}

		Dictionary<string, SourceTableDefinition> GetForeignKeyTableReference(EdwDenormalizedTableConfigRow denormTable, IEnumerable<SourceTableDefinition> sourceTableList)
		{
			List<string> foreignKeys = new List<string>();
			var denormColumns = denormTable.GetEdwDenormalizedColumnConfigRows();
			var foreignKeyTableReference = new Dictionary<string, SourceTableDefinition>();
			foreach (var baseTable in sourceTableList)
			{
				var regex = new Regex(String.Format(@"^\[?{0}\]?\.?\[?{1}Key\]?$", baseTable.Alias, baseTable.Table.Replace(BiConstants.EdwBaseTablePrefix, "")), RegexOptions.IgnoreCase);
				var foreignKey = denormColumns.Where(c => regex.Match(c.Expression).Success).FirstOrDefault();
				if (foreignKey != null)
				{
					foreignKeys.Add(foreignKey.Name);
					foreignKeyTableReference[foreignKey.Name] = baseTable;
				}
				else
				{
					throw new BiConfigurationException(String.Format("Soft key for [{0}].[{1}] not found in aggregate table [{2}].[{3}].", baseTable.Schema, baseTable.Table, denormTable.Schema, denormTable.Table));
				}
			}

			return foreignKeyTableReference;
		}

		string GetChangeKeysTableQuery(EdwDenormalizedTableConfigRow denormTable, Dictionary<string, SourceTableDefinition> foreignKeyTableReference)
		{
			var foreignKeys = foreignKeyTableReference.Keys;
			var foreignKeysCreate = string.Join(", ", foreignKeys.Select(k => string.Format(CultureInfo.InvariantCulture, "{0} BIGINT", k)));
			var foreignKeysCols = string.Join(", ", foreignKeys.Select(k => string.Format(CultureInfo.InvariantCulture, "{0}", k)));
			var foreignKeysColsWithReference = string.Join(", ", foreignKeys.Select(k => string.Format(CultureInfo.InvariantCulture, "a.{0}", k)));

			List<string> subqueries = new List<string>();
			foreach (var foreignKey in foreignKeys)
			{
				var baseTable = foreignKeyTableReference[foreignKey];
				subqueries.Add(string.Format(CultureInfo.InvariantCulture, @"
	SELECT {0}, tr.[SchemaName], tr.[TableName]
	FROM [{1}].[{2}] a with (nolock)
	INNER JOIN [{3}].[TransformedRow] tr
	ON
	(tr.[SchemaName] = '{4}' AND tr.[TableName] = '{5}' AND tr.[KeyValue] = a.{6} AND tr.PkValue IS NOT NULL)",
					/*0*/ foreignKeysColsWithReference,
					/*1*/ denormTable.Schema,
					/*2*/ denormTable.Name,
					/*3*/ BiConstants.BiAdminSchemaName,
					/*4*/ baseTable.Schema,
					/*5*/ baseTable.Table,
					/*6*/ foreignKey
				));
			}

			return string.Format(CultureInfo.InvariantCulture, @"
	CREATE TABLE #ChangesKeys ({3}, SchemaName VARCHAR(128), TableName VARCHAR(128), INDEX CX_ChangesKeys_Temp_SchemaTable CLUSTERED ([SchemaName], [TableName]))

	INSERT INTO #ChangesKeys WITH (TABLOCK) --Table:Inc_{0}_{1}
		({4}, SchemaName, TableName)

	{5}

	",
				/*0*/ denormTable.Schema,
				/*1*/ denormTable.Name,
				/*2*/ BiConstants.BiAdminSchemaName,
				/*3*/ foreignKeysCreate,
				/*4*/ foreignKeysCols,
				/*5*/ string.Join("\r\n\t\tUNION ALL\r\n", subqueries)
			);
		}

		string GetPrepareNullKeysTableQuery(EdwDenormalizedTableConfigRow denormTable, Dictionary<string, SourceTableDefinition> foreignKeyTableReference)
		{
			var foreignKeys = foreignKeyTableReference.Keys;
			var foreignKeysCols = string.Join(", ", foreignKeys.Select(k => string.Format(CultureInfo.InvariantCulture, "{0}", k)));
			List<string> subqueries = new List<string>();
			foreach (var foreignKey in foreignKeys)
			{
				List<string> onMembers = new List<string>();
				foreach (var otherKey in foreignKeys.Except(new[] { foreignKey }))
				{
					var baseTable = foreignKeyTableReference[otherKey];

					onMembers.Add(String.Format(CultureInfo.InvariantCulture, @"
			(vw.[{0}] IS NULL AND Q.[SchemaName] = '{1}' AND Q.[TableName] = '{2}')",
						otherKey,
						baseTable.Schema,
						baseTable.Table));
				}

				var subquery = String.Format(CultureInfo.InvariantCulture, @"
		--For {0}:
		SELECT {1}
		FROM [{2}].[{3}{4}] vw
		INNER JOIN
		(SELECT DISTINCT {0}, SchemaName, TableName FROM #ChangesKeys) Q
		ON
		Q.[{0}] = vw.[{0}] AND
		({5}
		)
",
				/*0*/ foreignKey,
				/*1*/ String.Join(", ", foreignKeys.Select(k => String.Format(CultureInfo.InvariantCulture, "vw.[{0}]", k))),
				/*2*/ denormTable.Schema,
				/*3*/ BiConstants.EdwAggregateViewPrefix,
				/*4*/ denormTable.Name,
				/*5*/ String.Join("\r\n\t\t\tOR", onMembers)
				);

				subqueries.Add(subquery);
			}

			string sqlText = String.Format(CultureInfo.InvariantCulture, @"
	--Step #1.1 Preparation of 'NullKeys' table

	UPDATE [{2}].[ModelTableState]
	SET 
		CurrentState = 'Transforming - (prepare NULL Key table)',
		StateModifiedTimestamp = GETDATE()

	WHERE ModelTableName = '{1}' and ModelSchemaName = '{0}'
	CREATE TABLE #NullKeys ({3})

	INSERT INTO #NullKeys WITH (TABLOCK) --Table:Inc_{0}_{1}
		({4})
	SELECT DISTINCT {5}
	FROM
	({6}
	) UT
",
				/*0*/ denormTable.Schema,
				/*1*/ denormTable.Name,
				/*2*/ BiConstants.BiAdminSchemaName,
				/*3*/ String.Join(", ", foreignKeys.Select(k => String.Format(CultureInfo.InvariantCulture, "[{0}] BIGINT NULL", k))),
				/*4*/ String.Join(", ", foreignKeys.Select(k => String.Format(CultureInfo.InvariantCulture, "[{0}]", k))),
				/*5*/ String.Join(", ", foreignKeys.Select(k => String.Format(CultureInfo.InvariantCulture, "UT.[{0}]", k))),
				/*6*/ String.Join("\r\n\t\tUNION ALL\r\n", subqueries)
			);
			return sqlText;
		}

		string GetCleanNullKeysTableQuery(EdwDenormalizedTableConfigRow denormTable, Dictionary<string, SourceTableDefinition> foreignKeyTableReference)
		{
			var onMembers = new List<string>();
			foreach (var foreignKey in foreignKeyTableReference.Keys)
			{
				onMembers.Add(String.Format(CultureInfo.InvariantCulture, @"
		(a.[{0}] = k.[{0}] OR (a.[{0}] IS NULL AND k.[{0}] IS NULL))",
				foreignKey));
			}

			string sqlText = String.Format(CultureInfo.InvariantCulture, @"
	--Step #1.2 Cleaning 'NullKeys' table

	UPDATE [{2}].[ModelTableState]
	SET 
		CurrentState = 'Transforming - (cleaning NULL Key table)',
		StateModifiedTimestamp = GETDATE()
	WHERE ModelTableName = '{1}' and ModelSchemaName = '{0}'

	DELETE k --Table:Inc_{0}_{1}
	FROM #NullKeys k
	INNER JOIN [{0}].[{1}] a
	ON
	({3}
	)
",
				denormTable.Schema,
				denormTable.Name,
				BiConstants.BiAdminSchemaName,
				String.Join("\r\n\t\t\tAND", onMembers)
			);

			return sqlText;
		}

		string GetDeleteModifiedRowsQuery(EdwDenormalizedTableConfigRow denormTable, Dictionary<string, SourceTableDefinition> foreignKeyTableReference)
		{
			var partitionKey = denormTable.GetEdwDenormalizedColumnConfigRows().Where(k => k.IsPartitionKey).Select(k => k.Name).FirstOrDefault();

			var deleteQueries = new List<string>();
			foreach (var foreignKey in foreignKeyTableReference.Keys)
			{
				var baseTable = foreignKeyTableReference[foreignKey];
				deleteQueries.Add(String.Format(CultureInfo.InvariantCulture, @"
	DELETE at --Table:Inc_{0}_{1}
	OUTPUT '{0}', '{1}', DELETED.[{3}Key]{4}
	INTO [{2}].[TransformedRow] ([SchemaName], [TableName], [KeyValue]{5})
	FROM [{0}].[{1}] at
	INNER JOIN [{2}].[TransformedRow] tr1
	ON tr1.[SchemaName] = '{6}' AND tr1.[TableName] = '{7}' AND [tr1].[KeyValue] = at.[{8}] AND tr1.PkValue IS NOT NULL

	SET @DelRowCount = @@ROWCOUNT + @DelRowCount",
						/*0*/ denormTable.Schema,
						/*1*/ denormTable.Name,
						/*2*/ BiConstants.BiAdminSchemaName,
						/*3*/ denormTable.BaseName,
						/*4*/ (partitionKey != null) ? String.Format(CultureInfo.InvariantCulture, ", ISNULL(DELETED.[{0}], '1900-01-01')", partitionKey) : "",
						/*5*/ (partitionKey != null) ? ", [CreateDate]" : "",
						/*6*/ baseTable.Schema,
						/*7*/ baseTable.Table,
						/*8*/ foreignKey
					)
				);
			}

			string sqlText = String.Format(CultureInfo.InvariantCulture,
@"	--Step #2 Delete modified rows

	UPDATE [{2}].[ModelTableState]
	SET 
		CurrentState = 'Transforming - Delete',
		StateModifiedTimestamp = GETDATE()
	WHERE ModelTableName = '{1}' and ModelSchemaName = '{0}'

	SET @DT1 = GETDATE()
{4}

	SET @DT2 = GETDATE()
	SET @DelDateDiff = DATEDIFF(MS, @DT1, @DT2)
",
				denormTable.Schema,
				denormTable.Name,
				BiConstants.BiAdminSchemaName,
				denormTable.BaseName,
				String.Join("\r\n", deleteQueries)
			);
			return sqlText;
		}

		string GetReinsertModifiedAndNewRowsQuery(EdwDenormalizedTableConfigRow denormTable, Dictionary<string, SourceTableDefinition> foreignKeyTableReference)
		{
			var fromClause = new List<string>();
			var leftJoinClause = new List<string>();
			var foreignKeys = foreignKeyTableReference.Keys;
			foreach (var foreignKey in foreignKeys)
			{
				var baseTable = foreignKeyTableReference[foreignKey];
				fromClause.Add(String.Format(CultureInfo.InvariantCulture, @"
		SELECT {4}
		FROM [{0}].[{2}{1}] vw
		INNER JOIN [{3}].[TransformedRow] tr
		ON tr.[KeyValue] = vw.[{5}] and [tr].[SchemaName] = '{6}' and tr.TableName = '{7}'  and tr.PkValue IS NULL
",
						denormTable.Schema,
						denormTable.Name,
						BiConstants.EdwAggregateViewPrefix,
						BiConstants.BiAdminSchemaName,
						String.Join(", ", foreignKeys.Select(k => String.Format(CultureInfo.InvariantCulture, "vw.[{0}]", k))),
						foreignKey,
						baseTable.Schema,
						baseTable.Table
					)
				);

				leftJoinClause.Add(String.Format(CultureInfo.InvariantCulture,
					@"LEFT JOIN [{0}].[{1}] AS {2} ON {2}.[{3}Key] = keys.[{4}]",
						baseTable.Schema,
						baseTable.Table,
						baseTable.Alias,
						baseTable.Table.Replace(BiConstants.EdwBaseTablePrefix, ""),
						foreignKey
					)
				);
			}

			var denormColumns = denormTable.GetEdwDenormalizedColumnConfigRows().Where(c => c.Name != denormTable.BaseName + "Key").OrderBy(c => c.Name);
			var partitionKey = denormColumns.Where(c => c.IsPartitionKey).Select(c => c.Name).FirstOrDefault();

			string sqlText = String.Format(CultureInfo.InvariantCulture,
@"	--STEP #3. Reinsert modified lines + new lines

	UPDATE [{2}].[ModelTableState]
	SET 
		CurrentState = 'Transforming - Insert',
		IncrementalDeleteRecordCount = @DelRowCount,
		IncrementalDeleteDurationMs = @DelDateDiff,
		StateModifiedTimestamp = GETDATE()
	WHERE ModelTableName = '{1}' and ModelSchemaName = '{0}'
	
	SET @DT1 = GETDATE()

	DECLARE @N BIGINT = 9223372036854775807

	INSERT INTO [{0}].[{1}] WITH (TABLOCK) --Table:Inc_{0}_{1}
		([{4}Key], [{3}])
	OUTPUT '{0}', '{1}', INSERTED.[{4}Key]{5}
	INTO [{2}].[TransformedRow] ([SchemaName], [TableName], [KeyValue]{6})
	SELECT TOP (@N) 
		@MaxID + ROW_NUMBER() OVER (ORDER BY (SELECT 1)), 
		{7}
	FROM
	({8}
		UNION

		SELECT {9}
		FROM #NullKeys nk
	) keys
	{10}

	SET @InsRowCount = @@ROWCOUNT
	SET @DT2 = GETDATE()
",
				denormTable.Schema,
				denormTable.Name,
				BiConstants.BiAdminSchemaName,
				String.Join("], [", denormColumns.Select(c => c.Name)),
				denormTable.BaseName,
				(partitionKey != null) ? String.Format(", ISNULL(INSERTED.[{0}], '1900-01-01')", partitionKey) : "",
				(partitionKey != null) ? ", [CreateDate]" : "",
				String.Join(",\r\n\t\t", denormColumns.Select(c => String.Format(CultureInfo.InvariantCulture, "[{0}] = {1}", c.Name, c.Expression))),
				String.Join("\r\n\t\tUNION\r\n", fromClause),
				String.Join(", ", foreignKeys.Select(k => String.Format(CultureInfo.InvariantCulture, "nk.[{0}]", k))),
				String.Join("\r\n\t", leftJoinClause)
			);
			return sqlText;
		}

		string GetPrepareKeysTableQuery(EdwDenormalizedTableConfigRow denormTable, Dictionary<string, SourceTableDefinition> foreignKeyTableReference)
		{
			var foreignKeys = foreignKeyTableReference.Keys;
			string sqlText = String.Format(CultureInfo.InvariantCulture,
@"	--STEP #4.1 Prepare Keys table

	UPDATE [{2}].[ModelTableState]
	SET 
		CurrentState = 'Transforming - Delete NULL neighbours',
		IncrementalInsertRecordCount = @InsRowCount,
		IncrementalInsertDurationMs = DATEDIFF(MS, @DT1, @DT2),
		StateModifiedTimestamp = GETDATE()
	WHERE ModelTableName = '{1}' and ModelSchemaName = '{0}'
	
	SET @DT1 = GETDATE()

	CREATE TABLE #LocalKeys ([{3}Key] BIGINT PRIMARY KEY, {4})

	INSERT INTO #LocalKeys WITH (TABLOCK) --Table:Inc_{0}_{1}
		([{3}Key], {5})
	SELECT [at_ins].[{3}Key], {5}
	FROM [{0}].[{1}] [at_ins]
	INNER JOIN [{2}].[TransformedRow] [tr]
	ON [tr].[SchemaName] = '{0}' AND [tr].[TableName] = '{1}' AND [tr].[KeyValue] = [at_ins].[{3}Key]
",
				denormTable.Schema,
				denormTable.Name,
				BiConstants.BiAdminSchemaName,
				denormTable.BaseName,
				String.Join(", ", foreignKeys.Select(k => String.Format(CultureInfo.InvariantCulture, "[{0}] BIGINT NULL", k))),
				String.Join(", ", foreignKeys.Select(k => String.Format(CultureInfo.InvariantCulture, "[at_ins].[{0}]", k)))
			);
			return sqlText;
		}

		string GetDeleteNullNeighborsQuery(EdwDenormalizedTableConfigRow denormTable, Dictionary<string, SourceTableDefinition> foreignKeyTableReference)
		{
			var deleteQueries = new List<string>();
			var partitionKey = denormTable.GetEdwDenormalizedColumnConfigRows().Where(c => c.IsPartitionKey).Select(c => c.Name).FirstOrDefault();
			var foreignKeys = foreignKeyTableReference.Keys;
			foreach (var foreignKey in foreignKeys)
			{
				var whereClause = new List<string>();
				foreach (var selectedKey in foreignKeys.Except(new[] { foreignKey }))
				{
					var andClause = new List<string>();
					foreach (var otherKey in foreignKeys.Except(new[] { foreignKey, selectedKey }))
					{
						andClause.Add(String.Format(CultureInfo.InvariantCulture, "([k].[{0}] = [at].[{0}] OR [at].[{0}] IS NULL)", otherKey));
					}
					whereClause.Add(String.Format(CultureInfo.InvariantCulture, @"
		(
			[at].[{0}] IS NULL
			AND [k].[{0}] IS NOT NULL{1}{2}
		)",
							selectedKey,
							andClause.Any() ? "\r\n\t\t\tAND " : "",
							String.Join("\r\n\t\t\tAND ", andClause)
						)
					);
				}

				var baseTable = foreignKeyTableReference[foreignKey];
				deleteQueries.Add(String.Format(CultureInfo.InvariantCulture, @"
	--For {0}
	DELETE [at] --Table:Inc_{1}_{2}
	OUTPUT '{1}', '{2}', DELETED.[{3}Key]{4}
	INTO [{5}].[TransformedRow] ([SchemaName], [TableName], [KeyValue]{6})
	FROM [{1}].[{2}] [at]
	INNER JOIN #LocalKeys k ON [at].[{0}] = [k].[{0}]
	WHERE
	({7}
	)

	SET @DelRowCount = @DelRowCount + @@ROWCOUNT",
						foreignKey,
						denormTable.Schema,
						denormTable.Name,
						denormTable.BaseName,
						(partitionKey != null) ? String.Format(CultureInfo.InvariantCulture, ", ISNULL(DELETED.[{0}], '1900-01-01')", partitionKey) : "",
						BiConstants.BiAdminSchemaName,
						(partitionKey != null) ? ", CreateDate" : "",
						String.Join("\r\n\t\tOR", whereClause)
					)
				);
			}

			string sqlText = String.Format(CultureInfo.InvariantCulture,
@"	--STEP #4.2 Delete NULL neighbors

{0}

	SET @DT2 = GETDATE();
",
				String.Join("\r\n", deleteQueries)
			);
			return sqlText;
		}

		string GetMergePartitionDatesQuery(EdwDenormalizedTableConfigRow denormTable)
		{
			var partitionKey = denormTable.GetEdwDenormalizedColumnConfigRows().Where(c => c.IsPartitionKey).Select(c => c.Name).FirstOrDefault();
			string sqlText = String.Format(CultureInfo.InvariantCulture,
@"	--STEP #5 Merge partition dates

	UPDATE [{2}].[ModelTableState]
	SET 
		CurrentState = 'Transforming (merge partition dates)',
		IncrementalDeleteRecordCount = @DelRowCount,
		IncrementalDeleteDurationMs = DATEDIFF(MS, @DT1, @DT2) + @DelDateDiff,
		StateModifiedTimestamp = GETDATE()
	WHERE ModelTableName = '{1}' and ModelSchemaName = '{0}'

	IF NOT EXISTS
		(SELECT 1 
		FROM [{2}].[SsasPartitionUnprocessedDate] 
		WHERE SchemaName = '{0}' and TableName = '{1}' AND CreateDate IS NULL)
	BEGIN
		INSERT INTO [{2}].[SsasPartitionUnprocessedDate] ([SchemaName], [TableName], [CreateDate])
		SELECT DISTINCT tr.[SchemaName], tr.[TableName], tr.[CreateDate]
		FROM [{2}].[TransformedRow] tr
		LEFT JOIN [{2}].[SsasPartitionUnprocessedDate] ud
			ON tr.[SchemaName] = ud.[SchemaName] AND tr.[TableName] = ud.[TableName] AND tr.[CreateDate] = ud.[CreateDate]
		WHERE tr.SchemaName = '{0}' AND tr.TableName = '{1}' AND ud.SchemaName IS NULL
	END
",
			denormTable.Schema,
			denormTable.Name,
			BiConstants.BiAdminSchemaName
			);
			return sqlText;
		}

		#endregion

		#endregion

		#region EDW Custom Tables

		partial class EdwCustomTableConfigDataTable
		{
			protected override void OnColumnChanged(DataColumnChangeEventArgs e)
			{
				base.OnColumnChanged(e);
				if (validationRequired && e.Row.RowState != DataRowState.Detached && e.Column.ColumnName != "Message")
				{
					SuspendValidation();
					((EdwCustomTableConfigRow)e.Row).Validate();
					ResumeValidation();
				}
			}

			public void SuspendValidation()
			{
				validationRequired = false;
			}

			public void ResumeValidation()
			{
				validationRequired = true;
			}

			bool validationRequired = true;
		}

		partial class EdwCustomColumnConfigDataTable
		{
			protected override void OnColumnChanged(DataColumnChangeEventArgs e)
			{
				base.OnColumnChanged(e);
				if (validationRequired && e.Row.RowState != DataRowState.Detached && e.Column.ColumnName != "Message")
				{
					SuspendValidation();
					((EdwCustomColumnConfigRow)e.Row).Validate();
					ResumeValidation();
				}
			}

			public void SuspendValidation()
			{
				validationRequired = false;
			}

			public void ResumeValidation()
			{
				validationRequired = true;
			}

			bool validationRequired = true;
		}

		partial class EdwCustomTableConfigRow
		{
			public string ViewSchema
			{
				get
				{
					string viewSchema = null;

					if (!string.IsNullOrWhiteSpace(CreateViewQuery))
					{
						var regex = new Regex(@"CREATE\s+VIEW\s*\[*(?<viewSchema>.+?)\]*\.", RegexOptions.IgnoreCase);
						var match = regex.Match(CreateViewQuery);
						if (match.Success)
						{
							viewSchema = match.Groups["viewSchema"].Value;
						}
					}

					return viewSchema;
				}
			}

			public string ViewName
			{
				get
				{
					string viewName = "";

					if (!string.IsNullOrWhiteSpace(CreateViewQuery))
					{
						var regex = new Regex(@"CREATE\s+VIEW\s*\[*.+?\]*\.\[*(?<viewName>\w.*\w)\]*", RegexOptions.IgnoreCase);
						var match = regex.Match(CreateViewQuery);
						if (match.Success)
						{
							viewName = match.Groups["viewName"].Value;
						}
					}

					return viewName;
				}
			}

			public string GetTableDependencyList()
			{
				var tableDependencyList = new List<string>();

				if (RunBeforeTransform)
				{
					var configData = (BiAutomationConfigDataSet)Table.DataSet;
					tableDependencyList.AddRange(configData.EdwTableConfig.Where(t => t.GetEdwColumnConfigRows().Any(c => c.ParentTable == Name)).Select(t => "[" + t.Schema + "].[" + t.Name + "]").ToList());
				}
				else
				{
					AddBaseTableDependencyListFromQuery(tableDependencyList, CreateViewQuery);
					AddBaseTableDependencyListFromQuery(tableDependencyList, InitialLoadQuery);
					AddBaseTableDependencyListFromQuery(tableDependencyList, IncrementalLoadQuery);
				}

				return String.Join(",", tableDependencyList);
			}

			void AddBaseTableDependencyListFromQuery(List<string> tableDependencyList, string query)
			{
				var regex = new Regex(String.Format(CultureInfo.InvariantCulture, @"\[*(?<Schema>\w+)\]*\.\[*(?<Table>{0}\w+)\]*", BiConstants.EdwBaseTablePrefix), RegexOptions.IgnoreCase);
				foreach (Match match in regex.Matches(query))
				{
					var schema = match.Groups["Schema"].Value;
					var table = match.Groups["Table"].Value;
					var tableDependency = "[" + schema + "].[" + table + "]";

					if (!tableDependencyList.Contains(tableDependency))
					{
						tableDependencyList.Add(tableDependency);
					}
				}
			}

			void AddAggregateTableDependencyListFromQuery(List<string> tableDependencyList, string query)
			{
				var regex = new Regex(String.Format(CultureInfo.InvariantCulture, @"\[*(?<Schema>\w+)\]*\.\[*(?<Table>{0}\w+)\]*", BiConstants.EdwAggregateTablePrefix), RegexOptions.IgnoreCase);
				foreach (Match match in regex.Matches(query))
				{
					var schema = match.Groups["Schema"].Value;
					var table = match.Groups["Table"].Value;
					var tableDependency = "[" + schema + "].[" + table + "]";

					if (!tableDependencyList.Contains(tableDependency))
					{
						tableDependencyList.Add(tableDependency);
					}
				}
			}

			public List<EdwCustomTableConfigRow> ParseCustomTablesFromQueries()
			{
				var dataSet = (BiAutomationConfigDataSet)Table.DataSet;
				var dependencyList = new List<EdwCustomTableConfigRow>();

				if (!string.IsNullOrEmpty(CreateViewQuery) ||
					!string.IsNullOrEmpty(InitialLoadQuery) ||
					!string.IsNullOrEmpty(IncrementalLoadQuery))
				{
					foreach (var customTable in dataSet.EdwCustomTableConfig.Where(t => t.Name != Name))
					{
						var regex = new Regex(String.Format(CultureInfo.InvariantCulture, @"\b{0}\b", customTable.Name), RegexOptions.IgnoreCase);
						var truncateRegex = new Regex(String.Format(CultureInfo.InvariantCulture, @"TRUNCATE\s+TABLE\s+\[*{0}\]*\.\[*{1}\]*", customTable.Schema, customTable.Name), RegexOptions.IgnoreCase);
						if ((regex.IsMatch(CreateViewQuery) || regex.IsMatch(InitialLoadQuery) || regex.IsMatch(IncrementalLoadQuery)) && !truncateRegex.IsMatch(InitialLoadQuery))
						{
							dependencyList.Add(customTable);
						}
					}
				}
				else
				{
					foreach (var customTable in dataSet.EdwCustomTableConfig.Where(t => t.Name != Name))
					{
						var regex = new Regex(String.Format(CultureInfo.InvariantCulture, @"TRUNCATE\s+TABLE\s+\[*{0}\]*\.\[*{1}\]*", Schema, Name), RegexOptions.IgnoreCase);
						if (regex.IsMatch(customTable.InitialLoadQuery))
						{
							dependencyList.Add(customTable);
						}
					}

					if (dependencyList.Count > 1)
					{
						throw new BiConfigurationException(String.Format(CultureInfo.InvariantCulture, "[{0}].[{1}] shouldn't be populated by multiple tables.", Schema, Name));
					}
				}

				return dependencyList;
			}

			public void Validate()
			{
				var configData = (BiAutomationConfigDataSet)Table.DataSet;
				var tableMessage = new StringBuilder();
				var columns = GetEdwCustomColumnConfigRows();
				var regex = new Regex("[^a-zA-Z0-9_]");

				if (string.IsNullOrEmpty(Schema))
				{
					tableMessage.AppendLine("Schema cannot be empty.");
				}
				else if (regex.Match(Schema).Success)
				{
					tableMessage.AppendLine("Schema can only contain letters, numbers, and underscores.");
				}

				if (string.IsNullOrEmpty(Name))
				{
					tableMessage.AppendLine("Name cannot be empty.");
				}
				else if (regex.Match(Name).Success)
				{
					tableMessage.AppendLine("Name can only contain letters, numbers, and underscores.");
				}
				else if (Name.StartsWith(BiConstants.EdwBaseTablePrefix))
				{
					tableMessage.AppendLine(String.Format(CultureInfo.InvariantCulture, "Name cannot start with '{0}'.", BiConstants.EdwBaseTablePrefix));
				}
				else if (Name.StartsWith(BiConstants.EdwAggregateTablePrefix))
				{
					tableMessage.AppendLine(String.Format(CultureInfo.InvariantCulture, "Name cannot start with '{0}'.", BiConstants.EdwAggregateTablePrefix));
				}
				else if (Name.StartsWith(BiConstants.EdwAggregateViewPrefix))
				{
					tableMessage.AppendLine(String.Format(CultureInfo.InvariantCulture, "Name cannot start with '{0}'.", BiConstants.EdwAggregateViewPrefix));
				}
				else if (Name.StartsWith(BiConstants.EdwModelViewPrefix))
				{
					tableMessage.AppendLine(String.Format(CultureInfo.InvariantCulture, "Name cannot start with '{0}'.", BiConstants.EdwModelViewPrefix));
				}

				if (RunBeforeTransform)
				{
					var tableDependencyList = new List<string>();
					AddBaseTableDependencyListFromQuery(tableDependencyList, CreateViewQuery);
					AddBaseTableDependencyListFromQuery(tableDependencyList, InitialLoadQuery);
					AddBaseTableDependencyListFromQuery(tableDependencyList, IncrementalLoadQuery);
					if (tableDependencyList.Any())
					{
						tableMessage.AppendLine("Queries cannot use base tables if running before transform.");
					}
				}

				{
					var tableDependencyList = new List<string>();
					AddAggregateTableDependencyListFromQuery(tableDependencyList, CreateViewQuery);
					AddAggregateTableDependencyListFromQuery(tableDependencyList, InitialLoadQuery);
					AddAggregateTableDependencyListFromQuery(tableDependencyList, IncrementalLoadQuery);
					if (tableDependencyList.Any())
					{
						tableMessage.AppendLine("Queries cannot use aggregate tables.");
					}
				}

				if (!string.IsNullOrWhiteSpace(CreateViewQuery) && string.IsNullOrWhiteSpace(ViewSchema))
				{
					tableMessage.AppendLine("View schema should be the same as table schema.");
				}

				if (columns.Any(c => c.HasErrors))
				{
					tableMessage.AppendLine("*See column error message");
				}

				if (tableMessage.Length > 0)
				{
					Message = tableMessage.ToString().TrimEnd();
					RowError = tableMessage.ToString().TrimEnd();
				}
				else
				{
					Message = null;
					RowError = null;
				}
			}
		}

		partial class EdwCustomColumnConfigRow
		{
			public EdwCustomTableConfigRow GetEdwCustomTable()
			{
				return (EdwCustomTableConfigRow)base.GetParentRow(Table.ParentRelations["FK_EdwCustomTableConfig_EdwCustomColumnConfig"]);
			}

			public void Validate()
			{
				var configData = (BiAutomationConfigDataSet)Table.DataSet;
				var columnMessage = new StringBuilder();
				var denormTable = GetEdwCustomTable();
				var regex = new Regex("[^a-zA-Z0-9_]");

				if (string.IsNullOrEmpty(Name))
				{
					columnMessage.AppendLine("Name cannot be empty.");
				}
				else if (regex.Match(Name).Success)
				{
					columnMessage.AppendLine("Name can only contain letters, numbers, and underscores.");
				}

				if (columnMessage.Length > 0)
				{
					Message = columnMessage.ToString().TrimEnd();
					RowError = columnMessage.ToString().TrimEnd();
				}
				else
				{
					Message = null;
					RowError = null;
				}

				if (denormTable != null)
				{
					denormTable.Validate();
				}
			}
		}

		#endregion

		#region Model Views

		partial class EdwModelViewTableConfigDataTable
		{
			protected override void OnColumnChanged(DataColumnChangeEventArgs e)
			{
				base.OnColumnChanged(e);
				if (validationRequired && e.Row.RowState != DataRowState.Detached && e.Column.ColumnName != "Message")
				{
					SuspendValidation();
					((EdwModelViewTableConfigRow)e.Row).Validate();
					ResumeValidation();
				}
			}

			public void SuspendValidation()
			{
				validationRequired = false;
			}

			public void ResumeValidation()
			{
				validationRequired = true;
			}

			bool validationRequired = true;
		}

		partial class EdwModelViewColumnConfigDataTable
		{
			public IEnumerable<EdwModelViewColumnConfigRow> GetDependentModelViewColumns(EdwColumnConfigRow edwColumn)
			{
				var configData = (BiAutomationConfigDataSet)DataSet;
				var result = new List<EdwModelViewColumnConfigRow>();
				var edwTable = edwColumn.GetEdwTable();
				if (edwTable != null)
				{
					var tableRegex = new Regex(String.Format(CultureInfo.InvariantCulture, @"\[*{0}\]*\.\[*{1}\]*", edwTable.Schema, edwTable.Name), RegexOptions.IgnoreCase);
					var modelViews = configData.EdwModelViewTableConfig.Where(t => tableRegex.Match(t.Expression).Success);

					if (modelViews.Any())
					{
						var columnRegex = new Regex(String.Format(CultureInfo.InvariantCulture, @"\b{0}\b", edwColumn.Name), RegexOptions.IgnoreCase);
						foreach (var modelView in modelViews)
						{
							var modelViewColumns = modelView.GetEdwModelViewColumnConfigRows().Where(c => columnRegex.Match(c.Expression).Success);
							if (modelViewColumns.Any())
							{
								result.AddRange(modelViewColumns);
							}
						}
					}
				}
				return result;
			}

			public IEnumerable<EdwModelViewColumnConfigRow> GetDependentModelViewColumns(EdwDenormalizedColumnConfigRow denormColumn)
			{
				var configData = (BiAutomationConfigDataSet)DataSet;
				var result = new List<EdwModelViewColumnConfigRow>();
				var denormTable = denormColumn.GetDenormalizedTable();
				if (denormTable != null)
				{
					var tableRegex = new Regex(String.Format(CultureInfo.InvariantCulture, @"\[*{0}\]*\.\[*{1}\]*", denormTable.Schema, denormTable.Name), RegexOptions.IgnoreCase);
					var modelViews = configData.EdwModelViewTableConfig.Where(t => tableRegex.Match(t.Expression).Success);

					if (modelViews.Any())
					{
						var columnRegex = new Regex(String.Format(CultureInfo.InvariantCulture, @"\b{0}\b", denormColumn.Name), RegexOptions.IgnoreCase);
						foreach (var modelView in modelViews)
						{
							var modelViewColumns = modelView.GetEdwModelViewColumnConfigRows().Where(c => columnRegex.Match(c.Expression).Success);
							if (modelViewColumns.Any())
							{
								result.AddRange(modelViewColumns);
							}
						}
					}
				}
				return result;
			}

			protected override void OnColumnChanged(DataColumnChangeEventArgs e)
			{
				base.OnColumnChanged(e);
				if (validationRequired && e.Row.RowState != DataRowState.Detached && e.Column.ColumnName != "Message")
				{
					SuspendValidation();
					((EdwModelViewColumnConfigRow)e.Row).Validate();
					ResumeValidation();
				}
			}

			public void SuspendValidation()
			{
				validationRequired = false;
			}

			public void ResumeValidation()
			{
				validationRequired = true;
			}

			bool validationRequired = true;
		}

		partial class EdwModelViewTableConfigRow
		{
			public string BaseName
			{
				get
				{
					return Name.Replace(BiConstants.EdwModelViewPrefix, "");
				}
			}

			public void Validate()
			{
				var tableMessage = new StringBuilder();
				var regex = new Regex("[^a-zA-Z0-9_]");

				if (string.IsNullOrEmpty(Schema))
				{
					tableMessage.AppendLine("Schema cannot be empty.");
				}
				else if (regex.Match(Schema).Success)
				{
					tableMessage.AppendLine("Schema can only contain letters, numbers, and underscores.");
				}

				if (string.IsNullOrEmpty(Name))
				{
					tableMessage.AppendLine("Name cannot be empty.");
				}
				else if (regex.Match(Name).Success)
				{
					tableMessage.AppendLine("Name can only contain letters, numbers, and underscores.");
				}

				else if (!Name.StartsWith(BiConstants.EdwModelViewPrefix))
				{
					tableMessage.AppendLine(String.Format(CultureInfo.InvariantCulture, "Name should start with '{0}'.", BiConstants.EdwModelViewPrefix));
				}

				if (string.IsNullOrEmpty(Expression))
				{
					tableMessage.AppendLine("Expression cannot be empty.");
				}
				else if (new Regex(@"\s+JOIN\s+(?=([^\]])*(?=(\[|$)))", RegexOptions.IgnoreCase).Match(Expression).Success)
				{
					tableMessage.AppendLine("Expression cannot have joins.");
				}
				else if (new Regex(@"\bWITH\b\s*\(.+\)", RegexOptions.IgnoreCase).Match(Expression).Success)
				{
					tableMessage.AppendLine("Hints are not allowed. Remove WITH expression.");
				}

				if (GetEdwModelViewColumnConfigRows().Any(c => c.HasErrors))
				{
					tableMessage.AppendLine("*See column error message");
				}

				if (tableMessage.Length > 0)
				{
					Message = tableMessage.ToString().TrimEnd();
					RowError = tableMessage.ToString().TrimEnd();
				}
				else
				{
					Message = null;
					RowError = null;
				}
			}
		}

		partial class EdwModelViewColumnConfigRow
		{
			public EdwModelViewTableConfigRow GetModelView()
			{
				return (EdwModelViewTableConfigRow)base.GetParentRow(Table.ParentRelations["FK_EdwModelViewTableConfig_EdwModelViewColumnConfig"]);
			}

			public void Validate()
			{
				var columnMessage = new StringBuilder();

				if (string.IsNullOrEmpty(Name))
				{
					columnMessage.AppendLine("Name cannot be empty.");
				}

				if (string.IsNullOrEmpty(Expression))
				{
					columnMessage.AppendLine("Expression cannot be empty.");
				}
				else
				{
					var castRegex = new Regex(@"\bCAST\b\s*\(.+?AS\s+\bDATE\b\s*\)", RegexOptions.IgnoreCase);
					var convertRegex = new Regex(@"\bCONVERT\b\s*\(\s*\bDATE\b\s*,.+?\)", RegexOptions.IgnoreCase);
					var ltrimRegex = new Regex(@"\bLTRIM\b\s*\(.+?\)", RegexOptions.IgnoreCase);
					var rtrimRegex = new Regex(@"\bRTRIM\b\s*\(.+?\)", RegexOptions.IgnoreCase);

					if (castRegex.Match(Expression).Success ||
						convertRegex.Match(Expression).Success ||
						ltrimRegex.Match(Expression).Success ||
						rtrimRegex.Match(Expression).Success)
					{
						columnMessage.AppendLine("Column expression should be created in base table level.");
					}
				}

				if (columnMessage.Length > 0)
				{
					Message = columnMessage.ToString().TrimEnd();
					RowError = columnMessage.ToString().TrimEnd();
				}
				else
				{
					Message = null;
					RowError = null;
				}

				GetModelView().Validate();
			}
		}

		#region Model View Creation

		public string GetDropAndCreateModelViewQuery(EdwModelViewTableConfigRow viewTable)
		{
			return String.Format(CultureInfo.InvariantCulture,
@"IF EXISTS
	(SELECT NULL FROM sys.views v INNER JOIN sys.schemas s ON v.schema_id = s.schema_id
		WHERE v.name = '{0}' AND s.name = '{1}')
	DROP VIEW [{1}].[{0}]

GO

{2}

GO",
				viewTable.Name,
				viewTable.Schema,
				GetCreateModelViewQuery(viewTable)
			);
		}

		public string GetCreateModelViewQuery(EdwModelViewTableConfigRow viewTable)
		{
			string sqlText = null;
			if (!viewTable.HasErrors)
			{
				try
				{
					var columnList = viewTable.GetEdwModelViewColumnConfigRows().OrderBy(c => c.Name);
					if (columnList.Any())
					{
						var columnQueryList = new List<string>();
						foreach (var column in columnList)
						{
							columnQueryList.Add(String.Format(CultureInfo.InvariantCulture, @"[{0}] = {1}", column.Name, column.Expression));
						}

						sqlText = String.Format(CultureInfo.InvariantCulture,
@"CREATE VIEW [{0}].[{1}]
WITH SCHEMABINDING AS
SELECT
	{2}
FROM {3}",
								viewTable.Schema,
								viewTable.Name,
								String.Join(",\r\n\t", columnQueryList),
								viewTable.Expression
							);
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
				}
			}
			return sqlText;
		}

		#endregion

		#endregion

		#region SSAS Objects
		partial class SsasTablesDataTable
		{
			protected override void OnColumnChanged(DataColumnChangeEventArgs e)
			{
				base.OnColumnChanged(e);
				if (validationRequired && e.Row.RowState != DataRowState.Detached && e.Column.ColumnName != "Message")
				{
					SuspendValidation();
					((SsasTablesRow)e.Row).Validate();
					ResumeValidation();
				}
			}

			public void SuspendValidation()
			{
				validationRequired = false;
			}

			public void ResumeValidation()
			{
				validationRequired = true;
			}

			bool validationRequired = true;
		}

		partial class SsasTablesRow
		{
			public void Validate()
			{
				var tableMessage = new StringBuilder();

				if (string.IsNullOrEmpty(Query))
				{
					tableMessage.AppendLine("Query cannot be empty.");
				}
				if (tableMessage.Length > 0)
				{
					Message = tableMessage.ToString().TrimEnd();
					RowError = tableMessage.ToString().TrimEnd();
				}
				else
				{
					Message = null;
					RowError = null;
				}
			}
			public SsasCubesRow GetSsasCube()
			{
				return (SsasCubesRow)base.GetParentRow(Table.ParentRelations["FK_SsasCubes_SsasTables"]);
			}
		}

		#endregion

		#region Tabular Model

		partial class LinkedTableDataTable
		{
			protected override void OnColumnChanged(DataColumnChangeEventArgs e)
			{
				base.OnColumnChanged(e);
				if (validationRequired && e.Row.RowState != DataRowState.Detached && e.Column.ColumnName != "Message")
				{
					SuspendValidation();
					((LinkedTableRow)e.Row).Validate();
					ResumeValidation();
				}
			}

			public void SuspendValidation()
			{
				validationRequired = false;
			}

			public void ResumeValidation()
			{
				validationRequired = true;
			}

			bool validationRequired = true;
		}

		partial class LinkedTableRow
		{
			public void Validate()
			{
				var tableMessage = new StringBuilder();

				if (string.IsNullOrEmpty(Name))
				{
					tableMessage.AppendLine("Name cannot be empty.");
				}

				if (string.IsNullOrEmpty(Schema))
				{
					tableMessage.AppendLine("Schema cannot be empty.");
				}

				if (tableMessage.Length > 0)
				{
					RowError = tableMessage.ToString().TrimEnd();
				}
				else
				{
					RowError = null;
				}
			}
		}

		#endregion

		#region Report

		public partial class ReportMappingTableConfigDataTable
		{
			protected override void OnColumnChanged(DataColumnChangeEventArgs e)
			{
				base.OnColumnChanged(e);
				if (validationRequired && e.Row.RowState != DataRowState.Detached && e.Column.ColumnName != "Message")
				{
					SuspendValidation();
					((ReportMappingTableConfigRow)e.Row).Validate();
					ResumeValidation();
				}
			}

			public void SuspendValidation()
			{
				validationRequired = false;
			}

			public void ResumeValidation()
			{
				validationRequired = true;
			}

			bool validationRequired = true;
		}

		public partial class ReportMappingTableConfigRow
		{
			public void Validate()
			{
				var tableMessage = new StringBuilder();

				if (string.IsNullOrEmpty(ReportName))
				{
					tableMessage.AppendLine("ReportName cannot be empty.");
				}

				if (string.IsNullOrEmpty(SourceSchema))
				{
					tableMessage.AppendLine("SourceSchema cannot be empty.");
				}

				if (string.IsNullOrEmpty(StagingTable))
				{
					tableMessage.AppendLine("StagingTable cannot be empty.");
				}

				if (tableMessage.Length > 0)
				{
					RowError = tableMessage.ToString().TrimEnd();
				}
				else
				{
					RowError = null;
				}
			}
		}

		#endregion
	}

	#endregion
}
