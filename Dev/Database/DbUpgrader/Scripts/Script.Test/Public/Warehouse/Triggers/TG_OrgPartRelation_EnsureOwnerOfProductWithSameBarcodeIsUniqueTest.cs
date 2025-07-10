using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_OrgPartRelation_EnsureOwnerOfProductWithSameBarcodeIsUnique))]
	class TG_OrgPartRelation_EnsureBarcodeOfProductWithSameOwnerIsUniqueTest : DBCreateTriggerScriptTest
	{
		#region Insert

		#region TestBarcodeOfProductWithSameOwnerIsUnique_DuplicateClientAndRelationshipIsNotOwner_Insert_DoesNotThrownException

		public void TestBarcodeOfProductWithSameOwnerIsUnique_DuplicateClientAndRelationshipIsNotOwner_Insert_DoesNotThrownException_SupplierSameAsOwner()
		{
			AssertBarcodeOfProductWithSameOwnerIsUnique_DuplicateClientAndRelationshipIsNotOwner_Insert_DoesNotThrownException("OWN", "SUP");
		}

		public void TestBarcodeOfProductWithSameOwnerIsUnique_DuplicateClientAndRelationshipIsNotOwner_Insert_DoesNotThrownException_OwnerSameAsSupplier()
		{
			AssertBarcodeOfProductWithSameOwnerIsUnique_DuplicateClientAndRelationshipIsNotOwner_Insert_DoesNotThrownException("SUP", "OWN");
		}

		public void TestBarcodeOfProductWithSameOwnerIsUnique_DuplicateClientAndRelationshipIsNotOwner_Insert_DoesNotThrownException_SupplierSameAsBoth()
		{
			AssertBarcodeOfProductWithSameOwnerIsUnique_DuplicateClientAndRelationshipIsNotOwner_Insert_DoesNotThrownException("BTH", "SUP");
		}

		public void TestBarcodeOfProductWithSameOwnerIsUnique_DuplicateClientAndRelationshipIsNotOwner_Insert_DoesNotThrownException_BothSameAsSupplier()
		{
			AssertBarcodeOfProductWithSameOwnerIsUnique_DuplicateClientAndRelationshipIsNotOwner_Insert_DoesNotThrownException("SUP", "BTH");
		}

		void AssertBarcodeOfProductWithSameOwnerIsUnique_DuplicateClientAndRelationshipIsNotOwner_Insert_DoesNotThrownException(string relation1, string relation2)
		{
			var sql = new SqlQueryBuilder();
			var owner = new OrgHeader("OWNER").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P0").AppendInsertAndReturnObject(sql);
			var relation = new OrgPartRelation(owner, product, relation1).AppendInsertAndReturnObject(sql);
			var barcode = new OrgSupplierPartBarcode("barcode123", product).AppendInsertAndReturnObject(sql);

			var newProduct = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			var barcode2 = new OrgSupplierPartBarcode("barcode123", newProduct).AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var insertSql = new SqlQueryBuilder();
			var relationTest = new OrgPartRelation(owner, newProduct, relation2).AppendInsertAndReturnObject(insertSql);
			AssertNoExceptionThrown(() => TestConnection.ExecuteNonQuery(insertSql.ToStringWithNewLineBetweenAppends()));
		}

		#endregion

		#region TestBarcodeOfProductWithSameOwnerIsUnique_UniqueClientAndRelationshipIsNotOwner_Insert_DoesNotThrownException

		public void TestBarcodeOfProductWithSameOwnerIsUnique_UniqueClientAndDuplicateRelationship_Insert_DoesNotThrownException_OwnerSameAsOwner()
		{
			AssertBarcodeOfProductWithSameOwnerIsUnique_UniqueClientAndDuplicateRelationship_Insert_DoesNotThrownException("OWN", "OWN");
		}

		public void TestBarcodeOfProductWithSameOwnerIsUnique_UniqueClientAndDuplicateRelationship_Insert_DoesNotThrownException_BothSameAsOwner()
		{
			AssertBarcodeOfProductWithSameOwnerIsUnique_UniqueClientAndDuplicateRelationship_Insert_DoesNotThrownException("OWN", "BTH");
		}

		public void TestBarcodeOfProductWithSameOwnerIsUnique_UniqueClientAndDuplicateRelationship_Insert_DoesNotThrownException_OwnerSameAsBoth()
		{
			AssertBarcodeOfProductWithSameOwnerIsUnique_UniqueClientAndDuplicateRelationship_Insert_DoesNotThrownException("BTH", "OWN");
		}

		public void TestBarcodeOfProductWithSameOwnerIsUnique_UniqueClientAndDuplicateRelationship_Insert_DoesNotThrownException_BothSameAsBoth()
		{
			AssertBarcodeOfProductWithSameOwnerIsUnique_UniqueClientAndDuplicateRelationship_Insert_DoesNotThrownException("BTH", "BTH");
		}

		void AssertBarcodeOfProductWithSameOwnerIsUnique_UniqueClientAndDuplicateRelationship_Insert_DoesNotThrownException(string relation1, string relation2)
		{
			var sql = new SqlQueryBuilder();
			var owner = new OrgHeader("OWNER").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P0").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner, product, relation1).AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode123", product).AppendInsertAndReturnObject(sql);

			var newProduct = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode123", newProduct).AppendInsertAndReturnObject(sql);
			var newOwner = new OrgHeader("OWNER1").AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var insertSql = new SqlQueryBuilder();

			new OrgPartRelation(newOwner, newProduct, relation2).AppendInsertAndReturnObject(insertSql);
			AssertNoExceptionThrown(() => TestConnection.ExecuteNonQuery(insertSql.ToStringWithNewLineBetweenAppends()));
		}

		#endregion

		#region TestBarcodeOfProductWithSameOwnerIsUnique_DuplicateClientAndRelationship_UniqueBarcode_Insert_DoesNotThrownException

		public void TestBarcodeOfProductWithSameOwnerIsUnique_DuplicateClientAndRelationship_UniqueBarcode_Insert_DoesNotThrownException_OwnerSameAsOwner()
		{
			AssertBarcodeOfProductWithSameOwnerIsUnique_DuplicateClientAndRelationship_UniqueBarcode_Insert_DoesNotThrownException("OWN", "OWN");
		}

		public void TestBarcodeOfProductWithSameOwnerIsUnique_DuplicateClientAndRelationship_UniqueBarcode_Insert_DoesNotThrownException_BothSameAsOwner()
		{
			AssertBarcodeOfProductWithSameOwnerIsUnique_DuplicateClientAndRelationship_UniqueBarcode_Insert_DoesNotThrownException("OWN", "BTH");
		}

		public void TestBarcodeOfProductWithSameOwnerIsUnique_DuplicateClientAndRelationship_UniqueBarcode_Insert_DoesNotThrownException_OwnerSameAsBoth()
		{
			AssertBarcodeOfProductWithSameOwnerIsUnique_DuplicateClientAndRelationship_UniqueBarcode_Insert_DoesNotThrownException("BTH", "OWN");
		}

		public void TestBarcodeOfProductWithSameOwnerIsUnique_DuplicateClientAndRelationship_UniqueBarcode_Insert_DoesNotThrownException_BothSameAsBoth()
		{
			AssertBarcodeOfProductWithSameOwnerIsUnique_DuplicateClientAndRelationship_UniqueBarcode_Insert_DoesNotThrownException("BTH", "BTH");
		}

		void AssertBarcodeOfProductWithSameOwnerIsUnique_DuplicateClientAndRelationship_UniqueBarcode_Insert_DoesNotThrownException(string relation1, string relation2)
		{
			var sql = new SqlQueryBuilder();
			var owner = new OrgHeader("OWNER").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P0").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner, product, relation1).AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode123", product).AppendInsertAndReturnObject(sql);

			var newProduct = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode456", newProduct).AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var insertSql = new SqlQueryBuilder();
			new OrgPartRelation(owner, newProduct, relation2).AppendInsertAndReturnObject(insertSql);
			AssertNoExceptionThrown(() => TestConnection.ExecuteNonQuery(insertSql.ToStringWithNewLineBetweenAppends()));
		}

		#endregion

		#region TestBarcodeOfProductWithSameOwnerIsUnique_DuplicateClientAndRelationship_UniqueBarcode_SameProductCode_Insert_DoesNotThrownException

		public void TestBarcodeOfProductWithSameOwnerIsUnique_DuplicateClientAndRelationship_UniqueBarcode_SameProductCode_Insert()
		{
			var sql = new SqlQueryBuilder();
			var owner = new OrgHeader("OWNER").AppendInsertAndReturnObject(sql);
			var owner2 = new OrgHeader("OWNER2").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner, product, "OWN").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner2, product, "SUP").AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode123", product).AppendInsertAndReturnObject(sql);

			var newProduct = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode456", newProduct).AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var insertSql = new SqlQueryBuilder();
			new OrgPartRelation(owner, newProduct, "OWN").AppendInsertAndReturnObject(insertSql);
			AssertExceptionThrown(
				typeof(SqlException),
				"Relationship between product (OP_PartNum) and owner (OU_OH) must be unique for active (OP_IsActive) products.",
				() => TestConnection.ExecuteNonQuery(insertSql.ToStringWithNewLineBetweenAppends()),
				true
			);
		}

		#endregion

		#region TestBarcodeOfProductWithSameOwnerIsUnique_DuplicateClientAndRelationship_Insert_ProductIsNotActive_DoesNotThrownException

		public void TestBarcodeOfProductWithSameOwnerIsUnique_DuplicateClientAndRelationship_Insert_ProductIsNotActive_DoesNotThrownException()
		{
			var sql = new SqlQueryBuilder();
			var owner = new OrgHeader("OWNER").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P0") { OP_IsActive = false }.AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner, product, "OWN").AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode123", product).AppendInsertAndReturnObject(sql);

			var newProduct = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode123", newProduct).AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var insertSql = new SqlQueryBuilder();
			new OrgPartRelation(owner, newProduct, "OWN").AppendInsertAndReturnObject(insertSql);
			AssertNoExceptionThrown(() => TestConnection.ExecuteNonQuery(insertSql.ToStringWithNewLineBetweenAppends()));
		}

		#endregion

		#region TestBarcodeOfProductWithSameOwnerIsUnique_DuplicateClientAndRelationship_Insert

		public void TestBarcodeOfProductWithSameOwnerIsUnique_DuplicateClientAndRelationship_Insert_OwnerSameAsOwner()
		{
			AssertBarcodeOfProductWithSameOwnerIsUnique_DuplicateClientAndRelationship_Insert("OWN", "OWN");
		}

		public void TestBarcodeOfProductWithSameOwnerIsUnique_DuplicateClientAndRelationship_Insert_BothSameAsOwner()
		{
			AssertBarcodeOfProductWithSameOwnerIsUnique_DuplicateClientAndRelationship_Insert("OWN", "BTH");
		}

		public void TestBarcodeOfProductWithSameOwnerIsUnique_DuplicateClientAndRelationship_Insert_OwnerSameAsBoth()
		{
			AssertBarcodeOfProductWithSameOwnerIsUnique_DuplicateClientAndRelationship_Insert("BTH", "OWN");
		}

		public void TestBarcodeOfProductWithSameOwnerIsUnique_DuplicateClientAndRelationship_Insert_BothSameAsBoth()
		{
			AssertBarcodeOfProductWithSameOwnerIsUnique_DuplicateClientAndRelationship_Insert("BTH", "BTH");
		}

		void AssertBarcodeOfProductWithSameOwnerIsUnique_DuplicateClientAndRelationship_Insert(string relation1, string relation2)
		{
			var sql = new SqlQueryBuilder();
			var owner = new OrgHeader("OWNER").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P0").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner, product, relation1).AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode123", product).AppendInsertAndReturnObject(sql);

			var newProduct = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode123", newProduct).AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var insertSql = new SqlQueryBuilder();
			new OrgPartRelation(owner, newProduct, relation2).AppendInsertAndReturnObject(insertSql);
			AssertExceptionThrown(typeof(SqlException), "The products with a same owner/code are not allowed to have the same barcode.", () => TestConnection.ExecuteNonQuery(insertSql.ToStringWithNewLineBetweenAppends()), true);
		}

		#endregion

		#region TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Insert

		public void TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Insert_OwnerSameAsOwner()
		{
			TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Insert("OWN", "OWN", expectHasException: true);
		}

		public void TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Insert_BothSameAsOwner()
		{
			TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Insert("OWN", "BTH", expectHasException: true);
		}

		public void TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Insert_OwnerSameAsBoth()
		{
			TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Insert("BTH", "OWN", expectHasException: true);
		}

		public void TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Insert_BothSameAsBoth()
		{
			TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Insert("BTH", "BTH", expectHasException: true);
		}

		public void TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Insert_SupplierSameAsBoth()
		{
			TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Insert("SUP", "BTH");
		}

		public void TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Insert_BothSameAsSupplier()
		{
			TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Insert("BTH", "SUP");
		}

		public void TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Insert_SupplierSameAsOwner()
		{
			TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Insert("SUP", "OWN");
		}

		public void TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Insert_OwnerSameAsSupplier()
		{
			TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Insert("OWN", "SUP");
		}

		public void TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Insert_SupplierSameAsSupplier()
		{
			TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Insert("SUP", "SUP");
		}

		void TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Insert(string relation1, string relation2, bool expectHasException = false)
		{
			var sql = new SqlQueryBuilder();
			var owner = new OrgHeader("OWNER").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("barcode456").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner, product, relation1).AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode123", product).AppendInsertAndReturnObject(sql);

			var newProduct = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode456", newProduct).AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var insertSql = new SqlQueryBuilder();
			new OrgPartRelation(owner, newProduct, relation2).AppendInsertAndReturnObject(insertSql);

			if (expectHasException)
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

		#region TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Insert_ProductHasNoBarcodes

		public void TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Insert_ProductHasNoBarcodes_OwnerSameAsOwner()
		{
			TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Insert_ProductHasNoBarcodes("OWN", "OWN", expectHasException: true);
		}

		public void TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Insert_ProductHasNoBarcodes_BothSameAsOwner()
		{
			TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Insert_ProductHasNoBarcodes("OWN", "BTH", expectHasException: true);
		}

		public void TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Insert_ProductHasNoBarcodes_OwnerSameAsBoth()
		{
			TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Insert_ProductHasNoBarcodes("BTH", "OWN", expectHasException: true);
		}

		public void TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Insert_ProductHasNoBarcodes_BothSameAsBoth()
		{
			TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Insert_ProductHasNoBarcodes("BTH", "BTH", expectHasException: true);
		}

		public void TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Insert_ProductHasNoBarcodes_SupplierSameAsBoth()
		{
			TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Insert_ProductHasNoBarcodes("SUP", "BTH");
		}

		public void TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Insert_ProductHasNoBarcodes_BothSameAsSupplier()
		{
			TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Insert_ProductHasNoBarcodes("BTH", "SUP");
		}

		public void TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Insert_ProductHasNoBarcodes_SupplierSameAsOwner()
		{
			TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Insert_ProductHasNoBarcodes("SUP", "OWN");
		}

		public void TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Insert_ProductHasNoBarcodes_OwnerSameAsSupplier()
		{
			TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Insert_ProductHasNoBarcodes("OWN", "SUP");
		}

		public void TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Insert_ProductHasNoBarcodes_SupplierSameAsSupplier()
		{
			TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Insert_ProductHasNoBarcodes("SUP", "SUP");
		}

		void TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Insert_ProductHasNoBarcodes(string relation1, string relation2, bool expectHasException = false)
		{
			var sql = new SqlQueryBuilder();
			var owner = new OrgHeader("OWNER").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P0").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner, product, relation1).AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("P1", product).AppendInsertAndReturnObject(sql);

			var newProduct = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var insertSql = new SqlQueryBuilder();
			new OrgPartRelation(owner, newProduct, relation2).AppendInsertAndReturnObject(insertSql);

			if (expectHasException)
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

		#endregion

		#region Update

		#region TestBarcodeOfProductWithSameOwnerIsUnique_DuplicateClientAndRelationshipIsNotOwner_Update_DoesNotThrownException

		public void TestBarcodeOfProductWithSameOwnerIsUnique_DuplicateClientAndRelationshipIsNotOwner_Update_DoesNotThrownException_SupplierSameAsOwner()
		{
			TestBarcodeOfProductWithSameOwnerIsUnique_DuplicateClientAndRelationshipIsNotOwner_Update_DoesNotThrownException("OWN", "SUP");
		}

		public void TestBarcodeOfProductWithSameOwnerIsUnique_DuplicateClientAndRelationshipIsNotOwner_Update_DoesNotThrownException_OwnerSameAsSupplier()
		{
			TestBarcodeOfProductWithSameOwnerIsUnique_DuplicateClientAndRelationshipIsNotOwner_Update_DoesNotThrownException("SUP", "OWN");
		}

		public void TestBarcodeOfProductWithSameOwnerIsUnique_DuplicateClientAndRelationshipIsNotOwner_Update_DoesNotThrownException_SupplierSameAsBoth()
		{
			TestBarcodeOfProductWithSameOwnerIsUnique_DuplicateClientAndRelationshipIsNotOwner_Update_DoesNotThrownException("BTH", "SUP");
		}

		public void TestBarcodeOfProductWithSameOwnerIsUnique_DuplicateClientAndRelationshipIsNotOwner_Update_DoesNotThrownException_BothSameAsSupplier()
		{
			TestBarcodeOfProductWithSameOwnerIsUnique_DuplicateClientAndRelationshipIsNotOwner_Update_DoesNotThrownException("SUP", "BTH");
		}

		void TestBarcodeOfProductWithSameOwnerIsUnique_DuplicateClientAndRelationshipIsNotOwner_Update_DoesNotThrownException(string relation1, string relation2)
		{
			var sql = new SqlQueryBuilder();
			var owner = new OrgHeader("OWNER").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P0").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner, product, relation1).AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode123", product).AppendInsertAndReturnObject(sql);

			var newOwner = new OrgHeader("OWNER1").AppendInsertAndReturnObject(sql);
			var newProduct = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			var newRelation = new OrgPartRelation(newOwner, newProduct, relation2).AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode123", newProduct).AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertNoExceptionThrown(
				() => OrgPartRelation
				.UpdateWhere(newRelation.PK)
				.Set(r => r.OU_OH, owner).Post(TestConnection));
		}

		#endregion

		#region TestBarcodeOfProductWithSameOwnerIsUnique_DuplicateRelationship_Update_WithUniqueClient_DoesNotThrownException

		public void TestBarcodeOfProductWithSameOwnerIsUnique_DuplicateRelationship_Update_WithUniqueClient_DoesNotThrownException_OwnerSameAsOwner()
		{
			AssertBarcodeOfProductWithSameOwnerIsUnique_DuplicateRelationship_Update_WithUniqueClient_DoesNotThrownException("OWN", "OWN");
		}

		public void TestBarcodeOfProductWithSameOwnerIsUnique_DuplicateRelationship_Update_WithUniqueClient_DoesNotThrownException_BothSameAsOwner()
		{
			AssertBarcodeOfProductWithSameOwnerIsUnique_DuplicateRelationship_Update_WithUniqueClient_DoesNotThrownException("OWN", "BTH");
		}

		public void TestBarcodeOfProductWithSameOwnerIsUnique_DuplicateRelationship_Update_WithUniqueClient_DoesNotThrownException_OwnerSameAsBoth()
		{
			AssertBarcodeOfProductWithSameOwnerIsUnique_DuplicateRelationship_Update_WithUniqueClient_DoesNotThrownException("BTH", "OWN");
		}

		public void TestBarcodeOfProductWithSameOwnerIsUnique_DuplicateRelationship_Update_WithUniqueClient_DoesNotThrownException_BothSameAsBoth()
		{
			AssertBarcodeOfProductWithSameOwnerIsUnique_DuplicateRelationship_Update_WithUniqueClient_DoesNotThrownException("BTH", "BTH");
		}

		void AssertBarcodeOfProductWithSameOwnerIsUnique_DuplicateRelationship_Update_WithUniqueClient_DoesNotThrownException(string relation1, string relation2)
		{
			var sql = new SqlQueryBuilder();
			var owner = new OrgHeader("OWNER").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P0").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner, product, relation1).AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode123", product).AppendInsertAndReturnObject(sql);

			var newOwner = new OrgHeader("OWNER1").AppendInsertAndReturnObject(sql);
			var newProduct = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			var newRelation = new OrgPartRelation(newOwner, newProduct, "SUP").AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode123", newProduct).AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertNoExceptionThrown(
				() => OrgPartRelation
				.UpdateWhere(newRelation.PK)
				.Set(r => r.OU_Relationship, relation2).Post(TestConnection));
		}

		#endregion

		#region TestBarcodeOfProductWithSameOwnerIsUnique_UniqueRelationshipAndUniqueClient_Update_WithUniqueBarcode_DoesNotThrownException

		public void TestBarcodeOfProductWithSameOwnerIsUnique_UniqueRelationshipAndUniqueClient_Update_WithUniqueBarcode_DoesNotThrownException_OwnerSameAsOwner()
		{
			AssertBarcodeOfProductWithSameOwnerIsUnique_UniqueRelationshipAndUniqueClient_Update_WithUniqueBarcode_DoesNotThrownException("OWN", "OWN");
		}

		public void TestBarcodeOfProductWithSameOwnerIsUnique_UniqueRelationshipAndUniqueClient_Update_WithUniqueBarcode_DoesNotThrownException_BothSameAsOwner()
		{
			AssertBarcodeOfProductWithSameOwnerIsUnique_UniqueRelationshipAndUniqueClient_Update_WithUniqueBarcode_DoesNotThrownException("OWN", "BTH");
		}

		public void TestBarcodeOfProductWithSameOwnerIsUnique_UniqueRelationshipAndUniqueClient_Update_WithUniqueBarcode_DoesNotThrownException_OwnerSameAsBoth()
		{
			AssertBarcodeOfProductWithSameOwnerIsUnique_UniqueRelationshipAndUniqueClient_Update_WithUniqueBarcode_DoesNotThrownException("BTH", "OWN");
		}

		public void TestBarcodeOfProductWithSameOwnerIsUnique_UniqueRelationshipAndUniqueClient_Update_WithUniqueBarcode_DoesNotThrownException_BothSameAsBoth()
		{
			AssertBarcodeOfProductWithSameOwnerIsUnique_UniqueRelationshipAndUniqueClient_Update_WithUniqueBarcode_DoesNotThrownException("BTH", "BTH");
		}

		void AssertBarcodeOfProductWithSameOwnerIsUnique_UniqueRelationshipAndUniqueClient_Update_WithUniqueBarcode_DoesNotThrownException(string relation1, string relation2)
		{
			var sql = new SqlQueryBuilder();
			var owner = new OrgHeader("OWNER").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P0").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner, product, relation1).AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode123", product).AppendInsertAndReturnObject(sql);

			var newOwner = new OrgHeader("OWNER1").AppendInsertAndReturnObject(sql);
			var newProduct = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			var newRelation = new OrgPartRelation(newOwner, newProduct, "SUP").AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode456", newProduct).AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertNoExceptionThrown(
				() => OrgPartRelation
				.UpdateWhere(newRelation.PK)
				.Set(r => r.OU_OH, owner)
				.Set(r => r.OU_Relationship, relation2).Post(TestConnection));
		}

		#endregion

		#region TestBarcodeOfProductWithSameOwnerIsUnique_DuplicateClient_Update_ProductIsNotActive_DoesNotThrownException

		public void TestBarcodeOfProductWithSameOwnerIsUnique_DuplicateClient_Update_ProductIsNotActive_DoesNotThrownException()
		{
			var sql = new SqlQueryBuilder();
			var owner = new OrgHeader("OWNER").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P0").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner, product, "OWN").AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode123", product).AppendInsertAndReturnObject(sql);

			var newOwner = new OrgHeader("OWNER1").AppendInsertAndReturnObject(sql);
			var newProduct = new OrgSupplierPart("P1") { OP_IsActive = false }.AppendInsertAndReturnObject(sql);
			var newRelation = new OrgPartRelation(newOwner, newProduct, "OWN").AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode123", newProduct).AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertNoExceptionThrown(
				() => OrgPartRelation
				.UpdateWhere(newRelation.PK)
				.Set(r => r.OU_OH, owner).Post(TestConnection));
		}

		#endregion

		#region TestBarcodeOfProductWithSameOwnerIsUnique_DuplicateClient_Update

		public void TestBarcodeOfProductWithSameOwnerIsUnique_DuplicateClient_Update_OwnerSameAsOwner()
		{
			AssertBarcodeOfProductWithSameOwnerIsUnique_DuplicateClient_Update("OWN", "OWN");
		}

		public void TestBarcodeOfProductWithSameOwnerIsUnique_DuplicateClient_Update_BothSameAsOwner()
		{
			AssertBarcodeOfProductWithSameOwnerIsUnique_DuplicateClient_Update("OWN", "BTH");
		}

		public void TestBarcodeOfProductWithSameOwnerIsUnique_DuplicateClient_Update_OwnerSameAsBoth()
		{
			AssertBarcodeOfProductWithSameOwnerIsUnique_DuplicateClient_Update("BTH", "OWN");
		}

		public void TestBarcodeOfProductWithSameOwnerIsUnique_DuplicateClient_Update_BothSameAsBoth()
		{
			AssertBarcodeOfProductWithSameOwnerIsUnique_DuplicateClient_Update("BTH", "BTH");
		}

		void AssertBarcodeOfProductWithSameOwnerIsUnique_DuplicateClient_Update(string relation1, string relation2)
		{
			var sql = new SqlQueryBuilder();
			var owner = new OrgHeader("OWNER").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P0").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner, product, relation1).AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode123", product).AppendInsertAndReturnObject(sql);

			var newOwner = new OrgHeader("OWNER1").AppendInsertAndReturnObject(sql);
			var newProduct = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			var newRelation = new OrgPartRelation(newOwner, newProduct, relation2).AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode123", newProduct).AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertExceptionThrown(typeof(SqlException), "The products with a same owner/code are not allowed to have the same barcode.",
				() => OrgPartRelation
				.UpdateWhere(newRelation.PK)
				.Set(r => r.OU_OH, owner).Post(TestConnection), true);
		}

		#endregion

		#region TestBarcodeOfProductWithSameOwnerIsUnique_DuplicateRelationship_Update

		public void TestBarcodeOfProductWithSameOwnerIsUnique_DuplicateRelationship_Update_OwnerSameAsOwner()
		{
			AssertBarcodeOfProductWithSameOwnerIsUnique_DuplicateRelationship_Update("OWN", "OWN");
		}

		public void TestBarcodeOfProductWithSameOwnerIsUnique_DuplicateRelationship_Update_BothSameAsOwner()
		{
			AssertBarcodeOfProductWithSameOwnerIsUnique_DuplicateRelationship_Update("OWN", "BTH");
		}

		public void TestBarcodeOfProductWithSameOwnerIsUnique_DuplicateRelationship_Update_OwnerSameAsBoth()
		{
			AssertBarcodeOfProductWithSameOwnerIsUnique_DuplicateRelationship_Update("BTH", "OWN");
		}

		public void TestBarcodeOfProductWithSameOwnerIsUnique_DuplicateRelationship_Update_BothSameAsBoth()
		{
			AssertBarcodeOfProductWithSameOwnerIsUnique_DuplicateRelationship_Update("BTH", "BTH");
		}

		void AssertBarcodeOfProductWithSameOwnerIsUnique_DuplicateRelationship_Update(string relation1, string relation2)
		{
			var sql = new SqlQueryBuilder();
			var owner = new OrgHeader("OWNER").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P0").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner, product, relation1).AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode123", product).AppendInsertAndReturnObject(sql);

			var newProduct = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode123", newProduct).AppendInsertAndReturnObject(sql);
			var newRelation = new OrgPartRelation(owner, newProduct, "SUP").AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertExceptionThrown(typeof(SqlException), "The products with a same owner/code are not allowed to have the same barcode.",
				() => OrgPartRelation
				.UpdateWhere(newRelation.PK)
				.Set(r => r.OU_Relationship, relation2).Post(TestConnection), true);
		}

		#endregion

		#region TestBarcodeOfProductWithSameOwnerIsUnique_DuplicateClientAndRelationship_Update

		public void TestBarcodeOfProductWithSameOwnerIsUnique_DuplicateClientAndRelationship_Update_OwnerSameAsOwner()
		{
			AssertBarcodeOfProductWithSameOwnerIsUnique_DuplicateClientAndRelationship_Update("OWN", "OWN");
		}

		public void TestBarcodeOfProductWithSameOwnerIsUnique_DuplicateClientAndRelationship_Update_BothSameAsOwner()
		{
			AssertBarcodeOfProductWithSameOwnerIsUnique_DuplicateClientAndRelationship_Update("OWN", "BTH");
		}

		public void TestBarcodeOfProductWithSameOwnerIsUnique_DuplicateClientAndRelationship_Update_OwnerSameAsBoth()
		{
			AssertBarcodeOfProductWithSameOwnerIsUnique_DuplicateClientAndRelationship_Update("BTH", "OWN");
		}

		public void TestBarcodeOfProductWithSameOwnerIsUnique_DuplicateClientAndRelationship_Update_BothSameAsBoth()
		{
			AssertBarcodeOfProductWithSameOwnerIsUnique_DuplicateClientAndRelationship_Update("BTH", "BTH");
		}

		void AssertBarcodeOfProductWithSameOwnerIsUnique_DuplicateClientAndRelationship_Update(string relation1, string relation2)
		{
			var sql = new SqlQueryBuilder();
			var owner = new OrgHeader("OWNER").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P0").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner, product, relation1).AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode123", product).AppendInsertAndReturnObject(sql);

			var newOwner = new OrgHeader("OWNER1").AppendInsertAndReturnObject(sql);
			var newProduct = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			var newRelation = new OrgPartRelation(newOwner, newProduct, "SUP").AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode123", newProduct).AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertExceptionThrown(typeof(SqlException), "The products with a same owner/code are not allowed to have the same barcode.",
				() => OrgPartRelation
				.UpdateWhere(newRelation.PK)
				.Set(r => r.OU_OH, owner)
				.Set(r => r.OU_Relationship, relation2).Post(TestConnection), true);
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
			var product = new OrgSupplierPart("P0").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner, product, relation1).AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode123", product).AppendInsertAndReturnObject(sql);

			var newOwner = new OrgHeader("OWNER1").AppendInsertAndReturnObject(sql);
			var newProduct = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			var newRelation = new OrgPartRelation(newOwner, newProduct, relation2).AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode123", newProduct).AppendInsertAndReturnObject(sql);

			var newProduct2 = new OrgSupplierPart("P2").AppendInsertAndReturnObject(sql);
			var newRelation2 = new OrgPartRelation(owner, newProduct2, relation2).AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertExceptionThrown(typeof(SqlException), "The products with a same owner/code are not allowed to have the same barcode.",
				() => OrgPartRelation
				.UpdateWhere(newRelation2.PK)
				.Set(r => r.OU_OP, newProduct).Post(TestConnection), true);
		}

		#endregion

		#region TestBarcodeOfProductWithSameOwnerIsUnique_SupplierPart_Update_SameProductCode

		public void TestBarcodeOfProductWithSameOwnerIsUnique_SupplierPart_Update_SameProductCode()
		{
			var sql = new SqlQueryBuilder();
			var owner = new OrgHeader("OWNER").AppendInsertAndReturnObject(sql);
			var owner2 = new OrgHeader("OWNER2").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner, product, "OWN").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner2, product, "SUP").AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode123", product).AppendInsertAndReturnObject(sql);

			var owner3 = new OrgHeader("OWNER3").AppendInsertAndReturnObject(sql);
			var product2 = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			var relation2 = new OrgPartRelation(owner3, product2, "OWN").AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode456", product2).AppendInsertAndReturnObject(sql);

			var product3 = new OrgSupplierPart("P2").AppendInsertAndReturnObject(sql);
			var relation3 = new OrgPartRelation(owner, product2, "OWN").AppendInsertAndReturnObject(sql);

			try
			{
				DisableTriggers();
				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}
			finally
			{
				EnableTriggers();
			}

			AssertExceptionThrown(
				typeof(SqlException),
				"Relationship between product (OP_PartNum) and owner (OU_OH) must be unique for active (OP_IsActive) products.",
				() => OrgPartRelation
				.UpdateWhere(relation3.PK)
				.Set(r => r.OU_OP, product2).Post(TestConnection), true);
		}

		#endregion

		#region TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Update_ChangeSupplierPart

		public void TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Update_ChangeSupplierPart()
		{
			var sql = new SqlQueryBuilder();
			var owner = new OrgHeader("OWNER").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner, product, "OWN").AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode123", product).AppendInsertAndReturnObject(sql);

			var newOwner = new OrgHeader("OWNER2").AppendInsertAndReturnObject(sql);
			var newProduct = new OrgSupplierPart("P2").AppendInsertAndReturnObject(sql);
			var newRelation = new OrgPartRelation(newOwner, newProduct, "OWN").AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("P1", newProduct).AppendInsertAndReturnObject(sql);

			var newProduct2 = new OrgSupplierPart("P3").AppendInsertAndReturnObject(sql);
			var newRelation2 = new OrgPartRelation(owner, newProduct2, "OWN").AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertExceptionThrown("Should throw exception", typeof(SqlException), "The products with a same owner/code are not allowed to have the same barcode.",
				() => OrgPartRelation
				.UpdateWhere(newRelation2.PK)
				.Set(r => r.OU_OP, newProduct).Post(TestConnection), true);
		}

		#endregion

		#region TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Update_ChangeSupplierPart_ProductHasNoBarcodes

		public void TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Update_ChangeSupplierPart_ProductHasNoBarcodes()
		{
			var sql = new SqlQueryBuilder();
			var owner = new OrgHeader("OWNER").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner, product, "OWN").AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode123", product).AppendInsertAndReturnObject(sql);

			var owner2 = new OrgHeader("OWNER2").AppendInsertAndReturnObject(sql);
			var product2 = new OrgSupplierPart("barcode123").AppendInsertAndReturnObject(sql);
			var relation2 = new OrgPartRelation(owner2, product2, "OWN").AppendInsertAndReturnObject(sql);

			var product3 = new OrgSupplierPart("P3").AppendInsertAndReturnObject(sql);
			var relation3 = new OrgPartRelation(owner, product3, "OWN").AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertExceptionThrown("Should throw exception", typeof(SqlException), "The products with a same owner/code are not allowed to have the same barcode.",
				() => OrgPartRelation
				.UpdateWhere(relation3.PK)
				.Set(r => r.OU_OP, product2).Post(TestConnection), true);
		}

		#endregion

		#region TestBarcodeOfProductWithSameOwnerIsUnique_Update_ChangeClient_SameProductCode

		public void TestBarcodeOfProductWithSameOwnerIsUnique_Update_ChangeClient_SameProductCode()
		{
			var sql = new SqlQueryBuilder();
			var owner = new OrgHeader("OWNER").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner, product, "OWN").AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode123", product).AppendInsertAndReturnObject(sql);

			var owner2 = new OrgHeader("OWNER2").AppendInsertAndReturnObject(sql);
			var product2 = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			var relation2 = new OrgPartRelation(owner2, product2, "OWN").AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertExceptionThrown(
				typeof(SqlException),
				"Relationship between product (OP_PartNum) and owner (OU_OH) must be unique for active (OP_IsActive) products.",
				() => OrgPartRelation
				.UpdateWhere(relation2.PK)
				.Set(r => r.OU_OH, owner).Post(TestConnection), true);
		}

		#endregion

		#region TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Update_ChangeClient

		public void TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Update_ChangeClient()
		{
			var sql = new SqlQueryBuilder();
			var owner = new OrgHeader("OWNER").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("barcode456").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner, product, "OWN").AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode123", product).AppendInsertAndReturnObject(sql);

			var owner2 = new OrgHeader("OWNER2").AppendInsertAndReturnObject(sql);
			var newProduct = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			var newRelation = new OrgPartRelation(owner2, newProduct, "OWN").AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode456", newProduct).AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertExceptionThrown("Should throw exception", typeof(SqlException), "The products with a same owner/code are not allowed to have the same barcode.",
				() => OrgPartRelation
				.UpdateWhere(newRelation.PK)
				.Set(r => r.OU_OH, owner).Post(TestConnection), true);
		}

		#endregion

		#region TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Update_ChangeClient_ProductHasNoBarcodes

		public void TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Update_ChangeClient_ProductHasNoBarcodes()
		{
			var sql = new SqlQueryBuilder();
			var owner = new OrgHeader("OWNER").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("barcode456").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner, product, "OWN").AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode123", product).AppendInsertAndReturnObject(sql);

			var owner2 = new OrgHeader("OWNER2").AppendInsertAndReturnObject(sql);
			var newProduct = new OrgSupplierPart("barcode123").AppendInsertAndReturnObject(sql);
			var newRelation = new OrgPartRelation(owner2, newProduct, "OWN").AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertExceptionThrown("Should throw exception", typeof(SqlException), "The products with a same owner/code are not allowed to have the same barcode.",
				() => OrgPartRelation
				.UpdateWhere(newRelation.PK)
				.Set(r => r.OU_OH, owner).Post(TestConnection), true);
		}

		#endregion

		#region TestBarcodeOfProductWithSameOwnerIsUnique_Update_ChangeRelationShip_SameProduct

		public void TestBarcodeOfProductWithSameOwnerIsUnique_Update_ChangeRelationShip_SameProductCode()
		{
			var sql = new SqlQueryBuilder();
			var owner = new OrgHeader("OWNER").AppendInsertAndReturnObject(sql);
			var owner2 = new OrgHeader("OWNER2").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P0").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner, product, "OWN").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner, product, "SUP").AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode123", product).AppendInsertAndReturnObject(sql);

			var newProduct = new OrgSupplierPart("P0").AppendInsertAndReturnObject(sql);
			var newRelation = new OrgPartRelation(owner, newProduct, "SUP").AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertExceptionThrown(
				typeof(SqlException),
				"Relationship between product (OP_PartNum) and owner (OU_OH) must be unique for active (OP_IsActive) products.",
				() => OrgPartRelation
				.UpdateWhere(newRelation.PK)
				.Set(r => r.OU_Relationship, "OWN").Post(TestConnection), true);
		}

		#endregion

		#region TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Update_ChangeRelationShip

		public void TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Update_ChangeRelationShipToOwner()
		{
			TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Update_ChangeRelationShip("OWN");
		}

		public void TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Update_ChangeRelationShipToBoth()
		{
			TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Update_ChangeRelationShip("BTH");
		}

		void TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Update_ChangeRelationShip(string relation)
		{
			var sql = new SqlQueryBuilder();
			var owner = new OrgHeader("OWNER").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("barcode456").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner, product, "OWN").AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode123", product).AppendInsertAndReturnObject(sql);

			var newProduct = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			var newRelation = new OrgPartRelation(owner, newProduct, "SUP").AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode456", newProduct).AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertExceptionThrown("Should throw exception", typeof(SqlException), "The products with a same owner/code are not allowed to have the same barcode.",
				() => OrgPartRelation
				.UpdateWhere(newRelation.PK)
				.Set(r => r.OU_Relationship, relation).Post(TestConnection), true);
		}

		#endregion

		#region TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Update_ChangeRelationShip_ProductHasNoBarcodes_ProductHasNoBarcodes

		public void TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Update_ChangeRelationShipToOwner_ProductHasNoBarcodes()
		{
			TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Update_ChangeRelationShip_ProductHasNoBarcodes("OWN");
		}

		public void TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Update_ChangeRelationShipToBoth_ProductHasNoBarcodes()
		{
			TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Update_ChangeRelationShip_ProductHasNoBarcodes("BTH");
		}

		void TestBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_Update_ChangeRelationShip_ProductHasNoBarcodes(string relation)
		{
			var sql = new SqlQueryBuilder();
			var owner = new OrgHeader("OWNER").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("barcode456").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner, product, "OWN").AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode123", product).AppendInsertAndReturnObject(sql);

			var newProduct = new OrgSupplierPart("barcode123").AppendInsertAndReturnObject(sql);
			var newRelation = new OrgPartRelation(owner, newProduct, "SUP").AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertExceptionThrown("Should throw exception", typeof(SqlException), "The products with a same owner/code are not allowed to have the same barcode.",
				() => OrgPartRelation
				.UpdateWhere(newRelation.PK)
				.Set(r => r.OU_Relationship, relation).Post(TestConnection), true);
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
