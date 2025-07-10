using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Client.UPE.Business;
using Enterprise.Customs.AU.AirCargo.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.UPE.GUI
{
	public partial class CalloutForm : CusHAWBForm, IAllowTabBackwardBetweenSomeOfMyChildren
	{
		Enterprise.ZArchitecture.GUI.ZGroupBox zGroupBox1;
		Enterprise.ZArchitecture.ZLabel zLabel31;
		Enterprise.ZArchitecture.ZLabel zLabel32;
		Enterprise.ZArchitecture.ZLabel zLabel33;
		Enterprise.ZArchitecture.ZLabel zLabel34;
		Enterprise.ZArchitecture.ZLabel zLabel36;
		Enterprise.ZArchitecture.ZLabel zLabel37;
		protected Enterprise.ZArchitecture.GUI.ZGroupBox zGroupBox5;
		Enterprise.ZArchitecture.ZLabel zLabel21;
		Enterprise.ZArchitecture.ZLabel zLabel22;
		protected Enterprise.ZArchitecture.ZGrid LineChargesGrid;
		protected Enterprise.ZArchitecture.GUI.ZGroupBox zGroupBox6;
		protected Enterprise.ZArchitecture.GUI.ZRadioButton ChequeRadioButton;
		Enterprise.ZArchitecture.GUI.ZRadioButton zRadioButton2;
		Enterprise.ZArchitecture.GUI.ZRadioButton zRadioButton3;
		Enterprise.ZArchitecture.GUI.ZRadioButton zRadioButton4;
		Enterprise.ZArchitecture.GUI.ZRadioButton zRadioButton5;
		Enterprise.ZArchitecture.GUI.ZRadioButton zRadioButton6;
		Enterprise.ZArchitecture.GUI.ZRadioButton zRadioButton7;
		Enterprise.ZArchitecture.GUI.ZRadioButton zRadioButton8;
		Enterprise.ZArchitecture.GUI.ZGroupBox zGroupBox4;
		Enterprise.ZArchitecture.GUI.ZCalcFindBox CustomsValueCalcFindBox;
		Enterprise.ZArchitecture.ZLabel zLabel10;
		Enterprise.ZArchitecture.ZLabel zLabel11;
		Enterprise.ZArchitecture.ZLabel zLabel12;
		Enterprise.ZArchitecture.ZLabel zLabel13;
		Enterprise.ZArchitecture.ZLabel zLabel14;
		Enterprise.ZArchitecture.ZLabel zLabel15;
		Enterprise.ZArchitecture.ZLabel zLabel16;
		Enterprise.ZArchitecture.ZLabel zLabel17;
		Enterprise.ZArchitecture.ZLabel zLabel18;
		Enterprise.ZArchitecture.ZLabel zLabel20;
		Enterprise.ZArchitecture.ZLabel zLabel25;
		Enterprise.ZArchitecture.ZTextBox ImporterContactTextBox;
		Enterprise.ZArchitecture.ZTextBox ImporterStateTextBox;
		Enterprise.ZArchitecture.ZTextBox ImporterAddress1TextBox;
		Enterprise.ZArchitecture.ZTextBox ImporterAccountClassTextBox;
		Enterprise.ZArchitecture.ZTextBox ImporterAccountNoTextBox;
		Enterprise.ZArchitecture.ZTextBox ImporterCityTextBox;
		internal PopupEditableZGuidFindBox ImporterOrganisationFindBox;
		Enterprise.ZArchitecture.ZTextBox ImporterPhoneTextBox;
		Enterprise.ZArchitecture.ZTextBox ImporterPostCodeTextBox;
		Enterprise.ZArchitecture.ZTextBox ImporterNameTextBox;
		Enterprise.ZArchitecture.ZTextBox ImporterAddress2TextBox;
		Enterprise.ZArchitecture.GUI.ZCodeFindBox ImporterCountryFindBox;
		Enterprise.ZArchitecture.ZGrid ChildPackagesGrid;
		Enterprise.ZArchitecture.GUI.ZGroupBox ChildPackagesGroupBox;
		Enterprise.ZArchitecture.ZLabel zLabel38;
		Enterprise.ZArchitecture.GUI.ZRadioButton zRadioButton9;
		protected Enterprise.ZArchitecture.ZLabel zLabel41;
		protected Enterprise.ZArchitecture.ZLabel zLabel42;
		Enterprise.ZArchitecture.ZTextBox InvoiceNumberTextBox;
		Enterprise.ZArchitecture.ZCalcEdit TotalAmountDueCalcEdit;
		protected Enterprise.ZArchitecture.GUI.ZDateEdit BisiUploadDateEdit;
		protected Enterprise.ZArchitecture.GUI.ZDateEdit BisiDownloadDateEdit;
		Enterprise.ZArchitecture.GUI.ZDateEdit ArrivalDateEdit;
		Enterprise.ZArchitecture.ZTextBox DescriptionTextBox;
		Enterprise.ZArchitecture.GUI.ZDateEdit DeliveryDateEdit;
		Enterprise.ZArchitecture.ZCalcEdit PiecesLandedCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit WeightInKGCalcEdit;
		Enterprise.ZArchitecture.ZTextBox ShipmentTypeTextBox;
		Enterprise.ZArchitecture.ZLabel zLabel26;
		Enterprise.ZArchitecture.ZTextBox BillingTermsTextBox;
		Enterprise.ZArchitecture.GUI.ZGroupBox LeadPackageGroupBox;
		Enterprise.ZArchitecture.ZLabel WaybillNumberLabel;
		Enterprise.ZArchitecture.ZTextBox WaybillNumberTextBox;
		Enterprise.ZArchitecture.ZTextBox WaybillShortNumberTextBox;
		Enterprise.ZArchitecture.ZLabel WayBillShortNumberLabel;
		protected Enterprise.ZArchitecture.GUI.ZGroupBox zGroupBox7;
		Enterprise.ZArchitecture.ZLabel zLabel27;
		Enterprise.ZArchitecture.ZTextBox zTextBox1;
		Enterprise.ZArchitecture.ZTextBox zTextBox2;
		Enterprise.ZArchitecture.ZLabel zLabel28;
		Enterprise.ZArchitecture.ZLabel zLabel29;
		Enterprise.ZArchitecture.ZTextBox zTextBox3;
		Enterprise.ZArchitecture.ZTextBox zTextBox4;
		Enterprise.ZArchitecture.ZTextBox zTextBox5;
		Enterprise.ZArchitecture.ZTextBox zTextBox6;
		Enterprise.ZArchitecture.GUI.ZDateEdit zDateEdit1;
		Enterprise.ZArchitecture.GUI.ZDateEdit zDateEdit2;
		Enterprise.ZArchitecture.GUI.ZDateEdit zDateEdit3;
		Enterprise.ZArchitecture.ZLabel zLabel30;
		Enterprise.ZArchitecture.ZTextBox zTextBox7;
		protected Enterprise.ZArchitecture.GUI.ZTabPage FinanceTabPage;
		Enterprise.ZArchitecture.GUI.ZCheckBox zCheckBox1;
		Enterprise.ZArchitecture.GUI.ZTemplateTabControl zTabControl1;
		Enterprise.ZArchitecture.GUI.ZTabPage zTabPage1;
		Enterprise.ZArchitecture.GUI.ZTabPage zTabPage2;
		Enterprise.ZArchitecture.ZLabel zLabel2;
		Enterprise.ZArchitecture.ZLabel zLabel3;
		Enterprise.ZArchitecture.ZLabel zLabel4;
		Enterprise.ZArchitecture.ZLabel zLabel5;
		Enterprise.ZArchitecture.ZLabel zLabel6;
		Enterprise.ZArchitecture.ZLabel zLabel7;
		Enterprise.ZArchitecture.ZLabel zLabel8;
		Enterprise.ZArchitecture.ZLabel zLabel9;
		Enterprise.ZArchitecture.ZLabel zLabel19;
		Enterprise.ZArchitecture.ZLabel zLabel24;
		Enterprise.ZArchitecture.ZTextBox BillToContactTextBox;
		Enterprise.ZArchitecture.ZTextBox BillToStateTextBox;
		Enterprise.ZArchitecture.ZTextBox BillToAddress1TextBox;
		Enterprise.ZArchitecture.ZTextBox BillToAccountClassTextBox;
		Enterprise.ZArchitecture.ZTextBox BillToAccountNumberTextBox;
		Enterprise.ZArchitecture.ZTextBox BillToCityTextBox;
		PopupEditableZGuidFindBox BillToOrganisationFindBox;
		Enterprise.ZArchitecture.ZTextBox BillToPhoneTextBox;
		Enterprise.ZArchitecture.ZTextBox BillToPostCodeTextBox;
		Enterprise.ZArchitecture.ZTextBox BillToNameTextBox;
		Enterprise.ZArchitecture.ZTextBox BillToAddress2TextBox;
		Enterprise.ZArchitecture.ZLabel zLabel1;
		Enterprise.ZArchitecture.GUI.ZCodeFindBox BillToCountryFindBox;
		Enterprise.ZArchitecture.GUI.ZTemplateTabControl zTabControl2;
		Enterprise.ZArchitecture.GUI.ZTabPage zTabPage3;
		Enterprise.ZArchitecture.ZLabel zLabel35;
		Enterprise.ZArchitecture.GUI.ZAddressControl ConsigneeAddressFindBox;
		Enterprise.ZArchitecture.ZLabel zLabel39;
		Enterprise.ZArchitecture.ZLabel zLabel40;
		Enterprise.ZArchitecture.ZTextBox zTextBox8;
		Enterprise.ZArchitecture.ZTextBox zTextBox9;
		Enterprise.ZArchitecture.ZTextBox zTextBox10;
		Enterprise.ZArchitecture.ZLabel zLabel43;
		Enterprise.ZArchitecture.ZTextBox zTextBox11;
		Enterprise.ZArchitecture.ZLabel zLabel44;
		Enterprise.ZArchitecture.ZLabel zLabel45;
		Enterprise.ZArchitecture.ZLabel zLabel46;
		Enterprise.ZArchitecture.ZTextBox zTextBox12;
		Enterprise.ZArchitecture.ZTextBox zTextBox13;
		Enterprise.ZArchitecture.ZLabel zLabel47;
		Enterprise.ZArchitecture.ZLabel zLabel48;
		Enterprise.ZArchitecture.ZTextBox zTextBox14;
		Enterprise.ZArchitecture.GUI.ZCodeFindBox zCodeFindBox1;
		Enterprise.ZArchitecture.ZLabel zLabel49;
		Enterprise.ZArchitecture.ZTextBox zTextBox15;
		Enterprise.ZArchitecture.ZTextBox zTextBox16;
		Enterprise.ZArchitecture.ZLabel zLabel50;
		Enterprise.ZArchitecture.ZTextBox zTextBox17;
		Enterprise.ZArchitecture.GUI.ZTabPage zTabPage4;
		Enterprise.ZArchitecture.ZLabel zLabel51;
		Enterprise.ZArchitecture.ZLabel zLabel52;
		Enterprise.ZArchitecture.GUI.ZCodeFindBox zCodeFindBox2;
		Enterprise.ZArchitecture.ZLabel zLabel53;
		Enterprise.ZArchitecture.ZLabel zLabel54;
		Enterprise.ZArchitecture.ZLabel zLabel55;
		Enterprise.ZArchitecture.ZLabel zLabel56;
		Enterprise.ZArchitecture.ZLabel zLabel57;
		Enterprise.ZArchitecture.ZLabel zLabel58;
		Enterprise.ZArchitecture.ZLabel zLabel59;
		Enterprise.ZArchitecture.ZLabel zLabel60;
		Enterprise.ZArchitecture.ZTextBox zTextBox18;
		Enterprise.ZArchitecture.ZTextBox zTextBox19;
		Enterprise.ZArchitecture.ZTextBox zTextBox20;
		Enterprise.ZArchitecture.ZTextBox zTextBox21;
		Enterprise.ZArchitecture.ZTextBox zTextBox22;
		Enterprise.ZArchitecture.GUI.ZAddressControl ConsignorAddressFindBox;
		Enterprise.ZArchitecture.ZTextBox zTextBox23;
		Enterprise.ZArchitecture.ZTextBox zTextBox24;
		Enterprise.ZArchitecture.ZTextBox zTextBox25;
		Enterprise.ZArchitecture.ZTextBox zTextBox26;
		Enterprise.ZArchitecture.ZLabel zLabel61;
		Enterprise.ZArchitecture.ZTextBox zTextBox27;
		Enterprise.ZArchitecture.GUI.ZTabPage zTabPage5;
		Enterprise.ZArchitecture.GUI.ZCheckBox zCheckBox5;
		Enterprise.ZArchitecture.ZLabel zLabel62;
		Enterprise.ZArchitecture.ZLabel zLabel63;
		Enterprise.ZArchitecture.ZLabel zLabel64;
		Enterprise.ZArchitecture.ZTextBox zTextBox28;
		Enterprise.ZArchitecture.ZTextBox zTextBox29;
		Enterprise.ZArchitecture.ZLabel zLabel66;
		Enterprise.ZArchitecture.ZTextBox zTextBox30;
		Enterprise.ZArchitecture.ZTextBox zTextBox31;
		Enterprise.ZArchitecture.ZLabel zLabel67;
		Enterprise.ZArchitecture.ZLabel zLabel68;
		Enterprise.ZArchitecture.ZTextBox zTextBox32;
		Enterprise.ZArchitecture.ZLabel zLabel69;
		Enterprise.ZArchitecture.ZTextBox zTextBox33;
		Enterprise.ZArchitecture.ZTextBox zTextBox34;
		Enterprise.ZArchitecture.ZTextBox zTextBox35;
		Enterprise.ZArchitecture.ZLabel zLabel65;
		ZLabel zLabelRefundEnquiry;
		Enterprise.ZArchitecture.ZTextBox zTextBox36;
		Enterprise.ZArchitecture.GUI.ZDateEdit zDateEdit4;
		protected Enterprise.ZArchitecture.GUI.ZButton RefundEnquiryButton;
		protected Enterprise.ZArchitecture.GUI.ZCheckBox IsExcludedFromBISIWarningBoundCheckBox;
		protected Enterprise.ZArchitecture.ZTextBox BISIDownloadNotRequiredOrForcedCaptionTextBox;
		Enterprise.ZArchitecture.GUI.ZDropEdit HoldForCollectionDepotDropEdit;
		ZGroupBox ActionsGroupBox;
		internal ZButton PartPaymentButton;
		ZGroupBox HoldForCollectionGroupBox;
		ZLabel zLabel23;
		ZLabel HFCContactPhoneLabel;
		ZLabel HFCContactNameLabel;
		ZTextBox HFCContactPhoneTextBox;
		ZTextBox HFCContactNameTextBox;

		new void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.FinanceTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.HoldForCollectionGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.HFCContactPhoneTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.HFCContactNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.HFCContactNameLabel = new Enterprise.ZArchitecture.ZLabel();
			this.HFCContactPhoneLabel = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel23 = new Enterprise.ZArchitecture.ZLabel();
			this.zCheckBox1 = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.HoldForCollectionDepotDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ActionsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PartPaymentButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.RefundEnquiryButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zLabelRefundEnquiry = new Enterprise.ZArchitecture.ZLabel();
			this.zTabControl2 = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.zTabPage3 = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.zLabel35 = new Enterprise.ZArchitecture.ZLabel();
			this.ConsigneeAddressFindBox = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.zLabel39 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel40 = new Enterprise.ZArchitecture.ZLabel();
			this.zTextBox8 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox9 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox10 = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel43 = new Enterprise.ZArchitecture.ZLabel();
			this.zTextBox11 = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel44 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel45 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel46 = new Enterprise.ZArchitecture.ZLabel();
			this.zTextBox12 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox13 = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel47 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel48 = new Enterprise.ZArchitecture.ZLabel();
			this.zTextBox14 = new Enterprise.ZArchitecture.ZTextBox();
			this.zCodeFindBox1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.zLabel49 = new Enterprise.ZArchitecture.ZLabel();
			this.zTextBox15 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox16 = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel50 = new Enterprise.ZArchitecture.ZLabel();
			this.zTextBox17 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTabPage4 = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.zLabel51 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel52 = new Enterprise.ZArchitecture.ZLabel();
			this.zCodeFindBox2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.zLabel53 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel54 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel55 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel56 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel57 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel58 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel59 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel60 = new Enterprise.ZArchitecture.ZLabel();
			this.zTextBox18 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox19 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox20 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox21 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox22 = new Enterprise.ZArchitecture.ZTextBox();
			this.ConsignorAddressFindBox = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.zTextBox23 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox24 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox25 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox26 = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel61 = new Enterprise.ZArchitecture.ZLabel();
			this.zTextBox27 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTabPage5 = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.zLabel62 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel63 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel64 = new Enterprise.ZArchitecture.ZLabel();
			this.zTextBox28 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox29 = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel66 = new Enterprise.ZArchitecture.ZLabel();
			this.zTextBox30 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox31 = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel67 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel68 = new Enterprise.ZArchitecture.ZLabel();
			this.zTextBox32 = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel69 = new Enterprise.ZArchitecture.ZLabel();
			this.zTextBox33 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox34 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox35 = new Enterprise.ZArchitecture.ZTextBox();
			this.zCheckBox5 = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.zTabControl1 = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.zTabPage1 = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.zLabel8 = new Enterprise.ZArchitecture.ZLabel();
			this.BillToOrganisationFindBox = new Enterprise.Client.UPE.GUI.PopupEditableZGuidFindBox();
			this.zLabel9 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel4 = new Enterprise.ZArchitecture.ZLabel();
			this.BillToAddress1TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BillToAccountClassTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BillToAccountNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel24 = new Enterprise.ZArchitecture.ZLabel();
			this.BillToCityTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel19 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel5 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel2 = new Enterprise.ZArchitecture.ZLabel();
			this.BillToPostCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BillToNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel3 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel6 = new Enterprise.ZArchitecture.ZLabel();
			this.BillToAddress2TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BillToCountryFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.zLabel7 = new Enterprise.ZArchitecture.ZLabel();
			this.BillToStateTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BillToPhoneTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.BillToContactTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zTabPage2 = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.zLabel18 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel17 = new Enterprise.ZArchitecture.ZLabel();
			this.ImporterCountryFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.zLabel10 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel11 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel12 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel13 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel14 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel15 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel16 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel20 = new Enterprise.ZArchitecture.ZLabel();
			this.ImporterContactTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ImporterStateTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ImporterAddress1TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ImporterAccountNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ImporterCityTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ImporterOrganisationFindBox = new Enterprise.Client.UPE.GUI.PopupEditableZGuidFindBox();
			this.ImporterPhoneTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ImporterPostCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ImporterNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ImporterAddress2TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel25 = new Enterprise.ZArchitecture.ZLabel();
			this.ImporterAccountClassTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zGroupBox4 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zLabel21 = new Enterprise.ZArchitecture.ZLabel();
			this.InvoiceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TotalAmountDueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zLabel22 = new Enterprise.ZArchitecture.ZLabel();
			this.zGroupBox5 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.BisiUploadDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.BISIDownloadNotRequiredOrForcedCaptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.IsExcludedFromBISIWarningBoundCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.LineChargesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.zLabel42 = new Enterprise.ZArchitecture.ZLabel();
			this.BisiDownloadDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.zLabel41 = new Enterprise.ZArchitecture.ZLabel();
			this.zGroupBox6 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ChequeRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.zRadioButton2 = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.zRadioButton3 = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.zRadioButton4 = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.zRadioButton5 = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.zRadioButton6 = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.zRadioButton7 = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.zRadioButton8 = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.zRadioButton9 = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ArrivalDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.zLabel31 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel32 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel33 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel36 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel37 = new Enterprise.ZArchitecture.ZLabel();
			this.DescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DeliveryDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.PiecesLandedCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.WeightInKGCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zLabel34 = new Enterprise.ZArchitecture.ZLabel();
			this.CustomsValueCalcFindBox = new Enterprise.ZArchitecture.GUI.ZCalcFindBox();
			this.zLabel38 = new Enterprise.ZArchitecture.ZLabel();
			this.ShipmentTypeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel26 = new Enterprise.ZArchitecture.ZLabel();
			this.BillingTermsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel65 = new Enterprise.ZArchitecture.ZLabel();
			this.zTextBox36 = new Enterprise.ZArchitecture.ZTextBox();
			this.ChildPackagesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ChildPackagesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.LeadPackageGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.WaybillNumberLabel = new Enterprise.ZArchitecture.ZLabel();
			this.WaybillNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.WaybillShortNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.WayBillShortNumberLabel = new Enterprise.ZArchitecture.ZLabel();
			this.zGroupBox7 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zLabel27 = new Enterprise.ZArchitecture.ZLabel();
			this.zTextBox1 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox2 = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel28 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel29 = new Enterprise.ZArchitecture.ZLabel();
			this.zTextBox3 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox4 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox5 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox6 = new Enterprise.ZArchitecture.ZTextBox();
			this.zDateEdit1 = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.zDateEdit2 = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.zDateEdit3 = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.zLabel30 = new Enterprise.ZArchitecture.ZLabel();
			this.zTextBox7 = new Enterprise.ZArchitecture.ZTextBox();
			this.zDateEdit4 = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.MainTabControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.FinanceTabPage.SuspendLayout();
			this.HoldForCollectionGroupBox.SuspendLayout();
			this.ActionsGroupBox.SuspendLayout();
			this.zTabControl2.SuspendLayout();
			this.zTabPage3.SuspendLayout();
			this.zTabPage4.SuspendLayout();
			this.zTabPage5.SuspendLayout();
			this.zTabControl1.SuspendLayout();
			this.zTabPage1.SuspendLayout();
			this.zTabPage2.SuspendLayout();
			this.zGroupBox4.SuspendLayout();
			this.zGroupBox5.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.LineChargesGrid)).BeginInit();
			this.zGroupBox6.SuspendLayout();
			this.zGroupBox1.SuspendLayout();
			this.ChildPackagesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ChildPackagesGrid)).BeginInit();
			this.LeadPackageGroupBox.SuspendLayout();
			this.zGroupBox7.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.FinanceTabPage);
			this.MainTabControl.Controls.SetChildIndex(this.FinanceTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(984, 616, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(992);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.UPE.Business.Callout);
			// 
			// FinanceTabPage
			// 
			this.FinanceTabPage.Controls.Add(this.HoldForCollectionGroupBox);
			this.FinanceTabPage.Controls.Add(this.ActionsGroupBox);
			this.FinanceTabPage.Controls.Add(this.zTabControl2);
			this.FinanceTabPage.Controls.Add(this.zTabControl1);
			this.FinanceTabPage.Controls.Add(this.zGroupBox4);
			this.FinanceTabPage.Controls.Add(this.zGroupBox5);
			this.FinanceTabPage.Controls.Add(this.zGroupBox6);
			this.FinanceTabPage.Controls.Add(this.zGroupBox1);
			this.FinanceTabPage.Controls.Add(this.ChildPackagesGroupBox);
			this.FinanceTabPage.Controls.Add(this.LeadPackageGroupBox);
			this.FinanceTabPage.Controls.Add(this.zGroupBox7);
			this.FinanceTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.FinanceTabPage.Name = "FinanceTabPage";
			this.FinanceTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(984, 616, true);
			this.FinanceTabPage.TabIndex = 4;
			this.FinanceTabPage.Text = "Finance";
			// 
			// HoldForCollectionGroupBox
			// 
			this.HoldForCollectionGroupBox.Controls.Add(this.HFCContactPhoneTextBox);
			this.HoldForCollectionGroupBox.Controls.Add(this.HFCContactNameTextBox);
			this.HoldForCollectionGroupBox.Controls.Add(this.HFCContactNameLabel);
			this.HoldForCollectionGroupBox.Controls.Add(this.HFCContactPhoneLabel);
			this.HoldForCollectionGroupBox.Controls.Add(this.zLabel23);
			this.HoldForCollectionGroupBox.Controls.Add(this.zCheckBox1);
			this.HoldForCollectionGroupBox.Controls.Add(this.HoldForCollectionDepotDropEdit);
			this.HoldForCollectionGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(383, 544, true);
			this.HoldForCollectionGroupBox.Name = "HoldForCollectionGroupBox";
			this.HoldForCollectionGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(680, 42, true);
			this.HoldForCollectionGroupBox.TabIndex = 2610;
			this.HoldForCollectionGroupBox.TabStop = false;
			this.HoldForCollectionGroupBox.Text = "      Hold For Collection";
			// 
			// HFCContactPhoneTextBox
			// 
			this.BindingSource.SetBindingMember(this.HFCContactPhoneTextBox, "HFCContactPhoneNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Callout)(null)).HFCContactPhoneNumber)));
			this.HFCContactPhoneTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.HFCContactPhoneTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(455, 16, true);
			this.HFCContactPhoneTextBox.Name = "HFCContactPhoneTextBox";
			this.HFCContactPhoneTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 20, true);
			this.HFCContactPhoneTextBox.TabIndex = 2617;
			// 
			// HFCContactNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.HFCContactNameTextBox, "HFCContactName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Callout)(null)).HFCContactName)));
			this.HFCContactNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.HFCContactNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(217, 16, true);
			this.HFCContactNameTextBox.Name = "HFCContactNameTextBox";
			this.HFCContactNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 20, true);
			this.HFCContactNameTextBox.TabIndex = 2615;
			// 
			// HFCContactNameLabel
			// 
			this.HFCContactNameLabel.AutoSize = true;
			this.HFCContactNameLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(162, 19, true);
			this.HFCContactNameLabel.Name = "HFCContactNameLabel";
			this.HFCContactNameLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(49, 13, true);
			this.HFCContactNameLabel.TabIndex = 2614;
			this.HFCContactNameLabel.Text = "Contact:";
			// 
			// HFCContactPhoneLabel
			// 
			this.HFCContactPhoneLabel.AutoSize = true;
			this.HFCContactPhoneLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(408, 19, true);
			this.HFCContactPhoneLabel.Name = "HFCContactPhoneLabel";
			this.HFCContactPhoneLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(41, 13, true);
			this.HFCContactPhoneLabel.TabIndex = 2616;
			this.HFCContactPhoneLabel.Text = "Phone:";
			// 
			// zLabel23
			// 
			this.zLabel23.AutoSize = true;
			this.zLabel23.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 19, true);
			this.zLabel23.Name = "zLabel23";
			this.zLabel23.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(45, 13, true);
			this.zLabel23.TabIndex = 2612;
			this.zLabel23.Text = "Held at:";
			// 
			// zCheckBox1
			// 
			this.zCheckBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.zCheckBox1.BindTo = "IsHoldForCollection";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZBool)(((Enterprise.Client.UPE.Business.Callout)(null)).IsHoldForCollection)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.Callout)(null)).IsHoldForCollectionInfo)));
			this.zCheckBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.zCheckBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 0, true);
			this.zCheckBox1.Name = "zCheckBox1";
			this.zCheckBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 16, true);
			this.zCheckBox1.TabIndex = 2611;
			// 
			// HoldForCollectionDepotDropEdit
			// 
			this.HoldForCollectionDepotDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.HoldForCollectionDepotDropEdit.BindTo = "HoldForCollectDepot";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.Callout)(null)).HoldForCollectDepotInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Callout)(null)).HoldForCollectDepot)));
			this.HoldForCollectionDepotDropEdit.BindToList = "Lookups+HoldForCollectDepotList";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Client.UPE.Business.Callout)(null)).Lookups.HoldForCollectDepotList)));
			this.HoldForCollectionDepotDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(59, 16, true);
			this.HoldForCollectionDepotDropEdit.Name = "HoldForCollectionDepotDropEdit";
			this.HoldForCollectionDepotDropEdit.PreBoundMaxLength = 3;
			this.HoldForCollectionDepotDropEdit.ShowDescriptionBox = false;
			this.HoldForCollectionDepotDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(54, 20, true);
			this.HoldForCollectionDepotDropEdit.TabIndex = 2613;
			// 
			// ActionsGroupBox
			// 
			this.ActionsGroupBox.Controls.Add(this.PartPaymentButton);
			this.ActionsGroupBox.Controls.Add(this.RefundEnquiryButton);
			this.ActionsGroupBox.Controls.Add(this.zLabelRefundEnquiry);
			this.ActionsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(659, 316, true);
			this.ActionsGroupBox.Name = "ActionsGroupBox";
			this.ActionsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(126, 72, true);
			this.ActionsGroupBox.TabIndex = 2570;
			this.ActionsGroupBox.TabStop = false;
			this.ActionsGroupBox.Text = "Actions";
			// 
			// PartPaymentButton
			// 
			this.PartPaymentButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 46, true);
			this.PartPaymentButton.Name = "PartPaymentButton";
			this.PartPaymentButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(92, 23, true);
			this.PartPaymentButton.TabIndex = 2572;
			this.PartPaymentButton.Text = "Part Payment";
			this.PartPaymentButton.UseVisualStyleBackColor = true;
			this.PartPaymentButton.Click += new System.EventHandler(this.PartPaymentButton_Click);
			// 
			// RefundEnquiryButton
			// 
			this.RefundEnquiryButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 16, true);
			this.RefundEnquiryButton.Name = "RefundEnquiryButton";
			this.RefundEnquiryButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(92, 23, true);
			this.RefundEnquiryButton.TabIndex = 2571;
			this.RefundEnquiryButton.Text = "Refund Enquiry";
			this.RefundEnquiryButton.Click += new System.EventHandler(this.OnRefundEnquiryButton_Click);
			// 
			// zLabelRefundEnquiry
			// 
			this.zLabelRefundEnquiry.AutoSize = true;
			this.zLabelRefundEnquiry.BindTo = "Refund.T10_ControlNumber";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.Callout)(null)).Refund.T10_ControlNumberInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Callout)(null)).Refund.T10_ControlNumber)));
			this.zLabelRefundEnquiry.ForeColor = System.Drawing.Color.Blue;
			this.zLabelRefundEnquiry.IsFontBold = true;
			this.zLabelRefundEnquiry.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 24, true);
			this.zLabelRefundEnquiry.Name = "zLabelRefundEnquiry";
			this.zLabelRefundEnquiry.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 13, true);
			this.zLabelRefundEnquiry.TabIndex = 7;
			this.zLabelRefundEnquiry.Text = "RE<Number>";
			// 
			// zTabControl2
			// 
			this.zTabControl2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.zTabControl2.Controls.Add(this.zTabPage3);
			this.zTabControl2.Controls.Add(this.zTabPage4);
			this.zTabControl2.Controls.Add(this.zTabPage5);
			this.zTabControl2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 315, true);
			this.zTabControl2.Name = "zTabControl2";
			this.zTabControl2.SelectedIndex = 0;
			this.zTabControl2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(375, 284, true);
			this.zTabControl2.TabIndex = 2200;
			this.zTabControl2.TabOrderExtendedToTabPages = false;
			// 
			// zTabPage3
			// 
			this.zTabPage3.Controls.Add(this.zLabel35);
			this.zTabPage3.Controls.Add(this.ConsigneeAddressFindBox);
			this.zTabPage3.Controls.Add(this.zLabel39);
			this.zTabPage3.Controls.Add(this.zLabel40);
			this.zTabPage3.Controls.Add(this.zTextBox8);
			this.zTabPage3.Controls.Add(this.zTextBox9);
			this.zTabPage3.Controls.Add(this.zTextBox10);
			this.zTabPage3.Controls.Add(this.zLabel43);
			this.zTabPage3.Controls.Add(this.zTextBox11);
			this.zTabPage3.Controls.Add(this.zLabel44);
			this.zTabPage3.Controls.Add(this.zLabel45);
			this.zTabPage3.Controls.Add(this.zLabel46);
			this.zTabPage3.Controls.Add(this.zTextBox12);
			this.zTabPage3.Controls.Add(this.zTextBox13);
			this.zTabPage3.Controls.Add(this.zLabel47);
			this.zTabPage3.Controls.Add(this.zLabel48);
			this.zTabPage3.Controls.Add(this.zTextBox14);
			this.zTabPage3.Controls.Add(this.zCodeFindBox1);
			this.zTabPage3.Controls.Add(this.zLabel49);
			this.zTabPage3.Controls.Add(this.zTextBox15);
			this.zTabPage3.Controls.Add(this.zTextBox16);
			this.zTabPage3.Controls.Add(this.zLabel50);
			this.zTabPage3.Controls.Add(this.zTextBox17);
			this.zTabPage3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zTabPage3.Name = "zTabPage3";
			this.zTabPage3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 257, true);
			this.zTabPage3.TabIndex = 0;
			this.zTabPage3.Text = "Consignee";
			// 
			// zLabel35
			// 
			this.zLabel35.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 227, true);
			this.zLabel35.Name = "zLabel35";
			this.zLabel35.AutoSize = true;
			this.zLabel35.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(54, 25, true);
			this.zLabel35.TabIndex = 2222;
			this.zLabel35.Text = "Post Code";
			// 
			// ConsigneeAddressFindBox
			// 
			this.BindingSource.SetBindingMember(this.ConsigneeAddressFindBox, "CS_OA_ConsigneeAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.UPE.Business.Callout)(null)).CS_OA_ConsigneeAddress)));
			this.ConsigneeAddressFindBox.BindToOrgList = "Lookups+ConsigneeList";
			this.ConsigneeAddressFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 8, true);
			this.ConsigneeAddressFindBox.Name = "ConsigneeAddressFindBox";
			this.ConsigneeAddressFindBox.PopupCaption = "";
			this.ConsigneeAddressFindBox.ShowAddress = false;
			this.ConsigneeAddressFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.ConsigneeAddressFindBox.TabIndex = 2202;
			// 
			// zLabel39
			// 
			this.zLabel39.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 224, true);
			this.zLabel39.Name = "zLabel39";
			this.zLabel39.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 23, true);
			this.zLabel39.TabIndex = 2220;
			this.zLabel39.Text = "State";
			// 
			// zLabel40
			// 
			this.zLabel40.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 80, true);
			this.zLabel40.Name = "zLabel40";
			this.zLabel40.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 23, true);
			this.zLabel40.TabIndex = 2209;
			this.zLabel40.Text = "Name";
			// 
			// zTextBox8
			// 
			this.BindingSource.SetBindingMember(this.zTextBox8, "CS_ConsigneeStreet");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Callout)(null)).CS_ConsigneeStreet)));
			this.zTextBox8.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 128, true);
			this.zTextBox8.Name = "zTextBox8";
			this.zTextBox8.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(287, 20, true);
			this.zTextBox8.TabIndex = 2214;
			// 
			// zTextBox9
			// 
			this.BindingSource.SetBindingMember(this.zTextBox9, "Consignee+AccountClass");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Callout)(null)).Consignee.AccountClass)));
			this.zTextBox9.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(224, 32, true);
			this.zTextBox9.Name = "zTextBox9";
			this.zTextBox9.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 20, true);
			this.zTextBox9.TabIndex = 2206;
			// 
			// zTextBox10
			// 
			this.BindingSource.SetBindingMember(this.zTextBox10, "ConsigneeAccountNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Callout)(null)).ConsigneeAccountNum)));
			this.zTextBox10.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 32, true);
			this.zTextBox10.Name = "zTextBox10";
			this.zTextBox10.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
			this.zTextBox10.TabIndex = 2204;
			// 
			// zLabel43
			// 
			this.zLabel43.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(191, 32, true);
			this.zLabel43.Name = "zLabel43";
			this.zLabel43.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 23, true);
			this.zLabel43.TabIndex = 2205;
			this.zLabel43.Text = "Class";
			// 
			// zTextBox11
			// 
			this.BindingSource.SetBindingMember(this.zTextBox11, "CS_ConsigneeCity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Callout)(null)).CS_ConsigneeCity)));
			this.zTextBox11.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 176, true);
			this.zTextBox11.Name = "zTextBox11";
			this.zTextBox11.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(287, 20, true);
			this.zTextBox11.TabIndex = 2217;
			// 
			// zLabel44
			// 
			this.zLabel44.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 200, true);
			this.zLabel44.Name = "zLabel44";
			this.zLabel44.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 23, true);
			this.zLabel44.TabIndex = 2218;
			this.zLabel44.Text = "Country/Region";
			// 
			// zLabel45
			// 
			this.zLabel45.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 104, true);
			this.zLabel45.Name = "zLabel45";
			this.zLabel45.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 23, true);
			this.zLabel45.TabIndex = 2211;
			this.zLabel45.Text = "Phone";
			// 
			// zLabel46
			// 
			this.zLabel46.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 34, true);
			this.zLabel46.Name = "zLabel46";
			this.zLabel46.AutoSize = true;
			this.zLabel46.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 23, true);
			this.zLabel46.TabIndex = 2203;
			this.zLabel46.Text = "Account No";
			// 
			// zTextBox12
			// 
			this.BindingSource.SetBindingMember(this.zTextBox12, "CS_ConsigneePostcode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Callout)(null)).CS_ConsigneePostcode)));
			this.zTextBox12.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(208, 224, true);
			this.zTextBox12.Name = "zTextBox12";
			this.zTextBox12.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(151, 20, true);
			this.zTextBox12.TabIndex = 2223;
			// 
			// zTextBox13
			// 
			this.BindingSource.SetBindingMember(this.zTextBox13, "CS_ConsigneeName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Callout)(null)).CS_ConsigneeName)));
			this.zTextBox13.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 80, true);
			this.zTextBox13.Name = "zTextBox13";
			this.zTextBox13.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(287, 20, true);
			this.zTextBox13.TabIndex = 2210;
			// 
			// zLabel47
			// 
			this.zLabel47.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 56, true);
			this.zLabel47.Name = "zLabel47";
			this.zLabel47.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 23, true);
			this.zLabel47.TabIndex = 2207;
			this.zLabel47.Text = "Contact";
			// 
			// zLabel48
			// 
			this.zLabel48.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 128, true);
			this.zLabel48.Name = "zLabel48";
			this.zLabel48.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 23, true);
			this.zLabel48.TabIndex = 2213;
			this.zLabel48.Text = "Address";
			// 
			// zTextBox14
			// 
			this.BindingSource.SetBindingMember(this.zTextBox14, "CS_ConsigneeStreet2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Callout)(null)).CS_ConsigneeStreet2)));
			this.zTextBox14.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 152, true);
			this.zTextBox14.Name = "zTextBox14";
			this.zTextBox14.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(287, 20, true);
			this.zTextBox14.TabIndex = 2215;
			// 
			// zCodeFindBox1
			// 
			this.zCodeFindBox1.BindTo = "CS_RN_NKConsigneeCountry";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.Callout)(null)).CS_RN_NKConsigneeCountryInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Callout)(null)).CS_RN_NKConsigneeCountry)));
			this.zCodeFindBox1.BindToList = "Lookups+ConsigneeCountryList";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Client.UPE.Business.Callout)(null)).Lookups.ConsigneeCountryList)));
			this.zCodeFindBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 200, true);
			this.zCodeFindBox1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
			this.zCodeFindBox1.Name = "zCodeFindBox1";
			this.zCodeFindBox1.PreBoundMaxLength = 2;
			this.zCodeFindBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(287, 20, true);
			this.zCodeFindBox1.TabIndex = 2219;
			// 
			// zLabel49
			// 
			this.zLabel49.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 176, true);
			this.zLabel49.Name = "zLabel49";
			this.zLabel49.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 23, true);
			this.zLabel49.TabIndex = 2216;
			this.zLabel49.Text = "City";
			// 
			// zTextBox15
			// 
			this.BindingSource.SetBindingMember(this.zTextBox15, "CS_ConsigneeState");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Callout)(null)).CS_ConsigneeState)));
			this.zTextBox15.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 224, true);
			this.zTextBox15.Name = "zTextBox15";
			this.zTextBox15.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			this.zTextBox15.TabIndex = 2221;
			// 
			// zTextBox16
			// 
			this.BindingSource.SetBindingMember(this.zTextBox16, "CS_ConsigneePhone");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Callout)(null)).CS_ConsigneePhone)));
			this.zTextBox16.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 104, true);
			this.zTextBox16.Name = "zTextBox16";
			this.zTextBox16.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(287, 20, true);
			this.zTextBox16.TabIndex = 2212;
			// 
			// zLabel50
			// 
			this.zLabel50.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.zLabel50.Name = "zLabel50";
			this.zLabel50.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 23, true);
			this.zLabel50.TabIndex = 2201;
			this.zLabel50.Text = "Party";
			// 
			// zTextBox17
			// 
			this.BindingSource.SetBindingMember(this.zTextBox17, "CS_ConsigneeContactName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Callout)(null)).CS_ConsigneeContactName)));
			this.zTextBox17.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 56, true);
			this.zTextBox17.Name = "zTextBox17";
			this.zTextBox17.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(287, 20, true);
			this.zTextBox17.TabIndex = 2208;
			// 
			// zTabPage4
			// 
			this.zTabPage4.Controls.Add(this.zLabel51);
			this.zTabPage4.Controls.Add(this.zLabel52);
			this.zTabPage4.Controls.Add(this.zCodeFindBox2);
			this.zTabPage4.Controls.Add(this.zLabel53);
			this.zTabPage4.Controls.Add(this.zLabel54);
			this.zTabPage4.Controls.Add(this.zLabel55);
			this.zTabPage4.Controls.Add(this.zLabel56);
			this.zTabPage4.Controls.Add(this.zLabel57);
			this.zTabPage4.Controls.Add(this.zLabel58);
			this.zTabPage4.Controls.Add(this.zLabel59);
			this.zTabPage4.Controls.Add(this.zLabel60);
			this.zTabPage4.Controls.Add(this.zTextBox18);
			this.zTabPage4.Controls.Add(this.zTextBox19);
			this.zTabPage4.Controls.Add(this.zTextBox20);
			this.zTabPage4.Controls.Add(this.zTextBox21);
			this.zTabPage4.Controls.Add(this.zTextBox22);
			this.zTabPage4.Controls.Add(this.ConsignorAddressFindBox);
			this.zTabPage4.Controls.Add(this.zTextBox23);
			this.zTabPage4.Controls.Add(this.zTextBox24);
			this.zTabPage4.Controls.Add(this.zTextBox25);
			this.zTabPage4.Controls.Add(this.zTextBox26);
			this.zTabPage4.Controls.Add(this.zLabel61);
			this.zTabPage4.Controls.Add(this.zTextBox27);
			this.zTabPage4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zTabPage4.Name = "zTabPage4";
			this.zTabPage4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 257, true);
			this.zTabPage4.TabIndex = 1;
			this.zTabPage4.Text = "Shipper";
			// 
			// zLabel51
			// 
			this.zLabel51.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 200, true);
			this.zLabel51.Name = "zLabel51";
			this.zLabel51.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 23, true);
			this.zLabel51.TabIndex = 2318;
			this.zLabel51.Text = "Country/Region";
			// 
			// zLabel52
			// 
			this.zLabel52.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 224, true);
			this.zLabel52.Name = "zLabel52";
			this.zLabel52.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 23, true);
			this.zLabel52.TabIndex = 2320;
			this.zLabel52.Text = "State";
			// 
			// zCodeFindBox2
			// 
			this.zCodeFindBox2.BindTo = "CS_RN_NKConsignorCountry";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.Callout)(null)).CS_RN_NKConsignorCountryInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Callout)(null)).CS_RN_NKConsignorCountry)));
			this.zCodeFindBox2.BindToList = "Lookups+ConsignorCountryList";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Client.UPE.Business.Callout)(null)).Lookups.ConsignorCountryList)));
			this.zCodeFindBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 200, true);
			this.zCodeFindBox2.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
			this.zCodeFindBox2.Name = "zCodeFindBox2";
			this.zCodeFindBox2.PreBoundMaxLength = 2;
			this.zCodeFindBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(287, 20, true);
			this.zCodeFindBox2.TabIndex = 2319;
			// 
			// zLabel53
			// 
			this.zLabel53.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 34, true);
			this.zLabel53.Name = "zLabel53";
			this.zLabel53.AutoSize = true;
			this.zLabel53.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(59, 23, true);
			this.zLabel53.TabIndex = 2303;
			this.zLabel53.Text = "Account No";
			// 
			// zLabel54
			// 
			this.zLabel54.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 56, true);
			this.zLabel54.Name = "zLabel54";
			this.zLabel54.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 23, true);
			this.zLabel54.TabIndex = 2307;
			this.zLabel54.Text = "Contact";
			// 
			// zLabel55
			// 
			this.zLabel55.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 80, true);
			this.zLabel55.Name = "zLabel55";
			this.zLabel55.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 23, true);
			this.zLabel55.TabIndex = 2309;
			this.zLabel55.Text = "Name";
			// 
			// zLabel56
			// 
			this.zLabel56.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 104, true);
			this.zLabel56.Name = "zLabel56";
			this.zLabel56.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 23, true);
			this.zLabel56.TabIndex = 2311;
			this.zLabel56.Text = "Phone";
			// 
			// zLabel57
			// 
			this.zLabel57.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 128, true);
			this.zLabel57.Name = "zLabel57";
			this.zLabel57.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 23, true);
			this.zLabel57.TabIndex = 2313;
			this.zLabel57.Text = "Address";
			// 
			// zLabel58
			// 
			this.zLabel58.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 176, true);
			this.zLabel58.Name = "zLabel58";
			this.zLabel58.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 23, true);
			this.zLabel58.TabIndex = 2316;
			this.zLabel58.Text = "City";
			// 
			// zLabel59
			// 
			this.zLabel59.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 224, true);
			this.zLabel59.Name = "zLabel59";
			this.zLabel59.AutoSize = true;
			this.zLabel59.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 25, true);
			this.zLabel59.TabIndex = 2322;
			this.zLabel59.Text = "Post Code";
			// 
			// zLabel60
			// 
			this.zLabel60.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(191, 34, true);
			this.zLabel60.Name = "zLabel60";
			this.zLabel60.AutoSize = true;
			this.zLabel60.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 23, true);
			this.zLabel60.TabIndex = 2305;
			this.zLabel60.Text = "Class";
			// 
			// zTextBox18
			// 
			this.BindingSource.SetBindingMember(this.zTextBox18, "CS_ConsignorContactName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Callout)(null)).CS_ConsignorContactName)));
			this.zTextBox18.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 56, true);
			this.zTextBox18.Name = "zTextBox18";
			this.zTextBox18.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(287, 20, true);
			this.zTextBox18.TabIndex = 2308;
			// 
			// zTextBox19
			// 
			this.BindingSource.SetBindingMember(this.zTextBox19, "CS_ConsignorState");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Callout)(null)).CS_ConsignorState)));
			this.zTextBox19.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 224, true);
			this.zTextBox19.Name = "zTextBox19";
			this.zTextBox19.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			this.zTextBox19.TabIndex = 2321;
			// 
			// zTextBox20
			// 
			this.BindingSource.SetBindingMember(this.zTextBox20, "CS_ConsignorStreet");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Callout)(null)).CS_ConsignorStreet)));
			this.zTextBox20.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 128, true);
			this.zTextBox20.Name = "zTextBox20";
			this.zTextBox20.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(287, 20, true);
			this.zTextBox20.TabIndex = 2314;
			// 
			// zTextBox21
			// 
			this.BindingSource.SetBindingMember(this.zTextBox21, "ConsignorAccountNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Callout)(null)).ConsignorAccountNum)));
			this.zTextBox21.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 32, true);
			this.zTextBox21.Name = "zTextBox21";
			this.zTextBox21.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
			this.zTextBox21.TabIndex = 2304;
			// 
			// zTextBox22
			// 
			this.BindingSource.SetBindingMember(this.zTextBox22, "CS_ConsignorCity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Callout)(null)).CS_ConsignorCity)));
			this.zTextBox22.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 176, true);
			this.zTextBox22.Name = "zTextBox22";
			this.zTextBox22.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(287, 20, true);
			this.zTextBox22.TabIndex = 2317;
			// 
			// ConsignorAddressFindBox
			// 
			this.BindingSource.SetBindingMember(this.ConsignorAddressFindBox, "CS_OA_ConsignorAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.UPE.Business.Callout)(null)).CS_OA_ConsignorAddress)));
			this.ConsignorAddressFindBox.BindToOrgList = "Lookups+ConsignorList";
			this.ConsignorAddressFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 8, true);
			this.ConsignorAddressFindBox.Name = "ConsignorAddressFindBox";
			this.ConsignorAddressFindBox.PopupCaption = "";
			this.ConsignorAddressFindBox.ShowAddress = false;
			this.ConsignorAddressFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.ConsignorAddressFindBox.TabIndex = 2302;
			// 
			// zTextBox23
			// 
			this.BindingSource.SetBindingMember(this.zTextBox23, "CS_ConsignorPhone");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Callout)(null)).CS_ConsignorPhone)));
			this.zTextBox23.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 104, true);
			this.zTextBox23.Name = "zTextBox23";
			this.zTextBox23.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(287, 20, true);
			this.zTextBox23.TabIndex = 2312;
			// 
			// zTextBox24
			// 
			this.BindingSource.SetBindingMember(this.zTextBox24, "CS_ConsignorPostcode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Callout)(null)).CS_ConsignorPostcode)));
			this.zTextBox24.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(208, 224, true);
			this.zTextBox24.Name = "zTextBox24";
			this.zTextBox24.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(151, 20, true);
			this.zTextBox24.TabIndex = 2323;
			// 
			// zTextBox25
			// 
			this.BindingSource.SetBindingMember(this.zTextBox25, "CS_ConsignorName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Callout)(null)).CS_ConsignorName)));
			this.zTextBox25.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 80, true);
			this.zTextBox25.Name = "zTextBox25";
			this.zTextBox25.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(287, 20, true);
			this.zTextBox25.TabIndex = 2310;
			// 
			// zTextBox26
			// 
			this.BindingSource.SetBindingMember(this.zTextBox26, "CS_ConsignorStreet2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Callout)(null)).CS_ConsignorStreet2)));
			this.zTextBox26.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 152, true);
			this.zTextBox26.Name = "zTextBox26";
			this.zTextBox26.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(287, 20, true);
			this.zTextBox26.TabIndex = 2315;
			// 
			// zLabel61
			// 
			this.zLabel61.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.zLabel61.Name = "zLabel61";
			this.zLabel61.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 23, true);
			this.zLabel61.TabIndex = 2301;
			this.zLabel61.Text = "Party";
			// 
			// zTextBox27
			// 
			this.BindingSource.SetBindingMember(this.zTextBox27, "Consignor+AccountClass");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Callout)(null)).Consignor.AccountClass)));
			this.zTextBox27.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(224, 32, true);
			this.zTextBox27.Name = "zTextBox27";
			this.zTextBox27.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 20, true);
			this.zTextBox27.TabIndex = 2306;
			// 
			// zTabPage5
			// 
			this.zTabPage5.Controls.Add(this.zLabel62);
			this.zTabPage5.Controls.Add(this.zLabel63);
			this.zTabPage5.Controls.Add(this.zLabel64);
			this.zTabPage5.Controls.Add(this.zTextBox28);
			this.zTabPage5.Controls.Add(this.zTextBox29);
			this.zTabPage5.Controls.Add(this.zLabel66);
			this.zTabPage5.Controls.Add(this.zTextBox30);
			this.zTabPage5.Controls.Add(this.zTextBox31);
			this.zTabPage5.Controls.Add(this.zLabel67);
			this.zTabPage5.Controls.Add(this.zLabel68);
			this.zTabPage5.Controls.Add(this.zTextBox32);
			this.zTabPage5.Controls.Add(this.zLabel69);
			this.zTabPage5.Controls.Add(this.zTextBox33);
			this.zTabPage5.Controls.Add(this.zTextBox34);
			this.zTabPage5.Controls.Add(this.zTextBox35);
			this.zTabPage5.Controls.Add(this.zCheckBox5);
			this.zTabPage5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zTabPage5.Name = "zTabPage5";
			this.zTabPage5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 257, true);
			this.zTabPage5.TabIndex = 2;
			this.zTabPage5.Text = "Delivery Address Redirection";
			// 
			// zLabel62
			// 
			this.zLabel62.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 184, true);
			this.zLabel62.Name = "zLabel62";
			this.zLabel62.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 23, true);
			this.zLabel62.TabIndex = 2415;
			this.zLabel62.Text = "Post Code";
			// 
			// zLabel63
			// 
			this.zLabel63.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 184, true);
			this.zLabel63.Name = "zLabel63";
			this.zLabel63.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 23, true);
			this.zLabel63.TabIndex = 2413;
			this.zLabel63.Text = "State";
			// 
			// zLabel64
			// 
			this.zLabel64.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 64, true);
			this.zLabel64.Name = "zLabel64";
			this.zLabel64.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 23, true);
			this.zLabel64.TabIndex = 2404;
			this.zLabel64.Text = "Name";
			// 
			// zTextBox28
			// 
			this.BindingSource.SetBindingMember(this.zTextBox28, "DeliveryAddressOverride+P3_Address1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Callout)(null)).DeliveryAddressOverride.P3_Address1)));
			this.zTextBox28.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 112, true);
			this.zTextBox28.Name = "zTextBox28";
			this.zTextBox28.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.zTextBox28.TabIndex = 2409;
			// 
			// zTextBox29
			// 
			this.BindingSource.SetBindingMember(this.zTextBox29, "DeliveryAddressOverride+P3_City");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Callout)(null)).DeliveryAddressOverride.P3_City)));
			this.zTextBox29.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 160, true);
			this.zTextBox29.Name = "zTextBox29";
			this.zTextBox29.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.zTextBox29.TabIndex = 2412;
			// 
			// zLabel66
			// 
			this.zLabel66.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 88, true);
			this.zLabel66.Name = "zLabel66";
			this.zLabel66.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 23, true);
			this.zLabel66.TabIndex = 2406;
			this.zLabel66.Text = "Phone";
			// 
			// zTextBox30
			// 
			this.BindingSource.SetBindingMember(this.zTextBox30, "DeliveryAddressOverride+P3_PostCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Callout)(null)).DeliveryAddressOverride.P3_PostCode)));
			this.zTextBox30.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(208, 184, true);
			this.zTextBox30.Name = "zTextBox30";
			this.zTextBox30.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.zTextBox30.TabIndex = 2416;
			// 
			// zTextBox31
			// 
			this.BindingSource.SetBindingMember(this.zTextBox31, "DeliveryAddressOverride+P3_CompanyName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Callout)(null)).DeliveryAddressOverride.P3_CompanyName)));
			this.zTextBox31.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 64, true);
			this.zTextBox31.Name = "zTextBox31";
			this.zTextBox31.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.zTextBox31.TabIndex = 2405;
			// 
			// zLabel67
			// 
			this.zLabel67.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 40, true);
			this.zLabel67.Name = "zLabel67";
			this.zLabel67.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 23, true);
			this.zLabel67.TabIndex = 2402;
			this.zLabel67.Text = "Contact";
			// 
			// zLabel68
			// 
			this.zLabel68.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 112, true);
			this.zLabel68.Name = "zLabel68";
			this.zLabel68.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 23, true);
			this.zLabel68.TabIndex = 2408;
			this.zLabel68.Text = "Address";
			// 
			// zTextBox32
			// 
			this.BindingSource.SetBindingMember(this.zTextBox32, "DeliveryAddressOverride+P3_Address2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Callout)(null)).DeliveryAddressOverride.P3_Address2)));
			this.zTextBox32.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 136, true);
			this.zTextBox32.Name = "zTextBox32";
			this.zTextBox32.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.zTextBox32.TabIndex = 2410;
			// 
			// zLabel69
			// 
			this.zLabel69.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 160, true);
			this.zLabel69.Name = "zLabel69";
			this.zLabel69.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 23, true);
			this.zLabel69.TabIndex = 2411;
			this.zLabel69.Text = "City";
			// 
			// zTextBox33
			// 
			this.BindingSource.SetBindingMember(this.zTextBox33, "DeliveryAddressOverride+P3_State");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Callout)(null)).DeliveryAddressOverride.P3_State)));
			this.zTextBox33.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 184, true);
			this.zTextBox33.Name = "zTextBox33";
			this.zTextBox33.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			this.zTextBox33.TabIndex = 2414;
			// 
			// zTextBox34
			// 
			this.BindingSource.SetBindingMember(this.zTextBox34, "DeliveryAddressOverride+P3_Phone");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Callout)(null)).DeliveryAddressOverride.P3_Phone)));
			this.zTextBox34.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 88, true);
			this.zTextBox34.Name = "zTextBox34";
			this.zTextBox34.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.zTextBox34.TabIndex = 2407;
			// 
			// zTextBox35
			// 
			this.BindingSource.SetBindingMember(this.zTextBox35, "DeliveryAddressOverride+P3_ContactName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Callout)(null)).DeliveryAddressOverride.P3_ContactName)));
			this.zTextBox35.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 40, true);
			this.zTextBox35.Name = "zTextBox35";
			this.zTextBox35.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.zTextBox35.TabIndex = 2403;
			// 
			// zCheckBox5
			// 
			this.zCheckBox5.BackColor = System.Drawing.SystemColors.Control;
			this.zCheckBox5.BindTo = "IsRedirected";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZBool)(((Enterprise.Client.UPE.Business.Callout)(null)).IsRedirected)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.Callout)(null)).IsRedirectedInfo)));
			this.zCheckBox5.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.zCheckBox5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.zCheckBox5.Name = "zCheckBox5";
			this.zCheckBox5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 24, true);
			this.zCheckBox5.TabIndex = 2401;
			this.zCheckBox5.Text = "Enable Redirection";
			this.zCheckBox5.UseVisualStyleBackColor = false;
			// 
			// zTabControl1
			// 
			this.zTabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.zTabControl1.Controls.Add(this.zTabPage1);
			this.zTabControl1.Controls.Add(this.zTabPage2);
			this.zTabControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 2, true);
			this.zTabControl1.Name = "zTabControl1";
			this.zTabControl1.SelectedIndex = 0;
			this.zTabControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(288, 292, true);
			this.zTabControl1.TabIndex = 2000;
			this.zTabControl1.TabOrderExtendedToTabPages = false;
			// 
			// zTabPage1
			// 
			this.zTabPage1.Controls.Add(this.zLabel8);
			this.zTabPage1.Controls.Add(this.BillToOrganisationFindBox);
			this.zTabPage1.Controls.Add(this.zLabel9);
			this.zTabPage1.Controls.Add(this.zLabel4);
			this.zTabPage1.Controls.Add(this.BillToAddress1TextBox);
			this.zTabPage1.Controls.Add(this.BillToAccountClassTextBox);
			this.zTabPage1.Controls.Add(this.BillToAccountNumberTextBox);
			this.zTabPage1.Controls.Add(this.zLabel24);
			this.zTabPage1.Controls.Add(this.BillToCityTextBox);
			this.zTabPage1.Controls.Add(this.zLabel19);
			this.zTabPage1.Controls.Add(this.zLabel5);
			this.zTabPage1.Controls.Add(this.zLabel2);
			this.zTabPage1.Controls.Add(this.BillToPostCodeTextBox);
			this.zTabPage1.Controls.Add(this.BillToNameTextBox);
			this.zTabPage1.Controls.Add(this.zLabel3);
			this.zTabPage1.Controls.Add(this.zLabel6);
			this.zTabPage1.Controls.Add(this.BillToAddress2TextBox);
			this.zTabPage1.Controls.Add(this.BillToCountryFindBox);
			this.zTabPage1.Controls.Add(this.zLabel7);
			this.zTabPage1.Controls.Add(this.BillToStateTextBox);
			this.zTabPage1.Controls.Add(this.BillToPhoneTextBox);
			this.zTabPage1.Controls.Add(this.zLabel1);
			this.zTabPage1.Controls.Add(this.BillToContactTextBox);
			this.zTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zTabPage1.Name = "zTabPage1";
			this.zTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 265, true);
			this.zTabPage1.TabIndex = 0;
			this.zTabPage1.Text = "Bill To";
			// 
			// zLabel8
			// 
			this.zLabel8.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 224, true);
			this.zLabel8.Name = "zLabel8";
			this.zLabel8.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 23, true);
			this.zLabel8.TabIndex = 2021;
			this.zLabel8.Text = "Post Code";
			// 
			// BillToOrganisationFindBox
			// 
			this.BillToOrganisationFindBox.BindTo = "BillToPK";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZGuid)(((Enterprise.Client.UPE.Business.Callout)(null)).BillToPK)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.Callout)(null)).BillToPKInfo)));
			this.BillToOrganisationFindBox.BindToList = "Lookups+BillToOrganisationList";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Client.UPE.Business.Callout)(null)).Lookups.BillToOrganisationList)));
			this.BillToOrganisationFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 8, true);
			this.BillToOrganisationFindBox.Name = "BillToOrganisationFindBox";
			this.BillToOrganisationFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.BillToOrganisationFindBox.TabIndex = 2002;
			// 
			// zLabel9
			// 
			this.zLabel9.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 224, true);
			this.zLabel9.Name = "zLabel9";
			this.zLabel9.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 21, true);
			this.zLabel9.TabIndex = 2020;
			this.zLabel9.Text = "State";
			// 
			// zLabel4
			// 
			this.zLabel4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 81, true);
			this.zLabel4.Name = "zLabel4";
			this.zLabel4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 21, true);
			this.zLabel4.TabIndex = 2009;
			this.zLabel4.Text = "Name";
			// 
			// BillToAddress1TextBox
			// 
			this.BindingSource.SetBindingMember(this.BillToAddress1TextBox, "BillTo+MainAddress+OA_Address1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Callout)(null)).BillTo.MainAddress.OA_Address1)));
			this.BillToAddress1TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 128, true);
			this.BillToAddress1TextBox.Name = "BillToAddress1TextBox";
			this.BillToAddress1TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.BillToAddress1TextBox.TabIndex = 2014;
			// 
			// BillToAccountClassTextBox
			// 
			this.BindingSource.SetBindingMember(this.BillToAccountClassTextBox, "BillTo+AccountClass");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Callout)(null)).BillTo.AccountClass)));
			this.BillToAccountClassTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(224, 32, true);
			this.BillToAccountClassTextBox.Name = "BillToAccountClassTextBox";
			this.BillToAccountClassTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 20, true);
			this.BillToAccountClassTextBox.TabIndex = 2105;
			// 
			// BillToAccountNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.BillToAccountNumberTextBox, "BillToAccountNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Callout)(null)).BillToAccountNumber)));
			this.BillToAccountNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 32, true);
			this.BillToAccountNumberTextBox.Name = "BillToAccountNumberTextBox";
			this.BillToAccountNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
			this.BillToAccountNumberTextBox.TabIndex = 2004;
			// 
			// zLabel24
			// 
			this.zLabel24.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(191, 34, true);
			this.zLabel24.Name = "zLabel24";
			this.zLabel24.AutoSize = true;
			this.zLabel24.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(29, 23, true);
			this.zLabel24.TabIndex = 2005;
			this.zLabel24.Text = "Class";
			// 
			// BillToCityTextBox
			// 
			this.BindingSource.SetBindingMember(this.BillToCityTextBox, "BillTo+MainAddress+OA_City");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Callout)(null)).BillTo.MainAddress.OA_City)));
			this.BillToCityTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 176, true);
			this.BillToCityTextBox.Name = "BillToCityTextBox";
			this.BillToCityTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.BillToCityTextBox.TabIndex = 2017;
			// 
			// zLabel19
			// 
			this.zLabel19.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 200, true);
			this.zLabel19.Name = "zLabel19";
			this.zLabel19.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 21, true);
			this.zLabel19.TabIndex = 2018;
			this.zLabel19.Text = "Country/Region";
			// 
			// zLabel5
			// 
			this.zLabel5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 105, true);
			this.zLabel5.Name = "zLabel5";
			this.zLabel5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 21, true);
			this.zLabel5.TabIndex = 2011;
			this.zLabel5.Text = "Phone";
			// 
			// zLabel2
			// 
			this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 35, true);
			this.zLabel2.Name = "zLabel2";
			this.zLabel2.AutoSize = true;
			this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 21, true);
			this.zLabel2.TabIndex = 2003;
			this.zLabel2.Text = "Account No";
			// 
			// BillToPostCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.BillToPostCodeTextBox, "BillTo+MainAddress+OA_PostCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Callout)(null)).BillTo.MainAddress.OA_PostCode)));
			this.BillToPostCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(208, 224, true);
			this.BillToPostCodeTextBox.Name = "BillToPostCodeTextBox";
			this.BillToPostCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.BillToPostCodeTextBox.TabIndex = 2023;
			// 
			// BillToNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.BillToNameTextBox, "BillTo+OH_FullName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Callout)(null)).BillTo.OH_FullName)));
			this.BillToNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 80, true);
			this.BillToNameTextBox.Name = "BillToNameTextBox";
			this.BillToNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.BillToNameTextBox.TabIndex = 2010;
			// 
			// zLabel3
			// 
			this.zLabel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 57, true);
			this.zLabel3.Name = "zLabel3";
			this.zLabel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 21, true);
			this.zLabel3.TabIndex = 2106;
			this.zLabel3.Text = "Contact";
			// 
			// zLabel6
			// 
			this.zLabel6.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 129, true);
			this.zLabel6.Name = "zLabel6";
			this.zLabel6.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 21, true);
			this.zLabel6.TabIndex = 2013;
			this.zLabel6.Text = "Address";
			// 
			// BillToAddress2TextBox
			// 
			this.BindingSource.SetBindingMember(this.BillToAddress2TextBox, "BillTo+MainAddress+OA_Address2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Callout)(null)).BillTo.MainAddress.OA_Address2)));
			this.BillToAddress2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 152, true);
			this.BillToAddress2TextBox.Name = "BillToAddress2TextBox";
			this.BillToAddress2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.BillToAddress2TextBox.TabIndex = 2015;
			// 
			// BillToCountryFindBox
			// 
			this.BillToCountryFindBox.BindTo = "BillTo+ClosestPort+Country+Code";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.Callout)(null)).BillTo.ClosestPort.Country.CodeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Callout)(null)).BillTo.ClosestPort.RL_RN_NKCountryCode)));
			this.BillToCountryFindBox.BindToList = "Lookups+BillToCountryList";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Client.UPE.Business.Callout)(null)).Lookups.BillToCountryList)));
			this.BillToCountryFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 200, true);
			this.BillToCountryFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
			this.BillToCountryFindBox.Name = "BillToCountryFindBox";
			this.BillToCountryFindBox.PreBoundMaxLength = 2;
			this.BillToCountryFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.BillToCountryFindBox.TabIndex = 2019;
			// 
			// zLabel7
			// 
			this.zLabel7.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 176, true);
			this.zLabel7.Name = "zLabel7";
			this.zLabel7.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 21, true);
			this.zLabel7.TabIndex = 2016;
			this.zLabel7.Text = "City";
			// 
			// BillToStateTextBox
			// 
			this.BindingSource.SetBindingMember(this.BillToStateTextBox, "BillTo+MainAddress+OA_State");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Callout)(null)).BillTo.MainAddress.OA_State)));
			this.BillToStateTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 224, true);
			this.BillToStateTextBox.Name = "BillToStateTextBox";
			this.BillToStateTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			this.BillToStateTextBox.TabIndex = 2020;
			// 
			// BillToPhoneTextBox
			// 
			this.BindingSource.SetBindingMember(this.BillToPhoneTextBox, "BillTo+MainAddress+OA_Phone");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Callout)(null)).BillTo.MainAddress.OA_Phone)));
			this.BillToPhoneTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 104, true);
			this.BillToPhoneTextBox.Name = "BillToPhoneTextBox";
			this.BillToPhoneTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.BillToPhoneTextBox.TabIndex = 2012;
			// 
			// zLabel1
			// 
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 9, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 21, true);
			this.zLabel1.TabIndex = 2001;
			this.zLabel1.Text = "Party";
			// 
			// BillToContactTextBox
			// 
			this.BindingSource.SetBindingMember(this.BillToContactTextBox, "BillTo+DefaultPayablesContactName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Callout)(null)).BillTo.DefaultPayablesContactName)));
			this.BillToContactTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 56, true);
			this.BillToContactTextBox.Name = "BillToContactTextBox";
			this.BillToContactTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.BillToContactTextBox.TabIndex = 2008;
			// 
			// zTabPage2
			// 
			this.zTabPage2.Controls.Add(this.zLabel18);
			this.zTabPage2.Controls.Add(this.zLabel17);
			this.zTabPage2.Controls.Add(this.ImporterCountryFindBox);
			this.zTabPage2.Controls.Add(this.zLabel10);
			this.zTabPage2.Controls.Add(this.zLabel11);
			this.zTabPage2.Controls.Add(this.zLabel12);
			this.zTabPage2.Controls.Add(this.zLabel13);
			this.zTabPage2.Controls.Add(this.zLabel14);
			this.zTabPage2.Controls.Add(this.zLabel15);
			this.zTabPage2.Controls.Add(this.zLabel16);
			this.zTabPage2.Controls.Add(this.zLabel20);
			this.zTabPage2.Controls.Add(this.ImporterContactTextBox);
			this.zTabPage2.Controls.Add(this.ImporterStateTextBox);
			this.zTabPage2.Controls.Add(this.ImporterAddress1TextBox);
			this.zTabPage2.Controls.Add(this.ImporterAccountNoTextBox);
			this.zTabPage2.Controls.Add(this.ImporterCityTextBox);
			this.zTabPage2.Controls.Add(this.ImporterOrganisationFindBox);
			this.zTabPage2.Controls.Add(this.ImporterPhoneTextBox);
			this.zTabPage2.Controls.Add(this.ImporterPostCodeTextBox);
			this.zTabPage2.Controls.Add(this.ImporterNameTextBox);
			this.zTabPage2.Controls.Add(this.ImporterAddress2TextBox);
			this.zTabPage2.Controls.Add(this.zLabel25);
			this.zTabPage2.Controls.Add(this.ImporterAccountClassTextBox);
			this.zTabPage2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zTabPage2.Name = "zTabPage2";
			this.zTabPage2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 265, true);
			this.zTabPage2.TabIndex = 1;
			this.zTabPage2.Text = "Importer";
			// 
			// zLabel18
			// 
			this.zLabel18.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 200, true);
			this.zLabel18.Name = "zLabel18";
			this.zLabel18.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 23, true);
			this.zLabel18.TabIndex = 2118;
			this.zLabel18.Text = "Country/Region";
			// 
			// zLabel17
			// 
			this.zLabel17.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 224, true);
			this.zLabel17.Name = "zLabel17";
			this.zLabel17.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 23, true);
			this.zLabel17.TabIndex = 2120;
			this.zLabel17.Text = "State";
			// 
			// ImporterCountryFindBox
			// 
			this.ImporterCountryFindBox.BindTo = "CusDecImporter+ClosestPort+Country+Code";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.Callout)(null)).CusDecImporter.ClosestPort.Country.CodeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Callout)(null)).CusDecImporter.ClosestPort.RL_RN_NKCountryCode)));
			this.ImporterCountryFindBox.BindToList = "Lookups+ImporterCountryList";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Client.UPE.Business.Callout)(null)).Lookups.ImporterCountryList)));
			this.ImporterCountryFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 200, true);
			this.ImporterCountryFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
			this.ImporterCountryFindBox.Name = "ImporterCountryFindBox";
			this.ImporterCountryFindBox.PreBoundMaxLength = 2;
			this.ImporterCountryFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.ImporterCountryFindBox.TabIndex = 2119;
			// 
			// zLabel10
			// 
			this.zLabel10.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 34, true);
			this.zLabel10.Name = "zLabel10";
			this.zLabel10.AutoSize = true;
			this.zLabel10.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 23, true);
			this.zLabel10.TabIndex = 2103;
			this.zLabel10.Text = "Account No";
			// 
			// zLabel11
			// 
			this.zLabel11.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 56, true);
			this.zLabel11.Name = "zLabel11";
			this.zLabel11.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 23, true);
			this.zLabel11.TabIndex = 2107;
			this.zLabel11.Text = "Contact";
			// 
			// zLabel12
			// 
			this.zLabel12.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 80, true);
			this.zLabel12.Name = "zLabel12";
			this.zLabel12.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 23, true);
			this.zLabel12.TabIndex = 2109;
			this.zLabel12.Text = "Name";
			// 
			// zLabel13
			// 
			this.zLabel13.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 104, true);
			this.zLabel13.Name = "zLabel13";
			this.zLabel13.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 23, true);
			this.zLabel13.TabIndex = 2111;
			this.zLabel13.Text = "Phone";
			// 
			// zLabel14
			// 
			this.zLabel14.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 128, true);
			this.zLabel14.Name = "zLabel14";
			this.zLabel14.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 23, true);
			this.zLabel14.TabIndex = 2113;
			this.zLabel14.Text = "Address";
			// 
			// zLabel15
			// 
			this.zLabel15.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 176, true);
			this.zLabel15.Name = "zLabel15";
			this.zLabel15.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 23, true);
			this.zLabel15.TabIndex = 2116;
			this.zLabel15.Text = "City";
			// 
			// zLabel16
			// 
			this.zLabel16.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 224, true);
			this.zLabel16.Name = "zLabel16";
			this.zLabel16.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 23, true);
			this.zLabel16.TabIndex = 2122;
			this.zLabel16.Text = "Post Code";
			// 
			// zLabel20
			// 
			this.zLabel20.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(191, 32, true);
			this.zLabel20.Name = "zLabel20";
			this.zLabel20.AutoSize = true;
			this.zLabel20.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(31, 25, true);
			this.zLabel20.TabIndex = 2105;
			this.zLabel20.Text = "Class";
			// 
			// ImporterContactTextBox
			// 
			this.BindingSource.SetBindingMember(this.ImporterContactTextBox, "CusDecImporter+DefaultImportAirFreightAgentContactName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Callout)(null)).CusDecImporter.DefaultImportAirFreightAgentContactName)));
			this.ImporterContactTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 56, true);
			this.ImporterContactTextBox.Name = "ImporterContactTextBox";
			this.ImporterContactTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.ImporterContactTextBox.TabIndex = 2108;
			// 
			// ImporterStateTextBox
			// 
			this.BindingSource.SetBindingMember(this.ImporterStateTextBox, "CusDecImporter+MainAddress+OA_State");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Callout)(null)).CusDecImporter.MainAddress.OA_State)));
			this.ImporterStateTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 224, true);
			this.ImporterStateTextBox.Name = "ImporterStateTextBox";
			this.ImporterStateTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			this.ImporterStateTextBox.TabIndex = 2121;
			// 
			// ImporterAddress1TextBox
			// 
			this.BindingSource.SetBindingMember(this.ImporterAddress1TextBox, "CusDecImporter+MainAddress+OA_Address1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Callout)(null)).CusDecImporter.MainAddress.OA_Address1)));
			this.ImporterAddress1TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 128, true);
			this.ImporterAddress1TextBox.Name = "ImporterAddress1TextBox";
			this.ImporterAddress1TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.ImporterAddress1TextBox.TabIndex = 2114;
			// 
			// ImporterAccountNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.ImporterAccountNoTextBox, "CusDecImporter+AccountNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Callout)(null)).CusDecImporter.AccountNumber)));
			this.ImporterAccountNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 32, true);
			this.ImporterAccountNoTextBox.Name = "ImporterAccountNoTextBox";
			this.ImporterAccountNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
			this.ImporterAccountNoTextBox.TabIndex = 2104;
			// 
			// ImporterCityTextBox
			// 
			this.BindingSource.SetBindingMember(this.ImporterCityTextBox, "CusDecImporter+MainAddress+OA_City");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Callout)(null)).CusDecImporter.MainAddress.OA_City)));
			this.ImporterCityTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 176, true);
			this.ImporterCityTextBox.Name = "ImporterCityTextBox";
			this.ImporterCityTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.ImporterCityTextBox.TabIndex = 2117;
			// 
			// ImporterOrganisationFindBox
			// 
			this.ImporterOrganisationFindBox.BindTo = "CusDecImporterPK";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZGuid)(((Enterprise.Client.UPE.Business.Callout)(null)).CusDecImporterPK)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.Callout)(null)).CusDecImporterPKInfo)));
			this.ImporterOrganisationFindBox.BindToList = "Lookups+ImporterOrganisationList";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Client.UPE.Business.Callout)(null)).Lookups.ImporterOrganisationList)));
			this.ImporterOrganisationFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 8, true);
			this.ImporterOrganisationFindBox.Name = "ImporterOrganisationFindBox";
			this.ImporterOrganisationFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.ImporterOrganisationFindBox.TabIndex = 2102;
			// 
			// ImporterPhoneTextBox
			// 
			this.BindingSource.SetBindingMember(this.ImporterPhoneTextBox, "CusDecImporter+MainAddress+OA_Phone");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Callout)(null)).CusDecImporter.MainAddress.OA_Phone)));
			this.ImporterPhoneTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 104, true);
			this.ImporterPhoneTextBox.Name = "ImporterPhoneTextBox";
			this.ImporterPhoneTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.ImporterPhoneTextBox.TabIndex = 2112;
			// 
			// ImporterPostCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.ImporterPostCodeTextBox, "CusDecImporter+MainAddress+OA_PostCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Callout)(null)).CusDecImporter.MainAddress.OA_PostCode)));
			this.ImporterPostCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(208, 224, true);
			this.ImporterPostCodeTextBox.Name = "ImporterPostCodeTextBox";
			this.ImporterPostCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.ImporterPostCodeTextBox.TabIndex = 2123;
			// 
			// ImporterNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.ImporterNameTextBox, "CusDecImporter+OH_FullName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Callout)(null)).CusDecImporter.OH_FullName)));
			this.ImporterNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 80, true);
			this.ImporterNameTextBox.Name = "ImporterNameTextBox";
			this.ImporterNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.ImporterNameTextBox.TabIndex = 2110;
			// 
			// ImporterAddress2TextBox
			// 
			this.BindingSource.SetBindingMember(this.ImporterAddress2TextBox, "CusDecImporter+MainAddress+OA_Address2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Callout)(null)).CusDecImporter.MainAddress.OA_Address2)));
			this.ImporterAddress2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 152, true);
			this.ImporterAddress2TextBox.Name = "ImporterAddress2TextBox";
			this.ImporterAddress2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.ImporterAddress2TextBox.TabIndex = 2115;
			// 
			// zLabel25
			// 
			this.zLabel25.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.zLabel25.Name = "zLabel25";
			this.zLabel25.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 23, true);
			this.zLabel25.TabIndex = 2101;
			this.zLabel25.Text = "Party";
			// 
			// ImporterAccountClassTextBox
			// 
			this.BindingSource.SetBindingMember(this.ImporterAccountClassTextBox, "CusDecImporter+AccountClass");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Callout)(null)).CusDecImporter.AccountClass)));
			this.ImporterAccountClassTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(224, 32, true);
			this.ImporterAccountClassTextBox.Name = "ImporterAccountClassTextBox";
			this.ImporterAccountClassTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 20, true);
			this.ImporterAccountClassTextBox.TabIndex = 2106;
			// 
			// zGroupBox4
			// 
			this.zGroupBox4.Controls.Add(this.zLabel21);
			this.zGroupBox4.Controls.Add(this.InvoiceNumberTextBox);
			this.zGroupBox4.Controls.Add(this.TotalAmountDueCalcEdit);
			this.zGroupBox4.Controls.Add(this.zLabel22);
			this.zGroupBox4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(296, 2, true);
			this.zGroupBox4.Name = "zGroupBox4";
			this.zGroupBox4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(680, 44, true);
			this.zGroupBox4.TabIndex = 2500;
			this.zGroupBox4.TabStop = false;
			this.zGroupBox4.Text = "Summary";
			// 
			// zLabel21
			// 
			this.zLabel21.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 16, true);
			this.zLabel21.Name = "zLabel21";
			this.zLabel21.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 23, true);
			this.zLabel21.TabIndex = 2501;
			this.zLabel21.Text = "BISI Invoice No";
			// 
			// InvoiceNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.InvoiceNumberTextBox, "InvoiceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Callout)(null)).InvoiceNumber)));
			this.InvoiceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 16, true);
			this.InvoiceNumberTextBox.Name = "InvoiceNumberTextBox";
			this.InvoiceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.InvoiceNumberTextBox.TabIndex = 2502;
			// 
			// TotalAmountDueCalcEdit
			// 
			this.TotalAmountDueCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.TotalAmountDueCalcEdit.BindTo = "TotalAmountDue";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.INumericZType)(((Enterprise.Client.UPE.Business.Callout)(null)).TotalAmountDue)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.Callout)(null)).TotalAmountDueInfo)));
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.INumericZType)(((Enterprise.Client.UPE.Business.Callout)(null)).TotalAmountDue)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.Callout)(null)).TotalAmountDueInfo)));
			this.TotalAmountDueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(320, 16, true);
			this.TotalAmountDueCalcEdit.Name = "TotalAmountDueCalcEdit";
			this.TotalAmountDueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.TotalAmountDueCalcEdit.TabIndex = 2504;
			this.TotalAmountDueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zLabel22
			// 
			this.zLabel22.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(216, 16, true);
			this.zLabel22.Name = "zLabel22";
			this.zLabel22.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(98, 23, true);
			this.zLabel22.TabIndex = 2503;
			this.zLabel22.Text = "Total Amount Due";
			// 
			// zGroupBox5
			// 
			this.zGroupBox5.Controls.Add(this.BisiUploadDateEdit);
			this.zGroupBox5.Controls.Add(this.BISIDownloadNotRequiredOrForcedCaptionTextBox);
			this.zGroupBox5.Controls.Add(this.IsExcludedFromBISIWarningBoundCheckBox);
			this.zGroupBox5.Controls.Add(this.LineChargesGrid);
			this.zGroupBox5.Controls.Add(this.zLabel42);
			this.zGroupBox5.Controls.Add(this.BisiDownloadDateEdit);
			this.zGroupBox5.Controls.Add(this.zLabel41);
			this.zGroupBox5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(296, 168, true);
			this.zGroupBox5.Name = "zGroupBox5";
			this.zGroupBox5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(582, 142, true);
			this.zGroupBox5.TabIndex = 2530;
			this.zGroupBox5.TabStop = false;
			this.zGroupBox5.Text = "Charges";
			// 
			// BisiUploadDateEdit
			// 
			this.BisiUploadDateEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BisiUploadDateEdit.AutoCompleteMonthThreshold = 1;
			this.BisiUploadDateEdit.AutoCompleteYear = true;
			this.BisiUploadDateEdit.BindTo = "BisiUploadDate";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZDateTime)(((Enterprise.Client.UPE.Business.Callout)(null)).BisiUploadDate)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.Callout)(null)).BisiUploadDateInfo)));
			this.BisiUploadDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.BisiUploadDateEdit.IsFixedReadOnly = false;
			this.BisiUploadDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(451, 34, true);
			this.BisiUploadDateEdit.Name = "BisiUploadDateEdit";
			this.BisiUploadDateEdit.TabIndex = 2532;
			// 
			// BISIDownloadNotRequiredOrForcedCaptionTextBox
			// 
			this.BISIDownloadNotRequiredOrForcedCaptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.BISIDownloadNotRequiredOrForcedCaptionTextBox, "BISIDownloadNotRequiredOrForcedCaption");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Callout)(null)).BISIDownloadNotRequiredOrForcedCaption)));
			this.BISIDownloadNotRequiredOrForcedCaptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(453, 77, true);
			this.BISIDownloadNotRequiredOrForcedCaptionTextBox.Name = "BISIDownloadNotRequiredOrForcedCaptionTextBox";
			this.BISIDownloadNotRequiredOrForcedCaptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 20, true);
			this.BISIDownloadNotRequiredOrForcedCaptionTextBox.TabIndex = 2534;
			this.BISIDownloadNotRequiredOrForcedCaptionTextBox.Text = "NOT REQUIRED";
			// 
			// IsExcludedFromBISIWarningBoundCheckBox
			// 
			this.IsExcludedFromBISIWarningBoundCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.IsExcludedFromBISIWarningBoundCheckBox.BindTo = "IsExcludedFromBISIWarning";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZBool)(((Enterprise.Client.UPE.Business.Callout)(null)).IsExcludedFromBISIWarning)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.Callout)(null)).IsExcludedFromBISIWarningInfo)));
			this.IsExcludedFromBISIWarningBoundCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsExcludedFromBISIWarningBoundCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(451, 103, true);
			this.IsExcludedFromBISIWarningBoundCheckBox.Name = "IsExcludedFromBISIWarningBoundCheckBox";
			this.IsExcludedFromBISIWarningBoundCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 32, true);
			this.IsExcludedFromBISIWarningBoundCheckBox.TabIndex = 2535;
			this.IsExcludedFromBISIWarningBoundCheckBox.Text = "Exclude From Warning Report";
			// 
			// LineChargesGrid
			// 
			this.LineChargesGrid.AllowNavigation = false;
			this.LineChargesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.LineChargesGrid.BindTo = "JobHeader+Charges";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Client.UPE.Business.Callout)(null)).JobHeader.Charges)));
			this.LineChargesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = "Desc";
			zTextBoxColumnStyleInfo1.ColumnName = "JR_Desc";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = "Amount";
			zCalcEditColumnStyleInfo1.ColumnName = "Amount";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.Caption = "Discount";
			zCalcEditColumnStyleInfo2.ColumnName = "Discount";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.Caption = "GST";
			zCalcEditColumnStyleInfo3.ColumnName = "GSTAmount";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.Caption = "Nett Amount";
			zCalcEditColumnStyleInfo4.ColumnName = "NettAmount";
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.Caption = "Non Taxable Amount";
			zCalcEditColumnStyleInfo5.ColumnName = "NonTaxableAmount";
			zCalcEditColumnStyleInfo5.IsVisible = false;
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.Caption = "Taxable Amount";
			zCalcEditColumnStyleInfo6.ColumnName = "TaxableAmount";
			zCalcEditColumnStyleInfo6.IsVisible = false;
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			this.LineChargesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.LineChargesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.LineChargesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.LineChargesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.LineChargesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.LineChargesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.LineChargesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.LineChargesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.LineChargesGrid.LayoutKey = "LineChargesGrid";
			this.LineChargesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.LineChargesGrid.Name = "LineChargesGrid";
			this.LineChargesGrid.ReadOnly = true;
			this.LineChargesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(442, 119, true);
			this.LineChargesGrid.TabIndex = 2531;
			// Compile time check for the above grid columns. If any of these lines fail, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.CalloutCharge)(((object)(((Enterprise.Client.UPE.Business.Callout)(null)).JobHeader.Charges)))).JR_DescInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.CalloutCharge)(((object)(((Enterprise.Client.UPE.Business.Callout)(null)).JobHeader.Charges)))).JR_Desc)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.INumericZType)(((Enterprise.Client.UPE.Business.CalloutCharge)(((object)(((Enterprise.Client.UPE.Business.Callout)(null)).JobHeader.Charges)))).Amount)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.CalloutCharge)(((object)(((Enterprise.Client.UPE.Business.Callout)(null)).JobHeader.Charges)))).AmountInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.INumericZType)(((Enterprise.Client.UPE.Business.CalloutCharge)(((object)(((Enterprise.Client.UPE.Business.Callout)(null)).JobHeader.Charges)))).Discount)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.CalloutCharge)(((object)(((Enterprise.Client.UPE.Business.Callout)(null)).JobHeader.Charges)))).DiscountInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.INumericZType)(((Enterprise.Client.UPE.Business.CalloutCharge)(((object)(((Enterprise.Client.UPE.Business.Callout)(null)).JobHeader.Charges)))).GSTAmount)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.CalloutCharge)(((object)(((Enterprise.Client.UPE.Business.Callout)(null)).JobHeader.Charges)))).GSTAmountInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.INumericZType)(((Enterprise.Client.UPE.Business.CalloutCharge)(((object)(((Enterprise.Client.UPE.Business.Callout)(null)).JobHeader.Charges)))).NettAmount)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.CalloutCharge)(((object)(((Enterprise.Client.UPE.Business.Callout)(null)).JobHeader.Charges)))).NettAmountInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.INumericZType)(((Enterprise.Client.UPE.Business.CalloutCharge)(((object)(((Enterprise.Client.UPE.Business.Callout)(null)).JobHeader.Charges)))).NonTaxableAmount)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.CalloutCharge)(((object)(((Enterprise.Client.UPE.Business.Callout)(null)).JobHeader.Charges)))).NonTaxableAmountInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.INumericZType)(((Enterprise.Client.UPE.Business.CalloutCharge)(((object)(((Enterprise.Client.UPE.Business.Callout)(null)).JobHeader.Charges)))).TaxableAmount)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.CalloutCharge)(((object)(((Enterprise.Client.UPE.Business.Callout)(null)).JobHeader.Charges)))).TaxableAmountInfo)));
			// 
			// zLabel42
			// 
			this.zLabel42.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.zLabel42.AutoSize = true;
			this.zLabel42.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(453, 61, true);
			this.zLabel42.Name = "zLabel42";
			this.zLabel42.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(124, 13, true);
			this.zLabel42.TabIndex = 2533;
			this.zLabel42.Text = "DL from BISI (Inv. Date)";
			// 
			// BisiDownloadDateEdit
			// 
			this.BisiDownloadDateEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BisiDownloadDateEdit.AutoCompleteMonthThreshold = 1;
			this.BisiDownloadDateEdit.AutoCompleteYear = true;
			this.BisiDownloadDateEdit.BindTo = "BisiDownloadDate";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZDateTime)(((Enterprise.Client.UPE.Business.Callout)(null)).BisiDownloadDate)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.Callout)(null)).BisiDownloadDateInfo)));
			this.BisiDownloadDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.BisiDownloadDateEdit.IsFixedReadOnly = false;
			this.BisiDownloadDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(453, 77, true);
			this.BisiDownloadDateEdit.Name = "BisiDownloadDateEdit";
			this.BisiDownloadDateEdit.TabIndex = 2;
			// 
			// zLabel41
			// 
			this.zLabel41.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.zLabel41.AutoSize = true;
			this.zLabel41.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(451, 16, true);
			this.zLabel41.Name = "zLabel41";
			this.zLabel41.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 13, true);
			this.zLabel41.TabIndex = 2532;
			this.zLabel41.Text = "Uploaded to BISI";
			// 
			// zGroupBox6
			// 
			this.zGroupBox6.Controls.Add(this.ChequeRadioButton);
			this.zGroupBox6.Controls.Add(this.zRadioButton2);
			this.zGroupBox6.Controls.Add(this.zRadioButton3);
			this.zGroupBox6.Controls.Add(this.zRadioButton4);
			this.zGroupBox6.Controls.Add(this.zRadioButton5);
			this.zGroupBox6.Controls.Add(this.zRadioButton6);
			this.zGroupBox6.Controls.Add(this.zRadioButton7);
			this.zGroupBox6.Controls.Add(this.zRadioButton8);
			this.zGroupBox6.Controls.Add(this.zRadioButton9);
			this.zGroupBox6.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(882, 48, true);
			this.zGroupBox6.Name = "zGroupBox6";
			this.zGroupBox6.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 262, true);
			this.zGroupBox6.TabIndex = 2540;
			this.zGroupBox6.TabStop = false;
			this.zGroupBox6.Text = "Payment";
			// 
			// ChequeRadioButton
			// 
			this.ChequeRadioButton.AutoCheck = false;
			this.ChequeRadioButton.BindTo = "Payment+IsCheque";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZBool)(((Enterprise.Client.UPE.Business.Callout)(null)).Payment.IsCheque)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.Callout)(null)).Payment.IsChequeInfo)));
			this.ChequeRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ChequeRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 26, true);
			this.ChequeRadioButton.Name = "ChequeRadioButton";
			this.ChequeRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 16, true);
			this.ChequeRadioButton.TabIndex = 2541;
			this.ChequeRadioButton.Text = "Cheque";
			// 
			// zRadioButton2
			// 
			this.zRadioButton2.AutoCheck = false;
			this.zRadioButton2.BindTo = "Payment+IsCreditCard";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZBool)(((Enterprise.Client.UPE.Business.Callout)(null)).Payment.IsCreditCard)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.Callout)(null)).Payment.IsCreditCardInfo)));
			this.zRadioButton2.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.zRadioButton2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 51, true);
			this.zRadioButton2.Name = "zRadioButton2";
			this.zRadioButton2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 16, true);
			this.zRadioButton2.TabIndex = 2542;
			this.zRadioButton2.Text = "Credit Card";
			// 
			// zRadioButton3
			// 
			this.zRadioButton3.AutoCheck = false;
			this.zRadioButton3.BindTo = "Payment+IsOther";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZBool)(((Enterprise.Client.UPE.Business.Callout)(null)).Payment.IsOther)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.Callout)(null)).Payment.IsOtherInfo)));
			this.zRadioButton3.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.zRadioButton3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 76, true);
			this.zRadioButton3.Name = "zRadioButton3";
			this.zRadioButton3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 16, true);
			this.zRadioButton3.TabIndex = 2543;
			this.zRadioButton3.Text = "Other";
			// 
			// zRadioButton4
			// 
			this.zRadioButton4.AutoCheck = false;
			this.zRadioButton4.BindTo = "Payment+IsPurchaseOrder";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZBool)(((Enterprise.Client.UPE.Business.Callout)(null)).Payment.IsPurchaseOrder)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.Callout)(null)).Payment.IsPurchaseOrderInfo)));
			this.zRadioButton4.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.zRadioButton4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 101, true);
			this.zRadioButton4.Name = "zRadioButton4";
			this.zRadioButton4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 16, true);
			this.zRadioButton4.TabIndex = 2544;
			this.zRadioButton4.Text = "P/O";
			// 
			// zRadioButton5
			// 
			this.zRadioButton5.AutoCheck = false;
			this.zRadioButton5.BindTo = "Payment+IsAccount";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZBool)(((Enterprise.Client.UPE.Business.Callout)(null)).Payment.IsAccount)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.Callout)(null)).Payment.IsAccountInfo)));
			this.zRadioButton5.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.zRadioButton5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 201, true);
			this.zRadioButton5.Name = "zRadioButton5";
			this.zRadioButton5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 16, true);
			this.zRadioButton5.TabIndex = 2548;
			this.zRadioButton5.Text = "Account";
			// 
			// zRadioButton6
			// 
			this.zRadioButton6.AutoCheck = false;
			this.zRadioButton6.BindTo = "Payment+IsEFT";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZBool)(((Enterprise.Client.UPE.Business.Callout)(null)).Payment.IsEFT)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.Callout)(null)).Payment.IsEFTInfo)));
			this.zRadioButton6.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.zRadioButton6.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 176, true);
			this.zRadioButton6.Name = "zRadioButton6";
			this.zRadioButton6.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 16, true);
			this.zRadioButton6.TabIndex = 2547;
			this.zRadioButton6.Text = "EFT";
			// 
			// zRadioButton7
			// 
			this.zRadioButton7.AutoCheck = false;
			this.zRadioButton7.BindTo = "Payment+IsBPay";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZBool)(((Enterprise.Client.UPE.Business.Callout)(null)).Payment.IsBPay)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.Callout)(null)).Payment.IsBPayInfo)));
			this.zRadioButton7.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.zRadioButton7.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 151, true);
			this.zRadioButton7.Name = "zRadioButton7";
			this.zRadioButton7.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 16, true);
			this.zRadioButton7.TabIndex = 2546;
			this.zRadioButton7.Text = "BPay";
			// 
			// zRadioButton8
			// 
			this.zRadioButton8.AutoCheck = false;
			this.zRadioButton8.BindTo = "Payment+IsNett7Day";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZBool)(((Enterprise.Client.UPE.Business.Callout)(null)).Payment.IsNett7Day)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.Callout)(null)).Payment.IsNett7DayInfo)));
			this.zRadioButton8.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.zRadioButton8.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 126, true);
			this.zRadioButton8.Name = "zRadioButton8";
			this.zRadioButton8.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 16, true);
			this.zRadioButton8.TabIndex = 2545;
			this.zRadioButton8.Text = "Nett 7 Day";
			// 
			// zRadioButton9
			// 
			this.zRadioButton9.AutoCheck = false;
			this.zRadioButton9.BindTo = "Payment+IsNone";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZBool)(((Enterprise.Client.UPE.Business.Callout)(null)).Payment.IsNone)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.Callout)(null)).Payment.IsNoneInfo)));
			this.zRadioButton9.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.zRadioButton9.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 226, true);
			this.zRadioButton9.Name = "zRadioButton9";
			this.zRadioButton9.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 16, true);
			this.zRadioButton9.TabIndex = 2549;
			this.zRadioButton9.Text = "None";
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.Controls.Add(this.ArrivalDateEdit);
			this.zGroupBox1.Controls.Add(this.zLabel31);
			this.zGroupBox1.Controls.Add(this.zLabel32);
			this.zGroupBox1.Controls.Add(this.zLabel33);
			this.zGroupBox1.Controls.Add(this.zLabel36);
			this.zGroupBox1.Controls.Add(this.zLabel37);
			this.zGroupBox1.Controls.Add(this.DescriptionTextBox);
			this.zGroupBox1.Controls.Add(this.DeliveryDateEdit);
			this.zGroupBox1.Controls.Add(this.PiecesLandedCalcEdit);
			this.zGroupBox1.Controls.Add(this.WeightInKGCalcEdit);
			this.zGroupBox1.Controls.Add(this.zLabel34);
			this.zGroupBox1.Controls.Add(this.CustomsValueCalcFindBox);
			this.zGroupBox1.Controls.Add(this.zLabel38);
			this.zGroupBox1.Controls.Add(this.ShipmentTypeTextBox);
			this.zGroupBox1.Controls.Add(this.zLabel26);
			this.zGroupBox1.Controls.Add(this.BillingTermsTextBox);
			this.zGroupBox1.Controls.Add(this.zLabel65);
			this.zGroupBox1.Controls.Add(this.zTextBox36);
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(383, 392, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 146, true);
			this.zGroupBox1.TabIndex = 2580;
			this.zGroupBox1.TabStop = false;
			this.zGroupBox1.Text = "Shipping Information";
			// 
			// ArrivalDateEdit
			// 
			this.ArrivalDateEdit.AutoCompleteMonthThreshold = 1;
			this.ArrivalDateEdit.AutoCompleteYear = true;
			this.ArrivalDateEdit.BindTo = "CS_ArrivalDate";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZDateTime)(((Enterprise.Client.UPE.Business.Callout)(null)).CS_ArrivalDate)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.Callout)(null)).CS_ArrivalDateInfo)));
			this.ArrivalDateEdit.IsFixedReadOnly = false;
			this.ArrivalDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 42, true);
			this.ArrivalDateEdit.Name = "ArrivalDateEdit";
			this.ArrivalDateEdit.TabIndex = 2584;
			// 
			// zLabel31
			// 
			this.zLabel31.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 16, true);
			this.zLabel31.Name = "zLabel31";
			this.zLabel31.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 23, true);
			this.zLabel31.TabIndex = 2581;
			this.zLabel31.Text = "Description";
			// 
			// zLabel32
			// 
			this.zLabel32.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 42, true);
			this.zLabel32.Name = "zLabel32";
			this.zLabel32.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 23, true);
			this.zLabel32.TabIndex = 2583;
			this.zLabel32.Text = "Arrival Date";
			// 
			// zLabel33
			// 
			this.zLabel33.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 68, true);
			this.zLabel33.Name = "zLabel33";
			this.zLabel33.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 23, true);
			this.zLabel33.TabIndex = 2587;
			this.zLabel33.Text = "Delivery Date";
			// 
			// zLabel36
			// 
			this.zLabel36.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 94, true);
			this.zLabel36.Name = "zLabel36";
			this.zLabel36.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 23, true);
			this.zLabel36.TabIndex = 2591;
			this.zLabel36.Text = "Pieces Landed";
			// 
			// zLabel37
			// 
			this.zLabel37.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(184, 94, true);
			this.zLabel37.Name = "zLabel37";
			this.zLabel37.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 23, true);
			this.zLabel37.TabIndex = 2593;
			this.zLabel37.Text = "Weight (KG)";
			// 
			// DescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.DescriptionTextBox, "CS_GoodsDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Callout)(null)).CS_GoodsDescription)));
			this.DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 16, true);
			this.DescriptionTextBox.Name = "DescriptionTextBox";
			this.DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(304, 20, true);
			this.DescriptionTextBox.TabIndex = 2582;
			// 
			// DeliveryDateEdit
			// 
			this.DeliveryDateEdit.AutoCompleteMonthThreshold = 1;
			this.DeliveryDateEdit.AutoCompleteYear = true;
			this.DeliveryDateEdit.BindTo = "DeliveryDate";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZDateTime)(((Enterprise.Client.UPE.Business.Callout)(null)).DeliveryDate)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.Callout)(null)).DeliveryDateInfo)));
			this.DeliveryDateEdit.IsFixedReadOnly = false;
			this.DeliveryDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 68, true);
			this.DeliveryDateEdit.Name = "DeliveryDateEdit";
			this.DeliveryDateEdit.TabIndex = 2588;
			// 
			// PiecesLandedCalcEdit
			// 
			this.PiecesLandedCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.PiecesLandedCalcEdit.BindTo = "CS_PiecesLanded";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.INumericZType)(((Enterprise.Client.UPE.Business.Callout)(null)).CS_PiecesLanded)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.Callout)(null)).CS_PiecesLandedInfo)));
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.INumericZType)(((Enterprise.Client.UPE.Business.Callout)(null)).CS_PiecesLanded)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.Callout)(null)).CS_PiecesLandedInfo)));
			this.PiecesLandedCalcEdit.Decimals = 0;
			this.PiecesLandedCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 94, true);
			this.PiecesLandedCalcEdit.Name = "PiecesLandedCalcEdit";
			this.PiecesLandedCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 20, true);
			this.PiecesLandedCalcEdit.TabIndex = 2592;
			this.PiecesLandedCalcEdit.Text = "0";
			this.PiecesLandedCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// WeightInKGCalcEdit
			// 
			this.WeightInKGCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.WeightInKGCalcEdit.BindTo = "WeightInKg";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.INumericZType)(((Enterprise.Client.UPE.Business.Callout)(null)).WeightInKg)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.Callout)(null)).WeightInKgInfo)));
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.INumericZType)(((Enterprise.Client.UPE.Business.Callout)(null)).WeightInKg)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.Callout)(null)).WeightInKgInfo)));
			this.WeightInKGCalcEdit.Decimals = 3;
			this.WeightInKGCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 94, true);
			this.WeightInKGCalcEdit.Name = "WeightInKGCalcEdit";
			this.WeightInKGCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 20, true);
			this.WeightInKGCalcEdit.TabIndex = 2594;
			this.WeightInKGCalcEdit.Text = "0.000";
			this.WeightInKGCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zLabel34
			// 
			this.zLabel34.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(184, 42, true);
			this.zLabel34.Name = "zLabel34";
			this.zLabel34.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 23, true);
			this.zLabel34.TabIndex = 2585;
			this.zLabel34.Text = "Customs Value";
			// 
			// CustomsValueCalcFindBox
			// 
			this.CustomsValueCalcFindBox.BindToAmount = "CS_GoodsValue";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZDecimal)(((Enterprise.Client.UPE.Business.Callout)(null)).CS_GoodsValue)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.Callout)(null)).CS_GoodsValueInfo)));
			this.CustomsValueCalcFindBox.BindToList = "Lookups+CurrencyList";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Client.UPE.Business.Callout)(null)).Lookups.CurrencyList)));
			this.CustomsValueCalcFindBox.BindToUnit = "CS_RX_NKGoodsCurrency";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((object)(((Enterprise.Client.UPE.Business.Callout)(null)).CS_RX_NKGoodsCurrency)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.Callout)(null)).CS_RX_NKGoodsCurrencyInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Callout)(null)).CS_RX_NKGoodsCurrency)));
			this.CustomsValueCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.CustomsValueCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 42, true);
			this.CustomsValueCalcFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
			this.CustomsValueCalcFindBox.Name = "CustomsValueCalcFindBox";
			this.CustomsValueCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 20, true);
			this.CustomsValueCalcFindBox.TabIndex = 2586;
			// 
			// zLabel38
			// 
			this.zLabel38.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(184, 68, true);
			this.zLabel38.Name = "zLabel38";
			this.zLabel38.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 23, true);
			this.zLabel38.TabIndex = 2589;
			this.zLabel38.Text = "Shipment Type";
			// 
			// ShipmentTypeTextBox
			// 
			this.BindingSource.SetBindingMember(this.ShipmentTypeTextBox, "ShipmentTypeReadOnly");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Callout)(null)).ShipmentTypeReadOnly)));
			this.ShipmentTypeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 68, true);
			this.ShipmentTypeTextBox.Name = "ShipmentTypeTextBox";
			this.ShipmentTypeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 20, true);
			this.ShipmentTypeTextBox.TabIndex = 2590;
			// 
			// zLabel26
			// 
			this.zLabel26.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.zLabel26.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 120, true);
			this.zLabel26.Name = "zLabel26";
			this.zLabel26.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 24, true);
			this.zLabel26.TabIndex = 2595;
			this.zLabel26.Text = "Billing Terms";
			// 
			// BillingTermsTextBox
			// 
			this.BillingTermsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.BillingTermsTextBox, "BillingTermsReadOnly");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Callout)(null)).BillingTermsReadOnly)));
			this.BillingTermsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 120, true);
			this.BillingTermsTextBox.Name = "BillingTermsTextBox";
			this.BillingTermsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 20, true);
			this.BillingTermsTextBox.TabIndex = 2596;
			// 
			// zLabel65
			// 
			this.zLabel65.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(184, 120, true);
			this.zLabel65.Name = "zLabel65";
			this.zLabel65.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 23, true);
			this.zLabel65.TabIndex = 2597;
			this.zLabel65.Text = "Duty Type";
			// 
			// zTextBox36
			// 
			this.BindingSource.SetBindingMember(this.zTextBox36, "DutyType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Callout)(null)).DutyType)));
			this.zTextBox36.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 120, true);
			this.zTextBox36.Name = "zTextBox36";
			this.zTextBox36.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 20, true);
			this.zTextBox36.TabIndex = 2598;
			// 
			// ChildPackagesGroupBox
			// 
			this.ChildPackagesGroupBox.Controls.Add(this.ChildPackagesGrid);
			this.ChildPackagesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(791, 316, true);
			this.ChildPackagesGroupBox.Name = "ChildPackagesGroupBox";
			this.ChildPackagesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 222, true);
			this.ChildPackagesGroupBox.TabIndex = 2600;
			this.ChildPackagesGroupBox.TabStop = false;
			this.ChildPackagesGroupBox.Text = "Child Packages";
			// 
			// ChildPackagesGrid
			// 
			this.ChildPackagesGrid.AllowNavigation = false;
			this.ChildPackagesGrid.BindTo = "ChildRelatedWayBills";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Client.UPE.Business.Callout)(null)).ChildRelatedWayBills)));
			this.ChildPackagesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo2.Caption = "Tracking Number";
			zTextBoxColumnStyleInfo2.ColumnName = "EB_WaybillNumber";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			zTextBoxColumnStyleInfo3.Caption = "Short Tracking Number";
			zTextBoxColumnStyleInfo3.ColumnName = "EB_WaybillShortNumber";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			this.ChildPackagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ChildPackagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ChildPackagesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ChildPackagesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ChildPackagesGrid.LayoutKey = "ChildPackagesGrid";
			this.ChildPackagesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ChildPackagesGrid.Name = "ChildPackagesGrid";
			this.ChildPackagesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(266, 203, true);
			this.ChildPackagesGrid.TabIndex = 2601;
			// Compile time check for the above grid columns. If any of these lines fail, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.UPEJobRelatedWayBill)(((object)(((Enterprise.Client.UPE.Business.Callout)(null)).ChildRelatedWayBills)))).EB_WaybillNumberInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.UPEJobRelatedWayBill)(((object)(((Enterprise.Client.UPE.Business.Callout)(null)).ChildRelatedWayBills)))).EB_WaybillNumber)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.UPEJobRelatedWayBill)(((object)(((Enterprise.Client.UPE.Business.Callout)(null)).ChildRelatedWayBills)))).EB_WaybillShortNumberInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.UPEJobRelatedWayBill)(((object)(((Enterprise.Client.UPE.Business.Callout)(null)).ChildRelatedWayBills)))).EB_WaybillShortNumber)));
			// 
			// LeadPackageGroupBox
			// 
			this.LeadPackageGroupBox.Controls.Add(this.WaybillNumberLabel);
			this.LeadPackageGroupBox.Controls.Add(this.WaybillNumberTextBox);
			this.LeadPackageGroupBox.Controls.Add(this.WaybillShortNumberTextBox);
			this.LeadPackageGroupBox.Controls.Add(this.WayBillShortNumberLabel);
			this.LeadPackageGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(383, 316, true);
			this.LeadPackageGroupBox.Name = "LeadPackageGroupBox";
			this.LeadPackageGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 72, true);
			this.LeadPackageGroupBox.TabIndex = 2560;
			this.LeadPackageGroupBox.TabStop = false;
			this.LeadPackageGroupBox.Text = "Lead Package";
			// 
			// WaybillNumberLabel
			// 
			this.WaybillNumberLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 16, true);
			this.WaybillNumberLabel.Name = "WaybillNumberLabel";
			this.WaybillNumberLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 23, true);
			this.WaybillNumberLabel.TabIndex = 2561;
			this.WaybillNumberLabel.Text = "Waybill No";
			// 
			// WaybillNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.WaybillNumberTextBox, "CS_HAWB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Callout)(null)).CS_HAWB)));
			this.WaybillNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 16, true);
			this.WaybillNumberTextBox.Name = "WaybillNumberTextBox";
			this.WaybillNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.WaybillNumberTextBox.TabIndex = 2562;
			// 
			// WaybillShortNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.WaybillShortNumberTextBox, "WayBillShort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Callout)(null)).WayBillShort)));
			this.WaybillShortNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 40, true);
			this.WaybillShortNumberTextBox.Name = "WaybillShortNumberTextBox";
			this.WaybillShortNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.WaybillShortNumberTextBox.TabIndex = 2564;
			// 
			// WayBillShortNumberLabel
			// 
			this.WayBillShortNumberLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 40, true);
			this.WayBillShortNumberLabel.Name = "WayBillShortNumberLabel";
			this.WayBillShortNumberLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 23, true);
			this.WayBillShortNumberLabel.TabIndex = 2563;
			this.WayBillShortNumberLabel.Text = "Waybill Short No";
			// 
			// zGroupBox7
			// 
			this.zGroupBox7.Controls.Add(this.zLabel27);
			this.zGroupBox7.Controls.Add(this.zTextBox1);
			this.zGroupBox7.Controls.Add(this.zTextBox2);
			this.zGroupBox7.Controls.Add(this.zLabel28);
			this.zGroupBox7.Controls.Add(this.zLabel29);
			this.zGroupBox7.Controls.Add(this.zTextBox3);
			this.zGroupBox7.Controls.Add(this.zTextBox4);
			this.zGroupBox7.Controls.Add(this.zTextBox5);
			this.zGroupBox7.Controls.Add(this.zTextBox6);
			this.zGroupBox7.Controls.Add(this.zDateEdit1);
			this.zGroupBox7.Controls.Add(this.zDateEdit2);
			this.zGroupBox7.Controls.Add(this.zDateEdit3);
			this.zGroupBox7.Controls.Add(this.zLabel30);
			this.zGroupBox7.Controls.Add(this.zTextBox7);
			this.zGroupBox7.Controls.Add(this.zDateEdit4);
			this.zGroupBox7.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(296, 48, true);
			this.zGroupBox7.Name = "zGroupBox7";
			this.zGroupBox7.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(582, 120, true);
			this.zGroupBox7.TabIndex = 2510;
			this.zGroupBox7.TabStop = false;
			this.zGroupBox7.Text = "Queues";
			// 
			// zLabel27
			// 
			this.zLabel27.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 43, true);
			this.zLabel27.Name = "zLabel27";
			this.zLabel27.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 21, true);
			this.zLabel27.TabIndex = 2514;
			this.zLabel27.Text = "Cargo Report";
			// 
			// zTextBox1
			// 
			this.BindingSource.SetBindingMember(this.zTextBox1, "CurrentQueue+CustomsQueueHeldOrCompleted");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Callout)(null)).CurrentQueue.CustomsQueueHeldOrCompleted)));
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 42, true);
			this.zTextBox1.Name = "zTextBox1";
			this.zTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			this.zTextBox1.TabIndex = 2515;
			// 
			// zTextBox2
			// 
			this.zTextBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.zTextBox2, "CurrentQueue+CustomsQueueSummary");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Callout)(null)).CurrentQueue.CustomsQueueSummary)));
			this.zTextBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(168, 42, true);
			this.zTextBox2.Name = "zTextBox2";
			this.zTextBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(294, 20, true);
			this.zTextBox2.TabIndex = 2516;
			// 
			// zLabel28
			// 
			this.zLabel28.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 69, true);
			this.zLabel28.Name = "zLabel28";
			this.zLabel28.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 21, true);
			this.zLabel28.TabIndex = 2518;
			this.zLabel28.Text = "Declaration";
			// 
			// zLabel29
			// 
			this.zLabel29.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 95, true);
			this.zLabel29.Name = "zLabel29";
			this.zLabel29.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 18, true);
			this.zLabel29.TabIndex = 2522;
			this.zLabel29.Text = "Finance";
			// 
			// zTextBox3
			// 
			this.BindingSource.SetBindingMember(this.zTextBox3, "DeclarationQueueHeldOrCompleted");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Callout)(null)).DeclarationQueueHeldOrCompleted)));
			this.zTextBox3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 68, true);
			this.zTextBox3.Name = "zTextBox3";
			this.zTextBox3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			this.zTextBox3.TabIndex = 2519;
			// 
			// zTextBox4
			// 
			this.BindingSource.SetBindingMember(this.zTextBox4, "CurrentQueue+CommercialQueueHeldOrCompleted");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Callout)(null)).CurrentQueue.CommercialQueueHeldOrCompleted)));
			this.zTextBox4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 94, true);
			this.zTextBox4.Name = "zTextBox4";
			this.zTextBox4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			this.zTextBox4.TabIndex = 2523;
			// 
			// zTextBox5
			// 
			this.zTextBox5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.zTextBox5, "DeclarationQueueSummary");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Callout)(null)).DeclarationQueueSummary)));
			this.zTextBox5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(168, 68, true);
			this.zTextBox5.Name = "zTextBox5";
			this.zTextBox5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(294, 20, true);
			this.zTextBox5.TabIndex = 2520;
			// 
			// zTextBox6
			// 
			this.zTextBox6.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.zTextBox6, "CurrentQueue+CommercialQueueSummary");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Callout)(null)).CurrentQueue.CommercialQueueSummary)));
			this.zTextBox6.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(168, 94, true);
			this.zTextBox6.Name = "zTextBox6";
			this.zTextBox6.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(294, 20, true);
			this.zTextBox6.TabIndex = 2525;
			// 
			// zDateEdit1
			// 
			this.zDateEdit1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.zDateEdit1.AutoCompleteMonthThreshold = 1;
			this.zDateEdit1.AutoCompleteYear = true;
			this.zDateEdit1.BindTo = "CurrentQueue+CustomsQueuedDate";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZDateTime)(((Enterprise.Client.UPE.Business.Callout)(null)).CurrentQueue.CustomsQueuedDate)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.Callout)(null)).CurrentQueue.CustomsQueuedDateInfo)));
			this.zDateEdit1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.zDateEdit1.IsFixedReadOnly = false;
			this.zDateEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(462, 42, true);
			this.zDateEdit1.Name = "zDateEdit1";
			this.zDateEdit1.TabIndex = 2517;
			// 
			// zDateEdit2
			// 
			this.zDateEdit2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.zDateEdit2.AutoCompleteMonthThreshold = 1;
			this.zDateEdit2.AutoCompleteYear = true;
			this.zDateEdit2.BindTo = "DeclarationQueuedDate";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZDateTime)(((Enterprise.Client.UPE.Business.Callout)(null)).DeclarationQueuedDate)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.Callout)(null)).DeclarationQueuedDateInfo)));
			this.zDateEdit2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.zDateEdit2.IsFixedReadOnly = false;
			this.zDateEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(462, 68, true);
			this.zDateEdit2.Name = "zDateEdit2";
			this.zDateEdit2.TabIndex = 2521;
			// 
			// zDateEdit3
			// 
			this.zDateEdit3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.zDateEdit3.AutoCompleteMonthThreshold = 1;
			this.zDateEdit3.AutoCompleteYear = true;
			this.zDateEdit3.BindTo = "CurrentQueue+CommercialQueuedDate";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZDateTime)(((Enterprise.Client.UPE.Business.Callout)(null)).CurrentQueue.CommercialQueuedDate)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.Callout)(null)).CurrentQueue.CommercialQueuedDateInfo)));
			this.zDateEdit3.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.zDateEdit3.IsFixedReadOnly = false;
			this.zDateEdit3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(462, 94, true);
			this.zDateEdit3.Name = "zDateEdit3";
			this.zDateEdit3.TabIndex = 2526;
			// 
			// zLabel30
			// 
			this.zLabel30.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 17, true);
			this.zLabel30.Name = "zLabel30";
			this.zLabel30.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 21, true);
			this.zLabel30.TabIndex = 2511;
			this.zLabel30.Text = "Resolution";
			// 
			// zTextBox7
			// 
			this.zTextBox7.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.zTextBox7, "CurrentQueue+Resolution");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Callout)(null)).CurrentQueue.Resolution)));
			this.zTextBox7.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 16, true);
			this.zTextBox7.Name = "zTextBox7";
			this.zTextBox7.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(366, 20, true);
			this.zTextBox7.TabIndex = 2512;
			this.zTextBox7.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// zDateEdit4
			// 
			this.zDateEdit4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.zDateEdit4.AutoCompleteMonthThreshold = 1;
			this.zDateEdit4.AutoCompleteYear = true;
			this.zDateEdit4.BindTo = "CurrentQueue+ResolutionUploadDateTime";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZDateTime)(((Enterprise.Client.UPE.Business.Callout)(null)).CurrentQueue.ResolutionUploadDateTime)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.Callout)(null)).CurrentQueue.ResolutionUploadDateTimeInfo)));
			this.zDateEdit4.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.zDateEdit4.IsFixedReadOnly = false;
			this.zDateEdit4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(462, 16, true);
			this.zDateEdit4.Name = "zDateEdit4";
			this.zDateEdit4.TabIndex = 2513;
			// 
			// CalloutForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1085, 720, true);
			this.DataSourceAssemblyName = "ZClientUPE";
			this.DataSourceType = typeof(Enterprise.Client.UPE.Business.Callout);
			this.DataSourceTypeName = "Enterprise.Client.UPE.Business.Callout";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1085, 737, true);
			this.Name = "CalloutForm";
			this.MainTabControl.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.FinanceTabPage.ResumeLayout(false);
			this.HoldForCollectionGroupBox.ResumeLayout(false);
			this.HoldForCollectionGroupBox.PerformLayout();
			this.ActionsGroupBox.ResumeLayout(false);
			this.ActionsGroupBox.PerformLayout();
			this.zTabControl2.ResumeLayout(false);
			this.zTabPage3.ResumeLayout(false);
			this.zTabPage3.PerformLayout();
			this.zTabPage4.ResumeLayout(false);
			this.zTabPage4.PerformLayout();
			this.zTabPage5.ResumeLayout(false);
			this.zTabPage5.PerformLayout();
			this.zTabControl1.ResumeLayout(false);
			this.zTabPage1.ResumeLayout(false);
			this.zTabPage1.PerformLayout();
			this.zTabPage2.ResumeLayout(false);
			this.zTabPage2.PerformLayout();
			this.zGroupBox4.ResumeLayout(false);
			this.zGroupBox4.PerformLayout();
			this.zGroupBox5.ResumeLayout(false);
			this.zGroupBox5.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.LineChargesGrid)).EndInit();
			this.zGroupBox6.ResumeLayout(false);
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			this.ChildPackagesGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ChildPackagesGrid)).EndInit();
			this.LeadPackageGroupBox.ResumeLayout(false);
			this.LeadPackageGroupBox.PerformLayout();
			this.zGroupBox7.ResumeLayout(false);
			this.zGroupBox7.PerformLayout();
			this.ResumeLayout(false);
		}
	}
}
