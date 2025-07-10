using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.BufferManagement.Module;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Scheduler.Business;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.BufferManagement.GUI.Test
{
	class TagRuleRunnerTest : BMSTestCaseWithFactory
	{
		#region FilterStrips

		public void TestMaintainMagnitudeTagRule_WhenStmModuleFilterIsNotPublished_ShouldStillWork()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "ABC");
			var tag = BMSTestHelper.CreateTagMagnitude(tagGroup, "BTH");
			var tagRule = BMSTestHelper.CreateTagRule(tag, "Froopy Land", TagRuleActionTypeList.Codes.MaintainMagnitude, 69m);

			FilterStripsTestHelper.AddFilterStrip<ModuleTextFilter>(tagRule.Filter, ProcessHeader.ModuleFilterConstants.CompletionStatement, f => f.Property = "Pink");

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Pink Sentient Switchblade");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Invisibility Cuffs");

			workflow1.AddTag(tag);
			workflow2.AddTag(tag);

			Factory.Save();

			AssertTagApplied(workflow1, tag, magnitude: 1m);
			AssertTagApplied(workflow2, tag, magnitude: 1m);

			RunTagRulesRegardlessOfLastRunTimeConsiderations(tagRule);

			AssertTagApplied(workflow1, tag, reloadInNewFactory: true, magnitude: 69m);
			AssertTagApplied(workflow2, tag, reloadInNewFactory: true, magnitude: 1m);

			TestConnection.ExecuteNonQuery(FormattableString.Invariant($@"
UPDATE dbo.StmModuleFilter
SET
	S9_IsPublished = 0,
	S9_RelatedEntityID = NEWID()
WHERE S9_PK = '{tagRule.Filter.PK}'"));
			tagRule = tagRule.Factory.CreateNewFactory().Load<TagRule>(tagRule.PK);

			RunTagRulesRegardlessOfLastRunTimeConsiderations(tagRule);

			AssertTagApplied(workflow1, tag, reloadInNewFactory: true, magnitude: 69m);
			AssertTagApplied(workflow2, tag, reloadInNewFactory: true, magnitude: 1m);
		}

		public void TestCustomSqlFilterStrip_Add()
		{
			var tagDefinition = BMSTestHelper.CreateTagDefinition(Factory, "AAA");
			tagDefinition.TGD_UsageScope = TagUsageScopeList.Codes.Rule;
			var tagMagnitude = BMSTestHelper.CreateTagMagnitude(tagDefinition, "AAA");

			var rule = BMSTestHelper.CreateTagRule(tagMagnitude, "Add CC1", TagRuleActionTypeList.Codes.AddTag);
			var stmFilter = rule.Filter;
			FilterStripsTestHelper.AddFilterStrips(stmFilter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Custom SQL Filter",
				FilterStripValueSetter = f => ((ModuleSQLFilter)f).Property1 = "FH_CompletionStatement = 'peekaboo'",
			});

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			workflow.FH_CompletionStatement = "peekaboo";

			Factory.Save();

			RunTagRules(rule);

			workflow = new BusinessObjectFactory().Load<ProcessHeader>(workflow.PK);
			AssertTagApplied("custom sql filter strip should have applied the tag", workflow, tagMagnitude);
		}

		public void TestCustomSqlFilterStrip_Add_ShouldNotRunPerformanceVerification_IfPerformanceCheckDisabled()
		{
			BMSRegistry.Instance.TagRulePerformanceCheckFrequency.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);

			var tagDefinition = BMSTestHelper.CreateTagDefinition(Factory, "AAA");
			tagDefinition.TGD_UsageScope = TagUsageScopeList.Codes.Rule;
			var tagMagnitude = BMSTestHelper.CreateTagMagnitude(tagDefinition, "AAA");

			var rule = BMSTestHelper.CreateTagRule(tagMagnitude, "Add CC1", TagRuleActionTypeList.Codes.AddTag);
			var stmFilter = rule.Filter;
			FilterStripsTestHelper.AddFilterStrips(stmFilter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Custom SQL Filter",
				FilterStripValueSetter = f => ((ModuleSQLFilter)f).Property1 = "FH_CompletionStatement = 'peekaboo'",
			});

			AssertEquals(ZDateTime.Empty, rule.TGR_LastPerformanceVerificationDateTimeUtc);

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			workflow.FH_CompletionStatement = "peekaboo";

			Factory.Save();

			RunTagRules(rule);

			AssertEquals("Verification should not have run", ZDateTime.Empty, rule.TGR_LastPerformanceVerificationDateTimeUtc);
		}

		public void TestCustomSqlFilterStrip_Add_ShouldRunPerformanceVerification_IfDateEmpty()
		{
			var tagDefinition = BMSTestHelper.CreateTagDefinition(Factory, "AAA");
			tagDefinition.TGD_UsageScope = TagUsageScopeList.Codes.Rule;
			var tagMagnitude = BMSTestHelper.CreateTagMagnitude(tagDefinition, "AAA");

			var rule = BMSTestHelper.CreateTagRule(tagMagnitude, "Add CC1", TagRuleActionTypeList.Codes.AddTag);
			var stmFilter = rule.Filter;
			FilterStripsTestHelper.AddFilterStrips(stmFilter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Custom SQL Filter",
				FilterStripValueSetter = f => ((ModuleSQLFilter)f).Property1 = "FH_CompletionStatement = 'peekaboo'",
			});

			AssertEquals(ZDateTime.Empty, rule.TGR_LastPerformanceVerificationDateTimeUtc);

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			workflow.FH_CompletionStatement = "peekaboo";

			Factory.Save();

			RunTagRules(rule);

			AssertNotEquals("Verification should have run", ZDateTime.Empty, rule.TGR_LastPerformanceVerificationDateTimeUtc);

			workflow = new BusinessObjectFactory().Load<ProcessHeader>(workflow.PK);
			AssertTagApplied("custom sql filter strip should have applied the tag", workflow, tagMagnitude);
		}

		[TestDate(2019, 11, 1)]
		public void TestCustomSqlFilterStrip_Add_ShouldRunPerformanceVerification_IfVerificationDateElapsed()
		{
			BMSRegistry.Instance.TagRulePerformanceCheckFrequency.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 2);

			var tagDefinition = BMSTestHelper.CreateTagDefinition(Factory, "AAA");
			tagDefinition.TGD_UsageScope = TagUsageScopeList.Codes.Rule;
			var tagMagnitude = BMSTestHelper.CreateTagMagnitude(tagDefinition, "AAA");

			var testDate = ZDateTime.UtcNow.AddDays(-1);

			var rule = BMSTestHelper.CreateTagRule(tagMagnitude, "Add CC1", TagRuleActionTypeList.Codes.AddTag);
			var stmFilter = rule.Filter;
			FilterStripsTestHelper.AddFilterStrips(stmFilter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Custom SQL Filter",
				FilterStripValueSetter = f => ((ModuleSQLFilter)f).Property1 = "FH_CompletionStatement = 'peekaboo'",
			});
			rule.TGR_LastPerformanceVerificationDateTimeUtc = testDate;

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			workflow.FH_CompletionStatement = "peekaboo";

			Factory.Save();

			RunTagRules(rule);

			AssertEquals("Verification should not have run", testDate, rule.TGR_LastPerformanceVerificationDateTimeUtc);

			workflow = new BusinessObjectFactory().Load<ProcessHeader>(workflow.PK);
			AssertTagApplied("custom sql filter strip should have applied the tag", workflow, tagMagnitude);

			testDate = ZDateTime.UtcNow.AddDays(-3);
			rule.TGR_LastPerformanceVerificationDateTimeUtc = testDate;

			RunTagRules(rule);
			AssertNotEquals("Verification should have run", testDate, rule.TGR_LastPerformanceVerificationDateTimeUtc);

			testDate = rule.TGR_LastPerformanceVerificationDateTimeUtc;

			workflow = new BusinessObjectFactory().Load<ProcessHeader>(workflow.PK);
			AssertTagApplied("custom sql filter strip should have applied the tag", workflow, tagMagnitude);

			RunTagRules(rule);
			AssertEquals("Verification should NOT have run", testDate, rule.TGR_LastPerformanceVerificationDateTimeUtc);
		}

		[TestDate(2019, 11, 1)]
		public void TestCustomSqlFilterStrip_Add_ShouldRunPerformanceVerification_AndChangeMaxdop()
		{
			BMSRegistry.Instance.TagRulePerformanceCheckFrequency.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 2);

			var tagDefinition = BMSTestHelper.CreateTagDefinition(Factory, "AAA");
			tagDefinition.TGD_UsageScope = TagUsageScopeList.Codes.Rule;
			var tagMagnitude = BMSTestHelper.CreateTagMagnitude(tagDefinition, "AAA");

			var rule = BMSTestHelper.CreateTagRule(tagMagnitude, "Add CC1", TagRuleActionTypeList.Codes.AddTag);
			var stmFilter = rule.Filter;
			FilterStripsTestHelper.AddFilterStrips(stmFilter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Custom SQL Filter",
				FilterStripValueSetter = f => ((ModuleSQLFilter)f).Property1 = "FH_CompletionStatement = 'peekaboo'",
			});

			rule.TGR_LastPerformanceVerificationDateTimeUtc = ZDateTime.UtcNow.AddDays(-3);

			AssertEquals(false, rule.TGR_RunAddQuerySingleThreaded);
			AssertEquals(false, rule.TGR_RunMagnitudeQuerySingleThreaded);
			AssertEquals(false, rule.TGR_RunRemoveQuerySingleThreaded);

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			workflow.FH_CompletionStatement = "peekaboo";

			rule.FirstRunTimeForTest = 9999.0;
			Factory.Save();

			var connectionProvider = SecondaryServerConnectionProviderProvider.GetProvider();

			var logger = new TestServiceLogger();
			var dummyRunner = new DummyTagRuleRunner(logger, rule);
			dummyRunner.Strategy = new AddTagRuleRunSqlStrategyForTest(connectionProvider, logger);
			using (Db.Connection.TrackExecutedCommands())
			{
				dummyRunner.Process();

				AssertNotEquals("Verification should have run", ZDateTime.UtcNow.AddDays(-3), rule.TGR_LastPerformanceVerificationDateTimeUtc);

				AssertEquals(true, rule.TGR_RunAddQuerySingleThreaded);
				AssertEquals(false, rule.TGR_RunMagnitudeQuerySingleThreaded);
				AssertEquals(false, rule.TGR_RunRemoveQuerySingleThreaded);

				workflow = new BusinessObjectFactory().Load<ProcessHeader>(workflow.PK);
				AssertTagApplied("custom sql filter strip should have applied the tag", workflow, tagMagnitude);

				AssertContains("Information|Verified performance of rule [Add CC1], MAXDOP set to 1 (single).", logger.ToString());
				AssertEquals("MAXDOP of 1 should be present in the tag rule query", 1, Db.Connection.ExecutedCommands.Count(p => p.Contains("OPTION (MAXDOP 1)")));
			}

			BMSRegistry.Instance.ThreadedQuerySlownessThresholdFactor.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1.0m);
			rule.TGR_LastPerformanceVerificationDateTimeUtc = ZDateTime.Empty;
			Factory.Save();

			using (Db.Connection.TrackExecutedCommands())
			{
				dummyRunner.Process();

				AssertContains("Information|Verified performance of rule [Add CC1], MAXDOP disabled (parallel).", logger.ToString());
				AssertEquals("MAXDOP should not be present in the tag rule query", 0, Db.Connection.ExecutedCommands.Count(p => p.Contains("OPTION (MAXDOP 0)")));
				AssertEquals(false, rule.TGR_RunAddQuerySingleThreaded);
				AssertEquals(false, rule.TGR_RunMagnitudeQuerySingleThreaded);
				AssertEquals(false, rule.TGR_RunRemoveQuerySingleThreaded);
			}

			rule.TGR_LastPerformanceVerificationDateTimeUtc = ZDateTime.Empty;
			rule.FirstRunTimeForTest = 0.0;
			rule.Schedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddMinutes(-1);
			Factory.Save();

			using (Db.Connection.TrackExecutedCommands())
			{
				dummyRunner.Process();
				AssertContains("Information|Verified performance of rule [Add CC1], MAXDOP unchanged.", logger.ToString());
				AssertEquals("MAXDOP should not be present in the tag rule query", 0, Db.Connection.ExecutedCommands.Count(p => p.Contains("OPTION (MAXDOP 0)")));
			}
		}

		[TestDate(2019, 11, 1)]
		public void TestCustomSqlFilterStrip_Add_ShouldDropTempTable()
		{
			BMSRegistry.Instance.TagRulePerformanceCheckFrequency.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 2);

			var tagDefinition = BMSTestHelper.CreateTagDefinition(Factory, "AAA");
			tagDefinition.TGD_UsageScope = TagUsageScopeList.Codes.Rule;
			var tagMagnitude = BMSTestHelper.CreateTagMagnitude(tagDefinition, "AAA");

			var rule = BMSTestHelper.CreateTagRule(tagMagnitude, "Add CC1", TagRuleActionTypeList.Codes.AddTag);
			var stmFilter = rule.Filter;
			FilterStripsTestHelper.AddFilterStrips(stmFilter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Custom SQL Filter",
				FilterStripValueSetter = f => ((ModuleSQLFilter)f).Property1 = "FH_CompletionStatement = 'something'",
			});

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			workflow.FH_CompletionStatement = "something";
			BMSTestHelper.SetupEmailAndGroup(Factory);

			var connectionProvider = SecondaryServerConnectionProviderProvider.GetProvider();

			var logger = new TestServiceLogger();
			var tempTableName = string.Format(System.Globalization.CultureInfo.InvariantCulture, "#ProcessHeadersToHaveExclusiveTagsRemoved");
			var result = Db.Connection.ExecuteScalar(string.Format("SELECT TOP 1 * INTO {0} FROM dbo.ProcessHeader", tempTableName)); // This is a unit test...

			rule.Schedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddMinutes(-1);
			var dummyRunner = new DummyTagRuleRunnerThatIgnoresValidation(logger, rule);
			dummyRunner.Strategy = new AddTagRuleRunSqlStrategyForTest(connectionProvider, logger);
			dummyRunner.Process();

			var sql = string.Format("SELECT object_id(N'tempdb..{0}')", tempTableName);
			result = Db.Connection.ExecuteScalar(sql); // This is a unit test...
			AssertNotContains("Error|Error occurred while processing rule [Add CC1]. Rule is now deactivated.", logger.ToString());
			AssertEquals(DBNull.Value, result);
		}

		[TestDate(2019, 11, 1)]
		public void TestMaintainTablesAndQueriesReused()
		{
			TempTableandQueryReused(TagRuleActionTypeList.Codes.MaintainMagnitude);
		}

		[TestDate(2019, 11, 1)]
		public void TestAddTagTablesAndQueriesReused()
		{
			TempTableandQueryReused(TagRuleActionTypeList.Codes.AddTag);
		}

		[TestDate(2019, 11, 1)]
		public void TestRemoveTagTablesAndQueriesReused()
		{
			TempTableandQueryReused(TagRuleActionTypeList.Codes.RemoveTag);
		}

		void TempTableandQueryReused(string actionType)
		{
			BMSRegistry.Instance.TagRulePerformanceCheckFrequency.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 2);

			var uniqueCode = actionType.Substring(0, 3);

			var tagDefinition = BMSTestHelper.CreateTagDefinition(Factory, uniqueCode);
			tagDefinition.TGD_UsageScope = TagUsageScopeList.Codes.Rule;
			var tagMagnitude = BMSTestHelper.CreateTagMagnitude(tagDefinition, uniqueCode);

			var rule = BMSTestHelper.CreateTagRule(tagMagnitude, uniqueCode, actionType);

			rule.TGR_LastPerformanceVerificationDateTimeUtc = ZDateTime.UtcNow.AddDays(-3);

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			workflow.FH_CompletionStatement = "peekaboo";

			rule.FirstRunTimeForTest = 9999.0;
			Factory.Save();

			var logger = new TestServiceLogger();
			var dummyRunner = new DummyTagRuleRunner(logger, rule);

			IEnumerable<string> trackerOutput;
			IEnumerable<string> trackerOutput2;

			using (var tracker = Db.Connection.TrackExecutedCommands())
			{
				dummyRunner.Process();
				trackerOutput = Db.Connection.ExecutedCommands;
			}

			var priorDate = TestDateAttribute.Date;
			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(5);
			var afterDate = TestDateAttribute.Date;
			var workflow2 = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			workflow2.FH_CompletionStatement = "peekaboo123";
			Factory.Save();

			using (var tracker2 = Db.Connection.TrackExecutedCommands())
			{
				dummyRunner.Process();
				trackerOutput2 = Db.Connection.ExecutedCommands;
			}

			var filteredOutput = trackerOutput.Select((element) =>
			{
				var stringPriorDate = priorDate.ToString(@"MM\/dd\/yyyy HH:mm:ss");
				var stringAfterDate = afterDate.ToString(@"MM\/dd\/yyyy HH:mm:ss");
				if (element.Contains(stringPriorDate))
				{
					element = element.Replace(stringPriorDate, stringAfterDate);
				}
				return element;
			});

			Assert("Rule must be processed", !trackerOutput.IsNullOrEmpty() && !trackerOutput2.IsNullOrEmpty());
			CombineAssertions(actionType + " tag query not reused, instead was:  ", () =>
			{
				foreach (var element in trackerOutput2)
				{
					if (element.Contains("Query Template:"))
					{
						Assert(element, filteredOutput.Contains(element));
					}
				}
			});
		}

		public void TestCustomSqlFilterStrip_TwoBatches_Add()
		{
			BMSRegistry.Instance.TagRuleRunnerPrimarySecondaryServerTransferBatchSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);
			var tagDefinition = BMSTestHelper.CreateTagDefinition(Factory, "AAA");
			tagDefinition.TGD_UsageScope = TagUsageScopeList.Codes.Rule;
			var tagMagnitude = BMSTestHelper.CreateTagMagnitude(tagDefinition, "AAA");

			var rule = BMSTestHelper.CreateTagRule(tagMagnitude, "Add CC1", TagRuleActionTypeList.Codes.AddTag);
			FilterStripsTestHelper.AddFilterStrips(rule.Filter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Custom SQL Filter",
				FilterStripValueSetter = f => ((ModuleSQLFilter)f).Property1 = "FH_CompletionStatement like 'peekaboo%'",
			});

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			workflow.FH_CompletionStatement = "peekaboo1";

			var workflow2 = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			workflow2.FH_CompletionStatement = "peekaboo1";

			Factory.Save();

			RunTagRules(rule);

			workflow = new BusinessObjectFactory().Load<ProcessHeader>(workflow.PK);
			workflow2 = new BusinessObjectFactory().Load<ProcessHeader>(workflow2.PK);
			AssertTagApplied("custom sql filter strip should have applied the tag", workflow, tagMagnitude);
			AssertTagApplied("custom sql filter strip should have applied the tag", workflow2, tagMagnitude);
		}

		public void TestCustomSqlFilterStrip_TwoBatches_Remove()
		{
			BMSRegistry.Instance.TagRuleRunnerPrimarySecondaryServerTransferBatchSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);
			var tagDefinition = BMSTestHelper.CreateTagDefinition(Factory, "AAA");
			tagDefinition.TGD_UsageScope = TagUsageScopeList.Codes.All;
			var tagMagnitude = BMSTestHelper.CreateTagMagnitude(tagDefinition, "AAA");

			var rule = BMSTestHelper.CreateTagRule(tagMagnitude, "Add CC1", TagRuleActionTypeList.Codes.RemoveTag);
			FilterStripsTestHelper.AddFilterStrips(rule.Filter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Custom SQL Filter",
				FilterStripValueSetter = f => ((ModuleSQLFilter)f).Property1 = "FH_CompletionStatement like 'peekaboo%'",
			});

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			workflow.FH_CompletionStatement = "peekaboo1";
			workflow.AddTag(tagMagnitude);

			var workflow2 = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			workflow2.FH_CompletionStatement = "peekaboo1";
			workflow2.AddTag(tagMagnitude);

			AssertTagApplied("custom sql filter strip should have applied the tag", workflow, tagMagnitude);
			AssertTagApplied("custom sql filter strip should have applied the tag", workflow2, tagMagnitude);

			Factory.Save();

			RunTagRules(rule);

			workflow = new BusinessObjectFactory().Load<ProcessHeader>(workflow.PK);
			workflow2 = new BusinessObjectFactory().Load<ProcessHeader>(workflow2.PK);
			AssertTagNotApplied("custom sql filter strip shouldnt have applied the tag", workflow, tagMagnitude);
			AssertTagNotApplied("custom sql filter strip shouldnt have applied the tag", workflow2, tagMagnitude);
		}

		public void TestCustomSqlFilterStrip_Remove()
		{
			var tagDefinition = BMSTestHelper.CreateTagDefinition(Factory, "AAA");
			tagDefinition.TGD_UsageScope = TagUsageScopeList.Codes.All;
			var tagMagnitude = BMSTestHelper.CreateTagMagnitude(tagDefinition, "AAA");

			var rule = BMSTestHelper.CreateTagRule(tagMagnitude, "Add CC1", TagRuleActionTypeList.Codes.RemoveTag);
			var stmFilter = rule.Filter;
			FilterStripsTestHelper.AddFilterStrips(stmFilter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Custom SQL Filter",
				FilterStripValueSetter = f => ((ModuleSQLFilter)f).Property1 = "FH_CompletionStatement = 'peekaboo'",
			});

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			workflow.FH_CompletionStatement = "peekaboo";
			workflow.AddTag(tagMagnitude);

			Factory.Save();

			AssertTagApplied("tag applied before removal", workflow, tagMagnitude);

			RunTagRules(rule);

			workflow = new BusinessObjectFactory().Load<ProcessHeader>(workflow.PK);
			AssertTagNotApplied("custom sql filter strip should have removed the tag", workflow, tagMagnitude);
		}

		public void TestCustomSqlFilterStrip_Remove_ShouldNotRunPerformanceVerification_IfPerformanceCheckDisabled()
		{
			BMSRegistry.Instance.TagRulePerformanceCheckFrequency.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);

			var tagDefinition = BMSTestHelper.CreateTagDefinition(Factory, "AAA");
			tagDefinition.TGD_UsageScope = TagUsageScopeList.Codes.All;
			var tagMagnitude = BMSTestHelper.CreateTagMagnitude(tagDefinition, "AAA");

			var rule = BMSTestHelper.CreateTagRule(tagMagnitude, "Add CC1", TagRuleActionTypeList.Codes.RemoveTag);
			var stmFilter = rule.Filter;
			FilterStripsTestHelper.AddFilterStrips(stmFilter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Custom SQL Filter",
				FilterStripValueSetter = f => ((ModuleSQLFilter)f).Property1 = "FH_CompletionStatement = 'peekaboo'",
			});

			AssertEquals(ZDateTime.Empty, rule.TGR_LastPerformanceVerificationDateTimeUtc);

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			workflow.FH_CompletionStatement = "peekaboo";
			workflow.AddTag(tagMagnitude);

			Factory.Save();

			AssertTagApplied("tag applied before removal", workflow, tagMagnitude);

			RunTagRules(rule);

			AssertEquals("Verification should NOT have run", ZDateTime.Empty, rule.TGR_LastPerformanceVerificationDateTimeUtc);
		}

		public void TestCustomSqlFilterStrip_Remove_ShouldRunPerformanceVerification_IfDateEmpty()
		{
			var tagDefinition = BMSTestHelper.CreateTagDefinition(Factory, "AAA");
			tagDefinition.TGD_UsageScope = TagUsageScopeList.Codes.All;
			var tagMagnitude = BMSTestHelper.CreateTagMagnitude(tagDefinition, "AAA");

			var rule = BMSTestHelper.CreateTagRule(tagMagnitude, "Add CC1", TagRuleActionTypeList.Codes.RemoveTag);
			var stmFilter = rule.Filter;
			FilterStripsTestHelper.AddFilterStrips(stmFilter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Custom SQL Filter",
				FilterStripValueSetter = f => ((ModuleSQLFilter)f).Property1 = "FH_CompletionStatement = 'peekaboo'",
			});

			AssertEquals(ZDateTime.Empty, rule.TGR_LastPerformanceVerificationDateTimeUtc);

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			workflow.FH_CompletionStatement = "peekaboo";
			workflow.AddTag(tagMagnitude);

			Factory.Save();

			AssertTagApplied("tag applied before removal", workflow, tagMagnitude);

			RunTagRules(rule);

			AssertNotEquals("Verification should have run", ZDateTime.Empty, rule.TGR_LastPerformanceVerificationDateTimeUtc);

			workflow = new BusinessObjectFactory().Load<ProcessHeader>(workflow.PK);
			AssertTagNotApplied("custom sql filter strip should have removed the tag", workflow, tagMagnitude);
		}

		[TestDate(2019, 11, 1)]
		public void TestCustomSqlFilterStrip_Remove_ShouldRunPerformanceVerification_IfVerificationDateElapsed()
		{
			BMSRegistry.Instance.TagRulePerformanceCheckFrequency.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 2);

			var tagDefinition = BMSTestHelper.CreateTagDefinition(Factory, "AAA");
			tagDefinition.TGD_UsageScope = TagUsageScopeList.Codes.All;
			var tagMagnitude = BMSTestHelper.CreateTagMagnitude(tagDefinition, "AAA");

			var testDate = ZDateTime.UtcNow.AddDays(-1);
			var rule = BMSTestHelper.CreateTagRule(tagMagnitude, "Add CC1", TagRuleActionTypeList.Codes.RemoveTag);
			var stmFilter = rule.Filter;
			FilterStripsTestHelper.AddFilterStrips(stmFilter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Custom SQL Filter",
				FilterStripValueSetter = f => ((ModuleSQLFilter)f).Property1 = "FH_CompletionStatement = 'peekaboo'",
			});

			rule.TGR_LastPerformanceVerificationDateTimeUtc = testDate;

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			workflow.FH_CompletionStatement = "peekaboo";
			workflow.AddTag(tagMagnitude);

			Factory.Save();

			AssertTagApplied("tag applied before removal", workflow, tagMagnitude);

			RunTagRules(rule);

			AssertEquals("Verification should not have run", testDate, rule.TGR_LastPerformanceVerificationDateTimeUtc);
			workflow = new BusinessObjectFactory().Load<ProcessHeader>(workflow.PK);
			AssertTagNotApplied("custom sql filter strip should have removed the tag", workflow, tagMagnitude);

			testDate = ZDateTime.UtcNow.AddDays(-3);
			rule.TGR_LastPerformanceVerificationDateTimeUtc = testDate;

			RunTagRules(rule);
			AssertNotEquals("Verification should have run", testDate, rule.TGR_LastPerformanceVerificationDateTimeUtc);

			testDate = rule.TGR_LastPerformanceVerificationDateTimeUtc;

			workflow = new BusinessObjectFactory().Load<ProcessHeader>(workflow.PK);
			AssertTagNotApplied("custom sql filter strip should have removed the tag", workflow, tagMagnitude);

			RunTagRules(rule);
			AssertEquals("Verification should NOT have run", testDate, rule.TGR_LastPerformanceVerificationDateTimeUtc);
		}

		[TestDate(2019, 11, 1)]
		public void TestCustomSqlFilterStrip_Remove_ShouldRunPerformanceVerification_AndChangeMaxdop()
		{
			BMSRegistry.Instance.TagRulePerformanceCheckFrequency.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 2);

			var tagDefinition = BMSTestHelper.CreateTagDefinition(Factory, "AAA");
			tagDefinition.TGD_UsageScope = TagUsageScopeList.Codes.All;
			var tagMagnitude = BMSTestHelper.CreateTagMagnitude(tagDefinition, "AAA");

			var rule = BMSTestHelper.CreateTagRule(tagMagnitude, "Add CC1", TagRuleActionTypeList.Codes.RemoveTag);
			var stmFilter = rule.Filter;
			FilterStripsTestHelper.AddFilterStrips(stmFilter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Custom SQL Filter",
				FilterStripValueSetter = f => ((ModuleSQLFilter)f).Property1 = "FH_CompletionStatement = 'peekaboo'",
			});

			rule.TGR_LastPerformanceVerificationDateTimeUtc = ZDateTime.UtcNow.AddDays(-3);

			AssertEquals(false, rule.TGR_RunAddQuerySingleThreaded);
			AssertEquals(false, rule.TGR_RunMagnitudeQuerySingleThreaded);
			AssertEquals(false, rule.TGR_RunRemoveQuerySingleThreaded);

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			workflow.FH_CompletionStatement = "peekaboo";
			workflow.AddTag(tagMagnitude);

			rule.FirstRunTimeForTest = 9999.0;
			Factory.Save();

			AssertTagApplied("tag applied before removal", workflow, tagMagnitude);

			var connectionProvider = SecondaryServerConnectionProviderProvider.GetProvider();

			var logger = new TestServiceLogger();
			var dummyRunner = new DummyTagRuleRunner(logger, rule);
			dummyRunner.Strategy = new RemoveTagRuleRunSqlStrategyForTest(connectionProvider, logger);
			using (Db.Connection.TrackExecutedCommands())
			{
				dummyRunner.Process();

				AssertNotEquals("Verification should have run", ZDateTime.Empty, rule.TGR_LastPerformanceVerificationDateTimeUtc);
				AssertEquals(false, rule.TGR_RunAddQuerySingleThreaded);
				AssertEquals(false, rule.TGR_RunMagnitudeQuerySingleThreaded);
				AssertEquals(true, rule.TGR_RunRemoveQuerySingleThreaded);

				workflow = new BusinessObjectFactory().Load<ProcessHeader>(workflow.PK);
				AssertTagNotApplied("custom sql filter strip should have removed the tag", workflow, tagMagnitude);

				AssertContains("Information|Verified performance of rule [Add CC1], MAXDOP set to 1 (single).", logger.ToString());
				AssertEquals("MAXDOP of 1 should be present in the tag rule query", 1, Db.Connection.ExecutedCommands.Count(p => p.Contains("OPTION (MAXDOP 1)")));
			}

			BMSRegistry.Instance.ThreadedQuerySlownessThresholdFactor.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1.0m);
			rule.TGR_LastPerformanceVerificationDateTimeUtc = ZDateTime.Empty;
			rule.Schedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddMinutes(-1);
			Factory.Save();
			using (Db.Connection.TrackExecutedCommands())
			{
				dummyRunner.Process();

				AssertContains("Information|Verified performance of rule [Add CC1], MAXDOP disabled (parallel).", logger.ToString());
				AssertEquals("MAXDOP should not be present in the tag rule query", 0, Db.Connection.ExecutedCommands.Count(p => p.Contains("OPTION (MAXDOP 0)")));
				AssertEquals(false, rule.TGR_RunAddQuerySingleThreaded);
				AssertEquals(false, rule.TGR_RunMagnitudeQuerySingleThreaded);
				AssertEquals(false, rule.TGR_RunRemoveQuerySingleThreaded);
			}

			rule.TGR_LastPerformanceVerificationDateTimeUtc = ZDateTime.Empty;
			rule.FirstRunTimeForTest = 0.0;
			rule.Schedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddMinutes(-1);
			Factory.Save();

			using (Db.Connection.TrackExecutedCommands())
			{
				dummyRunner.Process();
				AssertContains("Information|Verified performance of rule [Add CC1], MAXDOP unchanged.", logger.ToString());
				AssertEquals("MAXDOP should not be present in the tag rule query", 0, Db.Connection.ExecutedCommands.Count(p => p.Contains("OPTION (MAXDOP 0)")));
			}
		}

		public void TestCustomSqlFilterStrip_ReportErrorInNotes()
		{
			BMSTestHelper.SetupEmailAndGroup(Factory);

			var tagDefinition = BMSTestHelper.CreateTagDefinition(Factory, "AAA");
			tagDefinition.TGD_UsageScope = TagUsageScopeList.Codes.Rule;
			var tagMagnitude = BMSTestHelper.CreateTagMagnitude(tagDefinition, "AAA");

			var rule = BMSTestHelper.CreateTagRule(tagMagnitude, "Add CC1", TagRuleActionTypeList.Codes.AddTag);
			var stmFilter = rule.Filter;
			FilterStripsTestHelper.AddFilterStrips(stmFilter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Custom SQL Filter",
				FilterStripValueSetter = f => ((ModuleSQLFilter)f).Property1 = "FH_CompletionStatement = peekaboo'", //There is a sneaky missing quote
			});

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			workflow.FH_CompletionStatement = "peekaboo";

			Factory.Save();

			RunAllTagRules();

			var newFactory = new BusinessObjectFactory();
			workflow = newFactory.Load<ProcessHeader>(workflow.PK);
			var loadedRule = newFactory.Load<TagRule>(rule.PK);
			AssertNotNull(loadedRule);
			var note = (StmNote)loadedRule.Notes.GetAllNotes().FirstOrDefault();
			AssertNotNull(note);
			AssertEquals("Rule deactivated", note.ST_Description);

			var executeAsReaderFlag = DbCommand.ExecuteAsReaderFlagComments.Replace("\n", "").Trim();
			AssertNotContains(executeAsReaderFlag, note.ST_NoteDataAsText);
			AssertContains("Error occurred while processing rule [Add CC1]. Rule is now deactivated. Error message: Unclosed quotation mark after the character string '", note.ST_NoteDataAsText);
			AssertTagNotApplied("custom sql filter strip should have applied the tag", workflow, tagMagnitude);

			ErrorReporter.Clear();
		}

		public void TestTemplateFilterStrip_ShouldNotAddTagToWorkflowTemplates()
		{
			BMSRegistry.Instance.TagRulePerformanceCheckFrequency.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 2);

			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "DVA");
			var tag = BMSTestHelper.CreateTagMagnitude(tagGroup, "RIP");

			var rule = BMSTestHelper.CreateTagRule(tag, "Nerf This", TagRuleActionTypeList.Codes.AddTag);
			var stmFilter = rule.Filter;

			BMSTestHelper.CreateSystem(Factory, "ORG");
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");
			var templateWorkflow1 = (ProcessHeader)template.ProcessHeaders.AddNew();
			var templateWorkflow2 = (ProcessHeader)template.ProcessHeaders.AddNew();

			templateWorkflow1.FH_CompletionStatement = "Rip in pizza sauce";
			templateWorkflow2.FH_CompletionStatement = "Rip in peperoni";

			var nonTemplateWorkflow3 = BMSTestHelper.CreateWorkflowAndParents<SalesEnquiry>(Factory, "Rip in peace");

			FilterStripsTestHelper.AddFilterStrips(stmFilter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Rip"
			});

			Factory.Save();

			RunTagRules(rule);

			var freshFactory1 = new BusinessObjectFactory();

			var workflow1 = freshFactory1.Load<ProcessHeader>(templateWorkflow1.PK);
			var workflow2 = freshFactory1.Load<ProcessHeader>(templateWorkflow2.PK);
			var workflow3 = freshFactory1.Load<ProcessHeader>(nonTemplateWorkflow3.PK);

			AssertTagNotApplied("Tag should not have been applied to workflow template.", workflow1, tag);
			AssertTagNotApplied("Tag should not have been applied to workflow template.", workflow2, tag);
			AssertTagApplied("Tag should have been applied to the other workflow matching the filter.", workflow3, tag);

			nonTemplateWorkflow3.RemoveTag(tag);

			FilterStripsTestHelper.AddFilterStrips(stmFilter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.Template,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "TMP"
			});

			Factory.Save();

			rule.Schedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddMinutes(-1);
			RunTagRules(rule);

			var freshFactory2 = new BusinessObjectFactory();

			workflow1 = freshFactory2.Load<ProcessHeader>(templateWorkflow1.PK);
			workflow2 = freshFactory2.Load<ProcessHeader>(templateWorkflow2.PK);
			workflow3 = freshFactory2.Load<ProcessHeader>(nonTemplateWorkflow3.PK);

			AssertTagNotApplied("Tag should not have been applied to workflow template.", workflow1, tag);
			AssertTagNotApplied("Tag should not have been applied to workflow template.", workflow2, tag);
			AssertTagNotApplied("Tag should not have been applied to the other workflow not matching the filter.", workflow3, tag);
		}

		public void TestTemplateFilterStrip_ShouldNotRemoveTagOnWorkflowTemplates()
		{
			BMSRegistry.Instance.TagRulePerformanceCheckFrequency.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 2);

			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "DVA");
			var tag = BMSTestHelper.CreateTagMagnitude(tagGroup, "RIP");

			var rule = BMSTestHelper.CreateTagRule(tag, "Heroes Never Die", TagRuleActionTypeList.Codes.RemoveTag);
			var stmFilter = rule.Filter;

			BMSTestHelper.CreateSystem(Factory, "ORG");
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");
			var templateWorkflow1 = (ProcessHeader)template.ProcessHeaders.AddNew();
			var templateWorkflow2 = (ProcessHeader)template.ProcessHeaders.AddNew();

			templateWorkflow1.FH_CompletionStatement = "Rip in pizza sauce";
			templateWorkflow2.FH_CompletionStatement = "Rip in peperoni";

			var nonTemplateWorkflow3 = BMSTestHelper.CreateWorkflowAndParents<SalesEnquiry>(Factory, "Rip in peace");

			templateWorkflow1.AddTag(tag);
			templateWorkflow2.AddTag(tag);
			nonTemplateWorkflow3.AddTag(tag);

			FilterStripsTestHelper.AddFilterStrips(stmFilter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Rip"
			});

			Factory.Save();

			rule.Schedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddMinutes(-1);
			RunTagRules(rule);

			var freshFactory1 = new BusinessObjectFactory();

			var workflow1 = freshFactory1.Load<ProcessHeader>(templateWorkflow1.PK);
			var workflow2 = freshFactory1.Load<ProcessHeader>(templateWorkflow2.PK);
			var workflow3 = freshFactory1.Load<ProcessHeader>(nonTemplateWorkflow3.PK);

			AssertTagApplied("Tag should not have been removed from workflow template.", workflow1, tag);
			AssertTagApplied("Tag should not have been removed from workflow template.", workflow2, tag);
			AssertTagNotApplied("Tag should have been removed from the other workflow matching the filter.", workflow3, tag);

			nonTemplateWorkflow3.AddTag(tag);

			FilterStripsTestHelper.AddFilterStrips(stmFilter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.Template,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "TMP"
			});

			Factory.Save();

			rule.Schedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddMinutes(-1);
			RunTagRules(rule);

			var freshFactory2 = new BusinessObjectFactory();

			workflow1 = freshFactory2.Load<ProcessHeader>(templateWorkflow1.PK);
			workflow2 = freshFactory2.Load<ProcessHeader>(templateWorkflow2.PK);
			workflow3 = freshFactory2.Load<ProcessHeader>(nonTemplateWorkflow3.PK);

			AssertTagApplied("Tag should not have been removed from workflow template.", workflow1, tag);
			AssertTagApplied("Tag should not have been removed from workflow template.", workflow2, tag);
			AssertTagApplied("Tag should not have been removed from the other workflow not matching the filter.", workflow3, tag);
		}

		public void TestTemplateFilterStrip_ShouldMaintainTagOnWorkflowTemplates()
		{
			BMSRegistry.Instance.TagRulePerformanceCheckFrequency.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 2);

			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "DVA");
			var tag = BMSTestHelper.CreateTagMagnitude(tagGroup, "RIP");

			var rule = BMSTestHelper.CreateTagRule(tag, "Maintain Magnitude CCC", TagRuleActionTypeList.Codes.MaintainMagnitude);
			var oldMagnitude = 2m;
			var newMagnitude = 7m;
			rule.TagTemplate.TGL_Magnitude = newMagnitude;
			var stmFilter = rule.Filter;

			BMSTestHelper.CreateSystem(Factory, "ORG");
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");
			var templateWorkflow1 = (ProcessHeader)template.ProcessHeaders.AddNew();
			var templateWorkflow2 = (ProcessHeader)template.ProcessHeaders.AddNew();

			templateWorkflow1.FH_CompletionStatement = "Rip in pizza sauce";
			templateWorkflow2.FH_CompletionStatement = "Rip in pepperoni";

			var nonTemplateWorkflow3 = BMSTestHelper.CreateWorkflowAndParents<SalesEnquiry>(Factory, "Rip in peace");

			templateWorkflow1.AddTag(tag);
			templateWorkflow2.AddTag(tag);
			nonTemplateWorkflow3.AddTag(tag);

			templateWorkflow1.TagLinks.First().TGL_Magnitude = oldMagnitude;
			templateWorkflow2.TagLinks.First().TGL_Magnitude = oldMagnitude;
			nonTemplateWorkflow3.TagLinks.First().TGL_Magnitude = oldMagnitude;

			FilterStripsTestHelper.AddFilterStrips(stmFilter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Rip"
			});

			Factory.Save();

			RunTagRules(rule);

			var freshFactory1 = new BusinessObjectFactory();

			var workflow1 = freshFactory1.Load<ProcessHeader>(templateWorkflow1.PK);
			var workflow2 = freshFactory1.Load<ProcessHeader>(templateWorkflow2.PK);
			var workflow3 = freshFactory1.Load<ProcessHeader>(nonTemplateWorkflow3.PK);

			AssertEquals("Should have old magnitude", oldMagnitude, workflow1.TagLinks.First().TGL_Magnitude);
			AssertEquals("Should have old magnitude", oldMagnitude, workflow2.TagLinks.First().TGL_Magnitude);
			AssertEquals("Should have new magnitude", newMagnitude, workflow3.TagLinks.First().TGL_Magnitude);

			nonTemplateWorkflow3.RemoveTag(tag);
			nonTemplateWorkflow3.AddTag(tag);
			nonTemplateWorkflow3.TagLinks.First().TGL_Magnitude = oldMagnitude;

			FilterStripsTestHelper.AddFilterStrips(stmFilter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.Template,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "TMP"
			});

			Factory.Save();

			rule.Schedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddMinutes(-1);
			RunTagRules(rule);

			var freshFactory2 = new BusinessObjectFactory();

			workflow1 = freshFactory2.Load<ProcessHeader>(templateWorkflow1.PK);
			workflow2 = freshFactory2.Load<ProcessHeader>(templateWorkflow2.PK);
			workflow3 = freshFactory2.Load<ProcessHeader>(nonTemplateWorkflow3.PK);

			AssertEquals("Should have old magnitude", oldMagnitude, workflow1.TagLinks.First().TGL_Magnitude);
			AssertEquals("Should have old magnitude", oldMagnitude, workflow2.TagLinks.First().TGL_Magnitude);
			AssertEquals("Should have old magnitude", oldMagnitude, workflow3.TagLinks.First().TGL_Magnitude);
		}

		public void TestModuleGuidForeignCollectionFilter_AllMatchOperator_ShouldNotCreateInvalidQuery()
		{
			var rule = BMSTestHelper.CreateTagRuleWithDefAndMag(Factory, actionType: TagRuleActionTypeList.Codes.AddAndRemoveTag);
			FilterStripsTestHelper.AddFilterStrips(rule.Filter, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Tasks",
				ComparisonOperatorSetter = f => ((ModuleGuidForeignCollectionFilter)f).ComparisonOperator = ModuleTextFilter.ComparisonConstants.AllMatch,
				FilterStripValueSetter = f =>
				{
					((ModuleGuidForeignCollectionFilter)f).SelectedFilters.AddNkFilterStrip("Staff", "1");
				}
			});

			var logger = new TestServiceLogger();
			var runner = new DummyTagRuleRunner(logger, rule);
			runner.Process();

			AssertNotContains("The multi-part identifier \"ProcessHeader.FH_PK\" could not be bound", logger.ToString());
		}

		[TestDate(2018, 9, 25)]
		public void TestModuleGuidForeignCollectionFilter_AllMatchOperator_ShouldCorrectlyApplyTags()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "Shmasier", taskType: "COM");
			var workflow2 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "Shmiles", taskType: "UDF");
			var workflow3 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "Shmartin", taskType: "COM");
			var workflow4 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "Shmoz", taskType: "UDF");
			var workflow5 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "Shmaphne", taskType: "BUN");

			workflow1.Tasks.Single().P9_ScheduledDate = new ZDateTime(2018, 9, 26);
			workflow2.Tasks.Single().P9_ScheduledDate = new ZDateTime(2018, 9, 27);

			workflow3.Tasks.Single().P9_ScheduledDate = new ZDateTime(2018, 9, 28);
			var extraTask = BMSTestHelper.CreateTask(workflow3);
			extraTask.P9_ScheduledDate = new ZDateTime(2018, 9, 26);

			workflow4.Tasks.Single().P9_ScheduledDate = new ZDateTime(2018, 9, 29);
			workflow5.Tasks.Single().P9_ScheduledDate = new ZDateTime(2018, 9, 30);

			Factory.Save();

			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "DVA");
			var tag = BMSTestHelper.CreateTagMagnitude(tagGroup, "RIP");
			var rule = BMSTestHelper.CreateTagRule(tag, "Shmossed Shmalad and Shmambled Shmeggs", TagRuleActionTypeList.Codes.AddTag);

			FilterStripsTestHelper.AddFilterStrips(rule.Filter, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Tasks",
				ComparisonOperatorSetter = filter => ((ModuleGuidForeignCollectionFilter)filter).ComparisonOperator = ModuleTextFilter.ComparisonConstants.AllMatch,
				FilterStripValueSetter = filter =>
				{
					var dateFilterStrip = ((ModuleGuidForeignCollectionFilter)filter).SelectedFilters.AddDateFilterStrip("Scheduled Start");
					dateFilterStrip.Property1 = new ZDateTime(2018, 9, 27);
					dateFilterStrip.Property2 = new ZDateTime(2018, 9, 29);
					dateFilterStrip.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
				}
			});

			Factory.Save();

			var logger = new TestServiceLogger();
			var runner = new DummyTagRuleRunner(logger, rule);

			runner.Process();

			CombineAssertions("The tags all need to be correctly applied, otherwise what's this all been about? What are we working towards?", () =>
			{
				AssertTagNotApplied(workflow1, tag, reloadInNewFactory: true);
				AssertTagApplied(workflow2, tag, reloadInNewFactory: true);
				AssertTagNotApplied(workflow3, tag, reloadInNewFactory: true);
				AssertTagApplied(workflow4, tag, reloadInNewFactory: true);
				AssertTagNotApplied(workflow5, tag, reloadInNewFactory: true);
			});
		}

		public void TestFilterStripWithTableValuedParameter_ShouldNotCauseExceptions()
		{
			BMSTestHelper.AddTaskTypesToRegistry("ORG", "COM", "BUN");

			var jobHeader = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "Shmlony", taskType: "COM", taskStatus: "SUS");
			var workflow2 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "Shmlantha", taskType: "UDF", taskStatus: "SUS");
			var workflow3 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "Shmlona", taskType: "COM", taskStatus: "ASN");
			var workflow4 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "Shmlangela", taskType: "UDF", taskStatus: "ASN");
			var workflow5 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "Shmlonathan", taskType: "BUN", taskStatus: "SUS");

			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "DVA");
			var tag = BMSTestHelper.CreateTagMagnitude(tagGroup, "RIP");
			var rule = BMSTestHelper.CreateTagRule(tag, "Shmloo's the Shmloss?", TagRuleActionTypeList.Codes.AddTag);

			FilterStripsTestHelper.AddFilterStrips(rule.Filter, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Aggregated Task Status",
				FilterStripValueSetter = f =>
				{
					var filter = (ITaskStatusFilter)f;
					filter.TaskAggregator = "ANY";
					filter.TaskStatusCheckList[ProcessTaskStatusCodeList.Codes.Suspended].Value = true;
					filter.TaskTypeCheckList.First(l => l.Description.StartsWith("COM")).Value = true;
					filter.TaskTypeCheckList.First(l => l.Description.StartsWith("UDF")).Value = true;
				}
			});

			Factory.Save();

			var logger = new TestServiceLogger();
			var runner = new DummyTagRuleRunner(logger, rule);

			using (TestConnection.TrackExecutedCommands())
			{
				AssertNoExceptionThrown(runner.Process);
				AssertNull(ErrorReporter.LastExceptionReported?.Message);

				var command = TestConnection.ExecutedCommands.FirstOrDefault(x => x.Contains("P9_Type IN (SELECT Value FROM @TaskStatusFilterTypes"));

				AssertNotNull("The filter strip needs to use a table valued parameter, otherwise what's this all been about? What are we working towards?", command);
			}

			AssertTagApplied(workflow1, tag, reloadInNewFactory: true);
			AssertTagApplied(workflow2, tag, reloadInNewFactory: true);
			AssertTagNotApplied(workflow3, tag, reloadInNewFactory: true);
			AssertTagNotApplied(workflow4, tag, reloadInNewFactory: true);
			AssertTagNotApplied(workflow5, tag, reloadInNewFactory: true);
		}

		public void TestFilterStripWithCountrySpecificModule_WhenThatModuleNotAvailableInRuleContext_ShouldLogErrorAndDisableRule()
		{
			var company = Factory.New<IGlbCompany>();
			((BusinessObject)company).FillWithValidTestData();
			company.GC_RN_NKCountryCode = "DE";

			var branch = Factory.New<IGlbBranch>();
			((BusinessObject)branch).FillWithValidTestData();
			branch.GB_GC = company.PK;

			BMSTestHelper.SetupEmailAndGroup(Factory);

			var config = TestConfigsHelper.CreateTagsTestConfig(Factory, "DUM");
			var rule = BMSTestHelper.CreateTagRule(config.PrincessCelestiaTag, "Bad Rule", TagRuleActionTypeList.Codes.MaintainMagnitude);
			rule.TGR_GB_Branch = branch.PK;

			FilterStripsTestHelper.AddFilterStrip<ModuleGuidModuleSpecifiedFilter>(rule.Filter, "Parent Job",
				(filter) => filter.SelectedModule = ModuleIDs.Customs.AU.AirCargoOutturnBills.Name,
				(filter) => filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch);

			Factory.Save();

			var logger = new BufferManagementLogger();
			var runner = new DummyTagRuleRunner(logger, rule);

			AssertNoExceptionThrown("The bad filter strip should not cause the runner to throw an uncaught exception. SAD!", () => runner.Process());
			AssertContains(@"Error - Error occurred while processing rule [Bad Rule]. Rule is now deactivated. Error message: Unable to load the query for this filter's selected filters. Please ensure that the module associated with the filter is available in the current context.
Filter: Parent Job
Module: AirCargoOutturnBills", logger.ToString());

			rule.Reload();
			AssertEquals("The rule had a bad filter/branch combination so it should have been disabled. SAD!", false, rule.TGR_IsActive);
			AssertEquals("We should attempt to inform the BMS admins in this case. SAD!", 1, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		#endregion

		#region Logging and Events

		public void TestAddTagShouldAddAuditDetails()
		{
			var tagDefinition = BMSTestHelper.CreateTagDefinition(Factory, "AAA");
			tagDefinition.TGD_UsageScope = TagUsageScopeList.Codes.Rule;
			var tagMagnitude = BMSTestHelper.CreateTagMagnitude(tagDefinition, "AAA");

			var rule = BMSTestHelper.CreateTagRule(tagMagnitude, "Add CC1", TagRuleActionTypeList.Codes.AddTag);
			var stmFilter = rule.Filter;
			FilterStripsTestHelper.AddFilterStrips(stmFilter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Custom SQL Filter",
				FilterStripValueSetter = f => ((ModuleSQLFilter)f).Property1 = "FH_CompletionStatement = 'peekaboo'",
			});

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			workflow.FH_CompletionStatement = "peekaboo";

			Factory.Save();

			RunTagRules(rule);

			workflow = new BusinessObjectFactory().Load<ProcessHeader>(workflow.PK);
			var tagLink = workflow.TagLinks.Single();
			Assert(tagLink.TGL_SystemCreateTimeUtc >= ZDateTime.UtcNow.AddDays(-1));
			Assert(tagLink.TGL_SystemLastEditTimeUtc >= ZDateTime.UtcNow.AddDays(-1));
			AssertEquals("~BP", tagLink.TGL_SystemCreateUser);
			AssertEquals("~BP", tagLink.TGL_SystemLastEditUser);
		}

		public void TestAddTagShouldLogOnWorkflow()
		{
			var tagDefinition = BMSTestHelper.CreateTagDefinition(Factory, "AAA");
			tagDefinition.TGD_UsageScope = TagUsageScopeList.Codes.Rule;
			var tagMagnitude = BMSTestHelper.CreateTagMagnitude(tagDefinition, "AAA");

			var rule = BMSTestHelper.CreateTagRule(tagMagnitude, "Add CC1", TagRuleActionTypeList.Codes.AddTag);
			var stmFilter = rule.Filter;
			FilterStripsTestHelper.AddFilterStrips(stmFilter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Custom SQL Filter",
				FilterStripValueSetter = f => ((ModuleSQLFilter)f).Property1 = "FH_CompletionStatement = 'peekaboo'",
			});

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			workflow.FH_CompletionStatement = "peekaboo";

			Factory.Save();

			RunTagRules(rule);

			workflow = new BusinessObjectFactory().Load<ProcessHeader>(workflow.PK);

			var jobLog = ((OrgHeader)workflow.Parent).Logs.GetAllLogs().Cast<StmALog>().SingleOrDefault(l => l.SL_Reference.StartsWith("Added"));
			AssertNull(jobLog);

			AssertTagApplied("custom sql filter strip should have applied the tag", workflow, tagMagnitude);
			AssertTagEventRaised(workflow, TagActionType.AddTag, tagMagnitude, rule);
		}

		public void TestAddTagShouldLogOnWorkflow_Exclusive()
		{
			var tagDefinition = BMSTestHelper.CreateTagDefinition(Factory, "AAA", "C");
			tagDefinition.TGD_UsageScope = TagUsageScopeList.Codes.All;
			tagDefinition.TGD_IsExclusive = true;
			var tagMagnitude1 = BMSTestHelper.CreateTagMagnitude(tagDefinition, "AAA", "A");
			tagMagnitude1.TGM_RuleRunSequence = 0;
			var tagMagnitude2 = BMSTestHelper.CreateTagMagnitude(tagDefinition, "BBB", "B");
			tagMagnitude2.TGM_RuleRunSequence = 1;

			var rule = BMSTestHelper.CreateTagRule(tagMagnitude1, "Add CC1", TagRuleActionTypeList.Codes.AddTag);
			var stmFilter = rule.Filter;
			FilterStripsTestHelper.AddFilterStrips(stmFilter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Custom SQL Filter",
				FilterStripValueSetter = f => ((ModuleSQLFilter)f).Property1 = "FH_CompletionStatement = 'peekaboo'",
			});

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			workflow.FH_CompletionStatement = "peekaboo";
			workflow.AddTag(tagMagnitude2);

			Factory.Save();

			RunTagRules(rule);

			workflow = new BusinessObjectFactory().Load<ProcessHeader>(workflow.PK);

			AssertTagApplied("custom sql filter strip should have applied the tag", workflow, tagMagnitude1);
			AssertTagNotApplied("custom sql filter strip should have removed the tag", workflow, tagMagnitude2);

			AssertTagEventRaised(workflow, TagActionType.RemoveTag, tagMagnitude2, rule);
			AssertTagEventRaised(workflow, TagActionType.AddTag, tagMagnitude1, rule);
		}

		public void TestRemoveTagShouldLogOnWorkflow()
		{
			var tagDefinition = BMSTestHelper.CreateTagDefinition(Factory, "AAA");
			tagDefinition.TGD_UsageScope = TagUsageScopeList.Codes.All;
			var tagMagnitude = BMSTestHelper.CreateTagMagnitude(tagDefinition, "AAA");

			var rule = BMSTestHelper.CreateTagRule(tagMagnitude, "Add CC1", TagRuleActionTypeList.Codes.RemoveTag);
			var stmFilter = rule.Filter;
			FilterStripsTestHelper.AddFilterStrips(stmFilter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Custom SQL Filter",
				FilterStripValueSetter = f => ((ModuleSQLFilter)f).Property1 = "FH_CompletionStatement = 'peekaboo'",
			});

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			workflow.FH_CompletionStatement = "peekaboo";
			workflow.AddTag(tagMagnitude);

			Factory.Save();

			tagDefinition.TGD_UsageScope = TagUsageScopeList.Codes.Rule;

			Factory.Save();

			RunTagRules(rule);

			workflow = new BusinessObjectFactory().Load<ProcessHeader>(workflow.PK);

			var jobLog = ((OrgHeader)workflow.Parent).Logs.GetAllLogs().Cast<StmALog>().SingleOrDefault(l => l.SL_Reference.StartsWith("Removed"));
			AssertNull(jobLog);

			AssertTagNotApplied("custom sql filter strip should have unapplied the tag", workflow, tagMagnitude);
			AssertTagEventRaised(workflow, TagActionType.RemoveTag, tagMagnitude, rule);
		}

		[TestUtcOffset(10, 0, 0)]
		public void TestAddTag_ShouldFireUniversalTriggerOnJob()
		{
			var config = TestConfigsHelper.CreateTagsTestConfig(Factory);

			var universalTemplate = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG", isUniversal: true);
			var universalTrigger = BMSTestHelper.CreateUniversalTrigger(universalTemplate, Events.TagWasAddedOrRemoved);

			Factory.Save();

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "It's Friiiday");

			var rule = BMSTestHelper.CreateTagRule(config.PrincessCelestiaTag, "ORLY?", TagRuleActionTypeList.Codes.AddTag);
			rule.TGR_LastPerformanceVerificationDateTimeUtc = ZDateTime.UtcToday;

			FilterStripsTestHelper.AddFilterStrips(rule.Filter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Custom SQL Filter",
				FilterStripValueSetter = f => ((ModuleSQLFilter)f).Property1 = "FH_CompletionStatement = 'It''s Friiiday'",
			});

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var loadedRule = newFactory.Load<TagRule>(rule.PK);

			RunTagRules(loadedRule);

			var loadedWorkflow = newFactory.Load<ProcessHeader>(workflow.PK);
			AssertTagApplied(loadedWorkflow, config.PrincessCelestiaTag);

			BMSTestHelper.RunLogWalker();
			BMSTestHelper.RunLogWalker();

			newFactory = newFactory.CreateNewFactory();

			loadedWorkflow = newFactory.Load<ProcessHeader>(workflow.PK);
			loadedWorkflow.Parent.WorkflowItems.TriggersIncludingRelated.Rebuild();

			var log = loadedWorkflow.GetLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.TagWasAddedOrRemovedCode)).Single();
			var trigger = (ProcessTask)loadedWorkflow.Parent.WorkflowItems.TriggersIncludingRelated.Single();

			AssertEquals(log.SL_EventTime, ((IWorkflowTrigger)trigger).LastFiredTime.ToZDateTime());
			AssertEquals(log.SL_EventTimeUtc, ((IWorkflowTrigger)trigger).LastFiredTime.ToUtcZDateTime());

			var jobTrigger = universalTrigger.GetOrCreateJobVersionOfTrigger((IBusiness)loadedWorkflow.Parent, createIfNotFound: false);

			AssertNotNull(jobTrigger);
			AssertEquals(log.SL_EventTime, jobTrigger.LastFiredTime.ToZDateTime());
			AssertEquals(log.SL_EventTime.AddHours(-10), jobTrigger.LastFiredTime.ToUtcZDateTime());
		}

		#endregion

		#region Exclusive Tags

		public void TestApplyExclusiveTag_ShouldLeaveOtherTags()
		{
			var userScopeDefinition = BMSTestHelper.CreateTagDefinition(Factory, "AAA");
			userScopeDefinition.TGD_UsageScope = TagUsageScopeList.Codes.User;
			var userScopeMagnitude = BMSTestHelper.CreateTagMagnitude(userScopeDefinition, "AAA");

			var nonExclusiveDefinition = BMSTestHelper.CreateTagDefinition(Factory, "BBB");
			var nonExclusiveMagnitude = BMSTestHelper.CreateTagMagnitude(nonExclusiveDefinition, "BBB");

			var exclusiveDefinition = BMSTestHelper.CreateTagDefinition(Factory, "CCC");
			exclusiveDefinition.TGD_IsExclusive = true;
			var exclusiveMagnitude = BMSTestHelper.CreateTagMagnitude(exclusiveDefinition, "CCC");

			var rule = BMSTestHelper.CreateTagRule(exclusiveMagnitude, "Add CC1", TagRuleActionTypeList.Codes.AddTag);
			var stmFilter = rule.Filter;
			FilterStripsTestHelper.AddFilterStrips(stmFilter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Look out!",
			});

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			workflow.FH_CompletionStatement = "Look out!";
			workflow.AddTag(userScopeMagnitude);
			workflow.AddTag(nonExclusiveMagnitude);

			Factory.Save();

			RunTagRules(rule);

			workflow = new BusinessObjectFactory().Load<ProcessHeader>(workflow.PK);
			AssertTagApplied("Should leave existing tags from another tag group applied", workflow, userScopeMagnitude);
			AssertTagApplied("Should leave existing tags from another tag group applied", workflow, nonExclusiveMagnitude);
			AssertTagApplied("Should add tag from exclusive tag group", workflow, exclusiveMagnitude);
		}

		#endregion

		#region AddAndRemove (ARM) TagRule Action Code with RunSqlStrategy

		public void TestARMTagRuleActionCode_RunSqlStrategy()
		{
			var tagDef = BMSTestHelper.CreateTagDefinition(Factory, "CCC");
			var tagMag = BMSTestHelper.CreateTagMagnitude(tagDef, "CCC");

			var rule = BMSTestHelper.CreateTagRule(tagMag, "Add and Remove CCC", TagRuleActionTypeList.Codes.AddAndRemoveTag);
			var stmFilter = rule.Filter;
			FilterStripsTestHelper.AddFilterStrips(stmFilter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "This one!",
			});

			var workflow1 = CreateJobHeader<OrgHeader>().ProcessHeaders.AddNew();
			var workflow2 = CreateJobHeader<OrgHeader>().ProcessHeaders.AddNew();
			workflow1.FH_CompletionStatement = "This one!";
			workflow2.FH_CompletionStatement = "Not this one?";
			workflow2.AddTag(tagMag);

			Factory.Save();

			workflow1 = new BusinessObjectFactory().Load<ProcessHeader>(workflow1.PK);
			workflow2 = new BusinessObjectFactory().Load<ProcessHeader>(workflow2.PK);

			AssertTagNotApplied("Tag yet to be applied", workflow1, tagMag);
			AssertTagApplied("Tag yet to be removed", workflow2, tagMag);

			RunTagRules(rule);

			workflow1 = new BusinessObjectFactory().Load<ProcessHeader>(workflow1.PK);
			workflow2 = new BusinessObjectFactory().Load<ProcessHeader>(workflow2.PK);

			AssertTagApplied("Should add tag to matching item", workflow1, tagMag);
			AssertTagNotApplied("Should remove tag from non-matching item", workflow2, tagMag);
		}

		public void TestARMTagRuleActionCode_ShouldNotRunPerformanceVerification_IfPerformanceCheckDisabled()
		{
			BMSRegistry.Instance.TagRulePerformanceCheckFrequency.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);

			var tagDef = BMSTestHelper.CreateTagDefinition(Factory, "CCC");
			var tagMag = BMSTestHelper.CreateTagMagnitude(tagDef, "CCC");

			var rule = BMSTestHelper.CreateTagRule(tagMag, "Add and Remove CCC", TagRuleActionTypeList.Codes.AddAndRemoveTag);
			var stmFilter = rule.Filter;
			FilterStripsTestHelper.AddFilterStrips(stmFilter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "This one!",
			});

			var workflow1 = CreateJobHeader<OrgHeader>().ProcessHeaders.AddNew();
			var workflow2 = CreateJobHeader<OrgHeader>().ProcessHeaders.AddNew();
			workflow1.FH_CompletionStatement = "This one!";
			workflow2.FH_CompletionStatement = "Not this one?";
			workflow2.AddTag(tagMag);

			Factory.Save();

			RunTagRules(rule);

			AssertEquals("Verification should not have run", ZDateTime.Empty, rule.TGR_LastPerformanceVerificationDateTimeUtc);
		}

		[TestDate(2019, 11, 1)]
		public void TestARMTagRuleActionCode_ShouldRunPerformanceVerification_AndChangeMaxdop()
		{
			BMSRegistry.Instance.TagRulePerformanceCheckFrequency.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 2);

			var tagDef = BMSTestHelper.CreateTagDefinition(Factory, "CCC");
			var tagMag = BMSTestHelper.CreateTagMagnitude(tagDef, "CCC");

			var rule = BMSTestHelper.CreateTagRule(tagMag, "Add and Remove CCC", TagRuleActionTypeList.Codes.AddAndRemoveTag);
			var stmFilter = rule.Filter;
			FilterStripsTestHelper.AddFilterStrips(stmFilter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "This one!",
			});

			var workflow1 = CreateJobHeader<OrgHeader>().ProcessHeaders.AddNew();
			var workflow2 = CreateJobHeader<OrgHeader>().ProcessHeaders.AddNew();
			workflow1.FH_CompletionStatement = "This one!";
			workflow2.FH_CompletionStatement = "Not this one?";
			workflow2.AddTag(tagMag);

			rule.TGR_LastPerformanceVerificationDateTimeUtc = ZDateTime.UtcNow.AddDays(-3);
			rule.TGR_LastRunDurationInSeconds = 9999;

			AssertEquals(false, rule.TGR_RunAddQuerySingleThreaded);
			AssertEquals(false, rule.TGR_RunMagnitudeQuerySingleThreaded);
			AssertEquals(false, rule.TGR_RunRemoveQuerySingleThreaded);

			rule.FirstRunTimeForTest = 9999.0;
			Factory.Save();

			workflow1 = new BusinessObjectFactory().Load<ProcessHeader>(workflow1.PK);
			workflow2 = new BusinessObjectFactory().Load<ProcessHeader>(workflow2.PK);

			AssertTagNotApplied("Tag yet to be applied", workflow1, tagMag);
			AssertTagApplied("Tag yet to be removed", workflow2, tagMag);

			var connectionProvider = SecondaryServerConnectionProviderProvider.GetProvider();

			var logger = new TestServiceLogger();
			var dummyRunner = new DummyTagRuleRunner(logger, rule);
			dummyRunner.Strategy = new AddAndRemoveTagRuleRunSqlStrategyForTest(connectionProvider, logger);
			using (Db.Connection.TrackExecutedCommands())
			{
				dummyRunner.Process();

				workflow1 = new BusinessObjectFactory().Load<ProcessHeader>(workflow1.PK);
				workflow2 = new BusinessObjectFactory().Load<ProcessHeader>(workflow2.PK);

				AssertTagApplied("Should add tag to matching item", workflow1, tagMag);
				AssertTagNotApplied("Should remove tag from non-matching item", workflow2, tagMag);

				AssertEquals(true, rule.TGR_RunAddQuerySingleThreaded);
				AssertEquals(false, rule.TGR_RunMagnitudeQuerySingleThreaded);
				AssertEquals(true, rule.TGR_RunRemoveQuerySingleThreaded);

				AssertContains("Information|Verified performance of rule [Add and Remove CCC], MAXDOP set to 1 (single).", logger.ToString());
				AssertEquals("MAXDOP of 1 should be present in the tag rule query", 2, Db.Connection.ExecutedCommands.Count(p => p.Contains("OPTION (MAXDOP 1)")));
			}

			BMSRegistry.Instance.ThreadedQuerySlownessThresholdFactor.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1.0m);
			rule.TGR_LastPerformanceVerificationDateTimeUtc = ZDateTime.Empty;
			rule.Schedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddMinutes(-1);
			Factory.Save();

			using (Db.Connection.TrackExecutedCommands())
			{
				dummyRunner.Process();

				AssertContains("Information|Verified performance of rule [Add and Remove CCC], MAXDOP disabled (parallel).", logger.ToString());
				AssertEquals("MAXDOP should not be present in the tag rule query", 0, Db.Connection.ExecutedCommands.Count(p => p.Contains("OPTION (MAXDOP 0)")));
				AssertEquals(false, rule.TGR_RunAddQuerySingleThreaded);
				AssertEquals(false, rule.TGR_RunMagnitudeQuerySingleThreaded);
				AssertEquals(false, rule.TGR_RunRemoveQuerySingleThreaded);
			}

			rule.TGR_LastPerformanceVerificationDateTimeUtc = ZDateTime.Empty;
			rule.FirstRunTimeForTest = 0.0;
			rule.Schedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddMinutes(-1);
			Factory.Save();

			using (Db.Connection.TrackExecutedCommands())
			{
				dummyRunner.Process();
				AssertContains("Information|Verified performance of rule [Add and Remove CCC], MAXDOP unchanged.", logger.ToString());
				AssertEquals("MAXDOP should not be present in the tag rule query", 0, Db.Connection.ExecutedCommands.Count(p => p.Contains("OPTION (MAXDOP 0)")));
			}
		}

		public void TestThatSneakyNullReference()
		{
			var tagDef = BMSTestHelper.CreateTagDefinition(Factory, "CCC");
			var tagMag = BMSTestHelper.CreateTagMagnitude(tagDef, "CCC");

			var rule = BMSTestHelper.CreateTagRule(tagMag, "Add and Remove CCC", TagRuleActionTypeList.Codes.AddAndRemoveTag);
			var stmFilter = rule.Filter;
			FilterStripsTestHelper.AddFilterStrips(stmFilter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "This one!",
			});

			Factory.Save();

			var loadedRule = Factory.CreateNewFactory().Load<TagRule>(rule.PK);
			loadedRule.Factory.RefreshEnabled = false;
			loadedRule.Delete();
			loadedRule.Factory.Save();

			rule.TGR_Name = string.Empty;

			AssertHasError("Because the rule is invalid, it won't be executed by the rule runner.", rule.TGR_NameInfo, "Please enter a Name.");
			AssertNoExceptionThrown("There used to be a sneaky null reference exception here.", () => RunTagRules(rule));
		}

		public void TestAddTagRuleActionCode_RunSqlStrategy_ShouldNotApplyToTemplateWorkflow()
		{
			var tagDef = BMSTestHelper.CreateTagDefinition(Factory, "CCC");
			var tagMag = BMSTestHelper.CreateTagMagnitude(tagDef, "CCC");

			var rule = BMSTestHelper.CreateTagRule(tagMag, "Add CCC", TagRuleActionTypeList.Codes.AddTag);
			var stmFilter = rule.Filter;
			FilterStripsTestHelper.AddFilterStrips(stmFilter,
				new FilterStripsTestHelper.FilterStripDefinition
				{
					FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
					FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "This one!",
				});

			BMSTestHelper.CreateSystem(Factory, "ORG");
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");
			var templateWorkflow = (ProcessHeader)template.ProcessHeaders.AddNew();
			templateWorkflow.FH_CompletionStatement = "This one!";

			Factory.Save();

			templateWorkflow = new BusinessObjectFactory().Load<ProcessHeader>(templateWorkflow.PK);

			AssertTagNotApplied("Tag is not applied", templateWorkflow, tagMag);

			RunTagRules(rule);

			templateWorkflow = new BusinessObjectFactory().Load<ProcessHeader>(templateWorkflow.PK);

			AssertTagNotApplied("Should not apply tag since the item is a template workflow", templateWorkflow, tagMag);
		}

		public void TestRemoveTagRuleActionCode_RunSqlStrategy_ShouldNotRemoveFromTemplateWorkflow()
		{
			var tagDef = BMSTestHelper.CreateTagDefinition(Factory, "CCC");
			var tagMag = BMSTestHelper.CreateTagMagnitude(tagDef, "CCC");

			var rule = BMSTestHelper.CreateTagRule(tagMag, "Remove CCC", TagRuleActionTypeList.Codes.RemoveTag);
			var stmFilter = rule.Filter;
			FilterStripsTestHelper.AddFilterStrips(stmFilter,
				new FilterStripsTestHelper.FilterStripDefinition
				{
					FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
					FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "This one!",
				});

			BMSTestHelper.CreateSystem(Factory, "ORG");
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");
			var templateWorkflow = (ProcessHeader)template.ProcessHeaders.AddNew();
			templateWorkflow.FH_CompletionStatement = "This one!";
			templateWorkflow.AddTag(tagMag);

			Factory.Save();

			templateWorkflow = new BusinessObjectFactory().Load<ProcessHeader>(templateWorkflow.PK);

			AssertNotEquals(ZGuid.Empty, templateWorkflow.FH_P0_Template);
			AssertTagApplied("Tag is applied", templateWorkflow, tagMag);

			RunTagRules(rule);

			templateWorkflow = new BusinessObjectFactory().Load<ProcessHeader>(templateWorkflow.PK);

			AssertTagApplied("Should not remove tag since the item is a template workflow", templateWorkflow, tagMag);
		}

		public void TestAddRemoveTagRuleActionCode_RunSqlStrategy_ShouldNotAffectTemplateWorkflow()
		{
			var tagDef = BMSTestHelper.CreateTagDefinition(Factory, "CCC");
			var tagMag = BMSTestHelper.CreateTagMagnitude(tagDef, "CCC");

			var rule = BMSTestHelper.CreateTagRule(tagMag, "Add and Remove CCC", TagRuleActionTypeList.Codes.AddAndRemoveTag);
			var stmFilter = rule.Filter;
			FilterStripsTestHelper.AddFilterStrips(stmFilter,
				new FilterStripsTestHelper.FilterStripDefinition
				{
					FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
					FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "This one!",
				});

			BMSTestHelper.CreateSystem(Factory, "ORG");
			var template1 = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");
			var templateWorkflow1 = (ProcessHeader)template1.ProcessHeaders.AddNew();
			templateWorkflow1.FH_CompletionStatement = "This one!";

			var template2 = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");
			var templateWorkflow2 = (ProcessHeader)template2.ProcessHeaders.AddNew();
			templateWorkflow2.FH_CompletionStatement = "Not this one!";
			templateWorkflow2.AddTag(tagMag);

			Factory.Save();

			templateWorkflow1 = new BusinessObjectFactory().Load<ProcessHeader>(templateWorkflow1.PK);
			templateWorkflow2 = new BusinessObjectFactory().Load<ProcessHeader>(templateWorkflow2.PK);

			AssertNotEquals(ZGuid.Empty, templateWorkflow1.FH_P0_Template);
			AssertNotEquals(ZGuid.Empty, templateWorkflow2.FH_P0_Template);

			AssertTagNotApplied("Tag is not applied", templateWorkflow1, tagMag);
			AssertTagApplied("Tag is applied", templateWorkflow2, tagMag);

			RunTagRules(rule);

			templateWorkflow1 = new BusinessObjectFactory().Load<ProcessHeader>(templateWorkflow1.PK);
			templateWorkflow2 = new BusinessObjectFactory().Load<ProcessHeader>(templateWorkflow2.PK);

			AssertTagNotApplied("Tag is still not applied since it is a template workflow", templateWorkflow1, tagMag);
			AssertTagApplied("Tag is still applied since it is a template workflow", templateWorkflow2, tagMag);
		}

		#endregion

		#region Performance Test

		[TestDate(2016, 8, 22)]
		public void TestProcess_DBHits()
		{
			var userScopeDefinition = BMSTestHelper.CreateTagDefinition(Factory, "AAA");

			var exclusiveDefinition = BMSTestHelper.CreateTagDefinition(Factory, "BBB", isExclusive: true);
			var exclusiveMagnitude1 = BMSTestHelper.CreateTagMagnitude(exclusiveDefinition, "BAD", ruleRunSequence: 1);
			var exclusiveMagnitude2 = BMSTestHelper.CreateTagMagnitude(exclusiveDefinition, "SAD", ruleRunSequence: 2);

			var workQueue = BMSTestHelper.CreateWorkQueue(Factory, "NUU", "Quuu");

			var i = 0;

			var rules = new TagRuleActionTypeList().Cast<CodeDescriptionPair>().SelectMany(pair =>
			{
				var exclusiveTagRule = BMSTestHelper.CreateTagRule(exclusiveMagnitude1, "Add Bad" + ++i, pair.Code);
				var workQueueTagRule = BMSTestHelper.CreateTagRule(workQueue, "Add Q" + i, pair.Code);

				exclusiveTagRule.TGR_LastPerformanceVerificationDateTimeUtc = ZDateTime.UtcToday.AddDays(-1);
				workQueueTagRule.TGR_LastPerformanceVerificationDateTimeUtc = ZDateTime.UtcToday.AddDays(-1);

				FilterStripsTestHelper.AddFilterStrips(exclusiveTagRule.Filter,
				new FilterStripsTestHelper.FilterStripDefinition
				{
					FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
					FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Look out!",
				});

				FilterStripsTestHelper.AddFilterStrips(workQueueTagRule.Filter,
				new FilterStripsTestHelper.FilterStripDefinition
				{
					FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
					FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Look out!",
				});

				return new[] { exclusiveTagRule, workQueueTagRule };
			}).ToArray();

			var jobHeader = CreateJobHeader<OrgHeader>(false);
			var workflow = CreateWorkflow(jobHeader, "Look out!");
			workflow.AddTag(exclusiveMagnitude2);
			CreateWorkflow(jobHeader, "Look out!");
			CreateWorkflow(jobHeader, "Look out!");
			CreateWorkflow(jobHeader, "Look out!");

			Factory.Save();

			RowFactory.ResetCacheAfterDbUpgrade();

			var hits = new Dictionary<string, int>
			{
				{ BMNCNShapeSchema.Constants.TableName, 0 },
				{ ProcessHeaderSchema.Constants.TableName, 7 },
				{ ProcessHeaderLinkSchema.Constants.TableName, 8 },
				{ StmNoteSchema.Constants.TableName, 4 },
				{ StmUniversalCopySchema.Constants.TableName, 4 },
				{ TagDefinitionSchema.Constants.TableName, 2 },
				{ TagLinkSchema.Constants.TableName, 12 },
				{ StmDocDataOverrideSchema.Constants.TableName, 8 },
			};

			using (AssertDbHitsForAllFactories(hits, useOnlyNewFactories: true, ignoreUnspecified: true))
			{
				RunTagRules(rules);
			}
		}

		[TestDate(2016, 8, 22)]
		public void TestProcess_DBHits_EverythingLoadedInNewFactory()
		{
			var userScopeDefinition = BMSTestHelper.CreateTagDefinition(Factory, "AAA");

			var exclusiveDefinition = BMSTestHelper.CreateTagDefinition(Factory, "BBB", isExclusive: true);
			var exclusiveMagnitude1 = BMSTestHelper.CreateTagMagnitude(exclusiveDefinition, "BAD", ruleRunSequence: 1);
			var exclusiveMagnitude2 = BMSTestHelper.CreateTagMagnitude(exclusiveDefinition, "SAD", ruleRunSequence: 2);

			var workQueue = BMSTestHelper.CreateWorkQueue(Factory, "NUU", "Quuu");

			var i = 0;

			var rules = new TagRuleActionTypeList().Cast<CodeDescriptionPair>().SelectMany(pair =>
			{
				var exclusiveTagRule = BMSTestHelper.CreateTagRule(exclusiveMagnitude1, "Add Bad" + ++i, pair.Code);
				var workQueueTagRule = BMSTestHelper.CreateTagRule(workQueue, "Add Q" + i, pair.Code);

				exclusiveTagRule.TGR_LastPerformanceVerificationDateTimeUtc = ZDateTime.UtcToday.AddDays(-1);
				workQueueTagRule.TGR_LastPerformanceVerificationDateTimeUtc = ZDateTime.UtcToday.AddDays(-1);

				FilterStripsTestHelper.AddFilterStrips(exclusiveTagRule.Filter,
				new FilterStripsTestHelper.FilterStripDefinition
				{
					FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
					FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Look out!",
				});

				FilterStripsTestHelper.AddFilterStrips(workQueueTagRule.Filter,
				new FilterStripsTestHelper.FilterStripDefinition
				{
					FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
					FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Look out!",
				});

				return new[] { exclusiveTagRule, workQueueTagRule };
			}).ToArray();

			var jobHeader = CreateJobHeader<OrgHeader>(false);
			var workflow = CreateWorkflow(jobHeader, "Look out!");
			workflow.AddTag(exclusiveMagnitude2);
			CreateWorkflow(jobHeader, "Look out!");
			CreateWorkflow(jobHeader, "Look out!");
			CreateWorkflow(jobHeader, "Look out!");

			Factory.Save();

			RowFactory.ResetCacheAfterDbUpgrade();

			var hits = new Dictionary<string, int>
			{
				{ BMNCNShapeSchema.Constants.TableName, 0 },
				{ ProcessHeaderSchema.Constants.TableName, 7 },
				{ ProcessHeaderLinkSchema.Constants.TableName, 8 },
				{ StmDocDataOverrideSchema.Constants.TableName, 8 },
				{ StmModuleFilterSchema.Constants.TableName, 18 },
				{ StmModuleFilterUserDataSchema.Constants.TableName, 16 },
				{ StmNoteSchema.Constants.TableName, 4 },
				{ StmScheduleTaskSchema.Constants.TableName, 1 },
				{ StmUniversalCopySchema.Constants.TableName, 4 },
				{ TagDefinitionSchema.Constants.TableName, 4 },
				{ TagLinkSchema.Constants.TableName, 20 },
			};

			using (AssertDbHitsForAllFactories(hits, useOnlyNewFactories: true, ignoreUnspecified: true))
			{
				var logger = new BufferManagementLogger();
				var runner = new TagRuleRunner(logger);
				runner.Process();
			}
		}

		[StressTest]
		public void TestCustomSqlFilterStrip_ManyBatches_AddRemove()
		{
			BMSRegistry.Instance.TagRuleRunnerPrimarySecondaryServerTransferBatchSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1001);
			var tagDefinition = BMSTestHelper.CreateTagDefinition(Factory, "AAA");
			tagDefinition.TGD_UsageScope = TagUsageScopeList.Codes.Rule;
			var tagMagnitude = BMSTestHelper.CreateTagMagnitude(tagDefinition, "AAA");

			var rule = BMSTestHelper.CreateTagRule(tagMagnitude, "Add CC1", TagRuleActionTypeList.Codes.AddTag);
			FilterStripsTestHelper.AddFilterStrips(rule.Filter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Custom SQL Filter",
				FilterStripValueSetter = f => ((ModuleSQLFilter)f).Property1 = "FH_CompletionStatement like 'peekaboo%'",
			});

			var rule2 = BMSTestHelper.CreateTagRule(tagMagnitude, "Remove CC1", TagRuleActionTypeList.Codes.RemoveTag);
			FilterStripsTestHelper.AddFilterStrips(rule2.Filter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Custom SQL Filter",
				FilterStripValueSetter = f => ((ModuleSQLFilter)f).Property1 = "FH_CompletionStatement like 'peekaboo%'",
			});

			var workflowPks = CreatePerformanceWorkflows(1003);

			Factory.Save();

			RunTagRules(rule);

			var factoryToLoad = new BusinessObjectFactory();

			var workflow = new BusinessObjectFactory().Load<ProcessHeader>(workflowPks[0]);
			var lastWorkflowOfFirstBatch = new BusinessObjectFactory().Load<ProcessHeader>(workflowPks[1000]);
			var lastworkflow = new BusinessObjectFactory().Load<ProcessHeader>(workflowPks[1002]);
			AssertTagApplied("custom sql filter strip should have applied the tag", workflow, tagMagnitude);
			AssertTagApplied("custom sql filter strip should have applied the tag", lastWorkflowOfFirstBatch, tagMagnitude);
			AssertTagApplied("custom sql filter strip should have applied the tag", lastworkflow, tagMagnitude);

			RunTagRules(rule2);

			workflow = new BusinessObjectFactory().Load<ProcessHeader>(workflowPks[0]);
			lastWorkflowOfFirstBatch = new BusinessObjectFactory().Load<ProcessHeader>(workflowPks[1000]);
			lastworkflow = new BusinessObjectFactory().Load<ProcessHeader>(workflowPks[1002]);
			AssertTagNotApplied("custom sql filter strip shouldnt have applied the tag", workflow, tagMagnitude);
			AssertTagNotApplied("custom sql filter strip shouldnt have applied the tag", lastWorkflowOfFirstBatch, tagMagnitude);
			AssertTagNotApplied("custom sql filter strip shouldnt have applied the tag", lastworkflow, tagMagnitude);
		}

		List<ZGuid> CreatePerformanceWorkflows(int v)
		{
			var pks = new List<ZGuid>();

			for (int i = 0; i < v; i++)
			{
				var workflow = CreateJobHeader<SalesEnquiry>().ProcessHeaders[0];
				workflow.FH_CompletionStatement = "peekaboo" + i;
				pks.Add(workflow.PK);
			}

			return pks;
		}

		#endregion

		#region TagRuleRunner exception handling

		[TestDate(2015, 7, 14)]
		public void TestRun_ShouldNotReportTimeoutExceptionDeactivateRuleAndSendEmailWhenSufficientTimeoutExceptionsOccur()
		{
			BMSTestHelper.EnableBMSInRegistry();
			BMSRegistry.Instance.TagRulePerformanceCheckFrequency.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 2);

			var tagDef = BMSTestHelper.CreateTagDefinition(Factory, "CCC");
			var tagMag = BMSTestHelper.CreateTagMagnitude(tagDef, "CCC");

			var rule = BMSTestHelper.CreateTagRule(tagMag, "Maintain Magnitude CCC", TagRuleActionTypeList.Codes.MaintainMagnitude);
			FilterStripsTestHelper.AddFilterStrips(rule.Filter,
				new FilterStripsTestHelper.FilterStripDefinition
				{
					FilterStripName = "Custom SQL Filter",
					FilterStripValueSetter = f => ((ModuleSQLFilter)f).Property1 = "1 = 2",
				});
			rule.TGR_IsSystem = true;

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "staff@happy.com";
			var bmsNotificationGroup = Factory.NewWithValidTestData<GlbGroup>();
			staff.Groups.Add(bmsNotificationGroup);

			BMSRegistry.Instance.NotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, bmsNotificationGroup.PK.ToGuid());

			Factory.Save();

			Assert("Rule is active", rule.TGR_IsActive);

			var logger = new TagRuleRunnerTestLogger();
			var newFactory = Factory.CreateNewFactory();
			var runnerWhichGeneratesSqlTimeoutException = new DummyTagRuleRunnerThatThrowsExceptions<Exception>(logger, rule)
			{
				CreateExceptionFunc = () => SqlExceptionBuilder.CreateSqlException(3617, 51, 13, Db.ServerName, "The timeout period elapsed prior to completion of the operation or the server is not responding.", string.Empty, 1)
			};

			for (int i = 0; i < 6; ++i)
			{
				runnerWhichGeneratesSqlTimeoutException.Process();
			}

			var loadedRule = newFactory.Load<TagRule>(rule.PK);
			Assert("Exception caught and logged", logger.LogEntries.Any(log => log.Equals("Error occurred while processing system rule [Maintain Magnitude CCC]. Error message: The timeout period elapsed prior to completion of the operation or the server is not responding.")));
			Assert("Deactivation caught and logged", logger.LogEntries.Any(log => log.Equals("Repeated timeout exceptions occurred while processing system rule [Maintain Magnitude CCC]. Rule is now deactivated.")));
			Assert("Rule is now deactivated", !loadedRule.TGR_IsActive);
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);

			var sentEmail = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("System rule deactivated", sentEmail.Subject);
			AssertEquals("Error occurred while processing system rule [Maintain Magnitude CCC]. Error message: The timeout period elapsed prior to completion of the operation or the server is not responding.", sentEmail.Body);
			AssertNull("Should not report error", ErrorReporter.LastExceptionReported);
		}

		[TestDate(2015, 7, 14)]
		public void TestRun_ShouldNotReportTimeoutExceptionNotDeactivateRuleWhenInsufficientExceptionsOccur()
		{
			BMSTestHelper.EnableBMSInRegistry();
			BMSRegistry.Instance.TagRulePerformanceCheckFrequency.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 2);

			var tagDef = BMSTestHelper.CreateTagDefinition(Factory, "CCC");
			var tagMag = BMSTestHelper.CreateTagMagnitude(tagDef, "CCC");

			var rule = BMSTestHelper.CreateTagRule(tagMag, "Maintain Magnitude CCC", TagRuleActionTypeList.Codes.MaintainMagnitude);
			FilterStripsTestHelper.AddFilterStrips(rule.Filter,
				new FilterStripsTestHelper.FilterStripDefinition
				{
					FilterStripName = "Custom SQL Filter",
					FilterStripValueSetter = f => ((ModuleSQLFilter)f).Property1 = "1 = 2",
				});
			rule.TGR_IsSystem = true;

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "staff@happy.com";
			var bmsNotificationGroup = Factory.NewWithValidTestData<GlbGroup>();
			staff.Groups.Add(bmsNotificationGroup);

			BMSRegistry.Instance.NotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, bmsNotificationGroup.PK.ToGuid());

			Factory.Save();

			Assert("Rule is active", rule.TGR_IsActive);

			var logger = new TagRuleRunnerTestLogger();
			var newFactory = Factory.CreateNewFactory();
			var runnerWhichGeneratesSqlTimeoutException = new DummyTagRuleRunnerThatThrowsExceptions<Exception>(logger, rule)
			{
				CreateExceptionFunc = () => SqlExceptionBuilder.CreateSqlException(3617, 51, 13, Db.ServerName, "The timeout period elapsed prior to completion of the operation or the server is not responding.", string.Empty, 1)
			};

			for (int i = 0; i < 5; ++i)
			{
				runnerWhichGeneratesSqlTimeoutException.Process();

				var loadedRule = newFactory.Load<TagRule>(rule.PK);

				Assert("Exception caught and logged", logger.LogEntries.Any(log => log.Equals("Error occurred while processing system rule [Maintain Magnitude CCC]. Error message: The timeout period elapsed prior to completion of the operation or the server is not responding.")));
				Assert("Rule should still be active", loadedRule.TGR_IsActive);
				AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
			}
		}

		[TestDate(2015, 7, 14)]
		public void TestRun_ShouldNotReportPostLoginTimeoutExceptionAndNotDeactivateRuleAndNotSendEmail()
		{
			BMSTestHelper.EnableBMSInRegistry();
			BMSRegistry.Instance.TagRulePerformanceCheckFrequency.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 2);

			var tagDef = BMSTestHelper.CreateTagDefinition(Factory, "CCC");
			var tagMag = BMSTestHelper.CreateTagMagnitude(tagDef, "CCC");

			var rule = BMSTestHelper.CreateTagRule(tagMag, "Maintain Magnitude CCC", TagRuleActionTypeList.Codes.MaintainMagnitude);
			FilterStripsTestHelper.AddFilterStrips(rule.Filter,
				new FilterStripsTestHelper.FilterStripDefinition
				{
					FilterStripName = "Custom SQL Filter",
					FilterStripValueSetter = f => ((ModuleSQLFilter)f).Property1 = "1 = 2",
				});
			rule.TGR_IsSystem = true;

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "staff@happy.com";
			var bmsNotificationGroup = Factory.NewWithValidTestData<GlbGroup>();
			staff.Groups.Add(bmsNotificationGroup);

			BMSRegistry.Instance.NotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, bmsNotificationGroup.PK.ToGuid());

			Factory.Save();

			Assert("Rule is active", rule.TGR_IsActive);

			var logger = new TagRuleRunnerTestLogger();
			var newFactory = Factory.CreateNewFactory();
			var runnerWhichGeneratesSqlTimeoutException = new DummyTagRuleRunnerThatThrowsExceptions<Exception>(logger, rule)
			{
				CreateExceptionFunc = () => SqlExceptionBuilder.CreateSqlException(3617, 51, 13, Db.ServerName, "Connection Timeout Expired.  The timeout period elapsed during the post-login phase.  The connection could have timed out while waiting for server to complete the login process and respond; Or it could have timed out while attempting to create multiple active connections.", string.Empty, 1)
			};

			runnerWhichGeneratesSqlTimeoutException.Process();

			var loadedRule = newFactory.Load<TagRule>(rule.PK);

			Assert("Exception should have been caught and logged", logger.LogEntries.Any(log => log.Equals("Environmental error occurred while processing rule [Maintain Magnitude CCC]. Error message: Connection Timeout Expired.  The timeout period elapsed during the post-login phase.  The connection could have timed out while waiting for server to complete the login process and respond; Or it could have timed out while attempting to create multiple active connections.")));
			Assert("Rule should not be deactivated", loadedRule.TGR_IsActive);
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertNull("Should not report error", ErrorReporter.LastExceptionReported);
		}

		[TestDate(2015, 7, 14)]
		public void TestRun_ShouldIgnoreConcurrencyExceptions_ShouldLogThatTheyHappened()
		{
			BMSTestHelper.EnableBMSInRegistry();
			BMSRegistry.Instance.TagRulePerformanceCheckFrequency.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 2);

			var tagDef = BMSTestHelper.CreateTagDefinition(Factory, "CCC");
			var tagMag = BMSTestHelper.CreateTagMagnitude(tagDef, "CCC");

			var rule = BMSTestHelper.CreateTagRule(tagMag, "Maintain Magnitude CCC", TagRuleActionTypeList.Codes.MaintainMagnitude);

			FilterStripsTestHelper.AddFilterStrips(rule.Filter,
				new FilterStripsTestHelper.FilterStripDefinition
				{
					FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
					FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "This one!",
				});

			rule.TGR_LastPerformanceVerificationDateTimeUtc = ZDateTime.UtcNow.AddDays(-3);

			Factory.Save();

			var connectionProvider = SecondaryServerConnectionProviderProvider.GetProvider();

			var log = new TestServiceLogger();
			var dummyRunner = new DummyTagRuleRunner(log, rule);

			dummyRunner.GetStrategyCalled += (sender, e) =>
			{
				var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
				var loadedRule = newFactory.Load<TagRule>(((TagRule)sender).PK);

				loadedRule.TGR_Name = "Squanch";

				newFactory.Save();
			};

			dummyRunner.Strategy = new MaintainMagnitudeTagRuleRunStrategyForTest(connectionProvider, log);

			log.ClearLog();
			dummyRunner.Process();

			AssertStartsWith("The service task should log the concurrency error as a warning in the service task log, and NOT log the success of the assignment, since it failed when committing to the db.",
				$@"Information|Processing active rules allowed by schedule recurrence and frequency throttling. To run all active rules regardless of schedule and throttling, run the service task using the command 'TAG -configString:ALL' (without quotes).
Information|Verified performance of rule [Maintain Magnitude CCC], MAXDOP unchanged.
Information|Processed rule [Maintain Magnitude CCC], rows modified [0], branch [BNE], department [BRN]
Warning|Concurrency error: 
**CONCURRENCY Error Saving Record **

ServerName: {Db.ServerName}
DatabaseName: {Db.DatabaseName}
Tablename: TagRule
PK: {rule.PK}
RowState: Modified
Factory validation suspended: False
Factory name for debugging: 
Business object around row = Enterprise.BufferManagement.Business.TagRule
Business object validation suspended: 0
Business object is marking as needing validation suspended: False
Business object light validation is enabled: False
Business object additional info: 

Inner Message = ~ConcurrencyError~

Additional Information = 
--- Save Aborted Due to Concurrency Check ---

ROW INFORMATION
Table      = TagRule
PK         = {rule.PK}
RowState   = Modified", log.ToString());

			AssertEquals("The service task should not undo the assignment done by a human.", "Maintain Magnitude CCC", rule.TGR_Name);
		}

		public void TestTagRuleRunner_DeactivatesFailingRulesCorrectly()
		{
			BMSTestHelper.SetupEmailAndGroup(Factory);

			var tagDefinition = BMSTestHelper.CreateTagDefinition(Factory, "AAA");
			tagDefinition.TGD_UsageScope = TagUsageScopeList.Codes.Rule;
			var tagMagnitude = BMSTestHelper.CreateTagMagnitude(tagDefinition, "AAA");
			tagMagnitude.TGM_RuleRunSequence = 1;

			var failingRule = BMSTestHelper.CreateTagRule(tagMagnitude, "aaa", TagRuleActionTypeList.Codes.AddTag);
			FilterStripsTestHelper.AddFilterStrips(failingRule.Filter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Custom SQL Filter",
				FilterStripValueSetter = f => ((ModuleSQLFilter)f).Property1 = "FH_NonExistentColumn = 'WHY!?!?'",
			});

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			workflow.FH_CompletionStatement = "This one!";

			Factory.Save();

			var logger = new LoggerForTest();
			var runner = new DummyTagRuleRunnerThatIgnoresValidation(logger, failingRule);
			runner.Process();

			var newFactory = Factory.CreateNewFactory();
			var loadedRule = newFactory.Load<TagRule>(failingRule.PK);

			Assert("Exception caught and logged", logger.LogEntries.Any(x => x.StartsWith("Error occurred while processing rule [aaa]. Rule is now deactivated. Error message: Invalid column name 'FH_NonExistentColumn'.")));
			Assert("Rule is now deactivated", !loadedRule.TGR_IsActive);
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			var sentEmail = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Rule deactivated", sentEmail.Subject);
			AssertEquals("Error occurred while processing rule [aaa]. Rule is now deactivated. Error message: Invalid column name 'FH_NonExistentColumn'.", sentEmail.Body);

			AssertEquals(1, sentEmail.Recipients.Count);
			AssertEquals("frodo@bagend.com", sentEmail.Recipients[0].Email);

			ErrorReporter.Clear();
		}

		public void TestTagRuleRunner_DeactivatesSystemRules()
		{
			var tagDefinition = BMSTestHelper.CreateTagDefinition(Factory, "AAA");
			tagDefinition.TGD_UsageScope = TagUsageScopeList.Codes.Rule;
			var tagMagnitude = BMSTestHelper.CreateTagMagnitude(tagDefinition, "AAA");
			tagMagnitude.TGM_RuleRunSequence = 1;

			var failingRule = BMSTestHelper.CreateTagRule(tagMagnitude, "aaa", TagRuleActionTypeList.Codes.AddTag);
			failingRule.TGR_IsSystem = true;
			FilterStripsTestHelper.AddFilterStrips(failingRule.Filter,
				new FilterStripsTestHelper.FilterStripDefinition
				{
					FilterStripName = "Custom SQL Filter",
					FilterStripValueSetter = f => ((ModuleSQLFilter)f).Property1 = "FH_NonExistentColumn = 'WHY!?!?'",
				});

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			workflow.FH_CompletionStatement = "This one!";

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "frodo@bagend.com";

			var bmsNotificationGroup = Factory.NewWithValidTestData<GlbGroup>();
			staff.Groups.Add(bmsNotificationGroup);
			Factory.Save();

			BMSRegistry.Instance.NotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, bmsNotificationGroup.PK.ToGuid());

			var logger = new LoggerForTest();
			var runner = new DummyTagRuleRunnerThatIgnoresValidation(logger, failingRule);
			runner.Process();

			var newFactory = Factory.CreateNewFactory();
			var loadedRule = newFactory.Load<TagRule>(failingRule.PK);

			Assert("Rule is now deactivated", !loadedRule.TGR_IsActive);
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			var sentEmail = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("System rule deactivated", sentEmail.Subject);
			AssertEquals("Error occurred while processing system rule [aaa]. Rule is now deactivated. Error message: Invalid column name 'FH_NonExistentColumn'.", sentEmail.Body);

			AssertEquals(1, sentEmail.Recipients.Count);
			AssertEquals("frodo@bagend.com", sentEmail.Recipients[0].Email);

			AssertEquals("Error occurred while processing system rule [aaa]. Rule is now deactivated. Error message: Invalid column name 'FH_NonExistentColumn'.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestTagRuleRunner_HandlesDeveloperExceptionsCorrectly()
		{
			var tagDefinition = BMSTestHelper.CreateTagDefinition(Factory, "AAA");
			tagDefinition.TGD_UsageScope = TagUsageScopeList.Codes.Rule;

			var tagMagnitude = BMSTestHelper.CreateTagMagnitude(tagDefinition, "AAA");
			tagMagnitude.TGM_RuleRunSequence = 2;

			var goodRule = BMSTestHelper.CreateTagRule(tagMagnitude, "aaa", TagRuleActionTypeList.Codes.AddTag);
			FilterStripsTestHelper.AddFilterStrips(goodRule.Filter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "This one!",
			});

			Factory.Save();

			var logger = new LoggerForTest();
			var newFactory = Factory.CreateNewFactory();

			var runnerWhichGeneratesDeveloperException = new DummyTagRuleRunnerThatThrowsExceptions<NotImplementedException>(logger, goodRule);
			runnerWhichGeneratesDeveloperException.Process();

			var loadedRule = newFactory.Load<TagRule>(goodRule.PK);

			Assert("Exception caught and logged", logger.LogEntries.Any(x => x.StartsWith("Tag rule [aaa] failed to complete.")));
			Assert("Rule is still active", loadedRule.TGR_IsActive);
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);

			ErrorReporter.Clear();
		}

		public void TestTagRuleRunner_DoesNotDeactivateRulesOnEnvironmentalErrors()
		{
			BMSTestHelper.SetupEmailAndGroup(Factory);

			var queue = BMSTestHelper.CreateWorkQueue(Factory, "AAA", "aaa");

			var rule = BMSTestHelper.CreateTagRule(queue, queue.TGM_Description, TagRuleActionTypeList.Codes.AddAndRemoveTag);
			var stmFilter = rule.Filter;
			FilterStripsTestHelper.AddFilterStrips(stmFilter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "This one!",
			});

			Factory.Save();

			Assert("Rule is active", rule.TGR_IsActive);

			rule.TagTemplate.TGL_ParentTableCode = "FH";

			var logger = new TagRuleRunnerTestLogger();
			var newFactory = Factory.CreateNewFactory();

			var runnerWhichGeneratesSqlLockException = new DummyTagRuleRunnerThatThrowsExceptions<SqlLockLostException>(logger, rule);
			runnerWhichGeneratesSqlLockException.Process();

			var loadedRule = newFactory.Load<TagRule>(rule.PK);

			var log = logger.ExtendedLogEntries.Single(x => x.Item2.StartsWith("Environmental error occurred while processing rule [aaa]"));
			AssertEquals("Environmental errors should be logged as warnings, not errors", LogType.Warning, log.Item1);
			Assert("Rule is still active", loadedRule.TGR_IsActive);

			var runnerWhichGeneratesExecuteNonQueryEnvironmentalException = new DummyTagRuleRunnerThatThrowsExecuteNonQueryEnvironmentalExceptions(logger, rule);
			logger.ClearLogs();
			runnerWhichGeneratesExecuteNonQueryEnvironmentalException.Process();

			loadedRule = newFactory.Load<TagRule>(rule.PK);

			log = logger.ExtendedLogEntries.Single(x => x.Item2.StartsWith("Environmental error occurred while processing rule [aaa]"));
			AssertEquals("Environmental errors should be logged as warnings, not errors", LogType.Warning, log.Item1);
			Assert("Rule is still active", loadedRule.TGR_IsActive);

			ErrorReporter.Clear();
		}

		public void TestTagRuleRunner_DoesNotDeactivateRulesOnSIDOwnerErrors()
		{
			BMSTestHelper.SetupEmailAndGroup(Factory);

			var queue = BMSTestHelper.CreateWorkQueue(Factory, "AAA", "aaa");

			var rule = BMSTestHelper.CreateTagRule(queue, queue.TGM_Description, TagRuleActionTypeList.Codes.AddAndRemoveTag);
			var stmFilter = rule.Filter;
			FilterStripsTestHelper.AddFilterStrips(stmFilter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "This one!",
			});

			Factory.Save();

			Assert("Rule is active", rule.TGR_IsActive);

			rule.TagTemplate.TGL_ParentTableCode = "FH";

			var logger = new LoggerForTest();
			var newFactory = Factory.CreateNewFactory();

			var runnerWhichGeneratesSIDOwnerException = new DummyTagRuleRunnerThatThrowsSqlException(logger, 33009, rule);
			runnerWhichGeneratesSIDOwnerException.Process();

			var loadedRule = newFactory.Load<TagRule>(rule.PK);

			AssertEquals(true, logger.LogEntries.Any(x => x.Contains("Blah blah SID owner error blah blah")));
			AssertEquals(true, logger.LogEntries.Any(x => x.StartsWith("Environmental error occurred while processing rule [aaa]")));
			Assert("Rule is still active", loadedRule.TGR_IsActive);

			ErrorReporter.Clear();
		}

		public void TestSqlExceptionWithCustomSql_ShouldntSabotageSubsequentRules()
		{
			var tagDefinition = BMSTestHelper.CreateTagDefinition(Factory, "AAA");
			tagDefinition.TGD_UsageScope = TagUsageScopeList.Codes.Rule;
			var tagMagnitude = BMSTestHelper.CreateTagMagnitude(tagDefinition, "AAA");
			tagMagnitude.TGM_RuleRunSequence = 1;

			var tagMagnitude2 = BMSTestHelper.CreateTagMagnitude(tagDefinition, "BBB");
			tagMagnitude2.TGM_RuleRunSequence = 2;

			var failingRule = BMSTestHelper.CreateTagRule(tagMagnitude, "aaa", TagRuleActionTypeList.Codes.AddTag);
			FilterStripsTestHelper.AddFilterStrips(failingRule.Filter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Custom SQL Filter",
				FilterStripValueSetter = f => ((ModuleSQLFilter)f).Property1 = "FH_NonExistentColumn = 'WHY!?!?'",
			});

			var goodRule = BMSTestHelper.CreateTagRule(tagMagnitude2, "bbb", TagRuleActionTypeList.Codes.AddTag);
			FilterStripsTestHelper.AddFilterStrips(goodRule.Filter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "This one!",
			});

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			workflow.FH_CompletionStatement = "This one!";

			Factory.Save();

			var logger = new LoggerForTest();
			var runner = new DummyTagRuleRunnerThatIgnoresValidation(logger, failingRule, goodRule);
			runner.Process();

			AssertMultilineASCIIEquals("", @"Exception occurred when sending the following email:

Rule deactivated
Error occurred while processing rule [aaa]. Rule is now deactivated. Error message: Invalid column name 'FH_NonExistentColumn'.
", ErrorReporter.LastMessageReported);

			workflow = new BusinessObjectFactory().Load<ProcessHeader>(workflow.PK);
			AssertTagNotApplied("The tag rule should have failed and not applied the tag, and yet...", workflow, tagMagnitude);
			AssertTagApplied("The tag rule should have worked even after the custom sql failure of the previous rule, and yet...", workflow, tagMagnitude2);

			ErrorReporter.Clear();
		}

		public void TestSqlDeadlockError1205_ShouldNotDisableRule()
		{
			var rule = BMSTestHelper.CreateTagRuleWithDefAndMag(Factory);
			Factory.Save();

			var logger = new LoggerForTest();
			var runner = new DummyTagRuleRunnerThatThrowsSqlException(logger, 1205, rule);
			runner.Process();

			rule.Reload();
			AssertEquals(true, rule.TGR_IsActive);
			AssertEquals(true, logger.LogEntries.Any(x => x.Contains("Environmental error occurred while processing rule [The Tag Rule]")));
		}

		public void TestMaintainMagnitudeTagRuleRunStrategy_CanHandleTaskAggregateFilterStrips_AndNotDieHorribly()
		{
			var tagDefinition = BMSTestHelper.CreateTagDefinition(Factory, "AAA");
			tagDefinition.TGD_UsageScope = TagUsageScopeList.Codes.Rule;

			var tagMagnitude = BMSTestHelper.CreateTagMagnitude(tagDefinition, "AAA");
			tagMagnitude.TGM_RuleRunSequence = 2;

			var goodRule = BMSTestHelper.CreateTagRule(tagMagnitude, "aaa", TagRuleActionTypeList.Codes.MaintainMagnitude);
			FilterStripsTestHelper.AddFilterStrips(goodRule.Filter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = TaskStatusFilter.Schema.Identifier,
				FilterStripValueSetter = f =>
				{
					var taskFilter = (TaskStatusFilter)f;
					BMSTestHelper.SetValuesExclusive(taskFilter.TaskTypeCheckList, "UDF", "AAA");
					BMSTestHelper.SetValuesExclusive(taskFilter.TaskStatusCheckList, ProcessTaskStatusCodeList.Codes.Assigned, ProcessTaskStatusCodeList.Codes.Cancelled, ProcessTaskStatusCodeList.Codes.Closed);
					taskFilter.TaskAggregator = TaskStatusAggregatorList.Codes.All;
				},
			});

			ErrorReporter.Clear();
			Factory.Save();

			var logger = new LoggerForTest();
			var newFactory = Factory.CreateNewFactory();

			var runner = new DummyTagRuleRunner(logger, goodRule);
			runner.Process();

			var lastReport = ErrorReporter.LastMessageReported;

			AssertEquals("We had no reports happen, because the task aggregate filter is good and cool", string.Empty, lastReport);

			ErrorReporter.Clear();
		}

		class DummyTagRuleRunnerThatIgnoresValidation : DummyTagRuleRunner
		{
			public DummyTagRuleRunnerThatIgnoresValidation(ILogger logger, params TagRule[] tagRulesToRun_ForTest)
				: base(logger, tagRulesToRun_ForTest)
			{
			}

			protected override IEnumerable<TagRule> GetTagRulesToRun(TagRule[] tagRules)
			{
				return tagRules;
			}
		}

		class DummyTagRuleRunnerThatThrowsExceptions<E> : DummyTagRuleRunner where E : Exception, new()
		{
			internal Func<E> CreateExceptionFunc { get; set; }

			public DummyTagRuleRunnerThatThrowsExceptions(ILogger logger, params TagRule[] tagRulesToRun_ForTest)
				: base(logger, tagRulesToRun_ForTest)
			{
			}

			protected override TagRuleRunStrategyBase GetStrategy(TagRule rule, IConnectionProvider provider)
			{
				return new ExceptionStrategy<E>(CreateExceptionFunc);
			}

			protected override IEnumerable<TagRule> GetTagRulesToRun(TagRule[] tagRules)
			{
				return tagRules;
			}
		}

		class ExceptionStrategy<E> : TagRuleRunStrategyBase
			where E : Exception, new()
		{
			Func<E> CreateExceptionFunc { get; set; }

			public ExceptionStrategy(Func<E> createExceptionFunc)
				: base(null, null)
			{
				CreateExceptionFunc = createExceptionFunc;
			}

			public override ZQuery GetAffectedWorkflowsQuery(TagRule rule, bool asSubQuery = false)
			{
				return new ZQuery();
			}

			public override long Execute(TagRule rule)
			{
				throw CreateExceptionFunc?.Invoke() ?? new E();
			}
		}

		class DummyTagRuleRunnerThatThrowsExecuteNonQueryEnvironmentalExceptions : DummyTagRuleRunner
		{
			public DummyTagRuleRunnerThatThrowsExecuteNonQueryEnvironmentalExceptions(ILogger logger, params TagRule[] tagRulesToRun_ForTest)
				: base(logger, tagRulesToRun_ForTest)
			{
			}

			protected override TagRuleRunStrategyBase GetStrategy(TagRule rule, IConnectionProvider connectionProvider)
			{
				return new ExecuteNonQueryEnvironmentalExceptionStrategy();
			}

			protected override IEnumerable<TagRule> GetTagRulesToRun(TagRule[] tagRules)
			{
				return tagRules;
			}
		}

		class ExecuteNonQueryEnvironmentalExceptionStrategy : TagRuleRunStrategyBase
		{
			public ExecuteNonQueryEnvironmentalExceptionStrategy()
				: base(null, null)
			{
			}

			public override ZQuery GetAffectedWorkflowsQuery(TagRule rule, bool asSubQuery = false)
			{
				return new ZQuery();
			}

			public override long Execute(TagRule rule)
			{
				return ExecuteNonQuery();
			}

			public int ExecuteNonQuery() // the same signature as SqlCommand.ExecuteNonQuery()
			{
				throw new InvalidOperationException();
			}
		}

		class DummyTagRuleRunnerThatThrowsSqlException : DummyTagRuleRunner
		{
			readonly int sqlExceptionNumber;

			public DummyTagRuleRunnerThatThrowsSqlException(ILogger logger, int sqlExceptionNumber, params TagRule[] tagRulesToRun_ForTest)
				: base(logger, tagRulesToRun_ForTest)
			{
				this.sqlExceptionNumber = sqlExceptionNumber;
			}

			protected override TagRuleRunStrategyBase GetStrategy(TagRule rule, IConnectionProvider connectionProvider)
			{
				return new SqlExceptionThrowingStrategy(sqlExceptionNumber);
			}

			protected override IEnumerable<TagRule> GetTagRulesToRun(TagRule[] tagRules)
			{
				return tagRules;
			}
		}

		class SqlExceptionThrowingStrategy : TagRuleRunStrategyBase
		{
			readonly int sqlExceptionNumber;

			public SqlExceptionThrowingStrategy(int sqlExceptionNumber)
				: base(null, null)
			{
				this.sqlExceptionNumber = sqlExceptionNumber;
			}

			public override ZQuery GetAffectedWorkflowsQuery(TagRule rule, bool asSubQuery = false)
			{
				return new ZQuery();
			}

			public override long Execute(TagRule rule)
			{
				return ExecuteNonQuery();
			}

			public int ExecuteNonQuery() // the same signature as SqlCommand.ExecuteNonQuery()
			{
				var sqlError = SqlExceptionBuilder.CreateSqlError(sqlExceptionNumber, 0, 0, "server", "Blah blah SID owner error blah blah", "DoSomething", 123);
				throw SqlExceptionBuilder.CreateSqlException(SqlExceptionBuilder.CreateSqlErrorCollection(sqlError));
			}
		}

		#endregion

		#region Frequency Throttling

		[TestDate(2015, 3, 3)]
		public void TestTagRules_ShouldObeyThrottling()
		{
			var tagDefinition = BMSTestHelper.CreateTagDefinition(Factory, "AAA");
			tagDefinition.TGD_UsageScope = TagUsageScopeList.Codes.Rule;
			var tagMagnitude = BMSTestHelper.CreateTagMagnitude(tagDefinition, "AAA");
			var rule = BMSTestHelper.CreateTagRule(tagMagnitude, "Add CC1", TagRuleActionTypeList.Codes.AddTag);
			FilterStripsTestHelper.AddFilterStrips(rule.Filter,
				new FilterStripsTestHelper.FilterStripDefinition
				{
					FilterStripName = "Custom SQL Filter",
					FilterStripValueSetter = f => ((ModuleSQLFilter)f).Property1 = "FH_CompletionStatement = 'peekaboo'",
				});

			var logger = new BufferManagementLogger();
			var runner = new DummyTagRuleRunner(logger, rule);
			runner.Strategy = new DummyTagRuleStrategyWithSettableDuration(70);

			runner.Process();

			AssertContains("The rule hasn't been run with throttling enabled yet, so it should have run, and yet...", "Processed rule [Add CC1]", logger.ToString());
			AssertEquals(70, rule.TGR_LastRunDurationInSeconds);
			AssertEquals(new ZDateTime(2015, 3, 3), rule.TGR_LastRunStartTimeUtc);

			TestDateAttribute.Date = new DateTime(2015, 3, 3, 0, 5, 0);
			logger = new BufferManagementLogger();
			runner = new DummyTagRuleRunner(logger, rule);
			runner.Strategy = new DummyTagRuleStrategyWithSettableDuration(75);
			runner.Process();

			AssertNotContains("The required 30 minutes (for a rule that runs for more than 60 seconds) hasn't passed, so the rule should not have run, and yet...", "Processed rule [Add CC1]", logger.ToString(), ignoreCase: true);
			AssertEquals(70, rule.TGR_LastRunDurationInSeconds);
			AssertEquals(new ZDateTime(2015, 3, 3, 0, 0, 0), rule.TGR_LastRunStartTimeUtc);

			TestDateAttribute.Date = new DateTime(2015, 3, 3, 0, 30, 0);
			logger = new BufferManagementLogger();
			runner = new DummyTagRuleRunner(logger, rule);
			runner.Strategy = new DummyTagRuleStrategyWithSettableDuration(80);
			runner.Process();

			AssertContains("The required 30 minutes has passed, so the rule should have run, and yet...", "Processed rule [Add CC1]", logger.ToString());
			AssertEquals(80, rule.TGR_LastRunDurationInSeconds);
			AssertEquals(new ZDateTime(2015, 3, 3, 0, 30, 0), rule.TGR_LastRunStartTimeUtc);
		}

		[TestDate(2020, 02, 17, 11, 00, 00)]
		public void TestTagRulePerformanceData_ShouldNotRequireZDateTimeToProgress()
		{
			var tagDefinition = BMSTestHelper.CreateTagDefinition(Factory, "AAA");
			tagDefinition.TGD_UsageScope = TagUsageScopeList.Codes.Rule;
			var tagMagnitude = BMSTestHelper.CreateTagMagnitude(tagDefinition, "AAA");
			var rule = BMSTestHelper.CreateTagRule(tagMagnitude, "Add CC1", TagRuleActionTypeList.Codes.AddTag);
			FilterStripsTestHelper.AddFilterStrips(rule.Filter,
				new FilterStripsTestHelper.FilterStripDefinition
				{
					FilterStripName = "Custom SQL Filter",
					FilterStripValueSetter = f => ((ModuleSQLFilter)f).Property1 = "FH_CompletionStatement = 'peekaboo'",
				});

			var useless = ZDateTime.UtcNow;
			var logger = new BufferManagementLogger();
			var runner = new DummyTagRuleRunner(logger, rule);
			var strategy = new DummyTagRuleStrategyWithSettableExecuteAction(() =>
			{
				useless = ZDateTime.UtcNow.AddMinutes(10);
				TestDateAttribute.AddMinutes(10);
			},
			10);

			runner.Strategy = strategy;
			runner.Process();

			AssertContains("The rule hasn't been run with throttling enabled yet, so it should have run, and yet...", "Processed rule [Add CC1]", logger.ToString());
			AssertNotEquals("We want a value that's incremented by the stopwatch, not by the 10 minute ZDateTime increment we just did", 11 * 60, rule.TGR_LastRunDurationInSeconds);
			AssertEquals(new ZDateTime(2020, 02, 17, 11, 00, 00), rule.TGR_LastRunStartTimeUtc);
		}

		class TagRuleRunnerTestLogger : ILogger
		{
			public TagRuleRunnerTestLogger()
			{
				ClearLogs();
			}

			public List<Tuple<LogType, string, Exception>> ExtendedLogEntries;
			public int LogCount => ExtendedLogEntries.Count;
			public IEnumerable<string> LogEntries => ExtendedLogEntries.Select(l => l.Item2);
			public bool HasErrors => ExtendedLogEntries.Any(l => l.Item1 == LogType.Error);

			public void Log(LogType type, string message)
			{
				ExtendedLogEntries.Add(new Tuple<LogType, string, Exception>(type, message, null));
			}

			public void Log(LogType type, string message, Exception ex)
			{
				ExtendedLogEntries.Add(new Tuple<LogType, string, Exception>(type, message, ex));
			}

			public void ClearLogs()
			{
				ExtendedLogEntries = new List<Tuple<LogType, string, Exception>>();
			}
		}

		#endregion

		#region GetAffectedWorkflowsQueries

		public void TestAddTagRule_GetQuery()
		{
			var tagDef = BMSTestHelper.CreateTagDefinition(Factory, "CCC");
			var tagMag = BMSTestHelper.CreateTagMagnitude(tagDef, "CCC");

			var rule = BMSTestHelper.CreateTagRule(tagMag, "Add CCC", TagRuleActionTypeList.Codes.AddTag);
			FilterStripsTestHelper.AddFilterStrips(rule.Filter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "This one!",
			});

			var workflow1 = CreateJobHeader<OrgHeader>().ProcessHeaders.AddNew();
			var workflow2 = CreateJobHeader<OrgHeader>().ProcessHeaders.AddNew();
			workflow1.FH_CompletionStatement = "This one!";
			workflow2.FH_CompletionStatement = "Not this one?";

			Factory.Save();

			var provider = TagRuleRunStrategyProvider.GetStrategy(rule, null, null, new DummyLogger());
			var matchingWorkflows = new BusinessObjectFactory().Load<ProcessHeader>(provider.GetAffectedWorkflowsQuery(rule));

			AssertEquals(1, matchingWorkflows.Length);
			AssertEquals(workflow1.PK, matchingWorkflows[0].PK);
		}

		public void TestAddTagRuleWorkQueue_GetQuery()
		{
			var tagMag = BMSTestHelper.CreateWorkQueue(Factory, "CCC", "ccc");

			var rule = BMSTestHelper.CreateTagRule(tagMag, "Add CCC", TagRuleActionTypeList.Codes.AddTag);
			FilterStripsTestHelper.AddFilterStrips(rule.Filter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "This one!",
			});

			var workflow1 = CreateJobHeader<OrgHeader>().ProcessHeaders.AddNew();
			var workflow2 = CreateJobHeader<OrgHeader>().ProcessHeaders.AddNew();
			workflow1.FH_CompletionStatement = "This one!";
			workflow2.FH_CompletionStatement = "Not this one?";

			Factory.Save();

			var provider = TagRuleRunStrategyProvider.GetStrategy(rule, null, null, new DummyLogger());
			var matchingWorkflows = new BusinessObjectFactory().Load<ProcessHeader>(provider.GetAffectedWorkflowsQuery(rule));

			AssertEquals(1, matchingWorkflows.Length);
			AssertEquals(workflow1.PK, matchingWorkflows[0].PK);
		}

		public void TestRemoveTagRule_GetQuery()
		{
			var tagDef = BMSTestHelper.CreateTagDefinition(Factory, "CCC");
			var tagMag = BMSTestHelper.CreateTagMagnitude(tagDef, "CCC");

			var rule = BMSTestHelper.CreateTagRule(tagMag, "Remove CCC", TagRuleActionTypeList.Codes.RemoveTag);
			FilterStripsTestHelper.AddFilterStrips(rule.Filter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "This one!",
			});

			var workflow1 = CreateJobHeader<OrgHeader>().ProcessHeaders.AddNew();
			var workflow2 = CreateJobHeader<OrgHeader>().ProcessHeaders.AddNew();
			var workflow3 = CreateJobHeader<OrgHeader>().ProcessHeaders.AddNew();
			var workflow4 = CreateJobHeader<OrgHeader>().ProcessHeaders.AddNew();
			workflow1.FH_CompletionStatement = "This one!"; // matches FH_CompletionStatement and has tag
			workflow1.AddTag(tagMag);

			workflow2.FH_CompletionStatement = "Not this one?";

			workflow3.FH_CompletionStatement = "This one!";

			workflow4.FH_CompletionStatement = "Not this one?";
			workflow4.AddTag(tagMag);

			Factory.Save();

			var provider = TagRuleRunStrategyProvider.GetStrategy(rule, null, null, new DummyLogger());
			var matchingWorkflows = new BusinessObjectFactory().Load<ProcessHeader>(provider.GetAffectedWorkflowsQuery(rule));

			AssertEquals(1, matchingWorkflows.Length);
			AssertEquals(workflow1.PK, matchingWorkflows[0].PK);
		}

		public void TestRemoveTagRuleWorkQueue_GetQuery()
		{
			var tagMag = BMSTestHelper.CreateWorkQueue(Factory, "CCC", "ccc");

			var rule = BMSTestHelper.CreateTagRule(tagMag, "Remove CCC", TagRuleActionTypeList.Codes.RemoveTag);
			FilterStripsTestHelper.AddFilterStrips(rule.Filter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "This one!",
			});

			var workflow1 = CreateJobHeader<OrgHeader>().ProcessHeaders.AddNew();
			var workflow2 = CreateJobHeader<OrgHeader>().ProcessHeaders.AddNew();
			var workflow3 = CreateJobHeader<OrgHeader>().ProcessHeaders.AddNew();
			var workflow4 = CreateJobHeader<OrgHeader>().ProcessHeaders.AddNew();
			workflow1.FH_CompletionStatement = "This one!"; // matches FH_CompletionStatement and has tag
			workflow1.AddTag(tagMag);

			workflow2.FH_CompletionStatement = "Not this one?";

			workflow3.FH_CompletionStatement = "This one!";

			workflow4.FH_CompletionStatement = "Not this one?";
			workflow4.AddTag(tagMag);

			Factory.Save();

			var provider = TagRuleRunStrategyProvider.GetStrategy(rule, null, null, new DummyLogger());
			var matchingWorkflows = new BusinessObjectFactory().Load<ProcessHeader>(provider.GetAffectedWorkflowsQuery(rule));

			AssertEquals(1, matchingWorkflows.Length);
			AssertEquals(workflow1.PK, matchingWorkflows[0].PK);
		}

		public void TestAddRemoveTagRule_GetQuery()
		{
			var tagDef = BMSTestHelper.CreateTagDefinition(Factory, "CCC");
			var tagMag = BMSTestHelper.CreateTagMagnitude(tagDef, "CCC");

			var rule = BMSTestHelper.CreateTagRule(tagMag, "Add and Remove CCC", TagRuleActionTypeList.Codes.AddAndRemoveTag);
			FilterStripsTestHelper.AddFilterStrips(rule.Filter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "This one!",
			});

			var workflow1 = CreateJobHeader<OrgHeader>().ProcessHeaders.AddNew();
			var workflow2 = CreateJobHeader<OrgHeader>().ProcessHeaders.AddNew();
			workflow1.FH_CompletionStatement = "This one!";
			workflow2.FH_CompletionStatement = "Not this one?";
			workflow2.AddTag(tagMag);

			Factory.Save();

			var provider = TagRuleRunStrategyProvider.GetStrategy(rule, null, null, new DummyLogger());
			var matchingWorkflows = new BusinessObjectFactory().Load<ProcessHeader>(provider.GetAffectedWorkflowsQuery(rule));

			AssertEquals(2, matchingWorkflows.Length);
			AssertCollectionContains(matchingWorkflows, w => w.PK == workflow1.PK || w.PK == workflow2.PK);
		}

		public void TestAddRemoveTagRuleWorkQueue_GetQuery()
		{
			var tagMag = BMSTestHelper.CreateWorkQueue(Factory, "CCC", "ccc");

			var rule = BMSTestHelper.CreateTagRule(tagMag, "Add and Remove CCC", TagRuleActionTypeList.Codes.AddAndRemoveTag);
			FilterStripsTestHelper.AddFilterStrips(rule.Filter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "This one!",
			});

			var workflow1 = CreateJobHeader<OrgHeader>().ProcessHeaders.AddNew();
			var workflow2 = CreateJobHeader<OrgHeader>().ProcessHeaders.AddNew();
			workflow1.FH_CompletionStatement = "This one!";
			workflow2.FH_CompletionStatement = "Not this one?";
			workflow2.AddTag(tagMag);

			Factory.Save();

			var provider = TagRuleRunStrategyProvider.GetStrategy(rule, null, null, new DummyLogger());
			var matchingWorkflows = new BusinessObjectFactory().Load<ProcessHeader>(provider.GetAffectedWorkflowsQuery(rule));

			AssertEquals(2, matchingWorkflows.Length);
			AssertCollectionContains(matchingWorkflows, w => w.PK == workflow1.PK || w.PK == workflow2.PK);
		}

		public void TestMaintainMagnitudeTagRule_GetQuery()
		{
			var tagDef = BMSTestHelper.CreateTagDefinition(Factory, "CCC");
			var tagMag = BMSTestHelper.CreateTagMagnitude(tagDef, "CCC");

			var rule = BMSTestHelper.CreateTagRule(tagMag, "Maintain Magnitude CCC", TagRuleActionTypeList.Codes.MaintainMagnitude);
			rule.TagTemplate.TGL_Magnitude = 7m;

			FilterStripsTestHelper.AddFilterStrips(rule.Filter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "This one!",
			});

			var workflow1 = CreateJobHeader<OrgHeader>().ProcessHeaders.AddNew();
			var workflow2 = CreateJobHeader<OrgHeader>().ProcessHeaders.AddNew();
			var workflow3 = CreateJobHeader<OrgHeader>().ProcessHeaders.AddNew();
			var workflow4 = CreateJobHeader<OrgHeader>().ProcessHeaders.AddNew();

			workflow1.FH_CompletionStatement = "This one!";
			workflow1.AddTag(tagMag).Link.TGL_Magnitude = 5m;

			workflow2.FH_CompletionStatement = "Not this one?";
			workflow2.AddTag(tagMag).Link.TGL_Magnitude = 7m;

			workflow3.FH_CompletionStatement = "This one!";
			workflow3.AddTag(tagMag).Link.TGL_Magnitude = 7m;

			workflow4.FH_CompletionStatement = "This one!";

			Factory.Save();

			var provider = TagRuleRunStrategyProvider.GetStrategy(rule, null, null, new DummyLogger());
			var query = provider.GetAffectedWorkflowsQuery(rule);
			query.IncludeBlob(ProcessHeaderSchema.FH_CompletionStatement);

			var matchingWorkflows = new BusinessObjectFactory().Load<ProcessHeader>(query);

			AssertEquals(1, matchingWorkflows.Length);
			AssertEquals(workflow1.PK, matchingWorkflows[0].PK);
		}

		public void TestMaintainMagnitudeTagRule_GetQueryIsParameterized()
		{
			var tagDef = BMSTestHelper.CreateTagDefinition(Factory, "CCC");
			var tagMag = BMSTestHelper.CreateTagMagnitude(tagDef, "CCC");

			var rule = BMSTestHelper.CreateTagRule(tagMag, "Maintain Magnitude CCC", TagRuleActionTypeList.Codes.MaintainMagnitude);
			rule.TagTemplate.TGL_Magnitude = 7m;

			FilterStripsTestHelper.AddFilterStrips(rule.Filter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "This one!",
			});

			var workflow1 = CreateJobHeader<OrgHeader>().ProcessHeaders.AddNew();
			var workflow2 = CreateJobHeader<OrgHeader>().ProcessHeaders.AddNew();
			var workflow3 = CreateJobHeader<OrgHeader>().ProcessHeaders.AddNew();
			var workflow4 = CreateJobHeader<OrgHeader>().ProcessHeaders.AddNew();

			workflow1.FH_CompletionStatement = "This one!";
			workflow1.AddTag(tagMag).Link.TGL_Magnitude = 5m;

			workflow2.FH_CompletionStatement = "Not this one?";
			workflow2.AddTag(tagMag).Link.TGL_Magnitude = 7m;

			workflow3.FH_CompletionStatement = "This one!";
			workflow3.AddTag(tagMag).Link.TGL_Magnitude = 7m;

			workflow4.FH_CompletionStatement = "This one!";

			Factory.Save();

			var strategy = TagRuleRunStrategyProvider.GetStrategy(rule, null, null, new DummyLogger());
			var query = strategy.GetAffectedWorkflowsQuery(rule);
			query.IncludeBlob(ProcessHeaderSchema.FH_CompletionStatement);

			AssertNotNull("The query must have a parameter with the correct declared value - FilterStrip.Property with comparison operator 'startswith'",
									query.ParameterisedText.Parameters.SingleOrDefault(p => p.ValueForSql is string strValue && strValue == "This one!%"));

			var matchingWorkflows = new BusinessObjectFactory().Load<ProcessHeader>(query);

			AssertEquals(1, matchingWorkflows.Length);
			AssertEquals(workflow1.PK, matchingWorkflows[0].PK);
		}

		public void TestRuleSQL_ShouldBeParameterised_AddRule()
		{
			BMSRegistry.Instance.TagRuleRunnerPrimarySecondaryServerTransferBatchSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);
			var tagDefinition = BMSTestHelper.CreateTagDefinition(Factory, "AAA");
			var tagMagnitude = BMSTestHelper.CreateTagMagnitude(tagDefinition, "AAA");

			var rule = BMSTestHelper.CreateTagRule(tagMagnitude, "Add CC1", TagRuleActionTypeList.Codes.AddTag);
			FilterStripsTestHelper.AddFilterStrips(rule.Filter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Custom SQL Filter",
				FilterStripValueSetter = f => ((ModuleSQLFilter)f).Property1 = "FH_CompletionStatement like 'peekaboo%'",
			});

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			workflow.FH_CompletionStatement = "peekaboo1";

			Factory.Save();

			var runner = new DummyTagRuleRunner(false, rule);

			using (TestConnection.TrackExecutedCommands())
			{
				runner.Process();

				AssertNotNull("We expect a query that contains some fancy specific code to have been run, but instead we got bupkis!", TestConnection.ExecutedCommands);

				List<string> commands = new List<string>();
				foreach (string query in TestConnection.ExecutedCommands)
				{
					if (query.Contains("RULE NAME"))
					{
						commands.Add(query);
					}
				}

				Assert("We should have some number of queries here, and yet...", !commands.IsNullOrEmpty());

				foreach (var command in commands)
				{
					AssertDoesNotContainInlinedPKsInSQL(command);

					if (command.Contains(TagRuleRunStrategyBaseConstants.TemplateNames.AddTagRuleSQL))
					{
						AssertContains("The query should be parameterised, and yet...", TagRuleRunStrategyBaseConstants.ParameterNames.TagLinkMagnitudeValue, command);
						AssertContains("The query should be parameterised, and yet...", TagRuleRunStrategyBaseConstants.ParameterNames.TagRulesNow, command);
						AssertContains("The query should be parameterised, and yet...", TagRuleRunStrategyBaseConstants.ParameterNames.TagRulesUtcNow, command);
						AssertContains("The query should be parameterised, and yet...", TagRuleRunStrategyBaseConstants.ParameterNames.TagRulePK, command);
						AssertContains("The query should be parameterised, and yet...", TagRuleRunStrategyBaseConstants.ParameterNames.TagMagnitudePK, command);
					}
					else
					{
						AssertContains("The query should be parameterised, and yet...", TagRuleRunStrategyBaseConstants.TemplateNames.AddTagRuleWorkflowSelectQuery, command);
					}
				}
			}
		}

		public void TestRuleSQL_ShouldProcessBatches_UsingParameterisedRowNumbers()
		{
			BMSRegistry.Instance.TagRuleRunnerPrimarySecondaryServerTransferBatchSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);
			var tagDefinition = BMSTestHelper.CreateTagDefinition(Factory, "AAA");
			var tagMagnitude = BMSTestHelper.CreateTagMagnitude(tagDefinition, "AAA");

			var rule = BMSTestHelper.CreateTagRule(tagMagnitude, "Add CC1", TagRuleActionTypeList.Codes.AddTag);
			FilterStripsTestHelper.AddFilterStrips(rule.Filter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Custom SQL Filter",
				FilterStripValueSetter = f => ((ModuleSQLFilter)f).Property1 = "FH_CompletionStatement like 'peekaboo%'",
			});

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			workflow.FH_CompletionStatement = "peekaboo1";

			Factory.Save();

			var runner = new DummyTagRuleRunner(false, rule);

			using (TestConnection.TrackExecutedCommands())
			{
				runner.Process();

				AssertNotNull("We expect a query that contains some fancy specific code to have been run, but instead we got bupkis!", TestConnection.ExecutedCommands);

				List<string> commands = new List<string>();
				foreach (string query in TestConnection.ExecutedCommands)
				{
					if (query.Contains("FROM #WorkflowsToTagTempTable WHERE"))
					{
						commands.Add(query);
					}
				}

				Assert("We should have some number of queries here, and yet...", !commands.IsNullOrEmpty());

				foreach (var command in commands)
				{
					AssertDoesNotContainInlinedPKsInSQL(command);

					AssertContains("The query should be parameterised, and yet...", TagRuleRunStrategyBaseConstants.ParameterNames.BatchLowNumber, command);
					AssertContains("The query should be parameterised, and yet...", TagRuleRunStrategyBaseConstants.ParameterNames.BatchHighNumber, command);
				}
			}
		}

		public void TestRuleSQL_ShouldUpdateInPKOrder_AddTagRuleSQL()
		{
			BMSRegistry.Instance.TagRuleRunnerPrimarySecondaryServerTransferBatchSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);
			var tagDefinition = BMSTestHelper.CreateTagDefinition(Factory, "AAA");
			var tagMagnitude = BMSTestHelper.CreateTagMagnitude(tagDefinition, "AAA");

			var rule = BMSTestHelper.CreateTagRule(tagMagnitude, "Add CC1", TagRuleActionTypeList.Codes.AddTag);
			FilterStripsTestHelper.AddFilterStrips(rule.Filter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Custom SQL Filter",
				FilterStripValueSetter = f => ((ModuleSQLFilter)f).Property1 = "FH_CompletionStatement like 'peekaboo%'",
			});

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			workflow.FH_CompletionStatement = "peekaboo1";

			Factory.Save();

			var runner = new DummyTagRuleRunner(false, rule);
			using (TestConnection.TrackExecutedCommands())
			{
				runner.Process();

				var query = TestConnection.ExecutedCommands.SingleOrDefault(q => q.Contains(TagRuleRunStrategyBaseConstants.TemplateNames.AddTagRuleSQL));
				CombineAssertions(() =>
				{
					AssertNotNull("We expect a query that contains some fancy specific code to have been run, but instead we got bupkis!", query);
					AssertContains("The query should use the new method to update FH_SystemLastEditTimeUtc.", "@UpdatedPKs", query);
					AssertContains("The query should use the new method to update FH_SystemLastEditTimeUtc.", "INDEX([PK_UX__FH_PK])", query);
				});
			}
		}

		public void TestRuleSQL_ShouldBeParameterised_MaintainMagnitudeRule()
		{
			BMSRegistry.Instance.TagRulePerformanceCheckFrequency.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 2);

			var tagDef = BMSTestHelper.CreateTagDefinition(Factory, "CCC");
			var tagMag = BMSTestHelper.CreateTagMagnitude(tagDef, "CCC");

			var rule = BMSTestHelper.CreateTagRule(tagMag, "Maintain Magnitude CCC", TagRuleActionTypeList.Codes.MaintainMagnitude);
			rule.TagTemplate.TGL_Magnitude = 7m;

			FilterStripsTestHelper.AddFilterStrips(rule.Filter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "This one!",
			});

			var workflow1 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "This one!");
			var workflow2 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Not this one?");
			var workflow3 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow3");
			var workflow4 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow4");

			workflow1.AddTag(tagMag).Link.TGL_Magnitude = 5m;
			workflow2.AddTag(tagMag).Link.TGL_Magnitude = 7m;

			Factory.Save();

			var connectionProvider = SecondaryServerConnectionProviderProvider.GetProvider();

			var logger = new TestServiceLogger();
			var dummyRunner = new DummyTagRuleRunner(logger, rule);
			dummyRunner.Strategy = new MaintainMagnitudeTagRuleRunStrategyForTest(connectionProvider, logger);

			using (TestConnection.TrackExecutedCommands())
			{
				dummyRunner.Process();

				AssertNotNull("We expect a query that contains some fancy specific code to have been run, but instead we got bupkis!", TestConnection.ExecutedCommands);

				List<string> commands = new List<string>();
				foreach (string query in TestConnection.ExecutedCommands)
				{
					if (query.Contains("RULE NAME"))
					{
						commands.Add(query);
					}
				}

				foreach (var command in commands)
				{
					AssertDoesNotContainInlinedPKsInSQL(command);

					if (command.Contains(TagRuleRunStrategyBaseConstants.TemplateNames.SelectAffectedProcessHeadersSql))
					{
						AssertContains("The query should be parameterised, and yet...", TagRuleRunStrategyBaseConstants.ParameterNames.TagLinkMagnitudeValue, command);
						AssertContains("The query should be parameterised, and yet...", TagRuleRunStrategyBaseConstants.ParameterNames.TagMagnitudePK, command);
					}
					else if (command.Contains(TagRuleRunStrategyBaseConstants.TemplateNames.UpdateMagnitudeHeaderSql))
					{
						AssertContains("The query should be parameterised, and yet...", TagRuleRunStrategyBaseConstants.ParameterNames.TagLinkMagnitudeValue, command);
						AssertContains("The query should be parameterised, and yet...", TagRuleRunStrategyBaseConstants.ParameterNames.TagMagnitudePK, command);
					}
					else if (command.Contains(TagRuleRunStrategyBaseConstants.TemplateNames.UpdateMagnitudeSql))
					{
						AssertContains("The query should be parameterised, and yet...", TagRuleRunStrategyBaseConstants.ParameterNames.TagLinkMagnitudeValue, command);
						AssertContains("The query should be parameterised, and yet...", TagRuleRunStrategyBaseConstants.ParameterNames.TagRulesUtcNow, command);
					}
					else
					{
						Assert("If the test fails here, we're executing queries that do not involve any of our preset template names. Sad.", false);
					}
				}
			}
		}

		public void TestRuleSQL_ShouldUpdateInPKOrder_UpdateMagnitudeSql()
		{
			BMSRegistry.Instance.TagRulePerformanceCheckFrequency.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 2);

			var tagDef = BMSTestHelper.CreateTagDefinition(Factory, "CCC");
			var tagMag = BMSTestHelper.CreateTagMagnitude(tagDef, "CCC");

			var rule = BMSTestHelper.CreateTagRule(tagMag, "Maintain Magnitude CCC", TagRuleActionTypeList.Codes.MaintainMagnitude);
			rule.TagTemplate.TGL_Magnitude = 7m;

			FilterStripsTestHelper.AddFilterStrips(rule.Filter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "This one!",
			});

			var workflow1 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "This one!");

			workflow1.AddTag(tagMag).Link.TGL_Magnitude = 5m;

			Factory.Save();

			var runner = new DummyTagRuleRunner(false, rule);

			using (TestConnection.TrackExecutedCommands())
			{
				runner.Process();
				var query = TestConnection.ExecutedCommands.FirstOrDefault(q => q.Contains(TagRuleRunStrategyBaseConstants.TemplateNames.UpdateMagnitudeSql));
				CombineAssertions(() =>
				{
					AssertNotNull("We expect a query that contains some fancy specific code to have been run, but instead we got bupkis!", query);
					AssertContains("The query should use the new method to update FH_SystemLastEditTimeUtc.", "@UpdatedPKs", query);
					AssertContains("The query should use the new method to update FH_SystemLastEditTimeUtc.", "INDEX([PK_UX__FH_PK])", query);
				});
			}
		}

		public void TestRuleSQL_ShouldBeParameterised_RemoveRule()
		{
			BMSRegistry.Instance.TagRuleRunnerPrimarySecondaryServerTransferBatchSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);
			var tagDefinition = BMSTestHelper.CreateTagDefinition(Factory, "AAA");
			tagDefinition.TGD_UsageScope = TagUsageScopeList.Codes.All;
			var tagMagnitude = BMSTestHelper.CreateTagMagnitude(tagDefinition, "AAA");

			var rule = BMSTestHelper.CreateTagRule(tagMagnitude, "Add CC1", TagRuleActionTypeList.Codes.RemoveTag);
			FilterStripsTestHelper.AddFilterStrips(rule.Filter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Custom SQL Filter",
				FilterStripValueSetter = f => ((ModuleSQLFilter)f).Property1 = "FH_CompletionStatement like 'peekaboo%'",
			});

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			workflow.FH_CompletionStatement = "peekaboo1";
			workflow.AddTag(tagMagnitude);

			var workflow2 = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			workflow2.FH_CompletionStatement = "peekaboo1";
			workflow2.AddTag(tagMagnitude);

			AssertTagApplied("custom sql filter strip should have applied the tag", workflow, tagMagnitude);
			AssertTagApplied("custom sql filter strip should have applied the tag", workflow2, tagMagnitude);

			Factory.Save();

			var runner = new DummyTagRuleRunner(false, rule);

			using (TestConnection.TrackExecutedCommands())
			{
				runner.Process();

				AssertNotNull("We expect a query that contains some fancy specific code to have been run, but instead we got bupkis!", TestConnection.ExecutedCommands);

				List<string> commands = new List<string>();
				foreach (string query in TestConnection.ExecutedCommands)
				{
					if (query.Contains("RULE NAME"))
					{
						commands.Add(query);
					}
				}

				Assert("We should have some amount of queries here, and yet...", !commands.IsNullOrEmpty());

				foreach (var command in commands)
				{
					AssertDoesNotContainInlinedPKsInSQL(command);

					if (command.Contains(TagRuleRunStrategyBaseConstants.TemplateNames.RemoveTagRuleSQL))
					{
						AssertContains("The query should be parameterised, and yet...", TagRuleRunStrategyBaseConstants.ParameterNames.TagRulesNow, command);
						AssertContains("The query should be parameterised, and yet...", TagRuleRunStrategyBaseConstants.ParameterNames.TagRulesUtcNow, command);
						AssertContains("The query should be parameterised, and yet...", TagRuleRunStrategyBaseConstants.ParameterNames.TagRulePK, command);
					}
					else
					{
						AssertContains("The query should be parameterised, and yet...", TagRuleRunStrategyBaseConstants.TemplateNames.RemoveTagWorkflowSQL, command);
					}
				}
			}
		}
		public void TestRuleSQL_ShouldUpdateInPKOrder_RemoveTagRuleSQL()
		{
			BMSRegistry.Instance.TagRuleRunnerPrimarySecondaryServerTransferBatchSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);
			var tagDefinition = BMSTestHelper.CreateTagDefinition(Factory, "AAA");
			var tagMagnitude = BMSTestHelper.CreateTagMagnitude(tagDefinition, "AAA");

			var rule = BMSTestHelper.CreateTagRule(tagMagnitude, "Add CC1", TagRuleActionTypeList.Codes.RemoveTag);
			FilterStripsTestHelper.AddFilterStrips(rule.Filter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Custom SQL Filter",
				FilterStripValueSetter = f => ((ModuleSQLFilter)f).Property1 = "FH_CompletionStatement like 'peekaboo%'",
			});

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			workflow.FH_CompletionStatement = "peekaboo1";
			workflow.AddTag(tagMagnitude);

			Factory.Save();

			var runner = new DummyTagRuleRunner(false, rule);

			using (TestConnection.TrackExecutedCommands())
			{
				runner.Process();

				var query = TestConnection.ExecutedCommands.SingleOrDefault(q => q.Contains(TagRuleRunStrategyBaseConstants.TemplateNames.RemoveTagRuleSQL));
				CombineAssertions(() =>
				{
					AssertNotNull("We expect a query that contains some fancy specific code to have been run, but instead we got bupkis!", query);
					AssertContains("The query should use the new method to update FH_SystemLastEditTimeUtc.", "@UpdatedPKs", query);
					AssertContains("The query should use the new method to update FH_SystemLastEditTimeUtc.", "INDEX([PK_UX__FH_PK])", query);
				});
			}
		}

		public void TestMaintainMagnitudeTagRule_ShouldNotRunPerformanceVerification_IfPerformanceCheckDisabled()
		{
			BMSRegistry.Instance.TagRulePerformanceCheckFrequency.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);

			var tagDef = BMSTestHelper.CreateTagDefinition(Factory, "CCC");
			var tagMag = BMSTestHelper.CreateTagMagnitude(tagDef, "CCC");

			var rule = BMSTestHelper.CreateTagRule(tagMag, "Maintain Magnitude CCC", TagRuleActionTypeList.Codes.MaintainMagnitude);
			rule.TagTemplate.TGL_Magnitude = 7m;

			FilterStripsTestHelper.AddFilterStrips(rule.Filter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "This one!",
			});

			Factory.Save();

			AssertEquals("Verification should NOT have run", ZDateTime.Empty, rule.TGR_LastPerformanceVerificationDateTimeUtc);
		}

		[TestDate(2019, 11, 1)]
		public void TestMaintainMagnitudeTagRule_ShouldRunPerformanceVerification_AndChangeMaxdop()
		{
			BMSRegistry.Instance.TagRulePerformanceCheckFrequency.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 2);

			var tagDef = BMSTestHelper.CreateTagDefinition(Factory, "CCC");
			var tagMag = BMSTestHelper.CreateTagMagnitude(tagDef, "CCC");

			var rule = BMSTestHelper.CreateTagRule(tagMag, "Maintain Magnitude CCC", TagRuleActionTypeList.Codes.MaintainMagnitude);
			rule.TagTemplate.TGL_Magnitude = 7m;

			FilterStripsTestHelper.AddFilterStrips(rule.Filter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "This one!",
			});

			var workflow1 = CreateJobHeader<OrgHeader>().ProcessHeaders.AddNew();
			var workflow2 = CreateJobHeader<OrgHeader>().ProcessHeaders.AddNew();
			var workflow3 = CreateJobHeader<OrgHeader>().ProcessHeaders.AddNew();
			var workflow4 = CreateJobHeader<OrgHeader>().ProcessHeaders.AddNew();

			workflow1.FH_CompletionStatement = "This one!";
			workflow1.AddTag(tagMag).Link.TGL_Magnitude = 5m;

			workflow2.FH_CompletionStatement = "Not this one?";
			workflow2.AddTag(tagMag).Link.TGL_Magnitude = 7m;

			workflow3.FH_CompletionStatement = "This one!";
			workflow3.AddTag(tagMag).Link.TGL_Magnitude = 7m;

			workflow4.FH_CompletionStatement = "This one!";

			rule.TGR_LastPerformanceVerificationDateTimeUtc = ZDateTime.UtcNow.AddDays(-3);

			AssertEquals(false, rule.TGR_RunAddQuerySingleThreaded);
			AssertEquals(false, rule.TGR_RunMagnitudeQuerySingleThreaded);
			AssertEquals(false, rule.TGR_RunRemoveQuerySingleThreaded);

			rule.FirstRunTimeForTest = 9999.0;
			Factory.Save();

			var connectionProvider = SecondaryServerConnectionProviderProvider.GetProvider();

			var logger = new TestServiceLogger();
			var dummyRunner = new DummyTagRuleRunner(logger, rule);
			dummyRunner.Strategy = new MaintainMagnitudeTagRuleRunStrategyForTest(connectionProvider, logger);
			using (Db.Connection.TrackExecutedCommands())
			{
				dummyRunner.Process();

				AssertEquals("MAXDOP of 1 should be present in the tag rule query", 1, Db.Connection.ExecutedCommands.Count(p => p.Contains("OPTION (MAXDOP 1)")));
				AssertEquals(false, rule.TGR_RunAddQuerySingleThreaded);
				AssertEquals(true, rule.TGR_RunMagnitudeQuerySingleThreaded);
				AssertEquals(false, rule.TGR_RunRemoveQuerySingleThreaded);

				AssertContains("Information|Verified performance of rule [Maintain Magnitude CCC], MAXDOP set to 1 (single).", logger.ToString());

				var newFactory = new ReadOnlyBusinessObjectFactory();
				var loadedRule = newFactory.Load<TagRule>(rule.PK);
				AssertEquals("Performance Verification details should have actually been saved. SAD!", ZDateTime.UtcNow, loadedRule.TGR_LastPerformanceVerificationDateTimeUtc);
			}

			BMSRegistry.Instance.ThreadedQuerySlownessThresholdFactor.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1.0m);

			rule.TGR_LastPerformanceVerificationDateTimeUtc = ZDateTime.Empty;
			rule.Schedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddMinutes(-1);
			Factory.Save();

			using (Db.Connection.TrackExecutedCommands())
			{
				dummyRunner.Process();

				AssertEquals("No MAXDOP should be present in the tag rule query", 0, Db.Connection.ExecutedCommands.Count(p => p.Contains("OPTION (MAXDOP 0)")));
				AssertContains("Information|Verified performance of rule [Maintain Magnitude CCC], MAXDOP disabled (parallel).", logger.ToString());
				AssertEquals(false, rule.TGR_RunAddQuerySingleThreaded);
				AssertEquals(false, rule.TGR_RunMagnitudeQuerySingleThreaded);
				AssertEquals(false, rule.TGR_RunRemoveQuerySingleThreaded);
			}

			rule.TGR_LastPerformanceVerificationDateTimeUtc = ZDateTime.Empty;
			rule.FirstRunTimeForTest = 0.0;
			rule.Schedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddMinutes(-1);
			Factory.Save();

			using (Db.Connection.TrackExecutedCommands())
			{
				dummyRunner.Process();
				AssertContains("Information|Verified performance of rule [Maintain Magnitude CCC], MAXDOP unchanged.", logger.ToString());
				AssertEquals("No MAXDOP should be present in the tag rule query", 0, Db.Connection.ExecutedCommands.Count(p => p.Contains("OPTION (MAXDOP 0)")));
			}
		}

		[TestDate(2019, 11, 1)]
		public void TestMaintainMagnitudeTagRule_ShouldRunPerformanceVerification_WithParameterizedQuery()
		{
			BMSRegistry.Instance.TagRulePerformanceCheckFrequency.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 2);

			var tagDef = BMSTestHelper.CreateTagDefinition(Factory, "CCC");
			var tagMag = BMSTestHelper.CreateTagMagnitude(tagDef, "CCC");

			var rule = BMSTestHelper.CreateTagRule(tagMag, "Maintain Magnitude CCC", TagRuleActionTypeList.Codes.MaintainMagnitude);
			rule.TagTemplate.TGL_Magnitude = 7m;

			FilterStripsTestHelper.AddFilterStrips(rule.Filter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "This one!",
			});

			var workflow1 = CreateJobHeader<OrgHeader>().ProcessHeaders.AddNew();
			var workflow2 = CreateJobHeader<OrgHeader>().ProcessHeaders.AddNew();
			var workflow3 = CreateJobHeader<OrgHeader>().ProcessHeaders.AddNew();
			var workflow4 = CreateJobHeader<OrgHeader>().ProcessHeaders.AddNew();

			workflow1.FH_CompletionStatement = "This one!";
			workflow1.AddTag(tagMag).Link.TGL_Magnitude = 5m;

			workflow2.FH_CompletionStatement = "Not this one?";
			workflow2.AddTag(tagMag).Link.TGL_Magnitude = 7m;

			workflow3.FH_CompletionStatement = "This one!";
			workflow3.AddTag(tagMag).Link.TGL_Magnitude = 7m;

			workflow4.FH_CompletionStatement = "This one!";

			rule.TGR_LastPerformanceVerificationDateTimeUtc = ZDateTime.UtcNow.AddDays(-3);

			AssertEquals(false, rule.TGR_RunAddQuerySingleThreaded);
			AssertEquals(false, rule.TGR_RunMagnitudeQuerySingleThreaded);
			AssertEquals(false, rule.TGR_RunRemoveQuerySingleThreaded);

			rule.FirstRunTimeForTest = 9999.0;
			Factory.Save();

			var connectionProvider = SecondaryServerConnectionProviderProvider.GetProvider();

			var logger = new TestServiceLogger();
			var dummyRunner = new DummyTagRuleRunner(logger, rule);
			dummyRunner.Strategy = new MaintainMagnitudeTagRuleRunStrategyForTest(connectionProvider, logger);
			using (Db.Connection.TrackExecutedCommands())
			{
				rule.Schedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddMinutes(-1);
				dummyRunner.Process();

				AssertEquals(false, rule.TGR_RunAddQuerySingleThreaded);
				AssertEquals(true, rule.TGR_RunMagnitudeQuerySingleThreaded);
				AssertEquals(false, rule.TGR_RunRemoveQuerySingleThreaded);

				var executedParameterizedQueries = Db.Connection.ExecutedCommands.Where(p => p.Contains("FH_CompletionStatement like @")).ToArray();
				AssertEquals("MAXDOP of 1 should be present in the tag rule query", 1, Db.Connection.ExecutedCommands.Count(p => p.Contains("OPTION (MAXDOP 1)")));

				AssertEquals("'Verification' and 'Processed Rule' queries should be the two queries with a parameter with filter value: StartsWith('This one!%')",
					2, executedParameterizedQueries.Length);
				AssertContains("Number of rows modifed shouldn't be unexpectedly different with parameterized queries",
											"Information|Processed rule [Maintain Magnitude CCC], rows modified [1]", logger.ToString());
				AssertContains("Information|Verified performance of rule [Maintain Magnitude CCC], MAXDOP set to 1 (single).", logger.ToString());
			}

			BMSRegistry.Instance.ThreadedQuerySlownessThresholdFactor.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1.0m);
			rule.TGR_LastPerformanceVerificationDateTimeUtc = ZDateTime.Empty;
			Factory.Save();
			using (Db.Connection.TrackExecutedCommands())
			{
				rule.Schedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddMinutes(-1);
				dummyRunner.Process();

				var executedParameterizedQueries = Db.Connection.ExecutedCommands.Where(p => p.Contains("FH_CompletionStatement like @")).ToArray();
				AssertEquals("There should be no MAXDOP present in the tag rule query", 0, Db.Connection.ExecutedCommands.Count(p => p.Contains("OPTION (MAXDOP 0)")));

				AssertEquals("'Verification' and 'Processed Rule' queries should be the two queries with a parameter with filter value: StartsWith('This one!%')",
					2, executedParameterizedQueries.Length);
				AssertContains("Number of rows modifed shouldn't be unexpectedly different with parameterized queries",
											"Information|Processed rule [Maintain Magnitude CCC], rows modified [1]", logger.ToString());
				AssertContains("Information|Verified performance of rule [Maintain Magnitude CCC], MAXDOP disabled (parallel).", logger.ToString());
				AssertEquals(false, rule.TGR_RunAddQuerySingleThreaded);
				AssertEquals(false, rule.TGR_RunMagnitudeQuerySingleThreaded);
				AssertEquals(false, rule.TGR_RunRemoveQuerySingleThreaded);
			}

			rule.TGR_LastPerformanceVerificationDateTimeUtc = ZDateTime.Empty;
			rule.FirstRunTimeForTest = 0.0;
			Factory.Save();
			using (Db.Connection.TrackExecutedCommands())
			{
				rule.Schedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddMinutes(-1);
				dummyRunner.Process();
				var executedParameterizedQueries = Db.Connection.ExecutedCommands.Where(p => p.Contains("FH_CompletionStatement like @")).ToArray();

				AssertEquals("'Verification' and 'Processed Rule' queries should be the two queries with a parameter with filter value: StartsWith('This one!%')",
					2, executedParameterizedQueries.Length);
				AssertContains("Information|Verified performance of rule [Maintain Magnitude CCC], MAXDOP unchanged.", logger.ToString());
				AssertContains("Number of rows modifed shouldn't be unexpectedly different with parameterized queries",
											"Information|Processed rule [Maintain Magnitude CCC], rows modified [0]", logger.ToString());

				AssertEquals("No MAXDOP should be present in the tag rule query", 0, Db.Connection.ExecutedCommands.Count(p => p.Contains("OPTION (MAXDOP 0)")));
			}
		}

		#endregion

		#region Query Headings

		public void TestQueryHeadings_Add()
		{
			var rule = BMSTestHelper.CreateTagRuleWithDefAndMag(Factory, actionType: TagRuleActionTypeList.Codes.AddTag);
			FilterStripsTestHelper.AddStartsWithFilter(rule.Filter, ProcessHeader.ModuleFilterConstants.CompletionStatement, "W");
			var workflow = BMSTestHelper.CreateWorkflowAndParents<SalesEnquiry>(Factory, "Workflow");

			Factory.Save();

			var runner = new DummyTagRuleRunner(new DummyLogger(), rule);

			using (TestConnection.TrackExecutedCommands())
			{
				runner.Process();

				AssertQueryHeader(TagRuleRunStrategyBaseConstants.TemplateNames.AddTagRuleSQL);
				AssertQueryHeader(TagRuleRunStrategyBaseConstants.TemplateNames.AddTagRuleWorkflowSelectQuery);
			}
		}

		public void TestQueryHeadings_Remove()
		{
			var rule = BMSTestHelper.CreateTagRuleWithDefAndMag(Factory, actionType: TagRuleActionTypeList.Codes.RemoveTag);
			FilterStripsTestHelper.AddStartsWithFilter(rule.Filter, ProcessHeader.ModuleFilterConstants.CompletionStatement, "W");

			var workflow = BMSTestHelper.CreateWorkflowAndParents<SalesEnquiry>(Factory, "Workflow");
			workflow.AddTag(rule.TagTemplate.Magnitude);

			Factory.Save();

			var runner = new DummyTagRuleRunner(new DummyLogger(), rule);

			using (TestConnection.TrackExecutedCommands())
			{
				runner.Process();

				AssertQueryHeader(TagRuleRunStrategyBaseConstants.TemplateNames.RemoveTagRuleSQL);
				AssertQueryHeader(TagRuleRunStrategyBaseConstants.TemplateNames.RemoveTagWorkflowSQL);
			}
		}

		public void TestQueryHeadings_AddRemove()
		{
			var rule = BMSTestHelper.CreateTagRuleWithDefAndMag(Factory, actionType: TagRuleActionTypeList.Codes.AddAndRemoveTag);
			FilterStripsTestHelper.AddStartsWithFilter(rule.Filter, ProcessHeader.ModuleFilterConstants.CompletionStatement, "W");
			var workflow1 = BMSTestHelper.CreateWorkflowAndParents<SalesEnquiry>(Factory, "Workflow");
			var workflow2 = BMSTestHelper.CreateWorkflowAndParents<SalesEnquiry>(Factory, "Jorkflow");
			workflow2.AddTag(rule.TagTemplate.Magnitude);

			Factory.Save();

			var runner = new DummyTagRuleRunner(new DummyLogger(), rule);

			using (TestConnection.TrackExecutedCommands())
			{
				runner.Process();

				AssertQueryHeader(TagRuleRunStrategyBaseConstants.TemplateNames.AddTagRuleSQL);
				AssertQueryHeader(TagRuleRunStrategyBaseConstants.TemplateNames.AddTagRuleWorkflowSelectQuery);
				AssertQueryHeader(TagRuleRunStrategyBaseConstants.TemplateNames.RemoveTagRuleSQL);
				AssertQueryHeader(TagRuleRunStrategyBaseConstants.TemplateNames.RemoveTagWorkflowSQL);
			}
		}

		public void TestQueryHeadings_MaintainMagnitude()
		{
			var rule = BMSTestHelper.CreateTagRuleWithDefAndMag(Factory, actionType: TagRuleActionTypeList.Codes.MaintainMagnitude);
			FilterStripsTestHelper.AddStartsWithFilter(rule.Filter, ProcessHeader.ModuleFilterConstants.CompletionStatement, "W");

			var workflow = BMSTestHelper.CreateWorkflowAndParents<SalesEnquiry>(Factory, "Workflow");
			workflow.AddTag(rule.TagTemplate.Magnitude);

			Factory.Save();

			var runner = new DummyTagRuleRunner(new DummyLogger(), rule);

			using (TestConnection.TrackExecutedCommands())
			{
				runner.Process();

				AssertQueryHeader(TagRuleRunStrategyBaseConstants.TemplateNames.UpdateMagnitudeSql);

				var queries = TestConnection.ExecutedCommands.Where(x => x.Contains(TagRuleRunStrategyBaseConstants.TemplateNames.UpdateMagnitudeSql)).ToArray();
				AssertGreaterThanOrEqualTo(queries.Length, 1);
				foreach (var query in queries)
				{
					AssertNotContains("Queries should not include any temp table declaration. (i.e. not include ##)", "##", query);
				}
			}
		}

		void AssertQueryHeader(string queryTemplate)
		{
			var queries = TestConnection.ExecutedCommands.Where(x => x.Contains(queryTemplate)).ToArray();
			AssertGreaterThanOrEqualTo(queries.Length, 1);

			foreach (var query in queries)
			{
				AssertStartsWith("The query should include the database name so that we can track down bad queries that appear in Kibana. SAD!", $@"
--
-- Query Template: {queryTemplate}
-- Database:       {TestConnection.CurrentDatabase}
-- RULE NAME:      The Tag Rule
", query);
			}
		}

		public void TestGetAdditionalPreviewFilter_ForMaintainMagnitudeQuery_ShouldContainQueryInfo()
		{
			var rule = BMSTestHelper.CreateTagRuleWithDefAndMag(Factory, actionType: TagRuleActionTypeList.Codes.MaintainMagnitude);
			FilterStripsTestHelper.AddStartsWithFilter(rule.Filter, ProcessHeader.ModuleFilterConstants.CompletionStatement, "W");

			var workflow = BMSTestHelper.CreateWorkflowAndParents<SalesEnquiry>(Factory, "Workflow");
			workflow.AddTag(rule.TagTemplate.Magnitude);

			Factory.Save();

			using (TestConnection.TrackExecutedCommands())
			{
				Factory.Load<ProcessHeader>(rule.GetAdditionalPreviewFilter(ModuleIDs.ProcessHeader.Name, string.Empty));

				var query = TestConnection.ExecutedCommands.SingleOrDefault(x => x.Contains(TagRuleRunStrategyBaseConstants.TemplateNames.SelectAffectedProcessHeadersSql));
				AssertNotNull(query);

				AssertContains("The query should include the database name so that we can track down bad queries that appear in Kibana. SAD!", $@"
--
-- Query Template: {TagRuleRunStrategyBaseConstants.TemplateNames.SelectAffectedProcessHeadersSql}
-- Database:       {TestConnection.CurrentDatabase}
-- RULE NAME:      The Tag Rule
--", query);
			}
		}

		#endregion

		#region Branch and Department

		[TestDate(2019, 11, 1)]
		public void TestLoggedInBranchAndDepartment_WhenSpecifiedOnTagRule_ShouldMatchTagRule()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "AAA";
			var dept = Factory.NewWithValidTestData<GlbDepartment>();
			dept.GE_Code = "BBB";
			var tagDef = BMSTestHelper.CreateTagDefinition(Factory, "CCC");
			var tagMag = BMSTestHelper.CreateTagMagnitude(tagDef, "CCC");

			var rule = BMSTestHelper.CreateTagRule(tagMag, "A sailor went to CCC", TagRuleActionTypeList.Codes.MaintainMagnitude);
			rule.TGR_GB_Branch = branch.PK;
			rule.TGR_GE_Department = dept.PK;
			rule.TagTemplate.TGL_Magnitude = 1m;
			rule.Schedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddMinutes(-1);

			Factory.Save();

			var connectionProvider = SecondaryServerConnectionProviderProvider.GetProvider();
			var logger = new TestServiceLogger();
			var dummyRunner = new DummyTagRuleRunner(logger, rule);
			dummyRunner.Strategy = new MaintainMagnitudeTagRuleRunStrategyForTest(connectionProvider, logger);
			dummyRunner.Process();

			AssertContains("Processed rule [A sailor went to CCC], rows modified [0], branch [AAA], department [BBB]", logger.ToString());
		}

		[TestDate(2019, 11, 1)]
		public void TestLoggedInBranchAndDepartment_WhenNotSpecifiedOnTagRule_ShouldMatchLoggedInBranchAndDepartment()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "AAA";
			var dept = Factory.NewWithValidTestData<GlbDepartment>();
			dept.GE_Code = "BBB";

			var tagDef = BMSTestHelper.CreateTagDefinition(Factory, "CCC");
			var tagMag = BMSTestHelper.CreateTagMagnitude(tagDef, "CCC");

			var rule = BMSTestHelper.CreateTagRule(tagMag, "A sailor went to CCC", TagRuleActionTypeList.Codes.MaintainMagnitude);
			rule.TagTemplate.TGL_Magnitude = 1m;
			rule.Schedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddMinutes(-1);

			Factory.Save();

			var connectionProvider = SecondaryServerConnectionProviderProvider.GetProvider();
			var logger = new TestServiceLogger();
			var dummyRunner = new DummyTagRuleRunner(logger, rule);
			dummyRunner.Strategy = new MaintainMagnitudeTagRuleRunStrategyForTest(connectionProvider, logger);
			dummyRunner.Process();

			AssertContains($"Processed rule [A sailor went to CCC], rows modified [0], branch [{Env.CurrentBranch.Code}], department [{Env.CurrentDepartment.Code}]", logger.ToString());
		}

		[TestDate(2019, 11, 15, 0, 0, 0)] // DST in Sydney, not Brisbane
		public void TestUpdateTags_WhenTagRuleBranchIsBrisbane()
		{
			TestDateAttribute.UseUNLOCO = true;

			var branchSYD = Factory.NewWithValidTestData<GlbBranch>();
			branchSYD.GB_BranchName = "Rancid Knee";
			branchSYD.GB_Code = "AUS";
			branchSYD.GB_RL_NKHomePort = "AUSYD";

			var branchBNE = Factory.NewWithValidTestData<GlbBranch>();
			branchBNE.GB_BranchName = "Brisbane of my existence";
			branchBNE.GB_Code = "AUB";
			branchBNE.GB_RL_NKHomePort = "AUBNE";

			var dept = Factory.NewWithValidTestData<GlbDepartment>();
			dept.GE_Desc = "Jony";
			dept.GE_Code = "DEP";

			var tagDef = BMSTestHelper.CreateTagDefinition(Factory, "CCC");
			var tagMag = BMSTestHelper.CreateTagMagnitude(tagDef, "CCC");

			var workflow1 = BMSTestHelper.CreateWorkflow(Factory, "Won", null);
			workflow1.FH_AgreedDeliveryDate = new ZDateTime(2019, 11, 14, 23, 0, 0); // 15th NOV 9am BNE/15th NOV 10am SYD -- 14th NOV 11pm UTC
			var w1Tag = workflow1.AddTag(tagMag).Link;
			w1Tag.TGL_Magnitude = 1m;

			var workflow2 = BMSTestHelper.CreateWorkflow(Factory, "Too", null);
			workflow2.FH_AgreedDeliveryDate = new ZDateTime(2019, 11, 14, 13, 0, 0); // 14th NOV 11pm BNE/15th NOV 12am SYD -- 14th NOV 1pm UTC
			var w2Tag = workflow2.AddTag(tagMag).Link;
			w2Tag.TGL_Magnitude = 1m;

			var rule = BMSTestHelper.CreateTagRule(tagMag, "A sailor went to CCC", TagRuleActionTypeList.Codes.MaintainMagnitude);
			rule.TagTemplate.TGL_Magnitude = 2m;
			rule.TGR_GB_Branch = branchBNE.PK;
			rule.TGR_GE_Department = dept.PK;
			rule.Schedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddMinutes(-1);

			FilterStripsTestHelper.AddFilterStrips(rule.Filter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.AgreedDeliveryDate,
				FilterStripValueSetter = f => ((ModuleDateFilter)f).PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Today,
			});

			Factory.Save();

			AssertEquals("Australia/Sydney", branchSYD.HomePort.TimeZoneSet.R3_TimeZoneSetName);
			AssertEquals("Australia/Brisbane", branchBNE.HomePort.TimeZoneSet.R3_TimeZoneSetName);

			var connectionProvider = SecondaryServerConnectionProviderProvider.GetProvider();
			var logger = new TestServiceLogger();
			var dummyRunner = new DummyTagRuleRunner(logger, rule);
			dummyRunner.Strategy = new MaintainMagnitudeTagRuleRunStrategyForTest(connectionProvider, logger);
			dummyRunner.Process();

			var newFactory = new BusinessObjectFactory();
			var loadedw1Tag = newFactory.Load<TagLink>(w1Tag.PK);
			var loadedw2Tag = newFactory.Load<TagLink>(w2Tag.PK);

			CombineAssertions("Since only workflow 1's agreed delivery date is 'Today' in Brisbane, only the first workflow's tag should be updated", () =>
			{
				AssertEquals(2m, loadedw1Tag.TGL_Magnitude);
				AssertEquals(1m, loadedw2Tag.TGL_Magnitude);
				AssertContains("Processed rule [A sailor went to CCC], rows modified [1], branch [AUB], department [DEP]", logger.ToString());
			});
		}

		[TestDate(2019, 11, 15, 0, 0, 0)] // DST in Sydney, not Brisbane
		public void TestUpdateTags_WhenTagRuleBranchIsSydney()
		{
			TestDateAttribute.UseUNLOCO = true;

			var branchSYD = Factory.NewWithValidTestData<GlbBranch>();
			branchSYD.GB_BranchName = "Rancid Knee";
			branchSYD.GB_Code = "AUS";
			branchSYD.GB_RL_NKHomePort = "AUSYD";

			var branchBNE = Factory.NewWithValidTestData<GlbBranch>();
			branchBNE.GB_BranchName = "Brisbane of my existence";
			branchBNE.GB_Code = "AUB";
			branchBNE.GB_RL_NKHomePort = "AUBNE";

			var dept = Factory.NewWithValidTestData<GlbDepartment>();
			dept.GE_Desc = "Jony";
			dept.GE_Code = "DEP";

			var tagDef = BMSTestHelper.CreateTagDefinition(Factory, "CCC");
			var tagMag = BMSTestHelper.CreateTagMagnitude(tagDef, "CCC");

			var workflow1 = BMSTestHelper.CreateWorkflow(Factory, "Won", null);
			workflow1.FH_AgreedDeliveryDate = new ZDateTime(2019, 11, 14, 23, 0, 0); // 15th NOV 9am BNE/15th NOV 10am SYD
			var w1Tag = workflow1.AddTag(tagMag).Link;
			w1Tag.TGL_Magnitude = 1m;

			var workflow2 = BMSTestHelper.CreateWorkflow(Factory, "Too", null);
			workflow2.FH_AgreedDeliveryDate = new ZDateTime(2019, 11, 14, 13, 0, 0); // 14th NOV 11pm BNE/15th NOV 12am SYD
			var w2Tag = workflow2.AddTag(tagMag).Link;
			w2Tag.TGL_Magnitude = 1m;

			var rule = BMSTestHelper.CreateTagRule(tagMag, "A sailor went to CCC", TagRuleActionTypeList.Codes.MaintainMagnitude);
			rule.TagTemplate.TGL_Magnitude = 2m;
			rule.TGR_GB_Branch = branchSYD.PK;
			rule.TGR_GE_Department = dept.PK;
			rule.Schedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddMinutes(-1);

			FilterStripsTestHelper.AddFilterStrips(rule.Filter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.AgreedDeliveryDate,
				FilterStripValueSetter = f =>
				{
					((ModuleDateFilter)f).PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Today;
				},
			});

			Factory.Save();

			AssertEquals("Australia/Sydney", branchSYD.HomePort.TimeZoneSet.R3_TimeZoneSetName);
			AssertEquals("Australia/Brisbane", branchBNE.HomePort.TimeZoneSet.R3_TimeZoneSetName);

			var connectionProvider = SecondaryServerConnectionProviderProvider.GetProvider();
			var logger = new TestServiceLogger();
			var dummyRunner = new DummyTagRuleRunner(logger, rule);
			dummyRunner.Strategy = new MaintainMagnitudeTagRuleRunStrategyForTest(connectionProvider, logger);
			dummyRunner.Process();

			var newFactory = new BusinessObjectFactory();
			var loadedw1Tag = newFactory.Load<TagLink>(w1Tag.PK);
			var loadedw2Tag = newFactory.Load<TagLink>(w2Tag.PK);

			CombineAssertions("Since both workflows' agreed delivery date is 'Today' in Sydney, both workflows' tags should be updated", () =>
			{
				AssertEquals(2m, loadedw1Tag.TGL_Magnitude);
				AssertEquals(2m, loadedw2Tag.TGL_Magnitude);
				AssertContains("Processed rule [A sailor went to CCC], rows modified [2], branch [AUS], department [DEP]", logger.ToString());
			});
		}

		[TestDate(2019, 3, 11, 0, 0, 0)]
		public void TestAddTag_EnsureOnlyWorkflowsNotRecentlyProcessedAreProcessed()
		{
			TestDateAttribute.AddDays(-1);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Peter Frampton");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Rover Hendrix");

			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "LOL", "Lollapalooza");
			var tagMag = BMSTestHelper.CreateTagMagnitude(tagGroup, "CAN", "Cannonball");

			BMSTestHelper.CreateTagLink(workflow1, tagMag);

			var rule = BMSTestHelper.CreateTagRule(tagMag, "Billy Corgan Smashing Pumpkins", TagRuleActionTypeList.Codes.AddTag, templateDescription: "Homer Simpson Smiling Politely");
			rule.TagTemplate.TGL_Magnitude = 1m;
			rule.TGR_LastPerformanceVerificationDateTimeUtc = ZDateTime.UtcNow;
			rule.Schedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddMinutes(-1);

			Factory.Save();

			TestDateAttribute.AddDays(1);

			var utcNow = ZDateTime.UtcNow;

			AssertEquals(utcNow.AddDays(-1), workflow1.FH_SystemLastEditTimeUtc);
			AssertEquals(utcNow.AddDays(-1), workflow2.FH_SystemLastEditTimeUtc);

			var log = workflow1.GetLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.TagWasAddedOrRemovedCode));
			CombineAssertions("Initial log state", () =>
			{
				AssertEquals("workflow1 should have just one log", 1, log.Length);
				log = workflow1.GetLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.TagWasAddedOrRemovedCode)).Where(w => w.SL_Reference.Contains("RUL")).ToArray();
				AssertEquals("and that log should correspond with the tag link we manually created", 0, log.Length);

				log = workflow2.GetLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.TagWasAddedOrRemovedCode));
				AssertEquals("workflow2 should not have a log at all", 0, log.Length);
			});

			TestConnection.ExecuteNonQuery(@"
			CREATE TABLE #WorkflowsToTagTempTable
			(
				FH_PK uniqueidentifier,
				FH_ParentId uniqueidentifier,
				RowNumber INT NOT NULL IDENTITY(1,1) PRIMARY KEY
			)");

			var row = @"
			INSERT INTO #WorkflowsToTagTempTable
			(FH_PK, FH_ParentID) VALUES
			('{0}', '{1}')";

			TestConnection.ExecuteNonQuery(string.Format(row, workflow1.PK, workflow1.FH_ParentId));
			TestConnection.ExecuteNonQuery(string.Format(row, workflow2.PK, workflow2.FH_ParentId));

			var mockTempTable = new Mock<OffloadingTagRuleRunStrategy.ITempTableWorker>();

			mockTempTable.Setup(m => m.InsertWorkflowsToTempTable(It.IsAny<QueryTextAndParameters>())).Returns(2);
			mockTempTable.Setup(m => m.TempTableName).Returns("#WorkflowsToTagTempTable");
			var connectionProvider = SecondaryServerConnectionProviderProvider.GetProvider();
			var logger = new TestServiceLogger();

			using (Db.Connection.TrackExecutedCommands())
			{
				var dummyRunner = new DummyTagRuleRunner(logger, new[] { rule });
				dummyRunner.Strategy = new AddTagRuleRunSqlStrategyForTest(connectionProvider, logger, mockTempTable.Object);
				dummyRunner.Process();

				var newFactory = new BusinessObjectFactory();
				var query = new ZQuery(TagLinkSchema.TGL_TGM_Magnitude, tagMag.PK);
				query.AddToFilter(TagLinkSchema.TGL_ParentTableCode, ProcessHeaderSchema.Constants.Prefix);
				var taglinks = newFactory.Load<TagLink>(query);

				var loadedWorkflow1 = newFactory.Load<ProcessHeader>(workflow1.PK);
				var loadedWorkflow2 = newFactory.Load<ProcessHeader>(workflow2.PK);

				CombineAssertions("Only one of the rows should be processed. It's not? Quelle catastrophe!", () =>
				{
					AssertContains(@"Information|Processed rule [Billy Corgan Smashing Pumpkins], rows modified [1], branch [BNE], department [BRN]", logger.ToString());
					AssertEquals(2, taglinks.Length);
					AssertContainsExactElementsInAnyOrder("Should contain the tag link we created earlier and the one that the tag rule runner just created",
						new[] { workflow1.PK, workflow2.PK }, taglinks.Select(t => t.TGL_ParentId));

					log = loadedWorkflow1.GetLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.TagWasAddedOrRemovedCode)).
						Where(w => w.SL_Reference.Contains("RUL")).
						Where(w => w.SL_Reference.Contains("ACT=ADD")).ToArray();
					AssertEquals("workflow1 should NOT have a log added by tag rule", 0, log.Length);

					log = loadedWorkflow2.GetLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.TagWasAddedOrRemovedCode)).
						Where(w => w.SL_Reference.Contains("RUL")).
						Where(w => w.SL_Reference.Contains("ACT=ADD")).ToArray();
					AssertEquals("workflow2 should have a log added by tag rule", 1, log.Length);
				});

				AssertEquals("There should be no references to temporary tables that exist only on the secondary server in the add tag query",
					0, Db.Connection.ExecutedCommands.Count(p => p.Contains("#WorkflowsToTagTempTable") && p.Contains("Billy Corgan Smashing Pumpkins")));
			}
		}

		[TestDate(2019, 3, 11, 0, 0, 0)]
		public void TestAddTag_EnsureTagNotAddedMoreThanOnce()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Robert Underdunk Terwilliger");

			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "CLC", "Clown College");
			var tagMag = BMSTestHelper.CreateTagMagnitude(tagGroup, "PRI", "Princeton");

			var rule = BMSTestHelper.CreateTagRule(tagMag, "Especially Lisa", TagRuleActionTypeList.Codes.AddTag, templateDescription: "But especially Bart");
			rule.TagTemplate.TGL_Magnitude = 1m;
			rule.TGR_LastPerformanceVerificationDateTimeUtc = ZDateTime.UtcNow;
			rule.Schedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddMinutes(-1);

			Factory.Save();

			var utcNow = ZDateTime.UtcNow;

			TestConnection.ExecuteNonQuery(@"
			CREATE TABLE #WorkflowsToTagTempTable
			(
				FH_PK uniqueidentifier,
				FH_ParentId uniqueidentifier,
				RowNumber INT NOT NULL IDENTITY(1,1) PRIMARY KEY
			)");

			var row = @"
			INSERT INTO #WorkflowsToTagTempTable
			(FH_PK, FH_ParentID) VALUES
			('{0}', '{1}')";

			TestConnection.ExecuteNonQuery(string.Format(row, workflow.PK, workflow.FH_ParentId));

			var mockTempTable = new Mock<OffloadingTagRuleRunStrategy.ITempTableWorker>();

			mockTempTable.Setup(m => m.InsertWorkflowsToTempTable(It.IsAny<QueryTextAndParameters>())).Returns(2);
			mockTempTable.Setup(m => m.TempTableName).Returns("#WorkflowsToTagTempTable");
			var connectionProvider = SecondaryServerConnectionProviderProvider.GetProvider();
			var logger = new TestServiceLogger();

			using (Db.Connection.TrackExecutedCommands())
			{
				var dummyRunner = new DummyTagRuleRunner(logger, new[] { rule });
				dummyRunner.Strategy = new AddTagRuleRunSqlStrategyForTest(connectionProvider, logger, mockTempTable.Object, () =>
				{
					BMSTestHelper.CreateTagLink(workflow, tagMag);
					Factory.Save();
				});
				dummyRunner.Process();

				var newFactory = new BusinessObjectFactory();

				var query = new ZQuery(TagLinkSchema.TGL_TGM_Magnitude, tagMag.PK);
				query.AddToFilter(TagLinkSchema.TGL_ParentTableCode, ProcessHeaderSchema.Constants.Prefix);
				var taglinks = newFactory.Load<TagLink>(query);

				CombineAssertions("There should only be one tag link. It's not? Sacre bleu!", () =>
				{
					AssertContains(@"Information|Processed rule [Especially Lisa], rows modified [0], branch [BNE], department [BRN]", logger.ToString());
					AssertEquals(1, taglinks.Length);
					AssertEquals(1, taglinks.Where(t => t.TGL_ParentId == workflow.PK).Count());
				});
			}
		}

		public void TestTagRuleRunOrder()
		{
			var branchSYD = Factory.NewWithValidTestData<GlbBranch>();
			branchSYD.GB_BranchName = "Rancid Knee";
			branchSYD.GB_Code = "AUS";
			branchSYD.GB_RL_NKHomePort = "AUSYD";

			var branchBNE = Factory.NewWithValidTestData<GlbBranch>();
			branchBNE.GB_BranchName = "Brisbane of my existence";
			branchBNE.GB_Code = "AUB";
			branchBNE.GB_RL_NKHomePort = "AUBNE";

			var deptJony = Factory.NewWithValidTestData<GlbDepartment>();
			deptJony.GE_Desc = "Jony";
			deptJony.GE_Code = "DEP";

			var deptSleep = Factory.NewWithValidTestData<GlbDepartment>();
			deptSleep.GE_Desc = "Sleep";
			deptSleep.GE_Code = "ZZZ";

			var tagDefExclusive = BMSTestHelper.CreateTagDefinition(Factory, "CCC", isExclusive: true);
			var tagDefInclusive = BMSTestHelper.CreateTagDefinition(Factory, "DDD");
			var tagMagExclusive1 = BMSTestHelper.CreateTagMagnitude(tagDefExclusive, "EEE", ruleRunSequence: 1);
			var tagMagExclusive2 = BMSTestHelper.CreateTagMagnitude(tagDefExclusive, "FFF", ruleRunSequence: 2);
			var tagMagInclusive = BMSTestHelper.CreateTagMagnitude(tagDefInclusive, "GGG");

			var ruleExclusive1 = BMSTestHelper.CreateTagRule(tagMagExclusive2, "Sea Shanty Pipe Solo", TagRuleActionTypeList.Codes.MaintainMagnitude);
			ruleExclusive1.TagTemplate.TGL_Magnitude = 2m;
			ruleExclusive1.TGR_GB_Branch = branchBNE.PK;
			ruleExclusive1.TGR_GE_Department = deptSleep.PK;
			ruleExclusive1.TGR_LastPerformanceVerificationDateTimeUtc = ZDateTime.UtcNow;
			ruleExclusive1.Schedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddMinutes(-1);

			var ruleExclusive2 = BMSTestHelper.CreateTagRule(tagMagExclusive1, "Was the bottom of the deep blue CCC", TagRuleActionTypeList.Codes.MaintainMagnitude);
			ruleExclusive2.TagTemplate.TGL_Magnitude = 1m;
			ruleExclusive2.TGR_GB_Branch = branchSYD.PK;
			ruleExclusive2.TGR_GE_Department = deptJony.PK;
			ruleExclusive2.TGR_LastPerformanceVerificationDateTimeUtc = ZDateTime.UtcNow;
			ruleExclusive2.Schedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddMinutes(-1);

			var ruleInclusive1 = BMSTestHelper.CreateTagRule(tagMagInclusive, "To see what he could CCC", TagRuleActionTypeList.Codes.MaintainMagnitude);
			ruleInclusive1.TagTemplate.TGL_Magnitude = 3m;
			ruleInclusive1.TGR_GB_Branch = branchSYD.PK;
			ruleInclusive1.TGR_GE_Department = deptJony.PK;
			ruleInclusive1.TGR_LastPerformanceVerificationDateTimeUtc = ZDateTime.UtcNow;
			ruleInclusive1.Schedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddMinutes(-1);

			var ruleInclusive2 = BMSTestHelper.CreateTagRule(tagMagInclusive, "And all that he could CCC", TagRuleActionTypeList.Codes.MaintainMagnitude);
			ruleInclusive2.TagTemplate.TGL_Magnitude = 4m;
			ruleInclusive2.TGR_GB_Branch = branchSYD.PK;
			ruleInclusive2.TGR_GE_Department = deptSleep.PK;
			ruleInclusive2.TGR_LastPerformanceVerificationDateTimeUtc = ZDateTime.UtcNow;
			ruleInclusive2.Schedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddMinutes(-1);

			var ruleInclusive3 = BMSTestHelper.CreateTagRule(tagMagInclusive, "A sailor went to CCC", TagRuleActionTypeList.Codes.MaintainMagnitude);
			ruleInclusive3.TagTemplate.TGL_Magnitude = 4m;
			ruleInclusive3.TGR_GB_Branch = branchBNE.PK;
			ruleInclusive3.TGR_GE_Department = deptJony.PK;
			ruleInclusive3.TGR_LastPerformanceVerificationDateTimeUtc = ZDateTime.UtcNow;
			ruleInclusive3.Schedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddMinutes(-1);

			Factory.Save();

			var preProcessBranchPK = Env.CurrentBranchPK;
			var preProcessDepartmentPK = Env.CurrentDepartmentPK;

			var connectionProvider = SecondaryServerConnectionProviderProvider.GetProvider();
			var logger = new TestServiceLogger();
			var dummyRunner = new DummyTagRuleRunner(logger, new[] { ruleExclusive1, ruleExclusive2, ruleInclusive1, ruleInclusive2, ruleInclusive3 });
			dummyRunner.Strategy = new MaintainMagnitudeTagRuleRunStrategyForTest(connectionProvider, logger);
			dummyRunner.Process();

			var newFactory = new BusinessObjectFactory();

			AssertContains(@"Information|Processed rule [A sailor went to CCC], rows modified [0], branch [AUB], department [DEP]
Information|Processed rule [To see what he could CCC], rows modified [0], branch [AUS], department [DEP]
Information|Processed rule [And all that he could CCC], rows modified [0], branch [AUS], department [ZZZ]
Information|Processed rule [Was the bottom of the deep blue CCC], rows modified [0], branch [AUS], department [DEP]
Information|Processed rule [Sea Shanty Pipe Solo], rows modified [0], branch [AUB], department [ZZZ]", logger.ToString());

			CombineAssertions("Branch and department should switch back to pre tag rule run", () =>
			{
				AssertNotEquals(Env.CurrentBranchPK, branchSYD.PK);
				AssertNotEquals(Env.CurrentDepartmentPK, deptSleep.PK);
				AssertEquals(Env.CurrentBranchPK, preProcessBranchPK);
				AssertEquals(Env.CurrentDepartmentPK, preProcessDepartmentPK);
			});

			AssertEquals(5, dummyRunner.ContextSwitchCount);
		}

		public void TestTagRuleRun_MinimiseContextSwitching()
		{
			var branchSYD = Factory.NewWithValidTestData<GlbBranch>();
			branchSYD.GB_BranchName = "Rancid Knee";
			branchSYD.GB_Code = "AUS";
			branchSYD.GB_RL_NKHomePort = "AUSYD";

			var branchBNE = Factory.NewWithValidTestData<GlbBranch>();
			branchBNE.GB_BranchName = "Brisbane of my existence";
			branchBNE.GB_Code = "AUB";
			branchBNE.GB_RL_NKHomePort = "AUBNE";

			var deptJony = Factory.NewWithValidTestData<GlbDepartment>();
			deptJony.GE_Desc = "Jony";
			deptJony.GE_Code = "DEP";

			var deptSleep = Factory.NewWithValidTestData<GlbDepartment>();
			deptSleep.GE_Desc = "Sleep";
			deptSleep.GE_Code = "ZZZ";

			var tagDef = BMSTestHelper.CreateTagDefinition(Factory, "DDD");
			var tagMag = BMSTestHelper.CreateTagMagnitude(tagDef, "GGG");

			var rule1 = BMSTestHelper.CreateTagRule(tagMag, "To see what he could CCC", TagRuleActionTypeList.Codes.MaintainMagnitude);
			rule1.TagTemplate.TGL_Magnitude = 2m;
			rule1.TGR_GB_Branch = branchBNE.PK;
			rule1.TGR_GE_Department = deptJony.PK;
			rule1.TGR_LastPerformanceVerificationDateTimeUtc = ZDateTime.UtcNow;
			rule1.Schedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddMinutes(-1);

			var rule2 = BMSTestHelper.CreateTagRule(tagMag, "A sailor went to CCC", TagRuleActionTypeList.Codes.MaintainMagnitude);
			rule2.TagTemplate.TGL_Magnitude = 1m;
			rule2.TGR_GB_Branch = branchBNE.PK;
			rule2.TGR_GE_Department = deptJony.PK;
			rule2.TGR_LastPerformanceVerificationDateTimeUtc = ZDateTime.UtcNow;
			rule2.Schedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddMinutes(-1);

			var rule3 = BMSTestHelper.CreateTagRule(tagMag, "Was the bottom of the deep blue CCC", TagRuleActionTypeList.Codes.MaintainMagnitude);
			rule3.TagTemplate.TGL_Magnitude = 3m;
			rule3.TGR_GB_Branch = branchSYD.PK;
			rule3.TGR_GE_Department = deptSleep.PK;
			rule3.TGR_LastPerformanceVerificationDateTimeUtc = ZDateTime.UtcNow;
			rule3.Schedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddMinutes(-1);

			var rule4 = BMSTestHelper.CreateTagRule(tagMag, "ZZZ ZZZ ZZZ ZZZ", TagRuleActionTypeList.Codes.MaintainMagnitude);
			rule4.TagTemplate.TGL_Magnitude = 4m;
			rule4.TGR_GB_Branch = branchSYD.PK;
			rule4.TGR_GE_Department = deptSleep.PK;
			rule4.TGR_LastPerformanceVerificationDateTimeUtc = ZDateTime.UtcNow;
			rule4.Schedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddMinutes(-1);

			var rule5 = BMSTestHelper.CreateTagRule(tagMag, "And all that he could CCC", TagRuleActionTypeList.Codes.MaintainMagnitude);
			rule5.TagTemplate.TGL_Magnitude = 4m;
			rule5.TGR_GB_Branch = branchSYD.PK;
			rule5.TGR_GE_Department = deptJony.PK;
			rule5.TGR_LastPerformanceVerificationDateTimeUtc = ZDateTime.UtcNow;
			rule5.Schedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddMinutes(-1);

			Factory.Save();

			var preProcessBranchPK = Env.CurrentBranchPK;
			var preProcessDepartmentPK = Env.CurrentDepartmentPK;

			var connectionProvider = SecondaryServerConnectionProviderProvider.GetProvider();
			var logger = new TestServiceLogger();
			var dummyRunner = new DummyTagRuleRunner(logger, new[] { rule1, rule2, rule3, rule4, rule5 });
			dummyRunner.Strategy = new MaintainMagnitudeTagRuleRunStrategyForTest(connectionProvider, logger);
			dummyRunner.Process();

			var newFactory = new BusinessObjectFactory();

			AssertContains(@"Information|Processed rule [A sailor went to CCC], rows modified [0], branch [AUB], department [DEP]
Information|Processed rule [To see what he could CCC], rows modified [0], branch [AUB], department [DEP]
Information|Processed rule [And all that he could CCC], rows modified [0], branch [AUS], department [DEP]
Information|Processed rule [Was the bottom of the deep blue CCC], rows modified [0], branch [AUS], department [ZZZ]
Information|Processed rule [ZZZ ZZZ ZZZ ZZZ], rows modified [0], branch [AUS], department [ZZZ]", logger.ToString());

			CombineAssertions("Branch and department should switch back to pre tag rule run", () =>
			{
				AssertNotEquals(Env.CurrentBranchPK, branchSYD.PK);
				AssertNotEquals(Env.CurrentDepartmentPK, deptSleep.PK);
				AssertEquals(Env.CurrentBranchPK, preProcessBranchPK);
				AssertEquals(Env.CurrentDepartmentPK, preProcessDepartmentPK);
			});

			AssertEquals(3, dummyRunner.ContextSwitchCount);
		}

		#endregion

		#region Schedule

		[TestDate(2017, 1, 30, 10, 0, 0)]
		public void TestTagRules_ShouldRunOnTheirRespectiveSchedules()
		{
			var def = BMSTestHelper.CreateTagDefinition(Factory, "AAA");
			var mag = BMSTestHelper.CreateTagMagnitude(def, "BBB");
			var rule1 = BMSTestHelper.CreateTagRule(mag, "Rule1", TagRuleActionTypeList.Codes.MaintainMagnitude);
			var rule2 = BMSTestHelper.CreateTagRule(mag, "Rule2", TagRuleActionTypeList.Codes.MaintainMagnitude);
			var rule3 = BMSTestHelper.CreateTagRule(mag, "Rule3", TagRuleActionTypeList.Codes.MaintainMagnitude);

			rule1.Schedule.Recurrence.TaskPeriod = ScheduleRecurrenceType.Hourly;
			rule1.Schedule.S5_DailyStartTime = new ZDateTime(1900, 1, 1, 9, 0, 0);
			rule1.Schedule.S5_DailyEndTime = new ZDateTime(1900, 1, 1, 18, 0, 0);

			rule2.Schedule.Recurrence.TaskPeriod = ScheduleRecurrenceType.Hourly;
			rule2.Schedule.S5_DailyStartTime = new ZDateTime(1900, 1, 1, 9, 0, 0);
			rule2.Schedule.S5_DailyEndTime = new ZDateTime(1900, 1, 1, 18, 0, 0);

			rule3.Schedule.Recurrence.TaskPeriod = ScheduleRecurrenceType.Daily;

			rule1.Schedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddSeconds(1);
			rule2.Schedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddSeconds(1);
			rule3.Schedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddMinutes(30);

			Factory.Save();

			var logger = new TestServiceLogger();
			var runner = new DummyTagRuleRunner(logger, rule1, rule2, rule3);
			runner.Process();

			AssertNotContains("Rule1", logger.ToString());
			AssertNotContains("Rule2", logger.ToString());
			AssertNotContains("Rule3", logger.ToString());

			logger.ClearLog();
			TestDateAttribute.Date = TestDateAttribute.Date.AddSeconds(1);
			runner.Process();

			AssertContains("Rule1", logger.ToString());
			AssertContains("Rule2", logger.ToString());
			AssertNotContains("Rule3", logger.ToString());

			logger.ClearLog();
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(31);
			runner.Process();

			AssertNotContains("Rule1", logger.ToString());
			AssertNotContains("Rule2", logger.ToString());
			AssertContains("Rule3", logger.ToString());

			logger.ClearLog();
			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);
			runner.Process();

			AssertContains("Rule1", logger.ToString());
			AssertContains("Rule2", logger.ToString());
			AssertNotContains("Rule3", logger.ToString());

			logger.ClearLog();
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(30);
			runner.Process();

			AssertNotContains("Rule1", logger.ToString());
			AssertNotContains("Rule2", logger.ToString());
			AssertNotContains("Rule3", logger.ToString());

			logger.ClearLog();
			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(1);
			runner.Process();

			AssertContains("Rule1", logger.ToString());
			AssertContains("Rule2", logger.ToString());
			AssertContains("Rule3", logger.ToString());

			rule1.Reload();
			rule2.Reload();
			rule3.Reload();

			AssertEquals(new ZDateTime(2017, 1, 31, 13, 1, 1), rule1.Schedule.S5_NextScheduledPrintRunTimeUtc);
			AssertEquals(new ZDateTime(2017, 1, 31, 13, 1, 1), rule2.Schedule.S5_NextScheduledPrintRunTimeUtc);
			AssertEquals(new ZDateTime(2017, 2, 1), rule3.Schedule.S5_NextScheduledPrintRunTimeUtc);
		}

		public void TestProcess_ShouldCallFactorySaveOnlyOnce()
		{
			var def = BMSTestHelper.CreateTagDefinition(Factory, "AAA");
			var mag = BMSTestHelper.CreateTagMagnitude(def, "BBB");
			var rule1 = BMSTestHelper.CreateTagRule(mag, "one rule", TagRuleActionTypeList.Codes.MaintainMagnitude);
			var rule2 = BMSTestHelper.CreateTagRule(mag, "other rule", TagRuleActionTypeList.Codes.MaintainMagnitude);
			Factory.Save();

			var logger = new TestServiceLogger();
			var runner = new DummyTagRuleRunner(logger, rule1, rule2);

			var factorySavedCount = 0;
			Factory.Saved += (_, x_) => factorySavedCount++;

			runner.Process();
			AssertEquals(1, factorySavedCount);
		}

		#endregion

		#region WorkflowManagementMode Restrictions

		public void TestProcess_WhenPlanningManagementEnabled_ShouldRun()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.PlanningManagement);
			var rule = BMSTestHelper.CreateTagRuleWithDefAndMag(Factory);
			Factory.Save();

			var logger = new TestServiceLogger();
			var runner = new DummyTagRuleRunner(logger, rule) { ShouldRunAllRules = true };
			runner.Process();

			AssertContains("The runner should run because Buffer Management or better is enabled.", "The Tag Rule", logger.ToString());
			AssertNotContains("The registry item [Workflow Manager -> Buffer Management -> Workflow Management Mode] must be set to a higher level to use this processor.", logger.ToString());
		}

		public void TestProcess_WhenBufferManagementEnabled_ShouldRun()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.IncludesBufferManagement);
			var rule = BMSTestHelper.CreateTagRuleWithDefAndMag(Factory);
			Factory.Save();

			var logger = new TestServiceLogger();
			var runner = new DummyTagRuleRunner(logger, rule) { ShouldRunAllRules = true };
			runner.Process();

			AssertContains("The runner should run because Buffer Management or better is enabled.", "The Tag Rule", logger.ToString());
			AssertNotContains("The registry item [Workflow Manager -> Buffer Management -> Workflow Management Mode] must be set to a higher level to use this processor.", logger.ToString());
		}

		public void TestProcess_WhenEnhancedWorkflowManagementEnabled_ShouldDoNothing()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.EnhancedWorkflow);
			var rule = BMSTestHelper.CreateTagRuleWithDefAndMag(Factory);
			Factory.Save();

			var logger = new TestServiceLogger();
			var runner = new DummyTagRuleRunner(logger, rule) { ShouldRunAllRules = true };
			runner.Process();

			AssertNotContains("The runner should not run at all because Buffer Management or better not enabled.", "The Tag Rule", logger.ToString());
			AssertContains("The registry item [Workflow Manager -> Buffer Management -> Workflow Management Mode] must be set to a higher level to use this processor.", logger.ToString());
		}

		public void TestProcess_WhenBasicWorkflowManagementEnabled_ShouldDoNothing()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.BasicWorkflow);
			var rule = BMSTestHelper.CreateTagRuleWithDefAndMag(Factory);
			Factory.Save();

			var logger = new TestServiceLogger();
			var runner = new DummyTagRuleRunner(logger, rule) { ShouldRunAllRules = true };
			runner.Process();

			AssertNotContains("The runner should not run at all because Buffer Management or better not enabled.", "The Tag Rule", logger.ToString());
			AssertContains("The registry item [Workflow Manager -> Buffer Management -> Workflow Management Mode] must be set to a higher level to use this processor.", logger.ToString());
		}

		#endregion
	}

	class TagRuleRunnerNonTransactionedTest : NonTransactionedTestCase
	{
		#region Error Handling

		public void TestRulesAreKeptActiveWhenReaderAccountIsDisabled()
		{
			using (var connection = Db.NewAdminConnection())
			{
				var tagDefinition = BMSTestHelper.CreateTagDefinition(Factory, "AAA");
				tagDefinition.TGD_UsageScope = TagUsageScopeList.Codes.Rule;
				var tagMagnitude = BMSTestHelper.CreateTagMagnitude(tagDefinition, "AAA");

				var rule = BMSTestHelper.CreateTagRule(tagMagnitude, "Add CC1", TagRuleActionTypeList.Codes.AddTag);
				FilterStripsTestHelper.AddFilterStrips(rule.Filter,
				new FilterStripsTestHelper.FilterStripDefinition
				{
					FilterStripName = "Custom SQL Filter",
					FilterStripValueSetter = f => ((ModuleSQLFilter)f).Property1 = "FH_CompletionStatement = 'peekaboo'",
				});

				AssertEquals(true, rule.TGR_IsActive);

				var workflow = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory).ProcessHeaders[0];
				workflow.FH_CompletionStatement = "peekaboo";

				Factory.Save();

				new ReaderDatabaseLogin(connection).DropUserFromDatabase(connection.CurrentDatabase);
				BMSTestCaseWithFactory.RunTagRules(rule);

				workflow = new BusinessObjectFactory().Load<ProcessHeader>(workflow.PK);
				var loadedRule = Factory.CreateNewFactory().Load<TagRule>(rule.PK);

				AssertNotNull("Rule should still exist", loadedRule);
				AssertEquals("If a connection was not able to be obtained we should not disable rules", true, rule.TGR_IsActive);
				var note = (StmNote)loadedRule.Notes.GetAllNotes().FirstOrDefault();
				AssertNull("Note is not added because the reader account is missing and it was not the fault of the rule creator", note);

				ErrorReporter.Clear();
			}
		}

		public void TestRuleValidation_ShouldNotRunTagRuleToValidateIfWeShouldRunTagRule()
		{
			var config = TestConfigsHelper.CreateTagsTestConfig(Factory);

			var userToNotify = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			userToNotify.GS_EmailAddress = "ooohgirl@rupaul.com";

			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.Staff.Add(userToNotify);

			BMSRegistry.Instance.NotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());

			var rule1 = BMSTestHelper.CreateTagRule(config.PrincessCelestiaTag, "Add Princess Celestia tag", TagRuleActionTypeList.Codes.AddTag);
			var rule2 = BMSTestHelper.CreateTagRule(config.RedTag, "Add Red tag", TagRuleActionTypeList.Codes.AddTag);

			FilterStripsTestHelper.AddFilterStrips(rule1.Filter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Custom SQL Filter",
				FilterStripValueSetter = f => ((ModuleSQLFilter)f).Property1 = "FH_Hahahahaha = 'peekaboo'",
			});

			FilterStripsTestHelper.AddFilterStrips(rule2.Filter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Custom SQL Filter",
				FilterStripValueSetter = f => ((ModuleSQLFilter)f).Property1 = "FH_Hahahahaha = 'peekaboo'",
			});

			Factory.Save();

			config.PonyTags.TGD_UsageScope = TagUsageScopeList.Codes.User; // Can't be applied in tag rules.

			Factory.Save();

			AssertEquals("Precondtion", false, config.PrincessCelestiaTag.Definition.CanRuleUseTags);
			AssertEquals("Precondtion", true, config.RedTag.Definition.CanRuleUseTags);

			var newFactory = Factory.CreateNewFactory();
			var loadedRule1 = newFactory.Load<TagRule>(rule1.PK);
			var loadedRule2 = newFactory.Load<TagRule>(rule2.PK);

			var serviceTask = new TagServiceTask { ServiceLogger = new DummyLogger() };

			serviceTask.RunTask();

			try
			{
				AssertEquals(false, loadedRule1.TGR_IsActive);
				AssertEquals(false, loadedRule2.TGR_IsActive);

				var rule1Note = (StmNote)loadedRule1.Notes.GetAllNotes().SingleOrDefault();
				var rule2Note = (StmNote)loadedRule2.Notes.GetAllNotes().SingleOrDefault();

				AssertNotNull(rule1Note);
				AssertNotNull(rule2Note);

				AssertMultilineASCIIEquals("rule1 isn't allowed to run because it's applying a tag with USR scope.",
					@"Could not run rule [Add Princess Celestia tag], because the rule is invalid:
Error - TGL_TGM_Magnitude: Cannot modify [CEL - Princess Celestia] from here. Tag Group has scope of [Tags in this group can only be applied or removed by users.].", rule1Note.ST_NoteDataAsText);

				AssertEquals("rule2 is allowed to add the tag, so it actually runs (we don't validate bad SQL in the service task).",
					"Error occurred while processing rule [Add Red tag]. Rule is now deactivated. Error message: Invalid column name 'FH_Hahahahaha'.", rule2Note.ST_NoteDataAsText);
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestHelper.EnableBMSInRegistry();
		}

		#endregion
	}

	class WorkQueueTagRuleRunnerTest : BMSTestCaseWithFactory
	{
		#region AddTag Rule

		public void TestAddTag_MultipleQueueMembership()
		{
			var queue1 = BMSTestHelper.CreateWorkQueue(Factory, "AAA", "aaa");
			var queue2 = BMSTestHelper.CreateWorkQueue(Factory, "BBB", "bbb");

			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "jobHeader1");
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "jobHeader2");

			RunTagRules(TagRuleActionTypeList.Codes.AddTag, new[] { queue1, queue2 });

			AssertTagApplied(jobHeader1, queue1, reloadInNewFactory: true);
			AssertTagNotApplied("Cannot add an item to multiple queues.", jobHeader1, queue2, reloadInNewFactory: true);

			AssertTagApplied(jobHeader2, queue1, reloadInNewFactory: true);
			AssertTagNotApplied("Cannot add an item to multiple queues.", jobHeader2, queue2, reloadInNewFactory: true);

			queue1 = AssertQueueCountInNewFactory(queue1, 2);
			AssertQueueCountInNewFactory(queue2, 0);

			AssertSamePK(jobHeader1, queue1.MembersInSequence.ElementAt(0));
			AssertSamePK(jobHeader2, queue1.MembersInSequence.ElementAt(1));
		}

		public void TestAddTag_AddingWorkflowsOfJobsAlreadyInQueue()
		{
			var queue = BMSTestHelper.CreateWorkQueue(Factory, "AAA", "aaa");

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2");

			RunTagRules(TagRuleActionTypeList.Codes.AddTag, new[] { queue });

			AssertTagApplied(jobHeader, queue, reloadInNewFactory: true);

			AssertTagNotApplied("Cannot add a workflow of a job already in the queue.", workflow1, queue, reloadInNewFactory: true);
			AssertTagNotApplied("Cannot add a workflow of a job already in the queue.", workflow2, queue, reloadInNewFactory: true);

			queue = AssertQueueCountInNewFactory(queue, 1);

			AssertSamePK(jobHeader, queue.MembersInSequence.ElementAt(0));
		}

		public void TestAddTag_AddingJobsOfWorkflowsAlreadyInQueue()
		{
			var queue = BMSTestHelper.CreateWorkQueue(Factory, "AAA", "aaa");

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2");

			RunTagRules(TagRuleActionTypeList.Codes.AddTag, new[] { queue }, workflowsOnly: true);

			AssertTagApplied(workflow1, queue, reloadInNewFactory: true);
			AssertTagApplied(workflow2, queue, reloadInNewFactory: true);
			AssertTagNotApplied(jobHeader, queue, reloadInNewFactory: true);

			RunTagRules(TagRuleActionTypeList.Codes.AddTag, new[] { queue }, jobHeadersOnly: true);

			AssertTagNotApplied("Cannot add the job of a workflow already in the queue.", jobHeader, queue, reloadInNewFactory: true);
			queue = AssertQueueCountInNewFactory(queue, 2);

			AssertSamePK(workflow1, queue.MembersInSequence.ElementAt(0));
			AssertSamePK(workflow2, queue.MembersInSequence.ElementAt(1));
		}

		public void TestAddTag_AddingWorkflowsOfJobsAlreadyInAnotherQueue()
		{
			var queue1 = BMSTestHelper.CreateWorkQueue(Factory, "AAA", "aaa");
			var queue2 = BMSTestHelper.CreateWorkQueue(Factory, "BBB", "bbb");

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2");

			RunTagRules(TagRuleActionTypeList.Codes.AddTag, new[] { queue1 }, jobHeadersOnly: true);

			AssertTagApplied(jobHeader, queue1);

			RunTagRules(TagRuleActionTypeList.Codes.AddTag, new[] { queue2 }, workflowsOnly: true);

			AssertTagNotApplied("Cannot add a workflow of a job already in another queue (AAA - aaa).", workflow1, queue2, reloadInNewFactory: true);
			AssertTagNotApplied("Cannot add a workflow of a job already in another queue (AAA - aaa).", workflow2, queue2, reloadInNewFactory: true);

			queue1 = AssertQueueCountInNewFactory(queue1, 1);
			queue2 = AssertQueueCountInNewFactory(queue2, 0);

			AssertSamePK(jobHeader, queue1.MembersInSequence.ElementAt(0));
		}

		public void TestAddTag_AddingJobsOfWorkflowsAlreadyInAnotherQueue()
		{
			var queue1 = BMSTestHelper.CreateWorkQueue(Factory, "AAA", "aaa");
			var queue2 = BMSTestHelper.CreateWorkQueue(Factory, "BBB", "bbb");

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2");

			RunTagRules(TagRuleActionTypeList.Codes.AddTag, new[] { queue1 }, workflowsOnly: true);

			AssertTagNotApplied(jobHeader, queue1, reloadInNewFactory: true);
			AssertTagApplied(workflow1, queue1, reloadInNewFactory: true);
			AssertTagApplied(workflow2, queue1, reloadInNewFactory: true);

			RunTagRules(TagRuleActionTypeList.Codes.AddTag, new[] { queue2 }, jobHeadersOnly: true);

			AssertTagNotApplied("Cannot add the job of a workflow already in a queue.", jobHeader, queue2, reloadInNewFactory: true);

			queue1 = AssertQueueCountInNewFactory(queue1, 2);
			queue2 = AssertQueueCountInNewFactory(queue2, 0);

			AssertSamePK(workflow1, queue1.MembersInSequence.ElementAt(0));
			AssertSamePK(workflow2, queue1.MembersInSequence.ElementAt(1));
		}

		public void TestAddTag_ShouldIncrementSequence()
		{
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "jobHeader1");
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "jobHeader2");
			var jobHeader3 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "jobHeader3");
			var jobHeader4 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "jobHeader4");

			jobHeader1.GetOrCreateDependencyLink(jobHeader2);
			jobHeader2.GetOrCreateDependencyLink(jobHeader3);
			jobHeader3.GetOrCreateDependencyLink(jobHeader4);

			var queue = BMSTestHelper.CreateWorkQueue(Factory, "AAA", "aaa");
			RunTagRules(TagRuleActionTypeList.Codes.AddTag, new[] { queue });

			AssertTagApplied(jobHeader1, queue, reloadInNewFactory: true);
			AssertTagApplied(jobHeader2, queue, reloadInNewFactory: true);
			AssertTagApplied(jobHeader3, queue, reloadInNewFactory: true);
			AssertTagApplied(jobHeader4, queue, reloadInNewFactory: true);

			Factory.Save();

			var loadedQueue = AssertQueueCountInNewFactory(queue, 4);

			AssertEquals(4, loadedQueue.Members.Count);
			AssertEquals(false, loadedQueue.HasChanges);

			BMSTestCaseWithFactory.AssertSamePK(jobHeader1, loadedQueue.MembersInSequence.ElementAt(0));
			BMSTestCaseWithFactory.AssertSamePK(jobHeader2, loadedQueue.MembersInSequence.ElementAt(1));
			BMSTestCaseWithFactory.AssertSamePK(jobHeader3, loadedQueue.MembersInSequence.ElementAt(2));
			BMSTestCaseWithFactory.AssertSamePK(jobHeader4, loadedQueue.MembersInSequence.ElementAt(3));
		}

		public void TestAddTag_ShouldLogTagEvent()
		{
			var queue = BMSTestHelper.CreateWorkQueue(Factory, "AAA", "aaa");

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow");

			var rule = RunTagRules(TagRuleActionTypeList.Codes.AddTag, new[] { queue }).Single();

			AssertTagApplied(jobHeader, queue);
			AssertTagEventRaised(jobHeader, TagActionType.AddTag, queue, rule);
		}

		public void TestRemoveTag_ShouldLogTagEvent()
		{
			var queue = BMSTestHelper.CreateWorkQueue(Factory, "AAA", "aaa");

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow");

			queue.AddMember(jobHeader);

			var rule = RunTagRules(TagRuleActionTypeList.Codes.RemoveTag, new[] { queue }).Single();

			AssertTagNotApplied(jobHeader, queue);
			AssertTagEventRaised(jobHeader, TagActionType.RemoveTag, queue, rule);
		}

		#endregion

		#region AddAndRemove (ARM) TagRule Action Code with RunBizoStrategy

		public void TestARMTagRuleActionCode_RunBizoStrategy()
		{
			var queue = BMSTestHelper.CreateWorkQueue(Factory, "AAA", "aaa");

			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = CreateWorkflow(jobHeader, "workflow2");

			queue.AddMember(workflow2);
			workflow1.FH_CompletionStatement = "This one!";
			workflow2.FH_CompletionStatement = "Not this one?";

			var tagRule = BMSTestHelper.CreateTagRule(queue, "Add me tag", TagRuleActionTypeList.Codes.AddAndRemoveTag);
			FilterStripsTestHelper.AddFilterStrip<ModuleTextFilter>(tagRule.Filter, ProcessHeader.ModuleFilterConstants.CompletionStatement, f => f.Property = "This one!");

			Factory.Save();

			workflow1 = new BusinessObjectFactory().Load<ProcessHeader>(workflow1.PK);
			workflow2 = new BusinessObjectFactory().Load<ProcessHeader>(workflow2.PK);

			AssertTagNotApplied("Tag yet to be applied", workflow1, queue, reloadInNewFactory: true);
			AssertTagApplied("Tag yet to be removed", workflow2, queue, reloadInNewFactory: true);
			AssertEquals("Sequence should start at 1", (ZShort)1, queue.Members[0].TGL_Sequence);

			RunTagRules(tagRule);

			workflow1 = new BusinessObjectFactory().Load<ProcessHeader>(workflow1.PK);
			workflow2 = new BusinessObjectFactory().Load<ProcessHeader>(workflow2.PK);

			AssertTagApplied("Should add tag to matching item", workflow1, queue, reloadInNewFactory: true);
			AssertTagNotApplied("Should remove tag from non-matching item", workflow2, queue, reloadInNewFactory: true);
			AssertEquals("Sequence should remain at 1", (ZShort)1, queue.Members[0].TGL_Sequence);
		}

		public void TestARMTagRuleActionCode_RunBizoStrategy_CorrectSequencing()
		{
			var queue = BMSTestHelper.CreateWorkQueue(Factory, "AAA", "aaa");

			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = CreateWorkflow(jobHeader, "workflow2");
			var workflow3 = CreateWorkflow(jobHeader, "workflow3");

			queue.AddMember(workflow1);
			queue.AddMember(workflow2);
			workflow1.FH_CompletionStatement = "This one!";
			workflow2.FH_CompletionStatement = "Not this one?";
			workflow3.FH_CompletionStatement = "This one!";

			var tagRule = BMSTestHelper.CreateTagRule(queue, "Add me tag", TagRuleActionTypeList.Codes.AddAndRemoveTag);
			FilterStripsTestHelper.AddFilterStrip<ModuleTextFilter>(tagRule.Filter, ProcessHeader.ModuleFilterConstants.CompletionStatement, f => f.Property = "This one!");

			Factory.Save();

			workflow1 = new BusinessObjectFactory().Load<ProcessHeader>(workflow1.PK);
			workflow2 = new BusinessObjectFactory().Load<ProcessHeader>(workflow2.PK);
			workflow3 = new BusinessObjectFactory().Load<ProcessHeader>(workflow3.PK);

			var members = queue.Members.OrderBy(m => m.TGL_Sequence).ToArray();

			AssertTagApplied("Tag yet to be removed", workflow1, queue, reloadInNewFactory: true);
			AssertTagApplied("Tag yet to be removed", workflow2, queue, reloadInNewFactory: true);
			AssertTagNotApplied("Tag yet to be applied", workflow3, queue, reloadInNewFactory: true);
			AssertEquals("Sequence should start at 1", (ZShort)1, members[0].TGL_Sequence);
			AssertEquals("Sequence should continue to 2", (ZShort)2, members[1].TGL_Sequence);

			RunTagRules(tagRule);

			workflow1 = new BusinessObjectFactory().Load<ProcessHeader>(workflow1.PK);
			workflow2 = new BusinessObjectFactory().Load<ProcessHeader>(workflow2.PK);
			workflow3 = new BusinessObjectFactory().Load<ProcessHeader>(workflow3.PK);

			AssertTagApplied("Should remain in the queue", workflow1, queue, reloadInNewFactory: true);
			AssertTagNotApplied("Should remove tag from non-matching item", workflow2, queue, reloadInNewFactory: true);
			AssertTagApplied("Should add tag to matching item", workflow3, queue, reloadInNewFactory: true);

			members = queue.Members.OrderBy(m => m.TGL_Sequence).ToArray();

			AssertEquals("Sequence should remain at 1", (ZShort)1, members[0].TGL_Sequence);
			AssertEquals("Correct member at sequence", workflow1.PK, queue.MembersInSequence.ElementAt(0).PK);
			AssertEquals("New member sequence at 2", (ZShort)2, members[1].TGL_Sequence);
			AssertEquals("Correct member at sequence", workflow3.PK, queue.MembersInSequence.ElementAt(1).PK);
		}

		public void TestARMTagRuleActionCode_RunBizoStrategy_CorrectSequencingMultipleQueue()
		{
			var queue1 = BMSTestHelper.CreateWorkQueue(Factory, "AAA", "aaa");
			var queue2 = BMSTestHelper.CreateWorkQueue(Factory, "BBB", "bbb");

			var rule1 = BMSTestHelper.CreateTagRule(queue1, queue1.TGM_Description, TagRuleActionTypeList.Codes.AddAndRemoveTag);
			FilterStripsTestHelper.AddFilterStrips(rule1.Filter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Q1",
			});

			var rule2 = BMSTestHelper.CreateTagRule(queue2, queue2.TGM_Description, TagRuleActionTypeList.Codes.AddAndRemoveTag);
			FilterStripsTestHelper.AddFilterStrips(rule2.Filter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Q2",
			});

			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = CreateWorkflow(jobHeader, "Q2");
			var workflow2 = CreateWorkflow(jobHeader, "Q2");

			queue1.AddMember(workflow1);
			queue1.AddMember(workflow2);

			Factory.Save();

			workflow1 = new BusinessObjectFactory().Load<ProcessHeader>(workflow1.PK);
			workflow2 = new BusinessObjectFactory().Load<ProcessHeader>(workflow2.PK);

			var members = queue1.Members.OrderBy(m => m.TGL_Sequence).ToArray();

			AssertTagApplied("Tag yet to be removed", workflow1, queue1, reloadInNewFactory: true);
			AssertTagApplied("Tag yet to be removed", workflow2, queue1, reloadInNewFactory: true);
			AssertTagNotApplied("Tag yet to be applied", workflow1, queue2, reloadInNewFactory: true);
			AssertTagNotApplied("Tag yet to be applied", workflow2, queue2, reloadInNewFactory: true);
			AssertEquals("Sequence should start at 1", (ZShort)1, members[0].TGL_Sequence);
			AssertEquals("Sequence should continue to 2", (ZShort)2, members[1].TGL_Sequence);

			RunTagRules(rule1, rule2);

			members = queue2.Members.OrderBy(m => m.TGL_Sequence).ToArray();

			AssertTagNotApplied("Tag removed", workflow1, queue1, reloadInNewFactory: true);
			AssertTagNotApplied("Tag removed", workflow2, queue1, reloadInNewFactory: true);
			AssertTagApplied("Tag applied", workflow1, queue2, reloadInNewFactory: true);
			AssertTagApplied("Tag applied", workflow2, queue2, reloadInNewFactory: true);
			AssertEquals("Sequence should start at 1", (ZShort)1, members[0].TGL_Sequence);
			AssertEquals("Sequence should continue to 2", (ZShort)2, members[1].TGL_Sequence);

			var workflow3 = CreateWorkflow(jobHeader, "Q1");
			var workflow4 = CreateWorkflow(jobHeader, "Q1");

			Factory.Save();

			AssertTagNotApplied("Tag yet to be applied", workflow3, queue1, reloadInNewFactory: true);
			AssertTagNotApplied("Tag yet to be applied", workflow4, queue1, reloadInNewFactory: true);
			AssertTagNotApplied("Tag yet to be applied", workflow3, queue2, reloadInNewFactory: true);
			AssertTagNotApplied("Tag yet to be applied", workflow4, queue2, reloadInNewFactory: true);

			rule1.Schedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddMinutes(-1);
			rule2.Schedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddMinutes(-1);
			RunTagRules(rule1, rule2);

			members = queue1.Members.OrderBy(m => m.TGL_Sequence).ToArray();

			AssertTagApplied("Tag applied", workflow3, queue1, reloadInNewFactory: true);
			AssertTagApplied("Tag applied", workflow4, queue1, reloadInNewFactory: true);
			AssertTagNotApplied("Tag not applied", workflow3, queue2, reloadInNewFactory: true);
			AssertTagNotApplied("Tag not applied", workflow4, queue2, reloadInNewFactory: true);
			AssertEquals("Sequence should start at 1", (ZShort)1, members[0].TGL_Sequence);
			AssertEquals("Sequence should continue to 2", (ZShort)2, members[1].TGL_Sequence);
		}

		#endregion

		#region Implementation

		List<TagRule> RunTagRules(string actionType, WorkQueue[] queues, bool workflowsOnly = false, bool jobHeadersOnly = false)
		{
			var rules = new List<TagRule>();

			foreach (var queue in queues)
			{
				var rule = BMSTestHelper.CreateTagRule(queue, queue.TGM_Description + ++filterIndex_forNameUniqueness, actionType);
				rules.Add(rule);

				AddFilterStrips(rule.Filter, workflowsOnly, jobHeadersOnly);
			}

			Factory.Save();

			BMSTestCaseWithFactory.RunTagRules(rules.ToArray());

			return rules;
		}

		void AddFilterStrips(StmModuleFilter filter, bool workflowsOnly = false, bool jobHeadersOnly = false)
		{
			var filterStripDefinitions = new List<FilterStripsTestHelper.FilterStripDefinition>();

			filterStripDefinitions.Add(new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.JobOrWorkflow,
				FilterStripValueSetter = f =>
				{
					var filterStrip = (ModuleFlagsFilter)f;

					if (jobHeadersOnly && !workflowsOnly)
					{
						filterStrip.Property0 = true; // Jobs only
					}
					else if (workflowsOnly && !jobHeadersOnly)
					{
						filterStrip.Property1 = true; // Workflows only
					}
					else if (workflowsOnly && jobHeadersOnly)
					{
						filterStrip.Property2 = true;
					}
				}
			});

			var filterBizo = RelatedModuleFiltersHelper.GetNewFilterBusinessObject(ModuleIDs.BMFilterRule);
			var query = filterBizo.GetFilterWhere(filter.PK, f => f.Visibility != FilterVisibility.AlwaysApplied, filter.Factory);
			if (query == null || query.IsEmpty)
			{
				filterStripDefinitions.Add(new FilterStripsTestHelper.FilterStripDefinition
				{
					FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
					FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Purple monkey dishwasher, etc.", // To prevent validation errors for empty queries
					ComparisonOperatorSetter = f => ((ModuleTextFilter)f).ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual
				});
			}

			FilterStripsTestHelper.AddFilterStrips(filter, filterStripDefinitions.ToArray());
		}

		static WorkQueue AssertQueueCountInNewFactory(WorkQueue queue, int expectedCount)
		{
			var factory = queue.Factory.CreateNewFactory();
			var loadedQueue = factory.Load<WorkQueue>(queue.PK);

			AssertEquals(expectedCount, loadedQueue.Members.Count);
			AssertEquals(false, loadedQueue.HasChanges);

			return loadedQueue;
		}

		int filterIndex_forNameUniqueness;

		#endregion
	}
}
