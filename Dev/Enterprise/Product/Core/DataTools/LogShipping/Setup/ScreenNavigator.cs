#define SuppressResourceStringsCheckRegion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CargoWise.Common;

namespace Enterprise.LogShipping.Setup
{
	public enum ScreenEnum
	{
		SecondaryServer,
		SecondaryDatabase,
		PrimaryServer,
		PrimaryDatabase,
		PrimaryEDocsDatabases,
		SourceDirectory,
		LocalCopyDirectory,
		InitializeSecondaryDb,
		FinalSetupScreen,
	}

	public class ScreenNavigator
	{
		public ScreenNavigator()
		{
			CurrentScreen = ScreenEnum.SecondaryServer;
		}

		public void MoveBack()
		{
			if (PreviousScreensStack.Count > 0)
			{
				currentScreen = PreviousScreensStack.Pop();
			}
		}

		public void MoveNext(object value)
		{
			bool result = true;
			bool moveNext = true;

			switch (CurrentScreen)
			{
				case ScreenEnum.SecondaryServer:
					SetupInfo = new LogShippingInfo();
					result = CheckSecondaryServer(value) && StartSQLServerAgent();
					if (result && !CheckConfiguredLogShippingSecondaryDatabasesExists())
					{
						CurrentScreen = ScreenEnum.PrimaryServer;
						moveNext = false;
					}
					break;

				case ScreenEnum.SecondaryDatabase:

					object[] valueSetupAction = value as object[];

					if (valueSetupAction != null && valueSetupAction.Length > 0 && valueSetupAction[0] is SetupAction)
					{
						SetupAction setupAction = (SetupAction)valueSetupAction[0];
						LogShippingInfo newSetupInfo = null;
						object[] valueLogShipping;

						switch (setupAction)
						{
							case SetupAction.Setup:

								if (setupInfo != null)
								{
									newSetupInfo = new LogShippingInfo { SecondaryServer = SetupInfo.SecondaryServer };
								}

								SetupInfo = newSetupInfo;
								break;

							case SetupAction.Change:

								valueLogShipping = (object[])value;

								if (valueLogShipping.Length > 1)
								{
									newSetupInfo = (LogShippingInfo)valueLogShipping[1];
								}

								result = CheckSecondaryDatabaseWasSelected(newSetupInfo);

								if (result)
								{
									SetupInfo = newSetupInfo;
									CurrentScreen = ScreenEnum.PrimaryEDocsDatabases;
									moveNext = false;
								}
								break;

							case SetupAction.Remove:

								valueLogShipping = (object[])value;

								if (valueLogShipping.Length > 1)
								{
									newSetupInfo = (LogShippingInfo)valueLogShipping[1];
								}

								result = CheckSecondaryDatabaseWasSelected(newSetupInfo);
								if (result && newSetupInfo != null)
								{
									SetupInfo = newSetupInfo;
									SetupInfo.ShouldLogShipEDocsDatabases = true;
									CurrentScreen = ScreenEnum.FinalSetupScreen;
									moveNext = false;
								}
								break;
						}

						if (setupInfo != null)
						{
							SetupInfo.SetupAction = setupAction;
						}
					}

					break;

				case ScreenEnum.PrimaryServer:
					result = CheckPrimaryServer(value) && GetPrimaryDatabases();
					break;

				case ScreenEnum.PrimaryDatabase:
					result = CheckPrimaryDatabase((string)value);

					if (result && SetupInfo.MainDatabase != null && SetupInfo.MainDatabase.DependentDatabases.Count == 0)
					{
						CurrentScreen = ScreenEnum.SourceDirectory;
						moveNext = false;
					}

					break;

				case ScreenEnum.PrimaryEDocsDatabases:

					if (value != null)
					{
						SetupInfo.ShouldLogShipEDocsDatabases = (bool)value;
						FindNewDependentDatabases();
					}

					break;

				case ScreenEnum.SourceDirectory:
					result = CheckSourceDirectory((string)value) && GetListOfBackups();
					break;

				case ScreenEnum.LocalCopyDirectory:
					result = CheckAndSetLocalDirectory((string)value);
					break;

				case ScreenEnum.InitializeSecondaryDb:

					if (value != null)
					{
						result = CheckBackupFileName((((DatabaseInitialisationOptions)value) & DatabaseInitialisationOptions.Reinitialise) == DatabaseInitialisationOptions.Reinitialise)
								 && (!SetupInfo.ShouldInitializeSecondaryDb || InitializeSeconaryDatabase((((DatabaseInitialisationOptions)value) & DatabaseInitialisationOptions.OnlyGenerateScript) == DatabaseInitialisationOptions.OnlyGenerateScript))
								 && CheckSecondaryDatabasesExist();
					}
					break;
			}

			if (result)
			{
				if (moveNext)
				{
					CurrentScreen++;
				}

				Success();
			}
			else
			{
				Failure();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
#if DEBUG
		protected virtual
#endif
 bool CheckSecondaryDatabasesExist()
		{
			DatabaseInfo dbInfo = SetupInfo.GetDatabaseInfoListToProcess().Find(dbInfo1 => !dbInfo1.SecondaryDatabaseExists());
			bool result = dbInfo == null;

			if (!result)
			{
				ShowErrorMessage(string.Format("Secondary database {0} hasn't been properly restored.", dbInfo.SecondaryDatabaseName));
			}

			return result;
		}

		void FindNewDependentDatabases()
		{
			if (SetupInfo.SetupAction == SetupAction.Change && SetupInfo.PrimaryServer != null && SetupInfo.MainDatabase != null)
			{
				var linkedEDocsDatabases = DbManager.GetPrimaryLinkedEDocsDatabases(SetupInfo);

				if (linkedEDocsDatabases != null)
				{
					foreach (string eDocsDbName in linkedEDocsDatabases)
					{
						if (SetupInfo.MainDatabase.DependentDatabases.Find(dbInfo => dbInfo.DatabaseName == eDocsDbName) == null)
						{
							SetupInfo.MainDatabase.DependentDatabases.Add(new DependentDatabaseInfo(SetupInfo.MainDatabase, eDocsDbName) { IsNew = true });
						}
					}
				}
			}
		}

		public void Setup()
		{
			LogShippingConfigurator configurator = new LogShippingConfigurator();
			configurator.OnShowMessage += OnShowMessage;

			if (SetupInfo.SecondaryServer != null && configurator.Configure(SetupInfo))
			{
				Success();
			}
			else
			{
				Failure();
			}
		}

		#region SecondaryServer

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		bool CheckSecondaryServer(object value)
		{
			bool result = false;
			SqlServerInfo server = value as SqlServerInfo;
			if (server != null)
			{
				ShowMessage(string.Format(CheckingConnectionMessage, server.FullInstanceName));

				if (DbManager.CheckConnection(server.FullInstanceName))
				{
					result = CheckSecondaryServerName(server);
					if (result)
					{
						ShowSuccessMessage();
					}
				}
				else
				{
					ShowErrorMessage(DbManager.LastErrorMessage);
				}
			}
			else
			{
				string serverName = (string)value;
				if (!string.IsNullOrEmpty(serverName))
				{
					ShowMessage(string.Format(CheckingConnectionMessage, serverName));

					if (DbManager.CheckConnection(serverName))
					{
						ShowSuccessMessage();
						ShowMessage("Getting server version.");
						server = DbManager.GetSqlServerInfoFromServerName(serverName);
						if (server != null)
						{
							result = CheckSecondaryServerName(server);
							if (result)
							{
								ShowSuccessMessage();
							}
						}
						else
						{
							ShowErrorMessage(DbManager.LastErrorMessage);
						}
					}
					else
					{
						ShowErrorMessage(DbManager.LastErrorMessage);
					}
				}
				else
				{
					ShowErrorMessage("Server instance was not entered");
				}
			}
			return result;
		}

		bool CheckSecondaryServerName(SqlServerInfo secondaryServer)
		{
			Argument.NotNull(secondaryServer, nameof(secondaryServer));

			bool result = false;

			if (DbManager.CheckSqlServerName(secondaryServer.FullInstanceName))
			{
				SetupInfo.SecondaryServer = secondaryServer;
				result = true;
			}
			else
			{
				ShowErrorMessage(DbManager.LastErrorMessage);
			}

			return result;
		}

		bool CheckConfiguredLogShippingSecondaryDatabasesExists()
		{
			bool result = false;

			if (SetupInfo.SecondaryServer != null)
			{
				ExistingLogShippingConfigurations = DbManager.GetSecondaryDatabasesInfo(SetupInfo.SecondaryServer);
			}

			if (ExistingLogShippingConfigurations != null)
			{
				result = ExistingLogShippingConfigurations.Length > 0;
			}
			else
			{
				ShowErrorMessage(DbManager.LastErrorMessage);
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
#if DEBUG
		protected virtual
#endif
 bool StartSQLServerAgent()
		{
			bool result = true;
			try
			{
				ShowMessage("Starting SQL Server Agent.");

				if (SetupInfo.SecondaryServer != null && !string.IsNullOrEmpty(SetupInfo.SecondaryServer.SQLServerAgentName))
				{
					ServiceController sqlService = new ServiceController(SetupInfo.SecondaryServer.SQLServerAgentName);
					if (sqlService.Status == ServiceControllerStatus.Stopped)
					{
						sqlService.Start();
						sqlService.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(ServiceStartingTimeOut));
					}
					ShowSuccessMessage();
				}
			}
			catch (InvalidOperationException)
			{
				result = false;
				ShowErrorMessage("Could not start SQL Agent, please check it is properly installed and make sure the service log on windows account has 'sysadmin' privileges to SQL Server.");
			}
			catch (System.ServiceProcess.TimeoutException)
			{
				result = false;
				ShowErrorMessage("SQL Agent was started but then stopped. Probably an error occurred during running. Please have a look to the error log file (SQLAGENT.OUT) to get error description.");
			}
			return result;
		}

		const int ServiceStartingTimeOut = 10;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		const string CheckingConnectionMessage = "Checking connection for [{0}] SQL Server.";

		#endregion

		#region SecondaryDatabase

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		bool CheckSecondaryDatabaseWasSelected(object value)
		{
			bool result = true;

			if (value as LogShippingInfo == null)
			{
				result = false;
				ShowErrorMessage("Secondary database was not selected.");
			}

			return result;
		}

		#endregion

		#region PrimaryServer

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		bool CheckPrimaryServer(object value)
		{
			bool result = false;
			SqlServerInfo server = value as SqlServerInfo;
			if (server != null)
			{
				ShowMessage(string.Format(CheckingConnectionAndVersionMessage, server.FullInstanceName));
				result = CheckPrimaryAndSecondaryServersDiffer(server) && CheckConnectionAndVersion(server);
			}
			else
			{
				string serverName = (string)value;
				if (!string.IsNullOrEmpty(serverName))
				{
					ShowMessage(string.Format(CheckingConnectionAndVersionMessage, serverName));

					server = DbManager.GetSqlServerInfoFromServerName(serverName);
					if (server != null)
					{
						result = CheckPrimaryAndSecondaryServersDiffer(server) && CheckConnectionAndVersion(server);
					}
					else
					{
						ShowErrorMessage(DbManager.LastErrorMessage);
					}
				}
				else
				{
					ShowErrorMessage("Server name was not entered.");
				}
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
#if DEBUG
		protected virtual
#endif
 bool CheckPrimaryAndSecondaryServersDiffer(SqlServerInfo server)
		{
			Argument.NotNull(server, nameof(server));

			string fullInstanceName = (SetupInfo.SecondaryServer != null) ? SetupInfo.SecondaryServer.FullInstanceName : string.Empty;

			bool result = server.FullInstanceName != fullInstanceName;

			if (!result)
			{
				ShowErrorMessage("Primary and secondary servers cannot be the same server.");
			}

			return result;
		}

		bool CheckConnectionAndVersion(SqlServerInfo server)
		{
			Argument.NotNull(server, nameof(server));

			bool result = false;

			if (DbManager.TryConnectWithOdysseyAdminLogin(server.FullInstanceName))
			{
				result = CheckPrimaryServerVersion(server);

				if (result)
				{
					ShowSuccessMessage();
				}
			}
			else
			{
				ShowErrorMessage(DbManager.LastErrorMessage);
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		bool CheckPrimaryServerVersion(SqlServerInfo primaryServer)
		{
			Argument.NotNull(primaryServer, nameof(primaryServer));

			bool result = false;

			if (SetupInfo.SecondaryServer != null && primaryServer.Version.Major == SetupInfo.SecondaryServer.Version.Major)
			{
				SetupInfo.PrimaryServer = primaryServer;
				result = true;
			}
			else
			{
				ShowErrorMessage("Primary server version should be same as secondary server version.");
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		bool GetPrimaryDatabases()
		{
			bool result = true;

			ShowMessage("Getting CargoWise One databases.");

			if (SetupInfo.PrimaryServer != null)
			{
				PrimaryServerEnterpriseDatabases = DbManager.GetEnterpriseDatabases(SetupInfo.PrimaryServer.FullInstanceName);
			}

			if (PrimaryServerEnterpriseDatabases != null && PrimaryServerEnterpriseDatabases.Length > 0)
			{
				ShowSuccessMessage();
			}
			else
			{
				result = false;
				ShowErrorMessage(!string.IsNullOrEmpty(DbManager.LastErrorMessage) ? DbManager.LastErrorMessage : "There are no candidate databases.");
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		const string CheckingConnectionAndVersionMessage = "Checking connection and version for [{0}] SQL Server.";

		#endregion

		#region PrimaryDatabase

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		bool CheckPrimaryDatabase(string value)
		{
			var result = false;

			if (!string.IsNullOrEmpty(value))
			{
				var secondaryDatabase = value;

				if (!SecondaryDatabaseConfigured(secondaryDatabase))
				{
					SetupInfo.MainDatabase = new MainDatabaseInfo(SetupInfo, value) { ShouldInitialise = true };

					string[] eDocsDatabases = SetupInfo.PrimaryServer != null ? DbManager.GetPrimaryLinkedEDocsDatabases(SetupInfo, ShowErrorMessage) : null;

					//DbManager.primaryDbLogins = DbManager.GetGroupDatabaseLogins(SetupInfo.PrimaryServer.FullInstanceName, SetupInfo.MainDatabase.DatabaseName);

					if (eDocsDatabases != null)
					{
						foreach (string eDocsDbName in eDocsDatabases)
						{
							SetupInfo.MainDatabase.DependentDatabases.Add(new DependentDatabaseInfo(SetupInfo.MainDatabase, eDocsDbName) { ShouldInitialise = true });
						}

						result = true;
					}
				}
				else
				{
					ShowErrorMessage(string.Format(
						@"Secondary database - [{0}] is already configured for selected primary database. Please choose another primary one, or go back and select re-initialize the existing secondary database or select another secondary server instance.",
						secondaryDatabase));
				}
			}
			else
			{
				ShowErrorMessage("Database was not selected.");
			}

			return result;
		}

		bool SecondaryDatabaseConfigured(string dbName)
		{
			bool result = false;

			if (ExistingLogShippingConfigurations != null && ExistingLogShippingConfigurations.Length > 0)
			{
				foreach (LogShippingInfo info in ExistingLogShippingConfigurations)
				{
					if (info != null && info.MainDatabase != null && info.MainDatabase.SecondaryDatabaseName == dbName)
					{
						result = true;
						break;
					}
				}
			}

			return result;
		}

		#endregion

		#region Source Directory

		public static bool CheckDirectoryFormat(string value)
		{
			Argument.NotNull(value, nameof(value));

			return Regex.IsMatch(value, @"^\\\\[\w-\.]+\\[\w\s\p{P}\p{S}][^/]*$");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		bool CheckSourceDirectory(string value)
		{
			var result = false;

			if (value != null && CheckDirectoryFormat(value))
			{
				if (!string.IsNullOrEmpty(value) && Directory.Exists(value))
				{
					SetupInfo.BackupSourceDirectory = value;
					result = true;
				}
				else
				{
					ShowErrorMessage("Entered directory does not exist or access is denied.");
				}
			}
			else
			{
				ShowErrorMessage("Entered directory is not in UNC.");
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		bool GetListOfBackups()
		{
			bool result = true;

			if (SetupInfo.MainDatabase != null)
			{
				ShowMessage(String.Format(
					"Getting list of backup files from source folder for '{0}' database{1}",
					SetupInfo.MainDatabase.DatabaseName,
					SetupInfo.ShouldLogShipEDocsDatabases ? " and its linked eDocs databases" : ""
				));
			}

			Dictionary<string, List<string>> backups = null;

			if (SetupInfo.SecondaryServer != null)
			{
				backups = DbManager.GetPrimaryDatabaseBackups(SetupInfo);
			}

			if (((backups != null && backups.Count > 0) || SetupInfo.SetupAction == SetupAction.Change)
				&& String.IsNullOrEmpty(DbManager.LastErrorMessage))
			{
				ListOfBackups = backups;
			}
			else
			{
				result = false;

				if (String.IsNullOrEmpty(DbManager.LastErrorMessage))
				{
					ShowMessage("There are no backup files in the entered directory. Secondary database is not initialized yet and requires full backup of the primary database for initialization. Please copy full backup of the primary database to the source folder, and click next again.");
				}
				else
				{
					ShowErrorMessage(DbManager.LastErrorMessage);
				}
			}

			return result;
		}

		#endregion

		#region Local Directory

		bool CheckAndSetLocalDirectory(string value)
		{
			var error = "";
			bool result = CheckLocalDirectory(value, out error);

			if (result)
			{
				SetupInfo.BackupLocalCopyDirectory = value;
			}
			else
			{
				ShowErrorMessage(error);
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public static bool CheckLocalDirectory(string value, out string error)
		{
			bool result = false;
			error = "";

			if (!string.IsNullOrEmpty(value) && Directory.Exists(value))
			{
				Match match = Regex.Match(value, @"^([\p{Lu}\p{Ll}]):\\[\w\s\p{P}\p{S}][^/]*$");

				if (match.Success && new DriveInfo(match.Groups[1].Value).DriveType == DriveType.Fixed)
				{
					result = true;
				}
				else
				{
					error = "Entered directory is not local.";
				}
			}
			else
			{
				error = "Entered directory does not exist or access is denied.";
			}

			return result;
		}

		#endregion

		#region InitializeSecondaryDb

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		bool CheckBackupFileName(bool value)
		{
			bool result = true;

			SetupInfo.ShouldInitializeSecondaryDb = false;

			if (!SetupInfo.SecondaryDatabasesExist() || value)
			{
				if (SetupInfo.GetDatabaseInfoListToProcess().Find(dbInfo => dbInfo.ShouldInitialise && string.IsNullOrEmpty(dbInfo.BackupFileName)) == null)
				{
					SetupInfo.ShouldInitializeSecondaryDb = true;
				}
				else
				{
					ShowErrorMessage("Backup was not selected.");
					result = false;
				}
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI011:TempPathRule", Justification = "Outside of Enterprise")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1078:Do not use Process.Start to open a file or url, use WebUrlLauncher or FileOpener for proper integration with Remote Desktop Services. False alarm if you are running a process for a reason other than to open a file or url.", Justification = "Standalone app")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Standalone app")]
#if DEBUG
		protected virtual
#endif
 bool InitializeSeconaryDatabase(bool onlyGenerateScript)
		{
			bool result = true;

			RestoreManager restoreManager = new RestoreManager();
			restoreManager.OnShowMessage += OnShowMessage;

			if (onlyGenerateScript)
			{
				string tempFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".sql");

				if (!string.IsNullOrEmpty(tempFilePath))
				{
					using (StreamWriter writer = new StreamWriter(tempFilePath))
					{
						foreach (DatabaseInfo dbInfo in SetupInfo.GetDatabaseInfoListToProcess())
						{
							if (dbInfo != null &&
								dbInfo.SetupInfo.PrimaryServer != null &&
								dbInfo.SetupInfo.SecondaryServer != null &&
								!string.IsNullOrEmpty(dbInfo.SecondaryDatabaseName) &&
								!string.IsNullOrEmpty(dbInfo.BackupFullFileName))
							{
								writer.Write(restoreManager.GetRestoreScript(dbInfo));
							}
						}
					}
				}

				Process.Start("Notepad.exe", tempFilePath);
				MessageBox.Show("Please run the generated script against your secondary server and then click OK to continue.", "Restore script generated");
			}
			else
			{
				result = restoreManager.Restore(SetupInfo);
			}

			return result;
		}

		#endregion

		#region Events

		void ShowSuccessMessage()
		{
			ShowMessage("Success.");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		void ShowErrorMessage(string message)
		{
			ShowMessage(string.Format("Error: {0}", message));
		}

		void ShowMessage(string message)
		{
			if (OnShowMessage != null)
			{
				OnShowMessage(message);
			}
		}

		void Success()
		{
			if (OnSuccess != null)
			{
				OnSuccess();
			}
		}

		void Failure()
		{
			if (OnFailure != null)
			{
				OnFailure();
			}
		}

		public event NotificationDelegate OnShowMessage;
		public event ActionResultDelegate OnSuccess;
		public event ActionResultDelegate OnFailure;

		#endregion

		public Dictionary<string, List<string>> ListOfBackups { get; private set; }
		public string[] PrimaryServerEnterpriseDatabases { get; private set; }
		public LogShippingInfo[] ExistingLogShippingConfigurations { get; protected set; }

		public LogShippingInfo SetupInfo
		{
			get
			{
				if (setupInfo == null)
				{
					throw new InvalidOperationException("SetupInfo was queried before it was initialized");
				}

				return setupInfo;
			}
			protected set { setupInfo = value; }
		}
		LogShippingInfo setupInfo;

		DbManager DbManager
		{
			get
			{
				if (dbManager == null)
				{
					dbManager = new DbManager();
				}
				return dbManager;
			}
		}
		DbManager dbManager;

		#region Previous & Current Screens

		public ScreenEnum CurrentScreen
		{
			get { return currentScreen; }
			protected set
			{
				PreviousScreensStack.Push(currentScreen);
				currentScreen = value;
			}
		}
		ScreenEnum currentScreen;

		public ScreenEnum PreviousScreen
		{
			get
			{
				if (PreviousScreensStack.Count <= 0)
				{
					throw new InvalidOperationException("PreviousScreensStack was queried before it was initialized");
				}

				return PreviousScreensStack.Peek();
			}
		}

		Stack<ScreenEnum> PreviousScreensStack
		{
			get
			{
				if (previousScreensStack == null)
				{
					previousScreensStack = new Stack<ScreenEnum>();
				}
				return previousScreensStack;
			}
		}
		Stack<ScreenEnum> previousScreensStack;

		#endregion
	}
}
