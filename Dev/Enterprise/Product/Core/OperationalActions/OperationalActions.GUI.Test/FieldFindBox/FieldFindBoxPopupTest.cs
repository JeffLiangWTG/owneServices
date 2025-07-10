using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Services.OperationalActions.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;
using static Enterprise.ZArchitecture.GUI.Testing.TreeFindFormTest;

namespace Enterprise.Services.OperationalActions.GUI.Testing
{
	[TestedType(typeof(FieldFindBoxPopup))]
	sealed class FieldFindBoxPopupTest : ZFormBasherTest
	{
		#region DummyFieldFindBox

		class DummyFieldFindBox : IFieldFindBox
		{
			public string Value { get; set; }
			public bool AllowReadOnly { get; set; }
			public Type RootType { get; set; }
		}

		#endregion

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new FieldFindBoxPopup(new DummyFieldFindBox() { RootType = typeof(DummyBusinessObjectWithDocumentSupport) });
		}

		protected override bool ShouldIgnoreMissingBindingMember(Control control)
		{
			if (control.Name == "fieldTree")
			{
				return true;
			}
			return base.ShouldIgnoreMissingBindingMember(control);
		}

		#endregion

		public void TestFindBoxSearchWontGetErrorWhenRootTypeIsForwardingModuleShipment()
		{
			using (var fieldFindBoxPopup = new FieldFindBoxPopup(new DummyFieldFindBox() { RootType = typeof(ForwardingModuleShipment) }))
			{
				var fieldTree = (FieldTreeView)((KSplitContainer)fieldFindBoxPopup.Controls["midPanel"].Controls["treeDescriptionSplitPanel"]).Panel1.Controls["fieldTree"];

				using (var treeViewFindForm = new TreeViewFindFormForTest(fieldTree))
				{
					ZFormModaliser.Show(treeViewFindForm, fieldFindBoxPopup);

					treeViewFindForm.GetFindTextBox().Text = "blablabla";

					var findNextButton = treeViewFindForm.GetFindNextButton();
					var findPrevButton = treeViewFindForm.GetFindPreviousButton();
					AssertNoExceptionThrown(() =>
					{
						findNextButton.PerformClick();
						findPrevButton.PerformClick();
					});
				}
			}
		}

		public void TestStmPrintQueuePropertiesShownInThisForm()
		{
			using (var fieldFindBoxPopup = new FieldFindBoxPopup(new DummyFieldFindBox() { RootType = typeof(StmPrintQueue) }))
			{
				var fieldTree = (FieldTreeView)((KSplitContainer)fieldFindBoxPopup.Controls["midPanel"].Controls["treeDescriptionSplitPanel"]).Panel1.Controls["fieldTree"];

				var nodeNamesExpected = new List<string>() { "SQ_AllowPrinting", "SQ_ColumnScale", "SQ_DisplayName", "SQ_IsRollPaper", "SQ_LeftMargin", "SQ_PrintLanguage", "SQ_RowScale", "SQ_Scale", "SQ_SupressLetterhead", "SQ_TopMargin" };
				var nodeNamesActual = fieldTree.Nodes.Cast<TreeNode>().Select(t => t.Text);
				AssertContainsExactElementsInAnyOrder(nodeNamesExpected, nodeNamesActual);
			}
		}

		public void TestMilestoneTriggersTasksPropertiesShownInThisForm()
		{
			using (var fieldFindBoxPopup = new FieldFindBoxPopup(new DummyFieldFindBox() { RootType = typeof(DummyWithWorkflow) }))
			{
				var fieldTree = (FieldTreeView)((KSplitContainer)fieldFindBoxPopup.Controls["midPanel"].Controls["treeDescriptionSplitPanel"]).Panel1.Controls["fieldTree"];
				fieldFindBoxPopup.Show();
				fieldTree.Nodes.Cast<TreeNode>().First(x => x.Text == "WorkflowItems").Expand();
				Application.DoEvents();

				var nodeNamesActual = fieldTree.Nodes.Cast<TreeNode>().First(x => x.Text == "WorkflowItems").Nodes.Cast<TreeNode>().Select(t => t.Text);
				AssertCollectionContains("Milestones", nodeNamesActual);
				AssertCollectionContains("Triggers", nodeNamesActual);
				AssertCollectionContains("Tasks", nodeNamesActual);
			}
		}

		public void TestSearchForTaskPivotsInOrganisationActionSupporter()
		{
			OperationalActionSupporter actionSupporter = null;
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.Organisation))
			{
				actionSupporter = ((IOperationalActionSupportable)module).OperationalActionSupporter;
			}
			var context = new OperationalActionContext(actionSupporter, "Organisation", "ORG");
			var manager = new OperationalActionManager(Factory, context);

			using (var form = new OperationalActionCustomizationForm(manager))
			{
				form.Show();
				Application.DoEvents();

				var action = Factory.NewWithValidTestData<OperationalAction>();
				action.Context = context;
				manager.Actions.Add(action);
				manager.Actions[0].FieldDescriptors.Add(new OperationalActionFieldDescriptor(action));
				manager.Actions[0].FieldDescriptors[0].FieldName = "Name";
				Factory.Save();

				var cusCon = (CustomisationControl)typeof(OperationalActionCustomizationForm).InvokeMember(
					"customisationControl",
					BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance,
					null, form, Array.Empty<object>());

				var settingsTabControl = (ZTabControl)((KSplitContainer)cusCon.Controls["mainSplitContainer"]).Panel2.Controls["settingsTabControl"];
				var fieldsTabPage = (ZTabPage)settingsTabControl.Controls["fieldsTabPage"];
				settingsTabControl.SelectedTab = fieldsTabPage;
				var fieldsControl = fieldsTabPage.Controls["fieldsControl"];

				var fieldFilter = (FieldFilterControl)((KSplitContainer)fieldsControl.Controls["fieldSplitContainer"]).Panel2.Controls["fieldFilter"];
				var insertFieldButton = (ZButton)fieldFilter.Controls["insertFieldButton"];
				insertFieldButton.PerformClick();

				var fieldFindBoxPopup = ZFormModaliser.LastFormShownForTest;

				var fieldTree = (FieldTreeView)((KSplitContainer)fieldFindBoxPopup.Controls["midPanel"].Controls["treeDescriptionSplitPanel"]).Panel1.Controls["fieldTree"];

				using (var treeViewFindForm = new TreeViewFindFormForTest(fieldTree))
				{
					ZFormModaliser.Show(treeViewFindForm, fieldFindBoxPopup);

					treeViewFindForm.GetFindTextBox().Text = "Task";

					var findNextButton = treeViewFindForm.GetFindNextButton();
					AssertNoExceptionThrown(() =>
					{
						for (int i = 0; i < 100; i++)
						{
							findNextButton.PerformClick();
						}
					});
				}
			}
		}
	}
}
