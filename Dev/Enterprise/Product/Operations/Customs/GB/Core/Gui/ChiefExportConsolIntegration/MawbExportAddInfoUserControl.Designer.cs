namespace Enterprise.Customs.GB.GUI.Ccsuk.CcsukAirInventory
{
	partial class MawbExportAddInfoUserControl
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
            this.MasterUCRTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.RouteDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.SOEDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.MovementReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.GoodsLocationTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.CustomsReturnCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.EPUIdTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.EPUNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.ShedTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.GoodsArrivalDateTimeDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.ChiefGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.MucrCalcButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.ChiefCustomsActionDateFromFsnDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.ChiefCustomsActionTextFromFsnTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.TextBoxCAC = new Enterprise.ZArchitecture.ZTextBox();
            this.ConsolIsClosedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.CSRDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.MessagingGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.ME_TransportIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.CTStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.ME_TransportModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.UseAntiSmugglingCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.ME_TransportCountryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.MasterOptDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.PartIndicatorCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.ME_ExportLocationDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.ME_ExportShedDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.ME_ProfileDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.MovementDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.ccsukMessagesUserControl1 = new Enterprise.Customs.GB.GUI.Ccsuk.CcsukMessagesUserControl();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.RouteDropEdit.SuspendLayout();
            this.SOEDropEdit.SuspendLayout();
            this.CustomsReturnCodeDropEdit.SuspendLayout();
            this.GoodsArrivalDateTimeDateEdit.SuspendLayout();
            this.ChiefGroupBox.SuspendLayout();
            this.ChiefCustomsActionDateFromFsnDateEdit.SuspendLayout();
            this.CSRDropEdit.SuspendLayout();
            this.MessagingGroupBox.SuspendLayout();
            this.CTStatusDropEdit.SuspendLayout();
            this.ME_TransportModeDropEdit.SuspendLayout();
            this.ME_TransportCountryCodeFindBox.SuspendLayout();
            this.MasterOptDropEdit.SuspendLayout();
            this.ME_ExportLocationDropEdit.SuspendLayout();
            this.ME_ExportShedDropEdit.SuspendLayout();
            this.ME_ProfileDropEdit.SuspendLayout();
            this.MovementDateDateEdit.SuspendLayout();
            this.ccsukMessagesUserControl1.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration.MawbExportAddInfo);
            // 
            // MasterUCRTextBox
            // 
            this.BindingSource.SetBindingMember(this.MasterUCRTextBox, "ME_MasterUCR");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration.MawbExportAddInfo)(null)).ME_MasterUCR)));
            this.MasterUCRTextBox.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("MawbExportAddInfoUserControl|684ca2b3-1cca-4de0-a7c0-b2a177115336", "MUCR");
            this.MasterUCRTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 19, true);
            this.MasterUCRTextBox.Name = "MasterUCRTextBox";
            this.MasterUCRTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(134, 17, true);
            this.MasterUCRTextBox.TabIndex = 0;
            // 
            // RouteDropEdit
            // 
            this.RouteDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.RouteDropEdit, "ME_ChiefMasterRouteOfEntryConvertedToEnterpriseForBinding");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration.MawbExportAddInfo)(null)).ME_ChiefMasterRouteOfEntryConvertedToEnterpriseForBinding)));
            this.RouteDropEdit.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("MawbExportAddInfoUserControl|80502562-4eb0-43ed-a8a6-f335eaca6f37", "ROE", "Route", "Master Route of Entry on CHIEF.");
            this.RouteDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 43, true);
            this.RouteDropEdit.Name = "RouteDropEdit";
            this.RouteDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(186, 17, true);
            this.RouteDropEdit.TabIndex = 2;
            // 
            // SOEDropEdit
            // 
            this.SOEDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.SOEDropEdit, "ME_ChiefMasterStyleOfEntry");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration.MawbExportAddInfo)(null)).ME_ChiefMasterStyleOfEntry)));
            this.SOEDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 67, true);
            this.SOEDropEdit.Name = "SOEDropEdit";
            this.SOEDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(186, 17, true);
            this.SOEDropEdit.TabIndex = 3;
            // 
            // MovementReferenceTextBox
            // 
            this.BindingSource.SetBindingMember(this.MovementReferenceTextBox, "ME_ChiefMovementReference");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration.MawbExportAddInfo)(null)).ME_ChiefMovementReference)));
            this.MovementReferenceTextBox.CaptionResourceString = null;
            this.MovementReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 115, true);
            this.MovementReferenceTextBox.Name = "MovementReferenceTextBox";
            this.MovementReferenceTextBox.ReadOnly = true;
            this.MovementReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
            this.MovementReferenceTextBox.TabIndex = 5;
            // 
            // GoodsLocationTextBox
            // 
            this.BindingSource.SetBindingMember(this.GoodsLocationTextBox, "ME_ChiefGoodsLocation");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration.MawbExportAddInfo)(null)).ME_ChiefGoodsLocation)));
            this.GoodsLocationTextBox.CaptionResourceString = null;
            this.GoodsLocationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 139, true);
            this.GoodsLocationTextBox.Name = "GoodsLocationTextBox";
            this.GoodsLocationTextBox.ReadOnly = true;
            this.GoodsLocationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 17, true);
            this.GoodsLocationTextBox.TabIndex = 6;
            // 
            // CustomsReturnCodeDropEdit
            // 
            this.CustomsReturnCodeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.CustomsReturnCodeDropEdit, "ME_ChiefCustomsReturnCode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration.MawbExportAddInfo)(null)).ME_ChiefCustomsReturnCode)));
            this.CustomsReturnCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 91, true);
            this.CustomsReturnCodeDropEdit.Name = "CustomsReturnCodeDropEdit";
            this.CustomsReturnCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(186, 17, true);
            this.CustomsReturnCodeDropEdit.TabIndex = 4;
            // 
            // EPUIdTextBox
            // 
            this.BindingSource.SetBindingMember(this.EPUIdTextBox, "ME_ChiefEntryProcessingUnitID");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration.MawbExportAddInfo)(null)).ME_ChiefEntryProcessingUnitID)));
            this.EPUIdTextBox.CaptionResourceString = null;
            this.EPUIdTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(212, 165, true);
            this.EPUIdTextBox.Name = "EPUIdTextBox";
            this.EPUIdTextBox.ReadOnly = true;
            this.EPUIdTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 17, true);
            this.EPUIdTextBox.TabIndex = 9;
            // 
            // EPUNoTextBox
            // 
            this.BindingSource.SetBindingMember(this.EPUNoTextBox, "ME_ChiefEntryProcessingUnitNumber");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration.MawbExportAddInfo)(null)).ME_ChiefEntryProcessingUnitNumber)));
            this.EPUNoTextBox.CaptionResourceString = null;
            this.EPUNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 165, true);
            this.EPUNoTextBox.Name = "EPUNoTextBox";
            this.EPUNoTextBox.ReadOnly = true;
            this.EPUNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 17, true);
            this.EPUNoTextBox.TabIndex = 8;
            // 
            // ShedTextBox
            // 
            this.BindingSource.SetBindingMember(this.ShedTextBox, "ME_ChiefShed");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration.MawbExportAddInfo)(null)).ME_ChiefShed)));
            this.ShedTextBox.CaptionResourceString = null;
            this.ShedTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(212, 139, true);
            this.ShedTextBox.Name = "ShedTextBox";
            this.ShedTextBox.ReadOnly = true;
            this.ShedTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 17, true);
            this.ShedTextBox.TabIndex = 7;
            // 
            // GoodsArrivalDateTimeDateEdit
            // 
            this.GoodsArrivalDateTimeDateEdit.AllowDrop = true;
            this.GoodsArrivalDateTimeDateEdit.AutoCompleteMonthThreshold = 1;
            this.GoodsArrivalDateTimeDateEdit.AutoCompleteYear = true;
            this.BindingSource.SetBindingMember(this.GoodsArrivalDateTimeDateEdit, "ME_ChiefGoodsArrivalDateTime");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration.MawbExportAddInfo)(null)).ME_ChiefGoodsArrivalDateTime)));
            this.GoodsArrivalDateTimeDateEdit.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("MawbExportAddInfoUserControl|d7f4e0a3-2ab0-49dc-b526-9431652e246e", "Arrival Date", "Goods\' Arrival Date on CHIEF.");
            this.GoodsArrivalDateTimeDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
            this.GoodsArrivalDateTimeDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 191, true);
            this.GoodsArrivalDateTimeDateEdit.Name = "GoodsArrivalDateTimeDateEdit";
            this.GoodsArrivalDateTimeDateEdit.TabIndex = 10;
            // 
            // ChiefGroupBox
            // 
            this.ChiefGroupBox.Controls.Add(this.MucrCalcButton);
            this.ChiefGroupBox.Controls.Add(this.ChiefCustomsActionDateFromFsnDateEdit);
            this.ChiefGroupBox.Controls.Add(this.ChiefCustomsActionTextFromFsnTextBox);
            this.ChiefGroupBox.Controls.Add(this.TextBoxCAC);
            this.ChiefGroupBox.Controls.Add(this.SOEDropEdit);
            this.ChiefGroupBox.Controls.Add(this.GoodsArrivalDateTimeDateEdit);
            this.ChiefGroupBox.Controls.Add(this.MasterUCRTextBox);
            this.ChiefGroupBox.Controls.Add(this.ShedTextBox);
            this.ChiefGroupBox.Controls.Add(this.RouteDropEdit);
            this.ChiefGroupBox.Controls.Add(this.EPUNoTextBox);
            this.ChiefGroupBox.Controls.Add(this.MovementReferenceTextBox);
            this.ChiefGroupBox.Controls.Add(this.EPUIdTextBox);
            this.ChiefGroupBox.Controls.Add(this.GoodsLocationTextBox);
            this.ChiefGroupBox.Controls.Add(this.CustomsReturnCodeDropEdit);
            this.ChiefGroupBox.Controls.Add(this.ConsolIsClosedCheckBox);
            this.ChiefGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
            this.ChiefGroupBox.Name = "ChiefGroupBox";
            this.ChiefGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(274, 290, true);
            this.ChiefGroupBox.TabIndex = 0;
            this.ChiefGroupBox.TabStop = false;
            this.ChiefGroupBox.Text = "Last Data From Customs/CCSUK";
			// 
			// MucrCalcButton
			//

			//this.MucrCalcButton.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("MawbExportAddInfoUserControl|684ca2b3-1cca-4de0-a7c0-b2a177115621", "Calc");
			this.MucrCalcButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(219, 17, true);
			this.MucrCalcButton.Name = "ButtonFind";
			this.MucrCalcButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true);
			this.MucrCalcButton.TabIndex = 1;
			this.MucrCalcButton.Text = "Calc";
			this.MucrCalcButton.UseVisualStyleBackColor = true;
			this.MucrCalcButton.Click += new System.EventHandler(this.MucrCalcButton_Click);
            // 
            // ChiefCustomsActionDateFromFsnDateEdit
            // 
            this.ChiefCustomsActionDateFromFsnDateEdit.AllowDrop = true;
            this.ChiefCustomsActionDateFromFsnDateEdit.AutoCompleteMonthThreshold = 1;
            this.ChiefCustomsActionDateFromFsnDateEdit.AutoCompleteYear = true;
            this.BindingSource.SetBindingMember(this.ChiefCustomsActionDateFromFsnDateEdit, "ME_ChiefCustomsActionDateFromFsn");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration.MawbExportAddInfo)(null)).ME_ChiefCustomsActionDateFromFsn)));
            this.ChiefCustomsActionDateFromFsnDateEdit.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("b1c526f0-e53c-4f0d-afbb-0162b1115727", "FSN Date", "Date of Customs Action Code from FSN Message");
            this.ChiefCustomsActionDateFromFsnDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
            this.ChiefCustomsActionDateFromFsnDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 243, true);
            this.ChiefCustomsActionDateFromFsnDateEdit.Name = "ChiefCustomsActionDateFromFsnDateEdit";
            this.ChiefCustomsActionDateFromFsnDateEdit.TabIndex = 13;
            // 
            // ChiefCustomsActionTextFromFsnTextBox
            // 
            this.BindingSource.SetBindingMember(this.ChiefCustomsActionTextFromFsnTextBox, "ME_ChiefCustomsActionTextFromFsn");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration.MawbExportAddInfo)(null)).ME_ChiefCustomsActionTextFromFsn)));
            this.ChiefCustomsActionTextFromFsnTextBox.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("bdb398bc-04f5-48a3-85e9-a77877c3dadb", "CAT", "Customs Action Text from Last FSN Message");
            this.ChiefCustomsActionTextFromFsnTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 217, true);
            this.ChiefCustomsActionTextFromFsnTextBox.Name = "ChiefCustomsActionTextFromFsnTextBox";
            this.ChiefCustomsActionTextFromFsnTextBox.ReadOnly = true;
            this.ChiefCustomsActionTextFromFsnTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(148, 17, true);
            this.ChiefCustomsActionTextFromFsnTextBox.TabIndex = 12;
            // 
            // TextBoxCAC
            // 
            this.BindingSource.SetBindingMember(this.TextBoxCAC, "ME_ChiefCustomsActionCodeFromFsn");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration.MawbExportAddInfo)(null)).ME_ChiefCustomsActionCodeFromFsn)));
            this.TextBoxCAC.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("d1cf6597-4ad1-4936-9f3b-7f739aa9879c", "FSN", "Customs Action Code", "FSN Action Code", "Customs Action Code from Last FSN Message");
            this.TextBoxCAC.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 217, true);
            this.TextBoxCAC.Name = "TextBoxCAC";
            this.TextBoxCAC.ReadOnly = true;
            this.TextBoxCAC.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(29, 17, true);
            this.TextBoxCAC.TabIndex = 11;
            // 
            // ConsolIsClosedCheckBox
            // 
            this.BindingSource.SetBindingMember(this.ConsolIsClosedCheckBox, "ME_ChiefConsolIsClosed");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration.MawbExportAddInfo)(null)).ME_ChiefConsolIsClosed)));
            this.ConsolIsClosedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.ConsolIsClosedCheckBox.ForeColor = System.Drawing.SystemColors.GrayText;
            this.ConsolIsClosedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 265, true);
            this.ConsolIsClosedCheckBox.Name = "ConsolIsClosedCheckBox";
            this.ConsolIsClosedCheckBox.ReadOnly = true;
            this.ConsolIsClosedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 19, true);
            this.ConsolIsClosedCheckBox.TabIndex = 14;
            this.ConsolIsClosedCheckBox.Text = "Master is closed?";
            this.ConsolIsClosedCheckBox.UseVisualStyleBackColor = true;
            // 
            // CSRDropEdit
            // 
            this.CSRDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.CSRDropEdit, "ME_CustomsStatisticalReference");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration.MawbExportAddInfo)(null)).ME_CustomsStatisticalReference)));
            this.CSRDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 19, true);
            this.CSRDropEdit.Name = "CSRDropEdit";
            this.CSRDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(304, 17, true);
            this.CSRDropEdit.TabIndex = 0;
            // 
            // MessagingGroupBox
            // 
            this.MessagingGroupBox.Controls.Add(this.ME_TransportIDTextBox);
            this.MessagingGroupBox.Controls.Add(this.CTStatusDropEdit);
            this.MessagingGroupBox.Controls.Add(this.ME_TransportModeDropEdit);
            this.MessagingGroupBox.Controls.Add(this.UseAntiSmugglingCheckBox);
            this.MessagingGroupBox.Controls.Add(this.ME_TransportCountryCodeFindBox);
            this.MessagingGroupBox.Controls.Add(this.MasterOptDropEdit);
            this.MessagingGroupBox.Controls.Add(this.PartIndicatorCheckBox);
            this.MessagingGroupBox.Controls.Add(this.ME_ExportLocationDropEdit);
            this.MessagingGroupBox.Controls.Add(this.ME_ExportShedDropEdit);
            this.MessagingGroupBox.Controls.Add(this.ME_ProfileDropEdit);
            this.MessagingGroupBox.Controls.Add(this.MovementDateDateEdit);
            this.MessagingGroupBox.Controls.Add(this.CSRDropEdit);
            this.MessagingGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(284, 3, true);
            this.MessagingGroupBox.Name = "MessagingGroupBox";
            this.MessagingGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(421, 290, true);
            this.MessagingGroupBox.TabIndex = 12;
            this.MessagingGroupBox.TabStop = false;
            this.MessagingGroupBox.Text = "Data for Customs Messaging and Documents";
            // 
            // ME_TransportIDTextBox
            // 
            this.BindingSource.SetBindingMember(this.ME_TransportIDTextBox, "ME_TransportID");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration.MawbExportAddInfo)(null)).ME_TransportID)));
            this.ME_TransportIDTextBox.CaptionResourceString = null;
            this.ME_TransportIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 180, true);
            this.ME_TransportIDTextBox.Name = "ME_TransportIDTextBox";
            this.ME_TransportIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
            this.ME_TransportIDTextBox.TabIndex = 7;
            // 
            // CTStatusDropEdit
            // 
            this.CTStatusDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.CTStatusDropEdit, "ME_CommunityTransitStatus");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration.MawbExportAddInfo)(null)).ME_CommunityTransitStatus)));
            this.CTStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 261, true);
            this.CTStatusDropEdit.Name = "CTStatusDropEdit";
            this.CTStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(298, 17, true);
            this.CTStatusDropEdit.TabIndex = 11;
            // 
            // ME_TransportModeDropEdit
            // 
            this.ME_TransportModeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ME_TransportModeDropEdit, "ME_TransportMode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration.MawbExportAddInfo)(null)).ME_TransportMode)));
            this.ME_TransportModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 234, true);
            this.ME_TransportModeDropEdit.Name = "ME_TransportModeDropEdit";
            this.ME_TransportModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(298, 17, true);
            this.ME_TransportModeDropEdit.TabIndex = 10;
            // 
            // UseAntiSmugglingCheckBox
            // 
            this.BindingSource.SetBindingMember(this.UseAntiSmugglingCheckBox, "ME_UseAntiSmugglingTrptid");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration.MawbExportAddInfo)(null)).ME_UseAntiSmugglingTrptid)));
            this.UseAntiSmugglingCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.UseAntiSmugglingCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 154, true);
            this.UseAntiSmugglingCheckBox.Name = "UseAntiSmugglingCheckBox";
            this.UseAntiSmugglingCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 19, true);
            this.UseAntiSmugglingCheckBox.TabIndex = 6;
            this.UseAntiSmugglingCheckBox.Text = "Use Anti-Smuggling Format For Transport ID";
            this.UseAntiSmugglingCheckBox.UseVisualStyleBackColor = true;
            // 
            // ME_TransportCountryCodeFindBox
            // 
            this.ME_TransportCountryCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ME_TransportCountryCodeFindBox, "ME_TransportCountry");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration.MawbExportAddInfo)(null)).ME_TransportCountry)));
            this.ME_TransportCountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 207, true);
            this.ME_TransportCountryCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
            this.ME_TransportCountryCodeFindBox.Name = "ME_TransportCountryCodeFindBox";
            this.ME_TransportCountryCodeFindBox.ShouldResize = true;
            this.ME_TransportCountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(298, 17, true);
            this.ME_TransportCountryCodeFindBox.TabIndex = 9;
            // 
            // MasterOptDropEdit
            // 
            this.MasterOptDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.MasterOptDropEdit, "ME_MasterOpt");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration.MawbExportAddInfo)(null)).ME_MasterOpt)));
            this.MasterOptDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 100, true);
            this.MasterOptDropEdit.Name = "MasterOptDropEdit";
            this.MasterOptDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(304, 17, true);
            this.MasterOptDropEdit.TabIndex = 4;
            // 
            // PartIndicatorCheckBox
            // 
            this.BindingSource.SetBindingMember(this.PartIndicatorCheckBox, "ME_PartMovementIndicator");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration.MawbExportAddInfo)(null)).ME_PartMovementIndicator)));
            this.PartIndicatorCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.PartIndicatorCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(228, 181, true);
            this.PartIndicatorCheckBox.Name = "PartIndicatorCheckBox";
            this.PartIndicatorCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(181, 19, true);
            this.PartIndicatorCheckBox.TabIndex = 8;
            this.PartIndicatorCheckBox.Text = "Part Consignment Only";
            this.PartIndicatorCheckBox.UseVisualStyleBackColor = true;
            // 
            // ME_ExportLocationDropEdit
            // 
            this.ME_ExportLocationDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ME_ExportLocationDropEdit, "ME_ExportLocation");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration.MawbExportAddInfo)(null)).ME_ExportLocation)));
            this.ME_ExportLocationDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 73, true);
            this.ME_ExportLocationDropEdit.Name = "ME_ExportLocationDropEdit";
            this.ME_ExportLocationDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 17, true);
            this.ME_ExportLocationDropEdit.TabIndex = 2;
            // 
            // ME_ExportShedDropEdit
            // 
            this.ME_ExportShedDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ME_ExportShedDropEdit, "ME_ExportShed");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration.MawbExportAddInfo)(null)).ME_ExportShed)));
            this.ME_ExportShedDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(261, 73, true);
            this.ME_ExportShedDropEdit.Name = "ME_ExportShedDropEdit";
            this.ME_ExportShedDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(154, 17, true);
            this.ME_ExportShedDropEdit.TabIndex = 3;
            // 
            // ME_ProfileDropEdit
            // 
            this.ME_ProfileDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ME_ProfileDropEdit, "ME_Profile");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration.MawbExportAddInfo)(null)).ME_Profile)));
            this.ME_ProfileDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 46, true);
            this.ME_ProfileDropEdit.Name = "ME_ProfileDropEdit";
            this.ME_ProfileDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(304, 17, true);
            this.ME_ProfileDropEdit.TabIndex = 1;
            // 
            // MovementDateDateEdit
            // 
            this.MovementDateDateEdit.AllowDrop = true;
            this.MovementDateDateEdit.AutoCompleteMonthThreshold = 1;
            this.MovementDateDateEdit.AutoCompleteYear = true;
            this.BindingSource.SetBindingMember(this.MovementDateDateEdit, "ME_MovementDate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration.MawbExportAddInfo)(null)).ME_MovementDate)));
            this.MovementDateDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
            this.MovementDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 127, true);
            this.MovementDateDateEdit.Name = "MovementDateDateEdit";
            this.MovementDateDateEdit.TabIndex = 5;
            // 
            // ccsukMessagesUserControl1
            // 
            this.ccsukMessagesUserControl1.AllowDrop = true;
            this.ccsukMessagesUserControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.ccsukMessagesUserControl1, ".");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb)(((Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration.MawbExportAddInfo)(null)))));
            this.ccsukMessagesUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 299, true);
            this.ccsukMessagesUserControl1.Name = "ccsukMessagesUserControl1";
            this.ccsukMessagesUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(701, 318, true);
            this.ccsukMessagesUserControl1.TabIndex = 13;
            // 
            // MawbExportAddInfoUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.ccsukMessagesUserControl1);
            this.Controls.Add(this.MessagingGroupBox);
            this.Controls.Add(this.ChiefGroupBox);
            this.Name = "MawbExportAddInfoUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(709, 620, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.RouteDropEdit.ResumeLayout(true);
            this.RouteDropEdit.PerformLayout();
            this.SOEDropEdit.ResumeLayout(true);
            this.SOEDropEdit.PerformLayout();
            this.CustomsReturnCodeDropEdit.ResumeLayout(true);
            this.CustomsReturnCodeDropEdit.PerformLayout();
            this.GoodsArrivalDateTimeDateEdit.ResumeLayout(true);
            this.GoodsArrivalDateTimeDateEdit.PerformLayout();
            this.ChiefGroupBox.ResumeLayout(false);
            this.ChiefGroupBox.PerformLayout();
            this.ChiefCustomsActionDateFromFsnDateEdit.ResumeLayout(true);
            this.ChiefCustomsActionDateFromFsnDateEdit.PerformLayout();
            this.CSRDropEdit.ResumeLayout(true);
            this.CSRDropEdit.PerformLayout();
            this.MessagingGroupBox.ResumeLayout(false);
            this.MessagingGroupBox.PerformLayout();
            this.CTStatusDropEdit.ResumeLayout(true);
            this.CTStatusDropEdit.PerformLayout();
            this.ME_TransportModeDropEdit.ResumeLayout(true);
            this.ME_TransportModeDropEdit.PerformLayout();
            this.ME_TransportCountryCodeFindBox.ResumeLayout(true);
            this.ME_TransportCountryCodeFindBox.PerformLayout();
            this.MasterOptDropEdit.ResumeLayout(true);
            this.MasterOptDropEdit.PerformLayout();
            this.ME_ExportLocationDropEdit.ResumeLayout(true);
            this.ME_ExportLocationDropEdit.PerformLayout();
            this.ME_ExportShedDropEdit.ResumeLayout(true);
            this.ME_ExportShedDropEdit.PerformLayout();
            this.ME_ProfileDropEdit.ResumeLayout(true);
            this.ME_ProfileDropEdit.PerformLayout();
            this.MovementDateDateEdit.ResumeLayout(true);
            this.MovementDateDateEdit.PerformLayout();
            this.ccsukMessagesUserControl1.ResumeLayout(true);
            this.ccsukMessagesUserControl1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZTextBox MasterUCRTextBox;
		private ZArchitecture.GUI.ZDropEdit RouteDropEdit;
		private ZArchitecture.GUI.ZDropEdit SOEDropEdit;
		private ZArchitecture.ZTextBox MovementReferenceTextBox;
		private ZArchitecture.ZTextBox GoodsLocationTextBox;
		private ZArchitecture.GUI.ZDropEdit CustomsReturnCodeDropEdit;
		private ZArchitecture.ZTextBox EPUIdTextBox;
		private ZArchitecture.ZTextBox EPUNoTextBox;
		private ZArchitecture.ZTextBox ShedTextBox;
		private ZArchitecture.GUI.ZDateEdit GoodsArrivalDateTimeDateEdit;
		private ZArchitecture.GUI.ZGroupBox ChiefGroupBox;
		private ZArchitecture.GUI.ZDropEdit CSRDropEdit;
		private ZArchitecture.GUI.ZGroupBox MessagingGroupBox;
		private ZArchitecture.GUI.ZDateEdit MovementDateDateEdit;
		private ZArchitecture.GUI.ZDropEdit ME_ProfileDropEdit;
		private ZArchitecture.ZTextBox ME_TransportIDTextBox;
		private ZArchitecture.GUI.ZDropEdit ME_ExportShedDropEdit;
		private ZArchitecture.GUI.ZDropEdit ME_ExportLocationDropEdit;
		private ZArchitecture.GUI.ZDropEdit ME_TransportModeDropEdit;
		private ZArchitecture.GUI.ZCodeFindBox ME_TransportCountryCodeFindBox;
		private CcsukMessagesUserControl ccsukMessagesUserControl1;
		private ZArchitecture.GUI.ZCheckBox UseAntiSmugglingCheckBox;
		private ZArchitecture.GUI.ZCheckBox ConsolIsClosedCheckBox;
		private ZArchitecture.GUI.ZDropEdit MasterOptDropEdit;
		private ZArchitecture.GUI.ZCheckBox PartIndicatorCheckBox;
		private ZArchitecture.GUI.ZDropEdit CTStatusDropEdit;
		private ZArchitecture.ZTextBox TextBoxCAC;
		private ZArchitecture.GUI.ZDateEdit ChiefCustomsActionDateFromFsnDateEdit;
		private ZArchitecture.ZTextBox ChiefCustomsActionTextFromFsnTextBox;
		private ZArchitecture.GUI.ZButton MucrCalcButton;
	}
}
