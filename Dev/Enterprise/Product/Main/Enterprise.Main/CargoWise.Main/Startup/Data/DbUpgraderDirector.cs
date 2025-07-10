using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Upgrades;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Res = CargoWise.Main.Res;

#if DEBUG
using CargoWise.BuildTools;
#endif

namespace Enterprise.Startup
{
	/// <summary>
	/// Summary description for DbUpgraderDirector.
	/// </summary>
	public class DbUpgraderDirector : AbstractApplicationStartupTask, IApplicationStartupTaskProgress
	{
		public static DbUpgraderDirector New(CommandLineArguments arguments)
		{
#if DEBUG
			if ((bool)arguments[ApplicationArguments.OptionTestAdapter])
			{
				return new DatDbUpgraderDirector();
			}

			if ((bool)arguments[ApplicationArguments.OptionConsoleUpgrader])
			{
				return new ConsoleDbUpgraderDirector((bool)arguments[ApplicationArguments.OptionKeepConsoleOpenOnError]);
			}
#endif

			if ((bool)arguments[ApplicationArguments.OptionScheduledDbUpgrader])
			{
				return new ScheduledUpgraderDirector(
					(bool)arguments[ApplicationArguments.OptionNotifyOnSuccessfulUpgrade],
					(string)arguments[ApplicationArguments.NotificationGroupPK]);
			}

			return new DbUpgraderDirector();
		}

		public override string TaskDescription => Res.GetString("49FA86AE-7422-4b0c-BF29-F1CB0BC5B9EB", "Checking database version");

		protected override bool GetShouldExecute(CommandLineArguments arguments) => true;

		public override int FailureExitCode => ExitCodes.DbUpgraderDirectorFailure;

		protected override bool DoExecute(CommandLineArguments arguments)
		{
			ValidationResponse result;

			using (Db.DisableSchemaVersionCheck())
			{
				if (!CheckSoftwareUpgrade((string)arguments[ApplicationArguments.OptionUpgrade]))
				{
					return false;
				}

				if (softwareUpgrade != null && !IsAnyDbUpgradeRequired)
				{
					result = CommitSoftwareUpgradeOnly();
				}
				else
				{
					result = CheckAndUpgradeDb();
					RowFactory.ResetCacheAfterDbUpgrade();
				}

#if DEBUG // Don't auto register debug builds, nor send a version report
				if (Progress != null)
				{
					// Avoid compiler warning about Progress not used.
				}
#else
				if (result.Successful && IsAnyDbUpgradeRequired)
				{
					if (Progress != null)
					{
						Progress(Res.GetString("6F17AACF-80C8-4FE1-AD11-CD91EAC35183", "Updating product registration..."), 10);
					}

					ObjectFactory.Get<Enterprise.Integration.Licensing.IProductRegistration>().TryAutoRegisterAfterUpgrade();
				}

				if (result.Successful && (softwareUpgrade != null || IsAnyDbUpgradeRequired))
				{
					SendVersionReport();
				}
#endif

				if (softwareUpgrade != null && result.Successful)
				{
					ShowSoftwareUpgradeSuccessMessage(softwareUpgrade.Version, result.Information);
				}
			}

#if DEBUG
			if (Globals.IsDBUpgSkipped)
			{
				Db.DisableSchemaVersionCheckPermanently();
			}
#endif
			return result.Successful;
		}

