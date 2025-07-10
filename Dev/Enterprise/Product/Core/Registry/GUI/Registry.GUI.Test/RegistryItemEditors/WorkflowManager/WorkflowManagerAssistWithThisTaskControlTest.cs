using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(WorkflowManagerAssistWithThisTaskControl))]
	sealed class WorkflowManagerAssistWithThisTaskControlTest : Enterprise.Registry.GUI.Testing.RegistryZUserControlTestCase
	{
		public void TestParentGridIsAlwaysReadOnly()
		{
			using (var form = new ZForm())
			{
				using (var control = new WorkflowManagerAssistWithThisTaskControl())
				{
					form.Controls.Add(control);

					control.ReadOnly = true;
					AssertEquals("ParentGrid.ReadOnly", true, control.FindSingle<ZGrid>("ParentGrid").ReadOnly);

					control.ReadOnly = false;
					AssertEquals("ParentGrid.ReadOnly", true, control.FindSingle<ZGrid>("ParentGrid").ReadOnly);
				}
			}
		}

		public void TestGridsToggleReadOnly()
		{
			using (var form = new ZForm())
			{
				using (var control = new WorkflowManagerAssistWithThisTaskControl())
				{
					form.Controls.Add(control);

					control.ReadOnly = true;
					AssertEquals(true, control.FindSingle<ZTextBox>("WorkflowTypeBox").ReadOnly);
					AssertEquals(true, control.FindSingle<ZDropEdit>("TaskTypeBox").ReadOnly);
					AssertEquals(true, control.FindSingle<ZCalcEdit>("LowEstimateMinutesBox").ReadOnly);
					AssertEquals(true, control.FindSingle<ZCalcEdit>("VariationFactorBox").ReadOnly);

					control.ReadOnly = false;
					AssertEquals(true, control.FindSingle<ZTextBox>("WorkflowTypeBox").ReadOnly);
					AssertEquals(false, control.FindSingle<ZDropEdit>("TaskTypeBox").ReadOnly);
					AssertEquals(false, control.FindSingle<ZCalcEdit>("LowEstimateMinutesBox").ReadOnly);
					AssertEquals(false, control.FindSingle<ZCalcEdit>("VariationFactorBox").ReadOnly);
				}
			}
		}

		public void TestEditProperties_ShouldActivateSaveButton()
		{
			MasterFilesTestHelper.AddTaskTypesToRegistry("ORG", new Dictionary<string, string> { { "INV", "Investigation" }, { "AST", "Assist" } });
			MasterFilesTestHelper.SetUpAssistWithThisTaskInRegistry("ORG", "AST", 20, 4);
			ZFormModaliser.ShowDialogsInTest = true;

			using (var testForm = new RegistryFormTest.RegistryFormForTest(WorkflowDataRegistry.Instance.AssistWithThisTask))
			{
				testForm.Show();
				Application.DoEvents();

				var registriesTreeView = testForm.GetRegistriesTreeView();
				registriesTreeView.ExpandAll();
				var node = registriesTreeView.Nodes.Find(nameof(WorkflowDataRegistry.Instance.AssistWithThisTask), searchAllChildren: true).Single();
				registriesTreeView.SelectedNode = node;
				Application.DoEvents();

				var workflowTypesGrid = testForm.FindSingle<ZGrid>("ParentGrid");
				var categories = (CategorisedAssistWithThisTaskSettingCollection)workflowTypesGrid.DataSource;
				var orgCategory = (CategorisedAssistWithThisTaskSetting)categories.FindByCode("ORG");
				workflowTypesGrid.SelectSingleElement(orgCategory);
				Application.DoEvents();

				var saveButton = testForm.GetSaveButton();
				AssertEquals("Precondition: The Save button shouldn't be enabled yet.", false, saveButton.Enabled);

				var taskTypeBox = testForm.FindSingle<ZDropEdit>("TaskTypeBox");
				AssertEquals("AST", taskTypeBox.Text);

				orgCategory.Setting.TaskType = "INV";
				Application.DoEvents();

				AssertEquals("INV", taskTypeBox.Text);
				AssertEquals("Setting a value should enable the save button.", true, saveButton.Enabled);
			}
		}

		protected override RegistryZUserControl GetNewControl()
		{
			return new WorkflowManagerAssistWithThisTaskControl();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return control.FindSingle<ZDropEdit>("TaskTypeBox").ReadOnly;
		}

		protected override IBusiness GetNewBusinessEntity()
		{
			return new CategorisedAssistWithThisTaskSettingCollection(initialiseWithWorkflowDescriptorList: true);
		}

		protected override void SetUp()
		{
			base.SetUp();

			AssistWithThisTaskSettingForOneWorkflowTest.SetUpTaskTypesForAssistWithThisTaskTests();
		}
	}
}
