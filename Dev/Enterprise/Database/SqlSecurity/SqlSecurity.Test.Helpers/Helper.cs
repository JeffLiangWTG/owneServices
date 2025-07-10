using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ActiveDirectory.TestFramework;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Integration;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security.ActiveDirectory;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using WTG.Data.SqlDbSecuritySynchroniser;

namespace Enterprise.SqlSecurity.Test
{
	public enum DatabaseTestMode
	{
		HostedInWiseCloudDedicatedServer,
		HostedInWiseCloudSharedServer,
		SelfHostedLocked,
		SelfHostedOpen,
	}

	public static class Helper
	{
		public static byte[] HexStringToBytes(string hex)
		{
			hex = hex.Substring(2);
			return Enumerable.Range(0, hex.Length)
							 .Where(x => x % 2 == 0)
							 .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
							 .ToArray();
		}

		public static class ADTestUserLongNameB
		{
			public const string Guid = "cf37d78c-4d08-44a4-a0df-1bc92be23d99";
			public const string Name = "12345678901234567890B";
			public const string NamePreWindows2000 = "12345678901234567802";
			public const string NameWithDomain = "12345678901234567890B@sand.wtg.zone";
			public const string NameWithDomainPreWindows2000 = "sand\\12345678901234567802";
			public const string Password = "Changeme1234";
		}

		public static class ADTestUserLongNameC
		{
			public const string Guid = "60003917-fe84-4961-b196-65bd8764276b";
			public const string Name = "12345678901234567890C";
			public const string NamePreWindows2000 = "12345678901234567803";
			public const string NameWithDomain = "12345678901234567890C@sand.wtg.zone";
			public const string NameWithDomainPreWindows2000 = "sand\\12345678901234567803";
			public const string Password = "Changeme1234";
		}

		public const string MainDatabaseNameOutsideTestCase = "f962da91-CustomerDb";

		public static IEnumerable<string> Databases()
		{
			yield return Db.DatabaseName;
			yield return Helper.DatabaseNameFromDatabaseType(DatabaseType.Audit);
			yield return Helper.DatabaseNameFromDatabaseType(DatabaseType.EDW);
			yield return Helper.DatabaseNameFromDatabaseType(DatabaseType.SD);
			yield return Helper.DatabaseNameFromDatabaseType(DatabaseType.UserRepository);
			yield return Helper.DatabaseNameFromDatabaseType(DatabaseType.ExclusiveRef);
			yield return Helper.DatabaseNameFromDatabaseType(DatabaseType.SharedRef);
			yield return Helper.DatabaseNameFromDatabaseType(DatabaseType.SingleSharedRef);
		}

		public static IEnumerable<DatabaseType> GetExclusiveDatabaseTypes()
		{
			yield return DatabaseType.Main;
			yield return DatabaseType.SD;
			yield return DatabaseType.Audit;
			yield return DatabaseType.EDW;
			yield return DatabaseType.UserRepository;
			yield return DatabaseType.ExclusiveRef;
		}

		public static string DatabaseTestModeDescription(DatabaseTestMode databaseTestMode)
		{
			switch (databaseTestMode)
			{
				case DatabaseTestMode.SelfHostedOpen:
					return "self hosted open";
				case DatabaseTestMode.SelfHostedLocked:
					return "self hosted locked";
				case DatabaseTestMode.HostedInWiseCloudSharedServer:
					return "Wise cloud hosted shared";
				case DatabaseTestMode.HostedInWiseCloudDedicatedServer:
					return "Wise cloud hosted dedicated";
			}

			return string.Empty;
		}

		public static string DatabaseNameFromDatabaseType(DatabaseType databaseType)
		{
			switch (databaseType)
			{
				case DatabaseType.Main:
					return Helper.MainDatabaseNameOutsideTestCase;
				case DatabaseType.Audit:
					return $"{Helper.MainDatabaseNameOutsideTestCase}{Db.AuditDatabaseSuffix}";
				case DatabaseType.SD:
					return $"{Helper.MainDatabaseNameOutsideTestCase}{Db.SDDatabaseAffix}001";
				case DatabaseType.EDW:
					return $"{Helper.MainDatabaseNameOutsideTestCase}{Db.EdwDatabaseSuffix}";
				case DatabaseType.UserRepository:
					return $"{Helper.MainDatabaseNameOutsideTestCase}{DbUserRepository.RepositoryDbSuffix}";
				case DatabaseType.ExclusiveRef:
					return $"{Helper.MainDatabaseNameOutsideTestCase}_{RefDbTableNameResolver.RefDbAffix}_Test";
				case DatabaseType.SharedRef:
					return $"CW-{RefDbTableNameResolver.RefDbAffix}-Test";
				case DatabaseType.SingleSharedRef:
					return $"{Helper.MainDatabaseNameOutsideTestCase}-{RefDbTableNameResolver.DefaultSingleRefDbName}";
				default:
					throw new NotSupportedException($"Database type '{databaseType}' is not supported");
			}
		}

