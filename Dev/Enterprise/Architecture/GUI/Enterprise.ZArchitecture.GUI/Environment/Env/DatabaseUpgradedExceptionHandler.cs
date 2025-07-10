using CargoWise.Definitions;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Environment
{
	public interface IDatabaseUpgradedExceptionHandler
	{
		void Restart();
		void Exit(int exitCode);
	}

	public class DatabaseUpgradedExceptionHandler : IDatabaseUpgradedExceptionHandler
	{
		public void Restart()
		{
			ProgramRestarter.Instance.Restart();
		}

		public void Exit(int exitCode)
		{
			System.Environment.Exit(ExitCodes.DatabaseUpgraded);
		}
	}
}
