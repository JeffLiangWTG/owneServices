#if !NET8_0_OR_GREATER // Should be fixed in WI00669071: Remove usages of AppDomains
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Definitions;
using CargoWise.IO;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.DbUpgrader.Shared;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.ActivityLogging;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Microsoft.CSharp;
using NUnit.Framework;
using ServiceManager.Integration.CW;
using ServiceManager.Logging.CW.Test;

namespace Enterprise.Startup.Testing
{
	/// <summary>
	/// This test is to ensure an empty Enterprise database can be upgraded.
	/// It should need virtually no initial setup and the upgrade should create all tables and populate base data.
	///  - Only StmData (with no rows) and GlbStaff (with just the sysadmin user row) need to be there before the upgrade.
	/// Anything added to the application start up must not require any further tables/data.
	/// </summary>
	sealed class UpgradeAssuranceTest : TestCase
	{
		#region Run Enterprise On Empty Database

		[UseSnapshotProtection, SnailTest]
		public void TestScheduledDbUpgraderOnEmptyDatabaseWithNoBI()
		{
			SetupForAndRunDbUpgrade(disableBI: true);
		}

		[UseSnapshotProtection, SnailTest]
		public void TestScheduledDbUpgraderOnEmptyDatabaseWithBI()
		{
			SetupForAndRunDbUpgrade(disableBI: false);
		}

		[UseSnapshotProtection, SnailTest]
		public void TestCheckAndUpgradeDb_WhenSchemaVersionTooOld_ShouldInformUserAndNotRunUpgrade()
		{
			var currentSchemaVersion = DataRegistry.MinUpgradableDbMajorSchemaVersion - 1;
			var expectedFailureMessage = $"Current database schema version is too old (< {DataRegistry.MinUpgradableDbMajorSchemaVersion}.0). A two-stage upgrade is required. You must upgrade using the General Product release starting with";
			var expectedFailureCaption = "Current database schema version is too old";

			void SetupBeforeUpgrade(string dbName)
			{
				InsertStmData(dbName, "DATABASE_SCHEMA_VERSION", currentSchemaVersion.ToString(CultureInfo.InvariantCulture));
			}

			SetupForAndRunDbUpgrade(disableBI: true, additionalDBSetup: SetupBeforeUpgrade, shouldDatabaseBeUpgraded: false, expectedFailureMessage, expectedFailureCaption, useDatUpgradeDirector: false);
		}

		[UseSnapshotProtection, SnailTest]
		public void TestCheckAndUpgradeDb_WhenTransformVersionTooOld_ShouldInformUserAndNotRunUpgrade()
		{
			var currentTransformationVersion = DataRegistry.MinDatabaseMajorTransformationVersion - 1;
			var expectedFailureMessage = $"Current database transformation version is too old (< {DataRegistry.MinDatabaseMajorTransformationVersion}.0). A two-stage upgrade is required. You must upgrade using the General Product release starting with";
			var expectedFailureCaption = "Current database transformation version is too old";

			void SetupBeforeUpgrade(string dbName)
			{
				InsertStmData(dbName, "DatabaseMajorTransformationVersion", currentTransformationVersion.ToString(CultureInfo.InvariantCulture));
			}

			SetupForAndRunDbUpgrade(disableBI: true, additionalDBSetup: SetupBeforeUpgrade, shouldDatabaseBeUpgraded: false, expectedFailureMessage, expectedFailureCaption, useDatUpgradeDirector: false);
		}

		[UseSnapshotProtection, SnailTest]
		public void TestCheckAndUpgradeDb_WithDatUpgradeDirector_WhenSchemaVersionTooOld_ShouldInformUserAndNotRunUpgrade()
		{
			var currentSchemaVersion = DataRegistry.MinUpgradableDbMajorSchemaVersion - 1;
			var expectedFailureMessage = $"Current database schema version is too old (< {DataRegistry.MinUpgradableDbMajorSchemaVersion}.0). A two-stage upgrade is required. You must upgrade using the General Product release starting with";

			void SetupBeforeUpgrade(string dbName)
			{
				InsertStmData(dbName, "DATABASE_SCHEMA_VERSION", currentSchemaVersion.ToString(CultureInfo.InvariantCulture));
			}

			SetupForAndRunDbUpgrade(disableBI: true, additionalDBSetup: SetupBeforeUpgrade, shouldDatabaseBeUpgraded: false, expectedFailureMessage);
		}

