using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_OrgPartUnit_UpdateOrgSupplierPartUnitsPerPallet))]
	class TG_OrgPartUnit_UpdateOrgSupplierPartUnitsPerPalletTest : DBCreateTriggerScriptTest
	{
		#region TestTG_OrgPartUnit_UpdateOrgSupplierPartUnitsPerPallet_NewProduct

		public void TestTG_OrgPartUnit_UpdateOrgSupplierPartUnitsPerPallet_NewProduct()
		{
			var product = new OrgSupplierPart("BOOK") { OP_StockKeepingUnit = "UNT" }.InsertAndReturnObject(TestConnection);
			var productFromDB = OrgSupplierPart.ShallowLoadFromDB(TestConnection, product.PK);
			AssertEquals("OP_UnitsPerPallet should be 0", (decimal)0.0, productFromDB.OP_UnitsPerPallet);
		}

		#endregion

		#region TestTG_OrgPartUnit_UpdateOrgSupplierPartUnitsPerPallet_InsertOrgPartUnit

		public void TestTG_OrgPartUnit_UpdateOrgSupplierPartUnitsPerPallet_InsertOrgPartUnit_NoConversionToPallet()
		{
			var product = new OrgSupplierPart("BOOK") { OP_StockKeepingUnit = "UNT" }.InsertAndReturnObject(TestConnection);
			new OrgPartUnit(product, 5, "UNT", "CAS").Insert(TestConnection);

			var productFromDB = OrgSupplierPart.ShallowLoadFromDB(TestConnection, product.PK);
			AssertEquals("OP_UnitsPerPallet should be 0", (decimal)0.0, productFromDB.OP_UnitsPerPallet);
		}

		public void TestTG_OrgPartUnit_UpdateOrgSupplierPartUnitsPerPallet_InsertOrgPartUnit_ConversionToPallet()
		{
			var product = new OrgSupplierPart("BOOK") { OP_StockKeepingUnit = "UNT" }.InsertAndReturnObject(TestConnection);
			new OrgPartUnit(product, 5, "UNT", "CAS").Insert(TestConnection);
			new OrgPartUnit(product, 2, "CAS", "PLT").Insert(TestConnection);

			var productFromDB = OrgSupplierPart.ShallowLoadFromDB(TestConnection, product.PK);
			AssertEquals("OP_UnitsPerPallet should be 10", (decimal)10.0, productFromDB.OP_UnitsPerPallet);
		}

		public void TestTG_OrgPartUnit_UpdateOrgSupplierPartUnitsPerPallet_InsertOrgPartUnit_DecimalUnitsPerPallet()
		{
			var product = new OrgSupplierPart("BOOK") { OP_StockKeepingUnit = "UNT" }.InsertAndReturnObject(TestConnection);
			new OrgPartUnit(product, 5, "PLT", "UNT").Insert(TestConnection);

			var productFromDB = OrgSupplierPart.ShallowLoadFromDB(TestConnection, product.PK);
			AssertEquals("OP_UnitsPerPallet should be 0.2", (decimal)0.2, productFromDB.OP_UnitsPerPallet);
		}

		#endregion

		#region TestTG_OrgPartUnit_UpdateOrgSupplierPartUnitsPerPallet_UpdateOrgPartUnit

		public void TestTG_OrgPartUnit_UpdateOrgSupplierPartUnitsPerPallet_UpdateOrgPartUnit_PackType()
		{
			var product = new OrgSupplierPart("BOOK") { OP_StockKeepingUnit = "UNT" }.InsertAndReturnObject(TestConnection);
			new OrgPartUnit(product, 5, "UNT", "CAS").Insert(TestConnection);
			var conversion = new OrgPartUnit(product, 2, "CTN", "PLT").InsertAndReturnObject(TestConnection);

			var productFromDB = OrgSupplierPart.ShallowLoadFromDB(TestConnection, product.PK);
			AssertEquals("OP_UnitsPerPallet should be 0", (decimal)0.0, productFromDB.OP_UnitsPerPallet);

			OrgPartUnit
				.UpdateWhere(conversion.PK)
				.Set(u => u.OF_PackType, "CAS").Post(TestConnection);

			productFromDB = OrgSupplierPart.ShallowLoadFromDB(TestConnection, product.PK);
			AssertEquals("OP_UnitsPerPallet should be 10", (decimal)10.0, productFromDB.OP_UnitsPerPallet);
		}

		public void TestTG_OrgPartUnit_UpdateOrgSupplierPartUnitsPerPallet_UpdateOrgPartUnit_ParentPackType()
		{
			var product = new OrgSupplierPart("BOOK") { OP_StockKeepingUnit = "UNT" }.InsertAndReturnObject(TestConnection);
			new OrgPartUnit(product, 5, "UNT", "CAS").Insert(TestConnection);
			var conversion = new OrgPartUnit(product, 2, "CAS", "CTN").InsertAndReturnObject(TestConnection);

			var productFromDB = OrgSupplierPart.ShallowLoadFromDB(TestConnection, product.PK);
			AssertEquals("OP_UnitsPerPallet should be 0", (decimal)0.0, productFromDB.OP_UnitsPerPallet);

			OrgPartUnit
				.UpdateWhere(conversion.PK)
				.Set(u => u.OF_ParentPackType, "PLT").Post(TestConnection);

			productFromDB = OrgSupplierPart.ShallowLoadFromDB(TestConnection, product.PK);
			AssertEquals("OP_UnitsPerPallet should be 10", (decimal)10.0, productFromDB.OP_UnitsPerPallet);
		}

		public void TestTG_OrgPartUnit_UpdateOrgSupplierPartUnitsPerPallet_UpdateOrgPartUnit_QtyInParent()
		{
			var product = new OrgSupplierPart("BOOK") { OP_StockKeepingUnit = "UNT" }.InsertAndReturnObject(TestConnection);
			new OrgPartUnit(product, 5, "UNT", "CAS").Insert(TestConnection);
			var conversion = new OrgPartUnit(product, 10, "CAS", "PLT").InsertAndReturnObject(TestConnection);

			var productFromDB = OrgSupplierPart.ShallowLoadFromDB(TestConnection, product.PK);
			AssertEquals("OP_UnitsPerPallet should be 50", (decimal)50.0, productFromDB.OP_UnitsPerPallet);

			OrgPartUnit
				.UpdateWhere(conversion.PK)
				.Set(u => u.OF_QuantityInParent, 5m).Post(TestConnection);

			productFromDB = OrgSupplierPart.ShallowLoadFromDB(TestConnection, product.PK);
			AssertEquals("OP_UnitsPerPallet should be 25", (decimal)25.0, productFromDB.OP_UnitsPerPallet);
		}

		#endregion

		#region TestTG_OrgPartUnit_UpdateOrgSupplierPartUnitsPerPallet_DeleteOrgPartUnit

		public void TestTG_OrgPartUnit_UpdateOrgSupplierPartUnitsPerPallet_DeleteOrgPartUnit()
		{
			var product = new OrgSupplierPart("BOOK") { OP_StockKeepingUnit = "UNT" }.InsertAndReturnObject(TestConnection);
			var conversion = new OrgPartUnit(product, 5, "UNT", "PLT").InsertAndReturnObject(TestConnection);

			var productFromDB = OrgSupplierPart.ShallowLoadFromDB(TestConnection, product.PK);
			AssertEquals("OP_UnitsPerPallet should be 5", (decimal)5.0, productFromDB.OP_UnitsPerPallet);

			OrgPartUnit.DeleteInDB(TestConnection, conversion.PK);

			productFromDB = OrgSupplierPart.ShallowLoadFromDB(TestConnection, product.PK);
			AssertEquals("OP_UnitsPerPallet should be 0", (decimal)0.0, productFromDB.OP_UnitsPerPallet);
		}

		#endregion

		#region TestTG_OrgPartUnit_UpdateOrgSupplierPartUnitsPerPallet_UpdateIrrelevantColumn

		public void TestTG_OrgPartUnit_UpdateOrgSupplierPartUnitsPerPallet_UpdateIrrelevantColumn()
		{
			var product = new OrgSupplierPart("BOOK") { OP_StockKeepingUnit = "UNT" }.InsertAndReturnObject(TestConnection);
			new OrgPartUnit(product, 5, "UNT", "CAS").Insert(TestConnection);
			var conversion = new OrgPartUnit(product, 10, "CAS", "PLT").InsertAndReturnObject(TestConnection);

			var productFromDB = OrgSupplierPart.ShallowLoadFromDB(TestConnection, product.PK);
			AssertEquals("OP_UnitsPerPallet should be 50", (decimal)50.0, productFromDB.OP_UnitsPerPallet);

			OrgPartUnit
				.UpdateWhere(conversion.PK)
				.Set(u => u.OF_Width, 5.0m).Post(TestConnection);

			productFromDB = OrgSupplierPart.ShallowLoadFromDB(TestConnection, product.PK);
			AssertEquals("OP_UnitsPerPallet should be 50", (decimal)50.0, productFromDB.OP_UnitsPerPallet);
		}
		#endregion
	}
}

