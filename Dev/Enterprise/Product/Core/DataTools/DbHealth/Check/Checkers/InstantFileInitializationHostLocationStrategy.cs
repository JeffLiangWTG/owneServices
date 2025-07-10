using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DbHealth.Check
{
	class InstantFileInitializationHostLocationStrategy : IInstantFileInitializationStrategy
	{
		public bool? IsInstantFileInitializationEnabled(ILogger logger)
		{
			if (EnvProxy.IsHostedWithCargowise)
			{
				return true; // We monitor WiseCloud servers using other tools
			}

			return null;
		}
	}
}
