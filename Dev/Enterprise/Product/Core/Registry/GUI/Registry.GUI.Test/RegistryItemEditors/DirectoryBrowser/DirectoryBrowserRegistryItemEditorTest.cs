using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(DirectoryBrowserRegistryItemEditor))]
	sealed class DirectoryBrowserRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new DirectoryBrowserRegistryItemEditor(null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return editorPane.Enabled;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(DirectoryBrowserControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			StringRegistryItem result = new StringRegistryItem("", null, null, null, RegistryStorageFlags.System);
			result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser);
			return result;
		}

		protected override object[] GetValidRegistryValues()
		{
			return new string[] { "Hello!", "Goodbye!" };
		}

		protected override void AssertSetAndGetValuesEqual(object setValue, object getValue)
		{
			base.AssertSetAndGetValuesEqual(((string)setValue).ToUpper(), getValue);
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.TopLeftRight; }
		}

		#endregion
	}
}
