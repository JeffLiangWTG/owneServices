using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(ServiceUrlRegistryItemEditor))]
	sealed class ServiceUrlRegistryItemEditorTest : Enterprise.Registry.GUI.Testing.RegistryItemEditorTestCase
	{
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new ServiceUrlRegistryItem("", null, null, null, RegistryStorageFlags.System, String.Empty);
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new ServiceUrlRegistryItemEditor(RegistryItem.DataType);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(ServiceUrlControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { String.Empty };
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((ServiceUrlControl)editorPane).ReadOnly;
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.TopLeftRight; }
		}
	}
}
