using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.Registry.GUI.RegistryItemEditor;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(SecondarySMTPServerRegistryItemEditor))]
	sealed class SecondarySMTPServerRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor() => new SecondarySMTPServerRegistryItemEditor(RegistryItem.DataType, null, null);

		protected override bool GetEditorPaneEnabledState(Control editorPane) => !((SecondarySMTPServerControl)editorPane).ReadOnly;

		protected override Type GetExpectedEditorPaneType() => typeof(SecondarySMTPServerControl);

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel() => new SecondarySMTPServersRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport);

		protected override object[] GetValidRegistryValues()
		{
			var collection = new SecondarySMTPServerCollection();
			var server = collection.AddNew();
			server.SMTPServer = "mail.server1.com";
			server.SMTPPort = 587;
			server.SMTPSecureConnection = SecureConnectionTypes.SSL;
			server.SMTPUsername = "user1@server1.com";
			server.SMTPPassword = "password1";
			server.AllowEmailsToBeSentFromUsersAddress = true;
			server.SMTPSenderAddress = "sender@server1.com";
			server.SupportedDomains = "domain1.com, domain2.com";

			return new[] { collection };
		}

		protected override EditorPaneAnchor ExpectedAnchor => EditorPaneAnchor.All;
	}
}
