using Dat.Integration;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Dat.Implementation
{
	[CodeAlive("DAT Implementation")]
	public class TestAdapter : ITestAdapter, ICapabilityInfoProvider
	{
		public TestAdapter(TestAdapterContext adapterContext)
		{
			context = adapterContext;
		}

		public ITestDiscovery GetTestDiscovery()
		{
			// Test discovery is run from DAT ImplHost, only test execution is run
			// by CargoWise.WindowsDesktop.exe.
			AssemblyLoader.Enable();

			return new TestDiscovery(context);
		}

		public ITestClient GetTestClient(ITaskLogger logger)
		{
			return new TestClient(context, logger);
		}

		public int GetMaxCapabilityFlagBitPosition()
		{
			return 16;
		}

		readonly TestAdapterContext context;
	}
}
