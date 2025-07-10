using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.BE;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Customs.BE
{
	[TestedType(typeof(ChangeBE_CE_ParentTableAndCE_ParentIDFromHeaderToMovementHeaderNCTS))]
	class ChangeBE_CE_ParentTableAndCE_ParentIDFromHeaderToMovementHeaderNCTSTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new ChangeBE_CE_ParentTableAndCE_ParentIDFromHeaderToMovementHeaderNCTS();
		}

		public override string[] expectedIndex => new string[]
		{
			"NONCLUSTERED INDEX [_WTG__Link CusEntryNum to CusInBondMoveHeader where CE_EntryType is CID or REG. Only for BE._1] ON [dbo].[CusEntryNum] ([CE_ParentTable], [CE_Category], [CE_EntryType], [CE_RN_NKCountryCode]) INCLUDE ([CE_EntryStatus], [CE_SystemLastEditTimeUtc]) WHERE (([CE_EntryType] IN ('REG', 'CID')) AND [CE_ParentTable]='CusInBondHeader' AND [CE_Category]='CUS' AND [CE_RN_NKCountryCode]='BE') WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
		};

		protected override void PrepareTestData()
		{
			var sql = $@"
				DECLARE @CompanyPK          UNIQUEIDENTIFIER = NEWID(),
						@BranchPK           UNIQUEIDENTIFIER = NEWID(),
						@bhPK1				UNIQUEIDENTIFIER = NEWID(),
						@bhPK2				UNIQUEIDENTIFIER = NEWID(),
						@bhPK3				UNIQUEIDENTIFIER = NEWID(),
						@bhPK4				UNIQUEIDENTIFIER = NEWID(),
						@bmPK1				UNIQUEIDENTIFIER = '{bmPK1}',
						@bmPK2				UNIQUEIDENTIFIER = '{bmPK2}',
						@bmPK3				UNIQUEIDENTIFIER = '{bmPK3}',
						@bmPK4				UNIQUEIDENTIFIER = '{bmPK4}',
						@bmPK5				UNIQUEIDENTIFIER = '{bmPK5}',
						@entryNum1			UNIQUEIDENTIFIER = NEWID(),
						@entryNum2			UNIQUEIDENTIFIER = NEWID(),
						@entryNum3			UNIQUEIDENTIFIER = NEWID(),
						@entryNum4			UNIQUEIDENTIFIER = NEWID(),
						@entryNum5			UNIQUEIDENTIFIER = NEWID()

				DELETE FROM GlbCompany WHERE GC_PK = @CompanyPK
				DELETE FROM GlbBranch WHERE GB_PK = @BranchPK
				DELETE FROM CusInbondHeader WHERE BH_PK IN (@bhPK1, @bhPK2, @bhPK3)
				DELETE FROM CusEntryNum

				INSERT INTO GlbCompany (GC_PK, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency, GC_Code, GC_Name, GC_SystemCreateTimeUtc, GC_SystemCreateUser, GC_SystemLastEditTimeUtc, GC_SystemLastEditUser)
				VALUES (@CompanyPK, 'BE', 'EUR', 'DBE', 'BE company', GetUtcDate(), 'NLK', GetUtcDate(), 'NLK')

				INSERT INTO GlbBranch (GB_PK, GB_GC, GB_Code, GB_SystemCreateTimeUtc, GB_SystemCreateUser, GB_SystemLastEditTimeUtc, GB_SystemLastEditUser)
				VALUES (@BranchPK, @CompanyPK, 'BRN',GetUtcDate(), 'NLK', GetUtcDate(), 'NLK')
	

				INSERT INTO CusInbondHeader (BH_PK, BH_GB, BH_JobReference, BH_ApplicationCode, BH_SystemCreateTimeUtc, BH_IsActive, BH_SystemLastEditTimeUtc, BH_SystemLastEditUser, BH_SystemCreateUser) VALUES
					(@bhPK1, @BranchPK, 'BH01', 'NCT', '2022-07-30', 1, GetUtcDate(), 'NLK','NLK'),
					(@bhPK2, @BranchPK, 'BH02', 'NC5', '2022-12-15', 1, GetUtcDate(), 'NLK','NLK'),
					(@bhPK3, @BranchPK, 'BH03', 'NC5', '2023-03-02', 1, GetUtcDate(), 'NLK','NLK'),
					(@bhPK4, @BranchPK, 'BH04', 'NC5', '2024-01-01', 1, GetUtcDate(), 'NLK','NLK')

				INSERT INTO CusInbondMoveHeader (BM_PK, BM_BH, BM_SystemCreateTimeUtc, BM_SystemCreateUser, BM_SystemLastEditTimeUtc, BM_SystemLastEditUser, BM_EntryDate) VALUES
					(@bmPK1, @bhPK1, '2022-07-30', 'NLK', GetUtcDate(), 'NLK','2022-07-30'),
					(@bmPK2, @bhPK2, '2022-12-15', 'NLK', GetUtcDate(), 'NLK','2022-12-15'),
					(@bmPK3, @bhPK3, '2023-03-02', 'NLK', GetUtcDate(), 'NLK','2023-03-02'),
					(@bmPK4, @bhPK4, '2024-02-01', 'NLK', GetUtcDate(), 'NLK','2024-02-01')

				INSERT INTO CusEntryNum (CE_PK, CE_ParentTable, CE_ParentID, CE_SystemCreateTimeUtc, CE_SystemCreateUser, CE_SystemLastEditTimeUtc, CE_SystemLastEditUser, CE_EntryType, CE_EntryNum, CE_Category, CE_RN_NKCountryCode) VALUES
					(@entryNum1,'CusInBondHeader', @bhPK1, '2022-07-30', 'XCM', '2023-01-01 01:01:00', 'XCM', 'CID', 'TEST1', 'CUS', 'BE'),
					(@entryNum2,'CusInBondHeader', @bhPK1, '2022-07-30', 'XCM', '2023-01-01 01:01:00', 'XCM', 'REG', 'TEST2', 'CUS', 'BE'),
					(@entryNum3,'CusInBondHeader', @bhPK2, '2022-07-30', 'XCM', '2023-01-01 01:01:00', 'XCM', 'CID', 'TEST3', 'CUS', 'BE'),
					(@entryNum4,'CusInBondHeader', @bhPK3, '2022-07-30', 'XCM', '2023-01-01 01:01:00', 'XCM', 'REG', 'TEST4', 'CUS', 'BE'),
					(@entryNum5,'CusInBondHeader', @bhPK4, '2022-07-30', 'XCM', '2023-01-01 01:01:00', 'XCM', 'CID', 'TEST5', 'CUS', 'BE');";

			TestConnection.ExecuteNonQuery(sql);
		}

		protected override void AssertTransformationResults()
		{
			var resultList = new List<Tuple<Guid, string, string, string, string, DateTime>>();
			var utcDate = DateTime.UtcNow.ToShortDateString();
			TestConnection.ExecuteReader("SELECT * FROM CusEntryNum",
				reader => resultList.Add(Tuple.Create((Guid)reader["CE_ParentID"], (string)reader["CE_ParentTable"], (string)reader["CE_EntryType"], (string)reader["CE_EntryNum"], (string)reader["CE_SystemLastEditUser"], (DateTime)reader["CE_SystemLastEditTimeUtc"])));
			AssertContainsExactElementsInAnyOrder(new[]
			{
				$"{bmPK1}, CusInBondMoveHeader, CID, TEST1, ~BP, {utcDate}",
				$"{bmPK1}, CusInBondMoveHeader, REG, TEST2, ~BP, {utcDate}",
				$"{bmPK2}, CusInBondMoveHeader, CID, TEST3, ~BP, {utcDate}",
				$"{bmPK3}, CusInBondMoveHeader, REG, TEST4, ~BP, {utcDate}",
				$"{bmPK4}, CusInBondMoveHeader, CID, TEST5, ~BP, {utcDate}",
			}, resultList.Select(x => $"{x.Item1}, {x.Item2}, {x.Item3}, {x.Item4}, {x.Item5}, {x.Item6.ToShortDateString()}").ToArray());
		}
		Guid bmPK1 = Guid.NewGuid();
		Guid bmPK2 = Guid.NewGuid();
		Guid bmPK3 = Guid.NewGuid();
		Guid bmPK4 = Guid.NewGuid();
		Guid bmPK5 = Guid.NewGuid();
	}
}
