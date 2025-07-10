using System;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Core.Forms
{
	/// <summary>
	/// Allows ZGridCustomise form to have ZGridColumns as groups.
	/// </summary>
	public class ZGridColumnGroup : ICustomizableColumn, ISubmissiveColumn
	{
		public ZGridColumnGroup(ZGridColumn column)
		{
			if (column.GroupName == null || column.GroupName.IsEmpty())
			{
				var zGridColumnStyle = column.ColumnStyle as ZGridColumnStyle;
				var groupName = zGridColumnStyle?.ColumnCaption ?? column.ColumnStyle.HeaderText;
				column.GroupName = new ResourceStringData("", column.IsCustomColumn ? groupName + CustomColumnSuffix : groupName);
			}

			fGroupName = column.GroupName;
			Add(column);
		}

		public void Add(ZGridColumn column)
		{
			if (column.GroupName.Equals(fGroupName))
			{
				fIsVisible |= column.IsVisible;
				fIsMandatory |= column.IsMandatory;
				fIsUnavailable |= column.IsUnavailable;
				fIsCustomColumn |= column.IsCustomColumn;
				var errorMessage = column.ErrorMessageWhenUnavailable;
				if (!string.IsNullOrEmpty(errorMessage))
				{
					ErrorMessageWhenUnavailable += errorMessage + "\n";
				}

				var newColumns = new ZGridColumn[fColumns.Length + 1];
				fColumns.CopyTo(newColumns, 0);
				newColumns[fColumns.Length] = column;

				fColumns = newColumns;
			}
			else
			{
				throw new ArgumentException("Only Columns with the same GroupName can be added to an ZGridColumnGroup.");
			}
		}

		public ResourceStringData GroupName
		{
			get { return fGroupName; }
		}

		public ZGridColumn[] Columns
		{
			get { return fColumns; }
		}

		string ICustomizableColumn.ColumnName
		{
			get { return GroupName.Caption; }
		}

		public bool IsVisible
		{
			get { return fIsVisible; }
		}

		public bool IsCustomColumn
		{
			get { return fIsCustomColumn; }
		}
		bool ICustomizableColumn.IsVisible
		{
			get { return fIsVisible; }
			set { fIsVisible = value; }
		}

		bool ICustomizableColumn.IsCustomColumn
		{
			get { return fIsCustomColumn; }
			set { fIsCustomColumn = value; }
		}

		public bool IsMandatory
		{
			get { return fIsMandatory; }
		}

		public bool IsUnavailable
		{
			get { return fIsUnavailable; }
		}

		const string CustomColumnSuffix = "*";

		public override string ToString()
		{
			if (Columns.Length == 1)
			{
				var zGridColumnStyle = Columns[0].ColumnStyle as ZGridColumnStyle;
				var result = zGridColumnStyle != null ? zGridColumnStyle.ColumnCaption : Columns[0].ColumnStyle.HeaderText;
				return zGridColumnStyle.IsCustomColumn ? result + CustomColumnSuffix : result;
			}
			else
			{
				return GroupName.Caption;
			}
		}

		#region Implementation

		internal string ErrorMessageWhenUnavailable { get; set; } = string.Empty;

		ZGridColumn[] fColumns = Array.Empty<ZGridColumn>();
		readonly ResourceStringData fGroupName;
		bool fIsVisible;
		bool fIsMandatory;
		bool fIsUnavailable;
		bool fIsCustomColumn;

		#endregion

		#region ISubmissiveColumn

		bool ISubmissiveColumn.IsSubmissive
		{
			get
			{
				foreach (ISubmissiveColumn column in Columns)
				{
					if (column.IsSubmissive)
					{
						return true;
					}
				}
				return false;
			}
		}

		#endregion
	}
}
