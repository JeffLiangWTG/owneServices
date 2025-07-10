using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.GUI
{
	public partial class ManualScanning
	{
		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.scanLogPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.logPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.scanLogGrid = new Enterprise.ZArchitecture.ZGrid();
			this.buttonBanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.completeButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.scanResultPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.scanNoteLabel = new CargoWise.Windows.UI.KLabel();
			this.scanResultLabel = new CargoWise.Windows.UI.KLabel();
			this.surplusYesButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.surplusNoButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.scanLogPanel.SuspendLayout();
			this.logPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.scanLogGrid)).BeginInit();
			this.buttonBanel.SuspendLayout();
			this.scanResultPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 417, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(803, 25, true);
			this.MainStatusBar.Visible = false;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.ManualScanLineCollection);
			// 
			// ScanLogPanel
			// 
			this.scanLogPanel.Controls.Add(this.logPanel);
			this.scanLogPanel.Controls.Add(this.buttonBanel);
			this.scanLogPanel.Dock = System.Windows.Forms.DockStyle.Right;
			this.scanLogPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(442, 0, true);
			this.scanLogPanel.Name = "ScanLogPanel";
			this.scanLogPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(361, 417, true);
			this.scanLogPanel.TabIndex = 2;
			// 
			// LogPanel
			// 
			this.logPanel.AutoScroll = true;
			this.logPanel.Controls.Add(this.scanLogGrid);
			this.logPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.logPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.logPanel.Name = "LogPanel";
			this.logPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(361, 357, true);
			this.logPanel.TabIndex = 2;
			// 
			// ScanLogGrid
			// 
			this.scanLogGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.scanLogGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.ManualScanLine)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.AU.Declaration.Business.ManualScanLine)(null)).Time)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.ManualScanLine)(null)).Barcode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.ManualScanLine)(null)).Instruction)));
			this.scanLogGrid.CaptionVisible = false;
			zDateEditColumnStyleInfo1.ColumnName = "Time";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.TimeIncludingSeconds;
			zDateEditColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.ColumnName = "Barcode";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.ColumnName = "Instruction";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			this.scanLogGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.scanLogGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.scanLogGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.scanLogGrid.CopySelectedRowsAllowed = true;
			this.scanLogGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.scanLogGrid.GridId = "430986e5-8b5d-497d-9276-7e3277d175c0";
			this.scanLogGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.scanLogGrid.LayoutKey = "ScanLogGrid";
			this.scanLogGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.scanLogGrid.Name = "ScanLogGrid";
			this.scanLogGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(361, 357, true);
			this.scanLogGrid.TabIndex = 0;
			// 
			// ButtonBanel
			// 
			this.buttonBanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.buttonBanel.Controls.Add(this.cancelButton);
			this.buttonBanel.Controls.Add(this.completeButton);
			this.buttonBanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.buttonBanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 357, true);
			this.buttonBanel.Name = "ButtonBanel";
			this.buttonBanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(361, 60, true);
			this.buttonBanel.TabIndex = 1;
			// 
			// CancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(210, 7, true);
			this.cancelButton.Name = "CancelButton";
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 45, true);
			this.cancelButton.TabIndex = 2;
			this.cancelButton.Text = "Cancel Batch";
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// CompleteButton
			// 
			this.completeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.completeButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 7, true);
			this.completeButton.Name = "CompleteButton";
			this.completeButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 45, true);
			this.completeButton.TabIndex = 1;
			this.completeButton.Text = "Package Scan Completed - Exit";
			this.completeButton.UseVisualStyleBackColor = true;
			this.completeButton.Click += new System.EventHandler(this.CompleteButton_Click);
			// 
			// ScanResultPanel
			// 
			this.scanResultPanel.Controls.Add(this.scanNoteLabel);
			this.scanResultPanel.Controls.Add(this.scanResultLabel);
			this.scanResultPanel.Controls.Add(this.surplusYesButton);
			this.scanResultPanel.Controls.Add(this.surplusNoButton);
			this.scanResultPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.scanResultPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.scanResultPanel.Name = "ScanResultPanel";
			this.scanResultPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(442, 417, true);
			this.scanResultPanel.TabIndex = 3;
			// 
			// ScanNoteLabel
			// 
			this.scanNoteLabel.AutoSize = true;
			this.scanNoteLabel.Font = new System.Drawing.Font("Tahoma", 10F);
			this.scanNoteLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(42, 233, true);
			this.scanNoteLabel.Name = "ScanNoteLabel";
			this.scanNoteLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(37, 17, true);
			this.scanNoteLabel.TabIndex = 5;
			this.scanNoteLabel.Text = "Note";
			// 
			// ScanResultLabel
			// 
			this.scanResultLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.scanResultLabel.AutoSize = true;
			this.scanResultLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.scanResultLabel.Font = new System.Drawing.Font("Tahoma", 30F);
			this.scanResultLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(38, 159, true);
			this.scanResultLabel.Name = "ScanResultLabel";
			this.scanResultLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(375, 50, true);
			this.scanResultLabel.TabIndex = 4;
			this.scanResultLabel.Text = "Awaiting Scan Input";
			// 
			// SurplusYesButton
			// 
			this.surplusYesButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.surplusYesButton.Font = new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Bold);
			this.surplusYesButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(46, 293, true);
			this.surplusYesButton.Name = "SurplusYesButton";
			this.surplusYesButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 45, true);
			this.surplusYesButton.TabIndex = 3;
			this.surplusYesButton.Text = "Surplus - Yes";
			this.surplusYesButton.UseVisualStyleBackColor = true;
			this.surplusYesButton.Click += new System.EventHandler(this.SurplusYesButton_Click);
			// 
			// SurplusNoButton
			// 
			this.surplusNoButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.surplusNoButton.Font = new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Bold);
			this.surplusNoButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(259, 293, true);
			this.surplusNoButton.Name = "SurplusNoButton";
			this.surplusNoButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 45, true);
			this.surplusNoButton.TabIndex = 2;
			this.surplusNoButton.Text = "Rescan";
			this.surplusNoButton.UseVisualStyleBackColor = true;
			this.surplusNoButton.Click += new System.EventHandler(this.SurplusNoButton_Click);
			// 
			// ManualScanning
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(803, 442, true);
			this.Controls.Add(this.scanResultPanel);
			this.Controls.Add(this.scanLogPanel);
			this.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.ManualScanLineCollection);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "ManualScanning";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.scanLogPanel, 0);
			this.Controls.SetChildIndex(this.scanResultPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.scanLogPanel.ResumeLayout(false);
			this.logPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.scanLogGrid)).EndInit();
			this.buttonBanel.ResumeLayout(false);
			this.scanResultPanel.ResumeLayout(false);
			this.scanResultPanel.PerformLayout();
			this.ResumeLayout(false);
		}

		ZPanel scanLogPanel;
		ZArchitecture.ZGrid scanLogGrid;
		ZButton cancelButton;
		ZButton completeButton;
		ZPanel logPanel;
		ZPanel buttonBanel;
		ZButton surplusYesButton;
		ZButton surplusNoButton;
		CargoWise.Windows.UI.KLabel scanResultLabel;
		CargoWise.Windows.UI.KLabel scanNoteLabel;
		ZPanel scanResultPanel;
	}
}
