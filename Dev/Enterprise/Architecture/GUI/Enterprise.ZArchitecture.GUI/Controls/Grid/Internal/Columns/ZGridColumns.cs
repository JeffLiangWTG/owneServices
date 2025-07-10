using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Controls;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture
{
	public class ZGridColumns : IEnumerable<ZGridColumn>, IDisposable
	{
		readonly ZGrid grid;

		public ZGridColumns(ZGrid grid)
		{
			Argument.NotNull(grid, "grid");
			this.grid = grid;
			columns = new List<ZGridColumn>();
		}

		/// <summary>
		/// Creates a copy of the ZGridColumns collection (Note that the copy is not a complete clone - see implementation for details).
		/// </summary>
		public ZGridColumns Clone()
		{
			var result = (ZGridColumns)Activator.CreateInstance(GetType(), grid);

			// we only clone what we need
			foreach (var column in columns)
			{
				result.AddColumn(column.Clone());
			}

			return result;
		}

		#region Properties

		/// <summary>
		/// Gets a value indicating the number of columns in the columns collection.
		/// </summary>
		public int Count
		{
			get { return columns.Count; }
		}

		/// <summary>
		/// Gets or sets a value indicating the table name to which the columns collection belongs.
		/// </summary>
		public string TableName { get; set; }

		public ZGrid Grid
		{
			get { return grid; }
		}

		public string ColumnID { get; set; }

		#endregion

		#region Indexers

		/// <summary>
		/// Returns a read-only, enumerable collection of columns.
		/// </summary>
		IEnumerator<ZGridColumn> IEnumerable<ZGridColumn>.GetEnumerator()
		{
			return columns.GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<ZGridColumn>)this).GetEnumerator();
		}

		/// <summary>
		/// Returns a single column at the specified index.
		/// </summary>
		public ZGridColumn this[int index]
		{
			get
			{
				if (index < 0 || index >= columns.Count)
				{
					throw new ArgumentOutOfRangeException("Column Index '" + index + "'  is out of range.");
				}

				return columns[index];
			}
		}

		/// <summary>
		/// Returns a single column with column name matching the specified string.
		/// </summary>
		public ZGridColumn this[string columnName, bool useCustomColumnComparer = false]
		{
			get
			{
				foreach (var column in columns)
				{
					if (useCustomColumnComparer)
					{
						if (column.ColumnComparer(columnName))
						{
							return column;
						}
					}
					else
					{
						if (column.ColumnStyle.MappingName == columnName)
						{
							return column;
						}
					}
				}

				return null;
			}
		}

		#endregion

		#region Adding Columns

		/// <summary>
		/// Add a Column to the ZGridColumns collection.
		/// </summary>
		/// <param name="columnInfo">The ZGridColumnInfo that specifies the Column details.</param>
		public void Add(ZGridColumnInfo columnInfo)
		{
			Insert(columnInfo, -1);
		}

		/// <summary>
		/// Insert an ZGridColumn at the specified Index.
		/// </summary>
		/// <param name="columnInfo">The ZGridColumnInfo that specifies the Column details.</param>
		/// <param name="index">The Index of where to insert the Column.</param>
		public void Insert(ZGridColumnInfo columnInfo, int index)
		{
			var columnStyle = ((ZGridColumnStyle)Activator.CreateInstance(columnInfo.ColumnStyleType, new object[] { columnInfo }));
			try
			{
				var column =
					new ZGridColumn
					{
						IsMandatory = columnInfo.IsMandatory,
						IsVisible = columnInfo.IsVisible,
						GroupName = columnInfo.GroupName,
						ErrorMessageWhenUnavailable = columnInfo.ErrorMessageWhenUnavailable,
						IsUnavailable = columnInfo.IsUnavailable,
						ColumnStyle = columnStyle,
						ColumnComparer = columnInfo.ColumnComparer,
						IsCustomColumn = columnInfo.IsCustomColumn,
					};

				ControlDpiScalingHelper.SetWidth(column, columnInfo.Width, false);

				bool wasDefaulted;
				column.ColumnStyle.HeaderText = GetHeaderText(column, columnInfo, out wasDefaulted);
				((ZGridColumnStyle)column.ColumnStyle).HeaderTextWasDefaulted = wasDefaulted;

				AddGridColumn(column, index);
			}
			catch (Exception)
			{
				columnStyle.Dispose();
				throw;
			}
		}

		/// <summary>
		/// Add an existing column to the grid.
		/// </summary>
		/// <param name="column">The column to add.</param>
		public void AddColumn(ZGridColumn column)
		{
			AddGridColumn(column, -1);
		}

		#region AddTextColumn

		/// <summary>
		/// Adds a Text column to the grid.
		/// </summary>
		/// <param name="columnName">The name of the column.</param>
		/// <param name="width">The default width of the column.</param>
		/// <param name="visible">The default visibility of the column.</param>
		/// <param name="mandatory">Denotes whether the column must always be visible.</param>
		/// <param name="readOnly"></param>
		public void AddTextColumn(string columnName, int width, bool visible, bool mandatory, bool readOnly)
		{
			AddTextColumn(columnName, width, visible, mandatory, readOnly, CharacterCasing.Upper);
		}

		/// <summary>
		/// Adds a Text column to the grid.
		/// </summary>
		/// <param name="columnName">The name of the column.</param>
		/// <param name="width">The default width of the column.</param>
		/// <param name="isVisible">The default visibility of the column.</param>
		/// <param name="isMandatory">Denotes whether the column must always be visible.</param>
		/// <param name="isReadOnly"></param>
		/// <param name="casing"></param>
		public void AddTextColumn(string columnName, int width, bool isVisible, bool isMandatory, bool isReadOnly, CharacterCasing casing)
		{
			var info = new ZTextBoxColumnStyleInfo(columnName, width) { CharacterCasing = casing };
			AddGridColumn(new ZTextBoxColumnStyle(info), columnName, width, isVisible, isMandatory, isReadOnly);
		}

		/// <summary>
		/// Adds a Text column to the grid.
		/// </summary>
		/// <param name="columnName">The name of the column.</param>
		/// <param name="width">The default width of the column.</param>
		/// <param name="isVisible">The default visibility of the column.</param>
		/// <param name="isMandatory">Denotes whether the column must always be visible.</param>
		public void AddTextColumn(string columnName, int width, bool isVisible, bool isMandatory)
		{
			AddTextColumn(columnName, width, isVisible, isMandatory, false);
		}

		/// <summary>
		/// Adds a Text column to the grid.
		/// </summary>
		/// <param name="columnName">The name of the column.</param>
		/// <param name="width">The default width of the column.</param>
		public void AddTextColumn(string columnName, int width)
		{
			AddTextColumn(columnName, width, true, true);
		}

		/// <summary>
		/// Adds a Text column to the grid.
		/// </summary>
		/// <param name="columnName">The name of the column.</param>
		/// <param name="width">The default width of the column.</param>
		/// <param name="columnCharacterCasing">The character casing of the column.</param>
		public void AddTextColumn(string columnName, int width, CharacterCasing columnCharacterCasing)
		{
			AddTextColumn(columnName, width, true, true, false, columnCharacterCasing);
		}

		#endregion

		#region AddCalcEditColumn

		/// <summary>
		/// Adds a Numeric column to the grid.
		/// </summary>
		/// <param name="columnName">The name of the column.</param>
		/// <param name="width">The default width of the column.</param>
		/// <param name="isVisible">The default visibility of the column.</param>
		/// <param name="isMandatory">Denotes whether the column must always be visible.</param>
		/// <param name="decimals">The number of decimals to display.</param>
		/// <param name="allowNegative">Wheter allow or not to enter negative values</param>
		/// <param name="isReadOnly"></param>
		public void AddCalcEditColumn(string columnName, int width, bool isVisible, bool isMandatory, int decimals, bool allowNegative, bool isReadOnly)
		{
			var columnInfo = new ZCalcEditColumnStyleInfo(columnName, width, decimals);
			AddGridColumn(new ZCalcEditColumnStyle(columnInfo), columnName, width, isVisible, isMandatory, isReadOnly);
		}

		/// <summary>
		/// Adds a Numeric column to the grid.
		/// </summary>
		/// <param name="columnName">The name of the column.</param>
		/// <param name="width">The default width of the column.</param>
		/// <param name="isVisible">The default visibility of the column.</param>
		/// <param name="isMandatory">Denotes whether the column must always be visible.</param>
		/// <param name="decimals">The number of decimals to display.</param>
		/// <param name="allowNegative">Wheter allow or not to enter negative values</param>
		public void AddCalcEditColumn(string columnName, int width, bool isVisible, bool isMandatory, int decimals, bool allowNegative)
		{
			AddCalcEditColumn(columnName, width, isVisible, isMandatory, decimals, allowNegative, false);
		}

		/// <summary>
		/// Adds a Numeric column to the grid.
		/// </summary>
		/// <param name="columnName">The name of the column.</param>
		/// <param name="width">The default width of the column.</param>
		/// <param name="isVisible">The default visibility of the column.</param>
		/// <param name="isMandatory">Denotes whether the column must always be visible.</param>
		/// <param name="decimals">The number of decimals to display.</param>
		public void AddCalcEditColumn(string columnName, int width, bool isVisible, bool isMandatory, int decimals)
		{
			AddCalcEditColumn(columnName, width, isVisible, isMandatory, decimals, false);
		}

		/// <summary>
		/// Adds a Numeric column to the grid.
		/// </summary>
		/// <param name="columnName">The name of the column.</param>
		/// <param name="width">The default width of the column.</param>
		/// <param name="decimals">The number of decimals to display.</param>
		public void AddCalcEditColumn(string columnName, int width, int decimals)
		{
			AddCalcEditColumn(columnName, width, true, true, decimals);
		}

		#endregion

		#region AddDateColumn

		/// <summary>
		/// Adds a Date column to the grid.
		/// </summary>
		/// <param name="columnName">The name of the column.</param>
		/// <param name="width">The default width of the column.</param>
		/// <param name="isVisible">The default visibility of the column.</param>
		/// <param name="isMandatory">Denotes whether the column must always be visible.</param>
		/// <param name="format">The date-time formatting.</param>
		/// <param name="isReadOnly"></param>
		public void AddDateColumn(string columnName, int width, bool isVisible, bool isMandatory, ZDateTimePickerFormat format, bool isReadOnly)
		{
			var columnInfo = new ZDateEditColumnStyleInfo(columnName, width) { DateTimeFormat = format };
			AddGridColumn(new ZDateEditColumnStyle(columnInfo), columnName, width, isVisible, isMandatory, isReadOnly);
		}

		/// <summary>
		/// Adds a Date column to the grid.
		/// </summary>
		/// <param name="columnName">The name of the column.</param>
		/// <param name="width">The default width of the column.</param>
		/// <param name="isVisible">The default visibility of the column.</param>
		/// <param name="isMandatory">Denotes whether the column must always be visible.</param>
		/// <param name="format">The date-time formatting.</param>
		public void AddDateColumn(string columnName, int width, bool isVisible, bool isMandatory, ZDateTimePickerFormat format)
		{
			AddDateColumn(columnName, width, isVisible, isMandatory, format, false);
		}

		/// <summary>
		/// Adds a Date column to the grid.
		/// </summary>
		/// <param name="columnName">The name of the column.</param>
		/// <param name="width">The default width of the column.</param>
		/// <param name="isVisible">The default visibility of the column.</param>
		/// <param name="isMandatory">Denotes whether the column must always be visible.</param>
		public void AddDateColumn(string columnName, int width, bool isVisible, bool isMandatory)
		{
			AddDateColumn(columnName, width, isVisible, isMandatory, ZDateTimePickerFormat.Short);
		}

		/// <summary>
		/// Adds a Date column to the grid.
		/// </summary>
		/// <param name="columnName">The name of the column.</param>
		/// <param name="width">The default width of the column.</param>
		public void AddDateColumn(string columnName, int width)
		{
			AddDateColumn(columnName, width, true, true, ZDateTimePickerFormat.Short);
		}

		/// <summary>
		/// Adds a Date column to the grid.
		/// </summary>
		/// <param name="columnName">The name of the column.</param>
		/// <param name="width">The default width of the column.</param>
		/// <param name="format">The date-time formatting.</param>
		public void AddDateColumn(string columnName, int width, ZDateTimePickerFormat format)
		{
			AddDateColumn(columnName, width, true, true, format);
		}

		#endregion

		#region AddBoolColumn

		/// <summary>
		/// Adds a Bool column to the grid.
		/// </summary>
		/// <param name="columnName">The name of the column.</param>
		/// <param name="width">The default width of the column.</param>
		/// <param name="isVisible">The default visibility of the column.</param>
		/// <param name="isMandatory">Denotes whether the column must always be visible.</param>
		/// <param name="isReadOnly"></param>
		public void AddBoolColumn(string columnName, int width, bool isVisible, bool isMandatory, bool isReadOnly)
		{
			var columnInfo = new ZCheckBoxColumnStyleInfo(columnName, width);
			AddGridColumn(new ZCheckBoxColumnStyle(columnInfo), columnName, width, isVisible, isMandatory, isReadOnly);
		}

		/// <summary>
		/// Adds a Bool column to the grid.
		/// </summary>
		/// <param name="columnName">The name of the column.</param>
		/// <param name="width">The default width of the column.</param>
		/// <param name="isVisible">The default visibility of the column.</param>
		/// <param name="isMandatory">Denotes whether the column must always be visible.</param>
		public void AddBoolColumn(string columnName, int width, bool isVisible, bool isMandatory)
		{
			AddBoolColumn(columnName, width, isVisible, isMandatory, false);
		}

		/// <summary>
		/// Adds a Boolean column to the grid.
		/// </summary>
		/// <param name="columnName">The name of the column.</param>
		/// <param name="width">The default width of the column.</param>
		public void AddBoolColumn(string columnName, int width)
		{
			AddBoolColumn(columnName, width, true, true);
		}

		#endregion

		#endregion

		#region Remove & Contains

		/// <summary>
		/// Removes a Column from the ZGridColumns Collection.
		/// </summary>
		/// <param name="columnName">MappingName of Column.</param>
		public void Remove(string columnName)
		{
			ZGridColumn columnToRemove = null;

			foreach (var column in columns)
			{
				if (column.ColumnStyle.MappingName == columnName)
				{
					columnToRemove = column;
				}
			}

			if (columnToRemove != null)
			{
				grid.RegisterRemovedColumn(columnToRemove);

				UnHookWidthChanged(columnToRemove);
				columns.Remove(columnToRemove);
			}
		}

		/// <summary>
		/// Finds if the specified Column exists in the ZGridColumns Collection.
		/// </summary>
		/// <param name="columnName">MappingName of Column.</param>
		/// <returns>True if the Column exists, otherwise False.</returns>
		public bool Contains(string columnName)
		{
			return this[columnName] != null;
		}

		#endregion

		#region Move

		public void Move(ZGridColumn column, int newIndex)
		{
			columns.Remove(column);

			if (newIndex < 0)
			{
				newIndex = 0;
			}
			else if (newIndex > columns.Count)
			{
				newIndex = columns.Count;
			}

			columns.Insert(newIndex, column);
		}

		#endregion

		#region Implementation

		readonly Dictionary<string, string> tableNameCache = new Dictionary<string, string>();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Data Member Name")]
		protected virtual string GetTableName(string dataMember)
		{
			if (dataMember == "Code" || dataMember == "Description")
			{
				return "";
			}

			var tablePrefix = CargoWise.Schema.Schema.GetPrefixFromColumnName(dataMember);

			if (string.IsNullOrEmpty(tablePrefix))
			{
				return "";
			}

			if (tableNameCache.ContainsKey(tablePrefix))
			{
				return tableNameCache[tablePrefix];
			}

			string result;

#if DEBUG
			if (tablePrefix == DummyBaseBusinessObject.Schema.TablePrefix)
			{
				result = "TestTable";
			}
			else
#endif
			{
				var tableSchema = EnterpriseSchema.GetTableSchemaFromColumnNamePrefix(tablePrefix);
				result = (tableSchema != null) ? tableSchema.TableName : string.Empty;
			}
			tableNameCache[tablePrefix] = result;
			return result;
		}

		static string ForcePadRight(string headerText)
		{
			// This is a hack to fix .Net Bug with right justified Column captions being chopped off at the end.
			return headerText + (char)32 + (char)31;
		}

		/// <summary>
		/// Sets up the a single OGridColum and adds it to the column collection.
		/// </summary>
		void AddGridColumn(DataGridColumnStyle columnStyle, string columnName, int width, bool visible, bool mandatory, bool readOnly)
		{
			columnStyle.MappingName = columnName;

			var headerText = DataBoundResourceStrings.GetColumnDescriptiveName(GetTableName(columnName), columnName);
			if (columnStyle.Alignment == HorizontalAlignment.Right)
			{
				headerText = ForcePadRight(headerText);
			}

			columnStyle.HeaderText = headerText;
			ControlDpiScalingHelper.SetWidth(ref columnStyle, width, false);

			var column =
				new ZGridColumn
				{
					ColumnStyle = columnStyle,
					IsVisible = visible,
					IsMandatory = mandatory
				};
			ControlDpiScalingHelper.SetWidth(column, width, false);
			column.ColumnStyle.ReadOnly = readOnly;

			try
			{
				AddGridColumn(column, -1);
			}
			catch
			{
				column.ColumnStyle.Dispose();
				throw;
			}
		}

		/// <summary>
		/// Adds a single OGridColum to the column collection.
		/// </summary>
		void AddGridColumn(ZGridColumn newColumn, int index)
		{
			if (ColumnExists(newColumn))
			{
				throw new ArgumentException(string.Format("The column '{0}' already exists in table '{1}'.", new object[] { newColumn.ColumnStyle.MappingName, GetTableName(newColumn.ColumnStyle.MappingName) }));
			}

			((ZGridColumnStyle)newColumn.ColumnStyle).SetParentGrid(Grid);
			((ZGridColumnStyle)newColumn.ColumnStyle).HeaderFont = Grid.HeaderFont;

			if (index == -1)
			{
				columns.Add(newColumn);
			}
			else
			{
				columns.Insert(index, newColumn);
			}

			if (newColumn.IsVisible && index > 0 && !columns[index - 1].IsVisible)
			{
				newColumn.StackTraceWhenSettingVisible = System.Environment.StackTrace;
			}

			if (!newColumn.IsVisible && index > -1 && index < columns.Count - 1 && columns[index + 1].IsVisible)
			{
				newColumn.StackTraceWhenSettingInvisible = System.Environment.StackTrace;
			}

			newColumn.ColumnStyle.WidthChanged += ColumnStyle_WidthChanged;
		}

		/// <summary>
		/// Returns true if the column already exists (user is attempting to add it more than once).
		/// </summary>
		bool ColumnExists(ZGridColumn proposedColumn)
		{
			return Contains(proposedColumn.ColumnStyle.MappingName);
		}

		string GetHeaderText(ZGridColumn column, ZGridColumnInfo columnInfo, out bool wasDefaulted)
		{
			string headerText;
			wasDefaulted = false;

			if (!string.IsNullOrEmpty(columnInfo.Caption))
			{
				headerText = columnInfo.Caption;
			}
			else
			{
				headerText = DataBoundResourceStrings.GetColumnDescriptiveName(GetTableName(columnInfo.ColumnName), columnInfo.ColumnName);
				wasDefaulted = true;
			}

			if (column.ColumnStyle.Alignment == HorizontalAlignment.Right)
			{
				headerText = ForcePadRight(headerText);
			}

			return headerText;
		}

		readonly List<ZGridColumn> columns;

		#endregion

		#region IDisposable Members

		void IDisposable.Dispose()
		{
			DisposeAllColumns();
		}

		public void DisposeAllColumns()
		{
			foreach (var column in columns.ToList())
			{
				try
				{
					column.ColumnStyle.Dispose();
					UnHookWidthChanged(column);
				}
				catch (Exception ex)
				{
					var message = FormattableString.Invariant($@"{ex.Message}
Grid: {this.grid.Name} ({ControlDescription.GetControlPath(this.grid)})
Bind To: {this.grid.BindTo}"); // Silent Developer error
					ErrorReporter.ReportOnce("ErrorInDisposingGridColumn_" + column.ColumnName, message, ex);
				}
			}

			columns.Clear();
		}

		#endregion

		#region Layout Changed

		public bool HasLayoutChanged
		{
			get { return fHasLayoutChanged; }

			set
			{
				fHasLayoutChanged = value;
				if (value)
				{
					LastLayoutChangedStackTrace = System.Environment.StackTrace;
				}
			}
		}

		bool fHasLayoutChanged;

		internal string LastLayoutChangedStackTrace { get; private set; }

		void UnHookWidthChanged(ZGridColumn column)
		{
			column.ColumnStyle.WidthChanged -= ColumnStyle_WidthChanged;
		}

		void ColumnStyle_WidthChanged(object sender, EventArgs e)
		{
			HasLayoutChanged = true;
		}

		#endregion

		#region GetVisibleColumnNames

		public string[] GetVisibleColumnMappingNames()
		{
			var result = new List<string>();

			foreach (var gridColumn in this)
			{
				if (gridColumn.IsVisible)
				{
					result.Add(gridColumn.ColumnStyle.MappingName);
				}
			}

			return result.ToArray();
		}

		#endregion

		internal void ReOrderColumns(IEnumerable<string> columnNamesInSortOrder)
		{
			var workingList = columns.ToList();
			columns.Clear();
			foreach (var columnName in columnNamesInSortOrder)
			{
				var column = workingList.FirstOrDefault(x => x.ColumnStyle.MappingName == columnName);
				if (column != null)
				{
					workingList.Remove(column);
					columns.Add(column);
				}
			}
			columns.AddRange(workingList);
		}
	}
}
