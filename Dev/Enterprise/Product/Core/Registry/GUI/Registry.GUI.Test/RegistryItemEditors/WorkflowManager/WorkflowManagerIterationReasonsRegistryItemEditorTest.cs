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
	[TestedType(typeof(WorkflowManagerIterationReasonsRegistryItemEditor))]
	sealed class WorkflowManagerIterationReasonsRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		public void TestSetValueFromEditorPane()
		{
			using (var form = new ZForm())
			{
				var editorPane = (WorkflowManagerIterationReasonsControl)Editor.NewWinFormsEditorPane();
				form.Controls.Add(editorPane);
				form.Show();

				var workflowDescriptors = (CodeDescriptionPairList)ObjectFactory.Get<IWorkflowDescriptorList>();
				var collection = new CategorisedWorkflowIterationReasonsCollection();

				var parent1 = collection.AddNew();
				var parent2 = collection.AddNew();
				var parent3 = collection.AddNew();

				var child1 = parent1.IterationReasons.AddNew();
				var child2 = parent2.IterationReasons.AddNew();
				var child3 = parent3.IterationReasons.AddNew();

				parent1.Code = workflowDescriptors[0].Code;
				parent2.Code = workflowDescriptors[1].Code;
				parent3.Code = "!@#";

				child1.Code = "C1";
				child2.Code = "C2";
				child3.Code = "C3";

				Editor.SetValueFromEditorPane(editorPane, collection);
				collection.SynchroniseWithWorkflowDescriptorList(); // setting value for editor pane includes preparing data for binding which in turn runs synchronisation with workflow descriptors; we need to run this synchronisation for the initial collection too otherwise the data source and the collection will differ
				AssertSetAndGetValuesEqual(collection, ((IDataBoundControl)editorPane).DataSource);
			}
		}

		public void TestQualityIterationReasons_AfterTheUpgrade()
		{
			var value = WorkflowDataRegistry.Instance.IterationReasons.Value;

			var wkiReasons = value.Cast<CategorisedWorkflowIterationReasons>().Single(s => s.Code == "WKI");
			var shpReasons = value.Cast<CategorisedWorkflowIterationReasons>().Single(s => s.Code == "SHP");
			var taskType = shpReasons.IterationReasons.AddNew();
			taskType.Code = "BOG";
			value.Remove(wkiReasons);

			WorkflowDataRegistry.Instance.IterationReasons.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);

			using (ZForm form = new ZForm())
			{
				var editorPane = (WorkflowManagerIterationReasonsControl)Editor.NewWinFormsEditorPane();
				form.Controls.Add(editorPane);
				form.Show();
				Editor.SetValueFromEditorPane(editorPane, WorkflowDataRegistry.Instance.IterationReasons.Value); // display the value on the pane
				Application.DoEvents();

				var newValue = (CategorisedWorkflowIterationReasonsCollection)Editor.GetValueFromEditorPane(editorPane);
				Assert("Precondition: writing worked", newValue.GetIterationReasonsFromWorkflowCode("SHP").Cast<WorkflowIterationReason>().Any(n => n.Code == "BOG"));
				AssertNotNull("Should restore the deleted row", newValue.Cast<CategorisedWorkflowIterationReasons>().SingleOrDefault(c => c.Code == "WKI"));
			}
		}

		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new WorkflowManagerIterationReasonsRegistryItemEditor(RegistryItem.DataType, null, Factory);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(WorkflowManagerIterationReasonsControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new WorkflowIterationReasonsRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		protected override void AssertSetAndGetValuesEqual(object setValue, object getValue)
		{
			var setCollection = (CategorisedWorkflowIterationReasonsCollection)setValue;
			var getCollection = (CategorisedWorkflowIterationReasonsCollection)getValue;

			setCollection.SynchroniseWithWorkflowDescriptorList();
			AssertEquals("GetValueFromEditorPane().Count", setCollection.Count, getCollection.Count);
			base.AssertSetAndGetValuesEqual(setValue, getValue);
		}

		protected override object[] GetValidRegistryValues()
		{
			CategorisedWorkflowIterationReasonsCollection collection = new CategorisedWorkflowIterationReasonsCollection();
			return new object[] { collection };
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((WorkflowManagerIterationReasonsControl)editorPane).ReadOnly;
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
