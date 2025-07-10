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
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.AirCargo.Testing
{
	[TestedType(typeof(AUCustomsAirCargoFilterStripBusinessObject))]
	sealed class AUCustomsAirCargoFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestJobInvoicingStatusFilter()
		{
			var job1 = new JobHeader.Loader(mAWB1).TryLoadOrCreate();
			AssertNotNull(job1);
			AssertEquals(JobHeaderStatus.Working.Code, job1.JH_Status);
			Factory.Save();
			var filter = (ModuleTextBaseFilter)filterBO["Invoicing Job Status"];
			filter.Property = JobHeaderStatus.Working.Code;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			AssertCollectionContains(mAWB1, filterCollection);
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filterCollection.Load(filterBO.Filter);
			AssertCollectionNotContains(mAWB1, filterCollection);
		}

		public void TestHouseBillNumberFilter()
		{
			hAWB1.CS_CM = mAWB1.PK;
			hAWB2.CS_CM = mAWB2.PK;
			hAWB1.CS_HAWB = "ZZAAZ";
			hAWB2.CS_HAWB = "XAAXX";
			Factory.Save();
			ModuleNumberFilter houseBillNumberFilter = (ModuleNumberFilter)filterBO[AirCargoFilterConstants.NumberFilterTypes.HouseBillNumber];
			houseBillNumberFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			houseBillNumberFilter.Property = "ZZA";
			houseBillNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain MAWB1", filterCollection.Contains(mAWB1));
			Assert("Expect collection not to contain MAWB2", !filterCollection.Contains(mAWB2));
			houseBillNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			houseBillNumberFilter.Property = "XAAXX";
			houseBillNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain MAWB1", !filterCollection.Contains(mAWB1));
			Assert("Expect collection to contain MAWB2", filterCollection.Contains(mAWB2));
			houseBillNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			houseBillNumberFilter.Property = "AA";
			houseBillNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain MAWB1", filterCollection.Contains(mAWB1));
			Assert("Expect collection to contain MAWB2", filterCollection.Contains(mAWB2));
			houseBillNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			houseBillNumberFilter.Property = "YYYYY";
			houseBillNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain MAWB1", !filterCollection.Contains(mAWB1));
			Assert("Expect collection not to contain MAWB2", !filterCollection.Contains(mAWB2));
		}

		public void TestMasterBillNumberFilter()
		{
			mAWB1.CM_MAWB = "ZZZAAAZZZZZ";
			mAWB2.CM_MAWB = "XXXXXAAAXXX";
			Factory.Save();
			ModuleNumberFilter masterBillNumberFilter = (ModuleNumberFilter)filterBO[AirCargoFilterConstants.NumberFilterTypes.MasterBillNumber];
			filterCollection.Load(filterBO.Filter);
			masterBillNumberFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			masterBillNumberFilter.Property = "ZZZ";
			masterBillNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain MAWB1", filterCollection.Contains(mAWB1));
			Assert("Expect collection not to contain MAWB2", !filterCollection.Contains(mAWB2));
			masterBillNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			masterBillNumberFilter.Property = "XXXXXAAAXXX";
			masterBillNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain MAWB1", !filterCollection.Contains(mAWB1));
			Assert("Expect collection to contain MAWB2", filterCollection.Contains(mAWB2));
			masterBillNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			masterBillNumberFilter.Property = "AAA";
			masterBillNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain MAWB1", filterCollection.Contains(mAWB1));
			Assert("Expect collection to contain MAWB2", filterCollection.Contains(mAWB2));
			masterBillNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			masterBillNumberFilter.Property = "YYYYYYYYYYY";
			masterBillNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain MAWB1", !filterCollection.Contains(mAWB1));
			Assert("Expect collection not to contain MAWB2", !filterCollection.Contains(mAWB2));
		}

		public void TestConsolNumberFilter()
		{
			mAWB1.CM_JK = consol1.PK;
			mAWB2.CM_JK = consol2.PK;
			consol1.JK_UniqueConsignRef = "";
			consol2.JK_UniqueConsignRef = "";
			Factory.Save();
			ModuleNumberFilter consolNumberFilter = (ModuleNumberFilter)filterBO[AirCargoFilterConstants.NumberFilterTypes.ConsolNumber];
			consolNumberFilter.Property = consol1.JK_UniqueConsignRef;
			consolNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain MAWB1", filterCollection.Contains(mAWB1));
			Assert("Expect collection not to contain MAWB2", !filterCollection.Contains(mAWB2));
			consolNumberFilter.Property = consol2.JK_UniqueConsignRef.Substring(1);
			consolNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain MAWB1", !filterCollection.Contains(mAWB1));
			Assert("Expect collection to contain MAWB2", filterCollection.Contains(mAWB2));
			consolNumberFilter.Property = "Nothing";
			consolNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain MAWB1", !filterCollection.Contains(mAWB1));
			Assert("Expect collection not to contain MAWB2", !filterCollection.Contains(mAWB2));
		}

		public void TestJobNumberFilter()
		{
			hAWB1.CS_CM = mAWB1.PK;
			hAWB2.CS_CM = mAWB2.PK;
			hAWB1.CS_MessageReference = "A09999999";
			hAWB2.CS_MessageReference = "A09999998";
			Factory.Save();
			ModuleNumberFilter jobNumberFilter = (ModuleNumberFilter)filterBO[AirCargoFilterConstants.NumberFilterTypes.JobNumber];
			jobNumberFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			jobNumberFilter.Property = "9999999";
			jobNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain MAWB1", filterCollection.Contains(mAWB1));
			Assert("Expect collection not to contain MAWB2", !filterCollection.Contains(mAWB2));
			jobNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			jobNumberFilter.Property = "A09999998";
			jobNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain MAWB1", !filterCollection.Contains(mAWB1));
			Assert("Expect collection to contain MAWB2", filterCollection.Contains(mAWB2));
			jobNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			jobNumberFilter.Property = "9999";
			jobNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain MAWB1", !filterCollection.Contains(mAWB1));
			Assert("Expect collection not to contain MAWB2", !filterCollection.Contains(mAWB2));
		}

		public void TestFlightNoFilterForPartShip()
		{
			mAWB1.CM_FlightNo = "XBBBX";
			mAWB2.CM_FlightNo = "ZAAAZ";
			hAWB1.CS_CM = mAWB1.PK;
			underbond1.C4_ParentID = hAWB1.PK;
			underbond1.C4_FlightNo = "YAAAY";
			Factory.Save();
			ModuleNumberFilter flightNoFilter = (ModuleNumberFilter)filterBO[AirCargoFilterConstants.NumberFilterTypes.FlightNo];
			flightNoFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			flightNoFilter.Property = "XBB";
			flightNoFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain MAWB1", filterCollection.Contains(mAWB1));
			Assert("Expect collection not to contain MAWB2", !filterCollection.Contains(mAWB2));
			flightNoFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			flightNoFilter.Property = "YAAAY";
			flightNoFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain MAWB1", filterCollection.Contains(mAWB1));
			Assert("Expect collection not to contain MAWB2", !filterCollection.Contains(mAWB2));
			flightNoFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			flightNoFilter.Property = "AAA";
			flightNoFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain MAWB1", filterCollection.Contains(mAWB1));
			Assert("Expect collection to contain MAWB2", filterCollection.Contains(mAWB2));
			flightNoFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			flightNoFilter.Property = "CCC";
			flightNoFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain HAWB1", !filterCollection.Contains(hAWB1));
			Assert("Expect collection not to contain HAWB2", !filterCollection.Contains(hAWB2));
		}

		public void TestFlightNoFilter()
		{
			mAWB1.CM_FlightNo = "XAAAX";
			mAWB2.CM_FlightNo = "YAAAY";
			Factory.Save();
			ModuleNumberFilter flightNoFilter = (ModuleNumberFilter)filterBO[AirCargoFilterConstants.NumberFilterTypes.FlightNo];
			flightNoFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			flightNoFilter.Property = "XAA";
			flightNoFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain MAWB1", filterCollection.Contains(mAWB1));
			Assert("Expect collection not to contain MAWB2", !filterCollection.Contains(mAWB2));
			flightNoFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			flightNoFilter.Property = "YAAAY";
			flightNoFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain MAWB1", !filterCollection.Contains(mAWB1));
			Assert("Expect collection to contain MAWB2", filterCollection.Contains(mAWB2));
			flightNoFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			flightNoFilter.Property = "AAA";
			flightNoFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain MAWB1", filterCollection.Contains(mAWB1));
			Assert("Expect collection to contain MAWB2", filterCollection.Contains(mAWB2));
			flightNoFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			flightNoFilter.Property = "ZZZ";
			flightNoFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain MAWB1", !filterCollection.Contains(mAWB1));
			Assert("Expect collection not to contain MAWB2", !filterCollection.Contains(mAWB2));
		}

		public void TestCoLoadMasterFilter()
		{
			hAWB1.CS_MasterHouseBill = "ZZAAZ";
			hAWB2.CS_MasterHouseBill = "XAAXX";
			hAWB1.CS_CM = mAWB1.PK;
			hAWB2.CS_CM = mAWB2.PK;
			Factory.Save();
			ModuleNumberFilter coLoadMasterFilter = (ModuleNumberFilter)filterBO[AirCargoFilterConstants.NumberFilterTypes.CoLoadMaster];
			coLoadMasterFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			coLoadMasterFilter.Property = "ZZA";
			coLoadMasterFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain MAWB1", filterCollection.Contains(mAWB1));
			Assert("Expect collection not to contain MAWB2", !filterCollection.Contains(mAWB2));
			coLoadMasterFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			coLoadMasterFilter.Property = "XAAXX";
			coLoadMasterFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain MAWB1", !filterCollection.Contains(mAWB1));
			Assert("Expect collection to contain MAWB2", filterCollection.Contains(mAWB2));
			coLoadMasterFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			coLoadMasterFilter.Property = "AA";
			coLoadMasterFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain MAWB1", filterCollection.Contains(mAWB1));
			Assert("Expect collection to contain MAWB2", filterCollection.Contains(mAWB2));
			coLoadMasterFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			coLoadMasterFilter.Property = "YYYYY";
			coLoadMasterFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain MAWB1", !filterCollection.Contains(mAWB1));
			Assert("Expect collection not to contain MAWB2", !filterCollection.Contains(mAWB2));
		}

		public void TestConRefNumberFilter()
		{
			AUCustomsDataRegistry.Instance.ActivateAlternatePartShipmentModel.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			filterBO = (AUCustomsAirCargoFilterStripBusinessObject)GetNewFilterStripBusinessObject();
			hAWB1.CS_CM = mAWB1.PK;
			hAWB2.CS_CM = mAWB2.PK;
			hAWB1.CS_HAWB = "ZZAAZ";
			hAWB1.CS_fPartShipConsignmentReference = "CONREFAAA";
			hAWB2.CS_HAWB = "XAAXX";
			hAWB2.CS_fPartShipConsignmentReference = "CONREFBBB";
			Factory.Save();
			var conRefNumberFilter = (ModuleNumberFilter)filterBO[AirCargoFilterConstants.NumberFilterTypes.ConRef];
			conRefNumberFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			conRefNumberFilter.Property = "CONREFA";
			conRefNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain MAWB1", filterCollection.Contains(mAWB1));
			Assert("Expect collection not to contain MAWB2", !filterCollection.Contains(mAWB2));
			conRefNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			conRefNumberFilter.Property = "CONREFBBB";
			conRefNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain MAWB1", !filterCollection.Contains(mAWB1));
			Assert("Expect collection to contain MAWB2", filterCollection.Contains(mAWB2));
			conRefNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			conRefNumberFilter.Property = "REF";
			conRefNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain MAWB1", filterCollection.Contains(mAWB1));
			Assert("Expect collection to contain MAWB2", filterCollection.Contains(mAWB2));
			conRefNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			conRefNumberFilter.Property = "YYYYY";
			conRefNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain MAWB1", !filterCollection.Contains(mAWB1));
			Assert("Expect collection not to contain MAWB2", !filterCollection.Contains(mAWB2));
		}

		public void TestConsigneeFilter()
		{
			organisation1.OH_IsConsignee = true;
			organisation2.OH_IsConsignee = true;
			organisation3.OH_IsConsignee = true;
			hAWB1.CS_OA_ConsigneeAddress = organisation1.MainAddress.PK;
			hAWB2.CS_OA_ConsigneeAddress = organisation2.MainAddress.PK;
			hAWB1.CS_CM = mAWB1.PK;
			hAWB2.CS_CM = mAWB2.PK;
			Factory.Save();
			ModuleGuidsFilter consigneeFilter = (ModuleGuidsFilter)filterBO[string.Format("{0} / {1}", AirCargoFilterConstants.PartyFilterTypes.Consignor, AirCargoFilterConstants.PartyFilterTypes.Consignee)];
			consigneeFilter.Property2 = organisation1.PK;
			consigneeFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain MAWB1", filterCollection.Contains(mAWB1));
			Assert("Expect collection not to contain MAWB2", !filterCollection.Contains(mAWB2));
			consigneeFilter.Property2 = organisation3.PK;
			consigneeFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain MAWB1", !filterCollection.Contains(mAWB1));
			Assert("Expect collection not to contain MAWB2", !filterCollection.Contains(mAWB2));
		}

		public void TestConsignorFilter()
		{
			organisation1.OH_IsConsignor = true;
			organisation2.OH_IsConsignor = true;
			organisation3.OH_IsConsignor = true;
			hAWB1.CS_OA_ConsignorAddress = organisation1.MainAddress.PK;
			hAWB2.CS_OA_ConsignorAddress = organisation2.MainAddress.PK;
			hAWB1.CS_CM = mAWB1.PK;
			hAWB2.CS_CM = mAWB2.PK;
			Factory.Save();
			ModuleGuidsFilter consignorFilter = (ModuleGuidsFilter)filterBO[string.Format("{0} / {1}", AirCargoFilterConstants.PartyFilterTypes.Consignor, AirCargoFilterConstants.PartyFilterTypes.Consignee)];
			consignorFilter.Property1 = organisation1.PK;
			consignorFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain MAWB1", filterCollection.Contains(mAWB1));
			Assert("Expect collection not to contain MAWB2", !filterCollection.Contains(mAWB2));
			consignorFilter.Property1 = organisation3.PK;
			consignorFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain MAWB1", !filterCollection.Contains(mAWB1));
			Assert("Expect collection not to contain MAWB2", !filterCollection.Contains(mAWB2));
		}

		public void TestLocationLoadDischarge()
		{
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
			Assert("Expect collection to contain MAWB1", filterCollection.Contains(mAWB1));
			Assert("Expect collection not to contain MAWB2", !filterCollection.Contains(mAWB2));
			loadDichargeFilter.Property1 = "AUBNE";
			loadDichargeFilter.Property2 = "AUSYD";
			loadDichargeFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain MAWB1", !filterCollection.Contains(mAWB1));
			Assert("Expect collection to contain MAWB2", filterCollection.Contains(mAWB2));
			loadDichargeFilter.Property1 = ZString.Empty;
			loadDichargeFilter.Property2 = "AUSYD";
			loadDichargeFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain MAWB1", filterCollection.Contains(mAWB1));
			Assert("Expect collection to contain MAWB2", filterCollection.Contains(mAWB2));
			loadDichargeFilter.Property1 = "AUSYD";
			loadDichargeFilter.Property2 = ZString.Empty;
			loadDichargeFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain MAWB1", !filterCollection.Contains(mAWB1));
			Assert("Expect collection not to contain MAWB2", !filterCollection.Contains(mAWB2));
		}

		public void TestLocationOriginDestination()
		{
			hAWB1.CS_CM = mAWB1.PK;
			hAWB2.CS_CM = mAWB2.PK;
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
			Assert("Expect collection to contain MAWB1", filterCollection.Contains(mAWB1));
			Assert("Expect collection not to contain MAWB2", !filterCollection.Contains(mAWB2));
			loadDichargeFilter.Property1 = "AUBNE";
			loadDichargeFilter.Property2 = "AUSYD";
			loadDichargeFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain MAWB1", !filterCollection.Contains(mAWB1));
			Assert("Expect collection to contain MAWB2", filterCollection.Contains(mAWB2));
			loadDichargeFilter.Property1 = ZString.Empty;
			loadDichargeFilter.Property2 = "AUSYD";
			loadDichargeFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain MAWB1", filterCollection.Contains(mAWB1));
			Assert("Expect collection to contain MAWB2", filterCollection.Contains(mAWB2));
			loadDichargeFilter.Property1 = "AUSYD";
			loadDichargeFilter.Property2 = ZString.Empty;
			loadDichargeFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain MAWB1", !filterCollection.Contains(mAWB1));
			Assert("Expect collection not to contain MAWB2", !filterCollection.Contains(mAWB2));
		}

		public void TestStatusHeldOrConditional()
		{
			hAWB1.CS_CM = mAWB1.PK;
			hAWB2.CS_CM = mAWB2.PK;
			hAWB3.CS_CM = mAWB3.PK;
			hAWB1.CS_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.CondclearCargoCanBeReleasedIntoHomeConsumptionSubjectToConditionSTheseConditionsAreProvidedInSupplementaryInformation;
			hAWB2.CS_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.AcsseizedCargoIsSeizedByCustoms;
			hAWB3.CS_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl;
			Factory.Save();
			ModuleTextFilter statusFilter = (ModuleTextFilter)filterBO[AirCargoFilterConstants.StatusFilterType.CMRCustoms];
			statusFilter.Property = CMRConsolidatedCargoStatuses.Filter.Codes.HeldOrConditional;
			statusFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain MAWB1", filterCollection.Contains(mAWB1));
			Assert("Expect collection not to contain MAWB2", !filterCollection.Contains(mAWB2));
			Assert("Expect collection to contain MAWB3", filterCollection.Contains(mAWB3));
		}

		public void TestStatusClearOrConditional()
		{
			hAWB1.CS_CM = mAWB1.PK;
			hAWB2.CS_CM = mAWB2.PK;
			hAWB3.CS_CM = mAWB3.PK;
			hAWB1.CS_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased;
			hAWB2.CS_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.AcsseizedCargoIsSeizedByCustoms;
			hAWB3.CS_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.CondclearCargoCanBeReleasedIntoHomeConsumptionSubjectToConditionSTheseConditionsAreProvidedInSupplementaryInformation;
			Factory.Save();
			ModuleTextFilter statusFilter = (ModuleTextFilter)filterBO[AirCargoFilterConstants.StatusFilterType.CMRCustoms];
			statusFilter.Property = CMRConsolidatedCargoStatuses.Filter.Codes.ClearOrConditional;
			statusFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain MAWB1", filterCollection.Contains(mAWB1));
			Assert("Expect collection not to contain MAWB2", !filterCollection.Contains(mAWB2));
			Assert("Expect collection to contain MAWB3", filterCollection.Contains(mAWB3));
		}

		public void TestStatusCMRCustoms()
		{
			hAWB1.CS_CM = mAWB1.PK;
			hAWB2.CS_CM = mAWB2.PK;
			hAWB1.CS_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.ClearhrmCargoIsClearButIsIdentifiedAsHighRiskMovement;
			hAWB2.CS_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.AcsseizedCargoIsSeizedByCustoms;
			Factory.Save();
			ModuleTextFilter statusFilter = (ModuleTextFilter)filterBO[AirCargoFilterConstants.StatusFilterType.CMRCustoms];
			statusFilter.Property = CMRConsolidatedCargoStatuses.Codes.ClearhrmCargoIsClearButIsIdentifiedAsHighRiskMovement;
			statusFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain MAWB1", filterCollection.Contains(mAWB1));
			Assert("Expect collection not to contain MAWB2", !filterCollection.Contains(mAWB2));
			statusFilter.Property = CMRConsolidatedCargoStatuses.Codes.AcsseizedCargoIsSeizedByCustoms;
			statusFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain MAWB1", !filterCollection.Contains(mAWB1));
			Assert("Expect collection to contain MAWB2", filterCollection.Contains(mAWB2));
			statusFilter.Property = CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl;
			statusFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain MAWB1", !filterCollection.Contains(mAWB1));
			Assert("Expect collection not to contain MAWB2", !filterCollection.Contains(mAWB2));
		}

		public void TestStatusCMRCustomsNotClear()
		{
			hAWB1.CS_CM = mAWB1.PK;
			hAWB2.CS_CM = mAWB2.PK;
			hAWB3.CS_CM = mAWB3.PK;
			hAWB1.CS_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased;
			hAWB2.CS_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.AcsseizedCargoIsSeizedByCustoms;
			hAWB3.CS_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.ClearhrmCargoIsClearButIsIdentifiedAsHighRiskMovement;
			Factory.Save();
			ModuleTextFilter statusFilter = (ModuleTextFilter)filterBO[AirCargoFilterConstants.StatusFilterType.CMRCustoms];
			statusFilter.Property = CMRConsolidatedCargoStatuses.Filter.Codes.NotClear;
			statusFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain MAWB1", !filterCollection.Contains(mAWB1));
			Assert("Expect collection to contain MAWB2", filterCollection.Contains(mAWB2));
			Assert("Expect collection not to contain MAWB3", !filterCollection.Contains(mAWB3));
		}

		public void TestStatusCMRMessage()
		{
			hAWB1.CS_CM = mAWB1.PK;
			hAWB2.CS_CM = mAWB2.PK;
			hAWB1.CS_MsgStatus = CMRBaseStatuses.Codes.AmendmentAccepted;
			hAWB2.CS_MsgStatus = CMRBaseStatuses.Codes.AwaitingResponseToAmendment;
			Factory.Save();
			ModuleTextFilter statusFilter = (ModuleTextFilter)filterBO[AirCargoFilterConstants.StatusFilterType.CMRMessage];
			statusFilter.Property = CMRBaseStatuses.Codes.AmendmentAccepted;
			statusFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain MAWB1", filterCollection.Contains(mAWB1));
			Assert("Expect collection not to contain MAWB2", !filterCollection.Contains(mAWB2));
		}

		public void TestStatusUnderbond()
		{
			hAWB1.CS_CM = mAWB1.PK;
			hAWB2.CS_CM = mAWB2.PK;
			underbond1.C4_ParentID = hAWB1.PK;
			underbond2.C4_ParentID = hAWB2.PK;
			underbond3.C4_ParentID = mAWB3.PK;
			underbond4.C4_ParentID = mAWB4.PK;
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
			ModuleTextFilter statusFilter = (ModuleTextFilter)filterBO[AirCargoFilterConstants.StatusFilterType.CMRUnderbondBoth];
			statusFilter.Property = CMRBaseStatuses.Codes.NotSent;
			statusFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain MAWB1", filterCollection.Contains(mAWB1));
			Assert("Expect collection to contain MAWB2", filterCollection.Contains(mAWB2));
			Assert("Expect collection not to contain MAWB3", !filterCollection.Contains(mAWB3));
			Assert("Expect collection to contain MAWB4", filterCollection.Contains(mAWB4));
			statusFilter = (ModuleTextFilter)filterBO[AirCargoFilterConstants.StatusFilterType.CMRUnderbondMaster];
			statusFilter.Property = CMRBaseStatuses.Codes.NotSent;
			statusFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain MAWB1", !filterCollection.Contains(mAWB1));
			Assert("Expect collection to contain MAWB2", !filterCollection.Contains(mAWB2));
			Assert("Expect collection not to contain MAWB3", !filterCollection.Contains(mAWB3));
			Assert("Expect collection to contain MAWB4", filterCollection.Contains(mAWB4));
		}

		public void TestStatusOutturn()
		{
			GlbCompany.GetCurrentCompany(Factory).OrgProxy.Addresses[0].LocalControlledPremisesID = "9914N";
			hAWB1.CS_CM = mAWB1.PK;
			hAWB2.CS_CM = mAWB2.PK;
			underbond1.C4_ParentID = hAWB1.PK;
			underbond2.C4_ParentID = hAWB2.PK;
			underbond3.C4_ParentID = mAWB3.PK;
			underbond4.C4_ParentID = mAWB4.PK;
			underbond2.C4_DestinationPremiseID = "9914N";
			underbond3.C4_DestinationPremiseID = "9914N";
			underbond4.C4_DestinationPremiseID = "9914N";
			underbond2.OutturnStatus.Code = CMRBaseStatuses.Codes.NotSent;
			underbond3.OutturnStatus.Code = CMRBaseStatuses.Codes.AmendmentAccepted;
			underbond4.OutturnStatus.Code = CMRBaseStatuses.Codes.NotSent;
			Factory.Save();
			ModuleTextFilter statusFilter = (ModuleTextFilter)filterBO[AirCargoFilterConstants.StatusFilterType.OutturnBoth];
			statusFilter.Property = CMRBaseStatuses.Codes.NotSent;
			statusFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain MAWB1", !filterCollection.Contains(mAWB1));
			Assert("Expect collection to contain MAWB2", filterCollection.Contains(mAWB2));
			Assert("Expect collection not to contain MAWB3", !filterCollection.Contains(mAWB3));
			Assert("Expect collection to contain MAWB4", filterCollection.Contains(mAWB4));
			statusFilter = (ModuleTextFilter)filterBO[AirCargoFilterConstants.StatusFilterType.OutturnMaster];
			statusFilter.Property = CMRBaseStatuses.Codes.NotSent;
			statusFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain MAWB1", !filterCollection.Contains(mAWB1));
			Assert("Expect collection to contain MAWB2", !filterCollection.Contains(mAWB2));
			Assert("Expect collection not to contain MAWB3", !filterCollection.Contains(mAWB3));
			Assert("Expect collection to contain MAWB4", filterCollection.Contains(mAWB4));
		}

		public void TestWeDontMatchCTOMAWBs()
		{
			CTOCusMAWB cTOMAWB1 = Factory.NewWithValidTestData<CTOCusMAWB>();
			mAWB1.CM_FlightNo = "XAAAX";
			cTOMAWB1.CM_FlightNo = "YAAAY";
			mAWB2.CM_FlightNo = "XAAAX";
			mAWB2.CM_ApplicationCode = "XXX";
			Factory.Save();
			ModuleNumberFilter flightNoFilter = (ModuleNumberFilter)filterBO[AirCargoFilterConstants.NumberFilterTypes.FlightNo];
			flightNoFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			flightNoFilter.Property = "AAA";
			flightNoFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain MAWB1", filterCollection.Contains(mAWB1));
			Assert("Expect collection not to contain CTOMAWB1", !filterCollection.Contains(cTOMAWB1));
			Assert("Expect collection not to contain MAWB2", !filterCollection.Contains(mAWB2));
		}

		public void TestBranchFilter()
		{
			var anotherCompany = Factory.New<GlbCompany>();
			var anotherBranch = anotherCompany.Branches.AddNew();
			mAWB1.CM_GB = anotherBranch.PK;
			mAWB1.CM_FlightNo = "XAAAX";
			mAWB2.CM_FlightNo = "XAAAX";
			Factory.Save();
			var flightNoFilter = (ModuleNumberFilter)filterBO[AirCargoFilterConstants.NumberFilterTypes.FlightNo];
			flightNoFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			flightNoFilter.Property = "AAA";
			flightNoFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Collection doesn't contains mawb with another branch", !filterCollection.Contains(mAWB1));
			Assert("Collection contains mawb with current branch", filterCollection.Contains(mAWB2));
		}

		public void TestArrivalDateFilter()
		{
			mAWB1.CM_ArrivalDate = new ZDateTime(2000, 1, 1, 11, 0, 0); // 1 Jan 11:00
			mAWB2.CM_ArrivalDate = new ZDateTime(2000, 2, 2, 22, 0, 0); // 2 Feb 22:00
			Factory.Save();
			ModuleDateFilter arrivalDateFilter = (ModuleDateFilter)filterBO["Arrival Date"];
			arrivalDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			arrivalDateFilter.Property1 = new ZDateTime(2000, 2, 2);
			arrivalDateFilter.Property2 = ZDateTime.Empty;
			arrivalDateFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain MAWB1.", !filterCollection.Contains(mAWB1));
			Assert("Expect collection to contain MAWB2.", filterCollection.Contains(mAWB2));
			arrivalDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			arrivalDateFilter.Property1 = ZDateTime.Empty;
			arrivalDateFilter.Property2 = new ZDateTime(2000, 1, 1);
			arrivalDateFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain MAWB1.", filterCollection.Contains(mAWB1));
			Assert("Expect collection not to contain MAWB2.", !filterCollection.Contains(mAWB2));
			arrivalDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			arrivalDateFilter.Property1 = new ZDateTime(2000, 1, 1);
			arrivalDateFilter.Property2 = new ZDateTime(2000, 2, 2);
			arrivalDateFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain MAWB1.", filterCollection.Contains(mAWB1));
			Assert("Expect collection to contain MAWB2.", filterCollection.Contains(mAWB2));
			arrivalDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			arrivalDateFilter.Property1 = new ZDateTime(2000, 1, 1);
			arrivalDateFilter.Property2 = new ZDateTime(2000, 1, 1);
			arrivalDateFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain MAWB1.", filterCollection.Contains(mAWB1));
			Assert("Expect collection not to contain MAWB2.", !filterCollection.Contains(mAWB2));
			arrivalDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			arrivalDateFilter.Property1 = new ZDateTime(2000, 1, 2);
			arrivalDateFilter.Property2 = new ZDateTime(2000, 2, 1);
			arrivalDateFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain MAWB1.", !filterCollection.Contains(mAWB1));
			Assert("Expect collection not to contain MAWB2.", !filterCollection.Contains(mAWB2));
		}

		public void TestCustomFieldsFilter()
		{
			var template1 = Factory.New<ProcessTaskTemplate>();
			template1.P0_ProcessType = JobInvoicingConsumerTypes.CusMAWBCode;
			template1.P0_Name = "ACR-AU";
			template1.P0_Description = "TEST ACR for AU";
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
			ICustomFieldProvider customFieldProvider = mawb;
			ICustomPropertyContainer customBusinessObject = customFieldProvider.GetCustomBusinessObject();
			var customProperty = customBusinessObject.CustomProperties.FirstOrDefault(x => x.Info.Type.Name == "ZString");
			AssertContains("TestStringField", customProperty.Identifier, ignoreCase: true);
			customProperty.TrySetValue(mawb, new ZString("Test Value"));
			Factory.Save();
			filterBO = (AUCustomsAirCargoFilterStripBusinessObject)GetNewFilterStripBusinessObject();
			WorkflowCustomFieldsFilter.ClearCache();
			Assert("Workflow Custom Fields category exists", filterBO.ModuleFilters.Cast<ModuleFilter>().Any(x => x.Category.Description == "Workflow Custom Fields"));
			var customFieldFilter = (ModuleTextFilter)filterBO["TestStringField"];
			customFieldFilter.Property = "Test Value";
			customFieldFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain MAWB.", filterCollection.Contains(mawb));
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new AUCustomsAirCargoFilterStripBusinessObject();

		ForwardingConsol consol1;
		ForwardingConsol consol2;
		CusMAWB mAWB1;
		CusMAWB mAWB2;
		CusMAWB mAWB3;
		CusMAWB mAWB4;
		CusHAWB hAWB1;
		CusHAWB hAWB2;
		CusHAWB hAWB3;
		CusUnderbond underbond1;
		CusUnderbond underbond2;
		CusUnderbond underbond3;
		CusUnderbond underbond4;
		OrgHeader organisation1;
		OrgHeader organisation2;
		OrgHeader organisation3;
		CusEntryNumber entryNumber1;
		CusEntryNumber entryNumber2;
		CusEntryNumber entryNumber3;
		CusEntryNumber entryNumber4;
		ModuleMAWBCollection filterCollection;
		AUCustomsAirCargoFilterStripBusinessObject filterBO;
		protected override void SetUp()
		{
			base.SetUp();
			consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol2 = Factory.NewWithValidTestData<ForwardingConsol>();
			mAWB1 = Factory.NewWithValidTestData<CusMAWB>();
			mAWB2 = Factory.NewWithValidTestData<CusMAWB>();
			mAWB3 = Factory.NewWithValidTestData<CusMAWB>();
			mAWB4 = Factory.NewWithValidTestData<CusMAWB>();
			hAWB1 = Factory.NewWithValidTestData<CusHAWB>();
			hAWB2 = Factory.NewWithValidTestData<CusHAWB>();
			hAWB3 = Factory.NewWithValidTestData<CusHAWB>();
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
			filterCollection = new ModuleMAWBCollection(Factory);
			filterBO = (AUCustomsAirCargoFilterStripBusinessObject)GetNewFilterStripBusinessObject();
		}
	}
}
