namespace Enterprise.BufferManagement.GUI
{
	partial class DeferWorkflowForm
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.DoNotStartBeforeHintLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DoNotStartBeforeDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.DeferralReasonDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CancelDeferButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.DeferButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.JobNumberLabel = new Enterprise.ZArchitecture.ZLabel();
			this.WorkflowLabel = new Enterprise.ZArchitecture.ZLabel();
			this.JobDescriptionLabel = new Enterprise.ZArchitecture.ZLabel();
			this.WorkflowDetailsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.WorkflowHintLabel = new Enterprise.ZArchitecture.ZLabel();
			this.JobHintLabel = new Enterprise.ZArchitecture.ZLabel();
			this.WorkflowsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.DependencyRemovalHintLabel = new Enterprise.ZArchitecture.ZLabel();
			this.GridHintLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DeferControlsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.WorkflowsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DoNotStartBeforeDateEdit.SuspendLayout();
			this.WorkflowDetailsPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.WorkflowsGrid)).BeginInit();
			this.WorkflowsGrid.SuspendLayout();
			this.DeferControlsPanel.SuspendLayout();
			this.WorkflowsPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 364, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(676, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.BufferManagement.Business.DeferWorkflowViewModel);
			// 
			// DoNotStartBeforeHintLabel
			// 
			this.DoNotStartBeforeHintLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.DoNotStartBeforeHintLabel, "DoNotStartBeforeHint");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.DeferWorkflowViewModel)(null)).DoNotStartBeforeHint)));
			this.DoNotStartBeforeHintLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 6, true);
			this.DoNotStartBeforeHintLabel.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(430, 0, true);
			this.DoNotStartBeforeHintLabel.Name = "DoNotStartBeforeHintLabel";
			this.DoNotStartBeforeHintLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.DoNotStartBeforeHintLabel.TabIndex = 4;
			// 
			// DoNotStartBeforeDateEdit
			// 
			this.DoNotStartBeforeDateEdit.AllowDrop = true;
			this.DoNotStartBeforeDateEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.DoNotStartBeforeDateEdit.AutoCompleteMonthThreshold = 1;
			this.DoNotStartBeforeDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DoNotStartBeforeDateEdit, "DoNotStartBeforeDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.BufferManagement.Business.DeferWorkflowViewModel)(null)).DoNotStartBeforeDate)));
			this.DoNotStartBeforeDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.DoNotStartBeforeDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(348, 5, true);
			this.DoNotStartBeforeDateEdit.Name = "DoNotStartBeforeDateEdit";
			this.DoNotStartBeforeDateEdit.TabIndex = 6;
			// 
			// DeferralReasonDropEdit
			// 
			this.DeferralReasonDropEdit.AllowDrop = true;
			this.DeferralReasonDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.DeferralReasonDropEdit, "DeferralReason");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.BufferManagement.Business.DeferWorkflowViewModel)(null)).DeferralReason)));
			this.DeferralReasonDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(50, 5, true);
			this.DeferralReasonDropEdit.Name = "DeferralReasonDropEdit";
			this.DeferralReasonDropEdit.TabIndex = 5;
			// 
			// CancelDeferButton
			// 
			this.CancelDeferButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelDeferButton.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("e3a54c1a-3e7c-4db0-8326-f05c9320a2b0", "Cancel");
			this.CancelDeferButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelDeferButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(580, 3, true);
			this.CancelDeferButton.Name = "CancelDeferButton";
			this.CancelDeferButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(92, 23, true);
			this.CancelDeferButton.TabIndex = 8;
			this.CancelDeferButton.UseVisualStyleBackColor = false;
			this.CancelDeferButton.Click += new System.EventHandler(this.CancelDeferButton_Click);
			// 
			// DeferButton
			// 
			this.DeferButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.DeferButton.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("0e43f28e-00ce-4cd1-a772-b89f16f29e86", "Defer");
			this.DeferButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.DeferButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(482, 3, true);
			this.DeferButton.Name = "DeferButton";
			this.DeferButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(92, 23, true);
			this.DeferButton.TabIndex = 7;
			this.DeferButton.UseVisualStyleBackColor = false;
			this.DeferButton.Click += new System.EventHandler(this.DeferButton_Click);
			// 
			// JobNumberLabel
			// 
			this.JobNumberLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.JobNumberLabel, "Workflow.ProviderJobNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.DeferWorkflowViewModel)(null)).Workflow.ProviderJobNumber)));
			this.JobNumberLabel.IsFontBold = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.JobNumberLabel, false);
			this.JobNumberLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(79, 5, true);
			this.JobNumberLabel.Name = "JobNumberLabel";
			this.JobNumberLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(74, 13, true);
			this.JobNumberLabel.TabIndex = 9;
			// 
			// WorkflowLabel
			// 
			this.WorkflowLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.WorkflowLabel, "Workflow.FH_CompletionStatement");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.DeferWorkflowViewModel)(null)).Workflow.FH_CompletionStatement)));
			this.WorkflowLabel.IsFontBold = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.WorkflowLabel, false);
			this.WorkflowLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(79, 25, true);
			this.WorkflowLabel.Name = "WorkflowLabel";
			this.WorkflowLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 13, true);
			this.WorkflowLabel.TabIndex = 10;
			// 
			// JobDescriptionLabel
			// 
			this.JobDescriptionLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.JobDescriptionLabel, "Workflow.ProviderJobDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.DeferWorkflowViewModel)(null)).Workflow.ProviderJobDescription)));
			this.JobDescriptionLabel.IsFontBold = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.JobDescriptionLabel, false);
			this.JobDescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(177, 5, true);
			this.JobDescriptionLabel.Name = "JobDescriptionLabel";
			this.JobDescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 13, true);
			this.JobDescriptionLabel.TabIndex = 11;
			// 
			// WorkflowDetailsPanel
			// 
			this.WorkflowDetailsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.WorkflowDetailsPanel.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this.WorkflowDetailsPanel.Controls.Add(this.WorkflowHintLabel);
			this.WorkflowDetailsPanel.Controls.Add(this.JobHintLabel);
			this.WorkflowDetailsPanel.Controls.Add(this.JobNumberLabel);
			this.WorkflowDetailsPanel.Controls.Add(this.JobDescriptionLabel);
			this.WorkflowDetailsPanel.Controls.Add(this.WorkflowLabel);
			this.WorkflowDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 25, true);
			this.WorkflowDetailsPanel.Name = "WorkflowDetailsPanel";
			this.WorkflowDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(676, 43, true);
			this.WorkflowDetailsPanel.TabIndex = 12;
			// 
			// WorkflowHintLabel
			// 
			this.WorkflowHintLabel.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("4448abad-12f6-4e21-8fd1-62fb029714ef", "Workflow:");
			this.WorkflowHintLabel.IsFontBold = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.WorkflowHintLabel, false);
			this.WorkflowHintLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 25, true);
			this.WorkflowHintLabel.Name = "WorkflowHintLabel";
			this.WorkflowHintLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(68, 13, true);
			this.WorkflowHintLabel.TabIndex = 13;
			// 
			// JobHintLabel
			// 
			this.JobHintLabel.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("8129df0f-1d15-4a56-8477-db4b85d7768e", "Job:");
			this.JobHintLabel.IsFontBold = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.JobHintLabel, false);
			this.JobHintLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 5, true);
			this.JobHintLabel.Name = "JobHintLabel";
			this.JobHintLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(59, 13, true);
			this.JobHintLabel.TabIndex = 11;
			// 
			// WorkflowsGrid
			// 
			this.WorkflowsGrid.AllowNavigation = false;
			this.WorkflowsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.WorkflowsGrid, "WorkflowsToDefer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.BufferManagement.Business.DeferWorkflowViewModel)(null)).WorkflowsToDefer)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.WorkflowToDeferBusinessObject)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.DeferWorkflowViewModel)(null)).WorkflowsToDefer)).SyncRoot)).WorkflowName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.WorkflowToDeferBusinessObject)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.DeferWorkflowViewModel)(null)).WorkflowsToDefer)).SyncRoot)).ActionToBeTaken)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.WorkflowToDeferBusinessObject)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.DeferWorkflowViewModel)(null)).WorkflowsToDefer)).SyncRoot)).ActionDescription)));
			this.WorkflowsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "WorkflowName";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(240);
			zDropEditColumnStyleInfo1.ColumnName = "ActionToBeTaken";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo2.ColumnName = "ActionDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.WorkflowsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.WorkflowsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.WorkflowsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.WorkflowsGrid.GridId = "4f3aaac8-c2cf-4159-b3c2-e03b43d272b6";
			this.WorkflowsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.WorkflowsGrid.LayoutKey = "PrerequisitesGrid";
			this.WorkflowsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 25, true);
			this.WorkflowsGrid.Name = "WorkflowsGrid";
			this.WorkflowsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(662, 135, true);
			this.WorkflowsGrid.TabIndex = 2;
			// 
			// DependencyRemovalHintLabel
			// 
			this.DependencyRemovalHintLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.DependencyRemovalHintLabel.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("dd2e3720-8b5c-47b2-baeb-8ebae134f789", "If the dependencies remain, these workflows may be transferred to a preceding component by rules requiring prerequisites to be cleared. If you do not want these workflows to be transferred, select the action \'RMV\' and press ‘Defer’ (the prerequisite relationships will be deleted).\r\n\r\n\t• DFR: The workflow (or all released workflows in a job) will be deferred.\r\n\t• RMV: The pre-requisite relationship will be removed, ensuring the workflow is not moved later by transfer rules.\r\n\t• NON: No action will be taken now, but transfer rules may move the workflow to a preceding component.");
			this.DependencyRemovalHintLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 163, true);
			this.DependencyRemovalHintLabel.Name = "DependencyRemovalHintLabel";
			this.DependencyRemovalHintLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(652, 93, true);
			this.DependencyRemovalHintLabel.TabIndex = 1;
			// 
			// GridHintLabel
			// 
			this.GridHintLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.GridHintLabel, "WorkflowsHint");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.DeferWorkflowViewModel)(null)).WorkflowsHint)));
			this.GridHintLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 7, true);
			this.GridHintLabel.Name = "GridHintLabel";
			this.GridHintLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(664, 13, true);
			this.GridHintLabel.TabIndex = 0;
			// 
			// DeferControlsPanel
			// 
			this.DeferControlsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.DeferControlsPanel.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this.DeferControlsPanel.Controls.Add(this.DeferButton);
			this.DeferControlsPanel.Controls.Add(this.DoNotStartBeforeDateEdit);
			this.DeferControlsPanel.Controls.Add(this.DeferralReasonDropEdit);
			this.DeferControlsPanel.Controls.Add(this.CancelDeferButton);
			this.DeferControlsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 334, true);
			this.DeferControlsPanel.Name = "DeferControlsPanel";
			this.DeferControlsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(676, 30, true);
			this.DeferControlsPanel.TabIndex = 14;
			// 
			// WorkflowsPanel
			// 
			this.WorkflowsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.WorkflowsPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.WorkflowsPanel.Controls.Add(this.WorkflowsGrid);
			this.WorkflowsPanel.Controls.Add(this.GridHintLabel);
			this.WorkflowsPanel.Controls.Add(this.DependencyRemovalHintLabel);
			this.WorkflowsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 68, true);
			this.WorkflowsPanel.Name = "WorkflowsPanel";
			this.WorkflowsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(676, 265, true);
			this.WorkflowsPanel.TabIndex = 15;
			// 
			// DeferWorkflowForm
			// 
			this.AcceptButton = this.DeferButton;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.CancelDeferButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(676, 388, true);
			this.Controls.Add(this.WorkflowsPanel);
			this.Controls.Add(this.DeferControlsPanel);
			this.Controls.Add(this.WorkflowDetailsPanel);
			this.Controls.Add(this.DoNotStartBeforeHintLabel);
			this.DataSourceType = typeof(Enterprise.BufferManagement.Business.DeferWorkflowViewModel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "DeferWorkflowForm";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.DoNotStartBeforeHintLabel, 0);
			this.Controls.SetChildIndex(this.WorkflowDetailsPanel, 0);
			this.Controls.SetChildIndex(this.DeferControlsPanel, 0);
			this.Controls.SetChildIndex(this.WorkflowsPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DoNotStartBeforeDateEdit.ResumeLayout(true);
			this.DoNotStartBeforeDateEdit.PerformLayout();
			this.DeferralReasonDropEdit.ResumeLayout(true);
			this.DeferralReasonDropEdit.PerformLayout();
			this.WorkflowDetailsPanel.ResumeLayout(false);
			this.WorkflowDetailsPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.WorkflowsGrid)).EndInit();
			this.WorkflowsGrid.ResumeLayout(false);
			this.WorkflowsGrid.PerformLayout();
			this.DeferControlsPanel.ResumeLayout(false);
			this.DeferControlsPanel.PerformLayout();
			this.WorkflowsPanel.ResumeLayout(false);
			this.WorkflowsPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZLabel DoNotStartBeforeHintLabel;
		private ZArchitecture.GUI.ZDateEdit DoNotStartBeforeDateEdit;
		private ZArchitecture.GUI.ZDropEdit DeferralReasonDropEdit;
		public ZArchitecture.GUI.ZButton CancelDeferButton;
		public ZArchitecture.GUI.ZButton DeferButton;
		private ZArchitecture.ZLabel JobNumberLabel;
		private ZArchitecture.ZLabel WorkflowLabel;
		private ZArchitecture.ZLabel JobDescriptionLabel;
		private ZArchitecture.GUI.ZPanel WorkflowDetailsPanel;
		private ZArchitecture.ZLabel GridHintLabel;
		private ZArchitecture.ZLabel DependencyRemovalHintLabel;
		private ZArchitecture.ZGrid WorkflowsGrid;
		private ZArchitecture.GUI.ZPanel DeferControlsPanel;
		private ZArchitecture.GUI.ZPanel WorkflowsPanel;
		private ZArchitecture.ZLabel JobHintLabel;
		private ZArchitecture.ZLabel WorkflowHintLabel;
	}
}