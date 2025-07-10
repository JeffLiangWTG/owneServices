using Enterprise.MasterFiles.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class EUJobDeclarationUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		System.ComponentModel.IContainer components = null;

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
		void InitializeComponent()
		{
			this.GoodsLocationDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AgentsReference = new Enterprise.ZArchitecture.ZTextBox();
			this.CTStatusIDDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.InlandModeOfTransportDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DepartureTransportIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TransportNationalityFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.EarliestCustomsIssueDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.BadgeCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.GatewayDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.SpecificCircumstanceDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ImporterDocAddress = GetImporterDocAddressControl();
			this.SupplierDocAddress = GetSupplierDocAddressControl();
			this.StyleOfEntrySOEDropDown = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ZG_AgreedPlaceCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.JE_ShipmentIncoTermPlaceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.Box18TransportIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.Box18TrNationalityFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.AircraftRegistrationNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JE_IATALoadPortCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.PortOfFirstArrivalFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.JE_DateOfFirstArrivalBoundDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JE_UCRTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OrganisationsOuterPanel = new CargoWise.Windows.UI.KPanel();
			this.OrganisationsPanel = new CargoWise.Windows.UI.KPanel();
			this.OrganizationExportUserControl = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			this.OrganizationImportUserControl = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			this.CustomsOfficesUserControl = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			this.GoodsDestinationDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.GoodsOriginDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.IsHighValueOvrdCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.InlandTransportCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RegionOfDestinationDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
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
			this.GoodsLocationDropEdit.SuspendLayout();
			this.CTStatusIDDropEdit.SuspendLayout();
			this.InlandModeOfTransportDropEdit.SuspendLayout();
			this.TransportNationalityFindBox.SuspendLayout();
			this.EarliestCustomsIssueDateEdit.SuspendLayout();
			this.BadgeCodeDropEdit.SuspendLayout();
			this.GatewayDropEdit.SuspendLayout();
			this.SpecificCircumstanceDropEdit.SuspendLayout();
			this.ImporterDocAddress.SuspendLayout();
			this.SupplierDocAddress.SuspendLayout();
			this.StyleOfEntrySOEDropDown.SuspendLayout();
			this.ZG_AgreedPlaceCodeDropEdit.SuspendLayout();
			this.Box18TrNationalityFindBox.SuspendLayout();
			this.JE_IATALoadPortCodeFindBox.SuspendLayout();
			this.PortOfFirstArrivalFindBox.SuspendLayout();
			this.JE_DateOfFirstArrivalBoundDateEdit.SuspendLayout();
			this.OrganisationsOuterPanel.SuspendLayout();
			this.OrganisationsPanel.SuspendLayout();
			this.GoodsDestinationDropEdit.SuspendLayout();
			this.GoodsOriginDropEdit.SuspendLayout();
			this.InlandTransportCodeDropEdit.SuspendLayout();
			this.RegionOfDestinationDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// JE_MessageTypeBoundDropDownEdit
			// 
			this.JE_MessageTypeBoundDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 11, true);
			this.JE_MessageTypeBoundDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(145, 20, true);
			this.JE_MessageTypeBoundDropDownEdit.TabIndex = 0;
			// 
			// JE_TransportModeBoundDropDownEdit
			// 
			this.JE_TransportModeBoundDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 82, true);
			this.JE_TransportModeBoundDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(145, 20, true);
			this.JE_TransportModeBoundDropDownEdit.TabIndex = 3;
			// 
			// JE_ContainerModeBoundDropDownEdit
			// 
			this.JE_ContainerModeBoundDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 104, true);
			this.JE_ContainerModeBoundDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(145, 20, true);
			this.JE_ContainerModeBoundDropDownEdit.TabIndex = 4;
			this.JE_ContainerModeBoundDropDownEdit.Visible = false;
			// 
			// JE_MessageSubTypeBoundDropDownEdit
			// 
			this.BindingSource.SetBindingMember(this.JE_MessageSubTypeBoundDropDownEdit, "JE_EntryStyle");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).JE_EntryStyle)));
			this.JE_MessageSubTypeBoundDropDownEdit.BindToList = "";
			this.JE_MessageSubTypeBoundDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 33, true);
			this.JE_MessageSubTypeBoundDropDownEdit.PreBoundMaxLength = 2;
			this.JE_MessageSubTypeBoundDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(145, 20, true);
			this.JE_MessageSubTypeBoundDropDownEdit.TabIndex = 1;
			// 
			// WeightzCalcDropEdit
			// 
			this.WeightzCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 161, true);
			this.WeightzCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 20, true);
			this.WeightzCalcDropEdit.TabIndex = 12;
			// 
			// OwnersReferenceTextBox
			// 
			this.OwnersReferenceTextBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("EUJobDeclarationUserControl|b73d8385-00ff-4750-833b-7da7828db88e", "[7] Declarant\'s Ref");
			this.OwnersReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 137, true);
			this.OwnersReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 20, true);
			this.OwnersReferenceTextBox.TabIndex = 10;
			// 
			// ScreeningStatusDropEdit
			// 
			this.ScreeningStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 302, true);
			this.ScreeningStatusDropEdit.TabIndex = 26;
			// 
			// ScreenButton
			// 
			this.ScreenButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(221, 302, true);
			this.ScreenButton.TabIndex = 27;
			// 
			// TotalNoOfPacksCalcDropEdit
			// 
			this.TotalNoOfPacksCalcDropEdit.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("EUJobDeclarationUserControl|a119e5f2-7e37-4f80-ae1a-2a314d04959c", "[6] No. Pkgs.");
			this.TotalNoOfPacksCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(346, 139, true);
			this.TotalNoOfPacksCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 20, true);
			this.TotalNoOfPacksCalcDropEdit.TabIndex = 11;
			// 
			// GoodsDescriptionTextBox
			// 
			this.GoodsDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 88, true);
			this.GoodsDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(339, 20, true);
			this.GoodsDescriptionTextBox.TabIndex = 8;
			// 
			// JE_MasterBillForAirBoundTextBox
			// 
			this.JE_MasterBillForAirBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(93, 16, true);
			this.JE_MasterBillForAirBoundTextBox.TabIndex = 0;
			// 
			// FolioNumberTextBox
			// 
			this.FolioNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(431, 17, true);
			this.FolioNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(41, 20, true);
			this.FolioNumberTextBox.TabIndex = 15;
			this.FolioNumberTextBox.Visible = false;
			// 
			// VesselFindBox
			// 
			this.VesselFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 40, true);
			this.VesselFindBox.ShowDescriptionBox = false;
			this.VesselFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 20, true);
			this.VesselFindBox.TabIndex = 2;
			// 
			// JE_MasterBillForSeaBoundTextBox
			// 
			this.JE_MasterBillForSeaBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 16, true);
			this.JE_MasterBillForSeaBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(123, 20, true);
			this.JE_MasterBillForSeaBoundTextBox.TabIndex = 1;
			// 
			// JE_VoyageFlightNoBoundTextBox
			// 
			this.JE_VoyageFlightNoBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 64, true);
			this.JE_VoyageFlightNoBoundTextBox.TabIndex = 3;
			// 
			// JE_ExportDateBoundDateEdit
			// 
			this.JE_ExportDateBoundDateEdit.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("7c759cea-ace3-48a7-9dbb-ebe427d694b0", "Dep.");
			this.JE_ExportDateBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(258, 88, true);
			this.JE_ExportDateBoundDateEdit.TabIndex = 8;
			// 
			// JE_DateOfArrivalBoundDateEdit
			// 
			this.JE_DateOfArrivalBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(258, 133, true);
			this.JE_DateOfArrivalBoundDateEdit.TabIndex = 13;
			// 
			// PortOfDischargeFindBox
			// 
			this.PortOfDischargeFindBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("eedabf34-7646-4dc7-bddd-2d2c0920c1f9", "Discharge", "Discharge Port", "");
			this.PortOfDischargeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 133, true);
			this.PortOfDischargeFindBox.ShowDescriptionBox = false;
			this.PortOfDischargeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			this.PortOfDischargeFindBox.TabIndex = 12;
			// 
			// PortOfLoadingFindBox
			// 
			this.PortOfLoadingFindBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("b9c8930c-0e63-4e55-800d-9e5575ebb34e", "[27] Load Port");
			this.PortOfLoadingFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 88, true);
			this.PortOfLoadingFindBox.ShowDescriptionBox = false;
			this.PortOfLoadingFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			this.PortOfLoadingFindBox.TabIndex = 7;
			// 
			// ShipmentDetailsGroupBox
			// 
			this.ShipmentDetailsGroupBox.Controls.Add(this.JE_UCRTextBox);
			this.ShipmentDetailsGroupBox.Controls.Add(this.JE_ShipmentIncoTermPlaceTextBox);
			this.ShipmentDetailsGroupBox.Controls.Add(this.ZG_AgreedPlaceCodeDropEdit);
			this.ShipmentDetailsGroupBox.Controls.Add(this.GoodsDestinationDropEdit);
			this.ShipmentDetailsGroupBox.Controls.Add(this.GoodsOriginDropEdit);
			this.ShipmentDetailsGroupBox.Controls.Add(this.GatewayDropEdit);
			this.ShipmentDetailsGroupBox.Controls.Add(this.BadgeCodeDropEdit);
			this.ShipmentDetailsGroupBox.Controls.Add(this.GoodsLocationDropEdit);
			this.ShipmentDetailsGroupBox.Controls.Add(this.AgentsReference);
			this.ShipmentDetailsGroupBox.Controls.Add(this.RegionOfDestinationDropEdit);
			this.ShipmentDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(255, 317, true);
			this.ShipmentDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(464, 334, true);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.ScreeningStatusDropEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.ScreenButton, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.AgentsReference, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.GoodsLocationDropEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.BadgeCodeDropEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.GatewayDropEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.JE_TotalNoOfPiecesBoundCalcEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.TotalNoOfPacksCalcDropEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.JE_ContainerCountCalcEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.IncoTermExplainButton, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.IncoTermDropEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.GoodsDescriptionTextBox, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.OwnersReferenceTextBox, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.WeightzCalcDropEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.VolumeCalcDropEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.HouseBillParcelPostTextEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.OriginFindBox, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.GoodsOriginDropEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.JE_ExportDateBoundDateEdit2, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.JE_DateOfArrivalBoundDateEdit2, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.FinalDestinationFindBox, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.GoodsDestinationDropEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.ZG_AgreedPlaceCodeDropEdit, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.JE_ShipmentIncoTermPlaceTextBox, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.JE_UCRTextBox, 0);
			this.ShipmentDetailsGroupBox.Controls.SetChildIndex(this.RegionOfDestinationDropEdit, 0);
			// 
			// ShipmentTypeGroupBox
			// 
			this.ShipmentTypeGroupBox.Controls.Add(this.IsHighValueOvrdCheckBox);
			this.ShipmentTypeGroupBox.Controls.Add(this.SpecificCircumstanceDropEdit);
			this.ShipmentTypeGroupBox.Controls.Add(this.CTStatusIDDropEdit);
			this.ShipmentTypeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 380, true);
			this.ShipmentTypeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 261, true);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.CTStatusIDDropEdit, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.SpecificCircumstanceDropEdit, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.JE_ApplicationCodeBoundDropEdit, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.JE_ContainerModeBoundDropDownEdit, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.JE_TransportModeBoundDropDownEdit, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.JE_MessageTypeBoundDropDownEdit, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.JE_MessageSubTypeBoundDropDownEdit, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.JE_RS_NKServiceLevelBoundFindBox, 0);
			this.ShipmentTypeGroupBox.Controls.SetChildIndex(this.IsHighValueOvrdCheckBox, 0);
			// 
			// ImporterOrganisationControl
			// 
			this.ImporterOrganisationControl.IsCaptionOverridden = true;
			this.ImporterOrganisationControl.TabIndex = 2;
			// 
			// SupplierOrganisationControl
			// 
			this.SupplierOrganisationControl.IsCaptionOverridden = true;
			// 
			// FinalDestinationFindBox
			// 
			this.FinalDestinationFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(93, 64, true);
			this.FinalDestinationFindBox.TabIndex = 5;
			// 
			// JE_DateOfArrivalBoundDateEdit2
			// 
			this.JE_DateOfArrivalBoundDateEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(371, 64, true);
			this.JE_DateOfArrivalBoundDateEdit2.TabIndex = 7;
			// 
			// JE_ExportDateBoundDateEdit2
			// 
			this.JE_ExportDateBoundDateEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(371, 40, true);
			this.JE_ExportDateBoundDateEdit2.TabIndex = 4;
			// 
			// OriginFindBox
			// 
			this.OriginFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(93, 40, true);
			this.OriginFindBox.TabIndex = 2;
			// 
			// HouseBillParcelPostTextEdit
			// 
			this.HouseBillParcelPostTextEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(93, 16, true);
			this.HouseBillParcelPostTextEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 20, true);
			this.HouseBillParcelPostTextEdit.TabIndex = 0;
			// 
			// IncoTermDropEdit
			// 
			this.IncoTermDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(297, 161, true);
			this.IncoTermDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 20, true);
			this.IncoTermDropEdit.TabIndex = 13;
			// 
			// RightTabControl
			// 
			this.RightTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(329, 730, true);
			// 
			// OrganisationsTabPage
			// 
			this.OrganisationsTabPage.Controls.Add(this.OrganisationsOuterPanel);
			this.OrganisationsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(321, 703, true);
			this.OrganisationsTabPage.Controls.SetChildIndex(this.OrganisationsTopPanel, 0);
			this.OrganisationsTabPage.Controls.SetChildIndex(this.OrganisationsOuterPanel, 0);
			// 
			// OrganisationsTopPanel
			// 
			this.OrganisationsTopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(321, 206, true);
			// 
			// OrdersTabPage
			// 
			this.OrdersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(321, 703, true);
			// 
			// JE_RS_NKServiceLevelBoundFindBox
			// 
			this.JE_RS_NKServiceLevelBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 126, true);
			this.JE_RS_NKServiceLevelBoundFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(145, 20, true);
			this.JE_RS_NKServiceLevelBoundFindBox.TabIndex = 5;
			// 
			// DocsTabPage
			// 
			this.DocsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(321, 703, true);
			// 
			// JE_ContainerCountCalcEdit
			// 
			this.JE_ContainerCountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(405, 205, true);
			this.JE_ContainerCountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(51, 20, true);
			this.JE_ContainerCountCalcEdit.TabIndex = 21;
			this.JE_ContainerCountCalcEdit.Visible = false;
			// 
			// JE_TotalNoOfPiecesBoundCalcEdit
			// 
			this.JE_TotalNoOfPiecesBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(376, 205, true);
			this.JE_TotalNoOfPiecesBoundCalcEdit.TabIndex = 20;
			// 
			// IncoTermExplainButton
			// 
			this.IncoTermExplainButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(434, 159, true);
			this.IncoTermExplainButton.TabIndex = 14;
			// 
			// OverrideValuesCheckBox
			// 
			this.OverrideValuesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(232, 0, true);
			this.OverrideValuesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 17, true);
			this.OverrideValuesCheckBox.TabIndex = 2;
			// 
			// VolumeCalcDropEdit
			// 
			this.VolumeCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 184, true);
			this.VolumeCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 20, true);
			this.VolumeCalcDropEdit.TabIndex = 15;
			// 
			// ShipmentCustomFieldsPage
			// 
			this.ShipmentCustomFieldsPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(321, 703, true);
			// 
			// shipmentCustomFieldsControl1
			// 
			this.shipmentCustomFieldsControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(321, 703, true);
			// 
			// OrdersPanel
			// 
			this.OrdersPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(321, 703, true);
			// 
			// BondedWarehouseDocAddressControl
			// 
			this.BondedWarehouseDocAddressControl.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("EUJobDeclarationUserControl|7f792b4d-3ee5-4f7e-8901-1da0decaf1f6", "[49] Customs Warehouse");
			// 
			// NumbersTabPage
			// 
			this.NumbersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(321, 703, true);
			// 
			// OrdersAttachUserControl
			// 
			this.OrdersAttachUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(321, 418, true);
			// 
			// JE_ApplicationCodeBoundDropEdit
			// 
			this.JE_ApplicationCodeBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 170, true);
			this.JE_ApplicationCodeBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(145, 20, true);
			this.JE_ApplicationCodeBoundDropEdit.TabIndex = 7;
			// 
			// TransportDetailsGroupBox
			// 
			this.TransportDetailsGroupBox.Controls.Add(this.InlandTransportCodeDropEdit);
			this.TransportDetailsGroupBox.Controls.Add(this.JE_DateOfFirstArrivalBoundDateEdit);
			this.TransportDetailsGroupBox.Controls.Add(this.PortOfFirstArrivalFindBox);
			this.TransportDetailsGroupBox.Controls.Add(this.JE_IATALoadPortCodeFindBox);
			this.TransportDetailsGroupBox.Controls.Add(this.Box18TransportIDTextBox);
			this.TransportDetailsGroupBox.Controls.Add(this.Box18TrNationalityFindBox);
			this.TransportDetailsGroupBox.Controls.Add(this.TransportNationalityFindBox);
			this.TransportDetailsGroupBox.Controls.Add(this.DepartureTransportIDTextBox);
			this.TransportDetailsGroupBox.Controls.Add(this.InlandModeOfTransportDropEdit);
			this.TransportDetailsGroupBox.Controls.Add(this.AircraftRegistrationNumberTextBox);
			this.TransportDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(255, 55, true);
			this.TransportDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(464, 260, true);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.AircraftRegistrationNumberTextBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.PortOfLoadingFindBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.PortOfDischargeFindBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.InlandModeOfTransportDropEdit, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.DepartureTransportIDTextBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.JE_VoyageFlightNoBoundTextBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.JE_MasterBillForSeaBoundTextBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.VesselFindBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.FolioNumberTextBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.JE_MasterBillForAirBoundTextBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.JE_DateOfArrivalBoundDateEdit, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.JE_ExportDateBoundDateEdit, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.OverrideValuesCheckBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.TransportNationalityFindBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.Box18TrNationalityFindBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.Box18TransportIDTextBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.JE_IATALoadPortCodeFindBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.PortOfFirstArrivalFindBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.JE_DateOfFirstArrivalBoundDateEdit, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.InlandTransportCodeDropEdit, 0);
			// 
			// ExportDeclarationNumberBoundTextBox
			//
			this.ExportDeclarationNumberBoundTextBox.CaptionResourceString = null;
			this.ExportDeclarationNumberBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 17, true);
			this.ExportDeclarationNumberBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(129, 20, true);
			// 
			// DeclarationDetailsGroupBox
			// 
			this.DeclarationDetailsGroupBox.Controls.Add(this.StyleOfEntrySOEDropDown);
			this.DeclarationDetailsGroupBox.Controls.Add(this.EarliestCustomsIssueDateEdit);
			this.DeclarationDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(255, 8, true);
			this.DeclarationDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(733, 43, true);
			this.DeclarationDetailsGroupBox.Controls.SetChildIndex(this.EarliestCustomsIssueDateEdit, 0);
			this.DeclarationDetailsGroupBox.Controls.SetChildIndex(this.ExportDeclarationNumberBoundTextBox, 0);
			this.DeclarationDetailsGroupBox.Controls.SetChildIndex(this.StatusTextBox, 0);
			this.DeclarationDetailsGroupBox.Controls.SetChildIndex(this.StyleOfEntrySOEDropDown, 0);
			// 
			// StatusTextBox
			// 
			this.StatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(351, 17, true);
			this.StatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 20, true);
			this.StatusTextBox.TabIndex = 4;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.JobDeclaration);
			// 
			// GoodsLocationDropEdit
			// 
			this.GoodsLocationDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GoodsLocationDropEdit, "JE_LocationOfGoods");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).JE_LocationOfGoods)));
			this.GoodsLocationDropEdit.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("EUJobDeclarationUserControl|1A465A1A-7ECD-4ABA-A375-DD5040A1C25B", "[30] Goods Location");
			this.GoodsLocationDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 113, true);
			this.GoodsLocationDropEdit.Name = "GoodsLocationDropEdit";
			this.GoodsLocationDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(339, 20, true);
			this.GoodsLocationDropEdit.TabIndex = 9;
			// 
			// AgentsReference
			// 
			this.AgentsReference.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.AgentsReference, "JE_AgentsReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).JE_AgentsReference)));
			this.AgentsReference.CaptionResourceString = null;
			this.AgentsReference.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 231, true);
			this.AgentsReference.Name = "AgentsReference";
			this.AgentsReference.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(359, 20, true);
			this.AgentsReference.TabIndex = 22;
			// 
			// CTStatusIDDropEdit
			// 
			this.CTStatusIDDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CTStatusIDDropEdit, "ZG_CTStatusID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).ZG_CTStatusID)));
			this.CTStatusIDDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 148, true);
			this.CTStatusIDDropEdit.Name = "CTStatusIDDropEdit";
			this.CTStatusIDDropEdit.PreBoundMaxLength = 2;
			this.CTStatusIDDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(145, 20, true);
			this.CTStatusIDDropEdit.TabIndex = 6;
			// 
			// InlandModeOfTransportDropEdit
			// 
			this.InlandModeOfTransportDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InlandModeOfTransportDropEdit, "JE_TransportModeInland");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).JE_TransportModeInland)));
			this.InlandModeOfTransportDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 181, true);
			this.InlandModeOfTransportDropEdit.Name = "InlandModeOfTransportDropEdit";
			this.InlandModeOfTransportDropEdit.PreBoundMaxLength = 2;
			this.InlandModeOfTransportDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 20, true);
			this.InlandModeOfTransportDropEdit.TabIndex = 16;
			// 
			// DepartureTransportIDTextBox
			// 
			this.DepartureTransportIDTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.DepartureTransportIDTextBox, "JE_VesselName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).JE_VesselName)));
			this.DepartureTransportIDTextBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("EUJobDeclarationUserControl|accba832-d4d4-4f9e-8a1f-f0f5d90f0a84", "[21] Trans.", "[21] Transport ID", "");
			this.DepartureTransportIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 64, true);
			this.DepartureTransportIDTextBox.Name = "DepartureTransportIDTextBox";
			this.DepartureTransportIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.DepartureTransportIDTextBox.TabIndex = 4;
			// 
			// TransportNationalityFindBox
			// 
			this.TransportNationalityFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransportNationalityFindBox, "JE_RN_NKTransportNationality");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).JE_RN_NKTransportNationality)));
			this.TransportNationalityFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(345, 64, true);
			this.TransportNationalityFindBox.Name = "TransportNationalityFindBox";
			this.TransportNationalityFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.TransportNationalityFindBox.ParentType = null;
			this.TransportNationalityFindBox.PreBoundMaxLength = 2;
			this.TransportNationalityFindBox.ShowDescriptionBox = false;
			this.TransportNationalityFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 20, true);
			this.TransportNationalityFindBox.TabIndex = 5;
			// 
			// EarliestCustomsIssueDateEdit
			// 
			this.EarliestCustomsIssueDateEdit.AllowDrop = true;
			this.EarliestCustomsIssueDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.EarliestCustomsIssueDateEdit, "EarliestCustomsEntryIssueDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).EarliestCustomsEntryIssueDate)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.EarliestCustomsIssueDateEdit, false);
			this.EarliestCustomsIssueDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(224, 17, true);
			this.EarliestCustomsIssueDateEdit.Name = "EarliestCustomsIssueDateEdit";
			this.EarliestCustomsIssueDateEdit.TabIndex = 2;
			// 
			// BadgeCodeDropEdit
			// 
			this.BadgeCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BadgeCodeDropEdit, "JE_CustomsProfile");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).JE_CustomsProfile)));
			this.BadgeCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 279, true);
			this.BadgeCodeDropEdit.Name = "BadgeCodeDropEdit";
			this.BadgeCodeDropEdit.PreBoundMaxLength = 3;
			this.BadgeCodeDropEdit.ShowDescriptionBox = false;
			this.BadgeCodeDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.BadgeCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.BadgeCodeDropEdit.TabIndex = 24;
			// 
			// GatewayDropEdit
			// 
			this.GatewayDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GatewayDropEdit, "ZG_Gateway");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).ZG_Gateway)));
			this.GatewayDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(221, 279, true);
			this.GatewayDropEdit.Name = "GatewayDropEdit";
			this.GatewayDropEdit.PreBoundMaxLength = 5;
			this.GatewayDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(233, 20, true);
			this.GatewayDropEdit.TabIndex = 25;
			// 
			// SpecificCircumstanceDropEdit
			// 
			this.SpecificCircumstanceDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SpecificCircumstanceDropEdit, "ZG_SpecificCircumstanceIndicator");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).ZG_SpecificCircumstanceIndicator)));
			this.SpecificCircumstanceDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 192, true);
			this.SpecificCircumstanceDropEdit.Name = "SpecificCircumstanceDropEdit";
			this.SpecificCircumstanceDropEdit.PreBoundMaxLength = 1;
			this.SpecificCircumstanceDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(145, 20, true);
			this.SpecificCircumstanceDropEdit.TabIndex = 8;
			// 
			// ImporterDocAddress
			// 
			this.ImporterDocAddress.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ImporterDocAddress, "ImporterDocumentaryAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).ImporterDocumentaryAddress)));
			this.ImporterDocAddress.BindToOrganisations = "Lookups.ImportersList";
			this.ImporterDocAddress.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("9caac92d-2056-4b43-82fc-4b25e5b5e2b8", "[8] Importer");
			this.ImporterDocAddress.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.HideOverrideShowTabs;
			this.ImporterDocAddress.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 192, true);
			this.ImporterDocAddress.Name = "ImporterDocAddress";
			this.ImporterDocAddress.ReadOnly = false;
			this.ImporterDocAddress.SingleLineNoGroupBoxPanelWidth = 296;
			this.ImporterDocAddress.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.ImporterDocAddress.TabIndex = 1;
			this.ImporterDocAddress.ValidationJustForced = false;
			// 
			// SupplierDocAddress
			// 
			this.SupplierDocAddress.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SupplierDocAddress, "SupplierDocumentaryAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).SupplierDocumentaryAddress)));
			this.SupplierDocAddress.BindToOrganisations = "Lookups.SuppliersList";
			this.SupplierDocAddress.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("8c6ec262-82eb-4054-b4a9-f7d72784c56e", "[2] Supplier");
			this.SupplierDocAddress.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.HideOverrideShowTabs;
			this.SupplierDocAddress.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 8, true);
			this.SupplierDocAddress.Name = "SupplierDocAddress";
			this.SupplierDocAddress.ReadOnly = false;
			this.SupplierDocAddress.SingleLineNoGroupBoxPanelWidth = 296;
			this.SupplierDocAddress.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.SupplierDocAddress.TabIndex = 0;
			this.SupplierDocAddress.ValidationJustForced = false;
			// 
			// StyleOfEntrySOEDropDown
			// 
			this.StyleOfEntrySOEDropDown.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StyleOfEntrySOEDropDown, "ZG_StyleOfEntrySOE");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).ZG_StyleOfEntrySOE)));
			this.StyleOfEntrySOEDropDown.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(545, 17, true);
			this.StyleOfEntrySOEDropDown.Name = "StyleOfEntrySOEDropDown";
			this.StyleOfEntrySOEDropDown.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.StyleOfEntrySOEDropDown.TabIndex = 6;
			// 
			// ZG_AgreedPlaceCodeDropEdit
			// 
			this.ZG_AgreedPlaceCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ZG_AgreedPlaceCodeDropEdit, "ZG_AgreedPlaceCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).ZG_AgreedPlaceCode)));
			this.ZG_AgreedPlaceCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(352, 184, true);
			this.ZG_AgreedPlaceCodeDropEdit.Name = "ZG_AgreedPlaceCodeDropEdit";
			this.ZG_AgreedPlaceCodeDropEdit.PreBoundMaxLength = 1;
			this.ZG_AgreedPlaceCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.ZG_AgreedPlaceCodeDropEdit.TabIndex = 17;
			// 
			// JE_ShipmentIncoTermPlaceTextBox
			// 
			this.BindingSource.SetBindingMember(this.JE_ShipmentIncoTermPlaceTextBox, "JE_ShipmentIncoTermPlace");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).JE_ShipmentIncoTermPlace)));
			this.JE_ShipmentIncoTermPlaceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(273, 184, true);
			this.JE_ShipmentIncoTermPlaceTextBox.Name = "JE_ShipmentIncoTermPlaceTextBox";
			this.JE_ShipmentIncoTermPlaceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			this.JE_ShipmentIncoTermPlaceTextBox.TabIndex = 16;
			// 
			// Box18TransportIDTextBox
			// 
			this.BindingSource.SetBindingMember(this.Box18TransportIDTextBox, "ZG_Box18TransportID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).ZG_Box18TransportID)));
			this.Box18TransportIDTextBox.CaptionResourceString = null;
			this.Box18TransportIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(143, 157, true);
			this.Box18TransportIDTextBox.Name = "Box18TransportIDTextBox";
			this.Box18TransportIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(107, 20, true);
			this.Box18TransportIDTextBox.TabIndex = 14;
			// 
			// Box18TrNationalityFindBox
			// 
			this.Box18TrNationalityFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.Box18TrNationalityFindBox, "ZG_Box18TransportNationality");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).ZG_Box18TransportNationality)));
			this.Box18TrNationalityFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(371, 157, true);
			this.Box18TrNationalityFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
			this.Box18TrNationalityFindBox.Name = "Box18TrNationalityFindBox";
			this.Box18TrNationalityFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.Box18TrNationalityFindBox.ParentType = null;
			this.Box18TrNationalityFindBox.PreBoundMaxLength = 2;
			this.Box18TrNationalityFindBox.ShowDescriptionBox = false;
			this.Box18TrNationalityFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 20, true);
			this.Box18TrNationalityFindBox.TabIndex = 15;
			// 
			// AircraftRegistrationNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.AircraftRegistrationNumberTextBox, "JE_AircraftRegistration");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).JE_AircraftRegistration)));
			this.AircraftRegistrationNumberTextBox.CaptionResourceString = null;
			this.AircraftRegistrationNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 40, true);
			this.AircraftRegistrationNumberTextBox.Name = "AircraftRegistrationNumberTextBox";
			this.AircraftRegistrationNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.AircraftRegistrationNumberTextBox.TabIndex = 8;
			// 
			// JE_IATALoadPortCodeFindBox
			// 
			this.JE_IATALoadPortCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JE_IATALoadPortCodeFindBox, "JE_IATALoadPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).JE_IATALoadPort)));
			this.JE_IATALoadPortCodeFindBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("25e5086a-6bac-4737-a58e-e5a7e20e95be", "IATA");
			this.JE_IATALoadPortCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(381, 88, true);
			this.JE_IATALoadPortCodeFindBox.Name = "JE_IATALoadPortCodeFindBox";
			this.JE_IATALoadPortCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.JE_IATALoadPortCodeFindBox.ParentType = null;
			this.JE_IATALoadPortCodeFindBox.PreBoundMaxLength = 3;
			this.JE_IATALoadPortCodeFindBox.ShowDescriptionBox = false;
			this.JE_IATALoadPortCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 20, true);
			this.JE_IATALoadPortCodeFindBox.TabIndex = 9;
			// 
			// PortOfFirstArrivalFindBox
			// 
			this.PortOfFirstArrivalFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PortOfFirstArrivalFindBox, "JE_RL_NKPortOfFirstArrival");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).JE_RL_NKPortOfFirstArrival)));
			this.PortOfFirstArrivalFindBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("01054047-e592-449f-a5bb-2a917cb808c7", "Port of First EU Arrival");
			this.PortOfFirstArrivalFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 111, true);
			this.PortOfFirstArrivalFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			this.PortOfFirstArrivalFindBox.Name = "PortOfFirstArrivalFindBox";
			this.PortOfFirstArrivalFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.PortOfFirstArrivalFindBox.ParentType = null;
			this.PortOfFirstArrivalFindBox.PreBoundMaxLength = 5;
			this.PortOfFirstArrivalFindBox.ShowDescriptionBox = false;
			this.PortOfFirstArrivalFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			this.PortOfFirstArrivalFindBox.TabIndex = 10;
			// 
			// JE_DateOfFirstArrivalBoundDateEdit
			// 
			this.JE_DateOfFirstArrivalBoundDateEdit.AllowDrop = true;
			this.JE_DateOfFirstArrivalBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.JE_DateOfFirstArrivalBoundDateEdit, "JE_DateOfFirstArrival");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).JE_DateOfFirstArrival)));
			this.JE_DateOfFirstArrivalBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(258, 111, true);
			this.JE_DateOfFirstArrivalBoundDateEdit.Name = "JE_DateOfFirstArrivalBoundDateEdit";
			this.JE_DateOfFirstArrivalBoundDateEdit.TabIndex = 11;
			// 
			// JE_UCRTextBox
			// 
			this.JE_UCRTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.JE_UCRTextBox, "JE_UCR");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).JE_UCR)));
			this.JE_UCRTextBox.CaptionResourceString = null;
			this.JE_UCRTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 255, true);
			this.JE_UCRTextBox.Name = "JE_UCRTextBox";
			this.JE_UCRTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(359, 20, true);
			this.JE_UCRTextBox.TabIndex = 23;
			// 
			// OrganisationsOuterPanel
			// 
			this.OrganisationsOuterPanel.Controls.Add(this.OrganisationsPanel);
			this.OrganisationsOuterPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.OrganisationsOuterPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 206, true);
			this.OrganisationsOuterPanel.Name = "OrganisationsOuterPanel";
			this.OrganisationsOuterPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(321, 275, true);
			this.OrganisationsOuterPanel.TabIndex = 6;
			// 
			// OrganisationsPanel
			// 
			this.OrganisationsPanel.Controls.Add(this.OrganizationExportUserControl);
			this.OrganisationsPanel.Controls.Add(this.OrganizationImportUserControl);
			this.OrganisationsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OrganisationsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OrganisationsPanel.Name = "OrganisationsPanel";
			this.OrganisationsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(321, 275, true);
			this.OrganisationsPanel.TabIndex = 7;
			// 
			// OrganizationExportUserControl
			// 
			this.OrganizationExportUserControl.AllowDrop = true;
			this.OrganizationExportUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OrganizationExportUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OrganizationExportUserControl.Name = "OrganizationExportUserControl";
			this.OrganizationExportUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(321, 275, true);
			this.OrganizationExportUserControl.TabIndex = 0;
			// 
			// OrganizationImportUserControl
			// 
			this.OrganizationImportUserControl.AllowDrop = true;
			this.OrganizationImportUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OrganizationImportUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OrganizationImportUserControl.Name = "OrganizationImportUserControl";
			this.OrganizationImportUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(321, 275, true);
			this.OrganizationImportUserControl.TabIndex = 1;
			// 
			// CustomsOfficesUserControl
			// 
			this.CustomsOfficesUserControl.AllowDrop = true;
			this.CustomsOfficesUserControl.AutoSize = true;
			this.CustomsOfficesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(255, 653, true);
			this.CustomsOfficesUserControl.Name = "CustomsOfficesUserControl";
			this.CustomsOfficesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(469, 140, true);
			this.CustomsOfficesUserControl.TabIndex = 7;
			// 
			// GoodsDestinationDropEdit
			// 
			this.GoodsDestinationDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GoodsDestinationDropEdit, "JE_GoodsDestination");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).JE_GoodsDestination)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.GoodsDestinationDropEdit, false);
			this.GoodsDestinationDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(283, 64, true);
			this.GoodsDestinationDropEdit.Name = "GoodsDestinationDropEdit";
			this.GoodsDestinationDropEdit.PreBoundMaxLength = 2;
			this.GoodsDestinationDropEdit.ShowDescriptionBox = false;
			this.GoodsDestinationDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 20, true);
			this.GoodsDestinationDropEdit.TabIndex = 6;
			// 
			// GoodsOriginDropEdit
			// 
			this.GoodsOriginDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GoodsOriginDropEdit, "JE_GoodsOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).JE_GoodsOrigin)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.GoodsOriginDropEdit, false);
			this.GoodsOriginDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(283, 40, true);
			this.GoodsOriginDropEdit.Name = "GoodsOriginDropEdit";
			this.GoodsOriginDropEdit.PreBoundMaxLength = 2;
			this.GoodsOriginDropEdit.ShowDescriptionBox = false;
			this.GoodsOriginDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 20, true);
			this.GoodsOriginDropEdit.TabIndex = 3;
			// 
			// IsHighValueOvrdCheckBox
			// 
			this.IsHighValueOvrdCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsHighValueOvrdCheckBox, "ZG_IsHighValueOvrd");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).ZG_IsHighValueOvrd)));
			this.IsHighValueOvrdCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IsHighValueOvrdCheckBox.FlatAppearance.CheckedBackColor = System.Drawing.SystemColors.Control;
			this.IsHighValueOvrdCheckBox.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.Control;
			this.IsHighValueOvrdCheckBox.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Control;
			this.IsHighValueOvrdCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 238, true);
			this.IsHighValueOvrdCheckBox.Name = "IsHighValueOvrdCheckBox";
			this.IsHighValueOvrdCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.IsHighValueOvrdCheckBox.TabIndex = 2;
			this.IsHighValueOvrdCheckBox.UseVisualStyleBackColor = false;
			// 
			// InlandTransportCodeDropEdit
			// 
			this.InlandTransportCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InlandTransportCodeDropEdit, "JE_TransportMeans");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).JE_TransportMeans)));
			this.InlandTransportCodeDropEdit.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("1445c3ef-1f05-46d7-a796-5113e69cd7f3", "[18] Code", "[18] Inland Transport Code");
			this.InlandTransportCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(371, 181, true);
			this.InlandTransportCodeDropEdit.Name = "InlandTransportCodeDropEdit";
			this.InlandTransportCodeDropEdit.PreBoundMaxLength = 2;
			this.InlandTransportCodeDropEdit.ShowDescriptionBox = false;
			this.InlandTransportCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 20, true);
			this.InlandTransportCodeDropEdit.TabIndex = 17;
			// 
			// RegionOfDestinationDropEdit
			// 
			this.RegionOfDestinationDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RegionOfDestinationDropEdit, "ZG_RegionOfDestination");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).ZG_RegionOfDestination)));
			this.RegionOfDestinationDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(371, 181, true);
			this.RegionOfDestinationDropEdit.Name = "RegionOfDestinationDropEdit";
			this.RegionOfDestinationDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 20, true);
			this.RegionOfDestinationDropEdit.Visible = false;
			this.RegionOfDestinationDropEdit.TabIndex = 17;
			// 
			// EUJobDeclarationUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CustomsOfficesUserControl);
			this.Controls.Add(this.SupplierDocAddress);
			this.Controls.Add(this.ImporterDocAddress);
			this.Name = "EUJobDeclarationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1057, 800, true);
			this.Controls.SetChildIndex(this.ImporterDocAddress, 0);
			this.Controls.SetChildIndex(this.SupplierDocAddress, 0);
			this.Controls.SetChildIndex(this.SupplierOrganisationControl, 0);
			this.Controls.SetChildIndex(this.ImporterOrganisationControl, 0);
			this.Controls.SetChildIndex(this.ShipmentTypeGroupBox, 0);
			this.Controls.SetChildIndex(this.ShipmentDetailsGroupBox, 0);
			this.Controls.SetChildIndex(this.TransportDetailsGroupBox, 0);
			this.Controls.SetChildIndex(this.RightTabControl, 0);
			this.Controls.SetChildIndex(this.DeclarationDetailsGroupBox, 0);
			this.Controls.SetChildIndex(this.CustomsOfficesUserControl, 0);
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
			this.GoodsLocationDropEdit.ResumeLayout(true);
			this.GoodsLocationDropEdit.PerformLayout();
			this.CTStatusIDDropEdit.ResumeLayout(true);
			this.CTStatusIDDropEdit.PerformLayout();
			this.InlandModeOfTransportDropEdit.ResumeLayout(true);
			this.InlandModeOfTransportDropEdit.PerformLayout();
			this.TransportNationalityFindBox.ResumeLayout(true);
			this.TransportNationalityFindBox.PerformLayout();
			this.EarliestCustomsIssueDateEdit.ResumeLayout(true);
			this.EarliestCustomsIssueDateEdit.PerformLayout();
			this.BadgeCodeDropEdit.ResumeLayout(true);
			this.BadgeCodeDropEdit.PerformLayout();
			this.GatewayDropEdit.ResumeLayout(true);
			this.GatewayDropEdit.PerformLayout();
			this.SpecificCircumstanceDropEdit.ResumeLayout(true);
			this.SpecificCircumstanceDropEdit.PerformLayout();
			this.ImporterDocAddress.ResumeLayout(true);
			this.ImporterDocAddress.PerformLayout();
			this.SupplierDocAddress.ResumeLayout(true);
			this.SupplierDocAddress.PerformLayout();
			this.StyleOfEntrySOEDropDown.ResumeLayout(true);
			this.StyleOfEntrySOEDropDown.PerformLayout();
			this.ZG_AgreedPlaceCodeDropEdit.ResumeLayout(true);
			this.ZG_AgreedPlaceCodeDropEdit.PerformLayout();
			this.Box18TrNationalityFindBox.ResumeLayout(true);
			this.Box18TrNationalityFindBox.PerformLayout();
			this.JE_IATALoadPortCodeFindBox.ResumeLayout(true);
			this.JE_IATALoadPortCodeFindBox.PerformLayout();
			this.PortOfFirstArrivalFindBox.ResumeLayout(true);
			this.PortOfFirstArrivalFindBox.PerformLayout();
			this.JE_DateOfFirstArrivalBoundDateEdit.ResumeLayout(true);
			this.JE_DateOfFirstArrivalBoundDateEdit.PerformLayout();
			this.OrganisationsOuterPanel.ResumeLayout(false);
			this.OrganisationsOuterPanel.PerformLayout();
			this.OrganisationsPanel.ResumeLayout(false);
			this.OrganisationsPanel.PerformLayout();
			this.GoodsDestinationDropEdit.ResumeLayout(true);
			this.GoodsDestinationDropEdit.PerformLayout();
			this.GoodsOriginDropEdit.ResumeLayout(true);
			this.GoodsOriginDropEdit.PerformLayout();
			this.InlandTransportCodeDropEdit.ResumeLayout(true);
			this.InlandTransportCodeDropEdit.PerformLayout();
			this.RegionOfDestinationDropEdit.ResumeLayout(true);
			this.RegionOfDestinationDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public Enterprise.ZArchitecture.GUI.ZDropEdit CTStatusIDDropEdit;
		public Enterprise.ZArchitecture.ZTextBox DepartureTransportIDTextBox;
		public Enterprise.ZArchitecture.GUI.ZDropEdit BadgeCodeDropEdit;
		public Enterprise.ZArchitecture.GUI.ZDropEdit GatewayDropEdit;
		public Enterprise.MasterFiles.GUI.ZDocAddressControl ImporterDocAddress;
		public Enterprise.MasterFiles.GUI.ZDocAddressControl SupplierDocAddress;
		public ZArchitecture.GUI.ZDropEdit StyleOfEntrySOEDropDown;
		public ZArchitecture.GUI.ZDropEdit ZG_AgreedPlaceCodeDropEdit;
		public Enterprise.ZArchitecture.GUI.ZDropEdit GoodsLocationDropEdit;
		public ZArchitecture.ZTextBox JE_ShipmentIncoTermPlaceTextBox;
		public ZArchitecture.ZTextBox Box18TransportIDTextBox;
		public ZArchitecture.GUI.ZCodeFindBox Box18TrNationalityFindBox;
		public Enterprise.ZArchitecture.GUI.ZCodeFindBox JE_IATALoadPortCodeFindBox;
		public Enterprise.ZArchitecture.ZTextBox AircraftRegistrationNumberTextBox;
		public Enterprise.ZArchitecture.GUI.ZCodeFindBox PortOfFirstArrivalFindBox;
		public Enterprise.ZArchitecture.GUI.ZDateEdit JE_DateOfFirstArrivalBoundDateEdit;
		public Enterprise.ZArchitecture.ZTextBox AgentsReference;
		public ZArchitecture.ZTextBox JE_UCRTextBox;
		protected Enterprise.ZArchitecture.GUI.ZDropEdit InlandModeOfTransportDropEdit;
		public Enterprise.ZArchitecture.GUI.ZCodeFindBox TransportNationalityFindBox;
		protected Enterprise.ZArchitecture.GUI.ZDateEdit EarliestCustomsIssueDateEdit;
		protected Enterprise.ZArchitecture.GUI.ZDropEdit SpecificCircumstanceDropEdit;
		protected CargoWise.Windows.UI.KPanel OrganisationsOuterPanel;
		protected CargoWise.Windows.UI.KPanel OrganisationsPanel;
		protected ZArchitecture.GUI.ZDynamicControlCreationUserControl CustomsOfficesUserControl;
		protected Enterprise.ZArchitecture.GUI.ZDropEdit GoodsDestinationDropEdit;
		protected Enterprise.ZArchitecture.GUI.ZDropEdit GoodsOriginDropEdit;
		protected internal ZArchitecture.GUI.ZDynamicControlCreationUserControl OrganizationExportUserControl;
		protected internal ZArchitecture.GUI.ZDynamicControlCreationUserControl OrganizationImportUserControl;
		public ZArchitecture.GUI.ZCheckBox IsHighValueOvrdCheckBox;
		internal ZArchitecture.GUI.ZDropEdit InlandTransportCodeDropEdit;
		public ZArchitecture.GUI.ZDropEdit RegionOfDestinationDropEdit;
	}
}
