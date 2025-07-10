using Enterprise.Registry.Business.eServices.HealthCheckSettings;

namespace Enterprise.Registry.GUI
{
	partial class NotificationFrequencyControl
	{
		Enterprise.ZArchitecture.GUI.ZDropEdit settingsDropEdit;
		Enterprise.ZArchitecture.GUI.ZNumericUpDown timeIntervalNumericUpDownEdit;

		void InitializeComponent()
		{
			this.settingsDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.timeIntervalNumericUpDownEdit = new Enterprise.ZArchitecture.GUI.ZNumericUpDown();

			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.settingsDropEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.timeIntervalNumericUpDownEdit)).BeginInit();
			this.timeIntervalNumericUpDownEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.eServices.HealthCheckSettings.NotificationFrequency);
			// 
			// settingsDropEdit
			// 
			this.settingsDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.settingsDropEdit, "Settings");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Registry.Business.eServices.HealthCheckSettings.NotificationFrequency)(null)).Settings)));
			this.settingsDropEdit.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("57086202-06fc-4208-a48a-85e13b76390e", "Settings");
			this.settingsDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(52, 12, true);
			this.settingsDropEdit.Name = "settingsDropEdit";
			this.settingsDropEdit.PreBoundMaxLength = 3;
			this.settingsDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(381, 17, true);
			this.settingsDropEdit.TabIndex = 0;
			// 
			// timeIntervalNumericUpDownEdit
			// 
			this.BindingSource.SetBindingMember(this.timeIntervalNumericUpDownEdit, "TimeInterval");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((decimal)(((Enterprise.Registry.Business.eServices.HealthCheckSettings.NotificationFrequency)(null)).TimeInterval)));
			this.timeIntervalNumericUpDownEdit.BindTo = "TimeInterval";
			this.timeIntervalNumericUpDownEdit.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("e58ff535-a5d4-4e93-aebe-b31ca9fe6eae", "Time Interval (in minutes)");
			this.timeIntervalNumericUpDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 50, true);
			this.timeIntervalNumericUpDownEdit.Maximum = new decimal(new int[] {
				NotificationFrequency.MAX_TIME_INTERVAL_COUNT,
				0,
				0,
				0});
			this.timeIntervalNumericUpDownEdit.Name = "timeIntervalNumericUpDownEdit";
			this.timeIntervalNumericUpDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 17, true);
			this.timeIntervalNumericUpDownEdit.TabIndex = 1;
			this.timeIntervalNumericUpDownEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;

			// 
			// NotificationFrequencyControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.timeIntervalNumericUpDownEdit);
			this.Controls.Add(this.settingsDropEdit);
			this.Name = "NotificationFrequencyControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(438, 103, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.settingsDropEdit.ResumeLayout(true);
			this.settingsDropEdit.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.timeIntervalNumericUpDownEdit)).EndInit();
			this.timeIntervalNumericUpDownEdit.ResumeLayout(false);
			this.timeIntervalNumericUpDownEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
