using System;
using System.Collections;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Startup
{
	public class ApplicationArguments : CommandLineArguments
	{
		#region SuppressResourceStringsCheckRegion

		public const string OptionRunWithoutLoader = "-IAmDoingTheWrongThingByRunningEnterpriseWithoutLoader";
		public const string OptionSkipVersionCheck = "-SkipVersionCheck";
		public const string OptionForceCW1HomeScreen = ZArchitecture.Business.CWNextFeatureHelper.OptionForceCW1HomeScreen;

		public const string OptionRandomiseTesting = "-RandomiseTesting";
		public const string OptionUsePreviousTestOrder = "-UsePreviousTestOrder";
		public const string OptionRunDbCollationTest = "-RunDbCollationTest";
		public const string OptionScanStart = "-scanstart";
		public const string OptionShowLogin = "-ShowLogin";
		public const string OptionHideUserInterface = "-HideUserInterface";

		public const string OptionBranch = "-Branch:";
		public const string OptionDepartment = "-Department:";
		public const string OptionMoreArgs = "-MoreArgs:";
		public const string OptionDatEnterprisePath = "-DatEnterprisePath:";
		public const string OptionServerDirectoryPath = "-SDir:";
		public const string OptionShowInSystemTray = "-ShowInSystemTray";

		[Obsolete("Never passed by cw1.start")]
		public const string OptionInstance = "-Instance:";

		public const string OptionScheduledDbUpgrader = "-ScheduledDbUpgrader";
		public const string OptionNotifyOnSuccessfulUpgrade = "-NotifyOnSuccessfulUpgrade";
		public const string OptionNotifyOnFailedUpgrade = "-NotifyOnFailedUpgrade";
		public const string NotificationGroupPK = "-NotificationGroupPK:";
		public const string OptionServiceProcess = "-ServiceProcess:";
		public const string OptionUpgrade = "-Upgrade:";
		public const string OptionWebDeploy = "-WebDeploy";
		public const string OptionClient = "-Client:";

		[Obsolete("Could be passed from legacy version by particular upgrade paths")]
		public const string OptionUpdate = "-Update";

		[Obsolete("Doesn't do anything")]
		public const string OptionSkipWebDeployPreCheck = "-SkipWebDeployPreCheck";

		[Obsolete("Never passed by cw1.start")]
		public const string OptionDistributionFilesUpdated = "-DistributionFilesUpdated";

#if DEBUG
		public const string OptionTestAdapter = "-TestAdapter";
		public const string OptionDat = "-DAT";
		public const string OptionTestOnly = "-NUnit:";
		public const string OptionLabelCaptionLogger = "-LabelCaptionLogger:";
		public const string OpenRecordBaseline = "-RecordBaseline";
		public const string OptionConsoleUpgrader = "-ConsoleDbUpgrader";
		public const string OptionKeepConsoleOpenOnError = "-KeepConsoleOpenOnError";
		public const string OptionNoSplash = "-NoSplash";
		public const string OptionSlowBackgroundApplicationStartupTask = "-SlowBackgroundApplicationStartupTask";
		public const string OptionStartBlazorWinFormsInterop = "-StartBlazorWinFormsInterop";
#endif

		public const string OptionStiDevice = "-StiDevice:";
		public const string OptionStiEvent = "-StiEvent:";
		public const string OptionOpenDocDiag = "-OpenDocDiag";
		public const string OpenDocMaintain = "-OpenDocMaintain:";

		public const string OptionGlowServer = "-GlowServer:";

		public const string OptionVerboseLoginArgument = "-VerboseLoginFilename:";

		public const string OptionPersist = ProgramRestarter.OptionPersist;
		public const string OptionSkipDotNetVersionSwitch = "-SkipDotNetVersionSwitch";

		public const string OptionPW = "-PW";
		public const string OptionReconnect = "-Reconnect";
		public const string OptionDbArgsSignature = "-DbArgsSignature:";

		#endregion

		static Hashtable PossibleOptions
		{
			get
			{
				Hashtable possibleOptions = new Hashtable
				{
#if DEBUG
					{ OptionTestAdapter, false },
					{ OptionDat, false },
					{ OptionTestOnly, null },
					{ OptionLabelCaptionLogger, null },
					{ OpenRecordBaseline, false },
					{ OptionConsoleUpgrader, false },
					{ OptionKeepConsoleOpenOnError, false },
					{ OptionNoSplash, false },
					{ OptionSlowBackgroundApplicationStartupTask, false },
					{ OptionStartBlazorWinFormsInterop, false },
#endif
					{ OptionRandomiseTesting, false },
					{ OptionUsePreviousTestOrder, false },
					{ OptionRunDbCollationTest, false },
					{ OptionScheduledDbUpgrader, false },
					{ OptionServiceProcess, null },
					{ OptionUpgrade, null },
					{ OptionNotifyOnSuccessfulUpgrade, false },
					{ OptionNotifyOnFailedUpgrade, false },
					{ NotificationGroupPK, null },
					{ OptionRunWithoutLoader, false },
					{ OptionSkipVersionCheck, false },
					{ OptionShowLogin, false },
					{ OptionBranch, false },
					{ OptionDepartment, false },
					{ OptionMoreArgs, false },
					{ OptionDatEnterprisePath, null },
					{ OptionServerDirectoryPath, "" },
					{ OptionShowInSystemTray, false },
					{ OptionWebDeploy, false },
					{ OptionHideUserInterface, false },
					{ OptionSkipDotNetVersionSwitch, false },
					{ OptionForceCW1HomeScreen, false },

#pragma warning disable 0618   // options are obsolete, but keep as a possible options to allow enterprise to run if the option is passed by a legacy process
					{ OptionUpdate, false },
					{ OptionSkipWebDeployPreCheck, false },
					{ OptionDistributionFilesUpdated, false },
					{ OptionInstance, null },
#pragma warning restore 0618

#if DEBUG
					{ OptionClient, null },
#endif

					// DocManager command-line options
					{ OptionScanStart, false },
					{ OptionStiDevice, false },
					{ OptionStiEvent, false },
					{ OptionOpenDocDiag, false },
					{ OpenDocMaintain, false },
					{ OptionGlowServer, null },
					{ OptionVerboseLoginArgument, null },
					{ OptionPersist, null },
					{ OptionPW, false },
					{ OptionReconnect, false },
					{ OptionDbArgsSignature, null }
				};
				return possibleOptions;
			}
		}

		public ApplicationArguments(string argumentFilePath)
			: base(argumentFilePath, PossibleOptions)
		{
		}

		public ApplicationArguments(string[] args)
			: base(args, PossibleOptions)
		{
		}
	}
}
