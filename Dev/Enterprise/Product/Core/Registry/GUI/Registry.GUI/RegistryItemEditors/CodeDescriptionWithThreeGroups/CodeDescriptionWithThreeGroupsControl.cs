using System;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Registry.GUI
{
	public partial class CodeDescriptionWithThreeGroupsControl : RegistryZUserControl
	{
		public CodeDescriptionWithThreeGroupsControl()
		{
			InitializeComponent();
			CodeDescriptionWithThreeGroupsGrid.CurrentCellChanged += CodeDescriptionWithThreeGroupsGrid_CurrentCellChanged;
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			CodeDescriptionWithThreeGroupsGrid.ReadOnly = readOnly;
		}

		#region Column Names

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Programmatic constant")]
		internal const string GroupColumnName = "Group";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Programmatic constant")]
		internal const string CodeColumnName = "Code";
		internal const string MainDescriptionColumnName = "MainDescription";
		internal const string Group2ColumnName = "Group2";
		internal const string Group3ColumnName = "Group3";
		internal const string ExtraDescriptionColumnName = "ExtraDescription";

		#endregion

		#region Group Column

		public void SetupGroupColumns(string bgroupColumnCaption, string bgroup2ColumnCaption, string bgroup3ColumnCaption, bool areGroupColumnsVisible)
		{
			this.GroupColumnCaption = bgroupColumnCaption;
			this.Group2ColumnCaption = bgroup2ColumnCaption;
			this.Group3ColumnCaption = bgroup3ColumnCaption;
			this.AreGroupColumnsVisible = areGroupColumnsVisible;
		}

		public void SetupMainDescriptionColumn(string bmainDescriptionColumnCaption)
		{
			this.MainDescriptionColumnCaption = bmainDescriptionColumnCaption;
		}

		public void SetupExtraDescriptionColumn(string bextraDescriptionColumnCaption)
		{
			this.ExtraDescriptionColumnCaption = bextraDescriptionColumnCaption;
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			object dataSourceForBinding = dataSource as ICodeDescriptionWithThreeGroupsList;
			RemoveColumnsIfNotNeeded();
			base.SetDataBinding(dataSourceForBinding, dataMember);
			if (dataSourceForBinding != null)
			{
				SetGroupColumnLayoutIfNeeded();
				SetMainDescriptionColumnLayout();
				SetExtraDescriptionColumnLayout();
				ApplyEditMode();
				CodeDescriptionWithThreeGroupsGrid_CurrentCellChanged(dataSourceForBinding, new EventArgs());
			}
		}

		void RemoveColumnsIfNotNeeded()
		{
			if (!AreGroupColumnsVisible)
			{
				RemoveColumnCore(GroupColumnName);
				RemoveColumnCore(Group2ColumnName);
				RemoveColumnCore(Group3ColumnName);
			}
		}

		void RemoveColumnCore(string columnName)
		{
			for (int i = CodeDescriptionWithThreeGroupsGrid.ColumnStyles.Count - 1; i >= 0; --i)
			{
				var columnInfo = (ZGridColumnInfo)CodeDescriptionWithThreeGroupsGrid.ColumnStyles[i];
				if (columnInfo.ColumnName == columnName)
				{
					CodeDescriptionWithThreeGroupsGrid.ColumnStyles.RemoveAt(i);
				}
			}
		}

		void SetMainDescriptionColumnLayout()
		{
			ZGridColumn column = CodeDescriptionWithThreeGroupsGrid.Columns[MainDescriptionColumnName];

			if (column != null)
			{
				var columnStyle = column.ColumnStyle;
				columnStyle.HeaderText = MainDescriptionColumnCaption;

				using (var graphic = CodeDescriptionWithThreeGroupsGrid.CreateGraphics())
				{
					ControlDpiScalingHelper.SetWidth(columnStyle, (int)graphic.MeasureString(MainDescriptionColumnCaption, CodeDescriptionWithThreeGroupsGrid.Font).Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(300), false);
				}
			}
		}

		void SetExtraDescriptionColumnLayout()
		{
			ZGridColumn column = CodeDescriptionWithThreeGroupsGrid.Columns[ExtraDescriptionColumnName];

			if (column != null)
			{
				var columnStyle = column.ColumnStyle;
				columnStyle.HeaderText = ExtraDescriptionColumnCaption;

				using (var graphic = CodeDescriptionWithThreeGroupsGrid.CreateGraphics())
				{
					ControlDpiScalingHelper.SetWidth(columnStyle, (int)graphic.MeasureString(ExtraDescriptionColumnCaption, CodeDescriptionWithThreeGroupsGrid.Font).Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(300), false);
				}
			}
		}

		void SetGroupColumnLayoutIfNeeded()
		{
			if (AreGroupColumnsVisible)
			{
				ZGridColumn column = CodeDescriptionWithThreeGroupsGrid.Columns[GroupColumnName];

				if (column != null)
				{
					var columnStyle = column.ColumnStyle;
					columnStyle.HeaderText = GroupColumnCaption;

					using (var graphic = CodeDescriptionWithThreeGroupsGrid.CreateGraphics())
					{
						ControlDpiScalingHelper.SetWidth(columnStyle, (int)graphic.MeasureString(GroupColumnCaption, CodeDescriptionWithThreeGroupsGrid.Font).Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(5), false);
					}
				}

				ZGridColumn column2 = CodeDescriptionWithThreeGroupsGrid.Columns[Group2ColumnName];

				if (column2 != null)
				{
					var column2Style = column2.ColumnStyle;
					column2Style.HeaderText = Group2ColumnCaption;

					using (var graphic = CodeDescriptionWithThreeGroupsGrid.CreateGraphics())
					{
						ControlDpiScalingHelper.SetWidth(column2Style, (int)graphic.MeasureString(Group2ColumnCaption, CodeDescriptionWithThreeGroupsGrid.Font).Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(5), false);
					}
				}

				ZGridColumn column3 = CodeDescriptionWithThreeGroupsGrid.Columns[Group3ColumnName];

				if (column3 != null)
				{
					var column3Style = column3.ColumnStyle;
					column3Style.HeaderText = Group3ColumnCaption;

					using (var graphic = CodeDescriptionWithThreeGroupsGrid.CreateGraphics())
					{
						ControlDpiScalingHelper.SetWidth(column3Style, (int)graphic.MeasureString(Group3ColumnCaption, CodeDescriptionWithThreeGroupsGrid.Font).Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(5), false);
					}
				}
			}
		}

		internal void CodeDescriptionWithThreeGroupsGrid_CurrentCellChanged(object sender, EventArgs e)
		{
			var list = BindingSource.DataSource as ICodeDescriptionWithThreeGroupsList;
			foreach (CodeDescriptionWithThreeGroups row in list)
			{
				((IZPropertyInfoInternals)row.GroupInfo).SetHumanReadableNameFromGuiCaption(GroupColumnCaption);
				((IZPropertyInfoInternals)row.Group2Info).SetHumanReadableNameFromGuiCaption(Group2ColumnCaption);
				((IZPropertyInfoInternals)row.Group3Info).SetHumanReadableNameFromGuiCaption(Group3ColumnCaption);
			}
		}

		string GroupColumnCaption;
		string Group2ColumnCaption;
		string Group3ColumnCaption;
		string MainDescriptionColumnCaption;
		string ExtraDescriptionColumnCaption;
		bool AreGroupColumnsVisible;
		bool AreOnlyCodeAndGroupColumnsEditable;

		#endregion

		#region Edit Mode

		public void SetupEditMode(bool areOnlyCodeAndGroupColumnsEditable)
		{
			this.AreOnlyCodeAndGroupColumnsEditable = areOnlyCodeAndGroupColumnsEditable;
		}

		void ApplyEditMode()
		{
			if (AreGroupColumnsVisible && AreOnlyCodeAndGroupColumnsEditable)
			{
				ZGridColumn desc = CodeDescriptionWithThreeGroupsGrid.Columns[MainDescriptionColumnName];
				if (desc != null)
				{
					desc.ColumnStyle.ReadOnly = true;
				}

				ZGridColumn extraDesc = CodeDescriptionWithThreeGroupsGrid.Columns[ExtraDescriptionColumnName];
				if (extraDesc != null)
				{
					extraDesc.ColumnStyle.ReadOnly = true;
				}
				CodeDescriptionWithThreeGroupsGrid.RemoveAction = RemoveAction.RemoveAndDelete;
			}
		}

		#endregion
	}
}