		[UseSnapshotProtection, SnailTest]
		public void TestCheckAndUpgradeDb_WithDatUpgradeDirector_WhenTransformVersionTooOld_ShouldInformUserAndNotRunUpgrade()
		{
			var currentTransformationVersion = DataRegistry.MinDatabaseMajorTransformationVersion - 1;
			var expectedFailureMessage = $"Current database transformation version is too old (< {DataRegistry.MinDatabaseMajorTransformationVersion}.0). A two-stage upgrade is required. You must upgrade using the General Product release starting with";

			void SetupBeforeUpgrade(string dbName)
			{
				InsertStmData(dbName, "DatabaseMajorTransformationVersion", currentTransformationVersion.ToString(CultureInfo.InvariantCulture));
			}

			SetupForAndRunDbUpgrade(disableBI: true, additionalDBSetup: SetupBeforeUpgrade, shouldDatabaseBeUpgraded: false, expectedFailureMessage);
		}

		delegate void AdditionalDatabaseSetup(string dbName);

		void SetupForAndRunDbUpgrade(bool disableBI, AdditionalDatabaseSetup additionalDBSetup = null, bool shouldDatabaseBeUpgraded = true, string expectedFailureText = null, string expectedFailureCaption = null, bool useDatUpgradeDirector = true)
		{
			// Talk to Jacob or Dalmo when this ends up back on amnesty

			using (var tempDirectory = new TempDirectory())
			using (disableBI ? SystemDataRegistry.Instance.BiAuditServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty) : null)
			using (disableBI ? SystemDataRegistry.Instance.BiDataWarehouseServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty) : null)
			{
				var serviceManagerExe = Path.Combine(tempDirectory.DirectoryName, ServiceManagerConstants.ServiceManagerHostExe);
				using (var tempFileStream = TempFile.CreateWithDeleteOnClose())
				{
					var fs = (FileStream)tempFileStream;
					CreateServiceManagerExe(serviceManagerExe, fs.Name);
					using (var serviceManagerProcess = Process.Start(serviceManagerExe))
					{
						try
						{
							PrepareTestMainDatabase(TestMainDb);
							additionalDBSetup?.Invoke(TestMainDb);

							var args = new List<string>
							{
								Db.ServerName,
								TestMainDb,
								ApplicationArguments.OptionNoSplash,
								ApplicationArguments.OptionServiceProcess + Convert.ToBase64String(ProtectedData.Protect(Encoding.UTF8.GetBytes(serviceManagerProcess.Id.ToString()), null, DataProtectionScope.LocalMachine)),
								ApplicationArguments.OptionNotifyOnSuccessfulUpgrade,
								ApplicationArguments.NotificationGroupPK + "94755E71-A87A-4034-8DFA-785773A49607",
							};
							if (useDatUpgradeDirector)
							{
								args.Add(ApplicationArguments.OptionTestAdapter);
								args.Add(ApplicationArguments.OptionScheduledDbUpgrader);
							}
							if (shouldDatabaseBeUpgraded)
							{
								RunEnterpriseOnEmptyDbUsingSeparateAppDomain(args.ToArray());
								AssertEmptyDatabaseWasUpgraded(TestMainDb);
							}
							else
							{
								RunEnterpriseOnEmptyDbUsingSeparateAppDomain(true, expectedFailureText, expectedFailureCaption, args.ToArray());
								AssertEmptyDatabaseWasNotUpgraded(TestMainDb);
							}
						}
						finally
						{
							serviceManagerProcess.Kill();
							tempFileStream.Dispose();  // will be disposed again at end of using - this is to flag the process to close
							serviceManagerProcess.WaitForExit();
							DropTestUpgradeDatabases(TestMainDb);
							CultureInfo.CurrentCulture = DefaultCulture.Instance;
						}
					}
				}
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.BI })]
		public void TestTransformationVersionSetOnDowngrade()
		{
			var actualTranformationVersion = DataRegistry.Instance.DatabaseMajorTransformationVersion;
			DataRegistry.Instance.DatabaseMajorTransformationVersion = actualTranformationVersion + 2;
			var arguments = new ApplicationArguments(new[] { ApplicationArguments.OptionTestAdapter });
			var director = DbUpgraderDirector.New(arguments);
			Assert(director.Execute(arguments));
			AssertEquals(actualTranformationVersion, DataRegistry.Instance.DatabaseMajorTransformationVersion);
		}

		void CreateServiceManagerExe(string target, string flagFile)
		{
			var compiler = new CSharpCodeProvider();
			var options = new CompilerParameters();
			options.ReferencedAssemblies.Add("System.dll");
			options.GenerateExecutable = true;
			options.OutputAssembly = target;

			var sourceCode =
@"using System.IO;
public static class Program
					{
						public static void Main(string[] cmd)
						{
							while(File.Exists(@""" + flagFile + "\"))" +
@"							{
								System.Threading.Thread.Sleep(1000);
							}
						}
					}";

			var result = compiler.CompileAssemblyFromSource(options, sourceCode);
			var output = new string[result.Output.Count];
			result.Output.CopyTo(output, 0);
			AssertEquals(string.Join("\r\n", output), 0, result.Errors.Count);
		}

		void RunEnterpriseOnEmptyDbUsingSeparateAppDomain(params string[] arguments)
		{
			RunEnterpriseOnEmptyDbUsingSeparateAppDomain(false, null, null, arguments);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1157:Do not use System.AppDomain.", Justification = "Pending migration")] // WI00669071 - Do not use System.AppDomain.
		void RunEnterpriseOnEmptyDbUsingSeparateAppDomain(bool expectUpgradeFailure, string expectedFailureText, string expectedFailureCaption, string[] arguments)
		{
			var domain = AppDomain.CreateDomain("UpgradeAssuranceTest_TestCanUpgradeEmptyDatabase");
			domain.SetData("startupArgs", arguments);
			domain.SetData(nameof(expectUpgradeFailure), expectUpgradeFailure);
			domain.SetData(nameof(expectedFailureText), expectedFailureText);
			domain.SetData(nameof(expectedFailureCaption), expectedFailureCaption);

			try
			{
				domain.DoCallBack(CallBack);
			}
			finally
			{
				AppDomain.Unload(domain);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1157:Do not use System.AppDomain.", Justification = "Pending migration")] // WI00669071 - Do not use System.AppDomain.
		static void CallBack()
		{
			TestingState.Setup();
			var startupArgs = (string[])AppDomain.CurrentDomain.GetData("startupArgs");
			var expectUpgradeFailure = (bool)AppDomain.CurrentDomain.GetData("expectUpgradeFailure");
			var usingDatUpgradeDirector = startupArgs.Any(arg => arg.Equals(ApplicationArguments.OptionTestAdapter, StringComparison.OrdinalIgnoreCase));

			var retCode = 0;
			ExceptionReporterTestListener.Instance.StartTest(null, DateTime.Now);
			try
			{
				var director = new ApplicationStartupDirectorForUpgradeEmptyDbTest();
				retCode = director.StartEnterprise(startupArgs);
			}
			finally
			{
				ExceptionReporterTestListener.Instance.EndTest(null, DateTime.Now);
			}
			if (usingDatUpgradeDirector)
			{
				Assert("Scheduled DB upgrade should not be user interactive.", !Globals.IsUserInteractive);
			}

			if (retCode == ExitCodes.DbUpgraderDirectorFailure && !expectUpgradeFailure)
			{
				var outputFileForTest = StartupNotification.ScheduledUpgradeLogger.CurrentOutputFileForTest("UPG");
				if (File.Exists(outputFileForTest.FullName))
				{
					var errorMessage = outputFileForTest != null ? File.ReadAllText(outputFileForTest.FullName) : ErrorReporter.LastExceptionReported?.Message;
					Fail("Database Upgrade Failed" + errorMessage);
				}
			}

			if (expectUpgradeFailure)
			{
				var expectedFailureCaption = (string)AppDomain.CurrentDomain.GetData("expectedFailureCaption");
				var expectedFailureText = (string)AppDomain.CurrentDomain.GetData("expectedFailureText");

				CombineAssertions("Expected database upgrade to fail, but it didn't in the expected way.", () =>
				{
					AssertNotEquals("Upgrade process return value", 0, retCode);
					if (usingDatUpgradeDirector)
					{
						AssertContains("DatDbUpgraderDirector.UpgradeLog", expectedFailureText, DatDbUpgraderDirector.UpgradeLog);
					}
					else
					{
						AssertEquals("LastMessage.WasError", true, UnitTestUserNotification.Instance.LastMessage.WasError);
						AssertEquals("LastMessage.Caption", expectedFailureCaption, UnitTestUserNotification.Instance.LastMessage.Caption);
						AssertStartsWith("LastMessage.Text", expectedFailureText, UnitTestUserNotification.Instance.LastMessage.Text);
					}
				});
			}
			else
			{
				CombineAssertions("Database upgrade failed in some way. The best way to identify the cause is with the debugger, breaking on exception. When running this test on a developer machine, you should map any transforms manually such that they actually run. This involves editing Mapper.cs in the relevant transformation assemblies in the way that DAT does automatically.", () =>
				{
					AssertEquals("Startup return code", 0, retCode);

					foreach (var message in UnitTestUserNotification.Instance.PreviousMessages)
					{
						AssertEquals("Error message was displayed:\r\n" + message.Text, false, message.WasError);
					}
				});
			}
		}

		class ApplicationStartupDirectorForUpgradeEmptyDbTest : ApplicationStartupDirector
		{
			public const string AppLogin = "sysadmin";

			protected override IApplicationStartupTask[] GetEnterpriseStartupTasks(CommandLineArguments arguments)
			{
				var baseTasks = base.GetEnterpriseStartupTasks(arguments);

				for (var i = 0; i < baseTasks.Length; i++)
				{
					if (baseTasks[i] is LoginDirector)
					{
						baseTasks[i] = LoginDirector.Instance;
					}
					else if (baseTasks[i] is IApplicationStartupTaskExceptionHandler)
					{
						baseTasks[i] = new DummyApplicationStartupTaskForUpgradeEmptyDbTest();
					}
					else if (baseTasks[i] is ConfigurationItemChecker)
					{
						baseTasks[i] = new DummyApplicationStartupTaskForUpgradeEmptyDbTest();
					}
				}

				return baseTasks;
			}

			protected override void Application_RunCore(Form mainForm)
			{
			}

			protected internal override void DisposeSplash()
			{
				if (splash != null)
				{
					splash.Dispose();
				}
			}
		}

		class DummyApplicationStartupTaskForUpgradeEmptyDbTest : AbstractApplicationStartupTask
		{
			#region IApplicationStartupTask Members

			protected override bool DoExecute(CommandLineArguments arguments)
			{
				return true;
			}

			protected override bool GetShouldExecute(CommandLineArguments arguments)
			{
				return false;
			}

			public override string TaskDescription
			{
				get { return "This task does not do anything"; }
			}

			public override int FailureExitCode => ExitCodes.TestExitCode;

			#endregion
		}

		#endregion

		#region Run Silent Upgrade Only On Empty Database

		[UseSnapshotProtection(skipTransaction: true), SnailTest]
		public void TestCanUpgradeEmptyMainDatabaseAndReadOnlyStorageDocDatabasesWithNoBI()
		{
			// Talk to Jacob or Dalmo when this ends up back on amnesty

			try
			{
				PrepareTestMainDatabase(TestMainDb);
				PrepareTestStorageDocDatabase(TestEdocsDb1, true);
				PrepareTestStorageDocDatabase(TestEdocsDb2, false);

				Assert("StorageDoc database SD001 should be in a read-only state", !IsDbWriteable(TestEdocsDb1));
				Assert("StorageDoc database SD002 should be in a writable state", IsDbWriteable(TestEdocsDb2));

				using (SystemDataRegistry.Instance.BiAuditServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty))
				using (SystemDataRegistry.Instance.BiDataWarehouseServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty))
				{
					RunSilentUpgradeOnEmptyDbUsingSeparateAppDomain(TestMainDb);
				}

				AssertEmptyDatabaseWasUpgraded(TestMainDb);
				Assert("StorageDoc database SD001 should be in a read-only state", !IsDbWriteable(TestEdocsDb1));
				Assert("StorageDoc database SD002 should be in a writable state", IsDbWriteable(TestEdocsDb2));
			}
			finally
			{
				DropTestUpgradeDatabases(TestEdocsDb1);
				DropTestUpgradeDatabases(TestEdocsDb2);
				DropTestUpgradeDatabases(TestMainDb);
				CultureInfo.CurrentCulture = DefaultCulture.Instance;
			}
		}

		[UseSnapshotProtection(skipTransaction: true), SnailTest]
		public void TestCanUpgradeEmptyMainDatabaseAndReadOnlyStorageDocDatabasesWithBI()
		{
			// Talk to Jacob or Dalmo when this ends up back on amnesty

			try
			{
				PrepareTestMainDatabase(TestMainDb);
				PrepareTestStorageDocDatabase(TestEdocsDb1, true);
				PrepareTestStorageDocDatabase(TestEdocsDb2, false);

				Assert("StorageDoc database SD001 should be in a read-only state", !IsDbWriteable(TestEdocsDb1));
				Assert("StorageDoc database SD002 should be in a writable state", IsDbWriteable(TestEdocsDb2));

				RunSilentUpgradeOnEmptyDbUsingSeparateAppDomain(TestMainDb);

				AssertEmptyDatabaseWasUpgraded(TestMainDb);
				Assert("StorageDoc database SD001 should be in a read-only state", !IsDbWriteable(TestEdocsDb1));
				Assert("StorageDoc database SD002 should be in a writable state", IsDbWriteable(TestEdocsDb2));
			}
			finally
			{
				DropTestUpgradeDatabases(TestEdocsDb1);
				DropTestUpgradeDatabases(TestEdocsDb2);
				DropTestUpgradeDatabases(TestMainDb);
				CultureInfo.CurrentCulture = DefaultCulture.Instance;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1157:Do not use System.AppDomain.", Justification = "Pending migration")] // WI00669071 - Do not use System.AppDomain.
		void RunSilentUpgradeOnEmptyDbUsingSeparateAppDomain(string dbName)
		{
			var domain = AppDomain.CreateDomain("UpgradeAssuranceTest_TestCanUpgradeEmptyDatabase");
			domain.SetData("serverName", Db.ServerName);
			domain.SetData("databaseName", dbName);

			try
			{
				CrossAppDomainDelegate crossAppDomainDelegate = delegate
				{
					var currentDomain = AppDomain.CurrentDomain;
					Db.InitializeDatabaseDetails(
						(string)currentDomain.GetData("serverName"),
						(string)currentDomain.GetData("databaseName"));
					EnterpriseApplicationConfiguration.ConfigureObjectFactory();
					Initialisation.Initialiser.InitialiseWinForms();
					Globals.IsUserInteractive = false;

					using (Db.DisableSchemaVersionCheck())
					{
						TestingState.Setup();
						new UpgradeRunForTest().RunAndAssertResult();
						TestingState.TearDown();
					}
				};
				domain.DoCallBack(crossAppDomainDelegate);
			}
			finally
			{
				AppDomain.Unload(domain);
			}
		}

		class UpgradeRunForTest : IVersionChangeInfo
		{
			public void RunAndAssertResult()
			{
				var stopwatch = Stopwatch.StartNew();
				var upgradeResult = ObjectFactory.Get<IDbUpgraderRunner>().FullSilentUpgrade(this, null, new UpgraderEvent(OnUpgradeEvent));
				stopwatch.Stop();

				AssertEquals("Upgrade Failed" + System.Environment.NewLine + System.Environment.NewLine
					+ upgradeLog.ToString()
					, true, upgradeResult.Successful);

				var maxTotalTime = TimeSpan.FromMinutes(14);
				AssertEquals($"Upgrade process is longer than {maxTotalTime.TotalMinutes} min - " + stopwatch.Elapsed.ToString(@"'['hh\:mm\:ss']'") + System.Environment.NewLine
					+ "The longest tasks:" + System.Environment.NewLine
					+ longestTasks.ToString() + System.Environment.NewLine
					+ "Full upgrade log:" + System.Environment.NewLine
					+ upgradeLog.ToString()
					, true, stopwatch.Elapsed <= maxTotalTime);
			}

			void OnUpgradeEvent(UpgradeEventType eventType, string message)
			{
				var now = DateTime.Now;
				var newMessage = now.ToString("[yyyy-MM-dd HH:mm:ss]\t") + message;
				upgradeLog.AppendLine(newMessage);

				if (prevTime > DateTime.MinValue && now.Subtract(prevTime) >= TimeSpan.FromSeconds(30))
				{
					longestTasks.AppendLine(now.Subtract(prevTime).ToString(@"'['hh\:mm\:ss'] - '") + prevMessage);
				}

				prevTime = now;
				prevMessage = newMessage;
			}

			readonly StringBuilder upgradeLog = new StringBuilder();
			readonly StringBuilder longestTasks = new StringBuilder();
			DateTime prevTime = DateTime.MinValue;
			string prevMessage = "";

			#region IVersionChangeInfo Members

			public string[] AvailableClientDocuments => Array.Empty<string>();

			public VersionLabel DbReferenceVersion_Data { get; } = new VersionLabel(0, 0);
			public VersionLabel DbReferenceVersion_Schema { get; } = new VersionLabel(0, 0);
			public VersionLabel DbReferenceVersion_Script { get; } = new VersionLabel(0, 0);
			public VersionLabel DbReferenceVersion_CoreScript { get; } = new VersionLabel(0, 0);
			public VersionLabel DbReferenceVersion_Transformation { get; } = new VersionLabel(0, 0);
			public VersionLabel DbReferenceVersion_Clr { get; } = new VersionLabel(0, 0);

			public bool IsRequired_Data => true;
			public bool IsRequired_Schema => true;
			public bool IsRequired_Script => true;
			public bool IsRequired_Transformation => true;
			public bool IsRequired_ClientDocuments => false;
			public bool WithPreUpgrade => false;

			#endregion // IVersionChangeInfo Members
		}

		#endregion // Run Silent Upgrade Only On Empty Database

		#region Implementation

		static void AssertEmptyDatabaseWasUpgraded(string dbName)
		{
			AssertEmptyDatabaseUpgradedCore(dbName, shouldHaveBeenUpgraded: true);
		}

		static void AssertEmptyDatabaseWasNotUpgraded(string dbName)
		{
			AssertEmptyDatabaseUpgradedCore(dbName, shouldHaveBeenUpgraded: false);
		}

		static void AssertEmptyDatabaseUpgradedCore(string dbName, bool shouldHaveBeenUpgraded)
		{
			using (var connection = Db.NewAdminConnection())
			{
				var sqlText = FormattableString.Invariant($"SELECT convert(int, convert(nvarchar(max), convert(varbinary(max), SD_BinaryValue))) FROM [{dbName}]..StmData WHERE SD_Name = 'DATABASE_SCHEMA_VERSION'");
				var majorSchemaVersion = connection.ExecuteScalar(sqlText);

				sqlText = FormattableString.Invariant($"SELECT [name] FROM [{dbName}].sys.views WHERE [name] = 'vw_FkReferences'");
				var viewName = connection.ExecuteScalar(sqlText)?.ToString();

				if (shouldHaveBeenUpgraded)
				{
					AssertEquals("Empty DB Major Schema Version after upgrade", SchemaVersion.Application.Major, majorSchemaVersion);
					AssertEquals("vw_FkReferences view should be created", "vw_FkReferences", viewName);

					sqlText = FormattableString.Invariant($"SELECT OH_FullName FROM [{dbName}]..OrgHeader WHERE OH_Code = 'DEMORG'");
					var demoOrgName = connection.ExecuteScalar(sqlText)?.ToString();

					AssertEquals("Demo Organisation Name", "Demo Organisation", demoOrgName);
				}
				else
				{
					AssertNotEquals("DB Major Schema Version should not have been updated", SchemaVersion.Application.Major, majorSchemaVersion);
					AssertNull("vw_FkReferences view should not have been created", viewName);
				}
			}
		}

		static void InsertStmData(string dbName, string stmDataName, string value)
		{
			var sql = FormattableString.Invariant($@"
INSERT [{dbName}]..StmData (
	SD_PK, SD_Name, SD_Type, SD_BinaryValue
) VALUES (
	NEWID(), '{stmDataName}', 'BOL', convert(varbinary(max), convert(nvarchar(max), '{value}'))
)
");
			using (var connection = Db.NewAdminConnection(dbName))
			{
				connection.ExecuteNonQuery(sql);
			}
		}

		bool IsDbWriteable(string dbName)
		{
			return Db.Connection.IsDbWriteable(dbName);
		}

		void PrepareTestMainDatabase(string dbName)
		{
			DropTestUpgradeDatabases(dbName);

			using (var adminConn = Db.NewAdminConnection(Db.SqlMasterDb))
			{
				CreateTestUpgradeDatabase(adminConn, dbName);
				using (((ICurrentDbControl)adminConn).UseDatabase(dbName))
				{
					adminConn.ExecuteNonQuery(DataUtils.SQL_InitialTablesForEmptyDatabase());
				}
			}

			EnsureDbSpecificLoginForTestUpgradeDatabases(dbName);
		}

		void EnsureDbSpecificLoginForTestUpgradeDatabases(string mainTestDbName)
		{
			using (var securityCnx = Db.NewAdminConnection(mainTestDbName))
			{
				((IDbLoginRepair)securityCnx).EnsureRestrictedWriterDbLogin();
			}
		}

		void PrepareTestStorageDocDatabase(string dbName, bool makeDatabaseReadOnly)
		{
			using (var adminConn = Db.NewAdminConnection(Db.SqlMasterDb))
			{
				CreateTestUpgradeDatabase(adminConn, dbName);

				if (makeDatabaseReadOnly)
				{
					var sqlText = string.Format("ALTER DATABASE [{0}] SET {1};", dbName, "READ_ONLY");
					adminConn.ExecuteNonQuery(sqlText);
				}
			}
		}

		void CreateTestUpgradeDatabase(AdminConnection adminConn, string dbName)
		{
			AdoTestUtils.DropDbIfExists(adminConn, dbName);
			adminConn.ExecuteNonQuery(string.Format("CREATE DATABASE [{0}] COLLATE {1};", dbName, Db.DatabaseCollation));
		}

		void DropTestUpgradeDatabases(string mainTestDbName)
		{
			try
			{
				using (var adminConn = Db.NewAdminConnection(Db.SqlMasterDb))
				{
					var sqlText = string.Format(@"
						SELECT name
						FROM sys.databases
						WHERE name like '{0}%' OR name like '{1}%{0}%'",
						mainTestDbName,
						UpgUtils.UpgraderPrefix);

					var dbNames = DataUtils.GetListOfValuesFromQuery(adminConn, sqlText);

					foreach (var dbName in dbNames)
					{
						AdoTestUtils.DropDbIfExists(adminConn, dbName);
					}

					var dropLoginSql = string.Empty;
					var allLoginNames = Db.GetAllLoginNames(mainTestDbName);
					foreach (var loginName in allLoginNames)
					{
						dropLoginSql += string.Format(@"
						IF exists(SELECT null FROM sys.server_principals WHERE name = '{0}') DROP LOGIN [{0}];",
						loginName);
					}
					adminConn.ExecuteNonQuery(dropLoginSql);
				}
			}
			catch (SqlException ex)
			{
				if (new DbErrorMatch(ex).ExceptionType != DbErrorType.CannotOpenDbRequestedInLogin)
				{
					throw;
				}
			}
		}

		const string TestMainDb = "Db151CC7089F6346BCB33A9C0BDCCEB301";
		readonly string TestEdocsDb1 = TestMainDb + "_SD001";
		readonly string TestEdocsDb2 = TestMainDb + "_SD002";

		protected override void TearDown()
		{
			ZFormActivityLogger.Instance.DisableActivityLogger();
			base.TearDown();
		}

		#endregion
	}
}
#endif
