using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CusCAeMHItemSynchroniserTest : TestCaseWithFactory
	{
		public void TestCusCAeMHItemSynchroniser_PackLineData()
		{
			var helper = new CusCAeMHTestHelper(Factory);
			var shipment = helper.Shipment;
			var packline = shipment.OuterPackLines.AddNew();
			var product = packline.Products.AddNew();
			var item = helper.House.Items.AddNew();
			var synchroniser = new CusCAeMHItemSynchroniser(item, packline);
			synchroniser.SetEnabled(true, false);
			synchroniser.Synchronise();

			packline.JL_PackageCount = 100;
			product.D2_ProductQuantity = 50;
			AssertEquals(100m, item.BX_Quantity);
			packline.JL_PackageCount = 60;
			AssertEquals(60m, item.BX_Quantity);

			packline.JL_F3_NKPackType = Core.Constants.PkgUnit.Bag;
			product.D2_ProductUnitOfQty = Core.Constants.PkgUnit.Basket;
			AssertEquals(ACROSSPackageTypes.Codes.BAG, item.BX_QuantityUQ);
			packline.JL_F3_NKPackType = Core.Constants.PkgUnit.BaleCompressed;
			AssertEquals(ACROSSPackageTypes.Codes.BALEBLE, item.BX_QuantityUQ);

			packline.JL_HarmonisedCode = "123456";
			AssertEquals("123456", item.BX_HSCode);
			packline.JL_HarmonisedCode = "234567";
			AssertEquals("234567", item.BX_HSCode);

			shipment.JS_GoodsDescription = "SHIPMENT GOODS DESC";
			shipment.DetailedGoodsDescriptionNoteText = "SHIPMENT DETAIL DESC";
			packline.JL_Description = "PACK DESC";
			packline.JL_DetailedDescription = "DETAIL DESC";
			AssertEquals("DETAIL DESC", item.BX_Description);
			packline.JL_DetailedDescription = ZString.Empty;
			AssertEquals("PACK DESC", item.BX_Description);
			packline.JL_Description = ZString.Empty;
			AssertEquals("SHIPMENT DETAIL DESC", item.BX_Description);
			shipment.DetailedGoodsDescriptionNoteText = ZString.Empty;
			AssertEquals("SHIPMENT GOODS DESC", item.BX_Description);

			shipment.JS_MarksAndNumbers = "SHIPMENT MARKS";
			packline.JL_MarksAndNumbers = "PACK MARKS";
			AssertEquals("PACK MARKS", item.BX_Marks);
			packline.JL_MarksAndNumbers = ZString.Empty;
			AssertEquals("SHIPMENT MARKS", item.BX_Marks);
			shipment.JS_MarksAndNumbers = ZString.Empty;
			AssertEquals(ZString.Empty, item.BX_Marks);

			packline.JL_PackageCount = 0;
			shipment.JS_TotalPackageCount = 105;
			shipment.JS_F3_NKTotalCountPackType = ACROSSPackageTypes.Codes.CARTON;
			synchroniser.Synchronise();
			AssertEquals(105m, item.BX_Quantity);
			AssertEquals(ACROSSPackageTypes.Codes.CARTON, item.BX_QuantityUQ);
		}

		public void TestCusCAeMHItemSynchroniser_FromInnerPackLineAndOuterPackLine()
		{
			var helper = new CusCAeMHTestHelper(Factory);
			var shipment = helper.Shipment;
			var house = helper.House;
			var outerPackLine = shipment.OuterPackLines.AddNew();
			outerPackLine.JL_PackageCount = 100;
			var innerPackLine = shipment.InnerPackLines.AddNew();
			innerPackLine.JL_JL_OuterPackLine = outerPackLine.PK;
			innerPackLine.JL_PackageCount = 200;
			var item = helper.House.Items.AddNew();
			var synchroniser = new CusCAeMHItemSynchroniser(item, innerPackLine, outerPackLine);
			synchroniser.SetEnabled(true, false);
			synchroniser.Synchronise();

			AssertEquals("quantity from inner", 200m, item.BX_Quantity);
			innerPackLine.JL_PackageCount = 201;
			AssertEquals("quantity update from inner", 201m, item.BX_Quantity);

			outerPackLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Bag;
			innerPackLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Coil;
			AssertEquals("unit from inner", ACROSSPackageTypes.Codes.COIL, item.BX_QuantityUQ);
			innerPackLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Drum;
			AssertEquals("unit update from inner", ACROSSPackageTypes.Codes.DRUM, item.BX_QuantityUQ);

			outerPackLine.JL_HarmonisedCode = "111111";
			innerPackLine.JL_HarmonisedCode = "222222";
			AssertEquals("hs code from outer", "111111", item.BX_HSCode);
			outerPackLine.JL_HarmonisedCode = "333333";
			AssertEquals("hs code update from outer", "333333", item.BX_HSCode);

			shipment.JS_GoodsDescription = "SHIPMENT GOODS DESC";
			shipment.DetailedGoodsDescriptionNoteText = "SHIPMENT DETAIL DESC";
			innerPackLine.JL_Description = "INNER DESC";
			outerPackLine.JL_Description = "OUTER DESC";
			AssertEquals("INNER DESC", item.BX_Description);
			innerPackLine.JL_Description = ZString.Empty;
			AssertEquals("OUTER DESC", item.BX_Description);

			shipment.JS_MarksAndNumbers = "SHIPMENT MARKS";
			outerPackLine.JL_MarksAndNumbers = "OUTER PACK MARKS";
			innerPackLine.JL_MarksAndNumbers = "INNER PACK MARKS";
			AssertEquals("OUTER PACK MARKS", item.BX_Marks);
			outerPackLine.JL_MarksAndNumbers = ZString.Empty;
			AssertEquals("SHIPMENT MARKS", item.BX_Marks);
			shipment.JS_MarksAndNumbers = ZString.Empty;
			AssertEquals("marks from outer", ZString.Empty, item.BX_Marks);
		}
	}
}
