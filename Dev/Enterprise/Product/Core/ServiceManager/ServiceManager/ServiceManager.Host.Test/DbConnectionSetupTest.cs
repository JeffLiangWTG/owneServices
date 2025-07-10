#if NET48_OR_GREATER //There appears to be issues with SID contention when this unit test was multi-targeted, the Architecture Core team will take a look in CS01872231
using System;
using System.Text;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Database.Abstractions;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;
using ServiceManager.Host.Abstractions;
using ServiceManager.Logging.Abstractions;
using WTG.Data.SqlDbSecuritySynchroniser;

namespace Enterprise.ServiceManager.Host.Testing
{
	[UseSnapshotProtection]
	[NonParallelizable]
	class DbConnectionSetupTest : TestCase
	{
		public void TestCreatesRestrictedWriterLoginAndUsersIfMissingWhenModernSecuritySystemIsOff()
		{
			EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = false;

			//SL: users are not corrected on EDW in old functionality
			TestRecreatesMissingRestrictedWriterLoginAndUsers(databaseTypesToInclude: DatabaseType.All & ~DatabaseType.BI & ~DatabaseType.SingleSharedRef);
		}

		public void TestCreatesRestrictedWriterLoginAndUsersIfMissingWhenModernSecuritySystemIsOn()
		{
			EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = true;

			// modern security does not create users in single ref database
			TestRecreatesMissingRestrictedWriterLoginAndUsers(databaseTypesToInclude: DatabaseType.All & ~DatabaseType.SingleSharedRef);
		}

		public void TestCreatesRestrictedWriterLoginUserIfMissingInMainDbWhenModernSecuritySystemIsOff()
		{
			EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = false;

			//SL: users are not corrected on EDW in old functionality
			TestRecreatesMissingRestrictedWriterLoginUserIfMissingInMainDb(databaseTypesToInclude: DatabaseType.All & ~DatabaseType.EDW);
		}

		public void TestCreatesRestrictedWriterLoginUserIfMissingInMainDbWhenModernSecuritySystemIsOn()
		{
			EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = true;

			// modern security does not create users in single ref database
			TestRecreatesMissingRestrictedWriterLoginUserIfMissingInMainDb(databaseTypesToInclude: DatabaseType.All & ~DatabaseType.SingleSharedRef);
		}

		public void TestCreatesRestrictedWriterLoginIfMissingWhenModernSecuritySystemIsOff()
		{
			EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = false;

			//SL: users are not corrected on EDW in old functionality
			TestRecreatesMissingRestrictedWriterLogin(databaseTypesToInclude: DatabaseType.All & ~DatabaseType.EDW);
		}

		public void TestCreatesRestrictedWriterLoginIfMissingWhenModernSecuritySystemIsOn()
		{
			EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = true;

			// modern security does not create users in single ref database
			TestRecreatesMissingRestrictedWriterLogin(databaseTypesToInclude: DatabaseType.All & ~DatabaseType.SingleSharedRef);
		}

