using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Warehouse.Transactions.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(WarehousePickShortfallItemsWrapper))]
	sealed class WarehousePickShortfallItemsWrapperTest : WarehousePickingSlipWrapperTest
	{
		#region TestDocumentTitleCore

		protected override void TestDocumentTitleCore()
		{
			AssertEquals("Shortfall Items", PickShortfallItemsWrapper.DocumentTitle);
		}

		#endregion

		#region TestSecondaryHeading

		protected override void TestSecondaryHeadingCore()
		{
			AssertEquals("Pick Details", PickShortfallItemsWrapper.SecondaryHeading);
		}

		#endregion

		#region TestJobLines

		protected override void TestJobLinesCore()
		{
			AssertEquals("Precondition: WhsPick ItemsToPick Collection should be empty", 0, Pick.OrderedInventories.Count);
			AssertEquals("Precondition: Document Wrapper ShortfallItems Collection should be empty", 0, PickShortfallItemsWrapper.JobLines.Count);

			SetupInventoryWithItemToPick(Pick, 10m, 10m, 20m, 20m);
			var wrapperNoShortfalls = (WarehousePickShortfallItemsWrapper)GetNewWarehouseJobGenericWrapper(Pick);
			AssertEquals("Should have no shortfall items", 0, wrapperNoShortfalls.JobLines.Count);
		}

		public void TestJobLinesPickShortfall()
		{
			var pickOneShortfall = Factory.New<WhsPick>();
			SetupInventoryWithItemToPick(pickOneShortfall, 10m, 10m, 20m, 25m);
			var wrapperOneShortfall = (WarehousePickShortfallItemsWrapper)GetNewWarehouseJobGenericWrapper(pickOneShortfall);
			AssertEquals("Should have only one shortfall item", 1, wrapperOneShortfall.JobLines.Count);
		}

		public void TestJobLinesPickTwoShortfalls()
		{
			var pickTwoShortfalls = Factory.New<WhsPick>();
			SetupInventoryWithItemToPick(pickTwoShortfalls, 10m, 15m, 20m, 25m);
			var wrapperTwoShortfalls = (WarehousePickShortfallItemsWrapper)GetNewWarehouseJobGenericWrapper(pickTwoShortfalls);
			AssertEquals("Should have only two shortfall items", 2, wrapperTwoShortfalls.JobLines.Count);
		}

		#endregion

		#region Overriden Abstract Members

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return PickShortfallItemsWrapper;
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
			return new WarehousePickShortfallItemsWrapper((WhsPick)bizO, Factory);
		}

		#endregion

		#region PickShortfallItemsWrapper

		WarehousePickShortfallItemsWrapper PickShortfallItemsWrapper
		{
			get { return pickShortfallItemsWrapper ?? (pickShortfallItemsWrapper = (WarehousePickShortfallItemsWrapper)Wrapper); }
		}
		WarehousePickShortfallItemsWrapper pickShortfallItemsWrapper;

		#endregion

	}
}
