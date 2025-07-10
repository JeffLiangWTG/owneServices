using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using CargoWise.ActiveDirectory.TestFramework;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.DataProtection;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Licensing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ServiceManager.Tasks.DbSecurityAdmin;
using Enterprise.SqlSecurity;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using WTG.StaticAnalysis.Annotation;

[assembly: UsesConstants(typeof(TestConstants))]

namespace Enterprise.ServiceManager.Tasks.LoginSyncServiceTask.Testing
{
	sealed class DbSecurityAdminTaskSecurityTest : TestCase
	{
		[ExpectNoExceptions]
		[UseSnapshotProtection]
		public void TestLogsMessagesWhenUsingModernSecuritySystem()
		{
			// Arrange
			using (var adminConnection = Db.NewAdminConnection())
			{
				EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = true;

				var loggerMock = new Mock<ILogger>();

				// Act
				var adminTask = new DbSecurityAdminTask();
				adminTask.ServiceLogger = loggerMock.Object;

				adminTask.RunTask(CancellationToken.None);

				// Assert
				CombineAssertions(() =>
				{
					loggerMock.Verify(logger => logger.Log(LogType.Information, $"Building Sql security for server '{adminConnection.ServerName}' with main database name '{adminConnection.CurrentDatabase}'."), Times.Once());
					foreach (var databaseName in adminConnection.GetDatabases(DatabaseType.All))
					{
						loggerMock.Verify(logger => logger.Log(LogType.Information, $"Building Sql security for database '{databaseName}'."), Times.Once());
					}
				});
			}
		}

