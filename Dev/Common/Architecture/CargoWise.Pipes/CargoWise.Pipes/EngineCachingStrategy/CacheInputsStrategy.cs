using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using QuikGraph;

namespace CargoWise.Pipes
{
	/// <summary>
	/// Use this strategy when individual pipes have a heavy memory cost.
	/// </summary>
	class CacheInputsStrategy : EngineCachingStrategy
	{
		public CacheInputsStrategy(PipeEngineGraph<IPipe> graph)
			: base(graph)
		{
		}

		ConcurrentDictionary<Guid, PipeInputs> InputsByPipe { get; } = new ConcurrentDictionary<Guid, PipeInputs>();

		protected internal override void Initialise()
		{
			foreach (var pipe in Graph.PipesByKey.Values)
			{
				InputsByPipe.TryAdd(pipe.Key, new PipeInputs(pipe));
			}
		}

		protected internal override bool TryGetInput(IPipe pipe, IPipeDataSource dataSource, out object value)
		{
			PipeInputs inputs;
			if (InputsByPipe.TryGetValue(pipe.Key, out inputs))
			{
				return inputs.Inputs.TryGetValue(dataSource.Key, out value);
			}
			else
			{
				value = null;
				return false;
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception text")]
		protected internal override IEnumerable<object> GetInputs(IPipe pipe)
		{
			PipeInputs inputs;
			if (InputsByPipe.TryGetValue(pipe.Key, out inputs))
			{
				foreach (var input in pipe.Inputs)
				{
					object value;
					if (inputs.Inputs.TryGetValue(input.Key, out value))
					{
						yield return value;
					}
					else
					{
						throw pipe.GetPipeException(string.Format(CultureInfo.InvariantCulture, "Input not found. [{0}]", input));
					}
				}
			}
			else
			{
				throw pipe.GetPipeException("Pipe not found. (Probably already finished.)");
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1024:BadConcurrentCollectionAccess", Justification = "Different keys, and the TryRemove guarantees that only one thread will enter the block that touches the collection again")]
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception text")]
		protected internal override bool TryAddResult(IPipe pipe, object result)
		{
			PipeInputs outputInputs;
			IEnumerable<SEdge<Guid>> outEdges;

			if (InputsByPipe.TryRemove(pipe.Key, out outputInputs) && Graph.TryGetOutEdges(pipe.Key, out outEdges))
			{
				foreach (var dependentEdge in outEdges)
				{
					PipeInputs inputs;
					if (InputsByPipe.TryGetValue(dependentEdge.Target, out inputs))
					{
						inputs.FillInput(pipe, result);
					}
					else
					{
						throw pipe.GetPipeException("Partial collision when adding outputs.");
					}
				}

				return true;
			}
			else
			{
				return false;
			}
		}

		protected internal override bool IsStartable(IPipe pipe)
		{
			PipeInputs inputs;
			return InputsByPipe.TryGetValue(pipe.Key, out inputs) && inputs.Startable;
		}

		protected internal override bool IsFinished(IPipe pipe)
		{
			return !InputsByPipe.ContainsKey(pipe.Key);
		}

		protected internal override bool HasUnexecutedPipes
		{
			get { return InputsByPipe.Count > 0; }
		}

		protected internal override object GetResult(IPipeDataSource pipe)
		{
			throw new NotSupportedException("Cannot get result from a pipe engine that is not configured to cache results.");
		}

		class PipeInputs
		{
			public PipeInputs(IPipe pipe)
			{
				remaining = pipe.Inputs.Count();
			}

			int remaining;
			internal ConcurrentDictionary<Guid, object> Inputs { get; } = new ConcurrentDictionary<Guid, object>();

			internal bool Startable
			{
				get { return remaining == 0; }
			}

			internal void FillInput(IPipe inputPipe, object input)
			{
				if (Inputs.TryAdd(inputPipe.Key, input))
				{
					remaining--;
				}
			}
		}
	}
}
