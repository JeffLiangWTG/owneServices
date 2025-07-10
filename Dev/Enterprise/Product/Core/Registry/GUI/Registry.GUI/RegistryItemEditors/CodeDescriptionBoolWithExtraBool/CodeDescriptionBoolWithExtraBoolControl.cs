using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Registry.GUI
{
	public partial class CodeDescriptionBoolWithExtraBoolControl : RegistryZUserControl
	{
		public CodeDescriptionBoolWithExtraBoolControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			CodeDescriptionBoolGrid.ReadOnly = readOnly;
		}

		#region Bool Column

		public void SetupBoolColumn(string boolColumnCaption, bool isBoolColumnVisible, string bool2ColumnCaption, bool isBool2ColumnVisible)
		{
			this.BoolColumnCaption = boolColumnCaption;
			this.IsBoolColumnVisible = isBoolColumnVisible;
			this.Bool2ColumnCaption = bool2ColumnCaption;
			this.IsBool2ColumnVisible = isBool2ColumnVisible;
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			CodeDescriptionBoolWithExtraBoolCollection codeDescriptionBoolDataSource = dataSource as CodeDescriptionBoolWithExtraBoolCollection;
			object dataSourceForBinding = codeDescriptionBoolDataSource;
			RemoveColumnsIfNotNeeded();
			base.SetDataBinding(dataSourceForBinding, dataMember);
			if (dataSourceForBinding != null)
			{
				SetBoolColumnLayoutIfNeeded();
				ApplyEditMode();
			}
		}

		void RemoveColumnsIfNotNeeded()
		{
			if (!IsBool2ColumnVisible)
			{
				RemoveColumnCore(Bool2ColumnName);
			}
			if (!IsBoolColumnVisible)
			{
				RemoveColumnCore(BoolColumnName);
			}
			else if (IsOnlyBoolColumnEditable)
			{
				RemoveColumnCore(CodeColumnName);
			}
		}

		void RemoveColumnCore(string columnName)
		{
			for (int i = CodeDescriptionBoolGrid.ColumnStyles.Count - 1; i >= 0; --i)
			{
				ZGridColumnInfo columnInfo = (ZGridColumnInfo)CodeDescriptionBoolGrid.ColumnStyles[i];
				if (columnInfo.ColumnName == columnName)
				{
					CodeDescriptionBoolGrid.ColumnStyles.RemoveAt(i);
				}
			}
		}

		void SetBoolColumnLayoutIfNeeded()
		{
			if (IsBoolColumnVisible)
			{
				ZGridColumn column = CodeDescriptionBoolGrid.Columns[BoolColumnName];

				if (column != null)
				{
					DataGridColumnStyle columnStyle = column.ColumnStyle;
					columnStyle.HeaderText = BoolColumnCaption;

					using (var graphic = CodeDescriptionBoolGrid.CreateGraphics())
					{
						ControlDpiScalingHelper.SetWidth(ref columnStyle, (int)graphic.MeasureString(BoolColumnCaption, CodeDescriptionBoolGrid.Font).Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(5), false);
					}
				}
			}
			if (IsBool2ColumnVisible)
			{
				ZGridColumn column = CodeDescriptionBoolGrid.Columns[Bool2ColumnName];

				if (column != null)
				{
					DataGridColumnStyle columnStyle = column.ColumnStyle;
					columnStyle.HeaderText = Bool2ColumnCaption;

					using (var graphic = CodeDescriptionBoolGrid.CreateGraphics())
					{
						ControlDpiScalingHelper.SetWidth(ref columnStyle, (int)graphic.MeasureString(Bool2ColumnCaption, CodeDescriptionBoolGrid.Font).Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(5), false);
					}
				}
			}
		}

		string BoolColumnCaption;
		bool IsBoolColumnVisible;
		string Bool2ColumnCaption;
		bool IsBool2ColumnVisible;

		#endregion

		#region Edit Mode

		public void SetupEditMode(bool isOnlyBoolColumnEditable)
		{
			this.IsOnlyBoolColumnEditable = isOnlyBoolColumnEditable;
		}

		void ApplyEditMode()
		{
			if (IsBoolColumnVisible && IsOnlyBoolColumnEditable)
			{
				ZGridColumn code = CodeDescriptionBoolGrid.Columns[CodeColumnName];
				if (code != null)
				{
					code.ColumnStyle.ReadOnly = true;
				}

				ZGridColumn desc = CodeDescriptionBoolGrid.Columns[DescriptionColumnName];
				if (desc != null)
				{
					desc.ColumnStyle.ReadOnly = true;
				}

				CodeDescriptionBoolGrid.RemoveAction = RemoveAction.NoRemovePossible;
			}
		}

		bool IsOnlyBoolColumnEditable;

		#endregion
	}
}
