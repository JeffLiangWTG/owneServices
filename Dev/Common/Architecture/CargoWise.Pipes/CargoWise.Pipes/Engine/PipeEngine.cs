using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Async;
using QuikGraph;
using QuikGraph.Algorithms;

namespace CargoWise.Pipes
{
	public delegate void OnExecutionCompleted();

	public class PipeEngine
	{
		public PipeEngine(EngineConfigOptions config = EngineConfigOptions.None, OnExecutionCompleted onCompleted = null, string id = null)
		{
			Config = config;
			OnCompleted = onCompleted;
			ID = id;
		}

		EngineConfigOptions Config { get; }
		OnExecutionCompleted OnCompleted { get; }
		PipeEngineGraph<IPipe> Graph { get; } = new PipeEngineGraph<IPipe>();

		bool started;
		public string ID { get; }

		#region API

		public Pipe<TOutput> AddPipe<TOutput>(PipeType type, Delegate action, params IPipeDataSource[] inputs)
		{
			var pipe = new Pipe<TOutput>(type, action, inputs);
			AddPipe(pipe);
			return pipe;
		}

		public void AddPipe(IPipe pipe)
		{
			AddPipeCore(pipe);
		}

		public EngineExecutionResultSet ExecuteAll(IAsyncStrategy asyncStrategy, IDispatcher dispatcherStrategy)
		{
			EnsureNotStarted();
			started = true;

			var engineExecutionSet = new EngineExecutionResultSet(Config.HasFlag(EngineConfigOptions.CacheAllResults) ? new CacheOutputsStrategy(Graph) : new CacheInputsStrategy(Graph));
			ExecutePipes(engineExecutionSet, GetInitialPipes(), asyncStrategy, dispatcherStrategy, 0);

			return engineExecutionSet;
		}

		#endregion

		#region Populate

		void AddPipeCore(IPipe pipe)
		{
			EnsureNotStarted();
			Graph.AddVertex(pipe);
			var key = pipe.Key;
			Graph.AddEdgeRange(pipe.Inputs.Select(input => new SEdge<Guid>(input.Key, key)));
		}

		void EnsureNotStarted()
		{
			if (started)
			{
				throw new InvalidOperationException("Re-using a pipe engine is not yet supported. Engine cannot be modified after running.");
			}
		}

		#endregion

		#region Execute Pipes

