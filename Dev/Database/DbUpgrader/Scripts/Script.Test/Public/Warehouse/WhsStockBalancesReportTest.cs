using System;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse
{
	[TestedType(typeof(WhsStockBalancesReport))]
	class WhsStockBalancesReportTest : DbCreateScriptTest
	{
		#region TestNoArithmeticOverflowExceptionThrown_WhenStockOnHandIsMassive

		public void TestNoArithmeticOverflowExceptionThrown_WhenStockOnHandIsMassive()
		{
			var warehouse = new WhsWarehouse("WHS").WithDockDoor(TestConnection);
			var client = new OrgHeader("C1").InsertAndReturnObject(TestConnection);
			new OrgMiscServ(client).Insert(TestConnection);
			var product = new OrgSupplierPart("P1").InsertAndReturnObject(TestConnection);
			new OrgPartRelation(client, product, "OWN") { OU_ClientUQ = "UNT" }.Insert(TestConnection);
			var location = warehouse.CreateLocations(TestConnection, 1)[0];

			// insert over 1 Quadrillion units in stock on hand
			var receive = new WhsDocket(client.PK, warehouse.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = new DateTime(2018, 1, 1) }.InsertAndReturnObject(TestConnection);
			new WhsDocketLine(receive, product.PK, 900000000000000, location.PK) { WE_StockOnHand = 900000000000000 }.Insert(TestConnection);
			new WhsDocketLine(receive, product.PK, 900000000000000, location.PK) { WE_StockOnHand = 900000000000000 }.Insert(TestConnection);

			AssertEquals("Client Quantity should return 0, since Stock on Hand is too large to calculate Conversion.", 0m,
				TestConnection.ExecuteScalar($"SELECT ClientQuantity FROM WhsStockBalancesReport({DateTime.Today.AsSQL()}, null)"));
		}
		#endregion
	}
}

