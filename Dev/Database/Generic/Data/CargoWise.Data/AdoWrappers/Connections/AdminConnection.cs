using System;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data.Providers.Common;
using CargoWise.DataProtection;
using Microsoft.Extensions.DependencyInjection;

namespace CargoWise.Data
{
	#region Interfaces

	public interface IDbLockout
	{
		DbLockoutState AcquireLockout(LockoutReason lockoutReason, bool disableLogins = false);
		DbLockoutState CheckLockoutState();
		DbLockoutState ResetLockout();
		void DisableApplicationDbLogins();
	}

	public interface IDbLoginRepair
	{
		void EnableApplicationDbLogins();
		void CheckAndFixDbLoginSids();
		void EnsureReaderDbLogin();
		void EnsureRestrictedReaderDbLogin();
		void EnsureRestrictedWriterDbLogin();
		void EnsureUnrestrictedWriterDbLogin();
		void EnsureDbLoginsCorrectlyMappedToAllDatabases(Action<string> logMessage);
		void EnsureWriterDbLoginCorrectlyMappedToAllDatabases();
		void EnsureDbLoginsHaveRightsToCurrentDatabase();
		void EnsureWriterDbLoginHasRightsToCurrentDatabase();
		bool EnsureLoginCorrectlyMappedToAllDatabases(string loginName);
		bool HandlePermissionDeniedError(string loginWithoutAccess, string dbName);
		void DropDbLoginUsersFromDatabase(string dbName, Action<string> logMessage);
		string ReaderDbLoginName { get; }
		string RestrictedReaderDbLoginName { get; }
		string RestrictedWriterDbLoginName { get; }
		string UnrestrictedWriterDbLoginName { get; }
	}

	public enum DbLockoutState
	{
		NoLockout,
		AquiredLockout,
		ResetLockout,
		ValidLockout,
		InvalidLockout
	}

	#endregion

	public partial class AdminConnection : DbConnection<OdysseyAdminCredentials>, IDbLockout, IDbLoginRepair
	{
		protected AdminConnection(string serverName, string databaseName)
			: base(GetDataProviderFactory(serverName), serverName, databaseName, null, null)
		{
			Argument.NotNullOrEmpty(serverName, nameof(serverName));
			Argument.NotNullOrEmpty(databaseName, nameof(databaseName));
		}

		static IDataProviderFactory GetDataProviderFactory(string serverName)
		{
			IServiceProvider serviceProvider;
			SqlDataProviderFactory sqlDataProviderFactory;

			if (ProtectedDataService.IsGlobalServiceProviderConfigured)
			{
				serviceProvider = ProtectedDataService.GlobalServiceProvider;
			}
			else
			{
				IServiceCollection services = new ServiceCollection();
				services.ConfigureProtectedDataFactoryServices();
				services.ConfigureProtectedDataSqlExtensions(ApplicationType.Default);
				serviceProvider = services.BuildServiceProvider();
			}

			sqlDataProviderFactory = new SqlDataProviderFactory(serviceProvider.GetRequiredService<IProtectedDataServiceFactory>().CreateEnterpriseService(serverName), serviceProvider.GetRequiredService<ISqlConnectionProvider>());
			return sqlDataProviderFactory;
		}

		internal static AdminConnection New(string serverName, string databaseName)
		{
			Argument.NotNullOrEmpty(serverName, nameof(serverName));
			Argument.NotNullOrEmpty(databaseName, nameof(databaseName));

			return new AdminConnection(serverName, databaseName);
		}

		#region Database Lockout

		#region IDbLockout Members

		public DbLockoutState AcquireLockout(LockoutReason lockoutReason = LockoutReason.Upgrade, bool disableLogins = false)
			=> DbLockout.AcquireLockout(this, lockoutReason, disableLogins);

		public override bool HandleError(System.Data.Common.DbException e)
		{
			var errorType = new DbErrorMatch(e).ExceptionType;

			if (errorType == DbErrorType.CannotOpenDbRequestedInLogin)
			{
				return false;
			}

			return base.HandleError(e);
		}

		public DbLockoutState CheckLockoutState() => DbLockout.CheckLockoutState(this);
		public DbLockoutState ResetLockout() => DbLockout.ResetLockout(this);

		void IDbLockout.DisableApplicationDbLogins()
		{
			DisableApplicationDbLogins();
		}

		#endregion

		#region Set Login Enable State

		void DisableApplicationDbLogins() => Logins.ForEach(login => login.DisableLogin());

		#endregion

		#endregion

		#region Database Specific Logins

		#region IDbLoginRepair Members

		void IDbLoginRepair.EnableApplicationDbLogins() => Logins.ForEach(login => login.EnableLogin(msg => { }));

		void IDbLoginRepair.EnsureReaderDbLogin() => ReaderDbLogin.EnsureLogin();

		void IDbLoginRepair.EnsureRestrictedReaderDbLogin() => RestrictedReaderDatabaseLogin.EnsureLogin();

		void IDbLoginRepair.EnsureRestrictedWriterDbLogin() => RestrictedWriterDatabaseLogin.EnsureLogin();

		void IDbLoginRepair.EnsureUnrestrictedWriterDbLogin() => UnrestrictedWriterDatabaseLogin.EnsureLogin();

		void IDbLoginRepair.CheckAndFixDbLoginSids() => Logins.ForEach(login => login.CheckAndFixSidIfRequired());

		bool IDbLoginRepair.EnsureLoginCorrectlyMappedToAllDatabases(string loginName)
		{
			var login = Logins.FirstOrDefault(l => l.LoginName == loginName);
			if (login != null)
			{
				login.EnsureLoginCorrectlyMappedToAllDatabases(msg => { });

				return true;
			}
			else
			{
				return false;
			}
		}

