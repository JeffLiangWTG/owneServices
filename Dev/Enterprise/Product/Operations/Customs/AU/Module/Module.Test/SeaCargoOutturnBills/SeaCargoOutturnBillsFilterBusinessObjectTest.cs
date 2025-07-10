using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Freight.CFS.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.Testing
{
	[TestedType(typeof(SeaCargoOutturnBillsFilterBusinessObject))]
	sealed class SeaCargoOutturnBillsFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestLloydsNumberFilter()
		{
			outturn1.C5_C6 = header1.PK;
			outturn2.C5_C6 = header2.PK;
			header1.C6_LloydsIMO = "XAAAX";
			header2.C6_LloydsIMO = "YAAAY";
			Factory.Save();
			ModuleNumberFilter lloydsNumberFilter = (ModuleNumberFilter)filterBO[SeaCargoDepotFilterConstants.NumberFilterTypes.LloydsNumber];
			lloydsNumberFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			lloydsNumberFilter.Property = "XAA";
			lloydsNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain Outturn1", filterCollection.Contains(outturn1));
			Assert("Expect collection not to contain Outturn2", !filterCollection.Contains(outturn2));
			lloydsNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			lloydsNumberFilter.Property = "YAAAY";
			lloydsNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain Outturn1", !filterCollection.Contains(outturn1));
			Assert("Expect collection to contain Outturn2", filterCollection.Contains(outturn2));
			lloydsNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			lloydsNumberFilter.Property = "AAA";
			lloydsNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain Outturn1", filterCollection.Contains(outturn1));
			Assert("Expect collection to contain Outturn2", filterCollection.Contains(outturn2));
			lloydsNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			lloydsNumberFilter.Property = "ZZZ";
			lloydsNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain Outturn1", !filterCollection.Contains(outturn1));
			Assert("Expect collection not to contain Outturn2", !filterCollection.Contains(outturn2));
		}

		public void TestMasterBillNumberFilter()
		{
			outturn1.C5_MasterBill = "XAAAX";
			outturn2.C5_MasterBill = "YAAAY";
			Factory.Save();
			ModuleNumberFilter masterBillNumberFilter = (ModuleNumberFilter)filterBO[SeaCargoDepotFilterConstants.NumberFilterTypes.MasterBillNumber];
			masterBillNumberFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			masterBillNumberFilter.Property = "XAA";
			masterBillNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain Outturn1", filterCollection.Contains(outturn1));
			Assert("Expect collection not to contain Outturn2", !filterCollection.Contains(outturn2));
			masterBillNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			masterBillNumberFilter.Property = "YAAAY";
			masterBillNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain Outturn1", !filterCollection.Contains(outturn1));
			Assert("Expect collection to contain Outturn2", filterCollection.Contains(outturn2));
			masterBillNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			masterBillNumberFilter.Property = "AAA";
			masterBillNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain Outturn1", filterCollection.Contains(outturn1));
			Assert("Expect collection to contain Outturn2", filterCollection.Contains(outturn2));
			masterBillNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			masterBillNumberFilter.Property = "ZZZ";
			masterBillNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain Outturn1", !filterCollection.Contains(outturn1));
			Assert("Expect collection not to contain Outturn2", !filterCollection.Contains(outturn2));
		}

		public void TestHouseBillNumberFilter()
		{
			outturn1.C5_HouseBill = "XAAAX";
			outturn2.C5_HouseBill = "YAAAY";
			Factory.Save();
			ModuleNumberFilter houseBillNumberFilter = (ModuleNumberFilter)filterBO[SeaCargoDepotFilterConstants.NumberFilterTypes.HouseBillNumber];
			houseBillNumberFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			houseBillNumberFilter.Property = "XAA";
			houseBillNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain Outturn1", filterCollection.Contains(outturn1));
			Assert("Expect collection not to contain Outturn2", !filterCollection.Contains(outturn2));
			houseBillNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			houseBillNumberFilter.Property = "YAAAY";
			houseBillNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain Outturn1", !filterCollection.Contains(outturn1));
			Assert("Expect collection to contain Outturn2", filterCollection.Contains(outturn2));
			houseBillNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			houseBillNumberFilter.Property = "AAA";
			houseBillNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain Outturn1", filterCollection.Contains(outturn1));
			Assert("Expect collection to contain Outturn2", filterCollection.Contains(outturn2));
			houseBillNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			houseBillNumberFilter.Property = "ZZZ";
			houseBillNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain Outturn1", !filterCollection.Contains(outturn1));
			Assert("Expect collection not to contain Outturn2", !filterCollection.Contains(outturn2));
		}

		public void TestContainerNumberFilter()
		{
			outturn1.C5_ContainerNumber = "XAAAX";
			outturn2.C5_ContainerNumber = "YAAAY";
			Factory.Save();
			ModuleNumberFilter containerNumberFilter = (ModuleNumberFilter)filterBO[SeaCargoDepotFilterConstants.NumberFilterTypes.ContainerNumber];
			containerNumberFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			containerNumberFilter.Property = "XAA";
			containerNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain Outturn1", filterCollection.Contains(outturn1));
			Assert("Expect collection not to contain Outturn2", !filterCollection.Contains(outturn2));
			containerNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			containerNumberFilter.Property = "YAAAY";
			containerNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain Outturn1", !filterCollection.Contains(outturn1));
			Assert("Expect collection to contain Outturn2", filterCollection.Contains(outturn2));
			containerNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			containerNumberFilter.Property = "AAA";
			containerNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain Outturn1", filterCollection.Contains(outturn1));
			Assert("Expect collection to contain Outturn2", filterCollection.Contains(outturn2));
			containerNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			containerNumberFilter.Property = "ZZZ";
			containerNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain Outturn1", !filterCollection.Contains(outturn1));
			Assert("Expect collection not to contain Outturn2", !filterCollection.Contains(outturn2));
		}

		public void TestPremiseIDFilter()
		{
			outturn1.C5_C6 = header1.PK;
			outturn2.C5_C6 = header2.PK;
			header1.C6_OutturningPremiseID = "XAAAX";
			header2.C6_OutturningPremiseID = "YAAAY";
			Factory.Save();
			ModuleNumberFilter promiseIdFilter = (ModuleNumberFilter)filterBO[SeaCargoDepotFilterConstants.NumberFilterTypes.PremiseID];
			promiseIdFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			promiseIdFilter.Property = "XAA";
			promiseIdFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain Outturn1", filterCollection.Contains(outturn1));
			Assert("Expect collection not to contain Outturn2", !filterCollection.Contains(outturn2));
			promiseIdFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			promiseIdFilter.Property = "YAAAY";
			promiseIdFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain Outturn1", !filterCollection.Contains(outturn1));
			Assert("Expect collection to contain Outturn2", filterCollection.Contains(outturn2));
			promiseIdFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			promiseIdFilter.Property = "AAA";
			promiseIdFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain Outturn1", filterCollection.Contains(outturn1));
			Assert("Expect collection to contain Outturn2", filterCollection.Contains(outturn2));
			promiseIdFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			promiseIdFilter.Property = "ZZZ";
			promiseIdFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain Outturn1", !filterCollection.Contains(outturn1));
			Assert("Expect collection not to contain Outturn2", !filterCollection.Contains(outturn2));
		}

		public void TestShipmentOrContainerJobIDFilter()
		{
			outturn1.C5_ParentID = shipment1.PK;
			outturn2.C5_ParentID = shipment2.PK;
			outturn3.C5_ParentID = shipment3.PK;
			outturn4.C5_ParentID = container1.PK;
			outturn5.C5_ParentID = container2.PK;
			shipment1.JS_IsCancelled = ZBool.False;
			shipment2.JS_IsCancelled = ZBool.False;
			shipment3.JS_IsCancelled = ZBool.True;
			shipment1.JS_UniqueConsignRef = "AXXXA";
			shipment2.JS_UniqueConsignRef = "BXXXB";
			shipment3.JS_UniqueConsignRef = "CXXXC";
			container1.JC_ContainerJobID = "DXXXD";
			container2.JC_ContainerJobID = "DYYYD";
			Factory.Save();
			ModuleNumberFilter jobIdFilter = (ModuleNumberFilter)filterBO[SeaCargoDepotFilterConstants.NumberFilterTypes.ShipmentOrContainerJobID];
			jobIdFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			jobIdFilter.Property = "AXX";
			jobIdFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain Outturn1", filterCollection.Contains(outturn1));
			Assert("Expect collection not to contain Outturn2", !filterCollection.Contains(outturn2));
			Assert("Expect collection not to contain Outturn3", !filterCollection.Contains(outturn3));
			Assert("Expect collection not to contain Outturn4", !filterCollection.Contains(outturn4));
			Assert("Expect collection not to contain Outturn5", !filterCollection.Contains(outturn5));
			jobIdFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			jobIdFilter.Property = "BXXXB";
			jobIdFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain Outturn1", !filterCollection.Contains(outturn1));
			Assert("Expect collection to contain Outturn2", filterCollection.Contains(outturn2));
			Assert("Expect collection not to contain Outturn3", !filterCollection.Contains(outturn3));
			Assert("Expect collection not to contain Outturn4", !filterCollection.Contains(outturn4));
			Assert("Expect collection not to contain Outturn5", !filterCollection.Contains(outturn5));
			jobIdFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			jobIdFilter.Property = "XXX";
			jobIdFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain Outturn1", filterCollection.Contains(outturn1));
			Assert("Expect collection to contain Outturn2", filterCollection.Contains(outturn2));
			Assert("Expect collection not to contain Outturn3", !filterCollection.Contains(outturn3));
			Assert("Expect collection to contain Outturn4", filterCollection.Contains(outturn4));
			Assert("Expect collection not to contain Outturn5", !filterCollection.Contains(outturn5));
			jobIdFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			jobIdFilter.Property = "DXX";
			jobIdFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain Outturn1", !filterCollection.Contains(outturn1));
			Assert("Expect collection not to contain Outturn2", !filterCollection.Contains(outturn2));
			Assert("Expect collection not to contain Outturn3", !filterCollection.Contains(outturn3));
			Assert("Expect collection to contain Outturn4", filterCollection.Contains(outturn4));
			Assert("Expect collection not to contain Outturn5", !filterCollection.Contains(outturn5));
			jobIdFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			jobIdFilter.Property = "DYYYD";
			jobIdFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain Outturn1", !filterCollection.Contains(outturn1));
			Assert("Expect collection not to contain Outturn2", !filterCollection.Contains(outturn2));
			Assert("Expect collection not to contain Outturn3", !filterCollection.Contains(outturn3));
			Assert("Expect collection not to contain Outturn4", !filterCollection.Contains(outturn4));
			Assert("Expect collection to contain Outturn5", filterCollection.Contains(outturn5));
			jobIdFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			jobIdFilter.Property = "D";
			jobIdFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain Outturn1", !filterCollection.Contains(outturn1));
			Assert("Expect collection not to contain Outturn2", !filterCollection.Contains(outturn2));
			Assert("Expect collection not to contain Outturn3", !filterCollection.Contains(outturn3));
			Assert("Expect collection to contain Outturn4", filterCollection.Contains(outturn4));
			Assert("Expect collection to contain Outturn5", filterCollection.Contains(outturn5));
			jobIdFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			jobIdFilter.Property = "ZZZ";
			jobIdFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain Outturn1", !filterCollection.Contains(outturn1));
			Assert("Expect collection not to contain Outturn2", !filterCollection.Contains(outturn2));
			Assert("Expect collection not to contain Outturn3", !filterCollection.Contains(outturn3));
			Assert("Expect collection not to contain Outturn4", !filterCollection.Contains(outturn4));
			Assert("Expect collection not to contain Outturn5", !filterCollection.Contains(outturn5));
		}

		public void TestUnpackDateFilter()
		{
			outturn1.C5_CargoUnpackDate = new ZDateTime(2000, 1, 1, 11, 0, 0); // 1 Jan 11:00
			outturn2.C5_CargoUnpackDate = new ZDateTime(2000, 2, 2, 22, 0, 0); // 2 Feb 22:00
			Factory.Save();
			ModuleDateFilter unpackDateFilter = (ModuleDateFilter)filterBO[SeaCargoDepotFilterConstants.DateFilterTypes.UnpackDate];
			unpackDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			unpackDateFilter.Property1 = new ZDateTime(2000, 2, 2);
			unpackDateFilter.Property2 = ZDateTime.Empty;
			unpackDateFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain Outturn1.", !filterCollection.Contains(outturn1));
			Assert("Expect collection to contain Outturn2.", filterCollection.Contains(outturn2));
			unpackDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			unpackDateFilter.Property1 = ZDateTime.Empty;
			unpackDateFilter.Property2 = new ZDateTime(2000, 1, 1);
			unpackDateFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain Outturn1.", filterCollection.Contains(outturn1));
			Assert("Expect collection not to contain Outturn2.", !filterCollection.Contains(outturn2));
			unpackDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			unpackDateFilter.Property1 = new ZDateTime(2000, 1, 1);
			unpackDateFilter.Property2 = new ZDateTime(2000, 2, 2);
			unpackDateFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain Outturn1.", filterCollection.Contains(outturn1));
			Assert("Expect collection to contain Outturn2.", filterCollection.Contains(outturn2));
			unpackDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			unpackDateFilter.Property1 = new ZDateTime(2000, 1, 1);
			unpackDateFilter.Property2 = new ZDateTime(2000, 1, 1);
			unpackDateFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain Outturn1.", filterCollection.Contains(outturn1));
			Assert("Expect collection not to contain Outturn2.", !filterCollection.Contains(outturn2));
			unpackDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			unpackDateFilter.Property1 = new ZDateTime(2000, 1, 2);
			unpackDateFilter.Property2 = new ZDateTime(2000, 2, 1);
			unpackDateFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain Outturn1.", !filterCollection.Contains(outturn1));
			Assert("Expect collection not to contain Outturn2.", !filterCollection.Contains(outturn2));
		}

		public void TestReceiptDateFilter()
		{
			outturn1.C5_CargoReceiptDate = new ZDateTime(2000, 1, 1, 11, 0, 0); // 1 Jan 11:00
			outturn2.C5_CargoReceiptDate = new ZDateTime(2000, 2, 2, 22, 0, 0); // 2 Feb 22:00
			Factory.Save();
			ModuleDateFilter receiptDateFilter = (ModuleDateFilter)filterBO[SeaCargoDepotFilterConstants.DateFilterTypes.CargoReceiptDate];
			receiptDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			receiptDateFilter.Property1 = new ZDateTime(2000, 2, 2);
			receiptDateFilter.Property2 = ZDateTime.Empty;
			receiptDateFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain Outturn1.", !filterCollection.Contains(outturn1));
			Assert("Expect collection to contain Outturn2.", filterCollection.Contains(outturn2));
			receiptDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			receiptDateFilter.Property1 = ZDateTime.Empty;
			receiptDateFilter.Property2 = new ZDateTime(2000, 1, 1);
			receiptDateFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain Outturn1.", filterCollection.Contains(outturn1));
			Assert("Expect collection not to contain Outturn2.", !filterCollection.Contains(outturn2));
			receiptDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			receiptDateFilter.Property1 = new ZDateTime(2000, 1, 1);
			receiptDateFilter.Property2 = new ZDateTime(2000, 2, 2);
			receiptDateFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain Outturn1.", filterCollection.Contains(outturn1));
			Assert("Expect collection to contain Outturn2.", filterCollection.Contains(outturn2));
			receiptDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			receiptDateFilter.Property1 = new ZDateTime(2000, 1, 1);
			receiptDateFilter.Property2 = new ZDateTime(2000, 1, 1);
			receiptDateFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain Outturn1.", filterCollection.Contains(outturn1));
			Assert("Expect collection not to contain Outturn2.", !filterCollection.Contains(outturn2));
			receiptDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			receiptDateFilter.Property1 = new ZDateTime(2000, 1, 2);
			receiptDateFilter.Property2 = new ZDateTime(2000, 2, 1);
			receiptDateFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain Outturn1.", !filterCollection.Contains(outturn1));
			Assert("Expect collection not to contain Outturn2.", !filterCollection.Contains(outturn2));
		}

		public void TestGetStatusList()
		{
			AssertEquals("List should not contain anything", 0, filterBO.GetStatusList(null).Count);
			AssertEquals(13, filterBO.GetStatusList(FilterConstants.StatusTypes.Cargo).Count);
			AssertEquals(1, filterBO.GetStatusList(FilterConstants.StatusTypes.Commercial).Count);
			AssertEquals(6, filterBO.GetStatusList(FilterConstants.StatusTypes.Underbond).Count);
		}

		public void TestCargoStatusFilter()
		{
			outturn1.C5_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.AcsseizedCargoIsSeizedByCustoms;
			outturn2.C5_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased;
			outturn3.C5_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.AcsseizedCargoIsSeizedByCustoms;
			Factory.Save();
			ModuleTextFilter statusFilter = (ModuleTextFilter)filterBO[FilterConstants.StatusTypes.Cargo];
			statusFilter.Property = CMRConsolidatedCargoStatuses.Codes.AcsseizedCargoIsSeizedByCustoms;
			statusFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain Outturn1", filterCollection.Contains(outturn1));
			Assert("Expect collection not to contain Outturn2", !filterCollection.Contains(outturn2));
			Assert("Expect collection to contain Outturn3", filterCollection.Contains(outturn3));
			statusFilter.Property = CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased;
			statusFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain Outturn1", !filterCollection.Contains(outturn1));
			Assert("Expect collection to contain Outturn2", filterCollection.Contains(outturn2));
			Assert("Expect collection not to contain Outturn3", !filterCollection.Contains(outturn3));
			statusFilter.Property = CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl;
			statusFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain Outturn1", !filterCollection.Contains(outturn1));
			Assert("Expect collection not to contain Outturn2", !filterCollection.Contains(outturn2));
			Assert("Expect collection not to contain Outturn3", !filterCollection.Contains(outturn3));
		}

		public void TestCommercialStatusFilter()
		{
			outturn1.C5_CommercialStatus = "AAA";
			outturn2.C5_CommercialStatus = "BBB";
			outturn3.C5_CommercialStatus = "AAA";
			Factory.Save();
			ModuleTextFilter statusFilter = (ModuleTextFilter)filterBO[FilterConstants.StatusTypes.Commercial];
			statusFilter.Property = "AAA";
			statusFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain Outturn1", filterCollection.Contains(outturn1));
			Assert("Expect collection not to contain Outturn2", !filterCollection.Contains(outturn2));
			Assert("Expect collection to contain Outturn3", filterCollection.Contains(outturn3));
			statusFilter.Property = "BBB";
			statusFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain Outturn1", !filterCollection.Contains(outturn1));
			Assert("Expect collection to contain Outturn2", filterCollection.Contains(outturn2));
			Assert("Expect collection not to contain Outturn3", !filterCollection.Contains(outturn3));
			statusFilter.Property = "CCC";
			statusFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain Outturn1", !filterCollection.Contains(outturn1));
			Assert("Expect collection not to contain Outturn2", !filterCollection.Contains(outturn2));
			Assert("Expect collection not to contain Outturn3", !filterCollection.Contains(outturn3));
		}

		public void TestUnderbondStatusFilter()
		{
			outturn1.C5_MessageStatus = CMRUnderbondStatuses.Codes.ExpectedCargoArrivalAdviceReceived;
			outturn2.C5_MessageStatus = CMRUnderbondStatuses.Codes.ExpectedCargoArrivalRescindAdviceReceived;
			outturn3.C5_MessageStatus = CMRUnderbondStatuses.Codes.ExpectedCargoArrivalAdviceReceived;
			Factory.Save();
			ModuleTextFilter statusFilter = (ModuleTextFilter)filterBO[FilterConstants.StatusTypes.Underbond];
			statusFilter.Property = CMRUnderbondStatuses.Codes.ExpectedCargoArrivalAdviceReceived;
			statusFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain Outturn1", filterCollection.Contains(outturn1));
			Assert("Expect collection not to contain Outturn2", !filterCollection.Contains(outturn2));
			Assert("Expect collection to contain Outturn3", filterCollection.Contains(outturn3));
			statusFilter.Property = CMRUnderbondStatuses.Codes.ExpectedCargoArrivalRescindAdviceReceived;
			statusFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain Outturn1", !filterCollection.Contains(outturn1));
			Assert("Expect collection to contain Outturn2", filterCollection.Contains(outturn2));
			Assert("Expect collection not to contain Outturn3", !filterCollection.Contains(outturn3));
			statusFilter.Property = CMRUnderbondStatuses.Codes.UnderbondSendingDelayed;
			statusFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain Outturn1", !filterCollection.Contains(outturn1));
			Assert("Expect collection not to contain Outturn2", !filterCollection.Contains(outturn2));
			Assert("Expect collection not to contain Outturn3", !filterCollection.Contains(outturn3));
		}

		public void TestVesselsProperty()
		{
			AssertNotNull(filterBO.Vessels);
		}

		public void TestVoyageFilter()
		{
			outturn1.C5_C6 = header1.PK;
			outturn2.C5_C6 = header2.PK;
			header1.C6_VoyageNum = "XAAAX";
			header2.C6_VoyageNum = "YAAAY";
			Factory.Save();
			ModuleTextAndNkFilter voyageFilter = (ModuleTextAndNkFilter)filterBO["Vessel / Voyage"];
			voyageFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			voyageFilter.Property = "XAA";
			voyageFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain Outturn1", filterCollection.Contains(outturn1));
			Assert("Expect collection not to contain Outturn2", !filterCollection.Contains(outturn2));
			voyageFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			voyageFilter.Property = "YAAAY";
			voyageFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain Outturn1", !filterCollection.Contains(outturn1));
			Assert("Expect collection to contain Outturn2", filterCollection.Contains(outturn2));
			voyageFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			voyageFilter.Property = "AAA";
			voyageFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain Outturn1", filterCollection.Contains(outturn1));
			Assert("Expect collection to contain Outturn2", filterCollection.Contains(outturn2));
			voyageFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			voyageFilter.Property = "ZZZ";
			voyageFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain Outturn1", !filterCollection.Contains(outturn1));
			Assert("Expect collection not to contain Outturn2", !filterCollection.Contains(outturn2));
		}

		public void TestVesselFilter()
		{
			outturn1.C5_C6 = header1.PK;
			outturn2.C5_C6 = header2.PK;
			header1.C6_VesselName = "XAAAX";
			header2.C6_VesselName = "YAAAY";
			Factory.Save();
			ModuleTextAndNkFilter vesselFilter = (ModuleTextAndNkFilter)filterBO["Vessel / Voyage"];
			vesselFilter.NkProperty = "XAAAX";
			vesselFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain Outturn1", filterCollection.Contains(outturn1));
			Assert("Expect collection not to contain Outturn2", !filterCollection.Contains(outturn2));
			vesselFilter.NkProperty = "YAA";
			vesselFilter.IsActive = true;
			vesselFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain Outturn1", !filterCollection.Contains(outturn1));
			Assert("Expect collection to contain Outturn2", filterCollection.Contains(outturn2));
			vesselFilter.NkProperty = "ZZZ";
			vesselFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain Outturn1", !filterCollection.Contains(outturn1));
			Assert("Expect collection not to contain Outturn2", !filterCollection.Contains(outturn2));
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new SeaCargoOutturnBillsFilterBusinessObject();

		DepotCusOutturn outturn1;
		DepotCusOutturn outturn2;
		DepotCusOutturn outturn3;
		DepotCusOutturn outturn4;
		DepotCusOutturn outturn5;
		CusOutturnHeader header1;
		CusOutturnHeader header2;
		CFSShipment shipment1;
		CFSShipment shipment2;
		CFSShipment shipment3;
		CFSContainer container1;
		CFSContainer container2;
		DepotCusOutturnCollection filterCollection;
		SeaCargoOutturnBillsFilterBusinessObject filterBO;
		protected override void SetUp()
		{
			base.SetUp();
			outturn1 = Factory.NewWithValidTestData<DepotCusOutturn>();
			outturn2 = Factory.NewWithValidTestData<DepotCusOutturn>();
			outturn3 = Factory.NewWithValidTestData<DepotCusOutturn>();
			outturn4 = Factory.NewWithValidTestData<DepotCusOutturn>();
			outturn5 = Factory.NewWithValidTestData<DepotCusOutturn>();
			header1 = Factory.NewWithValidTestData<CusOutturnHeader>();
			header2 = Factory.NewWithValidTestData<CusOutturnHeader>();
			shipment1 = Factory.NewWithValidTestData<CFSShipment>();
			shipment2 = Factory.NewWithValidTestData<CFSShipment>();
			shipment3 = Factory.NewWithValidTestData<CFSShipment>();
			container1 = Factory.NewWithValidTestData<CFSContainer>();
			container2 = Factory.NewWithValidTestData<CFSContainer>();
			filterBO = (SeaCargoOutturnBillsFilterBusinessObject)GetNewFilterStripBusinessObject();
			filterCollection = new DepotCusOutturnCollection(Factory);
		}
	}
}
