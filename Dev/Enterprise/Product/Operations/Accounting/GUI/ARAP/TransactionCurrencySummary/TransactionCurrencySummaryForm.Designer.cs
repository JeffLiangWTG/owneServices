using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public partial class TransactionCurrencySummaryForm
	{


		#region Windows Form Designer generated code

		protected override void InitializeComponent()
		{
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new ZCodeFindBoxColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			this.CurrencySummaryGrid = new ZDisplayGrid();
			this.CloseButton = new Core.Forms.ZPostOrCancelButton();
			this.EarliestDueDateEdit = new ZDateEdit();
			this.LatestDueDteDateEdit = new ZDateEdit();
			this.TotalAmountCalcFindBox = new ZCalcFindBox();
			this.TotalOutStandingCalcFindBox = new ZCalcFindBox();
			this.TotalCountCalcEdit = new ZArchitecture.ZCalcEdit();
			this.LineSummaryGridPanel = new ZPanel();
			this.zPanel1 = new ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.CurrencySummaryGrid)).BeginInit();
			this.CurrencySummaryGrid.SuspendLayout();
			this.EarliestDueDateEdit.SuspendLayout();
			this.LatestDueDteDateEdit.SuspendLayout();
			this.TotalAmountCalcFindBox.SuspendLayout();
			this.TotalOutStandingCalcFindBox.SuspendLayout();
			this.LineSummaryGridPanel.SuspendLayout();
			this.zPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 197, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(737, 22, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(TransactionCurrencySummary);
			// 
			// CurrencySummaryGrid
			// 
			this.CurrencySummaryGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CurrencySummaryGrid, "TransactionCurrencySummaryRows");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((TransactionCurrencySummary)(null)).TransactionCurrencySummaryRows)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((TransactionCurrencySummaryRow)(((System.Collections.IList)(((TransactionCurrencySummary)(null)).TransactionCurrencySummaryRows)).SyncRoot)).Currency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((TransactionCurrencySummaryRow)(((System.Collections.IList)(((TransactionCurrencySummary)(null)).TransactionCurrencySummaryRows)).SyncRoot)).CurrencyDecimals)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((TransactionCurrencySummaryRow)(((System.Collections.IList)(((TransactionCurrencySummary)(null)).TransactionCurrencySummaryRows)).SyncRoot)).Amount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((TransactionCurrencySummaryRow)(((System.Collections.IList)(((TransactionCurrencySummary)(null)).TransactionCurrencySummaryRows)).SyncRoot)).LocalAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((TransactionCurrencySummaryRow)(((System.Collections.IList)(((TransactionCurrencySummary)(null)).TransactionCurrencySummaryRows)).SyncRoot)).OutStandingOSAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((TransactionCurrencySummaryRow)(((System.Collections.IList)(((TransactionCurrencySummary)(null)).TransactionCurrencySummaryRows)).SyncRoot)).OutStandingLocalAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((TransactionCurrencySummaryRow)(((System.Collections.IList)(((TransactionCurrencySummary)(null)).TransactionCurrencySummaryRows)).SyncRoot)).AverageExRate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((TransactionCurrencySummaryRow)(((System.Collections.IList)(((TransactionCurrencySummary)(null)).TransactionCurrencySummaryRows)).SyncRoot)).TransactionCount)));
			this.CurrencySummaryGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("TransactionCurrencySummaryForm|EC511F6C-BAF5-49CC-ACAC-60A23CFBC92B", "Curr.");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "Currency";
			zCodeFindBoxColumnStyleInfo1.IsMandatory = true;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(35);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = "CurrencyDecimals";
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("TransactionCurrencySummaryForm|CA0F2088-2122-4D7C-9500-E2304DCDDC42", "Total OS Amount");
			zCalcEditColumnStyleInfo1.ColumnName = "Amount";
			zCalcEditColumnStyleInfo1.IsMandatory = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("TransactionCurrencySummaryForm|88DDE1BF-ABE1-45B8-BF1E-77F9AAAEB970", "Total Local Amount");
			zCalcEditColumnStyleInfo2.ColumnName = "LocalAmount";
			zCalcEditColumnStyleInfo2.IsMandatory = true;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("TransactionCurrencySummaryForm|0E0BBD40-DD55-44FC-A114-F390B034F6CE", "Total Outstanding OS Amount");
			zCalcEditColumnStyleInfo3.ColumnName = "OutStandingOSAmount";
			zCalcEditColumnStyleInfo3.IsMandatory = true;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("TransactionCurrencySummaryForm|F419D04A-D4D8-472B-A70D-87B418170A2E", "Total Outstanding Local Amount");
			zCalcEditColumnStyleInfo4.ColumnName = "OutStandingLocalAmount";
			zCalcEditColumnStyleInfo4.IsMandatory = true;
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(170);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("TransactionCurrencySummaryForm|99300674-DC67-4E2D-8852-A4C5855BCBD9", "Average Exchange Rate");
			zCalcEditColumnStyleInfo5.ColumnName = "AverageExRate";
			zCalcEditColumnStyleInfo5.IsMandatory = true;
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("TransactionCurrencySummaryForm|4C4EDD50-1E9D-4056-927F-7FD93291E2CA", "Transactions Count");
			zCalcEditColumnStyleInfo6.ColumnName = "TransactionCount";
			zCalcEditColumnStyleInfo6.IsMandatory = true;
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			this.CurrencySummaryGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.CurrencySummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.CurrencySummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.CurrencySummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.CurrencySummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.CurrencySummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.CurrencySummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.CurrencySummaryGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CurrencySummaryGrid.GridId = "7AA1EE72-752D-4C2F-B3CE-605F87099C63";
			this.CurrencySummaryGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CurrencySummaryGrid.IsWholeRowSelectedOnClick = true;
			this.CurrencySummaryGrid.LayoutKey = "CurrencySummaryGrid";
			this.CurrencySummaryGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CurrencySummaryGrid.Name = "CurrencySummaryGrid";
			this.CurrencySummaryGrid.ReadOnly = true;
			this.CurrencySummaryGrid.ShouldSetErrorsOnTabPage = false;
			this.CurrencySummaryGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(737, 117, true);
			this.CurrencySummaryGrid.TabIndex = 0;
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("TransactionCurrencySummaryForm|07503CDD-57F1-416D-8059-D8200E07D1C8", "Close");
			this.CloseButton.IsCaptionOverridden = false;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(659, 49, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.CloseButton.TabIndex = 0;
			this.CloseButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.CloseButton.ToolTipCaption = null;
			// 
			// EarliestDueDateEdit
			// 
			this.EarliestDueDateEdit.AllowDrop = true;
			this.EarliestDueDateEdit.AutoCompleteMonthThreshold = 1;
			this.EarliestDueDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.EarliestDueDateEdit, "EarliestDueDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((TransactionCurrencySummary)(null)).EarliestDueDate)));
			this.EarliestDueDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("TransactionCurrencySummaryForm|A2D51758-EBE9-4E2F-9AE4-94BDC677958F", "Earliest Due Date");
			this.EarliestDueDateEdit.Enabled = false;
			this.EarliestDueDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 20, true);
			this.EarliestDueDateEdit.Name = "EarliestDueDateEdit";
			this.EarliestDueDateEdit.TabIndex = 1;
			// 
			// LatestDueDteDateEdit
			// 
			this.LatestDueDteDateEdit.AllowDrop = true;
			this.LatestDueDteDateEdit.AutoCompleteMonthThreshold = 1;
			this.LatestDueDteDateEdit.AutoCompleteYear = true;
			this.LatestDueDteDateEdit.AutoSize = true;
			this.BindingSource.SetBindingMember(this.LatestDueDteDateEdit, "LatestDueDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((TransactionCurrencySummary)(null)).LatestDueDate)));
			this.LatestDueDteDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("TransactionCurrencySummaryForm|6916856F-DC4F-49BC-A382-20FBA9B1F6EA", "Latest Due Date");
			this.LatestDueDteDateEdit.Enabled = false;
			this.LatestDueDteDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(350, 20, true);
			this.LatestDueDteDateEdit.Name = "LatestDueDteDateEdit";
			this.LatestDueDteDateEdit.TabIndex = 2;
			// 
			// TotalAmountCalcFindBox
			// 
			this.TotalAmountCalcFindBox.AllowDrop = true;
			this.TotalAmountCalcFindBox.AutoSize = true;
			this.TotalAmountCalcFindBox.BindToAmount = "LocalAmountTotal";
			this.TotalAmountCalcFindBox.BindToDecimalPlaces = "LocalDecimals";
			this.TotalAmountCalcFindBox.BindToUnit = "LocalCurrency";
			this.TotalAmountCalcFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("TransactionCurrencySummaryForm|8146FE9B-479A-4EA6-AA2F-CFA2005AA421", "Total Amt.");
			this.TotalAmountCalcFindBox.Enabled = false;
			this.TotalAmountCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.TotalAmountCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 50, true);
			this.TotalAmountCalcFindBox.Name = "TotalAmountCalcFindBox";
			this.TotalAmountCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 20, true);
			this.TotalAmountCalcFindBox.TabIndex = 3;
			// 
			// TotalOutStandingCalcFindBox
			// 
			this.TotalOutStandingCalcFindBox.AllowDrop = true;
			this.TotalOutStandingCalcFindBox.BindToAmount = "OutStandingAmountTotal";
			this.TotalOutStandingCalcFindBox.BindToDecimalPlaces = "LocalDecimals";
			this.TotalOutStandingCalcFindBox.BindToUnit = "LocalCurrency";
			this.TotalOutStandingCalcFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("TransactionCurrencySummaryForm|5C17C50A-F8F4-4A0B-878B-C5EF59E3EC23", "Total Outstanding");
			this.TotalOutStandingCalcFindBox.Enabled = false;
			this.TotalOutStandingCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.TotalOutStandingCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(350, 50, true);
			this.TotalOutStandingCalcFindBox.Name = "TotalOutStandingCalcFindBox";
			this.TotalOutStandingCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 20, true);
			this.TotalOutStandingCalcFindBox.TabIndex = 4;
			// 
			// TotalCountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TotalCountCalcEdit, "TransactionsCountTotal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((TransactionCurrencySummary)(null)).TransactionsCountTotal)));
			this.TotalCountCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("TransactionCurrencySummaryForm|C7083113-8FBA-4884-B83D-BED68DFC6C70", "Total Count");
			this.TotalCountCalcEdit.DecimalPlaces = 0;
			this.TotalCountCalcEdit.Decimals = 0;
			this.TotalCountCalcEdit.Enabled = false;
			this.TotalCountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(550, 50, true);
			this.TotalCountCalcEdit.Name = "TotalCountCalcEdit";
			this.TotalCountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(35, 20, true);
			this.TotalCountCalcEdit.TabIndex = 5;
			this.TotalCountCalcEdit.Text = "0";
			this.TotalCountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// LineSummaryGridPanel
			// 
			this.LineSummaryGridPanel.Controls.Add(this.CurrencySummaryGrid);
			this.LineSummaryGridPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LineSummaryGridPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LineSummaryGridPanel.Name = "LineSummaryGridPanel";
			this.LineSummaryGridPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(737, 117, true);
			this.LineSummaryGridPanel.TabIndex = 0;
			// 
			// zPanel1
			// 
			this.zPanel1.Controls.Add(this.EarliestDueDateEdit);
			this.zPanel1.Controls.Add(this.LatestDueDteDateEdit);
			this.zPanel1.Controls.Add(this.TotalAmountCalcFindBox);
			this.zPanel1.Controls.Add(this.TotalOutStandingCalcFindBox);
			this.zPanel1.Controls.Add(this.TotalCountCalcEdit);
			this.zPanel1.Controls.Add(this.CloseButton);
			this.zPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 117, true);
			this.zPanel1.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.zPanel1.Name = "zPanel1";
			this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(737, 80, true);
			this.zPanel1.TabIndex = 0;
			// 
			// TransactionCurrencySummaryForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("TransactionCurrencySummaryForm|4D85F6F5-3CB4-4DE3-913F-87D39CC58B80", "Selected Transactions Summary");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(737, 219, true);
			this.Controls.Add(this.LineSummaryGridPanel);
			this.Controls.Add(this.zPanel1);
			this.DataSourceAssemblyName = "Enterprise.Accounting.Business";
			this.DataSourceType = typeof(TransactionCurrencySummary);
			this.DataSourceTypeName = "Enterprise.Accounting.Business.Base.Transaction.TransactionCurrencySummary";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(690, 250, true);
			this.Name = "TransactionCurrencySummaryForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.zPanel1, 0);
			this.Controls.SetChildIndex(this.LineSummaryGridPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.CurrencySummaryGrid)).EndInit();
			this.CurrencySummaryGrid.ResumeLayout(false);
			this.CurrencySummaryGrid.PerformLayout();
			this.EarliestDueDateEdit.ResumeLayout(true);
			this.EarliestDueDateEdit.PerformLayout();
			this.LatestDueDteDateEdit.ResumeLayout(true);
			this.LatestDueDteDateEdit.PerformLayout();
			this.TotalAmountCalcFindBox.ResumeLayout(true);
			this.TotalAmountCalcFindBox.PerformLayout();
			this.TotalOutStandingCalcFindBox.ResumeLayout(true);
			this.TotalOutStandingCalcFindBox.PerformLayout();
			this.LineSummaryGridPanel.ResumeLayout(false);
			this.LineSummaryGridPanel.PerformLayout();
			this.zPanel1.ResumeLayout(false);
			this.zPanel1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#region IDisposable Members

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion
		#endregion
	}
}
