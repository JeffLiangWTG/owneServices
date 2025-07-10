using System;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Registry.GUI
{
	public partial class CodeDescriptionWithGroupControl : RegistryZUserControl
	{
		public CodeDescriptionWithGroupControl()
		{
			InitializeComponent();
			CodeDescriptionWithGroupGrid.CurrentCellChanged += CodeDescriptionWithGroupGrid_CurrentCellChanged;
		}

		public CodeDescriptionWithGroupControl(bool isDescriptionColumnTranslatable)
			: this()
		{
			if (isDescriptionColumnTranslatable)
			{
				descriptionColumnStyleInfo.ColumnName = DescriptionColumnName_Translatable;
			}
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			CodeDescriptionWithGroupGrid.ReadOnly = readOnly;
		}

		#region Column Names

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Programmatic constant")]
		internal const string DescriptionColumnName_Translatable = "Description";

		#endregion

		#region Group Column

		public void SetupGroupColumn(string bgroupColumnCaption, bool isGroupColumnVisible)
		{
			this.GroupColumnCaption = bgroupColumnCaption;
			this.IsGroupColumnVisible = isGroupColumnVisible;
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			object dataSourceForBinding = dataSource as ICodeDescriptionWithGroupList;
			RemoveColumnsIfNotNeeded();
			base.SetDataBinding(dataSourceForBinding, dataMember);
			if (dataSourceForBinding != null)
			{
				SetGroupColumnLayoutIfNeeded();
				ApplyEditMode();
				CodeDescriptionWithGroupGrid_CurrentCellChanged(dataSourceForBinding, new EventArgs());
			}
		}

		void RemoveColumnsIfNotNeeded()
		{
			if (!IsGroupColumnVisible)
			{
				RemoveColumnCore(GroupColumnName);
			}
		}

		void RemoveColumnCore(string columnName)
		{
			for (int i = CodeDescriptionWithGroupGrid.ColumnStyles.Count - 1; i >= 0; --i)
			{
				var columnInfo = (ZGridColumnInfo)CodeDescriptionWithGroupGrid.ColumnStyles[i];
				if (columnInfo.ColumnName == columnName)
				{
					CodeDescriptionWithGroupGrid.ColumnStyles.RemoveAt(i);
				}
			}
		}

		void SetGroupColumnLayoutIfNeeded()
		{
			if (IsGroupColumnVisible)
			{
				ZGridColumn column = CodeDescriptionWithGroupGrid.Columns[GroupColumnName];

				if (column != null)
				{
					var columnStyle = column.ColumnStyle;
					columnStyle.HeaderText = GroupColumnCaption;

					using (var graphic = CodeDescriptionWithGroupGrid.CreateGraphics())
					{
						ControlDpiScalingHelper.SetWidth(columnStyle, (int)graphic.MeasureString(GroupColumnCaption, CodeDescriptionWithGroupGrid.Font).Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(5), false);
					}
				}
			}
		}

		internal void CodeDescriptionWithGroupGrid_CurrentCellChanged(object sender, EventArgs e)
		{
			var list = BindingSource.DataSource as ICodeDescriptionWithGroupList;
			foreach (CodeDescriptionWithGroup row in list)
			{
				((IZPropertyInfoInternals)row.GroupInfo).SetHumanReadableNameFromGuiCaption(GroupColumnCaption);
			}
		}

		string GroupColumnCaption;
		bool IsGroupColumnVisible;

		#endregion

		#region Edit Mode

		public void SetupEditMode(bool isOnlyGroupColumnEditable)
		{
			this.IsOnlyGroupColumnEditable = isOnlyGroupColumnEditable;
		}

		void ApplyEditMode()
		{
			if (IsGroupColumnVisible && IsOnlyGroupColumnEditable)
			{
				ZGridColumn code = CodeDescriptionWithGroupGrid.Columns[CodeColumnName];
				if (code != null)
				{
					code.ColumnStyle.ReadOnly = true;
				}

				ZGridColumn desc = CodeDescriptionWithGroupGrid.Columns[DescriptionColumnName];
				if (desc != null)
				{
					desc.ColumnStyle.ReadOnly = true;
				}

				CodeDescriptionWithGroupGrid.RemoveAction = RemoveAction.NoRemovePossible;
			}
		}

		bool IsOnlyGroupColumnEditable;

		#endregion
	}
}
