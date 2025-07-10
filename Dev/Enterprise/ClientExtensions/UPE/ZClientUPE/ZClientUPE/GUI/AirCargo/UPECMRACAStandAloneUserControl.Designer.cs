using Enterprise.Customs.AU.AirCargo.GUI;

namespace Enterprise.Client.UPE.GUI
{
	public partial class UPECMRACAStandAloneUserControl : CMRACAStandAloneUserControl
	{
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.HouseBillsModuleButtonGrid)).BeginInit();
			this.UpperPanel.SuspendLayout();
			this.HouseBillsPanel.SuspendLayout();
			this.HouseDetailsTabPage.SuspendLayout();
			this.HouseBillsGroupBox.SuspendLayout();
			this.HAWBTabControl.SuspendLayout();
			this.MasterGroupBox.SuspendLayout();
			this.airCagoHouseBillPartiesUserControl.SuspendLayout();
			this.MasterTabControl.SuspendLayout();
			this.houseBillsTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// airCargoStandAloneHouseMessageUserControl
			// 
			this.AirCargoStandAloneHouseMessageUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(973, 261, true);
			// 
			// HouseBillsModuleButtonGrid
			// 
			zCheckBoxColumnStyleInfo1.Caption = "Is Surplus";
			zCheckBoxColumnStyleInfo1.ColumnName = "CS_IsSurplus";
			this.HouseBillsModuleButtonGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			// Compile time check for the above grid columns. If any of these lines fail, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).ChildBills)))).CS_MessageReferenceInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).ChildBills)))).CS_MessageReference)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).ChildBills)))).CS_HAWBInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).ChildBills)))).CS_HAWB)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).ChildBills)))).Lookups.OriginList)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).ChildBills)))).CS_RL_NKOriginInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).ChildBills)))).CS_RL_NKOrigin)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).ChildBills)))).Lookups.DestinationList)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).ChildBills)))).CS_RL_NKDestinationInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).ChildBills)))).CS_RL_NKDestination)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).ChildBills)))).CS_MasterHouseBillInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).ChildBills)))).CS_MasterHouseBill)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).ChildBills)))).CS_IsMasterHouse)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).ChildBills)))).CS_IsMasterHouseInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).ChildBills)))).CS_Weight)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).ChildBills)))).CS_WeightInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).ChildBills)))).Lookups.UnitOfWeightList)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).ChildBills)))).CS_WeightUQInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).ChildBills)))).CS_WeightUQ)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).ChildBills)))).CS_GoodsValue)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).ChildBills)))).CS_GoodsValueInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).ChildBills)))).Lookups.CurrencyList)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).ChildBills)))).CS_RX_NKGoodsCurrencyInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).ChildBills)))).CS_RX_NKGoodsCurrency)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).ChildBills)))).CS_GoodsDescriptionInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).ChildBills)))).CS_GoodsDescription)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).ChildBills)))).CS_PiecesManifested)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).ChildBills)))).CS_PiecesManifestedInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).ChildBills)))).CS_PiecesLanded)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).ChildBills)))).CS_PiecesLandedInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).ChildBills)))).Lookups.PrepaidCollectList)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).ChildBills)))).CS_FreightPrepaidCollectInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).ChildBills)))).CS_FreightPrepaidCollect)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).ChildBills)))).Lookups.ConsigneeList)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZGuid)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).ChildBills)))).CS_OH_Consignee)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).ChildBills)))).CS_OH_ConsigneeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).ChildBills)))).Lookups.ConsignorList)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZGuid)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).ChildBills)))).CS_OH_Consignor)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).ChildBills)))).CS_OH_ConsignorInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).ChildBills)))).CS_ConsigneeNameInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).ChildBills)))).CS_ConsigneeName)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).ChildBills)))).CS_ConsigneeStreetInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).ChildBills)))).CS_ConsigneeStreet)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).ChildBills)))).CS_ConsigneeCityInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).ChildBills)))).CS_ConsigneeCity)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).ChildBills)))).CS_ConsigneeStateInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).ChildBills)))).CS_ConsigneeState)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).ChildBills)))).CS_ConsigneePostcodeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).ChildBills)))).CS_ConsigneePostcode)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).ChildBills)))).CS_ConsigneePhoneInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).ChildBills)))).CS_ConsigneePhone)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).ChildBills)))).CS_ConsignorNameInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).ChildBills)))).CS_ConsignorName)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).ChildBills)))).CS_ConsignorStreetInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).ChildBills)))).CS_ConsignorStreet)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).ChildBills)))).CS_ConsignorCityInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).ChildBills)))).CS_ConsignorCity)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).ChildBills)))).CS_ConsignorStateInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).ChildBills)))).CS_ConsignorState)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).ChildBills)))).CS_ConsignorPostcodeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).ChildBills)))).CS_ConsignorPostcode)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).ChildBills)))).CS_ResponsiblePartyIDInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).ChildBills)))).CS_ResponsiblePartyID)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).ChildBills)))).CS_IsPersonalEffects)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).ChildBills)))).CS_IsPersonalEffectsInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).ChildBills)))).CS_IsSelfAssessedClearance)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).ChildBills)))).CS_IsSelfAssessedClearanceInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).ChildBills)))).Lookups.ShipmentTypeList)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).ChildBills)))).CS_ShipmentTypeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).ChildBills)))).CS_ShipmentType)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).ChildBills)))).CMRCargoStatus.DescriptionInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).ChildBills)))).CMRCargoStatus.Description)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).ChildBills)))).CMRMessageStatus.DescriptionInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).ChildBills)))).CMRMessageStatus.Description)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).ChildBills)))).CS_IsSurplus)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).ChildBills)))).CS_IsSurplusInfo)));
			// 
			// UPECMRACAStandAloneUserControl
			// 
			this.DataSourceAssemblyName = "Enterprise.Customs.AU.Declaration.Business";
			this.Name = "UPECMRACAStandAloneUserControl";
			((System.ComponentModel.ISupportInitialize)(this.HouseBillsModuleButtonGrid)).EndInit();
			this.UpperPanel.ResumeLayout(false);
			this.HouseBillsPanel.ResumeLayout(false);
			this.HouseDetailsTabPage.ResumeLayout(false);
			this.HouseBillsGroupBox.ResumeLayout(false);
			this.HAWBTabControl.ResumeLayout(false);
			this.MasterGroupBox.ResumeLayout(false);
			this.MasterGroupBox.PerformLayout();
			this.airCagoHouseBillPartiesUserControl.ResumeLayout(false);
			this.airCagoHouseBillPartiesUserControl.PerformLayout();
			this.MasterTabControl.ResumeLayout(false);
			this.houseBillsTabPage.ResumeLayout(false);
			this.ResumeLayout(false);
		}
	}
}
