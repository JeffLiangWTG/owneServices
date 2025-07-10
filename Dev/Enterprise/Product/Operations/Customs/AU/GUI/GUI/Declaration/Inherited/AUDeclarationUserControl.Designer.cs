using System;
using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class AUDeclarationUserControl
	{
		private void InitializeComponent()
		{
			this.DateOfFirstArrivalDateEdit = new ZDateEdit();
			this.JE_PortOfFirstArrivalCodeFindBox = new ZCodeFindBox();
			this.JE_ExportGoodsTypeBoundDropDownEdit = new ZDropEdit();
			this.marksAndNumbersTextBox = new ZTextBox();
			this.marksAndNumberButton = new ZStmNotePopupButton();
			this.agentReferenceTextBox = new ZTextBox();
			this.jE_ToOrderCheckBox = new ZCheckBox();
			this.statusDetailsButton = new ZButton();
			this.consolidatedCargoStatusTextBox = new ZTextBox();
			this.cargoStatusGroupBox = new ZGroupBox();
			this.drawbackAssessmentMethodDropEdit = new ZDropEdit();
			this.drawbackEDNControl = new EDNFindBox();
			this.custShipNoOverrideCheckBox = new ZCheckBox();
			this.custShipNoTextBox = new ZTextBox();
			this.importerToOrderCityTextBox = new ZTextBox();
			this.importerToOrderCityLabel = new ZLabel();
			this.zFolderBrowserDialog1 = new ZFolderBrowserDialog();
			this.PartShipConsignRefTextBox = new ZTextBox();
			this.PartShipConsignRefAlt1TextBox = new ZTextBox();
			this.PartShipConsignRefAlt2TextBox = new ZTextBox();
			this.declarationMessageAndWHSTransactionStatusPanel = new ZPanel();
			this.messageStatusDetailPanel = new ZPanel();
			this.detailsButton = new ZButton();
			this.messageStatusDescriptionTextBox = new ZTextBox();
			this.wHSTransactionStatusPanel = new ZPanel();
			this.warehouseTransactionStatusDescTextBox = new ZTextBox();
			this.tSS_SeparatorTextUserControl = new SeparatorUserControl();
			this.deliveryDocAddressControl = new MasterFiles.GUI.ZDocAddressControl();
			this.customsProcessingUserControl = new CustomsProcessingUserControl();
			this.ConsolidatedDeclarationAdviceLabel = new Enterprise.ZArchitecture.ZLabel();
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
			this.DateOfFirstArrivalDateEdit.SuspendLayout();
			this.JE_PortOfFirstArrivalCodeFindBox.SuspendLayout();
			this.JE_ExportGoodsTypeBoundDropDownEdit.SuspendLayout();
			this.cargoStatusGroupBox.SuspendLayout();
			this.drawbackAssessmentMethodDropEdit.SuspendLayout();
			this.drawbackEDNControl.SuspendLayout();
			this.declarationMessageAndWHSTransactionStatusPanel.SuspendLayout();
			this.messageStatusDetailPanel.SuspendLayout();
			this.wHSTransactionStatusPanel.SuspendLayout();
			this.tSS_SeparatorTextUserControl.SuspendLayout();
			this.deliveryDocAddressControl.SuspendLayout();
			this.customsProcessingUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// WeightzCalcDropEdit
			// 
			this.WeightzCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 144, true);
			this.WeightzCalcDropEdit.TabIndex = 18;
			// 
			// OwnersReferenceTextBox
			// 
			this.OwnersReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 123, true);
			this.OwnersReferenceTextBox.TabIndex = 16;
			// 
			// ScreeningStatusDropEdit
			// 
			this.ScreeningStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(322, 209, true);
			this.ScreeningStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 17, true);
			this.ScreeningStatusDropEdit.TabIndex = 33;
			// 
			// ScreenButton
			// 
			this.ScreenButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(430, 209, true);
			this.ScreenButton.TabIndex = 34;
			// 
			// TotalNoOfPacksCalcDropEdit
			// 
			this.TotalNoOfPacksCalcDropEdit.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUDeclarationUserControl|a119e5f2-7e37-4f80-ae1a-2a314d04959c", "Total Packages");
			this.TotalNoOfPacksCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 187, true);
			this.TotalNoOfPacksCalcDropEdit.TabIndex = 26;
			// 
			// GoodsDescriptionTextBox
			// 
			this.GoodsDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 80, true);
			// 
			// VesselFindBox
			// 
			this.VesselFindBox.PreBoundMaxLength = 30;
			this.VesselFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(325, 17, true);
			// 
			// PortOfDischargeFindBox
			// 
			this.PortOfDischargeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 110, true);
			// 
			// ShipmentDetailsGroupBox
			// 
			this.ShipmentDetailsGroupBox.Anchor = ((AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left)));
			this.ShipmentDetailsGroupBox.Controls.Add(this.PartShipConsignRefAlt1TextBox);
			this.ShipmentDetailsGroupBox.Controls.Add(this.PartShipConsignRefAlt2TextBox);
			this.ShipmentDetailsGroupBox.Controls.Add(this.PartShipConsignRefTextBox);
			this.ShipmentDetailsGroupBox.Controls.Add(this.drawbackEDNControl);
			this.ShipmentDetailsGroupBox.Controls.Add(this.drawbackAssessmentMethodDropEdit);
			this.ShipmentDetailsGroupBox.Controls.Add(this.agentReferenceTextBox);
			this.ShipmentDetailsGroupBox.Controls.Add(this.marksAndNumberButton);
			this.ShipmentDetailsGroupBox.Controls.Add(this.marksAndNumbersTextBox);
			this.ShipmentDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(255, 288, true);
			this.ShipmentDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(458, 275, true);
			this.ShipmentDetailsGroupBox.TabIndex = 8;
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.ScreeningStatusDropEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.ScreenButton, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.marksAndNumbersTextBox, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.marksAndNumberButton, 0);
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
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.agentReferenceTextBox, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.drawbackAssessmentMethodDropEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.drawbackEDNControl, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.PartShipConsignRefTextBox, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.PartShipConsignRefAlt2TextBox, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.PartShipConsignRefAlt1TextBox, 0);
			// 
			// ShipmentTypeGroupBox
			// 
			this.ShipmentTypeGroupBox.Controls.Add(this.JE_ExportGoodsTypeBoundDropDownEdit);
			this.ShipmentTypeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 304, true);
			this.ShipmentTypeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 208, true);
			this.ShipmentTypeGroupBox.TabIndex = 3;
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.JE_ContainerModeBoundDropDownEdit, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.JE_TransportModeBoundDropDownEdit, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.JE_MessageTypeBoundDropDownEdit, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.JE_MessageSubTypeBoundDropDownEdit, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.JE_RS_NKServiceLevelBoundFindBox, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.JE_ExportGoodsTypeBoundDropDownEdit, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.JE_ApplicationCodeBoundDropEdit, 0);
			// 
			// ImporterOrganisationControl
			//
			this.ImporterOrganisationControl.Captions = new string[] {
		"Importer" };
			this.ImporterOrganisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 152, true);
			// 
			// SupplierOrganisationControl
			//
			this.SupplierOrganisationControl.Captions = new string[] {
		"Main Supplier" };
			this.SupplierOrganisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			// 
			// FinalDestinationFindBox
			// 
			this.FinalDestinationFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 59, true);
			// 
			// JE_DateOfArrivalBoundDateEdit2
			// 
			this.JE_DateOfArrivalBoundDateEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(352, 59, true);
			// 
			// JE_ExportDateBoundDateEdit2
			// 
			this.JE_ExportDateBoundDateEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(352, 37, true);
			// 
			// OriginFindBox
			// 
			this.OriginFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 37, true);
			// 
			// HouseBillParcelPostTextEdit
			// 
			this.HouseBillParcelPostTextEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(323, 17, true);
			// 
			// IncoTermDropEdit
			// 
			this.IncoTermDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 209, true);
			this.IncoTermDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(127, 17, true);
			this.IncoTermDropEdit.TabIndex = 30;
			// 
			// RightTabControl
			// 
			this.RightTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(718, 72, true);
			this.RightTabControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 0, true);
			this.RightTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(284, 490, true);
			this.RightTabControl.TabIndex = 9;
			// 
			// OrganisationsTabPage
			// 
			this.OrganisationsTabPage.Controls.Add(this.deliveryDocAddressControl);
			this.OrganisationsTabPage.Controls.Add(this.tSS_SeparatorTextUserControl);
			this.OrganisationsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.OrganisationsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(279, 468, true);
			this.OrganisationsTabPage.Controls.SetChildIndex(this.OrganisationsTopPanel, 0);
			this.OrganisationsTabPage.Controls.SetChildIndex(this.tSS_SeparatorTextUserControl, 0);
			this.OrganisationsTabPage.Controls.SetChildIndex(this.deliveryDocAddressControl, 0);
			// 
			// OrganisationsTopPanel
			// 
			this.OrganisationsTopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(431, 228, true);
			// 
			// ShippingOrAirLineOrganisationControl
			// 
			this.ShippingOrAirLineOrganisationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(275, 17, true);
			// 
			// ForwarderOrganisationControl
			// 
			this.ForwarderOrganisationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(275, 17, true);
			// 
			// OrdersTabPage
			// 
			this.OrdersTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.OrdersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(279, 468, true);
			// 
			// DocsTabPage
			// 
			this.DocsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.DocsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(279, 468, true);
			// 
			// JE_ContainerCountCalcEdit
			// 
			this.JE_ContainerCountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 165, true);
			this.JE_ContainerCountCalcEdit.TabIndex = 22;
			// 
			// JE_TotalNoOfPiecesBoundCalcEdit
			// 
			this.JE_TotalNoOfPiecesBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 165, true);
			this.JE_TotalNoOfPiecesBoundCalcEdit.TabIndex = 22;
			// 
			// IncoTermExplainButton
			// 
			this.IncoTermExplainButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(242, 209, true);
			this.IncoTermExplainButton.TabIndex = 31;
			// 
			// OverrideValuesCheckBox
			// 
			this.OverrideValuesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(232, -1, true);
			this.OverrideValuesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(207, 16, true);
			// 
			// VolumeCalcDropEdit
			// 
			this.VolumeCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(316, 144, true);
			this.VolumeCalcDropEdit.TabIndex = 20;
			// 
			// ShipmentCustomFieldsPage
			// 
			this.ShipmentCustomFieldsPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.ShipmentCustomFieldsPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(279, 468, true);
			// 
			// shipmentCustomFieldsControl1
			// 
			this.shipmentCustomFieldsControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(279, 468, true);
			// 
			// OrdersPanel
			// 
			this.OrdersPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(279, 468, true);
			// 
			// NumbersTabPage
			// 
			this.NumbersTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.NumbersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(279, 468, true);
			// 
			// OrdersAttachUserControl
			// 
			this.OrdersAttachUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(279, 418, true);
			// 
			// JE_ApplicationCodeBoundDropEdit
			// 
			this.JE_ApplicationCodeBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 172, true);
			this.JE_ApplicationCodeBoundDropEdit.TabIndex = 13;
			// 
			// TransportDetailsGroupBox
			// 
			this.TransportDetailsGroupBox.Controls.Add(this.custShipNoTextBox);
			this.TransportDetailsGroupBox.Controls.Add(this.custShipNoOverrideCheckBox);
			this.TransportDetailsGroupBox.Controls.Add(this.DateOfFirstArrivalDateEdit);
			this.TransportDetailsGroupBox.Controls.Add(this.JE_PortOfFirstArrivalCodeFindBox);
			this.TransportDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(255, 124, true);
			this.TransportDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(458, 163, true);
			this.TransportDetailsGroupBox.TabIndex = 7;
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.OverrideValuesCheckBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.JE_PortOfFirstArrivalCodeFindBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.JE_VoyageFlightNoBoundTextBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.JE_MasterBillForSeaBoundTextBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.VesselFindBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.FolioNumberTextBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.JE_MasterBillForAirBoundTextBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.PortOfLoadingFindBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.PortOfDischargeFindBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.JE_DateOfArrivalBoundDateEdit, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.JE_ExportDateBoundDateEdit, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.DateOfFirstArrivalDateEdit, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.custShipNoOverrideCheckBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.custShipNoTextBox, 0);
			// 
			// DeclarationDetailsGroupBox
			// 
			this.DeclarationDetailsGroupBox.Anchor = ((AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.DeclarationDetailsGroupBox.Controls.Add(this.declarationMessageAndWHSTransactionStatusPanel);
			this.DeclarationDetailsGroupBox.Controls.Add(this.ConsolidatedDeclarationAdviceLabel);
			this.DeclarationDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(255, 0, true);
			this.DeclarationDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(746, 70, true);
			this.DeclarationDetailsGroupBox.TabIndex = 5;
			this.DeclarationDetailsGroupBox.Controls.SetChildIndex(this.ConsolidatedDeclarationAdviceLabel, 0);
			this.DeclarationDetailsGroupBox.Controls.SetChildIndex(this.ExportDeclarationNumberBoundTextBox, 0);
			this.DeclarationDetailsGroupBox.Controls.SetChildIndex(this.StatusTextBox, 0);
			this.DeclarationDetailsGroupBox.Controls.SetChildIndex(this.declarationMessageAndWHSTransactionStatusPanel, 0);
			// 
			// ConsolidatedDeclarationAdviceLabel
			// 
			this.ConsolidatedDeclarationAdviceLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 9, true);
			this.ConsolidatedDeclarationAdviceLabel.Name = "ConsolidatedDeclarationAdviceLabel";
			this.ConsolidatedDeclarationAdviceLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 15, true);
			this.ConsolidatedDeclarationAdviceLabel.TabIndex = 0;
			this.ConsolidatedDeclarationAdviceLabel.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUDeclarationUserControl|47A5E1C4-FB4C-4AC2-B0E2-7FA83528BE92", "This declaration is attached to a consolidated entry.");
			this.ConsolidatedDeclarationAdviceLabel.Visible = false;
			this.ConsolidatedDeclarationAdviceLabel.BackColor = System.Drawing.Color.Tomato;
			// 
			// ExportDeclarationNumberBoundTextBox
			// 
			this.ExportDeclarationNumberBoundTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUDeclarationUserControl|c1eeb997-7fb0-4d2f-8d48-f8bb76786d88", "Entry Number", "The Identifier / Entry Number assigned to a Customs Document which represents a declaration to Customs by a Party concerning either Goods or Persons that may cross the Australian border.");
			this.ExportDeclarationNumberBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 20, true);
			this.ExportDeclarationNumberBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(156, 17, true);
			this.ExportDeclarationNumberBoundTextBox.TabIndex = 1;
			// 
			// StatusTextBox
			// 
			this.StatusTextBox.Anchor = ((AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.StatusTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUDeclarationUserControl|f4f509d5-6346-4cf5-9a67-ab7e791adc6e", "Entry Status", "The Status a Customs Document / Entry which represents a Declaration to Customs.");
			this.StatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(358, 20, true);
			this.StatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(376, 17, true);
			this.StatusTextBox.TabIndex = 2;
			// 
			// customsProcessingUserControl
			// 
			this.BindingSource.SetBindingMember(this.customsProcessingUserControl, ".");
			this.customsProcessingUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 514, true);
			this.customsProcessingUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 44, true);
			this.customsProcessingUserControl.TabIndex = 4;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(JobDeclaration);
			// 
			// DateOfFirstArrivalDateEdit
			// 
			this.DateOfFirstArrivalDateEdit.AllowDrop = true;
			this.DateOfFirstArrivalDateEdit.AutoCompleteMonthThreshold = 1;
			this.DateOfFirstArrivalDateEdit.AutoCompleteYear = true;
			this.DateOfFirstArrivalDateEdit.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.DateOfFirstArrivalDateEdit, "JE_DateOfFirstArrival");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((JobDeclaration)(null)).JE_DateOfFirstArrival)));
			this.DateOfFirstArrivalDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(352, 136, true);
			this.DateOfFirstArrivalDateEdit.Name = "DateOfFirstArrivalDateEdit";
			this.DateOfFirstArrivalDateEdit.TabIndex = 20;
			// 
			// JE_PortOfFirstArrivalCodeFindBox
			// 
			this.JE_PortOfFirstArrivalCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JE_PortOfFirstArrivalCodeFindBox, "JE_RL_NKPortOfFirstArrival");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((JobDeclaration)(null)).JE_RL_NKPortOfFirstArrival)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((JobDeclaration)(null)).Lookups.DestinationList)));
			this.JE_PortOfFirstArrivalCodeFindBox.BindToList = "Lookups.DestinationList";
			this.JE_PortOfFirstArrivalCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 136, true);
			this.JE_PortOfFirstArrivalCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			this.JE_PortOfFirstArrivalCodeFindBox.Name = "JE_PortOfFirstArrivalCodeFindBox";
			this.JE_PortOfFirstArrivalCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.JE_PortOfFirstArrivalCodeFindBox.ParentType = null;
			this.JE_PortOfFirstArrivalCodeFindBox.PreBoundMaxLength = 5;
			this.JE_PortOfFirstArrivalCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 17, true);
			this.JE_PortOfFirstArrivalCodeFindBox.TabIndex = 18;
			// 
			// JE_ExportGoodsTypeBoundDropDownEdit
			// 
			this.JE_ExportGoodsTypeBoundDropDownEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JE_ExportGoodsTypeBoundDropDownEdit, "JE_ExportGoodsType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((JobDeclaration)(null)).JE_ExportGoodsType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((JobDeclaration)(null)).Lookups.JE_ExportGoodsType_List)));
			this.JE_ExportGoodsTypeBoundDropDownEdit.BindToList = "Lookups.JE_ExportGoodsType_List";
			this.JE_ExportGoodsTypeBoundDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 146, true);
			this.JE_ExportGoodsTypeBoundDropDownEdit.Name = "JE_ExportGoodsTypeBoundDropDownEdit";
			this.JE_ExportGoodsTypeBoundDropDownEdit.PreBoundMaxLength = 3;
			this.JE_ExportGoodsTypeBoundDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 17, true);
			this.JE_ExportGoodsTypeBoundDropDownEdit.TabIndex = 11;
			// 
			// MarksAndNumbersTextBox
			// 
			this.BindingSource.SetBindingMember(this.marksAndNumbersTextBox, "JE_MarksAndNumbersShort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((JobDeclaration)(null)).JE_MarksAndNumbersShort)));
			this.marksAndNumbersTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUDeclarationUserControl|a6780ffc-48da-4592-95be-e4fdfc82dcce", "Marks and Numbers");
			this.marksAndNumbersTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.marksAndNumbersTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 101, true);
			this.marksAndNumbersTextBox.Name = "MarksAndNumbersTextBox";
			this.marksAndNumbersTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(256, 17, true);
			this.marksAndNumbersTextBox.TabIndex = 13;
			// 
			// MarksAndNumberButton
			// 
			this.marksAndNumberButton.IsCaptionOverridden = true;
			this.marksAndNumberButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(388, 101, true);
			this.marksAndNumberButton.Name = "MarksAndNumberButton";
			this.marksAndNumberButton.NoteType = "Marks & Numbers";
			this.marksAndNumberButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.marksAndNumberButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 17, true);
			this.marksAndNumberButton.TabIndex = 14;
			this.marksAndNumberButton.Text = "More...";
			this.marksAndNumberButton.ToolTipCaption = null;
			this.marksAndNumberButton.NoteHasChangesChanged += new ZStmNotePopupForm.NoteHasChangesEventHandler(this.MarksAndNumberButton_NoteHasChangesChanged);
			// 
			// AgentReferenceTextBox
			// 
			this.BindingSource.SetBindingMember(this.agentReferenceTextBox, "JE_AgentsReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((JobDeclaration)(null)).JE_AgentsReference)));
			this.agentReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 230, true);
			this.agentReferenceTextBox.Name = "AgentReferenceTextBox";
			this.agentReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(288, 17, true);
			this.agentReferenceTextBox.TabIndex = 35;
			// 
			// JE_ToOrderCheckBox
			// 
			this.BindingSource.SetBindingMember(this.jE_ToOrderCheckBox, "JE_ToOrder");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((JobDeclaration)(null)).JE_ToOrder)));
			this.jE_ToOrderCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.jE_ToOrderCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 192, true);
			this.jE_ToOrderCheckBox.Name = "JE_ToOrderCheckBox";
			this.jE_ToOrderCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(69, 15, true);
			this.jE_ToOrderCheckBox.TabIndex = 2;
			this.jE_ToOrderCheckBox.Text = "To Order";
			this.jE_ToOrderCheckBox.UseVisualStyleBackColor = true;
			// 
			// StatusDetailsButton
			// 
			this.statusDetailsButton.IsCaptionOverridden = true;
			this.statusDetailsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(356, 18, true);
			this.statusDetailsButton.Name = "StatusDetailsButton";
			this.statusDetailsButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.statusDetailsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(44, 20, true);
			this.statusDetailsButton.TabIndex = 2;
			this.statusDetailsButton.Text = "Details";
			this.statusDetailsButton.ToolTipCaption = null;
			this.statusDetailsButton.Click += new EventHandler(this.StatusDetailsButton_Click);
			// 
			// ConsolidatedCargoStatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.consolidatedCargoStatusTextBox, "ConsolidatedCargoStatusDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((JobDeclaration)(null)).ConsolidatedCargoStatusDescription)));
			this.consolidatedCargoStatusTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUDeclarationUserControl|d980e5e8-8fbc-475d-84a6-217c366f6fce", "Summarized Packing Details Status", "The Consolidated Cargo Status as indicated from Customs in response message/s.");
			this.consolidatedCargoStatusTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.consolidatedCargoStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(204, 18, true);
			this.consolidatedCargoStatusTextBox.Name = "ConsolidatedCargoStatusTextBox";
			this.consolidatedCargoStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(148, 17, true);
			this.consolidatedCargoStatusTextBox.TabIndex = 1;
			this.consolidatedCargoStatusTextBox.TextChanged += new EventHandler(this.ConsolidatedCargoStatusTextBox_TextChanged);
			// 
			// CargoStatusGroupBox
			// 
			this.cargoStatusGroupBox.Controls.Add(this.statusDetailsButton);
			this.cargoStatusGroupBox.Controls.Add(this.consolidatedCargoStatusTextBox);
			this.cargoStatusGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(255, 72, true);
			this.cargoStatusGroupBox.Name = "CargoStatusGroupBox";
			this.cargoStatusGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(458, 45, true);
			this.cargoStatusGroupBox.TabIndex = 6;
			this.cargoStatusGroupBox.TabStop = false;
			this.cargoStatusGroupBox.Text = "Consolidated Cargo Status";
			// 
			// DrawbackAssessmentMethodDropEdit
			// 
			this.drawbackAssessmentMethodDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.drawbackAssessmentMethodDropEdit, "AddInfo+ZA_DAM_Hidden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((JobDeclaration)(null)).AddInfo.ZA_DAM_Hidden)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((JobDeclaration)(null)).AddInfo.Lookups.ZA_DAM_List)));
			this.drawbackAssessmentMethodDropEdit.BindToList = "AddInfo+Lookups+ZA_DAM_List";
			this.drawbackAssessmentMethodDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(321, 165, true);
			this.drawbackAssessmentMethodDropEdit.Name = "DrawbackAssessmentMethodDropEdit";
			this.drawbackAssessmentMethodDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(125, 17, true);
			this.drawbackAssessmentMethodDropEdit.TabIndex = 24;
			// 
			// DrawbackEDNControl
			// 
			this.drawbackEDNControl.AllowDrop = true;
			this.drawbackEDNControl.AutoValidate = System.Windows.Forms.AutoValidate.EnableAllowFocusChange;
			this.BindingSource.SetBindingMember(this.drawbackEDNControl, "AddInfo+ZA_EDN_Hidden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((JobDeclaration)(null)).AddInfo.ZA_EDN_Hidden)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((JobDeclaration)(null)).Lookups.ExportDeclarations)));
			this.drawbackEDNControl.BindToList = "Lookups+ExportDeclarations";
			this.drawbackEDNControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(320, 186, true);
			this.drawbackEDNControl.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.JobDeclaration;
			this.drawbackEDNControl.Name = "DrawbackEDNControl";
			this.drawbackEDNControl.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.drawbackEDNControl.ParentType = null;
			this.drawbackEDNControl.ShowDescriptionBox = false;
			this.drawbackEDNControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 17, true);
			this.drawbackEDNControl.TabIndex = 28;
			// 
			// CustShipNoOverrideCheckBox
			// 
			this.BindingSource.SetBindingMember(this.custShipNoOverrideCheckBox, "ZA_CustShipNoOverride_Hidden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((JobDeclaration)(null)).ZA_CustShipNoOverride_Hidden)));
			this.custShipNoOverrideCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.custShipNoOverrideCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(349, 67, true);
			this.custShipNoOverrideCheckBox.Name = "CustShipNoOverrideCheckBox";
			this.custShipNoOverrideCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 15, true);
			this.custShipNoOverrideCheckBox.TabIndex = 9;
			this.custShipNoOverrideCheckBox.Text = "O/R Lloyds";
			this.custShipNoOverrideCheckBox.UseVisualStyleBackColor = true;
			// 
			// CustShipNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.custShipNoTextBox, "ZA_CustShipNo_Hidden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((JobDeclaration)(null)).ZA_CustShipNo_Hidden)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.custShipNoTextBox, false);
			this.custShipNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(344, 40, true);
			this.custShipNoTextBox.Name = "CustShipNoTextBox";
			this.custShipNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(93, 17, true);
			this.custShipNoTextBox.TabIndex = 6;
			// 
			// ImporterToOrderCityTextBox
			// 
			this.BindingSource.SetBindingMember(this.importerToOrderCityTextBox, "JE_ToOrderComment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((JobDeclaration)(null)).JE_ToOrderComment)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.importerToOrderCityTextBox, false);
			this.importerToOrderCityTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 235, true);
			this.importerToOrderCityTextBox.Name = "ImporterToOrderCityTextBox";
			this.importerToOrderCityTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 17, true);
			this.importerToOrderCityTextBox.TabIndex = 9;
			// 
			// ImporterToOrderCityLabel
			// 
			this.importerToOrderCityLabel.AutoSize = true;
			this.importerToOrderCityLabel.FontType = ((ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.importerToOrderCityLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(47, 237, true);
			this.importerToOrderCityLabel.Name = "ImporterToOrderCityLabel";
			this.importerToOrderCityLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(26, 13, true);
			this.importerToOrderCityLabel.TabIndex = 34;
			this.importerToOrderCityLabel.Text = "City:";
			// 
			// zFolderBrowserDialog1
			// 
			this.zFolderBrowserDialog1.CreateDirectory = false;
			this.zFolderBrowserDialog1.Description = "";
			this.zFolderBrowserDialog1.RequireMappablePath = false;
			this.zFolderBrowserDialog1.RootFolder = System.Environment.SpecialFolder.Desktop;
			this.zFolderBrowserDialog1.ShowNewFolderButton = true;
			// 
			// PartShipConsignRefTextBox
			// 
			this.BindingSource.SetBindingMember(this.PartShipConsignRefTextBox, "JE_PartShipConsignmentReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((JobDeclaration)(null)).JE_PartShipConsignmentReference)));
			this.PartShipConsignRefTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUDeclarationUserControl|4BDCC9FE-4782-44AE-842A-B03661A2E45D", "Consign. Ref. No.");
			this.PartShipConsignRefTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 37, true);
			this.PartShipConsignRefTextBox.Name = "PartShipConsignRefTextBox";
			this.PartShipConsignRefTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(324, 17, true);
			this.PartShipConsignRefTextBox.TabIndex = 2;
			// 
			// PartShipConsignRefAlt1TextBox
			// 
			this.BindingSource.SetBindingMember(this.PartShipConsignRefAlt1TextBox, "JE_PartShipConsignmentReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((JobDeclaration)(null)).JE_PartShipConsignmentReference)));
			this.PartShipConsignRefAlt1TextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUDeclarationUserControl|ff8e6989-9074-4f7f-b666-e206dc3beb00", "Consign. Ref. No.");
			this.PartShipConsignRefAlt1TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 101, true);
			this.PartShipConsignRefAlt1TextBox.Name = "PartShipConsignRefAlt1TextBox";
			this.PartShipConsignRefAlt1TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(324, 17, true);
			this.PartShipConsignRefAlt1TextBox.TabIndex = 13;
			// 
			// PartShipConsignRefAlt2TextBox
			// 
			this.BindingSource.SetBindingMember(this.PartShipConsignRefAlt2TextBox, "JE_PartShipConsignmentReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((JobDeclaration)(null)).JE_PartShipConsignmentReference)));
			this.PartShipConsignRefAlt2TextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUDeclarationUserControl|4BDCC9FE-4782-44AE-842A-B03661A2E45D", "Consign. Ref. No.");
			this.PartShipConsignRefAlt2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 251, true);
			this.PartShipConsignRefAlt2TextBox.Name = "PartShipConsignRefAlt2TextBox";
			this.PartShipConsignRefAlt2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(324, 17, true);
			this.PartShipConsignRefAlt2TextBox.TabIndex = 36;
			// 
			// DeclarationMessageAndWHSTransactionStatusPanel
			// 
			this.declarationMessageAndWHSTransactionStatusPanel.Controls.Add(this.messageStatusDetailPanel);
			this.declarationMessageAndWHSTransactionStatusPanel.Controls.Add(this.wHSTransactionStatusPanel);
			this.declarationMessageAndWHSTransactionStatusPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.declarationMessageAndWHSTransactionStatusPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 40, true);
			this.declarationMessageAndWHSTransactionStatusPanel.Name = "DeclarationMessageAndWHSTransactionStatusPanel";
			this.declarationMessageAndWHSTransactionStatusPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(740, 22, true);
			this.declarationMessageAndWHSTransactionStatusPanel.TabIndex = 3;
			// 
			// MessageStatusDetailPanel
			// 
			this.messageStatusDetailPanel.Controls.Add(this.detailsButton);
			this.messageStatusDetailPanel.Controls.Add(this.messageStatusDescriptionTextBox);
			this.messageStatusDetailPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.messageStatusDetailPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.messageStatusDetailPanel.Name = "MessageStatusDetailPanel";
			this.messageStatusDetailPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(469, 20, true);
			this.messageStatusDetailPanel.TabIndex = 0;
			// 
			// DetailsButton
			// 
			this.detailsButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.detailsButton.IsCaptionOverridden = true;
			this.detailsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(415, 0, true);
			this.detailsButton.Name = "DetailsButton";
			this.detailsButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.detailsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(44, 20, true);
			this.detailsButton.TabIndex = 1;
			this.detailsButton.Text = "Details";
			this.detailsButton.ToolTipCaption = null;
			this.detailsButton.Click += new EventHandler(this.DetailsButton_Click);
			// 
			// MessageStatusDescriptionTextBox
			// 
			this.messageStatusDescriptionTextBox.Anchor = ((AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.messageStatusDescriptionTextBox, "JE_MessageStatusDescriptionIncludingOustandingAmendments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((JobDeclaration)(null)).JE_MessageStatusDescriptionIncludingOustandingAmendments)));
			this.messageStatusDescriptionTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUDeclarationUserControl|ae7493e5-541d-4bbc-a1a4-b333c150f9d2", "Message Status", "The Status of the latest Message referenced as a  Declaration to Customs. For full message/s details refer to the Entries tab.");
			this.messageStatusDescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.messageStatusDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 0, true);
			this.messageStatusDescriptionTextBox.Name = "MessageStatusDescriptionTextBox";
			this.messageStatusDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(317, 17, true);
			this.messageStatusDescriptionTextBox.TabIndex = 0;
			this.messageStatusDescriptionTextBox.TextChanged += new EventHandler(this.MessageStatusDescriptionTextBox_TextChanged);
			// 
			// WHSTransactionStatusPanel
			// 
			this.wHSTransactionStatusPanel.Controls.Add(this.warehouseTransactionStatusDescTextBox);
			this.wHSTransactionStatusPanel.Dock = System.Windows.Forms.DockStyle.Right;
			this.wHSTransactionStatusPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(469, 0, true);
			this.wHSTransactionStatusPanel.Name = "WHSTransactionStatusPanel";
			this.wHSTransactionStatusPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(273, 20, true);
			this.wHSTransactionStatusPanel.TabIndex = 1;
			// 
			// WarehouseTransactionStatusDescTextBox
			// 
			this.BindingSource.SetBindingMember(this.warehouseTransactionStatusDescTextBox, "WarehouseTransactionStatusDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((JobDeclaration)(null)).WarehouseTransactionStatusDescription)));
			this.warehouseTransactionStatusDescTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUDeclarationUserControl|899ce488-4c4d-457e-ad71-4ad33c1eed90", "WHS Status");
			this.warehouseTransactionStatusDescTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(91, 0, true);
			this.warehouseTransactionStatusDescTextBox.Name = "WarehouseTransactionStatusDescTextBox";
			this.warehouseTransactionStatusDescTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(173, 17, true);
			this.warehouseTransactionStatusDescTextBox.TabIndex = 0;
			// 
			// TSS_SeparatorTextUserControl
			// 
			this.tSS_SeparatorTextUserControl.AllowDrop = true;
			this.tSS_SeparatorTextUserControl.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUDeclarationUserControl|a2293e66-b688-4759-9c88-d27f85d231b6", "Delivery Address");
			this.tSS_SeparatorTextUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 231, true);
			this.tSS_SeparatorTextUserControl.Name = "TSS_SeparatorTextUserControl";
			this.tSS_SeparatorTextUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(428, 15, true);
			this.tSS_SeparatorTextUserControl.TabIndex = 1;
			// 
			// DeliveryDocAddressControl
			// 
			this.deliveryDocAddressControl.AddressValidationProcessCmdKey = null;
			this.deliveryDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.deliveryDocAddressControl, "ImporterDeliveryAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((MasterFiles.Business.JobDocAddress)(((JobDeclaration)(null)).ImporterDeliveryAddress)));
			this.deliveryDocAddressControl.BindToOrganisations = "Lookups+ImportersList";
			this.deliveryDocAddressControl.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUDeclarationUserControl|ff32aad3-8796-48d6-960b-1b5b2ae85f10", "Delivery Address");
			this.deliveryDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 252, true);
			this.deliveryDocAddressControl.Name = "DeliveryDocAddressControl";
			this.deliveryDocAddressControl.ReadOnly = false;
			this.deliveryDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.deliveryDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.deliveryDocAddressControl.TabIndex = 2;
			this.deliveryDocAddressControl.ValidationJustForced = false;
			// 
			// AUDeclarationUserControl
			// 
			this.Controls.Add(this.importerToOrderCityLabel);
			this.Controls.Add(this.importerToOrderCityTextBox);
			this.Controls.Add(this.cargoStatusGroupBox);
			this.Controls.Add(this.jE_ToOrderCheckBox);
			this.Controls.Add(this.customsProcessingUserControl);
			this.Name = "AUDeclarationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 563, true);
			this.Controls.SetChildIndex(this.RequiresMergeLabel, 0);
			this.Controls.SetChildIndex(this.ImporterOrganisationControl, 0);
			this.Controls.SetChildIndex(this.jE_ToOrderCheckBox, 0);
			this.Controls.SetChildIndex(this.SupplierOrganisationControl, 0);
			this.Controls.SetChildIndex(this.customsProcessingUserControl, 0);
			this.Controls.SetChildIndex(this.ShipmentTypeGroupBox, 0);
			this.Controls.SetChildIndex(this.ShipmentDetailsGroupBox, 0);
			this.Controls.SetChildIndex(this.TransportDetailsGroupBox, 0);
			this.Controls.SetChildIndex(this.DeclarationDetailsGroupBox, 0);
			this.Controls.SetChildIndex(this.RightTabControl, 0);
			this.Controls.SetChildIndex(this.cargoStatusGroupBox, 0);
			this.Controls.SetChildIndex(this.importerToOrderCityTextBox, 0);
			this.Controls.SetChildIndex(this.importerToOrderCityLabel, 0);
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
			this.customsProcessingUserControl.ResumeLayout(true);
			this.customsProcessingUserControl.PerformLayout();
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
			this.DateOfFirstArrivalDateEdit.ResumeLayout(true);
			this.DateOfFirstArrivalDateEdit.PerformLayout();
			this.JE_PortOfFirstArrivalCodeFindBox.ResumeLayout(true);
			this.JE_PortOfFirstArrivalCodeFindBox.PerformLayout();
			this.JE_ExportGoodsTypeBoundDropDownEdit.ResumeLayout(true);
			this.JE_ExportGoodsTypeBoundDropDownEdit.PerformLayout();
			this.cargoStatusGroupBox.ResumeLayout(false);
			this.cargoStatusGroupBox.PerformLayout();
			this.drawbackAssessmentMethodDropEdit.ResumeLayout(true);
			this.drawbackAssessmentMethodDropEdit.PerformLayout();
			this.drawbackEDNControl.ResumeLayout(true);
			this.drawbackEDNControl.PerformLayout();
			this.declarationMessageAndWHSTransactionStatusPanel.ResumeLayout(false);
			this.declarationMessageAndWHSTransactionStatusPanel.PerformLayout();
			this.messageStatusDetailPanel.ResumeLayout(false);
			this.messageStatusDetailPanel.PerformLayout();
			this.wHSTransactionStatusPanel.ResumeLayout(false);
			this.wHSTransactionStatusPanel.PerformLayout();
			this.tSS_SeparatorTextUserControl.ResumeLayout(true);
			this.tSS_SeparatorTextUserControl.PerformLayout();
			this.deliveryDocAddressControl.ResumeLayout(true);
			this.deliveryDocAddressControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		protected internal ZCodeFindBox JE_PortOfFirstArrivalCodeFindBox;
		protected internal ZDropEdit JE_ExportGoodsTypeBoundDropDownEdit;
		internal ZTextBox marksAndNumbersTextBox;
		internal ZStmNotePopupButton marksAndNumberButton;
		protected internal ZDateEdit DateOfFirstArrivalDateEdit;
		internal ZTextBox agentReferenceTextBox;
		private ZCheckBox jE_ToOrderCheckBox;
		private ZButton statusDetailsButton;
		internal ZTextBox consolidatedCargoStatusTextBox;
		internal ZGroupBox cargoStatusGroupBox;
		internal ZDropEdit drawbackAssessmentMethodDropEdit;
		internal EDNFindBox drawbackEDNControl;
		internal ZTextBox custShipNoTextBox;
		internal ZCheckBox custShipNoOverrideCheckBox;
		private ZTextBox importerToOrderCityTextBox;
		private ZLabel importerToOrderCityLabel;
		private ZFolderBrowserDialog zFolderBrowserDialog1;
		internal ZTextBox PartShipConsignRefTextBox;
		internal ZTextBox PartShipConsignRefAlt1TextBox;
		internal ZTextBox PartShipConsignRefAlt2TextBox;
		internal ZPanel declarationMessageAndWHSTransactionStatusPanel;
		internal ZPanel messageStatusDetailPanel;
		internal ZButton detailsButton;
		internal ZTextBox messageStatusDescriptionTextBox;
		private ZPanel wHSTransactionStatusPanel;
		private ZTextBox warehouseTransactionStatusDescTextBox;
		private SeparatorUserControl tSS_SeparatorTextUserControl;
		private MasterFiles.GUI.ZDocAddressControl deliveryDocAddressControl;
		internal CustomsProcessingUserControl customsProcessingUserControl;
		protected internal ZArchitecture.ZLabel ConsolidatedDeclarationAdviceLabel;
	}
}
