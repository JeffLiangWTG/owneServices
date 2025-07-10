using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web;
using System.Web.UI.WebControls;
using CargoWise.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.ZArchitecture.Web.GUI
{
	public class GridColumnProvider
	{
		public GridColumnProvider()
		{
		}

		#region OldLayoutFix

		public string FixOldLayout(string layout)
		{
			try
			{
				string[] keys = layout.Split(',');
				if (keys.Length > 0)
				{
					string newLayout = string.Empty;
					foreach (string key in keys)
					{
						try
						{
							int itemIndex = int.Parse(key);
							List<int> oldColumns = OldColumnsOrder;
							if (oldColumns.Count > itemIndex)
							{
								newLayout += (string.IsNullOrEmpty(newLayout) ? "" : ",") + oldColumns[itemIndex].ToString();
							}
						}
						catch (Exception e) when (!e.IsCriticalException()) { }
					}
					return newLayout;
				}
			}
			catch (Exception e) when (!e.IsCriticalException()) { }
			return layout;
		}

		public List<int> OldColumnsOrder
		{
			get { return GetOldColumnsOrder(); }
		}

		protected virtual List<int> GetOldColumnsOrder()
		{
			return new List<int>();
		}

		#endregion

		#region Properties

		public bool IsEmpty
		{
			get { return Columns.Count == 0; }
		}

		protected Dictionary<int, DataGridColumn> Columns
		{
			get
			{
				return columns ?? (columns = new Dictionary<int, DataGridColumn>());
			}
		}
		Dictionary<int, DataGridColumn> columns;

		public int UniqueColumnsCount
		{
			get { return Columns.Count; }
		}

		public List<int> Keys
		{
			get
			{
				List<int> result = new List<int>();
				result.AddRange(Columns.Keys);
				return result;
			}
		}

		public bool ContainsKey(object key)
		{
			return Columns.ContainsKey((int)key);
		}

		public DataGridColumn this[object key]
		{
			get { return Columns[(int)key]; }
		}

		public int? GetKeyByColumnName(string columnName)
		{
			int? result = null;
			foreach (var column in Columns)
			{
				if (column.Value.HeaderText == columnName)
				{
					result = column.Key;
					break;
				}
			}
			return result;
		}

		public List<DataGridColumn> AllColumns
		{
			get
			{
				return allColumns ?? (allColumns = new List<DataGridColumn>());
			}
		}
		List<DataGridColumn> allColumns;

		public List<int> DefaultColumns
		{
			get
			{
				return defaultColumns ?? (defaultColumns = new List<int>());
			}
		}
		List<int> defaultColumns;

		public List<int> RequiredColumns
		{
			get
			{
				return requiredColumns ?? (requiredColumns = new List<int>());
			}
		}
		List<int> requiredColumns;

		public List<int> RestrictedColumns
		{
			get { return restrictedColumns ?? (restrictedColumns = PopulateRestrictedColumns()); }
		}
		List<int> restrictedColumns;

		protected virtual List<int> PopulateRestrictedColumns()
		{
			return new List<int>();
		}

		public int[] RequiredAndDefaultColumns
		{
			get
			{
				if (requiredAndDefaultColumns == null)
				{
					requiredAndDefaultColumns = new int[DefaultColumns.Count + RequiredColumns.Count];
					RequiredColumns.ToArray().CopyTo(requiredAndDefaultColumns, 0);
					DefaultColumns.ToArray().CopyTo(requiredAndDefaultColumns, RequiredColumns.Count);
				}
				return requiredAndDefaultColumns;
			}
		}
		int[] requiredAndDefaultColumns;

		#region Helpers

		public DataGridColumn[] GridColumnFields
		{
			get
			{
				if (gridColumnFields == null)
				{
					var result = new List<DataGridColumn>();
					foreach (var column in Columns.Values)
					{
						result.Add(column);
					}
					gridColumnFields = result.ToArray();
				}
				return gridColumnFields;
			}
		}

		public DataGridColumn[] DefaultGridColumnFields
		{
			get
			{
				if (defaultGridColumnFields == null)
				{
					var result = new List<DataGridColumn>();
					foreach (var colKey in DefaultColumns.ToArray())
					{
						result.Add(Columns[colKey]);
					}
					defaultGridColumnFields = result.ToArray();
				}
				return defaultGridColumnFields;
			}
		}

		public DataGridColumn[] RequiredGridColumnFields
		{
			get
			{
				if (requiredGridColumnFields == null)
				{
					var result = new List<DataGridColumn>();
					foreach (var colKey in RequiredColumns.ToArray())
					{
						result.Add(Columns[colKey]);
					}

					requiredGridColumnFields = result.ToArray();
				}
				return requiredGridColumnFields;
			}
		}

		public DataGridColumn[] GroupMemberColumnFields
		{
			get
			{
				if (groupMemberColumnFields == null)
				{
					var result = new List<DataGridColumn>();
					foreach (var col in GridColumnFields)
					{
						if (col is ZGroupColumn)
						{
							result.AddRange(((ZGroupColumn)col).GroupMembers);
						}
					}
					groupMemberColumnFields = result.ToArray();
				}
				return groupMemberColumnFields;
			}
		}

		DataGridColumn[] gridColumnFields;
		DataGridColumn[] defaultGridColumnFields;
		DataGridColumn[] requiredGridColumnFields;
		DataGridColumn[] groupMemberColumnFields;

		#endregion

		#endregion

		#region Implementation

		public void CustomizeDictionary()
		{
			CustomizeDictionaryCore();
		}

		protected virtual void CustomizeDictionaryCore()
		{
#if DEBUG
			baseCustomizeDictionaryCalled = true;
#endif
		}

		#region Test

#if DEBUG
		[Browsable(false)]
		public bool BaseCustomizeDictionaryCalled
		{
			get { return baseCustomizeDictionaryCalled; }
		}
		bool baseCustomizeDictionaryCalled;
#endif
		#endregion

		#endregion

		#region Application path and formatiing of Urls

		protected string UrlFormatWithAppRoot(string page)
		{
			return AppPath + page;
		}

		protected string AppPath
		{
			get
			{
#if DEBUG
				if (Globals.IsTest)
				{
					return "/";
				}
				else
#endif
				{
					string result = HttpContext.Current.Request.ApplicationPath;
					if (result != "/")
					{
						result += "/";
					}

					return result;
				}
			}
		}

		#endregion

		#region AddToDictionary

		public void AddToDictionary(CustomLabelInfo field)
		{
			AddToDictionary(field, null);
		}

		public bool AddToDictionary(CustomLabelInfo field, object columnKey, string specifiedBindTo = null)
		{
			if (field != null && field.IsEnabled)
			{
				ZTemplateColumn column = ZTemplateColumn.GetNew(field.Caption, field.PropertyType, specifiedBindTo ?? field.PropertyName);
				column.ColumnKey = columnKey;
				AddToDictionary(column);
				return true;
			}
			return false;
		}

		public void AddGroupColumn(string headerText, params DataGridColumn[] columns)
		{
			AddGroupColumn(headerText, false, columns);
		}

		public void AddGroupColumn(string headerText, bool def, params DataGridColumn[] columns)
		{
			AddGroupColumn(headerText, null, false, columns);
		}

		public void AddGroupColumn(string headerText, object columnKey, bool def, params DataGridColumn[] columns)
		{
			var result = new ZGroupColumn(headerText, columns) { ColumnKey = columnKey };
			if (def)
			{
				AddToDictionaryAsDefault(result);
			}
			else
			{
				AddToDictionary(result);
			}
		}

		public void AddButtonColumn(string headerText, string columnName)
		{
			AddButtonColumn(headerText, columnName, null);
		}

		public void AddButtonColumn(string headerText, string columnName, object columnKey)
		{
			ZButtonColumn buttonColumn = new ZButtonColumn(headerText, columnName);
			buttonColumn.CommandName = "Select"; // Query related
			buttonColumn.ColumnKey = columnKey;
			AddToDictionaryAsRequired(buttonColumn);
		}

		public void AddToDictionaryAsRequired(IUniqueKeyColumn column)
		{
			if (column != null && column is DataGridColumn && !IsRestrictedColumn(column.UniqueKey))
			{
				AddToDictionary(column);
				RequiredColumns.Add(column.UniqueKey);
			}
		}

		public void AddToDictionaryAsDefault(IUniqueKeyColumn column)
		{
			if (column != null && !IsRestrictedColumn(column.UniqueKey))
			{
				AddToDictionary(column);
				DefaultColumns.Add(column.UniqueKey);
			}
		}

		bool IsRestrictedColumn(int columnKey)
		{
			return RestrictedColumns.Contains(columnKey);
		}

		public void AddToDictionary(IUniqueKeyColumn column)
		{
			if (column != null && column is DataGridColumn)
			{
				column.ColumnIndex = AllColumns.Count;
				AllColumns.Add((DataGridColumn)column);
				int key = column.UniqueKey;
				if (column != null)
				{
					if (Columns.ContainsKey(key))
					{
						Columns[key] = (DataGridColumn)column;
					}
					else
					{
						Columns.Add(key, (DataGridColumn)column);
					}
				}
			}
		}

		#endregion
	}
}
