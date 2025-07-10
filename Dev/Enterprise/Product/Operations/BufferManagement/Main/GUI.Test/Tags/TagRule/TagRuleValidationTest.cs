using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.GUI.Test
{
	public class TagRuleValidationTest : BusinessObjectValidationTestCase
	{
		public void TestAction()
		{
			var rule = Factory.NewWithValidTestData<TagRule>();
			AssertNoErrors(rule.TGR_ActionTypeInfo);

			rule.TGR_ActionType = string.Empty;
			AssertHasError(rule.TGR_ActionTypeInfo, "Please enter an Action Code.");

			rule.TGR_ActionType = "LEL";
			AssertHasError(rule.TGR_ActionTypeInfo, "Enter a valid Action Code.");
		}

		public void TestName()
		{
			var rule = Factory.New<TagRule>();

			rule.TGR_Name = string.Empty;
			AssertHasError(rule.TGR_NameInfo, "Please enter a Name.");

			rule.TGR_Name = "Rule 1";
			AssertNoErrors(rule.TGR_NameInfo);
		}

		public void TestName_ShouldBeUnique()
		{
			var rule = Factory.New<TagRule>();

			rule.TGR_Name = "Duplicate 1";
			AssertNoErrors(rule.TGR_NameInfo);

			var duplicateRule = Factory.New<TagRule>();
			duplicateRule.TGR_Name = "Duplicate 1";
			AssertHasError(duplicateRule.TGR_NameInfo, "The Name has been duplicated and must be unique.");

			duplicateRule.TGR_Name = "Duplicate 2";
			AssertNoErrors(duplicateRule.TGR_NameInfo);

			duplicateRule.TGR_Name = "duplicate 1";
			AssertHasError(duplicateRule.TGR_NameInfo, "The Name has been duplicated and must be unique.");
		}

		public void TestBranch()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();

			var rule = Factory.New<TagRule>();
			AssertNoErrors(rule.TGR_GB_BranchInfo);

			rule.TGR_GB_Branch = ZGuid.BrettsGuid;
			AssertHasError(rule.TGR_GB_BranchInfo, "Enter a valid Branch.");

			rule.TGR_GB_Branch = branch.PK;
			AssertNoErrors(rule.TGR_GB_BranchInfo);
		}

		public void TestBranchCannotBeNull()
		{
			var rule = Factory.New<TagRule>();
			AssertNoErrors(rule.TGR_GB_BranchInfo);

			rule.TGR_GB_Branch = ZGuid.Empty;
			AssertHasError(rule.TGR_GB_BranchInfo, "Please enter a Branch.");
		}

		public void TestDepartment()
		{
			var department = Factory.NewWithValidTestData<GlbDepartment>();

			var rule = Factory.New<TagRule>();
			AssertNoErrors(rule.TGR_GE_DepartmentInfo);

			rule.TGR_GE_Department = ZGuid.BrettsGuid;
			AssertHasError(rule.TGR_GE_DepartmentInfo, "Enter a valid Department.");

			rule.TGR_GE_Department = department.PK;
			AssertNoErrors(rule.TGR_GE_DepartmentInfo);
		}

		public void TestDepartmentCannotBeNull()
		{
			var rule = Factory.New<TagRule>();
			AssertNoErrors(rule.TGR_GB_BranchInfo);

			rule.TGR_GE_Department = ZGuid.Empty;
			AssertHasError(rule.TGR_GE_DepartmentInfo, "Please enter a Department.");
		}

		public void TestDefaultFilterValid()
		{
			var rule = Factory.NewWithValidTestData<TagRule>();
			rule.Filter.Validation.ValidateAll();
			AssertNoErrors(rule.Filter);
		}

		public void TestTagRuleWithEmptyFilter_ShouldAffectValidation()
		{
			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "STR", "Strike a light");
			var tag = BMSTestHelper.CreateTagMagnitude(tagGroup, "SHO", "Silly hats only");

			var rule = BMSTestHelper.CreateTagRule(tag, "Learn your rules. You better learn your rules.", TagRuleActionTypeList.Codes.AddTag);
			FilterStripsTestHelper.AddFilterStrips(rule.Filter, new FilterStripsTestHelper.FilterStripDefinition { FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement });
			Factory.Save();

			AssertEmptyFilterError(rule, TagRuleActionTypeList.Codes.AddTag);
			AssertEmptyFilterError(rule, TagRuleActionTypeList.Codes.AddAndRemoveTag);

			AssertEmptyFilterWarning(rule, TagRuleActionTypeList.Codes.MaintainMagnitude);
			AssertEmptyFilterWarning(rule, TagRuleActionTypeList.Codes.RemoveTag);

			FilterStripsTestHelper.AddFilterStrips(rule.Filter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Dishes are done"
			});

			AssertNoEmptyFilterNotifications(rule, TagRuleActionTypeList.Codes.AddTag);
			AssertNoEmptyFilterNotifications(rule, TagRuleActionTypeList.Codes.AddAndRemoveTag);
			AssertNoEmptyFilterNotifications(rule, TagRuleActionTypeList.Codes.MaintainMagnitude);
			AssertNoEmptyFilterNotifications(rule, TagRuleActionTypeList.Codes.RemoveTag);
		}

		public void TestValidateAll_WhenADDTagRuleHasNoFilters_ShouldHaveError_WhenExecutedMultipleTimes()
		{
			var rule = BMSTestHelper.CreateTagRuleWithDefAndMag(Factory);
			rule.Validation.ValidateAll();
			AssertHasError(rule.TGR_ActionTypeInfo, "At least one filter must be set for this action code.");

			rule.Validation.ValidateAll();
			AssertHasError("Re-validating should not remove this error.", rule.TGR_ActionTypeInfo, "At least one filter must be set for this action code.");
		}

		public void TestRuleWithFilterWithCountrySpecificModule_WhenModuleNotAvailableInRuleBranch_ShouldHaveError()
		{
			var company = Factory.New<IGlbCompany>();
			((BusinessObject)company).FillWithValidTestData();
			company.GC_RN_NKCountryCode = "DE";

			var branch = Factory.New<IGlbBranch>();
			((BusinessObject)branch).FillWithValidTestData();
			branch.GB_GC = company.PK;

			Factory.Save();

			var config = TestConfigsHelper.CreateTagsTestConfig(Factory, "DUM");
			var rule = BMSTestHelper.CreateTagRule(config.PrincessCelestiaTag, "Bad Rule", TagRuleActionTypeList.Codes.MaintainMagnitude);

			FilterStripsTestHelper.AddFilterStrip<ModuleGuidModuleSpecifiedFilter>(rule.Filter, "Parent Job",
				(filter) => filter.SelectedModule = ModuleIDs.Customs.AU.AirCargoOutturnBills.Name,
				(filter) => filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch);

			rule.Validation.ValidateAll();
			AssertContainsExactElementsInAnyOrder("The rule is configured by default for an AU branch, so there should be no error. SAD!", System.Array.Empty<string>(), rule.GetErrors().Select(x => x.Message));

			rule.TGR_GB_Branch = branch.PK;
			rule.Validation.ValidateAll();
			AssertContainsExactElementsInAnyOrder("Validation should now run in the configured Branch's context, so there should be an error. SAD!", new[] { @"Error - StmModuleFilter: Filter has errors.
Please select a supported module." }, rule.GetErrors().Select(x => x.Message));
		}

		public void TestMilestoneFilters_WithEventReferencePropertyUsed_ShouldHaveError()
		{
			var config = TestConfigsHelper.CreateTagsTestConfig(Factory, "DUM");
			var rule = BMSTestHelper.CreateTagRule(config.PrincessCelestiaTag, "Bad Rule", TagRuleActionTypeList.Codes.MaintainMagnitude);
			FilterStripsTestHelper.AddFilterStrip<ParentJobModuleFilter>(rule.Filter, "Parent Job", filter =>
			{
				filter.SelectedModule = ModuleIDs.JobShipment.Name;
				var milestoneFilter = filter.SelectedFilters.AddFilterStrip<WorkflowModuleTextFilter>("Milestone Completed");
				milestoneFilter.EventReference = "A";
			},
			filter =>
			{
				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch;
			});

			rule.Validation.ValidateAll();
			AssertContainsExactElementsInAnyOrder("We no longer allow this filter option because of its terrible performance.", new[] { @"Error - StmModuleFilter: Filter has errors.
The selected filters have one or more errors." }, rule.GetErrors().Select(x => x.Message));
		}

		public void TestMilestoneFilters_WithEventReferencePropertyNotUsed_ShouldNotHaveError()
		{
			var config = TestConfigsHelper.CreateTagsTestConfig(Factory, "DUM");
			var rule = BMSTestHelper.CreateTagRule(config.PrincessCelestiaTag, "Bad Rule", TagRuleActionTypeList.Codes.MaintainMagnitude);
			FilterStripsTestHelper.AddFilterStrip<ParentJobModuleFilter>(rule.Filter, "Parent Job", filter =>
				{
					filter.SelectedModule = ModuleIDs.JobShipment.Name;
					filter.SelectedFilters.AddFilterStrip<WorkflowModuleTextFilter>("Milestone Completed");
				},
				filter =>
				{
					filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch;
				});

			rule.Validation.ValidateAll();
			AssertContainsExactElementsInAnyOrder("The poorly performing option isn't used, so there should be no error.", System.Array.Empty<string>(), rule.GetErrors().Select(x => x.Message));
		}

		#region Tag Rule Conflicts

		#region ADD Rules

		public void TestTagRuleValidation_ShouldShowNoError_WhenCreatingAnotherADD()
		{
			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "STR", "descriptionTagGroup");
			var tagFirst = BMSTestHelper.CreateTagMagnitude(tagGroup, "TA1", "descriptionTag");

			var addRule = BMSTestHelper.CreateTagRule(tagFirst, "ADD_name", TagRuleActionTypeList.Codes.AddTag);
			AssertNoConflictsWithOtherRules(addRule);

			BMSTestHelper.CreateTagRule(tagFirst, "ADD1", TagRuleActionTypeList.Codes.AddTag);
			Factory.Save();

			AssertNoConflictsWithOtherRules(addRule);

			BMSTestHelper.CreateTagRule(tagFirst, "ARM1", TagRuleActionTypeList.Codes.AddAndRemoveTag);
			Factory.Save();

			AssertHasConflictsWithOtherRules(@"The tag TA1 has an existing ARM-type tag rule. It is not possible to create ADD and ARM tag rules for the same tag. The conflicting rule(s) are as follows:
• ARM1", addRule);
		}

		public void TestTagRuleValidation_ShouldShowError_WhenADD_ConflictsWithExistingARM()
		{
			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "STR", "descriptionTagGroup");
			var tagFirst = BMSTestHelper.CreateTagMagnitude(tagGroup, "TA1", "descriptionTag");

			var addRule = BMSTestHelper.CreateTagRule(tagFirst, "ADD_name", TagRuleActionTypeList.Codes.AddTag);
			AssertNoConflictsWithOtherRules(addRule);

			var conflictingRule1 = BMSTestHelper.CreateTagRule(tagFirst, "ARM1", TagRuleActionTypeList.Codes.AddAndRemoveTag);
			Factory.Save();

			AssertHasConflictsWithOtherRules(@"The tag TA1 has an existing ARM-type tag rule. It is not possible to create ADD and ARM tag rules for the same tag. The conflicting rule(s) are as follows:
• ARM1", addRule);

			var conflictingRule2 = BMSTestHelper.CreateTagRule(tagFirst, "ARM2", TagRuleActionTypeList.Codes.AddAndRemoveTag);
			var conflictingRule3 = BMSTestHelper.CreateTagRule(tagFirst, "ARM3", TagRuleActionTypeList.Codes.AddAndRemoveTag);
			Factory.Save();

			AssertHasConflictsWithOtherRules(@"The tag TA1 has an existing ARM-type tag rule. It is not possible to create ADD and ARM tag rules for the same tag. The conflicting rule(s) are as follows:
• ARM1
• ARM2
• ARM3", addRule);

			conflictingRule1.TGR_IsActive = false;
			Factory.Save();

			AssertHasConflictsWithOtherRules(@"The tag TA1 has an existing ARM-type tag rule. It is not possible to create ADD and ARM tag rules for the same tag. The conflicting rule(s) are as follows:
• ARM2
• ARM3", addRule);

			conflictingRule2.TGR_IsActive = false;
			conflictingRule3.TGR_IsActive = false;
			Factory.Save();

			AssertNoConflictsWithOtherRules(addRule);
		}

		public void TestTagRuleValidation_ShouldShowError_WhenADD_ConflictsWithDEL_WithSameFilters()
		{
			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "STR", "descriptionTagGroup");
			var tagFirst = BMSTestHelper.CreateTagMagnitude(tagGroup, "TA1", "descriptionTag");

			var addRule = BMSTestHelper.CreateTagRule(tagFirst, "ADD_name", TagRuleActionTypeList.Codes.AddTag);
			FilterStripsTestHelper.AddFilterStrips(addRule.Filter, new FilterStripsTestHelper.FilterStripDefinition[] {
				new FilterStripsTestHelper.FilterStripDefinition
				{
					FilterStripName = "Custom SQL Filter",
					FilterStripValueSetter = value1 => ((ModuleSQLFilter)value1).Property1 = "FH_CompletionStatement = 'This is a Big Cat'",
				},
				new FilterStripsTestHelper.FilterStripDefinition
				{
					FilterStripName = "Custom SQL Filter",
					FilterStripValueSetter = value1 => ((ModuleSQLFilter)value1).Property1 = "FH_Status = 'CLS'",
				}
			});

			AssertNoConflictsWithOtherRules(addRule);

			var delRule1 = BMSTestHelper.CreateTagRule(tagFirst, "DEL1", TagRuleActionTypeList.Codes.RemoveTag);
			FilterStripsTestHelper.AddFilterStrips(delRule1.Filter, new FilterStripsTestHelper.FilterStripDefinition[] {
				new FilterStripsTestHelper.FilterStripDefinition
				{
					FilterStripName = "Custom SQL Filter",
					FilterStripValueSetter = value1 => ((ModuleSQLFilter)value1).Property1 = "FH_CompletionStatement = 'This is a Big Cat'",
				}
			});
			Factory.Save();

			AssertNoConflictsWithOtherRules(addRule);

			var delRule2 = BMSTestHelper.CreateTagRule(tagFirst, "DEL2", TagRuleActionTypeList.Codes.RemoveTag);
			FilterStripsTestHelper.AddFilterStrips(delRule2.Filter, new FilterStripsTestHelper.FilterStripDefinition[] {
				new FilterStripsTestHelper.FilterStripDefinition
				{
					FilterStripName = "Custom SQL Filter",
					FilterStripValueSetter = value1 => ((ModuleSQLFilter)value1).Property1 = "FH_CompletionStatement = 'This is a Big Cat'",
				},
				new FilterStripsTestHelper.FilterStripDefinition
				{
					FilterStripName = "Custom SQL Filter",
					FilterStripValueSetter = value1 => ((ModuleSQLFilter)value1).Property1 = "FH_Status = 'CLS'",
				}
			});
			Factory.Save();

			AssertHasConflictsWithOtherRules(@"The tag TA1 has an existing DEL-type tag rule defined that matches the filters you are trying to add. It is not possible to create ADD and DEL tag rules with matching filters. The conflicting rule(s) are as follows:
• DEL2", addRule);

			delRule2.TGR_IsActive = false;
			Factory.Save();

			AssertNoConflictsWithOtherRules(addRule);
		}

		#endregion

		#region DEL Rules

		public void TestTagRuleValidation_ShouldShowNoError_WhenCreatingAnotherDEL()
		{
			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "STR", "descriptionTagGroup");
			var tagFirst = BMSTestHelper.CreateTagMagnitude(tagGroup, "TA1", "descriptionTag");

			var addRule = BMSTestHelper.CreateTagRule(tagFirst, "DEL_name", TagRuleActionTypeList.Codes.RemoveTag);
			AssertNoConflictsWithOtherRules(addRule);

			BMSTestHelper.CreateTagRule(tagFirst, "DEL1", TagRuleActionTypeList.Codes.RemoveTag);
			Factory.Save();

			AssertNoConflictsWithOtherRules(addRule);

			var conflictingRule = BMSTestHelper.CreateTagRule(tagFirst, "ARM1", TagRuleActionTypeList.Codes.AddAndRemoveTag);
			Factory.Save();

			AssertHasConflictsWithOtherRules(@"The tag TA1 has an existing ARM-type tag rule. It is not possible to create DEL and ARM tag rules for the same tag. The conflicting rule(s) are as follows:
• ARM1", addRule);

			conflictingRule.TGR_IsActive = false;
			Factory.Save();

			AssertNoConflictsWithOtherRules(addRule);
		}

		public void TestTagRuleValidation_ShouldShowError_WhenDEL_ConflictsWithARM()
		{
			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "STR", "descriptionTagGroup");
			var tagFirst = BMSTestHelper.CreateTagMagnitude(tagGroup, "TA1", "descriptionTag");

			var delRule = BMSTestHelper.CreateTagRule(tagFirst, "DEL_name", TagRuleActionTypeList.Codes.RemoveTag);
			AssertNoConflictsWithOtherRules(delRule);

			var conflictingRule1 = BMSTestHelper.CreateTagRule(tagFirst, "ARM1", TagRuleActionTypeList.Codes.AddAndRemoveTag);
			Factory.Save();

			AssertHasConflictsWithOtherRules(@"The tag TA1 has an existing ARM-type tag rule. It is not possible to create DEL and ARM tag rules for the same tag. The conflicting rule(s) are as follows:
• ARM1", delRule);

			var conflictingRule2 = BMSTestHelper.CreateTagRule(tagFirst, "ARM2", TagRuleActionTypeList.Codes.AddAndRemoveTag);
			var conflictingRule3 = BMSTestHelper.CreateTagRule(tagFirst, "ARM3", TagRuleActionTypeList.Codes.AddAndRemoveTag);
			Factory.Save();

			AssertHasConflictsWithOtherRules(@"The tag TA1 has an existing ARM-type tag rule. It is not possible to create DEL and ARM tag rules for the same tag. The conflicting rule(s) are as follows:
• ARM1
• ARM2
• ARM3", delRule);

			conflictingRule1.TGR_IsActive = false;
			Factory.Save();

			AssertHasConflictsWithOtherRules(@"The tag TA1 has an existing ARM-type tag rule. It is not possible to create DEL and ARM tag rules for the same tag. The conflicting rule(s) are as follows:
• ARM2
• ARM3", delRule);

			conflictingRule2.TGR_IsActive = false;
			conflictingRule3.TGR_IsActive = false;
			Factory.Save();

			AssertNoConflictsWithOtherRules(delRule);
		}

		public void TestTagRuleValidation_ShouldShowError_WhenDEL_ConflictsWithADD_WithSameFilters()
		{
			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "STR", "descriptionTagGroup");
			var tagFirst = BMSTestHelper.CreateTagMagnitude(tagGroup, "TA1", "descriptionTag");

			var delRule = BMSTestHelper.CreateTagRule(tagFirst, "DEL_name", TagRuleActionTypeList.Codes.RemoveTag);
			FilterStripsTestHelper.AddFilterStrips(delRule.Filter, new FilterStripsTestHelper.FilterStripDefinition[] {
				new FilterStripsTestHelper.FilterStripDefinition
				{
					FilterStripName = "Custom SQL Filter",
					FilterStripValueSetter = value1 => ((ModuleSQLFilter)value1).Property1 = "FH_CompletionStatement = 'This is a Big Cat'",
				},
				new FilterStripsTestHelper.FilterStripDefinition
				{
					FilterStripName = "Custom SQL Filter",
					FilterStripValueSetter = value1 => ((ModuleSQLFilter)value1).Property1 = "FH_Status = 'CLS'",
				}
			});

			AssertNoConflictsWithOtherRules(delRule);

			var addRule1 = BMSTestHelper.CreateTagRule(tagFirst, "ADD1", TagRuleActionTypeList.Codes.AddTag);
			FilterStripsTestHelper.AddFilterStrips(addRule1.Filter, new FilterStripsTestHelper.FilterStripDefinition[] {
				new FilterStripsTestHelper.FilterStripDefinition
				{
					FilterStripName = "Custom SQL Filter",
					FilterStripValueSetter = value1 => ((ModuleSQLFilter)value1).Property1 = "FH_CompletionStatement = 'This is a Big Cat'",
				}
			});
			Factory.Save();

			AssertNoConflictsWithOtherRules(delRule);

			var addRule2 = BMSTestHelper.CreateTagRule(tagFirst, "ADD2", TagRuleActionTypeList.Codes.AddTag);
			FilterStripsTestHelper.AddFilterStrips(addRule2.Filter, new FilterStripsTestHelper.FilterStripDefinition[] {
				new FilterStripsTestHelper.FilterStripDefinition
				{
					FilterStripName = "Custom SQL Filter",
					FilterStripValueSetter = value1 => ((ModuleSQLFilter)value1).Property1 = "FH_CompletionStatement = 'This is a Big Cat'",
				},
				new FilterStripsTestHelper.FilterStripDefinition
				{
					FilterStripName = "Custom SQL Filter",
					FilterStripValueSetter = value1 => ((ModuleSQLFilter)value1).Property1 = "FH_Status = 'CLS'",
				}
			});
			Factory.Save();

			AssertHasConflictsWithOtherRules(@"The tag TA1 has an existing ADD-type tag rule defined that matches the filters you are trying to add. It is not possible to create DEL and ADD tag rules with matching filters. The conflicting rule(s) are as follows:
• ADD2", delRule);

			addRule2.TGR_IsActive = false;
			Factory.Save();

			AssertNoConflictsWithOtherRules(delRule);
		}

		#endregion

		#region ARM Rules

		public void TestTagRuleValidation_ShouldShowError_WhenARM_ConflictsWithExistingADD()
		{
			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "STR", "descriptionTagGroup");
			var tagFirst = BMSTestHelper.CreateTagMagnitude(tagGroup, "TA1", "descriptionTag");

			var armRule = BMSTestHelper.CreateTagRule(tagFirst, "ARM_name", TagRuleActionTypeList.Codes.AddAndRemoveTag);
			AssertNoConflictsWithOtherRules(armRule);

			var conflictingRule1 = BMSTestHelper.CreateTagRule(tagFirst, "ADD1", TagRuleActionTypeList.Codes.AddTag);
			Factory.Save();

			AssertHasConflictsWithOtherRules(@"The tag TA1 has an existing ADD-type tag rule. It is not possible to create ADD and ARM tag rules for the same tag. The conflicting rule(s) are as follows:
• ADD1", armRule);

			var conflictingRule2 = BMSTestHelper.CreateTagRule(tagFirst, "ADD2", TagRuleActionTypeList.Codes.AddTag);
			Factory.Save();

			AssertHasConflictsWithOtherRules(@"The tag TA1 has an existing ADD-type tag rule. It is not possible to create ADD and ARM tag rules for the same tag. The conflicting rule(s) are as follows:
• ADD1
• ADD2", armRule);

			conflictingRule1.TGR_IsActive = false;
			Factory.Save();

			conflictingRule2.TGR_IsActive = false;
			Factory.Save();

			AssertNoConflictsWithOtherRules(armRule);
		}

		public void TestTagRuleValidation_ShouldShowError_WhenARM_ConflictsWithExistingDEL()
		{
			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "STR", "descriptionTagGroup");
			var tagFirst = BMSTestHelper.CreateTagMagnitude(tagGroup, "TA1", "descriptionTag");

			var armRule = BMSTestHelper.CreateTagRule(tagFirst, "ARM_name", TagRuleActionTypeList.Codes.AddAndRemoveTag);
			AssertNoConflictsWithOtherRules(armRule);

			var conflictingRule1 = BMSTestHelper.CreateTagRule(tagFirst, "DEL1", TagRuleActionTypeList.Codes.RemoveTag);
			Factory.Save();

			AssertHasConflictsWithOtherRules(@"The tag TA1 has an existing DEL-type tag rule. It is not possible to create DEL and ARM tag rules for the same tag. The conflicting rule(s) are as follows:
• DEL1", armRule);

			var conflictingRule2 = BMSTestHelper.CreateTagRule(tagFirst, "DEL2", TagRuleActionTypeList.Codes.RemoveTag);
			Factory.Save();

			AssertHasConflictsWithOtherRules(@"The tag TA1 has an existing DEL-type tag rule. It is not possible to create DEL and ARM tag rules for the same tag. The conflicting rule(s) are as follows:
• DEL1
• DEL2", armRule);

			conflictingRule1.TGR_IsActive = false;
			Factory.Save();

			AssertHasConflictsWithOtherRules(@"The tag TA1 has an existing DEL-type tag rule. It is not possible to create DEL and ARM tag rules for the same tag. The conflicting rule(s) are as follows:
• DEL2", armRule);

			conflictingRule2.TGR_IsActive = false;
			Factory.Save();

			AssertNoConflictsWithOtherRules(armRule);
		}

		public void TestTagRuleValidation_ShouldShowError_WhenARM_ConflictsWithExistingARM()
		{
			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "STR", "descriptionTagGroup");
			var tagFirst = BMSTestHelper.CreateTagMagnitude(tagGroup, "TA1", "descriptionTag");

			var armRule = BMSTestHelper.CreateTagRule(tagFirst, "ARM_name", TagRuleActionTypeList.Codes.AddAndRemoveTag);
			AssertNoConflictsWithOtherRules(armRule);

			var conflictingRule1 = BMSTestHelper.CreateTagRule(tagFirst, "ARM1", TagRuleActionTypeList.Codes.AddAndRemoveTag);
			Factory.Save();

			AssertHasConflictsWithOtherRules(@"The tag TA1 has an existing ARM-type tag rule defined. It is not possible to create two ARM-type rules for the same tag. It should be possible to update the filter rules on the existing tag rule to achieve the desired effect. The conflicting rule(s) are as follows:
• ARM1", armRule);

			var conflictingRule2 = BMSTestHelper.CreateTagRule(tagFirst, "ARM2", TagRuleActionTypeList.Codes.AddAndRemoveTag);
			Factory.Save();

			AssertHasConflictsWithOtherRules(@"The tag TA1 has an existing ARM-type tag rule defined. It is not possible to create two ARM-type rules for the same tag. It should be possible to update the filter rules on the existing tag rule to achieve the desired effect. The conflicting rule(s) are as follows:
• ARM1
• ARM2", armRule);

			conflictingRule1.TGR_IsActive = false;
			Factory.Save();

			AssertHasConflictsWithOtherRules(@"The tag TA1 has an existing ARM-type tag rule defined. It is not possible to create two ARM-type rules for the same tag. It should be possible to update the filter rules on the existing tag rule to achieve the desired effect. The conflicting rule(s) are as follows:
• ARM2", armRule);

			conflictingRule2.TGR_IsActive = false;
			Factory.Save();

			AssertNoConflictsWithOtherRules(armRule);
		}

		#endregion

		#endregion

		#region Assertion Methods

		void AssertEmptyFilterError(TagRule rule, string actionType)
		{
			rule.TGR_ActionType = actionType;
			Assert("The ActionType property should have an error because there is no filter set, and yet...", rule.TGR_ActionTypeInfo.HasError(TagRuleValidation.FilterRequiredMessage));
			Assert("The ActionType property should have an error rather than a warning since there is no filter set and because of the particular action type selected, and yet...", !rule.TGR_ActionTypeInfo.HasWarnings());
		}

		void AssertEmptyFilterWarning(TagRule rule, string actionType)
		{
			rule.TGR_ActionType = actionType;
			Assert("The ActionType property should have a warning because there is no filter set, and yet...", rule.TGR_ActionTypeInfo.HasWarning(TagRuleValidation.FilterWarningMessage));
			Assert("The ActionType property should have a warning rather than an error since there is no filter set and because of the particular action type selected, and yet...", !rule.TGR_ActionTypeInfo.HasErrors());
		}

		static void AssertNoEmptyFilterNotifications(TagRule rule, string actionType)
		{
			rule.TGR_ActionType = actionType;
			Assert("The ActionType property should not have warnings or erros because there is a filter set, and yet...", !rule.TGR_ActionTypeInfo.HasNotifications());
		}

		void AssertNoConflictsWithOtherRules(TagRule rule)
		{
			rule.Validation.ValidateTGR_ActionType();
			AssertNoErrorContaining(rule.TGR_ActionTypeInfo, hasConflictingRulesPartialMessage);
		}

		void AssertHasConflictsWithOtherRules(string expectedMessage, TagRule rule)
		{
			rule.Validation.ValidateTGR_ActionType();
			AssertHasError($"Existing rules should cause {rule.TGR_Name} to have an error", rule.TGR_ActionTypeInfo, expectedMessage);
			AssertHasErrorContaining(rule.TGR_ActionTypeInfo, hasConflictingRulesPartialMessage);
		}

		const string hasConflictingRulesPartialMessage = "conflicting rule";

		#endregion
	}
}
