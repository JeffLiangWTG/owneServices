using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.ProductWarehouse;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformation.DataModification.Warehouse.Testing
{
	[TestedType(typeof(PopulateWhsVASOrderClientColumn))]
	public class PopulateWhsVASOrderClientColumnTest : DataTransformationTestCase
	{
		public void TestClientColumnIsCreated()
		{
		var date = new DateTime(2023, 4, 16);
			var sql = new SqlQueryBuilder();
			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var address = new OrgAddress(client, "A1", "STREET").AppendInsertAndReturnObject(sql);

			var warehouse = new WhsWarehouse("WHS").WithDockDoor(sql);
			var serviceArea = new WhsArea(warehouse.PK, "SERVICE").AppendInsertAndReturnObject(sql);
			var transferIn = new WhsDocket(client.PK, warehouse.PK, "TFR", "TFR", "ENT", "TIN").AppendInsertAndReturnObject(sql);

			var vasOrder = new WhsVASOrderOld_V02(address, serviceArea, "V1")
			{
				WVO_WD_TransferIntoServiceArea = transferIn,
				WVO_SystemLastEditTimeUtc = date.AddHours(-4),
				WVO_SystemLastEditUser = "SE1",
				WVO_SystemCreateTimeUtc = date.AddHours(-5),
				WVO_SystemCreateUser = "SH1"
			}.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var column = WhsVASOrderSchema.WVO_OH_Client;
			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);
			transform.Run(TransformationSection.OfflinePreUpgrade, CancellationToken.None);

			var tableDescriptor = new DbObjectCreator.TableDescriptor(column.TableSchema.SqlSchemaName, column.TableName);
			AssertEquals("Column WVO_OH_Client should get created.", true, DbObjectCreator.ColumnExists(TestConnection, tableDescriptor, column.Name));

			WhsVASOrder.AssertFromDB(TestConnection, vasOrder.PK)
				.ExpectEquals("WVO_OH_Client", v => v.WVO_OH_Client.FK, client.PK)
				.VerifyAll();
		}

		public void TestSameCustomerReferenceNo_DifferentClients()
		{
			var date = new DateTime(2023, 4, 16);
			var sql = new SqlQueryBuilder();
			var client1 = new OrgHeader("Client1").AppendInsertAndReturnObject(sql);
			var client2 = new OrgHeader("Client2").AppendInsertAndReturnObject(sql);
			var address1 = new OrgAddress(client1, "A1", "STREET").AppendInsertAndReturnObject(sql);
			var address2 = new OrgAddress(client2, "A1", "STREET").AppendInsertAndReturnObject(sql);

			var warehouse = new WhsWarehouse("WHS").WithDockDoor(sql);
			var serviceArea = new WhsArea(warehouse.PK, "SERVICE").AppendInsertAndReturnObject(sql);

			var vasOrder1 = new WhsVASOrderOld_V02(address1, serviceArea, "V1")
			{
				WVO_CustomerReferenceNo = "TEST",
				WVO_SystemLastEditTimeUtc = date.AddHours(-4),
				WVO_SystemLastEditUser = "SE1",
				WVO_SystemCreateTimeUtc = date.AddHours(-5),
				WVO_SystemCreateUser = "SH1"
			}.AppendInsertAndReturnObject(sql);

			var vasOrder2 = new WhsVASOrderOld_V02(address2, serviceArea, "V2")
			{
				WVO_CustomerReferenceNo = "TEST",
				WVO_SystemLastEditTimeUtc = date.AddHours(-4),
				WVO_SystemLastEditUser = "SE1",
				WVO_SystemCreateTimeUtc = date.AddHours(-5),
				WVO_SystemCreateUser = "SH1"
			}.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var column = WhsVASOrderSchema.WVO_OH_Client;
			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);
			transform.Run(TransformationSection.OfflinePreUpgrade, CancellationToken.None);

			var tableDescriptor = new DbObjectCreator.TableDescriptor(column.TableSchema.SqlSchemaName, column.TableName);
			AssertEquals("Column WVO_OH_Client should get created.", true, DbObjectCreator.ColumnExists(TestConnection, tableDescriptor, column.Name));

			WhsVASOrder.AssertFromDB(TestConnection, vasOrder1.PK)
				.ExpectEquals("WVO_OH_Client", v => v.WVO_OH_Client.FK, client1.PK)
				.ExpectEquals("WVO_CustomerReferenceNo", v => v.WVO_CustomerReferenceNo, "TEST")
				.VerifyAll();

			WhsVASOrder.AssertFromDB(TestConnection, vasOrder2.PK)
				.ExpectEquals("WVO_OH_Client", v => v.WVO_OH_Client.FK, client2.PK)
				.ExpectEquals("WVO_CustomerReferenceNo", v => v.WVO_CustomerReferenceNo, "TEST")
				.VerifyAll();
		}

		public void TestSameCustomerReferenceNo_DifferentAddress()
		{
			var date = new DateTime(2023, 4, 16);
			var sql = new SqlQueryBuilder();
			var client1 = new OrgHeader("Client1").AppendInsertAndReturnObject(sql);
			var address1 = new OrgAddress(client1, "A1", "STREET").AppendInsertAndReturnObject(sql);
			var address2 = new OrgAddress(client1, "A2", "MILK").AppendInsertAndReturnObject(sql);

			var warehouse = new WhsWarehouse("WHS").WithDockDoor(sql);
			var serviceArea = new WhsArea(warehouse.PK, "SERVICE").AppendInsertAndReturnObject(sql);

			var vasOrder1 = new WhsVASOrderOld_V02(address1, serviceArea, "V1")
			{
				WVO_CustomerReferenceNo = "TEST",
				WVO_SystemLastEditTimeUtc = date.AddHours(-4),
				WVO_SystemLastEditUser = "SE1",
				WVO_SystemCreateTimeUtc = date.AddHours(-5),
				WVO_SystemCreateUser = "SH1"
			}.AppendInsertAndReturnObject(sql);

			var vasOrder2 = new WhsVASOrderOld_V02(address2, serviceArea, "V2")
			{
				WVO_CustomerReferenceNo = "TEST",
				WVO_SystemLastEditTimeUtc = date.AddHours(-4),
				WVO_SystemLastEditUser = "SE1",
				WVO_SystemCreateTimeUtc = date.AddHours(-5),
				WVO_SystemCreateUser = "SH1"
			}.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var column = WhsVASOrderSchema.WVO_OH_Client;
			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);
			transform.Run(TransformationSection.OfflinePreUpgrade, CancellationToken.None);

			var tableDescriptor = new DbObjectCreator.TableDescriptor(column.TableSchema.SqlSchemaName, column.TableName);
			AssertEquals("Column WVO_OH_Client should get created.", true, DbObjectCreator.ColumnExists(TestConnection, tableDescriptor, column.Name));

			WhsVASOrder.AssertFromDB(TestConnection, vasOrder1.PK)
				.ExpectEquals("WVO_OH_Client", v => v.WVO_OH_Client.FK, client1.PK)
				.ExpectEquals("WVO_CustomerReferenceNo", v => v.WVO_CustomerReferenceNo, "TEST")
				.VerifyAll();
			AssertEquals("WVO_AutoVersion is correct", (short)0, TestConnection.ExecuteScalar($"SELECT WVO_AutoVersion FROM dbo.WhsVASOrder WHERE WVO_PK = '{vasOrder1.PK}'"));

			WhsVASOrder.AssertFromDB(TestConnection, vasOrder2.PK)
				.ExpectEquals("WVO_OH_Client", v => v.WVO_OH_Client.FK, client1.PK)
				.ExpectEquals("WVO_CustomerReferenceNo", v => v.WVO_CustomerReferenceNo, "TEST-001")
				.VerifyAll();
			AssertEquals("WVO_AutoVersion is correct", (short)1, TestConnection.ExecuteScalar($"SELECT WVO_AutoVersion FROM dbo.WhsVASOrder WHERE WVO_PK = '{vasOrder2.PK}'"));
		}

		public void TestSameCustomerReferenceNo_DifferentAddress_ConflictWithExistingRow()
		{
			var date = new DateTime(2023, 4, 16);
			var sql = new SqlQueryBuilder();
			var client1 = new OrgHeader("Client1").AppendInsertAndReturnObject(sql);
			var address1 = new OrgAddress(client1, "A1", "STREET").AppendInsertAndReturnObject(sql);
			var address2 = new OrgAddress(client1, "A2", "MILK").AppendInsertAndReturnObject(sql);

			var warehouse = new WhsWarehouse("WHS").WithDockDoor(sql);
			var serviceArea = new WhsArea(warehouse.PK, "SERVICE").AppendInsertAndReturnObject(sql);

			var vasOrder1 = new WhsVASOrderOld_V02(address1, serviceArea, "V1")
			{
				WVO_CustomerReferenceNo = "TEST",
				WVO_SystemLastEditTimeUtc = date.AddHours(-4),
				WVO_SystemLastEditUser = "SE1",
				WVO_SystemCreateTimeUtc = date.AddHours(-5),
				WVO_SystemCreateUser = "SH1"
			}.AppendInsertAndReturnObject(sql);

			var vasOrder2 = new WhsVASOrderOld_V02(address2, serviceArea, "V2")
			{
				WVO_CustomerReferenceNo = "TEST",
				WVO_SystemLastEditTimeUtc = date.AddHours(-4),
				WVO_SystemLastEditUser = "SE1",
				WVO_SystemCreateTimeUtc = date.AddHours(-5),
				WVO_SystemCreateUser = "SH1"
			}.AppendInsertAndReturnObject(sql);

			var vasOrder3 = new WhsVASOrderOld_V02(address2, serviceArea, "V3")
			{
				WVO_CustomerReferenceNo = "TEST-002",
				WVO_SystemLastEditTimeUtc = date.AddHours(-4),
				WVO_SystemLastEditUser = "SE1",
				WVO_SystemCreateTimeUtc = date.AddHours(-5),
				WVO_SystemCreateUser = "SH1"
			}.AppendInsertAndReturnObject(sql);

			var vasOrder4 = new WhsVASOrderOld_V02(address2, serviceArea, "V4")
			{
				WVO_CustomerReferenceNo = "TEST-003",
				WVO_SystemLastEditTimeUtc = date.AddHours(-4),
				WVO_SystemLastEditUser = "SE1",
				WVO_SystemCreateTimeUtc = date.AddHours(-5),
				WVO_SystemCreateUser = "SH1"
			}.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var column = WhsVASOrderSchema.WVO_OH_Client;
			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);
			transform.Run(TransformationSection.OfflinePreUpgrade, CancellationToken.None);

			var tableDescriptor = new DbObjectCreator.TableDescriptor(column.TableSchema.SqlSchemaName, column.TableName);
			AssertEquals("Column WVO_OH_Client should get created.", true, DbObjectCreator.ColumnExists(TestConnection, tableDescriptor, column.Name));

			WhsVASOrder.AssertFromDB(TestConnection, vasOrder1.PK)
				.ExpectEquals("WVO_OH_Client", v => v.WVO_OH_Client.FK, client1.PK)
				.ExpectEquals("WVO_CustomerReferenceNo", v => v.WVO_CustomerReferenceNo, "TEST")
				.VerifyAll();

			WhsVASOrder.AssertFromDB(TestConnection, vasOrder2.PK)
				.ExpectEquals("WVO_OH_Client", v => v.WVO_OH_Client.FK, client1.PK)
				.ExpectEquals("WVO_CustomerReferenceNo", v => v.WVO_CustomerReferenceNo, "TEST-004")
				.VerifyAll();
		}

		public void TestSameCustomerReferenceNo_DifferentAddress_NoAutoVersion()
		{
			var date = new DateTime(2023, 4, 16);
			var sql = new SqlQueryBuilder();
			var client1 = new OrgHeader("Client1").AppendInsertAndReturnObject(sql);
			var address1 = new OrgAddress(client1, "A1", "STREET").AppendInsertAndReturnObject(sql);
			var address2 = new OrgAddress(client1, "A2", "MILK").AppendInsertAndReturnObject(sql);

			var warehouse = new WhsWarehouse("WHS").WithDockDoor(sql);
			var serviceArea = new WhsArea(warehouse.PK, "SERVICE").AppendInsertAndReturnObject(sql);
			new DbColumnDependencyRemover(WhsVASOrderSchema.Constants.TableName, "WVO_AutoVersion").DropRelateObjects(TestConnection);
			TestConnection.ExecuteNonQuery("ALTER TABLE dbo.WhsVASOrder DROP Column WVO_AutoVersion");

			var vasOrder1 = new WhsVASOrderOld_V02(address1, serviceArea, "V1")
			{
				WVO_CustomerReferenceNo = "TEST",
				WVO_SystemLastEditTimeUtc = date.AddHours(-4),
				WVO_SystemLastEditUser = "SE1",
				WVO_SystemCreateTimeUtc = date.AddHours(-5),
				WVO_SystemCreateUser = "SH1"
			}.AppendInsertAndReturnObject(sql);

			var vasOrder2 = new WhsVASOrderOld_V02(address2, serviceArea, "V2")
			{
				WVO_CustomerReferenceNo = "TEST",
				WVO_SystemLastEditTimeUtc = date.AddHours(-4),
				WVO_SystemLastEditUser = "SE1",
				WVO_SystemCreateTimeUtc = date.AddHours(-5),
				WVO_SystemCreateUser = "SH1"
			}.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var column = WhsVASOrderSchema.WVO_OH_Client;
			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);
			transform.Run(TransformationSection.OfflinePreUpgrade, CancellationToken.None);

			var tableDescriptor = new DbObjectCreator.TableDescriptor(column.TableSchema.SqlSchemaName, column.TableName);
			AssertEquals("Column WVO_OH_Client should get created.", true, DbObjectCreator.ColumnExists(TestConnection, tableDescriptor, column.Name));

			WhsVASOrder.AssertFromDB(TestConnection, vasOrder1.PK)
				.ExpectEquals("WVO_OH_Client", v => v.WVO_OH_Client.FK, client1.PK)
				.ExpectEquals("WVO_CustomerReferenceNo", v => v.WVO_CustomerReferenceNo, "TEST")
				.VerifyAll();

			WhsVASOrder.AssertFromDB(TestConnection, vasOrder2.PK)
				.ExpectEquals("WVO_OH_Client", v => v.WVO_OH_Client.FK, client1.PK)
				.ExpectEquals("WVO_CustomerReferenceNo", v => v.WVO_CustomerReferenceNo, "TEST-001")
				.VerifyAll();
		}

		public void TestSameCustomerReferenceNo_DifferentAddress_NoClientColumn()
		{
			var date = new DateTime(2023, 4, 16);
			var sql = new SqlQueryBuilder();
			var client1 = new OrgHeader("Client1").AppendInsertAndReturnObject(sql);
			var address1 = new OrgAddress(client1, "A1", "STREET").AppendInsertAndReturnObject(sql);
			var address2 = new OrgAddress(client1, "A2", "MILK").AppendInsertAndReturnObject(sql);

			var warehouse = new WhsWarehouse("WHS").WithDockDoor(sql);
			var serviceArea = new WhsArea(warehouse.PK, "SERVICE").AppendInsertAndReturnObject(sql);

			var vasOrder1 = new WhsVASOrderOld_V02(address1, serviceArea, "V1")
			{
				WVO_CustomerReferenceNo = "TEST",
				WVO_SystemLastEditTimeUtc = date.AddHours(-4),
				WVO_SystemLastEditUser = "SE1",
				WVO_SystemCreateTimeUtc = date.AddHours(-5),
				WVO_SystemCreateUser = "SH1"
			}.AppendInsertAndReturnObject(sql);

			var vasOrder2 = new WhsVASOrderOld_V02(address2, serviceArea, "V2")
			{
				WVO_CustomerReferenceNo = "TEST",
				WVO_SystemLastEditTimeUtc = date.AddHours(-4),
				WVO_SystemLastEditUser = "SE1",
				WVO_SystemCreateTimeUtc = date.AddHours(-5),
				WVO_SystemCreateUser = "SH1"
			}.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var column = WhsVASOrderSchema.WVO_OH_Client;
			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OfflinePreUpgrade, CancellationToken.None);

			var tableDescriptor = new DbObjectCreator.TableDescriptor(column.TableSchema.SqlSchemaName, column.TableName);
			AssertEquals("Column WVO_OH_Client should *not* get created.", false, DbObjectCreator.ColumnExists(TestConnection, tableDescriptor, column.Name));

			WhsVASOrderOld_V02.AssertFromDB(TestConnection, vasOrder1.PK)
				.ExpectEquals("WVO_CustomerReferenceNo", v => v.WVO_CustomerReferenceNo, "TEST")
				.VerifyAll();

			WhsVASOrderOld_V02.AssertFromDB(TestConnection, vasOrder2.PK)
				.ExpectEquals("WVO_CustomerReferenceNo", v => v.WVO_CustomerReferenceNo, "TEST")
				.VerifyAll();
		}

		public void TestSameCustomerReferenceNo_DifferentAddress_Multiple()
		{
			var date = new DateTime(2023, 4, 16);
			var sql = new SqlQueryBuilder();
			var client1 = new OrgHeader("Client1").AppendInsertAndReturnObject(sql);
			var address1 = new OrgAddress(client1, "A1", "STREET").AppendInsertAndReturnObject(sql);
			var address2 = new OrgAddress(client1, "A2", "MILK").AppendInsertAndReturnObject(sql);
			var address3 = new OrgAddress(client1, "A3", "AUDI").AppendInsertAndReturnObject(sql);

			var warehouse = new WhsWarehouse("WHS").WithDockDoor(sql);
			var serviceArea = new WhsArea(warehouse.PK, "SERVICE").AppendInsertAndReturnObject(sql);

			var vasOrder1 = new WhsVASOrderOld_V02(address1, serviceArea, "V1")
			{
				WVO_CustomerReferenceNo = "TEST",
				WVO_SystemLastEditTimeUtc = date.AddHours(-4),
				WVO_SystemLastEditUser = "SE1",
				WVO_SystemCreateTimeUtc = date.AddHours(-5),
				WVO_SystemCreateUser = "SH1"
			}.AppendInsertAndReturnObject(sql);

			var vasOrder2 = new WhsVASOrderOld_V02(address2, serviceArea, "V2")
			{
				WVO_CustomerReferenceNo = "TEST",
				WVO_SystemLastEditTimeUtc = date.AddHours(-4),
				WVO_SystemLastEditUser = "SE1",
				WVO_SystemCreateTimeUtc = date.AddHours(-5),
				WVO_SystemCreateUser = "SH1"
			}.AppendInsertAndReturnObject(sql);

			var vasOrder3 = new WhsVASOrderOld_V02(address3, serviceArea, "V3")
			{
				WVO_CustomerReferenceNo = "TEST",
				WVO_SystemLastEditTimeUtc = date.AddHours(-4),
				WVO_SystemLastEditUser = "SE1",
				WVO_SystemCreateTimeUtc = date.AddHours(-5),
				WVO_SystemCreateUser = "SH1"
			}.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var column = WhsVASOrderSchema.WVO_OH_Client;
			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);
			transform.Run(TransformationSection.OfflinePreUpgrade, CancellationToken.None);

			var tableDescriptor = new DbObjectCreator.TableDescriptor(column.TableSchema.SqlSchemaName, column.TableName);
			AssertEquals("Column WVO_OH_Client should get created.", true, DbObjectCreator.ColumnExists(TestConnection, tableDescriptor, column.Name));

			WhsVASOrder.AssertFromDB(TestConnection, vasOrder1.PK)
				.ExpectEquals("WVO_OH_Client", v => v.WVO_OH_Client.FK, client1.PK)
				.ExpectEquals("WVO_CustomerReferenceNo", v => v.WVO_CustomerReferenceNo, "TEST")
				.VerifyAll();

			WhsVASOrder.AssertFromDB(TestConnection, vasOrder2.PK)
				.ExpectEquals("WVO_OH_Client", v => v.WVO_OH_Client.FK, client1.PK)
				.ExpectEquals("WVO_CustomerReferenceNo", v => v.WVO_CustomerReferenceNo, "TEST-001")
				.VerifyAll();

			WhsVASOrder.AssertFromDB(TestConnection, vasOrder3.PK)
				.ExpectEquals("WVO_OH_Client", v => v.WVO_OH_Client.FK, client1.PK)
				.ExpectEquals("WVO_CustomerReferenceNo", v => v.WVO_CustomerReferenceNo, "TEST-002")
				.VerifyAll();
		}

		public void TestSameCustomerReferenceNo_DifferentAddress_MaxLength()
		{
			var date = new DateTime(2023, 4, 16);
			var sql = new SqlQueryBuilder();
			var client1 = new OrgHeader("Client1").AppendInsertAndReturnObject(sql);
			var address1 = new OrgAddress(client1, "A1", "STREET").AppendInsertAndReturnObject(sql);
			var address2 = new OrgAddress(client1, "A2", "MILK").AppendInsertAndReturnObject(sql);

			var warehouse = new WhsWarehouse("WHS").WithDockDoor(sql);
			var serviceArea = new WhsArea(warehouse.PK, "SERVICE").AppendInsertAndReturnObject(sql);

			var vasOrder1 = new WhsVASOrderOld_V02(address1, serviceArea, "V1")
			{
				WVO_CustomerReferenceNo = "SOMEVERYLONGREFERENCEUNFORTUNATELY",
				WVO_SystemLastEditTimeUtc = date.AddHours(-4),
				WVO_SystemLastEditUser = "SE1",
				WVO_SystemCreateTimeUtc = date.AddHours(-5),
				WVO_SystemCreateUser = "SH1"
			}.AppendInsertAndReturnObject(sql);

			var vasOrder2 = new WhsVASOrderOld_V02(address2, serviceArea, "V2")
			{
				WVO_CustomerReferenceNo = "SOMEVERYLONGREFERENCEUNFORTUNATELY",
				WVO_SystemLastEditTimeUtc = date.AddHours(-4),
				WVO_SystemLastEditUser = "SE1",
				WVO_SystemCreateTimeUtc = date.AddHours(-5),
				WVO_SystemCreateUser = "SH1"
			}.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var column = WhsVASOrderSchema.WVO_OH_Client;
			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);
			transform.Run(TransformationSection.OfflinePreUpgrade, CancellationToken.None);

			var tableDescriptor = new DbObjectCreator.TableDescriptor(column.TableSchema.SqlSchemaName, column.TableName);
			AssertEquals("Column WVO_OH_Client should get created.", true, DbObjectCreator.ColumnExists(TestConnection, tableDescriptor, column.Name));

			WhsVASOrder.AssertFromDB(TestConnection, vasOrder1.PK)
				.ExpectEquals("WVO_OH_Client", v => v.WVO_OH_Client.FK, client1.PK)
				.ExpectEquals("WVO_CustomerReferenceNo", v => v.WVO_CustomerReferenceNo, "SOMEVERYLONGREFERENCEUNFORTUNATELY")
				.VerifyAll();

			WhsVASOrder.AssertFromDB(TestConnection, vasOrder2.PK)
				.ExpectEquals("WVO_OH_Client", v => v.WVO_OH_Client.FK, client1.PK)
				.ExpectEquals("WVO_CustomerReferenceNo", v => v.WVO_CustomerReferenceNo, "SOMEVERYLONGREFERENCEUNFORTUNAT-001")
				.VerifyAll();
		}

		public void TestSameCustomerReferenceNo_DifferentAddress_MaxLength_Multiple()
		{
			var date = new DateTime(2023, 4, 16);
			var sql = new SqlQueryBuilder();
			var client1 = new OrgHeader("Client1").AppendInsertAndReturnObject(sql);
			var address1 = new OrgAddress(client1, "A1", "STREET").AppendInsertAndReturnObject(sql);
			var address2 = new OrgAddress(client1, "A2", "MILK").AppendInsertAndReturnObject(sql);
			var address3 = new OrgAddress(client1, "A3", "AUDI").AppendInsertAndReturnObject(sql);
			var address4 = new OrgAddress(client1, "A4", "PAPER").AppendInsertAndReturnObject(sql);

			var warehouse = new WhsWarehouse("WHS").WithDockDoor(sql);
			var serviceArea = new WhsArea(warehouse.PK, "SERVICE").AppendInsertAndReturnObject(sql);

			var vasOrder1 = new WhsVASOrderOld_V02(address1, serviceArea, "V1")
			{
				WVO_CustomerReferenceNo = "SOMEVERYLONGREFERENCEUNFORTUNATELY",
				WVO_SystemLastEditTimeUtc = date.AddHours(-4),
				WVO_SystemLastEditUser = "SE1",
				WVO_SystemCreateTimeUtc = date.AddHours(-9),
				WVO_SystemCreateUser = "SH1"
			}.AppendInsertAndReturnObject(sql);

			var vasOrder2 = new WhsVASOrderOld_V02(address2, serviceArea, "V2")
			{
				WVO_CustomerReferenceNo = "SOMEVERYLONGREFERENCEUNFORTUNATELY",
				WVO_SystemLastEditTimeUtc = date.AddHours(-4),
				WVO_SystemLastEditUser = "SE1",
				WVO_SystemCreateTimeUtc = date.AddHours(-8),
				WVO_SystemCreateUser = "SH1"
			}.AppendInsertAndReturnObject(sql);

			var vasOrder3 = new WhsVASOrderOld_V02(address3, serviceArea, "V3")
			{
				WVO_CustomerReferenceNo = "SOMEVERYLONGREFERENCEUNFORTUNATELI",
				WVO_SystemLastEditTimeUtc = date.AddHours(-4),
				WVO_SystemLastEditUser = "SE1",
				WVO_SystemCreateTimeUtc = date.AddHours(-7),
				WVO_SystemCreateUser = "SH1"
			}.AppendInsertAndReturnObject(sql);

			var vasOrder4 = new WhsVASOrderOld_V02(address3, serviceArea, "V4")
			{
				WVO_CustomerReferenceNo = "SOMEVERYLONGREFERENCEUNFORTUNATELI",
				WVO_SystemLastEditTimeUtc = date.AddHours(-4),
				WVO_SystemLastEditUser = "SE1",
				WVO_SystemCreateTimeUtc = date.AddHours(-6),
				WVO_SystemCreateUser = "SH1"
			}.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var column = WhsVASOrderSchema.WVO_OH_Client;
			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);
			transform.Run(TransformationSection.OfflinePreUpgrade, CancellationToken.None);

			var tableDescriptor = new DbObjectCreator.TableDescriptor(column.TableSchema.SqlSchemaName, column.TableName);
			AssertEquals("Column WVO_OH_Client should get created.", true, DbObjectCreator.ColumnExists(TestConnection, tableDescriptor, column.Name));

			WhsVASOrder.AssertFromDB(TestConnection, vasOrder1.PK)
				.ExpectEquals("WVO_OH_Client", v => v.WVO_OH_Client.FK, client1.PK)
				.ExpectEquals("WVO_CustomerReferenceNo", v => v.WVO_CustomerReferenceNo, "SOMEVERYLONGREFERENCEUNFORTUNATELY")
				.VerifyAll();

			WhsVASOrder.AssertFromDB(TestConnection, vasOrder2.PK)
				.ExpectEquals("WVO_OH_Client", v => v.WVO_OH_Client.FK, client1.PK)
				.ExpectEquals("WVO_CustomerReferenceNo", v => v.WVO_CustomerReferenceNo, "SOMEVERYLONGREFERENCEUNFORTUNAT-001")
				.VerifyAll();

			WhsVASOrder.AssertFromDB(TestConnection, vasOrder3.PK)
				.ExpectEquals("WVO_OH_Client", v => v.WVO_OH_Client.FK, client1.PK)
				.ExpectEquals("WVO_CustomerReferenceNo", v => v.WVO_CustomerReferenceNo, "SOMEVERYLONGREFERENCEUNFORTUNATELI")
				.VerifyAll();

			WhsVASOrder.AssertFromDB(TestConnection, vasOrder4.PK)
				.ExpectEquals("WVO_OH_Client", v => v.WVO_OH_Client.FK, client1.PK)
				.ExpectEquals("WVO_CustomerReferenceNo", v => v.WVO_CustomerReferenceNo, "SOMEVERYLONGREFERENCEUNFORTUNAT-002")
				.VerifyAll();
		}

		public void TestSameCustomerReferenceNo_DifferentAddress_MaxLength_Multiple_NotDuplicates()
		{
			var date = new DateTime(2023, 4, 16);
			var sql = new SqlQueryBuilder();
			var client1 = new OrgHeader("Client1").AppendInsertAndReturnObject(sql);
			var address1 = new OrgAddress(client1, "A1", "STREET").AppendInsertAndReturnObject(sql);
			var address2 = new OrgAddress(client1, "A2", "MILK").AppendInsertAndReturnObject(sql);
			var address3 = new OrgAddress(client1, "A3", "AUDI").AppendInsertAndReturnObject(sql);
			var address4 = new OrgAddress(client1, "A4", "PAPER").AppendInsertAndReturnObject(sql);

			var warehouse = new WhsWarehouse("WHS").WithDockDoor(sql);
			var serviceArea = new WhsArea(warehouse.PK, "SERVICE").AppendInsertAndReturnObject(sql);

			var vasOrder1 = new WhsVASOrderOld_V02(address1, serviceArea, "V1")
			{
				WVO_CustomerReferenceNo = "SOMEVERYLONGREFERENCEUNFORTUNATELY",
				WVO_SystemLastEditTimeUtc = date.AddHours(-4),
				WVO_SystemLastEditUser = "SE1",
				WVO_SystemCreateTimeUtc = date.AddHours(-5),
				WVO_SystemCreateUser = "SH1"
			}.AppendInsertAndReturnObject(sql);

			var vasOrder2 = new WhsVASOrderOld_V02(address2, serviceArea, "V2")
			{
				WVO_CustomerReferenceNo = "SOMEVERYLONGREFERENCEUNFORTUNATELI",
				WVO_SystemLastEditTimeUtc = date.AddHours(-4),
				WVO_SystemLastEditUser = "SE1",
				WVO_SystemCreateTimeUtc = date.AddHours(-5),
				WVO_SystemCreateUser = "SH1"
			}.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var column = WhsVASOrderSchema.WVO_OH_Client;
			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);
			transform.Run(TransformationSection.OfflinePreUpgrade, CancellationToken.None);

			var tableDescriptor = new DbObjectCreator.TableDescriptor(column.TableSchema.SqlSchemaName, column.TableName);
			AssertEquals("Column WVO_OH_Client should get created.", true, DbObjectCreator.ColumnExists(TestConnection, tableDescriptor, column.Name));

			WhsVASOrder.AssertFromDB(TestConnection, vasOrder1.PK)
				.ExpectEquals("WVO_OH_Client", v => v.WVO_OH_Client.FK, client1.PK)
				.ExpectEquals("WVO_CustomerReferenceNo", v => v.WVO_CustomerReferenceNo, "SOMEVERYLONGREFERENCEUNFORTUNATELY")
				.VerifyAll();

			WhsVASOrder.AssertFromDB(TestConnection, vasOrder2.PK)
				.ExpectEquals("WVO_OH_Client", v => v.WVO_OH_Client.FK, client1.PK)
				.ExpectEquals("WVO_CustomerReferenceNo", v => v.WVO_CustomerReferenceNo, "SOMEVERYLONGREFERENCEUNFORTUNATELI")
				.VerifyAll();
		}

		public void TestTriggerIsCreatedCorrectly()
		{
			var column = WhsVASOrderSchema.WVO_OH_Client;
			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			var triggerName = "TG_WhsVasOrder_KeepClientInSync";
			AssertEquals("Trigger should be created.", true, DbObjectCreator.TriggerExists(TestConnection, column.TableName, triggerName));

			var expectedTriggerDefinition = @"
CREATE TRIGGER dbo.TG_WhsVASOrder_KeepClientInSync
	ON dbo.WhsVASOrder
	FOR INSERT, UPDATE
AS
BEGIN
	SET NOCOUNT ON;
	IF UPDATE (WVO_OA_ClientAddress)
	BEGIN
		UPDATE dbo.WhsVASOrder SET WVO_OH_Client = OA_OH,
		WVO_SystemLastEditTimeUtc = GETUTCDATE(),
		WVO_SystemLastEditUser = WVO_SystemLastEditUser
		FROM dbo.WhsVASOrder WVO
		JOIN dbo.OrgAddress OA on OA_PK = WVO_OA_ClientAddress
		WHERE WVO_PK in (SELECT inserted.WVO_PK FROM inserted)
	END
END";
			AssertEquals("Trigger Definition is correct", expectedTriggerDefinition, DbObjectCreator.GetTriggerDefinition(TestConnection, triggerName));

			PrepareTestData();

			var vasOrder = WhsVASOrder.ShallowLoadFromDB(TestConnection, v => v.WVO_JobID == "V1").Single();
			var client = OrgHeader.ShallowLoadFromDB(TestConnection, o => o.OH_Code == "Client").Single();
			vasOrder.BuildAssertion(TestConnection)
				.ExpectEquals("WVO_OH_Client", v => v.WVO_OH_Client, client)
				.VerifyAll();
		}

		public void TestTriggerSetsClientAfterUpdate()
		{
			var column = WhsVASOrderSchema.WVO_OH_Client;
			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			PrepareTestData();
			var vasOrder = WhsVASOrder.ShallowLoadFromDB(TestConnection, v => v.WVO_JobID == "V1").Single();
			var client = OrgHeader.ShallowLoadFromDB(TestConnection, o => o.OH_Code == "Client").Single();
			vasOrder.BuildAssertion(TestConnection)
				.ExpectEquals("WVO_OH_Client", v => v.WVO_OH_Client, client)
				.VerifyAll();

			var sql = new SqlQueryBuilder();
			var client2 = new OrgHeader("C2").AppendInsertAndReturnObject(sql);
			var address2 = new OrgAddress(client2, "A2", "STREET").AppendInsertAndReturnObject(sql);
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var updateSQL = WhsVASOrderOld_V02.UpdateWhere(vasOrder.PK).Set(v => v.WVO_OA_ClientAddress.FK, address2.PK).AsSQL();
			TestConnection.ExecuteNonQuery(updateSQL);

			var updatedClient = OrgHeader.ShallowLoadFromDB(TestConnection, o => o.OH_Code == "C2").Single();
			WhsVASOrder.AssertFromDB(TestConnection, vasOrder.PK)
				.ExpectEquals("WVO_OH_Client", v => v.WVO_OH_Client, updatedClient)
				.VerifyAll();
		}

		public void TestTransformSetsLastEditDateAndUser()
		{
			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);
			var now = (DateTime)Db.Connection.ExecuteScalar("SELECT GETUTCDATE()");
			PrepareTestData();

			var vasOrder = WhsVASOrder.ShallowLoadFromDB(TestConnection, v => v.WVO_JobID == "V1").Single();
			vasOrder.BuildAssertion(TestConnection)
				.ExpectEquals("WVO_SystemLastEditUser", v => v.WVO_SystemLastEditUser, "SE1")
				.ExpectEquals("WVO_SystemLastEditTimeUtc", v => v.WVO_SystemLastEditTimeUtc > now.AddHours(-1), true)
				.VerifyAll();
		}

		public void TestTransformSetsClientWhenFromToJobIDsSet()
		{
			const string LastUpdatedJobID = "PopulateWhsVASOrderClientColumn.WHO_OH_Client.LastUpdatedJob";
			const string MaxJobIDToUpdate = "PopulateWhsVASOrderClientColumn.WHO_OH_Client.MaxJobIdToUpdate";

			var date = new DateTime(2023, 4, 16);
			var sql = new SqlQueryBuilder();
			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var address = new OrgAddress(client, "A1", "STREET").AppendInsertAndReturnObject(sql);

			var warehouse = new WhsWarehouse("WHS").WithDockDoor(sql);
			var serviceArea = new WhsArea(warehouse.PK, "SERVICE").AppendInsertAndReturnObject(sql);
			var transferIn = new WhsDocket(client.PK, warehouse.PK, "TFR", "TFR", "ENT", "TIN").AppendInsertAndReturnObject(sql);

			var vasOrder1 = new WhsVASOrderOld_V02(address, serviceArea, "V1")
			{
				WVO_WD_TransferIntoServiceArea = transferIn,
				WVO_SystemLastEditTimeUtc = date.AddHours(-4),
				WVO_SystemLastEditUser = "SE1",
				WVO_SystemCreateTimeUtc = date.AddHours(-5),
				WVO_SystemCreateUser = "SH1"
			}.AppendInsertAndReturnObject(sql);

			var client2 = new OrgHeader("C2").AppendInsertAndReturnObject(sql);
			var address2 = new OrgAddress(client2, "A2", "STREET").AppendInsertAndReturnObject(sql);
			var vasOrder2 = new WhsVASOrderOld_V02(address2, serviceArea, "V2")
			{
				WVO_SystemLastEditTimeUtc = date.AddHours(-4),
				WVO_SystemLastEditUser = "SE1",
				WVO_SystemCreateTimeUtc = date.AddHours(-5),
				WVO_SystemCreateUser = "SH1"
			}.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var maxJobId = "V2";
			var lastJobId = "V1";
			ExtProperty.Database.Update(Db.Connection, MaxJobIDToUpdate, maxJobId);
			ExtProperty.Database.Update(Db.Connection, LastUpdatedJobID, lastJobId);

			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			var dbVasOrder2 = WhsVASOrder.ShallowLoadFromDB(TestConnection, v => v.WVO_JobID == "V2").Single();
			var dbClient2 = OrgHeader.ShallowLoadFromDB(TestConnection, o => o.OH_Code == "C2").Single();
			dbVasOrder2.BuildAssertion(TestConnection)
				.ExpectEquals("WVO_OH_Client", v => v.WVO_OH_Client, dbClient2)
				.VerifyAll();
		}

		public void TestDefaultClientConstraintIsWorking()
		{
			var date = new DateTime(2023, 4, 16);
			var sql = new SqlQueryBuilder();
			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var address = new OrgAddress(client, "A1", "STREET").AppendInsertAndReturnObject(sql);
			var warehouse = new WhsWarehouse("WHS").WithDockDoor(sql);
			var serviceArea = new WhsArea(warehouse.PK, "SERVICE").AppendInsertAndReturnObject(sql);
			var transferIn = new WhsDocket(client.PK, warehouse.PK, "TFR", "TFR", "ENT", "TIN").AppendInsertAndReturnObject(sql);

			var vasOrder1 = new WhsVASOrderOld_V02(address, serviceArea, "V1")
			{
				WVO_WD_TransferIntoServiceArea = transferIn,
				WVO_SystemLastEditTimeUtc = date.AddHours(-4),
				WVO_SystemLastEditUser = "SE1",
				WVO_SystemCreateTimeUtc = date.AddHours(-5),
				WVO_SystemCreateUser = "SH1"
			}.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);
			DbObjectCreator.DropTriggerIfExists(Db.Connection, "TG_WhsVASOrder_KeepClientInSync");

			var sql2 = new SqlQueryBuilder();
			var client2 = new OrgHeader("C2").AppendInsertAndReturnObject(sql2);
			var address2 = new OrgAddress(client2, "A2", "STREET").AppendInsertAndReturnObject(sql2);
			var vasOrder2 = new WhsVASOrderOld_V02(address2, serviceArea, "V2")
			{
				WVO_SystemLastEditTimeUtc = date.AddHours(-4),
				WVO_SystemLastEditUser = "SE1",
				WVO_SystemCreateTimeUtc = date.AddHours(-5),
				WVO_SystemCreateUser = "SH1"
			}.AppendInsertAndReturnObject(sql2);

			TestConnection.ExecuteNonQuery(sql2.ToStringWithNewLineBetweenAppends());

			var dbVasOrder2 = WhsVASOrder.ShallowLoadFromDB(TestConnection, v => v.WVO_JobID == "V2").Single();
			var dbClient2 = OrgHeader.ShallowLoadFromDB(TestConnection, o => o.OH_Code == "Client").Single();
			dbVasOrder2.BuildAssertion(TestConnection)
				.ExpectEquals("WVO_OH_Client", v => v.WVO_OH_Client, dbClient2)
				.VerifyAll();
		}

		public void TestDBExternalPropertiesAreDeletedAfterTransform()
		{
			const string LastUpdatedJobID = "PopulateWhsVASOrderClientColumn.WHO_OH_Client.LastUpdatedJob";
			const string MaxJobIDToUpdate = "PopulateWhsVASOrderClientColumn.WHO_OH_Client.MaxJobIdToUpdate";
			const string UpdateClientCount = "PopulateWhsVASOrderClientColumn.WHO_OH_Client.UpdateCnt";

			var date = new DateTime(2023, 4, 16);
			var sql = new SqlQueryBuilder();
			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var address = new OrgAddress(client, "A1", "STREET").AppendInsertAndReturnObject(sql);

			var warehouse = new WhsWarehouse("WHS").WithDockDoor(sql);
			var serviceArea = new WhsArea(warehouse.PK, "SERVICE").AppendInsertAndReturnObject(sql);
			var transferIn = new WhsDocket(client.PK, warehouse.PK, "TFR", "TFR", "ENT", "TIN").AppendInsertAndReturnObject(sql);

			var vasOrder1 = new WhsVASOrderOld_V02(address, serviceArea, "V1")
			{
				WVO_WD_TransferIntoServiceArea = transferIn,
				WVO_SystemLastEditTimeUtc = date.AddHours(-4),
				WVO_SystemLastEditUser = "SE1",
				WVO_SystemCreateTimeUtc = date.AddHours(-5),
				WVO_SystemCreateUser = "SH1"
			}.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			var fromJobID = ExtProperty.Database.Select(Db.Connection, LastUpdatedJobID);
			var toJobID = ExtProperty.Database.Select(Db.Connection, MaxJobIDToUpdate);
			var updatedClientCnt = ExtProperty.Database.Select(Db.Connection, UpdateClientCount);

			AssertNull("The external DB proeprty should be deleted", fromJobID);
			AssertNull("The external DB proeprty should be deleted", toJobID);
			AssertNull("The external DB proeprty should be deleted", updatedClientCnt);
		}

		public void TestTransformBatchUpdatesClient()
		{
			var date = new DateTime(2023, 4, 16);
			var sql = new SqlQueryBuilder();
			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var address = new OrgAddress(client, "A1", "STREET").AppendInsertAndReturnObject(sql);
			var client2 = new OrgHeader("C2").AppendInsertAndReturnObject(sql);
			var address2 = new OrgAddress(client2, "A2", "STREET").AppendInsertAndReturnObject(sql);
			var warehouse = new WhsWarehouse("WHS").WithDockDoor(sql);
			var serviceArea = new WhsArea(warehouse.PK, "SERVICE").AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var vasOrders = new List<WhsVASOrderOld_V02>();
			var numberOfRecords = 1000;
			var vasOrderList = new WhsVASOrderOld_V02[1800];

			for (var i = 0; i < numberOfRecords; i++)
			{
				vasOrderList[i] = new WhsVASOrderOld_V02(address, serviceArea, $"V{i}")
				{
					WVO_SystemLastEditTimeUtc = date.AddHours(-4),
					WVO_SystemLastEditUser = "SE1",
					WVO_SystemCreateTimeUtc = date.AddHours(-5),
					WVO_SystemCreateUser = "SH1"
				};
			}

			numberOfRecords = 1600;
			for (var i = 1000; i < numberOfRecords; i++)
			{
				vasOrderList[i] = new WhsVASOrderOld_V02(address2, serviceArea, $"V{i}")
				{
					WVO_SystemLastEditTimeUtc = date.AddHours(-4),
					WVO_SystemLastEditUser = "SE1",
					WVO_SystemCreateTimeUtc = date.AddHours(-5),
					WVO_SystemCreateUser = "SH1"
				};
			}

			numberOfRecords = 1800;
			for (var i = 1600; i < numberOfRecords; i++)
			{
				vasOrderList[i] = new WhsVASOrderOld_V02(address2, serviceArea, $"V{i}")
				{
					WVO_SystemLastEditTimeUtc = date.AddHours(-4),
					WVO_SystemLastEditUser = "SE1",
					WVO_SystemCreateTimeUtc = date.AddHours(-5),
					WVO_SystemCreateUser = "SH1"
				};
			}

			if (numberOfRecords > 0)
			{
				TestConnection.ExecuteNonQuery(WhsVASOrderOld_V02.GetBulkInsertStatement(vasOrderList));
			}

			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);
			foreach (var vasOrder in vasOrderList)
			{
				var expectedAddr = vasOrder.WVO_OA_ClientAddress == address.PK ? client.PK : client2.PK;
				WhsVASOrder.AssertFromDB(TestConnection, vasOrder.PK)
				.ExpectNotEquals("WVO_OH_Client is populated", v => v.WVO_OH_Client, null)
				.ExpectEquals("WVO_OH_Client", v => v.WVO_OH_Client.FK, expectedAddr)
				.VerifyAll();
			}
		}

		public void TestTransformKeepsSameLastEditUserForExisitngData()
		{
			var date = new DateTime(2023, 4, 16);
			var sql = new SqlQueryBuilder();
			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var address = new OrgAddress(client, "A1", "STREET").AppendInsertAndReturnObject(sql);
			var warehouse = new WhsWarehouse("WHS").WithDockDoor(sql);
			var serviceArea = new WhsArea(warehouse.PK, "SERVICE").AppendInsertAndReturnObject(sql);
			var transferIn = new WhsDocket(client.PK, warehouse.PK, "TFR", "TFR", "ENT", "TIN").AppendInsertAndReturnObject(sql);

			var vasOrder1 = new WhsVASOrderOld_V02(address, serviceArea, "V1")
			{
				WVO_WD_TransferIntoServiceArea = transferIn,
				WVO_SystemLastEditTimeUtc = date.AddHours(-4),
				WVO_SystemLastEditUser = "SE1",
				WVO_SystemCreateTimeUtc = date.AddHours(-5),
				WVO_SystemCreateUser = "SH1"
			}.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			var vasOrder = WhsVASOrder.ShallowLoadFromDB(TestConnection, v => v.WVO_JobID == "V1").Single();
			vasOrder.BuildAssertion(TestConnection)
				.ExpectEquals("WVO_SystemLastEditUser", v => v.WVO_SystemLastEditUser, "SE1")
				.VerifyAll();
		}

		public override void TestNewIndex()
		{
			Assert("The scenarios for index are already tested seperately in this class", true);
		}

		public void TestIndexProvider()
		{
			var transformation = (PopulateWhsVASOrderClientColumn)GetNewTestTransformationInstance();

			AssertContainsExactElementsInAnyOrder
			(
				expected: Array.Empty<string>(),
				((ITransformationIndexProvider)transformation).IndexProvider.Select(index => index.Definition)
			);
		}

		public void TestIndexProvider_AfterOnlinePreUpgrade()
		{
			var transformation = (PopulateWhsVASOrderClientColumn)GetNewTestTransformationInstance();
			transformation.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			AssertContainsExactElementsInAnyOrder
			(
				expected: new[]
				{
					"NONCLUSTERED INDEX [_WTG__Populate WhsVASOrder WVO_OH_Client column._1] ON [dbo].[WhsVASOrder] ([WVO_OH_Client], [WVO_CustomerReferenceNo], [WVO_SystemCreateTimeUtc]) INCLUDE ([WVO_AutoVersion], [WVO_SystemLastEditTimeUtc], [WVO_SystemLastEditUser]) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
				},
				((ITransformationIndexProvider)transformation).IndexProvider.Select(index => index.Definition)
			);
		}

		public void TestIndexProvider_AfterOnlinePreUpgrade_NoAutoVersion()
		{
			new DbColumnDependencyRemover(WhsVASOrderSchema.Constants.TableName, "WVO_AutoVersion").DropRelateObjects(TestConnection);
			TestConnection.ExecuteNonQuery("ALTER TABLE dbo.WhsVASOrder DROP Column WVO_AutoVersion");

			var transformation = (PopulateWhsVASOrderClientColumn)GetNewTestTransformationInstance();
			transformation.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			AssertContainsExactElementsInAnyOrder
			(
				expected: new[]
				{
					"NONCLUSTERED INDEX [_WTG__Populate WhsVASOrder WVO_OH_Client column._1] ON [dbo].[WhsVASOrder] ([WVO_OH_Client], [WVO_CustomerReferenceNo], [WVO_SystemCreateTimeUtc]) INCLUDE ([WVO_SystemLastEditTimeUtc], [WVO_SystemLastEditUser]) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
				},
				((ITransformationIndexProvider)transformation).IndexProvider.Select(index => index.Definition)
			);
		}

		protected override void PrepareTestData()
		{
			var date = new DateTime(2023, 4, 16);
			var sql = new SqlQueryBuilder();
			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var address = new OrgAddress(client, "A1", "STREET").AppendInsertAndReturnObject(sql);

			var warehouse = new WhsWarehouse("WHS").WithDockDoor(sql);
			var serviceArea = new WhsArea(warehouse.PK, "SERVICE").AppendInsertAndReturnObject(sql);
			var transferIn = new WhsDocket(client.PK, warehouse.PK, "TFR", "TFR", "ENT", "TIN").AppendInsertAndReturnObject(sql);

			new WhsVASOrderOld_V02(address, serviceArea, "V1")
			{
				WVO_WD_TransferIntoServiceArea = transferIn,
				WVO_SystemLastEditTimeUtc = date.AddHours(-4),
				WVO_SystemLastEditUser = "SE1",
				WVO_SystemCreateTimeUtc = date.AddHours(-5),
				WVO_SystemCreateUser = "SH1"
			}.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
		}

		protected override void AssertTransformationResults()
		{
			var column = WhsVASOrderSchema.WVO_OH_Client;
			var tableDescriptor = new DbObjectCreator.TableDescriptor(column.TableSchema.SqlSchemaName, column.TableName);
			AssertEquals("Column WVO_OH_Client should get created.", true, DbObjectCreator.ColumnExists(TestConnection, tableDescriptor, column.Name));

			var vasOrder = WhsVASOrder.ShallowLoadFromDB(TestConnection, v => v.WVO_JobID == "V1").Single();
			var client = OrgHeader.ShallowLoadFromDB(TestConnection, o => o.OH_Code == "Client").Single();
			vasOrder.BuildAssertion(TestConnection)
				.ExpectEquals("WVO_OH_Client", v => v.WVO_OH_Client, client)
				.VerifyAll();
		}

		#region SetUp

		protected override void SetUp()
		{
			base.SetUp();
			TestConnection.ExecuteNonQuery("ALTER TABLE dbo.WhsVASOrder ADD WVO_OA_ClientAddress uniqueidentifier NOT NULL");
			DropClientColumnAndDependency();
		}

		void DropClientColumnAndDependency()
		{
			new DbColumnDependencyRemover(WhsVASOrderSchema.Constants.TableName, "WVO_OH_Client").DropRelateObjects(Db.Connection);
			TestConnection.ExecuteNonQuery("ALTER TABLE dbo.WhsVASOrder DROP Column WVO_OH_Client");
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new PopulateWhsVASOrderClientColumn();

		#endregion
	}
}
