using System;
using Enterprise.ZArchitecture;

namespace Enterprise.BufferManagement.GUI
{
	partial class WorkflowDetailsUserControl
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
			this.DetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.StatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TransferDiagnosisButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SystemNameFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.ReleaseGroupFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.ComponentDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
			this.ActualHoursTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EstimateSummaryTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ActiveCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.DescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DependencyStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.WorkflowRelationshipsButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.AutoAssignTasksCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CriticalHandoverCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ReleaseDelayFactorCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ReleaseDelayTimeEditEx = new Enterprise.ZArchitecture.GUI.ZTimeEditEx();
			this.AgreedDeliveryDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.EarliestStartDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.WorkflowSchedulingGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.IsStandbyCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.DateAcceptabilityControl = new Enterprise.BufferManagement.GUI.DateAcceptabilityControl();
			this.SequencingGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SequencePositionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SequenceNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OpenSequenceButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SequenceWorkflowTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SequenceParentJobLabel = new Enterprise.ZArchitecture.ZLabel();
			this.SequenceParentJobLinkLabel = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			this.RelationshipsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OpenPrereqsCalcEdit = new Enterprise.BufferManagement.GUI.WorkflowDependencyStatusCalcEdit();
			this.PrereqListButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.JobStaggeredStartsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ColumnTableLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.NewReleaseGateGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.IsApprovedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.DedicatedBufferTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BranchFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.DepartmentFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.EffectiveBranchTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EffectiveDepartmentTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NewEffectiveNudgeEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.EffectiveNudgeEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.EffectiveAgreedDeliveryDate = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.LatestAcceptableReleaseDate = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ReleaseSequenceSortDate = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.BufferTimespanDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
			this.EffectiveBufferDuration = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DetailsGroupBox.SuspendLayout();
			this.SystemNameFindBox.SuspendLayout();
			this.ReleaseGroupFindBox.SuspendLayout();
			this.ComponentDropEdit.SuspendLayout();
			this.AgreedDeliveryDateEdit.SuspendLayout();
			this.EarliestStartDateEdit.SuspendLayout();
			this.WorkflowSchedulingGroupBox.SuspendLayout();
			this.DateAcceptabilityControl.SuspendLayout();
			this.SequencingGroupBox.SuspendLayout();
			this.RelationshipsGroupBox.SuspendLayout();
			this.JobStaggeredStartsGroupBox.SuspendLayout();
			this.ColumnTableLayoutPanel.SuspendLayout();
			this.NewReleaseGateGroupBox.SuspendLayout();
			this.BranchFindBox.SuspendLayout();
			this.DepartmentFindBox.SuspendLayout();
			this.EffectiveAgreedDeliveryDate.SuspendLayout();
			this.LatestAcceptableReleaseDate.SuspendLayout();
			this.ReleaseSequenceSortDate.SuspendLayout();
			this.BufferTimespanDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.BufferManagement.Business.ProcessHeader);
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.DetailsGroupBox.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("509507b0-15f0-49ec-b32a-d8935c36ee56", "Details");
			this.DetailsGroupBox.Controls.Add(this.StatusTextBox);
			this.DetailsGroupBox.Controls.Add(this.TransferDiagnosisButton);
			this.DetailsGroupBox.Controls.Add(this.SystemNameFindBox);
			this.DetailsGroupBox.Controls.Add(this.ReleaseGroupFindBox);
			this.DetailsGroupBox.Controls.Add(this.ComponentDropEdit);
			this.DetailsGroupBox.Controls.Add(this.ActualHoursTextBox);
			this.DetailsGroupBox.Controls.Add(this.EstimateSummaryTextBox);
			this.DetailsGroupBox.Controls.Add(this.ActiveCheckBox);
			this.DetailsGroupBox.Controls.Add(this.DescriptionTextBox);
			this.DetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 0, true);
			this.DetailsGroupBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 0, 3, 3, true);
			this.DetailsGroupBox.Name = "DetailsGroupBox";
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(377, 177, true);
			this.DetailsGroupBox.TabIndex = 0;
			this.DetailsGroupBox.TabStop = false;
			// 
			// StatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.StatusTextBox, "FH_StatusDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.ProcessHeader)(null)).FH_StatusDescription)));
			this.StatusTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.StatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 146, true);
			this.StatusTextBox.Name = "StatusTextBox";
			this.StatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 20, true);
			this.StatusTextBox.TabIndex = 7;
			// 
			// TransferDiagnosisButton
			// 
			this.TransferDiagnosisButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.TransferDiagnosisButton.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("4fabd14c-c44f-4462-b9b2-824b2c295df8", "Transfer Diagnosis");
			this.TransferDiagnosisButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(239, 144, true);
			this.TransferDiagnosisButton.Name = "TransferDiagnosisButton";
			this.TransferDiagnosisButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(132, 22, true);
			this.TransferDiagnosisButton.TabIndex = 9;
			this.TransferDiagnosisButton.ToolTipCaption = null;
			this.TransferDiagnosisButton.UseVisualStyleBackColor = true;
			this.TransferDiagnosisButton.Click += new System.EventHandler(this.TransferFailureButton_Click);
			// 
			// SystemNameFindBox
			// 
			this.SystemNameFindBox.AllowDrop = true;
			this.SystemNameFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.SystemNameFindBox, "CurrentComponentSystemPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.BufferManagement.Business.ProcessHeader)(null)).CurrentComponentSystemPK)));
			this.SystemNameFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 68, true);
			this.SystemNameFindBox.Name = "SystemNameFindBox";
			this.SystemNameFindBox.ParentType = null;
			this.SystemNameFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(262, 20, true);
			this.SystemNameFindBox.TabIndex = 3;
			// 
			// ReleaseGroupFindBox
			// 
			this.ReleaseGroupFindBox.AllowDrop = true;
			this.ReleaseGroupFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ReleaseGroupFindBox, "FH_GG_ReleaseGroup");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.BufferManagement.Business.ProcessHeader)(null)).FH_GG_ReleaseGroup)));
			this.ReleaseGroupFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 120, true);
			this.ReleaseGroupFindBox.Name = "ReleaseGroupFindBox";
			this.ReleaseGroupFindBox.ParentType = null;
			this.ReleaseGroupFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(262, 20, true);
			this.ReleaseGroupFindBox.TabIndex = 6;
			// 
			// ComponentDropEdit
			// 
			this.ComponentDropEdit.AllowDrop = true;
			this.ComponentDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ComponentDropEdit, "FH_FC_CurrentComponent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.BufferManagement.Business.ProcessHeader)(null)).FH_FC_CurrentComponent)));
			this.ComponentDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ComponentDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 94, true);
			this.ComponentDropEdit.Name = "ComponentDropEdit";
			this.ComponentDropEdit.ShowDescriptionBox = false;
			this.ComponentDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.ComponentDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(262, 20, true);
			this.ComponentDropEdit.TabIndex = 4;
			this.ComponentDropEdit.UseFullWidthForCodeBox = true;
			// 
			// ActualHoursTextBox
			// 
			this.ActualHoursTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ActualHoursTextBox, "TotalActualHoursIncludingChildrenLabel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.ProcessHeader)(null)).TotalActualHoursIncludingChildrenLabel)));
			this.ActualHoursTextBox.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("86A31772-E3F8-45CC-90BD-5EBAC7C1B55D", "Actual Hours");
			this.ActualHoursTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(327, 42, true);
			this.ActualHoursTextBox.Name = "ActualHoursTextBox";
			this.ActualHoursTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(44, 20, true);
			this.ActualHoursTextBox.TabIndex = 2;
			this.ActualHoursTextBox.Text = "0:00";
			this.ActualHoursTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// EstimateSummaryTextBox
			// 
			this.EstimateSummaryTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.EstimateSummaryTextBox, "TotalEstimatedHoursSummary");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.ProcessHeader)(null)).TotalEstimatedHoursSummary)));
			this.EstimateSummaryTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.EstimateSummaryTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 42, true);
			this.EstimateSummaryTextBox.Name = "EstimateSummaryTextBox";
			this.EstimateSummaryTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(141, 20, true);
			this.EstimateSummaryTextBox.TabIndex = 1;
			// 
			// ActiveCheckBox
			// 
			this.ActiveCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ActiveCheckBox, "FH_IsActive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.BufferManagement.Business.ProcessHeader)(null)).FH_IsActive)));
			this.ActiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(172, 148, true);
			this.ActiveCheckBox.Name = "ActiveCheckBox";
			this.ActiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 17, true);
			this.ActiveCheckBox.TabIndex = 8;
			this.ActiveCheckBox.UseVisualStyleBackColor = true;
			// 
			// DescriptionTextBox
			// 
			this.DescriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.DescriptionTextBox, "FH_CompletionStatement");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.ProcessHeader)(null)).FH_CompletionStatement)));
			this.DescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 16, true);
			this.DescriptionTextBox.Name = "DescriptionTextBox";
			this.DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(262, 20, true);
			this.DescriptionTextBox.TabIndex = 0;
			// 
			// DependencyStatusTextBox
			// 
			this.DependencyStatusTextBox.AllowDrop = true;
			this.DependencyStatusTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.DependencyStatusTextBox, "PrerequisiteStatusShortDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.ProcessHeader)(null)).PrerequisiteStatusShortDescription)));
			this.DependencyStatusTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DependencyStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 19, true);
			this.DependencyStatusTextBox.Name = "DependencyStatusTextBox";
			this.DependencyStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(262, 20, true);
			this.DependencyStatusTextBox.TabIndex = 10;
			// 
			// WorkflowRelationshipsButton
			// 
			this.WorkflowRelationshipsButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.WorkflowRelationshipsButton.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("9537a18d-087b-40c6-8130-739470d40cfd", "Relationship Navigator");
			this.WorkflowRelationshipsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(239, 45, true);
			this.WorkflowRelationshipsButton.Name = "WorkflowRelationshipsButton";
			this.WorkflowRelationshipsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(132, 22, true);
			this.WorkflowRelationshipsButton.TabIndex = 13;
			this.WorkflowRelationshipsButton.ToolTipCaption = null;
			this.WorkflowRelationshipsButton.UseVisualStyleBackColor = true;
			this.WorkflowRelationshipsButton.Click += new System.EventHandler(this.WorkflowRelationshipsButton_Click);
			// 
			// AutoAssignTasksCheckBox
			// 
			this.AutoAssignTasksCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.AutoAssignTasksCheckBox, "FH_AllowTaskAutoAssignment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.BufferManagement.Business.ProcessHeader)(null)).FH_AllowTaskAutoAssignment)));
			this.AutoAssignTasksCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(219, 18, true);
			this.AutoAssignTasksCheckBox.Name = "AutoAssignTasksCheckBox";
			this.AutoAssignTasksCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 17, true);
			this.AutoAssignTasksCheckBox.TabIndex = 16;
			this.AutoAssignTasksCheckBox.UseVisualStyleBackColor = true;
			// 
			// CriticalHandoverCheckBox
			// 
			this.CriticalHandoverCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.CriticalHandoverCheckBox, "FH_IsCriticalHandover");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.BufferManagement.Business.ProcessHeader)(null)).FH_IsCriticalHandover)));
			this.CriticalHandoverCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 18, true);
			this.CriticalHandoverCheckBox.Name = "CriticalHandoverCheckBox";
			this.CriticalHandoverCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(107, 17, true);
			this.CriticalHandoverCheckBox.TabIndex = 14;
			this.CriticalHandoverCheckBox.UseVisualStyleBackColor = true;
			// 
			// ReleaseDelayFactorCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ReleaseDelayFactorCalcEdit, "JobHeader.FH_TimeDelayFactor");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BufferManagement.Business.ProcessHeader)(null)).JobHeader.FH_TimeDelayFactor)));
			this.ReleaseDelayFactorCalcEdit.DecimalPlaces = 2;
			this.ReleaseDelayFactorCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(282, 19, true);
			this.ReleaseDelayFactorCalcEdit.Name = "ReleaseDelayFactorCalcEdit";
			this.ReleaseDelayFactorCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 20, true);
			this.ReleaseDelayFactorCalcEdit.TabIndex = 51;
			this.ReleaseDelayFactorCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.ReleaseDelayFactorCalcEdit.TrackDisposedAccess = true;
			// 
			// ReleaseDelayTimeEditEx
			// 
			this.ReleaseDelayTimeEditEx.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.ReleaseDelayTimeEditEx, "JobHeader.StaggeredReleaseTimeDelay");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.BufferManagement.Business.ProcessHeader)(null)).JobHeader.StaggeredReleaseTimeDelay)));
			this.ReleaseDelayTimeEditEx.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 19, true);
			this.ReleaseDelayTimeEditEx.Name = "ReleaseDelayTimeEditEx";
			this.ReleaseDelayTimeEditEx.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 20, true);
			this.ReleaseDelayTimeEditEx.TabIndex = 50;
			// 
			// AgreedDeliveryDateEdit
			// 
			this.AgreedDeliveryDateEdit.AllowDrop = true;
			this.AgreedDeliveryDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.AgreedDeliveryDateEdit, "AgreedDeliveryDateLocal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.BufferManagement.Business.ProcessHeader)(null)).AgreedDeliveryDateLocal)));
			this.AgreedDeliveryDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.AgreedDeliveryDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 71, true);
			this.AgreedDeliveryDateEdit.Name = "AgreedDeliveryDateEdit";
			this.AgreedDeliveryDateEdit.TabIndex = 18;
			// 
			// EarliestStartDateEdit
			// 
			this.EarliestStartDateEdit.AllowDrop = true;
			this.EarliestStartDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.EarliestStartDateEdit, "DoNotStartBeforeDateLocal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.BufferManagement.Business.ProcessHeader)(null)).DoNotStartBeforeDateLocal)));
			this.EarliestStartDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.EarliestStartDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 45, true);
			this.EarliestStartDateEdit.Name = "EarliestStartDateEdit";
			this.EarliestStartDateEdit.TabIndex = 17;
			// 
			// WorkflowSchedulingGroupBox
			// 
			this.WorkflowSchedulingGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.WorkflowSchedulingGroupBox.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("568a9577-5708-489c-a4a3-124cb49e9a43", "Workflow Scheduling");
			this.WorkflowSchedulingGroupBox.Controls.Add(this.IsStandbyCheckBox);
			this.WorkflowSchedulingGroupBox.Controls.Add(this.DateAcceptabilityControl);
			this.WorkflowSchedulingGroupBox.Controls.Add(this.EarliestStartDateEdit);
			this.WorkflowSchedulingGroupBox.Controls.Add(this.AgreedDeliveryDateEdit);
			this.WorkflowSchedulingGroupBox.Controls.Add(this.CriticalHandoverCheckBox);
			this.WorkflowSchedulingGroupBox.Controls.Add(this.AutoAssignTasksCheckBox);
			this.WorkflowSchedulingGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 263, true);
			this.WorkflowSchedulingGroupBox.Name = "WorkflowSchedulingGroupBox";
			this.WorkflowSchedulingGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(377, 124, true);
			this.WorkflowSchedulingGroupBox.TabIndex = 2;
			this.WorkflowSchedulingGroupBox.TabStop = false;
			// 
			// IsStandbyCheckBox
			// 
			this.IsStandbyCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsStandbyCheckBox, "FH_IsStandby");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.BufferManagement.Business.ProcessHeader)(null)).FH_IsStandby)));
			this.IsStandbyCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(121, 18, true);
			this.IsStandbyCheckBox.Name = "IsStandbyCheckBox";
			this.IsStandbyCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(92, 17, true);
			this.IsStandbyCheckBox.TabIndex = 15;
			this.IsStandbyCheckBox.UseVisualStyleBackColor = true;
			// 
			// DateAcceptabilityControl
			// 
			this.DateAcceptabilityControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DateAcceptabilityControl, ".");
			this.DateAcceptabilityControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 96, true);
			this.DateAcceptabilityControl.Name = "DateAcceptabilityControl";
			this.DateAcceptabilityControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(253, 20, true);
			this.DateAcceptabilityControl.TabIndex = 19;
			// 
			// SequencingGroupBox
			// 
			this.SequencingGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.SequencingGroupBox.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("912e5732-272e-4f3c-8614-aac46fc5c00f", "Sequencing");
			this.SequencingGroupBox.Controls.Add(this.SequencePositionTextBox);
			this.SequencingGroupBox.Controls.Add(this.SequenceNameTextBox);
			this.SequencingGroupBox.Controls.Add(this.OpenSequenceButton);
			this.SequencingGroupBox.Controls.Add(this.SequenceWorkflowTextBox);
			this.SequencingGroupBox.Controls.Add(this.SequenceParentJobLabel);
			this.SequencingGroupBox.Controls.Add(this.SequenceParentJobLinkLabel);
			this.SequencingGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 686, true);
			this.SequencingGroupBox.Name = "SequencingGroupBox";
			this.SequencingGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(377, 128, true);
			this.SequencingGroupBox.TabIndex = 3;
			this.SequencingGroupBox.TabStop = false;
			// 
			// SequencePositionTextBox
			// 
			this.BindingSource.SetBindingMember(this.SequencePositionTextBox, "ReleaseSequencePosition");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.BufferManagement.Business.ProcessHeader)(null)).ReleaseSequencePosition)));
			this.SequencePositionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 45, true);
			this.SequencePositionTextBox.Name = "SequencePositionTextBox";
			this.SequencePositionTextBox.ReadOnly = true;
			this.SequencePositionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 20, true);
			this.SequencePositionTextBox.TabIndex = 21;
			// 
			// SequenceNameTextBox
			// 
			this.SequenceNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.SequenceNameTextBox, "ReleaseSequenceName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.ProcessHeader)(null)).ReleaseSequenceName)));
			this.SequenceNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.SequenceNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 19, true);
			this.SequenceNameTextBox.Name = "SequenceNameTextBox";
			this.SequenceNameTextBox.ReadOnly = true;
			this.SequenceNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(262, 20, true);
			this.SequenceNameTextBox.TabIndex = 20;
			// 
			// OpenSequenceButton
			// 
			this.OpenSequenceButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.OpenSequenceButton.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("aa9abf47-2158-4559-93e3-2398ec74f544", "Open Sequence");
			this.OpenSequenceButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(162, 43, true);
			this.OpenSequenceButton.Name = "OpenSequenceButton";
			this.OpenSequenceButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(209, 23, true);
			this.OpenSequenceButton.TabIndex = 22;
			this.OpenSequenceButton.ToolTipCaption = null;
			this.OpenSequenceButton.UseVisualStyleBackColor = true;
			this.OpenSequenceButton.Click += new System.EventHandler(this.OpenSequenceButton_Click);
			// 
			// SequenceWorkflowTextBox
			// 
			this.SequenceWorkflowTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.SequenceWorkflowTextBox, "ReleaseSequenceWorkflow");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.ProcessHeader)(null)).ReleaseSequenceWorkflow)));
			this.SequenceWorkflowTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.SequenceWorkflowTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 71, true);
			this.SequenceWorkflowTextBox.Name = "SequenceWorkflowTextBox";
			this.SequenceWorkflowTextBox.ReadOnly = true;
			this.SequenceWorkflowTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(262, 20, true);
			this.SequenceWorkflowTextBox.TabIndex = 23;
			// 
			// SequenceParentJobLabel
			// 
			this.SequenceParentJobLabel.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("C349EBF1-36A4-43C5-96F2-313D473C89FD", "Job Parent");
			this.SequenceParentJobLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.SequenceParentJobLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 100, true);
			this.SequenceParentJobLabel.Name = "SequenceParentJobLabel";
			this.SequenceParentJobLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(107, 20, true);
			this.SequenceParentJobLabel.TabIndex = 24;
			this.SequenceParentJobLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
			this.SequenceParentJobLabel.UseMnemonic = false;
			// 
			// SequenceParentJobLinkLabel
			// 
			this.SequenceParentJobLinkLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.SequenceParentJobLinkLabel, "ReleaseSequenceParentJob");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.ProcessHeader)(null)).ReleaseSequenceParentJob)));
			this.SequenceParentJobLinkLabel.IsFontBold = false;
			this.SequenceParentJobLinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 100, true);
			this.SequenceParentJobLinkLabel.Name = "SequenceParentJobLinkLabel";
			this.SequenceParentJobLinkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(246, 20, true);
			this.SequenceParentJobLinkLabel.TabIndex = 23;
			this.SequenceParentJobLinkLabel.Click += new System.EventHandler(this.SequenceParentJobLinkLabel_Click);
			// 
			// RelationshipsGroupBox
			// 
			this.RelationshipsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.RelationshipsGroupBox.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("7461ed81-2223-471f-b519-e6caab686651", "Dependency Relationships");
			this.RelationshipsGroupBox.Controls.Add(this.OpenPrereqsCalcEdit);
			this.RelationshipsGroupBox.Controls.Add(this.PrereqListButton);
			this.RelationshipsGroupBox.Controls.Add(this.WorkflowRelationshipsButton);
			this.RelationshipsGroupBox.Controls.Add(this.DependencyStatusTextBox);
			this.RelationshipsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 183, true);
			this.RelationshipsGroupBox.Name = "RelationshipsGroupBox";
			this.RelationshipsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(377, 74, true);
			this.RelationshipsGroupBox.TabIndex = 1;
			this.RelationshipsGroupBox.TabStop = false;
			// 
			// OpenPrereqsCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.OpenPrereqsCalcEdit, "NumberOfOpenPrerequisitesUpTheTree");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BufferManagement.Business.ProcessHeader)(null)).NumberOfOpenPrerequisitesUpTheTree)));
			this.OpenPrereqsCalcEdit.DecimalPlaces = 0;
			this.OpenPrereqsCalcEdit.Decimals = 0;
			this.OpenPrereqsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 45, true);
			this.OpenPrereqsCalcEdit.Name = "OpenPrereqsCalcEdit";
			this.OpenPrereqsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 20, true);
			this.OpenPrereqsCalcEdit.TabIndex = 11;
			this.OpenPrereqsCalcEdit.Text = "0";
			this.OpenPrereqsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.OpenPrereqsCalcEdit.TrackDisposedAccess = true;
			// 
			// PrereqListButton
			// 
			this.PrereqListButton.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("c07e77fc-9af0-47f3-bdb2-6bb8adc6dc4a", "Prerequisites", "View the list of all prerequisites of the selected workflow, including those inhe" +
        "rited from the job-level workflow and prerequisites of its prerequisites.");
			this.PrereqListButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 45, true);
			this.PrereqListButton.Name = "PrereqListButton";
			this.PrereqListButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 22, true);
			this.PrereqListButton.TabIndex = 12;
			this.PrereqListButton.ToolTipCaption = null;
			this.PrereqListButton.UseVisualStyleBackColor = true;
			this.PrereqListButton.Click += new System.EventHandler(this.PrereqListButton_Click);
			// 
			// JobStaggeredStartsGroupBox
			// 
			this.JobStaggeredStartsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.JobStaggeredStartsGroupBox.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("d0815e05-befa-45ea-b465-113e7520d972", "Job Staggered Release");
			this.JobStaggeredStartsGroupBox.Controls.Add(this.ReleaseDelayFactorCalcEdit);
			this.JobStaggeredStartsGroupBox.Controls.Add(this.ReleaseDelayTimeEditEx);
			this.JobStaggeredStartsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 820, true);
			this.JobStaggeredStartsGroupBox.Name = "JobStaggeredStartsGroupBox";
			this.JobStaggeredStartsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(377, 48, true);
			this.JobStaggeredStartsGroupBox.TabIndex = 3;
			this.JobStaggeredStartsGroupBox.TabStop = false;
			// 
			// ColumnTableLayoutPanel
			// 
			this.ColumnTableLayoutPanel.AutoSize = true;
			this.ColumnTableLayoutPanel.ColumnCount = 1;
			this.ColumnTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.ColumnTableLayoutPanel.Controls.Add(this.JobStaggeredStartsGroupBox, 0, 5);
			this.ColumnTableLayoutPanel.Controls.Add(this.DetailsGroupBox, 0, 0);
			this.ColumnTableLayoutPanel.Controls.Add(this.RelationshipsGroupBox, 0, 1);
			this.ColumnTableLayoutPanel.Controls.Add(this.SequencingGroupBox, 0, 4);
			this.ColumnTableLayoutPanel.Controls.Add(this.WorkflowSchedulingGroupBox, 0, 2);
			this.ColumnTableLayoutPanel.Controls.Add(this.NewReleaseGateGroupBox, 0, 3);
			this.ColumnTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ColumnTableLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ColumnTableLayoutPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.ColumnTableLayoutPanel.Name = "ColumnTableLayoutPanel";
			this.ColumnTableLayoutPanel.RowCount = 6;
			this.ColumnTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.ColumnTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.ColumnTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.ColumnTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.ColumnTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.ColumnTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.ColumnTableLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(383, 871, true);
			this.ColumnTableLayoutPanel.TabIndex = 4;
			// 
			// NewReleaseGateGroupBox
			// 
			this.NewReleaseGateGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.NewReleaseGateGroupBox.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("ec31d970-a78e-4dc9-a536-7731ca71284a", "New Release Gate");
			this.NewReleaseGateGroupBox.Controls.Add(this.IsApprovedCheckBox);
			this.NewReleaseGateGroupBox.Controls.Add(this.DedicatedBufferTextBox);
			this.NewReleaseGateGroupBox.Controls.Add(this.BranchFindBox);
			this.NewReleaseGateGroupBox.Controls.Add(this.DepartmentFindBox);
			this.NewReleaseGateGroupBox.Controls.Add(this.EffectiveBranchTextBox);
			this.NewReleaseGateGroupBox.Controls.Add(this.EffectiveDepartmentTextBox);
			this.NewReleaseGateGroupBox.Controls.Add(this.NewEffectiveNudgeEdit);
			this.NewReleaseGateGroupBox.Controls.Add(this.EffectiveNudgeEdit);
			this.NewReleaseGateGroupBox.Controls.Add(this.EffectiveAgreedDeliveryDate);
			this.NewReleaseGateGroupBox.Controls.Add(this.LatestAcceptableReleaseDate);
			this.NewReleaseGateGroupBox.Controls.Add(this.ReleaseSequenceSortDate);
			this.NewReleaseGateGroupBox.Controls.Add(this.BufferTimespanDropEdit);
			this.NewReleaseGateGroupBox.Controls.Add(this.EffectiveBufferDuration);
			this.NewReleaseGateGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 393, true);
			this.NewReleaseGateGroupBox.Name = "NewReleaseGateGroupBox";
			this.NewReleaseGateGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(377, 287, true);
			this.NewReleaseGateGroupBox.TabIndex = 24;
			this.NewReleaseGateGroupBox.TabStop = false;
			// 
			// IsApprovedCheckBox
			// 
			this.IsApprovedCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsApprovedCheckBox, "FH_IsApproved");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.BufferManagement.Business.ProcessHeader)(null)).FH_IsApproved)));
			this.IsApprovedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 18, true);
			this.IsApprovedCheckBox.Name = "IsApprovedCheckBox";
			this.IsApprovedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 17, true);
			this.IsApprovedCheckBox.TabIndex = 25;
			this.IsApprovedCheckBox.UseVisualStyleBackColor = true;
			// 
			// DedicatedBufferTextBox
			// 
			this.DedicatedBufferTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.DedicatedBufferTextBox, "DedicatedBufferName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.ProcessHeader)(null)).DedicatedBufferName)));
			this.DedicatedBufferTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 44, true);
			this.DedicatedBufferTextBox.Name = "DedicatedBufferTextBox";
			this.DedicatedBufferTextBox.ReadOnly = true;
			this.DedicatedBufferTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(261, 20, true);
			this.DedicatedBufferTextBox.TabIndex = 26;
			// 
			// BranchFindBox
			// 
			this.BranchFindBox.AllowDrop = true;
			this.BranchFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.BranchFindBox, "FH_GB_Branch");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.BufferManagement.Business.ProcessHeader)(null)).FH_GB_Branch)));
			this.BranchFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 70, true);
			this.BranchFindBox.Name = "BranchFindBox";
			this.BranchFindBox.ParentType = null;
			this.BranchFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(99, 20, true);
			this.BranchFindBox.TabIndex = 28;
			// 
			// DepartmentFindBox
			// 
			this.DepartmentFindBox.AllowDrop = true;
			this.DepartmentFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.DepartmentFindBox, "FH_GE_Department");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.BufferManagement.Business.ProcessHeader)(null)).FH_GE_Department)));
			this.DepartmentFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 96, true);
			this.DepartmentFindBox.Name = "DepartmentFindBox";
			this.DepartmentFindBox.ParentType = null;
			this.DepartmentFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(99, 20, true);
			this.DepartmentFindBox.TabIndex = 31;
			// 
			// EffectiveBranchTextBox
			// 
			this.EffectiveBranchTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.EffectiveBranchTextBox, "EffectiveBranchCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.ProcessHeader)(null)).EffectiveBranchCode)));
			this.EffectiveBranchTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(280, 70, true);
			this.EffectiveBranchTextBox.Name = "EffectiveBranchTextBox";
			this.EffectiveBranchTextBox.ReadOnly = true;
			this.EffectiveBranchTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(55, 20, true);
			this.EffectiveBranchTextBox.TabIndex = 29;
			// 
			// EffectiveDepartmentTextBox
			// 
			this.EffectiveDepartmentTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.EffectiveDepartmentTextBox, "EffectiveDepartmentCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.ProcessHeader)(null)).EffectiveDepartmentCode)));
			this.EffectiveDepartmentTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(280, 96, true);
			this.EffectiveDepartmentTextBox.Name = "EffectiveDepartmentTextBox";
			this.EffectiveDepartmentTextBox.ReadOnly = true;
			this.EffectiveDepartmentTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(55, 20, true);
			this.EffectiveDepartmentTextBox.TabIndex = 32;
			// 
			// NewEffectiveNudgeEdit
			// 
			this.BindingSource.SetBindingMember(this.NewEffectiveNudgeEdit, "FH_EffectiveNudge");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BufferManagement.Business.ProcessHeader)(null)).FH_EffectiveNudge)));
			this.NewEffectiveNudgeEdit.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("0E09075A-668E-4D1E-B490-860ED2E772DB", "Persistent Nudge");
			this.NewEffectiveNudgeEdit.DecimalPlaces = 4;
			this.NewEffectiveNudgeEdit.Decimals = 4;
			this.NewEffectiveNudgeEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(300, 122, true);
			this.NewEffectiveNudgeEdit.Name = "NewEffectiveNudgeEdit";
			this.NewEffectiveNudgeEdit.ReadOnly = true;
			this.NewEffectiveNudgeEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 20, true);
			this.NewEffectiveNudgeEdit.TabIndex = 33;
			this.NewEffectiveNudgeEdit.Text = "0.0000";
			this.NewEffectiveNudgeEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.NewEffectiveNudgeEdit.TrackDisposedAccess = true;
			// 
			// EffectiveNudgeEdit
			// 
			this.BindingSource.SetBindingMember(this.EffectiveNudgeEdit, "EffectiveNudge");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BufferManagement.Business.ProcessHeader)(null)).EffectiveNudge)));
			this.EffectiveNudgeEdit.DecimalPlaces = 4;
			this.EffectiveNudgeEdit.Decimals = 4;
			this.EffectiveNudgeEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 122, true);
			this.EffectiveNudgeEdit.Name = "EffectiveNudgeEdit";
			this.EffectiveNudgeEdit.ReadOnly = true;
			this.EffectiveNudgeEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 20, true);
			this.EffectiveNudgeEdit.TabIndex = 32;
			this.EffectiveNudgeEdit.Text = "0.0000";
			this.EffectiveNudgeEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.EffectiveNudgeEdit.TrackDisposedAccess = true;
			// 
			// EffectiveAgreedDeliveryDate
			// 
			this.EffectiveAgreedDeliveryDate.AllowDrop = true;
			this.EffectiveAgreedDeliveryDate.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.EffectiveAgreedDeliveryDate, "FH_EffectiveAgreedDeliveryDateUtc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.BufferManagement.Business.ProcessHeader)(null)).FH_EffectiveAgreedDeliveryDateUtc)));
			this.EffectiveAgreedDeliveryDate.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.EffectiveAgreedDeliveryDate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(207, 148, true);
			this.EffectiveAgreedDeliveryDate.Name = "EffectiveAgreedDeliveryDate";
			this.EffectiveAgreedDeliveryDate.TabIndex = 34;
			// 
			// LatestAcceptableReleaseDate
			// 
			this.LatestAcceptableReleaseDate.AllowDrop = true;
			this.LatestAcceptableReleaseDate.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.LatestAcceptableReleaseDate, "FH_LatestAcceptableReleaseDateUtc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.BufferManagement.Business.ProcessHeader)(null)).FH_LatestAcceptableReleaseDateUtc)));
			this.LatestAcceptableReleaseDate.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.LatestAcceptableReleaseDate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(207, 174, true);
			this.LatestAcceptableReleaseDate.Name = "LatestAcceptableReleaseDate";
			this.LatestAcceptableReleaseDate.TabIndex = 35;
			// 
			// ReleaseSequenceSortDate
			// 
			this.ReleaseSequenceSortDate.AllowDrop = true;
			this.ReleaseSequenceSortDate.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.ReleaseSequenceSortDate, "FH_ReleaseSequenceSortDateUtc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.BufferManagement.Business.ProcessHeader)(null)).FH_ReleaseSequenceSortDateUtc)));
			this.ReleaseSequenceSortDate.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.ReleaseSequenceSortDate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(207, 200, true);
			this.ReleaseSequenceSortDate.Name = "ReleaseSequenceSortDate";
			this.ReleaseSequenceSortDate.TabIndex = 36;
			// 
			// BufferTimespanDropEdit
			// 
			this.BufferTimespanDropEdit.AllowDrop = true;
			this.BufferTimespanDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.BufferTimespanDropEdit, "FH_BMT_BufferTimespan");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.BufferManagement.Business.ProcessHeader)(null)).FH_BMT_BufferTimespan)));
			this.BufferTimespanDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.BufferTimespanDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 226, true);
			this.BufferTimespanDropEdit.Name = "BufferTimespanDropEdit";
			this.BufferTimespanDropEdit.ShowDescriptionBox = false;
			this.BufferTimespanDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(231, 20, true);
			this.BufferTimespanDropEdit.TabIndex = 37;
			this.BufferTimespanDropEdit.UseFullWidthForCodeBox = true;
			// 
			// EffectiveBufferDuration
			// 
			this.EffectiveBufferDuration.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.EffectiveBufferDuration, "EffectiveBufferDurationString");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.ProcessHeader)(null)).EffectiveBufferDurationString)));
			this.EffectiveBufferDuration.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 261, true);
			this.EffectiveBufferDuration.Name = "EffectiveBufferDuration";
			this.EffectiveBufferDuration.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.EffectiveBufferDuration.TabIndex = 51;
			// 
			// WorkflowDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSize = true;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ColumnTableLayoutPanel);
			this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.Name = "WorkflowDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(383, 871, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.PerformLayout();
			this.SystemNameFindBox.ResumeLayout(true);
			this.SystemNameFindBox.PerformLayout();
			this.ReleaseGroupFindBox.ResumeLayout(true);
			this.ReleaseGroupFindBox.PerformLayout();
			this.ComponentDropEdit.ResumeLayout(true);
			this.ComponentDropEdit.PerformLayout();
			this.AgreedDeliveryDateEdit.ResumeLayout(true);
			this.AgreedDeliveryDateEdit.PerformLayout();
			this.EarliestStartDateEdit.ResumeLayout(true);
			this.EarliestStartDateEdit.PerformLayout();
			this.WorkflowSchedulingGroupBox.ResumeLayout(false);
			this.WorkflowSchedulingGroupBox.PerformLayout();
			this.DateAcceptabilityControl.ResumeLayout(true);
			this.DateAcceptabilityControl.PerformLayout();
			this.SequencingGroupBox.ResumeLayout(false);
			this.SequencingGroupBox.PerformLayout();
			this.RelationshipsGroupBox.ResumeLayout(false);
			this.RelationshipsGroupBox.PerformLayout();
			this.JobStaggeredStartsGroupBox.ResumeLayout(false);
			this.JobStaggeredStartsGroupBox.PerformLayout();
			this.ColumnTableLayoutPanel.ResumeLayout(false);
			this.ColumnTableLayoutPanel.PerformLayout();
			this.NewReleaseGateGroupBox.ResumeLayout(false);
			this.NewReleaseGateGroupBox.PerformLayout();
			this.BranchFindBox.ResumeLayout(true);
			this.BranchFindBox.PerformLayout();
			this.DepartmentFindBox.ResumeLayout(true);
			this.DepartmentFindBox.PerformLayout();
			this.EffectiveAgreedDeliveryDate.ResumeLayout(true);
			this.EffectiveAgreedDeliveryDate.PerformLayout();
			this.LatestAcceptableReleaseDate.ResumeLayout(true);
			this.LatestAcceptableReleaseDate.PerformLayout();
			this.ReleaseSequenceSortDate.ResumeLayout(true);
			this.ReleaseSequenceSortDate.PerformLayout();
			this.BufferTimespanDropEdit.ResumeLayout(true);
			this.BufferTimespanDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

