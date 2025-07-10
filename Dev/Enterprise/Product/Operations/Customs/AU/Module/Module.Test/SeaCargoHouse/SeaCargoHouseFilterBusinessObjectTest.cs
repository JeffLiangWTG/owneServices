using System;
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
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.Testing
{
	[TestedType(typeof(SeaCargoHouseFilterBusinessObject))]
	sealed class SeaCargoHouseFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestContainerNumberFilter()
		{
			var container1 = oceanBill1.Containers.AddNew();
			container1.CN_ContainerNumber = "XAAAX";
			var container2 = oceanBill2.Containers.AddNew();
			container2.CN_ContainerNumber = "YAAAY";
			var pivot1 = houseBill1.Pivot.AddNew();
			pivot1.CV_CN = container1.PK;
			var pivot2 = houseBill2.Pivot.AddNew();
			pivot2.CV_CN = container2.PK;
			Factory.Save();
			var containerNumberFilter = (ModuleNumberFilter)filterBO[SeaCargoFilterConstants.NumberFilterTypes.ContainerNumber];
			containerNumberFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			containerNumberFilter.Property = "XAA";
			containerNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain HouseBill1", filterCollection.Contains(houseBill1));
			Assert("Expect collection not to contain HouseBill2", !filterCollection.Contains(houseBill2));
			containerNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			containerNumberFilter.Property = "YAAAY";
			containerNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain HouseBill1", !filterCollection.Contains(houseBill1));
			Assert("Expect collection to contain HouseBill2", filterCollection.Contains(houseBill2));
			containerNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			containerNumberFilter.Property = "AAA";
			containerNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain HouseBill1", filterCollection.Contains(houseBill1));
			Assert("Expect collection to contain HouseBill2", filterCollection.Contains(houseBill2));
			containerNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			containerNumberFilter.Property = "ZZZ";
			containerNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain HouseBill1", !filterCollection.Contains(houseBill1));
			Assert("Expect collection not to contain HouseBill2", !filterCollection.Contains(houseBill2));
		}

		public void TestHouseBillNumberFilter()
		{
			houseBill1.CA_HouseBill = "XAAAX";
			houseBill2.CA_HouseBill = "YAAAY";
			Factory.Save();
			var houseBillNumberFilter = (ModuleNumberFilter)filterBO[SeaCargoFilterConstants.NumberFilterTypes.HouseBillNumber];
			houseBillNumberFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			houseBillNumberFilter.Property = "XAA";
			houseBillNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain HouseBill1", filterCollection.Contains(houseBill1));
			Assert("Expect collection not to contain HouseBill2", !filterCollection.Contains(houseBill2));
			houseBillNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			houseBillNumberFilter.Property = "YAAAY";
			houseBillNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain HouseBill1", !filterCollection.Contains(houseBill1));
			Assert("Expect collection to contain HouseBill2", filterCollection.Contains(houseBill2));
			houseBillNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			houseBillNumberFilter.Property = "AAA";
			houseBillNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain HouseBill1", filterCollection.Contains(houseBill1));
			Assert("Expect collection to contain HouseBill2", filterCollection.Contains(houseBill2));
			houseBillNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			houseBillNumberFilter.Property = "ZZZ";
			houseBillNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain HouseBill1", !filterCollection.Contains(houseBill1));
			Assert("Expect collection not to contain HouseBill2", !filterCollection.Contains(houseBill2));
		}

		public void TestOceanBillNumberFilter()
		{
			oceanBill1.CB_OceanBill = "OB1";
			houseBill1.CA_HouseBill = "XAAAX";
			houseBill2.CA_HouseBill = "YAAAY";
			oceanBill2.CB_GB = GlbBranch.CurrentBranch.PK;
			oceanBill2.CB_OceanBill = "OB2";
			houseBill3.CA_CB = oceanBill2.PK;
			houseBill3.CA_HouseBill = "ZAAAZ";
			Factory.Save();
			var oceanBillNumberFilter = (ModuleNumberFilter)filterBO[SeaCargoFilterConstants.NumberFilterTypes.OceanBillNumber];
			oceanBillNumberFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			oceanBillNumberFilter.Property = "OB";
			oceanBillNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain HouseBill1", filterCollection.Contains(houseBill1));
			Assert("Expect collection to contain HouseBill2", filterCollection.Contains(houseBill2));
			Assert("Expect collection to contain HouseBill3", filterCollection.Contains(houseBill3));
			oceanBillNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			oceanBillNumberFilter.Property = "OB2";
			oceanBillNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain HouseBill1", !filterCollection.Contains(houseBill1));
			Assert("Expect collection not to contain HouseBill1", !filterCollection.Contains(houseBill2));
			Assert("Expect collection to contain HouseBill3", filterCollection.Contains(houseBill3));
			oceanBillNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			oceanBillNumberFilter.Property = "B1";
			oceanBillNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain HouseBill1", filterCollection.Contains(houseBill1));
			Assert("Expect collection to contain HouseBill2", filterCollection.Contains(houseBill2));
			Assert("Expect collection not to contain HouseBill3", !filterCollection.Contains(houseBill3));
			oceanBillNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			oceanBillNumberFilter.Property = "ZZZ";
			oceanBillNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain HouseBill1", !filterCollection.Contains(houseBill1));
			Assert("Expect collection not to contain HouseBill2", !filterCollection.Contains(houseBill2));
			Assert("Expect collection not to contain HouseBill3", !filterCollection.Contains(houseBill3));
		}

		public void TestParentBillNumberFilter()
		{
			houseBill1.CA_MasterHouseBill = "XAAAX";
			houseBill2.CA_MasterHouseBill = "YAAAY";
			Factory.Save();
			var parentBillNumberFilter = (ModuleNumberFilter)filterBO[SeaCargoFilterConstants.NumberFilterTypes.ParentBillNumber];
			parentBillNumberFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			parentBillNumberFilter.Property = "XAA";
			parentBillNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain HouseBill1", filterCollection.Contains(houseBill1));
			Assert("Expect collection not to contain HouseBill2", !filterCollection.Contains(houseBill2));
			parentBillNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			parentBillNumberFilter.Property = "YAAAY";
			parentBillNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain HouseBill1", !filterCollection.Contains(houseBill1));
			Assert("Expect collection to contain HouseBill2", filterCollection.Contains(houseBill2));
			parentBillNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			parentBillNumberFilter.Property = "AAA";
			parentBillNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain HouseBill1", filterCollection.Contains(houseBill1));
			Assert("Expect collection to contain HouseBill2", filterCollection.Contains(houseBill2));
			parentBillNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			parentBillNumberFilter.Property = "ZZZ";
			parentBillNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain HouseBill1", !filterCollection.Contains(houseBill1));
			Assert("Expect collection not to contain HouseBill2", !filterCollection.Contains(houseBill2));
		}

		public void TestResponsiblePartyIDFilter()
		{
			houseBill1.CA_ResponsiblePartyID = "XAAAX";
			houseBill2.CA_ResponsiblePartyID = "YAAAY";
			Factory.Save();
			var responsiblePartyIdFilter = (ModuleNumberFilter)filterBO[SeaCargoFilterConstants.NumberFilterTypes.ResponsiblePartyID];
			responsiblePartyIdFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			responsiblePartyIdFilter.Property = "XAA";
			responsiblePartyIdFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain HouseBill1", filterCollection.Contains(houseBill1));
			Assert("Expect collection not to contain HouseBill2", !filterCollection.Contains(houseBill2));
			responsiblePartyIdFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			responsiblePartyIdFilter.Property = "YAAAY";
			responsiblePartyIdFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain HouseBill1", !filterCollection.Contains(houseBill1));
			Assert("Expect collection to contain HouseBill2", filterCollection.Contains(houseBill2));
			responsiblePartyIdFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			responsiblePartyIdFilter.Property = "AAA";
			responsiblePartyIdFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain HouseBill1", filterCollection.Contains(houseBill1));
			Assert("Expect collection to contain HouseBill2", filterCollection.Contains(houseBill2));
			responsiblePartyIdFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			responsiblePartyIdFilter.Property = "ZZZ";
			responsiblePartyIdFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain HouseBill1", !filterCollection.Contains(houseBill1));
			Assert("Expect collection not to contain HouseBill2", !filterCollection.Contains(houseBill2));
		}

		public void TestVesselVoyageFilter()
		{
			oceanBill1.CB_OceanBill = "OB1";
			oceanBill1.CB_Voyage = "VOY001";
			houseBill1.CA_HouseBill = "XAAAX";
			houseBill2.CA_HouseBill = "YAAAY";
			oceanBill2.CB_OceanBill = "OB2";
			oceanBill2.CB_Voyage = "VOY002";
			oceanBill2.CB_VesselName = "ALBATROSS";
			houseBill3.CA_CB = oceanBill2.PK;
			houseBill3.CA_HouseBill = "ZAAAZ";
			oceanBill3.CB_OceanBill = "OB3";
			oceanBill3.CB_Voyage = "";
			houseBill4.CA_CB = oceanBill3.PK;
			houseBill4.CA_HouseBill = "EEEE";
			Factory.Save();
			var vesselVoyageNumberFilter = (ModuleTextAndNkFilter)filterBO[SeaCargoFilterConstants.NumberFilterTypes.VesselVoyage];
			vesselVoyageNumberFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			vesselVoyageNumberFilter.Property = "VOY";
			vesselVoyageNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("does contain HouseBill1", filterCollection.Contains(houseBill1));
			Assert("does contain HouseBill2", filterCollection.Contains(houseBill2));
			Assert("does contain HouseBill3", filterCollection.Contains(houseBill3));
			Assert("not contains HouseBill4", !filterCollection.Contains(houseBill4));
			vesselVoyageNumberFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			vesselVoyageNumberFilter.Property = "";
			vesselVoyageNumberFilter.NkProperty = "ALBA";
			vesselVoyageNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("not contains HouseBill1", !filterCollection.Contains(houseBill1));
			Assert("not contains HouseBill2", !filterCollection.Contains(houseBill2));
			Assert("does contain HouseBill3", filterCollection.Contains(houseBill3));
			Assert("not contains HouseBill4", !filterCollection.Contains(houseBill4));
			vesselVoyageNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			vesselVoyageNumberFilter.Property = "VOY002";
			vesselVoyageNumberFilter.NkProperty = "";
			vesselVoyageNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("not contains HouseBill1", !filterCollection.Contains(houseBill1));
			Assert("not contains HouseBill1", !filterCollection.Contains(houseBill2));
			Assert("does contain HouseBill3", filterCollection.Contains(houseBill3));
			Assert("not contains HouseBill4", !filterCollection.Contains(houseBill4));
			vesselVoyageNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			vesselVoyageNumberFilter.Property = "001";
			vesselVoyageNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("does contain HouseBill1", filterCollection.Contains(houseBill1));
			Assert("does contain HouseBill2", filterCollection.Contains(houseBill2));
			Assert("not contains HouseBill3", !filterCollection.Contains(houseBill3));
			Assert("not contains HouseBill4", !filterCollection.Contains(houseBill4));
			vesselVoyageNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			vesselVoyageNumberFilter.Property = "ZZZ";
			vesselVoyageNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("not contains HouseBill1", !filterCollection.Contains(houseBill1));
			Assert("not contains HouseBill2", !filterCollection.Contains(houseBill2));
			Assert("not contains HouseBill3", !filterCollection.Contains(houseBill3));
			Assert("not contains HouseBill4", !filterCollection.Contains(houseBill4));
			vesselVoyageNumberFilter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			vesselVoyageNumberFilter.Property = ZString.Empty;
			vesselVoyageNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("not contains HouseBill1", !filterCollection.Contains(houseBill1));
			Assert("not contains HouseBill2", !filterCollection.Contains(houseBill2));
			Assert("not contains HouseBill3", !filterCollection.Contains(houseBill3));
			Assert("does contain HouseBill4", filterCollection.Contains(houseBill4));
			vesselVoyageNumberFilter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			vesselVoyageNumberFilter.Property = ZString.Empty;
			vesselVoyageNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("not contains HouseBill1", !filterCollection.Contains(houseBill1));
			Assert("not contains HouseBill2", !filterCollection.Contains(houseBill2));
			Assert("does contain HouseBill3", filterCollection.Contains(houseBill3));
			Assert("not contains HouseBill4", !filterCollection.Contains(houseBill4));
		}

		public void TestCustomsCargoStatusFilter()
		{
			houseBill1.CA_ShipmentStatus = CMRConsolidatedCargoStatuses.Codes.AcsseizedCargoIsSeizedByCustoms;
			houseBill2.CA_ShipmentStatus = CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased;
			Factory.Save();
			ModuleTextFilter statusFilter = (ModuleTextFilter)filterBO[SeaCargoFilterConstants.StatusFilterTypes.CustomsStatus];
			statusFilter.Property = CMRConsolidatedCargoStatuses.Codes.AcsseizedCargoIsSeizedByCustoms;
			statusFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain house1", filterCollection.Contains(houseBill1));
			Assert("Expect collection not to contain house2", !filterCollection.Contains(houseBill2));
			statusFilter.Property = CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased;
			statusFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain house1", !filterCollection.Contains(houseBill1));
			Assert("Expect collection to contain house2", filterCollection.Contains(houseBill2));
			statusFilter.Property = CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl;
			statusFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain house1", !filterCollection.Contains(houseBill1));
			Assert("Expect collection not to contain house2", !filterCollection.Contains(houseBill2));
		}

		public void TestCustomsMessageStatusFilter()
		{
			houseBill1.CA_MessageStatus = CMRBaseStatuses.Codes.NotSent;
			houseBill2.CA_MessageStatus = CMRBaseStatuses.Codes.AmendmentAccepted;
			Factory.Save();
			var statusFilter = (ModuleTextFilter)filterBO[SeaCargoFilterConstants.StatusFilterTypes.MessageStatus];
			statusFilter.Property = CMRBaseStatuses.Codes.NotSent;
			statusFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain HouseBill1", filterCollection.Contains(houseBill1));
			Assert("Expect collection not to contain HouseBill2", !filterCollection.Contains(houseBill2));
			statusFilter.Property = CMRBaseStatuses.Codes.AmendmentAccepted;
			statusFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain HouseBill1", !filterCollection.Contains(houseBill1));
			Assert("Expect collection to contain HouseBill2", filterCollection.Contains(houseBill2));
			statusFilter.Property = CMRBaseStatuses.Codes.AwaitingResponseToAmendment;
			statusFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain HouseBill1", !filterCollection.Contains(houseBill1));
			Assert("Expect collection not to contain HouseBill2", !filterCollection.Contains(houseBill2));
		}

		public void TestUnderbondStatusFilter()
		{
			houseBill3.CA_CB = oceanBill3.PK;
			houseBill4.CA_CB = oceanBill4.PK;
			AssociateUnderbondWithHouse(underbond1, houseBill1);
			AssociateUnderbondWithHouse(underbond2, houseBill2);
			AssociateUnderbondWithHouse(underbond3, houseBill3);
			AssociateUnderbondWithHouse(underbond4, houseBill4);
			CreateCusEntryNumberOnUnderbond(underbond1, entryType: CusEntryNumber.EntryType.OutturnStatus, entryStatus: CMRUnderbondStatuses.Codes.UnderbondSendingDelayed);
			CreateCusEntryNumberOnUnderbond(underbond2, entryType: CusEntryNumber.EntryType.UnderbondStatus, entryStatus: CMRUnderbondStatuses.Codes.UnderbondSendingDelayed);
			CreateCusEntryNumberOnUnderbond(underbond3, entryType: CusEntryNumber.EntryType.UnderbondStatus, entryStatus: CMRUnderbondStatuses.Codes.UnderbondApprovalAdviceReceived);
			CreateCusEntryNumberOnUnderbond(underbond4, entryType: CusEntryNumber.EntryType.UnderbondStatus, entryStatus: CMRUnderbondStatuses.Codes.UnderbondSendingDelayed);
			Factory.Save();
			ModuleTextFilter statusFilter = (ModuleTextFilter)filterBO[SeaCargoFilterConstants.StatusFilterTypes.UnderbondStatus];
			statusFilter.Property = CMRUnderbondStatuses.Codes.UnderbondSendingDelayed;
			statusFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain HouseBill1", !filterCollection.Contains(houseBill1));
			Assert("Expect collection to contain HouseBill2", filterCollection.Contains(houseBill2));
			Assert("Expect collection not to contain OceanBill3", !filterCollection.Contains(houseBill3));
			Assert("Expect collection to contain OceanBill4", filterCollection.Contains(houseBill4));
		}

		public void TestArrivalDateFilter()
		{
			AssertDateFilter(CusSCAOceanBillSchema.CB_DateOfArrival, SeaCargoFilterConstants.DateFilterTypes.ArrivalDate);
		}

		public void TestDepartureDateFilter()
		{
			AssertDateFilter(CusSCAOceanBillSchema.CB_DateOfDeparture, SeaCargoFilterConstants.DateFilterTypes.DepartureDate);
		}

		public void TestFirstArrivalDateFilter()
		{
			AssertDateFilter(CusSCAOceanBillSchema.CB_DateOfFirstArrival, SeaCargoFilterConstants.DateFilterTypes.FirstArrivalDate);
		}

		public void TestLoadDischargeFilter()
		{
			oceanBill1.CB_RL_NKPortOfLoading = "KRSEL";
			oceanBill2.CB_RL_NKPortOfLoading = "AUBNE";
			oceanBill1.CB_RL_NKPortOfDischarge = "AUSYD";
			oceanBill2.CB_RL_NKPortOfDischarge = "AUSYD";
			houseBill1.CA_CB = oceanBill1.PK;
			houseBill2.CA_CB = oceanBill2.PK;
			Factory.Save();
			var loadDischargeFilter = (ModuleLocationFilter)filterBO[SeaCargoFilterConstants.PortFilterTypes.LoadingDischarge];
			loadDischargeFilter.Property1 = "KR";
			loadDischargeFilter.Property2 = ZString.Empty;
			loadDischargeFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain HouseBill1", filterCollection.Contains(houseBill1));
			Assert("Expect collection not to contain HouseBill2", !filterCollection.Contains(houseBill2));
			loadDischargeFilter.Property1 = "AUBNE";
			loadDischargeFilter.Property2 = "AUSYD";
			loadDischargeFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain HouseBill1", !filterCollection.Contains(houseBill1));
			Assert("Expect collection to contain HouseBill2", filterCollection.Contains(houseBill2));
			loadDischargeFilter.Property1 = ZString.Empty;
			loadDischargeFilter.Property2 = "AUSYD";
			loadDischargeFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain HouseBill1", filterCollection.Contains(houseBill1));
			Assert("Expect collection to contain HouseBill2", filterCollection.Contains(houseBill2));
			loadDischargeFilter.Property1 = "AUSYD";
			loadDischargeFilter.Property2 = ZString.Empty;
			loadDischargeFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain HouseBill1", !filterCollection.Contains(houseBill1));
			Assert("Expect collection not to contain HouseBill2", !filterCollection.Contains(houseBill2));
		}

		public void TestOriginDestinationFilter()
		{
			houseBill1.CA_RL_NK_PortOfOrigin = "KRSEL";
			houseBill2.CA_RL_NK_PortOfOrigin = "AUBNE";
			houseBill1.CA_RL_NK_PortOfDestination = "AUSYD";
			houseBill2.CA_RL_NK_PortOfDestination = "AUSYD";
			Factory.Save();
			var originDestinationFilter = (ModuleLocationFilter)filterBO[SeaCargoFilterConstants.PortFilterTypes.OriginDestination];
			originDestinationFilter.Property1 = "KR";
			originDestinationFilter.Property2 = ZString.Empty;
			originDestinationFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain HouseBill1", filterCollection.Contains(houseBill1));
			Assert("Expect collection not to contain HouseBill2", !filterCollection.Contains(houseBill2));
			originDestinationFilter.Property1 = "AUBNE";
			originDestinationFilter.Property2 = "AUSYD";
			originDestinationFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain HouseBill1", !filterCollection.Contains(houseBill1));
			Assert("Expect collection to contain HouseBill2", filterCollection.Contains(houseBill2));
			originDestinationFilter.Property1 = ZString.Empty;
			originDestinationFilter.Property2 = "AUSYD";
			originDestinationFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain HouseBill1", filterCollection.Contains(houseBill1));
			Assert("Expect collection to contain HouseBill2", filterCollection.Contains(houseBill2));
			originDestinationFilter.Property1 = "AUSYD";
			originDestinationFilter.Property2 = ZString.Empty;
			originDestinationFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain HouseBill1", !filterCollection.Contains(houseBill1));
			Assert("Expect collection not to contain HouseBill2", !filterCollection.Contains(houseBill2));
		}

		public void TestDestinationAddressFilter()
		{
			AssertEstablishmentAddressFilter(SeaCargoFilterConstants.EstablishmentTypes.DestinationAddress, (underbond, addressPK) => underbond.C4_OA_DestinationAddress = addressPK);
		}

		public void TestOriginAddressFilter()
		{
			AssertEstablishmentAddressFilter(SeaCargoFilterConstants.EstablishmentTypes.OriginAddress, (underbond, addressPK) => underbond.C4_OA_OriginAddress = addressPK);
		}

		public void TestCustomFieldFilter()
		{
			var filterCollection = GetNewFilterStripBusinessObject().ModuleFilters;
			AssertNull(filterCollection["stringField"]);
			AssertNull(filterCollection["intField"]);
			AssertNull(filterCollection["dateTimeField"]);
			AssertNull(filterCollection["boolField"]);
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.CusSCAHouseWorkflowDescriptorCode;
			template.P0_IsActive = true;
			AddCustomField(template, "stringField", AddOnColumnDataType.Codes.String);
			AddCustomField(template, "intField", AddOnColumnDataType.Codes.Integer);
			AddCustomField(template, "dateTimeField", AddOnColumnDataType.Codes.Datetime);
			AddCustomField(template, "boolField", AddOnColumnDataType.Codes.Boolean);
			Factory.Save();
			WorkflowCustomFieldsFilter.ClearCache();
			filterCollection = GetNewFilterStripBusinessObject().ModuleFilters;
			AssertEquals(typeof(ModuleTextFilter), filterCollection["stringField"].GetType());
			AssertEquals(typeof(ModuleNumberRangeFilter), filterCollection["intField"].GetType());
			AssertEquals(typeof(ModuleDateFilter), filterCollection["dateTimeField"].GetType());
			AssertEquals(typeof(ModuleFlagsFilter), filterCollection["Workflow Flags"].GetType());
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new SeaCargoHouseFilterBusinessObject();

		CusSCAOceanBill oceanBill1;
		CusSCAOceanBill oceanBill2;
		CusSCAOceanBill oceanBill3;
		CusSCAOceanBill oceanBill4;
		CusSCAHouse houseBill1;
		CusSCAHouse houseBill2;
		CusSCAHouse houseBill3;
		CusSCAHouse houseBill4;
		CusUnderbond underbond1;
		CusUnderbond underbond2;
		CusUnderbond underbond3;
		CusUnderbond underbond4;
		SeaCargoHouseFilterBusinessObject filterBO;
		CusSCAHouseCollectionNonDependent filterCollection;
		protected override void SetUp()
		{
			base.SetUp();
			oceanBill1 = Factory.NewWithValidTestData<CusSCAOceanBill>();
			oceanBill2 = Factory.NewWithValidTestData<CusSCAOceanBill>();
			oceanBill3 = Factory.NewWithValidTestData<CusSCAOceanBill>();
			oceanBill4 = Factory.NewWithValidTestData<CusSCAOceanBill>();
			houseBill1 = Factory.NewWithValidTestData<CusSCAHouse>();
			houseBill2 = Factory.NewWithValidTestData<CusSCAHouse>();
			houseBill3 = Factory.NewWithValidTestData<CusSCAHouse>();
			houseBill4 = Factory.NewWithValidTestData<CusSCAHouse>();
			underbond1 = Factory.NewWithValidTestData<CusUnderbond>();
			underbond2 = Factory.NewWithValidTestData<CusUnderbond>();
			underbond3 = Factory.NewWithValidTestData<CusUnderbond>();
			underbond4 = Factory.NewWithValidTestData<CusUnderbond>();
			filterBO = (SeaCargoHouseFilterBusinessObject)GetNewFilterStripBusinessObject();
			filterCollection = new CusSCAHouseCollectionNonDependent(Factory);
			oceanBill1.CB_GB = GlbBranch.CurrentBranch.PK;
			houseBill1.CA_CB = oceanBill1.PK;
			houseBill2.CA_CB = oceanBill1.PK;
		}

		void CreateCusEntryNumberOnUnderbond(CusUnderbond underbond, string entryType, string entryStatus)
		{
			var entryNumber = Factory.NewWithValidTestData<CusEntryNumber>();
			entryNumber.CE_ParentID = underbond.PK;
			entryNumber.CE_EntryType = entryType;
			entryNumber.CE_EntryStatus = entryStatus;
		}

		void AssertDateFilter(SchemaDateTimeColumn column, string filterName)
		{
			oceanBill1[column] = new ZDateTime(2000, 1, 1);
			oceanBill2[column] = new ZDateTime(2000, 2, 2);
			houseBill1.CA_CB = oceanBill1.PK;
			houseBill2.CA_CB = oceanBill2.PK;
			Factory.Save();
			var filter = (ModuleDateFilter)filterBO[filterName];
			AssertEquals("The category of filter should be 'Dates'.", FilterCategories.Dates, filter.Category);
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 2, 2);
			filter.Property2 = ZDateTime.Empty;
			filter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain HouseBill1.", !filterCollection.Contains(houseBill1));
			Assert("Expect collection to contain HouseBill2.", filterCollection.Contains(houseBill2));
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = new ZDateTime(2000, 1, 1);
			filter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain HouseBill1.", filterCollection.Contains(houseBill1));
			Assert("Expect collection not to contain HouseBill2.", !filterCollection.Contains(houseBill2));
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 1);
			filter.Property2 = new ZDateTime(2000, 2, 2);
			filter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain HouseBill1.", filterCollection.Contains(houseBill1));
			Assert("Expect collection to contain HouseBill2.", filterCollection.Contains(houseBill2));
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 1);
			filter.Property2 = new ZDateTime(2000, 1, 1);
			filter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain HouseBill1.", filterCollection.Contains(houseBill1));
			Assert("Expect collection not to contain HouseBill2.", !filterCollection.Contains(houseBill2));
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 2);
			filter.Property2 = new ZDateTime(2000, 2, 1);
			filter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain HouseBill1.", !filterCollection.Contains(houseBill1));
			Assert("Expect collection not to contain HouseBill2.", !filterCollection.Contains(houseBill2));
		}

		void AssertEstablishmentAddressFilter(string establishmentType, Action<CusUnderbond, ZGuid> setUnderbondAddressFunc)
		{
			houseBill1.CA_CB = oceanBill1.PK;
			houseBill2.CA_CB = oceanBill2.PK;
			houseBill3.CA_CB = oceanBill3.PK;
			houseBill4.CA_CB = oceanBill4.PK;
			AssociateUnderbondWithHouse(underbond1, houseBill1);
			AssociateUnderbondWithHouse(underbond2, houseBill2);
			AssociateUnderbondWithHouse(underbond3, houseBill3);
			AssociateUnderbondWithHouse(underbond4, houseBill4);
			var address1 = CreateOrgAddress("BBBBBBBBBB", "CCCCAAACCC");
			var address2 = CreateOrgAddress("DDAAADDDDD", "EEEEEEEEEE");
			var address3 = CreateOrgAddress("FFFGGGHHHH", "");
			setUnderbondAddressFunc(underbond1, address1.PK);
			setUnderbondAddressFunc(underbond2, address2.PK);
			setUnderbondAddressFunc(underbond3, address3.PK);
			setUnderbondAddressFunc(underbond4, ZGuid.Empty);
			Factory.Save();
			var establishmentFilter = (ModuleTextFilter)filterBO[establishmentType];
			establishmentFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			establishmentFilter.Property = "BBB";
			establishmentFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain HouseBill1", filterCollection.Contains(houseBill1));
			Assert("Expect collection not to contain HouseBill2", !filterCollection.Contains(houseBill2));
			establishmentFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			establishmentFilter.Property = "DDAAADDDDD";
			establishmentFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain HouseBill1", !filterCollection.Contains(houseBill1));
			Assert("Expect collection to contain HouseBill2", filterCollection.Contains(houseBill2));
			establishmentFilter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			establishmentFilter.Property = "DDAAADDDDD";
			establishmentFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain HouseBill1", filterCollection.Contains(houseBill1));
			Assert("Expect collection not to contain HouseBill2", !filterCollection.Contains(houseBill2));
			establishmentFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			establishmentFilter.Property = "AAA";
			establishmentFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain HouseBill1", filterCollection.Contains(houseBill1));
			Assert("Expect collection to contain HouseBill2", filterCollection.Contains(houseBill2));
			establishmentFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			establishmentFilter.Property = "ZZZ";
			establishmentFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain HouseBill1", !filterCollection.Contains(houseBill1));
			Assert("Expect collection not to contain HouseBill2", !filterCollection.Contains(houseBill2));
			establishmentFilter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			establishmentFilter.Property = "";
			establishmentFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain HouseBill1", !filterCollection.Contains(houseBill1));
			Assert("Expect collection not to contain HouseBill2", !filterCollection.Contains(houseBill2));
			Assert("Expect collection not to contain HouseBill3", !filterCollection.Contains(houseBill3));
			Assert("Expect collection to contain HouseBill4", filterCollection.Contains(houseBill4));
		}

		GenCustomColumnDefinition AddCustomField(ProcessTaskTemplate template, ZString name, ZString addOnColumnDataTypeCode)
		{
			var result = template.GenCustomColumnDefinitions.AddNew();
			result.XC_Name = name;
			result.XC_Type = addOnColumnDataTypeCode;
			return result;
		}

		void AssociateUnderbondWithHouse(CusUnderbond underbond, CusSCAHouse house)
		{
			var container = house.OceanBill.Containers.AddNew();
			var pivot = house.Pivot.AddNew();
			pivot.CV_CN = container.PK;
			underbond.C4_ParentID = pivot.PK;
		}

		OrgAddress CreateOrgAddress(string address1, string address2)
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_Address1 = address1;
			address.OA_Address2 = address2;
			return address;
		}
	}
}
