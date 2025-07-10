using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business.Test;

namespace Enterprise.BufferManagement.Business.Test
{
	public class ChannelFactoryTest : BMSTestCaseWithFactory
	{
		public void TestShouldCreateNonConstrainedResourcesChannel()
		{
			Section.SectionConfiguration.IsReleaseScheduler = true;
			Staff.DesignateAsCCR(Buffer);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(Section);
			var primaryChannels = ChannelFactory.CreatePrimaryChannels(viewModel, Section).ToArray();
			var secondaryChannels = ChannelFactory.CreateSecondaryChannels(viewModel, Section).ToArray();

			AssertHasOneChannelOfType("NonConstrainedResourcesChannel", primaryChannels);
			AssertSequencesEqual(new[] { "ReleaseSchedulerReleasedChannel", "ReleaseSchedulerUnReleasedChannel" }, secondaryChannels.Select(x => x.GetType().Name));

			var releaseSchedulerSection = VisualBoardsTestHelper.CreateReleaseSchedulerBoardSection(Buffer, Group);
			viewModel = BMSTestHelper.CreateViewModel(releaseSchedulerSection);
			primaryChannels = ChannelFactory.CreatePrimaryChannels(viewModel, releaseSchedulerSection).ToArray();

			AssertSequencesEqual(new[] { "ResourceChannel", "NonConstrainedResourcesChannel" }, primaryChannels.Select(x => x.GetType().Name));
		}

		public void TestShouldCreateStaffChannel()
		{
			BMSTestHelper.CreatePrimaryChannelForSection(Section, ChannelTypeList.Codes.Resource, Staff.PK);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(Section);
			var channels = ChannelFactory.CreatePrimaryChannels(viewModel, Section).ToArray();

			var test = channels.First();
			AssertHasOneChannelOfType("ResourceChannel", channels);
		}

		public void TestShouldCreateGroupChannel()
		{
			BMSTestHelper.CreatePrimaryChannelForSection(Section, ChannelTypeList.Codes.Group, Group.PK);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(Section);
			var channels = ChannelFactory.CreatePrimaryChannels(viewModel, Section).ToArray();

			AssertHasOneChannelOfType("GroupChannel", channels);
		}

		public void TestShouldCreateCapabilityChannel()
		{
			var capability = CreateCapability("Cap", "capability");
			BMSTestHelper.CreatePrimaryChannelForSection(Section, ChannelTypeList.Codes.Capability, capability.PK);

			var viewModel = BMSTestHelper.CreateViewModel(Section);
			var channels = ChannelFactory.CreatePrimaryChannels(viewModel, Section).ToArray();

			AssertHasOneChannelOfType("CapabilityChannel", channels);
		}

		public void TestShouldCreateWorkQueueChannel()
		{
			var workQueue = BMSTestHelper.CreateWorkQueue(Factory, "Que", "WorkQueue");
			BMSTestHelper.CreatePrimaryChannelForSection(Section, ChannelTypeList.Codes.Tag, workQueue.PK);

			Factory.Save();

			var viewModel = VisualBoardsTestHelper.CreateViewModel(Section);
			var channels = ChannelFactory.CreatePrimaryChannels(viewModel, Section).ToArray();

			AssertHasOneChannelOfType("WorkQueueChannel", channels);
		}

		public void TestShouldCreateTagMagnitudeChannel()
		{
			var definition = BMSTestHelper.CreateTagDefinition(Factory, "AAA");
			var tag = BMSTestHelper.CreateTagMagnitude(definition, "Tag", "TagMagnitude");
			BMSTestHelper.CreatePrimaryChannelForSection(Section, ChannelTypeList.Codes.Tag, tag.PK);

			Factory.Save();

			var viewModel = VisualBoardsTestHelper.CreateViewModel(Section);
			var channels = ChannelFactory.CreatePrimaryChannels(viewModel, Section).ToArray();

			AssertHasOneChannelOfType("TagMagnitudeChannel", channels);
		}

		public void TestShouldCreateUnchanneledChannel()
		{
			var unchanneldChannel = BMSTestHelper.CreatePrimaryChannelForSection(Section, ChannelTypeList.Codes.NotChanneled, ZGuid.Empty);
			unchanneldChannel.IsUnChanneled = true;
			Factory.Save();

			var viewModel = VisualBoardsTestHelper.CreateViewModel(Section);
			var channels = ChannelFactory.CreatePrimaryChannels(viewModel, Section).ToArray();

			AssertHasOneChannelOfType("UnchanneledChannel", channels);
		}

		#region Implementation

		BMSystem System { get; set; }
		BMComponent Buffer { get; set; }
		BMBoardSection Section { get; set; }
		GlbGroup Group { get; set; }
		GlbStaff Staff { get; set; }

		protected override void SetUp()
		{
			base.SetUp();
			System = CreateSystem();
			Buffer = CreateBuffer(System, "Buffer");
			Section = CreateBoardSection(Buffer);
			Group = Factory.NewWithValidTestData<GlbGroup>();
			Staff = Factory.NewWithValidTestData<GlbStaff>();
			Group.Staff.Add(Staff);
		}

		void AssertHasOneChannelOfType(string channelType, IEnumerable<IVisualBoardChannel> channels)
		{
			AssertContainsExactElementsInAnyOrder(new[] { channelType }, channels.Select(x => x.GetType().Name));
		}

		#endregion
	}
}
