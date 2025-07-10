using System;
using System.Collections.Generic;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.JP;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Customs.JP.Testing
{
	[TestedType(typeof(MigrateJobDeclarationFieldsToCusEntryInstruction))]
	class MigrateJobDeclarationFieldsToCusEntryInstructionTest : DataTransformationTestCase
	{
		Guid[] declarationList;

		public override void TestNewIndex()
		{
			new TransformationTestDataCreator().CreateGlbCompany("~JP", "JP");
			base.TestNewIndex();
		}

		public override string[] expectedIndex => new string[]
		{
			"NONCLUSTERED INDEX [_WTG__Migrate JobDeclaration Fields To CusEntryInstruction For JP_1] ON [dbo].[JobDeclaration] ([JE_DataModel]) WHERE ([JE_DataModel]='JP') WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)"
		};

		protected override void AssertTransformationResults()
		{
			var resultList = new List<Tuple<Guid, string, string, string>>();

			var sql = string.Format(@"
			SELECT
				CEI_JE, CEI_Style, CEI_SubStyle, CEI_AddInfo
			FROM
				dbo.CusEntryInstruction;");
			TestConnection.ExecuteReader(sql, reader => resultList.Add(Tuple.Create((Guid)reader["CEI_JE"], (string)reader["CEI_Style"], (string)reader["CEI_SubStyle"], (string)reader["CEI_AddInfo"])));

			AssertContainsExactElementsInAnyOrder(new[]
			{
					Tuple.Create(declarationList[0], "A", "B", "ExportUnionCode=REW*DeclarationCargoType=C"),
					Tuple.Create(declarationList[1], "A", "B", "ExportUnionCode=REW"),
					Tuple.Create(declarationList[2], "D", "E", "DeclarationCargoType=F"),
					Tuple.Create(declarationList[3], "D", "E", ""),
					Tuple.Create(declarationList[4], "", "", "ExportUnionCode=REW")
			}, resultList);
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new MigrateJobDeclarationFieldsToCusEntryInstruction();

		protected override void PrepareTestData()
		{
			declarationList = new Guid[6] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
			var helper = new TransformationTestDataCreator();
			var jpCompanyPK = helper.CreateGlbCompany("~JP", "JP");
			var jpBranchPK = helper.CreateGlbBranch("~JP", jpCompanyPK);
			var usCompanyPK = helper.CreateGlbCompany("~US", "US");
			var usBranchPK = helper.CreateGlbBranch("~US", usCompanyPK);
			var sqlText = $@"
INSERT INTO dbo.JobDeclaration (JE_PK, JE_GB, JE_GC, JE_ClusterKey, JE_DeclarationReference, JE_DataModel, JE_MessageSubType, JE_AddInfo, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_SystemLastEditTimeUtc, JE_SystemLastEditUser) VALUES
	('{declarationList[0]}', '{jpBranchPK}', '{jpCompanyPK}', 1, 'Ref1', 'JP', 'A', 'DeclarationSubType=B*CargoType=C', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	('{declarationList[1]}', '{jpBranchPK}', '{jpCompanyPK}', 2, 'Ref2', 'JP', 'A', 'DeclarationSubType=B'            , GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	('{declarationList[2]}', '{jpBranchPK}', '{jpCompanyPK}', 3, 'Ref3', 'JP', 'D', 'DeclarationSubType=E*CargoType=F', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	('{declarationList[3]}', '{jpBranchPK}', '{jpCompanyPK}', 4, 'Ref4', 'JP', 'D', 'DeclarationSubType=E'            , GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	('{declarationList[4]}', '{usBranchPK}', '{usCompanyPK}', 5, 'Ref5', 'US', 'A', 'DeclarationSubType=B*CargoType=C', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	('{declarationList[5]}', '{usBranchPK}', '{usCompanyPK}', 6, 'Ref6', 'US', 'D', 'DeclarationSubType=E*CargoType=F', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
INSERT dbo.CusEntryInstruction (CEI_PK,CEI_DataModel,CEI_ClusterKey,CEI_JE,CEI_AddInfo,CEI_SystemCreateTimeUtc,CEI_SystemCreateUser,CEI_SystemLastEditTimeUtc,CEI_SystemLastEditUser) VALUES
	(NEWID(),'JP',1,'{declarationList[0]}','ExportUnionCode=REW',GetUtcDate(),'~BP',GetUtcDate(),'~BP'),
	(NEWID(),'JP',2,'{declarationList[1]}','ExportUnionCode=REW',GetUtcDate(),'~BP',GetUtcDate(),'~BP'),
	(NEWID(),'US',5,'{declarationList[4]}','ExportUnionCode=REW',GetUtcDate(),'~BP',GetUtcDate(),'~BP');
";
			Db.Connection.ExecuteNonQuery(sqlText);
		}
	}
}
