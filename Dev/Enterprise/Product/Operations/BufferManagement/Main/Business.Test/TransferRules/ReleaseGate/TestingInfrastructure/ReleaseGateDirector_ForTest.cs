using Enterprise.Integration;

namespace Enterprise.BufferManagement.Business.Test
{
	public class ReleaseGateDirector_ForTest : ReleaseGateDirector
	{
		public ReleaseGateDirector_ForTest(BMSystem system, ILogger logger, ReleaseGateLogger releaseGateLogger, ReleaseGateTestCoordinator coordinator)
			: base(system, logger, releaseGateLogger)
		{
			this.coordinator = coordinator;
		}

		readonly ReleaseGateTestCoordinator coordinator;

		protected override ReleaseGateKeeper GetReleaseGateKeeper(BMComponent buffer, ILogger logger, ReleaseGateLogger gateLogger, bool enableDataRefresh)
		{
			return new ReleaseGateKeeper_ForTest(buffer, logger, gateLogger, enableDataRefresh, coordinator);
		}
	}
}
