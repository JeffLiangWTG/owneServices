using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI.Test
{
	class ChannelRowControlTest : BMSTestCaseWithFactory
	{
		public void TestDragDrop()
		{
			var resource1 = Factory.NewWithValidTestData<GlbStaff>();
			var resource2 = Factory.NewWithValidTestData<GlbStaff>();
			var resource3 = Factory.NewWithValidTestData<GlbStaff>();
			resource1.GS_FullName = "Frodo Baggins";
			resource2.GS_FullName = "Bilbo Baggins";
			resource3.GS_FullName = "Drogo Baggins";

			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.Staff.AddRange(resource1, resource2, resource3);

			var system = CreateSystem();
			var bucket = CreateBucket(system);
			var section = CreateBoardSection(bucket);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;
			section.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.Resource;
			section.SectionConfiguration.SortPrimaryChannels = false;

			Factory.Save();

			using (var form = new ZForm())
			using (var control = new ChannelAssignmentControl())
			{
				form.Controls.Add(control);
				form.Show();

				control.ViewModel = section.SectionConfiguration.PrimaryChannelsViewModel;

				AssertChannelRows("Initial channel positions", control, "Frodo Baggins", "Bilbo Baggins", "Drogo Baggins");

				var channelRows = GetChannelRowControls(control);
				channelRows[1].Top = 0;
				channelRows[1].OnFinishDragging();

				AssertChannelRows("Dragging second channel to first position", control, "Bilbo Baggins", "Frodo Baggins", "Drogo Baggins");

				channelRows = GetChannelRowControls(control);
				channelRows[1].Top = channelRows[2].Top + 1;
				channelRows[1].OnFinishDragging();

				AssertChannelRows("Dragging second channel to last position", control, "Bilbo Baggins", "Drogo Baggins", "Frodo Baggins");

				channelRows = GetChannelRowControls(control);
				channelRows[1].Top--;
				channelRows[1].OnFinishDragging();

				AssertChannelRows("Dragging second channel, but it hasn't moved to another slot", control, "Bilbo Baggins", "Drogo Baggins", "Frodo Baggins");

				channelRows = GetChannelRowControls(control);
				channelRows[0].Top = channelRows[1].Top + 1;
				channelRows[0].OnFinishDragging();

				AssertChannelRows("Dragging first channel to second position", control, "Drogo Baggins", "Bilbo Baggins", "Frodo Baggins");
			}
		}

		public void TestDragDrop_SetsSortChannelsToFalse()
		{
			var resource1 = Factory.NewWithValidTestData<GlbStaff>();
			var resource2 = Factory.NewWithValidTestData<GlbStaff>();
			var resource3 = Factory.NewWithValidTestData<GlbStaff>();
			resource1.GS_FullName = "Frodo Baggins";
			resource2.GS_FullName = "Bilbo Baggins";
			resource3.GS_FullName = "Drogo Baggins";

			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.Staff.AddRange(resource1, resource2, resource3);

			var system = CreateSystem();
			var bucket = CreateBucket(system);
			var section = CreateBoardSection(bucket);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;
			section.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.Resource;

			Factory.Save();

			using (var form = new ZForm())
			using (var control = new ChannelAssignmentControl())
			{
				form.Controls.Add(control);
				form.Show();

				control.ViewModel = section.SectionConfiguration.PrimaryChannelsViewModel;
				control.ViewModel.SortChannels = true;

				AssertChannelRows("Sorted original position", control, "Bilbo Baggins", "Drogo Baggins", "Frodo Baggins");

				var channelRows = GetChannelRowControls(control);
				channelRows[1].Top = 0;
				channelRows[1].OnFinishDragging();

				AssertChannelRows("Dragging second channel to first position", control, "Drogo Baggins", "Bilbo Baggins", "Frodo Baggins");
				Assert(!control.ViewModel.SortChannels);
			}
		}

		static void AssertChannelRows(string message, ChannelAssignmentControl control, params string[] channelDescriptions)
		{
			CombineAssertions(message, () =>
			{
				var channelRows = GetChannelRowControls(control);
				AssertEquals("Number of channels", channelDescriptions.Length, channelRows.Length);

				for (int i = 0; i < channelRows.Length; i++)
				{
					var expectedChannelDescription = channelDescriptions[i];
					var channelDescription = channelRows[i].Channel_ForTesting.BizoDescription;

					AssertEquals("Channel description", expectedChannelDescription, channelDescription);
					AssertEquals("Channel sequence", i + 1, channelRows[i].Channel_ForTesting.MSC_Sequence);
				}
			});
		}

		static ChannelRowControl[] GetChannelRowControls(ChannelAssignmentControl control)
		{
			return control.FindAll<ChannelRowControl>().OrderBy(c => c.Top).ToArray();
		}

		public void TestDelete_WhenDefaultChannels_ShouldNotDelete()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var resource = Factory.NewWithValidTestData<GlbStaff>();

			config.ReleaseGroup.Staff.Add(resource);

			var section = config.BucketSection;
			section.SectionConfiguration.ReleaseGroupPK = config.ReleaseGroup.PK;
			section.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.Resource;

			var channel = section.SectionConfiguration.PrimaryAxisChannels.Single();

			var viewModel = new BMBoardSectionChannelsViewModel(section.SectionConfiguration, ChannelAxis.Primary);

			using (var form = new ZForm())
			using (var control = new ChannelRowControl(viewModel, channel, ChannelAxis.Primary))
			{
				form.Controls.Add(control);
				form.Show();

				control.DeleteButton.PerformClick();
				AssertEquals(false, channel.IsDeleted);

				section.SectionConfiguration.OverrideChannels = true;

				control.DeleteButton.PerformClick();
				AssertEquals(true, channel.IsDeleted);
			}
		}

		public void TestCurrentUserChannel()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			var bucket1 = system.Components.AddNew();
			bucket1.FC_Name = "bucket1";

			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();

			var channel = BMSTestHelper.CreatePrimaryChannelForSection(section);
			section.SectionConfiguration.OverrideChannels = true;

			channel.MSC_ChannelType = ChannelTypeList.Codes.CurrentUser;

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var reloadedChannel = newFactory.Load<BMBoardSectionChannel>(channel.PK);
			var reloadedSection = newFactory.Load<BMBoardSection>(section.PK);

			var viewModel = new BMBoardSectionChannelsViewModel(reloadedSection.SectionConfiguration, ChannelAxis.Primary);

			using (var form = new ZForm())
			using (var control = new ChannelRowControl(viewModel, reloadedChannel, ChannelAxis.Primary))
			{
				form.Controls.Add(control);
				form.Show();

				AssertEquals(false, control.ChannelEntityFindBox.IsVisibleForBinding);
				AssertEquals("Current User", control.DescriptionLabel.Text);
			}
		}
	}
}
