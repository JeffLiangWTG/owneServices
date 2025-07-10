using System;
using System.Linq;
using CargoWise.Data;
using Enterprise.Integration.Licensing;
using Moq;
using WTG.Data.SqlDbSecuritySynchroniser;

namespace Enterprise.ServiceManager.Tasks.LoginSyncServiceTask.Testing
{
	static class Helper
	{
		public static void DropDatabasePrincipals(AdminConnection connection, params string[] principals)
		{
			var synchroniser = new DatabaseSecuritySynchroniser(
				$@"-- No proposed principals or memberships
SELECT TOP 0 * FROM (VALUES
		(N'', N'', N'', N'')
) AS principalsAndMenberships (member_name, member_type, parent_role, default_schema_name)
",
				@"-- No proposed permissions
SELECT TOP 0 * FROM (VALUES
		('', N'', N'', N'', N'', N'', N'', N'')
) AS permissions (state, permission, securableType, securableSchema, securable, securableColumn, grantee, grantor)
",
				likeFilters: principals.Select(p => DataUtils.ReplaceSqlLikeWildcard(p)).ToArray(),
				notLikeFilters: Array.Empty<string>(),
				Db.DatabaseCollation,
				Mock.Of<ILogger>());

			synchroniser.Synchronise(((IDbConnectionInternals)connection).ADOConnection);
		}

		public static void DropServerPrincipals(AdminConnection connection, params string[] principals)
		{
			var serverSecuritySynchroniser = new ServerSecuritySynchroniser(
				$@"-- No proposed principals or memberships
SELECT TOP 0 * FROM (VALUES
		(N'', '', N'', NULL, NULL, N'', N'', NULL, NULL, NULL)
) AS principalsAndMenberships (member_name, member_type, parent_role, is_expiration_checked, is_policy_checked, default_database_name, default_language_name, password_hash_create, password_hash_alter, sid)
",
				@"-- No proposed permissions
SELECT TOP 0 * FROM (VALUES
		('', N'', N'', N'', N'', N'')
) AS permissions (state, permission, securableType, securable, grantee, grantor)
",
				likeFilters: principals.Select(p => DataUtils.ReplaceSqlLikeWildcard(p)).ToArray(),
				notLikeFilters: Array.Empty<string>(),
				Db.DatabaseCollation,
				Mock.Of<ILogger>());

			serverSecuritySynchroniser.Synchronise(((IDbConnectionInternals)connection).ADOConnection);
		}

		public static IProductRegistration GetProductRegistractionMock(string hostedLocation, string databaseSecurityMode)
		{
			var productRegistrationMock = new Mock<IProductRegistration>();
			var productRegistrationKeyMock = new Mock<IProductRegistrationKey>();
			productRegistrationMock.SetupGet(productRegistration => productRegistration.Key)
				.Returns(productRegistrationKeyMock.Object);
			productRegistrationKeyMock.SetupGet(productRegistrationKey => productRegistrationKey.HostedLocation)
				.Returns(hostedLocation);
			productRegistrationKeyMock.SetupGet(productRegistrationKey => productRegistrationKey.DbSecurityMode)
				.Returns(databaseSecurityMode);

			return productRegistrationMock.Object;
		}

		public static void CreateSynonym(AdminConnection adminConnection, string synonymName, string dbName, string tableName, string mainDbName)
		{
			using (((ICurrentDbControl)adminConnection).UseDatabase(mainDbName))
			{
				if (adminConnection.Exists($"FROM sys.synonyms WHERE name='{synonymName}'"))
				{
					DropSynonym();
				}

				adminConnection.ExecuteNonQuery($"CREATE SYNONYM [{synonymName}] FOR [{dbName}]..[{tableName}]");
			}

			void DropSynonym()
				=> adminConnection.ExecuteNonQuery($"DROP SYNONYM [{synonymName}];");
		}

		public static void EnsureMsdbDeniedRoleAndUsersDropped(AdminConnection connection)
		{
			foreach (var login in connection.Logins.Select(login => login.LoginName))
			{
				SqlSecurityUtils.DbUser.Drop(connection, Db.SqlMsdb, login);
			}
			SqlSecurityUtils.DbRole.Drop(connection, Db.SqlMsdb, DbRoleTypes.Constants.CwMsdbAccessDeniedRole);
		}
	}
}
