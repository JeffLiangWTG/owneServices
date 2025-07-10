using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.AirCargo.Testing
{
	[TestedType(typeof(AUCustomsAirCargoFilterStripBusinessObject))]
	sealed class AUCustomsAirCargoListsTest : FilterStripBusinessObjectTestCase
	{
		public void TestConsigneeConsignorSearch()
		{
			CusMAWB mAWB = Factory.New<CusMAWB>();
			mAWB.CM_MAWB = "08115072002";
			CusHAWB hAWB1 = mAWB.ChildBills.AddNew();
			hAWB1.CS_OA_ConsigneeAddress = Factory.LoadTop1<OrgHeader>(new ZQuery()).MainAddress.PK;
			Factory.Save();
			ModuleGuidsFilter guidsFilter = (ModuleGuidsFilter)filterBO[AirCargoFilterConstants.PartyFilterTypes.Consignor + " / " + AirCargoFilterConstants.PartyFilterTypes.Consignee];
			guidsFilter.IsActive = true;
			guidsFilter.Property2 = hAWB1.Consignee.PK;
			filterCollection.Load(filterBO.Filter);
			AssertEquals("1 record with this consignee", 1, filterCollection.Count);
			guidsFilter.Property2 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, hAWB1.Consignee.PK)).PK;
			filterCollection.Load(filterBO.Filter);
			AssertEquals("No record with this consignee", 0, filterCollection.Count);
		}

		public void TestCMROnlyList()
		{
			AUCustomsAirCargoLists lists = new AUCustomsAirCargoLists(Factory);
			AssertEquals(3, lists.CMROnlyList.Count);
			AssertEquals(AirCargoFilterConstants.CMROnlyTypes.All, lists.CMROnlyList[0].Code);
			AssertEquals(AirCargoFilterConstants.CMROnlyTypes.CMRonly, lists.CMROnlyList[1].Code);
			AssertEquals(AirCargoFilterConstants.CMROnlyTypes.LegacyOnly, lists.CMROnlyList[2].Code);
		}

		public void TestGetStatusList()
		{
			AUCustomsAirCargoLists lists = new AUCustomsAirCargoLists(Factory);
			AssertEquals("Should return everything", 80, lists.GetStatusList(null).Count);
			AssertEquals(29, lists.GetStatusList(AirCargoFilterConstants.StatusFilterType.CMRUnderbond).Count);
			CodeDescriptionPairList statusList = lists.GetStatusList(AirCargoFilterConstants.StatusFilterType.CMRCustoms);
			AssertEquals(16, statusList.Count);
			AssertEquals(true, statusList.ContainsCode(CMRConsolidatedCargoStatuses.Filter.Codes.NotClear));
			AssertEquals(10, lists.GetStatusList(AirCargoFilterConstants.StatusFilterType.CMRMessage).Count);
		}

		public void TestHeldOrConditional()
		{
			CusMAWB mAWB1 = Factory.New<CusMAWB>();
			CusMAWB mAWB2 = Factory.New<CusMAWB>();
			CusMAWB mAWB3 = Factory.New<CusMAWB>();
			mAWB1.CM_MAWB = "08156781349";
			mAWB2.CM_MAWB = "08156781350";
			mAWB3.CM_MAWB = "08156781351";
			CusHAWB hAWB1 = mAWB1.ChildBills.AddNew();
			hAWB1.CS_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.CondclearCargoCanBeReleasedIntoHomeConsumptionSubjectToConditionSTheseConditionsAreProvidedInSupplementaryInformation;
			CusHAWB hAWB2 = mAWB2.ChildBills.AddNew();
			hAWB2.CS_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.AcsseizedCargoIsSeizedByCustoms;
			CusHAWB hAWB3 = mAWB3.ChildBills.AddNew();
			hAWB3.CS_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl;
			Factory.Save();
			ModuleTextFilter statusFilter = (ModuleTextFilter)filterBO[AirCargoFilterConstants.StatusFilterType.CMRCustoms];
			statusFilter.IsActive = true;
			statusFilter.Property = CMRConsolidatedCargoStatuses.Filter.Codes.HeldOrConditional;
			filterCollection.Load(filterBO.Filter);
			AssertEquals("Two with the status", 2, filterCollection.Count);
		}

		public void TestClearOrConditional()
		{
			CusMAWB mAWB1 = Factory.New<CusMAWB>();
			CusMAWB mAWB2 = Factory.New<CusMAWB>();
			CusMAWB mAWB3 = Factory.New<CusMAWB>();
			mAWB1.CM_MAWB = "08156781349";
			mAWB2.CM_MAWB = "08156781350";
			mAWB3.CM_MAWB = "08156781351";
			CusHAWB hAWB1 = mAWB1.ChildBills.AddNew();
			hAWB1.CS_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.CondclearCargoCanBeReleasedIntoHomeConsumptionSubjectToConditionSTheseConditionsAreProvidedInSupplementaryInformation;
			CusHAWB hAWB2 = mAWB2.ChildBills.AddNew();
			hAWB2.CS_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.AcsseizedCargoIsSeizedByCustoms;
			CusHAWB hAWB3 = mAWB3.ChildBills.AddNew();
			hAWB3.CS_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.CondclearCargoCanBeReleasedIntoHomeConsumptionSubjectToConditionSTheseConditionsAreProvidedInSupplementaryInformation;
			Factory.Save();
			ModuleTextFilter statusFilter = (ModuleTextFilter)filterBO[AirCargoFilterConstants.StatusFilterType.CMRCustoms];
			statusFilter.IsActive = true;
			statusFilter.Property = CMRConsolidatedCargoStatuses.Filter.Codes.ClearOrConditional;
			filterCollection.Load(filterBO.Filter);
			AssertEquals("Two with the status", 2, filterCollection.Count);
		}

		public void TestCMRCustomsStatusQuery()
		{
			CusMAWB mAWB1 = Factory.New<CusMAWB>();
			mAWB1.CM_MAWB = "08156781349";
			CusHAWB hAWB1 = mAWB1.ChildBills.AddNew();
			hAWB1.CS_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.ClearhrmCargoIsClearButIsIdentifiedAsHighRiskMovement;
			CusHAWB hAWB2 = mAWB1.ChildBills.AddNew();
			hAWB2.CS_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.AcsseizedCargoIsSeizedByCustoms;
			Factory.Save();
			ModuleTextFilter statusFilter = (ModuleTextFilter)filterBO[AirCargoFilterConstants.StatusFilterType.CMRCustoms];
			statusFilter.IsActive = true;
			statusFilter.Property = CMRConsolidatedCargoStatuses.Codes.ClearhrmCargoIsClearButIsIdentifiedAsHighRiskMovement;
			filterCollection.Load(filterBO.Filter);
			AssertEquals("One HAWB with the status", 1, filterCollection.Count);
			statusFilter.Property = CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl;
			filterCollection.Load(filterBO.Filter);
			AssertEquals("No HAWB with the status", 0, filterCollection.Count);
			statusFilter.Property = CMRConsolidatedCargoStatuses.Codes.AcsseizedCargoIsSeizedByCustoms;
			filterCollection.Load(filterBO.Filter);
			AssertEquals("One HAWB with the status", 1, filterCollection.Count);
		}

		public void TestCMRCustomsStatusNotClear()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			CusMAWB mAWB1 = newFactory.New<CusMAWB>();
			mAWB1.CM_MAWB = "08156781349";
			CusHAWB hAWB1 = mAWB1.ChildBills.AddNew();
			hAWB1.CS_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased;
			CusHAWB hAWB2 = mAWB1.ChildBills.AddNew();
			hAWB2.CS_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.AcsseizedCargoIsSeizedByCustoms;
			CusHAWB hAWB3 = mAWB1.ChildBills.AddNew();
			hAWB3.CS_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.ClearhrmCargoIsClearButIsIdentifiedAsHighRiskMovement;
			newFactory.Save();
			ModuleTextFilter statusFilter = (ModuleTextFilter)filterBO[AirCargoFilterConstants.StatusFilterType.CMRCustoms];
			statusFilter.IsActive = true;
			statusFilter.Property = CMRConsolidatedCargoStatuses.Filter.Codes.NotClear;
			ModuleMAWBCollection coll = new ModuleMAWBCollection(newFactory);
			coll.Load(filterBO.Filter);
			AssertEquals("One HAWB Not clear", 1, coll.Count);
		}

		public void TestHouseBillNumberFilter()
		{
			CusMAWB mAWB1 = Factory.New<CusMAWB>();
			mAWB1.CM_MAWB = "08156781349";
			CusHAWB hAWB1 = mAWB1.ChildBills.AddNew();
			hAWB1.CS_HAWB = "Z1234";
			CusHAWB hAWB2 = mAWB1.ChildBills.AddNew();
			hAWB2.CS_HAWB = "X0987";
			Factory.Save();
			ModuleTextFilter numberFilter = (ModuleTextFilter)filterBO[AirCargoFilterConstants.NumberFilterTypes.HouseBillNumber];
			numberFilter.IsActive = true;
			numberFilter.Property = "Z123";
			filterCollection.Load(filterBO.Filter);
			AssertEquals("There are one record with HAWB", 1, filterCollection.Count);
			numberFilter.Property = "Z123";
			numberFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filterCollection.Load(filterBO.Filter);
			AssertEquals("There are one record with HAWB", 0, filterCollection.Count);
			numberFilter.Property = "X0987";
			numberFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filterCollection.Load(filterBO.Filter);
			AssertEquals("There are one record with HAWB", 1, filterCollection.Count);
		}

		public void TestMasterBillNumberFilter()
		{
			CusMAWB mAWB1 = Factory.New<CusMAWB>();
			mAWB1.CM_MAWB = "08156781349";
			CusHAWB hAWB1 = mAWB1.ChildBills.AddNew();
			CusHAWB hAWB2 = mAWB1.ChildBills.AddNew();
			CusMAWB mAWB2 = Factory.New<CusMAWB>();
			mAWB2.CM_MAWB = "08198453587";
			Factory.Save();
			ModuleTextFilter numberFilter = (ModuleTextFilter)filterBO[AirCargoFilterConstants.NumberFilterTypes.MasterBillNumber];
			numberFilter.IsActive = true;
			numberFilter.Property = mAWB1.CM_MAWB;
			filterCollection.Load(filterBO.Filter);
			AssertEquals("There is 1 MAWB", 1, filterCollection.Count);
			numberFilter.Property = mAWB2.CM_MAWB;
			filterCollection.Load(filterBO.Filter);
			AssertEquals("There is 1 MAWB", 1, filterCollection.Count);
			numberFilter.Property = "3245235";
			filterCollection.Load(filterBO.Filter);
			AssertEquals("There are no MAWBs", 0, filterCollection.Count);
		}

		public void TestConsolIDFilter()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			Transport transport = consol.Transports[0];
			transport.JW_RL_NKDiscPort = "AUSYD";
			CusMAWB mAWB = Factory.New<CusMAWB>();
			mAWB.CM_MAWB = "08156781349";
			mAWB.CM_JK = consol.PK;
			CusHAWB hAWB1 = mAWB.ChildBills.AddNew();
			CusHAWB hAWB2 = mAWB.ChildBills.AddNew();
			CusMAWB mAWB2 = Factory.New<CusMAWB>();
			mAWB2.CM_MAWB = "08198453587";
			Factory.Save();
			ModuleTextFilter numberFilter = (ModuleTextFilter)filterBO[AirCargoFilterConstants.NumberFilterTypes.ConsolNumber];
			numberFilter.IsActive = true;
			numberFilter.Property = consol.JK_UniqueConsignRef;
			filterCollection.Load(filterBO.Filter);
			AssertEquals("There is one MAWB", 1, filterCollection.Count);
			numberFilter.Property = consol.JK_UniqueConsignRef.Substring(1);
			filterCollection.Load(filterBO.Filter);
			AssertEquals("There is 1 MAWB", 1, filterCollection.Count);
			numberFilter.Property = "Nothing";
			filterCollection.Load(filterBO.Filter);
			AssertEquals("There are 0 MAWB", 0, filterCollection.Count);
		}

		public void TestJobNumberFilter()
		{
			CusMAWB mAWB1 = Factory.New<CusMAWB>();
			mAWB1.CM_MAWB = "08156781349";
			CusHAWB hAWB1 = mAWB1.ChildBills.AddNew();
			hAWB1.CS_MessageReference = "A09999999";
			CusHAWB hAWB2 = mAWB1.ChildBills.AddNew();
			hAWB2.CS_MessageReference = "A09999998";
			Factory.Save();
			ModuleTextFilter numberFilter = (ModuleTextFilter)filterBO[AirCargoFilterConstants.NumberFilterTypes.JobNumber];
			numberFilter.IsActive = true;
			numberFilter.Property = "9999999";
			filterCollection.Load(filterBO.Filter);
			AssertEquals("There is one record", 1, filterCollection.Count);
			numberFilter.Property = "9999";
			filterCollection.Load(filterBO.Filter);
			AssertEquals("There are no records", 0, filterCollection.Count);
			numberFilter.Property = "A09999998";
			filterCollection.Load(filterBO.Filter);
			AssertEquals("There is one records", 1, filterCollection.Count);
		}

		public void TestFlightNoFilterForPartShip()
		{
			CusMAWB mAWB = Factory.New<CusMAWB>();
			mAWB.CM_MAWB = "08115072002";
			mAWB.CM_FlightNo = "XX90";
			CusHAWB hAWB1 = mAWB.ChildBills.AddNew();
			hAWB1.CS_HAWB = "11111XXX";
			CusUnderbond underbond = hAWB1.Underbonds.AddNew();
			underbond.C4_ParentID = hAWB1.PK;
			underbond.C4_FlightNo = "YY456";
			Factory.Save();
			ModuleTextFilter numberFilter = (ModuleTextFilter)filterBO[AirCargoFilterConstants.NumberFilterTypes.FlightNo];
			numberFilter.IsActive = true;
			numberFilter.Property = "XX90";
			filterCollection.Load(filterBO.Filter);
			AssertEquals("One record", 1, filterCollection.Count);
			numberFilter.Property = "YY20";
			filterCollection.Load(filterBO.Filter);
			AssertEquals("No record", 0, filterCollection.Count);
			numberFilter.Property = "YY456";
			filterCollection.Load(filterBO.Filter);
			AssertEquals("One record", 1, filterCollection.Count);
		}

		public void TestFlightNoFilter()
		{
			CusMAWB mAWB1 = Factory.New<CusMAWB>();
			mAWB1.CM_MAWB = "08156781349";
			mAWB1.CM_FlightNo = "YY2";
			CusHAWB hAWB1 = mAWB1.ChildBills.AddNew();
			CusMAWB mAWB2 = Factory.New<CusMAWB>();
			mAWB2.CM_MAWB = "45611111111";
			mAWB2.CM_FlightNo = "XX90";
			CusHAWB hAWB2 = mAWB2.ChildBills.AddNew();
			Factory.Save();
			ModuleTextFilter numberFilter = (ModuleTextFilter)filterBO[AirCargoFilterConstants.NumberFilterTypes.FlightNo];
			numberFilter.IsActive = true;
			numberFilter.Property = "YY2";
			filterCollection.Load(filterBO.Filter);
			AssertEquals("One record", 1, filterCollection.Count);
			numberFilter.Property = "YY20";
			filterCollection.Load(filterBO.Filter);
			AssertEquals("No record", 0, filterCollection.Count);
			numberFilter.Property = "XX90";
			filterCollection.Load(filterBO.Filter);
			AssertEquals("One record", 1, filterCollection.Count);
		}

		public void TestWeDontMatchCTOMAWBs()
		{
			CTOCusMAWB mAWB = Factory.New<CTOCusMAWB>();
			mAWB.CM_FlightNo = "YY2";
			CTOCusHAWB hAWB = mAWB.ChildBills.AddNew();
			Factory.Save();
			ModuleTextFilter numberFilter = (ModuleTextFilter)filterBO[AirCargoFilterConstants.NumberFilterTypes.FlightNo];
			numberFilter.IsActive = true;
			numberFilter.Property = "YY2";
			filterCollection.Load(filterBO.Filter);
			AssertEquals("No records", 0, filterCollection.Count);
		}

		public void TestLoadPortFilter()
		{
			CusMAWB mAWB1 = Factory.New<CusMAWB>();
			mAWB1.CM_MAWB = "08156781349";
			mAWB1.CM_RL_NKLoadPort = "USLAX";
			mAWB1.CM_RL_NKDischargePort = "AUSYD";
			CusHAWB hAWB1 = mAWB1.ChildBills.AddNew();
			CusMAWB mAWB2 = Factory.New<CusMAWB>();
			mAWB2.CM_MAWB = "45611111111";
			mAWB2.CM_RL_NKLoadPort = "KRSEL";
			mAWB2.CM_RL_NKDischargePort = "AUBNE";
			CusHAWB hAWB2 = mAWB2.ChildBills.AddNew();
			CusHAWB hAWB3 = mAWB2.ChildBills.AddNew();
			Factory.Save();
			ModuleLocationFilter loadDichargeFilter = (ModuleLocationFilter)filterBO[AirCargoFilterConstants.PortFilterTypes.LoadDischarge];
			loadDichargeFilter.IsActive = true;
			loadDichargeFilter.Property1 = "USLAX";
			filterCollection.Load(filterBO.Filter);
			AssertEquals("One record", 1, filterCollection.Count);
			loadDichargeFilter.Property1 = "KRSEL";
			filterCollection.Load(filterBO.Filter);
			AssertEquals("1 records", 1, filterCollection.Count);
			loadDichargeFilter.Property1 = "KRPUS";
			filterCollection.Load(filterBO.Filter);
			AssertEquals("no record", 0, filterCollection.Count);
		}

		public void TestDischagePortFilter()
		{
			CusMAWB mAWB1 = Factory.New<CusMAWB>();
			mAWB1.CM_MAWB = "08156781349";
			mAWB1.CM_RL_NKLoadPort = "USLAX";
			mAWB1.CM_RL_NKDischargePort = "AUXXX";
			CusHAWB hAWB1 = mAWB1.ChildBills.AddNew();
			CusMAWB mAWB2 = Factory.New<CusMAWB>();
			mAWB2.CM_MAWB = "45611111111";
			mAWB2.CM_RL_NKLoadPort = "KRSEL";
			mAWB2.CM_RL_NKDischargePort = "AUYYY";
			CusHAWB hAWB2 = mAWB2.ChildBills.AddNew();
			CusHAWB hAWB3 = mAWB2.ChildBills.AddNew();
			Factory.Save();
			ModuleLocationFilter loadDichargeFilter = (ModuleLocationFilter)filterBO[AirCargoFilterConstants.PortFilterTypes.LoadDischarge];
			loadDichargeFilter.IsActive = true;
			loadDichargeFilter.Property2 = "AUXXX";
			filterCollection.Load(filterBO.Filter);
			AssertEquals("One record", 1, filterCollection.Count);
			loadDichargeFilter.Property2 = "AUYYY";
			filterCollection.Load(filterBO.Filter);
			AssertEquals("1 records", 1, filterCollection.Count);
			loadDichargeFilter.Property2 = "AUZZZ";
			filterCollection.Load(filterBO.Filter);
			AssertEquals("no record", 0, filterCollection.Count);
		}

		public void TestOriginDestinationPortFilter()
		{
			CusMAWB mAWB1 = Factory.New<CusMAWB>();
			mAWB1.CM_MAWB = "08156781349";
			CusHAWB hAWB1 = mAWB1.ChildBills.AddNew();
			hAWB1.CS_RL_NKOrigin = "NZAAA";
			hAWB1.CS_RL_NKDestination = "AUYYY";
			Factory.Save();
			ModuleLocationFilter originDestFilter = (ModuleLocationFilter)filterBO[AirCargoFilterConstants.PortFilterTypes.OriginDestination];
			originDestFilter.IsActive = true;
			originDestFilter.Property1 = "NZAAA";
			filterCollection.Load(filterBO.Filter);
			AssertEquals("One record", 1, filterCollection.Count);
			originDestFilter.Property2 = "AUYYY";
			filterCollection.Load(filterBO.Filter);
			AssertEquals("One record", 1, filterCollection.Count);
			originDestFilter.Property2 = "AUZZZ";
			filterCollection.Load(filterBO.Filter);
			AssertEquals("No record", 0, filterCollection.Count);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new AUCustomsAirCargoFilterStripBusinessObject();

		AUCustomsAirCargoFilterStripBusinessObject filterBO;
		ModuleMAWBCollection filterCollection;
		protected override void SetUp()
		{
			base.SetUp();
			filterBO = new AUCustomsAirCargoFilterStripBusinessObject();
			filterCollection = new ModuleMAWBCollection(Factory);
		}
	}
}
