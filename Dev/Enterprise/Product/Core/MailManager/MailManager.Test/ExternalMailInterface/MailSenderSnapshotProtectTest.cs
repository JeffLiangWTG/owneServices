using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MailManager.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.MailManager.ExternalMailInterface.Testing
{
	[UseSnapshotProtection]
	sealed class MailSenderSnapshotProtectTest : TestCase
	{
		public void TestDoesntCatchSqlLockLostExceptions()
		{
			using (ObjectFactory.Substitute(new Mock<IMailSender>().Object))
			{
				var connection = Db.Connection;

				var factory = new BusinessObjectFactory(connection);
				var mailItem = factory.NewWithValidTestData<MailItem>();
				mailItem.AddRecipientForUserCommunication("jimmy@gmail.com");
				Assert("PRE: We want it to take the branch that involves a db hit within the try", mailItem.HasActiveRecipients());

				SqlApplicationLock appLock;
				Assert("PRE: Lock is acquired", connection.TryGetLock("TestDoesntCatchSqlLockLostExceptions", out appLock));

				using (appLock)
				{
					var mailSender = new MailSender();
					using (var adminConnection = Db.NewAdminConnection())
					{
						adminConnection.ExecuteNonQuery("KILL " + connection.SPID);
					}

					AssertExceptionThrown<SqlLockLostException>(() => mailSender.SendMail(new[] { mailItem }, new DummyLogger()));
				}
			}
		}
	}
}
