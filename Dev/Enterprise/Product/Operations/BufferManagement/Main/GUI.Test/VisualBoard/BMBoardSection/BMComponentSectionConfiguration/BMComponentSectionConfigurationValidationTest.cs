using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.BufferManagement.GUI.Test
{
	class BMComponentSectionConfigurationValidationTest : BusinessObjectValidationTestCase
	{
		#region ShowWorkInReleaseGroupOnly

		public void TestShowWorkInReleaseGroupOnly_WhenNoChannels_ShouldAddWarning()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var section = BMSTestHelper.CreateBoardSection(BMSTestHelper.CreateBuffer(system));
			var group = Factory.NewWithValidTestData<GlbGroup>();
			var resource = Factory.NewWithValidTestData<GlbStaff>();
			group.Staff.Add(resource);

			system.ReleaseGroups.AddNew().FSG_GG_Group = group.PK;

			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			AssertEquals(false, section.SectionConfiguration.ShowWorkInReleaseGroupOnly);
			AssertHasWarning(section.SectionConfiguration.ShowWorkInReleaseGroupOnlyInfo, "There may be many items shown on this section. We recommend filtering by Release Group to improve the performance of this board section.");

			section.SectionConfiguration.ShowWorkInReleaseGroupOnly = true;
			AssertNoWarnings(section.SectionConfiguration.ShowWorkInReleaseGroupOnlyInfo);

			section.SectionConfiguration.ShowWorkInReleaseGroupOnly = false;
			AssertHasWarning(section.SectionConfiguration.ShowWorkInReleaseGroupOnlyInfo, "There may be many items shown on this section. We recommend filtering by Release Group to improve the performance of this board section.");

			section.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.Resource;
			AssertEquals(1, section.SectionConfiguration.PrimaryAxisChannels.Count);
			AssertNoWarning(section.SectionConfiguration.ShowWorkInReleaseGroupOnlyInfo, "There may be many items shown on this section. We recommend filtering by Release Group to improve the performance of this board section.");

			section.SectionConfiguration.PrimaryAxisChannels.DeleteAll();
			section.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.NotChanneled;
			AssertHasWarning(section.SectionConfiguration.ShowWorkInReleaseGroupOnlyInfo, "There may be many items shown on this section. We recommend filtering by Release Group to improve the performance of this board section.");

			section.SectionConfiguration.ChannelSecondaryBy = ChannelTypeList.Codes.Resource;
			AssertEquals(1, section.SectionConfiguration.SecondaryAxisChannels.Count);
			AssertNoWarning(section.SectionConfiguration.ShowWorkInReleaseGroupOnlyInfo, "There may be many items shown on this section. We recommend filtering by Release Group to improve the performance of this board section.");
		}

	public void TestShowWorkInReleaseGroupOnly_WhenNoSystemReleaseGroups_ShouldNotAddWarning()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var section = BMSTestHelper.CreateBoardSection(BMSTestHelper.CreateBuffer(system));
			var group = Factory.NewWithValidTestData<GlbGroup>();
			var resource = Factory.NewWithValidTestData<GlbStaff>();
			group.Staff.Add(resource);

			section.SectionConfiguration.ShowWorkInReleaseGroupOnly = false;
			AssertEquals("GIVEN ShowWorkInReleaseGroupOnly = false", false, section.SectionConfiguration.ShowWorkInReleaseGroupOnly);
			AssertEquals("GIVEN no system release groups", false, system.ReleaseGroups.Any());
			AssertEquals("GIVEN No PrimaryAxisChannel", 0, section.SectionConfiguration.PrimaryAxisChannels.Count);
			AssertEquals("GIVEN No SecondaryAxisChannels", 0, section.SectionConfiguration.SecondaryAxisChannels.Count);
			AssertNoWarnings("WHEN validating THEN no warning", section.SectionConfiguration.ShowWorkInReleaseGroupOnlyInfo);

			system.ReleaseGroups.AddNew().FSG_GG_Group = group.PK;
			section.SectionConfiguration.ShowWorkInReleaseGroupOnly = false;
			AssertEquals("GIVEN ShowWorkInReleaseGroupOnly = false", false, section.SectionConfiguration.ShowWorkInReleaseGroupOnly);
			AssertEquals("GIVEN system release groups", true, system.ReleaseGroups.Any());
			AssertEquals("GIVEN No PrimaryAxisChannel", 0, section.SectionConfiguration.PrimaryAxisChannels.Count);
			AssertEquals("GIVEN No SecondaryAxisChannels", 0, section.SectionConfiguration.SecondaryAxisChannels.Count);
			AssertHasWarning("WHEN validationg THEN show warning", section.SectionConfiguration.ShowWorkInReleaseGroupOnlyInfo, "There may be many items shown on this section. We recommend filtering by Release Group to improve the performance of this board section.");

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource.PK);
			AssertEquals("GIVEN 1 PrimaryAxisChannel", 1, section.SectionConfiguration.PrimaryAxisChannels.Count);
			AssertEquals("GIVEN No SecondaryAxisChannels", 0, section.SectionConfiguration.SecondaryAxisChannels.Count);
			AssertHasWarning("WHEN validationg THEN show warning", section.SectionConfiguration.ShowWorkInReleaseGroupOnlyInfo, "There may be many items shown on this section. We recommend filtering by Release Group to improve the performance of this board section.");

			var resource2 = Factory.NewWithValidTestData<GlbStaff>();
			group.Staff.Add(resource2);
			BMSTestHelper.CreateSecondaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource2.PK);
			AssertEquals("GIVEN 1 PrimaryAxisChannel", 1, section.SectionConfiguration.PrimaryAxisChannels.Count);
			AssertEquals("GIVEN 1 SecondaryAxisChannels", 1, section.SectionConfiguration.SecondaryAxisChannels.Count);
			AssertNoWarning("WHEN validationg THEN show warning", section.SectionConfiguration.ShowWorkInReleaseGroupOnlyInfo, "There may be many items shown on this section. We recommend filtering by Release Group to improve the performance of this board section.");
		}

		#endregion

		#region Task Filters

		public void TestConfigurationTaskFilters_WhenHasCurrentTaskOnlyFilter_ShouldNotBeValid()
		{
			AssertFilter_WhenHasCurrentTaskOnlyFilter_ShouldNotBeValid(
				filter: BufferSection.TaskFilter,
				addCurrentTaskOnlyFilter: (filter) => CurrentTaskOnlyFilterTestHelper.AddFilterToTaskFilter(filter));
		}

		public void TestConfigurationTaskFilters_WhenHasNestedCurrentTaskOnlyFilter_ShouldNotBeValid()
		{
			AssertFilter_WhenHasCurrentTaskOnlyFilter_ShouldNotBeValid(
				filter: BufferSection.TaskFilter,
				addCurrentTaskOnlyFilter: (filter) => CurrentTaskOnlyFilterTestHelper.AddNestedFilterToTaskFilter(filter));
		}

		#endregion

		#region Workflow Filters

		public void TestConfigurationWorkflowFilters_WhenHasCurrentTaskOnlyFilter_ShouldNotBeValid()
		{
			AssertFilter_WhenHasCurrentTaskOnlyFilter_ShouldNotBeValid(
				filter: BufferSection.WorkflowFilter,
				addCurrentTaskOnlyFilter: (filter) => CurrentTaskOnlyFilterTestHelper.AddFilterToWorkflowFilter(filter));
		}

		public void TestConfigurationWorkflowFilters_WhenHasUserDefinedCurrentTaskOnlyFilter_ShouldNotBeValid()
		{
			AssertFilter_WhenHasCurrentTaskOnlyFilter_ShouldNotBeValid(
				filter: BufferSection.WorkflowFilter,
				addCurrentTaskOnlyFilter: (filter) => CurrentTaskOnlyFilterTestHelper.AddUserDefinedFilterToWorkflowFilter(filter));
		}

		#endregion

		#region Implementation

		void AssertFilter_WhenHasCurrentTaskOnlyFilter_ShouldNotBeValid(StmModuleFilter filter, Action<StmModuleFilter> addCurrentTaskOnlyFilter)
		{
			AssertNoErrors("Precondition: BufferSection.HasErrors", BufferSection);

			addCurrentTaskOnlyFilter(filter);

			BufferSection.Configuration.RunPreSaveValidation();

			var expectedMessage = "The 'Startable Task Only' filter cannot be chosen on board section filters. Use the configuration option labeled 'Enable Show Startable Items filter by default' instead.";
			AssertHasRowError("WHEN Section-Configuration has CurrentTaskOnly filter THEN should show error", filter, expectedMessage);
		}

		protected override void SetUp()
		{
			BMSTestHelper.EnableBMSInRegistry();

			Config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			BufferSection = Config.BufferSection;
		}

		VisualBoardTestConfig Config;
		BMBoardSection BufferSection;

		#endregion
	}
}
