using System;
using Enterprise.Accounting.Business.PeriodManagement;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public partial class PeriodEndDateEditForm
	{


		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.PeriodsGrid = new ZArchitecture.ZGrid();
			this.TopPanel = new ZPanel();
			this.FinancialYearEdit = new ZYearEdit();
			this.BottomPanel = new ZPanel();
			this.CloseButton = new Core.Forms.ZPostOrCancelButton();
			this.OKButton = new Core.Forms.ZPostOrCancelButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PeriodsGrid)).BeginInit();
			this.TopPanel.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 388, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(568, 22, true);
			this.MainStatusBar.TabIndex = 3;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(276);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(277);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(PeriodManager);
			// 
			// PeriodsGrid
			// 
			this.PeriodsGrid.AllowNavigation = false;
			this.PeriodsGrid.AllowSorting = false;
			this.BindingSource.SetBindingMember(this.PeriodsGrid, "Periods");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((PeriodManager)(null)).Periods)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Period)(((System.Collections.IList)(((PeriodManager)(null)).Periods)).SyncRoot)).AM_Year)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Period)(((System.Collections.IList)(((PeriodManager)(null)).Periods)).SyncRoot)).AM_Period)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Period)(((System.Collections.IList)(((PeriodManager)(null)).Periods)).SyncRoot)).AM_StartDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Period)(((System.Collections.IList)(((PeriodManager)(null)).Periods)).SyncRoot)).AM_EndDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Period)(((System.Collections.IList)(((PeriodManager)(null)).Periods)).SyncRoot)).AM_IsSubLedgerClosed)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Period)(((System.Collections.IList)(((PeriodManager)(null)).Periods)).SyncRoot)).AM_IsGeneralLedgerClosed)));
			this.PeriodsGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "AM_Year";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.IsReadOnly = false;
			zCalcEditColumnStyleInfo1.ShowGroupSeparators = false;
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "AM_Period";
			zCalcEditColumnStyleInfo2.Decimals = 0;
			zCalcEditColumnStyleInfo2.IsReadOnly = false;
			zCalcEditColumnStyleInfo2.ShowGroupSeparators = false;
			zDateEditColumnStyleInfo1.ColumnName = "AM_StartDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.ColumnName = "AM_EndDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zCheckBoxColumnStyleInfo1.ColumnName = "AM_IsSubLedgerClosed";
			zCheckBoxColumnStyleInfo1.IsReadOnly = true;
			zCheckBoxColumnStyleInfo2.ColumnName = "AM_IsGeneralLedgerClosed";
			zCheckBoxColumnStyleInfo2.IsReadOnly = true;
			this.PeriodsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.PeriodsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.PeriodsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.PeriodsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.PeriodsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.PeriodsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.PeriodsGrid.GridId = "582b85b7-6fc8-47dc-adfa-c24d4a2c40cb";
			this.PeriodsGrid.CopySelectedRowsAllowed = true;
			this.PeriodsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PeriodsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PeriodsGrid.LayoutKey = "PeriodsGrid";
			this.PeriodsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 32, true);
			this.PeriodsGrid.Name = "PeriodsGrid";
			this.PeriodsGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.PeriodsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(568, 321, true);
			this.PeriodsGrid.TabIndex = 1;
			// 
			// TopPanel
			// 
			this.TopPanel.Controls.Add(this.FinancialYearEdit);
			this.TopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TopPanel.Name = "TopPanel";
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(568, 32, true);
			this.TopPanel.TabIndex = 0;
			// 
			// FinancialYearEdit
			// 
			this.BindingSource.SetBindingMember(this.FinancialYearEdit, "FinancialYear");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((PeriodManager)(null)).FinancialYear)));
			this.FinancialYearEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PeriodEndDateEditForm|e9b6e56e-2ffc-4aba-88e6-b2832e4ce225", "Financial year");
			this.FinancialYearEdit.DecimalPlaces = 0;
			this.FinancialYearEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 6, true);
			this.FinancialYearEdit.Name = "FinancialYearEdit";
			this.FinancialYearEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.FinancialYearEdit.TabIndex = 0;
			this.FinancialYearEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.CloseButton);
			this.BottomPanel.Controls.Add(this.OKButton);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 353, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(568, 35, true);
			this.BottomPanel.TabIndex = 2;
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PeriodEndDateEditForm|1f6006fb-08b7-4e38-b38f-d9ebc02bc84c", "Cancel");
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(468, 8, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 21, true);
			this.CloseButton.TabIndex = 1;
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PeriodEndDateEditForm|b0cd271b-12ae-4e76-862e-962d3309b48f", "OK");
			this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(372, 8, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 21, true);
			this.OKButton.TabIndex = 0;
			// 
			// PeriodEndDateEditForm
			// 

			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(568, 410, true);
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PeriodEndDateEditForm|e3ca4476-b6c1-4908-bb13-284c477e51ab", "Period End Date Edit");
			this.Controls.Add(this.PeriodsGrid);
			this.Controls.Add(this.BottomPanel);
			this.Controls.Add(this.TopPanel);
			this.DataSourceAssemblyName = "Enterprise.Accounting.Business";
			this.DataSourceType = typeof(PeriodManager);
			this.DataSourceTypeName = "Enterprise.Accounting.Business.PeriodManagement.PeriodManager";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(576, 446, true);
			this.Name = "PeriodEndDateEditForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.TopPanel, 0);
			this.Controls.SetChildIndex(this.BottomPanel, 0);
			this.Controls.SetChildIndex(this.PeriodsGrid, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PeriodsGrid)).EndInit();
			this.TopPanel.ResumeLayout(false);
			this.TopPanel.PerformLayout();
			this.BottomPanel.ResumeLayout(false);
			this.ResumeLayout(false);
		}
		#endregion

	}
}