		public static IDomainCredentials TestDomainCredentials
		{
			get
			{
				var testDomainsCredentialsMock = new Mock<IDomainCredentials>();
				testDomainsCredentialsMock.CallBase = true;

				testDomainsCredentialsMock.SetupGet(domainCredentials => domainCredentials.DomainName).Returns(TestConstants.Domain);
				testDomainsCredentialsMock.SetupGet(domainCredentials => domainCredentials.DomainUserName).Returns(TestConstants.ADTestAdminAccount.NameWithDomain);
				testDomainsCredentialsMock.SetupGet(domainCredentials => domainCredentials.DomainUserPassword).Returns(TestConstants.ADTestAdminAccount.Password);
				testDomainsCredentialsMock.SetupGet(domainCredentials => domainCredentials.IsDefaultDomain).Returns(true);
				testDomainsCredentialsMock.SetupGet(domainCredentials => domainCredentials.UserOrganisationalUnit).Returns(TestConstants.ValidOU);
				testDomainsCredentialsMock.SetupGet(domainCredentials => domainCredentials.GroupOrganisationalUnit).Returns(TestConstants.ValidOU);
				testDomainsCredentialsMock.SetupGet(domainCredentials => domainCredentials.DefaultPassword).Returns(DomainCredentials.DefaultPasswordValue);

				return testDomainsCredentialsMock.Object;
			}
		}

		public static IDomainCredentials TestDomainCredentialsInvalidDomain
		{
			get
			{
				var testDomainsCredentialsMock = new Mock<IDomainCredentials>();
				testDomainsCredentialsMock.CallBase = true;

				testDomainsCredentialsMock.SetupGet(domainCredentials => domainCredentials.DomainName).Returns("IvalidDomain");
				testDomainsCredentialsMock.SetupGet(domainCredentials => domainCredentials.DomainUserName).Returns($"InvalidDomain\\{TestConstants.ADTestAdminAccount.Name}");
				testDomainsCredentialsMock.SetupGet(domainCredentials => domainCredentials.DomainUserPassword).Returns(TestConstants.ADTestAdminAccount.Password);
				testDomainsCredentialsMock.SetupGet(domainCredentials => domainCredentials.IsDefaultDomain).Returns(true);
				testDomainsCredentialsMock.SetupGet(domainCredentials => domainCredentials.UserOrganisationalUnit).Returns(TestConstants.ValidOU);
				testDomainsCredentialsMock.SetupGet(domainCredentials => domainCredentials.GroupOrganisationalUnit).Returns(TestConstants.ValidOU);
				testDomainsCredentialsMock.SetupGet(domainCredentials => domainCredentials.DefaultPassword).Returns(DomainCredentials.DefaultPasswordValue);

				return testDomainsCredentialsMock.Object;
			}
		}

		public static IDedicatedSqlServerInstanceDefinition GetDedicatedSqlServerInstanceDefinitionMock(DatabaseTestMode databaseTestMode)
		{
			var dedicatedServerInstanceDefinitionMock = new Mock<IDedicatedSqlServerInstanceDefinition>();
			dedicatedServerInstanceDefinitionMock
				.Setup(dedicatedServerInstanceDefinition => dedicatedServerInstanceDefinition.IsDedicated(It.IsAny<string>()))
				.Returns(databaseTestMode == DatabaseTestMode.HostedInWiseCloudDedicatedServer);
			return dedicatedServerInstanceDefinitionMock.Object;
		}

		public static IProductRegistration GetProductRegistractionMock(DatabaseTestMode databaseTestMode)
		{
			var productRegistrationMock = new Mock<IProductRegistration>();
			var productRegistrationKeyMock = new Mock<IProductRegistrationKey>();
			productRegistrationMock.SetupGet(productRegistration => productRegistration.Key)
				.Returns(productRegistrationKeyMock.Object);
			productRegistrationKeyMock.SetupGet(productRegistrationKey => productRegistrationKey.HostedLocation)
				.Returns(databaseTestMode == DatabaseTestMode.HostedInWiseCloudSharedServer || databaseTestMode == DatabaseTestMode.HostedInWiseCloudDedicatedServer ? "SYD" : CargoWise.Licensing.Constants.NotHostedWithCargoWise);
			productRegistrationKeyMock.SetupGet(productRegistrationKey => productRegistrationKey.DbSecurityMode)
				.Returns(databaseTestMode == DatabaseTestMode.SelfHostedOpen ? DatabaseSecurityModePairList.Codes.OpenMode : DatabaseSecurityModePairList.Codes.Indeterminate);

			return productRegistrationMock.Object;
		}

