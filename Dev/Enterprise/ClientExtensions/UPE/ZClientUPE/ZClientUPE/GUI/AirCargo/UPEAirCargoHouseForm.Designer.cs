using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Client.UPE.Business;
using Enterprise.Core.Forms;
using Enterprise.Customs.AU.AirCargo.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.UPE.GUI
{
	public partial class UPEAirCargoHouseForm : AirCargoHouseForm
	{
		Enterprise.ZArchitecture.ZGrid ChildPackagesGrid;
		Enterprise.ZArchitecture.GUI.ZGroupBox ChildPackagesGroupBox;
		Enterprise.ZArchitecture.ZLabel WaybillNumberLabel;
		Enterprise.ZArchitecture.ZTextBox WaybillNumberTextBox;
		Enterprise.ZArchitecture.ZTextBox WaybillShortNumberTextBox;
		Enterprise.ZArchitecture.ZLabel WayBillShortNumberLabel;
		Enterprise.ZArchitecture.GUI.ZTabPage UPSTabPage;
		Enterprise.ZArchitecture.GUI.ZGroupBox zGroupBox1;
		Enterprise.ZArchitecture.GUI.ZGroupBox FlagGroupBox;
		Enterprise.ZArchitecture.ZLabel zLabel1;
		Enterprise.ZArchitecture.ZLabel zLabel2;
		Enterprise.ZArchitecture.ZLabel zLabel3;
		Enterprise.ZArchitecture.GUI.ZDropEdit zDropEdit1;
		Enterprise.ZArchitecture.GUI.ZDropEdit zDropEdit2;
		Enterprise.ZArchitecture.GUI.ZDropEdit zDropEdit3;
		Enterprise.ZArchitecture.GUI.ZCheckBox zCheckBox1;
		Enterprise.ZArchitecture.GUI.ZCheckBox zCheckBox2;
		Enterprise.ZArchitecture.GUI.ZCheckBox zCheckBox3;
		Enterprise.ZArchitecture.GUI.ZCheckBox zCheckBox4;
		Enterprise.ZArchitecture.GUI.ZGroupBox zGroupBox2;
		Enterprise.ZArchitecture.ZLabel zLabel4;
		Enterprise.ZArchitecture.ZLabel zLabel5;
		Enterprise.ZArchitecture.ZLabel zLabel6;
		Enterprise.ZArchitecture.ZLabel zLabel8;
		Enterprise.ZArchitecture.ZLabel zLabel10;
		Enterprise.ZArchitecture.ZLabel zLabel11;
		Enterprise.ZArchitecture.ZLabel zLabel12;
		Enterprise.ZArchitecture.GUI.ZCheckBox IsRedirectedCheckBox;
		Enterprise.ZArchitecture.ZTextBox zTextBox1;
		Enterprise.ZArchitecture.ZTextBox zTextBox3;
		Enterprise.ZArchitecture.ZTextBox DeliveryPostCodeTextBox;
		Enterprise.ZArchitecture.ZTextBox DeliveryStateTextBox;
		Enterprise.ZArchitecture.ZTextBox DeliveryPhoneTextBox;
		Enterprise.ZArchitecture.ZTextBox DeliveryAddress1TextBox;
		Enterprise.ZArchitecture.ZTextBox DeliveryAddress2TextBox;
		Enterprise.ZArchitecture.ZTextBox DeliveryCityTextBox;
		ZLabel zLabel9;
		ZTextBox Level1RecordConsignorAccountNumBoundTextBox;
		ZLabel zLabel7;
		ZTextBox Level1RecordConsigneeAccountNumBoundTextBox;
		ZCheckBox IsHoldForCollectionCheckBox;
		ZDropEdit HoldForCollectionDepotDropEdit;
		ZGroupBox HoldForCollectionGroupBox;
		ZLabel HoldForCollectionLabel;
		ZLabel HFCContactNameLabel;
		ZLabel HFCPhoneNumberLabel;
		ZTextBox HFCPhoneNumberTextBox;
		ZTextBox HFCContactNameTextBox;
		ZCheckBox zCheckBox5;
		ZTextBox PiecesLandedTextBox;
		ZLabel zLabel13;

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();

			UPSTabPage.RunWhenBindingOrFirstShown(((s, e) =>
			{
				IsHoldForCollectionCheckBox.AllowOutsideOfParent();
				zLabel5.AllowOverlap(IsRedirectedCheckBox);
				IsRedirectedCheckBox.AllowOutsideOfParent();
			}));
		}

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.UPSTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MainTabControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			this.SuspendLayout();
			//
			// MainTabControl
			//
			this.MainTabControl.Controls.Add(this.UPSTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(992, 631, true);
			this.MainTabControl.Controls.SetChildIndex(this.UPSTabPage, 0);
			//
			// MainTabPage
			//
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(984, 604, true);
			//
			// UPSTabPage
			//
			this.UPSTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.UPSTabPage.Name = "UPSTabPage";
			this.UPSTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(984, 604, true);
			this.UPSTabPage.TabIndex = 4;
			this.UPSTabPage.Text = "UPS";
			this.UPSTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.UPSTabPage_InitializeTab));
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.UPECusHAWB)(null)).CS_PiecesLandedInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.INumericZType)(((Enterprise.Client.UPE.Business.UPECusHAWB)(null)).CS_PiecesLanded)));
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.UPECusHAWB)(null)).HFCContactPhoneNumberInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.UPECusHAWB)(null)).HFCContactPhoneNumber)));
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.UPECusHAWB)(null)).HFCContactNameInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.UPECusHAWB)(null)).HFCContactName)));
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.UPECusHAWB)(null)).HoldForCollectDepotInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.UPECusHAWB)(null)).HoldForCollectDepot)));
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Client.UPE.Business.UPECusHAWB)(null)).Lookups.HoldForCollectDepotList)));
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZBool)(((Enterprise.Client.UPE.Business.UPECusHAWB)(null)).IsHoldForCollection)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.UPECusHAWB)(null)).IsHoldForCollectionInfo)));
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.UPECusHAWB)(null)).Level1RecordConsignorAccountNumInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.UPECusHAWB)(null)).Level1RecordConsignorAccountNum)));
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.UPECusHAWB)(null)).Level1RecordConsigneeAccountNumInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.UPECusHAWB)(null)).Level1RecordConsigneeAccountNum)));
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.UPECusHAWB)(null)).BillingTermsInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.UPECusHAWB)(null)).BillingTerms)));
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Client.UPE.Business.UPECusHAWB)(null)).Lookups.PrepaidCollectList)));
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZBool)(((Enterprise.Client.UPE.Business.UPECusHAWB)(null)).CS_IsSurplus)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.UPECusHAWB)(null)).CS_IsSurplusInfo)));
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZBool)(((Enterprise.Client.UPE.Business.UPECusHAWB)(null)).IsFreeDomicile)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.UPECusHAWB)(null)).IsFreeDomicileInfo)));
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZBool)(((Enterprise.Client.UPE.Business.UPECusHAWB)(null)).IsTranshipment)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.UPECusHAWB)(null)).IsTranshipmentInfo)));
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZBool)(((Enterprise.Client.UPE.Business.UPECusHAWB)(null)).IsAbandoned)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.UPECusHAWB)(null)).IsAbandonedInfo)));
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZBool)(((Enterprise.Client.UPE.Business.UPECusHAWB)(null)).IsRTS)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.UPECusHAWB)(null)).IsRTSInfo)));
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Client.UPE.Business.UPECusHAWB)(null)).ChildRelatedWayBills)));
			// Compile time check for the above grid columns. If any of these lines fail, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.UPEJobRelatedWayBill)(((object)(((Enterprise.Client.UPE.Business.UPECusHAWB)(null)).ChildRelatedWayBills)))).EB_WaybillNumberInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.UPEJobRelatedWayBill)(((object)(((Enterprise.Client.UPE.Business.UPECusHAWB)(null)).ChildRelatedWayBills)))).EB_WaybillNumber)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.UPEJobRelatedWayBill)(((object)(((Enterprise.Client.UPE.Business.UPECusHAWB)(null)).ChildRelatedWayBills)))).EB_WaybillShortNumberInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.UPEJobRelatedWayBill)(((object)(((Enterprise.Client.UPE.Business.UPECusHAWB)(null)).ChildRelatedWayBills)))).EB_WaybillShortNumber)));
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.UPECusHAWB)(null)).WayBillShortInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.UPECusHAWB)(null)).WayBillShort)));
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.UPECusHAWB)(null)).CS_HAWBInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.UPECusHAWB)(null)).CS_HAWB)));
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.UPECusHAWB)(null)).ShipmentTypeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.UPECusHAWB)(null)).ShipmentType)));
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Client.UPE.Business.UPECusHAWB)(null)).Lookups.ShipmentTypeList)));
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.UPECusHAWB)(null)).DutyTypeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.UPECusHAWB)(null)).DutyType)));
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Client.UPE.Business.UPECusHAWB)(null)).Lookups.DutyTypeList)));
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZBool)(((Enterprise.Client.UPE.Business.UPECusHAWB)(null)).IsRedirected)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.UPECusHAWB)(null)).IsRedirectedInfo)));
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.UPECusHAWB)(null)).DeliveryAddressOverride.P3_PostCodeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.UPECusHAWB)(null)).DeliveryAddressOverride.P3_PostCode)));
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.UPECusHAWB)(null)).DeliveryAddressOverride.P3_CompanyNameInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.UPECusHAWB)(null)).DeliveryAddressOverride.P3_CompanyName)));
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.UPECusHAWB)(null)).DeliveryAddressOverride.P3_StateInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.UPECusHAWB)(null)).DeliveryAddressOverride.P3_State)));
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.UPECusHAWB)(null)).DeliveryAddressOverride.P3_ContactNameInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.UPECusHAWB)(null)).DeliveryAddressOverride.P3_ContactName)));
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.UPECusHAWB)(null)).DeliveryAddressOverride.P3_Address1Info)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.UPECusHAWB)(null)).DeliveryAddressOverride.P3_Address1)));
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.UPECusHAWB)(null)).DeliveryAddressOverride.P3_Address2Info)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.UPECusHAWB)(null)).DeliveryAddressOverride.P3_Address2)));
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.UPECusHAWB)(null)).DeliveryAddressOverride.P3_CityInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.UPECusHAWB)(null)).DeliveryAddressOverride.P3_City)));
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.UPECusHAWB)(null)).DeliveryAddressOverride.P3_PhoneInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.UPECusHAWB)(null)).DeliveryAddressOverride.P3_Phone)));
			//
			// UPEAirCargoHouseForm
			//
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(992, 727, true);
			this.DataSourceAssemblyName = "ZClientUPE";
			this.DataSourceTypeName = "Enterprise.Client.UPE.Business.UPECusHAWB";
			this.Name = "UPEAirCargoHouseForm";
			this.ShouldSerializeTabPageMethods = true;
			this.MainTabControl.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			this.ResumeLayout(false);
		}

		void UPSTabPage_InitializeTab(object sender, EventArgs e)
		{
			//
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			//
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.PiecesLandedTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.HoldForCollectionGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.HFCPhoneNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.HFCContactNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.HFCPhoneNumberLabel = new Enterprise.ZArchitecture.ZLabel();
			this.HFCContactNameLabel = new Enterprise.ZArchitecture.ZLabel();
			this.HoldForCollectionLabel = new Enterprise.ZArchitecture.ZLabel();
			this.HoldForCollectionDepotDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.IsHoldForCollectionCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.zLabel9 = new Enterprise.ZArchitecture.ZLabel();
			this.Level1RecordConsignorAccountNumBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel7 = new Enterprise.ZArchitecture.ZLabel();
			this.Level1RecordConsigneeAccountNumBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zDropEdit1 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.FlagGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zCheckBox5 = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.zCheckBox1 = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.zCheckBox2 = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.zCheckBox3 = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.zCheckBox4 = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zLabel13 = new Enterprise.ZArchitecture.ZLabel();
			this.ChildPackagesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ChildPackagesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.WaybillNumberLabel = new Enterprise.ZArchitecture.ZLabel();
			this.WayBillShortNumberLabel = new Enterprise.ZArchitecture.ZLabel();
			this.WaybillShortNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.WaybillNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel2 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel3 = new Enterprise.ZArchitecture.ZLabel();
			this.zDropEdit2 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zDropEdit3 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zGroupBox2 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zLabel4 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel5 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel6 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel8 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel10 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel11 = new Enterprise.ZArchitecture.ZLabel();
			this.IsRedirectedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.DeliveryPostCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox3 = new Enterprise.ZArchitecture.ZTextBox();
			this.DeliveryStateTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox1 = new Enterprise.ZArchitecture.ZTextBox();
			this.DeliveryAddress1TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DeliveryAddress2TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DeliveryCityTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DeliveryPhoneTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel12 = new Enterprise.ZArchitecture.ZLabel();
			this.UPSTabPage.SuspendLayout();
			this.HoldForCollectionGroupBox.SuspendLayout();
			this.FlagGroupBox.SuspendLayout();
			this.zGroupBox1.SuspendLayout();
			this.ChildPackagesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ChildPackagesGrid)).BeginInit();
			this.zGroupBox2.SuspendLayout();
			this.UPSTabPage.Controls.Add(this.HoldForCollectionGroupBox);
			this.UPSTabPage.Controls.Add(this.zLabel9);
			this.UPSTabPage.Controls.Add(this.Level1RecordConsignorAccountNumBoundTextBox);
			this.UPSTabPage.Controls.Add(this.zLabel7);
			this.UPSTabPage.Controls.Add(this.Level1RecordConsigneeAccountNumBoundTextBox);
			this.UPSTabPage.Controls.Add(this.zDropEdit1);
			this.UPSTabPage.Controls.Add(this.FlagGroupBox);
			this.UPSTabPage.Controls.Add(this.zGroupBox1);
			this.UPSTabPage.Controls.Add(this.zLabel1);
			this.UPSTabPage.Controls.Add(this.zLabel2);
			this.UPSTabPage.Controls.Add(this.zLabel3);
			this.UPSTabPage.Controls.Add(this.zDropEdit2);
			this.UPSTabPage.Controls.Add(this.zDropEdit3);
			this.UPSTabPage.Controls.Add(this.zGroupBox2);
			//
			// PiecesLandedTextBox
			//
			this.PiecesLandedTextBox.BindTo = "CS_PiecesLanded";
			this.PiecesLandedTextBox.Enabled = false;
			this.PiecesLandedTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(355, 47);
			this.PiecesLandedTextBox.Name = "PiecesLandedTextBox";
			this.PiecesLandedTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(37, 20);
			this.PiecesLandedTextBox.TabIndex = 14;
			//
			// HoldForCollectionGroupBox
			//
			this.HoldForCollectionGroupBox.Controls.Add(this.HFCPhoneNumberTextBox);
			this.HoldForCollectionGroupBox.Controls.Add(this.HFCContactNameTextBox);
			this.HoldForCollectionGroupBox.Controls.Add(this.HFCPhoneNumberLabel);
			this.HoldForCollectionGroupBox.Controls.Add(this.HFCContactNameLabel);
			this.HoldForCollectionGroupBox.Controls.Add(this.HoldForCollectionLabel);
			this.HoldForCollectionGroupBox.Controls.Add(this.HoldForCollectionDepotDropEdit);
			this.HoldForCollectionGroupBox.Controls.Add(this.IsHoldForCollectionCheckBox);
			this.HoldForCollectionGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(531, 172);
			this.HoldForCollectionGroupBox.Name = "HoldForCollectionGroupBox";
			this.HoldForCollectionGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(205, 136);
			this.HoldForCollectionGroupBox.TabIndex = 13;
			this.HoldForCollectionGroupBox.TabStop = false;
			this.HoldForCollectionGroupBox.Text = "       Hold For Collection";
			//
			// HFCPhoneNumberTextBox
			//
			this.HFCPhoneNumberTextBox.BindTo = "HFCContactPhoneNumber";
			this.HFCPhoneNumberTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.HFCPhoneNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(57, 66);
			this.HFCPhoneNumberTextBox.Name = "HFCPhoneNumberTextBox";
			this.HFCPhoneNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20);
			this.HFCPhoneNumberTextBox.TabIndex = 16;
			//
			// HFCContactNameTextBox
			//
			this.HFCContactNameTextBox.BindTo = "HFCContactName";
			this.HFCContactNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.HFCContactNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(57, 42);
			this.HFCContactNameTextBox.Name = "HFCContactNameTextBox";
			this.HFCContactNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20);
			this.HFCContactNameTextBox.TabIndex = 15;
			//
			// HFCPhoneNumberLabel
			//
			this.HFCPhoneNumberLabel.AutoSize = true;
			this.HFCPhoneNumberLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 69);
			this.HFCPhoneNumberLabel.Name = "HFCPhoneNumberLabel";
			this.HFCPhoneNumberLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(41, 13);
			this.HFCPhoneNumberLabel.TabIndex = 14;
			this.HFCPhoneNumberLabel.Text = "Phone:";
			//
			// HFCContactNameLabel
			//
			this.HFCContactNameLabel.AutoSize = true;
			this.HFCContactNameLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 45);
			this.HFCContactNameLabel.Name = "HFCContactNameLabel";
			this.HFCContactNameLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(49, 13);
			this.HFCContactNameLabel.TabIndex = 13;
			this.HFCContactNameLabel.Text = "Contact:";
			//
			// HoldForCollectionLabel
			//
			this.HoldForCollectionLabel.AutoSize = true;
			this.HoldForCollectionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 21);
			this.HoldForCollectionLabel.Name = "HoldForCollectionLabel";
			this.HoldForCollectionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(45, 13);
			this.HoldForCollectionLabel.TabIndex = 12;
			this.HoldForCollectionLabel.Text = "Held at:";
			//
			// HoldForCollectionDepotDropEdit
			//
			this.HoldForCollectionDepotDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.HoldForCollectionDepotDropEdit.BindTo = "HoldForCollectDepot";
			this.HoldForCollectionDepotDropEdit.BindToList = "Lookups+HoldForCollectDepotList";
			this.HoldForCollectionDepotDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(57, 19);
			this.HoldForCollectionDepotDropEdit.Name = "HoldForCollectionDepotDropEdit";
			this.HoldForCollectionDepotDropEdit.PreBoundMaxLength = 3;
			this.HoldForCollectionDepotDropEdit.ShowDescriptionBox = false;
			this.HoldForCollectionDepotDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20);
			this.HoldForCollectionDepotDropEdit.TabIndex = 12;
			//
			// IsHoldForCollectionCheckBox
			//
			this.IsHoldForCollectionCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.IsHoldForCollectionCheckBox.BindTo = "IsHoldForCollection";
			this.IsHoldForCollectionCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsHoldForCollectionCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, -3);
			this.IsHoldForCollectionCheckBox.Name = "IsHoldForCollectionCheckBox";
			this.IsHoldForCollectionCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(17, 23);
			this.IsHoldForCollectionCheckBox.TabIndex = 11;
			//
			// zLabel9
			//
			this.zLabel9.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(416, 102);
			this.zLabel9.Name = "zLabel9";
			this.zLabel9.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(141, 23);
			this.zLabel9.TabIndex = 10;
			this.zLabel9.Text = "Screened Consignor Acc #:";
			//
			// Level1RecordConsignorAccountNumBoundTextBox
			//
			this.Level1RecordConsignorAccountNumBoundTextBox.BindTo = "Level1RecordConsignorAccountNum";
			this.Level1RecordConsignorAccountNumBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(559, 104);
			this.Level1RecordConsignorAccountNumBoundTextBox.Name = "Level1RecordConsignorAccountNumBoundTextBox";
			this.Level1RecordConsignorAccountNumBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20);
			this.Level1RecordConsignorAccountNumBoundTextBox.TabIndex = 3;
			//
			// zLabel7
			//
			this.zLabel7.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(416, 126);
			this.zLabel7.Name = "zLabel7";
			this.zLabel7.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(141, 23);
			this.zLabel7.TabIndex = 8;
			this.zLabel7.Text = "Screened Consignee Acc #:";
			//
			// Level1RecordConsigneeAccountNumBoundTextBox
			//
			this.Level1RecordConsigneeAccountNumBoundTextBox.BindTo = "Level1RecordConsigneeAccountNum";
			this.Level1RecordConsigneeAccountNumBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(559, 128);
			this.Level1RecordConsigneeAccountNumBoundTextBox.Name = "Level1RecordConsigneeAccountNumBoundTextBox";
			this.Level1RecordConsigneeAccountNumBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20);
			this.Level1RecordConsigneeAccountNumBoundTextBox.TabIndex = 4;
			//
			// zDropEdit1
			//
			this.zDropEdit1.BindTo = "BillingTerms";
			this.zDropEdit1.BindToList = "Lookups+PrepaidCollectList";
			this.zDropEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(559, 8);
			this.zDropEdit1.Name = "zDropEdit1";
			this.zDropEdit1.PreBoundMaxLength = 3;
			this.zDropEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20);
			this.zDropEdit1.TabIndex = 0;
			//
			// FlagGroupBox
			//
			this.FlagGroupBox.Controls.Add(this.zCheckBox5);
			this.FlagGroupBox.Controls.Add(this.zCheckBox1);
			this.FlagGroupBox.Controls.Add(this.zCheckBox2);
			this.FlagGroupBox.Controls.Add(this.zCheckBox3);
			this.FlagGroupBox.Controls.Add(this.zCheckBox4);
			this.FlagGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(416, 172);
			this.FlagGroupBox.Name = "FlagGroupBox";
			this.FlagGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(109, 136);
			this.FlagGroupBox.TabIndex = 6;
			this.FlagGroupBox.TabStop = false;
			this.FlagGroupBox.Text = "Options";
			//
			// zCheckBox5
			//
			this.zCheckBox5.BindTo = "CS_IsSurplus";
			this.zCheckBox5.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.zCheckBox5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 112);
			this.zCheckBox5.Name = "zCheckBox5";
			this.zCheckBox5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 20);
			this.zCheckBox5.TabIndex = 4;
			this.zCheckBox5.Text = "Surplus";
			//
			// zCheckBox1
			//
			this.zCheckBox1.BindTo = "IsFreeDomicile";
			this.zCheckBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.zCheckBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 18);
			this.zCheckBox1.Name = "zCheckBox1";
			this.zCheckBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 20);
			this.zCheckBox1.TabIndex = 0;
			this.zCheckBox1.Text = "Free Domicile";
			//
			// zCheckBox2
			//
			this.zCheckBox2.BindTo = "IsTranshipment";
			this.zCheckBox2.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.zCheckBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 42);
			this.zCheckBox2.Name = "zCheckBox2";
			this.zCheckBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 20);
			this.zCheckBox2.TabIndex = 1;
			this.zCheckBox2.Text = "Transhipment";
			//
			// zCheckBox3
			//
			this.zCheckBox3.BindTo = "IsAbandoned";
			this.zCheckBox3.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.zCheckBox3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 66);
			this.zCheckBox3.Name = "zCheckBox3";
			this.zCheckBox3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 20);
			this.zCheckBox3.TabIndex = 2;
			this.zCheckBox3.Text = "Abandon";
			//
			// zCheckBox4
			//
			this.zCheckBox4.BindTo = "IsRTS";
			this.zCheckBox4.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.zCheckBox4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 90);
			this.zCheckBox4.Name = "zCheckBox4";
			this.zCheckBox4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 20);
			this.zCheckBox4.TabIndex = 3;
			this.zCheckBox4.Text = "RTS";
			//
			// zGroupBox1
			//
			this.zGroupBox1.Controls.Add(this.PiecesLandedTextBox);
			this.zGroupBox1.Controls.Add(this.zLabel13);
			this.zGroupBox1.Controls.Add(this.ChildPackagesGroupBox);
			this.zGroupBox1.Controls.Add(this.WaybillNumberLabel);
			this.zGroupBox1.Controls.Add(this.WayBillShortNumberLabel);
			this.zGroupBox1.Controls.Add(this.WaybillShortNumberTextBox);
			this.zGroupBox1.Controls.Add(this.WaybillNumberTextBox);
			this.zGroupBox1.Dock = System.Windows.Forms.DockStyle.Left;
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(408, 604);
			this.zGroupBox1.TabIndex = 6;
			this.zGroupBox1.TabStop = false;
			this.zGroupBox1.Text = "Packages";
			//
			// zLabel13
			//
			this.zLabel13.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(273, 45);
			this.zLabel13.Name = "zLabel13";
			this.zLabel13.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(79, 23);
			this.zLabel13.TabIndex = 4;
			this.zLabel13.Text = "Pieces Landed: ";
			//
			// ChildPackagesGroupBox
			//
			this.ChildPackagesGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
									| System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right)));
			this.ChildPackagesGroupBox.Controls.Add(this.ChildPackagesGrid);
			this.ChildPackagesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 72);
			this.ChildPackagesGroupBox.Name = "ChildPackagesGroupBox";
			this.ChildPackagesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(392, 524);
			this.ChildPackagesGroupBox.TabIndex = 2;
			this.ChildPackagesGroupBox.TabStop = false;
			this.ChildPackagesGroupBox.Text = "Child Packages";
			//
			// ChildPackagesGrid
			//
			this.ChildPackagesGrid.AllowNavigation = false;
			this.ChildPackagesGrid.BindTo = "ChildRelatedWayBills";
			this.ChildPackagesGrid.CaptionVisible = false;
			this.ChildPackagesGrid.GridId = "825327db-4376-48d1-bd05-1a2dc0faa7c2";
			zTextBoxColumnStyleInfo1.Caption = "Waybill Number";
			zTextBoxColumnStyleInfo1.ColumnName = "EB_WaybillNumber";
			zTextBoxColumnStyleInfo2.Caption = "Waybill Short Number";
			zTextBoxColumnStyleInfo2.ColumnName = "EB_WaybillShortNumber";
			this.ChildPackagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ChildPackagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ChildPackagesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ChildPackagesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ChildPackagesGrid.LayoutKey = "ChildPackagesGrid";
			this.ChildPackagesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16);
			this.ChildPackagesGrid.Name = "ChildPackagesGrid";
			this.ChildPackagesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(386, 505);
			this.ChildPackagesGrid.TabIndex = 0;
			//
			// WaybillNumberLabel
			//
			this.WaybillNumberLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 19);
			this.WaybillNumberLabel.Name = "WaybillNumberLabel";
			this.WaybillNumberLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 23);
			this.WaybillNumberLabel.TabIndex = 3;
			this.WaybillNumberLabel.Text = "Waybill Number:";
			//
			// WayBillShortNumberLabel
			//
			this.WayBillShortNumberLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 43);
			this.WayBillShortNumberLabel.Name = "WayBillShortNumberLabel";
			this.WayBillShortNumberLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 23);
			this.WayBillShortNumberLabel.TabIndex = 3;
			this.WayBillShortNumberLabel.Text = "Waybill Short Number:";
			//
			// WaybillShortNumberTextBox
			//
			this.WaybillShortNumberTextBox.BindTo = "WayBillShort";
			this.WaybillShortNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 45);
			this.WaybillShortNumberTextBox.Name = "WaybillShortNumberTextBox";
			this.WaybillShortNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20);
			this.WaybillShortNumberTextBox.TabIndex = 1;
			//
			// WaybillNumberTextBox
			//
			this.WaybillNumberTextBox.BindTo = "CS_HAWB";
			this.WaybillNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 21);
			this.WaybillNumberTextBox.Name = "WaybillNumberTextBox";
			this.WaybillNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(264, 20);
			this.WaybillNumberTextBox.TabIndex = 0;
			//
			// zLabel1
			//
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(416, 8);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 23);
			this.zLabel1.TabIndex = 3;
			this.zLabel1.Text = "Billing Terms:";
			//
			// zLabel2
			//
			this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(416, 56);
			this.zLabel2.Name = "zLabel2";
			this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 23);
			this.zLabel2.TabIndex = 3;
			this.zLabel2.Text = "Duty Type:";
			//
			// zLabel3
			//
			this.zLabel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(416, 32);
			this.zLabel3.Name = "zLabel3";
			this.zLabel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 23);
			this.zLabel3.TabIndex = 3;
			this.zLabel3.Text = "Shipment Type:";
			//
			// zDropEdit2
			//
			this.zDropEdit2.BindTo = "ShipmentType";
			this.zDropEdit2.BindToList = "Lookups+ShipmentTypeList";
			this.zDropEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(559, 32);
			this.zDropEdit2.Name = "zDropEdit2";
			this.zDropEdit2.PreBoundMaxLength = 3;
			this.zDropEdit2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20);
			this.zDropEdit2.TabIndex = 1;
			//
			// zDropEdit3
			//
			this.zDropEdit3.BindTo = "DutyType";
			this.zDropEdit3.BindToList = "Lookups+DutyTypeList";
			this.zDropEdit3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(559, 56);
			this.zDropEdit3.Name = "zDropEdit3";
			this.zDropEdit3.PreBoundMaxLength = 3;
			this.zDropEdit3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20);
			this.zDropEdit3.TabIndex = 2;
			//
			// zGroupBox2
			//
			this.zGroupBox2.Controls.Add(this.zLabel4);
			this.zGroupBox2.Controls.Add(this.zLabel5);
			this.zGroupBox2.Controls.Add(this.zLabel6);
			this.zGroupBox2.Controls.Add(this.zLabel8);
			this.zGroupBox2.Controls.Add(this.zLabel10);
			this.zGroupBox2.Controls.Add(this.zLabel11);
			this.zGroupBox2.Controls.Add(this.IsRedirectedCheckBox);
			this.zGroupBox2.Controls.Add(this.DeliveryPostCodeTextBox);
			this.zGroupBox2.Controls.Add(this.zTextBox3);
			this.zGroupBox2.Controls.Add(this.DeliveryStateTextBox);
			this.zGroupBox2.Controls.Add(this.zTextBox1);
			this.zGroupBox2.Controls.Add(this.DeliveryAddress1TextBox);
			this.zGroupBox2.Controls.Add(this.DeliveryAddress2TextBox);
			this.zGroupBox2.Controls.Add(this.DeliveryCityTextBox);
			this.zGroupBox2.Controls.Add(this.DeliveryPhoneTextBox);
			this.zGroupBox2.Controls.Add(this.zLabel12);
			this.zGroupBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(416, 314);
			this.zGroupBox2.Name = "zGroupBox2";
			this.zGroupBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 192);
			this.zGroupBox2.TabIndex = 7;
			this.zGroupBox2.TabStop = false;
			this.zGroupBox2.Text = "       Delivery Address Redirection";
			//
			// zLabel4
			//
			this.zLabel4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 40);
			this.zLabel4.Name = "zLabel4";
			this.zLabel4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 23);
			this.zLabel4.TabIndex = 3;
			this.zLabel4.Text = "Name";
			//
			// zLabel5
			//
			this.zLabel5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 16);
			this.zLabel5.Name = "zLabel5";
			this.zLabel5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 23);
			this.zLabel5.TabIndex = 3;
			this.zLabel5.Text = "Contact";
			//
			// zLabel6
			//
			this.zLabel6.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 88);
			this.zLabel6.Name = "zLabel6";
			this.zLabel6.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 23);
			this.zLabel6.TabIndex = 3;
			this.zLabel6.Text = "Address";
			//
			// zLabel8
			//
			this.zLabel8.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 136);
			this.zLabel8.Name = "zLabel8";
			this.zLabel8.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 23);
			this.zLabel8.TabIndex = 3;
			this.zLabel8.Text = "City";
			//
			// zLabel10
			//
			this.zLabel10.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 160);
			this.zLabel10.Name = "zLabel10";
			this.zLabel10.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 23);
			this.zLabel10.TabIndex = 3;
			this.zLabel10.Text = "State";
			//
			// zLabel11
			//
			this.zLabel11.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 160);
			this.zLabel11.Name = "zLabel11";
			this.zLabel11.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 23);
			this.zLabel11.TabIndex = 3;
			this.zLabel11.Text = "Post Code";
			//
			// IsRedirectedCheckBox
			//
			this.IsRedirectedCheckBox.BindTo = "IsRedirected";
			this.IsRedirectedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsRedirectedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, -4);
			this.IsRedirectedCheckBox.Name = "IsRedirectedCheckBox";
			this.IsRedirectedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(16, 24);
			this.IsRedirectedCheckBox.TabIndex = 0;
			//
			// DeliveryPostCodeTextBox
			//
			this.DeliveryPostCodeTextBox.BindTo = "DeliveryAddressOverride+P3_PostCode";
			this.DeliveryPostCodeTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DeliveryPostCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(224, 160);
			this.DeliveryPostCodeTextBox.Name = "DeliveryPostCodeTextBox";
			this.DeliveryPostCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 20);
			this.DeliveryPostCodeTextBox.TabIndex = 8;
			//
			// zTextBox3
			//
			this.zTextBox3.BindTo = "DeliveryAddressOverride+P3_CompanyName";
			this.zTextBox3.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.zTextBox3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 40);
			this.zTextBox3.Name = "zTextBox3";
			this.zTextBox3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20);
			this.zTextBox3.TabIndex = 2;
			//
			// DeliveryStateTextBox
			//
			this.DeliveryStateTextBox.BindTo = "DeliveryAddressOverride+P3_State";
			this.DeliveryStateTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DeliveryStateTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 160);
			this.DeliveryStateTextBox.Name = "DeliveryStateTextBox";
			this.DeliveryStateTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20);
			this.DeliveryStateTextBox.TabIndex = 7;
			//
			// zTextBox1
			//
			this.zTextBox1.BindTo = "DeliveryAddressOverride+P3_ContactName";
			this.zTextBox1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 16);
			this.zTextBox1.Name = "zTextBox1";
			this.zTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20);
			this.zTextBox1.TabIndex = 1;
			//
			// DeliveryAddress1TextBox
			//
			this.DeliveryAddress1TextBox.BindTo = "DeliveryAddressOverride+P3_Address1";
			this.DeliveryAddress1TextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DeliveryAddress1TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 88);
			this.DeliveryAddress1TextBox.Name = "DeliveryAddress1TextBox";
			this.DeliveryAddress1TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20);
			this.DeliveryAddress1TextBox.TabIndex = 4;
			//
			// DeliveryAddress2TextBox
			//
			this.DeliveryAddress2TextBox.BindTo = "DeliveryAddressOverride+P3_Address2";
			this.DeliveryAddress2TextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DeliveryAddress2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 112);
			this.DeliveryAddress2TextBox.Name = "DeliveryAddress2TextBox";
			this.DeliveryAddress2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20);
			this.DeliveryAddress2TextBox.TabIndex = 5;
			//
			// DeliveryCityTextBox
			//
			this.DeliveryCityTextBox.BindTo = "DeliveryAddressOverride+P3_City";
			this.DeliveryCityTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DeliveryCityTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 136);
			this.DeliveryCityTextBox.Name = "DeliveryCityTextBox";
			this.DeliveryCityTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20);
			this.DeliveryCityTextBox.TabIndex = 6;
			//
			// DeliveryPhoneTextBox
			//
			this.DeliveryPhoneTextBox.BindTo = "DeliveryAddressOverride+P3_Phone";
			this.DeliveryPhoneTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DeliveryPhoneTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 64);
			this.DeliveryPhoneTextBox.Name = "DeliveryPhoneTextBox";
			this.DeliveryPhoneTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20);
			this.DeliveryPhoneTextBox.TabIndex = 3;
			//
			// zLabel12
			//
			this.zLabel12.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 64);
			this.zLabel12.Name = "zLabel12";
			this.zLabel12.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 23);
			this.zLabel12.TabIndex = 3;
			this.zLabel12.Text = "Phone";
			this.UPSTabPage.PerformLayout();
			this.HoldForCollectionGroupBox.ResumeLayout(false);
			this.HoldForCollectionGroupBox.PerformLayout();
			this.FlagGroupBox.ResumeLayout(false);
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			this.ChildPackagesGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ChildPackagesGrid)).EndInit();
			this.zGroupBox2.ResumeLayout(false);
			this.zGroupBox2.PerformLayout();
			this.UPSTabPage.ResumeLayout(true);
		}
	}
}
