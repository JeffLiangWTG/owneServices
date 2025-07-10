using CargoWise.ActiveDirectory.TestFramework;
using CargoWise.EntityFramework;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Security.ActiveDirectory.GUI.Test
{
	[TestedType(typeof(DomainCredentialsCollectionRegistryControl))]
	class DomainCredentialsCollectionRegistryControlTest : RegistryZUserControlTestCase
	{
		public void TestMaximumRows()
		{
			using (var control = new DomainCredentialsCollectionRegistryControl())
			{
				AssertEquals("Should not restrict the number of rows", 0, control.DomainCredentialsCollectionGrid.MaximumRows);
			}
		}

		#region Implementation

		protected override IBusiness GetNewBusinessEntity()
		{
			var domainCredentialsCollection = new DomainCredentialsCollection();
			var domainCredentials = new DomainCredentials
			{
				DomainName = TestConstants.Domain,
				DomainUserName = TestConstants.ADTestUserAccount.NameWithDomain,
				DomainUserPassword = TestConstants.ADTestUserAccount.Password,
				IsDefaultDomain = true,
				UserOrganisationalUnit = TestConstants.ValidOU,
				GroupOrganisationalUnit = TestConstants.ValidOU,
				DefaultPassword = "Changeme1234"
			};
			domainCredentialsCollection.Add(domainCredentials);

			return domainCredentialsCollection;
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			var realControl = (DomainCredentialsCollectionRegistryControl)control;

			return realControl.DomainCredentialsCollectionGrid.ReadOnly;
		}

		#endregion
	}
}
