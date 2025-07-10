using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Windows.UI;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(WorkflowManagerTaskTypesRegistryItemEditor))]
	sealed class WorkflowManagerTaskTypesRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		public void TestSetValueFromEditorPane()
		{
			using (ZForm form = new ZForm())
			{
				WorkflowManagerTaskTypesControl editorPane = (WorkflowManagerTaskTypesControl)Editor.NewWinFormsEditorPane();
				form.Controls.Add(editorPane);
				form.Show();

				var workflowDescriptors = (CodeDescriptionPairList)ObjectFactory.Get<IWorkflowDescriptorList>();
				CategorisedWorkflowTaskTypesCollection collection = new CategorisedWorkflowTaskTypesCollection();

				CategorisedWorkflowTaskTypes parent1 = collection.AddNew();
				CategorisedWorkflowTaskTypes parent2 = collection.AddNew();
				CategorisedWorkflowTaskTypes parent3 = collection.AddNew();

				WorkflowTaskType child1 = parent1.TaskTypes.AddNew();
				WorkflowTaskType child2 = parent2.TaskTypes.AddNew();
				WorkflowTaskType child3 = parent3.TaskTypes.AddNew();

				parent1.Code = workflowDescriptors[0].Code;
				parent2.Code = workflowDescriptors[1].Code;
				parent3.Code = "!@#";

				child1.Code = "C1";
				child2.Code = "C2";
				child3.Code = "C3";

				Editor.SetValueFromEditorPane(editorPane, collection);
				collection.SynchroniseWithWorkflowDescriptorList();
				AssertSetAndGetValuesEqual(collection, ((IDataBoundControl)editorPane).DataSource);
			}
		}

		public void TestTaskTypes_AfterTheUpgrade()
		{
			var value = WorkflowDataRegistry.Instance.TaskTypes.Value;

			var wkiTask = value.Cast<CategorisedWorkflowTaskTypes>().Single(s => s.Code == "WKI");
			var shpTask = value.Cast<CategorisedWorkflowTaskTypes>().Single(s => s.Code == "SHP");
			var taskType = shpTask.TaskTypes.AddNew();
			taskType.Code = "BOG";
			value.Remove(wkiTask);

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);

			using (ZForm form = new ZForm())
			{
				var editorPane = (WorkflowManagerTaskTypesControl)Editor.NewWinFormsEditorPane();
				form.Controls.Add(editorPane);
				form.Show();
				Editor.SetValueFromEditorPane(editorPane, WorkflowDataRegistry.Instance.TaskTypes.Value);
				Application.DoEvents();

				var newValue = (CategorisedWorkflowTaskTypesCollection)Editor.GetValueFromEditorPane(editorPane);
				Assert("Precondition: writing worked", newValue.GetTaskTypesFromWorkflowCode("SHP").Cast<WorkflowTaskType>().Any(n => n.Code == "BOG"));
				AssertNotNull("Restore the deleted row.", newValue.Cast<CategorisedWorkflowTaskTypes>().SingleOrDefault(c => c.Code == "WKI"));
			}
		}

		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new WorkflowManagerTaskTypesRegistryItemEditor(RegistryItem.DataType, null, Factory);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(WorkflowManagerTaskTypesControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new WorkflowTaskTypesRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		protected override void AssertSetAndGetValuesEqual(object setValue, object getValue)
		{
			var setCollection = (CategorisedWorkflowTaskTypesCollection)setValue;
			var getCollection = (CategorisedWorkflowTaskTypesCollection)getValue;

			setCollection.SynchroniseWithWorkflowDescriptorList();
			AssertEquals("GetValueFromEditorPane().Count", setCollection.Count, getCollection.Count);
			base.AssertSetAndGetValuesEqual(setValue, getValue);
		}

		protected override object[] GetValidRegistryValues()
		{
			CategorisedWorkflowTaskTypesCollection collection = new CategorisedWorkflowTaskTypesCollection();
			return new object[] { collection };
		}

		protected override bool GetEditorPaneEnabledState(System.Windows.Forms.Control editorPane)
		{
			return !((WorkflowManagerTaskTypesControl)editorPane).ReadOnly;
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
