using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ServiceManager.GUI
{
	partial class StmServiceTaskRecurrenceControl
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
			this.MainGroupBox = new ZGroupBox();
			this.SecondPanel = new ZPanel();
			this.EndTimeSecond = new ZDateEdit();
			this.StartTimeSecond = new ZDateEdit();
			this.PeriodLabelSeconds = new ZLabel();
			this.EverySecond = new ZCalcEdit();
			this.SecondsPanelLabel = new ZLabel();
			this.MinutePanel = new ZPanel();
			this.EndTimeMinute = new ZDateEdit();
			this.StartTimeMinute = new ZDateEdit();
			this.PeriodLabelMinutes = new ZLabel();
			this.EveryMinute = new ZCalcEdit();
			this.MinutesPanelLabel = new ZLabel();
			this.PeriodPanel = new ZPanel();
			this.SecondRadioButton = new ZRadioButton();
			this.MinuteRadioButton = new ZRadioButton();
			this.HourlyRadioButton = new ZRadioButton();
			this.YearlyRadioButton = new ZRadioButton();
			this.MonthlyRadioButton = new ZRadioButton();
			this.DailyRadioButton = new ZRadioButton();
			this.WeeklyRadioButton = new ZRadioButton();
			this.MonthlyPanel = new ZPanel();
			this.WeeklyNextRuntimeLocal = new ZTextBox();
			this.hourlyEndLocal = new ZTextBox();
			this.hourlyStartLocal = new ZTextBox();
			this.SecondlyEndLocal = new ZTextBox();
			this.SecondlyStartLocal = new ZTextBox();
			this.MinuteEndLocal = new ZTextBox();
			this.MinuteStartLocal = new ZTextBox();
			this.MonthlyNextRunTimeLocal = new ZTextBox();
			this.MonthlyRecurringStartTimeEdit = new ZDateEdit();
			this.MonthLastDay = new ZCheckBox();
			this.MonthlyWeekDay = new ZRadioButton();
			this.MonthlyNumWeek = new ZDropEdit();
			this.MonthlyNumMonth = new ZCalcEdit();
			this.MonthlyDate = new ZCalcEdit();
			this.zLabel28 = new ZLabel();
			this.MonthlyWeekDayDropEdit = new ZDropEdit();
			this.MonthlyDay = new ZRadioButton();
			this.zLabel30 = new ZLabel();
			this.HourlyPanel = new ZPanel();
			this.EndTimeHour = new ZDateEdit();
			this.StartTimeHour = new ZDateEdit();
			this.lblHours = new ZLabel();
			this.EveryHour = new ZCalcEdit();
			this.lblHourFrequency = new ZLabel();
			this.WeeklyPanel = new ZPanel();
			this.WeeklyRecurringStartTimeEdit = new ZDateEdit();
			this.SundayCheckBox = new ZCheckBox();
			this.SaturdayCheckBox = new ZCheckBox();
			this.FridayCheckBox = new ZCheckBox();
			this.ThursdayCheckBox = new ZCheckBox();
			this.WednesdayCheckBox = new ZCheckBox();
			this.TuesdayCheckBox = new ZCheckBox();
			this.MondayCheckBox = new ZCheckBox();
			this.zLabel22 = new ZLabel();
			this.RecurEveryWeekTextBox = new ZCalcEdit();
			this.DailyPanel = new ZPanel();
			this.DailyNextRunTimeLocal = new ZTextBox();
			this.DailyRecurringStartTimeEdit = new ZDateEdit();
			this.DailyDaysNumber = new ZCalcEdit();
			this.zLabel3 = new ZLabel();
			this.DailyWeekDay = new ZRadioButton();
			this.DailyDay = new ZRadioButton();
			this.YearlyPanel = new ZPanel();
			this.YearlyNextRunTimeLocal = new ZTextBox();
			this.YearlyRecurringStartTimeEdit = new ZDateEdit();
			this.YearlyMonth = new ZDropEdit();
			this.YearlyWeekDayMonth = new ZDropEdit();
			this.YearlyWeekDay = new ZRadioButton();
			this.YearlyWeekNum = new ZDropEdit();
			this.YearlyDay = new ZCalcEdit();
			this.YearlyWeekDayDropEdit = new ZDropEdit();
			this.YearlyEvery = new ZRadioButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MainGroupBox.SuspendLayout();
			this.PeriodPanel.SuspendLayout();
			this.WeeklyPanel.SuspendLayout();
			this.WeeklyRecurringStartTimeEdit.SuspendLayout();
			this.DailyPanel.SuspendLayout();
			this.DailyRecurringStartTimeEdit.SuspendLayout();
			this.YearlyPanel.SuspendLayout();
			this.YearlyRecurringStartTimeEdit.SuspendLayout();
			this.YearlyMonth.SuspendLayout();
			this.YearlyWeekDayMonth.SuspendLayout();
			this.YearlyWeekNum.SuspendLayout();
			this.YearlyWeekDayDropEdit.SuspendLayout();
			this.MonthlyPanel.SuspendLayout();
			this.MonthlyRecurringStartTimeEdit.SuspendLayout();
			this.MonthlyNumWeek.SuspendLayout();
			this.MonthlyWeekDayDropEdit.SuspendLayout();
			this.SecondPanel.SuspendLayout();
			this.EndTimeSecond.SuspendLayout();
			this.StartTimeSecond.SuspendLayout();
			this.MinutePanel.SuspendLayout();
			this.EndTimeMinute.SuspendLayout();
			this.StartTimeMinute.SuspendLayout();
			this.HourlyPanel.SuspendLayout();
			this.EndTimeHour.SuspendLayout();
			this.StartTimeHour.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ServiceManager.Business.StmServiceTask);
			// 
			// MainGroupBox
			//
			this.MainGroupBox.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("RecurrenceControl|dae82ed2-f065-4b0e-9c8a-8d86ebd164af", "Recurrence Pattern");
			this.MainGroupBox.Controls.Add(this.MonthlyPanel);
			this.MainGroupBox.Controls.Add(this.SecondPanel);
			this.MainGroupBox.Controls.Add(this.MinutePanel);
			this.MainGroupBox.Controls.Add(this.PeriodPanel);
			this.MainGroupBox.Controls.Add(this.HourlyPanel);
			this.MainGroupBox.Controls.Add(this.WeeklyPanel);
			this.MainGroupBox.Controls.Add(this.DailyPanel);
			this.MainGroupBox.Controls.Add(this.YearlyPanel);
			this.MainGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.MainGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainGroupBox.Name = "MainGroupBox";
			this.MainGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(652, 215);
			this.MainGroupBox.TabIndex = 26;
			this.MainGroupBox.TabStop = false;
			// 
			// SecondPanel
			// 
			this.SecondPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.SecondPanel.Controls.Add(this.SecondlyEndLocal);
			this.SecondPanel.Controls.Add(this.SecondlyStartLocal);
			this.SecondPanel.Controls.Add(this.EndTimeSecond);
			this.SecondPanel.Controls.Add(this.StartTimeSecond);
			this.SecondPanel.Controls.Add(this.PeriodLabelSeconds);
			this.SecondPanel.Controls.Add(this.EverySecond);
			this.SecondPanel.Controls.Add(this.SecondsPanelLabel);
			this.SecondPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(158, 19, true);
			this.SecondPanel.Name = "SecondPanel";
			this.SecondPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(485, 159, true);
			this.SecondPanel.TabIndex = 44;
			this.SecondPanel.Visible = false;
			// 
			// EndTimeSecond
			// 
			this.EndTimeSecond.AutoCompleteMonthThreshold = 1;
			this.EndTimeSecond.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.EndTimeSecond, "Recurrence+CalcDailyEndTimeUtc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.ServiceManager.Business.StmServiceTask)(null)).Recurrence.CalcDailyEndTimeUtc)));
			this.EndTimeSecond.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("RecurrenceControl|d8e83e6a-d87f-4020-adaf-50a2d382933b", "End time (UTC)");
			this.EndTimeSecond.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Time;
			this.EndTimeSecond.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(103, 90, true);
			this.EndTimeSecond.Name = "EndTimeSecond";
			this.EndTimeSecond.TabIndex = 42;
			// 
			// StartTimeSecond
			// 
			this.StartTimeSecond.AutoCompleteMonthThreshold = 1;
			this.StartTimeSecond.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.StartTimeSecond, "Recurrence+CalcDailyStartTimeUtc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.ServiceManager.Business.StmServiceTask)(null)).Recurrence.CalcDailyStartTimeUtc)));
			this.StartTimeSecond.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("RecurrenceControl|be72586a-e34f-483a-83c3-9e868db2a485", "Start time (UTC)");
			this.StartTimeSecond.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Time;
			this.StartTimeSecond.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 60, true);
			this.StartTimeSecond.Name = "StartTimeSecond";
			this.StartTimeSecond.TabIndex = 38;
			// 
			// PeriodLabelSeconds
			// 
			this.PeriodLabelSeconds.AutoSize = true;
			this.PeriodLabelSeconds.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("RecurrenceControl|d5b2b572-329f-4bdf-b20c-4e95958e6280", "second(s)");
			this.PeriodLabelSeconds.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(197, 32, true);
			this.PeriodLabelSeconds.Name = "PeriodLabelSeconds";
			this.PeriodLabelSeconds.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 13, true);
			this.PeriodLabelSeconds.TabIndex = 32;
			// 
			// EverySecond
			// 
			this.EverySecond.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.EverySecond, "Recurrence+Period");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.ServiceManager.Business.StmServiceTask)(null)).Recurrence.Period)));
			this.EverySecond.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("RecurrenceControl|5113e138-5402-4660-9a57-2b472dcf46cc", "Every");
			this.EverySecond.DecimalPlaces = 2;
			this.EverySecond.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 30, true);
			this.EverySecond.Name = "EverySecond";
			this.EverySecond.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(84, 20, true);
			this.EverySecond.TabIndex = 31;
			this.EverySecond.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// SecondsPanelLabel
			// 
			this.SecondsPanelLabel.AutoSize = true;
			this.SecondsPanelLabel.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("RecurrenceControl|60a4c086-caf2-4aca-be54-6ef9412459d6", "Second frequency");
			this.SecondsPanelLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 9, true);
			this.SecondsPanelLabel.Name = "SecondsPanelLabel";
			this.SecondsPanelLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 13, true);
			this.SecondsPanelLabel.TabIndex = 10;
			// 
			// MinutePanel
			// 
			this.MinutePanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.MinutePanel.Controls.Add(this.MinuteEndLocal);
			this.MinutePanel.Controls.Add(this.MinuteStartLocal);
			this.MinutePanel.Controls.Add(this.EndTimeMinute);
			this.MinutePanel.Controls.Add(this.StartTimeMinute);
			this.MinutePanel.Controls.Add(this.PeriodLabelMinutes);
			this.MinutePanel.Controls.Add(this.EveryMinute);
			this.MinutePanel.Controls.Add(this.MinutesPanelLabel);
			this.MinutePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(158, 19, true);
			this.MinutePanel.Name = "MinutePanel";
			this.MinutePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(485, 159, true);
			this.MinutePanel.TabIndex = 43;
			this.MinutePanel.Visible = false;
			// 
			// MinuteEndLocal
			// 
			this.MinuteEndLocal.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.MinuteEndLocal, "Recurrence+CalcDailyEndTimeLocalText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ServiceManager.Business.StmServiceTask)(null)).Recurrence.CalcDailyEndTimeLocalText)));
			this.MinuteEndLocal.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("RecurrenceControl|149AC1DB-517B-4DB9-AD0A-7FD97506611D", "(Local)");
			this.MinuteEndLocal.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(230, 92, true);
			this.MinuteEndLocal.Enabled = false;
			this.MinuteEndLocal.Name = "MinuteEndLocal";
			this.MinuteEndLocal.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(45, 15, true);
			this.MinuteEndLocal.TabIndex = 45;
			this.MinuteEndLocal.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.MinuteEndLocal.TrackDisposedAccess = true;
			// 
			// MinuteStartLocal
			// 
			this.MinuteStartLocal.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.MinuteStartLocal, "Recurrence+CalcDailyStartTimeLocalText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ServiceManager.Business.StmServiceTask)(null)).Recurrence.CalcDailyStartTimeLocalText)));
			this.MinuteStartLocal.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("RecurrenceControl|25EDD7BD-67F7-48CC-BADA-BBCD746D34C7", "(Local)");
			this.MinuteStartLocal.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(230, 64, true);
			this.MinuteStartLocal.Enabled = false;
			this.MinuteStartLocal.Name = "MinuteStartLocal";
			this.MinuteStartLocal.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(45, 15, true);
			this.MinuteStartLocal.TabIndex = 44;
			this.MinuteStartLocal.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.MinuteStartLocal.TrackDisposedAccess = true;
			// 
			// EndTimeMinute
			// 
			this.EndTimeMinute.AutoCompleteMonthThreshold = 1;
			this.EndTimeMinute.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.EndTimeMinute, "Recurrence+CalcDailyEndTimeUtc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.ServiceManager.Business.StmServiceTask)(null)).Recurrence.CalcDailyEndTimeUtc)));
			this.EndTimeMinute.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("RecurrenceControl|e99cad8b-8443-4d84-b1ca-e221d8af07b5", "End time (UTC)");
			this.EndTimeMinute.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Time;
			this.EndTimeMinute.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(103, 92, true);
			this.EndTimeMinute.Name = "EndTimeMinute";
			this.EndTimeMinute.TabIndex = 42;
			// 
			// StartTimeMinute
			// 
			this.StartTimeMinute.AllowDrop = true;
			this.StartTimeMinute.AutoCompleteMonthThreshold = 1;
			this.StartTimeMinute.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.StartTimeMinute, "Recurrence+CalcDailyStartTimeUtc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.ServiceManager.Business.StmServiceTask)(null)).Recurrence.CalcDailyStartTimeUtc)));
			this.StartTimeMinute.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Time;
			this.StartTimeMinute.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 62, true);
			this.StartTimeMinute.Name = "StartTimeMinute";
			this.StartTimeMinute.TabIndex = 38;
			this.StartTimeMinute.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("RecurrenceControl|0adc0471-c2d3-4f7a-b510-718accfdd590", "Start time (UTC)");
			// 
			// PeriodLabelMinutes
			// 
			this.PeriodLabelMinutes.AutoSize = true;
			this.PeriodLabelMinutes.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("RecurrenceControl|22f49c14-7529-4a02-aa26-0d3483c33fb9", "minute(s)");
			this.PeriodLabelMinutes.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(193, 33, true);
			this.PeriodLabelMinutes.Name = "PeriodLabelMinutes";
			this.PeriodLabelMinutes.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(49, 13, true);
			this.PeriodLabelMinutes.TabIndex = 32;
			// 
			// EveryMinute
			// 
			this.EveryMinute.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.EveryMinute, "Recurrence+Period");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.ServiceManager.Business.StmServiceTask)(null)).Recurrence.Period)));
			this.EveryMinute.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("RecurrenceControl|17355728-ea46-4b99-97c4-b8f6a426488b", "Every");
			this.EveryMinute.DecimalPlaces = 2;
			this.EveryMinute.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(103, 30, true);
			this.EveryMinute.Name = "EveryMinute";
			this.EveryMinute.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(84, 20, true);
			this.EveryMinute.TabIndex = 31;
			this.EveryMinute.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// MinutesPanelLabel
			// 
			this.MinutesPanelLabel.AutoSize = true;
			this.MinutesPanelLabel.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("RecurrenceControl|0b866e4d-9daf-4f5c-a6d5-60da4b5e9d87", "Minute frequency");
			this.MinutesPanelLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 9, true);
			this.MinutesPanelLabel.Name = "MinutesPanelLabel";
			this.MinutesPanelLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(89, 13, true);
			this.MinutesPanelLabel.TabIndex = 10;
			// 
			// PeriodPanel
			// 
			this.PeriodPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.PeriodPanel.Controls.Add(this.SecondRadioButton);
			this.PeriodPanel.Controls.Add(this.MinuteRadioButton);
			this.PeriodPanel.Controls.Add(this.HourlyRadioButton);
			this.PeriodPanel.Controls.Add(this.YearlyRadioButton);
			this.PeriodPanel.Controls.Add(this.MonthlyRadioButton);
			this.PeriodPanel.Controls.Add(this.DailyRadioButton);
			this.PeriodPanel.Controls.Add(this.WeeklyRadioButton);
			this.PeriodPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 19, true);
			this.PeriodPanel.Name = "PeriodPanel";
			this.PeriodPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 159, true);
			this.PeriodPanel.TabIndex = 17;
			// 
			// SecondRadioButton
			// 
			this.SecondRadioButton.AutoCheck = false;
			this.SecondRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.SecondRadioButton, "Recurrence+SecondsRange");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ServiceManager.Business.StmServiceTask)(null)).Recurrence.SecondsRange)));
			this.SecondRadioButton.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("RecurrenceControl|5c7e01e0-2c3a-48dc-8faa-9f5120b457ab", "Second");
			this.SecondRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SecondRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 8, true);
			this.SecondRadioButton.Name = "SecondRadioButton";
			this.SecondRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 17, true);
			this.SecondRadioButton.TabIndex = 0;
			this.SecondRadioButton.UseVisualStyleBackColor = false;
			// 
			// MinuteRadioButton
			// 
			this.MinuteRadioButton.AutoCheck = false;
			this.MinuteRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.MinuteRadioButton, "Recurrence+MinutesRange");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ServiceManager.Business.StmServiceTask)(null)).Recurrence.MinutesRange)));
			this.MinuteRadioButton.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("RecurrenceControl|d06e8b70-c663-4474-a130-1abbd7f91a58", "Minute");
			this.MinuteRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.MinuteRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 29, true);
			this.MinuteRadioButton.Name = "MinuteRadioButton";
			this.MinuteRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 17, true);
			this.MinuteRadioButton.TabIndex = 1;
			this.MinuteRadioButton.UseVisualStyleBackColor = false;
			// 
			// HourlyRadioButton
			// 
			this.HourlyRadioButton.AutoCheck = false;
			this.HourlyRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.HourlyRadioButton, "Recurrence+HoursRange");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ServiceManager.Business.StmServiceTask)(null)).Recurrence.HoursRange)));
			this.HourlyRadioButton.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("RecurrenceControl|eb593818-9832-43b3-93d3-e5aca6dcff2a", "Hourly");
			this.HourlyRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.HourlyRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 50, true);
			this.HourlyRadioButton.Name = "HourlyRadioButton";
			this.HourlyRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(55, 17, true);
			this.HourlyRadioButton.TabIndex = 2;
			this.HourlyRadioButton.UseVisualStyleBackColor = false;
			// 
			// YearlyRadioButton
			// 
			this.YearlyRadioButton.AutoCheck = false;
			this.YearlyRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.YearlyRadioButton, "Recurrence+YearsRange");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ServiceManager.Business.StmServiceTask)(null)).Recurrence.YearsRange)));
			this.YearlyRadioButton.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("RecurrenceControl|4db455c6-062c-4b14-8e50-9d3efc6780d6", "Yearly");
			this.YearlyRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.YearlyRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 134, true);
			this.YearlyRadioButton.Name = "YearlyRadioButton";
			this.YearlyRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(54, 17, true);
			this.YearlyRadioButton.TabIndex = 6;
			this.YearlyRadioButton.TabStop = true;
			this.YearlyRadioButton.UseVisualStyleBackColor = false;
			// 
			// MonthlyRadioButton
			// 
			this.MonthlyRadioButton.AutoCheck = false;
			this.MonthlyRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.MonthlyRadioButton, "Recurrence+MonthsRange");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ServiceManager.Business.StmServiceTask)(null)).Recurrence.MonthsRange)));
			this.MonthlyRadioButton.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("RecurrenceControl|f1e3ad6f-7d5e-424d-8368-c0ab0362e4f2", "Monthly");
			this.MonthlyRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.MonthlyRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 113, true);
			this.MonthlyRadioButton.Name = "MonthlyRadioButton";
			this.MonthlyRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 17, true);
			this.MonthlyRadioButton.TabIndex = 5;
			this.MonthlyRadioButton.TabStop = true;
			this.MonthlyRadioButton.UseVisualStyleBackColor = false;
			// 
			// DailyRadioButton
			// 
			this.DailyRadioButton.AutoCheck = false;
			this.DailyRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.DailyRadioButton, "Recurrence+DaysRange");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ServiceManager.Business.StmServiceTask)(null)).Recurrence.DaysRange)));
			this.DailyRadioButton.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("RecurrenceControl|eb773336-9626-4586-887c-e63ca85e3cb7", "Daily");
			this.DailyRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.DailyRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 71, true);
			this.DailyRadioButton.Name = "DailyRadioButton";
			this.DailyRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 17, true);
			this.DailyRadioButton.TabIndex = 3;
			this.DailyRadioButton.UseVisualStyleBackColor = false;
			// 
			// WeeklyRadioButton
			// 
			this.WeeklyRadioButton.AutoCheck = false;
			this.WeeklyRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.WeeklyRadioButton, "Recurrence+WeeksRange");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ServiceManager.Business.StmServiceTask)(null)).Recurrence.WeeksRange)));
			this.WeeklyRadioButton.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("RecurrenceControl|d979e86b-ffaa-4073-a7e7-beffbc11c1f0", "Weekly");
			this.WeeklyRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.WeeklyRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 92, true);
			this.WeeklyRadioButton.Name = "WeeklyRadioButton";
			this.WeeklyRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(61, 17, true);
			this.WeeklyRadioButton.TabIndex = 4;
			this.WeeklyRadioButton.TabStop = true;
			this.WeeklyRadioButton.UseVisualStyleBackColor = false;
			// 
			// WeeklyNextRuntimeLocal
			// 
			this.WeeklyNextRuntimeLocal.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.WeeklyNextRuntimeLocal, "Recurrence+RecurringStartTimeLocalText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ServiceManager.Business.StmServiceTask)(null)).Recurrence.RecurringStartTimeLocalText)));
			this.WeeklyNextRuntimeLocal.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("RecurrenceControl|5DC33217-AC9D-4833-9761-02F0B5F9D928", "(Local)");
			this.WeeklyNextRuntimeLocal.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(282, 82, true);
			this.WeeklyNextRuntimeLocal.Enabled = false;
			this.WeeklyNextRuntimeLocal.Name = "WeeklyNextRuntimeLocal";
			this.WeeklyNextRuntimeLocal.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(45, 15, true);
			this.WeeklyNextRuntimeLocal.TabIndex = 46;
			this.WeeklyNextRuntimeLocal.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.WeeklyNextRuntimeLocal.TrackDisposedAccess = true;
			// 
			// hourlyEndLocal
			// 
			this.hourlyEndLocal.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.hourlyEndLocal, "Recurrence+CalcDailyEndTimeLocalText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ServiceManager.Business.StmServiceTask)(null)).Recurrence.CalcDailyEndTimeLocalText)));
			this.hourlyEndLocal.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("RecurrenceControl|4BCB91D2-6FFD-47EA-8C1B-FE264978461B", "(Local)");
			this.hourlyEndLocal.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(224, 91, true);
			this.hourlyEndLocal.Enabled = false;
			this.hourlyEndLocal.Name = "hourlyEndLocal";
			this.hourlyEndLocal.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(45, 15, true);
			this.hourlyEndLocal.TabIndex = 46;
			this.hourlyEndLocal.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.hourlyEndLocal.TrackDisposedAccess = true;
			// 
			// hourlyStartLocal
			// 
			this.hourlyStartLocal.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.hourlyStartLocal, "Recurrence+CalcDailyStartTimeLocalText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ServiceManager.Business.StmServiceTask)(null)).Recurrence.CalcDailyStartTimeLocalText)));
			this.hourlyStartLocal.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("RecurrenceControl|34E0B07F-BFDE-4EB2-B5BC-26F14202E118", "(Local)");
			this.hourlyStartLocal.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(224, 62, true);
			this.hourlyStartLocal.Enabled = false;
			this.hourlyStartLocal.Name = "hourlyStartLocal";
			this.hourlyStartLocal.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(45, 15, true);
			this.hourlyStartLocal.TabIndex = 45;
			this.hourlyStartLocal.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.hourlyStartLocal.TrackDisposedAccess = true;
			// 
			// SecondlyEndLocal
			// 
			this.SecondlyEndLocal.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.SecondlyEndLocal, "Recurrence+CalcDailyEndTimeLocalText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ServiceManager.Business.StmServiceTask)(null)).Recurrence.CalcDailyEndTimeLocalText)));
			this.SecondlyEndLocal.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("RecurrenceControl|C75FF3B7-1AD4-40DF-A8D2-279989E9FCDD", "(Local)");
			this.SecondlyEndLocal.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(223, 90, true);
			this.SecondlyEndLocal.Enabled = false;
			this.SecondlyEndLocal.Name = "SecondlyEndLocal";
			this.SecondlyEndLocal.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(45, 15, true);
			this.SecondlyEndLocal.TabIndex = 44;
			this.SecondlyEndLocal.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.SecondlyEndLocal.TrackDisposedAccess = true;
			// 
			// SecondlyStartLocal
			// 
			this.SecondlyStartLocal.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.SecondlyStartLocal, "Recurrence+CalcDailyStartTimeLocalText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ServiceManager.Business.StmServiceTask)(null)).Recurrence.CalcDailyStartTimeLocalText)));
			this.SecondlyStartLocal.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("RecurrenceControl|CA5AC665-4520-4B7D-9EFA-D10E03A1CE36", "(Local)");
			this.SecondlyStartLocal.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(223, 60, true);
			this.SecondlyStartLocal.Enabled = false;
			this.SecondlyStartLocal.Name = "SecondlyStartLocal";
			this.SecondlyStartLocal.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(45, 15, true);
			this.SecondlyStartLocal.TabIndex = 43;
			this.SecondlyStartLocal.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.SecondlyStartLocal.TrackDisposedAccess = true;
			// 
			// MonthlyPanel
			// 
			this.MonthlyPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.MonthlyPanel.Controls.Add(this.MonthlyNextRunTimeLocal);
			this.MonthlyPanel.Controls.Add(this.MonthlyRecurringStartTimeEdit);
			this.MonthlyPanel.Controls.Add(this.MonthLastDay);
			this.MonthlyPanel.Controls.Add(this.MonthlyWeekDay);
			this.MonthlyPanel.Controls.Add(this.MonthlyNumWeek);
			this.MonthlyPanel.Controls.Add(this.MonthlyNumMonth);
			this.MonthlyPanel.Controls.Add(this.MonthlyDate);
			this.MonthlyPanel.Controls.Add(this.zLabel28);
			this.MonthlyPanel.Controls.Add(this.MonthlyWeekDayDropEdit);
			this.MonthlyPanel.Controls.Add(this.MonthlyDay);
			this.MonthlyPanel.Controls.Add(this.zLabel30);
			this.MonthlyPanel.ForeColor = System.Drawing.SystemColors.WindowText;
			this.MonthlyPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(158, 19, true);
			this.MonthlyPanel.Name = "MonthlyPanel";
			this.MonthlyPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(485, 159, true);
			this.MonthlyPanel.TabIndex = 20;
			this.MonthlyPanel.Visible = false;
			// 
			// MonthlyNextRunTime
			// 
			this.MonthlyNextRunTimeLocal.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.MonthlyNextRunTimeLocal, "Recurrence+RecurringStartTimeLocalText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ServiceManager.Business.StmServiceTask)(null)).Recurrence.RecurringStartTimeLocalText)));
			this.MonthlyNextRunTimeLocal.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("RecurrenceControl|102B38A3-FAB4-4486-B74D-C6BC21E55024", "(Local)");
			this.MonthlyNextRunTimeLocal.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(306, 60, true);
			this.MonthlyNextRunTimeLocal.Enabled = false;
			this.MonthlyNextRunTimeLocal.Name = "MonthlyNextRunTime";
			this.MonthlyNextRunTimeLocal.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(45, 15, true);
			this.MonthlyNextRunTimeLocal.TabIndex = 34;
			this.MonthlyNextRunTimeLocal.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.MonthlyNextRunTimeLocal.TrackDisposedAccess = true;
			// 
			// MonthlyRecurringStartTimeEdit
			// 
			this.MonthlyRecurringStartTimeEdit.AutoCompleteMonthThreshold = 1;
			this.MonthlyRecurringStartTimeEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.MonthlyRecurringStartTimeEdit, "Recurrence+RecurringStartTimeUtc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.ServiceManager.Business.StmServiceTask)(null)).Recurrence.RecurringStartTimeUtc)));
			this.MonthlyRecurringStartTimeEdit.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("RecurrenceControl|a37a8d52-2e60-409e-b015-1f86fde67116", "Scheduled Run Time (UTC)");
			this.MonthlyRecurringStartTimeEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Time;
			this.MonthlyRecurringStartTimeEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(188, 62, true);
			this.MonthlyRecurringStartTimeEdit.Name = "MonthlyRecurringStartTimeEdit";
			this.MonthlyRecurringStartTimeEdit.TabIndex = 33;
			// 
			// MonthLastDay
			// 
			this.MonthLastDay.AutoSize = true;
			this.BindingSource.SetBindingMember(this.MonthLastDay, "Recurrence+MonthsLastDay");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ServiceManager.Business.StmServiceTask)(null)).Recurrence.MonthsLastDay)));
			this.MonthLastDay.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("RecurrenceControl|c91f2999-2a5b-44e6-a236-3a9f22d249c6", "Last Day");
			this.MonthLastDay.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.MonthLastDay.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 11, true);
			this.MonthLastDay.Name = "MonthLastDay";
			this.MonthLastDay.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(68, 17, true);
			this.MonthLastDay.TabIndex = 28;
			// 
			// MonthlyWeekDay
			// 
			this.MonthlyWeekDay.AutoCheck = false;
			this.MonthlyWeekDay.AutoSize = true;
			this.MonthlyWeekDay.BackColor = System.Drawing.SystemColors.Info;
			this.BindingSource.SetBindingMember(this.MonthlyWeekDay, "Recurrence+MonthlyWeekDay");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ServiceManager.Business.StmServiceTask)(null)).Recurrence.MonthlyWeekDay)));
			this.MonthlyWeekDay.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("RecurrenceControl|d8628db2-a906-493d-9a53-be9be068c6f8", "The");
			this.MonthlyWeekDay.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.MonthlyWeekDay.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 39, true);
			this.MonthlyWeekDay.Name = "MonthlyWeekDay";
			this.MonthlyWeekDay.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(44, 17, true);
			this.MonthlyWeekDay.TabIndex = 21;
			this.MonthlyWeekDay.TabStop = true;
			this.MonthlyWeekDay.UseVisualStyleBackColor = false;
			// 
			// MonthlyNumWeek
			// 
			this.BindingSource.SetBindingMember(this.MonthlyNumWeek, "Recurrence+WeekCountAsString");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.ServiceManager.Business.StmServiceTask)(null)).Recurrence.WeekCountAsString)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.ServiceManager.Business.StmServiceTask)(null)).Recurrence.Lookups.WeekCounts)));
			this.MonthlyNumWeek.BindToList = "Lookups+WeekCounts";
			this.MonthlyNumWeek.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 36, true);
			this.MonthlyNumWeek.Name = "MonthlyNumWeek";
			this.MonthlyNumWeek.PreBoundMaxLength = 1;
			this.MonthlyNumWeek.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.MonthlyNumWeek.TabIndex = 26;
			// 
			// MonthlyNumMonth
			// 
			this.MonthlyNumMonth.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.MonthlyNumMonth, "Recurrence+Period");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.ServiceManager.Business.StmServiceTask)(null)).Recurrence.Period)));
			this.MonthlyNumMonth.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("RecurrenceControl|2f56579c-fe00-4f50-8ac6-0f22b16cdbff", "of every");
			this.MonthlyNumMonth.DecimalPlaces = 2;
			this.MonthlyNumMonth.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(318, 9, true);
			this.MonthlyNumMonth.Name = "MonthlyNumMonth";
			this.MonthlyNumMonth.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 20, true);
			this.MonthlyNumMonth.TabIndex = 25;
			this.MonthlyNumMonth.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// MonthlyDate
			// 
			this.MonthlyDate.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.MonthlyDate, "Recurrence+DayOfMonth");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.ServiceManager.Business.StmServiceTask)(null)).Recurrence.DayOfMonth)));
			this.MonthlyDate.DecimalPlaces = 2;
			this.MonthlyDate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 9, true);
			this.MonthlyDate.Name = "MonthlyDate";
			this.MonthlyDate.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(45, 20, true);
			this.MonthlyDate.TabIndex = 24;
			this.MonthlyDate.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zLabel28
			// 
			this.zLabel28.AutoSize = true;
			this.zLabel28.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("RecurrenceControl|b4c302e4-c255-4caa-b6a0-900cbcc67b05", "of every month(s)");
			this.zLabel28.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(283, 39, true);
			this.zLabel28.Name = "zLabel28";
			this.zLabel28.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 13, true);
			this.zLabel28.TabIndex = 22;
			// 
			// MonthlyWeekDayDropEdit
			// 
			this.BindingSource.SetBindingMember(this.MonthlyWeekDayDropEdit, "Recurrence+DayName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.ServiceManager.Business.StmServiceTask)(null)).Recurrence.DayName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.ServiceManager.Business.StmServiceTask)(null)).Recurrence.Lookups.WeekDays)));
			this.MonthlyWeekDayDropEdit.BindToList = "Recurrence+Lookups+WeekDays";
			this.MonthlyWeekDayDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(214, 36, true);
			this.MonthlyWeekDayDropEdit.Name = "MonthlyWeekDayDropEdit";
			this.MonthlyWeekDayDropEdit.PreBoundMaxLength = 3;
			this.MonthlyWeekDayDropEdit.ShowDescriptionBox = false;
			this.MonthlyWeekDayDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.MonthlyWeekDayDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.MonthlyWeekDayDropEdit.TabIndex = 27;
			// 
			// MonthlyDay
			// 
			this.MonthlyDay.AutoCheck = false;
			this.MonthlyDay.AutoSize = true;
			this.MonthlyDay.BackColor = System.Drawing.SystemColors.Info;
			this.BindingSource.SetBindingMember(this.MonthlyDay, "Recurrence+MonthlyDay");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ServiceManager.Business.StmServiceTask)(null)).Recurrence.MonthlyDay)));
			this.MonthlyDay.Checked = true;
			this.MonthlyDay.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("RecurrenceControl|a81ba6af-a6bb-41c0-a623-48806dd48e06", "Day");
			this.MonthlyDay.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.MonthlyDay.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 9, true);
			this.MonthlyDay.Name = "MonthlyDay";
			this.MonthlyDay.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(44, 17, true);
			this.MonthlyDay.TabIndex = 20;
			this.MonthlyDay.TabStop = true;
			this.MonthlyDay.UseVisualStyleBackColor = false;
			// 
			// zLabel30
			// 
			this.zLabel30.AutoSize = true;
			this.zLabel30.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("RecurrenceControl|77fec973-59c9-45b1-be10-cf118e4d8741", "month(s)");
			this.zLabel30.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(368, 12, true);
			this.zLabel30.Name = "zLabel30";
			this.zLabel30.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 13, true);
			this.zLabel30.TabIndex = 18;
			// 
			// HourlyPanel
			// 
			this.HourlyPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.HourlyPanel.Controls.Add(this.hourlyEndLocal);
			this.HourlyPanel.Controls.Add(this.hourlyStartLocal);
			this.HourlyPanel.Controls.Add(this.EndTimeHour);
			this.HourlyPanel.Controls.Add(this.StartTimeHour);
			this.HourlyPanel.Controls.Add(this.lblHours);
			this.HourlyPanel.Controls.Add(this.EveryHour);
			this.HourlyPanel.Controls.Add(this.lblHourFrequency);
			this.HourlyPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(158, 19, true);
			this.HourlyPanel.Name = "HourlyPanel";
			this.HourlyPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(485, 159, true);
			this.HourlyPanel.TabIndex = 44;
			this.HourlyPanel.Visible = false;
			// 
			// EndTimeHour
			// 
			this.EndTimeHour.AutoCompleteMonthThreshold = 1;
			this.EndTimeHour.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.EndTimeHour, "Recurrence+CalcDailyEndTimeUtc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.ServiceManager.Business.StmServiceTask)(null)).Recurrence.CalcDailyEndTimeUtc)));
			this.EndTimeHour.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("RecurrenceControl|3084a92f-9756-4952-904f-b97134c63ece", "End time (UTC)");
			this.EndTimeHour.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Time;
			this.EndTimeHour.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 92, true);
			this.EndTimeHour.Name = "EndTimeHour";
			this.EndTimeHour.TabIndex = 42;
			// 
			// StartTimeHour
			// 
			this.StartTimeHour.AutoCompleteMonthThreshold = 1;
			this.StartTimeHour.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.StartTimeHour, "Recurrence+CalcDailyStartTimeUtc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.ServiceManager.Business.StmServiceTask)(null)).Recurrence.CalcDailyStartTimeUtc)));
			this.StartTimeHour.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("RecurrenceControl|74e36a81-f088-478f-b961-3d9473d17ab1", "Start time (UTC)");
			this.StartTimeHour.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Time;
			this.StartTimeHour.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 62, true);
			this.StartTimeHour.Name = "StartTimeHour";
			this.StartTimeHour.TabIndex = 38;
			// 
			// lblHours
			// 
			this.lblHours.AutoSize = true;
			this.lblHours.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("RecurrenceControl|9c675dde-371a-46c2-8062-d40209e61c19", "hour(s)");
			this.lblHours.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(186, 33, true);
			this.lblHours.Name = "lblHours";
			this.lblHours.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(39, 13, true);
			this.lblHours.TabIndex = 32;
			// 
			// EveryHour
			// 
			this.EveryHour.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.EveryHour, "Recurrence+Period");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.ServiceManager.Business.StmServiceTask)(null)).Recurrence.Period)));
			this.EveryHour.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("RecurrenceControl|3757c85f-1864-4711-a701-d3d632c2b967", "Every");
			this.EveryHour.DecimalPlaces = 2;
			this.EveryHour.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 30, true);
			this.EveryHour.Name = "EveryHour";
			this.EveryHour.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(84, 20, true);
			this.EveryHour.TabIndex = 31;
			this.EveryHour.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// lblHourFrequency
			// 
			this.lblHourFrequency.AutoSize = true;
			this.lblHourFrequency.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("RecurrenceControl|e89fd5c8-7597-491e-8617-1f5d5311fc4c", "Hour frequency");
			this.lblHourFrequency.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 9, true);
			this.lblHourFrequency.Name = "lblHourFrequency";
			this.lblHourFrequency.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 13, true);
			this.lblHourFrequency.TabIndex = 10;
			// 
			// WeeklyPanel
			// 
			this.WeeklyPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.WeeklyPanel.Controls.Add(this.WeeklyNextRuntimeLocal);
			this.WeeklyPanel.Controls.Add(this.WeeklyRecurringStartTimeEdit);
			this.WeeklyPanel.Controls.Add(this.SundayCheckBox);
			this.WeeklyPanel.Controls.Add(this.SaturdayCheckBox);
			this.WeeklyPanel.Controls.Add(this.FridayCheckBox);
			this.WeeklyPanel.Controls.Add(this.ThursdayCheckBox);
			this.WeeklyPanel.Controls.Add(this.WednesdayCheckBox);
			this.WeeklyPanel.Controls.Add(this.TuesdayCheckBox);
			this.WeeklyPanel.Controls.Add(this.MondayCheckBox);
			this.WeeklyPanel.Controls.Add(this.zLabel22);
			this.WeeklyPanel.Controls.Add(this.RecurEveryWeekTextBox);
			this.WeeklyPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(158, 19, true);
			this.WeeklyPanel.Name = "WeeklyPanel";
			this.WeeklyPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(485, 159, true);
			this.WeeklyPanel.TabIndex = 21;
			this.WeeklyPanel.Visible = false;
			// 
			// WeeklyRecurringStartTimeEdit
			// 
			this.WeeklyRecurringStartTimeEdit.AutoCompleteMonthThreshold = 1;
			this.WeeklyRecurringStartTimeEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.WeeklyRecurringStartTimeEdit, "Recurrence+RecurringStartTimeUtc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.ServiceManager.Business.StmServiceTask)(null)).Recurrence.RecurringStartTimeUtc)));
			this.WeeklyRecurringStartTimeEdit.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("RecurrenceControl|D6F3B523-6EFF-4325-BDE6-94CE92B4AF1E", "Scheduled Run Time (UTC)");
			this.WeeklyRecurringStartTimeEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Time;
			this.WeeklyRecurringStartTimeEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(162, 82, true);
			this.WeeklyRecurringStartTimeEdit.Name = "WeeklyRecurringStartTimeEdit";
			this.WeeklyRecurringStartTimeEdit.TabIndex = 33;
			// 
			// SundayCheckBox
			// 
			this.SundayCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.SundayCheckBox, "Recurrence+Sunday");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ServiceManager.Business.StmServiceTask)(null)).Recurrence.Sunday)));
			this.SundayCheckBox.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("RecurrenceControl|5dc4bf29-c904-4b77-afd0-97b9d554c838", "Sunday");
			this.SundayCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SundayCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(227, 52, true);
			this.SundayCheckBox.Name = "SundayCheckBox";
			this.SundayCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 17, true);
			this.SundayCheckBox.TabIndex = 16;
			// 
			// SaturdayCheckBox
			// 
			this.SaturdayCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.SaturdayCheckBox, "Recurrence+Saturday");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ServiceManager.Business.StmServiceTask)(null)).Recurrence.Saturday)));
			this.SaturdayCheckBox.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("RecurrenceControl|d68be445-b1af-41d3-b956-11eb1f802bd3", "Saturday");
			this.SaturdayCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SaturdayCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 52, true);
			this.SaturdayCheckBox.Name = "SaturdayCheckBox";
			this.SaturdayCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(68, 17, true);
			this.SaturdayCheckBox.TabIndex = 15;
			// 
			// FridayCheckBox
			// 
			this.FridayCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.FridayCheckBox, "Recurrence+Friday");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ServiceManager.Business.StmServiceTask)(null)).Recurrence.Friday)));
			this.FridayCheckBox.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("RecurrenceControl|0047ecd3-97e7-4856-8f13-0be48214bc6a", "Friday");
			this.FridayCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.FridayCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 52, true);
			this.FridayCheckBox.Name = "FridayCheckBox";
			this.FridayCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(54, 17, true);
			this.FridayCheckBox.TabIndex = 14;
			// 
			// ThursdayCheckBox
			// 
			this.ThursdayCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ThursdayCheckBox, "Recurrence+Thursday");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ServiceManager.Business.StmServiceTask)(null)).Recurrence.Thursday)));
			this.ThursdayCheckBox.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("RecurrenceControl|777d2378-4730-4336-8118-f17ebe655eee", "Thursday");
			this.ThursdayCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ThursdayCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(357, 31, true);
			this.ThursdayCheckBox.Name = "ThursdayCheckBox";
			this.ThursdayCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 17, true);
			this.ThursdayCheckBox.TabIndex = 13;
			// 
			// WednesdayCheckBox
			// 
			this.WednesdayCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.WednesdayCheckBox, "Recurrence+Wednesday");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ServiceManager.Business.StmServiceTask)(null)).Recurrence.Wednesday)));
			this.WednesdayCheckBox.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("RecurrenceControl|d6e27f65-f40f-4874-9dde-8298f49393cc", "Wednesday");
			this.WednesdayCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.WednesdayCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(227, 31, true);
			this.WednesdayCheckBox.Name = "WednesdayCheckBox";
			this.WednesdayCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 17, true);
			this.WednesdayCheckBox.TabIndex = 12;
			// 
			// TuesdayCheckBox
			// 
			this.TuesdayCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.TuesdayCheckBox, "Recurrence+Tuesday");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ServiceManager.Business.StmServiceTask)(null)).Recurrence.Tuesday)));
			this.TuesdayCheckBox.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("RecurrenceControl|53fed761-f846-4f57-b5e2-8f80d3bde0e8", "Tuesday");
			this.TuesdayCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.TuesdayCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 31, true);
			this.TuesdayCheckBox.Name = "TuesdayCheckBox";
			this.TuesdayCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 17, true);
			this.TuesdayCheckBox.TabIndex = 11;
			// 
			// MondayCheckBox
			// 
			this.MondayCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.MondayCheckBox, "Recurrence+Monday");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ServiceManager.Business.StmServiceTask)(null)).Recurrence.Monday)));
			this.MondayCheckBox.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("RecurrenceControl|2ae65a16-ea29-4208-999b-59873587aecb", "Monday");
			this.MondayCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.MondayCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 32, true);
			this.MondayCheckBox.Name = "MondayCheckBox";
			this.MondayCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 17, true);
			this.MondayCheckBox.TabIndex = 10;
			// 
			// zLabel22
			// 
			this.zLabel22.AutoSize = true;
			this.zLabel22.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("RecurrenceControl|f2d230a0-c552-4f6a-8297-4a8d1a7b76d2", "week(s) on");
			this.zLabel22.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(166, 9, true);
			this.zLabel22.Name = "zLabel22";
			this.zLabel22.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(59, 13, true);
			this.zLabel22.TabIndex = 8;
			// 
			// RecurEveryWeekTextBox
			// 
			this.RecurEveryWeekTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.RecurEveryWeekTextBox, "Recurrence+Period");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.ServiceManager.Business.StmServiceTask)(null)).Recurrence.Period)));
			this.RecurEveryWeekTextBox.DecimalPlaces = 2;
			this.RecurEveryWeekTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 6, true);
			this.RecurEveryWeekTextBox.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("RecurrenceControl|50ec4d0f-8738-43e8-9391-328eded743aa", "Recur every");
			this.RecurEveryWeekTextBox.Name = "RecurEveryWeekTextBox";
			this.RecurEveryWeekTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(35, 20, true);
			this.RecurEveryWeekTextBox.TabIndex = 7;
			this.RecurEveryWeekTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// DailyPanel
			// 
			this.DailyPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.DailyPanel.Controls.Add(this.DailyNextRunTimeLocal);
			this.DailyPanel.Controls.Add(this.DailyRecurringStartTimeEdit);
			this.DailyPanel.Controls.Add(this.DailyDaysNumber);
			this.DailyPanel.Controls.Add(this.zLabel3);
			this.DailyPanel.Controls.Add(this.DailyWeekDay);
			this.DailyPanel.Controls.Add(this.DailyDay);
			this.DailyPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(158, 19, true);
			this.DailyPanel.Name = "DailyPanel";
			this.DailyPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(485, 159, true);
			this.DailyPanel.TabIndex = 18;
			// 
			// DailyNextRunTimeLocal
			// 
			this.DailyNextRunTimeLocal.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.DailyNextRunTimeLocal, "Recurrence+RecurringStartTimeLocalText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ServiceManager.Business.StmServiceTask)(null)).Recurrence.RecurringStartTimeLocalText)));
			this.DailyNextRunTimeLocal.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("RecurrenceControl|8E877FEB-3985-483F-AB7D-4999E19C0A9C", "(Local)");
			this.DailyNextRunTimeLocal.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(294, 64, true);
			this.DailyNextRunTimeLocal.Enabled = false;
			this.DailyNextRunTimeLocal.Name = "DailyNextRunTimeLocal";
			this.DailyNextRunTimeLocal.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(45, 15, true);
			this.DailyNextRunTimeLocal.TabIndex = 47;
			this.DailyNextRunTimeLocal.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.DailyNextRunTimeLocal.TrackDisposedAccess = true;
			// 
			// DailyRecurringStartTimeEdit
			// 
			this.DailyRecurringStartTimeEdit.AutoCompleteMonthThreshold = 1;
			this.DailyRecurringStartTimeEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DailyRecurringStartTimeEdit, "Recurrence+RecurringStartTimeUtc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.ServiceManager.Business.StmServiceTask)(null)).Recurrence.RecurringStartTimeUtc)));
			this.DailyRecurringStartTimeEdit.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("RecurrenceControl|28215238-3def-4b6f-bd91-c4b2f084bd23", "Scheduled Run Time (UTC)");
			this.DailyRecurringStartTimeEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Time;
			this.DailyRecurringStartTimeEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(172, 64, true);
			this.DailyRecurringStartTimeEdit.Name = "DailyRecurringStartTimeEdit";
			this.DailyRecurringStartTimeEdit.TabIndex = 32;
			// 
			// DailyDaysNumber
			// 
			this.DailyDaysNumber.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.DailyDaysNumber, "Recurrence+Period");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.ServiceManager.Business.StmServiceTask)(null)).Recurrence.Period)));
			this.DailyDaysNumber.DecimalPlaces = 2;
			this.DailyDaysNumber.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 8, true);
			this.DailyDaysNumber.Name = "DailyDaysNumber";
			this.DailyDaysNumber.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(39, 20, true);
			this.DailyDaysNumber.TabIndex = 7;
			this.DailyDaysNumber.Text = "1";
			this.DailyDaysNumber.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zLabel3
			// 
			this.zLabel3.AutoSize = true;
			this.zLabel3.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("RecurrenceControl|61e82a4a-b4af-4ae9-bc1b-870cdd24c373", "day(s)");
			this.zLabel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 12, true);
			this.zLabel3.Name = "zLabel3";
			this.zLabel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(35, 13, true);
			this.zLabel3.TabIndex = 6;
			// 
			// DailyWeekDay
			// 
			this.DailyWeekDay.AutoCheck = false;
			this.DailyWeekDay.AutoSize = true;
			this.BindingSource.SetBindingMember(this.DailyWeekDay, "Recurrence+WeekDaysOnly");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ServiceManager.Business.StmServiceTask)(null)).Recurrence.WeekDaysOnly)));
			this.DailyWeekDay.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("RecurrenceControl|3600efff-d9f6-4b38-b4a3-05ca541c2548", "Every weekday");
			this.DailyWeekDay.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.DailyWeekDay.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 39, true);
			this.DailyWeekDay.Name = "DailyWeekDay";
			this.DailyWeekDay.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(98, 17, true);
			this.DailyWeekDay.TabIndex = 5;
			this.DailyWeekDay.TabStop = true;
			this.DailyWeekDay.UseVisualStyleBackColor = true;
			// 
			// DailyDay
			// 
			this.DailyDay.AutoCheck = false;
			this.DailyDay.AutoSize = true;
			this.DailyDay.BackColor = System.Drawing.SystemColors.Info;
			this.BindingSource.SetBindingMember(this.DailyDay, "Recurrence+DailyDay");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ServiceManager.Business.StmServiceTask)(null)).Recurrence.DailyDay)));
			this.DailyDay.Checked = true;
			this.DailyDay.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("RecurrenceControl|71802d3b-3afe-4da5-8fef-8fb6299fc922", "Every");
			this.DailyDay.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.DailyDay.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 9, true);
			this.DailyDay.Name = "DailyDay";
			this.DailyDay.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 17, true);
			this.DailyDay.TabIndex = 4;
			this.DailyDay.TabStop = true;
			this.DailyDay.UseVisualStyleBackColor = false;
			// 
			// YearlyPanel
			// 
			this.YearlyPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.YearlyPanel.Controls.Add(this.YearlyNextRunTimeLocal);
			this.YearlyPanel.Controls.Add(this.YearlyRecurringStartTimeEdit);
			this.YearlyPanel.Controls.Add(this.YearlyMonth);
			this.YearlyPanel.Controls.Add(this.YearlyWeekDayMonth);
			this.YearlyPanel.Controls.Add(this.YearlyWeekDay);
			this.YearlyPanel.Controls.Add(this.YearlyWeekNum);
			this.YearlyPanel.Controls.Add(this.YearlyDay);
			this.YearlyPanel.Controls.Add(this.YearlyWeekDayDropEdit);
			this.YearlyPanel.Controls.Add(this.YearlyEvery);
			this.YearlyPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(158, 19, true);
			this.YearlyPanel.Name = "YearlyPanel";
			this.YearlyPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(485, 159, true);
			this.YearlyPanel.TabIndex = 21;
			this.YearlyPanel.Visible = false;
			// 
			// YearlyNextRunTimeLocal
			// 
			this.YearlyNextRunTimeLocal.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.YearlyNextRunTimeLocal, "Recurrence+RecurringStartTimeLocalText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ServiceManager.Business.StmServiceTask)(null)).Recurrence.RecurringStartTimeLocalText)));
			this.YearlyNextRunTimeLocal.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("RecurrenceControl|34D0EC78-8EAF-4F01-BFFA-21F2C6D85F38", "(Local)");
			this.YearlyNextRunTimeLocal.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(280, 63, true);
			this.YearlyNextRunTimeLocal.Enabled = false;
			this.YearlyNextRunTimeLocal.Name = "YearlyNextRunTimeLocal";
			this.YearlyNextRunTimeLocal.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(45, 15, true);
			this.YearlyNextRunTimeLocal.TabIndex = 48;
			this.YearlyNextRunTimeLocal.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.YearlyNextRunTimeLocal.TrackDisposedAccess = true;
			// 
			// YearlyRecurringStartTimeEdit
			// 
			this.YearlyRecurringStartTimeEdit.AutoCompleteMonthThreshold = 1;
			this.YearlyRecurringStartTimeEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.YearlyRecurringStartTimeEdit, "Recurrence+RecurringStartTimeUtc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.ServiceManager.Business.StmServiceTask)(null)).Recurrence.RecurringStartTimeUtc)));
			this.YearlyRecurringStartTimeEdit.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("RecurrenceControl|e1e5bea8-4c40-4913-bd66-fa2fd7ed516d", "Scheduled Run Time (UTC)");
			this.YearlyRecurringStartTimeEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Time;
			this.YearlyRecurringStartTimeEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(158, 63, true);
			this.YearlyRecurringStartTimeEdit.Name = "YearlyRecurringStartTimeEdit";
			this.YearlyRecurringStartTimeEdit.TabIndex = 33;
			// 
			// YearlyMonth
			// 
			this.BindingSource.SetBindingMember(this.YearlyMonth, "Recurrence+EveryMonthNumberDayAsString");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.ServiceManager.Business.StmServiceTask)(null)).Recurrence.EveryMonthNumberDayAsString)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.ServiceManager.Business.StmServiceTask)(null)).Recurrence.Lookups.Months)));
			this.YearlyMonth.BindToList = "Recurrence+Lookups+Months";
			this.YearlyMonth.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(93, 9, true);
			this.YearlyMonth.Name = "YearlyMonth";
			this.YearlyMonth.PreBoundMaxLength = 2;
			this.YearlyMonth.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.YearlyMonth.TabIndex = 28;
			// 
			// YearlyWeekDayMonth
			// 
			this.BindingSource.SetBindingMember(this.YearlyWeekDayMonth, "Recurrence+MonthNumberAsString");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.ServiceManager.Business.StmServiceTask)(null)).Recurrence.MonthNumberAsString)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.ServiceManager.Business.StmServiceTask)(null)).Recurrence.Lookups.Months)));
			this.YearlyWeekDayMonth.BindToList = "Recurrence+Lookups+Months";
			this.YearlyWeekDayMonth.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(313, 36, true);
			this.YearlyWeekDayMonth.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("RecurrenceControl|84a78524-62fd-4d1c-83ee-080a67ee77f9", "of");
			this.YearlyWeekDayMonth.MaxItemsToShowInDropDown = 12;
			this.YearlyWeekDayMonth.Name = "YearlyWeekDayMonth";
			this.YearlyWeekDayMonth.PreBoundMaxLength = 2;
			this.YearlyWeekDayMonth.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.YearlyWeekDayMonth.TabIndex = 29;
			// 
			// YearlyWeekDay
			// 
			this.YearlyWeekDay.AutoCheck = false;
			this.YearlyWeekDay.AutoSize = true;
			this.YearlyWeekDay.BackColor = System.Drawing.SystemColors.Info;
			this.BindingSource.SetBindingMember(this.YearlyWeekDay, "Recurrence+YearlyWeekDay");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ServiceManager.Business.StmServiceTask)(null)).Recurrence.YearlyWeekDay)));
			this.YearlyWeekDay.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("RecurrenceControl|05225899-5381-4f23-8b9b-39b824c70854", "The");
			this.YearlyWeekDay.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.YearlyWeekDay.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 39, true);
			this.YearlyWeekDay.Name = "YearlyWeekDay";
			this.YearlyWeekDay.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(44, 17, true);
			this.YearlyWeekDay.TabIndex = 21;
			this.YearlyWeekDay.TabStop = true;
			this.YearlyWeekDay.UseVisualStyleBackColor = false;
			// 
			// YearlyWeekNum
			// 
			this.BindingSource.SetBindingMember(this.YearlyWeekNum, "Recurrence+WeekCountAsString");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.ServiceManager.Business.StmServiceTask)(null)).Recurrence.WeekCountAsString)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.ServiceManager.Business.StmServiceTask)(null)).Recurrence.Lookups.WeekCounts)));
			this.YearlyWeekNum.BindToList = "Lookups+WeekCounts";
			this.YearlyWeekNum.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(93, 36, true);
			this.YearlyWeekNum.Name = "YearlyWeekNum";
			this.YearlyWeekNum.PreBoundMaxLength = 1;
			this.YearlyWeekNum.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.YearlyWeekNum.TabIndex = 26;
			// 
			// YearlyDay
			// 
			this.YearlyDay.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.YearlyDay, "Recurrence+YearlyDay");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.ServiceManager.Business.StmServiceTask)(null)).Recurrence.YearlyDay)));
			this.YearlyDay.DecimalPlaces = 2;
			this.YearlyDay.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(222, 9, true);
			this.YearlyDay.Name = "YearlyDay";
			this.YearlyDay.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(35, 20, true);
			this.YearlyDay.TabIndex = 25;
			this.YearlyDay.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// YearlyWeekDayDropEdit
			// 
			this.BindingSource.SetBindingMember(this.YearlyWeekDayDropEdit, "Recurrence+DayName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.ServiceManager.Business.StmServiceTask)(null)).Recurrence.DayName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.ServiceManager.Business.StmServiceTask)(null)).Recurrence.Lookups.WeekDays)));
			this.YearlyWeekDayDropEdit.BindToList = "Recurrence+Lookups+WeekDays";
			this.YearlyWeekDayDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(222, 36, true);
			this.YearlyWeekDayDropEdit.Name = "YearlyWeekDayDropEdit";
			this.YearlyWeekDayDropEdit.PreBoundMaxLength = 3;
			this.YearlyWeekDayDropEdit.ShowDescriptionBox = false;
			this.YearlyWeekDayDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.YearlyWeekDayDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.YearlyWeekDayDropEdit.TabIndex = 27;
			// 
			// YearlyEvery
			// 
			this.YearlyEvery.AutoCheck = false;
			this.YearlyEvery.AutoSize = true;
			this.YearlyEvery.BackColor = System.Drawing.SystemColors.Info;
			this.BindingSource.SetBindingMember(this.YearlyEvery, "Recurrence+YearlyEvery");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ServiceManager.Business.StmServiceTask)(null)).Recurrence.YearlyEvery)));
			this.YearlyEvery.Checked = true;
			this.YearlyEvery.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("RecurrenceControl|c8e42511-4278-4c31-b592-2dfe5b4c5a7e", "Every");
			this.YearlyEvery.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.YearlyEvery.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 9, true);
			this.YearlyEvery.Name = "YearlyEvery";
			this.YearlyEvery.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 17, true);
			this.YearlyEvery.TabIndex = 20;
			this.YearlyEvery.TabStop = true;
			this.YearlyEvery.UseVisualStyleBackColor = false;
			// 
			// StmServiceTaskRecurrenceControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MainGroupBox);
			this.Name = "StmServiceTaskRecurrenceControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(660, 217, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainGroupBox.ResumeLayout(false);
			this.MainGroupBox.PerformLayout();
			this.PeriodPanel.ResumeLayout(false);
			this.PeriodPanel.PerformLayout();
			this.WeeklyPanel.ResumeLayout(false);
			this.WeeklyPanel.PerformLayout();
			this.WeeklyRecurringStartTimeEdit.ResumeLayout(true);
			this.WeeklyRecurringStartTimeEdit.PerformLayout();
			this.DailyPanel.ResumeLayout(false);
			this.DailyPanel.PerformLayout();
			this.DailyRecurringStartTimeEdit.ResumeLayout(true);
			this.DailyRecurringStartTimeEdit.PerformLayout();
			this.YearlyPanel.ResumeLayout(false);
			this.YearlyPanel.PerformLayout();
			this.YearlyRecurringStartTimeEdit.ResumeLayout(true);
			this.YearlyRecurringStartTimeEdit.PerformLayout();
			this.YearlyMonth.ResumeLayout(true);
			this.YearlyMonth.PerformLayout();
			this.YearlyWeekDayMonth.ResumeLayout(true);
			this.YearlyWeekDayMonth.PerformLayout();
			this.YearlyWeekNum.ResumeLayout(true);
			this.YearlyWeekNum.PerformLayout();
			this.YearlyWeekDayDropEdit.ResumeLayout(true);
			this.YearlyWeekDayDropEdit.PerformLayout();
			this.MonthlyPanel.ResumeLayout(false);
			this.MonthlyPanel.PerformLayout();
			this.MonthlyRecurringStartTimeEdit.ResumeLayout(true);
			this.MonthlyRecurringStartTimeEdit.PerformLayout();
			this.MonthlyNumWeek.ResumeLayout(true);
			this.MonthlyNumWeek.PerformLayout();
			this.MonthlyWeekDayDropEdit.ResumeLayout(true);
			this.MonthlyWeekDayDropEdit.PerformLayout();
			this.SecondPanel.ResumeLayout(false);
			this.SecondPanel.PerformLayout();
			this.EndTimeSecond.ResumeLayout(true);
			this.EndTimeSecond.PerformLayout();
			this.StartTimeSecond.ResumeLayout(true);
			this.StartTimeSecond.PerformLayout();
			this.MinutePanel.ResumeLayout(false);
			this.MinutePanel.PerformLayout();
			this.EndTimeMinute.ResumeLayout(true);
			this.EndTimeMinute.PerformLayout();
			this.StartTimeMinute.ResumeLayout(true);
			this.StartTimeMinute.PerformLayout();
			this.HourlyPanel.ResumeLayout(false);
			this.HourlyPanel.PerformLayout();
			this.EndTimeHour.ResumeLayout(true);
			this.EndTimeHour.PerformLayout();
			this.StartTimeHour.ResumeLayout(true);
			this.StartTimeHour.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private ZGroupBox MainGroupBox;
		private ZPanel PeriodPanel;
		private ZRadioButton SecondRadioButton;
		private ZRadioButton MinuteRadioButton;
		private ZRadioButton HourlyRadioButton;
		private ZRadioButton DailyRadioButton;
		private ZRadioButton WeeklyRadioButton;
		private ZRadioButton MonthlyRadioButton;
		private ZRadioButton YearlyRadioButton;

		// Seconds calculator
		protected internal ZPanel SecondPanel;
		private ZDateEdit EndTimeSecond;
		private ZDateEdit StartTimeSecond;
		private ZLabel PeriodLabelSeconds;
		private ZCalcEdit EverySecond;
		private ZLabel SecondsPanelLabel;
		private ZTextBox SecondlyEndLocal;
		private ZTextBox SecondlyStartLocal;

		// Minutes calculator
		protected internal ZPanel MinutePanel;
		private ZDateEdit EndTimeMinute;
		private ZDateEdit StartTimeMinute;
		private ZLabel PeriodLabelMinutes;
		private ZCalcEdit EveryMinute;
		private ZLabel MinutesPanelLabel;
		private ZTextBox MinuteEndLocal;
		private ZTextBox MinuteStartLocal;

		// Hours calculator
		protected internal ZPanel HourlyPanel;
		private ZDateEdit EndTimeHour;
		private ZDateEdit StartTimeHour;
		private ZLabel lblHours;
		private ZCalcEdit EveryHour;
		private ZLabel lblHourFrequency;
		private ZTextBox hourlyEndLocal;
		private ZTextBox hourlyStartLocal;

		// Days calculators
		protected internal ZPanel DailyPanel;
		private ZCalcEdit DailyDaysNumber;
		private ZLabel zLabel3;
		private ZRadioButton DailyWeekDay;
		private ZRadioButton DailyDay;
		private ZDateEdit DailyRecurringStartTimeEdit;
		private ZTextBox DailyNextRunTimeLocal;

		// Weeks calculator
		protected internal ZPanel WeeklyPanel;
		private ZCheckBox SundayCheckBox;
		private ZCheckBox SaturdayCheckBox;
		private ZCheckBox FridayCheckBox;
		private ZCheckBox ThursdayCheckBox;
		private ZCheckBox WednesdayCheckBox;
		private ZCheckBox TuesdayCheckBox;
		private ZCheckBox MondayCheckBox;
		private ZLabel zLabel22;
		private ZCalcEdit RecurEveryWeekTextBox;
		private ZTextBox WeeklyNextRuntimeLocal;
		private ZDateEdit WeeklyRecurringStartTimeEdit;

		// Months calculator
		protected internal ZPanel MonthlyPanel;
		private ZCheckBox MonthLastDay;
		private ZRadioButton MonthlyWeekDay;
		private ZDropEdit MonthlyNumWeek;
		private ZCalcEdit MonthlyNumMonth;
		private ZCalcEdit MonthlyDate;
		private ZLabel zLabel28;
		private ZDropEdit MonthlyWeekDayDropEdit;
		private ZRadioButton MonthlyDay;
		private ZLabel zLabel30;
		private ZTextBox MonthlyNextRunTimeLocal;
		private ZDateEdit MonthlyRecurringStartTimeEdit;

		// Years calculator
		protected internal ZPanel YearlyPanel;
		private ZDropEdit YearlyWeekDayMonth;
		private ZRadioButton YearlyWeekDay;
		private ZDropEdit YearlyWeekNum;
		private ZCalcEdit YearlyDay;
		private ZDropEdit YearlyWeekDayDropEdit;
		private ZRadioButton YearlyEvery;
		private ZDropEdit YearlyMonth;
		private ZTextBox YearlyNextRunTimeLocal;
		private ZDateEdit YearlyRecurringStartTimeEdit;
	}
}
