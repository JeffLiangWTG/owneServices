using CargoWise.Definitions;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Startup
{
	class StartupDocManagerCommandLineHandler : InitializingApplicationStartupTask
	{
		public override string TaskDescription
		{
			get { return (NoResString)"DocManager Command Line Handler"; }
		}

		public override int FailureExitCode => ExitCodes.StartupDocManagerCommandLineHandlerError;

		protected override bool GetShouldExecute(CommandLineArguments arguments)
		{
			return (bool)arguments[ApplicationArguments.OptionScanStart];
		}

		protected override bool DoExecute(CommandLineArguments arguments)
		{
			if (!DocumentScanningOptions(arguments))
			{
				return false;
			}
			return true;
		}

		bool DocumentScanningOptions(CommandLineArguments cmdLineArguments)
		{
			DocumentScanning.Launch.DocManagerOptionsHelp dMOptionsHelp = new DocumentScanning.Launch.DocManagerOptionsHelp();

			try
			{
				DocumentScanning.Launch.DocManagerOptionsHelp.OptionsResult optResult = dMOptionsHelp.ProcessOptions(cmdLineArguments);

				switch (optResult)
				{
					case Enterprise.DocumentScanning.Launch.DocManagerOptionsHelp.OptionsResult.opClose:
						return false;

					case Enterprise.DocumentScanning.Launch.DocManagerOptionsHelp.OptionsResult.opOpenAllocateDocuments:
						((WinFormsEnvironment)Env.Instance).OpenDocumentScanningForm = true;
						break;
				}
			}
			finally
			{
				dMOptionsHelp = null;
			}

			return true;
		}
	}
}
