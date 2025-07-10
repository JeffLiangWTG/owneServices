using CargoWise.Bi.Development.PowerBiSync;
using CargoWise.BuildTools.Testing;
using NUnit.Framework;

namespace CargoWise.Bi.Development.SchemaSync.Testing
{
	class RunSyncTest : TestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRunSync()
		{
			MockSourceControl.Setup();
			AssertNoExceptionThrown(PowerBiSynchroniser.Sync);
			MockSourceControl.TearDown();
		}
	}
}
