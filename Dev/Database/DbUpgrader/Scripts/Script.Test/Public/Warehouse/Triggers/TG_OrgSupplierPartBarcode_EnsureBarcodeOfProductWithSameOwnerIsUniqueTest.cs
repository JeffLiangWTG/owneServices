using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_OrgSupplierPartBarcode_EnsureBarcodeOfProductWithSameOwnerIsUnique))]
	class TG_OrgSupplierPartBarcode_EnsureBarcodeOfProductWithSameOwnerIsUniqueTest : DBCreateTriggerScriptTest
	{
		#region Insert

		#region TestBarcodeOfProductWithSameOwnerIsUnique_UniqueBarcode_Insert_DoesNotThrownException

		public void TestBarcodeOfProductWithSameOwnerIsUnique_UniqueBarcode_Insert_DoesNotThrownException()
		{
			var sql = new SqlQueryBuilder();
			var owner = new OrgHeader("OWNER").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P0").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner, product, "OWN").AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode123", product).AppendInsertAndReturnObject(sql);

			var newProduct = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner, newProduct, "OWN").AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var insertSql = new SqlQueryBuilder();
			new OrgSupplierPartBarcode("barcode456", newProduct).AppendInsertAndReturnObject(insertSql);
			AssertNoExceptionThrown(() => TestConnection.ExecuteNonQuery(insertSql.ToStringWithNewLineBetweenAppends()));
		}

		#endregion

		#region TestBarcodeOfProductWithSameOwnerIsUnique_UniqueBarcode_Insert_TwoProducts_SameProductCode_DoesNotThrownException

		public void TestBarcodeOfProductWithSameOwnerIsUnique_UniqueBarcode_Insert_TwoProducts_SameProductCode_DoesNotThrownException()
		{
			var sql = new SqlQueryBuilder();
			var owner = new OrgHeader("OWNER").AppendInsertAndReturnObject(sql);
			var owner2 = new OrgHeader("OWNER2").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P0").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner, product, "OWN").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner2, product, "SUP").AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode123", product).AppendInsertAndReturnObject(sql);

			var newProduct = new OrgSupplierPart("P0").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner, newProduct, "OWN").AppendInsertAndReturnObject(sql);

			try
			{
				DisableTriggers();
				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}
			finally
			{
				EnableTriggers();
			}

			var insertSql = new SqlQueryBuilder();
			new OrgSupplierPartBarcode("barcode456", newProduct).AppendInsertAndReturnObject(insertSql);

			AssertNoExceptionThrown(() => TestConnection.ExecuteNonQuery(insertSql.ToStringWithNewLineBetweenAppends()));
		}

		#endregion

		#region TestBarcodeOfProductWithSameOwnerIsUnique_RelationshipIsNotOwner_Insert_DoesNotThrownException

		public void TestBarcodeOfProductWithSameOwnerIsUnique_RelationshipIsNotOwner_Insert_DoesNotThrownException_InsertSupplier()
		{
			AssertBarcodeOfProductWithSameOwnerIsUnique_RelationshipIsNotOwner_Insert_DoesNotThrownException("OWN", "SUP");
		}

		public void TestBarcodeOfProductWithSameOwnerIsUnique_RelationshipIsNotOwner_Insert_DoesNotThrownException_InsertOwner()
		{
			AssertBarcodeOfProductWithSameOwnerIsUnique_RelationshipIsNotOwner_Insert_DoesNotThrownException("SUP", "OWN");
		}

		void AssertBarcodeOfProductWithSameOwnerIsUnique_RelationshipIsNotOwner_Insert_DoesNotThrownException(string relation1, string relation2)
		{
			var sql = new SqlQueryBuilder();
			var owner = new OrgHeader("OWNER").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P0").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner, product, relation1).AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode123", product).AppendInsertAndReturnObject(sql);

			var newProduct = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner, newProduct, relation2).AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var insertSql = new SqlQueryBuilder();
			new OrgSupplierPartBarcode("barcode123", newProduct).AppendInsertAndReturnObject(insertSql);
			AssertNoExceptionThrown(() => TestConnection.ExecuteNonQuery(insertSql.ToStringWithNewLineBetweenAppends()));
		}

		#endregion

		#region TestBarcodeOfProductWithSameOwnerIsUnique_DifferentOwners_Insert_DoesNotThrownException

		public void TestBarcodeOfProductWithSameOwnerIsUnique_DifferentOwners_Insert_DoesNotThrownException()
		{
			var sql = new SqlQueryBuilder();
			var owner = new OrgHeader("OWNER").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P0").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner, product, "OWN").AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode123", product).AppendInsertAndReturnObject(sql);

			var newOwner = new OrgHeader("OWNER1").AppendInsertAndReturnObject(sql);
			var newProduct = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(newOwner, newProduct, "OWN").AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var insertSql = new SqlQueryBuilder();
			new OrgSupplierPartBarcode("barcode123", newProduct).AppendInsertAndReturnObject(insertSql);
			AssertNoExceptionThrown(() => TestConnection.ExecuteNonQuery(insertSql.ToStringWithNewLineBetweenAppends()));
		}

		#endregion

		#region TestBarcodeOfProductWithSameOwnerIsUnique_DuplicateBarcode_Insert_ProductIsNotActive_DoesNotThrownException

		public void TestBarcodeOfProductWithSameOwnerIsUnique_DuplicateBarcode_Insert_ProductIsNotActive_DoesNotThrownException()
		{
			var sql = new SqlQueryBuilder();
			var owner = new OrgHeader("OWNER").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P0") { OP_IsActive = false }.AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner, product, "OWN").AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode123", product).AppendInsertAndReturnObject(sql);

			var newProduct = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner, newProduct, "OWN").AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var insertSql = new SqlQueryBuilder();
			new OrgSupplierPartBarcode("barcode123", newProduct).AppendInsertAndReturnObject(insertSql);
			AssertNoExceptionThrown(() => TestConnection.ExecuteNonQuery(insertSql.ToStringWithNewLineBetweenAppends()));
		}

		#endregion

		#region TestBarcodeOfProductWithSameOwnerIsUnique_DuplicateBarcode_Insert

		public void TestBarcodeOfProductWithSameOwnerIsUnique_DuplicateBarcode_Insert_OwnerSameAsOwner()
		{
			AssertBarcodeOfProductWithSameOwnerIsUnique_DuplicateBarcode_Insert("OWN", "OWN");
		}

		public void TestBarcodeOfProductWithSameOwnerIsUnique_DuplicateBarcode_Insert_BothSameAsOwner()
		{
			AssertBarcodeOfProductWithSameOwnerIsUnique_DuplicateBarcode_Insert("OWN", "BTH");
		}

		public void TestBarcodeOfProductWithSameOwnerIsUnique_DuplicateBarcode_Insert_OwnerSameAsBoth()
		{
			AssertBarcodeOfProductWithSameOwnerIsUnique_DuplicateBarcode_Insert("BTH", "OWN");
		}

		public void TestBarcodeOfProductWithSameOwnerIsUnique_DuplicateBarcode_Insert_BothSameAsBoth()
		{
			AssertBarcodeOfProductWithSameOwnerIsUnique_DuplicateBarcode_Insert("BTH", "BTH");
		}

		void AssertBarcodeOfProductWithSameOwnerIsUnique_DuplicateBarcode_Insert(string relation1, string relation2)
		{
			var sql = new SqlQueryBuilder();
			var owner = new OrgHeader("OWNER").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P0").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner, product, relation1).AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode123", product).AppendInsertAndReturnObject(sql);

			var newProduct = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner, newProduct, relation2).AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var insertSql = new SqlQueryBuilder();
			new OrgSupplierPartBarcode("barcode123", newProduct).AppendInsertAndReturnObject(insertSql);
			AssertExceptionThrown(typeof(SqlException), "The products with a same owner/code are not allowed to have the same barcode.", () => TestConnection.ExecuteNonQuery(insertSql.ToStringWithNewLineBetweenAppends()), true);
		}

		#endregion

		#region TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Insert

		public void TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Insert()
		{
			var sql = new SqlQueryBuilder();
			var owner = new OrgHeader("OWNER").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P0").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner, product, "OWN").AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var insertSql = new SqlQueryBuilder();
			new OrgSupplierPartBarcode("P0", product).AppendInsertAndReturnObject(insertSql);

			AssertExceptionThrown("Should throw exception", typeof(SqlException), "The products with a same owner/code are not allowed to have the same barcode.",
				() => TestConnection.ExecuteNonQuery(insertSql.ToStringWithNewLineBetweenAppends()), true);
		}

		#endregion

		#region TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Insert_TwoProducts

		public void TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Insert_TwoProducts_OwnerSameAsOwner()
		{
			TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Insert_TwoProducts("OWN", "OWN");
		}

		public void TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Insert_TwoProducts_BothSameAsOwner()
		{
			TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Insert_TwoProducts("OWN", "BTH");
		}

		public void TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Insert_TwoProducts_OwnerSameAsBoth()
		{
			TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Insert_TwoProducts("BTH", "OWN");
		}

		public void TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Insert_TwoProducts_BothSameAsBoth()
		{
			TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Insert_TwoProducts("BTH", "BTH");
		}

		public void TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Insert_TwoProducts_SupplierSameAsBoth()
		{
			TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Insert_TwoProducts("SUP", "BTH");
		}

		public void TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Insert_TwoProducts_BothSameAsSupplier()
		{
			TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Insert_TwoProducts("BTH", "SUP");
		}

		public void TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Insert_TwoProducts_SupplierSameAsOwner()
		{
			TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Insert_TwoProducts("SUP", "OWN");
		}

		public void TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Insert_TwoProducts_OwnerSameAsSupplier()
		{
			TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Insert_TwoProducts("OWN", "SUP");
		}

		public void TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Insert_TwoProducts_SupplierSameAsSupplier()
		{
			TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Insert_TwoProducts("SUP", "SUP");
		}

		void TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Insert_TwoProducts(string relation1, string relation2)
		{
			var sql = new SqlQueryBuilder();
			var owner = new OrgHeader("OWNER").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("barcode456").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner, product, relation1).AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode123", product).AppendInsertAndReturnObject(sql);

			var newProduct = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner, newProduct, relation2).AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var insertSql = new SqlQueryBuilder();
			new OrgSupplierPartBarcode("barcode456", newProduct).AppendInsertAndReturnObject(insertSql);

			if (relation1 != "SUP" && relation2 != "SUP")
			{
				AssertExceptionThrown("Should throw exception", typeof(SqlException), "The products with a same owner/code are not allowed to have the same barcode.",
					() => TestConnection.ExecuteNonQuery(insertSql.ToStringWithNewLineBetweenAppends()), true);
			}
			else
			{
				AssertNoExceptionThrown("Should not throw exception as one of relationship is supplier", () => TestConnection.ExecuteNonQuery(insertSql.ToStringWithNewLineBetweenAppends()));
			}
		}

		#endregion

		#region TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Insert_HasNoRelatedOrg

		public void TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Insert_HasNoRelatedOrg()
		{
			var sql = new SqlQueryBuilder();
			var product = new OrgSupplierPart("P0").AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var insertSql = new SqlQueryBuilder();
			new OrgSupplierPartBarcode("P0", product).AppendInsertAndReturnObject(insertSql);

			AssertNoExceptionThrown("Should not throw exception as products are not have related organisations", () => TestConnection.ExecuteNonQuery(insertSql.ToString()));
		}

		#endregion

		#region TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Insert_HasNoRelatedOrg_TwoProducts

		public void TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Insert_HasNoRelatedOrg_TwoProducts()
		{
			var sql = new SqlQueryBuilder();
			var owner = new OrgHeader("OWNER").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P0").AppendInsertAndReturnObject(sql);

			var newProduct = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			var barcode2 = new OrgSupplierPartBarcode("barcode456", newProduct).AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var insertSql = new SqlQueryBuilder();
			new OrgSupplierPartBarcode("P1", product).AppendInsertAndReturnObject(insertSql);

			AssertNoExceptionThrown("Should not throw exception as product has no owner", () => TestConnection.ExecuteNonQuery(insertSql.ToStringWithNewLineBetweenAppends()));
		}

		#endregion

		#endregion

		#region Update

		#region TestBarcodeOfProductWithSameOwnerIsUnique_UniqueBarcode_Update_DoesNotThrownException

		public void TestBarcodeOfProductWithSameOwnerIsUnique_UniqueBarcode_Update_DoesNotThrownException()
		{
			var sql = new SqlQueryBuilder();
			var owner = new OrgHeader("OWNER").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P0").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner, product, "OWN").AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode123", product).AppendInsertAndReturnObject(sql);

			var newProduct = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner, newProduct, "OWN").AppendInsertAndReturnObject(sql);
			var barcode = new OrgSupplierPartBarcode("barcode456", newProduct).AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertNoExceptionThrown(
				() => OrgSupplierPartBarcode
				.UpdateWhere(barcode.PK)
				.Set(b => b.PH_Barcode, "barcode789").Post(TestConnection));
		}

		#endregion

		#region TestBarcodeOfProductWithSameOwnerIsUnique_UniqueBarcode_Update_SameProductCode_DoesNotThrownException

		public void TestBarcodeOfProductWithSameOwnerIsUnique_UniqueBarcode_Update_SameProductCode()
		{
			var sql = new SqlQueryBuilder();
			var owner = new OrgHeader("OWNER").AppendInsertAndReturnObject(sql);
			var owner2 = new OrgHeader("OWNEr2").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P0").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner, product, "OWN").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner2, product, "SUP").AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode123", product).AppendInsertAndReturnObject(sql);

			var newProduct = new OrgSupplierPart("P0").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner, newProduct, "OWN").AppendInsertAndReturnObject(sql);
			var barcode = new OrgSupplierPartBarcode("barcode456", newProduct).AppendInsertAndReturnObject(sql);

			try
			{
				DisableTriggers();
				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}
			finally
			{
				EnableTriggers();
			}

			AssertNoExceptionThrown(
				() => OrgSupplierPartBarcode
				.UpdateWhere(barcode.PK)
				.Set(b => b.PH_Barcode, "barcode789").Post(TestConnection));
		}

		#endregion

		#region TestBarcodeOfProductWithSameOwnerIsUnique_RelationIsNotOwner_Update_DoesNotThrownException

		public void TestBarcodeOfProductWithSameOwnerIsUnique_RelationshipIsNotOwner_Update_DoesNotThrownException_InsertSupplier()
		{
			AssertBarcodeOfProductWithSameOwnerIsUnique_RelationshipIsNotOwner_Update_DoesNotThrownException("OWN", "SUP");
		}

		public void TestBarcodeOfProductWithSameOwnerIsUnique_RelationshipIsNotOwner_Update_DoesNotThrownException_InsertOwner()
		{
			AssertBarcodeOfProductWithSameOwnerIsUnique_RelationshipIsNotOwner_Update_DoesNotThrownException("SUP", "OWN");
		}

		void AssertBarcodeOfProductWithSameOwnerIsUnique_RelationshipIsNotOwner_Update_DoesNotThrownException(string relation1, string relation2)
		{
			var sql = new SqlQueryBuilder();
			var owner = new OrgHeader("OWNER").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P0").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner, product, relation1).AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode123", product).AppendInsertAndReturnObject(sql);

			var newProduct = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner, newProduct, relation2).AppendInsertAndReturnObject(sql);
			var barcode = new OrgSupplierPartBarcode("barcode456", newProduct).AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertNoExceptionThrown(
				() => OrgSupplierPartBarcode
				.UpdateWhere(barcode.PK)
				.Set(b => b.PH_Barcode, "barcode123").Post(TestConnection));
		}

		#endregion

		#region TestBarcodeOfProductWithSameOwnerIsUnique_DifferentOwners_Update_DoesNotThrownException

		public void TestBarcodeOfProductWithSameOwnerIsUnique_DifferentOwners_Update_DoesNotThrownException()
		{
			var sql = new SqlQueryBuilder();
			var owner = new OrgHeader("OWNER").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P0").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner, product, "OWN").AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode123", product).AppendInsertAndReturnObject(sql);

			var newOwner = new OrgHeader("OWNER1").AppendInsertAndReturnObject(sql);
			var newProduct = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(newOwner, newProduct, "OWN").AppendInsertAndReturnObject(sql);
			var barcode = new OrgSupplierPartBarcode("barcode456", newProduct).AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertNoExceptionThrown(
				() => OrgSupplierPartBarcode
				.UpdateWhere(barcode.PK)
				.Set(b => b.PH_Barcode, "barcode123").Post(TestConnection));
		}

		#endregion

		#region TestBarcodeOfProductWithSameOwnerIsUnique_DuplicateBarcode_Update_ProductIsNotActive_DoesNotThrownException

		public void TestBarcodeOfProductWithSameOwnerIsUnique_DuplicateBarcode_Update_ProductIsNotActive_DoesNotThrownException()
		{
			var sql = new SqlQueryBuilder();
			var owner = new OrgHeader("OWNER").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P0") { OP_IsActive = false }.AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner, product, "OWN").AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode123", product).AppendInsertAndReturnObject(sql);

			var newProduct = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner, newProduct, "OWN").AppendInsertAndReturnObject(sql);
			var barcode = new OrgSupplierPartBarcode("barcode456", newProduct).AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertNoExceptionThrown(
				() => OrgSupplierPartBarcode
				.UpdateWhere(barcode.PK)
				.Set(b => b.PH_Barcode, "barcode123").Post(TestConnection));
		}

		#endregion

		#region TestBarcodeOfProductWithSameOwnerIsUnique_DuplicateBarcode_Update

		public void TestBarcodeOfProductWithSameOwnerIsUnique_DuplicateBarcode_Update_OwnerSameAsOwner()
		{
			AssertBarcodeOfProductWithSameOwnerIsUnique_DuplicateBarcode_Update("OWN", "OWN");
		}

		public void TestBarcodeOfProductWithSameOwnerIsUnique_DuplicateBarcode_Update_BothSameAsOwner()
		{
			AssertBarcodeOfProductWithSameOwnerIsUnique_DuplicateBarcode_Update("OWN", "BTH");
		}

		public void TestBarcodeOfProductWithSameOwnerIsUnique_DuplicateBarcode_Update_OwnerSameAsBoth()
		{
			AssertBarcodeOfProductWithSameOwnerIsUnique_DuplicateBarcode_Update("BTH", "OWN");
		}

		public void TestBarcodeOfProductWithSameOwnerIsUnique_DuplicateBarcode_Update_BothSameAsBoth()
		{
			AssertBarcodeOfProductWithSameOwnerIsUnique_DuplicateBarcode_Update("BTH", "BTH");
		}

		void AssertBarcodeOfProductWithSameOwnerIsUnique_DuplicateBarcode_Update(string relation1, string relation2)
		{
			var sql = new SqlQueryBuilder();
			var owner = new OrgHeader("OWNER").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P0").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner, product, relation1).AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode123", product).AppendInsertAndReturnObject(sql);

			var newProduct = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner, newProduct, relation2).AppendInsertAndReturnObject(sql);
			var barcode = new OrgSupplierPartBarcode("barcode456", newProduct).AppendInsertAndReturnObject(sql);
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertExceptionThrown(typeof(SqlException), "The products with a same owner/code are not allowed to have the same barcode.",
				() => OrgSupplierPartBarcode
				.UpdateWhere(barcode.PK)
				.Set(b => b.PH_Barcode, "barcode123").Post(TestConnection), true);
		}

		#endregion

		#region TestBarcodeOfProductWithSameOwnerIsUnique_SupplierPart_Update

		public void TestBarcodeOfProductWithSameOwnerIsUnique_SupplierPart_Update_OwnerSameAsOwner()
		{
			AssertBarcodeOfProductWithSameOwnerIsUnique_SupplierPart_Update("OWN", "OWN");
		}

		public void TestBarcodeOfProductWithSameOwnerIsUnique_SupplierPart_Update_BothSameAsOwner()
		{
			AssertBarcodeOfProductWithSameOwnerIsUnique_SupplierPart_Update("OWN", "BTH");
		}

		public void TestBarcodeOfProductWithSameOwnerIsUnique_SupplierPart_Update_OwnerSameAsBoth()
		{
			AssertBarcodeOfProductWithSameOwnerIsUnique_SupplierPart_Update("BTH", "OWN");
		}

		public void TestBarcodeOfProductWithSameOwnerIsUnique_SupplierPart_Update_BothSameAsBoth()
		{
			AssertBarcodeOfProductWithSameOwnerIsUnique_SupplierPart_Update("BTH", "BTH");
		}

		void AssertBarcodeOfProductWithSameOwnerIsUnique_SupplierPart_Update(string relation1, string relation2)
		{
			var sql = new SqlQueryBuilder();
			var owner = new OrgHeader("OWNER").AppendInsertAndReturnObject(sql);
			var product1 = new OrgSupplierPart("P0").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner, product1, relation1) { OU_ClientUQ = "UNT" }.AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode123", product1) { PH_F3_NKPackType = "UNT" }.AppendInsertAndReturnObject(sql);

			var product2 = new OrgSupplierPart("P2").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner, product2, relation2) { OU_ClientUQ = "UNT" }.AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode456", product2) { PH_F3_NKPackType = "PKG" }.AppendInsertAndReturnObject(sql);

			var product3 = new OrgSupplierPart("P3").AppendInsertAndReturnObject(sql);
			var product3Barcode = new OrgSupplierPartBarcode("barcode123", product3) { PH_F3_NKPackType = "CAS" }.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertExceptionThrown(typeof(SqlException), "The products with a same owner/code are not allowed to have the same barcode.",
				() => OrgSupplierPartBarcode
				.UpdateWhere(product3Barcode.PK)
				.Set(b => b.PH_OP, product2).Post(TestConnection), true);
		}

		#endregion

		#region TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Update

		public void TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Update()
		{
			var sql = new SqlQueryBuilder();
			var owner = new OrgHeader("OWNER").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P0").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner, product, "OWN").AppendInsertAndReturnObject(sql);
			var barcode = new OrgSupplierPartBarcode("barcode123", product).AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertExceptionThrown(typeof(SqlException), "The products with a same owner/code are not allowed to have the same barcode.",
				() => OrgSupplierPartBarcode
				.UpdateWhere(barcode.PK)
				.Set(b => b.PH_Barcode, "P0").Post(TestConnection), true);
		}

		#endregion

		#region TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Update_HasNoRelatedOrg

		public void TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Update_HasNoRelatedOrg()
		{
			var sql = new SqlQueryBuilder();
			var product = new OrgSupplierPart("P0").AppendInsertAndReturnObject(sql);
			var barcode = new OrgSupplierPartBarcode("barcode123", product).AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertNoExceptionThrown("Should not throw exception as products are not have related organisations",
				() => OrgSupplierPartBarcode
				.UpdateWhere(barcode.PK)
				.Set(b => b.PH_Barcode, "P0").Post(TestConnection));
		}

		#endregion

		#region TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Update_HasNoRelatedOrg_TwoProducts

		public void TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Update_HasNoRelatedOrg_TwoProducts()
		{
			var sql = new SqlQueryBuilder();
			var owner = new OrgHeader("OWNER").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P0").AppendInsertAndReturnObject(sql);
			var barcode = new OrgSupplierPartBarcode("barcode123", product).AppendInsertAndReturnObject(sql);

			var newProduct = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			var barcode2 = new OrgSupplierPartBarcode("barcode456", newProduct).AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertNoExceptionThrown("Should not throw exception as product has no owner",
				() => OrgSupplierPartBarcode
				.UpdateWhere(barcode.PK)
				.Set(b => b.PH_Barcode, "P1").Post(TestConnection));
		}

		#endregion

		#region TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Update_AnotherProduct

		public void TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Update_TwoProducts_OwnerSameAsOwner()
		{
			TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Update_TwoProducts("OWN", "OWN", expectHasException: true);
		}

		public void TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Update_TwoProducts_BothSameAsOwner()
		{
			TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Update_TwoProducts("OWN", "BTH", expectHasException: true);
		}

		public void TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Update_TwoProducts_OwnerSameAsBoth()
		{
			TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Update_TwoProducts("BTH", "OWN", expectHasException: true);
		}

		public void TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Update_TwoProducts_BothSameAsBoth()
		{
			TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Update_TwoProducts("BTH", "BTH", expectHasException: true);
		}

		public void TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Update_TwoProducts_SupplierSameAsBoth()
		{
			TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Update_TwoProducts("SUP", "BTH");
		}

		public void TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Update_TwoProducts_BothSameAsSupplier()
		{
			TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Update_TwoProducts("BTH", "SUP");
		}

		public void TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Update_TwoProducts_SupplierSameAsOwner()
		{
			TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Update_TwoProducts("SUP", "OWN");
		}

		public void TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Update_TwoProducts_OwnerSameAsSupplier()
		{
			TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Update_TwoProducts("OWN", "SUP");
		}

		public void TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Update_TwoProducts_SupplierSameAsSupplier()
		{
			TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Update_TwoProducts("SUP", "SUP");
		}

		void TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Update_TwoProducts(string relation1, string relation2, bool expectHasException = false)
		{
			var sql = new SqlQueryBuilder();
			var owner = new OrgHeader("OWNER").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("barcode456").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner, product, relation1).AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode123", product).AppendInsertAndReturnObject(sql);

			var newProduct = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner, newProduct, relation2).AppendInsertAndReturnObject(sql);
			var barcode = new OrgSupplierPartBarcode("barcode789", newProduct).AppendInsertAndReturnObject(sql);
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var updateSql = OrgSupplierPartBarcode
				.UpdateWhere(barcode.PK)
				.Set(b => b.PH_Barcode, "barcode456").AsSQL();

			if (expectHasException)
			{
				AssertExceptionThrown("Should throw exception", typeof(SqlException), "The products with a same owner/code are not allowed to have the same barcode.",
					() => TestConnection.ExecuteNonQuery(updateSql), true);
			}
			else
			{
				AssertNoExceptionThrown("Should not throw exception as one of relationship is supplier", () => TestConnection.ExecuteNonQuery(updateSql));
			}
		}

		#endregion

		#region TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Update_SupplierPart

		public void TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Update_SupplierPart()
		{
			var sql = new SqlQueryBuilder();
			var owner1 = new OrgHeader("OWNER1").AppendInsertAndReturnObject(sql);
			var owner2 = new OrgHeader("OWNER2").AppendInsertAndReturnObject(sql);
			var product1 = new OrgSupplierPart("barcode456").AppendInsertAndReturnObject(sql);
			var product2 = new OrgSupplierPart("P2").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner1, product1, "OWN") { OU_ClientUQ = "UNT" }.AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner2, product2, "OWN") { OU_ClientUQ = "UNT" }.AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode123", product1) { PH_F3_NKPackType = "UNT" }.AppendInsertAndReturnObject(sql);
			var product2Barcode = new OrgSupplierPartBarcode("barcode456", product2) { PH_F3_NKPackType = "PKG" }.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var updateSql = $@"
				UPDATE 
					dbo.OrgSupplierPartBarcode
				SET
					PH_OP = '{product1.PK}',
					PH_SystemLastEditTimeUtc = SYSUTCDATETIME(),
					PH_SystemLastEditUser = '~BP'
				WHERE
					PH_PK = '{product2Barcode.PK}'";

			AssertExceptionThrown("Should throw exception", typeof(SqlException), "The products with a same owner/code are not allowed to have the same barcode.",
				() => OrgSupplierPartBarcode
				.UpdateWhere(product2Barcode.PK)
				.Set(b => b.PH_OP, product1).Post(TestConnection), true);
		}
		#endregion

		#endregion

		void DisableTriggers()
		{
			TestConnection.ExecuteNonQuery(@"
DISABLE TRIGGER TG_OrgSupplierPart_DuplicateCheck ON OrgSupplierPart;
DISABLE TRIGGER TG_OrgPartRelation_DuplicateCheck ON OrgPartRelation;
");
		}

		void EnableTriggers()
		{
			TestConnection.ExecuteNonQuery(@"
ENABLE TRIGGER TG_OrgSupplierPart_DuplicateCheck ON OrgSupplierPart;
ENABLE TRIGGER TG_OrgPartRelation_DuplicateCheck ON OrgPartRelation;
");
		}
	}
}
