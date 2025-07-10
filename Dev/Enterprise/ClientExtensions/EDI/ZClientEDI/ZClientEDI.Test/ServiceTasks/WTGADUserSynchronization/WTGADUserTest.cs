using System;
using System.Linq;
using CargoWise.ActiveDirectory;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.WTGADUserSynchronization;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;

namespace ZClientEDI.ServiceTasks.WTGADUserSynchronization.Test
{
	[TestedType(typeof(WTGADUser))]
	class WTGADUserTest : TestCaseWithFactory
	{
		public void TestDoNotCreateUserIfCannotFindADUserFromObjectId()
		{
			var objectId = Guid.NewGuid();
			var staff = Factory.NewWithValidTestData<EDIGlbStaff>();
			staff.GS_LoginName = "Test.User";
			staff.StaffEx.GS9_EdiActiveDirectoryObjectGuid = objectId;

			var staffToNotification = Factory.NewWithValidTestData<GlbStaff>();
			staffToNotification.GS_EmailAddress = "test@123.com";
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.Staff.Add(staffToNotification);
			Factory.Save();

			using (EDIDataRegistry.Instance.NotificationGroupForADETask.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid()))
			{
				var directorySearcherMock = new Mock<IDirectorySearcher>();
				directorySearcherMock.Setup(s => s.FindOrganisationalUnit(It.IsAny<string>())).Returns(() => null);
				directorySearcherMock.Setup(s => s.FindUser(It.IsAny<Guid>(), It.IsAny<string>())).Returns(() => null);
				var mockLogger = new Mock<ILogger>();
				var wtgADUser = new WTGADUser(staff, directorySearcherMock.Object, mockLogger.Object);
				wtgADUser.Synchronise();

				AssertEquals(1, Enterprise.Environment.Env.OutgoingMailManager.EmailsCreated.Count);

				var email = Enterprise.Environment.Env.OutgoingMailManager.EmailsCreated.First();
				AssertEquals("ADE - Synchronize EDI Staffs to Active Directory", email.Subject);
				AssertEquals($"The staff 'Test.User' is linked to a AD user with object id '{objectId}', but it doesn't exist in AD. If 'Test.User' was created or activated recently please try again later.", email.Body);

				directorySearcherMock.Verify(s => s.FindUser(It.IsAny<Guid>(), It.IsAny<string>()), Times.Once);
				directorySearcherMock.Verify(s => s.FindOrganisationalUnit(It.IsAny<string>()), Times.Never);
			}
		}

		public void TestTruncateUserForPre2000UserName()
		{
			var sAMAAccount1 = "WTG.TestUser.WithVer";
			var sAMAAccount2 = "WTG.TestUser.WithV01";
			var guid = Guid.NewGuid();
			var staff = Factory.NewWithValidTestData<EDIGlbStaff>();
			staff.GS_LoginName = "TestUser.WithVeryLongUserName";
			staff.StaffEx.GS9_EdiActiveDirectoryObjectGuid = guid;
			Factory.Save();

			var sAMAccountName = "";
			var mockUserDirectoryEntry = new Mock<IUserDirectoryEntry>();
			mockUserDirectoryEntry.Setup(user => user.UserPrincipalName).Returns("WTG.TestUser.WithVeryLongUserName");
			mockUserDirectoryEntry.Setup(user => user.Guid).Returns(guid);
			mockUserDirectoryEntry.Setup(user => user.OrganisationalUnit).Returns(EDIDataRegistry.Instance.WTGActiveDirectoryCredentials.Value.OrganizationalUnitPath);
			mockUserDirectoryEntry.SetupSet(s => s[ADAttributes.SAMAccountName] = It.IsAny<string>()).Callback((string a, int b, object value) => { sAMAccountName = value.ToString(); });

			var directorySearcherMock = new Mock<IDirectorySearcher>();
			directorySearcherMock.Setup(s => s.FindUser(It.IsAny<Guid>(), It.IsAny<string>())).Returns(() => mockUserDirectoryEntry.Object);
			directorySearcherMock.Setup(s => s.FindUser(sAMAAccount1, It.IsAny<string>())).Returns(() => mockUserDirectoryEntry.Object);
			directorySearcherMock.Setup(s => s.FindUser(sAMAAccount2, It.IsAny<string>())).Returns(() => null);
			var mockLogger = new Mock<ILogger>();

			var wtgADUser = new WTGADUser(staff, directorySearcherMock.Object, mockLogger.Object);

			wtgADUser.Synchronise();
			AssertEquals("Set the pre-win2000 name with truncated name.", sAMAAccount1, sAMAccountName);
			directorySearcherMock.Verify(s => s.FindUser(It.IsAny<Guid>(), It.IsAny<string>()), Times.Once);
			directorySearcherMock.Verify(s => s.FindUser(sAMAAccount1, It.IsAny<string>()), Times.Once);
			directorySearcherMock.Verify(s => s.FindUser(sAMAAccount2, It.IsAny<string>()), Times.Never);

			wtgADUser.Synchronise();
			AssertEquals("Still set the pre-win2000 name with same truncated name.", sAMAAccount1, sAMAccountName);
			directorySearcherMock.Verify(s => s.FindUser(It.IsAny<Guid>(), It.IsAny<string>()), Times.Exactly(2));
			directorySearcherMock.Verify(s => s.FindUser(sAMAAccount1, It.IsAny<string>()), Times.Exactly(2));
			directorySearcherMock.Verify(s => s.FindUser(sAMAAccount2, It.IsAny<string>()), Times.Never);
		}

