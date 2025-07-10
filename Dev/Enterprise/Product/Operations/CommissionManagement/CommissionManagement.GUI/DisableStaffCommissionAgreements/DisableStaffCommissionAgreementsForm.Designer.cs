namespace Enterprise.CommissionManagement.GUI
{
	partial class DisableStaffCommissionAgreementsForm
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
			this.yesButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.yesAndApproveButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.noButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.buttonsTableLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.buttonsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ShouldUpdateEarlierEndDatesCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.EndDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.MessageLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.buttonsTableLayoutPanel.SuspendLayout();
			this.buttonsPanel.SuspendLayout();
			this.EndDateEdit.SuspendLayout();
			this.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 131, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(437, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.CommissionManagement.Business.DisableStaffCommissionAgreementsAction);
			// 
			// yesButton
			// 
			this.yesButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.yesButton.CaptionResourceString = Enterprise.CommissionManagement.GUI.Res.GetData("db87565a-a33e-4fff-9c3d-41397cf12ad3", "Yes");
			this.yesButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 47, true);
			this.yesButton.Name = "yesButton";
			this.yesButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 23, true);
			this.yesButton.TabIndex = 2;
			this.yesButton.Click += new System.EventHandler(this.YesButton_Click);
			// 
			// yesAndApproveButton
			// 
			this.yesAndApproveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.yesAndApproveButton.CaptionResourceString = Enterprise.CommissionManagement.GUI.Res.GetData("b13e69f4-a503-4c45-a414-6f6dd0f9c063", "Yes && Approve");
			this.yesAndApproveButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 47, true);
			this.yesAndApproveButton.Name = "yesAndApproveButton";
			this.yesAndApproveButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 23, true);
			this.yesAndApproveButton.TabIndex = 3;
			this.yesAndApproveButton.Click += new System.EventHandler(this.YesAndApproveButton_Click);
			// 
			// noButton
			// 
			this.noButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.noButton.CaptionResourceString = Enterprise.CommissionManagement.GUI.Res.GetData("e4d8cf68-e9cb-4d6b-a759-8a89390e968c", "No");
			this.noButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.noButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(194, 47, true);
			this.noButton.Name = "noButton";
			this.noButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 23, true);
			this.noButton.TabIndex = 4;
			this.noButton.Click += new System.EventHandler(this.NoButton_Click);
			// 
			// buttonsTableLayoutPanel
			// 
			this.buttonsTableLayoutPanel.ColumnCount = 3;
			this.buttonsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.buttonsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.buttonsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.buttonsTableLayoutPanel.Controls.Add(this.buttonsPanel, 1, 0);
			this.buttonsTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.buttonsTableLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 46, true);
			this.buttonsTableLayoutPanel.Name = "buttonsTableLayoutPanel";
			this.buttonsTableLayoutPanel.RowCount = 1;
			this.buttonsTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.buttonsTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(20)));
			this.buttonsTableLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(437, 85, true);
			this.buttonsTableLayoutPanel.TabIndex = 1;
			// 
			// buttonsPanel
			// 
			this.buttonsPanel.Controls.Add(this.ShouldUpdateEarlierEndDatesCheckBox);
			this.buttonsPanel.Controls.Add(this.EndDateEdit);
			this.buttonsPanel.Controls.Add(this.noButton);
			this.buttonsPanel.Controls.Add(this.yesAndApproveButton);
			this.buttonsPanel.Controls.Add(this.yesButton);
			this.buttonsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 2, true);
			this.buttonsPanel.Name = "buttonsPanel";
			this.buttonsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(277, 71, true);
			this.buttonsPanel.TabIndex = 0;
			// 
			// ShouldUpdateEarlierEndDatesCheckBox
			// 
			this.ShouldUpdateEarlierEndDatesCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ShouldUpdateEarlierEndDatesCheckBox, "ShouldUpdateEarlierEndDates");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.CommissionManagement.Business.DisableStaffCommissionAgreementsAction)(null)).ShouldUpdateEarlierEndDates)));
			this.ShouldUpdateEarlierEndDatesCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ShouldUpdateEarlierEndDatesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 29, true);
			this.ShouldUpdateEarlierEndDatesCheckBox.Name = "ShouldUpdateEarlierEndDatesCheckBox";
			this.ShouldUpdateEarlierEndDatesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(268, 16, true);
			this.ShouldUpdateEarlierEndDatesCheckBox.TabIndex = 1;
			this.ShouldUpdateEarlierEndDatesCheckBox.UseVisualStyleBackColor = true;
			// 
			// EndDateEdit
			// 
			this.EndDateEdit.AllowDrop = true;
			this.EndDateEdit.AutoCompleteMonthThreshold = 1;
			this.EndDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.EndDateEdit, "Date");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.CommissionManagement.Business.DisableStaffCommissionAgreementsAction)(null)).Date)));
			this.EndDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 4, true);
			this.EndDateEdit.Name = "EndDateEdit";
			this.EndDateEdit.TabIndex = 0;
			// 
			// MessageLabel
			// 
			this.MessageLabel.CaptionResourceString = Enterprise.CommissionManagement.GUI.Res.GetData("36af17a2-ea6e-487d-98a6-db94965bd36a", "Message");
			this.MessageLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessageLabel.Name = "MessageLabel";
			this.MessageLabel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(10, true);
			this.MessageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(437, 46, true);
			this.MessageLabel.TabIndex = 0;
			// 
			// DisableStaffCommissionAgreementsForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.noButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.CommissionManagement.GUI.Res.GetData("7f649898-a9b3-4f62-a185-fe6b331b8297", "Disable Staff Commission Agreement Rates");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(437, 155, true);
			this.Controls.Add(this.MessageLabel);
			this.Controls.Add(this.buttonsTableLayoutPanel);
			this.DataSourceType = typeof(Enterprise.CommissionManagement.Business.DisableStaffCommissionAgreementsAction);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "DisableStaffCommissionAgreementsForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.buttonsTableLayoutPanel, 0);
			this.Controls.SetChildIndex(this.MessageLabel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.buttonsTableLayoutPanel.ResumeLayout(false);
			this.buttonsTableLayoutPanel.PerformLayout();
			this.buttonsPanel.ResumeLayout(false);
			this.buttonsPanel.PerformLayout();
			this.EndDateEdit.ResumeLayout(true);
			this.EndDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.GUI.ZButton yesButton;
		private ZArchitecture.GUI.ZButton yesAndApproveButton;
		private ZArchitecture.GUI.ZButton noButton;
		private CargoWise.Windows.UI.KTableLayoutPanel buttonsTableLayoutPanel;
		private ZArchitecture.GUI.ZPanel buttonsPanel;
		private ZArchitecture.GUI.ZDateEdit EndDateEdit;
		private ZArchitecture.ZLabel MessageLabel;
		private ZArchitecture.GUI.ZCheckBox ShouldUpdateEarlierEndDatesCheckBox;
	}
}