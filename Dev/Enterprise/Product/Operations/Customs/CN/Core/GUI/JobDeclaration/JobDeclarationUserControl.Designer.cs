using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CN.GUI
{
	public partial class CNJobDeclarationUserControl
	{
		void InitializeComponent()
		{
			this.ManufacturerDocAddressControl = new Enterprise.Customs.CN.GUI.CNJobDocAddressControl();
			this.JE_CustomsOfficeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.JE_RN_NKCountryOfTradeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.JE_CNTransportModeDropDownEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.JE_CIQOfficeOfEntryExitCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.JE_CNPortOfOriginCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.JE_CNPortOfDestinationCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.JE_OfficeOfEntryExitCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ReceiptNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JE_CNLastPortBeforeEntryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.PortOfStopoverCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CNCountryOfLoadingTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CNCountryOfArrivalTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LocationOfGoodsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CIQDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OfficeOfDestinationCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.JE_DateOfUnloadCompleteDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JE_MarksAndNumbersLongTextBox = new Enterprise.Customs.GUI.LongTextControl();
			this.BuyerDocAddressControl = new Enterprise.Customs.CN.GUI.CNJobDocAddressControl();
			this.ImporterDocAddressControl = new Enterprise.Customs.CN.GUI.CNJobDocAddressControl();
			this.SupplierDocAddressControl = new Enterprise.Customs.CN.GUI.CNJobDocAddressControl();
			this.OrganisationsButtomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.JE_ClearanceModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.JE_LicenseInvolvedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.JE_InspectionInvolvedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.JE_TaxInvolvedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ClearanceModeAndCheckBoxesDescriptionLabel = new Enterprise.ZArchitecture.ZLabel();
			this.FullValidationCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.JE_TransitModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.JE_TransportModeInlandDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.JE_VoyageInlandTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JE_VesselInlandTextBox = new Enterprise.ZArchitecture.ZTextBox();
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
			this.ManufacturerDocAddressControl.SuspendLayout();
			this.JE_CustomsOfficeCodeFindBox.SuspendLayout();
			this.JE_RN_NKCountryOfTradeCodeFindBox.SuspendLayout();
			this.JE_CNTransportModeDropDownEdit.SuspendLayout();
			this.JE_CIQOfficeOfEntryExitCodeFindBox.SuspendLayout();
			this.JE_CNPortOfOriginCodeFindBox.SuspendLayout();
			this.JE_CNPortOfDestinationCodeFindBox.SuspendLayout();
			this.JE_OfficeOfEntryExitCodeFindBox.SuspendLayout();
			this.JE_CNLastPortBeforeEntryCodeFindBox.SuspendLayout();
			this.PortOfStopoverCodeFindBox.SuspendLayout();
			this.CIQDetailsGroupBox.SuspendLayout();
			this.OfficeOfDestinationCodeFindBox.SuspendLayout();
			this.JE_DateOfUnloadCompleteDateEdit.SuspendLayout();
			this.JE_MarksAndNumbersLongTextBox.SuspendLayout();
			this.BuyerDocAddressControl.SuspendLayout();
			this.ImporterDocAddressControl.SuspendLayout();
			this.SupplierDocAddressControl.SuspendLayout();
			this.OrganisationsButtomPanel.SuspendLayout();
			this.JE_ClearanceModeDropEdit.SuspendLayout();
			this.JE_TransitModeDropEdit.SuspendLayout();
			this.JE_TransportModeInlandDropEdit.SuspendLayout();
			this.SuspendLayout();
			//
			// JE_MessageTypeBoundDropDownEdit
			//
			this.JE_MessageTypeBoundDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 19, true);
			this.JE_MessageTypeBoundDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(132, 20, true);
			//
			// JE_TransportModeBoundDropDownEdit
			//
			this.JE_TransportModeBoundDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 65, true);
			this.JE_TransportModeBoundDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(132, 20, true);
			this.JE_TransportModeBoundDropDownEdit.TabIndex = 3;
			//
			// JE_ContainerModeBoundDropDownEdit
			//
			this.JE_ContainerModeBoundDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 111, true);
			this.JE_ContainerModeBoundDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(132, 20, true);
			this.JE_ContainerModeBoundDropDownEdit.TabIndex = 5;
			//
			// JE_MessageSubTypeBoundDropDownEdit
			//
			this.JE_MessageSubTypeBoundDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 42, true);
			this.JE_MessageSubTypeBoundDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(132, 20, true);
			this.JE_MessageSubTypeBoundDropDownEdit.TabIndex = 2;
			//
			// WeightzCalcDropEdit
			//
			this.WeightzCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(316, 208, true);
			this.WeightzCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(132, 20, true);
			this.WeightzCalcDropEdit.TabIndex = 17;
			//
			// OwnersReferenceTextBox
			//
			this.OwnersReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 136, true);
			this.OwnersReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(336, 20, true);
			//
			// ScreeningStatusDropEdit
			//
			this.ScreeningStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 280, true);
			this.ScreeningStatusDropEdit.TabIndex = 27;
			//
			// ScreenButton
			//
			this.ScreenButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(238, 280, true);
			this.ScreenButton.TabIndex = 28;
			//
			// TotalNoOfPacksCalcDropEdit
			//
			this.TotalNoOfPacksCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(316, 232, true);
			this.TotalNoOfPacksCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(132, 20, true);
			//
			// GoodsDescriptionTextBox
			//
			this.GoodsDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 112, true);
			this.GoodsDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(336, 20, true);
			this.GoodsDescriptionTextBox.TabIndex = 12;
			//
			// JE_MasterBillForAirBoundTextBox
			//
			this.JE_MasterBillForAirBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 16, true);
			this.JE_MasterBillForAirBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 20, true);
			//
			// FolioNumberTextBox
			//
			this.FolioNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(300, 64, true);
			this.FolioNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(148, 20, true);
			//
			// VesselFindBox
			//
			this.VesselFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 40, true);
			this.VesselFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 20, true);
			//
			// JE_MasterBillForSeaBoundTextBox
			//
			this.JE_MasterBillForSeaBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 16, true);
			this.JE_MasterBillForSeaBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 20, true);
			//
			// JE_VoyageFlightNoBoundTextBox
			//
			this.JE_VoyageFlightNoBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 64, true);
			this.JE_VoyageFlightNoBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(194, 20, true);
			//
			// JE_ExportDateBoundDateEdit
			//
			this.JE_ExportDateBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(363, 88, true);
			this.JE_ExportDateBoundDateEdit.TabIndex = 13;
			//
			// JE_DateOfArrivalBoundDateEdit
			//
			this.JE_DateOfArrivalBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(363, 112, true);
			//
			// PortOfDischargeFindBox
			//
			this.PortOfDischargeFindBox.CaptionResourceString = Enterprise.Customs.CN.GUI.Res.GetData("CNJobDeclarationUserControl|67b37f8f-9534-485b-ab80-ffd8f8f6a50f", "Discharge", "Port Of Discharge", "The Port at which the Vessel/Craft will arrive at in the Discharge Country. Country of this port will be submitted as Country Of Discharge.");
			this.PortOfDischargeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 112, true);
			this.PortOfDischargeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			//
			// PortOfLoadingFindBox
			//
			this.PortOfLoadingFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 88, true);
			this.PortOfLoadingFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.PortOfLoadingFindBox.TabIndex = 11;
			//
			// ShipmentDetailsGroupBox
			//
			this.ShipmentDetailsGroupBox.Controls.Add(this.JE_MarksAndNumbersLongTextBox);
			this.ShipmentDetailsGroupBox.Controls.Add(this.PortOfStopoverCodeFindBox);
			this.ShipmentDetailsGroupBox.Controls.Add(this.LocationOfGoodsTextBox);
			this.ShipmentDetailsGroupBox.Controls.Add(this.JE_RN_NKCountryOfTradeCodeFindBox);
			this.ShipmentDetailsGroupBox.Controls.Add(this.JE_CNLastPortBeforeEntryCodeFindBox);
			this.ShipmentDetailsGroupBox.Controls.Add(this.JE_CNPortOfOriginCodeFindBox);
			this.ShipmentDetailsGroupBox.Controls.Add(this.JE_CNPortOfDestinationCodeFindBox);
			this.ShipmentDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 194, true);
			this.ShipmentDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(456, 307, true);
			this.ShipmentDetailsGroupBox.TabIndex = 4;
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.GoodsDescriptionTextBox, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.TotalNoOfPacksCalcDropEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.OwnersReferenceTextBox, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.JE_ContainerCountCalcEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.WeightzCalcDropEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.VolumeCalcDropEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.JE_CNPortOfDestinationCodeFindBox, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.JE_CNPortOfOriginCodeFindBox, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.HouseBillParcelPostTextEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.OriginFindBox, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.JE_CNLastPortBeforeEntryCodeFindBox, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.JE_ExportDateBoundDateEdit2, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.JE_DateOfArrivalBoundDateEdit2, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.FinalDestinationFindBox, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.IncoTermDropEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.JE_TotalNoOfPiecesBoundCalcEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.IncoTermExplainButton, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.ScreeningStatusDropEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.ScreenButton, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.JE_RN_NKCountryOfTradeCodeFindBox, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.LocationOfGoodsTextBox, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.PortOfStopoverCodeFindBox, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.JE_MarksAndNumbersLongTextBox, 0);
			//
			// ShipmentTypeGroupBox
			//
			this.ShipmentTypeGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)));
			this.ShipmentTypeGroupBox.Controls.Add(this.JE_TransitModeDropEdit);
			this.ShipmentTypeGroupBox.Controls.Add(this.JE_CNTransportModeDropDownEdit);
			this.ShipmentTypeGroupBox.Controls.Add(this.FullValidationCheckBox);
			this.ShipmentTypeGroupBox.Controls.Add(this.ClearanceModeAndCheckBoxesDescriptionLabel);
			this.ShipmentTypeGroupBox.Controls.Add(this.JE_TaxInvolvedCheckBox);
			this.ShipmentTypeGroupBox.Controls.Add(this.JE_InspectionInvolvedCheckBox);
			this.ShipmentTypeGroupBox.Controls.Add(this.JE_LicenseInvolvedCheckBox);
			this.ShipmentTypeGroupBox.Controls.Add(this.JE_ClearanceModeDropEdit);
			this.ShipmentTypeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 368, true);
			this.ShipmentTypeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 360, true);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.JE_ClearanceModeDropEdit, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.JE_LicenseInvolvedCheckBox, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.JE_InspectionInvolvedCheckBox, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.JE_TaxInvolvedCheckBox, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.ClearanceModeAndCheckBoxesDescriptionLabel, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.FullValidationCheckBox, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.JE_CNTransportModeDropDownEdit, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.JE_ContainerModeBoundDropDownEdit, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.JE_TransportModeBoundDropDownEdit, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.JE_MessageTypeBoundDropDownEdit, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.JE_MessageSubTypeBoundDropDownEdit, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.JE_RS_NKServiceLevelBoundFindBox, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.JE_ApplicationCodeBoundDropEdit, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.JE_TransitModeDropEdit, 0);
			//
			// ImporterOrganisationControl
			//
			this.ImporterOrganisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 186, true);
			this.ImporterOrganisationControl.Visible = false;
			//
			// SupplierOrganisationControl
			//
			this.SupplierOrganisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			//
			// FinalDestinationFindBox
			//
			this.FinalDestinationFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(253, 64, true);
			this.FinalDestinationFindBox.ShowDescriptionBox = false;
			this.FinalDestinationFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			//
			// JE_DateOfArrivalBoundDateEdit2
			//
			this.JE_DateOfArrivalBoundDateEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(363, 64, true);
			//
			// JE_ExportDateBoundDateEdit2
			//
			this.JE_ExportDateBoundDateEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(363, 40, true);
			//
			// OriginFindBox
			//
			this.OriginFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(253, 40, true);
			this.OriginFindBox.ShowDescriptionBox = false;
			this.OriginFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			//
			// HouseBillParcelPostTextEdit
			//
			this.HouseBillParcelPostTextEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 20, true);
			//
			// IncoTermDropEdit
			//
			this.IncoTermDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 256, true);
			this.IncoTermDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(92, 20, true);
			//
			// RightTabControl
			//
			this.RightTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(725, 75, true);
			this.RightTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(266, 653, true);
			this.RightTabControl.TabIndex = 7;
			//
			// OrganisationsTabPage
			//
			this.OrganisationsTabPage.Controls.Add(this.OrganisationsButtomPanel);
			this.OrganisationsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 626, true);
			this.OrganisationsTabPage.Controls.SetChildIndex(this.OrganisationsTopPanel, 0);
			this.OrganisationsTabPage.Controls.SetChildIndex(this.OrganisationsButtomPanel, 0);
			//
			// OrganisationsTopPanel
			//
			this.OrganisationsTopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 206, true);
			//
			// ShippingOrAirLineOrganisationControl
			//
			this.ShippingOrAirLineOrganisationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 20, true);
			//
			// ForwarderOrganisationControl
			//
			this.ForwarderOrganisationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 20, true);
			//
			// OrdersTabPage
			//
			this.OrdersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 626, true);
			//
			// JE_RS_NKServiceLevelBoundFindBox
			//
			this.JE_RS_NKServiceLevelBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 134, true);
			this.JE_RS_NKServiceLevelBoundFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(132, 20, true);
			this.JE_RS_NKServiceLevelBoundFindBox.TabIndex = 6;
			//
			// DocsTabPage
			//
			this.DocsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 626, true);
			//
			// JE_ContainerCountCalcEdit
			//
			this.JE_ContainerCountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 232, true);
			//
			// JE_TotalNoOfPiecesBoundCalcEdit
			//
			this.JE_TotalNoOfPiecesBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 232, true);
			//
			// IncoTermExplainButton
			//
			this.IncoTermExplainButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(210, 256, true);
			//
			// OverrideValuesCheckBox
			//
			this.OverrideValuesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 17, true);
			//
			// VolumeCalcDropEdit
			//
			this.VolumeCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 208, true);
			this.VolumeCalcDropEdit.TabIndex = 16;
			//
			// ShipmentCustomFieldsPage
			//
			this.ShipmentCustomFieldsPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 626, true);
			//
			// shipmentCustomFieldsControl1
			//
			this.shipmentCustomFieldsControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 626, true);
			//
			// OrdersPanel
			//
			this.OrdersPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 626, true);
			//
			// NumbersTabPage
			//
			this.NumbersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 626, true);
			//
			// OrdersAttachUserControl
			//
			this.OrdersAttachUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 418, true);
			//
			// JE_ApplicationCodeBoundDropEdit
			//
			this.JE_ApplicationCodeBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 157, true);
			this.JE_ApplicationCodeBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(132, 20, true);
			this.JE_ApplicationCodeBoundDropEdit.TabIndex = 7;
			//
			// TransportDetailsGroupBox
			//
			this.TransportDetailsGroupBox.Controls.Add(this.JE_VesselInlandTextBox);
			this.TransportDetailsGroupBox.Controls.Add(this.JE_VoyageInlandTextBox);
			this.TransportDetailsGroupBox.Controls.Add(this.JE_TransportModeInlandDropEdit);
			this.TransportDetailsGroupBox.Controls.Add(this.ReceiptNumberTextBox);
			this.TransportDetailsGroupBox.Controls.Add(this.CNCountryOfLoadingTextBox);
			this.TransportDetailsGroupBox.Controls.Add(this.CNCountryOfArrivalTextBox);
			this.TransportDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 2, true);
			this.TransportDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(456, 186, true);
			this.TransportDetailsGroupBox.TabIndex = 3;
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.CNCountryOfArrivalTextBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.CNCountryOfLoadingTextBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.ReceiptNumberTextBox, 0);
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
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.JE_TransportModeInlandDropEdit, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.JE_VoyageInlandTextBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.JE_VesselInlandTextBox, 0);
			//
			// ExportDeclarationNumberBoundTextBox
			//
			this.ExportDeclarationNumberBoundTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ExportDeclarationNumberBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(159, 20, true);
			//
			// DeclarationDetailsGroupBox
			//
			this.DeclarationDetailsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
			this.DeclarationDetailsGroupBox.CaptionResourceString = Enterprise.Customs.CN.GUI.Res.GetData("bbd5d912-329b-4b91-be0b-80860c7024f3", "Entry Details");
			this.DeclarationDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(725, 2, true);
			this.DeclarationDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(266, 69, true);
			this.DeclarationDetailsGroupBox.TabIndex = 6;
			//
			// StatusTextBox
			//
			this.StatusTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
			this.StatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 40, true);
			this.StatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(159, 20, true);
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CN.Business.JobDeclaration);
			//
			// ManufacturerDocAddressControl
			//
			this.ManufacturerDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ManufacturerDocAddressControl, "ManufacturerDocumentaryAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.CN.Business.CNJobDocAddress)(((Enterprise.Customs.CN.Business.JobDeclaration)(null)).ManufacturerDocumentaryAddress)));
			this.ManufacturerDocAddressControl.BindToOrganisations = "Lookups.Manufacturers";
			this.ManufacturerDocAddressControl.CaptionResourceString = Enterprise.Customs.CN.GUI.Res.GetData("d0a111a1-0e4e-4825-81a9-63dd9917b1e9", "Manufacturer");
			this.ManufacturerDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 0, true);
			this.ManufacturerDocAddressControl.Name = "ManufacturerDocAddressControl";
			this.ManufacturerDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.ManufacturerDocAddressControl.TabIndex = 8;
			//
			// JE_CustomsOfficeCodeFindBox
			//
			this.JE_CustomsOfficeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JE_CustomsOfficeCodeFindBox, "JE_CustomsOffice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.JobDeclaration)(null)).JE_CustomsOffice)));
			this.JE_CustomsOfficeCodeFindBox.CaptionResourceString = Enterprise.Customs.CN.GUI.Res.GetData("e8ffb6d0-f6c2-4c9e-8646-7ecfa70d2882", "Customs Office");
			this.JE_CustomsOfficeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(141, 21, true);
			this.JE_CustomsOfficeCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
			this.JE_CustomsOfficeCodeFindBox.Name = "JE_CustomsOfficeCodeFindBox";
			this.JE_CustomsOfficeCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.JE_CustomsOfficeCodeFindBox.ParentType = null;
			this.JE_CustomsOfficeCodeFindBox.PreBoundMaxLength = 4;
			this.JE_CustomsOfficeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(307, 20, true);
			this.JE_CustomsOfficeCodeFindBox.TabIndex = 0;
			//
			// JE_RN_NKCountryOfTradeCodeFindBox
			//
			this.JE_RN_NKCountryOfTradeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JE_RN_NKCountryOfTradeCodeFindBox, "JE_RN_NKCountryOfTrade");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.JobDeclaration)(null)).JE_RN_NKCountryOfTrade)));
			this.JE_RN_NKCountryOfTradeCodeFindBox.CaptionResourceString = Enterprise.Customs.CN.GUI.Res.GetData("559b8fa8-ef7a-4e7d-b078-5f9b313fc687", "Country of Trade");
			this.JE_RN_NKCountryOfTradeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(336, 256, true);
			this.JE_RN_NKCountryOfTradeCodeFindBox.Name = "JE_RN_NKCountryOfTradeCodeFindBox";
			this.JE_RN_NKCountryOfTradeCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.JE_RN_NKCountryOfTradeCodeFindBox.ParentType = null;
			this.JE_RN_NKCountryOfTradeCodeFindBox.PreBoundMaxLength = 2;
			this.JE_RN_NKCountryOfTradeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
			this.JE_RN_NKCountryOfTradeCodeFindBox.TabIndex = 26;
			//
			// JE_CNTransportModeDropDownEdit
			//
			this.JE_CNTransportModeDropDownEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JE_CNTransportModeDropDownEdit, "JE_CNTransportMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CN.Business.JobDeclaration)(null)).JE_CNTransportMode)));
			this.JE_CNTransportModeDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 88, true);
			this.JE_CNTransportModeDropDownEdit.Name = "JE_CNTransportModeDropDownEdit";
			this.JE_CNTransportModeDropDownEdit.PreBoundMaxLength = 1;
			this.JE_CNTransportModeDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(132, 20, true);
			this.JE_CNTransportModeDropDownEdit.TabIndex = 4;
			//
			// JE_CIQOfficeOfEntryExitCodeFindBox
			//
			this.JE_CIQOfficeOfEntryExitCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JE_CIQOfficeOfEntryExitCodeFindBox, "JE_CIQOfficeOfEntryExit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.JobDeclaration)(null)).JE_CIQOfficeOfEntryExit)));
			this.JE_CIQOfficeOfEntryExitCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(141, 93, true);
			this.JE_CIQOfficeOfEntryExitCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
			this.JE_CIQOfficeOfEntryExitCodeFindBox.Name = "JE_CIQOfficeOfEntryExitCodeFindBox";
			this.JE_CIQOfficeOfEntryExitCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.JE_CIQOfficeOfEntryExitCodeFindBox.ParentType = null;
			this.JE_CIQOfficeOfEntryExitCodeFindBox.PreBoundMaxLength = 6;
			this.JE_CIQOfficeOfEntryExitCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(307, 20, true);
			this.JE_CIQOfficeOfEntryExitCodeFindBox.TabIndex = 3;
			//
			// JE_CNPortOfOriginCodeFindBox
			//
			this.JE_CNPortOfOriginCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JE_CNPortOfOriginCodeFindBox, "JE_CNPortOfOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.JobDeclaration)(null)).JE_CNPortOfOrigin)));
			this.JE_CNPortOfOriginCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 40, true);
			this.JE_CNPortOfOriginCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
			this.JE_CNPortOfOriginCodeFindBox.Name = "JE_CNPortOfOriginCodeFindBox";
			this.JE_CNPortOfOriginCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.JE_CNPortOfOriginCodeFindBox.ParentType = null;
			this.JE_CNPortOfOriginCodeFindBox.PreBoundMaxLength = 5;
			this.JE_CNPortOfOriginCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(137, 20, true);
			this.JE_CNPortOfOriginCodeFindBox.TabIndex = 2;
			//
			// JE_CNPortOfDestinationCodeFindBox
			//
			this.JE_CNPortOfDestinationCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JE_CNPortOfDestinationCodeFindBox, "JE_CNPortOfDestination");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.JobDeclaration)(null)).JE_CNPortOfDestination)));
			this.JE_CNPortOfDestinationCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 64, true);
			this.JE_CNPortOfDestinationCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
			this.JE_CNPortOfDestinationCodeFindBox.Name = "JE_CNPortOfDestinationCodeFindBox";
			this.JE_CNPortOfDestinationCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.JE_CNPortOfDestinationCodeFindBox.ParentType = null;
			this.JE_CNPortOfDestinationCodeFindBox.PreBoundMaxLength = 5;
			this.JE_CNPortOfDestinationCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(137, 20, true);
			this.JE_CNPortOfDestinationCodeFindBox.TabIndex = 6;
			//
			// JE_OfficeOfEntryExitCodeFindBox
			//
			this.JE_OfficeOfEntryExitCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JE_OfficeOfEntryExitCodeFindBox, "JE_OfficeOfEntryExit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.JobDeclaration)(null)).JE_OfficeOfEntryExit)));
			this.JE_OfficeOfEntryExitCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(141, 45, true);
			this.JE_OfficeOfEntryExitCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
			this.JE_OfficeOfEntryExitCodeFindBox.Name = "JE_OfficeOfEntryExitCodeFindBox";
			this.JE_OfficeOfEntryExitCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.JE_OfficeOfEntryExitCodeFindBox.ParentType = null;
			this.JE_OfficeOfEntryExitCodeFindBox.PreBoundMaxLength = 4;
			this.JE_OfficeOfEntryExitCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(307, 20, true);
			this.JE_OfficeOfEntryExitCodeFindBox.TabIndex = 1;
			//
			// ReceiptNumberTextBox
			//
			this.ReceiptNumberTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.ReceiptNumberTextBox, "JE_VesselName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.JobDeclaration)(null)).JE_VesselName)));
			this.ReceiptNumberTextBox.CaptionResourceString = Enterprise.Customs.CN.GUI.Res.GetData("38DF32B1-9235-4389-A2F6-239EE0F44163", "Car/Receipt No.", "Carriage Number or Transit Receipt Number");
			this.ReceiptNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 40, true);
			this.ReceiptNumberTextBox.Name = "ReceiptNumberTextBox";
			this.ReceiptNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(328, 20, true);
			this.ReceiptNumberTextBox.TabIndex = 5;
			//
			// JE_CNLastPortBeforeEntryCodeFindBox
			//
			this.JE_CNLastPortBeforeEntryCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JE_CNLastPortBeforeEntryCodeFindBox, "JE_CNLastPortBeforeEntry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.JobDeclaration)(null)).JE_CNLastPortBeforeEntry)));
			this.JE_CNLastPortBeforeEntryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 88, true);
			this.JE_CNLastPortBeforeEntryCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
			this.JE_CNLastPortBeforeEntryCodeFindBox.Name = "JE_CNLastPortBeforeEntryCodeFindBox";
			this.JE_CNLastPortBeforeEntryCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.JE_CNLastPortBeforeEntryCodeFindBox.ParentType = null;
			this.JE_CNLastPortBeforeEntryCodeFindBox.PreBoundMaxLength = 5;
			this.JE_CNLastPortBeforeEntryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(137, 20, true);
			this.JE_CNLastPortBeforeEntryCodeFindBox.TabIndex = 10;
			//
			// PortOfStopoverCodeFindBox
			//
			this.PortOfStopoverCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PortOfStopoverCodeFindBox, "JE_LastPortBeforeEntry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.JobDeclaration)(null)).JE_LastPortBeforeEntry)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PortOfStopoverCodeFindBox, false);
			this.PortOfStopoverCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(253, 88, true);
			this.PortOfStopoverCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			this.PortOfStopoverCodeFindBox.Name = "PortOfStopoverCodeFindBox";
			this.PortOfStopoverCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.PortOfStopoverCodeFindBox.ParentType = null;
			this.PortOfStopoverCodeFindBox.PreBoundMaxLength = 5;
			this.PortOfStopoverCodeFindBox.ShowDescriptionBox = false;
			this.PortOfStopoverCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			this.PortOfStopoverCodeFindBox.TabIndex = 11;
			//
			// CNCountryOfLoadingTextBox
			//
			this.BindingSource.SetBindingMember(this.CNCountryOfLoadingTextBox, "CNCountryOfLoading");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.JobDeclaration)(null)).CNCountryOfLoading)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.CNCountryOfLoadingTextBox, false);
			this.CNCountryOfLoadingTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(263, 88, true);
			this.CNCountryOfLoadingTextBox.Name = "CNCountryOfLoadingTextBox";
			this.CNCountryOfLoadingTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(31, 20, true);
			this.CNCountryOfLoadingTextBox.TabIndex = 12;
			//
			// CNCountryOfArrivalTextBox
			//
			this.BindingSource.SetBindingMember(this.CNCountryOfArrivalTextBox, "CNCountryOfArrival");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.JobDeclaration)(null)).CNCountryOfArrival)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.CNCountryOfArrivalTextBox, false);
			this.CNCountryOfArrivalTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(263, 112, true);
			this.CNCountryOfArrivalTextBox.Name = "CNCountryOfArrivalTextBox";
			this.CNCountryOfArrivalTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(31, 20, true);
			this.CNCountryOfArrivalTextBox.TabIndex = 15;
			//
			// LocationOfGoodsTextBox
			//
			this.BindingSource.SetBindingMember(this.LocationOfGoodsTextBox, "JE_LocationOfGoods");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.JobDeclaration)(null)).JE_LocationOfGoods)));
			this.LocationOfGoodsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 160, true);
			this.LocationOfGoodsTextBox.Name = "LocationOfGoodsTextBox";
			this.LocationOfGoodsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(336, 20, true);
			this.LocationOfGoodsTextBox.TabIndex = 14;
			//
			// CIQDetailsGroupBox
			//
			this.CIQDetailsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)));
			this.CIQDetailsGroupBox.CaptionResourceString = Enterprise.Customs.CN.GUI.Res.GetData("275d19d6-184e-45af-b9a1-9d532a04452e", "Declaration Details");
			this.CIQDetailsGroupBox.Controls.Add(this.OfficeOfDestinationCodeFindBox);
			this.CIQDetailsGroupBox.Controls.Add(this.JE_CustomsOfficeCodeFindBox);
			this.CIQDetailsGroupBox.Controls.Add(this.JE_OfficeOfEntryExitCodeFindBox);
			this.CIQDetailsGroupBox.Controls.Add(this.JE_CIQOfficeOfEntryExitCodeFindBox);
			this.CIQDetailsGroupBox.Controls.Add(this.JE_DateOfUnloadCompleteDateEdit);
			this.CIQDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 507, true);
			this.CIQDetailsGroupBox.Name = "CIQDetailsGroupBox";
			this.CIQDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(456, 221, true);
			this.CIQDetailsGroupBox.TabIndex = 6;
			this.CIQDetailsGroupBox.TabStop = false;
			//
			// OfficeOfDestinationCodeFindBox
			//
			this.OfficeOfDestinationCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OfficeOfDestinationCodeFindBox, "OfficeOfDestination");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.JobDeclaration)(null)).OfficeOfDestination)));
			this.OfficeOfDestinationCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(141, 69, true);
			this.OfficeOfDestinationCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
			this.OfficeOfDestinationCodeFindBox.Name = "OfficeOfDestinationCodeFindBox";
			this.OfficeOfDestinationCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.OfficeOfDestinationCodeFindBox.ParentType = null;
			this.OfficeOfDestinationCodeFindBox.PreBoundMaxLength = 4;
			this.OfficeOfDestinationCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(307, 20, true);
			this.OfficeOfDestinationCodeFindBox.TabIndex = 2;
			//
			// JE_DateOfUnloadCompleteDateEdit
			//
			this.JE_DateOfUnloadCompleteDateEdit.AllowDrop = true;
			this.JE_DateOfUnloadCompleteDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.JE_DateOfUnloadCompleteDateEdit, "JE_DateOfUnloadComplete");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CN.Business.JobDeclaration)(null)).JE_DateOfUnloadComplete)));
			this.JE_DateOfUnloadCompleteDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(141, 117, true);
			this.JE_DateOfUnloadCompleteDateEdit.Name = "JE_DateOfUnloadCompleteDateEdit";
			this.JE_DateOfUnloadCompleteDateEdit.TabIndex = 4;
			//
			// JE_MarksAndNumbersLongTextBox
			//
			this.JE_MarksAndNumbersLongTextBox.AllowDrop = true;
			this.JE_MarksAndNumbersLongTextBox.CaptionResourceString = Enterprise.Customs.CN.GUI.Res.GetData("7847977b-1b87-4bd6-bf3b-a644071ab6fb", "Marks & Numbers");
			this.JE_MarksAndNumbersLongTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			this.JE_MarksAndNumbersLongTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 183, true);
			this.JE_MarksAndNumbersLongTextBox.Name = "JE_MarksAndNumbersLongTextBox";
			this.JE_MarksAndNumbersLongTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(336, 20, true);
			this.JE_MarksAndNumbersLongTextBox.TabIndex = 15;
			//
			// BuyerDocAddressControl
			//
			this.BuyerDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BuyerDocAddressControl, "BuyerDocAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.CN.Business.CNJobDocAddress)(((Enterprise.Customs.CN.Business.JobDeclaration)(null)).BuyerDocAddress)));
			this.BuyerDocAddressControl.BindToOrganisations = "Lookups.Buyers";
			this.BuyerDocAddressControl.CaptionResourceString = Enterprise.Customs.CN.GUI.Res.GetData("d41efa2a-2894-4206-8d72-617d8b75885e", "Buyer");
			this.BuyerDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 183, true);
			this.BuyerDocAddressControl.Name = "BuyerDocAddressControl";
			this.BuyerDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.BuyerDocAddressControl.TabIndex = 10;
			//
			// ImporterDocAddressControl
			//
			this.ImporterDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ImporterDocAddressControl, "ImporterDocumentaryAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.CN.Business.CNJobDocAddress)(((Enterprise.Customs.CN.Business.JobDeclaration)(null)).ImporterDocumentaryAddress)));
			this.ImporterDocAddressControl.BindToOrganisations = "Lookups.ImportersList";
			this.ImporterDocAddressControl.CaptionResourceString = Enterprise.Customs.CN.GUI.Res.GetData("b22afb8a-b189-467e-ab33-5d3b17f9500b", "Importer");
			this.ImporterDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 183, true);
			this.ImporterDocAddressControl.Name = "ImporterDocAddressControl";
			this.ImporterDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.ImporterDocAddressControl.TabIndex = 1;
			//
			// SupplierDocAddressControl
			//
			this.SupplierDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SupplierDocAddressControl, "SupplierDocumentaryAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.CN.Business.CNJobDocAddress)(((Enterprise.Customs.CN.Business.JobDeclaration)(null)).SupplierDocumentaryAddress)));
			this.SupplierDocAddressControl.BindToOrganisations = "Lookups.SuppliersList";
			this.SupplierDocAddressControl.CaptionResourceString = Enterprise.Customs.CN.GUI.Res.GetData("1692b82d-8bda-4f06-b021-b613b0020bef", "Supplier");
			this.SupplierDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SupplierDocAddressControl.Name = "SupplierDocAddressControl";
			this.SupplierDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.SupplierDocAddressControl.TabIndex = 0;
			//
			// OrganisationsButtomPanel
			//
			this.OrganisationsButtomPanel.Controls.Add(this.BuyerDocAddressControl);
			this.OrganisationsButtomPanel.Controls.Add(this.ManufacturerDocAddressControl);
			this.OrganisationsButtomPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.OrganisationsButtomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 206, true);
			this.OrganisationsButtomPanel.Name = "OrganisationsButtomPanel";
			this.OrganisationsButtomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 366, true);
			this.OrganisationsButtomPanel.TabIndex = 1;
			//
			// JE_ClearanceModeDropEdit
			//
			this.JE_ClearanceModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JE_ClearanceModeDropEdit, "JE_ClearanceMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CN.Business.JobDeclaration)(null)).JE_ClearanceMode)));
			this.JE_ClearanceModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 203, true);
			this.JE_ClearanceModeDropEdit.Name = "JE_ClearanceModeDropEdit";
			this.JE_ClearanceModeDropEdit.PreBoundMaxLength = 3;
			this.JE_ClearanceModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(132, 20, true);
			this.JE_ClearanceModeDropEdit.TabIndex = 9;
			//
			// JE_LicenseInvolvedCheckBox
			//
			this.JE_LicenseInvolvedCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.JE_LicenseInvolvedCheckBox, "JE_LicenseInvolved");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CN.Business.JobDeclaration)(null)).JE_LicenseInvolved)));
			this.JE_LicenseInvolvedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 227, true);
			this.JE_LicenseInvolvedCheckBox.Name = "JE_LicenseInvolvedCheckBox";
			this.JE_LicenseInvolvedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 17, true);
			this.JE_LicenseInvolvedCheckBox.TabIndex = 10;
			this.JE_LicenseInvolvedCheckBox.UseVisualStyleBackColor = true;
			//
			// JE_InspectionInvolvedCheckBox
			//
			this.JE_InspectionInvolvedCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.JE_InspectionInvolvedCheckBox, "JE_InspectionInvolved");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CN.Business.JobDeclaration)(null)).JE_InspectionInvolved)));
			this.JE_InspectionInvolvedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 247, true);
			this.JE_InspectionInvolvedCheckBox.Name = "JE_InspectionInvolvedCheckBox";
			this.JE_InspectionInvolvedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 17, true);
			this.JE_InspectionInvolvedCheckBox.TabIndex = 11;
			this.JE_InspectionInvolvedCheckBox.UseVisualStyleBackColor = true;
			//
			// JE_TaxInvolvedCheckBox
			//
			this.JE_TaxInvolvedCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.JE_TaxInvolvedCheckBox, "JE_TaxInvolved");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CN.Business.JobDeclaration)(null)).JE_TaxInvolved)));
			this.JE_TaxInvolvedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 267, true);
			this.JE_TaxInvolvedCheckBox.Name = "JE_TaxInvolvedCheckBox";
			this.JE_TaxInvolvedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(84, 17, true);
			this.JE_TaxInvolvedCheckBox.TabIndex = 0;
			this.JE_TaxInvolvedCheckBox.UseVisualStyleBackColor = true;
			//
			// ClearanceModeAndCheckBoxesDescriptionLabel
			//
			this.ClearanceModeAndCheckBoxesDescriptionLabel.AutoSize = true;
			this.ClearanceModeAndCheckBoxesDescriptionLabel.BackColor = System.Drawing.Color.LightSalmon;
			this.BindingSource.SetBindingMember(this.ClearanceModeAndCheckBoxesDescriptionLabel, "ClearanceModeDetailedDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.JobDeclaration)(null)).ClearanceModeDetailedDescription)));
			this.ClearanceModeAndCheckBoxesDescriptionLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ClearanceModeAndCheckBoxesDescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(23, 317, true);
			this.ClearanceModeAndCheckBoxesDescriptionLabel.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(210, 0, true);
			this.ClearanceModeAndCheckBoxesDescriptionLabel.Name = "ClearanceModeAndCheckBoxesDescriptionLabel";
			this.ClearanceModeAndCheckBoxesDescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.ClearanceModeAndCheckBoxesDescriptionLabel.TabIndex = 0;
			this.ClearanceModeAndCheckBoxesDescriptionLabel.UseMnemonic = false;
			//
			// FullValidationCheckBox
			//
			this.FullValidationCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.FullValidationCheckBox, "FullValidation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CN.Business.JobDeclaration)(null)).FullValidation)));
			this.FullValidationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 287, true);
			this.FullValidationCheckBox.Name = "FullValidationCheckBox";
			this.FullValidationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(91, 17, true);
			this.FullValidationCheckBox.TabIndex = 0;
			this.FullValidationCheckBox.UseVisualStyleBackColor = true;
			//
			// JE_TransitModeDropEdit
			//
			this.JE_TransitModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JE_TransitModeDropEdit, "JE_TransitMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CN.Business.JobDeclaration)(null)).JE_TransitMode)));
			this.JE_TransitModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 180, true);
			this.JE_TransitModeDropEdit.Name = "JE_TransitModeDropEdit";
			this.JE_TransitModeDropEdit.PreBoundMaxLength = 1;
			this.JE_TransitModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(132, 20, true);
			this.JE_TransitModeDropEdit.TabIndex = 8;
			//
			// JE_TransportModeInlandDropEdit
			//
			this.JE_TransportModeInlandDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JE_TransportModeInlandDropEdit, "JE_TransportModeInland");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CN.Business.JobDeclaration)(null)).JE_TransportModeInland)));
			this.JE_TransportModeInlandDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 136, true);
			this.JE_TransportModeInlandDropEdit.Name = "JE_TransportModeInlandDropEdit";
			this.JE_TransportModeInlandDropEdit.PreBoundMaxLength = 3;
			this.JE_TransportModeInlandDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(164, 20, true);
			this.JE_TransportModeInlandDropEdit.TabIndex = 17;
			//
			// JE_VoyageInlandTextBox
			//
			this.BindingSource.SetBindingMember(this.JE_VoyageInlandTextBox, "JE_VoyageInland");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.JobDeclaration)(null)).JE_VoyageInland)));
			this.JE_VoyageInlandTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(300, 160, true);
			this.JE_VoyageInlandTextBox.Name = "JE_VoyageInlandTextBox";
			this.JE_VoyageInlandTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(148, 20, true);
			this.JE_VoyageInlandTextBox.TabIndex = 19;
			//
			// JE_VesselInlandTextBox
			//
			this.BindingSource.SetBindingMember(this.JE_VesselInlandTextBox, "JE_VesselInland");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.JobDeclaration)(null)).JE_VesselInland)));
			this.JE_VesselInlandTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 160, true);
			this.JE_VesselInlandTextBox.Name = "JE_VesselInlandTextBox";
			this.JE_VesselInlandTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(164, 20, true);
			this.JE_VesselInlandTextBox.TabIndex = 18;
			//
			// CNJobDeclarationUserControl
			//
			this.Controls.Add(this.SupplierDocAddressControl);
			this.Controls.Add(this.ImporterDocAddressControl);
			this.Controls.Add(this.CIQDetailsGroupBox);
			this.Name = "CNJobDeclarationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1003, 731, true);
			this.Controls.SetChildIndex(this.RequiresMergeLabel, 0);
			this.Controls.SetChildIndex(this.CIQDetailsGroupBox, 0);
			this.Controls.SetChildIndex(this.SupplierOrganisationControl, 0);
			this.Controls.SetChildIndex(this.ShipmentTypeGroupBox, 0);
			this.Controls.SetChildIndex(this.ImporterOrganisationControl, 0);
			this.Controls.SetChildIndex(this.ShipmentDetailsGroupBox, 0);
			this.Controls.SetChildIndex(this.TransportDetailsGroupBox, 0);
			this.Controls.SetChildIndex(this.RightTabControl, 0);
			this.Controls.SetChildIndex(this.DeclarationDetailsGroupBox, 0);
			this.Controls.SetChildIndex(this.ImporterDocAddressControl, 0);
			this.Controls.SetChildIndex(this.SupplierDocAddressControl, 0);
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
			this.ManufacturerDocAddressControl.ResumeLayout(true);
			this.ManufacturerDocAddressControl.PerformLayout();
			this.JE_CustomsOfficeCodeFindBox.ResumeLayout(true);
			this.JE_CustomsOfficeCodeFindBox.PerformLayout();
			this.JE_RN_NKCountryOfTradeCodeFindBox.ResumeLayout(true);
			this.JE_RN_NKCountryOfTradeCodeFindBox.PerformLayout();
			this.JE_CNTransportModeDropDownEdit.ResumeLayout(true);
			this.JE_CNTransportModeDropDownEdit.PerformLayout();
			this.JE_CIQOfficeOfEntryExitCodeFindBox.ResumeLayout(true);
			this.JE_CIQOfficeOfEntryExitCodeFindBox.PerformLayout();
			this.JE_CNPortOfOriginCodeFindBox.ResumeLayout(true);
			this.JE_CNPortOfOriginCodeFindBox.PerformLayout();
			this.JE_CNPortOfDestinationCodeFindBox.ResumeLayout(true);
			this.JE_CNPortOfDestinationCodeFindBox.PerformLayout();
			this.JE_OfficeOfEntryExitCodeFindBox.ResumeLayout(true);
			this.JE_OfficeOfEntryExitCodeFindBox.PerformLayout();
			this.JE_CNLastPortBeforeEntryCodeFindBox.ResumeLayout(true);
			this.JE_CNLastPortBeforeEntryCodeFindBox.PerformLayout();
			this.PortOfStopoverCodeFindBox.ResumeLayout(true);
			this.PortOfStopoverCodeFindBox.PerformLayout();
			this.CIQDetailsGroupBox.ResumeLayout(false);
			this.CIQDetailsGroupBox.PerformLayout();
			this.OfficeOfDestinationCodeFindBox.ResumeLayout(true);
			this.OfficeOfDestinationCodeFindBox.PerformLayout();
			this.JE_DateOfUnloadCompleteDateEdit.ResumeLayout(true);
			this.JE_DateOfUnloadCompleteDateEdit.PerformLayout();
			this.JE_MarksAndNumbersLongTextBox.ResumeLayout(true);
			this.JE_MarksAndNumbersLongTextBox.PerformLayout();
			this.BuyerDocAddressControl.ResumeLayout(true);
			this.BuyerDocAddressControl.PerformLayout();
			this.ImporterDocAddressControl.ResumeLayout(true);
			this.ImporterDocAddressControl.PerformLayout();
			this.SupplierDocAddressControl.ResumeLayout(true);
			this.SupplierDocAddressControl.PerformLayout();
			this.OrganisationsButtomPanel.ResumeLayout(false);
			this.OrganisationsButtomPanel.PerformLayout();
			this.JE_ClearanceModeDropEdit.ResumeLayout(true);
			this.JE_ClearanceModeDropEdit.PerformLayout();
			this.JE_TransitModeDropEdit.ResumeLayout(true);
			this.JE_TransitModeDropEdit.PerformLayout();
			this.JE_TransportModeInlandDropEdit.ResumeLayout(true);
			this.JE_TransportModeInlandDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal ZDropEdit JE_CNTransportModeDropDownEdit;
		internal ZCodeFindBox JE_CNPortOfOriginCodeFindBox;
		internal ZCodeFindBox JE_CNPortOfDestinationCodeFindBox;
		internal ZCodeFindBox JE_CustomsOfficeCodeFindBox;
		internal ZCodeFindBox JE_RN_NKCountryOfTradeCodeFindBox;
		internal ZCodeFindBox JE_OfficeOfEntryExitCodeFindBox;
		internal Enterprise.ZArchitecture.ZTextBox ReceiptNumberTextBox;
		public ZCodeFindBox PortOfStopoverCodeFindBox;
		internal ZCodeFindBox JE_CNLastPortBeforeEntryCodeFindBox;
		internal ZArchitecture.ZTextBox LocationOfGoodsTextBox;
		internal ZGroupBox CIQDetailsGroupBox;
		internal ZDateEdit JE_DateOfUnloadCompleteDateEdit;
		internal ZCodeFindBox JE_CIQOfficeOfEntryExitCodeFindBox;
		internal ZArchitecture.ZTextBox CNCountryOfLoadingTextBox;
		internal ZArchitecture.ZTextBox CNCountryOfArrivalTextBox;
		internal Customs.GUI.LongTextControl JE_MarksAndNumbersLongTextBox;
		internal CNJobDocAddressControl BuyerDocAddressControl;
		internal CNJobDocAddressControl ImporterDocAddressControl;
		internal CNJobDocAddressControl SupplierDocAddressControl;
		internal CNJobDocAddressControl ManufacturerDocAddressControl;
		internal ZCheckBox FullValidationCheckBox;
		ZPanel OrganisationsButtomPanel;
		private ZDropEdit JE_ClearanceModeDropEdit;
		private ZCheckBox JE_LicenseInvolvedCheckBox;
		private ZCheckBox JE_TaxInvolvedCheckBox;
		private ZCheckBox JE_InspectionInvolvedCheckBox;
		private ZLabel ClearanceModeAndCheckBoxesDescriptionLabel;
		private ZDropEdit JE_TransitModeDropEdit;
		internal ZArchitecture.ZTextBox JE_VesselInlandTextBox;
		internal ZArchitecture.ZTextBox JE_VoyageInlandTextBox;
		private ZDropEdit JE_TransportModeInlandDropEdit;
		internal ZCodeFindBox OfficeOfDestinationCodeFindBox;
	}
}
