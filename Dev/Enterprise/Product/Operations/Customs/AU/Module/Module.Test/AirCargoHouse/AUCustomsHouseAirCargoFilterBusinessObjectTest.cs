using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.AirCargo.Testing
{
	[TestedType(typeof(AUCustomsHouseAirCargoFilterBusinessObject))]
	sealed class AUCustomsHouseAirCargoFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestHouseBillNumberFilter()
		{
			hAWB1.CS_HAWB = "ZZAAZ";
			hAWB2.CS_HAWB = "XAAXX";
			Factory.Save();
			ModuleNumberFilter houseBillNumberFilter = (ModuleNumberFilter)filterBO[AirCargoFilterConstants.NumberFilterTypes.HouseBillNumber];
			AssertEquals(ModuleNumberFilter.MultiplyMaxLength(CusHAWBSchema.CS_HAWB.MaxLength), houseBillNumberFilter.MaxLength);
			houseBillNumberFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			houseBillNumberFilter.Property = "ZZA";
			houseBillNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain HAWB1", filterCollection.Contains(hAWB1));
			Assert("Expect collection not to contain HAWB2", !filterCollection.Contains(hAWB2));
			houseBillNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			houseBillNumberFilter.Property = "XAAXX";
			houseBillNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain HAWB1", !filterCollection.Contains(hAWB1));
			Assert("Expect collection to contain HAWB2", filterCollection.Contains(hAWB2));
			houseBillNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			houseBillNumberFilter.Property = "AA";
			houseBillNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain HAWB1", filterCollection.Contains(hAWB1));
			Assert("Expect collection to contain HAWB2", filterCollection.Contains(hAWB2));
			houseBillNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			houseBillNumberFilter.Property = "YYYYY";
			houseBillNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain HAWB1", !filterCollection.Contains(hAWB1));
			Assert("Expect collection not to contain HAWB2", !filterCollection.Contains(hAWB2));
		}

		public void TestMasterBillNumberFilter()
		{
			hAWB1.CS_CM = mAWB1.PK;
			hAWB2.CS_CM = mAWB2.PK;
			mAWB1.CM_MAWB = "ZZZAAAZZZZZ";
			mAWB2.CM_MAWB = "XXXXXAAAXXX";
			Factory.Save();
			ModuleNumberFilter masterBillNumberFilter = (ModuleNumberFilter)filterBO[AirCargoFilterConstants.NumberFilterTypes.MasterBillNumber];
			AssertEquals(ModuleNumberFilter.MultiplyMaxLength(CusMAWBSchema.CM_MAWB.MaxLength), masterBillNumberFilter.MaxLength);
			filterCollection.Load(filterBO.Filter);
			masterBillNumberFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			masterBillNumberFilter.Property = "ZZZ";
			masterBillNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain HAWB1", filterCollection.Contains(hAWB1));
			Assert("Expect collection not to contain HAWB2", !filterCollection.Contains(hAWB2));
			masterBillNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			masterBillNumberFilter.Property = "XXXXXAAAXXX";
			masterBillNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain HAWB1", !filterCollection.Contains(hAWB1));
			Assert("Expect collection to contain HAWB2", filterCollection.Contains(hAWB2));
			masterBillNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			masterBillNumberFilter.Property = "AAA";
			masterBillNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain HAWB1", filterCollection.Contains(hAWB1));
			Assert("Expect collection to contain HAWB2", filterCollection.Contains(hAWB2));
			masterBillNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			masterBillNumberFilter.Property = "YYYYYYYYYYY";
			masterBillNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain HAWB1", !filterCollection.Contains(hAWB1));
			Assert("Expect collection not to contain HAWB2", !filterCollection.Contains(hAWB2));
		}

		public void TestConsolNumberFilter()
		{
			mAWB1.CM_JK = consol1.PK;
			mAWB2.CM_JK = consol2.PK;
			hAWB1.CS_CM = mAWB1.PK;
			hAWB2.CS_CM = mAWB2.PK;
			consol1.JK_UniqueConsignRef = "";
			consol2.JK_UniqueConsignRef = "";
			Factory.Save();
			ModuleNumberFilter consolNumberFilter = (ModuleNumberFilter)filterBO[AirCargoFilterConstants.NumberFilterTypes.ConsolNumber];
			AssertEquals(ModuleNumberFilter.MultiplyMaxLength(JobConsolSchema.JK_UniqueConsignRef.MaxLength), consolNumberFilter.MaxLength);
			consolNumberFilter.Property = consol1.JK_UniqueConsignRef;
			consolNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain HAWB1", filterCollection.Contains(hAWB1));
			Assert("Expect collection not to contain HAWB2", !filterCollection.Contains(hAWB2));
			consolNumberFilter.Property = consol2.JK_UniqueConsignRef.Substring(1);
			consolNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain HAWB1", !filterCollection.Contains(hAWB1));
			Assert("Expect collection to contain HAWB2", filterCollection.Contains(hAWB2));
			consolNumberFilter.Property = "Nothing";
			consolNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain HAWB1", !filterCollection.Contains(hAWB1));
			Assert("Expect collection not to contain HAWB2", !filterCollection.Contains(hAWB2));
		}

		public void TestJobNumberFilter()
		{
			hAWB1.CS_MessageReference = "A09999999";
			hAWB2.CS_MessageReference = "A09999998";
			Factory.Save();
			ModuleNumberFilter jobNumberFilter = (ModuleNumberFilter)filterBO[AirCargoFilterConstants.NumberFilterTypes.JobNumber];
			AssertEquals(ModuleNumberFilter.MultiplyMaxLength(CusHAWBSchema.CS_MessageReference.MaxLength), jobNumberFilter.MaxLength);
			jobNumberFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			jobNumberFilter.Property = "9999999";
			jobNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain HAWB1", filterCollection.Contains(hAWB1));
			Assert("Expect collection not to contain HAWB2", !filterCollection.Contains(hAWB2));
			jobNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			jobNumberFilter.Property = "A09999998";
			jobNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain HAWB1", !filterCollection.Contains(hAWB1));
			Assert("Expect collection to contain HAWB2", filterCollection.Contains(hAWB2));
			jobNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			jobNumberFilter.Property = "9999";
			jobNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain HAWB1", !filterCollection.Contains(hAWB1));
			Assert("Expect collection not to contain HAWB2", !filterCollection.Contains(hAWB2));
		}

		public void TestFlightNoFilterForPartShip()
		{
			mAWB1.CM_FlightNo = "XAAAX";
			hAWB1.CS_CM = mAWB1.PK;
			hAWB2.CS_CM = mAWB1.PK;
			underbond1.C4_ParentID = hAWB2.PK;
			underbond1.C4_FlightNo = "YAAAY";
			Factory.Save();
			ModuleNumberFilter flightNoFilter = (ModuleNumberFilter)filterBO[AirCargoFilterConstants.NumberFilterTypes.FlightNo];
			flightNoFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			flightNoFilter.Property = "XAA";
			flightNoFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain HAWB1", filterCollection.Contains(hAWB1));
			Assert("Expect collection to contain HAWB2", filterCollection.Contains(hAWB2));
			flightNoFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			flightNoFilter.Property = "YAAAY";
			flightNoFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain HAWB1", !filterCollection.Contains(hAWB1));
			Assert("Expect collection to contain HAWB2", filterCollection.Contains(hAWB2));
			flightNoFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			flightNoFilter.Property = "AAA";
			flightNoFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain HAWB1", filterCollection.Contains(hAWB1));
			Assert("Expect collection to contain HAWB2", filterCollection.Contains(hAWB2));
			flightNoFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			flightNoFilter.Property = "ZZZ";
			flightNoFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain HAWB1", !filterCollection.Contains(hAWB1));
			Assert("Expect collection not to contain HAWB2", !filterCollection.Contains(hAWB2));
		}

		public void TestFlightNoFilter()
		{
			hAWB1.CS_CM = mAWB1.PK;
			hAWB2.CS_CM = mAWB2.PK;
			mAWB1.CM_FlightNo = "XAAAX";
			mAWB2.CM_FlightNo = "YAAAY";
			Factory.Save();
			ModuleNumberFilter flightNoFilter = (ModuleNumberFilter)filterBO[AirCargoFilterConstants.NumberFilterTypes.FlightNo];
			AssertEquals(ModuleNumberFilter.MultiplyMaxLength(CusMAWBSchema.CM_FlightNo.MaxLength), flightNoFilter.MaxLength);
			flightNoFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			flightNoFilter.Property = "XAA";
			flightNoFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain HAWB1", filterCollection.Contains(hAWB1));
			Assert("Expect collection not to contain HAWB2", !filterCollection.Contains(hAWB2));
			flightNoFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			flightNoFilter.Property = "YAAAY";
			flightNoFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain HAWB1", !filterCollection.Contains(hAWB1));
			Assert("Expect collection to contain HAWB2", filterCollection.Contains(hAWB2));
			flightNoFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			flightNoFilter.Property = "AAA";
			flightNoFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain HAWB1", filterCollection.Contains(hAWB1));
			Assert("Expect collection to contain HAWB2", filterCollection.Contains(hAWB2));
			flightNoFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			flightNoFilter.Property = "ZZZ";
			flightNoFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain HAWB1", !filterCollection.Contains(hAWB1));
			Assert("Expect collection not to contain HAWB2", !filterCollection.Contains(hAWB2));
		}

		public void TestCoLoadMasterFilter()
		{
			hAWB1.CS_MasterHouseBill = "ZZAAZ";
			hAWB2.CS_MasterHouseBill = "XAAXX";
			Factory.Save();
			ModuleNumberFilter coLoadMasterFilter = (ModuleNumberFilter)filterBO[AirCargoFilterConstants.NumberFilterTypes.CoLoadMaster];
			AssertEquals(ModuleNumberFilter.MultiplyMaxLength(CusHAWBSchema.CS_MasterHouseBill.MaxLength), coLoadMasterFilter.MaxLength);
			coLoadMasterFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			coLoadMasterFilter.Property = "ZZA";
			coLoadMasterFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain HAWB1", filterCollection.Contains(hAWB1));
			Assert("Expect collection not to contain HAWB2", !filterCollection.Contains(hAWB2));
			coLoadMasterFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			coLoadMasterFilter.Property = "XAAXX";
			coLoadMasterFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain HAWB1", !filterCollection.Contains(hAWB1));
			Assert("Expect collection to contain HAWB2", filterCollection.Contains(hAWB2));
			coLoadMasterFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			coLoadMasterFilter.Property = "AA";
			coLoadMasterFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain HAWB1", filterCollection.Contains(hAWB1));
			Assert("Expect collection to contain HAWB2", filterCollection.Contains(hAWB2));
			coLoadMasterFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			coLoadMasterFilter.Property = "YYYYY";
			coLoadMasterFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain HAWB1", !filterCollection.Contains(hAWB1));
			Assert("Expect collection not to contain HAWB2", !filterCollection.Contains(hAWB2));
		}

		public void TestConRefNumberFilter()
		{
			AssertNull((ModuleNumberFilter)filterBO[AirCargoFilterConstants.NumberFilterTypes.ConRef]);
			AUCustomsDataRegistry.Instance.ActivateAlternatePartShipmentModel.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			filterBO = (AUCustomsHouseAirCargoFilterBusinessObject)GetNewFilterStripBusinessObject();
			hAWB1.CS_HAWB = "ZZAAZ";
			hAWB1.CS_fPartShipConsignmentReference = "CONREFAAA";
			hAWB2.CS_HAWB = "XAAXX";
			hAWB2.CS_fPartShipConsignmentReference = "CONREFBBB";
			Factory.Save();
			var conRefNumberFilter = (ModuleNumberFilter)filterBO[AirCargoFilterConstants.NumberFilterTypes.ConRef];
			AssertNotNull(conRefNumberFilter);
			conRefNumberFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			conRefNumberFilter.Property = "CONREFA";
			conRefNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain HAWB1", filterCollection.Contains(hAWB1));
			Assert("Expect collection not to contain HAWB2", !filterCollection.Contains(hAWB2));
			conRefNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			conRefNumberFilter.Property = "CONREFBBB";
			conRefNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain HAWB1", !filterCollection.Contains(hAWB1));
			Assert("Expect collection to contain HAWB2", filterCollection.Contains(hAWB2));
			conRefNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			conRefNumberFilter.Property = "REF";
			conRefNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain HAWB1", filterCollection.Contains(hAWB1));
			Assert("Expect collection to contain HAWB2", filterCollection.Contains(hAWB2));
			conRefNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			conRefNumberFilter.Property = "YYYYY";
			conRefNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain HAWB1", !filterCollection.Contains(hAWB1));
			Assert("Expect collection not to contain HAWB2", !filterCollection.Contains(hAWB2));
		}

		public void TestConsigneesAndConsignorsProperties()
		{
			AssertNotNull(filterBO.Consignees);
			AssertNotNull(filterBO.Consignors);
		}

		public void TestConsignorFilter()
		{
			organisation1.OH_IsConsignor = true;
			organisation2.OH_IsConsignor = true;
			organisation3.OH_IsConsignor = true;
			hAWB1.CS_OA_ConsignorAddress = organisation1.MainAddress.PK;
			hAWB2.CS_OA_ConsignorAddress = organisation2.MainAddress.PK;
			Factory.Save();
			ModuleGuidsFilter consignorFilter = (ModuleGuidsFilter)filterBO[string.Format("{0} / {1}", AirCargoFilterConstants.PartyFilterTypes.Consignor, AirCargoFilterConstants.PartyFilterTypes.Consignee)];
			consignorFilter.Property1 = organisation1.PK;
			consignorFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain HAWB1", filterCollection.Contains(hAWB1));
			Assert("Expect collection not to contain HAWB2", !filterCollection.Contains(hAWB2));
			consignorFilter.Property1 = organisation3.PK;
			consignorFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain HAWB1", !filterCollection.Contains(hAWB1));
			Assert("Expect collection not to contain HAWB2", !filterCollection.Contains(hAWB2));
		}

		public void TestConsigneeFilter()
		{
			organisation1.OH_IsConsignee = true;
			organisation2.OH_IsConsignee = true;
			organisation3.OH_IsConsignee = true;
			hAWB1.CS_OA_ConsigneeAddress = organisation1.MainAddress.PK;
			hAWB2.CS_OA_ConsigneeAddress = organisation2.MainAddress.PK;
			Factory.Save();
			ModuleGuidsFilter consigneeFilter = (ModuleGuidsFilter)filterBO[string.Format("{0} / {1}", AirCargoFilterConstants.PartyFilterTypes.Consignor, AirCargoFilterConstants.PartyFilterTypes.Consignee)];
			consigneeFilter.Property2 = organisation1.PK;
			consigneeFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain HAWB1", filterCollection.Contains(hAWB1));
			Assert("Expect collection not to contain HAWB2", !filterCollection.Contains(hAWB2));
			consigneeFilter.Property2 = organisation3.PK;
			consigneeFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain HAWB1", !filterCollection.Contains(hAWB1));
			Assert("Expect collection not to contain HAWB2", !filterCollection.Contains(hAWB2));
		}

		public void TestLocationsProperty()
		{
			AssertNotNull(filterBO.Locations);
		}

		public void TestLocationLoadDischarge()
		{
			hAWB1.CS_CM = mAWB1.PK;
			hAWB2.CS_CM = mAWB2.PK;
			mAWB1.CM_RL_NKLoadPort = "KRSEL";
			mAWB2.CM_RL_NKLoadPort = "AUBNE";
			mAWB1.CM_RL_NKDischargePort = "AUSYD";
			mAWB2.CM_RL_NKDischargePort = "AUSYD";
			Factory.Save();
			ModuleLocationFilter loadDichargeFilter = (ModuleLocationFilter)filterBO[AirCargoFilterConstants.PortFilterTypes.LoadDischarge];
			loadDichargeFilter.Property1 = "KR";
			loadDichargeFilter.Property2 = ZString.Empty;
			loadDichargeFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain HAWB1", filterCollection.Contains(hAWB1));
			Assert("Expect collection not to contain HAWB2", !filterCollection.Contains(hAWB2));
			loadDichargeFilter.Property1 = "AUBNE";
			loadDichargeFilter.Property2 = "AUSYD";
			loadDichargeFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain HAWB1", !filterCollection.Contains(hAWB1));
			Assert("Expect collection to contain HAWB2", filterCollection.Contains(hAWB2));
			loadDichargeFilter.Property1 = ZString.Empty;
			loadDichargeFilter.Property2 = "AUSYD";
			loadDichargeFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain HAWB1", filterCollection.Contains(hAWB1));
			Assert("Expect collection to contain HAWB2", filterCollection.Contains(hAWB2));
			loadDichargeFilter.Property1 = "AUSYD";
			loadDichargeFilter.Property2 = ZString.Empty;
			loadDichargeFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain HAWB1", !filterCollection.Contains(hAWB1));
			Assert("Expect collection not to contain HAWB2", !filterCollection.Contains(hAWB2));
		}

		public void TestLocationOriginDestination()
		{
			hAWB1.CS_RL_NKOrigin = "KRSEL";
			hAWB2.CS_RL_NKOrigin = "AUBNE";
			hAWB1.CS_RL_NKDestination = "AUSYD";
			hAWB2.CS_RL_NKDestination = "AUSYD";
			Factory.Save();
			ModuleLocationFilter loadDichargeFilter = (ModuleLocationFilter)filterBO[AirCargoFilterConstants.PortFilterTypes.OriginDestination];
			loadDichargeFilter.Property1 = "KR";
			loadDichargeFilter.Property2 = ZString.Empty;
			loadDichargeFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain HAWB1", filterCollection.Contains(hAWB1));
			Assert("Expect collection not to contain HAWB2", !filterCollection.Contains(hAWB2));
			loadDichargeFilter.Property1 = "AUBNE";
			loadDichargeFilter.Property2 = "AUSYD";
			loadDichargeFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain HAWB1", !filterCollection.Contains(hAWB1));
			Assert("Expect collection to contain HAWB2", filterCollection.Contains(hAWB2));
			loadDichargeFilter.Property1 = ZString.Empty;
			loadDichargeFilter.Property2 = "AUSYD";
			loadDichargeFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain HAWB1", filterCollection.Contains(hAWB1));
			Assert("Expect collection to contain HAWB2", filterCollection.Contains(hAWB2));
			loadDichargeFilter.Property1 = "AUSYD";
			loadDichargeFilter.Property2 = ZString.Empty;
			loadDichargeFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain HAWB1", !filterCollection.Contains(hAWB1));
			Assert("Expect collection not to contain HAWB2", !filterCollection.Contains(hAWB2));
		}

		public void TestGetInBondStoreQuery()
		{
			hAWB1.CS_IsHeldAtOutturn = true;
			hAWB1.CargoReceivedAtDepotLogs.AddNew();
			hAWB2.CS_IsHeldAtOutturn = true;
			Factory.Save();
			Enterprise.Registry.Business.HVLVDataRegistry.HasHVLVClearance = true;
			var filterBO = new AUCustomsHouseAirCargoFilterBusinessObject();
			var inBondStoreFilter = (ModuleFlagsFilter)filterBO[AirCargoFilterConstants.StatusFilterType.InBondStore];
			inBondStoreFilter.IsActive = true;
			inBondStoreFilter.Property0 = true;
			Assert(hAWB1.MatchesFilter(filterBO.Filter));
			Assert(!hAWB2.MatchesFilter(filterBO.Filter));
		}

		public void TestNotClearCodeIsNotContainedInCMRAllStatuses()
		{
			CMRAllStatuses allStatuses = new CMRAllStatuses();
			AssertEquals("Assuming made up code '" + CMRConsolidatedCargoStatuses.Filter.Codes.NotClear + "' does not exist in CMRAllStatuses. Change code for AirCargoFilterConstants.NotClear if this fails", false, allStatuses.ContainsCode(CMRConsolidatedCargoStatuses.Filter.Codes.NotClear));
		}

		public void TestGetStatusList()
		{
			AssertEquals("List should contain everything", 80, filterBO.GetStatusList(null).Count);
			AssertEquals(29, filterBO.GetStatusList(AirCargoFilterConstants.StatusFilterType.CMRUnderbond).Count);
			AssertEquals(10, filterBO.GetStatusList(AirCargoFilterConstants.StatusFilterType.CMRMessage).Count);
			AssertEquals(10, filterBO.GetStatusList(AirCargoFilterConstants.StatusFilterType.Outturn).Count);
			CodeDescriptionPairList statusList = filterBO.GetStatusList(AirCargoFilterConstants.StatusFilterType.CMRCustoms);
			AssertEquals(16, statusList.Count);
			AssertEquals(true, statusList.ContainsCode(CMRConsolidatedCargoStatuses.Filter.Codes.NotClear));
			AssertEquals(true, statusList.ContainsCode(CMRConsolidatedCargoStatuses.Filter.Codes.HeldOrConditional));
			AssertEquals(true, statusList.ContainsCode(CMRConsolidatedCargoStatuses.Filter.Codes.ClearOrConditional));
		}

		public void TestStatusHeldOrConditional()
		{
			hAWB1.CS_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.CondclearCargoCanBeReleasedIntoHomeConsumptionSubjectToConditionSTheseConditionsAreProvidedInSupplementaryInformation;
			hAWB2.CS_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.AcsseizedCargoIsSeizedByCustoms;
			hAWB3.CS_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl;
			Factory.Save();
			ModuleTextFilter statusFilter = (ModuleTextFilter)filterBO[AirCargoFilterConstants.StatusFilterType.CMRCustoms];
			statusFilter.Property = CMRConsolidatedCargoStatuses.Filter.Codes.HeldOrConditional;
			statusFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain HAWB1", filterCollection.Contains(hAWB1));
			Assert("Expect collection not to contain HAWB2", !filterCollection.Contains(hAWB2));
			Assert("Expect collection to contain HAWB3", filterCollection.Contains(hAWB3));
		}

		public void TestStatusClearOrConditional()
		{
			hAWB1.CS_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased;
			hAWB2.CS_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.AcsseizedCargoIsSeizedByCustoms;
			hAWB3.CS_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.CondclearCargoCanBeReleasedIntoHomeConsumptionSubjectToConditionSTheseConditionsAreProvidedInSupplementaryInformation;
			Factory.Save();
			ModuleTextFilter statusFilter = (ModuleTextFilter)filterBO[AirCargoFilterConstants.StatusFilterType.CMRCustoms];
			statusFilter.Property = CMRConsolidatedCargoStatuses.Filter.Codes.ClearOrConditional;
			statusFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain HAWB1", filterCollection.Contains(hAWB1));
			Assert("Expect collection not to contain HAWB2", !filterCollection.Contains(hAWB2));
			Assert("Expect collection to contain HAWB3", filterCollection.Contains(hAWB3));
		}

		public void TestStatusCMRCustoms()
		{
			hAWB1.CS_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.ClearhrmCargoIsClearButIsIdentifiedAsHighRiskMovement;
			hAWB2.CS_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.AcsseizedCargoIsSeizedByCustoms;
			Factory.Save();
			ModuleTextFilter statusFilter = (ModuleTextFilter)filterBO[AirCargoFilterConstants.StatusFilterType.CMRCustoms];
			statusFilter.Property = CMRConsolidatedCargoStatuses.Codes.ClearhrmCargoIsClearButIsIdentifiedAsHighRiskMovement;
			statusFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain HAWB1", filterCollection.Contains(hAWB1));
			Assert("Expect collection not to contain HAWB2", !filterCollection.Contains(hAWB2));
			statusFilter.Property = CMRConsolidatedCargoStatuses.Codes.AcsseizedCargoIsSeizedByCustoms;
			statusFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain HAWB1", !filterCollection.Contains(hAWB1));
			Assert("Expect collection to contain HAWB2", filterCollection.Contains(hAWB2));
			statusFilter.Property = CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl;
			statusFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain HAWB1", !filterCollection.Contains(hAWB1));
			Assert("Expect collection not to contain HAWB2", !filterCollection.Contains(hAWB2));
		}

		public void TestStatusCMRCustomsNotClear()
		{
			hAWB1.CS_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased;
			hAWB2.CS_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.AcsseizedCargoIsSeizedByCustoms;
			hAWB3.CS_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.ClearhrmCargoIsClearButIsIdentifiedAsHighRiskMovement;
			Factory.Save();
			ModuleTextFilter statusFilter = (ModuleTextFilter)filterBO[AirCargoFilterConstants.StatusFilterType.CMRCustoms];
			statusFilter.Property = CMRConsolidatedCargoStatuses.Filter.Codes.NotClear;
			statusFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain HAWB1", !filterCollection.Contains(hAWB1));
			Assert("Expect collection to contain HAWB2", filterCollection.Contains(hAWB2));
			Assert("Expect collection not to contain HAWB3", !filterCollection.Contains(hAWB3));
		}

		public void TestStatusUnderbond()
		{
			hAWB1.CS_CM = mAWB1.PK;
			hAWB2.CS_CM = mAWB2.PK;
			underbond1.C4_ParentID = mAWB1.PK;
			underbond2.C4_ParentID = mAWB2.PK;
			underbond3.C4_ParentID = hAWB3.PK;
			underbond4.C4_ParentID = hAWB4.PK;
			entryNumber1.CE_ParentID = underbond1.PK;
			entryNumber2.CE_ParentID = underbond2.PK;
			entryNumber3.CE_ParentID = underbond3.PK;
			entryNumber4.CE_ParentID = underbond4.PK;
			entryNumber1.CE_EntryType = CusEntryNumber.EntryType.OutturnStatus;
			entryNumber2.CE_EntryType = CusEntryNumber.EntryType.UnderbondStatus;
			entryNumber3.CE_EntryType = CusEntryNumber.EntryType.UnderbondStatus;
			entryNumber4.CE_EntryType = CusEntryNumber.EntryType.UnderbondStatus;
			entryNumber1.CE_EntryStatus = CMRBaseStatuses.Codes.NotSent;
			entryNumber2.CE_EntryStatus = CMRBaseStatuses.Codes.NotSent;
			entryNumber3.CE_EntryStatus = CMRBaseStatuses.Codes.AmendmentAccepted;
			entryNumber4.CE_EntryStatus = CMRBaseStatuses.Codes.NotSent;
			Factory.Save();
			ModuleTextFilter statusFilter = (ModuleTextFilter)filterBO[AirCargoFilterConstants.StatusFilterType.CMRUnderbond];
			statusFilter.Property = CMRBaseStatuses.Codes.NotSent;
			statusFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain HAWB1", filterCollection.Contains(hAWB1));
			Assert("Expect collection to contain HAWB2", filterCollection.Contains(hAWB2));
			Assert("Expect collection not to contain HAWB3", !filterCollection.Contains(hAWB3));
			Assert("Expect collection to contain HAWB4", filterCollection.Contains(hAWB4));
		}

		public void TestStatusOutturn()
		{
			GlbCompany.GetCurrentCompany(Factory).OrgProxy.Addresses[0].LocalControlledPremisesID = "9914N";
			hAWB1.CS_CM = mAWB1.PK;
			hAWB2.CS_CM = mAWB2.PK;
			underbond1.C4_ParentID = mAWB1.PK;
			underbond2.C4_ParentID = mAWB2.PK;
			underbond3.C4_ParentID = hAWB3.PK;
			underbond4.C4_ParentID = hAWB4.PK;
			underbond2.C4_DestinationPremiseID = "9914N";
			underbond3.C4_DestinationPremiseID = "9914N";
			underbond4.C4_DestinationPremiseID = "9914N";
			underbond2.OutturnStatus.Code = CMRBaseStatuses.Codes.NotSent;
			underbond3.OutturnStatus.Code = CMRBaseStatuses.Codes.AmendmentAccepted;
			underbond4.OutturnStatus.Code = CMRBaseStatuses.Codes.NotSent;
			Factory.Save();
			ModuleTextFilter statusFilter = (ModuleTextFilter)filterBO[AirCargoFilterConstants.StatusFilterType.Outturn];
			statusFilter.Property = CMRBaseStatuses.Codes.NotSent;
			statusFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain HAWB1", !filterCollection.Contains(hAWB1));
			Assert("Expect collection to contain HAWB2", filterCollection.Contains(hAWB2));
			Assert("Expect collection not to contain HAWB3", !filterCollection.Contains(hAWB3));
			Assert("Expect collection to contain HAWB4", filterCollection.Contains(hAWB4));
		}

		public void TestStatusCMRMessage()
		{
			hAWB1.CS_MsgStatus = CMRBaseStatuses.Codes.AmendmentAccepted;
			hAWB2.CS_MsgStatus = CMRBaseStatuses.Codes.AwaitingResponseToAmendment;
			Factory.Save();
			ModuleTextFilter statusFilter = (ModuleTextFilter)filterBO[AirCargoFilterConstants.StatusFilterType.CMRMessage];
			statusFilter.Property = CMRBaseStatuses.Codes.AmendmentAccepted;
			statusFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain HAWB1", filterCollection.Contains(hAWB1));
			Assert("Expect collection not to contain HAWB2", !filterCollection.Contains(hAWB2));
		}

		public void TestWeDontMatchCTOMAWBs()
		{
			CTOCusMAWB cTOMAWB1 = Factory.NewWithValidTestData<CTOCusMAWB>();
			CTOCusHAWB cTOHAWB1 = Factory.NewWithValidTestData<CTOCusHAWB>();
			hAWB1.CS_CM = mAWB1.PK;
			cTOHAWB1.CS_CM = cTOMAWB1.PK;
			mAWB1.CM_FlightNo = "XAAAX";
			cTOMAWB1.CM_FlightNo = "YAAAY";
			hAWB2.CS_CM = mAWB2.PK;
			mAWB2.CM_FlightNo = "XAAAX";
			mAWB2.CM_ApplicationCode = "XXX";
			Factory.Save();
			ModuleNumberFilter flightNoFilter = (ModuleNumberFilter)filterBO[AirCargoFilterConstants.NumberFilterTypes.FlightNo];
			flightNoFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			flightNoFilter.Property = "AAA";
			flightNoFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain HAWB1", filterCollection.Contains(hAWB1));
			Assert("Expect collection not to contain CTOHAWB1", !filterCollection.Contains(cTOHAWB1));
			Assert("Expect collection not to contain HAWB2", !filterCollection.Contains(hAWB2));
		}

		public void TestBranchFilter()
		{
			var anotherCompany = Factory.New<GlbCompany>();
			var anotherBranch = anotherCompany.Branches.AddNew();
			mAWB1.CM_GB = anotherBranch.PK;
			mAWB1.CM_FlightNo = "XAAAX";
			mAWB2.CM_FlightNo = "XAAAX";
			hAWB1.CS_CM = mAWB1.PK;
			hAWB2.CS_CM = mAWB2.PK;
			Factory.Save();
			var flightNoFilter = (ModuleNumberFilter)filterBO[AirCargoFilterConstants.NumberFilterTypes.FlightNo];
			flightNoFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			flightNoFilter.Property = "AAA";
			flightNoFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Collection doesn't contains hawb with another branch", !filterCollection.Contains(hAWB1));
			Assert("Collection contains hawb with current branch", filterCollection.Contains(hAWB2));
		}

		public void TestArrivalDateFilter()
		{
			hAWB1.CS_CM = mAWB1.PK;
			hAWB2.CS_CM = mAWB2.PK;
			mAWB1.CM_ArrivalDate = new ZDateTime(2000, 1, 1, 11, 0, 0); // 1 Jan 11:00
			mAWB2.CM_ArrivalDate = new ZDateTime(2000, 2, 2, 22, 0, 0); // 2 Feb 22:00
			Factory.Save();
			ModuleDateFilter arrivalDateFilter = (ModuleDateFilter)filterBO["Arrival Date"];
			arrivalDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			arrivalDateFilter.Property1 = new ZDateTime(2000, 2, 2);
			arrivalDateFilter.Property2 = ZDateTime.Empty;
			arrivalDateFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain HAWB1.", !filterCollection.Contains(hAWB1));
			Assert("Expect collection to contain HAWB2.", filterCollection.Contains(hAWB2));
			arrivalDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			arrivalDateFilter.Property1 = ZDateTime.Empty;
			arrivalDateFilter.Property2 = new ZDateTime(2000, 1, 1);
			arrivalDateFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain HAWB1.", filterCollection.Contains(hAWB1));
			Assert("Expect collection not to contain HAWB2.", !filterCollection.Contains(hAWB2));
			arrivalDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			arrivalDateFilter.Property1 = new ZDateTime(2000, 1, 1);
			arrivalDateFilter.Property2 = new ZDateTime(2000, 2, 2);
			arrivalDateFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain HAWB1.", filterCollection.Contains(hAWB1));
			Assert("Expect collection to contain HAWB2.", filterCollection.Contains(hAWB2));
			arrivalDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			arrivalDateFilter.Property1 = new ZDateTime(2000, 1, 1);
			arrivalDateFilter.Property2 = new ZDateTime(2000, 1, 1);
			arrivalDateFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain HAWB1.", filterCollection.Contains(hAWB1));
			Assert("Expect collection not to contain HAWB2.", !filterCollection.Contains(hAWB2));
			arrivalDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			arrivalDateFilter.Property1 = new ZDateTime(2000, 1, 2);
			arrivalDateFilter.Property2 = new ZDateTime(2000, 2, 1);
			arrivalDateFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain HAWB1.", !filterCollection.Contains(hAWB1));
			Assert("Expect collection not to contain HAWB2.", !filterCollection.Contains(hAWB2));
		}

		public void TestCustomFieldsFilter()
		{
			var template1 = Factory.New<ProcessTaskTemplate>();
			template1.P0_ProcessType = WorkflowDescriptors.CustomsHouseAirCargoCode;
			template1.P0_Name = "HAC-AU";
			template1.P0_Description = "TEST HAC for AU";
			template1.P0_LoadPortCountry = "";
			template1.P0_DischargePortCountry = "AU";
			var customField1 = template1.GenCustomColumnDefinitions.AddNew();
			customField1.XC_Name = "TestStringField";
			customField1.XC_Type = AddOnColumnDataType.Codes.String;
			Factory.Save();
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_FlightNo = "QF123";
			mawb.CM_RL_NKLoadPort = "NZAKL";
			mawb.CM_RL_NKDischargePort = "AUSYD";
			var hawb1 = mawb.FilteredChildBills.AddNew();
			var hawb2 = mawb.FilteredChildBills.AddNew();
			ICustomFieldProvider customFieldProvider = hawb1;
			ICustomPropertyContainer customBusinessObject = customFieldProvider.GetCustomBusinessObject();
			var customProperty = customBusinessObject.CustomProperties.FirstOrDefault(x => x.Info.Type.Name == "ZString");
			AssertContains("TestStringField", customProperty.Identifier, ignoreCase: true);
			customProperty.TrySetValue(hawb1, new ZString("Test Value"));
			Factory.Save();
			filterBO = (AUCustomsHouseAirCargoFilterBusinessObject)GetNewFilterStripBusinessObject();
			WorkflowCustomFieldsFilter.ClearCache();
			Assert("Workflow Custom Fields category exists", filterBO.ModuleFilters.Cast<ModuleFilter>().Any(x => x.Category.Description == "Workflow Custom Fields"));
			var customFieldFilter = (ModuleTextFilter)filterBO["TestStringField"];
			customFieldFilter.Property = "Test Value";
			customFieldFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain HAWB1.", filterCollection.Contains(hawb1));
			Assert("Expect collection to NOT contain HAWB2.", !filterCollection.Contains(hawb2));
		}

		CusMAWB mAWB1;
		CusMAWB mAWB2;
		CusHAWB hAWB1;
		CusHAWB hAWB2;
		CusHAWB hAWB3;
		CusHAWB hAWB4;
		CusUnderbond underbond1;
		CusUnderbond underbond2;
		CusUnderbond underbond3;
		CusUnderbond underbond4;
		ForwardingConsol consol1;
		ForwardingConsol consol2;
		OrgHeader organisation1;
		OrgHeader organisation2;
		OrgHeader organisation3;
		CusEntryNumber entryNumber1;
		CusEntryNumber entryNumber2;
		CusEntryNumber entryNumber3;
		CusEntryNumber entryNumber4;
		ModuleHAWBCollection filterCollection;
		AUCustomsHouseAirCargoFilterBusinessObject filterBO;

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new AUCustomsHouseAirCargoFilterBusinessObject();

		protected override void SetUp()
		{
			base.SetUp();
			consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol2 = Factory.NewWithValidTestData<ForwardingConsol>();
			mAWB1 = Factory.NewWithValidTestData<CusMAWB>();
			mAWB2 = Factory.NewWithValidTestData<CusMAWB>();
			hAWB1 = Factory.NewWithValidTestData<CusHAWB>();
			hAWB2 = Factory.NewWithValidTestData<CusHAWB>();
			hAWB3 = Factory.NewWithValidTestData<CusHAWB>();
			hAWB4 = Factory.NewWithValidTestData<CusHAWB>();
			underbond1 = Factory.NewWithValidTestData<CusUnderbond>();
			underbond2 = Factory.NewWithValidTestData<CusUnderbond>();
			underbond3 = Factory.NewWithValidTestData<CusUnderbond>();
			underbond4 = Factory.NewWithValidTestData<CusUnderbond>();
			organisation1 = Factory.NewWithValidTestData<OrgHeader>();
			organisation2 = Factory.NewWithValidTestData<OrgHeader>();
			organisation3 = Factory.NewWithValidTestData<OrgHeader>();
			entryNumber1 = Factory.NewWithValidTestData<CusEntryNumber>();
			entryNumber1.CE_ParentTable = "CusUnderbond";
			entryNumber2 = Factory.NewWithValidTestData<CusEntryNumber>();
			entryNumber2.CE_ParentTable = "CusUnderbond";
			entryNumber3 = Factory.NewWithValidTestData<CusEntryNumber>();
			entryNumber3.CE_ParentTable = "CusUnderbond";
			entryNumber4 = Factory.NewWithValidTestData<CusEntryNumber>();
			entryNumber4.CE_ParentTable = "CusUnderbond";
			filterCollection = new ModuleHAWBCollection(Factory);
			filterBO = (AUCustomsHouseAirCargoFilterBusinessObject)GetNewFilterStripBusinessObject();
		}
	}
}
