using System;
using System.Collections.Generic;
using System.Linq;
using Azure.Identity;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.ServiceTasks.B2CSecretCheck;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace ZClientEDI.Test.ServiceTasks.B2CSecretCheck
{
	[TestedType(typeof(B2CSecretCheckServiceTask))]
	public class B2CSecretCheckServiceTaskTest : ServiceTaskTestCase<B2CSecretCheckServiceTask>
	{
		public void TestDefaultRun()
		{
			Assert("Should not be active by default.", !GetHostedServiceAttributes().Single().ActiveByDefault);
		}

		public void TestAuthenticationException()
		{
			mockB2CSecretChecker.Setup(checker => checker.IsCheckerReady(out It.Ref<string>.IsAny)).Returns(true);
			mockB2CSecretChecker.Setup(checker => checker.GetExpiryDate()).Throws(new Microsoft.Graph.ServiceException(new Microsoft.Graph.Error(), new AuthenticationFailedException("Mock authentication exception")));

			var logger = InitialiseAndRunTaskSchedule(serviceTask);

			AssertEquals("Error|Authentication failed while checking B2C secret expiry date|Azure.Identity.AuthenticationFailedException: Mock authentication exception\r\n", logger.ToString());
		}

		public void TestRun_CheckerIsNotReady()
		{
			mockB2CSecretChecker.Setup(checker => checker.IsCheckerReady(out It.Ref<string>.IsAny)).Returns((out string message) =>
			{
				message = "checker is not ready.";
				return false;
			});

			var logger = InitialiseAndRunTaskSchedule(serviceTask);

			CombineAssertions(() =>
			{
				AssertEquals($"Warning|The task cannot run due to: checker is not ready.\r\n", logger.ToString());
				mockB2CSecretChecker.Verify(checker => checker.GetExpiryDate(), Times.Never());
			});
		}

		public void TestRun_SecretIsNotAboutToExpiredYet()
		{
			var expiredDate = DateTime.UtcNow.AddDays(31);
			mockB2CSecretChecker.Setup(checker => checker.IsCheckerReady(out It.Ref<string>.IsAny)).Returns(true);
			mockB2CSecretChecker.Setup(checker => checker.GetExpiryDate()).Returns(expiredDate);

			var logger = InitialiseAndRunTaskSchedule(serviceTask);

			AssertEquals($"Information|B2C secret is not about to expired yet, expiry date: {expiredDate}\r\n", logger.ToString());
		}

		public void TestRun_SecretIsAboutToExpired()
		{
			var staffToNotification = Factory.NewWithValidTestData<GlbStaff>();
			staffToNotification.GS_EmailAddress = "test@123.com";
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.Staff.Add(staffToNotification);
			Factory.Save();

			using (EDIDataRegistry.Instance.CertProcessingNotificationGroup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid()))
			{
				var expiredDate = DateTime.UtcNow.AddDays(29);
				mockB2CSecretChecker.Setup(checker => checker.IsCheckerReady(out It.Ref<string>.IsAny)).Returns(true);
				mockB2CSecretChecker.Setup(checker => checker.GetExpiryDate()).Returns(expiredDate);

				var logger = InitialiseAndRunTaskSchedule(serviceTask);

				AssertEquals("Information|B2C secret is about to expired, a notification email has been sent.\r\n", logger.ToString());

				AssertEquals("Notification email should be sent.", 1, Enterprise.Environment.Env.OutgoingMailManager.EmailsCreated.Count);
				CombineAssertions(() =>
				{
					AssertEquals("Warning: Azure B2C Secret about to expire", Enterprise.Environment.Env.OutgoingMailManager.EmailsCreated.First().Subject);
					AssertEquals($"The WTG B2C Secret Checker service task (code: BSC) on ediProd has detected an impending expiry of one or more secrets used by Azure B2C services. Please take the appropriate actions to renew the secret(s). Expiry date: {expiredDate}", Enterprise.Environment.Env.OutgoingMailManager.EmailsCreated.First().Body);
				});
			}
		}

		protected override void SetUpCore()
		{
			base.SetUpCore();
			mockB2CSecretChecker = new Mock<IB2CSecretChecker>();
			serviceTask = new B2CSecretCheckServiceTask(mockB2CSecretChecker.Object);
		}

		B2CSecretCheckServiceTask serviceTask;
		Mock<IB2CSecretChecker> mockB2CSecretChecker;

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();
	}
}
