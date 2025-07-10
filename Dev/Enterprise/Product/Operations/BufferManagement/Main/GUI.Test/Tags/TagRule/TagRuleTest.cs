using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.BufferManagement.Module;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	[TestedType(typeof(TagRule))]
	public class TagRuleTest : EnterpriseBusinessObjectTestCase
	{
		#region Delete

		public void TestDelete_ShouldDeleteSchedule()
		{
			var rule = BMSTestHelper.CreateTagRuleWithDefAndMag(Factory);
			Factory.Save();

			var loadedRule = new BusinessObjectFactory().Load<TagRule>(rule.PK);
			var loadedSchedule = loadedRule.Schedule;
			AssertEquals(false, loadedSchedule.IsDeleted);

			loadedRule.Delete();
			loadedRule.Factory.Save();

			AssertEquals(true, loadedSchedule.IsDeleted);
		}

		public void TestDelete_ShouldAlsoDeleteStmModuleFilters()
		{
			var initialFilterCount = Factory.GetDatabaseCount(typeof(StmModuleFilter));
			var rule = BMSTestHelper.CreateTagRuleWithDefAndMag(Factory, "One Glue to rule them all", TagRuleActionTypeList.Codes.AddTag);
			var filter = rule.Filter;

			Factory.Save();
			AssertEquals(false, filter.IsDeleted);

			var newFactory = Factory.CreateNewFactory();
			var loadedRule = newFactory.Load<TagRule>(rule.PK);

			loadedRule.Delete();
			newFactory.Save();

			var filterInDatabase = newFactory.Load<StmModuleFilter>(filter.PK);
			AssertNull("The filter has been deleted so this should be null and yet...", filterInDatabase);
			AssertEquals("The filter count should be the same as before the rule was created", initialFilterCount, Factory.GetDatabaseCount(typeof(StmModuleFilter)));
		}

		#endregion

		#region System Rules

		public void TestSystemColumnsReadOnly()
		{
			var definition = BMSTestHelper.CreateTagDefinition(Factory, "ARM", "Arms have things");
			var magnitude = BMSTestHelper.CreateTagMagnitude(definition, "FIN", "Fingers! :D");
			var rule = BMSTestHelper.CreateTagRule(magnitude, "The I like eggs rule", TagRuleActionTypeList.Codes.AddTag);

			rule.TGR_IsSystem = true;

			AssertEquals(true, rule.TGR_ActionTypeInfo.ReadOnly);
			AssertEquals(true, rule.TGR_NameInfo.ReadOnly);
			AssertEquals(true, rule.TGR_IsSystemInfo.ReadOnly);

			AssertEquals(false, rule.TGR_IsActiveInfo.ReadOnly);
		}

		public void TestCannotDeleteSystem()
		{
			var definition = BMSTestHelper.CreateTagDefinition(Factory, "OOT", "Look OOT");
			var magnitude = BMSTestHelper.CreateTagMagnitude(definition, "BEL", "Below");
			var tagRule1 = BMSTestHelper.CreateTagRule(magnitude, "The Yofl Rule", TagRuleActionTypeList.Codes.AddTag);
			var tagRule2 = BMSTestHelper.CreateTagRule(magnitude, "The Pofl Rule", TagRuleActionTypeList.Codes.AddTag);

			AssertNoExceptionThrown(() => tagRule1.Delete());

			tagRule2.TGR_IsSystem = true;

			AssertExceptionThrown<CannotDeleteException>(() => tagRule2.Delete());
		}

		#endregion

		#region Clone

		public void TestClone_DontShareFilter()
		{
			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "WHE");
			var tagMagnitude = BMSTestHelper.CreateTagMagnitude(tagGroup, "BIS", "Biscuits can be fun!");

			var tagRule = Factory.New<TagRule>();
			tagRule.TGR_Name = "Shoop Da Whoop";
			tagRule.TagTemplate.TGL_TGM_Magnitude = tagMagnitude.PK;
			FilterStripsTestHelper.AddStartsWithFilter(tagRule.Filter, "Completion Statement", "I hate every ape I see...");

			var clonedTagRule = (TagRule)tagRule.Clone();

			BMSTestHelper.AssertModuleFilterDeepClone(tagRule.Filter, clonedTagRule.Filter, clonedTagRule);

			AssertNoExceptionThrown(() => Factory.Save());
		}

		public void TestClone_DontShareTemplate()
		{
			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "SOR");
			var tagMagnitude = BMSTestHelper.CreateTagMagnitude(tagGroup, "ARM", "I have sore arms!");

			var tagRule = Factory.New<TagRule>();
			tagRule.TGR_Name = "Mice lay eggs";
			tagRule.TagTemplate.TGL_TGM_Magnitude = tagMagnitude.PK;
			var clonedTagRule = (TagRule)tagRule.Clone();

			AssertNotEquals(clonedTagRule.TagTemplate.TGL_ParentId, tagRule.TagTemplate.TGL_ParentId);
			AssertEquals(tagRule.PK, tagRule.TagTemplate.TGL_ParentId);
			AssertEquals(clonedTagRule.PK, clonedTagRule.TagTemplate.TGL_ParentId);
			AssertNotEquals(tagRule.TagTemplate.PK, clonedTagRule.TagTemplate.PK);

			AssertNoExceptionThrown(() => Factory.Save());
		}

		public void TestClone()
		{
			var tagRule = Factory.New<TagRule>();
			tagRule.TGR_IsSystem = true;
			tagRule.TGR_LastRunStartTimeUtc = ZDateTime.BrettsBirthday;
			tagRule.TGR_LastRunDurationInSeconds = 1;
			var clonedTagRule = (TagRule)tagRule.Clone();

			AssertEquals(tagRule.TGR_Name + " Copy", clonedTagRule.TGR_Name);

			AssertEquals(tagRule.ActionDescription, clonedTagRule.ActionDescription);

			AssertEquals(tagRule.TagTemplate.Magnitude, clonedTagRule.TagTemplate.Magnitude);
			AssertEquals(tagRule.TagTemplate.MagnitudeCode, clonedTagRule.TagTemplate.MagnitudeCode);

			AssertEquals(tagRule.TagDescription, clonedTagRule.TagDescription);

			AssertEquals(ZDateTime.Empty, clonedTagRule.TGR_LastRunStartTimeUtc);
			AssertEquals(0, clonedTagRule.TGR_LastRunDurationInSeconds);

			BMSTestHelper.AssertModuleFilterDeepClone(tagRule.Filter, clonedTagRule.Filter, clonedTagRule);
			AssertEquals(clonedTagRule.Filter.S9_IsSystem, false);

			AssertEquals(tagRule.TGR_IsActive, clonedTagRule.TGR_IsActive);

			AssertEquals(clonedTagRule.TGR_IsSystem, false);
		}

		public void TestCloneTruncation()
		{
			var tagRule = Factory.New<TagRule>();
			tagRule.TGR_Name = new string('w', TagRuleSchema.TGR_Name.MaxLength);
			var clonedTagRule = (TagRule)tagRule.Clone();
			AssertEquals(tagRule.TGR_Name.Substring(0, TagRuleSchema.TGR_Name.MaxLength - 5) + " Copy", clonedTagRule.TGR_Name);
		}

		#endregion

		#region TagTemplate

		public void TestCreateTemplate()
		{
			var rule = Factory.New<TagRule>();
			var template = rule.TagTemplate;

			AssertNotNull(template);
			AssertEquals(template, rule.TagTemplate);

			AssertEquals(rule.PK, template.TGL_ParentId);
			AssertEquals(TagRuleSchema.Constants.Prefix, template.TGL_ParentTableCode);

			AssertNull(template.Parent);
		}

		public void TestDeleteTemplate()
		{
			var rule = Factory.New<TagRule>();
			var template = rule.TagTemplate;
			template.Delete();

			AssertEquals(true, template.IsDeleted);
			AssertNotEquals(template, rule.TagTemplate);
			AssertEquals(false, rule.TagTemplate.IsDeleted);
		}

		public void TestDeleteRuleAlsoDeletesTemplate()
		{
			var rule = Factory.New<TagRule>();
			var template = rule.TagTemplate;

			rule.Delete();

			AssertEquals(true, template.IsDeleted);
			AssertEquals(true, rule.IsDeleted);
		}

		#endregion

		#region Filters

		public void TestHasSameFilters_ShouldBeTrue_WhenTwoTagRulesHaveSameFiltersInSameOrder()
		{
			var definition = BMSTestHelper.CreateTagDefinition(Factory, "ABC", "Cats");
			definition.TGD_UsageScope = TagUsageScopeList.Codes.Rule;
			var magnitude = BMSTestHelper.CreateTagMagnitude(definition, "CAT", "A Big Cat");

			var tagRule1 = BMSTestHelper.CreateTagRule(magnitude, "ADD A Big Cat rule", TagRuleActionTypeList.Codes.AddTag);
			var tagRule2 = BMSTestHelper.CreateTagRule(magnitude, "DEL A Big Cat rule", TagRuleActionTypeList.Codes.RemoveTag);

			var filterStrip1 = new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Custom SQL Filter",
				FilterStripValueSetter = value1 => ((ModuleSQLFilter)value1).Property1 = "FH_CompletionStatement = 'This is a Big Cat'",
			};
			var filterStrip2 = new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Custom SQL Filter",
				FilterStripValueSetter = value1 => ((ModuleSQLFilter)value1).Property1 = "FH_Status = 'CLS'",
			};

			var filterStrips = new FilterStripsTestHelper.FilterStripDefinition[] { filterStrip1, filterStrip2 };

			FilterStripsTestHelper.AddFilterStrips(tagRule1.Filter, filterStrips);
			FilterStripsTestHelper.AddFilterStrips(tagRule2.Filter, filterStrips);

			Assert(tagRule1.HasSameFilters(tagRule2));
		}

		public void TestHasSameFilters_ShouldBeTrue_WhenTwoTagRulesHaveSameANDFiltersInDifferentOrder()
		{
			var definition = BMSTestHelper.CreateTagDefinition(Factory, "ASD", "Dogs");
			definition.TGD_UsageScope = TagUsageScopeList.Codes.Rule;
			var magnitude = BMSTestHelper.CreateTagMagnitude(definition, "DOG", "A Small Dog");

			var tagRule1 = BMSTestHelper.CreateTagRule(magnitude, "ADD A Small Dog rule", TagRuleActionTypeList.Codes.AddTag);
			var tagRule2 = BMSTestHelper.CreateTagRule(magnitude, "DEL A Small Dog rule", TagRuleActionTypeList.Codes.RemoveTag);

			var filterStrip1 = new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Custom SQL Filter",
				FilterStripValueSetter = value1 => ((ModuleSQLFilter)value1).Property1 = "FH_CompletionStatement = 'This is a Small Dog'",
			};
			var filterStrip2 = new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Custom SQL Filter",
				FilterStripValueSetter = value1 => ((ModuleSQLFilter)value1).Property1 = "FH_Status = 'CLS'",
			};
			var filterStrip3 = new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Custom SQL Filter",
				FilterStripValueSetter = value1 => ((ModuleSQLFilter)value1).Property1 = "FH_WorfklowType = 'DOG'",
			};
			var parentGuid = Guid.NewGuid();
			var filterStrip4 = new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Custom SQL Filter",
				FilterStripValueSetter = value1 => ((ModuleSQLFilter)value1).Property1 = $"FH_FH_ParentHeader = '{parentGuid}'",
			};

			FilterStripsTestHelper.AddFilterStrips(tagRule1.Filter, new FilterStripsTestHelper.FilterStripDefinition[] { filterStrip1, filterStrip2, filterStrip3, filterStrip4 });
			FilterStripsTestHelper.AddFilterStrips(tagRule2.Filter, new FilterStripsTestHelper.FilterStripDefinition[] { filterStrip2, filterStrip4, filterStrip1, filterStrip3 });

			Assert(tagRule1.HasSameFilters(tagRule2));
		}

		public void TestHasSameFilters_ShouldBeTrue_WhenTwoTagRulesHaveSameORFiltersInDifferentOrder()
		{
			var definition = BMSTestHelper.CreateTagDefinition(Factory, "ASD", "Dogs");
			definition.TGD_UsageScope = TagUsageScopeList.Codes.Rule;
			var magnitude = BMSTestHelper.CreateTagMagnitude(definition, "DOG", "A Small Dog");

			var tagRule1 = BMSTestHelper.CreateTagRule(magnitude, "ADD A Small Dog rule", TagRuleActionTypeList.Codes.AddTag);
			var tagRule2 = BMSTestHelper.CreateTagRule(magnitude, "DEL A Small Dog rule", TagRuleActionTypeList.Codes.RemoveTag);

			var filterStrip1 = new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Custom SQL Filter",
				FilterStripValueSetter = value1 => ((ModuleSQLFilter)value1).Property1 = "FH_CompletionStatement = 'This is a Small Dog'",
			};
			var filterStrip2 = new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Custom SQL Filter",
				FilterStripValueSetter = value1 => ((ModuleSQLFilter)value1).Property1 = "FH_Status = 'CLS'",
			};
			var filterStrip3 = new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Custom SQL Filter",
				FilterStripValueSetter = value1 => ((ModuleSQLFilter)value1).Property1 = "FH_WorfklowType = 'DOG'",
			};

			filterStrip2.OrCategory = FilterOrCategory.Blue;
			filterStrip3.OrCategory = FilterOrCategory.Blue;

			FilterStripsTestHelper.AddFilterStrips(tagRule1.Filter, new FilterStripsTestHelper.FilterStripDefinition[] { filterStrip1, filterStrip2, filterStrip3 });
			FilterStripsTestHelper.AddFilterStrips(tagRule2.Filter, new FilterStripsTestHelper.FilterStripDefinition[] { filterStrip3, filterStrip2, filterStrip1 });

			Assert(tagRule1.HasSameFilters(tagRule2));
		}

		public void TestHasSameFilters_ShouldBeFalse_WhenTwoTagRulesHaveDifferentFilters()
		{
			var definition = BMSTestHelper.CreateTagDefinition(Factory, "ABC", "Cats");
			definition.TGD_UsageScope = TagUsageScopeList.Codes.Rule;
			var magnitude = BMSTestHelper.CreateTagMagnitude(definition, "CAT", "A Big Cat");

			var tagRule1 = BMSTestHelper.CreateTagRule(magnitude, "ADD A Big Cat rule", TagRuleActionTypeList.Codes.AddTag);
			var tagRule2 = BMSTestHelper.CreateTagRule(magnitude, "DEL A Big Cat rule", TagRuleActionTypeList.Codes.RemoveTag);

			var filterStrip1 = new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Custom SQL Filter",
				FilterStripValueSetter = value1 => ((ModuleSQLFilter)value1).Property1 = "FH_CompletionStatement = 'This is a Big Cat'",
			};
			var filterStrip2 = new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Custom SQL Filter",
				FilterStripValueSetter = value1 => ((ModuleSQLFilter)value1).Property1 = "FH_Status = 'CLS'",
			};
			var filterStrip3 = new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Custom SQL Filter",
				FilterStripValueSetter = value1 => ((ModuleSQLFilter)value1).Property1 = "FH_CompletionStatement = 'This is a Small Dog'",
			};

			FilterStripsTestHelper.AddFilterStrips(tagRule1.Filter, new FilterStripsTestHelper.FilterStripDefinition[] { filterStrip1, filterStrip2 });
			FilterStripsTestHelper.AddFilterStrips(tagRule2.Filter, new FilterStripsTestHelper.FilterStripDefinition[] { filterStrip3, filterStrip2 });

			AssertEquals(false, tagRule1.HasSameFilters(tagRule2));
		}

		public void TestHasSameFilters_ShouldBeFalse_WhenTwoTagRulesHaveFiltersWithDifferentConjunctiveOperators()
		{
			var definition = BMSTestHelper.CreateTagDefinition(Factory, "ABC", "Cats");
			definition.TGD_UsageScope = TagUsageScopeList.Codes.Rule;
			var magnitude = BMSTestHelper.CreateTagMagnitude(definition, "CAT", "A Big Cat");

			var tagRule1 = BMSTestHelper.CreateTagRule(magnitude, "ADD A Big Cat rule", TagRuleActionTypeList.Codes.AddTag);
			var tagRule2 = BMSTestHelper.CreateTagRule(magnitude, "DEL A Big Cat rule", TagRuleActionTypeList.Codes.RemoveTag);

			var filterStrip1 = new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Custom SQL Filter",
				FilterStripValueSetter = value1 => ((ModuleSQLFilter)value1).Property1 = "FH_CompletionStatement = 'This is a Big Cat'",
			};
			var filterStrip2 = new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Custom SQL Filter",
				FilterStripValueSetter = value1 => ((ModuleSQLFilter)value1).Property1 = "FH_Status = 'CLS'",
			};
			var filterStrip3 = new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Custom SQL Filter",
				FilterStripValueSetter = value1 => ((ModuleSQLFilter)value1).Property1 = "FH_WorfklowType = 'CAT'",
			};

			// filterstrips query =  filterStrip1 AND filterStrip2 AND filterStrip3
			FilterStripsTestHelper.AddFilterStrips(tagRule1.Filter, new FilterStripsTestHelper.FilterStripDefinition[] { filterStrip1, filterStrip2, filterStrip3 });

			filterStrip2.OrCategory = FilterOrCategory.Red;
			filterStrip3.OrCategory = FilterOrCategory.Red;

			// differentFilterStrips query = filterStrip1 AND ( filterStrip2 OR filterStrip3)
			FilterStripsTestHelper.AddFilterStrips(tagRule2.Filter, new FilterStripsTestHelper.FilterStripDefinition[] { filterStrip1, filterStrip2, filterStrip3 });

			AssertEquals(false, tagRule1.HasSameFilters(tagRule2));
		}

		public void TestHasSameFilters_ShouldBeFalse_WhenTwoTagRulesHaveFiltersWithDifferentSuperimposedConditions()
		{
			var definition = BMSTestHelper.CreateTagDefinition(Factory, "ABC", "Cats");
			definition.TGD_UsageScope = TagUsageScopeList.Codes.Rule;
			var magnitude = BMSTestHelper.CreateTagMagnitude(definition, "CAT", "A Big Cat");

			var tagRule1 = BMSTestHelper.CreateTagRule(magnitude, "ADD A Big Cat rule", TagRuleActionTypeList.Codes.AddTag);
			var tagRule2 = BMSTestHelper.CreateTagRule(magnitude, "DEL A Big Cat rule", TagRuleActionTypeList.Codes.RemoveTag);

			var filterStrip1 = new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Custom SQL Filter",
				FilterStripValueSetter = value1 => ((ModuleSQLFilter)value1).Property1 = "FH_CompletionStatement = 'This is a Big Cat'",
			};
			var filterStrip2 = new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Custom SQL Filter",
				FilterStripValueSetter = value1 => ((ModuleSQLFilter)value1).Property1 = "FH_Status = 'CLS'",
			};
			var filterStrip3 = new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Custom SQL Filter",
				FilterStripValueSetter = value1 => ((ModuleSQLFilter)value1).Property1 = "FH_WorfklowType = 'CAT'",
			};

			filterStrip1.OrCategory = FilterOrCategory.Red;
			filterStrip2.OrCategory = FilterOrCategory.Red;

			// filterStrips query = ( filterStrip1 OR filterStrip2 ) AND filterStrip3
			FilterStripsTestHelper.AddFilterStrips(tagRule1.Filter, new FilterStripsTestHelper.FilterStripDefinition[] { filterStrip1, filterStrip2, filterStrip3 });

			filterStrip1.OrCategory = FilterOrCategory.None;
			filterStrip3.OrCategory = FilterOrCategory.Red;

			// differentFilterStrips query = filterStrip1 AND ( filterStrip2 OR filterStrip3 )
			FilterStripsTestHelper.AddFilterStrips(tagRule2.Filter, new FilterStripsTestHelper.FilterStripDefinition[] { filterStrip1, filterStrip2, filterStrip3 });

			AssertEquals(false, tagRule1.HasSameFilters(tagRule2));
		}

		public void TestHasSameFilters_ShouldBeTrue_WhenTwoTagRulesHaveSameFiltersInFilterGroups()
		{
			var definition = BMSTestHelper.CreateTagDefinition(Factory, "ABC", "Cats");
			definition.TGD_UsageScope = TagUsageScopeList.Codes.Rule;
			var magnitude = BMSTestHelper.CreateTagMagnitude(definition, "CAT", "A Big Cat");

			var tagRule1 = BMSTestHelper.CreateTagRule(magnitude, "ADD A Big Cat rule", TagRuleActionTypeList.Codes.AddTag);
			var tagRule2 = BMSTestHelper.CreateTagRule(magnitude, "DEL A Big Cat rule", TagRuleActionTypeList.Codes.RemoveTag);

			var filterStrip1 = new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Custom SQL Filter",
				FilterStripValueSetter = value1 => ((ModuleSQLFilter)value1).Property1 = "FH_CompletionStatement = 'This is a Big Cat'",
			};
			var filterStrip2 = new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Custom SQL Filter",
				FilterStripValueSetter = value1 => ((ModuleSQLFilter)value1).Property1 = "FH_CompletionStatement = 'This is a FAT Cat'",
			};
			var filterStrip3 = new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Custom SQL Filter",
				FilterStripValueSetter = value1 => ((ModuleSQLFilter)value1).Property1 = "FH_Status = 'BLK'",
			};
			var filterStrip4 = new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Custom SQL Filter",
				FilterStripValueSetter = value1 => ((ModuleSQLFilter)value1).Property1 = "FH_Status = 'CLS'",
			};
			var filterStrip5 = new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Custom SQL Filter",
				FilterStripValueSetter = value1 => ((ModuleSQLFilter)value1).Property1 = "FH_WorfklowType = 'CAT'",
			};

			filterStrip1.GroupName = "Group1";
			filterStrip1.GroupOrCategory = FilterOrCategory.Blue;
			filterStrip1.OrCategory = FilterOrCategory.Green;
			filterStrip2.GroupName = "Group1";
			filterStrip2.GroupOrCategory = FilterOrCategory.Blue;
			filterStrip2.OrCategory = FilterOrCategory.Green;

			filterStrip3.GroupName = "Group2";
			filterStrip3.GroupOrCategory = FilterOrCategory.Blue;
			filterStrip3.OrCategory = FilterOrCategory.Red;
			filterStrip4.GroupName = "Group2";
			filterStrip4.GroupOrCategory = FilterOrCategory.Blue;
			filterStrip4.OrCategory = FilterOrCategory.Red;
			filterStrip5.GroupName = "Group2";
			filterStrip5.GroupOrCategory = FilterOrCategory.Blue;

			// filterStrips query = ( filterStrip1 OR filterStrip2 ) OR ( (filterStrip3 OR filterStrip4) AND filterStrip5 )
			var filterStrips = new FilterStripsTestHelper.FilterStripDefinition[] { filterStrip1, filterStrip2, filterStrip3, filterStrip4, filterStrip5 };

			FilterStripsTestHelper.AddFilterStrips(tagRule1.Filter, filterStrips);
			FilterStripsTestHelper.AddFilterStrips(tagRule2.Filter, filterStrips);

			Assert(tagRule1.HasSameFilters(tagRule2));
		}

		public void TestHasSameFilters_ShouldBeTrue_WhenTwoTagRulesHaveSameFiltersInFilterGroupsInDifferentOrder()
		{
			var definition = BMSTestHelper.CreateTagDefinition(Factory, "ABC", "Cats");
			definition.TGD_UsageScope = TagUsageScopeList.Codes.Rule;
			var magnitude = BMSTestHelper.CreateTagMagnitude(definition, "CAT", "A Big Cat");

			var tagRule1 = BMSTestHelper.CreateTagRule(magnitude, "ADD A Big Cat rule", TagRuleActionTypeList.Codes.AddTag);
			var tagRule2 = BMSTestHelper.CreateTagRule(magnitude, "DEL A Big Cat rule", TagRuleActionTypeList.Codes.RemoveTag);

			var filterStrip1 = new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Custom SQL Filter",
				FilterStripValueSetter = value1 => ((ModuleSQLFilter)value1).Property1 = "FH_CompletionStatement = 'This is a Big Cat'",
			};
			var filterStrip2 = new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Custom SQL Filter",
				FilterStripValueSetter = value1 => ((ModuleSQLFilter)value1).Property1 = "FH_CompletionStatement = 'This is a FAT Cat'",
			};
			var filterStrip3 = new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Custom SQL Filter",
				FilterStripValueSetter = value1 => ((ModuleSQLFilter)value1).Property1 = "FH_Status = 'BLK'",
			};
			var filterStrip4 = new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Custom SQL Filter",
				FilterStripValueSetter = value1 => ((ModuleSQLFilter)value1).Property1 = "FH_Status = 'CLS'",
			};
			var filterStrip5 = new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Custom SQL Filter",
				FilterStripValueSetter = value1 => ((ModuleSQLFilter)value1).Property1 = "FH_WorfklowType = 'CAT'",
			};

			filterStrip1.GroupName = "Group1";
			filterStrip1.GroupOrCategory = FilterOrCategory.Blue;
			filterStrip1.OrCategory = FilterOrCategory.Green;
			filterStrip2.GroupName = "Group1";
			filterStrip2.GroupOrCategory = FilterOrCategory.Blue;
			filterStrip2.OrCategory = FilterOrCategory.Green;

			filterStrip3.GroupName = "Group2";
			filterStrip3.GroupOrCategory = FilterOrCategory.Blue;
			filterStrip3.OrCategory = FilterOrCategory.Red;
			filterStrip4.GroupName = "Group2";
			filterStrip4.GroupOrCategory = FilterOrCategory.Blue;
			filterStrip4.OrCategory = FilterOrCategory.Red;
			filterStrip5.GroupName = "Group2";
			filterStrip5.GroupOrCategory = FilterOrCategory.Blue;
			filterStrip5.OrCategory = FilterOrCategory.Yellow;

			// filterStrips query = ( filterStrip1 OR filterStrip2 ) OR ( (filterStrip3 OR filterStrip4) AND filterStrip5 )
			FilterStripsTestHelper.AddFilterStrips(tagRule1.Filter, new FilterStripsTestHelper.FilterStripDefinition[] { filterStrip1, filterStrip2, filterStrip3, filterStrip4, filterStrip5 });

			// filterStrips query = ( filterStrip5 AND ( filterStrip3 OR filterStrip4 ) ) OR ( filterStrip2 OR filterStrip1 )
			FilterStripsTestHelper.AddFilterStrips(tagRule2.Filter, new FilterStripsTestHelper.FilterStripDefinition[] { filterStrip5, filterStrip3, filterStrip4, filterStrip2, filterStrip1 });

			Assert(tagRule1.HasSameFilters(tagRule2));
		}
		public void TestHasSameFilters_ShouldBeFalse_WhenTwoTagRulesHaveDifferentFiltersInFilterGroups()
		{
			var definition = BMSTestHelper.CreateTagDefinition(Factory, "ABC", "Cats");
			definition.TGD_UsageScope = TagUsageScopeList.Codes.Rule;
			var magnitude = BMSTestHelper.CreateTagMagnitude(definition, "CAT", "A Big Cat");

			var tagRule1 = BMSTestHelper.CreateTagRule(magnitude, "ADD A Big Cat rule", TagRuleActionTypeList.Codes.AddTag);
			var tagRule2 = BMSTestHelper.CreateTagRule(magnitude, "DEL A Big Cat rule", TagRuleActionTypeList.Codes.RemoveTag);

			var filterStrip1 = new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Custom SQL Filter",
				FilterStripValueSetter = value1 => ((ModuleSQLFilter)value1).Property1 = "FH_CompletionStatement = 'This is a Big Cat'",
			};
			var filterStrip2 = new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Custom SQL Filter",
				FilterStripValueSetter = value1 => ((ModuleSQLFilter)value1).Property1 = "FH_CompletionStatement = 'This is a FAT Cat'",
			};
			var filterStrip3 = new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Custom SQL Filter",
				FilterStripValueSetter = value1 => ((ModuleSQLFilter)value1).Property1 = "FH_Status = 'BLK'",
			};
			var filterStrip4 = new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Custom SQL Filter",
				FilterStripValueSetter = value1 => ((ModuleSQLFilter)value1).Property1 = "FH_Status = 'CLS'",
			};
			var filterStrip5 = new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Custom SQL Filter",
				FilterStripValueSetter = value1 => ((ModuleSQLFilter)value1).Property1 = "FH_WorfklowType = 'CAT'",
			};

			var filterStrip6 = new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Custom SQL Filter",
				FilterStripValueSetter = value1 => ((ModuleSQLFilter)value1).Property1 = "FH_CompletionStatement = 'This is a Small Dog'",
			};
			var filterStrip7 = new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Custom SQL Filter",
				FilterStripValueSetter = value1 => ((ModuleSQLFilter)value1).Property1 = "FH_CompletionStatement = 'This is a TINY Dog'",
			};
			var filterStrip8 = new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Custom SQL Filter",
				FilterStripValueSetter = value1 => ((ModuleSQLFilter)value1).Property1 = "FH_WorfklowType = 'DOG'",
			};

			filterStrip1.GroupName = "Group1";
			filterStrip1.GroupOrCategory = FilterOrCategory.Blue;
			filterStrip1.OrCategory = FilterOrCategory.Green;
			filterStrip2.GroupName = "Group1";
			filterStrip2.GroupOrCategory = FilterOrCategory.Blue;
			filterStrip2.OrCategory = FilterOrCategory.Green;

			filterStrip3.GroupName = "Group2";
			filterStrip3.GroupOrCategory = FilterOrCategory.Blue;
			filterStrip3.OrCategory = FilterOrCategory.Red;
			filterStrip4.GroupName = "Group2";
			filterStrip4.GroupOrCategory = FilterOrCategory.Blue;
			filterStrip4.OrCategory = FilterOrCategory.Red;
			filterStrip5.GroupName = "Group2";
			filterStrip5.GroupOrCategory = FilterOrCategory.Blue;
			filterStrip5.OrCategory = FilterOrCategory.Yellow;

			// filterStrips query = ( filterStrip1 OR filterStrip2 ) OR ( (filterStrip3 OR filterStrip4) AND filterStrip5 )
			FilterStripsTestHelper.AddFilterStrips(tagRule1.Filter, new FilterStripsTestHelper.FilterStripDefinition[] { filterStrip1, filterStrip2, filterStrip3, filterStrip4, filterStrip5 });

			filterStrip6.GroupName = "Group1";
			filterStrip6.GroupOrCategory = FilterOrCategory.Blue;
			filterStrip6.OrCategory = FilterOrCategory.Green;
			filterStrip7.GroupName = "Group1";
			filterStrip7.GroupOrCategory = FilterOrCategory.Blue;
			filterStrip7.OrCategory = FilterOrCategory.Green;

			filterStrip8.GroupName = "Group2";
			filterStrip8.GroupOrCategory = FilterOrCategory.Blue;
			filterStrip8.OrCategory = FilterOrCategory.Yellow;

			// filterStrips query = ( filterStrip6 OR filterStrip7 ) OR ( (filterStrip3 OR filterStrip4) AND filterStrip8 )
			FilterStripsTestHelper.AddFilterStrips(tagRule2.Filter, new FilterStripsTestHelper.FilterStripDefinition[] { filterStrip6, filterStrip7, filterStrip3, filterStrip4, filterStrip8 });

			AssertEquals(false, tagRule1.HasSameFilters(tagRule2));
		}

		public void TestHasSameFilters_ShouldBeFalse_WhenTwoTagRulesHaveDifferentFilterParameters()
		{
			var definition = BMSTestHelper.CreateTagDefinition(Factory, "ABC", "Cats");
			definition.TGD_UsageScope = TagUsageScopeList.Codes.Rule;
			var magnitude = BMSTestHelper.CreateTagMagnitude(definition, "CAT", "A Big Cat");

			var tagRule1 = BMSTestHelper.CreateTagRule(magnitude, "ADD A Big Cat rule", TagRuleActionTypeList.Codes.AddTag);
			var tagRule2 = BMSTestHelper.CreateTagRule(magnitude, "DEL A Small Dog rule", TagRuleActionTypeList.Codes.RemoveTag);

			var filterStrip1 = new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "This is a Big Cat",
			};
			var filterStrip2 = new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "This is a Small Dog",
			};

			FilterStripsTestHelper.AddFilterStrips(tagRule1.Filter, new FilterStripsTestHelper.FilterStripDefinition[] { filterStrip1 });
			FilterStripsTestHelper.AddFilterStrips(tagRule2.Filter, new FilterStripsTestHelper.FilterStripDefinition[] { filterStrip2 });

			AssertEquals(false, tagRule1.HasSameFilters(tagRule2));
		}

		#endregion

		#region Properties

		public void TestDefaultValues()
		{
			var rule = Factory.New<TagRule>();
			AssertEquals(rule.TGR_ActionType, TagRuleActionTypeList.Codes.AddTag);
			AssertEquals(rule.TGR_GB_Branch, Env.CurrentBranchPK);
			AssertEquals(rule.TGR_GE_Department, Env.CurrentDepartmentPK);
		}

		public void TestTemplateColumnPrefix()
		{
			var definition = BMSTestHelper.CreateTagDefinition(Factory, "SAN", "The San Plan");
			var magnitude = BMSTestHelper.CreateTagMagnitude(definition, "AGR", "Aggregation leads to elevation");
			var rule = BMSTestHelper.CreateTagRule(magnitude, "I feel uncomfortable with this", TagRuleActionTypeList.Codes.AddTag);

			AssertEquals("TGR", rule.TagTemplate.TGL_ParentTableCode);
		}

		public void TestNameIsntLongerThanTableName()
		{
			AssertEquals(true, TagRuleSchema.TGR_Name.MaxLength >= StmModuleFilterSchema.S9_FilterName.MaxLength - 4);
		}

		[TestDate(2015, 3, 3)]
		public void TestLastRunStartTimeLocal()
		{
			var rule = Factory.New<TagRule>();
			var testDateUtc = new ZDateTime(TestDateAttribute.Date);
			rule.TGR_LastRunStartTimeUtc = testDateUtc;

			AssertEquals(testDateUtc.ToLocalBranchTime(), rule.LastRunStartTimeLocal);
			AssertNotEquals(testDateUtc, testDateUtc.ToLocalBranchTime());
		}

		public void TestLastRunDuration()
		{
			var rule = Factory.New<TagRule>();
			rule.TGR_LastRunDurationInSeconds = 70;

			AssertEquals(new TimeSpan(0, 1, 10).ToString(), rule.LastRunDuration);
		}

		public void TestNextRunTime()
		{
			var rule = BMSTestHelper.CreateTagRuleWithDefAndMag(Factory);
			AssertEquals(rule.Schedule.CalcNextRunTimeLocal, rule.NextRunTime);
		}

		#endregion

		#region Schedule

		[TestDate(2017, 1, 30)]
		public void TestTagRuleSchedule_DefaultValues()
		{
			var rule = Factory.NewWithValidTestData<TagRule>();
			var schedule = rule.Schedule;

			AssertNotNull(schedule);
			AssertEquals(rule.PK, schedule.S5_ParentID);
			AssertEquals("TGR", schedule.S5_ParentTableCode);
			AssertEquals("H", schedule.S5_TaskPeriod);
			AssertEquals(1, schedule.S5_TaskPeriodCount);
			AssertEquals(true, schedule.S5_IsActive);
			AssertEquals(ZDateTime.UtcNow.AddSeconds(-1), schedule.S5_NextScheduledPrintRunTimeUtc);
		}

		#endregion

		#region Preview

		public void TestGetAdditionalPreviewFilter_ForAddRemoveRule_ShouldReturnCorrectQueryDependingOnWhichMenuItemWasClicked()
		{
			var user = Factory.NewWithValidTestData<GlbStaff>();
			var def = BMSTestHelper.CreateTagDefinition(Factory, "AAA");
			var mag = BMSTestHelper.CreateTagMagnitude(def, "BBB");
			var rule = BMSTestHelper.CreateTagRule(mag, "Sweet sweet can", TagRuleActionTypeList.Codes.AddAndRemoveTag, templateMagnitude: 1);
			FilterStripsTestHelper.AddFilterStrip<ModuleNkFilter>(rule.Filter, "Creating User", filter => filter.Property = user.GS_Code);

			var workflowA = BMSTestHelper.CreateWorkflowAndParents<SalesEnquiry>(Factory, "A");
			workflowA.FH_SystemCreateUser = user.GS_Code;
			var workflowB = BMSTestHelper.CreateWorkflowAndParents<SalesEnquiry>(Factory, "B");
			var tagB = workflowB.AddTag(mag).Link;
			tagB.TGL_Magnitude = 1;

			Factory.Save();

			var query = rule.GetAdditionalPreviewFilter(ModuleIDs.BMFilterRule.Name, AddRemoveTagRulePreviewOptionsList.Codes.Add);
			var results = Factory.Load<ProcessHeader>(query);
			AssertContainsExactElementsInAnyOrder(new[] { "A" }, results.Select(x => x.FH_CompletionStatement));

			query = rule.GetAdditionalPreviewFilter(ModuleIDs.BMFilterRule.Name, AddRemoveTagRulePreviewOptionsList.Codes.Remove);
			results = Factory.Load<ProcessHeader>(query);
			AssertContainsExactElementsInAnyOrder(new[] { "B" }, results.Select(x => x.FH_CompletionStatement));

			rule.TGR_ActionType = TagRuleActionTypeList.Codes.AddTag;
			query = rule.GetAdditionalPreviewFilter(ModuleIDs.BMFilterRule.Name, AddRemoveTagRulePreviewOptionsList.Codes.Remove);
			results = Factory.Load<ProcessHeader>(query);
			AssertContainsExactElementsInAnyOrder("The dropDownCode argument should be ignored because the rule isn't using the AddAndRemove action type, and yet...", new[] { "A" }, results.Select(x => x.FH_CompletionStatement));

			rule.TGR_ActionType = TagRuleActionTypeList.Codes.RemoveTag;
			query = rule.GetAdditionalPreviewFilter(ModuleIDs.BMFilterRule.Name, AddRemoveTagRulePreviewOptionsList.Codes.Add);
			results = Factory.Load<ProcessHeader>(query);
			AssertContainsExactElementsInAnyOrder("The dropDownCode argument should be ignored because the rule isn't using the AddAndRemove action type, and yet...", Array.Empty<string>(), results.Select(x => x.FH_CompletionStatement));

			var tagA = workflowA.AddTag(mag).Link;
			tagA.TGL_Magnitude = 1;
			Factory.Save();
			query = rule.GetAdditionalPreviewFilter(ModuleIDs.BMFilterRule.Name, AddRemoveTagRulePreviewOptionsList.Codes.Add);
			results = Factory.Load<ProcessHeader>(query);
			AssertContainsExactElementsInAnyOrder("The dropDownCode argument should be ignored because the rule isn't using the AddAndRemove action type, and yet...", new[] { "A" }, results.Select(x => x.FH_CompletionStatement));

			tagB.TGL_Magnitude = 5;
			rule.TGR_ActionType = TagRuleActionTypeList.Codes.MaintainMagnitude;
			Factory.Save();
			query = rule.GetAdditionalPreviewFilter(ModuleIDs.BMFilterRule.Name, AddRemoveTagRulePreviewOptionsList.Codes.Remove);
			results = Factory.Load<ProcessHeader>(query);
			AssertContainsExactElementsInAnyOrder("The dropDownCode argument should be ignored because the rule isn't using the AddAndRemove action type, and yet...", Array.Empty<string>(), results.Select(x => x.FH_CompletionStatement));

			tagA.TGL_Magnitude = 5;
			Factory.Save();
			query = rule.GetAdditionalPreviewFilter(ModuleIDs.BMFilterRule.Name, AddRemoveTagRulePreviewOptionsList.Codes.Remove);
			results = Factory.Load<ProcessHeader>(query);
			AssertContainsExactElementsInAnyOrder("The dropDownCode argument should be ignored because the rule isn't using the AddAndRemove action type, and yet...", new[] { "A" }, results.Select(x => x.FH_CompletionStatement));
		}

		public void TestGetAdditionalPreviewFilter_MagTagRule_TagIsAppliedFilter_ShouldNotThrowException()
		{
			var user = Factory.NewWithValidTestData<GlbStaff>();
			var def1 = BMSTestHelper.CreateTagDefinition(Factory, "AAA");
			var mag1 = BMSTestHelper.CreateTagMagnitude(def1, "BBB");

			var def2 = BMSTestHelper.CreateTagDefinition(Factory, "CCC");
			var mag2 = BMSTestHelper.CreateTagMagnitude(def2, "DDD");

			Factory.Save();

			var rule = BMSTestHelper.CreateTagRule(mag1, "Oh no my corn", TagRuleActionTypeList.Codes.MaintainMagnitude, templateMagnitude: 1);
			FilterStripsTestHelper.AddFilterStrip<ModuleGuidAppliedToSubCollectionFilter>(rule.Filter, "Tag Magnitude", filter =>
			{
				filter.Property = mag2.PK;
				filter.ComparisonOperator = ModuleGuidAppliedToSubCollectionFilter.IsAppliedComparisonOperator;
			});

			var workflowA = BMSTestHelper.CreateWorkflowAndParents<SalesEnquiry>(Factory, "Paul Newman");
			var tagA = workflowA.AddTag(mag2).Link;
			tagA.TGL_Magnitude = 1;
			var workflowB = BMSTestHelper.CreateWorkflowAndParents<SalesEnquiry>(Factory, "Broken legs");
			var tagB = workflowB.AddTag(mag1).Link;
			tagB.TGL_Magnitude = 1;

			Factory.Save();

			var query = rule.GetAdditionalPreviewFilter(ModuleIDs.BMFilterRule.Name, AddRemoveTagRulePreviewOptionsList.Codes.Add);
			var results = Factory.Load<ProcessHeader>(query);
			AssertContainsExactElementsInAnyOrder("The dropDownCode argument should be ignored because the rule isn't using the AddAndRemove action type, and yet...",
				Array.Empty<string>(), results.Select(x => x.FH_CompletionStatement));
		}

		#endregion

		#region Logs

		public void TestNoStmALogs()
		{
			var tagRule = Factory.NewWithValidTestData<TagRule>();

			Factory.Save();

			var query = new ZQuery(StmALogSchema.SL_Parent, tagRule.PK);
			AssertEquals("Should not create Add event", 0, Factory.Load<StmALog>(query).Length);

			tagRule.TGR_Name = "New name";
			Factory.Save();
			AssertEquals("Should not create Edit event", 0, Factory.Load<StmALog>(query).Length);

			tagRule.Delete();
			Factory.Save();
			AssertEquals("Should not create Delete event", 0, Factory.Load<StmALog>(query).Length);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var rule = (TagRule)base.GetNewBusinessObject();
			rule.TGR_ActionType = TagRuleActionTypeList.Codes.AddTag;

			var stmFilter = rule.Filter;
			stmFilter.FillWithValidTestData();

			var template = rule.TagTemplate;
			template.FillWithValidTestData();

			return rule;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestHelper.EnableBMSInRegistry();
			BMSTestHelper.DisableAcceptabilityBandResultCache();

			disposables = new DisposableList(new[] { DummySecondaryServerConnectionProvider.TemporarilyEnableDummyProvider() });
		}
		DisposableList disposables;

		#region TearDown

		protected override void TearDown()
		{
			base.TearDown();

			disposables.Dispose();
		}

		#endregion

		#endregion
	}

	[TestedType(typeof(TagRule))]
	public class TagRuleRelatedFilterTest : RelatedModuleFilterSupportableTestCase<TagRule>
	{
		protected override IEnumerable<FilterRuleTestSet> GetFilterRules(TagRule businessObject)
		{
			return new[] { new FilterRuleTestSet(null, () => businessObject.Filter, "BMFilterRuleFilterBusinessObject") };
		}

		protected override void ValidateBusinessObject(TagRule businessObject)
		{
			businessObject.Validation.ValidateAll();
		}

		protected override TagRule GetNewBusinessObject()
		{
			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "WHE");
			var tagMagnitude = BMSTestHelper.CreateTagMagnitude(tagGroup, "BIS", "Biscuits can be fun!");

			var tagRule = Factory.New<TagRule>();
			tagRule.TGR_Name = "Oooh the garage";
			tagRule.TagTemplate.TGL_TGM_Magnitude = tagMagnitude.PK;
			return tagRule;
		}
	}
}
