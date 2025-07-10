using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.Testing
{
	[TestedType(typeof(SeaCargoFilterStripBusinessObject))]
	sealed class SeaCargoFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestCustomFieldFilter()
		{
			var filterCollection = new SeaCargoFilterStripBusinessObject().ModuleFilters;
			AssertNull(filterCollection["stringField"]);
			AssertNull(filterCollection["intField"]);
			AssertNull(filterCollection["dateTimeField"]);
			AssertNull(filterCollection["boolField"]);
			var template = CreateWorkflowTemplate(WorkflowDescriptors.CusSCAOceanBillWorkflowDescriptorCode);
			AddCustomField(template, "stringField", AddOnColumnDataType.Codes.String);
			AddCustomField(template, "intField", AddOnColumnDataType.Codes.Integer);
			AddCustomField(template, "dateTimeField", AddOnColumnDataType.Codes.Datetime);
			AddCustomField(template, "boolField", AddOnColumnDataType.Codes.Boolean);
			Factory.Save();
			WorkflowCustomFieldsFilter.ClearCache();
			filterCollection = new SeaCargoFilterStripBusinessObject().ModuleFilters;
			AssertEquals(typeof(ModuleTextFilter), filterCollection["stringField"].GetType());
			AssertEquals(typeof(ModuleNumberRangeFilter), filterCollection["intField"].GetType());
			AssertEquals(typeof(ModuleDateFilter), filterCollection["dateTimeField"].GetType());
			AssertEquals(typeof(ModuleFlagsFilter), filterCollection["Workflow Flags"].GetType());
		}

		public void TestOceanBillNumberFilter()
		{
			OceanBill1.CB_OceanBill = "XAAAX";
			OceanBill2.CB_OceanBill = "YAAAY";
			Factory.Save();
			ModuleNumberFilter oceanBillNumberFilter = (ModuleNumberFilter)filterBO[SeaCargoFilterConstants.NumberFilterTypes.OceanBillNumber];
			oceanBillNumberFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			oceanBillNumberFilter.Property = "XAA";
			oceanBillNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain OceanBill1", filterCollection.Contains(OceanBill1));
			Assert("Expect collection not to contain OceanBill2", !filterCollection.Contains(OceanBill2));
			oceanBillNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			oceanBillNumberFilter.Property = "YAAAY";
			oceanBillNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain OceanBill1", !filterCollection.Contains(OceanBill1));
			Assert("Expect collection to contain OceanBill2", filterCollection.Contains(OceanBill2));
			oceanBillNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			oceanBillNumberFilter.Property = "AAA";
			oceanBillNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain OceanBill1", filterCollection.Contains(OceanBill1));
			Assert("Expect collection to contain OceanBill2", filterCollection.Contains(OceanBill2));
			oceanBillNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			oceanBillNumberFilter.Property = "ZZZ";
			oceanBillNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain OceanBill1", !filterCollection.Contains(OceanBill1));
			Assert("Expect collection not to contain OceanBill2", !filterCollection.Contains(OceanBill2));
		}

		public void TestParentBillNumberFilter()
		{
			OceanBill1.CB_MasterHouseBill = "XAAAX";
			OceanBill2.CB_MasterHouseBill = "YAAAY";
			Factory.Save();
			ModuleNumberFilter parentBillNumberFilter = (ModuleNumberFilter)filterBO[SeaCargoFilterConstants.NumberFilterTypes.ParentBillNumber];
			parentBillNumberFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			parentBillNumberFilter.Property = "XAA";
			parentBillNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain OceanBill1", filterCollection.Contains(OceanBill1));
			Assert("Expect collection not to contain OceanBill2", !filterCollection.Contains(OceanBill2));
			parentBillNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			parentBillNumberFilter.Property = "YAAAY";
			parentBillNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain OceanBill1", !filterCollection.Contains(OceanBill1));
			Assert("Expect collection to contain OceanBill2", filterCollection.Contains(OceanBill2));
			parentBillNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			parentBillNumberFilter.Property = "AAA";
			parentBillNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain OceanBill1", filterCollection.Contains(OceanBill1));
			Assert("Expect collection to contain OceanBill2", filterCollection.Contains(OceanBill2));
			parentBillNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			parentBillNumberFilter.Property = "ZZZ";
			parentBillNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain OceanBill1", !filterCollection.Contains(OceanBill1));
			Assert("Expect collection not to contain OceanBill2", !filterCollection.Contains(OceanBill2));
		}

		public void TestResponsiblePartyIDFilter()
		{
			OceanBill1.CB_ResponsiblePartyID = "XAAAX";
			OceanBill2.CB_ResponsiblePartyID = "YAAAY";
			Factory.Save();
			ModuleNumberFilter responsiblePartyIdFilter = (ModuleNumberFilter)filterBO[SeaCargoFilterConstants.NumberFilterTypes.ResponsiblePartyID];
			responsiblePartyIdFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			responsiblePartyIdFilter.Property = "XAA";
			responsiblePartyIdFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain OceanBill1", filterCollection.Contains(OceanBill1));
			Assert("Expect collection not to contain OceanBill2", !filterCollection.Contains(OceanBill2));
			responsiblePartyIdFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			responsiblePartyIdFilter.Property = "YAAAY";
			responsiblePartyIdFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain OceanBill1", !filterCollection.Contains(OceanBill1));
			Assert("Expect collection to contain OceanBill2", filterCollection.Contains(OceanBill2));
			responsiblePartyIdFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			responsiblePartyIdFilter.Property = "AAA";
			responsiblePartyIdFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain OceanBill1", filterCollection.Contains(OceanBill1));
			Assert("Expect collection to contain OceanBill2", filterCollection.Contains(OceanBill2));
			responsiblePartyIdFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			responsiblePartyIdFilter.Property = "ZZZ";
			responsiblePartyIdFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain OceanBill1", !filterCollection.Contains(OceanBill1));
			Assert("Expect collection not to contain OceanBill2", !filterCollection.Contains(OceanBill2));
		}

		public void TestContainerNumberFilter()
		{
			Container1.CN_CB = OceanBill1.PK;
			Container2.CN_CB = OceanBill2.PK;
			Container1.CN_ContainerNumber = "XAAAX";
			Container2.CN_ContainerNumber = "YAAAY";
			Factory.Save();
			ModuleNumberFilter containerNumberFilter = (ModuleNumberFilter)filterBO[SeaCargoFilterConstants.NumberFilterTypes.ContainerNumber];
			containerNumberFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			containerNumberFilter.Property = "XAA";
			containerNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain OceanBill1", filterCollection.Contains(OceanBill1));
			Assert("Expect collection not to contain OceanBill2", !filterCollection.Contains(OceanBill2));
			containerNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			containerNumberFilter.Property = "YAAAY";
			containerNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain OceanBill1", !filterCollection.Contains(OceanBill1));
			Assert("Expect collection to contain OceanBill2", filterCollection.Contains(OceanBill2));
			containerNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			containerNumberFilter.Property = "AAA";
			containerNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain OceanBill1", filterCollection.Contains(OceanBill1));
			Assert("Expect collection to contain OceanBill2", filterCollection.Contains(OceanBill2));
			containerNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			containerNumberFilter.Property = "ZZZ";
			containerNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain OceanBill1", !filterCollection.Contains(OceanBill1));
			Assert("Expect collection not to contain OceanBill2", !filterCollection.Contains(OceanBill2));
		}

		public void TestHouseBillNumberFilter()
		{
			HouseBill1.CA_CB = OceanBill1.PK;
			HouseBill2.CA_CB = OceanBill2.PK;
			HouseBill1.CA_HouseBill = "XAAAX";
			HouseBill2.CA_HouseBill = "YAAAY";
			Factory.Save();
			ModuleNumberFilter houseBillNumberFilter = (ModuleNumberFilter)filterBO[SeaCargoFilterConstants.NumberFilterTypes.HouseBillNumber];
			houseBillNumberFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			houseBillNumberFilter.Property = "XAA";
			houseBillNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain OceanBill1", filterCollection.Contains(OceanBill1));
			Assert("Expect collection not to contain OceanBill2", !filterCollection.Contains(OceanBill2));
			houseBillNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			houseBillNumberFilter.Property = "YAAAY";
			houseBillNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain OceanBill1", !filterCollection.Contains(OceanBill1));
			Assert("Expect collection to contain OceanBill2", filterCollection.Contains(OceanBill2));
			houseBillNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			houseBillNumberFilter.Property = "AAA";
			houseBillNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain OceanBill1", filterCollection.Contains(OceanBill1));
			Assert("Expect collection to contain OceanBill2", filterCollection.Contains(OceanBill2));
			houseBillNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			houseBillNumberFilter.Property = "ZZZ";
			houseBillNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain OceanBill1", !filterCollection.Contains(OceanBill1));
			Assert("Expect collection not to contain OceanBill2", !filterCollection.Contains(OceanBill2));
		}

		public void TestDepartureDateFilter()
		{
			AssertDateFilter(CusSCAOceanBillSchema.CB_DateOfDeparture, SeaCargoFilterConstants.DateFilterTypes.DepartureDate);
		}

		public void TestArrivalDateFilter()
		{
			AssertDateFilter(CusSCAOceanBillSchema.CB_DateOfArrival, SeaCargoFilterConstants.DateFilterTypes.ArrivalDate);
		}

		public void TestFirstArrivalDateFilter()
		{
			AssertDateFilter(CusSCAOceanBillSchema.CB_DateOfFirstArrival, SeaCargoFilterConstants.DateFilterTypes.FirstArrivalDate);
		}

		public void TestLoadDischargeFilter()
		{
			OceanBill1.CB_RL_NKPortOfLoading = "KRSEL";
			OceanBill2.CB_RL_NKPortOfLoading = "AUBNE";
			OceanBill1.CB_RL_NKPortOfDischarge = "AUSYD";
			OceanBill2.CB_RL_NKPortOfDischarge = "AUSYD";
			Factory.Save();
			ModuleLocationFilter loadDischargeFilter = (ModuleLocationFilter)filterBO[SeaCargoFilterConstants.PortFilterTypes.LoadingDischarge];
			loadDischargeFilter.Property1 = "KR";
			loadDischargeFilter.Property2 = ZString.Empty;
			loadDischargeFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain OceanBill1", filterCollection.Contains(OceanBill1));
			Assert("Expect collection not to contain OceanBill2", !filterCollection.Contains(OceanBill2));
			loadDischargeFilter.Property1 = "AUBNE";
			loadDischargeFilter.Property2 = "AUSYD";
			loadDischargeFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain OceanBill1", !filterCollection.Contains(OceanBill1));
			Assert("Expect collection to contain OceanBill2", filterCollection.Contains(OceanBill2));
			loadDischargeFilter.Property1 = ZString.Empty;
			loadDischargeFilter.Property2 = "AUSYD";
			loadDischargeFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain OceanBill1", filterCollection.Contains(OceanBill1));
			Assert("Expect collection to contain OceanBill2", filterCollection.Contains(OceanBill2));
			loadDischargeFilter.Property1 = "AUSYD";
			loadDischargeFilter.Property2 = ZString.Empty;
			loadDischargeFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain OceanBill1", !filterCollection.Contains(OceanBill1));
			Assert("Expect collection not to contain OceanBill2", !filterCollection.Contains(OceanBill2));
		}

		public void TestOriginDestinationFilter()
		{
			HouseBill1.CA_CB = OceanBill1.PK;
			HouseBill2.CA_CB = OceanBill2.PK;
			HouseBill1.CA_RL_NK_PortOfOrigin = "KRSEL";
			HouseBill2.CA_RL_NK_PortOfOrigin = "AUBNE";
			HouseBill1.CA_RL_NK_PortOfDestination = "AUSYD";
			HouseBill2.CA_RL_NK_PortOfDestination = "AUSYD";
			Factory.Save();
			ModuleLocationFilter originDestinationFilter = (ModuleLocationFilter)filterBO[SeaCargoFilterConstants.PortFilterTypes.OriginDestination];
			originDestinationFilter.Property1 = "KR";
			originDestinationFilter.Property2 = ZString.Empty;
			originDestinationFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain OceanBill1", filterCollection.Contains(OceanBill1));
			Assert("Expect collection not to contain OceanBill2", !filterCollection.Contains(OceanBill2));
			originDestinationFilter.Property1 = "AUBNE";
			originDestinationFilter.Property2 = "AUSYD";
			originDestinationFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain OceanBill1", !filterCollection.Contains(OceanBill1));
			Assert("Expect collection to contain OceanBill2", filterCollection.Contains(OceanBill2));
			originDestinationFilter.Property1 = ZString.Empty;
			originDestinationFilter.Property2 = "AUSYD";
			originDestinationFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain OceanBill1", filterCollection.Contains(OceanBill1));
			Assert("Expect collection to contain OceanBill2", filterCollection.Contains(OceanBill2));
			originDestinationFilter.Property1 = "AUSYD";
			originDestinationFilter.Property2 = ZString.Empty;
			originDestinationFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain OceanBill1", !filterCollection.Contains(OceanBill1));
			Assert("Expect collection not to contain OceanBill2", !filterCollection.Contains(OceanBill2));
		}

		public void TestGetStatusList()
		{
			AssertEquals("List should contain everything", 29, filterBO.GetStatusList(null).Count);
			AssertEquals(10, filterBO.GetStatusList(SeaCargoFilterConstants.StatusFilterTypes.MessageStatus).Count);
			AssertEquals(29, filterBO.GetStatusList(SeaCargoFilterConstants.StatusFilterTypes.OutturnStatus).Count);
			AssertEquals(6, filterBO.GetStatusList(SeaCargoFilterConstants.StatusFilterTypes.UnderbondStatus).Count);
			CodeDescriptionPairList statusList = filterBO.GetStatusList(SeaCargoFilterConstants.StatusFilterTypes.CustomsStatus);
			AssertEquals(14, statusList.Count);
			AssertEquals(true, statusList.ContainsCode(CMRConsolidatedCargoStatuses.Filter.Codes.NotClear));
		}

		public void TestStatusCustomsNotClearFilter()
		{
			var ocean1 = Factory.NewWithValidTestData<CusSCAOceanBill>();
			var ocean2 = Factory.NewWithValidTestData<CusSCAOceanBill>();
			var ocean3 = Factory.NewWithValidTestData<CusSCAOceanBill>();
			var container1 = ocean1.Containers.AddNew();
			container1.FillWithValidTestData();
			var container2 = ocean2.Containers.AddNew();
			container2.FillWithValidTestData();
			var container3 = ocean3.Containers.AddNew();
			container3.FillWithValidTestData();
			var house1 = ocean1.HouseBills.AddNew();
			house1.FillWithValidTestData();
			var house2 = ocean2.HouseBills.AddNew();
			house2.FillWithValidTestData();
			var house3 = ocean3.HouseBills.AddNew();
			house3.FillWithValidTestData();
			house1.Pivot.AddNew().CV_CargoStatus = CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased;
			container1.Pivots.Add(house1.Pivot[0]);
			house2.Pivot.AddNew().CV_CargoStatus = CMRConsolidatedCargoStatuses.Codes.AcsseizedCargoIsSeizedByCustoms;
			container2.Pivots.Add(house2.Pivot[0]);
			house3.Pivot.AddNew().CV_CargoStatus = CMRConsolidatedCargoStatuses.Codes.ClearhrmCargoIsClearButIsIdentifiedAsHighRiskMovement;
			container3.Pivots.Add(house3.Pivot[0]);
			Factory.Save();
			ModuleTextFilter statusFilter = (ModuleTextFilter)filterBO[SeaCargoFilterConstants.StatusFilterTypes.CustomsStatus];
			statusFilter.Property = CMRConsolidatedCargoStatuses.Filter.Codes.NotClear;
			statusFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain ocean1", !filterCollection.Contains(ocean1));
			Assert("Expect collection to contain ocean2", filterCollection.Contains(ocean2));
			Assert("Expect collection not to contain ocean3", !filterCollection.Contains(ocean3));
		}

		public void TestStatusCustomsFilter()
		{
			var ocean1 = Factory.NewWithValidTestData<CusSCAOceanBill>();
			var ocean2 = Factory.NewWithValidTestData<CusSCAOceanBill>();
			var container1 = ocean1.Containers.AddNew();
			container1.FillWithValidTestData();
			var container2 = ocean2.Containers.AddNew();
			container2.FillWithValidTestData();
			var house1 = ocean1.HouseBills.AddNew();
			house1.FillWithValidTestData();
			var house2 = ocean2.HouseBills.AddNew();
			house2.FillWithValidTestData();
			house1.Pivot.AddNew().CV_CargoStatus = CMRConsolidatedCargoStatuses.Codes.AcsseizedCargoIsSeizedByCustoms;
			house2.Pivot.AddNew().CV_CargoStatus = CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased;
			container1.Pivots.Add(house1.Pivot[0]);
			container2.Pivots.Add(house2.Pivot[0]);
			Factory.Save();
			ModuleTextFilter statusFilter = (ModuleTextFilter)filterBO[SeaCargoFilterConstants.StatusFilterTypes.CustomsStatus];
			statusFilter.Property = CMRConsolidatedCargoStatuses.Codes.AcsseizedCargoIsSeizedByCustoms;
			statusFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain ocean1", filterCollection.Contains(ocean1));
			Assert("Expect collection not to contain OceanBill2", !filterCollection.Contains(ocean2));
			statusFilter.Property = CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased;
			statusFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain ocean1", !filterCollection.Contains(ocean1));
			Assert("Expect collection to contain ocean2", filterCollection.Contains(ocean2));
			statusFilter.Property = CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl;
			statusFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain OceanBill1", !filterCollection.Contains(ocean1));
			Assert("Expect collection not to contain OceanBill2", !filterCollection.Contains(ocean2));
		}

		public void TestStatusUnderbondFilter()
		{
			Container1.CN_CB = OceanBill1.PK;
			Container2.CN_CB = OceanBill2.PK;
			Container3.CN_CB = OceanBill3.PK;
			Container4.CN_CB = OceanBill4.PK;
			Pivot1.CV_CN = Container1.PK;
			Pivot2.CV_CN = Container2.PK;
			Underbond1.C4_ParentID = Pivot1.PK;
			Underbond2.C4_ParentID = Pivot2.PK;
			Underbond3.C4_ParentID = Container3.PK;
			Underbond4.C4_ParentID = Container4.PK;
			EntryNumber1.CE_ParentID = Underbond1.PK;
			EntryNumber2.CE_ParentID = Underbond2.PK;
			EntryNumber3.CE_ParentID = Underbond3.PK;
			EntryNumber4.CE_ParentID = Underbond4.PK;
			EntryNumber1.CE_EntryType = CusEntryNumber.EntryType.OutturnStatus;
			EntryNumber2.CE_EntryType = CusEntryNumber.EntryType.UnderbondStatus;
			EntryNumber3.CE_EntryType = CusEntryNumber.EntryType.UnderbondStatus;
			EntryNumber4.CE_EntryType = CusEntryNumber.EntryType.UnderbondStatus;
			EntryNumber1.CE_EntryStatus = CMRUnderbondStatuses.Codes.UnderbondSendingDelayed;
			EntryNumber2.CE_EntryStatus = CMRUnderbondStatuses.Codes.UnderbondSendingDelayed;
			EntryNumber3.CE_EntryStatus = CMRUnderbondStatuses.Codes.UnderbondApprovalAdviceReceived;
			EntryNumber4.CE_EntryStatus = CMRUnderbondStatuses.Codes.UnderbondSendingDelayed;
			Factory.Save();
			ModuleTextFilter statusFilter = (ModuleTextFilter)filterBO[SeaCargoFilterConstants.StatusFilterTypes.UnderbondStatus];
			statusFilter.Property = CMRUnderbondStatuses.Codes.UnderbondSendingDelayed;
			statusFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain OceanBill1", !filterCollection.Contains(OceanBill1));
			Assert("Expect collection to contain OceanBill2", filterCollection.Contains(OceanBill2));
			Assert("Expect collection not to contain OceanBill3", !filterCollection.Contains(OceanBill3));
			Assert("Expect collection to contain OceanBill4", filterCollection.Contains(OceanBill4));
		}

		public void TestStatusMessageFilter()
		{
			HouseBill1.CA_CB = OceanBill1.PK;
			HouseBill2.CA_CB = OceanBill2.PK;
			HouseBill1.CA_MessageStatus = CMRBaseStatuses.Codes.NotSent;
			HouseBill2.CA_MessageStatus = CMRBaseStatuses.Codes.AmendmentAccepted;
			Factory.Save();
			ModuleTextFilter statusFilter = (ModuleTextFilter)filterBO[SeaCargoFilterConstants.StatusFilterTypes.MessageStatus];
			statusFilter.Property = CMRBaseStatuses.Codes.NotSent;
			statusFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain OceanBill1", filterCollection.Contains(OceanBill1));
			Assert("Expect collection not to contain OceanBill2", !filterCollection.Contains(OceanBill2));
			statusFilter.Property = CMRBaseStatuses.Codes.AmendmentAccepted;
			statusFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain OceanBill1", !filterCollection.Contains(OceanBill1));
			Assert("Expect collection to contain OceanBill2", filterCollection.Contains(OceanBill2));
			statusFilter.Property = CMRBaseStatuses.Codes.AwaitingResponseToAmendment;
			statusFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain OceanBill1", !filterCollection.Contains(OceanBill1));
			Assert("Expect collection not to contain OceanBill2", !filterCollection.Contains(OceanBill2));
		}

		public void TestOriginAddressFilter()
		{
			Container1.CN_CB = OceanBill1.PK;
			Container2.CN_CB = OceanBill2.PK;
			Underbond1.C4_ParentID = Container1.PK;
			Underbond2.C4_ParentID = Container2.PK;
			Underbond1.C4_OA_OriginAddress = Address1.PK;
			Underbond2.C4_OA_OriginAddress = Address2.PK;
			Address1.OA_Address1 = "BBBBBBBBBB";
			Address1.OA_Address2 = "CCCCAAACCC";
			Address2.OA_Address1 = "DDAAADDDDD";
			Address2.OA_Address2 = "EEEEEEEEEE";
			Factory.Save();
			ModuleTextFilter establishmentFilter = (ModuleTextFilter)filterBO[SeaCargoFilterConstants.EstablishmentTypes.OriginAddress];
			establishmentFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			establishmentFilter.Property = "BBB";
			establishmentFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain OceanBill1", filterCollection.Contains(OceanBill1));
			Assert("Expect collection not to contain OceanBill2", !filterCollection.Contains(OceanBill2));
			establishmentFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			establishmentFilter.Property = "DDAAADDDDD";
			establishmentFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain OceanBill1", !filterCollection.Contains(OceanBill1));
			Assert("Expect collection to contain OceanBill2", filterCollection.Contains(OceanBill2));
			establishmentFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			establishmentFilter.Property = "AAA";
			establishmentFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain OceanBill1", filterCollection.Contains(OceanBill1));
			Assert("Expect collection to contain OceanBill2", filterCollection.Contains(OceanBill2));
			establishmentFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			establishmentFilter.Property = "ZZZ";
			establishmentFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain OceanBill1", !filterCollection.Contains(OceanBill1));
			Assert("Expect collection not to contain OceanBill2", !filterCollection.Contains(OceanBill2));
		}

		public void TestOriginCodeFilter()
		{
			Container1.CN_CB = OceanBill1.PK;
			Container2.CN_CB = OceanBill2.PK;
			Underbond1.C4_ParentID = Container1.PK;
			Underbond2.C4_ParentID = Container2.PK;
			Underbond1.C4_OriginPremiseID = "XAAAX";
			Underbond2.C4_OriginPremiseID = "YAAAY";
			Factory.Save();
			ModuleTextFilter establishmentFilter = (ModuleTextFilter)filterBO[SeaCargoFilterConstants.EstablishmentTypes.OriginCode];
			establishmentFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			establishmentFilter.Property = "XAA";
			establishmentFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain OceanBill1", filterCollection.Contains(OceanBill1));
			Assert("Expect collection not to contain OceanBill2", !filterCollection.Contains(OceanBill2));
			establishmentFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			establishmentFilter.Property = "YAAAY";
			establishmentFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain OceanBill1", !filterCollection.Contains(OceanBill1));
			Assert("Expect collection to contain OceanBill2", filterCollection.Contains(OceanBill2));
			establishmentFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			establishmentFilter.Property = "AAA";
			establishmentFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain OceanBill1", filterCollection.Contains(OceanBill1));
			Assert("Expect collection to contain OceanBill2", filterCollection.Contains(OceanBill2));
			establishmentFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			establishmentFilter.Property = "ZZZ";
			establishmentFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain OceanBill1", !filterCollection.Contains(OceanBill1));
			Assert("Expect collection not to contain OceanBill2", !filterCollection.Contains(OceanBill2));
		}

		public void TestDestinationAddressFilter()
		{
			Container1.CN_CB = OceanBill1.PK;
			Container2.CN_CB = OceanBill2.PK;
			Underbond1.C4_ParentID = Container1.PK;
			Underbond2.C4_ParentID = Container2.PK;
			Underbond1.C4_OA_DestinationAddress = Address1.PK;
			Underbond2.C4_OA_DestinationAddress = Address2.PK;
			Address1.OA_Address1 = "BBBBBBBBBB";
			Address1.OA_Address2 = "CCCCAAACCC";
			Address2.OA_Address1 = "DDAAADDDDD";
			Address2.OA_Address2 = "EEEEEEEEEE";
			Factory.Save();
			ModuleTextFilter establishmentFilter = (ModuleTextFilter)filterBO[SeaCargoFilterConstants.EstablishmentTypes.DestinationAddress];
			establishmentFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			establishmentFilter.Property = "BBB";
			establishmentFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain OceanBill1", filterCollection.Contains(OceanBill1));
			Assert("Expect collection not to contain OceanBill2", !filterCollection.Contains(OceanBill2));
			establishmentFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			establishmentFilter.Property = "DDAAADDDDD";
			establishmentFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain OceanBill1", !filterCollection.Contains(OceanBill1));
			Assert("Expect collection to contain OceanBill2", filterCollection.Contains(OceanBill2));
			establishmentFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			establishmentFilter.Property = "AAA";
			establishmentFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain OceanBill1", filterCollection.Contains(OceanBill1));
			Assert("Expect collection to contain OceanBill2", filterCollection.Contains(OceanBill2));
			establishmentFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			establishmentFilter.Property = "ZZZ";
			establishmentFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain OceanBill1", !filterCollection.Contains(OceanBill1));
			Assert("Expect collection not to contain OceanBill2", !filterCollection.Contains(OceanBill2));
		}

		public void TestDestinationCodeFilter()
		{
			Container1.CN_CB = OceanBill1.PK;
			Container2.CN_CB = OceanBill2.PK;
			Underbond1.C4_ParentID = Container1.PK;
			Underbond2.C4_ParentID = Container2.PK;
			Underbond1.C4_DestinationPremiseID = "XAAAX";
			Underbond2.C4_DestinationPremiseID = "YAAAY";
			Factory.Save();
			ModuleTextFilter establishmentFilter = (ModuleTextFilter)filterBO[SeaCargoFilterConstants.EstablishmentTypes.DestinationCode];
			establishmentFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			establishmentFilter.Property = "XAA";
			establishmentFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain OceanBill1", filterCollection.Contains(OceanBill1));
			Assert("Expect collection not to contain OceanBill2", !filterCollection.Contains(OceanBill2));
			establishmentFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			establishmentFilter.Property = "YAAAY";
			establishmentFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain OceanBill1", !filterCollection.Contains(OceanBill1));
			Assert("Expect collection to contain OceanBill2", filterCollection.Contains(OceanBill2));
			establishmentFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			establishmentFilter.Property = "AAA";
			establishmentFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain OceanBill1", filterCollection.Contains(OceanBill1));
			Assert("Expect collection to contain OceanBill2", filterCollection.Contains(OceanBill2));
			establishmentFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			establishmentFilter.Property = "ZZZ";
			establishmentFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain OceanBill1", !filterCollection.Contains(OceanBill1));
			Assert("Expect collection not to contain OceanBill2", !filterCollection.Contains(OceanBill2));
		}

		public void TestVesselsProperty()
		{
			AssertNotNull(filterBO.Vessels);
		}

		public void TestVoyageFilter()
		{
			OceanBill1.CB_Voyage = "XAAAX";
			OceanBill2.CB_Voyage = "YAAAY";
			Factory.Save();
			ModuleTextAndNkFilter voyageFilter = (ModuleTextAndNkFilter)filterBO["Vessel / Voyage"];
			voyageFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			voyageFilter.Property = "XAA";
			voyageFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain OceanBill1", filterCollection.Contains(OceanBill1));
			Assert("Expect collection not to contain OceanBill2", !filterCollection.Contains(OceanBill2));
			voyageFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			voyageFilter.Property = "YAAAY";
			voyageFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain OceanBill1", !filterCollection.Contains(OceanBill1));
			Assert("Expect collection to contain OceanBill2", filterCollection.Contains(OceanBill2));
			voyageFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			voyageFilter.Property = "AAA";
			voyageFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain OceanBill1", filterCollection.Contains(OceanBill1));
			Assert("Expect collection to contain OceanBill2", filterCollection.Contains(OceanBill2));
			voyageFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			voyageFilter.Property = "ZZZ";
			voyageFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain OceanBill1", !filterCollection.Contains(OceanBill1));
			Assert("Expect collection not to contain OceanBill2", !filterCollection.Contains(OceanBill2));
		}

		public void TestVesselFilter()
		{
			OceanBill1.CB_VesselName = "XAAAX";
			OceanBill2.CB_VesselName = "YAAAY";
			Factory.Save();
			ModuleTextAndNkFilter vesselFilter = (ModuleTextAndNkFilter)filterBO["Vessel / Voyage"];
			vesselFilter.NkProperty = "XAAAX";
			vesselFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain OceanBill1", filterCollection.Contains(OceanBill1));
			Assert("Expect collection not to contain OceanBill2", !filterCollection.Contains(OceanBill2));
			vesselFilter.NkProperty = "YAA";
			vesselFilter.IsActive = true;
			vesselFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain OceanBill1", !filterCollection.Contains(OceanBill1));
			Assert("Expect collection to contain OceanBill2", filterCollection.Contains(OceanBill2));
			vesselFilter.NkProperty = "ZZZ";
			vesselFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain OceanBill1", !filterCollection.Contains(OceanBill1));
			Assert("Expect collection not to contain OceanBill2", !filterCollection.Contains(OceanBill2));
		}

		[ExpectNoExceptions]
		public void TestVoyageFilterMaxSize()
		{
			var voyageFilter = (ModuleTextAndNkFilter)filterBO["Vessel / Voyage"];
			voyageFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			voyageFilter.Property = "HOEGH AMERICA";
			voyageFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
		}

		[ExpectNoExceptions]
		public void TestHiddenApplicationCodeFilter()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var currentCompanyPK = GlbCompany.CurrentCompany.PK;
				var branch = Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, "SYD");
				if (branch == null)
				{
					branch.GB_Code = "SYD";
					branch.GB_GC = currentCompanyPK;
				}

				using (branch.SetAsTemporaryContext())
				{
					HouseBill1.CA_CB = OceanBill1.PK;
					HouseBill2.CA_CB = OceanBill2.PK;
					HouseBill3.CA_CB = OceanBill3.PK;
					Factory.Save();
					SetBranchAndApplicationCodeForOceanBill(OceanBill1, string.Empty, branch.PK);
					SetBranchAndApplicationCodeForOceanBill(OceanBill2, Core.Constants.Customs.CusSCAOceanBillApplicationCodes.AustraliaCMR, branch.PK);
					SetBranchAndApplicationCodeForOceanBill(OceanBill3, Core.Constants.Customs.CusSCAOceanBillApplicationCodes.CanadaACISea, branch.PK);
					Factory.Save();
					filterCollection.Load(filterBO.Filter);
					filterCollection.Cast<CusSCAOceanBill>().ForEach(x => EnsureNoInvalidCastException(x));
					AssertEquals(1, filterCollection.Count);
					Assert(filterCollection.Contains(OceanBill2));
				}
			}

			void SetBranchAndApplicationCodeForOceanBill(CusSCAOceanBill oceanBill, ZString applicationCode, ZGuid branch)
			{
				oceanBill.CB_ApplicationCode = applicationCode;
				oceanBill.CB_GB = branch;
			}

			void EnsureNoInvalidCastException(CusSCAOceanBill oceanBill)
			{
				var houseBill = Factory.LoadTop1<CusSCAHouse>(new ZQuery(CusSCAHouseSchema.CA_CB, oceanBill.PK));
				_ = houseBill.OceanBill;
			}
		}

		public void TestBranchFilter()
		{
			var anotherCompany = Factory.New<GlbCompany>();
			var anotherBranch = anotherCompany.Branches.AddNew();
			OceanBill1.CB_GB = anotherBranch.PK;
			OceanBill1.CB_VesselName = "XAAAX";
			OceanBill2.CB_VesselName = "YAAAY";
			Factory.Save();
			var vesselFilter = (ModuleTextAndNkFilter)filterBO["Vessel / Voyage"];
			vesselFilter.NkProperty = "AAA";
			vesselFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			vesselFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain OceanBill1", !filterCollection.Contains(OceanBill1));
			Assert("Expect collection to contain OceanBill2", filterCollection.Contains(OceanBill2));
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new SeaCargoFilterStripBusinessObject();

		CusSCAOceanBill oceanBill1;
		CusSCAOceanBill OceanBill1 => oceanBill1 ?? (oceanBill1 = Factory.NewWithValidTestData<CusSCAOceanBill>());

		CusSCAOceanBill oceanBill2;
		CusSCAOceanBill OceanBill2 => oceanBill2 ?? (oceanBill2 = Factory.NewWithValidTestData<CusSCAOceanBill>());

		CusSCAOceanBill oceanBill3;
		CusSCAOceanBill OceanBill3 => oceanBill3 ?? (oceanBill3 = Factory.NewWithValidTestData<CusSCAOceanBill>());

		CusSCAOceanBill oceanBill4;
		CusSCAOceanBill OceanBill4 => oceanBill4 ?? (oceanBill4 = Factory.NewWithValidTestData<CusSCAOceanBill>());

		CusSCAPivot pivot1;
		CusSCAPivot Pivot1 => pivot1 ?? (pivot1 = Factory.NewWithValidTestData<CusSCAPivot>());

		CusSCAPivot pivot2;
		CusSCAPivot Pivot2 => pivot2 ?? (pivot2 = Factory.NewWithValidTestData<CusSCAPivot>());

		CusSCAContainer container1;
		CusSCAContainer Container1 => container1 ?? (container1 = Factory.NewWithValidTestData<CusSCAContainer>());

		CusSCAContainer container2;
		CusSCAContainer Container2 => container2 ?? (container2 = Factory.NewWithValidTestData<CusSCAContainer>());

		CusSCAContainer container3;
		CusSCAContainer Container3 => container3 ?? (container3 = Factory.NewWithValidTestData<CusSCAContainer>());

		CusSCAContainer container4;
		CusSCAContainer Container4 => container4 ?? (container4 = Factory.NewWithValidTestData<CusSCAContainer>());

		CusSCAHouse houseBill1;
		CusSCAHouse HouseBill1 => houseBill1 ?? (houseBill1 = Factory.NewWithValidTestData<CusSCAHouse>());

		CusSCAHouse houseBill2;
		CusSCAHouse HouseBill2 => houseBill2 ?? (houseBill2 = Factory.NewWithValidTestData<CusSCAHouse>());

		CusSCAHouse houseBill3;
		CusSCAHouse HouseBill3 => houseBill3 ?? (houseBill3 = Factory.NewWithValidTestData<CusSCAHouse>());

		CusUnderbond underbond1;
		CusUnderbond Underbond1 => underbond1 ?? (underbond1 = Factory.NewWithValidTestData<CusUnderbond>());

		CusUnderbond underbond2;
		CusUnderbond Underbond2 => underbond2 ?? (underbond2 = Factory.NewWithValidTestData<CusUnderbond>());

		CusUnderbond underbond3;
		CusUnderbond Underbond3 => underbond3 ?? (underbond3 = Factory.NewWithValidTestData<CusUnderbond>());

		CusUnderbond underbond4;
		CusUnderbond Underbond4 => underbond4 ?? (underbond4 = Factory.NewWithValidTestData<CusUnderbond>());

		CusEntryNumber entryNumber1;
		CusEntryNumber EntryNumber1 => entryNumber1 ?? (entryNumber1 = Factory.NewWithValidTestData<CusEntryNumber>());

		CusEntryNumber entryNumber2;
		CusEntryNumber EntryNumber2 => entryNumber2 ?? (entryNumber2 = Factory.NewWithValidTestData<CusEntryNumber>());

		CusEntryNumber entryNumber3;
		CusEntryNumber EntryNumber3 => entryNumber3 ?? (entryNumber3 = Factory.NewWithValidTestData<CusEntryNumber>());

		CusEntryNumber entryNumber4;
		CusEntryNumber EntryNumber4 => entryNumber4 ?? (entryNumber4 = Factory.NewWithValidTestData<CusEntryNumber>());

		OrgAddress address1;
		OrgAddress Address1 => address1 ?? (address1 = Factory.NewWithValidTestData<OrgAddress>());

		OrgAddress address2;
		OrgAddress Address2 => address2 ?? (address2 = Factory.NewWithValidTestData<OrgAddress>());

		SeaCargoFilterStripBusinessObject filterBO;
		CusSCAOceanBillCollection filterCollection;
		protected override void SetUp()
		{
			base.SetUp();
			filterBO = (SeaCargoFilterStripBusinessObject)GetNewFilterStripBusinessObject();
			filterCollection = new CusSCAOceanBillCollection(Factory);
		}

		ProcessTaskTemplate CreateWorkflowTemplate(ZString workflowDescriptorCode)
		{
			var result = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			result.P0_ProcessType = workflowDescriptorCode;
			return result;
		}

		GenCustomColumnDefinition AddCustomField(ProcessTaskTemplate template, ZString name, ZString addOnColumnDataTypeCode)
		{
			var result = template.GenCustomColumnDefinitions.AddNew();
			result.XC_Name = name;
			result.XC_Type = addOnColumnDataTypeCode;
			return result;
		}

		void AssertDateFilter(SchemaDateTimeColumn column, string filterName)
		{
			OceanBill1[column] = new ZDateTime(2000, 1, 1);
			OceanBill2[column] = new ZDateTime(2000, 2, 2);
			Factory.Save();
			var filter = (ModuleDateFilter)filterBO[filterName];
			AssertEquals("The category of filter should be 'Dates'.", FilterCategories.Dates, filter.Category);
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 2, 2);
			filter.Property2 = ZDateTime.Empty;
			filter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain OceanBill1.", !filterCollection.Contains(OceanBill1));
			Assert("Expect collection to contain OceanBill2.", filterCollection.Contains(OceanBill2));
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = new ZDateTime(2000, 1, 1);
			filter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain OceanBill1.", filterCollection.Contains(OceanBill1));
			Assert("Expect collection not to contain OceanBill2.", !filterCollection.Contains(OceanBill2));
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 1);
			filter.Property2 = new ZDateTime(2000, 2, 2);
			filter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain OceanBill1.", filterCollection.Contains(OceanBill1));
			Assert("Expect collection to contain OceanBill2.", filterCollection.Contains(OceanBill2));
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 1);
			filter.Property2 = new ZDateTime(2000, 1, 1);
			filter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain OceanBill1.", filterCollection.Contains(OceanBill1));
			Assert("Expect collection not to contain OceanBill2.", !filterCollection.Contains(OceanBill2));
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 2);
			filter.Property2 = new ZDateTime(2000, 2, 1);
			filter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain OceanBill1.", !filterCollection.Contains(OceanBill1));
			Assert("Expect collection not to contain OceanBill2.", !filterCollection.Contains(OceanBill2));
		}
	}
}
