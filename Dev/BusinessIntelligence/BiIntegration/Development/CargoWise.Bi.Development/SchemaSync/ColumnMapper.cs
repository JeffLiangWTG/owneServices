using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Bi.Configuration.DataSets;
using CargoWise.Bi.Development.Common;

namespace CargoWise.Bi.Developement.SchemaSync
{
	public class ColumnMapper
	{
		#region Map Tabular Models to CDC tables

		public void MapTabularModelsToCdcTables()
		{
			ClearTabularModelMapping();

			foreach (var model in ConfigData.SsasCubes)
			{
				AddTabularModelRow(model.SsasModelLogicalName);
			}
		}

		void ClearTabularModelMapping()
		{
			ConfigData.LinkedTable.Clear();
			ConfigData.TabularModel.Clear();
		}

		void AddTabularModelRow(string ssasModelLogicalName)
		{
			var tabularModel = ConfigData.TabularModel.AddTabularModelRow(ssasModelLogicalName);

			foreach (var linkedTable in GetLinkedTablesForModel(ssasModelLogicalName))
			{
				ConfigData.LinkedTable.AddLinkedTableRow(tabularModel, linkedTable.Schema, linkedTable.Name);
			}
		}

		IEnumerable<LinkedTable> GetLinkedTablesForModel(string ssasModelLogicalName)
		{
			var modelViews = GetViewsFromModelQueries(ssasModelLogicalName);
			var baseTables = GetAllBaseTables(modelViews);
			var cdcTables = GetCdcTablesFromBaseTables(baseTables);

			return cdcTables;
		}

		IEnumerable<LinkedTable> GetViewsFromModelQueries(string ssasModelLogicalName)
		{
			var result = new List<LinkedTable>();

			var ssasCube = ConfigData.SsasCubes.FirstOrDefault(c => c.SsasModelLogicalName == ssasModelLogicalName);
			foreach (var ssasTable in ssasCube.GetSsasTablesRows().Where(t => !t.IsCalculated))
			{
				var views = GetTableListFromRegex(modelViewRegex, ssasTable.Query);
				result.AddRange(views);
			}

			return result.Distinct();
		}

		IEnumerable<LinkedTable> GetAllBaseTables(IEnumerable<LinkedTable> modelViews)
		{
			var customTables = GetCustomTablesFromModelViews(modelViews);
			var aggTables = GetAllDependentAggregateTables(modelViews, customTables);
			var baseTables = GetAllDependentBaseTables(modelViews, customTables, aggTables);

			return baseTables.Distinct();
		}

		#region Get Custom Tables

		IEnumerable<LinkedTable> GetCustomTablesFromModelViews(IEnumerable<LinkedTable> modelViews)
		{
			var customTables = new List<LinkedTable>();
			foreach (var modelView in modelViews)
			{
				customTables.AddRange(GetCustomTablesFromModelView(modelView));
			}

			var result = GetAllDependentCustomTablesFromCustomTableList(customTables.Distinct());
			return result;
		}

		IEnumerable<LinkedTable> GetAllDependentCustomTablesFromCustomTableList(IEnumerable<LinkedTable> customTables)
		{
			var result = new List<LinkedTable>();
			foreach (var customTable in customTables)
			{
				var dependentCustomTables = GetDependentCustomTablesFromCustomTable(customTable);
				if (dependentCustomTables.Except(customTables).Any())
				{
					result.AddRange(dependentCustomTables.Except(customTables));
				}
			}

			if (result.Any())
			{
				result.AddRange(GetAllDependentCustomTablesFromCustomTableList(result));
			}

			result.AddRange(customTables);

			return result.Distinct();
		}

		#endregion

		#region Get Aggregate Tables

		IEnumerable<LinkedTable> GetAllDependentAggregateTables(IEnumerable<LinkedTable> modelViews, IEnumerable<LinkedTable> customTables)
		{
			var aggTables = new List<LinkedTable>();

			aggTables.AddRange(GetAggregateTablesFromModelViews(modelViews));
			aggTables.AddRange(GetAggregateTablesFromCustomTables(customTables));

			var result = GetAllDependentAggregateTablesFromAggregateTableList(aggTables.Distinct());
			return result;
		}

		IEnumerable<LinkedTable> GetAggregateTablesFromModelViews(IEnumerable<LinkedTable> modelViews)
		{
			var aggTables = new List<LinkedTable>();

			foreach (var modelView in modelViews)
			{
				aggTables.AddRange(GetAggregateTablesFromModelView(modelView));
			}

			return aggTables.Distinct();
		}

