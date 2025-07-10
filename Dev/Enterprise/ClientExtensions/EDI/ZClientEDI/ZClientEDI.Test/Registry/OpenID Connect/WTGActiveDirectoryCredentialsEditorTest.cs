using System;
using CargoWise.Types;
using Enterprise.Client.EDI.Registry.GUI;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;
using ZClientEDI.Business;

namespace ZClientEDI.Test.Registry
{
	[TestedType(typeof(WTGActiveDirectoryCredentialsEditor))]
	public class WTGActiveDirectoryCredentialsEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new WTGActiveDirectoryCredentialsEditor(new WTGActiveDirectoryCredentialsRegistryItem.WTGActiveDirectoryCredentialsRegistryDataType(credentials));
		}

		protected override bool GetEditorPaneEnabledState(System.Windows.Forms.Control editorPane)
		{
			return !((WTGActiveDirectoryCredentialsControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(WTGActiveDirectoryCredentialsControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new WTGActiveDirectoryCredentialsRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, credentials);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { credentials };
		}
		#endregion

		readonly WTGActiveDirectoryCredentials credentials = new WTGActiveDirectoryCredentials
		{
			DomainName = "fake.domain",
			DomainUserName = "Dexter",
			DomainUserPassword = "DasIstEinPasswort",
			OrganizationalUnitPath = "root/Accounts/Token Based Authentication",
			IsEnabled = ZBool.True
		};
	}
}
