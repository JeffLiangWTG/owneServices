using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class CcpHelperTest : TestCaseWithFactory
{
	public void TestGetWarehouseCodeFromCcp()
	{
		AssertNoExceptionThrown("No exception should be thrown when string lenght is smaller than 7 chars", () => CcpHelper.GetWarehouseCodeFromCcp(""));

		var warehouseCode = CcpHelper.GetWarehouseCodeFromCcp("C123456XIT");
		AssertEquals("Warehosue code lenght must be = 7", 7, warehouseCode.Length);
		AssertEquals("123456X", warehouseCode);
	}
}
