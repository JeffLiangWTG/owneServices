using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.BufferManagement.Module;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.ZArchitecture.GUI.Testing.FilterStripsTestHelper;

namespace Enterprise.BufferManagement.GUI.Test
{
	public class TagRuleRunnerCompanyFilterTest : BMSTestCaseWithFactory
	{
		#region Allow Company Filters In Tag Rules Registry

		[TestDate(2000, 1, 1, 8, 0, 0)]
		public void TestAllowCompanyFiltersInTagRulesRegistry()
		{
			using (EnvProxy.Instance.SetTemporaryUserContext("CWService", Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			using (EnvProxy.Instance.TemporaryServiceTaskContext(TagServiceTask.Code, true))
			{
				CombineAssertions(() =>
				{
					foreach (var moduleName in GetValidParentJobModuleFilters(Factory))
					{
						workflow.RemoveTag(tagMagnitude);
						SetFilterStrips(tagRule.Filter, moduleName: moduleName);
						Factory.Save();

						RunTagRuleAndAssertCompanyFilter(
							"WHEN AllowCompanyFiltersInTagRulesRegistry = false THEN company filter should not exist",
							moduleName,
							allowCompanyFiltersInTagRules: false);

						RunTagRuleAndAssertCompanyFilter(
							"WHEN AllowCompanyFiltersInTagRulesRegistry = true THEN company filter can exist",
							moduleName,
							allowCompanyFiltersInTagRules: true);
					}
				});
			}
		}

		#endregion

		#region Implementation

		void RunTagRuleAndAssertCompanyFilter(string message, string moduleName, bool allowCompanyFiltersInTagRules)
		{
			//We're about to modify the filters of an FSBO mid-test, so clear cache.
			ModuleFilter.ClearSelectedFiltersCache();

			BMSRegistry.Instance.AllowCompanyFiltersInTagRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, allowCompanyFiltersInTagRules);

			using (TestConnection.TrackExecutedCommands())
			{
				RunTagRulesRegardlessOfLastRunTimeConsiderations(tagRule);

				message = $"{message}: Module = '{moduleName}', AllowCompanyFiltersInTagRules = {allowCompanyFiltersInTagRules}, ";

				AssertTagApplied($"{message}THEN tag should be applied", workflow, tagMagnitude);

				if (allowCompanyFiltersInTagRules)
				{
					AssertCompanyFilterCanExist(message, moduleName, TestConnection.ExecutedCommands);
				}
				else
				{
					AssertCompanyFilter(message, TestConnection.ExecutedCommands, expectCompanyFilter: false);
				}
			}
		}

		static void AssertCompanyFilterCanExist(string message, string moduleName, IEnumerable<string> sqlCommands)
		{
			var filterBizo = new ProcessHeaderFilterBusinessObject();

			var parentJobFilter = (ModuleGuidModuleSpecifiedFilter)filterBizo["Parent Job"];
			parentJobFilter.SelectedModule = moduleName;
			parentJobFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch;
			parentJobFilter.IsActive = true;

			var isCompanyFilterExistInNonTagRuleRunner = IsCompanyRelatedFilterExist(filterBizo.Filter.LiteralTextADO);
			var tagRuleSQLCommands = GetTagRuleSQLCommands(sqlCommands);

			AssertEquals(
				$"{message}Company Filter can exist",
				isCompanyFilterExistInNonTagRuleRunner,
				tagRuleSQLCommands.All(sqlCommand => IsCompanyRelatedFilterExist(sqlCommand)));
		}

		IEnumerable<string> GetValidParentJobModuleFilters(BusinessObjectFactory factory)
		{
			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var filter = (ModuleGuidModuleSpecifiedFilter)filterBizo["Parent Job"];

			return filter
				.ModuleOptions
				.GetAllCodes()
				.Where(moduleName => !exclusionParentJobModuleFilters.Contains(moduleName));
		}

		readonly HashSet<string> exclusionParentJobModuleFilters = new HashSet<string>() {
			"AccCollectionBatch",
			"AccCollectionOrder",
			"AccComplianceReport",
			"AccComplianceSequence",
			"AccPayableOrder",
			"APComplianceDocument",
			"ARComplianceDocument",
			"ClientRates",
			"EntryHeader",
			"EUH7",
			"ExitControl",
			"GlbCompanyCampaign",
			"GlobalRates",
			"GoodsCatalog",
			"HRGlbCompanyCampaign",
			"NctsMovementModule",
			"Quotations",
			"QuotedBookings",
			"USeManifest",
			"USInBond",
			"AUCustomsHouseSeaCargo"
		};

		public static void AssertCompanyFilter(string message, IEnumerable<string> sqlCommands, bool expectCompanyFilter)
		{
			var tagRuleSQLCommands = GetTagRuleSQLCommands(sqlCommands);
			Assert($"{message}, tag-rule filter should exist", tagRuleSQLCommands.Any());

			AssertEquals(
				$"{message}, company filter {(expectCompanyFilter ? "should" : "should not")} exist: \r\n\r\n{string.Join("\r\n\r\n-------------------- Query -------------------\r\n\r\n", tagRuleSQLCommands)}",
				expectCompanyFilter,
				tagRuleSQLCommands.All(sqlCommand => IsCompanyRelatedFilterExist(sqlCommand)));
		}

		public static void SetFilterStrips(StmModuleFilter tagRuleFilter, string moduleName, string completionStatementFilter = "Workflow N")
		{
			var parentJobFilter = new FilterStripDefinition
			{
				FilterStripName = "Parent Job",
				FilterStripValueSetter = filter =>
				{
					var moduleGuidModuleSpecifiedFilter = (ModuleGuidModuleSpecifiedFilter)filter;
					moduleGuidModuleSpecifiedFilter.SelectedModule = moduleName;
					moduleGuidModuleSpecifiedFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch;
				},
				OrCategory = FilterOrCategory.Red
			};

			var customSQLFilter = new FilterStripDefinition
			{
				FilterStripName = "Custom SQL Filter",
				FilterStripValueSetter = (filter) => ((ModuleSQLFilter)filter).Property1 = $"FH_CompletionStatement = '{completionStatementFilter}'",
				OrCategory = FilterOrCategory.Red
			};

			FilterStripsTestHelper.AddFilterStrips(
				tagRuleFilter,
				new[] { customSQLFilter, parentJobFilter });
		}

		static IEnumerable<string> GetTagRuleSQLCommands(IEnumerable<string> sqlCommands)
		{
			return sqlCommands.Where(sqlCommand => sqlCommand.Contains("Workflow N"));
		}

		static bool IsCompanyRelatedFilterExist(string sqlCommand)
		{
			return sqlCommand.Contains(Env.CurrentCompanyPK.ToString()) || (sqlCommand.Contains(GlbBranchSchema.GB_RL_NKHomePort.Name));
		}

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestHelper.EnableBMSInRegistry();

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var tagDefintion = BMSTestHelper.CreateTagDefinition(Factory, "TAG");
			tagMagnitude = BMSTestHelper.CreateTagMagnitude(tagDefintion, "TMG");
			tagRule = BMSTestHelper.CreateTagRule(tagMagnitude, "Tag Rule N", TagRuleActionTypeList.Codes.AddTag, 100m);

			workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			workflow.FH_CompletionStatement = "Workflow N";

			Factory.Save();
		}

		TagMagnitude tagMagnitude;
		TagRule tagRule;

		ProcessHeader workflow;

		#endregion
	}
}
