using System.Linq;
using CargoWise.Customs.AE.MessageContracts.Mirsal2;
using Enterprise.Customs.AE.Business.MessageSending.Mirsal2.Testing;

namespace Enterprise.Customs.AE.Business.Testing;

sealed class VehicleDetailsTypeDataProviderTest : Mirsal2VehicleDetailsTypeDataProviderAbstractClassBase
{
	public override void TestVehicleChassisNumber()
	{
		vehicle.CVH_VehicleIdentificationNumber = "VIN12345";
		AssertEquals(vehicle.CVH_VehicleIdentificationNumber, CreateDataProvider().VehicleChassisNumber);
	}

	public override void TestVehicleBrand()
	{
		vehicle.CVH_BrandName = "24";
		AssertEquals(24, CreateDataProvider().VehicleBrand);
	}

	public override void TestVehicleModel()
	{
		vehicle.CVH_ModelName = "ModelX";
		AssertEquals(vehicle.CVH_ModelName, CreateDataProvider().VehicleModel);
	}

	public override void TestVehicleEngineNumber()
	{
		engine.CEG_EngineNumber = "ENG12345";
		AssertEquals(engine.CEG_EngineNumber, CreateDataProvider().VehicleEngineNumber);
	}

	public override void TestVehicleEngineCapacity()
	{
		engine.CEG_CapacityCC = 1.23m;
		AssertEquals(engine.CEG_CapacityCC, CreateDataProvider().VehicleEngineCapacity);
	}

	public override void TestVehiclePassengerCapacity()
	{
		Assert("to do in future WI", true);
	}

	public override void TestCarriageCapacity()
	{
		Assert("to do in future WI", true);
	}

	public override void TestVehicleYearOfBuilt()
	{
		vehicle.CVH_ModelYear = "2025";
		AssertEquals(vehicle.CVH_ModelYear, CreateDataProvider().VehicleYearOfBuilt);
	}

	public override void TestVehicleColour()
	{
		vehicle.CVH_Color = "white";
		AssertEquals(vehicle.CVH_Color, CreateDataProvider().VehicleColour);
	}

	public override void TestVehicleCondition() => CombineAssertions(() =>
	{
		vehicle.CVH_IsUsed = true;
		AssertEquals("O", CreateDataProvider().VehicleCondition);
		vehicle.CVH_IsUsed = false;
		AssertEquals("N", CreateDataProvider().VehicleCondition);
	});

	public override void TestVehicleType()
	{
		vehicle.CVH_CarType = "RWD";
		AssertEquals(vehicle.CVH_CarType, CreateDataProvider().VehicleType);
	}

	public override void TestVehicleDrive()
	{
		vehicle.CVH_DriveSide = "L";
		AssertEquals(vehicle.CVH_DriveSide, CreateDataProvider().VehicleDrive);
	}

	public override void TestVehicleSpecificationStandardCode()
	{
		vehicle.CVH_SpecificationStandard = "1";
		AssertEquals(vehicle.CVH_SpecificationStandard, CreateDataProvider().VehicleSpecificationStandardCode);
	}

	protected override VehicleDetailsTypeDataProviderAbstractClass CreateDataProvider()
	{
		return DeclarationRequestDataProvider.CreateProvider(header, additionalDataProvider).Declaration.ShippingDetails.Invoices.First().InvoiceItemsDetail.First().VehicleDetail.First();
	}

	protected override void SetUp()
	{
		base.SetUp();
		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

		var cusEntryLine = header.MergedLines.AddNew();
		cusEntryLine.InvoiceLines.Add(invoiceLine);

		vehicle = invoiceLine.Vehicles.AddNew();
		engine = (CusEngine)vehicle.Engines.AddNew();
	}

	CusVehicle vehicle;
	CusEngine engine;
}
