using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using CargoWise.Bi.Development.Common;

namespace CargoWise.Bi.Development.Automation.Manager
{
	public class SearchView
	{
		#region SuppressResourceStringsCheckRegion

		public string[] TableSearchCategories = new string[] { "Action", "Source Schema", "Source Table", "Audit Schema", "Table In Audit", "Table In Edw", "Message" };

		public string[] ColumnSearchCategories = new string[] { "Action", "Source Column", "Data Type", "MaxLength", " Precision", "Scale", "Nullable", "Is Primary Key", "Cdc Enabled", "Column In Audit", "Column In Edw", "Reference Schema", "Reference Table", "Message" };

		public string[] TableActionValues = new string[] { "Unchanged", "Added", "Changed", "Deleted", "ChangedColumnOnly" };

		public string[] ColumnActionValues = new string[] { "Unchanged", "Added", "Changed", "Deleted" };

		public string[] BooleanValues = new string[] { "True", "False" };

		public List<string> GetValueList(SearchParameter parameter)
		{
			List<string> result = new List<string>();
			var columnName = parameter.Category.Replace(" ", "");
			switch (columnName)
			{
				case "Action":
					if (parameter.Type == CollectionType.TableParameters)
					{
						result.AddRange(TableActionValues);
					}
					else if (parameter.Type == CollectionType.ColumnParameters)
					{
						result.AddRange(ColumnActionValues);
					}
					break;
				case "TableInAudit":
				case "TableInEdw":
				case "CdcEnabled":
				case "ColumnInAudit":
				case "ColumnInEdw":
				case "IsPrimaryKey":
					result.AddRange(BooleanValues);
					break;
				case "SourceSchema":
				case "SourceTable":
				case "EdwSchema":
					result.AddRange(GetDistinctValuesFromTable(columnName, BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.CdcTableConfig));
					break;
				case "SourceColumn":
				case "DataType":
				case "ReferenceSchema":
				case "ReferenceTable":
					result.AddRange(GetDistinctValuesFromTable(columnName, BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.CdcColumnConfig, parameter.SourceTable));
					break;
				default:
					break;
			}
			result.Sort();
			return result;
		}

		List<string> GetDistinctValuesFromTable(string columnName, DataTable table, string sourceTable = null)
		{
			List<string> result = new List<string>();
			var defaultView = table.DefaultView;

			if (!string.IsNullOrEmpty(sourceTable))
			{
				defaultView.RowFilter = string.Format(CultureInfo.InvariantCulture, "[SourceTable] = '{0}'", sourceTable);
			}

			var rows = defaultView.ToTable(true, columnName).Rows;
			foreach (DataRow row in rows)
			{
				if (row.ItemArray[0] != DBNull.Value)
				{
					var item = (string)row.ItemArray[0];
					if (!string.IsNullOrEmpty(item))
					{
						result.Add(item);
					}
				}
			}
			return result;
		}

		public DataView Search(SearchParameterCollection collection, DataTable dataTable, string sourceTable = null)
		{
			try
			{
				DataView result = dataTable.DefaultView;
				List<string> filterList = new List<string>();

				foreach (var parameter in collection.ParameterList)
				{
					if (!string.IsNullOrEmpty(parameter.Value))
					{
						var category = parameter.Category.Replace(" ", "");
						switch (category)
						{
							case "":
								break;
							case "TableInAudit":
							case "TableInEdw":
							case "Nullable":
							case "IsPrimaryKey":
							case "CdcEnabled":
							case "ColumnInAudit":
							case "ColumnInEdw":
								filterList.Add(string.Format(CultureInfo.InvariantCulture, "[{0}] = {1}", category, parameter.Value.ToUpperInvariant()));
								break;
							case "MaxLength":
							case "Precision":
							case "Scale":
								filterList.Add(string.Format(CultureInfo.InvariantCulture, "[{0}] = {1}", category, parameter.Value));
								break;
							default:
								filterList.Add(string.Format(CultureInfo.InvariantCulture, "[{0}] LIKE '%{1}%'", category, parameter.Value));
								break;
						}
					}
				}

				if (!string.IsNullOrEmpty(sourceTable))
				{
					filterList.Add(string.Format(CultureInfo.InvariantCulture, "[SourceTable] = '{0}'", sourceTable));
				}

				result.RowFilter = string.Join(" AND ", filterList);

				return result;
			}
			catch
			{
				throw new BiAutomationException("Invalid Search Parameters");
			}
		}

		#endregion
	}

	public enum CollectionType
	{
		TableParameters,
		ColumnParameters
	}

	public class SearchParameterCollection
	{
		public SearchParameterCollection(CollectionType type)
		{
			Type = type;
		}
		public readonly CollectionType Type;

		public List<SearchParameter> ParameterList = new List<SearchParameter>();

		public void Add(string category, string value, string sourceTable)
		{
			ParameterList.Add(new SearchParameter(category, value, Type, sourceTable));
		}

		public void Remove(SearchParameter parameter)
		{
			ParameterList.Remove(parameter);
		}

		public void Clear()
		{
			ParameterList.Clear();
		}

		public int Count
		{
			get { return ParameterList.Count; }
		}

		public void RefreshOptions(string sourceTable)
		{
			ParameterList.ForEach((parameter) => parameter.RefreshOptions(sourceTable));
		}
	}

	public class SearchParameter
	{
		public SearchParameter(string category, string value, CollectionType type, string sourceTable)
		{
			Category = category;
			Value = value;
			Type = type;
			SourceTable = sourceTable;
		}

		public string SourceTable;

		public readonly CollectionType Type;

		public string Category
		{
			get
			{
				return category;
			}
			set
			{
				category = value;
				Options = new SearchView().GetValueList(this);
			}
		}
		string category;

		public string Value { get; set; }

		public void RefreshOptions(string sourceTable)
		{
			SourceTable = sourceTable;
			Options = new SearchView().GetValueList(this);
		}

		public List<string> Options { get; private set; }
	}
}
