using CargoWise.Async;
using NUnit.Framework;

namespace CargoWise.Pipes.Test
{
	abstract class PipeEngineTestCase : TestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			MockDispatcher = new MockDispatcher();
			AsyncStrategy = DefaultAsyncStrategy.Get();
			Engine = new PipeEngine(EngineConfigOptions.CacheAllResults);
		}

		protected IAsyncStrategy AsyncStrategy { get; set; }
		protected MockDispatcher MockDispatcher { get; set; }
		protected PipeEngine Engine { get; set; }
	}
}
