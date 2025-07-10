using System;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_WhsSerialNumber_PreventUpdateClientProductSerialNumber))]
	class TG_WhsSerialNumber_PreventUpdateClientProductSerialNumberTest : DBCreateTriggerScriptTest
	{
		#region Test_TG_WhsSerialNumber_PreventUpdateClientProductSerialNumber

		public void Test_TG_WhsSerialNumber_PreventUpdateClientProductSerialNumber_Valid()
		{
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WH1").WithDockDoor(TestConnection);

			var serialNumber = "Serial Number 1";
			var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			var whsSerialNumber = new WhsSerialNumber(client, product, serialNumber).AppendInsertAndReturnObject(sql);
	
			AssertNoExceptionThrown("Should save without issue.", () => SaveToDB(sql));

			var updateBuilder = WhsSerialNumber.UpdateWhere(whsSerialNumber.PK)
				.Set(s => s.WSN_OH_Client, client)
				.Set(s => s.WSN_OP_Product, product)
				.Set(s => s.WSN_SerialNumber, serialNumber);

			AssertNoExceptionThrown("Should save without issue.", () => updateBuilder.Post(TestConnection));

			WhsSerialNumber.AssertFromDB(TestConnection, whsSerialNumber.PK)
				.ExpectEquals("WSN_OH_Client: ", i => i.WSN_OH_Client, client)
				.ExpectEquals("WSN_OP_Product: ", i => i.WSN_OP_Product, product)
				.ExpectEquals("WSN_SerialNumber: ", i => i.WSN_SerialNumber, serialNumber)
				.VerifyAll();
		}

		public void Test_TG_WhsSerialNumber_PreventUpdateClientProductSerialNumber_Client()
		{
			Test_TG_WhsSerialNumber_PreventUpdateClientProductSerialNumber_Core((otherClient, _) => $"WSN_OH_Client = '{otherClient.PK}'");
		}

		public void Test_TG_WhsSerialNumber_PreventUpdateClientProductSerialNumber_Product()
		{
			Test_TG_WhsSerialNumber_PreventUpdateClientProductSerialNumber_Core((_, otherProduct) => $"WSN_OP_Product = '{otherProduct.PK}'");
		}

		public void Test_TG_WhsSerialNumber_PreventUpdateClientProductSerialNumber_SerialNumber()
		{
			Test_TG_WhsSerialNumber_PreventUpdateClientProductSerialNumber_Core((_, _) => $"WSN_SerialNumber = 'SN2'");
		}

		void Test_TG_WhsSerialNumber_PreventUpdateClientProductSerialNumber_Core(Func<OrgHeader, OrgSupplierPart, string> getUpdateClause)
		{
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WH1").WithDockDoor(TestConnection);

			var serialNumber = "SN1";
			var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
			var otherClient = new OrgHeader("CLIENT Other").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			var otherProduct = new OrgSupplierPart("P2").AppendInsertAndReturnObject(sql);
			var whsSerialNumber = new WhsSerialNumber(client, product, serialNumber).AppendInsertAndReturnObject(sql);

			AssertNoExceptionThrown("Should save without issue.", () => SaveToDB(sql));

			WhsSerialNumber.AssertFromDB(TestConnection, whsSerialNumber.PK)
				.ExpectEquals("WSN_OH_Client: ", i => i.WSN_OH_Client, client)
				.ExpectEquals("WSN_OP_Product: ", i => i.WSN_OP_Product, product)
				.ExpectEquals("WSN_SerialNumber: ", i => i.WSN_SerialNumber, serialNumber)
				.VerifyAll();

			var update = getUpdateClause(otherClient, otherProduct);
			var updateSQL = new SqlQueryBuilder($"UPDATE dbo.WhsSerialNumber SET {update} WHERE WSN_PK = '{whsSerialNumber.PK}'");
			AssertExceptionThrown("Trigger should prevent updated.", typeof(SqlException), TriggerErrorMessage, () => SaveToDB(updateSQL), assertStartsWith: true);
		}

		public void Test_TG_WhsSerialNumber_PreventUpdateClientProductSerialNumber_CanChangeUnRelatedFields()
		{
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WH1").WithDockDoor(TestConnection);

			var serialNumber = "Serial Number 1";
			var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			var whsSerialNumber = new WhsSerialNumber(client, product, serialNumber).AppendInsertAndReturnObject(sql);

			AssertNoExceptionThrown("Should save without issue.", () => SaveToDB(sql));

			WhsSerialNumber.AssertFromDB(TestConnection, whsSerialNumber.PK)
				.ExpectEquals("WSN_OH_Client: ", i => i.WSN_OH_Client, client)
				.ExpectEquals("WSN_OP_Product: ", i => i.WSN_OP_Product, product)
				.ExpectEquals("WSN_SerialNumber: ", i => i.WSN_SerialNumber, serialNumber)
				.ExpectEquals("WSN_OP_Product: ", i => i.WSN_IsInUse, true)
				.VerifyAll("Precondition");

			var updateBuilder = WhsSerialNumber.UpdateWhere(whsSerialNumber.PK)
				.Set(s => s.WSN_IsInUse, false)
				.Set(s => s.WSN_SerialNumber, serialNumber);

			AssertNoExceptionThrown("Should updated without issue.", () => updateBuilder.Post(TestConnection));

			WhsSerialNumber.AssertFromDB(TestConnection, whsSerialNumber.PK)
				.ExpectEquals("WSN_OH_Client: ", i => i.WSN_OH_Client, client)
				.ExpectEquals("WSN_OP_Product: ", i => i.WSN_OP_Product, product)
				.ExpectEquals("WSN_SerialNumber: ", i => i.WSN_SerialNumber, serialNumber)
				.ExpectEquals("WSN_OP_Product: ", i => i.WSN_IsInUse, false)
				.VerifyAll();
		}

		#endregion

		#region Helper

		void SaveToDB(SqlQueryBuilder sql) => TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

		string TriggerErrorMessage => "Attempt to update Client or Product or Serial Number columns which is not allowed.";

		#endregion
	}
}
