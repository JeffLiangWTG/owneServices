using System.Linq;
using CargoWise.Pipes;
using CargoWise.Pipes.Test;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	class PipeFactoryHandoverTest : BMSTestCaseWithFactory
	{
		[RequiresSTA]
		public void TestPipeFactoryHandover()
		{
			var engine = new PipeEngine();
			var factory = Factory.CreateNewFactory();
			factory.ThreadSentry.RelinquishThreadOwnership();

			var factoryPipe = engine.AddSync(() => factory).AddHandover(f => f.ThreadSentry.TakeThreadOwnership(), f => f.ThreadSentry.RelinquishThreadOwnership());
			var parallels = Enumerable.Range(0, 10).Select(i => engine.AddAsync(f => f.NewWithValidTestData<ProcessHeader>(), factoryPipe)).ToArray();

			var mock = new MockDispatcher();
			using (var set = engine.ExecuteAll(mock, mock))
			{
				mock.DispatchAll();
				set.AwaitAll();

				AssertEquals(false, factory.ThreadSentry.IsOwner);
			}
		}
	}
}
