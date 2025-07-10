using System.Windows.Forms;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ZArchitecture
{
	/// <summary>
	/// Wrapper class for DataGridColumn to handle IsVisible and IsMandatory properties.
	/// </summary>
	public class ZGridColumn : ILayoutDetailTreeNode, ICustomizableColumn, ISubmissiveColumn
	{
		/// <summary>
		/// Delegate to allow overriding of column identification
		/// </summary>
		/// <param name="columnName">Column name to check this column against</param>
		/// <returns>True if this column matches the provided column name</returns>
		public delegate bool CustomColumnComparer(string columnName);

		/// <summary>
		/// Overridden to return the ColumnStyle.HeaderText.
		/// </summary>
		public override string ToString()
		{
			return columnStyle.HeaderText;
		}

		/// <summary>
		/// Gets or sets the System.Windows.Forms.DataGridColumnStyle.
		/// </summary>
		public DataGridColumnStyle ColumnStyle
		{
			get { return columnStyle; }
			set { columnStyle = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating if the column is visible in the Grid.
		/// </summary>
		public bool IsVisible
		{
			get { return visible; }
			set { visible = value; }
		}

		internal string StackTraceWhenSettingVisible { get; set; }
		internal string StackTraceWhenSettingInvisible { get; set; }

		/// <summary>
		/// Gets or sets a value indicating if the column must always be visible.
		/// </summary>
		public bool IsMandatory
		{
			get { return mandatory; }
			set { mandatory = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating if the column is custom
		/// </summary>
		public bool IsCustomColumn
		{
			get { return isCustomColumn; }
			set { isCustomColumn = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating if the column can not currently be added to the grid.
		/// </summary>
		public bool IsUnavailable
		{
			get { return unavailable; }
			set { unavailable = value; }
		}

		public string ErrorMessageWhenUnavailable
		{
			get { return errorMessageWhenUnavailable; }
			set { errorMessageWhenUnavailable = value; }
		}

		/// <summary>
		/// Gets or sets a value determining if the Column is part of a group in the ZGridCustomise form.
		/// </summary>
		public ResourceStringData GroupName
		{
			get;
			set;
		}

		internal int Width
		{
			get { return width; }
			set { width = value; }
		}

		public ZGridColumn Clone()
		{
			var cloned = new ZGridColumn();
			cloned.ColumnStyle = ColumnStyle;
			cloned.IsMandatory = IsMandatory;
			cloned.IsVisible = IsVisible;
			cloned.IsCustomColumn = IsCustomColumn;
			cloned.GroupName = GroupName;
			cloned.ColumnComparer = ColumnComparer;
			ControlDpiScalingHelper.SetWidth(ref cloned, Width, false);
			return cloned;
		}

		public string ColumnID { get; set; }

		public string ColumnName
		{
			get { return ColumnStyle != null ? ColumnStyle.MappingName : string.Empty; }
		}

		internal CustomColumnComparer ColumnComparer
		{
			get => columnComparer;
			set => columnComparer = value ?? columnComparer;
		}

		#region Implementation

		DataGridColumnStyle columnStyle;
		bool visible;
		bool mandatory;
		bool unavailable;
		bool isCustomColumn;
		string errorMessageWhenUnavailable = "";
		int width;
		CustomColumnComparer columnComparer;

		public ZGridColumn()
		{
			ColumnComparer = columnName => ColumnStyle.MappingName == columnName;
		}

		#endregion

		#region ILayoutDetailTreeNode Members

		string ILayoutDetailTreeNode.UniqueID
		{
			get { return GroupName == null || GroupName.IsEmpty() ? ColumnStyle.HeaderText : GroupName.Caption; }
		}

		string ILayoutDetailTreeNode.ParentUniqueID
		{
			get { return string.Empty; }
		}

		#endregion

		#region ISubmissiveColumn

		bool ISubmissiveColumn.IsSubmissive { get { return ColumnStyle is ZGridColumnStyle && ((ZGridColumnStyle)ColumnStyle).IsSubmissive; } }

		#endregion
	}
}
