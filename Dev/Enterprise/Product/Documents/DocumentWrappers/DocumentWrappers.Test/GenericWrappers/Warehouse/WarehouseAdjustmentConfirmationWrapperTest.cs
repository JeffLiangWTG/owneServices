using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(WarehouseAdjustmentConfirmationWrapper))]
	sealed class WarehouseAdjustmentConfirmationWrapperTest : WarehouseDocketWrapperTest
	{
		#region TestSecondaryReference

		protected override void TestSecondaryReferenceCore()
		{
			var adjustment = GetNewDocket();
			var adjustmentConfirmationWrapper = GetNewWarehouseJobGenericWrapper(adjustment);

			AssertEquals("Pre-condition", "ADJUSTMENT", adjustmentConfirmationWrapper.SecondaryReference.Value);
			adjustment.WD_ExternalReference = "Test12345";
			AssertEquals("Reference", adjustmentConfirmationWrapper.SecondaryReference.Label);
			AssertEquals("Test12345", adjustmentConfirmationWrapper.SecondaryReference.Value);
		}

		#endregion

		#region TestDocumentTitle

		protected override void TestDocumentTitleCore()
		{
			AssertEquals("Adjustment Confirmation", AdjustmentConfirmationWrapper.DocumentTitle);
		}

		#endregion

		#region TestJobLines

		protected override void TestJobLinesCore()
		{
			var adjustmentConfirmationWrapper = (WarehouseAdjustmentConfirmationWrapper)GetNewWarehouseJobGenericWrapper(null);
			AssertEquals(0, adjustmentConfirmationWrapper.JobLines.Count);

			var data = new TestDataSimpleEnvironment(Factory);
			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 10m, data.Whs1.DefaultLocation);
			var adjustmentConfirmationWrapper2 = (WarehouseAdjustmentConfirmationWrapper)GetNewWarehouseJobGenericWrapper(adjustment);
			AssertEquals(1, adjustmentConfirmationWrapper2.JobLines.Count);
		}

		#endregion

		#region TestPalletizedInventory

		protected override void TestPalletizedInventory_Setup(TestDataSimpleEnvironment data, out WhsDocket docket, out WhsInventoryView inventory)
		{
			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "ADJ1", new TestNotificationBuffer());
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 10m, "A-1", "PLT-2");
			adjustment.FinaliseDocket();
			AssertEquals("Precondition - ensure Adjustment is finalised.", true, adjustment.IsFinalised);

			Factory.Save();

			docket = adjustment;
			inventory = Factory.LoadTop1<WhsInventoryView>(new ZQuery(WhsInventoryViewSchema.WI_WE_InDocketLine, adjustmentLine.PK));
		}

		#endregion

		#region TestWrapperMappingsEmpty_NotDependantToBizOProperties

		protected override void TestWrapperMappingsEmpty_NotDependantToBizOProperties(WarehouseJobGenericWrapper emptyWrapper)
		{
			AssertEquals("DocumentTitle", "Adjustment Confirmation", emptyWrapper.DocumentTitle);
			AssertEquals("JobNumberHeading", "", emptyWrapper.JobNumberHeading);
			AssertNull("TotalExtendedLinePrice", emptyWrapper.TotalExtendedLinePrice);
		}

		#endregion

		#region Overriden Abstract Members

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return AdjustmentConfirmationWrapper;
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
CODAmount :  is null
CODType :  is null
ConfirmationInstructions : 
Consignee :  is null
ConsigneeAddress :  is null
Consignor : 
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
JobClient : 
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
SecondaryReference : ADJUSTMENT
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
", Environment.Env.CurrentUser.Initials);
			}
		}

		#endregion

		#region TestIWhsDocumentInventory_SetInventory

		public void TestIWhsDocumentInventory_SetInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "1", new TestNotificationBuffer());
			var line = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 2, "A");
			adjustment.FinaliseDocket();
			Factory.Save();
			AssertEquals("Precondition", true, adjustment.IsFinalised);

			line.Inventory.Load();
			var inventoryLine = line.Inventory.Cast<WhsInventoryView>().Single();
			var docInventory = new WhsDocumentInventory(inventoryLine);
			docInventory.LabelsToPrint = 2;
			var wrapper = new WarehouseAdjustmentConfirmationWrapper(adjustment, Factory);
			var documentInventory = (IWhsDocumentInventory)wrapper;
			documentInventory.SetInventory(docInventory);
			AssertEquals("Two job lines should be returned for two inventory items.", 2, wrapper.JobLines.Count);
			AssertEquals(inventoryLine.InDocketLine, wrapper.JobLines[0].WrappedObject);
			AssertEquals(inventoryLine.InDocketLine, wrapper.JobLines[1].WrappedObject);
		}

		#endregion

		#region TestJobType

		protected override void TestJobTypeCore()
		{
			var adjustment = Factory.New<WhsAdjustment>();
			var wrapper = new WarehouseAdjustmentConfirmationWrapper(adjustment, Factory);
			AssertEquals("Adjustment", wrapper.JobType);
		}

		#endregion

		#region Implementation

		#region Adjustment

		protected override WhsDocket GetNewDocket()
		{
			return Factory.New<WhsAdjustment>();
		}

		#endregion

		#region GetNewWarehouseJobGenericWrapper

		protected override WarehouseJobGenericWrapper GetNewWarehouseJobGenericWrapper(BusinessObject bizO)
		{
			return new WarehouseAdjustmentConfirmationWrapper((WhsAdjustment)bizO, Factory);
		}

		#endregion

		#region TestWarehouseBOWrapper_FactoryCached

		protected override WhsDocket GetNewDocketInSpecifiedFactory(BusinessObjectFactory factory)
		{
			return factory.New<WhsAdjustment>();
		}

		protected override WarehouseJobGenericWrapper GetNewWarehouseJobGenericWrapperInSpecifiedFactory(BusinessObject bizO, BusinessObjectFactory factory)
		{
			return new WarehouseAdjustmentConfirmationWrapper((WhsAdjustment)bizO, factory);
		}

		#endregion

		#region AdjustmentConfirmationWrapper

		WarehouseAdjustmentConfirmationWrapper AdjustmentConfirmationWrapper => adjustmentConfirmationWrapper ?? (adjustmentConfirmationWrapper = (WarehouseAdjustmentConfirmationWrapper)Wrapper);
		WarehouseAdjustmentConfirmationWrapper adjustmentConfirmationWrapper;

		#endregion

		#endregion
	}
}
