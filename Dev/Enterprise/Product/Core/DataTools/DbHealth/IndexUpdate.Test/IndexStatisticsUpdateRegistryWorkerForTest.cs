using Enterprise.DbHealth.Shared.Test;

namespace Enterprise.DbHealth.IndexUpdate.Testing
{
	sealed class IndexStatisticsUpdateRegistryWorkerForTest : DbRegistryWorkerForTest
	{
		public override bool SteppingApplied
		{
			get { return false; }
		}
	}
}
