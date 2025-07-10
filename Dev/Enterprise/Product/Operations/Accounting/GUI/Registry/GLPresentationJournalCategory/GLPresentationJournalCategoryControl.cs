using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture;

namespace Enterprise.Registry.GUI
{
	public partial class GLPresentationJournalCategoryControl : RegistryZUserControl
	{
		public GLPresentationJournalCategoryControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			GLPresentationJournalCategoryGrid.ReadOnly = readOnly;
		}

		#region Column Names

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Programmatic constant")]
		internal const string BoolColumnName = "Bool";
		internal const string Bool2ColumnName = "Bool2";// Programmatic constant
		internal const string Bool3ColumnName = "Bool3";// Programmatic constant
		internal const string Bool4ColumnName = "Bool4";// Programmatic constant
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Programmatic constant")]
		internal const string CodeColumnName = "Code";
		internal const string ParentCodeColumnName = "ParentCode";// Programmatic constant
		internal const string DescriptionColumnName = "EnglishDescription";// Programmatic constant

		#endregion

		#region Bool Column

		public void SetupBoolColumn(string boolColumnCaption, string bool2ColumnCaption, string bool3ColumnCaption, string bool4ColumnCaption)
		{
			this.BoolColumnCaption = boolColumnCaption;
			this.IsBoolColumnVisible = true;
			this.Bool2ColumnCaption = bool2ColumnCaption;
			this.IsBool2ColumnVisible = true;
			this.Bool3ColumnCaption = bool3ColumnCaption;
			this.IsBool3ColumnVisible = true;
			this.Bool4ColumnCaption = bool4ColumnCaption;
			this.IsBool4ColumnVisible = true;
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			GLPresentationJournalCategoryCollection codeDescriptionBoolDataSource = dataSource as GLPresentationJournalCategoryCollection;
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
				RemoveColumnCore(ParentCodeColumnName);
			}
		}

		void RemoveColumnCore(string columnName)
		{
			for (int i = GLPresentationJournalCategoryGrid.ColumnStyles.Count - 1; i >= 0; --i)
			{
				ZGridColumnInfo columnInfo = (ZGridColumnInfo)GLPresentationJournalCategoryGrid.ColumnStyles[i];
				if (columnInfo.ColumnName == columnName)
				{
					GLPresentationJournalCategoryGrid.ColumnStyles.RemoveAt(i);
				}
			}
		}

		void SetBoolColumnLayoutIfNeeded()
		{
			if (IsBoolColumnVisible)
			{
				ZGridColumn column = GLPresentationJournalCategoryGrid.Columns[BoolColumnName];

				if (column != null)
				{
					DataGridColumnStyle columnStyle = column.ColumnStyle;
					columnStyle.HeaderText = BoolColumnCaption;

					using (var graphic = GLPresentationJournalCategoryGrid.CreateGraphics())
					{
						ControlDpiScalingHelper.SetWidth(ref columnStyle, (int)graphic.MeasureString(BoolColumnCaption, GLPresentationJournalCategoryGrid.Font).Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(5), false);
					}
				}
			}
			if (IsBool2ColumnVisible)
			{
				ZGridColumn column = GLPresentationJournalCategoryGrid.Columns[Bool2ColumnName];

				if (column != null)
				{
					DataGridColumnStyle columnStyle = column.ColumnStyle;
					columnStyle.HeaderText = Bool2ColumnCaption;

					using (var graphic = GLPresentationJournalCategoryGrid.CreateGraphics())
					{
						ControlDpiScalingHelper.SetWidth(ref columnStyle, (int)graphic.MeasureString(Bool2ColumnCaption, GLPresentationJournalCategoryGrid.Font).Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(5), false);
					}
				}
			}
			if (IsBool3ColumnVisible)
			{
				ZGridColumn column = GLPresentationJournalCategoryGrid.Columns[Bool3ColumnName];

				if (column != null)
				{
					DataGridColumnStyle columnStyle = column.ColumnStyle;
					columnStyle.HeaderText = Bool3ColumnCaption;

					using (var graphic = GLPresentationJournalCategoryGrid.CreateGraphics())
					{
						ControlDpiScalingHelper.SetWidth(ref columnStyle, (int)graphic.MeasureString(Bool3ColumnCaption, GLPresentationJournalCategoryGrid.Font).Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(5), false);
					}
				}
			}
			if (IsBool4ColumnVisible)
			{
				ZGridColumn column = GLPresentationJournalCategoryGrid.Columns[Bool4ColumnName];

				if (column != null)
				{
					DataGridColumnStyle columnStyle = column.ColumnStyle;
					columnStyle.HeaderText = Bool4ColumnCaption;

					using (var graphic = GLPresentationJournalCategoryGrid.CreateGraphics())
					{
						ControlDpiScalingHelper.SetWidth(ref columnStyle, (int)graphic.MeasureString(Bool4ColumnCaption, GLPresentationJournalCategoryGrid.Font).Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(5), false);
					}
				}
			}
		}

		string BoolColumnCaption;
		bool IsBoolColumnVisible;
		string Bool2ColumnCaption;
		bool IsBool2ColumnVisible;
		string Bool3ColumnCaption;
		bool IsBool3ColumnVisible;
		string Bool4ColumnCaption;
		bool IsBool4ColumnVisible;

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
				ZGridColumn code = GLPresentationJournalCategoryGrid.Columns[CodeColumnName];
				if (code != null)
				{
					code.ColumnStyle.ReadOnly = true;
				}

				ZGridColumn parentCode = GLPresentationJournalCategoryGrid.Columns[ParentCodeColumnName];
				if (parentCode != null)
				{
					parentCode.ColumnStyle.ReadOnly = true;
				}

				ZGridColumn desc = GLPresentationJournalCategoryGrid.Columns[DescriptionColumnName];
				if (desc != null)
				{
					desc.ColumnStyle.ReadOnly = true;
				}

				GLPresentationJournalCategoryGrid.RemoveAction = RemoveAction.NoRemovePossible;
			}
		}

		bool IsOnlyBoolColumnEditable;

		#endregion
	}
}

