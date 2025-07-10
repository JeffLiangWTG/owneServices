using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.Registry.GUI.Testing
{
	[TestedType(typeof(DocumentSigningServicePartnerCredentialsRegistryItemEditor))]
	sealed class DocumentSigningServicePartnerCredentialsRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new DocumentSigningServicePartnerCredentialsRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((DocumentSigningServicePartnerCredentialsControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(DocumentSigningServicePartnerCredentialsControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new DocumentSigningServicePartnerCredentialsRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport, new DocumentSigningServicePartnerCredentials() { PartnerID = "id", PartnerAccessKey = "key" });
		}

		protected override object[] GetValidRegistryValues()
		{
			DocumentSigningServicePartnerCredentials result = new DocumentSigningServicePartnerCredentials();
			result.PartnerID = "client";
			result.PartnerAccessKey = "key";
			return new object[] { result };
		}

		#endregion
	}
}
