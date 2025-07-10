using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(BMBoardSectionChannel))]
	class BMBoardSectionChannelTest : EnterpriseBusinessObjectTestCase
	{
		public void TestUseDefaultChannels_ShouldReturnTrue_WhenNoBMComponentSectionConfiguration()
		{
			var sectionChannel = Factory.New<BMBoardSectionChannel>();

			AssertNull(sectionChannel.Section);
			Assert(sectionChannel.UseDefaultChannels);

			var section = Factory.New<BMBoardSection>();
			section.MS_SectionType = BMConstants.ModuleGridSectionType;
			sectionChannel.MSC_MS_Section = section.PK;

			AssertNotNull(sectionChannel.Section);
			AssertNull(section.SectionConfiguration);
			Assert(sectionChannel.UseDefaultChannels);

			section.MS_SectionType = BMConstants.ComponentSectionType;

			AssertNotNull(sectionChannel.Section);
			AssertNotNull(section.SectionConfiguration);
			Assert(sectionChannel.UseDefaultChannels);

			section.SectionConfiguration.OverrideChannels = true;

			AssertEquals(false, sectionChannel.UseDefaultChannels);

			section.SectionConfiguration.OverrideChannels = false;
			section.SectionConfiguration.OverrideSecondaryChannels = true;
			sectionChannel.MSC_Axis = ChannelAxisCodeList.Codes.Secondary;

			AssertEquals(false, sectionChannel.UseDefaultChannels);
		}

		[TestDate(2013, 4, 16)]
		public void TestCopyReleaseSchedulerChannels()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, GlbDepartment.CurrentDepartment.PK);

			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);
			var releaseSchedulerSection = VisualBoardsTestHelper.CreateReleaseSchedulerBoardSection(config.Buffer, config.ReleaseGroup);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(releaseSchedulerSection);

			AssertType<NonConstrainedResourcesChannel>(viewModel.PrimaryChannels.OfType<NonConstrainedResourcesChannel>().Single().CreateCopy(Factory));
			AssertType<ReleaseSchedulerReleasedChannel>(viewModel.SecondaryAxisChannels.OfType<ReleaseSchedulerReleasedChannel>().Single().CreateCopy(Factory));
			AssertType<ReleaseSchedulerUnReleasedChannel>(viewModel.SecondaryAxisChannels.OfType<ReleaseSchedulerUnReleasedChannel>().Single().CreateCopy(Factory));
		}

		public void TestChannel_ForUnChanneledChannel()
		{
			var section = Factory.New<BMSystem>().Boards.AddNew().Sections.AddNew();
			var channel = section.SectionConfiguration.PrimaryAxisChannels.AddNew();

			var sectionViewModel = BMSTestHelper.CreateViewModel(section);

			AssertNull(sectionViewModel.GetOrCreateChannelForTest(null, section.Factory));

			channel.IsUnChanneled = true;

			channel.MSC_ChannelType = ChannelTypeList.Codes.Resource;
			AssertType<UnchanneledChannel>(sectionViewModel.GetOrCreateChannelForTest(channel, section.Factory));
			AssertEquals(ChannelTypeList.Codes.Resource, sectionViewModel.GetOrCreateChannelForTest(channel, section.Factory).EntityType);

			channel.MSC_ChannelType = ChannelTypeList.Codes.Group;
			AssertType<UnchanneledChannel>(sectionViewModel.GetOrCreateChannelForTest(channel, section.Factory));
			AssertEquals(ChannelTypeList.Codes.Group, sectionViewModel.GetOrCreateChannelForTest(channel, section.Factory).EntityType);

			channel.MSC_ChannelType = ChannelTypeList.Codes.Capability;
			AssertType<UnchanneledChannel>(sectionViewModel.GetOrCreateChannelForTest(channel, section.Factory));
			AssertEquals(ChannelTypeList.Codes.Capability, sectionViewModel.GetOrCreateChannelForTest(channel, section.Factory).EntityType);

			channel.MSC_ChannelType = ChannelTypeList.Codes.Tag;
			AssertType<UnchanneledChannel>(sectionViewModel.GetOrCreateChannelForTest(channel, section.Factory));
			AssertEquals(ChannelTypeList.Codes.Tag, sectionViewModel.GetOrCreateChannelForTest(channel, section.Factory).EntityType);
		}

		public void TestChannelKeys_ReadOnly()
		{
			var section = Factory.New<BMSystem>().Boards.AddNew().Sections.AddNew();
			var channel = BMSTestHelper.CreatePrimaryChannelForSection(section);
			section.SectionConfiguration.OverrideChannels = true;

			AssertEquals(false, channel.MSC_ParentIDInfo.ReadOnly);

			channel.IsUnChanneled = true;
			AssertEquals(true, channel.MSC_ParentIDInfo.ReadOnly);

			channel.IsUnChanneled = false;
			AssertEquals(false, channel.MSC_ParentIDInfo.ReadOnly);
		}

		public void TestChannelType()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			var tag1 = Factory.NewWithValidTestData<TagMagnitude>();

			var system = Factory.NewWithValidTestData<BMSystem>();
			var bucket1 = system.Components.AddNew();
			bucket1.FC_Name = "bucket1";

			var board = system.Boards.AddNew();
			var section1 = board.Sections.AddNew();
			section1.MS_FC_Component = bucket1.PK;

			var viewModel = BMSTestHelper.CreateViewModel(section1);

			var channel = section1.SectionConfiguration.PrimaryAxisChannels.AddNew();
			AssertEquals(ZString.Empty, channel.MSC_ChannelType);
			AssertNull(viewModel.GetOrCreateChannelForTest(channel, section1.Factory));

			channel.MSC_ChannelType = ChannelTypeList.Codes.Resource;
			channel.MSC_ParentID = staff1.PK;
			AssertEquals(staff1, BMSTestHelper.GetChannelEntity<GlbStaff>(viewModel.GetOrCreateChannelForTest(channel, section1.Factory), Factory));

			channel.MSC_ChannelType = ChannelTypeList.Codes.Group;
			channel.MSC_ParentID = group1.PK;
			AssertEquals(group1, BMSTestHelper.GetChannelEntity<GlbGroup>(viewModel.GetOrCreateChannelForTest(channel, section1.Factory), Factory));

			channel.MSC_ChannelType = ChannelTypeList.Codes.Capability;
			channel.MSC_ParentID = capability1.PK;
			AssertEquals(capability1, BMSTestHelper.GetChannelEntity<GlbCapability>(viewModel.GetOrCreateChannelForTest(channel, section1.Factory), Factory));

			channel.MSC_ChannelType = ChannelTypeList.Codes.Tag;
			channel.MSC_ParentID = tag1.PK;
			AssertEquals(tag1, BMSTestHelper.GetChannelEntity<TagMagnitude>(viewModel.GetOrCreateChannelForTest(channel, section1.Factory), Factory));
		}

		public void TestTagMagnitude_ShouldErrorForInactiveTags()
		{
			var config = TestConfigsHelper.CreateTagsTestConfig(Factory);
			config.GoldTag.TGM_IsActive = false;

			Factory.Save();

			var sectionAndView = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 13, FlowDirectionList.Codes.Up, LastCellList.Codes.Right, string.Empty);
			var section = sectionAndView.Item1;
			var viewModel = sectionAndView.Item2;
			section.SectionConfiguration.OverrideChannels = true;

			var channel = section.SectionConfiguration.PrimaryAxisChannels.AddNew();
			AssertEquals(ZString.Empty, channel.MSC_ChannelType);
			AssertNull(viewModel.GetOrCreateChannelForTest(channel, section.Factory));

			channel.MSC_ChannelType = ChannelTypeList.Codes.Tag;
			channel.MSC_ParentID = config.RedTag.PK;
			AssertNoErrors("Setting active-tag should not add inactive error", channel);

			channel.MSC_ParentID = config.GoldTag.PK;
			AssertHasError("Setting inactive-tag should add inactive error", channel.MSC_ParentIDInfo, "This Entity is inactive - it may not be used.");
		}

		public void TestChannelsForJobWorkflow()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			var staff3 = Factory.NewWithValidTestData<GlbStaff>();

			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.Staff.Add(staff1);
			group1.Staff.Add(staff2);
			group1.Staff.Add(staff3);

			var system = Factory.NewWithValidTestData<BMSystem>();
			var buffer = BMSTestHelper.CreateBuffer(system);

			var board = system.Boards.AddNew();
			var section1 = board.Sections.AddNew();
			section1.MS_FC_Component = buffer.PK;
			section1.SectionConfiguration.CardType = CardTypeList.Codes.JobLevelWorkflow;

			var viewModel = BMSTestHelper.CreateViewModel(section1);

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "Test1";
			var header = ProcessJobHeader.GetForParent(org, Factory);

			var workflow1 = BMSTestHelper.CreateProcessHeader(header, buffer, "Workflow 1", ZDateTime.Now.AddHours(-1));
			BMSTestHelper.CreateTasks(org, workflow1, Tuple.Create(ProcessTaskStatusCodeList.Codes.Assigned, 4, staff1.GS_Code.ToString(), ZGuid.Empty),
													Tuple.Create(ProcessTaskStatusCodeList.Codes.Open, 3, "", ZGuid.Empty),
													Tuple.Create(ProcessTaskStatusCodeList.Codes.Closed, 2, "", ZGuid.Empty),
													Tuple.Create(ProcessTaskStatusCodeList.Codes.Cancelled, 1, "", ZGuid.Empty));

			var workflow2 = BMSTestHelper.CreateProcessHeader(header, buffer, "Workflow 2", ZDateTime.Now.AddHours(-2));
			BMSTestHelper.CreateTasks(org, workflow2, Tuple.Create(ProcessTaskStatusCodeList.Codes.Assigned, 4, staff2.GS_Code.ToString(), ZGuid.Empty),
												Tuple.Create(ProcessTaskStatusCodeList.Codes.Open, 3, "", ZGuid.Empty),
												Tuple.Create(ProcessTaskStatusCodeList.Codes.Closed, 2, "", ZGuid.Empty),
												Tuple.Create(ProcessTaskStatusCodeList.Codes.Cancelled, 1, "", ZGuid.Empty));

			var workflow3 = BMSTestHelper.CreateProcessHeader(header, buffer, "Workflow 3", ZDateTime.Now.AddHours(-1));
			BMSTestHelper.CreateTasks(org, workflow3, Tuple.Create(ProcessTaskStatusCodeList.Codes.Assigned, 4, staff3.GS_Code.ToString(), ZGuid.Empty),
												Tuple.Create(ProcessTaskStatusCodeList.Codes.Open, 3, "", ZGuid.Empty),
												Tuple.Create(ProcessTaskStatusCodeList.Codes.Closed, 2, "", ZGuid.Empty),
												Tuple.Create(ProcessTaskStatusCodeList.Codes.Cancelled, 1, "", ZGuid.Empty));

			var channel = BMSTestHelper.CreatePrimaryChannelForSection(section1, ChannelTypeList.Codes.Resource, staff1.PK);
			AssertEquals(staff1, BMSTestHelper.GetChannelEntity<GlbStaff>(viewModel.GetOrCreateChannelForTest(channel, section1.Factory), Factory));
			Factory.Save();

			var dataSource = BMSTestHelper.GetDataSource(section1);
			var tasks = dataSource.GetIncompleteTasksForCurrentChannels().ToArray();

			AssertEquals(1, tasks.Length);
			AssertEquals(tasks.FirstOrDefault().P9_NotesAsString, "Workflow 1 Task 4");

			channel.MSC_ChannelType = ChannelTypeList.Codes.Group;
			channel.MSC_ParentID = group1.PK;
			AssertEquals(group1, BMSTestHelper.GetChannelEntity<GlbGroup>(viewModel.GetOrCreateChannelForTest(channel, section1.Factory), Factory));

			dataSource = BMSTestHelper.GetDataSource(section1);
			tasks = dataSource.GetIncompleteTasksForCurrentChannels().ToArray();

			AssertEquals(3, tasks.Length);

			AssertCollectionContains(tasks, t => t.P9_NotesAsString == "Workflow 1 Task 4");
			AssertCollectionContains(tasks, t => t.P9_NotesAsString == "Workflow 2 Task 4");
			AssertCollectionContains(tasks, t => t.P9_NotesAsString == "Workflow 3 Task 4");

			channel.MSC_ChannelType = ChannelTypeList.Codes.Capability;
			AssertEquals(ZGuid.Empty, channel.MSC_ParentID);

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			staff1.Capabilities.Add(capability1);

			channel.MSC_ParentID = capability1.PK;
			AssertEquals(capability1, BMSTestHelper.GetChannelEntity<GlbCapability>(viewModel.GetOrCreateChannelForTest(channel, section1.Factory), Factory));

			var capabilityOnlyTask1 = BMSTestHelper.CreateTask(workflow1, capability: capability1, description: "capabilityOnlyTask1");
			Factory.Save();

			dataSource = BMSTestHelper.GetDataSource(section1);
			tasks = dataSource.GetIncompleteTasksForCurrentChannels().ToArray();

			AssertEquals(1, tasks.Length);
			AssertEquals(tasks.FirstOrDefault().P9_Description, "capabilityOnlyTask1");

			channel.MSC_ChannelType = ChannelTypeList.Codes.Tag;
			AssertEquals(ZGuid.Empty, channel.MSC_ParentID);

			var tag1 = Factory.NewWithValidTestData<TagMagnitude>();

			header.AddTag(tag1, false);
			workflow1.AddTag(tag1, false);
			tasks.FirstOrDefault().AddTag(tag1, false);
			Factory.Save();

			channel.MSC_ParentID = tag1.PK;
			AssertEquals(tag1, BMSTestHelper.GetChannelEntity<TagMagnitude>(viewModel.GetOrCreateChannelForTest(channel, section1.Factory), Factory));

			dataSource = BMSTestHelper.GetDataSource(section1);
			tasks = dataSource.GetIncompleteTasksForCurrentChannels().ToArray();

			AssertEquals(7, tasks.Length);
		}

		public void TestBizoDescription()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "AAA";
			staff.GS_FullName = "Aaron A. Aaronson";

			var system = Factory.NewWithValidTestData<BMSystem>();
			var bucket1 = system.Components.AddNew();
			bucket1.FC_Name = "bucket1";

			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();

			var channel = BMSTestHelper.CreatePrimaryChannelForSection(section);
			section.SectionConfiguration.OverrideChannels = true;

			channel.IsUnChanneled = true;
			AssertEquals(BMConstants.UnchanneledDisplayName, channel.BizoDescription);

			channel.IsUnChanneled = false;
			channel.MSC_ChannelType = ChannelTypeList.Codes.CurrentUser;
			AssertEquals(ChannelTypeList.Descriptions.CurrentUser, channel.BizoDescription);

			channel.MSC_ChannelType = ChannelTypeList.Codes.Resource;
			channel.MSC_ParentID = staff.PK;

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var reloadedChannel = newFactory.Load<BMBoardSectionChannel>(channel.PK);

			AssertEquals("Aaron A. Aaronson", reloadedChannel.BizoDescription);
		}

		public void TestGetCurrentUserChannel()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "AAA";
			staff.GS_FullName = "Aaron A. Aaronson";

			Factory.Save();

			var system = Factory.NewWithValidTestData<BMSystem>();
			var bucket1 = system.Components.AddNew();
			bucket1.FC_Name = "bucket1";

			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();

			Factory.Save();

			var channel = BMSTestHelper.CreatePrimaryChannelForSection(section);

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				var newChannel = BMBoardSectionChannel.GetCurrentUserChannel(channel);

				AssertEquals(channel.MSC_MS_Section, newChannel.MSC_MS_Section);
				AssertEquals(channel.MSC_Axis, newChannel.MSC_Axis);
				AssertEquals(channel.MSC_Sequence, newChannel.MSC_Sequence);
				AssertEquals(ChannelTypeList.Codes.Resource, newChannel.MSC_ChannelType);
				AssertEquals(staff.PK, newChannel.MSC_ParentID);
			}
		}

		public void TestGetCurrentUserChannel_ThrowsException_WhenCurrentUserPK_IsEmptyGuid()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			var bucket1 = system.Components.AddNew();
			bucket1.FC_Name = "bucket1";

			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();

			Factory.Save();

			var channel = BMSTestHelper.CreatePrimaryChannelForSection(section);

			using (Env.SetTemporaryUserContext(Guid.Empty, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				AssertExceptionThrown<UserNotFoundException>(() => BMBoardSectionChannel.GetCurrentUserChannel(channel));
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<BMSystem>().Boards.AddNew().Sections.AddNew().SectionConfiguration.PrimaryAxisChannels.AddNew();
		}
	}
}
