using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Warehouse.Transactions.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(WarehousePickNonPickedItemsWrapper))]
	sealed class WarehousePickNonPickedItemsWrapperTest : WarehousePickingSlipWrapperTest
	{
		#region TestDocumentTitleCore

		protected override void TestDocumentTitleCore()
		{
			AssertEquals("Non Picked Items", NonPickedItemsWrapper.DocumentTitle);
		}

		#endregion

		#region TestSecondaryHeading

		protected override void TestSecondaryHeadingCore()
		{
			AssertEquals("Pick Details", NonPickedItemsWrapper.SecondaryHeading);
		}

		#endregion

		#region TestJobLines

		protected override void TestJobLinesCore()
		{
			AssertEquals("Pre-Condition", 0, Pick.OrderedInventories.Count);
			AssertEquals("Pre-Condition", 0, DocWrapper.JobLines.Count);

			SetupInventoryWithItemToPick(Pick, 10m, 10m, 10m, 20m);
			var wrapperNoNonPicked = (WarehousePickNonPickedItemsWrapper)GetNewWarehouseJobGenericWrapper(Pick);
			AssertEquals("Should have no non picked items", 0, wrapperNoNonPicked.JobLines.Count);
		}

		public void TestJobLinesOneNonPicked()
		{
			var pickOneNonPicked = Factory.New<WhsPick>();
			SetupInventoryWithItemToPick(pickOneNonPicked, 10m, 10m, 0m, 25m);
			var wrapperOneNonPicked = (WarehousePickNonPickedItemsWrapper)GetNewWarehouseJobGenericWrapper(pickOneNonPicked);
			AssertEquals("Should have only one non picked item", 1, wrapperOneNonPicked.JobLines.Count);
		}

		public void TestJobLinesTwoNonPicked()
		{
			var pickTwoNonPicked = Factory.New<WhsPick>();
			SetupInventoryWithItemToPick(pickTwoNonPicked, 0m, 15m, 0m, 25m);
			var wrapperTwoNonPicked = (WarehousePickNonPickedItemsWrapper)GetNewWarehouseJobGenericWrapper(pickTwoNonPicked);
			AssertEquals("Should have only two non picked items", 2, wrapperTwoNonPicked.JobLines.Count);
		}

		#endregion

		#region Overriden Abstract Members

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return NonPickedItemsWrapper;
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
CarrierAccount :  is null
CarrierServiceLevel :  is null
Client :  is null
ClientRequestedBillToParty :  is null
CODAmount :  is null
CODType :  is null
ConfirmationInstructions : 
Consignee :  is null
ConsigneeAddress :  is null
Consignor :  is null
CubicSent :  is null
CustomerReference : 
CustomsStatus :  is null
Destination :  is null
DistributionCentreAddress :  is null
DropMode :  is null
DropOffAddress :  is null
FinalisedDate : 
Forwarder :  is null
FulfillRule :  is null
GoodsBillToAddress :  is null
HandlingInstructions : 
IncoTerm :  is null
Insurance :  is null
JobClient :  is null
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
ServiceLevel :  is null
SOPCarrierServiceLevel : 
SOPOrderNumber : 
SOPRequiredDate : 
SOPSpecialInstructions : 
SOPStagingAreaName : 
SOPTransportCompany : 
SplitNumber : 
StagingAreaName : 
StagingLocationString : 
Status : 
Supplier :  is null
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
TransportCoAddress :  is null
TransportCompany :  is null
TransportReference : 
VehicleReference : 
Warehouse :  is null
WarehouseName : 
WeightSent :  is null
WhoCreated : 
WhoFinalised :
";
			}
		}

		#endregion
		#region GetNewWarehouseJobGenericWrapper

		protected override WarehouseJobGenericWrapper GetNewWarehouseJobGenericWrapper(BusinessObject bizO)
		{
			return new WarehousePickNonPickedItemsWrapper((WhsPick)bizO, Factory);
		}

		#endregion

		#region NonPickedItemsWrapper

		WarehousePickNonPickedItemsWrapper NonPickedItemsWrapper
		{
			get { return nonPickedItemsWrapper ?? (nonPickedItemsWrapper = (WarehousePickNonPickedItemsWrapper)Wrapper); }
		}
		WarehousePickNonPickedItemsWrapper nonPickedItemsWrapper;

		#endregion

	}
}
