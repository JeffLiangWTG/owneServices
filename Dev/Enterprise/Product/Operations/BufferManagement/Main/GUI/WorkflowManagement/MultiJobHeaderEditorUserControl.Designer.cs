namespace Enterprise.BufferManagement.GUI
{
	partial class MultiJobHeaderEditorUserControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo zTimeEditExColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.JobHeadersGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SchedulesZGrid = new Enterprise.ZArchitecture.ZGrid();
			this.UpdateADDButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.UpdateESDButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.AgreedDeliveryDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.EarliestStartDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.BulkUpdateGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.IncrementValueGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.IncrementADDButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.IncrementESDButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ADDHoursTimeEdit = new Enterprise.ZArchitecture.GUI.ZTimeEditEx();
			this.ESDHoursTimeEdit = new Enterprise.ZArchitecture.GUI.ZTimeEditEx();
			this.ADDDaysCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ESDDaysCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.IncrementValuesHintLabel = new Enterprise.ZArchitecture.ZLabel();
			this.SetValueGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.UpdateDateAcceptabilityButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.DateAcceptabilityControl = new Enterprise.BufferManagement.GUI.DateAcceptabilityControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.JobHeadersGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SchedulesZGrid)).BeginInit();
			this.SchedulesZGrid.SuspendLayout();
			this.AgreedDeliveryDateEdit.SuspendLayout();
			this.EarliestStartDateEdit.SuspendLayout();
			this.BulkUpdateGroupBox.SuspendLayout();
			this.IncrementValueGroupBox.SuspendLayout();
			this.SetValueGroupBox.SuspendLayout();
			this.DateAcceptabilityControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.BufferManagement.Business.MultiJobHeaderEditorViewModel);
			// 
			// JobHeadersGroupBox
			// 
			this.JobHeadersGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.JobHeadersGroupBox.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("b45eaa62-8656-47f5-af74-6e598202cf91", "Job Details");
			this.JobHeadersGroupBox.Controls.Add(this.SchedulesZGrid);
			this.JobHeadersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.JobHeadersGroupBox.Name = "JobHeadersGroupBox";
			this.JobHeadersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1032, 398, true);
			this.JobHeadersGroupBox.TabIndex = 0;
			this.JobHeadersGroupBox.TabStop = false;
			// 
			// SchedulesZGrid
			// 
			this.SchedulesZGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.SchedulesZGrid, "JobHeaderViews");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.BufferManagement.Business.MultiJobHeaderEditorViewModel)(null)).JobHeaderViews)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.JobHeaderView)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.MultiJobHeaderEditorViewModel)(null)).JobHeaderViews)).SyncRoot)).Job)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.JobHeaderView)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.MultiJobHeaderEditorViewModel)(null)).JobHeaderViews)).SyncRoot)).JobDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.JobHeaderView)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.MultiJobHeaderEditorViewModel)(null)).JobHeaderViews)).SyncRoot)).Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.JobHeaderView)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.MultiJobHeaderEditorViewModel)(null)).JobHeaderViews)).SyncRoot)).ProcessHeader.FH_StatusDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.BufferManagement.Business.JobHeaderView)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.MultiJobHeaderEditorViewModel)(null)).JobHeaderViews)).SyncRoot)).ProcessHeader.PlannedDuration)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.BufferManagement.Business.JobHeaderView)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.MultiJobHeaderEditorViewModel)(null)).JobHeaderViews)).SyncRoot)).EarliestStartDateLocal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.BufferManagement.Business.JobHeaderView)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.MultiJobHeaderEditorViewModel)(null)).JobHeaderViews)).SyncRoot)).AgreedDeliveryDateLocal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.JobHeaderView)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.MultiJobHeaderEditorViewModel)(null)).JobHeaderViews)).SyncRoot)).DateAcceptability)));
			this.SchedulesZGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "Job";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zTextBoxColumnStyleInfo2.ColumnName = "JobDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo3.ColumnName = "Description";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo4.ColumnName = "ProcessHeader+FH_StatusDescription";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTimeEditExColumnStyleInfo1.AllowNegative = false;
			zTimeEditExColumnStyleInfo1.ColumnName = "ProcessHeader+PlannedDuration";
			zTimeEditExColumnStyleInfo1.IsReadOnly = true;
			zTimeEditExColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo1.ColumnName = "EarliestStartDateLocal";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			zDateEditColumnStyleInfo2.ColumnName = "AgreedDeliveryDateLocal";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			zDropEditColumnStyleInfo1.ColumnName = "DateAcceptability";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.SchedulesZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.SchedulesZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.SchedulesZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.SchedulesZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.SchedulesZGrid.ColumnStyles.Add(zTimeEditExColumnStyleInfo1);
			this.SchedulesZGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.SchedulesZGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.SchedulesZGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.SchedulesZGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SchedulesZGrid.GridId = "e8dd3189-5ae5-42ea-b7ee-db780c4ee63d";
			this.SchedulesZGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SchedulesZGrid.LayoutKey = "SchedulesZGrid";
			this.SchedulesZGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.SchedulesZGrid.Name = "SchedulesZGrid";
			this.SchedulesZGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1026, 379, true);
			this.SchedulesZGrid.TabIndex = 4;
			this.SchedulesZGrid.DoubleClick += new System.EventHandler(this.SchedulesZGrid_DoubleClick);
			// 
			// UpdateADDButton
			// 
			this.UpdateADDButton.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("c33fa8eb-9b18-41ce-b4be-27775e8293b1", "Update Selected Rows");
			this.UpdateADDButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(271, 50, true);
			this.UpdateADDButton.Name = "UpdateADDButton";
			this.UpdateADDButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.UpdateADDButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(141, 20, true);
			this.UpdateADDButton.TabIndex = 3;
			this.UpdateADDButton.UseVisualStyleBackColor = false;
			this.UpdateADDButton.Click += new System.EventHandler(this.UpdateADDButton_Click);
			// 
			// UpdateESDButton
			// 
			this.UpdateESDButton.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("aff239e9-7129-4345-8cdb-c338386c0638", "Update Selected Rows");
			this.UpdateESDButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(271, 19, true);
			this.UpdateESDButton.Name = "UpdateESDButton";
			this.UpdateESDButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.UpdateESDButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(141, 20, true);
			this.UpdateESDButton.TabIndex = 1;
			this.UpdateESDButton.UseVisualStyleBackColor = false;
			this.UpdateESDButton.Click += new System.EventHandler(this.UpdateESDButton_Click);
			// 
			// AgreedDeliveryDateEdit
			// 
			this.AgreedDeliveryDateEdit.AllowDrop = true;
			this.AgreedDeliveryDateEdit.AutoCompleteMonthThreshold = 1;
			this.AgreedDeliveryDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.AgreedDeliveryDateEdit, "AgreedDeliveryDateLocal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.BufferManagement.Business.MultiJobHeaderEditorViewModel)(null)).AgreedDeliveryDateLocal)));
			this.AgreedDeliveryDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.AgreedDeliveryDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 50, true);
			this.AgreedDeliveryDateEdit.Name = "AgreedDeliveryDateEdit";
			this.AgreedDeliveryDateEdit.TabIndex = 2;
			// 
			// EarliestStartDateEdit
			// 
			this.EarliestStartDateEdit.AllowDrop = true;
			this.EarliestStartDateEdit.AutoCompleteMonthThreshold = 1;
			this.EarliestStartDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.EarliestStartDateEdit, "EarliestStartDateLocal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.BufferManagement.Business.MultiJobHeaderEditorViewModel)(null)).EarliestStartDateLocal)));
			this.EarliestStartDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.EarliestStartDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 19, true);
			this.EarliestStartDateEdit.Name = "EarliestStartDateEdit";
			this.EarliestStartDateEdit.TabIndex = 0;
			// 
			// BulkUpdateGroupBox
			// 
			this.BulkUpdateGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BulkUpdateGroupBox.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("9163a611-d8ac-4a9e-8560-d6a81c1231e3", "Bulk Update");
			this.BulkUpdateGroupBox.Controls.Add(this.IncrementValueGroupBox);
			this.BulkUpdateGroupBox.Controls.Add(this.SetValueGroupBox);
			this.BulkUpdateGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 404, true);
			this.BulkUpdateGroupBox.Name = "BulkUpdateGroupBox";
			this.BulkUpdateGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1032, 219, true);
			this.BulkUpdateGroupBox.TabIndex = 1;
			this.BulkUpdateGroupBox.TabStop = false;
			// 
			// IncrementValueGroupBox
			// 
			this.IncrementValueGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.IncrementValueGroupBox.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("28772498-4eab-4545-871f-e3970519d4db", "Add to existing values");
			this.IncrementValueGroupBox.Controls.Add(this.IncrementADDButton);
			this.IncrementValueGroupBox.Controls.Add(this.IncrementESDButton);
			this.IncrementValueGroupBox.Controls.Add(this.ADDHoursTimeEdit);
			this.IncrementValueGroupBox.Controls.Add(this.ESDHoursTimeEdit);
			this.IncrementValueGroupBox.Controls.Add(this.ADDDaysCalcEdit);
			this.IncrementValueGroupBox.Controls.Add(this.ESDDaysCalcEdit);
			this.IncrementValueGroupBox.Controls.Add(this.IncrementValuesHintLabel);
			this.IncrementValueGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 100, true);
			this.IncrementValueGroupBox.Name = "IncrementValueGroupBox";
			this.IncrementValueGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1020, 113, true);
			this.IncrementValueGroupBox.TabIndex = 5;
			this.IncrementValueGroupBox.TabStop = false;
			// 
			// IncrementADDButton
			// 
			this.IncrementADDButton.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("c6b58b98-33fe-4105-a544-0beeb7efbdd1", "Update Selected Rows");
			this.IncrementADDButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(311, 69, true);
			this.IncrementADDButton.Name = "IncrementADDButton";
			this.IncrementADDButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.IncrementADDButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(141, 19, true);
			this.IncrementADDButton.TabIndex = 15;
			this.IncrementADDButton.UseVisualStyleBackColor = false;
			this.IncrementADDButton.Click += new System.EventHandler(this.IncrementADDButton_Click);
			// 
			// IncrementESDButton
			// 
			this.IncrementESDButton.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("6241b8d2-185d-42b0-b9e8-6b163784008c", "Update Selected Rows");
			this.IncrementESDButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(311, 36, true);
			this.IncrementESDButton.Name = "IncrementESDButton";
			this.IncrementESDButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.IncrementESDButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(141, 20, true);
			this.IncrementESDButton.TabIndex = 12;
			this.IncrementESDButton.UseVisualStyleBackColor = false;
			this.IncrementESDButton.Click += new System.EventHandler(this.IncrementESDButton_Click);
			// 
			// ADDHoursTimeEdit
			// 
			this.ADDHoursTimeEdit.AllowNegative = true;
			this.ADDHoursTimeEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.ADDHoursTimeEdit, "AgreedDeliveryDateOffset");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.BufferManagement.Business.MultiJobHeaderEditorViewModel)(null)).AgreedDeliveryDateOffset)));
			this.ADDHoursTimeEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(263, 69, true);
			this.ADDHoursTimeEdit.Name = "ADDHoursTimeEdit";
			this.ADDHoursTimeEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 20, true);
			this.ADDHoursTimeEdit.TabIndex = 14;
			// 
			// ESDHoursTimeEdit
			// 
			this.ESDHoursTimeEdit.AllowNegative = true;
			this.ESDHoursTimeEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.ESDHoursTimeEdit, "EarliestStartDateOffset");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.BufferManagement.Business.MultiJobHeaderEditorViewModel)(null)).EarliestStartDateOffset)));
			this.ESDHoursTimeEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(263, 36, true);
			this.ESDHoursTimeEdit.Name = "ESDHoursTimeEdit";
			this.ESDHoursTimeEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 20, true);
			this.ESDHoursTimeEdit.TabIndex = 11;
			// 
			// ADDDaysCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ADDDaysCalcEdit, "AgreedDeliveryDateDays");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BufferManagement.Business.MultiJobHeaderEditorViewModel)(null)).AgreedDeliveryDateDays)));
			this.ADDDaysCalcEdit.DecimalPlaces = 0;
			this.ADDDaysCalcEdit.Decimals = 0;
			this.ADDDaysCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 69, true);
			this.ADDDaysCalcEdit.Name = "ADDDaysCalcEdit";
			this.ADDDaysCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			this.ADDDaysCalcEdit.TabIndex = 13;
			this.ADDDaysCalcEdit.Text = "0";
			this.ADDDaysCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ESDDaysCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ESDDaysCalcEdit, "EarliestStartDateDays");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BufferManagement.Business.MultiJobHeaderEditorViewModel)(null)).EarliestStartDateDays)));
			this.ESDDaysCalcEdit.DecimalPlaces = 0;
			this.ESDDaysCalcEdit.Decimals = 0;
			this.ESDDaysCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 36, true);
			this.ESDDaysCalcEdit.Name = "ESDDaysCalcEdit";
			this.ESDDaysCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			this.ESDDaysCalcEdit.TabIndex = 10;
			this.ESDDaysCalcEdit.Text = "0";
			this.ESDDaysCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// IncrementValuesHintLabel
			// 
			this.IncrementValuesHintLabel.AutoSize = true;
			this.IncrementValuesHintLabel.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("8a1e597b-6562-4776-9927-1eccb8c29de4", "Adds the specified amount of time to the selected rows (or the current time, if there is no existing value on a row for that field).");
			this.IncrementValuesHintLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 16, true);
			this.IncrementValuesHintLabel.Name = "IncrementValuesHintLabel";
			this.IncrementValuesHintLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(593, 13, true);
			this.IncrementValuesHintLabel.TabIndex = 0;
			// 
			// SetValueGroupBox
			// 
			this.SetValueGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.SetValueGroupBox.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("44070355-9e95-4789-b918-fc3a2c75ae59", "Set a specific value");
			this.SetValueGroupBox.Controls.Add(this.UpdateDateAcceptabilityButton);
			this.SetValueGroupBox.Controls.Add(this.DateAcceptabilityControl);
			this.SetValueGroupBox.Controls.Add(this.EarliestStartDateEdit);
			this.SetValueGroupBox.Controls.Add(this.UpdateADDButton);
			this.SetValueGroupBox.Controls.Add(this.UpdateESDButton);
			this.SetValueGroupBox.Controls.Add(this.AgreedDeliveryDateEdit);
			this.SetValueGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.SetValueGroupBox.Name = "SetValueGroupBox";
			this.SetValueGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1020, 80, true);
			this.SetValueGroupBox.TabIndex = 4;
			this.SetValueGroupBox.TabStop = false;
			// 
			// UpdateDateAcceptabilityButton
			// 
			this.UpdateDateAcceptabilityButton.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("7e877cfb-788f-444e-a53e-12c538bf9ada", "‎Update Selected Rows", "‎Update Selected Rows", "");
			this.UpdateDateAcceptabilityButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(698, 17, true);
			this.UpdateDateAcceptabilityButton.Name = "UpdateDateAcceptabilityButton";
			this.UpdateDateAcceptabilityButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.UpdateDateAcceptabilityButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(141, 20, true);
			this.UpdateDateAcceptabilityButton.TabIndex = 22;
			this.UpdateDateAcceptabilityButton.UseVisualStyleBackColor = false;
			this.UpdateDateAcceptabilityButton.Click += new System.EventHandler(this.UpdateDateAcceptabilityButton_Click);
			// 
			// DateAcceptabilityControl
			// 
			this.DateAcceptabilityControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DateAcceptabilityControl, ".");
			this.DateAcceptabilityControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(437, 19, true);
			this.DateAcceptabilityControl.Name = "DateAcceptabilityControl";
			this.DateAcceptabilityControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(253, 20, true);
			this.DateAcceptabilityControl.TabIndex = 20;
			// 
			// MultiJobHeaderEditorUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.BulkUpdateGroupBox);
			this.Controls.Add(this.JobHeadersGroupBox);
			this.Name = "MultiJobHeaderEditorUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1032, 626, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.JobHeadersGroupBox.ResumeLayout(false);
			this.JobHeadersGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.SchedulesZGrid)).EndInit();
			this.SchedulesZGrid.ResumeLayout(false);
			this.SchedulesZGrid.PerformLayout();
			this.AgreedDeliveryDateEdit.ResumeLayout(true);
			this.AgreedDeliveryDateEdit.PerformLayout();
			this.EarliestStartDateEdit.ResumeLayout(true);
			this.EarliestStartDateEdit.PerformLayout();
			this.BulkUpdateGroupBox.ResumeLayout(false);
			this.BulkUpdateGroupBox.PerformLayout();
			this.IncrementValueGroupBox.ResumeLayout(false);
			this.IncrementValueGroupBox.PerformLayout();
			this.SetValueGroupBox.ResumeLayout(false);
			this.SetValueGroupBox.PerformLayout();
			this.DateAcceptabilityControl.ResumeLayout(true);
			this.DateAcceptabilityControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox JobHeadersGroupBox;
		private ZArchitecture.ZGrid SchedulesZGrid;
		private ZArchitecture.GUI.ZDateEdit AgreedDeliveryDateEdit;
		private ZArchitecture.GUI.ZDateEdit EarliestStartDateEdit;
		private ZArchitecture.GUI.ZButton UpdateADDButton;
		private ZArchitecture.GUI.ZButton UpdateESDButton;
		private ZArchitecture.GUI.ZGroupBox BulkUpdateGroupBox;
		private ZArchitecture.GUI.ZGroupBox IncrementValueGroupBox;
		private ZArchitecture.ZLabel IncrementValuesHintLabel;
		private ZArchitecture.GUI.ZGroupBox SetValueGroupBox;
		private ZArchitecture.ZCalcEdit ESDDaysCalcEdit;
		private ZArchitecture.ZCalcEdit ADDDaysCalcEdit;
		private ZArchitecture.GUI.ZTimeEditEx ADDHoursTimeEdit;
		private ZArchitecture.GUI.ZTimeEditEx ESDHoursTimeEdit;
		private ZArchitecture.GUI.ZButton IncrementADDButton;
		private ZArchitecture.GUI.ZButton IncrementESDButton;
		private DateAcceptabilityControl DateAcceptabilityControl;
		private ZArchitecture.GUI.ZButton UpdateDateAcceptabilityButton;
	}
}
