namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.DocumentWrappers.GenericWrappers;
	using Enterprise.DocumentWrappers.GenericWrappers.Base;
	using Enterprise.DocumentWrappers.GenericWrappers.Warehouse;
	using Enterprise.Warehouse.Transactions.Business;
	using NUnit.Framework;

	[TestedType(typeof(WarehouseDynamicWorkOrderWrapper))]
	public class WarehouseDynamicWorkOrderWrapperTest : WarehousePickableDocketWrapperTest
	{
		protected override void TestIsWorkOrderCore()
		{
			var wrapper = new WarehouseDynamicWorkOrderWrapper(Factory.New<WhsDynamicWorkOrder>(), Factory);
			Assert(wrapper.IsWorkOrder);
		}

		protected override void TestFulfillRuleCore()
		{
			var wrapper = new WarehouseDynamicWorkOrderWrapper(Factory.New<WhsDynamicWorkOrder>(), Factory);
			AssertEquals("", wrapper.FulfillRule.Code);
		}

		protected override void TestIsCustomsTransactionsCore()
		{
			var wrapper = new WarehouseDynamicWorkOrderWrapper(Factory.New<WhsDynamicWorkOrder>(), Factory);
			AssertEquals(true, wrapper.IsCustomsTransaction);
		}

		#region Implementation

		protected override WarehouseJobGenericWrapper GetNewWarehouseJobGenericWrapper(BusinessObject bizO)
		{
			return new WarehouseDynamicWorkOrderWrapper((WhsDynamicWorkOrder)bizO, Factory);
		}

		protected override WarehouseJobGenericWrapper GetNewWarehouseJobGenericWrapperInSpecifiedFactory(BusinessObject bizO, BusinessObjectFactory factory)
		{
			return new WarehouseDynamicWorkOrderWrapper((WhsDynamicWorkOrder)bizO, factory);
		}

		protected override WhsDocket GetNewDocket()
		{
			return Factory.NewWithValidTestData<WhsDynamicWorkOrder>();
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			var load = Factory.NewWithValidTestData<WhsDynamicWorkOrder>();
			return new WarehouseDynamicWorkOrderWrapper(load, Factory);
		}

		protected override WhsDocket GetNewDocketInSpecifiedFactory(BusinessObjectFactory factory) => factory.New<WhsDynamicWorkOrder>();
		protected override BusinessObject GetNewWhsBusinessObject() => Factory.New<WhsDynamicWorkOrder>();

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
CarrierAccount :  is null
CarrierServiceLevel : 
Client : CLIENT\n#1
ClientRequestedBillToParty :  is null
CODAmount : 
CODType : 
ConfirmationInstructions : 
Consignee : 
ConsigneeAddress : 
Consignor : CLIENT\n#1
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
JobClient : CLIENT\n#1
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
WhoCreated : E
WhoFinalised : 
";
			}
		}

		#endregion
	}
}
