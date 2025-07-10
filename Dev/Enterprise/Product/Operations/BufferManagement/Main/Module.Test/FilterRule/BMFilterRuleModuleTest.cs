using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.GUI;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(BMFilterRuleModule))]
	class BMFilterRuleModuleTest : ZModuleBasherTest
	{
		public void TestFilterCollection_ApplyToTransfer()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket = BMSTestHelper.CreateBucket(system);
			var buffer = BMSTestHelper.CreateBuffer(system);
			var link = BMSTestHelper.LinkComponents(bucket, buffer);

			var stmFilter = link.FilterRule;

			using (var module = new BMFilterRuleModule())
			{
				var filters = module.FilterBusinessObject;

				var filterStrip = filters.FilterStrips.AddNew("Completion Statement");
				var filter = (ModuleTextFilter)filterStrip.CurrentModuleFilter;
				filter.Property = "Look out!";

				filters.WriteFilterStripsToXml(stmFilter, filters.FilterStrips, new EmptyLayoutsHelper());
			}

			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "DAN", "Daniel Keogh");
			var job = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);

			var workflow1 = job.ProcessHeaders.AddNew();
			var workflow2 = job.ProcessHeaders.AddNew();
			workflow1.FH_FC_CurrentComponent = bucket.PK;
			workflow1.FH_CompletionStatement = "Look out!";
			workflow2.FH_FC_CurrentComponent = bucket.PK;

			Factory.Save();

			ZQuery GetQuery()
			{
				var config = new WorkflowLoadConfig();
				var source = new WorkflowComponentsData(Array.Empty<ZQuery>(), WorkflowLoader.GetStmModuleFilterQuery(link.FilterRule, true), new[] { link.FL_FC_ComponentFrom });
				return WorkflowLoader.GetQuery(config, source);
			}

			var processheaders = Factory.Load<ProcessHeader>(GetQuery());

			AssertCollectionContains(workflow1, processheaders);
			AssertCollectionNotContains(workflow2, processheaders);
		}

		public void TestGetInCompleteTasks_UsesSectionFilter()
		{
			var systemAndBuffer = BMSTestHelper.CreateSystemAndBuffer(Factory, "ORG");

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();

			var board = systemAndBuffer.Item1.Boards.AddNew();
			var section = board.Sections.AddNew();
			section.MS_FC_Component = systemAndBuffer.Item2.PK;

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "QuaidDoug";
			var header = ProcessJobHeader.GetForParent(org, Factory);

			var workflow1 = BMSTestHelper.CreateProcessHeader(header, systemAndBuffer.Item2, "Conan", ZDateTime.Now.AddHours(-1));
			BMSTestHelper.CreateTasks(org, workflow1, Tuple.Create(ProcessTaskStatusCodeList.Codes.Assigned, 4, staff1.GS_Code.ToString(), ZGuid.Empty));

			var workflow2 = BMSTestHelper.CreateProcessHeader(header, systemAndBuffer.Item2, "Eraser", ZDateTime.Now.AddHours(-2));
			BMSTestHelper.CreateTasks(org, workflow2, Tuple.Create(ProcessTaskStatusCodeList.Codes.Assigned, 4, staff1.GS_Code.ToString(), ZGuid.Empty));

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staff1.PK);

			var stmFilter = section.WorkflowFilter;

			using (var module = new BMFilterRuleModule())
			{
				var filters = module.FilterBusinessObject;

				var filterStrip = filters.FilterStrips.AddNew("Completion Statement");
				var filter = (ModuleTextFilter)filterStrip.CurrentModuleFilter;
				filter.Property = "Conan";

				filters.WriteFilterStripsToXml(stmFilter, filters.FilterStrips, new EmptyLayoutsHelper());
			}

			Factory.Save();

			var strategy = new TaskTrackingAsyncBaseStrategy();
			using (VisualBoardsTestCase.ApplyAsyncStrategy(strategy))
			{
				var dataSource = BMSTestHelper.GetDataSource(section);
				var tasks = dataSource.GetIncompleteTasksForCurrentChannels().ToArray();
				strategy.AwaitAll(taskToIgnore: null);

				AssertEquals(1, tasks.Length);

				AssertCollectionContains(tasks, t => t.P9_NotesAsString == "Conan Task 4");
			}
		}

		public void TestAddOrCategoriesUpdatesFilter()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket = BMSTestHelper.CreateBucket(system);
			var board = system.Boards.AddNew();
			var section1 = BMSTestHelper.CreateBoardSection(bucket, board);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = jobHeader.ProcessHeaders.AddNew();
			workflow.FH_CompletionStatement = "LOOK OUT!";

			FilterStripsTestHelper.AddFilterStrips(section1.WorkflowFilter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "LOOK OUT!",
			});

			Factory.Save();

			using (var form = new BMBoardForm(board))
			{
				form.Show();
				Application.DoEvents();
				var workflowFilterTabControl = (ZTabPage)form.Controls.Find("WorkflowFilterTabControl", true).Single();
				var filterStripControl = workflowFilterTabControl.FindAll<FilterRuleFilterStripControl>().Single();
				var tabControl = form.FindAll<ZTabControl>().Single(g => g.Name == "SectionConfigTabControl");

				tabControl.SelectTab("WorkflowFilterTabControl");
				Application.DoEvents();

				AssertEquals(section1.WorkflowFilter, filterStripControl.BindingSource.Current);
				AssertCollectionContains(filterStripControl.FindAll<ZFilterStrip>().Single().FindAll<ZTextBox>(), s => s.Text == "LOOK OUT!");
				var stripControl = filterStripControl.FindAll<ZFilterStrip>().Single();

				AssertEquals(false, section1.WorkflowFilter.HasChanges);

				stripControl.CurrentDataItem.OrCategory = FilterOrCategory.Red;

				AssertEquals(true, section1.WorkflowFilter.HasChanges);

				form.FireSaveButton();
				Application.DoEvents();
			}

			using (var form = new BMBoardForm(board))
			{
				form.Show();
				Application.DoEvents();
				var grid = form.FindAll<ZGrid>().Single(g => g.Name == "BoardSectionsGrid");
				var workflowFilterTabControl = (ZTabPage)form.Controls.Find("WorkflowFilterTabControl", true).Single();
				var filterStripControl = workflowFilterTabControl.FindAll<FilterRuleFilterStripControl>().Single();
				var tabControl = form.FindAll<ZTabControl>().Single(g => g.Name == "SectionConfigTabControl");

				tabControl.SelectTab("WorkflowFilterTabControl");
				Application.DoEvents();

				AssertCollectionContains(filterStripControl.FindAll<ZFilterStrip>().Single().FindAll<ZTextBox>(), s => s.Text == "LOOK OUT!");
				AssertEquals(FilterOrCategory.Red, filterStripControl.FindAll<ZFilterStrip>().Single().OrCategory);
			}
		}

		public void TestSectionListChangedUpdatesFilter()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var group = BMSTestHelper.CreateGroup(Factory, "AAA");
			var bucket = BMSTestHelper.CreateBucket(system);
			var board = system.Boards.AddNew();
			board.MB_Name = "Board1";
			var section1 = BMSTestHelper.CreateBoardSection(bucket, board);
			section1.SectionConfiguration.ReleaseGroupPK = group.PK;
			var section2 = BMSTestHelper.CreateBoardSection(bucket, board);
			section2.SectionConfiguration.ReleaseGroupPK = group.PK;
			section2.Row = 1;
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = jobHeader.ProcessHeaders.AddNew();
			workflow.FH_CompletionStatement = "Harr";
			workflow.FH_GG_ReleaseGroup = group.PK;

			FilterStripsTestHelper.AddFilterStrips(section1.WorkflowFilter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "LOOK OUT!",
			});

			FilterStripsTestHelper.AddFilterStrips(section2.WorkflowFilter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "I DID...",
			});

			Factory.Save();

			using (var form = new BMBoardForm(board))
			{
				form.Show();
				Application.DoEvents();
				var grid = form.FindAll<ZGrid>().Single(g => g.Name == "BoardSectionsGrid");
				var workflowFilterTabControl = (ZTabPage)form.Controls.Find("WorkflowFilterTabControl", true).Single();
				var filterStripControl = workflowFilterTabControl.FindAll<FilterRuleFilterStripControl>().Single();
				var tabControl = form.FindAll<ZTabControl>().Single(g => g.Name == "SectionConfigTabControl");

				tabControl.SelectTab("WorkflowFilterTabControl");
				Application.DoEvents();

				AssertEquals(section1.WorkflowFilter, filterStripControl.BindingSource.Current);
				AssertCollectionContains(filterStripControl.FindAll<ZFilterStrip>().Single().FindAll<ZTextBox>(), s => s.Text == "LOOK OUT!");

				grid.ListManager.Position = 2;
				Application.DoEvents();

				AssertEquals(section2.WorkflowFilter, filterStripControl.BindingSource.Current);
				var textBoxes = filterStripControl.FindAll<ZFilterStrip>().Single().FindAll<ZTextBox>();
				AssertCollectionContains(textBoxes, s => s.Text == "I DID...");

				var stripControl = filterStripControl.FindAll<ZFilterStrip>().Single(s => s.FindAll<ZTextBox>().Any(t => t.Text == "I DID..."));
				((ModuleTextFilter)stripControl.CurrentDataItem.CurrentModuleFilter).Property = "HARR";
				Application.DoEvents();

				grid.ListManager.Position = 0;
				Application.DoEvents();

				AssertEquals(section1.WorkflowFilter, filterStripControl.BindingSource.Current);
				AssertCollectionContains(filterStripControl.FindAll<ZFilterStrip>().Single().FindAll<ZTextBox>(), s => s.Text == "LOOK OUT!");

				grid.ListManager.Position = 2;
				Application.DoEvents();

				AssertEquals(section2.WorkflowFilter, filterStripControl.BindingSource.Current);
				AssertCollectionContains(filterStripControl.FindAll<ZFilterStrip>().Single().FindAll<ZTextBox>(), s => s.Text == "HARR");

				var textBox = form.Controls.Find("NameTextBox", true)[0];
				textBox.Focus();

				form.FireSaveButton();
				Application.DoEvents();

				var newFactory = Factory.CreateNewFactory();
				var loadedSection = newFactory.Load<BMBoardSection>(section2.PK);

				AssertEquals(2, newFactory.Load<ProcessHeader>(new ZQuery()).Length);

				var query = RelatedModuleFiltersHelper.GetFilterQuery(loadedSection.WorkflowFilter);
				var filteredResults = newFactory.Load<ProcessHeader>(query);

				AssertEquals(1, filteredResults.Length);
				VisualBoardsTestCase.AssertSamePK(workflow, filteredResults[0]);
			}
		}

		public void TestDoesNotHaveOperationalActionsPlugins()
		{
			using (var module = new BMFilterRuleModule())
			{
				AssertNull("BMFilterRuleModule should not have the operational actions plugin installed", module.Plugins.GetPlugin(ControllerIDs.OperationalActions));
			}
		}

		public void TestDoesNotHaveOperationalActions()
		{
			using (var module = new BMFilterRuleModule())
			{
				var containsOperationalActions = false;
				Menu.MenuItemCollection actionMenuItems = null;

				foreach (var actionMenu in module.FormActionMenu)
				{
					if (actionMenu.Text == @"&Actions")
					{
						actionMenuItems = actionMenu.MenuItems;
					}
				}

				AssertNotNull("BMFilterRuleModule should contain action menu items", actionMenuItems);

				foreach (MenuItem item in actionMenuItems)
				{
					if (item.Name == "PlaceholderForPluginsActionMenuItems")
					{
						containsOperationalActions = true;
					}
				}

				AssertEquals("BMFilterRuleModule should not support operational actions", false, containsOperationalActions);
			}
		}

		#region Implementation

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.BMFilterRule;
		}

		protected override void SaveUserDefinedFilter(FilterStripBusinessObject filterBizo)
		{
			using (var module = (ZFilterModule)ZModuleFactory.Instance.Create(ModuleIDs.ProcessHeader))
			{
				GetFilterForUserDefinedFilterTest(module.FilterBusinessObject);
				FilterStripsTestHelper.SaveFilterLayout(module.FilterBusinessObject, "User-Defined", true, false, isUserDefinedFilter: true);
			}
		}

		#endregion
	}
}
