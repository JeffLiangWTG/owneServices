using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(BMTagRuleFilterBusinessObject))]
	class BMTagRuleFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestOwnerGroup()
		{
			var factory = new BusinessObjectFactory();

			var definition1 = BMSTestHelper.CreateTagDefinition(factory, "AAA");
			var tagMagnitude1 = BMSTestHelper.CreateTagMagnitude(definition1, "Ma1");
			var ownerGroup1 = factory.NewWithValidTestData<GlbGroup>();
			tagMagnitude1.TGM_GG_OwnerGroup = ownerGroup1.PK;
			var rule1 = BMSTestHelper.CreateTagRule(tagMagnitude1, "Rule1", TagRuleActionTypeList.Codes.AddTag);
			rule1.TagTemplate.TGL_TGM_Magnitude = tagMagnitude1.PK;

			var definition2 = BMSTestHelper.CreateTagDefinition(factory, "BBB");
			var tagMagnitude2 = BMSTestHelper.CreateTagMagnitude(definition2, "Ma2");
			var ownerGroup2 = factory.NewWithValidTestData<GlbGroup>();
			tagMagnitude2.TGM_GG_OwnerGroup = ownerGroup2.PK;
			var rule2 = BMSTestHelper.CreateTagRule(tagMagnitude2, "Rule2", TagRuleActionTypeList.Codes.AddTag);
			rule2.TagTemplate.TGL_TGM_Magnitude = tagMagnitude2.PK;

			factory.Save();

			var bizo = new BMTagRuleFilterBusinessObject();
			var filterStrip = (ModuleGuidFilter)bizo["Owner Group"];
			filterStrip.IsActive = true;
			filterStrip.Property = ownerGroup1.PK;

			var rules = factory.Load<TagRule>(bizo.Filter);
			AssertEquals(1, rules.Length);
			AssertCollectionContains(rule1, rules);
		}

		public void TestTagRuleName()
		{
			var definition1 = BMSTestHelper.CreateTagDefinition(Factory, "AAA");
			var tagMagnitude1 = BMSTestHelper.CreateTagMagnitude(definition1, "Ma1");
			var rule1 = BMSTestHelper.CreateTagRule(tagMagnitude1, "Later Ron", TagRuleActionTypeList.Codes.AddTag);
			var rule2 = BMSTestHelper.CreateTagRule(tagMagnitude1, "Right Now", TagRuleActionTypeList.Codes.AddTag);

			Factory.Save();

			var bizo = new BMTagRuleFilterBusinessObject();
			var filterStrip = (ModuleTextFilter)bizo["Name"];
			filterStrip.IsActive = true;
			filterStrip.Property = "Later Ron";

			var rules = Factory.Load<TagRule>(bizo.Filter);
			AssertEquals(1, rules.Length);
			AssertCollectionContains(rule1, rules);
		}

		public void TestTagRuleAction()
		{
			var definition1 = BMSTestHelper.CreateTagDefinition(Factory, "AAA");
			var tagMagnitude1 = BMSTestHelper.CreateTagMagnitude(definition1, "Ma1");
			var rule1 = BMSTestHelper.CreateTagRule(tagMagnitude1, "Later Ron", TagRuleActionTypeList.Codes.AddTag);
			var rule2 = BMSTestHelper.CreateTagRule(tagMagnitude1, "Right Now", TagRuleActionTypeList.Codes.RemoveTag);

			Factory.Save();

			var bizo = new BMTagRuleFilterBusinessObject();
			var filterStrip = (ModuleTextFilter)bizo["Action Type"];
			filterStrip.IsActive = true;
			filterStrip.Property = TagRuleActionTypeList.Codes.AddTag;

			var rules = Factory.Load<TagRule>(bizo.Filter);
			AssertCollectionContains(rule1, rules);
		}

		public void TestTagMagnitude()
		{
			var definition = Factory.NewWithValidTestData<TagDefinition>();
			var magnitude1 = BMSTestHelper.CreateTagMagnitude(definition, "BAS");
			var magnitude2 = BMSTestHelper.CreateTagMagnitude(definition, "QUA");

			var rule1 = Factory.NewWithValidTestData<TagRule>();
			rule1.TagTemplate.TGL_TGM_Magnitude = magnitude1.PK;
			var rule2 = Factory.NewWithValidTestData<TagRule>();
			rule2.TagTemplate.TGL_TGM_Magnitude = magnitude2.PK;

			Factory.Save();

			var bizo = new BMTagRuleFilterBusinessObject();
			var filterStrip = (ModuleGuidFilter)bizo["Tag Magnitude"];
			filterStrip.IsActive = true;
			filterStrip.Property = magnitude1.PK;

			var rules = Factory.Load<TagRule>(bizo.Filter);
			AssertEquals(1, rules.Length);
			AssertCollectionContains(rule1, rules);
		}

		public void TestTagGroupFilter()
		{
			var definition1 = Factory.NewWithValidTestData<TagDefinition>();
			var magnitude1_1 = BMSTestHelper.CreateTagMagnitude(definition1, "BAS");
			var magnitude1_2 = BMSTestHelper.CreateTagMagnitude(definition1, "QUA");

			var definition2 = Factory.NewWithValidTestData<TagDefinition>();
			var magnitude2_1 = BMSTestHelper.CreateTagMagnitude(definition2, "BOO");

			var rule1 = Factory.NewWithValidTestData<TagRule>();
			rule1.TagTemplate.TGL_TGM_Magnitude = magnitude1_1.PK;
			var rule2 = Factory.NewWithValidTestData<TagRule>();
			rule2.TagTemplate.TGL_TGM_Magnitude = magnitude1_2.PK;
			var rule3 = Factory.NewWithValidTestData<TagRule>();
			rule3.TagTemplate.TGL_TGM_Magnitude = magnitude2_1.PK;

			Factory.Save();

			var bizo = new BMTagRuleFilterBusinessObject();
			var filterStrip = (ModuleGuidFilter)bizo["Tag Group"];
			filterStrip.IsActive = true;
			filterStrip.Property = definition1.PK;

			var rules = Factory.Load<TagRule>(bizo.Filter);
			AssertEquals(2, rules.Length);
			AssertCollectionContains(rule1, rules);
			AssertCollectionContains(rule2, rules);

			filterStrip.Property = definition2.PK;
			rules = Factory.Load<TagRule>(bizo.Filter);
			AssertEquals(1, rules.Length);
			AssertCollectionContains(rule3, rules);
		}

		public void TestSystemFilter()
		{
			var rule1 = Factory.NewWithValidTestData<TagRule>();
			var rule2 = Factory.NewWithValidTestData<TagRule>();
			rule2.TGR_IsSystem = true;

			var bizo = new BMTagRuleFilterBusinessObject();
			bizo.QueryObjectType = typeof(TagRule);
			var filterStrip = (ModuleTextFilter)bizo["Is System Defined"];
			filterStrip.IsActive = true;

			filterStrip.Property = "System";
			var rules = Factory.Load<TagRule>(bizo.Filter);
			AssertCollectionContains(rule2, rules);
			AssertCollectionNotContains(rule1, rules);

			filterStrip.Property = "Not System";
			rules = Factory.Load<TagRule>(bizo.Filter);
			AssertCollectionContains(rule1, rules);
			AssertCollectionNotContains(rule2, rules);
		}

		public void TestLastRunDurationInSeconds()
		{
			var definition1 = BMSTestHelper.CreateTagDefinition(Factory, "AAA");
			var tagMagnitude1 = BMSTestHelper.CreateTagMagnitude(definition1, "Ma1");
			var rule1 = BMSTestHelper.CreateTagRule(tagMagnitude1, "Later Ron", TagRuleActionTypeList.Codes.AddTag);
			var rule2 = BMSTestHelper.CreateTagRule(tagMagnitude1, "Right Now", TagRuleActionTypeList.Codes.AddTag);
			rule1.TGR_LastRunDurationInSeconds = 10;
			rule2.TGR_LastRunDurationInSeconds = 20;

			Factory.Save();

			var bizo = new BMTagRuleFilterBusinessObject();
			var filterStrip = (ModuleNumberRangeFilter)bizo["Last Run Duration"];
			filterStrip.IsActive = true;
			filterStrip.Property1 = 15;
			filterStrip.Property2 = 25;

			var rules = Factory.Load<TagRule>(bizo.Filter);
			AssertEquals(1, rules.Length);
			AssertCollectionContains(rule2, rules);
		}

		[TestDate(2015, 2, 15)]
		public void TestLastRunStartTime()
		{
			var definition1 = BMSTestHelper.CreateTagDefinition(Factory, "AAA");
			var tagMagnitude1 = BMSTestHelper.CreateTagMagnitude(definition1, "Ma1");
			var rule1 = BMSTestHelper.CreateTagRule(tagMagnitude1, "Later Ron", TagRuleActionTypeList.Codes.AddTag);
			var rule2 = BMSTestHelper.CreateTagRule(tagMagnitude1, "Right Now", TagRuleActionTypeList.Codes.AddTag);
			rule1.TGR_LastRunStartTimeUtc = new ZDateTime(2015, 1, 1);
			rule2.TGR_LastRunStartTimeUtc = new ZDateTime(2015, 2, 1);

			Factory.Save();

			var bizo = new BMTagRuleFilterBusinessObject();
			var filterStrip = (ModuleDateFilter)bizo["Last Run Start Time"];
			filterStrip.IsActive = true;
			filterStrip.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filterStrip.Property1 = new ZDateTime(2015, 1, 15);
			filterStrip.Property2 = new ZDateTime(2015, 2, 15);

			var rules = Factory.Load<TagRule>(bizo.Filter);
			AssertEquals(1, rules.Length);
			AssertCollectionContains(rule2, rules);
		}

		public void TestBranchFilterStrip()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();

			var definition1 = BMSTestHelper.CreateTagDefinition(Factory, "AAA");
			var tagMagnitude1 = BMSTestHelper.CreateTagMagnitude(definition1, "Ma1");
			var rule1 = BMSTestHelper.CreateTagRule(tagMagnitude1, "Later Ron", TagRuleActionTypeList.Codes.AddTag);
			var rule2 = BMSTestHelper.CreateTagRule(tagMagnitude1, "Right Now", TagRuleActionTypeList.Codes.AddTag);

			rule1.TGR_GB_Branch = branch.PK;

			Factory.Save();

			var bizo = new BMTagRuleFilterBusinessObject();
			var filterStrip = (ModuleGuidFilter)bizo["Branch"];
			filterStrip.IsActive = true;
			filterStrip.Property = branch.PK;

			var rules = Factory.Load<TagRule>(bizo.Filter);
			AssertEquals(1, rules.Length);
			AssertCollectionContains(rule1, rules);
		}

		public void TestDepartmentFilterStrip()
		{
			var department = Factory.NewWithValidTestData<GlbDepartment>();

			var definition1 = BMSTestHelper.CreateTagDefinition(Factory, "AAA");
			var tagMagnitude1 = BMSTestHelper.CreateTagMagnitude(definition1, "Ma1");
			var rule1 = BMSTestHelper.CreateTagRule(tagMagnitude1, "Later Ron", TagRuleActionTypeList.Codes.AddTag);
			var rule2 = BMSTestHelper.CreateTagRule(tagMagnitude1, "Right Now", TagRuleActionTypeList.Codes.AddTag);

			rule1.TGR_GE_Department = department.PK;

			Factory.Save();

			var bizo = new BMTagRuleFilterBusinessObject();
			var filterStrip = (ModuleGuidFilter)bizo["Department"];
			filterStrip.IsActive = true;
			filterStrip.Property = department.PK;

			var rules = Factory.Load<TagRule>(bizo.Filter);
			AssertEquals(1, rules.Length);
			AssertCollectionContains(rule1, rules);
		}

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new BMTagRuleFilterBusinessObject();
		}

		protected override void SetUp()
		{
			base.SetUp();
			BMSTestHelper.EnableBMSInRegistry();
		}

		#endregion
	}
}
