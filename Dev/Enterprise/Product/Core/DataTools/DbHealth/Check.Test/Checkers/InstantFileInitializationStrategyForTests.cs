using Enterprise.Integration;

namespace Enterprise.DbHealth.Check
{
	sealed class InstantFileInitializationStrategyForTests : IInstantFileInitializationStrategy
	{
		readonly bool? result;

		public InstantFileInitializationStrategyForTests(bool? result)
		{
			this.result = result;
		}

		public bool WasCalled { get; private set; }

		public bool? IsInstantFileInitializationEnabled(ILogger logger)
		{
			WasCalled = true;
			return result;
		}
	}
}