		void ExecutePipes(EngineExecutionResultSet resultSet, IEnumerable<IPipe> pipes, IAsyncStrategy asyncStrategy, IDispatcher dispatcherStrategy, int depth)
		{
			var queue = new Queue<IPipe>(pipes);

			while (!resultSet.IsDisposed && queue.Count > 0)
			{
				var pipe = queue.Dequeue();
				ExecutePipe(resultSet, pipe, asyncStrategy, dispatcherStrategy, depth);
			}

			if (!resultSet.Cache.HasUnexecutedPipes)
			{
				resultSet.Cache.Complete();

				if (OnCompleted != null)
				{
					dispatcherStrategy.Dispatch(OnCompleted);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Error message., Debug only thread name, Debug only thread ID")]
		void ExecutePipe(EngineExecutionResultSet resultSet, IPipe pipe, IAsyncStrategy asyncStrategy, IDispatcher dispatcherStrategy, int depth)
		{
			var action = new Func<object>(() =>
			{
				var handovers = pipe.Inputs.OfType<IPipeHandoverWrapper>().ToArray();
				if (!resultSet.IsDisposed && !resultSet.Cache.IsFinished(pipe) && TryExecuteHandoverClaims(resultSet, pipe, handovers))
				{
					var result = pipe.Do(resultSet.Cache.GetInputs(pipe).ToArray());
					ExecuteHandoverReleases(resultSet, pipe, handovers);

					if (!resultSet.Cache.TryAddResult(pipe, result))
					{
						throw pipe.GetPipeException("Already has input.");
					}
					ExecutePipes(resultSet, GetStartableDependentPipes(resultSet, pipe).Union(handovers.SelectMany(h => GetStartableDependentPipes(resultSet, h))), asyncStrategy, dispatcherStrategy, depth + 1);

					return result;
				}
				else
				{
					return null;
				}
			});

			if (pipe.Type.HasFlag(PipeType.Asynchronous) && !Config.HasFlag(EngineConfigOptions.DisableAsync))
			{
				var debugName = pipe.DebugName;

				var threadName = string.Format(CultureInfo.InvariantCulture, "AsyncPipe: {0} Depth: {1} ", !string.IsNullOrEmpty(debugName) ? pipe.DebugName : pipe.ToString(), depth);

				var threadNameWithThreadID = !string.IsNullOrEmpty(ID)
					? string.Format(CultureInfo.InvariantCulture, "ID: [{0}] ", ID) + threadName
					: threadName;

				resultSet.AddTask(asyncStrategy.GetAsync(action, threadName: threadNameWithThreadID, cancellationTokenSource: resultSet.CancellationTokenSource)
					.ContinueWith(t =>
					{
						if (t.IsFaulted)
						{
							_ = t.Exception; // Eating the exception because we're already reporting non-critical exceptions in the previous task.
							return null;
						}
						else
						{
							return t.Result;
						}
					}));
			}
			else
			{
				dispatcherStrategy.Dispatch(action);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1024:BadConcurrentCollectionAccessRule", Justification = "Lock used on parent of concurrent collection")]
		static bool TryExecuteHandoverClaims(EngineExecutionResultSet resultSet, IPipe pipe, IEnumerable<IPipeHandoverWrapper> handovers)
		{
			if (!handovers.Any())
			{
				return true;
			}
			else
			{
				// This should be the only lock. It is necessary to prevent deadlocks between handovers which should be the only dynamic locking mechanism.
				lock (resultSet)
				{
					if (handovers.Any(h => resultSet.Handovers.ContainsKey(h.Key)))
					{
						return false;
					}
					else
					{
						foreach (var handover in handovers)
						{
							object value;

							if (!resultSet.Handovers.TryAdd(handover.Key, handover))
							{
								throw new InvalidOperationException("There is a logic error. This handover is already there.");
							}
							else if (resultSet.Cache.TryGetInput(pipe, handover, out value))
							{
								handover.OnClaim(value);
							}
						}

						return true;
					}
				}
			}
		}

		static void ExecuteHandoverReleases(EngineExecutionResultSet resultSet, IPipe pipe, IEnumerable<IPipeHandoverWrapper> handovers)
		{
			foreach (var handover in handovers)
			{
				object value;

				if (resultSet.Cache.TryGetInput(pipe, handover, out value))
				{
					handover.OnRelease(value);
				}
				IPipeHandoverWrapper other;
				if (resultSet.Handovers.TryRemove(handover.Key, out other) && other != handover)
				{
					throw new InvalidOperationException("There is a logic error. Trying to remove a different handover to the one currently locking.");
				}
			}
		}

		#endregion

		#region Pipe Getters

		public IEnumerable<IPipe> AllPipes
		{
			get { return Graph.PipesByKey.Values; }
		}

		IEnumerable<IPipe> GetInitialPipes()
		{
			return Graph.Roots().Select(key => Graph.PipesByKey[key]);
		}

		IEnumerable<IPipe> GetStartableDependentPipes(EngineExecutionResultSet resultSet, IPipeDataSource pipe)
		{
			IEnumerable<SEdge<Guid>> outEdges;
			if (Graph.TryGetOutEdges(pipe.Key, out outEdges))
			{
				foreach (var edge in outEdges)
				{
					var dependentPipe = Graph.PipesByKey[edge.Target];
					if (resultSet.Cache.IsStartable(dependentPipe))
					{
						yield return dependentPipe;
					}
				}
			}
		}

		#endregion
	}
}
