using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Registry.GUI
{
	public partial class CodeDescriptionBoolControl : RegistryZUserControl
	{
		public CodeDescriptionBoolControl()
		{
			InitializeComponent();
		}

		public CodeDescriptionBoolControl(bool isDescriptionColumnTranslatable)
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
			CodeDescriptionBoolGrid.ReadOnly = readOnly;
		}

		#region Column Names

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Programmatic constant")]
		internal const string DescriptionColumnName_Translatable = "Description";

		#endregion

		#region Columns

		public void SetupColumns(string boolColumnCaption, bool isBoolColumnVisible, bool isCodeColumnVisible)
		{
			this.BoolColumnCaption = boolColumnCaption;
			this.IsBoolColumnVisible = isBoolColumnVisible;
			this.IsCodeColumnVisible = isCodeColumnVisible;
		}

		public void SetupCodeColumnCaption(string codeColumnCaption)
		{
			this.CodeColumnCaption = codeColumnCaption;
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			object dataSourceForBinding = dataSource as ICodeDescriptionBoolList;
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
			if (!IsBoolColumnVisible)
			{
				RemoveColumnCore(BoolColumnName);
			}
			else if (!IsCodeColumnVisible)
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
				SetColumnLayout(BoolColumnName, BoolColumnCaption);
			}

			if (!string.IsNullOrEmpty(CodeColumnCaption) && IsCodeColumnVisible)
			{
				SetColumnLayout(CodeColumnName, CodeColumnCaption);
				CodeDescriptionBoolGrid.ReOrderColumns(new[] { CodeColumnName, DescriptionColumnName, BoolColumnName });
			}

			void SetColumnLayout(string columnName, string columnCaption)
			{
				ZGridColumn column = CodeDescriptionBoolGrid.Columns[columnName];

				if (column != null)
				{
					DataGridColumnStyle columnStyle = column.ColumnStyle;
					columnStyle.HeaderText = columnCaption;

					using (var graphic = CodeDescriptionBoolGrid.CreateGraphics())
					{
						ControlDpiScalingHelper.SetWidth(columnStyle, (int)graphic.MeasureString(columnCaption, CodeDescriptionBoolGrid.Font).Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(5), false);
					}
				}
			}
		}

		string BoolColumnCaption;
		string CodeColumnCaption;
		bool IsBoolColumnVisible;
		internal bool IsCodeColumnVisible { get; set; }

		#endregion

		#region Edit Mode

		public void SetupEditMode(bool isOnlyBoolColumnEditable, bool forceDescriptionColumnReadOnly  = false)
		{
			this.IsOnlyBoolColumnEditable = isOnlyBoolColumnEditable;
			ForceDescriptionColumnReadOnly = forceDescriptionColumnReadOnly;
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
				else
				{
					desc = CodeDescriptionBoolGrid.Columns[DescriptionColumnName_Translatable];
					if (desc != null && ForceDescriptionColumnReadOnly)
					{
						desc.ColumnStyle.ReadOnly = true;
					}
				}

				CodeDescriptionBoolGrid.RemoveAction = RemoveAction.NoRemovePossible;
			}
		}

		bool IsOnlyBoolColumnEditable;
		bool ForceDescriptionColumnReadOnly;

		#endregion
	}
}
