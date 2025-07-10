using System;
using Enterprise.Semaphores.Common;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.Web.GlobalBase
{
	public class WebHeartbeatInfoFactory : IHeartbeatInfoFactory
	{
		public virtual IHeartbeatInfo New()
		{
			return new HeartbeatInfo(GetHeartbeatId(), GetHostName(), GetUserPk(), GetProcessId(), GetHeartBeatType(), null);
		}

		protected virtual Guid GetHeartbeatId() => Guid.Empty;

		protected virtual string GetHostName() => System.Environment.MachineName;

		protected virtual Guid GetUserPk()
		{
			IUser currentUser = EnvProxy.Instance.CurrentUser;
			return currentUser?.PK ?? Guid.Empty;
		}

		protected virtual int GetProcessId() => ProcessId;

		protected virtual string GetHeartBeatType() => WebHeartbeatType;

		static int ProcessId => System.Diagnostics.Process.GetCurrentProcess().Id;
		const string WebHeartbeatType = "WEB";
	}
}
