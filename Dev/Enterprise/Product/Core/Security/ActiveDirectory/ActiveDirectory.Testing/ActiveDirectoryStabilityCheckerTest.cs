using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using CargoWise.ActiveDirectory;
using CargoWise.ActiveDirectory.TestFramework;
using Enterprise.Integration;
using Enterprise.StabilityChecker;
using Enterprise.ZArchitecture.Environment;
using Moq;

namespace Enterprise.Security.ActiveDirectory.Test
{
	public class ActiveDirectoryStabilityCheckerTest : TestCaseWithFactoryAndMocks
	{
		public void TestHandleExceptions()
		{
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;
			ActiveDirectoryRegistry.Instance.EntitiesToSync = EntitiesToSync.UsersAndGroups;

			var domainCredentials = ADTestHelper.CreateDomainCredentials();
			ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DomainCredentialsCollection { domainCredentials });

			var testException = new DirectoryServicesException(@"something goes wrong here");

			var mockedOU = new Mock<IOrganisationalUnit>(MockBehavior.Strict);
			directorySearcherMock.Setup(s => s.FindOrganisationalUnit(TestConstants.ValidOU)).Returns(mockedOU.Object);
			mockedOU.Setup(x => x.CreateNewChild(It.IsAny<string>(), It.IsAny<DirectoryObjectType>(), It.IsAny<IDirectorySearcher>())).Throws(testException);

			DirectorySearcherProviderSubstitution.DoNotMock = false;
			DirectorySearcherProviderSubstitution.OrganisationalUnitMock = mockedOU.Object;

			AssertErrors(StabilityResultLevel.Exception, ActiveDirectoryStabilityChecker.GetExceptionError(domainCredentials, testException));
		}