		bool IDbLoginRepair.HandlePermissionDeniedError(string loginWithoutAccess, string dbName)
		{
			var login = Logins.FirstOrDefault(l => l.LoginName == loginWithoutAccess);
			if (login != null)
			{
				return login.EnsureLoginHasRightsToRelevantDatabase(dbName);
			}
			else
			{
				return false;
			}
		}

		void IDbLoginRepair.EnsureDbLoginsCorrectlyMappedToAllDatabases(Action<string> logMessage)
		{
			foreach (var login in Logins)
			{
				login.EnableLogin(logMessage);
				login.EnsureLoginCorrectlyMappedToAllDatabases(logMessage);
			}
		}

		void IDbLoginRepair.EnsureWriterDbLoginCorrectlyMappedToAllDatabases() => RestrictedWriterDatabaseLogin.EnsureLoginCorrectlyMappedToAllDatabases(msg => { });

		void IDbLoginRepair.EnsureDbLoginsHaveRightsToCurrentDatabase() => Logins.ForEach(login => login.EnsureLoginHasRightsToCurrentDatabase());

		void IDbLoginRepair.EnsureWriterDbLoginHasRightsToCurrentDatabase() => RestrictedWriterDatabaseLogin.EnsureLoginHasRightsToCurrentDatabase();

		void IDbLoginRepair.DropDbLoginUsersFromDatabase(string dbName, Action<string> logMessage)
		{
			Logins.ForEach(login =>
			{
				login.DropUserFromDatabase(dbName);
				logMessage(string.Format(CultureInfo.InvariantCulture, $"User [{login.LoginName}] is dropped from database [{dbName}]."));
			});
		}

		string IDbLoginRepair.ReaderDbLoginName => ReaderDbLogin.LoginName;

		string IDbLoginRepair.RestrictedReaderDbLoginName => RestrictedReaderDatabaseLogin.LoginName;

		string IDbLoginRepair.RestrictedWriterDbLoginName => RestrictedWriterDatabaseLogin.LoginName;

		string IDbLoginRepair.UnrestrictedWriterDbLoginName => UnrestrictedWriterDatabaseLogin.LoginName;

		public string GetSqlLoginSid(string sqlLoginName)
		{
			Argument.NotNullOrEmpty(sqlLoginName, nameof(sqlLoginName));
			var sqlScript = @"
SELECT UPPER(sys.fn_varbintohexsubstring(1, sid, 1, 0)) AS SidStr
FROM sys.server_principals WITH(NOLOCK)
WHERE name = @sqlLoginName";
			using (var cmd = Command(sqlScript))
			{
				cmd.AddParameter("@sqlLoginName", SqlDbType.NVarChar, 128, sqlLoginName);
				return (string)cmd.ExecuteScalar();
			}
		}

		public string GetSqlLoginHashedPassword(string sqlLoginName)
		{
			Argument.NotNullOrEmpty(sqlLoginName, nameof(sqlLoginName));
			var sqlScript = @"
SELECT sys.fn_varbintohexsubstring(1, convert(varbinary(256), password_hash), 1, 0) AS HashedPassword
FROM sys.sql_logins WITH(NOLOCK)
WHERE name = @sqlLoginName";
			using (var cmd = Command(sqlScript))
			{
				cmd.AddParameter("@sqlLoginName", SqlDbType.NVarChar, 128, sqlLoginName);
				return (string)cmd.ExecuteScalar();
			}
		}

		#endregion

		RestrictedReaderDatabaseLogin RestrictedReaderDatabaseLogin => (RestrictedReaderDatabaseLogin)Logins[0];
		RestrictedWriterDatabaseLogin RestrictedWriterDatabaseLogin => (RestrictedWriterDatabaseLogin)Logins[1];
		UnrestrictedWriterDatabaseLogin UnrestrictedWriterDatabaseLogin => (UnrestrictedWriterDatabaseLogin)Logins[2];
		ReaderDatabaseLogin ReaderDbLogin => (ReaderDatabaseLogin)Logins[3];

		public DatabaseLogin[] Logins => logins ?? (logins = new DatabaseLogin[] {
			new RestrictedReaderDatabaseLogin(this),
			new RestrictedWriterDatabaseLogin(this),
			new UnrestrictedWriterDatabaseLogin(this),
			new ReaderDatabaseLogin(this),
			new CargoWiseWriterLogin(this)
		});
		DatabaseLogin[] logins;

		#endregion

		#region Server SID

		public static Guid ServerSid
		{
			get
			{
				if (serverSid == Guid.Empty)
				{
					lock (serverSidLock)
					{
						if (serverSid == Guid.Empty)
						{
							using (var sidConnection = Db.NewAdminConnection())
							{
								serverSid = sidConnection.GetAdminLoginSid();
							}
						}
					}
				}

				return serverSid;
			}
		}

		public override string UserLogin => OdysseyAdminCredentials.AdminUserName;

		static Guid serverSid;
		static readonly object serverSidLock = new object();

		public Guid GetAdminLoginSid()
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture,
				"SELECT convert(uniqueidentifier, sid) FROM sys.server_principals WHERE name = '{0}'", // May be a part of SQL expression.
				OdysseyAdminCredentials.AdminUserName);
			object scalarResult = ExecuteScalar(sqlText);
			return (scalarResult == null || scalarResult == DBNull.Value)
				? Guid.Empty
				: new Guid(scalarResult.ToString());
		}

		#endregion
	}
}
