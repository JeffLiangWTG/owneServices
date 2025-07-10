using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(RatesServiceUrlRegistryItemEditor))]
	sealed class RatesServiceUrlRegistryItemEditorTest : Enterprise.Registry.GUI.Testing.RegistryItemEditorTestCase
	{
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new RatesServiceUrlRegistryItem("", null, null, null, RegistryStorageFlags.System, String.Empty);
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new RatesServiceUrlRegistryItemEditor(RegistryItem.DataType);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(RatesServiceUrlControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { String.Empty };
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((RatesServiceUrlControl)editorPane).ReadOnly;
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.TopLeftRight; }
		}
	}
}
