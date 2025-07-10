using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTools.DbBackupAndRestore.Business;

namespace Enterprise.Client.EDI.DbRestoreKey
{
	class DbRestoreKeyGenerator : NonPersistentBusinessObject, IObsoleteValidation
	{
		public DbRestoreKeyGenerator(BusinessObjectFactory factory)
		{
		}

		public void GenerateReleaseKey()
		{
			SessionInfoForKeyGeneration restoreInfo = new SessionInfoForKeyGeneration(ServerName, DatabaseName, SessionId);
			SetReleaseKey(restoreInfo.CalculateReleaseKey());
		}

		public void ResetReleaseKey()
		{
			SetReleaseKey(ZString.Empty);
		}

		#region ServerName

		ZString fServerName;
		[CargoWise.ComponentModel.MaxLength(300)]
		public ZString ServerName
		{
			get { return fServerName; }
			set
			{
				CheckMaximumLength(ServerNameInfo, value);
				fServerName = value;
				ServerNameInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ServerNameInfo
		{
			get { return GetZPropertyInfo(nameof(ServerName)); }
		}

		#endregion

		#region DatabaseName

		ZString fDatabaseName;
		[CargoWise.ComponentModel.MaxLength(200)]
		public ZString DatabaseName
		{
			get { return fDatabaseName; }
			set
			{
				CheckMaximumLength(DatabaseNameInfo, value);
				fDatabaseName = value;
				DatabaseNameInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DatabaseNameInfo
		{
			get { return GetZPropertyInfo(nameof(DatabaseName)); }
		}

		#endregion

		#region SessionId

		ZString fSessionId;
		[CargoWise.ComponentModel.MaxLength(500)]
		public ZString SessionId
		{
			get { return fSessionId; }
			set
			{
				CheckMaximumLength(SessionIdInfo, value);
				fSessionId = value;
				SessionIdInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo SessionIdInfo
		{
			get { return GetZPropertyInfo(nameof(SessionId)); }
		}

		#endregion

		#region ReleaseKey

		ZString fReleaseKey;
		[CargoWise.ComponentModel.MaxLength(500)]
		public ZString ReleaseKey
		{
			get { return fReleaseKey; }
		}

		public ZPropertyInfo ReleaseKeyInfo
		{
			get { return GetZPropertyInfo(nameof(ReleaseKey)); }
		}

		void SetReleaseKey(ZString value)
		{
			CheckMaximumLength(ReleaseKeyInfo, value);
			fReleaseKey = value;
			ReleaseKeyInfo.RefreshBinding();
		}

		#endregion
	}
}
