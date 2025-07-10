using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(BMBoardSectionChannelsViewModel))]
	class BMBoardSectionChannelsViewModelTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDeleteChannel_Primary()
		{
			var section = Factory.New<BMSystem>().Boards.AddNew().Sections.AddNew();
			var channel = BMSTestHelper.CreatePrimaryChannelForSection(section);

			var viewModel = new BMBoardSectionChannelsViewModel(section.SectionConfiguration, ChannelAxis.Primary);
			var changedCount = 0;
			viewModel.ChannelsChanged += (s, e) => { changedCount++; AssertEquals(ChannelAxis.Primary, e.Axis); };

			viewModel.DeleteChannel(channel, ChannelAxis.Primary);
			Assert(channel.IsDeleted);
			AssertEquals(1, changedCount);
		}

		public void TestDeleteChannel_Secondary()
		{
			var section = Factory.New<BMSystem>().Boards.AddNew().Sections.AddNew();
			var channel = BMSTestHelper.CreatePrimaryChannelForSection(section);

			var viewModel = new BMBoardSectionChannelsViewModel(section.SectionConfiguration, ChannelAxis.Primary);
			var changedCount = 0;
			viewModel.ChannelsChanged += (s, e) => { changedCount++; AssertEquals(ChannelAxis.Secondary, e.Axis); };

			viewModel.DeleteChannel(channel, ChannelAxis.Secondary);
			Assert(channel.IsDeleted);
			AssertEquals(1, changedCount);
		}

		public void TestRemoveIrrelevantChannels()
		{
			var system = Factory.New<BMSystem>();
			var group = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup = BMSTestHelper.CreateReleaseGroup(system, group);
			var resource = Factory.NewWithValidTestData<GlbStaff>();
			resource.Groups.Add(group);

			var section = system.Boards.AddNew().Sections.AddNew();
			section.SectionConfiguration.ReleaseGroupPK = group.PK;
			AssertEquals(ChannelTypeList.Codes.NotChanneled, section.SectionConfiguration.ChannelBy);

			section.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.Resource;
			AssertEquals(1, BMBoardSectionTestHelper.GetPrimaryAxisChannelCount(section));
			AssertEquals(resource.PK, section.SectionConfiguration.PrimaryAxisChannels[0].MSC_ParentID);

			section.SectionConfiguration.ShowUnchanneled = true;

			AssertEquals(2, BMBoardSectionTestHelper.GetPrimaryAxisChannelCount(section));
			Assert(section.SectionConfiguration.PrimaryAxisChannels.Cast<BMBoardSectionChannel>().Any(c => !c.IsUnChanneled && c.MSC_ParentID == resource.PK));
			Assert(section.SectionConfiguration.PrimaryAxisChannels.Cast<BMBoardSectionChannel>().Any(c => c.IsUnChanneled && c.MSC_ChannelType == ChannelTypeList.Codes.Resource));

			var viewModel = new BMBoardSectionChannelsViewModel(section.SectionConfiguration, ChannelAxis.Primary);
			var changedCount = 0;
			viewModel.ChannelsChanged += (s, e) => changedCount++;

			viewModel.RemoveIrrelevantChannels();
			AssertEquals(2, changedCount);

			AssertEquals(1, BMBoardSectionTestHelper.GetPrimaryAxisChannelCount(section));
			AssertEquals(resource.PK, section.SectionConfiguration.PrimaryAxisChannels[0].MSC_ParentID);
			AssertEquals(false, section.SectionConfiguration.PrimaryAxisChannels[0].IsUnChanneled);
		}

		public void TestRemoveIrrelevantChannels_ShouldUseEachAxisChannelBy()
		{
			var system = Factory.New<BMSystem>();
			var resource = Factory.NewWithValidTestData<GlbStaff>();
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.Staff.Add(resource);

			var section = system.Boards.AddNew().Sections.AddNew();
			section.SectionConfiguration.ReleaseGroupPK = group.PK;
			section.SectionConfiguration.OverrideChannels = true;
			var manualChannel = BMSTestHelper.CreatePrimaryChannelForSection(section);
			manualChannel.MSC_ParentID = resource.PK;

			section.SectionConfiguration.ChannelSecondaryBy = ChannelTypeList.Codes.Resource;

			AssertEquals(1, BMBoardSectionTestHelper.GetPrimaryAxisChannelCount(section));
			AssertEquals(1, BMBoardSectionTestHelper.GetSecondaryAxisChannelCount(section));

			var viewModel = new BMBoardSectionChannelsViewModel(section.SectionConfiguration, ChannelAxis.Secondary);
			viewModel.RemoveIrrelevantChannels();

			AssertEquals("Should not remove customised primary channel", 1, BMBoardSectionTestHelper.GetPrimaryAxisChannelCount(section));
			AssertEquals("Secondary axis channel is still relevant", 1, BMBoardSectionTestHelper.GetSecondaryAxisChannelCount(section));
		}

		public void TestProperties()
		{
			var section = Factory.New<BMSystem>().Boards.AddNew().Sections.AddNew();
			section.SectionConfiguration.OverrideChannels = true;
			section.SectionConfiguration.ShowUnchanneled = false;
			section.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.Capability;

			section.SectionConfiguration.OverrideSecondaryChannels = false;
			section.SectionConfiguration.ShowSecondaryUnchanneled = true;
			section.SectionConfiguration.ChannelSecondaryBy = BMConstants.ChannelByTimeCode;

			var primaryViewModel = new BMBoardSectionChannelsViewModel(section.SectionConfiguration, ChannelAxis.Primary);
			AssertEquals(ChannelTypeList.Codes.Capability, primaryViewModel.ChannelBy);
			AssertEquals(true, primaryViewModel.OverrideChannels);
			AssertEquals(false, primaryViewModel.ShowUnchanneled);

			var secondaryViewModel = new BMBoardSectionChannelsViewModel(section.SectionConfiguration, ChannelAxis.Secondary);
			AssertEquals(BMConstants.ChannelByTimeCode, secondaryViewModel.ChannelBy);
			AssertEquals(false, secondaryViewModel.OverrideChannels);
			AssertEquals(true, secondaryViewModel.ShowUnchanneled);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new BMBoardSectionChannelsViewModel(Factory.New<BMSystem>().Boards.AddNew().Sections.AddNew().SectionConfiguration, ChannelAxis.Primary);
		}
	}
}
