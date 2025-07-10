using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.ChildWrappers.Warehouse;
using Enterprise.DocumentWrappers.GenericWrappers.Warehouse;
using Enterprise.Environment;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(WarehouseTransferWrapper))]
	sealed class WarehouseTransferWrapperTest : WarehouseDocketWrapperTest
	{
		#region TestJobLines

		protected override void TestJobLinesCore()
		{
			AssertEquals(typeof(WarehouseTransferLineWrapperCollection), ((WarehouseTransferWrapper)Wrapper).JobLines.GetType());
		}

		#endregion

		#region TestPalletizedInventory

		protected override void TestPalletizedInventory_Setup(TestDataSimpleEnvironment data, out WhsDocket docket, out WhsInventoryView inventory)
		{
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.FindLocation("A-1"), "PLT-1");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", new TestNotificationBuffer());
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "PLT-1", "A-2", "PLT-2");
			transfer.FinaliseDocket();
			AssertEquals("Precondition - ensure Transfer is Finalised.", true, transfer.IsFinalised);

			docket = transfer;
			inventory = transferLine.Inventory[0];
		}

		#endregion

		#region TestSecondaryReference

		protected override void TestSecondaryReferenceCore()
		{
			var transfer = GetNewDocket();
			var transferWrapper = GetNewWarehouseJobGenericWrapper(transfer);
			AssertEquals("Pre-condition", "", transferWrapper.SecondaryReference.Value);

			transfer.WD_ExternalReference = "Test12345";
			AssertEquals("Transfer Reference", transferWrapper.SecondaryReference.Label);
			AssertEquals("Test12345", transferWrapper.SecondaryReference.Value);
		}

		#endregion

		#region TestJobType

		protected override void TestJobTypeCore()
		{
			var transfer = Factory.New<WhsTransfer>();
			var wrapper = new WarehouseTransferWrapper(transfer, Factory);
			AssertEquals("Transfer", wrapper.JobType);
		}

		#endregion

		#region TestIWhsDocumentInventory_SetInventory

		public void TestIWhsDocumentInventory_SetInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory, saveFactory_doNotUseForNewTests: false);
			var destinationWarehouse = Helper.CreateWarehouse("W2", "R1");
			Factory.Save();
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 3, data.Whs1.FindLocation("A"), "", true, true);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1", new TestNotificationBuffer(), TransferType.Codes.InterWhsSource);
			Helper.CreateWhsTransferLine(transfer, data.Part1, 2, "A", destinationWarehouse.PK, "R1");
			transfer.FinaliseDocket();
			Factory.Save();
			AssertEquals("Precondition", true, transfer.IsFinalised);

			var line = transfer.ChildTransfers.ElementAt(0).Lines.Single();
			line.Inventory.Load();
			var inventoryLine = line.Inventory.Cast<WhsInventoryView>().Single();
			var docInventory = new WhsDocumentInventory(inventoryLine);
			docInventory.LabelsToPrint = 2;
			var wrapper = new WarehouseTransferWrapper(transfer, Factory);
			var documentInventory = (IWhsDocumentInventory)wrapper;
			documentInventory.SetInventory(docInventory);
			AssertEquals("Two job lines should be returned for two inventory items.", 2, wrapper.JobLines.Count);
			AssertEquals(inventoryLine.InDocketLine, wrapper.JobLines[0].WrappedObject);
			AssertEquals(inventoryLine.InDocketLine, wrapper.JobLines[1].WrappedObject);
		}

		#endregion

		#region TestWarehouseBOWrapper_FactoryCached

		protected override WhsDocket GetNewDocketInSpecifiedFactory(BusinessObjectFactory factory)
		{
			return factory.New<WhsTransfer>();
		}

		protected override WarehouseJobGenericWrapper GetNewWarehouseJobGenericWrapperInSpecifiedFactory(BusinessObject bizO, BusinessObjectFactory factory)
		{
			return new WarehouseTransferWrapper((WhsTransfer)bizO, factory);
		}

		#endregion

		#region Implementation

		protected override WhsDocket GetNewDocket()
		{
			return Factory.New<WhsTransfer>();
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			WhsTransfer transfer = Factory.NewWithValidTestData<WhsTransfer>();
			return new WarehouseTransferWrapper(transfer, Factory);
		}

		protected override WarehouseJobGenericWrapper GetNewWarehouseJobGenericWrapper(BusinessObject bizO)
		{
			return new WarehouseTransferWrapper((WhsTransfer)bizO, Factory);
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return string.Format(@"
CarrierAccount :  is null
CarrierServiceLevel : 
Client : CLIENT\n#1
ClientRequestedBillToParty :  is null
CODAmount :  is null
CODType :  is null
ConfirmationInstructions : 
Consignee :  is null
ConsigneeAddress :  is null
Consignor : CLIENT\n#1
CubicSent :  is null
CustomerReference : 
CustomsStatus :  is null
Destination :  is null
DistributionCentreAddress :  is null
DropMode : 
DropOffAddress :  is null
FinalisedDate : 
Forwarder : 
FulfillRule :  is null
GoodsBillToAddress :  is null
HandlingInstructions : 
IncoTerm :  is null
Insurance :  is null
JobClient : CLIENT\n#1
PackagesSent :  is null
PickingInstructions : 
PickMethod : 
PickNo : 
PickNumberReference : 
PickOption :  is null
PickUpAddress :  is null
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
SupplierDocAddress :  is null
TotalExtendedLinePrice :  is null
TotalLoadedPackages : 
TotalLoadedUnits : 
TotalLoadedVolume : 
TotalLoadedWeight : 
TotalOuterPackagesVolume : 
TotalOuterPackagesWeight : 
TransportationUnit :  is null
TransportBillToAddress :  is null
TransportCoAddress : 
TransportCompany : 
TransportReference : 
VehicleReference : 
Warehouse : 
WarehouseName : 
WeightSent :  is null
WhoCreated : {0}
WhoFinalised :
", Env.CurrentUser.Initials);
			}
		}

		#endregion
	}
}
