using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Enterprise.DataTools.DbBackupAndRestore.Business;

namespace Enterprise.DataTools.DbBackupAndRestore
{
	public class CommandLineSet
	{
		public CommandLineSet()
		{
			Add("h|help", "Help", c => { Defaults.Instance.DisplayHelpText = true; });

			AddMainDatabaseArgumentOptions();
			AddRestoreOptions();
			AddAuditDatabaseArgumentOptions();
			AddEdwDatabaseArgumentOptions();
			AddAvailabilityGroupOptions();
		}

		void AddMainDatabaseArgumentOptions()
		{
			Add("s|server", "Destination SQL Instance", c => { Defaults.Instance.ServerName = c?.Parameter; });
			Add("d|database", "Target database name", c => { Defaults.Instance.DatabaseName = c?.Parameter; });
			Add("bak|backup", "Backup path (full path of the main database .bak/.dbk file)", c => { Defaults.Instance.BackupFileName = c?.Parameter; });
			Add("dfp|DataFilePath", "Data File Path", c => { Defaults.Instance.DataFilePath = c?.Parameter; });
			Add("lfp|LogFilePath", "Log File Path", c => { Defaults.Instance.LogFilePath = c?.Parameter; });
		}

		void AddRestoreOptions()
		{
			Add("r|restore", "Restore database without GUI", c => { Defaults.Instance.RunDbRestore = c != null; });
			Add("ro|restoreoption", "RestoreWithRecovery | RestoreWithNoRecovery | ProdToTestCopy", c => { SetRestoreOption(c?.Parameter); });
			Add("SalesDbRestore", "", c => { Defaults.Instance.RunSalesDbRestore = c != null; Defaults.Instance.SalesDbRestore = c?.Parameter; });
			Add("resdiff|RestoreDifferentialBackup", "Yes | No", c => { Defaults.Instance.RestoreDifferentialBackups = (c != null) && (c.Parameter == "Yes"); });
			Add("reslog|RestoreTransactionLogBackup", "Yes | No", c => { Defaults.Instance.RestoreTransactionLogBackups = (c != null) && (c.Parameter == "Yes"); });
			Add("excludeauditdb", "Restore database without Audit DB", c => { Defaults.Instance.IsAuditDBExcludedFromRestore = c != null; });
		}

		void AddAuditDatabaseArgumentOptions()
		{
			Add("as|auditserver", "Destination SQL instance for audit database", c => { Defaults.Instance.AuditServerName = c?.Parameter; });
			Add("ab|auditbackup", "Backup path (full path of the audit database .bak/.dbk file)", c => { Defaults.Instance.AuditBackupFileName = c?.Parameter; });
			Add("adfp|AuditDataFilePath", "Audit Data File Path", c => { Defaults.Instance.AuditDataFilePath = c?.Parameter; });
			Add("alfp|AuditLogFilePath", "Audit Log File Path", c => { Defaults.Instance.AuditLogFilePath = c?.Parameter; });
		}

		void AddEdwDatabaseArgumentOptions()
		{
			Add("dws|datawarehouseserver", "Destination SQL instance for EDW database", c => { Defaults.Instance.DataWarehouseServerName = c?.Parameter; });
			Add("eb|edwbackup", "Backup path (full path of the EDW database .bak/.dbk file)", c => { Defaults.Instance.EdwBackupFileName = c?.Parameter; });
			Add("edfp|EdwDataFilePath", "EDW Data File Path", c => { Defaults.Instance.EdwDataFilePath = c?.Parameter; });
			Add("elfp|EdwLogFilePath", "EDW Log File Path", c => { Defaults.Instance.EdwLogFilePath = c?.Parameter; });
		}

		void AddAvailabilityGroupOptions()
		{
			Add("adag|AddDatabaseToAvailabilityGroup", "Yes | No", c => { Defaults.Instance.AddDbToAvailabilityGroup = (c != null) && (c.Parameter == "Yes"); });
			Add("ag|availabilitygroup", "Availability Group", c => { Defaults.Instance.AvailabilityGroup = c?.Parameter; });
		}

		public class CommandLineContext
		{
			public CommandLineContext(string name, string parameter)
			{
				Name = name;
				Parameter = parameter;
			}

			public string Parameter
			{
				get;
			}

			public string Name
			{
				get;
			}
		}

		class CommandLineOptions
		{
			public CommandLineOptions(String filter, String description, Action<CommandLineContext> action)
			{
				this.filter = filter;
				this.description = description;
				this.action = action;
			}

			public readonly Action<CommandLineContext> action;
			public readonly String filter;
			public readonly String description;
		}