		internal void SendVersionReport()
		{
			try
			{
				var rego = ObjectFactory.Get<Integration.Licensing.IProductRegistration>();
				if (rego.LocalVerify() == Integration.Licensing.ProductRegistrationVerifyResult.OK)
				{
					IDisposable tempEnv = null;
					if (Env.CurrentBranch == null)
					{
						var branch = GlbBranch.GetFirstActiveBranch();
						if (branch != null)
						{
							tempEnv = DisposableEnvironment.ForBranch(branch.PK.ToGuid());
						}
					}

					using (tempEnv)
					{
						if (Env.CurrentBranch != null)
						{
							ObjectFactory.Get<Integration.IVersionUpgradeReportSender>().Send();
						}
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportOnce("SendVersionReport", ex);
			}
		}

		public bool CheckSoftwareUpgrade(string softwareUpgradePk)
		{
			if (softwareUpgradePk != null)
			{
				Guid softwareUpgradePkGuid;
				try
				{
					softwareUpgradePkGuid = new Guid(softwareUpgradePk);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					ShowDenyUpgradeMessage((NoResString)"An invalid upgrade command line option given.", (NoResString)"Software Upgrade Failure", null);
					return false;
				}
				var upgrade = UpgradeManagerFactory.NewUpgradeManager().QueryVersion(softwareUpgradePkGuid);
				if (upgrade == null)
				{
					ShowDenyUpgradeMessage((NoResString)"The requrested upgrade package cannot be found.", (NoResString)"Software Upgrade Failure", null);
					return false;
				}
				if (new VersionNumber(upgrade.Version) != ReleaseInfo.Instance.VersionNumber)
				{
					ShowDenyUpgradeMessage((NoResString)"You are attempting to upgrade to software version " + upgrade.Version + (NoResString)" with software version " + ReleaseInfo.Instance.VersionNumber, (NoResString)"Software Upgrade Failure", null);
					return false;
				}
				softwareUpgrade = upgrade;
			}
			return true;
		}

		protected UpgradeInfo softwareUpgrade;

		public ValidationResponse CommitSoftwareUpgradeOnly()
		{
			return Upgrade();
		}

		/// <summary>
		/// This method should reference nothing but constants in the DbUpgrader solution
		/// to avoid loading the Enterprise.DbUpgrader.dll unless a DB upgrade is required.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "database related")]
		public ValidationResponse CheckAndUpgradeDb()
		{
			var result = new ValidationResponse();
			var terminationMessage = "The application will be terminated.";
#if DEBUG
			if ((IsAnyDbUpgradeRequired || ((DbUpgraderVersionInfo)UpgradeVersionInfo).IsMajorVersionDowngradeAttempt) &&
					!Globals.IsTest)
			{
				if
				(
					BuildConstants.LocalSourcePathAvailable
					&&
					SkipDBChecker.IsDbInFile(Db.ServerName, Db.DatabaseName)
					&&
					Globals.Message.Show(
						string.Format("Do you wish to skip the database ({0} {1}) upgrade?", Db.ServerName, Db.DatabaseName),
						"Database Upgrade",
						MessageBoxButtons.YesNo,
						MessageBoxIcon.Question) == DialogResult.Yes
				)
				{
					Globals.IsDBUpgSkipped = true;
					result.Successful = true;
					return result;
				}
			}
#endif

			var isMajorDowngrade = ((DbUpgraderVersionInfo)UpgradeVersionInfo).IsMajorVersionDowngradeAttempt;
			var isCurrentVersionTooOld = ((DbUpgraderVersionInfo)UpgradeVersionInfo).IsCurrentMajorVersionTooOld;
			var isCurrentTransformVersionTooOld = ((DbUpgraderVersionInfo)UpgradeVersionInfo).IsCurrentTransformVersionTooOld;

			string denyReason = default;
			string denyReasonFullExplanation = default;

			if (isMajorDowngrade)
			{
				denyReason = denyReasonFullExplanation = (NoResString)"Application version older than DB version";
			}
			if (isCurrentVersionTooOld)
			{
				denyReason = (NoResString)"Current database schema version is too old";
				denyReasonFullExplanation = string.Format((NoResString)"Current database schema version is too old (< {0}.0). A two-stage upgrade is required. You must upgrade using the General Product release starting with {1}", DataRegistry.MinUpgradableDbMajorSchemaVersion.ToString(), DataRegistry.GeneralProductReleaseContainingUpgradeScripts);
			}
			if (isCurrentTransformVersionTooOld)
			{
				denyReason = (NoResString)"Current database transformation version is too old";
				denyReasonFullExplanation = string.Format((NoResString)"Current database transformation version is too old (< {0}.0). A two-stage upgrade is required. You must upgrade using the General Product release starting with {1}", DataRegistry.MinDatabaseMajorTransformationVersion.ToString(), DataRegistry.GeneralProductReleaseContainingUpgradeScripts);
			}

			var validationResponse = TryRunningUpgrade(result, ref terminationMessage, ref denyReason, denyReasonFullExplanation);
			CloseForm(result);

			return validationResponse;
		}

		// extracted only to reduce the number of assemblies being loaded on the normal path
		ValidationResponse TryRunningUpgrade(ValidationResponse result, ref string terminationMessage, ref string denyReason, string denyReasonFullExplanation)
		{
			if (denyReason.IsNullOrEmpty() && IsDbUpgradeRequired())
			{
				var provider = ObjectFactory.Get<IOnlineTransformationProvider>();

#if DEBUG
				// Modify status if needed: add new and remove unavailable transforms
				provider.GetRunningTasks();
#endif

				var deletedPendingTasks = provider.DeletedPendingTasks.ToArray();
				if (deletedPendingTasks.Any())
				{
					denyReason = (NoResString)"Pending deleted transformations exist:\r\n";
					terminationMessage += "\r\n";
					terminationMessage += Res.GetString("RunODTServiceMessage", "The Online Data Transformation Service (ODT) Service Task must run to completion before the upgrade can be completed.");

					terminationMessage += (NoResString)"\r\nThe following online transformations have yet to complete:";

					foreach (var transform in deletedPendingTasks)
					{
						denyReason += "\r\n\t" + transform; // Include the deleted transformations in the title so we know what failed when we have a report.
						terminationMessage += "\r\n\t" + transform; // Include the deleted transformations in the description so we can see what's failed.
					}
				}
				else
				{
					var upgradeResult = Upgrade();
					result.Successful = upgradeResult.Successful;
					return upgradeResult;
				}
			}

			if (denyReason.IsNullOrEmpty())
			{
				result.Successful = true;
			}
			else
			{
				ShowDenyUpgradeMessage(terminationMessage, denyReason, denyReasonFullExplanation);
				result.Successful = false;
			}

			return result;
		}

#if DEBUG
		public
#endif
void CloseForm(ValidationResponse result)
		{
			if (result.Successful)
			{
				return;
			}

			foreach (var f in CargoWise.Windows.UI.ZApplication.GetOpenForms())
			{
				if (f.GetType().Name == "SplashForm")
				{
					f.Close();
					f.Dispose();
				}
			}
		}

		public
#if DEBUG
 virtual
#endif
 bool IsDbUpgradeRequired()
		{
			return IsAnyDbUpgradeRequired;
		}

		#region Implementation

		bool IsAnyDbUpgradeRequired
		{
			get
			{
				return
					UpgradeVersionInfo.IsRequired_Schema
					|| UpgradeVersionInfo.IsRequired_Script
					|| UpgradeVersionInfo.IsRequired_Data
					|| UpgradeVersionInfo.IsRequired_Transformation
					|| UpgradeVersionInfo.IsRequired_ClientDocuments;
			}
		}

		protected internal virtual void ShowDenyUpgradeMessage(string messageSufix, string denyReason, string denyReasonFullExplanation)
		{
			var message = new StringBuilder();
			if (!string.IsNullOrEmpty(denyReasonFullExplanation))
			{
				message.Append(denyReasonFullExplanation);
				message.AppendLine();
			}
			message.Append((NoResString)"Software Version: ");
			message.Append(ReleaseInfo.Instance.VersionNumber);
			message.AppendLine();
			message.AppendLine();
			message.Append((NoResString)"Server: ");
			message.Append(Db.ServerName);
			message.Append((NoResString)" - Database: ");
			message.Append(Db.DatabaseName);
			message.AppendLine();
			message.AppendLine();
			message.Append((NoResString)"Application Schema Version = ");
			message.Append(SchemaVersion.Application);
			message.AppendLine();
			message.Append((NoResString)"Database Schema Version = ");
			message.Append(UpgradeVersionInfo.DbReferenceVersion_Schema);
			message.AppendLine();
			message.AppendLine();
			message.Append(messageSufix);

			Globals.Message.ShowError(message.ToString(), denyReason);
		}

		protected virtual void ShowSoftwareUpgradeSuccessMessage(Version version, string information)
		{
			if (!string.IsNullOrEmpty(information))
			{
				Globals.Message.ShowInformation(Res.GetString("64042680-02D4-4AC5-8AB2-740EEAF4D87B", "Upgrade to version {0} completed successfully. However, an exception occurred during upgrade conclusion steps, and some post upgrade cleanup may not have finished: {1}", version, information));
			}
			else
			{
				Globals.Message.ShowInformation(Res.GetString("669ba1f3-c2e4-4c31-aced-ba9fb7a88099", "Upgrade to version {0} completed successfully.", version));
			}
		}

		protected
#if DEBUG
 virtual
#endif
 ValidationResponse Upgrade()
		{
			var result = new ValidationResponse();
			if (LoginForUpgrade())
			{
				result = DoUpgrade();
			}
			else
			{
				result.Successful = false;
			}
			return result;
		}

		protected virtual ValidationResponse DoUpgrade()
		{
			return ObjectFactory.Get<IDbUpgraderRunner>().FullUpgrade(UpgradeVersionInfo, softwareUpgrade, AppendToLogToFile, RunUpgradeGUI);
		}

		internal static Func<BaseUpgradeManager, bool> RunUpgradeGUI => (upgradeManager) =>
		{
			using (var upgraderForm = new UpgForm(upgradeManager))
			{
				upgraderForm.ShowDialog();
				return upgraderForm.UpgradeSucceeded;
			}
		};

		#region Login For Upgrade

		protected virtual bool LoginForUpgrade()
		{
			if (IsEmptyDatabaseUpgrade())
			{
				return true;
			}

			var versionChangeText = GetVersionChangeText();
#if DEBUG
			if (softwareUpgrade == null)
			{
				return GetDeveloperConfirmation(versionChangeText);
			}
			else
#endif
			{
				return ShowUpgradeLoginForm(versionChangeText);
			}
		}

		bool IsEmptyDatabaseUpgrade()
		{
			return
				UpgradeVersionInfo.DbReferenceVersion_Schema.CompareTo(new VersionLabel(0, 0)) == 0 &&
				UpgradeVersionInfo.DbReferenceVersion_Script.CompareTo(new VersionLabel(0, 0)) == 0 &&
				UpgradeVersionInfo.DbReferenceVersion_Transformation.CompareTo(new VersionLabel(0, 0)) == 0 &&
				UpgradeVersionInfo.DbReferenceVersion_Data.CompareTo(new VersionLabel(0, 0)) == 0;
		}

		bool ShowUpgradeLoginForm(string versionChangeText)
		{
			var loginForm = new LoginForUpgradeForm(versionChangeText, IsDocManagerSchemaUpgradeIncluded, Db.DatabaseName);
			return ZFormModaliser.ShowDialogAndDispose(loginForm) == DialogResult.OK;
		}

#if DEBUG
		bool GetDeveloperConfirmation(string versionChangeText)
		{
			MessageBoxIcon icon;
			string caption;
			var message = "The Database Schema and/or System Data must be updated. Otherwise the application will be terminated." +
				System.Environment.NewLine + System.Environment.NewLine;
			string messageFooter;
			if (IsDownGrade)
			{
				icon = MessageBoxIcon.Warning;
				caption = "Downgrade Schema / System Data";
				messageFooter = "Do you want to downgrade your database?" + System.Environment.NewLine;
			}
			else
			{
				icon = MessageBoxIcon.Information;
				caption = "Upgrade Schema / System Data";
				messageFooter = "Do you want to upgrade your database?" + System.Environment.NewLine;
			}

			if (IsDocManagerSchemaUpgradeIncluded)
			{
				message += String.Format(
					"*** PLEASE NOTE ***\r\nUpgrading to this version will take longer than usual. " +
					"It will apply changes to all eDocs databases ({0}_SDxxx) in addition to the main one ({0}).\r\n\r\n",
					Db.DatabaseName);
			}

			message += "Server: " + Db.ServerName + System.Environment.NewLine +
				"Database: " + Db.DatabaseName + System.Environment.NewLine +
				versionChangeText;

			if (Db.ServerName.Contains(".db.") || Db.ServerName.Contains("SSQL"))
			{
				message += System.Environment.NewLine + System.Environment.NewLine +
					"*********************************************************************************************" +
					System.Environment.NewLine +
					"   THIS IS A SHARED DATABASE" +
					System.Environment.NewLine +
					"   YOU MUST NOT UPGRADE IT WITHOUT DEPLOYING A NEW RELEASE FOR IT" +
					System.Environment.NewLine + System.Environment.NewLine +
					"*********************************************************************************************";
			}
			message += System.Environment.NewLine + System.Environment.NewLine + messageFooter;

			return (Globals.Message.Show(message, caption, MessageBoxButtons.OKCancel, icon) == DialogResult.OK);
		}

		bool IsDownGrade
		{
			get
			{
				return
					SchemaVersion.Application.CompareTo(UpgradeVersionInfo.DbReferenceVersion_Schema) < 0
					|| ScriptVersion.Application.CompareTo(UpgradeVersionInfo.DbReferenceVersion_Script) < 0
					|| DataVersion.Application.CompareTo(UpgradeVersionInfo.DbReferenceVersion_Data) < 0
					|| TransformationVersion.ApplicationNumber.CompareTo(UpgradeVersionInfo.DbReferenceVersion_Transformation) < 0;
			}
		}

#endif

		#region SuppressResourceStringsCheckRegion

		internal string GetVersionChangeText()
		{
			var versionChangeText_Release = "Release: " + new EnterpriseInformationRetriever().Release;
			var versionChangeText_Schema = "Schema Version: " + GetSpecificVersionChangeText(UpgradeVersionInfo.DbReferenceVersion_Schema, SchemaVersion.Application, UpgradeVersionInfo.IsRequired_Schema);
			var versionChangeText_Script = "Script Version: " + GetSpecificVersionChangeText(UpgradeVersionInfo.DbReferenceVersion_Script, ScriptVersion.Application, UpgradeVersionInfo.IsRequired_Script);
			var versionChangeText_Data = "Data Version: " + GetSpecificVersionChangeText(UpgradeVersionInfo.DbReferenceVersion_Data, DataVersion.Application, UpgradeVersionInfo.IsRequired_Data);
			var versionChangeText_Transformation = "Transformation Version: " + GetSpecificVersionChangeText(UpgradeVersionInfo.DbReferenceVersion_Transformation, TransformationVersion.ApplicationNumber, UpgradeVersionInfo.IsRequired_Transformation);

			return String.Format(
				"{0}\r\n\r\n{1}\r\n{2}\r\n{3}\r\n{4}",
				versionChangeText_Release,
				versionChangeText_Schema,
				versionChangeText_Script,
				versionChangeText_Data,
				versionChangeText_Transformation);
		}

		string GetSpecificVersionChangeText(object versionBeforeUpgrade, object appVersion, bool isUpgradeRequired)
		{
			var result = String.Format("{0} => {1}", versionBeforeUpgrade.ToString(), appVersion.ToString());

			if (!isUpgradeRequired)
			{
				result += "  (no update required)";
			}

			return result;
		}

		#endregion

		internal bool IsDocManagerSchemaUpgradeIncluded
		{
			get { return SchemaVersion.DocManager.IsBetweenOrEqualToTopVersion(UpgradeVersionInfo.DbReferenceVersion_Schema, SchemaVersion.Application); }
		}

		#endregion

		#region SkipDBChecker
#if DEBUG

		DbFileChecker SkipDBChecker
		{
			get
			{
				if (fSkipDBChecker == null)
				{
					fSkipDBChecker = new DbFileChecker(Path.Combine(BuildConstants.LocalEnterprisePath, @"DBUPG_SkipUpgradeDbs.txt"));
				}
				return fSkipDBChecker;
			}
		}

		DbFileChecker fSkipDBChecker;

#endif
		#endregion

		#region Logging

		protected virtual void AppendToLogToFile(UpgradeEventType eventType, string text)
		{
			using (var outputFileWriter = new StreamWriter(UpgradeLogFilePath, true))
			{
				if (eventType == UpgradeEventType.TaskFailed)
				{
					outputFileWriter.Write(System.Environment.NewLine);
				}
				outputFileWriter.Write(LogTimestamp);
				outputFileWriter.Write('\t');
				if (eventType == UpgradeEventType.SubtaskStarted)
				{
					outputFileWriter.Write('\t');
				}
				outputFileWriter.Write(text);
				outputFileWriter.Write(System.Environment.NewLine);
				if (eventType == UpgradeEventType.TaskFailed)
				{
					outputFileWriter.Write(System.Environment.NewLine);
				}
			}
		}

		internal string UpgradeLogFilePath
		{
			get
			{
				if (upgradeLogFilePath == null)
				{
					var currentDateTime = Globals.IsTest ? CargoWise.Types.ZDateTime.Now.ToDateTime() : DateTime.Now;
					var currentDateTimeString = currentDateTime.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
					var counter = 0;
					var fileNameTemplate = Path.Combine(UpgradeLogDirectory, "DBUPG_" + currentDateTimeString + "{0}.LOG");
					upgradeLogFilePath = null;
					do
					{
						var suffix = counter > 0 ? " (" + counter.ToString(CultureInfo.InvariantCulture) + ")" : string.Empty;
						var fileName = string.Format(CultureInfo.InvariantCulture, fileNameTemplate, suffix);

						try
						{
							using (new FileStream(fileName, FileMode.CreateNew))
							{ }
							upgradeLogFilePath = fileName;
						}
						catch (IOException)
						{
							if (!File.Exists(fileName))
							{
								throw; // There is no such file but it cannot be created - rethrow exception further
							}
							counter++;
						}
					} while (string.IsNullOrEmpty(upgradeLogFilePath));
				}

				return upgradeLogFilePath;
			}
		}
		string upgradeLogFilePath;

		protected virtual string UpgradeLogDirectory
		{
			get
			{
				var upgradeLogDirectory = CommonProgramData.GetCargoWiseDirectory((NoResString)"Upgrade Logs", Db.ServerName, Db.DatabaseName);
				Directory.CreateDirectory(upgradeLogDirectory);
				return upgradeLogDirectory;
			}
		}

		protected string LogTimestamp
		{
			get { return DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss]"); }
		}

		#endregion

		protected IVersionChangeInfo UpgradeVersionInfo
		{
			get
			{
				if (upgradeVersionInfo == null)
				{
					upgradeVersionInfo = new DbUpgraderVersionInfo(softwareUpgrade);
				}

				return upgradeVersionInfo;
			}
		}
		IVersionChangeInfo upgradeVersionInfo;

		#endregion

		public event ZArchitecture.Core.Progress Progress;
	}
}