		public void TestAtLeastOneDomainCredentialsMustBeSet()
		{
			ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DomainCredentialsCollection());
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = false;
			AssertNoErrors();

			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;
			AssertErrors(string.Format("At least one Domain Credentials must be set in registry: {0}", ((IMultilingualRegistryItem)ActiveDirectoryRegistry.Instance.DomainCredentialsCollection).LocationMultilingual));
		}

		public void TestStabilityChecker_UserOU()
		{
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;
			ActiveDirectoryRegistry.Instance.EntitiesToSync = EntitiesToSync.UsersAndGroups;

			var domainCredentials = ADTestHelper.CreateDomainCredentials();
			ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DomainCredentialsCollection { domainCredentials });

			AssertNoErrors();

			using (ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.DataType.SuspendValidation())
			{
				domainCredentials.UserOrganisationalUnit = TestConstants.InvalidOU;
				ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DomainCredentialsCollection { domainCredentials });
				AssertErrors(ActiveDirectoryStabilityChecker.GetUserOUError(domainCredentials));

				domainCredentials.UserOrganisationalUnit = TestConstants.ValidOU;
				ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DomainCredentialsCollection { domainCredentials });
				AssertNoErrors();

				// We skip IsPasswordMatchingPolicy() when test for rootOU to avoid unnecessary write to the root OU in SAND
				ActiveDirectoryRegistry.Instance.SyncMode = SyncMode.ADIsMaster;
				ActiveDirectoryRegistry.Instance.SyncDirection = SyncDirection.OneWay;
				ActiveDirectoryRegistry.Instance.SyncDirectionGroup = SyncDirection.OneWay;
				domainCredentials.UserOrganisationalUnit = "";
				ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DomainCredentialsCollection { domainCredentials });
				AssertNoErrors();
			}
		}

		public void TestStabilityChecker_GroupOU()
		{
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;
			ActiveDirectoryRegistry.Instance.EntitiesToSync = EntitiesToSync.UsersAndGroups;

			var domainCredentials = ADTestHelper.CreateDomainCredentials();
			ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DomainCredentialsCollection { domainCredentials });
			AssertNoErrors();

			using (ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.DataType.SuspendValidation()) //Otherwise setting "" or InvalidOU will throw exception before we can access
			{
				domainCredentials.GroupOrganisationalUnit = TestConstants.InvalidOU;
				ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DomainCredentialsCollection { domainCredentials });
				AssertErrors(ActiveDirectoryStabilityChecker.GetGroupOUError(domainCredentials));

				domainCredentials.GroupOrganisationalUnit = TestConstants.ValidOU;
				ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DomainCredentialsCollection { domainCredentials });
				AssertNoErrors();

				// We skip writable test for root OU as the test AD account doesn't have write access to root OU
				ActiveDirectoryRegistry.Instance.SyncMode = SyncMode.ADIsMaster;
				ActiveDirectoryRegistry.Instance.SyncDirection = SyncDirection.OneWay;
				ActiveDirectoryRegistry.Instance.SyncDirectionGroup = SyncDirection.OneWay;
				domainCredentials.GroupOrganisationalUnit = "";
				ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DomainCredentialsCollection { domainCredentials });
				AssertNoErrors();
			}
		}

		public void TestStabilityChecker_ErrorMessagePatterns()
		{
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;
			ActiveDirectoryRegistry.Instance.EntitiesToSync = EntitiesToSync.UsersAndGroups;

			var domainCredentials = ADTestHelper.CreateDomainCredentials();
			ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DomainCredentialsCollection { domainCredentials });

			AssertNoErrors();

			using (ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.DataType.SuspendValidation())
			{
				// Invalid User and Group OU - get both User and Group OU errors
				domainCredentials.UserOrganisationalUnit = TestConstants.InvalidOU;
				domainCredentials.GroupOrganisationalUnit = TestConstants.InvalidOU;
				ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DomainCredentialsCollection { domainCredentials });
				AssertErrors(ActiveDirectoryStabilityChecker.GetUserOUError(domainCredentials), ActiveDirectoryStabilityChecker.GetGroupOUError(domainCredentials));

				// Invalid Group OU
				domainCredentials.UserOrganisationalUnit = TestConstants.ValidOU;
				ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DomainCredentialsCollection { domainCredentials });
				AssertErrors(ActiveDirectoryStabilityChecker.GetGroupOUError(domainCredentials));

				//When not require write privilege - only Group OU error
				ActiveDirectoryRegistry.Instance.SyncMode = SyncMode.ADIsMaster;
				ActiveDirectoryRegistry.Instance.SyncDirection = SyncDirection.OneWay;
				ActiveDirectoryRegistry.Instance.SyncDirectionGroup = SyncDirection.OneWay;
				AssertErrors(ActiveDirectoryStabilityChecker.GetGroupOUError(domainCredentials));

				//When sync user only, invalid group OU won't cause any error
				ActiveDirectoryRegistry.Instance.EntitiesToSync = EntitiesToSync.UsersOnly;
				AssertNoErrors();

				//When sync user only and require write privilege, invalid group OU won't cause any error
				ActiveDirectoryRegistry.Instance.SyncMode = SyncMode.EnterpriseIsMaster;
				ActiveDirectoryRegistry.Instance.SyncDirection = SyncDirection.TwoWay;
				ActiveDirectoryRegistry.Instance.SyncDirectionGroup = SyncDirection.TwoWay;
				AssertNoErrors();
			}
		}

		public void TestStabilityChecker_DefaultPassword()
		{
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;
			ActiveDirectoryRegistry.Instance.EntitiesToSync = EntitiesToSync.UsersAndGroups;

			var domainCredentials = ADTestHelper.CreateDomainCredentials();
			ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DomainCredentialsCollection { domainCredentials });
			AssertNoErrors();

			using (ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.DataType.SuspendValidation()) //Otherwise setting "" or InvalidOU will throw exception before we can access
			{
				// **Invalid Password**
				domainCredentials.DefaultPassword = "short";
				ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DomainCredentialsCollection { domainCredentials });
				AssertErrors(ActiveDirectoryStabilityChecker.GetDefaultPasswordError(domainCredentials));

				//If User OU is invalid, we can't test the password but should still report the invalid OU
				domainCredentials.UserOrganisationalUnit = TestConstants.InvalidOU;
				ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DomainCredentialsCollection { domainCredentials });
				AssertErrors(ActiveDirectoryStabilityChecker.GetUserOUError(domainCredentials));

				//If User OU is valid but Group OU is invalid, only get Group OU error as it can't do password validity test because DirectorySearcherFactory can't return a writable DirectorySearcher if Group OU is invalid
				domainCredentials.UserOrganisationalUnit = TestConstants.ValidOU;
				domainCredentials.GroupOrganisationalUnit = TestConstants.InvalidOU;
				ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DomainCredentialsCollection { domainCredentials });
				AssertErrors(ActiveDirectoryStabilityChecker.GetGroupOUError(domainCredentials));

				//But if sync user only, invalid group OU will be ignored
				ActiveDirectoryRegistry.Instance.EntitiesToSync = EntitiesToSync.UsersOnly;
				AssertErrors(ActiveDirectoryStabilityChecker.GetDefaultPasswordError(domainCredentials));

				// **Valid password**
				// If User OU is valid but Group OU is invalid

				domainCredentials.DefaultPassword = ADTestHelper.PasswordMatchingPolicy;
				ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DomainCredentialsCollection { domainCredentials });
				ActiveDirectoryRegistry.Instance.EntitiesToSync = EntitiesToSync.UsersAndGroups;
				AssertErrors(ActiveDirectoryStabilityChecker.GetGroupOUError(domainCredentials));

				//Everything is valid
				domainCredentials.GroupOrganisationalUnit = TestConstants.ValidOU;
				ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DomainCredentialsCollection { domainCredentials });
				AssertNoErrors();
			}
		}

		public void TestStabilityChecker_CanHandleExceptionInDeleteADSCTempUser()
		{
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;
			ActiveDirectoryRegistry.Instance.EntitiesToSync = EntitiesToSync.UsersAndGroups;

			var domainCredentials = ADTestHelper.CreateDomainCredentials();
			ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DomainCredentialsCollection { domainCredentials });

			var currentADSCUserName = "ADSCFD689E900BE64315";
			var currentADSCDirectoryEntry = DummyDirectoryEntryWrapper.CreateUser(currentADSCUserName);
			currentADSCDirectoryEntry.ADDeleteAction = () => new DirectoryServicesCOMException("bla");
			var mockedOU = new Mock<IOrganisationalUnit>();

			var directorySearcherMock = new Mock<IDirectorySearcher>();
			DirectorySearcherProviderSubstitution.DirectorySearcherMock = directorySearcherMock;

			mockedOU.Setup(x => x.CreateNewChild(It.IsAny<string>(), It.IsAny<DirectoryObjectType>(), It.IsAny<IDirectorySearcher>())).Returns(currentADSCDirectoryEntry);
			directorySearcherMock.Setup(s => s.FindOrganisationalUnit(TestConstants.ValidOU)).Returns(mockedOU.Object);

			StabilityResult[] results = null;
			AssertNoExceptionThrown(() => results = new ActiveDirectoryStabilityChecker().Check());
			AssertEquals(string.Join(System.Environment.NewLine, results.Select(r => r.Description)), 0, results.Length);
		}

		public void TestStabilityChecker_CanCleanUpLeftoverADSCTempUser()
		{
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;
			ActiveDirectoryRegistry.Instance.EntitiesToSync = EntitiesToSync.UsersAndGroups;

			var domainCredentials = ADTestHelper.CreateDomainCredentials();
			ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DomainCredentialsCollection { domainCredentials });

			// Leftover ADSC user more than an hour
			var leftoverADSCUserName1 = "ADSCD040568FA5AC41BE";
			var leftoverADSCEntry1 = DummyDirectoryEntryWrapper.CreateUser(leftoverADSCUserName1);
			leftoverADSCEntry1.SetCreationDate(DateTime.UtcNow.AddMinutes(-61));
			var isLeftoverADSCEntry1Deleted = false;
			leftoverADSCEntry1.ADDeleteAction = () => isLeftoverADSCEntry1Deleted = true;
			var leftoverADSCUser1 = new Mock<IUserDirectorySearchResult>();

			// Leftover ADSC user less than an hour
			var leftoverADSCUserName2 = "ADSCB300890F5831487B";
			var leftoverADSCEntry2 = DummyDirectoryEntryWrapper.CreateUser(leftoverADSCUserName2);
			leftoverADSCEntry2.SetCreationDate(DateTime.UtcNow.AddMinutes(-58));
			var isLeftoverADSCEntry2Deleted = false;
			leftoverADSCEntry2.ADDeleteAction = () => isLeftoverADSCEntry2Deleted = true;
			var leftoverADSCUser2 = new Mock<IUserDirectorySearchResult>();

			// Non ADSC user
			var nonADSCUserName = "ADSCGHIJKLMNOPXY";
			var nonADSCEntry = DummyDirectoryEntryWrapper.CreateUser(nonADSCUserName);
			nonADSCEntry.SetCreationDate(DateTime.UtcNow.AddDays(1));
			var isNonADSCAEntryDeleted = false;
			nonADSCEntry.ADDeleteAction = () => isNonADSCAEntryDeleted = true;
			var nonADSCUser = new Mock<IUserDirectorySearchResult>();

			//Matching user search results
			var matchingUserList = new Mock<IDirectorySearchResults<IUserDirectorySearchResult>>();

			// new ADSC Users
			var currentADSCUserName = "ADSCFD689E900BE64315";
			var currentADSCEntry = DummyDirectoryEntryWrapper.CreateUser(currentADSCUserName);
			bool isCurrentEntryDeleted = false;
			currentADSCEntry.ADDeleteAction = () => isCurrentEntryDeleted = true;

			var mockedOU = new Mock<IOrganisationalUnit>();

			var directorySearcherMock = new Mock<IDirectorySearcher>();
			DirectorySearcherProviderSubstitution.DirectorySearcherMock = directorySearcherMock;

			leftoverADSCUser1.Setup(x => x.GetDirectoryEntry()).Returns(leftoverADSCEntry1);
			leftoverADSCUser1.SetupGet(x => x.IsActive).Returns(false);
			leftoverADSCUser1.SetupGet(x => x.Win2KName).Returns(leftoverADSCUserName1);

			leftoverADSCUser2.Setup(x => x.GetDirectoryEntry()).Returns(leftoverADSCEntry2);
			leftoverADSCUser2.SetupGet(x => x.IsActive).Returns(false);
			leftoverADSCUser2.SetupGet(x => x.Win2KName).Returns(leftoverADSCUserName2);

			nonADSCUser.Setup(x => x.GetDirectoryEntry()).Returns(nonADSCEntry);
			nonADSCUser.SetupGet(x => x.IsActive).Returns(false);
			nonADSCUser.SetupGet(x => x.Win2KName).Returns(nonADSCUserName);

			matchingUserList.Setup(x => x.GetEnumerator())
				.Returns(new List<IUserDirectorySearchResult>(new[] { leftoverADSCUser1.Object, leftoverADSCUser2.Object, nonADSCUser.Object }).GetEnumerator());

			mockedOU.Setup(x => x.CreateNewChild(It.IsAny<string>(), It.IsAny<DirectoryObjectType>(), It.IsAny<IDirectorySearcher>())).Returns(currentADSCEntry);
			directorySearcherMock.Setup(s => s.FindOrganisationalUnit(TestConstants.ValidOU)).Returns(mockedOU.Object);
			directorySearcherMock.Setup(s => s.FindMatchingUsers("ADSC*", SearchScope.Subtree, ResultMatchingMode.AllowWildcards, domainCredentials.UserOrganisationalUnit)).Returns(matchingUserList.Object);

			var results = new ActiveDirectoryStabilityChecker().Check();
			AssertEquals(string.Join(System.Environment.NewLine, results.Select(r => r.Description)), 0, results.Length);
			AssertEquals("Leftover ADSC AD entry created more than an house should be deleted", true, isLeftoverADSCEntry1Deleted);
			AssertEquals("Leftover ADSC AD entry created less than an hour should not be deleted", false, isLeftoverADSCEntry2Deleted);
			AssertEquals("Non ADSC AD entry should not be deleted", false, isNonADSCAEntryDeleted);
			AssertEquals("Current ADSC AD entry should be deleted", true, isCurrentEntryDeleted);
		}

		public void TestStabilityChecker_DefaultPassword_OnlyCheckWhenSyncingToAD_UsersOnly()
		{
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;
			ActiveDirectoryRegistry.Instance.EntitiesToSync = EntitiesToSync.UsersOnly;

			var domainCredentials = ADTestHelper.CreateDomainCredentials();
			domainCredentials.DefaultPassword = "short";
			ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DomainCredentialsCollection { domainCredentials });

			//AD Master, 2-way sync => should error
			ActiveDirectoryRegistry.Instance.SyncMode = SyncMode.ADIsMaster;
			ActiveDirectoryRegistry.Instance.SyncDirection = SyncDirection.TwoWay;
			ActiveDirectoryRegistry.Instance.SyncDirectionGroup = SyncDirection.OneWay;
			AssertErrors(ActiveDirectoryStabilityChecker.GetDefaultPasswordError(domainCredentials));
			ActiveDirectoryRegistry.Instance.SyncDirectionGroup = SyncDirection.TwoWay;
			AssertErrors(ActiveDirectoryStabilityChecker.GetDefaultPasswordError(domainCredentials));

			//AD Master, 1-way sync => should not error
			ActiveDirectoryRegistry.Instance.SyncDirection = SyncDirection.OneWay;
			ActiveDirectoryRegistry.Instance.SyncDirectionGroup = SyncDirection.OneWay;
			AssertNoErrors();
			ActiveDirectoryRegistry.Instance.SyncDirectionGroup = SyncDirection.TwoWay;
			AssertNoErrors();

			//CW1 Master, 1-way sync => should error
			ActiveDirectoryRegistry.Instance.SyncMode = SyncMode.EnterpriseIsMaster;
			ActiveDirectoryRegistry.Instance.SyncDirection = SyncDirection.OneWay;
			ActiveDirectoryRegistry.Instance.SyncDirectionGroup = SyncDirection.OneWay;
			AssertErrors(ActiveDirectoryStabilityChecker.GetDefaultPasswordError(domainCredentials));
			ActiveDirectoryRegistry.Instance.SyncDirectionGroup = SyncDirection.TwoWay;
			AssertErrors(ActiveDirectoryStabilityChecker.GetDefaultPasswordError(domainCredentials));

			//CW1 Master, 2-way sync => should error
			ActiveDirectoryRegistry.Instance.SyncDirection = SyncDirection.TwoWay;
			ActiveDirectoryRegistry.Instance.SyncDirectionGroup = SyncDirection.OneWay;
			AssertErrors(ActiveDirectoryStabilityChecker.GetDefaultPasswordError(domainCredentials));
			ActiveDirectoryRegistry.Instance.SyncDirectionGroup = SyncDirection.TwoWay;
			AssertErrors(ActiveDirectoryStabilityChecker.GetDefaultPasswordError(domainCredentials));
		}

		public void TestStabilityChecker_DefaultPassword_OnlyCheckWhenSyncingToAD_UserAndGroup()
		{
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;
			ActiveDirectoryRegistry.Instance.EntitiesToSync = EntitiesToSync.UsersAndGroups;

			var domainCredentials = ADTestHelper.CreateDomainCredentials();
			domainCredentials.DefaultPassword = "short";
			ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DomainCredentialsCollection { domainCredentials });

			//AD Master, 2-way sync => should error
			ActiveDirectoryRegistry.Instance.SyncMode = SyncMode.ADIsMaster;
			ActiveDirectoryRegistry.Instance.SyncDirection = SyncDirection.TwoWay;
			ActiveDirectoryRegistry.Instance.SyncDirectionGroup = SyncDirection.OneWay;
			AssertErrors(ActiveDirectoryStabilityChecker.GetDefaultPasswordError(domainCredentials));
			ActiveDirectoryRegistry.Instance.SyncDirectionGroup = SyncDirection.TwoWay;
			AssertErrors(ActiveDirectoryStabilityChecker.GetDefaultPasswordError(domainCredentials));

			//AD Master, 1-way sync => should not error
			ActiveDirectoryRegistry.Instance.SyncDirection = SyncDirection.OneWay;
			ActiveDirectoryRegistry.Instance.SyncDirectionGroup = SyncDirection.OneWay;
			AssertNoErrors();
			ActiveDirectoryRegistry.Instance.SyncDirectionGroup = SyncDirection.TwoWay;
			AssertNoErrors();

			//CW1 Master, 1-way sync => should error
			ActiveDirectoryRegistry.Instance.SyncMode = SyncMode.EnterpriseIsMaster;
			ActiveDirectoryRegistry.Instance.SyncDirection = SyncDirection.OneWay;
			ActiveDirectoryRegistry.Instance.SyncDirectionGroup = SyncDirection.OneWay;
			AssertErrors(ActiveDirectoryStabilityChecker.GetDefaultPasswordError(domainCredentials));
			ActiveDirectoryRegistry.Instance.SyncDirectionGroup = SyncDirection.TwoWay;
			AssertErrors(ActiveDirectoryStabilityChecker.GetDefaultPasswordError(domainCredentials));

			//CW1 Master, 2-way sync => should error
			ActiveDirectoryRegistry.Instance.SyncDirection = SyncDirection.TwoWay;
			ActiveDirectoryRegistry.Instance.SyncDirectionGroup = SyncDirection.OneWay;
			AssertErrors(ActiveDirectoryStabilityChecker.GetDefaultPasswordError(domainCredentials));
			ActiveDirectoryRegistry.Instance.SyncDirectionGroup = SyncDirection.TwoWay;
			AssertErrors(ActiveDirectoryStabilityChecker.GetDefaultPasswordError(domainCredentials));
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
