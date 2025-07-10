namespace Enterprise.BufferManagement.GUI
{
	partial class WorkflowTransferDiagnosisUserControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();


			this.TransferDetailsTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.TransferTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.TransferAndBufferReleaseSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.TransferFailureSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.TransferFailureGrid = new Enterprise.ZArchitecture.ZGrid();
			this.FilterRulesControl = new Enterprise.BufferManagement.GUI.FilterRulesDiagnosisUserControl();
			this.TransferRulesControl = new Enterprise.BufferManagement.GUI.TransferRulesDiagnosisUserControl();
			this.ReleaseTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.SuccessfulReleaseControl = new Enterprise.BufferManagement.GUI.LastSuccessfulReleaseUserControl();
			this.WorkflowDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.StatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OpenJobButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ReleaseDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ComponentTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JobTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MainSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TransferDetailsTabControl.SuspendLayout();
			this.TransferTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TransferAndBufferReleaseSplitContainer)).BeginInit();
			this.TransferAndBufferReleaseSplitContainer.Panel1.SuspendLayout();
			this.TransferAndBufferReleaseSplitContainer.Panel2.SuspendLayout();
			this.TransferAndBufferReleaseSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TransferFailureSplitContainer)).BeginInit();
			this.TransferFailureSplitContainer.Panel1.SuspendLayout();
			this.TransferFailureSplitContainer.Panel2.SuspendLayout();
			this.TransferFailureSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TransferFailureGrid)).BeginInit();
			this.TransferFailureGrid.SuspendLayout();
			this.FilterRulesControl.SuspendLayout();
			this.TransferRulesControl.SuspendLayout();
			this.ReleaseTabPage.SuspendLayout();
			this.SuccessfulReleaseControl.SuspendLayout();
			this.WorkflowDetailsGroupBox.SuspendLayout();
			this.StatusDropEdit.SuspendLayout();
			this.ReleaseDateEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).BeginInit();
			this.MainSplitContainer.Panel1.SuspendLayout();
			this.MainSplitContainer.Panel2.SuspendLayout();
			this.MainSplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.BufferManagement.Business.WorkflowTransferDiagnosisViewModel);
			// 
			// TransferDetailsTabControl
			// 
			this.TransferDetailsTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.TransferDetailsTabControl.Controls.Add(this.TransferTabPage);
			this.TransferDetailsTabControl.Controls.Add(this.ReleaseTabPage);
			this.TransferDetailsTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TransferDetailsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TransferDetailsTabControl.Name = "TransferDetailsTabControl";
			this.TransferDetailsTabControl.SelectedIndex = 0;
			this.TransferDetailsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 456, true);
			this.TransferDetailsTabControl.TabIndex = 7;
			// 
			// TransferTabPage
			// 
			this.TransferTabPage.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("7afbbfbb-1d89-4029-80f2-ed9b0ecbc795", "Transfers to Other Components");
			this.TransferTabPage.Controls.Add(this.TransferAndBufferReleaseSplitContainer);
			this.TransferTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.TransferTabPage.Name = "TransferTabPage";
			this.TransferTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.TransferTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(792, 429, true);
			this.TransferTabPage.TabIndex = 0;
			this.TransferTabPage.UseVisualStyleBackColor = true;
			//
			// TransferAndBufferReleaseSplitContainer
			//
			this.TransferAndBufferReleaseSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TransferAndBufferReleaseSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.TransferAndBufferReleaseSplitContainer.Name = "TransferAndBufferReleaseSplitContainer";
			this.TransferAndBufferReleaseSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			//
			// TransferAndBufferReleaseSplitContainer.Panel1
			//
			this.TransferAndBufferReleaseSplitContainer.Panel1.Controls.Add(this.TransferFailureSplitContainer);
			//
			// TransferAndBufferReleaseSplitContainer.Panel2
			//
			this.TransferAndBufferReleaseSplitContainer.Panel2.AutoScroll = true;
			this.TransferAndBufferReleaseSplitContainer.Panel2.Controls.Add(this.TransferRulesControl);
			this.TransferAndBufferReleaseSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(786, 423, true);
			this.TransferAndBufferReleaseSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(192);
			this.TransferAndBufferReleaseSplitContainer.TabIndex = 1;
			// 
			// TransferFailureSplitContainer
			// 
			this.TransferFailureSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TransferFailureSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.TransferFailureSplitContainer.Name = "TransferFailureSplitContainer";
			// 
			// TransferFailureSplitContainer.Panel1
			// 
			this.TransferFailureSplitContainer.Panel1.Controls.Add(this.TransferFailureGrid);
			// 
			// TransferFailureSplitContainer.Panel2
			// 
			this.TransferFailureSplitContainer.Panel2.Controls.Add(this.FilterRulesControl);
			this.TransferFailureSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(786, 423, true);
			this.TransferFailureSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(192);
			this.TransferFailureSplitContainer.TabIndex = 3;
			// 
			// TransferFailureGrid
			// 
			this.TransferFailureGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.TransferFailureGrid, "TransferDiagnoses");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.BufferManagement.Business.WorkflowTransferDiagnosisViewModel)(null)).TransferDiagnoses)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.WorkflowTransferDiagnosis)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.WorkflowTransferDiagnosisViewModel)(null)).TransferDiagnoses)).SyncRoot)).LinkName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.WorkflowTransferDiagnosis)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.WorkflowTransferDiagnosisViewModel)(null)).TransferDiagnoses)).SyncRoot)).ComponentToName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.WorkflowTransferDiagnosis)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.WorkflowTransferDiagnosisViewModel)(null)).TransferDiagnoses)).SyncRoot)).FilterRuleMatchingStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.WorkflowTransferDiagnosis)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.WorkflowTransferDiagnosisViewModel)(null)).TransferDiagnoses)).SyncRoot)).DestinationType)));

			this.TransferFailureGrid.CaptionVisible = false;

			zTextBoxColumnStyleInfo1.ColumnName = "LinkName";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "ComponentToName";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo3.ColumnName = "FilterRuleMatchingStatus";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo4.ColumnName = "DestinationType";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);

			this.TransferFailureGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.TransferFailureGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.TransferFailureGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.TransferFailureGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);


			this.TransferFailureGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TransferFailureGrid.GridId = "4336388a-f421-47e1-afd5-84664848a628";
			this.TransferFailureGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.TransferFailureGrid.LayoutKey = "TransferFailureGrid";
			this.TransferFailureGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TransferFailureGrid.Name = "TransferFailureGrid";
			this.TransferFailureGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(192, 423, true);
			this.TransferFailureGrid.TabIndex = 0;
			// 
			// FilterRulesControl
			// 
			this.FilterRulesControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FilterRulesControl, ".");
			this.FilterRulesControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FilterRulesControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.FilterRulesControl.Name = "FilterRulesControl";
			this.FilterRulesControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(186, 67, true);
			this.FilterRulesControl.TabIndex = 0;
			// 
			// TransferRulesControl
			// 
			this.TransferRulesControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransferRulesControl, ".");
			this.TransferRulesControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TransferRulesControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.TransferRulesControl.Name = "TransferRulesControl";
			this.TransferRulesControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(576, 390, true);
			this.TransferRulesControl.TabIndex = 0;
			// 
			// ReleaseTabPage
			// 
			this.ReleaseTabPage.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("3e3ba2b4-d36a-4976-9f36-c945754edc8a", "Last Successful Release");
			this.ReleaseTabPage.Controls.Add(this.SuccessfulReleaseControl);
			this.ReleaseTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ReleaseTabPage.Name = "ReleaseTabPage";
			this.ReleaseTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ReleaseTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(792, 429, true);
			this.ReleaseTabPage.TabIndex = 1;
			this.ReleaseTabPage.UseVisualStyleBackColor = true;
			// 
			// SuccessfulReleaseControl
			// 
			this.SuccessfulReleaseControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SuccessfulReleaseControl, ".");
			this.SuccessfulReleaseControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SuccessfulReleaseControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.SuccessfulReleaseControl.Name = "SuccessfulReleaseControl";
			this.SuccessfulReleaseControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(786, 423, true);
			this.SuccessfulReleaseControl.TabIndex = 0;
			// 
			// WorkflowDetailsGroupBox
			//
			this.WorkflowDetailsGroupBox.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("04e22e8a-3981-43ff-805d-b06156ad9d98", "Details");
			this.WorkflowDetailsGroupBox.Controls.Add(this.StatusDropEdit);
			this.WorkflowDetailsGroupBox.Controls.Add(this.OpenJobButton);
			this.WorkflowDetailsGroupBox.Controls.Add(this.ReleaseDateEdit);
			this.WorkflowDetailsGroupBox.Controls.Add(this.ComponentTextBox);
			this.WorkflowDetailsGroupBox.Controls.Add(this.JobTextBox);
			this.WorkflowDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.WorkflowDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.WorkflowDetailsGroupBox.Name = "WorkflowDetailsGroupBox";
			this.WorkflowDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 70, true);
			this.WorkflowDetailsGroupBox.TabIndex = 6;
			this.WorkflowDetailsGroupBox.TabStop = false;
			// 
			// StatusDropEdit
			// 
			this.StatusDropEdit.AllowDrop = true;
			this.StatusDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.StatusDropEdit, "Workflow.FH_Status");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.BufferManagement.Business.WorkflowTransferDiagnosisViewModel)(null)).Workflow.FH_Status)));
			this.StatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(526, 19, true);
			this.StatusDropEdit.Name = "StatusDropEdit";
			this.StatusDropEdit.PreBoundMaxLength = 3;
			this.StatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(147, 20, true);
			this.StatusDropEdit.TabIndex = 6;
			// 
			// OpenJobButton
			// 
			this.OpenJobButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.OpenJobButton.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("bbcf0a2e-3e0d-4359-963b-2e1dc8c7e921", "Open Job");
			this.OpenJobButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(679, 17, true);
			this.OpenJobButton.Name = "OpenJobButton";
			this.OpenJobButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.OpenJobButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 22, true);
			this.OpenJobButton.TabIndex = 3;
			this.OpenJobButton.ToolTipCaption = null;
			this.OpenJobButton.UseVisualStyleBackColor = true;
			this.OpenJobButton.Click += new System.EventHandler(this.OpenJobButton_Click);
			// 
			// ReleaseDateEdit
			// 
			this.ReleaseDateEdit.AllowDrop = true;
			this.ReleaseDateEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ReleaseDateEdit.AutoCompleteMonthThreshold = 1;
			this.ReleaseDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ReleaseDateEdit, "Workflow.LastTransferDateLocal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.BufferManagement.Business.WorkflowTransferDiagnosisViewModel)(null)).Workflow.LastTransferDateLocal)));
			this.ReleaseDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.ReleaseDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(679, 44, true);
			this.ReleaseDateEdit.Name = "ReleaseDateEdit";
			this.ReleaseDateEdit.TabIndex = 5;
			// 
			// ComponentTextBox
			// 
			this.ComponentTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ComponentTextBox, "Workflow.CurrentComponent.FC_Name");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.WorkflowTransferDiagnosisViewModel)(null)).Workflow.CurrentComponent.FC_Name)));
			this.ComponentTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ComponentTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 45, true);
			this.ComponentTextBox.Name = "ComponentTextBox";
			this.ComponentTextBox.ReadOnly = true;
			this.ComponentTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 20, true);
			this.ComponentTextBox.TabIndex = 4;
			// 
			// JobTextBox
			// 
			this.JobTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.JobTextBox, "Workflow.Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.WorkflowTransferDiagnosisViewModel)(null)).Workflow.Description)));
			this.JobTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.JobTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 19, true);
			this.JobTextBox.Name = "JobTextBox";
			this.JobTextBox.ReadOnly = true;
			this.JobTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 20, true);
			this.JobTextBox.TabIndex = 1;
			// 
			// MainSplitContainer
			// 
			this.MainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.MainSplitContainer.IsSplitterFixed = true;
			this.MainSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainSplitContainer.Name = "MainSplitContainer";
			this.MainSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// MainSplitContainer.Panel1
			// 
			this.MainSplitContainer.Panel1.Controls.Add(this.WorkflowDetailsGroupBox);
			// 
			// MainSplitContainer.Panel2
			// 
			this.MainSplitContainer.Panel2.Controls.Add(this.TransferDetailsTabControl);
			this.MainSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 530, true);
			this.MainSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(70);
			this.MainSplitContainer.TabIndex = 8;
			// 
			// WorkflowTransferDiagnosisUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MainSplitContainer);
			this.Name = "WorkflowTransferDiagnosisUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 530, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TransferDetailsTabControl.ResumeLayout(false);
			this.TransferDetailsTabControl.PerformLayout();
			this.TransferTabPage.ResumeLayout(false);
			this.TransferTabPage.PerformLayout();
			this.TransferAndBufferReleaseSplitContainer.Panel1.ResumeLayout(false);
			this.TransferAndBufferReleaseSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.TransferAndBufferReleaseSplitContainer)).EndInit();
			this.TransferAndBufferReleaseSplitContainer.ResumeLayout(false);
			this.TransferAndBufferReleaseSplitContainer.PerformLayout();
			this.TransferFailureSplitContainer.Panel1.ResumeLayout(false);
			this.TransferFailureSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.TransferFailureSplitContainer)).EndInit();
			this.TransferFailureSplitContainer.ResumeLayout(false);
			this.TransferFailureSplitContainer.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.TransferFailureGrid)).EndInit();
			this.TransferFailureGrid.ResumeLayout(false);
			this.TransferFailureGrid.PerformLayout();
			this.FilterRulesControl.ResumeLayout(true);
			this.FilterRulesControl.PerformLayout();
			this.TransferRulesControl.ResumeLayout(true);
			this.TransferRulesControl.PerformLayout();
			this.ReleaseTabPage.ResumeLayout(false);
			this.ReleaseTabPage.PerformLayout();
			this.SuccessfulReleaseControl.ResumeLayout(true);
			this.SuccessfulReleaseControl.PerformLayout();
			this.WorkflowDetailsGroupBox.ResumeLayout(false);
			this.WorkflowDetailsGroupBox.PerformLayout();
			this.StatusDropEdit.ResumeLayout(true);
			this.StatusDropEdit.PerformLayout();
			this.ReleaseDateEdit.ResumeLayout(true);
			this.ReleaseDateEdit.PerformLayout();
			this.MainSplitContainer.Panel1.ResumeLayout(false);
			this.MainSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).EndInit();
			this.MainSplitContainer.ResumeLayout(false);
			this.MainSplitContainer.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZTabControl TransferDetailsTabControl;
		private ZArchitecture.GUI.ZTabPage TransferTabPage;
		private CargoWise.Windows.UI.KSplitContainer TransferAndBufferReleaseSplitContainer;
		private CargoWise.Windows.UI.KSplitContainer TransferFailureSplitContainer;
		private ZArchitecture.ZGrid TransferFailureGrid;
		private ZArchitecture.GUI.ZTabPage ReleaseTabPage;
		private ZArchitecture.GUI.ZGroupBox WorkflowDetailsGroupBox;
		private ZArchitecture.GUI.ZButton OpenJobButton;
		private ZArchitecture.GUI.ZDateEdit ReleaseDateEdit;
		private ZArchitecture.ZTextBox ComponentTextBox;
		private ZArchitecture.ZTextBox JobTextBox;
		private CargoWise.Windows.UI.KSplitContainer MainSplitContainer;
		private FilterRulesDiagnosisUserControl FilterRulesControl;
		private TransferRulesDiagnosisUserControl TransferRulesControl;
		private ZArchitecture.GUI.ZDropEdit StatusDropEdit;
		private LastSuccessfulReleaseUserControl SuccessfulReleaseControl;
	}

}