		IEnumerable<LinkedTable> GetAggregateTablesFromCustomTables(IEnumerable<LinkedTable> customTables)
		{
			var aggTables = new List<LinkedTable>();

			foreach (var customTable in customTables)
			{
				aggTables.AddRange(GetAggregateTablesFromCustomTable(customTable));
			}

			return aggTables.Distinct();
		}

		IEnumerable<LinkedTable> GetAllDependentAggregateTablesFromAggregateTableList(IEnumerable<LinkedTable> aggTables)
		{
			var result = new List<LinkedTable>();
			foreach (var aggTable in aggTables)
			{
				var dependentAggTables = GetDependentAggregateTablesFromAggregateTable(aggTable);
				if (dependentAggTables.Except(aggTables).Any())
				{
					result.AddRange(dependentAggTables.Except(aggTables));
				}
			}

			if (result.Any())
			{
				result.AddRange(GetAllDependentAggregateTablesFromAggregateTableList(result));
			}

			result.AddRange(aggTables);

			return result.Distinct();
		}

		#endregion

		#region Get Base Tables

		IEnumerable<LinkedTable> GetAllDependentBaseTables(IEnumerable<LinkedTable> modelViews, IEnumerable<LinkedTable> customTables, IEnumerable<LinkedTable> aggTables)
		{
			var baseTables = new List<LinkedTable>();

			baseTables.AddRange(GetBaseTablesFromModelViews(modelViews));
			baseTables.AddRange(GetBaseTablesFromCustomTables(customTables));
			baseTables.AddRange(GetBaseTablesFromAggregateTables(aggTables));

			var result = GetAllDependentBaseTablesFromBaseTableList(baseTables.Distinct());
			return result;
		}

		IEnumerable<LinkedTable> GetBaseTablesFromModelViews(IEnumerable<LinkedTable> modelViews)
		{
			var baseTables = new List<LinkedTable>();
			foreach (var modelView in modelViews)
			{
				baseTables.AddRange(GetBaseTablesFromModelView(modelView));
			}
			return baseTables.Distinct();
		}

		IEnumerable<LinkedTable> GetBaseTablesFromCustomTables(IEnumerable<LinkedTable> customTables)
		{
			var baseTables = new List<LinkedTable>();

			foreach (var customTable in customTables)
			{
				baseTables.AddRange(GetBaseTablesFromCustomTable(customTable));
			}

			return baseTables.Distinct();
		}

		IEnumerable<LinkedTable> GetBaseTablesFromAggregateTables(IEnumerable<LinkedTable> aggTables)
		{
			var baseTables = new List<LinkedTable>();

			foreach (var aggTable in aggTables)
			{
				baseTables.AddRange(GetBaseTablesFromAggregateTable(aggTable));
			}

			return baseTables.Distinct();
		}

		IEnumerable<LinkedTable> GetAllDependentBaseTablesFromBaseTableList(IEnumerable<LinkedTable> baseTables)
		{
			var result = new List<LinkedTable>();
			foreach (var baseTable in baseTables)
			{
				var dependentBaseTables = GetDependentBaseTablesFromBaseTable(baseTable);
				if (dependentBaseTables.Except(baseTables).Any())
				{
					result.AddRange(dependentBaseTables.Except(baseTables));
				}
			}

			if (result.Any())
			{
				result.AddRange(GetAllDependentBaseTablesFromBaseTableList(result));
			}

			result.AddRange(baseTables);

			return result.Distinct();
		}

		#endregion

		#region Get list from base table

		IEnumerable<LinkedTable> GetDependentBaseTablesFromBaseTable(LinkedTable baseTable)
		{
			var edwTable = ConfigData.EdwTableConfig.First(t => t.Schema.Equals(baseTable.Schema, StringComparison.OrdinalIgnoreCase) && t.Name.Equals(baseTable.Name, StringComparison.OrdinalIgnoreCase));

			var dependentTableNameList = edwTable.GetEdwColumnConfigRows().Where(c => !string.IsNullOrEmpty(c.ParentTable)).Select(c => c.ParentTable);

			return ConfigData.EdwTableConfig.Where(t => dependentTableNameList.Contains(t.Name)).Select(t => new LinkedTable(t.Schema, t.Name));
		}

