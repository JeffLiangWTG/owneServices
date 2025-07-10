using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Definitions;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using ServiceManager.Integration.ServiceTasks.CW;
using LogType = Enterprise.Integration.LogType;

namespace Enterprise.Startup
{
	public static class StartupNotification
	{
		enum StartupType
		{
			Normal = 0,
			ScheduledUpgrade = 1,
#if DEBUG
			DAT = 2,
#endif
		}

		public static DialogResult Show(string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
		{
			return Show(message, caption, buttons, icon, null);
		}

		public static DialogResult Show(string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, Action customGuiMessageAction)
		{
			DialogResult result = DialogResult.Abort;
			switch (startupType)
			{
				case StartupType.ScheduledUpgrade:
					LogType logType;
					switch (icon)
					{
						case MessageBoxIcon.Error:
							logType = LogType.Error;
							break;
						case MessageBoxIcon.Warning:
							logType = LogType.Warning;
							break;
						default:
							logType = LogType.Information;
							break;
					}
					ScheduledUpgradeLogger.Log(logType, caption + " - " + message);
					break;
#if DEBUG
				case StartupType.DAT:
					if (Globals.IsTest)
					{
						result = ShowWithGlobals(message, caption, buttons, icon);
					}
					else
					{
						throw new InvalidOperationException("StartupNotification.Show: " + icon + ": " + caption + " - " + message);
					}
					break;
#endif
				default:
					if (customGuiMessageAction != null)
					{
						customGuiMessageAction();
					}
					else if (isSafeToUseGlobals || Globals.IsTest)
					{
						result = ShowWithGlobals(message, caption, buttons, icon);
					}
					else
					{
						if (System.Environment.UserInteractive)
						{
							result = MessageBox.Show(message, caption, buttons, icon);
						}
					}
					break;
			}

			return result;
		}

		static DialogResult ShowWithGlobals(string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
		{
			DialogResult result = DialogResult.None;
			if (icon == MessageBoxIcon.Error && buttons == MessageBoxButtons.OK)
			{
				Globals.Message.ShowError(message, caption);
			}
			else
			{
				result = Globals.Message.Show(message, caption, buttons, icon);
			}
			return result;
		}

		public static void ShowError(string message, string caption)
		{
			if (Globals.IsConsoleSession)
			{
				Console.WriteLine(message);
			}
			else
			{
				ShowError(message, caption, null);
			}
		}

		public static void ShowError(string message, string caption, Action customGuiMessageAction)
		{
			Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Error, customGuiMessageAction);
		}

		public static bool ShowConfirmation(string message, string caption, string confirmationString)
		{
			var result = Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Information, null);
			return result == DialogResult.Yes;
		}

		public static bool ShowConfirmation(string message, string caption, string confirmationString, MessageBoxIcon icon)
		{
			switch (startupType)
			{
				case StartupType.ScheduledUpgrade:
					ScheduledUpgradeLogger.Log(LogType.Warning,
						"User confirmation required: " + caption + " - " + message);
					return false;
#if DEBUG
				case StartupType.DAT:
					if (Globals.IsTest)
					{
						return Globals.Message.ShowConfirmation(message, caption, confirmationString, icon) == DialogResult.OK;
					}
					else
					{
						throw new InvalidOperationException("StartupNotification.ShowConfirmation: " + caption + " - " + message);
					}
#endif
				default:
					if (isSafeToUseGlobals)
					{
						return Globals.Message.ShowConfirmation(message, caption, confirmationString, icon) == DialogResult.OK;
					}
					else
					{
						return MessageBox.Show(message, caption, MessageBoxButtons.YesNo, icon) == DialogResult.Yes;
					}
			}
		}

		public static ILogger ScheduledUpgradeLogger
		{
			get { return scheduledUpgradeLogger ?? (scheduledUpgradeLogger = ObjectFactory.Get<ILoggerFactory>().NewScheduledUpgradeLogger(Db.ServerName, Db.DatabaseName)); }
		}

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		static ILogger scheduledUpgradeLogger;

		public static void SafeToUseGlobals()
		{
			isSafeToUseGlobals = true;
		}

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		static StartupType startupType;

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		static bool isSafeToUseGlobals;

		public class InitializationTask : InitializingApplicationStartupTask
		{
			public override string TaskDescription => (NoResString)"Initialization task";

			public override int FailureExitCode => ExitCodes.InitializationTaskError;

			protected override bool DoExecute(CommandLineArguments arguments)
			{
				if ((bool)arguments[ApplicationArguments.OptionScheduledDbUpgrader])
				{
					startupType = StartupType.ScheduledUpgrade;
					Globals.IsUserInteractive = false;
				}

#if DEBUG
				if ((bool)arguments[ApplicationArguments.OptionConsoleUpgrader])
				{
					Globals.IsConsoleSession = true;
				}

				if ((bool)arguments[ApplicationArguments.OptionTestAdapter])
				{
					startupType = StartupType.DAT;
				}
#endif
				return true;
			}
		}
	}
}