#if WINZOR
		private void InitializeWorkflowRelationshipDesignerButton()
		{
			this.WorkflowRelationshipDesignerButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.WorkflowRelationshipDesignerButton.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("e631a050-8005-4648-8e0a-51ced746eda3", "Workflow Relationship Designer", "Open Workflow Relationship Designer");
			this.WorkflowRelationshipDesignerButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 70, true);
			this.WorkflowRelationshipDesignerButton.Name = "WorkflowRelationshipDesignerButton";
			this.WorkflowRelationshipDesignerButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 22, true);
			this.WorkflowRelationshipDesignerButton.TabIndex = 14;
			this.WorkflowRelationshipDesignerButton.UseVisualStyleBackColor = true;
			this.WorkflowRelationshipDesignerButton.Click += new System.EventHandler(this.WorkflowRelationshipDesignerButton_Click);

			this.RelationshipsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(371, 100, true);
			this.RelationshipsGroupBox.Controls.Add(this.WorkflowRelationshipDesignerButton);
		}

		public ZArchitecture.GUI.ZButton WorkflowRelationshipDesignerButton;
#endif

		private ZArchitecture.GUI.ZGroupBox DetailsGroupBox;
		private ZArchitecture.ZTextBox DescriptionTextBox;
		private ZArchitecture.GUI.ZCheckBox ActiveCheckBox;
		private ZArchitecture.GUI.ZCheckBox CriticalHandoverCheckBox;
		private ZArchitecture.ZTextBox EstimateSummaryTextBox;
		private ZArchitecture.GUI.ZCheckBox AutoAssignTasksCheckBox;
		private ZArchitecture.ZTextBox ActualHoursTextBox;
		private ZArchitecture.GUI.ZDateEdit EarliestStartDateEdit;
		private ZArchitecture.GUI.ZDateEdit AgreedDeliveryDateEdit;
		private ZArchitecture.GUI.ZTimeEditEx ReleaseDelayTimeEditEx;
		private ZArchitecture.ZCalcEdit ReleaseDelayFactorCalcEdit;
		private ZArchitecture.GUI.ZGuidDropEdit ComponentDropEdit;
		public ZArchitecture.GUI.ZGuidFindBox ReleaseGroupFindBox;
		private ZArchitecture.GUI.ZButton TransferDiagnosisButton;
		private ZArchitecture.GUI.ZButton WorkflowRelationshipsButton;
		private ZArchitecture.GUI.ZGroupBox WorkflowSchedulingGroupBox;
		public ZArchitecture.GUI.ZGuidFindBox SystemNameFindBox;
		private DateAcceptabilityControl DateAcceptabilityControl;
		private ZArchitecture.GUI.ZCheckBox IsStandbyCheckBox;
		private ZArchitecture.ZTextBox DependencyStatusTextBox;
		private ZArchitecture.GUI.ZGroupBox RelationshipsGroupBox;
		private ZArchitecture.GUI.ZButton PrereqListButton;
		private ZArchitecture.ZTextBox StatusTextBox;
		private Enterprise.BufferManagement.GUI.WorkflowDependencyStatusCalcEdit OpenPrereqsCalcEdit;
		private ZArchitecture.GUI.ZGroupBox JobStaggeredStartsGroupBox;
		public ZArchitecture.GUI.ZGroupBox SequencingGroupBox;
		public ZArchitecture.GUI.ZButton OpenSequenceButton;
		private ZArchitecture.ZTextBox SequencePositionTextBox;
		private ZArchitecture.ZTextBox SequenceNameTextBox;
		private ZArchitecture.ZTextBox SequenceWorkflowTextBox;
		private ZArchitecture.ZLabel SequenceParentJobLabel;
		public ZArchitecture.GUI.ZLinkLabel SequenceParentJobLinkLabel;
		private CargoWise.Windows.UI.KTableLayoutPanel ColumnTableLayoutPanel;
		private ZArchitecture.GUI.ZGroupBox NewReleaseGateGroupBox;
		private ZArchitecture.GUI.ZCheckBox IsApprovedCheckBox;
		private ZArchitecture.ZTextBox DedicatedBufferTextBox;
		public ZArchitecture.GUI.ZGuidFindBox BranchFindBox;
		public ZArchitecture.GUI.ZGuidFindBox DepartmentFindBox;
		public ZArchitecture.ZTextBox EffectiveBranchTextBox;
		public ZArchitecture.ZTextBox EffectiveDepartmentTextBox;
		private ZArchitecture.ZCalcEdit EffectiveNudgeEdit;
		private ZArchitecture.ZCalcEdit NewEffectiveNudgeEdit;
		private ZArchitecture.GUI.ZDateEdit EffectiveAgreedDeliveryDate;
		private ZArchitecture.GUI.ZDateEdit ReleaseSequenceSortDate;
		private ZArchitecture.GUI.ZDateEdit LatestAcceptableReleaseDate;
		private ZArchitecture.GUI.ZGuidDropEdit BufferTimespanDropEdit;
		private ZArchitecture.ZTextBox EffectiveBufferDuration;
	}
}
