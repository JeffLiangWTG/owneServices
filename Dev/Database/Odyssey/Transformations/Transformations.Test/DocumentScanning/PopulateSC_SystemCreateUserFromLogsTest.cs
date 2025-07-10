using System;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.DocumentScanning;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.DocumentScanning
{
	[TestedType(typeof(PopulateSC_SystemCreateUserFromLogs))]
	[UseSnapshotProtection]
	public class PopulateSC_SystemCreateUserFromLogsTest : DataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			var query = "Select * from StorageDocs with(nolock)";

			using (var adminConnection = Db.NewAdminConnection())
			{
				var storageDocs = DataUtils.GetDataTableFromQuery(adminConnection, query);
				AssertEquals(1, storageDocs.Rows.Count);
				AssertEquals("Test3", storageDocs.Rows[0]["SC_FileName"]);
				AssertEquals("Create use is NOT changed to user1 as the create date is before 27-Apr-2021", ServiceUser, storageDocs.Rows[0]["SC_SystemCreateUser"]);
				AssertEquals("Last edit use is NOT changed to user2 as the create date is before 27-Apr-2021", ServiceUser, storageDocs.Rows[0]["SC_SystemLastEditUser"]);
			}

			using (var docConnection = Db.NewAdminConnection(docManagerDB))
			{
				var storageDocs1 = DataUtils.GetDataTableFromQuery(docConnection, query);
				AssertEquals(3, storageDocs1.Rows.Count);
				AssertEquals("Test4", storageDocs1.Rows[0]["SC_FileName"]);
				AssertEquals("Create user is changed to user1 after transform", User1, storageDocs1.Rows[0]["SC_SystemCreateUser"]);
				AssertEquals("Last edit user is changed to user2 after transform", User2, storageDocs1.Rows[0]["SC_SystemLastEditUser"]);

				AssertEquals("Test5", storageDocs1.Rows[1]["SC_FileName"]);
				AssertEquals("Create user is NOT changed to user2 as SC_SystemCreateUser is not ~BP", User1, storageDocs1.Rows[1]["SC_SystemCreateUser"]);
				AssertEquals("Last edit user is NOT changed to user2 as SC_SystemCreateUser is not ~BP", User1, storageDocs1.Rows[1]["SC_SystemLastEditUser"]);

				AssertEquals("Test6", storageDocs1.Rows[2]["SC_FileName"]);
				AssertEquals("Create user is changed to user2 as SC_SystemCreateUser is not ~BP", User2, storageDocs1.Rows[2]["SC_SystemCreateUser"]);
				AssertEquals("Last edit user is NOT changed to user2 as SC_SystemCreateUser is not ~BP", User2, storageDocs1.Rows[2]["SC_SystemLastEditUser"]);
			}
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new PopulateSC_SystemCreateUserFromLogs() { TopCount = 1 };
		}

		protected override void PrepareTestData()
		{
			var storageDocId1 = Guid.NewGuid();
			var storageDocId2 = Guid.NewGuid();
			var storageDocId3 = Guid.NewGuid();
			var storageDocId4 = Guid.NewGuid();
			var sqlText = $@"
insert into StorageDocs(SC_PK, SC_DataType, SC_DocType, SC_Desc, SC_FileName, SC_Date, SC_IsDeleted, SC_IsPublished, SC_IsSystemGenerated, SC_SaveVersions, SC_ImageData, SC_ParentID, SC_SM, SC_GE_Department, SC_GC_Company, SC_GB_Branch, SC_SystemCreateTimeUtc, SC_SystemLastEditTimeUtc, SC_SystemCreateUser, SC_SystemLastEditUser)
values('{storageDocId1}', 'AAA', 'AAA', 'AAA', 'Test3', '2021-04-25 23:59:59', 'N', 'Y', 'N', 'Y', CAST(1 as VARBINARY), NEWID(), NEWID(), NEWID(), NEWID(), NEWID(), '2021-04-25 23:59:59', '2021-04-25 23:59:59', '{ServiceUser}','{ServiceUser}')
insert into {docManagerDB}.dbo.StorageDocs(SC_PK, SC_DataType, SC_DocType, SC_Desc, SC_FileName, SC_Date, SC_IsDeleted, SC_IsPublished, SC_IsSystemGenerated, SC_SaveVersions, SC_ImageData, SC_ParentID, SC_SM, SC_GE_Department, SC_GC_Company, SC_GB_Branch, SC_SystemCreateTimeUtc, SC_SystemLastEditTimeUtc, SC_SystemCreateUser, SC_SystemLastEditUser)
values('{storageDocId2}', 'AAA', 'AAA', 'AAA', 'Test4', '2022-10-11 06:00:00', 'N', 'Y', 'N', 'Y', CAST(1 as VARBINARY), NEWID(), NEWID(), NEWID(), NEWID(), NEWID(), '2022-10-11 06:00:00', '2022-10-11 06:00:00', '{ServiceUser}','{ServiceUser}')
insert into {docManagerDB}.dbo.StorageDocs(SC_PK, SC_DataType, SC_DocType, SC_Desc, SC_FileName, SC_Date, SC_IsDeleted, SC_IsPublished, SC_IsSystemGenerated, SC_SaveVersions, SC_ImageData, SC_ParentID, SC_SM, SC_GE_Department, SC_GC_Company, SC_GB_Branch, SC_SystemCreateTimeUtc, SC_SystemLastEditTimeUtc, SC_SystemCreateUser, SC_SystemLastEditUser)
values('{storageDocId3}', 'AAA', 'AAA', 'AAA', 'Test5', '2022-10-11 06:00:00', 'N', 'Y', 'N', 'Y', CAST(1 as VARBINARY), NEWID(), NEWID(), NEWID(), NEWID(), NEWID(), '2022-10-11 06:00:00', '2022-10-11 06:00:00', '{User1}','{User1}')
insert into {docManagerDB}.dbo.StorageDocs(SC_PK, SC_DataType, SC_DocType, SC_Desc, SC_FileName, SC_Date, SC_IsDeleted, SC_IsPublished, SC_IsSystemGenerated, SC_SaveVersions, SC_ImageData, SC_ParentID, SC_SM, SC_GE_Department, SC_GC_Company, SC_GB_Branch, SC_SystemCreateTimeUtc, SC_SystemLastEditTimeUtc, SC_SystemCreateUser, SC_SystemLastEditUser)
values('{storageDocId4}', 'AAA', 'AAA', 'AAA', 'Test6', '2022-11-11 06:00:00', 'N', 'Y', 'N', 'Y', CAST(1 as VARBINARY), NEWID(), NEWID(), NEWID(), NEWID(), NEWID(), '2022-11-11 06:00:00', '2022-11-11 06:00:00', '{ServiceUser}','{ServiceUser}')
insert into StmALog (SL_PK, SL_Reference, SL_SE_NKEvent, SL_EventTime, SL_PostedTimeUtc, SL_Parent, SL_Table, SL_GS_NKUser) 
values(NEWID(), 'Discount 5%', 'ADD', '2021-04-25 23:59:59', '2021-04-25 23:59:59', '{storageDocId1}', 'StorageDocs', '{User1}')
insert into StmALog(SL_PK, SL_Reference, SL_SE_NKEvent, SL_EventTime, SL_PostedTimeUtc, SL_Parent, SL_Table, SL_GS_NKUser)
values(NEWID(), 'Discount 5%', 'EDT', '2021-10-12 07:00:00', '2021-10-12 06:00:00', '{storageDocId1}', 'StorageDocs', '{User2}')
insert into StmALog (SL_PK, SL_Reference, SL_SE_NKEvent, SL_EventTime, SL_PostedTimeUtc, SL_Parent, SL_Table, SL_GS_NKUser) 
values(NEWID(), 'Discount 5%', 'ADD', '2022-10-11 07:00:00', '2022-10-11 06:00:00', '{storageDocId2}', 'StorageDocs', '{User1}')
insert into StmALog (SL_PK, SL_Reference, SL_SE_NKEvent, SL_EventTime, SL_PostedTimeUtc, SL_Parent, SL_Table, SL_GS_NKUser) 
values(NEWID(), 'Discount 5%', 'EDT', '2022-11-11 07:00:00', '2022-11-11 06:00:00', '{storageDocId2}', 'StorageDocs', '{User2}')
insert into StmALog (SL_PK, SL_Reference, SL_SE_NKEvent, SL_EventTime, SL_PostedTimeUtc, SL_Parent, SL_Table, SL_GS_NKUser) 
values(NEWID(), 'Discount 5%', 'ADD', '2022-10-11 07:00:00', '2022-10-11 06:00:00', '{storageDocId3}', 'StorageDocs', '{User2}')
insert into StmALog(SL_PK, SL_Reference, SL_SE_NKEvent, SL_EventTime, SL_PostedTimeUtc, SL_Parent, SL_Table, SL_GS_NKUser)
values(NEWID(), 'Discount 5%', 'EDT', '2022-10-13 07:00:00', '2022-10-13 06:00:00', '{storageDocId3}', 'StorageDocs', '{User2}')
insert into StmALog(SL_PK, SL_Reference, SL_SE_NKEvent, SL_EventTime, SL_PostedTimeUtc, SL_Parent, SL_Table, SL_GS_NKUser)
values(NEWID(), 'Discount 5%', 'EDT', '2022-10-14 07:00:00', '2022-10-14 06:00:00', '{storageDocId3}', 'StorageDocs', '{User2}')
insert into StmALog(SL_PK, SL_Reference, SL_SE_NKEvent, SL_EventTime, SL_PostedTimeUtc, SL_Parent, SL_Table, SL_GS_NKUser)
values(NEWID(), 'Discount 5%', 'ADD', '2022-11-11 06:00:00', '2022-11-11 06:00:00', '{storageDocId4}', 'StorageDocs', '{User2}')
insert into StmALog(SL_PK, SL_Reference, SL_SE_NKEvent, SL_EventTime, SL_PostedTimeUtc, SL_Parent, SL_Table, SL_GS_NKUser)
values(NEWID(), 'Discount 5%', 'EDT', '2022-11-14 06:00:00', '2022-11-14 06:00:00', '{storageDocId4}', 'StorageDocs', '{User2}')
";
			Db.Connection.Command(sqlText).ExecuteNonQuery();
		}

		protected override void SetUp()
		{
			base.SetUp();
			using (var adminConnection = Db.NewAdminConnection())
			{
				AdoTestUtils.DropDbIfExists(adminConnection, docManagerDB);
				AdoTestUtils.CreateDbIfNotExists(adminConnection, docManagerDB);
			}
		}

		protected override void TearDown()
		{
			base.TearDown();
			using (var adminConnection = Db.NewAdminConnection())
			{
				AdoTestUtils.DropDbIfExists(adminConnection, docManagerDB);
			}
		}

		readonly string docManagerDB = Db.DatabaseName + "_SD001";
		const string ServiceUser = "~BP";
		const string User1 = "ABC";
		const string User2 = "DEF";
	}
}
