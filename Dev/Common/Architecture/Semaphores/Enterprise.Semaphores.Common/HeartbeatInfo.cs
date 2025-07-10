using System;
using CargoWise.Common;

namespace Enterprise.Semaphores.Common
{
	public class HeartbeatInfo : IHeartbeatInfo
	{
		public HeartbeatInfo(Guid heartbeatId, string hostName, Guid userPk, string fullUserName, string logonCode, string emailAddress, LogonType logonType, int processId, string heartbeatType, string clientIdentifier)
			: this(heartbeatId, hostName, userPk, processId, heartbeatType, clientIdentifier)
		{
			Argument.NotNull(hostName, nameof(hostName));
			Argument.NotNull(heartbeatType, nameof(heartbeatType));
			Argument.NotNull(fullUserName, nameof(fullUserName));
			Argument.NotNull(logonCode, nameof(logonCode));
			Argument.NotNull(emailAddress, nameof(emailAddress));

			this.fullUserName = fullUserName;
			this.code = logonCode;
			this.emailAddress = emailAddress;
			this.logonType = logonType;
		}

		public HeartbeatInfo(Guid heartbeatId, string hostName, Guid userPk, int processId, string heartbeatType, string clientIdentifier)
		{
			Argument.NotNull(hostName, nameof(hostName));
			Argument.NotNull(heartbeatType, nameof(heartbeatType));

			this.heartbeatId = heartbeatId;
			this.hostName = hostName;
			this.userPk = userPk;
			this.processId = processId;
			this.heartbeatType = heartbeatType;
			this.fullUserName = string.Empty;
			this.code = string.Empty;
			this.emailAddress = string.Empty;
			this.clientIdentifier = clientIdentifier;
		}

		#region IHeartbeatInfo Members

		Guid IHeartbeatInfo.HeartbeatId
		{
			get { return heartbeatId; }
		}
		readonly Guid heartbeatId;

		string IHeartbeatInfo.HostName
		{
			get { return hostName; }
		}
		readonly string hostName;

		string IHeartbeatInfo.ClientIdentifier
		{
			get { return clientIdentifier; }
		}
		readonly string clientIdentifier;

		Guid IHeartbeatInfo.UserPk
		{
			get { return userPk; }
		}
		readonly Guid userPk;

		string IHeartbeatInfo.FullUserName
		{
			get { return fullUserName; }
		}
		readonly string fullUserName;

		int IHeartbeatInfo.ProcessId
		{
			get { return processId; }
		}
		readonly int processId;

		string IHeartbeatInfo.SessionReference
		{
			get { return null; }
		}

		string IHeartbeatInfo.LogonIdentificationCode
		{
			get { return code; }
		}
		readonly string code;

		string IHeartbeatInfo.EmailAddress
		{
			get { return emailAddress; }
		}
		readonly string emailAddress;

		LogonType IHeartbeatInfo.LogonType
		{
			get { return logonType; }
		}
		readonly LogonType logonType;

		string IHeartbeatInfo.HeartbeatType
		{
			get { return heartbeatType; }
		}
		readonly string heartbeatType;

		#endregion
	}
}
