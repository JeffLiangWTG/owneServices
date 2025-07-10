using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ActiveDirectory.TestFramework;
using Enterprise.Integration;
using Enterprise.Security.ActiveDirectory.Test;
using Enterprise.StabilityChecker;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Security.ActiveDirectory.GUI.Test
{
	public class ActiveDirectoryStabilityCheckerTest : TestCaseWithFactoryAndMocks
	{
		public void TestStabilityChecker_HasDomainRight()
		{
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;
			ActiveDirectoryRegistry.Instance.EntitiesToSync = EntitiesToSync.UsersAndGroups;

			var domainCredentials = ADTestHelper.CreateDomainCredentials();
			ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DomainCredentialsCollection { domainCredentials });
			AssertNoErrors();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;

			using (ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.DataType.SuspendValidation())
			{
				domainCredentials.DomainUserName = TestConstants.ADTestUserAccountNoOURight.NameWithDomain;
				domainCredentials.DomainUserPassword = TestConstants.ADTestUserAccountNoOURight.Password;
				ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DomainCredentialsCollection { domainCredentials });

				AssertErrors(ActiveDirectoryStabilityChecker.GetNoWritePrivilegeError(domainCredentials));
			}

			domainCredentials.DomainUserName = TestConstants.ADTestUserAccount.NameWithDomain;
			domainCredentials.DomainUserPassword = TestConstants.ADTestUserAccount.Password;
			ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DomainCredentialsCollection { domainCredentials });
			AssertNoErrors();
		}

		public void TestStabilityChecker_HasDomainRight_OnlyCheckWhenSyncingToAD()
		{
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;
			ActiveDirectoryRegistry.Instance.EntitiesToSync = EntitiesToSync.UsersAndGroups;

			var domainCredentials = ADTestHelper.CreateDomainCredentials();
			ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DomainCredentialsCollection { domainCredentials });
			AssertNoErrors();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;

			domainCredentials.DomainUserName = TestConstants.ADTestUserAccountNoOURight.NameWithDomain;
			domainCredentials.DomainUserPassword = TestConstants.ADTestUserAccountNoOURight.Password;
			ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DomainCredentialsCollection { domainCredentials });

			//AD Master, 2-way sync => should error
			ActiveDirectoryRegistry.Instance.SyncMode = SyncMode.ADIsMaster;
			ActiveDirectoryRegistry.Instance.SyncDirection = SyncDirection.TwoWay;
			ActiveDirectoryRegistry.Instance.SyncDirectionGroup = SyncDirection.TwoWay;
			AssertErrors(ActiveDirectoryStabilityChecker.GetNoWritePrivilegeError(domainCredentials));

			//AD Master, 1-way sync => should not error
			ActiveDirectoryRegistry.Instance.SyncDirection = SyncDirection.OneWay;
			ActiveDirectoryRegistry.Instance.SyncDirectionGroup = SyncDirection.OneWay;
			AssertNoErrors();

			//CW1 Master, 1-way sync => should error
			ActiveDirectoryRegistry.Instance.SyncMode = SyncMode.EnterpriseIsMaster;
			AssertErrors(ActiveDirectoryStabilityChecker.GetNoWritePrivilegeError(domainCredentials));

			//CW1 Master, 2-way sync => should error
			ActiveDirectoryRegistry.Instance.SyncDirection = SyncDirection.TwoWay;
			ActiveDirectoryRegistry.Instance.SyncDirectionGroup = SyncDirection.TwoWay;
			AssertErrors(ActiveDirectoryStabilityChecker.GetNoWritePrivilegeError(domainCredentials));

			//if staff doesn't error but group does => should error
			ActiveDirectoryRegistry.Instance.SyncMode = SyncMode.ADIsMaster;
			ActiveDirectoryRegistry.Instance.SyncDirection = SyncDirection.OneWay;
			ActiveDirectoryRegistry.Instance.SyncDirectionGroup = SyncDirection.TwoWay;
			AssertErrors(ActiveDirectoryStabilityChecker.GetNoWritePrivilegeError(domainCredentials));

			ActiveDirectoryRegistry.Instance.SyncDirectionGroup = SyncDirection.OneWay;
			ActiveDirectoryRegistry.Instance.SyncMode = SyncMode.EnterpriseIsMaster;
			AssertErrors(ActiveDirectoryStabilityChecker.GetNoWritePrivilegeError(domainCredentials));

			ActiveDirectoryRegistry.Instance.SyncDirectionGroup = SyncDirection.TwoWay;
			AssertErrors(ActiveDirectoryStabilityChecker.GetNoWritePrivilegeError(domainCredentials));

			//unless we're not syncing group => should not error
			ActiveDirectoryRegistry.Instance.EntitiesToSync = EntitiesToSync.UsersOnly;

			ActiveDirectoryRegistry.Instance.SyncMode = SyncMode.ADIsMaster;
			ActiveDirectoryRegistry.Instance.SyncDirection = SyncDirection.OneWay;
			ActiveDirectoryRegistry.Instance.SyncDirectionGroup = SyncDirection.TwoWay;
			AssertNoErrors();

			ActiveDirectoryRegistry.Instance.SyncDirectionGroup = SyncDirection.OneWay;
			AssertNoErrors();
		}

		void AssertNoErrors()
		{
			var results = new ActiveDirectoryStabilityChecker().Check();
			AssertEquals(string.Join(System.Environment.NewLine, results.Select(r => r.Description)), 0, results.Length);
		}

		void AssertErrors(params string[] expectedWarningDescriptions) => AssertErrors(StabilityResultLevel.Critical, expectedWarningDescriptions);

		void AssertErrors(StabilityResultLevel stabilityResultLevel, params string[] expectedWarningDescriptions)
		{
			var results = new ActiveDirectoryStabilityChecker().Check();
			AssertEquals(expectedWarningDescriptions.Length, results.Length);
			foreach (var result in results)
			{
				AssertEquals(stabilityResultLevel, result.StabilityLevel);
				AssertCollectionContains("Should contain message: " + result.Description, (string)result.Description, expectedWarningDescriptions);
			}
		}

		#region implementation

		protected override void SetUp()
		{
			base.SetUp();
			DirectorySearcherProviderSubstitution.DoNotMock = true;
		}

		#endregion
	}
}
