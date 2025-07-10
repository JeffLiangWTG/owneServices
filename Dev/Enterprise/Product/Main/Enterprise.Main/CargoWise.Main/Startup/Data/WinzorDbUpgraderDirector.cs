using CargoWise.Definitions;
using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Startup
{
	[CodeAlive("Used as an application startup task.")]
	public class WinzorDbUpgraderDirector : IApplicationStartupTask
	{
		readonly DbUpgraderDirector dbUpgraderDirector;

		public WinzorDbUpgraderDirector(ApplicationArguments arguments)
		{
			dbUpgraderDirector = DbUpgraderDirector.New(arguments);
		}

		public string TaskDescription => dbUpgraderDirector.TaskDescription;

		public int FailureExitCode { get; private set; } = ExitCodes.DatabaseUpgraded;

		public bool Execute(CommandLineArguments arguments)
		{
			var dbUpgradeDirectorResult = dbUpgraderDirector.Execute(arguments);

			if (!dbUpgradeDirectorResult)
			{
				System.Environment.Exit(ExitCodes.DatabaseUpgraded);
			}
			return dbUpgradeDirectorResult;
		}

		public bool ShouldExecute(CommandLineArguments arguments) => dbUpgraderDirector.ShouldExecute(arguments);
	}
}

