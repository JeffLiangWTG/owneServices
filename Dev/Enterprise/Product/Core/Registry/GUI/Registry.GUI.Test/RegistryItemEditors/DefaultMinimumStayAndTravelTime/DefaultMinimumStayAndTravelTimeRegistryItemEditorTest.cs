using System;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(DefaultMinimumStayAndTravelTimeRegistryItemEditor))]
	sealed class DefaultMinimumStayAndTravelTimeRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new DefaultMinimumStayAndTravelTimeCollectionRegistryItem("", null, null, null, RegistryStorageFlags.System, new DefaultMinimumStayAndTravelTimeCollection());
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new DefaultMinimumStayAndTravelTimeRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(DefaultMinimumStayAndTravelTimeControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { DefaultMinimumStayAndTravelTimeCollection.DefaultValue };
		}

		protected override bool GetEditorPaneEnabledState(System.Windows.Forms.Control editorPane)
		{
			return !((DefaultMinimumStayAndTravelTimeControl)editorPane).ReadOnly;
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;
	}
}
