using System;
using System.Windows.Forms;
using CargoWise.ActiveDirectory.TestFramework;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Security.ActiveDirectory.GUI.Test
{
	[TestedType(typeof(DomainCredentialsCollectionRegistryEditor))]
	class DomainCredentialsCollectionRegistryEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new DomainCredentialsCollectionRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyEditableBySupportIfHosted, new DomainCredentialsCollection());
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new DomainCredentialsCollectionRegistryEditor(new DomainCredentialsCollectionRegistryDataType(new DomainCredentialsCollection()), null, Factory);
		}

		protected override Type GetExpectedEditorPaneType() => typeof(DomainCredentialsCollectionRegistryControl);

		protected override object[] GetValidRegistryValues()
		{
			var domainCredentialsCollection1 = new DomainCredentialsCollection();

			var domainCredentialsCollection2 = new DomainCredentialsCollection
			{
				new DomainCredentials
				{
					DomainName = TestConstants.Domain,
					DomainUserName = TestConstants.ADTestUserAccount.NameWithDomain,
					DomainUserPassword = TestConstants.ADTestUserAccount.Password,
					IsDefaultDomain = true,
					UserOrganisationalUnit = TestConstants.ValidOU,
					GroupOrganisationalUnit = TestConstants.ValidOU,
					DefaultPassword = "Changeme1234"
				}
			};

			var domainCredentialsCollection3 = new DomainCredentialsCollection
			{
				new DomainCredentials
				{
					DomainName = TestConstants.Domain,
					DomainUserName = TestConstants.ADTestUserAccount.NameWithDomain,
					DomainUserPassword = TestConstants.ADTestUserAccount.Password,
					IsDefaultDomain = true,
					UserOrganisationalUnit = TestConstants.ValidOU,
					GroupOrganisationalUnit = TestConstants.ValidOU,
					DefaultPassword = "Changeme1234"
				},
				new DomainCredentials
				{
					DomainName = "fake.domain",
					DomainUserName = "fake.user",
					DomainUserPassword = "fakepassword",
					IsDefaultDomain = false,
					UserOrganisationalUnit = "fake/users/ou",
					GroupOrganisationalUnit = "fake/groups/ou",
					DefaultPassword = "Changeme1234"
				}
			};

			return new[] { domainCredentialsCollection1, domainCredentialsCollection2, domainCredentialsCollection3 };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;

		protected override bool GetEditorPaneEnabledState(Control editorPane) => !((DomainCredentialsCollectionRegistryControl)editorPane).ReadOnly;

		#endregion
	}
}
