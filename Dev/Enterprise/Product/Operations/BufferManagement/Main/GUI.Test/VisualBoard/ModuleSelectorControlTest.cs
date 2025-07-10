using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Async;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.GUI;
using Enterprise.Accounting.GUI.ARAP.TransactionView;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Module;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.GUI;
using Enterprise.Registry.Business;
using Enterprise.UniversalCopy.Business;
using Enterprise.UniversalCopy.GUI;
using Enterprise.VisualBoards.Business;
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
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	class ModuleSelectorControlTest : BMSGUITestCase
	{
		#region Bashing

		public void TestBashModuleGrids()
		{
			var system = CreateSystem();
			var component = CreateBucket(system);
			var section = CreateBoardSection(component);
			section.MS_SectionType = BMConstants.ModuleGridSectionType;
			var config = (ModuleGridSectionConfiguration)section.Configuration;

			var panelConfig = new ModuleGridSectionPanelConfiguration(section);
			config.PanelConfigurations.Add(panelConfig);

			CombineAssertions(() =>
				{
					foreach (var moduleId in ModuleIDs.AllExcludingClientModules)
					{
						panelConfig.ModuleName = moduleId.Name;

						AssertNoExceptionThrown(string.Format("Expected no error for module: {0}", moduleId.Name),
							() =>
							{
								using (var form = new ZForm())
								using (var control = CreateModuleControl(config))
								{
									form.Controls.Add(control);
									form.Show();
									Application.DoEvents();
								}
							});
					}
				});
		}

		#endregion

		#region Ordinary Grid Functionality

		[ExpectNoExceptions]
		public void TestOpenUpJobWorkflowsAndClickAround()
		{
			var system = CreateSystem("ORG");
			var component = CreateBucket(system);
			var section = CreateBoardSection(component);
			section.MS_SectionType = BMConstants.ModuleGridSectionType;
			var config = (ModuleGridSectionConfiguration)section.Configuration;

			var panelConfig = new ModuleGridSectionPanelConfiguration(section);
			config.PanelConfigurations.Add(panelConfig);
			panelConfig.ModuleName = ModuleIDs.ProcessHeader.Name;

			var jobHeader = CreateJobHeader<OrgHeader>();

			Factory.Save();

			using (var form = new ZForm())
			using (var control = CreateModuleControl(config))
			{
				control.Dock = DockStyle.Fill;
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();

				var stripControl = control.FindAll<StripControl>().Single();
				var toolStrip = stripControl.FindAll<ZToolStrip>().Single(t => t.Name == "ToolStrip");
				var button = toolStrip.Items.OfType<ToolStripSplitButton>().Single(n => n.Name == "ToolStripFindDropButton");

				button.PerformClick();
				button.PerformButtonClick();

				Application.DoEvents();

				var grid = control.FindAll<ZGrid>().Single();
				AssertEquals(2, grid.VisibleRowCount);

				grid.Select(0);
				Application.DoEvents();

				grid.GetFirstSelectedRow();
				grid.Select(1);
				Application.DoEvents();
			}
		}

		public void TestClientSpecificModule_ShouldLoad()
		{
			using (OverrideClientAssemblyAndSecurityCheckpointsForTest(Clients.EDI))
			{
				var system = CreateSystem("ORG");
				var component = CreateBucket(system);
				var board = CreateBoard(system);
				var section = CreateBoardSection(component, board);
				section.MS_SectionType = BMConstants.ModuleGridSectionType;
				var config = (ModuleGridSectionConfiguration)section.Configuration;

				var panelConfig = new ModuleGridSectionPanelConfiguration(section);
				config.PanelConfigurations.Add(panelConfig);
				panelConfig.ModuleName = "SupportIncident";

				Factory.Save();

				using (var form = new ZForm())
				using (var control = CreateModuleControl(config))
				{
					form.Controls.Add(control);
					form.Show();
					Application.DoEvents();

					CombineAssertions(() =>
					{
						var grids = form.FindAll<ZGrid>();
						AssertEquals("The grid should have been loaded correctly, and yet...", 1, grids.Count());

						var labels = form.FindAll<ZLabel>(x => x.Text.StartsWith("Unable to load a grid for the module"));
						AssertEquals("There should be no 'unable to load' label, and yet...", 0, labels.Count());
					});
				}
			}
		}

		#endregion

		#region Filter Strips

		public void TestCurrentComponentFilter_WhenBoardIsNotWorkflowModeller_ShouldNotBeAutomaticallyAdded()
		{
			ZGuid layoutPk;

			using (var module = ZFilterModule.GetZFilterModule(ModuleIDs.ProcessHeader))
			{
				var filterBizo = module.FilterBusinessObject;
				filterBizo.AddTextFilterStrip("Completion Statement", "Dinglebop");
				layoutPk = filterBizo.SaveLayout("Grumbo").PK;
			}

			var system = CreateSystem("INQ");
			var board = CreateBoard(system);
			var section = BMSTestHelper.CreateBoardSection(ModuleIDs.ProcessHeader, board);
			var config = (ModuleGridSectionConfiguration)section.Configuration;

			var panelConfig = config.PanelConfigurations.First() as ModuleGridSectionPanelConfiguration;
			panelConfig.FilterLayout = layoutPk;

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(board))
			{
				var control = form.FindSingle<StripControl>();
				var filters = control.FilterBusinessObject.ActiveModuleFilters.Select(x => x.Description);

				AssertContainsExactElementsInAnyOrder("A Current Component filter should not have been added. SAD!", new[] { "Completion Statement" }, filters);
			}
		}

		public void TestCurrentComponentFilter_WhenBoardIsNotWorkflowModeller_ShouldNotBeAltered()
		{
			ZGuid layoutPk;
			var system = CreateSystem("INQ");
			var component = BMSTestHelper.CreateBucket(system);
			Factory.Save();

			using (var module = ZFilterModule.GetZFilterModule(ModuleIDs.ProcessHeader))
			{
				var filterBizo = module.FilterBusinessObject;
				filterBizo.AddTextFilterStrip("Completion Statement", "Fleeb");
				var componentFilter = filterBizo.AddGuidFilterStrip("Current Component", component.PK);
				componentFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
				layoutPk = filterBizo.SaveLayout("Hizzards").PK;
			}

			var board = CreateBoard(system);
			var section = BMSTestHelper.CreateBoardSection(ModuleIDs.ProcessHeader, board);
			var config = (ModuleGridSectionConfiguration)section.Configuration;

			var panelConfig = config.PanelConfigurations.First() as ModuleGridSectionPanelConfiguration;
			panelConfig.FilterLayout = layoutPk;

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(board))
			{
				var control = form.FindSingle<StripControl>();
				var filters = control.FilterBusinessObject.ActiveModuleFilters.Select(x => x.Description);

				AssertContainsExactElementsInAnyOrder(new[] { "Completion Statement", "Current Component" }, filters);

				var componentFilter = (ModuleGuidFilter)control.FilterBusinessObject.ActiveModuleFilters.Single(x => x.Description == "Current Component");
				AssertEquals("The values on the component filter should not have been altered. SAD!", ModuleTextFilter.ComparisonConstants.NotEqual, componentFilter.ComparisonOperator);
				AssertEquals("The values on the component filter should not have been altered. SAD!", component.PK, componentFilter.Property);
			}
		}

		public void TestBufferManagementComponentFilter_WhenBoardIsNotWorkflowModeller_ShouldNotBeAutomaticallyAdded()
		{
			ZGuid layoutPk;

			using (var module = ZFilterModule.GetZFilterModule(ModuleIDs.SalesEnquiry))
			{
				var filterBizo = module.FilterBusinessObject;
				filterBizo.AddTextFilterStrip("City", "Shlamie");
				layoutPk = filterBizo.SaveLayout("Schleem").PK;
			}

			var system = CreateSystem("INQ");
			var board = CreateBoard(system);
			var section = BMSTestHelper.CreateBoardSection(ModuleIDs.SalesEnquiry, board);
			var config = (ModuleGridSectionConfiguration)section.Configuration;

			var panelConfig = config.PanelConfigurations.First() as ModuleGridSectionPanelConfiguration;
			panelConfig.FilterLayout = layoutPk;

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(board))
			{
				var control = form.FindSingle<StripControl>();
				var filters = control.FilterBusinessObject.ActiveModuleFilters.Select(x => x.Description);

				AssertContainsExactElementsInAnyOrder("A Buffer Management Component filter should not have been added. SAD!", new[] { "City" }, filters);
			}
		}

		public void TestBufferManagementComponentFilter_WhenBoardIsNotWorkflowModeller_ShouldNotBeAltered()
		{
			ZGuid layoutPk;
			var system = CreateSystem("INQ");
			var component = BMSTestHelper.CreateBucket(system);
			Factory.Save();

			using (var module = ZFilterModule.GetZFilterModule(ModuleIDs.SalesEnquiry))
			{
				var filterBizo = module.FilterBusinessObject;
				filterBizo.AddTextFilterStrip("City", "Schmlonathan");
				var componentFilter = filterBizo.AddGuidFilterStrip("Buffer Management Component", component.PK);
				componentFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
				layoutPk = filterBizo.SaveLayout("Schmangela").PK;
			}

			var board = CreateBoard(system);
			var section = BMSTestHelper.CreateBoardSection(ModuleIDs.SalesEnquiry, board);
			var config = (ModuleGridSectionConfiguration)section.Configuration;

			var panelConfig = config.PanelConfigurations.First() as ModuleGridSectionPanelConfiguration;
			panelConfig.FilterLayout = layoutPk;

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(board))
			{
				var control = form.FindSingle<StripControl>();
				var filters = control.FilterBusinessObject.ActiveModuleFilters.Select(x => x.Description);

				AssertContainsExactElementsInAnyOrder(new[] { "City", "Buffer Management Component" }, filters);

				var componentFilter = (ModuleGuidFilter)control.FilterBusinessObject.ActiveModuleFilters.Single(x => x.Description == "Buffer Management Component");
				AssertEquals("The values on the component filter should not have been altered. SAD!", ModuleTextFilter.ComparisonConstants.NotEqual, componentFilter.ComparisonOperator);
				AssertEquals("The values on the component filter should not have been altered. SAD!", component.PK, componentFilter.Property);
			}
		}

		public void TestRefreshBoardShouldNotShowFilterStripSearchResultMessageBox()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var board = BMSTestHelper.CreateBoard(system);
			var section = BMSTestHelper.CreateBoardSection(ModuleIDs.ProcessHeader, board);
			var config = (ModuleGridSectionConfiguration)section.Configuration;

			Factory.Save();

			using (DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(VisualBoardFormTest.GetViewModel(section.Board)))
			using (var control = CreateModuleControl(config))
			{
				form.Controls.Add(control);
				form.Show();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				form.controlsPanel.Expand();
				form.RefreshButton.PerformClick();
				Application.DoEvents();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				var stripControl = control.FindAll<StripControl>().Single();
				var toolStrip = stripControl.FindAll<ZToolStrip>().Single(t => t.Name == "ToolStrip");
				var button = toolStrip.Items.OfType<ToolStripSplitButton>().Single(n => n.Name == "ToolStripFindDropButton");

				button.PerformClick();
				button.PerformButtonClick();

				Application.DoEvents();
				AssertEquals("There are no records that match your search.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestMultipleModuleGridSections_ShouldAllLoadCorrectFilters()
		{
			ZGuid layout1;
			ZGuid layout2;

			using (var module = ZFilterModule.GetZFilterModule(ModuleIDs.SalesEnquiry))
			{
				var filterBizo = module.FilterBusinessObject;
				filterBizo.AddTextFilterStrip("City", "Little Bits");
				layout1 = filterBizo.SaveLayout("Schmlonathan").PK;
			}

			using (var module = ZFilterModule.GetZFilterModule(ModuleIDs.SalesEnquiry))
			{
				var filterBizo = module.FilterBusinessObject;
				filterBizo.AddTextFilterStrip("State", "Schmlona");
				layout2 = filterBizo.SaveLayout("Schmlangela").PK;
			}

			var system = CreateSystem("INQ");
			var board = CreateBoard(system);

			var section1 = BMSTestHelper.CreateBoardSection(ModuleIDs.SalesEnquiry, board);
			var panelConfig1_1 = ((ModuleGridSectionConfiguration)section1.Configuration).PanelConfigurations.First() as ModuleGridSectionPanelConfiguration;
			panelConfig1_1.FilterLayout = layout1;
			panelConfig1_1.ShowFilters = true;

			var panelConfig1_2 = new ModuleGridSectionPanelConfiguration(section1); // need two panels for the ModuleSwitcher to show
			panelConfig1_2.FilterLayout = layout1;
			panelConfig1_2.ShowFilters = true;
			((ModuleGridSectionConfiguration)section1.Configuration).PanelConfigurations.Add(panelConfig1_2);

			var section2 = BMSTestHelper.CreateBoardSection(ModuleIDs.SalesEnquiry, board);
			var panelConfig2_1 = ((ModuleGridSectionConfiguration)section2.Configuration).PanelConfigurations.First() as ModuleGridSectionPanelConfiguration;
			panelConfig2_1.FilterLayout = layout2;
			panelConfig2_1.ShowFilters = true;

			var panelConfig2_2 = new ModuleGridSectionPanelConfiguration(section2); // need two panels for the ModuleSwitcher to show
			panelConfig2_2.FilterLayout = layout2;
			panelConfig2_2.ShowFilters = true;
			((ModuleGridSectionConfiguration)section2.Configuration).PanelConfigurations.Add(panelConfig2_2);

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(board))
			{
				var controls = form.FindAll<ModuleSelectorControl>().ToArray();
				var dropEdit1 = controls[0].FindSingle<ZDropEdit>(c => c.Name == "ModuleSelectDropEdit");
				var dropEdit2 = controls[1].FindSingle<ZDropEdit>(c => c.Name == "ModuleSelectDropEdit");

				AssertEquals("1", dropEdit1.Text);
				AssertEquals("1", dropEdit2.Text);

				int index1 = 0, index2 = 0;
				if (dropEdit1.DescriptionBox.Text == "Schmlonathan")
				{
					AssertEquals("Schmlangela", dropEdit2.DescriptionBox.Text);
					index2 = 1;
				}
				else if (dropEdit1.DescriptionBox.Text == "Schmlangela")
				{
					AssertEquals("Schmlonathan", dropEdit2.DescriptionBox.Text);
					index1 = 1;
				}
				else
				{
					Fail("Invalid panel name discovered: " + dropEdit1.DescriptionBox.Text);
				}

				var stripControl = controls[index1].FindSingle<StripControl>();
				var filters = stripControl.FilterBusinessObject.ActiveModuleFilters.Select(x => x.Description);
				AssertContainsExactElementsInAnyOrder("The correct layout should have been loaded, not the other one. SAD!", new[] { "City" }, filters);

				stripControl = controls[index2].FindSingle<StripControl>();
				filters = stripControl.FilterBusinessObject.ActiveModuleFilters.Select(x => x.Description);
				AssertContainsExactElementsInAnyOrder("The correct layout should have been loaded, not the other one. SAD!", new[] { "State" }, filters);
			}
		}

		public void TestModuleGridSections_ShouldNotAffectLastUsedLayoutOnModule()
		{
			ZGuid layoutPk;

			using (var module = ZFilterModule.GetZFilterModule(ModuleIDs.SalesEnquiry))
			{
				var filterBizo = module.FilterBusinessObject;
				filterBizo.AddTextFilterStrip("State", "Two Brothers");
				layoutPk = filterBizo.SaveLayout("Eye Holes").PK;
			}

			using (var module = ZFilterModule.GetZFilterModule(ModuleIDs.SalesEnquiry))
			{
				var filterBizo = module.FilterBusinessObject;
				filterBizo.AddTextFilterStrip("City", "Fake Doors");
				var layout = filterBizo.SaveLayout("Strawberry Smiggles");
				filterBizo.SaveLastUsedLayout(layout.PK);
			}

			using (var module = ZFilterModule.GetZFilterModule(ModuleIDs.SalesEnquiry))
			using (module.ShowPopup())
			{
				Application.DoEvents();
				var filterBizo = module.FilterBusinessObject;
				AssertContainsExactElementsInAnyOrder("The 'Strawberry Smiggles' layout should automatically be selected because it was the last used layout. SAD!",
					new[] { "Fake Doors" }, filterBizo.ActiveModuleFilters.Select(x => ((ModuleTextFilter)x).Property));
			}

			var system = BMSTestHelper.CreateSystem(Factory, "INQ");
			var board = CreateBoard(system);
			var section = BMSTestHelper.CreateBoardSection(ModuleIDs.SalesEnquiry, board);
			var config = (ModuleGridSectionConfiguration)section.Configuration;

			var panelConfig = config.PanelConfigurations.First() as ModuleGridSectionPanelConfiguration;
			panelConfig.FilterLayout = layoutPk;

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(board))
			{
				var stripControl = form.FindSingle<StripControl>();
				var filters = stripControl.FilterBusinessObject.ActiveModuleFilters.Select(x => ((ModuleTextFilter)x).Property);
				AssertContainsExactElementsInAnyOrder("The correct layout should have been loaded. SAD!", new[] { "Two Brothers" }, filters);
			}

			using (var module = ZFilterModule.GetZFilterModule(ModuleIDs.SalesEnquiry))
			using (module.ShowPopup())
			{
				Application.DoEvents();
				var filterBizo = module.FilterBusinessObject;
				AssertContainsExactElementsInAnyOrder("The 'Strawberry Smiggles' layout should still automatically be selected because it was the last used layout, despite the fact that a different layout was loaded on a visual board. SAD!",
					new[] { "Fake Doors" }, filterBizo.ActiveModuleFilters.Select(x => ((ModuleTextFilter)x).Property));
			}
		}

		public void TestToggleFilterVisibility()
		{
			ZGuid layoutPk;

			using (var module = ZFilterModule.GetZFilterModule(ModuleIDs.ProcessHeader))
			{
				var filterBizo = module.FilterBusinessObject;
				filterBizo.AddTextFilterStrip("Completion Statement", "LALALA");
				layoutPk = filterBizo.SaveLayout("FilterLayout1").PK;
			}

			var system = CreateSystem("DUM");
			var board = CreateBoard(system);
			var section = BMSTestHelper.CreateBoardSection(ModuleIDs.ProcessHeader, board);
			var config = (ModuleGridSectionConfiguration)section.Configuration;

			var panelConfig = config.PanelConfigurations.First() as ModuleGridSectionPanelConfiguration;
			panelConfig.ShowFilters = true;
			panelConfig.AllowFilterToggle = true;
			panelConfig.Sequence = 11;
			panelConfig.FilterLayout = layoutPk;

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(board))
			{
				var dropEdits = form.FindAll<ZFilterStripDropEdit>();
				var textbox = dropEdits.First();
				AssertEquals("Description", textbox.Text);

				var control = form.FindSingle<ModuleSelectorControl>();

				var toggleFilterVisibilityButton = control.FindAll<ZButton>().SingleOrDefault(z => z.Name == "HideShowFilterButton");
				AssertEquals("Filter toggle button is visible", true, toggleFilterVisibilityButton.Visible);
				var filterStripControl = form.FindSingle<ZFilterStripControl>();
				AssertEquals("Filter strip should be visibile", true, filterStripControl.IsFilterVisible);

				toggleFilterVisibilityButton.PerformClick();

				AssertEquals("Filter strip should be invisibile after the button click", false, filterStripControl.IsFilterVisible);
			}
		}

		#endregion

		public void TestShowsOverriddenSectionName()
		{
			var system = CreateSystem("ORG");
			var component = CreateBucket(system);
			var board = CreateBoard(system);
			var section = CreateBoardSection(component, board);
			section.MS_SectionType = BMConstants.ModuleGridSectionType;
			var config = (ModuleGridSectionConfiguration)section.Configuration;

			var panelConfig = new ModuleGridSectionPanelConfiguration(section);
			config.PanelConfigurations.Add(panelConfig);
			panelConfig.ModuleName = ModuleIDs.WorkItem.Name;

			using (var form = new ZForm())
			using (var control = CreateModuleControl(config))
			{
				control.Dock = DockStyle.Fill;
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();

				var groupBox = control.FindAll<ZGroupBox>().Single();
				AssertEquals("Work Items", groupBox.Text);
			}

			config.SectionNameIsOverridden = true;
			config.SectionNameOverride = "I will override you!";

			using (var form = new ZForm())
			using (var control = CreateModuleControl(config))
			{
				control.Dock = DockStyle.Fill;
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();

				var groupBox = control.FindAll<ZGroupBox>().Single();
				AssertEquals("I will override you!: Work Items", groupBox.Text);
			}
		}

		public void TestAddsSuffixToSectionName_AfterFilterChanged()
		{
			ZGuid layoutPk;

			using (var module = ZFilterModule.GetZFilterModule(ModuleIDs.ProcessHeader))
			{
				var filterBizo = module.FilterBusinessObject;
				filterBizo.AddTextFilterStrip("Completion Statement", "Dinglebop");
				layoutPk = filterBizo.SaveLayout("Grumbo").PK;
			}

			var system = CreateSystem("INQ");
			var board = CreateBoard(system);
			var section = BMSTestHelper.CreateBoardSection(ModuleIDs.ProcessHeader, board);
			var config = (ModuleGridSectionConfiguration)section.Configuration;

			var panelConfig = config.PanelConfigurations.First() as ModuleGridSectionPanelConfiguration;
			panelConfig.FilterLayout = layoutPk;
			panelConfig.ModuleName = ModuleIDs.WorkItem.Name;
			config.PanelConfigurations.Add(panelConfig);

			Factory.Save();

			using (var form = new ZForm())
			using (var control = CreateModuleControl(config))
			{
				control.Dock = DockStyle.Fill;
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();

				var groupBox = control.FindAll<ZGroupBox>().Single();
				AssertEquals("Work Items", groupBox.Text);

				var stripControl = form.FindSingle<StripControl>();
				var filter = stripControl.FilterBusinessObject;
				var textFilter = (ModuleTextFilter)filter["Priority"];
				textFilter.Property = "1";

				groupBox = control.FindAll<ZGroupBox>().Single();
				AssertEquals("Work Items - filters modified", groupBox.Text);

				textFilter.Property = "2";

				groupBox = control.FindAll<ZGroupBox>().Single();
				AssertEquals("Work Items - filters modified", groupBox.Text);
			}
		}

		#region Multipanel

		public void TestMultipanelDisplay_ShouldPerformSearch_WhenSelect()
		{
			ZGuid layoutPk; // We really only need this to filter out the job-level workflow, but it's good to test that the layout is loaded too.

			using (var module = ZFilterModule.GetZFilterModule(ModuleIDs.ProcessHeader))
			{
				var filterBizo = module.FilterBusinessObject;
				var filter = filterBizo.AddFilterStrip<JobOrWorkflowFilter>(ProcessHeader.ModuleFilterConstants.JobOrWorkflow);
				filter.SetWorkflowOnly();
				layoutPk = filterBizo.SaveLayout("Eye Holes").PK;
			}

			var system = CreateSystem(DummyWorkflowDescriptor.Instance.Code);
			var component = CreateBucket(system);
			var board = CreateBoard(system);
			var section = CreateBoardSection(component, board);
			section.MS_SectionType = BMConstants.ModuleGridSectionType;
			var config = (ModuleGridSectionConfiguration)section.Configuration;

			var panelConfig1 = new ModuleGridSectionPanelConfiguration(section);
			config.PanelConfigurations.Add(panelConfig1);
			panelConfig1.ModuleName = ModuleIDs.ProcessTasks.Name;
			panelConfig1.ShowFilters = true;
			panelConfig1.Sequence = 1;

			var panelConfig2 = new ModuleGridSectionPanelConfiguration(section);
			config.PanelConfigurations.Add(panelConfig2);
			panelConfig2.ModuleName = ModuleIDs.ProcessHeader.Name;
			panelConfig2.FilterLayout = layoutPk;
			panelConfig2.ShowFilters = true;
			panelConfig2.Sequence = 2;

			BMSTestHelper.CreateWorkflowAndTask(Factory, "Workflow for MOD section", description: "Task for MOD section");

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(board))
			{
				var grid = form.FindSingle<ZGrid>();
				var tasks = grid.ListManager.List.OfType<ProcessTask>().Select(x => x.P9_Description);
				AssertContainsExactElementsInAnyOrder("The first module should be shown and searched automatically when the form opens.",
					new[] { "Task for MOD section" }, tasks);

				var control = form.FindSingle<ModuleSelectorControl>();
				var dropEdit = control.FindSingle<ZDropEdit>(c => c.Name == "ModuleSelectDropEdit");

				dropEdit.CodeBox.Text = "2";
				dropEdit.OnItemSelected(dropEdit.List[1] as ICodeDescription, true);
				Application.DoEvents();
				UserIdleWorker.Flush();

				grid = form.FindSingle<ZGrid>();
				var workflows = grid.ListManager.List.OfType<ProcessHeader>().Select(x => x.FH_CompletionStatement);
				AssertContainsExactElementsInAnyOrder("The other module should have been displayed and automatically searched when the dropdown changed.",
					new[] { "Workflow for MOD section" }, workflows);

				dropEdit.CodeBox.Text = "1";
				dropEdit.OnItemSelected(dropEdit.List[0] as ICodeDescription, true);
				Application.DoEvents();
				UserIdleWorker.Flush();

				grid = form.FindSingle<ZGrid>();
				tasks = grid.ListManager.List.OfType<ProcessTask>().Select(x => x.P9_Description);
				AssertContainsExactElementsInAnyOrder("The other module should have been displayed and automatically searched when the dropdown changed.",
					new[] { "Task for MOD section" }, tasks);
			}
		}

		public void TestMultipanelDisplay_WhenSelectionChanged_AndLastUsedLayoutExists_ShouldSelectConfiguredLayout()
		{
			ZGuid layoutPk;

			using (var module = ZFilterModule.GetZFilterModule(ModuleIDs.SalesEnquiry))
			{
				var filterBizo = module.FilterBusinessObject;
				filterBizo.AddTextFilterStrip("State", "Two Brothers");
				layoutPk = filterBizo.SaveLayout("Eye Holes").PK;
			}

			using (var module = ZFilterModule.GetZFilterModule(ModuleIDs.SalesEnquiry))
			{
				var filterBizo = module.FilterBusinessObject;
				filterBizo.AddTextFilterStrip("City", "Fake Doors");
				var layout = filterBizo.SaveLayout("Strawberry Smiggles");
				filterBizo.SaveLastUsedLayout(layout.PK);
			}

			using (var module = ZFilterModule.GetZFilterModule(ModuleIDs.SalesEnquiry))
			using (module.ShowPopup())
			{
				Application.DoEvents();
				var filterBizo = module.FilterBusinessObject;
				AssertContainsExactElementsInAnyOrder("The 'Strawberry Smiggles' layout should automatically be selected because it was the last used layout. SAD!",
					new[] { "Fake Doors" }, filterBizo.ActiveModuleFilters.Select(x => ((ModuleTextFilter)x).Property));
			}

			var system = CreateSystem(DummyWorkflowDescriptor.Instance.Code);
			var component = CreateBucket(system);
			var board = CreateBoard(system);
			var section = CreateBoardSection(component, board);
			section.MS_SectionType = BMConstants.ModuleGridSectionType;
			var config = (ModuleGridSectionConfiguration)section.Configuration;

			var panelConfig1 = new ModuleGridSectionPanelConfiguration(section);
			config.PanelConfigurations.Add(panelConfig1);
			panelConfig1.ModuleName = DummyModuleIDs.Dummy.Name;
			panelConfig1.ShowFilters = true;
			panelConfig1.Sequence = 1;

			var panelConfig2 = new ModuleGridSectionPanelConfiguration(section);
			config.PanelConfigurations.Add(panelConfig2);
			panelConfig2.ModuleName = ModuleIDs.SalesEnquiry.Name;
			panelConfig2.FilterLayout = layoutPk;
			panelConfig2.ShowFilters = true;
			panelConfig2.Sequence = 2;

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(board))
			{
				var control = form.FindSingle<ModuleSelectorControl>();
				var dropEdit = control.FindSingle<ZDropEdit>(c => c.Name == "ModuleSelectDropEdit");

				dropEdit.CodeBox.Text = "2";
				dropEdit.OnItemSelected(dropEdit.List[1] as ICodeDescription, true);
				Application.DoEvents();
				UserIdleWorker.Flush();

				var stripControl = form.FindSingle<StripControl>();
				var filters = stripControl.FilterBusinessObject.ActiveModuleFilters.Select(x => ((ModuleTextFilter)x).Property);
				AssertContainsExactElementsInAnyOrder("The correct layout should have been loaded. SAD!", new[] { "Two Brothers" }, filters);
			}

			using (var module = ZFilterModule.GetZFilterModule(ModuleIDs.SalesEnquiry))
			using (module.ShowPopup())
			{
				Application.DoEvents();
				var filterBizo = module.FilterBusinessObject;
				AssertContainsExactElementsInAnyOrder("The 'Strawberry Smiggles' layout should still automatically be selected because it was the last used layout, despite the fact that a different layout was loaded on a visual board. SAD!",
					new[] { "Fake Doors" }, filterBizo.ActiveModuleFilters.Select(x => ((ModuleTextFilter)x).Property));
			}
		}

		public void TestOpenInANewWindowButtonInvisibility()
		{
			var system = CreateSystem("DUM");
			var board = CreateBoard(system);
			var section = BMSTestHelper.CreateBoardSection(ModuleIDs.ProcessHeader, board);
			var config = (ModuleGridSectionConfiguration)section.Configuration;

			var panelConfig = config.PanelConfigurations.First() as ModuleGridSectionPanelConfiguration;
			panelConfig.ShowFilters = true;
			panelConfig.Sequence = 11;
			panelConfig.AllowOpenModule = false;

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(board))
			{
				var control = form.FindSingle<ModuleSelectorControl>();
				var button = control.FindSingle<ZButton>(z => z.Name == "OpenInANewWindowButton");
				Assert(!button.Visible);
			}
		}

		public void TestOpenInANewWindowButton()
		{
			ZGuid layoutPk;

			using (var module = ZFilterModule.GetZFilterModule(ModuleIDs.ProcessHeader))
			{
				var filterBizo = module.FilterBusinessObject;
				filterBizo.AddTextFilterStrip("Completion Statement", "LALALA");
				layoutPk = filterBizo.SaveLayout("FilterLayout1").PK;
			}

			var system = CreateSystem("DUM");
			var board = CreateBoard(system);
			var section = BMSTestHelper.CreateBoardSection(ModuleIDs.ProcessHeader, board);
			var config = (ModuleGridSectionConfiguration)section.Configuration;

			var panelConfig = config.PanelConfigurations.First() as ModuleGridSectionPanelConfiguration;
			panelConfig.ShowFilters = true;
			panelConfig.AllowFilterEdit = true;
			panelConfig.AllowFilterToggle = true;
			panelConfig.Sequence = 11;
			panelConfig.FilterLayout = layoutPk;

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(board))
			{
				var dropEdits = form.FindAll<ZFilterStripDropEdit>();
				var textbox = dropEdits.First();
				AssertEquals("Description", textbox.Text);

				var control = form.FindSingle<ModuleSelectorControl>();
				var button = control.FindSingle<ZButton>(z => z.Name == "OpenInANewWindowButton");
				Assert(button.Visible);

				button.PerformClick();
				Assert(!button.Enabled);

				var moduleForm = Application.OpenForms.OfType<EmbeddedModulePopup>().Single();
				AssertNotNull(moduleForm);

				var filterStripControl = moduleForm.FindSingle<ZFilterStripControl>();
				AssertEquals("Filter strip should be visibile", true, filterStripControl.IsFilterVisible);
				AssertEquals("Filter strip is editable", false, filterStripControl.IsFilterReadonly);

				var grid = control.FindAll<ZGrid>().Single();
				AssertNotNull(grid.ContextMenu.MenuItems.FindByText("Hide/Show Filters"));

				moduleForm.Close();
				Assert(button.Enabled);
			}
		}

		public void TestOpenInANewWindowButtonFilterStripInvisibility()
		{
			ZGuid layoutPk;

			using (var module = ZFilterModule.GetZFilterModule(ModuleIDs.ProcessHeader))
			{
				var filterBizo = module.FilterBusinessObject;
				filterBizo.AddTextFilterStrip("Completion Statement", "LALALA");
				layoutPk = filterBizo.SaveLayout("FilterLayout1").PK;
			}

			var system = CreateSystem("DUM");
			var board = CreateBoard(system);
			var section = BMSTestHelper.CreateBoardSection(ModuleIDs.ProcessHeader, board);
			var config = (ModuleGridSectionConfiguration)section.Configuration;

			var panelConfig = config.PanelConfigurations.First() as ModuleGridSectionPanelConfiguration;
			panelConfig.Sequence = 11;
			panelConfig.AllowOpenModule = true;
			panelConfig.ShowFilters = false;
			panelConfig.FilterLayout = layoutPk;

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(board))
			{
				var control = form.FindSingle<ModuleSelectorControl>();
				var button = control.FindSingle<ZButton>(z => z.Name == "OpenInANewWindowButton");
				Assert(button.Visible);

				button.PerformClick();
				Assert(!button.Enabled);

				var moduleForm = Application.OpenForms.OfType<EmbeddedModulePopup>().Single();
				AssertNotNull(moduleForm);

				var filterStripControl = moduleForm.FindSingle<ZFilterStripControl>();
				AssertEquals("Filter strip should be invisibile", false, filterStripControl.IsFilterVisible);

				moduleForm.Close();
				Assert(button.Enabled);
			}
		}

		public void TestOpenInANewWindowButtonFilterStripReadOnly()
		{
			ZGuid layoutPk;

			using (var module = ZFilterModule.GetZFilterModule(ModuleIDs.ProcessHeader))
			{
				var filterBizo = module.FilterBusinessObject;
				filterBizo.AddTextFilterStrip("Completion Statement", "LALALA");
				layoutPk = filterBizo.SaveLayout("FilterLayout1").PK;
			}

			var system = CreateSystem("DUM");
			var board = CreateBoard(system);
			var section = BMSTestHelper.CreateBoardSection(ModuleIDs.ProcessHeader, board);
			var config = (ModuleGridSectionConfiguration)section.Configuration;

			var panelConfig = config.PanelConfigurations.First() as ModuleGridSectionPanelConfiguration;
			panelConfig.Sequence = 11;
			panelConfig.AllowOpenModule = true;
			panelConfig.ShowFilters = true;
			panelConfig.AllowFilterEdit = false;
			panelConfig.FilterLayout = layoutPk;

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(board))
			{
				var control = form.FindSingle<ModuleSelectorControl>();
				var button = control.FindSingle<ZButton>(z => z.Name == "OpenInANewWindowButton");
				Assert(button.Visible);

				button.PerformClick();
				Assert(!button.Enabled);

				var moduleForm = Application.OpenForms.OfType<EmbeddedModulePopup>().Single();
				AssertNotNull(moduleForm);

				var filterStripControl = moduleForm.FindSingle<ZFilterStripControl>();
				AssertEquals("Filter strip should be visibile", true, filterStripControl.IsFilterVisible);
				AssertEquals("Filter strip should be readonly", true, filterStripControl.IsFilterReadonly);

				moduleForm.Close();
				Assert(button.Enabled);
			}
		}

		public void TestOpenInANewWindowButtonFilterToggleNotInContextMenu()
		{
			ZGuid layoutPk;

			using (var module = ZFilterModule.GetZFilterModule(ModuleIDs.ProcessHeader))
			{
				var filterBizo = module.FilterBusinessObject;
				filterBizo.AddTextFilterStrip("Completion Statement", "LALALA");
				layoutPk = filterBizo.SaveLayout("FilterLayout1").PK;
			}

			var system = CreateSystem("DUM");
			var board = CreateBoard(system);
			var section = BMSTestHelper.CreateBoardSection(ModuleIDs.ProcessHeader, board);
			var config = (ModuleGridSectionConfiguration)section.Configuration;

			var panelConfig = config.PanelConfigurations.First() as ModuleGridSectionPanelConfiguration;
			panelConfig.Sequence = 11;
			panelConfig.AllowOpenModule = true;
			panelConfig.ShowFilters = true;
			panelConfig.AllowFilterToggle = false;
			panelConfig.FilterLayout = layoutPk;

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(board))
			{
				var control = form.FindSingle<ModuleSelectorControl>();
				var button = control.FindSingle<ZButton>(z => z.Name == "OpenInANewWindowButton");
				Assert(button.Visible);

				button.PerformClick();
				Assert(!button.Enabled);

				var moduleForm = Application.OpenForms.OfType<EmbeddedModulePopup>().Single();
				AssertNotNull(moduleForm);

				var grid = control.FindAll<ZGrid>().Single();
				AssertNull(grid.ContextMenu.MenuItems.FindByText("Hide/Show Filters"));

				moduleForm.Close();
				Assert(button.Enabled);
			}
		}

		public void TestOpenInANewWindow_WhenNoLayoutConfigured_ShouldLoadLastUsedLayout()
		{
			using (var module = ZFilterModule.GetZFilterModule(ModuleIDs.ProcessHeader))
			{
				var filterBizo = module.FilterBusinessObject;
				filterBizo.AddTextFilterStrip("Completion Statement", "A");
				filterBizo.SaveLayout("ZName that will appear last");
			}

			using (var module = ZFilterModule.GetZFilterModule(ModuleIDs.ProcessHeader))
			{
				var filterBizo = module.FilterBusinessObject;
				filterBizo.AddTextFilterStrip("Completion Statement", "B");
				var layout = filterBizo.SaveLayout("FilterLayout2");
				filterBizo.SaveLastUsedLayout(layout.PK);
			}

			using (var module = ZFilterModule.GetZFilterModule(ModuleIDs.ProcessHeader))
			using (module.ShowPopup())
			{
				Application.DoEvents();
				var filterBizo = module.FilterBusinessObject;
				AssertContainsExactElementsInAnyOrder("'FilterLayout2' should automatically be selected because it was the last used layout.",
					new[] { "B" }, filterBizo.ActiveModuleFilters.Select(x => ((ModuleTextFilter)x).Property));
			}

			var jobLevelWorkflow = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobLevelWorkflow, "A Workflow");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobLevelWorkflow, "B Workflow");

			var system = CreateSystem("DUM");
			var board = CreateBoard(system);
			var section = BMSTestHelper.CreateBoardSection(ModuleIDs.ProcessHeader, board);
			var config = (ModuleGridSectionConfiguration)section.Configuration;

			var panelConfig = config.PanelConfigurations.First() as ModuleGridSectionPanelConfiguration;
			panelConfig.ShowFilters = true;
			panelConfig.Sequence = 11;

			AssertEquals(ZGuid.Empty, panelConfig.FilterLayout);

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(board))
			{
				var button = form.FindSingle<ZButton>(z => z.Name == "OpenInANewWindowButton");
				EmbeddedModulePopup popupForm = null;

				try
				{
					button.PerformClick();
					Application.DoEvents();

					popupForm = BMSFormTestHelper.GetOpenForms<EmbeddedModulePopup>().SingleOrDefault();
					AssertNotNull(popupForm);

					var filterBizo = popupForm.Module_ForTest.FilterBusinessObject;
					AssertContainsExactElementsInAnyOrder("'FilterLayout2' should be selected in the popup form as well because it was the last used layout.",
						new[] { "B" }, filterBizo.ActiveModuleFilters.Select(x => ((ModuleTextFilter)x).Property));

					var results = popupForm.Module_ForTest.GridCollection.Cast<ProcessHeader>().Select(x => x.FH_CompletionStatement);
					AssertContainsExactElementsInAnyOrder("'FilterLayout2' should be selected in the popup form as well because it was the last used layout.",
						new[] { "B Workflow" }, results);
				}
				finally
				{
					popupForm?.Dispose();
					Application.DoEvents();
				}
			}
		}

