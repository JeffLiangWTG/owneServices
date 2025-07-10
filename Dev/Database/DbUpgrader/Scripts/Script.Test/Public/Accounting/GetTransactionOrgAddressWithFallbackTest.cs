using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Testing
{
	[TestedType(typeof(GetTransactionOrgAddressWithFallback))]
	class GetTransactionOrgAddressWithFallbackTest : DbCreateScriptTest
	{
		public void TestGetData()
		{
			var org1Pk = Guid.NewGuid();
			var org2Pk = Guid.NewGuid();
			var org3Pk = Guid.NewGuid();

			var addr1Pk = Guid.NewGuid();
			var addr2Pk = Guid.NewGuid();
			var addr3Pk = Guid.NewGuid();
			var addr4Pk = Guid.NewGuid();
			var addr5Pk = Guid.NewGuid();
			var addr6Pk = Guid.NewGuid();
			var addr7Pk = Guid.NewGuid();
			var addr8Pk = Guid.NewGuid();
			var addr9Pk = Guid.NewGuid();
			var addr10Pk = Guid.NewGuid();
			var addr11Pk = Guid.NewGuid();
			var addr12Pk = Guid.NewGuid();
			var addr13Pk = Guid.NewGuid();

			var insertSql = $@"
declare @Org1 uniqueidentifier = '{org1Pk}'
declare @Org2 uniqueidentifier = '{org2Pk}'
declare @Org3 uniqueidentifier = '{org3Pk}'

declare @Addr1 uniqueidentifier = '{addr1Pk}'
declare @Addr2 uniqueidentifier = '{addr2Pk}'
declare @Addr3 uniqueidentifier = '{addr3Pk}'
declare @Addr4 uniqueidentifier = '{addr4Pk}'
declare @Addr5 uniqueidentifier = '{addr5Pk}'
declare @Addr6 uniqueidentifier = '{addr6Pk}'
declare @Addr7 uniqueidentifier = '{addr7Pk}'
declare @Addr8 uniqueidentifier = '{addr8Pk}'
declare @Addr9 uniqueidentifier = '{addr9Pk}'
declare @Addr10 uniqueidentifier = '{addr10Pk}'
declare @Addr11 uniqueidentifier = '{addr11Pk}'
declare @Addr12 uniqueidentifier = '{addr12Pk}'
declare @Addr13 uniqueidentifier = '{addr13Pk}'

insert into dbo.OrgHeader (OH_PK, OH_Code)
values
	(@Org1, 'DDDABCSYD'),
	(@Org2, 'DDDDEFMEL'),
	(@Org3, 'DDDDEFMEE')

insert into dbo.OrgAddress (OA_PK, OA_OH, OA_Code, OA_Address1)
values
	(@Addr1, @Org1, '1 Test', '1 Test Rd'),
	(@Addr2, @Org1, '2 Test', '2 Test Rd'),
	(@Addr3, @Org1, '3 Test', '3 Test Rd'),
	(@Addr4, @Org1, '4 Test', '4 Test Rd'),
	(@Addr5, @Org1, '5 Test', '5 Test Rd'),
	(@Addr6, @Org1, '6 Test', '6 Test Rd'),
	(@Addr7, @Org1, '7 Test', '7 Test Rd'),
	(@Addr8, @Org1, '8 Test', '8 Test Rd'),
	(@Addr9, @Org1, '9 Test', '9 Test Rd'),
	(@Addr10, @Org2, '10 Test', '10 Test Rd'),
	(@Addr11, @Org2, '11 Test', '11 Test Rd'),
	(@Addr12, @Org2, '12 Test', '12 Test Rd'),
	(@Addr13, @Org2, '13 Test', '13 Test Rd')

insert into dbo.OrgAddressCapability (PZ_PK, PZ_OA, PZ_AddressType, PZ_IsMainAddress)
values
	(NEWID(), @Addr1, 'ARM', 1),
	(NEWID(), @Addr2, 'APM', 1),
	(NEWID(), @Addr3, 'OFC', 1),
	(NEWID(), @Addr4, 'ARM', 1),
	(NEWID(), @Addr5, 'APM', 0),
	(NEWID(), @Addr6, 'OFC', 0),
	(NEWID(), @Addr11, 'ARM', 1),
	(NEWID(), @Addr12, 'APM', 1),
	(NEWID(), @Addr13, 'OFC', 1)
";
			TestConnection.ExecuteNonQuery(insertSql);

			//Take Main ARM for org1 & org2
			var sql = $"SELECT * FROM GetTransactionOrgAddressWithFallback(NULL,NULL,NULL,'{org1Pk}','INV','AR')";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, sql);
			AssertEquals("Result should have rows", 1, result.Rows.Count);
			AssertEquals("1 Test", result.Rows[0]["OA_Code"]);

			sql = $"SELECT * FROM GetTransactionOrgAddressWithFallback(NULL,NULL,NULL,'{org2Pk}','INV','AR')";
			result = DataUtils.GetDataTableFromQuery(TestConnection, sql);
			AssertEquals("Result should have rows", 1, result.Rows.Count);
			AssertEquals("11 Test", result.Rows[0]["OA_Code"]);

			//Take Main APM for org1 & org2
			sql = $"SELECT * FROM GetTransactionOrgAddressWithFallback(NULL,NULL,NULL,'{org1Pk}','INV','AP')";
			result = DataUtils.GetDataTableFromQuery(TestConnection, sql);
			AssertEquals("Result should have rows", 1, result.Rows.Count);
			AssertEquals("2 Test", result.Rows[0]["OA_Code"]);

			sql = $"SELECT * FROM GetTransactionOrgAddressWithFallback(NULL,NULL,NULL,'{org2Pk}','INV','AP')";
			result = DataUtils.GetDataTableFromQuery(TestConnection, sql);
			AssertEquals("Result should have rows", 1, result.Rows.Count);
			AssertEquals("12 Test", result.Rows[0]["OA_Code"]);

			//Take Main OFC for org1 & org2
			sql = $"SELECT * FROM GetTransactionOrgAddressWithFallback(NULL,NULL,NULL,'{org1Pk}','JNL','GL')";
			result = DataUtils.GetDataTableFromQuery(TestConnection, sql);
			AssertEquals("Result should have rows", 1, result.Rows.Count);
			AssertEquals("3 Test", result.Rows[0]["OA_Code"]);

			sql = $"SELECT * FROM GetTransactionOrgAddressWithFallback(NULL,NULL,NULL,'{org2Pk}','JNL','GL')";
			result = DataUtils.GetDataTableFromQuery(TestConnection, sql);
			AssertEquals("Result should have rows", 1, result.Rows.Count);
			AssertEquals("13 Test", result.Rows[0]["OA_Code"]);

			//Take @AH_OA_InvoiceAddressOverride
			sql = $"SELECT * FROM GetTransactionOrgAddressWithFallback('{addr7Pk}','{addr8Pk}','{addr9Pk}','{org1Pk}','INV','AR')";
			result = DataUtils.GetDataTableFromQuery(TestConnection, sql);
			AssertEquals("Result should have rows", 1, result.Rows.Count);
			AssertEquals("7 Test", result.Rows[0]["OA_Code"]);

			//Take @JH_OA_LocalChargesAddr
			sql = $"SELECT * FROM GetTransactionOrgAddressWithFallback(NULL,'{addr8Pk}','{addr9Pk}','{org1Pk}','INV','AR')";
			result = DataUtils.GetDataTableFromQuery(TestConnection, sql);
			AssertEquals("Result should have rows", 1, result.Rows.Count);
			AssertEquals("8 Test", result.Rows[0]["OA_Code"]);

			//Take @JH_OA_AgentCollectAddr
			sql = $"SELECT * FROM GetTransactionOrgAddressWithFallback(NULL,NULL,'{addr9Pk}','{org1Pk}','INV','AR')";
			result = DataUtils.GetDataTableFromQuery(TestConnection, sql);
			AssertEquals("Result should have rows", 1, result.Rows.Count);
			AssertEquals("9 Test", result.Rows[0]["OA_Code"]);

			//OA_OH not match when incoming @AH_OA_InvoiceAddressOverride => Take @AH_OA_InvoiceAddressOverride
			sql = $"SELECT * FROM GetTransactionOrgAddressWithFallback('{addr1Pk}', NULL,NULL,'{org2Pk}','JNL','GL')";
			result = DataUtils.GetDataTableFromQuery(TestConnection, sql);
			AssertEquals("Result should have rows", 1, result.Rows.Count);
			AssertEquals("1 Test", result.Rows[0]["OA_Code"]);

			//OA_OH not match when incoming @JH_OA_LocalChargesAddr => Take Main OFC
			sql = $"SELECT * FROM GetTransactionOrgAddressWithFallback(NULL,'{addr1Pk}',NULL,'{org2Pk}','JNL','GL')";
			result = DataUtils.GetDataTableFromQuery(TestConnection, sql);
			AssertEquals("Result should have rows", 1, result.Rows.Count);
			AssertEquals("13 Test", result.Rows[0]["OA_Code"]);

			//OA_OH not match when incoming @JH_OA_AgentCollectAddr => Take Main OFC
			sql = $"SELECT * FROM GetTransactionOrgAddressWithFallback(NULL, NULL,'{addr1Pk}','{org2Pk}','JNL','GL')";
			result = DataUtils.GetDataTableFromQuery(TestConnection, sql);
			AssertEquals("Result should have rows", 1, result.Rows.Count);
			AssertEquals("13 Test", result.Rows[0]["OA_Code"]);

			//Null address
			sql = $"SELECT * FROM GetTransactionOrgAddressWithFallback(NULL,NULL,NULL,'{org3Pk}','INV','AR')";
			result = DataUtils.GetDataTableFromQuery(TestConnection, sql);
			AssertEquals("Result should NOT have rows", 0, result.Rows.Count);
		}
	}
}

