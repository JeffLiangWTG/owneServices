using Enterprise.Customs.JP.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.JP.GUI
{
	partial class JobDeclarationUserControl
	{

		void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(JobDeclarationUserControl));
			this.ImporterDocAddressControl = new Enterprise.Customs.JP.GUI.JPDocAddressControl();
			this.SupplierDocAddressControl = new Enterprise.Customs.JP.GUI.JPDocAddressControl();
			this.CustomsOfficeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CustomsOfficeDepartmentDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.FinalDestinationNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ReceiptModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DeliveryModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ArrivalAtLoadingDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.RadioCallSignCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.OwnerSectionCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JE_MessageTypeBoundDropDownEdit.SuspendLayout();
			this.JE_TransportModeBoundDropDownEdit.SuspendLayout();
			this.JE_ContainerModeBoundDropDownEdit.SuspendLayout();
			this.JE_MessageSubTypeBoundDropDownEdit.SuspendLayout();
			this.WeightzCalcDropEdit.SuspendLayout();
			this.ScreeningStatusDropEdit.SuspendLayout();
			this.TotalNoOfPacksCalcDropEdit.SuspendLayout();
			this.JE_MasterBillForAirBoundTextBox.SuspendLayout();
			this.VesselFindBox.SuspendLayout();
			this.JE_ExportDateBoundDateEdit.SuspendLayout();
			this.JE_DateOfArrivalBoundDateEdit.SuspendLayout();
			this.PortOfDischargeFindBox.SuspendLayout();
			this.PortOfLoadingFindBox.SuspendLayout();
			this.ShipmentDetailsGroupBox.SuspendLayout();
			this.ShipmentTypeGroupBox.SuspendLayout();
			this.ImporterOrganisationControl.SuspendLayout();
			this.SupplierOrganisationControl.SuspendLayout();
			this.FinalDestinationFindBox.SuspendLayout();
			this.JE_DateOfArrivalBoundDateEdit2.SuspendLayout();
			this.JE_ExportDateBoundDateEdit2.SuspendLayout();
			this.OriginFindBox.SuspendLayout();
			this.IncoTermDropEdit.SuspendLayout();
			this.RightTabControl.SuspendLayout();
			this.OrganisationsTabPage.SuspendLayout();
			this.OrganisationsTopPanel.SuspendLayout();
			this.ShippingOrAirLineOrganisationControl.SuspendLayout();
			this.ForwarderOrganisationControl.SuspendLayout();
			this.OrdersTabPage.SuspendLayout();
			this.JE_RS_NKServiceLevelBoundFindBox.SuspendLayout();
			this.DocsTabPage.SuspendLayout();
			this.VolumeCalcDropEdit.SuspendLayout();
			this.ContainerTerminalOperatorAddressControl.SuspendLayout();
			this.ShipmentCustomFieldsPage.SuspendLayout();
			this.shipmentCustomFieldsControl1.SuspendLayout();
			this.OrdersPanel.SuspendLayout();
			this.DepotAddressControl.SuspendLayout();
			this.BondedWarehouseDocAddressControl.SuspendLayout();
			this.ContainerYardAddressControl.SuspendLayout();
			this.NumbersTabPage.SuspendLayout();
			this.OrdersAttachUserControl.SuspendLayout();
			this.JE_ApplicationCodeBoundDropEdit.SuspendLayout();
			this.ExternalBrokerGuidFindBox.SuspendLayout();
			this.ControllingCustomerGuidFindBox.SuspendLayout();
			this.ControllingAgentGuidFindBox.SuspendLayout();
			this.TransportDetailsGroupBox.SuspendLayout();
			this.DeclarationDetailsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ImporterDocAddressControl.SuspendLayout();
			this.SupplierDocAddressControl.SuspendLayout();
			this.CustomsOfficeDropEdit.SuspendLayout();
			this.CustomsOfficeDepartmentDropEdit.SuspendLayout();
			this.ReceiptModeDropEdit.SuspendLayout();
			this.DeliveryModeDropEdit.SuspendLayout();
			this.ArrivalAtLoadingDateEdit.SuspendLayout();
			this.RadioCallSignCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// JE_MessageTypeBoundDropDownEdit
			// 
			this.JE_MessageTypeBoundDropDownEdit.TabIndex = 0;
			// 
			// JE_TransportModeBoundDropDownEdit
			// 
			this.JE_TransportModeBoundDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 67, true);
			// 
			// JE_ContainerModeBoundDropDownEdit
			// 
			this.JE_ContainerModeBoundDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 93, true);
			// 
			// JE_MessageSubTypeBoundDropDownEdit
			// 
			this.JE_MessageSubTypeBoundDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 171, true);
			this.JE_MessageSubTypeBoundDropDownEdit.TabIndex = 2;
			this.JE_MessageSubTypeBoundDropDownEdit.Visible = false;
			// 
			// WeightzCalcDropEdit
			// 
			this.WeightzCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 207, true);
			this.WeightzCalcDropEdit.TabIndex = 10;
			// 
			// OwnersReferenceTextBox
			// 
			this.OwnersReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 183, true);
			this.OwnersReferenceTextBox.TabIndex = 9;
			// 
			// ScreeningStatusDropEdit
			// 
			this.ScreeningStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 327, true);
			this.ScreeningStatusDropEdit.TabIndex = 18;
			// 
			// ScreenButton
			// 
			this.ScreenButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(237, 327, true);
			this.ScreenButton.TabIndex = 19;
			// 
			// TotalNoOfPacksCalcDropEdit
			// 
			this.TotalNoOfPacksCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 279, true);
			this.TotalNoOfPacksCalcDropEdit.TabIndex = 15;
			// 
			// GoodsDescriptionTextBox
			// 
			this.GoodsDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 136, true);
			this.GoodsDescriptionTextBox.TabIndex = 7;
			// 
			// JE_MasterBillForAirBoundTextBox
			// 
			this.JE_MasterBillForAirBoundTextBox.TabIndex = 1;
			// 
			// FolioNumberTextBox
			// 
			this.FolioNumberTextBox.TabIndex = 5;
			// 
			// VesselFindBox
			// 
			this.VesselFindBox.BindToForDescription = "Vessel+RV_RadioCallSign";
			this.VesselFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(339, 20, true);
			this.VesselFindBox.TabIndex = 3;
			// 
			// JE_MasterBillForSeaBoundTextBox
			// 
			this.JE_MasterBillForSeaBoundTextBox.TabIndex = 2;
			// 
			// JE_VoyageFlightNoBoundTextBox
			// 
			this.JE_VoyageFlightNoBoundTextBox.TabIndex = 4;
			// 
			// JE_ExportDateBoundDateEdit
			// 
			this.JE_ExportDateBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(351, 114, true);
			this.JE_ExportDateBoundDateEdit.TabIndex = 9;
			// 
			// JE_DateOfArrivalBoundDateEdit
			// 
			this.JE_DateOfArrivalBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(351, 159, true);
			this.JE_DateOfArrivalBoundDateEdit.TabIndex = 11;
			// 
			// PortOfDischargeFindBox
			// 
			this.PortOfDischargeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 160, true);
			this.PortOfDischargeFindBox.TabIndex = 10;
			// 
			// PortOfLoadingFindBox
			// 
			this.PortOfLoadingFindBox.BindToForDescription = "PortOfLoadingDescription";
			this.PortOfLoadingFindBox.TabIndex = 7;
			// 
			// ShipmentDetailsGroupBox
			// 
			this.ShipmentDetailsGroupBox.Controls.Add(this.OwnerSectionCodeTextBox);
			this.ShipmentDetailsGroupBox.Controls.Add(this.DeliveryModeDropEdit);
			this.ShipmentDetailsGroupBox.Controls.Add(this.ReceiptModeDropEdit);
			this.ShipmentDetailsGroupBox.Controls.Add(this.CustomsOfficeDepartmentDropEdit);
			this.ShipmentDetailsGroupBox.Controls.Add(this.CustomsOfficeDropEdit);
			this.ShipmentDetailsGroupBox.Controls.Add(this.FinalDestinationNameTextBox);
			this.ShipmentDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 245, true);
			this.ShipmentDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(456, 444, true);
			this.ShipmentDetailsGroupBox.TabIndex = 6;
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.FinalDestinationNameTextBox, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.GoodsDescriptionTextBox, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.TotalNoOfPacksCalcDropEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.OwnersReferenceTextBox, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.JE_ContainerCountCalcEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.WeightzCalcDropEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.VolumeCalcDropEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.HouseBillParcelPostTextEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.OriginFindBox, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.JE_ExportDateBoundDateEdit2, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.JE_DateOfArrivalBoundDateEdit2, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.FinalDestinationFindBox, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.IncoTermDropEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.JE_TotalNoOfPiecesBoundCalcEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.IncoTermExplainButton, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.ScreeningStatusDropEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.ScreenButton, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.CustomsOfficeDropEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.CustomsOfficeDepartmentDropEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.ReceiptModeDropEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.DeliveryModeDropEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.OwnerSectionCodeTextBox, 0);
			// 
			// ShipmentTypeGroupBox
			// 
			this.ShipmentTypeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 384, true);
			this.ShipmentTypeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 240, true);
			this.ShipmentTypeGroupBox.TabIndex = 3;
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.JE_ContainerModeBoundDropDownEdit, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.JE_TransportModeBoundDropDownEdit, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.JE_MessageTypeBoundDropDownEdit, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.JE_MessageSubTypeBoundDropDownEdit, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.JE_RS_NKServiceLevelBoundFindBox, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.JE_ApplicationCodeBoundDropEdit, 0);
			// 
			// FinalDestinationFindBox
			// 
			this.FinalDestinationFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 88, true);
			this.FinalDestinationFindBox.TabIndex = 4;
			// 
			// JE_DateOfArrivalBoundDateEdit2
			// 
			this.JE_DateOfArrivalBoundDateEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(352, 88, true);
			this.JE_DateOfArrivalBoundDateEdit2.TabIndex = 5;
			// 
			// JE_ExportDateBoundDateEdit2
			// 
			this.JE_ExportDateBoundDateEdit2.TabIndex = 2;
			// 
			// OriginFindBox
			// 
			this.OriginFindBox.TabIndex = 1;
			// 
			// HouseBillParcelPostTextEdit
			// 
			this.HouseBillParcelPostTextEdit.TabIndex = 0;
			// 
			// IncoTermDropEdit
			// 
			this.IncoTermDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 303, true);
			this.IncoTermDropEdit.TabIndex = 16;
			// 
			// RightTabControl
			// 
			this.RightTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(267, 574, true);
			this.RightTabControl.TabIndex = 7;
			// 
			// OrganisationsTabPage
			// 
			this.OrganisationsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 21, true);
			this.OrganisationsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 548, true);
			// 
			// OrganisationsTopPanel
			// 
			this.OrganisationsTopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 206, true);
			// 
			// OrdersTabPage
			// 
			this.OrdersTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 21, true);
			this.OrdersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 548, true);
			// 
			// JE_RS_NKServiceLevelBoundFindBox
			// 
			this.JE_RS_NKServiceLevelBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 119, true);
			this.JE_RS_NKServiceLevelBoundFindBox.TabIndex = 8;
			// 
			// DocsTabPage
			// 
			this.DocsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 21, true);
			this.DocsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 548, true);
			// 
			// JE_ContainerCountCalcEdit
			// 
			this.JE_ContainerCountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 255, true);
			this.JE_ContainerCountCalcEdit.TabIndex = 14;
			// 
			// JE_TotalNoOfPiecesBoundCalcEdit
			// 
			this.JE_TotalNoOfPiecesBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 255, true);
			this.JE_TotalNoOfPiecesBoundCalcEdit.TabIndex = 13;
			// 
			// IncoTermExplainButton
			// 
			this.IncoTermExplainButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(424, 298, true);
			this.IncoTermExplainButton.TabIndex = 17;
			// 
			// OverrideValuesCheckBox
			// 
			this.OverrideValuesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(205, 15, true);
			// 
			// VolumeCalcDropEdit
			// 
			this.VolumeCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 231, true);
			this.VolumeCalcDropEdit.TabIndex = 12;
			// 
			// ShipmentCustomFieldsPage
			// 
			this.ShipmentCustomFieldsPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 21, true);
			this.ShipmentCustomFieldsPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 548, true);
			// 
			// shipmentCustomFieldsControl1
			// 
			this.shipmentCustomFieldsControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 548, true);
			// 
			// OrdersPanel
			// 
			this.OrdersPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 548, true);
			// 
			// NumbersTabPage
			// 
			this.NumbersTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 21, true);
			this.NumbersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 548, true);
			// 
			// JE_ApplicationCodeBoundDropEdit
			// 
			this.JE_ApplicationCodeBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 145, true);
			this.JE_ApplicationCodeBoundDropEdit.TabIndex = 9;
			// 
			// TransportDetailsGroupBox
			// 
			this.TransportDetailsGroupBox.Controls.Add(this.RadioCallSignCodeFindBox);
			this.TransportDetailsGroupBox.Controls.Add(this.ArrivalAtLoadingDateEdit);
			this.TransportDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(456, 185, true);
			this.TransportDetailsGroupBox.TabIndex = 5;
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.JE_VoyageFlightNoBoundTextBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.JE_MasterBillForSeaBoundTextBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.VesselFindBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.FolioNumberTextBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.JE_MasterBillForAirBoundTextBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.PortOfLoadingFindBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.PortOfDischargeFindBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.JE_DateOfArrivalBoundDateEdit, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.JE_ExportDateBoundDateEdit, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.OverrideValuesCheckBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.ArrivalAtLoadingDateEdit, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.RadioCallSignCodeFindBox, 0);
			// 
			// DeclarationDetailsGroupBox
			// 
			this.DeclarationDetailsGroupBox.TabIndex = 4;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.JP.Business.JobDeclaration);
			// 
			// ImporterDocAddressControl
			// 
			this.ImporterDocAddressControl.AddressValidationProcessCmdKey = null;
			this.ImporterDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ImporterDocAddressControl, "ImporterDocumentaryAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).ImporterDocumentaryAddress)));
			this.ImporterDocAddressControl.BindToOrganisations = "Lookups.ImportersList";
			this.ImporterDocAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.ShowOverrideAndTabs;
			this.ImporterDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 192, true);
			this.ImporterDocAddressControl.Name = "ImporterDocAddressControl";
			this.ImporterDocAddressControl.ReadOnly = false;
			this.ImporterDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.ImporterDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.ImporterDocAddressControl.TabIndex = 1;
			this.ImporterDocAddressControl.ValidationJustForced = false;
			this.ImporterDocAddressControl.ContactTabPageTabVisible = false;
			// 
			// SupplierDocAddressControl
			// 
			this.SupplierDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SupplierDocAddressControl, "SupplierDocumentaryAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).SupplierDocumentaryAddress)));
			this.SupplierDocAddressControl.BindToOrganisations = "Lookups.SuppliersList";
			this.SupplierDocAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.ShowOverrideAndTabs;
			this.SupplierDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 8, true);
			this.SupplierDocAddressControl.Name = "SupplierDocAddressControl";
			this.SupplierDocAddressControl.ReadOnly = false;
			this.SupplierDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.SupplierDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.SupplierDocAddressControl.TabIndex = 0;
			this.SupplierDocAddressControl.ValidationJustForced = false;
			this.SupplierDocAddressControl.ContactTabPageTabVisible = false;
			// 
			// CustomsOfficeDropEdit
			// 
			this.CustomsOfficeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsOfficeDropEdit, "JE_CustomsOffice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).JE_CustomsOffice)));
			this.CustomsOfficeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 351, true);
			this.CustomsOfficeDropEdit.Name = "CustomsOfficeDropEdit";
			this.CustomsOfficeDropEdit.PreBoundMaxLength = 2;
			this.CustomsOfficeDropEdit.ShouldResizeByMaxLength = false;
			this.CustomsOfficeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 16, true);
			this.CustomsOfficeDropEdit.TabIndex = 20;
			// 
			// CustomsOfficeDepartmentDropEdit
			// 
			this.CustomsOfficeDepartmentDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsOfficeDepartmentDropEdit, "JE_CustomsOfficeDepartment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).JE_CustomsOfficeDepartment)));
			this.CustomsOfficeDepartmentDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 375, true);
			this.CustomsOfficeDepartmentDropEdit.Name = "CustomsOfficeDepartmentDropEdit";
			this.CustomsOfficeDepartmentDropEdit.PreBoundMaxLength = 2;
			this.CustomsOfficeDepartmentDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 16, true);
			this.CustomsOfficeDepartmentDropEdit.TabIndex = 21;
			// 
			// FinalDestinationNameTextBox
			// 
			this.FinalDestinationNameTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.FinalDestinationNameTextBox, "JE_FinalDestinationName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).JE_FinalDestinationName)));
			this.FinalDestinationNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 88, true);
			this.FinalDestinationNameTextBox.Name = "FinalDestinationNameTextBox";
			this.FinalDestinationNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 16, true);
			this.FinalDestinationNameTextBox.TabIndex = 8;
			// 
			// ReceiptModeDropEdit
			// 
			this.ReceiptModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ReceiptModeDropEdit, "JE_ReceiptMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).JE_ReceiptMode)));
			this.ReceiptModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(352, 64, true);
			this.ReceiptModeDropEdit.Name = "ReceiptModeDropEdit";
			this.ReceiptModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 16, true);
			this.ReceiptModeDropEdit.TabIndex = 3;
			// 
			// DeliveryModeDropEdit
			// 
			this.DeliveryModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DeliveryModeDropEdit, "JE_DeliveryMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).JE_DeliveryMode)));
			this.DeliveryModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(352, 112, true);
			this.DeliveryModeDropEdit.Name = "DeliveryModeDropEdit";
			this.DeliveryModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 16, true);
			this.DeliveryModeDropEdit.TabIndex = 6;
			// 
			// ArrivalAtLoadingDateEdit
			// 
			this.ArrivalAtLoadingDateEdit.AllowDrop = true;
			this.ArrivalAtLoadingDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.ArrivalAtLoadingDateEdit, "JE_ArrivalAtLoadingDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).JE_ArrivalAtLoadingDate)));
			this.ArrivalAtLoadingDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(351, 88, true);
			this.ArrivalAtLoadingDateEdit.Name = "ArrivalAtLoadingDateEdit";
			this.ArrivalAtLoadingDateEdit.TabIndex = 8;
			// 
			// RadioCallSignCodeFindBox
			// 
			this.RadioCallSignCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RadioCallSignCodeFindBox, "JE_RadioCallSign");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).JE_RadioCallSign)));
			this.RadioCallSignCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(341, 64, true);
			this.RadioCallSignCodeFindBox.Name = "RadioCallSignCodeFindBox";
			this.RadioCallSignCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefVesselZZ;
			this.RadioCallSignCodeFindBox.ParentType = null;
			this.RadioCallSignCodeFindBox.ShowDescriptionBox = false;
			this.RadioCallSignCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 16, true);
			this.RadioCallSignCodeFindBox.TabIndex = 6;
			// 
			// OwnerSectionCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.OwnerSectionCodeTextBox, "JE_OwnerSectionCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).JE_OwnerSectionCode)));
			this.OwnerSectionCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 160, true);
			this.OwnerSectionCodeTextBox.Name = "OwnerSectionCodeTextBox";
			this.OwnerSectionCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(325, 16, true);
			this.OwnerSectionCodeTextBox.TabIndex = 8;
			// 
			// JobDeclarationUserControl
			// 
			this.Controls.Add(this.SupplierDocAddressControl);
			this.Controls.Add(this.ImporterDocAddressControl);
			this.Name = "JobDeclarationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1003, 598, true);
			this.Controls.SetChildIndex(this.ImporterDocAddressControl, 0);
			this.Controls.SetChildIndex(this.SupplierDocAddressControl, 0);
			this.Controls.SetChildIndex(this.RequiresMergeLabel, 0);
			this.Controls.SetChildIndex(this.SupplierOrganisationControl, 0);
			this.Controls.SetChildIndex(this.ShipmentTypeGroupBox, 0);
			this.Controls.SetChildIndex(this.ImporterOrganisationControl, 0);
			this.Controls.SetChildIndex(this.ShipmentDetailsGroupBox, 0);
			this.Controls.SetChildIndex(this.TransportDetailsGroupBox, 0);
			this.Controls.SetChildIndex(this.RightTabControl, 0);
			this.Controls.SetChildIndex(this.DeclarationDetailsGroupBox, 0);
			this.JE_MessageTypeBoundDropDownEdit.ResumeLayout(true);
			this.JE_MessageTypeBoundDropDownEdit.PerformLayout();
			this.JE_TransportModeBoundDropDownEdit.ResumeLayout(true);
			this.JE_TransportModeBoundDropDownEdit.PerformLayout();
			this.JE_ContainerModeBoundDropDownEdit.ResumeLayout(true);
			this.JE_ContainerModeBoundDropDownEdit.PerformLayout();
			this.JE_MessageSubTypeBoundDropDownEdit.ResumeLayout(true);
			this.JE_MessageSubTypeBoundDropDownEdit.PerformLayout();
			this.WeightzCalcDropEdit.ResumeLayout(true);
			this.WeightzCalcDropEdit.PerformLayout();
			this.ScreeningStatusDropEdit.ResumeLayout(true);
			this.ScreeningStatusDropEdit.PerformLayout();
			this.TotalNoOfPacksCalcDropEdit.ResumeLayout(true);
			this.TotalNoOfPacksCalcDropEdit.PerformLayout();
			this.JE_MasterBillForAirBoundTextBox.ResumeLayout(true);
			this.JE_MasterBillForAirBoundTextBox.PerformLayout();
			this.VesselFindBox.ResumeLayout(true);
			this.VesselFindBox.PerformLayout();
			this.JE_ExportDateBoundDateEdit.ResumeLayout(true);
			this.JE_ExportDateBoundDateEdit.PerformLayout();
			this.JE_DateOfArrivalBoundDateEdit.ResumeLayout(true);
			this.JE_DateOfArrivalBoundDateEdit.PerformLayout();
			this.PortOfDischargeFindBox.ResumeLayout(true);
			this.PortOfDischargeFindBox.PerformLayout();
			this.PortOfLoadingFindBox.ResumeLayout(true);
			this.PortOfLoadingFindBox.PerformLayout();
			this.ShipmentDetailsGroupBox.ResumeLayout(false);
			this.ShipmentDetailsGroupBox.PerformLayout();
			this.ShipmentTypeGroupBox.ResumeLayout(false);
			this.ShipmentTypeGroupBox.PerformLayout();
			this.ImporterOrganisationControl.ResumeLayout(true);
			this.ImporterOrganisationControl.PerformLayout();
			this.SupplierOrganisationControl.ResumeLayout(true);
			this.SupplierOrganisationControl.PerformLayout();
			this.FinalDestinationFindBox.ResumeLayout(true);
			this.FinalDestinationFindBox.PerformLayout();
			this.JE_DateOfArrivalBoundDateEdit2.ResumeLayout(true);
			this.JE_DateOfArrivalBoundDateEdit2.PerformLayout();
			this.JE_ExportDateBoundDateEdit2.ResumeLayout(true);
			this.JE_ExportDateBoundDateEdit2.PerformLayout();
			this.OriginFindBox.ResumeLayout(true);
			this.OriginFindBox.PerformLayout();
			this.IncoTermDropEdit.ResumeLayout(true);
			this.IncoTermDropEdit.PerformLayout();
			this.RightTabControl.ResumeLayout(false);
			this.RightTabControl.PerformLayout();
			this.OrganisationsTabPage.ResumeLayout(false);
			this.OrganisationsTabPage.PerformLayout();
			this.OrganisationsTopPanel.ResumeLayout(false);
			this.OrganisationsTopPanel.PerformLayout();
			this.ShippingOrAirLineOrganisationControl.ResumeLayout(true);
			this.ShippingOrAirLineOrganisationControl.PerformLayout();
			this.ForwarderOrganisationControl.ResumeLayout(true);
			this.ForwarderOrganisationControl.PerformLayout();
			this.OrdersTabPage.ResumeLayout(false);
			this.OrdersTabPage.PerformLayout();
			this.JE_RS_NKServiceLevelBoundFindBox.ResumeLayout(true);
			this.JE_RS_NKServiceLevelBoundFindBox.PerformLayout();
			this.DocsTabPage.ResumeLayout(false);
			this.DocsTabPage.PerformLayout();
			this.VolumeCalcDropEdit.ResumeLayout(true);
			this.VolumeCalcDropEdit.PerformLayout();
			this.ContainerTerminalOperatorAddressControl.ResumeLayout(true);
			this.ContainerTerminalOperatorAddressControl.PerformLayout();
			this.ShipmentCustomFieldsPage.ResumeLayout(false);
			this.ShipmentCustomFieldsPage.PerformLayout();
			this.shipmentCustomFieldsControl1.ResumeLayout(true);
			this.shipmentCustomFieldsControl1.PerformLayout();
			this.OrdersPanel.ResumeLayout(false);
			this.OrdersPanel.PerformLayout();
			this.DepotAddressControl.ResumeLayout(true);
			this.DepotAddressControl.PerformLayout();
			this.BondedWarehouseDocAddressControl.ResumeLayout(true);
			this.BondedWarehouseDocAddressControl.PerformLayout();
			this.ContainerYardAddressControl.ResumeLayout(true);
			this.ContainerYardAddressControl.PerformLayout();
			this.NumbersTabPage.ResumeLayout(false);
			this.NumbersTabPage.PerformLayout();
			this.OrdersAttachUserControl.ResumeLayout(true);
			this.OrdersAttachUserControl.PerformLayout();
			this.JE_ApplicationCodeBoundDropEdit.ResumeLayout(true);
			this.JE_ApplicationCodeBoundDropEdit.PerformLayout();
			this.ExternalBrokerGuidFindBox.ResumeLayout(true);
			this.ExternalBrokerGuidFindBox.PerformLayout();
			this.ControllingCustomerGuidFindBox.ResumeLayout(true);
			this.ControllingCustomerGuidFindBox.PerformLayout();
			this.ControllingAgentGuidFindBox.ResumeLayout(true);
			this.ControllingAgentGuidFindBox.PerformLayout();
			this.TransportDetailsGroupBox.ResumeLayout(false);
			this.TransportDetailsGroupBox.PerformLayout();
			this.DeclarationDetailsGroupBox.ResumeLayout(false);
			this.DeclarationDetailsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ImporterDocAddressControl.ResumeLayout(true);
			this.ImporterDocAddressControl.PerformLayout();
			this.SupplierDocAddressControl.ResumeLayout(true);
			this.SupplierDocAddressControl.PerformLayout();

			this.CustomsOfficeDropEdit.ResumeLayout(true);
			this.CustomsOfficeDropEdit.PerformLayout();
			this.CustomsOfficeDepartmentDropEdit.ResumeLayout(true);
			this.CustomsOfficeDepartmentDropEdit.PerformLayout();
			this.ReceiptModeDropEdit.ResumeLayout(true);
			this.ReceiptModeDropEdit.PerformLayout();
			this.DeliveryModeDropEdit.ResumeLayout(true);
			this.DeliveryModeDropEdit.PerformLayout();
			this.ArrivalAtLoadingDateEdit.ResumeLayout(true);
			this.ArrivalAtLoadingDateEdit.PerformLayout();
			this.RadioCallSignCodeFindBox.ResumeLayout(true);
			this.RadioCallSignCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		ZArchitecture.GUI.ZDropEdit CustomsOfficeDropEdit;
		ZArchitecture.GUI.ZDropEdit CustomsOfficeDepartmentDropEdit;
		public JPDocAddressControl ImporterDocAddressControl;
		public JPDocAddressControl SupplierDocAddressControl;
		ZArchitecture.ZTextBox FinalDestinationNameTextBox;
		Enterprise.ZArchitecture.GUI.ZDropEdit ReceiptModeDropEdit;
		Enterprise.ZArchitecture.GUI.ZDropEdit DeliveryModeDropEdit;
		public ZArchitecture.GUI.ZDateEdit ArrivalAtLoadingDateEdit;
		ZArchitecture.GUI.ZCodeFindBox RadioCallSignCodeFindBox;
		private ZArchitecture.ZTextBox OwnerSectionCodeTextBox;
	}
}
