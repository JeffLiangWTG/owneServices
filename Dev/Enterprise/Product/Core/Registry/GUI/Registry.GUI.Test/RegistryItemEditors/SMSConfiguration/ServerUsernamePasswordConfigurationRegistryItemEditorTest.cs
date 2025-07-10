using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(ServerUsernamePasswordConfigurationRegistryItemEditor))]
	sealed class ServerUsernamePasswordConfigurationRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new ServerUsernamePasswordConfigurationRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((ServerUsernamePasswordConfigurationControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(ServerUsernamePasswordConfigurationControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new ServerUsernamePasswordConfigurationRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		protected override object[] GetValidRegistryValues()
		{
			ServerUsernamePasswordConfiguration result = new ServerUsernamePasswordConfiguration();

			result.UserName = "geoffuser";
			result.Password = "geoffpass";
			result.ConfirmPassword = "geoffpass";

			return new object[] { result };
		}

		#endregion
	}
}
