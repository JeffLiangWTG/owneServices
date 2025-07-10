using System;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.ClientSpecific.EDI
{
	[TestedType(typeof(AddAuditColumnsToEdiTokenAuthOnBoardingDataTransformation))]
	internal class AddAuditColumnsToEdiTokenAuthOnBoardingDataTransformationTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new AddAuditColumnsToEdiTokenAuthOnBoardingDataTransformation();
		}

		protected override void PrepareTestData()
		{
			DbObjectCreator.CreateTableIfNotExists(TestConnection, "EdiTokenAuthOnBoardingData", @"
CREATE TABLE dbo.EdiTokenAuthOnBoardingData 
(
	[TOD_PK] uniqueidentifier NOT NULL,
	[TOD_SystemUniqueIdentifier] varchar(100) NOT NULL,
	[TOD_LE] uniqueidentifier NOT NULL,
	[TOD_IDT] uniqueidentifier NOT NULL,
	[TOD_VerificationUsername] nvarchar(254) NOT NULL DEFAULT '',
	[TOD_VerificationUserPassword] nvarchar(35) NOT NULL DEFAULT '',
	[TOD_ConfigurationIdentifier] varchar(20) NOT NULL,
	[TOD_ClaimMappingName] varchar(50) NOT NULL,
	[TOD_ClaimMappingIdentifier] varchar(50) NOT NULL,
	[TOD_Status] varchar(3) NOT NULL DEFAULT 'NEW',
	[TOD_IM] uniqueidentifier NOT NULL,
	[TOD_Retry] int NOT NULL DEFAULT 0,
	[TOD_OIDCServer] varchar(3) NOT NULL DEFAULT 'AZU',
	[TOD_ValidTokenIssuerPrefix] varchar(100) NOT NULL DEFAULT '',
	[TOD_StagingPRLink] varchar(200) NOT NULL DEFAULT '',
	[TOD_ProdPRLink] varchar(200) NOT NULL DEFAULT '',
	[TOD_Enabled] bit NOT NULL DEFAULT 0
)");

			var insertSql = @$"
insert into EdiTokenAuthOnBoardingData values ('{guid1}', '96DE8ED6-C518-4340-B95F-07C784905494', newid(), newid(), 'testuser', 'testpassword', 'Azure', 'user_name', 'GS_LoginName', 'NEW', newid(), 0, 'AZU', '', '', '', 1);
insert into EdiTokenAuthOnBoardingData values ('{guid2}', 'B837D6FE-9B85-4E19-A0C0-A22EBA3DD871', newid(), newid(), 'testuser', 'testpassword', 'Azure', 'user_name', 'GS_LoginName', 'NEW', newid(), 0, 'AZU', '', '', '', 1);
insert into EdiTokenAuthOnBoardingData values ('{guid3}', 'C0D19E06-A91C-41C4-A6E0-BB8606C5F472', newid(), newid(), 'testuser', 'testpassword', 'Azure', 'user_name', 'GS_LoginName', 'NEW', newid(), 0, 'AZU', '', '', '', 1);";

			TestConnection.ExecuteNonQuery(insertSql);

			var insertStmAlog = @$"INSERT INTO dbo.StmALog (SL_PK, SL_Table, SL_Parent, SL_PostedTimeUtc, SL_EventTime, SL_SE_NKEvent, SL_GS_NKUser) values
(newid(), 'EdiTokenAuthOnBoardingData', '{guid1}', '2025-5-1', '2025-5-2', 'ADD', 'AAA'),
(newid(), 'EdiTokenAuthOnBoardingData', '{guid3}', '2025-5-2', '2025-5-4', 'ADD', 'CCC'),
(newid(), 'EdiTokenAuthOnBoardingData', '{guid1}', '2025-5-4', '2025-5-5', 'EDT', 'DDD'),
(newid(), 'EdiTokenAuthOnBoardingData', '{guid2}', '2025-5-5', '2025-5-6', 'EDT', 'EEE')
";
			TestConnection.ExecuteNonQuery(insertStmAlog);
		}

		Guid guid1 = Guid.NewGuid();
		Guid guid2 = Guid.NewGuid();
		Guid guid3 = Guid.NewGuid();

		protected override void AssertTransformationResults()
		{
			AssertData(guid1, new DateTime(2025, 5, 1), new DateTime(2025, 5, 4), "AAA", "DDD");
			AssertData(guid2, new DateTime(2023, 4, 21), new DateTime(2025, 5, 5), "~BP", "EEE");
			AssertData(guid3, new DateTime(2025, 5, 2), new DateTime(2025, 5, 2), "CCC", "CCC");
		}

		void AssertData(Guid pk, DateTime createTime, DateTime lastEditTime, string createUser, string lastEditUser)
		{
			AssertEquals(createTime, TestConnection.ExecuteScalar<DateTime>($"select TOD_SystemCreateTimeUtc from EdiTokenAuthOnBoardingData where TOD_PK = '{pk}'"));
			AssertEquals(lastEditTime, TestConnection.ExecuteScalar<DateTime>($"select TOD_SystemLastEditTimeUtc from EdiTokenAuthOnBoardingData where TOD_PK = '{pk}'"));
			AssertEquals(createUser, TestConnection.ExecuteScalar<string>($"select TOD_SystemCreateUser from EdiTokenAuthOnBoardingData where TOD_PK = '{pk}'"));
			AssertEquals(lastEditUser, TestConnection.ExecuteScalar<string>($"select TOD_SystemLastEditUser from EdiTokenAuthOnBoardingData where TOD_PK = '{pk}'"));
		}
	}
}
