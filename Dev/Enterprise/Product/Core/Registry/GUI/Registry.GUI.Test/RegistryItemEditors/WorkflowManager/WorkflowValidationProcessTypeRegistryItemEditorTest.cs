using System;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(WorkflowValidationProcessTypeRegistryItemEditor))]
	sealed class WorkflowValidationProcessTypeRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new WorkflowValidationProcessTypeCollectionRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyForDevelopers, new WorkflowValidationProcessTypeCollection());
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new WorkflowValidationProcessTypeRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(WorkflowValidationProcessTypeControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new WorkflowValidationProcessTypeCollection() };
		}

		protected override bool GetEditorPaneEnabledState(System.Windows.Forms.Control editorPane)
		{
			return !((WorkflowValidationProcessTypeControl)editorPane).ReadOnly;
		}
	}
}
