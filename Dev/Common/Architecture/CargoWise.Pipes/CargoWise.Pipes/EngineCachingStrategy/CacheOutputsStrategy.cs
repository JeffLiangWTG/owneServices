using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using QuikGraph;

namespace CargoWise.Pipes
{
	class CacheOutputsStrategy : EngineCachingStrategy
	{
		public CacheOutputsStrategy(PipeEngineGraph<IPipe> graph)
			: base(graph)
		{
		}

		ConcurrentDictionary<Guid, object> CompletedPipeDataSources { get; } = new ConcurrentDictionary<Guid, object>();

		protected internal override void Initialise()
		{
			// No work to be done.
		}

		protected internal override bool IsStartable(IPipe pipe)
		{
			IEnumerable<SEdge<Guid>> inEdges;

			if (Graph.TryGetInEdges(pipe.Key, out inEdges))
			{
				return inEdges.All(edge => CompletedPipeDataSources.ContainsKey(edge.Source));
			}
			else
			{
				return false;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Error message")]
		protected internal override IEnumerable<object> GetInputs(IPipe pipe)
		{
			foreach (var input in pipe.Inputs)
			{
				object value;
				if (CompletedPipeDataSources.TryGetValue(input.Key, out value))
				{
					yield return value;
				}
				else
				{
					throw pipe.GetPipeException(string.Format(CultureInfo.InvariantCulture, "Could not find input: {0}", input.DebugName));
				}
			}
		}

		protected internal override bool IsFinished(IPipe pipe)
		{
			return CompletedPipeDataSources.ContainsKey(pipe.Key);
		}

		protected internal override bool TryAddResult(IPipe pipe, object result)
		{
			return CompletedPipeDataSources.TryAdd(pipe.Key, result);
		}

		protected internal override bool TryGetInput(IPipe destination, IPipeDataSource input, out object value)
		{
			return CompletedPipeDataSources.TryGetValue(input.Key, out value);
		}

		protected internal override object GetResult(IPipeDataSource pipe)
		{
			return CompletedPipeDataSources[pipe.Key];
		}

		protected internal override bool HasUnexecutedPipes
		{
			get { return CompletedPipeDataSources.Count < Graph.VertexCount; }
		}
	}
}
