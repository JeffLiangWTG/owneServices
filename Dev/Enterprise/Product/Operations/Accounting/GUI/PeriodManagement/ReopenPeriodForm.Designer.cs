using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.PeriodManagement
{
	public partial class ReopenPeriodForm
	{

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.ReSetPeriodsGroupBox = new ZGroupBox();
			this.CloseButton = new ZButton();
			this.ReopenForAdjustmentsCheckBox = new ZCheckBox();
			this.ReopenGeneralLedgerPeriodCheckBox = new ZCheckBox();
			this.ReopenSubLedgerPeriodCheckBox = new ZCheckBox();
			this.ReopenButton = new ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ReSetPeriodsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 169, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(339, 26, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(146);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(146);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.PeriodManagement.NewYearPeriodSettings);
			// 
			// ReSetPeriodsGroupBox
			// 
			this.ReSetPeriodsGroupBox.Controls.Add(this.CloseButton);
			this.ReSetPeriodsGroupBox.Controls.Add(this.ReopenForAdjustmentsCheckBox);
			this.ReSetPeriodsGroupBox.Controls.Add(this.ReopenGeneralLedgerPeriodCheckBox);
			this.ReSetPeriodsGroupBox.Controls.Add(this.ReopenSubLedgerPeriodCheckBox);
			this.ReSetPeriodsGroupBox.Controls.Add(this.ReopenButton);
			this.ReSetPeriodsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ReSetPeriodsGroupBox, false);
			this.ReSetPeriodsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ReSetPeriodsGroupBox.Name = "ReSetPeriodsGroupBox";
			this.ReSetPeriodsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(339, 169, true);
			this.ReSetPeriodsGroupBox.TabIndex = 1;
			this.ReSetPeriodsGroupBox.TabStop = false;
			// 
			// CloseButton
			// 
			this.CloseButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReopenPeriodForm|2cfbe676-98b4-415e-8621-65867948a8d7", "Cancel");
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(182, 125, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(122, 30, true);
			this.CloseButton.TabIndex = 7;
			this.CloseButton.Click += new EventHandler(this.CancelButton_Click);
			// 
			// ReopenForAdjustmentsCheckBox
			// 
			this.ReopenForAdjustmentsCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReopenPeriodForm|2d808854-600c-4657-908f-623f169521af", "Reopen for Adjustments");
			this.ReopenForAdjustmentsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ReopenForAdjustmentsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(42, 83, true);
			this.ReopenForAdjustmentsCheckBox.Name = "ReopenForAdjustmentsCheckBox";
			this.ReopenForAdjustmentsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 22, true);
			this.ReopenForAdjustmentsCheckBox.TabIndex = 5;
			// 
			// ReopenGeneralLedgerPeriodCheckBox
			// 
			this.ReopenGeneralLedgerPeriodCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReopenPeriodForm|348e4433-6dec-4e99-bf5a-c54429badf1d", "Reopen General Ledger Period");
			this.ReopenGeneralLedgerPeriodCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ReopenGeneralLedgerPeriodCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(42, 55, true);
			this.ReopenGeneralLedgerPeriodCheckBox.Name = "ReopenGeneralLedgerPeriodCheckBox";
			this.ReopenGeneralLedgerPeriodCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(174, 22, true);
			this.ReopenGeneralLedgerPeriodCheckBox.TabIndex = 4;
			// 
			// ReopenSubLedgerPeriodCheckBox
			// 
			this.ReopenSubLedgerPeriodCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReopenPeriodForm|ee38a052-cbe4-43dd-8691-157b40c9bd52", "Reopen Sub Ledger Period");
			this.ReopenSubLedgerPeriodCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ReopenSubLedgerPeriodCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(42, 27, true);
			this.ReopenSubLedgerPeriodCheckBox.Name = "ReopenSubLedgerPeriodCheckBox";
			this.ReopenSubLedgerPeriodCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 22, true);
			this.ReopenSubLedgerPeriodCheckBox.TabIndex = 3;
			// 
			// ReopenButton
			// 
			this.ReopenButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReopenPeriodForm|21fc6aec-415d-41bb-af71-b56a1ce1e8cb", "Reopen Period");
			this.ReopenButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(42, 125, true);
			this.ReopenButton.Name = "ReopenButton";
			this.ReopenButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(122, 30, true);
			this.ReopenButton.TabIndex = 6;
			this.ReopenButton.Click += new EventHandler(this.ReopenButton_Click);
			// 
			// ReopenPeriodForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(339, 195, true);
			this.Controls.Add(this.ReSetPeriodsGroupBox);
			this.DataSourceAssemblyName = "Enterprise.Accounting.Business";
			this.DataSourceType = typeof(Business.PeriodManagement.NewYearPeriodSettings);
			this.DataSourceTypeName = "Enterprise.Accounting.Business.PeriodManagement.NewYearPeriodSettings";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(352, 231, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(352, 231, true);
			this.Name = "ReopenPeriodForm";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.ReSetPeriodsGroupBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ReSetPeriodsGroupBox.ResumeLayout(false);
			this.ReSetPeriodsGroupBox.PerformLayout();
			this.ResumeLayout(false);
		}
		#endregion

	}
}
