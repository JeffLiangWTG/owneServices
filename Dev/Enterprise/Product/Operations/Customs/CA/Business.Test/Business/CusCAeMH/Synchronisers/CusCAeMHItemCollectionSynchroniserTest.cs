using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CusCAeMHItemCollectionSynchroniserTest : TestCaseWithFactory
	{
		public void TestCusCAeMHItemCollectionSynchroniser()
		{
			var helper = new CusCAeMHTestHelper(Factory);
			var shipment = helper.Shipment;
			var house = helper.House;
			var synchroniser = new CusCAeMHItemCollectionSynchroniser(shipment, house);
			AssertEquals(0, house.Items.Count);
			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 100;
			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 50;
			AssertEquals(2, house.Items.Count);
			var item1 = house.Items[0];
			var item2 = house.Items[1];
			if (item1.BX_Quantity == 50)
			{
				item1 = house.Items[1];
				item2 = house.Items[0];
			}
			AssertEquals("item1.BX_Quantity", 100m, item1.BX_Quantity);
			AssertEquals("item2.BX_Quantity", 50m, item2.BX_Quantity);

			var packLine1Product1 = packLine1.Products.AddNew();
			packLine1Product1.D2_ProductQuantity = 60;
			synchroniser.Synchronise();
			AssertEquals(2, house.Items.Count);
			AssertEquals(false, item1.IsDeleted);
			AssertEquals(false, item2.IsDeleted);
			item1 = house.Items[0];
			item2 = house.Items[1];
			if (item1.BX_Quantity == 50)
			{
				item1 = house.Items[1];
				item2 = house.Items[0];
			}
			AssertEquals("item1.BX_Quantity", 100m, item1.BX_Quantity);
			AssertEquals("item2.BX_Quantity", 50m, item2.BX_Quantity);

			packLine1Product1.Delete();
			AssertEquals(2, house.Items.Count);
			AssertCollectionContains(item1, house.Items);
			AssertCollectionContains(item2, house.Items);
			AssertEquals("item1.IsDeleted", false, item1.IsDeleted);
			AssertEquals("item2.BX_Quantity", 50m, item2.BX_Quantity);

			packLine1.Delete();
			AssertEquals(1, house.Items.Count);
			AssertCollectionContains(item2, house.Items);
			AssertEquals("item1.IsDeleted", true, item1.IsDeleted);
			AssertEquals("item2.BX_Quantity", 50m, item2.BX_Quantity);
		}

		public void TestCusCAeMHItemCollectionSynchroniser_FromInnerPackLineAndOuterPackLine()
		{
			var helper = new CusCAeMHTestHelper(Factory);
			var shipment = helper.Shipment;
			var house = helper.House;
			var synchroniser = new CusCAeMHItemCollectionSynchroniser(shipment, house);
			AssertEquals(0, house.Items.Count);
			var outerPackLine1 = shipment.OuterPackLines.AddNew();
			outerPackLine1.JL_PackageCount = 101;
			var outerPackLine2 = shipment.OuterPackLines.AddNew();
			outerPackLine2.JL_PackageCount = 102;
			var innerPackLine1 = shipment.InnerPackLines.AddNew();
			innerPackLine1.JL_PackageCount = 201;
			innerPackLine1.JL_JL_OuterPackLine = outerPackLine1.PK;
			var innerPackLine2 = shipment.InnerPackLines.AddNew();
			innerPackLine2.JL_PackageCount = 202;
			innerPackLine2.JL_JL_OuterPackLine = outerPackLine1.PK;
			synchroniser.Synchronise();
			AssertEquals("2 inner 1 outer, total 3", 3, house.Items.Count);
			AssertNotNull("201 from inner pack line", house.Items.FirstOrDefault(f => f.BX_Quantity == 201));
			AssertNotNull("202 from inner pack line", house.Items.FirstOrDefault(f => f.BX_Quantity == 202));
			AssertNotNull("102 from outer pack line", house.Items.FirstOrDefault(f => f.BX_Quantity == 102));

			innerPackLine2.JL_JL_OuterPackLine = outerPackLine2.PK;
			synchroniser.Synchronise();
			AssertEquals("2 inner, total 2", 2, house.Items.Count);
			AssertNotNull("201 from inner pack line", house.Items.FirstOrDefault(f => f.BX_Quantity == 201));
			AssertNotNull("202 from inner pack line", house.Items.FirstOrDefault(f => f.BX_Quantity == 202));

			shipment.InnerPackLines.Remove(innerPackLine1);
			synchroniser.Synchronise();
			AssertEquals("1 inner 1 outer, total 2", 2, house.Items.Count);
			AssertNotNull("202 from inner pack line", house.Items.FirstOrDefault(f => f.BX_Quantity == 202));
			AssertNotNull("101 from outer pack line", house.Items.FirstOrDefault(f => f.BX_Quantity == 101));

			shipment.InnerPackLines.Remove(innerPackLine2);
			synchroniser.Synchronise();
			AssertEquals("2 outer, total 2", 2, house.Items.Count);
			AssertNotNull("101 from outer pack line", house.Items.FirstOrDefault(f => f.BX_Quantity == 101));
			AssertNotNull("102 from outer pack line", house.Items.FirstOrDefault(f => f.BX_Quantity == 102));
		}
	}
}
