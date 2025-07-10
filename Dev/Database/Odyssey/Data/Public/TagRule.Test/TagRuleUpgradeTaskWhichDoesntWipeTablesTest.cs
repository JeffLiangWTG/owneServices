using System;
using System.Text;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.Testing
{
	sealed class TagRuleUpgradeTaskWhichDoesntWipeTablesTest : TransactionedTestCase
	{
		public void TestLeavesWorkQueueTagsAlone()
		{
			var definitionGuid = BMDbTestHelper.GetWorkQueueTagGroupPK(Db.Connection);
			var magnitudeGuid = Guid.NewGuid();

			Db.Connection.ExecuteNonQuery(BMDbTestHelper.GetTagMagnitudeInsertSql(magnitudeGuid, "HAR", definitionGuid));

			new TagRuleUpgradeTask().Run();

			AssertEquals(definitionGuid, (Guid)Db.Connection.ExecuteScalar("SELECT TGD_PK FROM dbo.TagDefinition WHERE TGD_Code = 'QUE'"));
			AssertEquals(magnitudeGuid, (Guid)Db.Connection.ExecuteScalar("SELECT TGM_PK FROM dbo.TagMagnitude WHERE TGM_Code = 'HAR'"));
		}

		public void TestLeavesWorkQueueTagRulesAlone()
		{
			var definitionGuid = BMDbTestHelper.GetWorkQueueTagGroupPK(Db.Connection);
			var magnitudeGuid = Guid.NewGuid();
			var tagLinkGuid = Guid.NewGuid();
			var filterGuid = Guid.NewGuid();
			var ruleGuid = Guid.NewGuid();
			var branchGuid = DataHelpers.GetFirstKey(GlbBranchSchema.Instance);
			var departmentGuid = DataHelpers.GetFirstKey(GlbDepartmentSchema.Instance);

			var insertSql = new StringBuilder();
			insertSql.AppendLine(BMDbTestHelper.GetTagMagnitudeInsertSql(magnitudeGuid, "HAR", definitionGuid));
			insertSql.AppendLine(BMDbTestHelper.GetTagLinkInsertSql(tagLinkGuid, magnitudeGuid, ruleGuid, parentTableCode: "TGR"));
			insertSql.AppendLine(BMDbTestHelper.GetStmModuleFilterRuleInsertSql(filterGuid, ruleGuid, "TGR"));
			insertSql.AppendLine(string.Format(BMDbTestHelper.StmModuleFilterUserDataInsertSql, Guid.NewGuid(), filterGuid, Guid.NewGuid(), string.Empty, "0x1234"));
			insertSql.AppendLine(BMDbTestHelper.GetTagRuleInsertSql(ruleGuid, "Rule", "MAG", branchGuid, departmentGuid));
			Db.Connection.ExecuteNonQuery(insertSql.ToString());

			new TagRuleUpgradeTask().Run();

			AssertEquals(definitionGuid, (Guid)Db.Connection.ExecuteScalar("SELECT TGD_PK FROM dbo.TagDefinition WHERE TGD_Code = 'QUE'"));
			AssertEquals(magnitudeGuid, (Guid)Db.Connection.ExecuteScalar("SELECT TGM_PK FROM dbo.TagMagnitude WHERE TGM_Code = 'HAR'"));

			AssertEquals(ruleGuid, (Guid)Db.Connection.ExecuteScalar(string.Format(@"
				SELECT TGR_PK FROM dbo.TagRule
				JOIN dbo.TagLink on TGR_PK = TGL_ParentId AND TGL_ParentTableCode = 'TGR'
				WHERE TGL_TGM_Magnitude = '{0}'", magnitudeGuid)));
		}
	}
}