		[SnailTest]
		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.BI })]

		public void TestDropsUnexpectedPermissionsInExclusiveDbsWhenHostedInCWNotOpenUsingModernSqlSecurity()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock("SYD", DatabaseSecurityModePairList.Codes.Indeterminate)))
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = true;
				var exclusiveDatabases = adminConnection.GetDatabases(DatabaseType.AllExclusive);
				DropsUnexpectedGrantPermissionsForApplicationUsers(exclusiveDatabases, adminConnection);
				DropsUnexpectedDenyPermissionsForApplicationUsers(exclusiveDatabases, adminConnection);

				DropsUnexpectedGrantPermissionsForStaffUsers(exclusiveDatabases, adminConnection);
				DropsUnexpectedDenyPermissionsForStaffUsers(exclusiveDatabases, adminConnection);

				DropsUnexpectedPermissionsForApplicationRoles(exclusiveDatabases, adminConnection);
			}
		}

		[SnailTest]
		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.BI })]
		public void TestDropsUnexpectedPermissionsInExclusiveDbsWhenSelfHostedOpenUsingModernSqlSecurity()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(Constants.NotHostedWithCargoWise, DatabaseSecurityModePairList.Codes.OpenMode)))
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = true;
				var exclusiveDatabases = adminConnection.GetDatabases(DatabaseType.AllExclusive);
				DropsUnexpectedGrantPermissionsForApplicationUsers(exclusiveDatabases, adminConnection);
				DropsUnexpectedDenyPermissionsForApplicationUsers(exclusiveDatabases, adminConnection);

				DropsUnexpectedGrantPermissionsForStaffUsers(exclusiveDatabases, adminConnection);
				DropsUnexpectedDenyPermissionsForStaffUsers(exclusiveDatabases, adminConnection);

				DropsUnexpectedPermissionsForApplicationRoles(exclusiveDatabases, adminConnection);
			}
		}

		[UseSnapshotProtection]
		[SnailTest]

		public void TestDropsUnexpectedPermissionsForUsersButNotRolesInSharedRefDbsWhenHosteInCWdNotOpenUsingModernSqlSecurity()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock("SYD", DatabaseSecurityModePairList.Codes.Indeterminate)))
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = true;
				var sharedDatabases = adminConnection.GetDatabases(DatabaseType.All & ~DatabaseType.AllExclusive & ~DatabaseType.SingleSharedRef);

				DropsUnexpectedGrantPermissionsForApplicationUsers(sharedDatabases, adminConnection);
				DropsUnexpectedDenyPermissionsForApplicationUsers(sharedDatabases, adminConnection);

				DropsUnexpectedGrantPermissionsForStaffUsers(sharedDatabases, adminConnection);
				DropsUnexpectedDenyPermissionsForStaffUsers(sharedDatabases, adminConnection);

				DoesNotDropUnexpectedPermissionsForDbRoles(sharedDatabases, adminConnection);
			}
		}

		[UseSnapshotProtection]
		[SnailTest]
		public void TestDropsUnexpectedPermissionsForUsersButNotRolesInSharedRefDbsWhenSelfHostedOpenUsingModernSqlSecurity()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(Constants.NotHostedWithCargoWise, DatabaseSecurityModePairList.Codes.OpenMode)))
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = true;
				var sharedDatabases = adminConnection.GetDatabases(DatabaseType.All & ~DatabaseType.AllExclusive & ~DatabaseType.SingleSharedRef);
				DropsUnexpectedGrantPermissionsForApplicationUsers(sharedDatabases, adminConnection);
				DropsUnexpectedDenyPermissionsForApplicationUsers(sharedDatabases, adminConnection);

				DropsUnexpectedGrantPermissionsForStaffUsers(sharedDatabases, adminConnection);
				DropsUnexpectedDenyPermissionsForStaffUsers(sharedDatabases, adminConnection);

				DoesNotDropUnexpectedPermissionsForDbRoles(sharedDatabases, adminConnection);
			}
		}

		[UseSnapshotProtection]

		public void TestDropsUnexpectedPermissionsForLoginsWhenHostedInCWNotOpenUsingModernSqlSecurity()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock("SYD", DatabaseSecurityModePairList.Codes.Indeterminate)))
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = true;
				DropsUnexpectedGrantPermissionsForLogins(adminConnection);
				DropsUnexpectedDenyPermissionsForLogins(adminConnection);
			}
		}

		[UseSnapshotProtection]
		public void TestDropsUnexpectedPermissionsForLoginsWhenSelfHostedOpenUsingModernSqlSecurity()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(Constants.NotHostedWithCargoWise, DatabaseSecurityModePairList.Codes.OpenMode)))
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = true;
				DropsUnexpectedGrantPermissionsForLogins(adminConnection);
				DropsUnexpectedDenyPermissionsForLogins(adminConnection);
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestDropsUnexpectedGrantButNotDenyPermissionsForUsersButNotDbRolesWhenHostedInCWNotOpenUsingLegacySecurityCleanUp()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock("SYD", DatabaseSecurityModePairList.Codes.Indeterminate)))
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = false;
				var databases = adminConnection.GetDatabases(DatabaseType.All & ~DatabaseType.EDW & ~DatabaseType.SingleSharedRef);
				DropsUnexpectedGrantPermissionsForApplicationUsers(databases, adminConnection);
				DropsUnexpectedGrantPermissionsForStaffUsers(databases, adminConnection);

				DoesNotDropUnexpectedDenyPermissionsForApplicationUsers(databases, adminConnection);
				DoesNotDropUnexpectedDenyPermissionsForStaffUsers(databases, adminConnection);

				DoesNotDropUnexpectedPermissionsForDbRoles(databases, adminConnection);
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestDoesNotDropUnexpectedPermissionsWhenSelfHostedHostedOpenUsingLegacySecurityCleanUp()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(Constants.NotHostedWithCargoWise, DatabaseSecurityModePairList.Codes.OpenMode)))
			{
				EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = false;
				var databases = adminConnection.GetDatabases(DatabaseType.All & ~DatabaseType.SingleSharedRef & ~DatabaseType.EDW);
				DoesNotDropUnexpectedGrantPermissionsForApplicationUsers(databases, adminConnection);
				DoesNotDropUnexpectedGrantPermissionsForStaffUsers(databases, adminConnection);

				DoesNotDropUnexpectedDenyPermissionsForApplicationUsers(databases, adminConnection);
				DoesNotDropUnexpectedDenyPermissionsForStaffUsers(databases, adminConnection);

				DoesNotDropUnexpectedPermissionsForDbRoles(databases, adminConnection);
			}
		}

		[UseSnapshotProtection]
		public void TestDoesNotDropUnexpectedPermissionsForLoginsWhenHostedInCWNotOpenUsingLegacySecurityCleanUp()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock("SYD", DatabaseSecurityModePairList.Codes.Indeterminate)))
			{
				EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = false;
				DoesNotDropUnexpectedGrantPermissionsForLogins(adminConnection);
				DoesNotDropUnexpectedDenyPermissionsForLogins(adminConnection);
			}
		}

		[UseSnapshotProtection]
		public void TestDoesNotDropUnexpectedPermissionsForLoginsWhenSelfHostedOpenUsingLegacySecurityCleanUp()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(Constants.NotHostedWithCargoWise, DatabaseSecurityModePairList.Codes.OpenMode)))
			{
				EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = false;
				DoesNotDropUnexpectedGrantPermissionsForLogins(adminConnection);
				DoesNotDropUnexpectedDenyPermissionsForLogins(adminConnection);
			}
		}

		[UseSnapshotProtection]
		public void TestRestoresDroppedRequiredServerAndDatabasePrincipalsPermissionsWhenHostedInCWNotOpenUsingModernSecuritySystem()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock("SYD", DatabaseSecurityModePairList.Codes.Indeterminate)))
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = true;
				RestoresDroppedRequiredPermissions(adminConnection);
			}
		}

		[UseSnapshotProtection]
		public void TestRestoresDroppedRequiredServerAndDatabasePrincipalsPermissionsWhenSelfHostedOpenUsingModernSecuritySystem()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(Constants.NotHostedWithCargoWise, DatabaseSecurityModePairList.Codes.OpenMode)))
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = true;
				RestoresDroppedRequiredPermissions(adminConnection);
			}
		}

		[UseSnapshotProtection]
		public void TestDoesNotRestoreDroppedRequiredServerAndDatabasePrincipalsPermissionsWhenHostedInCWNotOpenUsingLegacySecurityCleanUp()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock("SYD", DatabaseSecurityModePairList.Codes.Indeterminate)))
			{
				EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = false;
				DoesNotRestoreDroppedRequiredPermissions(adminConnection);
			}
		}

		[UseSnapshotProtection]
		public void TestDoesNotRestoreDroppedRequiredServerAndDatabasePrincipalsPermissionsWhenSelfHostedOpenUsingLegacySecurityCleanUp()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(Constants.NotHostedWithCargoWise, DatabaseSecurityModePairList.Codes.OpenMode)))
			{
				EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = false;
				DoesNotRestoreDroppedRequiredPermissions(adminConnection);
			}
		}

		[UseSnapshotProtection]
		[SnailTest]
		public void TestDropsUnexpectedRoleMembershipsInExclusiveDatabasesWhenSelfHostedOpenUsingModernSecuritySystem()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(Constants.NotHostedWithCargoWise, DatabaseSecurityModePairList.Codes.OpenMode)))
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = true;
				var databases = adminConnection.GetDatabases(DatabaseType.AllExclusive);
				DropsUnexpectedMembershipOfRolesUsedByCWForApplicationUsers(databases, adminConnection);
				DropsUnexpectedMembershipOfRolesNotUsedByCWForApplicationUsers(databases, adminConnection);

				DropsUnexpectedMembershipsOfRolesUsedByCWForStaffUsers(databases, adminConnection);
				DropsUnexpectedMembershipsOfRolesNotUsedByCWForStaffUsers(databases, adminConnection);

				DropsUnexpectedMembershipsOfRolesUsedByCWForApplicationRoles(databases, adminConnection);
				DropsUnexpectedMembershipsOfRolesNotUsedByCWForApplicationRoles(databases, adminConnection);
			}
		}

		[UseSnapshotProtection]
		public void TestDropsUnexpectedRoleMembershipsForStaffAndApplicationUsersInSharedDatabasesWhenSelfhostedOpenUsingModernSecuritySystem()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(Constants.NotHostedWithCargoWise, DatabaseSecurityModePairList.Codes.OpenMode)))
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = true;
				var databases = adminConnection.GetDatabases(DatabaseType.All & ~DatabaseType.AllExclusive & ~DatabaseType.SingleSharedRef);
				DropsUnexpectedMembershipOfRolesUsedByCWForApplicationUsers(databases, adminConnection);
				DropsUnexpectedMembershipOfRolesNotUsedByCWForApplicationUsers(databases, adminConnection);

				DropsUnexpectedMembershipsOfRolesUsedByCWForStaffUsers(databases, adminConnection);
				DropsUnexpectedMembershipsOfRolesNotUsedByCWForStaffUsers(databases, adminConnection);
			}
		}

		[UseSnapshotProtection]
		public void TestDropsUnexpectedRoleMembershipForLoginsWhenSelfHostedOpenUsingModernSecuritySystem()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(Constants.NotHostedWithCargoWise, DatabaseSecurityModePairList.Codes.OpenMode)))
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = true;
				DropsUnexpectedMembershipsForStaffAndApplicationLogins(adminConnection);
			}
		}

		[UseSnapshotProtection]
		[SnailTest]
		public void TestDropsUnexpectedRoleMembershipsInExclusiveDatabasesWhenHostedInCWUsingModernSecuritySystem()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock("SYD", DatabaseSecurityModePairList.Codes.Indeterminate)))
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = true;
				var databases = adminConnection.GetDatabases(DatabaseType.AllExclusive);
				DropsUnexpectedMembershipOfRolesUsedByCWForApplicationUsers(databases, adminConnection);
				DropsUnexpectedMembershipOfRolesNotUsedByCWForApplicationUsers(databases, adminConnection);

				DropsUnexpectedMembershipsOfRolesUsedByCWForStaffUsers(databases, adminConnection);
				DropsUnexpectedMembershipsOfRolesNotUsedByCWForStaffUsers(databases, adminConnection);

				DropsUnexpectedMembershipsOfRolesUsedByCWForApplicationRoles(databases, adminConnection);
				DropsUnexpectedMembershipsOfRolesNotUsedByCWForApplicationRoles(databases, adminConnection);
			}
		}

		[UseSnapshotProtection]
		[SnailTest]
		public void TestDropsUnexpectedRoleMembershipsForStaffAndApplicationUsersInSharedDatabaseWhenHostedInCWUsingModernSecuritySystem()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock("SYD", DatabaseSecurityModePairList.Codes.Indeterminate)))
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = true;
				var databases = adminConnection.GetDatabases(DatabaseType.All & ~DatabaseType.AllExclusive & ~DatabaseType.SingleSharedRef);
				DropsUnexpectedMembershipOfRolesUsedByCWForApplicationUsers(databases, adminConnection);
				DropsUnexpectedMembershipOfRolesNotUsedByCWForApplicationUsers(databases, adminConnection);

				DropsUnexpectedMembershipsOfRolesUsedByCWForStaffUsers(databases, adminConnection);
				DropsUnexpectedMembershipsOfRolesNotUsedByCWForStaffUsers(databases, adminConnection);
			}
		}

		[UseSnapshotProtection]
		public void TestDoesNotDropUnexpectedRoleMembershipsForAndApplicationRolesInSharedDatabasesWhenHostedInCWUsingModernSecuritySystem()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock("SYD", DatabaseSecurityModePairList.Codes.Indeterminate)))
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = true;
				var databases = adminConnection.GetDatabases(DatabaseType.All & ~DatabaseType.AllExclusive & ~DatabaseType.SingleSharedRef);
				DoesNotDropUnexpectedMembershipsOfRolesUsedInCWForApplicationRoles(databases, adminConnection);
				DoesNotDropUnexpectedMembershipsOfRolesNotUsedInCWForApplicationRoles(databases, adminConnection);
			}
		}

		[UseSnapshotProtection]
		public void TestDropsUnexpectedRoleMembershipForLoginsWhenHostedInCWUsingModernSecuritySystem()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock("SYD", DatabaseSecurityModePairList.Codes.Indeterminate)))
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = true;
				DropsUnexpectedMembershipsForStaffAndApplicationLogins(adminConnection);
			}
		}

		[UseSnapshotProtection]
		[SnailTest]
		public void TestDropsUnexpectedDatabaseRoleMembershipsOfRolesNotUsedByCWWhenCWHostedNotOpenUsingLegacySecurityCleanUp()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock("SYD", DatabaseSecurityModePairList.Codes.Indeterminate)))
			{
				EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = false;
				var databases = adminConnection.GetDatabases(DatabaseType.All & ~DatabaseType.SingleSharedRef & ~DatabaseType.EDW);
				DropsUnexpectedMembershipOfRolesNotUsedByCWForApplicationUsers(databases, adminConnection);
				DropsUnexpectedMembershipsOfRolesNotUsedByCWForStaffUsers(databases, adminConnection);
				DropsUnexpectedMembershipsOfRolesNotUsedByCWForApplicationRoles(databases, adminConnection);
			}
		}

		[UseSnapshotProtection]
		public void TestDoesNotDropUnexpectedServerRoleMembershipsWhenCWHostedNotOpenUsingLegacySecurityCleanUp()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock("SYD", DatabaseSecurityModePairList.Codes.Indeterminate)))
			{
				EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = false;
				DoesNotDropUnexpectedMembershipsForStaffAndApplicationLogins(adminConnection);
			}
		}

		[UseSnapshotProtection]
		public void TestDoesNotDropUnexpectedDatabaseRoleMembershipsWhenSelfHostedOpenUsingLegacySecurityCleanUp()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock("SYD", DatabaseSecurityModePairList.Codes.OpenMode)))
			{
				EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = false;
				var databases = adminConnection.GetDatabases(DatabaseType.All & ~DatabaseType.SingleSharedRef & ~DatabaseType.EDW);

				DoesNotDropUnexpectedMembershipOfRolesUsedByCWForApplicationUsers(databases, adminConnection);
				DoesNotDropUnexpectedMembershipsOfRolesUsedInCWForStaffUsers(databases, adminConnection);
				DoesNotDropUnexpectedMembershipsOfRolesUsedInCWForApplicationRoles(databases, adminConnection);
			}
		}

		[UseSnapshotProtection]
		[SnailTest]
		public void TestRestoresDroppedRequiredRoleMembershipsWhenHostedInCWUsingModernSecuritySystem()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock("SYD", DatabaseSecurityModePairList.Codes.Indeterminate)))
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = true;
				RestoresDroppedRequiredApplicationRoleAndUserRoles(adminConnection);
				RestoresDroppedRequiredUserRepositoryStaffUserRoles(adminConnection);
				RestoresDroppedRequiredStaffUserRolesHostedInCW(adminConnection);
			}
		}

		[UseSnapshotProtection]
		[SnailTest]
		public void TestRestoresDroppedRequiredRoleMembershipsWhenSelfHostedOpenUsingModernSecuritySystem()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(Constants.NotHostedWithCargoWise, DatabaseSecurityModePairList.Codes.OpenMode)))
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = true;
				RestoresDroppedRequiredApplicationRoleAndUserRoles(adminConnection);
				RestoresDroppedRequiredUserRepositoryStaffUserRoles(adminConnection);
				RestoresDroppedRequiredStaffUserRolesInSelfHosted(adminConnection);
			}
		}

		[UseSnapshotProtection]
		public void TestDoesNotRestoreDroppedRequiredRoleMembershipsWhenHostedInCWNotOpenUsingLegacySecurityCleanUp()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock("SYD", DatabaseSecurityModePairList.Codes.Indeterminate)))
			{
				EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = false;
				DoesNotRestoreDroppedRequiredRoleMemberships(adminConnection);
			}
		}

		[UseSnapshotProtection]
		public void TestDoesNotRestoreDroppedRequiredRoleMembershipsWhenSelfHostedOpenUsingLegacySecurityCleanUp()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(Constants.NotHostedWithCargoWise, DatabaseSecurityModePairList.Codes.OpenMode)))
			{
				EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = false;
				DoesNotRestoreDroppedRequiredRoleMemberships(adminConnection);
			}
		}

		[UseSnapshotProtection]
		public void TestDropsOnlyUnknownServerPrincipalsSatisfyingPatternWhenSelfHostedOpenUsingModernSecuritySystem()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(Constants.NotHostedWithCargoWise, DatabaseSecurityModePairList.Codes.OpenMode)))
			{
				EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = true;
				DropsUnknownServerPrincipalsSatisfyingPattern(adminConnection);
				DoesNotDropUnknownServerPrincipalsNotSatisfyingPattern(adminConnection);
			}
		}

		[UseSnapshotProtection]
		public void TestDropsOnlyUnknownServerPrincipalsSatisfyingPatternWhenHostedInCWNotOpenUsingModernSecuritySystem()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock("SYD", DatabaseSecurityModePairList.Codes.Indeterminate)))
			{
				EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = true;
				DropsUnknownServerPrincipalsSatisfyingPattern(adminConnection);
				DoesNotDropUnknownServerPrincipalsNotSatisfyingPattern(adminConnection);
			}
		}

		[UseSnapshotProtection]
		public void TestDoesNotDropUnknownServerPrincipalsWhenSelfHostedOpenUsingLegacySecurityCleanUp()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(Constants.NotHostedWithCargoWise, DatabaseSecurityModePairList.Codes.OpenMode)))
			{
				EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = false;
				DoesNotDropUnknownServerPrincipalsSatisfyingPattern(adminConnection);
				DoesNotDropUnknownServerPrincipalsNotSatisfyingPattern(adminConnection);
			}
		}

		[UseSnapshotProtection]
		public void TestDoesNotDropUnknownServerPrincipalsSatisfyingPatternWhenHostedInCWNotOpenUsingLegacySecurityCleanUp()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock("SYD", DatabaseSecurityModePairList.Codes.Indeterminate)))
			{
				EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = false;
				DoesNotDropUnknownServerPrincipalsSatisfyingPattern(adminConnection);
				DoesNotDropUnknownServerPrincipalsNotSatisfyingPattern(adminConnection);
			}
		}

		[UseSnapshotProtection]
		public void TestDropsOnlyUnknownDatabasePrincipalsSatisfyingPatternInExclusiveDatabasesWhenSelfHostedOpenUsingModernSecuritySystem()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(Constants.NotHostedWithCargoWise, DatabaseSecurityModePairList.Codes.OpenMode)))
			{
				EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = true;
				var databases = adminConnection.GetDatabases(DatabaseType.AllExclusive);
				DropsUnknownDatabaseUsersSatisfyingStaffUserPattern(databases, adminConnection);
				DropsUnknownDatabaseUsersSatisfyingApplicationUserPattern(databases, adminConnection);
				DropsUnknownDatabaseRolesSatisfyingPattern(databases, adminConnection);
				DoesNotDropUnknownDatabasePrincipalsNotSatisfyingPattern(databases, adminConnection);
			}
		}

		[UseSnapshotProtection]
		public void TestDropsUnknownDatabasePrincipalsInExclusiveDatabasesWhenHostedInCWNotOpenUsingModernSecuritySystem()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock("SYD", DatabaseSecurityModePairList.Codes.Indeterminate)))
			{
				EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = true;
				var databases = adminConnection.GetDatabases(DatabaseType.AllExclusive);
				DropsUnknownDatabaseUsersSatisfyingStaffUserPattern(databases, adminConnection);
				DropsUnknownDatabaseUsersSatisfyingApplicationUserPattern(databases, adminConnection);
				DropsUnknownDatabaseRolesSatisfyingPattern(databases, adminConnection);
				DropsUnknownDatabasePrincipalsNotSatisfyingPattern(databases, adminConnection);
			}
		}

		[UseSnapshotProtection]
		public void TestDropsOnlyUnknownDatabaseUsersSatisfyingPatternInSharedDatabasesWhenSelfHostedOpenUsingModernSecuritySystem()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(Constants.NotHostedWithCargoWise, DatabaseSecurityModePairList.Codes.OpenMode)))
			{
				EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = true;
				var databases = adminConnection.GetDatabases(DatabaseType.All & ~DatabaseType.AllExclusive & ~DatabaseType.SingleSharedRef);
				DropsUnknownDatabaseUsersSatisfyingStaffUserPattern(databases, adminConnection);
				DropsUnknownDatabaseUsersSatisfyingApplicationUserPattern(databases, adminConnection);
				DoesNotDropUnknownDatabaseRolesSatisfyingPattern(databases, adminConnection);
				DoesNotDropUnknownDatabasePrincipalsNotSatisfyingPattern(databases, adminConnection);
			}
		}

		[UseSnapshotProtection]
		public void TestDropsOnlyUnknownDatabaseUsersSatisfyingPatternInSharedDatabasesWhenHostedInCWNotOpenUsingModernSecuritySystem()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock("SYD", DatabaseSecurityModePairList.Codes.Indeterminate)))
			{
				EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = true;
				var databases = adminConnection.GetDatabases(DatabaseType.All & ~DatabaseType.AllExclusive & ~DatabaseType.SingleSharedRef);
				DropsUnknownDatabaseUsersSatisfyingStaffUserPattern(databases, adminConnection);
				DropsUnknownDatabaseUsersSatisfyingApplicationUserPattern(databases, adminConnection);
				DoesNotDropUnknownDatabaseRolesSatisfyingPattern(databases, adminConnection);
				DoesNotDropUnknownDatabasePrincipalsNotSatisfyingPattern(databases, adminConnection);
			}
		}

		[UseSnapshotProtection]
		public void TestDropsUnknownDatabaseUsersInExclusiveDatabasesWhenHostedInCWNotOpenUsingLegacySecurityCleanup()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock("SYD", DatabaseSecurityModePairList.Codes.Indeterminate)))
			{
				EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = false;
				var databases = adminConnection.GetDatabases(DatabaseType.AllExclusive & ~DatabaseType.EDW);
				DropsUnknownDatabaseUsersSatisfyingStaffUserPattern(databases, adminConnection);
				DropsUnknownDatabaseUsersSatisfyingApplicationUserPattern(databases, adminConnection);
				DropsUnknownDatabaseRolesSatisfyingPattern(databases, adminConnection);
				DropsUnknownDatabasePrincipalsNotSatisfyingPattern(databases, adminConnection);
			}
		}

		[UseSnapshotProtection]
		public void TestDropsUnknownDatabaseUsersAndRolesSatisfyingApplicationUserAndRolePatternInSharedDatabasesWhenHostedInCWNotOpenUsingLegacySecurityCleanup()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock("SYD", DatabaseSecurityModePairList.Codes.Indeterminate)))
			{
				EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = false;
				var databases = adminConnection.GetDatabases(DatabaseType.All & ~DatabaseType.AllExclusive & ~DatabaseType.SingleSharedRef);
				DropsUnknownDatabaseUsersSatisfyingApplicationUserPattern(databases, adminConnection);
				DropsUnknownDatabaseRolesSatisfyingPattern(databases, adminConnection);
			}
		}

		[UseSnapshotProtection]
		public void TestDropsUnknownDatabaseUsersAndRolesNotSatisfyingAnyPatternInSharedDatabasesWhenHostedInCWNotOpenUsingLegacySecurityCleanup()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock("SYD", DatabaseSecurityModePairList.Codes.Indeterminate)))
			{
				EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = false;
				var databases = adminConnection.GetDatabases(DatabaseType.All & ~DatabaseType.AllExclusive & ~DatabaseType.SingleSharedRef);
				DropsUnknownDatabasePrincipalsNotSatisfyingPattern(databases, adminConnection);
			}
		}

		[UseSnapshotProtection]
		public void TestDoesNotDropUnknownDatabaseUsersSatisfyingStaffUserPatternInSharedDatabasesWhenHostedInCWNotOpenUsingLegacySecurityCleanup()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock("SYD", DatabaseSecurityModePairList.Codes.Indeterminate)))
			{
				EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = false;
				var databases = adminConnection.GetDatabases(DatabaseType.All & ~DatabaseType.AllExclusive & ~DatabaseType.SingleSharedRef);
				DoesNotDropUnknownDatabaseUsersSatisfyingStaffUserPattern(databases, adminConnection);
			}
		}

		[UseSnapshotProtection]
		public void TestRestoresDroppedRequiredPrincipalsWhenSelfHostedOpenUsingModernSecuritySystem()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(Constants.NotHostedWithCargoWise, DatabaseSecurityModePairList.Codes.OpenMode)))
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = true;
				var databases = adminConnection.GetDatabases(DatabaseType.All & ~DatabaseType.SingleSharedRef);
				RestoresDropedRequiredPrincipals(databases, adminConnection);
			}
		}

		[UseSnapshotProtection]
		public void TestRestoresDroppedRequiredPrincipalsWhenHostedInCWNotOpenUsingModernSecuritySystem()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock("SYD", DatabaseSecurityModePairList.Codes.Indeterminate)))
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = true;
				var databases = adminConnection.GetDatabases(DatabaseType.All & ~DatabaseType.SingleSharedRef);
				RestoresDropedRequiredPrincipals(databases, adminConnection);
			}
		}

		[UseSnapshotProtection]
		public void TestDoesNotRestoreDroppedRequiredPrincipalsWhenSelfHostedOpenUsingLegacySecurityCleanUp()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(Constants.NotHostedWithCargoWise, DatabaseSecurityModePairList.Codes.OpenMode)))
			{
				EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = false;
				var databases = adminConnection.GetDatabases(DatabaseType.All & ~DatabaseType.SingleSharedRef);
				DoesNotRestoreDropedRequiredPrincipals(databases, adminConnection);
			}
		}

		[UseSnapshotProtection]
		public void TestDoesNotRestoreDroppedRequiredPrincipalsWhenHostedInCWNotOpenUsingLegacySecurityCleanUp()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock("SYD", DatabaseSecurityModePairList.Codes.Indeterminate)))
			{
				EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = false;
				var databases = adminConnection.GetDatabases(DatabaseType.All & ~DatabaseType.SingleSharedRef);
				DoesNotRestoreDropedRequiredPrincipals(databases, adminConnection);
			}
		}

		#region Triggers tests

		[UseSnapshotProtection]
		[DeveloperOnlyTest]
		public void TestDoesNotResetSaPasswordWhenSelfHostedOpenUsingModernSecuritySystem()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(Constants.NotHostedWithCargoWise, DatabaseSecurityModePairList.Codes.OpenMode)))
			{
				TestSaPasswordReset(adminConnection, useModernSecuritySystem: true, expectPasswordReset: false);
			}
		}

		[UseSnapshotProtection]
		[DeveloperOnlyTest]
		public void TestDoesNotResetSaPasswordWhenHostedInCWNotOpenUsingModertSecuritySystem()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock("SYD", DatabaseSecurityModePairList.Codes.Indeterminate)))
			{
				TestSaPasswordReset(adminConnection, useModernSecuritySystem: true, expectPasswordReset: false);
			}
		}

		[UseSnapshotProtection]
		[DeveloperOnlyTest]
		public void TestDoesNotResetSaPasswordWhenSelfHostedOpenUsingLegacySecuriytCleanUp()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(Constants.NotHostedWithCargoWise, DatabaseSecurityModePairList.Codes.OpenMode)))
			{
				TestSaPasswordReset(adminConnection, useModernSecuritySystem: false, expectPasswordReset: false);
			}
		}

		[UseSnapshotProtection]
		[DeveloperOnlyTest]
		public void TestDoesNotResetSaPasswordWhenHostedInCWNotOpenUsingLegacySecuriytCleanUp()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock("SYD", DatabaseSecurityModePairList.Codes.Indeterminate)))
			{
				TestSaPasswordReset(adminConnection, useModernSecuritySystem: false, expectPasswordReset: false);
			}
		}

		[UseSnapshotProtection]
		public void TestDisablesDdlTriggersWhenSelfHostedOpenUsingModernSecuritySystem()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(Constants.NotHostedWithCargoWise, DatabaseSecurityModePairList.Codes.OpenMode)))
			{
				TestDdlTriggersDisabling(adminConnection, useModernSecuritySystem: true, expectTriggerDisabled: false);
			}
		}

		[UseSnapshotProtection]
		public void TestDisablesDdlTriggersWhenHostedInCWNotOpenUsingModernSecuritySystem()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock("SYD", DatabaseSecurityModePairList.Codes.Indeterminate)))
			{
				TestDdlTriggersDisabling(adminConnection, useModernSecuritySystem: true, expectTriggerDisabled: false);
			}
		}

		[UseSnapshotProtection]
		public void TestDisablesDdlTriggersWhenSelfHostedOpenUsingLegacySecuriytCleanUp()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(Constants.NotHostedWithCargoWise, DatabaseSecurityModePairList.Codes.OpenMode)))
			{
				TestDdlTriggersDisabling(adminConnection, useModernSecuritySystem: false, expectTriggerDisabled: false);
			}
		}

		[UseSnapshotProtection]
		public void TestDisablesDdlTriggersWhenHostedInCWNotOpenUsingLegacySecuriytCleanUp()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock("SYD", DatabaseSecurityModePairList.Codes.Indeterminate)))
			{
				TestDdlTriggersDisabling(adminConnection, useModernSecuritySystem: false, expectTriggerDisabled: false);
			}
		}

		[UseSnapshotProtection]
		public void TestDisablesSqlAgentJobsWhenSelfHostedOpenUsingModernSecuritySystem()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(Constants.NotHostedWithCargoWise, DatabaseSecurityModePairList.Codes.OpenMode)))
			{
				TestSqlAgentJobsDisabling(adminConnection, useModernSecuritySystem: true, expectAgentJobDisabled: false);
			}
		}

		[UseSnapshotProtection]
		public void TestDisablesSqlAgentJobsWhenHostedInCWNotOpenUsingModernSecuritySystem()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock("SYD", DatabaseSecurityModePairList.Codes.Indeterminate)))
			{
				TestSqlAgentJobsDisabling(adminConnection, useModernSecuritySystem: true, expectAgentJobDisabled: false);
			}
		}

		[UseSnapshotProtection]
		public void TestDisablesSqlAgentJobsWhenSelfHostedOpenUsingLegacySecuriytCleanUp()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(Constants.NotHostedWithCargoWise, DatabaseSecurityModePairList.Codes.OpenMode)))
			{
				TestSqlAgentJobsDisabling(adminConnection, useModernSecuritySystem: false, expectAgentJobDisabled: false);
			}
		}

		[UseSnapshotProtection]
		public void TestDisablesSqlAgentJobsWhenHostedInCWNotOpenUsingLegacySecuriytCleanUp()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock("SYD", DatabaseSecurityModePairList.Codes.Indeterminate)))
			{
				TestSqlAgentJobsDisabling(adminConnection, useModernSecuritySystem: false, expectAgentJobDisabled: false);
			}
		}

		#endregion Triggers tests

		#region Tests from MasterFiles.Business.Test for ModernSecurity

		[UseSnapshotProtection]
		public void TestCheckPolicyIsTurnedOff()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = true;
				var sqlSecurityManager = new SqlSecurityManager(Mock.Of<ILogger>(), Db.DatabaseName, allowTransaction: false);

				var factory = new BusinessObjectFactory(adminConnection);
				var staffName = "StaffLogin01";
				var staff = factory.New<GlbStaff>();

				staff.GS_FullName = staffName;
				staff.GS_LoginName = staffName;
				staff.GS_Code = "123";
				staff.StaffPlainTextPassword = "TOPSECRET";

				staff.IsDatabaseDeveloper = true;
				staff.IsReadOnlyDBUser = true;

				SetStaffPassword(staff);

				var fullDbStaffLogin = $"{DbUserRepository.GetStaffDbLoginFullPrefix(Db.DatabaseName)}{staff.GS_LoginName}";

				try
				{
					factory.Save();

					sqlSecurityManager.BuildServerSecurity(adminConnection, trialRun: false);

					AssertServerPrincipalExistsAndIsNotDisabled(adminConnection, fullDbStaffLogin, "S");

					adminConnection.ExecuteNonQuery($"ALTER LOGIN {fullDbStaffLogin.QuoteName()} WITH CHECK_POLICY = ON");

					AssertEquals(
						"Check policy should be turned on",
						adminConnection.ExecuteScalar<bool>($"SELECT is_policy_checked FROM sys.sql_logins WHERE name = '{fullDbStaffLogin}'"),
						true);

					var adminTask = new DbSecurityAdminTask();
					adminTask.ServiceLogger = Mock.Of<ILogger>();

					// Act

					adminTask.RunTask(CancellationToken.None);

					AssertEquals(
						"Check policy should be turned off",
						adminConnection.ExecuteScalar<bool>($"SELECT is_policy_checked FROM sys.sql_logins WHERE name = '{fullDbStaffLogin}'"),
						false);
				}
				finally
				{
					Helper.DropServerPrincipals(adminConnection, fullDbStaffLogin);
				}
			}
		}

		[UseSnapshotProtection]
		public void TestEnablesLogin()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = true;
				var sqlSecurityManager = new SqlSecurityManager(Mock.Of<ILogger>(), Db.DatabaseName, allowTransaction: false);

				var factory = new BusinessObjectFactory(adminConnection);
				var staffName = "StaffLogin01";
				var staff = factory.New<GlbStaff>();

				staff.GS_FullName = staffName;
				staff.GS_LoginName = staffName;
				staff.GS_Code = "123";
				staff.StaffPlainTextPassword = "TOPSECRET";

				staff.IsBackupOperator = true;
				staff.IsReadOnlyDBUser = true;

				SetStaffPassword(staff);

				var fullDbStaffLogin = $"{DbUserRepository.GetStaffDbLoginFullPrefix(Db.DatabaseName)}{staffName}";

				try
				{
					factory.Save();

					sqlSecurityManager.BuildServerSecurity(adminConnection, trialRun: false);

					AssertServerPrincipalExistsAndIsNotDisabled(adminConnection, fullDbStaffLogin, "S");

					var adminTask = new DbSecurityAdminTask();
					adminTask.ServiceLogger = Mock.Of<ILogger>();

					adminConnection.ExecuteNonQuery($"ALTER LOGIN {fullDbStaffLogin.QuoteName()} DISABLE");
					AssertEquals(
						"StaffLogin01 sql login should be disabled",
						true,
						adminConnection.ExecuteScalar<bool>($"SELECT is_disabled FROM sys.sql_logins WHERE name = '{fullDbStaffLogin}'"));

					// Act
					adminTask.RunTask(CancellationToken.None);

					// Assert
					AssertEquals(
						"StaffLogin01 sql login should be enabled",
						false,
						adminConnection.ExecuteScalar<bool>($"SELECT is_disabled FROM sys.sql_logins WHERE name = '{fullDbStaffLogin}'"));
				}
				finally
				{
					Helper.DropServerPrincipals(adminConnection, fullDbStaffLogin);
				}
			}
		}

		[UseSnapshotProtection]
		public void TestEnablesEmptyPasswordLogin()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = true;
				var sqlSecurityManager = new SqlSecurityManager(Mock.Of<ILogger>(), Db.DatabaseName, allowTransaction: false);

				var factory = new BusinessObjectFactory(adminConnection);
				var staffName = "StaffLogin01";
				var staff = factory.New<GlbStaff>();

				staff.GS_FullName = staffName;
				staff.GS_LoginName = staffName;
				staff.GS_Code = "123";
				staff.StaffPlainTextPassword = "";

				staff.IsBackupOperator = true;
				staff.IsReadOnlyDBUser = true;

				SetStaffPassword(staff);

				var fullDbStaffLogin = $"{DbUserRepository.GetStaffDbLoginFullPrefix(Db.DatabaseName)}{staffName}";

				try
				{
					factory.Save();

					sqlSecurityManager.BuildServerSecurity(adminConnection, trialRun: false);

					AssertServerPrincipalExistsAndIsNotDisabled(adminConnection, fullDbStaffLogin, "S");

					var adminTask = new DbSecurityAdminTask();
					adminTask.ServiceLogger = Mock.Of<ILogger>();

					adminConnection.ExecuteNonQuery($"ALTER LOGIN {fullDbStaffLogin.QuoteName()} DISABLE");
					AssertEquals(
						"StaffLogin01 sql login should be disabled",
						true,
						adminConnection.ExecuteScalar<bool>($"SELECT is_disabled FROM sys.sql_logins WHERE name = '{fullDbStaffLogin}'"));

					// Act
					adminTask.RunTask(CancellationToken.None);

					// Assert
					AssertEquals(
						"StaffLogin01 sql login should be enabled",
						false,
						adminConnection.ExecuteScalar<bool>($"SELECT is_disabled FROM sys.sql_logins WHERE name = '{fullDbStaffLogin}'"));
				}
				finally
				{
					Helper.DropServerPrincipals(adminConnection, fullDbStaffLogin);
				}
			}
		}

		[UseSnapshotProtection]
		public void TestLoginNameIsNullForStaffMembersWithNoDatabaseAccess()
		{
			using (var adminConnection = Db.NewAdminConnection())
			{
				EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = true;

				var factory = new BusinessObjectFactory(adminConnection);
				var staffName = "StaffLogin01";
				var staff = factory.New<GlbStaff>();

				staff.GS_FullName = staffName;
				staff.GS_LoginName = staffName;
				staff.GS_Code = "123";
				staff.StaffPlainTextPassword = "TOPSECRET";

				staff.IsReadOnlyDBUser = false;
				staff.IsDatabaseDeveloper = false;
				staff.IsBackupOperator = false;

				staff.GS_IsActive = true;

				SetStaffPassword(staff);

				var staffLoginName = $"{DbUserRepository.GetStaffDbLoginFullPrefix(Db.DatabaseName)}{staff.GS_LoginName}";

				try
				{
					factory.Save();

					var adminTask = new DbSecurityAdminTask();
					adminTask.ServiceLogger = Mock.Of<ILogger>();

					// Act
					adminTask.RunTask(CancellationToken.None);

					// Assert
					AssertServerPrincipalIsMissing(adminConnection, staffLoginName);
					AssertEquals(null, staff.SQLUserName);
				}
				finally
				{
					Helper.DropServerPrincipals(adminConnection, staffLoginName);
				}
			}
		}

		[UseSnapshotProtection]
		public void TestDatabaseAccessNotAddedForInactiveRecords()
		{
			using (var adminConnection = Db.NewAdminConnection())
			{
				EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = true;

				var factory = new BusinessObjectFactory(adminConnection);
				var staffName = "StaffLogin01";
				var staff = factory.New<GlbStaff>();

				staff.GS_FullName = staffName;
				staff.GS_LoginName = staffName;
				staff.GS_Code = "123";
				staff.StaffPlainTextPassword = "TOPSECRET";

				staff.IsReadOnlyDBUser = true;
				staff.GS_IsActive = false;

				SetStaffPassword(staff);

				var staffLoginName = $"{DbUserRepository.GetStaffDbLoginFullPrefix(Db.DatabaseName)}{staff.GS_LoginName}";

				try
				{
					factory.Save();

					var adminTask = new DbSecurityAdminTask();
					adminTask.ServiceLogger = Mock.Of<ILogger>();

					// Act
					adminTask.RunTask(CancellationToken.None);

					// Assert
					AssertServerPrincipalIsMissing(adminConnection, staffLoginName);
				}
				finally
				{
					Helper.DropServerPrincipals(adminConnection, staffLoginName);
				}
			}
		}

		[UseSnapshotProtection]
		public void TestDatabaseAccessRemovedForInactiveRecords()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = true;
				var sqlSecurityManager = new SqlSecurityManager(Mock.Of<ILogger>(), Db.DatabaseName, allowTransaction: false);

				var factory = new BusinessObjectFactory(adminConnection);
				var staff01Name = "StaffLogin01";
				var staff = factory.New<GlbStaff>();

				staff.GS_FullName = staff01Name;
				staff.GS_LoginName = staff01Name;
				staff.GS_Code = "123";
				staff.StaffPlainTextPassword = "TOPSECRET";

				staff.IsReadOnlyDBUser = true;

				SetStaffPassword(staff);

				var fullDbStaffLogin = $"{DbUserRepository.GetStaffDbLoginFullPrefix(Db.DatabaseName)}{staff.GS_LoginName}";

				try
				{
					factory.Save();

					sqlSecurityManager.BuildServerSecurity(adminConnection, trialRun: false);

					AssertServerPrincipalExistsAndIsNotDisabled(adminConnection, fullDbStaffLogin, "S");

					using (((ICurrentDbControl)adminConnection).UseDatabase(Db.DatabaseName))
					{
						adminConnection.ExecuteNonQuery($"UPDATE dbo.GlbStaff SET GS_IsActive = 0, GS_SystemLastEditUser = 'E', GS_SystemLastEditTimeUtc = GetUtcDate() WHERE GS_LoginName = N'{staff.GS_LoginName}'");
					}

					var adminTask = new DbSecurityAdminTask();
					adminTask.ServiceLogger = Mock.Of<ILogger>();

					// Act
					adminTask.RunTask(CancellationToken.None);

					// Assert
					AssertServerPrincipalIsMissing(adminConnection, fullDbStaffLogin);
				}
				finally
				{
					Helper.DropServerPrincipals(adminConnection, fullDbStaffLogin);
				}
			}
		}

		[UseSnapshotProtection]
		public void TestChangingLoginNameCreatesNewLoginAndDeletesOld()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = true;
				var sqlSecurityManager = new SqlSecurityManager(Mock.Of<ILogger>(), Db.DatabaseName, allowTransaction: false);

				var factory = new BusinessObjectFactory(adminConnection);
				var staffName = "beaver";
				var staff = factory.New<GlbStaff>();

				staff.GS_FullName = staffName;
				staff.GS_LoginName = staffName;
				staff.GS_Code = "123";
				staff.StaffPlainTextPassword = "TOPSECRET";

				staff.IsReadOnlyDBUser = true;

				SetStaffPassword(staff);

				var fullDbStaffLogin01 = $"{DbUserRepository.GetStaffDbLoginFullPrefix(Db.DatabaseName)}{staff.GS_LoginName}";
				var fullDbStaffLogin02 = fullDbStaffLogin01.Replace("beaver", "evilBeaver");

				try
				{
					factory.Save();

					sqlSecurityManager.BuildServerSecurity(adminConnection, trialRun: false);

					AssertServerPrincipalExistsAndIsNotDisabled(adminConnection, fullDbStaffLogin01, "S");

					using (((ICurrentDbControl)adminConnection).UseDatabase(Db.DatabaseName))
					{
						adminConnection.ExecuteNonQuery($"UPDATE dbo.GlbStaff SET GS_LoginName = N'evilBeaver', GS_SystemLastEditUser = 'E', GS_SystemLastEditTimeUtc = GetUtcDate() WHERE GS_LoginName = N'beaver'");
					}

					AssertServerPrincipalExistsAndIsNotDisabled(adminConnection, fullDbStaffLogin01, "S");
					AssertServerPrincipalIsMissing(adminConnection, fullDbStaffLogin02);
					var adminTask = new DbSecurityAdminTask();

					adminTask.ServiceLogger = Mock.Of<ILogger>();

					// Act
					adminTask.RunTask(CancellationToken.None);

					// Assert
					AssertServerPrincipalExistsAndIsNotDisabled(adminConnection, fullDbStaffLogin02, "S");
					AssertServerPrincipalIsMissing(adminConnection, fullDbStaffLogin01);
				}
				finally
				{
					Helper.DropServerPrincipals(adminConnection, fullDbStaffLogin01, fullDbStaffLogin02);
				}
			}
		}

		[UseSnapshotProtection]
		public void TestADIntegratedHostedSystemsCreateBothWindowsAndSqlLogins()
		{
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock("SYD", DatabaseSecurityModePairList.Codes.Locked)))
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			using (var adminConnection = Db.NewAdminConnection())
			{
				EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = true;

				var currentUserName = TestConstants.ADTestAdminAccount.Name;
				var domainName = TestConstants.DomainPreWin2000;

				var sqlUsername = $"{DbUserRepository.StaffDbLoginPrefix}_{Db.DatabaseName}_{currentUserName}";
				var windowsUsername = TestConstants.ADTestAdminAccount.NameWithDomainPreWindows2000;

				ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;
				SetDomainCredentialCollection();

				var factory = new BusinessObjectFactory(adminConnection);
				var staff = factory.New<GlbStaff>();

				staff.GS_LoginName = currentUserName;
				staff.GS_Code = "123";
				staff.StaffPlainTextPassword = "TOPSECRET";
				staff.DomainName = domainName;
				staff.GS_ActiveDirectoryObjectGuid = Guid.Parse(TestConstants.ADTestAdminAccount.Guid);
				staff.IsReadOnlyDBUser = true;
				staff.IsDatabaseDeveloper = true;

				SetStaffPassword(staff);

				try
				{
					factory.Save();

					AssertDatabasePrincipalIsMissing(adminConnection, sqlUsername);
					AssertDatabasePrincipalIsMissing(adminConnection, windowsUsername);

					var adminTask = new DbSecurityAdminTask();
					adminTask.ServiceLogger = Mock.Of<ILogger>();

					// Act
					adminTask.RunTask(CancellationToken.None);

					// Assert
					AssertServerPrincipalExistsAndIsNotDisabled(adminConnection, sqlUsername, "S");
					AssertServerPrincipalExistsAndIsNotDisabled(adminConnection, windowsUsername, "U");
				}
				finally
				{
					Helper.DropServerPrincipals(adminConnection, sqlUsername, windowsUsername);
				}
			}
		}

		[UseSnapshotProtection]
		public void TestADIntegratedSelfHostedSystemsCreateOnlyWindowsLogins()
		{
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(Constants.NotHostedWithCargoWise, DatabaseSecurityModePairList.Codes.Locked)))
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			using (var adminConnection = Db.NewAdminConnection())
			{
				EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = true;

				var currentUserName = TestConstants.ADTestAdminAccount.Name;
				var domainName = TestConstants.DomainPreWin2000;

				var sqlUsername = $"{DbUserRepository.StaffDbLoginPrefix}_{Db.DatabaseName}_{currentUserName}";
				var windowsUsername = TestConstants.ADTestAdminAccount.NameWithDomainPreWindows2000;

				ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;
				SetDomainCredentialCollection();

				var factory = new BusinessObjectFactory(adminConnection);
				var staff = factory.New<GlbStaff>();

				staff.GS_LoginName = currentUserName;
				staff.GS_Code = "123";
				staff.StaffPlainTextPassword = "TOPSECRET";
				staff.DomainName = domainName;
				staff.GS_ActiveDirectoryObjectGuid = Guid.Parse(TestConstants.ADTestAdminAccount.Guid);
				staff.IsReadOnlyDBUser = true;
				staff.IsDatabaseDeveloper = true;

				SetStaffPassword(staff);

				try
				{
					factory.Save();

					AssertDatabasePrincipalIsMissing(adminConnection, sqlUsername);
					AssertDatabasePrincipalIsMissing(adminConnection, windowsUsername);

					var adminTask = new DbSecurityAdminTask();
					adminTask.ServiceLogger = Mock.Of<ILogger>();

					// Act
					adminTask.RunTask(CancellationToken.None);

					// Assert
					AssertDatabasePrincipalIsMissing(adminConnection, sqlUsername);
					AssertServerPrincipalExistsAndIsNotDisabled(adminConnection, windowsUsername, "U");
				}
				finally
				{
					Helper.DropServerPrincipals(adminConnection, sqlUsername, windowsUsername);
				}
			}
		}

		[UseSnapshotProtection]
		public void TestLoginRightsDependOnHostedLocation()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = true;

				var factory = new BusinessObjectFactory(adminConnection);
				var staffName = "beaver";
				var staff = factory.New<GlbStaff>();

				staff.GS_FullName = staffName;
				staff.GS_LoginName = staffName;
				staff.GS_Code = "123";
				staff.StaffPlainTextPassword = "TOPSECRET";

				staff.IsReadOnlyDBUser = true;
				staff.IsDatabaseDeveloper = true;
				staff.IsBackupOperator = false;

				SetStaffPassword(staff);
				factory.Save();

				var adminTask = new DbSecurityAdminTask();
				adminTask.ServiceLogger = Mock.Of<ILogger>();

				var staffLoginName = $"{DbUserRepository.StaffDbLoginPrefix}_{Db.DatabaseName}_{staffName}";

				try
				{
					// Self-hosted client
					EnvProxy.SetHostedLocationForTest(Constants.NotHostedWithCargoWise);
					adminTask.RunTask(CancellationToken.None);

					AssertServerPermissionExists(adminConnection, "G", "ALTER TRACE", staffLoginName, $"Self-Hosted/IsDeveloper - [{staffLoginName}] was GRANTED [ALTER TRACE] rights on the server?");
					AssertServerPermissionMissing(adminConnection, "D", "VIEW ANY DATABASE", staffLoginName, $"Self-Hosted/IsDeveloper - [{staffLoginName}] was NOT DENIED [VIEW ANY DATABASE] rights on the server?");
					AssertServerPermissionExists(adminConnection, "G", "VIEW ANY DEFINITION", staffLoginName, $"Self-Hosted/IsDeveloper - [{staffLoginName}] was GRANTED [VIEW ANY DEFINITION] rights on the server?");
					AssertServerPermissionExists(adminConnection, "G", "ALTER ANY EVENT SESSION", staffLoginName, $"Self-Hosted/IsDeveloper - [{staffLoginName}] was GRANTED [ALTER ANY EVENT SESSION] rights on the server?");

					// Act/Assert for Hosted at WiseGrid
					EnvProxy.SetHostedLocationForTest("SYD");
					staff.IsBackupOperator = true;

					factory.Save();

					adminTask.RunTask(CancellationToken.None);

					AssertServerPermissionMissing(adminConnection, "G", "ALTER TRACE", staffLoginName, $"Hosted/IsDeveloper - [{staffLoginName}] was NOT GRANTED [ALTER TRACE] rights on the server?");
					AssertServerPermissionExists(adminConnection, "D", "VIEW ANY DATABASE", staffLoginName, $"Hosted/IsDeveloper - [{staffLoginName}] was DENIED [VIEW ANY DATABASE] rights on the server?");
					AssertServerPermissionMissing(adminConnection, "G", "VIEW ANY DEFINITION", staffLoginName, $"Hosted/IsDeveloper - [{staffLoginName}] was NOT GRANTED [VIEW ANY DEFINITION] rights on the server?");
					AssertServerPermissionMissing(adminConnection, "G", "ALTER ANY EVENT SESSION", staffLoginName, $"Hosted/IsDeveloper - [{staffLoginName}] was NOT GRANTED [ALTER ANY EVENT SESSION] rights on the server?");

					staff.IsDatabaseDeveloper = false;

					// Act/Assert for Self-hosted client
					EnvProxy.SetHostedLocationForTest(Constants.NotHostedWithCargoWise);
					factory.Save();
					adminTask.RunTask(CancellationToken.None);

					AssertServerPermissionMissing(adminConnection, "G", "ALTER TRACE", staffLoginName, $"Self-Hosted/Non-Developer - [{staffLoginName}] was NOT GRANTED [ALTER TRACE] rights on the server?");
					AssertServerPermissionMissing(adminConnection, "D", "VIEW ANY DATABASE", staffLoginName, $"Self-Hosted/Non-Developer - [{staffLoginName}] was NOT DENIED [VIEW ANY DATABASE] rights on the server?");
					AssertServerPermissionMissing(adminConnection, "G", "VIEW ANY DEFINITION", staffLoginName, $"Self-Hosted/Non-Developer - [{staffLoginName}] was NOT GRANTED [VIEW ANY DEFINITION] rights on the server?");
					AssertServerPermissionMissing(adminConnection, "G", "ALTER ANY EVENT SESSION", staffLoginName, $"Self-Hosted/Non-Developer - [{staffLoginName}] was NOT GRANTED [ALTER ANY EVENT SESSION] rights on the server?");

					// Act/Assert for Hosted at WiseGrid
					EnvProxy.SetHostedLocationForTest("SYD");
					staff.IsBackupOperator = false;
					factory.Save();
					adminTask.RunTask(CancellationToken.None);

					AssertServerPermissionMissing(adminConnection, "G", "ALTER TRACE", staffLoginName, $"Hosted/Non-Developer - [{staffLoginName}] was NOT GRANTED [ALTER TRACE] rights on the server?");
					AssertServerPermissionExists(adminConnection, "D", "VIEW ANY DATABASE", staffLoginName, $"Hosted/Non-Developer - [{staffLoginName}] was DENIED [VIEW ANY DATABASE] rights on the server?");
					AssertServerPermissionMissing(adminConnection, "G", "VIEW ANY DEFINITION", staffLoginName, $"Hosted/Non-Developer - [{staffLoginName}] was NOT GRANTED [VIEW ANY DEFINITION] rights on the server?");
					AssertServerPermissionMissing(adminConnection, "G", "ALTER ANY EVENT SESSION", staffLoginName, $"Hosted/Non-Developer - [{staffLoginName}] was NOT GRANTED [ALTER ANY EVENT SESSION] rights on the server?");
				}
				finally
				{
					EnvProxy.SetHostedLocationForTest(null);
					Helper.DropServerPrincipals(adminConnection, staffLoginName);
				}
			}
		}

		[UseSnapshotProtection]
		public void TestSqlUserNameADIntegrationDisabledWiseCloudHosted()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = true;

				EnvProxy.SetHostedLocationForTest("SYD");
				ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = false;

				var factory = new BusinessObjectFactory(adminConnection);
				var staffName = "UserName001";
				var staff = factory.New<GlbStaff>();

				staff.GS_FullName = staffName;
				staff.GS_LoginName = staffName;
				staff.GS_Code = "123";
				staff.StaffPlainTextPassword = "TOPSECRET";

				staff.IsReadOnlyDBUser = true;
				staff.GS_IsActive = true;

				SetStaffPassword(staff);

				var staffLoginName = $"EnterpriseDbUser_{Db.DatabaseName}_{staffName}";

				try
				{
					factory.Save();

					var adminTask = new DbSecurityAdminTask();
					adminTask.ServiceLogger = Mock.Of<ILogger>();

					// Act
					adminTask.RunTask(CancellationToken.None);

					// Assert
					AssertServerPrincipalExistsAndIsNotDisabled(adminConnection, staff.SQLUserName, "S");
					AssertEquals(staffLoginName, staff.SQLUserName);
				}
				finally
				{
					Helper.DropServerPrincipals(adminConnection, staffLoginName);
				}
			}
		}

		[UseSnapshotProtection]
		public void TestSqlUserNameADIntegrationDisabledSelfHosted()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = true;

				EnvProxy.SetHostedLocationForTest(Constants.NotHostedWithCargoWise);
				ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = false;

				var factory = new BusinessObjectFactory(adminConnection);
				var staffName = "UserName001";
				var staff = factory.New<GlbStaff>();

				staff.GS_FullName = staffName;
				staff.GS_LoginName = staffName;
				staff.GS_Code = "123";
				staff.StaffPlainTextPassword = "TOPSECRET";

				staff.IsReadOnlyDBUser = true;
				staff.GS_IsActive = true;

				SetStaffPassword(staff);

				var staffLoginName = $"EnterpriseDbUser_{Db.DatabaseName}_{staffName}";

				try
				{
					factory.Save();

					AssertEquals(null, staff.SQLUserName);

					var adminTask = new DbSecurityAdminTask();
					adminTask.ServiceLogger = Mock.Of<ILogger>();

					// Act
					adminTask.RunTask(CancellationToken.None);

					// Assert
					AssertServerPrincipalExistsAndIsNotDisabled(adminConnection, staff.SQLUserName, "S");
					AssertEquals(staffLoginName, staff.SQLUserName);
				}
				finally
				{
					Helper.DropServerPrincipals(adminConnection, staffLoginName);
				}
			}
		}

		[UseSnapshotProtection]
		public void TestSQLUserNameAdIntegrationEnabledHostedInWiseCloud()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = true;

				var userName = TestConstants.ADTestAdminAccount.Name;
				var domainName = TestConstants.DomainPreWin2000;

				var sqlUsername = $"{DbUserRepository.StaffDbLoginPrefix}_{Db.DatabaseName}_{userName}";
				var windowsUsername = TestConstants.ADTestAdminAccount.NameWithDomainPreWindows2000;

				EnvProxy.SetHostedLocationForTest("SYD");
				ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;

				var factory = new BusinessObjectFactory(adminConnection);

				SetDomainCredentialCollection();

				var staff = factory.New<GlbStaff>();
				staff.GS_LoginName = userName;
				staff.DomainName = domainName;
				staff.IsDatabaseDeveloper = true;
				staff.GS_ActiveDirectoryObjectGuid = Guid.NewGuid();

				staff.IsDatabaseDeveloper = true;

				SetStaffPassword(staff);

				try
				{
					factory.Save();

					AssertEquals(null, staff.SQLUserName);

					var adminTask = new DbSecurityAdminTask();
					adminTask.ServiceLogger = Mock.Of<ILogger>();

					// Act
					adminTask.RunTask(CancellationToken.None);

					// Assert
					AssertServerPrincipalExistsAndIsNotDisabled(adminConnection, sqlUsername, "S");
					AssertServerPrincipalExistsAndIsNotDisabled(adminConnection, windowsUsername, "U");
					AssertEquals("Hosted. AD Integration enabled, Staff linked to AD:", sqlUsername.ToLower(), staff.SQLUserName.ToLower());
				}
				finally
				{
					Helper.DropServerPrincipals(adminConnection, sqlUsername, windowsUsername);
				}
			}
		}

		[UseSnapshotProtection]
		public void TestSQLUserNameAdIntegrationEnabledSelfHosted()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = true;

				var userName = TestConstants.ADTestAdminAccount.Name;
				var domainName = TestConstants.DomainPreWin2000;

				var sqlUsername = $"{DbUserRepository.StaffDbLoginPrefix}_{Db.DatabaseName}_{userName}";
				var windowsUsername = TestConstants.ADTestAdminAccount.NameWithDomainPreWindows2000;

				EnvProxy.SetHostedLocationForTest(Constants.NotHostedWithCargoWise);
				ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;

				var factory = new BusinessObjectFactory(adminConnection);

				SetDomainCredentialCollection();

				var staff = factory.New<GlbStaff>();
				staff.GS_LoginName = userName;
				staff.DomainName = domainName;
				staff.IsDatabaseDeveloper = true;
				staff.IsBackupOperator = true;
				staff.GS_ActiveDirectoryObjectGuid = Guid.NewGuid();

				staff.IsDatabaseDeveloper = true;

				SetStaffPassword(staff);

				try
				{
					factory.Save();

					AssertEquals(null, staff.SQLUserName);

					var adminTask = new DbSecurityAdminTask();
					adminTask.ServiceLogger = Mock.Of<ILogger>();

					// Act
					adminTask.RunTask(CancellationToken.None);

					// Assert
					AssertEquals("Self-hosted. AD Integration enabled, Staff linked to AD:", windowsUsername.ToLower(), staff.SQLUserName.ToLower());
					AssertServerPrincipalIsMissing(adminConnection, sqlUsername);
					AssertServerPrincipalExistsAndIsNotDisabled(adminConnection, windowsUsername, "U");
				}
				finally
				{
					Helper.DropServerPrincipals(
						adminConnection,
						windowsUsername,
						sqlUsername);
				}
			}
		}

		[UseSnapshotProtection]
		public void TestCorrectUserRightsAreAddedToDatabaseDeveloperSelfHostedOpenUsingModernSecuritySystem()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(Constants.NotHostedWithCargoWise, DatabaseSecurityModePairList.Codes.OpenMode)))
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				CorrectUserRightsAreAddedToDatabaseDeveloper(adminConnection, "Self-Hosted, open");
			}
		}

		[UseSnapshotProtection]
		public void TestCorrectUserRightsAreAddedToDatabaseDeveloperWCHostedNotOpen()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock("SYD", DatabaseSecurityModePairList.Codes.Indeterminate)))
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				CorrectUserRightsAreAddedToDatabaseDeveloper(adminConnection, "WC-Hosted");
			}
		}

		[UseSnapshotProtection]
		public void TestCorrectUserRightsAreAddedToDatabaseReaderlfHostedOpenUsingModernSecuritySystem()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(Constants.NotHostedWithCargoWise, DatabaseSecurityModePairList.Codes.OpenMode)))
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				CorrectUserRightsAreAddedToDatabaseReader(adminConnection, "Self-Hosted, open");
			}
		}

		[UseSnapshotProtection]
		public void TestCorrectUserRightsAreAddedToDatabaseReaderWCHostedNotOpen()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock("SYD", DatabaseSecurityModePairList.Codes.Indeterminate)))
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				CorrectUserRightsAreAddedToDatabaseReader(adminConnection, "WC-Hosted");
			}
		}

		[UseSnapshotProtection]
		public void TestCorrectUserRightsAreAddedToDatabaseBackupOperatorlfHostedOpenUsingModernSecuritySystemUsingModernSecuritySystem()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(Constants.NotHostedWithCargoWise, DatabaseSecurityModePairList.Codes.OpenMode)))
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				CorrectUserRightsAreAddedToDatabaseBackupOperator(adminConnection, wiseCloudHosted: false, "Self-Hosted, open");
			}
		}

		[UseSnapshotProtection]
		public void TestCorrectUserRightsAreAddedToDatabaseBackupOperatorrWCHostedNotOpenUsingModernSecuritySystem()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock("SYD", DatabaseSecurityModePairList.Codes.Indeterminate)))
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				CorrectUserRightsAreAddedToDatabaseBackupOperator(adminConnection, wiseCloudHosted: true, "WC-Hosted");
			}
		}

		[UseSnapshotProtection]
		public void TestLoginsAndUsersAreDroppedWhenStaffMemberIsDeletedSelfHostedOpen()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(Constants.NotHostedWithCargoWise, DatabaseSecurityModePairList.Codes.OpenMode)))
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				LoginsAndUsersAreDroppedWhenStaffMemberIsDeleted(adminConnection, "Self-Hosted, open");
			}
		}

		[UseSnapshotProtection]
		public void TestLoginsAndUsersAreDroppedWhenStaffMemberIsDeletedWCHostedNotOpen()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock("SYD", DatabaseSecurityModePairList.Codes.Indeterminate)))
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				LoginsAndUsersAreDroppedWhenStaffMemberIsDeleted(adminConnection, "WC-Hosted");
			}
		}

		[UseSnapshotProtection]
		public void TestLoginsAndUsersAreDroppedWhenDatabaseAccessIsRevokedFromStaffMemberSelfHostedOpenUsingModernSecuritySystem()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(Constants.NotHostedWithCargoWise, DatabaseSecurityModePairList.Codes.OpenMode)))
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				LoginsAndUsersAreDroppedWhenDatabaseAccessIsRevokedFromStaffMember(adminConnection, "Self-Hosted, open");
			}
		}

		[UseSnapshotProtection]
		public void TestLoginsAndUsersAreDroppedWhenDatabaseAccessIsRevokedFromStaffMemberWCHostedNotOpenUsingModernSecuritySystem()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock("SYD", DatabaseSecurityModePairList.Codes.Indeterminate)))
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				LoginsAndUsersAreDroppedWhenDatabaseAccessIsRevokedFromStaffMember(adminConnection, "WC-Hosted");
			}
		}

		[UseSnapshotProtection]
		public void TestChangingGlbStaffLoginNameChangesItsSqlLoginNameCase()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = true;
				ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = false;

				var sqlSecurityManager = new SqlSecurityManager(Mock.Of<ILogger>(), Db.DatabaseName, allowTransaction: false);

				var factory = new BusinessObjectFactory(adminConnection);
				var staffName = "beaver";
				var staff = factory.New<GlbStaff>();

				staff.GS_FullName = staffName;
				staff.GS_LoginName = staffName;
				staff.GS_Code = "123";
				staff.StaffPlainTextPassword = "TOPSECRET";

				staff.IsReadOnlyDBUser = true;
				SetStaffPassword(staff);

				var fullDbStaffLogin01 = $"{DbUserRepository.GetStaffDbLoginFullPrefix(Db.DatabaseName)}{staff.GS_LoginName}";
				var fullDbStaffLogin02 = fullDbStaffLogin01.Replace("beaver", "Beaver");

				try
				{
					factory.Save();
					sqlSecurityManager.BuildServerSecurity(adminConnection, trialRun: false);

					AssertServerPrincipalExistsAndIsNotDisabled(adminConnection, fullDbStaffLogin01, "S", caseSensitive: true);

					using (((ICurrentDbControl)adminConnection).UseDatabase(Db.DatabaseName))
					{
						adminConnection.ExecuteNonQuery($"UPDATE GlbStaff SET GS_LoginName = N'Beaver', GS_SystemLastEditUser = 'E', GS_SystemLastEditTimeUtc = GetUtcDate() WHERE GS_LoginName = N'beaver'");
					}

					AssertServerPrincipalExistsAndIsNotDisabled(adminConnection, fullDbStaffLogin01, "S", caseSensitive: true);
					AssertServerPrincipalIsMissing(adminConnection, fullDbStaffLogin02, caseSensitive: true);

					var adminTask = new DbSecurityAdminTask();
					adminTask.ServiceLogger = Mock.Of<ILogger>();

					// Act
					adminTask.RunTask(CancellationToken.None);

					// Assert
					AssertServerPrincipalExistsAndIsNotDisabled(adminConnection, fullDbStaffLogin02, "S", caseSensitive: true);
					AssertServerPrincipalIsMissing(adminConnection, fullDbStaffLogin01, caseSensitive: true);
				}
				finally
				{
					Helper.DropServerPrincipals(adminConnection, fullDbStaffLogin01, fullDbStaffLogin02);
				}
			}
		}

		#region Helper methods for tests from Tests from MasterFiles.Business.Test for ModernSecurity

		void LoginsAndUsersAreDroppedWhenStaffMemberIsDeleted(AdminConnection adminConnection, string hosingMode)
		{
			// Arrange
			var sqlSecurityManager = new SqlSecurityManager(Mock.Of<ILogger>(), Db.DatabaseName, allowTransaction: false);
			EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = true;

			var factory = new BusinessObjectFactory(adminConnection);
			var staffDeveloper = factory.New<GlbStaff>();

			staffDeveloper.GS_FullName = databaseDeveloperLogin;
			staffDeveloper.GS_LoginName = databaseDeveloperLogin;
			staffDeveloper.GS_Code = "123";
			staffDeveloper.StaffPlainTextPassword = "TOPSECRET";

			staffDeveloper.IsReadOnlyDBUser = true;
			staffDeveloper.IsDatabaseDeveloper = true;
			staffDeveloper.IsBackupOperator = false;

			var staffReader = factory.New<GlbStaff>();

			staffReader.GS_FullName = databaseReaderLogin;
			staffReader.GS_LoginName = databaseReaderLogin;
			staffReader.GS_Code = "124";
			staffReader.StaffPlainTextPassword = "TOPSECRET";

			staffReader.IsReadOnlyDBUser = true;
			staffReader.IsDatabaseDeveloper = false;
			staffReader.IsBackupOperator = false;

			var staffBackupOperator = factory.New<GlbStaff>();

			staffBackupOperator.GS_FullName = databaseBackupOperatorLogin;
			staffBackupOperator.GS_LoginName = databaseBackupOperatorLogin;
			staffBackupOperator.GS_Code = "125";
			staffBackupOperator.StaffPlainTextPassword = "TOPSECRET";

			staffBackupOperator.IsReadOnlyDBUser = false;
			staffBackupOperator.IsDatabaseDeveloper = false;
			staffBackupOperator.IsBackupOperator = true;

			SetStaffPassword(staffDeveloper);
			SetStaffPassword(staffReader);
			SetStaffPassword(staffBackupOperator);

			var staffDeveloperLoginName = $"{DbUserRepository.StaffDbLoginPrefix}_{Db.DatabaseName}_{staffDeveloper.GS_LoginName}";
			var staffReaderLoginName = $"{DbUserRepository.StaffDbLoginPrefix}_{Db.DatabaseName}_{staffReader.GS_LoginName}";
			var staffBackupOperatorLoginName = $"{DbUserRepository.StaffDbLoginPrefix}_{Db.DatabaseName}_{staffBackupOperator.GS_LoginName}";

			try
			{
				factory.Save();

				sqlSecurityManager.BuildSecurity(adminConnection, CancellationToken.None);

				AssertServerPrincipalExistsAndIsNotDisabled(adminConnection, staffDeveloperLoginName, "S");
				AssertServerPrincipalExistsAndIsNotDisabled(adminConnection, staffReaderLoginName, "S");
				AssertServerPrincipalExistsAndIsNotDisabled(adminConnection, staffBackupOperatorLoginName, "S");

				foreach (var dbName in adminConnection.GetDatabases(DatabaseType.All & ~DatabaseType.SingleSharedRef))
				{
					using (((ICurrentDbControl)adminConnection).UseDatabase(dbName))
					{
						AssertDatabasePrincipalExists(adminConnection, staffDeveloperLoginName, "S");
						AssertDatabasePrincipalExists(adminConnection, staffReaderLoginName, "S");
						AssertDatabasePrincipalExists(adminConnection, staffBackupOperatorLoginName, "S");
					}
				}

				staffDeveloper.Delete();
				staffReader.Delete();
				staffBackupOperator.Delete();
				factory.Save();

				// Act
				var adminTask = new DbSecurityAdminTask();
				adminTask.ServiceLogger = Mock.Of<ILogger>();

				adminTask.RunTask(CancellationToken.None);

				// Assert
				AssertServerPrincipalIsMissing(adminConnection, staffDeveloperLoginName, $"{hosingMode}: Login '{staffDeveloperLoginName}' should have been dropped.");
				AssertServerPrincipalIsMissing(adminConnection, staffReaderLoginName, $"{hosingMode}: Login '{staffReaderLoginName}' should have been dropped.");
				AssertServerPrincipalIsMissing(adminConnection, staffBackupOperatorLoginName, $"{hosingMode}: Login '{staffBackupOperatorLoginName}' should have been dropped.");

				foreach (var dbName in adminConnection.GetDatabases(DatabaseType.All & ~DatabaseType.SingleSharedRef))
				{
					using (((ICurrentDbControl)adminConnection).UseDatabase(dbName))
					{
						AssertDatabasePrincipalIsMissing(adminConnection, staffDeveloperLoginName, $"{hosingMode}: User '{staffDeveloperLoginName}' should have been dropped from database '{dbName}'");
						AssertDatabasePrincipalIsMissing(adminConnection, staffReaderLoginName, $"{hosingMode}: User '{staffDeveloperLoginName}' should have been dropped from database '{dbName}'");
						AssertDatabasePrincipalIsMissing(adminConnection, staffBackupOperatorLoginName, $"{hosingMode}: User '{staffDeveloperLoginName}' should have been dropped from database '{dbName}'");
					}
				}
			}
			finally
			{
				Helper.DropServerPrincipals(
					adminConnection,
					staffDeveloperLoginName,
					staffReaderLoginName,
					staffBackupOperatorLoginName);
			}
		}

		void LoginsAndUsersAreDroppedWhenDatabaseAccessIsRevokedFromStaffMember(AdminConnection adminConnection, string hosingMode)
		{
			// Arrange
			var sqlSecurityManager = new SqlSecurityManager(Mock.Of<ILogger>(), Db.DatabaseName, allowTransaction: false);
			EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = true;

			var factory = new BusinessObjectFactory(adminConnection);
			var staffDeveloper = factory.New<GlbStaff>();

			staffDeveloper.GS_FullName = databaseDeveloperLogin;
			staffDeveloper.GS_LoginName = databaseDeveloperLogin;
			staffDeveloper.GS_Code = "123";
			staffDeveloper.StaffPlainTextPassword = "TOPSECRET";

			staffDeveloper.IsReadOnlyDBUser = true;
			staffDeveloper.IsDatabaseDeveloper = true;
			staffDeveloper.IsBackupOperator = false;

			var staffReader = factory.New<GlbStaff>();

			staffReader.GS_FullName = databaseReaderLogin;
			staffReader.GS_LoginName = databaseReaderLogin;
			staffReader.GS_Code = "124";
			staffReader.StaffPlainTextPassword = "TOPSECRET";

			staffReader.IsReadOnlyDBUser = true;
			staffReader.IsDatabaseDeveloper = false;
			staffReader.IsBackupOperator = false;

			var staffBackupOperator = factory.New<GlbStaff>();

			staffBackupOperator.GS_FullName = databaseBackupOperatorLogin;
			staffBackupOperator.GS_LoginName = databaseBackupOperatorLogin;
			staffBackupOperator.GS_Code = "125";
			staffBackupOperator.StaffPlainTextPassword = "TOPSECRET";

			staffBackupOperator.IsReadOnlyDBUser = false;
			staffBackupOperator.IsDatabaseDeveloper = false;
			staffBackupOperator.IsBackupOperator = true;

			SetStaffPassword(staffDeveloper);
			SetStaffPassword(staffReader);
			SetStaffPassword(staffBackupOperator);

			var staffDeveloperLoginName = $"{DbUserRepository.StaffDbLoginPrefix}_{Db.DatabaseName}_{staffDeveloper.GS_LoginName}";
			var staffReaderLoginName = $"{DbUserRepository.StaffDbLoginPrefix}_{Db.DatabaseName}_{staffReader.GS_LoginName}";
			var staffBackupOperatorLoginName = $"{DbUserRepository.StaffDbLoginPrefix}_{Db.DatabaseName}_{staffBackupOperator.GS_LoginName}";

			try
			{
				factory.Save();

				sqlSecurityManager.BuildSecurity(adminConnection, CancellationToken.None);

				AssertServerPrincipalExistsAndIsNotDisabled(adminConnection, staffDeveloperLoginName, "S");
				AssertServerPrincipalExistsAndIsNotDisabled(adminConnection, staffReaderLoginName, "S");
				AssertServerPrincipalExistsAndIsNotDisabled(adminConnection, staffBackupOperatorLoginName, "S");

				foreach (var dbName in adminConnection.GetDatabases(DatabaseType.All & ~DatabaseType.SingleSharedRef))
				{
					using (((ICurrentDbControl)adminConnection).UseDatabase(dbName))
					{
						AssertDatabasePrincipalExists(adminConnection, staffDeveloperLoginName, "S");
						AssertDatabasePrincipalExists(adminConnection, staffReaderLoginName, "S");
						AssertDatabasePrincipalExists(adminConnection, staffBackupOperatorLoginName, "S");
					}
				}

				staffDeveloper.IsDatabaseDeveloper = false;
				staffDeveloper.IsBackupOperator = false;
				staffDeveloper.IsReadOnlyDBUser = false;

				staffReader.IsDatabaseDeveloper = false;
				staffReader.IsBackupOperator = false;
				staffReader.IsReadOnlyDBUser = false;

				staffBackupOperator.IsDatabaseDeveloper = false;
				staffBackupOperator.IsBackupOperator = false;
				staffBackupOperator.IsReadOnlyDBUser = false;

				factory.Save();

				// Act
				var adminTask = new DbSecurityAdminTask();
				adminTask.ServiceLogger = Mock.Of<ILogger>();

				adminTask.RunTask(CancellationToken.None);

				// Assert
				AssertServerPrincipalIsMissing(adminConnection, staffDeveloperLoginName, $"{hosingMode}: Login '{staffDeveloperLoginName}' should have been dropped.");
				AssertServerPrincipalIsMissing(adminConnection, staffReaderLoginName, $"{hosingMode}: Login '{staffReaderLoginName}' should have been dropped.");
				AssertServerPrincipalIsMissing(adminConnection, staffBackupOperatorLoginName, $"{hosingMode}: Login '{staffBackupOperatorLoginName}' should have been dropped.");

				foreach (var dbName in adminConnection.GetDatabases(DatabaseType.All & ~DatabaseType.SingleSharedRef))
				{
					using (((ICurrentDbControl)adminConnection).UseDatabase(dbName))
					{
						AssertDatabasePrincipalIsMissing(adminConnection, staffDeveloperLoginName, $"{hosingMode}: User '{staffDeveloperLoginName}' should have been dropped from database '{dbName}'");
						AssertDatabasePrincipalIsMissing(adminConnection, staffReaderLoginName, $"{hosingMode}: User '{staffDeveloperLoginName}' should have been dropped from database '{dbName}'");
						AssertDatabasePrincipalIsMissing(adminConnection, staffBackupOperatorLoginName, $"{hosingMode}: User '{staffDeveloperLoginName}' should have been dropped from database '{dbName}'");
					}
				}
			}
			finally
			{
				Helper.DropServerPrincipals(
					adminConnection,
					staffDeveloperLoginName,
					staffReaderLoginName,
					staffBackupOperatorLoginName);
			}
		}

		void CorrectUserRightsAreAddedToDatabaseDeveloper(AdminConnection adminConnection, string hostingMode)
		{
			// Arrange
			EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = true;
			var userRepositoryDatabaseName = adminConnection.GetDatabases(DatabaseType.UserRepository).First();

			var factory = new BusinessObjectFactory(adminConnection);
			var staff = factory.New<GlbStaff>();

			staff.GS_FullName = databaseDeveloperLogin;
			staff.GS_LoginName = databaseDeveloperLogin;
			staff.GS_Code = "123";
			staff.StaffPlainTextPassword = "TOPSECRET";

			staff.IsReadOnlyDBUser = true;
			staff.IsDatabaseDeveloper = true;
			staff.IsBackupOperator = false;

			SetStaffPassword(staff);

			var staffLoginName = $"{DbUserRepository.StaffDbLoginPrefix}_{Db.DatabaseName}_{staff.GS_LoginName}";

			try
			{
				factory.Save();

				var adminTask = new DbSecurityAdminTask();
				adminTask.ServiceLogger = Mock.Of<ILogger>();

				// Act
				adminTask.RunTask(CancellationToken.None);

				// Assert
				using (((ICurrentDbControl)adminConnection).UseDatabase(userRepositoryDatabaseName))
				{
					AssertDatabaseMembershipExists(adminConnection, DbRoleTypes.DbDataWriterRole, staffLoginName, $"{hostingMode}/IsDeveloper has [datawriter] rights on database '{userRepositoryDatabaseName}'?");
					AssertDatabaseMembershipExists(adminConnection, DbRoleTypes.CwRestrictedReaderRole, staffLoginName, $"{hostingMode}/IsDeveloper has [cwRestrictedReaderRole] rights on database '{userRepositoryDatabaseName}'?");
					AssertDatabaseMembershipMissing(adminConnection, DbRoleTypes.DbBackupOperatorRole, staffLoginName, $"{hostingMode}/IsDeveloper has NO [backupoperator] rights on database '{userRepositoryDatabaseName}'?");

					AssertDatabasePermissionExists(adminConnection, "G", "CREATE TYPE", staffLoginName, $"{hostingMode}/IsDeveloper - [{staffLoginName}] has [CREATE TYPE] rights on user repository DB?");
					AssertDatabasePermissionExists(adminConnection, "G", "CREATE SCHEMA", staffLoginName, $"{hostingMode}/IsDeveloper - [{staffLoginName}] has [CREATE SCHEMA] rights on user repository DB?");
					AssertDatabasePermissionExists(adminConnection, "G", "CREATE TABLE", staffLoginName, $"{hostingMode}/IsDeveloper - [{staffLoginName}] has [CREATE TABLE] rights on user repository DB?");
					AssertDatabasePermissionExists(adminConnection, "G", "CREATE VIEW", staffLoginName, $"{hostingMode}/IsDeveloper - [{staffLoginName}] has [CREATE VIEW] rights on user repository DB?");
					AssertDatabasePermissionExists(adminConnection, "G", "CREATE FUNCTION", staffLoginName, $"{hostingMode}/IsDeveloper - [{staffLoginName}] has [CREATE FUNCTION] rights on user repository DB?");
					AssertDatabasePermissionExists(adminConnection, "G", "CREATE PROCEDURE", staffLoginName, $"{hostingMode}/IsDeveloper - [{staffLoginName}] has [CREATE PROCEDURE] rights on user repository DB?");
					AssertDatabasePermissionExists(adminConnection, "G", "INSERT", staffLoginName, $"{hostingMode}/IsDeveloper - [{staffLoginName}] has [INSERT] rights on user repository DB?");
					AssertDatabasePermissionExists(adminConnection, "G", "UPDATE", staffLoginName, $"{hostingMode}/IsDeveloper - [{staffLoginName}] has [UPDATE] rights on user repository DB?");
					AssertDatabasePermissionExists(adminConnection, "G", "DELETE", staffLoginName, $"{hostingMode}/IsDeveloper - [{staffLoginName}] has [DELETE] rights on user repository DB?");
					AssertDatabasePermissionExists(adminConnection, "G", "REFERENCES", staffLoginName, $"{hostingMode}/IsDeveloper - [{staffLoginName}] has [REFERENCES] rights on user repository DB?");
					AssertSchemaPermissionExists(adminConnection, "G", "ALTER", "dbo", staffLoginName, $"{hostingMode}/IsDeveloper - [{staffLoginName}] has [ALTER dbo SCHEMA] rights on user repository DB?");
					AssertDatabasePermissionExists(adminConnection, "G", "EXECUTE", staffLoginName, $"{hostingMode}/IsDeveloper - [{staffLoginName}] has [EXECUTE] rights on user repository DB??");
				}

				foreach (var dbName in adminConnection.GetDatabases(DatabaseType.All & ~DatabaseType.UserRepository & ~DatabaseType.SingleSharedRef))
				{
					using (((ICurrentDbControl)adminConnection).UseDatabase(dbName))
					{
						AssertDatabaseMembershipMissing(adminConnection, DbRoleTypes.DbDataWriterRole, staffLoginName, $"{hostingMode}/IsDeveloper has NO [datawriter] rights on database '{dbName}'?");
						AssertDatabaseMembershipExists(adminConnection, DbRoleTypes.CwRestrictedReaderRole, staffLoginName, $"{hostingMode}/IsDeveloper has [cwRestrictedReaderRole] rights on database '{dbName}'?");
						AssertDatabaseMembershipMissing(adminConnection, DbRoleTypes.DbBackupOperatorRole, staffLoginName, $"{hostingMode}/IsDeveloper has NO [backupoperator] rights on database '{dbName}'?");

						AssertDatabasePermissionMissing(adminConnection, "G", "CREATE TYPE", staffLoginName, $"{hostingMode}/IsDeveloper - [{staffLoginName}] has NO [CREATE TYPE] rights on database '{dbName}'?");
						AssertDatabasePermissionMissing(adminConnection, "G", "CREATE SCHEMA", staffLoginName, $"{hostingMode}/IsDeveloper - [{staffLoginName}] has NO [CREATE SCHEMA] rights on database '{dbName}'?");
						AssertDatabasePermissionMissing(adminConnection, "G", "CREATE TABLE", staffLoginName, $"{hostingMode}/IsDeveloper - [{staffLoginName}] has NO [CREATE TABLE] rights on database '{dbName}'?");
						AssertDatabasePermissionMissing(adminConnection, "G", "CREATE VIEW", staffLoginName, $"{hostingMode}/IsDeveloper - [{staffLoginName}] has NO [CREATE VIEW] rights on database '{dbName}'?");
						AssertDatabasePermissionMissing(adminConnection, "G", "CREATE FUNCTION", staffLoginName, $"{hostingMode}/IsDeveloper - [{staffLoginName}] has NO [CREATE FUNCTION] rights on database '{dbName}'?");
						AssertDatabasePermissionMissing(adminConnection, "G", "CREATE PROCEDURE", staffLoginName, $"{hostingMode}/IsDeveloper - [{staffLoginName}] has [CREATE PROCEDURE] rights on database '{dbName}'?");
						AssertDatabasePermissionMissing(adminConnection, "G", "INSERT", staffLoginName, $"{hostingMode}/IsDeveloper - [{staffLoginName}] has NO [INSERT] rights on database '{dbName}'?");
						AssertDatabasePermissionMissing(adminConnection, "G", "UPDATE", staffLoginName, $"{hostingMode}/IsDeveloper - [{staffLoginName}] has NO [UPDATE] rights on database '{dbName}'?");
						AssertDatabasePermissionMissing(adminConnection, "G", "DELETE", staffLoginName, $"{hostingMode}/IsDeveloper - [{staffLoginName}] has NO [DELETE] rights on database '{dbName}'?");
						AssertDatabasePermissionMissing(adminConnection, "G", "REFERENCES", staffLoginName, $"{hostingMode}/IsDeveloper - [{staffLoginName}] has NO [REFERENCES] rights on database '{dbName}'?");
						AssertSchemaPermissionMissing(adminConnection, "G", "ALTER", "dbo", staffLoginName, $"{hostingMode}/IsDeveloper - [{staffLoginName}] has NO [ALTER dbo SCHEMA] rights on user repository DB?");
						AssertDatabasePermissionMissing(adminConnection, "G", "EXECUTE", staffLoginName, $"{hostingMode}/IsDeveloper - [{staffLoginName}] has NO [EXECUTE] rights on udatabase '{dbName}'?");
					}
				}
			}
			finally
			{
				Helper.DropServerPrincipals(adminConnection, staffLoginName);
			}
		}

		void CorrectUserRightsAreAddedToDatabaseReader(AdminConnection adminConnection, string hostingMode)
		{
			// Arrange
			EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = true;

			var userRepositoryDatabaseName = adminConnection.GetDatabases(DatabaseType.UserRepository).First();

			var factory = new BusinessObjectFactory(adminConnection);
			var staff = factory.New<GlbStaff>();

			staff.GS_FullName = databaseReaderLogin;
			staff.GS_LoginName = databaseReaderLogin;
			staff.GS_Code = "123";
			staff.StaffPlainTextPassword = "TOPSECRET";

			staff.IsReadOnlyDBUser = true;
			staff.IsDatabaseDeveloper = false;
			staff.IsBackupOperator = false;

			SetStaffPassword(staff);

			var staffLoginName = $"{DbUserRepository.StaffDbLoginPrefix}_{Db.DatabaseName}_{staff.GS_LoginName}";

			try
			{
				factory.Save();

				var adminTask = new DbSecurityAdminTask();
				adminTask.ServiceLogger = Mock.Of<ILogger>();

				// Act
				adminTask.RunTask(CancellationToken.None);

				// Assert
				using (((ICurrentDbControl)adminConnection).UseDatabase(userRepositoryDatabaseName))
				{
					AssertDatabaseMembershipMissing(adminConnection, DbRoleTypes.DbDataWriterRole, staffLoginName, $"{hostingMode}/IsReadOnly - [{staffLoginName}] has NO [datawriter] rights on database '{userRepositoryDatabaseName}'?");
					AssertDatabaseMembershipExists(adminConnection, DbRoleTypes.CwRestrictedReaderRole, staffLoginName, $"{hostingMode}/IsReadOnly - [{staffLoginName}] has [cwRestrictedReaderRole] rights on database '{userRepositoryDatabaseName}'?");
					AssertDatabaseMembershipMissing(adminConnection, DbRoleTypes.DbBackupOperatorRole, staffLoginName, $"{hostingMode}/IsReadOnly - [{staffLoginName}] has NO [backupoperator] rights on database '{userRepositoryDatabaseName}'?");

					AssertDatabasePermissionMissing(adminConnection, "G", "CREATE TYPE", staffLoginName, $"{hostingMode}/IsReadOnly - [{staffLoginName}] has NO [CREATE TYPE] rights on database '{userRepositoryDatabaseName}'?");
					AssertDatabasePermissionMissing(adminConnection, "G", "CREATE SCHEMA", staffLoginName, $"{hostingMode}/IsReadOnly - [{staffLoginName}] has NO [CREATE SCHEMA] rights on database '{userRepositoryDatabaseName}'?");
					AssertDatabasePermissionMissing(adminConnection, "G", "CREATE TABLE", staffLoginName, $"{hostingMode}/IsReadOnly - [{staffLoginName}] has NO [CREATE TABLE] rights on database '{userRepositoryDatabaseName}'?");
					AssertDatabasePermissionMissing(adminConnection, "G", "CREATE VIEW", staffLoginName, $"{hostingMode}/IsReadOnly - [{staffLoginName}] has NO [CREATE VIEW] rights on database '{userRepositoryDatabaseName}'?");
					AssertDatabasePermissionMissing(adminConnection, "G", "CREATE FUNCTION", staffLoginName, $"{hostingMode}/IsReadOnly - [{staffLoginName}] has NO [CREATE FUNCTION] rights on database '{userRepositoryDatabaseName}'?");
					AssertDatabasePermissionMissing(adminConnection, "G", "CREATE PROCEDURE", staffLoginName, $"{hostingMode}/IsReadOnly - [{staffLoginName}] has [CREATE PROCEDURE] rights on database '{userRepositoryDatabaseName}'?");
					AssertDatabasePermissionMissing(adminConnection, "G", "INSERT", staffLoginName, $"{hostingMode}/IsReadOnly - [{staffLoginName}] has NO [INSERT] rights on database '{userRepositoryDatabaseName}'?");
					AssertDatabasePermissionMissing(adminConnection, "G", "UPDATE", staffLoginName, $"{hostingMode}/IsReadOnly - [{staffLoginName}] has NO [UPDATE] rights on database '{userRepositoryDatabaseName}'?");
					AssertDatabasePermissionMissing(adminConnection, "G", "DELETE", staffLoginName, $"{hostingMode}/IsReadOnly - [{staffLoginName}] has NO [DELETE] rights on database '{userRepositoryDatabaseName}'?");
					AssertDatabasePermissionMissing(adminConnection, "G", "REFERENCES", staffLoginName, $"{hostingMode}/IsReadOnly - [{staffLoginName}] has NO [REFERENCES] rights on database '{userRepositoryDatabaseName}'?");
					AssertDatabasePermissionMissing(adminConnection, "G", "ALTER SCHEMA", staffLoginName, $"{hostingMode}/IsReadOnly - [{staffLoginName}] has NO [ALTER SCHEMA] rights on database '{userRepositoryDatabaseName}'?");

					AssertDatabasePermissionExists(adminConnection, "G", "EXECUTE", staffLoginName, $"{hostingMode}/IsReadOnly - [{staffLoginName}] has [EXECUTE] rights on user repository DB??");
				}

				foreach (var dbName in adminConnection.GetDatabases(DatabaseType.All & ~DatabaseType.UserRepository & ~DatabaseType.SingleSharedRef))
				{
					using (((ICurrentDbControl)adminConnection).UseDatabase(dbName))
					{
						AssertDatabaseMembershipMissing(adminConnection, DbRoleTypes.DbDataWriterRole, staffLoginName, $"{hostingMode}/IsReadOnly has NO [datawriter] rights on database '{dbName}'?");
						AssertDatabaseMembershipExists(adminConnection, DbRoleTypes.CwRestrictedReaderRole, staffLoginName, $"{hostingMode} /IsReadOnly has [cwRestrictedReaderRole] rights on database '{dbName}'?");
						AssertDatabaseMembershipMissing(adminConnection, DbRoleTypes.DbBackupOperatorRole, staffLoginName, $"{hostingMode} /IsReadOnly has NO [backupoperator] rights on database '{dbName}'?");

						AssertDatabasePermissionMissing(adminConnection, "G", "CREATE TYPE", staffLoginName, $"{hostingMode}/IsReadOnly - [{staffLoginName}] has NO [CREATE TYPE] rights on database '{dbName}'?");
						AssertDatabasePermissionMissing(adminConnection, "G", "CREATE SCHEMA", staffLoginName, $"{hostingMode}/IsReadOnly - [{staffLoginName}] has NO [CREATE SCHEMA] rights on database '{dbName}'?");
						AssertDatabasePermissionMissing(adminConnection, "G", "CREATE TABLE", staffLoginName, $"{hostingMode}/IsReadOnly - [{staffLoginName}] has NO [CREATE TABLE] rights on database '{dbName}'?");
						AssertDatabasePermissionMissing(adminConnection, "G", "CREATE VIEW", staffLoginName, $"{hostingMode}/IsReadOnly - [{staffLoginName}] has NO [CREATE VIEW] rights on database '{dbName}'?");
						AssertDatabasePermissionMissing(adminConnection, "G", "CREATE FUNCTION", staffLoginName, $"{hostingMode}/IsReadOnly - [{staffLoginName}] has NO [CREATE FUNCTION] rights on database '{dbName}'?");
						AssertDatabasePermissionMissing(adminConnection, "G", "CREATE PROCEDURE", staffLoginName, $"{hostingMode}/IsReadOnly - [{staffLoginName}] has NO [CREATE PROCEDURE] rights on database '{dbName}'?");
						AssertDatabasePermissionMissing(adminConnection, "G", "INSERT", staffLoginName, $"{hostingMode}/IsReadOnly - [{staffLoginName}] has NO [INSERT] rights on database '{dbName}'?");
						AssertDatabasePermissionMissing(adminConnection, "G", "UPDATE", staffLoginName, $"{hostingMode}/IsReadOnly - [{staffLoginName}] has NO [UPDATE] rights on database '{dbName}'?");
						AssertDatabasePermissionMissing(adminConnection, "G", "DELETE", staffLoginName, $"{hostingMode}/IsReadOnly - [{staffLoginName}] has NO [DELETE] rights on database '{dbName}'?");
						AssertDatabasePermissionMissing(adminConnection, "G", "REFERENCES", staffLoginName, $"{hostingMode}/IsReadOnly - [{staffLoginName}] has NO [REFERENCES] rights on database '{dbName}'?");
						AssertDatabasePermissionMissing(adminConnection, "G", "ALTER SCHEMA", staffLoginName, $"{hostingMode}/IsReadOnly - [{staffLoginName}] has NO [ALTER SCHEMA] rights on database '{dbName}'?");
						AssertDatabasePermissionMissing(adminConnection, "G", "EXECUTE", staffLoginName, $"{hostingMode}/IsReadOnly - [{staffLoginName}] has NO [EXECUTE] rights on udatabase '{dbName}'?");
					}
				}
			}
			finally
			{
				Helper.DropServerPrincipals(adminConnection, staffLoginName);
			}
		}

		void CorrectUserRightsAreAddedToDatabaseBackupOperator(AdminConnection adminConnection, bool wiseCloudHosted, string hostingMode)
		{
			// Arrange
			EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = true;

			var userRepositoryDatabaseName = adminConnection.GetDatabases(DatabaseType.UserRepository).First();

			var factory = new BusinessObjectFactory(adminConnection);
			var staff = factory.New<GlbStaff>();

			staff.GS_FullName = databaseBackupOperatorLogin;
			staff.GS_LoginName = databaseBackupOperatorLogin;
			staff.GS_Code = "123";
			staff.StaffPlainTextPassword = "TOPSECRET";

			staff.IsReadOnlyDBUser = false;
			staff.IsDatabaseDeveloper = false;
			staff.IsBackupOperator = true;

			SetStaffPassword(staff);

			var staffLoginName = $"{DbUserRepository.StaffDbLoginPrefix}_{Db.DatabaseName}_{databaseBackupOperatorLogin}";

			try
			{
				factory.Save();

				var adminTask = new DbSecurityAdminTask();
				adminTask.ServiceLogger = Mock.Of<ILogger>();

				// Act
				adminTask.RunTask(CancellationToken.None);

				// Assert
				foreach (var dbName in adminConnection.GetDatabases(DatabaseType.All & ~DatabaseType.SingleSharedRef))
				{
					using (((ICurrentDbControl)adminConnection).UseDatabase(dbName))
					{
						AssertDatabaseMembershipMissing(adminConnection, DbRoleTypes.DbDataWriterRole, staffLoginName, $"{hostingMode}/IsBackupOperator has NO [datawriter] rights on database '{dbName}'?");
						AssertDatabaseMembershipMissing(adminConnection, DbRoleTypes.CwRestrictedReaderRole, staffLoginName, $"{hostingMode} /IsBackupOperator has NO [cwRestrictedReaderRole] rights on database '{dbName}'?");
						if (wiseCloudHosted)
						{
							AssertDatabaseMembershipMissing(adminConnection, DbRoleTypes.DbBackupOperatorRole, staffLoginName, $"{hostingMode}/IsBackupOperator - [{staffLoginName}] has NO [backupoperator] rights on database '{userRepositoryDatabaseName}'?");
						}
						else
						{
							AssertDatabaseMembershipExists(adminConnection, DbRoleTypes.DbBackupOperatorRole, staffLoginName, $"{hostingMode}/IsBackupOperator - [{staffLoginName}] has [backupoperator] rights on database '{userRepositoryDatabaseName}'?");
						}

						AssertDatabasePermissionMissing(adminConnection, "G", "CREATE TYPE", staffLoginName, $"{hostingMode}/IsBackupOperator - [{staffLoginName}] has NO [CREATE TYPE] rights on database '{dbName}'?");
						AssertDatabasePermissionMissing(adminConnection, "G", "CREATE SCHEMA", staffLoginName, $"{hostingMode}/IsBackupOperator - [{staffLoginName}] has NO [CREATE SCHEMA] rights on database '{dbName}'?");
						AssertDatabasePermissionMissing(adminConnection, "G", "CREATE TABLE", staffLoginName, $"{hostingMode}/IsBackupOperator - [{staffLoginName}] has NO [CREATE TABLE] rights on database '{dbName}'?");
						AssertDatabasePermissionMissing(adminConnection, "G", "CREATE VIEW", staffLoginName, $"{hostingMode}/IsBackupOperator - [{staffLoginName}] has NO [CREATE VIEW] rights on database '{dbName}'?");
						AssertDatabasePermissionMissing(adminConnection, "G", "CREATE FUNCTION", staffLoginName, $"{hostingMode}/IsBackupOperator - [{staffLoginName}] has NO [CREATE FUNCTION] rights on database '{dbName}'?");
						AssertDatabasePermissionMissing(adminConnection, "G", "CREATE PROCEDURE", staffLoginName, $"{hostingMode}/IsBackupOperator - [{staffLoginName}] has NO [CREATE PROCEDURE] rights on database '{dbName}'?");
						AssertDatabasePermissionMissing(adminConnection, "G", "INSERT", staffLoginName, $"{hostingMode}/IsBackupOperator - [{staffLoginName}] has NO [INSERT] rights on database '{dbName}'?");
						AssertDatabasePermissionMissing(adminConnection, "G", "UPDATE", staffLoginName, $"{hostingMode}/IsBackupOperator - [{staffLoginName}] has NO [UPDATE] rights on database '{dbName}'?");
						AssertDatabasePermissionMissing(adminConnection, "G", "DELETE", staffLoginName, $"{hostingMode}/IsBackupOperator - [{staffLoginName}] has NO [DELETE] rights on database '{dbName}'?");
						AssertDatabasePermissionMissing(adminConnection, "G", "REFERENCES", staffLoginName, $"{hostingMode}/IsBackupOperator - [{staffLoginName}] has NO [REFERENCES] rights on database '{dbName}'?");
						AssertDatabasePermissionMissing(adminConnection, "G", "ALTER SCHEMA", staffLoginName, $"{hostingMode}/IsBackupOperator - [{staffLoginName}] has NO [ALTER SCHEMA] rights on database '{dbName}'?");
						AssertDatabasePermissionMissing(adminConnection, "G", "EXECUTE", staffLoginName, $"{hostingMode}/IsBackupOperator - [{staffLoginName}] has NO [EXECUTE] rights on udatabase '{dbName}'?");
					}
				}
			}
			finally
			{
				Helper.DropServerPrincipals(adminConnection, staffLoginName);
			}
		}

		const string databaseDeveloperLogin = "staffDeveloper";
		const string databaseReaderLogin = "staffReader";
		const string databaseBackupOperatorLogin = "staffBackupOperator";

		#endregion Helper methods for tests from Tests from MasterFiles.Business.Test for ModernSecurity

		#endregion Tests from MasterFiles.Business.Test for ModernSecurity

		#region password hash tests

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.BI })]
		public void TestCanLoginWhenSqlPasswordIsSetAfterDSAHasRun()
		{
			// Arrange
			const string staffSqlLoginPassword = "pA$$w0rD!";

			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = true;

				var dbUserManager = new DbUserManager();

				var factory = new BusinessObjectFactory(adminConnection);
				var staffDeveloperAndReaderName = "TestDeveloperAndReaderGrant";
				var staffDeveloperAndReader = factory.New<GlbStaff>();

				staffDeveloperAndReader.GS_FullName = staffDeveloperAndReaderName;
				staffDeveloperAndReader.GS_LoginName = staffDeveloperAndReaderName;
				staffDeveloperAndReader.GS_Code = "12G";

				staffDeveloperAndReader.IsDatabaseDeveloper = true;
				staffDeveloperAndReader.IsReadOnlyDBUser = true;

				SetStaffPassword(staffDeveloperAndReader, staffSqlLoginPassword);

				var sqlStaffLoginDeveloperAndReader = $"{DbUserRepository.GetStaffDbLoginFullPrefix(Db.DatabaseName)}{staffDeveloperAndReader.GS_LoginName}";

				try
				{
					factory.Save();

					using (var connection = Db.NewExtraConnection(Db.ServerName, Db.DatabaseName, sqlStaffLoginDeveloperAndReader, staffSqlLoginPassword))
					{
						AssertExceptionThrown<SqlException>(() => connection.EnsureIsOpen());
					}

					var adminTask = new DbSecurityAdminTask();
					adminTask.ServiceLogger = Mock.Of<ILogger>();

					// Act

					adminTask.RunTask(CancellationToken.None);

					// Assert
					using (var connection = Db.NewExtraConnection(Db.ServerName, Db.DatabaseName, sqlStaffLoginDeveloperAndReader, staffSqlLoginPassword))
					{
						AssertNoExceptionThrown(() => connection.EnsureIsOpen());
					}
				}
				finally
				{
					Helper.DropServerPrincipals(adminConnection, sqlStaffLoginDeveloperAndReader);

					foreach (var dbName in adminConnection.GetDatabases(DatabaseType.All & ~DatabaseType.SingleSharedRef))
					{
						using (((ICurrentDbControl)adminConnection).UseDatabase(dbName))
						{
							Helper.DropDatabasePrincipals(adminConnection, sqlStaffLoginDeveloperAndReader);
						}
					}
				}
			}
		}

		[UseSnapshotProtection]
		public void TestWarningIsIssuedForStaffMembersThatShouldHaveSqlLoginButMissingPasswordHashInStaffRecord()
		{
			// Arrange
			var loggerMock = new Mock<ILogger>();

			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = true;
				ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = false;

				var dbUserManager = new DbUserManager();

				var factory = new BusinessObjectFactory(adminConnection);
				var staffDeveloperAndReaderName = "TestDeveloperAndReaderGrant";
				var staffDeveloperAndReader = factory.New<GlbStaff>();

				staffDeveloperAndReader.GS_FullName = staffDeveloperAndReaderName;
				staffDeveloperAndReader.GS_LoginName = staffDeveloperAndReaderName;
				staffDeveloperAndReader.GS_Code = "12G";
				staffDeveloperAndReader.StaffPlainTextPassword = "TOPSECRET";

				staffDeveloperAndReader.IsDatabaseDeveloper = true;
				staffDeveloperAndReader.IsReadOnlyDBUser = true;

				factory.Save();

				var sqlStaffLoginDeveloperAndReader = $"{DbUserRepository.GetStaffDbLoginFullPrefix(Db.DatabaseName)}{staffDeveloperAndReader.GS_LoginName}";

				adminConnection.ExecuteNonQuery(
					"UPDATE dbo.GlbStaff SET GS_SqlLoginPasswordHash = NULL, GS_SystemLastEditTimeUtc = GetUtcDate(), GS_SystemLastEditUser = 'E' WHERE GS_LoginName = @loginName",
					cmd => cmd.AddParameterBasedOnDbColumn("@loginName", staffDeveloperAndReader.GS_LoginName.ToString(), GlbStaffSchema.GS_LoginName));

				AssertEquals($"Precondition: Sql password hash should not be set for the staff member '{staffDeveloperAndReader.GS_LoginName}'.",
					System.DBNull.Value,
					adminConnection.ExecuteScalar(
						"SELECT GS_SqlLoginPasswordHash FROM dbo.GlbStaff WHERE GS_LoginName = @loginName",
						cmd => cmd.AddParameterBasedOnDbColumn("@loginName", staffDeveloperAndReader.GS_LoginName.ToString(), GlbStaffSchema.GS_LoginName)));

				var adminTask = new DbSecurityAdminTask();
				adminTask.ServiceLogger = loggerMock.Object;

				// Act
				adminTask.RunTask(CancellationToken.None);

				// Assert
				AssertServerPrincipalIsMissing(adminConnection, sqlStaffLoginDeveloperAndReader);
				loggerMock.Verify(logger => logger.Log(
				LogType.Warning,
					$"Skipped synchronising SQL login for staff record {staffDeveloperAndReaderName}. The record is missing its SQL login password hash value. Consider setting SQL password via the following CW1 menu item: Help > Set SQL password"));
			}
		}

		[UseSnapshotProtection]
		public void TestSqlLoginIsDroppedForStaffMembersThatShouldHaveSqlLoginButMissingPasswordHashInStaffRecord()
		{
			// Arrange
			var loggerMock = new Mock<ILogger>();

			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = true;
				ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = false;

				var dbUserManager = new DbUserManager();

				var factory = new BusinessObjectFactory(adminConnection);
				var staffDeveloperAndReaderName = "TestDeveloperAndReaderGrant";
				var staffDeveloperAndReader = factory.New<GlbStaff>();

				staffDeveloperAndReader.GS_FullName = staffDeveloperAndReaderName;
				staffDeveloperAndReader.GS_LoginName = staffDeveloperAndReaderName;
				staffDeveloperAndReader.GS_Code = "12G";

				staffDeveloperAndReader.IsDatabaseDeveloper = true;
				staffDeveloperAndReader.IsReadOnlyDBUser = true;

				SetStaffPassword(staffDeveloperAndReader);
				factory.Save();

				var sqlStaffLoginDeveloperAndReader = $"{DbUserRepository.GetStaffDbLoginFullPrefix(Db.DatabaseName)}{staffDeveloperAndReader.GS_LoginName.ToString()}";

				adminConnection.ExecuteNonQuery(
					"UPDATE dbo.GlbStaff SET GS_SqlLoginPasswordHash = NULL, GS_SystemLastEditTimeUtc = GetUtcDate(), GS_SystemLastEditUser = 'E' WHERE GS_LoginName = @loginName",
					cmd => cmd.AddParameterBasedOnDbColumn("@loginName", staffDeveloperAndReader.GS_LoginName.ToString(), GlbStaffSchema.GS_LoginName));

				AssertEquals($"Precondition: Sql password hash should not be set for the staff member '{staffDeveloperAndReader.GS_LoginName}'.",
					DBNull.Value,
					adminConnection.ExecuteScalar(
						"SELECT GS_SqlLoginPasswordHash FROM dbo.GlbStaff WHERE GS_LoginName = @loginName",
						cmd => cmd.AddParameterBasedOnDbColumn("@loginName", staffDeveloperAndReader.GS_LoginName.ToString(), GlbStaffSchema.GS_LoginName)));

				adminConnection.ExecuteNonQuery($@"
IF NOT EXISTS(SELECT name FROM sys.sql_logins WHERE name = N'{sqlStaffLoginDeveloperAndReader}')
	CREATE LOGIN {sqlStaffLoginDeveloperAndReader.QuoteName()} WITH PASSWORD = N'SOM123pASSWORD[]', DEFAULT_DATABASE = {Db.DatabaseName.QuoteName()}, DEFAULT_LANGUAGE = us_english, CHECK_EXPIRATION = OFF, CHECK_POLICY = OFF
");

				AssertServerPrincipalExistsAndIsNotDisabled(adminConnection, sqlStaffLoginDeveloperAndReader, "S");

				var adminTask = new DbSecurityAdminTask();
				adminTask.ServiceLogger = loggerMock.Object;

				// Act
				adminTask.RunTask(CancellationToken.None);

				// Assert
				AssertServerPrincipalIsMissing(adminConnection, sqlStaffLoginDeveloperAndReader);
				loggerMock.Verify(logger => logger.Log(
					LogType.Warning,
					$"Skipped synchronising SQL login for staff record {staffDeveloperAndReaderName}. The record is missing its SQL login password hash value. Consider setting SQL password via the following CW1 menu item: Help > Set SQL password"));
			}
		}

		#endregion password hash tests

		#region Implementation

		readonly string dbReaderLogin = CargoWiseReaderLoginCredentials.UserNameFor(Db.DatabaseName);
		readonly string dbWriterLogin = CargoWiseWriterLoginCredentials.UserNameFor(Db.DatabaseName);
		readonly string dbRestrictedReaderLogin = RestrictedReaderLoginCredentials.UserNameFor(Db.DatabaseName);
		readonly string dbRestrictedWriterLogin = RestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName);
		readonly string dbUnrestrictedWriterLogin = UnrestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName);

		readonly BusinessObjectFactory factory;

		#region Triggers tests helpers

		void TestSaPasswordReset(AdminConnection adminConnection, bool useModernSecuritySystem, bool expectPasswordReset)
		{
			// Arrange
			EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = useModernSecuritySystem;

			var originalPasswordHash = adminConnection.ExecuteScalar<byte[]>("SELECT password_hash FROM sys.sql_logins WHERE name = N'sa'");
			var originalCheckPolicy = adminConnection.ExecuteScalar<bool>("SELECT is_policy_checked FROM sys.sql_logins WHERE name = N'sa'");
			var originalExpirationCheck = adminConnection.ExecuteScalar<bool>("SELECT is_expiration_checked FROM sys.sql_logins WHERE name = N'sa'");

			try
			{
				adminConnection.ExecuteNonQuery("ALTER LOGIN [sa] WITH PASSWORD = N'someRandomop123[]{}{Password';");

				var adminTask = new DbSecurityAdminTask();
				adminTask.ServiceLogger = Mock.Of<ILogger>();

				// Act
				adminTask.RunTask(CancellationToken.None);

				// Assert
				AssertEquals(adminConnection.ExecuteScalar(
					"SELECT PWDCOMPARE(@password, password_hash) FROM sys.sql_logins WHERE name = N'sa'",
					command => command.AddParameter("@password", SqlDbType.NVarChar, 128, expectPasswordReset ? Db.SaValue : "someRandomop123[]{}{Password")), 1);
			}
			finally
			{
				adminConnection.ExecuteNonQuery(@$"
ALTER LOGIN [sa] WITH CHECK_POLICY = OFF, CHECK_EXPIRATION = OFF;
ALTER LOGIN [sa] WITH PASSWORD = {DataUtils.BytesToHexString(originalPasswordHash)} HASHED;
ALTER LOGIN [sa] WITH CHECK_POLICY = {(originalCheckPolicy ? "ON" : "OFF")}, CHECK_EXPIRATION = {(originalExpirationCheck ? "ON" : "OFF")};
");
			}
		}

		void TestDdlTriggersDisabling(AdminConnection adminConnection, bool useModernSecuritySystem, bool expectTriggerDisabled)
		{
			// Arrange
			EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = useModernSecuritySystem;

			adminConnection.ExecuteNonQuery("DROP TRIGGER IF EXISTS _Tst_Random_Trigger_Name ON ALL SERVER");

			adminConnection.ExecuteNonQuery("CREATE TRIGGER _Tst_Random_Trigger_Name ON ALL SERVER FOR CREATE_DATABASE AS PRINT 'Database Created.'");
			AssertEquals(adminConnection.ExecuteScalar<bool>("SELECT is_disabled FROM sys.server_triggers WHERE name = N'_Tst_Random_Trigger_Name'"), false);

			var adminTask = new DbSecurityAdminTask();
			adminTask.ServiceLogger = Mock.Of<ILogger>();

			// Act
			adminTask.RunTask(CancellationToken.None);

			// Assert
			AssertEquals(adminConnection.ExecuteScalar<bool>("SELECT is_disabled FROM sys.server_triggers WHERE name = N'_Tst_Random_Trigger_Name'"), expectTriggerDisabled);
		}

		void TestSqlAgentJobsDisabling(AdminConnection adminConnection, bool useModernSecuritySystem, bool expectAgentJobDisabled)
		{
			// Arrange
			EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = useModernSecuritySystem;

			using (ChangeDatabase(adminConnection, Db.SqlMsdb))
			{
				adminConnection.ExecuteNonQuery(@"
IF EXISTS(SELECT name FROM dbo.sysjobs WHERE name = N'_Tst_Random_Agent_Job')
	EXEC sp_delete_job @job_name = N'_Tst_Random_Agent_Job' ; 
");

				adminConnection.ExecuteNonQuery("EXEC sp_add_job @job_name = N'_Tst_Random_Agent_Job'");
				AssertEquals(adminConnection.ExecuteScalar<byte>("SELECT enabled FROM dbo.sysjobs WHERE name = N'_Tst_Random_Agent_Job'"), (byte)1);
			}

			var adminTask = new DbSecurityAdminTask();
			adminTask.ServiceLogger = Mock.Of<ILogger>();

			// Act
			adminTask.RunTask(CancellationToken.None);

			// Assert
			using (ChangeDatabase(adminConnection, Db.SqlMsdb))
			{
				AssertEquals(adminConnection.ExecuteScalar<byte>("SELECT enabled FROM dbo.sysjobs WHERE name = N'_Tst_Random_Agent_Job'"), expectAgentJobDisabled ? (byte)0 : (byte)1);
			}
		}

		#endregion Triggers tests helpers

		#region Principals synchronisation helpers

		void SetStaffPassword(GlbStaff staff, string password = null)
		{
			var dbUserManager = new DbUserManager();
			dbUserManager.SetPasswordForStaff(staff, string.IsNullOrEmpty(password) ? "someRandom123Password[]" : password);
		}

		void DropsUnexpectedGrantPermissionsForApplicationUsers(IEnumerable<string> databases, AdminConnection adminConnection)
		{
			// Arrange
			foreach (var dbName in databases)
			{
				using (ChangeDatabase(adminConnection, dbName))
				{
					adminConnection.ExecuteNonQuery($@"
GRANT ALTER ANY APPLICATION ROLE TO {dbUnrestrictedWriterLogin.QuoteName()};
GRANT CONTROL ON SCHEMA::dbo TO {dbWriterLogin.QuoteName()} WITH GRANT OPTION;
");
					AssertDatabasePermissionExists(adminConnection, "G", "ALTER ANY APPLICATION ROLE", dbUnrestrictedWriterLogin);
					AssertSchemaPermissionExists(adminConnection, "W", "CONTROL", "dbo", dbWriterLogin);
				}
			}

			var adminTask = new DbSecurityAdminTask();
			adminTask.ServiceLogger = Mock.Of<ILogger>();

			// Act
			adminTask.RunTask(CancellationToken.None);

			// Assert
			foreach (var dbName in databases)
			{
				using (ChangeDatabase(adminConnection, dbName))
				{
					AssertDatabasePermissionMissing(adminConnection, "G", "ALTER ANY APPLICATION ROLE", dbUnrestrictedWriterLogin);
					AssertSchemaPermissionMissing(adminConnection, "dbo", "G", "CONTROL", dbWriterLogin);
				}
			}
		}

		void DoesNotDropUnexpectedGrantPermissionsForApplicationUsers(IEnumerable<string> databases, AdminConnection adminConnection)
		{
			// Arrange
			foreach (var dbName in databases)
			{
				using (ChangeDatabase(adminConnection, dbName))
				{
					adminConnection.ExecuteNonQuery($@"
GRANT ALTER ANY APPLICATION ROLE TO {dbUnrestrictedWriterLogin.QuoteName()};
GRANT CONTROL ON SCHEMA::dbo TO {dbWriterLogin.QuoteName()} WITH GRANT OPTION;
");
					AssertDatabasePermissionExists(adminConnection, "G", "ALTER ANY APPLICATION ROLE", dbUnrestrictedWriterLogin);
					AssertSchemaPermissionExists(adminConnection, "W", "CONTROL", "dbo", dbWriterLogin);
				}
			}

			var adminTask = new DbSecurityAdminTask();
			adminTask.ServiceLogger = Mock.Of<ILogger>();

			// Act
			adminTask.RunTask(CancellationToken.None);

			// Assert
			foreach (var dbName in databases)
			{
				using (ChangeDatabase(adminConnection, dbName))
				{
					AssertDatabasePermissionExists(adminConnection, "G", "ALTER ANY APPLICATION ROLE", dbUnrestrictedWriterLogin);
					AssertSchemaPermissionExists(adminConnection, "W", "CONTROL", "dbo", dbWriterLogin);
				}
			}
		}

		void DropsUnexpectedDenyPermissionsForApplicationUsers(IEnumerable<string> databases, AdminConnection adminConnection)
		{
			// Arrange
			foreach (var dbName in databases)
			{
				using (ChangeDatabase(adminConnection, dbName))
				{
					adminConnection.ExecuteNonQuery($@"
DENY CONTROL ON SCHEMA::dbo TO {dbWriterLogin.QuoteName()};
DENY UPDATE TO {dbRestrictedReaderLogin.QuoteName()};
");
					AssertSchemaPermissionExists(adminConnection, "D", "CONTROL", "dbo", dbWriterLogin);
					AssertDatabasePermissionExists(adminConnection, "D", "UPDATE", dbRestrictedReaderLogin);
				}
			}

			var adminTask = new DbSecurityAdminTask();
			adminTask.ServiceLogger = Mock.Of<ILogger>();

			// Act
			adminTask.RunTask(CancellationToken.None);

			// Assert
			foreach (var dbName in databases)
			{
				using (ChangeDatabase(adminConnection, dbName))
				{
					AssertSchemaPermissionMissing(adminConnection, "dbo", "D", "CONTROL", dbWriterLogin);
					AssertDatabasePermissionMissing(adminConnection, dbRestrictedReaderLogin, "D", "UPDATE");
				}
			}
		}

		void DoesNotDropUnexpectedDenyPermissionsForApplicationUsers(IEnumerable<string> databases, AdminConnection adminConnection)
		{
			// Arrange
			foreach (var dbName in databases)
			{
				using (ChangeDatabase(adminConnection, dbName))
				{
					adminConnection.ExecuteNonQuery($@"
DENY CONTROL ON SCHEMA::dbo TO {dbWriterLogin.QuoteName()} CASCADE;
DENY UPDATE TO {dbRestrictedReaderLogin.QuoteName()} CASCADE;
");
					AssertSchemaPermissionExists(adminConnection, "D", "CONTROL", "dbo", dbWriterLogin);
					AssertDatabasePermissionExists(adminConnection, "D", "UPDATE", dbRestrictedReaderLogin);
				}
			}

			var adminTask = new DbSecurityAdminTask();
			adminTask.ServiceLogger = Mock.Of<ILogger>();

			// Act
			adminTask.RunTask(CancellationToken.None);

			// Assert
			foreach (var dbName in databases)
			{
				using (ChangeDatabase(adminConnection, dbName))
				{
					AssertSchemaPermissionExists(adminConnection, "D", "CONTROL", "dbo", dbWriterLogin);
					AssertDatabasePermissionExists(adminConnection, "D", "UPDATE", dbRestrictedReaderLogin);
				}
			}
		}

		void DropsUnexpectedGrantPermissionsForStaffUsers(IEnumerable<string> databases, AdminConnection adminConnection)
		{
			// Arrange
			var sqlSecurityManager = new SqlSecurityManager(Mock.Of<ILogger>(), Db.DatabaseName, allowTransaction: false);

			var factory = new BusinessObjectFactory(adminConnection);
			var staffDeveloperAndReaderName = "TestDeveloperAndReaderGrant";
			var staffDeveloperAndReader = factory.New<GlbStaff>();

			staffDeveloperAndReader.GS_FullName = staffDeveloperAndReaderName;
			staffDeveloperAndReader.GS_LoginName = staffDeveloperAndReaderName;
			staffDeveloperAndReader.GS_Code = "12G";
			staffDeveloperAndReader.StaffPlainTextPassword = "TOPSECRET";

			staffDeveloperAndReader.IsDatabaseDeveloper = true;
			staffDeveloperAndReader.IsReadOnlyDBUser = true;

			SetStaffPassword(staffDeveloperAndReader);

			var sqlStaffLoginDeveloperAndReader = $"{DbUserRepository.GetStaffDbLoginFullPrefix(Db.DatabaseName)}{staffDeveloperAndReader.GS_LoginName}";

			try
			{
				factory.Save();

				sqlSecurityManager.BuildServerSecurity(adminConnection, trialRun: false);

				foreach (var dbName in databases)
				{
					using (ChangeDatabase(adminConnection, dbName))
					{
						sqlSecurityManager.BuildDatabaseSecurity(adminConnection, dbName, trialRun: false);

						adminConnection.ExecuteNonQuery($@"
GRANT ALTER ANY APPLICATION ROLE TO {sqlStaffLoginDeveloperAndReader.QuoteName()};
GRANT CONTROL ON SCHEMA::dbo TO {sqlStaffLoginDeveloperAndReader.QuoteName()} WITH GRANT OPTION;
");
						AssertDatabasePermissionExists(adminConnection, "G", "ALTER ANY APPLICATION ROLE", sqlStaffLoginDeveloperAndReader);
						AssertSchemaPermissionExists(adminConnection, "W", "CONTROL", "dbo", sqlStaffLoginDeveloperAndReader);
					}
				}

				var adminTask = new DbSecurityAdminTask();
				adminTask.ServiceLogger = Mock.Of<ILogger>();

				// Act
				adminTask.RunTask(CancellationToken.None);

				// Assert
				foreach (var dbName in databases)
				{
					using (ChangeDatabase(adminConnection, dbName))
					{
						AssertDatabasePermissionMissing(adminConnection, sqlStaffLoginDeveloperAndReader, "G", "ALTER ANY APPLICATION ROLE");
						AssertSchemaPermissionMissing(adminConnection, "dbo", "W", "CONTROL", sqlStaffLoginDeveloperAndReader);
					}
				}
			}
			finally
			{
				Helper.DropServerPrincipals(adminConnection, sqlStaffLoginDeveloperAndReader);
			}
		}

		void DoesNotDropUnexpectedGrantPermissionsForStaffUsers(IEnumerable<string> databases, AdminConnection adminConnection)
		{
			// Arrange
			var factory = new BusinessObjectFactory(adminConnection);
			var staffDeveloperAndReaderName = "TestDeveloperAndReaderGrantNotDropped";
			var staffDeveloperAndReader = factory.New<GlbStaff>();

			staffDeveloperAndReader.GS_FullName = staffDeveloperAndReaderName;
			staffDeveloperAndReader.GS_LoginName = staffDeveloperAndReaderName;
			staffDeveloperAndReader.GS_Code = "1DG";
			staffDeveloperAndReader.StaffPlainTextPassword = "TOPSECRET";

			staffDeveloperAndReader.IsDatabaseDeveloper = true;
			staffDeveloperAndReader.IsReadOnlyDBUser = true;

			SetStaffPassword(staffDeveloperAndReader);

			var sqlStaffLoginDeveloperAndReader = $"{DbUserRepository.StaffDbLoginPrefix}_{Db.DatabaseName}_{staffDeveloperAndReader.GS_LoginName}";

			try
			{
				factory.Save();

				foreach (var dbName in databases)
				{
					using (ChangeDatabase(adminConnection, dbName))
					{
						adminConnection.ExecuteNonQuery($@"
GRANT ALTER ANY APPLICATION ROLE TO {sqlStaffLoginDeveloperAndReader.QuoteName()};
GRANT CONTROL ON SCHEMA::dbo TO {sqlStaffLoginDeveloperAndReader.QuoteName()} WITH GRANT OPTION;
");
						AssertDatabasePermissionExists(adminConnection, "G", "ALTER ANY APPLICATION ROLE", sqlStaffLoginDeveloperAndReader);
						AssertSchemaPermissionExists(adminConnection, "W", "CONTROL", "dbo", sqlStaffLoginDeveloperAndReader);
					}
				}

				var adminTask = new DbSecurityAdminTask();
				adminTask.ServiceLogger = Mock.Of<ILogger>();

				// Act

				adminTask.RunTask(CancellationToken.None);

				// Assert
				foreach (var dbName in databases)
				{
					using (ChangeDatabase(adminConnection, dbName))
					{
						AssertDatabasePermissionExists(adminConnection, "G", "ALTER ANY APPLICATION ROLE", sqlStaffLoginDeveloperAndReader);
						AssertSchemaPermissionExists(adminConnection, "W", "CONTROL", "dbo", sqlStaffLoginDeveloperAndReader);
					}
				}
			}
			finally
			{
				Helper.DropServerPrincipals(adminConnection, sqlStaffLoginDeveloperAndReader);
			}
		}

		void DropsUnexpectedDenyPermissionsForStaffUsers(IEnumerable<string> databases, AdminConnection adminConnection)
		{
			// Arrange
			var sqlSecurityManager = new SqlSecurityManager(Mock.Of<ILogger>(), Db.DatabaseName, allowTransaction: false);

			var factory = new BusinessObjectFactory(adminConnection);
			var staffDeveloperAndReaderName = "TestDeveloperAndReaderDeny";
			var staffDeveloperAndReader = factory.New<GlbStaff>();

			staffDeveloperAndReader.GS_FullName = staffDeveloperAndReaderName;
			staffDeveloperAndReader.GS_LoginName = staffDeveloperAndReaderName;
			staffDeveloperAndReader.GS_Code = "12D";
			staffDeveloperAndReader.StaffPlainTextPassword = "TOPSECRET";

			staffDeveloperAndReader.IsDatabaseDeveloper = true;
			staffDeveloperAndReader.IsReadOnlyDBUser = true;

			SetStaffPassword(staffDeveloperAndReader);
			var sqlStaffLoginDeveloperAndReader = $"{DbUserRepository.GetStaffDbLoginFullPrefix(Db.DatabaseName)}{staffDeveloperAndReader.GS_LoginName}";

			try
			{
				factory.Save();

				sqlSecurityManager.BuildServerSecurity(adminConnection, trialRun: false);

				foreach (var dbName in databases)
				{
					using (ChangeDatabase(adminConnection, dbName))
					{
						sqlSecurityManager.BuildDatabaseSecurity(adminConnection, dbName, trialRun: false);

						adminConnection.ExecuteNonQuery($@"
DENY CONTROL ON SCHEMA::dbo TO {sqlStaffLoginDeveloperAndReader.QuoteName()};
DENY UPDATE TO {sqlStaffLoginDeveloperAndReader.QuoteName()};
");
						AssertSchemaPermissionExists(adminConnection, "D", "CONTROL", "dbo", sqlStaffLoginDeveloperAndReader);
						AssertDatabasePermissionExists(adminConnection, "D", "UPDATE", sqlStaffLoginDeveloperAndReader);
					}
				}

				var adminTask = new DbSecurityAdminTask();
				adminTask.ServiceLogger = Mock.Of<ILogger>();

				// Act
				adminTask.RunTask(CancellationToken.None);

				// Assert
				foreach (var dbName in databases)
				{
					using (ChangeDatabase(adminConnection, dbName))
					{
						AssertSchemaPermissionMissing(adminConnection, "dbo", "D", "CONTROL", sqlStaffLoginDeveloperAndReader);
						AssertDatabasePermissionMissing(adminConnection, sqlStaffLoginDeveloperAndReader, "D", "UPDATE");
					}
				}
			}
			finally
			{
				Helper.DropServerPrincipals(adminConnection, sqlStaffLoginDeveloperAndReader);
			}
		}

		void DoesNotDropUnexpectedDenyPermissionsForStaffUsers(IEnumerable<string> databases, AdminConnection adminConnection)
		{
			// Arrange
			var factory = new BusinessObjectFactory(adminConnection);
			var staffDeveloperAndReaderName = "TestDeveloperAndReaderDeny";
			var staffDeveloperAndReader = factory.New<GlbStaff>();

			staffDeveloperAndReader.GS_FullName = staffDeveloperAndReaderName;
			staffDeveloperAndReader.GS_LoginName = staffDeveloperAndReaderName;
			staffDeveloperAndReader.GS_Code = "12D";
			staffDeveloperAndReader.StaffPlainTextPassword = "TOPSECRET";

			staffDeveloperAndReader.IsDatabaseDeveloper = true;
			staffDeveloperAndReader.IsReadOnlyDBUser = true;

			SetStaffPassword(staffDeveloperAndReader);

			var sqlStaffLoginDeveloperAndReader = $"{DbUserRepository.StaffDbLoginPrefix}_{Db.DatabaseName}_{staffDeveloperAndReader.GS_LoginName}";

			try
			{
				factory.Save();

				foreach (var dbName in databases)
				{
					using (ChangeDatabase(adminConnection, dbName))
					{
						adminConnection.ExecuteNonQuery($@"
DENY CONTROL ON SCHEMA::dbo TO {sqlStaffLoginDeveloperAndReader.QuoteName()};
DENY UPDATE TO {sqlStaffLoginDeveloperAndReader.QuoteName()};
");
						AssertSchemaPermissionExists(adminConnection, "D", "CONTROL", "dbo", sqlStaffLoginDeveloperAndReader);
						AssertDatabasePermissionExists(adminConnection, "D", "UPDATE", sqlStaffLoginDeveloperAndReader);
					}
				}

				var adminTask = new DbSecurityAdminTask();
				adminTask.ServiceLogger = Mock.Of<ILogger>();

				// Act
				adminTask.RunTask(CancellationToken.None);

				// Assert
				foreach (var dbName in databases)
				{
					using (ChangeDatabase(adminConnection, dbName))
					{
						AssertSchemaPermissionExists(adminConnection, "D", "CONTROL", "dbo", sqlStaffLoginDeveloperAndReader);
						AssertDatabasePermissionExists(adminConnection, "D", "UPDATE", sqlStaffLoginDeveloperAndReader);
					}
				}
			}
			finally
			{
				Helper.DropServerPrincipals(adminConnection, sqlStaffLoginDeveloperAndReader);
			}
		}

		void DropsUnexpectedPermissionsForApplicationRoles(IEnumerable<string> databases, AdminConnection adminConnection)
		{
			// Arrange
			foreach (var dbName in databases)
			{
				using (ChangeDatabase(adminConnection, dbName))
				{
					adminConnection.ExecuteNonQuery($@"
GRANT ALTER ANY APPLICATION ROLE TO {nameof(CwUnrestrictedWriterRole).QuoteName()};
GRANT CONTROL ON SCHEMA::dbo TO {nameof(CwUnrestrictedWriterRole).QuoteName()} WITH GRANT OPTION;
DENY UPDATE TO {nameof(CwUnrestrictedWriterRole).QuoteName()};
");
					AssertDatabasePermissionExists(adminConnection, "G", "ALTER ANY APPLICATION ROLE", nameof(CwUnrestrictedWriterRole));
					AssertSchemaPermissionExists(adminConnection, "W", "CONTROL", "dbo", nameof(CwUnrestrictedWriterRole));
					AssertDatabasePermissionExists(adminConnection, "D", "UPDATE", nameof(CwUnrestrictedWriterRole));
				}
			}

			var adminTask = new DbSecurityAdminTask();
			adminTask.ServiceLogger = Mock.Of<ILogger>();

			// Act
			adminTask.RunTask(CancellationToken.None);

			// Assert
			foreach (var dbName in databases)
			{
				using (ChangeDatabase(adminConnection, dbName))
				{
					AssertDatabasePermissionMissing(adminConnection, nameof(CwUnrestrictedWriterRole), "G", "ALTER ANY APPLICATION ROLE");
					AssertSchemaPermissionMissing(adminConnection, "dbo", "W", "CONTROL", nameof(CwUnrestrictedWriterRole));
					AssertDatabasePermissionMissing(adminConnection, nameof(CwUnrestrictedWriterRole), "D", "UPDATE");
				}
			}
		}

		void DoesNotDropUnexpectedPermissionsForDbRoles(IEnumerable<string> databases, AdminConnection adminConnection)
		{
			// Arrange
			foreach (var dbName in databases)
			{
				using (ChangeDatabase(adminConnection, dbName))
				{
					adminConnection.ExecuteNonQuery($@"
GRANT ALTER ANY APPLICATION ROLE TO {nameof(CwUnrestrictedWriterRole).QuoteName()};
GRANT CONTROL ON SCHEMA::dbo TO {nameof(CwRestrictedWriterRole).QuoteName()} WITH GRANT OPTION;
DENY UPDATE TO {nameof(CwRestrictedReaderRole).QuoteName()};
");
					AssertDatabasePermissionExists(adminConnection, "G", "ALTER ANY APPLICATION ROLE", nameof(CwUnrestrictedWriterRole));
					AssertSchemaPermissionExists(adminConnection, "W", "CONTROL", "dbo", nameof(CwRestrictedWriterRole));
					AssertDatabasePermissionExists(adminConnection, "D", "UPDATE", nameof(CwRestrictedReaderRole));
				}
			}

			var adminTask = new DbSecurityAdminTask();
			adminTask.ServiceLogger = Mock.Of<ILogger>();

			// Act
			adminTask.RunTask(CancellationToken.None);

			// Assert
			foreach (var dbName in databases)
			{
				using (ChangeDatabase(adminConnection, dbName))
				{
					AssertDatabasePermissionExists(adminConnection, "G", "ALTER ANY APPLICATION ROLE", nameof(CwUnrestrictedWriterRole));
					AssertSchemaPermissionExists(adminConnection, "W", "CONTROL", "dbo", nameof(CwRestrictedWriterRole));
					AssertDatabasePermissionExists(adminConnection, "D", "UPDATE", nameof(CwRestrictedReaderRole));
				}
			}
		}

		void DropsUnexpectedGrantPermissionsForLogins(AdminConnection adminConnection)
		{
			// Arrange
			var sqlSecurityManager = new SqlSecurityManager(Mock.Of<ILogger>(), Db.DatabaseName, allowTransaction: false);
			var factory = new BusinessObjectFactory(adminConnection);
			var staffDeveloperAndReaderName = "TestDeveloperAndReaderGrant";
			var staffDeveloperAndReader = factory.New<GlbStaff>();

			staffDeveloperAndReader.GS_FullName = staffDeveloperAndReaderName;
			staffDeveloperAndReader.GS_LoginName = staffDeveloperAndReaderName;
			staffDeveloperAndReader.GS_Code = "12G";
			staffDeveloperAndReader.StaffPlainTextPassword = "TOPSECRET";

			staffDeveloperAndReader.IsDatabaseDeveloper = true;
			staffDeveloperAndReader.IsReadOnlyDBUser = true;

			SetStaffPassword(staffDeveloperAndReader);

			var sqlStaffLoginDeveloperAndReader = $"{DbUserRepository.GetStaffDbLoginFullPrefix(Db.DatabaseName)}{staffDeveloperAndReader.GS_LoginName}";

			try
			{
				factory.Save();
				sqlSecurityManager.BuildServerSecurity(adminConnection, trialRun: false);

				using (ChangeDatabase(adminConnection, Db.SqlMasterDb))
				{
					adminConnection.ExecuteNonQuery($@"
GRANT CONNECT ANY DATABASE TO {sqlStaffLoginDeveloperAndReader.QuoteName()} WITH GRANT OPTION;
GRANT CONTROL SERVER TO {dbReaderLogin.QuoteName()};
");
				}

				AssertServerPermissionExists(adminConnection, "W", "CONNECT ANY DATABASE", sqlStaffLoginDeveloperAndReader);
				AssertServerPermissionExists(adminConnection, "G", "CONTROL SERVER", dbReaderLogin);

				var adminTask = new DbSecurityAdminTask();
				adminTask.ServiceLogger = Mock.Of<ILogger>();

				// Act
				adminTask.RunTask(CancellationToken.None);

				// Assert
				AssertServerPermissionMissing(adminConnection, "W", "CONNECT ANY DATABASE", sqlStaffLoginDeveloperAndReader);
				AssertServerPermissionMissing(adminConnection, "G", "CONTROL SERVER", dbReaderLogin);
			}
			finally
			{
				Helper.DropServerPrincipals(adminConnection, sqlStaffLoginDeveloperAndReader);
			}
		}

		void DoesNotDropUnexpectedGrantPermissionsForLogins(AdminConnection adminConnection)
		{
			// Arrange
			var factory = new BusinessObjectFactory(adminConnection);
			var staffDeveloperAndReaderName = "TestDeveloperAndReaderGrant";
			var staffDeveloperAndReader = factory.New<GlbStaff>();

			staffDeveloperAndReader.GS_FullName = staffDeveloperAndReaderName;
			staffDeveloperAndReader.GS_LoginName = staffDeveloperAndReaderName;
			staffDeveloperAndReader.GS_Code = "12G";
			staffDeveloperAndReader.StaffPlainTextPassword = "TOPSECRET";

			staffDeveloperAndReader.IsDatabaseDeveloper = true;
			staffDeveloperAndReader.IsReadOnlyDBUser = true;

			SetStaffPassword(staffDeveloperAndReader);

			var sqlStaffLoginDeveloperAndReader = $"{DbUserRepository.StaffDbLoginPrefix}_{Db.DatabaseName}_{staffDeveloperAndReader.GS_LoginName}";

			try
			{
				factory.Save();

				using (ChangeDatabase(adminConnection, Db.SqlMasterDb))
				{
					adminConnection.ExecuteNonQuery($@"
GRANT CONNECT ANY DATABASE TO {sqlStaffLoginDeveloperAndReader.QuoteName()} WITH GRANT OPTION;
GRANT CONTROL SERVER TO {dbReaderLogin.QuoteName()};
");
				}

				AssertServerPermissionExists(adminConnection, "W", "CONNECT ANY DATABASE", sqlStaffLoginDeveloperAndReader);
				AssertServerPermissionExists(adminConnection, "G", "CONTROL SERVER", dbReaderLogin);

				var adminTask = new DbSecurityAdminTask();
				adminTask.ServiceLogger = Mock.Of<ILogger>();

				// Act
				adminTask.RunTask(CancellationToken.None);

				// Assert
				AssertServerPermissionExists(adminConnection, "W", "CONNECT ANY DATABASE", sqlStaffLoginDeveloperAndReader);
				AssertServerPermissionExists(adminConnection, "G", "CONTROL SERVER", dbReaderLogin);
			}
			finally
			{
				Helper.DropServerPrincipals(adminConnection, sqlStaffLoginDeveloperAndReader);
			}
		}

		void DropsUnexpectedDenyPermissionsForLogins(AdminConnection adminConnection)
		{
			// Arrange
			var sqlSecurityManager = new SqlSecurityManager(Mock.Of<ILogger>(), Db.DatabaseName, allowTransaction: false);

			var factory = new BusinessObjectFactory(adminConnection);
			var staffDeveloperAndReaderName = "TestDeveloperAndReaderDeny";
			var staffDeveloperAndReader = factory.New<GlbStaff>();

			staffDeveloperAndReader.GS_FullName = staffDeveloperAndReaderName;
			staffDeveloperAndReader.GS_LoginName = staffDeveloperAndReaderName;
			staffDeveloperAndReader.GS_Code = "12D";
			staffDeveloperAndReader.StaffPlainTextPassword = "TOPSECRET";

			staffDeveloperAndReader.IsDatabaseDeveloper = true;
			staffDeveloperAndReader.IsReadOnlyDBUser = true;

			SetStaffPassword(staffDeveloperAndReader);

			var sqlStaffLoginDeveloperAndReader = $"{DbUserRepository.GetStaffDbLoginFullPrefix(Db.DatabaseName)}{staffDeveloperAndReader.GS_LoginName}";

			try
			{
				factory.Save();

				sqlSecurityManager.BuildServerSecurity(adminConnection, trialRun: false);

				using (ChangeDatabase(adminConnection, Db.SqlMasterDb))
				{
					adminConnection.ExecuteNonQuery($@"
DENY CONNECT ANY DATABASE TO {sqlStaffLoginDeveloperAndReader.QuoteName()};
DENY CONTROL SERVER TO {dbReaderLogin.QuoteName()};
");
				}

				AssertServerPermissionExists(adminConnection, "D", "CONNECT ANY DATABASE", sqlStaffLoginDeveloperAndReader);
				AssertServerPermissionExists(adminConnection, "D", "CONTROL SERVER", dbReaderLogin);

				var adminTask = new DbSecurityAdminTask();
				adminTask.ServiceLogger = Mock.Of<ILogger>();

				// Act
				adminTask.RunTask(CancellationToken.None);

				// Assert
				AssertServerPermissionMissing(adminConnection, "D", "CONNECT ANY DATABASE", sqlStaffLoginDeveloperAndReader);
				AssertServerPermissionMissing(adminConnection, "D", "CONTROL SERVER", dbReaderLogin);
			}
			finally
			{
				Helper.DropServerPrincipals(adminConnection, sqlStaffLoginDeveloperAndReader);
			}
		}

		void DoesNotDropUnexpectedDenyPermissionsForLogins(AdminConnection adminConnection)
		{
			// Arrange

			var factory = new BusinessObjectFactory(adminConnection);
			var staffDeveloperAndReaderName = "TestDeveloperAndReaderDeny";
			var staffDeveloperAndReader = factory.New<GlbStaff>();

			staffDeveloperAndReader.GS_FullName = staffDeveloperAndReaderName;
			staffDeveloperAndReader.GS_LoginName = staffDeveloperAndReaderName;
			staffDeveloperAndReader.GS_Code = "123";
			staffDeveloperAndReader.StaffPlainTextPassword = "TOPSECRET";

			staffDeveloperAndReader.IsDatabaseDeveloper = true;
			staffDeveloperAndReader.IsReadOnlyDBUser = true;

			SetStaffPassword(staffDeveloperAndReader);

			var sqlStaffLoginDeveloperAndReader = $"{DbUserRepository.StaffDbLoginPrefix}_{Db.DatabaseName}_{staffDeveloperAndReader.GS_LoginName}";

			try
			{
				factory.Save();

				using (ChangeDatabase(adminConnection, Db.SqlMasterDb))
				{
					adminConnection.ExecuteNonQuery($@"
DENY CONNECT ANY DATABASE TO {sqlStaffLoginDeveloperAndReader.QuoteName()};
DENY CONTROL SERVER TO {dbReaderLogin.QuoteName()};
");
				}

				AssertServerPermissionExists(adminConnection, "D", "CONNECT ANY DATABASE", sqlStaffLoginDeveloperAndReader);
				AssertServerPermissionExists(adminConnection, "D", "CONTROL SERVER", dbReaderLogin);

				var adminTask = new DbSecurityAdminTask();
				adminTask.ServiceLogger = Mock.Of<ILogger>();

				// Act
				adminTask.RunTask(CancellationToken.None);

				// Assert
				AssertServerPermissionExists(adminConnection, "D", "CONNECT ANY DATABASE", sqlStaffLoginDeveloperAndReader);
				AssertServerPermissionExists(adminConnection, "D", "CONTROL SERVER", dbReaderLogin);
			}
			finally
			{
				Helper.DropServerPrincipals(adminConnection, sqlStaffLoginDeveloperAndReader);
			}
		}

		void DropsUnexpectedMembershipOfRolesUsedByCWForApplicationUsers(IEnumerable<string> databases, AdminConnection adminConnection)
		{
			// Arrange
			foreach (var dbName in databases)
			{
				using (ChangeDatabase(adminConnection, dbName))
				{
					adminConnection.ExecuteNonQuery($@"
ALTER ROLE {DbRoleTypes.DbBackupOperatorRole.QuoteName()} ADD MEMBER {dbUnrestrictedWriterLogin.QuoteName()};
ALTER ROLE {nameof(CwRestrictedWriterRole).QuoteName()} ADD MEMBER {dbReaderLogin.QuoteName()};
");
					AssertDatabaseMembershipExists(adminConnection, DbRoleTypes.DbBackupOperatorRole, dbUnrestrictedWriterLogin);
					AssertDatabaseMembershipExists(adminConnection, nameof(CwRestrictedWriterRole), dbReaderLogin);
				}
			}

			var adminTask = new DbSecurityAdminTask();
			adminTask.ServiceLogger = Mock.Of<ILogger>();

			// Act
			adminTask.RunTask(CancellationToken.None);

			// Assert
			foreach (var dbName in databases)
			{
				using (ChangeDatabase(adminConnection, dbName))
				{
					AssertDatabaseMembershipMissing(adminConnection, DbRoleTypes.DbBackupOperatorRole, dbUnrestrictedWriterLogin);
					AssertDatabaseMembershipMissing(adminConnection, nameof(CwRestrictedWriterRole), dbReaderLogin);
				}
			}
		}

		void DropsUnexpectedMembershipsOfRolesUsedByCWForStaffUsers(IEnumerable<string> databases, AdminConnection adminConnection)
		{
			// Arrange
			var sqlSecurityManager = new SqlSecurityManager(Mock.Of<ILogger>(), Db.DatabaseName, allowTransaction: false);

			var factory = new BusinessObjectFactory(adminConnection);
			var staffDeveloperAndReaderName = "TestDeveloperAndReaderUser";
			var staffDeveloperAndReader = factory.New<GlbStaff>();

			staffDeveloperAndReader.GS_FullName = staffDeveloperAndReaderName;
			staffDeveloperAndReader.GS_LoginName = staffDeveloperAndReaderName;
			staffDeveloperAndReader.GS_Code = "12D";
			staffDeveloperAndReader.StaffPlainTextPassword = "TOPSECRET";

			staffDeveloperAndReader.IsDatabaseDeveloper = true;
			staffDeveloperAndReader.IsReadOnlyDBUser = true;

			var staffBackupOperatorName = "TestBackupOperatorUser";
			var staffBackupOperator = factory.New<GlbStaff>();

			staffBackupOperator.GS_FullName = staffBackupOperatorName;
			staffBackupOperator.GS_LoginName = staffBackupOperatorName;
			staffBackupOperator.GS_Code = "12B";
			staffBackupOperator.StaffPlainTextPassword = "TOPSECRET";

			staffBackupOperator.IsBackupOperator = true;

			SetStaffPassword(staffDeveloperAndReader);
			SetStaffPassword(staffBackupOperator);

			var sqlStaffLoginDeveloperAndReader = $"{DbUserRepository.GetStaffDbLoginFullPrefix(Db.DatabaseName)}{staffDeveloperAndReader.GS_LoginName}";
			var sqlStaffLoginBackupOperator = $"{DbUserRepository.GetStaffDbLoginFullPrefix(Db.DatabaseName)}{staffBackupOperator.GS_LoginName}";

			try
			{
				factory.Save();

				sqlSecurityManager.BuildServerSecurity(adminConnection, trialRun: false);

				foreach (var dbName in databases)
				{
					using (ChangeDatabase(adminConnection, dbName))
					{
						sqlSecurityManager.BuildDatabaseSecurity(adminConnection, dbName, trialRun: false);

						adminConnection.ExecuteNonQuery($@"
ALTER ROLE {DbRoleTypes.DbBackupOperatorRole.QuoteName()} ADD MEMBER {sqlStaffLoginDeveloperAndReader.QuoteName()};
ALTER ROLE {nameof(CwRestrictedWriterRole).QuoteName()} ADD MEMBER {sqlStaffLoginBackupOperator.QuoteName()};
");
						AssertDatabaseMembershipExists(adminConnection, DbRoleTypes.DbBackupOperatorRole, sqlStaffLoginDeveloperAndReader);
						AssertDatabaseMembershipExists(adminConnection, nameof(CwRestrictedWriterRole), sqlStaffLoginBackupOperator);
					}
				}

				var adminTask = new DbSecurityAdminTask();
				adminTask.ServiceLogger = Mock.Of<ILogger>();

				// Act
				adminTask.RunTask(CancellationToken.None);

				// Assert
				foreach (var dbName in databases)
				{
					using (ChangeDatabase(adminConnection, dbName))
					{
						AssertDatabaseMembershipMissing(adminConnection, DbRoleTypes.DbBackupOperatorRole, sqlStaffLoginDeveloperAndReader);
						AssertDatabaseMembershipMissing(adminConnection, nameof(CwRestrictedWriterRole), sqlStaffLoginBackupOperator);
					}
				}
			}
			finally
			{
				Helper.DropServerPrincipals(
					adminConnection,
					sqlStaffLoginDeveloperAndReader,
					sqlStaffLoginBackupOperator);
			}
		}

		void DropsUnexpectedMembershipsOfRolesUsedByCWForApplicationRoles(IEnumerable<string> databases, AdminConnection adminConnection)
		{
			// Arrange
			foreach (var dbName in databases)
			{
				using (ChangeDatabase(adminConnection, dbName))
				{
					adminConnection.ExecuteNonQuery($@"
ALTER ROLE {DbRoleTypes.DbBackupOperatorRole.QuoteName()} ADD MEMBER {nameof(CwRestrictedWriterRole).QuoteName()};
ALTER ROLE {nameof(CwRestrictedWriterRole).QuoteName()} ADD MEMBER {nameof(CwUnrestrictedWriterRole).QuoteName()};
");
					AssertDatabaseMembershipExists(adminConnection, DbRoleTypes.DbBackupOperatorRole, nameof(CwRestrictedWriterRole));
					AssertDatabaseMembershipExists(adminConnection, nameof(CwRestrictedWriterRole), nameof(CwUnrestrictedWriterRole));
				}
			}

			var adminTask = new DbSecurityAdminTask();
			adminTask.ServiceLogger = Mock.Of<ILogger>();

			// Act
			adminTask.RunTask(CancellationToken.None);

			// Assert
			foreach (var dbName in databases)
			{
				using (ChangeDatabase(adminConnection, dbName))
				{
					AssertDatabaseMembershipMissing(adminConnection, DbRoleTypes.DbBackupOperatorRole, nameof(CwRestrictedWriterRole));
					AssertDatabaseMembershipMissing(adminConnection, nameof(CwRestrictedWriterRole), nameof(CwUnrestrictedWriterRole));
				}
			}
		}

		void DropsUnexpectedMembershipOfRolesNotUsedByCWForApplicationUsers(IEnumerable<string> databases, AdminConnection adminConnection)
		{
			// Arrange
			foreach (var dbName in databases)
			{
				using (ChangeDatabase(adminConnection, dbName))
				{
					adminConnection.ExecuteNonQuery($@"
ALTER ROLE db_securityadmin ADD MEMBER {dbUnrestrictedWriterLogin.QuoteName()};
ALTER ROLE db_accessadmin ADD MEMBER {dbReaderLogin.QuoteName()};
");
					AssertDatabaseMembershipExists(adminConnection, "db_securityadmin", dbUnrestrictedWriterLogin);
					AssertDatabaseMembershipExists(adminConnection, "db_accessadmin", dbReaderLogin);
				}
			}

			var adminTask = new DbSecurityAdminTask();
			adminTask.ServiceLogger = Mock.Of<ILogger>();

			// Act
			adminTask.RunTask(CancellationToken.None);

			// Assert
			foreach (var dbName in databases)
			{
				using (ChangeDatabase(adminConnection, dbName))
				{
					AssertDatabaseMembershipMissing(adminConnection, "db_securityadmin", dbUnrestrictedWriterLogin);
					AssertDatabaseMembershipMissing(adminConnection, "db_accessadmin", dbReaderLogin);
				}
			}
		}

		void DropsUnexpectedMembershipsOfRolesNotUsedByCWForStaffUsers(IEnumerable<string> databases, AdminConnection adminConnection)
		{
			// Arrange
			var sqlSecurityManager = new SqlSecurityManager(Mock.Of<ILogger>(), Db.DatabaseName, allowTransaction: false);

			var factory = new BusinessObjectFactory(adminConnection);
			var staffDeveloperAndReaderName = "TestDeveloperAndReaderUserToDrop";
			var staffDeveloperAndReader = factory.New<GlbStaff>();

			staffDeveloperAndReader.GS_FullName = staffDeveloperAndReaderName;
			staffDeveloperAndReader.GS_LoginName = staffDeveloperAndReaderName;
			staffDeveloperAndReader.GS_Code = "1DD";
			staffDeveloperAndReader.StaffPlainTextPassword = "TOPSECRET";

			staffDeveloperAndReader.IsDatabaseDeveloper = true;
			staffDeveloperAndReader.IsReadOnlyDBUser = true;

			var staffBackupOperatorName = "TestBackupOperatorUserToDrop";
			var staffBackupOperator = factory.New<GlbStaff>();

			staffBackupOperator.GS_FullName = staffBackupOperatorName;
			staffBackupOperator.GS_LoginName = staffBackupOperatorName;
			staffBackupOperator.GS_Code = "1DB";
			staffBackupOperator.StaffPlainTextPassword = "TOPSECRET";

			staffBackupOperator.IsBackupOperator = true;

			SetStaffPassword(staffDeveloperAndReader);
			SetStaffPassword(staffBackupOperator);
			factory.Save();

			var sqlStaffLoginDeveloperAndReader = $"{DbUserRepository.GetStaffDbLoginFullPrefix(Db.DatabaseName)}{staffDeveloperAndReader.GS_LoginName}";
			var sqlStaffLoginBackupOperator = $"{DbUserRepository.GetStaffDbLoginFullPrefix(Db.DatabaseName)}{staffBackupOperator.GS_LoginName}";

			try
			{
				sqlSecurityManager.BuildServerSecurity(adminConnection, trialRun: false);

				foreach (var dbName in databases)
				{
					using (ChangeDatabase(adminConnection, dbName))
					{
						sqlSecurityManager.BuildDatabaseSecurity(adminConnection, dbName, trialRun: false);

						adminConnection.ExecuteNonQuery($@"
ALTER ROLE db_accessadmin ADD MEMBER {sqlStaffLoginDeveloperAndReader.QuoteName()};
ALTER ROLE db_securityadmin ADD MEMBER {sqlStaffLoginBackupOperator.QuoteName()};
");
						AssertDatabaseMembershipExists(adminConnection, "db_accessadmin", sqlStaffLoginDeveloperAndReader);
						AssertDatabaseMembershipExists(adminConnection, "db_securityadmin", sqlStaffLoginBackupOperator);
					}
				}

				var adminTask = new DbSecurityAdminTask();
				adminTask.ServiceLogger = Mock.Of<ILogger>();

				// Act
				adminTask.RunTask(CancellationToken.None);

				// Assert
				foreach (var dbName in databases)
				{
					using (ChangeDatabase(adminConnection, dbName))
					{
						AssertDatabaseMembershipMissing(adminConnection, "db_accessadmin", sqlStaffLoginDeveloperAndReader);
						AssertDatabaseMembershipMissing(adminConnection, "db_securityadmin", sqlStaffLoginBackupOperator);
					}
				}
			}
			finally
			{
				Helper.DropServerPrincipals(
					adminConnection,
					sqlStaffLoginDeveloperAndReader,
					sqlStaffLoginBackupOperator);
			}
		}

		void DropsUnexpectedMembershipsOfRolesNotUsedByCWForApplicationRoles(IEnumerable<string> databases, AdminConnection adminConnection)
		{
			// Arrange
			foreach (var dbName in databases)
			{
				using (ChangeDatabase(adminConnection, dbName))
				{
					adminConnection.ExecuteNonQuery($@"
ALTER ROLE db_accessadmin ADD MEMBER {nameof(CwRestrictedWriterRole).QuoteName()};
ALTER ROLE db_securityadmin ADD MEMBER {nameof(CwUnrestrictedWriterRole).QuoteName()};
");
					AssertDatabaseMembershipExists(adminConnection, "db_accessadmin", nameof(CwRestrictedWriterRole));
					AssertDatabaseMembershipExists(adminConnection, "db_securityadmin", nameof(CwUnrestrictedWriterRole));
				}
			}

			var adminTask = new DbSecurityAdminTask();
			adminTask.ServiceLogger = Mock.Of<ILogger>();

			// Act
			adminTask.RunTask(CancellationToken.None);

			// Assert
			foreach (var dbName in databases)
			{
				using (ChangeDatabase(adminConnection, dbName))
				{
					AssertDatabaseMembershipMissing(adminConnection, "db_accessadmin", nameof(CwRestrictedWriterRole));
					AssertDatabaseMembershipMissing(adminConnection, "db_securityadmin", nameof(CwUnrestrictedWriterRole));
				}
			}
		}

		void DoesNotDropUnexpectedMembershipOfRolesUsedByCWForApplicationUsers(IEnumerable<string> databases, AdminConnection adminConnection)
		{
			// Arrange
			foreach (var dbName in databases)
			{
				using (ChangeDatabase(adminConnection, dbName))
				{
					adminConnection.ExecuteNonQuery($@"
ALTER ROLE {DbRoleTypes.DbBackupOperatorRole.QuoteName()} ADD MEMBER {dbUnrestrictedWriterLogin.QuoteName()};
ALTER ROLE {nameof(CwRestrictedWriterRole).QuoteName()} ADD MEMBER {dbReaderLogin.QuoteName()};
");
					AssertDatabaseMembershipExists(adminConnection, DbRoleTypes.DbBackupOperatorRole, dbUnrestrictedWriterLogin);
					AssertDatabaseMembershipExists(adminConnection, nameof(CwRestrictedWriterRole), dbReaderLogin);
				}
			}

			var adminTask = new DbSecurityAdminTask();
			adminTask.ServiceLogger = Mock.Of<ILogger>();

			// Act
			adminTask.RunTask(CancellationToken.None);

			// Assert
			foreach (var dbName in databases)
			{
				using (ChangeDatabase(adminConnection, dbName))
				{
					AssertDatabaseMembershipExists(adminConnection, DbRoleTypes.DbBackupOperatorRole, dbUnrestrictedWriterLogin);
					AssertDatabaseMembershipExists(adminConnection, nameof(CwRestrictedWriterRole), dbReaderLogin);
				}
			}
		}

		void DoesNotDropUnexpectedMembershipOfRolesNotUsedByCWForApplicationUsers(IEnumerable<string> databases, AdminConnection adminConnection)
		{
			// Arrange
			foreach (var dbName in databases)
			{
				using (ChangeDatabase(adminConnection, dbName))
				{
					adminConnection.ExecuteNonQuery($@"
ALTER ROLE db_securityadmin ADD MEMBER {dbWriterLogin.QuoteName()};
");
					AssertDatabaseMembershipExists(adminConnection, "db_securityadmin", dbWriterLogin);
				}
			}

			var adminTask = new DbSecurityAdminTask();
			adminTask.ServiceLogger = Mock.Of<ILogger>();

			// Act
			adminTask.RunTask(CancellationToken.None);

			// Assert
			foreach (var dbName in databases)
			{
				using (ChangeDatabase(adminConnection, dbName))
				{
					AssertDatabaseMembershipExists(adminConnection, "db_securityadmin", dbWriterLogin);
				}
			}
		}

		void DoesNotDropUnexpectedMembershipsOfRolesUsedInCWForStaffUsers(IEnumerable<string> databases, AdminConnection adminConnection)
		{
			// Arrange
			var factory = new BusinessObjectFactory(adminConnection);
			var staffDeveloperAndReaderName = "TestDeveloperAndReaderUser";
			var staffDeveloperAndReader = factory.New<GlbStaff>();

			staffDeveloperAndReader.GS_FullName = staffDeveloperAndReaderName;
			staffDeveloperAndReader.GS_LoginName = staffDeveloperAndReaderName;
			staffDeveloperAndReader.GS_Code = "12M";
			staffDeveloperAndReader.StaffPlainTextPassword = "TOPSECRET";

			staffDeveloperAndReader.IsDatabaseDeveloper = true;
			staffDeveloperAndReader.IsReadOnlyDBUser = true;

			var staffBackupOperatorName = "TestBackupOperator";
			var staffBackupOperator = factory.New<GlbStaff>();

			staffBackupOperator.GS_FullName = staffBackupOperatorName;
			staffBackupOperator.GS_LoginName = staffBackupOperatorName;
			staffBackupOperator.GS_Code = "14M";
			staffBackupOperator.StaffPlainTextPassword = "TOPSECRET";

			staffBackupOperator.IsBackupOperator = true;

			SetStaffPassword(staffDeveloperAndReader);
			SetStaffPassword(staffBackupOperator);

			var sqlStaffLoginDeveloperAndReader = $"{DbUserRepository.StaffDbLoginPrefix}_{Db.DatabaseName}_{staffDeveloperAndReader.GS_LoginName}";
			var sqlStaffLoginBackupOperator = $"{DbUserRepository.StaffDbLoginPrefix}_{Db.DatabaseName}_{staffBackupOperator.GS_LoginName}";

			try
			{
				factory.Save();

				foreach (var dbName in databases)
				{
					using (ChangeDatabase(adminConnection, dbName))
					{
						adminConnection.ExecuteNonQuery($@"
ALTER ROLE {DbRoleTypes.DbBackupOperatorRole.QuoteName()} ADD MEMBER {sqlStaffLoginDeveloperAndReader.QuoteName()};
ALTER ROLE {nameof(CwRestrictedWriterRole).QuoteName()} ADD MEMBER {sqlStaffLoginBackupOperator.QuoteName()};
");
						AssertDatabaseMembershipExists(adminConnection, DbRoleTypes.DbBackupOperatorRole, sqlStaffLoginDeveloperAndReader);
						AssertDatabaseMembershipExists(adminConnection, nameof(CwRestrictedWriterRole), sqlStaffLoginBackupOperator);
					}
				}

				var adminTask = new DbSecurityAdminTask();
				adminTask.ServiceLogger = Mock.Of<ILogger>();

				// Act
				adminTask.RunTask(CancellationToken.None);

				// Assert
				foreach (var dbName in databases)
				{
					using (ChangeDatabase(adminConnection, dbName))
					{
						AssertDatabaseMembershipExists(adminConnection, DbRoleTypes.DbBackupOperatorRole, sqlStaffLoginDeveloperAndReader);
						AssertDatabaseMembershipExists(adminConnection, nameof(CwRestrictedWriterRole), sqlStaffLoginBackupOperator);
					}
				}
			}
			finally
			{
				Helper.DropServerPrincipals(
					adminConnection,
					sqlStaffLoginDeveloperAndReader,
					sqlStaffLoginBackupOperator);
			}
		}

		void DoesNotDropUnexpectedMembershipsOfRolesNotUsedInCWForStaffUsers(IEnumerable<string> databases, AdminConnection adminConnection)
		{
			// Arrange
			var factory = new BusinessObjectFactory(adminConnection);
			var staffDeveloperAndReaderName = "TestDeveloperAndReaderUser";
			var staffDeveloperAndReader = factory.New<GlbStaff>();

			staffDeveloperAndReader.GS_FullName = staffDeveloperAndReaderName;
			staffDeveloperAndReader.GS_LoginName = staffDeveloperAndReaderName;
			staffDeveloperAndReader.GS_Code = "1NM";
			staffDeveloperAndReader.StaffPlainTextPassword = "TOPSECRET";

			staffDeveloperAndReader.IsDatabaseDeveloper = true;
			staffDeveloperAndReader.IsReadOnlyDBUser = true;

			SetStaffPassword(staffDeveloperAndReader);

			var sqlStaffLoginDeveloperAndReader = $"{DbUserRepository.StaffDbLoginPrefix}_{Db.DatabaseName}_{staffDeveloperAndReader.GS_LoginName}";

			try
			{
				factory.Save();

				foreach (var dbName in databases)
				{
					using (ChangeDatabase(adminConnection, dbName))
					{
						adminConnection.ExecuteNonQuery($@"
ALTER ROLE db_accessadmin ADD MEMBER {sqlStaffLoginDeveloperAndReader.QuoteName()};
");
						AssertDatabaseMembershipExists(adminConnection, "db_accessadmin", sqlStaffLoginDeveloperAndReader);
					}
				}

				var adminTask = new DbSecurityAdminTask();
				adminTask.ServiceLogger = Mock.Of<ILogger>();

				// Act
				adminTask.RunTask(CancellationToken.None);

				// Assert
				foreach (var dbName in databases)
				{
					using (ChangeDatabase(adminConnection, dbName))
					{
						AssertDatabaseMembershipExists(adminConnection, "db_accessadmin", sqlStaffLoginDeveloperAndReader);
					}
				}
			}
			finally
			{
				Helper.DropServerPrincipals(adminConnection, sqlStaffLoginDeveloperAndReader);
			}
		}

		void DoesNotDropUnexpectedMembershipsOfRolesUsedInCWForApplicationRoles(IEnumerable<string> databases, AdminConnection adminConnection)
		{
			// Arrange
			foreach (var dbName in databases)
			{
				using (ChangeDatabase(adminConnection, dbName))
				{
					adminConnection.ExecuteNonQuery($@"
ALTER ROLE {DbRoleTypes.DbBackupOperatorRole.QuoteName()} ADD MEMBER {nameof(CwRestrictedWriterRole).QuoteName()};
ALTER ROLE {nameof(CwRestrictedWriterRole).QuoteName()} ADD MEMBER {nameof(CwUnrestrictedWriterRole).QuoteName()};
");
					AssertDatabaseMembershipExists(adminConnection, DbRoleTypes.DbBackupOperatorRole, nameof(CwRestrictedWriterRole));
					AssertDatabaseMembershipExists(adminConnection, nameof(CwRestrictedWriterRole), nameof(CwUnrestrictedWriterRole));
				}
			}

			var adminTask = new DbSecurityAdminTask();
			adminTask.ServiceLogger = Mock.Of<ILogger>();

			// Act
			adminTask.RunTask(CancellationToken.None);

			// Assert
			foreach (var dbName in databases)
			{
				using (ChangeDatabase(adminConnection, dbName))
				{
					AssertDatabaseMembershipExists(adminConnection, DbRoleTypes.DbBackupOperatorRole, nameof(CwRestrictedWriterRole));
					AssertDatabaseMembershipExists(adminConnection, nameof(CwRestrictedWriterRole), nameof(CwUnrestrictedWriterRole));
				}
			}
		}

		void DoesNotDropUnexpectedMembershipsOfRolesNotUsedInCWForApplicationRoles(IEnumerable<string> databases, AdminConnection adminConnection)
		{
			// Arrange
			foreach (var dbName in databases)
			{
				using (ChangeDatabase(adminConnection, dbName))
				{
					adminConnection.ExecuteNonQuery($@"
ALTER ROLE db_accessadmin ADD MEMBER {nameof(CwRestrictedWriterRole).QuoteName()};
");
					AssertDatabaseMembershipExists(adminConnection, "db_accessadmin", nameof(CwRestrictedWriterRole));
				}
			}

			var adminTask = new DbSecurityAdminTask();
			adminTask.ServiceLogger = Mock.Of<ILogger>();

			// Act
			adminTask.RunTask(CancellationToken.None);

			// Assert
			foreach (var dbName in databases)
			{
				using (ChangeDatabase(adminConnection, dbName))
				{
					AssertDatabaseMembershipExists(adminConnection, "db_accessadmin", nameof(CwRestrictedWriterRole));
				}
			}
		}

		void DropsUnexpectedMembershipsForStaffAndApplicationLogins(AdminConnection adminConnection)
		{
			// Arrange
			var sqlSecurityManager = new SqlSecurityManager(Mock.Of<ILogger>(), Db.DatabaseName, allowTransaction: false);

			var factory = new BusinessObjectFactory(adminConnection);
			var staffDeveloperAndReaderName = "TestDeveloperAndReaderLogin";
			var staffDeveloperAndReader = factory.New<GlbStaff>();

			staffDeveloperAndReader.GS_FullName = staffDeveloperAndReaderName;
			staffDeveloperAndReader.GS_LoginName = staffDeveloperAndReaderName;
			staffDeveloperAndReader.GS_Code = "12M";
			staffDeveloperAndReader.StaffPlainTextPassword = "TOPSECRET";

			staffDeveloperAndReader.IsDatabaseDeveloper = true;
			staffDeveloperAndReader.IsReadOnlyDBUser = true;

			SetStaffPassword(staffDeveloperAndReader);

			var sqlStaffLoginDeveloperAndReader = $"{DbUserRepository.GetStaffDbLoginFullPrefix(Db.DatabaseName)}{staffDeveloperAndReader.GS_LoginName}";

			try
			{
				factory.Save();

				sqlSecurityManager.BuildServerSecurity(adminConnection, trialRun: false);

				using (ChangeDatabase(adminConnection, Db.SqlMasterDb))
				{
					adminConnection.ExecuteNonQuery($@"
ALTER SERVER ROLE sysadmin ADD MEMBER {sqlStaffLoginDeveloperAndReader.QuoteName()};
ALTER SERVER ROLE dbcreator ADD MEMBER {dbReaderLogin.QuoteName()};
");
				}

				AssertServerMembershipExists(adminConnection, "sysadmin", sqlStaffLoginDeveloperAndReader);
				AssertServerMembershipExists(adminConnection, "dbcreator", dbReaderLogin);

				var adminTask = new DbSecurityAdminTask();
				adminTask.ServiceLogger = Mock.Of<ILogger>();

				// Act
				adminTask.RunTask(CancellationToken.None);

				// Assert
				AssertServerMembershipMissing(adminConnection, "sysadmin", sqlStaffLoginDeveloperAndReader);
				AssertServerMembershipMissing(adminConnection, "dbcreator", dbReaderLogin);
			}
			finally
			{
				Helper.DropServerPrincipals(adminConnection, sqlStaffLoginDeveloperAndReader);
			}
		}

		void DoesNotDropUnexpectedMembershipsForStaffAndApplicationLogins(AdminConnection adminConnection)
		{
			// Arrange
			var factory = new BusinessObjectFactory(adminConnection);
			var staffDeveloperAndReaderName = "TestDeveloperAndReaderLogin";
			var staffDeveloperAndReader = factory.New<GlbStaff>();

			staffDeveloperAndReader.GS_FullName = staffDeveloperAndReaderName;
			staffDeveloperAndReader.GS_LoginName = staffDeveloperAndReaderName;
			staffDeveloperAndReader.GS_Code = "12L";
			staffDeveloperAndReader.StaffPlainTextPassword = "TOPSECRET";

			staffDeveloperAndReader.IsDatabaseDeveloper = true;
			staffDeveloperAndReader.IsReadOnlyDBUser = true;

			SetStaffPassword(staffDeveloperAndReader);

			var sqlStaffLoginDeveloperAndReader = $"{DbUserRepository.StaffDbLoginPrefix}_{Db.DatabaseName}_{staffDeveloperAndReader.GS_LoginName}";

			try
			{
				factory.Save();

				using (ChangeDatabase(adminConnection, Db.SqlMasterDb))
				{
					adminConnection.ExecuteNonQuery($@"
ALTER SERVER ROLE sysadmin ADD MEMBER {sqlStaffLoginDeveloperAndReader.QuoteName()};
ALTER SERVER ROLE dbcreator ADD MEMBER {dbReaderLogin.QuoteName()};
");
				}

				AssertServerMembershipExists(adminConnection, "sysadmin", sqlStaffLoginDeveloperAndReader);
				AssertServerMembershipExists(adminConnection, "dbcreator", dbReaderLogin);

				var adminTask = new DbSecurityAdminTask();
				adminTask.ServiceLogger = Mock.Of<ILogger>();

				// Act
				adminTask.RunTask(CancellationToken.None);

				// Assert
				AssertServerMembershipExists(adminConnection, "sysadmin", sqlStaffLoginDeveloperAndReader);
				AssertServerMembershipExists(adminConnection, "dbcreator", dbReaderLogin);
			}
			finally
			{
				adminConnection.ExecuteNonQuery($"ALTER SERVER ROLE dbcreator DROP MEMBER {dbReaderLogin.QuoteName()}");
				Helper.DropServerPrincipals(adminConnection, sqlStaffLoginDeveloperAndReader);
			}
		}

		void RestoresDroppedRequiredPermissions(AdminConnection adminConnection)
		{
			// Arrange
			var sqlSecurityManager = new SqlSecurityManager(Mock.Of<ILogger>(), Db.DatabaseName, allowTransaction: false);
			var factory = new BusinessObjectFactory(adminConnection);
			var staffDeveloperAndReaderName = "TestDeveloperAndReader";
			var staffDeveloperAndReader = factory.New<GlbStaff>();

			staffDeveloperAndReader.GS_FullName = staffDeveloperAndReaderName;
			staffDeveloperAndReader.GS_LoginName = staffDeveloperAndReaderName;
			staffDeveloperAndReader.GS_Code = "123";
			staffDeveloperAndReader.StaffPlainTextPassword = "TOPSECRET";

			staffDeveloperAndReader.IsDatabaseDeveloper = true;
			staffDeveloperAndReader.IsReadOnlyDBUser = true;

			SetStaffPassword(staffDeveloperAndReader);

			var sqlStaffLoginDeveloperAndReader = $"{DbUserRepository.GetStaffDbLoginFullPrefix(Db.DatabaseName)}{staffDeveloperAndReader.GS_LoginName}";

			try
			{
				factory.Save();

				sqlSecurityManager.BuildServerSecurity(adminConnection, trialRun: false);

				using (ChangeDatabase(adminConnection, Db.SqlMasterDb))
				{
					adminConnection.ExecuteNonQuery($@"
REVOKE CONNECT SQL TO {sqlStaffLoginDeveloperAndReader.QuoteName()};
REVOKE CONNECT SQL TO {dbReaderLogin.QuoteName()};
				");
				}

				AssertServerPermissionMissing(adminConnection, "G", "CONNECT SQL", sqlStaffLoginDeveloperAndReader);
				AssertServerPermissionMissing(adminConnection, "G", "CONNECT SQL", dbReaderLogin);

				foreach (var dbName in adminConnection.GetDatabases(DatabaseType.All & ~DatabaseType.SingleSharedRef))
				{
					using (ChangeDatabase(adminConnection, dbName))
					{
						sqlSecurityManager.BuildDatabaseSecurity(adminConnection, dbName, trialRun: false);
						adminConnection.ExecuteNonQuery($@"
REVOKE SELECT ON SCHEMA::dbo TO {DbRoleTypes.CwRestrictedWriterRole.QuoteName()};
REVOKE EXECUTE TO {dbWriterLogin.QuoteName()};
REVOKE CONNECT TO {sqlStaffLoginDeveloperAndReader.QuoteName()};
");
						AssertSchemaPermissionMissing(adminConnection, "dbo", "G", "SELECT", DbRoleTypes.CwRestrictedWriterRole);
						AssertDatabasePermissionMissing(adminConnection, dbWriterLogin, "G", "EXECUTE");
						AssertDatabasePermissionMissing(adminConnection, sqlStaffLoginDeveloperAndReader, "G", "CONNECT");
					}
				}

				using (ChangeDatabase(adminConnection, RefDbTableNameResolver.SingleRefDatabaseName))
				{
					adminConnection.ExecuteNonQuery($@"
REVOKE SELECT TO guest
");
					AssertDatabasePermissionMissing(adminConnection, "guest", "G", "SELECT");
				}

				var adminTask = new DbSecurityAdminTask();
				adminTask.ServiceLogger = Mock.Of<ILogger>();

				// Act
				adminTask.RunTask(CancellationToken.None);

				// Assert
				AssertServerPermissionExists(adminConnection, "G", "CONNECT SQL", sqlStaffLoginDeveloperAndReader);
				AssertServerPermissionExists(adminConnection, "G", "CONNECT SQL", dbReaderLogin);

				foreach (var dbName in adminConnection.GetDatabases(DatabaseType.All & ~DatabaseType.SingleSharedRef))
				{
					using (ChangeDatabase(adminConnection, dbName))
					{
						AssertSchemaPermissionExists(adminConnection, "G", "SELECT", "dbo", DbRoleTypes.CwRestrictedWriterRole);
						AssertDatabasePermissionExists(adminConnection, "G", "EXECUTE", dbWriterLogin);
						AssertDatabasePermissionExists(adminConnection, "G", "CONNECT", sqlStaffLoginDeveloperAndReader);
					}
				}

				using (ChangeDatabase(adminConnection, RefDbTableNameResolver.SingleRefDatabaseName))
				{
					AssertDatabasePermissionExists(adminConnection, "G", "SELECT", "guest");
				}
			}
			finally
			{
				using (ChangeDatabase(adminConnection, Db.SqlMasterDb))
				{
					adminConnection.ExecuteNonQuery($@"
GRANT CONNECT SQL TO {dbReaderLogin.QuoteName()}
");
					Helper.DropServerPrincipals(adminConnection, sqlStaffLoginDeveloperAndReader);
				}
			}
		}

		void DoesNotRestoreDroppedRequiredPermissions(AdminConnection adminConnection)
		{
			// Arrange
			var factory = new BusinessObjectFactory(adminConnection);
			var staffDeveloperAndReaderName = "TestDeveloperAndReader";
			var staffDeveloperAndReader = factory.New<GlbStaff>();

			staffDeveloperAndReader.GS_FullName = staffDeveloperAndReaderName;
			staffDeveloperAndReader.GS_LoginName = staffDeveloperAndReaderName;
			staffDeveloperAndReader.GS_Code = "123";
			staffDeveloperAndReader.StaffPlainTextPassword = "TOPSECRET";

			staffDeveloperAndReader.IsDatabaseDeveloper = true;
			staffDeveloperAndReader.IsReadOnlyDBUser = true;

			SetStaffPassword(staffDeveloperAndReader);

			var sqlStaffLoginDeveloperAndReader = $"{DbUserRepository.StaffDbLoginPrefix}_{Db.DatabaseName}_{staffDeveloperAndReader.GS_LoginName}";

			try
			{
				factory.Save();

				using (ChangeDatabase(adminConnection, Db.SqlMasterDb))
				{
					adminConnection.ExecuteNonQuery($@"
REVOKE CONNECT SQL TO {sqlStaffLoginDeveloperAndReader.QuoteName()};
REVOKE CONNECT SQL TO {dbReaderLogin.QuoteName()};
				");
				}

				AssertServerPermissionMissing(adminConnection, "G", "CONNECT SQL", sqlStaffLoginDeveloperAndReader);
				AssertServerPermissionMissing(adminConnection, "G", "CONNECT SQL", dbReaderLogin);

				foreach (var dbName in adminConnection.GetDatabases(DatabaseType.All & ~DatabaseType.SingleSharedRef & ~DatabaseType.EDW))
				{
					using (ChangeDatabase(adminConnection, dbName))
					{
						adminConnection.ExecuteNonQuery($@"
REVOKE SHOWPLAN TO {nameof(CwReaderRole).QuoteName()};
REVOKE EXECUTE TO {dbWriterLogin.QuoteName()};
REVOKE CONNECT TO {sqlStaffLoginDeveloperAndReader.QuoteName()};
");
						AssertDatabasePermissionMissing(adminConnection, "G", "SHOWPLAN", nameof(CwReaderRole));
						AssertDatabasePermissionMissing(adminConnection, dbWriterLogin, "G", "EXECUTE");
						AssertDatabasePermissionMissing(adminConnection, sqlStaffLoginDeveloperAndReader, "G", "CONNECT");
					}
				}

				using (ChangeDatabase(adminConnection, RefDbTableNameResolver.SingleRefDatabaseName))
				{
					adminConnection.ExecuteNonQuery($@"
REVOKE SELECT TO guest;
REVOKE SELECT ON SCHEMA :: dbo FROM GUEST;
");
					AssertDatabasePermissionMissing(adminConnection, "G", "SELECT", "guest");
				}

				var adminTask = new DbSecurityAdminTask();
				adminTask.ServiceLogger = Mock.Of<ILogger>();

				// Act
				adminTask.RunTask(CancellationToken.None);

				// Assert
				AssertServerPermissionMissing(adminConnection, "G", "CONNECT SQL", sqlStaffLoginDeveloperAndReader);
				AssertServerPermissionMissing(adminConnection, "G", "CONNECT SQL", dbReaderLogin);

				foreach (var dbName in adminConnection.GetDatabases(DatabaseType.All & ~DatabaseType.SingleSharedRef & ~DatabaseType.EDW))
				{
					using (ChangeDatabase(adminConnection, dbName))
					{
						AssertDatabasePermissionMissing(adminConnection, "G", "SHOWPLAN", nameof(CwReaderRole));
						AssertDatabasePermissionMissing(adminConnection, "G", "EXECUTE", dbWriterLogin);
						AssertDatabasePermissionMissing(adminConnection, "G", "CONNECT", sqlStaffLoginDeveloperAndReader);
					}
				}

				using (ChangeDatabase(adminConnection, RefDbTableNameResolver.SingleRefDatabaseName))
				{
					AssertDatabasePermissionMissing(adminConnection, "G", "SELECT", "guest");
				}
			}
			finally
			{
				using (ChangeDatabase(adminConnection, RefDbTableNameResolver.SingleRefDatabaseName))
				{
					adminConnection.ExecuteNonQuery(@"GRANT SELECT TO guest;
GRANT SELECT ON SCHEMA :: dbo TO GUEST;");
				}

				using (ChangeDatabase(adminConnection, Db.SqlMasterDb))
				{
					adminConnection.ExecuteNonQuery($@"
GRANT CONNECT SQL TO {dbReaderLogin.QuoteName()}
");
					Helper.DropServerPrincipals(adminConnection, sqlStaffLoginDeveloperAndReader);
				}
			}
		}

		void RestoresDroppedRequiredApplicationRoleAndUserRoles(AdminConnection adminConnection)
		{
			// Arrange
			foreach (var dbName in adminConnection.GetDatabases(DatabaseType.All & ~DatabaseType.SingleSharedRef))
			{
				using (ChangeDatabase(adminConnection, dbName))
				{
					adminConnection.ExecuteNonQuery($@"
ALTER ROLE {nameof(CwRestrictedReaderRole).QuoteName()} DROP MEMBER {dbRestrictedReaderLogin.QuoteName()};
ALTER ROLE {nameof(CwUnrestrictedWriterRole).QuoteName()} DROP MEMBER {dbUnrestrictedWriterLogin.QuoteName()};

ALTER ROLE {DbRoleTypes.DbDataReaderRole.QuoteName()} DROP MEMBER {nameof(CwReaderRole).QuoteName()};
");
					AssertDatabaseMembershipMissing(adminConnection, nameof(CwRestrictedReaderRole), dbRestrictedReaderLogin);
					AssertDatabaseMembershipMissing(adminConnection, nameof(CwUnrestrictedWriterRole), dbUnrestrictedWriterLogin);
					AssertDatabaseMembershipMissing(adminConnection, DbRoleTypes.DbDataReaderRole, nameof(CwReaderRole));
				}
			}

			var adminTask = new DbSecurityAdminTask();
			adminTask.ServiceLogger = Mock.Of<ILogger>();

			// Act
			adminTask.RunTask(CancellationToken.None);

			// Assert
			foreach (var dbName in adminConnection.GetDatabases(DatabaseType.All & ~DatabaseType.SingleSharedRef))
			{
				using (ChangeDatabase(adminConnection, dbName))
				{
					AssertDatabaseMembershipExists(adminConnection, nameof(CwRestrictedReaderRole), dbRestrictedReaderLogin);
					AssertDatabaseMembershipExists(adminConnection, nameof(CwUnrestrictedWriterRole), dbUnrestrictedWriterLogin);
					AssertDatabaseMembershipExists(adminConnection, DbRoleTypes.DbDataReaderRole, nameof(CwReaderRole));
				}
			}
		}

		void RestoresDroppedRequiredUserRepositoryStaffUserRoles(AdminConnection adminConnection)
		{
			// Arrange
			var sqlSecurityManager = new SqlSecurityManager(Mock.Of<ILogger>(), Db.DatabaseName, allowTransaction: false);
			var factory = new BusinessObjectFactory(adminConnection);
			var staffDeveloperAndReaderName = "TestDeveloperAndReaderInUserRepository";
			var staffDeveloperAndReader = factory.New<GlbStaff>();

			staffDeveloperAndReader.GS_FullName = staffDeveloperAndReaderName;
			staffDeveloperAndReader.GS_LoginName = staffDeveloperAndReaderName;
			staffDeveloperAndReader.GS_Code = "12U";
			staffDeveloperAndReader.StaffPlainTextPassword = "TOPSECRET";

			staffDeveloperAndReader.IsDatabaseDeveloper = true;
			staffDeveloperAndReader.IsReadOnlyDBUser = true;

			SetStaffPassword(staffDeveloperAndReader);

			var sqlStaffLoginDeveloperAndReader = $"{DbUserRepository.GetStaffDbLoginFullPrefix(Db.DatabaseName)}{staffDeveloperAndReader.GS_LoginName}";

			try
			{
				factory.Save();

				sqlSecurityManager.BuildServerSecurity(adminConnection, trialRun: false);

				foreach (var dbName in adminConnection.GetDatabases(DatabaseType.UserRepository))
				{
					using (ChangeDatabase(adminConnection, dbName))
					{
						sqlSecurityManager.BuildDatabaseSecurity(adminConnection, dbName, trialRun: false);
						adminConnection.ExecuteNonQuery($@"
ALTER ROLE {DbRoleTypes.DbDataWriterRole.QuoteName()} DROP MEMBER {sqlStaffLoginDeveloperAndReader.QuoteName()};
ALTER ROLE {nameof(CwRestrictedReaderRole).QuoteName()} DROP MEMBER {sqlStaffLoginDeveloperAndReader.QuoteName()};
");
						AssertDatabaseMembershipMissing(adminConnection, DbRoleTypes.DbDataWriterRole, sqlStaffLoginDeveloperAndReader);
						AssertDatabaseMembershipMissing(adminConnection, nameof(CwRestrictedReaderRole), sqlStaffLoginDeveloperAndReader);
					}
				}

				var adminTask = new DbSecurityAdminTask();
				adminTask.ServiceLogger = Mock.Of<ILogger>();

				// Act
				adminTask.RunTask(CancellationToken.None);

				// Assert
				foreach (var dbName in adminConnection.GetDatabases(DatabaseType.UserRepository))
				{
					using (ChangeDatabase(adminConnection, dbName))
					{
						AssertDatabaseMembershipExists(adminConnection, DbRoleTypes.DbDataWriterRole, sqlStaffLoginDeveloperAndReader);
						AssertDatabaseMembershipExists(adminConnection, nameof(CwRestrictedReaderRole), sqlStaffLoginDeveloperAndReader);
					}
				}
			}
			finally
			{
				Helper.DropServerPrincipals(adminConnection, sqlStaffLoginDeveloperAndReader);
			}
		}

		void RestoresDroppedRequiredStaffUserRolesInSelfHosted(AdminConnection adminConnection)
		{
			// Arrange
			var sqlSecurityManager = new SqlSecurityManager(Mock.Of<ILogger>(), Db.DatabaseName, allowTransaction: false);

			var factory = new BusinessObjectFactory(adminConnection);
			var staffDeveloperAndReaderName = "TestDeveloperAndReader";
			var staffDeveloperAndReader = factory.New<GlbStaff>();

			staffDeveloperAndReader.GS_FullName = staffDeveloperAndReaderName;
			staffDeveloperAndReader.GS_LoginName = staffDeveloperAndReaderName;
			staffDeveloperAndReader.GS_Code = "123";
			staffDeveloperAndReader.StaffPlainTextPassword = "TOPSECRET";

			staffDeveloperAndReader.IsDatabaseDeveloper = true;
			staffDeveloperAndReader.IsReadOnlyDBUser = true;

			var staffBackupOperatorName = "TestBackupOperator";
			var staffBackupOperator = factory.New<GlbStaff>();

			staffBackupOperator.GS_FullName = staffBackupOperatorName;
			staffBackupOperator.GS_LoginName = staffBackupOperatorName;
			staffBackupOperator.GS_Code = "12G";
			staffBackupOperator.StaffPlainTextPassword = "TOPSECRET";

			staffBackupOperator.IsBackupOperator = true;

			SetStaffPassword(staffDeveloperAndReader);
			SetStaffPassword(staffBackupOperator);

			var sqlStaffLoginDeveloperAndReader = $"{DbUserRepository.GetStaffDbLoginFullPrefix(Db.DatabaseName)}{staffDeveloperAndReader.GS_LoginName}";
			var sqlStaffLoginBackupOperator = $"{DbUserRepository.GetStaffDbLoginFullPrefix(Db.DatabaseName)}{staffBackupOperator.GS_LoginName}";

			try
			{
				factory.Save();

				sqlSecurityManager.BuildServerSecurity(adminConnection, trialRun: false);

				foreach (var dbName in adminConnection.GetDatabases(DatabaseType.All & ~DatabaseType.SingleSharedRef))
				{
					using (ChangeDatabase(adminConnection, dbName))
					{
						sqlSecurityManager.BuildDatabaseSecurity(adminConnection, dbName, trialRun: false);

						adminConnection.ExecuteNonQuery($@"
ALTER ROLE {nameof(CwRestrictedReaderRole).QuoteName()} DROP MEMBER {sqlStaffLoginDeveloperAndReader.QuoteName()};
ALTER ROLE {DbRoleTypes.DbBackupOperatorRole.QuoteName()} DROP MEMBER {sqlStaffLoginBackupOperator.QuoteName()};
");
						AssertDatabaseMembershipMissing(adminConnection, nameof(CwRestrictedReaderRole), sqlStaffLoginDeveloperAndReader);
						AssertDatabaseMembershipMissing(adminConnection, DbRoleTypes.DbBackupOperatorRole, sqlStaffLoginBackupOperator);
					}
				}

				var adminTask = new DbSecurityAdminTask();
				adminTask.ServiceLogger = Mock.Of<ILogger>();

				// Act
				adminTask.RunTask(CancellationToken.None);

				// Assert
				foreach (var dbName in adminConnection.GetDatabases(DatabaseType.All & ~DatabaseType.SingleSharedRef))
				{
					using (ChangeDatabase(adminConnection, dbName))
					{
						AssertDatabaseMembershipExists(adminConnection, nameof(CwRestrictedReaderRole), sqlStaffLoginDeveloperAndReader);
						AssertDatabaseMembershipExists(adminConnection, DbRoleTypes.DbBackupOperatorRole, sqlStaffLoginBackupOperator);
					}
				}
			}
			finally
			{
				Helper.DropServerPrincipals(
					adminConnection,
					sqlStaffLoginDeveloperAndReader,
					sqlStaffLoginBackupOperator);
			}
		}

		void RestoresDroppedRequiredStaffUserRolesHostedInCW(AdminConnection adminConnection)
		{
			// Arrange
			var sqlSecurityManager = new SqlSecurityManager(Mock.Of<ILogger>(), Db.DatabaseName, allowTransaction: false);
			var factory = new BusinessObjectFactory(adminConnection);
			var staffDeveloperAndReaderName = "TestDeveloperAndReader";
			var staffDeveloperAndReader = factory.New<GlbStaff>();

			staffDeveloperAndReader.GS_FullName = staffDeveloperAndReaderName;
			staffDeveloperAndReader.GS_LoginName = staffDeveloperAndReaderName;
			staffDeveloperAndReader.GS_Code = "123";
			staffDeveloperAndReader.StaffPlainTextPassword = "TOPSECRET";

			staffDeveloperAndReader.IsDatabaseDeveloper = true;
			staffDeveloperAndReader.IsReadOnlyDBUser = true;

			var staffBackupOperatorName = "TestBackupOperator";
			var staffBackupOperator = factory.New<GlbStaff>();

			staffBackupOperator.GS_FullName = staffBackupOperatorName;
			staffBackupOperator.GS_LoginName = staffBackupOperatorName;
			staffBackupOperator.GS_Code = "12G";
			staffBackupOperator.StaffPlainTextPassword = "TOPSECRET";

			staffBackupOperator.IsBackupOperator = true;

			SetStaffPassword(staffDeveloperAndReader);
			SetStaffPassword(staffBackupOperator);

			var sqlStaffLoginDeveloperAndReader = $"{DbUserRepository.GetStaffDbLoginFullPrefix(Db.DatabaseName)}{staffDeveloperAndReader.GS_LoginName}";
			var sqlStaffLoginBackupOperator = $"{DbUserRepository.GetStaffDbLoginFullPrefix(Db.DatabaseName)}{staffBackupOperator.GS_LoginName}";

			try
			{
				factory.Save();

				sqlSecurityManager.BuildServerSecurity(adminConnection, trialRun: false);

				foreach (var dbName in adminConnection.GetDatabases(DatabaseType.All & ~DatabaseType.SingleSharedRef))
				{
					using (ChangeDatabase(adminConnection, dbName))
					{
						sqlSecurityManager.BuildDatabaseSecurity(adminConnection, dbName, trialRun: false);
						adminConnection.ExecuteNonQuery($@"
ALTER ROLE {nameof(CwRestrictedReaderRole).QuoteName()} DROP MEMBER {sqlStaffLoginDeveloperAndReader.QuoteName()};
ALTER ROLE {DbRoleTypes.DbBackupOperatorRole.QuoteName()} DROP MEMBER {sqlStaffLoginBackupOperator.QuoteName()};
");
						AssertDatabaseMembershipMissing(adminConnection, nameof(CwRestrictedReaderRole), sqlStaffLoginDeveloperAndReader);
						AssertDatabaseMembershipMissing(adminConnection, DbRoleTypes.DbBackupOperatorRole, sqlStaffLoginBackupOperator);
					}
				}

				var adminTask = new DbSecurityAdminTask();
				adminTask.ServiceLogger = Mock.Of<ILogger>();

				// Act
				adminTask.RunTask(CancellationToken.None);

				// Assert
				foreach (var dbName in adminConnection.GetDatabases(DatabaseType.All & ~DatabaseType.SingleSharedRef))
				{
					using (ChangeDatabase(adminConnection, dbName))
					{
						AssertDatabaseMembershipExists(adminConnection, nameof(CwRestrictedReaderRole), sqlStaffLoginDeveloperAndReader);
						AssertDatabaseMembershipMissing(adminConnection, DbRoleTypes.DbBackupOperatorRole, sqlStaffLoginBackupOperator); //backup operator role should still be missing in wise cloud hosted environment
					}
				}
			}
			finally
			{
				Helper.DropServerPrincipals(
					adminConnection,
					sqlStaffLoginDeveloperAndReader,
					sqlStaffLoginBackupOperator);
			}
		}

		void DoesNotRestoreDroppedRequiredRoleMemberships(AdminConnection adminConnection)
		{
			// Arrange
			var factory = new BusinessObjectFactory(adminConnection);
			var staffDeveloperAndReaderName = "TestDeveloperAndReader";
			var staffDeveloperAndReader = factory.New<GlbStaff>();

			staffDeveloperAndReader.GS_FullName = staffDeveloperAndReaderName;
			staffDeveloperAndReader.GS_LoginName = staffDeveloperAndReaderName;
			staffDeveloperAndReader.GS_Code = "123";
			staffDeveloperAndReader.StaffPlainTextPassword = "TOPSECRET";

			staffDeveloperAndReader.IsDatabaseDeveloper = true;
			staffDeveloperAndReader.IsReadOnlyDBUser = true;

			var staffBackupOperatorName = "TestBackupOperator";
			var staffBackupOperator = factory.New<GlbStaff>();

			staffBackupOperator.GS_FullName = staffBackupOperatorName;
			staffBackupOperator.GS_LoginName = staffBackupOperatorName;
			staffBackupOperator.GS_Code = "12G";
			staffBackupOperator.StaffPlainTextPassword = "TOPSECRET";

			staffBackupOperator.IsBackupOperator = true;

			SetStaffPassword(staffDeveloperAndReader);
			SetStaffPassword(staffBackupOperator);

			var sqlStaffLoginDeveloperAndReader = $"{DbUserRepository.GetStaffDbLoginFullPrefix(Db.DatabaseName)}{staffDeveloperAndReader.GS_LoginName}";
			var sqlStaffLoginBackupOperator = $"{DbUserRepository.GetStaffDbLoginFullPrefix(Db.DatabaseName)}{staffBackupOperator.GS_LoginName}";

			try
			{
				factory.Save();

				foreach (var dbName in adminConnection.GetDatabases(DatabaseType.All & ~DatabaseType.SingleSharedRef & ~DatabaseType.EDW))
				{
					using (ChangeDatabase(adminConnection, dbName))
					{
						adminConnection.ExecuteNonQuery($@"
ALTER ROLE {nameof(CwRestrictedReaderRole).QuoteName()} DROP MEMBER {dbRestrictedReaderLogin.QuoteName()};
ALTER ROLE {nameof(CwUnrestrictedWriterRole).QuoteName()} DROP MEMBER {dbUnrestrictedWriterLogin.QuoteName()};

ALTER ROLE {DbRoleTypes.DbDataReaderRole.QuoteName()} DROP MEMBER {nameof(CwReaderRole).QuoteName()};

ALTER ROLE {DbRoleTypes.DbDataWriterRole.QuoteName()} DROP MEMBER {sqlStaffLoginDeveloperAndReader.QuoteName()};
ALTER ROLE {nameof(CwRestrictedReaderRole).QuoteName()} DROP MEMBER {sqlStaffLoginDeveloperAndReader.QuoteName()};

ALTER ROLE {DbRoleTypes.DbBackupOperatorRole.QuoteName()} DROP MEMBER {sqlStaffLoginBackupOperator.QuoteName()};
");
						AssertDatabaseMembershipMissing(adminConnection, nameof(CwRestrictedReaderRole), dbRestrictedReaderLogin);
						AssertDatabaseMembershipMissing(adminConnection, nameof(CwUnrestrictedWriterRole), dbUnrestrictedWriterLogin);
						AssertDatabaseMembershipMissing(adminConnection, DbRoleTypes.DbDataReaderRole, nameof(CwReaderRole));

						AssertDatabaseMembershipMissing(adminConnection, DbRoleTypes.DbDataWriterRole, sqlStaffLoginDeveloperAndReader);
						AssertDatabaseMembershipMissing(adminConnection, nameof(CwRestrictedReaderRole), sqlStaffLoginDeveloperAndReader);

						AssertDatabaseMembershipMissing(adminConnection, DbRoleTypes.DbBackupOperatorRole, sqlStaffLoginBackupOperator);
					}
				}

				var adminTask = new DbSecurityAdminTask();
				adminTask.ServiceLogger = Mock.Of<ILogger>();

				// Act
				adminTask.RunTask(CancellationToken.None);

				// Assert
				foreach (var dbName in adminConnection.GetDatabases(DatabaseType.All & ~DatabaseType.SingleSharedRef & ~DatabaseType.EDW))
				{
					using (ChangeDatabase(adminConnection, dbName))
					{
						AssertDatabaseMembershipMissing(adminConnection, nameof(CwRestrictedReaderRole), dbRestrictedReaderLogin);
						AssertDatabaseMembershipMissing(adminConnection, nameof(CwUnrestrictedWriterRole), dbUnrestrictedWriterLogin);
						AssertDatabaseMembershipMissing(adminConnection, DbRoleTypes.DbDataReaderRole, nameof(CwReaderRole));

						AssertDatabaseMembershipMissing(adminConnection, DbRoleTypes.DbDataWriterRole, sqlStaffLoginDeveloperAndReader);
						AssertDatabaseMembershipMissing(adminConnection, nameof(CwRestrictedReaderRole), sqlStaffLoginDeveloperAndReader);

						AssertDatabaseMembershipMissing(adminConnection, DbRoleTypes.DbBackupOperatorRole, sqlStaffLoginBackupOperator);
					}
				}
			}
			finally
			{
				Helper.DropServerPrincipals(
					adminConnection,
					sqlStaffLoginDeveloperAndReader,
					sqlStaffLoginBackupOperator);
			}
		}

		void DropsUnknownServerPrincipalsSatisfyingPattern(AdminConnection adminConnection)
		{
			// Arrange
			var loginLikeApplicationLoginToDropName = $"{Db.DatabaseName}_LoginToDrop";
			var loginLikeStaffLoginToDropName = $"{DbUserRepository.GetStaffDbLoginFullPrefix(Db.DatabaseName)}ToDrop";

			try
			{
				adminConnection.ExecuteNonQuery($@"
CREATE LOGIN [{loginLikeApplicationLoginToDropName}] WITH PASSWORD = N'SOME343[][rANDOMpWD';
CREATE LOGIN [{loginLikeStaffLoginToDropName}] WITH PASSWORD = N'SOME343[][rANDOMpWD';
");

				AssertServerPrincipalExistsAndIsNotDisabled(adminConnection, loginLikeApplicationLoginToDropName, "S");
				AssertServerPrincipalExistsAndIsNotDisabled(adminConnection, loginLikeStaffLoginToDropName, "S");

				var adminTask = new DbSecurityAdminTask();
				adminTask.ServiceLogger = Mock.Of<ILogger>();

				// Act
				adminTask.RunTask(CancellationToken.None);

				// Assert
				AssertServerPrincipalIsMissing(adminConnection, loginLikeApplicationLoginToDropName);
				AssertServerPrincipalIsMissing(adminConnection, loginLikeStaffLoginToDropName);
			}
			finally
			{
				Helper.DropServerPrincipals(
					adminConnection,
					loginLikeApplicationLoginToDropName,
					loginLikeStaffLoginToDropName);
			}
		}

		void DoesNotDropUnknownServerPrincipalsSatisfyingPattern(AdminConnection adminConnection)
		{
			// Arrange
			var loginLikeApplicationLoginToDropName = $"{Db.DatabaseName}_LoginToDrop";
			var loginLikeStaffLoginToDropName = $"{DbUserRepository.GetStaffDbLoginFullPrefix(Db.DatabaseName)}ToDrop";

			try
			{
				adminConnection.ExecuteNonQuery($@"
CREATE LOGIN [{loginLikeApplicationLoginToDropName}] WITH PASSWORD = N'SOME343[][rANDOMpWD';
CREATE LOGIN [{loginLikeStaffLoginToDropName}] WITH PASSWORD = N'SOME343[][rANDOMpWD';
");
				AssertServerPrincipalExistsAndIsNotDisabled(adminConnection, loginLikeApplicationLoginToDropName, "S");
				AssertServerPrincipalExistsAndIsNotDisabled(adminConnection, loginLikeStaffLoginToDropName, "S");

				var adminTask = new DbSecurityAdminTask();
				adminTask.ServiceLogger = Mock.Of<ILogger>();

				// Act
				adminTask.RunTask(CancellationToken.None);

				// Assert
				AssertServerPrincipalExistsAndIsNotDisabled(adminConnection, loginLikeApplicationLoginToDropName, "S");
				AssertServerPrincipalExistsAndIsNotDisabled(adminConnection, loginLikeStaffLoginToDropName, "S");
			}
			finally
			{
				Helper.DropServerPrincipals(
					adminConnection,
					loginLikeApplicationLoginToDropName,
					loginLikeStaffLoginToDropName);
			}
		}

		void DoesNotDropUnknownServerPrincipalsNotSatisfyingPattern(AdminConnection adminConnection)
		{
			// Arrange
			var loginNotLikeApplicationLoginToDropName = $"_Tst_{Db.DatabaseName}LoginToDrop";
			var loginNotLikeStaffLoginToDropName = $"_Tst_{DbUserRepository.GetStaffDbLoginFullPrefix(Db.DatabaseName)}ToDropButNotDropped";

			try
			{
				adminConnection.ExecuteNonQuery($@"
CREATE LOGIN [{loginNotLikeApplicationLoginToDropName}] WITH PASSWORD = N'SOME343[][rANDOMpWD';
CREATE LOGIN [{loginNotLikeStaffLoginToDropName}] WITH PASSWORD = N'SOME343[][rANDOMpWD';
");
				AssertServerPrincipalExistsAndIsNotDisabled(adminConnection, loginNotLikeApplicationLoginToDropName, "S");
				AssertServerPrincipalExistsAndIsNotDisabled(adminConnection, loginNotLikeStaffLoginToDropName, "S");

				var adminTask = new DbSecurityAdminTask();
				adminTask.ServiceLogger = Mock.Of<ILogger>();

				// Act
				adminTask.RunTask(CancellationToken.None);

				// Assert
				AssertServerPrincipalExistsAndIsNotDisabled(adminConnection, loginNotLikeApplicationLoginToDropName, "S");
				AssertServerPrincipalExistsAndIsNotDisabled(adminConnection, loginNotLikeStaffLoginToDropName, "S");
			}
			finally
			{
				Helper.DropServerPrincipals(
					adminConnection,
					loginNotLikeApplicationLoginToDropName,
					loginNotLikeStaffLoginToDropName);
			}
		}

		void DropsUnknownDatabaseUsersSatisfyingStaffUserPattern(IEnumerable<string> databases, AdminConnection adminConnection)
		{
			// Arrange
			var loginLikeStaffLoginToDropName = $"{DbUserRepository.GetStaffDbLoginFullPrefix(Db.DatabaseName)}ToDrop";

			try
			{
				adminConnection.ExecuteNonQuery($@"
CREATE LOGIN [{loginLikeStaffLoginToDropName}] WITH PASSWORD = N'SOME343[][rANDOMpWD';
");

				foreach (var database in databases)
				{
					using (ChangeDatabase(adminConnection, database))
					{
						adminConnection.ExecuteNonQuery($@"
CREATE USER [{loginLikeStaffLoginToDropName}] FROM LOGIN [{loginLikeStaffLoginToDropName}];
");
						AssertDatabasePrincipalExists(adminConnection, loginLikeStaffLoginToDropName, "S");
					}
				}

				var adminTask = new DbSecurityAdminTask();
				adminTask.ServiceLogger = Mock.Of<ILogger>();

				// Act
				adminTask.RunTask(CancellationToken.None);

				// Assert
				foreach (var database in databases)
				{
					using (ChangeDatabase(adminConnection, database))
					{
						AssertDatabasePrincipalIsMissing(adminConnection, loginLikeStaffLoginToDropName);
					}
				}
			}
			finally
			{
				Helper.DropServerPrincipals(adminConnection, loginLikeStaffLoginToDropName);
			}
		}

		void DropsUnknownDatabaseUsersSatisfyingApplicationUserPattern(IEnumerable<string> databases, AdminConnection adminConnection)
		{
			// Arrange
			var loginLikeApplicationLoginToDropName = $"{Db.DatabaseName}_LoginToDrop";

			try
			{
				adminConnection.ExecuteNonQuery($@"
CREATE LOGIN [{loginLikeApplicationLoginToDropName}] WITH PASSWORD = N'SOME343[][rANDOMpWD';
");

				foreach (var database in databases)
				{
					using (ChangeDatabase(adminConnection, database))
					{
						adminConnection.ExecuteNonQuery($@"
CREATE USER [{loginLikeApplicationLoginToDropName}] FROM LOGIN [{loginLikeApplicationLoginToDropName}];
");
						AssertDatabasePrincipalExists(adminConnection, loginLikeApplicationLoginToDropName, "S");
					}
				}

				var adminTask = new DbSecurityAdminTask();
				adminTask.ServiceLogger = Mock.Of<ILogger>();

				// Act
				adminTask.RunTask(CancellationToken.None);

				// Assert
				foreach (var database in databases)
				{
					using (ChangeDatabase(adminConnection, database))
					{
						AssertDatabasePrincipalIsMissing(adminConnection, loginLikeApplicationLoginToDropName);
					}
				}
			}
			finally
			{
				Helper.DropServerPrincipals(adminConnection, loginLikeApplicationLoginToDropName);
			}
		}

		void DoesNotDropUnknownDatabaseUsersSatisfyingStaffUserPattern(IEnumerable<string> databases, AdminConnection adminConnection)
		{
			// Arrange
			var loginLikeStaffLoginToDropName = $"{DbUserRepository.GetStaffDbLoginFullPrefix(Db.DatabaseName)}ToDropButNotDropped";

			try
			{
				adminConnection.ExecuteNonQuery($@"
CREATE LOGIN [{loginLikeStaffLoginToDropName}] WITH PASSWORD = N'SOME343[][rANDOMpWD';
");

				foreach (var database in databases)
				{
					using (ChangeDatabase(adminConnection, database))
					{
						adminConnection.ExecuteNonQuery($@"
CREATE USER [{loginLikeStaffLoginToDropName}] FROM LOGIN [{loginLikeStaffLoginToDropName}];
");
						AssertDatabasePrincipalExists(adminConnection, loginLikeStaffLoginToDropName, "S");
					}
				}

				var adminTask = new DbSecurityAdminTask();
				adminTask.ServiceLogger = Mock.Of<ILogger>();

				// Act
				adminTask.RunTask(CancellationToken.None);

				// Assert
				foreach (var database in databases)
				{
					using (ChangeDatabase(adminConnection, database))
					{
						AssertDatabasePrincipalExists(adminConnection, loginLikeStaffLoginToDropName, "S");
					}
				}
			}
			finally
			{
				Helper.DropServerPrincipals(adminConnection, loginLikeStaffLoginToDropName);
			}
		}

		void DoesNotDropUnknownDatabaseUsersSatisfyingApplicationUserPattern(IEnumerable<string> databases, AdminConnection adminConnection)
		{
			// Arrange
			var loginLikeApplicationLoginToDropName = $"{Db.DatabaseName}_LoginToDropButNotDropped";
			try
			{
				adminConnection.ExecuteNonQuery($@"
CREATE LOGIN [{loginLikeApplicationLoginToDropName}] WITH PASSWORD = N'SOME343[][rANDOMpWD';
");

				foreach (var database in databases)
				{
					using (ChangeDatabase(adminConnection, database))
					{
						adminConnection.ExecuteNonQuery($@"
CREATE USER [{loginLikeApplicationLoginToDropName}] FROM LOGIN [{loginLikeApplicationLoginToDropName}];
");
						AssertDatabasePrincipalExists(adminConnection, loginLikeApplicationLoginToDropName, "S");
					}
				}

				var adminTask = new DbSecurityAdminTask();
				adminTask.ServiceLogger = Mock.Of<ILogger>();

				// Act
				adminTask.RunTask(CancellationToken.None);

				// Assert
				foreach (var database in databases)
				{
					using (ChangeDatabase(adminConnection, database))
					{
						AssertDatabasePrincipalExists(adminConnection, loginLikeApplicationLoginToDropName, "S");
					}
				}
			}
			finally
			{
				Helper.DropServerPrincipals(adminConnection, loginLikeApplicationLoginToDropName);
			}
		}

		void DropsUnknownDatabaseRolesSatisfyingPattern(IEnumerable<string> databases, AdminConnection adminConnection)
		{
			// Arrange
			var roleToDrop = "cwToDropRole";

			foreach (var database in databases)
			{
				using (ChangeDatabase(adminConnection, database))
				{
					adminConnection.ExecuteNonQuery($@"
CREATE ROLE [{roleToDrop}];
");
					AssertDatabasePrincipalExists(adminConnection, roleToDrop, "R");
				}
			}

			var adminTask = new DbSecurityAdminTask();
			adminTask.ServiceLogger = Mock.Of<ILogger>();

			// Act
			adminTask.RunTask(CancellationToken.None);

			// Assert
			foreach (var database in databases)
			{
				using (ChangeDatabase(adminConnection, database))
				{
					AssertDatabasePrincipalIsMissing(adminConnection, roleToDrop);
				}
			}
		}

		void DoesNotDropUnknownDatabaseRolesSatisfyingPattern(IEnumerable<string> databases, AdminConnection adminConnection)
		{
			// Arrange
			var roleToDrop = "cwToDropRole";

			foreach (var database in databases)
			{
				using (ChangeDatabase(adminConnection, database))
				{
					adminConnection.ExecuteNonQuery($@"
CREATE ROLE [{roleToDrop}];
");
					AssertDatabasePrincipalExists(adminConnection, roleToDrop, "R");
				}
			}

			var adminTask = new DbSecurityAdminTask();
			adminTask.ServiceLogger = Mock.Of<ILogger>();

			// Act
			adminTask.RunTask(CancellationToken.None);

			// Assert
			foreach (var database in databases)
			{
				using (ChangeDatabase(adminConnection, database))
				{
					AssertDatabasePrincipalExists(adminConnection, roleToDrop, "R");
				}
			}
		}

		void DropsUnknownDatabasePrincipalsNotSatisfyingPattern(IEnumerable<string> databases, AdminConnection adminConnection)
		{
			// Arrange
			var loginNotLikeApplicationLoginToDropName = $"_Tst_{Db.DatabaseName}LoginToDrop";
			var loginNotLikeStaffLoginToDropName = $"_Tst_{DbUserRepository.GetStaffDbLoginFullPrefix(Db.DatabaseName)}ToDrop";

			try
			{
				var roleToDrop = "_Tst_CwToDropRole";
				adminConnection.ExecuteNonQuery($@"
CREATE LOGIN [{loginNotLikeApplicationLoginToDropName}] WITH PASSWORD = N'SOME343[][rANDOMpWD';
CREATE LOGIN [{loginNotLikeStaffLoginToDropName}] WITH PASSWORD = N'SOME343[][rANDOMpWD';
");

				foreach (var database in databases)
				{
					using (ChangeDatabase(adminConnection, database))
					{
						adminConnection.ExecuteNonQuery($@"
CREATE USER [{loginNotLikeApplicationLoginToDropName}] FROM LOGIN [{loginNotLikeApplicationLoginToDropName}];
CREATE USER [{loginNotLikeStaffLoginToDropName}] FROM LOGIN [{loginNotLikeStaffLoginToDropName}];
CREATE ROLE [{roleToDrop}];
");
						AssertDatabasePrincipalExists(adminConnection, loginNotLikeApplicationLoginToDropName, "S");
						AssertDatabasePrincipalExists(adminConnection, loginNotLikeStaffLoginToDropName, "S");
						AssertDatabasePrincipalExists(adminConnection, roleToDrop, "R");
					}
				}

				var adminTask = new DbSecurityAdminTask();
				adminTask.ServiceLogger = Mock.Of<ILogger>();

				// Act
				adminTask.RunTask(CancellationToken.None);

				// Assert
				foreach (var database in databases)
				{
					using (ChangeDatabase(adminConnection, database))
					{
						AssertDatabasePrincipalIsMissing(adminConnection, loginNotLikeApplicationLoginToDropName);
						AssertDatabasePrincipalIsMissing(adminConnection, loginNotLikeStaffLoginToDropName);
						AssertDatabasePrincipalIsMissing(adminConnection, roleToDrop);
					}
				}
			}
			finally
			{
				Helper.DropServerPrincipals(
					adminConnection,
					loginNotLikeApplicationLoginToDropName,
					loginNotLikeStaffLoginToDropName);
			}
		}

		void DoesNotDropUnknownDatabasePrincipalsNotSatisfyingPattern(IEnumerable<string> databases, AdminConnection adminConnection)
		{
			// Arrange
			var loginNotLikeApplicationLoginToDropName = $"_Tst_{Db.DatabaseName}LoginToDropButNotDropped";
			var loginNotLikeStaffLoginToDropName = $"_Tst_{DbUserRepository.GetStaffDbLoginFullPrefix(Db.DatabaseName)}ToDropButNotDropped";

			try
			{
				var roleToDrop = "_Tst_CwToDropRole";
				adminConnection.ExecuteNonQuery($@"
CREATE LOGIN [{loginNotLikeApplicationLoginToDropName}] WITH PASSWORD = N'SOME343[][rANDOMpWD';
CREATE LOGIN [{loginNotLikeStaffLoginToDropName}] WITH PASSWORD = N'SOME343[][rANDOMpWD';
");

				foreach (var database in databases)
				{
					using (ChangeDatabase(adminConnection, database))
					{
						adminConnection.ExecuteNonQuery($@"
CREATE USER [{loginNotLikeApplicationLoginToDropName}] FROM LOGIN [{loginNotLikeApplicationLoginToDropName}];
CREATE USER [{loginNotLikeStaffLoginToDropName}] FROM LOGIN [{loginNotLikeStaffLoginToDropName}];
CREATE ROLE [{roleToDrop}];
");
						AssertDatabasePrincipalExists(adminConnection, loginNotLikeApplicationLoginToDropName, "S");
						AssertDatabasePrincipalExists(adminConnection, loginNotLikeStaffLoginToDropName, "S");
						AssertDatabasePrincipalExists(adminConnection, roleToDrop, "R");
					}
				}

				var adminTask = new DbSecurityAdminTask();
				adminTask.ServiceLogger = Mock.Of<ILogger>();

				// Act
				adminTask.RunTask(CancellationToken.None);

				// Assert
				foreach (var database in databases)
				{
					using (ChangeDatabase(adminConnection, database))
					{
						AssertDatabasePrincipalExists(adminConnection, loginNotLikeApplicationLoginToDropName, "S");
						AssertDatabasePrincipalExists(adminConnection, loginNotLikeStaffLoginToDropName, "S");
						AssertDatabasePrincipalExists(adminConnection, roleToDrop, "R");
					}
				}
			}
			finally
			{
				Helper.DropServerPrincipals(
					adminConnection,
					loginNotLikeApplicationLoginToDropName,
					loginNotLikeStaffLoginToDropName);
			}
		}

		void RestoresDropedRequiredPrincipals(IEnumerable<string> databases, AdminConnection adminConnection)
		{
			// Arrange
			var factory = new BusinessObjectFactory(adminConnection);
			var staffDeveloperAndReaderName = "TestDeveloperAndReader";
			var staffDeveloperAndReader = factory.New<GlbStaff>();

			staffDeveloperAndReader.GS_FullName = staffDeveloperAndReaderName;
			staffDeveloperAndReader.GS_LoginName = staffDeveloperAndReaderName;
			staffDeveloperAndReader.GS_Code = "123";
			staffDeveloperAndReader.StaffPlainTextPassword = "TOPSECRET";

			staffDeveloperAndReader.IsDatabaseDeveloper = true;
			staffDeveloperAndReader.IsReadOnlyDBUser = true;

			var staffBackupOperatorName = "TestBackupOperator";
			var staffBackupOperator = factory.New<GlbStaff>();

			staffBackupOperator.GS_FullName = staffBackupOperatorName;
			staffBackupOperator.GS_LoginName = staffBackupOperatorName;
			staffBackupOperator.GS_Code = "12G";
			staffBackupOperator.StaffPlainTextPassword = "TOPSECRET";

			staffBackupOperator.IsBackupOperator = true;

			SetStaffPassword(staffDeveloperAndReader);
			SetStaffPassword(staffBackupOperator);

			var sqlStaffLoginDeveloperAndReader = $"{DbUserRepository.GetStaffDbLoginFullPrefix(Db.DatabaseName)}{staffDeveloperAndReader.GS_LoginName}";
			var sqlStaffLoginBackupOperator = $"{DbUserRepository.GetStaffDbLoginFullPrefix(Db.DatabaseName)}{staffBackupOperator.GS_LoginName}";

			try
			{
				factory.Save();

				Helper.DropServerPrincipals(
					adminConnection,
					dbUnrestrictedWriterLogin,
					sqlStaffLoginDeveloperAndReader,
					sqlStaffLoginBackupOperator);

				AssertServerPrincipalIsMissing(adminConnection, dbUnrestrictedWriterLogin);
				AssertServerPrincipalIsMissing(adminConnection, sqlStaffLoginDeveloperAndReader);
				AssertServerPrincipalIsMissing(adminConnection, sqlStaffLoginBackupOperator);

				foreach (var database in databases)
				{
					using (ChangeDatabase(adminConnection, database))
					{
						Helper.DropDatabasePrincipals(
							adminConnection,
							dbUnrestrictedWriterLogin,
							sqlStaffLoginDeveloperAndReader,
							sqlStaffLoginBackupOperator,
							nameof(CwUnrestrictedWriterRole));

						AssertDatabasePrincipalIsMissing(adminConnection, dbUnrestrictedWriterLogin);
						AssertDatabasePrincipalIsMissing(adminConnection, sqlStaffLoginDeveloperAndReader);
						AssertDatabasePrincipalIsMissing(adminConnection, sqlStaffLoginBackupOperator);
						AssertDatabasePrincipalIsMissing(adminConnection, nameof(CwUnrestrictedWriterRole));
					}
				}

				var adminTask = new DbSecurityAdminTask();
				adminTask.ServiceLogger = Mock.Of<ILogger>();

				// Act
				adminTask.RunTask(CancellationToken.None);

				// Assert
				AssertServerPrincipalExistsAndIsNotDisabled(adminConnection, dbUnrestrictedWriterLogin, "S");
				AssertServerPrincipalExistsAndIsNotDisabled(adminConnection, sqlStaffLoginDeveloperAndReader, "S");
				AssertServerPrincipalExistsAndIsNotDisabled(adminConnection, sqlStaffLoginBackupOperator, "S");

				foreach (var database in databases)
				{
					using (ChangeDatabase(adminConnection, database))
					{
						AssertDatabasePrincipalExists(adminConnection, dbUnrestrictedWriterLogin, "S");
						AssertDatabasePrincipalExists(adminConnection, sqlStaffLoginDeveloperAndReader, "S");
						AssertDatabasePrincipalExists(adminConnection, sqlStaffLoginBackupOperator, "S");
						AssertDatabasePrincipalExists(adminConnection, nameof(CwUnrestrictedWriterRole), "R");
					}
				}
			}
			finally
			{
				Helper.DropServerPrincipals(
					adminConnection,
					sqlStaffLoginDeveloperAndReader,
					sqlStaffLoginBackupOperator);
			}
		}

		void DoesNotRestoreDropedRequiredPrincipals(IEnumerable<string> databases, AdminConnection adminConnection)
		{
			// Arrange
			var factory = new BusinessObjectFactory(adminConnection);
			var staffDeveloperAndReaderName = "TestDeveloperAndReader";
			var staffDeveloperAndReader = factory.New<GlbStaff>();

			staffDeveloperAndReader.GS_FullName = staffDeveloperAndReaderName;
			staffDeveloperAndReader.GS_LoginName = staffDeveloperAndReaderName;
			staffDeveloperAndReader.GS_Code = "123";
			staffDeveloperAndReader.StaffPlainTextPassword = "TOPSECRET";

			staffDeveloperAndReader.IsDatabaseDeveloper = true;
			staffDeveloperAndReader.IsReadOnlyDBUser = true;

			var staffBackupOperatorName = "TestBackupOperator";
			var staffBackupOperator = factory.New<GlbStaff>();

			staffBackupOperator.GS_FullName = staffBackupOperatorName;
			staffBackupOperator.GS_LoginName = staffBackupOperatorName;
			staffBackupOperator.GS_Code = "12G";
			staffBackupOperator.StaffPlainTextPassword = "TOPSECRET";

			staffBackupOperator.IsBackupOperator = true;

			SetStaffPassword(staffDeveloperAndReader);
			SetStaffPassword(staffBackupOperator);

			var sqlStaffLoginDeveloperAndReader = $"{DbUserRepository.StaffDbLoginPrefix}_{Db.DatabaseName}_{staffDeveloperAndReader.GS_LoginName}";
			var sqlStaffLoginBackupOperator = $"{DbUserRepository.StaffDbLoginPrefix}_{Db.DatabaseName}_{staffBackupOperator.GS_LoginName}";

			try
			{
				factory.Save();

				Helper.DropServerPrincipals(
					adminConnection,
					dbUnrestrictedWriterLogin,
					sqlStaffLoginDeveloperAndReader,
					sqlStaffLoginBackupOperator);

				AssertServerPrincipalIsMissing(adminConnection, dbUnrestrictedWriterLogin);
				AssertServerPrincipalIsMissing(adminConnection, sqlStaffLoginDeveloperAndReader);
				AssertServerPrincipalIsMissing(adminConnection, sqlStaffLoginBackupOperator);

				foreach (var database in databases)
				{
					using (ChangeDatabase(adminConnection, database))
					{
						Helper.DropDatabasePrincipals(
							adminConnection,
							dbUnrestrictedWriterLogin,
							sqlStaffLoginDeveloperAndReader,
							sqlStaffLoginBackupOperator,
							nameof(CwUnrestrictedWriterRole));

						AssertDatabasePrincipalIsMissing(adminConnection, dbUnrestrictedWriterLogin);
						AssertDatabasePrincipalIsMissing(adminConnection, sqlStaffLoginDeveloperAndReader);
						AssertDatabasePrincipalIsMissing(adminConnection, sqlStaffLoginBackupOperator);
						AssertDatabasePrincipalIsMissing(adminConnection, nameof(CwUnrestrictedWriterRole));
					}
				}

				var adminTask = new DbSecurityAdminTask();
				adminTask.ServiceLogger = Mock.Of<ILogger>();

				// Act
				adminTask.RunTask(CancellationToken.None);

				// Assert
				AssertServerPrincipalIsMissing(adminConnection, dbUnrestrictedWriterLogin);
				AssertServerPrincipalIsMissing(adminConnection, sqlStaffLoginDeveloperAndReader);
				AssertServerPrincipalIsMissing(adminConnection, sqlStaffLoginBackupOperator);

				foreach (var database in databases)
				{
					using (ChangeDatabase(adminConnection, database))
					{
						AssertDatabasePrincipalIsMissing(adminConnection, dbUnrestrictedWriterLogin);
						AssertDatabasePrincipalIsMissing(adminConnection, sqlStaffLoginDeveloperAndReader);
						AssertDatabasePrincipalIsMissing(adminConnection, sqlStaffLoginBackupOperator);
						AssertDatabasePrincipalIsMissing(adminConnection, nameof(CwUnrestrictedWriterRole));
					}
				}
			}
			finally
			{
				Helper.DropServerPrincipals(
					adminConnection,
					sqlStaffLoginDeveloperAndReader,
					sqlStaffLoginBackupOperator);
			}
		}

		static void AssertServerPermissionExists(AdminConnection connection, string permissionState, string permissionName, string granteeName, string message = "")
		{
			var exists = connection.Exists(
				@"
FROM sys.server_permissions AS pm
	JOIN sys.server_principals AS grantee ON pm.grantee_principal_id = grantee.principal_id
WHERE 1=1
	AND pm.permission_name = @permissionName
	AND grantee.name       = @granteeName
	AND pm.state           = @permissionState
",
				command =>
				{
					command.AddParameter("@permissionName", SqlDbType.NVarChar, 128, permissionName);
					command.AddParameter("@granteeName", SqlDbType.NVarChar, 128, granteeName);
					command.AddParameter("@permissionState", SqlDbType.Char, 1, permissionState);
				});

			AssertEquals(!string.IsNullOrEmpty(message) ? message : $"Principal '{granteeName}' must have server permission '{permissionName}'.", true, exists);
		}

		static void AssertServerPermissionMissing(AdminConnection connection, string permissionState, string permissionName, string granteeName, string message = "")
		{
			var exists = connection.Exists(
				@"
FROM sys.server_permissions AS pm
	JOIN sys.server_principals AS grantee ON pm.grantee_principal_id = grantee.principal_id
WHERE 1=1
	AND pm.permission_name = @permissionName
	AND grantee.name       = @granteeName
	AND pm.state           = @permissionState
",
				command =>
				{
					command.AddParameter("@permissionName", SqlDbType.NVarChar, 128, permissionName);
					command.AddParameter("@granteeName", SqlDbType.NVarChar, 128, granteeName);
					command.AddParameter("@permissionState", SqlDbType.Char, 1, permissionState);
				});

			AssertEquals(!string.IsNullOrEmpty(message) ? message : $"Principal '{granteeName}' should have no server permission '{permissionName}' of state '{permissionState}'.", false, exists);
		}

		static void AssertDatabasePermissionExists(AdminConnection connection, string permissionState, string permissionName, string granteeName, string message = "")
		{
			var exists = connection.Exists(
				@"
FROM sys.database_permissions AS pm
	JOIN sys.database_principals AS grantee ON pm.grantee_principal_id = grantee.principal_id
WHERE 1=1
	AND pm.permission_name = @permissionName
	AND grantee.name       =  @granteeName
	AND pm.state           = @permissionState
",
				command =>
				{
					command.AddParameter("@permissionName", SqlDbType.NVarChar, 128, permissionName);
					command.AddParameter("@granteeName", SqlDbType.NVarChar, 128, granteeName);
					command.AddParameter("@permissionState", SqlDbType.Char, 1, permissionState);
				});

			AssertEquals(!string.IsNullOrEmpty(message) ? message : $"Principal '{granteeName}' must have database permission '{permissionName}' of type '{permissionState}' in the database '{connection.CurrentDatabase}'.", true, exists);
		}

		static void AssertDatabasePermissionMissing(AdminConnection connection, string permissionState, string permissionName, string granteeName, string message = "")
		{
			var exists = connection.Exists(
				@"
FROM sys.database_permissions AS pm
	JOIN sys.database_principals AS grantee ON pm.grantee_principal_id = grantee.principal_id
WHERE 1=1
	AND pm.permission_name = @permissionName
	AND grantee.name       = @granteeName
	AND pm.state           = @permissionState
",
				command =>
				{
					command.AddParameter("@permissionName", SqlDbType.NVarChar, 128, permissionName);
					command.AddParameter("@granteeName", SqlDbType.NVarChar, 128, granteeName);
					command.AddParameter("@permissionState", SqlDbType.Char, 1, permissionState);
				});

			AssertEquals(!string.IsNullOrEmpty(message) ? message : $"Principal '{granteeName}' should have no database permission '{permissionName}' of state '{permissionState}' in the database '{connection.CurrentDatabase}'.", false, exists);
		}

		static void AssertSchemaPermissionExists(AdminConnection connection, string permissionState, string permissionName, string schemaName, string granteeName, string message = "")
		{
			var exists = connection.Exists(
				@"
FROM sys.database_permissions AS pm
	JOIN sys.database_principals AS grantee ON pm.grantee_principal_id = grantee.principal_id
	JOIN sys.schemas AS sch ON sch.schema_id = pm.major_id
WHERE 1=1
	AND pm.permission_name = @permissionName
	AND sch.name           = @schemaName
	AND grantee.name       = @granteeName
	AND pm.state           = @permissionState
",
				command =>
				{
					command.AddParameter("@permissionName", SqlDbType.NVarChar, 128, permissionName);
					command.AddParameter("@granteeName", SqlDbType.NVarChar, 128, granteeName);
					command.AddParameter("@schemaName", SqlDbType.NVarChar, 128, schemaName);
					command.AddParameter("@permissionState", SqlDbType.Char, 1, permissionState);
				});

			AssertEquals(!string.IsNullOrEmpty(message) ? message : $"Principal '{granteeName}' must have permission '{permissionName}' on schema '{schemaName}' in the database '{connection.CurrentDatabase}'.", true, exists);
		}

		static void AssertSchemaPermissionMissing(AdminConnection connection, string permissionState, string permissionName, string schemaName, string granteeName, string message = "")
		{
			var exists = connection.Exists(
				@"
FROM sys.database_permissions AS pm
	JOIN sys.database_principals AS grantee ON pm.grantee_principal_id = grantee.principal_id
	JOIN sys.schemas AS sch ON sch.schema_id = pm.major_id
WHERE 1=1
	AND pm.permission_name = @permissionName
	AND sch.name           = @schemaName
	AND grantee.name       = @granteeName
	AND pm.state           = @permissionState
",
				command =>
				{
					command.AddParameter("@permissionName", SqlDbType.NVarChar, 128, permissionName);
					command.AddParameter("@granteeName", SqlDbType.NVarChar, 128, granteeName);
					command.AddParameter("@schemaName", SqlDbType.NVarChar, 128, schemaName);
					command.AddParameter("@permissionState", SqlDbType.Char, 1, permissionState);
				});

			AssertEquals(!string.IsNullOrEmpty(message) ? message : $"Principal '{granteeName}' should have no permission '{permissionName}' of state '{permissionState}' on schema '{schemaName}' in the database '{connection.CurrentDatabase}'.", false, exists);
		}

		static void AssertDatabaseMembershipExists(AdminConnection connection, string roleName, string memberName, string message = "")
		{
			var exists = connection.Exists(
				@"
FROM sys.database_role_members AS rm
	JOIN sys.database_principals AS member ON member.principal_id = member_principal_id
	JOIN sys.database_principals AS role ON role.principal_id = role_principal_id
WHERE 1=1
	AND member.name = @memberName
	AND role.name   =  @roleName
",
				command =>
				{
					command.AddParameter("@memberName", SqlDbType.NVarChar, 128, memberName);
					command.AddParameter("@roleName", SqlDbType.NVarChar, 128, roleName);
				});

			AssertEquals(!string.IsNullOrEmpty(message) ? message : $"Principal '{memberName}' must be a member of role '{roleName}' in the database '{connection.CurrentDatabase}'.", true, exists);
		}

		static void AssertDatabaseMembershipMissing(AdminConnection connection, string roleName, string memberName, string message = "")
		{
			var exists = connection.Exists(
				@"
FROM sys.database_role_members AS rm
	JOIN sys.database_principals AS member ON member.principal_id = member_principal_id
	JOIN sys.database_principals AS role ON role.principal_id = role_principal_id
WHERE 1=1
	AND member.name = @memberName
	AND role.name   =  @roleName
",
				command =>
				{
					command.AddParameter("@memberName", SqlDbType.NVarChar, 128, memberName);
					command.AddParameter("@roleName", SqlDbType.NVarChar, 128, roleName);
				});

			AssertEquals(!string.IsNullOrEmpty(message) ? message : $"Principal '{memberName}' must not be a member of role '{roleName}' in the database '{connection.CurrentDatabase}'.", false, exists);
		}

		static void AssertServerMembershipExists(AdminConnection connection, string roleName, string memberName)
		{
			var exists = connection.Exists(
				@"
FROM sys.server_role_members AS rm
	JOIN sys.server_principals AS member ON member.principal_id = member_principal_id
	JOIN sys.server_principals AS role ON role.principal_id = role_principal_id
WHERE 1=1
	AND member.name = @memberName
	AND role.name   =  @roleName
",
				command =>
				{
					command.AddParameter("@memberName", SqlDbType.NVarChar, 128, memberName);
					command.AddParameter("@roleName", SqlDbType.NVarChar, 128, roleName);
				});

			AssertEquals($"Server principal '{memberName}' must be a member of role '{roleName}'.", true, exists);
		}

		static void AssertServerMembershipMissing(AdminConnection connection, string roleName, string memberName)
		{
			var exists = connection.Exists(
				@"
FROM sys.server_role_members AS rm
	JOIN sys.server_principals AS member ON member.principal_id = member_principal_id
	JOIN sys.server_principals AS role ON role.principal_id = role_principal_id
WHERE 1=1
	AND member.name = @memberName
	AND role.name   =  @roleName
",
				command =>
				{
					command.AddParameter("@memberName", SqlDbType.NVarChar, 128, memberName);
					command.AddParameter("@roleName", SqlDbType.NVarChar, 128, roleName);
				});

			AssertEquals($"Server principal '{memberName}' must not be a member of role '{roleName}'.", false, exists);
		}

		static void AssertServerPrincipalExistsAndIsNotDisabled(AdminConnection connection, string principalName, string type, bool caseSensitive = false)
		{
			var actualType = connection.ExecuteScalar<string>(
				$"SELECT type FROM sys.server_principals WHERE name = @principalName{(caseSensitive ? $" COLLATE {Db.DatabaseCaseSensitiveCollation}" : string.Empty)} AND is_disabled = 0",
				command => command.AddParameter("@principalName", SqlDbType.NVarChar, 128, principalName));

			AssertEquals($"Server principal '{principalName}' of type '{type}' should be be present.", type, actualType);
		}

		static void AssertServerPrincipalIsMissing(AdminConnection connection, string principalName, string message = "", bool caseSensitive = false)
		{
			var exists = connection.Exists(
				$"FROM sys.server_principals WHERE name = @principalName{(caseSensitive ? $" COLLATE {Db.DatabaseCaseSensitiveCollation}" : string.Empty)} AND is_disabled = 0",
				command => command.AddParameter("@principalName", SqlDbType.NVarChar, 128, principalName));

			AssertEquals(!string.IsNullOrEmpty(message) ? message : $"Server principal '{principalName}' should not be present.", exists, false);
		}

		static void AssertDatabasePrincipalExists(AdminConnection connection, string principalName, string type)
		{
			var actualType = connection.ExecuteScalar<string>(
				$"SELECT type FROM sys.database_principals WHERE name = @principalName",
				command => command.AddParameter("@principalName", SqlDbType.NVarChar, 128, principalName));

			AssertEquals($"Database principal '{principalName}' of type '{type}' should be be present in database '{connection.CurrentDatabase}'.", type, actualType);
		}

		static void AssertDatabasePrincipalIsMissing(AdminConnection connection, string principalName, string message = "")
		{
			var exists = connection.Exists(
				"FROM sys.database_principals WHERE name = @principalName",
				command => command.AddParameter("@principalName", SqlDbType.NVarChar, 128, principalName));

			AssertEquals(!string.IsNullOrEmpty(message) ? message : $"Database principal '{principalName}' should not be present in database '{connection.CurrentDatabase}'.", exists, false);
		}

		internal static void SetDomainCredentialCollection()
		{
			var domainCredentials = ObjectFactory.New<IDomainCredentials>();
			domainCredentials.DomainName = TestConstants.Domain;
			domainCredentials.DomainUserName = TestConstants.ADTestUserAccount.Name;
			domainCredentials.DomainUserPassword = TestConstants.ADTestUserAccount.Password;
			domainCredentials.IsDefaultDomain = true;
			domainCredentials.UserOrganisationalUnit = TestConstants.ValidOU;
			domainCredentials.GroupOrganisationalUnit = TestConstants.ValidOU;
			domainCredentials.DefaultPassword = "Changeme12345";

			ObjectFactory.Get<IADRegistry>().DefaultDomainCredentials = domainCredentials;
		}

		#endregion Principals synchronisation helpers

		IDisposable dropUserRepository;
		IDisposable dropTestSharedRefDb;
		IDisposable resetIsTestForTest;

		string TestSharedRefDbName => $"CW-RefDb-Xxx-ZZ-000000-{nameof(DbSecurityAdminTaskSecurityTest)}";

		protected override void SetUp()
		{
			base.SetUp();
			using (var connection = Db.NewAdminConnection())
			{
				// make sure we have all databases created and that we may need for testing BEFORE transaction scope open
				var userRepositoryDb = $"{Db.DatabaseName}{DbUserRepository.RepositoryDbSuffix}";
				if (!connection.DatabaseExists(userRepositoryDb))
				{
					dropUserRepository = new DisposableAction(() =>
					{
						using (var adminConnection = Db.NewAdminConnection())
						{
							Helper.EnsureMsdbDeniedRoleAndUsersDropped(adminConnection);
							AdoTestUtils.DropDbIfExists(adminConnection, userRepositoryDb);
						}
					});

					(new DbUserRepository()).CreateRepositoryDatabase();
				}

				dropTestSharedRefDb = AdoTestUtils.CreateDbDropExistingDisposable(TestSharedRefDbName, Db.DatabaseName);
				resetIsTestForTest = Globals.TemporaryOverrideForIsTest(true);
			}

			using (var adminConnectiobn = Db.NewAdminConnection())
			{
				Helper.CreateSynonym(adminConnectiobn, $"RefDb{TestSharedRefDbName}", TestSharedRefDbName, "Test", Db.DatabaseName);

				var sqlSecurityManager = new SqlSecurityManager(Mock.Of<ILogger>(), Db.DatabaseName, allowTransaction: false);
				sqlSecurityManager.BuildSecurity(adminConnectiobn, CancellationToken.None, trialRun: false);
			}
		}

		protected override void TearDown()
		{
			dropUserRepository?.Dispose();
			dropTestSharedRefDb?.Dispose();
			resetIsTestForTest?.Dispose();

			// Clear stale state: Tests that call GetAllDatabases would get TestSharedRefDbName from the cache, even though it has been deleted.
			Db.Connection.ClearRefDbNameBuffers_ForTest();

			base.TearDown();
		}

		IDisposable ChangeDatabase(AdminConnection connection, string newDatabaseName)
		{
			var previousDatabase = connection.CurrentDatabase;
			((ICurrentDbControl)connection).UseDatabase(newDatabaseName);

			return new DisposableAction(() =>
			{
				try
				{
					((ICurrentDbControl)connection).UseDatabase(previousDatabase);
				}
				catch (InvalidOperationException ex) when (ex.Message.Contains("The transaction associated with the current connection has completed but has not been disposed", StringComparison.OrdinalIgnoreCase))
				{
					// ignore those errors to allow actual error to show
				}
			});
		}

		#endregion Implementation

	}
}
