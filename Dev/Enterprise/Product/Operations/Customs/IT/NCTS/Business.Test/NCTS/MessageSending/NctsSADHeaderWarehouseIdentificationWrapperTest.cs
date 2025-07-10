using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsSADHeaderWarehouseIdentificationWrapperTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When movementHeader is null", () => new NctsSADHeaderWarehouseIdentificationWrapper(null));
		AssertNoExceptionThrown(() => new NctsSADHeaderWarehouseIdentificationWrapper(nctsHeader.MovementHeader));
	}

	public void TestType()
	{
		AssertEquals($"When WarehouseAddress not set, {nameof(warehouseIdentificationWrapper.Type)}", ZString.Empty, warehouseIdentificationWrapper.Type);

		nctsHeader.MovementHeader.BM_OA_WarehouseAddress = warehouseAddress.PK;
		warehouseIdentificationWrapper = new NctsSADHeaderWarehouseIdentificationWrapper(nctsHeader.MovementHeader);
		AssertEquals($"When WarehouseAddress is set, {nameof(warehouseIdentificationWrapper.Type)}", "A", warehouseIdentificationWrapper.Type);
	}

	public void TestIdentification()
	{
		AssertEquals($"When WarehouseAddress not set, {nameof(warehouseIdentificationWrapper.Identification)}", ZString.Empty, warehouseIdentificationWrapper.Identification);

		nctsHeader.MovementHeader.BM_OA_WarehouseAddress = warehouseAddress.PK;
		warehouseIdentificationWrapper = new NctsSADHeaderWarehouseIdentificationWrapper(nctsHeader.MovementHeader);
		AssertEquals($"When WarehouseAddress is set, {nameof(warehouseIdentificationWrapper.Identification)}", "123456", warehouseIdentificationWrapper.Identification);
	}

	public void TestCinIdentification()
	{
		AssertEquals($"When WarehouseAddress not set, {nameof(warehouseIdentificationWrapper.CinIdentification)}", ZString.Empty, warehouseIdentificationWrapper.CinIdentification);

		nctsHeader.MovementHeader.BM_OA_WarehouseAddress = warehouseAddress.PK;
		warehouseIdentificationWrapper = new NctsSADHeaderWarehouseIdentificationWrapper(nctsHeader.MovementHeader);
		AssertEquals($"When WarehouseAddress is set, {nameof(warehouseIdentificationWrapper.CinIdentification)}", "N", warehouseIdentificationWrapper.CinIdentification);
	}

	public void TestAuthorizingCountry()
	{
		AssertEquals($"When WarehouseAddress not set, {nameof(warehouseIdentificationWrapper.AuthorizingCountry)}", ZString.Empty, warehouseIdentificationWrapper.AuthorizingCountry);

		nctsHeader.MovementHeader.BM_OA_WarehouseAddress = warehouseAddress.PK;
		warehouseIdentificationWrapper = new NctsSADHeaderWarehouseIdentificationWrapper(nctsHeader.MovementHeader);
		AssertEquals($"When WarehouseAddress is set, {nameof(warehouseIdentificationWrapper.AuthorizingCountry)}", "IT", warehouseIdentificationWrapper.AuthorizingCountry);
	}

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.NewDepartureNctsHeader();
		warehouseAddress = Factory.New<OrgHeader>().Addresses.AddNew();
		warehouseAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "A123456NIT", "IT");
		warehouseIdentificationWrapper = new NctsSADHeaderWarehouseIdentificationWrapper(nctsHeader.MovementHeader);
	}

	NctsHeader nctsHeader;
	OrgAddress warehouseAddress;
	NctsSADHeaderWarehouseIdentificationWrapper warehouseIdentificationWrapper;
}
