using System;
using System.Threading;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.Environment;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.ExternalMailInterface;
using Enterprise.MailManager.ExternalMailInterface.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.ServiceManager.Tasks.MailProcessor.Testing
{
	internal class OutboundMailTaskMultipleInstancesTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestSendEmail()
		{
			const int numOfEmail = 10;

			for (var i = 0; i < numOfEmail; i++)
			{
				var test = "Test" + i;
				var recv = "a" + i + "@bbb.cc";
				CreateEmail(test, "", recv);
			}

			Sent = null;

			const int numOfThreads = 2;
			var exceptions = new Exception[numOfThreads];
			var threads = new Thread[numOfThreads];

			for (var i = 0; i < numOfThreads; i++)
			{
				var index = i;
				exceptions[index] = null;
				threads[index] = new Thread(() =>
				{
					using (Db.DisposableActionForDbConnection())
					{
						ThreadSendEmail(ref exceptions[index]);
					}
				});
				threads[index].Start();
			}

			for (var i = 0; i < numOfThreads; i++)
			{
				threads[i].Join();
			}

			for (var i = 0; i < numOfThreads; i++)
			{
				Assert(exceptions[i] == null ? "" : exceptions[i].ToString() + "Error in thread " + i, exceptions[i] == null);
			}

			Assert("Total sent email: " + mailsSent, mailsSent == numOfEmail);
		}

		void ThreadSendEmail(ref Exception exception)
		{
			try
			{
				var omt = new OutboundMailTask { ServiceLogger = new TestServiceLogger() };
				omt.RunTask(TokenSource.Token);
			}
			catch (Exception ex)
			{
				exception = ex;
			}
		}
		public CancellationTokenSource TokenSource => new CancellationTokenSource();

		void CreateEmail(string subject, string body, params string[] recipients)
		{
			if (!existingMailDisabled)
			{
				DisableExistingMail();
				existingMailDisabled = true;
			}

			var mail = new EmailDef();
			mail.FromAddress = "Default@edi.com.au";
			mail.Subject = subject;

			foreach (var recipient in recipients)
			{
				mail.AddRecipientForUserCommunication(recipient);
			}

			mail.Body = body;
			Env.OutgoingMailManager.CreateAndSave(mail);
		}

		bool existingMailDisabled;

		void DisableExistingMail()
		{
			var sqlText = String.Format("UPDATE {0} SET {1} = '{2}' WHERE {3} = '{4}'",
					MailDBItemsSchema.Constants.TableName,
					MailDBItemsSchema.MI_Status.Name, MailStatus.Failed,
					MailDBItemsSchema.MI_Direction.Name, MailDirection.Transmit);
			Db.Connection.ExecuteNonQuery(sqlText);
		}

		protected override void SetUp()
		{
			base.SetUp();
			mailsSent = 0;

			mailSenderForTestingHolder = ObjectFactory.Substitute<IMailSender>(new MailSenderForTesting((mi) =>
			{
				Interlocked.Increment(ref mailsSent);

				if (Sent != null)
				{
					Sent(this, new SentEventArgs(mi));
				}
			}));
		}

		static int mailsSent;

		protected override void TearDown()
		{
			mailSenderForTestingHolder.Dispose();
			base.TearDown();
		}

		IDisposable mailSenderForTestingHolder;
		EventHandler<SentEventArgs> Sent;

		class SentEventArgs : EventArgs
		{
			public SentEventArgs(MailItem item)
			{
				Item = item;
			}

			public MailItem Item { get; private set; }
		}
	}
}