		readonly List<CommandLineOptions> options = new List<CommandLineOptions>();

		#region Help Text

		public void DisplayHelpText()
		{
			Logger.Instance.LogMessage("Usage: Enterprise.DbBackupAndRestore.exe [option] [value]");
			Logger.Instance.LogMessage("");
			Logger.Instance.LogMessage("Options:");
			Logger.Instance.LogMessage("");

			foreach (var option in options.Where(o => o.description != "Help"))
			{
				Logger.Instance.LogMessage($"\t-{option.filter}{GetSpacesForHelp(option.filter)}{option.description}");
			}
		}

		string GetSpacesForHelp(string filter)
		{
			var maxLength = options.Max(o => o.filter.Length) + 1;
			var requiredTabs = maxLength + 8 - filter.Length;

			return new string(' ', requiredTabs);
		}

		#endregion

		void SetRestoreOption(string restoreOption)
		{
			if (!string.IsNullOrEmpty(restoreOption))
			{
				if (Enum.TryParse(restoreOption, true, out DbRestoreOption cmdOption))
				{
					Defaults.Instance.RestoreOption = cmdOption;
				}
				else
				{
					Logger.Instance.LogErrorMessage(string.Format(CultureInfo.InvariantCulture, "Restore option '{0}' is not a valid option.", restoreOption));
					throw new ArgumentException(restoreOption);
				}
			}
		}

		void Add(string filter, String description, Action<CommandLineContext> action)
		{
			options.Add(new CommandLineOptions(filter, description, action));
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public bool Parse(string[] args)
		{
			var argList = FixArgumentFormat(args);
			if (CheckParametersAreValid(argList))
			{
				try
				{
					options.ForEach(option =>
					{
						var regEx = new Regex(string.Format(CultureInfo.InvariantCulture, "^-({0})$", option.filter), RegexOptions.IgnoreCase);
						for (int pos = 0; pos < argList.Count; ++pos)
						{
							if (regEx.IsMatch(argList[pos]))
							{
								var argValue = ((pos + 1 < argList.Count) && (!argList[pos + 1].StartsWith("-", StringComparison.OrdinalIgnoreCase))) ? argList[pos + 1] : null;
								option.action.Invoke(new CommandLineContext(argList[pos], argValue));
								break;
							}
						}
					});
					return true;
				}
				catch
				{
					return false;
				}
			}
			else
			{
				return false;
			}
		}

		List<string> FixArgumentFormat(string[] args)
		{
			var argList = new List<string>();

			foreach (var arg in args)
			{
				var argLine = arg.Replace("\" ", "\\\" ");
				argList.AddRange(argLine.Split('\"').Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x)));
			}
			argList = AddMissingArguments(argList);

			return argList;
		}

		List<string> AddMissingArguments(List<string> argList)
		{
			if (argList == null || argList.Count == 0)
			{
				return argList;
			}

			if (!argList.Any(x => x.Equals("-s", StringComparison.OrdinalIgnoreCase) || x.Equals("-server", StringComparison.OrdinalIgnoreCase)) &&
				!argList[0].StartsWith("-", StringComparison.OrdinalIgnoreCase))
			{
				argList.Insert(0, "-s");
			}
			if (argList.Count >= 3 &&
				!argList.Any(x => x.Equals("-d", StringComparison.OrdinalIgnoreCase) || x.Equals("-database", StringComparison.OrdinalIgnoreCase)) &&
				(argList[0].Equals("-s", StringComparison.OrdinalIgnoreCase) || argList[0].Equals("-server", StringComparison.OrdinalIgnoreCase)) &&
				!argList[2].StartsWith("-", StringComparison.OrdinalIgnoreCase))
			{
				argList.Insert(2, "-d");
			}
			return argList;
		}

		bool CheckParametersAreValid(List<string> argList)
		{
			var result = true;

			foreach (var arg in argList.Where(a => a.StartsWith("-", StringComparison.OrdinalIgnoreCase)))
			{
				var isValidParameter = false;
				foreach (var option in options)
				{
					var regEx = new Regex(string.Format(CultureInfo.InvariantCulture, "^-({0})$", option.filter), RegexOptions.IgnoreCase);
					if (regEx.IsMatch(arg))
					{
						isValidParameter = true;
						break;
					}
				}

				if (!isValidParameter)
				{
					result = false;
					break;
				}
			}

			return result;
		}

		public override string ToString()
		{
			string result = "";
			options.ForEach(o =>
			{
				result += System.Environment.NewLine + string.Format(CultureInfo.InvariantCulture, "-[{0}]\t{1}", o.filter, o.description);
			});

			return result;
		}
	}
}
