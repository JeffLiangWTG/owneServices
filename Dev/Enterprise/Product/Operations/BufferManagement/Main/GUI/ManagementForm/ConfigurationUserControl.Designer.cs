
namespace Enterprise.BufferManagement.GUI
{
	partial class ConfigurationUserControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo zTimeEditExColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo();
			this.RelatedWorkflowTypesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zLabel7 = new Enterprise.ZArchitecture.ZLabel();
			this.JobTypesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.splitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.ReleaseGroupsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.RelatedWorkflowTypesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.JobTypesGrid)).BeginInit();
			this.JobTypesGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.zGroupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ReleaseGroupsGrid)).BeginInit();
			this.ReleaseGroupsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.BufferManagement.Business.BMSystem);
			// 
			// RelatedWorkflowTypesGroupBox
			// 
			this.RelatedWorkflowTypesGroupBox.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("ca7e2393-be89-492d-bef2-8cc8bbd27ccb", "Related Job Types");
			this.RelatedWorkflowTypesGroupBox.Controls.Add(this.zLabel7);
			this.RelatedWorkflowTypesGroupBox.Controls.Add(this.JobTypesGrid);
			this.RelatedWorkflowTypesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RelatedWorkflowTypesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RelatedWorkflowTypesGroupBox.Name = "RelatedWorkflowTypesGroupBox";
			this.RelatedWorkflowTypesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(505, 597, true);
			this.RelatedWorkflowTypesGroupBox.TabIndex = 5;
			this.RelatedWorkflowTypesGroupBox.TabStop = false;
			// 
			// zLabel7
			// 
			this.zLabel7.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.zLabel7.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("85f74486-bccf-4776-8d9d-0e8e34aa238f", "A Buffer Management System is created to manage the flow of work. The below list specifies the type(s) of jobs that are managed by this Buffer Management System schematic and rules by default. Jobs may use other Buffer Management Systems which are selected using Workflow Templates.");
			this.zLabel7.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel7.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 23, true);
			this.zLabel7.Name = "zLabel7";
			this.zLabel7.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(499, 28, true);
			this.zLabel7.TabIndex = 23;
			// 
			// JobTypesGrid
			// 
			this.JobTypesGrid.AllowNavigation = false;
			this.JobTypesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.JobTypesGrid, "RelatedWorkflowTypes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.BufferManagement.Business.BMSystem)(null)).RelatedWorkflowTypes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.BMSystemWorkflowDeterminer)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.BMSystem)(null)).RelatedWorkflowTypes)).SyncRoot)).FSW_WorkflowType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.BMSystemWorkflowDeterminer)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.BMSystem)(null)).RelatedWorkflowTypes)).SyncRoot)).WorkflowTypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.BufferManagement.Business.BMSystemWorkflowDeterminer)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.BMSystem)(null)).RelatedWorkflowTypes)).SyncRoot)).FSW_IsActive)));
			this.JobTypesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnComparer = null;
			zDropEditColumnStyleInfo1.ColumnName = "FSW_WorkflowType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnComparer = null;
			zTextBoxColumnStyleInfo1.ColumnName = "WorkflowTypeDescription";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			zCheckBoxColumnStyleInfo1.ColumnComparer = null;
			zCheckBoxColumnStyleInfo1.ColumnName = "FSW_IsActive";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.JobTypesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.JobTypesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.JobTypesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.JobTypesGrid.GridId = "5b4e7ed3-10f7-47e5-845c-30402c313eef";
			this.JobTypesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.JobTypesGrid.LayoutKey = "BMComponentGrid";
			this.JobTypesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 65, true);
			this.JobTypesGrid.Name = "JobTypesGrid";
			this.JobTypesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(496, 526, true);
			this.JobTypesGrid.TabIndex = 3;
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainer1.Name = "splitContainer1";
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.RelatedWorkflowTypesGroupBox);
			this.splitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(994, 597, true);
			this.splitContainer1.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.zGroupBox1);
			this.splitContainer1.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			this.splitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(505);
			this.splitContainer1.TabIndex = 6;
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("30ab9228-0814-4215-9093-f19eccaa240b", "Release Group Configuration");
			this.zGroupBox1.Controls.Add(this.zLabel1);
			this.zGroupBox1.Controls.Add(this.ReleaseGroupsGrid);
			this.zGroupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(485, 597, true);
			this.zGroupBox1.TabIndex = 6;
			this.zGroupBox1.TabStop = false;
			// 
			// zLabel1
			// 
			this.zLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.zLabel1.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("b9af11cc-c878-4a25-b07e-d79ce6faa092", "Add Release Groups to this list in order to specify configuration options for them. Listing groups here is optional--any group can be used with any Buffer Management System.");
			this.zLabel1.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 23, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(476, 28, true);
			this.zLabel1.TabIndex = 23;
			// 
			// ReleaseGroupsGrid
			// 
			this.ReleaseGroupsGrid.AllowNavigation = false;
			this.ReleaseGroupsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ReleaseGroupsGrid, "ReleaseGroups");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.BufferManagement.Business.BMSystem)(null)).ReleaseGroups)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.BufferManagement.Business.BMSystemReleaseGroup)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.BMSystem)(null)).ReleaseGroups)).SyncRoot)).FSG_GG_Group)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.BMSystemReleaseGroup)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.BMSystem)(null)).ReleaseGroups)).SyncRoot)).ReleaseGroupDesc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.BufferManagement.Business.BMSystemReleaseGroup)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.BMSystem)(null)).ReleaseGroups)).SyncRoot)).FSG_ResourceCountdownTime)));
			this.ReleaseGroupsGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.ColumnComparer = null;
			zGuidFindBoxColumnStyleInfo1.ColumnName = "FSG_GG_Group";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnComparer = null;
			zTextBoxColumnStyleInfo2.ColumnName = "ReleaseGroupDesc";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zTimeEditExColumnStyleInfo1.AllowNegative = false;
			zTimeEditExColumnStyleInfo1.ColumnComparer = null;
			zTimeEditExColumnStyleInfo1.ColumnName = "FSG_ResourceCountdownTime";
			zTimeEditExColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ReleaseGroupsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.ReleaseGroupsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ReleaseGroupsGrid.ColumnStyles.Add(zTimeEditExColumnStyleInfo1);
			this.ReleaseGroupsGrid.GridId = "5b4e7ed3-10f7-47e5-845c-30402c313eef";
			this.ReleaseGroupsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ReleaseGroupsGrid.LayoutKey = "BMComponentGrid";
			this.ReleaseGroupsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 65, true);
			this.ReleaseGroupsGrid.Name = "ReleaseGroupsGrid";
			this.ReleaseGroupsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(476, 526, true);
			this.ReleaseGroupsGrid.TabIndex = 3;
			// 
			// ConfigurationUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.splitContainer1);
			this.Name = "ConfigurationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(994, 597, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.RelatedWorkflowTypesGroupBox.ResumeLayout(false);
			this.RelatedWorkflowTypesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.JobTypesGrid)).EndInit();
			this.JobTypesGrid.ResumeLayout(false);
			this.JobTypesGrid.PerformLayout();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.splitContainer1.PerformLayout();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ReleaseGroupsGrid)).EndInit();
			this.ReleaseGroupsGrid.ResumeLayout(false);
			this.ReleaseGroupsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox RelatedWorkflowTypesGroupBox;
		private ZArchitecture.ZLabel zLabel7;
		private ZArchitecture.ZGrid JobTypesGrid;
		private CargoWise.Windows.UI.KSplitContainer splitContainer1;
		private ZArchitecture.GUI.ZGroupBox zGroupBox1;
		private ZArchitecture.ZLabel zLabel1;
		private ZArchitecture.ZGrid ReleaseGroupsGrid;
	}
}
