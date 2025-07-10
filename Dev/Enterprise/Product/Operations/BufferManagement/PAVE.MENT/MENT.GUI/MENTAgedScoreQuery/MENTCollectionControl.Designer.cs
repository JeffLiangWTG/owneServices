namespace Enterprise.PAVE.MENT.GUI
{
	partial class MENTCollectionControl
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
		void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.zTabControl1 = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.detailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MentActiveCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.dataCollectionDetailsControl1 = new Enterprise.PAVE.MENT.GUI.DataCollectionDetailsControl();
			this.scheduleTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.scheduleTaskRecurrenceControl1 = new Enterprise.BufferManagement.GUI.ScheduleTaskRecurrenceControl();
			this.visualisationTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.visualisationConfigurationControl1 = new Enterprise.PAVE.MENT.GUI.VisualisationConfigurationControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zTabControl1.SuspendLayout();
			this.detailsTabPage.SuspendLayout();
			this.dataCollectionDetailsControl1.SuspendLayout();
			this.scheduleTabPage.SuspendLayout();
			this.scheduleTaskRecurrenceControl1.SuspendLayout();
			this.visualisationTabPage.SuspendLayout();
			this.visualisationConfigurationControl1.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.PAVE.MENT.Business.MENTAcceptabilityBandViewModel);
			// 
			// zTabControl1
			// 
			this.zTabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.zTabControl1.Controls.Add(this.detailsTabPage);
			this.zTabControl1.Controls.Add(this.scheduleTabPage);
			this.zTabControl1.Controls.Add(this.visualisationTabPage);
			this.zTabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zTabControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zTabControl1.Name = "zTabControl1";
			this.zTabControl1.SelectedIndex = 0;
			this.zTabControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(769, 692, true);
			this.zTabControl1.TabIndex = 0;
			// 
			// detailsTabPage
			// 
			this.detailsTabPage.CaptionResourceString = Enterprise.PAVE.MENT.GUI.Res.GetData("916e5814-39dd-4535-b953-ec886765b585", "Details");
			this.detailsTabPage.Controls.Add(this.MentActiveCheckBox);
			this.detailsTabPage.Controls.Add(this.dataCollectionDetailsControl1);
			this.detailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.detailsTabPage.Name = "detailsTabPage";
			this.detailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.detailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(761, 665, true);
			this.detailsTabPage.TabIndex = 0;
			this.detailsTabPage.UseVisualStyleBackColor = true;
			// 
			// MentActiveCheckBox
			// 
			this.MentActiveCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.MentActiveCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.MentActiveCheckBox, "MentEnabled");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((Enterprise.PAVE.MENT.Business.MENTAcceptabilityBandViewModel)(null)).MentEnabled)));
			this.MentActiveCheckBox.CaptionResourceString = Enterprise.PAVE.MENT.GUI.Res.GetData("0461bbbd-10d6-4ec0-9036-f0fce0c1a011", "MENT Enabled");
			this.MentActiveCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.MentActiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 6, true);
			this.MentActiveCheckBox.Name = "MentActiveCheckBox";
			this.MentActiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(99, 17, true);
			this.MentActiveCheckBox.TabIndex = 0;
			this.MentActiveCheckBox.UseVisualStyleBackColor = true;
			// 
			// dataCollectionDetailsControl1
			// 
			this.dataCollectionDetailsControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.dataCollectionDetailsControl1, "Query");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.PAVE.MENT.Business.MENTAgedScoreQuery)(((Enterprise.PAVE.MENT.Business.MENTAcceptabilityBandViewModel)(null)).Query)));
			this.dataCollectionDetailsControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 29, true);
			this.dataCollectionDetailsControl1.Name = "dataCollectionDetailsControl1";
			this.dataCollectionDetailsControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(749, 186, true);
			this.dataCollectionDetailsControl1.TabIndex = 1;
			// 
			// scheduleTabPage
			// 
			this.scheduleTabPage.CaptionResourceString = Enterprise.PAVE.MENT.GUI.Res.GetData("950bddf4-1a41-4eee-91b4-c633be748b9a", "Schedule");
			this.scheduleTabPage.Controls.Add(this.scheduleTaskRecurrenceControl1);
			this.scheduleTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.scheduleTabPage.Name = "scheduleTabPage";
			this.scheduleTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.scheduleTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(761, 665, true);
			this.scheduleTabPage.TabIndex = 1;
			this.scheduleTabPage.UseVisualStyleBackColor = true;
			// 
			// scheduleTaskRecurrenceControl1
			// 
			this.scheduleTaskRecurrenceControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.scheduleTaskRecurrenceControl1, "Query.QuerySchedule");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Scheduler.Business.StmScheduleTask)(((Enterprise.PAVE.MENT.Business.MENTAcceptabilityBandViewModel)(null)).Query.QuerySchedule)));
			this.scheduleTaskRecurrenceControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 6, true);
			this.scheduleTaskRecurrenceControl1.Name = "scheduleTaskRecurrenceControl1";
			this.scheduleTaskRecurrenceControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(710, 447, true);
			this.scheduleTaskRecurrenceControl1.TabIndex = 0;
			// 
			// visualisationTabPage
			// 
			this.visualisationTabPage.BackColor = System.Drawing.Color.Transparent;
			this.visualisationTabPage.CaptionResourceString = Enterprise.PAVE.MENT.GUI.Res.GetData("845cd7be-1eea-48d2-98dd-e42979386b3d", "Visualization");
			this.visualisationTabPage.Controls.Add(this.visualisationConfigurationControl1);
			this.visualisationTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.visualisationTabPage.Name = "visualisationTabPage";
			this.visualisationTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(761, 665, true);
			this.visualisationTabPage.TabIndex = 2;
			// 
			// visualisationConfigurationControl1
			// 
			this.visualisationConfigurationControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.visualisationConfigurationControl1, "Query");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.PAVE.MENT.Business.MENTAgedScoreQuery)(((Enterprise.PAVE.MENT.Business.MENTAcceptabilityBandViewModel)(null)).Query)));
			this.visualisationConfigurationControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.visualisationConfigurationControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.visualisationConfigurationControl1.Name = "visualisationConfigurationControl1";
			this.visualisationConfigurationControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(762, 665, true);
			this.visualisationConfigurationControl1.TabIndex = 0;
			// 
			// MENTCollectionControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.zTabControl1);
			this.Name = "MENTCollectionControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(769, 692, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zTabControl1.ResumeLayout(false);
			this.zTabControl1.PerformLayout();
			this.detailsTabPage.ResumeLayout(false);
			this.detailsTabPage.PerformLayout();
			this.dataCollectionDetailsControl1.ResumeLayout(true);
			this.dataCollectionDetailsControl1.PerformLayout();
			this.scheduleTabPage.ResumeLayout(false);
			this.scheduleTabPage.PerformLayout();
			this.scheduleTaskRecurrenceControl1.ResumeLayout(true);
			this.scheduleTaskRecurrenceControl1.PerformLayout();
			this.visualisationTabPage.ResumeLayout(false);
			this.visualisationTabPage.PerformLayout();
			this.visualisationConfigurationControl1.ResumeLayout(true);
			this.visualisationConfigurationControl1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZTabControl zTabControl1;
		private ZArchitecture.GUI.ZTabPage detailsTabPage;
		private ZArchitecture.GUI.ZTabPage scheduleTabPage;
		private ZArchitecture.GUI.ZTabPage visualisationTabPage;
		private Enterprise.BufferManagement.GUI.ScheduleTaskRecurrenceControl scheduleTaskRecurrenceControl1;
		private DataCollectionDetailsControl dataCollectionDetailsControl1;
		private VisualisationConfigurationControl visualisationConfigurationControl1;
		private ZArchitecture.GUI.ZCheckBox MentActiveCheckBox;
	}
}
