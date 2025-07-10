using System;
using System.Diagnostics;
using System.Drawing;
using CargoWise.BrandManager;
using CargoWise.Common;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class SafeEventLogExtensionsTest : TransactionedTestCase
	{
		public void TestSafeWriteEntryToApplicationLog_WhenBrandingFactoryReturnsEmptyProductName_ShouldUseCargoWiseOne()
		{
			using (BrandingFactory.ConfigureTemporary(() => new EmptyBranding()))
			{
				AssertEquals("Pre-condition: ProductName should be empty", string.Empty, BrandingFactory.Instance.ProductName);
				AssertNoExceptionThrown(() => SafeEventLogExtensions.SafeWriteEntryToApplicationLog("I refuse to sign legislation that would allow more than eight Jan-Michael Vincents to a precinct.", EventLogEntryType.Error, ignoreAllExceptions: false));
			}
		}

		public void TestSafeWriteEntry_WhenBrandingFactoryReturnsEmptyProductName_ShouldUseCargoWiseOne()
		{
			using (BrandingFactory.ConfigureTemporary(() => new EmptyBranding()))
			{
				AssertEquals("Pre-condition: ProductName should be empty", string.Empty, BrandingFactory.Instance.ProductName);

				var log = new EventLog { Source = string.Empty };

				AssertNoExceptionThrown(() => log.SafeWriteEntry("I refuse to sign legislation that would allow more than eight Jan-Michael Vincents to a precinct.", EventLogEntryType.Error, ignoreAllExceptions: false));
			}
		}

		[TestRequiresAdministrativePrivileges("required for writing event log properly")]
		public void TestSafeWriteEntry_LengthEventLog()
		{
			UnitTestUserNotification.Instance.ClearMessages();
			Assert("Pre-condition: Clear all existing user notification message", UnitTestUserNotification.Instance.LastMessage.WasNone);

			var text = new string('c', 40000);
			AssertNoExceptionThrown("Expect not throwing out of range argument exception.",
				() => SafeEventLogExtensions.SafeWriteEntryToApplicationLog(text, EventLogEntryType.Error, ignoreAllExceptions: false));
			Assert("No user notification message", UnitTestUserNotification.Instance.LastMessage.WasNone);
		}

		public void TestSafeWriteEntry_IgnoringAllExceptions()
		{
			var invalidEventLog = new EventLog("This is not an event log");
			invalidEventLog.SafeWriteEntry(null, EventLogEntryType.Error, ignoreAllExceptions: true);

			AssertEquals(0, ExceptionReporterTestListener.Instance.Count);
			try
			{
				new EventLog("This is not an event log").SafeWriteEntry(null, EventLogEntryType.Error, ignoreAllExceptions: false);

				AssertEquals(1, ExceptionReporterTestListener.Instance.Count);
				AssertType<NullReferenceException>(ExceptionReporterTestListener.Instance[0]);
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		[ExpectNoExceptions]
		public void TestSafeWriteEntry_WithW32Exception()
		{
			SafeEventLogExtensions.useTestEmail = true;
			UnitTestUserNotification.Instance.ClearMessages();
			EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Clear();

			var eventLog = new EventLog { Source = "  !@#$%^&*()_-+={[}]|\\:;\"'<.>//?`~	" };

			eventLog.SafeWriteEntry("", EventLogEntryType.Error, ignoreAllExceptions: false);

			AssertEquals("Sent an email for InvalidOperationException", 1, EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Count);
			Assert("Did not show developer exception", UnitTestUserNotification.Instance.LastMessage.WasNone);
			AssertEquals("Did not report InvalidOperationException", 0, ExceptionReporterTestListener.Instance.Count);
		}

		public void TestNotifyControllersReportsOnlyOnce()
		{
			SafeEventLogExtensions.useTestEmail = true;
			AssertEquals("Precondition: no emails sent", 0, EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Count);
			Exception ex1 = new Exception("a");
			Exception ex2 = new DivideByZeroException("b");
			SafeEventLogExtensions.NotifyControllers("", ex1);
			AssertEquals("Sent an email", 1, EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Count);
			SafeEventLogExtensions.NotifyControllers("", ex1);
			AssertEquals("Did not send an email for duplicate", 1, EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Count);
			SafeEventLogExtensions.NotifyControllers("", new Exception("a"));
			AssertEquals("Did not send an email for new instance with same content", 1, EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Count);
			SafeEventLogExtensions.NotifyControllers("", ex2);
			AssertEquals("Sent an email for unrelated exception", 2, EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Count);
			SafeEventLogExtensions.NotifyControllers("", ex2);
			AssertEquals("Did not send an email for duplicate", 2, EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Count);
			SafeEventLogExtensions.NotifyControllers("", new DivideByZeroException("b"));
			AssertEquals("Did not send an email for new instance with same content", 2, EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Count);
			var defaultTimeoutInMinutes = SafeEventLogExtensions.timeoutInMinutes;
			SafeEventLogExtensions.timeoutInMinutes = 0;
			SafeEventLogExtensions.NotifyControllers("", ex1);
			AssertEquals("Sent an email after timeout", 3, EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Count);
			SafeEventLogExtensions.timeoutInMinutes = defaultTimeoutInMinutes;
		}

		public void TestSafeWriteEntry_NoSource()
		{
			var eventLog = new EventLog("Log123", "Machine1");
			AssertNoExceptionThrown("No exception when no source on an EventLog", () => eventLog.SafeWriteEntry("", EventLogEntryType.Error, ignoreAllExceptions: false));
			AssertEquals("eventLog Source will default to 'CargoWise' if not specified", "CargoWise", eventLog.Source);
		}

		public void TestSafeWriteEntry_UserInformationReported()
		{
			SafeEventLogExtensions.useTestEmail = true;
			UnitTestUserNotification.Instance.ClearMessages();
			EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Clear();

			var eventLog = new EventLog { Source = "  !@#$%^&*()_-+={[}]|\\:;\"'<.>//?`~	" };

			eventLog.SafeWriteEntry("", EventLogEntryType.Error, ignoreAllExceptions: false);

			AssertEquals("Sent an email for InvalidOperationException", 1, EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Count);
			AssertContains(EnvProxy.Instance.CurrentUser.LoginName, EnvProxy.Instance.OutgoingMailManager.EmailsCreated[0].Body);
			AssertContains($"{System.Environment.UserDomainName}\\{System.Environment.UserName}", EnvProxy.Instance.OutgoingMailManager.EmailsCreated[0].Body);
		}

		protected override void SetUp()
		{
			base.SetUp();

			SafeEventLogExtensions.NotifiedExceptions.Clear();
		}

		class EmptyBranding : IBranding
		{
			public string CompanyName => throw new NotImplementedException();

			public string CompanyBrandingName => throw new NotImplementedException();

			public string ProductName => string.Empty;

			public string ProductBrandingName => string.Empty;

			public string ProductWebSite => throw new NotImplementedException();

			public string ProductSupportName => throw new NotImplementedException();

			public Image CompanyLogo => throw new NotImplementedException();

			public Image CompanyLogoSmall => throw new NotImplementedException();

			public Image CompanyLogoShadow => throw new NotImplementedException();

			public Image CompanyLogoButton => throw new NotImplementedException();

			public Image ProductLogo => throw new NotImplementedException();

			public Icon ProductIcon => throw new NotImplementedException();

			public Image SplashScreenImageWithoutProductName => throw new NotImplementedException();

			public Image SplashScreenImage => throw new NotImplementedException();

			public Image LoginScreenImage => throw new NotImplementedException();

			public Image AboutScreenImage => throw new NotImplementedException();

			public Image SetSqlPasswordScreenImage => throw new NotImplementedException();
		}
	}
}
