using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ArchiveManager.GUI.Schedule
{
	partial class ArchiveScheduleTaskForm
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
			this.archiveSystemDropDown = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.descriptionTextBox = new Enterprise.ZArchitecture.ZTranslatableTextControl();
			this.alertLabel = new Enterprise.ZArchitecture.ZLabel();
			this.nextScheduledRunDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.onOrBeforeTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.dateParameterDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.archiveOnOrBeforeRelativeDateRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.archiveOnOrBeforeDateRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.parameterGroupbox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OnOrBeforePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.onOrBeforeCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.onOrBeforeDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.maxRunDurationCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.JobsOptionsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.WithoutJobsRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.WithJobsRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.activeCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.RecurrenceControl = new Enterprise.MasterFiles.GUI.Scheduler.RecurrenceControl();
			this.verboseLogCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.useOnOrBeforeDateWhenWatermarkResetCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.archiveDecCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.archiveSystemDropDown.SuspendLayout();
			this.descriptionTextBox.SuspendLayout();
			this.nextScheduledRunDateEdit.SuspendLayout();
			this.dateParameterDropEdit.SuspendLayout();
			this.onOrBeforeTypeDropEdit.SuspendLayout();
			this.parameterGroupbox.SuspendLayout();
			this.OnOrBeforePanel.SuspendLayout();
			this.onOrBeforeDateEdit.SuspendLayout();
			this.JobsOptionsPanel.SuspendLayout();
			this.RecurrenceControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1042, 419, true);
			this.MainTabControl.TabIndex = 0;
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.archiveDecCheckBox);
			this.MainTabPage.Controls.Add(this.verboseLogCheckBox);
			this.MainTabPage.Controls.Add(this.useOnOrBeforeDateWhenWatermarkResetCheckBox);
			this.MainTabPage.Controls.Add(this.activeCheckBox);
			this.MainTabPage.Controls.Add(this.parameterGroupbox);
			this.MainTabPage.Controls.Add(this.nextScheduledRunDateEdit);
			this.MainTabPage.Controls.Add(this.descriptionTextBox);
			this.MainTabPage.Controls.Add(this.alertLabel);
			this.MainTabPage.Controls.Add(this.archiveSystemDropDown);
			this.MainTabPage.Controls.Add(this.RecurrenceControl);
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 27, true);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1037, 389, true);
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 27, true);
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1037, 369, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 27, true);
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1037, 389, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1042, 419, true);
			// 
			// PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel
			// 
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.TabIndex = 0;
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1042, 24, true);
			this.MainStatusBar.TabIndex = 0;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ArchiveManager.Business.Schedule.ArchiveScheduleTask);
			// 
			// archiveSystemDropDown
			// 
			this.archiveSystemDropDown.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.archiveSystemDropDown, "S5_ScheduleType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.ArchiveManager.Business.Schedule.ArchiveScheduleTask)(null)).S5_ScheduleType)));
			this.archiveSystemDropDown.CaptionResourceString = Enterprise.ArchiveManager.GUI.Res.GetData("ArchiveScheduleTaskForm|bafde6ea-26ea-413a-8eae-1c1608165532", "Archive System");
			this.archiveSystemDropDown.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 11, true);
			this.archiveSystemDropDown.Name = "archiveSystemDropDown";
			this.archiveSystemDropDown.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(222, 25, true);
			this.archiveSystemDropDown.TabIndex = 0;
			// 
			// descriptionTextBox
			// 
			this.descriptionTextBox.AcceptsReturn = false;
			this.descriptionTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.descriptionTextBox, "S5_ScheduleDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ArchiveManager.Business.Schedule.ArchiveScheduleTask)(null)).S5_ScheduleDescription)));
			this.descriptionTextBox.GridCurrent = null;
			this.descriptionTextBox.GridMember = null;
			this.descriptionTextBox.IsLanguageEditingEnabled = true;
			this.descriptionTextBox.IsMultiLine = false;
			this.descriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 37, true);
			this.descriptionTextBox.Name = "descriptionTextBox";
			this.descriptionTextBox.ReadOnly = false;
			this.descriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(218, 20, true);
			this.descriptionTextBox.TabIndex = 1;
			// 
			// alertLabel
			// 
			this.alertLabel.AutoSize = true;
			this.alertLabel.CaptionResourceString = Enterprise.ArchiveManager.GUI.Res.GetData("ArchiveScheduleTaskForm|88CC22F7-EA6A-4CC2-B190-2AF0DFCEFB20", "Warning: You have selected a Purge System. Proceed with caution.");
			this.alertLabel.ForeColor = System.Drawing.Color.Red;
			this.alertLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 97, true);
			this.alertLabel.Name = "alertLabel";
			this.alertLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(551, 21, true);
			this.alertLabel.TabIndex = 10;
			this.alertLabel.Visible = false;
			// 
			// nextScheduledRunDateEdit
			// 
			this.nextScheduledRunDateEdit.AllowDrop = true;
			this.nextScheduledRunDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.nextScheduledRunDateEdit, "CalcNextRunTimeLocal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.ArchiveManager.Business.Schedule.ArchiveScheduleTask)(null)).CalcNextRunTimeLocal)));
			this.nextScheduledRunDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.nextScheduledRunDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 67, true);
			this.nextScheduledRunDateEdit.Name = "nextScheduledRunDateEdit";
			this.nextScheduledRunDateEdit.TabIndex = 2;
			//
			// dateParameterDropEdit
			//
			this.dateParameterDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.dateParameterDropEdit, "DateParameter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.ArchiveManager.Business.Schedule.ArchiveScheduleTask)(null)).DateParameter)));
			this.dateParameterDropEdit.CaptionResourceString = Enterprise.ArchiveManager.GUI.Res.GetData("ArchiveScheduleTaskForm|5851165E-E24E-4AB1-80CC-FAD4B87D8907", "Select Date Parameter", "Archive records based on selected date parameter.");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.dateParameterDropEdit, true);
			this.dateParameterDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(277, 79, true);
			this.dateParameterDropEdit.Name = "dateParameterDropEdit";
			this.dateParameterDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(145, 25, true);
			this.dateParameterDropEdit.TabIndex = 6;
			//
			// onOrBeforeTypeDropEdit
			// 
			this.onOrBeforeTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.onOrBeforeTypeDropEdit, "ArchiveRecordsOnOrBeforeType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.ArchiveManager.Business.Schedule.ArchiveScheduleTask)(null)).ArchiveRecordsOnOrBeforeType)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.onOrBeforeTypeDropEdit, false);
			this.onOrBeforeTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(315, 2, true);
			this.onOrBeforeTypeDropEdit.Name = "onOrBeforeTypeDropEdit";
			this.onOrBeforeTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(107, 25, true);
			this.onOrBeforeTypeDropEdit.TabIndex = 2;
			// 
			// archiveOnOrBeforeRelativeDateRadioButton
			// 
			this.archiveOnOrBeforeRelativeDateRadioButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.archiveOnOrBeforeRelativeDateRadioButton.AutoCheck = false;
			this.archiveOnOrBeforeRelativeDateRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.archiveOnOrBeforeRelativeDateRadioButton, "IsArchiveRecordsOnOrBeforeRelativeDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ArchiveManager.Business.Schedule.ArchiveScheduleTask)(null)).IsArchiveRecordsOnOrBeforeRelativeDate)));
			this.archiveOnOrBeforeRelativeDateRadioButton.CaptionResourceString = Enterprise.ArchiveManager.GUI.Res.GetData("ArchiveScheduleTaskForm|40be3e18-325d-4a39-86ae-55f59471937e", "Archive Records On or Before:");
			this.archiveOnOrBeforeRelativeDateRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(209, 2, true);
			this.archiveOnOrBeforeRelativeDateRadioButton.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(257, 23, true);
			this.archiveOnOrBeforeRelativeDateRadioButton.Name = "archiveOnOrBeforeRelativeDateRadioButton";
			this.archiveOnOrBeforeRelativeDateRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 23, true);
			this.archiveOnOrBeforeRelativeDateRadioButton.TabIndex = 0;
			this.archiveOnOrBeforeRelativeDateRadioButton.TabStop = true;
			this.archiveOnOrBeforeRelativeDateRadioButton.UseVisualStyleBackColor = true;
			this.archiveOnOrBeforeRelativeDateRadioButton.CheckedChanged += new System.EventHandler(this.archiveOnOrBeforeTypeRadioButton_CheckedChanged);
			// 
			// archiveOnOrBeforeDateRadioButton
			// 
			this.archiveOnOrBeforeDateRadioButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.archiveOnOrBeforeDateRadioButton.AutoCheck = false;
			this.archiveOnOrBeforeDateRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.archiveOnOrBeforeDateRadioButton, "IsArchiveRecordsOnOrBeforeDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ArchiveManager.Business.Schedule.ArchiveScheduleTask)(null)).IsArchiveRecordsOnOrBeforeDate)));
			this.archiveOnOrBeforeDateRadioButton.CaptionResourceString = Enterprise.ArchiveManager.GUI.Res.GetData("ArchiveScheduleTaskForm|6c9ab14e-b7e3-4592-b1de-b401b9f184f3", "Archive Records On or Before:");
			this.archiveOnOrBeforeDateRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(209, 29, true);
			this.archiveOnOrBeforeDateRadioButton.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(257, 23, true);
			this.archiveOnOrBeforeDateRadioButton.Name = "archiveOnOrBeforeDateRadioButton";
			this.archiveOnOrBeforeDateRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 23, true);
			this.archiveOnOrBeforeDateRadioButton.TabIndex = 3;
			this.archiveOnOrBeforeDateRadioButton.TabStop = true;
			this.archiveOnOrBeforeDateRadioButton.UseVisualStyleBackColor = true;
			this.archiveOnOrBeforeDateRadioButton.CheckedChanged += new System.EventHandler(this.archiveOnOrBeforeDateRadioButton_CheckedChanged);
			// 
			// parameterGroupbox
			// 
			this.parameterGroupbox.CaptionResourceString = Enterprise.ArchiveManager.GUI.Res.GetData("ArchiveScheduleTaskForm|38afd628-85eb-4004-a42b-0d3323dcadc2", "Archive System Parameters", "Specify the archive configuration for this archive schedule.");
			this.parameterGroupbox.Controls.Add(this.OnOrBeforePanel);
			this.parameterGroupbox.Controls.Add(this.JobsOptionsPanel);
			this.parameterGroupbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(369, 11, true);
			this.parameterGroupbox.Name = "parameterGroupbox";
			this.parameterGroupbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(446, 159, true);
			this.parameterGroupbox.TabIndex = 3;
			this.parameterGroupbox.TabStop = false;
			// 
			// OnOrBeforePanel
			// 
			this.OnOrBeforePanel.AutoSize = true;
			this.OnOrBeforePanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.OnOrBeforePanel.Controls.Add(this.dateParameterDropEdit);
			this.OnOrBeforePanel.Controls.Add(this.onOrBeforeCalcEdit);
			this.OnOrBeforePanel.Controls.Add(this.onOrBeforeDateEdit);
			this.OnOrBeforePanel.Controls.Add(this.onOrBeforeTypeDropEdit);
			this.OnOrBeforePanel.Controls.Add(this.archiveOnOrBeforeDateRadioButton);
			this.OnOrBeforePanel.Controls.Add(this.archiveOnOrBeforeRelativeDateRadioButton);
			this.OnOrBeforePanel.Controls.Add(this.maxRunDurationCalcEdit);
			this.OnOrBeforePanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.OnOrBeforePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 63, true);
			this.OnOrBeforePanel.Name = "OnOrBeforePanel";
			this.OnOrBeforePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(442, 80, true);
			this.OnOrBeforePanel.TabIndex = 12;
			// 
			// onOrBeforeCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.onOrBeforeCalcEdit, "ArchiveRecordsOnOrBeforeNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.ArchiveManager.Business.Schedule.ArchiveScheduleTask)(null)).ArchiveRecordsOnOrBeforeNumber)));
			this.onOrBeforeCalcEdit.DecimalPlaces = 0;
			this.onOrBeforeCalcEdit.Decimals = 0;
			this.onOrBeforeCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(277, 2, true);
			this.onOrBeforeCalcEdit.Name = "onOrBeforeCalcEdit";
			this.onOrBeforeCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(35, 25, true);
			this.onOrBeforeCalcEdit.TabIndex = 1;
			this.onOrBeforeCalcEdit.Text = "0";
			this.onOrBeforeCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// onOrBeforeDateEdit
			// 
			this.onOrBeforeDateEdit.AllowDrop = true;
			this.onOrBeforeDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.onOrBeforeDateEdit, "ArchiveRecordsOnOrBeforeDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.ArchiveManager.Business.Schedule.ArchiveScheduleTask)(null)).ArchiveRecordsOnOrBeforeDate)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.onOrBeforeDateEdit, false);
			this.onOrBeforeDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(277, 27, true);
			this.onOrBeforeDateEdit.Name = "onOrBeforeDateEdit";
			this.onOrBeforeDateEdit.TabIndex = 4;
			// 
			// maxRunDurationCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.maxRunDurationCalcEdit, "MaxRunDurationInMinutes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.ArchiveManager.Business.Schedule.ArchiveScheduleTask)(null)).MaxRunDurationInMinutes)));
			this.maxRunDurationCalcEdit.CaptionResourceString = Enterprise.ArchiveManager.GUI.Res.GetData("ArchiveScheduleTaskForm|52bf832c-fa7c-45fa-a1e5-7feb2d6321ad", "Max. Run Duration (Minutes)");
			this.maxRunDurationCalcEdit.DecimalPlaces = 0;
			this.maxRunDurationCalcEdit.Decimals = 0;
			this.maxRunDurationCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(277, 53, true);
			this.maxRunDurationCalcEdit.Name = "maxRunDurationCalcEdit";
			this.maxRunDurationCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 25, true);
			this.maxRunDurationCalcEdit.TabIndex = 5;
			this.maxRunDurationCalcEdit.Text = "0";
			this.maxRunDurationCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// JobsOptionsPanel
			// 
			this.JobsOptionsPanel.Controls.Add(this.WithoutJobsRadioButton);
			this.JobsOptionsPanel.Controls.Add(this.WithJobsRadioButton);
			this.JobsOptionsPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.JobsOptionsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 23, true);
			this.JobsOptionsPanel.Name = "JobsOptionsPanel";
			this.JobsOptionsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(442, 41, true);
			this.JobsOptionsPanel.TabIndex = 11;
			// 
			// WithoutJobsRadioButton
			// 
			this.WithoutJobsRadioButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.WithoutJobsRadioButton.AutoCheck = false;
			this.WithoutJobsRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.WithoutJobsRadioButton, "ShouldIncludeRecordsWithoutJobs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ArchiveManager.Business.Schedule.ArchiveScheduleTask)(null)).ShouldIncludeRecordsWithoutJobs)));
			this.WithoutJobsRadioButton.CaptionResourceString = Enterprise.ArchiveManager.GUI.Res.GetData("ArchiveScheduleTaskForm|9157a6e2-b9d3-4fca-a871-b9f78eb10a59", "Without Jobs", "Include Documents of Operational Records without Jobs", "Whether to Purge Documents of Operational Records without a Job");
			this.WithoutJobsRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(215, 14, true);
			this.WithoutJobsRadioButton.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(257, 23, true);
			this.WithoutJobsRadioButton.Name = "WithoutJobsRadioButton";
			this.WithoutJobsRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(131, 23, true);
			this.WithoutJobsRadioButton.TabIndex = 2;
			this.WithoutJobsRadioButton.TabStop = true;
			this.WithoutJobsRadioButton.UseVisualStyleBackColor = true;
			// 
			// WithJobsRadioButton
			// 
			this.WithJobsRadioButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.WithJobsRadioButton.AutoCheck = false;
			this.WithJobsRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.WithJobsRadioButton, "ShouldIncludeRecordsWithJobs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ArchiveManager.Business.Schedule.ArchiveScheduleTask)(null)).ShouldIncludeRecordsWithJobs)));
			this.WithJobsRadioButton.CaptionResourceString = Enterprise.ArchiveManager.GUI.Res.GetData("ArchiveScheduleTaskForm|b977842d-27f2-454e-a40b-425af4b9e837", "With Jobs", "Include Documents of Operational Records with Jobs", "Whether to Purge Documents of Operational Records with a Job");
			this.WithJobsRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(143, 14, true);
			this.WithJobsRadioButton.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(257, 23, true);
			this.WithJobsRadioButton.Name = "WithJobsRadioButton";
			this.WithJobsRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(68, 23, true);
			this.WithJobsRadioButton.TabIndex = 1;
			this.WithJobsRadioButton.TabStop = true;
			this.WithJobsRadioButton.UseVisualStyleBackColor = true;
			// 
			// activeCheckBox
			// 
			this.activeCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.activeCheckBox, "S5_IsActive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ArchiveManager.Business.Schedule.ArchiveScheduleTask)(null)).S5_IsActive)));
			this.activeCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(819, 21, true);
			this.activeCheckBox.Name = "activeCheckBox";
			this.activeCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 23, true);
			this.activeCheckBox.TabIndex = 7;
			this.activeCheckBox.UseVisualStyleBackColor = true;
			// 
			// RecurrenceControl
			// 
			this.RecurrenceControl.AllowDrop = true;
			this.RecurrenceControl.AutoSize = true;
			this.RecurrenceControl.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.BindingSource.SetBindingMember(this.RecurrenceControl, "Recurrence");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Scheduler.Business.StmScheduleTaskRecurrence)(((Enterprise.ArchiveManager.Business.Schedule.ArchiveScheduleTask)(null)).Recurrence)));
			this.RecurrenceControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 173, true);
			this.RecurrenceControl.Name = "RecurrenceControl";
			this.RecurrenceControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(741, 150, true);
			this.RecurrenceControl.TabIndex = 6;
			// 
			// verboseLogCheckBox
			// 
			this.verboseLogCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.verboseLogCheckBox, "IsVerboseLog");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ArchiveManager.Business.Schedule.ArchiveScheduleTask)(null)).IsVerboseLog)));
			this.verboseLogCheckBox.CaptionResourceString = Enterprise.ArchiveManager.GUI.Res.GetData("ArchiveScheduleTaskForm|087071D4-4521-4B36-953B-C25D33A11C71", "Verbose Logging", "Turn on verbose logging to log more details about archiving process.");
			this.verboseLogCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(819, 49, true);
			this.verboseLogCheckBox.Name = "verboseLogCheckBox";
			this.verboseLogCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(163, 23, true);
			this.verboseLogCheckBox.TabIndex = 8;
			this.verboseLogCheckBox.UseVisualStyleBackColor = true;
			// 
			// useOnOrBeforeDateWhenWatermarkResetCheckBox
			// 
			this.useOnOrBeforeDateWhenWatermarkResetCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.useOnOrBeforeDateWhenWatermarkResetCheckBox, "UseOnOrBeforeDateWhenWatermarkReset");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ArchiveManager.Business.Schedule.ArchiveScheduleTask)(null)).UseOnOrBeforeDateWhenWatermarkReset)));
			this.useOnOrBeforeDateWhenWatermarkResetCheckBox.CaptionResourceString = Enterprise.ArchiveManager.GUI.Res.GetData("ArchiveScheduleTaskForm|6DDEB728-CFDA-4661-8A47-54C35B9013F9", "Use On or Before Date When Watermark is Reset", "Watermark is reset to the current 'On or Before Date' instead of 'Earliest Possible Start Date' when all records have been processed.");
			this.useOnOrBeforeDateWhenWatermarkResetCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(819, 103, true);
			this.useOnOrBeforeDateWhenWatermarkResetCheckBox.Name = "useOnOrBeforeDateWhenWatermarkResetCheckBox";
			this.useOnOrBeforeDateWhenWatermarkResetCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(163, 23, true);
			this.useOnOrBeforeDateWhenWatermarkResetCheckBox.TabIndex = 10;
			this.useOnOrBeforeDateWhenWatermarkResetCheckBox.UseVisualStyleBackColor = true;
			// 
			// archiveDecCheckBox
			// 
			this.archiveDecCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.archiveDecCheckBox, "ShouldArchiveDeclaration");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ArchiveManager.Business.Schedule.ArchiveScheduleTask)(null)).ShouldArchiveDeclaration)));
			this.archiveDecCheckBox.CaptionResourceString = Enterprise.ArchiveManager.GUI.Res.GetData("ArchiveScheduleTaskForm|873d495a-6128-47c1-8807-90d3050f10a0", "Incl. Customs", "Include Customs data in the archive schedule run?");
			this.archiveDecCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(819, 76, true);
			this.archiveDecCheckBox.Name = "archiveDecCheckBox";
			this.archiveDecCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(181, 23, true);
			this.archiveDecCheckBox.TabIndex = 9;
			this.archiveDecCheckBox.UseVisualStyleBackColor = true;
			this.archiveDecCheckBox.Visible = false;
			// 
			// ArchiveScheduleTaskForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.ArchiveManager.GUI.Res.GetData("ArchiveScheduleTaskForm|948e8d12-7563-40a8-a0c3-6c7990a9824c", "Archive Schedule");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1042, 475, true);
			this.DataSourceType = typeof(Enterprise.ArchiveManager.Business.Schedule.ArchiveScheduleTask);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(907, 512, true);
			this.Name = "ArchiveScheduleTaskForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "ArchiveScheduleTaskForm";
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
			this.archiveSystemDropDown.ResumeLayout(true);
			this.archiveSystemDropDown.PerformLayout();
			this.descriptionTextBox.ResumeLayout(true);
			this.descriptionTextBox.PerformLayout();
			this.nextScheduledRunDateEdit.ResumeLayout(true);
			this.nextScheduledRunDateEdit.PerformLayout();
			this.dateParameterDropEdit.ResumeLayout(true);
			this.dateParameterDropEdit.PerformLayout();
			this.onOrBeforeTypeDropEdit.ResumeLayout(true);
			this.onOrBeforeTypeDropEdit.PerformLayout();
			this.parameterGroupbox.ResumeLayout(false);
			this.parameterGroupbox.PerformLayout();
			this.OnOrBeforePanel.ResumeLayout(false);
			this.OnOrBeforePanel.PerformLayout();
			this.onOrBeforeDateEdit.ResumeLayout(true);
			this.onOrBeforeDateEdit.PerformLayout();
			this.JobsOptionsPanel.ResumeLayout(false);
			this.JobsOptionsPanel.PerformLayout();
			this.RecurrenceControl.ResumeLayout(true);
			this.RecurrenceControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.MasterFiles.GUI.Scheduler.RecurrenceControl RecurrenceControl;
		public Enterprise.ZArchitecture.GUI.ZDropEdit archiveSystemDropDown;
		private Enterprise.ZArchitecture.ZTranslatableTextControl descriptionTextBox;
		public Enterprise.ZArchitecture.ZLabel alertLabel;
		public Enterprise.ZArchitecture.GUI.ZDateEdit nextScheduledRunDateEdit;
		public Enterprise.ZArchitecture.GUI.ZDropEdit dateParameterDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit onOrBeforeTypeDropEdit;
		public Enterprise.ZArchitecture.GUI.ZRadioButton archiveOnOrBeforeRelativeDateRadioButton;
		public Enterprise.ZArchitecture.GUI.ZGroupBox parameterGroupbox;
		public Enterprise.ZArchitecture.GUI.ZRadioButton archiveOnOrBeforeDateRadioButton;
		private Enterprise.ZArchitecture.ZCalcEdit onOrBeforeCalcEdit;
		private Enterprise.ZArchitecture.GUI.ZDateEdit onOrBeforeDateEdit;
		private Enterprise.ZArchitecture.GUI.ZCheckBox activeCheckBox;
		private Enterprise.ZArchitecture.ZCalcEdit maxRunDurationCalcEdit;
		private Enterprise.ZArchitecture.GUI.ZCheckBox verboseLogCheckBox;
		public Enterprise.ZArchitecture.GUI.ZCheckBox useOnOrBeforeDateWhenWatermarkResetCheckBox;
		public Enterprise.ZArchitecture.GUI.ZCheckBox archiveDecCheckBox;
		public ZArchitecture.GUI.ZPanel JobsOptionsPanel;
		public ZArchitecture.GUI.ZRadioButton WithJobsRadioButton;
		public ZArchitecture.GUI.ZRadioButton WithoutJobsRadioButton;
		public ZPanel OnOrBeforePanel;
	}
}
