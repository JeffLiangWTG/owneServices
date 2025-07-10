using System;
using System.Diagnostics;
using System.Linq;
using CargoWise.Application;
using Enterprise.RemoteDesktopServices;
using Enterprise.Semaphores.Common;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Core.Environment.Semaphores
{
	public class EnterpriseHeartbeatInfoFactory : IHeartbeatInfoFactory
	{
		#region IHeartbeatInfoFactory Members

		IHeartbeatInfo IHeartbeatInfoFactory.New()
		{
			return new HeartbeatInfo(Guid.Empty, GetHostName(), GetUserPk(), processId, HeartbeatTypes.Enterprise, Globals.ClientIdentifier);
		}

		#endregion

		Guid GetUserPk()
		{
			IUser currentUser = EnvProxy.Instance.CurrentUser;
			return (currentUser == null) ? Guid.Empty : currentUser.PK;
		}

		string GetHostName()
		{
			var clientName = remoteClientName;
			if (Globals.ClientIdentifier != null)
			{
				clientName = Globals.ClientIdentifier.Split('/').FirstOrDefault();
			}
			return clientName == null ? machineName : machineName + '/' + clientName;
		}

		static string TerminalServiceSessionName()
		{
			var terminalService = ObjectFactory.Get<TerminalService>();
			return terminalService.IsWTSSession ? terminalService.SessionClientName() : null;
		}

		static readonly string machineName = System.Environment.MachineName;
		static readonly int processId = Process.GetCurrentProcess().Id;

#if DEBUG
		internal
#else
		static readonly
#endif
		string remoteClientName = TerminalServiceSessionName();
	}
}
