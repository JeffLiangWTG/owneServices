using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.Registry.Business;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.VisualBoards.GUI;
using Enterprise.VisualBoards.GUI.Test;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	[TestedType(typeof(BMBoardForm))]
	class BMBoardFormTest : ZFormBasherTest
	{
		public void TestVisualBoardPreviewUserControlShouldOnlyPreviewOnceWhenOpeningForm()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			var board = system.Boards.AddNew();
			board.Sections.AddNew();
			Factory.Save();

			using (var form = new BMBoardForm_ForTest(board))
			{
				form.Show();
				AssertEquals(1, form.UpdatePreviewsCount);
			}
		}

		public void TestViewBoard_ViewModeOrNoPermissions_ShouldNotShowSaveDialog()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory, "HAH");
			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection);

			Factory.Save();

			var controller = ZControllerFactory.Instance.GetControllerForBizo(config.BufferBoard);

			using (var form = (Form)controller.ShowViewForm(config.BufferBoard))
			{
				Application.DoEvents();

				((ZButton)form.Controls.Find("ViewVisualBoardButton", true)[0]).PerformClick();
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Text.IsNullOrEmpty());
			}
			OpenedFormCache.GetInstance().CloseAllCachedForms();
		}

		public void TestSaveForm_WhenNewAutoChannelCreatedOnSecondSection_ForPrimaryAxis_ShouldNotCausePhantomValidationError()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			config.ReleaseGroup.Staff.Add(resource1);

			var board = BMSTestHelper.CreateBoard(config.System);
			var section1 = BMSTestHelper.CreateBoardSection(config.Bucket, board);
			var section2 = BMSTestHelper.CreateBoardSection(config.Bucket, board);

			section1.SectionConfiguration.ReleaseGroupPK = config.ReleaseGroup.PK;
			section1.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.Resource;

			section2.SectionConfiguration.ReleaseGroupPK = config.ReleaseGroup.PK;
			section2.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.Resource;
			section2.Row = 1;

			Factory.Save();

			AssertEquals(1, section1.SectionConfiguration.PrimaryAxisChannels.Count);
			AssertEquals(1, section2.SectionConfiguration.PrimaryAxisChannels.Count);

			var resource2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			config.ReleaseGroup.Staff.Add(resource2);

			Factory.Save();

			using (var form = (BMBoardForm)ZControllerFactory.Create(ControllerIDs.BMBoard).ShowEditForm(board))
			{
				var formBoard = (BMBoard)form.BusinessEntity;

				formBoard.MB_Description = "Remember when I let that escaped lunatic in the house... 'cause he was dressed like Santa Claus?";

				VisualBoardsTestCase.AssertSaved(form.FireSaveButton(), formBoard);

				var formSection1 = formBoard.Sections.Single(s => s.PK == section1.PK);
				var formSection2 = formBoard.Sections.Single(s => s.PK == section2.PK);

				AssertEquals(2, formSection1.SectionConfiguration.PrimaryAxisChannels.Count);
				AssertEquals(2, formSection2.SectionConfiguration.PrimaryAxisChannels.Count);
			}
		}

		public void TestSaveForm_WhenNewAutoChannelCreatedOnSecondSection_ForSecondaryAxis_ShouldNotCausePhantomValidationError()
		{
			var config = TestConfigsHelper.CreateTagsTestConfig(Factory);

			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			config.ReleaseGroup.Staff.Add(resource1);

			var board = BMSTestHelper.CreateBoard(config.System);
			var section1 = BMSTestHelper.CreateBoardSection(config.Bucket, board);
			var section2 = BMSTestHelper.CreateBoardSection(config.Bucket, board);

			BMSTestHelper.CreatePrimaryChannelForSection(section1, ChannelTypeList.Codes.Tag, config.RedTag.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(section2, ChannelTypeList.Codes.Tag, config.RedTag.PK);

			section1.SectionConfiguration.ReleaseGroupPK = config.ReleaseGroup.PK;
			section1.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.NotChanneled;
			section1.SectionConfiguration.ChannelSecondaryBy = ChannelTypeList.Codes.Resource;

			section2.SectionConfiguration.ReleaseGroupPK = config.ReleaseGroup.PK;
			section2.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.NotChanneled;
			section2.SectionConfiguration.ChannelSecondaryBy = ChannelTypeList.Codes.Resource;
			section2.Row = 1;

			Factory.Save();

			AssertEquals(1, section1.SectionConfiguration.SecondaryAxisChannels.Count);
			AssertEquals(1, section2.SectionConfiguration.SecondaryAxisChannels.Count);

			var resource2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			config.ReleaseGroup.Staff.Add(resource2);

			Factory.Save();

			using (var form = (BMBoardForm)ZControllerFactory.Create(ControllerIDs.BMBoard).ShowEditForm(board))
			{
				var formBoard = (BMBoard)form.BusinessEntity;

				formBoard.MB_Description = "Well, YOUUUU have a gambling problem!";

				VisualBoardsTestCase.AssertSaved(form.FireSaveButton(), formBoard);

				var formSection1 = formBoard.Sections.Single(s => s.PK == section1.PK);
				var formSection2 = formBoard.Sections.Single(s => s.PK == section2.PK);

				AssertEquals(2, formSection1.SectionConfiguration.SecondaryAxisChannels.Count);
				AssertEquals(2, formSection2.SectionConfiguration.SecondaryAxisChannels.Count);
			}
		}

		public void TestSaveForm_WhenAttemptingToSaveBoardWithValidationError_ShouldNotThrowException()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			Factory.Save();

			var board = BMSTestHelper.CreateBoard(config.System);

			using (var form = new BMBoardForm(board))
			{
				form.Show();
				Application.DoEvents();

				var section = board.Sections.AddNew();
				section.MS_FC_Component = config.Bucket.PK;

				board.RunPreSaveValidation();
				AssertHasError(section.SectionConfiguration.ReleaseGroupPKInfo, "Please enter a Release Group or add channels to this board section. Without these it is likely there would be a large volume of work shown on this board section.");

				form.FireSaveButton();

				AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);

				section.SectionConfiguration.ReleaseGroupPK = config.ReleaseGroup.PK;

				board.RunPreSaveValidation();
				AssertNoErrors(board);

				AssertNoExceptionThrown(() => form.FireSaveButton());
			}
		}

		public void TestSaveForm_ShouldUpdateApplicableLayouts()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var board = BMSTestHelper.CreateBoard(config.System);
			var section = BMSTestHelper.CreateBoardSection(config.Bucket, board);
			var layout = BMSTestHelper.CreateControlCustomisation(Factory, name: "ส็็็็็็็็็็็็็็็็็็็_(ツ)_ส้้้้้้้้้้้้้้้้้้้้");
			var layoutLink = BMSTestHelper.CreateControlCustomisationLink(Factory, board, layout);

			Factory.Save();

			using (var form = new BMBoardForm(board))
			{
				form.Show();
				Application.DoEvents();

				var tabControl = form.FindAll<BoardSectionConfigControl>().Single().FindAll<ZTabControl>().Single();
				var tabPage = tabControl.TabPages.Cast<ZTabPage>().Single(t => t.Text == "Customized Layouts");

				tabControl.SelectedTab = tabPage;
				Application.DoEvents();

				var layoutsGrid = (ZGrid)tabPage.Controls.Find("RelevantLayoutsGrid", true)[0];

				AssertEquals(1, layoutsGrid.List.Count);
				AssertEquals("ส็็็็็็็็็็็็็็็็็็็_(ツ)_ส้้้้้้้้้้้้้้้้้้้้", layoutsGrid[0, 0]);

				layout.FM_Name = "top lel";
				AssertEquals("ส็็็็็็็็็็็็็็็็็็็_(ツ)_ส้้้้้้้้้้้้้้้้้้้้", layoutsGrid[0, 0]);

				Factory.Save();
				AssertEquals("top lel", layoutsGrid[0, 0]);
			}
		}

		[ExpectNoExceptions]
		public void TestWorkflowFilterStrips_SwitchingBetweenSections_FiltersRefreshCorrectly()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var component = BMSTestHelper.CreateBucket(system);
			var board = BMSTestHelper.CreateBoard(system);
			var section1 = BMSTestHelper.CreateBoardSection(component, board);
			var section2 = BMSTestHelper.CreateBoardSection(component, board);

			Factory.Save();

			using (var form = new BMBoardForm(board))
			{
				var control = form.FindAll<BoardSectionConfigControl>().Single();
				form.Show();
				Application.DoEvents();

				var tabControl = control.FindAll<ZTabControl>().Single();
				tabControl.SelectTab("WorkflowFilterTabControl");
				var filterTabControl = tabControl.FindAll<FilterTabPageControl>().Single(x => x.BindToProperty == "WorkflowFilter");
				Application.DoEvents();

				var filterStripControl = filterTabControl.FindAll<FilterRuleFilterStripControl>().Single();

				var addStripButton = filterStripControl.FindAll<ZFilterStripAddButton>().Single().FindAll<ZButton>().Single();
				addStripButton.PerformClick();
				addStripButton.PerformClick();
				Application.DoEvents();

				var stripControls = filterStripControl.FindAll<ZFilterStrip>().ToArray();
				AssertEquals(3, stripControls.Length);
				AssertEquals(section1.WorkflowFilter, filterStripControl.CurrentDataItem);

				BMSGUITestCase.SetFilterStripName(stripControls[0], ProcessHeader.ModuleFilterConstants.JobOrWorkflow);
				BMSGUITestCase.SetFilterStripName(stripControls[1], ProcessHeader.ModuleFilterConstants.JobOrWorkflow);
				BMSGUITestCase.SetFilterStripName(stripControls[2], ProcessHeader.ModuleFilterConstants.JobOrWorkflow);

				control.BoardSectionsGrid.ListManager.Position = 1;
				Application.DoEvents();
				AssertEquals(1, filterStripControl.FindAll<ZFilterStrip>().ToArray().Length);
				AssertEquals(section2.WorkflowFilter, filterStripControl.CurrentDataItem);

				control.BoardSectionsGrid.ListManager.Position = 0;
				Application.DoEvents();
				AssertEquals(3, filterStripControl.FindAll<ZFilterStrip>().ToArray().Length);
				AssertEquals(section1.WorkflowFilter, filterStripControl.CurrentDataItem);
			}
		}

		[ExpectNoExceptions]
		public void TestTaskFilterStrips_SwitchingBetweenSections_FiltersRefreshCorrectly()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var component = BMSTestHelper.CreateBucket(system);
			var board = BMSTestHelper.CreateBoard(system);
			var section1 = BMSTestHelper.CreateBoardSection(component, board);
			var section2 = BMSTestHelper.CreateBoardSection(component, board);

			Factory.Save();

			using (var form = new BMBoardForm(board))
			{
				var control = form.FindAll<BoardSectionConfigControl>().Single();
				form.Show();
				Application.DoEvents();

				var tabControl = control.FindAll<ZTabControl>().Single();
				tabControl.SelectTab("TaskFilterTabControl");
				var filterTabControl = tabControl.FindAll<FilterTabPageControl>().Single(x => x.BindToProperty == "TaskFilter");
				Application.DoEvents();

				var filterStripControl = filterTabControl.FindAll<FilterRuleFilterStripControl>().Single();

				var addStripButton = filterStripControl.FindAll<ZFilterStripAddButton>().Single().FindAll<ZButton>().Single();
				addStripButton.PerformClick();
				addStripButton.PerformClick();
				Application.DoEvents();

				var stripControls = filterStripControl.FindAll<ZFilterStrip>().ToArray();
				AssertEquals(3, stripControls.Length);
				AssertEquals(section1.SectionConfiguration.TaskFilter, filterStripControl.CurrentDataItem);

				BMSGUITestCase.SetFilterStripName(stripControls[0], "Description");
				BMSGUITestCase.SetFilterStripName(stripControls[1], "Description");
				BMSGUITestCase.SetFilterStripName(stripControls[2], "Description");

				control.BoardSectionsGrid.ListManager.Position = 1;
				Application.DoEvents();
				AssertEquals(1, filterStripControl.FindAll<ZFilterStrip>().ToArray().Length);
				AssertEquals(section2.SectionConfiguration.TaskFilter, filterStripControl.CurrentDataItem);

				control.BoardSectionsGrid.ListManager.Position = 0;
				Application.DoEvents();
				AssertEquals(3, filterStripControl.FindAll<ZFilterStrip>().ToArray().Length);
				AssertEquals(section1.SectionConfiguration.TaskFilter, filterStripControl.CurrentDataItem);
			}
		}

		public void TestHasChanges_BoardSectionConfig()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket = BMSTestHelper.CreateBucket(system);
			var board = BMSTestHelper.CreateBoard(system);
			var section = BMSTestHelper.CreateBoardSection(bucket, board);

			Factory.Save();

			using (var form = (BMBoardForm)ZControllerFactory.Create(ControllerIDs.BMBoard).ShowEditForm(board))
			{
				form.Show();
				Application.DoEvents();

				AssertEquals(false, board.HasChanges);

				var rowPercentControl = form.Find(x => x.Name == "RowHeightPercentCalcEdit").Cast<ZCalcEdit>().Single();
				rowPercentControl.Focus();
				rowPercentControl.CalcValue = 7;

				var anotherControl = form.Controls.Find("ColWidthPercentCalcEdit", true)[0];
				anotherControl.Focus();

				Application.DoEvents();

				var sectionControl = form.Find(x => x.Name == "BoardSectionConfigControl").OfType<BoardSectionConfigControl>().Single();
				var loadedBoard = form.DataSource;
				var loadedSection = sectionControl.GetSelectedSection();

				AssertEquals(section.PK, loadedSection.PK);
				AssertEquals(board.PK, ((BMBoard)sectionControl.CurrentDataItem).PK);

				Application.DoEvents();
				AssertEquals(7, loadedSection.RowHeightPercent);
				AssertEquals(true, loadedSection.HasChanges);
				AssertEquals(true, ((IBusiness)loadedBoard.Sections).HasChanges);
				AssertEquals(true, loadedBoard.HasChanges);
			}
		}

		public void TestSectionDrawsProperly()
		{
			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket = BMSTestHelper.CreateBucket(system);
			var board = BMSTestHelper.CreateBoard(system);
			var section = BMSTestHelper.CreateBoardSection(bucket, board);

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staff.PK);

			Factory.Save();

			using (var form = new BMBoardForm(board))
			{
				form.Show();
				Application.DoEvents(); // Necessary so tab controls are added.
				var tabControl = form.FindAll<ZTabControl>().Single(t => t.Name == "SectionConfigTabControl");
				var primaryChannelTabPage = tabControl.FindAll<ZTabPage>().Single(t => t.Text == "Primary Channels");
				tabControl.SelectedTab = primaryChannelTabPage;

				var primaryChannelsTabPageControl = primaryChannelTabPage.FindAll<ChannelAssignmentControl>().Single();
				Application.DoEvents(); // Necessary so that channelRowControls are rendered.

				AssertEquals("Just the staff from before", 1, primaryChannelsTabPageControl.FindAll<ChannelRowControl>().Count());

				primaryChannelsTabPageControl.FindAll<ZButton>().Single(c => c.Name == "AddChannelButton").PerformClick();

				Application.DoEvents();

				var channelRowControls = form.FindAll<PrimaryChannelsTabPageControl>().Single().FindAll<ChannelRowControl>().ToArray();
				AssertEquals(2, channelRowControls.Length);
				AssertEquals(true, channelRowControls[0].IsHandleCreated);
			}
		}

		public void TestSectionHasErrors()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket = BMSTestHelper.CreateBucket(system);
			var board = BMSTestHelper.CreateBoard(system);
			var section = BMSTestHelper.CreateBoardSection(bucket, board);

			Factory.Save();

			using (var form = new BMBoardForm(board))
			{
				section.MS_SectionType = "ERR";
				form.Show();
				Assert(section.HasErrors);
			}
		}

		#region Preview

		public void TestShowBoardPreview()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "RC1", "Resource 1");
			var resource2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "RC2", "Resource 2");

			var sectionAndView = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 13, FlowDirectionList.Codes.Up, LastCellList.Codes.Right, ChannelTypeList.Codes.Resource, resource1);
			var section1 = sectionAndView.Item1;
			var buffer = section1.Component;
			var board = sectionAndView.Item1.Board;

			var section2 = BMSTestHelper.CreateBoardSection(buffer, board, 1, 1);
			section2.SectionConfiguration.CellsPerSubsection = 4;
			BMSTestHelper.CreatePrimaryChannelForSection(section2, ChannelTypeList.Codes.Resource, resource2.PK, true);

			Factory.Save();

			using (var form = new BMBoardForm(board))
			{
				form.Show();

				Assert(!section1.HasErrors);
				Assert(!section2.HasErrors);

				Application.DoEvents();

				var boardSectionPreviewControl = GetSectionPreviewControl(form);
				var boardPreviewControl = form.FindAll<VisualBoardPreviewUserControl>().FirstOrDefault(c => c.Name == "BoardPreviewControl");

				var previewTabControl = form.FindAll<ZTabControl>().FirstOrDefault(c => c.Name == "PreviewTabControl");
				previewTabControl.SelectedIndex = 1;

				Assert("Section-preview must be drawn", boardSectionPreviewControl.Controls.Count > 0);
				Assert("Board-preview must be drawn", boardPreviewControl.Controls.Count > 0);
			}
		}

		[ExpectNoExceptions]
		public void TestLoadForm_BeforeSavingToDatabase()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var board = config.Buffer.System.Boards.AddNew();
			board.Sections.AddNew();

			using (var form = (BMBoardForm)ZControllerFactory.Create(ControllerIDs.BMBoard).ShowFormForNewEntity(board))
			{
				Assert(!board.IsInDatabase && form != null);
			}
		}

		[ExpectNoExceptions]
		public void TestShowBoardPreview_BeforeSavingToDatabase()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var board = config.Buffer.System.Boards.AddNew();
			board.Sections.AddNew();

			using (KUserControl.TrackInstantiatedControls_ForTest())
			using (var form = (BMBoardForm)ZControllerFactory.Create(ControllerIDs.BMBoard).ShowFormForNewEntity(board))
			{
				Assert("Preview should be able to load before saving", KUserControl.InstantiatedControls_ForTest[typeof(BMComponentControl)] > 0);
			}
		}

		public void TestShowBoardPreview_DoesNotPreviewWhenValidationErrorsArePresent()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var resource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "RC1", "Resource 1");

			var sectionAndView = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 1, 13, FlowDirectionList.Codes.Up, LastCellList.Codes.Right, ChannelTypeList.Codes.Resource);
			var section = sectionAndView.Item1;
			section.SectionConfiguration.TimePerCell = ZDateTime.Today;
			section.SectionConfiguration.ReleaseGroupPK = config.ReleaseGroup.PK;
			var bucket = section.Component;
			var board = sectionAndView.Item1.Board;

			Factory.Save();

			using (var form = new BMBoardForm(board))
			{
				form.Show();

				AssertNoErrors(section);

				Application.DoEvents();

				var boardSectionPreviewControl = GetSectionPreviewControl(form);
				var previewTabControl = form.FindAll<ZTabControl>().FirstOrDefault(c => c.Name == "PreviewTabControl");
				previewTabControl.SelectedIndex = 1;

				Assert("Section-preview must be drawn", boardSectionPreviewControl.Controls.Count > 0);
			}

			//set the two channel to the same resource PK should expect an error
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource.PK, true);
			BMSTestHelper.CreateSecondaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource.PK, true);

			using (var form = new BMBoardForm(board))
			{
				form.Show();

				Assert(section.HasErrors);

				Application.DoEvents();

				var boardSectionPreviewControl = GetSectionPreviewControl(form);
				var previewTabControl = form.FindAll<ZTabControl>().FirstOrDefault(c => c.Name == "PreviewTabControl");
				previewTabControl.SelectedIndex = 1;

				AssertEquals("Section-preview shan't be drawn", boardSectionPreviewControl.FindAll<ZLabel>().Single().Text, "Cannot preview, please check section values.");
			}
		}

		public void TestDeleteChannelDoesNotCauseException()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var resource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "RC1", "Resource 1");
			var capacity = BMSTestHelper.CreateCapability(Factory, "CP1", "Capability 1");

			var sectionAndView = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 1, 13, FlowDirectionList.Codes.Up, LastCellList.Codes.Right, ChannelTypeList.Codes.Resource);
			var section = sectionAndView.Item1;
			section.SectionConfiguration.TimePerCell = ZDateTime.Today;
			section.SectionConfiguration.ReleaseGroupPK = config.ReleaseGroup.PK;
			var bucket = section.Component;
			var board = sectionAndView.Item1.Board;

			var primChan = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource.PK, true);

			section.SectionConfiguration.OverrideSecondaryChannels = true;
			section.SectionConfiguration.ShowSecondaryUnchanneled = true; // when this set to true, one item will be added automatically
			var secChan = section.SectionConfiguration.SecondaryAxisChannels[0];
			secChan.MSC_ChannelType = ChannelTypeList.Codes.Capability;

			Factory.Save();

			using (var form = new BMBoardForm(board))
			{
				form.Show();

				AssertNoErrors(section);

				Application.DoEvents();

				var boardSectionPreviewControl = GetSectionPreviewControl(form);
				var previewTabControl = form.FindAll<ZTabControl>().FirstOrDefault(c => c.Name == "PreviewTabControl");
				previewTabControl.SelectedIndex = 1;

				Assert("Section-preview must be drawn", boardSectionPreviewControl.Controls.Count > 0);
			}

			//clear primary channel and it should have validation and not throwing exception
			var viewModel = new BMBoardSectionChannelsViewModel(section.SectionConfiguration, ChannelAxis.Primary);
			viewModel.DeleteChannel(primChan, ChannelAxis.Primary);

			using (var form = new BMBoardForm(board))
			{
				form.Show();

				Assert(section.HasErrors);

				Application.DoEvents();

				var boardSectionPreviewControl = GetSectionPreviewControl(form);
				var previewTabControl = form.FindAll<ZTabControl>().FirstOrDefault(c => c.Name == "PreviewTabControl");
				previewTabControl.SelectedIndex = 1;

				AssertEquals("Section-preview shan't be drawn", boardSectionPreviewControl.FindAll<ZLabel>().Single().Text, "Cannot preview, please check section values.");
			}
		}

		public void TestPreview_ShouldDisposePreviouslyRenderedBitmaps()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var board = BMSTestHelper.CreateBoard(config.System);
			BMSTestHelper.CreateBoardSection(config.Bucket, board);
			BMSTestHelper.CreateBoardSection(config.Buffer, board, row: 1);

			using (var form = new BMBoardForm(board))
			{
				form.Show();
				Application.DoEvents();

				var configControl = form.FindAll<BoardSectionConfigControl>().Single();
				var boardSectionPreviewControl = GetSectionPreviewControl(form);
#if WINZOR
				var pictureBox = boardSectionPreviewControl.FindSingleOrDefault<System.Windows.Forms.Adaptive.PreviewBox>();
				AssertNotNull(pictureBox);
				AssertNotNull(pictureBox.PreviewControl);

				configControl.BoardSectionsGrid.ListManager.Position = 1;
				Application.DoEvents();

				var newPictureBox = boardSectionPreviewControl.FindSingleOrDefault<System.Windows.Forms.Adaptive.PreviewBox>();

				AssertNotNull(newPictureBox);
				AssertNotEquals("Should have re-rendered the section preview since a new section was selected", pictureBox, newPictureBox);

				AssertNotNull(newPictureBox.PreviewControl);
				AssertNotEquals(pictureBox.PreviewControl, newPictureBox.PreviewControl);

				AssertEquals("The new picture box is still being shown, so it should not have been disposed", false, newPictureBox.IsDisposed);
				AssertEquals("The new picture box is still being shown, so its PreviewControl should not have been disposed", false, newPictureBox.PreviewControl.IsDisposed);

				AssertEquals("The first picture box is no longer shown, so it should have been disposed", true, pictureBox.IsDisposed);
				AssertEquals("The first picture box has been disposed, so its PreviewControl should have been disposed too", true, pictureBox.IsDisposed);
#else
				var pictureBox = boardSectionPreviewControl.FindSingleOrDefault<OptimisticPictureBox>();

				AssertNotNull(pictureBox);
				AssertNotNull(pictureBox.Image);

				var firstRenderedImage = pictureBox.Image;

				AssertEquals("800x600 is a reasonable resolution for the preview pane", new Size(800, 600), firstRenderedImage.Size);

				configControl.BoardSectionsGrid.ListManager.Position = 1;
				Application.DoEvents();

				var newPictureBox = boardSectionPreviewControl.FindSingleOrDefault<OptimisticPictureBox>();

				AssertNotNull(newPictureBox);
				AssertNotEquals("Should have re-rendered the section preview since a new section was selected", pictureBox, newPictureBox);

				AssertNotNull(newPictureBox.Image);
				AssertNotEquals(firstRenderedImage, newPictureBox.Image);

				AssertEquals("The new picture box is still being shown, so it should not have been disposed", false, newPictureBox.IsDisposed);
				AssertEquals("The new picture box is still being shown, so its image should not have been disposed", false, newPictureBox.Image.IsDisposed());

				AssertEquals("The first picture box is no longer shown, so it should have been disposed", true, pictureBox.IsDisposed);
				AssertEquals("The first picture box has been disposed, so its image should have been disposed too", true, firstRenderedImage.IsDisposed());
#endif
			}
		}

		#endregion

		public void TestShowPreDeleteDialogs()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "WKI");
			var boardToDelete = BMSTestHelper.CreateBoard(system, "No Slideshows Board");

			Factory.Save();

			using (var form = new BMBoardForm_ForTest(boardToDelete) { DisplayMode = ODisplayMode.Delete })
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.FireDeleteButton();

				AssertNull("The dialog should not have been shown because the board to delete has no slideshows associated.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("The board should have been deleted.", boardToDelete.IsDeleted);
			}

			boardToDelete = BMSTestHelper.CreateBoard(system, "Doomed Board");
			var otherLinkedBoard = BMSTestHelper.CreateBoard(system, "Resillient Board");

			var slideshow1 = BMSTestHelper.CreateSlideshow(Factory, boardToDelete);
			slideshow1.MD_Name = "Oatmeal Enthusiasts Unite!";
			var slideshow2 = BMSTestHelper.CreateSlideshow(Factory, boardToDelete);
			slideshow2.MD_Name = "Trampoline Inspection Rules";

			var boardSlideShow1Pivot = slideshow1.BoardPivots.Single();
			var boardSlideShow2Pivot = slideshow2.BoardPivots.Single();
			var otherBoardSlideshow2Pivot = Factory.New<BMBoardSlideshowPivot>();
			otherBoardSlideshow2Pivot.MC_MB_Board = otherLinkedBoard.PK;
			otherBoardSlideshow2Pivot.MC_MD_Slideshow = slideshow2.PK;

			Factory.Save();

			using (var form = new BMBoardForm_ForTest(boardToDelete) { DisplayMode = ODisplayMode.Delete })
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				form.FireDeleteButton();

				var expected = "This board will be permanently deleted and unlinked from the following slide shows:\r\n\r\nOatmeal Enthusiasts Unite! (slide show will be deleted)\r\nTrampoline Inspection Rules\r\n\r\nDo you want to proceed?";

				AssertEquals("The correct warning message should have been displayed.", expected, UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("The board should not have been deleted because the response to the warning dialog was Cancel.", !boardToDelete.IsDeleted);
				Assert("The first slideshow should not have been deleted because the response to the warning dialog was Cancel.", !slideshow1.IsDeleted);
				Assert("The second slideshow should not have been deleted because it still has undeleted pivots.", !slideshow2.IsDeleted);
				Assert("The pivot to the first slideshow should not have been deleted because the response to the warning dialog was Cancel.", !boardSlideShow1Pivot.IsDeleted);
				Assert("The pivot to the second slideshow should not have been deleted because the response to the warning dialog was Cancel.", !boardSlideShow2Pivot.IsDeleted);
				Assert("The pivot between the other board and slideshow2 should not have been deleted because it isn't involved in this operation.", !otherBoardSlideshow2Pivot.IsDeleted);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				form.FireDeleteButton();

				AssertEquals("The correct warning message should have been displayed.", expected, UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("The board should have been deleted because the response to the warning dialog was OK.", boardToDelete.IsDeleted);
				Assert("The first slideshow should have been deleted because the response to the warning dialog was OK and all its board pivots were deleted.", slideshow1.IsDeleted);
				Assert("The second slideshow should not have been deleted because, though the response to the warning dialog was OK and the first board was deleted, it has a remaining pivot.", !slideshow2.IsDeleted);
				Assert("The pivot to the first slideshow should have been deleted because the response to the warning dialog was OK.", boardSlideShow1Pivot.IsDeleted);
				Assert("The pivot to the second slideshow should have been deleted because the response to the warning dialog was OK.", boardSlideShow2Pivot.IsDeleted);
				Assert("The pivot between the other board and slideshow2 should not have been deleted because it isn't involved in this operation.", !otherBoardSlideshow2Pivot.IsDeleted);
			}
		}

		public void TestNewUnidentifiedFilterWrapperControl_ShouldThrowException()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer 1");
			var board = BMSTestHelper.CreateBoard(system, "Boardie");
			var section = BMSTestHelper.CreateBoardSection(buffer, board);

			Factory.Save();

			using (var form = new BMBoardForm(board))
			{
				form.Show();

				var sectionControl = form.FindAll<BoardSectionConfigControl>().Single();
				Application.DoEvents();
				var filterTabControl = sectionControl.SectionConfigTabControl.GetTabPage("WorkflowFilterTabControl");
				sectionControl.SectionConfigTabControl.SelectTab(filterTabControl);
				var filterControl = filterTabControl.FindAll<BMFilterStripWrapperControl>().Single();
				filterControl.FilterControlIdentifier = "New Control";
				var toolStrip = filterControl.FindAll<ZToolStrip>().Single(x => x.Name == "ToolStrip");
				var previewButton = toolStrip.Items["ToolStripPreviewDropButton"];

				AssertExceptionThrown<UnidentifiedFilterControlException>("BMBoardForm's GetObjectForPreview method isn't set up to handle previewing from a control with the FilterControlIdentifier == 'New Control', so an exception should have been thrown, and yet...",
					() => previewButton.PerformClick());
			}
		}

		public void TestExperimentalSettingsButtonDisabledByDefault()
		{
			var system = Factory.New<BMSystem>();
			var board = system.Boards.AddNew();

			using (var form = new BMBoardForm(board))
			{
				form.Show();
				var button = form.GetControl<ZButton>("ExperimentalSettingsButton");
				AssertEquals(false, button.Visible);
			}
		}

		public void TestExperimentalSettingsButtonEnabledWithRegistryKeyOn()
		{
			BMSRegistry.Instance.EnablePaveExperimentalFeatures.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var system = Factory.New<BMSystem>();
			var board = system.Boards.AddNew();

			using (var form = new BMBoardForm(board))
			{
				form.Show();
				var button = form.GetControl<ZButton>("ExperimentalSettingsButton");
				AssertEquals(true, button.Visible);
			}
		}

		public void TestExperimentalSettingsButtonDisabledWithRegistryKeyOnButNoPermission()
		{
			BMSRegistry.Instance.EnablePaveExperimentalFeatures.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Env.Security.BMSystemsEdit.IsAllowed = false;

			var system = Factory.New<BMSystem>();
			var board = system.Boards.AddNew();

			using (var form = new BMBoardForm(board))
			{
				form.Show();
				var button = form.GetControl<ZButton>("ExperimentalSettingsButton");
				AssertEquals(false, button.Visible);
			}
		}

		#region Section Name

		public void TestCmpCustomSectionNameIsAutoTruncated()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");
			var board = BMSTestHelper.CreateBoard(system, "Boardie");
			var boardSection = BMSTestHelper.CreateBoardSection(buffer, board);

			Factory.Save();

			using (var form = new BMBoardForm(board))
			{
				form.Show();
				Application.DoEvents();

				var overrideBox = form.FindSingle<ZOverridableTextBox>(x => x.Name == "OverriddenSectionNameCheckedTextBox");
				AssertEquals("Precondition: MaxLength should equal to...", 200, overrideBox.MaxLength);

				overrideBox.Checked = true;
				overrideBox.TextOverride = ZString.Replicate('A', 201);
				AssertEquals("When checked, custom section name should be auto truncated if too long", "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", overrideBox.Text); //truncated to 200 characters
			}
		}

		public void TestModCustomSectionNameIsAutoTruncated()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer 1");
			var board = BMSTestHelper.CreateBoard(system, "Boardie");
			var modSection = BMSTestHelper.CreateBoardSection(ModuleIDs.WorkItem, board);
			var config = (ModuleGridSectionConfiguration)modSection.Configuration;

			Factory.Save();

			using (var form = new BMBoardForm(board))
			{
				form.Show();
				Application.DoEvents();

				var overrideBox = form.FindSingle<ZOverridableTextBox>(x => x.Name == "OverriddenSectionNameCheckedTextBox");
				AssertEquals("Precondition: MaxLength should equal to...", 200, overrideBox.MaxLength);

				overrideBox.Checked = true;
				overrideBox.TextOverride = ZString.Replicate('A', 201);
				AssertEquals("When checked, custom section name should be auto truncated if too long", "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", overrideBox.Text); //truncated to 200 characters
			}
		}

		public void TestCmpDefaultSectionNameIsTruncated()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer0Name = ZString.Replicate('A', 100);
			var buffer0 = BMSTestHelper.CreateBuffer(system, buffer0Name);
			var buffer1Name = ZString.Replicate('B', 100);
			var buffer1 = BMSTestHelper.CreateBuffer(system, buffer1Name);
			var buffer2Name = ZString.Replicate('C', 100);
			var buffer2 = BMSTestHelper.CreateBuffer(system, buffer2Name);
			var board = BMSTestHelper.CreateBoard(system, "Boardie");
			var section = BMSTestHelper.CreateBoardSection(buffer0, board);
			var config = (BMComponentSectionConfiguration)section.Configuration;

			var additionalComponent1 = Factory.NewWithValidTestData<BMBoardSectionAdditionalComponent>();
			additionalComponent1.BSA_MS_Section = section.PK;
			additionalComponent1.BSA_FC_Component = buffer1.PK;
			config.AdditionalComponents.Add(additionalComponent1);

			var additionalComponent2 = Factory.NewWithValidTestData<BMBoardSectionAdditionalComponent>();
			additionalComponent2.BSA_MS_Section = section.PK;
			additionalComponent2.BSA_FC_Component = buffer2.PK;
			config.AdditionalComponents.Add(additionalComponent2);

			Factory.Save();

			using (var form = new BMBoardForm(board))
			{
				form.Show();
				Application.DoEvents();

				var overrideBox = form.FindSingle<ZOverridableTextBox>(x => x.Name == "OverriddenSectionNameCheckedTextBox");
				AssertEquals("Precondition: MaxLength should equal to...", 200, overrideBox.MaxLength);
				var defaultSectionName = buffer0Name + ", " + buffer1Name + ", " + buffer2Name;
				AssertEquals("", defaultSectionName, config.DefaultSectionName);

				overrideBox.Checked = false;
				AssertEquals("When not checked, custom section name should equal to truncated default section name if it is too long", "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA, BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB...", overrideBox.Text);

				overrideBox.Checked = true;
				AssertEquals("When switched to checked, custom section name should be cleared if default section name is too long", string.Empty, overrideBox.Text);
			}
		}

		public void TestChangingBoardComponentUpdatesSectionNameOverride()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer1 = BMSTestHelper.CreateBuffer(system, "Buffer 1");
			var buffer2 = BMSTestHelper.CreateBuffer(system, "Buffer with a very long name");
			var board = BMSTestHelper.CreateBoard(system, "Boardie");
			var boardSection = BMSTestHelper.CreateBoardSection(buffer1, board);

			Factory.Save();

			using (var form = new BMBoardForm(board))
			{
				form.Show();
				Application.DoEvents();

				var overrideBox = form.FindSingle<ZOverridableTextBox>(x => x.Name == "OverriddenSectionNameCheckedTextBox");
				overrideBox.MaxLength = 20;
				overrideBox.Checked = false;

				AssertEquals("Precondition: PlaceholderText should contain the name of the buffer", "Buffer 1", overrideBox.PlaceholderText);
				AssertEquals("Precondition: When not checked, Text should equal to default section name if the name is short enough", "Buffer 1", overrideBox.Text);

				boardSection.MS_FC_Component = buffer2.PK;
				AssertEquals("When component is changed, Text should be updated taking MaxLength into account", "Buffer with a ver...", overrideBox.Text);
			}
		}

		public void TestOverriddenSectionNameCheckedTextBox_IsCorrectlyInitialized_ForCmpSection()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");
			var board = BMSTestHelper.CreateBoard(system, "Boardie");
			var boardSection = BMSTestHelper.CreateBoardSection(buffer, board);
			var config = (BMComponentSectionConfiguration)boardSection.Configuration;
			config.SectionNameIsOverridden = false;
			config.SectionNameOverride = "This text should not be shown because section name is not overridden";

			Factory.Save();

			using (var form = new BMBoardForm(board))
			{
				form.Show();
				Application.DoEvents();

				var overrideBox = form.FindSingle<ZOverridableTextBox>(x => x.Name == "OverriddenSectionNameCheckedTextBox");
				AssertEquals("Text should be initialized based on SectionNameIsOverridden and contain DefaultSectionName when SectionNameOverridden is set to false", "Buffer", overrideBox.Text);
			}

			config.SectionNameIsOverridden = true;
			config.SectionNameOverride = "I'll override you!";

			Factory.Save();

			using (var form = new BMBoardForm(board))
			{
				form.Show();
				Application.DoEvents();

				var overrideBox = form.FindSingle<ZOverridableTextBox>(x => x.Name == "OverriddenSectionNameCheckedTextBox");
				AssertEquals("Text should be initialized based on SectionNameIsOverridden and contain SectionNameOverride when SectionNameOverridden is set to true", "I'll override you!", overrideBox.Text);
			}
		}

		public void TestOverriddenSectionNameCheckedTextBox_IsCorrectlyInitialized_ForModSection()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var board = BMSTestHelper.CreateBoard(system, "Boardie");
			var section = BMSTestHelper.CreateBoardSection(ModuleIDs.ProcessHeader, board);
			var config = (ModuleGridSectionConfiguration)section.Configuration;
			AssertEquals(string.Empty, config.SectionName);

			var panelConfig = config.PanelConfigurations.First() as ModuleGridSectionPanelConfiguration;
			var filter = Factory.New<StmModuleFilter>();
			filter.S9_ModuleID = ModuleIDs.ProcessHeader.Name;
			filter.S9_FilterName = "I will be a section name!";
			panelConfig.FilterLayout = filter.PK;
			AssertEquals("I will be a section name!", panelConfig.PanelName);

			config.SectionNameIsOverridden = false;
			config.SectionNameOverride = "This text should not be shown because section name is not overridden";

			Factory.Save();

			using (var form = new BMBoardForm(board))
			{
				form.Show();
				Application.DoEvents();

				var overrideBox = form.FindSingle<ZOverridableTextBox>(x => x.Name == "OverriddenSectionNameCheckedTextBox");
				AssertEquals("Text should be initialized based on SectionNameIsOverridden and contain DefaultSectionName when SectionNameOverridden is set to false", string.Empty, overrideBox.Text);
			}

			config.SectionNameIsOverridden = true;
			config.SectionNameOverride = "I'll override you!";

			Factory.Save();

			using (var form = new BMBoardForm(board))
			{
				form.Show();
				Application.DoEvents();

				var overrideBox = form.FindSingle<ZOverridableTextBox>(x => x.Name == "OverriddenSectionNameCheckedTextBox");
				AssertEquals("Text should be initialized based on SectionNameIsOverridden and contain SectionNameOverride when SectionNameOverridden is set to true", "I'll override you!", overrideBox.Text);
			}
		}

		public void TestPanelGridCheckboxes_AreUnsubscribingFromMouseUpEvents_WhenSwitchingBetweenModSectionsInSectionGrid()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var board = BMSTestHelper.CreateBoard(system, "Boardie");

			var section1 = BMSTestHelper.CreateBoardSection(ModuleIDs.ProcessHeader, board);
			var config1 = (ModuleGridSectionConfiguration)section1.Configuration;
			var panelConfig1 = config1.PanelConfigurations.First() as ModuleGridSectionPanelConfiguration;

			var section2 = BMSTestHelper.CreateBoardSection(ModuleIDs.ProcessTasks, board);
			var config2 = (ModuleGridSectionConfiguration)section2.Configuration;
			var panelConfig2 = config2.PanelConfigurations.First() as ModuleGridSectionPanelConfiguration;

			Factory.Save();

			int countTrue = 0, countFalse = 0;
			panelConfig1.HasChangesChanged += (object sender, HasChangesChangedEventArgs e) =>
			{
				if ((sender as ModuleGridSectionPanelConfiguration).SectionNameIsOverridden)
				{
					++countTrue;
				}
				else
				{
					++countFalse;
				}
			};

			using (var form = new BMBoardForm(board))
			{
				form.Show();

				var sectionGrid = form.FindSingle<ZGrid>(g => g.Name == "BoardSectionsGrid");
				var panelGrid = form.FindSingle<ZGrid>(g => g.Name == "PanelGrid");
				var columnStyle = (ZCheckBoxColumnStyle)panelGrid.Columns.Single(c => c.ColumnName == "AllowFilterEdit").ColumnStyle;

				CombineAssertions(() =>
				{
					AssertEquals(0, countTrue);
					AssertEquals(0, countFalse);
				});

				ManipulateSectionAndPanelGrids(sectionGrid, panelGrid, 0);
				CombineAssertions(() =>
				{
					AssertEquals(2, countTrue);
					AssertEquals(0, countFalse);
				});

				ManipulateSectionAndPanelGrids(sectionGrid, panelGrid, 1);
				CombineAssertions(() =>
				{
					AssertEquals(2, countTrue);
					AssertEquals(0, countFalse);
				});

				ManipulateSectionAndPanelGrids(sectionGrid, panelGrid, 0);
				CombineAssertions(() =>
				{
					AssertEquals(2, countTrue);
					AssertEquals(2, countFalse);
				});
			}
		}

		void ManipulateSectionAndPanelGrids(ZGrid sectionGrid, ZGrid panelGrid, int sectionGridRow)
		{
			sectionGrid.UnSelectAll();
			Application.DoEvents();
			sectionGrid.Select(sectionGridRow);
			Application.DoEvents();
			sectionGrid.PerformMouseDownForTest(sectionGridRow, 1);
			Application.DoEvents();
			panelGrid.Focus();
			Application.DoEvents();
			panelGrid.CurrentCell = new DataGridCell(0, 2);
			var pos = panelGrid.GetCellBounds(0, 2).Location;
			panelGrid.PerformMouseUpForTest(new MouseEventArgs(MouseButtons.Left, 1, pos.X + 10, pos.Y + 10, 0), column: 2);
			Application.DoEvents();
		}

		#endregion

		#region Read-Only

		public void TestViewVisualBoardButton_ForAllFormModes_ShouldStillBeEnabled()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);

			CombineAssertions(() =>
			{
				foreach (ODisplayMode mode in Enum.GetValues(typeof(ODisplayMode)))
				{
					using (var form = new BMBoardForm(config.BucketBoard))
					{
						form.DisplayMode = mode;
						form.Show();
						Application.DoEvents();

						var button = form.FindSingle<ZButton>("ViewVisualBoardButton");
						AssertEquals($"The View Board button should always be enabled, even in {mode} mode. SAD!", true, button.Enabled);
					}
				}
			});
		}

		#endregion

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();

			Factory.Save();

			var form = new BMBoardForm(board);
			form.Size = ControlDpiScalingHelper.NewScaledSize(1792, 768, true);
			return form;
		}

		public override void TestMinimumSizeNotTooBig()
		{
			Assert("1024 wide is too small. RJW approved 1080p for BMS.", true);
		}

		class BMBoardForm_ForTest : BMBoardForm
		{
			public BMBoardForm_ForTest(BMBoard board)
				: base(board)
			{
			}

			public override void UpdatePreviews()
			{
				UpdatePreviewsCount++;
				base.UpdatePreviews();
			}

			public int UpdatePreviewsCount;

			public void FireDeleteButton()
			{
				((IPostingButtonsProvider)this).CommandButtonPost.PerformClick();
			}
		}

		static VisualBoardPreviewUserControl GetSectionPreviewControl(BMBoardForm form)
		{
			return form.FindSingleOrDefault<VisualBoardPreviewUserControl>(c => c.Name == "BoardSectionPreviewControl");
		}

		#endregion
	}

	#region NonTransactionedTestCase

	class BMBoardFormNonTransactionedTest : NonTransactionedTestCase
	{
		public void TestViewBoard_WhenNotYetSaved_ShouldNotThrowExceptions()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var board = config.System.Boards.AddNew();
			board.MB_Name = "Dat Board";
			board.HasChanges = false;

			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = new BMBoardForm(board))
			{
				form.Show();

				((ZButton)form.Controls.Find("ViewVisualBoardButton", true)[0]).PerformClick();

				AssertEquals("You must save this form before trying to preview the Visual Board. Would you like to save now?", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			AssertEquals(true, board.IsInDatabase);

			Application.DoEvents();

			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var visualBoardForm = Application.OpenForms.OfType<VisualBoardForm>().Single())
			{
				visualBoardForm.Invoke(new Action(() => visualBoardForm.DialogResult = DialogResult.OK));
			}

			Application.DoEvents();
			AssertEquals(0, Application.OpenForms.OfType<VisualBoardForm>().Count());
		}

		public void TestViewBoard_WhenNotYetSaved_CancelOptionChosen()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var board = config.System.Boards.AddNew();
			board.MB_Name = "Dat Board";
			board.HasChanges = false;

			using (var form = new BMBoardForm(board))
			{
				form.Show();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				((ZButton)form.Controls.Find("ViewVisualBoardButton", true)[0]).PerformClick();

				AssertEquals("You must save this form before trying to preview the Visual Board. Would you like to save now?", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			AssertEquals(false, board.IsInDatabase);

			var visualBoardForm = Application.OpenForms.OfType<VisualBoardForm>().SingleOrDefault();
			AssertNull(visualBoardForm);
		}

		public void TestViewBoard_ShouldTemporarilyDisable_WhenOpeningBoard()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var board = config.System.Boards.AddNew();
			board.MB_Name = "Dat Board";
			Factory.Save();

			using (VisualBoardsTestCase.ApplyAsyncStrategy(new TaskTrackingAsyncBaseStrategy()))
			using (var form = new BMBoardForm(board))
			{
				form.Show();
				var viewBoardButton = (ZButton)form.Controls.Find("ViewVisualBoardButton", true)[0];

				AssertEquals("Precondition", true, viewBoardButton.Enabled);
				AssertEquals("Precondition", "View Board", viewBoardButton.Text);

				viewBoardButton.PerformClick();
				var visualBoardForm = ZApplication.GetOpenForms().OfType<VisualBoardForm>().SingleOrDefault();

				AssertEquals("Opening...", viewBoardButton.Text);
				visualBoardForm.AwaitAll();

				AssertEquals(true, viewBoardButton.Enabled);
				AssertEquals("View Board", viewBoardButton.Text);

				AssertNotNull(visualBoardForm);

				visualBoardForm.Close();

				AssertEquals(true, viewBoardButton.Enabled);
				AssertEquals("View Board", viewBoardButton.Text);
			}
		}

		public void TestViewBoard_ShouldBeAbleToOpenAgain_AfterClosingOpenedBoard()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var board = config.System.Boards.AddNew();
			board.MB_Name = "Dat Board";
			Factory.Save();

			using (VisualBoardsTestCase.ApplyAsyncStrategy(new TaskTrackingAsyncBaseStrategy()))
			using (var form = new BMBoardForm(board))
			{
				form.Show();
				var viewBoardButton = (ZButton)form.Controls.Find("ViewVisualBoardButton", true)[0];

				viewBoardButton.PerformClick();

				var visualBoardForm = ZApplication.GetOpenForms().OfType<VisualBoardForm>().SingleOrDefault();
				AssertNotNull(visualBoardForm);
				visualBoardForm.AwaitAll();

				visualBoardForm.Close();
				Application.DoEvents();

				AssertEquals("Precondition", true, viewBoardButton.Enabled);
				AssertEquals("Precondition", "View Board", viewBoardButton.Text);
				viewBoardButton.PerformClick();
				visualBoardForm = ZApplication.GetOpenForms().OfType<VisualBoardForm>().SingleOrDefault();
				AssertEquals("Opening...", viewBoardButton.Text);
				visualBoardForm.AwaitAll();
				AssertEquals(true, viewBoardButton.Enabled);
				AssertEquals("View Board", viewBoardButton.Text);

				AssertNotNull(visualBoardForm);

				visualBoardForm.Close();

				AssertEquals(true, viewBoardButton.Enabled);
				AssertEquals("View Board", viewBoardButton.Text);
			}
		}

		public void TestPreview_ShouldNotHitStatisticsView_SimpleOrDetailedMode()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var board = config.BucketBoard;
			AssertShouldNotHitStatisticsView_SimpleOrDetailedMode(board, nameof(EnabledState.Simple));
			AssertShouldNotHitStatisticsView_SimpleOrDetailedMode(board, nameof(EnabledState.Detailed));
		}

		void AssertShouldNotHitStatisticsView_SimpleOrDetailedMode(BMBoard board, string mode)
		{
			SystemDataRegistry.Instance.StatisticsCollectionEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mode);
			AssertEquals($"GIVEN Performance-stats mode = {mode}", mode, ObjectFactory.Get<ISystemDataRegistry>().StatisticsCollectionEnabled);

			Factory.Save();

			const string statisticsViewName = "vw_StatisticsMeasurementsIncludingChildren";
			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (TestConnection.TrackExecutedCommands(includeStackTrace: true))
			using (var form = new BMBoardForm(board))
			{
				form.Show();

				BMSTestCaseWithFactory.AssertExecutedCommandCountMatching("Showing the board layout in the preview pane should not hit the statistics view, because we don't care about how long it will take to refresh each section yet.", statisticsViewName, 0);

				form.FindAndClickButton("ViewVisualBoardButton");
				Application.DoEvents();

				using (var boardForm = Application.OpenForms.OfType<VisualBoardForm>().Single())
				{
					BMSTestCaseWithFactory.AssertExecutedCommandCountMatching("Now that the board has been opened outside preview, should hit the statistics view to get loading time estimates.", statisticsViewName, 1);
				}
			}
		}
	}

	#endregion
}
