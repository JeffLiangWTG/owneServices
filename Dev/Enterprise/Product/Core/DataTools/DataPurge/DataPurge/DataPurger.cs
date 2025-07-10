using System;
using System.Collections.Generic;
using System.Diagnostics;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataPurge
{
	class DataPurger : NonPersistentBusinessObject, IObsoleteValidation
	{
		public DataPurger() : base(new BusinessObjectFactory())
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			Output_ReadOnly = true;
			InitialisePurgeCompanyPk();
			InitialiseBackupFilePath();
		}

		void InitialisePurgeCompanyPk()
		{
			if (GlbCompany.CurrentCompany != null)
			{
				fCompanySpecificPk = GlbCompany.CurrentCompany.PK;
			}
		}

		readonly RepositoryManager PurgeScriptRepositoryManager = new RepositoryManager();

		#region Properties

		#region BackupFilePath

		[MaxLength(1000)]
		public ZString BackupFilePath
		{
			get { return fBackupFilePath; }
			set { SetNonPersistentPropertyValue(BackupFilePathInfo, ref fBackupFilePath, value); }
		}
		ZString fBackupFilePath;

		public ZPropertyInfo BackupFilePathInfo
		{
			get { return GetZPropertyInfo(nameof(BackupFilePath)); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1054:DoNotHardcodePaths", Justification = "This is a suggested directory to place the backup file as a fallback for the BackupDirectoryPath registry item.")]
		protected void InitialiseBackupFilePath()
		{
			string backupFolder = Env.Registry.BackupDirectoryPath.Trim();

			if (string.IsNullOrEmpty(backupFolder))
			{
				backupFolder = @"C:\"; // This is a suggested directory to place the backup file as a fallback for the BackupDirectoryPath registry item.
			}

			if (!backupFolder.EndsWith(@"\"))
			{
				backupFolder += @"\";
			}

			BackupFilePath = backupFolder + Db.DatabaseName + ZDateTime.Now.ToString("_yyyyMMdd_HHmmss") + ".bak";
		}

		#endregion

		#region Purge Script Groups

		#region System Level Purge

		#region Has Rating Info Purge Script

		ZBool fHasRatingInfoPurgeScript;
		public ZBool HasRatingInfoPurgeScript
		{
			get
			{
				return fHasRatingInfoPurgeScript;
			}
			set
			{
				if (value != fHasRatingInfoPurgeScript)
				{
					SetNonPersistentPropertyValue(HasRatingInfoPurgeScriptInfo, ref fHasRatingInfoPurgeScript, value);

					if (value)
					{
						HasQuotationsPurgeScriptInfo.Value = ZBool.True;
					}
					else
					{
						HasNonProxyOrganisationsPurgeScriptInfo.Value = ZBool.False;
					}
				}
			}
		}

		public ZPropertyInfo HasRatingInfoPurgeScriptInfo
		{
			get { return GetZPropertyInfo(nameof(HasRatingInfoPurgeScript)); }
		}

		#endregion

		#region Has Product Info Purge Script

		ZBool fHasProductInfoPurgeScript;
		public ZBool HasProductInfoPurgeScript
		{
			get
			{
				return fHasProductInfoPurgeScript;
			}
			set
			{
				if (value != fHasProductInfoPurgeScript)
				{
					SetNonPersistentPropertyValue(HasProductInfoPurgeScriptInfo, ref fHasProductInfoPurgeScript, value);
				}
			}
		}

		public ZPropertyInfo HasProductInfoPurgeScriptInfo
		{
			get { return GetZPropertyInfo(nameof(HasProductInfoPurgeScript)); }
		}

		#endregion

		#region Has Non Proxy Organisations Purge Script

		ZBool fHasNonProxyOrganisationsPurgeScript;
		public ZBool HasNonProxyOrganisationsPurgeScript
		{
			get
			{
				return fHasNonProxyOrganisationsPurgeScript;
			}
			set
			{
				if (value != fHasNonProxyOrganisationsPurgeScript)
				{
					SetNonPersistentPropertyValue(HasNonProxyOrganisationsPurgeScriptInfo, ref fHasNonProxyOrganisationsPurgeScript, value);

					if (value)
					{
						HasRatingInfoPurgeScriptInfo.Value = ZBool.True;
					}
				}
			}
		}

		public ZPropertyInfo HasNonProxyOrganisationsPurgeScriptInfo
		{
			get { return GetZPropertyInfo(nameof(HasNonProxyOrganisationsPurgeScript)); }
		}

		#endregion

		#region Has Non System Charge Codes Purge Script

		ZBool fHasNonSystemChageCodesPurgeScript;
		public ZBool HasNonSystemChageCodesPurgeScript
		{
			get
			{
				return fHasNonSystemChageCodesPurgeScript;
			}
			set
			{
				if (value != fHasNonSystemChageCodesPurgeScript)
				{
					SetNonPersistentPropertyValue(HasNonSystemChageCodesPurgeScriptInfo, ref fHasNonSystemChageCodesPurgeScript, value);
				}
			}
		}

		public ZPropertyInfo HasNonSystemChageCodesPurgeScriptInfo
		{
			get { return GetZPropertyInfo(nameof(HasNonSystemChageCodesPurgeScript)); }
		}

		#endregion

		#region Has Tariff Info Purge Script

		ZBool fHasTariffInfoPurgeScript;
		public ZBool HasTariffInfoPurgeScript
		{
			get
			{
				return fHasTariffInfoPurgeScript;
			}
			set
			{
				if (value != fHasTariffInfoPurgeScript)
				{
					SetNonPersistentPropertyValue(HasTariffInfoPurgeScriptInfo, ref fHasTariffInfoPurgeScript, value);
				}
			}
		}

		public ZPropertyInfo HasTariffInfoPurgeScriptInfo
		{
			get { return GetZPropertyInfo(nameof(HasTariffInfoPurgeScript)); }
		}

		#endregion

		#region Has Quotations Purge Script

		public ZBool HasQuotationsPurgeScript
		{
			get { return hasQuotationsPurgeScript; }
			set
			{
				if (value != hasQuotationsPurgeScript)
				{
					SetNonPersistentPropertyValue(HasQuotationsPurgeScriptInfo, ref hasQuotationsPurgeScript, value);

					if (!value)
					{
						HasRatingInfoPurgeScriptInfo.Value = ZBool.False;
					}
				}
			}
		}
		ZBool hasQuotationsPurgeScript;

		public ZPropertyInfo HasQuotationsPurgeScriptInfo
		{
			get { return GetZPropertyInfo(nameof(HasQuotationsPurgeScript)); }
		}

		#endregion

		#endregion

		#region Company Specific Purge

		#region CompanySpecificPk

		public ZGuid CompanySpecificPk
		{
			get { return fCompanySpecificPk; }
			set
			{
				if (value != fCompanySpecificPk)
				{
					SetNonPersistentPropertyValue(CompanySpecificPkInfo, ref fCompanySpecificPk, value);
					ValidateCompanySpecificPk();
				}
			}
		}

		public ZPropertyInfo CompanySpecificPkInfo
		{
			get { return GetZPropertyInfo(nameof(CompanySpecificPk)); }
		}

		public void ValidateCompanySpecificPk()
		{
			if (!IsValidationSuspended)
			{
				CompanySpecificPkInfo.ClearAllNotifications();
				MandatoryValidation.CheckEntered(CompanySpecificPkInfo);
				TypeValidation.CheckValidGuid(CompanySpecificPkInfo);
			}
		}

		ZGuid fCompanySpecificPk;

		#endregion

		#endregion

		#endregion

		#region Output

		[MaxLength(200000)]
		public ZString Output
		{
			get
			{
				return fOutput;
			}
			set
			{
				SetNonPersistentPropertyValue(OutputInfo, ref fOutput, value);
			}
		}
		ZString fOutput;

		public ZPropertyInfo OutputInfo
		{
			get { return GetZPropertyInfo(nameof(Output)); }
		}

		public bool Output_ReadOnly { get; set; }

		#endregion

		#region NonDemoCompanies

		public GlbCompanyCollection NonDemoCompanies
		{
			get
			{
				if (fNonDemoCompanies == null)
				{
					ZQuery nonDemoCompanyFilter = new ZQuery(GlbCompanySchema.GC_Code, SQLComparisonOperator.NotEqual, "DEM");
					fNonDemoCompanies = new GlbCompanyCollection(Factory, nonDemoCompanyFilter);
				}
				return fNonDemoCompanies;
			}
		}

		protected GlbCompanyCollection fNonDemoCompanies;

		#endregion

		#endregion

		#region Backup & Purge

		public void BackupAndPurgeSystemWideTransactional()
		{
			BackupAndPurgeCatchingExceptions();
		}

		public void BackupAndPurgeSystemWideWithoutTransaction()
		{
			if (IsNonTransactionalPurgeAllowed)
			{
				BackupAndPurgeCatchingExceptions(isCompanySpecificPurge: false, isTransactional: false);
			}
			else
			{
				AppendOutputLine(Res.GetString("44C0B47C-4397-41D2-A462-8BC7CB1D2B2E", "No purge performed as Data Purge requires a single atomic transaction in production systems."));
			}
		}

		public bool IsNonTransactionalPurgeAllowed
		{
			get
			{
				if (isNonTransactionalPurgeAllowed == null)
				{
					isNonTransactionalPurgeAllowed = (ObjectFactory.Get<IProductRegistration>()?.Key?.DatabaseType != DatabaseTypes.Codes.Production);
				}
				return isNonTransactionalPurgeAllowed.Value;
			}
		}
		bool? isNonTransactionalPurgeAllowed;

		public void BackupAndPurgeCompanySpecific()
		{
			ValidateCompanySpecificPk();

			if (!CompanySpecificPkInfo.HasErrors())
			{
				BackupAndPurgeCatchingExceptions(isCompanySpecificPurge: true);
			}
		}

		void BackupAndPurgeCatchingExceptions(bool isCompanySpecificPurge = false, bool isTransactional = true)
		{
			Output = "";
			string astrixSymbols = "***";
			string threeDots = "...";
			AppendOutputLine(Res.GetString("fc4d5146-b081-4009-8f86-7295820b22e5", "{0} Data Purge - Start {0}", astrixSymbols) + "\r\n");

			using (((IDbUpgradeSupport)Db.Instance).ElevateToAdminConnectionForUpgrade())
			using (DbEnv.Instance.DisableTimerDuringDbUpgrade())
			{
				try
				{
					if (Db.AdminConnection?.AcquireLockout(LockoutReason.Purge) == DbLockoutState.AquiredLockout)
					{
						KillOtherConnections();

						AppendOutputLine(Res.GetString("a67b305a-6174-48b8-852d-07f022507879", "Performing database backup..."));
						AppendOutputLine(DoBackup(Db.AdminConnection));

						AppendOutputLine(
							Res.GetString("7874def7-9469-482b-b6ba-9414d430c665", "Purging Data")
							+ (isCompanySpecificPurge ? " (" + Res.GetString("097C61B6-B6D3-4973-B7C2-06FC23F5B8C2", "Company Specific") + ")" : "")
							+ threeDots
						);

						if (!isTransactional)
						{
							AppendOutputLine("* " + Res.GetString("5913B074-9A6B-417A-B32B-B59E6ED95EFB", "Purging in multiple database transactions - Please restore from the backup in case of error") + " *\r\n");
						}

						RunScript(isCompanySpecificPurge, isTransactional);

						AppendOutputLine("\r\n" + Res.GetString("944e918b-ba0d-4837-8281-fdafb9e17a9c", "{0} Data Purge - End   {0}", astrixSymbols));
					}
					else
					{
						var errorMessage = Res.GetString("5DF792BF-C902-4796-86BD-AFDD713A7ABD", "Failed to lock application access to the database [{0}].", Db.DatabaseName);
						AppendOutputLine("\r\n" + errorMessage);
					}
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					if (!isTransactional)
					{
						SetRunStatusFlag(Constants.PurgeDataRunStatusFlagCodes.Error);
					}
					string errorMessage = (e is SqlException || e is OdysseyDataException) ? e.Message : e.ToString();
					AppendOutputLine((NoResString)"\r\n\r\n" + (NoResString)"Failed to purge application data:");
					AppendOutputLine(errorMessage);
				}
				finally
				{
					Db.AdminConnection?.ResetLockout();
				}
			}
		}

		public void BackupCatchingExceptions()
		{
			Output = Res.GetString("a67b305a-6174-48b8-852d-07f022507879", "Performing database backup...");

			try
			{
				using (var adminConnection = Db.NewAdminConnection())
				{
					Output = DoBackup(adminConnection);
				}
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				string errorMessage = (e is SqlException || e is OdysseyDataException) ? e.Message : e.ToString();
				Output = errorMessage;
			}
		}

		void KillOtherConnections()
		{
			var writeLog = true;
			var timer = Stopwatch.StartNew();
			Action<string> feedBackAction = (message) =>
			{
				if (writeLog)
				{
					AppendOutputLine("\t\t" + message);
					timer.Restart();
				}

				writeLog = (timer.Elapsed >= TimeSpan.FromMinutes(1));
			};

			if (!DbConnectionKiller.KillOtherConnections(Db.AdminConnection, Db.AdminConnection.CurrentDatabase, feedBackAction))
			{
				AppendOutputLine(System.Environment.NewLine + System.Environment.NewLine + Res.GetString("433DB8CF-DE56-4294-9579-50FC66BA3D73", "Failed to kill other connection ..."));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		void AppendOutputLine(string lineToAppend)
		{
			Output += lineToAppend + System.Environment.NewLine;
			System.Windows.Forms.Application.DoEvents();
		}

		#endregion

		#region Implementation

		void RunScript(bool isCompanySpecificPurge, bool isTransactional)
		{
			var scripts = GetPurgeScripts(isCompanySpecificPurge);
			var commandTimeout = Env.Registry.PurgeDataTimeoutLimit * 60;

			if (isTransactional)
			{
				using (var manager = Db.Connection.BeginTransactionWithManager())
				{
					RunScriptCollectionDisablingTriggers(Db.Connection, scripts, commandTimeout);
					manager.CommitTransaction();
				}
			}
			else
			{
				SetRunStatusFlag(Constants.PurgeDataRunStatusFlagCodes.Running);
				RunScriptCollectionDisablingTriggers(Db.Connection, scripts, commandTimeout);
				SetRunStatusFlag(Constants.PurgeDataRunStatusFlagCodes.Completed);
			}
		}

		internal virtual void RunScriptCollectionDisablingTriggers(DbConnection connection, IEnumerable<string> scripts, int? cmdTimeoutInSeconds = null)
		{
			try
			{
				DisableTriggers();
				RunScriptCollection(connection, scripts, cmdTimeoutInSeconds);
			}
			finally
			{
				EnableTriggers();
			}
		}

		internal static void RunScriptCollection(DbConnection connection, IEnumerable<string> scripts, int? cmdTimeoutInSeconds = null)
		{
			foreach (var script in scripts)
			{
				using (var cmd = connection.Command(script, cmdTimeoutInSeconds))
				{
					cmd.ExecuteNonQuery();
				}
			}
		}

#if DEBUG
		internal virtual
#endif
		IEnumerable<string> GetPurgeScripts(bool isCompanySpecificPurge = false)
		{
			if (isCompanySpecificPurge)
			{
				yield return PurgeScriptRepositoryManager.GetPurgeCompanySpecificScript(CompanySpecificPk);
			}
			else
			{
				var systemScriptEnumerable = PurgeScriptRepositoryManager.GetSytemLevelPurgeScripts(
					((IDbConnected)Factory).Connection,
					HasRatingInfoPurgeScript,
					HasProductInfoPurgeScript,
					HasNonProxyOrganisationsPurgeScript,
					HasNonSystemChageCodesPurgeScript,
					HasTariffInfoPurgeScript,
					HasQuotationsPurgeScript);

				foreach (var script in systemScriptEnumerable)
				{
					yield return script;
				}
			}
		}

		string DoBackup(AdminConnection adminConnection)
		{
			string sqlText = String.Format((NoResString)"BACKUP DATABASE {0} TO DISK = '{1}' WITH INIT", Db.DatabaseName, BackupFilePath);

			try
			{
				adminConnection.ExecuteNonQuery(sqlText, 0);
			}
			catch (SqlException e)
			{
				if (new DbErrorHandler(e, adminConnection).ExceptionType == DbErrorType.CannotOpenBackupDeviceOrInvalidDeviceName)
				{
					string errorMessage = String.Format((NoResString)"Backup Failed. The backup path ({0}) is not valid on the database server or access is denied.", BackupFilePath);
					throw new OdysseyDataException(errorMessage);
				}
				else
				{
					throw;
				}
			}

			return Res.GetString("5698f8c3-cbf9-4c20-8d6a-152a33c92f72", "Database {0} has been successfully backed-up to [{1}]. This file path is relative to the database server.", Db.DatabaseName, BackupFilePath);
		}

		void DisableTriggers()
		{
			Utilities.RunResultingScriptsBasedOnSQLScriptGenerator((NoResString)"SELECT Command FROM dbo.vw_GetDisableTriggerScripts");
		}

		void EnableTriggers()
		{
			Utilities.RunResultingScriptsBasedOnSQLScriptGenerator((NoResString)"SELECT Command FROM dbo.vw_GetEnableTriggerScripts");
		}

		void SetRunStatusFlag(string status)
		{
			Env.Registry.PurgeDataRunStatusFlag = status;
		}

		#endregion
	}
}