		public void TestSetGS9_EdiActiveDirectoryObjectGuidWhenADUserExists()
		{
			var guid = Guid.NewGuid();
			var staff = Factory.NewWithValidTestData<EDIGlbStaff>();
			staff.GS_LoginName = "Test.User";

			var staffToNotification = Factory.NewWithValidTestData<GlbStaff>();
			staffToNotification.GS_EmailAddress = "test@123.com";
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.Staff.Add(staffToNotification);
			Factory.Save();

			using (EDIDataRegistry.Instance.NotificationGroupForADETask.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid()))
			{
				var mockUserDirectoryEntry = new Mock<IUserDirectoryEntry>();
				mockUserDirectoryEntry.Setup(user => user.Guid).Returns(guid);
				mockUserDirectoryEntry.Setup(user => user.UserPrincipalName).Returns("WTG.Test.User");
				mockUserDirectoryEntry.Setup(user => user.OrganisationalUnit).Returns("Token_Authentication");

				var directorySearcherMock = new Mock<IDirectorySearcher>();
				directorySearcherMock.Setup(s => s.FindUser(It.IsAny<string>(), It.IsAny<string>())).Returns(() => mockUserDirectoryEntry.Object);
				var mockLogger = new Mock<ILogger>();
				var warningMessage = "";
				mockLogger.Setup(m => m.Log(LogType.Warning, It.IsAny<string>())).Callback((LogType type, string message) => warningMessage = message);
				var wtgADUser = new WTGADUser(staff, directorySearcherMock.Object, mockLogger.Object);
				wtgADUser.Synchronise();

				staff.Reload();
				AssertEquals("GS9_EdiActiveDirectoryObjectGuid should be updated to existing AD user.", guid, staff.StaffEx.GS9_EdiActiveDirectoryObjectGuid);
				AssertEquals($"Skip creating because there is AD user with same login name 'WTG.Test.User' in OU 'Token_Authentication'", warningMessage);
				directorySearcherMock.Verify(s => s.FindUser(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
			}
		}

		public void TestSetGS9_EdiActiveDirectoryObjectGuidWhenADUserExistsAndLoginNameLongerThan20Chars()
		{
			var guid = Guid.NewGuid();
			var staff = Factory.NewWithValidTestData<EDIGlbStaff>();
			staff.GS_LoginName = "TestUserName.LongerThan20Length";

			var staffToNotification = Factory.NewWithValidTestData<GlbStaff>();
			staffToNotification.GS_EmailAddress = "test@123.com";
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.Staff.Add(staffToNotification);
			Factory.Save();

			using (EDIDataRegistry.Instance.NotificationGroupForADETask.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid()))
			{
				var mockUserDirectoryEntry = new Mock<IUserDirectoryEntry>();
				mockUserDirectoryEntry.Setup(user => user.Guid).Returns(guid);
				mockUserDirectoryEntry.Setup(user => user.UserPrincipalName).Returns("WTG.TestUserName.LongerThan20Length");
				mockUserDirectoryEntry.Setup(user => user.OrganisationalUnit).Returns("Token_Authentication");
				var newSAMAccountName = string.Empty;
				mockUserDirectoryEntry.SetupSet(s => s[ADAttributes.SAMAccountName] = It.IsAny<string>()).Callback((string a, int b, object value) => { newSAMAccountName = value.ToString(); });

				var directorySearcherMock = new Mock<IDirectorySearcher>();
				directorySearcherMock.Setup(s => s.FindUser("WTG.TestUserName.LongerThan20Length", It.IsAny<string>())).Returns(() => mockUserDirectoryEntry.Object);
				directorySearcherMock.Setup(s => s.FindUser("WTG.TestUserName.Lon", It.IsAny<string>())).Returns(() => mockUserDirectoryEntry.Object);
				var mockLogger = new Mock<ILogger>();
				var warningMessage = "";
				mockLogger.Setup(m => m.Log(LogType.Warning, It.IsAny<string>())).Callback((LogType type, string message) => warningMessage = message);
				var wtgADUser = new WTGADUser(staff, directorySearcherMock.Object, mockLogger.Object);
				wtgADUser.Synchronise();

				staff.Reload();
				AssertEquals("WTG.TestUserName.Lon", newSAMAccountName);
				AssertEquals("GS9_EdiActiveDirectoryObjectGuid should be updated to existing AD user.", guid, staff.StaffEx.GS9_EdiActiveDirectoryObjectGuid);
				AssertEquals($"Skip creating because there is AD user with same login name 'WTG.TestUserName.LongerThan20Length' in OU 'Token_Authentication'", warningMessage);
				directorySearcherMock.Verify(s => s.FindUser("WTG.TestUserName.LongerThan20Length", It.IsAny<string>()), Times.Once);
				directorySearcherMock.Verify(s => s.FindUser("WTG.TestUserName.Lon", It.IsAny<string>()), Times.Once);
			}
		}

