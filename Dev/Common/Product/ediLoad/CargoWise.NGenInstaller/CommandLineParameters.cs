using System;
using CargoWise.NGenInstallerProgram.Exceptions;

namespace CargoWise.NGenInstallerProgram
{
	class CommandLineParameters
	{
		public NGenAction Action { get; private set; }
		public string RootFilePath { get; private set; }
		public TimeSpan? ExecutionTimeout { get; private set; }
		public TimeSpan DelayTimeForTest { get; private set; }
		public string NGenPathForTest { get; private set; }

		CommandLineParameters()
		{
		}

		public static CommandLineParameters Parse(string[] args)
		{
			try
			{
				return new CommandLineParameters
				{
					Action = (NGenAction)Enum.Parse(typeof(NGenAction), args[0]),
					RootFilePath = args[1],
					ExecutionTimeout = args.Length > 2 && args[2].Length > 0 ? TimeSpan.FromSeconds(int.Parse(args[2])) : null,
					DelayTimeForTest = args.Length > 3 ? TimeSpan.FromMilliseconds(int.Parse(args[3])) : TimeSpan.Zero,
					NGenPathForTest = args.Length > 4 ? args[4] : string.Empty,
				};
			}
			catch (Exception ex)
			{
				throw new ParameterParseException(ex);
			}
		}
	}
}
