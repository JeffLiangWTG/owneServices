using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	[TestedType(typeof(FilterStripsForm))]
	public class FilterStripsFormTest : ZFormBasherTest
	{
		public void TestFilterStripsFormShowsErrors()
		{
			using (var form = GetFormToBashCore())
			{
				form.Show();
				Application.DoEvents();

				var button = form.FindAll<ZFilterStripAddButton>().Single().FindAll<ZButton>().Single();
				button.PerformClick();
				Application.DoEvents();

				var dropEdits = form.FindAll<ZFilterStripDropEdit>();
				AssertEquals(2, dropEdits.Count());

				var textbox = dropEdits.Last();
				textbox.Text = "Custom SQL Filter";
				Application.DoEvents();
				dropEdits.First().Focus();
				Application.DoEvents();

				var sqlTextBox = form.FindAll<ZTextBox>().Single(t => t.IsDynamicMultiline);
				sqlTextBox.Text = "AAAAAAAA";
				sqlTextBox.Focus();
				Application.DoEvents();
				dropEdits.First().Focus();
				Application.DoEvents();

				var applyButton = (ZButton)form.Controls.Find("ApplyButton", false).Single();
				applyButton.PerformClick();
				Application.DoEvents();

				AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);

				AssertEquals(@"The Custom SQL Filter should be in the format of F1 = 'Value' AND F2 = 'Value 2'.
What you have typed is: AAAAAAAA
Error message: An expression of non-boolean type specified in a context where a condition is expected, near 'AND'.
Filters: AAAAAAAA", UnitTestUserNotification.Instance.PreviousMessages[1].Text);
			}
		}

		public void TestFilterStripsForm_WhenInitializedWithManyWorkflowFilters()
		{
			var viewModel = CreateViewModel();
			AddNineteenFilterStripsToLayout(viewModel.WorkflowFilter, ProcessHeader.ModuleFilterConstants.CriticalHandover);

			using (var form = new FilterStripsForm(viewModel))
			{
				form.Show();
				Application.DoEvents();

				var tabControl = form.FindAll<ZTabControl>().Single();
				var workflowTabPage = tabControl.AllTabPages.SingleOrDefault(l => l.Text == "Workflow Filters");

				AssertEquals("There should be 19 filter strips on the workflow tab page.", 19, workflowTabPage.FindAll<ZFilterStrip>().Count());
				AssertEquals("There should be 19 filter strips on the workflow tab page with the correct description.", 19, workflowTabPage.FindAll<ZFilterStrip>().Select(l => l.Name == "Description").Count());
				AssertFormSize(form);
			}
		}

		public void TestFilterStripsForm_WhenInitializedWithManyTaskFilters()
		{
			var viewModel = CreateViewModel();
			AddNineteenFilterStripsToLayout(viewModel.TaskFilter, "Description");

			using (var form = new FilterStripsForm(viewModel))
			{
				form.Show();
				Application.DoEvents();

				AssertFormSize(form);

				var tabControl = form.FindAll<ZTabControl>().Single();
				tabControl.SelectNextTabPage();
				Application.DoEvents();
				var taskTabPage = tabControl.AllTabPages.SingleOrDefault(l => l.Text == "Task Filters");

				AssertEquals("There should be 19 filter strips on the tasks tab page.", 19, taskTabPage.FindAll<ZFilterStrip>().Count());
				AssertEquals("There should be 19 filter strips on the tasks tab page with the correct description.", 19, taskTabPage.FindAll<ZFilterStrip>().Select(l => l.Name == "Description").Count());
				AssertFormSize(form);
			}
		}

		public void TestFilterStripsForm_ShouldHaveTabsForWorkflowAndTaskFilters()
		{
			using (var form = GetFormToBashCore())
			{
				var tabControl = form.FindAll<ZTabControl>().Single();
				AssertNotNull(tabControl.AllTabPages.SingleOrDefault(x => x.Text == "Workflow Filters"));
				AssertNotNull(tabControl.AllTabPages.SingleOrDefault(x => x.Text == "Task Filters"));
			}
		}

		public void TestFilterStripsForm_LocationOfFirstStripOnWorkflowTab()
		{
			var viewModel = CreateViewModel();
			using (var form = new FilterStripsForm(viewModel))
			{
				form.Show();
				Application.DoEvents();

				var tabControl = form.FindAll<ZTabControl>().Single();
				var workflowTabPage = tabControl.AllTabPages.SingleOrDefault(l => l.Text == "Workflow Filters");
				Application.DoEvents();

				var addButton = workflowTabPage.FindAll<ZFilterStripAddButton>().Single().FindAll<ZButton>().Single();
				var locationOfFirstStrip = workflowTabPage.FindAll<ZFilterStrip>().Single().Location;
				for (int i = 0; i < 9; i++)
				{
					addButton.PerformClick();
					Application.DoEvents();
				}

				workflowTabPage.FindAll<FilterRuleFilterStripControl>().Single().CurrentGroupName = "The FLASH";
				addButton.PerformClick();
				Application.DoEvents();

				workflowTabPage.FindAll<KPanel>().FirstOrDefault().VerticalScroll.Value = 0;
				Application.DoEvents();

				var locationOfFirstStripAfterScrollingUp = workflowTabPage.FindAll<ZFilterStrip>().First().Location;
				AssertEquals("There should be 11 filter strips on the workflow tab page (1 from the beginning, then we add 9, then 1 from the new group).", 11, workflowTabPage.FindAll<ZFilterStrip>().Count());
				AssertEquals("After adding 9 strips, then a group, then scrolling up, the position of the first strip should be the same.", locationOfFirstStrip, locationOfFirstStripAfterScrollingUp);
			}
		}

		public void TestFilterStripsForm_LocationOfFirstStripOnTasksTab()
		{
			var viewModel = CreateViewModel();
			using (var form = new FilterStripsForm(viewModel))
			{
				form.Show();
				Application.DoEvents();

				var tabControl = form.FindAll<ZTabControl>().Single();
				tabControl.SelectNextTabPage();
				var taskTabPage = tabControl.AllTabPages.SingleOrDefault(l => l.Text == "Task Filters");
				Application.DoEvents();

				var addButton = taskTabPage.FindAll<ZFilterStripAddButton>().Single().FindAll<ZButton>().Single();
				var locationOfFirstStrip = taskTabPage.FindAll<ZFilterStrip>().Single().Location;
				for (int i = 0; i < 9; i++)
				{
					addButton.PerformClick();
					Application.DoEvents();
				}

				taskTabPage.FindAll<FilterRuleFilterStripControl>().Single().CurrentGroupName = "Reverse FLASH";
				addButton.PerformClick();
				Application.DoEvents();

				taskTabPage.FindAll<KPanel>().FirstOrDefault().VerticalScroll.Value = 0;
				Application.DoEvents();

				var locationOfFirstStripAfterScrollingUp = taskTabPage.FindAll<ZFilterStrip>().First().Location;
				AssertEquals("There should be 11 filter strips on the tasks tab page (1 from the beginning, then we add 9, then 1 from the new group).", 11, taskTabPage.FindAll<ZFilterStrip>().Count());
				AssertEquals("After adding 9 strips, then a group, then scrolling up, the position of the first strip should be the same.", locationOfFirstStrip, locationOfFirstStripAfterScrollingUp);
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var viewModel = CreateViewModel();

			return new FilterStripsForm(viewModel);
		}

		StmModuleFilterViewModel CreateViewModel()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var workflowFilter = config.BufferSection.WorkflowFilter;
			var taskFilter = config.BufferSection.TaskFilter;

			return new StmModuleFilterViewModel(workflowFilter, taskFilter, "Test Section");
		}

		void AddNineteenFilterStripsToLayout(StmModuleFilter layout, string filterStripDescription)
		{
			FilterStripsTestHelper.AddFilterStrips(layout,
				new FilterStripsTestHelper.FilterStripDefinition { FilterStripName = filterStripDescription },
				new FilterStripsTestHelper.FilterStripDefinition { FilterStripName = filterStripDescription },
				new FilterStripsTestHelper.FilterStripDefinition { FilterStripName = filterStripDescription },
				new FilterStripsTestHelper.FilterStripDefinition { FilterStripName = filterStripDescription },
				new FilterStripsTestHelper.FilterStripDefinition { FilterStripName = filterStripDescription },
				new FilterStripsTestHelper.FilterStripDefinition { FilterStripName = filterStripDescription },
				new FilterStripsTestHelper.FilterStripDefinition { FilterStripName = filterStripDescription },
				new FilterStripsTestHelper.FilterStripDefinition { FilterStripName = filterStripDescription },
				new FilterStripsTestHelper.FilterStripDefinition { FilterStripName = filterStripDescription },
				new FilterStripsTestHelper.FilterStripDefinition { FilterStripName = filterStripDescription },
				new FilterStripsTestHelper.FilterStripDefinition { FilterStripName = filterStripDescription },
				new FilterStripsTestHelper.FilterStripDefinition { FilterStripName = filterStripDescription },
				new FilterStripsTestHelper.FilterStripDefinition { FilterStripName = filterStripDescription },
				new FilterStripsTestHelper.FilterStripDefinition { FilterStripName = filterStripDescription },
				new FilterStripsTestHelper.FilterStripDefinition { FilterStripName = filterStripDescription },
				new FilterStripsTestHelper.FilterStripDefinition { FilterStripName = filterStripDescription },
				new FilterStripsTestHelper.FilterStripDefinition { FilterStripName = filterStripDescription },
				new FilterStripsTestHelper.FilterStripDefinition { FilterStripName = filterStripDescription },
				new FilterStripsTestHelper.FilterStripDefinition { FilterStripName = filterStripDescription }
				);
		}

		void AssertFormSize(FilterStripsForm form)
		{
			AssertEquals("The form should be resizeable", FormBorderStyle.Sizable, form.FormBorderStyle);
			AssertEquals("The form should be resizeable so maximum size should not be set", new Size(0, 0), form.MaximumSize);
			AssertEquals("The form should not have magically changed size", form.MinimumSize.Height, form.Height, 2m);
			AssertEquals("The form should not have magically changed size", form.MinimumSize.Width, form.Width, 2m);
		}

		#endregion
	}
}
