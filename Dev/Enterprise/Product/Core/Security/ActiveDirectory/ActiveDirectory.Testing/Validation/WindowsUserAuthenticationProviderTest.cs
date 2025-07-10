using System;
using System.Collections.Generic;
using CargoWise.ActiveDirectory;
using CargoWise.ActiveDirectory.TestFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Moq;

namespace Enterprise.Security.ActiveDirectory.Test
{
	class WindowsUserAuthenticationProviderTest : TestCaseWithFactory
	{
		public void TestValidateCredentials_WhenDomainUserFailedToLogin()
		{
			var staff = CreateTestStaff(TestConstants.ADTestUserAccountNoOURight.Name, TestConstants.Domain);
			staff.GS_ActiveDirectoryObjectGuid = new ZGuid(TestConstants.ADTestUserAccountNoOURight.Guid);
			Factory.Save();

			var domainCredentials = ADTestHelper.CreateDomainCredentials();
			domainCredentials.DomainUserPassword = "wrongPass"; // The registry has a domain credentials with wrong password and cant login
			ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ADTestHelper.CreateDomainCredentialsCollection(domainCredentials));
			DirectorySearcherFactory.DirectorySearcherOverride_ForTest = null;
			DirectorySearcherFactory.ClearCache();

			var provider = new WindowsUserAuthenticationProvider();
			AssertEquals(ValidateCredentialsResult.OK, provider.ValidateCredentials(staff.PK.ToGuid(), TestConstants.ADTestUserAccountNoOURight.Password));
		}

		public void TestValidateCurrentUserSession_WhenDomainUserFailedToLogin()
		{
			var staff = CreateTestStaff(TestConstants.ADTestUserAccountNoOURight.Name, TestConstants.Domain);
			staff.GS_ActiveDirectoryObjectGuid = new ZGuid(TestConstants.ADTestUserAccountNoOURight.Guid);
			Factory.Save();

			var domainCredentials = ADTestHelper.CreateDomainCredentials();
			domainCredentials.DomainUserPassword = "wrongPass"; // The registry has a domain credentials with wrong password and cant login
			ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ADTestHelper.CreateDomainCredentialsCollection(domainCredentials));
			DirectorySearcherFactory.DirectorySearcherOverride_ForTest = null;
			DirectorySearcherFactory.ClearCache();

			var provider = new WindowsUserAuthenticationProviderForTest();
			provider.CurrentWindowsUserGuid_ForTest = staff.GS_ActiveDirectoryObjectGuid.ToGuid();