		public static IADRegistry GetADRegistryMock(bool isADIntegrationEnabled, string userLoginPrefix, IDomainCredentials domainCredentials)
		{
			var adRegisteryMock = new Mock<IADRegistry>();
			adRegisteryMock.SetupGet(adRegistry => adRegistry.IsIntegrationEnabled).Returns(isADIntegrationEnabled);
			adRegisteryMock.SetupGet(adRegistry => adRegistry.DefaultDomainCredentials).Returns(domainCredentials);
			adRegisteryMock.SetupGet(adRegistry => adRegistry.UserLoginPrefix).Returns(userLoginPrefix);
			if (domainCredentials != null)
			{
				adRegisteryMock.Setup(adRegistry => adRegistry.DomainCredentialsCollection).Returns(new List<IDomainCredentials>() { domainCredentials });
			}

			return adRegisteryMock.Object;
		}

		public static IADEntityProvider GetADEntityProviderMock()
		{
			var adEntityProviderMock = new Mock<IADEntityProvider>();
			foreach (var adUserName in new (string NamePreWindows2000, string Name)[]
			{
				(TestConstants.ADTestUserAccount.Name, TestConstants.ADTestUserAccount.Name),
				(ADTestUserLongNameB.NamePreWindows2000, ADTestUserLongNameB.Name),
				(ADTestUserLongNameC.NamePreWindows2000, ADTestUserLongNameC.Name),
			})
			{
				var adUserMock = new Mock<IADUser>();
				adUserMock.SetupGet(adUser => adUser.DomainNetBiosName).Returns(TestConstants.DomainPreWin2000);
				adUserMock.SetupGet(adUser => adUser.SAMAccountName).Returns(adUserName.NamePreWindows2000);
				adUserMock.Setup(adUser => adUser.HasExistingDirectoryEntry()).Returns(true);

				adEntityProviderMock.Setup(entityProvider => entityProvider.GetADUser(It.Is<IGlbStaff>(s => s.GS_LoginName == adUserName.Name))).Returns(adUserMock.Object);
				adUserMock.Setup(adUser => adUser.DomainCredentials).Returns(Helper.TestDomainCredentials);
			}

			return adEntityProviderMock.Object;
		}

		public static string GetEnterpriseLoginFullName(string loginName, string databaseName)
		{
			return $"{DbUserRepository.GetStaffDbLoginFullPrefix(databaseName)}{loginName}";
		}

		public static void DropServerTestEntities(AdminConnection connection, string databaseName)
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
				likeFilters: new[]
				{
					$"[_]Tst[_]%",
					$"cw%Role",
					$"{DataUtils.ReplaceSqlLikeWildcard(databaseName)}[_]%",
					$"{DataUtils.ReplaceSqlLikeWildcard(DbUserRepository.GetStaffDbLoginFullPrefix(databaseName))}%",
					$"{DataUtils.ReplaceSqlLikeWildcard(TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000)}",
					$"{DataUtils.ReplaceSqlLikeWildcard(ADTestUserLongNameB.NameWithDomainPreWindows2000)}",
					$"{DataUtils.ReplaceSqlLikeWildcard(ADTestUserLongNameC.NameWithDomainPreWindows2000)}",
				},
				notLikeFilters: Array.Empty<string>(),
				Db.DatabaseCollation,
				Mock.Of<WTG.Data.SqlDbSecuritySynchroniser.ILogger>());

