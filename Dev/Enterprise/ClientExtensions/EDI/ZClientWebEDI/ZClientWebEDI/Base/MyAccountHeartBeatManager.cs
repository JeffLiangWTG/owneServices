using System;
using System.Linq;
using System.Web.SessionState;
using Enterprise.Semaphores.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZClientWebCargoWiseEDI.Base
{
	public class MyAccountHeartBeatManager
	{
		public MyAccountHeartBeatManager(HttpSessionState session)
		{
			this.session = session;
		}

		readonly HttpSessionState session;

		object GetDataFromUserData(string key) => session[key];

		void SetUserContext(string key, object value) => session[key] = value;

		readonly SemaphoreDbManager dbManager = new SemaphoreDbManager();

		Guid CurrentHeartbeatID
		{
			get
			{
				var userData = GetDataFromUserData(HeartbeatIDKey);
				if (userData != null)
				{
					return Guid.TryParse(userData.ToString(), out var result) ? result : Guid.Empty;
				}

				return Guid.Empty;
			}
		}

		string HostName => session.SessionID;

		int ProcessID => session.GetHashCode();

		public void RegisterOrUpdateUserContext(Guid userPk)
		{
			if (CurrentHeartbeatID == Guid.Empty)
			{
				var heartBeatId = Guid.NewGuid();
				dbManager.CreateHeartbeatInDatabase(heartBeatId, HostName, ProcessID, userPk, OrgContactSchema.Constants.Prefix, (int)TimeSpan.FromDays(1).TotalSeconds, HeartBeatType, null);
				dbManager.CreateSemaphoreHandleInTransaction(heartBeatId, LockInfo, Category, 0);
				SetUserContext(HeartbeatIDKey, heartBeatId);
			}
			else
			{
				UpdateContext(userPk);
			}
		}

		public bool HasUserContext() => CurrentHeartbeatID != Guid.Empty;

		public bool IsCurrentContextValid
		{
			get
			{
				if (CurrentHeartbeatID != Guid.Empty)
				{
					return dbManager.GetActiveSemaphoreHandles(CurrentHeartbeatID).Any(x => x.OwnerSession.HostName == HostName);
				}

				return false;
			}
		}

		public void UpdateContext(Guid userPk)
		{
			dbManager.UpdateUserContext(CurrentHeartbeatID, HostName, userPk, OrgContactSchema.Constants.Prefix, HeartBeatType);
		}

		public void RemoteLogOff(Guid userPK)
		{
			dbManager.RemoteLogoff(userPK, HostName, HeartBeatType, string.Empty);
		}

		public void DisposeCurrentLoginContext()
		{
			if (CurrentHeartbeatID != Guid.Empty)
			{
				dbManager.DeleteHeartbeatFromDatabase(CurrentHeartbeatID);
				SetUserContext(HeartbeatIDKey, Guid.Empty);
			}
		}

		const string HeartbeatIDKey = "heartbeatUniqueId";

		const string LockInfo = "MYAUserLogin";

		const string Category = "LGN";

		const string HeartBeatType = "MYA";
	}
}
