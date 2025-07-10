namespace Enterprise.CommissionManagement.GUI
{
	partial class BulkCancelCommissionLinesForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.CancelCommissionLineActionsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.PostingButtonsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.RemoveErrorLinesButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.PostingButtons = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.CancelCommissionLineActionsGrid)).BeginInit();
			this.CancelCommissionLineActionsGrid.SuspendLayout();
			this.PostingButtonsPanel.SuspendLayout();
			this.PostingButtons.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 278, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(881, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.CommissionManagement.Business.BulkCancelCommissionLinesAction);
			// 
			// CancelCommissionLineActionsGrid
			// 
			this.CancelCommissionLineActionsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CancelCommissionLineActionsGrid, "CancelCommissionLineActionCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.CommissionManagement.Business.BulkCancelCommissionLinesAction)(null)).CancelCommissionLineActionCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.CommissionManagement.Business.CancelCommissionLineAction)(((System.Collections.IList)(((Enterprise.CommissionManagement.Business.BulkCancelCommissionLinesAction)(null)).CancelCommissionLineActionCollection)).SyncRoot)).CommissionLine.CommissionHeader.CH0_GC)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.CommissionManagement.Business.CancelCommissionLineAction)(((System.Collections.IList)(((Enterprise.CommissionManagement.Business.BulkCancelCommissionLinesAction)(null)).CancelCommissionLineActionCollection)).SyncRoot)).CommissionLine.CommissionHeader.GroupingSourceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.CommissionManagement.Business.CancelCommissionLineAction)(((System.Collections.IList)(((Enterprise.CommissionManagement.Business.BulkCancelCommissionLinesAction)(null)).CancelCommissionLineActionCollection)).SyncRoot)).CommissionLine.EntityCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.CommissionManagement.Business.CancelCommissionLineAction)(((System.Collections.IList)(((Enterprise.CommissionManagement.Business.BulkCancelCommissionLinesAction)(null)).CancelCommissionLineActionCollection)).SyncRoot)).CommissionLine.EntityName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.CommissionManagement.Business.CancelCommissionLineAction)(((System.Collections.IList)(((Enterprise.CommissionManagement.Business.BulkCancelCommissionLinesAction)(null)).CancelCommissionLineActionCollection)).SyncRoot)).CommissionLine.CommissionHeader.CH0_OH_Customer)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.CommissionManagement.Business.CancelCommissionLineAction)(((System.Collections.IList)(((Enterprise.CommissionManagement.Business.BulkCancelCommissionLinesAction)(null)).CancelCommissionLineActionCollection)).SyncRoot)).CommissionLine.CommissionHeader.CH0_OH_Debtor)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.CommissionManagement.Business.CancelCommissionLineAction)(((System.Collections.IList)(((Enterprise.CommissionManagement.Business.BulkCancelCommissionLinesAction)(null)).CancelCommissionLineActionCollection)).SyncRoot)).CommissionLine.VCL_AC)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.CommissionManagement.Business.CancelCommissionLineAction)(((System.Collections.IList)(((Enterprise.CommissionManagement.Business.BulkCancelCommissionLinesAction)(null)).CancelCommissionLineActionCollection)).SyncRoot)).CommissionLine.VCL_TransactionAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.CommissionManagement.Business.CancelCommissionLineAction)(((System.Collections.IList)(((Enterprise.CommissionManagement.Business.BulkCancelCommissionLinesAction)(null)).CancelCommissionLineActionCollection)).SyncRoot)).CommissionLine.VCL_RX_NKTransactionCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.CommissionManagement.Business.CancelCommissionLineAction)(((System.Collections.IList)(((Enterprise.CommissionManagement.Business.BulkCancelCommissionLinesAction)(null)).CancelCommissionLineActionCollection)).SyncRoot)).CommissionLine.VCL_TotalCommissionableAmountInLocalCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.CommissionManagement.Business.CancelCommissionLineAction)(((System.Collections.IList)(((Enterprise.CommissionManagement.Business.BulkCancelCommissionLinesAction)(null)).CancelCommissionLineActionCollection)).SyncRoot)).CommissionLine.VCL_RX_NKLocalCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.CommissionManagement.Business.CancelCommissionLineAction)(((System.Collections.IList)(((Enterprise.CommissionManagement.Business.BulkCancelCommissionLinesAction)(null)).CancelCommissionLineActionCollection)).SyncRoot)).CommissionLine.VCL_EntityCommissionAmountInPreferredCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.CommissionManagement.Business.CancelCommissionLineAction)(((System.Collections.IList)(((Enterprise.CommissionManagement.Business.BulkCancelCommissionLinesAction)(null)).CancelCommissionLineActionCollection)).SyncRoot)).CommissionLine.VCL_RX_NKPreferredPaymentCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.CommissionManagement.Business.CancelCommissionLineAction)(((System.Collections.IList)(((Enterprise.CommissionManagement.Business.BulkCancelCommissionLinesAction)(null)).CancelCommissionLineActionCollection)).SyncRoot)).CommissionLine.CommissionStatusDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.CommissionManagement.Business.CancelCommissionLineAction)(((System.Collections.IList)(((Enterprise.CommissionManagement.Business.BulkCancelCommissionLinesAction)(null)).CancelCommissionLineActionCollection)).SyncRoot)).CommissionLine.IsCancelled)));
			this.CancelCommissionLineActionsGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.ColumnName = "CommissionLine+CommissionHeader+CH0_GC";
			zGuidFindBoxColumnStyleInfo1.IsReadOnly = true;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo1.ColumnName = "CommissionLine+CommissionHeader+GroupingSourceNumber";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo2.ColumnName = "CommissionLine+EntityCode";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo3.ColumnName = "CommissionLine+EntityName";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zGuidFindBoxColumnStyleInfo2.ColumnName = "CommissionLine+CommissionHeader+CH0_OH_Customer";
			zGuidFindBoxColumnStyleInfo2.IsReadOnly = true;
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo3.ColumnName = "CommissionLine+CommissionHeader+CH0_OH_Debtor";
			zGuidFindBoxColumnStyleInfo3.IsReadOnly = true;
			zGuidFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo4.ColumnName = "CommissionLine+VCL_AC";
			zGuidFindBoxColumnStyleInfo4.IsReadOnly = true;
			zGuidFindBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "CommissionLine+VCL_TransactionAmount";
			zCalcEditColumnStyleInfo1.IsReadOnly = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zTextBoxColumnStyleInfo4.ColumnName = "CommissionLine+VCL_RX_NKTransactionCurrency";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "CommissionLine+VCL_TotalCommissionableAmountInLocalCurrency";
			zCalcEditColumnStyleInfo2.IsReadOnly = true;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zTextBoxColumnStyleInfo5.ColumnName = "CommissionLine+VCL_RX_NKLocalCurrency";
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "CommissionLine+VCL_EntityCommissionAmountInPreferredCurrency";
			zCalcEditColumnStyleInfo3.IsReadOnly = true;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zTextBoxColumnStyleInfo6.ColumnName = "CommissionLine+VCL_RX_NKPreferredPaymentCurrency";
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo7.ColumnName = "CommissionLine+CommissionStatusDescription";
			zTextBoxColumnStyleInfo7.IsReadOnly = true;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo1.ColumnName = "CommissionLine+IsCancelled";
			zCheckBoxColumnStyleInfo1.IsReadOnly = true;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			this.CancelCommissionLineActionsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.CancelCommissionLineActionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CancelCommissionLineActionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.CancelCommissionLineActionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.CancelCommissionLineActionsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.CancelCommissionLineActionsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo3);
			this.CancelCommissionLineActionsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo4);
			this.CancelCommissionLineActionsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.CancelCommissionLineActionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.CancelCommissionLineActionsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.CancelCommissionLineActionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.CancelCommissionLineActionsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.CancelCommissionLineActionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.CancelCommissionLineActionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.CancelCommissionLineActionsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.CancelCommissionLineActionsGrid.CopySelectedRowsAllowed = true;
			this.CancelCommissionLineActionsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CancelCommissionLineActionsGrid.GridId = "9e0a0cc5-9d58-4c49-87c6-13d619959422";
			this.CancelCommissionLineActionsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CancelCommissionLineActionsGrid.LayoutKey = "CancelCommissionLineActionsGrid";
			this.CancelCommissionLineActionsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.CancelCommissionLineActionsGrid.Name = "CancelCommissionLineActionsGrid";
			this.CancelCommissionLineActionsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(881, 245, true);
			this.CancelCommissionLineActionsGrid.TabIndex = 1;
			// 
			// PostingButtonsPanel
			// 
			this.PostingButtonsPanel.Controls.Add(this.RemoveErrorLinesButton);
			this.PostingButtonsPanel.Controls.Add(this.PostingButtons);
			this.PostingButtonsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.PostingButtonsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 248, true);
			this.PostingButtonsPanel.Name = "PostingButtonsPanel";
			this.PostingButtonsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(881, 30, true);
			this.PostingButtonsPanel.TabIndex = 2;
			// 
			// RemoveErrorLinesButton
			// 
			this.RemoveErrorLinesButton.CaptionResourceString = Enterprise.CommissionManagement.GUI.Res.GetData("5fbe8ac7-3afb-4059-b3ff-3013b52cee1b", "Remove Entity Commissions That Can\'t Be Canceled");
			this.RemoveErrorLinesButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 5, true);
			this.RemoveErrorLinesButton.Name = "RemoveErrorLinesButton";
			this.RemoveErrorLinesButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(275, 23, true);
			this.RemoveErrorLinesButton.TabIndex = 1;
			this.RemoveErrorLinesButton.Click += new System.EventHandler(this.RemoveErrorLinesButton_Click);
			// 
			// PostingButtons
			// 
			this.PostingButtons.AllowDrop = true;
			this.PostingButtons.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtons.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(640, 5, true);
			this.PostingButtons.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtons.Name = "PostingButtons";
			this.PostingButtons.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtons.TabIndex = 0;
			// 
			// BulkCancelCommissionLinesForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.CommissionManagement.GUI.Res.GetData("4d8eb2d9-ea97-4be8-aafe-145fdf8eb6c3", "Cancel Multiple Entity Commissions");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(887, 305, true);
			this.Controls.Add(this.CancelCommissionLineActionsGrid);
			this.Controls.Add(this.PostingButtonsPanel);
			this.DataSourceType = typeof(Enterprise.CommissionManagement.Business.BulkCancelCommissionLinesAction);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 300, true);
			this.Name = "BulkCancelCommissionLinesForm";
			this.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.PostingButtonsPanel, 0);
			this.Controls.SetChildIndex(this.CancelCommissionLineActionsGrid, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.CancelCommissionLineActionsGrid)).EndInit();
			this.CancelCommissionLineActionsGrid.ResumeLayout(false);
			this.CancelCommissionLineActionsGrid.PerformLayout();
			this.PostingButtonsPanel.ResumeLayout(false);
			this.PostingButtonsPanel.PerformLayout();
			this.PostingButtons.ResumeLayout(true);
			this.PostingButtons.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZGrid CancelCommissionLineActionsGrid;
		private ZArchitecture.GUI.ZPanel PostingButtonsPanel;
		private Core.Forms.ZPostingButtonsUserControl PostingButtons;
		protected ZArchitecture.GUI.ZButton RemoveErrorLinesButton;

	}
}