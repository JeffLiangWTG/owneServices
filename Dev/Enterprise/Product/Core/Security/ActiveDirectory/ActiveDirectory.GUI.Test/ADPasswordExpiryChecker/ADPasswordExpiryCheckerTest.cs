using System;
using System.Threading.Tasks;
using CargoWise.ActiveDirectory;
using CargoWise.ActiveDirectory.TestFramework;
using CargoWise.Application;
using CargoWise.Async;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Security.ActiveDirectory.Test;
using Enterprise.ZArchitecture.Environment;
using Moq;

namespace Enterprise.Security.ActiveDirectory.GUI.Test
{
	public class ADPasswordExpiryCheckerTest : TestCaseWithFactoryAndMocks
	{
		public void TestChecker_NotificationIsDeliveredIfWithinTheDaysLimit()
		{
			var maxAge = DataRegistry.Instance.PromptPasswordChangeBeforeExpireDays - 2;
			AssertNotificationIsDelivered(maxAge, true);

			ObjectFactory.Get<IADRegistry>().DisableADPasswordChange = true;
			maxAge = DataRegistry.Instance.PromptPasswordChangeBeforeExpireDays - 2;
			AssertNotificationIsDelivered(maxAge, false);
		}

		public void TestChecker_NotificationIsNotDeliveredIfNotWithinTheDaysLimit()
		{
			var maxAge = DataRegistry.Instance.PromptPasswordChangeBeforeExpireDays + 2;
			AssertNotificationIsDelivered(maxAge, false);
		}

		void AssertNotificationIsDelivered(int passwordMaxAge, bool shouldDipslayNotification)
		{
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;

			var staff = GlbStaff.CurrentUser;
			var guid = Guid.NewGuid();
			staff.GS_ActiveDirectoryObjectGuid = guid;

			var directorySearcher = new Mock<IDirectorySearcher>(MockBehavior.Strict);
			var directoryEntry = DummyDirectoryEntryWrapper.CreateUser(staff.GS_LoginName);
			directoryEntry[ADAttributes.PasswordLastSet] = new ADLargeInteger(ZDateTime.UtcNow.ToDateTime());

			var adUser = new Mock<ADUser>(staff);
			adUser.Setup(user => user.GetNumberOfDaysTillPasswordExpiry()).Returns(passwordMaxAge);
			ADEntityProviderSubstitution.ADUser = adUser.Object;

			DirectorySearcherFactory.DirectorySearcherOverride_ForTest = directorySearcher.Object;

			directorySearcher.Setup(s => s.FindUser(guid, string.Empty)).Returns(directoryEntry);

			bool notificationDisplayed = false;
			var deliverer = new ADPasswordExpiryChecker();
			deliverer.NotificationDisplayed += (s, e) => { notificationDisplayed = true; };

			AssertEquals(false, notificationDisplayed);
			deliverer.Run();
			AssertEquals(shouldDipslayNotification, notificationDisplayed);
		}

		public void TestCheck_NoAccessToDomain()
		{
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;

			var staff = GlbStaff.CurrentUser;
			var guid = Guid.NewGuid();
			staff.GS_ActiveDirectoryObjectGuid = guid;

			var adUser = new Mock<ADUser>(staff);
			adUser.Setup(user => user.GetNumberOfDaysTillPasswordExpiry()).Throws(new NoDomainPrivilegeException());
			ADEntityProviderSubstitution.ADUser = adUser.Object;

			bool notificationDisplayed = false;
			var deliverer = new ADPasswordExpiryChecker();
			deliverer.NotificationDisplayed += (s, e) => { notificationDisplayed = true; };

			AssertEquals(false, notificationDisplayed);
			deliverer.Run();
			AssertEquals(false, notificationDisplayed);
		}

		protected override void SetUp()
		{
			base.SetUp();
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());
		}
	}
}
