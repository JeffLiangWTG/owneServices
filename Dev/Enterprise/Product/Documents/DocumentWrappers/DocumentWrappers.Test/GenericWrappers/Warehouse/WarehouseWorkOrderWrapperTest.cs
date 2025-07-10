using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Warehouse;
using Enterprise.DocumentWrappers.GenericWrappers.Warehouse.ChildWrappers;
using Enterprise.Environment;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(WarehouseWorkOrderWrapper))]
	sealed class WarehouseWorkOrderWrapperTest : WarehousePickableDocketWrapperTest
	{
		#region Test Properties

		protected override void TestSecondaryReferenceCore()
		{
			WorkOrder.WD_ExternalReference = "ABC";
			AssertEquals("SecondaryReference Label.", "Work Order No.", WorkOrderWrapper.SecondaryReference.Label);
			AssertEquals("SecondaryReference Value.", "ABC", WorkOrderWrapper.SecondaryReference.Value);
		}

		protected override void TestIsWorkOrderCore()
		{
			Assert(WorkOrderWrapper.IsWorkOrder);
		}

		protected override void TestFulfillRuleCore()
		{
			//Check this with Shane
			AssertEquals("", WorkOrderWrapper.FulfillRule.Code);
		}

		protected override void TestJobLinesCore()
		{
			AssertEquals(typeof(WarehouseWorkOrderLineWrapperCollection), WorkOrderWrapper.JobLines.GetType());
		}

		public void TestWorkOrderLevelHeirarchyTree()
		{
			AssertNotNull("Precondition", WorkOrder);
			AssertNotNull("Precondition", WorkOrderWrapper);

			for (int idx = 1; idx < 11; idx++)
			{
				AssertEquals("Master Work Order levels", true, GetWorkOrderLevel(WorkOrderWrapper, idx).Contains(ZString.Format("Level {0}", idx)));
			}
		}

		protected override bool IsUsingDockDoorLocation
		{
			get { return false; }
		}

		#endregion

		#region Implementation

		ZString GetWorkOrderLevel(WarehouseWorkOrderWrapper workOrderDocketWrapper, int level)
		{
			return workOrderDocketWrapper.WorkOrderLevelDescription(level);
		}

		#region TestSubTypeDesc

		protected override void TestSubTypeDescCore()
		{
			Docket.WD_DocketSubType = "ASS";
			AssertEquals("Assemble", WorkOrderWrapper.SubTypeDesc);
		}

		#endregion

		#region TestWarehouseBOWrapper_FactoryCached

		protected override WhsDocket GetNewDocketInSpecifiedFactory(BusinessObjectFactory factory)
		{
			return factory.New<WhsWorkOrder>();
		}

		protected override WarehouseJobGenericWrapper GetNewWarehouseJobGenericWrapperInSpecifiedFactory(BusinessObject bizO, BusinessObjectFactory factory)
		{
			return new WarehouseWorkOrderWrapper((WhsWorkOrder)bizO, factory);
		}

		#endregion

		WhsWorkOrder WorkOrder
		{
			get
			{
				if (workOrder == null)
				{
					workOrder = Helper.CreateWhsWorkOrder(Helper.CreateClient("CL1"), Helper.CreateWarehouse("W1"));
					Docket.WD_OH_Client = workOrder.WD_OH_Client;
					Docket.WD_WW_Whs = workOrder.WD_WW_Whs;
					TestDataForBOM.CreateBOMProducts();
					TestDataForBOM.BOM.BulkLoadDeepLevelMasterChildWorkOrders(workOrder, 15);
				}
				return workOrder;
			}
		}
		WhsWorkOrder workOrder;

		WarehouseWorkOrderWrapper WorkOrderWrapper => workOrderWrapper ?? (workOrderWrapper = new WarehouseWorkOrderWrapper(WorkOrder, Factory));
		WarehouseWorkOrderWrapper workOrderWrapper;

		TestDataForBOM TestDataForBOM => testDataForBOM ?? (testDataForBOM = new TestDataForBOM(Factory));
		TestDataForBOM testDataForBOM;

		protected override WhsDocket GetNewDocket()
		{
			return Factory.New<WhsWorkOrder>();
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return string.Format(@"
CarrierAccount :  is null
CarrierServiceLevel : 
Client : 
ClientRequestedBillToParty :  is null
CODAmount : 
CODType : 
ConfirmationInstructions : 
Consignee : 
ConsigneeAddress : 
Consignor : 
CubicSent : 
CustomerReference : 
CustomsStatus :  is null
Destination :  is null
DistributionCentreAddress :  is null
DropMode : 
DropOffAddress : 
FinalisedDate : 
Forwarder : 
FulfillRule : 
GoodsBillToAddress : 
HandlingInstructions : 
IncoTerm : 
Insurance : 
JobClient : 
PackagesSent : 
PickingInstructions : 
PickMethod : 
PickNo : 
PickNumberReference : 
PickOption : AUT - Auto Pick
PickUpAddress : 
PrimaryBarcode : 
Registry : (No Default Field Value Available on Registry)
RequiredDate : 
SalesChannel :  is null
SecondaryReference : 
ServiceLevel : 
SOPCarrierServiceLevel : 
SOPOrderNumber : 
SOPRequiredDate : 
SOPSpecialInstructions : 
SOPStagingAreaName : 
SOPTransportCompany : 
SplitNumber : 
StagingAreaName : 
StagingLocationString : 
Status : New Entry (Unsaved)
Supplier : 
SupplierBuyerLink :  is null
SupplierDocAddress : 
TotalExtendedLinePrice : 
TotalLoadedPackages : 
TotalLoadedUnits : 
TotalLoadedVolume : 
TotalLoadedWeight : 
TotalOuterPackagesVolume : 
TotalOuterPackagesWeight : 
TransportationUnit :  is null
TransportBillToAddress : 
TransportCoAddress : 
TransportCompany : 
TransportReference : 
VehicleReference : 
Warehouse : 
WarehouseName : 
WeightSent : 
WhoCreated : {0}
WhoFinalised :
", Env.CurrentUser.Initials);
			}
		}

		protected override WarehouseJobGenericWrapper GetNewWarehouseJobGenericWrapper(BusinessObject bizO)
		{
			return new WarehouseWorkOrderWrapper((WhsWorkOrder)bizO, Factory);
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			var workOrder = Factory.New<WhsWorkOrder>();
			return new WarehouseWorkOrderWrapper(workOrder, Factory);
		}

		#endregion

		public override void TestWrapperMappingsEmpty()
		{
			var emptyWrapper = GetNewWarehouseJobGenericWrapper(null);
			AssertEquals("BOLNumber", "", emptyWrapper.BOLNumber);
			AssertNull("CarrierServiceLevel", emptyWrapper.CarrierServiceLevel);
			AssertEquals("CartageAdviceClosingText", "", emptyWrapper.CartageAdviceClosingText);
			AssertEquals("CartageAdviceOpeningText", "", emptyWrapper.CartageAdviceOpeningText);
			AssertEquals("CartageDropMode", "", emptyWrapper.CartageDropMode);
			AssertNull("Client", emptyWrapper.Client);
			AssertNull("CODAmount", emptyWrapper.CODAmount);
			AssertNull("CODType", emptyWrapper.CODType);
			AssertNull("Consignee", emptyWrapper.Consignee);
			AssertNull("ConsigneeAddress", emptyWrapper.ConsigneeAddress);
			AssertEquals("ConsolidatedInvoiceRef", "", emptyWrapper.ConsolidatedInvoiceRef);
			AssertEquals("ContainerNumberAndTypeLine", "", emptyWrapper.ContainerNumberAndTypeLine);
			AssertEquals("CustomerReference", true, emptyWrapper.CustomerReference.IsEmpty);
			AssertNull("Destination", emptyWrapper.Destination);
			AssertNull("DropMode", emptyWrapper.DropMode);
			AssertEquals("FinalisedDate", true, emptyWrapper.FinalisedDate.IsEmpty);
			AssertNull("FulfillRule", emptyWrapper.FulfillRule);
			AssertNull("GoodsBillToAddress", emptyWrapper.GoodsBillToAddress);
			AssertEquals("HandlingInstructions", true, emptyWrapper.HandlingInstructions.IsEmpty);
			AssertNull("IncoTerm", emptyWrapper.IncoTerm);
			AssertNull("Insurance", emptyWrapper.Insurance);
			AssertEquals("InvoiceNumber", "", emptyWrapper.InvoiceNumber);
			AssertNull("JobClient", emptyWrapper.JobClient);
			AssertEquals("JobNumber", "", emptyWrapper.JobNumber);
			AssertEquals("PickingInstructions", true, emptyWrapper.PickingInstructions.IsEmpty);
			AssertEquals("PickMethod", true, emptyWrapper.PickMethod.IsEmpty);
			AssertEquals("PickNo", true, emptyWrapper.PickNo.IsEmpty);
			AssertEquals("PickNumberReference", true, emptyWrapper.PickNumberReference.IsEmpty);
			AssertNull("", emptyWrapper.PickOption);
			AssertEquals("PrimaryBarcode", true, emptyWrapper.PrimaryBarcode.IsEmpty);
			AssertEquals("PrimaryBarcodeText", true, emptyWrapper.PrimaryBarcodeText.IsEmpty);
			AssertEquals("References", "", emptyWrapper.References);
			AssertEquals("RequiredDate", true, emptyWrapper.RequiredDate.IsEmpty);
			AssertEquals("SecondaryHeading", "", emptyWrapper.SecondaryHeading);
			AssertEquals("SecondaryNumber", "", emptyWrapper.SecondaryNumber);
			AssertEquals("SecondaryReference", true, emptyWrapper.SecondaryReference.IsEmpty);
			AssertNull("ServiceLevel", emptyWrapper.ServiceLevel);
			AssertEquals("Status", true, emptyWrapper.Status.IsEmpty);
			AssertEquals("TotalNumberOfLabels", 0, emptyWrapper.TotalNumberOfLabels);
			AssertEquals("TotalNumberOfPackageLabels", 0, emptyWrapper.TotalNumberOfPackageLabels);
			AssertNull("TransportBillToAddress", emptyWrapper.TransportBillToAddress);
			AssertEquals("TransportCoAddress", AddressWrapper.Empty(Factory).Address, emptyWrapper.TransportCoAddress.Address);
			AssertNull("TransportCompany", emptyWrapper.TransportCompany);
			AssertEquals("TransportReference", true, emptyWrapper.TransportReference.IsEmpty);
			AssertNull("Warehouse", emptyWrapper.Warehouse);
			AssertEquals("WarehouseCartageCoordinatorName", "", emptyWrapper.WarehouseCartageCoordinatorName);
			AssertEquals("WarehouseCartageCoordinatorPhone", "", emptyWrapper.WarehouseCartageCoordinatorPhone);
			AssertEquals("WhoCreated", true, emptyWrapper.WhoCreated.IsEmpty);
			AssertEquals("WhoFinalised", true, emptyWrapper.WhoFinalised.IsEmpty);

			TestWrapperMappingsEmpty_NotDependantToBizOProperties(emptyWrapper);
		}
	}
}
