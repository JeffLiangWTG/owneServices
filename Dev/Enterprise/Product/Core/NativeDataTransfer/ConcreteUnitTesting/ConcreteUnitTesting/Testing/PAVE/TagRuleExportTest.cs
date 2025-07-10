using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.ConcreteUnitTesting
{
	class TagRuleExportTest : TestCaseWithFactory
	{
		[TestDate(2016, 05, 30)]
		public void TestExportTagRuleIncludesTagLinkCorrectlyLinked()
		{
			var definition = Factory.New<TagDefinition>();
			definition.TGD_Code = "FAP";
			var magnitude = Factory.New<TagMagnitude>();
			magnitude.TGM_Code = "FOP";
			magnitude.TGM_TGD_Tag = definition.PK;

			var tagRule = Factory.New<TagRule>();
			tagRule.TGR_Name = "one rule to rule them all";
			var template = tagRule.TagRuleTemplate;
			template.TGL_TGM_Magnitude = magnitude.PK;
			template.TGL_ParentTableCode = TagRuleSchema.Constants.Prefix;

			var filter = tagRule.Filter;
			filter.S9_FilterData = new ZBlob(new byte[] { 1, 2 });

			var userValues = filter.GetOrCreateLayoutUserData(new FilterStripLayoutsHelper());
			userValues.S0_FilterDataValues = new ZBlob(new byte[] { 1, 2 });

			Factory.Save();

			string actualMessage = "";
			using (var dataStream = NativeDataTransferTestHelper.ExportToStream(tagRule))
			using (var reader = new StreamReader(dataStream))
			{
				actualMessage = reader.ReadToEnd();
			}

			AssertContains("TagLink should have action = merge", "<TagLink Action=\"MERGE\">", actualMessage);
		}

		public void TestExportAndImportMultipleTagRules_ShouldNotShareFiltersBetweenRules()
		{
			var config = TestConfigsHelper.CreateTagsTestConfig(Factory);
			var rule1 = BMSTestHelper.CreateTagRule(config.PlatinumTag, "Add Platinum", TagRuleActionTypeList.Codes.AddTag);
			var rule2 = BMSTestHelper.CreateTagRule(config.PlatinumTag, "Remove Platinum", TagRuleActionTypeList.Codes.RemoveTag);

			FilterStripsTestHelper.AddCustomSQLFilterStrip(rule1.Filter, "1=2");
			FilterStripsTestHelper.AddCustomSQLFilterStrip(rule2.Filter, "2=3");

			Factory.Save();

			using (var rule1Stream = NativeDataTransferTestHelper.ExportToStream(rule1))
			using (var rule2Stream = NativeDataTransferTestHelper.ExportToStream(rule2))
			{
				// Delete existing tag rules so that we don't conflict with the unique index on TGR_Name
				rule1.Delete();
				rule2.Delete();
				Factory.Save();

				var log1 = NativeDataTransferTestHelper.ImportAndGetInsertLog(rule1Stream);
				var log2 = NativeDataTransferTestHelper.ImportAndGetInsertLog(rule2Stream);

				const string expectedImportLog =
@"--- Start Import Process --------------------------------------------------------------
Processed: TagRule
--- Import Process Finished -----------------------------------------------------------
TagRule - 1 inserts, 0 updates, 0 deletes
StmModuleFilter - 1 inserts, 0 updates, 0 deletes
StmModuleFilterUserData - 1 inserts, 0 updates, 0 deletes
TagLink - 1 inserts, 0 updates, 0 deletes";

				AssertMultilineASCIIEquals("Import log for rule1", expectedImportLog, log1);
				AssertMultilineASCIIEquals("Import log for rule2", expectedImportLog, log2);
			}

			var newFactory = Factory.CreateNewFactory();
			var loadedRule1 = newFactory.LoadTop1<TagRule>(new ZQuery(TagRuleSchema.TGR_Name, "Add Platinum"));
			var loadedRule2 = newFactory.LoadTop1<TagRule>(new ZQuery(TagRuleSchema.TGR_Name, "Remove Platinum"));

			AssertNotNull(loadedRule1);
			AssertNotNull(loadedRule2);
			AssertNotEquals(rule1.PK, loadedRule1.PK);
			AssertNotEquals(rule2.PK, loadedRule2.PK);

			AssertEquals("Filter should have been created during the import process, not lazily during this test", true, loadedRule1.Filter.IsInDatabase);
			AssertEquals("Filter should have been created during the import process, not lazily during this test", true, loadedRule2.Filter.IsInDatabase);

			AssertNotEquals("Filter should not be shared between the two new rules", loadedRule1.Filter, loadedRule2.Filter);
		}
	}
}
