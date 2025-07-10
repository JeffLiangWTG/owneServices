using CargoWise.Common;
using CargoWise.Loader.Common;

namespace CargoWise.Loader.Client
{
	public class ClientInstallation : Installation
	{
		public ClientInstallation(ClientConfiguration configuration)
			: base(configuration)
		{
			Argument.NotNull(configuration, nameof(configuration)); // Suggested By ReviewBot 
		}

		public new ClientConfiguration Configuration
		{
			get
			{
				return (ClientConfiguration)base.Configuration;
			}
		}
	}

	public abstract class ClientConfiguration : Configuration
	{
		const string PathArgumentPrefix = "-path=";

		protected ClientConfiguration()
		{
			OtherProgramArguments = string.Empty;
		}

		protected string OtherProgramArguments { get; set; }

		public virtual string ProgramArguments
		{
			get { return string.Empty; }
		}

		public abstract string ProgramFileName { get; }

		protected sealed override void ParseCommandLineArgument(string arg)
		{
			// These arguments are only important for this instance of the loader.
			if (!ParseCommandLineArgument(arg, CommandLineArgumentType.ThisLoaderInstance))
			{
				if (arg != null && arg.StartsWith(PathArgumentPrefix))
				{
					BaseTargetPath = arg.Substring(PathArgumentPrefix.Length).Replace("\"", string.Empty);
				}
				else if (!ParseCommandLineArgument(arg, CommandLineArgumentType.NextLoaderInstance))
				{
					// These arguments should also be passed on to the program being launched.
					if (!string.IsNullOrEmpty(OtherProgramArguments))
					{
						OtherProgramArguments += ' ';
					}
					OtherProgramArguments += EscapeArgument(arg);
					ParseCommandLineArgument(arg, CommandLineArgumentType.ProgramBeingLaunched);
				}
			}
		}

		protected virtual bool ParseCommandLineArgument(string arg, CommandLineArgumentType type)
		{
			return false;
		}
	}
}