		public void TestCreateADUser_WhenADUserIsCreatedWithUserPrincipalNamePropertyInMemory()
		{
			var staff = Factory.NewWithValidTestData<EDIGlbStaff>();
			staff.GS_LoginName = "TestUser";
			Factory.Save();

			var isGuidLoaded = false;
			var userPrincipalName = "";
			var mockUserDirectoryEntry = new Mock<IUserDirectoryEntry>();
			mockUserDirectoryEntry.Setup(user => user.UserPrincipalName).Returns(() => isGuidLoaded ? null : "WTG.TestUser@sand.wtg.zone");
			mockUserDirectoryEntry.Setup(user => user.Guid).Returns(() => { isGuidLoaded = true; return Guid.NewGuid(); });
			mockUserDirectoryEntry.SetupSet(s => s.UserPrincipalName = It.IsAny<string>()).Callback((object value) => { userPrincipalName = value.ToString(); });

			var organizationalUnit = new Mock<IOrganisationalUnit>();
			organizationalUnit.Setup(x => x.CreateNewChild(It.IsAny<string>(), It.IsAny<DirectoryObjectType>(), It.IsAny<IDirectorySearcher>())).Returns(mockUserDirectoryEntry.Object);

			var directorySearcherMock = new Mock<IDirectorySearcher>();
			directorySearcherMock.Setup(directorySearcher => directorySearcher.DomainName).Returns("sand.wtg.zone");
			directorySearcherMock.Setup(s => s.FindOrganisationalUnit(It.IsAny<string>())).Returns(() => organizationalUnit.Object);
			var mockLogger = new Mock<ILogger>();

			var wtgADUser = new WTGADUser(staff, directorySearcherMock.Object, mockLogger.Object);

			wtgADUser.Synchronise();

			AssertEquals("WTG.TestUser@sand.wtg.zone", userPrincipalName);
		}

		public void TestSkipProcessWhenADUserIsNotInExpectedOU_SendNotification()
		{
			SkipProcessWhenADUserIsNotInExpectedOU_SendNotificationCore(true, true);
		}

		public void TestSkipProcessWhenADUserIsNotInExpectedOU_SendNotificationWhenStaffIsNotActive()
		{
			SkipProcessWhenADUserIsNotInExpectedOU_SendNotificationCore(false, true);
		}

		public void TestSkipProcessWhenADUserIsNotInExpectedOU_SendNotificationWhenADUserIsNotActive()
		{
			SkipProcessWhenADUserIsNotInExpectedOU_SendNotificationCore(true, false);
		}

