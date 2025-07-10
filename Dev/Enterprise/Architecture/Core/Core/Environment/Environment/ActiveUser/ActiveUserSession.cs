using System;
using Enterprise.Semaphores.Common;

namespace Enterprise.Core.Environment
{
	public interface IActiveUserSession
	{
		Guid HeartbeatId { get; }
		string FullName { get; }
		Guid UserPk { get; }
		string LogonIdentificationCode { get; }
		LogonType UserType { get; }
		string ComputerName { get; }
		int ProcessId { get; }
		DateTime LoginTimeUtc { get; }
		string LoginSource { get; }
	}

	class ActiveUserSession : IActiveUserSession
	{
		public ActiveUserSession(Guid heartbeatId, string fullName, Guid userPk, string logonIdentificationCode, LogonType userType, string computerName, int processId, DateTime loginTimeUtc, string loginSource)
		{
			HeartbeatId = heartbeatId;
			FullName = fullName;
			UserPk = userPk;
			LogonIdentificationCode = logonIdentificationCode;
			UserType = userType;
			ComputerName = computerName;
			ProcessId = processId;
			LoginTimeUtc = loginTimeUtc;
			LoginSource = loginSource;
		}

		#region IActiveUserSession Members

		public Guid HeartbeatId { get; private set; }

		public string FullName { get; private set; }

		public Guid UserPk { get; private set; }

		public string LogonIdentificationCode { get; private set; }

		public LogonType UserType { get; private set; }

		public string ComputerName { get; private set; }

		public int ProcessId { get; private set; }

		public DateTime LoginTimeUtc { get; private set; }

		public string LoginSource { get; private set; }

		#endregion
	}
}
