using System;

namespace Enterprise.Builder.Generator
{
	public sealed class GeneratorArguments
	{
		public static GeneratorArguments Parse(string[] args)
		{
			string serverName = null;
			string databaseName = null;
			string option = null;
			string argument = null;
			string argument2 = null;
			string cwsharedPath = null;

			foreach (var arg in args)
			{
				if (string.IsNullOrEmpty(arg))
				{
					continue;
				}

				if (!arg.StartsWith("-"))
				{
					if (serverName == null)
					{
						serverName = arg;
						continue;
					}
					else if (databaseName == null)
					{
						databaseName = arg;
						continue;
					}
				}
				else if (arg.StartsWith("-CWSHARED=", StringComparison.OrdinalIgnoreCase))
				{
					if (cwsharedPath != null)
					{
						throw new CommandLineArgumentException("Cannot specify -cwshared multiple times.");
					}

					cwsharedPath = arg.Substring(10);
					continue;
				}

				if (option == null)
				{
					option = arg;
					continue;
				}
				else if (argument == null)
				{
					argument = arg;
					continue;
				}
				else if (argument2 == null)
				{
					argument2 = arg;
					continue;
				}
				else
				{
					throw new CommandLineArgumentException("Unrecognised argument: " + arg);
				}
			}

			if (serverName == null || databaseName == null)
			{
				throw new CommandLineArgumentException("Server and database names must be specified.");
			}

			return new GeneratorArguments(serverName, databaseName, option ?? string.Empty, argument ?? string.Empty, argument2 ?? string.Empty, cwsharedPath);
		}

		public GeneratorArguments(string serverName, string databaseName, string option = null, string argument = null, string argument2 = null, string cwsharedPath = null)
		{
			ServerName = serverName;
			DatabaseName = databaseName;
			Option = option ?? string.Empty;
			Argument = argument ?? string.Empty;
			Argument2 = argument2 ?? string.Empty;
			CWSharedPath = cwsharedPath;
		}

		public string ServerName { get; }
		public string DatabaseName { get; }
		public string Option { get; }
		public string Argument { get; }
		public string Argument2 { get; }
		public string CWSharedPath { get; }
	}
}
