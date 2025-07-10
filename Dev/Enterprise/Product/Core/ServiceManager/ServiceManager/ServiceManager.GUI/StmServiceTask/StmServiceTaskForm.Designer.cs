using System.Windows.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.ZArchitecture.Environment.RegistryConstants;

namespace Enterprise.ServiceManager.GUI
{
	partial class StmServiceTaskForm : Enterprise.ZArchitecture.GUI.ZTemplateForm
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
			this.TopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DefaultScheduleControl = new Enterprise.ServiceManager.GUI.StmServiceTaskDefaultScheduleControl();
			this.RecurrenceControl = new Enterprise.ServiceManager.GUI.StmServiceTaskRecurrenceControl();
			this.DescriptionPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.NextRunTimeDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.BranchFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.LocalNextRunTimeDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.IsActiveCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.TaskDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LogFilesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.LogViewer = new Enterprise.ServiceManager.GUI.LogViewerControl();
			this.ExtendedConfigTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ExtendedConfigControl = new Enterprise.ServiceManager.GUI.StmServiceTaskExtendedConfigurationControl();
			this.NextRunTimeEstimatorControl = new Enterprise.ServiceManager.GUI.NextRunTimeEstimatorControl();
			this.ConfigTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			this.SaveButtonUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TopPanel.SuspendLayout();
			this.DefaultScheduleControl.SuspendLayout();
			this.RecurrenceControl.SuspendLayout();
			this.DescriptionPanel.SuspendLayout();
			this.NextRunTimeDateEdit.SuspendLayout();
			this.BranchFindBox.SuspendLayout();
			this.LocalNextRunTimeDateEdit.SuspendLayout();
			this.LogFilesTabPage.SuspendLayout();
			this.LogViewer.SuspendLayout();
			this.ExtendedConfigTabPage.SuspendLayout();
			this.ExtendedConfigControl.SuspendLayout();
			this.NextRunTimeEstimatorControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			//
			this.MainTabControl.Controls.Add(this.LogFilesTabPage);
			this.MainTabControl.Controls.Add(this.ConfigTabPage);
			this.MainTabControl.Controls.Add(this.ExtendedConfigTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(835, 610, true);
			this.MainTabControl.Controls.SetChildIndex(this.NotesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.ExtendedConfigTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.ConfigTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.LogFilesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.TopPanel);
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(835, 513, true);
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(835, 513, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(835, 536, true);
			// 
			// SaveButtonUserControl
			// 
			this.SaveButtonUserControl.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(835, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ServiceManager.Business.StmServiceTask);
			// 
			// TopPanel
			// 
			this.TopPanel.Controls.Add(this.NextRunTimeEstimatorControl);
			this.TopPanel.Controls.Add(this.DefaultScheduleControl);
			this.TopPanel.Controls.Add(this.RecurrenceControl);
			this.TopPanel.Controls.Add(this.DescriptionPanel);
			this.TopPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TopPanel.Name = "TopPanel";
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(830, 513, true);
			this.TopPanel.TabIndex = 5;
			// 
			// DefaultScheduleControl
			// 
			this.DefaultScheduleControl.Enabled = false;
			this.DefaultScheduleControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 305, true);
			this.DefaultScheduleControl.Name = "DefaultScheduleControl";
			this.DefaultScheduleControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(652, 215, true);
			this.DefaultScheduleControl.TabIndex = 2;
			// 
			// RecurrenceControl
			//
			this.RecurrenceControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RecurrenceControl, ".");
			this.RecurrenceControl.BindTo = null;
			this.RecurrenceControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 86, true);
			this.RecurrenceControl.Name = "RecurrenceControl";
			this.RecurrenceControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(652, 220, true);
			this.RecurrenceControl.TabIndex = 1;
			// 
			// NextRunTimeEstimatorControl
			//
			this.BindingSource.SetBindingMember(this.NextRunTimeEstimatorControl, "NextRunTimeEstimator");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ServiceManager.Business.NextRunTimeEstimator)(((Enterprise.ServiceManager.Business.StmServiceTask)(null)).NextRunTimeEstimator)));
			this.NextRunTimeEstimatorControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(665, 86, true);
			this.NextRunTimeEstimatorControl.Name = "NextRunTimeEstimatorControl";
			this.NextRunTimeEstimatorControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 380, true);
			this.NextRunTimeEstimatorControl.TabIndex = 8;
			// 
			// DescriptionPanel
			// 
			this.DescriptionPanel.Controls.Add(this.NextRunTimeDateEdit);
			this.DescriptionPanel.Controls.Add(this.BranchFindBox);
			this.DescriptionPanel.Controls.Add(this.LocalNextRunTimeDateEdit);
			this.DescriptionPanel.Controls.Add(this.IsActiveCheckBox);
			this.DescriptionPanel.Controls.Add(this.TaskDescriptionTextBox);
			this.DescriptionPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 3, true);
			this.DescriptionPanel.Name = "DescriptionPanel";
			this.DescriptionPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(652, 81, true);
			this.DescriptionPanel.TabIndex = 0;
			// 
			// NextRunTimeDateEdit
			// 
			this.NextRunTimeDateEdit.AllowDrop = true;
			this.NextRunTimeDateEdit.AutoCompleteMonthThreshold = 1;
			this.NextRunTimeDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.NextRunTimeDateEdit, "NextRunTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.ServiceManager.Business.StmServiceTask)(null)).NextRunTime)));
			this.NextRunTimeDateEdit.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("73053947-0123-46a9-a733-6667c9e95e0b", "Next Run Time (UTC)");
			this.NextRunTimeDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.NextRunTimeDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(154, 33, true);
			this.NextRunTimeDateEdit.Name = "NextRunTimeDateEdit";
			this.NextRunTimeDateEdit.TabIndex = 4;
			// 
			// BranchFindBox
			// 
			this.BindingSource.SetBindingMember(this.BranchFindBox, "SST_GB_Branch");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.ServiceManager.Business.StmServiceTask)(null)).SST_GB_Branch)));
			this.BranchFindBox.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("4d46dcc0-986b-4544-9de2-83e4f5df1f43", "Branch");
			this.BranchFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(154, 57, true);
			this.BranchFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbBranch;
			this.BranchFindBox.Name = "BranchFindBox";
			this.BranchFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.BranchFindBox.ParentType = null;
			this.BranchFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(265, 15, true);
			this.BranchFindBox.TabIndex = 6;
			// 
			// LocalNextRunTimeDateEdit
			// 
			this.LocalNextRunTimeDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.LocalNextRunTimeDateEdit, "NextRunTimeLocal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.ServiceManager.Business.StmServiceTask)(null)).NextRunTimeLocal)));
			this.LocalNextRunTimeDateEdit.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("7985e49b-99e8-460a-b52e-9c4a43eb06e7", "(Local)");
			this.LocalNextRunTimeDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.LocalNextRunTimeDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(316, 33, true);
			this.LocalNextRunTimeDateEdit.Name = "LocalNextRunTimeDateEdit";
			this.LocalNextRunTimeDateEdit.TabIndex = 7;
			// 
			// IsActiveCheckBox
			// 
			this.IsActiveCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsActiveCheckBox, "SST_Active");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ServiceManager.Business.StmServiceTask)(null)).SST_Active)));
			this.IsActiveCheckBox.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("8daed22a-227e-4c9f-a448-bf1324a2d835", "Active");
			this.IsActiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(581, 9, true);
			this.IsActiveCheckBox.Name = "IsActiveCheckBox";
			this.IsActiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 13, true);
			this.IsActiveCheckBox.TabIndex = 2;
			// 
			// TaskDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.TaskDescriptionTextBox, "DescriptionMultilingual");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ServiceManager.Business.StmServiceTask)(null)).DescriptionMultilingual)));
			this.TaskDescriptionTextBox.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("616f7c88-04bf-4185-a44d-a6c0a16b46ac", "Schedule Task Description");
			this.TaskDescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.TaskDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(154, 10, true);
			this.TaskDescriptionTextBox.Name = "TaskDescriptionTextBox";
			this.TaskDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(409, 15, true);
			this.TaskDescriptionTextBox.TabIndex = 1;
			//
			// LogFilesTabPage
			//
			this.LogFilesTabPage.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("StmServiceTaskForm|C1865C09-C261-414D-B6BE-423443250A51", "Log Files");
			this.LogFilesTabPage.Controls.Add(this.LogViewer);
			this.LogFilesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.LogFilesTabPage.Name = "LogFilesTabPage";
			this.LogFilesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.LogFilesTabPage.ShouldBeReadOnlyInViewMode = false;
			this.LogFilesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(666, 286, true);
			this.LogFilesTabPage.TabIndex = 3;
			this.LogFilesTabPage.UseVisualStyleBackColor = true;
			//
			// LogViewer
			//
			this.LogViewer.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LogViewer, "LogViewer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ServiceManager.Business.ServiceTaskLogViewer)(((Enterprise.ServiceManager.Business.StmServiceTask)(null)).LogViewer)));
			this.LogViewer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LogViewer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.LogViewer.Name = "LogViewer";
			this.LogViewer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(660, 280, true);
			this.LogViewer.TabIndex = 0;
			// 
			// ExtendedConfigTabPage
			//
			this.ExtendedConfigTabPage.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("1e934153-4272-43f6-b276-a2f300f81812", "Extended Configuration");
			this.ExtendedConfigTabPage.Controls.Add(this.ExtendedConfigControl);
			this.ExtendedConfigTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ExtendedConfigTabPage.Name = "ExtendedConfigTabPage";
			this.ExtendedConfigTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ExtendedConfigTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(835, 307, true);
			this.ExtendedConfigTabPage.TabIndex = 4;
			this.ExtendedConfigTabPage.UseVisualStyleBackColor = true;
			// 
			// ExtendedConfigControl
			// 
			this.ExtendedConfigControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExtendedConfigControl, ".");
			this.ExtendedConfigControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ExtendedConfigControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ExtendedConfigControl.Name = "ExtendedConfigControl";
			this.ExtendedConfigControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(660, 301, true);
			this.ExtendedConfigControl.TabIndex = 0;
			//
			// ConfigTabPage
			//
			this.ConfigTabPage.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("0D9D3002-049A-4013-9A76-2F5C91A1A6B9", "Configuration");
			this.ConfigTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ConfigTabPage.Name = "ConfigTabPage";
			this.ConfigTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ConfigTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(666, 307, true);
			this.ConfigTabPage.TabIndex = 5;
			this.ConfigTabPage.UseVisualStyleBackColor = true;
			// 
			// StmServiceTaskForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(953, 655, true);
			this.DataSourceType = typeof(Enterprise.ServiceManager.Business.StmServiceTask);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(953, 655, true);
			this.CaptionResourceString = GetFormCaption();
			this.Name = "StmServiceTaskForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(false);
			this.NotesTabPage.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.SaveButtonUserControl.ResumeLayout(true);
			this.SaveButtonUserControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TopPanel.ResumeLayout(false);
			this.TopPanel.PerformLayout();
			this.DefaultScheduleControl.ResumeLayout(true);
			this.DefaultScheduleControl.PerformLayout();
			this.RecurrenceControl.ResumeLayout(true);
			this.RecurrenceControl.PerformLayout();
			this.DescriptionPanel.ResumeLayout(false);
			this.DescriptionPanel.PerformLayout();
			this.NextRunTimeDateEdit.ResumeLayout(true);
			this.NextRunTimeDateEdit.PerformLayout();
			this.BranchFindBox.ResumeLayout(true);
			this.BranchFindBox.PerformLayout();
			this.LocalNextRunTimeDateEdit.ResumeLayout(true);
			this.LocalNextRunTimeDateEdit.PerformLayout();
			this.LogViewer.ResumeLayout(true);
			this.LogViewer.PerformLayout();
			this.LogFilesTabPage.ResumeLayout(false);
			this.LogFilesTabPage.PerformLayout();
			this.ExtendedConfigTabPage.ResumeLayout(false);
			this.ExtendedConfigTabPage.PerformLayout();
			this.ExtendedConfigTabPage.ResumeLayout(true);
			this.ExtendedConfigControl.PerformLayout();
			this.NextRunTimeEstimatorControl.ResumeLayout(true);
			this.NextRunTimeEstimatorControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private ZPanel TopPanel;
		private ZPanel DescriptionPanel;
		private StmServiceTaskRecurrenceControl RecurrenceControl;
		private StmServiceTaskDefaultScheduleControl DefaultScheduleControl;
		private ZTextBox TaskDescriptionTextBox;
		private ZDateEdit NextRunTimeDateEdit;
		private ZDateEdit LocalNextRunTimeDateEdit;
		private ZGuidFindBox BranchFindBox;
		private ZCheckBox IsActiveCheckBox;
		private ZTabPage LogFilesTabPage;
		private LogViewerControl LogViewer;
		private ZTabPage ConfigTabPage;
		private ZTabPage ExtendedConfigTabPage;
		private StmServiceTaskExtendedConfigurationControl ExtendedConfigControl;
		private NextRunTimeEstimatorControl NextRunTimeEstimatorControl;

		#endregion
	}
}
