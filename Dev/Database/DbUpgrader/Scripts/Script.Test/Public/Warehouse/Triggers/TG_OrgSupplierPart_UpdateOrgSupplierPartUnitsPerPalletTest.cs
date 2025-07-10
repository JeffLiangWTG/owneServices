using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_OrgSupplierPart_UpdateOrgSupplierPartUnitsPerPallet))]
	class TG_OrgSupplierPart_UpdateOrgSupplierPartUnitsPerPalletTest : DBCreateTriggerScriptTest
	{
		#region TestTG_OrgSupplierPart_UpdateOrgSupplierPartUnitsPerPallet_NewProduct

		public void TestTG_OrgSupplierPart_UpdateOrgSupplierPartUnitsPerPallet_NewProduct()
		{
			var product = new OrgSupplierPart("BOOK") { OP_StockKeepingUnit = "UNT" }.InsertAndReturnObject(TestConnection);
			var productFromDB = OrgSupplierPart.ShallowLoadFromDB(TestConnection, product.PK);
			AssertEquals("OP_UnitsPerPallet should be 0", 0m, productFromDB.OP_UnitsPerPallet);
		}

		#endregion

		#region TestTG_OrgSupplierPart_UpdateOrgSupplierPartUnitsPerPallet_UpdateStockKeepingUnit

		public void TestTG_OrgSupplierPart_UpdateOrgSupplierPartUnitsPerPallet_UpdateStockKeepingUnit()
		{
			var product = new OrgSupplierPart("BOOK") { OP_StockKeepingUnit = "UNT" }.InsertAndReturnObject(TestConnection);
			new OrgPartUnit(product, 5, "UNT", "CAS").Insert(TestConnection);
			new OrgPartUnit(product, 4, "CAS", "BOX").Insert(TestConnection);
			new OrgPartUnit(product, 2, "BOX", "PLT").Insert(TestConnection);

			var productFromDB = OrgSupplierPart.ShallowLoadFromDB(TestConnection, product.PK);
			AssertEquals("OP_UnitsPerPallet should be 40", 40m, productFromDB.OP_UnitsPerPallet);

			OrgSupplierPart.UpdateWhere(product.PK).Set(o => o.OP_StockKeepingUnit, "CAS").Post(TestConnection);

			productFromDB = OrgSupplierPart.ShallowLoadFromDB(TestConnection, product.PK);
			AssertEquals("OP_UnitsPerPallet should be 8", 8m, productFromDB.OP_UnitsPerPallet);

			OrgSupplierPart.UpdateWhere(product.PK).Set(o => o.OP_StockKeepingUnit, "BOX").Post(TestConnection);

			productFromDB = OrgSupplierPart.ShallowLoadFromDB(TestConnection, product.PK);
			AssertEquals("OP_UnitsPerPallet should be 2", 2m, productFromDB.OP_UnitsPerPallet);
		}

		#endregion
	}
}