		IEnumerable<LinkedTable> GetCdcTablesFromBaseTables(IEnumerable<LinkedTable> baseTables)
		{
			var result = new List<LinkedTable>();

			foreach (var baseTable in baseTables)
			{
				var edwTable = ConfigData.EdwTableConfig.First(t => t.Schema.Equals(baseTable.Schema, StringComparison.OrdinalIgnoreCase) && t.Name.Equals(baseTable.Name, StringComparison.OrdinalIgnoreCase));
				var cdcTable = ConfigData.CdcTableConfig.First(t => t.SourceTable.Equals(edwTable.StagingTable, StringComparison.OrdinalIgnoreCase));
				var table = new LinkedTable(cdcTable.SourceSchema, cdcTable.SourceTable);
				result.Add(table);
			}

			return result.Distinct();
		}

		#endregion

		#region Get list from aggregate table

		IEnumerable<LinkedTable> GetBaseTablesFromAggregateTable(LinkedTable aggTable)
		{
			return GetTableListFromAggregateTable(aggTable, baseTableRegex);
		}

		IEnumerable<LinkedTable> GetDependentAggregateTablesFromAggregateTable(LinkedTable aggTable)
		{
			return GetTableListFromAggregateTable(aggTable, aggTableRegex);
		}

		IEnumerable<LinkedTable> GetTableListFromAggregateTable(LinkedTable aggTable, Regex regex)
		{
			var table = ConfigData.EdwDenormalizedTableConfig.First(t => t.Schema.Equals(aggTable.Schema, StringComparison.OrdinalIgnoreCase) && t.Name.Equals(aggTable.Name, StringComparison.OrdinalIgnoreCase));

			var result = GetTableListFromRegex(regex, table.Expression).Distinct();
			return result;
		}

		#endregion

		#region Get list from custom table

		IEnumerable<LinkedTable> GetBaseTablesFromCustomTable(LinkedTable customTable)
		{
			return GetTableListFromCustomTable(customTable, baseTableRegex);
		}

		IEnumerable<LinkedTable> GetAggregateTablesFromCustomTable(LinkedTable customTable)
		{
			return GetTableListFromCustomTable(customTable, aggTableRegex);
		}

		IEnumerable<LinkedTable> GetDependentCustomTablesFromCustomTable(LinkedTable customTable)
		{
			return GetTableListFromCustomTable(customTable, customTableRegex);
		}

		IEnumerable<LinkedTable> GetTableListFromCustomTable(LinkedTable customTable, Regex regex)
		{
			var table = ConfigData.EdwCustomTableConfig.First(t => t.Schema.Equals(customTable.Schema, StringComparison.OrdinalIgnoreCase) && t.Name.Equals(customTable.Name, StringComparison.OrdinalIgnoreCase));

			var result = new List<LinkedTable>();
			result.AddRange(GetTableListFromRegex(regex, table.CreateViewQuery));
			result.AddRange(GetTableListFromRegex(regex, table.InitialLoadQuery));
			result.AddRange(GetTableListFromRegex(regex, table.IncrementalLoadQuery));

			return result.Distinct();
		}

		#endregion

		#region Get list from model view

		IEnumerable<LinkedTable> GetBaseTablesFromModelView(LinkedTable modelView)
		{
			return GetTableListFromModelView(modelView, baseTableRegex);
		}

		IEnumerable<LinkedTable> GetAggregateTablesFromModelView(LinkedTable modelView)
		{
			return GetTableListFromModelView(modelView, aggTableRegex);
		}

		IEnumerable<LinkedTable> GetCustomTablesFromModelView(LinkedTable modelView)
		{
			return GetTableListFromModelView(modelView, customTableRegex);
		}

		IEnumerable<LinkedTable> GetTableListFromModelView(LinkedTable modelView, Regex regex)
		{
			var view = ConfigData.EdwModelViewTableConfig.First(v => v.Schema.Equals(modelView.Schema, StringComparison.OrdinalIgnoreCase) && v.Name.Equals(modelView.Name, StringComparison.OrdinalIgnoreCase));

			return GetTableListFromRegex(regex, view.Expression);
		}

		#endregion

		IEnumerable<LinkedTable> GetTableListFromRegex(Regex regex, string query)
		{
			var result = new List<LinkedTable>();

			foreach (Match match in regex.Matches(query))
			{
				var schema = match.Groups["Schema"].Value;
				var name = match.Groups["Name"].Value;
				var table = new LinkedTable(schema, name);

				result.Add(table);
			}

			return result.Distinct();
		}

