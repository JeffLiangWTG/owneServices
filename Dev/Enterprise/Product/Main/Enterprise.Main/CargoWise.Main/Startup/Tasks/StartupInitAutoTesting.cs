#if DEBUG

using CargoWise.Definitions;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Startup
{
	class StartupInitAutoTesting : InitializingApplicationStartupTask
	{
		public override string TaskDescription => "AutoTesting";

		public override int FailureExitCode => ExitCodes.StartupInitAutoTestingError;

		protected override bool DoExecute(CommandLineArguments arguments)
		{
			string enterprisePath = (string)arguments[ApplicationArguments.OptionDatEnterprisePath];
			SetIsRunningOnDAT(arguments);
			SetRecordBaseline(arguments);
			if (!string.IsNullOrEmpty(enterprisePath))
			{
				NUnit.Framework.TestCase.BaseSourcePath = enterprisePath;
			}
			return true;
		}

		void SetIsRunningOnDAT(CommandLineArguments arguments)
		{
			TestingState.IsRunningOnDAT = (bool)arguments[ApplicationArguments.OptionDat];
		}

		void SetRecordBaseline(CommandLineArguments arguments)
		{
			TestingState.RecordBaseline = (bool)arguments[ApplicationArguments.OpenRecordBaseline];
		}
	}
}

#endif
