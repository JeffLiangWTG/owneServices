using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.EU;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Customs.EU
{
	[TestedType(typeof(LinkNctsPhase5DepartureDeclarationEdiMessageToCusInbondMoveHeader))]
	abstract class LinkNctsPhase5DepartureDeclarationEdiMessageToCusInbondMoveHeaderTest : DataTransformationTestCase
	{
		protected abstract string TargetCountryCode { get; }

		protected abstract string ForeignCountryCode { get; }

		protected override void PrepareTestData()
		{
			var targetBranchPK = TestDataCreator.CreateBranch(TestDataCreator.CreateCompany("TC1", TargetCountryCode, "CUR"), "TB1", $"{TargetCountryCode} - Port", TargetCountryCode);
			var foreignBranchPK = TestDataCreator.CreateBranch(TestDataCreator.CreateCompany("FC1", ForeignCountryCode, "CUR"), "FB1", $"{ForeignCountryCode} - Port", ForeignCountryCode);
			var departmentPK = TestDataCreator.CreateDepartment("DEP1");

			var sql = $@"
DECLARE @bhPK2 AS UNIQUEIDENTIFIER = NEWID();
DECLARE @bhPK6 AS UNIQUEIDENTIFIER = NEWID();

INSERT INTO [dbo].[CusInBondHeader] ([BH_PK]   , [BH_GB]            , [BH_JobReference], [BH_ApplicationCode], [BH_IsActive], [BH_SystemCreateTimeUtc], [BH_SystemLastEditTimeUtc], [BH_SystemLastEditUser], [BH_SystemCreateUser])
                             VALUES ('{bhPK1}' , '{targetBranchPK}' , 'BH01'           , 'NCT'               , 1            , GetUtcDate()            , GetUtcDate()              , '~BP'                  ,'~BP'),
                                    (@bhPK2    , '{targetBranchPK}' , 'BH02'           , 'NC5'               , 1            , GetUtcDate()            , GetUtcDate()              , '~BP'                  ,'~BP'),
                                    ('{bhPK3}' , '{targetBranchPK}' , 'BH03'           , 'NC5'               , 1            , GetUtcDate()            , GetUtcDate()              , '~BP'                  ,'~BP'),
                                    ('{bhPK4}' , '{targetBranchPK}' , 'BH04'           , 'NC5'               , 1            , GetUtcDate()            , GetUtcDate()              , '~BP'                  ,'~BP'),
                                    ('{bhPK5}' , '{foreignBranchPK}', 'BH05'           , 'NC5'               , 1            , GetUtcDate()            , GetUtcDate()              , '~BP'                  ,'~BP'),
                                    (@bhPK6    , '{targetBranchPK}' , 'BH06'           , 'NC5'               , 1            , GetUtcDate()            , GetUtcDate()              , '~BP'                  ,'~BP');

INSERT INTO [dbo].[CusInbondMoveHeader] ([BM_PK]   , [BM_BH]   , [BM_SubApplicationCode], [BM_SystemCreateTimeUtc], [BM_SystemLastEditUser], [BM_SystemLastEditTimeUtc], [BM_SystemCreateUser])
                                 VALUES (NEWID()   , '{bhPK1}' , 'D'                    , GetUtcDate()            , '~BP'                  , GetUtcDate()              , '~BP'),
                                        ('{bmPK2}' , @bhPK2    , 'D'                    , GetUtcDate()            , '~BP'                  , GetUtcDate()              , '~BP'),
                                        (NEWID()   , '{bhPK3}' , 'DA'                   , GetUtcDate()            , '~BP'                  , GetUtcDate()              , '~BP'),
                                        (NEWID()   , '{bhPK4}' , 'A'                    , GetUtcDate()            , '~BP'                  , GetUtcDate()              , '~BP'),
                                        (NEWID()   , '{bhPK5}' , 'D'                    , GetUtcDate()            , '~BP'                  , GetUtcDate()              , '~BP'),
                                        ('{bmPK6}' , @bhPK6    , 'D'                    , GetUtcDate()            , '~BP'                  , GetUtcDate()              , '~BP');

INSERT INTO [dbo].[EDIMessage] ([EM_PK]   , [EM_GB]            , [EM_GE]         , [EM_LinkTable]       , [EM_LinkUniqueID], [EM_SystemCreateTimeUtc], [EM_MessageNum], [EM_SystemLastEditUser], [EM_SystemLastEditTimeUtc], [EM_SystemCreateUser])
                        VALUES ('{emPK1}' , '{targetBranchPK}' , '{departmentPK}', 'CusInBondHeader'    , '{bhPK1}'        , GETDATE()               , '1'            , '~BP'                  , GETDATE()                 , '~BP'),
	                           ('{emPK2}' , '{targetBranchPK}' , '{departmentPK}', 'CusInBondHeader'    , @bhPK2           , GETDATE()               , '2'            , '~BP'                  , GETDATE()                 , '~BP'),
	                           ('{emPK3}' , '{targetBranchPK}' , '{departmentPK}', 'CusInBondHeader'    , '{bhPK3}'        , GETDATE()               , '3'            , '~BP'                  , GETDATE()                 , '~BP'),
	                           ('{emPK4}' , '{targetBranchPK}' , '{departmentPK}', 'CusInBondHeader'    , '{bhPK4}'        , GETDATE()               , '4'            , '~BP'                  , GETDATE()                 , '~BP'),
	                           ('{emPK5}' , '{foreignBranchPK}', '{departmentPK}', 'CusInBondHeader'    , '{bhPK5}'        , GETDATE()               , '5'            , '~BP'                  , GETDATE()                 , '~BP'),
	                           ('{emPK6}' , '{targetBranchPK}' , '{departmentPK}', 'CusInBondMoveHeader', '{bmPK6}'        , GETDATE()               , '6'            , '~BP'                  , GETDATE()                 , '~BP');
			";

			TestConnection.ExecuteNonQuery(sql);
		}

		protected override void AssertTransformationResults() => CombineAssertions(() =>
		{
			var ediMessages = LoadEDIMessages(emPK1, emPK2, emPK3, emPK4, emPK5, emPK6);

			AssertEDIMessage(emPK1, bhPK1, "CusInBondHeader", "Other BH_ApplicationCode: No changes");
			AssertEDIMessage(emPK2, bmPK2, "CusInBondMoveHeader", "BH_HeaderType = 'D': Change linking");
			AssertEDIMessage(emPK3, bhPK3, "CusInBondHeader", "BH_HeaderType = 'DA': No changes");
			AssertEDIMessage(emPK4, bhPK4, "CusInBondHeader", "BH_HeaderType = 'A': No changes");
			AssertEDIMessage(emPK5, bhPK5, "CusInBondHeader", "Other GC_RN_NKCountryCode: No changes");
			AssertEDIMessage(emPK6, bmPK6, "CusInBondMoveHeader", "Already linked to CusInBondMoveHeader: No changes");

			List<(Guid emPk, string linkedTable, Guid linkUniqueID)> LoadEDIMessages(params Guid[] emPks)
			{
				var sql = $"SELECT [EM_PK], [EM_LinkTable], [EM_LinkUniqueID] FROM [dbo].[EDIMessage] WHERE [EM_PK] IN ({string.Join(", ", emPks.Select(pk => $"'{pk}'"))});";

				var resultList = new List<(Guid, string, Guid)>();

				using (var command = Db.Connection.Command(sql))
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						resultList.Add((reader.GetGuid(0), reader.GetString(1), reader.GetGuid(2)));
					}
				}
				return resultList;
			}

			void AssertEDIMessage(Guid emPk, Guid expectedLinkUniqueID, string expectedLinkTable, string testCase)
			{
				var ediMessage = ediMessages.FirstOrDefault(e => e.emPk == emPk);
				AssertEquals($"{testCase} (EM_LinkTable)", expectedLinkTable, ediMessage.linkedTable);
				AssertEquals($"{testCase} (EM_LinkUniqueID)", expectedLinkUniqueID, ediMessage.linkUniqueID);
			}
		});

		Guid bhPK1 = Guid.NewGuid();
		Guid bhPK3 = Guid.NewGuid();
		Guid bhPK4 = Guid.NewGuid();
		Guid bhPK5 = Guid.NewGuid();

		Guid bmPK2 = Guid.NewGuid();
		Guid bmPK6 = Guid.NewGuid();

		Guid emPK1 = Guid.NewGuid();
		Guid emPK2 = Guid.NewGuid();
		Guid emPK3 = Guid.NewGuid();
		Guid emPK4 = Guid.NewGuid();
		Guid emPK5 = Guid.NewGuid();
		Guid emPK6 = Guid.NewGuid();
	}
}