		readonly Regex baseTableRegex = new Regex(@"\[*(?<Schema>\w+)\]*\.\[*(?<Name>BAS__\w+)\]*", RegexOptions.IgnoreCase);
		readonly Regex aggTableRegex = new Regex(@"\[*(?<Schema>\w+)\]*\.\[*(?<Name>AGG__\w+)\]*", RegexOptions.IgnoreCase);
		readonly Regex customTableRegex = new Regex(@"\[*(?<Schema>\w+)\]*\.\[*(?<Name>(CUS|GRP)__\w+)\]*", RegexOptions.IgnoreCase);
		readonly Regex modelViewRegex = new Regex(@"\[*(?<Schema>\w+)\]*\.\[*(?<Name>MDL__\w+)\]*", RegexOptions.IgnoreCase);

		class LinkedTable
		{
			public LinkedTable(string schema, string name)
			{
				Schema = schema;
				Name = name;
			}

			public override bool Equals(object obj)
			{
				var other = obj as LinkedTable;
				if (other != null)
				{
					return Schema.Equals(other.Schema, StringComparison.OrdinalIgnoreCase)
						&& Name.Equals(other.Name, StringComparison.OrdinalIgnoreCase);
				}
				else
				{
					return false;
				}
			}

			public string Schema;
			public string Name;

			public override int GetHashCode()
			{
				return Schema.GetHashCode() ^
					Name.GetHashCode();
			}
		}

		#endregion

		#region Create Mapping

		public void MapCdcColumnsToModelView()
		{
			ResetModelViewColumns();
			foreach (var cdcTable in ConfigData.CdcTableConfig.Where(t => t.TableInEdw))
			{
				foreach (var cdcColumn in cdcTable.GetCdcColumnConfigRows().Where(c => c.ColumnInEdw))
				{
					var cdcColumnRegex = new Regex(string.Format(CultureInfo.InvariantCulture, @"\b{0}\b", cdcColumn.SourceColumn), RegexOptions.IgnoreCase);
					var modelViewColumnList = new List<string>();

					foreach (var edwTable in ConfigData.EdwTableConfig.Where(t => t.SourceSchema == cdcTable.SourceSchema && t.StagingTable == cdcTable.SourceTable))
					{
						GetModelViewsMappedToBaseTable(modelViewColumnList, cdcColumnRegex, edwTable);

						foreach (var denormTable in GetAggregateTableFromSource(edwTable.Schema, edwTable.Name))
						{
							GetModelViewsMappedToAggregateTable(modelViewColumnList, cdcColumnRegex, edwTable, denormTable);
						}
					}
					cdcColumn.ModelViewColumn = string.Join(", ", modelViewColumnList);
				}
			}
		}

		void GetModelViewsMappedToBaseTable(List<string> modelViewColumnList, Regex cdcColumnRegex, BiAutomationConfigDataSet.EdwTableConfigRow edwTable)
		{
			var modelViewForBaseTable = GetModelViewFromSource(edwTable.Schema, edwTable.Name);
			if (modelViewForBaseTable != null)
			{
				var modelViewColumns = new Dictionary<string, List<BiAutomationConfigDataSet.EdwModelViewColumnConfigRow>>(StringComparer.OrdinalIgnoreCase);
				var columnRegex = new Regex(@"\w*_\w*");
				foreach (var modelViewColumn in modelViewForBaseTable.GetEdwModelViewColumnConfigRows())
				{
					foreach (Match match in columnRegex.Matches(modelViewColumn.Expression))
					{
						List<BiAutomationConfigDataSet.EdwModelViewColumnConfigRow> items;
						if (!modelViewColumns.TryGetValue(match.Value, out items))
						{
							items = new List<BiAutomationConfigDataSet.EdwModelViewColumnConfigRow>();
							modelViewColumns.Add(match.Value, items);
						}
						items.Add(modelViewColumn);
					}
				}

				foreach (var edwColumn in edwTable.GetEdwColumnConfigRows().Where(c => cdcColumnRegex.Match(c.Expression).Success))
				{
					List<BiAutomationConfigDataSet.EdwModelViewColumnConfigRow> matchingModelViewColumns;
					if (modelViewColumns.TryGetValue(edwColumn.Name, out matchingModelViewColumns))
					{
						foreach (var modelViewColumn in matchingModelViewColumns)
						{
							var column = string.Format(CultureInfo.InvariantCulture, "[{0}].[{1}].[{2}]", modelViewForBaseTable.Schema, modelViewForBaseTable.Name, modelViewColumn.Name);
							if (!modelViewColumnList.Contains(column))
							{
								modelViewColumnList.Add(column);
							}
						}
					}
				}
			}
		}

