namespace Enterprise.Customs.CA.GUI
{
	partial class BillDetailsPlugInUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.splitter1 = new CargoWise.Windows.UI.KSplitter();
			this.ConsigneeTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.ConsigneeTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ConsigneeStateDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ConsigneeCountryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CA_OH_ConsigneeBoundGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.CA_ConsigneeContactNameBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CA_ConsigneePhoneBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CA_ConsigneePostcodeBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CA_ConsigneeAddress3BoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CA_ConsigneeAddress2BoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CA_ConsigneeAddress1BoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CA_ConsigneeNameBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ConsignorTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ConsignorStateDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CA_ConsignorContactNameBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CA_ConsignorPhoneTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ConsignorCountryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CA_OH_ConsignorBoundGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.CA_ConsignorPostcodeBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CA_ConsignorAddress3BoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CA_ConsignorAddress2BoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CA_ConsignorAddress1BoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CA_ConsignorNameBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NotifyPartyTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.NotifyStateDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.NotifyCountryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CA_OH_NotifyBoundGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.CA_NotifyContactNameBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CA_NotifyPhoneBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CA_NotifyPostcodeBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CA_NotifyAddress3BoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CA_NotifyAddress2BoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CA_NotifyAddress1BoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CA_NotifyNameBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DeliveryTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.DeliveryDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CA_DeliveryCountryTextBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CA_OA_DeliveryAddressGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.CA_DeliveryContactNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CA_DeliveryPhoneTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CA_DeliveryPostodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CA_DeliverySuburbTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CA_DeliveryAddress2TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CA_DeliveryAddress1TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CA_DeliveryNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.GrouBoxHouseBill = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CA_AuthenticationCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CA_SpecialInstructionsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CA_FROBTransitImportCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CA_CargoFacilityLocationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CA_RL_NK_PortOfDestinationBoundCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.OverrideDefaultShipmentValuesCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SuppRefNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CarrierCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ShipmentStatusZTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MessageStatusZTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MessageStatusDescriptionZTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ShipmentStatusDescriptionZTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ConsigneeTabControl.SuspendLayout();
			this.ConsigneeTabPage.SuspendLayout();
			this.ConsignorTabPage.SuspendLayout();
			this.NotifyPartyTabPage.SuspendLayout();
			this.DeliveryTabPage.SuspendLayout();
			this.GrouBoxHouseBill.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.CusSCAHouse);
			// 
			// splitter1
			// 
			this.splitter1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.splitter1.Dock = System.Windows.Forms.DockStyle.Right;
			this.splitter1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(690, 16, true);
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(5, 556, true);
			this.splitter1.TabIndex = 3;
			this.splitter1.TabStop = false;
			// 
			// ConsigneeTabControl
			// 
			this.ConsigneeTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.ConsigneeTabControl.Controls.Add(this.ConsigneeTabPage);
			this.ConsigneeTabControl.Controls.Add(this.ConsignorTabPage);
			this.ConsigneeTabControl.Controls.Add(this.NotifyPartyTabPage);
			this.ConsigneeTabControl.Controls.Add(this.DeliveryTabPage);
			this.ConsigneeTabControl.Dock = System.Windows.Forms.DockStyle.Right;
			this.ConsigneeTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(706, 0, true);
			this.ConsigneeTabControl.Multiline = true;
			this.ConsigneeTabControl.Name = "ConsigneeTabControl";
			this.ConsigneeTabControl.SelectedIndex = 0;
			this.ConsigneeTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(376, 253, true);
			this.ConsigneeTabControl.TabIndex = 2;
			// 
			// ConsigneeTabPage
			// 
			this.ConsigneeTabPage.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("BillDetailsPlugInUserControl|31cbb31e-4017-4987-a199-586814e22930", "Consignee");
			this.ConsigneeTabPage.Controls.Add(this.ConsigneeStateDropEdit);
			this.ConsigneeTabPage.Controls.Add(this.ConsigneeCountryCodeFindBox);
			this.ConsigneeTabPage.Controls.Add(this.CA_OH_ConsigneeBoundGuidFindBox);
			this.ConsigneeTabPage.Controls.Add(this.CA_ConsigneeContactNameBoundTextBox);
			this.ConsigneeTabPage.Controls.Add(this.CA_ConsigneePhoneBoundTextBox);
			this.ConsigneeTabPage.Controls.Add(this.CA_ConsigneePostcodeBoundTextBox);
			this.ConsigneeTabPage.Controls.Add(this.CA_ConsigneeAddress3BoundTextBox);
			this.ConsigneeTabPage.Controls.Add(this.CA_ConsigneeAddress2BoundTextBox);
			this.ConsigneeTabPage.Controls.Add(this.CA_ConsigneeAddress1BoundTextBox);
			this.ConsigneeTabPage.Controls.Add(this.CA_ConsigneeNameBoundTextBox);
			this.ConsigneeTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ConsigneeTabPage.Name = "ConsigneeTabPage";
			this.ConsigneeTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(368, 226, true);
			this.ConsigneeTabPage.TabIndex = 0;
			// 
			// ConsigneeStateDropEdit
			// 
			this.ConsigneeStateDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConsigneeStateDropEdit, "CA_ConsigneeState");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).CA_ConsigneeState)));
			this.ConsigneeStateDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("BillDetailsPlugInUserControl|822ba286-b90c-420d-b87d-034270e3749a", "State/Province");
			this.ConsigneeStateDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 127, true);
			this.ConsigneeStateDropEdit.Name = "ConsigneeStateDropEdit";
			this.ConsigneeStateDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.ConsigneeStateDropEdit.TabIndex = 5;
			// 
			// ConsigneeCountryCodeFindBox
			// 
			this.ConsigneeCountryCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConsigneeCountryCodeFindBox, "CA_RN_NKConsigneeCountryCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).CA_RN_NKConsigneeCountryCode)));
			this.ConsigneeCountryCodeFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("BillDetailsPlugInUserControl|32a3b597-fc70-4586-8bdd-517eda6ea81c", "Country/Region", "Consignee Country/Region override.");
			this.ConsigneeCountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(281, 151, true);
			this.ConsigneeCountryCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
			this.ConsigneeCountryCodeFindBox.Name = "ConsigneeCountryCodeFindBox";
			this.ConsigneeCountryCodeFindBox.PreBoundMaxLength = 2;
			this.ConsigneeCountryCodeFindBox.ShowDescriptionBox = false;
			this.ConsigneeCountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 20, true);
			this.ConsigneeCountryCodeFindBox.TabIndex = 7;
			// 
			// CA_OH_ConsigneeBoundGuidFindBox
			// 
			this.CA_OH_ConsigneeBoundGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CA_OH_ConsigneeBoundGuidFindBox, "CA_OH_Consignee");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).CA_OH_Consignee)));
			this.CA_OH_ConsigneeBoundGuidFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("BillDetailsPlugInUserControl|972c48bd-d1d9-472f-b6c2-238b6a31a9ae", "Consignee", "Consignee Code", "The Consignee\'s Code. Leave blank to override.");
			this.CA_OH_ConsigneeBoundGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 5, true);
			this.CA_OH_ConsigneeBoundGuidFindBox.Name = "CA_OH_ConsigneeBoundGuidFindBox";
			this.CA_OH_ConsigneeBoundGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.CA_OH_ConsigneeBoundGuidFindBox.TabIndex = 0;
			// 
			// CA_ConsigneeContactNameBoundTextBox
			// 
			this.CA_ConsigneeContactNameBoundTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CA_ConsigneeContactNameBoundTextBox, "CA_ConsigneeContactName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).CA_ConsigneeContactName)));
			this.CA_ConsigneeContactNameBoundTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("BillDetailsPlugInUserControl|d8477d40-8812-4b67-a703-825c333ba1e4", "Contact");
			this.CA_ConsigneeContactNameBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 175, true);
			this.CA_ConsigneeContactNameBoundTextBox.Name = "CA_ConsigneeContactNameBoundTextBox";
			this.CA_ConsigneeContactNameBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.CA_ConsigneeContactNameBoundTextBox.TabIndex = 8;
			// 
			// CA_ConsigneePhoneBoundTextBox
			// 
			this.CA_ConsigneePhoneBoundTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CA_ConsigneePhoneBoundTextBox, "CA_ConsigneePhone");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).CA_ConsigneePhone)));
			this.CA_ConsigneePhoneBoundTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("BillDetailsPlugInUserControl|b7fc41f9-6262-4759-a54f-d50a1a9eef79", "Phone");
			this.CA_ConsigneePhoneBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 199, true);
			this.CA_ConsigneePhoneBoundTextBox.Name = "CA_ConsigneePhoneBoundTextBox";
			this.CA_ConsigneePhoneBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.CA_ConsigneePhoneBoundTextBox.TabIndex = 9;
			// 
			// CA_ConsigneePostcodeBoundTextBox
			// 
			this.CA_ConsigneePostcodeBoundTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CA_ConsigneePostcodeBoundTextBox, "CA_ConsigneePostcode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).CA_ConsigneePostcode)));
			this.CA_ConsigneePostcodeBoundTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("BillDetailsPlugInUserControl|c16524e0-5204-4820-a24b-bc79cf7b0eac", "Post Code", "Consignee Postcode override.");
			this.CA_ConsigneePostcodeBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 151, true);
			this.CA_ConsigneePostcodeBoundTextBox.Name = "CA_ConsigneePostcodeBoundTextBox";
			this.CA_ConsigneePostcodeBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 20, true);
			this.CA_ConsigneePostcodeBoundTextBox.TabIndex = 6;
			// 
			// CA_ConsigneeAddress3BoundTextBox
			// 
			this.CA_ConsigneeAddress3BoundTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CA_ConsigneeAddress3BoundTextBox, "CA_ConsigneeSuburb");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).CA_ConsigneeSuburb)));
			this.CA_ConsigneeAddress3BoundTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("BillDetailsPlugInUserControl|ad3ae4e8-9581-4744-a73f-3e1a65eb86a2", "City");
			this.CA_ConsigneeAddress3BoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 103, true);
			this.CA_ConsigneeAddress3BoundTextBox.Name = "CA_ConsigneeAddress3BoundTextBox";
			this.CA_ConsigneeAddress3BoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.CA_ConsigneeAddress3BoundTextBox.TabIndex = 4;
			// 
			// CA_ConsigneeAddress2BoundTextBox
			// 
			this.CA_ConsigneeAddress2BoundTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CA_ConsigneeAddress2BoundTextBox, "CA_ConsigneeAddress2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).CA_ConsigneeAddress2)));
			this.CA_ConsigneeAddress2BoundTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("BillDetailsPlugInUserControl|bf4d894e-6c42-407f-b547-863f977cf19e", "Address 2", "Consignee Address 2 override.");
			this.CA_ConsigneeAddress2BoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 79, true);
			this.CA_ConsigneeAddress2BoundTextBox.Name = "CA_ConsigneeAddress2BoundTextBox";
			this.CA_ConsigneeAddress2BoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.CA_ConsigneeAddress2BoundTextBox.TabIndex = 3;
			// 
			// CA_ConsigneeAddress1BoundTextBox
			// 
			this.CA_ConsigneeAddress1BoundTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CA_ConsigneeAddress1BoundTextBox, "CA_ConsigneeAddress1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).CA_ConsigneeAddress1)));
			this.CA_ConsigneeAddress1BoundTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("BillDetailsPlugInUserControl|135cf9af-f807-4608-8e7b-fda342bdee30", "Address 1", "Consignee Address 1 override.");
			this.CA_ConsigneeAddress1BoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 55, true);
			this.CA_ConsigneeAddress1BoundTextBox.Name = "CA_ConsigneeAddress1BoundTextBox";
			this.CA_ConsigneeAddress1BoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.CA_ConsigneeAddress1BoundTextBox.TabIndex = 2;
			// 
			// CA_ConsigneeNameBoundTextBox
			// 
			this.CA_ConsigneeNameBoundTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CA_ConsigneeNameBoundTextBox, "CA_ConsigneeName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).CA_ConsigneeName)));
			this.CA_ConsigneeNameBoundTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("BillDetailsPlugInUserControl|564e7646-40bc-4468-9a00-22447fa2d37d", "Name", "Consignee Name override.");
			this.CA_ConsigneeNameBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 31, true);
			this.CA_ConsigneeNameBoundTextBox.Name = "CA_ConsigneeNameBoundTextBox";
			this.CA_ConsigneeNameBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.CA_ConsigneeNameBoundTextBox.TabIndex = 1;
			// 
			// ConsignorTabPage
			// 
			this.ConsignorTabPage.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("BillDetailsPlugInUserControl|3d08acca-1c05-4552-8ded-e4dfd73e978a", "Shipper");
			this.ConsignorTabPage.Controls.Add(this.ConsignorStateDropEdit);
			this.ConsignorTabPage.Controls.Add(this.CA_ConsignorContactNameBoundTextBox);
			this.ConsignorTabPage.Controls.Add(this.CA_ConsignorPhoneTextBox);
			this.ConsignorTabPage.Controls.Add(this.ConsignorCountryCodeFindBox);
			this.ConsignorTabPage.Controls.Add(this.CA_OH_ConsignorBoundGuidFindBox);
			this.ConsignorTabPage.Controls.Add(this.CA_ConsignorPostcodeBoundTextBox);
			this.ConsignorTabPage.Controls.Add(this.CA_ConsignorAddress3BoundTextBox);
			this.ConsignorTabPage.Controls.Add(this.CA_ConsignorAddress2BoundTextBox);
			this.ConsignorTabPage.Controls.Add(this.CA_ConsignorAddress1BoundTextBox);
			this.ConsignorTabPage.Controls.Add(this.CA_ConsignorNameBoundTextBox);
			this.ConsignorTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ConsignorTabPage.Name = "ConsignorTabPage";
			this.ConsignorTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(368, 226, true);
			this.ConsignorTabPage.TabIndex = 2;
			// 
			// ConsignorStateDropEdit
			// 
			this.ConsignorStateDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConsignorStateDropEdit, "CA_ConsignorState");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).CA_ConsignorState)));
			this.ConsignorStateDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("BillDetailsPlugInUserControl|7deb2d7b-a515-4366-b10a-afa491ad1432", "State/Province", "Consignor State/Province override.");
			this.ConsignorStateDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 127, true);
			this.ConsignorStateDropEdit.Name = "ConsignorStateDropEdit";
			this.ConsignorStateDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.ConsignorStateDropEdit.TabIndex = 5;
			// 
			// CA_ConsignorContactNameBoundTextBox
			// 
			this.CA_ConsignorContactNameBoundTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CA_ConsignorContactNameBoundTextBox, "CA_ConsignorContactName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).CA_ConsignorContactName)));
			this.CA_ConsignorContactNameBoundTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("BillDetailsPlugInUserControl|c2688a7c-c943-42f3-b741-8945c7ee3593", "Contact");
			this.CA_ConsignorContactNameBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 175, true);
			this.CA_ConsignorContactNameBoundTextBox.Name = "CA_ConsignorContactNameBoundTextBox";
			this.CA_ConsignorContactNameBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.CA_ConsignorContactNameBoundTextBox.TabIndex = 8;
			// 
			// CA_ConsignorPhoneTextBox
			// 
			this.CA_ConsignorPhoneTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CA_ConsignorPhoneTextBox, "CA_ConsignorPhone");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).CA_ConsignorPhone)));
			this.CA_ConsignorPhoneTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("BillDetailsPlugInUserControl|55f6b64b-6b62-4ca9-bab4-8416485fc793", "Phone");
			this.CA_ConsignorPhoneTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 199, true);
			this.CA_ConsignorPhoneTextBox.Name = "CA_ConsignorPhoneTextBox";
			this.CA_ConsignorPhoneTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.CA_ConsignorPhoneTextBox.TabIndex = 9;
			// 
			// ConsignorCountryCodeFindBox
			// 
			this.ConsignorCountryCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConsignorCountryCodeFindBox, "CA_RN_NKConsignorCountryCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).CA_RN_NKConsignorCountryCode)));
			this.ConsignorCountryCodeFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("BillDetailsPlugInUserControl|8d1b92c4-c3d0-4b2e-876a-8854f417df9d", "Country/Region");
			this.ConsignorCountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(281, 151, true);
			this.ConsignorCountryCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
			this.ConsignorCountryCodeFindBox.Name = "ConsignorCountryCodeFindBox";
			this.ConsignorCountryCodeFindBox.PreBoundMaxLength = 2;
			this.ConsignorCountryCodeFindBox.ShowDescriptionBox = false;
			this.ConsignorCountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 20, true);
			this.ConsignorCountryCodeFindBox.TabIndex = 7;
			// 
			// CA_OH_ConsignorBoundGuidFindBox
			// 
			this.CA_OH_ConsignorBoundGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CA_OH_ConsignorBoundGuidFindBox, "CA_OH_Consignor");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).CA_OH_Consignor)));
			this.CA_OH_ConsignorBoundGuidFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("BillDetailsPlugInUserControl|56839208-068b-4ff8-8669-3ca4135c7b62", "Shipper", "The Consignor\'s Code. Leave blank to override.");
			this.CA_OH_ConsignorBoundGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 5, true);
			this.CA_OH_ConsignorBoundGuidFindBox.Name = "CA_OH_ConsignorBoundGuidFindBox";
			this.CA_OH_ConsignorBoundGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.CA_OH_ConsignorBoundGuidFindBox.TabIndex = 0;
			// 
			// CA_ConsignorPostcodeBoundTextBox
			// 
			this.CA_ConsignorPostcodeBoundTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CA_ConsignorPostcodeBoundTextBox, "CA_ConsignorPostcode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).CA_ConsignorPostcode)));
			this.CA_ConsignorPostcodeBoundTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("BillDetailsPlugInUserControl|85868e05-f755-4729-8ba8-a74d2a730141", "Post Code", "Consignor Postcode override.");
			this.CA_ConsignorPostcodeBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 151, true);
			this.CA_ConsignorPostcodeBoundTextBox.Name = "CA_ConsignorPostcodeBoundTextBox";
			this.CA_ConsignorPostcodeBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 20, true);
			this.CA_ConsignorPostcodeBoundTextBox.TabIndex = 6;
			// 
			// CA_ConsignorAddress3BoundTextBox
			// 
			this.CA_ConsignorAddress3BoundTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CA_ConsignorAddress3BoundTextBox, "CA_ConsignorSuburb");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).CA_ConsignorSuburb)));
			this.CA_ConsignorAddress3BoundTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("BillDetailsPlugInUserControl|b3d313cc-fcca-444e-9805-fe55c481c464", "City");
			this.CA_ConsignorAddress3BoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 103, true);
			this.CA_ConsignorAddress3BoundTextBox.Name = "CA_ConsignorAddress3BoundTextBox";
			this.CA_ConsignorAddress3BoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.CA_ConsignorAddress3BoundTextBox.TabIndex = 4;
			// 
			// CA_ConsignorAddress2BoundTextBox
			// 
			this.CA_ConsignorAddress2BoundTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CA_ConsignorAddress2BoundTextBox, "CA_ConsignorAddress2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).CA_ConsignorAddress2)));
			this.CA_ConsignorAddress2BoundTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("BillDetailsPlugInUserControl|8086cb2a-86f0-45db-9d9f-49df109dde9b", "Address 2", "Consignor Address 2 override.");
			this.CA_ConsignorAddress2BoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 79, true);
			this.CA_ConsignorAddress2BoundTextBox.Name = "CA_ConsignorAddress2BoundTextBox";
			this.CA_ConsignorAddress2BoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.CA_ConsignorAddress2BoundTextBox.TabIndex = 3;
			// 
			// CA_ConsignorAddress1BoundTextBox
			// 
			this.CA_ConsignorAddress1BoundTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CA_ConsignorAddress1BoundTextBox, "CA_ConsignorAddress1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).CA_ConsignorAddress1)));
			this.CA_ConsignorAddress1BoundTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("BillDetailsPlugInUserControl|f6d40adf-e3b2-489e-9c7c-fc661abe940c", "Address 1", "Consignor Address 1 override.");
			this.CA_ConsignorAddress1BoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 55, true);
			this.CA_ConsignorAddress1BoundTextBox.Name = "CA_ConsignorAddress1BoundTextBox";
			this.CA_ConsignorAddress1BoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.CA_ConsignorAddress1BoundTextBox.TabIndex = 2;
			// 
			// CA_ConsignorNameBoundTextBox
			// 
			this.CA_ConsignorNameBoundTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CA_ConsignorNameBoundTextBox, "CA_ConsignorName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).CA_ConsignorName)));
			this.CA_ConsignorNameBoundTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("BillDetailsPlugInUserControl|3c362563-6436-4d44-b6bb-747bdf692d91", "Name");
			this.CA_ConsignorNameBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 31, true);
			this.CA_ConsignorNameBoundTextBox.Name = "CA_ConsignorNameBoundTextBox";
			this.CA_ConsignorNameBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.CA_ConsignorNameBoundTextBox.TabIndex = 1;
			// 
			// NotifyPartyTabPage
			// 
			this.NotifyPartyTabPage.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("BillDetailsPlugInUserControl|92fca5f5-6e43-4248-8e19-ce6d20645535", "Notify Party");
			this.NotifyPartyTabPage.Controls.Add(this.NotifyStateDropEdit);
			this.NotifyPartyTabPage.Controls.Add(this.NotifyCountryCodeFindBox);
			this.NotifyPartyTabPage.Controls.Add(this.CA_OH_NotifyBoundGuidFindBox);
			this.NotifyPartyTabPage.Controls.Add(this.CA_NotifyContactNameBoundTextBox);
			this.NotifyPartyTabPage.Controls.Add(this.CA_NotifyPhoneBoundTextBox);
			this.NotifyPartyTabPage.Controls.Add(this.CA_NotifyPostcodeBoundTextBox);
			this.NotifyPartyTabPage.Controls.Add(this.CA_NotifyAddress3BoundTextBox);
			this.NotifyPartyTabPage.Controls.Add(this.CA_NotifyAddress2BoundTextBox);
			this.NotifyPartyTabPage.Controls.Add(this.CA_NotifyAddress1BoundTextBox);
			this.NotifyPartyTabPage.Controls.Add(this.CA_NotifyNameBoundTextBox);
			this.NotifyPartyTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.NotifyPartyTabPage.Name = "NotifyPartyTabPage";
			this.NotifyPartyTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(368, 226, true);
			this.NotifyPartyTabPage.TabIndex = 1;
			// 
			// NotifyStateDropEdit
			// 
			this.NotifyStateDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NotifyStateDropEdit, "CA_NotifyState");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).CA_NotifyState)));
			this.NotifyStateDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("BillDetailsPlugInUserControl|669b3364-fb0f-4b87-8ed7-7e4519d0505c", "State/Province");
			this.NotifyStateDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 127, true);
			this.NotifyStateDropEdit.Name = "NotifyStateDropEdit";
			this.NotifyStateDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.NotifyStateDropEdit.TabIndex = 5;
			// 
			// NotifyCountryCodeFindBox
			// 
			this.NotifyCountryCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NotifyCountryCodeFindBox, "CA_RN_NKNotifyCountryCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).CA_RN_NKNotifyCountryCode)));
			this.NotifyCountryCodeFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("BillDetailsPlugInUserControl|0a2e0a19-4274-4f01-9318-b489b9256861", "Country/Region", "Notify Party Country/Region override.");
			this.NotifyCountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(281, 151, true);
			this.NotifyCountryCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
			this.NotifyCountryCodeFindBox.Name = "NotifyCountryCodeFindBox";
			this.NotifyCountryCodeFindBox.PreBoundMaxLength = 2;
			this.NotifyCountryCodeFindBox.ShowDescriptionBox = false;
			this.NotifyCountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 20, true);
			this.NotifyCountryCodeFindBox.TabIndex = 7;
			// 
			// CA_OH_NotifyBoundGuidFindBox
			// 
			this.CA_OH_NotifyBoundGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CA_OH_NotifyBoundGuidFindBox, "CA_OH_Notify");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).CA_OH_Notify)));
			this.CA_OH_NotifyBoundGuidFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("BillDetailsPlugInUserControl|633b6a15-6824-4378-bc96-c92b8060fc0a", "Notify Party", "The Notify Party\'s Code. Leave blank to override.");
			this.CA_OH_NotifyBoundGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 5, true);
			this.CA_OH_NotifyBoundGuidFindBox.Name = "CA_OH_NotifyBoundGuidFindBox";
			this.CA_OH_NotifyBoundGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.CA_OH_NotifyBoundGuidFindBox.TabIndex = 0;
			// 
			// CA_NotifyContactNameBoundTextBox
			// 
			this.CA_NotifyContactNameBoundTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CA_NotifyContactNameBoundTextBox, "CA_NotifyContactName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).CA_NotifyContactName)));
			this.CA_NotifyContactNameBoundTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("BillDetailsPlugInUserControl|f4d2d1d8-b3b3-4605-8752-0285246b3a8d", "Contact");
			this.CA_NotifyContactNameBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 175, true);
			this.CA_NotifyContactNameBoundTextBox.Name = "CA_NotifyContactNameBoundTextBox";
			this.CA_NotifyContactNameBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.CA_NotifyContactNameBoundTextBox.TabIndex = 8;
			// 
			// CA_NotifyPhoneBoundTextBox
			// 
			this.CA_NotifyPhoneBoundTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CA_NotifyPhoneBoundTextBox, "CA_NotifyPhone");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).CA_NotifyPhone)));
			this.CA_NotifyPhoneBoundTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("BillDetailsPlugInUserControl|074b2be3-b8c2-4d3d-a09f-270674855dbf", "Phone");
			this.CA_NotifyPhoneBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 199, true);
			this.CA_NotifyPhoneBoundTextBox.Name = "CA_NotifyPhoneBoundTextBox";
			this.CA_NotifyPhoneBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.CA_NotifyPhoneBoundTextBox.TabIndex = 9;
			// 
			// CA_NotifyPostcodeBoundTextBox
			// 
			this.CA_NotifyPostcodeBoundTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CA_NotifyPostcodeBoundTextBox, "CA_NotifyPostcode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).CA_NotifyPostcode)));
			this.CA_NotifyPostcodeBoundTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("BillDetailsPlugInUserControl|ae4490d1-4e5d-4b59-8c24-fa6415b2c816", "Post Code", "Notify Party Postcode override.");
			this.CA_NotifyPostcodeBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 151, true);
			this.CA_NotifyPostcodeBoundTextBox.Name = "CA_NotifyPostcodeBoundTextBox";
			this.CA_NotifyPostcodeBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 20, true);
			this.CA_NotifyPostcodeBoundTextBox.TabIndex = 6;
			// 
			// CA_NotifyAddress3BoundTextBox
			// 
			this.CA_NotifyAddress3BoundTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CA_NotifyAddress3BoundTextBox, "CA_NotifySuburb");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).CA_NotifySuburb)));
			this.CA_NotifyAddress3BoundTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("BillDetailsPlugInUserControl|b1ba02f1-369b-436b-8254-1e8997531cd2", "City");
			this.CA_NotifyAddress3BoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 103, true);
			this.CA_NotifyAddress3BoundTextBox.Name = "CA_NotifyAddress3BoundTextBox";
			this.CA_NotifyAddress3BoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.CA_NotifyAddress3BoundTextBox.TabIndex = 4;
			// 
			// CA_NotifyAddress2BoundTextBox
			// 
			this.CA_NotifyAddress2BoundTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CA_NotifyAddress2BoundTextBox, "CA_NotifyAddress2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).CA_NotifyAddress2)));
			this.CA_NotifyAddress2BoundTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("BillDetailsPlugInUserControl|809b6016-4a99-41a9-bffd-2d07d3d5bdcb", "Address 2", "Notify Party Address 2 override.");
			this.CA_NotifyAddress2BoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 79, true);
			this.CA_NotifyAddress2BoundTextBox.Name = "CA_NotifyAddress2BoundTextBox";
			this.CA_NotifyAddress2BoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.CA_NotifyAddress2BoundTextBox.TabIndex = 3;
			// 
			// CA_NotifyAddress1BoundTextBox
			// 
			this.CA_NotifyAddress1BoundTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CA_NotifyAddress1BoundTextBox, "CA_NotifyAddress1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).CA_NotifyAddress1)));
			this.CA_NotifyAddress1BoundTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("BillDetailsPlugInUserControl|61a5c86e-5433-4a78-aba7-bd813baff1be", "Address 1", "Notify Party Address 1 override.");
			this.CA_NotifyAddress1BoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 55, true);
			this.CA_NotifyAddress1BoundTextBox.Name = "CA_NotifyAddress1BoundTextBox";
			this.CA_NotifyAddress1BoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.CA_NotifyAddress1BoundTextBox.TabIndex = 2;
			// 
			// CA_NotifyNameBoundTextBox
			// 
			this.CA_NotifyNameBoundTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CA_NotifyNameBoundTextBox, "CA_NotifyName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).CA_NotifyName)));
			this.CA_NotifyNameBoundTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("BillDetailsPlugInUserControl|0f3ba993-dd12-4b14-9e29-11adc1b847f5", "Name", "Notify Party Name override.");
			this.CA_NotifyNameBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 31, true);
			this.CA_NotifyNameBoundTextBox.Name = "CA_NotifyNameBoundTextBox";
			this.CA_NotifyNameBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.CA_NotifyNameBoundTextBox.TabIndex = 1;
			// 
			// DeliveryTabPage
			// 
			this.DeliveryTabPage.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("BillDetailsPlugInUserControl|4f0f8bae-be60-4691-9202-fab3d299c81c", "Delivery");
			this.DeliveryTabPage.Controls.Add(this.DeliveryDropEdit);
			this.DeliveryTabPage.Controls.Add(this.CA_DeliveryCountryTextBox);
			this.DeliveryTabPage.Controls.Add(this.CA_OA_DeliveryAddressGuidFindBox);
			this.DeliveryTabPage.Controls.Add(this.CA_DeliveryContactNameTextBox);
			this.DeliveryTabPage.Controls.Add(this.CA_DeliveryPhoneTextBox);
			this.DeliveryTabPage.Controls.Add(this.CA_DeliveryPostodeTextBox);
			this.DeliveryTabPage.Controls.Add(this.CA_DeliverySuburbTextBox);
			this.DeliveryTabPage.Controls.Add(this.CA_DeliveryAddress2TextBox);
			this.DeliveryTabPage.Controls.Add(this.CA_DeliveryAddress1TextBox);
			this.DeliveryTabPage.Controls.Add(this.CA_DeliveryNameTextBox);
			this.DeliveryTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DeliveryTabPage.Name = "DeliveryTabPage";
			this.DeliveryTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(368, 226, true);
			this.DeliveryTabPage.TabIndex = 3;
			// 
			// DeliveryDropEdit
			// 
			this.DeliveryDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DeliveryDropEdit, "CA_DeliveryState");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).CA_DeliveryState)));
			this.DeliveryDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("BillDetailsPlugInUserControl|85f494d7-aa1b-4bf9-bf1d-1803cdae4f98", "State/Province", "Delivery State/Province override.");
			this.DeliveryDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 127, true);
			this.DeliveryDropEdit.Name = "DeliveryDropEdit";
			this.DeliveryDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.DeliveryDropEdit.TabIndex = 5;
			// 
			// CA_DeliveryCountryTextBox
			// 
			this.CA_DeliveryCountryTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CA_DeliveryCountryTextBox, "CA_RN_NKDeliveryCountryCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).CA_RN_NKDeliveryCountryCode)));
			this.CA_DeliveryCountryTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("BillDetailsPlugInUserControl|22bb2fc5-154e-4abb-9b22-aaff1adaf3e9", "Country/Region");
			this.CA_DeliveryCountryTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(281, 151, true);
			this.CA_DeliveryCountryTextBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
			this.CA_DeliveryCountryTextBox.Name = "CA_DeliveryCountryTextBox";
			this.CA_DeliveryCountryTextBox.PreBoundMaxLength = 2;
			this.CA_DeliveryCountryTextBox.ShowDescriptionBox = false;
			this.CA_DeliveryCountryTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 20, true);
			this.CA_DeliveryCountryTextBox.TabIndex = 7;
			// 
			// CA_OA_DeliveryAddressGuidFindBox
			// 
			this.CA_OA_DeliveryAddressGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CA_OA_DeliveryAddressGuidFindBox, "CA_OA_DeliveryAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).CA_OA_DeliveryAddress)));
			this.CA_OA_DeliveryAddressGuidFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("BillDetailsPlugInUserControl|ed089580-0c06-4e03-97d7-47765adac0a9", "Delivery", "The delivery address, if different to the consignee address.");
			this.CA_OA_DeliveryAddressGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 5, true);
			this.CA_OA_DeliveryAddressGuidFindBox.Name = "CA_OA_DeliveryAddressGuidFindBox";
			this.CA_OA_DeliveryAddressGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.CA_OA_DeliveryAddressGuidFindBox.TabIndex = 0;
			// 
			// CA_DeliveryContactNameTextBox
			// 
			this.CA_DeliveryContactNameTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CA_DeliveryContactNameTextBox, "CA_DeliveryContactName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).CA_DeliveryContactName)));
			this.CA_DeliveryContactNameTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("BillDetailsPlugInUserControl|d831597c-257a-4819-9f3f-4173d1a5e93c", "Contact");
			this.CA_DeliveryContactNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 175, true);
			this.CA_DeliveryContactNameTextBox.Name = "CA_DeliveryContactNameTextBox";
			this.CA_DeliveryContactNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.CA_DeliveryContactNameTextBox.TabIndex = 8;
			// 
			// CA_DeliveryPhoneTextBox
			// 
			this.CA_DeliveryPhoneTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CA_DeliveryPhoneTextBox, "CA_DeliveryPhone");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).CA_DeliveryPhone)));
			this.CA_DeliveryPhoneTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("BillDetailsPlugInUserControl|7be72e51-4866-4e0b-8016-9824994b3b45", "Phone", "Delivery Phone override.");
			this.CA_DeliveryPhoneTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 199, true);
			this.CA_DeliveryPhoneTextBox.Name = "CA_DeliveryPhoneTextBox";
			this.CA_DeliveryPhoneTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.CA_DeliveryPhoneTextBox.TabIndex = 9;
			// 
			// CA_DeliveryPostodeTextBox
			// 
			this.CA_DeliveryPostodeTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CA_DeliveryPostodeTextBox, "CA_DeliveryPostcode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).CA_DeliveryPostcode)));
			this.CA_DeliveryPostodeTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("BillDetailsPlugInUserControl|e21f3608-12f6-44ce-8edb-4a4a747db8c3", "Post Code", "Delivery Post Code override.");
			this.CA_DeliveryPostodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 151, true);
			this.CA_DeliveryPostodeTextBox.Name = "CA_DeliveryPostodeTextBox";
			this.CA_DeliveryPostodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 20, true);
			this.CA_DeliveryPostodeTextBox.TabIndex = 6;
			// 
			// CA_DeliverySuburbTextBox
			// 
			this.CA_DeliverySuburbTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CA_DeliverySuburbTextBox, "CA_DeliverySuburb");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).CA_DeliverySuburb)));
			this.CA_DeliverySuburbTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("BillDetailsPlugInUserControl|419801c5-2e35-461d-a8db-a3d831c2f2f0", "City", "Delivery City override.");
			this.CA_DeliverySuburbTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 103, true);
			this.CA_DeliverySuburbTextBox.Name = "CA_DeliverySuburbTextBox";
			this.CA_DeliverySuburbTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.CA_DeliverySuburbTextBox.TabIndex = 4;
			// 
			// CA_DeliveryAddress2TextBox
			// 
			this.CA_DeliveryAddress2TextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CA_DeliveryAddress2TextBox, "CA_DeliveryAddress2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).CA_DeliveryAddress2)));
			this.CA_DeliveryAddress2TextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("BillDetailsPlugInUserControl|7ab04d6d-8289-486b-b60d-abdc0ee9acb8", "Address 2", "Delivery Address 2 override.");
			this.CA_DeliveryAddress2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 79, true);
			this.CA_DeliveryAddress2TextBox.Name = "CA_DeliveryAddress2TextBox";
			this.CA_DeliveryAddress2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.CA_DeliveryAddress2TextBox.TabIndex = 3;
			// 
			// CA_DeliveryAddress1TextBox
			// 
			this.CA_DeliveryAddress1TextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CA_DeliveryAddress1TextBox, "CA_DeliveryAddress1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).CA_DeliveryAddress1)));
			this.CA_DeliveryAddress1TextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("BillDetailsPlugInUserControl|733db018-4218-4645-9b20-db67cdb1164c", "Address 1", "Delivery Address 1 override.");
			this.CA_DeliveryAddress1TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 55, true);
			this.CA_DeliveryAddress1TextBox.Name = "CA_DeliveryAddress1TextBox";
			this.CA_DeliveryAddress1TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.CA_DeliveryAddress1TextBox.TabIndex = 2;
			// 
			// CA_DeliveryNameTextBox
			// 
			this.CA_DeliveryNameTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CA_DeliveryNameTextBox, "CA_DeliveryName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).CA_DeliveryName)));
			this.CA_DeliveryNameTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("BillDetailsPlugInUserControl|a98b5e4e-60c3-4fa7-bf7a-4f5d4912a784", "Name", "Delivery name override.");
			this.CA_DeliveryNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 31, true);
			this.CA_DeliveryNameTextBox.Name = "CA_DeliveryNameTextBox";
			this.CA_DeliveryNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.CA_DeliveryNameTextBox.TabIndex = 1;
			// 
			// GrouBoxHouseBill
			// 
			this.GrouBoxHouseBill.Controls.Add(this.CA_AuthenticationCodeTextBox);
			this.GrouBoxHouseBill.Controls.Add(this.CA_SpecialInstructionsTextBox);
			this.GrouBoxHouseBill.Controls.Add(this.CA_FROBTransitImportCodeDropEdit);
			this.GrouBoxHouseBill.Controls.Add(this.CA_CargoFacilityLocationTextBox);
			this.GrouBoxHouseBill.Controls.Add(this.CA_RL_NK_PortOfDestinationBoundCodeFindBox);
			this.GrouBoxHouseBill.Controls.Add(this.OverrideDefaultShipmentValuesCheckBox);
			this.GrouBoxHouseBill.Controls.Add(this.SuppRefNoTextBox);
			this.GrouBoxHouseBill.Controls.Add(this.CarrierCodeTextBox);
			this.GrouBoxHouseBill.Controls.Add(this.ShipmentStatusZTextBox);
			this.GrouBoxHouseBill.Controls.Add(this.MessageStatusZTextBox);
			this.GrouBoxHouseBill.Controls.Add(this.MessageStatusDescriptionZTextBox);
			this.GrouBoxHouseBill.Controls.Add(this.ShipmentStatusDescriptionZTextBox);
			this.GrouBoxHouseBill.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GrouBoxHouseBill.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GrouBoxHouseBill.Name = "GrouBoxHouseBill";
			this.GrouBoxHouseBill.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1082, 253, true);
			this.GrouBoxHouseBill.TabIndex = 3;
			this.GrouBoxHouseBill.TabStop = false;
			// 
			// CA_AuthenticationCodeTextBox
			// 
			this.CA_AuthenticationCodeTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CA_AuthenticationCodeTextBox, "CA_AuthenticationCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).CA_AuthenticationCode)));
			this.CA_AuthenticationCodeTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("BillDetailsPlugInUserControl|16ef7d3d-ea45-4d3a-b970-67b72d4680cc", "Authentication", "Authentication. Not required if a performance agreement is signed between the Trader and Customs.");
			this.CA_AuthenticationCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 195, true);
			this.CA_AuthenticationCodeTextBox.Name = "CA_AuthenticationCodeTextBox";
			this.CA_AuthenticationCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.CA_AuthenticationCodeTextBox.TabIndex = 10;
			// 
			// CA_SpecialInstructionsTextBox
			// 
			this.CA_SpecialInstructionsTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CA_SpecialInstructionsTextBox, "CA_SpecialInstructions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).CA_SpecialInstructions)));
			this.CA_SpecialInstructionsTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("BillDetailsPlugInUserControl|140c904c-b92b-4252-87e6-acfabeaa5af3", "Special Instr.", "Special Instructions. Directions for handling a shipment and/or delivery directions for a\r\nshipment.");
			this.CA_SpecialInstructionsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 169, true);
			this.CA_SpecialInstructionsTextBox.Name = "CA_SpecialInstructionsTextBox";
			this.CA_SpecialInstructionsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(510, 20, true);
			this.CA_SpecialInstructionsTextBox.TabIndex = 9;
			// 
			// CA_FROBTransitImportCodeDropEdit
			// 
			this.CA_FROBTransitImportCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CA_FROBTransitImportCodeDropEdit, "CA_FROBTransitImportCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).CA_FROBTransitImportCode)));
			this.CA_FROBTransitImportCodeDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("BillDetailsPlugInUserControl|b8a708ab-e635-465b-ba02-5c716bbc7666", "Application Type", "Indicate whether the report is associated to cargo being imported into Canada for domestic consumption (24), in-transit including ramp transfers (23) or freight remaining on board a conveyance that is not being offloaded at a Canadian port (FROB) (26).");
			this.CA_FROBTransitImportCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 143, true);
			this.CA_FROBTransitImportCodeDropEdit.Name = "CA_FROBTransitImportCodeDropEdit";
			this.CA_FROBTransitImportCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(190, 20, true);
			this.CA_FROBTransitImportCodeDropEdit.TabIndex = 8;
			// 
			// CA_CargoFacilityLocationTextBox
			// 
			this.CA_CargoFacilityLocationTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CA_CargoFacilityLocationTextBox, "CA_CargoFacilityLocation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).CA_CargoFacilityLocation)));
			this.CA_CargoFacilityLocationTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("BillDetailsPlugInUserControl|310cd751-c214-439a-83d6-463367bbcf50", "Port/Terminal", "Port/Terminal Name", "The name of the port/terminal where the goods are to be delivered.");
			this.CA_CargoFacilityLocationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 117, true);
			this.CA_CargoFacilityLocationTextBox.Name = "CA_CargoFacilityLocationTextBox";
			this.CA_CargoFacilityLocationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 20, true);
			this.CA_CargoFacilityLocationTextBox.TabIndex = 7;
			// 
			// CA_RL_NK_PortOfDestinationBoundCodeFindBox
			// 
			this.CA_RL_NK_PortOfDestinationBoundCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CA_RL_NK_PortOfDestinationBoundCodeFindBox, "CA_RL_NK_PortOfDestination");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).CA_RL_NK_PortOfDestination)));
			this.CA_RL_NK_PortOfDestinationBoundCodeFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("BillDetailsPlugInUserControl|982b1ce6-7381-40bf-ad0d-e6dbea5a0566", "Dest.", "Destination", "Destination", "ISO Port code of destination. The place at which the goods are destined under Customs control of transit procedure.");
			this.CA_RL_NK_PortOfDestinationBoundCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 91, true);
			this.CA_RL_NK_PortOfDestinationBoundCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			this.CA_RL_NK_PortOfDestinationBoundCodeFindBox.Name = "CA_RL_NK_PortOfDestinationBoundCodeFindBox";
			this.CA_RL_NK_PortOfDestinationBoundCodeFindBox.PopupCaption = null;
			this.CA_RL_NK_PortOfDestinationBoundCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(422, 20, true);
			this.CA_RL_NK_PortOfDestinationBoundCodeFindBox.TabIndex = 6;
			// 
			// OverrideDefaultShipmentValuesCheckBox
			// 
			this.BindingSource.SetBindingMember(this.OverrideDefaultShipmentValuesCheckBox, "CA_OverrideFreightDefaults");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).CA_OverrideFreightDefaults)));
			this.OverrideDefaultShipmentValuesCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OverrideDefaultShipmentValuesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 218, true);
			this.OverrideDefaultShipmentValuesCheckBox.Name = "OverrideDefaultShipmentValuesCheckBox";
			this.OverrideDefaultShipmentValuesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 24, true);
			this.OverrideDefaultShipmentValuesCheckBox.TabIndex = 5;
			this.OverrideDefaultShipmentValuesCheckBox.UseVisualStyleBackColor = true;
			// 
			// SuppRefNoTextBox
			// 
			this.SuppRefNoTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.SuppRefNoTextBox, "SupplementaryReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).SupplementaryReferenceNumber)));
			this.SuppRefNoTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("BillDetailsPlugInUserControl|53171cbe-631d-40f3-bcd7-13b1927ab3f1", "SRN", "Supp Ref No.", "Supplementary Reference Number", "The SRN is the reference number that has been issued for this supplementary report.It consists of the carrier code of the party transmitting the report and a unique reference number.");
			this.SuppRefNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(386, 65, true);
			this.SuppRefNoTextBox.Name = "SuppRefNoTextBox";
			this.SuppRefNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(134, 20, true);
			this.SuppRefNoTextBox.TabIndex = 5;
			// 
			// CarrierCodeTextBox
			// 
			this.CarrierCodeTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CarrierCodeTextBox, "CarrierCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).CarrierCode)));
			this.CarrierCodeTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("BillDetailsPlugInUserControl|a44739c6-6fe8-4849-a1f4-8134c9eb0ae2", "Carrier Code");
			this.CarrierCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 65, true);
			this.CarrierCodeTextBox.Name = "CarrierCodeTextBox";
			this.CarrierCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			this.CarrierCodeTextBox.TabIndex = 4;
			// 
			// ShipmentStatusZTextBox
			// 
			this.ShipmentStatusZTextBox.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.ShipmentStatusZTextBox, "CA_ShipmentStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).CA_ShipmentStatus)));
			this.ShipmentStatusZTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("BillDetailsPlugInUserControl|13f74f0f-e67e-4258-882d-fe5d1e963e82", "Shipment Status");
			this.ShipmentStatusZTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 13, true);
			this.ShipmentStatusZTextBox.Multiline = true;
			this.ShipmentStatusZTextBox.Name = "ShipmentStatusZTextBox";
			this.ShipmentStatusZTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 20, true);
			this.ShipmentStatusZTextBox.TabIndex = 0;
			// 
			// MessageStatusZTextBox
			// 
			this.MessageStatusZTextBox.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.MessageStatusZTextBox, "CA_MessageStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).CA_MessageStatus)));
			this.MessageStatusZTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("BillDetailsPlugInUserControl|255eddb8-8172-4ab5-8bcb-c510d54d8868", "Message Status");
			this.MessageStatusZTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 39, true);
			this.MessageStatusZTextBox.Multiline = true;
			this.MessageStatusZTextBox.Name = "MessageStatusZTextBox";
			this.MessageStatusZTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 20, true);
			this.MessageStatusZTextBox.TabIndex = 2;
			// 
			// MessageStatusDescriptionZTextBox
			// 
			this.MessageStatusDescriptionZTextBox.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.MessageStatusDescriptionZTextBox, "MessageStatusDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).MessageStatusDescription)));
			this.MessageStatusDescriptionZTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 39, true);
			this.MessageStatusDescriptionZTextBox.Multiline = true;
			this.MessageStatusDescriptionZTextBox.Name = "MessageStatusDescriptionZTextBox";
			this.MessageStatusDescriptionZTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(368, 20, true);
			this.MessageStatusDescriptionZTextBox.TabIndex = 3;
			// 
			// ShipmentStatusDescriptionZTextBox
			// 
			this.ShipmentStatusDescriptionZTextBox.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.ShipmentStatusDescriptionZTextBox, "ShipmentStatusDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).ShipmentStatusDescription)));
			this.ShipmentStatusDescriptionZTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 13, true);
			this.ShipmentStatusDescriptionZTextBox.Multiline = true;
			this.ShipmentStatusDescriptionZTextBox.Name = "ShipmentStatusDescriptionZTextBox";
			this.ShipmentStatusDescriptionZTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(368, 20, true);
			this.ShipmentStatusDescriptionZTextBox.TabIndex = 1;
			this.ShipmentStatusDescriptionZTextBox.TextChanged += new System.EventHandler(this.ShipmentStatusDescriptionZTextBox_TextChanged);
			// 
			// BillDetailsPlugInUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ConsigneeTabControl);
			this.Controls.Add(this.GrouBoxHouseBill);
			this.Name = "BillDetailsPlugInUserControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1082, 253, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ConsigneeTabControl.ResumeLayout(false);
			this.ConsigneeTabPage.ResumeLayout(false);
			this.ConsigneeTabPage.PerformLayout();
			this.ConsignorTabPage.ResumeLayout(false);
			this.ConsignorTabPage.PerformLayout();
			this.NotifyPartyTabPage.ResumeLayout(false);
			this.NotifyPartyTabPage.PerformLayout();
			this.DeliveryTabPage.ResumeLayout(false);
			this.DeliveryTabPage.PerformLayout();
			this.GrouBoxHouseBill.ResumeLayout(false);
			this.GrouBoxHouseBill.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private CargoWise.Windows.UI.KSplitter splitter1;
		private ZArchitecture.GUI.ZTemplateTabControl ConsigneeTabControl;
		private ZArchitecture.GUI.ZTabPage ConsigneeTabPage;
		private ZArchitecture.GUI.ZDropEdit ConsigneeStateDropEdit;
		private ZArchitecture.GUI.ZCodeFindBox ConsigneeCountryCodeFindBox;
		private ZArchitecture.GUI.ZGuidFindBox CA_OH_ConsigneeBoundGuidFindBox;
		private ZArchitecture.ZTextBox CA_ConsigneeContactNameBoundTextBox;
		private ZArchitecture.ZTextBox CA_ConsigneePhoneBoundTextBox;
		private ZArchitecture.ZTextBox CA_ConsigneePostcodeBoundTextBox;
		private ZArchitecture.ZTextBox CA_ConsigneeAddress3BoundTextBox;
		private ZArchitecture.ZTextBox CA_ConsigneeAddress2BoundTextBox;
		private ZArchitecture.ZTextBox CA_ConsigneeAddress1BoundTextBox;
		private ZArchitecture.ZTextBox CA_ConsigneeNameBoundTextBox;
		private ZArchitecture.GUI.ZTabPage ConsignorTabPage;
		private ZArchitecture.GUI.ZDropEdit ConsignorStateDropEdit;
		private ZArchitecture.ZTextBox CA_ConsignorContactNameBoundTextBox;
		private ZArchitecture.ZTextBox CA_ConsignorPhoneTextBox;
		private ZArchitecture.GUI.ZCodeFindBox ConsignorCountryCodeFindBox;
		private ZArchitecture.GUI.ZGuidFindBox CA_OH_ConsignorBoundGuidFindBox;
		private ZArchitecture.ZTextBox CA_ConsignorPostcodeBoundTextBox;
		private ZArchitecture.ZTextBox CA_ConsignorAddress3BoundTextBox;
		private ZArchitecture.ZTextBox CA_ConsignorAddress2BoundTextBox;
		private ZArchitecture.ZTextBox CA_ConsignorAddress1BoundTextBox;
		private ZArchitecture.ZTextBox CA_ConsignorNameBoundTextBox;
		private ZArchitecture.GUI.ZTabPage NotifyPartyTabPage;
		private ZArchitecture.GUI.ZDropEdit NotifyStateDropEdit;
		private ZArchitecture.GUI.ZCodeFindBox NotifyCountryCodeFindBox;
		private ZArchitecture.GUI.ZGuidFindBox CA_OH_NotifyBoundGuidFindBox;
		private ZArchitecture.ZTextBox CA_NotifyContactNameBoundTextBox;
		private ZArchitecture.ZTextBox CA_NotifyPhoneBoundTextBox;
		private ZArchitecture.ZTextBox CA_NotifyPostcodeBoundTextBox;
		private ZArchitecture.ZTextBox CA_NotifyAddress3BoundTextBox;
		private ZArchitecture.ZTextBox CA_NotifyAddress2BoundTextBox;
		private ZArchitecture.ZTextBox CA_NotifyAddress1BoundTextBox;
		private ZArchitecture.ZTextBox CA_NotifyNameBoundTextBox;
		private ZArchitecture.GUI.ZTabPage DeliveryTabPage;
		private ZArchitecture.GUI.ZDropEdit DeliveryDropEdit;
		private ZArchitecture.GUI.ZCodeFindBox CA_DeliveryCountryTextBox;
		private ZArchitecture.GUI.ZGuidFindBox CA_OA_DeliveryAddressGuidFindBox;
		private ZArchitecture.ZTextBox CA_DeliveryContactNameTextBox;
		private ZArchitecture.ZTextBox CA_DeliveryPhoneTextBox;
		private ZArchitecture.ZTextBox CA_DeliveryPostodeTextBox;
		private ZArchitecture.ZTextBox CA_DeliverySuburbTextBox;
		private ZArchitecture.ZTextBox CA_DeliveryAddress2TextBox;
		private ZArchitecture.ZTextBox CA_DeliveryAddress1TextBox;
		private ZArchitecture.ZTextBox CA_DeliveryNameTextBox;
		private ZArchitecture.GUI.ZGroupBox GrouBoxHouseBill;
		private ZArchitecture.ZTextBox CA_AuthenticationCodeTextBox;
		private ZArchitecture.ZTextBox CA_SpecialInstructionsTextBox;
		private ZArchitecture.GUI.ZDropEdit CA_FROBTransitImportCodeDropEdit;
		private ZArchitecture.ZTextBox CA_CargoFacilityLocationTextBox;
		private ZArchitecture.GUI.ZCodeFindBox CA_RL_NK_PortOfDestinationBoundCodeFindBox;
		private ZArchitecture.ZTextBox SuppRefNoTextBox;
		private ZArchitecture.ZTextBox CarrierCodeTextBox;
		private ZArchitecture.ZTextBox ShipmentStatusZTextBox;
		private ZArchitecture.ZTextBox MessageStatusZTextBox;
		private ZArchitecture.ZTextBox MessageStatusDescriptionZTextBox;
		internal ZArchitecture.ZTextBox ShipmentStatusDescriptionZTextBox;
		private ZArchitecture.GUI.ZCheckBox OverrideDefaultShipmentValuesCheckBox;
	}
}