		public void TestCreatesAllMissingApplicationLoginsAndUsersIfRestrictedWriterLoginIsMissingWhenModernSecuritySystemIsOn()
		{
			// Arrange
			EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = true;

			var databases = Db.Connection.GetDatabases(DatabaseType.All & ~DatabaseType.SingleSharedRef);

			Db.Connection.Dispose();
			var sqlCommandToGetServerPrincipalName = $"SELECT TOP 1 name FROM sys.server_principals WHERE name = @principalName and type = @type";
			var sqlCommandToGetDatabasePrincipalName = $"SELECT TOP 1 name FROM sys.database_principals WHERE name = @principalName and type = @type";

			using (var adminConnection = Db.NewAdminConnection())
			{
				DropServerPrincipals(adminConnection, $"{Db.DatabaseName}[_]%");

				NUnit.Framework.Assert.That(adminConnection
					.ExecuteScalar(sqlCommandToGetServerPrincipalName,
					cmd =>
					{
						cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, $"{Db.DatabaseName}_RestrictedWriterLogin");
						cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "S");
					}), Is.EqualTo(default(object)), $"Login '{Db.DatabaseName}_RestrictedWriterLogin' should be missing.");
				NUnit.Framework.Assert.That(adminConnection
					.ExecuteScalar(sqlCommandToGetServerPrincipalName,
					cmd =>
					{
						cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, $"{Db.DatabaseName}_RestrictedReaderLogin");
						cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "S");
					}), Is.EqualTo(default(object)), $"Login '{Db.DatabaseName}_RestrictedReaderLogin' should be missing.");
				NUnit.Framework.Assert.That(adminConnection
					.ExecuteScalar(sqlCommandToGetServerPrincipalName,
					cmd =>
					{
						cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, $"{Db.DatabaseName}_UnrestrictedWriterLogin");
						cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "S");
					}), Is.EqualTo(default(object)), $"Login '{Db.DatabaseName}_UnrestrictedWriterLogin' should be missing.");
				NUnit.Framework.Assert.That(adminConnection
					.ExecuteScalar(sqlCommandToGetServerPrincipalName,
					cmd =>
					{
						cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, $"{Db.DatabaseName}_CargoWiseReaderLogin");
						cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "S");
					}), Is.EqualTo(default(object)), $"Login '{Db.DatabaseName}_CargoWiseReaderLogin' should be missing.");
				NUnit.Framework.Assert.That(adminConnection
					.ExecuteScalar(sqlCommandToGetServerPrincipalName,
					cmd =>
					{
						cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, $"{Db.DatabaseName}_CargoWiseWriterLogin");
						cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "S");
					}), Is.EqualTo(default(object)), $"Login '{Db.DatabaseName}_CargoWiseWriterLogin' should be missing.");

				foreach (var dbName in databases)
				{
					using (((ICurrentDbControl)adminConnection).UseDatabase(dbName))
					{
						DropDatabasePrincipals(adminConnection, "cwHrmStaffRole");
						DropDatabasePrincipals(adminConnection, "cwRestrictedReaderRole");
						DropDatabasePrincipals(adminConnection, "cwRestrictedWriterRole");
						DropDatabasePrincipals(adminConnection, "cwUnrestrictedWriterRole");

						DropDatabasePrincipals(adminConnection, $"{Db.DatabaseName}[_]RestrictedReaderLogin");
						DropDatabasePrincipals(adminConnection, $"{Db.DatabaseName}[_]UnrestrictedWriterLogin");
						DropDatabasePrincipals(adminConnection, $"{Db.DatabaseName}[_]CargoWiseReaderLogin");
						DropDatabasePrincipals(adminConnection, $"{Db.DatabaseName}[_]CargoWiseWriterLogin");

						NUnit.Framework.Assert.That(adminConnection
							.ExecuteScalar<string>(sqlCommandToGetDatabasePrincipalName,
							cmd =>
							{
								cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, $"{Db.DatabaseName}_RestrictedWriterLogin");
								cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "S");
							}), Is.EqualTo($"{Db.DatabaseName}_RestrictedWriterLogin"), $"User '{Db.DatabaseName}_RestrictedWriterLogin' should be present on database '{dbName}'.");

						NUnit.Framework.Assert.That(adminConnection
							.ExecuteScalar(sqlCommandToGetDatabasePrincipalName,
							cmd =>
							{
								cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, $"{Db.DatabaseName}_RestrictedReaderLogin");
								cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "S");
							}), Is.EqualTo(default(object)), $"User '{Db.DatabaseName}_RestrictedReaderLogin' should be missing on database '{dbName}'.");
						NUnit.Framework.Assert.That(adminConnection
							.ExecuteScalar(sqlCommandToGetDatabasePrincipalName,
							cmd =>
							{
								cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, $"{Db.DatabaseName}_UnrestrictedWriterLogin");
								cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "S");
							}), Is.EqualTo(default(object)), $"User '{Db.DatabaseName}_UnrestrictedWriterLogin' should be missing on database '{dbName}'.");
						NUnit.Framework.Assert.That(adminConnection
							.ExecuteScalar(sqlCommandToGetDatabasePrincipalName,
							cmd =>
							{
								cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, $"{Db.DatabaseName}_CargoWiseReaderLogin");
								cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "S");
							}), Is.EqualTo(default(object)), $"User '{Db.DatabaseName}_CargoWiseReaderLogin' should be missing on database '{dbName}'.");
						NUnit.Framework.Assert.That(adminConnection
							.ExecuteScalar(sqlCommandToGetDatabasePrincipalName,
							cmd =>
							{
								cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, $"{Db.DatabaseName}_CargoWiseWriterLogin");
								cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "S");
							}), Is.EqualTo(default(object)), $"User '{Db.DatabaseName}_CargoWiseWriterLogin' should be missing on database '{dbName}'.");

						NUnit.Framework.Assert.That(adminConnection
							.ExecuteScalar(sqlCommandToGetDatabasePrincipalName,
							cmd =>
							{
								cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, "cwRestrictedReaderRole");
								cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "R");
							}), Is.EqualTo(default(object)), $"Role 'cwRestrictedReaderRole' should be missing on database '{dbName}'.");

						NUnit.Framework.Assert.That(adminConnection
							.ExecuteScalar(sqlCommandToGetDatabasePrincipalName,
							cmd =>
							{
								cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, "cwRestrictedWriterRole");
								cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "R");
							}), Is.EqualTo(default(object)), $"Role 'cwRestrictedWriterRole' should be missing on database '{dbName}'.");
						NUnit.Framework.Assert.That(adminConnection
							.ExecuteScalar(sqlCommandToGetDatabasePrincipalName,
							cmd =>
							{
								cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, "cwUnrestrictedWriterRole");
								cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "R");
							}), Is.EqualTo(default(object)), $"Role 'cwUnrestrictedWriterRole' should be missing on database '{dbName}'.");
						NUnit.Framework.Assert.That(adminConnection
							.ExecuteScalar(sqlCommandToGetDatabasePrincipalName,
							cmd =>
							{
								cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, "cwHRMStaffRole");
								cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "R");
							}), Is.EqualTo(default(object)), $"Role 'cwHRMStaffRole' should be missing on database '{dbName}'.");
					}
				}
			}

			Db.ConnectionOverrideForTest = Db.NewExtraConnectionWithMainDbCredentials(Db.ServerName, Db.DatabaseName);

			AssertExceptionThrown<SqlException>($"Should not be able to open connection as Login '{Db.DatabaseName}_RestrictedWriterLogin' is missing.", () => Db.Connection.EnsureIsOpen());

			// Act
			var connectionSetupErrorHander = new DbConnectionSetup(Mock.Of<IEventLogger>(), Mock.Of<ICancellationTokenProvider>());
			connectionSetupErrorHander.TryConnectAndHandleErrors();

			// Assert
			AssertNoExceptionThrown($"Should be able to open connection after a call to TryConnectAndHandleErrors.", () => Db.Connection.EnsureIsOpen());

			using (var adminConnection = Db.NewAdminConnection())
			{
				NUnit.Framework.Assert.That(adminConnection
				.ExecuteScalar<string>(sqlCommandToGetServerPrincipalName,
				cmd =>
				{
					cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, $"{Db.DatabaseName}_RestrictedWriterLogin");
					cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "S");
				}), Is.EqualTo($"{Db.DatabaseName}_RestrictedWriterLogin"), $"Login '{Db.DatabaseName}_RestrictedWriterLogin' should be created");
				NUnit.Framework.Assert.That(adminConnection
					.ExecuteScalar<string>(sqlCommandToGetServerPrincipalName,
					cmd =>
					{
						cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, $"{Db.DatabaseName}_RestrictedReaderLogin");
						cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "S");
					}), Is.EqualTo($"{Db.DatabaseName}_RestrictedReaderLogin"), $"Login '{Db.DatabaseName}_RestrictedReaderLogin' should be created.");
				NUnit.Framework.Assert.That(adminConnection
					.ExecuteScalar<string>(sqlCommandToGetServerPrincipalName,
					cmd =>
					{
						cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, $"{Db.DatabaseName}_UnrestrictedWriterLogin");
						cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "S");
					}), Is.EqualTo($"{Db.DatabaseName}_UnrestrictedWriterLogin"), $"Login '{Db.DatabaseName}_UnrestrictedWriterLogin' should be created.");
				NUnit.Framework.Assert.That(adminConnection
					.ExecuteScalar<string>(sqlCommandToGetServerPrincipalName,
					cmd =>
					{
						cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, $"{Db.DatabaseName}_CargoWiseReaderLogin");
						cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "S");
					}), Is.EqualTo($"{Db.DatabaseName}_CargoWiseReaderLogin"), $"Login '{Db.DatabaseName}_CargoWiseReaderLogin' should be created.");
				NUnit.Framework.Assert.That(adminConnection
					.ExecuteScalar<string>(sqlCommandToGetServerPrincipalName,
					cmd =>
					{
						cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, $"{Db.DatabaseName}_CargoWiseWriterLogin");
						cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "S");
					}), Is.EqualTo($"{Db.DatabaseName}_CargoWiseWriterLogin"), $"Login '{Db.DatabaseName}_CargoWiseWriterLogin' should be created.");
			}

			foreach (var dbName in databases)
			{
				using (((ICurrentDbControl)Db.Connection).UseDatabase(dbName))
				{
					NUnit.Framework.Assert.That(Db.Connection
						.ExecuteScalar<string>(sqlCommandToGetDatabasePrincipalName,
						cmd =>
						{
							cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, $"{Db.DatabaseName}_RestrictedWriterLogin");
							cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "S");
						}), Is.EqualTo($"{Db.DatabaseName}_RestrictedWriterLogin"), $"User '{Db.DatabaseName}_RestrictedWriterLogin' should be present on '{dbName}'.");
					NUnit.Framework.Assert.That(Db.Connection
						.ExecuteScalar<string>(sqlCommandToGetDatabasePrincipalName,
						cmd =>
						{
							cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, $"{Db.DatabaseName}_RestrictedReaderLogin");
							cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "S");
						}), Is.EqualTo($"{Db.DatabaseName}_RestrictedReaderLogin"), $"User '{Db.DatabaseName}_RestrictedReaderLogin' should be created on '{dbName}'.");
					NUnit.Framework.Assert.That(Db.Connection
						.ExecuteScalar<string>(sqlCommandToGetDatabasePrincipalName,
						cmd =>
						{
							cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, $"{Db.DatabaseName}_UnrestrictedWriterLogin");
							cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "S");
						}), Is.EqualTo($"{Db.DatabaseName}_UnrestrictedWriterLogin"), $"User '{Db.DatabaseName}_UnrestrictedWriterLogin' should be created on '{dbName}'.");
					NUnit.Framework.Assert.That(Db.Connection
						.ExecuteScalar<string>(sqlCommandToGetDatabasePrincipalName,
						cmd =>
						{
							cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, $"{Db.DatabaseName}_CargoWiseReaderLogin");
							cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "S");
						}), Is.EqualTo($"{Db.DatabaseName}_CargoWiseReaderLogin"), $"User '{Db.DatabaseName}_CargoWiseReaderLogin' should be created on '{dbName}'.");
					NUnit.Framework.Assert.That(Db.Connection
						.ExecuteScalar<string>(sqlCommandToGetDatabasePrincipalName,
						cmd =>
						{
							cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, $"{Db.DatabaseName}_CargoWiseWriterLogin");
							cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "S");
						}), Is.EqualTo($"{Db.DatabaseName}_CargoWiseWriterLogin"), $"User '{Db.DatabaseName}_CargoWiseWriterLogin' should be created on '{dbName}'.");

					NUnit.Framework.Assert.That(Db.Connection
						.ExecuteScalar<string>(sqlCommandToGetDatabasePrincipalName,
						cmd =>
						{
							cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, "cwRestrictedReaderRole");
							cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "R");
						}).ToLower(), Is.EqualTo("cwRestrictedReaderRole".ToLower()), $"Role 'cwRestrictedReaderRole' should be created on '{dbName}'.");
					NUnit.Framework.Assert.That(Db.Connection
						.ExecuteScalar<string>(sqlCommandToGetDatabasePrincipalName,
						cmd =>
						{
							cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, "cwRestrictedWriterRole");
							cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "R");
						}).ToLower(), Is.EqualTo("cwRestrictedWriterRole".ToLower()), $"Role 'cwRestrictedWriterRole' should be created on '{dbName}'.");
					NUnit.Framework.Assert.That(Db.Connection
						.ExecuteScalar<string>(sqlCommandToGetDatabasePrincipalName,
						cmd =>
						{
							cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, "cwUnrestrictedWriterRole");
							cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "R");
						}).ToLower(), Is.EqualTo("cwUnrestrictedWriterRole".ToLower()), $"Role 'cwUnrestrictedWriterRole' should be created on '{dbName}'.");
					NUnit.Framework.Assert.That(Db.Connection
						.ExecuteScalar<string>(sqlCommandToGetDatabasePrincipalName,
						cmd =>
						{
							cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, "cwHRMStaffRole");
							cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "R");
						}).ToLower(), Is.EqualTo("cwHRMStaffRole".ToLower()), $"Role 'cwHRMStaffRole' should be created on '{dbName}'.");
				}
			}
		}

		public void TestCreatesAllMissingApplicationLoginsAndUsersIfRestrictedWriterLoginUserIsMissingOnMainDbWhenModernSecuritySystemIsOn()
		{
			EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = true;

			var databases = Db.Connection.GetDatabases(DatabaseType.All & ~DatabaseType.SingleSharedRef);

			Db.Connection.Dispose();
			var sqlCommandToGetServerPrincipalName = $"SELECT TOP 1 name FROM sys.server_principals WHERE name = @principalName and type = @type";
			var sqlCommandToGetDatabasePrincipalName = $"SELECT TOP 1 name FROM sys.database_principals WHERE name = @principalName and type = @type";

			using (var adminConnection = Db.NewAdminConnection())
			{
				DropServerPrincipals(adminConnection, $"{Db.DatabaseName}[_]RestrictedReaderLogin");
				DropServerPrincipals(adminConnection, $"{Db.DatabaseName}[_]UnrestrictedWriterLogin");
				DropServerPrincipals(adminConnection, $"{Db.DatabaseName}[_]CargoWiseReaderLogin");
				DropServerPrincipals(adminConnection, $"{Db.DatabaseName}[_]CargoWiseWriterLogin");
				NUnit.Framework.Assert.That(adminConnection
					.ExecuteScalar<string>(sqlCommandToGetServerPrincipalName,
					cmd =>
					{
						cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, $"{Db.DatabaseName}_RestrictedWriterLogin");
						cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "S");
					}), Is.EqualTo($"{Db.DatabaseName}_RestrictedWriterLogin"), $"Login '{Db.DatabaseName}_RestrictedWriterLogin' should be present.");

				NUnit.Framework.Assert.That(adminConnection
					.ExecuteScalar(sqlCommandToGetServerPrincipalName,
					cmd =>
					{
						cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, $"{Db.DatabaseName}_RestrictedReaderLogin");
						cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "S");
					}), Is.EqualTo(default(object)), $"Login '{Db.DatabaseName}_RestrictedReaderLogin' should be missing");
				NUnit.Framework.Assert.That(adminConnection
					.ExecuteScalar(sqlCommandToGetServerPrincipalName,
					cmd =>
					{
						cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, $"{Db.DatabaseName}_UnrestrictedWriterLogin");
						cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "S");
					}), Is.EqualTo(default(object)), $"Login '{Db.DatabaseName}_UnrestrictedWriterLogin' should be missing");
				NUnit.Framework.Assert.That(adminConnection
					.ExecuteScalar(sqlCommandToGetServerPrincipalName,
					cmd =>
					{
						cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, $"{Db.DatabaseName}_CargoWiseReaderLogin");
						cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "S");
					}), Is.EqualTo(default(object)), $"Login '{Db.DatabaseName}_CargoWiseReaderLogin' should be missing");
				NUnit.Framework.Assert.That(adminConnection
					.ExecuteScalar(sqlCommandToGetServerPrincipalName,
					cmd =>
					{
						cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, $"{Db.DatabaseName}_CargoWiseWriterLogin");
						cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "S");
					}), Is.EqualTo(default(object)), $"Login '{Db.DatabaseName}_CargoWiseWriterLogin' should be missing");

				foreach (var dbName in databases)
				{
					using (((ICurrentDbControl)adminConnection).UseDatabase(dbName))
					{
						DropDatabasePrincipals(adminConnection, "cwHrmStaffRole");
						DropDatabasePrincipals(adminConnection, "cwRestrictedReaderRole");
						DropDatabasePrincipals(adminConnection, "cwRestrictedWriterRole");
						DropDatabasePrincipals(adminConnection, "cwUnrestrictedWriterRole");

						DropDatabasePrincipals(adminConnection, $"{Db.DatabaseName}[_]%");

						NUnit.Framework.Assert.That(adminConnection
							.ExecuteScalar(sqlCommandToGetDatabasePrincipalName,
							cmd =>
							{
								cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, $"{Db.DatabaseName}_RestrictedWriterLogin");
								cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "S");
							}), Is.EqualTo(default(object)), $"User '{Db.DatabaseName}_RestrictedWriterLogin' should be missing");

						NUnit.Framework.Assert.That(adminConnection
							.ExecuteScalar(sqlCommandToGetDatabasePrincipalName,
							cmd =>
							{
								cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, $"{Db.DatabaseName}_RestrictedReaderLogin");
								cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "S");
							}), Is.EqualTo(default(object)), $"User '{Db.DatabaseName}_RestrictedReaderLogin' should be missing");
						NUnit.Framework.Assert.That(adminConnection
							.ExecuteScalar(sqlCommandToGetDatabasePrincipalName,
							cmd =>
							{
								cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, $"{Db.DatabaseName}_UnrestrictedWriterLogin");
								cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "S");
							}), Is.EqualTo(default(object)), $"User '{Db.DatabaseName}_UnrestrictedWriterLogin' should be missing");
						NUnit.Framework.Assert.That(adminConnection
							.ExecuteScalar(sqlCommandToGetDatabasePrincipalName,
							cmd =>
							{
								cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, $"{Db.DatabaseName}_CargoWiseReaderLogin");
								cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "S");
							}), Is.EqualTo(default(object)), $"User '{Db.DatabaseName}_CargoWiseReaderLogin' should be missing");
						NUnit.Framework.Assert.That(adminConnection
							.ExecuteScalar(sqlCommandToGetDatabasePrincipalName,
							cmd =>
							{
								cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, $"{Db.DatabaseName}_CargoWiseWriterLogin");
								cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "S");
							}), Is.EqualTo(default(object)), $"User '{Db.DatabaseName}_CargoWiseWriterLogin' should be missing");

						NUnit.Framework.Assert.That(adminConnection
							.ExecuteScalar(sqlCommandToGetDatabasePrincipalName,
							cmd =>
							{
								cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, "cwRestrictedReaderRole");
								cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "R");
							}), Is.EqualTo(default(object)), $"Role 'cwRestrictedReaderRole' should be missing");
						NUnit.Framework.Assert.That(adminConnection
							.ExecuteScalar(sqlCommandToGetDatabasePrincipalName,
							cmd =>
							{
								cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, "cwRestrictedWriterRole");
								cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "R");
							}), Is.EqualTo(default(object)), $"Role 'cwRestrictedWriterRole' should be missing");
						NUnit.Framework.Assert.That(adminConnection
							.ExecuteScalar(sqlCommandToGetDatabasePrincipalName,
							cmd =>
							{
								cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, "cwUnrestrictedWriterRole");
								cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "R");
							}), Is.EqualTo(default(object)), $"Role 'cwUnrestrictedWriterRole' should be missing");
						NUnit.Framework.Assert.That(adminConnection
							.ExecuteScalar(sqlCommandToGetDatabasePrincipalName,
							cmd =>
							{
								cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, "cwHRMStaffRole");
								cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "R");
							}), Is.EqualTo(default(object)), $"Role 'cwHRMStaffRole' should be missing");
					}
				}
			}

			Db.ConnectionOverrideForTest = Db.NewExtraConnectionWithMainDbCredentials(Db.ServerName, Db.DatabaseName);

			AssertExceptionThrown<SqlException>($"Should not be able to open connection as User '{Db.DatabaseName}_RestrictedWriterLogin' is missing on main db.", () => Db.Connection.EnsureIsOpen());

			// Act

			var connectionSetupErrorHander = new DbConnectionSetup(Mock.Of<IEventLogger>(), Mock.Of<ICancellationTokenProvider>());
			connectionSetupErrorHander.TryConnectAndHandleErrors();

			// Assert
			AssertNoExceptionThrown($"Should be able to open connection after a call to TryConnectAndHandleErrors", () => Db.Connection.EnsureIsOpen());

			using (var adminConnection = Db.NewAdminConnection())
			{
				NUnit.Framework.Assert.That(adminConnection
						.ExecuteScalar<string>(sqlCommandToGetServerPrincipalName,
						cmd =>
						{
							cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, $"{Db.DatabaseName}_RestrictedWriterLogin");
							cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "S");
						}), Is.EqualTo($"{Db.DatabaseName}_RestrictedWriterLogin"), $"Login '{Db.DatabaseName}_RestrictedWriterLogin' should be created.");
				NUnit.Framework.Assert.That(adminConnection
					.ExecuteScalar<string>(sqlCommandToGetServerPrincipalName,
					cmd =>
					{
						cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, $"{Db.DatabaseName}_RestrictedReaderLogin");
						cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "S");
					}), Is.EqualTo($"{Db.DatabaseName}_RestrictedReaderLogin"), $"Login '{Db.DatabaseName}_RestrictedReaderLogin' should be created.");
				NUnit.Framework.Assert.That(adminConnection
					.ExecuteScalar<string>(sqlCommandToGetServerPrincipalName,
					cmd =>
					{
						cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, $"{Db.DatabaseName}_UnrestrictedWriterLogin");
						cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "S");
					}), Is.EqualTo($"{Db.DatabaseName}_UnrestrictedWriterLogin"), $"Login '{Db.DatabaseName}_RestrictedReaderLogin' should be created.");
				NUnit.Framework.Assert.That(adminConnection
					.ExecuteScalar<string>(sqlCommandToGetServerPrincipalName,
					cmd =>
					{
						cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, $"{Db.DatabaseName}_CargoWiseReaderLogin");
						cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "S");
					}), Is.EqualTo($"{Db.DatabaseName}_CargoWiseReaderLogin"), $"Login '{Db.DatabaseName}_RestrictedReaderLogin' should be created.");
				NUnit.Framework.Assert.That(adminConnection
					.ExecuteScalar<string>(sqlCommandToGetServerPrincipalName,
					cmd =>
					{
						cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, $"{Db.DatabaseName}_CargoWiseWriterLogin");
						cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "S");
					}), Is.EqualTo($"{Db.DatabaseName}_CargoWiseWriterLogin"), $"Login '{Db.DatabaseName}_RestrictedReaderLogin' should be created.");
			}

			foreach (var dbName in databases)
			{
				using (((ICurrentDbControl)Db.Connection).UseDatabase(dbName))
				{
					NUnit.Framework.Assert.That(Db.Connection
						.ExecuteScalar<string>(sqlCommandToGetDatabasePrincipalName,
						cmd =>
						{
							cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, $"{Db.DatabaseName}_RestrictedWriterLogin");
							cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "S");
						}), Is.EqualTo($"{Db.DatabaseName}_RestrictedWriterLogin"), $"User '{Db.DatabaseName}_RestrictedWriterLogin' should be present '{dbName}'.");
					NUnit.Framework.Assert.That(Db.Connection
						.ExecuteScalar<string>(sqlCommandToGetDatabasePrincipalName,
						cmd =>
						{
							cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, $"{Db.DatabaseName}_RestrictedReaderLogin");
							cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "S");
						}), Is.EqualTo($"{Db.DatabaseName}_RestrictedReaderLogin"), $"User '{Db.DatabaseName}_RestrictedReaderLogin' should be created on '{dbName}'.");
					NUnit.Framework.Assert.That(Db.Connection
						.ExecuteScalar<string>(sqlCommandToGetDatabasePrincipalName,
						cmd =>
						{
							cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, $"{Db.DatabaseName}_UnrestrictedWriterLogin");
							cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "S");
						}), Is.EqualTo($"{Db.DatabaseName}_UnrestrictedWriterLogin"), $"User '{Db.DatabaseName}_UnrestrictedWriterLogin' should be created on '{dbName}'.");
					NUnit.Framework.Assert.That(Db.Connection
						.ExecuteScalar<string>(sqlCommandToGetDatabasePrincipalName,
						cmd =>
						{
							cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, $"{Db.DatabaseName}_CargoWiseReaderLogin");
							cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "S");
						}), Is.EqualTo($"{Db.DatabaseName}_CargoWiseReaderLogin"), $"User '{Db.DatabaseName}_CargoWiseReaderLogin' should be created on '{dbName}'.");
					NUnit.Framework.Assert.That(Db.Connection
						.ExecuteScalar<string>(sqlCommandToGetDatabasePrincipalName,
						cmd =>
						{
							cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, $"{Db.DatabaseName}_CargoWiseWriterLogin");
							cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "S");
						}), Is.EqualTo($"{Db.DatabaseName}_CargoWiseWriterLogin"), $"User '{Db.DatabaseName}_CargoWiseWriterLogin' should be created on '{dbName}'.");

					NUnit.Framework.Assert.That(Db.Connection
						.ExecuteScalar<string>(sqlCommandToGetDatabasePrincipalName,
						cmd =>
						{
							cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, "cwRestrictedReaderRole");
							cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "R");
						}).ToLower(), Is.EqualTo("cwRestrictedReaderRole".ToLower()), $"Role 'cwRestrictedReaderRole' should be created on '{dbName}'.");
					NUnit.Framework.Assert.That(Db.Connection
						.ExecuteScalar<string>(sqlCommandToGetDatabasePrincipalName,
						cmd =>
						{
							cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, "cwRestrictedWriterRole");
							cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "R");
						}).ToLower(), Is.EqualTo("cwRestrictedWriterRole".ToLower()), $"Role 'cwRestrictedWriterRole' should be created on '{dbName}'.");
					NUnit.Framework.Assert.That(Db.Connection
						.ExecuteScalar<string>(sqlCommandToGetDatabasePrincipalName,
						cmd =>
						{
							cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, "cwUnrestrictedWriterRole");
							cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "R");
						}).ToLower(), Is.EqualTo("cwUnrestrictedWriterRole".ToLower()), $"Role 'cwUnrestrictedWriterRole' should be created on '{dbName}'.");
					NUnit.Framework.Assert.That(Db.Connection
						.ExecuteScalar<string>(sqlCommandToGetDatabasePrincipalName,
						cmd =>
						{
							cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, "cwHRMStaffRole");
							cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "R");
						}).ToLower(), Is.EqualTo("cwHRMStaffRole".ToLower()), $"Role 'cwHRMStaffRole' should be created on '{dbName}'.");
				}
			}
		}

		public void TestCreatesAllMissingApplicationLoginsAndUsersIfRestrictedWriterLoginPasswordShouldBeResetWhenModernSecuritySystemIsOn()
		{
			// Arrange
			EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = true;
			byte[] correctPasswordHash = null;
			var databases = Db.Connection.GetDatabases(DatabaseType.All & ~DatabaseType.SingleSharedRef);

			Db.Connection.Dispose();
			var sqlCommandToGetServerPrincipalName = $"SELECT TOP 1 name FROM sys.server_principals WHERE name = @principalName and type = @type";
			var sqlCommandToGetDatabasePrincipalName = $"SELECT TOP 1 name FROM sys.database_principals WHERE name = @principalName and type = @type";

			using (var adminConnection = Db.NewAdminConnection())
			{
				correctPasswordHash = adminConnection
					.ExecuteScalar<byte[]>(
					$"SELECT password_hash from sys.sql_logins WHERE name = @loginName",
					cmd =>
					{
						cmd.AddParameter("@loginName", System.Data.SqlDbType.NVarChar, 128, $"{Db.DatabaseName}_RestrictedWriterLogin");
					});

				var sid = adminConnection
					.ExecuteScalar<byte[]>(
					$"SELECT sid from sys.sql_logins WHERE name = @loginName",
					cmd =>
					{
						cmd.AddParameter("@loginName", System.Data.SqlDbType.NVarChar, 128, $"{Db.DatabaseName}_RestrictedWriterLogin");
					});

				DropServerPrincipals(adminConnection, $"{Db.DatabaseName}[_]%");

				adminConnection.ExecuteNonQuery(
					$@"
CREATE LOGIN [{Db.DatabaseName}_RestrictedWriterLogin] WITH PASSWORD = N'SOME RANDOM password 123]124}}??', SID = 0x{ByteArrayToHexString(sid)}
",
					cmd =>
					{
						cmd.AddParameter("@loginName", System.Data.SqlDbType.NVarChar, 128, $"{Db.DatabaseName}_RestrictedWriterLogin");
					});

				NUnit.Framework.Assert.That(adminConnection.ExecuteScalar<int>(
						$@"
SELECT PWDCOMPARE(N'SOME RANDOM password 123]124}}??', @correctPwdHash)
",
						cmd =>
						{
							cmd.AddParameter("@correctPwdHash", System.Data.SqlDbType.Binary, 512, correctPasswordHash);
						}), Is.EqualTo(0), $"Login '{Db.DatabaseName}_RestrictedWriterLogin' should be present but have incorrect password.");

				NUnit.Framework.Assert.That(adminConnection
					.ExecuteScalar(sqlCommandToGetServerPrincipalName,
					cmd =>
					{
						cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, $"{Db.DatabaseName}_RestrictedReaderLogin");
						cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "S");
					}), Is.EqualTo(default(object)), $"Login '{Db.DatabaseName}_RestrictedReaderLogin' should be missing.");
				NUnit.Framework.Assert.That(adminConnection
					.ExecuteScalar(sqlCommandToGetServerPrincipalName,
					cmd =>
					{
						cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, $"{Db.DatabaseName}_UnrestrictedWriterLogin");
						cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "S");
					}), Is.EqualTo(default(object)), $"Login '{Db.DatabaseName}_UnrestrictedWriterLogin' should be missing.");
				NUnit.Framework.Assert.That(adminConnection
					.ExecuteScalar(sqlCommandToGetServerPrincipalName,
					cmd =>
					{
						cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, $"{Db.DatabaseName}_CargoWiseReaderLogin");
						cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "S");
					}), Is.EqualTo(default(object)), $"Login '{Db.DatabaseName}_CargoWiseReaderLogin' should be missing.");
				NUnit.Framework.Assert.That(adminConnection
					.ExecuteScalar(sqlCommandToGetServerPrincipalName,
					cmd =>
					{
						cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, $"{Db.DatabaseName}_CargoWiseWriterLogin");
						cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "S");
					}), Is.EqualTo(default(object)), $"Login '{Db.DatabaseName}_CargoWiseWriterLogin' should be missing.");

				foreach (var dbName in databases)
				{
					using (((ICurrentDbControl)adminConnection).UseDatabase(dbName))
					{
						DropDatabasePrincipals(adminConnection, "cwHrmStaffRole");
						DropDatabasePrincipals(adminConnection, "cwRestrictedReaderRole");
						DropDatabasePrincipals(adminConnection, "cwRestrictedWriterRole");
						DropDatabasePrincipals(adminConnection, "cwUnrestrictedWriterRole");

						DropDatabasePrincipals(adminConnection, $"{Db.DatabaseName}[_]RestrictedReaderLogin");
						DropDatabasePrincipals(adminConnection, $"{Db.DatabaseName}[_]UnrestrictedWriterLogin");
						DropDatabasePrincipals(adminConnection, $"{Db.DatabaseName}[_]CargoWiseReaderLogin");
						DropDatabasePrincipals(adminConnection, $"{Db.DatabaseName}[_]CargoWiseWriterLogin");

						NUnit.Framework.Assert.That(adminConnection
							.ExecuteScalar<string>(sqlCommandToGetServerPrincipalName,
							cmd =>
							{
								cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, $"{Db.DatabaseName}_RestrictedWriterLogin");
								cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "S");
							}), Is.EqualTo($"{Db.DatabaseName}_RestrictedWriterLogin"), $"Login '{Db.DatabaseName}_RestrictedWriterLogin' should be present '{dbName}'.");

						NUnit.Framework.Assert.That(adminConnection
							.ExecuteScalar(sqlCommandToGetDatabasePrincipalName,
							cmd =>
							{
								cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, $"{Db.DatabaseName}_RestrictedReaderLogin");
								cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "S");
							}), Is.EqualTo(default(object)), $"User '{Db.DatabaseName}_RestrictedReaderLogin' should be missing");
						NUnit.Framework.Assert.That(adminConnection
							.ExecuteScalar(sqlCommandToGetDatabasePrincipalName,
							cmd =>
							{
								cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, $"{Db.DatabaseName}_UnrestrictedWriterLogin");
								cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "S");
							}), Is.EqualTo(default(object)), $"User '{Db.DatabaseName}_UnrestrictedWriterLogin' should be missing");
						NUnit.Framework.Assert.That(adminConnection
							.ExecuteScalar(sqlCommandToGetDatabasePrincipalName,
							cmd =>
							{
								cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, $"{Db.DatabaseName}_CargoWiseReaderLogin");
								cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "S");
							}), Is.EqualTo(default(object)), $"User '{Db.DatabaseName}_CargoWiseReaderLogin' should be missing");
						NUnit.Framework.Assert.That(adminConnection
							.ExecuteScalar(sqlCommandToGetDatabasePrincipalName,
							cmd =>
							{
								cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, $"{Db.DatabaseName}_CargoWiseWriterLogin");
								cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "S");
							}), Is.EqualTo(default(object)), $"User '{Db.DatabaseName}_CargoWiseWriterLogin' should be missing");

						NUnit.Framework.Assert.That(adminConnection
							.ExecuteScalar(sqlCommandToGetDatabasePrincipalName,
							cmd =>
							{
								cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, "cwRestrictedReaderRole");
								cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "R");
							}), Is.EqualTo(default(object)), $"Role 'cwRestrictedReaderRole' should be missing");
						NUnit.Framework.Assert.That(adminConnection
							.ExecuteScalar(sqlCommandToGetDatabasePrincipalName,
							cmd =>
							{
								cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, "cwRestrictedWriterRole");
								cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "R");
							}), Is.EqualTo(default(object)), $"Role 'cwRestrictedWriterRole' should be missing");
						NUnit.Framework.Assert.That(adminConnection
							.ExecuteScalar(sqlCommandToGetDatabasePrincipalName,
							cmd =>
							{
								cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, "cwUnrestrictedWriterRole");
								cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "R");
							}), Is.EqualTo(default(object)), $"Role 'cwUnrestrictedWriterRole' should be missing");
						NUnit.Framework.Assert.That(adminConnection
							.ExecuteScalar(sqlCommandToGetDatabasePrincipalName,
							cmd =>
							{
								cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, "cwHRMStaffRole");
								cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "R");
							}), Is.EqualTo(default(object)), $"Role 'cwHRMStaffRole' should be missing");
					}
				}
			}

			Db.ConnectionOverrideForTest = Db.NewExtraConnectionWithMainDbCredentials(Db.ServerName, Db.DatabaseName);

			AssertExceptionThrown<SqlException>($"Should not be able to open connection as '{Db.DatabaseName}_RestrictedWriterLogin' Login password is incorrect.", () => Db.Connection.EnsureIsOpen());

			// Act

			var connectionSetupErrorHander = new DbConnectionSetup(Mock.Of<IEventLogger>(), Mock.Of<ICancellationTokenProvider>());
			connectionSetupErrorHander.TryConnectAndHandleErrors();

			// Assert
			AssertNoExceptionThrown($"Should be able to open connection after a call to TryConnectAndHandleErrors.", () => Db.Connection.EnsureIsOpen());

			using (var adminConnection = Db.NewAdminConnection())
			{
				var cnnStringPwd = "badPwd";

				NUnit.Framework.Assert.That(adminConnection.ExecuteScalar<int>(
							$@"
SELECT PWDCOMPARE(@pwd, @correctPwdHash)
",
							cmd =>
							{
								cmd.AddParameter("@correctPwdHash", System.Data.SqlDbType.Binary, 512, correctPasswordHash);
								cmd.AddParameter("@pwd", System.Data.SqlDbType.NVarChar, 256, cnnStringPwd);
							}), Is.EqualTo(0), $"Login '{Db.DatabaseName}_RestrictedWriterLogin' should be present but have incorrect password.");

				NUnit.Framework.Assert.That(adminConnection
					.ExecuteScalar<string>(sqlCommandToGetServerPrincipalName,
					cmd =>
					{
						cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, $"{Db.DatabaseName}_RestrictedReaderLogin");
						cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "S");
					}), Is.EqualTo($"{Db.DatabaseName}_RestrictedReaderLogin"), $"Login '{Db.DatabaseName}_RestrictedReaderLogin' should be created.");
				NUnit.Framework.Assert.That(adminConnection
					.ExecuteScalar<string>(sqlCommandToGetServerPrincipalName,
					cmd =>
					{
						cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, $"{Db.DatabaseName}_UnrestrictedWriterLogin");
						cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "S");
					}), Is.EqualTo($"{Db.DatabaseName}_UnrestrictedWriterLogin"), $"Login '{Db.DatabaseName}_UnrestrictedWriterLogin' should be created.");
				NUnit.Framework.Assert.That(adminConnection
					.ExecuteScalar<string>(sqlCommandToGetServerPrincipalName,
					cmd =>
					{
						cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, $"{Db.DatabaseName}_CargoWiseReaderLogin");
						cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "S");
					}), Is.EqualTo($"{Db.DatabaseName}_CargoWiseReaderLogin"), $"Login '{Db.DatabaseName}_CargoWiseReaderLogin' should be created.");
				NUnit.Framework.Assert.That(adminConnection
					.ExecuteScalar<string>(sqlCommandToGetServerPrincipalName,
					cmd =>
					{
						cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, $"{Db.DatabaseName}_CargoWiseWriterLogin");
						cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "S");
					}), Is.EqualTo($"{Db.DatabaseName}_CargoWiseWriterLogin"), $"Login '{Db.DatabaseName}_CargoWiseWriterLogin' should be created.");
			}

			foreach (var dbName in databases)
			{
				using (((ICurrentDbControl)Db.Connection).UseDatabase(dbName))
				{
					NUnit.Framework.Assert.That(Db.Connection
						.ExecuteScalar<string>(sqlCommandToGetDatabasePrincipalName,
						cmd =>
						{
							cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, $"{Db.DatabaseName}_RestrictedWriterLogin");
							cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "S");
						}), Is.EqualTo($"{Db.DatabaseName}_RestrictedWriterLogin"), $"User '{Db.DatabaseName}_RestrictedWriterLogin' should be present '{dbName}'.");
					NUnit.Framework.Assert.That(Db.Connection
						.ExecuteScalar<string>(sqlCommandToGetDatabasePrincipalName,
						cmd =>
						{
							cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, $"{Db.DatabaseName}_RestrictedReaderLogin");
							cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "S");
						}), Is.EqualTo($"{Db.DatabaseName}_RestrictedReaderLogin"), $"User '{Db.DatabaseName}_RestrictedReaderLogin' should be created on '{dbName}'.");
					NUnit.Framework.Assert.That(Db.Connection
						.ExecuteScalar<string>(sqlCommandToGetDatabasePrincipalName,
						cmd =>
						{
							cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, $"{Db.DatabaseName}_UnrestrictedWriterLogin");
							cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "S");
						}), Is.EqualTo($"{Db.DatabaseName}_UnrestrictedWriterLogin"), $"User '{Db.DatabaseName}_UnrestrictedWriterLogin' should be created on '{dbName}'.");
					NUnit.Framework.Assert.That(Db.Connection
						.ExecuteScalar<string>(sqlCommandToGetDatabasePrincipalName,
						cmd =>
						{
							cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, $"{Db.DatabaseName}_CargoWiseReaderLogin");
							cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "S");
						}), Is.EqualTo($"{Db.DatabaseName}_CargoWiseReaderLogin"), $"User '{Db.DatabaseName}_CargoWiseReaderLogin' should be created on '{dbName}'.");
					NUnit.Framework.Assert.That(Db.Connection
						.ExecuteScalar<string>(sqlCommandToGetDatabasePrincipalName,
						cmd =>
						{
							cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, $"{Db.DatabaseName}_CargoWiseWriterLogin");
							cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "S");
						}), Is.EqualTo($"{Db.DatabaseName}_CargoWiseWriterLogin"), $"User '{Db.DatabaseName}_CargoWiseWriterLogin' should be created on '{dbName}'.");

					NUnit.Framework.Assert.That(Db.Connection
						.ExecuteScalar<string>(sqlCommandToGetDatabasePrincipalName,
						cmd =>
						{
							cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, "cwRestrictedReaderRole");
							cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "R");
						}).ToLower(), Is.EqualTo("cwRestrictedReaderRole".ToLower()), $"Role 'cwRestrictedReaderRole' should be created on '{dbName}'.");
					NUnit.Framework.Assert.That(Db.Connection
						.ExecuteScalar<string>(sqlCommandToGetDatabasePrincipalName,
						cmd =>
						{
							cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, "cwRestrictedWriterRole");
							cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "R");
						}).ToLower(), Is.EqualTo("cwRestrictedWriterRole".ToLower()), $"Role 'cwRestrictedWriterRole' should be created on '{dbName}'.");
					NUnit.Framework.Assert.That(Db.Connection
						.ExecuteScalar<string>(sqlCommandToGetDatabasePrincipalName,
						cmd =>
						{
							cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, "cwUnrestrictedWriterRole");
							cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "R");
						}).ToLower(), Is.EqualTo("cwUnrestrictedWriterRole".ToLower()), $"Role 'cwUnrestrictedWriterRole' should be created on '{dbName}'.");
					NUnit.Framework.Assert.That(Db.Connection
						.ExecuteScalar<string>(sqlCommandToGetDatabasePrincipalName,
						cmd =>
						{
							cmd.AddParameter("@principalName", System.Data.SqlDbType.NVarChar, 128, "cwHRMStaffRole");
							cmd.AddParameter("@type", System.Data.SqlDbType.Char, 2, "R");
						}).ToLower(), Is.EqualTo("cwHRMStaffRole".ToLower()), $"Role 'cwHRMStaffRole' should be created on '{dbName}'.");
				}
			}
		}

		public void TestSetupConnectionWithNoErrorWhenNewDbVersionWhenModernSecurityIsOff()
		{
			// Arrange
			EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = false;

			TestSetupConnectionWithNoErrorWhenNewDbVersion();
		}

		public void TestSetupConnectionWithNoErrorWhenNewDbVersionWhenModernSecurityIsOn()
		{
			// Arrange
			EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = true;

			TestSetupConnectionWithNoErrorWhenNewDbVersion();
		}

		[DeveloperOnlyTest("The test may result in amnesties and we do not really use password policy on logins.")]
		public void TestUnlocksRestrictedWriterLoginIfLockedWhenModernSecurityIsOff()
		{
			// Arrange
			var loginName = $"{Db.DatabaseName}_RestrictedWriterLogin";

			EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = false;

			AssertNoExceptionThrown("Should be able to login before login is locked out", () => Db.Connection.EnsureIsOpen());

			if (!TryLockLogin(loginName, 100))
			{
				Assert("Unable to lockout login to setup the test. Please check that 'Local Security Policy -> Account Lockout Policy' are set to reasonable values on the test box", false);
			}

			Db.Connection.CloseConnection();

			Db.ConnectionOverrideForTest = Db.NewExtraConnectionWithMainDbCredentials(Db.ServerName, Db.DatabaseName);

			AssertExceptionThrown<SqlException>($"Should not be able to open connection as Login '{loginName}' should be locked out", () => Db.Connection.EnsureIsOpen());
			var connectionSetupErrorHander = new DbConnectionSetup(Mock.Of<IEventLogger>(), Mock.Of<ICancellationTokenProvider>());
			connectionSetupErrorHander.TryConnectAndHandleErrors();

			AssertNoExceptionThrown($"Should be able to open connection now as Login '{loginName}' should no longer be locked out", () => Db.Connection.EnsureIsOpen());
		}

		public void TestResetsPasswordForRestrictedWriterLoginWhenModernSecurityIsOff()
		{
			EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = false;

			TestResetsPasswordForRestrictedWriterLogin();
		}

		public void TestResetsPasswordForRestrictedWriterLoginWhenModernSecurityIsOn()
		{
			EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = true;

			TestResetsPasswordForRestrictedWriterLogin();
		}

		public void TestPropagatesExceptionIfFailedToConnectWhenModernSecurityIsOff()
		{
			EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = false;

			TestPropagatesExceptionIfFailedToConnect();
		}

		public void TestPropagatesExceptionIfFailedToConnectWhenModernSecurityIsOn()
		{
			EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = true;

			TestPropagatesExceptionIfFailedToConnect();
		}

		#region Implementation

		const string connectionTriggerName = "connection_limit_trigger_e507c1db-5e2d-491a-b933-c6f3b52b825e";

		protected override void SetUp()
		{
			base.SetUp();
			Db.ConnectionOverrideForTest?.Dispose();
			Db.ConnectionOverrideForTest = null;

			var dropTrigger = $"DROP TRIGGER IF EXISTS [{connectionTriggerName}] ON ALL SERVER";
			using (var adminConnection = Db.NewAdminConnection())
			{
				adminConnection.ExecuteNonQuery(dropTrigger);
			}
		}

		protected override void TearDown()
		{
			var dropTrigger = $"DROP TRIGGER IF EXISTS [{connectionTriggerName}] ON ALL SERVER";
			using (var adminConnection = Db.NewAdminConnection())
			{
				adminConnection.ExecuteNonQuery(dropTrigger);
			}

			Db.ConnectionOverrideForTest?.Dispose();
			Db.ConnectionOverrideForTest = null;
			base.TearDown();
		}

		static string ByteArrayToHexString(byte[] ba)
		{
			StringBuilder hex = new StringBuilder(ba.Length * 2);
			foreach (byte b in ba)
			{
				hex.AppendFormat("{0:x2}", b);
			}
			return hex.ToString();
		}

		bool TryLockLogin(string loginName, int maxAttemptsToLock)
		{
			using (var adminConnection = Db.NewAdminConnection())
			{
				var alterLoginSql = $"ALTER LOGIN [{loginName}] WITH CHECK_POLICY = ON, CHECK_EXPIRATION = OFF";
				adminConnection.ExecuteNonQuery(alterLoginSql);

				var checkLoginIsLockedSql = $"SELECT LOGINPROPERTY ( '{loginName}' , 'IsLocked' )";

				for (var i = 0; i < maxAttemptsToLock && (adminConnection.ExecuteScalar<int>(checkLoginIsLockedSql)) == 0; i++)
				{
					try
					{
						// need different password every time so bad password count is actually increasing
						var connectionWithWrongPassword = Db.NewExtraConnection(
							Db.ServerName,
							Db.DatabaseName,
							loginName,
							$"123-{Guid.NewGuid()}ABC");
						connectionWithWrongPassword.EnsureIsOpen();
						connectionWithWrongPassword.Dispose();
					}
					catch (SqlException ex)
					{
						if (ex.Number != 18456 /* Login failed */)
						{
							throw;
						}
					}
				}

				return adminConnection.ExecuteScalar<int>(checkLoginIsLockedSql) != 0;
			}
		}

		void TestPropagatesExceptionIfFailedToConnect()
		{
			// Arrange
			var loginName = $"{Db.DatabaseName}_RestrictedWriterLogin";
			var createTrigger = $@"
CREATE TRIGGER [{connectionTriggerName}]
	ON ALL SERVER
	FOR LOGON AS
	BEGIN
		IF ORIGINAL_LOGIN() = N'{loginName}'
			ROLLBACK;
	END;
";

			AssertNoExceptionThrown("Should be able to login before trigger is introduced", () => Db.Connection.EnsureIsOpen());

			Db.ConnectionOverrideForTest = Db.NewExtraConnectionWithMainDbCredentials(Db.ServerName, Db.DatabaseName);

			using (var adminConnection = Db.NewAdminConnection())
			{
				adminConnection.ExecuteNonQuery(createTrigger);
			}

			AssertExceptionThrown<SqlException>($"Should not be able to open connection", () => Db.Connection.EnsureIsOpen());

			var dbSetup = new DbConnectionSetup(Mock.Of<IEventLogger>(), Mock.Of<ICancellationTokenProvider>());

			// Act
			// Assert
			AssertExceptionThrown<SqlException>("Should throw SqlException as it should not be able to login", () => dbSetup.TryConnectAndHandleErrors());
			AssertExceptionThrown<SqlException>($"Should not be able to open connection after a call to DbConnectionSetup.TryConnectAndHandleErrors as it should have failed to fix the problem", () => Db.Connection.EnsureIsOpen());
		}

		static void TestSetupConnectionWithNoErrorWhenNewDbVersion()
		{
			// Arrange
			var bumpedSchemaVersion = ObjectFactory.Get<IDatabaseAspectVersions>().SchemaVersion.Major + 10;
			var versionMock = Mock.Of<IDatabaseAspectVersions>(x => x.SchemaVersion == new VersionLabel(bumpedSchemaVersion, 0));

			using (ObjectFactory.Substitute(versionMock))
			{
				Db.Connection.Dispose();

				// Act
				// Assert
				AssertNoExceptionThrown(() => new DbConnectionSetup(Mock.Of<IEventLogger>(), Mock.Of<ICancellationTokenProvider>()).TryConnectAndHandleErrors());
			}
		}

		static void TestRecreatesMissingRestrictedWriterLoginAndUsers(DatabaseType databaseTypesToInclude)
		{
			// Arrange
			var missingLogin = $"{Db.DatabaseName}_RestrictedWriterLogin";

			var databases = Db.Connection.GetDatabases(databaseTypesToInclude);
			Db.Connection.Dispose();

			var sqlCommandToGetLoginName = $"SELECT TOP 1 name FROM sys.server_principals WHERE name = '{missingLogin}'";
			var sqlCommandToGetUserName = $"SELECT TOP 1 name FROM sys.database_principals WHERE name = '{missingLogin}'";

			using (var adminConnection = Db.NewAdminConnection())
			{
				DropServerPrincipals(adminConnection, DataUtils.ReplaceSqlLikeWildcard(missingLogin));
				foreach (var dbName in databases)
				{
					using (((ICurrentDbControl)adminConnection).UseDatabase(dbName))
					{
						DropDatabasePrincipals(adminConnection, DataUtils.ReplaceSqlLikeWildcard(missingLogin));

						NUnit.Framework.Assert.That(adminConnection.ExecuteScalar(sqlCommandToGetUserName), Is.EqualTo(default(object)), $"User '{missingLogin}' should be missing in database '{dbName}'.");
					}
				}

				NUnit.Framework.Assert.That(adminConnection.ExecuteScalar(sqlCommandToGetLoginName), Is.EqualTo(default(object)), $"Login '{missingLogin}' should be missing.");
			}

			Db.ConnectionOverrideForTest = Db.NewExtraConnectionWithMainDbCredentials(Db.ServerName, Db.DatabaseName);

			AssertExceptionThrown<SqlException>($"Should not be able to open connection as both Login and Users '{missingLogin}' are missing.", () => Db.Connection.EnsureIsOpen());

			// Act
			var connectionSetupErrorHander = new DbConnectionSetup(Mock.Of<IEventLogger>(), Mock.Of<ICancellationTokenProvider>());
			connectionSetupErrorHander.TryConnectAndHandleErrors();

			// Assert
			AssertNoExceptionThrown($"Should be able to open connection after a call to TryConnectAndHandleErrors.", () => Db.Connection.EnsureIsOpen());

			NUnit.Framework.Assert.That(Db.Connection.ExecuteScalar<string>(sqlCommandToGetLoginName), Is.EqualTo(missingLogin), $"Login '{missingLogin}' should be created after a call to TryConnectAndHandleErrors.");

			foreach (var dbName in databases)
			{
				using (((ICurrentDbControl)Db.Connection).UseDatabase(dbName))
				{
					NUnit.Framework.Assert.That(Db.Connection.ExecuteScalar<string>(sqlCommandToGetUserName), Is.EqualTo(missingLogin), $"User '{missingLogin}' should be created after a call to TryConnectAndHandleErrors in database '{dbName}'.");
				}
			}
		}

		static void TestRecreatesMissingRestrictedWriterLoginUserIfMissingInMainDb(DatabaseType databaseTypesToInclude)
		{
			// Arrange
			var missingUser = $"{Db.DatabaseName}_RestrictedWriterLogin";

			var databases = Db.Connection.GetDatabases(databaseTypesToInclude);
			Db.Connection.Dispose();

			var sqlCommandToGetLoginName = $"SELECT TOP 1 name FROM sys.server_principals WHERE name = N'{missingUser}'";
			var sqlCommandToGetUserName = $"SELECT TOP 1 name FROM sys.database_principals WHERE name = N'{missingUser}'";

			using (var adminConnection = Db.NewAdminConnection())
			{
				using (((ICurrentDbControl)adminConnection).UseDatabase(Db.DatabaseName))
				{
					DropDatabasePrincipals(adminConnection, DataUtils.ReplaceSqlLikeWildcard(missingUser));

					NUnit.Framework.Assert.That(adminConnection.ExecuteScalar(sqlCommandToGetUserName), Is.EqualTo(default(object)), $"User '{missingUser}' should be missing in database '{Db.DatabaseName}'.");
				}

				NUnit.Framework.Assert.That(adminConnection.ExecuteScalar<string>(sqlCommandToGetLoginName), Is.EqualTo(missingUser), $"Login '{missingUser}' should be present.");
			}

			Db.ConnectionOverrideForTest = Db.NewExtraConnectionWithMainDbCredentials(Db.ServerName, Db.DatabaseName);

			AssertExceptionThrown<SqlException>($"Should not be able to open connection as user '{missingUser}' is missing in main database.", () => Db.Connection.EnsureIsOpen());

			// Act
			var connectionSetupErrorHander = new DbConnectionSetup(Mock.Of<IEventLogger>(), Mock.Of<ICancellationTokenProvider>());
			connectionSetupErrorHander.TryConnectAndHandleErrors();

			// Assert
			AssertNoExceptionThrown($"Should be able to open connection after a call to TryConnectAndHandleErrors.", () => Db.Connection.EnsureIsOpen());

			using (((ICurrentDbControl)Db.Connection).UseDatabase(Db.DatabaseName))
			{
				NUnit.Framework.Assert.That(Db.Connection.ExecuteScalar<string>(sqlCommandToGetUserName), Is.EqualTo(missingUser), $"User '{missingUser}' should be created after a call to TryConnectAndHandleErrors in database '{Db.DatabaseName}'.");
			}
		}

		static void TestRecreatesMissingRestrictedWriterLogin(DatabaseType databaseTypesToInclude)
		{
			// Arrange
			var missingLogin = $"{Db.DatabaseName}_RestrictedWriterLogin";

			var databases = Db.Connection.GetDatabases(databaseTypesToInclude);
			Db.Connection.Dispose();

			var sqlCommandToGetLoginName = $"SELECT TOP 1 name FROM sys.server_principals WHERE name = '{missingLogin}'";
			var sqlCommandToGetUserName = $"SELECT TOP 1 name FROM sys.database_principals WHERE name = '{missingLogin}'";

			using (var adminConnection = Db.NewAdminConnection())
			{
				DropServerPrincipals(adminConnection, DataUtils.ReplaceSqlLikeWildcard(missingLogin));

				using (((ICurrentDbControl)adminConnection).UseDatabase(Db.DatabaseName))
				{
					NUnit.Framework.Assert.That(adminConnection.ExecuteScalar<string>(sqlCommandToGetUserName), Is.EqualTo(missingLogin), $"User '{missingLogin}' should be present in main db.");
				}

				NUnit.Framework.Assert.That(adminConnection.ExecuteScalar(sqlCommandToGetLoginName), Is.EqualTo(default(object)), $"Login '{missingLogin}' should be missing.");
			}

			Db.ConnectionOverrideForTest = Db.NewExtraConnectionWithMainDbCredentials(Db.ServerName, Db.DatabaseName);

			AssertExceptionThrown<SqlException>($"Should not be able to open connection as Login '{missingLogin}' is missing.", () => Db.Connection.EnsureIsOpen());

			// Act
			var connectionSetupErrorHander = new DbConnectionSetup(Mock.Of<IEventLogger>(), Mock.Of<ICancellationTokenProvider>());
			connectionSetupErrorHander.TryConnectAndHandleErrors();

			// Assert
			AssertNoExceptionThrown($"Should be able to open connection after a call to TryConnectAndHandleErrors.", () => Db.Connection.EnsureIsOpen());

			NUnit.Framework.Assert.That(Db.Connection.ExecuteScalar<string>(sqlCommandToGetLoginName), Is.EqualTo(missingLogin), $"Login '{missingLogin}' should be created after a call to TryConnectAndHandleErrors.");
		}

		static void TestResetsPasswordForRestrictedWriterLogin()
		{
			// Arrange
			var loginName = $"{Db.DatabaseName}_RestrictedWriterLogin";

			Db.Connection.Dispose();

			using (var adminConnection = Db.NewAdminConnection())
			{
				var correctPasswordHash = adminConnection
					.ExecuteScalar<byte[]>(
					$"SELECT password_hash from sys.sql_logins WHERE name = @loginName",
					cmd =>
					{
						cmd.AddParameter("@loginName", System.Data.SqlDbType.NVarChar, 128, $"{Db.DatabaseName}_RestrictedWriterLogin");
					});

				var sid = adminConnection
					.ExecuteScalar<byte[]>(
					$"SELECT sid from sys.sql_logins WHERE name = @loginName",
					cmd =>
					{
						cmd.AddParameter("@loginName", System.Data.SqlDbType.NVarChar, 128, $"{Db.DatabaseName}_RestrictedWriterLogin");
					});

				DropServerPrincipals(adminConnection, $"{Db.DatabaseName}[_]RestrictedWriterLogin");

				adminConnection.ExecuteNonQuery(
					$@"
CREATE LOGIN [{Db.DatabaseName}_RestrictedWriterLogin] WITH PASSWORD = N'SOME RANDOM password 123]124}}??', SID = 0x{ByteArrayToHexString(sid)}
",
					cmd =>
					{
						cmd.AddParameter("@loginName", System.Data.SqlDbType.NVarChar, 128, $"{Db.DatabaseName}_RestrictedWriterLogin");
					});

				NUnit.Framework.Assert.That(adminConnection.ExecuteScalar<int>(
						$@"
SELECT PWDCOMPARE('SOME RANDOM password 123]124}}??', @correctPwdHash)
",
						cmd =>
						{
							cmd.AddParameter("@correctPwdHash", System.Data.SqlDbType.Binary, 512, correctPasswordHash);
						}), Is.EqualTo(0), $"Login '{Db.DatabaseName}_RestrictedWriterLogin' should be present but have incorrect password.");
			}

			Db.ConnectionOverrideForTest = Db.NewExtraConnectionWithMainDbCredentials(Db.ServerName, Db.DatabaseName);

			AssertExceptionThrown<SqlException>($"Should not be able to open connection as Login '{loginName}' has incorrect password.", () => Db.Connection.EnsureIsOpen());

			// Act
			var connectionSetupErrorHander = new DbConnectionSetup(Mock.Of<IEventLogger>(), Mock.Of<ICancellationTokenProvider>());
			connectionSetupErrorHander.TryConnectAndHandleErrors();

			// Assert
			AssertNoExceptionThrown($"Should be able to open connection now as '{loginName}' Login's password should have been reset.", () => Db.Connection.EnsureIsOpen());
		}

		static void DropServerPrincipals(AdminConnection connection, string principalNameLikeWildCard)
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
					principalNameLikeWildCard,
				},
				notLikeFilters: Array.Empty<string>(),
				Db.DatabaseCollation,
				Mock.Of<ILogger>());

			serverSecuritySynchroniser.Synchronise(((IDbConnectionInternals)connection).ADOConnection);
		}

		public static void DropDatabasePrincipals(AdminConnection connection, string principalNameLikeWildCard)
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
				likeFilters: new[]
				{
					principalNameLikeWildCard
				},
				notLikeFilters: Array.Empty<string>(),
				Db.DatabaseCollation,
				Mock.Of<ILogger>());

			synchroniser.Synchronise(((IDbConnectionInternals)connection).ADOConnection);
		}

		#endregion Implementation

	}
}
#endif