		void GetModelViewsMappedToAggregateTable(List<string> modelViewColumnList, Regex cdcColumnRegex, BiAutomationConfigDataSet.EdwTableConfigRow edwTable, AggregateTable denormTable)
		{
			var modelViewForAggregateTable = GetModelViewFromSource(denormTable.Table.Schema, denormTable.Table.Name);
			if (modelViewForAggregateTable != null)
			{
				foreach (var edwColumn in edwTable.GetEdwColumnConfigRows().Where(c => cdcColumnRegex.Match(c.Expression).Success))
				{
					var edwColumnRegex = new Regex(string.Format(CultureInfo.InvariantCulture, @"\[*\b{0}\b\]*\.*\[*\b{1}\b\]*", denormTable.BaseTableAlias, edwColumn.Name), RegexOptions.IgnoreCase);
					foreach (var denormColumn in denormTable.Table.GetEdwDenormalizedColumnConfigRows().Where(c => edwColumnRegex.Match(c.Expression).Success))
					{
						var denormColumnRegex = new Regex(string.Format(CultureInfo.InvariantCulture, @"\b{0}\b", denormColumn.Name), RegexOptions.IgnoreCase);
						foreach (var modelViewColumn in modelViewForAggregateTable.GetEdwModelViewColumnConfigRows().Where(c => denormColumnRegex.Match(c.Expression).Success))
						{
							var column = string.Format(CultureInfo.InvariantCulture, "[{0}].[{1}].[{2}]", modelViewForAggregateTable.Schema, modelViewForAggregateTable.Name, modelViewColumn.Name);
							if (!modelViewColumnList.Contains(column))
							{
								modelViewColumnList.Add(column);
							}
						}

						var aggTable = denormColumn.GetDenormalizedTable();
						foreach (var dependentDenormTable in GetAggregateTableFromSource(aggTable.Schema, aggTable.Name))
						{
							GetModelViewsMappedToDependentAggregateTable(modelViewColumnList, denormColumn, dependentDenormTable);
						}
					}
				}
			}
		}

		void GetModelViewsMappedToDependentAggregateTable(List<string> modelViewColumnList, BiAutomationConfigDataSet.EdwDenormalizedColumnConfigRow denormColumn, AggregateTable denormTable)
		{
			var modelViewForAggregateTable = GetModelViewFromSource(denormTable.Table.Schema, denormTable.Table.Name);
			if (modelViewForAggregateTable != null)
			{
				var denormColumnRegex = new Regex(string.Format(CultureInfo.InvariantCulture, @"\[*\b{0}\b\]*\.*\[*\b{1}\b\]*", denormTable.BaseTableAlias, denormColumn.Name), RegexOptions.IgnoreCase);
				foreach (var dependentDenormColumn in denormTable.Table.GetEdwDenormalizedColumnConfigRows().Where(c => denormColumnRegex.Match(c.Expression).Success))
				{
					var viewColumnRegex = new Regex(string.Format(CultureInfo.InvariantCulture, @"\b{0}\b", dependentDenormColumn.Name), RegexOptions.IgnoreCase);
					foreach (var modelViewColumn in modelViewForAggregateTable.GetEdwModelViewColumnConfigRows().Where(c => viewColumnRegex.Match(c.Expression).Success))
					{
						var column = string.Format(CultureInfo.InvariantCulture, "[{0}].[{1}].[{2}]", modelViewForAggregateTable.Schema, modelViewForAggregateTable.Name, modelViewColumn.Name);
						if (!modelViewColumnList.Contains(column))
						{
							modelViewColumnList.Add(column);
						}
					}

					var aggTable = dependentDenormColumn.GetDenormalizedTable();
					foreach (var dependentDenormTable in GetAggregateTableFromSource(aggTable.Schema, aggTable.Name))
					{
						GetModelViewsMappedToDependentAggregateTable(modelViewColumnList, dependentDenormColumn, dependentDenormTable);
					}
				}
			}
		}

