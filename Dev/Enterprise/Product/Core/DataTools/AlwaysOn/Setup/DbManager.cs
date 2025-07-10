#define SuppressResourceStringsCheckRegion

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using CargoWise.Common;
using CargoWise.DataProtection.Administration.SqlServer;
using static System.FormattableString;

namespace Enterprise.AlwaysOn.Setup
{
	public static class DbManager
	{
		#region Security (logins)

		/// <summary>
		/// Msg 18456, Level 14, State 1, Line 1
		/// Login failed for user 'login'.
		/// </summary>
		public const int LoginFailedForUserErrorNumber = 18456;

		/// <summary>
		/// Msg 15025, Level 16, State 1, Line 1
		/// The server principal 'login' already exists.
		/// </summary>
		public const int LoginAlreadyExistsErrorNumber = 15025;
		/// Ensure secondary server has all required database logins with the same sid.
		/// </summary>
		internal static void PropagateDatabaseLogins(ISqlExecutionContext secondaryServerContext, IEnumerable<DbLoginInfo> primaryDbLogins)
		{
			foreach (var dbLogin in primaryDbLogins)
			{
				CreateDatabaseLogin(secondaryServerContext, dbLogin.LoginName, dbLogin.Type, dbLogin.DefaultDatabase, dbLogin.LoginSid, dbLogin.PwdHash);
			}
		}

		/// <summary>
		/// TODO: Might need to repair login SID and/or PWD
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		internal static void CreateDatabaseLogin(ISqlExecutionContext sqlContext, string loginName, DbLoginInfo.LoginType loginType, string defaultDatabase, string loginSid, string pwdHash)
		{
			var databaseName = string.IsNullOrWhiteSpace(defaultDatabase) ? MasterDbName : defaultDatabase;
			var withTerms = loginType == DbLoginInfo.LoginType.SQL
				? Invariant($"WITH PASSWORD = {pwdHash} HASHED, CHECK_POLICY = OFF, ") // This is a SQL Script
				: "FROM WINDOWS WITH"; // This is a SQL Script

			if (loginType == DbLoginInfo.LoginType.SQL && !string.IsNullOrEmpty(loginSid))
			{
				withTerms += Invariant($"SID = {loginSid}, ");
			}

			var createLoginScript = Invariant(
				$@"IF NOT EXISTS (SELECT null FROM sys.server_principals WHERE [name] = '{loginName.QuoteEscapedName('\'')}') CREATE LOGIN [{loginName}] {withTerms} DEFAULT_DATABASE = [{databaseName}];"
			); // This is a SQL Script

			sqlContext.ExecuteNonQuery(createLoginScript);
		}

		#endregion // Security (logins)

		#region GetFullDomainName

		#endregion

		#region Always on

		public static bool IsDbWriteableAndOnline(ISqlExecutionContext sqlContext, string dbName)
		{
			Argument.NotNullOrEmpty(dbName, nameof(dbName));

			if (IsSecondaryReplicaDatabase(sqlContext, dbName))
			{
				return false;
			}

			const string sql = @"
SELECT CONVERT(bit, CASE WHEN EXISTS (SELECT NULL FROM sys.databases WHERE name = @dbName AND is_read_only = 0 AND state = 0) THEN 1 ELSE 0 END);
"; // this is a sql query

			var result = sqlContext.ExecuteScalar(sql, CommandType.Text, cmd => cmd.AddParameter("@dbName", DbType.String, dbName));
			return Convert.ToBoolean(result, CultureInfo.InvariantCulture);
		}

		public static bool IsSecondaryReplicaDatabase(ISqlExecutionContext sqlContext, string dbName)
		{
			Argument.NotNullOrEmpty(dbName, nameof(dbName));

			return
				IsAlwaysOnHighAvailabilityEnabled(sqlContext)
				&& IsDbPartOfAlwaysOn(sqlContext, dbName)
				&& !IsPrimaryReplicaDb(sqlContext, dbName);
		}

		public static bool IsAlwaysOnHighAvailabilityEnabled(ISqlExecutionContext sqlContext)
		{
			const string sql = @"
SELECT SERVERPROPERTY ('IsHadrEnabled') WHERE 1 = 1
"; // this is a sql query

			var result = sqlContext.ExecuteScalar(sql);
			return Convert.ToBoolean(result, CultureInfo.InvariantCulture);
		}

		public static bool IsDbPartOfAlwaysOn(ISqlExecutionContext sqlContext, string dbName)
		{
			Argument.NotNull(sqlContext, nameof(sqlContext));
			Argument.NotNullOrEmpty(dbName, nameof(dbName));

			const string sql = @"
SELECT CONVERT(bit, CASE WHEN EXISTS(SELECT NULL FROM sys.databases WHERE name = @dbName AND group_database_id IS NOT NULL) THEN 1 ELSE 0 END)
"; // this is a sql query

			var result = sqlContext.ExecuteScalar(sql, CommandType.Text, cmd => cmd.AddParameter("@dbName", DbType.String, dbName));
			return Convert.ToBoolean(result, CultureInfo.InvariantCulture);
		}

		static bool IsPrimaryReplicaDb(ISqlExecutionContext sqlContext, string dbName)
		{
			Argument.NotNull(sqlContext, nameof(sqlContext));
			Argument.NotNullOrEmpty(dbName, nameof(dbName));

			const string sql = @"
select sys.fn_hadr_is_primary_replica(@dbName)
"; // this is a sql query

			var result = sqlContext.ExecuteScalar(sql, CommandType.Text, cmd => cmd.AddParameter("@dbName", DbType.String, dbName));
			return Convert.ToBoolean(result, CultureInfo.InvariantCulture);
		}

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		public const string MasterDbName = "master"; // This is a SQL Script
		public const string ApplicationName = "cwAlwaysOnSetup";
	}
}
