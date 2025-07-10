using System;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	[TestedType(typeof(WorkflowCategoriesRegistryItemEditor))]
	class WorkflowCategoriesRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		public void TestSetValueFromEditorPane()
		{
			using (var form = new ZForm())
			{
				var editorPane = (WorkflowCategoriesControl)Editor.NewWinFormsEditorPane();
				form.Controls.Add(editorPane);
				form.Show();

				var workflowDescriptors = (CodeDescriptionPairList)ObjectFactory.Get<IWorkflowDescriptorList>();
				var collection = new CategorisedWorkflowCategoriesCollection();

				var parent1 = collection.AddNew();
				var parent2 = collection.AddNew();
				var parent3 = collection.AddNew();

				var child1 = parent1.Categories.AddNew();
				var child2 = parent2.Categories.AddNew();
				var child3 = parent3.Categories.AddNew();

				parent1.Code = workflowDescriptors[0].Code;
				parent2.Code = workflowDescriptors[1].Code;
				parent3.Code = "!@#";

				child1.Code = "C1";
				child2.Code = "C2";
				child3.Code = "C3";

				Editor.SetValueFromEditorPane(editorPane, collection);
				AssertSetAndGetValuesEqual(collection, ((IDataBoundControl)editorPane).DataSource);
			}
		}

		#region Implementation

		protected override RegistryItemEditor GetEditor() => new WorkflowCategoriesRegistryItemEditor(RegistryItem.DataType, null, Factory);

		protected override Type GetExpectedEditorPaneType() => typeof(WorkflowCategoriesControl);

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel() => new WorkflowCategoriesRegistryItem("", null, null, null, RegistryStorageFlags.System);

		protected override void AssertSetAndGetValuesEqual(object setValue, object getValue)
		{
			var setCollection = (CategorisedWorkflowCategoriesCollection)setValue;
			var getCollection = (CategorisedWorkflowCategoriesCollection)getValue;

			AssertEquals("GetValueFromEditorPane().Count", setCollection.Count, getCollection.Count);
			base.AssertSetAndGetValuesEqual(setValue, getValue);
		}

		protected override object[] GetValidRegistryValues()
		{
			CategorisedWorkflowCategoriesCollection collection = new CategorisedWorkflowCategoriesCollection();
			return new object[] { collection };
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane) => !((WorkflowCategoriesControl)editorPane).ReadOnly;

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;

		#endregion
	}
}
