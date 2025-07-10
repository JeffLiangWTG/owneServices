using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.IE;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Customs.IE
{
	[TestedType(typeof(InsertNctsPhase5ArrivalRegisteredCusEntryNum))]
	public class InsertNctsPhase5ArrivalRegisteredCusEntryNumTest : DataTransformationTestCase
	{
		public override string[] expectedIndex => new string[]
		{
			"NONCLUSTERED INDEX [_WTG__Insert Registered CusEntryNum for IE NCTS Arrival Reports_1] ON [dbo].[EDIMessage] ([EM_MessageType], [EM_ApplicationCode], [EM_Status]) WHERE (([EM_MessageType] IN ('028', '043')) AND [EM_ApplicationCode]='IEN' AND [EM_Status]='PRS') WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)"
		};

		protected override void AssertPreConditions()
		{
			var sql = @"SELECT CE_EntryNum
				FROM dbo.CusEntryNum
				WHERE CE_ParentTable = 'CusInBondHeader'
				  AND CE_ParentID IN (@declarationPK1, @declarationPK2)
				  AND CE_EntryType = 'REG'
				  AND CE_Category = 'CUS'";

			var resultList = new List<string>();
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@declarationPK1", SqlDbType.UniqueIdentifier, declarationPK1);
				command.AddParameter("@declarationPK2", SqlDbType.UniqueIdentifier, declarationPK2);

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						resultList.Add(reader.GetString(0));
					}
				}
			}

			AssertEquals(1, resultList.Count);
			AssertEquals("345678", resultList[0]);
		}

		protected override void AssertTransformationResults()
		{
			var sql = @"SELECT CE_EntryLineReference, CE_EntryNum, CE_IssueDate, CE_RN_NKCountryCode
				FROM dbo.CusEntryNum
				WHERE CE_ParentTable = 'CusInBondHeader'
				  AND CE_ParentID IN (@declarationPK1, @declarationPK2)
				  AND CE_EntryType = 'REG'
				  AND CE_Category = 'CUS'";

			var resultList = new List<Tuple<string, string, DateTime, string>>();
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@declarationPK1", SqlDbType.UniqueIdentifier, declarationPK1);
				command.AddParameter("@declarationPK2", SqlDbType.UniqueIdentifier, declarationPK2);

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						resultList.Add(Tuple.Create(reader.GetString(0), reader.GetString(1), reader.GetDateTime(2), reader.GetString(3)));
					}
				}
			}

			AssertEquals(2, resultList.Count);
			AssertContainsExactElementsInAnyOrder(new[]
			{
				Tuple.Create("TA-PRCHEA", "123456", new DateTime(2024, 6, 6, 17, 42, 0), "IE"),
				Tuple.Create("TA-PRCHEA", "345678", new DateTime(2024, 6, 6, 17, 44, 0), "IE"),
			}, resultList);
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new InsertNctsPhase5ArrivalRegisteredCusEntryNum();

		protected override void PrepareTestData()
		{
			declarationPK1 = Guid.NewGuid();
			declarationPK2 = Guid.NewGuid();

			var sqlText = @"
DECLARE @IECompanyPK UNIQUEIDENTIFIER = NEWID();
INSERT INTO dbo.GlbCompany(GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_SystemCreateTimeUtc, GC_SystemCreateUser, GC_SystemLastEditTimeUtc, GC_SystemLastEditUser) VALUES (@IECompanyPK, 'CIE', 'IE company', 'IE', '2024-06-06 12:12:00', 'AAA', '2024-06-06 12:12:00', 'AAA');
DECLARE @IEBranchPK UNIQUEIDENTIFIER = NEWID();
INSERT INTO dbo.GlbBranch(GB_PK, GB_GC, GB_Code, GB_SystemCreateTimeUtc, GB_SystemCreateUser, GB_SystemLastEditTimeUtc, GB_SystemLastEditUser) VALUES(@IEBranchPK, @IECompanyPK, 'BIE', '2024-06-06 12:12:00', 'AAA', '2024-06-06 12:12:00', 'AAA');
DECLARE @IEDepartmentPK UNIQUEIDENTIFIER = NEWID();
INSERT INTO dbo.GlbDepartment(GE_PK, GE_Code, GE_SystemCreateTimeUtc, GE_SystemCreateUser, GE_SystemLastEditTimeUtc, GE_SystemLastEditUser) VALUES(@IEDepartmentPK, 'DIE', '2024-06-06 12:12:00', 'AAA', '2024-06-06 12:12:00', 'AAA');

DECLARE @DestinationTraderHeaderPK UNIQUEIDENTIFIER = NEWID();
INSERT INTO dbo.OrgHeader(OH_PK, OH_Code, OH_ISActive, OH_SystemCreateTimeUtc, OH_SystemCreateUser, OH_SystemLastEditTimeUtc, OH_SystemLastEditUser) VALUES (@DestinationTraderHeaderPK, 'PRCHEA', 1, '2024-06-06 12:12:00', 'AAA', '2024-06-06 12:12:00', 'AAA');
DECLARE @DestinationTraderAddressPK UNIQUEIDENTIFIER = NEWID();
INSERT INTO dbo.OrgAddress(OA_PK, OA_OH, OA_Code, OA_IsActive, OA_Address1, OA_RN_NKCountryCode, OA_SystemCreateTimeUtc, OA_SystemCreateUser, OA_SystemLastEditTimeUtc, OA_SystemLastEditUser) VALUES(@DestinationTraderAddressPK, @DestinationTraderHeaderPK, 'PRCADD', 1, 'Address Line 1', 'IE', '2024-06-06 12:12:00', 'AAA', '2024-06-06 12:12:00', 'AAA');

INSERT INTO dbo.CusInBondHeader(BH_PK, BH_GB, BH_ApplicationCode, BH_HeaderType, BH_SystemCreateTimeUtc, BH_SystemCreateUser, BH_SystemLastEditTimeUtc, BH_SystemLastEditUser)
	VALUES (@declarationPK1, @IEBranchPK, 'NC5', 'A', '2024-06-06 17:12:00', 'AAA', '2024-06-06 17:12:00', 'AAA');
INSERT INTO dbo.CusInBondMoveHeader(BM_PK, BM_BH, BM_SubApplicationCode, BM_PaperlessInbondNum, BM_SystemCreateTimeUtc, BM_SystemCreateUser, BM_SystemLastEditTimeUtc, BM_SystemLastEditUser)
	VALUES (NEWID(), @declarationPK1, 'A', '123456', '2024-06-06 17:12:00', 'AAA', '2024-06-06 17:12:00', 'AAA');
INSERT INTO dbo.CusInBondMoveHeader(BM_PK, BM_BH, BM_SubApplicationCode, BM_PaperlessInbondNum, BM_SystemCreateTimeUtc, BM_SystemCreateUser, BM_SystemLastEditTimeUtc, BM_SystemLastEditUser)
	VALUES (NEWID(), @declarationPK1, 'U', '', '2024-06-06 17:12:00', 'AAA', '2024-06-06 17:12:00', 'AAA');
INSERT INTO dbo.JobDocAddress(E2_PK, E2_IsValid, E2_AddressType, E2_OA_Address, E2_ParentID, E2_ParentTableCode, E2_SystemCreateTimeUtc, E2_SystemCreateUser, E2_SystemLastEditTimeUtc, E2_SystemLastEditUser)
	VALUES (NEWID(), 1, 'IMD', @DestinationTraderAddressPK, @declarationPK1, 'BH', '2024-06-06 17:12:00', 'AAA', '2024-06-06 17:12:00', 'AAA');
INSERT INTO dbo.EDIMessage(EM_PK, EM_IsActive, EM_ApplicationCode, EM_MessageType, EM_ReceiveTransmit, EM_Status, EM_LinkTable, EM_LinkUniqueID, EM_GB, EM_GE, EM_SystemCreateTimeUtc, EM_SystemCreateUser, EM_SystemLastEditTimeUtc, EM_SystemLastEditUser)
	VALUES (NEWID(), 1, 'IEN', '043', 'RCV', 'PRS', 'CusInBondHeader', @declarationPK1, @IEBranchPK, @IEDepartmentPK, '2024-06-06 17:42:00', 'E', '2024-06-06 17:42:00', '~BP');

INSERT INTO dbo.CusInBondHeader(BH_PK, BH_GB, BH_ApplicationCode, BH_HeaderType, BH_SystemCreateTimeUtc, BH_SystemCreateUser, BH_SystemLastEditTimeUtc, BH_SystemLastEditUser)
	VALUES (@declarationPK2, @IEBranchPK, 'NC5', 'A', '2024-06-06 17:14:00', 'BBB', '2024-06-06 17:14:00', 'BBB');
INSERT INTO dbo.CusInBondMoveHeader(BM_PK, BM_BH, BM_SubApplicationCode, BM_PaperlessInbondNum, BM_SystemCreateTimeUtc, BM_SystemCreateUser, BM_SystemLastEditTimeUtc, BM_SystemLastEditUser)
	VALUES (NEWID(), @declarationPK2, 'A', '345678', '2024-06-06 17:14:00', 'BBB', '2024-06-06 17:14:00', 'BBB');
INSERT INTO dbo.CusInBondMoveHeader(BM_PK, BM_BH, BM_SubApplicationCode, BM_PaperlessInbondNum, BM_SystemCreateTimeUtc, BM_SystemCreateUser, BM_SystemLastEditTimeUtc, BM_SystemLastEditUser)
	VALUES (NEWID(), @declarationPK2, 'U', '', '2024-06-06 17:14:00', 'BBB', '2024-06-06 17:14:00', 'BBB');
INSERT INTO dbo.JobDocAddress(E2_PK, E2_IsValid, E2_AddressType, E2_OA_Address, E2_ParentID, E2_ParentTableCode, E2_SystemCreateTimeUtc, E2_SystemCreateUser, E2_SystemLastEditTimeUtc, E2_SystemLastEditUser)
	VALUES (NEWID(), 1, 'IMD', @DestinationTraderAddressPK, @declarationPK2, 'BH', '2024-06-06 17:14:00', 'AAA', '2024-06-06 17:14:00', 'AAA');
INSERT INTO dbo.EDIMessage(EM_PK, EM_IsActive, EM_ApplicationCode, EM_MessageType, EM_ReceiveTransmit, EM_Status, EM_LinkTable, EM_LinkUniqueID, EM_GB, EM_GE, EM_SystemCreateTimeUtc, EM_SystemCreateUser, EM_SystemLastEditTimeUtc, EM_SystemLastEditUser)
	VALUES (NEWID(), 1, 'IEN', '043', 'RCV', 'PRS', 'CusInBondHeader', @declarationPK2, @IEBranchPK, @IEDepartmentPK, '2024-06-06 17:44:00', 'E', '2024-06-06 17:44:00', '~BP');
INSERT INTO dbo.CusEntryNum(CE_PK, CE_IsValid, CE_ParentID, CE_ParentTable, CE_EntryNum, CE_Category, CE_EntryType, CE_EntryLineReference, CE_IssueDate, CE_RN_NKCountryCode, CE_SystemCreateTimeUtc, CE_SystemCreateUser, CE_SystemLastEditTimeUtc, CE_SystemLastEditUser)
	VALUES (NEWID(), 1, @declarationPK2, 'CusInBondHeader', '345678', 'CUS', 'REG', 'TA-PRCHEA', '2024-06-06 17:44:00', 'IE', '2024-06-06 17:44:00', 'E', '2024-06-06 17:44:00', '~BP');
";
			var cmd = Db.Connection.Command(sqlText);
			cmd.AddParameter("@declarationPK1", SqlDbType.UniqueIdentifier, declarationPK1);
			cmd.AddParameter("@declarationPK2", SqlDbType.UniqueIdentifier, declarationPK2);
			cmd.ExecuteNonQuery();
		}

		Guid declarationPK1;
		Guid declarationPK2;
	}
}
