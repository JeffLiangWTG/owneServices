namespace Enterprise.BufferManagement.GUI
{
	partial class WorkflowManagementUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.MainSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.WorkflowsControl = new Enterprise.BufferManagement.GUI.WorkflowsUserControl();
			this.WorkflowTasksAndNotesTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.WorkflowTasksTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.TaskDetailsControl = new Enterprise.BufferManagement.GUI.TaskDetailsUserControl();
			this.WorkflowNotesTabPage = new Enterprise.BufferManagement.GUI.WorkflowNotesTabPage();
			this.WorkflowNotesControl = new Enterprise.BufferManagement.GUI.WorkflowNotesUserControl();
			this.WorkflowDetailsControl = new Enterprise.BufferManagement.GUI.WorkflowDetailsUserControl();
			this.ApprovedShapeDetailsControl = new Enterprise.BufferManagement.GUI.ApprovedShapeDetailsUserControl();
			this.WorkflowDetailsAndSchedulePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.WorkflowDetailsAndScheduleTablePanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).BeginInit();
			this.MainSplitContainer.Panel1.SuspendLayout();
			this.MainSplitContainer.Panel2.SuspendLayout();
			this.MainSplitContainer.SuspendLayout();
			this.WorkflowDetailsAndSchedulePanel.SuspendLayout();
			this.WorkflowDetailsAndScheduleTablePanel.SuspendLayout();
			this.WorkflowsControl.SuspendLayout();
			this.WorkflowTasksAndNotesTabControl.SuspendLayout();
			this.WorkflowTasksTabPage.SuspendLayout();
			this.TaskDetailsControl.SuspendLayout();
			this.WorkflowNotesTabPage.SuspendLayout();
			this.WorkflowNotesControl.SuspendLayout();
			this.WorkflowDetailsControl.SuspendLayout();
			this.ApprovedShapeDetailsControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.BufferManagement.Business.WorkflowManagementViewModel);
			// 
			// MainSplitContainer
			// 
			this.MainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainSplitContainer.Name = "MainSplitContainer";
			this.MainSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// MainSplitContainer.Panel1
			// 
			this.MainSplitContainer.Panel1.Controls.Add(this.WorkflowsControl);
			// 
			// MainSplitContainer.Panel2
			// 
			this.MainSplitContainer.Panel2.Controls.Add(this.WorkflowTasksAndNotesTabControl);
			this.MainSplitContainer.Panel2.Controls.Add(this.WorkflowDetailsAndSchedulePanel);
			this.MainSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 820, true);
			this.MainSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(215);
			this.MainSplitContainer.TabIndex = 5;
			// 
			// WorkflowDetailsAndSchedulePanel
			// 
			this.WorkflowDetailsAndSchedulePanel.AutoScroll = true;
			this.WorkflowDetailsAndSchedulePanel.Dock = System.Windows.Forms.DockStyle.Left;
			this.WorkflowDetailsAndSchedulePanel.Controls.Add(this.WorkflowDetailsAndScheduleTablePanel);
			this.WorkflowDetailsAndSchedulePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.WorkflowDetailsAndSchedulePanel.Name = "WorkflowDetailsAndSchedulePanel";
			this.WorkflowDetailsAndSchedulePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(395, 601, true);
			this.WorkflowDetailsAndSchedulePanel.TabIndex = 1;
			// 
			// WorkflowDetailsAndScheduleTablePanel
			// 
			this.WorkflowDetailsAndScheduleTablePanel.AutoSize = true;
			this.WorkflowDetailsAndScheduleTablePanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.WorkflowDetailsAndScheduleTablePanel.ColumnCount = 1;
			this.WorkflowDetailsAndScheduleTablePanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.WorkflowDetailsAndScheduleTablePanel.Controls.Add(this.WorkflowDetailsControl, 0, 0);
			this.WorkflowDetailsAndScheduleTablePanel.Controls.Add(this.ApprovedShapeDetailsControl, 0, 1);
			this.WorkflowDetailsAndScheduleTablePanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.WorkflowDetailsAndScheduleTablePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.WorkflowDetailsAndScheduleTablePanel.Name = "WorkflowDetailsAndScheduleTablePanel";
			this.WorkflowDetailsAndScheduleTablePanel.RowCount = 2;
			this.WorkflowDetailsAndScheduleTablePanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.WorkflowDetailsAndScheduleTablePanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.WorkflowDetailsAndScheduleTablePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(378, 686, true);
			this.WorkflowDetailsAndScheduleTablePanel.TabIndex = 2;
			//
			// WorkflowsControl
			// 
			this.WorkflowsControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.WorkflowsControl, ".");
			this.WorkflowsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.WorkflowsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.WorkflowsControl.Name = "WorkflowsControl";
			this.WorkflowsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 215, true);
			this.WorkflowsControl.TabIndex = 0;
			// 
			// WorkflowTasksAndNotesTabControl
			// 
			this.WorkflowTasksAndNotesTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.WorkflowTasksAndNotesTabControl.Controls.Add(this.WorkflowTasksTabPage);
			this.WorkflowTasksAndNotesTabControl.Controls.Add(this.WorkflowNotesTabPage);
			this.WorkflowTasksAndNotesTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.WorkflowTasksAndNotesTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(377, 0, true);
			this.WorkflowTasksAndNotesTabControl.Name = "WorkflowTasksAndNotesTabControl";
			this.WorkflowTasksAndNotesTabControl.SelectedIndex = 0;
			this.WorkflowTasksAndNotesTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(623, 601, true);
			this.WorkflowTasksAndNotesTabControl.TabIndex = 3;
			// 
			// WorkflowTasksTabPage
			// 
			this.WorkflowTasksTabPage.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("bdaddf6b-82c8-4f0c-9561-c684c164e288", "Tasks");
			this.WorkflowTasksTabPage.Controls.Add(this.TaskDetailsControl);
			this.WorkflowTasksTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.WorkflowTasksTabPage.Name = "WorkflowTasksTabPage";
			this.WorkflowTasksTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.WorkflowTasksTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(615, 565, true);
			this.WorkflowTasksTabPage.TabIndex = 0;
			this.WorkflowTasksTabPage.UseVisualStyleBackColor = true;
			// 
			// TaskDetailsControl
			// 
			this.TaskDetailsControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TaskDetailsControl, "AllProcessHeaders");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.BufferManagement.Business.ProcessHeader)(((Enterprise.BufferManagement.Business.ProcessHeader)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.WorkflowManagementViewModel)(null)).AllProcessHeaders)).SyncRoot)))));
			this.TaskDetailsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TaskDetailsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.TaskDetailsControl.Name = "TaskDetailsControl";
			this.TaskDetailsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(609, 512, true);
			this.TaskDetailsControl.TabIndex = 2;
			// 
			// WorkflowNotesTabPage
			// 
			this.WorkflowNotesTabPage.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("5ac6e095-e161-47a8-ab95-ab7dd54c5a11", "Workflow Notes");
			this.WorkflowNotesTabPage.Controls.Add(this.WorkflowNotesControl);
			this.WorkflowNotesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.WorkflowNotesTabPage.Name = "WorkflowNotesTabPage";
			this.WorkflowNotesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.WorkflowNotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(615, 574, true);
			this.WorkflowNotesTabPage.TabIndex = 1;
			this.WorkflowNotesTabPage.UseVisualStyleBackColor = true;
			// 
			// WorkflowNotesControl
			// 
			this.WorkflowNotesControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.WorkflowNotesControl, "AllProcessHeaders");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.BufferManagement.Business.ProcessHeader)(((Enterprise.BufferManagement.Business.ProcessHeader)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.WorkflowManagementViewModel)(null)).AllProcessHeaders)).SyncRoot)))));
			this.WorkflowNotesControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.WorkflowNotesControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.WorkflowNotesControl.Name = "WorkflowNotesControl";
			this.WorkflowNotesControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(609, 568, true);
			this.WorkflowNotesControl.TabIndex = 0;
			this.WorkflowNotesControl.CaptionRenderingEnabled = true;
			// 
			// WorkflowDetailsControl
			// 
			this.WorkflowDetailsControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.WorkflowDetailsControl, "AllProcessHeaders");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.BufferManagement.Business.ProcessHeader)(((Enterprise.BufferManagement.Business.ProcessHeader)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.WorkflowManagementViewModel)(null)).AllProcessHeaders)).SyncRoot)))));
			this.WorkflowDetailsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.WorkflowDetailsControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.WorkflowDetailsControl.Name = "WorkflowDetailsControl";
			this.WorkflowDetailsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(377, 526, true);
			this.WorkflowDetailsControl.TabIndex = 0;
			// 
			// ApprovedShapeDetailsControl
			// 
			this.ApprovedShapeDetailsControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ApprovedShapeDetailsControl, "AllProcessHeaders");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.BufferManagement.Business.ProcessHeader)(((Enterprise.BufferManagement.Business.ProcessHeader)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.WorkflowManagementViewModel)(null)).AllProcessHeaders)).SyncRoot)))));
			this.ApprovedShapeDetailsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 529, true);
			this.ApprovedShapeDetailsControl.Name = "ApprovedShapeDetailsControl";
			this.ApprovedShapeDetailsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(371, 154, true);
			this.ApprovedShapeDetailsControl.TabIndex = 1;
			// 
			// WorkflowManagementUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MainSplitContainer);
			this.Name = "WorkflowManagementUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 820, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainSplitContainer.Panel1.ResumeLayout(false);
			this.MainSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).EndInit();
			this.MainSplitContainer.ResumeLayout(false);
			this.MainSplitContainer.PerformLayout();
			this.WorkflowsControl.ResumeLayout(true);
			this.WorkflowsControl.PerformLayout();
			this.WorkflowTasksAndNotesTabControl.ResumeLayout(false);
			this.WorkflowTasksAndNotesTabControl.PerformLayout();
			this.WorkflowTasksTabPage.ResumeLayout(false);
			this.WorkflowTasksTabPage.PerformLayout();
			this.TaskDetailsControl.ResumeLayout(true);
			this.TaskDetailsControl.PerformLayout();
			this.WorkflowNotesTabPage.ResumeLayout(false);
			this.WorkflowNotesTabPage.PerformLayout();
			this.WorkflowNotesControl.ResumeLayout(true);
			this.WorkflowNotesControl.PerformLayout();
			this.WorkflowDetailsAndSchedulePanel.ResumeLayout(false);
			this.WorkflowDetailsAndSchedulePanel.PerformLayout();
			this.WorkflowDetailsAndScheduleTablePanel.ResumeLayout(false);
			this.WorkflowDetailsControl.ResumeLayout(true);
			this.WorkflowDetailsControl.PerformLayout();
			this.ApprovedShapeDetailsControl.ResumeLayout(true);
			this.ApprovedShapeDetailsControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer MainSplitContainer;
		public WorkflowDetailsUserControl WorkflowDetailsControl;
		private WorkflowsUserControl WorkflowsControl;
		public TaskDetailsUserControl TaskDetailsControl;
		public ApprovedShapeDetailsUserControl ApprovedShapeDetailsControl;
		private ZArchitecture.GUI.ZTabControl WorkflowTasksAndNotesTabControl;
		public ZArchitecture.GUI.ZTabPage WorkflowTasksTabPage;
		private Enterprise.BufferManagement.GUI.WorkflowNotesTabPage WorkflowNotesTabPage;
		private WorkflowNotesUserControl WorkflowNotesControl;
		private System.ComponentModel.IContainer components;
		public ZArchitecture.GUI.ZPanel WorkflowDetailsAndSchedulePanel;
		private CargoWise.Windows.UI.KTableLayoutPanel WorkflowDetailsAndScheduleTablePanel;
	}
}
