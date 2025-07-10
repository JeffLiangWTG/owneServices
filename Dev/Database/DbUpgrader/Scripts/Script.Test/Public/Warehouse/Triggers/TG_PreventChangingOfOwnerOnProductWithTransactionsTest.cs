using System;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_PreventChangingOfOwnerOnProductWithTransactions))]
	class TG_PreventChangingOfOwnerOnProductWithTransactionsTest : DBCreateTriggerScriptTest
	{
		#region TestTG_PreventChangingOfOwnerOnProductWithTransactions_ChangeOrg

		public void TestTG_PreventChangingOfOwnerOnProductWithTransactions_ChangeOrg_Owner()
		{
			TestTG_PreventChangingOfOwnerOnProductWithTransactions_ChangeOrg(Owner, true);
		}

		public void TestTG_PreventChangingOfOwnerOnProductWithTransactions_ChangeOrg_Both()
		{
			TestTG_PreventChangingOfOwnerOnProductWithTransactions_ChangeOrg(Both, true);
		}

		public void TestTG_PreventChangingOfOwnerOnProductWithTransactions_ChangeOrg_Supplier()
		{
			TestTG_PreventChangingOfOwnerOnProductWithTransactions_ChangeOrg(Supplier, false);
		}

		void TestTG_PreventChangingOfOwnerOnProductWithTransactions_ChangeOrg(string relationship, bool expectException)
		{
			var sql = new SqlQueryBuilder();
			var client1 = new OrgHeader("CLIENT1").AppendInsertAndReturnObject(sql);
			var client2 = new OrgHeader("CLIENT2").AppendInsertAndReturnObject(sql);
			var product1 = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			var product2 = new OrgSupplierPart("P2").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(client1, product1, relationship) { OU_ClientUQ = "UNT" }.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			AssertNoExceptionThrown(
				() => OrgPartRelation
				.UpdateWhere(r => r.OU_OP == product1.PK && r.OU_OH == client1.PK)
				.Set(r => r.OU_OH, client2).Post(TestConnection));

			sql = new SqlQueryBuilder();
			var branch1 = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs1 = new WhsWarehouse("WH1", branch1.PK).WithDockDoor(TestConnection);
			new OrgPartRelation(client2, product2, relationship) { OU_ClientUQ = "UNT" }.AppendInsertAndReturnObject(sql);
			CreateSOH(sql, whs1, client2, product2);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var updateSQL = OrgPartRelation
				.UpdateWhere(r => r.OU_OP == product2.PK && r.OU_OH == client2.PK)
				.Set(r => r.OU_OH, client1).AsSQL();
			if (expectException)
			{
				AssertExceptionThrown(typeof(SqlException), "Cannot change or delete the Owner of a Product with existing transactions.", () => TestConnection.ExecuteNonQuery(updateSQL), true);
			}
			else
			{
				AssertNoExceptionThrown(() => TestConnection.ExecuteNonQuery(updateSQL));
			}
		}

		#endregion

		#region TestTG_PreventChangingOfOwnerOnProductWithTransactions_ChangeProduct

		public void TestTG_PreventChangingOfOwnerOnProductWithTransactions_ChangeProduct_Owner()
		{
			TestTG_PreventChangingOfOwnerOnProductWithTransactions_ChangeProduct(Owner, true);
		}

		public void TestTG_PreventChangingOfOwnerOnProductWithTransactions_ChangeProduct_Both()
		{
			TestTG_PreventChangingOfOwnerOnProductWithTransactions_ChangeProduct(Both, true);
		}

		public void TestTG_PreventChangingOfOwnerOnProductWithTransactions_ChangeProduct_Supplier()
		{
			TestTG_PreventChangingOfOwnerOnProductWithTransactions_ChangeProduct(Supplier, false);
		}

		void TestTG_PreventChangingOfOwnerOnProductWithTransactions_ChangeProduct(string relationship, bool expectException)
		{
			var sql = new SqlQueryBuilder();
			var client1 = new OrgHeader("CLIENT1").AppendInsertAndReturnObject(sql);
			var client2 = new OrgHeader("CLIENT2").AppendInsertAndReturnObject(sql);
			var product1 = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			var product2 = new OrgSupplierPart("P2").AppendInsertAndReturnObject(sql);
			var product3 = new OrgSupplierPart("P3").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(client1, product1, relationship) { OU_ClientUQ = "UNT" }.AppendInsertAndReturnObject(sql);
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertNoExceptionThrown(
				() => OrgPartRelation
				.UpdateWhere(r => r.OU_OP == product1.PK && r.OU_OH == client1.PK)
				.Set(r => r.OU_OP, product3).Post(TestConnection));

			sql = new SqlQueryBuilder();
			var branch1 = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs1 = new WhsWarehouse("WH1", branch1.PK).WithDockDoor(TestConnection);
			new OrgPartRelation(client2, product2, relationship) { OU_ClientUQ = "UNT" }.AppendInsertAndReturnObject(sql);
			CreateSOH(sql, whs1, client2, product2);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			var updateSQL = OrgPartRelation
				.UpdateWhere(r => r.OU_OP == product2.PK && r.OU_OH == client2.PK)
				.Set(r => r.OU_OP, product3).AsSQL();

			if (expectException)
			{
				AssertExceptionThrown(typeof(SqlException), "Cannot change or delete the Owner of a Product with existing transactions.", () => TestConnection.ExecuteNonQuery(updateSQL), true);
			}
			else
			{
				AssertNoExceptionThrown(() => TestConnection.ExecuteNonQuery(updateSQL));
			}
		}

		#endregion

		#region TestTG_PreventChangingOfOwnerOnProductWithTransactions_ChangeRelationship

		public void TestTG_PreventChangingOfOwnerOnProductWithTransactions_ChangeRelationship_Owner()
		{
			TestTG_PreventChangingOfOwnerOnProductWithTransactions_ChangeRelationship_Core(Owner);
		}

		public void TestTG_PreventChangingOfOwnerOnProductWithTransactions_ChangeRelationship_Both()
		{
			TestTG_PreventChangingOfOwnerOnProductWithTransactions_ChangeRelationship_Core(Both);
		}

		void TestTG_PreventChangingOfOwnerOnProductWithTransactions_ChangeRelationship_Core(string relationship)
		{
			var sql = new SqlQueryBuilder();
			var client1 = new OrgHeader("CLIENT1").AppendInsertAndReturnObject(sql);
			var product1 = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			var product2 = new OrgSupplierPart("P2").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(client1, product1, Supplier) { OU_ClientUQ = "UNT" }.AppendInsertAndReturnObject(sql);
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertNoExceptionThrown(
				() => OrgPartRelation
				.UpdateWhere(r => r.OU_OP == product1.PK && r.OU_OH == client1.PK)
				.Set(r => r.OU_Relationship, relationship).Post(TestConnection));

			AssertNoExceptionThrown(
				() => OrgPartRelation
				.UpdateWhere(r => r.OU_OP == product1.PK && r.OU_OH == client1.PK)
				.Set(r => r.OU_Relationship, "SUP").Post(TestConnection));

			sql = new SqlQueryBuilder();
			var branch1 = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs1 = new WhsWarehouse("WH1", branch1.PK).WithDockDoor(TestConnection);
			new OrgPartRelation(client1, product2, Supplier) { OU_ClientUQ = "UNT" }.AppendInsertAndReturnObject(sql);
			CreateSOH(sql, whs1, client1, product2);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertNoExceptionThrown(
				() => OrgPartRelation
				.UpdateWhere(r => r.OU_OP == product2.PK && r.OU_OH == client1.PK)
				.Set(r => r.OU_Relationship, relationship).Post(TestConnection));

			AssertExceptionThrown(typeof(SqlException), "Cannot change or delete the Owner of a Product with existing transactions.", 
				() => OrgPartRelation
				.UpdateWhere(r => r.OU_OP == product2.PK && r.OU_OH == client1.PK)
				.Set(r => r.OU_Relationship, "SUP").Post(TestConnection), true);
		}

		#endregion

		#region TestTG_PreventChangingOfOwnerOnProductWithTransactions_ChangeUnrelatedField

		public void TestTG_PreventChangingOfOwnerOnProductWithTransactions_ChangeUnrelatedField()
		{
			var sql = new SqlQueryBuilder();
			var branch1 = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs1 = new WhsWarehouse("WH1", branch1.PK).WithDockDoor(TestConnection);
			var client1 = new OrgHeader("CLIENT1").AppendInsertAndReturnObject(sql);
			var client2 = new OrgHeader("CLIENT2").AppendInsertAndReturnObject(sql);
			var product1 = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(client1, product1, Owner) { OU_ClientUQ = "UNT" }.AppendInsertAndReturnObject(sql);
			CreateSOH(sql, whs1, client1, product1);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			var updateSQL = $@"
				UPDATE 
					dbo.OrgPartRelation
				SET
					OU_LocalPartDescription = 'SOMENEWDESCRIPTION',
					OU_SystemLastEditTimeUtc = SYSUTCDATETIME(),
					OU_SystemLastEditUser = '~BP'
				WHERE
					OU_OP ='{product1.PK}' AND OU_OH ='{client1.PK}'";
			AssertNoExceptionThrown("This should work, as updated field is not Client or Product or Relationship Type", () => TestConnection.ExecuteNonQuery(updateSQL));

			updateSQL = $@"
				UPDATE 
					dbo.OrgPartRelation
				SET
					OU_LocalPartDescription = 'ANOTHERDESCRIPTION',
					OU_OH = '{client2.PK}',
					OU_SystemLastEditTimeUtc = SYSUTCDATETIME(),
					OU_SystemLastEditUser = '~BP'
				WHERE
					OU_OP ='{product1.PK}' AND OU_OH ='{client1.PK}'";
			AssertExceptionThrown("This should FAIL, as one of the updated fields is Client", typeof(SqlException), "Cannot change or delete the Owner of a Product with existing transactions.", () => TestConnection.ExecuteNonQuery(updateSQL), true);
		}

		#endregion

		#region TestTG_PreventChangingOfOwnerOnProductWithTransactions_RemoveRelationship

		public void TestTG_PreventChangingOfOwnerOnProductWithTransactions_RemoveRelationship_Owner()
		{
			var sql = new SqlQueryBuilder();
			var branch1 = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs1 = new WhsWarehouse("WH1", branch1.PK).WithDockDoor(TestConnection);
			var client1 = new OrgHeader("CLIENT1").AppendInsertAndReturnObject(sql);
			var product1 = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			var product2 = new OrgSupplierPart("P2").AppendInsertAndReturnObject(sql);
			var product3 = new OrgSupplierPart("P3").AppendInsertAndReturnObject(sql);
			var product4 = new OrgSupplierPart("P4").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(client1, product1, Owner) { OU_ClientUQ = "UNT" }.AppendInsertAndReturnObject(sql);
			new OrgPartRelation(client1, product2, Supplier) { OU_ClientUQ = "UNT" }.AppendInsertAndReturnObject(sql);
			new OrgPartRelation(client1, product3, Owner) { OU_ClientUQ = "UNT" }.AppendInsertAndReturnObject(sql);
			new OrgPartRelation(client1, product4, Supplier) { OU_ClientUQ = "UNT" }.AppendInsertAndReturnObject(sql);
			CreateSOH(sql, whs1, client1, product3);
			CreateSOH(sql, whs1, client1, product4);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			AssertNoExceptionThrown(
				() => OrgPartRelation
				.DeleteInDB(TestConnection, r => r.OU_OP == product1.PK || r.OU_OP == product2.PK || r.OU_OP == product4.PK));

			AssertExceptionThrown(typeof(SqlException), "Cannot change or delete the Owner of a Product with existing transactions.",
				() => OrgPartRelation
				.DeleteInDB(TestConnection, r => r.OU_OH == client1.PK && r.OU_OP == product3.PK), true);
		}

		public void TestTG_PreventChangingOfOwnerOnProductWithTransactions_RemoveRelationship_Both()
		{
			var sql = new SqlQueryBuilder();
			var branch1 = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs1 = new WhsWarehouse("WH1", branch1.PK).WithDockDoor(TestConnection);
			var client1 = new OrgHeader("CLIENT1").AppendInsertAndReturnObject(sql);
			var product1 = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			var product2 = new OrgSupplierPart("P2").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(client1, product1, Both) { OU_ClientUQ = "UNT" }.AppendInsertAndReturnObject(sql);
			new OrgPartRelation(client1, product2, Both) { OU_ClientUQ = "UNT" }.AppendInsertAndReturnObject(sql);
			CreateSOH(sql, whs1, client1, product2);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			AssertNoExceptionThrown(
				() => OrgPartRelation
				.DeleteInDB(TestConnection, r => r.OU_OP == product1.PK));

			AssertExceptionThrown(typeof(SqlException), "Cannot change or delete the Owner of a Product with existing transactions.",
				() => OrgPartRelation
				.DeleteInDB(TestConnection, r => r.OU_OH == client1.PK && r.OU_OP == product2.PK), true);
		}

		public void TestTG_PreventChangingOfOwnerOnProductWithTransactions_RemoveRelationship_OneProductTwoOwners()
		{
			var sql = new SqlQueryBuilder();
			var branch1 = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs1 = new WhsWarehouse("WH1", branch1.PK).WithDockDoor(TestConnection);
			var client1 = new OrgHeader("CLIENT1").AppendInsertAndReturnObject(sql);
			var client2 = new OrgHeader("CLIENT2").AppendInsertAndReturnObject(sql);
			var product1 = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(client1, product1, Owner) { OU_ClientUQ = "UNT" }.AppendInsertAndReturnObject(sql);
			new OrgPartRelation(client2, product1, Owner) { OU_ClientUQ = "UNT" }.AppendInsertAndReturnObject(sql);
			CreateSOH(sql, whs1, client2, product1);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			// Should be able to delete - no transactions for productPK1 / clientPK1
			AssertNoExceptionThrown(
				() => OrgPartRelation
				.DeleteInDB(TestConnection, r => r.OU_OH == client1.PK && r.OU_OP == product1.PK));

			// Should FAIL to delete - there is a transaction for productPK1 / clientPK2
			AssertExceptionThrown(typeof(SqlException), "Cannot change or delete the Owner of a Product with existing transactions.",
				() => OrgPartRelation
				.DeleteInDB(TestConnection, r => r.OU_OH == client2.PK && r.OU_OP == product1.PK), true);
		}

		public void TestTG_AllowChangingOfRelationOnProductWithTransactions_IfDuplicateOwners()
		{
			var sql = new SqlQueryBuilder();
			var branch1 = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs1 = new WhsWarehouse("WH1", branch1.PK).WithDockDoor(TestConnection);
			var client1 = new OrgHeader("CLIENT1").AppendInsertAndReturnObject(sql);
			var product1 = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);

			sql.Append(";disable trigger TG_OrgPartRelation_DuplicateCheck on OrgPartRelation;");
			var relation1 = new OrgPartRelation(client1, product1, Owner) { OU_ClientUQ = "UNT" }.AppendInsertAndReturnObject(sql);
			var relation2 = new OrgPartRelation(client1, product1, Both) { OU_ClientUQ = "UNT" }.AppendInsertAndReturnObject(sql);
			sql.Append(";enable trigger TG_OrgPartRelation_DuplicateCheck on OrgPartRelation;");
			CreateSOH(sql, whs1, client1, product1);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			// Should be able to delete - another owner relation still exists after delete
			var updateSQL = $@"
				DELETE FROM
					dbo.OrgPartRelation
				WHERE
					OU_PK ='{relation1.PK}'";
			AssertNoExceptionThrown(
				() => OrgPartRelation
				.DeleteInDB(TestConnection, relation1.PK));

			// Should FAIL to delete - at least one owner relation should remain
			updateSQL = $@"
				DELETE FROM
					dbo.OrgPartRelation
				WHERE
					OU_PK ='{relation2.PK}'";
			AssertExceptionThrown(typeof(SqlException), "Cannot change or delete the Owner of a Product with existing transactions.", 
				() => OrgPartRelation
				.DeleteInDB(TestConnection, relation2.PK), true);
		}

		#endregion

		#region Implementation

		void CreateSOH(SqlQueryBuilder sql, WhsWarehouse whs, OrgHeader client, OrgSupplierPart product)
		{
			var now = DateTime.Now;
			var area1 = new WhsArea(whs.PK, Guid.NewGuid().ToString().Substring(0, 25)).AppendInsertAndReturnObject(sql);
			var row1 = new WhsRow(whs, Guid.NewGuid().ToString().Substring(0, 25)).AppendInsertAndReturnObject(sql);
			var locationA1 = new WhsLocation(row1.PK, area1.PK, area1.PK).AppendInsertAndReturnObject(sql);

			var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", Guid.NewGuid().ToString().Substring(0, 15)) { WD_FinalisedDate = now }.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, product.PK, 1m, locationA1.PK) { WE_DocketLineStatus = "FIN", WE_FinalisedDate = now, WE_StockOnHand = 1m }.AppendInsertAndReturnObject(sql);
		}

		const string Both = "BTH";
		const string Owner = "OWN";
		const string Supplier = "SUP";
		#endregion
	}
}

