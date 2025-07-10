using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using AppDomainWrappers.Net;
using CargoWise.IO;
using Dat.Integration;
using Enterprise.Dat.Implementation;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using WTG.DeploymentUtils.FileSystem;
using static System.FormattableString;

namespace Enterprise.Dat.Deployment.Testing
{
	[Serializable]
	sealed class DatBackupFileGeneratorTests : TestCase
	{
		[RequiresSoftware(RequiredSoftware.IsVM)]
		[SnailTest]
		public void TestBackupDatabaseLogFileIsShrunk()
		{
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
			var tempBackupPath = new TempDirectory();
			var domainData = new Dictionary<string, object>
			{
				{ "BinPath", Assembly.GetExecutingAssembly().Location },
				{ "SourcePath", string.Empty },
				{ "BackupPath", tempBackupPath.DirectoryName },
			};

			try
			{
				AssertNoExceptionThrown(() =>
				{
					using (var appDomainWrapper = new AppDomainWrapper())
					{
						appDomainWrapper.RunActionInAppDomain(() =>
						{
							EnterpriseApplicationConfiguration.ConfigureObjectFactory();

							var wrapper = new DatBackupFileGeneratorWrapper();
							wrapper.TestDatBackupFileUpdate(
								(string)AppDomain.CurrentDomain.GetData("BinPath"),
								(string)AppDomain.CurrentDomain.GetData("SourcePath"),
								(string)AppDomain.CurrentDomain.GetData("BackupPath"));
						}, domainData);
					}
				});

				var backupFiles = Directory.GetFiles(tempBackupPath.DirectoryName);
				Assert("Some files backup files should have been generated", backupFiles.Length > 0);

				CombineAssertions(() =>
				{
					using (var sqlConnection = LocalDBConnection.GetConnection())
					{
						sqlConnection.Open();
						foreach (var backupFilePath in backupFiles)
						{
							var testDatabase = Guid.NewGuid().ToString();
							using (var temp = new TempDirectory())
							{
								try
								{
									RestoreFromFile(sqlConnection, testDatabase, backupFilePath, temp);
									using (var cmd = sqlConnection.CreateCommand())
									{
										var logFileSizes = new List<(string LogicalName, int Size)>();
										cmd.CommandText = Invariant($"select log_reuse_wait_desc from sys.databases where name = N'{testDatabase}'");
										var logReuseWaitDesc = (string)cmd.ExecuteScalar();

										Assert(
											$"log_reuse_wait_desc must be NOTHING or CHECKPOINT but was {logReuseWaitDesc} for {backupFilePath}",
											string.Equals("NOTHING", logReuseWaitDesc, StringComparison.OrdinalIgnoreCase)
											|| string.Equals("CHECKPOINT", logReuseWaitDesc, StringComparison.OrdinalIgnoreCase));

										cmd.CommandText = Invariant($"USE [{testDatabase}]");
										cmd.ExecuteNonQuery();

										cmd.CommandText = "select name, type, size, growth, is_percent_growth from sys.database_files";
										using (var reader = cmd.ExecuteReader())
										{
											while (reader.Read())
											{
												var logicalName = reader["name"].ToString();

												if ((byte)reader["type"] == 1)
												{
													// (int)reader["size"] is defined as "Current size of the file, in 8-KB pages"
													var sizeInBytes = (int)reader["size"] << 13;
													var sizeInMebibytes = sizeInBytes >> 20;
													Assert($"Logical file {logicalName}, size is: {sizeInMebibytes}MB, expecting <= 10MB", sizeInMebibytes <= 10); // fileSize used to be >500MB, this tests that they have been reduced to a neglible size
												}

												var growth = (int)reader["growth"];
												if ((bool)reader["is_percent_growth"])
												{
													Assert($"Logical file {logicalName}, growth is: {growth}%, expecting <= 20% or 20MB", growth < 20);
												}
												else
												{
													growth = (int)(growth * 0.008);
													Assert($"Logical file {logicalName}, growth is: {growth}MB, expecting <= 20% or 20MB", growth < 20);
												}
											}
										}
									}
								}
								finally
								{
									DropDatabase(sqlConnection, testDatabase);
								}
							}
						}
					}
				});
			}
			finally
			{
				tempBackupPath.Dispose();
			}
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCurrentDATDbBackupFileExists()
		{
			var assemblyLocation = Assembly.GetExecutingAssembly().Location;

			var configuration = DeploymentConfiguration.FromConfigurationString(
					"DatBackupFileUpdate:UnitTest:" + "C:\\AAA",
					assemblyLocation,
					BaseSourcePath,
					null);
			var process = new DatBackupFileGenerator(configuration);

			Assert($"File does not exist at {process.CurrentDATDbBackupPrefixFileLocation}", File.Exists(process.CurrentDATDbBackupPrefixFileLocation));
		}

		public sealed class DatBackupFileGeneratorWrapper
		{
			public string TestDatBackupFileUpdate(string binPath, string sourcePath, string backupPath)
			{
				using (var taskLogger = new TestLogger())
				{
					try
					{
						var configuration = DeploymentConfiguration.FromConfigurationString("DatBackupFileUpdate:UnitTest:" + backupPath, binPath, sourcePath, null);
						var factory = new DeploymentProcessFactory();
						var process = factory.Create(configuration);

						Assert(process is DatBackupFileGenerator);

						ReleaseInfo.CreateNewInstanceForTesting("1.2.3.4", DateTime.Now, WTG.DevTools.Definitions.ReleaseRings.Codes.ALP);

						process.Deploy(taskLogger);

						return DbRestorer.OverridableDatabaseBackupsPath.Value;
					}
					catch (Exception exception)
					{
						throw new Exception("Logs: " + taskLogger.GetLogs(), exception);
					}
				}
			}
		}

		#region helpers
		[SuppressMessage("Microsoft.Security", "CA2100:ReviewSqlQueriesForSecurityVulnerabilities", Justification = "Database name cannot be parameterized")]
		static void DropDatabase(SqlConnection connection, string databaseName)
		{
			using (var cmd = connection.CreateCommand())
			{
				cmd.CommandText = "USE MASTER";
				cmd.ExecuteNonQuery();

				cmd.CommandText = Invariant($"ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE");
				cmd.ExecuteNonQuery();

				cmd.CommandText = Invariant($"DROP DATABASE [{databaseName}]");
				cmd.ExecuteNonQuery();
			}
		}

		[SuppressMessage("Microsoft.Security", "CA2100:ReviewSqlQueriesForSecurityVulnerabilities", Justification = "Database backup path cannot be parameterized")]
		static void RestoreFromFile(SqlConnection connection, string restoreDatabaseName, string fromBackupFile, string moveFilesToPath)
		{
			using (var cmd = connection.CreateCommand())
			{
				var moveFiles = new List<string>();
				cmd.CommandText = Invariant($"RESTORE FILELISTONLY FROM DISK='{fromBackupFile}'");
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var logicalName = reader["LogicalName"].ToString();
						var physicalName = reader["PhysicalName"].ToString();

						moveFiles.Add(Invariant($"MOVE '{logicalName}' TO '{Path.Combine(moveFilesToPath, Path.GetFileName(physicalName))}'"));
					}
				}

				cmd.CommandText = $"USE master";
				cmd.ExecuteNonQuery();

				cmd.CommandText = Invariant($"RESTORE DATABASE [{restoreDatabaseName}] FROM DISK = '{fromBackupFile}' WITH RECOVERY, {string.Join(",", moveFiles)}");
				cmd.ExecuteNonQuery();
			}
		}

		#endregion

		#region Debug Logger
		sealed class TestLogger : ITaskLogger, IDisposable
		{
			readonly StringWriter writer;
			public TestLogger()
			{
				writer = new StringWriter();
			}

			public void Dispose()
			{
				Dispose(true);
			}

			bool disposed;
			void Dispose(bool disposing)
			{
				if (disposing)
				{
					if (!disposed)
					{
						writer.Flush();
						writer.Close();
						writer.Dispose();
					}

					disposed = true;
				}
			}

			public void RecordInfo(string message)
			{
				writer.WriteLine(message);
			}

			public IDisposable RecordTask(string taskInfo)
			{
				writer.WriteLine(taskInfo);
				return new DisposableAction(() => writer.Flush());
			}

			public string GetLogs() => writer.ToString();
		}

		sealed class DisposableAction : IDisposable
		{
			public DisposableAction(Action action)
			{
				this.action = action;
			}

			public void Dispose()
			{
				action?.Invoke();
			}

			readonly Action action;
		}
		#endregion
	}
}
