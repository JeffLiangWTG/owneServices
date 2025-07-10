using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_OrgSupplierPart_EnsureActiveProductMustHasUniqueOwnerAndBarcode))]
	class TG_OrgSupplierPart_EnsureActiveProductMustHasUniqueOwnerAndBarcodeTest : DBCreateTriggerScriptTest
	{
		#region Insert

		#region TestActiveProductMustHasUniqueOwnerAndBarcode_Insert_ProductCodeEqualToBarcode

		public void TestActiveProductMustHasUniqueOwnerAndBarcode_Insert_UniqueBarcode_IncorrectCodeAndActivatedProduct()
		{
			TestActiveProductMustHasUniqueOwnerAndBarcode_Insert_ProductCodeEqualToBarcodeCore("P1", true);
		}

		public void TestActiveProductMustHasUniqueOwnerAndBarcode_Insert_UniqueBarcode_CorrectCodeAndActivatedProduct()
		{
			TestActiveProductMustHasUniqueOwnerAndBarcode_Insert_ProductCodeEqualToBarcodeCore("P2", true);
		}

		public void TestActiveProductMustHasUniqueOwnerAndBarcode_Insert_UniqueBarcode_CorrectCodeAndInactivatedProduct()
		{
			TestActiveProductMustHasUniqueOwnerAndBarcode_Insert_ProductCodeEqualToBarcodeCore("P2", false);
		}

		public void TestActiveProductMustHasUniqueOwnerAndBarcode_Insert_UniqueBarcode_IncorrectCodeAndInactivatedProduct()
		{
			TestActiveProductMustHasUniqueOwnerAndBarcode_Insert_ProductCodeEqualToBarcodeCore("P1", false);
		}

		void TestActiveProductMustHasUniqueOwnerAndBarcode_Insert_ProductCodeEqualToBarcodeCore(string productCode, bool isActive)
		{
			var sql = new SqlQueryBuilder();
			var owner = new OrgHeader("OWNER").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P0").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner, product, "OWN").AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("P1", product).AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var insertSql = new SqlQueryBuilder();
			new OrgSupplierPart("P1") { OP_IsActive = isActive }.AppendInsertAndReturnObject(insertSql);

			AssertNoExceptionThrown(() => TestConnection.ExecuteNonQuery(insertSql.ToString()));
		}

		#endregion

		#region TestActiveProductMustHasUniqueOwnerAndBarcode_Insert_ProductCodeEqualToBarcode_Relationship

		public void TestActiveProductMustHasUniqueOwnerAndBarcode_Insert_ProductCodeEqualToBarcode_Relationship_Owner()
		{
			TestActiveProductMustHasUniqueOwnerAndBarcode_Insert_ProductCodeEqualToBarcode_Relationship("OWN");
		}

		public void TestActiveProductMustHasUniqueOwnerAndBarcode_Insert_ProductCodeEqualToBarcode_Relationship_Both()
		{
			TestActiveProductMustHasUniqueOwnerAndBarcode_Insert_ProductCodeEqualToBarcode_Relationship("BTH");
		}

		public void TestActiveProductMustHasUniqueOwnerAndBarcode_Insert_ProductCodeEqualToBarcode_Relationship_Supplier()
		{
			TestActiveProductMustHasUniqueOwnerAndBarcode_Insert_ProductCodeEqualToBarcode_Relationship("SUP");
		}

		void TestActiveProductMustHasUniqueOwnerAndBarcode_Insert_ProductCodeEqualToBarcode_Relationship(string relation)
		{
			var sql = new SqlQueryBuilder();
			var owner = new OrgHeader("OWNER").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P0").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner, product, relation).AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("P1", product).AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var insertSql = new SqlQueryBuilder();
			new OrgSupplierPart("P1").AppendInsertAndReturnObject(insertSql);

			AssertNoExceptionThrown(() => TestConnection.ExecuteNonQuery(insertSql.ToString()));
		}

		#endregion

		#region TestActiveProductMustHasUniqueOwnerAndBarcode_Insert_ProductCodeEqualToBarcode_HasNoRelateOrgs

		public void TestActiveProductMustHasUniqueOwnerAndBarcode_Insert_ProductCodeEqualToBarcode_HasNoRelateOrgs()
		{
			var sql = new SqlQueryBuilder();
			var owner = new OrgHeader("OWNER").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner, product, "OWN").AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode", product).AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var insertSql = new SqlQueryBuilder();
			new OrgSupplierPart("barcode").AppendInsertAndReturnObject(insertSql);

			AssertNoExceptionThrown(() => TestConnection.ExecuteNonQuery(insertSql.ToString()));
		}

		#endregion

		#endregion

		#region Update

		#region TestActiveProductMustHasUniqueOwnerAndBarcode_Update_UniqueBarcode

		public void TestActiveProductMustHasUniqueOwnerAndBarcode_Update_UniqueBarcode_ActivateProduct()
		{
			AssertActiveProductMustHasUniqueOwnerAndBarcode_Update_UniqueBarcode(true);
		}

		public void TestActiveProductMustHasUniqueOwnerAndBarcode_Update_UniqueBarcode_InactivateProduct()
		{
			AssertActiveProductMustHasUniqueOwnerAndBarcode_Update_UniqueBarcode(false);
		}

		void AssertActiveProductMustHasUniqueOwnerAndBarcode_Update_UniqueBarcode(bool isActive)
		{
			var sql = new SqlQueryBuilder();
			var owner = new OrgHeader("OWNER").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P0").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner, product, "OWN").AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode123", product).AppendInsertAndReturnObject(sql);

			var newProduct = new OrgSupplierPart("P1") { OP_IsActive = !isActive }.AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner, newProduct, "OWN").AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode456", newProduct).AppendInsertAndReturnObject(sql);
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertNoExceptionThrown(
				() => OrgSupplierPart
				.UpdateWhere(newProduct.PK)
				.Set(p => p.OP_IsActive, isActive).Post(TestConnection));
		}

		#endregion

		#region TestActiveProductMustHasUniqueOwnerAndBarcode_Update_UniqueBarcode_SameProductCode

		public void TestActiveProductMustHasUniqueOwnerAndBarcode_Update_UniqueBarcode_SameProductCode()
		{
			var sql = new SqlQueryBuilder();
			var owner = new OrgHeader("OWNER").AppendInsertAndReturnObject(sql);
			var owner2 = new OrgHeader("OWNER2").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P0").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner, product, "OWN").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner2, product, "SUP").AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode123", product).AppendInsertAndReturnObject(sql);

			var newProduct = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner, newProduct, "OWN").AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode456", newProduct).AppendInsertAndReturnObject(sql);
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertNoExceptionThrown(
				() => OrgSupplierPart
				.UpdateWhere(newProduct.PK)
				.Set(p => p.OP_PartNum, "P1").Post(TestConnection));
		}

		#endregion

		#region TestActiveProductMustHasUniqueOwnerAndBarcode_Update_RelationIsNotOwner

		public void TestActiveProductMustHasUniqueOwnerAndBarcode_Update_RelationIsNotOwner_ActivateProduct()
		{
			AssertActiveProductMustHasUniqueOwnerAndBarcode_Update_RelationIsNotOwner(true);
		}

		public void TestActiveProductMustHasUniqueOwnerAndBarcode_Update_RelationIsNotOwner_InactivateProduct()
		{
			AssertActiveProductMustHasUniqueOwnerAndBarcode_Update_RelationIsNotOwner(false);
		}

		void AssertActiveProductMustHasUniqueOwnerAndBarcode_Update_RelationIsNotOwner(bool isActive)
		{
			var sql = new SqlQueryBuilder();
			var owner = new OrgHeader("OWNER").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P0").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner, product, "OWN").AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode123", product).AppendInsertAndReturnObject(sql);

			var newProduct = new OrgSupplierPart("P1") { OP_IsActive = isActive }.AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner, newProduct, "SUP").AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode123", newProduct).AppendInsertAndReturnObject(sql);
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertNoExceptionThrown(
				() => OrgSupplierPart
				.UpdateWhere(newProduct.PK)
				.Set(p => p.OP_IsActive, isActive).Post(TestConnection));
		}

		#endregion

		#region TestActiveProductMustHasUniqueOwnerAndBarcode_Update_DifferentOwners

		public void TestActiveProductMustHasUniqueOwnerAndBarcode_Update_DifferentOwners_ActivateProduct()
		{
			AssertActiveProductMustHasUniqueOwnerAndBarcode_Update_DifferentOwners(true);
		}

		public void TestActiveProductMustHasUniqueOwnerAndBarcode_Update_DifferentOwners_InactivateProduct()
		{
			AssertActiveProductMustHasUniqueOwnerAndBarcode_Update_DifferentOwners(false);
		}

		void AssertActiveProductMustHasUniqueOwnerAndBarcode_Update_DifferentOwners(bool isActive)
		{
			var sql = new SqlQueryBuilder();
			var owner = new OrgHeader("OWNER0").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P0").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner, product, "OWN").AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode123", product).AppendInsertAndReturnObject(sql);

			var newOwner = new OrgHeader("OWNER1").AppendInsertAndReturnObject(sql);
			var newProduct = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			if (isActive)
			{
				sql.Append($@"
					UPDATE
						dbo.OrgSupplierPart
					SET
						OP_IsActive = 0,
						OP_SystemLastEditTimeUtc = SYSUTCDATETIME(),
						OP_SystemLastEditUser = '~BP'
					WHERE
						OP_PK = '{newProduct.PK}'");
			}
			new OrgPartRelation(newOwner, newProduct, "OWN").AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode123", newProduct).AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertNoExceptionThrown(
				() => OrgSupplierPart
				.UpdateWhere(newProduct.PK)
				.Set(p => p.OP_IsActive, isActive).Post(TestConnection));
		}

		#endregion

		#region TestActiveProductMustHasUniqueOwnerAndBarcode_Update_SameOwnerAndBarcode_ActivateProduct

		public void TestActiveProductMustHasUniqueOwnerAndBarcode_Update_SameOwnerAndBarcode_ActivateProduct()
		{
			var sql = new SqlQueryBuilder();
			var owner = new OrgHeader("OWNER").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P0").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner, product, "OWN").AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode123", product).AppendInsertAndReturnObject(sql);

			var newProduct = new OrgSupplierPart("P1") { OP_IsActive = false }.AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner, newProduct, "OWN").AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode123", newProduct).AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertExceptionThrown(typeof(SqlException), "The products with a same owner/code are not allowed to have the same barcode.",
				() => OrgSupplierPart
				.UpdateWhere(newProduct.PK)
				.Set(p => p.OP_IsActive, true).Post(TestConnection), true);
		}

		#endregion

		#region TestActiveProductMustHasUniqueOwnerAndBarcode_Update_SameProductCodeAndOwnerAndBarcode_ActivateProduct

		public void TestActiveProductMustHasUniqueOwnerAndBarcode_Update_SameProductCodeAndOwnerAndBarcode_ActivateProduct()
		{
			var sql = new SqlQueryBuilder();
			var owner = new OrgHeader("OWNER").AppendInsertAndReturnObject(sql);
			var owner2 = new OrgHeader("OWNER2").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner, product, "OWN").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner2, product, "SUP").AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode123", product).AppendInsertAndReturnObject(sql);

			var newProduct = new OrgSupplierPart("P1") { OP_IsActive = false }.AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner, newProduct, "OWN").AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode123", newProduct).AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var updateSql = new SqlQueryBuilder($@"
				-- triggers for unique product owners also detect this operation as invalid, so we need to disable them temporarily to reliably check that barcode trigger also works
				DISABLE TRIGGER TG_OrgSupplierPart_DuplicateCheck ON OrgSupplierPart;");

			updateSql.Append(OrgSupplierPart
				.UpdateWhere(newProduct.PK)
				.Set(p => p.OP_IsActive, true).AsSQL() + ";");
			updateSql.Append("ENABLE TRIGGER TG_OrgSupplierPart_DuplicateCheck ON OrgSupplierPart;");

			AssertExceptionThrown(typeof(SqlException), "The products with a same owner/code are not allowed to have the same barcode.",
				() => TestConnection.ExecuteNonQuery(updateSql.ToStringWithNewLineBetweenAppends()), true);
		}

		#endregion

		#region TestActiveProductMustHasUniqueOwnerAndBarcode_Update_ProductCodeEqualToBarcode

		public void TestActiveProductMustHasUniqueOwnerAndBarcode_Update_ProductCodeEqualToBarcode_ActiveProduct()
		{
			TestActiveProductMustHasUniqueOwnerAndBarcode_Update_ProductCodeEqualToBarcode(true);
		}

		public void TestActiveProductMustHasUniqueOwnerAndBarcode_Update_ProductCodeEqualToBarcode_InActiveProduct()
		{
			TestActiveProductMustHasUniqueOwnerAndBarcode_Update_ProductCodeEqualToBarcode(false);
		}

		void TestActiveProductMustHasUniqueOwnerAndBarcode_Update_ProductCodeEqualToBarcode(bool isActive)
		{
			var sql = new SqlQueryBuilder();
			var owner = new OrgHeader("OWNER").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P0") { OP_IsActive = isActive }.AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner, product, "OWN").AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode123", product).AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var updateSql = OrgSupplierPart
				.UpdateWhere(product.PK)
				.Set(p => p.OP_PartNum, "barcode123").AsSQL();

			if (isActive)
			{
				AssertExceptionThrown(typeof(SqlException), "The products with a same owner/code are not allowed to have the same barcode.",
					() => TestConnection.ExecuteNonQuery(updateSql), true);
			}
			else
			{
				AssertNoExceptionThrown("Should not throw exception as the Product is in-active",
					() => TestConnection.ExecuteNonQuery(updateSql));

				AssertExceptionThrown("Should throw exception as the Product is activing", typeof(SqlException), "The products with a same owner/code are not allowed to have the same barcode.",
					() => OrgSupplierPart
					.UpdateWhere(product.PK)
					.Set(p => p.OP_IsActive, true).Post(TestConnection), true);
			}
		}

		#endregion

		#region TestActiveProductMustHasUniqueOwnerAndBarcode_Update_ProductCodeEqualToBarcode_AnotherProduct

		public void TestActiveProductMustHasUniqueOwnerAndBarcode_Update_ProductCodeEqualToBarcode_AnotherProduct_Product1ActiveAndProduct2Active()
		{
			TestActiveProductMustHasUniqueOwnerAndBarcode_Update_ProductCodeEqualToBarcode_AnotherProduct(true, true);
		}

		public void TestActiveProductMustHasUniqueOwnerAndBarcode_Update_ProductCodeEqualToBarcode_AnotherProduct_Product1ActiveAndProduct2Inactive()
		{
			TestActiveProductMustHasUniqueOwnerAndBarcode_Update_ProductCodeEqualToBarcode_AnotherProduct(true, false);
		}

		public void TestActiveProductMustHasUniqueOwnerAndBarcode_Update_ProductCodeEqualToBarcode_AnotherProduct_Product1InactiveAndProduct2Active()
		{
			TestActiveProductMustHasUniqueOwnerAndBarcode_Update_ProductCodeEqualToBarcode_AnotherProduct(false, true);
		}

		public void TestActiveProductMustHasUniqueOwnerAndBarcode_Update_ProductCodeEqualToBarcode_AnotherProduct_Product1InactiveAndProduct2Inactive()
		{
			TestActiveProductMustHasUniqueOwnerAndBarcode_Update_ProductCodeEqualToBarcode_AnotherProduct(false, false);
		}

		void TestActiveProductMustHasUniqueOwnerAndBarcode_Update_ProductCodeEqualToBarcode_AnotherProduct(bool product1IsActive, bool product2IsActive)
		{
			var sql = new SqlQueryBuilder();
			var owner = new OrgHeader("OWNER").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1") { OP_IsActive = product1IsActive }.AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner, product, "OWN").AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode123", product).AppendInsertAndReturnObject(sql);

			var product2 = new OrgSupplierPart("P2") { OP_IsActive = product2IsActive }.AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner, product2, "OWN").AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode456", product2).AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var updateSql = OrgSupplierPart
				.UpdateWhere(product.PK)
				.Set(p => p.OP_PartNum, "barcode456").AsSQL();

			if (product1IsActive && product2IsActive)
			{
				AssertExceptionThrown("Should throw exception", typeof(SqlException), "The products with a same owner/code are not allowed to have the same barcode.",
					() => TestConnection.ExecuteNonQuery(updateSql), true);
			}
			else if (!product1IsActive && product2IsActive)
			{
				AssertNoExceptionThrown("Should not throw exception as the Product1 is in-active",
					() => TestConnection.ExecuteNonQuery(updateSql));
			}
			else if (product1IsActive && !product2IsActive)
			{
				AssertNoExceptionThrown("Should not throw exception as the Product2 is in-active",
					() => TestConnection.ExecuteNonQuery(updateSql));
			}
			else if (!product1IsActive && !product2IsActive)
			{
				AssertNoExceptionThrown("Should not throw exception as the Product1 & Product2 are all in-active",
					() => TestConnection.ExecuteNonQuery(updateSql));
			}
		}

		#endregion

		#region TestActiveProductMustHasUniqueOwnerAndBarcode_Update_ProductCodeEqualToBarcode_Update_AnotherProduct_RelationShip

		public void TestActiveProductMustHasUniqueOwnerAndBarcode_Update_ProductCodeEqualToBarcode_Update_AnotherProduct_RelationShip_OwnerSameAsOwner()
		{
			TestActiveProductMustHasUniqueOwnerAndBarcode_Update_ProductCodeEqualToBarcode_Update_AnotherProduct_RelationShip("OWN", "OWN", expectHasException: true);
		}

		public void TestActiveProductMustHasUniqueOwnerAndBarcode_Update_ProductCodeEqualToBarcode_Update_AnotherProduct_RelationShip_BothSameAsOwner()
		{
			TestActiveProductMustHasUniqueOwnerAndBarcode_Update_ProductCodeEqualToBarcode_Update_AnotherProduct_RelationShip("OWN", "BTH", expectHasException: true);
		}

		public void TestActiveProductMustHasUniqueOwnerAndBarcode_Update_ProductCodeEqualToBarcode_Update_AnotherProduct_RelationShip_OwnerSameAsBoth()
		{
			TestActiveProductMustHasUniqueOwnerAndBarcode_Update_ProductCodeEqualToBarcode_Update_AnotherProduct_RelationShip("BTH", "OWN", expectHasException: true);
		}

		public void TestActiveProductMustHasUniqueOwnerAndBarcode_Update_ProductCodeEqualToBarcode_Update_AnotherProduct_RelationShip_BothSameAsBoth()
		{
			TestActiveProductMustHasUniqueOwnerAndBarcode_Update_ProductCodeEqualToBarcode_Update_AnotherProduct_RelationShip("BTH", "BTH", expectHasException: true);
		}

		public void TestActiveProductMustHasUniqueOwnerAndBarcode_Update_ProductCodeEqualToBarcode_Update_AnotherProduct_RelationShip_SupplierSameAsBoth()
		{
			TestActiveProductMustHasUniqueOwnerAndBarcode_Update_ProductCodeEqualToBarcode_Update_AnotherProduct_RelationShip("SUP", "BTH");
		}

		public void TestActiveProductMustHasUniqueOwnerAndBarcode_Update_ProductCodeEqualToBarcode_Update_AnotherProduct_RelationShip_BothSameAsSupplier()
		{
			TestActiveProductMustHasUniqueOwnerAndBarcode_Update_ProductCodeEqualToBarcode_Update_AnotherProduct_RelationShip("BTH", "SUP");
		}

		public void TestActiveProductMustHasUniqueOwnerAndBarcode_Update_ProductCodeEqualToBarcode_Update_AnotherProduct_RelationShip_SupplierSameAsOwner()
		{
			TestActiveProductMustHasUniqueOwnerAndBarcode_Update_ProductCodeEqualToBarcode_Update_AnotherProduct_RelationShip("SUP", "OWN");
		}

		public void TestActiveProductMustHasUniqueOwnerAndBarcode_Update_ProductCodeEqualToBarcode_Update_AnotherProduct_RelationShip_OwnerSameAsSupplier()
		{
			TestActiveProductMustHasUniqueOwnerAndBarcode_Update_ProductCodeEqualToBarcode_Update_AnotherProduct_RelationShip("OWN", "SUP");
		}

		public void TestActiveProductMustHasUniqueOwnerAndBarcode_Update_ProductCodeEqualToBarcode_Update_AnotherProduct_RelationShip_SupplierSameAsSupplier()
		{
			TestActiveProductMustHasUniqueOwnerAndBarcode_Update_ProductCodeEqualToBarcode_Update_AnotherProduct_RelationShip("SUP", "SUP");
		}

		void TestActiveProductMustHasUniqueOwnerAndBarcode_Update_ProductCodeEqualToBarcode_Update_AnotherProduct_RelationShip(string relation1, string relation2, bool expectHasException = false)
		{
			var sql = new SqlQueryBuilder();
			var owner = new OrgHeader("OWNER").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner, product, relation1).AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode123", product).AppendInsertAndReturnObject(sql);

			var product2 = new OrgSupplierPart("P2").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner, product2, relation2).AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode456", product2).AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var updateSql = OrgSupplierPart
				.UpdateWhere(product.PK)
				.Set(p => p.OP_PartNum, "barcode456").AsSQL();

			if (expectHasException)
			{
				AssertExceptionThrown(typeof(SqlException), "The products with a same owner/code are not allowed to have the same barcode.",
					() => TestConnection.ExecuteNonQuery(updateSql), true);
			}
			else
			{
				AssertNoExceptionThrown("Should not throw exception as one of relationship is supplier", () => TestConnection.ExecuteNonQuery(updateSql));
			}
		}

		#endregion

		#region TestActiveProductMustHasUniqueOwnerAndBarcode_Update_ProductCodeEqualToBarcode_AnotherProduct_DifferentOwner

		public void TestActiveProductMustHasUniqueOwnerAndBarcode_Update_ProductCodeEqualToBarcode_AnotherProduct_DifferentOwner_Product1ActiveAndProduct2Active()
		{
			TestActiveProductMustHasUniqueOwnerAndBarcode_Update_ProductCodeEqualToBarcode_AnotherProduct_DifferentOwner(true, true);
		}

		public void TestActiveProductMustHasUniqueOwnerAndBarcode_Update_ProductCodeEqualToBarcode_AnotherProduc_DifferentOwnert_Product1ActiveAndProduct2Inactive()
		{
			TestActiveProductMustHasUniqueOwnerAndBarcode_Update_ProductCodeEqualToBarcode_AnotherProduct_DifferentOwner(true, false);
		}

		public void TestActiveProductMustHasUniqueOwnerAndBarcode_Update_ProductCodeEqualToBarcode_AnotherProduct_DifferentOwner_Product1InactiveAndProduct2Active()
		{
			TestActiveProductMustHasUniqueOwnerAndBarcode_Update_ProductCodeEqualToBarcode_AnotherProduct_DifferentOwner(false, true);
		}

		public void TestActiveProductMustHasUniqueOwnerAndBarcode_Update_ProductCodeEqualToBarcode_AnotherProduct_DifferentOwner_Product1InactiveAndProduct2Inactive()
		{
			TestActiveProductMustHasUniqueOwnerAndBarcode_Update_ProductCodeEqualToBarcode_AnotherProduct_DifferentOwner(false, false);
		}

		void TestActiveProductMustHasUniqueOwnerAndBarcode_Update_ProductCodeEqualToBarcode_AnotherProduct_DifferentOwner(bool product1IsActive, bool product2IsActive, bool expectHasException = false)
		{
			var sql = new SqlQueryBuilder();
			var owner = new OrgHeader("OWNER").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1") { OP_IsActive = product1IsActive }.AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner, product, "OWN").AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode123", product).AppendInsertAndReturnObject(sql);

			var owner2 = new OrgHeader("OWNER2").AppendInsertAndReturnObject(sql);
			var product2 = new OrgSupplierPart("P2") { OP_IsActive = product2IsActive }.AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner2, product2, "OWN").AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode456", product2).AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var updateSql = OrgSupplierPart
				.UpdateWhere(product.PK)
				.Set(p => p.OP_PartNum, "barcode456").AsSQL();

			if (expectHasException)
			{
				AssertExceptionThrown(typeof(SqlException), "The products with a same owner/code are not allowed to have the same barcode.",
					() => TestConnection.ExecuteNonQuery(updateSql), true);
			}
			else
			{
				AssertNoExceptionThrown("Should not throw exception as the Product1 and Product2 are in different owner",
					() => TestConnection.ExecuteNonQuery(updateSql));
			}
		}

		#endregion

		#region TestActiveProductMustHasUniqueOwnerAndBarcode_Update_ProductCodeEqualToBarcode_HasNoRelatedOrg

		public void TestActiveProductMustHasUniqueOwnerAndBarcode_Update_ProductCodeEqualToBarcode_HasNoRelatedOrg()
		{
			var sql = new SqlQueryBuilder();
			var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode123", product).AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertNoExceptionThrown("Should not throw exception as products are not have related organisations",
				() => OrgSupplierPart
				.UpdateWhere(product.PK)
				.Set(p => p.OP_PartNum, "barcode123").Post(TestConnection));
		}

		#endregion

		#region TestActiveProductMustHasUniqueOwnerAndBarcode_Update_ProductCodeEqualToBarcode_HasNoRelatedOrg_AnotherProduct

		public void TestActiveProductMustHasUniqueOwnerAndBarcode_Update_ProductCodeEqualToBarcode_HasNoRelatedOrg_AnotherProduct()
		{
			var sql = new SqlQueryBuilder();
			var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode123", product).AppendInsertAndReturnObject(sql);

			var product2 = new OrgSupplierPart("P2").AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode456", product2).AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertNoExceptionThrown("Should not throw exception as products are not have related organisations", 
				() => OrgSupplierPart
				.UpdateWhere(product.PK)
				.Set(p => p.OP_PartNum, "barcode456").Post(TestConnection));
		}

		#endregion

		#region TestActiveProductMustHasUniqueOwnerAndBarcode_Update_ProductCodeEqualToBarcode_HasNoBarcodes

		public void TestActiveProductMustHasUniqueOwnerAndBarcode_Update_ProductCodeEqualToBarcode_HasNoBarcodes()
		{
			var sql = new SqlQueryBuilder();
			var owner = new OrgHeader("OWNER").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner, product, "OWN").AppendInsertAndReturnObject(sql);

			var product2 = new OrgSupplierPart("P2").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(owner, product2, "OWN").AppendInsertAndReturnObject(sql);
			new OrgSupplierPartBarcode("barcode456", product2).AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertExceptionThrown(typeof(SqlException), "The products with a same owner/code are not allowed to have the same barcode.",
				() =>  OrgSupplierPart
				.UpdateWhere(product.PK)
				.Set(p => p.OP_PartNum, "barcode456").Post(TestConnection), true);
		}
		#endregion

		#endregion
	}
}

