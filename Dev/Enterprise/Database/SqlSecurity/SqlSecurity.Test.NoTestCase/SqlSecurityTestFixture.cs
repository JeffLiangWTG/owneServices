using System;
using CargoWise.Data;
using CargoWise.DataProtection;
using CargoWise.DataProtection.Administration.SqlServer;
using CargoWise.DataProtection.TestFramework;
using Moq;
using WTG.Data.SqlDbSecuritySynchroniser;

namespace Enterprise.SqlSecurity.Test.NoTestCase
{
	class SqlSecurityTestFixture
	{
		//protected readonly DataProtectionTestBed dpTestBed;
		protected readonly DBCredentials testReaderCredentials;
		protected readonly DBCredentials testWriterCredentials;
		protected readonly DBCredentials testRestrictedReaderCredentials;
		protected readonly DBCredentials testRestrictedWriterCredentials;
		protected readonly DBCredentials testUnrestrictedWriterCredentials;
		public SqlSecurityTestFixture()
		{
			testReaderCredentials  = DataProtectionTestBed.Current.GetDefaultCredentialsFor<CargoWiseReaderLoginCredentials>(Helper.MainDatabaseNameOutsideTestCase);
			testWriterCredentials  = DataProtectionTestBed.Current.GetDefaultCredentialsFor<CargoWiseWriterLoginCredentials>(Helper.MainDatabaseNameOutsideTestCase);
			testRestrictedReaderCredentials  = DataProtectionTestBed.Current.GetDefaultCredentialsFor<RestrictedReaderLoginCredentials>(Helper.MainDatabaseNameOutsideTestCase);
			testRestrictedWriterCredentials  = DataProtectionTestBed.Current.GetDefaultCredentialsFor<RestrictedWriterLoginCredentials>(Helper.MainDatabaseNameOutsideTestCase);
			testUnrestrictedWriterCredentials = DataProtectionTestBed.Current.GetDefaultCredentialsFor<UnrestrictedWriterLoginCredentials>(Helper.MainDatabaseNameOutsideTestCase);
		}

		public void CreateServerTestEntities(AdminConnection connection, string databaseName)
		{
			var logins = $@"
SELECT *
FROM
	(VALUES
		(N'{testReaderCredentials.UserName}', 'S', N'', 0, 0, N'{Db.SqlMasterDb}',  N'us_english', PWDENCRYPT(N'{testReaderCredentials.Password}'), NULL, {SqlServerLoginUtilities.ComputeSqlLoginSid(testReaderCredentials.UserName)}),
		(N'{testWriterCredentials.UserName}', 'S', N'', 0, 0, N'{Db.SqlMasterDb}',  N'us_english',  PWDENCRYPT(N'{testWriterCredentials.Password}'), NULL, {SqlServerLoginUtilities.ComputeSqlLoginSid(testWriterCredentials.UserName)}),
		(N'{testRestrictedReaderCredentials.UserName}', 'S', N'', 0, 0, N'{Db.SqlMasterDb}',  N'us_english',  PWDENCRYPT(N'{testRestrictedReaderCredentials.Password}'), NULL, {SqlServerLoginUtilities.ComputeSqlLoginSid(testRestrictedReaderCredentials.UserName)}),
		(N'{testRestrictedWriterCredentials.UserName}', 'S', N'', 0, 0, N'{Db.SqlMasterDb}',  N'us_english',  PWDENCRYPT(N'{testRestrictedWriterCredentials.Password}'), NULL, {SqlServerLoginUtilities.ComputeSqlLoginSid(testRestrictedWriterCredentials.UserName)}),
		(N'{testUnrestrictedWriterCredentials.UserName}', 'S', N'', 0, 0, N'{Db.SqlMasterDb}',  N'us_english',  PWDENCRYPT(N'{testUnrestrictedWriterCredentials.Password}'), NULL, {SqlServerLoginUtilities.ComputeSqlLoginSid(testUnrestrictedWriterCredentials.UserName)})
	) AS applicationLogins (member_name, member_type, parent_role, is_expiration_checked, is_policy_checked, default_database_name, default_language_name, password_hash_create, password_hash_alter, sid)
";
			var synchroniser = new ServerSecuritySynchroniser(
				logins,
				@"-- No proposed permissions
SELECT TOP 0 * FROM (VALUES
		('', N'', N'', N'', N'', N'')
) AS permissions (state, permission, securableType, securable, grantee, grantor)
",
				likeFilters: new[]
				{
					$"{DataUtils.ReplaceSqlLikeWildcard(databaseName)}[_]%"
				},
				notLikeFilters: Array.Empty<string>(),
				Db.DatabaseCollation,
				Mock.Of<ILogger>());

			synchroniser.Synchronise(((IDbConnectionInternals)connection).ADOConnection);
		}
	}
}
