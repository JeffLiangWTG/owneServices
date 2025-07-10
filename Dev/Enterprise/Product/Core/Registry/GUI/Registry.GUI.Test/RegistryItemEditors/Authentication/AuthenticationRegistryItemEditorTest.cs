using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(AuthenticationRegistryItemEditor))]
	sealed class AuthenticationRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new AuthenticationRegistryItemEditor(false, new StringRegistryDataType());
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return editorPane.Enabled;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(AuthenticationControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			StringRegistryItem result = new StringRegistryItem("", null, null, null, RegistryStorageFlags.System);
			result.EditorInfo = new AuthenticationRegistryItemEditorInfo();
			return result;
		}

		protected override object[] GetValidRegistryValues()
		{
			return new string[] { "UserName|Password" };
		}

		#endregion
	}
}
