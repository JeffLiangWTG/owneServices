namespace Enterprise.DocumentEngine.GUI.Scheduler
{
	partial class DateScheduleForm
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
		private new void InitializeComponent()
		{
            this.PeriodGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.YearRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
            this.EditDateScheduleControl = new Enterprise.DocumentEngine.GUI.Scheduler.EditDateScheduleControl();
            this.WeekRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
            this.MonthRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
            this.TodayRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.HourMinuteRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.DescriptionLabel = new Enterprise.ZArchitecture.ZLabel();
            this.DescriptionGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.MainPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.PeriodGroupBox.SuspendLayout();
            this.EditDateScheduleControl.SuspendLayout();
            this.DescriptionGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainPanel
            // 
            this.MainPanel.Controls.Add(this.DescriptionGroupBox);
            this.MainPanel.Controls.Add(this.PeriodGroupBox);
            this.MainPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
            this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(358, 188, true);
            // 
            // MainStatusBar
            // 
            this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 218, true);
            this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(358, 24, true);
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.DocumentEngine.Scheduler.Business.DateSchedule);
            // 
            // PeriodGroupBox
            // 
            this.PeriodGroupBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DateScheduleForm|c4d58002-4ed6-4b71-88ba-18e1673e5e7e", "Calculate By");
            this.PeriodGroupBox.Controls.Add(this.HourMinuteRadioButton);
            this.PeriodGroupBox.Controls.Add(this.YearRadioButton);
            this.PeriodGroupBox.Controls.Add(this.EditDateScheduleControl);
            this.PeriodGroupBox.Controls.Add(this.WeekRadioButton);
            this.PeriodGroupBox.Controls.Add(this.MonthRadioButton);
            this.PeriodGroupBox.Controls.Add(this.TodayRadioButton);
            this.PeriodGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 12, true);
            this.PeriodGroupBox.Name = "PeriodGroupBox";
            this.PeriodGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(334, 96, true);
            this.PeriodGroupBox.TabIndex = 0;
            this.PeriodGroupBox.TabStop = false;
            // 
            // YearRadioButton
            // 
            this.YearRadioButton.AutoCheck = false;
            this.YearRadioButton.AutoSize = true;
            this.BindingSource.SetBindingMember(this.YearRadioButton, "ByYear");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentEngine.Scheduler.Business.DateSchedule)(null)).ByYear)));
            this.YearRadioButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DateScheduleForm|0f3bbe5e-7657-48ae-9ed9-b6c2aa956ee6", "Year");
            this.YearRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(178, 19, true);
            this.YearRadioButton.Name = "YearRadioButton";
            this.YearRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(45, 16, true);
            this.YearRadioButton.TabIndex = 3;
            this.YearRadioButton.UseVisualStyleBackColor = false;
            // 
            // EditDateScheduleControl
            // 
            this.EditDateScheduleControl.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.EditDateScheduleControl, ".");
            this.EditDateScheduleControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 42, true);
            this.EditDateScheduleControl.Name = "EditDateScheduleControl";
            this.EditDateScheduleControl.Period = null;
            this.EditDateScheduleControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(319, 48, true);
            this.EditDateScheduleControl.TabIndex = 4;
            // 
            // WeekRadioButton
            // 
            this.WeekRadioButton.AutoCheck = false;
            this.WeekRadioButton.AutoSize = true;
            this.BindingSource.SetBindingMember(this.WeekRadioButton, "ByWeek");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentEngine.Scheduler.Business.DateSchedule)(null)).ByWeek)));
            this.WeekRadioButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DateScheduleForm|a514bebc-30c2-4385-9499-913bfa82b9b2", "Week");
            this.WeekRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(67, 19, true);
            this.WeekRadioButton.Name = "WeekRadioButton";
            this.WeekRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 16, true);
            this.WeekRadioButton.TabIndex = 1;
            this.WeekRadioButton.UseVisualStyleBackColor = false;
            // 
            // MonthRadioButton
            // 
            this.MonthRadioButton.AutoCheck = false;
            this.MonthRadioButton.AutoSize = true;
            this.BindingSource.SetBindingMember(this.MonthRadioButton, "ByMonth");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentEngine.Scheduler.Business.DateSchedule)(null)).ByMonth)));
            this.MonthRadioButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DateScheduleForm|ab3702cb-909c-46c9-82b3-59f34339806d", "Month");
            this.MonthRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(121, 19, true);
            this.MonthRadioButton.Name = "MonthRadioButton";
            this.MonthRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 16, true);
            this.MonthRadioButton.TabIndex = 2;
            this.MonthRadioButton.UseVisualStyleBackColor = false;
            // 
            // TodayRadioButton
            // 
            this.TodayRadioButton.AutoCheck = false;
            this.TodayRadioButton.AutoSize = true;
            this.BindingSource.SetBindingMember(this.TodayRadioButton, "ByDay");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentEngine.Scheduler.Business.DateSchedule)(null)).ByDay)));
            this.TodayRadioButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DateScheduleForm|8c97c58a-babf-49cb-91e4-eac842eb5680", "Day");
            this.TodayRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 19, true);
            this.TodayRadioButton.Name = "TodayRadioButton";
            this.TodayRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(41, 16, true);
            this.TodayRadioButton.TabIndex = 0;
            this.TodayRadioButton.UseVisualStyleBackColor = false;
			// 
			// HourMinuteRadioButton
			// 
			this.HourMinuteRadioButton.AutoCheck = false;
			this.HourMinuteRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.HourMinuteRadioButton, "ByHourAndMinute");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentEngine.Scheduler.Business.DateSchedule)(null)).ByHourAndMinute)));
			this.HourMinuteRadioButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DateScheduleForm|52f85211-5d5d-4ecc-9aa0-826fb3294595", "Hour/Minute");
			this.HourMinuteRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(227, 20, true);
			this.HourMinuteRadioButton.Name = "HourMinuteRadioButton";
			this.HourMinuteRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 13, true);
			this.HourMinuteRadioButton.TabIndex = 5;
			this.HourMinuteRadioButton.UseVisualStyleBackColor = false;
			// 
			// DescriptionLabel
			// 
			this.BindingSource.SetBindingMember(this.DescriptionLabel, "Description");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.Scheduler.Business.DateSchedule)(null)).Description)));
            this.DescriptionLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DescriptionLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.DescriptionLabel, false);
            this.DescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
            this.DescriptionLabel.Name = "DescriptionLabel";
            this.DescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(330, 51, true);
            this.DescriptionLabel.TabIndex = 0;
            this.DescriptionLabel.Text = "Description";
            this.DescriptionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // DescriptionGroupBox
            // 
            this.DescriptionGroupBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DateScheduleForm|a466421a-6638-4918-a72b-d89ecaedd6e5", "Calculated Result");
            this.DescriptionGroupBox.Controls.Add(this.DescriptionLabel);
            this.DescriptionGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 114, true);
            this.DescriptionGroupBox.Name = "DescriptionGroupBox";
            this.DescriptionGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(334, 68, true);
            this.DescriptionGroupBox.TabIndex = 1;
            this.DescriptionGroupBox.TabStop = false;
            // 
            // DateScheduleForm
            // 
            this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(358, 242, true);
            this.DataSourceAssemblyName = "Enterprise.DocumentEngine";
            this.DataSourceType = typeof(Enterprise.DocumentEngine.Scheduler.Business.DateSchedule);
            this.DataSourceTypeName = "Enterprise.DocumentEngine.Scheduler.Business.DateSchedule";
            this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(364, 270, true);
            this.Name = "DateScheduleForm";
            this.MainPanel.ResumeLayout(false);
            this.MainPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.PeriodGroupBox.ResumeLayout(false);
            this.PeriodGroupBox.PerformLayout();
            this.EditDateScheduleControl.ResumeLayout(true);
            this.EditDateScheduleControl.PerformLayout();
            this.DescriptionGroupBox.ResumeLayout(false);
            this.DescriptionGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZRadioButton YearRadioButton;
		private Enterprise.ZArchitecture.GUI.ZRadioButton MonthRadioButton;
		private Enterprise.ZArchitecture.GUI.ZRadioButton WeekRadioButton;
		private Enterprise.ZArchitecture.GUI.ZRadioButton TodayRadioButton;
		private Enterprise.ZArchitecture.GUI.ZGroupBox PeriodGroupBox;
		private Enterprise.ZArchitecture.ZLabel DescriptionLabel;
		private Enterprise.ZArchitecture.GUI.ZGroupBox DescriptionGroupBox;
		private EditDateScheduleControl EditDateScheduleControl;
        internal ZArchitecture.GUI.ZRadioButton HourMinuteRadioButton;
    }
}
