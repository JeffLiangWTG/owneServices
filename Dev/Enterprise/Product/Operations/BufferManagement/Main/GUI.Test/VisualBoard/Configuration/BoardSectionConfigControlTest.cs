using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.GUI.Test.FilterRule;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	class BoardSectionConfigControlTest : BMSTestCaseWithFactory
	{
		public void TestReadOnly_ShouldDisableAddChannelButton()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection);

			Factory.Save();

			var controller = ZControllerFactory.Instance.GetControllerForBizo(config.BufferBoard);
			AssertAddButtonEnabled("The form was opened normally, so the form isn't readonly and the button should be enabled, and yet...", () => controller.ShowEditForm(config.BufferBoard), true);
			AssertAddButtonEnabled("The form was opened in view mode, so the form should be readonly and the button should be disabled, and yet...", () => controller.ShowViewForm(config.BufferBoard), false);
		}

		static void AssertAddButtonEnabled(string message, Func<IZForm> showBoardFunc, bool shouldBeEnabled)
		{
			using (var form = (Form)showBoardFunc())
			{
				Application.DoEvents();
				var tab = form.FindSingle<ZTabPage>(x => x.Text == "Primary Channels");
				((ZTabControl)tab.Parent).SelectTab(tab);
				Application.DoEvents();

				var button = tab.FindSingle<ZButton>(x => x.Text == "Add");
				AssertEquals(message, shouldBeEnabled, button.Enabled);
			}
		}

		public void TestLoad_ShouldSetChannelsViewModel()
		{
			var board = Factory.New<BMSystem>().Boards.AddNew();
			var section = board.Sections.AddNew();

			using (var form = new ZForm())
			using (var control = new BoardSectionConfigControl())
			{
				form.Controls.Add(control);
				control.SetDataBinding(board, string.Empty);
				form.Show();
				Application.DoEvents();

				var primaryChannelsControl = (ChannelAssignmentControl)control.Controls.Find("PrimaryChannelsControl", true).First();
				var secondaryChannelsControl = (ChannelAssignmentControl)control.Controls.Find("SecondaryChannelsControl", true).First();

				AssertNotNull(primaryChannelsControl.ViewModel);
				AssertNotNull(secondaryChannelsControl.ViewModel);
			}
		}

		public void TestAddChannel_ShouldNotFailWhenNoSectionSelected()
		{
			var board = Factory.New<BMSystem>().Boards.AddNew();
			board.Sections.DeleteAll();

			using (var form = new ZForm())
			using (var control = new BoardSectionConfigControl())
			{
				form.Controls.Add(control);
				control.SetDataBinding(board, string.Empty);
				form.Show();
				Application.DoEvents();

				var primaryChannelsControl = (ChannelAssignmentControl)control.Controls.Find("PrimaryChannelsControl", true).First();
				var addChannelButton = (ZButton)primaryChannelsControl.Controls.Find("AddChannelButton", true).First();

				AssertNoExceptionThrown(() => addChannelButton.PerformClick());
			}
		}

		public void TestAllSectionConfigurationsBindToConfiguration()
		{
			var system = CreateSystem("ORG");
			var board = system.Boards.AddNew();
			var section = CreateBoardSection(CreateBucket(system), board);

			using (var form = new ZForm())
			using (var control = new BoardSectionConfigControl())
			{
				form.Controls.Add(control);
				control.SetDataBinding(board, string.Empty);
				form.Show();
				Application.DoEvents();

				var configurationControl = (ZUserControl)control.Find(c => c.Name == "configurationControl").SingleOrDefault();

				AssertNotNull(configurationControl);

				AssertEquals(string.Empty, configurationControl.BindingSource.DataMember);
				AssertType<BMComponentSectionConfiguration>(configurationControl.BindingSource.Current);
			}
		}

		public void TestAddNewSectionConfig_SholdSelectFirstTab()
		{
			var system = CreateSystem("ORG");
			var board = system.Boards.AddNew();

			using (var form = new ZForm())
			using (var control = new BoardSectionConfigControl())
			{
				form.Controls.Add(control);
				control.SetDataBinding(board, string.Empty);
				form.Show();

				Application.DoEvents();

				control.BoardSectionsGrid.ListManager.AddNew();
				var componentSectionConfig = control.BoardSectionsGrid.GetCurrent() as BMBoardSection;

				AssertType<BMComponentSectionConfiguration>(componentSectionConfig.Configuration);
				AssertEquals(0, control.SectionConfigTabControl.SelectedIndex);

				control.SectionConfigTabControl.SelectedIndex = 1;
				control.BoardSectionsGrid.ListManager.AddNew();
				var webSectionConfig = control.BoardSectionsGrid.GetCurrent() as BMBoardSection;

				AssertEquals(0, control.SectionConfigTabControl.SelectedIndex);
			}
		}

		public void TestDefaultSectionConfig_ShouldLoadSectionControlsAndTabPages()
		{
			var system = CreateSystem("ORG");
			var board = system.Boards.AddNew();
			var section = CreateBoardSection(CreateBucket(system), board);

			using (var form = new ZForm())
			using (var control = new BoardSectionConfigControl())
			{
				form.Controls.Add(control);
				control.SetDataBinding(board, string.Empty);
				form.Show();
				Application.DoEvents();

				var tabControl = control.FindAll<ZTabControl>().Single();
				AssertEquals(9, tabControl.TabPages.Count);

				var tabIndex = 0;
				AssertNotNull(tabControl.TabPages[tabIndex++].FindAll<ComponentConfigurationControl>().SingleOrDefault());
				AssertNotNull(tabControl.TabPages[tabIndex++].FindAll<FilterTabPageControl>().SingleOrDefault());
				AssertNotNull(tabControl.TabPages[tabIndex++].FindAll<FilterTabPageControl>().SingleOrDefault());
				AssertNotNull(tabControl.TabPages[tabIndex++].FindAll<ColorsTabPageControl>().SingleOrDefault());
				AssertNotNull(tabControl.TabPages[tabIndex++].FindAll<PrimaryChannelsTabPageControl>().SingleOrDefault());
				AssertNotNull(tabControl.TabPages[tabIndex++].FindAll<SecondaryChannelsTabPageControl>().SingleOrDefault());
				AssertNotNull(tabControl.TabPages[tabIndex++].FindAll<AdditionalComponentsTabPageControl>().SingleOrDefault());
				AssertNotNull(tabControl.TabPages[tabIndex++].FindAll<AcceptabilityBandsTabPageControl>().SingleOrDefault());
				AssertNotNull(tabControl.TabPages[tabIndex++].FindAll<CustomisedLayoutsSectionConfigControl>().SingleOrDefault());
			}
		}

		public void TestUpdatesSectionNameColumn_WhenConfigIsUpdated()
		{
			var system = CreateSystem("ORG");
			var board = system.Boards.AddNew();
			var section = CreateBoardSection(CreateBucket(system), board);
			var config = (BMComponentSectionConfiguration)section.Configuration;

			using (var form = new ZForm())
			using (var control = new BoardSectionConfigControl())
			{
				form.Controls.Add(control);
				control.SetDataBinding(board, string.Empty);
				form.Show();
				Application.DoEvents();

				var bmboard = control.BoardSectionsGrid.GetCurrent() as BMBoardSection;
				config.SectionNameIsOverridden = true;
				config.SectionNameOverride = "Overridden Name";
				AssertEquals("Overridden Name", bmboard.SectionName);
			}
		}

		public void TestCustomConfig_ChangingSectionTypeShouldUpdateControls()
		{
			var descriptor1 = new DummyDescriptorWithTwoTabPages { Type = "DAT" };
			var descriptor2 = new DummyDescriptorWithOneTabPage { Type = "DIS" };

			using (ObjectFactory.Substitute("VisualBoardSectionDescriptors", new ArrayList { descriptor1, descriptor2 }))
			{
				var system = CreateSystem("ORG");
				var board = system.Boards.AddNew();
				var section = CreateBoardSection(CreateBucket(system), board);
				section.MS_SectionType = "DAT";

				using (var form = new ZForm())
				using (var control = new BoardSectionConfigControl())
				{
					form.Controls.Add(control);
					control.SetDataBinding(board, string.Empty);
					form.Show();
					Application.DoEvents();

					var configPanel = control.Controls.Find("ConfigurationControlPanel", true).Single();
					AssertEquals(1, configPanel.Controls.Count);
					AssertType<ZUserControl>(configPanel.Controls[0]);
					AssertColorEquals(Color.HotPink, configPanel.Controls[0].BackColor);

					var tabControl = control.FindAll<ZTabControl>().Single();
					AssertEquals(3, tabControl.TabPages.Count);

					var tab1 = tabControl.TabPages[1];
					AssertEquals("DatTab", tab1.Name);
					AssertEquals("Dat Tab", tab1.Text);
					AssertEquals(1, tab1.Controls.Count);
					AssertType<ZLabel>(tab1.Controls[0]);
					AssertEquals("IAmA Label - AMA!~", tab1.Controls[0].Text);

					var tab2 = tabControl.TabPages[2];
					AssertEquals("DisTab", tab2.Name);
					AssertEquals("Dis Tab", tab2.Text);
					AssertEquals(1, tab2.Controls.Count);
					AssertType<ZTextBox>(tab2.Controls[0]);
					AssertEquals("Wibbly-Wobbly Timey-Wimey", tab2.Controls[0].Text);

					var configurationControl = (ZUserControl)configPanel.Find(c => c.Name == "configurationControl").SingleOrDefault();
					AssertNotNull(configurationControl);
					AssertEquals(string.Empty, configurationControl.BindingSource.DataMember);
					AssertType<AnotherDifferentDummyNonPersistentBusinessObject>(configurationControl.BindingSource.Current);

					section.MS_SectionType = "DIS";
					Application.DoEvents();

					AssertEquals(1, configPanel.Controls.Count);
					AssertType<ZUserControl>(configPanel.Controls[0]);
					AssertColorEquals(Color.Blue, configPanel.Controls[0].BackColor);

					AssertEquals(2, tabControl.TabPages.Count);

					tab1 = tabControl.TabPages[1];
					AssertEquals("AnotherTab", tab1.Name);
					AssertEquals("Another Tab", tab1.Text);
					AssertEquals(1, tab1.Controls.Count);
					AssertType<ZLabel>(tab1.Controls[0]);
					AssertEquals("Stop trying to make tabs happen. They're not going to happen.", tab1.Controls[0].Text);

					configurationControl = (ZUserControl)configPanel.Find(c => c.Name == "configurationControl").SingleOrDefault();
					AssertNotNull(configurationControl);
					AssertEquals(string.Empty, configurationControl.BindingSource.DataMember);
					AssertType<DifferentDummyNonPersistentBusinessObject>(configurationControl.BindingSource.Current);
				}
			}
		}

		public void TestChangingSectionTypeDeletesOldConfigurationAndCreatesNewOne()
		{
			var descriptor1 = new DummyDescriptorWithTwoTabPages { Type = "DAT" };
			var descriptor2 = new DummyDescriptorWithOneTabPage { Type = "DIS" };

			using (ObjectFactory.Substitute("VisualBoardSectionDescriptors", new ArrayList { descriptor1, descriptor2 }))
			{
				var system = BMSTestHelper.CreateSystem(Factory);
				var board = system.Boards.AddNew();
				var section = board.Sections.AddNew();
				section.MS_SectionType = "DAT";
				var descriptor = SectionDescriptorProvider.Get(section.MS_SectionType);

				var oldConfig = section.Configuration;
				AssertNotNull(oldConfig);
				AssertType<AnotherDifferentDummyNonPersistentBusinessObject>(oldConfig);

				var oldConfigPK = oldConfig.Identifier;

				section.MS_SectionType = "DIS";

				AssertEquals(true, ((BusinessObject)oldConfig).IsDeleted);

				var newConfig = section.Configuration;

				AssertNotEquals(oldConfigPK, newConfig.Identifier);
				AssertEquals(false, ((BusinessObject)newConfig).IsDeleted);
				AssertType<DifferentDummyNonPersistentBusinessObject>(newConfig);
			}
		}

		[ExpectNoExceptions]
		public void TestBindingShouldNotFailWithDeletedSection()
		{
			var descriptor1 = new DummyDescriptorWithTwoTabPages { Type = "DAT" };

			using (ObjectFactory.Substitute("VisualBoardSectionDescriptors", new ArrayList { descriptor1 }))
			{
				var system = CreateSystem("ORG");
				var board = system.Boards.AddNew();
				var bucket = CreateBucket(system);
				var section = CreateBoardSection(bucket, board);
				section.MS_SectionType = "DAT";

				using (var form = new ZForm())
				using (var control = new BoardSectionConfigControl())
				{
					form.Controls.Add(control);
					control.SetDataBinding(board, string.Empty);
					form.Show();
					Application.DoEvents();

					var configPanel = control.Controls.Find("ConfigurationControlPanel", true).Single();
					AssertEquals(1, configPanel.Controls.Count);
					AssertType<ZUserControl>(configPanel.Controls[0]);
					AssertColorEquals(Color.HotPink, configPanel.Controls[0].BackColor);

					var tabControl = control.FindAll<ZTabControl>().Single();
					AssertEquals(3, tabControl.TabPages.Count);

					var section2 = CreateBoardSection(bucket, board);
					section.MS_SectionType = "DAT";

					control.BoardSectionsGrid.ListManager.Position = 1;

					Application.DoEvents();

					control.BoardSectionsGrid.ListManager.Position = 0;

					section.Delete();
					Application.DoEvents();

					control.BoardSectionsGrid.ListManager.Position = 1;
					Application.DoEvents();
				}
			}
		}

		public void TestBindingShouldNotFail_WhenRearangeSections()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var board = BMSTestHelper.CreateBoard(system);
			var buffer = BMSTestHelper.CreateBuffer(system);

			var moduleSection = board.Sections.AddNew();
			moduleSection.MS_SectionType = BMConstants.ModuleGridSectionType;
			moduleSection.Row = 0;

			var componentSection = board.Sections.AddNew();
			componentSection.MS_FC_Component = buffer.PK;
			componentSection.Row = 1;

			Factory.Save();

			using (var form = new BMBoardForm(board))
			{
				form.Show();
				Application.DoEvents();

				for (int i = 0; i < 3; i++)
				{
					var boardSectionGrid = form.FindSingle<ZGrid>("BoardSectionsGrid");

					var sectionControl = form.FindSingle<BoardSectionConfigControl>();
					var rowCalcEdit = sectionControl.FindSingle<ZCalcEdit>("RowCalcEdit");

					var configurationControl = sectionControl.FindSingle<ZUserControl>("configurationControl");

					rowCalcEdit.Text = "1";

					boardSectionGrid.ListManager.Position = 1;

					Application.DoEvents();

					sectionControl = form.FindSingle<BoardSectionConfigControl>();
					rowCalcEdit = sectionControl.FindSingle<ZCalcEdit>("RowCalcEdit");
					rowCalcEdit.Text = "0";

					boardSectionGrid.ListManager.Position = 0;
					Application.DoEvents();

					AssertNoExceptionThrown(() => form.FireSaveButton());
				}
			}

			board = Factory.Load<BMBoard>(board.PK);
			moduleSection = board.Sections.First(s => s.MS_SectionType == BMConstants.ModuleGridSectionType);
			componentSection = board.Sections.First(s => s.MS_SectionType == BMConstants.ComponentSectionType);

			AssertEquals("Module Section row should be 1", 1, moduleSection.Row);
			AssertEquals("Component Section row should be 0", 0, componentSection.Row);
		}

		public void TestFilterStripControl_WorkflowFilters_Preview()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var board = system.Boards.AddNew();
			var bucket1 = BMSTestHelper.CreateBucket(system, "Bucket 1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "Bucket 2");
			var section1 = CreateBoardSection(bucket1, board);
			var section2 = CreateBoardSection(bucket2, board);

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = BMSTestHelper.CreateJobHeader(orgHeader);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Shalala bucket1", bucket1);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Uslurp bucket1", bucket1);
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "Shalala bucket2", bucket2);
			var workflow4 = BMSTestHelper.CreateWorkflow(jobHeader, "Uslurp bucket2", bucket2);

			Factory.Save();

			using (var form = new BMBoardForm(board))
			{
				form.Show();
				var sectionControl = form.FindAll<BoardSectionConfigControl>().Single();
				Application.DoEvents();
				var filterTabControl = (ZTabPage)sectionControl.Controls.Find("WorkflowFilterTabControl", true).Single();
				sectionControl.SectionConfigTabControl.SelectTab(filterTabControl);
				var filterControl = filterTabControl.FindAll<BMFilterStripWrapperControl>().Single();
				AssertEquals(true, filterControl.IsPreviewAllowed);

				var stripControl = filterControl.FindAll<FilterRuleFilterStripControl>().Single();
				var strip = stripControl.AddNewFilterStrip();
				strip.CurrentDataItem.FilterDescription = "Completion Statement";
				((ModuleTextFilter)strip.CurrentDataItem.CurrentModuleFilter).Property = "Shalala";

				BusinessObject[] result = null;
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((o) =>
				{
					var popupForm = o as EmbeddedModulePopup;
					if (popupForm != null)
					{
						popupForm.Closing += (s, e) =>
						{
							result = popupForm.Module_ForTest.GridCollection.ToArray();
						};
					}
				});

				var toolStrip = filterControl.FindAll<ZToolStrip>().Single(x => x.Name == "ToolStrip");
				var previewButton = toolStrip.Items["ToolStripPreviewDropButton"];
				previewButton.PerformClick();

				AssertContainsExactElementsInAnyOrder("The result should reflect the added filter strip AND the auto added component-related filter strip, and yet...",
					new[] { workflow1.FH_CompletionStatement }, result.Cast<ProcessHeader>().Select(x => x.FH_CompletionStatement));

				sectionControl.BoardSectionsGrid.CurrentRowIndex = 1;
				Application.DoEvents();
				filterTabControl = (ZTabPage)sectionControl.Controls.Find("WorkflowFilterTabControl", true).Single();
				filterControl = filterTabControl.FindAll<BMFilterStripWrapperControl>().Single();
				sectionControl.SectionConfigTabControl.SelectTab(filterTabControl);

				toolStrip = filterControl.FindAll<ZToolStrip>().Single(x => x.Name == "ToolStrip");
				previewButton = toolStrip.Items["ToolStripPreviewDropButton"];
				previewButton.PerformClick();

				AssertContainsExactElementsInAnyOrder("Should have shown results for headers from the second bucket since that one was selected, and yet...",
					new[] { workflow3.FH_CompletionStatement, workflow4.FH_CompletionStatement }, result.Cast<ProcessHeader>().Select(x => x.FH_CompletionStatement));

				ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogsAndClearStackForTest();
			}
		}

		public void TestFilterStripControl_WorkflowFilters_Preview_ShouldShowInAdditionalCompoent()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var board = system.Boards.AddNew();
			var mainBuffer = BMSTestHelper.CreateBuffer(system, "Main Buffer");
			var additionalBuffer = BMSTestHelper.CreateBuffer(system, "Additional Buffer");
			var section = CreateBoardSection(mainBuffer, board);
			BMSTestHelper.CreateAdditionalComponent(section, additionalBuffer);

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = BMSTestHelper.CreateJobHeader(orgHeader, false);
			var workflowInMainBuffer = BMSTestHelper.CreateWorkflow(jobHeader, nameof(mainBuffer), mainBuffer);
			var workflowInAdditionalBuffer = BMSTestHelper.CreateWorkflow(jobHeader, nameof(additionalBuffer), additionalBuffer);

			Factory.Save();

			using (var form = new BMBoardForm(board))
			{
				form.Show();
				var sectionControl = form.FindSingle<BoardSectionConfigControl>();
				Application.DoEvents();
				var filterTabControl = (ZTabPage)sectionControl.Controls.Find("WorkflowFilterTabControl", true).Single();
				sectionControl.SectionConfigTabControl.SelectTab(filterTabControl);
				var filterControl = filterTabControl.FindSingle<BMFilterStripWrapperControl>();
				AssertEquals(true, filterControl.IsPreviewAllowed);

				BusinessObject[] result = null;
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((o) =>
				{
					if (o is EmbeddedModulePopup popupForm)
					{
						popupForm.Closing += (s, e) =>
						{
							result = popupForm.Module_ForTest.GridCollection.ToArray();
						};
					}
				});

				var toolStrip = filterControl.FindSingle<ZToolStrip>("ToolStrip");
				var previewButton = toolStrip.Items["ToolStripPreviewDropButton"];
				previewButton.PerformClick();

				AssertContainsExactElementsInAnyOrder("The result should reflect the added filter strip",
					new[] { workflowInMainBuffer.FH_CompletionStatement, workflowInAdditionalBuffer.FH_CompletionStatement }, result.Cast<ProcessHeader>().Select(x => x.FH_CompletionStatement));

				ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogsAndClearStackForTest();
			}
		}

		public void TestFilterStripControl_TaskFilters_Preview()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var board = system.Boards.AddNew();
			var bucket1 = BMSTestHelper.CreateBucket(system, "Bucket 1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "Bucket 2");
			var section1 = CreateBoardSection(bucket1, board);

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = BMSTestHelper.CreateJobHeader(orgHeader);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Shalala bucket1", bucket1);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Uslurp bucket1", bucket1);
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "Shalala bucket2", bucket2);

			var task1 = BMSTestHelper.CreateTask(workflow1, taskType: "ABC", description: "You can't see me... I'm wearing the hat.");
			var task2 = BMSTestHelper.CreateTask(workflow1, taskType: "DEF", description: "All shall tremble before my visibility.");
			var task3 = BMSTestHelper.CreateTask(workflow2, taskType: "ABC", description: "My workflow shall not be seen.");
			var task4 = BMSTestHelper.CreateTask(workflow2, taskType: "DEF", description: "Mine either.");
			var task5 = BMSTestHelper.CreateTask(workflow3, taskType: "DEF", description: "I'm in the wrong component :(");

			Factory.Save();

			using (var form = new BMBoardForm(board))
			{
				form.Show();
				var sectionControl = form.FindAll<BoardSectionConfigControl>().Single();
				Application.DoEvents();

				var taskFilterTabControl = (ZTabPage)sectionControl.Controls.Find("TaskFilterTabControl", true).Single();
				sectionControl.SectionConfigTabControl.SelectTab(taskFilterTabControl);
				var taskFilterControl = taskFilterTabControl.FindAll<BMFilterStripWrapperControl>().Single();
				AssertEquals(true, taskFilterControl.IsPreviewAllowed);

				var taskStripControl = taskFilterControl.FindAll<FilterRuleFilterStripControl>().Single();
				var taskStrip = taskStripControl.AddNewFilterStrip();
				taskStrip.CurrentDataItem.FilterDescription = "Task Type";
				((ModuleTextFilter)taskStrip.CurrentDataItem.CurrentModuleFilter).Property = "DEF";

				BusinessObject[] result = null;
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((o) =>
				{
					var popupForm = o as EmbeddedModulePopup;
					if (popupForm != null)
					{
						popupForm.Closing += (s, e) =>
						{
							result = popupForm.Module_ForTest.GridCollection.ToArray();
						};
					}
				});

				var toolStrip = taskFilterControl.FindAll<ZToolStrip>().Single(x => x.Name == "ToolStrip");
				var previewButton = toolStrip.Items["ToolStripPreviewDropButton"];
				previewButton.PerformClick();

				AssertContainsExactElementsInAnyOrder("Should have selected all DEF tasks in both workflows because no workflow filters have been set yet, and yet...",
					new[] { task2.P9_Description, task4.P9_Description }, result.Cast<ProcessTask>().Select(x => x.P9_Description));

				var workflowFilterTabControl = (ZTabPage)sectionControl.Controls.Find("WorkflowFilterTabControl", true).Single();
				sectionControl.SectionConfigTabControl.SelectTab(workflowFilterTabControl);
				var workflowFilterControl = workflowFilterTabControl.FindAll<BMFilterStripWrapperControl>().Single();
				AssertEquals(true, workflowFilterControl.IsPreviewAllowed);

				var workflowStripControl = workflowFilterControl.FindAll<FilterRuleFilterStripControl>().Single();
				var workflowStrip = workflowStripControl.AddNewFilterStrip();
				workflowStrip.CurrentDataItem.FilterDescription = "Completion Statement";
				((ModuleTextFilter)workflowStrip.CurrentDataItem.CurrentModuleFilter).Property = "Shalala";

				result = null;
				previewButton.PerformClick();

				AssertContainsExactElementsInAnyOrder("Should only show matching tasks that are also in the matching workflows from the workflow filters, and yet...",
					new[] { task2.P9_Description }, result.Cast<ProcessTask>().Select(x => x.P9_Description));

				ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogsAndClearStackForTest();
			}
		}

		[ExpectNoExceptions]
		public void TestPreviewBufferBoardInConstrainedMode_WithShortBufferTimespanAndLargeCellsPerSubsectionCount_ShouldNotThrowException()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system, timespanMinutes: 12);
			BMSTestHelper.CreateSubBuffer(buffer, "Pre-Constraint", timespanMinutes: 6, offsetMinutes: 0);
			BMSTestHelper.CreateConstraint(buffer, "Constraint", offsetMinutes: 6);
			BMSTestHelper.CreateSubBuffer(buffer, "Post-Constraint", timespanMinutes: 6, offsetMinutes: 6);

			var group = BMSecurityTestHelper.CreateGroup(Factory, "GAA");
			BMSTestHelper.CreateReleaseGroup(system, group, constrainedModeComponent: buffer);

			var section = BMSTestHelper.CreateBoardSection(buffer);
			section.SectionConfiguration.CellsPerSubsection = 30;
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			Factory.Save();

			using (var form = new BMBoardForm(section.Board))
			{
				form.Show();
				Application.DoEvents();
			}
		}

		public void TestPreview_ShowWorkInReleaseGroupOnlyIsSelected_ShouldFilterByReleaseGroup()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var board = system.Boards.AddNew();
			var bucket1 = BMSTestHelper.CreateBucket(system, "Bucket 1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "Bucket 2");
			var section = CreateBoardSection(bucket1, board);

			var releaseGroup1 = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup2 = Factory.NewWithValidTestData<GlbGroup>();
			section.SectionConfiguration.ReleaseGroupPK = releaseGroup1.PK;
			section.SectionConfiguration.ShowWorkInReleaseGroupOnly = false;

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = BMSTestHelper.CreateJobHeader(orgHeader);
			var jobHeader2 = BMSTestHelper.CreateJobHeader(orgHeader2);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Shalala bucket1", bucket1, releaseGroupPK: releaseGroup1.PK);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Uslurp bucket1", bucket1, releaseGroupPK: releaseGroup1.PK);
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader2, "Shalala bucket1", bucket1, releaseGroupPK: releaseGroup2.PK);
			var workflow4 = BMSTestHelper.CreateWorkflow(jobHeader2, "Uslurp bucket1", bucket1, releaseGroupPK: releaseGroup2.PK);

			var task1 = BMSTestHelper.CreateTask(workflow1, taskType: "ABC", description: "You can't see me... I'm wearing the hat.");
			var task2 = BMSTestHelper.CreateTask(workflow1, taskType: "DEF", description: "All shall tremble before my visibility.");
			var task3 = BMSTestHelper.CreateTask(workflow2, taskType: "ABC", description: "My workflow shall not be seen.");
			var task4 = BMSTestHelper.CreateTask(workflow2, taskType: "DEF", description: "Mine either.");
			var task5 = BMSTestHelper.CreateTask(workflow3, taskType: "ABC", description: "You can't see me... I'm wearing the hat. OTHER RG");
			var task6 = BMSTestHelper.CreateTask(workflow3, taskType: "DEF", description: "All shall tremble before my visibility. OTHER RG");
			var task7 = BMSTestHelper.CreateTask(workflow4, taskType: "ABC", description: "My workflow shall not be seen. OTHER RG");
			var task8 = BMSTestHelper.CreateTask(workflow4, taskType: "DEF", description: "Mine either. OTHER RG");

			Factory.Save();

			using (var form = new BMBoardForm(board))
			{
				form.Show();
				var sectionControl = form.FindAll<BoardSectionConfigControl>().Single();
				Application.DoEvents();

				var taskFilterTabControl = (ZTabPage)sectionControl.Controls.Find("TaskFilterTabControl", true).Single();
				sectionControl.SectionConfigTabControl.SelectTab(taskFilterTabControl);
				var taskFilterControl = taskFilterTabControl.FindAll<BMFilterStripWrapperControl>().Single();
				AssertEquals(true, taskFilterControl.IsPreviewAllowed);

				var taskStripControl = taskFilterControl.FindAll<FilterRuleFilterStripControl>().Single();
				var taskStrip = taskStripControl.AddNewFilterStrip();
				taskStrip.CurrentDataItem.FilterDescription = "Task Type";
				((ModuleTextFilter)taskStrip.CurrentDataItem.CurrentModuleFilter).Property = "DEF";

				BusinessObject[] result = null;
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((o) =>
				{
					var popupForm = o as EmbeddedModulePopup;
					if (popupForm != null)
					{
						popupForm.Closing += (s, e) =>
						{
							result = popupForm.Module_ForTest.GridCollection.ToArray();
						};
					}
				});

				var toolStrip = taskFilterControl.FindAll<ZToolStrip>().Single(x => x.Name == "ToolStrip");
				var previewButton = toolStrip.Items["ToolStripPreviewDropButton"];
				previewButton.PerformClick();

				AssertContainsExactElementsInAnyOrder("Should have selected all DEF tasks in both workflows because no workflow filters have been set yet, and yet...",
					new[] { task2.P9_Description, task4.P9_Description, task6.P9_Description, task8.P9_Description }, result.Cast<ProcessTask>().Select(x => x.P9_Description));

				var workflowFilterTabControl = (ZTabPage)sectionControl.Controls.Find("WorkflowFilterTabControl", true).Single();
				sectionControl.SectionConfigTabControl.SelectTab(workflowFilterTabControl);
				var workflowFilterControl = workflowFilterTabControl.FindAll<BMFilterStripWrapperControl>().Single();
				AssertEquals(true, workflowFilterControl.IsPreviewAllowed);

				var workflowStripControl = workflowFilterControl.FindAll<FilterRuleFilterStripControl>().Single();
				var workflowStrip = workflowStripControl.AddNewFilterStrip();
				workflowStrip.CurrentDataItem.FilterDescription = "Completion Statement";
				((ModuleTextFilter)workflowStrip.CurrentDataItem.CurrentModuleFilter).Property = "Shalala";

				result = null;
				previewButton.PerformClick();

				AssertContainsExactElementsInAnyOrder("Should only show matching tasks that are also in the matching workflows from the workflow filters, and yet...",
					new[] { task2.P9_Description, task6.P9_Description }, result.Cast<ProcessTask>().Select(x => x.P9_Description));

				section.SectionConfiguration.ShowWorkInReleaseGroupOnly = true;
				result = null;
				previewButton.PerformClick();

				AssertContainsExactElementsInAnyOrder("Should further filter by release group specified for the section, and yet...",
					new[] { task2.P9_Description }, result.Cast<ProcessTask>().Select(x => x.P9_Description));

				ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogsAndClearStackForTest();
			}
		}

		#region Clone Menu Item

		public void TestCloneBoardSection()
		{
			var board = Factory.New<BMSystem>().Boards.AddNew();
			var section = board.Sections.AddNew();

			var preCloneCountBusinessLayer = board.Sections.Count;

			using (var form = new ZForm())
			using (var control = new BoardSectionConfigControl())
			{
				form.Controls.Add(control);
				control.SetDataBinding(board, string.Empty);
				form.Show();
				control.BoardSectionsGrid.Show();
				Application.DoEvents();

				var contextMenu = control.BoardSectionsGrid.ContextMenu;
				AssertNotNull(contextMenu);

				var cloneSectionMenuItem = contextMenu.MenuItems.Find("clone", true).FirstOrDefault();
				AssertNotNull(cloneSectionMenuItem);
				control.BoardSectionsGrid.SelectSingleElement(section);
				var preCloneCount = control.BoardSectionsGrid.List.Count;
				cloneSectionMenuItem.PerformClick();
				var postCloneCount = control.BoardSectionsGrid.List.Count;
				var postCloneCountBusinessLayer = board.Sections.Count;
				AssertEquals(preCloneCount + 1, postCloneCount);
				AssertEquals(preCloneCountBusinessLayer + 1, postCloneCountBusinessLayer);
			}
		}

		#endregion

		#region Dummy Descriptors

		class DummyDescriptorWithTwoTabPages : IBoardSectionDescriptor
		{
			public string Type { get; set; }
			public string Description { get; set; }

			public IBoardSectionConfigurationBizo GetSectionConfigurationBizo(IBMBoardSection section)
			{
				return new AnotherDifferentDummyNonPersistentBusinessObject();
			}

			public object GetSectionConfigurationControl()
			{
				return new ZUserControl
				{
					BackColor = Color.HotPink,
				};
			}

			public IEnumerable<TabSpec> GetAdditionalTabs()
			{
				yield return new TabSpec
				{
					Name = "DatTab",
					Text = "Dat Tab",
					TabContentControl = new ZLabel
					{
						Text = "IAmA Label - AMA!~"
					},
				};
				yield return new TabSpec
				{
					Name = "DisTab",
					Text = "Dis Tab",
					TabContentControl = new ZTextBox
					{
						CharacterCasing = CharacterCasing.Normal,
						Text = "Wibbly-Wobbly Timey-Wimey"
					},
				};
			}

			public IBoardSectionControl GetSectionControl(IBMBoardSection section, BoardSectionViewModel sectionViewModel)
			{
				throw new NotImplementedException();
			}

			public BoardSectionViewModel GetViewModel(IBMBoardSection section, BoardViewModel boardViewModel)
			{
				throw new NotImplementedException();
			}
		}

		class DummyDescriptorWithOneTabPage : IBoardSectionDescriptor
		{
			public string Type { get; set; }
			public string Description { get; set; }

			public IBoardSectionConfigurationBizo GetSectionConfigurationBizo(IBMBoardSection section)
			{
				return new DifferentDummyNonPersistentBusinessObject();
			}

			public object GetSectionConfigurationControl()
			{
				return new ZUserControl
				{
					BackColor = Color.Blue,
				};
			}

			public IEnumerable<TabSpec> GetAdditionalTabs()
			{
				yield return new TabSpec
				{
					Name = "AnotherTab",
					Text = "Another Tab",
					TabContentControl = new ZLabel
					{
						Text = "Stop trying to make tabs happen. They're not going to happen."
					},
				};
			}

			public IBoardSectionControl GetSectionControl(IBMBoardSection section, BoardSectionViewModel sectionViewModel)
			{
				throw new NotImplementedException();
			}

			public BoardSectionViewModel GetViewModel(IBMBoardSection section, BoardViewModel boardViewModel)
			{
				throw new NotImplementedException();
			}
		}

		#endregion

		#region Filter Tab Page Tests

		public class BoardSectionWorkflowFilterTabPageControlTest : FilterTabPageControlTestCase
		{
			protected override string TabPageControlName => "WorkflowFilterTabControl";

			protected override string TabControlName => "SectionConfigTabControl";

			protected override BusinessObject GetBusinessObjectForBinding(VisualBoardTestConfig config)
			{
				return config.BufferBoard;
			}
		}

		public class BoardSectionTaskFilterTabPageControlTest : FilterTabPageControlTestCase
		{
			protected override string TabPageControlName => "TaskFilterTabControl";

			protected override string TabControlName => "SectionConfigTabControl";

			protected override BusinessObject GetBusinessObjectForBinding(VisualBoardTestConfig config)
			{
				return config.BufferBoard;
			}
		}

		class DifferentDummyNonPersistentBusinessObject : DummyBoardSectionConfigurationBizo
		{
		}

		class AnotherDifferentDummyNonPersistentBusinessObject : DummyBoardSectionConfigurationBizo
		{
		}

		#endregion
	}
}
