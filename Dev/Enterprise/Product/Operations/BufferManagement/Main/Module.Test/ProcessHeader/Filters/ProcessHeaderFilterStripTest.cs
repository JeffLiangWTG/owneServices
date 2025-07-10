using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.Module.Test
{
	public class ProcessHeaderFilterStripTest : BMSTestCaseWithFactory
	{
		public void TestCheckBoxListControls_FiresHasChanges()
		{
			BMSTestHelper.AddTaskTypesToRegistry("ORG", "BEN", "PET");
			Factory.Save();

			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "DES", "Banana");
			var tagmag = BMSTestHelper.CreateTagMagnitude(tagGroup, "HAJ", "Description");
			var tagRule = BMSTestHelper.CreateTagRule(tagmag, "The pool Rool", TagRuleActionTypeList.Codes.AddTag);
			var filter = tagRule.Filter;

			using (var form = new ZForm())
			using (var control = new BMFilterStripWrapperControl())
			{
				form.Controls.Add(control);
				form.Show();

				control.SetDataBinding(tagRule, "Filter");
				var stripControl = control.FindAll<FilterRuleFilterStripControl>().Single();
				stripControl.AddNewFilterStrip();

				var filterStrip = stripControl.FindAll<ZFilterStrip>().First();
				filterStrip.CurrentDataItem.FilterDescription = TaskStatusFilter.Schema.Identifier;

				AssertEquals("Adding the filter strip HasChanges", true, filter.HasChanges);
				Factory.Save();

				var checkLists = filterStrip.FindAll<ZCheckedListBox>().ToArray();
				AssertEquals(2, checkLists.Length);

				AssertEquals(false, filter.HasChanges);

				checkLists[0].BindingItems.First().Value = true;
				AssertEquals("Checking a checkbox fires HasChanges", true, filter.HasChanges);

				Factory.Save();

				checkLists[1].BindingItems.First().Value = true;
				AssertEquals("Checking a checkbox fires HasChanges", true, filter.HasChanges);
			}
		}

		public void TestProcessHeaderFilterStrip_Draggable()
		{
			BMSTestHelper.AddTaskTypesToRegistry("ORG", "BEN", "PET");
			Factory.Save();

			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "DES", "Banana");
			var tagmag = BMSTestHelper.CreateTagMagnitude(tagGroup, "HAJ", "Description");
			var tagRule = BMSTestHelper.CreateTagRule(tagmag, "The pool Rool", TagRuleActionTypeList.Codes.AddTag);

			using (var form = new ZForm())
			using (var control = new BMFilterStripWrapperControl())
			{
				form.Controls.Add(control);
				form.Show();

				control.SetDataBinding(tagRule, "Filter");
				var stripControlText = control.FindAll<TextBox>().First();

				AssertEquals("Adding the filter strip HasChanges", false, stripControlText.AllowDrop);

				stripControlText.AllowDrop = true;
				AssertEquals("Adding the filter strip HasChanges", true, stripControlText.AllowDrop);
				Factory.Save();
			}
		}

		public void TestJobTextPropertyFilterControlsTabIndex()
		{
			BMSTestHelper.AddTaskTypesToRegistry("ORG", "BEN", "PET");
			Factory.Save();

			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "DES", "Banana");
			var tagmag = BMSTestHelper.CreateTagMagnitude(tagGroup, "HAJ", "Description");
			var tagRule = BMSTestHelper.CreateTagRule(tagmag, "The Pool Room", TagRuleActionTypeList.Codes.AddTag);
			var filter = tagRule.Filter;

			using (var form = new ZForm())
			using (var control = new BMFilterStripWrapperControl())
			{
				form.Controls.Add(control);
				form.Show();

				control.SetDataBinding(tagRule, "Filter");
				var stripControl = control.FindAll<FilterRuleFilterStripControl>().Single();
				stripControl.AddNewFilterStrip();

				var filterStrip = stripControl.FindAll<ZFilterStrip>().First();
				filterStrip.CurrentDataItem.FilterDescription = "Job Property";

				var comparisonControl = filterStrip.FindSingle<ZDropEdit>(x => x.BindTo == "ComparisonOperator");
				var workflowTypeDropEdit = filterStrip.FindSingle<ZDropEdit>(x => x.BindTo == JobTextPropertyFilter.Schema.WorkflowTypeCode);
				var nameDropEdit = filterStrip.FindSingle<ZDropEdit>(x => x.BindTo == JobTextPropertyFilter.Schema.JobPropertyName);
				var textBox = filterStrip.FindSingle<ZTextBox>(x => x.BindTo == "Property");

				CombineAssertions("The correct tab index sequence should be as follows:", () =>
				{
					AssertEquals(0, comparisonControl.TabIndex);
					AssertEquals(1, workflowTypeDropEdit.TabIndex);
					AssertEquals(2, nameDropEdit.TabIndex);
					AssertEquals(3, textBox.TabIndex);
				});
			}
		}

		public void TestWorkflowCategoryFilterControlsTabIndex()
		{
			BMSTestHelper.AddTaskTypesToRegistry("ORG", "BEN", "PET");
			Factory.Save();

			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "DES", "Saunders");
			var tagmag = BMSTestHelper.CreateTagMagnitude(tagGroup, "HAJ", "Pilgrimage");
			var tagRule = BMSTestHelper.CreateTagRule(tagmag, "Mecca", TagRuleActionTypeList.Codes.AddTag);
			var filter = tagRule.Filter;

			using (var form = new ZForm())
			using (var control = new BMFilterStripWrapperControl())
			{
				form.Controls.Add(control);
				form.Show();

				control.SetDataBinding(tagRule, "Filter");
				var stripControl = control.FindAll<FilterRuleFilterStripControl>().Single();
				stripControl.AddNewFilterStrip();

				var filterStrip = stripControl.FindAll<ZFilterStrip>().First();
				filterStrip.CurrentDataItem.FilterDescription = "Workflow Category";

				var workflowTypeDropEdit = filterStrip.FindSingle<ZDropEdit>(x => x.BindTo == WorkflowCategoryFilter.Schema.WorkflowType);
				var categoryDropEdit = filterStrip.FindSingle<ZDropEdit>(x => x.BindTo == WorkflowCategoryFilter.Schema.WorkflowCategory);

				CombineAssertions("The correct tab index sequence should be as follows:", () =>
				{
					AssertEquals(0, workflowTypeDropEdit.TabIndex);
					AssertEquals(1, categoryDropEdit.TabIndex);
				});
			}
		}

		public void TestTaskStatusFilterStrip_ShouldHaveExpectedHeight_AtStandardDpi()
		{
			AssertTaskStatusFilterStrip(100, 105);
		}

		public void TestTaskStatusFilterStrip_ShouldHaveExpectedHeight_AtZoomedDpi()
		{
			AssertTaskStatusFilterStrip(150, 158);
		}

		static void AssertTaskStatusFilterStrip(float dpiScaling, int expectedHeight)
		{
			using (var module = ZFilterModule.GetZFilterModule(ModuleIDs.ProcessHeader))
			using (ControlDpiScalingHelper.OverrideDPI_ForTesting(dpiScaling, dpiScaling))
			using (var popup = (EmbeddedModulePopup)module.ShowPopup())
			{
				popup.Show();
				Application.DoEvents();

				var stripControl = popup.FindSingle<StripControl>();

				var filterStrip = stripControl.FindSingle<ZFilterStrip>();
				filterStrip.CurrentDataItem.FilterDescription = TaskStatusFilter.Schema.Identifier;
				Application.DoEvents();

				AssertEquals("The filter strip's height should be scaled according to the display's DPI settings.", expectedHeight, filterStrip.Height);
			}
		}
	}
}
