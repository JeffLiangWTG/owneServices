using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_PreventForLocationTypeHaveClassNotFIXIfPickFace))]
	class TG_PreventForLocationTypeHaveClassNotFIXIfPickFaceTest : DBCreateTriggerScriptTest
	{
		public void TestTG_PreventClassUpdateForLocationTypeUsedByPickFace()
		{
			var sql = new SqlQueryBuilder();
			var locationTypeFIXUsed = new WhsLocationType("PF1") { WLT_LocationClass = "FIX", WLT_MaximumNumberOfProducts = 1 }.AppendInsertAndReturnObject(sql);
			var locationTypeFIXNotUsed = new WhsLocationType("PF2") { WLT_LocationClass = "FIX", WLT_MaximumNumberOfProducts = 1 }.AppendInsertAndReturnObject(sql);

			var branch1 = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs1 = new WhsWarehouse("WH1", branch1.PK).WithDockDoor(TestConnection);
			var area1 = new WhsArea(whs1.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row1 = new WhsRow(whs1, "B").AppendInsertAndReturnObject(sql);
			var locationA1 = new WhsLocation(row1.PK, area1.PK, area1.PK, locationTypeFIXUsed.PK).AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("CLT001").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("CLT001").AppendInsertAndReturnObject(sql);

			var pickFace = new WhsPickFace(product.PK, client.PK, locationA1.PK).AppendInsertAndReturnObject(sql);
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertNoExceptionThrown(
				() => WhsLocationType
				.UpdateWhere(locationTypeFIXNotUsed.PK)
				.Set(lt => lt.WLT_LocationClass, "NOR")
				.Set(lt => lt.WLT_MaximumNumberOfProducts, 0).Post(TestConnection));

			AssertExceptionThrown(typeof(SqlException), "A location type ultimately used in a Pick Face must have a class of FIX", 
				() => WhsLocationType
				.UpdateWhere(locationTypeFIXUsed.PK)
				.Set(lt => lt.WLT_LocationClass, "NOR")
				.Set(lt => lt.WLT_MaximumNumberOfProducts, 0).Post(TestConnection), true);
		}
	}
}

