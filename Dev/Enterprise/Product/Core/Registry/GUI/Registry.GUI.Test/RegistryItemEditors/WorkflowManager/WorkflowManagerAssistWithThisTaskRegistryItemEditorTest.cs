using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(WorkflowManagerAssistWithThisTaskRegistryItemEditor))]
	sealed class WorkflowManagerAssistWithThisTaskRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new WorkflowManagerAssistWithThisTaskRegistryItemEditor(RegistryItem.DataType, null, Factory);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(WorkflowManagerAssistWithThisTaskControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new AssistWithThisTaskRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		protected override void AssertSetAndGetValuesEqual(object setValue, object getValue)
		{
			var setCollection = (CategorisedAssistWithThisTaskSettingCollection)setValue;
			var getCollection = (CategorisedAssistWithThisTaskSettingCollection)getValue;

			setCollection.SynchroniseWithWorkflowDescriptorList();
			AssertEquals("GetValueFromEditorPane().Count", setCollection.Count, getCollection.Count);
			base.AssertSetAndGetValuesEqual(setValue, getValue);
		}

		protected override object[] GetValidRegistryValues()
		{
			var collection = new CategorisedAssistWithThisTaskSettingCollection();
			return new object[] { collection };
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((WorkflowManagerAssistWithThisTaskControl)editorPane).ReadOnly;
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		protected override void SetUp()
		{
			base.SetUp();

			AssistWithThisTaskSettingForOneWorkflowTest.SetUpTaskTypesForAssistWithThisTaskTests();
		}
	}
}
