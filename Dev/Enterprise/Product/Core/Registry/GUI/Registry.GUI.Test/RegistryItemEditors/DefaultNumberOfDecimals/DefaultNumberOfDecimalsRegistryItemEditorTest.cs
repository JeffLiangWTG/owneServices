using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(DefaultNumberOfDecimalsRegistryItemEditor))]
	sealed class DefaultNumberOfDecimalsRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new DefaultNumberOfDecimalsRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((DefaultNumberOfDecimalsControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(DefaultNumberOfDecimalsControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new DefaultNumberOfDecimalsRegistryItem("", null, null, null, RegistryStorageFlags.System, new DefaultNumberOfDecimalsCollection(Module.Freight));
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new DefaultNumberOfDecimalsCollection(Module.Freight) };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}
	}
}
