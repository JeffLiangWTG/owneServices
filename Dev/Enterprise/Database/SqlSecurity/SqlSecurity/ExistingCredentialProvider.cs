using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.DataProtection;
using CargoWise.DataProtection.Administration.SqlServer;
using Enterprise.MasterFiles.Business;
using Microsoft.Extensions.DependencyInjection;

namespace Enterprise.SqlSecurity
{
	class ExistingCredentialProvider
	{
		Dictionary<string, (byte[] Hash, byte[] Sid)> HashesAndSids { get; set; }

		public CommonCredentials Logins { get; private set; }

		ExistingCredentialProvider(
			Dictionary<string, (byte[] Hash, byte[] Sid)> hashesAndSids,
			CommonCredentials credentials)
		{
			HashesAndSids = hashesAndSids;
			Logins = credentials;
		}

		public byte[] GetHashForLogin(string loginName)
		{
			return HashesAndSids[loginName].Hash;
		}

		public byte[] GetSidForLogin(string loginName)
		{
			return HashesAndSids[loginName].Sid;
		}

		public static ExistingCredentialProvider Create(AdminConnection connection, string mainDatabaseName, IEnumerable<DbUserManager.StaffLoginInfo> staffLoginsInfo)
		{
			var credentials = GetActiveApplicationLoginsFromMainDatabase(connection, mainDatabaseName);

			var escapedDbReaderLogin = credentials.Reader.UserName.QuoteEscapedName('\'');
			var escapedDbWriterLogin = credentials.Writer.UserName.QuoteEscapedName('\'');
			var escapedDbRestrictedReaderLogin = credentials.RestrictedReader.UserName.QuoteEscapedName('\'');
			var escapedDbRestrictedWriterLogin = credentials.RestrictedWriter.UserName.QuoteEscapedName('\'');
			var escapedDbUnrestrictedWriterLogin = credentials.UnrestrictedWriter.UserName.QuoteEscapedName('\'');

			var passwordHashes = new Dictionary<string, (byte[] Hash, byte[] Sid)>();
			var staffLoginsPasswords = string.Join(
				$",{System.Environment.NewLine}",
				staffLoginsInfo
				.Where(staffInfo => staffInfo.DbAuthenticationMode == DbUserManager.DatabaseAuthenticationMode.Sql)
				.Select(staffInfo =>
				$"(N'{staffInfo.LoginName.QuoteEscapedName('\'')}', N'', {staffInfo.HashedPassword}, NULL, 0)"));
			connection.ExecuteReader(
				$@"
SELECT
	logins.name
	, password_hash =
		CASE isApplicationLogin
			WHEN 1 THEN IIF(PWDCOMPARE(logins.password, existingLogins.password_hash) = 1, existingLogins.password_hash, PWDENCRYPT(logins.password))
			ELSE logins.password_hash
		END
	, sid =
		CASE isApplicationLogin
			WHEN 1 THEN logins.sid
			ELSE existingLogins.sid
		END

FROM
	(VALUES
		(N'{escapedDbReaderLogin}', N'{credentials.Reader.Password.QuoteEscapedName('\'')}', NULL, {SqlServerLoginUtilities.ComputeSqlLoginSid(escapedDbReaderLogin)}, 1),
		(N'{escapedDbWriterLogin}', N'{credentials.Writer.Password.QuoteEscapedName('\'')}', NULL, {SqlServerLoginUtilities.ComputeSqlLoginSid(escapedDbWriterLogin)}, 1),
		(N'{escapedDbRestrictedReaderLogin}', N'{credentials.RestrictedReader.Password.QuoteEscapedName('\'')}', NULL, {SqlServerLoginUtilities.ComputeSqlLoginSid(escapedDbRestrictedReaderLogin)}, 1),
		(N'{escapedDbRestrictedWriterLogin}', N'{credentials.RestrictedWriter.Password.QuoteEscapedName('\'')}', NULL, {SqlServerLoginUtilities.ComputeSqlLoginSid(escapedDbRestrictedWriterLogin)}, 1),
		(N'{escapedDbUnrestrictedWriterLogin}', N'{credentials.UnrestrictedWriter.Password.QuoteEscapedName('\'')}', NULL, {SqlServerLoginUtilities.ComputeSqlLoginSid(escapedDbUnrestrictedWriterLogin)}, 1)
		{(!string.IsNullOrWhiteSpace(staffLoginsPasswords) ? $",{System.Environment.NewLine}{staffLoginsPasswords}" : string.Empty)}
	 ) AS logins (name, password, password_hash, sid, isApplicationLogin)
LEFT JOIN sys.sql_logins AS existingLogins ON existingLogins.name = logins.name
",
				dataRecord =>
				passwordHashes[(string)dataRecord["name"]] = ((byte[])dataRecord["password_hash"], dataRecord["sid"] as byte[]));

			return new ExistingCredentialProvider(passwordHashes, credentials);
		}

		public static CommonCredentials GetActiveApplicationLoginsFromMainDatabase(AdminConnection mainDbConnection, string mainDatabaseName)
		{
			var adminContextManager = ProtectedDataService.GlobalServiceProvider.GetService<IProtectedDataAdministrationSqlExecutionContextManager>()
				as CargowisePDSAdministrationSqlContextManager ?? throw new InvalidOperationException("Protected DataServices is not properly configured for SQL Server administration. The context manager is not available or is not of the expected type.");
			var stateService = ProtectedDataService.GlobalServiceProvider.GetRequiredService<IProtectedDataStateService>();

			using (var scope = adminContextManager.BeginScope(mainDbConnection, mainDbConnection))
			{
				return new CommonCredentials
				{
					Reader = stateService.GetActiveProtectedData<CargoWiseReaderLoginCredentials>(mainDbConnection.ServerName, mainDatabaseName),
					Writer = stateService.GetActiveProtectedData<CargoWiseWriterLoginCredentials>(mainDbConnection.ServerName, mainDatabaseName),
					RestrictedReader = stateService.GetActiveProtectedData<RestrictedReaderLoginCredentials>(mainDbConnection.ServerName, mainDatabaseName),
					RestrictedWriter = stateService.GetActiveProtectedData<RestrictedWriterLoginCredentials>(mainDbConnection.ServerName, mainDatabaseName),
					UnrestrictedWriter = stateService.GetActiveProtectedData<UnrestrictedWriterLoginCredentials>(mainDbConnection.ServerName, mainDatabaseName)
				};
			}
		}
	}

	class CommonCredentials
	{
		public DBCredentials Reader { get; set; }
		public DBCredentials Writer { get; set; }
		public DBCredentials RestrictedReader { get; set; }
		public DBCredentials RestrictedWriter { get; set; }
		public DBCredentials UnrestrictedWriter { get; set; }
	}
}
