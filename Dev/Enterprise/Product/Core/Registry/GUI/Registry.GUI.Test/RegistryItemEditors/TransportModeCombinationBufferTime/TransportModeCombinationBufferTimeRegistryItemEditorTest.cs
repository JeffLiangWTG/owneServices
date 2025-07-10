using System;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(TransportModeCombinationBufferTimeRegistryItemEditor))]
	sealed class TransportModeCombinationBufferTimeEditorTest : RegistryItemEditorTestCase
	{
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new TransportModeCombinationBufferTimeRegistryItem("", null, null, null, RegistryStorageFlags.System, new TransportModeCombinationBufferTimeCollection());
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new TransportModeCombinationBufferTimeRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(TransportModeCombinationBufferTimeRegistryControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { TransportModeCombinationBufferTimeCollection.DefaultValue };
		}

		protected override bool GetEditorPaneEnabledState(System.Windows.Forms.Control editorPane)
		{
			return !((TransportModeCombinationBufferTimeRegistryControl)editorPane).ReadOnly;
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;
	}
}
