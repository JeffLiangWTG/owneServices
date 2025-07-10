using System;
using System.Globalization;
using Enterprise.Builder.Generator;

namespace Enterprise.Builder.GenerateDbUpgraderResources
{
	sealed class CommandLineArgs
	{
		public const string Merge = "-MERGE";
		public const string AutoRegenMajor = "-AUTOREGEN";
		public const string AutoRegenMinor = "-AUTOREGENMINOR";
		public const string BumpVersionMajor = "-BUMP";
		public const string BumpVersionMinor = "-BUMPMINOR";

		public const string SSCheckOut = "-CHECKOUT";
		public const string SSCheckIn = "-CHECKIN";
		public const string SSUndoCheckOut = "-UNDOCHECKOUT";

		CommandLineArgs(string setupAction, string sourceSafeAction, string dataRegenAction, string cwShared, bool showGui)
		{
			SetupAction = setupAction ?? throw new ArgumentNullException(nameof(setupAction));
			SourceSafeAction = sourceSafeAction ?? throw new ArgumentNullException(nameof(sourceSafeAction));
			DataRegenAction = dataRegenAction ?? throw new ArgumentNullException(nameof(dataRegenAction));
			CWShared = cwShared ?? throw new ArgumentNullException(nameof(cwShared));
			ShowGui = showGui;
		}

		public string SetupAction { get; }
		public string SourceSafeAction { get; }
		public string DataRegenAction { get; }
		public string CWShared { get; }
		public bool ShowGui { get; }

		public static CommandLineArgs ParseCommandLineArguments(string[] commandLineArguments)
		{
			string setupAction = null;
			string sourceSafeAction = null;
			string dataRegenAction = null;
			string cwShared = null;
			bool showGui = true;

			foreach (var arg in commandLineArguments)
			{
				if (string.IsNullOrEmpty(arg))
				{
					continue;
				}

				if (IsValidSetupAction(arg))
				{
					if (setupAction != null)
					{
						return null;
					}

					setupAction = arg.ToUpper(CultureInfo.InvariantCulture);
				}
				else if (IsValidSourceSafeAction(arg))
				{
					if (sourceSafeAction != null)
					{
						return null;
					}

					sourceSafeAction = arg.ToUpper(CultureInfo.InvariantCulture);
				}
				else if (IsValidDataRegenAction(arg))
				{
					if (dataRegenAction != null)
					{
						return null;
					}

					dataRegenAction = arg.ToUpper(CultureInfo.InvariantCulture);
				}
				else if (arg.Equals("-NOGUI", StringComparison.OrdinalIgnoreCase))
				{
					showGui = false;
				}
				else if (arg.StartsWith("-CWSHARED=", StringComparison.OrdinalIgnoreCase))
				{
					cwShared = arg.Substring(10);
				}
				else
				{
					return null;
				}
			}

			if (dataRegenAction != null && (setupAction == null || sourceSafeAction == null))
			{
				return null;
			}

			if (sourceSafeAction != null && setupAction == null)
			{
				if (!IsCheckInOrUndoCheckOut(sourceSafeAction))
				{
					return null;
				}
			}

			return new CommandLineArgs(
				setupAction ?? string.Empty,
				sourceSafeAction ?? string.Empty,
				dataRegenAction ?? string.Empty,
				cwShared ?? string.Empty,
				showGui);
		}

		static bool IsValidSetupAction(string action)
		{
			return
				action.Equals(AutoRegenMajor, StringComparison.OrdinalIgnoreCase) ||
				action.Equals(AutoRegenMinor, StringComparison.OrdinalIgnoreCase) ||
				action.Equals(BumpVersionMajor, StringComparison.OrdinalIgnoreCase) ||
				action.Equals(BumpVersionMinor, StringComparison.OrdinalIgnoreCase) ||
				action.Equals(Merge, StringComparison.OrdinalIgnoreCase);
		}

		static bool IsValidSourceSafeAction(string argument) => argument.Equals(SSCheckOut, StringComparison.OrdinalIgnoreCase) || argument.Equals(SSCheckIn, StringComparison.OrdinalIgnoreCase) || argument.Equals(SSUndoCheckOut, StringComparison.OrdinalIgnoreCase);
		static bool IsValidDataRegenAction(string argument) => argument.Equals(GeneratorEntryPoint.DataRegen, StringComparison.OrdinalIgnoreCase) || argument.Equals(GeneratorEntryPoint.NoDataRegen, StringComparison.OrdinalIgnoreCase) || argument.Equals(GeneratorEntryPoint.SkipSchemaRegen, StringComparison.OrdinalIgnoreCase);
		static bool IsCheckInOrUndoCheckOut(string argument) => argument.Equals(SSCheckIn, StringComparison.OrdinalIgnoreCase) || argument.Equals(SSUndoCheckOut, StringComparison.OrdinalIgnoreCase);
	}
}
