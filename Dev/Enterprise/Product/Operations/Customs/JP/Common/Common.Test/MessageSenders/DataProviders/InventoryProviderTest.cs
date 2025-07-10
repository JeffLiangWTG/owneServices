using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Common.Testing
{
	[TestedType(typeof(InventoryProvider))]
	sealed class InventoryProviderTest : TestCaseWithFactory
	{
		public void TestInventoryManagementNumber()
		{
			var info = Factory.New<CusSupportingInfo>();
			info.CSI_Code = "T4";
			var provider = new InventoryProvider(info);
			AssertEquals("T4", provider.InventoryManagementNumber);
		}

		public void TestMoveInQuantity()
		{
			var info = Factory.New<CusSupportingInfo>();
			info.CSI_Quantity = 5m;
			var provider = new InventoryProvider(info);
			AssertEquals(5m, provider.MoveInQuantity);
		}
	}
}
