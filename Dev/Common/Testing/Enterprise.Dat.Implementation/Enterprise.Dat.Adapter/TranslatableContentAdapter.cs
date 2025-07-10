using System;
using System.Linq;
using CargoWise.Data;
using CargoWise.Definitions;
using Dat.Integration;
using Dat.Integration.VersionControl;
using Enterprise.Dat.Implementation.Preconditions;
using Enterprise.Initialisation;
using Enterprise.ResourceStrings.Business;
using Enterprise.ResourceStrings.Maintenance.Test;
using NUnit.Framework;

using TestResult = Dat.Integration.TestResult;

namespace Enterprise.Dat.Implementation
{
	public sealed class TranslatableContentAdapter : ITranslatableContentAdapter, IDisposable
	{
		public TranslatableContentAdapter(TranslatableContentAdapterContext adpaterContext)
		{
			AssemblyLoader.Enable();

			this.adpaterContext = adpaterContext;
			translationFileExporter = new TranslationFileExporter();
		}

		void EnsureLocalEnvironment()
		{
			if (!Db.ServerNameIsInitialized)
			{
				dbInitialized = true;
				FixSqlServerNameAndRestartIfNecessary(adpaterContext.Logger);
				using (var connection = LocalDBConnection.GetConnection())
				{
					connection.Open();
					new DbRestorer(connection, adpaterContext.Logger, DatabaseName).RestoreCurrentDbs();
				}
				TestClient.ExecuteStartup(DatabaseName, adpaterContext.SourcePath, adpaterContext.Logger, true);
				dbDisposableAction = Db.DisposableActionForDbConnection();
				DatabaseResourceStringSource.Disable();
				ResourcesDeltaSource.ForcedSourcePath = adpaterContext.SourcePath;
			}
		}

		void FixSqlServerNameAndRestartIfNecessary(ITaskLogger logger)
		{
			var restarter = new SqlServerServiceRestarter();
			var nameCheck = new SqlServerNameChecker(restarter);
			if (!nameCheck.CheckPreconditionMet())
			{
				logger.RecordInfo(nameCheck.ErrorMessage);
			}
			if (!restarter.CheckPreconditionMet())
			{
				logger.RecordInfo(restarter.ErrorMessage);
			}
		}

		void EnsureConsoleApp()
		{
			if (!appIsInitialised)
			{
				appIsInitialised = true;
				Initialiser.InitialiseConsoleApp();
			}
		}

		void EnsureUAREnvironment()
		{
			if (!Db.ServerNameIsInitialized)
			{
				EnsureConsoleApp();
				Db.InitializeDatabaseDetails(TranslationFeedbackMasterInfo.DatabaseServer, TranslationFeedbackMasterInfo.DatabaseName);
				disableSchemaVersionCheck = Db.DisableSchemaVersionCheck();
				dbDisposableAction = Db.DisposableActionForDbConnection();
				ResourcesDeltaSource.ForcedSourcePath = adpaterContext.SourcePath;
			}
		}

		public void ExportAllContent(string contentModule, string targetDirectory)
		{
			if (contentModule == TranslationFeedbackContentModule)
			{
				throw new InvalidOperationException("Must specify Untranslated Only when exporting TranslationFeedback strings");
			}

			if (contentModule == MyAccountLearningCentreModule)
			{
				EnsureConsoleApp();
				new TranslationContentAdapter(contentModule).Export(targetDirectory);
			}
			else if (contentModule == EdiBilling)
			{
				EnsureLocalEnvironment();
				translationFileExporter.ExportAllContent(contentModule, targetDirectory);

				EnsureConsoleApp();
				new TranslationContentAdapter(contentModule).Export(targetDirectory);
			}
			else
			{
				EnsureLocalEnvironment();
				translationFileExporter.ExportAllContent(contentModule, targetDirectory);
			}
		}

		public void ExportUntranslatedContent(string contentModule, string language, string targetDirectory)
		{
			if (contentModule == MyAccountLearningCentreModule)
			{
				EnsureConsoleApp();
				new TranslationContentAdapter(contentModule).Export(targetDirectory, language);
			}
			else if (contentModule == EdiBilling)
			{
				EnsureLocalEnvironment();
				translationFileExporter.ExportUntranslatedContent(contentModule, language, targetDirectory);

				EnsureConsoleApp();
				new TranslationContentAdapter(contentModule).Export(targetDirectory, language);
			}
			else
			{
				if (contentModule == TranslationFeedbackContentModule)
				{
					EnsureUAREnvironment();
					TranslationFeedbackContentAdapter.Export(language, targetDirectory);
				}
				else
				{
					EnsureLocalEnvironment();
					translationFileExporter.ExportUntranslatedContent(contentModule, language, targetDirectory);
				}
			}
		}

		public void ImportTranslatedContent(string contentModule, string language, string contentDirectory, IWorkspaceAccess workspace)
		{
			if (contentModule == MyAccountLearningCentreModule)
			{
				EnsureConsoleApp();
				new TranslationContentAdapter(contentModule).Import(language, contentDirectory);
			}
			else if (contentModule == EdiBilling)
			{
				EnsureLocalEnvironment();
				using (TestingState.SuspendIsRunningTests())
				{
					translationFileExporter.ImportTranslatedContent(contentModule, language, contentDirectory, workspace);
				}

				EnsureConsoleApp();
				new TranslationContentAdapter(contentModule).Import(language, contentDirectory);
			}
			else
			{
				EnsureLocalEnvironment();
				using (TestingState.SuspendIsRunningTests())
				{
					if (contentModule == TranslationFeedbackContentModule)
					{
						TranslationFeedbackContentAdapter.Import(language, contentDirectory);
					}
					else
					{
						translationFileExporter.ImportTranslatedContent(contentModule, language, contentDirectory, workspace);
					}
				}
			}
		}

		public string HandleFailures(string contentModule, string language, IWorkspaceAccess workspace, TestResult[] failedTests, out bool reshelve)
		{
			return translationFileExporter.HandleFailures(contentModule, language, workspace, failedTests, out reshelve);
		}

		public void Dispose()
		{
			if (translationFileExporter != null)
			{
				translationFileExporter.Dispose();
			}
			if (dbInitialized)
			{
				DropDatabases();
			}
			dbDisposableAction?.Dispose();
			disableSchemaVersionCheck?.Dispose();
			GC.SuppressFinalize(this);
		}

		void DropDatabases()
		{
			string[] dbs;
			using (var adminConnection = Db.NewAdminConnection(LocalDBConnection.GetServerName(), DatabaseName))
			{
				dbs = adminConnection.GetDatabases(DatabaseType.All & ~DatabaseType.EDW).Where(db => db.StartsWith(DatabaseName, StringComparison.OrdinalIgnoreCase)).ToArray();
			}
			using (var adminConnection = Db.NewAdminConnection(LocalDBConnection.GetServerName(), Db.SqlMasterDb))
			{
				foreach (var db in dbs)
				{
					AdoTestUtils.DropDbIfExists(adminConnection, "DATREF-" + db);
					AdoTestUtils.DropDbIfExists(adminConnection, db);
				}
			}
		}

		readonly TranslatableContentAdapterContext adpaterContext;
		readonly TranslationFileExporter translationFileExporter;
		bool dbInitialized;
		bool appIsInitialised;
		IDisposable dbDisposableAction;
		IDisposable disableSchemaVersionCheck;
		public const string DatabaseName = "OdysseyGL";
		const string TranslationFeedbackContentModule = "TranslationFeedback";
		const string MyAccountLearningCentreModule = "MyAccountLearningCentre";
		const string EdiBilling = "Billing";
	}
}
