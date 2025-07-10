using System.Data;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class OrderReferenceListTest : ScriptTest
	{
		public void TestOrderReferenceList()
		{
			var shipment = TestObjectCreator.CreateShipment("S0001");
			var orderHeaderWithShipment1 = shipment.AttachedOrders.AddNew();
			orderHeaderWithShipment1.BuyerPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			orderHeaderWithShipment1.JD_OrderNumber = "Order1";
			var orderHeaderWithShipment2 = shipment.AttachedOrders.AddNew();
			orderHeaderWithShipment2.BuyerPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			orderHeaderWithShipment2.JD_OrderNumber = "Order2";

			var declaration = Factory.New<BaseJobDeclaration>();
			var orderHeaderWithDeclaration1 = declaration.AttachedOrders.AddNew();
			orderHeaderWithDeclaration1.JD_OrderNumber = "Order3";
			orderHeaderWithDeclaration1.BuyerPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			var orderHeaderWithDepartment2 = declaration.AttachedOrders.AddNew();
			orderHeaderWithDepartment2.JD_OrderNumber = "Order4";
			orderHeaderWithDepartment2.BuyerPK = Factory.NewWithValidTestData<OrgHeader>().PK;

			Factory.Save();

			var shipmentDocsAndCartage = shipment.DocsAndCartage;
			var orderItem1 = shipmentDocsAndCartage.OrderItems.AddNew();
			orderItem1.JT_OrderReference = "OrderItem1";
			var orderItem2 = shipmentDocsAndCartage.OrderItems.AddNew();
			orderItem2.JT_OrderReference = "OrderItem2";

			var declationDocsAndCartage = declaration.DocsAndCartage;
			var orderItem3 = declationDocsAndCartage.OrderItems.AddNew();
			orderItem3.JT_OrderReference = "OrderItem3";
			var orderItem4 = declationDocsAndCartage.OrderItems.AddNew();
			orderItem4.JT_OrderReference = "OrderItem4";

			Factory.Save();

			var result = RunScript(shipment.PK, new ZGuid(), new ZGuid(), new ZGuid());
			AssertEquals("Order1, Order2", result.Rows[0][0]);
			result = RunScript(new ZGuid(), new ZGuid(), shipmentDocsAndCartage.PK, new ZGuid());
			AssertEquals("OrderItem1, OrderItem2", result.Rows[0][0]);

			result = RunScript(new ZGuid(), declaration.PK, new ZGuid(), new ZGuid());
			AssertEquals("Order3, Order4", result.Rows[0][0]);
			result = RunScript(new ZGuid(), new ZGuid(), new ZGuid(), declationDocsAndCartage.PK);
			AssertEquals("OrderItem3, OrderItem4", result.Rows[0][0]);
		}

		DataTable RunScript(ZGuid shipmentPK, ZGuid declarationPK, ZGuid shipmentCartagePK, ZGuid declarationCartagePK)
		{
			string sql = string.Format(@"SELECT OrderReferences FROM dbo.OrderReferenceList('{0}','{1}', '{2}', '{3}')", shipmentPK, declarationPK, shipmentCartagePK, declarationCartagePK);

			return DataUtils.GetDataTableFromQuery(Db.Connection, sql);
		}
	}
}


