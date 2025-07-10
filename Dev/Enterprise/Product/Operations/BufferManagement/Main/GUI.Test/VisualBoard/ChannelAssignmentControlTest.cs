using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI.Test
{
	class ChannelAssignmentControlTest : BMSTestCaseWithFactory
	{
		public void TestChannelBy_ShouldRemoveRedundantChannels()
		{
			var system = CreateSystem("ORG");

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			var capability2 = Factory.NewWithValidTestData<GlbCapability>();

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup = CreateReleaseGroup(system, group);
			var resource1 = Factory.NewWithValidTestData<GlbStaff>();
			var resource2 = Factory.NewWithValidTestData<GlbStaff>();
			group.Staff.AddRange(resource1, resource2);

			var section = system.Boards.AddNew().Sections.AddNew();
			section.SectionConfiguration.ReleaseGroupPK = group.PK;
			section.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.Capability;
			AssertEquals(0, BMBoardSectionTestHelper.GetPrimaryAxisChannelCount(section));

			using (var control = new ChannelAssignmentControl())
			{
				control.SetDataBinding(section.SectionConfiguration.PrimaryChannelsViewModel, string.Empty);
				control.ViewModel = section.SectionConfiguration.PrimaryChannelsViewModel;

				section.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.Resource;
				AssertEquals(2, BMBoardSectionTestHelper.GetPrimaryAxisChannelCount(section));

				section.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.Capability;
				AssertEquals(0, BMBoardSectionTestHelper.GetPrimaryAxisChannelCount(section));
			}
		}

		public void TestSecondaryChannelBy_ShouldRemoveRedundantChannels()
		{
			var system = CreateSystem("ORG");

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			var capability2 = Factory.NewWithValidTestData<GlbCapability>();

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup = CreateReleaseGroup(system, group);
			var resource1 = Factory.NewWithValidTestData<GlbStaff>();
			var resource2 = Factory.NewWithValidTestData<GlbStaff>();
			group.Staff.AddRange(resource1, resource2);

			var sectionConfiguration = system.Boards.AddNew().Sections.AddNew().SectionConfiguration;
			sectionConfiguration.ReleaseGroupPK = group.PK;
			sectionConfiguration.ChannelSecondaryBy = ChannelTypeList.Codes.Resource;
			AssertEquals(2, sectionConfiguration.SecondaryAxisChannels.Count);

			using (var control = new ChannelAssignmentControl())
			{
				control.SetDataBinding(sectionConfiguration.SecondaryChannelsViewModel, string.Empty);
				control.ViewModel = sectionConfiguration.SecondaryChannelsViewModel;

				sectionConfiguration.ChannelSecondaryBy = ChannelTypeList.Codes.Capability;
				AssertEquals(0, sectionConfiguration.SecondaryAxisChannels.Count);

				sectionConfiguration.ChannelSecondaryBy = ChannelTypeList.Codes.Resource;
				AssertEquals(2, sectionConfiguration.SecondaryAxisChannels.Count);
			}
		}

		public void TestChangeShowUnchanneled_ShouldCreateUnchanneledChannel()
		{
			var sectionConfiguration = Factory.New<BMSystem>().Boards.AddNew().Sections.AddNew().SectionConfiguration;
			using (var control = new ChannelAssignmentControl())
			{
				control.SetDataBinding(sectionConfiguration.PrimaryChannelsViewModel, string.Empty);
				control.ViewModel = sectionConfiguration.PrimaryChannelsViewModel;
				AssertEquals(0, BMBoardSectionTestHelper.GetPrimaryAxisChannelCount(sectionConfiguration));
				sectionConfiguration.ShowUnchanneled = true;
				AssertEquals(1, BMBoardSectionTestHelper.GetPrimaryAxisChannelCount(sectionConfiguration));
				AssertEquals(1, control.ChannelsPanel.Controls.Count);
			}
		}

		public void TestChangeOverrideChannels_ShouldChangeReadOnly()
		{
			var system = CreateSystem("ORG");

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			var capability2 = Factory.NewWithValidTestData<GlbCapability>();
			var capability3 = Factory.NewWithValidTestData<GlbCapability>();

			var sectionConfiguration = Factory.New<BMSystem>().Boards.AddNew().Sections.AddNew().SectionConfiguration;

			using (var zForm = new ZForm())
			{
				using (var control = new ChannelAssignmentControl())
				{
					zForm.Controls.Add(control);
					zForm.Show();
					control.Show();

					control.SetDataBinding(sectionConfiguration.PrimaryChannelsViewModel, string.Empty);
					control.ViewModel = sectionConfiguration.PrimaryChannelsViewModel;
					sectionConfiguration.ChannelBy = ChannelTypeList.Codes.Capability;

					sectionConfiguration.OverrideChannels = false;
					var dropControls = control.Controls.Find("PriorityTagDropEdit", true);
					Assert(dropControls.All(x => x.GetReadOnly()));

					sectionConfiguration.OverrideChannels = true;
					dropControls = control.Controls.Find("PriorityTagDropEdit", true);
					Assert(dropControls.All(x => !x.GetReadOnly()));

					sectionConfiguration.OverrideChannels = false;
					dropControls = control.Controls.Find("PriorityTagDropEdit", true);
					Assert(dropControls.All(x => x.GetReadOnly()));
				}
			}
		}

		public void TestChangeChannelBy_ShouldRecreateChannels()
		{
			var system = CreateSystem("ORG");
			var capability1 = Factory.NewWithValidTestData<GlbCapability>();

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup = CreateReleaseGroup(system, group);
			var resource = Factory.NewWithValidTestData<GlbStaff>();
			resource.Groups.Add(group);

			var sectionConfiguration = system.Boards.AddNew().Sections.AddNew().SectionConfiguration;
			sectionConfiguration.ReleaseGroupPK = group.PK;
			AssertEquals(ChannelTypeList.Codes.NotChanneled, sectionConfiguration.ChannelBy);

			using (var control = new ChannelAssignmentControl())
			{
				control.SetDataBinding(sectionConfiguration.PrimaryChannelsViewModel, string.Empty);
				control.ViewModel = sectionConfiguration.PrimaryChannelsViewModel;
				AssertEquals(0, control.ChannelsPanel.Controls.Count);

				sectionConfiguration.ChannelBy = ChannelTypeList.Codes.Capability;
				AssertEquals(0, control.ChannelsPanel.Controls.Count);

				AssertNull(control.ChannelsPanel.Controls.OfType<ChannelRowControl>().FirstOrDefault());

				sectionConfiguration.ChannelBy = ChannelTypeList.Codes.Resource;
				AssertEquals(1, control.ChannelsPanel.Controls.Count);

				var channelRow = control.ChannelsPanel.Controls.OfType<ChannelRowControl>().First();
				AssertEquals(resource.PK, channelRow.Channel_ForTesting.MSC_ParentID);
			}
		}

		public void TestChangeChannelBy_ShouldUpdateChannels()
		{
			var system = Factory.New<BMSystem>();

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup = CreateReleaseGroup(system, group);
			var resource1 = Factory.NewWithValidTestData<GlbStaff>();
			var resource2 = Factory.NewWithValidTestData<GlbStaff>();
			group.Staff.AddRange(resource1, resource2);

			var sectionConfiguration = Factory.New<BMSystem>().Boards.AddNew().Sections.AddNew().SectionConfiguration;
			sectionConfiguration.ReleaseGroupPK = group.PK;

			using (var form = new ZForm())
			using (var control = new ChannelAssignmentControl())
			{
				control.SetDataBinding(sectionConfiguration.PrimaryChannelsViewModel, string.Empty);
				control.ViewModel = sectionConfiguration.PrimaryChannelsViewModel;
				AssertEquals(0, control.ChannelsPanel.Controls.Count);

				sectionConfiguration.ChannelBy = ChannelTypeList.Codes.Resource;
				AssertEquals(2, control.ChannelsPanel.Controls.Count);
			}
		}

		public void TestAddChannel_WhenNotOverridden_ShouldNotAdd()
		{
			var sectionConfiguration = Factory.New<BMSystem>().Boards.AddNew().Sections.AddNew().SectionConfiguration;

			using (var control = new ChannelAssignmentControl())
			{
				control.SetDataBinding(sectionConfiguration.PrimaryChannelsViewModel, string.Empty);
				control.ViewModel = sectionConfiguration.PrimaryChannelsViewModel;
				control.AddChannelButton.PerformClick();

				AssertEquals("Should not add a new channel when channels are not overridden", 0, BMBoardSectionTestHelper.GetPrimaryAxisChannelCount(sectionConfiguration));
			}
		}

		public void TestAddChannel_WhenOverridden_ShouldAdd()
		{
			var sectionConfiguration = Factory.New<BMSystem>().Boards.AddNew().Sections.AddNew().SectionConfiguration;
			sectionConfiguration.OverrideChannels = true;

			using (var control = new ChannelAssignmentControl())
			{
				control.SetDataBinding(sectionConfiguration.PrimaryChannelsViewModel, string.Empty);
				control.ViewModel = sectionConfiguration.PrimaryChannelsViewModel;
				control.AddChannelButton.PerformClick();

				AssertEquals(1, BMBoardSectionTestHelper.GetPrimaryAxisChannelCount(sectionConfiguration));
			}
		}

		void AddChannel_WhenOverridden_NoBinding_ShouldAdd(bool shouldFireSave, string tabName, bool isPrimary = true)
		{
			var system = Factory.New<BMSystem>();
			system.FS_Name = "dont do that";
			var board = system.Boards.AddNew();

			using (var zForm = new BMBoardForm(board))
			{
				zForm.Show();
				Application.DoEvents();

				var boardSectionsGrid = zForm.FindAll<ZArchitecture.ZGrid>().Single(g => g.Name == "BoardSectionsGrid");
				AssertNotNull(boardSectionsGrid);
				boardSectionsGrid.ListManager.AddNew();
				Application.DoEvents();

				boardSectionsGrid[0, 0] = new ZString("CMP");
				Application.DoEvents();

				var tabControl = zForm.FindAll<ZTabControl>().Single(t => t.Name == "SectionConfigTabControl");
				var channelTabPage = tabControl.FindAll<ZTabPage>().Single(t => t.Text == tabName);
				tabControl.SelectedTab = channelTabPage;
				Application.DoEvents();

				var overrideCheckBox = channelTabPage.FindAll<ZCheckBox>().Single(b => b.Text == "Override Channels");
				overrideCheckBox.Checked = true;

				if (shouldFireSave)
				{
					zForm.FindSingle<ZPostingButtonsUserControl>().SaveButton.PerformClick();
					Application.DoEvents();
				}

				var control = zForm.FindSingle<ChannelAssignmentControl>(w => isPrimary
						? w.Parent is PrimaryChannelsTabPageControl
						: w.Parent is SecondaryChannelsTabPageControl);
				control.AddChannelButton.PerformClick();

				AssertNotNull("The section channels view model should be found", control.ViewModel);
				var axisChannels = isPrimary ? control.ViewModel.PrimaryAxisChannels : control.ViewModel.SecondaryAxisChannels;
				AssertEquals("The channel should be added", 1, axisChannels.Count);
				AssertEquals("The channel should be read-only", false, axisChannels.First().ReadOnly);

				AssertEquals(1, control.ChannelsPanel.Controls.OfType<ChannelRowControl>().Count());
				var channelControl = control.ChannelsPanel.Controls.OfType<ChannelRowControl>().First();
				AssertNotNull(channelControl);
				AssertEquals("Should not be read-only", false, channelControl.ChannelEntityFindBox.ReadOnly);
			}
		}

		public void TestAddChannel_WhenOverridden_NoBinding_FireSaving_ShouldAdd()
		{
			AddChannel_WhenOverridden_NoBinding_ShouldAdd(shouldFireSave: true, tabName: "Primary Channels");
		}

		public void TestAddChannel_WhenOverridden_NoBinding_NoSaving_ShouldAdd_PrimaryChannel()
		{
			AddChannel_WhenOverridden_NoBinding_ShouldAdd(shouldFireSave: false, tabName: "Primary Channels");
		}

		public void TestAddChannel_WhenOverridden_NoBinding_NoSaving_ShouldAdd_SecondaryChannel()
		{
			AddChannel_WhenOverridden_NoBinding_ShouldAdd(shouldFireSave: false, tabName: "Secondary Channels", isPrimary: false);
		}

		public void TestAddChannel_WhenOverridden_NoBinding_FireSaving_CheckSecondaryChannelNotReadOnly()
		{
			var system = Factory.New<BMSystem>();
			system.FS_Name = "dont do that";
			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();
			var sectionConfiguration = section.SectionConfiguration;

			Factory.Save();

			using (var zForm = new BMBoardForm(board))
			{
				zForm.Show();
				Application.DoEvents();

				var tabControl = zForm.FindAll<ZTabControl>().Single(t => t.Name == "SectionConfigTabControl");
				var secondaryChannelTabPage = tabControl.FindAll<ZTabPage>().Single(t => t.Text == "Secondary Channels");
				tabControl.SelectedTab = secondaryChannelTabPage;
				Application.DoEvents();

				var boardSectionsGrid = zForm.FindAll<ZArchitecture.ZGrid>().Single(g => g.Name == "BoardSectionsGrid");
				AssertNotNull(boardSectionsGrid);
				boardSectionsGrid.ListManager.AddNew();
				Application.DoEvents();

				tabControl.SelectedTab = secondaryChannelTabPage;
				boardSectionsGrid[1, 0] = new ZString("CMP");
				Application.DoEvents();

				var overrideCheckBox = secondaryChannelTabPage.FindAll<ZCheckBox>().Single(b => b.Text == "Override Channels");
				overrideCheckBox.Checked = true;

				var control = zForm.FindSingle<ChannelAssignmentControl>(w => w.Parent is SecondaryChannelsTabPageControl);
				control.AddChannelButton.PerformClick();

				AssertNotNull(control.ViewModel);
				AssertEquals(1, control.ViewModel.SecondaryAxisChannels.Count);
				AssertEquals(1, control.ChannelsPanel.Controls.OfType<ChannelRowControl>().Count());
				var channelControl = control.ChannelsPanel.Controls.OfType<ChannelRowControl>().First();
				AssertNotNull(channelControl);
				AssertEquals("Should not be read-only", false, channelControl.ChannelEntityFindBox.ReadOnly);
			}
		}

		public void TestSortChannels()
		{
			var system = Factory.New<BMSystem>();

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup = CreateReleaseGroup(system, group);
			var resource1 = Factory.NewWithValidTestData<GlbStaff>();
			resource1.GS_FullName = "AAA";
			var resource2 = Factory.NewWithValidTestData<GlbStaff>();
			resource2.GS_FullName = "BBB";
			group.Staff.AddRange(resource1, resource2);

			var sectionConfiguration = Factory.New<BMSystem>().Boards.AddNew().Sections.AddNew().SectionConfiguration;
			sectionConfiguration.ReleaseGroupPK = group.PK;
			sectionConfiguration.SortPrimaryChannels = false;

			using (var form = new ZForm())
			using (var control = new ChannelAssignmentControl())
			{
				control.SetDataBinding(sectionConfiguration.PrimaryChannelsViewModel, string.Empty);
				control.ViewModel = sectionConfiguration.PrimaryChannelsViewModel;
				AssertEquals(0, control.ChannelsPanel.Controls.Count);

				sectionConfiguration.ChannelBy = ChannelTypeList.Codes.Resource;
				AssertEquals(2, control.ChannelsPanel.Controls.Count);
				AssertEquals("AAA", control.ChannelsPanel.Controls.OfType<ChannelRowControl>().OrderBy(c => c.Channel_ForTesting.MSC_Sequence).ElementAt(0).Channel_ForTesting.BizoDescription);
				AssertEquals("BBB", control.ChannelsPanel.Controls.OfType<ChannelRowControl>().OrderBy(c => c.Channel_ForTesting.MSC_Sequence).ElementAt(1).Channel_ForTesting.BizoDescription);

				control.ChannelsPanel.Controls.OfType<ChannelRowControl>().OrderBy(c => c.Channel_ForTesting.MSC_Sequence).ElementAt(0).Channel_ForTesting.MSC_Sequence = 2;
				control.ChannelsPanel.Controls.OfType<ChannelRowControl>().OrderBy(c => c.Channel_ForTesting.MSC_Sequence).ElementAt(1).Channel_ForTesting.MSC_Sequence = 1;

				AssertEquals("BBB", control.ChannelsPanel.Controls.OfType<ChannelRowControl>().OrderBy(c => c.Channel_ForTesting.MSC_Sequence).ElementAt(0).Channel_ForTesting.BizoDescription);
				AssertEquals("AAA", control.ChannelsPanel.Controls.OfType<ChannelRowControl>().OrderBy(c => c.Channel_ForTesting.MSC_Sequence).ElementAt(1).Channel_ForTesting.BizoDescription);

				control.ViewModel.SortChannels = true;

				AssertEquals("AAA", control.ChannelsPanel.Controls.OfType<ChannelRowControl>().OrderBy(c => c.Channel_ForTesting.MSC_Sequence).ElementAt(0).Channel_ForTesting.BizoDescription);
				AssertEquals("BBB", control.ChannelsPanel.Controls.OfType<ChannelRowControl>().OrderBy(c => c.Channel_ForTesting.MSC_Sequence).ElementAt(1).Channel_ForTesting.BizoDescription);
			}
		}

		public void TestSortChannels_SaveButton()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			system.FS_Name = "Systembolaget";
			system.FS_Description = "Sweden's government monopoly chain for alcohol retail.";
			var component = BMSTestHelper.CreateBuffer(system, "Buffer", 9001);

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup = CreateReleaseGroup(system, group);
			releaseGroup.Group.GG_Code = "GGG";
			releaseGroup.Group.GG_Desc = "Sverige";

			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "AAA", "Gunnar Borkborkborksen");
			var resource2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "BBB", "Stig Volvenhaagen");

			group.Staff.AddRange(resource1, resource2);

			var resource3 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "CCC", "Ingvar Kamprad");

			var tag1 = BMSTestHelper.CreateTagDefinition(Factory, "TAG", "Tagg");
			var tagitude1 = BMSTestHelper.CreateTagMagnitude(tag1, "TAM", "Tagg");

			var capability1 = BMSTestHelper.CreateCapability(Factory, "CAP", "Kapabel");

			var board = system.Boards.AddNew();
			board.MB_Name = "Smorgasbord";
			var section = board.Sections.AddNew();
			section.MS_FC_Component = component.PK;
			var sectionConfiguration = section.SectionConfiguration;
			sectionConfiguration.ReleaseGroupPK = group.PK;
			sectionConfiguration.SortPrimaryChannels = false;
			sectionConfiguration.PanelLayoutStyle = "STK";
			sectionConfiguration.CellsPerSubsection = 13;

			Factory.Save();

			using (var form = new BMBoardForm(board))
			using (form.DisablePreviewUpdating())
			{
				form.Show();
				Application.DoEvents();

				sectionConfiguration.ChannelBy = ChannelTypeList.Codes.Resource;
				var tabControl = form.FindAll<ZTabControl>().Single(t => t.Name == "SectionConfigTabControl");
				var primaryChannelTabPage = tabControl.FindAll<ZTabPage>().Single(t => t.Text == "Primary Channels");
				tabControl.SelectedTab = primaryChannelTabPage;
				Application.DoEvents();

				var overrideCheckBox = primaryChannelTabPage.FindAll<ZCheckBox>().Single(b => b.Name == "OverrideChannelsCheckBox");
				overrideCheckBox.Checked = true;

				var displayCheckBox = primaryChannelTabPage.FindAll<ZCheckBox>().Single(b => b.Name == "SortCheckBox");
				displayCheckBox.Checked = true;

				var addChannelButton = primaryChannelTabPage.FindAll<ZButton>().Single(b => b.Name == "AddChannelButton");
				addChannelButton.PerformClick();

				var channelsPanel = primaryChannelTabPage.FindAll<ZPanel>().Single(p => p.Name == "ChannelsPanel");
				var channelRowControl = channelsPanel.Controls.OfType<ChannelRowControl>();
				AssertEquals(3, channelRowControl.Count());
				channelRowControl.ElementAt(0).Channel_ForTesting.MSC_ChannelType = ChannelTypeList.Codes.Resource;
				channelRowControl.ElementAt(0).Channel_ForTesting.MSC_ParentID = resource3.PK;

				AssertSaved(form.FireSaveButton());
				Application.DoEvents();

				AssertEquals("Gunnar Borkborkborksen", channelRowControl.ElementAt(0).Channel_ForTesting.BizoDescription);
				AssertEquals("Ingvar Kamprad", channelRowControl.ElementAt(1).Channel_ForTesting.BizoDescription);
				AssertEquals("Stig Volvenhaagen", channelRowControl.ElementAt(2).Channel_ForTesting.BizoDescription);

				AssertEquals(1, channelRowControl.ElementAt(0).Channel_ForTesting.MSC_Sequence);
				AssertEquals(2, channelRowControl.ElementAt(1).Channel_ForTesting.MSC_Sequence);
				AssertEquals(3, channelRowControl.ElementAt(2).Channel_ForTesting.MSC_Sequence);

				var postingButtons = (IPostingButtonsProvider)form;
				Assert(postingButtons.CommandButtonApply.Visible);

				AssertEquals("&New", postingButtons.CommandButtonApply.Text);

				var newFactory = new BusinessObjectFactory();
				var loadedSections = newFactory.Load<BMBoardSection>(new ZQuery());
				AssertEquals(1, loadedSections.Length);

				var loadedChannels = loadedSections[0].SectionConfiguration.PrimaryAxisChannels.OfType<BMBoardSectionChannel>();
				AssertEquals(3, loadedChannels.Count());

				var loadedChannel1 = loadedChannels.First(x => x.MSC_Sequence == 1);
				var loadedChannel2 = loadedChannels.First(x => x.MSC_Sequence == 2);
				var loadedChannel3 = loadedChannels.First(x => x.MSC_Sequence == 3);

				AssertEquals("Gunnar Borkborkborksen", loadedChannel1.BizoDescription);
				AssertEquals("Ingvar Kamprad", loadedChannel2.BizoDescription);
				AssertEquals("Stig Volvenhaagen", loadedChannel3.BizoDescription);

				addChannelButton.PerformClick();
				channelRowControl.ElementAt(0).Channel_ForTesting.MSC_ChannelType = ChannelTypeList.Codes.Group;
				channelRowControl.ElementAt(0).Channel_ForTesting.MSC_ParentID = group.PK;

				addChannelButton.PerformClick();
				channelRowControl.ElementAt(0).Channel_ForTesting.MSC_ChannelType = ChannelTypeList.Codes.Tag;
				channelRowControl.ElementAt(0).Channel_ForTesting.MSC_ParentID = tagitude1.PK;

				addChannelButton.PerformClick();
				channelRowControl.ElementAt(0).Channel_ForTesting.MSC_ChannelType = ChannelTypeList.Codes.Capability;
				channelRowControl.ElementAt(0).Channel_ForTesting.MSC_ParentID = capability1.PK;

				AssertEquals("&Save", postingButtons.CommandButtonApply.Text);

				form.BusinessEntity.RunPreSaveValidation();
				AssertNoErrors((BusinessObject)form.BusinessEntity);

				AssertSaved(form.FireSaveButton());
				Application.DoEvents();

				AssertEquals("&New", postingButtons.CommandButtonApply.Text);

				newFactory = new BusinessObjectFactory();
				loadedSections = newFactory.Load<BMBoardSection>(new ZQuery());
				loadedChannels = loadedSections[0].SectionConfiguration.PrimaryAxisChannels.OfType<BMBoardSectionChannel>();
				AssertEquals(6, loadedChannels.Count());

				loadedChannel1 = loadedChannels.First(x => x.MSC_Sequence == 1);
				loadedChannel2 = loadedChannels.First(x => x.MSC_Sequence == 2);
				loadedChannel3 = loadedChannels.First(x => x.MSC_Sequence == 3);
				var loadedChannel4 = loadedChannels.First(x => x.MSC_Sequence == 4);
				var loadedChannel5 = loadedChannels.First(x => x.MSC_Sequence == 5);
				var loadedChannel6 = loadedChannels.First(x => x.MSC_Sequence == 6);

				AssertEquals("Gunnar Borkborkborksen", loadedChannel1.BizoDescription);
				AssertEquals("Ingvar Kamprad", loadedChannel2.BizoDescription);
				AssertEquals("Kapabel", loadedChannel3.BizoDescription);
				AssertEquals("Stig Volvenhaagen", loadedChannel4.BizoDescription);
				AssertEquals("Sverige", loadedChannel5.BizoDescription);
				AssertEquals("Tagg (Tagg)", loadedChannel6.BizoDescription);
			}
		}

		public void TestSortChannels_AddNewChannelSortAutomatically()
		{
			var system = Factory.New<BMSystem>();

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup = CreateReleaseGroup(system, group);
			var resource1 = Factory.NewWithValidTestData<GlbStaff>();
			resource1.GS_FullName = "BBB";
			group.Staff.Add(resource1);

			var sectionConfiguration = Factory.New<BMSystem>().Boards.AddNew().Sections.AddNew().SectionConfiguration;
			sectionConfiguration.ReleaseGroupPK = group.PK;

			using (var form = new ZForm())
			using (var control = new ChannelAssignmentControl())
			{
				control.SetDataBinding(sectionConfiguration.PrimaryChannelsViewModel, string.Empty);
				control.ViewModel = sectionConfiguration.PrimaryChannelsViewModel;
				control.ViewModel.SortChannels = true;
				AssertEquals(0, control.ChannelsPanel.Controls.Count);

				sectionConfiguration.ChannelBy = ChannelTypeList.Codes.Resource;
				AssertEquals(1, control.ChannelsPanel.Controls.Count);
				AssertEquals("BBB", control.ChannelsPanel.Controls.OfType<ChannelRowControl>().OrderBy(c => c.Channel_ForTesting.MSC_Sequence).ElementAt(0).Channel_ForTesting.BizoDescription);

				control.ViewModel.OverrideChannels = true;
				control.AddChannelButton.PerformClick();

				AssertEquals(2, control.ChannelsPanel.Controls.Count);
				AssertEquals(ZString.Empty, control.ChannelsPanel.Controls.OfType<ChannelRowControl>().OrderBy(c => c.Channel_ForTesting.MSC_Sequence).ElementAt(0).Channel_ForTesting.BizoDescription);
				AssertEquals("BBB", control.ChannelsPanel.Controls.OfType<ChannelRowControl>().OrderBy(c => c.Channel_ForTesting.MSC_Sequence).ElementAt(1).Channel_ForTesting.BizoDescription);
			}
		}
	}
}
