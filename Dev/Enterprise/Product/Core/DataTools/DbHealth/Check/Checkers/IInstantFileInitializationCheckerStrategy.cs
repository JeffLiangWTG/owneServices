using Enterprise.Integration;

namespace Enterprise.DbHealth.Check
{
	interface IInstantFileInitializationStrategy
	{
		bool? IsInstantFileInitializationEnabled(ILogger logger);
	}
}
