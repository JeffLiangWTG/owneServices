using System;
using System.Web;
using Enterprise.Environment;
using Enterprise.Semaphores.Common;

namespace Enterprise.Web.TestWebApplication.Common
{
	public class WebHeartbeatInfoFactory : IHeartbeatInfoFactory
	{
		public IHeartbeatInfo New()
		{
			return new HeartbeatInfo(HeartbeatId, hostName, UserPk, fullUserName, userId, userEmail, LogonType.Staff, ProcessId, HeartbeatType, null);
		}

		public static readonly Guid HeartbeatId = Guid.NewGuid();
		public static readonly Guid UserPk = Guid.NewGuid();

		readonly string userId = Env.CurrentUser.LoginName;
		readonly string fullUserName = Env.CurrentUser.FullName;
		readonly string userEmail = Env.CurrentUser.EmailAddress;
		readonly string hostName = HttpContext.Current.Request.Url.Host;

		static int ProcessId => System.Diagnostics.Process.GetCurrentProcess().Id;
		const string HeartbeatType = "TST";
	}
}
