using System;
using System.Drawing;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business;

namespace Enterprise.BufferManagement.Business.Test
{
	class BMBoardSectionValidationTest : BusinessObjectValidationTestCase
	{
		public void TestBackgroundColorCanBeEmptyOnBufferSections()
		{
			var buffer = BMSTestHelper.CreateBuffer(BMSTestHelper.CreateSystem(Factory));
			var section = BMSTestHelper.CreateBoardSection(buffer);
			var sectionConfiguration = section.SectionConfiguration;

			sectionConfiguration.ShowZones = false;
			section.BackgroundColor = "";

			AssertNoErrors(section.BackgroundColorInfo);
		}

		public void TestBackgroundColorCanBeEmptyOnBucketSections()
		{
			var bucket = BMSTestHelper.CreateBucket(BMSTestHelper.CreateSystem(Factory));
			var section = BMSTestHelper.CreateBoardSection(bucket);
			var sectionConfiguration = section.SectionConfiguration;

			section.BackgroundColor = "";

			AssertNoErrors(section.BackgroundColorInfo);
		}

		public void TestBackgroundPreventsTransparency()
		{
			var buffer = BMSTestHelper.CreateBuffer(BMSTestHelper.CreateSystem(Factory));
			var bucket = BMSTestHelper.CreateBucket(BMSTestHelper.CreateSystem(Factory));
			var bufferSection = BMSTestHelper.CreateBoardSection(buffer);
			var bucketSection = BMSTestHelper.CreateBoardSection(bucket);

			AssertNoErrors(bufferSection.BackgroundColorInfo);
			bufferSection.BackgroundColor = "Transparent";
			AssertHasError(bufferSection.BackgroundColorInfo, "Buffers and buckets cannot have transparent backgrounds");

			bufferSection.BackgroundColor = "Red";
			AssertNoErrors(bufferSection.BackgroundColorInfo);

			AssertNoErrors(bucketSection.BackgroundColorInfo);
			bucketSection.BackgroundColor = "Transparent";
			AssertHasError(bucketSection.BackgroundColorInfo, "Buffers and buckets cannot have transparent backgrounds");

			bucketSection.BackgroundColor = "Red";
			AssertNoErrors(bucketSection.BackgroundColorInfo);
		}

		public void TestFadeBackgroundAtPercentage()
		{
			var system = Factory.New<BMSystem>();
			var component = system.Components.AddNew();
			var section = system.Boards.AddNew().Sections.AddNew();
			section.MS_FC_Component = component.PK;
			var sectionConfiguration = section.SectionConfiguration;

			sectionConfiguration.Validation.ValidateAll();
			AssertEquals(0, sectionConfiguration.FadeBackgroundAtPercentage);
			AssertNoErrors(sectionConfiguration.FadeBackgroundAtPercentageInfo);

			component.FC_Type = BMComponentTypeList.Codes.Bucket;
			sectionConfiguration.CellsPerSubsection = 12;
			sectionConfiguration.FadeBackgroundAtPercentage = 10;
			AssertNoErrors(sectionConfiguration.FadeBackgroundAtPercentageInfo);

			sectionConfiguration.CellsPerSubsection = 0;
			AssertHasError(sectionConfiguration.FadeBackgroundAtPercentageInfo, "When 'Background Fade Percent' is greater than 0, 'Cells Per Subsection' must be greater than 1.");

			sectionConfiguration.CellsPerSubsection = 1;
			AssertHasError(sectionConfiguration.FadeBackgroundAtPercentageInfo, "When 'Background Fade Percent' is greater than 0, 'Cells Per Subsection' must be greater than 1.");

			sectionConfiguration.CellsPerSubsection = 2;
			AssertNoErrors(sectionConfiguration.FadeBackgroundAtPercentageInfo);

			component.FC_Type = BMComponentTypeList.Codes.Buffer;
			sectionConfiguration.FadeBackgroundAtPercentage = 20;
			AssertNoErrors(sectionConfiguration.FadeBackgroundAtPercentageInfo);

			sectionConfiguration.FadeBackgroundAtPercentage = 101;
			AssertHasError(sectionConfiguration.FadeBackgroundAtPercentageInfo, "Please enter a 'Background Fade Percentage' within the range 0 to 100.");
		}

		public void TestChangeSubsections_ShouldValidateLastCell()
		{
			var sectionConfiguration = Factory.New<BMSystem>().Boards.AddNew().Sections.AddNew().SectionConfiguration;
			sectionConfiguration.Subsections = 5;
			AssertHasError(sectionConfiguration.LastCellInfo, "Please enter a Last Cell.");
		}

		public void TestValidateShowWorkInReleaseGroupOnly()
		{
			var system = Factory.New<BMSystem>();
			var group = Factory.New<GlbGroup>();
			var releaseGroup = system.ReleaseGroups.AddNew();
			releaseGroup.FSG_GG_Group = group.PK;
			var sectionConfiguration = system.Boards.AddNew().Sections.AddNew().SectionConfiguration;

			sectionConfiguration.ShowWorkInReleaseGroupOnly = true;
			AssertHasWarning(sectionConfiguration.ShowWorkInReleaseGroupOnlyInfo, "Please select a Release Group.");
			sectionConfiguration.ReleaseGroupPK = group.PK;
			AssertNoWarning(sectionConfiguration.ShowWorkInReleaseGroupOnlyInfo, "Please select a Release Group.");
		}

		public void TestShowWorkInReleaseGroupOnly_WarningShouldBeSystemSpecific()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			var resource = Factory.NewWithValidTestData<GlbStaff>();
			group.Staff.Add(resource);

			var system1 = BMSTestHelper.CreateSystem(Factory, "ORG");
			var section1 = BMSTestHelper.CreateBoardSection(BMSTestHelper.CreateBuffer(system1));
			section1.SectionConfiguration.ShowWorkInReleaseGroupOnly = false;
			AssertNoWarnings("WHEN validating THEN no warning", section1.SectionConfiguration.ShowWorkInReleaseGroupOnlyInfo);

			var system2 = BMSTestHelper.CreateSystem(Factory, "WKI");
			var section2 = BMSTestHelper.CreateBoardSection(BMSTestHelper.CreateBucket(system2));
			system2.ReleaseGroups.AddNew().FSG_GG_Group = group.PK;
			section2.SectionConfiguration.ShowWorkInReleaseGroupOnly = false;
			AssertHasWarning("WHEN validationg THEN show warning", section2.SectionConfiguration.ShowWorkInReleaseGroupOnlyInfo, "There may be many items shown on this section. We recommend filtering by Release Group to improve the performance of this board section.");
			AssertNoWarnings("WHEN validating THEN no warning", section1.SectionConfiguration.ShowWorkInReleaseGroupOnlyInfo);
		}

		public void TestValidateIsReleaseScheduler()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var bucket = BMSTestHelper.CreateBucket(system);
			var buffer = BMSTestHelper.CreateBuffer(system);
			var section = BMSTestHelper.CreateBoardSection(bucket);
			var sectionConfiguration = section.SectionConfiguration;

			sectionConfiguration.IsReleaseScheduler = true;
			AssertHasError(sectionConfiguration.IsReleaseSchedulerInfo, "Release Scheduler board sections must be for a Buffer component.");

			sectionConfiguration.IsReleaseScheduler = false;
			AssertNoErrors(sectionConfiguration.IsReleaseSchedulerInfo);

