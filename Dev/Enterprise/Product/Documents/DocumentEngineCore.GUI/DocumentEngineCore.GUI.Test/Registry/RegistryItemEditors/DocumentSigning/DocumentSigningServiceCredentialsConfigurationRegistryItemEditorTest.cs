using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.Registry.GUI.Testing
{
	[TestedType(typeof(DocumentSigningServiceCredentialsConfigurationRegistryItemEditor))]
	sealed class DocumentSigningServiceCredentialsConfigurationRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new DocumentSigningServiceCredentialsConfigurationRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((DocumentSigningServiceCredentialsConfigurationControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(DocumentSigningServiceCredentialsConfigurationControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new DocumentSigningServiceCredentialsRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		protected override object[] GetValidRegistryValues()
		{
			var result = new DocumentSigningServiceCredentialsConfiguration();
			result.ClientID = "client";
			result.AccessKey = "key";
			result.KeyID = "id";

			return new object[] { result };
		}

		#endregion
	}
}
