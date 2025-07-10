using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(ColorThemeSelectorRegistryItemEditor))]
	sealed class ColorThemeSelectorRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new ColorThemeSelectorRegistryItemEditor(null, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((ColorThemeSelectorControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(ColorThemeSelectorControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			ColorThemeRegistryItem result = new ColorThemeRegistryItem("", null, null, null, new ColorThemeSelector());
			return result;
		}

		protected override void AssertSetAndGetValuesEqual(object setValue, object getValue)
		{
			AssertEquals("GetValueFromEditorPaneCore()", setValue, getValue);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new ColorThemeSelector() };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