			sectionConfiguration.IsReleaseScheduler = true;
			section.MS_FC_Component = buffer.PK;
			AssertNoErrors(sectionConfiguration.IsReleaseSchedulerInfo);
		}

		public void TestValidateCardType_ForReleaseSchedulerSections()
		{
			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);
			var section = BMSTestHelper.CreateReleaseSchedulerBoardSection(config.Buffer, config.ReleaseGroup);

			AssertEquals(CardTypeList.Codes.Workflow, section.SectionConfiguration.CardType);
			AssertNoErrors(section.SectionConfiguration.CardTypeInfo);

			section.SectionConfiguration.CardType = CardTypeList.Codes.JobLevelWorkflow;
			AssertHasError(section.SectionConfiguration.CardTypeInfo, "Release Scheduler board sections can only display one ticket per workflow. Please use the WKF option.");

			section.SectionConfiguration.CardType = CardTypeList.Codes.Task;
			AssertHasError(section.SectionConfiguration.CardTypeInfo, "Release Scheduler board sections can only display one ticket per workflow. Please use the WKF option.");

			section.SectionConfiguration.CardType = CardTypeList.Codes.Workflow;
			AssertNoErrors(section.SectionConfiguration.CardTypeInfo);
		}

		public void TestValidateStuffWhichDependsOnReleaseScheduler_Layout()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var buffer = BMSTestHelper.CreateBuffer(system);
			var section = BMSTestHelper.CreateBoardSection(buffer);
			var sectionConfiguration = section.SectionConfiguration;
			sectionConfiguration.IsReleaseScheduler = true;
			sectionConfiguration.Subsections = 4;
			sectionConfiguration.CellsPerSubsection = 10;
			sectionConfiguration.MaxOverdueSlots = 5;
			sectionConfiguration.FadeBackgroundAtPercentage = 70;

			AssertHasError(sectionConfiguration.SubsectionsInfo, "Must be only one Subsection on a Release Scheduler board section.");
			AssertHasError(sectionConfiguration.CellsPerSubsectionInfo, "Must be only one Cell Per Subsection on a Release Scheduler board section.");
			AssertHasError(sectionConfiguration.MaxOverdueSlotsInfo, "There should be no Overdue Slots specified for a Release Scheduler board section.");
			AssertHasError(sectionConfiguration.FadeBackgroundAtPercentageInfo, "Must be no Fade Percentage on a Release Scheduler board section.");
			AssertHasError(sectionConfiguration.CardTypeInfo, "Release Scheduler board sections can only display one ticket per workflow. Please use the WKF option.");

			sectionConfiguration.Subsections = 1;
			sectionConfiguration.CellsPerSubsection = 1;
			sectionConfiguration.MaxOverdueSlots = 0;
			sectionConfiguration.FadeBackgroundAtPercentage = 0;
			sectionConfiguration.CardType = CardTypeList.Codes.Workflow;

			AssertNoErrors(sectionConfiguration.SubsectionsInfo);
			AssertNoErrors(sectionConfiguration.CellsPerSubsectionInfo);
			AssertNoErrors(sectionConfiguration.MaxOverdueSlotsInfo);
			AssertNoErrors(sectionConfiguration.FadeBackgroundAtPercentageInfo);
			AssertNoErrors(sectionConfiguration.CardTypeInfo);
		}

		public void TestValidateStuffWhichDependsOnReleaseScheduler_Channels()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var buffer = BMSTestHelper.CreateBuffer(system);
			var section = BMSTestHelper.CreateBoardSection(buffer);
			var sectionConfiguration = section.SectionConfiguration;

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var resource = group.Staff.AddNew();

			sectionConfiguration.ReleaseGroupPK = group.PK;

			sectionConfiguration.ChannelBy = ChannelTypeList.Codes.Resource;
			sectionConfiguration.ChannelSecondaryBy = ChannelTypeList.Codes.Capability;
			sectionConfiguration.OverrideChannels = true;
			sectionConfiguration.OverrideSecondaryChannels = true;
			sectionConfiguration.ShowUnchanneled = true;
			sectionConfiguration.ShowSecondaryUnchanneled = true;
			sectionConfiguration.Validation.ValidateAll();

			AssertEquals(2, sectionConfiguration.PrimaryAxisChannels.Count);
			AssertEquals(1, sectionConfiguration.SecondaryAxisChannels.Count);

			sectionConfiguration.IsReleaseScheduler = true;

			AssertHasError(sectionConfiguration.ChannelByInfo, "Release Scheduler board sections should have Release Scheduler Channels specified since channels are automatically chosen based on constrained resources.");
			AssertHasError(sectionConfiguration.ChannelSecondaryByInfo, "Release Scheduler board sections should have Release Scheduler Channels specified since secondary channels are used for released and un-released work.");
			AssertHasError(sectionConfiguration.OverrideChannelsInfo, "Release Scheduler board sections should not override default channels since channels are automatically chosen based on constrained resources.");
			AssertHasError(sectionConfiguration.OverrideSecondaryChannelsInfo, "Secondary channels for a Buffer cannot be overridden.");
			AssertHasError(sectionConfiguration.ShowUnchanneledInfo, "Release Scheduler board sections should not show un-channeled work.");
			AssertHasError(sectionConfiguration.ShowSecondaryUnchanneledInfo, "Cannot show secondary un-channeled work for a Buffer.");

			AssertEquals(false, sectionConfiguration.ChannelByInfo.ReadOnly);
			AssertEquals(false, sectionConfiguration.ChannelSecondaryByInfo.ReadOnly);

			sectionConfiguration.ChannelBy = ChannelTypeList.Codes.ReleaseSchedulerChannels;
			sectionConfiguration.ChannelSecondaryBy = ChannelTypeList.Codes.ReleaseSchedulerChannels;
			sectionConfiguration.OverrideChannels = false;
			sectionConfiguration.OverrideSecondaryChannels = false;
			sectionConfiguration.ShowUnchanneled = false;
			sectionConfiguration.ShowSecondaryUnchanneled = false;
			sectionConfiguration.Validation.ValidateAll();

			AssertNoErrors(sectionConfiguration.ChannelByInfo);
			AssertNoErrors(sectionConfiguration.ChannelSecondaryByInfo);
			AssertNoErrors(sectionConfiguration.OverrideChannelsInfo);
			AssertNoErrors(sectionConfiguration.OverrideSecondaryChannelsInfo);
			AssertNoErrors(sectionConfiguration.ShowUnchanneledInfo);
			AssertNoErrors(sectionConfiguration.ShowSecondaryUnchanneledInfo);
		}

		public void TestValidateOverrideForResourceNotInReleaseGroup()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var buffer = BMSTestHelper.CreateBuffer(system);
			var section = BMSTestHelper.CreateBoardSection(buffer);
			var sectionConfiguration = section.SectionConfiguration;

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "AAA", "Aaron A. Aaronson");
			group.Staff.Add(resource1);
			var resource2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "BBB", "Bearon B. Bearonson");
			group.Staff.Add(resource2);
			var resource3 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "CCC", "Corin C. Corinson");
			group.Staff.Add(resource3);

			sectionConfiguration.ReleaseGroupPK = group.PK;
			sectionConfiguration.ChannelBy = ChannelTypeList.Codes.Resource;

			AssertEquals(false, sectionConfiguration.OverrideChannels);
			AssertEquals(3, sectionConfiguration.PrimaryAxisChannels.Count);

			group.Staff.Remove(resource3);

			sectionConfiguration.Validation.ValidateAll();

			AssertNoErrors(sectionConfiguration.OverrideChannelsInfo);
		}

		public void TestShowingUnChanneledChannelWhenUsingDefaultChannels()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var section = config.BufferSection;

			var resource = Factory.NewWithValidTestData<GlbStaff>();
			config.ReleaseGroup.Staff.Add(resource);

			section.SectionConfiguration.ReleaseGroupPK = config.ReleaseGroup.PK;
			section.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.Resource;
			section.SectionConfiguration.ShowUnchanneled = true;

			var unChanneledChannel = section.SectionConfiguration.PrimaryAxisChannels.Single(c => c.IsUnChanneled);
			AssertEquals(ChannelTypeList.Codes.Resource, unChanneledChannel.MSC_ChannelType);

			section.RunPreSaveValidation();

			AssertNoErrors(section);
		}

		public void TestChannelSecondaryBy_NoPrimaryChannels()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var bucket = BMSTestHelper.CreateBucket(system);
			var section = BMSTestHelper.CreateBoardSection(bucket);
			var sectionConfiguration = section.SectionConfiguration;

			sectionConfiguration.ChannelBy = ChannelTypeList.Codes.NotChanneled;
			BMSTestHelper.CreateSecondaryChannelForSection(section, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser.PK);

			sectionConfiguration.Validation.ValidateAll();

			Assert(!sectionConfiguration.PrimaryAxisChannels.Any());
			AssertHasError(sectionConfiguration.ChannelSecondaryByInfo, "When there are no primary channels, the only valid values for Channel Secondary By is by Time or Not Channeled");

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser.PK);

			sectionConfiguration.OverrideSecondaryChannels = false;
			sectionConfiguration.ChannelSecondaryBy = BMConstants.ChannelByTimeCode;
			AssertNoErrors(sectionConfiguration.ChannelSecondaryByInfo);

			sectionConfiguration.ChannelSecondaryBy = ChannelTypeList.Codes.NotChanneled;
			AssertNoErrors(sectionConfiguration.ChannelSecondaryByInfo);
		}

		public void TestChannelBy_NonReleaseSchedulerSection()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var buffer = BMSTestHelper.CreateBuffer(system);
			var section = BMSTestHelper.CreateBoardSection(buffer);
			var sectionConfiguration = section.SectionConfiguration;

			sectionConfiguration.ChannelBy = ChannelTypeList.Codes.ReleaseSchedulerChannels;
			sectionConfiguration.ChannelSecondaryBy = ChannelTypeList.Codes.ReleaseSchedulerChannels;
			AssertHasError(sectionConfiguration.ChannelByInfo, "Release Scheduler Channels are only valid on Release Scheduler board sections.");
			AssertHasError(sectionConfiguration.ChannelSecondaryByInfo, "Buffer components must have time as their secondary axis.");

			sectionConfiguration.ChannelBy = ChannelTypeList.Codes.Resource;
			sectionConfiguration.ChannelSecondaryBy = BMConstants.ChannelByTimeCode;
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser.PK);
			AssertNoErrors(sectionConfiguration.ChannelByInfo);
			AssertNoErrors(sectionConfiguration.ChannelSecondaryByInfo);
		}

		public void TestChannelAndUnchanneledTheSameTypeInBothAxes()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var buffer = BMSTestHelper.CreateBuffer(system);
			var section = BMSTestHelper.CreateBoardSection(buffer);
			var sectionConfiguration = section.SectionConfiguration;

			var resource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "RC1", "Resource 1");
			var capacity = BMSTestHelper.CreateCapability(Factory, "CP1", "Capability 1");

			string error = "Un-channeled shows items that do not belong in any channel. Having un-channeled by {0} and a {0} channel on the other axis will never return any results.";

			sectionConfiguration.ChannelBy = ChannelTypeList.Codes.Resource;

			sectionConfiguration.OverrideSecondaryChannels = true;
			sectionConfiguration.ShowSecondaryUnchanneled = true;
			var secChan = BMSTestHelper.CreateSecondaryChannelForSection(section, ChannelTypeList.Codes.Capability, capacity.PK);
			secChan.IsUnChanneled = true;
			sectionConfiguration.Validation.ValidateAll();
			AssertNoError("Primary By RES, Secondary Unchannelled by CAP", sectionConfiguration.ChannelSecondaryByInfo, string.Format(error, ChannelTypeList.Codes.Resource));

			sectionConfiguration.SecondaryAxisChannels.DeleteAll();
			secChan = BMSTestHelper.CreateSecondaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource.PK);
			secChan.IsUnChanneled = true;
			sectionConfiguration.Validation.ValidateAll();
			AssertHasError("Primary By RES, Secondary Unchannelled by RES", sectionConfiguration.ChannelSecondaryByInfo, string.Format(error, ChannelTypeList.Codes.Resource));
		}

		public void TestChannelAndUnchanneledTheSameTypeInBothAxesErrorMessage()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var buffer = BMSTestHelper.CreateBuffer(system);
			var section = BMSTestHelper.CreateBoardSection(buffer);
			var sectionConfiguration = section.SectionConfiguration;

			var resource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "RC1", "Resource 1");
			var capacity = BMSTestHelper.CreateCapability(Factory, "CP1", "Capability 1");
			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "TD1", "TagGroup 1");
			var tag = BMSTestHelper.CreateTagMagnitude(tagGroup, "TM1", "Tag 1");

			string error = "Un-channeled shows items that do not belong in any channel. Having un-channeled by {0} and a {0} channel on the other axis will never return any results.";

			sectionConfiguration.ChannelBy = ChannelTypeList.Codes.Resource;
			sectionConfiguration.OverrideSecondaryChannels = true;
			sectionConfiguration.ShowSecondaryUnchanneled = true;

			sectionConfiguration.SecondaryAxisChannels.DeleteAll();
			var secChan = BMSTestHelper.CreateSecondaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource.PK);
			secChan.IsUnChanneled = true;
			sectionConfiguration.Validation.ValidateAll();
			AssertHasError("Primary By RES, Secondary Unchanneled by RES", sectionConfiguration.ChannelSecondaryByInfo, string.Format(error, ChannelTypeList.Codes.Resource));

			sectionConfiguration.ChannelBy = ChannelTypeList.Codes.Capability;
			sectionConfiguration.SecondaryAxisChannels.DeleteAll();
			secChan = BMSTestHelper.CreateSecondaryChannelForSection(section, ChannelTypeList.Codes.Capability, capacity.PK);
			secChan.IsUnChanneled = true;
			sectionConfiguration.Validation.ValidateAll();
			AssertHasError("Primary By CAP, Secondary Unchanneled by CAP", sectionConfiguration.ChannelSecondaryByInfo, string.Format(error, ChannelTypeList.Codes.Capability));

			sectionConfiguration.ChannelBy = ChannelTypeList.Codes.Tag;
			sectionConfiguration.SecondaryAxisChannels.DeleteAll();
			secChan = BMSTestHelper.CreateSecondaryChannelForSection(section, ChannelTypeList.Codes.Tag, tag.PK);
			secChan.IsUnChanneled = true;
			sectionConfiguration.Validation.ValidateAll();
			AssertHasError("Primary By TAG, Secondary Unchanneled by TAG", sectionConfiguration.ChannelSecondaryByInfo, string.Format(error, ChannelTypeList.Codes.Tag));
		}

		public void TestReleaseGroup_Bucket()
		{
			var system = Factory.New<BMSystem>();
			var bucket = system.Components.AddNew();
			var releaseGroup = system.ReleaseGroups.AddNew();
			releaseGroup.FSG_GG_Group = Factory.New<GlbGroup>().PK;

			var section = system.Boards.AddNew().Sections.AddNew();
			section.MS_FC_Component = bucket.PK;
			var sectionConfiguration = section.SectionConfiguration;

			sectionConfiguration.Validation.ValidateAll();
			AssertHasError(sectionConfiguration.ReleaseGroupPKInfo, "Please enter a Release Group or add channels to this board section. Without these it is likely there would be a large volume of work shown on this board section.");

			sectionConfiguration.ReleaseGroupPK = releaseGroup.FSG_GG_Group;
			AssertNoErrors(sectionConfiguration.ReleaseGroupPKInfo);

			sectionConfiguration.ReleaseGroupPK = ZGuid.NewZGuid();
			AssertHasError(sectionConfiguration.ReleaseGroupPKInfo, "Enter a valid Release Group.");
		}

		public void TestReleaseGroup_Bucket_WithPrimaryChannel()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var resource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);

			var section = BMSTestHelper.CreateBoardSection(config.Bucket, setReleaseGroupIfRequired: false);
			var sectionConfiguration = section.SectionConfiguration;

			sectionConfiguration.Validation.ValidateAll();
			AssertHasError(sectionConfiguration.ReleaseGroupPKInfo, "Please enter a Release Group or add channels to this board section. Without these it is likely there would be a large volume of work shown on this board section.");

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource.PK);
			sectionConfiguration.Validation.ValidateAll();

			AssertNoErrors(sectionConfiguration.ReleaseGroupPKInfo);
		}

		public void TestReleaseGroup_Bucket_WithSecondaryChannel()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var resource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);

			var section = BMSTestHelper.CreateBoardSection(config.Bucket, setReleaseGroupIfRequired: false);
			var sectionConfiguration = section.SectionConfiguration;

			sectionConfiguration.Validation.ValidateAll();
			AssertHasError(sectionConfiguration.ReleaseGroupPKInfo, "Please enter a Release Group or add channels to this board section. Without these it is likely there would be a large volume of work shown on this board section.");

			BMSTestHelper.CreateSecondaryChannelForSection(section).MSC_ParentID = resource.PK;
			sectionConfiguration.Validation.ValidateAll();

			AssertNoErrors(sectionConfiguration.ReleaseGroupPKInfo);
		}

		public void TestReleaseGroup_Bucket_NoReleaseGroupsDefined()
		{
			var system = Factory.New<BMSystem>();
			var bucket = system.Components.AddNew();

			var section = system.Boards.AddNew().Sections.AddNew();
			var sectionConfiguration = section.SectionConfiguration;
			section.MS_FC_Component = bucket.PK;

			section.Validation.ValidateAll();
			AssertNoErrors(sectionConfiguration.ReleaseGroupPKInfo);
		}

		public void TestReleaseGroup_Buffer()
		{
			var system = Factory.New<BMSystem>();
			var component = system.Components.AddNew();
			component.FC_Type = BMComponentTypeList.Codes.Buffer;
			var releaseGroup = system.ReleaseGroups.AddNew();
			releaseGroup.FSG_GG_Group = Factory.New<GlbGroup>().PK;

			var section = system.Boards.AddNew().Sections.AddNew();
			var sectionConfiguration = section.SectionConfiguration;
			section.MS_FC_Component = component.PK;

			sectionConfiguration.Validation.ValidateAll();
			AssertNoErrors(sectionConfiguration.ReleaseGroupPKInfo);

			component.FC_Type = BMComponentTypeList.Codes.Bucket;
			sectionConfiguration.Validation.ValidateAll();
			AssertHasError(sectionConfiguration.ReleaseGroupPKInfo, "Please enter a Release Group or add channels to this board section. Without these it is likely there would be a large volume of work shown on this board section.");
		}

		public void TestReleaseGroup_NotInSystem()
		{
			var system = Factory.New<BMSystem>();
			var component = system.Components.AddNew();
			component.FC_Type = BMComponentTypeList.Codes.Buffer;
			var systemReleaseGroup = system.ReleaseGroups.AddNew();
			systemReleaseGroup.FSG_GG_Group = Factory.New<GlbGroup>().PK;

			var nonReleaseGroup = Factory.New<GlbGroup>();

			var section = system.Boards.AddNew().Sections.AddNew();
			var sectionConfiguration = section.SectionConfiguration;
			sectionConfiguration.ReleaseGroupPK = nonReleaseGroup.PK;
			section.MS_FC_Component = component.PK;

			sectionConfiguration.Validation.ValidateAll();
			AssertNoErrors(sectionConfiguration.ReleaseGroupPKInfo);

			component.FC_Type = BMComponentTypeList.Codes.Bucket;
			sectionConfiguration.Validation.ValidateAll();
			AssertNoErrors(sectionConfiguration.ReleaseGroupPKInfo);
		}

		public void TestValidateForegroundColor_Bucket()
		{
			var system = Factory.New<BMSystem>();
			var component = system.Components.AddNew();
			component.FC_Type = BMComponentTypeList.Codes.Bucket;

			var section = system.Boards.AddNew().Sections.AddNew();
			section.MS_FC_Component = component.PK;
			section.ForegroundColor = "Booo";
			AssertHasErrors(section.ForegroundColorInfo);
			section.ForegroundColor = "Blanched Almond";
			AssertNoErrors(section.ForegroundColorInfo);

			section.BackgroundColor = "Blanched Almond";
			section.Validation.ValidateAll();
			AssertHasError(section.ForegroundColorInfo, "The Foreground Color and Background Color cannot be the same.");
			AssertHasError(section.BackgroundColorInfo, "The Background Color and Foreground Color cannot be the same.");
		}

		public void TestValidateForegroundColor_Buffer()
		{
			var system = Factory.New<BMSystem>();
			var component = system.Components.AddNew();
			component.FC_Type = BMComponentTypeList.Codes.Buffer;

			var section = system.Boards.AddNew().Sections.AddNew();
			section.MS_FC_Component = component.PK;
			AssertEquals("Black", section.ForegroundColor);
			AssertEquals("", section.BackgroundColor);

			section.Validation.ValidateAll();
			AssertNoErrors(section.ForegroundColorInfo);
			AssertNoErrors(section.BackgroundColorInfo);

			section.BackgroundColor = "Blanched Almond";
			AssertNoErrors(section.BackgroundColorInfo);
			section.ForegroundColor = "Black";
			AssertNoErrors(section.ForegroundColorInfo);
		}

		public void TestValidateBackgroundColor()
		{
			var section = Factory.New<BMBoardSection>();
			section.BackgroundColor = "Booo";
			AssertHasErrors(section.BackgroundColorInfo);
			section.BackgroundColor = "Blanched Almond";
			AssertNoErrors(section.BackgroundColorInfo);
		}

		public void TestValidateZoneColors()
		{
			var section = Factory.New<BMBoardSection>();
			var configuration = section.SectionConfiguration;

			configuration.BufferZone0Color = Color.Transparent.Name;
			configuration.BufferZone1Color = Color.Transparent.Name;
			configuration.BufferZone2Color = Color.Transparent.Name;
			configuration.BufferZone3Color = Color.Transparent.Name;

			AssertHasErrors(configuration.BufferZone0ColorInfo);
			AssertHasErrors(configuration.BufferZone1ColorInfo);
			AssertHasErrors(configuration.BufferZone2ColorInfo);
			AssertHasErrors(configuration.BufferZone3ColorInfo);

			configuration.BufferZone0Color = Color.Tomato.Name;
			configuration.BufferZone1Color = Color.Blue.Name;
			configuration.BufferZone2Color = Color.Yellow.Name;
			configuration.BufferZone3Color = Color.Moccasin.Name;

			AssertNoErrors(configuration.BufferZone0ColorInfo);
			AssertNoErrors(configuration.BufferZone1ColorInfo);
			AssertNoErrors(configuration.BufferZone2ColorInfo);
			AssertNoErrors(configuration.BufferZone3ColorInfo);
		}

		public void TestValidateRowSpan()
		{
			var section = Factory.New<BMBoardSection>();
			section.RowSpan = 0;
			AssertHasErrors(section.RowSpanInfo);
			section.RowSpan = 1;
			AssertNoErrors(section.RowSpanInfo);
		}

		public void TestValidateColSpan()
		{
			var section = Factory.New<BMBoardSection>();
			section.ColSpan = 0;
			AssertHasErrors(section.ColSpanInfo);
			section.ColSpan = 1;
			AssertNoErrors(section.ColSpanInfo);
		}

		public void TestValidateRow()
		{
			var board = Factory.New<BMBoard>();
			var section1 = Factory.New<BMBoardSection>();
			board.Sections.Add(section1);

			section1.Row = -1;
			AssertHasError(section1.RowInfo, "Please enter a 'Row' greater than or equal to 0.");
			section1.Row = 0;
			AssertNoError(section1.RowInfo, "Please enter a 'Row' greater than or equal to 0.");

			var section2 = Factory.New<BMBoardSection>();
			board.Sections.Add(section2);

			section2.Row = 0;
			AssertHasError(section2.RowInfo, "Column and Row combinations must be unique.");
			AssertHasError(section2.ColumnInfo, "Column and Row combinations must be unique.");

			section2.Row = 1;
			AssertNoError(section2.RowInfo, "Column and Row combinations must be unique.");
			AssertNoError(section2.ColumnInfo, "Column and Row combinations must be unique.");

			section1.Row = 1;
			AssertHasError(section1.RowInfo, "Column and Row combinations must be unique.");
			AssertHasError(section1.ColumnInfo, "Column and Row combinations must be unique.");
		}

		public void TestValidateCellsIntersection()
		{
			var board = Factory.New<BMBoard>();
			var section1 = Factory.New<BMBoardSection>();
			board.Sections.Add(section1);

			var section2 = Factory.New<BMBoardSection>();
			board.Sections.Add(section2);
			section2.Row = 1;
			section2.ColSpan = 2;
			AssertNoErrors(section2.RowInfo);
			AssertNoErrors(section2.ColumnInfo);
			AssertNoErrors(section2.RowSpanInfo);
			AssertNoErrors(section2.ColSpanInfo);

			var section3 = Factory.New<BMBoardSection>();
			board.Sections.Add(section3);

			section3.Column = 1;
			AssertNoErrors(section3.RowInfo);
			AssertNoErrors(section3.ColumnInfo);
			AssertNoErrors(section3.RowSpanInfo);
			AssertNoErrors(section3.ColSpanInfo);

			section3.RowSpan = 2;
			AssertHasError(section3.RowSpanInfo, "Sections should not be intersecting.");
			AssertNoErrors(section3.ColSpanInfo);
		}

		public void TestValidateColumn()
		{
			var section = Factory.New<BMBoardSection>();
			section.Column = -1;
			AssertHasErrors(section.ColumnInfo);
			section.Column = 0;
			AssertNoErrors(section.ColumnInfo);
		}

		public void TestValidateMS_FC_Component()
		{
			var board = Factory.New<BMBoard>();
			var section1 = board.Sections.AddNew();
			var section2 = board.Sections.AddNew();

			var cmp1 = Factory.New<BMComponent>();
			cmp1.FC_Type = BMComponentTypeList.Codes.Bucket;
			var cmp2 = Factory.New<BMComponent>();
			cmp2.FC_Type = BMComponentTypeList.Codes.Bucket;

			section1.MS_FC_Component = cmp1.PK;
			section2.MS_FC_Component = cmp1.PK;

			section1.Validation.ValidateAll();
			section2.Validation.ValidateAll();

			AssertNoErrors(section1.MS_FC_ComponentInfo);
			AssertNoErrors(section2.MS_FC_ComponentInfo);

			section2.MS_FC_Component = cmp2.PK;

			section1.Validation.ValidateAll();
			section2.Validation.ValidateAll();

			AssertNoErrors(section1.MS_FC_ComponentInfo);
			AssertNoErrors(section2.MS_FC_ComponentInfo);

			cmp1.FC_Type = BMComponentTypeList.Codes.Constraint;
			section1.Validation.ValidateAll();
			AssertHasError(section1.MS_FC_ComponentInfo, "Cannot add a component type other than Buffer or Bucket to a Buffer Board.");

			section1.MS_FC_Component = ZGuid.Empty;
			AssertHasError(section1.MS_FC_ComponentInfo, "Please enter a Component.");

			section1.MS_FC_Component = ZGuid.NewZGuid();
			AssertHasError(section1.MS_FC_ComponentInfo, "Enter a valid Component.");
		}

		public void TestValidateMS_FC_Component_ComponentSectionType()
		{
			var board = Factory.New<BMBoard>();
			var section = board.Sections.AddNew();
			section.MS_SectionType = BMConstants.ComponentSectionType;

			section.Validation.ValidateAll();
			AssertHasError(section.MS_FC_ComponentInfo, "Please enter a Component.");

			section.MS_FC_Component = ZGuid.NewZGuid();
			AssertHasError(section.MS_FC_ComponentInfo, "Enter a valid Component.");

			var cmp = Factory.New<BMComponent>();
			cmp.FC_Type = BMComponentTypeList.Codes.Bucket;
			section.MS_FC_Component = cmp.PK;

			AssertNoErrors(section.MS_FC_ComponentInfo);
		}

		public void TestValidateMS_FC_Component_NonComponentSectionType()
		{
			var board = Factory.New<BMBoard>();
			var section = board.Sections.AddNew();
			section.MS_SectionType = BMConstants.ModuleGridSectionType;

			section.Validation.ValidateAll();
			AssertNoErrors(section.MS_FC_ComponentInfo);
		}

		public void TestValidateComponentBelongingToAnotherSystem()
		{
			var system1 = Factory.New<BMSystem>();
			var component11 = BMSTestHelper.CreateBucket(system1, "bucket 1");
			var system2 = Factory.New<BMSystem>();
			var component21 = BMSTestHelper.CreateBucket(system2, "bucket 2");
			var component22 = BMSTestHelper.CreateBucket(system2, "bucket 3");

			var board = BMSTestHelper.CreateBoard(system1, "I'm Board");
			var section = BMSTestHelper.CreateBoardSection(component21, board);
			var additionalSection = BMSTestHelper.CreateAdditionalComponent(section, component22);

			board.Validation.ValidateAll();

			AssertNoErrors(additionalSection.BSA_MS_SectionInfo);
			AssertNoErrors(section.MS_FC_ComponentInfo);
		}

		public void TestValidateSizePercentages()
		{
			var board = Factory.New<BMBoard>();
			var section = board.Sections.AddNew();

			section.ColWidthPercent = 50;
			section.RowHeightPercent = 50;

			AssertNoError(section.ColWidthPercentInfo, "Please enter a 'Column Width Percent' within the range 0 to 100.");
			AssertNoError(section.RowHeightPercentInfo, "Please enter a 'Row Height Percent' within the range 0 to 100.");

			section.ColWidthPercent = -1;
			section.RowHeightPercent = 101;

			AssertHasError(section.ColWidthPercentInfo, "Please enter a 'Column Width Percent' within the range 0 to 100.");
			AssertHasError(section.RowHeightPercentInfo, "Please enter a 'Row Height Percent' within the range 0 to 100.");

			section.ColWidthPercent = 101;
			section.RowHeightPercent = -1;

			AssertHasError(section.ColWidthPercentInfo, "Please enter a 'Column Width Percent' within the range 0 to 100.");
			AssertHasError(section.RowHeightPercentInfo, "Please enter a 'Row Height Percent' within the range 0 to 100.");
		}

		public void TestValidateSubsections()
		{
			var section = Factory.New<BMBoardSection>();
			var sectionConfiguration = section.SectionConfiguration;
			AssertEquals("Default value", 1, sectionConfiguration.Subsections);
			AssertNoErrors(sectionConfiguration.SubsectionsInfo);

			sectionConfiguration.Subsections = 0;
			AssertHasError(sectionConfiguration.SubsectionsInfo, "Please enter a 'Subsections' greater than or equal to 1.");

			sectionConfiguration.Subsections = 1;
			AssertNoError(sectionConfiguration.SubsectionsInfo, "Please enter a 'Subsections' greater than or equal to 1.");
		}

		public void TestValidateShowZones()
		{
			var section = Factory.New<BMBoardSection>();
			AssertEquals(true, section.SectionConfiguration.ShowZones);

			var system = Factory.New<BMSystem>();
			var component = system.Components.AddNew();
			component.FC_Type = BMComponentTypeList.Codes.Bucket;

			section.MS_FC_Component = component.PK;
			section.SectionConfiguration.Validation.ValidateShowZones();
			AssertNoErrors(section.SectionConfiguration.ShowZonesInfo);
			section.SectionConfiguration.ShowZones = true;
			AssertHasError(section.SectionConfiguration.ShowZonesInfo, "Zones must be not turned on for Bucket sections.");

			component.FC_Type = BMComponentTypeList.Codes.Buffer;
			section.SectionConfiguration.ShowZones = false;
			AssertHasWarning(section.SectionConfiguration.ShowZonesInfo, "Zones will not be visible on this buffer section.");
			AssertNoErrors(section.SectionConfiguration.ShowZonesInfo);

			section.SectionConfiguration.ShowZones = true;
			AssertNoWarnings(section.SectionConfiguration.ShowZonesInfo);
			AssertNoErrors(section.SectionConfiguration.ShowZonesInfo);

			section.SectionConfiguration.IsReleaseScheduler = true;
			section.SectionConfiguration.ShowZones = true;
			AssertHasError(section.SectionConfiguration.ShowZonesInfo, "Zones must be not turned on for Release Scheduler sections.");

			section.SectionConfiguration.ShowZones = false;
			AssertNoErrors(section.SectionConfiguration.ShowZonesInfo);
		}

		public void TestValidateShowChildComponentZones()
		{
			var section = Factory.New<BMBoardSection>();
			AssertEquals(false, section.SectionConfiguration.ShowChildComponentZones);

			var system = Factory.New<BMSystem>();
			var component = system.Components.AddNew();
			component.FC_Type = BMComponentTypeList.Codes.Bucket;

			section.MS_FC_Component = component.PK;
			section.SectionConfiguration.Validation.ValidateShowZones();
			AssertNoErrors(section.SectionConfiguration.ShowChildComponentZonesInfo);
			section.SectionConfiguration.ShowChildComponentZones = true;
			AssertHasError(section.SectionConfiguration.ShowChildComponentZonesInfo, "Child component zones must be not turned on for Bucket sections.");

			component.FC_Type = BMComponentTypeList.Codes.Buffer;
			section.SectionConfiguration.ShowZones = false;
			AssertHasError(section.SectionConfiguration.ShowChildComponentZonesInfo, "In order to display child component zones, Show Zones needs to be selected.");

			section.SectionConfiguration.ShowZones = true;
			AssertNoErrors(section.SectionConfiguration.ShowChildComponentZonesInfo);

			component.FC_Type = BMComponentTypeList.Codes.Bucket;
			section.SectionConfiguration.ShowChildComponentZones = false;
			AssertNoErrors(section.SectionConfiguration.ShowChildComponentZonesInfo);
		}

		public void TestValidatePanelLayoutStyle()
		{
			var section = Factory.New<BMBoardSection>();
			section.SectionConfiguration.PanelLayoutStyle = ZString.Empty;
			section.SectionConfiguration.Validation.ValidatePanelLayoutStyle();
			AssertHasError(section.SectionConfiguration.PanelLayoutStyleInfo, "Please enter a Panel Layout.");

			var system = Factory.New<BMSystem>();
			var component = system.Components.AddNew();
			component.FC_Type = BMComponentTypeList.Codes.Bucket;

			section.MS_FC_Component = component.PK;
			section.SectionConfiguration.Validation.ValidatePanelLayoutStyle();
			AssertNoErrors(section.SectionConfiguration.PanelLayoutStyleInfo);
			section.SectionConfiguration.PanelLayoutStyle = PanelLayoutTypeList.Codes.Stacked;
			AssertNoErrors(section.SectionConfiguration.PanelLayoutStyleInfo);
			section.SectionConfiguration.PanelLayoutStyle = PanelLayoutTypeList.Codes.Staggered;
			AssertNoErrors(section.SectionConfiguration.PanelLayoutStyleInfo);

			component.FC_Type = BMComponentTypeList.Codes.Buffer;
			section.SectionConfiguration.PanelLayoutStyle = PanelLayoutTypeList.Codes.Stacked;
			AssertNoErrors(section.SectionConfiguration.PanelLayoutStyleInfo);
			section.SectionConfiguration.PanelLayoutStyle = PanelLayoutTypeList.Codes.Staggered;
			AssertNoErrors(section.SectionConfiguration.PanelLayoutStyleInfo);

			section.SectionConfiguration.IsReleaseScheduler = true;
			section.SectionConfiguration.PanelLayoutStyle = PanelLayoutTypeList.Codes.Staggered;
			AssertHasError(section.SectionConfiguration.PanelLayoutStyleInfo, "Release Scheduler sections must use the Stacked Panel Layout Style.");

			section.SectionConfiguration.PanelLayoutStyle = PanelLayoutTypeList.Codes.Stacked;
			AssertNoErrors(section.SectionConfiguration.PanelLayoutStyleInfo);
		}

		public void TestValidateFlowDirection()
		{
			var sectionConfiguration = Factory.New<BMSystem>().Boards.AddNew().Sections.AddNew().SectionConfiguration;

			sectionConfiguration.Validation.ValidateAll();
			AssertNoError(sectionConfiguration.FlowDirectionInfo, "Please enter a Flow Direction.");

			sectionConfiguration.FlowDirection = ZString.Empty;
			AssertHasError(sectionConfiguration.FlowDirectionInfo, "Please enter a Flow Direction.");

			sectionConfiguration.FlowDirection = "boo";
			AssertHasError(sectionConfiguration.FlowDirectionInfo, "Enter a valid Flow Direction.");

			sectionConfiguration.FlowDirection = FlowDirectionList.Codes.Down;
			AssertNoError(sectionConfiguration.FlowDirectionInfo, "Enter a valid Flow Direction.");
		}

		public void TestValidateTimeProgressionMode()
		{
			var system = Factory.New<BMSystem>();
			var bucket = system.Components.AddNew();
			var buffer = system.Components.AddNew();
			buffer.FC_Type = BMComponentTypeList.Codes.Buffer;
			var section = system.Boards.AddNew().Sections.AddNew();
			var sectionConfiguration = section.SectionConfiguration;
			section.MS_FC_Component = bucket.PK;

			section.Validation.ValidateAll();
			AssertNoError(sectionConfiguration.TimeProgressionModeInfo, "Please enter a Time Progression Mode.");

			sectionConfiguration.TimeProgressionMode = ZString.Empty;
			AssertHasError(sectionConfiguration.TimeProgressionModeInfo, "Please enter a Time Progression Mode.");

			sectionConfiguration.TimeProgressionMode = "boo";
			AssertHasError(sectionConfiguration.TimeProgressionModeInfo, "Enter a valid Time Progression Mode.");

			sectionConfiguration.TimeProgressionMode = TimeProgressionModeList.Codes.Due;
			AssertNoError(sectionConfiguration.TimeProgressionModeInfo, "Enter a valid Time Progression Mode.");

			section.MS_FC_Component = buffer.PK;
			AssertEquals(TimeProgressionModeList.Codes.Age, sectionConfiguration.TimeProgressionMode);
			AssertNoError(sectionConfiguration.TimeProgressionModeInfo, "Only the Age Time Progression Mode is valid for a buffer.");

			sectionConfiguration.TimeProgressionMode = TimeProgressionModeList.Codes.Due;
			AssertHasError(sectionConfiguration.TimeProgressionModeInfo, "Only the Age Time Progression Mode is valid for a buffer.");
		}

		public void TestValidateTimeField()
		{
			var sectionConfiguration = Factory.New<BMSystem>().Boards.AddNew().Sections.AddNew().SectionConfiguration;

			sectionConfiguration.TimeField = ZString.Empty;
			AssertHasError(sectionConfiguration.TimeFieldInfo, "Please enter a Date/Time for Progression.");

			sectionConfiguration.TimeField = "boo";
			AssertHasError(sectionConfiguration.TimeFieldInfo, "Enter a valid Date/Time for Progression.");

			sectionConfiguration.TimeField = TimeProgressionFieldList.Codes.TransferTime;
			AssertNoError(sectionConfiguration.TimeFieldInfo, "Enter a valid Date/Time for Progression.");

			sectionConfiguration.TimeProgressionMode = TimeProgressionModeList.Codes.Age;
			sectionConfiguration.TimeField = TimeProgressionFieldList.Codes.WorkingTimeSinceStartable;
			AssertNoError(sectionConfiguration.TimeFieldInfo, "Enter a valid Date/Time for Progression.");

			sectionConfiguration.TimeProgressionMode = TimeProgressionModeList.Codes.Due;
			sectionConfiguration.TimeField = TimeProgressionFieldList.Codes.WorkingTimeSinceStartable;
			AssertHasError(sectionConfiguration.TimeFieldInfo, "Only the Age Time Progression Mode is valid for Working Time Since Task Became Startable.");
		}

		public void TestValidateMaxOverdueSlots()
		{
			var system = Factory.New<BMSystem>();
			var component = system.Components.AddNew();
			var section = system.Boards.AddNew().Sections.AddNew();
			var sectionConfiguration = section.SectionConfiguration;
			section.MS_FC_Component = component.PK;

			section.Validation.ValidateAll();
			AssertNoError(sectionConfiguration.MaxOverdueSlotsInfo, "The Overdue Slots must be less than or equal to the Cells Per Subsection.");

			sectionConfiguration.CellsPerSubsection = 5;
			sectionConfiguration.MaxOverdueSlots = 10;
			AssertHasError(sectionConfiguration.MaxOverdueSlotsInfo, "The Overdue Slots must be less than or equal to the Cells Per Subsection.");

			sectionConfiguration.MaxOverdueSlots = 5;
			AssertNoError(sectionConfiguration.MaxOverdueSlotsInfo, "The Overdue Slots must be less than or equal to the Cells Per Subsection.");

			sectionConfiguration.MaxOverdueSlots = -1;
			AssertHasError(sectionConfiguration.MaxOverdueSlotsInfo, "Please enter an 'Overdue Slots' greater than or equal to 0.");

			component.FC_Type = BMComponentTypeList.Codes.Buffer;
			sectionConfiguration.MaxOverdueSlots = 1;
			AssertHasError(sectionConfiguration.MaxOverdueSlotsInfo, "There should be no Overdue Slots specified for a Buffer component.");

			sectionConfiguration.MaxOverdueSlots = 0;
			AssertNoErrors(sectionConfiguration.MaxOverdueSlotsInfo);
		}

		public void TestValidateOverdueBackgroundColor()
		{
			var sectionConfiguration = Factory.New<BMSystem>().Boards.AddNew().Sections.AddNew().SectionConfiguration;

			sectionConfiguration.Validation.ValidateAll();
			AssertNoErrors(sectionConfiguration.OverdueBackgroundColorInfo);

			sectionConfiguration.OverdueBackgroundColor = "boo";
			AssertHasError(sectionConfiguration.OverdueBackgroundColorInfo, "Enter a valid Overdue Background Color.");

			sectionConfiguration.OverdueBackgroundColor = Color.Aqua.Name;
			AssertNoError(sectionConfiguration.OverdueBackgroundColorInfo, "Enter a valid Overdue Background Color.");
		}

		public void TestValidateOverdueForegroundColor()
		{
			var sectionConfiguration = Factory.New<BMSystem>().Boards.AddNew().Sections.AddNew().SectionConfiguration;

			sectionConfiguration.Validation.ValidateAll();
			AssertNoErrors(sectionConfiguration.OverdueForegroundColorInfo);

			sectionConfiguration.OverdueForegroundColor = "boo";
			AssertHasError(sectionConfiguration.OverdueForegroundColorInfo, "Enter a valid Overdue Foreground Color.");

			sectionConfiguration.OverdueForegroundColor = Color.Aqua.Name;
			AssertNoError(sectionConfiguration.OverdueForegroundColorInfo, "Enter a valid Overdue Foreground Color.");
		}

		public void TestValidateLastCell()
		{
			var sectionConfiguration = Factory.New<BMSystem>().Boards.AddNew().Sections.AddNew().SectionConfiguration;

			sectionConfiguration.Validation.ValidateAll();
			AssertNoError("Should not require LastCell when not wrapped", sectionConfiguration.LastCellInfo, "Please enter a Last Cell.");

			sectionConfiguration.Subsections = 2;
			sectionConfiguration.Validation.ValidateLastCell();
			AssertHasError(sectionConfiguration.LastCellInfo, "Please enter a Last Cell.");

			sectionConfiguration.LastCell = "boo";
			AssertHasError(sectionConfiguration.LastCellInfo, "Enter a valid Last Cell.");

			sectionConfiguration.LastCell = LastCellList.Codes.Bottom;
			AssertHasError(sectionConfiguration.LastCellInfo, "Enter a valid Last Cell.");

			sectionConfiguration.FlowDirection = FlowDirectionList.Codes.Right;
			sectionConfiguration.Validation.ValidateLastCell();
			AssertNoError(sectionConfiguration.LastCellInfo, "Enter a valid Last Cell.");
		}

		public void TestValidateSubsectionCells_Bucket()
		{
			var system = Factory.New<BMSystem>();
			var component = system.Components.AddNew();
			component.FC_Type = BMComponentTypeList.Codes.Bucket;
			var board = Factory.New<BMBoard>();
			var section = board.Sections.AddNew();
			var sectionConfiguration = section.SectionConfiguration;
			section.MS_FC_Component = component.PK;
			AssertEquals("Default value", 1, sectionConfiguration.CellsPerSubsection);
			AssertNoErrors(sectionConfiguration.CellsPerSubsectionInfo);

			sectionConfiguration.CellsPerSubsection = -1;
			AssertHasError(sectionConfiguration.CellsPerSubsectionInfo, "Please enter a 'Cells Per Subsection' greater than or equal to 0.");

			sectionConfiguration.CellsPerSubsection = 0;
			AssertNoErrors(sectionConfiguration.CellsPerSubsectionInfo);
		}

		public void TestValidateSubsectionCells_Buffer()
		{
			var system = Factory.New<BMSystem>();
			var component = system.Components.AddNew();
			component.FC_Type = BMComponentTypeList.Codes.Buffer;
			var board = Factory.New<BMBoard>();
			var section = board.Sections.AddNew();
			section.MS_FC_Component = component.PK;
			var sectionConfiguration = section.SectionConfiguration;

			sectionConfiguration.CellsPerSubsection = 10;
			AssertNoError(sectionConfiguration.CellsPerSubsectionInfo, "Please enter a 'Cells Per Subsection' greater than or equal to 4.");

			sectionConfiguration.CellsPerSubsection = 1;
			AssertHasError(sectionConfiguration.CellsPerSubsectionInfo, "Please enter a 'Cells Per Subsection' greater than or equal to 4.");

			sectionConfiguration.CellsPerSubsection = 3;
			AssertHasError(sectionConfiguration.CellsPerSubsectionInfo, "Please enter a 'Cells Per Subsection' greater than or equal to 4.");

			sectionConfiguration.CellsPerSubsection = 4;
			AssertNoError(sectionConfiguration.CellsPerSubsectionInfo, "Please enter a 'Cells Per Subsection' greater than or equal to 4.");

			sectionConfiguration.Subsections = 2;
			sectionConfiguration.CellsPerSubsection = 2;
			AssertNoError(sectionConfiguration.CellsPerSubsectionInfo, "Please enter a 'Cells Per Subsection' greater than or equal to 2.");

			sectionConfiguration.Subsections = 3;
			sectionConfiguration.CellsPerSubsection = 0;
			AssertHasError(sectionConfiguration.CellsPerSubsectionInfo, "Please enter a 'Cells Per Subsection' greater than or equal to 2.");

			sectionConfiguration.Subsections = 3;
			sectionConfiguration.CellsPerSubsection = 1;
			AssertHasError(sectionConfiguration.CellsPerSubsectionInfo, "Please enter a 'Cells Per Subsection' greater than or equal to 2.");

			sectionConfiguration.Subsections = 4;
			sectionConfiguration.CellsPerSubsection = 0;
			AssertHasError(sectionConfiguration.CellsPerSubsectionInfo, "Please enter a 'Cells Per Subsection' greater than or equal to 1.");

			sectionConfiguration.ShowZones = true;
			sectionConfiguration.CellsPerSubsection = 1;
			sectionConfiguration.Subsections = 4;
			AssertNoError(sectionConfiguration.SubsectionsInfo, "Please enter a 'Cells Per Subsection' greater than or equal to 1.");

			sectionConfiguration.Subsections = 3;
			AssertHasError(sectionConfiguration.CellsPerSubsectionInfo, "Please enter a 'Cells Per Subsection' greater than or equal to 2.");

			sectionConfiguration.Subsections = 1;
			AssertHasError(sectionConfiguration.CellsPerSubsectionInfo, "Please enter a 'Cells Per Subsection' greater than or equal to 4.");
		}

		public void TestValidateSubsectionCells_Buffer_DontShowZones()
		{
			var system = Factory.New<BMSystem>();
			var component = system.Components.AddNew();
			component.FC_Type = BMComponentTypeList.Codes.Buffer;
			var board = Factory.New<BMBoard>();
			var section = board.Sections.AddNew();
			var sectionConfiguration = section.SectionConfiguration;
			section.MS_FC_Component = component.PK;
			sectionConfiguration.ShowZones = false;
			sectionConfiguration.CellsPerSubsection = 1;
			AssertEquals("Default value", 1, sectionConfiguration.CellsPerSubsection);
			AssertNoErrors(sectionConfiguration.CellsPerSubsectionInfo);

			sectionConfiguration.CellsPerSubsection = -1;
			AssertHasError(sectionConfiguration.CellsPerSubsectionInfo, "Please enter a 'Cells Per Subsection' greater than or equal to 0.");

			for (var i = 0; i <= 4; i++)
			{
				sectionConfiguration.CellsPerSubsection = i;
				AssertNoErrors("CellsPerSubsection = " + i, sectionConfiguration.CellsPerSubsectionInfo);
			}
		}

		public void TestValidateTimePerCell_ValidDateTime()
		{
			var system = Factory.New<BMSystem>();
			var component = system.Components.AddNew();

			var section = system.Boards.AddNew().Sections.AddNew();
			var sectionConfiguration = section.SectionConfiguration;
			sectionConfiguration.CellsPerSubsection = 2;
			section.MS_FC_Component = component.PK;

			sectionConfiguration.TimePerCell = ZDateTime.Today;
			AssertNoError(sectionConfiguration.TimePerCellInfo, "Enter a valid Time Per Cell.");

			sectionConfiguration.TimePerCell = ZDateTime.Invalid;
			AssertHasError(sectionConfiguration.TimePerCellInfo, "Enter a valid Time Per Cell.");

			sectionConfiguration.TimePerCell = ZDateTime.Today;
			AssertNoError(sectionConfiguration.TimePerCellInfo, "Enter a valid Time Per Cell.");
		}

		public void TestValidateTimePerCell_Bucket()
		{
			var system = Factory.New<BMSystem>();
			var component = system.Components.AddNew();

			var section = system.Boards.AddNew().Sections.AddNew();
			var sectionConfiguration = section.SectionConfiguration;
			sectionConfiguration.CellsPerSubsection = 2;
			section.MS_FC_Component = component.PK;

			sectionConfiguration.TimePerCell = ZDateTime.Empty;
			sectionConfiguration.Validation.ValidateAll();
			AssertHasError(sectionConfiguration.TimePerCellInfo, "Please enter a Time Per Cell.");

			sectionConfiguration.TimePerCell = ZDateTime.Today;
			AssertNoError(sectionConfiguration.TimePerCellInfo, "Please enter a Time Per Cell.");
		}

		public void TestValidateTimePerCell_Bucket_MoreThanOneSubsection()
		{
			var system = Factory.New<BMSystem>();
			var component = system.Components.AddNew();

			var section = system.Boards.AddNew().Sections.AddNew();
			var sectionConfiguration = section.SectionConfiguration;
			section.MS_FC_Component = component.PK;

			sectionConfiguration.TimePerCell = ZDateTime.Empty;
			sectionConfiguration.Validation.ValidateAll();
			AssertNoError(sectionConfiguration.TimePerCellInfo, "Please enter a Time Per Cell.");

			sectionConfiguration.CellsPerSubsection = 2;
			AssertHasError(sectionConfiguration.TimePerCellInfo, "Please enter a Time Per Cell.");
		}

		public void TestValidateTimePerCell_Bucket_SecondaryChannelByTime()
		{
			var system = Factory.New<BMSystem>();
			var component = system.Components.AddNew();

			var section = system.Boards.AddNew().Sections.AddNew();
			var sectionConfiguration = section.SectionConfiguration;
			section.MS_FC_Component = component.PK;
			sectionConfiguration.CellsPerSubsection = 2;
			sectionConfiguration.ChannelSecondaryBy = ChannelTypeList.Codes.Capability;

			sectionConfiguration.TimePerCell = ZDateTime.Empty;
			sectionConfiguration.Validation.ValidateAll();
			AssertNoError(sectionConfiguration.TimePerCellInfo, "Please enter a Time Per Cell.");

			sectionConfiguration.ChannelSecondaryBy = BMConstants.ChannelByTimeCode;
			AssertHasError(sectionConfiguration.TimePerCellInfo, "Please enter a Time Per Cell.");
		}

		public void TestValidateTimePerCell_Buffer()
		{
			var system = Factory.New<BMSystem>();
			var component = system.Components.AddNew();
			component.FC_Type = BMComponentTypeList.Codes.Buffer;

			var section = system.Boards.AddNew().Sections.AddNew();
			var sectionConfiguration = section.SectionConfiguration;
			section.MS_FC_Component = component.PK;

			sectionConfiguration.TimePerCell = ZDateTime.Empty;
			sectionConfiguration.Validation.ValidateAll();
			AssertNoErrors(sectionConfiguration.TimePerCellInfo);
		}

		public void TestValidateChannelBy()
		{
			var system = Factory.New<BMSystem>();
			var section = system.Boards.AddNew().Sections.AddNew();
			var sectionConfiguration = section.SectionConfiguration;

			AssertEquals(ChannelTypeList.Codes.NotChanneled, sectionConfiguration.ChannelBy);
			section.Validation.ValidateAll();
			AssertNoError(sectionConfiguration.ChannelByInfo, "Please enter a Channel By.");

			sectionConfiguration.ChannelBy = ZString.Empty;
			AssertHasError(sectionConfiguration.ChannelByInfo, "Please enter a Channel By.");

			sectionConfiguration.OverrideChannels = true;
			AssertNoError(sectionConfiguration.ChannelByInfo, "Please enter a Channel By.");

			sectionConfiguration.ChannelBy = "boo";
			AssertHasError(sectionConfiguration.ChannelByInfo, "Enter a valid Channel By.");
		}

		public void TestValidateChannelSecondaryBy()
		{
			var system = Factory.New<BMSystem>();
			var section = system.Boards.AddNew().Sections.AddNew();
			var sectionConfiguration = section.SectionConfiguration;

			AssertEquals(BMConstants.ChannelByTimeCode, sectionConfiguration.ChannelSecondaryBy);
			section.Validation.ValidateAll();
			AssertNoError(sectionConfiguration.ChannelSecondaryByInfo, "Please enter a Channel By.");

			sectionConfiguration.ChannelSecondaryBy = ZString.Empty;
			AssertHasError(sectionConfiguration.ChannelSecondaryByInfo, "Please enter a Channel By.");

			sectionConfiguration.OverrideSecondaryChannels = true;
			sectionConfiguration.ChannelSecondaryBy = BMConstants.ChannelByTimeCode;
			AssertHasError(sectionConfiguration.ChannelSecondaryByInfo, "Cannot show time units on the secondary axis when channels are overridden.");
			sectionConfiguration.OverrideSecondaryChannels = false;
			AssertNoError(sectionConfiguration.ChannelSecondaryByInfo, "Cannot show time units on the secondary axis when channels are overridden.");

			sectionConfiguration.ChannelSecondaryBy = "boo";
			AssertHasError(sectionConfiguration.ChannelSecondaryByInfo, "Enter a valid Channel By.");

			sectionConfiguration.ChannelSecondaryBy = ChannelTypeList.Codes.NotChanneled;
			sectionConfiguration.CellsPerSubsection = 1;
			AssertNoErrors(sectionConfiguration.ChannelSecondaryByInfo);

			sectionConfiguration.CellsPerSubsection = 2;
			AssertHasError(sectionConfiguration.ChannelSecondaryByInfo, "When there is more than one cell per subsection, the only valid value for Channel Secondary By is by Time.");

			sectionConfiguration.ChannelSecondaryBy = BMConstants.ChannelByTimeCode;
			AssertNoError(sectionConfiguration.ChannelSecondaryByInfo, "When there is more than one cell per subsection, the only valid value for Channel Secondary By is by Time.");

			sectionConfiguration.ChannelSecondaryBy = ZString.Empty;
			AssertNoError(sectionConfiguration.ChannelSecondaryByInfo, "When there is more than one cell per subsection, the only valid value for Channel Secondary By is by Time.");
		}

		public void TestValidateChannelSecondaryBy_Buffer()
		{
			var system = Factory.New<BMSystem>();
			var buffer = system.Components.AddNew();
			buffer.FC_Type = BMComponentTypeList.Codes.Buffer;
			var section = system.Boards.AddNew().Sections.AddNew();
			section.MS_FC_Component = buffer.PK;
			var sectionConfiguration = section.SectionConfiguration;

			sectionConfiguration.ChannelSecondaryBy = ChannelTypeList.Codes.Capability;
			AssertHasError(sectionConfiguration.ChannelSecondaryByInfo, "Buffer components must have time as their secondary axis.");

			sectionConfiguration.ChannelSecondaryBy = BMConstants.ChannelByTimeCode;
			AssertNoError(sectionConfiguration.ChannelSecondaryByInfo, "Buffer components must have time as their secondary axis.");
		}

		public void TestValidateOverrideSecondaryChannels_Buffer()
		{
			var system = Factory.New<BMSystem>();
			var buffer = system.Components.AddNew();
			buffer.FC_Type = BMComponentTypeList.Codes.Buffer;
			var section = system.Boards.AddNew().Sections.AddNew();
			section.MS_FC_Component = buffer.PK;
			var sectionConfiguration = section.SectionConfiguration;

			sectionConfiguration.OverrideSecondaryChannels = true;
			AssertHasError(sectionConfiguration.OverrideSecondaryChannelsInfo, "Secondary channels for a Buffer cannot be overridden.");

			sectionConfiguration.OverrideSecondaryChannels = false;
			AssertNoError(sectionConfiguration.OverrideSecondaryChannelsInfo, "Secondary channels for a Buffer cannot be overridden.");
		}

		public void TestValidateOverrideSecondaryChannels_Bucket()
		{
			var system = Factory.New<BMSystem>();
			var buffer = system.Components.AddNew();
			buffer.FC_Type = BMComponentTypeList.Codes.Bucket;
			var section = system.Boards.AddNew().Sections.AddNew();
			section.MS_FC_Component = buffer.PK;
			var sectionConfiguration = section.SectionConfiguration;

			sectionConfiguration.OverrideSecondaryChannels = true;
			AssertNoErrors(sectionConfiguration.OverrideSecondaryChannelsInfo);

			sectionConfiguration.OverrideSecondaryChannels = false;
			AssertNoErrors(sectionConfiguration.OverrideSecondaryChannelsInfo);
		}

		public void TestValidateShowSecondaryUnChanneled_Buffer()
		{
			var system = Factory.New<BMSystem>();
			var buffer = system.Components.AddNew();
			buffer.FC_Type = BMComponentTypeList.Codes.Buffer;
			var section = system.Boards.AddNew().Sections.AddNew();
			section.MS_FC_Component = buffer.PK;
			var sectionConfiguration = section.SectionConfiguration;

			sectionConfiguration.ShowSecondaryUnchanneled = true;
			AssertHasError(sectionConfiguration.ShowSecondaryUnchanneledInfo, "Cannot show secondary un-channeled work for a Buffer.");

			sectionConfiguration.ShowSecondaryUnchanneled = false;
			AssertNoError(sectionConfiguration.ShowSecondaryUnchanneledInfo, "Cannot show secondary un-channeled work for a Buffer.");
		}

		public void TestValidateShowSecondaryUnChanneled_Bucket()
		{
			var system = Factory.New<BMSystem>();
			var buffer = system.Components.AddNew();
			buffer.FC_Type = BMComponentTypeList.Codes.Bucket;
			var section = system.Boards.AddNew().Sections.AddNew();
			section.MS_FC_Component = buffer.PK;
			var sectionConfiguration = section.SectionConfiguration;

			sectionConfiguration.ShowSecondaryUnchanneled = true;
			AssertNoErrors(sectionConfiguration.ShowSecondaryUnchanneledInfo);

			sectionConfiguration.ShowSecondaryUnchanneled = false;
			AssertNoErrors(sectionConfiguration.ShowSecondaryUnchanneledInfo);
		}

		#region Section Type

		public void TestSectionType()
		{
			var section = Factory.New<BMBoardSection>();
			AssertEquals(BMConstants.ComponentSectionType, section.MS_SectionType);
			AssertNoErrors(section.MS_SectionTypeInfo);

			section.MS_SectionType = ZString.Empty;
			AssertHasError(section.MS_SectionTypeInfo, "Please enter a Type.");

			section.MS_SectionType = "ARG";
			AssertHasError(section.MS_SectionTypeInfo, "Enter a valid Type.");

			section.MS_SectionType = BMConstants.ComponentSectionType;
			AssertNoErrors(section.MS_SectionTypeInfo);
		}

		public void TestSectionType_PlanningManagementEnabled()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.PlanningManagement);
			BMSRegistry.Instance.EnableMENTSections.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var section = Factory.New<BMBoardSection>();

			foreach (var sectionType in new SectionTypeList().ToArray().Select(x => x.Code))
			{
				if (sectionType == "WEB")
				{
					continue;
				}
				section.MS_SectionType = sectionType;

				AssertNoErrors("Full Planning Management is enabled, so all section types should be allowed.", section.MS_SectionTypeInfo);
			}
		}

		public void TestSectionType_BufferManagementEnabled()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.IncludesBufferManagement);
			BMSRegistry.Instance.EnableMENTSections.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var section = Factory.New<BMBoardSection>();
			var allowedSectionTypes = new[] { BMConstants.ComponentSectionType, BMConstants.ModuleGridSectionType, BMConstants.MENTSectionType };

			foreach (var sectionType in new SectionTypeList().ToArray().Select(x => x.Code))
			{
				section.MS_SectionType = sectionType;

				if (allowedSectionTypes.Contains(sectionType))
				{
					AssertNoErrors("This section type should be allowed when Buffer Management is enabled.", section.MS_SectionTypeInfo);
				}
				else
				{
					AssertHasError(section.MS_SectionTypeInfo,
						"The selected section type is only available when the registry item [Workflow Manager -> Buffer Management -> Workflow Management Mode] is set to 'PLN - Enhanced Workflow, Buffer, and Planning Management'.");
				}
			}
		}

		public void TestSectionType_EnhancedWorkflowManagementEnabled()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.EnhancedWorkflow);

			var section = Factory.New<BMBoardSection>();
			var allowedSectionTypes = new[] { BMConstants.ComponentSectionType, BMConstants.ModuleGridSectionType };

			foreach (var sectionType in new SectionTypeList().ToArray().Select(x => x.Code))
			{
				section.MS_SectionType = sectionType;

				if (allowedSectionTypes.Contains(sectionType))
				{
					AssertNoErrors("This section type should be allowed when Enhanced Workflow Management only is enabled.", section.MS_SectionTypeInfo);
				}
				else
				{
					var expectedMessage = sectionType == "MNT"
						? "The selected section type is only available when the registry item [Workflow Manager -> Buffer Management -> Workflow Management Mode] is set to 'BUF - Enhanced Workflow and Buffer Management' or 'PLN - Enhanced Workflow, Buffer, and Planning Management'."
						: "The selected section type is only available when the registry item [Workflow Manager -> Buffer Management -> Workflow Management Mode] is set to 'PLN - Enhanced Workflow, Buffer, and Planning Management'.";
					AssertHasError(section.MS_SectionTypeInfo, expectedMessage);
				}
			}
		}

		public void TestSectionType_MENTSection_ShouldErrorByFDefault()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.PlanningManagement);

			var section = Factory.New<BMBoardSection>();
			section.MS_SectionType = BMConstants.MENTSectionType;
			AssertHasError("MENT sections no longer supported by default", section.MS_SectionTypeInfo, "MENT sections are no longer supported.");
		}

		public void TestSectionType_WEBSection_ShouldError()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.PlanningManagement);

			var section = Factory.New<BMBoardSection>();
			section.MS_SectionType = BMConstants.WEBSectionType;
			AssertHasError("WEB sections no longer supported by default", section.MS_SectionTypeInfo, "WEB sections are no longer supported.");
		}

		#endregion

		public void TestSectionNameOverrideShouldBeDefinedWhenOverridden()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var sectionConfig = config.BufferSection.SectionConfiguration;

			sectionConfig.SectionNameIsOverridden = true;

			sectionConfig.SectionNameOverride = ZString.Empty;
			AssertHasError("There should be an error when section name is overridden, but section name override is empty", sectionConfig.SectionNameOverrideInfo, "Custom section name cannot be empty when overriding section name.");

			sectionConfig.SectionNameOverride = "   ";
			AssertHasError("There should be an error when section name is overridden, but section name override is a whitespace", sectionConfig.SectionNameOverrideInfo, "Custom section name cannot be empty when overriding section name.");

			sectionConfig.SectionNameOverride = "I'll override you!";
			AssertNoNotifications("There should not be any warnings or errors section name is overridden with a non-empty string", sectionConfig.SectionNameOverrideInfo);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestHelper.EnableBMSInRegistry();
		}

		#endregion
	}
}