		void SkipProcessWhenADUserIsNotInExpectedOU_SendNotificationCore(bool isStaffActive, bool isADUserActive)
		{
			var objectId = Guid.NewGuid();
			var staff = Factory.NewWithValidTestData<EDIGlbStaff>();
			staff.GS_LoginName = "Test.User";
			staff.GS_IsActive = isStaffActive;
			staff.StaffEx.GS9_EdiActiveDirectoryObjectGuid = objectId;

			var staffToNotification = Factory.NewWithValidTestData<GlbStaff>();
			staffToNotification.GS_EmailAddress = "test@123.com";
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.Staff.Add(staffToNotification);
			Factory.Save();

			using (EDIDataRegistry.Instance.NotificationGroupForADETask.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid()))
			{
				var mockUserDirectoryEntry = new Mock<IUserDirectoryEntry>();
				mockUserDirectoryEntry.Setup(user => user.Guid).Returns(objectId);
				mockUserDirectoryEntry.Setup(user => user.IsActive).Returns(isADUserActive);
				mockUserDirectoryEntry.Setup(user => user.UserPrincipalName).Returns("WTG.Test.User");
				mockUserDirectoryEntry.Setup(user => user.OrganisationalUnit).Returns("Token_Authentication");

				var directorySearcherMock = new Mock<IDirectorySearcher>();
				directorySearcherMock.Setup(s => s.FindUser(It.IsAny<Guid>(), It.IsAny<string>())).Returns(() => mockUserDirectoryEntry.Object);
				var mockLogger = new Mock<ILogger>();
				var loggerMessage = "";
				mockLogger.Setup(logger => logger.Log(It.IsAny<LogType>(), It.IsAny<string>())).Callback((LogType type, string message) => { loggerMessage += $"{type} - {message}"; });
				var wtgADUser = new WTGADUser(staff, directorySearcherMock.Object, mockLogger.Object);
				wtgADUser.Synchronise();

				AssertEquals(1, Enterprise.Environment.Env.OutgoingMailManager.EmailsCreated.Count);

				var email = Enterprise.Environment.Env.OutgoingMailManager.EmailsCreated.First();
				AssertEquals("ADE - Synchronize EDI Staffs to Active Directory", email.Subject);
				AssertEquals("The staff 'Test.User' is linked to an AD user 'WTG.Test.User' in OU 'Token_Authentication' instead of OU 'root/Accounts/Token Based Authentication'", email.Body);
				AssertEquals("Warning - The staff 'Test.User' is linked to an AD user 'WTG.Test.User' in OU 'Token_Authentication' instead of OU 'root/Accounts/Token Based Authentication'", loggerMessage);

				mockUserDirectoryEntry.Verify(entry => entry.CommitChanges(), Times.Never);
			}
		}

		public void TestSkipProcessWhenADUserIsNotInExpectedOU_NotSendNotification()
		{
			var objectId = Guid.NewGuid();
			var staff = Factory.NewWithValidTestData<EDIGlbStaff>();
			staff.GS_LoginName = "Test.User";
			staff.GS_IsActive = false;
			staff.StaffEx.GS9_EdiActiveDirectoryObjectGuid = objectId;

			var staffToNotification = Factory.NewWithValidTestData<GlbStaff>();
			staffToNotification.GS_EmailAddress = "test@123.com";
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.Staff.Add(staffToNotification);
			Factory.Save();

			using (EDIDataRegistry.Instance.NotificationGroupForADETask.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid()))
			{
				var mockUserDirectoryEntry = new Mock<IUserDirectoryEntry>();
				mockUserDirectoryEntry.Setup(user => user.Guid).Returns(objectId);
				mockUserDirectoryEntry.Setup(user => user.UserPrincipalName).Returns("WTG.Test.User");
				mockUserDirectoryEntry.Setup(user => user.OrganisationalUnit).Returns("Token_Authentication");
				mockUserDirectoryEntry.Setup(user => user.IsActive).Returns(false);

				var directorySearcherMock = new Mock<IDirectorySearcher>();
				directorySearcherMock.Setup(s => s.FindUser(It.IsAny<Guid>(), It.IsAny<string>())).Returns(() => mockUserDirectoryEntry.Object);
				var mockLogger = new Mock<ILogger>();
				var loggerMessage = "";
				mockLogger.Setup(logger => logger.Log(It.IsAny<LogType>(), It.IsAny<string>())).Callback((LogType type, string message) => { loggerMessage += $"{type} - {message}"; });
				var wtgADUser = new WTGADUser(staff, directorySearcherMock.Object, mockLogger.Object);
				wtgADUser.Synchronise();

				AssertEquals(0, Enterprise.Environment.Env.OutgoingMailManager.EmailsCreated.Count);
				AssertEquals("Information - The staff 'Test.User' is linked to an AD user 'WTG.Test.User' in OU 'Token_Authentication' instead of OU 'root/Accounts/Token Based Authentication'", loggerMessage);

				mockUserDirectoryEntry.Verify(entry => entry.CommitChanges(), Times.Never);
			}
		}
	}
}
