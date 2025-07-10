using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business;
using Enterprise.VisualBoards.Business.Test;

namespace Enterprise.BufferManagement.Business.Test
{
	class VisualBoardChannelAggregateTest : BMSTestCaseWithFactory
	{
		public void TestMatcherDoesNotPersistFactory()
		{
			var cache = new PropertyCache();
			var factoryName = "MatcherFactory";

			foreach (var channel in Channels.OfType<IVisualBoardChannel>())
			{
				SeedChannel(channel, factoryName);
				var message = string.Format("Expect channel [{0}] with entity type [{1}] not to persist factory", channel.GetChannelName(DisplayNameType.FullName), channel.EntityType);
				CombineAssertions(message, () =>
				{
					AssertFactoryWasGarbageCollected(factoryName);
				});
			}
		}

		static void SeedChannel(IVisualBoardChannel channel, string factoryName)
		{
			// This function is wrapped to prevent the factory from being held in memory.
			channel.SeedCacheWithChannelMatcherTest(new BusinessObjectFactory { NameForDebugging = factoryName });
		}

		public void TestAllChannelTypesAreRepresented()
		{
			foreach (var code in new ChannelTypeList().GetAllCodes())
			{
				AssertEquals("Expected setup to contain a channel of type [{0}] but there wasn't any.", true, Channels.Any(channel => channel.EntityType == code));
			}
		}

		#region Setup

		protected override void SetUp()
		{
			base.SetUp();

			var config = TestConfigsHelper.CreateTagsTestConfig(Factory);

			var workQueue = BMSTestHelper.CreateWorkQueue(Factory, "GOB", "Goblin Queue");
			var capability = CreateCapability("GOB", "Goblins");
			var staff = CreateStaffInCurrentBranchDept("GOB", "The Human Goblin", capability);
			var glbGroup = Factory.NewWithValidTestData<GlbGroup>();

			var board = CreateBoard(config.System);
			var bufferSection = CreateBoardSection(config.Bucket, board);

			Factory.Save();

			ViewModel = VisualBoardsTestHelper.CreateViewModel(bufferSection);

			var tagChannel = ViewModel.CreateChannelForTest(config.RedTag);
			var workQueueChannel = ViewModel.CreateChannelForTest(workQueue);
			var staffChannel = ViewModel.CreateChannelForTest(staff);
			var capabilityChannel = ViewModel.CreateChannelForTest(capability);
			var groupChannel = ViewModel.CreateChannelForTest(glbGroup);
			var releaseSchedulerChannel = new UnchanneledChannel(ChannelTypeList.Codes.ReleaseSchedulerChannels);
			var unchanneledChannel = new UnchanneledChannel(ChannelTypeList.Codes.NotChanneled);
			var unchanneledResource = new UnchanneledChannel(ChannelTypeList.Codes.Resource);
			var currentChannel = new UnchanneledChannel(ChannelTypeList.Codes.CurrentUser);

			Channels = new IVisualBoardChannel[] { tagChannel, workQueueChannel, staffChannel, capabilityChannel, groupChannel, releaseSchedulerChannel, unchanneledChannel, unchanneledResource, currentChannel };
		}

		IVisualBoardChannel[] Channels { get; set; }
		BMBoardSectionViewModel ViewModel { get; set; }

		#endregion
	}
}
