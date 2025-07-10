using System.Collections.Generic;

namespace CargoWise.Pipes
{
	abstract class EngineCachingStrategy
	{
		protected EngineCachingStrategy(PipeEngineGraph<IPipe> graph)
		{
			Graph = graph;
		}

		protected PipeEngineGraph<IPipe> Graph { get; private set; }

		protected internal abstract void Initialise();

		protected internal void Complete()
		{
			Graph = null; // to avoid memory leaks after completion if pipes accidentally hold instances in memory
		}

		protected internal abstract bool IsStartable(IPipe pipe);
		protected internal abstract bool IsFinished(IPipe pipe);
		protected internal abstract object GetResult(IPipeDataSource pipe);
		protected internal abstract bool TryGetInput(IPipe pipe, IPipeDataSource dataSource, out object value);
		protected internal abstract bool TryAddResult(IPipe pipe, object result);
		protected internal abstract IEnumerable<object> GetInputs(IPipe pipe);
		protected internal abstract bool HasUnexecutedPipes { get; }
	}
}
