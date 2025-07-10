using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI
{
	partial class ManualSubmissionForm
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
		protected new void InitializeComponent()
		{
			this.GroupBoxSubmission = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.messageTypeManualSubmission = new Enterprise.ZArchitecture.ZTextBox();
			this.ManualSubmissionDate = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.PortOfClearanceFindGoxFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ButtonDeleteRELSubmission = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ButtonSaveSubmission = new Enterprise.ZArchitecture.GUI.ZButton();
			this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ButtonClose = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.GroupBoxSubmission.SuspendLayout();
			this.ManualSubmissionDate.SuspendLayout();
			this.PortOfClearanceFindGoxFindBox.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 138, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(410, 24, true);
			this.MainStatusBar.TabIndex = 2;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.ManualSubmissionBO);
			// 
			// GroupBoxSubmission
			// 
			this.GroupBoxSubmission.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("fbde46c4-9aab-4c7f-9348-ad1ff801c08b", "Manual Submission");
			this.GroupBoxSubmission.Controls.Add(this.messageTypeManualSubmission);
			this.GroupBoxSubmission.Controls.Add(this.ManualSubmissionDate);
			this.GroupBoxSubmission.Controls.Add(this.PortOfClearanceFindGoxFindBox);
			this.GroupBoxSubmission.Dock = System.Windows.Forms.DockStyle.Top;
			this.GroupBoxSubmission.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GroupBoxSubmission.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, 2, 2, 4, true);
			this.GroupBoxSubmission.Name = "GroupBoxSubmission";
			this.GroupBoxSubmission.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(410, 103, true);
			this.GroupBoxSubmission.TabIndex = 0;
			this.GroupBoxSubmission.TabStop = false;
			// 
			// messageTypeManualSubmission
			// 
			this.BindingSource.SetBindingMember(this.messageTypeManualSubmission, "CurrentEntrySubmissionBO.MessageType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.ManualSubmissionBO)(null)).CurrentEntrySubmissionBO.MessageType)));
			this.messageTypeManualSubmission.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("ba1af5c1-3b80-483b-871b-c799600c99b7", "Message Type", "Message Type", "Message Type", "");
			this.messageTypeManualSubmission.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 21, true);
			this.messageTypeManualSubmission.Name = "messageTypeManualSubmission";
			this.messageTypeManualSubmission.ReadOnly = true;
			this.messageTypeManualSubmission.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(49, 18, true);
			this.messageTypeManualSubmission.TabIndex = 0;
			this.messageTypeManualSubmission.TabStop = false;
			// 
			// ManualSubmissionDate
			// 
			this.ManualSubmissionDate.AllowDrop = true;
			this.ManualSubmissionDate.AutoCompleteMonthThreshold = 1;
			this.ManualSubmissionDate.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ManualSubmissionDate, "CurrentEntrySubmissionBO.ManualSubmissionDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.ManualSubmissionBO)(null)).CurrentEntrySubmissionBO.ManualSubmissionDate)));
			this.ManualSubmissionDate.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("9f770270-7b22-45f4-91ae-95e8d69a119a", "Submission Date", "Submission Date", "Submission Date", "Manual Submission Date");
			this.ManualSubmissionDate.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.ManualSubmissionDate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 46, true);
			this.ManualSubmissionDate.Name = "ManualSubmissionDate";
			this.ManualSubmissionDate.TabIndex = 1;
			// 
			// PortOfClearanceFindGoxFindBox
			// 
			this.PortOfClearanceFindGoxFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PortOfClearanceFindGoxFindBox, "CurrentEntrySubmissionBO.PortOfClearanceOverride");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.ManualSubmissionBO)(null)).CurrentEntrySubmissionBO.PortOfClearanceOverride)));
			this.PortOfClearanceFindGoxFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("fe497143-aa7f-4e7a-88bb-fd95f56c6af6", "Port of Clearance", "Port of Clearance", "Port of Clearance", "Port of Clearance Override");
			this.PortOfClearanceFindGoxFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 71, true);
			this.PortOfClearanceFindGoxFindBox.Name = "PortOfClearanceFindGoxFindBox";
			this.PortOfClearanceFindGoxFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(295, 18, true);
			this.PortOfClearanceFindGoxFindBox.TabIndex = 2;
			// 
			// ButtonDeleteRELSubmission
			// 
			this.ButtonDeleteRELSubmission.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonDeleteRELSubmission.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("c7443816-d40b-431b-bd78-0eb1006b0ec3", "Delete", "Delete", "Delete", "");
			this.ButtonDeleteRELSubmission.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(230, 7, true);
			this.ButtonDeleteRELSubmission.Name = "ButtonDeleteRELSubmission";
			this.ButtonDeleteRELSubmission.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ButtonDeleteRELSubmission.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 23, true);
			this.ButtonDeleteRELSubmission.TabIndex = 1;
			this.ButtonDeleteRELSubmission.UseVisualStyleBackColor = true;
			this.ButtonDeleteRELSubmission.Click += new System.EventHandler(this.OnButtonDeleteRelSubmissionClick);
			// 
			// ButtonSaveSubmission
			// 
			this.ButtonSaveSubmission.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonSaveSubmission.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("302840d7-082a-4646-9ff6-d895e1d9b345", "Save");
			this.ButtonSaveSubmission.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 7, true);
			this.ButtonSaveSubmission.Name = "ButtonSaveSubmission";
			this.ButtonSaveSubmission.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ButtonSaveSubmission.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 23, true);
			this.ButtonSaveSubmission.TabIndex = 0;
			this.ButtonSaveSubmission.UseVisualStyleBackColor = true;
			this.ButtonSaveSubmission.Click += new System.EventHandler(this.OnButtonSaveSubmissionClick);
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.ButtonDeleteRELSubmission);
			this.BottomPanel.Controls.Add(this.ButtonClose);
			this.BottomPanel.Controls.Add(this.ButtonSaveSubmission);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 103, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(410, 35, true);
			this.BottomPanel.TabIndex = 1;
			// 
			// ButtonClose
			// 
			this.ButtonClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonClose.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("33e791bd-8074-4cc7-aaf5-199a84adf7fe", "Cancel");
			this.ButtonClose.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(317, 7, true);
			this.ButtonClose.Name = "ButtonClose";
			this.ButtonClose.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ButtonClose.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 23, true);
			this.ButtonClose.TabIndex = 2;
			this.ButtonClose.UseVisualStyleBackColor = true;
			this.ButtonClose.Click += new System.EventHandler(this.OnButtonCloseClick);
			// 
			// ManualSubmissionForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("73b3404e-9f8c-4075-96c0-c5fb94a27a3c", "Manual Submission");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(410, 162, true);
			this.Controls.Add(this.BottomPanel);
			this.Controls.Add(this.GroupBoxSubmission);
			this.DataSourceType = typeof(Enterprise.Customs.CA.Business.ManualSubmissionBO);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(424, 200, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(424, 200, true);
			this.Name = "ManualSubmissionForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.GroupBoxSubmission, 0);
			this.Controls.SetChildIndex(this.BottomPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.GroupBoxSubmission.ResumeLayout(false);
			this.GroupBoxSubmission.PerformLayout();
			this.ManualSubmissionDate.ResumeLayout(true);
			this.ManualSubmissionDate.PerformLayout();
			this.PortOfClearanceFindGoxFindBox.ResumeLayout(true);
			this.PortOfClearanceFindGoxFindBox.PerformLayout();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZGroupBox GroupBoxSubmission;
		public ZTextBox messageTypeManualSubmission;
		public ZDateEdit ManualSubmissionDate;
		public ZCodeFindBox PortOfClearanceFindGoxFindBox;
		public ZButton ButtonDeleteRELSubmission;
		public ZButton ButtonSaveSubmission;
		private ZPanel BottomPanel;
		public ZButton ButtonClose;
	}
}