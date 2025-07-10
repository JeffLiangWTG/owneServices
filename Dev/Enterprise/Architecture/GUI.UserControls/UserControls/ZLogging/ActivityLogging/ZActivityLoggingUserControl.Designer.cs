using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI
{
	internal partial class ZActivityLoggingUserControl
	{
		#region Component Designer generated code

		private ZGroupBox zGroupBox3;
		private ZCalcEdit zCalcEdit8;
		private ZCalcEdit zCalcEdit9;
		private ZCalcEdit zCalcEdit10;
		private ZLabel zLabel20;
		private ZCalcEdit zCalcEdit6;
		private ZLabel zLabel22;
		private ZCalcEdit zCalcEdit7;
		private ZGroupBox zGroupBox2;
		protected ZButton ClearButton;
		protected ZButton FindButton;
		private ZDateEdit zDateEdit3;
		private ZLabel zLabel15;
		private ZDateEdit zDateEdit4;
		private ZGroupBox zGroupBox1;
		private ZDateEdit zDateEdit2;
		private ZDateEdit zDateEdit1;
		private ZLabel zLabel11;
		private ZCalcEdit zCalcEdit2;
		private ZLabel zLabel10;
		private ZCalcEdit zCalcEdit5;
		private ZCalcEdit zCalcEdit4;
		private ZCalcEdit zCalcEdit3;
		private ZCalcEdit zCalcEdit1;
		private ZTextBox zTextBox2;
		private ZTextBox zTextBox1;
		private ZGrid ActivityLogGrid;
		private ZLabel zLabel3;
		private ZCodeFindBox zCodeFindBox1;
		private System.ComponentModel.IContainer components;

		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ZActivityLoggingUserControl));
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new ZCheckBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new ZCalcEditColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZDateEditColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new ZDateEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZTextBoxColumnStyleInfo();
			this.zGroupBox3 = new ZGroupBox();
			this.zCalcEdit8 = new ZCalcEdit();
			this.zCalcEdit9 = new ZCalcEdit();
			this.zCalcEdit10 = new ZCalcEdit();
			this.zLabel20 = new ZLabel();
			this.zCalcEdit6 = new ZCalcEdit();
			this.zLabel22 = new ZLabel();
			this.zCalcEdit7 = new ZCalcEdit();
			this.zGroupBox2 = new ZGroupBox();
			this.zCodeFindBox1 = new ZCodeFindBox();
			this.zLabel3 = new ZLabel();
			this.ClearButton = new ZButton();
			this.FindButton = new ZButton();
			this.zDateEdit3 = new ZDateEdit();
			this.zLabel15 = new ZLabel();
			this.zDateEdit4 = new ZDateEdit();
			this.zGroupBox1 = new ZGroupBox();
			this.zDateEdit2 = new ZDateEdit();
			this.zDateEdit1 = new ZDateEdit();
			this.zLabel11 = new ZLabel();
			this.zCalcEdit2 = new ZCalcEdit();
			this.zLabel10 = new ZLabel();
			this.zCalcEdit5 = new ZCalcEdit();
			this.zCalcEdit4 = new ZCalcEdit();
			this.zCalcEdit3 = new ZCalcEdit();
			this.zCalcEdit1 = new ZCalcEdit();
			this.zTextBox2 = new ZTextBox();
			this.zTextBox1 = new ZTextBox();
			this.ActivityLogGrid = new ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zGroupBox3.SuspendLayout();
			this.zGroupBox2.SuspendLayout();
			this.zGroupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ActivityLogGrid)).BeginInit();
			this.SuspendLayout();
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(StmActivityLogFilterProvider);
			//
			// zGroupBox3
			//
			this.zGroupBox3.Anchor = ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right);
			this.zGroupBox3.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZActivityLoggingUserControl|45bcaf61-8ae1-4604-b8d5-04fe455894c6", "Totals");
			this.zGroupBox3.Controls.Add(this.zCalcEdit8);
			this.zGroupBox3.Controls.Add(this.zCalcEdit9);
			this.zGroupBox3.Controls.Add(this.zCalcEdit10);
			this.zGroupBox3.Controls.Add(this.zLabel20);
			this.zGroupBox3.Controls.Add(this.zCalcEdit6);
			this.zGroupBox3.Controls.Add(this.zLabel22);
			this.zGroupBox3.Controls.Add(this.zCalcEdit7);
			this.zGroupBox3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 486, true);
			this.zGroupBox3.Name = "zGroupBox3";
			this.zGroupBox3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(808, 62, true);
			this.zGroupBox3.TabIndex = 0;
			this.zGroupBox3.TabStop = false;
			//
			// zCalcEdit8
			//
			this.BindingSource.SetBindingMember(this.zCalcEdit8, "ActivityLogTotalControlChanges");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((StmActivityLogFilterProvider)(null)).ActivityLogTotalControlChanges);
			this.zCalcEdit8.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZActivityLoggingUserControl|99b7425b-798e-4920-9a8f-b8090ebc922d", "Control Changes");
			this.zCalcEdit8.Decimals = 0;
			this.zCalcEdit8.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(483, 36, true);
			this.zCalcEdit8.Name = "zCalcEdit8";
			this.zCalcEdit8.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(44, 20, true);
			this.zCalcEdit8.TabIndex = 11;
			this.zCalcEdit8.Text = "0";
			this.zCalcEdit8.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// zCalcEdit9
			//
			this.BindingSource.SetBindingMember(this.zCalcEdit9, "ActivityLogTotalMouseClicks");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((StmActivityLogFilterProvider)(null)).ActivityLogTotalMouseClicks);
			this.zCalcEdit9.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZActivityLoggingUserControl|53632e32-6347-49b3-9c1a-c98ce606254f", "Mouse Clicks");
			this.zCalcEdit9.Decimals = 0;
			this.zCalcEdit9.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(297, 36, true);
			this.zCalcEdit9.Name = "zCalcEdit9";
			this.zCalcEdit9.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(44, 20, true);
			this.zCalcEdit9.TabIndex = 9;
			this.zCalcEdit9.Text = "0";
			this.zCalcEdit9.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// zCalcEdit10
			//
			this.BindingSource.SetBindingMember(this.zCalcEdit10, "ActivityLogTotalKeyStrokes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((StmActivityLogFilterProvider)(null)).ActivityLogTotalKeyStrokes);
			this.zCalcEdit10.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZActivityLoggingUserControl|0311e63f-49ad-4d29-ba14-fc7e8efd9170", "Key Strokes");
			this.zCalcEdit10.Decimals = 0;
			this.zCalcEdit10.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 36, true);
			this.zCalcEdit10.Name = "zCalcEdit10";
			this.zCalcEdit10.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(44, 20, true);
			this.zCalcEdit10.TabIndex = 7;
			this.zCalcEdit10.Text = "0";
			this.zCalcEdit10.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// zLabel20
			//
			this.zLabel20.AutoSize = true;
			this.zLabel20.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZActivityLoggingUserControl|d41e9869-0c34-450d-8ed1-69ec7c6c1a0f", "minutes");
			this.zLabel20.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(483, 15, true);
			this.zLabel20.Name = "zLabel20";
			this.zLabel20.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.zLabel20.TabIndex = 5;
			//
			// zCalcEdit6
			//
			this.BindingSource.SetBindingMember(this.zCalcEdit6, "ActivityLogTotalInactiveMinutes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((StmActivityLogFilterProvider)(null)).ActivityLogTotalInactiveMinutes);
			this.zCalcEdit6.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZActivityLoggingUserControl|4f947e6b-67f1-4c97-b527-7c07fa36cc9d", "Total Inactive Duration");
			this.zCalcEdit6.Decimals = 1;
			this.zCalcEdit6.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(400, 12, true);
			this.zCalcEdit6.Name = "zCalcEdit6";
			this.zCalcEdit6.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 20, true);
			this.zCalcEdit6.TabIndex = 4;
			this.zCalcEdit6.Text = "0.0";
			this.zCalcEdit6.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// zLabel22
			//
			this.zLabel22.AutoSize = true;
			this.zLabel22.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZActivityLoggingUserControl|ee8d74b4-f4f8-4333-ab58-0271e052a063", "minutes");
			this.zLabel22.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(202, 15, true);
			this.zLabel22.Name = "zLabel22";
			this.zLabel22.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.zLabel22.TabIndex = 2;
			//
			// zCalcEdit7
			//
			this.BindingSource.SetBindingMember(this.zCalcEdit7, "ActivityLogTotalActiveMinutes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((StmActivityLogFilterProvider)(null)).ActivityLogTotalActiveMinutes);
			this.zCalcEdit7.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZActivityLoggingUserControl|dafa3c2e-f405-4daa-9c4f-54c87c1f5a1f", "Total Active Duration");
			this.zCalcEdit7.Decimals = 1;
			this.zCalcEdit7.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 12, true);
			this.zCalcEdit7.Name = "zCalcEdit7";
			this.zCalcEdit7.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 20, true);
			this.zCalcEdit7.TabIndex = 1;
			this.zCalcEdit7.Text = "0.0";
			this.zCalcEdit7.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// zGroupBox2
			//
			this.zGroupBox2.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right);
			this.zGroupBox2.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZActivityLoggingUserControl|a8012add-eef5-4373-bcf1-418238aa96a5", "Filter");
			this.zGroupBox2.Controls.Add(this.zCodeFindBox1);
			this.zGroupBox2.Controls.Add(this.zLabel3);
			this.zGroupBox2.Controls.Add(this.ClearButton);
			this.zGroupBox2.Controls.Add(this.FindButton);
			this.zGroupBox2.Controls.Add(this.zDateEdit3);
			this.zGroupBox2.Controls.Add(this.zLabel15);
			this.zGroupBox2.Controls.Add(this.zDateEdit4);
			this.zGroupBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 0, true);
			this.zGroupBox2.Name = "zGroupBox2";
			this.zGroupBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(808, 67, true);
			this.zGroupBox2.TabIndex = 0;
			this.zGroupBox2.TabStop = false;
			//
			// zCodeFindBox1
			//
			this.BindingSource.SetBindingMember(this.zCodeFindBox1, "ActivityLogFilterStaff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((StmActivityLogFilterProvider)(null)).ActivityLogFilterStaff);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((StmActivityLogFilterProvider)(null)).Staff);
			this.zCodeFindBox1.BindToList = "Staff";
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zCodeFindBox1, false);
			this.zCodeFindBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 39, true);
			this.zCodeFindBox1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbStaff;
			this.zCodeFindBox1.Name = "zCodeFindBox1";
			this.zCodeFindBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(299, 20, true);
			this.zCodeFindBox1.TabIndex = 10;
			//
			// zLabel3
			//
			this.zLabel3.AutoSize = true;
			this.zLabel3.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZActivityLoggingUserControl|a3fc887a-3c80-45fc-af0e-3e1fab94d8aa", "Activity User:");
			this.zLabel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 42, true);
			this.zLabel3.Name = "zLabel3";
			this.zLabel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.zLabel3.TabIndex = 9;
			//
			// ClearButton
			//
			this.ClearButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZActivityLoggingUserControl|cffadb7d-0872-45dc-a421-d04b5b1a3ebd", "Clear");
			this.ClearButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(456, 39, true);
			this.ClearButton.Name = "ClearButton";
			this.ClearButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 23, true);
			this.ClearButton.TabIndex = 12;
			this.ClearButton.UseVisualStyleBackColor = true;
			this.ClearButton.Click += new EventHandler(this.ClearButton_Click);
			//
			// FindButton
			//
			this.FindButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZActivityLoggingUserControl|d437f627-d510-451a-9142-a54188c87eaf", "Find");
			this.FindButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(456, 13, true);
			this.FindButton.Name = "FindButton";
			this.FindButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 23, true);
			this.FindButton.TabIndex = 11;
			this.FindButton.UseVisualStyleBackColor = true;
			this.FindButton.Click += new EventHandler(this.FindButton_Click);
			//
			// zDateEdit3
			//
			this.zDateEdit3.AutoCompleteMonthThreshold = 1;
			this.zDateEdit3.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.zDateEdit3, "ActivityLogFilterDateTo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((StmActivityLogFilterProvider)(null)).ActivityLogFilterDateTo);
			this.zDateEdit3.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZActivityLoggingUserControl|d7032fd8-be43-4c3b-babd-a25c46eae6b6", "To");
			this.zDateEdit3.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.zDateEdit3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(335, 13, true);
			this.zDateEdit3.Name = "zDateEdit3";
			this.zDateEdit3.TabIndex = 4;
			//
			// zLabel15
			//
			this.zLabel15.AutoSize = true;
			this.zLabel15.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZActivityLoggingUserControl|0a7a972d-28f4-4b19-852e-98ec4bd92cd6", "Open Time:");
			this.zLabel15.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 16, true);
			this.zLabel15.Name = "zLabel15";
			this.zLabel15.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.zLabel15.TabIndex = 0;
			//
			// zDateEdit4
			//
			this.zDateEdit4.AutoCompleteMonthThreshold = 1;
			this.zDateEdit4.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.zDateEdit4, "ActivityLogFilterDateFrom");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((StmActivityLogFilterProvider)(null)).ActivityLogFilterDateFrom);
			this.zDateEdit4.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZActivityLoggingUserControl|374a9946-54f5-4562-a996-d541dc12b4be", "From");
			this.zDateEdit4.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.zDateEdit4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 13, true);
			this.zDateEdit4.Name = "zDateEdit4";
			this.zDateEdit4.TabIndex = 2;
			//
			// zGroupBox1
			//
			this.zGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right);
			this.zGroupBox1.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZActivityLoggingUserControl|16faffd2-d0c8-4296-98ba-3999bbf73ec5", "Activity Log Details");
			this.zGroupBox1.Controls.Add(this.zDateEdit2);
			this.zGroupBox1.Controls.Add(this.zDateEdit1);
			this.zGroupBox1.Controls.Add(this.zLabel11);
			this.zGroupBox1.Controls.Add(this.zCalcEdit2);
			this.zGroupBox1.Controls.Add(this.zLabel10);
			this.zGroupBox1.Controls.Add(this.zCalcEdit5);
			this.zGroupBox1.Controls.Add(this.zCalcEdit4);
			this.zGroupBox1.Controls.Add(this.zCalcEdit3);
			this.zGroupBox1.Controls.Add(this.zCalcEdit1);
			this.zGroupBox1.Controls.Add(this.zTextBox2);
			this.zGroupBox1.Controls.Add(this.zTextBox1);
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 378, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(808, 102, true);
			this.zGroupBox1.TabIndex = 3;
			this.zGroupBox1.TabStop = false;
			//
			// zDateEdit2
			//
			this.zDateEdit2.AutoCompleteMonthThreshold = 1;
			this.zDateEdit2.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.zDateEdit2, "Collection.S7_CloseDateTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((StmActivityLog)(((System.Collections.IList)(((StmActivityLogFilterProvider)(null)).Collection)).SyncRoot)).S7_CloseDateTime);
			this.zDateEdit2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.zDateEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(400, 77, true);
			this.zDateEdit2.Name = "zDateEdit2";
			this.zDateEdit2.TabIndex = 13;
			//
			// zDateEdit1
			//
			this.zDateEdit1.AutoCompleteMonthThreshold = 1;
			this.zDateEdit1.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.zDateEdit1, "Collection.S7_OpenDateTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((StmActivityLog)(((System.Collections.IList)(((StmActivityLogFilterProvider)(null)).Collection)).SyncRoot)).S7_OpenDateTime);
			this.zDateEdit1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.zDateEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 77, true);
			this.zDateEdit1.Name = "zDateEdit1";
			this.zDateEdit1.TabIndex = 11;
			//
			// zLabel11
			//
			this.zLabel11.AutoSize = true;
			this.zLabel11.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZActivityLoggingUserControl|267ee151-3e17-4120-8945-e0e9edfd52c8", "minutes");
			this.zLabel11.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(483, 59, true);
			this.zLabel11.Name = "zLabel11";
			this.zLabel11.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.zLabel11.TabIndex = 9;
			//
			// zCalcEdit2
			//
			this.BindingSource.SetBindingMember(this.zCalcEdit2, "Collection.InactiveDurationMinutes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((StmActivityLog)(((System.Collections.IList)(((StmActivityLogFilterProvider)(null)).Collection)).SyncRoot)).InactiveDurationMinutes);
			this.zCalcEdit2.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZActivityLoggingUserControl|f8be4c8e-3ae3-4bba-ba4d-ad93ca1e4c8c", "Inactive Duration");
			this.zCalcEdit2.Decimals = 1;
			this.zCalcEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(400, 56, true);
			this.zCalcEdit2.Name = "zCalcEdit2";
			this.zCalcEdit2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 20, true);
			this.zCalcEdit2.TabIndex = 8;
			this.zCalcEdit2.Text = "0.0";
			this.zCalcEdit2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// zLabel10
			//
			this.zLabel10.AutoSize = true;
			this.zLabel10.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZActivityLoggingUserControl|6d4b9646-f812-42cf-b54f-d28d85ec18ad", "minutes");
			this.zLabel10.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(202, 59, true);
			this.zLabel10.Name = "zLabel10";
			this.zLabel10.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.zLabel10.TabIndex = 6;
			//
			// zCalcEdit5
			//
			this.BindingSource.SetBindingMember(this.zCalcEdit5, "Collection.S7_ControlChanges");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((StmActivityLog)(((System.Collections.IList)(((StmActivityLogFilterProvider)(null)).Collection)).SyncRoot)).S7_ControlChanges);
			this.zCalcEdit5.Decimals = 0;
			this.zCalcEdit5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(739, 56, true);
			this.zCalcEdit5.Name = "zCalcEdit5";
			this.zCalcEdit5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(44, 20, true);
			this.zCalcEdit5.TabIndex = 19;
			this.zCalcEdit5.Text = "0";
			this.zCalcEdit5.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// zCalcEdit4
			//
			this.BindingSource.SetBindingMember(this.zCalcEdit4, "Collection.S7_MouseClicks");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((StmActivityLog)(((System.Collections.IList)(((StmActivityLogFilterProvider)(null)).Collection)).SyncRoot)).S7_MouseClicks);
			this.zCalcEdit4.Decimals = 0;
			this.zCalcEdit4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(739, 35, true);
			this.zCalcEdit4.Name = "zCalcEdit4";
			this.zCalcEdit4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(44, 20, true);
			this.zCalcEdit4.TabIndex = 17;
			this.zCalcEdit4.Text = "0";
			this.zCalcEdit4.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// zCalcEdit3
			//
			this.BindingSource.SetBindingMember(this.zCalcEdit3, "Collection.S7_KeyStrokes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((StmActivityLog)(((System.Collections.IList)(((StmActivityLogFilterProvider)(null)).Collection)).SyncRoot)).S7_KeyStrokes);
			this.zCalcEdit3.Decimals = 0;
			this.zCalcEdit3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(739, 13, true);
			this.zCalcEdit3.Name = "zCalcEdit3";
			this.zCalcEdit3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(44, 20, true);
			this.zCalcEdit3.TabIndex = 15;
			this.zCalcEdit3.Text = "0";
			this.zCalcEdit3.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// zCalcEdit1
			//
			this.BindingSource.SetBindingMember(this.zCalcEdit1, "Collection.ActiveDurationMinutes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((StmActivityLog)(((System.Collections.IList)(((StmActivityLogFilterProvider)(null)).Collection)).SyncRoot)).ActiveDurationMinutes);
			this.zCalcEdit1.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZActivityLoggingUserControl|95e7ecd9-3dcf-4314-86ed-eadf4be5465a", "Active Duration");
			this.zCalcEdit1.Decimals = 1;
			this.zCalcEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 56, true);
			this.zCalcEdit1.Name = "zCalcEdit1";
			this.zCalcEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 20, true);
			this.zCalcEdit1.TabIndex = 5;
			this.zCalcEdit1.Text = "0.0";
			this.zCalcEdit1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// zTextBox2
			//
			this.BindingSource.SetBindingMember(this.zTextBox2, "Collection.S7_ControllerID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((StmActivityLog)(((System.Collections.IList)(((StmActivityLogFilterProvider)(null)).Collection)).SyncRoot)).S7_ControllerID);
			this.zTextBox2.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.zTextBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 34, true);
			this.zTextBox2.Name = "zTextBox2";
			this.zTextBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(396, 20, true);
			this.zTextBox2.TabIndex = 3;
			//
			// zTextBox1
			//
			this.BindingSource.SetBindingMember(this.zTextBox1, "Collection.S7_FormCaption");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((StmActivityLog)(((System.Collections.IList)(((StmActivityLogFilterProvider)(null)).Collection)).SyncRoot)).S7_FormCaption);
			this.zTextBox1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.zTextBox1.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZActivityLoggingUserControl|3bfb464a-4521-4838-a307-358fa6750deb", "Form Caption");
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 13, true);
			this.zTextBox1.Name = "zTextBox1";
			this.zTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(396, 20, true);
			this.zTextBox1.TabIndex = 1;
			//
			// ActivityLogGrid
			//
			this.ActivityLogGrid.AllowNavigation = false;
			this.ActivityLogGrid.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right);
			this.BindingSource.SetBindingMember(this.ActivityLogGrid, "Collection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((StmActivityLogFilterProvider)(null)).Collection);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((StmActivityLog)(((System.Collections.IList)(((StmActivityLogFilterProvider)(null)).Collection)).SyncRoot)).S7_FormCaption);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((StmActivityLog)(((System.Collections.IList)(((StmActivityLogFilterProvider)(null)).Collection)).SyncRoot)).S7_ControllerID);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((StmActivityLog)(((System.Collections.IList)(((StmActivityLogFilterProvider)(null)).Collection)).SyncRoot)).S7_EnterpriseActivity);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((StmActivityLog)(((System.Collections.IList)(((StmActivityLogFilterProvider)(null)).Collection)).SyncRoot)).S7_KeyStrokes);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((StmActivityLog)(((System.Collections.IList)(((StmActivityLogFilterProvider)(null)).Collection)).SyncRoot)).S7_MouseClicks);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((StmActivityLog)(((System.Collections.IList)(((StmActivityLogFilterProvider)(null)).Collection)).SyncRoot)).S7_ControlChanges);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((StmActivityLog)(((System.Collections.IList)(((StmActivityLogFilterProvider)(null)).Collection)).SyncRoot)).ActiveDurationMinutes);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((StmActivityLog)(((System.Collections.IList)(((StmActivityLogFilterProvider)(null)).Collection)).SyncRoot)).InactiveDurationMinutes);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((StmActivityLog)(((System.Collections.IList)(((StmActivityLogFilterProvider)(null)).Collection)).SyncRoot)).S7_OpenDateTime);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((StmActivityLog)(((System.Collections.IList)(((StmActivityLogFilterProvider)(null)).Collection)).SyncRoot)).S7_CloseDateTime);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((StmActivityLog)(((System.Collections.IList)(((StmActivityLogFilterProvider)(null)).Collection)).SyncRoot)).S7_GS_NKUser);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((StmActivityLog)(((System.Collections.IList)(((StmActivityLogFilterProvider)(null)).Collection)).SyncRoot)).UserFullName);
			this.ActivityLogGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "S7_FormCaption";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zTextBoxColumnStyleInfo2.ColumnName = "S7_ControllerID";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zCheckBoxColumnStyleInfo1.ColumnName = "S7_EnterpriseActivity";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZActivityLoggingUserControl|384373e7-1250-4644-8034-c8f54bc07205", "Keys");
			zCalcEditColumnStyleInfo1.ColumnName = "S7_KeyStrokes";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZActivityLoggingUserControl|dd22e979-c0b2-45e2-8145-114817f0832f", "Mouse");
			zCalcEditColumnStyleInfo2.ColumnName = "S7_MouseClicks";
			zCalcEditColumnStyleInfo2.Decimals = 0;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZActivityLoggingUserControl|f48772c2-bb3e-420b-9d28-289291850ebb", "Change");
			zCalcEditColumnStyleInfo3.ColumnName = "S7_ControlChanges";
			zCalcEditColumnStyleInfo3.Decimals = 0;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZActivityLoggingUserControl|0c8de398-58ad-4da5-b1ec-c9d1f22a8388", "Active");
			zCalcEditColumnStyleInfo4.ColumnName = "ActiveDurationMinutes";
			zCalcEditColumnStyleInfo4.Decimals = 1;
			zCalcEditColumnStyleInfo4.ToolTip = "Active duration in minutes";
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = "";
			zCalcEditColumnStyleInfo5.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZActivityLoggingUserControl|91ff074a-030d-47b8-9239-6f768ffa79ef", "Inactive");
			zCalcEditColumnStyleInfo5.ColumnName = "InactiveDurationMinutes";
			zCalcEditColumnStyleInfo5.Decimals = 1;
			zCalcEditColumnStyleInfo5.ToolTip = "Inactive duration in minutes";
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDateEditColumnStyleInfo1.ColumnName = "S7_OpenDateTime";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateEditColumnStyleInfo2.ColumnName = "S7_CloseDateTime";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo3.ColumnName = "S7_GS_NKUser";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZActivityLoggingUserControl|ea6d732e-1311-487f-9373-76274762ba12", "User (Full Name)");
			zTextBoxColumnStyleInfo4.ColumnName = "UserFullName";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo4.IsVisible = false;
			this.ActivityLogGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ActivityLogGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ActivityLogGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.ActivityLogGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ActivityLogGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.ActivityLogGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.ActivityLogGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.ActivityLogGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.ActivityLogGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.ActivityLogGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.ActivityLogGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ActivityLogGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ActivityLogGrid.GridId = "80238a50-900b-4e43-bd90-08125ab97d2e";
			this.ActivityLogGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ActivityLogGrid.IsWholeRowSelectedOnClick = true;
			this.ActivityLogGrid.LayoutKey = "zGrid2";
			this.ActivityLogGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 73, true);
			this.ActivityLogGrid.Name = "ActivityLogGrid";
			this.ActivityLogGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(808, 299, true);
			this.ActivityLogGrid.TabIndex = 1;
			//
			// ZActivityLoggingUserControl
			//
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.zGroupBox3);
			this.Controls.Add(this.zGroupBox2);
			this.Controls.Add(this.zGroupBox1);
			this.Controls.Add(this.ActivityLogGrid);
			this.Name = "ZActivityLoggingUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(828, 557, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zGroupBox3.ResumeLayout(false);
			this.zGroupBox3.PerformLayout();
			this.zGroupBox2.ResumeLayout(false);
			this.zGroupBox2.PerformLayout();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ActivityLogGrid)).EndInit();
			this.ResumeLayout(false);
		}
		#endregion
	}
}
