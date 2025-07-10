using System;
using System.Windows.Forms;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.GUI.Registry.Testing
{
	[TestedType(typeof(ColorPairSelectorRegistryItemEditor))]
	sealed class ColorPairSelectorRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new ColorPairSelectorRegistryItemEditor(null, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((ColorPairSelectorControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(ColorPairSelectorControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new ColorPairRegistryItem("", null, null, null, RegistryStorageFlags.System, new ColorPairSelector());
		}

		protected override void AssertSetAndGetValuesEqual(object setValue, object getValue)
		{
			AssertEquals("GetValueFromEditorPaneCore()", setValue, getValue);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new ColorPairSelector() };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
