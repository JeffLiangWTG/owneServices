using System.Text;
using CargoWise.Data;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Core.Environment.Testing
{
	sealed class ActiveUserQueryTest : TestCase
	{
		public void TestGetActiveUserSessions()
		{
			bool currentUserFound = false;
			StringBuilder assertMessageBuilder = new StringBuilder();
			IActiveUserSession[] activeUsers;

			using (var adminConnection = Db.NewAdminConnection())
			{
				activeUsers = ActiveUserQuery.GetActiveUserSessions(true, adminConnection);
			}

			foreach (IActiveUserSession user in activeUsers)
			{
				assertMessageBuilder.AppendLine(user.FullName);

				if (
					user.FullName == EnvProxy.Instance.CurrentUser.FullName &&
					user.UserPk == EnvProxy.Instance.CurrentUser.PK &&
					user.ComputerName.Contains(System.Environment.MachineName) &&
					user.ProcessId == System.Diagnostics.Process.GetCurrentProcess().Id
					)
				{
					currentUserFound = true;
					break;
				}
			}

			string assertMessage = string.Format("Current user ({0}) should be among the active users.\r\n{1}",
				EnvProxy.Instance.CurrentUser.FullName, assertMessageBuilder.ToString());
			Assert(assertMessage, currentUserFound);
		}

		public void TestGetEnterpriseActiveSemaphoresForUser()
		{
			using (var adminConnection = Db.NewAdminConnection())
			{
				var currentUserSemaphores = ActiveUserQuery.GetEnterpriseActiveSemaphoresForUser(ActiveUserQuery.GetActiveUserSessions(true, adminConnection)[0].HeartbeatId);
				Assert("There should be at least one semaphore for current user", currentUserSemaphores.Length > 0);
			}
		}
	}
}
