using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.ChildWrappers.Warehouse;
using Enterprise.Warehouse.Transit.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(WhsItemTransferHeaderWrapper))]
	sealed class WhsItemTransferHeaderWrapperTest : WarehouseJobGenericWrapperTest
	{
		#region TestJobNumberHeadingCore

		protected override void TestJobNumberHeadingCore()
		{
			AssertEquals("Transfer ID", TransferHeaderWrapper.JobNumberHeading);
		}

		#endregion

		#region TestJobNumberCore

		protected override void TestJobNumberCore()
		{
			TransferHeader.WTH_ReferenceNumber = "WTH010101";

			AssertEquals("WTH010101", TransferHeaderWrapper.JobNumber);
		}

		#endregion

		#region TestJobNumberCore

		protected override void TestJobLinesCore()
		{
			AssertEquals(typeof(WhsItemTransferLineWrapperCollection), TransferHeaderWrapper.JobLines.GetType());
			AssertEquals(0, TransferHeaderWrapper.JobLines.Count);
		}

		#endregion

		#region TestJobType

		protected override void TestJobTypeCore()
		{
			AssertNotNull(TransferHeaderWrapper.JobType);
			AssertEquals(TransferHeader.WTH_TransferType, TransferHeaderWrapper.JobType);
		}

		#endregion

		#region Implementation

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

		public override void TestWrapperMappingsEmpty()
		{
		}

		#region TestWarehouseBOWrapper_FactoryCached

		protected override void SetWhsBusinessObjectWarehouse(BusinessObject bizO, ZGuid warehousePK)
		{
			((WhsItemTransferHeader)bizO).WTH_WW_Warehouse = warehousePK;
		}

		protected override BusinessObject GetNewWhsBusinessObjectInSpecifiedFactory(BusinessObjectFactory factory)
		{
			return factory.New<WhsItemTransferHeader>();
		}

		protected override WarehouseJobGenericWrapper GetNewWarehouseJobGenericWrapperInSpecifiedFactory(BusinessObject bizO, BusinessObjectFactory factory)
		{
			return new WhsItemTransferHeaderWrapper((WhsItemTransferHeader)bizO, factory);
		}

		#endregion

		protected override WarehouseJobGenericWrapper GetNewWarehouseJobGenericWrapper(BusinessObject bizO)
		{
			return new WhsItemTransferHeaderWrapper((WhsItemTransferHeader)bizO, Factory);
		}

		protected override BusinessObject GetNewWhsBusinessObject()
		{
			return Factory.New<WhsItemTransferHeader>();
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new WhsItemTransferHeaderWrapper(TransferHeader, Factory);
		}

		#endregion

		#region TestIsFinalised

		protected override void TestIsFinalisedCore()
		{
			TransferHeader.WTH_IsFinalised = ZBool.True;
			AssertEquals(ZBool.True, TransferHeaderWrapper.IsFinalised);
		}

		#endregion

		WhsItemTransferHeader TransferHeader => (WhsItemTransferHeader)WhsBusinessObject;

		WhsItemTransferHeaderWrapper TransferHeaderWrapper => (WhsItemTransferHeaderWrapper)Wrapper;
	}
}