			provider.IsUserLocalAccount_ForTest = false;
			AssertEquals("Valid user", ValidateCredentialsResult.OK, provider.ValidateCurrentUserSession(staff.GS_ActiveDirectoryObjectGuid.ToGuid()));
		}

		public void TestValidateCredentials_WhenInvalidDomainServerSpecified()
		{
			var staff = CreateTestStaff("bla", "bla");
			Factory.Save();
			ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ADTestHelper.CreateDomainCredentialsCollection(domainName: "frack", domainUserName: "Starbuck", domainUserPassword: "Longshot"));
			DirectorySearcherFactory.DirectorySearcherOverride_ForTest = null;
			DirectorySearcherFactory.ClearCache();
			var provider = new WindowsUserAuthenticationProvider();
			AssertEquals(ValidateCredentialsResult.Invalid, provider.ValidateCredentials(staff.PK.ToGuid(), "Changeme1234"));
		}

		public void TestValidateCredentials_WithInvalidOperationException()
		{
			var provider = new WindowsUserAuthenticationProvider();
			var result = ValidateCredentialsResult.OK;

			AssertNoExceptionThrown(() => result = provider.ValidateCredentials(Guid.NewGuid(), "testpass"));
			AssertEquals(ValidateCredentialsResult.Invalid, result);
		}

		public void TestValidateCredentials()
		{
			ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ADTestHelper.CreateDomainCredentialsCollection());

			var staff = CreateTestStaff("Starbuck", "");
			var staffNoAD = CreateTestStaff("Gloria", "");
			var staffNotLinked = CreateTestStaff("Oldtown", "");
			staffNotLinked.GS_ActiveDirectoryObjectGuid = ZGuid.Empty;
			var staffLockedOut = CreateTestStaff("Pooh", "");
			Factory.Save();

			directorySearcherMock.Setup(s => s.FindUser(staff.GS_ActiveDirectoryObjectGuid.ToGuid(), It.IsAny<string>())).Returns(DummyDirectoryEntryWrapper.CreateUser(staff.GS_LoginName));
			directorySearcherMock.Setup(s => s.FindUser(staffLockedOut.GS_ActiveDirectoryObjectGuid.ToGuid(), It.IsAny<string>())).Returns(DummyDirectoryEntryWrapper.CreateUser(staffLockedOut.GS_LoginName));

			var provider = new WindowsUserAuthenticationProviderForTest();
			var validationProviderMock = new Mock<IValidationPrincipalContextProvider>();
			provider.GetValidationProvider_ForTest = _ => validationProviderMock.Object;
			var theRightPassword = "Changeme1234";
			validationProviderMock.Setup(z => z.ValidateCredentials(staff.GS_LoginName, theRightPassword)).Returns(true);
			validationProviderMock.Setup(z => z.ValidateCredentials(staffLockedOut.GS_LoginName, It.IsAny<string>())).Throws(new UserLockedOutException());

			using (AssertDbHitsForAllFactories("Should not hit DB from any factory", new Dictionary<string, int>()))
			{
				AssertEquals("Valid user, valid password", ValidateCredentialsResult.OK, provider.ValidateCredentials(staff.PK.ToGuid(), theRightPassword));
				AssertEquals("Valid user, invalid password", ValidateCredentialsResult.Invalid, provider.ValidateCredentials(staff.PK.ToGuid(), "wrong password!"));
				AssertEquals("User AD record invalid", ValidateCredentialsResult.ADRecordNotFound, provider.ValidateCredentials(staffNoAD.PK.ToGuid(), theRightPassword));
				AssertEquals("User not linked", ValidateCredentialsResult.ADRecordNotCreated, provider.ValidateCredentials(staffNotLinked.PK.ToGuid(), theRightPassword));
				AssertEquals("Invalid user", ValidateCredentialsResult.Invalid, provider.ValidateCredentials(Guid.NewGuid(), theRightPassword));
				AssertEquals("Locked out user", ValidateCredentialsResult.LockedOut, provider.ValidateCredentials(staffLockedOut.PK.ToGuid(), "bear"));
			}
		}

		public void TestValidateCredentials_MultipleDomains()
		{
			var domainCredentials1 = ADTestHelper.CreateDomainCredentials(domainName: "sand.wtg.zone");
			var domainCredentials2 = ADTestHelper.CreateDomainCredentials(domainName: "sandpit.local");
			var domainCredentialCollection = ADTestHelper.CreateDomainCredentialsCollection(domainCredentials1, domainCredentials2);

			using (ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.DataType.SuspendValidation())
			{
				ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, domainCredentialCollection);
			}

			var staff1 = CreateTestStaff("Starbuck", domainCredentials1.DomainName);
			var validationProvider1 = new Mock<IValidationPrincipalContextProvider>();
			validationProvider1.Setup(z => z.ValidateCredentials("Starbuck", "Changeme1234")).Returns(true);

			var staff2 = CreateTestStaff("gloria", domainCredentials2.DomainName);
			var validationProvider2 = new Mock<IValidationPrincipalContextProvider>();
			validationProvider2.Setup(z => z.ValidateCredentials("gloria", "Changeme1234")).Returns(true);

			var staffNoAD = CreateTestStaff("oldcow", "");
			var staffNotLinked = CreateTestStaff("Oldtown", "");
			staffNotLinked.GS_ActiveDirectoryObjectGuid = ZGuid.Empty;

			Factory.Save();

			directorySearcherMock.Setup(s => s.FindUser(staff1.GS_ActiveDirectoryObjectGuid.ToGuid(), It.IsAny<string>())).Returns(DummyDirectoryEntryWrapper.CreateUser("Starbuck"));
			directorySearcherMock.Setup(s => s.FindUser(staff2.GS_ActiveDirectoryObjectGuid.ToGuid(), It.IsAny<string>())).Returns(DummyDirectoryEntryWrapper.CreateUser("gloria"));

			var provider = new WindowsUserAuthenticationProviderForTest();
			provider.GetValidationProvider_ForTest = domainCredentials =>
			{
				if (domainCredentials.DomainName == domainCredentials1.DomainName)
				{
					return validationProvider1.Object;
				}
				else if (domainCredentials.DomainName == domainCredentials2.DomainName)
				{
					return validationProvider2.Object;
				}
				else
				{
					return null;
				}
			};

			using (AssertDbHitsForAllFactories("Should not hit DB from any factory", new Dictionary<string, int>()))
			{
				AssertEquals("Domain1 user, valid password", ValidateCredentialsResult.OK, provider.ValidateCredentials(staff1.PK.ToGuid(), "Changeme1234"));
				AssertEquals("Domain1 user, invalid password", ValidateCredentialsResult.Invalid, provider.ValidateCredentials(staff1.PK.ToGuid(), "wrongpassword"));
				AssertEquals("Domain2 user, valid password", ValidateCredentialsResult.OK, provider.ValidateCredentials(staff2.PK.ToGuid(), "Changeme1234"));
				AssertEquals("Domain2 user, invalid password", ValidateCredentialsResult.Invalid, provider.ValidateCredentials(staff2.PK.ToGuid(), "wrongpassword"));
				AssertEquals("User AD record invalid", ValidateCredentialsResult.ADRecordNotFound, provider.ValidateCredentials(staffNoAD.PK.ToGuid(), "Changeme1234"));
				AssertEquals("User not linked", ValidateCredentialsResult.ADRecordNotCreated, provider.ValidateCredentials(staffNotLinked.PK.ToGuid(), "Changeme1234"));
				AssertEquals("Invalid user", ValidateCredentialsResult.Invalid, provider.ValidateCredentials(Guid.NewGuid(), "Changeme1234"));
			}
		}

		public void TestValidateCredentials_ShouldUseUserPrincipalName()
		{
			ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ADTestHelper.CreateDomainCredentialsCollection());

			var notLinkedUserWithSameUserLogin = "TestUser";
			var linkUserButNotSameUserLogin = "LinkedUser";
			var staff = CreateTestStaff(notLinkedUserWithSameUserLogin, "");
			Factory.Save();

			var linkedUser = DummyDirectoryEntryWrapper.CreateUser(linkUserButNotSameUserLogin, guid: staff.GS_ActiveDirectoryObjectGuid.ToGuid());
			var notLinkedUser = DummyDirectoryEntryWrapper.CreateUser(notLinkedUserWithSameUserLogin);

			directorySearcherMock.Setup(s => s.FindUser(staff.GS_ActiveDirectoryObjectGuid.ToGuid(), It.IsAny<string>())).Returns(linkedUser);
			directorySearcherMock.Setup(s => s.FindUser(notLinkedUserWithSameUserLogin, It.IsAny<string>())).Returns(notLinkedUser);

			var provider = new WindowsUserAuthenticationProviderForTest();
			var validationProviderMock = new Mock<IValidationPrincipalContextProvider>();
			provider.GetValidationProvider_ForTest = _ => validationProviderMock.Object;
			var linkUserPassword = "linkUserPassword";
			var notLinkedUserPassword = "notLinkedUserPassword";
			validationProviderMock.Setup(z => z.ValidateCredentials(linkUserButNotSameUserLogin, linkUserPassword)).Returns(true);
			validationProviderMock.Setup(z => z.ValidateCredentials(notLinkedUserWithSameUserLogin, notLinkedUserPassword)).Returns(true);

			using (AssertDbHitsForAllFactories("Should not hit DB from any factory", new Dictionary<string, int>()))
			{
				AssertEquals("Not linked user and valid password", ValidateCredentialsResult.Invalid, provider.ValidateCredentials(staff.PK.ToGuid(), notLinkedUserPassword));
				AssertEquals("Valid user, valid password", ValidateCredentialsResult.OK, provider.ValidateCredentials(staff.PK.ToGuid(), linkUserPassword));
			}
		}

		public void TestValidateCurrentUserSession()
		{
			ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ADTestHelper.CreateDomainCredentialsCollection());

			var staff = CreateTestStaff("Starbuck", "");
			var staffNoAD = CreateTestStaff("Gloria", "");
			var staffNotCurrent = CreateTestStaff("Oldtown", "");

			Factory.Save();

			directorySearcherMock.Setup(s => s.FindUser(staff.GS_ActiveDirectoryObjectGuid.ToGuid(), TestConstants.ValidOU)).Returns(DummyDirectoryEntryWrapper.CreateUser("Starbuck"));
			directorySearcherMock.Setup(s => s.FindUser(staffNotCurrent.GS_ActiveDirectoryObjectGuid.ToGuid(), TestConstants.ValidOU)).Returns(DummyDirectoryEntryWrapper.CreateUser("Oldtown"));

			var provider = new WindowsUserAuthenticationProviderForTest();
			provider.CurrentWindowsUserGuid_ForTest = staff.GS_ActiveDirectoryObjectGuid.ToGuid();

			using (AssertDbHitsForAllFactories("Should not hit DB from any factory", new Dictionary<string, int>()))
			{
				provider.IsUserLocalAccount_ForTest = false;
				AssertEquals("Valid user", ValidateCredentialsResult.OK, provider.ValidateCurrentUserSession(staff.GS_ActiveDirectoryObjectGuid.ToGuid()));
				AssertEquals("User AD record invalid", ValidateCredentialsResult.ADRecordNotFound, provider.ValidateCurrentUserSession(staffNoAD.GS_ActiveDirectoryObjectGuid.ToGuid()));
				AssertEquals("User not linked", ValidateCredentialsResult.ADRecordNotCreated, provider.ValidateCurrentUserSession(Guid.Empty));
				AssertEquals("Invalid user", ValidateCredentialsResult.Invalid, provider.ValidateCurrentUserSession(staffNotCurrent.GS_ActiveDirectoryObjectGuid.ToGuid()));

				provider.IsUserLocalAccount_ForTest = true;
				AssertEquals("Local account", ValidateCredentialsResult.Invalid, provider.ValidateCurrentUserSession(staff.GS_ActiveDirectoryObjectGuid.ToGuid()));
			}
		}

		public void TestValidateCredentials_HandleADException()
		{
			var staff = CreateTestStaff("boo", "");
			Factory.Save();

			directorySearcherMock.Setup(s => s.FindUser(staff.GS_ActiveDirectoryObjectGuid.ToGuid(), It.IsAny<string>())).Returns(DummyDirectoryEntryWrapper.CreateUser(staff.GS_LoginName));

			var provider = new WindowsUserAuthenticationProviderForTest();
			var validationProviderMock = new Mock<IValidationPrincipalContextProvider>();
			provider.GetValidationProvider_ForTest = _ => validationProviderMock.Object;
			validationProviderMock.Setup(z => z.ValidateCredentials(It.IsAny<string>(), It.IsAny<string>())).Throws(new UnableToConnectToDomainException(null, "crap"));

			AssertEquals("AD Error", ValidateCredentialsResult.ADError, provider.ValidateCredentials(staff.PK.ToGuid(), "boo"));
		}

		GlbStaff CreateTestStaff(string userName, string domainName)
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = userName;
			staff.GS_DomainName = domainName;
			staff.GS_ActiveDirectoryObjectGuid = Guid.NewGuid();
			return staff;
		}

		protected override void TearDown()
		{
			base.TearDown();

			DirectorySearcherFactory.ClearCache();
		}

		protected override void SetUp()
		{
			base.SetUp();
			directorySearcherMock = new Mock<IDirectorySearcher>();
			DirectorySearcherFactory.DirectorySearcherOverride_ForTest = directorySearcherMock.Object;
		}
		Mock<IDirectorySearcher> directorySearcherMock;
	}

	public class WindowsUserAuthenticationProviderForTest : WindowsUserAuthenticationProvider
	{
		public Func<IDomainCredentials, IValidationPrincipalContextProvider> GetValidationProvider_ForTest { get; set; }

		public Guid CurrentWindowsUserGuid_ForTest { get; set; }

		protected override bool IsUserLocalAccount => IsUserLocalAccount_ForTest ?? base.IsUserLocalAccount;

		public bool? IsUserLocalAccount_ForTest { get; set; }

		protected override Guid GetCurrentWindowsUserGuid() => CurrentWindowsUserGuid_ForTest;

		protected override IValidationPrincipalContextProvider GetValidationProvider(IDomainCredentials domainCredentials)
		{
			if (GetValidationProvider_ForTest != null)
			{
				return GetValidationProvider_ForTest(domainCredentials);
			}
			else
			{
				return base.GetValidationProvider(domainCredentials);
			}
		}
	}
}
