using System;
using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing.DataAccess;
using Enterprise.RemotePrinting.Server.RPSCore;
using NotificationType = CargoWise.ComponentModel.NotificationType;

namespace Enterprise.RemotePrinting.Server.Testing
{
	public class ClientUpdateErrorNotifierTest : TransactionCoordinatorTestCase
	{
		public void TestAddError_SendEmail()
		{
			var serverPK = Guid.NewGuid();
			TestConnection.ExecuteNonQuery($"insert into dbo.StmPrintServer(SPS_PK, SPS_ServerName) values('{serverPK}', 'M1')");
			TestConnection.ExecuteNonQuery($"insert into dbo.StmPrintQueue(SQ_PK, SQ_AllowPrinting, SQ_DisplayName, SQ_QueueName, SQ_SPS_Server) values(NEWID(), 1, 'Q1', 'Q1', '{serverPK}')");

			var notifier = new ClientUpdateErrorNotifier(clientInfo, emailSender, TestConnection);
			notifier.AddError("Test 1");

			AssertEquals(1, emailSender.Emails.Count);
			AssertContains("Test 1", emailSender.Emails[0]);
		}

		public void TestEmailContaintMachineNameAndCurrentClientVersion()
		{
			var serverPK = Guid.NewGuid();
			TestConnection.ExecuteNonQuery($"insert into dbo.StmPrintServer(SPS_PK, SPS_ServerName) values('{serverPK}', 'M1')");
			TestConnection.ExecuteNonQuery($"insert into dbo.StmPrintQueue(SQ_PK, SQ_AllowPrinting, SQ_DisplayName, SQ_QueueName, SQ_SPS_Server) values(NEWID(), 1, 'Q1', 'Q1', '{serverPK}')");

			var notifier = new ClientUpdateErrorNotifier(clientInfo, emailSender, TestConnection);
			notifier.AddError("Test 1");
			AssertEquals(1, emailSender.Emails.Count);
			AssertContains("Client version : '1.0'", emailSender.Emails[0]);
		}

		public void TestAddError_ShouldNotNotify_NoQueues()
		{
			var serverPK = Guid.NewGuid();
			TestConnection.ExecuteNonQuery($"insert into dbo.StmPrintServer(SPS_PK, SPS_ServerName) values('{serverPK}', 'M1')");

			// This queue is inactive: SQ_AllowPrinting = 0
			TestConnection.ExecuteNonQuery($"insert into dbo.StmPrintQueue(SQ_PK, SQ_AllowPrinting, SQ_DisplayName, SQ_QueueName, SQ_SPS_Server) values(NEWID(), 0, 'Q1', 'Q1', '{serverPK}')");

			var notifier = new ClientUpdateErrorNotifier(clientInfo, emailSender, TestConnection);
			notifier.AddError("Test 1");

			AssertEquals("Should not send errors", 0, emailSender.Emails.Count);
		}

		public void TestAddError_ShouldNotNotify_TooSoon()
		{
			var serverPK = Guid.NewGuid();
			TestConnection.ExecuteNonQuery($"insert into dbo.StmPrintServer(SPS_PK, SPS_ServerName) values('{serverPK}', 'M1')");
			TestConnection.ExecuteNonQuery($"insert into dbo.StmPrintQueue(SQ_PK, SQ_AllowPrinting, SQ_DisplayName, SQ_QueueName, SQ_SPS_Server) values(NEWID(), 1, 'Q1', 'Q1', '{serverPK}')");

			// Update last notification date
			new ClientUpdateErrorNotifier.NotificationDateTimeRegistryItem(clientInfo.MachineName).SetLastNotificationDateTimeUtc(TestConnection);

			var notifier = new ClientUpdateErrorNotifier(clientInfo, emailSender, TestConnection);
			notifier.AddError("Test 1");

			AssertEquals("Should not send errors", 0, emailSender.Emails.Count);
		}

		public void TestAddWarning()
		{
			var notifier = new ClientUpdateErrorNotifier(clientInfo, emailSender, TestConnection);
			notifier.AddWarning("Test 1");

			AssertEquals(0, emailSender.Emails.Count);
		}

		public void TestAddInfo()
		{
			var notifier = new ClientUpdateErrorNotifier(clientInfo, emailSender, TestConnection);
			notifier.Add(new Notification(NotificationType.Information, "Test 1"));

			AssertEquals(0, emailSender.Emails.Count);
		}

		protected override void SetUp()
		{
			base.SetUp();

			clientInfo = new ClientInfo
			{
				ClientVersion = "1.0",
				DotNetVersion = "4.0",
				OSVersion = "10.0",
				MachineName = "M1"
			};

			emailSender = new EmailSenderForTest();
		}

		ClientInfo clientInfo;
		EmailSenderForTest emailSender;

		class EmailSenderForTest : IEmailSender
		{
			public IList<string> Emails { get; } = new List<string>();

			public void SendEmailToGroup(Guid recipientPk, string subject, string body)
			{
				Emails.Add(recipientPk + "|" + subject + "|" + body);
			}

			public bool SendEmailToUser(string recipientStaffCode, string subject, string body)
			{
				Emails.Add(recipientStaffCode + "|" + subject + "|" + body);
				return true;
			}

			public void SendLogsEmail(string recipientEmail, byte[] fileData, string fileName, string body)
			{
				var dataLength = fileData?.Length.ToString() ?? "null";
				Emails.Add(recipientEmail + "|" + fileName + "|" + dataLength + "|" + body);
			}
		}
	}
}
