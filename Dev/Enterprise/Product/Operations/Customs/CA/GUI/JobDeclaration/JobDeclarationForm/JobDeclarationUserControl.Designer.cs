using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI
{
	public partial class CAJobDeclarationUserControl
	{
		#region Component Designer generated code

		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.VendorAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.ExportPortPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.PlaceOfReportCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.PortOfExitCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.PermitsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PermitsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ExportExtraDetailPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.CA_RX_DeclaredCurrGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.ReasonForExportDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CA_TransportDocumentNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.B3EntryStatusDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LowValueShipmentLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ImportPortPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.PortOfClearanceCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.PortOfUnladingCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.TransactionNumberPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.CheckDigitTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SequentialNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SecurityCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FormattedTransactionNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ImportExtraDetailsZPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ExceptionDescriptionLabel = new Enterprise.ZArchitecture.ZLabel();
			this.SubLocationNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ReleaseDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.EstReleaseDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.WarehouseReleaseDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.SubLocationCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ExamLocationCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ExamLocationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CarrierCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CarrierCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.EDIReleaseOptionsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AssesmentOptionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ServiceOptionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.EnableB3CheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.EnableACROSSCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.PriorityIndDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.NetWeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.VendorAndOriginatorPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ImporterOfRecordDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.CommercialInvoiceOriginatorDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.ImporterDocumentaryAddress = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.ScheduledB3DateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.SupplierDocumentAddress = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.ImporterOrgAddressControl = new Enterprise.MasterFiles.GUI.ZOrgAddressControl();
			this.SupplierOrgAddressControl = new Enterprise.MasterFiles.GUI.ZOrgAddressControl();
			this.groupBoxManualSubmissionInfo = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.B3CSubmittedOffice = new Enterprise.ZArchitecture.ZTextBox();
			this.RELSubmittedOffice = new Enterprise.ZArchitecture.ZTextBox();
			this.B3CSubmittedDate = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.RELSubmittedDate = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.CargoControlNumberNoBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DefaultHWBButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.PortOfFirstArrivalCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.DateOfFirstArrivalDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.CargoControlNumberPrefixNoBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CargoControlNumberSuffixNoBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DeliveryInstructionsLongTextControl = new Enterprise.Customs.GUI.LongTextControl();
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
			this.DepotAddressControl.SuspendLayout();
			this.BondedWarehouseDocAddressControl.SuspendLayout();
			this.ContainerYardAddressControl.SuspendLayout();
			this.NumbersTabPage.SuspendLayout();
			this.JE_ApplicationCodeBoundDropEdit.SuspendLayout();
			this.ExternalBrokerGuidFindBox.SuspendLayout();
			this.ControllingCustomerGuidFindBox.SuspendLayout();
			this.ControllingAgentGuidFindBox.SuspendLayout();
			this.TransportDetailsGroupBox.SuspendLayout();
			this.DeclarationDetailsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.VendorAddressControl.SuspendLayout();
			this.ExportPortPanel.SuspendLayout();
			this.PlaceOfReportCodeFindBox.SuspendLayout();
			this.PortOfExitCodeFindBox.SuspendLayout();
			this.PermitsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PermitsGrid)).BeginInit();
			this.PermitsGrid.SuspendLayout();
			this.ExportExtraDetailPanel.SuspendLayout();
			this.CA_RX_DeclaredCurrGuidFindBox.SuspendLayout();
			this.ReasonForExportDropEdit.SuspendLayout();
			this.ImportPortPanel.SuspendLayout();
			this.PortOfClearanceCodeFindBox.SuspendLayout();
			this.PortOfUnladingCodeFindBox.SuspendLayout();
			this.TransactionNumberPanel.SuspendLayout();
			this.ImportExtraDetailsZPanel.SuspendLayout();
			this.ReleaseDateDateEdit.SuspendLayout();
			this.EstReleaseDateDateEdit.SuspendLayout();
			this.WarehouseReleaseDateDateEdit.SuspendLayout();
			this.SubLocationCodeFindBox.SuspendLayout();
			this.ExamLocationCodeFindBox.SuspendLayout();
			this.CarrierCodeFindBox.SuspendLayout();
			this.EDIReleaseOptionsGroupBox.SuspendLayout();
			this.AssesmentOptionDropEdit.SuspendLayout();
			this.ServiceOptionDropEdit.SuspendLayout();
			this.PriorityIndDropEdit.SuspendLayout();
			this.NetWeightCalcDropEdit.SuspendLayout();
			this.VendorAndOriginatorPanel.SuspendLayout();
			this.ImporterOfRecordDocAddressControl.SuspendLayout();
			this.CommercialInvoiceOriginatorDocAddressControl.SuspendLayout();
			this.ImporterDocumentaryAddress.SuspendLayout();
			this.ScheduledB3DateEdit.SuspendLayout();
			this.SupplierDocumentAddress.SuspendLayout();
			this.ImporterOrgAddressControl.SuspendLayout();
			this.SupplierOrgAddressControl.SuspendLayout();
			this.groupBoxManualSubmissionInfo.SuspendLayout();
			this.B3CSubmittedDate.SuspendLayout();
			this.RELSubmittedDate.SuspendLayout();
			this.PortOfFirstArrivalCodeFindBox.SuspendLayout();
			this.DateOfFirstArrivalDateEdit.SuspendLayout();
			this.DeliveryInstructionsLongTextControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// JE_MessageTypeBoundDropDownEdit
			// 
			this.JE_MessageTypeBoundDropDownEdit.BindToList = "Lookups.EditableMessageTypeList";
			this.JE_MessageTypeBoundDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(69, 16, true);
			this.JE_MessageTypeBoundDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(176, 20, true);
			// 
			// JE_TransportModeBoundDropDownEdit
			// 
			this.JE_TransportModeBoundDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(69, 37, true);
			this.JE_TransportModeBoundDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(176, 20, true);
			this.JE_TransportModeBoundDropDownEdit.TabIndex = 3;
			// 
			// JE_ContainerModeBoundDropDownEdit
			// 
			this.JE_ContainerModeBoundDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(69, 58, true);
			this.JE_ContainerModeBoundDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(176, 20, true);
			this.JE_ContainerModeBoundDropDownEdit.TabIndex = 5;
			// 
			// JE_MessageSubTypeBoundDropDownEdit
			// 
			this.JE_MessageSubTypeBoundDropDownEdit.BindToList = "";
			this.JE_MessageSubTypeBoundDropDownEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("0b9f71e6-9ad6-4e46-b239-cf40b43a33cd", "Entry E Type");
			this.JE_MessageSubTypeBoundDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(69, 79, true);
			this.JE_MessageSubTypeBoundDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(176, 20, true);
			this.JE_MessageSubTypeBoundDropDownEdit.TabIndex = 8;
			// 
			// WeightzCalcDropEdit
			// 
			this.WeightzCalcDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|4a2e0994-d909-4f38-babe-a657e6134625", "Weight", "Gross Weight", "Total Gross Weight", "The total gross weight of the shipment");
			this.WeightzCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 104, true);
			this.WeightzCalcDropEdit.TabIndex = 10;
			// 
			// OwnersReferenceTextBox
			// 
			this.OwnersReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 10, true);
			this.OwnersReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(137, 20, true);
			this.OwnersReferenceTextBox.TabIndex = 2;
			// 
			// ScreeningStatusDropEdit
			// 
			this.ScreeningStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(313, 150, true);
			this.ScreeningStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(109, 20, true);
			this.ScreeningStatusDropEdit.TabIndex = 15;
			// 
			// ScreenButton
			// 
			this.ScreenButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(424, 150, true);
			this.ScreenButton.TabIndex = 16;
			// 
			// TotalNoOfPacksCalcDropEdit
			// 
			this.TotalNoOfPacksCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(313, 104, true);
			this.TotalNoOfPacksCalcDropEdit.TabIndex = 11;
			// 
			// GoodsDescriptionTextBox
			// 
			this.GoodsDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 82, true);
			this.GoodsDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 20, true);
			this.GoodsDescriptionTextBox.TabIndex = 7;
			// 
			// JE_MasterBillForAirBoundTextBox
			// 
			this.JE_MasterBillForAirBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 79, true);
			this.JE_MasterBillForAirBoundTextBox.TabIndex = 5;
			// 
			// FolioNumberTextBox
			// 
			this.FolioNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(250, 102, true);
			// 
			// VesselFindBox
			// 
			this.VesselFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(250, 102, true);
			this.VesselFindBox.ShowDescriptionBox = false;
			this.VesselFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(145, 20, true);
			this.VesselFindBox.CodeBox.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(145, 20, true);
			this.VesselFindBox.TabIndex = 9;
			// 
			// JE_MasterBillForSeaBoundTextBox
			// 
			this.JE_MasterBillForSeaBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 79, true);
			this.JE_MasterBillForSeaBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(295, 20, true);
			this.JE_MasterBillForSeaBoundTextBox.TabIndex = 6;
			// 
			// JE_VoyageFlightNoBoundTextBox
			// 
			this.JE_VoyageFlightNoBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 102, true);
			// 
			// JE_ExportDateBoundDateEdit
			// 
			this.JE_ExportDateBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(328, 124, true);
			this.JE_ExportDateBoundDateEdit.TabIndex = 11;
			// 
			// JE_DateOfArrivalBoundDateEdit
			// 
			this.JE_DateOfArrivalBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(328, 147, true);
			this.JE_DateOfArrivalBoundDateEdit.TabIndex = 13;
			// 
			// PortOfDischargeFindBox
			// 
			this.PortOfDischargeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 147, true);
			this.PortOfDischargeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(174, 20, true);
			this.PortOfDischargeFindBox.TabIndex = 12;
			// 
			// PortOfLoadingFindBox
			// 
			this.PortOfLoadingFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 124, true);
			this.PortOfLoadingFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(174, 20, true);
			// 
			// ShipmentDetailsGroupBox
			//
			this.ShipmentDetailsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.ShipmentDetailsGroupBox.Controls.Add(this.NetWeightCalcDropEdit);
			this.ShipmentDetailsGroupBox.Controls.Add(this.ExportExtraDetailPanel);
			this.ShipmentDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(253, 310, true);
			this.ShipmentDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(456, 290, true);
			this.ShipmentDetailsGroupBox.TabIndex = 8;
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.ScreeningStatusDropEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.ScreenButton, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.ExportExtraDetailPanel, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.NetWeightCalcDropEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.GoodsDescriptionTextBox, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.TotalNoOfPacksCalcDropEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.OwnersReferenceTextBox, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.JE_ContainerCountCalcEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.WeightzCalcDropEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.VolumeCalcDropEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.OriginFindBox, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.JE_ExportDateBoundDateEdit2, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.JE_DateOfArrivalBoundDateEdit2, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.FinalDestinationFindBox, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.IncoTermDropEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.JE_TotalNoOfPiecesBoundCalcEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.IncoTermExplainButton, 0);
			// 
			// ShipmentTypeGroupBox
			// 
			this.ShipmentTypeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 343, true);
			this.ShipmentTypeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 146, true);
			// 
			// ImporterOrganisationControl
			// 
			this.ImporterOrganisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 187, true);
			//
						// ImporterOrgAddressControl
			// 
			this.ImporterOrgAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ImporterOrgAddressControl, "JE_OA_ImporterAddress_ZAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Business.ZAddress)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).JE_OA_ImporterAddress_ZAddress)));
			this.ImporterOrgAddressControl.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("9B5B45A7-D360-485A-ABE3-4153164D2B33", "Consignee");
			this.ImporterOrgAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 167, true);
			this.ImporterOrgAddressControl.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 162, true);
			this.ImporterOrgAddressControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 162, true);
			this.ImporterOrgAddressControl.Name = "ImporterOrgAddressControl";
			this.ImporterOrgAddressControl.OnlyStopOnDebtor = false;
			this.ImporterOrgAddressControl.PopupCaption = "";
			this.ImporterOrgAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 162, true);
			this.ImporterOrgAddressControl.TabIndex = 1;

			// 
			// SupplierOrgAddressControl
			// 
			this.SupplierOrgAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SupplierOrgAddressControl, "JE_OA_SupplierAddress_ZAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Business.ZAddress)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).JE_OA_SupplierAddress_ZAddress)));
			this.SupplierOrgAddressControl.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("604E16EB-10BD-4D60-AF24-C011BEFAB748", "Exporter");
			this.SupplierOrgAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 4, true);
			this.SupplierOrgAddressControl.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 162, true);
			this.SupplierOrgAddressControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 162, true);
			this.SupplierOrgAddressControl.Name = "SupplierOrgAddressControl";
			this.SupplierOrgAddressControl.OnlyStopOnDebtor = false;
			this.SupplierOrgAddressControl.PopupCaption = "";
			this.SupplierOrgAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 162, true);
			this.SupplierOrgAddressControl.TabIndex = 0;
			// SupplierOrganisationControl
			// 
			this.SupplierOrganisationControl.Captions = new string[] {
        "Main Supplier "};
			this.SupplierOrganisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 4, true);
			// 
			// FinalDestinationFindBox
			// 
			this.FinalDestinationFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 59, true);
			this.FinalDestinationFindBox.TabIndex = 5;
			// 
			// JE_DateOfArrivalBoundDateEdit2
			// 
			this.JE_DateOfArrivalBoundDateEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(365, 59, true);
			this.JE_DateOfArrivalBoundDateEdit2.TabIndex = 6;
			// 
			// JE_ExportDateBoundDateEdit2
			// 
			this.JE_ExportDateBoundDateEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(365, 36, true);
			this.JE_ExportDateBoundDateEdit2.TabIndex = 4;
			// 
			// OriginFindBox
			// 
			this.OriginFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 36, true);
			// 
			// HouseBillParcelPostTextEdit
			// 
			this.HouseBillParcelPostTextEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 58, true);
			this.HouseBillParcelPostTextEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.HouseBillParcelPostTextEdit.TabIndex = 4;
			// 
			// IncoTermDropEdit
			// 
			this.IncoTermDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(365, 82, true);
			this.IncoTermDropEdit.ShowDescriptionBox = false;
			this.IncoTermDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.IncoTermDropEdit.TabIndex = 8;
			// 
			// RightTabControl
			// 
			this.RightTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(715, 81, true);
			this.RightTabControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(266, 480, true);
			this.RightTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(287, 546, true);
			this.RightTabControl.TabIndex = 9;
			// 
			// OrganisationsTabPage
			// 
			this.OrganisationsTabPage.Controls.Add(this.VendorAndOriginatorPanel);
			this.OrganisationsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(279, 519, true);
			this.OrganisationsTabPage.Controls.SetChildIndex(this.OrganisationsTopPanel, 0);
			this.OrganisationsTabPage.Controls.SetChildIndex(this.VendorAndOriginatorPanel, 0);
			// 
			// OrganisationsTopPanel
			// 
			this.OrganisationsTopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(279, 206, true);
			// 
			// OrdersTabPage
			// 
			this.OrdersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(279, 519, true);
			// 
			// JE_RS_NKServiceLevelBoundFindBox
			// 
			this.JE_RS_NKServiceLevelBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(69, 100, true);
			this.JE_RS_NKServiceLevelBoundFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(176, 20, true);
			// 
			// DocsTabPage
			// 
			this.DocsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(279, 519, true);
			// 
			// JE_ContainerCountCalcEdit
			// 
			this.JE_ContainerCountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 127, true);
			this.JE_ContainerCountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 20, true);
			this.JE_ContainerCountCalcEdit.TabIndex = 12;
			// 
			// JE_TotalNoOfPiecesBoundCalcEdit
			// 
			this.JE_TotalNoOfPiecesBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 127, true);
			this.JE_TotalNoOfPiecesBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.JE_TotalNoOfPiecesBoundCalcEdit.TabIndex = 12;
			// 
			// IncoTermExplainButton
			// 
			this.IncoTermExplainButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(424, 81, true);
			this.IncoTermExplainButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(26, 21, true);
			this.IncoTermExplainButton.TabIndex = 9;
			// 
			// OverrideValuesCheckBox
			// 
			this.OverrideValuesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, -2, true);
			this.OverrideValuesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 17, true);
			// 
			// VolumeCalcDropEdit
			// 
			this.VolumeCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(313, 128, true);
			this.VolumeCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 20, true);
			this.VolumeCalcDropEdit.TabIndex = 13;
			// 
			// ShipmentCustomFieldsPage
			// 
			this.ShipmentCustomFieldsPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(279, 519, true);
			// 
			// shipmentCustomFieldsControl1
			// 
			this.shipmentCustomFieldsControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(279, 519, true);
			// 
			// TransportDetailsGroupBox
			// 
			this.TransportDetailsGroupBox.Controls.Add(this.DeliveryInstructionsLongTextControl);
			this.TransportDetailsGroupBox.Controls.Add(this.DateOfFirstArrivalDateEdit);
			this.TransportDetailsGroupBox.Controls.Add(this.PortOfFirstArrivalCodeFindBox);
			this.TransportDetailsGroupBox.Controls.Add(this.CarrierCodeFindBox);
			this.TransportDetailsGroupBox.Controls.Add(this.CarrierCodeTextBox);
			this.TransportDetailsGroupBox.Controls.Add(this.ImportExtraDetailsZPanel);
			this.TransportDetailsGroupBox.Controls.Add(this.ExportPortPanel);
			this.TransportDetailsGroupBox.Controls.Add(this.CargoControlNumberSuffixNoBoundTextBox);
			this.TransportDetailsGroupBox.Controls.Add(this.CargoControlNumberPrefixNoBoundTextBox);
			this.TransportDetailsGroupBox.Controls.Add(this.ImportPortPanel);
			this.TransportDetailsGroupBox.Controls.Add(this.DefaultHWBButton);
			this.TransportDetailsGroupBox.Controls.Add(this.CargoControlNumberNoBoundTextBox);
			this.TransportDetailsGroupBox.Controls.Add(this.HouseBillParcelPostTextEdit);
			this.TransportDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(253, 71, true);
			this.TransportDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(456, 392, true);
			this.TransportDetailsGroupBox.TabIndex = 7;
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.HouseBillParcelPostTextEdit, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.CargoControlNumberNoBoundTextBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.DefaultHWBButton, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.ImportPortPanel, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.CargoControlNumberPrefixNoBoundTextBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.CargoControlNumberSuffixNoBoundTextBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.ExportPortPanel, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.ImportExtraDetailsZPanel, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.CarrierCodeTextBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.CarrierCodeFindBox, 0);
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
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.PortOfFirstArrivalCodeFindBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.DateOfFirstArrivalDateEdit, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.DeliveryInstructionsLongTextControl, 0);
			// 
			// ExportDeclarationNumberBoundTextBox
			// 
			this.ExportDeclarationNumberBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 16, true);
			// 
			// DeclarationDetailsGroupBox
			// 
			this.DeclarationDetailsGroupBox.Controls.Add(this.ScheduledB3DateEdit);
			this.DeclarationDetailsGroupBox.Controls.Add(this.TransactionNumberPanel);
			this.DeclarationDetailsGroupBox.Controls.Add(this.B3EntryStatusDescriptionTextBox);
			this.DeclarationDetailsGroupBox.Controls.Add(this.LowValueShipmentLabel);
			this.DeclarationDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(253, 8, true);
			this.DeclarationDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(745, 62, true);
			this.DeclarationDetailsGroupBox.TabIndex = 6;
			this.DeclarationDetailsGroupBox.Controls.SetChildIndex(this.LowValueShipmentLabel, 0);
			this.DeclarationDetailsGroupBox.Controls.SetChildIndex(this.B3EntryStatusDescriptionTextBox, 0);
			this.DeclarationDetailsGroupBox.Controls.SetChildIndex(this.TransactionNumberPanel, 0);
			this.DeclarationDetailsGroupBox.Controls.SetChildIndex(this.ScheduledB3DateEdit, 0);
			this.DeclarationDetailsGroupBox.Controls.SetChildIndex(this.ExportDeclarationNumberBoundTextBox, 0);
			this.DeclarationDetailsGroupBox.Controls.SetChildIndex(this.StatusTextBox, 0);
			// 
			// StatusTextBox
			// 
			this.StatusTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|f4f509d5-6346-4cf5-9a67-ab7e791adc6e", "Release Status");
			this.StatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(342, 37, true);
			this.StatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(388, 20, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.JobDeclaration);
			// 
			// VendorAddressControl
			// 
			this.VendorAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.VendorAddressControl, "VendorDocAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).VendorDocAddress)));
			this.VendorAddressControl.BindToOrganisations = "Lookups+Organizations";
			this.VendorAddressControl.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|12d50bf5-f97c-451b-ad77-8f198e117c08", "Vendor (Seller)");
			this.VendorAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox;
			this.VendorAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 22, true);
			this.VendorAddressControl.Name = "VendorAddressControl";
			this.VendorAddressControl.ReadOnly = false;
			this.VendorAddressControl.SingleLineNoGroupBoxPanelWidth = 320;
			this.VendorAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
			this.VendorAddressControl.TabIndex = 1;
			this.VendorAddressControl.ValidationJustForced = false;
			// 
			// ExportPortPanel
			// 
			this.ExportPortPanel.Controls.Add(this.PlaceOfReportCodeFindBox);
			this.ExportPortPanel.Controls.Add(this.PortOfExitCodeFindBox);
			this.ExportPortPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 193, true);
			this.ExportPortPanel.Name = "ExportPortPanel";
			this.ExportPortPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(440, 50, true);
			this.ExportPortPanel.TabIndex = 16;
			// 
			// PlaceOfReportCodeFindBox
			// 
			this.PlaceOfReportCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PlaceOfReportCodeFindBox, "CA_PlaceOfReport");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).CA_PlaceOfReport)));
			this.PlaceOfReportCodeFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|262f9b71-b908-4d51-b916-1e5dcc50a4ee", "Place Of Report", "Customs Place Of Report", "");
			this.PlaceOfReportCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 27, true);
			this.PlaceOfReportCodeFindBox.Name = "PlaceOfReportCodeFindBox";
			this.PlaceOfReportCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(295, 20, true);
			this.PlaceOfReportCodeFindBox.TabIndex = 1;
			// 
			// PortOfExitCodeFindBox
			// 
			this.PortOfExitCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PortOfExitCodeFindBox, "CA_PortOfExit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).CA_PortOfExit)));
			this.PortOfExitCodeFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|f9dc01cc-75b0-4dae-98ae-f34d306dc4d6", "Port Of Exit", "Customs Port Of Exit", "");
			this.PortOfExitCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 3, true);
			this.PortOfExitCodeFindBox.Name = "PortOfExitCodeFindBox";
			this.PortOfExitCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(295, 20, true);
			this.PortOfExitCodeFindBox.TabIndex = 0;
			// 
			// PermitsGroupBox
			// 
			this.PermitsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.PermitsGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|cd6c11a7-9b30-4ab6-8388-7dffba14b116", "Permits");
			this.PermitsGroupBox.Controls.Add(this.PermitsGrid);
			this.PermitsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 495, true);
			this.PermitsGroupBox.Name = "PermitsGroupBox";
			this.PermitsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 99, true);
			this.PermitsGroupBox.TabIndex = 9;
			this.PermitsGroupBox.TabStop = false;
			// 
			// PermitsGrid
			// 
			this.PermitsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PermitsGrid, "Permits");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).Permits)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.DeclarationExportPermit)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).Permits)).SyncRoot)).CY_Data)));
			this.PermitsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|504da8f8-dee7-4444-85e0-62974d925306", "Permit Number");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "CY_Data";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			this.PermitsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.PermitsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PermitsGrid.GridId = "a258c3d7-3a24-4f20-803d-68b053562ef8";
			this.PermitsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PermitsGrid.LayoutKey = "zGrid1";
			this.PermitsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.PermitsGrid.Name = "PermitsGrid";
			this.PermitsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(243, 80, true);
			this.PermitsGrid.TabIndex = 0;
			// 
			// ExportExtraDetailPanel
			// 
			this.ExportExtraDetailPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ExportExtraDetailPanel.Controls.Add(this.CA_RX_DeclaredCurrGuidFindBox);
			this.ExportExtraDetailPanel.Controls.Add(this.ReasonForExportDropEdit);
			this.ExportExtraDetailPanel.Controls.Add(this.CA_TransportDocumentNumberTextBox);
			this.ExportExtraDetailPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 173, true);
			this.ExportExtraDetailPanel.Name = "ExportExtraDetailPanel";
			this.ExportExtraDetailPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(450, 94, true);
			this.ExportExtraDetailPanel.TabIndex = 27;
			// 
			// CA_RX_DeclaredCurrGuidFindBox
			// 
			this.CA_RX_DeclaredCurrGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CA_RX_DeclaredCurrGuidFindBox, "CA_RX_DeclaredCurr");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).CA_RX_DeclaredCurr)));
			this.CA_RX_DeclaredCurrGuidFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|b4067645-a8b6-4340-b12b-f6917f5a5dce", "Declared Currency");
			this.CA_RX_DeclaredCurrGuidFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.CA_RX_DeclaredCurrGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 2, true);
			this.CA_RX_DeclaredCurrGuidFindBox.Name = "CA_RX_DeclaredCurrGuidFindBox";
			this.CA_RX_DeclaredCurrGuidFindBox.PreBoundMaxLength = 3;
			this.CA_RX_DeclaredCurrGuidFindBox.ShouldResize = true;
			this.CA_RX_DeclaredCurrGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(323, 20, true);
			this.CA_RX_DeclaredCurrGuidFindBox.TabIndex = 0;
			// 
			// ReasonForExportDropEdit
			// 
			this.ReasonForExportDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ReasonForExportDropEdit, "CA_ReasonForExportCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).CA_ReasonForExportCode)));
			this.ReasonForExportDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|b58bb214-7f62-4022-9f8f-977505ffe27a", "Reason For Export");
			this.ReasonForExportDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 25, true);
			this.ReasonForExportDropEdit.Name = "ReasonForExportDropEdit";
			this.ReasonForExportDropEdit.PreBoundMaxLength = 2;
			this.ReasonForExportDropEdit.ShouldResizeByMaxLength = true;
			this.ReasonForExportDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(323, 20, true);
			this.ReasonForExportDropEdit.TabIndex = 1;
			// 
			// CA_TransportDocumentNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.CA_TransportDocumentNumberTextBox, "CA_TransportDocumentNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).CA_TransportDocumentNumber)));
			this.CA_TransportDocumentNumberTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|add03fca-c7d0-4242-abbb-a0c2f0fb450b", "Trans Doc (CCN)");
			this.CA_TransportDocumentNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 71, true);
			this.CA_TransportDocumentNumberTextBox.Name = "CA_TransportDocumentNumberTextBox";
			this.CA_TransportDocumentNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(323, 20, true);
			this.CA_TransportDocumentNumberTextBox.TabIndex = 3;
			// 
			// B3EntryStatusDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.B3EntryStatusDescriptionTextBox, "B3EntryHeader.EntryHeaderStatusDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).B3EntryHeader.EntryHeaderStatusDescription)));
			this.B3EntryStatusDescriptionTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("7B904C93-6059-462E-A475-D8B96B4F75EB", "Entry Status");
			this.B3EntryStatusDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(342, 13, true);
			this.B3EntryStatusDescriptionTextBox.Name = "B3EntryStatusDescriptionTextBox";
			this.B3EntryStatusDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.B3EntryStatusDescriptionTextBox.TabIndex = 2;
			// 
			// LowValueShipmentLabel
			// 
			this.LowValueShipmentLabel.BackColor = System.Drawing.Color.LightSalmon;
			this.LowValueShipmentLabel.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("13DA0F31-71B3-4FE0-A6B0-584DD652E913", "Low Value Shipment");
			this.LowValueShipmentLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.LowValueShipmentLabel.IsFontBold = true;
			this.LowValueShipmentLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(50, 35, true);
			this.LowValueShipmentLabel.Name = "LowValueShipmentLabel";
			this.LowValueShipmentLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(134, 22, true);
			this.LowValueShipmentLabel.TabIndex = 55;
			this.LowValueShipmentLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// ImportPortPanel
			// 
			this.ImportPortPanel.Controls.Add(this.PortOfClearanceCodeFindBox);
			this.ImportPortPanel.Controls.Add(this.PortOfUnladingCodeFindBox);
			this.ImportPortPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 192, true);
			this.ImportPortPanel.Name = "ImportPortPanel";
			this.ImportPortPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(440, 50, true);
			this.ImportPortPanel.TabIndex = 17;
			// 
			// PortOfClearanceCodeFindBox
			// 
			this.PortOfClearanceCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PortOfClearanceCodeFindBox, "JE_CustomsOffice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).JE_CustomsOffice)));
			this.PortOfClearanceCodeFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|d54c0860-d12e-4362-b195-d23598c52f77", "Port Of Clearance", "Cust. Port Of Clearance", "Customs Port Of Clearance", "");
			this.PortOfClearanceCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 4, true);
			this.PortOfClearanceCodeFindBox.Name = "PortOfClearanceCodeFindBox";
			this.PortOfClearanceCodeFindBox.ShouldResize = true;
			this.PortOfClearanceCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(295, 20, true);
			this.PortOfClearanceCodeFindBox.TabIndex = 0;
			// 
			// PortOfUnladingCodeFindBox
			// 
			this.PortOfUnladingCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PortOfUnladingCodeFindBox, "CA_UnladingOffice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).CA_UnladingOffice)));
			this.PortOfUnladingCodeFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|a734a778-6112-4dde-8687-e41e4fca660f", "Unlading", "Port Of Unlading", "Customs Port Of Unlading", "Required for marine shipments valued at greater than CAN$2500 exported from the United States.");
			this.PortOfUnladingCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 27, true);
			this.PortOfUnladingCodeFindBox.Name = "PortOfUnladingCodeFindBox";
			this.PortOfUnladingCodeFindBox.ShouldResize = true;
			this.PortOfUnladingCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(295, 20, true);
			this.PortOfUnladingCodeFindBox.TabIndex = 1;
			// 
			// TransactionNumberPanel
			// 
			this.TransactionNumberPanel.Controls.Add(this.CheckDigitTextBox);
			this.TransactionNumberPanel.Controls.Add(this.SequentialNumberTextBox);
			this.TransactionNumberPanel.Controls.Add(this.SecurityCodeTextBox);
			this.TransactionNumberPanel.Controls.Add(this.FormattedTransactionNumberTextBox);
			this.TransactionNumberPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 13, true);
			this.TransactionNumberPanel.Name = "TransactionNumberPanel";
			this.TransactionNumberPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(273, 24, true);
			this.TransactionNumberPanel.TabIndex = 0;
			// 
			// CheckDigitTextBox
			// 
			this.CheckDigitTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CheckDigitTextBox, "TransactionNumber.CheckDigit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).TransactionNumber.CheckDigit)));
			this.CheckDigitTextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.CheckDigitTextBox, false);
			this.CheckDigitTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(255, 2, true);
			this.CheckDigitTextBox.Name = "CheckDigitTextBox";
			this.CheckDigitTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 20, true);
			this.CheckDigitTextBox.TabIndex = 2;
			this.CheckDigitTextBox.Text = "1";
			this.CheckDigitTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// SequentialNumberTextBox
			// 
			this.SequentialNumberTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.SequentialNumberTextBox, "TransactionNumber.SequentialNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).TransactionNumber.SequentialNumber)));
			this.SequentialNumberTextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.SequentialNumberTextBox, false);
			this.SequentialNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(192, 2, true);
			this.SequentialNumberTextBox.Name = "SequentialNumberTextBox";
			this.SequentialNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 20, true);
			this.SequentialNumberTextBox.TabIndex = 1;
			this.SequentialNumberTextBox.Text = "12345678";
			this.SequentialNumberTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// SecurityCodeTextBox
			// 
			this.SecurityCodeTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.SecurityCodeTextBox, "TransactionNumber.AccountSecurityCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).TransactionNumber.AccountSecurityCode)));
			this.SecurityCodeTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|d50d8243-17b7-4156-bdde-447c14229331", "Transaction Number");
			this.SecurityCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(146, 2, true);
			this.SecurityCodeTextBox.Name = "SecurityCodeTextBox";
			this.SecurityCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.SecurityCodeTextBox.TabIndex = 0;
			this.SecurityCodeTextBox.Text = "12345000000012";
			this.SecurityCodeTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// FormattedTransactionNumberTextBox
			// 
			this.FormattedTransactionNumberTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.FormattedTransactionNumberTextBox, "TransactionNumber.FormattedTransactionNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).TransactionNumber.FormattedTransactionNumber)));
			this.FormattedTransactionNumberTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|d50d8243-17b7-4156-bdde-447c14229331", "Transaction Number");
			this.FormattedTransactionNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(146, 2, true);
			this.FormattedTransactionNumberTextBox.Name = "FormattedTransactionNumberTextBox";
			this.FormattedTransactionNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.FormattedTransactionNumberTextBox.TabIndex = 0;
			this.FormattedTransactionNumberTextBox.Text = "12345";
			this.FormattedTransactionNumberTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ImportExtraDetailsZPanel
			// 
			this.ImportExtraDetailsZPanel.Controls.Add(this.ExceptionDescriptionLabel);
			this.ImportExtraDetailsZPanel.Controls.Add(this.SubLocationNameTextBox);
			this.ImportExtraDetailsZPanel.Controls.Add(this.ReleaseDateDateEdit);
			this.ImportExtraDetailsZPanel.Controls.Add(this.EstReleaseDateDateEdit);
			this.ImportExtraDetailsZPanel.Controls.Add(this.WarehouseReleaseDateDateEdit);
			this.ImportExtraDetailsZPanel.Controls.Add(this.SubLocationCodeFindBox);
			this.ImportExtraDetailsZPanel.Controls.Add(this.ExamLocationCodeFindBox);
			this.ImportExtraDetailsZPanel.Controls.Add(this.ExamLocationTextBox);
			this.ImportExtraDetailsZPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 245, true);
			this.ImportExtraDetailsZPanel.Name = "ImportExtraDetailsZPanel";
			this.ImportExtraDetailsZPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(440, 118, true);
			this.ImportExtraDetailsZPanel.TabIndex = 18;
			// 
			// ExceptionDescriptionLabel
			// 
			this.ExceptionDescriptionLabel.BackColor = System.Drawing.Color.LightSalmon;
			this.BindingSource.SetBindingMember(this.ExceptionDescriptionLabel, "CA_DeclarationExceptionDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).CA_DeclarationExceptionDescription)));
			this.ExceptionDescriptionLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.ExceptionDescriptionLabel.IsFontBold = true;
			this.ExceptionDescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 92, true);
			this.ExceptionDescriptionLabel.Name = "ExceptionDescriptionLabel";
			this.ExceptionDescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(323, 22, true);
			this.ExceptionDescriptionLabel.TabIndex = 0;
			// 
			// SubLocationNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.SubLocationNameTextBox, "CA_SubLocationName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).CA_SubLocationName)));
			this.SubLocationNameTextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.SubLocationNameTextBox, false);
			this.SubLocationNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(179, 5, true);
			this.SubLocationNameTextBox.Name = "SubLocationNameTextBox";
			this.SubLocationNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(256, 20, true);
			this.SubLocationNameTextBox.TabIndex = 3;
			// 
			// ReleaseDateDateEdit
			// 
			this.ReleaseDateDateEdit.AllowDrop = true;
			this.ReleaseDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.ReleaseDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ReleaseDateDateEdit, "JE_EntryAuthorisationDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).JE_EntryAuthorisationDate)));
			this.ReleaseDateDateEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|9bbe62e7-884c-4580-bdfc-097058b32eb5", "ARD", "Actual Release Date", "");
			this.ReleaseDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(346, 71, true);
			this.ReleaseDateDateEdit.Name = "ReleaseDateDateEdit";
			this.ReleaseDateDateEdit.TabIndex = 9;
			// 
			// EstReleaseDateDateEdit
			// 
			this.EstReleaseDateDateEdit.AllowDrop = true;
			this.EstReleaseDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.EstReleaseDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.EstReleaseDateDateEdit, "CA_EstReleaseDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).CA_EstReleaseDate)));
			this.EstReleaseDateDateEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|d31c8914-451a-4244-9d6f-356a6ab11dcd", "ERD", "Estimated Release Date", "");
			this.EstReleaseDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(149, 70, true);
			this.EstReleaseDateDateEdit.Name = "EstReleaseDateDateEdit";
			this.EstReleaseDateDateEdit.TabIndex = 7;
			// 
			// WarehouseReleaseDateDateEdit
			// 
			this.WarehouseReleaseDateDateEdit.AllowDrop = true;
			this.WarehouseReleaseDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.WarehouseReleaseDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.WarehouseReleaseDateDateEdit, "JE_WarehouseReleaseDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).JE_WarehouseReleaseDate)));
			this.WarehouseReleaseDateDateEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|16956e9e-8d2b-44d4-8a2b-dee84b93a645", "ETD from Whs.", "ETD from Warehouse");
			this.WarehouseReleaseDateDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.WarehouseReleaseDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 49, true);
			this.WarehouseReleaseDateDateEdit.Name = "WarehouseReleaseDateDateEdit";
			this.WarehouseReleaseDateDateEdit.TabIndex = 6;
			// 
			// SubLocationCodeFindBox
			// 
			this.SubLocationCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SubLocationCodeFindBox, "JE_LocationOfGoods");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).JE_LocationOfGoods)));
			this.SubLocationCodeFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|fdbaf485-e77c-407f-ae04-9677c5ff63ae", "Sub-Loc.", "Sub-Location", "");
			this.SubLocationCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 5, true);
			this.SubLocationCodeFindBox.Name = "SubLocationCodeFindBox";
			this.SubLocationCodeFindBox.PreBoundMaxLength = 4;
			this.SubLocationCodeFindBox.ShouldResize = true;
			this.SubLocationCodeFindBox.ShowDescriptionBox = false;
			this.SubLocationCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 20, true);
			this.SubLocationCodeFindBox.TabIndex = 2;
			// 
			// ExamLocationCodeFindBox
			// 
			this.ExamLocationCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExamLocationCodeFindBox, "CA_ExamLocationCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).CA_ExamLocationCode)));
			this.ExamLocationCodeFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|33BB9AC3-19BD-4737-A6C9-6151CB75D394", "Exam Location");
			this.ExamLocationCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 27, true);
			this.ExamLocationCodeFindBox.Name = "ExamLocationCodeFindBox";
			this.ExamLocationCodeFindBox.PreBoundMaxLength = 4;
			this.ExamLocationCodeFindBox.ShouldResize = true;
			this.ExamLocationCodeFindBox.ShowDescriptionBox = false;
			this.ExamLocationCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 20, true);
			this.ExamLocationCodeFindBox.TabIndex = 4;
			// 
			// ExamLocationTextBox
			// 
			this.BindingSource.SetBindingMember(this.ExamLocationTextBox, "ExamLocationDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).ExamLocationDescription)));
			this.ExamLocationTextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ExamLocationTextBox, false);
			this.ExamLocationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(179, 27, true);
			this.ExamLocationTextBox.Name = "ExamLocationTextBox";
			this.ExamLocationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(256, 20, true);
			this.ExamLocationTextBox.TabIndex = 5;
			// 
			// CarrierCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.CarrierCodeTextBox, "CA_CarrierName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).CA_CarrierName)));
			this.CarrierCodeTextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.CarrierCodeTextBox, false);
			this.CarrierCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(179, 15, true);
			this.CarrierCodeTextBox.Name = "CarrierCodeTextBox";
			this.CarrierCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(256, 20, true);
			this.CarrierCodeTextBox.TabIndex = 1;
			// 
			// CarrierCodeFindBox
			// 
			this.CarrierCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CarrierCodeFindBox, "JE_CarrierCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).JE_CarrierCode)));
			this.CarrierCodeFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|3cae9391-ee6f-42b0-b608-8eba7e3ae18e", "Carrier", "Carrier at Import.", "Carrier at Importation", "");
			this.CarrierCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 15, true);
			this.CarrierCodeFindBox.Name = "CarrierCodeFindBox";
			this.CarrierCodeFindBox.PreBoundMaxLength = 4;
			this.CarrierCodeFindBox.ShouldResize = true;
			this.CarrierCodeFindBox.ShowDescriptionBox = false;
			this.CarrierCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 20, true);
			this.CarrierCodeFindBox.TabIndex = 0;
			// 
			// EDIReleaseOptionsGroupBox
			// 
			this.EDIReleaseOptionsGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|306ad8fe-6a50-46e1-832b-e09663c09cab", "Options", "Release Options", "");
			this.EDIReleaseOptionsGroupBox.Controls.Add(this.AssesmentOptionDropEdit);
			this.EDIReleaseOptionsGroupBox.Controls.Add(this.ServiceOptionDropEdit);
			this.EDIReleaseOptionsGroupBox.Controls.Add(this.EnableB3CheckBox);
			this.EDIReleaseOptionsGroupBox.Controls.Add(this.EnableACROSSCheckBox);
			this.EDIReleaseOptionsGroupBox.Controls.Add(this.PriorityIndDropEdit);
			this.EDIReleaseOptionsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 494, true);
			this.EDIReleaseOptionsGroupBox.Name = "EDIReleaseOptionsGroupBox";
			this.EDIReleaseOptionsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 84, true);
			this.EDIReleaseOptionsGroupBox.TabIndex = 3;
			this.EDIReleaseOptionsGroupBox.TabStop = false;
			// 
			// AssesmentOptionDropEdit
			// 
			this.AssesmentOptionDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AssesmentOptionDropEdit, "CA_AssesmentOption");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).CA_AssesmentOption)));
			this.AssesmentOptionDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|b9698847-86b9-4ce2-ac9f-47d4bcead07c", "Assessment", "Assessment Option", "");
			this.AssesmentOptionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(70, 37, true);
			this.AssesmentOptionDropEdit.Name = "AssesmentOptionDropEdit";
			this.AssesmentOptionDropEdit.PreBoundMaxLength = 3;
			this.AssesmentOptionDropEdit.ShouldResizeByMaxLength = true;
			this.AssesmentOptionDropEdit.ShowDescriptionBox = false;
			this.AssesmentOptionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.AssesmentOptionDropEdit.TabIndex = 1;
			// 
			// ServiceOptionDropEdit
			// 
			this.ServiceOptionDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ServiceOptionDropEdit, "CA_ServiceOption");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).CA_ServiceOption)));
			this.ServiceOptionDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|ab284046-1e2a-4190-a05f-9ac8e709b1f4", "Service", "Option", "Service Option");
			this.ServiceOptionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(70, 16, true);
			this.ServiceOptionDropEdit.Name = "ServiceOptionDropEdit";
			this.ServiceOptionDropEdit.PreBoundMaxLength = 3;
			this.ServiceOptionDropEdit.ShouldResizeByMaxLength = true;
			this.ServiceOptionDropEdit.ShowDescriptionBox = false;
			this.ServiceOptionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.ServiceOptionDropEdit.TabIndex = 0;
			// 
			// EnableB3CheckBox
			// 
			this.BindingSource.SetBindingMember(this.EnableB3CheckBox, "IsEnableB3Validation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).IsEnableB3Validation)));
			this.EnableB3CheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("4f78bcd5-085b-4b49-92c8-deca32ad293f", "Validate CADEX", "Check this box to enable Entry level validations.  If not checked, validations will be performed only when the Entry is sent", "Check this box to enable Entry level validations. If not checked, validations will be performed only when the Entry is sent.");
			this.EnableB3CheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.EnableB3CheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.EnableB3CheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 58, true);
			this.EnableB3CheckBox.Name = "EnableB3CheckBox";
			this.EnableB3CheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 24, true);
			this.EnableB3CheckBox.TabIndex = 4;
			this.EnableB3CheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.EnableB3CheckBox.UseVisualStyleBackColor = true;
			// 
			// EnableACROSSCheckBox
			// 
			this.BindingSource.SetBindingMember(this.EnableACROSSCheckBox, "IsEnableACROSSValidation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).IsEnableACROSSValidation)));
			this.EnableACROSSCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("feea56db-a0bc-4223-b16b-fa42685237d1", "Validate Release", "Check this box to enable Release level validations.  If not checked, validations will be performed only when this declaration is sent to Release", "Check this box to enable Release level validations. If not checked, validations will be performed only when this declaration is sent to Release.");
			this.EnableACROSSCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.EnableACROSSCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.EnableACROSSCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 33, true);
			this.EnableACROSSCheckBox.Name = "EnableACROSSCheckBox";
			this.EnableACROSSCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 24, true);
			this.EnableACROSSCheckBox.TabIndex = 2;
			this.EnableACROSSCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.EnableACROSSCheckBox.UseVisualStyleBackColor = true;
			// 
			// PriorityIndDropEdit
			// 
			this.PriorityIndDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PriorityIndDropEdit, "CA_PriorityInd");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).CA_PriorityInd)));
			this.PriorityIndDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|c58d30ae-7529-4800-a51c-996de52d76cf", "Priority", "Priority Ind.", "Priority Indicator", "Indicates the type of processing priority.");
			this.PriorityIndDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(70, 58, true);
			this.PriorityIndDropEdit.Name = "PriorityIndDropEdit";
			this.PriorityIndDropEdit.ShouldResizeByMaxLength = true;
			this.PriorityIndDropEdit.ShowDescriptionBox = false;
			this.PriorityIndDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.PriorityIndDropEdit.TabIndex = 3;
			// 
			// NetWeightCalcDropEdit
			// 
			this.NetWeightCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NetWeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).CA_NetWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).CA_NetWeightUQ)));
			this.NetWeightCalcDropEdit.BindToAmount = "CA_NetWeight";
			this.NetWeightCalcDropEdit.BindToUnit = "CA_NetWeightUQ";
			this.NetWeightCalcDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|5f10f788-e099-4bbb-a3e5-b211d050c5df", "Weight", "Net Weight", "Total Net Weight", "The total net weight of this shipment.");
			this.NetWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 150, true);
			this.NetWeightCalcDropEdit.Name = "NetWeightCalcDropEdit";
			this.NetWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.NetWeightCalcDropEdit.TabIndex = 14;
			this.NetWeightCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// VendorAndOriginatorPanel
			// 
			this.VendorAndOriginatorPanel.Controls.Add(this.ImporterOfRecordDocAddressControl);
			this.VendorAndOriginatorPanel.Controls.Add(this.CommercialInvoiceOriginatorDocAddressControl);
			this.VendorAndOriginatorPanel.Controls.Add(this.ImporterDocumentaryAddress);
			this.VendorAndOriginatorPanel.Controls.Add(this.VendorAddressControl);
			this.VendorAndOriginatorPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.VendorAndOriginatorPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 206, true);
			this.VendorAndOriginatorPanel.Name = "VendorAndOriginatorPanel";
			this.VendorAndOriginatorPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(279, 256, true);
			this.VendorAndOriginatorPanel.TabIndex = 11;
			// 
			// CommercialInvoiceOriginatorDocAddressControl
			// 
			this.CommercialInvoiceOriginatorDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CommercialInvoiceOriginatorDocAddressControl, "CommercialInvoiceOriginator");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).CommercialInvoiceOriginator)));
			this.CommercialInvoiceOriginatorDocAddressControl.BindToOrganisations = "Lookups+Organizations";
			this.CommercialInvoiceOriginatorDocAddressControl.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("312e84f3-9037-4b06-b6a2-f5c006ef26e1", "Com. Inv. Originator", "Commercial Invoice Originator", "");
			this.CommercialInvoiceOriginatorDocAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox;
			this.CommercialInvoiceOriginatorDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 44, true);
			this.CommercialInvoiceOriginatorDocAddressControl.Name = "CommercialInvoiceOriginatorDocAddressControl";
			this.CommercialInvoiceOriginatorDocAddressControl.ReadOnly = false;
			this.CommercialInvoiceOriginatorDocAddressControl.SingleLineNoGroupBoxPanelWidth = 320;
			this.CommercialInvoiceOriginatorDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
			this.CommercialInvoiceOriginatorDocAddressControl.TabIndex = 2;
			this.CommercialInvoiceOriginatorDocAddressControl.ValidationJustForced = false;
			// 
			// ImporterDocumentaryAddress
			// 
			this.ImporterDocumentaryAddress.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ImporterDocumentaryAddress, "ImporterDocumentaryAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).ImporterDocumentaryAddress)));
			this.ImporterDocumentaryAddress.BindToOrganisations = "Lookups.ImportersList";
			this.ImporterDocumentaryAddress.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("D5AD5113-07A6-4749-829A-6E6E4EFA2CB5", "Consignee");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ImporterDocumentaryAddress, false);
			this.ImporterDocumentaryAddress.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 70, true);
			this.ImporterDocumentaryAddress.Name = "ImporterDocumentaryAddress";
			this.ImporterDocumentaryAddress.ReadOnly = false;
			this.ImporterDocumentaryAddress.SingleLineNoGroupBoxPanelWidth = 296;
			this.ImporterDocumentaryAddress.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.ImporterDocumentaryAddress.TabIndex = 3;
			this.ImporterDocumentaryAddress.ValidationJustForced = false;
			this.ImporterDocumentaryAddress.Visible = false;
			// 
			// ScheduledB3DateEdit
			// 
			this.ScheduledB3DateEdit.AllowDrop = true;
			this.ScheduledB3DateEdit.AutoCompleteMonthThreshold = 1;
			this.ScheduledB3DateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ScheduledB3DateEdit, "ScheduledB3SendingDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).ScheduledB3SendingDate)));
			this.ScheduledB3DateEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("6D22C7AC-3B6D-44D4-9AB5-5452D0A1DC15", "Entry Scheduled Time", "Scheduled Sending Time For Entry Message", "");
			this.ScheduledB3DateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.ScheduledB3DateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(626, 13, true);
			this.ScheduledB3DateEdit.Name = "ScheduledB3DateEdit";
			this.ScheduledB3DateEdit.TabIndex = 56;
			// 
			// SupplierDocumentAddress
			// 
			this.SupplierDocumentAddress.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SupplierDocumentAddress, "SupplierDocumentaryAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).SupplierDocumentaryAddress)));
			this.SupplierDocumentAddress.BindToOrganisations = "Lookups.SuppliersList";
			this.SupplierDocumentAddress.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("24e60521-3d52-486b-9b99-f316d928ec02", "Main Vendor");
			this.SupplierDocumentAddress.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 4, true);
			this.SupplierDocumentAddress.Name = "SupplierDocumentAddress";
			this.SupplierDocumentAddress.ReadOnly = false;
			this.SupplierDocumentAddress.SingleLineNoGroupBoxPanelWidth = 296;
			this.SupplierDocumentAddress.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.SupplierDocumentAddress.TabIndex = 0;
			this.SupplierDocumentAddress.ValidationJustForced = false;
			this.SupplierDocumentAddress.Visible = false;
			// 
			// ImporterOfRecordDocAddressControl
			// 
			this.ImporterOfRecordDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ImporterOfRecordDocAddressControl, "ImporterOfRecordAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).ImporterOfRecordAddress)));
			this.ImporterOfRecordDocAddressControl.BindToOrganisations = "Lookups+ImportersList";
			this.ImporterOfRecordDocAddressControl.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("85f8829f-6c73-49b8-b670-fc291e1ec083", "Importer of Record");
			this.ImporterOfRecordDocAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox;
			this.ImporterOfRecordDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 0, true);
			this.ImporterOfRecordDocAddressControl.Name = "ImporterOfRecordDocAddressControl";
			this.ImporterOfRecordDocAddressControl.ReadOnly = false;
			this.ImporterOfRecordDocAddressControl.SingleLineNoGroupBoxPanelWidth = 320;
			this.ImporterOfRecordDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
			this.ImporterOfRecordDocAddressControl.TabIndex = 0;
			this.ImporterOfRecordDocAddressControl.ValidationJustForced = false;
			// 
			// groupBoxManualSubmissionInfo
			// 
			this.groupBoxManualSubmissionInfo.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|91f545f9-0623-49d8-8e22-e9da4f616f6e", "Manual Submission Info");
			this.groupBoxManualSubmissionInfo.Controls.Add(this.B3CSubmittedOffice);
			this.groupBoxManualSubmissionInfo.Controls.Add(this.RELSubmittedOffice);
			this.groupBoxManualSubmissionInfo.Controls.Add(this.B3CSubmittedDate);
			this.groupBoxManualSubmissionInfo.Controls.Add(this.RELSubmittedDate);
			this.groupBoxManualSubmissionInfo.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1002, 8, true);
			this.groupBoxManualSubmissionInfo.Name = "groupBoxManualSubmissionInfo";
			this.groupBoxManualSubmissionInfo.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(305, 62, true);
			this.groupBoxManualSubmissionInfo.TabIndex = 10;
			this.groupBoxManualSubmissionInfo.TabStop = false;
			// 
			// B3CSubmittedOffice
			// 
			this.BindingSource.SetBindingMember(this.B3CSubmittedOffice, "B3EntryHeader+EffectivePortOfClearance");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).B3EntryHeader.EffectivePortOfClearance)));
			this.B3CSubmittedOffice.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("81c4fd18-ad35-4510-a953-dac17785a8fb", "Office");
			this.B3CSubmittedOffice.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(253, 37, true);
			this.B3CSubmittedOffice.Name = "B3CSubmittedOffice";
			this.B3CSubmittedOffice.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.B3CSubmittedOffice.TabIndex = 6;
			// 
			// RELSubmittedOffice
			// 
			this.BindingSource.SetBindingMember(this.RELSubmittedOffice, "ReleaseEntryHeader+EffectivePortOfClearance");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).ReleaseEntryHeader.EffectivePortOfClearance)));
			this.RELSubmittedOffice.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("d2b1ad2b-83d2-4dd0-971b-a1b71848dd37", "Office");
			this.RELSubmittedOffice.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(253, 13, true);
			this.RELSubmittedOffice.Name = "RELSubmittedOffice";
			this.RELSubmittedOffice.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.RELSubmittedOffice.TabIndex = 5;
			// 
			// B3CSubmittedDate
			// 
			this.B3CSubmittedDate.AllowDrop = true;
			this.B3CSubmittedDate.AutoCompleteMonthThreshold = 1;
			this.B3CSubmittedDate.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.B3CSubmittedDate, "B3EntryHeader+CH_EntrySubmittedDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).B3EntryHeader.CH_EntrySubmittedDate)));
			this.B3CSubmittedDate.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("cda7bb1f-99da-4a4d-ba09-505c0116d745", "B3C Submitted Time");
			this.B3CSubmittedDate.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.B3CSubmittedDate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 37, true);
			this.B3CSubmittedDate.Name = "B3CSubmittedDate";
			this.B3CSubmittedDate.TabIndex = 4;
			// 
			// RELSubmittedDate
			// 
			this.RELSubmittedDate.AllowDrop = true;
			this.RELSubmittedDate.AutoCompleteMonthThreshold = 1;
			this.RELSubmittedDate.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.RELSubmittedDate, "ReleaseEntryHeader+CH_EntrySubmittedDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).ReleaseEntryHeader.CH_EntrySubmittedDate)));
			this.RELSubmittedDate.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("f91d1188-dac5-4a10-8bb7-05428a495484", "REL Submitted Time");
			this.RELSubmittedDate.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.RELSubmittedDate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 13, true);
			this.RELSubmittedDate.Name = "RELSubmittedDate";
			this.RELSubmittedDate.TabIndex = 3;
			// 
			// CargoControlNumberNoBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.CargoControlNumberNoBoundTextBox, "EffectiveCCN");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).EffectiveCCN)));
			this.CargoControlNumberNoBoundTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("55f4d997-b3e4-4ddd-b487-b755a6683e83", "Cargo Control No");
			this.CargoControlNumberNoBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 37, true);
			this.CargoControlNumberNoBoundTextBox.Name = "CargoControlNumberNoBoundTextBox";
			this.CargoControlNumberNoBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(217, 20, true);
			this.CargoControlNumberNoBoundTextBox.TabIndex = 1;
			// 
			// DefaultHWBButton
			// 
			this.DefaultHWBButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(338, 37, true);
			this.DefaultHWBButton.Name = "DefaultHWBButton";
			this.DefaultHWBButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.DefaultHWBButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(99, 20, true);
			this.DefaultHWBButton.TabIndex = 3;
			this.DefaultHWBButton.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("88c744e5-35f5-4e4b-a224-02c1bf319b4e", "Default HWB");
			this.DefaultHWBButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.DefaultHWBButton.ToolTipCaption = null;
			this.DefaultHWBButton.Click += new System.EventHandler(this.DefaultHWBButton_Click);
			// 
			// PortOfFirstArrivalCodeFindBox
			// 
			this.PortOfFirstArrivalCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PortOfFirstArrivalCodeFindBox, "JE_RL_NKPortOfFirstArrival");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).JE_RL_NKPortOfFirstArrival)));
			this.PortOfFirstArrivalCodeFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("88f9141d-0f17-4400-8197-89774d1a3985", "First Port Arr.", "First Port Arrival");
			this.PortOfFirstArrivalCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 169, true);
			this.PortOfFirstArrivalCodeFindBox.Name = "PortOfFirstArrivalCodeFindBox";
			this.PortOfFirstArrivalCodeFindBox.ShouldResize = true;
			this.PortOfFirstArrivalCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(174, 20, true);
			this.PortOfFirstArrivalCodeFindBox.TabIndex = 14;
			// 
			// DateOfFirstArrivalDateEdit
			// 
			this.DateOfFirstArrivalDateEdit.AllowDrop = true;
			this.DateOfFirstArrivalDateEdit.AutoCompleteMonthThreshold = 1;
			this.DateOfFirstArrivalDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DateOfFirstArrivalDateEdit, "JE_DateOfFirstArrival");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).JE_DateOfFirstArrival)));
			this.DateOfFirstArrivalDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.DateOfFirstArrivalDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(327, 169, true);
			this.DateOfFirstArrivalDateEdit.Name = "DateOfFirstArrivalDateEdit";
			this.DateOfFirstArrivalDateEdit.TabIndex = 15;
			// 
			// CargoControlNumberPrefixNoBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.CargoControlNumberPrefixNoBoundTextBox, "EffectiveCCNPrefix");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).EffectiveCCNPrefix)));
			this.CargoControlNumberPrefixNoBoundTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("F1DE62AB-30A3-4A46-9D41-4C4BCFF2084C", "Cargo Control No");
			this.CargoControlNumberPrefixNoBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 37, true);
			this.CargoControlNumberPrefixNoBoundTextBox.Name = "CargoControlNumberPrefixNoBoundTextBox";
			this.CargoControlNumberPrefixNoBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 20, true);
			this.CargoControlNumberPrefixNoBoundTextBox.TabIndex = 1;
			// 
			// CargoControlNumberSuffixNoBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.CargoControlNumberSuffixNoBoundTextBox, "EffectiveCCNSuffix");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).EffectiveCCNSuffix)));
			this.CargoControlNumberSuffixNoBoundTextBox.CaptionResourceString = null;
			this.CargoControlNumberSuffixNoBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(180, 37, true);
			this.CargoControlNumberSuffixNoBoundTextBox.Name = "CargoControlNumberSuffixNoBoundTextBox";
			this.CargoControlNumberSuffixNoBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(149, 20, true);
			this.CargoControlNumberSuffixNoBoundTextBox.TabIndex = 2;
			// 
			// DeliveryInstructionsLongTextControl
			//
			this.BindingSource.SetBindingMember(this.DeliveryInstructionsLongTextControl, "ReleaseEntryHeader.CH_CustomsDeliveryInstructions");
			this.DeliveryInstructionsLongTextControl.AllowDrop = true;
			this.DeliveryInstructionsLongTextControl.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("168db965-f186-46b0-8bb5-3221419d785e", "Delivery Instructions");
			this.DeliveryInstructionsLongTextControl.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			this.DeliveryInstructionsLongTextControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 369, true);
			this.DeliveryInstructionsLongTextControl.Name = "DeliveryInstructionsLongTextControl";
			this.DeliveryInstructionsLongTextControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
			this.DeliveryInstructionsLongTextControl.TabIndex = 19;
			// 
			// CAJobDeclarationUserControl
			// 
			this.Controls.Add(this.groupBoxManualSubmissionInfo);
			this.Controls.Add(this.SupplierOrgAddressControl);
			this.Controls.Add(this.ImporterOrgAddressControl);
			this.Controls.Add(this.SupplierDocumentAddress);
			this.Controls.Add(this.EDIReleaseOptionsGroupBox);
			this.Controls.Add(this.PermitsGroupBox);
			this.Name = "CAJobDeclarationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1003, 630, true);
			this.Controls.SetChildIndex(this.SupplierOrganisationControl, 0);
			this.Controls.SetChildIndex(this.PermitsGroupBox, 0);
			this.Controls.SetChildIndex(this.EDIReleaseOptionsGroupBox, 0);
			this.Controls.SetChildIndex(this.ShipmentTypeGroupBox, 0);
			this.Controls.SetChildIndex(this.ImporterOrganisationControl, 0);
			this.Controls.SetChildIndex(this.ShipmentDetailsGroupBox, 0);
			this.Controls.SetChildIndex(this.TransportDetailsGroupBox, 0);
			this.Controls.SetChildIndex(this.RightTabControl, 0);
			this.Controls.SetChildIndex(this.DeclarationDetailsGroupBox, 0);
			this.Controls.SetChildIndex(this.SupplierDocumentAddress, 0);
			this.Controls.SetChildIndex(this.ImporterOrgAddressControl, 0);
			this.Controls.SetChildIndex(this.SupplierOrgAddressControl, 0);
			this.Controls.SetChildIndex(this.groupBoxManualSubmissionInfo, 0);
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
			this.DepotAddressControl.ResumeLayout(true);
			this.DepotAddressControl.PerformLayout();
			this.BondedWarehouseDocAddressControl.ResumeLayout(true);
			this.BondedWarehouseDocAddressControl.PerformLayout();
			this.ContainerYardAddressControl.ResumeLayout(true);
			this.ContainerYardAddressControl.PerformLayout();
			this.NumbersTabPage.ResumeLayout(false);
			this.NumbersTabPage.PerformLayout();
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
			this.VendorAddressControl.ResumeLayout(true);
			this.VendorAddressControl.PerformLayout();
			this.ExportPortPanel.ResumeLayout(false);
			this.ExportPortPanel.PerformLayout();
			this.PlaceOfReportCodeFindBox.ResumeLayout(true);
			this.PlaceOfReportCodeFindBox.PerformLayout();
			this.PortOfExitCodeFindBox.ResumeLayout(true);
			this.PortOfExitCodeFindBox.PerformLayout();
			this.PermitsGroupBox.ResumeLayout(false);
			this.PermitsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PermitsGrid)).EndInit();
			this.PermitsGrid.ResumeLayout(false);
			this.PermitsGrid.PerformLayout();
			this.ExportExtraDetailPanel.ResumeLayout(false);
			this.ExportExtraDetailPanel.PerformLayout();
			this.CA_RX_DeclaredCurrGuidFindBox.ResumeLayout(true);
			this.CA_RX_DeclaredCurrGuidFindBox.PerformLayout();
			this.ReasonForExportDropEdit.ResumeLayout(true);
			this.ReasonForExportDropEdit.PerformLayout();
			this.ImportPortPanel.ResumeLayout(false);
			this.ImportPortPanel.PerformLayout();
			this.PortOfClearanceCodeFindBox.ResumeLayout(true);
			this.PortOfClearanceCodeFindBox.PerformLayout();
			this.PortOfUnladingCodeFindBox.ResumeLayout(true);
			this.PortOfUnladingCodeFindBox.PerformLayout();
			this.TransactionNumberPanel.ResumeLayout(false);
			this.TransactionNumberPanel.PerformLayout();
			this.ImportExtraDetailsZPanel.ResumeLayout(false);
			this.ImportExtraDetailsZPanel.PerformLayout();
			this.ReleaseDateDateEdit.ResumeLayout(true);
			this.ReleaseDateDateEdit.PerformLayout();
			this.EstReleaseDateDateEdit.ResumeLayout(true);
			this.EstReleaseDateDateEdit.PerformLayout();
			this.WarehouseReleaseDateDateEdit.ResumeLayout(true);
			this.WarehouseReleaseDateDateEdit.PerformLayout();
			this.SubLocationCodeFindBox.ResumeLayout(true);
			this.SubLocationCodeFindBox.PerformLayout();
			this.ExamLocationCodeFindBox.ResumeLayout(true);
			this.ExamLocationCodeFindBox.PerformLayout();
			this.CarrierCodeFindBox.ResumeLayout(true);
			this.CarrierCodeFindBox.PerformLayout();
			this.EDIReleaseOptionsGroupBox.ResumeLayout(false);
			this.EDIReleaseOptionsGroupBox.PerformLayout();
			this.AssesmentOptionDropEdit.ResumeLayout(true);
			this.AssesmentOptionDropEdit.PerformLayout();
			this.ServiceOptionDropEdit.ResumeLayout(true);
			this.ServiceOptionDropEdit.PerformLayout();
			this.PriorityIndDropEdit.ResumeLayout(true);
			this.PriorityIndDropEdit.PerformLayout();
			this.NetWeightCalcDropEdit.ResumeLayout(true);
			this.NetWeightCalcDropEdit.PerformLayout();
			this.VendorAndOriginatorPanel.ResumeLayout(false);
			this.VendorAndOriginatorPanel.PerformLayout();
			this.ImporterOfRecordDocAddressControl.ResumeLayout(true);
			this.ImporterOfRecordDocAddressControl.PerformLayout();
			this.CommercialInvoiceOriginatorDocAddressControl.ResumeLayout(true);
			this.CommercialInvoiceOriginatorDocAddressControl.PerformLayout();
			this.ImporterDocumentaryAddress.ResumeLayout(true);
			this.ImporterDocumentaryAddress.PerformLayout();
			this.ScheduledB3DateEdit.ResumeLayout(true);
			this.ScheduledB3DateEdit.PerformLayout();
			this.SupplierDocumentAddress.ResumeLayout(true);
			this.SupplierDocumentAddress.PerformLayout();
			this.ImporterOrgAddressControl.ResumeLayout(true);
			this.ImporterOrgAddressControl.PerformLayout();
			this.SupplierOrgAddressControl.ResumeLayout(true);
			this.SupplierOrgAddressControl.PerformLayout();
			this.groupBoxManualSubmissionInfo.ResumeLayout(false);
			this.groupBoxManualSubmissionInfo.PerformLayout();
			this.B3CSubmittedDate.ResumeLayout(true);
			this.B3CSubmittedDate.PerformLayout();
			this.RELSubmittedDate.ResumeLayout(true);
			this.RELSubmittedDate.PerformLayout();
			this.PortOfFirstArrivalCodeFindBox.ResumeLayout(true);
			this.PortOfFirstArrivalCodeFindBox.PerformLayout();
			this.DateOfFirstArrivalDateEdit.ResumeLayout(true);
			this.DateOfFirstArrivalDateEdit.PerformLayout();
			this.DeliveryInstructionsLongTextControl.ResumeLayout(true);
			this.DeliveryInstructionsLongTextControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private Enterprise.ZArchitecture.GUI.ZPanel ExportPortPanel;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox PortOfExitCodeFindBox;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox PlaceOfReportCodeFindBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox PermitsGroupBox;
		private Enterprise.ZArchitecture.ZGrid PermitsGrid;
		private Enterprise.ZArchitecture.GUI.ZPanel ExportExtraDetailPanel;
		public Enterprise.ZArchitecture.ZTextBox B3EntryStatusDescriptionTextBox;
		private Enterprise.ZArchitecture.ZLabel LowValueShipmentLabel;
		private Enterprise.ZArchitecture.GUI.ZPanel ImportPortPanel;
		private ZCodeFindBox PortOfClearanceCodeFindBox;
		private ZCodeFindBox PortOfUnladingCodeFindBox;
		internal Enterprise.ZArchitecture.GUI.ZPanel TransactionNumberPanel;
		private Enterprise.ZArchitecture.ZTextBox CheckDigitTextBox;
		private Enterprise.ZArchitecture.ZTextBox SequentialNumberTextBox;
		private Enterprise.ZArchitecture.ZTextBox SecurityCodeTextBox;
		private Enterprise.ZArchitecture.ZTextBox FormattedTransactionNumberTextBox;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox CA_RX_DeclaredCurrGuidFindBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit ReasonForExportDropEdit;
		private Enterprise.ZArchitecture.ZTextBox CA_TransportDocumentNumberTextBox;
		private Enterprise.ZArchitecture.GUI.ZPanel ImportExtraDetailsZPanel;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox CarrierCodeFindBox;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox SubLocationCodeFindBox;
		private Enterprise.ZArchitecture.GUI.ZDateEdit WarehouseReleaseDateDateEdit;
		private Enterprise.ZArchitecture.GUI.ZDateEdit ReleaseDateDateEdit;
		private Enterprise.ZArchitecture.GUI.ZDateEdit EstReleaseDateDateEdit;
		private Enterprise.ZArchitecture.ZLabel ExceptionDescriptionLabel;
		private Enterprise.ZArchitecture.ZTextBox SubLocationNameTextBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox EDIReleaseOptionsGroupBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit AssesmentOptionDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit ServiceOptionDropEdit;
		public ZArchitecture.GUI.ZCalcDropEdit NetWeightCalcDropEdit;
		private ZArchitecture.ZTextBox CarrierCodeTextBox;
		internal ZPanel VendorAndOriginatorPanel;
		protected MasterFiles.GUI.ZDocAddressControl CommercialInvoiceOriginatorDocAddressControl;
		internal MasterFiles.GUI.ZDocAddressControl ImporterDocumentaryAddress;
		private ZDateEdit ScheduledB3DateEdit;
		protected Enterprise.MasterFiles.GUI.ZDocAddressControl VendorAddressControl;
		private ZDropEdit PriorityIndDropEdit;
		internal ZArchitecture.GUI.ZCheckBox EnableACROSSCheckBox;
		private ZArchitecture.GUI.ZCheckBox EnableB3CheckBox;
		internal Enterprise.MasterFiles.GUI.ZDocAddressControl SupplierDocumentAddress;
		internal MasterFiles.GUI.ZOrgAddressControl ImporterOrgAddressControl;
		internal MasterFiles.GUI.ZOrgAddressControl SupplierOrgAddressControl;
		internal Enterprise.MasterFiles.GUI.ZDocAddressControl ImporterOfRecordDocAddressControl;
		private ZGroupBox groupBoxManualSubmissionInfo;
		public ZArchitecture.GUI.ZDateEdit B3CSubmittedDate;
		public ZArchitecture.GUI.ZDateEdit RELSubmittedDate;
		public ZArchitecture.ZTextBox B3CSubmittedOffice;
		public ZArchitecture.ZTextBox RELSubmittedOffice;
		public ZArchitecture.ZTextBox CargoControlNumberNoBoundTextBox;
		public ZArchitecture.GUI.ZButton DefaultHWBButton;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox ExamLocationCodeFindBox;
		private ZArchitecture.ZTextBox ExamLocationTextBox;
		private ZDateEdit DateOfFirstArrivalDateEdit;
		private ZCodeFindBox PortOfFirstArrivalCodeFindBox;
		public ZArchitecture.ZTextBox CargoControlNumberPrefixNoBoundTextBox;
		public ZArchitecture.ZTextBox CargoControlNumberSuffixNoBoundTextBox;

		#endregion

		private Customs.GUI.LongTextControl DeliveryInstructionsLongTextControl;
	}
}
