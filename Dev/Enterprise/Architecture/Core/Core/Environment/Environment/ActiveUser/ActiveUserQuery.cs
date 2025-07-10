using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using CargoWise.Data;
using CargoWise.Integration;
using Enterprise.Core.Environment.Semaphores;
using Enterprise.Semaphores.Common;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Core.Environment
{
	public sealed class ActiveUserQuery : IActiveUserQuery
	{
		#region IActiveUserQuery Members

		string[] IActiveUserQuery.GetActiveUsers(bool includeCurrentUser)
		{
			return GetActiveUserSessions(includeCurrentUser).Select(userSession => GetUserInfoAsString(userSession.LoginSource, userSession)).ToArray();
		}

		string GetUserInfoAsString(string caption, IActiveUserSession userSession)
		{
			string result = string.Format("{0}: {1}|{2}|{3}|{4}|{5}",
				caption,
				userSession.FullName,
				userSession.LogonIdentificationCode,
				userSession.ComputerName,
				userSession.ProcessId.ToString(),
				(userSession.LoginTimeUtc == NullType.DateTime) ? "" : userSession.LoginTimeUtc.ToString("dd/MM/yyyy HH:mm"));

			return result;
		}

		#endregion

		/// <summary>
		/// IMPORTANT NOTE:
		///   - This method is called by DbUpgrader before the schema is updated.
		///     Therefore it must use the minimum of database references as possible
		///     to avoid conflicts with old schema versions.
		///   - Every time you need to change a column used in this method, you should
		///     provide a way of this to work for old schemas as well 
		///     (eg: try/catch and retry with old column)
		/// </summary>
		public static IActiveUserSession[] GetActiveUserSessions(bool includeCurrentUser, AdminConnection adminConnection = null)
		{
			List<IActiveUserSession> result = new List<IActiveUserSession>();
			result.AddRange(GetAllInternalActiveUserSessions(includeCurrentUser));
			result.AddRange(
				(adminConnection == null)
				? GetExternalActiveUserSessionsWithNewAdminConnection()
				: GetExternalActiveUserSessions(adminConnection)
			);
			return result.ToArray();
		}

		public static IActiveUserSession[] GetEnterpriseActiveUserSessions(bool includeCurrentUser)
		{
			return GetActiveUserSessions(includeCurrentUser, enterpriseActiveLoginSemaphore);
		}

		public static IActiveUserSession[] GetEnterpriseWinzorActiveUserSessions(bool includeCurrentUser)
		{
			return GetActiveUserSessions(includeCurrentUser, enterpriseWinzorActiveLoginSemaphore);
		}

		internal static IActiveUserSession[] GetGlowActiveUserSessions(bool includeCurrentUser)
		{
			return GetActiveUserSessions(includeCurrentUser, glowLoginSemaphore);
		}

		public static IActiveUserSession[] GetAllInternalActiveUserSessions(bool includeCurrentUser)
		{
			return GetEnterpriseActiveUserSessions(includeCurrentUser)
				.Union(GetGlowActiveUserSessions(includeCurrentUser))
				.Union(GetEnterpriseWinzorActiveUserSessions(includeCurrentUser))
				.ToArray();
		}

		static IActiveUserSession[] GetActiveUserSessions(bool includeCurrentUser, ISemaphoreType semaphoreType)
		{
			List<IActiveUserSession> userSessions = new List<IActiveUserSession>();
			ISemaphoreInfo[] activeSemaphores = EnvProxy.Instance.SemaphoreProvider.GetActiveSemaphoreHandles(semaphoreType);

			foreach (ISemaphoreInfo info in activeSemaphores)
			{
				if (includeCurrentUser || EnvProxy.Instance.CurrentUser == null
					|| (info.OwnerSession.UserPk != EnvProxy.Instance.CurrentUser.PK))
				{
					IActiveUserSession activeUser = new ActiveUserSession(
						info.OwnerSession.HeartbeatId,
						info.OwnerSession.FullUserName,
						info.OwnerSession.UserPk,
						info.OwnerSession.LogonIdentificationCode,
						info.OwnerSession.LogonType,
						info.OwnerSession.HostName,
						info.OwnerSession.ProcessId,
						info.CreateTimeUtc,
						(Object.ReferenceEquals(semaphoreType, glowLoginSemaphore) ? GlowUserLabel : ApplicationUserLabel)
					);

					userSessions.Add(activeUser);
				}
			}

			return userSessions.ToArray();
		}

		public static IActiveSemaphoreHandle[] GetEnterpriseActiveSemaphoresForUser(Guid heartbeatId)
		{
			return EnvProxy.Instance.SemaphoreProvider.GetActiveSemaphoreHandles(heartbeatId)
				.Select(info => new ActiveSemaphoreHandle(info.Semaphore.Category, info.Semaphore.LockInfo, info.Semaphore.MaxConcurrentHandles, info.CreateTimeUtc))
				.Cast<IActiveSemaphoreHandle>().ToArray();
		}

		/// <summary>
		/// The number of Enterprise users logged into the system at the moment.
		/// </summary>
		public static int GetNumberOfActiveNonSystemUsers()
		{
			string sqlText = string.Format(@"
				SELECT count(*)
				FROM {0}
				WHERE {1} = 0 AND {2} IN ({3})",
				GlbStaffSchema.Constants.TableName,
				GlbStaffSchema.Constants.GS_IsSystemAccount,
				GlbStaffSchema.Constants.PK, GetActiveUserPKsSqlList(out var paramsAction));
			return (int)Db.Connection.ExecuteScalar(sqlText, paramsAction); // User class uses complex SQL scripts that can't be accomplished by using Business Objects
		}

		#region Implementation

		static ISemaphoreType enterpriseActiveLoginSemaphore => new EnterpriseActiveLoginSemaphore();
		static ISemaphoreType enterpriseWinzorActiveLoginSemaphore => new EnterpriseWinzorActiveLoginSemaphore();
		static ISemaphoreType glowLoginSemaphore => new GlowLogonSemaphore();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "hard-coded constant")]
		static string ApplicationUserLabel => Constants.ProductName + " User";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "hard-coded constant")]
		const string GlowUserLabel = "GLOW User";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "hard-coded constant")]
		const string ExternalUserLabel = "Other Applications";

		static IActiveUserSession[] GetExternalActiveUserSessionsWithNewAdminConnection()
		{
			using (var adminConnection = Db.NewAdminConnection())
			{
				return GetExternalActiveUserSessions(adminConnection);
			}
		}

		static IActiveUserSession[] GetExternalActiveUserSessions(AdminConnection adminConnection)
		{
			string sqlText = string.Format(@"
				SELECT
					rtrim(program_name) + ' - ' + rtrim(login_name) COLLATE {0} AS [FullName],
					rtrim(host_name) COLLATE {0} [ComputerName],
					host_process_id [ProcessId]
				FROM sys.dm_exec_sessions
				WHERE host_name != '' AND database_id = db_id(@DbName) AND session_id != @@spid
				AND program_name not like ('ediEnterprise%')
				AND program_name not like ('CargoWiseOne%')
				AND program_name != 'ediWebPrint'",
				Db.DatabaseCollation);

			DataTable usersTable;
			using (adminConnection.UseMasterDb())
			{
				usersTable = Utilities.GetDataTableFromQuery(adminConnection, sqlText, ("@DbName", SqlDbType.NVarChar, 128, (object)Db.DatabaseName));
			}
			var users = new IActiveUserSession[usersTable.Rows.Count];
			int i = 0;

			foreach (DataRow row in usersTable.Rows)
			{
				users[i++] = new ActiveUserSession(
					Guid.Empty,
					Utilities.GetStringFromObject(row["FullName"]),
					Guid.Empty,
					string.Empty,
					LogonType.Unknown,
					Utilities.GetStringFromObject(row["ComputerName"]),
					Utilities.ConvertToInt32(row["ProcessId"]),
					NullType.DateTime,
					ExternalUserLabel
				);
			}

			return users;
		}

		static string GetActiveUserPKsSqlList(out Action<DbCommand> paramsAction)
		{
			var provider = EnvProxy.Instance.SemaphoreProvider;
			var activeSemaphores = provider.GetActiveSemaphoreHandles(enterpriseActiveLoginSemaphore)
				.Union(provider.GetActiveSemaphoreHandles(enterpriseWinzorActiveLoginSemaphore))
				.Union(provider.GetActiveSemaphoreHandles(glowLoginSemaphore));
			StringBuilder result = new StringBuilder();
			var count = 0;
			paramsAction = null;
			foreach (ISemaphoreInfo info in activeSemaphores)
			{
				var paramName = "@UserPk" + count;
				result.Append(string.Format("{0},", paramName));
				paramsAction += command => command.AddParameterBasedOnDbColumn(paramName, info.OwnerSession.UserPk, GlbStaffSchema.PK);
				count++;
			}
			result.Remove(result.Length - 1, 1);

			return result.ToString();
		}

		#endregion
	}
}
