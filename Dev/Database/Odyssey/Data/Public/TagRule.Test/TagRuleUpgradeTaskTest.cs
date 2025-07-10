using System;
using System.Data;
using System.Text;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.Testing
{
	sealed class TagRuleUpgradeTaskTest : TransactionedTestCase
	{
		[ExpectNoExceptions]
		public void TestRunUpgrade()
		{
			task.Run();

			var tempFile = new TagRuleDataFile();
			var data = tempFile.LoadDataFromDatabase();

			AssertEquals("Table Count", 6, data.Tables.Count);
		}

		public void TestEnsureBranchAndDepartmentAreNotNull()
		{
			var testTagRuleTable = new DataTable();
			testTagRuleTable.Columns.Add(TagRuleSchema.Constants.PK, typeof(Guid));
			testTagRuleTable.Columns.Add(TagRuleSchema.Constants.TGR_GE_Department, typeof(Guid));
			testTagRuleTable.Columns.Add(TagRuleSchema.Constants.TGR_GB_Branch, typeof(Guid));

			var testTagRuleRow = testTagRuleTable.NewRow();
			testTagRuleRow[TagRuleSchema.Constants.PK] = Guid.Empty;

			task.EnsureBranchAndDepartmentAreNotNull(testTagRuleRow);

			var expectedBranchPK = (Guid)Db.Connection.ExecuteScalar("SELECT TOP 1 GB_PK FROM dbo.GlbBranch");
			var expectedDepartmentPK = (Guid)Db.Connection.ExecuteScalar("SELECT GE_PK FROM dbo.GlbDepartment WHERE GE_Code = 'BRN'");

			AssertEquals("Method has replaced SourceRow Empty Branch with Top 1 Branch", expectedBranchPK, testTagRuleRow[TagRuleSchema.Constants.TGR_GB_Branch]);
			AssertEquals("Method has replaced SourceRow Empty Department with BRN Department", expectedDepartmentPK, testTagRuleRow[TagRuleSchema.Constants.TGR_GE_Department]);
		}

		public void TestLeavesNonSystemTagsAlone()
		{
			var definitionGuid = Guid.NewGuid();
			var magnitudeGuid = Guid.NewGuid();

			var insertSql = new StringBuilder();

			insertSql.AppendLine(BMDbTestHelper.GetTagDefinitionInsertSql(definitionGuid, "LOL"));
			insertSql.AppendLine(BMDbTestHelper.GetTagMagnitudeInsertSql(magnitudeGuid, "HAR", definitionGuid));

			Db.Connection.ExecuteNonQuery(insertSql.ToString());

			task.Run();

			var countDefinitionSQL = @"
				SELECT COUNT(*) FROM dbo.TagDefinition
				WHERE TGD_Code = 'LOL'";
			AssertEquals(1, (int)Db.Connection.ExecuteScalar(countDefinitionSQL));

			var countMagnitudeSQL = @"
				SELECT COUNT(*) FROM dbo.TagMagnitude
				WHERE TGM_Code = 'HAR'";
			AssertEquals(1, (int)Db.Connection.ExecuteScalar(countMagnitudeSQL));
		}

		public void TestIgnoresExcludedColumnsOnUpdate_TagMagnitude()
		{
			task.Run();

			var updateMagnitudeTagSql = @"
				UPDATE dbo.TagMagnitude
				SET TGM_NudgeAmount = 25, TGM_VisualizationData = CAST('<yabbadabbadoo />' AS XML), TGM_Description = 'boop';";

			Db.Connection.ExecuteNonQuery(updateMagnitudeTagSql);

			task.Run();

			var countMagnitudeUpdatedFieldSql = @"
				SELECT COUNT(*) FROM dbo.TagMagnitude
				WHERE TGM_Description = 'boop'";
			AssertEquals(0, (int)Db.Connection.ExecuteScalar(countMagnitudeUpdatedFieldSql));

			var countMagnitudeExcludedField = @"
				SELECT COUNT(*) FROM dbo.TagMagnitude
				WHERE TGM_NudgeAmount = 25 AND TGM_VisualizationData IS NOT NULL";
			AssertNotEquals(0, (int)Db.Connection.ExecuteScalar(countMagnitudeExcludedField));
		}

		public void TestIgnoresExcludedColumnsOnUpdate_TagRule()
		{
			task.Run();

			var countActiveTagRules = @"
				SELECT COUNT(*) FROM dbo.TagRule
				WHERE TGR_IsActive = 1";

			var countInActiveTagRules = @"
				SELECT COUNT(*) FROM dbo.TagRule
				WHERE TGR_IsActive = 0";

			AssertEquals(4, (int)Db.Connection.ExecuteScalar(countActiveTagRules));
			AssertEquals(0, (int)Db.Connection.ExecuteScalar(countInActiveTagRules));

			var updateMagnitudeTagSql = @"
				UPDATE dbo.TagRule
				SET TGR_IsActive = 0;";

			Db.Connection.ExecuteNonQuery(updateMagnitudeTagSql);

			task.Run();

			AssertEquals(0, (int)Db.Connection.ExecuteScalar(countActiveTagRules));
			AssertEquals(4, (int)Db.Connection.ExecuteScalar(countInActiveTagRules));
		}

		public void TestTagLinkWithoutTagRules_ShouldBeReplaced()
		{
			var tagRuleXML = new TagRuleDataFile();
			var tagLinkPKs = tagRuleXML.DataSet.Tables[TagLinkSchema.Constants.TableName]
				.AsEnumerable()
				.Select(row => new Guid(row[TagLinkSchema.Constants.PK].ToString()));

			var definitionGuid = Guid.NewGuid();
			var parentPK = Guid.NewGuid();
			var tagMagnitudePK = Guid.NewGuid();

			var insertSql = new StringBuilder();

			insertSql.AppendLine(BMDbTestHelper.GetTagDefinitionInsertSql(definitionGuid, "LOL"));
			insertSql.AppendLine(BMDbTestHelper.GetTagMagnitudeInsertSql(tagMagnitudePK, "HAR", definitionGuid));

			foreach (var linkPk in tagLinkPKs)
			{
				insertSql.AppendLine(BMDbTestHelper.GetTagLinkInsertSql(linkPk, tagMagnitudePK, parentPK, parentTableCode: "TGR", description: "REPLACE ME"));
			}

			var countTagRulesSql = "SELECT COUNT(*) FROM dbo.TagRule";
			var countTagLinksSql = "SELECT COUNT(*) FROM dbo.TagLink";

			AssertEquals("precondition - NO tagLinks", 0, (int)Db.Connection.ExecuteScalar(countTagLinksSql));

			Db.Connection.ExecuteNonQuery(insertSql.ToString());

			var tagRulesInXmlCount = tagRuleXML.DataSet.Tables[TagRuleSchema.Constants.TableName].Rows.Count;
			var tagLinksInXmlCount = tagRuleXML.DataSet.Tables[TagLinkSchema.Constants.TableName].Rows.Count;

			AssertEquals("precondition - There are 4 TagRules in the XML", 4, tagRulesInXmlCount);
			AssertEquals("precondition - There are 4 TagLinks in XML", 4, tagLinksInXmlCount);
			AssertEquals("precondition - No tagRules", 0, (int)Db.Connection.ExecuteScalar(countTagRulesSql));
			AssertEquals("precondition - TagLinks was inserted ", tagLinksInXmlCount, (int)Db.Connection.ExecuteScalar(countTagLinksSql));

			AssertNoExceptionThrown("Should run smoothly", () => new TagRuleUpgradeTask().Run());

			var countTagLinksSqlWhereNoDescriptionWithParentTagRule = countTagLinksSql + " WHERE TGL_Description = '' AND TGL_ParentId IN (SELECT TGR_PK FROM dbo.TagRule)";
			var countTagLinksSqlWhereDescriptionIsReplaceMe = countTagLinksSql + " WHERE TGL_Description = 'REPLACE ME'";

			AssertEquals("TagRules were inserted", tagRulesInXmlCount, (int)Db.Connection.ExecuteScalar(countTagRulesSql));
			AssertEquals("TagLinks were replaced, all have no description now!", tagLinksInXmlCount, (int)Db.Connection.ExecuteScalar(countTagLinksSqlWhereNoDescriptionWithParentTagRule));
			AssertEquals("TagLinks were replaced, there is no tagLinks with REPLACE ME description", 0, (int)Db.Connection.ExecuteScalar(countTagLinksSqlWhereDescriptionIsReplaceMe));
		}

		public void TestStmModuleFilterWithoutTagRules_ShouldBeReplaced()
		{
			var tagRuleXML = new TagRuleDataFile();
			var stmModuleFilterPKs = tagRuleXML.DataSet.Tables[StmModuleFilterSchema.Constants.TableName]
				.AsEnumerable()
				.Select(row => new Guid(row[StmModuleFilterSchema.Constants.PK].ToString()));

			var insertModuleFilterValuesSql = string.Join(Environment.NewLine, stmModuleFilterPKs
				.Select(stmModuleFilterPK => $"('{stmModuleFilterPK}', 'REPLACE ME', '{Guid.NewGuid()}', 'TGR', 'FRU'),"));
			insertModuleFilterValuesSql = insertModuleFilterValuesSql.Substring(0, insertModuleFilterValuesSql.Length - 1) + ";";

			var insertStmModuleFilterSql = $@"INSERT dbo.StmModuleFilter (
					S9_PK, S9_ModuleID, S9_ParentID, S9_ParentTableCode, S9_FilterType
				) VALUES {insertModuleFilterValuesSql}";

			var insertModuleFilterUserDataValuesSql = string.Join(Environment.NewLine, stmModuleFilterPKs
			.Select(stmModuleFilterPK => $"(NEWID(), '{Guid.Empty}', '{stmModuleFilterPK}'),"));
			insertModuleFilterUserDataValuesSql = insertModuleFilterUserDataValuesSql.Substring(0, insertModuleFilterUserDataValuesSql.Length - 1) + ";";

			var insertStmModuleFilterUserDataSql = $@"INSERT dbo.StmModuleFilterUserData (
					S0_PK, S0_RelatedEntityID, S0_S9
				) VALUES {insertModuleFilterUserDataValuesSql}";

			var countTagRulesSql = "SELECT COUNT(*) FROM dbo.TagRule";
			var countStmModuleFiltersSql = "SELECT COUNT(*) FROM dbo.StmModuleFilter";
			var countStmModuleFilterUserDataSql = "SELECT COUNT(*) FROM dbo.StmModuleFilterUserData";

			AssertEquals("precondition - NO StmModuleFilters", 0, (int)Db.Connection.ExecuteScalar(countStmModuleFiltersSql));
			AssertEquals("precondition - NO StmModuleFilterUserData", 0, (int)Db.Connection.ExecuteScalar(countStmModuleFilterUserDataSql));

			Db.Connection.ExecuteNonQuery(insertStmModuleFilterSql);
			Db.Connection.ExecuteNonQuery(insertStmModuleFilterUserDataSql);

			var tagRulesInXmlCount = tagRuleXML.DataSet.Tables[TagRuleSchema.Constants.TableName].Rows.Count;
			var stmModuleFilterInXmlCount = tagRuleXML.DataSet.Tables[StmModuleFilterSchema.Constants.TableName].Rows.Count;
			var stmModuleFilterUserDataXmlCount = tagRuleXML.DataSet.Tables[StmModuleFilterUserDataSchema.Constants.TableName].Rows.Count;

			AssertEquals("precondition - There are 4 TagRules in the XML", 4, tagRulesInXmlCount);
			AssertEquals("precondition - There are 4 StmModuleFilter in XML", 4, stmModuleFilterInXmlCount);
			AssertEquals("precondition - There are 4 StmModuleFilterUserData in XML", 4, stmModuleFilterUserDataXmlCount);
			AssertEquals("precondition - No tagRules", 0, (int)Db.Connection.ExecuteScalar(countTagRulesSql));
			AssertEquals("precondition - StmModuleFilters was inserted ", stmModuleFilterInXmlCount, (int)Db.Connection.ExecuteScalar(countStmModuleFiltersSql));
			AssertEquals("precondition - StmModuleFilterUserData was inserted ", stmModuleFilterUserDataXmlCount, (int)Db.Connection.ExecuteScalar(countStmModuleFilterUserDataSql));

			AssertNoExceptionThrown("Should run smoothly", () => new TagRuleUpgradeTask().Run());

			var countStmModuleFiltersSqlWhereWithParentTagRule = countStmModuleFiltersSql + " WHERE S9_ModuleID = 'BMFilterRule' AND S9_ParentID IN (SELECT TGR_PK FROM dbo.TagRule)";
			var countStmModuleFiltersSqlWhereModuleIDIsReplaceMe = countStmModuleFiltersSql + " WHERE S9_ModuleID = 'REPLACE ME'";

			AssertEquals("TagRules were inserted", tagRulesInXmlCount, (int)Db.Connection.ExecuteScalar(countTagRulesSql));
			AssertEquals("StmModuleFilters were replaced", stmModuleFilterInXmlCount, (int)Db.Connection.ExecuteScalar(countStmModuleFiltersSqlWhereWithParentTagRule));
			AssertEquals("StmModuleFilters were replaced, there is no StmModuleFilters with REPLACE ME S9_ModuleID", 0, (int)Db.Connection.ExecuteScalar(countStmModuleFiltersSqlWhereModuleIDIsReplaceMe));

			var countStmModuleFilterUserDataSqlWhereWithFilterData = countStmModuleFilterUserDataSql + " WHERE S0_FilterDataValues IS NOT NULL";
			var countStmModuleFilterUserDataSqlWhereWithoutFilterData = countStmModuleFilterUserDataSql + " WHERE S0_FilterDataValues IS NULL";

			AssertEquals("StmModuleFilterUserData were replaced", stmModuleFilterUserDataXmlCount, (int)Db.Connection.ExecuteScalar(countStmModuleFilterUserDataSqlWhereWithFilterData));
			AssertEquals("StmModuleFilterUserData were replaced, there is no StmModuleFilterUserData without FilterDataValues", 0, (int)Db.Connection.ExecuteScalar(countStmModuleFilterUserDataSqlWhereWithoutFilterData));
		}

		protected override void SetUp()
		{
			base.SetUp();

			DataHelpers.ClearTable(TagRuleSchema.Constants.TableName);
			DataHelpers.ClearTable(StmModuleFilterSchema.Constants.TableName);
			DataHelpers.ClearTable(TagLinkSchema.Constants.TableName);
			DataHelpers.ClearTable(TagMagnitudeSchema.Constants.TableName);
			DataHelpers.ClearTable(TagDefinitionSchema.Constants.TableName);

			task = new TagRuleUpgradeTask();
		}

		TagRuleUpgradeTask task;
	}
}