			serverSecuritySynchroniser.Synchronise(((IDbConnectionInternals)connection).ADOConnection);
		}

		public static void DropLoginIfExists(AdminConnection adminConnection, string loginName, string loginType)
		{
			if (ServerPrincipalExists(adminConnection, loginName, loginType))
			{
				adminConnection.ExecuteNonQuery($"DROP LOGIN {loginName.QuoteName()}");
			}
		}

		public static void DropAssymetricKeyIfExists(AdminConnection adminConnection, string asymmetricKey)
		{
			if (AsymmetricKeyExists(adminConnection, asymmetricKey))
			{
				adminConnection.ExecuteNonQuery($"DROP ASYMMETRIC KEY {asymmetricKey.QuoteName()}");
			}
		}

		public static IDictionary<string,
			(string member_name,
			string member_type,
			string parent_role,
			bool? is_expiration_checked,
			bool? is_policy_checked,
			string default_database,
			string default_language,
			byte[] passwordHash_create,
			byte[] passwordHash_alter,
			byte[] sid)>
			ServerPrincipalsList(AdminConnection connection, string principalsSelect)
		{
			var principals = new Dictionary<string, (
				string member_name,
				string member_type,
				string parent_role,
				bool? is_expiration_checked,
				bool? is_policy_checked,
				string default_database,
				string default_language,
				byte[] passwordHash_create,
				byte[] passwordHash_alter,
				byte[] sid)>();

			using (var command = connection.Command(principalsSelect))
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					var member_name = (string)reader["member_name"];
					var parent_role = (string)reader["parent_role"];

					principals.Add(
						member_name + parent_role,
						(member_name: member_name,
						member_type: (string)reader["member_type"],
						parent_role: parent_role,
						is_expiration_checked: reader["is_expiration_checked"].ToNullableBool(),
						is_policy_checked: reader["is_policy_checked"].ToNullableBool(),
						default_database: (string)reader["default_database_name"],
						default_language: (string)reader["default_language_name"],
						passwordHash_create: reader["password_hash_create"].ToNullableBytes(),
						passwordHash_alter: reader["password_hash_alter"].ToNullableBytes(),
						sid: reader["sid"].ToNullableBytes()));
				}
			}

			return principals;
		}

		public static HashSet<
			(string State,
			string Permission,
			string SecurableType,
			string Securable,
			string Grantee,
			string Grantor)>
			ServerPermissionsList(AdminConnection connection, string permissionsSelect)
		{
			var permissions =
				new HashSet<
					(string State,
					string Permission,
					string SecurableType,
					string Securable,
					string Grantee,
					string Grantor)>();

			using (var command = connection.Command(permissionsSelect))
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					permissions.Add((State: (string)reader["state"],
						Permission: (string)reader["permission"],
						SecurableType: (string)reader["securableType"],
						Securable: (string)reader["securable"],
						Grantee: (string)reader["grantee"],
						Grantor: (string)reader["grantor"]));
				}
			}

			return permissions;
		}
		public static void CleanUpStaffLoginsAndUsers(DbConnection connection)
		{
			var proposedPrincipalsAndMembershipsSql = $@" -- proposed principals and memberships
SELECT TOP 0 * FROM (VALUES
	('', '', '', NULL, NULL, '', '', NULL, NULL, NULL)
) AS PER (member_name, member_type, parent_role, is_expiration_checked, is_policy_checked, default_database_name, default_language_name, password_hash_create, password_hash_alter, sid)
";
			var proposedPermissionsSelectSql = $@"
SELECT TOP 0 * FROM (VALUES
	('', '', '', '', '', '')
) AS PER (state, permission, securableType, securable, grantee, grantor)
";

			var sqlServerSecuritySynchroniser = new ServerSecuritySynchroniser(
				proposedPrincipalsAndMembershipsSql,
				proposedPermissionsSelectSql,
				new[]
				{
					ADTestUserLongNameC.NameWithDomainPreWindows2000,
					ADTestUserLongNameB.NameWithDomainPreWindows2000,
					TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000,
					TestConstants.ADTestAdminAccount.NameWithDomainPreWindows2000,
					$"{DataUtils.ReplaceSqlLikeWildcard(DbUserRepository.GetStaffDbLoginFullPrefix(Db.DatabaseName))}%",
					"[_]Tst[_]",
				},
				Array.Empty<string>(),
				Db.DatabaseCollation,
				Mock.Of<WTG.Data.SqlDbSecuritySynchroniser.ILogger>());

			sqlServerSecuritySynchroniser.Synchronise(((IDbConnectionInternals)connection).ADOConnection, trialRun: false);

			proposedPrincipalsAndMembershipsSql = $@" -- proposed principals and memberships
SELECT TOP 0 * FROM (VALUES
	('', '', '', NULL)
) AS PER (member_name, member_type, parent_role, default_schema_name)
";
			proposedPermissionsSelectSql = $@"
SELECT TOP 0 * FROM (VALUES
	('', '', '', '', '', '', '', '')
) AS PER (state, permission, securableType, securableSchema, securable, securableColumn, grantee, grantor)
";
			foreach (var databaseName in connection.GetDatabases(DatabaseType.All))
			{
				using (((ICurrentDbControl)connection).UseDatabase(databaseName))
				{
					var sqlDatabaseSecuritySynchroniser = new DatabaseSecuritySynchroniser(
						proposedPrincipalsAndMembershipsSql,
						proposedPermissionsSelectSql,
						new[]
						{
							ADTestUserLongNameC.NameWithDomainPreWindows2000,
							ADTestUserLongNameB.NameWithDomainPreWindows2000,
							TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000,
							TestConstants.ADTestAdminAccount.NameWithDomainPreWindows2000,
							$"{DataUtils.ReplaceSqlLikeWildcard(DbUserRepository.GetStaffDbLoginFullPrefix(Db.DatabaseName))}%",
							"[_]Tst[_]",
						},
						Array.Empty<string>(),
						Db.DatabaseCollation,
						Mock.Of<WTG.Data.SqlDbSecuritySynchroniser.ILogger>());

					sqlDatabaseSecuritySynchroniser.Synchronise(((IDbConnectionInternals)connection).ADOConnection, trialRun: false);
				}
			}
		}

		public static void CleanUpGlbTables(AdminConnection connection)
		{
			connection.ExecuteNonQuery($@"
DELETE FROM dbo.GlbGroupLink;
DELETE FROM dbo.GlbGroupRole;
DELETE FROM dbo.GlbGroup;
DELETE FROM dbo.GlbStaff;
DELETE FROM dbo.GlbPerson;
");
		}

		public static void CreateGlbStaffWithDatabaseAccessRolesInDatabase(string staffLogin, string domainName, Guid? activeDirectoryObjectGuid, AdminConnection connection, params string[] dbRoles)
		{
			var passwordHash = "0X0200A7945F61A9BD3B168BC5A2D132F5151A12F927FC06D0B04C18AA515EC62B3B093F886D5D474AFE9202F3F875A6460B06BDBA1F8670A20FE29EB0D6CBED71773B629ED053";
			var groupPK = Guid.NewGuid();
			var groupsNumber = connection.ExecuteScalar<int>("SELECT COUNT(GG_PK) FROM dbo.GlbGroup WHERE GG_Code LIKE 'TG[_]%'");
			using (var command = connection.Command(
				$@"
DECLARE @staffPK AS uniqueidentifier = NEWID();
DECLARE @psersonPK AS uniqueidentifier = NEWID();

INSERT INTO dbo.GlbPerson(PER_PK, PER_FullName)
VALUES(@psersonPK, @staffLogin);

INSERT dbo.GlbStaff
(GS_PK, GS_Code, GS_LoginName, GS_DomainName, GS_SqlLoginPasswordHash, GS_ActiveDirectoryObjectGuid, GS_PER, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
VALUES
(@staffPK, 'TS{groupsNumber + 1}', @staffLogin, @domainName, {passwordHash}, @activeDirectoryObjectGuid, @psersonPK, GetUtcDate(), 'X', GetUtcDate(), 'X')

------ Database Access Groups
INSERT dbo.GlbGroup
(GG_PK, GG_Code, GG_Desc, GG_IsActive, GG_SystemCreateTimeUtc, GG_SystemCreateUser, GG_SystemLastEditTimeUtc, GG_SystemLastEditUser)
VALUES
(@groupPk, 'TG_{groupsNumber + 1:000}', 'Some random group name', 1, GetUtcDate(), 'X', GetUtcDate(), 'X')

INSERT dbo.GlbGroupLink
(GK_PK, GK_GG, GK_GS)
VALUES
(NEWID(), @groupPK, @staffPK)
"))
			{
				command.AddParameter("@staffLogin", SqlDbType.NVarChar, 128, staffLogin);
				command.AddParameter("@groupPK", SqlDbType.UniqueIdentifier, groupPK);
				command.AddParameter(
					"@activeDirectoryObjectGuid",
					SqlDbType.UniqueIdentifier,
					(object)activeDirectoryObjectGuid ?? DBNull.Value);
				command.AddParameter(
					"@domainName",
					SqlDbType.NVarChar,
					128,
					domainName ?? string.Empty);

				command.ExecuteNonQuery();

				if (dbRoles.Any())
				{
					for (var i = 0; i < dbRoles.Length; i++)
					{
						command.AddParameter($"dbRole{i}", SqlDbType.NVarChar, 128, dbRoles[i]);
					}

					command.CommandText = $@"
INSERT dbo.GlbGroupRole
(GGR_PK, GGR_RoleName, GGR_GG_Group, GGR_SystemCreateTimeUtc, GGR_SystemCreateUser, GGR_SystemLastEditTimeUtc, GGR_SystemLastEditUser)
VALUES
{string.Join(
	$",{System.Environment.NewLine}",
	dbRoles.Select((dbRole, dbRoleIndex) => $@"
(NEWID(), @dbRole{dbRoleIndex}, @groupPK, GetUtcDate(), 'X', GetUtcDate(), 'X')
"))}
";
					command.ExecuteNonQuery();
				}
			}
		}

		public static int GetStaffRolesNumber(AdminConnection connection, string staffLogin)
		{
			using (var command = connection.Command($@"
SELECT COUNT(r.GGR_RoleName)
FROM dbo.GlbStaff AS s
	JOIN dbo.GlbGroupLink AS g ON s.GS_PK = g.GK_GS
	JOIN dbo.GlbGroupRole AS r ON r.GGR_GG_Group = g.GK_GG
WHERE s.GS_LoginName = @staffLogin
"))
			{
				command.AddParameterBasedOnDbColumn("@staffLogin", staffLogin, GlbStaffSchema.GS_LoginName);
				return (int)command.ExecuteScalar();
			}
		}

		public static void DropDatabasePrincipals(AdminConnection connection)
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
				likeFilters: Array.Empty<string>(),
				notLikeFilters: new[]
				{
					"sys",
					"cdc",
					"dbo",
					"INFORMATION_SCHEMA",
					"public",
					"guest",
					"db[_]%"
				},
				Db.DatabaseCollation,
				Mock.Of<WTG.Data.SqlDbSecuritySynchroniser.ILogger>());

			synchroniser.Synchronise(((IDbConnectionInternals)connection).ADOConnection);
		}

		public static void EnsureSchemasForDatabase(AdminConnection adminConnection, string databaseName, params string[] schemaNames)
		{
			using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
			{
				foreach (var schemaName in schemaNames)
				{
					EnsureSchema(adminConnection, schemaName);
				}
			}
		}

		public static void EnsureSchema(AdminConnection adminConnection, string schemaName)
		{
			if (!Helper.SchemaExists(adminConnection, schemaName))
			{
				adminConnection.ExecuteNonQuery($"CREATE SCHEMA {schemaName.QuoteName()}");
			}
		}

		public static void EnsureSchemasMissingForDatabase(AdminConnection adminConnection, string databaseName, params string[] schemaNames)
		{
			using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
			{
				foreach (var schemaName in schemaNames)
				{
					EnsureSchemaMissing(adminConnection, schemaName);
				}
			}
		}

		public static void EnsureSchemaMissing(AdminConnection adminConnection, string schemaName)
		{
			adminConnection.ExecuteNonQuery($"DROP SCHEMA IF EXISTS {schemaName.QuoteName()}");
		}

		public static bool SchemaExists(AdminConnection adminConnection, string schemaName)
		{
			return adminConnection.Exists(
				"FROM sys.schemas WHERE name = @schemaName",
				dbCommand => dbCommand.AddParameter("@schemaName", SqlDbType.NVarChar, 128, schemaName));
		}

		public static void EnsureExtraDatabasesWithSchemas(AdminConnection adminConnection)
		{
			adminConnection.ExecuteNonQuery($@"
DROP DATABASE IF EXISTS [{Helper.DatabaseNameFromDatabaseType(DatabaseType.Audit)}];
CREATE DATABASE [{Helper.DatabaseNameFromDatabaseType(DatabaseType.Audit)}];

DROP DATABASE IF EXISTS [{Helper.DatabaseNameFromDatabaseType(DatabaseType.EDW)}];
CREATE DATABASE [{Helper.DatabaseNameFromDatabaseType(DatabaseType.EDW)}];

DROP DATABASE IF EXISTS [{Helper.DatabaseNameFromDatabaseType(DatabaseType.SD)}];
CREATE DATABASE [{Helper.DatabaseNameFromDatabaseType(DatabaseType.SD)}];

DROP DATABASE IF EXISTS [{Helper.DatabaseNameFromDatabaseType(DatabaseType.UserRepository)}];
CREATE DATABASE [{Helper.DatabaseNameFromDatabaseType(DatabaseType.UserRepository)}];

DROP DATABASE IF EXISTS [{Helper.DatabaseNameFromDatabaseType(DatabaseType.ExclusiveRef)}];
CREATE DATABASE [{Helper.DatabaseNameFromDatabaseType(DatabaseType.ExclusiveRef)}];

DROP DATABASE IF EXISTS [{Helper.DatabaseNameFromDatabaseType(DatabaseType.SharedRef)}];
CREATE DATABASE [{Helper.DatabaseNameFromDatabaseType(DatabaseType.SharedRef)}];

DROP DATABASE IF EXISTS [{Helper.DatabaseNameFromDatabaseType(DatabaseType.SingleSharedRef)}];
CREATE DATABASE [{Helper.DatabaseNameFromDatabaseType(DatabaseType.SingleSharedRef)}];
");
			var schemasToEnsure = new[] { DbSecurity.SqlHrmSchema, "OrderTracking", DbSecurity.SqlStagingSchema, DbSecurity.SqlCdcSchema };

			Helper.EnsureSchemasForDatabase(adminConnection, Db.DatabaseName, schemasToEnsure);
			Helper.EnsureSchemasForDatabase(adminConnection, Helper.DatabaseNameFromDatabaseType(DatabaseType.Audit), schemasToEnsure);
			Helper.EnsureSchemasForDatabase(adminConnection, Helper.DatabaseNameFromDatabaseType(DatabaseType.EDW), schemasToEnsure);
			Helper.EnsureSchemasForDatabase(adminConnection, Helper.DatabaseNameFromDatabaseType(DatabaseType.SD), schemasToEnsure);
			Helper.EnsureSchemasForDatabase(adminConnection, Helper.DatabaseNameFromDatabaseType(DatabaseType.UserRepository), schemasToEnsure);
			Helper.EnsureSchemasForDatabase(adminConnection, Helper.DatabaseNameFromDatabaseType(DatabaseType.ExclusiveRef), schemasToEnsure);
			Helper.EnsureSchemasForDatabase(adminConnection, Helper.DatabaseNameFromDatabaseType(DatabaseType.SharedRef), schemasToEnsure);
		}

		public static void DropExtraDatabases(AdminConnection adminConnection)
		{
			var dbsToDrop = new List<string>();
			adminConnection.ExecuteReader(
				$@"
SELECT
	name
FROM
	sys.databases
	WHERE 1=2
		OR name LIKE @dbNameLike
		OR name = @singleRefDb
		OR name = @sharedrefDb
				",
				command =>
				{
					command.AddParameter("@dbNameLike", SqlDbType.NVarChar, 1000, $"{Db.DatabaseName}[_]%");
					command.AddParameter("@singleRefDb", SqlDbType.NVarChar, 128, Helper.DatabaseNameFromDatabaseType(DatabaseType.SingleSharedRef));
					command.AddParameter("@sharedrefDb", SqlDbType.NVarChar, 128, Helper.DatabaseNameFromDatabaseType(DatabaseType.SharedRef));
				},
				dbRecord =>
				{
					dbsToDrop.Add((string)dbRecord["name"]);
				});

			foreach (var databaseName in dbsToDrop)
			{
				adminConnection.ExecuteNonQuery(
					$@"
IF EXISTS(SELECT NULL FROM sys.databases WHERE name = @dbName AND state_desc = 'OFFLINE')
	ALTER DATABASE {databaseName.QuoteName()} SET ONLINE;
DROP DATABASE IF EXISTS {databaseName.QuoteName()};
",
					cmd => cmd.AddParameter("@dbName", SqlDbType.NVarChar, 128, databaseName));
			}
		}

		public static bool AsymmetricKeyExists(AdminConnection adminConnection, string asymmetricKey)
		{
			return adminConnection.Exists(
				"FROM sys.asymmetric_keys WHERE name = @asymmetricKey",
				dbCommand => dbCommand.AddParameter("@asymmetricKey", SqlDbType.NVarChar, 128, asymmetricKey));
		}

		public static bool ServerPrincipalExists(AdminConnection adminConnection, string principalName, string principalType)
		{
			return adminConnection.Exists(
				"FROM sys.server_principals WHERE name = @principalName AND type = @principalType",
				dbCommand =>
				{
					dbCommand.AddParameter("@principalName", SqlDbType.NVarChar, 128, principalName);
					dbCommand.AddParameter("@principalType", SqlDbType.Char, 2, principalType);
				});
		}

		public static bool FunctionExists(AdminConnection adminConnection, string schemaName, string functionType, string functionName)
		{
			return adminConnection.Exists(
				@"
FROM sys.objects
WHERE 1=1
AND name = @functionName
AND type = @functionType
AND schema_id = SCHEMA_ID(@schemaName)
",
				dbCommand =>
				{
					dbCommand.AddParameter("@functionName", SqlDbType.NVarChar, 128, functionName);
					dbCommand.AddParameter("@functionType", SqlDbType.Char, 2, functionType);
					dbCommand.AddParameter("@schemaName", SqlDbType.NVarChar, 128, schemaName);
				});
		}

		public static bool TypeExists(AdminConnection adminConnection, string schemaName, string typeName)
		{
			return adminConnection.Exists(
				"FROM sys.types WHERE name = @typeName AND schema_id = SCHEMA_ID(@schemaName)",
				dbCommand =>
				{
					dbCommand.AddParameter("@typeName", SqlDbType.NVarChar, 128, typeName);
					dbCommand.AddParameter("@schemaName", SqlDbType.NVarChar, 128, schemaName);
				});
		}

		public static void CreateType(AdminConnection adminConnection, string schemaName, string typeName)
		{
			if (!Helper.TypeExists(adminConnection, schemaName, typeName))
			{
				adminConnection.ExecuteNonQuery($@"
CREATE TYPE {schemaName.QuoteName()}.{typeName.QuoteName()}
AS TABLE (
	TestColumn1 int,
	TestColumn2 char(1)
)
");
			}
		}

		public static IDictionary<string,
			(string member_name,
			string member_type,
			string parent_role,
			string default_schema)>
			DatabasePrincipalsList(AdminConnection adminConnection, string principalsSelect)
		{
			var principals = new Dictionary<string, (
				string member_name,
				string member_type,
				string parent_role,
				string default_schema)>();

			adminConnection.ExecuteReader(principalsSelect, dataRecord =>
			{
				var member_name = (string)dataRecord["member_name"];
				var parent_role = (string)dataRecord["parent_role"];

				principals.Add(
					member_name + parent_role,
					(member_name,
					(string)dataRecord["member_type"],
					parent_role,
					dataRecord["default_schema_name"].ToStringOrNull()));
			});

			return principals;
		}

		public static HashSet<
			(string State,
			string Permission,
			string SecurableType,
			string SecurableSchema,
			string Securable,
			string SecurableColumn,
			string Grantee,
			string Grantor)>
			DatabasePermissionsList(AdminConnection adminConnection, string permissionsSelect)
		{
			var permissions =
				new HashSet<
					(string State,
					string Permission,
					string SecurableType,
					string SecurableSchema,
					string Securable,
					string SecurableColumn,
					string Grantee,
					string Grantor)>();

			adminConnection.ExecuteReader(permissionsSelect, dataRecord =>
			{
				permissions.Add(((string)dataRecord["state"],
					 (string)dataRecord["permission"],
					 (string)dataRecord["securableType"],
					 (string)dataRecord["securableSchema"],
					 (string)dataRecord["securable"],
					 (string)dataRecord["securableColumn"],
					 (string)dataRecord["grantee"],
					 (string)dataRecord["grantor"]));
			});

			return permissions;
		}

		public static List<string> GetFnAndFsAndAFObjectsNames(AdminConnection adminConnection)
		{
			var names = new List<string>();
			adminConnection.ExecuteReader($@"
SELECT name = obj.name
FROM sys.objects AS obj
WHERE type IN ('FN', 'FS', 'AF') AND obj.is_ms_shipped = 0
",
	record =>
	{
		names.Add((string)record["name"]);
	});
			return names;
		}

		public static List<string> GetCustomTypeNames(AdminConnection adminConnection)
		{
			var names = new List<string>();
			adminConnection.ExecuteReader($@"
SELECT name = tp.name
FROM sys.types AS tp
WHERE tp.system_type_id = 243
",
	record =>
	{
		names.Add((string)record["name"]);
	});
			return names;
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

		public static DisposableAction CreateRefDbSynonyms(AdminConnection adminConnection, string mainDbName)
		{
			var sql = $@"
SELECT
	name
FROM
	sys.databases
WHERE 1=2
	OR name LIKE '{RefDbTableNameResolver.SharedRefDbPrefix}%'
	OR name LIKE '{DataUtils.ReplaceSqlLikeWildcard(mainDbName)}[_]{RefDbTableNameResolver.RefDbAffix}[_]%'
";
			var refDbs = new List<string>();
			adminConnection.ExecuteReader(sql, record =>
			{
				refDbs.Add((string)record["name"]);
			});

			foreach (var dbName in refDbs)
			{
				using (((ICurrentDbControl)adminConnection).UseDatabase(dbName))
				{
					adminConnection.ExecuteNonQuery(@"
DROP TABLE IF EXISTS Test
CREATE TABLE Test (val int)");
				}

				CreateSynonym(adminConnection, $"RefDb{dbName}", dbName, "Test", mainDbName);
			}

			return new DisposableAction(() =>
			{
				foreach (var dbName in refDbs)
				{
					var synonymName = $"RefDb{dbName}";
					using (((ICurrentDbControl)adminConnection).UseDatabase(dbName))
					{
						if (adminConnection.Exists($"FROM sys.synonyms WHERE name='{synonymName}'"))
						{
							adminConnection.ExecuteNonQuery($"DROP SYNONYM IF EXISTS [{synonymName}];");
						}
					}
				}
			});
		}
	}
}
