using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.VisualBoards.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(BMBoardSection))]
	public class BMBoardSectionTest : EnterpriseBusinessObjectTestCase
	{
		#region Clone

		public void TestCloneCopiesAdditionalComponents()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var bucket = BMSTestHelper.CreateBucket(system);
			var buffer = BMSTestHelper.CreateBuffer(system);
			var section = BMSTestHelper.CreateBoardSection(buffer);
			var additionalComponent = section.SectionConfiguration.AdditionalComponents.AddNew();
			additionalComponent.BSA_FC_Component = bucket.PK;

			var clone = (BMBoardSection)section.Clone();

			AssertEquals(1, clone.SectionConfiguration.AdditionalComponents.Count);
			AssertEquals(clone.PK, clone.SectionConfiguration.AdditionalComponents.Single().BSA_MS_Section);
			AssertEquals(1, section.SectionConfiguration.AdditionalComponents.Count);
			AssertEquals(section.PK, section.SectionConfiguration.AdditionalComponents.Single().BSA_MS_Section);
		}

		public void TestCloneHandlesDefaultValues()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var buffer = BMSTestHelper.CreateBuffer(system);
			var section = BMSTestHelper.CreateBoardSection(buffer);

			Factory.Save();

			section.SectionConfiguration.ShowZones = false;
			AssertEquals("Precondition: Cells Per Subsection should be defaulted to 10 for a buffer", 10, section.SectionConfiguration.CellsPerSubsection);

			Factory.Save();

			var clone = (BMBoardSection)section.Clone();

			AssertEquals(false, section.SectionConfiguration.ShowZones);
			AssertEquals(10, section.SectionConfiguration.CellsPerSubsection);
		}

		public void TestClone_ShouldCopyLinksToCustomisedLayouts()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var buffer = BMSTestHelper.CreateBuffer(system);
			var section = BMSTestHelper.CreateBoardSection(buffer);

			var layout1 = BMControlCustomisation.GetNewDefaultCardLayout(Factory, CustomisedControlTypeList.Codes.DetailedCard);
			var layout2 = BMControlCustomisation.GetNewDefaultCardLayout(Factory, CustomisedControlTypeList.Codes.TaskCard);
			var layout3 = BMControlCustomisation.GetNewDefaultCardLayout(Factory, CustomisedControlTypeList.Codes.WorkflowDetailedCard);
			var layout4 = BMControlCustomisation.GetNewDefaultCardLayout(Factory, CustomisedControlTypeList.Codes.WorkflowSummaryCard);

			BMSTestHelper.CreateControlCustomisationLink(Factory, section, layout1);
			BMSTestHelper.CreateControlCustomisationLink(Factory, section, layout2);
			BMSTestHelper.CreateControlCustomisationLink(Factory, section, layout3);
			BMSTestHelper.CreateControlCustomisationLink(Factory, section, layout4);

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var loadedSection = newFactory.Load<BMBoardSection>(section.PK);
			var clone = (BMBoardSection)loadedSection.Clone();

			AssertEquals(4, clone.SectionConfiguration.CustomisedLayoutLinks.Count);

			AssertCollectionContains(clone.SectionConfiguration.CustomisedLayoutLinks, l => l.FML_FM_ControlCustomisation == layout1.PK);
			AssertCollectionContains(clone.SectionConfiguration.CustomisedLayoutLinks, l => l.FML_FM_ControlCustomisation == layout2.PK);
			AssertCollectionContains(clone.SectionConfiguration.CustomisedLayoutLinks, l => l.FML_FM_ControlCustomisation == layout3.PK);
			AssertCollectionContains(clone.SectionConfiguration.CustomisedLayoutLinks, l => l.FML_FM_ControlCustomisation == layout4.PK);
		}

		public void TestClone_ShouldCloneChannels()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var capability1 = BMSTestHelper.CreateCapability(Factory, "AAA", "aaa");
			var capability2 = BMSTestHelper.CreateCapability(Factory, "BBB", "bbb");
			var capability3 = BMSTestHelper.CreateCapability(Factory, "CCC", "ccc");

			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var resource2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);

			config.ReleaseGroup.Staff.AddRange(new[] { resource1, resource2 });

			config.BufferSection.SectionConfiguration.ReleaseGroupPK = config.ReleaseGroup.PK;
			config.BufferSection.SectionConfiguration.OverrideChannels = true;
			config.BufferSection.SectionConfiguration.ChannelSecondaryBy = ChannelTypeList.Codes.Resource;

			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection).MSC_ParentID = capability1.PK;
			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection).MSC_ParentID = capability2.PK;
			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection).MSC_ParentID = capability3.PK;

			config.BucketSection.SectionConfiguration.ReleaseGroupPK = config.ReleaseGroup.PK;
			config.BucketSection.SectionConfiguration.OverrideSecondaryChannels = true;
			config.BucketSection.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.Resource;

			BMSTestHelper.CreateSecondaryChannelForSection(config.BucketSection).MSC_ParentID = capability1.PK;
			BMSTestHelper.CreateSecondaryChannelForSection(config.BucketSection).MSC_ParentID = capability2.PK;
			BMSTestHelper.CreateSecondaryChannelForSection(config.BucketSection).MSC_ParentID = capability3.PK;

			AssertEquals(3, config.BufferSection.SectionConfiguration.PrimaryAxisChannels.Count);
			AssertEquals(2, config.BufferSection.SectionConfiguration.SecondaryAxisChannels.Count);
			AssertEquals(2, config.BucketSection.SectionConfiguration.PrimaryAxisChannels.Count);
			AssertEquals(3, config.BucketSection.SectionConfiguration.SecondaryAxisChannels.Count);

			var bufferSectionClone = (BMBoardSection)config.BufferSection.Clone();
			var bucketSectionClone = (BMBoardSection)config.BucketSection.Clone();

			AssertEquals(3, bufferSectionClone.SectionConfiguration.PrimaryAxisChannels.Count);
			AssertEquals(2, bufferSectionClone.SectionConfiguration.SecondaryAxisChannels.Count);
			AssertEquals(2, bucketSectionClone.SectionConfiguration.PrimaryAxisChannels.Count);
			AssertEquals(3, bucketSectionClone.SectionConfiguration.SecondaryAxisChannels.Count);

			AssertEquals(capability1.PK, bufferSectionClone.SectionConfiguration.PrimaryAxisChannels[0].MSC_ParentID);
			AssertEquals(capability2.PK, bufferSectionClone.SectionConfiguration.PrimaryAxisChannels[1].MSC_ParentID);
			AssertEquals(capability3.PK, bufferSectionClone.SectionConfiguration.PrimaryAxisChannels[2].MSC_ParentID);

			AssertEquals(resource1.PK, bufferSectionClone.SectionConfiguration.SecondaryAxisChannels[0].MSC_ParentID);
			AssertEquals(resource2.PK, bufferSectionClone.SectionConfiguration.SecondaryAxisChannels[1].MSC_ParentID);

			AssertEquals(resource1.PK, bucketSectionClone.SectionConfiguration.PrimaryAxisChannels[0].MSC_ParentID);
			AssertEquals(resource2.PK, bucketSectionClone.SectionConfiguration.PrimaryAxisChannels[1].MSC_ParentID);

			AssertEquals(capability1.PK, bucketSectionClone.SectionConfiguration.SecondaryAxisChannels[0].MSC_ParentID);
			AssertEquals(capability2.PK, bucketSectionClone.SectionConfiguration.SecondaryAxisChannels[1].MSC_ParentID);
			AssertEquals(capability3.PK, bucketSectionClone.SectionConfiguration.SecondaryAxisChannels[2].MSC_ParentID);

			AssertNoErrors(bufferSectionClone);
			AssertNoErrors(bucketSectionClone);

			bucketSectionClone.SectionConfiguration.SecondaryAxisChannels[0].MSC_ParentID = bucketSectionClone.SectionConfiguration.SecondaryAxisChannels[1].MSC_ParentID;

			AssertHasError(bucketSectionClone.SectionConfiguration.SecondaryAxisChannels[0].MSC_ParentIDInfo, "Cannot have the same channel more than once on one board section.");
		}

		#endregion

		#region Channels

		public void TestOverridePrimaryChannels_DefaultSecondaryChannels()
		{
			var system = Factory.New<BMSystem>();
			var resource1 = Factory.NewWithValidTestData<GlbStaff>();
			var resource2 = Factory.NewWithValidTestData<GlbStaff>();
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.Staff.AddRange(resource1, resource2);

			var section = system.Boards.AddNew().Sections.AddNew();
			var sectionConfiguration = section.SectionConfiguration;
			sectionConfiguration.OverrideChannels = true;
			sectionConfiguration.ReleaseGroupPK = group.PK;
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, Factory.New<GlbStaff>().PK);

			AssertEquals(0, sectionConfiguration.SecondaryAxisChannels.Count);
			sectionConfiguration.ChannelSecondaryBy = ChannelTypeList.Codes.Resource;
			AssertEquals(2, sectionConfiguration.SecondaryAxisChannels.Count);
		}

		abstract class BoardSectionChannelsTest : BMSTestCaseWithFactory
		{
			public void TestToggleUnchanneledChannel_DefaultChannels()
			{
				var section = Factory.New<BMSystem>().Boards.AddNew().Sections.AddNew();
				GetChannelByInfo(section).Value = (ZString)ChannelTypeList.Codes.Resource;

				AssertEquals(0, GetChannels(section).Count);

				GetShowUnchanneledInfo(section).Value = ZBool.True;
				AssertEquals(1, GetChannels(section).Count);
				AssertEquals(true, GetChannels(section)[0].IsUnChanneled);
				AssertEquals(ChannelTypeList.Codes.Resource, GetChannels(section)[0].MSC_ChannelType);

				GetShowUnchanneledInfo(section).Value = ZBool.False;
				AssertEquals(0, GetChannels(section).Count);
			}

			public void TestToggleUnchanneledChannel_OverridenChannels()
			{
				var section = Factory.New<BMSystem>().Boards.AddNew().Sections.AddNew();
				GetOverrideChannelsInfo(section).Value = ZBool.True;

				AssertEquals(0, GetChannels(section).Count);

				GetShowUnchanneledInfo(section).Value = ZBool.True;
				AssertEquals(1, GetChannels(section).Count);
				AssertEquals(true, GetChannels(section)[0].IsUnChanneled);
				AssertEquals(ZString.Empty, GetChannels(section)[0].MSC_ChannelType);

				GetShowUnchanneledInfo(section).Value = ZBool.False;
				AssertEquals(0, GetChannels(section).Count);
			}

			public void TestToggleUnchanneled_ShouldNotDoStoopidThings()
			{
				var section = Factory.New<BMSystem>().Boards.AddNew().Sections.AddNew();

				AssertEquals(0, GetChannels(section).Count);

				GetShowUnchanneledInfo(section).Value = ZBool.True;
				AssertEquals("Should add un-channeled channel", 1, GetChannels(section).Count);
				AssertEquals(true, GetChannels(section)[0].IsUnChanneled);

				GetShowUnchanneledInfo(section).Value = ZBool.True;
				AssertEquals("Should leave un-channeled channel since 'Show Unchanneled' is still activated", 1, GetChannels(section).Count);
				AssertEquals(true, GetChannels(section)[0].IsUnChanneled);

				GetShowUnchanneledInfo(section).Value = ZBool.False;

				AssertEquals("Should remove un-channeled channel", 0, GetChannels(section).Count);

				GetShowUnchanneledInfo(section).Value = ZBool.False;
				AssertEquals("Should not add un-channeled channel since 'Show Unchanneled' is still deactivated", 0, GetChannels(section).Count);
			}

			public void TestOverrideChannels_ShouldClearChannelBy()
			{
				var system = Factory.New<BMSystem>();

				var resource1 = Factory.NewWithValidTestData<GlbStaff>();
				var resource2 = Factory.NewWithValidTestData<GlbStaff>();
				var group = Factory.NewWithValidTestData<GlbGroup>();
				group.Staff.AddRange(resource1, resource2);

				var section = system.Boards.AddNew().Sections.AddNew();
				section.SectionConfiguration.ReleaseGroupPK = group.PK;
				GetChannelByInfo(section).Value = (ZString)ChannelTypeList.Codes.Resource;
				AssertEquals(2, GetChannels(section).Count);

				GetOverrideChannelsInfo(section).Value = ZBool.True;
				AssertEquals(ZString.Empty, GetChannelByInfo(section).Value);
				AssertEquals(2, GetChannels(section).Count);
			}

			public void TestChannels()
			{
				var staff1 = Factory.NewWithValidTestData<GlbStaff>();
				var staff2 = Factory.NewWithValidTestData<GlbStaff>();
				var group1 = Factory.NewWithValidTestData<GlbGroup>();
				var group2 = Factory.NewWithValidTestData<GlbGroup>();
				var capability1 = Factory.NewWithValidTestData<GlbCapability>();
				var capability2 = Factory.NewWithValidTestData<GlbCapability>();

				var system = Factory.NewWithValidTestData<BMSystem>();
				var bucket1 = system.Components.AddNew();
				bucket1.FC_Name = "bucket1";
				var bucket2 = system.Components.AddNew();
				bucket2.FC_Name = "bucket2";

				var board = system.Boards.AddNew();
				var section1 = board.Sections.AddNew();
				section1.MS_FC_Component = bucket1.PK;
				GetOverrideChannelsInfo(section1).Value = ZBool.True;
				var section2 = board.Sections.AddNew();
				section2.MS_FC_Component = bucket2.PK;
				GetOverrideChannelsInfo(section2).Value = ZBool.True;

				var channel1_1 = GetChannels(section1).AddNew();
				channel1_1.MSC_ChannelType = ChannelTypeList.Codes.Resource;
				channel1_1.MSC_ParentID = staff1.PK;
				var channel1_2 = GetChannels(section1).AddNew();
				channel1_2.MSC_ChannelType = ChannelTypeList.Codes.Group;
				channel1_2.MSC_ParentID = group1.PK;
				var channel1_3 = GetChannels(section1).AddNew();
				channel1_3.MSC_ChannelType = ChannelTypeList.Codes.Capability;
				channel1_3.MSC_ParentID = capability1.PK;

				var channel2_1 = GetChannels(section2).AddNew();
				channel2_1.MSC_ChannelType = ChannelTypeList.Codes.Resource;
				channel2_1.MSC_ParentID = staff2.PK;
				var channel2_2 = GetChannels(section2).AddNew();
				channel2_2.MSC_ChannelType = ChannelTypeList.Codes.Group;
				channel2_2.MSC_ParentID = group2.PK;
				var channel2_3 = GetChannels(section2).AddNew();
				channel2_3.MSC_ChannelType = ChannelTypeList.Codes.Capability;
				channel2_3.MSC_ParentID = capability2.PK;

				Factory.Save();

				var loadedSystem = new BusinessObjectFactory().Load<BMSystem>(system.PK);
				var loadedSection1 = loadedSystem.Boards[0].Sections.FirstOrDefault(s => s.MS_FC_Component == bucket1.PK);
				var loadedSection2 = loadedSystem.Boards[0].Sections.FirstOrDefault(s => s.MS_FC_Component == bucket2.PK);

				AssertEquals(3, GetChannels(loadedSection1).Count);
				var channels = GetChannels(loadedSection1).Cast<BMBoardSectionChannel>();
				Assert(channels.Any(c => c.MSC_ParentID == staff1.PK));
				Assert(channels.Any(c => c.MSC_ParentID == group1.PK));
				Assert(channels.Any(c => c.MSC_ParentID == capability1.PK));

				AssertEquals(3, GetChannels(loadedSection2).Count);
				channels = GetChannels(loadedSection2).Cast<BMBoardSectionChannel>();
				Assert(channels.Any(c => c.MSC_ParentID == staff2.PK));
				Assert(channels.Any(c => c.MSC_ParentID == group2.PK));
				Assert(channels.Any(c => c.MSC_ParentID == capability2.PK));
			}

			public void TestChannels_ShouldAddNewChannelsAsRecordsAreAdded()
			{
				var group = Factory.NewWithValidTestData<GlbGroup>();
				var staff1 = Factory.NewWithValidTestData<GlbStaff>();
				var staff2 = Factory.NewWithValidTestData<GlbStaff>();
				var staff3 = Factory.NewWithValidTestData<GlbStaff>();
				group.Staff.AddRange(new[] { staff1, staff2, staff3 });

				var system = Factory.NewWithValidTestData<BMSystem>();
				var releaseGroup = system.ReleaseGroups.AddNew();
				releaseGroup.FSG_GG_Group = group.PK;

				var section = system.Boards.AddNew().Sections.AddNew();
				section.SectionConfiguration.ReleaseGroupPK = group.PK;
				GetChannelByInfo(section).Value = (ZString)ChannelTypeList.Codes.Resource;
				AssertEquals(3, GetChannels(section).Count);

				Factory.Save();

				var newFactory = new BusinessObjectFactory();

				var loadedGroup = newFactory.Load<GlbGroup>(group.PK);
				var staff4 = newFactory.New<GlbStaff>();
				loadedGroup.Staff.Add(staff4);

				var loadedSection = newFactory.Load<BMBoardSection>(section.PK);
				var viewModel = BMSTestHelper.CreateViewModel(loadedSection);

				AssertEquals("Constructing a board section viewmodel should create additional channels for the new resource in the section's release group.", 4, GetChannels(loadedSection).Count);
			}

			public void TestOverrideChannels_ShouldAddNewChannels()
			{
				var group = Factory.NewWithValidTestData<GlbGroup>();
				var staff1 = Factory.NewWithValidTestData<GlbStaff>();
				var staff2 = Factory.NewWithValidTestData<GlbStaff>();
				var staff3 = Factory.NewWithValidTestData<GlbStaff>();
				group.Staff.AddRange(new[] { staff1, staff2, staff3 });

				var system = Factory.NewWithValidTestData<BMSystem>();
				var releaseGroup = system.ReleaseGroups.AddNew();
				releaseGroup.FSG_GG_Group = group.PK;

				var section = system.Boards.AddNew().Sections.AddNew();
				section.SectionConfiguration.ReleaseGroupPK = group.PK;
				GetChannelByInfo(section).Value = (ZString)ChannelTypeList.Codes.Resource;
				AssertEquals(3, GetChannels(section).Count);
				GetOverrideChannelsInfo(section).Value = ZBool.True;
				GetChannelByInfo(section).Value = ZString.Empty;

				Factory.Save();

				var newFactory = new BusinessObjectFactory();

				var loadedGroup = newFactory.Load<GlbGroup>(group.PK);
				var staff4 = newFactory.New<GlbStaff>();
				loadedGroup.Staff.Add(staff4);

				var loadedSection = newFactory.Load<BMBoardSection>(section.PK);
				AssertEquals(3, GetChannels(loadedSection).Count);
			}

			protected abstract BMBoardSectionChannelCollection GetChannels(BMBoardSection section);
			protected abstract ZPropertyInfo GetChannelByInfo(BMBoardSection section);
			protected abstract ZPropertyInfo GetOverrideChannelsInfo(BMBoardSection section);
			protected abstract ZPropertyInfo GetShowUnchanneledInfo(BMBoardSection section);
		}

		public void TestRemoveResourceChannelIfResourceIsInactiveAndHasNoTasksAssigned_OnSectionLoad()
		{
			var group = Factory.New<GlbGroup>();
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			var staff3 = Factory.NewWithValidTestData<GlbStaff>();
			var staff4 = Factory.NewWithValidTestData<GlbStaff>();
			group.Staff.AddRange(new[] { staff1, staff2, staff3, staff4 });

			staff4.GS_IsActive = false;

			var system = Factory.New<BMSystem>();
			system.FS_Name = "Tim";
			var releaseGroup = system.ReleaseGroups.AddNew();
			releaseGroup.FSG_GG_Group = group.PK;

			var section = system.Boards.AddNew().Sections.AddNew();
			var sectionConfiguration = section.SectionConfiguration;
			sectionConfiguration.ReleaseGroupPK = group.PK;

			sectionConfiguration.ChannelBy = ChannelTypeList.Codes.Resource;
			AssertEquals(3, sectionConfiguration.PrimaryAxisChannels.Count);
			Assert(sectionConfiguration.PrimaryAxisChannels.Cast<BMBoardSectionChannel>().Any(c => c.MSC_ParentID == staff1.PK));
			Assert(sectionConfiguration.PrimaryAxisChannels.Cast<BMBoardSectionChannel>().Any(c => c.MSC_ParentID == staff2.PK));
			Assert(sectionConfiguration.PrimaryAxisChannels.Cast<BMBoardSectionChannel>().Any(c => c.MSC_ParentID == staff3.PK));

			sectionConfiguration.OverrideChannels = true;
			staff1.GS_IsActive = false;
			Factory.Save();

			DefaultChannelsProvider.RefreshChannels(sectionConfiguration, section.Factory);

			AssertEquals(2, sectionConfiguration.PrimaryAxisChannels.Count);
			Assert(sectionConfiguration.PrimaryAxisChannels.Cast<BMBoardSectionChannel>().Any(c => c.MSC_ParentID == staff2.PK));
			Assert(sectionConfiguration.PrimaryAxisChannels.Cast<BMBoardSectionChannel>().Any(c => c.MSC_ParentID == staff3.PK));

			var task = Factory.NewWithValidTestData<ProcessTask>();
			task.P9_GS_NKAssignedStaffMember = staff2.GS_Code;
			staff2.GS_IsActive = false;
			Factory.Save();

			DefaultChannelsProvider.RefreshChannels(sectionConfiguration, section.Factory);

			AssertEquals(2, sectionConfiguration.PrimaryAxisChannels.Count);
			Assert(sectionConfiguration.PrimaryAxisChannels.Cast<BMBoardSectionChannel>().Any(c => c.MSC_ParentID == staff2.PK));
			Assert(sectionConfiguration.PrimaryAxisChannels.Cast<BMBoardSectionChannel>().Any(c => c.MSC_ParentID == staff3.PK));
		}

		#region Implementation

		class PrimaryChannelsTest : BoardSectionChannelsTest
		{
			protected override BMBoardSectionChannelCollection GetChannels(BMBoardSection section)
			{
				return section.SectionConfiguration.PrimaryAxisChannels;
			}

			protected override ZPropertyInfo GetChannelByInfo(BMBoardSection section)
			{
				return section.SectionConfiguration.ChannelByInfo;
			}

			protected override ZPropertyInfo GetOverrideChannelsInfo(BMBoardSection section)
			{
				return section.SectionConfiguration.OverrideChannelsInfo;
			}

			protected override ZPropertyInfo GetShowUnchanneledInfo(BMBoardSection section)
			{
				return section.SectionConfiguration.ShowUnchanneledInfo;
			}
		}

		class SecondaryChannelsTest : BoardSectionChannelsTest
		{
			protected override BMBoardSectionChannelCollection GetChannels(BMBoardSection section)
			{
				return section.SectionConfiguration.SecondaryAxisChannels;
			}

			protected override ZPropertyInfo GetChannelByInfo(BMBoardSection section)
			{
				return section.SectionConfiguration.ChannelSecondaryByInfo;
			}

			protected override ZPropertyInfo GetOverrideChannelsInfo(BMBoardSection section)
			{
				return section.SectionConfiguration.OverrideSecondaryChannelsInfo;
			}

			protected override ZPropertyInfo GetShowUnchanneledInfo(BMBoardSection section)
			{
				return section.SectionConfiguration.ShowSecondaryUnchanneledInfo;
			}
		}

		#endregion

		#endregion

		#region Properties

		public void TestAdditionalComponent_ShouldNotThrowNullReferenceException()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "TBS");
			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();

			var relationship = BMSTestHelper.CreateComponentRelationship(Factory, name: "relationship");

			var buffer = BMSTestHelper.CreateBuffer(system, "buffer");
			var relationshipLink = BMSTestHelper.CreateComponentRelationshipLink(Factory, relationship, buffer);

			BMSTestHelper.CreateAdditionalComponent(section, relationship);

			relationshipLink.FL_FC_ComponentTo = ZGuid.NewZGuid();

			AssertNoExceptionThrown(() => section.AdditionalActiveComponents.ToArray());
		}

		public void TestChangeTypeChangesZones_DifferentComponentTypes()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var bucket = BMSTestHelper.CreateBucket(system);
			var buffer = BMSTestHelper.CreateBuffer(system);

			var section = Factory.New<BMBoard>().Sections.AddNew();

			section.MS_FC_Component = bucket.PK;
			AssertEquals(false, section.SectionConfiguration.ShowZones);

			section.MS_FC_Component = buffer.PK;
			AssertEquals(true, section.SectionConfiguration.ShowZones);
		}

		public void TestChangeTypeChangesZones_SameComponentTypes()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var bucket1 = BMSTestHelper.CreateBucket(system);
			var bucket2 = BMSTestHelper.CreateBucket(system, "bucket2");
			var buffer1 = BMSTestHelper.CreateBuffer(system);
			var buffer2 = BMSTestHelper.CreateBuffer(system, "buffer2");

			var section = Factory.New<BMBoard>().Sections.AddNew();

			section.MS_FC_Component = bucket1.PK;
			AssertEquals(false, section.SectionConfiguration.ShowZones);

			section.MS_FC_Component = bucket2.PK;
			AssertEquals(false, section.SectionConfiguration.ShowZones);

			section.MS_FC_Component = buffer1.PK;
			AssertEquals(true, section.SectionConfiguration.ShowZones);

			section.MS_FC_Component = buffer2.PK;
			AssertEquals(true, section.SectionConfiguration.ShowZones);

			section.SectionConfiguration.ShowZones = false;

			section.MS_FC_Component = buffer1.PK;
			AssertEquals(false, section.SectionConfiguration.ShowZones);
		}

		[TestDate(2013, 6, 17)]
		public void TestGetTimeFieldValue()
		{
			var jobHeader = ProcessJobHeader.GetForParent(Factory.New<DummyWithWorkflow>(), Factory);
			var processHeader = jobHeader.ProcessHeaders.AddNew();
			processHeader.FH_AgreedDeliveryDate = ZDateTime.Today.AddDays(1);
			processHeader.JobHeader.FH_ReleaseDateTime = ZDateTime.Today.AddDays(-1);
			processHeader.FH_DoNotStartBeforeDate = ZDateTime.Today.AddDays(-2);

			var sectionConfiguration = Factory.New<BMBoardSection>().SectionConfiguration;
			sectionConfiguration.TimeField = TimeProgressionFieldList.Codes.AgreedDeliveryDate;
			AssertEquals(ZDateTime.Today.AddDays(1), sectionConfiguration.GetTimeFieldValue(processHeader));

			sectionConfiguration.TimeField = TimeProgressionFieldList.Codes.TransferTime;
			AssertEquals(ZDateTime.Today.AddDays(-1), sectionConfiguration.GetTimeFieldValue(processHeader));

			sectionConfiguration.TimeField = TimeProgressionFieldList.Codes.DoNotStartBeforeDate;
			AssertEquals(ZDateTime.Today.AddDays(-2), sectionConfiguration.GetTimeFieldValue(processHeader));

			sectionConfiguration.TimeField = "boo";
			AssertEquals(ZDateTime.Empty, sectionConfiguration.GetTimeFieldValue(processHeader));

			AssertExceptionThrown<ArgumentNullException>(() => sectionConfiguration.GetTimeFieldValue(null));
		}

		[TestDate(2014, 1, 02)]
		public void TestOnSavingOfSectionLastEditDateIsSet()
		{
			var testSystem = BMSTestHelper.CreateSystem(Factory, "TBS");
			var board = testSystem.Boards.AddNew();
			var section = board.Sections.AddNew();
			section.SectionConfiguration.TimeField = "hi";

			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(1);

			section.SectionConfiguration.TimeField = "boo";
			Factory.Save();
			AssertEquals(ZDateTime.UtcNow, section.Board.MB_SystemLastEditTimeUtc);
		}

		public void TestOrientationDefaultToVerticalOnInvalidFlowDirection()
		{
			var section = Factory.New<BMBoardSection>();
			section.SectionConfiguration.FlowDirection = "DIR";
			AssertEquals(BMBoardSectionOrientation.Vertical, section.SectionConfiguration.OrientationValue);
		}

		public void TestTimeProgressionModeAndTimeField_DefaultValue()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			var component = system.Components.AddNew();
			component.FC_Name = "Buffer";
			var section = system.Boards.AddNew().Sections.AddNew();
			var sectionConfiguration = section.SectionConfiguration;
			section.MS_FC_Component = component.PK;

			AssertEquals(TimeProgressionModeList.Codes.Age, sectionConfiguration.TimeProgressionMode);
			AssertEquals(TimeProgressionFieldList.Codes.TransferTime, sectionConfiguration.TimeField);

			Factory.Save();

			Db.Connection.ExecuteScalar( // I need to make the XML column empty to simulate data created by older versions
				string.Format(@"
UPDATE dbo.BMBoardSection
SET MS_LayoutData = '',
MS_SystemLastEditTimeUTC = getutcdate(),
MS_SystemLastEditUser = '~BP'
WHERE MS_PK = '{0}'
", section.PK));

			var loadedSection = new BusinessObjectFactory().Load<BMBoardSection>(section.PK);
			var loadedSectionConfiguration = loadedSection.SectionConfiguration;
			AssertEquals(TimeProgressionModeList.Codes.Age, loadedSectionConfiguration.TimeProgressionMode);
			AssertEquals(TimeProgressionFieldList.Codes.TransferTime, loadedSectionConfiguration.TimeField);
		}

		public void TestBackForeColors_ShouldBeEmptyForBuffer()
		{
			var system = Factory.New<BMSystem>();
			var bucket = system.Components.AddNew();
			bucket.FC_Type = BMComponentTypeList.Codes.Bucket;
			var buffer = system.Components.AddNew();
			buffer.FC_Type = BMComponentTypeList.Codes.Buffer;
			var section = system.Boards.AddNew().Sections.AddNew();
			section.ForegroundColor = section.BackgroundColor = "Blue";

			section.MS_FC_Component = bucket.PK;

			AssertEquals("Blue", section.ForegroundColor);
			AssertEquals("Blue", section.BackgroundColor);

			section.MS_FC_Component = buffer.PK;

			AssertEquals("Blue", section.ForegroundColor);
			AssertEquals("Blue", section.BackgroundColor);
		}

		public void TestTimePerCell_Buffer()
		{
			var system = Factory.New<BMSystem>();
			var component = system.Components.AddNew();
			component.FC_Type = BMComponentTypeList.Codes.Buffer;
			component.FC_BufferTimespanInMinutes = 96 * 60; // 96 hours = 12 days

			var section = system.Boards.AddNew().Sections.AddNew();
			var sectionConfiguration = section.SectionConfiguration;
			section.MS_FC_Component = component.PK;
			sectionConfiguration.Subsections = 1;
			sectionConfiguration.CellsPerSubsection = 13;

			AssertEquals(true, sectionConfiguration.TimePerCellInfo.ReadOnly);
			AssertEquals(new ZInt(60 * 8).GetDateTimeFromMinutes(), sectionConfiguration.TimePerCell);
		}

		public void TestTimePerCell_Bucket()
		{
			var system = Factory.New<BMSystem>();
			var component = system.Components.AddNew();
			component.FC_Type = BMComponentTypeList.Codes.Bucket;

			var section = system.Boards.AddNew().Sections.AddNew();
			var sectionConfiguration = section.SectionConfiguration;
			section.MS_FC_Component = component.PK;

			AssertEquals(false, sectionConfiguration.TimePerCellInfo.ReadOnly);
			AssertEquals(ZDateTime.Empty, sectionConfiguration.TimePerCell);
		}

		public void TestChangeFlowDirection_ShouldClearLastCellIfNeeded()
		{
			var section = Factory.New<BMBoardSection>();
			var sectionConfiguration = section.SectionConfiguration;
			sectionConfiguration.FlowDirection = FlowDirectionList.Codes.Right;
			sectionConfiguration.LastCell = LastCellList.Codes.Bottom;

			sectionConfiguration.FlowDirection = FlowDirectionList.Codes.Down;
			AssertEquals(ZString.Empty, sectionConfiguration.LastCell);

			sectionConfiguration.LastCell = LastCellList.Codes.Right;
			sectionConfiguration.FlowDirection = FlowDirectionList.Codes.Up;
			AssertEquals(LastCellList.Codes.Right, sectionConfiguration.LastCell);

			sectionConfiguration.FlowDirection = FlowDirectionList.Codes.Left;
			AssertEquals(ZString.Empty, sectionConfiguration.LastCell);
		}

		public void TestBackgroundColorValue_ShouldReturnDefault()
		{
			var boardSection = Factory.NewWithValidTestData<BMBoardSection>();
			boardSection.BackgroundColor = null;

			AssertEquals(Color.Beige, boardSection.SectionConfiguration.Section.BackgroundColorValue);
		}

		public void TestBackgroundColorValue_ShouldReturnBlue()
		{
			var boardSection = Factory.NewWithValidTestData<BMBoardSection>();
			boardSection.BackgroundColor = "Blue";

			AssertEquals(Color.Blue, boardSection.SectionConfiguration.Section.BackgroundColorValue);
		}

		public void TestDefaultValuesFromXmlNotContainingFields()
		{
			var section = Factory.NewWithValidTestData<BMSystem>().Boards.AddNew().Sections.AddNew();

			Db.Connection.ExecuteScalar( // I need to make the XML column empty to simulate data created by older versions
				string.Format(@"
UPDATE dbo.BMBoardSection
SET MS_LayoutData = ''
WHERE MS_PK = '{0}'
", section.PK));

			Factory.Save();

			var loadedSection = new BusinessObjectFactory().Load<BMBoardSection>(section.PK);
			var loadedSectionConfiguration = loadedSection.SectionConfiguration;

			AssertColorEquals(Color.Red, loadedSectionConfiguration.CountdownTargetBorderColorValue.Value);
			AssertEquals(new SizedButtonBorderStyle(VisualBoardButtonBorderStyle.Solid, true), loadedSectionConfiguration.CountdownTargetBorderStyleValue);
			AssertColorEquals(Color.Blue, loadedSectionConfiguration.CountdownStartableBorderColorValue.Value);
			AssertEquals(new SizedButtonBorderStyle(VisualBoardButtonBorderStyle.Solid, true), loadedSectionConfiguration.CountdownStartableBorderStyleValue);
		}

		public void TestDisplaySequence()
		{
			var board = Factory.New<BMBoard>();
			var section1 = board.Sections.AddNew();
			var section2 = board.Sections.AddNew();
			var section3 = board.Sections.AddNew();
			var section4 = board.Sections.AddNew();
			var section5 = board.Sections.AddNew();

			section1.Row = 1;
			section1.Column = 1;
			section2.Row = 0;
			section2.Column = 10;
			section3.Row = 0;
			section3.Column = 2;
			section4.Row = 1;
			section4.Column = 2;
			section5.Row = 0;
			section5.Column = 0;

			AssertEquals(1, section5.DisplaySequence);
			AssertEquals(2, section3.DisplaySequence);
			AssertEquals(3, section2.DisplaySequence);
			AssertEquals(4, section1.DisplaySequence);
			AssertEquals(5, section4.DisplaySequence);

			section5.Row = 10;

			AssertEquals(1, section3.DisplaySequence);
			AssertEquals(2, section2.DisplaySequence);
			AssertEquals(3, section1.DisplaySequence);
			AssertEquals(4, section4.DisplaySequence);
			AssertEquals(5, section5.DisplaySequence);
		}

		public void TestReleaseGroupPK()
		{
			var system = Factory.New<BMSystem>();
			var group = Factory.New<GlbGroup>();
			var component = system.Components.AddNew();
			var section = system.Boards.AddNew().Sections.AddNew();
			var sectionConfiguration = section.SectionConfiguration;

			section.MS_FC_Component = component.PK;
			AssertEquals(false, sectionConfiguration.ShowWorkInReleaseGroupOnly);

			sectionConfiguration.ReleaseGroupPK = group.PK;
			AssertEquals(true, sectionConfiguration.ShowWorkInReleaseGroupOnly);

			sectionConfiguration.ReleaseGroupPK = ZGuid.Empty;
			AssertEquals(false, sectionConfiguration.ShowWorkInReleaseGroupOnly);

			component.FC_Type = BMComponentTypeList.Codes.Buffer;
			sectionConfiguration.ReleaseGroupPK = group.PK;
			AssertEquals(false, sectionConfiguration.ShowWorkInReleaseGroupOnly);
		}

		public void TestFilterProperties_ShouldReturnConfigurationFilters()
		{
			var section = Factory.New<BMBoardSection>();
			var config = section.SectionConfiguration;

			AssertEquals(section.WorkflowFilter, config.WorkflowFilter);
			AssertEquals(section.TaskFilter, config.TaskFilter);
		}

		public void TestHumanReadableName()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			AssertEquals("Visual Board: Buffer Board, buffer", config.BufferSection.HumanReadableName);
		}

		#endregion

		#region LayoutData

		public void TestChangeLayoutConfig_ShouldFireLayoutUpdated()
		{
			var system = Factory.New<BMSystem>();
			var releaseGroup = system.ReleaseGroups.AddNew();
			var group = Factory.New<GlbGroup>();
			releaseGroup.FSG_GG_Group = group.PK;
			var section = system.Boards.AddNew().Sections.AddNew();
			var sectionConfiguration = section.SectionConfiguration;
			var layoutConfigUpdatedFireCount = 0;
			section.LayoutConfigUpdated += (s, e) => layoutConfigUpdatedFireCount++;

			var expectedLayoutFiredCount = 0;

			section.Row = 100;
			AssertEquals(++expectedLayoutFiredCount, layoutConfigUpdatedFireCount);
			section.Column = 100;
			AssertEquals(++expectedLayoutFiredCount, layoutConfigUpdatedFireCount);
			section.RowSpan = 2;
			AssertEquals(++expectedLayoutFiredCount, layoutConfigUpdatedFireCount);
			section.ColSpan = 2;
			AssertEquals(++expectedLayoutFiredCount, layoutConfigUpdatedFireCount);
			section.RowHeightPercent = 10;
			AssertEquals(++expectedLayoutFiredCount, layoutConfigUpdatedFireCount);
			section.ColWidthPercent = 10;
			AssertEquals(++expectedLayoutFiredCount, layoutConfigUpdatedFireCount);

			section.BackgroundColor = Color.Blue.Name;
			AssertEquals(++expectedLayoutFiredCount, layoutConfigUpdatedFireCount);
			section.ForegroundColor = Color.White.Name;
			AssertEquals(++expectedLayoutFiredCount, layoutConfigUpdatedFireCount);
			sectionConfiguration.Subsections = 2;
			AssertEquals(++expectedLayoutFiredCount, layoutConfigUpdatedFireCount);
			sectionConfiguration.FlowDirection = FlowDirectionList.Codes.Right;
			AssertEquals(++expectedLayoutFiredCount, layoutConfigUpdatedFireCount);
			sectionConfiguration.LastCell = LastCellList.Codes.Top;
			AssertEquals(++expectedLayoutFiredCount, layoutConfigUpdatedFireCount);
			sectionConfiguration.CellsPerSubsection = 2;
			AssertEquals(++expectedLayoutFiredCount, layoutConfigUpdatedFireCount);
			sectionConfiguration.BufferZone3Color = "Orange";
			AssertEquals(++expectedLayoutFiredCount, layoutConfigUpdatedFireCount);
			sectionConfiguration.BufferZone2Color = "Orange";
			AssertEquals(++expectedLayoutFiredCount, layoutConfigUpdatedFireCount);
			sectionConfiguration.BufferZone1Color = "Orange";
			AssertEquals(++expectedLayoutFiredCount, layoutConfigUpdatedFireCount);
			sectionConfiguration.BufferZone0Color = "Orange";
			AssertEquals(++expectedLayoutFiredCount, layoutConfigUpdatedFireCount);
			sectionConfiguration.TimePerCell = new ZInt(10).GetDateTimeFromMinutes();
			AssertEquals(++expectedLayoutFiredCount, layoutConfigUpdatedFireCount);
			sectionConfiguration.CountdownTargetBorderColor = Color.Aqua.Name;
			AssertEquals(expectedLayoutFiredCount, layoutConfigUpdatedFireCount);
			sectionConfiguration.CountdownStartableBorderColor = Color.Bisque.Name;
			AssertEquals(expectedLayoutFiredCount, layoutConfigUpdatedFireCount);
			sectionConfiguration.CountdownTargetBorderStyle = "None";
			AssertEquals(expectedLayoutFiredCount, layoutConfigUpdatedFireCount);
			sectionConfiguration.CountdownStartableBorderStyle = "Inset - Small";
			AssertEquals(expectedLayoutFiredCount, layoutConfigUpdatedFireCount);

			sectionConfiguration.ReleaseGroupPK = group.PK;
			AssertEquals(expectedLayoutFiredCount, layoutConfigUpdatedFireCount);

			// Re-setting same values should not fire event again

			section.Row = 100;
			AssertEquals(expectedLayoutFiredCount, layoutConfigUpdatedFireCount);
			section.Column = 100;
			AssertEquals(expectedLayoutFiredCount, layoutConfigUpdatedFireCount);
			section.RowSpan = 2;
			AssertEquals(expectedLayoutFiredCount, layoutConfigUpdatedFireCount);
			section.ColSpan = 2;
			AssertEquals(expectedLayoutFiredCount, layoutConfigUpdatedFireCount);
			section.RowHeightPercent = 10;
			AssertEquals(expectedLayoutFiredCount, layoutConfigUpdatedFireCount);
			section.ColWidthPercent = 10;
			AssertEquals(expectedLayoutFiredCount, layoutConfigUpdatedFireCount);

			section.BackgroundColor = Color.Blue.Name;
			AssertEquals(expectedLayoutFiredCount, layoutConfigUpdatedFireCount);
			section.ForegroundColor = Color.White.Name;
			AssertEquals(expectedLayoutFiredCount, layoutConfigUpdatedFireCount);
			sectionConfiguration.Subsections = 2;
			AssertEquals(expectedLayoutFiredCount, layoutConfigUpdatedFireCount);
			sectionConfiguration.FlowDirection = FlowDirectionList.Codes.Right;
			AssertEquals(expectedLayoutFiredCount, layoutConfigUpdatedFireCount);
			sectionConfiguration.LastCell = LastCellList.Codes.Top;
			AssertEquals(expectedLayoutFiredCount, layoutConfigUpdatedFireCount);
			sectionConfiguration.CellsPerSubsection = 2;
			AssertEquals(expectedLayoutFiredCount, layoutConfigUpdatedFireCount);
			sectionConfiguration.BufferZone3Color = "Orange";
			AssertEquals(expectedLayoutFiredCount, layoutConfigUpdatedFireCount);
			sectionConfiguration.BufferZone2Color = "Orange";
			AssertEquals(expectedLayoutFiredCount, layoutConfigUpdatedFireCount);
			sectionConfiguration.BufferZone1Color = "Orange";
			AssertEquals(expectedLayoutFiredCount, layoutConfigUpdatedFireCount);
			sectionConfiguration.BufferZone0Color = "Orange";
			AssertEquals(expectedLayoutFiredCount, layoutConfigUpdatedFireCount);
			sectionConfiguration.ReleaseGroupPK = group.PK;
			AssertEquals(expectedLayoutFiredCount, layoutConfigUpdatedFireCount);
			sectionConfiguration.TimePerCell = new ZInt(10).GetDateTimeFromMinutes();
			AssertEquals(expectedLayoutFiredCount, layoutConfigUpdatedFireCount);
		}

		public void TestChangeLayoutOnSectionConfigurationCallsSectionFireLayout()
		{
			var system = Factory.New<BMSystem>();
			var releaseGroup = system.ReleaseGroups.AddNew();
			var group = Factory.New<GlbGroup>();
			releaseGroup.FSG_GG_Group = group.PK;
			var section = system.Boards.AddNew().Sections.AddNew();
			var sectionConfiguration = section.SectionConfiguration;
			var layoutConfigUpdatedFireCount = 0;
			section.LayoutConfigUpdated += (s, e) => layoutConfigUpdatedFireCount++;

			var expectedLayoutFiredCount = 0;

			sectionConfiguration.Subsections = 2;
			AssertEquals(++expectedLayoutFiredCount, layoutConfigUpdatedFireCount);
			sectionConfiguration.Subsections = 10;
			AssertEquals(++expectedLayoutFiredCount, layoutConfigUpdatedFireCount);
		}

		public void TestSerialiseLayoutParameters()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			var releaseGroup = system.ReleaseGroups.AddNew();
			releaseGroup.FSG_GG_Group = Factory.NewWithValidTestData<GlbGroup>().PK;
			var component = Factory.NewWithValidTestData<BMComponent>();
			system.Components.Add(component);
			component.FC_Type = BMComponentTypeList.Codes.Bucket;
			var board = Factory.NewWithValidTestData<BMBoard>();
			system.Boards.Add(board);
			var section = Factory.NewWithValidTestData<BMBoardSection>();
			var sectionConfiguration = section.SectionConfiguration;
			board.Sections.Add(section);
			section.MS_FC_Component = component.PK;
			section.Row = 1;
			section.Column = 2;
			section.RowSpan = 3;
			section.ColSpan = 4;
			section.RowHeightPercent = 100;
			section.ColWidthPercent = 100;
			section.BackgroundColor = "Blue";
			section.ForegroundColor = "White";
			sectionConfiguration.Subsections = 2;
			sectionConfiguration.FlowDirection = FlowDirectionList.Codes.Right;
			sectionConfiguration.LastCell = LastCellList.Codes.Bottom;
			sectionConfiguration.CellsPerSubsection = 3;
			sectionConfiguration.TimePerCell = new ZInt(60).GetDateTimeFromMinutes();
			sectionConfiguration.ShowWorkInReleaseGroupOnly = true;
			sectionConfiguration.ReleaseGroupPK = releaseGroup.FSG_GG_Group;
			sectionConfiguration.ChannelBy = ChannelTypeList.Codes.Resource;
			sectionConfiguration.ShowUnchanneled = true;
			sectionConfiguration.OverrideChannels = true;
			sectionConfiguration.CardType = CardTypeList.Codes.Workflow;
			sectionConfiguration.ChannelSecondaryBy = BMConstants.ChannelByTimeCode;
			sectionConfiguration.TimeField = TimeProgressionFieldList.Codes.TransferTime;
			sectionConfiguration.ShowZones = false;
			sectionConfiguration.BufferZone3Color = "Black";
			sectionConfiguration.BufferZone2Color = "Orange";
			sectionConfiguration.BufferZone1Color = "Purple";
			sectionConfiguration.BufferZone0Color = "White";
			sectionConfiguration.CountdownTargetBorderColor = Color.Aqua.Name;
			sectionConfiguration.CountdownStartableBorderColor = Color.Bisque.Name;
			sectionConfiguration.CountdownTargetBorderStyle = "None";
			sectionConfiguration.CountdownStartableBorderStyle = "Inset - Small";

			sectionConfiguration.Validation.ValidateAll();
			section.Validation.ValidateAll();
			AssertNoErrors(section);
			AssertNoErrors(sectionConfiguration);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			section = newFactory.Load<BMBoardSection>(section.PK);
			sectionConfiguration = section.SectionConfiguration;

			AssertEquals(1, section.Row);
			AssertEquals(2, section.Column);
			AssertEquals(3, section.RowSpan);
			AssertEquals(4, section.ColSpan);
			AssertEquals(100, section.RowHeightPercent);
			AssertEquals(100, section.ColWidthPercent);
			AssertEquals("Blue", section.BackgroundColor);
			AssertEquals("White", section.ForegroundColor);
			AssertEquals("Horizontal", sectionConfiguration.Orientation);

			AssertEquals(2, sectionConfiguration.Subsections);
			AssertEquals(FlowDirectionList.Codes.Right, sectionConfiguration.FlowDirection);
			AssertEquals(LastCellList.Codes.Bottom, sectionConfiguration.LastCell);
			AssertEquals(3, sectionConfiguration.CellsPerSubsection);
			AssertEquals(new ZInt(60).GetDateTimeFromMinutes(), sectionConfiguration.TimePerCell);
			AssertEquals(true, sectionConfiguration.ShowWorkInReleaseGroupOnly);
			AssertEquals(releaseGroup.FSG_GG_Group, sectionConfiguration.ReleaseGroupPK);
			AssertEquals(ZString.Empty, sectionConfiguration.ChannelBy);
			AssertEquals(true, sectionConfiguration.ShowUnchanneled);
			AssertEquals(true, sectionConfiguration.ShowWorkflowOrJobWorkflowCards);
			AssertEquals(BMConstants.ChannelByTimeCode, sectionConfiguration.ChannelSecondaryBy);
			AssertEquals(false, sectionConfiguration.ShowSecondaryUnchanneled);
			AssertEquals(TimeProgressionFieldList.Codes.TransferTime, sectionConfiguration.TimeField);
			AssertEquals(false, sectionConfiguration.ShowZones);
			AssertEquals(Color.Black, sectionConfiguration.BufferZone3ColorValue);
			AssertEquals(Color.Orange, sectionConfiguration.BufferZone2ColorValue);
			AssertEquals(Color.Purple, sectionConfiguration.BufferZone1ColorValue);
			AssertEquals(Color.White, sectionConfiguration.BufferZone0ColorValue);
			AssertEquals(Color.Aqua, sectionConfiguration.CountdownTargetBorderColorValue);
			AssertEquals(Color.Bisque, sectionConfiguration.CountdownStartableBorderColorValue);
			AssertEquals(new SizedButtonBorderStyle(VisualBoardButtonBorderStyle.None, null), sectionConfiguration.CountdownTargetBorderStyleValue);
			AssertEquals(new SizedButtonBorderStyle(VisualBoardButtonBorderStyle.Inset, false), sectionConfiguration.CountdownStartableBorderStyleValue);

			AssertEquals("loaded xml configuration should not fire changes", false, sectionConfiguration.HasChanges);
			AssertEquals(false, section.HasChanges);

			section.RowSpan = 10;
			newFactory.Save();

			newFactory = new BusinessObjectFactory();
			section = newFactory.Load<BMBoardSection>(section.PK);

			AssertEquals("Changes to Layout properties should be captured when they are the only change", 10, section.RowSpan);
		}

		#endregion

		#region Customised Cards

		public void TestCustomisedCards_WorkflowCardSection()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			var section = system.Boards.AddNew().Sections.AddNew();
			section.SectionConfiguration.CardType = CardTypeList.Codes.Workflow;

			var workflowDetailedCard = Factory.NewWithValidTestData<BMControlCustomisation>();
			workflowDetailedCard.FM_ControlType = CustomisedControlTypeList.Codes.WorkflowDetailedCard;
			var workflowSummaryCard = Factory.NewWithValidTestData<BMControlCustomisation>();
			workflowSummaryCard.FM_ControlType = CustomisedControlTypeList.Codes.WorkflowSummaryCard;

			BMSTestHelper.CreateControlCustomisationLink(Factory, section, workflowDetailedCard);
			BMSTestHelper.CreateControlCustomisationLink(Factory, section, workflowSummaryCard);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedSection = newFactory.Load<BMBoardSection>(section.PK);

			var loadedWorkflowDetailedCard = BMControlCustomisation.GetCustomisedDetailedCard(loadedSection.SectionConfiguration, string.Empty);
			var loadedWorkflowSummaryCard = BMControlCustomisation.GetCustomisedSummaryCard(loadedSection.SectionConfiguration, string.Empty);

			AssertNotNull(loadedWorkflowDetailedCard);
			AssertNotNull(loadedWorkflowSummaryCard);

			AssertEquals(workflowDetailedCard.PK, loadedWorkflowDetailedCard.PK);
			AssertEquals(workflowSummaryCard.PK, loadedWorkflowSummaryCard.PK);
		}

		public void TestCustomisedCards_TaskCardSection()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			var section = system.Boards.AddNew().Sections.AddNew();

			var taskDetailedCard = Factory.NewWithValidTestData<BMControlCustomisation>();
			taskDetailedCard.FM_ControlType = CustomisedControlTypeList.Codes.DetailedCard;
			var taskSummaryCard = Factory.NewWithValidTestData<BMControlCustomisation>();
			taskSummaryCard.FM_ControlType = CustomisedControlTypeList.Codes.TaskCard;

			BMSTestHelper.CreateControlCustomisationLink(Factory, section, taskDetailedCard);
			BMSTestHelper.CreateControlCustomisationLink(Factory, section, taskSummaryCard);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedSection = newFactory.Load<BMBoardSection>(section.PK);

			var loadedTaskDetailedCard = BMControlCustomisation.GetCustomisedDetailedCard(loadedSection.SectionConfiguration, string.Empty);
			var loadedTaskSummaryCard = BMControlCustomisation.GetCustomisedSummaryCard(loadedSection.SectionConfiguration, string.Empty);

			AssertNotNull(loadedTaskDetailedCard);
			AssertNotNull(loadedTaskSummaryCard);

			AssertEquals(taskDetailedCard.PK, loadedTaskDetailedCard.PK);
			AssertEquals(taskSummaryCard.PK, loadedTaskSummaryCard.PK);
		}

		public void TestDetailedCard()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			var section = system.Boards.AddNew().Sections.AddNew();
			var customisedCard = Factory.NewWithValidTestData<BMControlCustomisation>();
			customisedCard.FM_ControlType = CustomisedControlTypeList.Codes.DetailedCard;

			BMSTestHelper.CreateControlCustomisationLink(Factory, section, customisedCard);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedDetailedCard = BMControlCustomisation.GetCustomisedDetailedCard(newFactory.Load<BMBoardSection>(section.PK).SectionConfiguration, string.Empty);
			AssertNotNull(loadedDetailedCard);
			AssertEquals(customisedCard.PK, loadedDetailedCard.PK);
		}

		public void TestCustomisedLayoutCollection()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var customisation = Factory.NewWithValidTestData<BMControlCustomisation>();
			var link = config.BufferSection.SectionConfiguration.CustomisedLayoutLinks.AddNew();

			link.FML_FM_ControlCustomisation = customisation.PK;

			AssertNoExceptionThrown(Factory.Save);
		}

		#endregion

		#region SectionDescriptor

		public void TestGetSectionDescriptorForDefaultSection_ShouldBeValid()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();
			var descriptor = SectionDescriptorProvider.Get(section.MS_SectionType);

			AssertNotNull(descriptor);
			AssertEquals(BMConstants.ComponentSectionType, descriptor.Type);
			AssertEquals(BMConstants.ComponentSectionType, section.MS_SectionType);
		}

		#endregion

		#region Filter

		public void TestBMBoardSectionFilter()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();
			var buffer = BMSTestHelper.CreateBuffer(system);
			section.MS_FC_Component = buffer.PK;

			var filter = section.WorkflowFilter;
			filter.S9_SaveColumnLayout = true;

			Factory.Save();

			var loadedSection = Factory.Load<BMBoardSection>(section.PK);

			AssertEquals(filter.PK, loadedSection.WorkflowFilter.PK);
		}

		#endregion

		#region Delete

		public void TestDeleteSectionWithNullConfiguration()
		{
			var section = Factory.New<BMSystem>().Boards.AddNew().Sections.AddNew();
			section.MS_SectionType = "XXX";

			AssertNoExceptionThrown(() => section.Delete());
		}

		public void TestDelete_ShouldDeleteLayoutLinks()
		{
			var section = (BMBoardSection)GetNewBusinessObject();
			var layout = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.DetailedCard);
			var link = BMSTestHelper.CreateControlCustomisationLink(Factory, section, layout);

			Factory.Save();

			var loadedSection = Factory.CreateNewFactory().Load<BMBoardSection>(section.PK);
			loadedSection.Delete();
			loadedSection.Factory.Save();

			AssertEquals(true, link.IsDeleted);
		}

		#endregion

		#region DBHits

		public void TestAllComponents_DBHits()
		{
			const int numRelationships = 6;
			const int numBuffersPerRelationship = 6;
			var section = MakeSectionWithRelationships(numRelationships, numBuffersPerRelationship);
			var newFactory = Factory.CreateNewFactory();

			Factory.Save();

			var expectedHitCounts = new Dictionary<string, int>
			{
				{ BMBoardSectionAdditionalComponentSchema.Constants.TableName, 1 },
				{ BMComponentSchema.Constants.TableName, 2 }, // 2 for newfactory
				{ BMComponentLinkSchema.Constants.TableName, 1 },
			};

			var loadedSection = newFactory.Load<BMBoardSection>(section.PK);

			using (AssertDbHitsForAllFactories(expectedHitCounts, ignoreUnspecified: true))
			{
				var allComponents = loadedSection.AllComponents;

				AssertEquals("Failed to load all components from section", numRelationships * numBuffersPerRelationship + 1, allComponents.Count());
			}
		}

		#endregion

		#region Branch/Department

		public void TestIBranchDepartmentProviderMembers_BufferSection()
		{
			var section = Factory.New<BMBoardSection>();
			AssertNull("If the section's board and component are null, a null branch should be returned. SAD!", BMSTestHelper.GetBranch(section, Factory));
			AssertNull("If the section's board and component are null, a null department should be returned. SAD!", BMSTestHelper.GetDepartment(section, Factory));

			var board = Factory.New<BMBoard>();
			section.MS_MB_Board = board.PK;
			AssertNull("If the section's board has no branch, and its component is null, a null branch should be returned. SAD!", BMSTestHelper.GetBranch(section, Factory));
			AssertNull("If the section's board has no department, and its component is null, a null department should be returned. SAD!", BMSTestHelper.GetDepartment(section, Factory));

			var component = Factory.New<BMComponent>();
			component.FC_Type = BMComponentTypeList.Codes.Buffer;
			component.FC_GB_AgingBranch = ZGuid.Empty; // Validation and db constraints will prevent this, but testing here just in case.
			component.FC_GE_AgingDepartment = ZGuid.Empty;
			section.MS_FC_Component = component.PK;

			AssertNull("If the section's component also has no branch, a null branch should be returned. SAD!", BMSTestHelper.GetBranch(section, Factory));
			AssertNull("If the section's component also has no department, a null department should be returned. SAD!", BMSTestHelper.GetDepartment(section, Factory));

			var branch1 = Factory.New<GlbBranch>();
			var branch2 = Factory.New<GlbBranch>();
			var department1 = Factory.New<GlbDepartment>();
			var department2 = Factory.New<GlbDepartment>();

			board.MB_GB_AgingBranch = branch1.PK;
			board.MB_GE_AgingDepartment = department1.PK;

			AssertEquals("The board's branch should be used as the section's branch. SAD!", branch1, BMSTestHelper.GetBranch(section, Factory));
			AssertEquals("The board's branch should be used as the section's branch. SAD!", department1, BMSTestHelper.GetDepartment(section, Factory));

			component.FC_GB_AgingBranch = branch2.PK;
			component.FC_GE_AgingDepartment = department2.PK;

			AssertEquals("The board's branch should be chosen in preference to the component's branch. SAD!", branch1, BMSTestHelper.GetBranch(section, Factory));
			AssertEquals("The board's department should be chosen in preference to the component's department. SAD!", department1, BMSTestHelper.GetDepartment(section, Factory));

			board.MB_GB_AgingBranch = ZGuid.Empty;
			board.MB_GE_AgingDepartment = ZGuid.Empty;

			AssertEquals("The component's branch should be chosen because the board didn't have one. SAD!", branch2, BMSTestHelper.GetBranch(section, Factory));
			AssertEquals("The component's department should be chosen because the board didn't have one. SAD!", department2, BMSTestHelper.GetDepartment(section, Factory));

			ErrorReporter.Clear();
		}

		public void TestIBranchDepartmentProviderMembers_BucketSection()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var board = config.BucketBoard;
			config.Bucket.FC_GB_AgingBranch = ZGuid.Empty;
			config.Bucket.FC_GE_AgingDepartment = ZGuid.Empty;
			AssertEquals(ZGuid.Empty, board.MB_GB_AgingBranch);
			AssertEquals(ZGuid.Empty, board.MB_GE_AgingDepartment);

			AssertEquals("If the bucket section's component also has no branch, the current branch should be returned.", Env.CurrentBranch.Code, ((IBranchDepartmentProvider)config.BucketSection).GetBranch(Factory)?.GB_Code);
			AssertEquals("If the bucket section's component also has no department, the current department should be returned.", Env.CurrentDepartment.Code, ((IBranchDepartmentProvider)config.BucketSection).GetDepartment(Factory)?.GE_Code);

			var branch1 = Factory.New<GlbBranch>();
			var branch2 = Factory.New<GlbBranch>();
			var department1 = Factory.New<GlbDepartment>();
			var department2 = Factory.New<GlbDepartment>();

			config.Bucket.FC_GB_AgingBranch = branch1.PK;
			config.Bucket.FC_GE_AgingDepartment = department1.PK;

			AssertEquals("If the bucket section's component has a branch specified, that should be used.", branch1.GB_Code, ((IBranchDepartmentProvider)config.BucketSection).GetBranch(Factory)?.GB_Code);
			AssertEquals("If the bucket section's component has a department specified, that should be used.", department1.GE_Code, ((IBranchDepartmentProvider)config.BucketSection).GetDepartment(Factory)?.GE_Code);

			board.MB_GB_AgingBranch = branch2.PK;
			board.MB_GE_AgingDepartment = department2.PK;

			AssertEquals("If the board has a branch specified, that should be used.", branch2.GB_Code, ((IBranchDepartmentProvider)config.BucketSection).GetBranch(Factory)?.GB_Code);
			AssertEquals("If the board has a department specified, that should be used.", department2.GE_Code, ((IBranchDepartmentProvider)config.BucketSection).GetDepartment(Factory)?.GE_Code);
		}

		#endregion

		#region Implementation

		BMBoardSection MakeSectionWithRelationships(int numRelationships, int numBuffersPerRelationship)
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var primaryBuffer = BMSTestHelper.CreateBuffer(system);
			var section = BMSTestHelper.CreateBoardSection(primaryBuffer);

			for (int i = 0; i < numRelationships; i++)
			{
				var relationship = BMSTestHelper.CreateComponentRelationship(Factory, name: "relationship " + i);

				for (int j = 0; j < numBuffersPerRelationship; j++)
				{
					var buffer = BMSTestHelper.CreateBuffer(system, name: "buffer " + i + j);
					BMSTestHelper.CreateComponentRelationshipLink(Factory, relationship, buffer);
				}

				BMSTestHelper.CreateAdditionalComponent(section, relationship);
			}

			return section;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			// This section should have no component selected to ensure validation etc can cope with it.

			var system = Factory.New<BMSystem>();
			system.FS_Name = "BLAH";
			var board = system.Boards.AddNew();
			return board.Sections.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var result = GetNewBusinessObject();
			return result;
		}

		protected override IEnumerable<string> XmlMemberNames
		{
			get
			{
				yield return "Configuration";
				yield return "RowSpan";
				yield return "ColSpan";
				yield return "RowHeightPercent";
				yield return "ColWidthPercent";
				yield return "BackgroundColor";
				yield return "ForegroundColor";
				yield return "Row";
				yield return "Column";
			}
		}

		#endregion
	}
}
