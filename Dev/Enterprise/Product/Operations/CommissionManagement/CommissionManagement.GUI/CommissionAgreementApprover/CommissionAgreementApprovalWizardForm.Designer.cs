using System.Windows.Forms;

namespace Enterprise.CommissionManagement.GUI
{
	partial class CommissionAgreementApprovalWizardForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.TopToolStrip = new Enterprise.ZArchitecture.GUI.ZToolStrip();
			this.ViewQueueButton = new Enterprise.ZArchitecture.GUI.ZToolStripButton();
			this.FromDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.AgreementsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AgreementsSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.AgreementListSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.AppendButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SelectAllButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.DeselectAllButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.AgreementPreviewSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.BackdateCommissionOptionsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.backdateCommissionOptionsHintLabel = new Enterprise.ZArchitecture.ZLabel();
			this.FromTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ShouldOverwriteOldCommissionCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.DisapproveButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.checkBoxAddToQueue = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ApproveButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.FromDateDateEdit.SuspendLayout();
			this.AgreementsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AgreementsSplitContainer)).BeginInit();
			this.AgreementsSplitContainer.Panel1.SuspendLayout();
			this.AgreementsSplitContainer.Panel2.SuspendLayout();
			this.AgreementsSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AgreementListSplitContainer)).BeginInit();
			this.AgreementListSplitContainer.Panel2.SuspendLayout();
			this.AgreementListSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AgreementPreviewSplitContainer)).BeginInit();
			this.AgreementPreviewSplitContainer.SuspendLayout();
			this.BackdateCommissionOptionsGroupBox.SuspendLayout();
			this.FromTypeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 657, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1184, 24, true);
			this.MainStatusBar.TabIndex = 3;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.CommissionManagement.Business.CommissionAgreementApprovalWizard);
			// 
			// TopToolStrip
			// 
			this.TopToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.TopToolStrip.ImageScalingSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(16, 16, true);
			this.TopToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
				this.ViewQueueButton});
			this.TopToolStrip.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TopToolStrip.Name = "TopToolStrip";
			this.TopToolStrip.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.TopToolStrip.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1184, 28, true);
			this.TopToolStrip.TabIndex = 3;
			this.TopToolStrip.Text = "zToolStrip1";
			// 
			// ViewQueueButton
			// 
			this.ViewQueueButton.CaptionResourceString = Enterprise.CommissionManagement.GUI.Res.GetData("2530F808-D604-4378-8845-3525937BFA10", "Calculation Queue");
			this.ViewQueueButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.ViewQueueButton.Name = "ViewQueueButton";
			this.ViewQueueButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(109, 19, true);
			this.ViewQueueButton.Click += new System.EventHandler(this.ViewQueueButton_Click);
			// 
			// FromDateDateEdit
			// 
			this.FromDateDateEdit.AllowDrop = true;
			this.FromDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.FromDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.FromDateDateEdit, "FromDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.CommissionManagement.Business.CommissionAgreementApprovalWizard)(null)).FromDate)));
			this.FromDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(235, 43, true);
			this.FromDateDateEdit.Name = "FromDateDateEdit";
			this.FromDateDateEdit.TabIndex = 0;
			// 
			// AgreementsGroupBox
			// 
			this.AgreementsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.AgreementsGroupBox.CaptionResourceString = Enterprise.CommissionManagement.GUI.Res.GetData("32080fc9-4da2-4973-9c78-d31995ce7ca5", "Agreements");
			this.AgreementsGroupBox.Controls.Add(this.AgreementsSplitContainer);
			this.AgreementsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 109, true);
			this.AgreementsGroupBox.Name = "AgreementsGroupBox";
			this.AgreementsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1340, 513, true);
			this.AgreementsGroupBox.TabIndex = 1;
			this.AgreementsGroupBox.TabStop = false;
			// 
			// AgreementsSplitContainer
			// 
			this.AgreementsSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AgreementsSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.AgreementsSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(760);
			this.AgreementsSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.AgreementsSplitContainer.Name = "AgreementsSplitContainer";
			// 
			// AgreementsSplitContainer.Panel1
			// 
			this.AgreementsSplitContainer.Panel1.Controls.Add(this.AgreementListSplitContainer);
			// 
			// AgreementsSplitContainer.Panel2
			// 
			this.AgreementsSplitContainer.Panel2.Controls.Add(this.AgreementPreviewSplitContainer);
			this.AgreementsSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1154, 494, true);
			this.AgreementsSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(449);
			this.AgreementsSplitContainer.TabIndex = 4;
			// 
			// AppendButton
			// 
			this.AppendButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.AppendButton.CaptionResourceString = Enterprise.CommissionManagement.GUI.Res.GetData("2681b1b1-d8e2-4367-9029-85ce27021a36", "Append");
			this.AppendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(30, 0, true);
			this.AppendButton.Name = "AppendButton";
			this.AppendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.AppendButton.TabIndex = 3;
			this.AppendButton.Click += new System.EventHandler(this.AppendButton_Click);
			this.AppendButton.ShouldSetReadOnlyWhenSettingIncludingChildren = true;
			// 
			// SelectAllButton
			// 
			this.SelectAllButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.SelectAllButton.CaptionResourceString = Enterprise.CommissionManagement.GUI.Res.GetData("85520251-ad01-4009-ba69-cdfe55a08daa", "Select All");
			this.SelectAllButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 0, true);
			this.SelectAllButton.Name = "SelectAllButton";
			this.SelectAllButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.SelectAllButton.TabIndex = 1;
			this.SelectAllButton.Click += new System.EventHandler(this.SelectAllButton_Click);
			this.SelectAllButton.ShouldSetReadOnlyWhenSettingIncludingChildren = true;
			// 
			// DeselectAllButton
			// 
			this.DeselectAllButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.DeselectAllButton.CaptionResourceString = Enterprise.CommissionManagement.GUI.Res.GetData("35cbf319-faf3-4d43-8e09-900bde0f8b36", "Deselect All");
			this.DeselectAllButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 0, true);
			this.DeselectAllButton.Name = "DeselectAllButton";
			this.DeselectAllButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.DeselectAllButton.TabIndex = 2;
			this.DeselectAllButton.Click += new System.EventHandler(this.DeselectAllButton_Click);
			this.DeselectAllButton.ShouldSetReadOnlyWhenSettingIncludingChildren = true;
			// 
			// AgreementPreviewSplitContainer
			// 
			this.AgreementPreviewSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AgreementPreviewSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AgreementPreviewSplitContainer.Name = "AgreementPreviewSplitContainer";
			this.AgreementPreviewSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			this.AgreementPreviewSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(167);
			this.AgreementPreviewSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(701, 494, true);
			this.AgreementPreviewSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(299);
			this.AgreementPreviewSplitContainer.TabIndex = 0;
			// 
			// AgreementListSplitContainer
			//
			this.AgreementListSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AgreementListSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AgreementListSplitContainer.Name = "AgreementPreviewSplitContainer";
			this.AgreementListSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			this.AgreementListSplitContainer.IsSplitterFixed = true;
			this.AgreementListSplitContainer.FixedPanel = FixedPanel.Panel2;
			this.AgreementListSplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			// 
			// AgreementListSplitContainer.Panel2
			// 
			this.AgreementListSplitContainer.Panel2.Controls.Add(this.AppendButton);
			this.AgreementListSplitContainer.Panel2.Controls.Add(this.SelectAllButton);
			this.AgreementListSplitContainer.Panel2.Controls.Add(this.DeselectAllButton);
			// 
			// BackdateCommissionOptionsGroupBox
			// 
			this.BackdateCommissionOptionsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BackdateCommissionOptionsGroupBox.CaptionResourceString = Enterprise.CommissionManagement.GUI.Res.GetData("629a122f-77c7-4d3b-b6ab-90b0eecc2567", "Backdate Commission Options");
			this.BackdateCommissionOptionsGroupBox.Controls.Add(this.backdateCommissionOptionsHintLabel);
			this.BackdateCommissionOptionsGroupBox.Controls.Add(this.FromTypeDropEdit);
			this.BackdateCommissionOptionsGroupBox.Controls.Add(this.ShouldOverwriteOldCommissionCheckBox);
			this.BackdateCommissionOptionsGroupBox.Controls.Add(this.FromDateDateEdit);
			this.BackdateCommissionOptionsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 32, true);
			this.BackdateCommissionOptionsGroupBox.Name = "BackdateCommissionOptionsGroupBox";
			this.BackdateCommissionOptionsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1340, 71, true);
			this.BackdateCommissionOptionsGroupBox.TabIndex = 0;
			this.BackdateCommissionOptionsGroupBox.TabStop = false;
			// 
			// backdateCommissionOptionsHintLabel
			// 
			this.backdateCommissionOptionsHintLabel.CaptionResourceString = Enterprise.CommissionManagement.GUI.Res.GetData("9874eda9-6347-4831-ab57-59e6cd84ad30", "These options control what commissionable amounts are created in arrears for the agreements being approved.");
			this.backdateCommissionOptionsHintLabel.Dock = System.Windows.Forms.DockStyle.Top;
			this.backdateCommissionOptionsHintLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.backdateCommissionOptionsHintLabel.Name = "backdateCommissionOptionsHintLabel";
			this.backdateCommissionOptionsHintLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1154, 23, true);
			this.backdateCommissionOptionsHintLabel.TabIndex = 2;
			// 
			// FromTypeDropEdit
			// 
			this.FromTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FromTypeDropEdit, "FromType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.CommissionManagement.Business.CommissionAgreementApprovalWizard)(null)).FromType)));
			this.FromTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(70, 43, true);
			this.FromTypeDropEdit.Name = "FromTypeDropEdit";
			this.FromTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(155, 20, true);
			this.FromTypeDropEdit.TabIndex = 0;
			// 
			// ShouldOverwriteOldCommissionCheckBox
			// 
			this.ShouldOverwriteOldCommissionCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ShouldOverwriteOldCommissionCheckBox, "ShouldOverwriteOldCommission");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.CommissionManagement.Business.CommissionAgreementApprovalWizard)(null)).ShouldOverwriteOldCommission)));
			this.ShouldOverwriteOldCommissionCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ShouldOverwriteOldCommissionCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(396, 45, true);
			this.ShouldOverwriteOldCommissionCheckBox.Name = "ShouldOverwriteOldCommissionCheckBox";
			this.ShouldOverwriteOldCommissionCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(145, 17, true);
			this.ShouldOverwriteOldCommissionCheckBox.TabIndex = 1;
			// 
			// DisapproveButton
			// 
			this.DisapproveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.DisapproveButton.CaptionResourceString = Enterprise.CommissionManagement.GUI.Res.GetData("825e3e6b-b3da-41a9-a767-47d264898b49", "Disapprove");
			this.DisapproveButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1194, 628, true);
			this.DisapproveButton.Name = "DisapproveButton";
			this.DisapproveButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.DisapproveButton.TabIndex = 4;
			this.DisapproveButton.Click += new System.EventHandler(this.DisapproveButton_Click);
			this.DisapproveButton.ShouldSetReadOnlyWhenSettingIncludingChildren = true;
			// 
			// checkBoxAddToQueue
			// 
			this.checkBoxAddToQueue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.checkBoxAddToQueue.AutoSize = true;
			this.BindingSource.SetBindingMember(this.checkBoxAddToQueue, "ShouldAddToCalculationQueue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.CommissionManagement.Business.CommissionAgreementApprovalWizard)(null)).ShouldAddToCalculationQueue)));
			this.checkBoxAddToQueue.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.checkBoxAddToQueue.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(940, 632, true);
			this.checkBoxAddToQueue.Name = "checkBoxAddToQueue";
			this.checkBoxAddToQueue.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(147, 17, true);
			this.checkBoxAddToQueue.TabIndex = 3;
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.CommissionManagement.GUI.Res.GetData("80A25FD7-3DE2-4AEC-B563-B7DCD56C15A9", "Close");
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1274, 628, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CloseButton.TabIndex = 6;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// ApproveButton
			// 
			this.ApproveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ApproveButton.CaptionResourceString = Enterprise.CommissionManagement.GUI.Res.GetData("1efbce2f-47e2-468f-ae1e-e4249f8083ff", "Approve");
			this.ApproveButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1112, 628, true);
			this.ApproveButton.Name = "ApproveButton";
			this.ApproveButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.ApproveButton.TabIndex = 2;
			this.ApproveButton.Click += new System.EventHandler(this.ApproveButton_Click);
			this.ApproveButton.ShouldSetReadOnlyWhenSettingIncludingChildren = true;
			// 
			// CommissionAgreementApprovalWizardForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.CommissionManagement.GUI.Res.GetData("9320df67-1bc5-44c6-a14d-46ea938ac8e0", "Commission Agreement Approval");
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.TopToolStrip);
			this.Controls.Add(this.checkBoxAddToQueue);
			this.Controls.Add(this.DisapproveButton);
			this.Controls.Add(this.BackdateCommissionOptionsGroupBox);
			this.Controls.Add(this.AgreementsGroupBox);
			this.Controls.Add(this.ApproveButton);
			this.DataSourceType = typeof(Enterprise.CommissionManagement.Business.CommissionAgreementApprovalWizard);
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1364, 681, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1364, 681, true);
			this.Name = "CommissionAgreementApprovalWizardForm";
			this.Controls.SetChildIndex(this.ApproveButton, 0);
			this.Controls.SetChildIndex(this.AgreementsGroupBox, 0);
			this.Controls.SetChildIndex(this.BackdateCommissionOptionsGroupBox, 0);
			this.Controls.SetChildIndex(this.DisapproveButton, 0);
			this.Controls.SetChildIndex(this.checkBoxAddToQueue, 0);
			this.Controls.SetChildIndex(this.TopToolStrip, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.FromDateDateEdit.ResumeLayout(true);
			this.FromDateDateEdit.PerformLayout();
			this.AgreementsGroupBox.ResumeLayout(false);
			this.AgreementsGroupBox.PerformLayout();
			this.AgreementsSplitContainer.Panel1.ResumeLayout(false);
			this.AgreementsSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.AgreementsSplitContainer)).EndInit();
			this.AgreementsSplitContainer.ResumeLayout(false);
			this.AgreementsSplitContainer.PerformLayout();
			this.AgreementListSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.AgreementListSplitContainer)).EndInit();
			this.AgreementListSplitContainer.ResumeLayout(false);
			this.AgreementListSplitContainer.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.AgreementPreviewSplitContainer)).EndInit();
			this.AgreementPreviewSplitContainer.ResumeLayout(false);
			this.AgreementPreviewSplitContainer.PerformLayout();
			this.BackdateCommissionOptionsGroupBox.ResumeLayout(false);
			this.BackdateCommissionOptionsGroupBox.PerformLayout();
			this.FromTypeDropEdit.ResumeLayout(true);
			this.FromTypeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected ZArchitecture.GUI.ZDateEdit FromDateDateEdit;
		private ZArchitecture.GUI.ZGroupBox AgreementsGroupBox;
		private ZArchitecture.GUI.ZGroupBox BackdateCommissionOptionsGroupBox;
		private ZArchitecture.GUI.ZCheckBox ShouldOverwriteOldCommissionCheckBox;
		protected ZArchitecture.GUI.ZButton DeselectAllButton;
		protected ZArchitecture.GUI.ZButton SelectAllButton;
		private CargoWise.Windows.UI.KSplitContainer AgreementsSplitContainer;
		private CargoWise.Windows.UI.KSplitContainer AgreementPreviewSplitContainer;
		private CargoWise.Windows.UI.KSplitContainer AgreementListSplitContainer;
		private ZArchitecture.GUI.ZDropEdit FromTypeDropEdit;
		private ZArchitecture.ZLabel backdateCommissionOptionsHintLabel;
		protected ZArchitecture.GUI.ZButton AppendButton;
		protected ZArchitecture.GUI.ZButton DisapproveButton;
		protected ZArchitecture.GUI.ZCheckBox checkBoxAddToQueue;
		protected ZArchitecture.GUI.ZButton CloseButton;
		protected ZArchitecture.GUI.ZButton ApproveButton;
		protected ZArchitecture.GUI.ZToolStrip TopToolStrip;
		protected ZArchitecture.GUI.ZToolStripButton ViewQueueButton;
	}
}
