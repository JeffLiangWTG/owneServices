namespace Enterprise.BufferManagement.GUI
{
	partial class TagRuleForm
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
		private new void InitializeComponent()
		{
			this.tagRuleControl1 = new Enterprise.BufferManagement.GUI.TagRuleControl();
			this.ScheduleTab = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.scheduleControl = new Enterprise.BufferManagement.GUI.ScheduleTaskRecurrenceControl();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.tagRuleControl1.SuspendLayout();
			this.ScheduleTab.SuspendLayout();
			this.scheduleControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.ScheduleTab);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(826, 630, true);
			this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.NotesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.ScheduleTab, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.tagRuleControl1);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(818, 582, true);
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(818, 582, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(818, 582, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(826, 630, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(826, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.BufferManagement.Business.TagRule);
			// 
			// tagRuleControl1
			// 
			this.tagRuleControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.tagRuleControl1, ".");
			this.tagRuleControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tagRuleControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.tagRuleControl1.Name = "tagRuleControl1";
			this.tagRuleControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(818, 582, true);
			this.tagRuleControl1.TabIndex = 0;
			// 
			// ScheduleTab
			// 
			this.ScheduleTab.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("6cfaacd4-6ffd-4e6e-856f-f62e72200969", "Schedule");
			this.ScheduleTab.Controls.Add(this.scheduleControl);
			this.ScheduleTab.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ScheduleTab.Name = "ScheduleTab";
			this.ScheduleTab.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ScheduleTab.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(818, 603, true);
			this.ScheduleTab.TabIndex = 4;
			this.ScheduleTab.UseVisualStyleBackColor = true;
			// 
			// scheduleControl
			// 
			this.scheduleControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.scheduleControl, "Schedule");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Scheduler.Business.StmScheduleTask)(((Enterprise.BufferManagement.Business.TagRule)(null)).Schedule)));
			this.scheduleControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.scheduleControl.IsCollectionActiveCheckboxVisible = false;
			this.scheduleControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.scheduleControl.Name = "scheduleControl";
			this.scheduleControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(812, 597, true);
			this.scheduleControl.TabIndex = 0;
			// 
			// TagRuleForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("d46ae487-da07-4815-a3b5-bddeaa3930ad", "Tag Rule");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(826, 686, true);
			this.DataSourceType = typeof(Enterprise.BufferManagement.Business.TagRule);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 725, true);
			this.Name = "TagRuleForm";
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
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.tagRuleControl1.ResumeLayout(true);
			this.tagRuleControl1.PerformLayout();
			this.ScheduleTab.ResumeLayout(false);
			this.ScheduleTab.PerformLayout();
			this.scheduleControl.ResumeLayout(true);
			this.scheduleControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ScheduleTaskRecurrenceControl scheduleControl;
		private TagRuleControl tagRuleControl1;
		private ZArchitecture.GUI.ZTabPage ScheduleTab;
	}
}
