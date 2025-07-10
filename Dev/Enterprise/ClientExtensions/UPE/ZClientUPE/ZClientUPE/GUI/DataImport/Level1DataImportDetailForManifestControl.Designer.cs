namespace Enterprise.Client.UPE.GUI.DataImport
{
	partial class Level1DataImportDetailForManifestControl
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
			this.FlightScheduleGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DisableDecisionProviderCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.MasterBillTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CycleNumberDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CycleDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.DepartureDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.RoadTransportCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.FlightNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ArivalDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.PortOfDischargeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.PortOfLoadingCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ShippingAgentAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.FlightScheduleGroupBox.SuspendLayout();
			this.CycleNumberDropEdit.SuspendLayout();
			this.CycleDateDateEdit.SuspendLayout();
			this.DepartureDateDateEdit.SuspendLayout();
			this.ArivalDateDateEdit.SuspendLayout();
			this.PortOfDischargeCodeFindBox.SuspendLayout();
			this.PortOfLoadingCodeFindBox.SuspendLayout();
			this.ShippingAgentAddressControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.UPE.Business.DataImport.Level1DataImport);
			// 
			// FlightScheduleGroupBox
			// 
			this.FlightScheduleGroupBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("b6ff8d93-7a93-48b6-ac87-5b0c94fdfdea", "Import Details");
			this.FlightScheduleGroupBox.Controls.Add(this.DisableDecisionProviderCheckBox);
			this.FlightScheduleGroupBox.Controls.Add(this.MasterBillTextBox);
			this.FlightScheduleGroupBox.Controls.Add(this.CycleNumberDropEdit);
			this.FlightScheduleGroupBox.Controls.Add(this.CycleDateDateEdit);
			this.FlightScheduleGroupBox.Controls.Add(this.DepartureDateDateEdit);
			this.FlightScheduleGroupBox.Controls.Add(this.RoadTransportCheckBox);
			this.FlightScheduleGroupBox.Controls.Add(this.FlightNumberTextBox);
			this.FlightScheduleGroupBox.Controls.Add(this.ArivalDateDateEdit);
			this.FlightScheduleGroupBox.Controls.Add(this.PortOfDischargeCodeFindBox);
			this.FlightScheduleGroupBox.Controls.Add(this.PortOfLoadingCodeFindBox);
			this.FlightScheduleGroupBox.Controls.Add(this.ShippingAgentAddressControl);
			this.FlightScheduleGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FlightScheduleGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FlightScheduleGroupBox.Name = "FlightScheduleGroupBox";
			this.FlightScheduleGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(685, 141, true);
			this.FlightScheduleGroupBox.TabIndex = 1;
			this.FlightScheduleGroupBox.TabStop = false;
			// 
			// DisableDecisionProviderCheckBox
			// 
			this.BindingSource.SetBindingMember(this.DisableDecisionProviderCheckBox, "DisableDecisionProvider");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.UPE.Business.DataImport.Level1DataImport)(null)).DisableDecisionProvider)));
			this.DisableDecisionProviderCheckBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("78c5f60f-879a-4116-b734-de92ae3bc5e5", "Disable Decision Provider", "The Decision Provider determines if the shipment is to be imported and a TradeNet Declaration created");
			this.DisableDecisionProviderCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.DisableDecisionProviderCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.DisableDecisionProviderCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(505, 106, true);
			this.DisableDecisionProviderCheckBox.Name = "DisableDecisionProviderCheckBox";
			this.DisableDecisionProviderCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(162, 16, true);
			this.DisableDecisionProviderCheckBox.TabIndex = 10;
			this.DisableDecisionProviderCheckBox.Text = "Disable Decision Provider";
			this.DisableDecisionProviderCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// MasterBillTextBox
			// 
			this.BindingSource.SetBindingMember(this.MasterBillTextBox, "MasterBill");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.DataImport.Level1DataImport)(null)).MasterBill)));
			this.MasterBillTextBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("6f04a66c-2b73-491e-9676-8ec30da35bc6", "Master Bill");
			this.MasterBillTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 60, true);
			this.MasterBillTextBox.Name = "MasterBillTextBox";
			this.MasterBillTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.MasterBillTextBox.TabIndex = 2;
			// 
			// CycleNumberDropEdit
			// 
			this.CycleNumberDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CycleNumberDropEdit, "CycleNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.UPE.Business.DataImport.Level1DataImport)(null)).CycleNumber)));
			this.CycleNumberDropEdit.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("1aec89e7-f461-439a-b091-2d0c63b9f5d8", "Cycle Number");
			this.CycleNumberDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(374, 105, true);
			this.CycleNumberDropEdit.Name = "CycleNumberDropEdit";
			this.CycleNumberDropEdit.PreBoundMaxLength = 3;
			this.CycleNumberDropEdit.ShowDescriptionBox = false;
			this.CycleNumberDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.CycleNumberDropEdit.TabIndex = 9;
			// 
			// CycleDateDateEdit
			// 
			this.CycleDateDateEdit.AllowDrop = true;
			this.CycleDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.CycleDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.CycleDateDateEdit, "CycleDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.UPE.Business.DataImport.Level1DataImport)(null)).CycleDate)));
			this.CycleDateDateEdit.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("553795c1-5d50-43f8-a00a-509e3e038329", "Cycle Date");
			this.CycleDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(374, 82, true);
			this.CycleDateDateEdit.Name = "CycleDateDateEdit";
			this.CycleDateDateEdit.TabIndex = 8;
			// 
			// DepartureDateDateEdit
			// 
			this.DepartureDateDateEdit.AllowDrop = true;
			this.DepartureDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.DepartureDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DepartureDateDateEdit, "DepartureDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.UPE.Business.DataImport.Level1DataImport)(null)).DepartureDate)));
			this.DepartureDateDateEdit.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("0469f9e9-6010-4eae-ab3f-cf28af488cc9", "Departure Date");
			this.DepartureDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 105, true);
			this.DepartureDateDateEdit.Name = "DepartureDateDateEdit";
			this.DepartureDateDateEdit.TabIndex = 4;
			// 
			// RoadTransportCheckBox
			// 
			this.BindingSource.SetBindingMember(this.RoadTransportCheckBox, "IsRoad");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.UPE.Business.DataImport.Level1DataImport)(null)).IsRoad)));
			this.RoadTransportCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.RoadTransportCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RoadTransportCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 40, true);
			this.RoadTransportCheckBox.Name = "RoadTransportCheckBox";
			this.RoadTransportCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 16, true);
			this.RoadTransportCheckBox.TabIndex = 1;
			this.RoadTransportCheckBox.Text = "Road";
			this.RoadTransportCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// FlightNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.FlightNumberTextBox, "FlightNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.DataImport.Level1DataImport)(null)).FlightNumber)));
			this.FlightNumberTextBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("3068faf8-fe28-439f-b7b1-84028bc7fc06", "Flight Number");
			this.FlightNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 17, true);
			this.FlightNumberTextBox.Name = "FlightNumberTextBox";
			this.FlightNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 20, true);
			this.FlightNumberTextBox.TabIndex = 0;
			// 
			// ArivalDateDateEdit
			// 
			this.ArivalDateDateEdit.AllowDrop = true;
			this.ArivalDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.ArivalDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ArivalDateDateEdit, "ArrivalDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.UPE.Business.DataImport.Level1DataImport)(null)).ArrivalDate)));
			this.ArivalDateDateEdit.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("9dc5df21-e949-4eb1-9fcf-46729ea6d1da", "Arrival Date");
			this.ArivalDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 82, true);
			this.ArivalDateDateEdit.Name = "ArivalDateDateEdit";
			this.ArivalDateDateEdit.TabIndex = 3;
			// 
			// PortOfDischargeCodeFindBox
			// 
			this.PortOfDischargeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PortOfDischargeCodeFindBox, "PortOfDischarge");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.DataImport.Level1DataImport)(null)).PortOfDischarge)));
			this.PortOfDischargeCodeFindBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("d7fe92fc-3ed6-4c04-8977-d8f95ee0822d", "Discharge");
			this.PortOfDischargeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(374, 38, true);
			this.PortOfDischargeCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			this.PortOfDischargeCodeFindBox.Name = "PortOfDischargeCodeFindBox";
			this.PortOfDischargeCodeFindBox.ShouldResize = true;
			this.PortOfDischargeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.PortOfDischargeCodeFindBox.TabIndex = 6;
			// 
			// PortOfLoadingCodeFindBox
			// 
			this.PortOfLoadingCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PortOfLoadingCodeFindBox, "PortOfLoading");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.DataImport.Level1DataImport)(null)).PortOfLoading)));
			this.PortOfLoadingCodeFindBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("2b24c836-cbd8-4bdb-8e56-e0213ce5c76a", "Loading");
			this.PortOfLoadingCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(374, 17, true);
			this.PortOfLoadingCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			this.PortOfLoadingCodeFindBox.Name = "PortOfLoadingCodeFindBox";
			this.PortOfLoadingCodeFindBox.ShouldResize = true;
			this.PortOfLoadingCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.PortOfLoadingCodeFindBox.TabIndex = 5;
			// 
			// ShippingAgentAddressControl
			// 
			this.ShippingAgentAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShippingAgentAddressControl, "ShippingAgentAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.UPE.Business.DataImport.Level1DataImport)(null)).ShippingAgentAddress)));
			this.ShippingAgentAddressControl.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("dec77be3-d336-4536-9ce9-04159c5dce79", "Shipping Agent");
			this.ShippingAgentAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(374, 60, true);
			this.ShippingAgentAddressControl.Name = "ShippingAgentAddressControl";
			this.ShippingAgentAddressControl.PopupCaption = "";
			this.ShippingAgentAddressControl.ReadOnly = false;
			this.ShippingAgentAddressControl.ShowAddress = false;
			this.ShippingAgentAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.ShippingAgentAddressControl.TabIndex = 7;
			// 
			// Level1DataImportDetailForManifestControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.FlightScheduleGroupBox);
			this.Name = "Level1DataImportDetailForManifestControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(685, 141, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.FlightScheduleGroupBox.ResumeLayout(false);
			this.FlightScheduleGroupBox.PerformLayout();
			this.CycleNumberDropEdit.ResumeLayout(true);
			this.CycleNumberDropEdit.PerformLayout();
			this.CycleDateDateEdit.ResumeLayout(true);
			this.CycleDateDateEdit.PerformLayout();
			this.DepartureDateDateEdit.ResumeLayout(true);
			this.DepartureDateDateEdit.PerformLayout();
			this.ArivalDateDateEdit.ResumeLayout(true);
			this.ArivalDateDateEdit.PerformLayout();
			this.PortOfDischargeCodeFindBox.ResumeLayout(true);
			this.PortOfDischargeCodeFindBox.PerformLayout();
			this.PortOfLoadingCodeFindBox.ResumeLayout(true);
			this.PortOfLoadingCodeFindBox.PerformLayout();
			this.ShippingAgentAddressControl.ResumeLayout(true);
			this.ShippingAgentAddressControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox FlightScheduleGroupBox;
		private ZArchitecture.GUI.ZDropEdit CycleNumberDropEdit;
		private ZArchitecture.GUI.ZDateEdit CycleDateDateEdit;
		private ZArchitecture.GUI.ZDateEdit DepartureDateDateEdit;
		private ZArchitecture.GUI.ZCheckBox RoadTransportCheckBox;
		private ZArchitecture.ZTextBox FlightNumberTextBox;
		private ZArchitecture.GUI.ZDateEdit ArivalDateDateEdit;
		private ZArchitecture.GUI.ZCodeFindBox PortOfLoadingCodeFindBox;
		private ZArchitecture.ZTextBox MasterBillTextBox;
		private ZArchitecture.GUI.ZCheckBox DisableDecisionProviderCheckBox;
		private ZArchitecture.GUI.ZCodeFindBox PortOfDischargeCodeFindBox;
		private ZArchitecture.GUI.ZAddressControl ShippingAgentAddressControl;
	}
}
