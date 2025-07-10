using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.AU.AirCargo.GUI
{
	partial class AirCTOHawbExportUserControl
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
		void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AirCTOHawbExportUserControl));
			this.airWayBillLabel = new Enterprise.ZArchitecture.ZLabel();
			this.airWayBillTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.goodsDescLabel = new Enterprise.ZArchitecture.ZLabel();
			this.goodsDescTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.lineCanLabel = new Enterprise.ZArchitecture.ZLabel();
			this.lineCanTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.goodsOwnerLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ownerTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.goodsOwnerPartyIDLabel = new Enterprise.ZArchitecture.ZLabel();
			this.goodsOwnerPartyIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ownerOrgLabel = new Enterprise.ZArchitecture.ZLabel();
			this.noOfPacksLabel = new Enterprise.ZArchitecture.ZLabel();
			this.noOfPacksCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.destinationCountryLabel = new Enterprise.ZArchitecture.ZLabel();
			this.destCountryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ctoRecDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ctoRemDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ctoRecLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ctoRemLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ownerOrganisationFindBox = new Enterprise.MasterFiles.GUI.ZOrganisationFindBox();
			this.canTypeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.typeOfCANDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ediMessageUserControl = new Enterprise.Messaging.GUI.EDIMessageUserControl();
			this.topPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.consigneeDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.consignorDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.volumeBoundTextBox = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.weightBoundTextBox = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.volumeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.weightLabel = new Enterprise.ZArchitecture.ZLabel();
			this.topPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// airWayBillLabel
			// 
			this.airWayBillLabel.AutoSize = true;
			this.airWayBillLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 16, true);
			this.airWayBillLabel.Name = "airWayBillLabel";
			this.airWayBillLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(61, 13, true);
			this.airWayBillLabel.TabIndex = 0;
			this.airWayBillLabel.Text = "AirWay Bill:";
			// 
			// airWayBillTextBox
			// 
			this.airWayBillTextBox.BindTo = "EL_AirWayBill";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.ExportCustomsManifestLines)(null)).EL_AirWayBillInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.ExportCustomsManifestLines)(null)).EL_AirWayBill)));
			this.airWayBillTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 16, true);
			this.airWayBillTextBox.Name = "airWayBillTextBox";
			this.airWayBillTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.airWayBillTextBox.TabIndex = 1;
			// 
			// goodsDescLabel
			// 
			this.goodsDescLabel.AutoSize = true;
			this.goodsDescLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 160, true);
			this.goodsDescLabel.Name = "goodsDescLabel";
			this.goodsDescLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 13, true);
			this.goodsDescLabel.TabIndex = 20;
			this.goodsDescLabel.Text = "Goods Desc:";
			// 
			// goodsDescTextBox
			// 
			this.goodsDescTextBox.BindTo = "EL_GoodsDescription";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.ExportCustomsManifestLines)(null)).EL_GoodsDescriptionInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.ExportCustomsManifestLines)(null)).EL_GoodsDescription)));
			this.goodsDescTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 160, true);
			this.goodsDescTextBox.Name = "goodsDescTextBox";
			this.goodsDescTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 20, true);
			this.goodsDescTextBox.TabIndex = 21;
			// 
			// lineCanLabel
			// 
			this.lineCanLabel.AutoSize = true;
			this.lineCanLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 64, true);
			this.lineCanLabel.Name = "lineCanLabel";
			this.lineCanLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 13, true);
			this.lineCanLabel.TabIndex = 8;
			this.lineCanLabel.Text = "CAN:";
			// 
			// lineCanTextBox
			// 
			this.lineCanTextBox.BindTo = "EL_CAN";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.ExportCustomsManifestLines)(null)).EL_CANInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.ExportCustomsManifestLines)(null)).EL_CAN)));
			this.lineCanTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 64, true);
			this.lineCanTextBox.Name = "lineCanTextBox";
			this.lineCanTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.lineCanTextBox.TabIndex = 9;
			// 
			// goodsOwnerLabel
			// 
			this.goodsOwnerLabel.AutoSize = true;
			this.goodsOwnerLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(224, 40, true);
			this.goodsOwnerLabel.Name = "goodsOwnerLabel";
			this.goodsOwnerLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 13, true);
			this.goodsOwnerLabel.TabIndex = 6;
			this.goodsOwnerLabel.Text = "Owner:";
			// 
			// ownerTextBox
			// 
			this.ownerTextBox.BindTo = "EL_GoodsOwner";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.ExportCustomsManifestLines)(null)).EL_GoodsOwnerInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.ExportCustomsManifestLines)(null)).EL_GoodsOwner)));
			this.ownerTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(320, 40, true);
			this.ownerTextBox.Name = "ownerTextBox";
			this.ownerTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.ownerTextBox.TabIndex = 7;
			// 
			// goodsOwnerPartyIDLabel
			// 
			this.goodsOwnerPartyIDLabel.AutoSize = true;
			this.goodsOwnerPartyIDLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(224, 64, true);
			this.goodsOwnerPartyIDLabel.Name = "goodsOwnerPartyIDLabel";
			this.goodsOwnerPartyIDLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(86, 13, true);
			this.goodsOwnerPartyIDLabel.TabIndex = 10;
			this.goodsOwnerPartyIDLabel.Text = "Owner Party ID:";
			// 
			// goodsOwnerPartyIDTextBox
			// 
			this.goodsOwnerPartyIDTextBox.BindTo = "EL_GoodsOwnerPartyID";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.ExportCustomsManifestLines)(null)).EL_GoodsOwnerPartyIDInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.ExportCustomsManifestLines)(null)).EL_GoodsOwnerPartyID)));
			this.goodsOwnerPartyIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(320, 64, true);
			this.goodsOwnerPartyIDTextBox.Name = "goodsOwnerPartyIDTextBox";
			this.goodsOwnerPartyIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.goodsOwnerPartyIDTextBox.TabIndex = 11;
			// 
			// ownerOrgLabel
			// 
			this.ownerOrgLabel.AutoSize = true;
			this.ownerOrgLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(224, 16, true);
			this.ownerOrgLabel.Name = "ownerOrgLabel";
			this.ownerOrgLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 13, true);
			this.ownerOrgLabel.TabIndex = 2;
			this.ownerOrgLabel.Text = "Owner Org:";
			// 
			// noOfPacksLabel
			// 
			this.noOfPacksLabel.AutoSize = true;
			this.noOfPacksLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 88, true);
			this.noOfPacksLabel.Name = "noOfPacksLabel";
			this.noOfPacksLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 13, true);
			this.noOfPacksLabel.TabIndex = 12;
			this.noOfPacksLabel.Text = "No. of Packs:";
			// 
			// noOfPacksCalcEdit
			// 
			this.noOfPacksCalcEdit.BindTo = "EL_NumberOfPackages";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.ExportCustomsManifestLines)(null)).EL_NumberOfPackages)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.ExportCustomsManifestLines)(null)).EL_NumberOfPackagesInfo)));
			this.noOfPacksCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 88, true);
			this.noOfPacksCalcEdit.Name = "noOfPacksCalcEdit";
			this.noOfPacksCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.noOfPacksCalcEdit.TabIndex = 13;
			this.noOfPacksCalcEdit.Text = "0";
			this.noOfPacksCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// destinationCountryLabel
			// 
			this.destinationCountryLabel.AutoSize = true;
			this.destinationCountryLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(224, 88, true);
			this.destinationCountryLabel.Name = "destinationCountryLabel";
			this.destinationCountryLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(79, 13, true);
			this.destinationCountryLabel.TabIndex = 14;
			this.destinationCountryLabel.Text = "Dest. Ctry/Rgn.:";
			// 
			// destCountryCodeFindBox
			// 
			this.destCountryCodeFindBox.BindTo = "EL_RN_NKCountryOfDestination";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.ExportCustomsManifestLines)(null)).EL_RN_NKCountryOfDestinationInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.ExportCustomsManifestLines)(null)).EL_RN_NKCountryOfDestination)));
			this.destCountryCodeFindBox.BindToList = "Lookups+CountryOfDestinations";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.ExportCustomsManifestLines)(null)).Lookups.CountryOfDestinations)));
			this.destCountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(320, 88, true);
			this.destCountryCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
			this.destCountryCodeFindBox.Name = "destCountryCodeFindBox";
			this.destCountryCodeFindBox.PreBoundMaxLength = 2;
			this.destCountryCodeFindBox.ShowDescriptionBox = false;
			this.destCountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 20, true);
			this.destCountryCodeFindBox.TabIndex = 15;
			// 
			// ctoRecDropEdit
			// 
			this.ctoRecDropEdit.BindTo = "CTORECStatus+Code";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.ExportCustomsManifestLines)(null)).CTORECStatus.CodeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.ExportCustomsManifestLines)(null)).CTORECStatus.Code)));
			this.ctoRecDropEdit.BindToForDescription = "CTORECStatus+Description";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.ExportCustomsManifestLines)(null)).CTORECStatus.DescriptionInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.ExportCustomsManifestLines)(null)).CTORECStatus.Description)));
			this.ctoRecDropEdit.BindToList = "CTORECStatus+List";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.ExportCustomsManifestLines)(null)).CTORECStatus.List)));
			this.ctoRecDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 136, true);
			this.ctoRecDropEdit.Name = "ctoRecDropEdit";
			this.ctoRecDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 20, true);
			this.ctoRecDropEdit.TabIndex = 19;
			// 
			// ctoRemDropEdit
			// 
			this.ctoRemDropEdit.BindTo = "CTOREMStatus+Code";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.ExportCustomsManifestLines)(null)).CTOREMStatus.CodeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.ExportCustomsManifestLines)(null)).CTOREMStatus.Code)));
			this.ctoRemDropEdit.BindToForDescription = "CTOREMStatus+Description";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.ExportCustomsManifestLines)(null)).CTOREMStatus.DescriptionInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.ExportCustomsManifestLines)(null)).CTOREMStatus.Description)));
			this.ctoRemDropEdit.BindToList = "CTOREMStatus+List";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.ExportCustomsManifestLines)(null)).CTOREMStatus.List)));
			this.ctoRemDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 112, true);
			this.ctoRemDropEdit.Name = "ctoRemDropEdit";
			this.ctoRemDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 20, true);
			this.ctoRemDropEdit.TabIndex = 17;
			// 
			// ctoRecLabel
			// 
			this.ctoRecLabel.AutoSize = true;
			this.ctoRecLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 136, true);
			this.ctoRecLabel.Name = "ctoRecLabel";
			this.ctoRecLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(86, 13, true);
			this.ctoRecLabel.TabIndex = 18;
			this.ctoRecLabel.Text = "CTOREC Status:";
			// 
			// ctoRemLabel
			// 
			this.ctoRemLabel.AutoSize = true;
			this.ctoRemLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 112, true);
			this.ctoRemLabel.Name = "ctoRemLabel";
			this.ctoRemLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 13, true);
			this.ctoRemLabel.TabIndex = 16;
			this.ctoRemLabel.Text = "CTOREM Status:";
			// 
			// ownerOrganisationFindBox
			// 
			this.ownerOrganisationFindBox.BindTo = "EL_OH_Owner";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZGuid)(((Enterprise.Customs.AU.Declaration.Business.ExportCustomsManifestLines)(null)).EL_OH_Owner)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.ExportCustomsManifestLines)(null)).EL_OH_OwnerInfo)));
			this.ownerOrganisationFindBox.BindToList = "Lookups+Owners";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.ExportCustomsManifestLines)(null)).Lookups.Owners)));
			this.ownerOrganisationFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(320, 16, true);
			this.ownerOrganisationFindBox.Name = "ownerOrganisationFindBox";
			this.ownerOrganisationFindBox.ShowDescriptionBox = false;
			this.ownerOrganisationFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.ownerOrganisationFindBox.TabIndex = 3;
			// 
			// canTypeLabel
			// 
			this.canTypeLabel.AutoSize = true;
			this.canTypeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 40, true);
			this.canTypeLabel.Name = "canTypeLabel";
			this.canTypeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(59, 13, true);
			this.canTypeLabel.TabIndex = 4;
			this.canTypeLabel.Text = "CAN Type:";
			// 
			// typeOfCANDropEdit
			// 
			this.typeOfCANDropEdit.BindTo = "EL_TypeOfCAN";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.ExportCustomsManifestLines)(null)).EL_TypeOfCANInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.ExportCustomsManifestLines)(null)).EL_TypeOfCAN)));
			this.typeOfCANDropEdit.BindToList = "Lookups+TypeOfCANs";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.ExportCustomsManifestLines)(null)).Lookups.TypeOfCANs)));
			this.typeOfCANDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 40, true);
			this.typeOfCANDropEdit.Name = "typeOfCANDropEdit";
			this.typeOfCANDropEdit.PreBoundMaxLength = 3;
			this.typeOfCANDropEdit.ShowDescriptionBox = false;
			this.typeOfCANDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.typeOfCANDropEdit.TabIndex = 5;
			// 
			// ediMessageUserControl
			//
			this.ediMessageUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ediMessageUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 208, true);
			this.ediMessageUserControl.Name = "ediMessageUserControl";
			this.ediMessageUserControl.ShowChangingBlueMessageHeading = false;
			this.ediMessageUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1004, 352, true);
			this.ediMessageUserControl.TabIndex = 1;
			// 
			// topPanel
			// 
			this.topPanel.Controls.Add(this.consigneeDocAddressControl);
			this.topPanel.Controls.Add(this.consignorDocAddressControl);
			this.topPanel.Controls.Add(this.volumeBoundTextBox);
			this.topPanel.Controls.Add(this.weightBoundTextBox);
			this.topPanel.Controls.Add(this.airWayBillTextBox);
			this.topPanel.Controls.Add(this.goodsOwnerPartyIDTextBox);
			this.topPanel.Controls.Add(this.typeOfCANDropEdit);
			this.topPanel.Controls.Add(this.noOfPacksLabel);
			this.topPanel.Controls.Add(this.ownerOrgLabel);
			this.topPanel.Controls.Add(this.goodsOwnerPartyIDLabel);
			this.topPanel.Controls.Add(this.canTypeLabel);
			this.topPanel.Controls.Add(this.noOfPacksCalcEdit);
			this.topPanel.Controls.Add(this.volumeLabel);
			this.topPanel.Controls.Add(this.weightLabel);
			this.topPanel.Controls.Add(this.airWayBillLabel);
			this.topPanel.Controls.Add(this.ownerTextBox);
			this.topPanel.Controls.Add(this.ownerOrganisationFindBox);
			this.topPanel.Controls.Add(this.destinationCountryLabel);
			this.topPanel.Controls.Add(this.goodsOwnerLabel);
			this.topPanel.Controls.Add(this.ctoRemLabel);
			this.topPanel.Controls.Add(this.destCountryCodeFindBox);
			this.topPanel.Controls.Add(this.goodsDescLabel);
			this.topPanel.Controls.Add(this.lineCanTextBox);
			this.topPanel.Controls.Add(this.ctoRecLabel);
			this.topPanel.Controls.Add(this.ctoRecDropEdit);
			this.topPanel.Controls.Add(this.goodsDescTextBox);
			this.topPanel.Controls.Add(this.lineCanLabel);
			this.topPanel.Controls.Add(this.ctoRemDropEdit);
			this.topPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.topPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.topPanel.Name = "topPanel";
			this.topPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1004, 208, true);
			this.topPanel.TabIndex = 0;
			// 
			// consigneeDocAddressControl
			// 
			this.consigneeDocAddressControl.BindTo = "ConsigneeDocumentaryAddress";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.AU.Declaration.Business.ExportCustomsManifestLines)(null)).ConsigneeDocumentaryAddress)));
			this.consigneeDocAddressControl.BindToContacts = "";
			this.consigneeDocAddressControl.BindToOrganisations = "Consignee_List";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((Enterprise.MasterFiles.Business.OrgHeaderCollection)(((Enterprise.Customs.AU.Declaration.Business.ExportCustomsManifestLines)(null)).Consignee_List)));
			this.consigneeDocAddressControl.Text = "Consignee";
			this.consigneeDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(712, 8, true);
			this.consigneeDocAddressControl.Name = "consigneeDocAddressControl";
			this.consigneeDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 182, true);
			this.consigneeDocAddressControl.TabIndex = 27;
			// 
			// consignorDocAddressControl
			// 
			this.consignorDocAddressControl.BindTo = "ConsignorDocumentaryAddress";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.AU.Declaration.Business.ExportCustomsManifestLines)(null)).ConsignorDocumentaryAddress)));
			this.consignorDocAddressControl.BindToContacts = "";
			this.consignorDocAddressControl.BindToOrganisations = "Consignor_List";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((Enterprise.MasterFiles.Business.OrgHeaderCollection)(((Enterprise.Customs.AU.Declaration.Business.ExportCustomsManifestLines)(null)).Consignor_List)));
			this.consignorDocAddressControl.Text = "Consignor";
			this.consignorDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(440, 8, true);
			this.consignorDocAddressControl.Name = "consignorDocAddressControl";
			this.consignorDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 182, true);
			this.consignorDocAddressControl.TabIndex = 26;
			// 
			// volumeBoundTextBox
			// 
			this.volumeBoundTextBox.BindToAmount = "EL_Volume";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.ExportCustomsManifestLines)(null)).EL_Volume)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.ExportCustomsManifestLines)(null)).EL_VolumeInfo)));
			this.volumeBoundTextBox.BindToList = "EL_Volume_List";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.ExportCustomsManifestLines)(null)).EL_Volume_List)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.ExportCustomsManifestLines)(null)).EL_Volume_List)));
			this.volumeBoundTextBox.BindToUnit = "EL_VolumeUQ";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.ExportCustomsManifestLines)(null)).EL_VolumeUQInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.ExportCustomsManifestLines)(null)).EL_VolumeUQ)));
			this.volumeBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(320, 184, true);
			this.volumeBoundTextBox.Name = "volumeBoundTextBox";
			this.volumeBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.volumeBoundTextBox.TabIndex = 25;
			this.volumeBoundTextBox.UnitPreBoundMaxLength = 2;
			// 
			// weightBoundTextBox
			// 
			this.weightBoundTextBox.BindToAmount = "EL_Weight";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.ExportCustomsManifestLines)(null)).EL_Weight)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.ExportCustomsManifestLines)(null)).EL_WeightInfo)));
			this.weightBoundTextBox.BindToList = "EL_Weight_List";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.ExportCustomsManifestLines)(null)).EL_Weight_List)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.ExportCustomsManifestLines)(null)).EL_Weight_List)));
			this.weightBoundTextBox.BindToUnit = "EL_WeightUQ";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.ExportCustomsManifestLines)(null)).EL_WeightUQInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.ExportCustomsManifestLines)(null)).EL_WeightUQ)));
			this.weightBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 184, true);
			this.weightBoundTextBox.Name = "weightBoundTextBox";
			this.weightBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.weightBoundTextBox.TabIndex = 23;
			this.weightBoundTextBox.UnitPreBoundMaxLength = 2;
			// 
			// volumeLabel
			// 
			this.volumeLabel.AutoSize = true;
			this.volumeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(224, 184, true);
			this.volumeLabel.Name = "volumeLabel";
			this.volumeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(45, 13, true);
			this.volumeLabel.TabIndex = 24;
			this.volumeLabel.Text = "Volume:";
			// 
			// weightLabel
			// 
			this.weightLabel.AutoSize = true;
			this.weightLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 184, true);
			this.weightLabel.Name = "weightLabel";
			this.weightLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(45, 13, true);
			this.weightLabel.TabIndex = 22;
			this.weightLabel.Text = "Weight:";
			// 
			// AirCTOHawbExportUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.ediMessageUserControl);
			this.Controls.Add(this.topPanel);
			this.DataSourceAssemblyName = "Enterprise.Customs.AU.Declaration.Business";
			this.DataSourceTypeName = "Enterprise.Customs.AU.Declaration.Business.ExportCustomsManifestLines";
			this.Name = "AirCTOHawbExportUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1004, 560, true);
			this.topPanel.ResumeLayout(false);
			this.topPanel.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.ZLabel airWayBillLabel;
		private Enterprise.ZArchitecture.ZTextBox airWayBillTextBox;
		private Enterprise.ZArchitecture.ZLabel goodsDescLabel;
		private Enterprise.ZArchitecture.ZTextBox goodsDescTextBox;
		private Enterprise.ZArchitecture.ZLabel lineCanLabel;
		private Enterprise.ZArchitecture.ZTextBox lineCanTextBox;
		private Enterprise.ZArchitecture.ZLabel goodsOwnerLabel;
		private Enterprise.ZArchitecture.ZTextBox ownerTextBox;
		private Enterprise.ZArchitecture.ZLabel goodsOwnerPartyIDLabel;
		private Enterprise.ZArchitecture.ZTextBox goodsOwnerPartyIDTextBox;
		private Enterprise.ZArchitecture.ZLabel ownerOrgLabel;
		private Enterprise.ZArchitecture.ZLabel noOfPacksLabel;
		private Enterprise.ZArchitecture.ZCalcEdit noOfPacksCalcEdit;
		private Enterprise.ZArchitecture.ZLabel destinationCountryLabel;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox destCountryCodeFindBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit ctoRecDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit ctoRemDropEdit;
		private Enterprise.ZArchitecture.ZLabel ctoRecLabel;
		private Enterprise.ZArchitecture.ZLabel ctoRemLabel;
		private Enterprise.MasterFiles.GUI.ZOrganisationFindBox ownerOrganisationFindBox;
		private Enterprise.ZArchitecture.ZLabel canTypeLabel;
		private Enterprise.ZArchitecture.GUI.ZDropEdit typeOfCANDropEdit;
		private Enterprise.Messaging.GUI.EDIMessageUserControl ediMessageUserControl;
		private Enterprise.ZArchitecture.GUI.ZPanel topPanel;
		private Enterprise.MasterFiles.GUI.ZDocAddressControl consignorDocAddressControl;
		private Enterprise.MasterFiles.GUI.ZDocAddressControl consigneeDocAddressControl;
		private Enterprise.ZArchitecture.GUI.ZCalcDropEdit volumeBoundTextBox;
		private Enterprise.ZArchitecture.GUI.ZCalcDropEdit weightBoundTextBox;
		private Enterprise.ZArchitecture.ZLabel volumeLabel;
		private Enterprise.ZArchitecture.ZLabel weightLabel;
	}
}
