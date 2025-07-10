using System;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using GlbStaff = Enterprise.MasterFiles.Business.GlbStaff;

namespace Enterprise.Startup.Testing
{
	sealed class StartupCheckPLCertificateExpirationDateTest : TestCase
	{
		public void TestShouldExecute()
		{
			Assert(CertificateExpiryCheckTask.ShouldExecute());
		}

		[TestDate(2022, 9, 15)]
		public void TestExecute()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			GlbStaff.CurrentUser.GS_IsSystemAccount = false;
			var certificate = GlbStaff.CurrentUser.GetPLWrapper().PLBPassword;

			CertificateExpiryCheckTask.Execute();

			CombineAssertions(() =>
			{
				var expectedMessage = "Your certificate will expire soon. You can continue working, but you must apply for a new one.";
				Assert("No certificate found", UnitTestUserNotification.Instance.LastMessage.WasNone);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				certificate.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
				CertificateExpiryCheckTask.Execute();
				Assert("No expiry date for certificate", UnitTestUserNotification.Instance.LastMessage.WasNone);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				certificate.GP_ExpiryDate = new DateTime(2022, 9, 22);
				CertificateExpiryCheckTask.Execute();
				AssertEquals("Warning for certificate expiry", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				certificate.GP_ExpiryDate = new DateTime(2022, 9, 30);
				CertificateExpiryCheckTask.Execute();
				Assert("No warning when certificate expiry date is sufficiently in the future", UnitTestUserNotification.Instance.LastMessage.WasNone);
			});
		}

		public void TestTaskDescription()
		{
			AssertEquals("Task description", "Check the certificate expiration date", CertificateExpiryCheckTask.TaskDescription);
		}

		StartupCheckPLCertificateExpirationDate CertificateExpiryCheckTask => certificateExpiryCheckTask ?? (certificateExpiryCheckTask = new StartupCheckPLCertificateExpirationDate());
		StartupCheckPLCertificateExpirationDate certificateExpiryCheckTask;
	}
}
