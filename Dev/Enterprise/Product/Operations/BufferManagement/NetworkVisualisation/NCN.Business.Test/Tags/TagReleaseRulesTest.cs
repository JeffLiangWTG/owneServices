using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	[TestDate(2014, 6, 11)]
	abstract class TagReleaseRulesTest : NetworkTestCase
	{
		#region System-Defined Tags/Rules Tests

		public void TestTagsExist()
		{
			var tagGroup = GetTagGroup();
			AssertNotNull(tagGroup);
			AssertEquals(TagRulePrefix + " Release", tagGroup.TGD_Description);
			AssertEquals(true, tagGroup.TGD_IsSystem);
			AssertEquals(true, tagGroup.TGD_IsExclusive);

			AssertEquals(2, tagGroup.Magnitudes.Count);

			var readyToReleaseTag = GetReadyToReleaseTag(tagGroup);
			AssertEquals("Ready to Release", readyToReleaseTag.TGM_Description);

			var releaseBlockedTag = GetReleaseBlockedTag(tagGroup);
			AssertEquals("Release Blocked", releaseBlockedTag.TGM_Description);

			AssertEquals(0, readyToReleaseTag.TGM_RuleRunSequence);
			AssertEquals(0, releaseBlockedTag.TGM_RuleRunSequence);
		}

		public void TestRulesExist()
		{
			var tagGroup = GetTagGroup();
			var ruleQuery = GetTagRuleBaseQuery(new ZQuery(TagRuleSchema.TGR_Name, SQLComparisonOperator.StartsWith, TagRulePrefix + " "));
			var rules = Factory.Load<TagRule>(ruleQuery);
			AssertEquals(2, rules.Length);

			var readyToReleaseRule = rules.Single(r => r.TGR_Name == TagRulePrefix + " Ready to Release Rule");
			AssertRuleDetails(readyToReleaseRule, TagRuleActionTypeList.Codes.AddAndRemoveTag, GetReadyToReleaseTag(tagGroup));

			var releaseBlockedRule = rules.Single(r => r.TGR_Name == TagRulePrefix + " Release Blocked");
			AssertRuleDetails(releaseBlockedRule, TagRuleActionTypeList.Codes.AddAndRemoveTag, GetReleaseBlockedTag(tagGroup));

			AssertEquals(false, rules.Any(r => r.TGR_Name == TagRulePrefix + " No longer Ready to Release Rule"));
			AssertEquals(false, rules.Any(r => r.TGR_Name == TagRulePrefix + " No longer Blocked Rule"));
		}

		#endregion

		#region Assertions

		protected static void AssertRuleDetails(TagRule rule, string action, TagMagnitude tag)
		{
			AssertEquals(action, rule.TGR_ActionType);
			AssertEquals(tag.PK, rule.TagTemplate.TGL_TGM_Magnitude);
		}

		#endregion

		#region Helper Methods

		protected TagDefinition GetTagGroup()
		{
			var query = new ZQuery(TagDefinitionSchema.TGD_IsSystem, true);
			query.AddToFilter(TagDefinitionSchema.TGD_Code, TagDefinitionCode);

			return Factory.LoadTop1<TagDefinition>(query);
		}

		protected TagMagnitude GetReadyToReleaseTag(TagDefinition tagGroup)
		{
			return tagGroup.Magnitudes.Single(m => m.TGM_Code == BMConstants.ReadyToReleaseTagCode);
		}

		protected TagMagnitude GetReleaseBlockedTag(TagDefinition tagGroup)
		{
			return tagGroup.Magnitudes.Single(m => m.TGM_Code == BMConstants.ReleaseBlockedTagCode);
		}

		protected TagRule GetReadyToReleaseRule()
		{
			return Factory.LoadTop1<TagRule>(GetTagRuleBaseQuery(new ZQuery(TagRuleSchema.TGR_Name, TagRulePrefix + " Ready to Release Rule")));
		}

		protected TagRule GetReleaseBlockedRule()
		{
			return Factory.LoadTop1<TagRule>(GetTagRuleBaseQuery(new ZQuery(TagRuleSchema.TGR_Name, TagRulePrefix + " Release Blocked")));
		}

		static ZQuery GetTagRuleBaseQuery(ZQuery additionalQuery)
		{
			var query = new ZQuery(TagRuleSchema.TGR_IsSystem, true);
			query.AddToFilter(additionalQuery);

			return query;
		}

		#endregion

		protected abstract string TagDefinitionCode { get; }

		protected abstract string TagRulePrefix { get; }

		protected override void SetUp()
		{
			base.SetUp();

			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
		}
	}
}
