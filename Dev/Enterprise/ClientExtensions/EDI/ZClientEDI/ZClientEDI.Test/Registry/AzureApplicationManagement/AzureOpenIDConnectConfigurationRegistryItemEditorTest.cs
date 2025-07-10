using System;
using System.Windows.Forms;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.GUI.Testing
{
	[TestedType(typeof(AzureOpenIDConnectConfigurationRegistryItemEditor))]
	public class AzureOpenIDConnectConfigurationRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new AzureOpenIDConnectConfigurationRegistryItemEditor(new AzureOpenIDConnectConfigurationRegistryDataType(), new ZArchitecture.Environment.FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((AzureOpenIDConnectConfigurationControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(AzureOpenIDConnectConfigurationControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new AzureOpenIDConnectConfigurationRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new AzureOpenIDConnectConfigurationCollection
				{
					new AzureOpenIDConnectConfiguration
					{
						Code = "PRD",
						AuthorityUrl = "https://www.example.com",
						ClientID = "Test1"
					}
				}
			};
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;
	}
}
