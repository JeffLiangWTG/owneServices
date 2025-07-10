using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Security;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Security
{
	[TestedType(typeof(PopulateSqlStaffLoginPasswordHash))]
	public class PopulateSqlStaffLoginPasswordHashTest : DataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			Assert(Db.Connection.ExecuteScalar<bool>(
				$@"
SELECT CAST(IIF(login.password_hash = staff.GS_SqlLoginPasswordHash, 1, 0) AS bit)
FROM sys.sql_logins AS login
	JOIN GlbStaff AS staff ON staff.GS_LoginName = @loginName
WHERE login.name = @sqlLoginName
",
				cmd =>
				{
					cmd.AddParameter("@loginName", SqlDbType.NVarChar, 128, databaseDeveloper);
					cmd.AddParameter("@sqlLoginName", SqlDbType.NVarChar, 128, databaseDeveloperSqlLogin);
				}));

			Assert(Db.Connection.ExecuteScalar<bool>(
				$@"
SELECT CAST(IIF(login.password_hash = staff.GS_SqlLoginPasswordHash, 1, 0) AS bit)
FROM sys.sql_logins AS login
	JOIN GlbStaff AS staff ON staff.GS_LoginName = @loginName
WHERE login.name = @sqlLoginName
",
				cmd =>
				{
					cmd.AddParameter("@loginName", SqlDbType.NVarChar, 128, databaseReader);
					cmd.AddParameter("@sqlLoginName", SqlDbType.NVarChar, 128, databaseReaderSqlLogin);
				}));

			Assert(Db.Connection.ExecuteScalar<bool>(
				$@"
SELECT CAST(IIF(login.password_hash = staff.GS_SqlLoginPasswordHash, 1, 0) AS bit)
FROM sys.sql_logins AS login
	JOIN GlbStaff AS staff ON staff.GS_LoginName = @loginName
WHERE login.name = @sqlLoginName
",
				cmd =>
				{
					cmd.AddParameter("@loginName", SqlDbType.NVarChar, 128, databaseBackupOperator);
					cmd.AddParameter("@sqlLoginName", SqlDbType.NVarChar, 128, databaseBackupOperatorSqlLogin);
				}));

			Assert(Db.Connection.ExecuteScalar<bool>(
				$@"
SELECT CAST(IIF(login.password_hash = staff.GS_SqlLoginPasswordHash, 1, 0) AS bit)
FROM sys.sql_logins AS login
	JOIN GlbStaff AS staff ON staff.GS_LoginName = @loginName
WHERE login.name = @sqlLoginName
",
				cmd =>
				{
					cmd.AddParameter("@loginName", SqlDbType.NVarChar, 128, databaseHRMUser);
					cmd.AddParameter("@sqlLoginName", SqlDbType.NVarChar, 128, databaseHRMUserSqlLogin);
				}));
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new PopulateSqlStaffLoginPasswordHash();
		}

		protected override void PrepareTestData()
		{
			CreateGlbStaffWithDatabaseAccessRolesInDatabase(Db.Connection, databaseDeveloper, DbRoleTypes.DbDataWriterRole, DbRoleTypes.CwRestrictedReaderRole);
			CreateGlbStaffWithDatabaseAccessRolesInDatabase(Db.Connection, databaseReader, DbRoleTypes.CwRestrictedReaderRole);
			CreateGlbStaffWithDatabaseAccessRolesInDatabase(Db.Connection, databaseBackupOperator, DbRoleTypes.DbBackupOperatorRole);
			CreateGlbStaffWithDatabaseAccessRolesInDatabase(Db.Connection, databaseHRMUser, DbRoleTypes.CwHRMStaffRole);

				Db.Connection.ExecuteNonQuery($@"
CREATE LOGIN [{databaseDeveloperSqlLogin}] WITH PASSWORD = N'pWD123[]RD', CHECK_POLICY = OFF;
CREATE LOGIN [{databaseReaderSqlLogin}] WITH PASSWORD = N'pWD123[]RD', CHECK_POLICY = OFF;
CREATE LOGIN [{databaseBackupOperatorSqlLogin}] WITH PASSWORD = N'pWD123[]RD', CHECK_POLICY = OFF;
CREATE LOGIN [{databaseHRMUserSqlLogin}] WITH PASSWORD = N'pWD123[]RD', CHECK_POLICY = OFF;
");
		}

		const string databaseDeveloper = "databaseDeveloper";
		const string databaseReader = "databaseReader";
		const string databaseBackupOperator = "databaseBackupOperator";
		const string databaseHRMUser = "databaseHRMUser";

		string databaseDeveloperSqlLogin => $"{DbUserRepository.GetStaffDbLoginFullPrefix(Db.DatabaseName)}{databaseDeveloper}";
		string databaseReaderSqlLogin => $"{DbUserRepository.GetStaffDbLoginFullPrefix(Db.DatabaseName)}{databaseReader}";
		string databaseBackupOperatorSqlLogin => $"{DbUserRepository.GetStaffDbLoginFullPrefix(Db.DatabaseName)}{databaseBackupOperator}";
		string databaseHRMUserSqlLogin => $"{DbUserRepository.GetStaffDbLoginFullPrefix(Db.DatabaseName)}{databaseHRMUser}";

		public static void CreateGlbStaffWithDatabaseAccessRolesInDatabase(DbConnection connection, string staffLogin, params string[] dbRoles)
		{
			var groupPK = Guid.NewGuid();
			var groupsNumber = connection.ExecuteScalar<int>("SELECT COUNT(GG_PK) FROM dbo.GlbGroup WHERE GG_Code LIKE 'TG[_]%'");
			using (var command = connection.Command(
				$@"
DECLARE @staffPK AS uniqueidentifier = NEWID();
DECLARE @psersonPK AS uniqueidentifier = NEWID();

INSERT INTO dbo.GlbPerson(PER_PK, PER_FullName, PER_SystemCreateTimeUtc, PER_SystemCreateUser, PER_SystemLastEditTimeUtc, PER_SystemLastEditUser)
VALUES(@psersonPK, @staffLogin,  GetUtcDate(), 'X', GetUtcDate(), 'X');

INSERT dbo.GlbStaff
(GS_PK, GS_Code, GS_LoginName, GS_DomainName, GS_SqlLoginPasswordHash, GS_ActiveDirectoryObjectGuid, GS_PER, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
VALUES
(@staffPK, 'TS{groupsNumber + 1}', @staffLogin, N'', Null, Null, @psersonPK, GetUtcDate(), 'X', GetUtcDate(), 'X')

------ Database Access Groups
INSERT dbo.GlbGroup
(GG_PK, GG_Code, GG_Desc, GG_IsActive, GG_SystemCreateTimeUtc, GG_SystemCreateUser, GG_SystemLastEditTimeUtc, GG_SystemLastEditUser)
VALUES
(@groupPk, 'TG_{groupsNumber + 1:000}', 'Some random group name', 1, GetUtcDate(), 'X', GetUtcDate(), 'X')

INSERT dbo.GlbGroupLink
(GK_PK, GK_GG, GK_GS, GK_SystemCreateTimeUtc, GK_SystemCreateUser, GK_SystemLastEditTimeUtc, GK_SystemLastEditUser)
VALUES
(NEWID(), @groupPK, @staffPK, GetUtcDate(), 'X', GetUtcDate(), 'X')
"))
			{
				command.AddParameter("@staffLogin", SqlDbType.NVarChar, 128, staffLogin);
				command.AddParameter("@groupPK", SqlDbType.UniqueIdentifier, groupPK);
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

		AdminConnection adminConnection;

		protected override void SetUp()
		{
			Db.ConnectionOverrideForTest = (AdminConnection)TestConnection;

			base.SetUp();
		}

		protected override void TearDown()
		{
			base.TearDown();
			adminConnection?.Dispose();
			Db.ConnectionOverrideForTest = null;
		}

		protected override DbConnection TestConnection
		{
			get
			{
				if (adminConnection == null)
				{
					adminConnection = Db.NewAdminConnection();
				}

				return adminConnection;
			}
		}
	}
}