#if !WINZOR
		public void TestSingleScrollbarVisible()
		{
			var system = CreateSystem("DUM");
			var board = CreateBoard(system);
			var section = BMSTestHelper.CreateBoardSection(ModuleIDs.ProcessHeader, board);
			var config = (ModuleGridSectionConfiguration)section.Configuration;

			var panelConfig = config.PanelConfigurations.First() as ModuleGridSectionPanelConfiguration;
			panelConfig.ShowFilters = true;
			panelConfig.Sequence = 11;

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(board))
			{
				var control = form.FindSingle<ModuleSelectorControl>();
				Assert(control.FindSingle<HScrollBar>().Visible);
				Assert(control.FindSingle<ZDisplayGrid>().FindSingle<HScrollBar>().Visible);
			}
		}
#endif

		public void TestShouldDisposeEmbeddedControl_WhenNoModulesSpecified()
		{
			var system = CreateSystem("DUM");
			var board = CreateBoard(system);
			var section = BMSTestHelper.CreateBoardSection(ModuleIDs.ProcessHeader, board);
			var config = (ModuleGridSectionConfiguration)section.Configuration;

			config.PanelConfigurations.RemoveAll();

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(board))
			{
			}

			Assert("We just need to ensure there are no undisposed objects after the test determined by DisposableLeakListener", true);
		}

		public void TestShouldDisposeEmbeddedControl_WhenModulesAreSpecified()
		{
			var system = CreateSystem("DUM");
			var board = CreateBoard(system);
			var section = BMSTestHelper.CreateBoardSection(ModuleIDs.ProcessHeader, board);
			var config = (ModuleGridSectionConfiguration)section.Configuration;

			Assert("Precondition", config.PanelConfigurations.Any());
			var panelConfig = config.PanelConfigurations.First() as ModuleGridSectionPanelConfiguration;
			panelConfig.ShowFilters = true;
			panelConfig.Sequence = 11;

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(board))
			{
			}

			Assert("We just need to ensure there are no undisposed objects after the test determined by DisposableLeakListener", true);
		}

		public void TestShouldDisposeEmbeddedControl_AfterSwitchingBetweenModules()
		{
			var system = CreateSystem("DUM");
			var board = CreateBoard(system);
			var section = BMSTestHelper.CreateBoardSection(ModuleIDs.ProcessHeader, board);
			var config = (ModuleGridSectionConfiguration)section.Configuration;

			config.PanelConfigurations.RemoveAll();

			var panelConfig1 = new ModuleGridSectionPanelConfiguration(section);
			panelConfig1.ModuleName = ModuleIDs.ProcessHeader.Name;
			panelConfig1.ShowFilters = true;
			panelConfig1.AllowFilterToggle = true;
			panelConfig1.Sequence = 1;
			config.PanelConfigurations.Add(panelConfig1);

			var panelConfig2 = new ModuleGridSectionPanelConfiguration(section);
			panelConfig2.ModuleName = ModuleIDs.ProcessTasks.Name;
			panelConfig2.ShowFilters = true;
			panelConfig2.AllowFilterToggle = false;
			panelConfig2.Sequence = 2;
			config.PanelConfigurations.Add(panelConfig2);

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(board))
			{
				var control = form.FindSingle<ModuleSelectorControl>();
				var dropEdit = control.FindSingle<ZDropEdit>(c => c.Name == "ModuleSelectDropEdit");

				CombineAssertions("Preconditions", () =>
				{
					AssertEquals(2, dropEdit.List.Count);
					AssertEquals("1", ((CodeDescriptionPair)dropEdit.List[0]).Code);
					AssertEquals(ModuleIDs.ProcessHeader.Description, ((CodeDescriptionPair)dropEdit.List[0]).Description);
					AssertEquals("2", ((CodeDescriptionPair)dropEdit.List[1]).Code);
					AssertEquals(ModuleIDs.ProcessTasks.Description, ((CodeDescriptionPair)dropEdit.List[1]).Description);
				});

				dropEdit.SelectItem("2");
				Application.DoEvents();
				dropEdit.SelectItem("1");
				Application.DoEvents();
			}

			Assert("We just need to ensure there are no undisposed objects after the test determined by DisposableLeakListener", true);
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			BMSTestHelper.EnableBMSInRegistry();
		}

		internal static ModuleSelectorControl CreateModuleControl(ModuleGridSectionConfiguration config)
		{
			var boardViewModel = VisualBoardsTestHelper.CreateBoardViewModel(config.Section.Board);
			var sectionViewModel = new BoardSectionViewModel(config.Section, boardViewModel);

			return new ModuleSelectorControl(config, sectionViewModel);
		}

		protected override bool ShouldDisableAsyncBehaviour => true;

		#endregion
	}

	[TestedType(typeof(ModuleSelectorControl))]
	class ModuleSelectorControlIBoardSectionControlTest : BoardSectionControlTestCase<ModuleSelectorControl>
	{
		protected override ModuleSelectorControl GetControl()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var board = BMSTestHelper.CreateBoard(system);
			var section = BMSTestHelper.CreateBoardSection(ModuleIDs.ProcessHeader, board);
			var config = (ModuleGridSectionConfiguration)section.Configuration;

			return ModuleSelectorControlTest.CreateModuleControl(config);
		}
	}

	class ModuleSelectorControlNonTransactionedTest : NonTransactionedTestCase
	{
		#region Threading OnShowForm

		[TestDate(2019, 11, 11)]
		public void TestShowForm_ShouldNotThrowThreadSentryException_WhenNewFormHasFilterBusinessObjects()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);

			var section = BMSTestHelper.CreateBoardSection(config.Buffer, config.BufferBoard);
			section.MS_SectionType = BMConstants.ModuleGridSectionType;
			var sectionConfig = (ModuleGridSectionConfiguration)section.Configuration;

			var panelConfig = new ModuleGridSectionPanelConfiguration(section);
			sectionConfig.PanelConfigurations.Add(panelConfig);
			panelConfig.ModuleName = ModuleIDs.ZAPMatching.Name;
			panelConfig.AllowFilterToggle = false;
			panelConfig.AllowOpenModule = false;

			var testObjectCreator = new TestObjectCreator(Factory);
			var org = testObjectCreator.CreateOrgHeader("TSTORG1", creditor: true, debtor: false);

			SetupAccountingEnvironment(testObjectCreator, org);

			Factory.Save();

			using (VisualBoardsTestCase.ApplyAsyncStrategy(new TaskTrackingAsyncBaseStrategy()))
			using (var form = VisualBoardFormDisplayer.ShowBoard(sectionConfig.Board))
			{
				form.AwaitAll();
				var moduleSelectorControl = form.FindAll<ModuleSelectorControl>().FirstOrDefault();
				AssertNotNull("GIVEN VisualBoard with 'MOD'(Module-Grid) section-type", moduleSelectorControl);

				var grid = moduleSelectorControl.FindAll<ZGrid>().Single(c => c.Name == "FilteredGrid");

				grid.Invoke(new Action(() =>
				{
					AssertEquals("GIVEN 1 row", 1, grid.ListManager.List.Count);
				}));

				AssertNoThreadSentryErrorsWhenPerformingMenuItemAction(form, "New", f => DoMatchingThings(f, org.PK));
			}
		}

		[TestDate(2019, 11, 11)]
		public void TestShowForm_ShouldNotThrowThreadSentryException_WhenShipmentIsDeactivated()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);

			var section = BMSTestHelper.CreateBoardSection(config.Buffer, config.BufferBoard);
			section.MS_SectionType = BMConstants.ModuleGridSectionType;
			var sectionConfig = (ModuleGridSectionConfiguration)section.Configuration;

			var panelConfig = new ModuleGridSectionPanelConfiguration(section);
			sectionConfig.PanelConfigurations.Add(panelConfig);
			panelConfig.ModuleName = ModuleIDs.JobShipment.Name;

			var shipment = Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();

			Factory.Save();

			using (VisualBoardsTestCase.ApplyAsyncStrategy(new TaskTrackingAsyncBaseStrategy()))
			using (var form = VisualBoardFormDisplayer.ShowBoard(sectionConfig.Board))
			{
				form.AwaitAll();
				var moduleSelectorControl = form.FindAll<ModuleSelectorControl>().FirstOrDefault();
				AssertNotNull("GIVEN VisualBoard with 'MOD'(Module-Grid) section-type", moduleSelectorControl);

				var grid = moduleSelectorControl.FindAll<ZGrid>().Single(c => c.Name == "FilteredGrid");
				grid.Invoke(new Action(() =>
				{
					AssertEquals("GIVEN 1 row", 1, grid.ListManager.List.Count);
					grid.Select(0);
				}));

				AssertNoThreadSentryErrorsWhenPerformingMenuItemAction(form, "Delete");
			}
		}

		public void TestShowForm_ShouldNotThrowThreadSentryException_WhenTemplateCopyForm()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);

			var section = BMSTestHelper.CreateBoardSection(config.Buffer, config.BufferBoard);
			section.MS_SectionType = BMConstants.ModuleGridSectionType;
			var sectionConfig = (ModuleGridSectionConfiguration)section.Configuration;

			var panelConfig = new ModuleGridSectionPanelConfiguration(section);
			sectionConfig.PanelConfigurations.Add(panelConfig);
			panelConfig.ModuleName = ModuleIDs.ARTransaction.Name;

			Factory.NewWithValidTestData<ARInvoice>();
			Factory.Save();

			using (VisualBoardsTestCase.ApplyAsyncStrategy(new TaskTrackingAsyncBaseStrategy()))
			using (var form = VisualBoardFormDisplayer.ShowBoard(sectionConfig.Board))
			{
				form.AwaitAll();
				var moduleSelectorControl = form.FindAll<ModuleSelectorControl>().FirstOrDefault();
				AssertNotNull("GIVEN VisualBoard with 'MOD'(Module-Grid) section-type", moduleSelectorControl);
				var grid = moduleSelectorControl.FindAll<ZGrid>().Single(c => c.Name == "FilteredGrid");

				grid.Invoke(new Action(() =>
				{
					AssertEquals("GIVEN 1 row", 1, grid.ListManager.List.Count);
				}));

				AssertNoThreadSentryErrorsWhenPerformingMenuItemAction(form, "Copy");
			}
		}

		public void TestShowForm_ShouldNotThrowThreadSentryException_WhenOpenFromFindBox()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "DUM");
			var board = BMSTestHelper.CreateBoard(system);
			var section = BMSTestHelper.CreateBoardSection(ModuleIDs.JobShipment, board);
			var sectionConfig = (ModuleGridSectionConfiguration)section.Configuration;

			var panelConfig = sectionConfig.PanelConfigurations.First() as ModuleGridSectionPanelConfiguration;
			panelConfig.ModuleName = ModuleIDs.JobShipment.Name;

			var moduleFilter = Factory.New<StmModuleFilter>();
			moduleFilter.S9_ModuleID = panelConfig.ModuleName;
			moduleFilter.S9_IsPublished = true;
			moduleFilter.S9_FilterName = "Job Sales Staff";

			var currentUserCode = (Env.CurrentUser as GlbStaff).GS_Code;

			FilterStripsTestHelper.AddFilterStrips(moduleFilter, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = moduleFilter.S9_FilterName,
				FilterStripValueSetter = f => (f as ModuleNkFilter).Property = currentUserCode
			});

			panelConfig.FilterLayout = moduleFilter.PK;
			panelConfig.ShowFilters = true;

			Factory.Save();

			var waitableMainThreadInvocationStrategy = new WaitableMainThreadInvocationStrategy();

			using (VisualBoardsTestCase.ApplyAsyncStrategy(new TaskTrackingAsyncBaseStrategy()))
			using (MainThreadRunner.OverrideInvocationStrategy(waitableMainThreadInvocationStrategy))
			using (var form = VisualBoardFormDisplayer.ShowBoard(sectionConfig.Board))
			{
				form.AwaitAll();
				var moduleSelectorControl = form.FindAll<ModuleSelectorControl>().FirstOrDefault();
				AssertNotNull("moduleSelectorControl should be loaded", moduleSelectorControl);

				var codeFindBox = moduleSelectorControl.FindSingle<ZCodeFindBox>(f => f.Text == currentUserCode);

				AssertNotNull("codeFindBox with current user code should be shown", codeFindBox);

				GlbStaffForm staffForm = null;

				try
				{
					KeySender.SendKeyDownToProcessCmdKey(codeFindBox.CodeBox, (int)Keys.F3);
					Application.DoEvents();

					staffForm = Application.OpenForms.OfType<GlbStaffForm>().SingleOrDefault();

					AssertNotNull("Staff form should be loaded", staffForm);
					Assert("Staff form should be visible", staffForm.Visible);
				}
				finally
				{
					staffForm.ForceClose();
					staffForm.Dispose();
				}

				AssertNull("No exception should be thrown", ErrorReporter.LastExceptionReported);
			}
		}

		public void TestShowForm_ShouldNotThrowThreadSentryException_WhenOpenFromOpenInANewWindowButton()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "DUM");
			var board = BMSTestHelper.CreateBoard(system);
			var section = BMSTestHelper.CreateBoardSection(ModuleIDs.JobShipment, board);
			var sectionConfig = (ModuleGridSectionConfiguration)section.Configuration;

			var panelConfig = sectionConfig.PanelConfigurations.First() as ModuleGridSectionPanelConfiguration;
			panelConfig.ModuleName = ModuleIDs.JobShipment.Name;

			var moduleFilter = Factory.New<StmModuleFilter>();
			moduleFilter.S9_ModuleID = panelConfig.ModuleName;
			moduleFilter.S9_IsPublished = true;
			moduleFilter.S9_FilterName = "Job Sales Staff";

			var currentUserCode = (Env.CurrentUser as GlbStaff).GS_Code;

			FilterStripsTestHelper.AddFilterStrips(moduleFilter, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = moduleFilter.S9_FilterName,
				FilterStripValueSetter = f => (f as ModuleNkFilter).Property = currentUserCode
			});

			panelConfig.FilterLayout = moduleFilter.PK;
			panelConfig.ShowFilters = true;
			panelConfig.AllowOpenModule = true;

			Factory.Save();

			using (VisualBoardsTestCase.ApplyAsyncStrategy(new TaskTrackingAsyncBaseStrategy()))
			using (var form = VisualBoardFormDisplayer.ShowBoard(sectionConfig.Board))
			{
				form.AwaitAll();
				EmbeddedModulePopup popup = null;

				try
				{
					var button = form.FindSingle<ZButton>(b => b.Name == "OpenInANewWindowButton");
					button.Invoke(new Action(() => button.PerformClick()));

					popup = Application.OpenForms.OfType<EmbeddedModulePopup>().Single();
					AssertEquals(ModuleIDs.JobShipment.Name, popup.CurrentModule.ModuleID.Name);
				}
				finally
				{
					popup.ForceClose();
					popup.Dispose();
				}

				AssertNull("No exception should be thrown", ErrorReporter.LastExceptionReported);
			}
		}

		public void TestMultiPanelRefresh()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "DUM");
			var board = BMSTestHelper.CreateBoard(system);
			var section = BMSTestHelper.CreateBoardSection(ModuleIDs.WorkItem, board);
			var sectionConfig = (ModuleGridSectionConfiguration)section.Configuration;

			var panelConfig1 = sectionConfig.PanelConfigurations.First() as ModuleGridSectionPanelConfiguration;
			var panelConfig2 = new ModuleGridSectionPanelConfiguration(section);
			sectionConfig.PanelConfigurations.Add(panelConfig2);
			panelConfig2.ModuleName = ModuleIDs.SalesEnquiry.Name;
			panelConfig2.ShowFilters = true;
			panelConfig2.Sequence = 2;

			Factory.Save();

			using (VisualBoardsTestCase.ApplyAsyncStrategy(new TaskTrackingAsyncBaseStrategy()))
			using (var form = VisualBoardFormDisplayer.ShowBoard(section.Board))
			{
				form.AwaitAll();
				var control = form.FindSingle<ModuleSelectorControl>();
				var dropEdit = control.FindSingle<ZDropEdit>(c => c.Name == "ModuleSelectDropEdit");

				dropEdit.Invoke(new Action(() =>
				{
					dropEdit.CodeBox.Text = "2";
					dropEdit.OnItemSelected(dropEdit.List[1] as ICodeDescription, true);
				}));

				BMSFormTestHelper.PressHotkeys(form, Keys.F5);
				form.AwaitAll();

				CombineAssertions(() =>
				{
					AssertEquals("2", dropEdit.Text);
					AssertEquals("Enterprise.MasterFiles.Module.SalesEnquiryFilterControl", control.FindSingle<ZFilterStripBaseControl>().GetType().ToString());
				});
			}
		}

		public void TestMultiPanelReload()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "DUM");
			var board = BMSTestHelper.CreateBoard(system);
			var section = BMSTestHelper.CreateBoardSection(ModuleIDs.WorkItem, board);
			var sectionConfig = (ModuleGridSectionConfiguration)section.Configuration;

			var panelConfig1 = sectionConfig.PanelConfigurations.First() as ModuleGridSectionPanelConfiguration;
			var panelConfig2 = new ModuleGridSectionPanelConfiguration(section);
			sectionConfig.PanelConfigurations.Add(panelConfig2);
			panelConfig2.ModuleName = ModuleIDs.SalesEnquiry.Name;
			panelConfig2.ShowFilters = true;
			panelConfig2.Sequence = 2;

			Factory.Save();

			using (VisualBoardsTestCase.ApplyAsyncStrategy(new TaskTrackingAsyncBaseStrategy()))
			using (var form = VisualBoardFormDisplayer.ShowBoard(section.Board))
			{
				form.AwaitAll();
				var control = form.FindSingle<ModuleSelectorControl>();
				var dropEdit = control.FindSingle<ZDropEdit>(c => c.Name == "ModuleSelectDropEdit");

				dropEdit.CodeBox.Text = "2";
				dropEdit.OnItemSelected(dropEdit.List[1] as ICodeDescription, true);

				BMSFormTestHelper.PressHotkeys(form, Keys.F5 | Keys.Shift);
				form.AwaitAll();

				// board redrawn, so lookup again
				control = form.FindSingle<ModuleSelectorControl>();
				dropEdit = control.FindSingle<ZDropEdit>(c => c.Name == "ModuleSelectDropEdit");

				CombineAssertions(() =>
				{
					AssertEquals("1", dropEdit.Text);
					AssertEquals("Enterprise.ProcessManagement.Module.WorkItemFilterControl", control.FindSingle<ZFilterStripBaseControl>().GetType().ToString());
				});
			}
		}

		void DoMatchingThings(ZForm myFormSmileyFace, ZGuid orgPK)
		{
			NewMatchGroupForm matchForm = (NewMatchGroupForm)myFormSmileyFace;

			matchForm.Show();
			Application.DoEvents();

			var orgGuidBox = matchForm.FindAll<ZGuidFindBox>(box => box.Name == "PrimaryOrgGuidFindBox");
			AssertEquals("We should find a single orgGuidFindBox on the matching form", 1, orgGuidBox.Count());

			var zapper = (APMatchingBase)matchForm.DataSource;
			zapper.PrimaryOrganisationForGUINotification = orgPK;

			matchForm.Show();
			Application.DoEvents();

			var outstandingTransactionsGrid = matchForm.FindAll<ZDisplayGrid>().First(grid => grid.Name == "UnmatchedTransactionsGrid");
			var includedTransactionsGrid = matchForm.FindAll<ZGrid>().First(grid => grid.Name == "MatchTransactionsGrid");

			AssertEquals("We have a two outstanding transactions for our organisation", 2, outstandingTransactionsGrid.VisibleRowCount);
			AssertEquals("We have no included transactions for our organisation, yet", 0, includedTransactionsGrid.VisibleRowCount);

			outstandingTransactionsGrid.Select(0);

			var registerTransactionButton = matchForm.FindAll<ZButton>().First(button => button.Name == "MoveDownButton");
			registerTransactionButton.PerformClick();

			AssertEquals("We have one outstanding transaction for our organisation", 1, outstandingTransactionsGrid.VisibleRowCount);
			AssertEquals("We have a single included transaction for our organisation", 1, includedTransactionsGrid.VisibleRowCount);

			var makeMatchButton = matchForm.FindAll<ZButton>().First(button => button.Name == "OverpaymentButton");
			makeMatchButton.PerformClick();

			var matchPaymentForm = Application.OpenForms.OfType<TransactionViewForm>().FirstOrDefault();
			AssertNotNull(matchPaymentForm);
			var addPaymentButton = matchPaymentForm.FindAll<ZPostingButtonsUserControl>().First();
			((IPostingButtonsProvider)matchPaymentForm).CommandButtonPost.PerformClick(); // click the "Add" button to add a payment override

			AssertEquals("We have two included transactions for our organisation, a deduction, and an addition", 2, includedTransactionsGrid.VisibleRowCount);

			matchForm.Show();
			Application.DoEvents();

			((IPostingButtonsProvider)matchForm).CommandButtonApply.PerformClick();

			matchForm.Show();
			Application.DoEvents();
		}

		void AssertNoThreadSentryErrorsWhenPerformingMenuItemAction(VisualBoardForm form, string menuItemName, Action<ZForm> testingAction = null)
		{
			var moduleSelectorControl = form.FindAll<ModuleSelectorControl>().FirstOrDefault();
			var grid = moduleSelectorControl.FindAll<ZGrid>().Single(c => c.Name == "FilteredGrid");
			var contextMenu = grid.ContextMenu;
			var menuItem = contextMenu.MenuItems.FindByText(menuItemName);

			menuItem.PerformClick();

			ZForm objectForm = null;

			try
			{
				objectForm = WaitForFormToOpen<ZForm>((f) => !(f is VisualBoardForm));
				AssertNotNull(string.Format("WHEN clicking {0} THEN object form should be shown", menuItemName), objectForm);

				testingAction?.Invoke(objectForm);
			}
			finally
			{
				objectForm?.Dispose();
			}

			AssertEquals(string.Format("WHEN clicking {0} THEN should not cause ThreadSentry Exception", menuItemName), string.Empty, ErrorReporter.LastMessageReported);
		}

		#region Warehousing specific tests

		public void TestShowNewFormWithFilter_WhsAdHocServiceJob_ShouldNotThrowThreadSentryException()
		{
			ShowNewFormWithFilter_Warehousing(ModuleIDs.WhsAdHocServiceJob, "AdHocServiceJobEntryForm");
		}

		public void TestShowNewFormWithFilter_WhsAdjustment_ShouldNotThrowThreadSentryException()
		{
			ShowNewFormWithFilter_Warehousing(ModuleIDs.WhsAdjustment, "AdjustmentEntryForm");
		}

		public void TestShowNewFormWithFilter_WhsConfigArea_ShouldNotThrowThreadSentryException()
		{
			ShowNewFormWithFilter_Warehousing(ModuleIDs.WhsConfigArea, "AreaEntryForm");
		}

		public void TestShowNewFormWithFilter_WhsConfigRow_ShouldNotThrowThreadSentryException()
		{
			ShowNewFormWithFilter_Warehousing(ModuleIDs.WhsConfigRow, "RowEntryForm");
		}

		public void TestShowNewFormWithFilter_WhsInvoicing_ShouldNotThrowThreadSentryException()
		{
			ShowNewFormWithFilter_Warehousing(ModuleIDs.WhsInvoicing, "InvoicingForm");
		}

		public void TestShowNewFormWithFilter_WhsOrder_ShouldNotThrowThreadSentryException()
		{
			ShowNewFormWithFilter_Warehousing(ModuleIDs.WhsOrder, "OrderEntryForm");
		}

		public void TestShowNewFormWithFilter_WhsReceive_ShouldNotThrowThreadSentryException()
		{
			ShowNewFormWithFilter_Warehousing(ModuleIDs.WhsReceive, "ReceiveEntryForm");
		}

		public void TestShowNewFormWithFilter_WhsStocktake_ShouldNotThrowThreadSentryException()
		{
			ShowNewFormWithFilter_Warehousing(ModuleIDs.WhsStocktake, "StocktakeEntryForm");
		}

		public void TestShowNewFormWithFilter_WhsTransfer_ShouldNotThrowThreadSentryException()
		{
			ShowNewFormWithFilter_Warehousing(ModuleIDs.WhsTransfer, "TransferEntryForm");
		}

		public void TestShowNewFormWithFilter_WhsWorkOrder_ShouldNotThrowThreadSentryException()
		{
			ShowNewFormWithFilter_Warehousing(ModuleIDs.WhsWorkOrder, "WorkOrderEntryForm");
		}

		void ShowNewFormWithFilter_Warehousing(ModuleIdentifier id, string form)
		{
			ShowNewFormWithFilter(id, form, "Warehouse", new ZGuid("{347716F6-9D18-4FA8-81ED-F44B503A5235}"));
		}

		#endregion

		void ShowNewFormWithFilter(ModuleIdentifier id, string formName, string filterDescription, ZGuid guidToFilter)
		{
			ZFilterModule module = null;

			try
			{
				var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);

				var section = BMSTestHelper.CreateBoardSection(id, config.BufferBoard);
				var sectionConfig = (ModuleGridSectionConfiguration)section.Configuration;
				var panelConfig = sectionConfig.PanelConfigurations.First() as ModuleGridSectionPanelConfiguration;
				panelConfig.ShowFilters = true;

				module = ZFilterModule.GetZFilterModule(id);
				var filterBizo = module.FilterBusinessObject;
				filterBizo.AddGuidFilterStrip(filterDescription, guidToFilter);
				ZGuid layoutPk = filterBizo.SaveLayout("Layout_" + id.Name).PK;
				panelConfig.FilterLayout = layoutPk;

				Factory.Save();

				using (VisualBoardsTestCase.ApplyAsyncStrategy(new TaskTrackingAsyncBaseStrategy()))
				using (var form = VisualBoardFormDisplayer.ShowBoard(sectionConfig.Board))
				{
					form.AwaitAll();
					var moduleSelectorControl = form.FindAll<ModuleSelectorControl>().FirstOrDefault(m => m.SectionViewModel.SectionName == sectionConfig.SectionName);
					var grid = moduleSelectorControl.FindAll<ZGrid>().Last();
					var contextMenu = grid.ContextMenu;

					var menuName = "New";
					var menuItem = contextMenu.MenuItems.FindByText(menuName);
					menuItem.PerformClick();

					ZForm objectForm = null;
					try
					{
						objectForm = WaitForFormToOpen<ZForm>(f => f.GetType().ToString().Contains(formName));
						AssertNotNull(string.Format("After clicking '{0}', object form should be shown", menuName), objectForm);
					}
					finally
					{
						objectForm?.Dispose();
					}

					AssertEquals(string.Format("Clicking '{0}' should not cause ThreadSentry Exception", menuName), string.Empty, ErrorReporter.LastMessageReported);
				}
			}
			finally
			{
				if (module != null)
				{
					module.Dispose();
				}

				ErrorReporter.Clear();
			}
		}

		#endregion

		#region Drag And Drop

		[ExpectNoExceptions]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDragAndDrop_WhenTargetingAlreadyOpenBizo_ShouldNotThrowThreadSentryError()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var section = BMSTestHelper.CreateBoardSection(ModuleIDs.Organisation, config.BufferBoard);
			section.MS_FC_Component = config.Buffer.PK;

			var orgSection = BMSTestHelper.CreateBoardSection(config.Buffer, config.BufferBoard);
			orgSection.MS_SectionType = BMConstants.ModuleGridSectionType;

			var sectionConfig = (ModuleGridSectionConfiguration)section.Configuration;
			var panelConfig = sectionConfig.PanelConfigurations.First() as ModuleGridSectionPanelConfiguration;
			panelConfig.ModuleName = ModuleIDs.SalesEnquiry.Name;
			panelConfig.AllowFilterToggle = false;
			panelConfig.AllowOpenModule = false;

			var iEnquire = Factory.NewWithValidTestData<SalesEnquiry>();
			iEnquire.Address1 = "yeet street";

			Factory.Save();

			using (VisualBoardsTestCase.ApplyAsyncStrategy(new TaskTrackingAsyncBaseStrategy()))
			using (var form = VisualBoardFormDisplayer.ShowBoard(config.BufferBoard))
			{
				form.AwaitAll();
				var moduleSelectorControl = form.FindAll<ModuleSelectorControl>().FirstOrDefault();
				AssertNotNull("GIVEN VisualBoard with 'MOD'(Module-Grid) section-type", moduleSelectorControl);

				var filter = moduleSelectorControl.FindAll<ZFilterStripControl>().Single();
				var grid = moduleSelectorControl.FindAll<ZGrid>().Single(c => c.Name == "FilteredGrid");

				grid.Invoke(new Action(() =>
				{
					AssertEquals("GIVEN 1 row", 1, grid.ListManager.List.Count);
					AssertContainsExactElementsInAnyOrder("GIVEN 1 row", new[] { "yeet street" }, grid.ListManager.List.Cast<SalesEnquiry>().Select(enq => enq.Address1.ToString()));
				}));

				CreateUniversalCopyTemplate(grid);
				Application.DoEvents();
				CreateUniversalCopyofSalesEnquiry(grid);
				Application.DoEvents();

				SimulateDragDropOnSomeRow(filter, rowToDragOnto: 1);

				SalesEnquiryForm objectForm = null;

				try
				{
					objectForm = WaitForFormToOpen<SalesEnquiryForm>();
					AssertNotNull("WHEN dragging and dropping THEN object form should be shown", objectForm);

					var formSentry = objectForm.BusinessEntity.Factory.ThreadSentry;
					AssertEquals("WHEN dragging and dropping THEN object form should be open in the main thread", System.Environment.CurrentManagedThreadId, formSentry.OwnerThread.ThreadID);
				}
				finally
				{
					objectForm?.Dispose();
				}
			}
		}

		#region Universal Copy Methods

		void CreateUniversalCopyTemplate(ZGrid grid)
		{
			grid.Select(0);
			var newCopyTemplateMenuItem = grid.ContextMenu.MenuItems.FindByText("New Copy Template", true);
			newCopyTemplateMenuItem.PerformClick();

			Application.DoEvents();

			using (var templateForm = Application.OpenForms.OfType<UniversalCopyTemplateForm>().First())
			{
				templateForm.Show();
				Application.DoEvents();

				var templateGrid = templateForm.FindAll<ZGrid>().First();
				templateGrid.Select(0);

				Application.DoEvents();

				var templateRow = (PropertyCopyTemplateBizo)templateGrid.SelectedElements.First();

				Assert(true);

				templateRow.CopyMethod = "CPY"; // means "Copy the source's value for this field"
				templateForm.Template.CopyTemplateTree.ConfigurationName = "template!";

				templateForm.FireSaveButton();
			}
		}

		void CreateUniversalCopyofSalesEnquiry(ZGrid grid)
		{
			grid.Select(0);
			grid.ContextMenu.MenuItems.FindByText("Universal Copy", true).PerformSelect();

			var makeCopyButton = grid.ContextMenu.MenuItems.FindByText("templ&ate!", true);
			makeCopyButton.PerformClick();

			Application.DoEvents();

			var newCopyForm = Application.OpenForms.OfType<SalesEnquiryForm>().First();
			var enquiry = (SalesEnquiry)newCopyForm.DataSource;
			var enquiryTypeToSet = (CodeDescriptionBool)OrganisationsDataRegistry.Instance.SalesEnquiryTypeList.Value.First();
			enquiry.O1_EnquiryType = enquiryTypeToSet.Code;
			enquiry.O1_CompanyName = "yeet CO.";
			enquiry.O1_ContactName = "yate";

			AssertEquals("We can save our new copy", ContinueWithSave.Yes, newCopyForm.FireSaveButton());
			newCopyForm.Close();
			Application.DoEvents();

			grid.SelectAllElements();

			AssertEquals(2, grid.SelectedElements.Length);
		}

		#endregion

		#region Simulate DragDrop Methods

		void SimulateDragDropOnSomeRow(ZFilterStripControl filter, int rowToDragOnto = 0)
		{
			var cellPosition = filter.Grid.GetCellBounds(rowToDragOnto, 0).Location;
			var point = filter.Grid.PointToScreen(cellPosition);
			SimulateDragDrop(filter, point);
		}

		void SimulateDragDrop(ZFilterStripControl control, Point point)
		{
			var filePath = Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business\TestDocs\200kb.tif");
			var dataObject = new DataObject(DataFormats.FileDrop, new[] { filePath });
			var e = new DragEventArgs(dataObject, 0, point.X, point.Y, DragDropEffects.Copy | DragDropEffects.Move, DragDropEffects.None);

			Invoke("OnDragOver");
			Invoke("OnDragDrop");

			void Invoke(string name)
				=> control.GetType().InvokeMember(name, BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.NonPublic, null, control, new object[] { e });
		}

		#endregion

		#endregion

		#region Menu Items

		[SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		public void TestUniversalPopulateMenuItems_ShouldNotThrowZSecurityThreadSentryException()
		{
			var oldUserContext = Env.CurrentUserContext;
			try
			{
				SystemDataRegistry.Instance.ErrorReportingFormsRunningInBackground.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

				var group = Factory.NewWithValidTestData<GlbGroup>();
				group.GG_Code = "ABC";

				// Get rid of all security rights so SecurityState for each level is implicit.
				var manageSecurityRights = group.SecurityPermissions.Find(s => s.GU_SecurityRight == "Manage");
				var manageSecurityRight = manageSecurityRights.FirstOrDefault();

				group.SecurityPermissions.RemoveAndDelete(manageSecurityRight);

				var staff = group.Staff.AddNew();
				staff.GS_Code = "HAP";
				staff.GS_LoginName = "Harry Potter";

				Factory.Save();

				Env.SetUserContext(new UserContext(staff, Env.CurrentBranch.PK, Env.CurrentDepartment.PK));
				Env.Security.GroupPK = group.PK.ToGuid();

				var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);

				var section = BMSTestHelper.CreateBoardSection(config.Buffer, config.BufferBoard);
				section.MS_SectionType = BMConstants.ModuleGridSectionType;

				var sectionConfig = (ModuleGridSectionConfiguration)section.Configuration;
				var panelConfig = new ModuleGridSectionPanelConfiguration(section);
				sectionConfig.PanelConfigurations.Add(panelConfig);
				panelConfig.ModuleName = ModuleIDs.SalesEnquiry.Name;

				Factory.Save();

				using (VisualBoardsTestCase.ApplyAsyncStrategy(new TaskTrackingAsyncBaseStrategy()))
				using (var form = VisualBoardFormDisplayer.ShowBoard(sectionConfig.Board))
				{
					form.AwaitAll();
					var moduleSelectorControl = form.FindAll<ModuleSelectorControl>().FirstOrDefault();

					var grid = moduleSelectorControl.FindAll<ZGrid>().Single(c => c.Name == "FilteredGrid");

					var universalCopyMenuItems = grid.ContextMenu.MenuItems.FindByText("Universal Copy");

					universalCopyMenuItems.PerformSelect();

					Application.DoEvents();

					AssertEquals("There should be no ThreadSentry Exception", string.Empty, ErrorReporter.LastMessageReported);
				}
			}
			finally
			{
				Env.SetUserContext(oldUserContext);
			}
		}

		#endregion

		#region Grid Actions

		[ExpectNoExceptions]
		public void TestExecuteUniversalCopy_ShouldNotThrowThreadingExceptions()
		{
			SystemDataRegistry.Instance.ErrorReportingFormsRunningInBackground.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);

			var section = BMSTestHelper.CreateBoardSection(config.Buffer, config.BufferBoard);
			section.MS_SectionType = BMConstants.ModuleGridSectionType;

			var sectionConfig = (ModuleGridSectionConfiguration)section.Configuration;
			var panelConfig = new ModuleGridSectionPanelConfiguration(section);
			sectionConfig.PanelConfigurations.Add(panelConfig);
			panelConfig.ModuleName = ModuleIDs.SalesEnquiry.Name;

			var iEnquire = Factory.NewWithValidTestData<SalesEnquiry>();
			iEnquire.Address1 = "yeet street";

			Factory.Save();

			using (VisualBoardsTestCase.ApplyAsyncStrategy(new TaskTrackingAsyncBaseStrategy()))
			using (var form = VisualBoardFormDisplayer.ShowBoard(sectionConfig.Board))
			{
				form.AwaitAll();

				var moduleSelectorControl = form.FindAll<ModuleSelectorControl>().FirstOrDefault();
				AssertNotNull("GIVEN VisualBoard with 'MOD'(Module-Grid) section-type", moduleSelectorControl);

				var grid = moduleSelectorControl.FindAll<ZGrid>().Single(c => c.Name == "FilteredGrid");

				AssertEquals("PRE: We have an initial row to play with", 1, grid.ListManager.List.Count);

				var universalCopyMenuItems = grid.ContextMenu.MenuItems.FindByText("Universal Copy");
				var newCopyTemplateMenuItem = universalCopyMenuItems.MenuItems.FindByText("New Copy Template");

				newCopyTemplateMenuItem.PerformClick();

				Application.DoEvents();

				using (var openedForm = Application.OpenForms.OfType<UniversalCopyTemplateForm>().First())
				{
					AssertNotNull("We opened our universalcopy form", openedForm);
					openedForm.Show();
					Application.DoEvents();
				}
			}
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			var allSystems = Factory.Load<BMSystem>(new ZQuery());
			allSystems.DeleteAll();
			Factory.Save();

			BMSTestHelper.EnableBMSInRegistry();
		}

		class WaitableMainThreadInvocationStrategy : IMainThreadInvocationStrategy
		{
			TaskCompletionSource<object> dispatcherOperationTcs;

			public void BeginInvoke(Action action) => action();

			public T Invoke<T>(Func<T> func)
			{
				dispatcherOperationTcs = new TaskCompletionSource<object>();
				ApplicationDispatcher.Current.BeginInvoke(() =>
				{
					func();
					dispatcherOperationTcs.SetResult(null);
				});
				Application.DoEvents();
				return default;
			}

			public void Wait(Action action)
			{
				action.Invoke();
				Application.DoEvents();
				dispatcherOperationTcs.Task.Wait();
			}
		}

		//TODO: replace WaitForFormToOpen with WaitableMainThreadInvocationStrategy to avoid Thread.Sleep
		T WaitForFormToOpen<T>(Func<T, bool> formPredicate = null) where T : ZForm
		{
			T objectForm = null;
			formPredicate = formPredicate ?? (f => true);

			var maxWait = 200;
			do
			{
				objectForm = Application.OpenForms.OfType<T>().SingleOrDefault(formPredicate);

				Application.DoEvents();
				Thread.Sleep(100);
				maxWait--;
			}
			while (objectForm == null && maxWait > 0);

			Application.DoEvents();
			return objectForm;
		}

		void SetupAccountingEnvironment(TestObjectCreator testObjectCreator, OrgHeader org)
		{
			var accountedFor = new AccountingPeriodTestHelper(Factory);
			accountedFor.SetupPeriods();

			testObjectCreator.CreateAPInvoice<APInvoice>("APINV001", testObjectCreator.AUD, 1.0m, 90m, 10m, 0m, 100m, 10m, 0m, org);
			testObjectCreator.CreateAPInvoice<APInvoice>("APINV002", testObjectCreator.AUD, 1.0m, 90m, 10m, 0m, 100m, 10m, 0m, org);

			ARPayment aRPAY = testObjectCreator.CreateARPayment(1.0m, 100m, ZDateTime.Today, ZDateTime.Today, testObjectCreator.ABIGAS.PK, testObjectCreator.AUDBankAccount.PK);
			aRPAY.AH_LocalExTaxAmount = 100M;
			aRPAY.AH_OSExTaxAmount = 100M;
			aRPAY.AH_OutstandingAmount = 0M;
			aRPAY.AH_FullyPaidDate = ZDateTime.Today;

			APPayment aPPAY = testObjectCreator.CreateAPPayment(1.0m, -100m, ZDateTime.Today, ZDateTime.Today, testObjectCreator.ABIGAS.PK, testObjectCreator.AUDBankAccount.PK);
			aPPAY.AH_LocalExTaxAmount = -100M;
			aPPAY.AH_OSExTaxAmount = -100M;
			aPPAY.AH_OutstandingAmount = 0M;
			aPPAY.AH_FullyPaidDate = ZDateTime.Today;

			TransactionMatchLinkGroup matchLinks = new TransactionMatchLinkGroup(Factory);

			TransactionMatchLink matchlinkForReceivables = matchLinks.AddNew();
			matchlinkForReceivables.AP_AH = aRPAY.PK;
			matchlinkForReceivables.AP_Amount = 100M;

			TransactionMatchLink matchlinkForPayables = matchLinks.AddNew();
			matchlinkForPayables.AP_AH = aPPAY.PK;
			matchlinkForPayables.AP_Amount = -100M;
			TestObjectCreator.SetupMatchLinkMatchDate(matchLinks);
		}
		#endregion
	}
}
