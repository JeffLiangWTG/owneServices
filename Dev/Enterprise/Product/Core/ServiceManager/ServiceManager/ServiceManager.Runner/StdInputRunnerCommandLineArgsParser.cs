using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Runner.Abstractions;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Runner
{
	public class StdInputRunnerCommandLineArgsParser
	{
		internal static class ProcessCommands
		{
			public const string Stop = "stop";
			public const string Exit = "exit";
			public const string Run = "run";
		}

		readonly IClientHostedServiceAttributeProvider hostedServiceAttributeProvider;

		public StdInputRunnerCommandLineArgsParser(IClientHostedServiceAttributeProvider hostedServiceAttributeProvider)
		{
			this.hostedServiceAttributeProvider = hostedServiceAttributeProvider;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1106:Do Not Leave In Debug Messages", Justification = "This is a console application.")]
		public ICommandInfo Parse(string args)
		{
			var splitArgs = GetArgs(args);
			if (splitArgs.Length <= 0)
			{
				throw new ArgumentException("No arguments specified.");
			}

			string assemblyName = null;
			string code = null;
			string command = null;
			string configString = null;

			foreach (var splitArg in splitArgs)
			{
				if (splitArg.StartsWith(OptionAssemblyName, StringComparison.OrdinalIgnoreCase))
				{
					assemblyName = splitArg.Substring(OptionAssemblyName.Length);
				}

				if (splitArg.StartsWith(OptionCode, StringComparison.OrdinalIgnoreCase))
				{
					code = splitArg.Substring(OptionCode.Length);
				}

				if (splitArg.StartsWith(OptionConfigString, StringComparison.OrdinalIgnoreCase))
				{
					configString = splitArg.Substring(OptionConfigString.Length);
				}
			}

			var validCommands = typeof(ProcessCommands).GetFields(BindingFlags.Static | BindingFlags.Public).Select(field => field.GetValue(null).ToString());
			if (!validCommands.Any(processCommand => splitArgs[0].Equals(processCommand, StringComparison.OrdinalIgnoreCase)))
			{
				command = ProcessCommands.Run;
				if (assemblyName == null && code == null)
				{
					code = splitArgs[0];
				}
			}
			else
			{
				command = splitArgs[0];
				if (assemblyName == null && code == null)
				{
					code = splitArgs.Length > 1 ? splitArgs[1] : null;
				}
			}

			if (command.Equals(ProcessCommands.Stop, StringComparison.OrdinalIgnoreCase) ||
				command.Equals(ProcessCommands.Exit, StringComparison.OrdinalIgnoreCase))
			{
				return new StopCommandInfo();
			}

			if (command.Equals(ProcessCommands.Run, StringComparison.OrdinalIgnoreCase))
			{
				if (code == null)
				{
					throw new ArgumentException("No code specified.");
				}

				if (assemblyName == null)
				{
					var serviceConfig = hostedServiceAttributeProvider.GetClientHostedServiceAttribute(code);
					assemblyName = serviceConfig?.TypeAssemblyName;
					if (assemblyName == null)
					{
						throw new ArgumentException($"Cannot determine assembly name from code '{code}'. (Make sure you have executed AssemblyMetaDataExtractor.exe, e.g. via QGL)");
					}
					if ((typeof(IServiceTaskConfigurationUser).IsAssignableFrom(serviceConfig.Type) ||
						typeof(IServiceTaskConfigurationUserProvider).IsAssignableFrom(serviceConfig.Type))
						&& string.IsNullOrEmpty(configString))
					{
						Console.WriteLine($"{serviceConfig.Code} service task will run with the configuration settings from the task schedule. \"{OptionConfigString}\" argument can be used to override it.");
					}
				}
			}

			if (command.Equals(ProcessCommands.Run, StringComparison.OrdinalIgnoreCase))
			{
				return new DirectRunCommandInfo(assemblyName, code, Guid.NewGuid(), configString);
			}

			throw new InvalidOperationException($"Unrecognised command [{command}].");
		}

		const string OptionAssemblyName = "-assemblyName:";
		const string OptionCode = "-code:";
		const string OptionConfigString = "-configString:";

		static string[] GetArgs(string args)
		{
			var regex = new Regex("[^\\s\"']+|\"[^\"]*\"|'[^']*'");
			var matches = regex.Matches(args);
			return matches.Cast<Match>().Select(x => x.Value).ToArray();
		}
	}
}
