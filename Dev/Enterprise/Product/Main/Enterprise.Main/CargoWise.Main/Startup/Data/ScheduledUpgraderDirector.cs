using System;
using System.Collections.Specialized;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.DbUpgrader.Shared;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Res = CargoWise.Main.Res;

namespace Enterprise.Startup
{
	class ScheduledUpgraderDirector : DbUpgraderDirector
	{
		public ScheduledUpgraderDirector(bool notifyOnSuccess, string notificationGroupPK)
			: this(notifyOnSuccess, notificationGroupPK, () => StartupNotification.ScheduledUpgradeLogger)
		{
		}

		internal ScheduledUpgraderDirector(bool notifyOnSuccess, string notificationGroupPK, Func<ILogger> loggerFunc)
		{
			this.notifyOnSuccess = notifyOnSuccess;
			this.notificationGroupPK = Guid.Empty;
			logger = new Lazy<ILogger>(loggerFunc);

			if (!string.IsNullOrWhiteSpace(notificationGroupPK))
			{
				try
				{
					this.notificationGroupPK = new Guid(notificationGroupPK);
				}
				catch (ArgumentException) { }
				catch (FormatException) { }
			}
		}

		internal const string ServiceManagerRunnerName = "ServiceManager.Runner";

		readonly bool notifyOnSuccess;
		readonly Guid notificationGroupPK;
		readonly Lazy<ILogger> logger;

		ILogger Logger => logger.Value;

		protected override ValidationResponse DoUpgrade()
		{
			return ObjectFactory.Get<IDbUpgraderRunner>().FullSilentUpgrade(UpgradeVersionInfo, softwareUpgrade, AppendToLogToFile);
		}

		protected override void AppendToLogToFile(UpgradeEventType eventType, string text)
		{
			if (eventType == UpgradeEventType.SubtaskStarted)
			{
				text = " - " + text;
			}
			Logger.Log(eventType == UpgradeEventType.TaskFailed ? LogType.Error : LogType.Information, text);
		}

		protected internal override void ShowDenyUpgradeMessage(string messageSuffix, string denyReason, string denyReasonFullExplanation)
		{
			var error = (denyReasonFullExplanation ?? denyReason) + " - " + messageSuffix;
			Logger.Log(LogType.Warning, error);
		}

		protected override void ShowSoftwareUpgradeSuccessMessage(Version version, string information)
		{
			string success = string.Empty;
			if (!string.IsNullOrEmpty(information))
			{
				success = string.Format(CultureInfo.InvariantCulture, (NoResString)"Upgrade to version {0} completed successfully. However, an exception occurred during upgrade conclusion steps, and some post upgrade cleanup may not have finished: {1}", version, information);
			}
			else
			{
				success = string.Format(CultureInfo.InvariantCulture, (NoResString)"Upgrade to version {0} completed successfully.", version);
			}
			Logger.Log(LogType.Information, success);
			HandleSuccessNotificationEmail();
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Controller messages should not be localized")]
		internal void HandleSuccessNotificationEmail()
		{
			if (!notifyOnSuccess)
			{
				return;
			}

			try
			{
				string version = softwareUpgrade?.Version.ToString() ?? (NoResString)"(none)";
				EmailDef email = new EmailDef();
				StringCollection recipients = null;

				email.Subject = Res.GetString("6838a733-f576-40fb-9b7b-5cd445fdfd32", "Upgrade to version {0} success notification", version);
				email.Body = Res.GetString("5cd75efb-30b2-495b-ab1a-4ca1ab551f5d", "Upgrade to version {0} finished successfully.\r\n\tDB Server name:\t{1}\r\n\tDatabase name:\t{2}\r\n\tUpgrade time:\t{3}", version, Db.ServerName, Db.DatabaseName, ZDateTime.Now);

				if (notificationGroupPK != Guid.Empty)
				{
					try
					{
						recipients = new EmailGroupUtility().GetGroupEmailCollection(notificationGroupPK, true);
						email.AddRecipientForSystemCommunication(recipients.OfType<string>().ToArray(), RecipientDef.RecipientTypes.TO);
					}
					catch (EmailHasNoRecipientsException e)
					{
						var code = GetGroupCodeWithoutFactory(notificationGroupPK);
						Logger.Log(LogType.Warning, FormattableString.Invariant($"Failure to send upgrade email: Could not find a recipient for group (PK = {notificationGroupPK}, Code = {code ?? "(Unknown)"}). Reason: {e.Message}"));
						return;
					}
				}

				Env.Instance.OutgoingMailManager.CreateAndSave(email);
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				Logger.Log(LogType.Warning, "Failure while sending upgrade email: " + e);
			}

			[SuppressMessage("CargoWiseOne", "CW1107:Use the BusinessObjectFactory rather than hitting the DB directly.")]
			string GetGroupCodeWithoutFactory(Guid groupPK)
			{
				using (Db.DisposableActionForDbConnection())
				using (var cmd = Db.Connection.Command($@"SELECT {GlbGroupSchema.Constants.GG_Code} FROM {GlbGroupSchema.Constants.SqlSchemaName}.{GlbGroupSchema.Constants.TableName} WHERE {GlbGroupSchema.Constants.PK} = @groupPK"))
				{
					cmd.AddParameter("@groupPK", SqlDbType.UniqueIdentifier, groupPK);
					var result = cmd.ExecuteScalar();
					return (string)result;
				}
			}
		}

		// This provides some basic protection against a accidentally running the upgrade outside of the normal scheduled process.
		// It is passed -ServiceProcess:ENCRYPTED_PID command line arg, where ENCRYPTED_PID is an encrypted process ID.
		// The process ID's name is expected to contain "ServiceManager.Runner", as a check that it is being launched in the expected way by
		// a service task runner.
		// This is not intended as a security measure.
		protected override bool LoginForUpgrade()
		{
			string serviceProcessValue = (string)ApplicationArguments.UsedToLaunchApplication[ApplicationArguments.OptionServiceProcess];
			if (serviceProcessValue != null)
			{
				serviceProcessValue = Encoding.UTF8.GetString(ProtectedData.Unprotect(Convert.FromBase64String(serviceProcessValue), null, DataProtectionScope.LocalMachine));
				int serviceProcessId;
				if (int.TryParse(serviceProcessValue, out serviceProcessId))
				{
					try
					{
						Process serviceProcess = Process.GetProcessById(serviceProcessId);
						if (serviceProcess != null &&
							serviceProcess.ProcessName.Contains(ServiceManagerRunnerName, StringComparison.OrdinalIgnoreCase))
						{
							return true;
						}
					}
					catch (ArgumentException)
					{ }
				}
			}
			else if (!EnvProxy.Instance.IsProductionSystem)
			{
				return true;
			}

			Logger.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "Validation of parent service manager process failed. Stack trace: {0}", new StackTrace().ToString()));
			return false;
		}
	}
}
