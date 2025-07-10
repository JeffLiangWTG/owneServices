using System;
using CargoWise.Data;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers.CheckProcedures;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Testing
{
	[TestedType(typeof(WhsCheckSerialNumberMatchWithDocketLineQuantity))]
	class WhsCheckSerialNumberMatchWithDocketLineQuantityTest : DbCreateScriptTest
	{
		public void Test_WhsCheckSerialNumberMatchWithDocketLineQuantity_NoSerilNumber()
		{
			Test_WhsCheckSerialNumberMatchWithDocketLineQuantity_InvalidSerilNumber_Core(numberOfSN: 0);
		}

		public void Test_WhsCheckSerialNumberMatchWithDocketLineQuantity_Less()
		{
			Test_WhsCheckSerialNumberMatchWithDocketLineQuantity_InvalidSerilNumber_Core(numberOfSN: 1);
		}

		public void Test_WhsCheckSerialNumberMatchWithDocketLineQuantity_Exact()
		{
			Test_WhsCheckSerialNumberMatchWithDocketLineQuantity_InvalidSerilNumber_Core(numberOfSN: 2);
		}

		public void Test_WhsCheckSerialNumberMatchWithDocketLineQuantity_More()
		{
			Test_WhsCheckSerialNumberMatchWithDocketLineQuantity_InvalidSerilNumber_Core(numberOfSN: 3);
		}

		void Test_WhsCheckSerialNumberMatchWithDocketLineQuantity_InvalidSerilNumber_Core(int numberOfSN)
		{
			var today = DateTime.UtcNow.ToSmallDateTimeFloor();
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WHS", "PRW", branch.PK).WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);
			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Product").AppendInsertAndReturnObject(sql);
			var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, product.PK, 1m, location.PK) { WE_StockOnHand = 1m, WE_DocketLineStatus = "FIN", WE_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			for (int i = 0; i < numberOfSN; i++)
			{
				var whsSerialNumber = new WhsSerialNumber(client, product, $"SN{i}").AppendInsertAndReturnObject(sql);
				new WhsSerialNumberPivot(receiveLine.PK, WhsDocketLineSchema.Constants.Prefix, whsSerialNumber.PK).AppendInsertAndReturnObject(sql);
			}

			AssertNoExceptionThrown(() => Save(sql));

			var expectAnException = (numberOfSN > 0 && numberOfSN != receiveLine.WE_TransactionQuantity); // Exception when is not Zero and Not Match
			RunCheckProcedure(new[] { receiveLine.PK }, expectAnException);
		}

		public void Test_WhsCheckSerialNumberMatchWithDocketLineQuantity_NotFinalised()
		{
			var today = DateTime.UtcNow.ToSmallDateTimeFloor();
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WHS", "PRW", branch.PK).WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);
			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Product").AppendInsertAndReturnObject(sql);
			var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "ENT", "R1").AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, product.PK, 10m, location.PK) { WE_StockOnHand = 10m, WE_CurrentInventoryStatus = "PUT", WE_OriginalInventoryStatus = "PUT" }.AppendInsertAndReturnObject(sql);
			var whsSerialNumber = new WhsSerialNumber(client, product, "SN1").AppendInsertAndReturnObject(sql);
			new WhsSerialNumberPivot(receiveLine.PK, WhsDocketLineSchema.Constants.Prefix, whsSerialNumber.PK).AppendInsertAndReturnObject(sql);

			AssertNoExceptionThrown(() => Save(sql));

			RunCheckProcedure(new[] { receiveLine.PK }, expectAnException: false);
		}

		#region Helper

		void Save(SqlQueryBuilder sql)
		{
			using (TestWhsDataSetupHelper.SuspendTrigger(nameof(WhsCheckSerialNumberMatchWithDocketLineQuantity), WhsDocketLineSchema.Constants.TableName, TestConnection))
			{
				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}
		}

		void RunCheckProcedure(Guid[] docketLinePKs, bool expectAnException)
		{
			const string sql = "EXEC dbo.WhsCheckSerialNumberMatchWithDocketLineQuantity @WhsDocketLinePKs";
			using (var sqlCommand = Db.Connection.Command(sql))
			{
				sqlCommand.AddTableValuedParameter("@WhsDocketLinePKs", "dbo.TVP_uniqueidentifier", docketLinePKs);
				if (expectAnException)
				{
					AssertExceptionThrown(typeof(SqlException), () => sqlCommand.ExecuteNonQuery());
				}
				else
				{
					AssertNoExceptionThrown(() => sqlCommand.ExecuteNonQuery());
				}
			}
		}

		string TriggerErrorMessage => "Attempt to change finalise docket line where the quantity does not match the number of serial numbers.";

		#endregion
	}
}
