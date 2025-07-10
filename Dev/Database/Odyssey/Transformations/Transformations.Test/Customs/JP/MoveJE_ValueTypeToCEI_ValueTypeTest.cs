using System;
using System.Collections.Generic;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.JP;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Customs.JP.Testing
{
	[TestedType(typeof(MoveJE_ValueTypeToCEI_ValueType))]
	class MoveJE_ValueTypeToCEI_ValueTypeTest : DataTransformationTestCase
	{
		Guid[] declarationList;

		public override void TestNewIndex()
		{
			new TransformationTestDataCreator().CreateGlbCompany("~JP", "JP");
			base.TestNewIndex();
		}

		public override string[] expectedIndex =>
		[
			"NONCLUSTERED INDEX [_WTG__Move JE_ValueType To CEI_ValueType_1] ON [dbo].[JobDeclaration] ([JE_DataModel]) WHERE ([JE_DataModel]='JP') WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)"
		];

		protected override void AssertTransformationResults()
		{
			var resultList = new List<Tuple<Guid, string, string>>();

			var sql = string.Format(@"
			SELECT
				CEI_JE, CEI_AddInfo, CEI_DataModel
			FROM
				dbo.CusEntryInstruction;");
			TestConnection.ExecuteReader(sql, reader => resultList.Add(Tuple.Create((Guid)reader["CEI_JE"], (string)reader["CEI_AddInfo"], (string)reader["CEI_DataModel"])));

			AssertContainsExactElementsInAnyOrder(
			[
					Tuple.Create(declarationList[0], "ExportUnionCode=REW*ValueType=A", "JP"),
					Tuple.Create(declarationList[0], "ExportUnionCode=REW", "US"),
					Tuple.Create(declarationList[1], "ValueType=B", "JP"),
			], resultList);
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new MoveJE_ValueTypeToCEI_ValueType();

		protected override void PrepareTestData()
		{
			declarationList = [Guid.NewGuid(), Guid.NewGuid()];
			var helper = new TransformationTestDataCreator();
			var jpCompanyPK = helper.CreateGlbCompany("~JP", "JP");
			var jpBranchPK = helper.CreateGlbBranch("~JP", jpCompanyPK);
			var usCompanyPK = helper.CreateGlbCompany("~US", "US");
			var usBranchPK = helper.CreateGlbBranch("~US", usCompanyPK);
			var sqlText = $@"
INSERT INTO dbo.JobDeclaration (JE_PK, JE_GB, JE_GC, JE_ClusterKey, JE_DeclarationReference, JE_DataModel, JE_MessageSubType, JE_AddInfo, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_SystemLastEditTimeUtc, JE_SystemLastEditUser) VALUES
	('{declarationList[0]}', '{jpBranchPK}', '{jpCompanyPK}', 1, 'Ref1', 'JP', 'A', 'ValueType=A', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	('{declarationList[1]}', '{jpBranchPK}', '{jpCompanyPK}', 2, 'Ref2', 'JP', 'A', 'ValueType=B', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
INSERT dbo.CusEntryInstruction (CEI_PK,CEI_DataModel,CEI_ClusterKey,CEI_JE,CEI_AddInfo,CEI_SystemCreateTimeUtc,CEI_SystemCreateUser,CEI_SystemLastEditTimeUtc,CEI_SystemLastEditUser) VALUES
	(NEWID(),'JP',1,'{declarationList[0]}','ExportUnionCode=REW',GetUtcDate(),'~BP',GetUtcDate(),'~BP'),
	(NEWID(),'US',1,'{declarationList[0]}','ExportUnionCode=REW',GetUtcDate(),'~BP',GetUtcDate(),'~BP');
";
			Db.Connection.ExecuteNonQuery(sqlText);
		}
	}
}
