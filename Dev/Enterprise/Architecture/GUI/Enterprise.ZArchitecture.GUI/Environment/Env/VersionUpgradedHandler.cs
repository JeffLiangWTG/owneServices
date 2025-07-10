using CargoWise.Definitions;

namespace Enterprise.Environment
{
	public interface IVersionUpgradedHandler
	{
		void Exit(int exitCode);
	}

	public class VersionUpgradedHandler : IVersionUpgradedHandler
	{
		public void Exit(int exitCode)
		{
			System.Environment.Exit(ExitCodes.VersionUpgraded);
		}
	}
}
