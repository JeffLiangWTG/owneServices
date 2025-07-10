using CargoWise.Data.Testing;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ArchiveManager.Test.SystemDescriptors
{
	[UseSnapshotProtection(skipTransaction: true)]
	public abstract class ArchiveSystemIntegrationTest : TestCaseWithFactory
	{
		public TestConfiguration TestConfig { get; set; }

		protected TestArchiveLogger archiveLogger;

		protected override void SetUp()
		{
			base.SetUp();
			archiveLogger = new();
		}
	}
}
