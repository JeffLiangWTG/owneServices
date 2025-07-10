using System;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Import.Testing;

sealed class WarehouseWrapperTest : TestCase
{
	public void TestContructor()
	{
		AssertExceptionThrown<ArgumentException>("Exception expected when Warehouse Type is empty", () => new WarehouseWrapper(ZString.Empty, "ID123"));
		AssertExceptionThrown<ArgumentException>("Exception expected when Warehouse ID is empty", () => new WarehouseWrapper("R", ZString.Empty));
	}

	public void TestIdentificationNumber()
	{
		IWarehouse warehouseWrapper = new WarehouseWrapper("R", "123456");
		AssertEquals(nameof(IWarehouse.IdentificationNumber), "123456", warehouseWrapper.IdentificationNumber);
	}

	public void TestWarehouseType()
	{
		IWarehouse warehouseWrapper = new WarehouseWrapper("A", "123456");
		AssertEquals(nameof(IWarehouse.WarehouseType), "A", warehouseWrapper.WarehouseType);
	}
}