		IEnumerable<AggregateTable> GetAggregateTableFromSource(string schemaName, string tableName)
		{
			var result = new List<AggregateTable>();

			foreach (var denormTable in ConfigData.EdwDenormalizedTableConfig)
			{
				foreach (var sourceTable in BiAutomationConfigDataSet.ParseSourceTables(denormTable.Expression))
				{
					if (sourceTable != null && sourceTable.Schema == schemaName && sourceTable.Table == tableName)
					{
						result.Add(new AggregateTable(denormTable, sourceTable.Alias));
					}
				}
			}

			return result;
		}

		class AggregateTable
		{
			public AggregateTable(BiAutomationConfigDataSet.EdwDenormalizedTableConfigRow denormTable, string alias)
			{
				Table = denormTable;
				BaseTableAlias = alias;
			}
			public readonly BiAutomationConfigDataSet.EdwDenormalizedTableConfigRow Table;
			public readonly string BaseTableAlias;
		}

		BiAutomationConfigDataSet.EdwModelViewTableConfigRow GetModelViewFromSource(string schemaName, string tableName)
		{
			BiAutomationConfigDataSet.EdwModelViewTableConfigRow result = null;

			foreach (var modelView in ConfigData.EdwModelViewTableConfig)
			{
				var sourceTable = BiAutomationConfigDataSet.ParseSourceTables(modelView.Expression).FirstOrDefault();
				if (sourceTable != null && sourceTable.Schema == schemaName && sourceTable.Table == tableName)
				{
					result = modelView;
					break;
				}
			}

			return result;
		}

		void ResetModelViewColumns()
		{
			foreach (var cdcColumn in ConfigData.CdcColumnConfig)
			{
				cdcColumn.ModelViewColumn = null;
			}
		}

		#endregion

		#region Display Mapping

		public IEnumerable<ViewMapping> GetViewMappingFromStringOfColumns(string input)
		{
			var result = new List<ViewMapping>();

			var columns = input.Split(new [] { '\r','\n',' ', ',' }).Where(c => !string.IsNullOrWhiteSpace(c)).Distinct();
			foreach (var columnName in columns)
			{
				var cdcColumn = ConfigData.CdcColumnConfig.FirstOrDefault(c => c.SourceColumn == columnName);
				if (cdcColumn != null)
				{
					var cdcTable = cdcColumn.GetCdcTable();
					var sourceSchema = cdcTable.SourceSchema;
					var sourceTable = cdcTable.SourceTable;
					if (!string.IsNullOrEmpty(cdcColumn.ModelViewColumn))
					{
						var modelViewColumns = cdcColumn.ModelViewColumn.Split(new[] { ',' });
						var regex = new Regex(@"\[(?<Schema>.*?)\]\.\[(?<View>.*?)\]\.\[(?<Column>.*?)\]", RegexOptions.IgnoreCase);
						foreach (var modelViewColumn in modelViewColumns)
						{
							foreach (Match match in regex.Matches(modelViewColumn))
							{
								var modelSchema = match.Groups["Schema"].Value;
								var modelView = match.Groups["View"].Value;
								var modelColumn = match.Groups["Column"].Value;

								result.Add(new ViewMapping(sourceSchema, sourceTable, columnName, cdcColumn.ColumnInEdw, modelSchema, modelView, modelColumn));
							}
						}
					}
					else
					{
						result.Add(new ViewMapping(sourceSchema, sourceTable, columnName, cdcColumn.ColumnInEdw, null, null, null));
					}
				}
				else
				{
					result.Add(new ViewMapping(null, null, columnName, false, null, null, null));
				}
			}

			return result;
		}

		public class ViewMapping
		{
			public ViewMapping(string sourceSchema, string sourceTable, string sourceColumn, bool columnInEdw, string modelSchema, string modelView, string modelColumn)
			{
				SourceSchema = sourceSchema;
				SourceTable = sourceTable;
				SourceColumn = sourceColumn;
				ColumnInEdw = columnInEdw;
				ModelSchema = modelSchema;
				ModelView = modelView;
				ModelColumn = modelColumn;
			}
			public string SourceSchema { get; private set; }
			public string SourceTable { get; private set; }
			public string SourceColumn { get; private set; }
			public bool ColumnInEdw { get; private set; }
			public string ModelSchema { get; private set; }
			public string ModelView { get; private set; }
			public string ModelColumn { get; private set; }
		}

		#endregion

		BiAutomationConfigDataSet ConfigData
		{
			get
			{
				return configData ?? (configData = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData);
			}
		}
		BiAutomationConfigDataSet configData;
	}
}
