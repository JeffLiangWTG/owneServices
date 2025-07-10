using System;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.DocumentScanning;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.DocumentScanning
{
	[TestedType(typeof(RemoveStorageDocsAuditLogs))]
	public class RemoveStorageDocsAuditLogsTest : DataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			var query = "Select * from dbo.StmALog where SL_Table='StorageDocs'";

			using (var adminConnection = Db.NewAdminConnection())
			{
				var logs = DataUtils.GetDataTableFromQuery(adminConnection, query);
				AssertEquals(0, logs.Rows.Count);
			}
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new RemoveStorageDocsAuditLogs();
		}

		protected override void PrepareTestData()
		{
			var storageDocId = Guid.NewGuid();
			var sqlText = $@"
insert into StorageDocs(SC_PK, SC_DataType, SC_DocType, SC_Desc, SC_FileName, SC_Date, SC_IsDeleted, SC_IsPublished, SC_IsSystemGenerated, SC_SaveVersions, SC_ImageData, SC_ParentID, SC_SM, SC_GE_Department, SC_GC_Company, SC_GB_Branch, SC_SystemCreateTimeUtc, SC_SystemLastEditTimeUtc, SC_SystemCreateUser, SC_SystemLastEditUser)
values('{storageDocId}', 'AAA', 'AAA', 'AAA', 'Test3', '2021-04-25 23:59:59', 'N', 'Y', 'N', 'Y', CAST(1 as VARBINARY), NEWID(), NEWID(), NEWID(), NEWID(), NEWID(), '2021-04-25 23:59:59', '2021-04-25 23:59:59', 'E','E')
insert into StmALog (SL_PK, SL_Reference, SL_SE_NKEvent, SL_EventTime, SL_PostedTimeUtc, SL_Parent, SL_Table, SL_GS_NKUser) 
values(NEWID(), 'Discount 5%', 'ADD', '2021-04-25 23:59:59', '2021-04-25 23:59:59', '{storageDocId}', 'StorageDocs', 'E')
insert into StmALog(SL_PK, SL_Reference, SL_SE_NKEvent, SL_EventTime, SL_PostedTimeUtc, SL_Parent, SL_Table, SL_GS_NKUser)
values(NEWID(), 'Discount 5%', 'EDT', '2021-10-12 07:00:00', '2021-10-12 06:00:00', '{storageDocId}', 'StorageDocs', 'E')
insert into StmALog (SL_PK, SL_Reference, SL_SE_NKEvent, SL_EventTime, SL_PostedTimeUtc, SL_Parent, SL_Table, SL_GS_NKUser) 
values(NEWID(), 'Discount 5%', 'ADD', '2022-10-11 07:00:00', '2022-10-11 06:00:00', '{storageDocId}', 'StorageDocs', 'E')
insert into StmALog (SL_PK, SL_Reference, SL_SE_NKEvent, SL_EventTime, SL_PostedTimeUtc, SL_Parent, SL_Table, SL_GS_NKUser) 
values(NEWID(), 'Discount 5%', 'EDT', '2022-11-11 07:00:00', '2022-11-11 06:00:00', '{storageDocId}', 'StorageDocs', 'E')
insert into StmALog (SL_PK, SL_Reference, SL_SE_NKEvent, SL_EventTime, SL_PostedTimeUtc, SL_Parent, SL_Table, SL_GS_NKUser) 
values(NEWID(), 'Discount 5%', 'ADD', '2022-10-11 07:00:00', '2022-10-11 06:00:00', '{storageDocId}', 'StorageDocs', 'E')
insert into StmALog(SL_PK, SL_Reference, SL_SE_NKEvent, SL_EventTime, SL_PostedTimeUtc, SL_Parent, SL_Table, SL_GS_NKUser)
values(NEWID(), 'Discount 5%', 'EDT', '2022-10-13 07:00:00', '2022-10-13 06:00:00', '{storageDocId}', 'StorageDocs', 'E')
insert into StmALog(SL_PK, SL_Reference, SL_SE_NKEvent, SL_EventTime, SL_PostedTimeUtc, SL_Parent, SL_Table, SL_GS_NKUser)
values(NEWID(), 'Discount 5%', 'EDT', '2022-10-14 07:00:00', '2022-10-14 06:00:00', '{storageDocId}', 'StorageDocs', 'E')
insert into StmALog(SL_PK, SL_Reference, SL_SE_NKEvent, SL_EventTime, SL_PostedTimeUtc, SL_Parent, SL_Table, SL_GS_NKUser)
values(NEWID(), 'Discount 5%', 'ADD', '2022-11-11 06:00:00', '2022-11-11 06:00:00', '{storageDocId}', 'StorageDocs', 'E')
insert into StmALog(SL_PK, SL_Reference, SL_SE_NKEvent, SL_EventTime, SL_PostedTimeUtc, SL_Parent, SL_Table, SL_GS_NKUser)
values(NEWID(), 'Discount 5%', 'EDT', '2022-11-14 06:00:00', '2022-11-14 06:00:00', '{storageDocId}', 'StorageDocs', 'E')
";
			_ = Db.Connection.Command(sqlText).ExecuteNonQuery();

			ExtProperty.Database.Update(Db.Connection, RemoveStorageDocsAuditLogs.RemoveStorageDocsAuditLogsProcessed, "2021-04-25 23:59:59");
		}
	}
}
