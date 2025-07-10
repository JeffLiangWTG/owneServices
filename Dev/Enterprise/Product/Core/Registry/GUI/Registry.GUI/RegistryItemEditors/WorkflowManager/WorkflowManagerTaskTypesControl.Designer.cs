using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	partial class WorkflowManagerTaskTypesControl : RegistryZUserControl
	{
		ZGroupBox ParentGroupBox;
		CargoWise.Windows.UI.KSplitter GridSplitter;
		ZGroupBox ChildGroupBox;
		ZArchitecture.ZGrid ChildGrid;
		ZArchitecture.ZGrid ParentGrid;
		ZArchitecture.ZCheckBoxColumnStyleInfo IsApprovalTaskColumnStyle;

		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			IsApprovalTaskColumnStyle = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			this.ParentGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ParentGrid = new Enterprise.ZArchitecture.ZGrid();
			this.GridSplitter = new CargoWise.Windows.UI.KSplitter();
			this.ChildGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ChildGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ParentGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ParentGrid)).BeginInit();
			this.ParentGrid.SuspendLayout();
			this.ChildGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ChildGrid)).BeginInit();
			this.ChildGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.CategorisedWorkflowTaskTypesCollection);
			// 
			// ParentGroupBox
			// 
			this.ParentGroupBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("WorkflowManagerTaskTypesControl|324d46bd-e215-4053-86ca-124c09858369", "Workflow Types");
			this.ParentGroupBox.Controls.Add(this.ParentGrid);
			this.ParentGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.ParentGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ParentGroupBox.Name = "ParentGroupBox";
			this.ParentGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(481, 176, true);
			this.ParentGroupBox.TabIndex = 1;
			this.ParentGroupBox.TabStop = false;
			// 
			// ParentGrid
			// 
			this.ParentGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ParentGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.CategorisedWorkflowTaskTypes)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.CategorisedWorkflowTaskTypes)(null)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.CategorisedWorkflowTaskTypes)(null)).EnglishDescription)));
			this.ParentGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("WorkflowManagerTaskTypesControl|c418a11d-1ff3-481d-8c49-4834e414a756", "Code");
			zTextBoxColumnStyleInfo1.ColumnName = "Code";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("WorkflowManagerTaskTypesControl|0928573f-8ce0-424f-8741-cf425dab7d1f", "Description");
			zTextBoxColumnStyleInfo2.ColumnName = "EnglishDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			this.ParentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ParentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ParentGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ParentGrid.GridId = "89db08b3-5143-4cfe-9e00-e0d758b0ed74";
			this.ParentGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ParentGrid.LayoutKey = "zGrid1";
			this.ParentGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ParentGrid.Name = "ParentGrid";
			this.ParentGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(475, 157, true);
			this.ParentGrid.TabIndex = 0;
			// 
			// GridSplitter
			// 
			this.GridSplitter.Dock = System.Windows.Forms.DockStyle.Top;
			this.GridSplitter.DoNotSaveSplitterLayout = false;
			this.GridSplitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 176, true);
			this.GridSplitter.Name = "GridSplitter";
			this.GridSplitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(481, 3, true);
			this.GridSplitter.TabIndex = 3;
			this.GridSplitter.TabStop = false;
			// 
			// ChildGroupBox
			// 
			this.ChildGroupBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("WorkflowManagerTaskTypesControl|9974d323-bce7-4e79-a410-e94a2c65c467", "Task Types");
			this.ChildGroupBox.Controls.Add(this.ChildGrid);
			this.ChildGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ChildGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 179, true);
			this.ChildGroupBox.Name = "ChildGroupBox";
			this.ChildGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(481, 177, true);
			this.ChildGroupBox.TabIndex = 4;
			this.ChildGroupBox.TabStop = false;
			// 
			// ChildGrid
			// 
			this.ChildGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ChildGrid, "TaskTypes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.CategorisedWorkflowTaskTypes)(null)).TaskTypes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.WorkflowTaskType)(((System.Collections.IList)(((Enterprise.Registry.Business.CategorisedWorkflowTaskTypes)(null)).TaskTypes)).SyncRoot)).IsActive)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.WorkflowTaskType)(((System.Collections.IList)(((Enterprise.Registry.Business.CategorisedWorkflowTaskTypes)(null)).TaskTypes)).SyncRoot)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.WorkflowTaskType)(((System.Collections.IList)(((Enterprise.Registry.Business.CategorisedWorkflowTaskTypes)(null)).TaskTypes)).SyncRoot)).EnglishDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.WorkflowTaskType)(((System.Collections.IList)(((Enterprise.Registry.Business.CategorisedWorkflowTaskTypes)(null)).TaskTypes)).SyncRoot)).CreatesAppointment)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.WorkflowTaskType)(((System.Collections.IList)(((Enterprise.Registry.Business.CategorisedWorkflowTaskTypes)(null)).TaskTypes)).SyncRoot)).CanCloseTaskNotAssignedToSelf)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.WorkflowTaskType)(((System.Collections.IList)(((Enterprise.Registry.Business.CategorisedWorkflowTaskTypes)(null)).TaskTypes)).SyncRoot)).CanCancelTask)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.WorkflowTaskType)(((System.Collections.IList)(((Enterprise.Registry.Business.CategorisedWorkflowTaskTypes)(null)).TaskTypes)).SyncRoot)).IsExcludedFromTransferRules)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.WorkflowTaskType)(((System.Collections.IList)(((Enterprise.Registry.Business.CategorisedWorkflowTaskTypes)(null)).TaskTypes)).SyncRoot)).IsCompletionStatementTaskType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.WorkflowTaskType)(((System.Collections.IList)(((Enterprise.Registry.Business.CategorisedWorkflowTaskTypes)(null)).TaskTypes)).SyncRoot)).ContainmentBarrierIterationType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.WorkflowTaskType)(((System.Collections.IList)(((Enterprise.Registry.Business.CategorisedWorkflowTaskTypes)(null)).TaskTypes)).SyncRoot)).IsRequireActualDuration)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.WorkflowTaskType)(((System.Collections.IList)(((Enterprise.Registry.Business.CategorisedWorkflowTaskTypes)(null)).TaskTypes)).SyncRoot)).IsWorkProduction)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.WorkflowTaskType)(((System.Collections.IList)(((Enterprise.Registry.Business.CategorisedWorkflowTaskTypes)(null)).TaskTypes)).SyncRoot)).IsApprovalTask)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.WorkflowTaskType)(((System.Collections.IList)(((Enterprise.Registry.Business.CategorisedWorkflowTaskTypes)(null)).TaskTypes)).SyncRoot)).WorkingStatusChangeType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.WorkflowTaskType)(((System.Collections.IList)(((Enterprise.Registry.Business.CategorisedWorkflowTaskTypes)(null)).TaskTypes)).SyncRoot)).AllowTaskReset)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Registry.Business.WorkflowTaskType)(((System.Collections.IList)(((Enterprise.Registry.Business.CategorisedWorkflowTaskTypes)(null)).TaskTypes)).SyncRoot)).DefaultCapability)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.WorkflowTaskType)(((System.Collections.IList)(((Enterprise.Registry.Business.CategorisedWorkflowTaskTypes)(null)).TaskTypes)).SyncRoot)).GlbCapabilityCollection)));
			this.ChildGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("e22799b5-a3f8-4838-bde0-1326c8fbb5e8", "Active", "Specify whether this Task Type can be used. Existing tasks with this type selected will have a warning shown.");
			zCheckBoxColumnStyleInfo1.ColumnName = "IsActive";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("WorkflowManagerTaskTypesControl|c418a11d-1ff3-481d-8c49-4834e414a756", "Code");
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "Code";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("WorkflowManagerTaskTypesControl|0928573f-8ce0-424f-8741-cf425dab7d1f", "Description");
			zTextBoxColumnStyleInfo4.ColumnName = "EnglishDescription";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(170);
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("WorkflowManagerTaskTypesControl|632b7c7f-b1b6-4cff-814f-f7d2e0315116", "Creates Appointment");
			zCheckBoxColumnStyleInfo2.ColumnName = "CreatesAppointment";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(115);
			zCheckBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("WorkflowManagerTaskTypesControl|0ccd2500-07b9-4483-9ce1-c8db54ffd51a", "Can Close Others\' Tasks");
			zCheckBoxColumnStyleInfo3.ColumnName = "CanCloseTaskNotAssignedToSelf";
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(115);
			zCheckBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("WorkflowManagerTaskTypesControl|0a7e78e9-c947-45a0-9745-aa104140c98a", "Can Cancel Tasks");
			zCheckBoxColumnStyleInfo4.ColumnName = "CanCancelTask";
			zCheckBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(115);
			zCheckBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("eae32d84-9577-48c6-9255-13f3287e6588", "Excl. From Transfer Rules", "Excluded From Transfer Rules", "");
			zCheckBoxColumnStyleInfo5.ColumnName = "IsExcludedFromTransferRules";
			zCheckBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zCheckBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("dc0bc18c-18d7-4af2-bf8b-ff54d180f87b", "Completion Statement Task Type");
			zCheckBoxColumnStyleInfo6.ColumnName = "IsCompletionStatementTaskType";
			zCheckBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zCheckBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("19d0a503-1d47-4d8c-8db1-6375558bb281", "Require Actual Duration");
			zCheckBoxColumnStyleInfo7.ColumnName = "IsRequireActualDuration";
			zCheckBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			zCheckBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("d3758ccc-8e12-42cc-b876-959f802d6a67", "Is Work Production");
			zCheckBoxColumnStyleInfo8.ColumnName = "IsWorkProduction";
			zCheckBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(115);
			IsApprovalTaskColumnStyle.ColumnName = "IsApprovalTask";
			IsApprovalTaskColumnStyle.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			IsApprovalTaskColumnStyle.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("49b9f652-5e00-4061-b830-f7c303018f56", "Is Approval Task");
			zDropEditColumnStyleInfo1.ColumnName = "ContainmentBarrierIterationType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("WorkflowManagerTaskTypesControl|03ff8a04-b1ea-43bf-9d53-f0c9d6b337f4", "Allow Working Status Changes in Buckets");
			zDropEditColumnStyleInfo2.ColumnName = "WorkingStatusChangeType";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zCheckBoxColumnStyleInfo11.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("deb4ea00-c990-4378-96c1-811b84db1642", "Allow Task Reset on Assignment or Re-assignment");
			zCheckBoxColumnStyleInfo11.ColumnName = "AllowTaskReset";
			zCheckBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zGuidFindBoxColumnStyleInfo1.BindToList = "GlbCapabilityCollection";
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("DefaultCapabilityControl|475F77CB-95F2-40B9-B138-8FE25C593E53", "Default Capability");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "DefaultCapability";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			this.ChildGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.ChildGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ChildGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ChildGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.ChildGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.ChildGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo4);
			this.ChildGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo5);
			this.ChildGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo6);
			this.ChildGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ChildGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo7);
			this.ChildGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo8);
			this.ChildGrid.ColumnStyles.Add(IsApprovalTaskColumnStyle);
			this.ChildGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.ChildGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo11);
			this.ChildGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.ChildGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ChildGrid.GridId = "1e923bb9-0742-426e-a71c-b6b1f5cb13d0";
			this.ChildGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ChildGrid.LayoutKey = "zGrid1";
			this.ChildGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ChildGrid.Name = "ChildGrid";
			this.ChildGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(475, 158, true);
			this.ChildGrid.TabIndex = 1;
			// 
			// WorkflowManagerTaskTypesControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ChildGroupBox);
			this.Controls.Add(this.GridSplitter);
			this.Controls.Add(this.ParentGroupBox);
			this.Name = "WorkflowManagerTaskTypesControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(481, 356, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ParentGroupBox.ResumeLayout(false);
			this.ParentGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ParentGrid)).EndInit();
			this.ParentGrid.ResumeLayout(false);
			this.ParentGrid.PerformLayout();
			this.ChildGroupBox.ResumeLayout(false);
			this.ChildGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ChildGrid)).EndInit();
			this.ChildGrid.ResumeLayout(false);
			this.ChildGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
