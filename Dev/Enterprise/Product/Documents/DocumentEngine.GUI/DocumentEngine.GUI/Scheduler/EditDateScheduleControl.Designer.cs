using System;

namespace Enterprise.DocumentEngine.GUI.Scheduler
{
	partial class EditDateScheduleControl
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
			if (disposing)
			{
				if (DateSchedule != null)
				{
					DateSchedule.PeriodInfo.ValueChanged -= new EventHandler(PeriodInfo_ValueChanged);
				}

				if ((components != null))
				{
					components.Dispose();
				}
			}

			UnBindEvents();

			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
            this.FlowLayoutPanel = new CargoWise.Windows.UI.KFlowLayoutPanel();
            this.DayPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.DayNumericUpDown = new Enterprise.ZArchitecture.GUI.ZNumericUpDown();
            this.DayScopeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.WeekPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.WeekNumericUpDown = new Enterprise.ZArchitecture.GUI.ZNumericUpDown();
            this.WeekDayDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.WeekScopeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.MonthPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.MonthDayNumericUpDown = new Enterprise.ZArchitecture.GUI.ZNumericUpDown();
            this.MonthNumericUpDown = new Enterprise.ZArchitecture.GUI.ZNumericUpDown();
            this.MonthLastDayCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.MonthScopeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.YearPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.YearDayNumericUpDown = new Enterprise.ZArchitecture.GUI.ZNumericUpDown();
            this.YearNumericUpDown = new Enterprise.ZArchitecture.GUI.ZNumericUpDown();
            this.YearLastDayCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.YearScopeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.HourMinutePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.MinuteNumericUpDown = new Enterprise.ZArchitecture.GUI.ZNumericUpDown();
            this.HourNumericUpDown = new Enterprise.ZArchitecture.GUI.ZNumericUpDown();
            this.HourMinuteScopeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.FlowLayoutPanel.SuspendLayout();
            this.DayPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DayNumericUpDown)).BeginInit();
            this.DayNumericUpDown.SuspendLayout();
            this.DayScopeDropEdit.SuspendLayout();
            this.WeekPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.WeekNumericUpDown)).BeginInit();
            this.WeekNumericUpDown.SuspendLayout();
            this.WeekDayDropEdit.SuspendLayout();
            this.WeekScopeDropEdit.SuspendLayout();
            this.MonthPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MonthDayNumericUpDown)).BeginInit();
            this.MonthDayNumericUpDown.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MonthNumericUpDown)).BeginInit();
            this.MonthNumericUpDown.SuspendLayout();
            this.MonthScopeDropEdit.SuspendLayout();
            this.YearPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.YearDayNumericUpDown)).BeginInit();
            this.YearDayNumericUpDown.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.YearNumericUpDown)).BeginInit();
            this.YearNumericUpDown.SuspendLayout();
            this.YearScopeDropEdit.SuspendLayout();
            this.HourMinutePanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MinuteNumericUpDown)).BeginInit();
            this.MinuteNumericUpDown.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.HourNumericUpDown)).BeginInit();
            this.HourNumericUpDown.SuspendLayout();
            this.HourMinuteScopeDropEdit.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.DocumentEngine.Scheduler.Business.DateSchedule);
            // 
            // FlowLayoutPanel
            // 
            this.FlowLayoutPanel.Controls.Add(this.DayPanel);
            this.FlowLayoutPanel.Controls.Add(this.WeekPanel);
            this.FlowLayoutPanel.Controls.Add(this.MonthPanel);
            this.FlowLayoutPanel.Controls.Add(this.YearPanel);
            this.FlowLayoutPanel.Controls.Add(this.HourMinutePanel);
            this.FlowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FlowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.FlowLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.FlowLayoutPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
            this.FlowLayoutPanel.Name = "FlowLayoutPanel";
            this.FlowLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(328, 280, true);
            this.FlowLayoutPanel.TabIndex = 0;
            // 
            // DayPanel
            // 
            this.DayPanel.Controls.Add(this.DayNumericUpDown);
            this.DayPanel.Controls.Add(this.DayScopeDropEdit);
            this.DayPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.DayPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
            this.DayPanel.Name = "DayPanel";
            this.DayPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 48, true);
            this.DayPanel.TabIndex = 0;
            // 
            // DayNumericUpDown
            // 
            this.BindingSource.SetBindingMember(this.DayNumericUpDown, "PeriodCount");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((decimal)(((Enterprise.DocumentEngine.Scheduler.Business.DateSchedule)(null)).PeriodCount)));
            this.DayNumericUpDown.BindTo = "PeriodCount";
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.DayNumericUpDown, false);
            this.DayNumericUpDown.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(251, 3, true);
            this.DayNumericUpDown.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.DayNumericUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.DayNumericUpDown.Name = "DayNumericUpDown";
            this.DayNumericUpDown.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(46, 17, true);
            this.DayNumericUpDown.TabIndex = 1;
            this.DayNumericUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.DayNumericUpDown.ValueChanged += new System.EventHandler(this.PeriodNumberUpDown_ValueChanged);
            // 
            // DayScopeDropEdit
            // 
            this.DayScopeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.DayScopeDropEdit, "PeriodScope");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.DocumentEngine.Scheduler.Business.DateSchedule)(null)).PeriodScope)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentEngine.Scheduler.Business.DateSchedule)(null)).Lookups.PeriodScopes)));
            this.DayScopeDropEdit.BindToList = "Lookups+PeriodScopes";
            this.DayScopeDropEdit.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("EditDateScheduleControl|bf1bed98-24a9-4b45-a00d-1ad732ea91fb", "Scope");
            this.DayScopeDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.DayScopeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 3, true);
            this.DayScopeDropEdit.Name = "DayScopeDropEdit";
            this.DayScopeDropEdit.PreBoundMaxLength = 8;
            this.DayScopeDropEdit.ShowDescriptionBox = false;
            this.DayScopeDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
            this.DayScopeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(131, 17, true);
            this.DayScopeDropEdit.TabIndex = 0;
            // 
            // WeekPanel
            // 
            this.WeekPanel.Controls.Add(this.WeekNumericUpDown);
            this.WeekPanel.Controls.Add(this.WeekDayDropEdit);
            this.WeekPanel.Controls.Add(this.WeekScopeDropEdit);
            this.WeekPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 48, true);
            this.WeekPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
            this.WeekPanel.Name = "WeekPanel";
            this.WeekPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 48, true);
            this.WeekPanel.TabIndex = 1;
            // 
            // WeekNumericUpDown
            // 
            this.BindingSource.SetBindingMember(this.WeekNumericUpDown, "PeriodCount");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((decimal)(((Enterprise.DocumentEngine.Scheduler.Business.DateSchedule)(null)).PeriodCount)));
            this.WeekNumericUpDown.BindTo = "PeriodCount";
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.WeekNumericUpDown, false);
            this.WeekNumericUpDown.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(251, 3, true);
            this.WeekNumericUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.WeekNumericUpDown.Name = "WeekNumericUpDown";
            this.WeekNumericUpDown.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(46, 17, true);
            this.WeekNumericUpDown.TabIndex = 2;
            this.WeekNumericUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.WeekNumericUpDown.ValueChanged += new System.EventHandler(this.PeriodNumberUpDown_ValueChanged);
            // 
            // WeekDayDropEdit
            // 
            this.WeekDayDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.WeekDayDropEdit, "DayName");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.DocumentEngine.Scheduler.Business.DateSchedule)(null)).DayName)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentEngine.Scheduler.Business.DateSchedule)(null)).Lookups.WeekDays)));
            this.WeekDayDropEdit.BindToList = "Lookups+WeekDays";
            this.WeekDayDropEdit.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("EditDateScheduleControl|e6c4fb98-5c10-4589-8bb5-2efd99ce23d3", "Day");
            this.WeekDayDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(47, 3, true);
            this.WeekDayDropEdit.Name = "WeekDayDropEdit";
            this.WeekDayDropEdit.PreBoundMaxLength = 3;
            this.WeekDayDropEdit.ShowDescriptionBox = false;
            this.WeekDayDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
            this.WeekDayDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(69, 17, true);
            this.WeekDayDropEdit.TabIndex = 0;
            this.WeekDayDropEdit.SelectedIndexChanged += new System.EventHandler(this.PeriodNumberUpDown_ValueChanged);
            // 
            // WeekScopeDropEdit
            // 
            this.WeekScopeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.WeekScopeDropEdit, "PeriodScope");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.DocumentEngine.Scheduler.Business.DateSchedule)(null)).PeriodScope)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentEngine.Scheduler.Business.DateSchedule)(null)).Lookups.PeriodScopes)));
            this.WeekScopeDropEdit.BindToList = "Lookups+PeriodScopes";
            this.WeekScopeDropEdit.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("EditDateScheduleControl|17bbbd45-4e49-41b1-a42e-09a84415509d", "of");
            this.WeekScopeDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.WeekScopeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 3, true);
            this.WeekScopeDropEdit.Name = "WeekScopeDropEdit";
            this.WeekScopeDropEdit.PreBoundMaxLength = 8;
            this.WeekScopeDropEdit.ShowDescriptionBox = false;
            this.WeekScopeDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
            this.WeekScopeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(131, 17, true);
            this.WeekScopeDropEdit.TabIndex = 1;
            // 
            // MonthPanel
            // 
            this.MonthPanel.Controls.Add(this.MonthDayNumericUpDown);
            this.MonthPanel.Controls.Add(this.MonthNumericUpDown);
            this.MonthPanel.Controls.Add(this.MonthLastDayCheckBox);
            this.MonthPanel.Controls.Add(this.MonthScopeDropEdit);
            this.MonthPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 96, true);
            this.MonthPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
            this.MonthPanel.Name = "MonthPanel";
            this.MonthPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 48, true);
            this.MonthPanel.TabIndex = 2;
            // 
            // MonthDayNumericUpDown
            // 
            this.BindingSource.SetBindingMember(this.MonthDayNumericUpDown, "DayNumber");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((decimal)(((Enterprise.DocumentEngine.Scheduler.Business.DateSchedule)(null)).DayNumber)));
            this.MonthDayNumericUpDown.BindTo = "DayNumber";
            this.MonthDayNumericUpDown.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("EditDateScheduleControl|1d086783-f5c8-4312-8e5c-a721afd16483", "Day");
            this.MonthDayNumericUpDown.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(47, 3, true);
            this.MonthDayNumericUpDown.Maximum = new decimal(new int[] {
            31,
            0,
            0,
            0});
            this.MonthDayNumericUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.MonthDayNumericUpDown.Name = "MonthDayNumericUpDown";
            this.MonthDayNumericUpDown.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(46, 17, true);
            this.MonthDayNumericUpDown.TabIndex = 0;
            this.MonthDayNumericUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.MonthDayNumericUpDown.ValueChanged += new System.EventHandler(this.DayNumberUpDown_ValueChanged);
            // 
            // MonthNumericUpDown
            // 
            this.BindingSource.SetBindingMember(this.MonthNumericUpDown, "PeriodCount");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((decimal)(((Enterprise.DocumentEngine.Scheduler.Business.DateSchedule)(null)).PeriodCount)));
            this.MonthNumericUpDown.BindTo = "PeriodCount";
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.MonthNumericUpDown, false);
            this.MonthNumericUpDown.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(251, 3, true);
            this.MonthNumericUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.MonthNumericUpDown.Name = "MonthNumericUpDown";
            this.MonthNumericUpDown.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(46, 17, true);
            this.MonthNumericUpDown.TabIndex = 2;
            this.MonthNumericUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.MonthNumericUpDown.ValueChanged += new System.EventHandler(this.PeriodNumberUpDown_ValueChanged);
            // 
            // MonthLastDayCheckBox
            // 
            this.MonthLastDayCheckBox.AutoSize = true;
            this.BindingSource.SetBindingMember(this.MonthLastDayCheckBox, "LastDay");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentEngine.Scheduler.Business.DateSchedule)(null)).LastDay)));
            this.MonthLastDayCheckBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("EditDateScheduleControl|91fb0037-229e-4c34-9418-69b6d067be97", "Last Day");
            this.MonthLastDayCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(47, 29, true);
            this.MonthLastDayCheckBox.Name = "MonthLastDayCheckBox";
            this.MonthLastDayCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(65, 16, true);
            this.MonthLastDayCheckBox.TabIndex = 3;
            // 
            // MonthScopeDropEdit
            // 
            this.MonthScopeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.MonthScopeDropEdit, "PeriodScope");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.DocumentEngine.Scheduler.Business.DateSchedule)(null)).PeriodScope)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentEngine.Scheduler.Business.DateSchedule)(null)).Lookups.PeriodScopes)));
            this.MonthScopeDropEdit.BindToList = "Lookups+PeriodScopes";
            this.MonthScopeDropEdit.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("EditDateScheduleControl|247efb4d-8134-437b-8d99-5a005d76b2da", "of");
            this.MonthScopeDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.MonthScopeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 3, true);
            this.MonthScopeDropEdit.Name = "MonthScopeDropEdit";
            this.MonthScopeDropEdit.PreBoundMaxLength = 8;
            this.MonthScopeDropEdit.ShowDescriptionBox = false;
            this.MonthScopeDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
            this.MonthScopeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(131, 17, true);
            this.MonthScopeDropEdit.TabIndex = 1;
            // 
            // YearPanel
            // 
            this.YearPanel.Controls.Add(this.YearDayNumericUpDown);
            this.YearPanel.Controls.Add(this.YearNumericUpDown);
            this.YearPanel.Controls.Add(this.YearLastDayCheckBox);
            this.YearPanel.Controls.Add(this.YearScopeDropEdit);
            this.YearPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 144, true);
            this.YearPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
            this.YearPanel.Name = "YearPanel";
            this.YearPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 48, true);
            this.YearPanel.TabIndex = 3;
            // 
            // YearDayNumericUpDown
            // 
            this.BindingSource.SetBindingMember(this.YearDayNumericUpDown, "DayNumber");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((decimal)(((Enterprise.DocumentEngine.Scheduler.Business.DateSchedule)(null)).DayNumber)));
            this.YearDayNumericUpDown.BindTo = "DayNumber";
            this.YearDayNumericUpDown.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("EditDateScheduleControl|061b0b39-c862-4065-adad-7aaaba7ec17a", "Day");
            this.YearDayNumericUpDown.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(47, 3, true);
            this.YearDayNumericUpDown.Maximum = new decimal(new int[] {
            366,
            0,
            0,
            0});
            this.YearDayNumericUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.YearDayNumericUpDown.Name = "YearDayNumericUpDown";
            this.YearDayNumericUpDown.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(46, 17, true);
            this.YearDayNumericUpDown.TabIndex = 0;
            this.YearDayNumericUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.YearDayNumericUpDown.ValueChanged += new System.EventHandler(this.DayNumberUpDown_ValueChanged);
            // 
            // YearNumericUpDown
            // 
            this.BindingSource.SetBindingMember(this.YearNumericUpDown, "PeriodCount");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((decimal)(((Enterprise.DocumentEngine.Scheduler.Business.DateSchedule)(null)).PeriodCount)));
            this.YearNumericUpDown.BindTo = "PeriodCount";
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.YearNumericUpDown, false);
            this.YearNumericUpDown.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(251, 3, true);
            this.YearNumericUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.YearNumericUpDown.Name = "YearNumericUpDown";
            this.YearNumericUpDown.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(46, 17, true);
            this.YearNumericUpDown.TabIndex = 2;
            this.YearNumericUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.YearNumericUpDown.ValueChanged += new System.EventHandler(this.PeriodNumberUpDown_ValueChanged);
            // 
            // YearLastDayCheckBox
            // 
            this.YearLastDayCheckBox.AutoSize = true;
            this.BindingSource.SetBindingMember(this.YearLastDayCheckBox, "LastDay");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentEngine.Scheduler.Business.DateSchedule)(null)).LastDay)));
            this.YearLastDayCheckBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("EditDateScheduleControl|2d869e95-937c-4a79-b5c6-7cc5584b0305", "Last Day");
            this.YearLastDayCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(47, 29, true);
            this.YearLastDayCheckBox.Name = "YearLastDayCheckBox";
            this.YearLastDayCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(65, 16, true);
            this.YearLastDayCheckBox.TabIndex = 3;
            // 
            // YearScopeDropEdit
            // 
            this.YearScopeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.YearScopeDropEdit, "PeriodScope");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.DocumentEngine.Scheduler.Business.DateSchedule)(null)).PeriodScope)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentEngine.Scheduler.Business.DateSchedule)(null)).Lookups.PeriodScopes)));
            this.YearScopeDropEdit.BindToList = "Lookups+PeriodScopes";
            this.YearScopeDropEdit.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("EditDateScheduleControl|22c55dee-a66b-4202-a08f-787da7f6f0e4", "of");
            this.YearScopeDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.YearScopeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 3, true);
            this.YearScopeDropEdit.Name = "YearScopeDropEdit";
            this.YearScopeDropEdit.PreBoundMaxLength = 8;
            this.YearScopeDropEdit.ShowDescriptionBox = false;
            this.YearScopeDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
            this.YearScopeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(131, 17, true);
            this.YearScopeDropEdit.TabIndex = 1;
            // 
            // HourMinutePanel
            // 
            this.HourMinutePanel.Controls.Add(this.MinuteNumericUpDown);
            this.HourMinutePanel.Controls.Add(this.HourNumericUpDown);
            this.HourMinutePanel.Controls.Add(this.HourMinuteScopeDropEdit);
            this.HourMinutePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 192, true);
            this.HourMinutePanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
            this.HourMinutePanel.Name = "HourMinutePanel";
            this.HourMinutePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 48, true);
            this.HourMinutePanel.TabIndex = 4;
            // 
            // MinuteNumericUpDown
            // 
            this.BindingSource.SetBindingMember(this.MinuteNumericUpDown, "MinuteOfHour");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((decimal)(((Enterprise.DocumentEngine.Scheduler.Business.DateSchedule)(null)).MinuteOfHour)));
            this.MinuteNumericUpDown.BindTo = "MinuteOfHour";
            this.MinuteNumericUpDown.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("EditDateScheduleControl|361d37ce-7074-43ce-8ee1-14424071909e", "Minute");
            this.MinuteNumericUpDown.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(251, 23, true);
            this.MinuteNumericUpDown.Maximum = new decimal(new int[] {
            59,
            0,
            0,
            0});
            this.MinuteNumericUpDown.Name = "MinuteNumericUpDown";
            this.MinuteNumericUpDown.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(46, 17, true);
            this.MinuteNumericUpDown.TabIndex = 2;
            this.MinuteNumericUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.MinuteNumericUpDown.ValueChanged += new System.EventHandler(this.MinuteNumericUpDown_ValueChanged);
            // 
            // HourNumericUpDown
            // 
            this.BindingSource.SetBindingMember(this.HourNumericUpDown, "Hour");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((decimal)(((Enterprise.DocumentEngine.Scheduler.Business.DateSchedule)(null)).Hour)));
            this.HourNumericUpDown.BindTo = "Hour";
            this.HourNumericUpDown.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("EditDateScheduleControl|fccc7016-9334-4a7c-923a-3bdc8912918e", "Hour");
            this.HourNumericUpDown.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(251, 2, true);
            this.HourNumericUpDown.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
            this.HourNumericUpDown.Name = "HourNumericUpDown";
            this.HourNumericUpDown.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(46, 17, true);
            this.HourNumericUpDown.TabIndex = 1;
            this.HourNumericUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.HourNumericUpDown.ValueChanged += new System.EventHandler(this.HourNumericUpDown_ValueChanged);
            // 
            // HourMinuteScopeDropEdit
            // 
            this.HourMinuteScopeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.HourMinuteScopeDropEdit, "PeriodScope");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.DocumentEngine.Scheduler.Business.DateSchedule)(null)).PeriodScope)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentEngine.Scheduler.Business.DateSchedule)(null)).Lookups.HourMinutePeriodScopes)));
            this.HourMinuteScopeDropEdit.BindToList = "Lookups+HourMinutePeriodScopes";
            this.HourMinuteScopeDropEdit.CaptionResourceString = this.DayScopeDropEdit.CaptionResourceString;
            this.HourMinuteScopeDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.HourMinuteScopeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(53, 2, true);
            this.HourMinuteScopeDropEdit.Name = "HourMinuteScopeDropEdit";
            this.HourMinuteScopeDropEdit.PreBoundMaxLength = 8;
            this.HourMinuteScopeDropEdit.ShowDescriptionBox = false;
            this.HourMinuteScopeDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
            this.HourMinuteScopeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(131, 17, true);
            this.HourMinuteScopeDropEdit.TabIndex = 0;
            // 
            // EditDateScheduleControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.FlowLayoutPanel);
            this.DoubleBuffered = true;
            this.Name = "EditDateScheduleControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(328, 280, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.FlowLayoutPanel.ResumeLayout(false);
            this.FlowLayoutPanel.PerformLayout();
            this.DayPanel.ResumeLayout(false);
            this.DayPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DayNumericUpDown)).EndInit();
            this.DayNumericUpDown.ResumeLayout(false);
            this.DayNumericUpDown.PerformLayout();
            this.DayScopeDropEdit.ResumeLayout(true);
            this.DayScopeDropEdit.PerformLayout();
            this.WeekPanel.ResumeLayout(false);
            this.WeekPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.WeekNumericUpDown)).EndInit();
            this.WeekNumericUpDown.ResumeLayout(false);
            this.WeekNumericUpDown.PerformLayout();
            this.WeekDayDropEdit.ResumeLayout(true);
            this.WeekDayDropEdit.PerformLayout();
            this.WeekScopeDropEdit.ResumeLayout(true);
            this.WeekScopeDropEdit.PerformLayout();
            this.MonthPanel.ResumeLayout(false);
            this.MonthPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MonthDayNumericUpDown)).EndInit();
            this.MonthDayNumericUpDown.ResumeLayout(false);
            this.MonthDayNumericUpDown.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MonthNumericUpDown)).EndInit();
            this.MonthNumericUpDown.ResumeLayout(false);
            this.MonthNumericUpDown.PerformLayout();
            this.MonthScopeDropEdit.ResumeLayout(true);
            this.MonthScopeDropEdit.PerformLayout();
            this.YearPanel.ResumeLayout(false);
            this.YearPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.YearDayNumericUpDown)).EndInit();
            this.YearDayNumericUpDown.ResumeLayout(false);
            this.YearDayNumericUpDown.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.YearNumericUpDown)).EndInit();
            this.YearNumericUpDown.ResumeLayout(false);
            this.YearNumericUpDown.PerformLayout();
            this.YearScopeDropEdit.ResumeLayout(true);
            this.YearScopeDropEdit.PerformLayout();
            this.HourMinutePanel.ResumeLayout(false);
            this.HourMinutePanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MinuteNumericUpDown)).EndInit();
            this.MinuteNumericUpDown.ResumeLayout(false);
            this.MinuteNumericUpDown.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.HourNumericUpDown)).EndInit();
            this.HourNumericUpDown.ResumeLayout(false);
            this.HourNumericUpDown.PerformLayout();
            this.HourMinuteScopeDropEdit.ResumeLayout(true);
            this.HourMinuteScopeDropEdit.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KFlowLayoutPanel FlowLayoutPanel;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit DayScopeDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit WeekDayDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit WeekScopeDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZCheckBox MonthLastDayCheckBox;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit MonthScopeDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZCheckBox YearLastDayCheckBox;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit YearScopeDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZPanel DayPanel;
		internal Enterprise.ZArchitecture.GUI.ZPanel WeekPanel;
		internal Enterprise.ZArchitecture.GUI.ZPanel MonthPanel;
		internal Enterprise.ZArchitecture.GUI.ZPanel YearPanel;
		private Enterprise.ZArchitecture.GUI.ZNumericUpDown DayNumericUpDown;
		private Enterprise.ZArchitecture.GUI.ZNumericUpDown WeekNumericUpDown;
		private Enterprise.ZArchitecture.GUI.ZNumericUpDown MonthNumericUpDown;
		private Enterprise.ZArchitecture.GUI.ZNumericUpDown YearNumericUpDown;
		internal Enterprise.ZArchitecture.GUI.ZNumericUpDown MonthDayNumericUpDown;
		internal Enterprise.ZArchitecture.GUI.ZNumericUpDown YearDayNumericUpDown;
		internal ZArchitecture.GUI.ZPanel HourMinutePanel;
		internal ZArchitecture.GUI.ZNumericUpDown HourNumericUpDown;
		internal ZArchitecture.GUI.ZDropEdit HourMinuteScopeDropEdit;
		internal ZArchitecture.GUI.ZNumericUpDown MinuteNumericUpDown;
	}
}
