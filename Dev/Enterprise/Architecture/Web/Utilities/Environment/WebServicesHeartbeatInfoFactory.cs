using System;
using System.Web;
using Enterprise.Environment;
using Enterprise.Semaphores.Common;

namespace Enterprise.ZArchitecture.Web.Utilities.Environment
{
	internal class WebServicesHeartbeatInfoFactory : IHeartbeatInfoFactory
	{
		public static readonly Guid UserPk = Guid.NewGuid();

		public readonly string UserId = Env.CurrentUser.LoginName;
		public readonly string FullUserName = Env.CurrentUser.FullName;
		public readonly string UserEmail = Env.CurrentUser.EmailAddress;
		public readonly string HeartbeatType = HeartbeatTypes.Enterprise;

		public static readonly string HostName = HttpContext.Current.Request.Url.Host;
		readonly int ProcessId = new Random().Next();

		public IHeartbeatInfo New()
		{
			return new HeartbeatInfo(Guid.NewGuid(), HostName, UserPk, FullUserName, UserId, UserEmail, LogonType.Staff, ProcessId, HeartbeatType, null);
		}
	}
}
