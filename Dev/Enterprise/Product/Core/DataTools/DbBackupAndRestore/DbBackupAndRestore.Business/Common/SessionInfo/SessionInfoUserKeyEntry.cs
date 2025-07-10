using System;

using CargoWise.Data;

namespace Enterprise.DataTools.DbBackupAndRestore.Business
{
	public class SessionInfoUserKeyEntry : SessionInfo
	{
		#region Constructor

		public SessionInfoUserKeyEntry(string dbServer, string databaseName, string latestLog, DateTime serverDateTime)
			: base(dbServer, databaseName)
		{
			this.latestLog = latestLog;
			this.serverDateTime = serverDateTime;

			InitializeSessionDisplay();
		}

		void InitializeSessionDisplay()
		{
			string sessionIdHash = GetSessionIdHashForDate(serverDateTime);
			string timeString = serverDateTime.ToString("fffssmmHH");
			sessionDisplay = GetSessionDisplayByMergingExtraCharactersToIdHash(sessionIdHash, timeString);
		}

		#endregion

		#region Public

		public string UserEnteredReleaseKey
		{
			get { return fUserEnteredReleaseKey; }
			set { fUserEnteredReleaseKey = value; }
		}

		public bool ShouldRelease()
		{
			bool result = false;

			// Tries with current date, than -1, -2 and -3
			for (int offsetDays = 0; offsetDays >= -3; offsetDays--)
			{
				string expectedKey = CalculateReleaseKeyWithDateOffset(offsetDays);

				if (UserEnteredReleaseKey == expectedKey)
				{
					result = true;
					break;
				}
			}

			return result;
		}

		public static SessionInfoUserKeyEntry GetSessionInfo(DbConnection connection, string dbServer, string databaseName)
		{
			string latestLog = GetLatestLogKey(connection, dbServer, databaseName);
			DateTime currentDateTime = GetTargetServerCurrentDateTime(connection, dbServer, databaseName);

			return new SessionInfoUserKeyEntry(dbServer, databaseName, latestLog, currentDateTime);
		}

		#endregion

		#region Implementation

		protected static string GetLatestLogKey(DbConnection connection, string dbServer, string databaseName)
		{
			string sqlText = String.Format(@"
				DECLARE @LogString varchar(100)

				SET @LogString =
				(
					SELECT TOP 1 convert(varchar(36), SL_PK) + convert(varchar(30), SL_PostedTimeUtc, 126)
					FROM [{0}]..StmALog
					ORDER BY SL_PostedTimeUtc DESC
				)

				SELECT isnull(@LogString, '0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789A')",
				databaseName);

			string result = connection.ExecuteScalar(sqlText).ToString().Trim();

			return result;
		}

		static DateTime GetTargetServerCurrentDateTime(DbConnection connection, string dbServer, string databaseName)
		{
			string sqlText = "SELECT getdate()";
			DateTime result = Convert.ToDateTime(connection.ExecuteScalar(sqlText));
			return result;
		}

		string CalculateReleaseKeyWithDateOffset(int offsetDays)
		{
			string sessionIdHash = GetSessionIdHashWithDateOffset(offsetDays);
			string result = GetReleaseKey(sessionIdHash);

			return result;
		}

		string GetSessionIdHashWithDateOffset(int offsetDays)
		{
			DateTime offsetDate = serverDateTime.AddDays(offsetDays);
			return GetSessionIdHashForDate(offsetDate);
		}

		protected string GetSessionIdHashForDate(DateTime sessionDate)
		{
			string dateString = sessionDate.Date.ToString("yyyyMMdd");
			string hashSource = latestLog + dateString;
			return GetBase64HashFromString(hashSource);
		}

		#endregion

		readonly string latestLog;
		readonly DateTime serverDateTime;
		string fUserEnteredReleaseKey = "";
	}
}
