using System;
using System.Globalization;
using System.Text;
using CargoWise.Common;
using Enterprise.EConversation.ServiceTasks;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.EConversation.Testing
{
	public abstract class EmailProcessorServiceProviderTestCase<T> : ServiceTaskTestCase<T>
			where T : EmailProcessorServiceProvider
	{
		public void TestRunTask_ServerExceptionWhileGettingStringEmailFromPosition()
		{
			var emailReaderFactory = new EmailReaderFactoryForTest(reader =>
			{
				var email = new EmailBuilderForTesting()
					.Body("BodyThatWouldNotGetSent")
					.GetEmail();
				reader.AddEmailBundle(email);
				reader.ThrowServerException = true;
			});

			CreateServiceTask(emailReaderFactory).RunTask();
			AssertLogs(
				new LogForTest(LogType.Information, $"Attempting to connect to Server:{EmailReaderForTest.TestServer} | Mailbox:{EmailReaderForTest.TestMailbox}", null),
				new LogForTest(LogType.Error, "Message unavailable", null));
		}

		public void TestRunTask_SocketExceptionWhileGettingStringEmailFromPosition()
		{
			var emailReaderFactory = new EmailReaderFactoryForTest(reader =>
			{
				var email = new EmailBuilderForTesting()
					.Body("BodyThatWouldNotGetSent")
					.GetEmail();
				reader.AddEmailBundle(email);
				reader.ThrowSocketException = true;
			});

			CreateServiceTask(emailReaderFactory).RunTask();
			AssertLogs(
				new LogForTest(LogType.Information, $"Attempting to connect to Server:{EmailReaderForTest.TestServer} | Mailbox:{EmailReaderForTest.TestMailbox}", null),
				new LogForTest(LogType.Warning, "Connection lost", null));
		}

		public void TestRunTask_SocketException_WhenSourceIsSystem()
		{
			var emailReaderFactory = new EmailReaderFactoryForTest(reader =>
			{
				var email = new EmailBuilderForTesting()
					.Body("BodyThatWouldNotGetSent")
					.GetEmail();
				reader.AddEmailBundle(email);
				reader.ThrowSocketException = true;
				reader.ExceptionSource = nameof(System);
			});

			CreateServiceTask(emailReaderFactory).RunTask();
			AssertLogs(
				new LogForTest(LogType.Information, $"Attempting to connect to Server:{EmailReaderForTest.TestServer} | Mailbox:{EmailReaderForTest.TestMailbox}", null),
				new LogForTest(LogType.Warning, "Connection lost", null));
		}

		public void TestRunTask_TimeoutException_WhenSourceIsSystem()
		{
			var emailReaderFactory = new EmailReaderFactoryForTest(reader =>
			{
				var email = new EmailBuilderForTesting()
					.Body("BodyThatWouldNotGetSent")
					.GetEmail();
				reader.AddEmailBundle(email);
				reader.ThrowTimeoutException = true;
				reader.ExceptionSource = nameof(System);
			});

			CreateServiceTask(emailReaderFactory).RunTask();
			AssertLogs(
				new LogForTest(LogType.Information, $"Attempting to connect to Server:{EmailReaderForTest.TestServer} | Mailbox:{EmailReaderForTest.TestMailbox}", null),
				new LogForTest(LogType.Warning, "Connection timed out", null));
		}

		public void TestRunTask_ExceptionWhileProcessingEmail()
		{
			var emailReaderFactory = new EmailReaderFactoryForTest(reader =>
			{
				var email = new EmailBuilderForTesting().From("").Subject("Dummy").GetEmail();
				reader.AddEmailBundle(email);
			});

			var serviceProvider = CreateServiceTask(emailReaderFactory);
			var expectedException = new InvalidOperationException("Simulated processing error");
			serviceProvider.processingErrorSimulationEmail = new EmailBuilderForTesting()
				.Body("BadBody")
				.Subject("BadSubject")
				.WithAttachment("BadAttach.xml", "<data />")
				.GetEmail();
			serviceProvider.processingErrorSimulationException = expectedException;

			GlbGroup group = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			group.Staff[0].GS_EmailAddress = "bademail@cargowise.com";
			Factory.Save();
			ErrorReporter.Clear();

			serviceProvider.RunTask();
			AssertLogs(
				new LogForTest(LogType.Information, $"Attempting to connect to Server:{EmailReaderForTest.TestServer} | Mailbox:{EmailReaderForTest.TestMailbox}", null),
				new LogForTest(LogType.Information, "Email:1 Attachments:0 From: Subject:Dummy", null),
				new LogForTest(LogType.Error, string.Format(CultureInfo.InvariantCulture, "Exception processing email from NULL: {0}", expectedException.Message), null),
				new LogForTest(LogType.Information, "Processed 1 emails", null),
				new LogForTest(LogType.Information, "1 emails read, processed and deleted", null));
			AssertEquals(expectedException.GetType(), TestLogger.Logs[2].ex.GetType());
			AssertEquals("Exception email created", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
			Assert("subject", email.Subject.Contains(" error "));
			Assert("recipients", email.Recipients.Contains("bademail@cargowise.com"));
			Assert("body contains exception message", email.Body.Contains(expectedException.Message));
			AssertEquals("has attachments", 2, email.Attachments.Count);
			AssertEquals("first attachment name", "Unprocessed.eml", email.Attachments[0].DisplayName);
			AssertEquals("second attachment name", "Decoded.txt", email.Attachments[1].DisplayName);
			string rawEml = Encoding.UTF8.GetString(email.Attachments[0].Data);
			AssertContains("first attachment content", "Subject: BadSubject", rawEml);
			AssertContains("first attachment content", "BadBody", rawEml);
			AssertContains("first attachment content", "name=BadAttach.xml", rawEml);
			AssertEquals("second attachment content", "<data />", Encoding.UTF8.GetString(email.Attachments[1].Data));

			AssertEquals(expectedException.GetType(), ErrorReporter.LastExceptionReported.GetType());
			AssertEquals(ErrorReporter.LastExceptionReported.Message, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestRunTask_RecoverFromExceptionWhileProcessingEmail()
		{
			var group = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			group.Staff[0].GS_EmailAddress = "bademail@cargowise.com";
			Factory.Save();

			var email1 = new EmailBuilderForTesting()
				.Subject("Test 1")
				.Body("BodyThatWouldNotGetSent")
				.GetEmail();

			var email2 = new EmailBuilderForTesting()
				.Subject("Test 2")
				.Body("BodyThatWouldNotGetSent")
				.GetEmail();

			var email3 = new EmailBuilderForTesting()
				.Subject("Test 3")
				.Body("BodyThatWouldNotGetSent")
				.GetEmail();

			var emailReaderFactory = new EmailReaderFactoryForTest(reader =>
			{
				reader.AddEmailBundle(email1, email2, email3);
				reader.ThrowServerExceptionOnSecondEmail = true;
			});

			CreateServiceTask(emailReaderFactory).RunTask();
			AssertLogs(
				new LogForTest(LogType.Information, $"Attempting to connect to Server:{EmailReaderForTest.TestServer} | Mailbox:{EmailReaderForTest.TestMailbox}", null),
				new LogForTest(LogType.Information, "Email:1 Attachments:0 From: Subject:Test 1", null),
				new LogForTest(LogType.Error, "Server disconnected", null),
				new LogForTest(LogType.Information, "1 emails read, processed and deleted", null));
		}

		public void TestRunTaskInvalidFormat()
		{
			TestLogger.Logs.Clear();

			var emailReaderFactory = new EmailReaderFactoryForTest(reader =>
			{
				reader.AddEmailBundle("test invalid format");
			});

			GlbGroup group = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			group.Staff[0].GS_EmailAddress = "bademail@cargowise.com";
			Factory.Save();

			CreateServiceTask(emailReaderFactory).RunTask();
			AssertLogs(
				new LogForTest(LogType.Information, $"Attempting to connect to Server:{EmailReaderForTest.TestServer} | Mailbox:{EmailReaderForTest.TestMailbox}", null),
				new LogForTest(LogType.Information, "Email:1 Attachments:0 From: Subject:", null),
				new LogForTest(LogType.Information, "Processed 1 emails", null),
				new LogForTest(LogType.Information, "1 emails read, processed and deleted", null));
			ErrorReporter.Clear();
			AssertEquals("No exception email created", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestRunTaskInvalidZip()
		{
			TestLogger.Logs.Clear();

			GlbGroup group = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			group.Staff[0].GS_EmailAddress = "bademail@cargowise.com";
			Factory.Save();

			var expectedException = new IndexOutOfRangeException("Index was outside the bounds of the array.");
			var email = new EmailBuilderForTesting()
				.To("", "test@test.net.au")
				.Body("test index out of range on decoding zip")
				.Subject("Invalid Zip")
				.WithAttachment("InvalidZip.zip", "dummyAttachment").GetEmail();
			var emailReaderFactory = new EmailReaderFactoryForTest(reader => reader.AddEmailBundle(email));
			var serviceProvider = CreateServiceTask(emailReaderFactory);
			serviceProvider.processingErrorSimulationEmail = email;
			serviceProvider.processingErrorSimulationException = expectedException;

			serviceProvider.RunTask();
			AssertLogs(
				new LogForTest(LogType.Information, $"Attempting to connect to Server:{EmailReaderForTest.TestServer} | Mailbox:{EmailReaderForTest.TestMailbox}", null),
				new LogForTest(LogType.Information, "Email:1 Attachments:1 From: Subject:Invalid Zip", null),
				new LogForTest(LogType.Error, string.Format(CultureInfo.InvariantCulture, "Exception processing email from NULL: {0}", expectedException.Message), null),
				new LogForTest(LogType.Information, "Processed 1 emails", null),
				new LogForTest(LogType.Information, "1 emails read, processed and deleted", null));
			ErrorReporter.Clear();
			AssertEquals("Exception email created", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			EmailDef emailError = Env.OutgoingMailManager.EmailsCreated[0];
			Assert("subject", emailError.Subject.Contains(" error "));
			Assert("recipients", emailError.Recipients.Contains("bademail@cargowise.com"));
			Assert("body contains exception message", emailError.Body.Contains("Index was outside the bounds of the array."));
			AssertEquals("has attachments", 1, emailError.Attachments.Count);
			AssertEquals("first attachment name", "Unprocessed.eml", emailError.Attachments[0].DisplayName);
			string rawEml = Encoding.UTF8.GetString(emailError.Attachments[0].Data);
			AssertContains("first attachment content", "test index out of range on decoding zip", rawEml);
		}

		public void TestEmailProcessorLogger()
		{
			EnableVerboseModeRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var serviceTask = CreateServiceTask(null);
			serviceTask.ServiceLogger = TestLogger;
			serviceTask.SetIsVerboseModeFromRegistry();
			((IEmailProcessorLogger)serviceTask).Log(LogType.Error, false, "meh {0}", 123);
			((IEmailProcessorLogger)serviceTask).Log(LogType.Error, true, "Wont be logged");
			((IEmailProcessorLogger)serviceTask).Log(LogType.Information, false, "Another info {0}", "test");
			AssertEquals(2, TestLogger.Logs.Count);
			AssertEquals(LogType.Error, TestLogger.Logs[0].type);
			AssertEquals("meh 123", TestLogger.Logs[0].message);
			AssertEquals(LogType.Information, TestLogger.Logs[1].type);
			AssertEquals("Another info test", TestLogger.Logs[1].message);

			EnableVerboseModeRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			serviceTask.SetIsVerboseModeFromRegistry();
			((IEmailProcessorLogger)serviceTask).Log(LogType.Warning, true, "Verbose but logged");
			AssertEquals(3, TestLogger.Logs.Count);
			AssertEquals(LogType.Warning, TestLogger.Logs[2].type);
			AssertEquals("Verbose but logged", TestLogger.Logs[2].message);
		}

		protected override void SetUpCore()
		{
			base.SetUpCore();
			Env.Registry.MailServer = EmailReaderForTest.TestServer;
		}

		protected T CreateServiceTask(IEmailReaderFactory emailReaderFactory)
		{
			var task = CreateServiceTaskCore(emailReaderFactory);
			task.ServiceLogger = TestLogger;

			return task;
		}

		protected abstract T CreateServiceTaskCore(IEmailReaderFactory emailReaderFactory);

		protected abstract IRegistryItem EnableVerboseModeRegistryItem { get; }

		#region Logger

		protected void AssertLogs(params LogForTest[] logs)
		{
			string expected = string.Join("\r\n", Array.ConvertAll(logs, l => l.ToString()));
			string actual = string.Join("\r\n", TestLogger.Logs.ConvertAll(l => l.ToString()).ToArray());
			string message = string.Format(CultureInfo.InvariantCulture, "\r\nEXPECTED:\r\n{0}\r\n\r\nACTUAL:\r\n{1}\r\n", expected, actual);

			AssertEquals(message, logs.Length, TestLogger.Logs.Count);
			for (int i = 0; i < logs.Length; i++)
			{
				string lineErrorMessage = string.Format(CultureInfo.InvariantCulture, "Line differs:{0}\r\n{1}", i + 1, message);
				AssertEquals(lineErrorMessage, logs[i], TestLogger.Logs[i]);
			}
		}

		protected LoggerForTest TestLogger { get; } = new();

		#endregion

	}
}
