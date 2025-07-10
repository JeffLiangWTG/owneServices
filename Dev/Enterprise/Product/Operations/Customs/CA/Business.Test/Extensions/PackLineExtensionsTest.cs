using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class PackLineExtensionsTest : TestCaseWithFactory
	{
		public void TestAddWeekDays()
		{
			var helper = new CusCAeMHTestHelper(Factory);
			var shipment = helper.Shipment;
			var container = helper.MasterBill.Containers.AddNew();
			var packLine = shipment.OuterPackLines.AddNew();
			AssertEquals("No consol does not cause exception", ZString.Empty, packLine.ContainerNumberForConsol(null));
			AssertEquals("No container", ZString.Empty, packLine.ContainerNumberForConsol(helper.Consol));
			helper.Container1.JC_ContainerNum = "C1";
			packLine.JL_JC = helper.Container1.PK;
			AssertEquals("With container", "C1", packLine.ContainerNumberForConsol(helper.Consol));
		}

		public void TestGetEffectivePackLineQuantity()
		{
			var helper = new CusCAeMHTestHelper(Factory);
			var shipment = helper.Shipment;

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 100;
			packLine.JL_F3_NKPackType = "BAG";

			var data = packLine.GetEffectivePackLineQuantity();
			AssertEquals(100, data.Qty);
			AssertEquals("BAG", data.UQ);

			var product = packLine.Products.AddNew();
			product.D2_ProductQuantity = 0;
			product.D2_ProductUnitOfQty = Core.Constants.PkgUnit.Bag;
			data = packLine.GetEffectivePackLineQuantity();
			AssertEquals(100, data.Qty);
			AssertEquals("BAG", data.UQ);

			product.D2_ProductQuantity = 0;
			packLine.JL_PackageCount = 0;
			shipment.JS_TotalPackageCount = 101;
			shipment.JS_F3_NKPackType = "CTN";
			data = packLine.GetEffectivePackLineQuantity();
			AssertEquals(101, data.Qty);
			AssertEquals("CTN", data.UQ);
		}
	}
}
