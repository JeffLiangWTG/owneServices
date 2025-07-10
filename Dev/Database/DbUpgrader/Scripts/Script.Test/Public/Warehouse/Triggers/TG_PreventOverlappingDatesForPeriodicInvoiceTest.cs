using System;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_PreventOverlappingDatesForPeriodicInvoice))]
	class TG_PreventOverlappingDatesForPeriodicInvoiceTest : DBCreateTriggerScriptTest
	{
	}

	[UseSnapshotProtection]
	class Trigger_TG_PreventOverlappingDatesForPeriodicInvoice : TestCase
	{
		#region TestTrigger_Create

		public void TestTrigger_Create_NotOverlapped_MustAllow()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch = new GlbBranch("BR1").InsertAndReturnObject(connection);
				var whs = new WhsWarehouse("WH1", "PRW", branch.PK).WithDockDoor(connection);
				var client = new OrgHeader("C1").AppendInsertAndReturnObject(sql);
				var invoice1 = new JobStorage("I1", whs, client, new DateTime(2017, 5, 1), new DateTime(2017, 5, 10)).AppendInsertAndReturnObject(sql);
				var invoice2 = new JobStorage("I2", whs, client, new DateTime(2017, 5, 11), new DateTime(2017, 5, 14)).AppendInsertAndReturnObject(sql);

				AssertNoExceptionThrown("Trigger should allow since there are no overlapping dates.", () => SaveToDB(connection, sql));

				JobStorage.AssertFromDB(connection, invoice1.PK)
					.ExpectEquals("ET_WW: ", i => i.ET_WW, whs)
					.ExpectEquals("ET_OH_Client: ", i => i.ET_OH_Client, client)
					.ExpectEquals("ET_StorageFromDate: ", i => i.ET_StorageFromDate, new DateTime(2017, 5, 1))
					.ExpectEquals("ET_StorageToDate: ", i => i.ET_StorageToDate, new DateTime(2017, 5, 10))
					.VerifyAll();

				JobStorage.AssertFromDB(connection, invoice2.PK)
					.ExpectEquals("ET_WW: ", i => i.ET_WW, whs)
					.ExpectEquals("ET_OH_Client: ", i => i.ET_OH_Client, client)
					.ExpectEquals("ET_StorageFromDate: ", i => i.ET_StorageFromDate, new DateTime(2017, 5, 11))
					.ExpectEquals("ET_StorageToDate: ", i => i.ET_StorageToDate, new DateTime(2017, 5, 14))
					.VerifyAll();
			}
		}

		public void TestTrigger_Create_Overlapped_MustPrevent()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch = new GlbBranch("BR1").InsertAndReturnObject(connection);
				var whs = new WhsWarehouse("WH1", "PRW", branch.PK).WithDockDoor(connection);
				var client = new OrgHeader("C1").AppendInsertAndReturnObject(sql);
				var invoice1 = new JobStorage("I1", whs, client, new DateTime(2017, 5, 1), new DateTime(2017, 5, 7)).AppendInsertAndReturnObject(sql);
				var invoice2 = new JobStorage("I2", whs, client, new DateTime(2017, 5, 1), new DateTime(2017, 5, 7)).AppendInsertAndReturnObject(sql);

				AssertExceptionThrown("Trigger should prevent overlapping dates", typeof(SqlException), TriggerErrorMessage, () => SaveToDB(connection, sql), assertStartsWith: true);
			}
		}

		public void TestTrigger_Create_Overlapped_OnlyStartDate_MustPrevent()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch = new GlbBranch("BR1").InsertAndReturnObject(connection);
				var whs = new WhsWarehouse("WH1", "PRW", branch.PK).WithDockDoor(connection);
				var client = new OrgHeader("C1").AppendInsertAndReturnObject(sql);
				var invoice1 = new JobStorage("I1", whs, client, new DateTime(2017, 5, 1), new DateTime(2017, 5, 7)).AppendInsertAndReturnObject(sql);
				var invoice2 = new JobStorage("I2", whs, client, new DateTime(2017, 5, 7), new DateTime(2017, 5, 13)).AppendInsertAndReturnObject(sql);

				AssertExceptionThrown("Trigger should prevent overlapped start dates", typeof(SqlException), TriggerErrorMessage, () => SaveToDB(connection, sql), assertStartsWith: true);
			}
		}

		public void TestTrigger_Create_Overlapped_OnlyFinishDate_MustPrevent()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch = new GlbBranch("BR1").InsertAndReturnObject(connection);
				var whs = new WhsWarehouse("WH1", "PRW", branch.PK).WithDockDoor(connection);
				var client = new OrgHeader("C1").AppendInsertAndReturnObject(sql);
				var invoice1 = new JobStorage("I1", whs, client, new DateTime(2017, 5, 1), new DateTime(2017, 5, 7)).AppendInsertAndReturnObject(sql);
				var invoice2 = new JobStorage("I2", whs, client, new DateTime(2017, 4, 25), new DateTime(2017, 5, 1)).AppendInsertAndReturnObject(sql);

				AssertExceptionThrown("Trigger should prevent overlapped finish dates", typeof(SqlException), TriggerErrorMessage, () => SaveToDB(connection, sql), assertStartsWith: true);
			}
		}

		public void TestTrigger_Create_Overlapped_FirstInvoiceContainedWithinSecond_MustPrevent()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch = new GlbBranch("BR1").InsertAndReturnObject(connection);
				var whs = new WhsWarehouse("WH1", "PRW", branch.PK).WithDockDoor(connection);
				var client = new OrgHeader("C1").AppendInsertAndReturnObject(sql);
				var invoice1 = new JobStorage("I1", whs, client, new DateTime(2017, 5, 2), new DateTime(2017, 5, 4)).AppendInsertAndReturnObject(sql);
				var invoice2 = new JobStorage("I2", whs, client, new DateTime(2017, 4, 30), new DateTime(2017, 5, 6)).AppendInsertAndReturnObject(sql);

				AssertExceptionThrown("Trigger should prevent invoice with dates containing existing invoice dates.", typeof(SqlException), TriggerErrorMessage, () => SaveToDB(connection, sql), assertStartsWith: true);
			}
		}

		public void TestTrigger_Create_Overlapped_SecondInvoiceContainedWithinFirst_MustPrevent()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch = new GlbBranch("BR1").InsertAndReturnObject(connection);
				var whs = new WhsWarehouse("WH1", "PRW", branch.PK).WithDockDoor(connection);
				var client = new OrgHeader("C1").AppendInsertAndReturnObject(sql);
				var invoice1 = new JobStorage("I1", whs, client, new DateTime(2017, 5, 1), new DateTime(2017, 5, 7)).AppendInsertAndReturnObject(sql);
				var invoice2 = new JobStorage("I2", whs, client, new DateTime(2017, 5, 3), new DateTime(2017, 5, 5)).AppendInsertAndReturnObject(sql);

				AssertExceptionThrown("Trigger should prevent invoice with dates contained within existing invoice.", typeof(SqlException), TriggerErrorMessage, () => SaveToDB(connection, sql), assertStartsWith: true);
			}
		}

		public void TestTrigger_Create_Overlapped_DifferentClients_MustAllow()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch = new GlbBranch("BR1").InsertAndReturnObject(connection);
				var whs = new WhsWarehouse("WH1", "PRW", branch.PK).WithDockDoor(connection);
				var client1 = new OrgHeader("C1").AppendInsertAndReturnObject(sql);
				var client2 = new OrgHeader("C2").AppendInsertAndReturnObject(sql);
				var invoice1 = new JobStorage("I1", whs, client1, new DateTime(2017, 5, 1), new DateTime(2017, 5, 10)).AppendInsertAndReturnObject(sql);
				var invoice2 = new JobStorage("I2", whs, client2, new DateTime(2017, 5, 1), new DateTime(2017, 5, 10)).AppendInsertAndReturnObject(sql);

				AssertNoExceptionThrown("Trigger should allow creation since clients are different.", () => SaveToDB(connection, sql));

				JobStorage.AssertFromDB(connection, invoice1.PK)
					.ExpectEquals("ET_WW: ", i => i.ET_WW, whs)
					.ExpectEquals("ET_OH_Client: ", i => i.ET_OH_Client, client1)
					.ExpectEquals("ET_StorageFromDate: ", i => i.ET_StorageFromDate, new DateTime(2017, 5, 1))
					.ExpectEquals("ET_StorageToDate: ", i => i.ET_StorageToDate, new DateTime(2017, 5, 10))
					.VerifyAll();

				JobStorage.AssertFromDB(connection, invoice2.PK)
					.ExpectEquals("ET_WW: ", i => i.ET_WW, whs)
					.ExpectEquals("ET_OH_Client: ", i => i.ET_OH_Client, client2)
					.ExpectEquals("ET_StorageFromDate: ", i => i.ET_StorageFromDate, new DateTime(2017, 5, 1))
					.ExpectEquals("ET_StorageToDate: ", i => i.ET_StorageToDate, new DateTime(2017, 5, 10))
					.VerifyAll();
			}
		}

		public void TestTrigger_Create_Overlapped_DifferentWhs_MustAllow()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch1 = new GlbBranch("BR1").InsertAndReturnObject(connection);
				var branch2 = new GlbBranch("BR2").InsertAndReturnObject(connection);
				var whs1 = new WhsWarehouse("WH1", "PRW", branch1.PK).WithDockDoor(connection);
				var whs2 = new WhsWarehouse("WH2", "PRW", branch2.PK).WithDockDoor(connection);
				var client = new OrgHeader("C1").AppendInsertAndReturnObject(sql);
				var invoice1 = new JobStorage("I1", whs1, client, new DateTime(2017, 5, 1), new DateTime(2017, 5, 10)).AppendInsertAndReturnObject(sql);
				var invoice2 = new JobStorage("I2", whs2, client, new DateTime(2017, 5, 1), new DateTime(2017, 5, 10)).AppendInsertAndReturnObject(sql);

				AssertNoExceptionThrown("Trigger should allow creation since warehouses are different.", () => SaveToDB(connection, sql));

				JobStorage.AssertFromDB(connection, invoice1.PK)
					.ExpectEquals("ET_WW: ", i => i.ET_WW, whs1)
					.ExpectEquals("ET_OH_Client: ", i => i.ET_OH_Client, client)
					.ExpectEquals("ET_StorageFromDate: ", i => i.ET_StorageFromDate, new DateTime(2017, 5, 1))
					.ExpectEquals("ET_StorageToDate: ", i => i.ET_StorageToDate, new DateTime(2017, 5, 10))
					.VerifyAll();

				JobStorage.AssertFromDB(connection, invoice2.PK)
					.ExpectEquals("ET_WW: ", i => i.ET_WW, whs2)
					.ExpectEquals("ET_OH_Client: ", i => i.ET_OH_Client, client)
					.ExpectEquals("ET_StorageFromDate: ", i => i.ET_StorageFromDate, new DateTime(2017, 5, 1))
					.ExpectEquals("ET_StorageToDate: ", i => i.ET_StorageToDate, new DateTime(2017, 5, 10))
					.VerifyAll();
			}
		}

		public void TestTrigger_Create_NullDates()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch = new GlbBranch("BR1").InsertAndReturnObject(connection);
				var whs = new WhsWarehouse("WH1", "PRW", branch.PK).WithDockDoor(connection);
				var client = new OrgHeader("C1").AppendInsertAndReturnObject(sql);
				var invoice1 = new JobStorage("I1", whs, client, null, new DateTime(2017, 5, 5)).AppendInsertAndReturnObject(sql);
				var invoice2 = new JobStorage("I2", whs, client, new DateTime(2017, 5, 5), new DateTime(2017, 5, 9)).AppendInsertAndReturnObject(sql);

				AssertNoExceptionThrown("Invoices should be saved since they have empty dates not evaluated by the trigger.", () => SaveToDB(connection, sql));

				JobStorage.AssertFromDB(connection, invoice1.PK)
					.ExpectEquals("Precondition: Expected empty date.", i => i.ET_StorageFromDate, null)
					.VerifyAll();
			}
		}

		public void TestTrigger_Create_ManyInvoices()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch = new GlbBranch("BR1").InsertAndReturnObject(connection);
				var whs = new WhsWarehouse("WH1", "PRW", branch.PK).WithDockDoor(connection);
				var client = new OrgHeader("C1").AppendInsertAndReturnObject(sql);
				var invoice1 = new JobStorage("I1", whs, client, null, new DateTime(2017, 5, 5)).AppendInsertAndReturnObject(sql);
				var invoice2 = new JobStorage("I2", whs, client, new DateTime(2016, 5, 1), new DateTime(2016, 5, 7)).AppendInsertAndReturnObject(sql);
				var invoice3 = new JobStorage("I3", whs, client, new DateTime(2017, 5, 1), new DateTime(2017, 5, 7)).AppendInsertAndReturnObject(sql);
				var invoice4 = new JobStorage("I4", whs, client, new DateTime(2017, 6, 1), new DateTime(2017, 6, 7)).AppendInsertAndReturnObject(sql);
				var invoice5 = new JobStorage("I5", whs, client, new DateTime(2017, 5, 8), new DateTime(2017, 5, 14)).AppendInsertAndReturnObject(sql);

				AssertNoExceptionThrown("Expected no errors since invoices have no overlapping dates.", () => SaveToDB(connection, sql));
			}
		}

		#endregion

		#region TestTrigger_Update

		#region TestTrigger_Update_FromDate

		public void TestTrigger_Update_FromDate_NotOverlapped_MustAllow()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch = new GlbBranch("BR1").InsertAndReturnObject(connection);
				var whs = new WhsWarehouse("WH1", "PRW", branch.PK).WithDockDoor(connection);
				var client = new OrgHeader("C1").AppendInsertAndReturnObject(sql);
				var invoice1 = new JobStorage("I1", whs, client, new DateTime(2017, 5, 1), new DateTime(2017, 5, 10)).AppendInsertAndReturnObject(sql);
				var invoice2 = new JobStorage("I2", whs, client, new DateTime(2017, 5, 11), new DateTime(2017, 5, 14)).AppendInsertAndReturnObject(sql);

				AssertNoExceptionThrown("Prerequisite: Trigger should allow since there are no overlapping dates.", () => SaveToDB(connection, sql));

				var updateSQL = new SqlQueryBuilder(@$"
UPDATE dbo.JobStorage
SET
	ET_StorageFromDate = '2017-5-13 00:00:00',
	ET_SystemLastEditTimeUtc = GETUTCDATE(),
	ET_SystemLastEditUser = '~BP'
WHERE
	ET_PK = '{invoice2.PK}'");
				AssertNoExceptionThrown("Trigger should allow update since there are no overlapping dates.", () => SaveToDB(connection, updateSQL));

				JobStorage.AssertFromDB(connection, invoice2.PK)
					.ExpectEquals("ET_WW: ", i => i.ET_WW, whs)
					.ExpectEquals("ET_OH_Client: ", i => i.ET_OH_Client, client)
					.ExpectEquals("ET_StorageFromDate: ", i => i.ET_StorageFromDate, new DateTime(2017, 5, 13))
					.ExpectEquals("ET_StorageToDate: ", i => i.ET_StorageToDate, new DateTime(2017, 5, 14))
					.VerifyAll();
			}
		}

		public void TestTrigger_Update_FromDate_Overlapped_MustPrevent()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch = new GlbBranch("BR1").InsertAndReturnObject(connection);
				var whs = new WhsWarehouse("WH1", "PRW", branch.PK).WithDockDoor(connection);
				var client = new OrgHeader("C1").AppendInsertAndReturnObject(sql);
				var invoice1 = new JobStorage("I1", whs, client, new DateTime(2017, 5, 1), new DateTime(2017, 5, 10)).AppendInsertAndReturnObject(sql);
				var invoice2 = new JobStorage("I2", whs, client, new DateTime(2017, 5, 11), new DateTime(2017, 5, 14)).AppendInsertAndReturnObject(sql);

				AssertNoExceptionThrown("Prerequisite: Trigger should allow since there are no overlapping dates.", () => SaveToDB(connection, sql));

				var updateSQL = new SqlQueryBuilder(@$"
UPDATE dbo.JobStorage
SET
	ET_StorageFromDate = '2017-5-7 00:00:00',
	ET_SystemLastEditTimeUtc = GETUTCDATE(),
	ET_SystemLastEditUser = '~BP'
WHERE
	ET_PK = '{invoice2.PK}'");
				AssertExceptionThrown("Trigger should prevent updated overlapping start date.", typeof(SqlException), TriggerErrorMessage, () => SaveToDB(connection, updateSQL), assertStartsWith: true);
			}
		}

		#endregion

		#region TestTrigger_Update_ToDate

		public void TestTrigger_Update_ToDate_NotOverlapped_MustAllow()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch = new GlbBranch("BR1").InsertAndReturnObject(connection);
				var whs = new WhsWarehouse("WH1", "PRW", branch.PK).WithDockDoor(connection);
				var client = new OrgHeader("C1").AppendInsertAndReturnObject(sql);
				var invoice1 = new JobStorage("I1", whs, client, new DateTime(2017, 5, 11), new DateTime(2017, 5, 14)).AppendInsertAndReturnObject(sql);
				var invoice2 = new JobStorage("I2", whs, client, new DateTime(2017, 5, 1), new DateTime(2017, 5, 10)).AppendInsertAndReturnObject(sql);

				AssertNoExceptionThrown("Prerequisite: Trigger should allow since there are no overlapping finish dates.", () => SaveToDB(connection, sql));

				var updateSQL = new SqlQueryBuilder(@$"
UPDATE dbo.JobStorage
SET
	ET_StorageToDate = '2017-5-7 00:00:00',
	ET_SystemLastEditTimeUtc = GETUTCDATE(),
	ET_SystemLastEditUser = '~BP'
WHERE
	ET_PK = '{invoice2.PK}'");
				AssertNoExceptionThrown("Trigger should allow update since there are no overlapping finish dates.", () => SaveToDB(connection, updateSQL));

				JobStorage.AssertFromDB(connection, invoice2.PK)
					.ExpectEquals("ET_WW: ", i => i.ET_WW, whs)
					.ExpectEquals("ET_OH_Client: ", i => i.ET_OH_Client, client)
					.ExpectEquals("ET_StorageFromDate: ", i => i.ET_StorageFromDate, new DateTime(2017, 5, 1))
					.ExpectEquals("ET_StorageToDate: ", i => i.ET_StorageToDate, new DateTime(2017, 5, 7))
					.VerifyAll();
			}
		}

		public void TestTrigger_Update_ToDate_Overlapped_MustPrevent()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch = new GlbBranch("BR1").InsertAndReturnObject(connection);
				var whs = new WhsWarehouse("WH1", "PRW", branch.PK).WithDockDoor(connection);
				var client = new OrgHeader("C1").AppendInsertAndReturnObject(sql);
				var invoice1 = new JobStorage("I1", whs, client, new DateTime(2017, 5, 11), new DateTime(2017, 5, 14)).AppendInsertAndReturnObject(sql);
				var invoice2 = new JobStorage("I2", whs, client, new DateTime(2017, 5, 7), new DateTime(2017, 5, 10)).AppendInsertAndReturnObject(sql);

				AssertNoExceptionThrown("Prerequisite: Trigger should allow since there are no overlapping finish dates.", () => SaveToDB(connection, sql));

				var updateSQL = new SqlQueryBuilder(@$"
UPDATE dbo.JobStorage
SET
	ET_StorageToDate = '2017-5-12 00:00:00',
	ET_SystemLastEditTimeUtc = GETUTCDATE(),
	ET_SystemLastEditUser = '~BP'
WHERE
	ET_PK = '{invoice2.PK}'");
				AssertExceptionThrown("Trigger should prevent updated overlapping finish date.", typeof(SqlException), TriggerErrorMessage, () => SaveToDB(connection, updateSQL), assertStartsWith: true);
			}
		}

		#endregion

		#region TestTrigger_Update_Whs

		public void TestTrigger_Update_Whs_NotOverlapped_MustAllow()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch1 = new GlbBranch("BR1").InsertAndReturnObject(connection);
				var branch2 = new GlbBranch("BR2").InsertAndReturnObject(connection);
				var whs1 = new WhsWarehouse("WH1", "PRW", branch1.PK).WithDockDoor(connection);
				var whs2 = new WhsWarehouse("WH2", "PRW", branch2.PK).WithDockDoor(connection);
				var client = new OrgHeader("C1").AppendInsertAndReturnObject(sql);
				var invoice1 = new JobStorage("I1", whs1, client, new DateTime(2017, 5, 1), new DateTime(2017, 5, 5)).AppendInsertAndReturnObject(sql);
				var invoice2 = new JobStorage("I2", whs2, client, new DateTime(2017, 5, 6), new DateTime(2017, 5, 9)).AppendInsertAndReturnObject(sql);

				AssertNoExceptionThrown("Prerequisite: Trigger should allow since there are no overlapping dates.", () => SaveToDB(connection, sql));

				var updateSQL = new SqlQueryBuilder(@$"
UPDATE dbo.JobStorage
SET
	ET_WW = '{whs1.PK}',
	ET_SystemLastEditTimeUtc = GETUTCDATE(),
	ET_SystemLastEditUser = '~BP'
WHERE
	ET_PK = '{invoice2.PK}'");
				AssertNoExceptionThrown("Trigger should allow update since warehouses are different.", () => SaveToDB(connection, updateSQL));

				JobStorage.AssertFromDB(connection, invoice2.PK)
					.ExpectEquals("ET_WW: ", i => i.ET_WW, whs1)
					.ExpectEquals("ET_OH_Client: ", i => i.ET_OH_Client, client)
					.ExpectEquals("ET_StorageFromDate: ", i => i.ET_StorageFromDate, new DateTime(2017, 5, 6))
					.ExpectEquals("ET_StorageToDate: ", i => i.ET_StorageToDate, new DateTime(2017, 5, 9))
					.VerifyAll();
			}
		}

		public void TestTrigger_Update_Whs_Overlapped_MustPrevent()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch1 = new GlbBranch("BR1").InsertAndReturnObject(connection);
				var branch2 = new GlbBranch("BR2").InsertAndReturnObject(connection);
				var whs1 = new WhsWarehouse("WH1", "PRW", branch1.PK).WithDockDoor(connection);
				var whs2 = new WhsWarehouse("WH2", "PRW", branch2.PK).WithDockDoor(connection);
				var client = new OrgHeader("C1").AppendInsertAndReturnObject(sql);
				var invoice1 = new JobStorage("I1", whs1, client, new DateTime(2017, 5, 1), new DateTime(2017, 5, 5)).AppendInsertAndReturnObject(sql);
				var invoice2 = new JobStorage("I2", whs2, client, new DateTime(2017, 5, 1), new DateTime(2017, 5, 5)).AppendInsertAndReturnObject(sql);

				AssertNoExceptionThrown("Prerequisite: Trigger should allow since there are no overlapping dates.", () => SaveToDB(connection, sql));

				var updateSQL = new SqlQueryBuilder($"UPDATE dbo.JobStorage SET ET_WW = '{whs1.PK}', ET_SystemLastEditTimeUtc = GETUTCDATE(), ET_SystemLastEditUser = '~BP' WHERE ET_PK = '{invoice2.PK}'");
				AssertExceptionThrown("Trigger should prevent updating WHS for invoice with overlapping dates.", typeof(SqlException), TriggerErrorMessage, () => SaveToDB(connection, updateSQL), assertStartsWith: true);
			}
		}

		#endregion

		#region TestTrigger_Update_Client

		public void TestTrigger_Update_Client_NotOverlapped_MustAllow()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch = new GlbBranch("BR1").InsertAndReturnObject(connection);
				var whs = new WhsWarehouse("WH1", "PRW", branch.PK).WithDockDoor(connection);
				var client1 = new OrgHeader("C1").AppendInsertAndReturnObject(sql);
				var client2 = new OrgHeader("C2").AppendInsertAndReturnObject(sql);
				var invoice1 = new JobStorage("I1", whs, client1, new DateTime(2017, 5, 1), new DateTime(2017, 5, 5)).AppendInsertAndReturnObject(sql);
				var invoice2 = new JobStorage("I2", whs, client2, new DateTime(2017, 5, 6), new DateTime(2017, 5, 9)).AppendInsertAndReturnObject(sql);

				AssertNoExceptionThrown("Prerequisite: Trigger should allow since there are no overlapping dates.", () => SaveToDB(connection, sql));

				var updateSQL = new SqlQueryBuilder(@$"
UPDATE dbo.JobStorage
SET
	ET_OH_Client = '{client1.PK}',
	ET_SystemLastEditTimeUtc = GETUTCDATE(),
	ET_SystemLastEditUser = '~BP'
WHERE
	ET_PK = '{invoice2.PK}'");
				AssertNoExceptionThrown("Trigger should allow update since dates are not overlapped.", () => SaveToDB(connection, updateSQL));

				JobStorage.AssertFromDB(connection, invoice2.PK)
					.ExpectEquals("ET_WW: ", i => i.ET_WW, whs)
					.ExpectEquals("ET_OH_Client: ", i => i.ET_OH_Client, client1)
					.ExpectEquals("ET_StorageFromDate: ", i => i.ET_StorageFromDate, new DateTime(2017, 5, 6))
					.ExpectEquals("ET_StorageToDate: ", i => i.ET_StorageToDate, new DateTime(2017, 5, 9))
					.VerifyAll();
			}
		}

		public void TestTrigger_Update_Client_Overlapped_MustPrevent()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch = new GlbBranch("BR1").InsertAndReturnObject(connection);
				var whs = new WhsWarehouse("WH1", "PRW", branch.PK).WithDockDoor(connection);
				var client1 = new OrgHeader("C1").AppendInsertAndReturnObject(sql);
				var client2 = new OrgHeader("C2").AppendInsertAndReturnObject(sql);
				var invoice1 = new JobStorage("I1", whs, client1, new DateTime(2017, 5, 1), new DateTime(2017, 5, 5)).AppendInsertAndReturnObject(sql);
				var invoice2 = new JobStorage("I2", whs, client2, new DateTime(2017, 5, 1), new DateTime(2017, 5, 5)).AppendInsertAndReturnObject(sql);

				AssertNoExceptionThrown("Prerequisite: Trigger should allow since there are no overlapping dates.", () => SaveToDB(connection, sql));

				var updateSQL = new SqlQueryBuilder($"UPDATE dbo.JobStorage SET ET_OH_Client = '{client1.PK}', ET_SystemLastEditTimeUtc = GETUTCDATE(), ET_SystemLastEditUser = '~BP' WHERE ET_PK = '{invoice2.PK}'");
				AssertExceptionThrown("Trigger should prevent updating Client for invoice with overlapping dates.", typeof(SqlException), TriggerErrorMessage, () => SaveToDB(connection, updateSQL), assertStartsWith: true);
			}
		}

		#endregion

		#region TestTrigger_Update_StorageType

		public void TestTrigger_Update_StorageType_NotOverlapped_MustAllow()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch = new GlbBranch("BR1").InsertAndReturnObject(connection);
				var whs = new WhsWarehouse("WH1", "PRW", branch.PK).WithDockDoor(connection);
				var client = new OrgHeader("C1").AppendInsertAndReturnObject(sql);
				var invoice1 = new JobStorage("I1", whs, client, new DateTime(2017, 5, 1), new DateTime(2017, 5, 5)) { ET_StorageType = "WHS" }.AppendInsertAndReturnObject(sql);
				var invoice2 = new JobStorage("I2", whs, client, new DateTime(2017, 5, 6), new DateTime(2017, 5, 9)) { ET_StorageType = "OTH" }.AppendInsertAndReturnObject(sql);

				AssertNoExceptionThrown("Prerequisite: Trigger should allow since there are no overlapping dates.", () => SaveToDB(connection, sql));

				var updateSQL = new SqlQueryBuilder(@$"
UPDATE dbo.JobStorage
SET
	ET_StorageType = 'WHS',
	ET_SystemLastEditTimeUtc = GETUTCDATE(),
	ET_SystemLastEditUser = '~BP'
WHERE
	ET_PK = '{invoice2.PK}'");
				AssertNoExceptionThrown("Trigger should allow update since dates are not overlapped.", () => SaveToDB(connection, updateSQL));

				JobStorage.AssertFromDB(connection, invoice2.PK)
					.ExpectEquals("ET_WW: ", i => i.ET_WW, whs)
					.ExpectEquals("ET_OH_Client: ", i => i.ET_OH_Client, client)
					.ExpectEquals("ET_StorageType: ", i => i.ET_StorageType, "WHS")
					.ExpectEquals("ET_StorageFromDate: ", i => i.ET_StorageFromDate, new DateTime(2017, 5, 6))
					.ExpectEquals("ET_StorageToDate: ", i => i.ET_StorageToDate, new DateTime(2017, 5, 9))
					.VerifyAll();
			}
		}

		public void TestTrigger_Update_StorageType_Overlapped_MustPrevent()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch = new GlbBranch("BR1").InsertAndReturnObject(connection);
				var whs = new WhsWarehouse("WH1", "PRW", branch.PK).WithDockDoor(connection);
				var client = new OrgHeader("C1").AppendInsertAndReturnObject(sql);
				var invoice1 = new JobStorage("I1", whs, client, new DateTime(2017, 5, 1), new DateTime(2017, 5, 5)) { ET_StorageType = "WHS" }.AppendInsertAndReturnObject(sql);
				var invoice2 = new JobStorage("I2", whs, client, new DateTime(2017, 5, 1), new DateTime(2017, 5, 5)) { ET_StorageType = "OTH" }.AppendInsertAndReturnObject(sql);

				AssertNoExceptionThrown("Prerequisite: Trigger should allow save since types are different.", () => SaveToDB(connection, sql));

				var updateSQL = new SqlQueryBuilder(@$"
UPDATE dbo.JobStorage
SET
	ET_StorageType = 'WHS',
	ET_SystemLastEditTimeUtc = GETUTCDATE(),
	ET_SystemLastEditUser = '~BP'
WHERE
	ET_PK = '{invoice2.PK}'");
				AssertExceptionThrown("Trigger should prevent updating Type for invoice with overlapping dates.", typeof(SqlException), TriggerErrorMessage, () => SaveToDB(connection, updateSQL), assertStartsWith: true);
			}
		}

		#endregion

		#region TestTrigger_Update_EdgeCases

		public void TestTrigger_Update_EdgeCases_OverlappedDateToNotOverlapped()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch = new GlbBranch("BR1").InsertAndReturnObject(connection);
				var whs = new WhsWarehouse("WH1", "PRW", branch.PK).WithDockDoor(connection);
				var client = new OrgHeader("C1").AppendInsertAndReturnObject(sql);
				var invoice1 = new JobStorage("I1", whs, client, new DateTime(2017, 5, 1), new DateTime(2017, 5, 5)).AppendInsertAndReturnObject(sql);
				var invoice2 = new JobStorage("I2", whs, client, new DateTime(2017, 5, 5), new DateTime(2017, 5, 9)).AppendInsertAndReturnObject(sql);

				using (TestWhsDataSetupHelper.SuspendTrigger("TG_PreventOverlappingDatesForPeriodicInvoice", JobStorageSchema.Constants.TableName, connection))
				{
					AssertNoExceptionThrown("Prerequisite: Since trigger is disabled, overlapping dates should be allowed.", () => SaveToDB(connection, sql));
				}

				JobStorage.AssertFromDB(connection, invoice1.PK)
					.ExpectEquals("ET_StorageFromDate: ", i => i.ET_StorageFromDate, new DateTime(2017, 5, 1))
					.ExpectEquals("ET_StorageToDate: ", i => i.ET_StorageToDate, new DateTime(2017, 5, 5))
					.VerifyAll();

				JobStorage.AssertFromDB(connection, invoice2.PK)
					.ExpectEquals("ET_StorageFromDate: ", i => i.ET_StorageFromDate, new DateTime(2017, 5, 5))
					.ExpectEquals("ET_StorageToDate: ", i => i.ET_StorageToDate, new DateTime(2017, 5, 9))
					.VerifyAll();

				var updateSQL = new SqlQueryBuilder(@$"
UPDATE dbo.JobStorage
SET
	ET_StorageFromDate = '2017-5-6 00:00:00',
	ET_SystemLastEditTimeUtc = GETUTCDATE(),
	ET_SystemLastEditUser = '~BP'
WHERE
	ET_PK = '{invoice2.PK}'");
				AssertNoExceptionThrown("Trigger should allow update of start date since there are no overlaps.", () => SaveToDB(connection, updateSQL));

				JobStorage.AssertFromDB(connection, invoice2.PK)
					.ExpectEquals("ET_StorageFromDate: ", i => i.ET_StorageFromDate, new DateTime(2017, 5, 6))
					.ExpectEquals("ET_StorageToDate: ", i => i.ET_StorageToDate, new DateTime(2017, 5, 9))
					.VerifyAll();
			}
		}

		#endregion

		#endregion

		#region TestTrigger_Delete

		public void TestTrigger_Delete_OverlappedDates()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch = new GlbBranch("BR1").InsertAndReturnObject(connection);
				var whs = new WhsWarehouse("WH1", "PRW", branch.PK).WithDockDoor(connection);
				var client = new OrgHeader("C1").AppendInsertAndReturnObject(sql);
				var invoice1 = new JobStorage("I1", whs, client, new DateTime(2017, 5, 1), new DateTime(2017, 5, 5)).AppendInsertAndReturnObject(sql);
				var invoice2 = new JobStorage("I2", whs, client, new DateTime(2017, 5, 1), new DateTime(2017, 5, 5)).AppendInsertAndReturnObject(sql);

				using (TestWhsDataSetupHelper.SuspendTrigger("TG_PreventOverlappingDatesForPeriodicInvoice", JobStorageSchema.Constants.TableName, connection))
				{
					AssertNoExceptionThrown("Prerequisite: Since trigger is disabled, overlapping dates should be allowed.", () => SaveToDB(connection, sql));
				}

				JobStorage.AssertFromDB(connection, invoice1.PK)
					.ExpectEquals("ET_StorageFromDate: ", i => i.ET_StorageFromDate, new DateTime(2017, 5, 1))
					.ExpectEquals("ET_StorageToDate: ", i => i.ET_StorageToDate, new DateTime(2017, 5, 5))
					.VerifyAll();

				JobStorage.AssertFromDB(connection, invoice2.PK)
					.ExpectEquals("ET_StorageFromDate: ", i => i.ET_StorageFromDate, new DateTime(2017, 5, 1))
					.ExpectEquals("ET_StorageToDate: ", i => i.ET_StorageToDate, new DateTime(2017, 5, 5))
					.VerifyAll();

				AssertNoExceptionThrown("Trigger should allow deletion of invoice with overlapping dates.", () => JobStorage.DeleteInDB(connection, invoice2.PK));
			}
		}

		#endregion

		#region Implementation

		void SaveToDB(DbConnection connection, SqlQueryBuilder sql)
		{
			connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
		}

		string TriggerErrorMessage => "Attempt to add an invoice which overlaps with dates in existing invoices.";
		#endregion
	}
}
