using System;
using System.Linq;
using CargoWise.Definitions;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Startup.Tasks
{
	public class ProductBrandingCommandLineDeterminerTask : ProductBrandingDeterminerTask
	{
		public ProductBrandingCommandLineDeterminerTask(Action updateBrandingCallback)
			: base(updateBrandingCallback)
		{
		}

		public override int FailureExitCode => ExitCodes.ProductBrandingCommandLineDeterminerTaskError;

		public override bool CheckForProductivityWiseModeEnabled(CommandLineArguments arguments) => arguments.UnparsedArguments.Any(a => a == "-PW");
	}
}
