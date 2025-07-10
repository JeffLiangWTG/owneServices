using System;
using System.Windows.Forms;
using Enterprise.Dash.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Dash.GUI.Tests
{
	[TestedType(typeof(WorkflowConfigurationStepCollectionRegistryItemEditor))]
	sealed class WorkflowConfigurationStepCollectionRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new WorkflowConfigurationStepCollectionRegistryItemEditor(null, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((WorkflowConfigurationStepCollectionControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(WorkflowConfigurationStepCollectionControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new WorkflowConfigurationStepRegistryItem("", null, null, null, RegistryStorageFlags.System, TestHelper.GetCodesProviderForTesting());
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new WorkflowConfigurationStepCollection(TestHelper.GetCodesProviderForTesting()) };
		}
	}
}
