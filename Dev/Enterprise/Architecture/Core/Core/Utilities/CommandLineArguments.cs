using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using CargoWise.Common.Testing;

namespace Enterprise.ZArchitecture.Core
{
	public class CommandLineArguments
	{
		#region SuppressResourceStringsCheckRegion
		// Resource strings will not be initialized when command line arguments are parsed

		public CommandLineArguments(string argumentFilePath, Hashtable options)
		{
			if (File.Exists(argumentFilePath))
			{
				string arguments = "";

				using (StreamReader reader = File.OpenText(argumentFilePath))
				{
					arguments = reader.ReadToEnd();
				}

				arguments = arguments.Replace("\r\n", "\n");
				OptionalArgs = options;
				ParseArguments(arguments.Split('\n'));
			}
			else
			{
				OptionalArgs = new Hashtable();
				throw new ArgumentException(argumentFilePath + " is not a valid file path");
			}
		}

		public CommandLineArguments(string[] args, Hashtable options)
		{
			OptionalArgs = options;
			ParseArguments(args);
		}

		public string ServerName;
		public string DatabaseName;
		public string ModuleName = "";

		[SuppressWeaklyTypedCollectionMessage]
		public readonly Hashtable OptionalArgs;

		public object this[string key]
		{
			get { return OptionalArgs[key]; }
		}

		public string[] UnparsedArguments
		{
			get;
			private set;
		}

		#region Implementation

		protected void ParseArguments(string[] arguments)
		{
			UnparsedArguments = arguments;

			int argumentCount = 0;

			foreach (var argument in arguments)
			{
				if (IsOption(argument))
				{
					string argumentName;
					object argumentValue;

					if (argument.Contains(":"))
					{
						argumentValue = argument.Substring(argument.IndexOf(":") + 1);
						argumentName = argument.Substring(0, argument.IndexOf(":") + 1);
					}
					else
					{
						argumentValue = true;
						argumentName = argument;
					}

					if (OptionalArgs.ContainsKey(argumentName))
					{
						OptionalArgs[argumentName] = argumentValue;
					}
					else
					{
						ThrowArgumentsException(argument, arguments);
					}
				}
				else
				{
					switch (argumentCount)
					{
						case 0:
							ServerName = argument;
							break;
						case 1:
							DatabaseName = argument;
							break;
						case 2:
							ModuleName = argument;
							break;
						default:
							ThrowArgumentsException(argument, arguments);
							break;
					}

					++argumentCount;
				}
			}
		}

		protected bool IsOption(string argument)
		{
			return argument.StartsWith("-");
		}

		protected void ThrowArgumentsException(string arg, string[] args)
		{
			string message = "Invalid argument: " + arg + System.Environment.NewLine + ValidArgumentsReport + ArgumentsReport(args);
			throw new ArgumentException(message);
		}

		protected string ValidArgumentsReport
		{
			get
			{
				string optionList = "";

				foreach (string key in OptionalArgs.Keys)
				{
					optionList += " <" + key + "> ";
				}

				return
					"Valid arguments are: " +
					optionList +
					" <ServerName> <DatabaseName> <ModuleName>" +
					System.Environment.NewLine;
			}
		}

		protected string ArgumentsReport(string[] args)
		{
			string result = "Arguments were: " + System.Environment.NewLine;

			foreach (string arg in args)
			{
				result += arg + System.Environment.NewLine;
			}

			return result;
		}

		#endregion

		public override string ToString()
		{
			var arguments = new List<string>();
			if (ServerName != null)
			{
				arguments.Add(CommandLineArgEncoder.EnquoteArgumentIfNeeded(ServerName));
			}

			if (DatabaseName != null)
			{
				arguments.Add(CommandLineArgEncoder.EnquoteArgumentIfNeeded(DatabaseName));
			}

			if (ModuleName != null)
			{
				arguments.Add(CommandLineArgEncoder.EnquoteArgumentIfNeeded(ModuleName));
			}

			foreach (string passArgument in OptionalArgs.Keys)
			{
				object value = this[passArgument];
				if (value is bool && (bool)value)
				{
					arguments.Add(passArgument);
				}
				else if (value is string)
				{
					arguments.Add(passArgument + CommandLineArgEncoder.EnquoteArgumentIfNeeded((string)value));
				}
			}
			return string.Join(" ", arguments);
		}

		public CommandLineArguments Clone()
		{
			return new CommandLineArguments(UnparsedArguments, OptionalArgs);
		}

		[SuppressThreadStaticFieldMessage]
		public static CommandLineArguments UsedToLaunchApplication;
		#endregion
	}
}
