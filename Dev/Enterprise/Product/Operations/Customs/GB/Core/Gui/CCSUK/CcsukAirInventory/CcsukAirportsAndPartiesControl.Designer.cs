using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	partial class CcsukAirportsAndPartiesControl
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
			this.PartiesAndCodesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AgentCodeFindBox = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ConsignmentType = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ProfileDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.SDC = new Enterprise.ZArchitecture.GUI.ZDropEdit(); 
			this.GuidFindBoxBranch = new Enterprise.ZArchitecture.GUI.ZGuidFindBox(); 
			this.AirportGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AirportOfOriginFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.AoDDropEdit2 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AoADropEdit1 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.Shed = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PartiesAndCodesGroupBox.SuspendLayout();
			this.AirportGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb);
			// 
			// PartiesAndCodesGroupBox
			// 
			this.PartiesAndCodesGroupBox.CaptionResourceString = Res.GetData("9dce8426-dccf-4fcf-951a-ba64618453f2", "Parties and codes");
			this.PartiesAndCodesGroupBox.Controls.Add(this.AgentCodeFindBox);
			this.PartiesAndCodesGroupBox.Controls.Add(this.ConsignmentType);
			this.PartiesAndCodesGroupBox.Controls.Add(this.ProfileDropEdit);
			this.PartiesAndCodesGroupBox.Controls.Add(this.SDC); 
			this.PartiesAndCodesGroupBox.Controls.Add(this.GuidFindBoxBranch);
			this.LabelCaptionRenderProvider.SetLabelTop(this.PartiesAndCodesGroupBox, 0);
			this.PartiesAndCodesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 4, true);
			this.PartiesAndCodesGroupBox.Name = "PartiesAndCodesGroupBox";
			this.PartiesAndCodesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 154, true);
			this.PartiesAndCodesGroupBox.TabIndex = 0;
			this.PartiesAndCodesGroupBox.TabStop = false;
			// 
			// AgentCodeFindBox
			// 
			this.AgentCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AgentCodeFindBox, "AgentBadge");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb)(null)).AgentBadge)));
			this.AgentCodeFindBox.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("CcsukAirportsAndPartiesControl|804741a2-5869-4b74-90f4-210dfcd79928", "Agt.", "Agent", "Nominated Agent", "Current Nominated Agent.");
			this.AgentCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 45, true);
			this.AgentCodeFindBox.Name = "AgentCodeFindBox";
			this.AgentCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(207, 20, true);
			this.AgentCodeFindBox.TabIndex = 1;
			// 
			// ConsignmentType
			// 
			this.ConsignmentType.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConsignmentType, "ConsignmentOrEntryType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb)(null)).ConsignmentOrEntryType)));
			this.ConsignmentType.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("CcsukAirportsAndPartiesControl|a2d8d0d7-3ab4-44a2-a80c-726580c0f08c", "Type", "Shipment Type", "Type of Shipment", "");
			this.ConsignmentType.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 97, true);
			this.ConsignmentType.Name = "ConsignmentType";
			this.ConsignmentType.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(206, 20, true);
			this.ConsignmentType.TabIndex = 3;
			// 
			// ProfileDropEdit
			// 
			this.ProfileDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ProfileDropEdit, "Profile");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb)(null)).Profile)));
			this.ProfileDropEdit.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("CcsukAirportsAndPartiesControl|e4fa96f5-efa5-4a3d-afe3-ffe8ca00cef9", "PIMA", "Profile", "Profile/PIMA", "PIMA or Profile code to be used for messaging.");
			this.ProfileDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 19, true);
			this.ProfileDropEdit.Name = "ProfileDropEdit";
			this.ProfileDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(207, 20, true);
			this.ProfileDropEdit.TabIndex = 0;
			// 
			// SDC
			// 
			this.SDC.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SDC, "ShipmentDescriptionCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb)(null)).ShipmentDescriptionCode)));
			this.SDC.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("CcsukAirportsAndPartiesControl|723e717b-e298-4cb5-9dc6-b1e740601343", "SDC", "Shipment Code", "Shipment Description Code", "");
			this.SDC.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 71, true);
			this.SDC.Name = "SDC";
			this.SDC.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(206, 20, true);
			this.SDC.TabIndex = 2;
			// 
			 
			// 
			this.GuidFindBoxBranch.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GuidFindBoxBranch, CusMAWB.Schema.CM_GB);  			
			this.GuidFindBoxBranch.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 123, true);
			this.GuidFindBoxBranch.Name = "GuidFindBoxBranch";
			this.GuidFindBoxBranch.Size = AgentCodeFindBox.Size;
			this.GuidFindBoxBranch.PreBoundMaxLength = 3;
			this.GuidFindBoxBranch.TabIndex = 4;


			// 
			// AirportGroupBox
			// 
			this.AirportGroupBox.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("721159d7-1925-4d4c-b487-cd2ce44fbdb3", "Airports");
			this.AirportGroupBox.Controls.Add(this.AirportOfOriginFindBox);
			this.AirportGroupBox.Controls.Add(this.AoDDropEdit2);
			this.AirportGroupBox.Controls.Add(this.AoADropEdit1);
			this.AirportGroupBox.Controls.Add(this.Shed);
			this.LabelCaptionRenderProvider.SetLabelTop(this.AirportGroupBox, 0);
			this.AirportGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(318, 4, true);
			this.AirportGroupBox.Name = "AirportGroupBox";
			this.AirportGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(292, 154, true);
			this.AirportGroupBox.TabIndex = 1;
			this.AirportGroupBox.TabStop = false;
			// 
			// AirportOfOriginFindBox
			// 
			this.AirportOfOriginFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AirportOfOriginFindBox, "AirportOfOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb)(null)).AirportOfOrigin)));
			this.AirportOfOriginFindBox.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("CcsukAirportsAndPartiesControl|9185b9fa-604b-4201-8d8f-8131752a2bf5", "Org.", "Origin", "Origin", "Airport of Origin.  Must be outside the UK.");
			this.AirportOfOriginFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 14, true);
			this.AirportOfOriginFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			this.AirportOfOriginFindBox.Name = "AirportOfOriginFindBox";
			this.AirportOfOriginFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 20, true);
			this.AirportOfOriginFindBox.TabIndex = 0;
			// 
			// AoDDropEdit2
			// 
			this.AoDDropEdit2.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AoDDropEdit2, "AirportOfDestination");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb)(null)).AirportOfDestination)));
			this.AoDDropEdit2.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("CcsukAirportsAndPartiesControl|71e48535-c3ca-4cef-8c32-0da6ae28e2bb", "Discharge");
			this.AoDDropEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 66, true);
			this.AoDDropEdit2.Name = "AoDDropEdit2";
			this.AoDDropEdit2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 20, true);
			this.AoDDropEdit2.TabIndex = 2;
			// 
			// AoADropEdit1
			// 
			this.AoADropEdit1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AoADropEdit1, "AirportOfArrival");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb)(null)).AirportOfArrival)));
			this.AoADropEdit1.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("CcsukAirportsAndPartiesControl|1678b23e-4141-4474-85f5-c6e376f63356", "Arrival");
			this.AoADropEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 40, true);
			this.AoADropEdit1.Name = "AoADropEdit1";
			this.AoADropEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 20, true);
			this.AoADropEdit1.TabIndex = 1;
			// 
			// Shed
			// 
			this.Shed.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.Shed, "CargoTerminalOperatorAirportAndShed");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb)(null)).CargoTerminalOperatorAirportAndShed)));
			this.Shed.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("CcsukAirportsAndPartiesControl|84f81f03-ebc8-4a52-ae9b-8496a4caa4fc", "Airport and Shed");
			this.Shed.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 92, true);
			this.Shed.Name = "Shed";
			this.Shed.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 20, true);
			this.Shed.TabIndex = 4;
			// 
			// CcsukAirportsAndPartiesControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PartiesAndCodesGroupBox);
			this.Controls.Add(this.AirportGroupBox);
			this.Name = "CcsukAirportsAndPartiesControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(612, 158, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PartiesAndCodesGroupBox.ResumeLayout(false);
			this.AirportGroupBox.ResumeLayout(false);
			this.AirportGroupBox.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox PartiesAndCodesGroupBox;
		private ZArchitecture.GUI.ZDropEdit SDC;
		private ZArchitecture.GUI.ZGroupBox AirportGroupBox;
		private ZArchitecture.GUI.ZDropEdit AoDDropEdit2;
		private ZArchitecture.GUI.ZDropEdit AoADropEdit1;
		private ZArchitecture.GUI.ZDropEdit Shed;
		private ZArchitecture.GUI.ZDropEdit ProfileDropEdit;
		private ZArchitecture.GUI.ZDropEdit ConsignmentType;
		private ZArchitecture.GUI.ZDropEdit AgentCodeFindBox;
		private ZArchitecture.GUI.ZCodeFindBox AirportOfOriginFindBox;
		Enterprise.ZArchitecture.GUI.ZGuidFindBox GuidFindBoxBranch;
	}
}
