using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace Enterprise.ZArchitecture.Benchmark.Framework
{
	public sealed class BenchmarkRunner(BenchmarkConfig config, MethodInfo testMethod)
	{
		public BenchmarkRunner(BenchmarkConfig config, Action testMethod)
			: this(config, testMethod.Method)
		{
		}

		public BenchmarkConfig Config { get; } = config;
		public MethodInfo TestMethod { get; } = testMethod;

		public BenchmarkResult Run(Func<object, object> oneIteration, Func<object> setup = null, Action<object, object> tearDown = null)
		{
			if (Config.TestContext.IsRunningOnDAT)
			{
				var param = setup?.Invoke();
				var iterationResult = oneIteration(param);
				if (iterationResult is BenchmarkException ex)
				{
					throw ex;
				}
				tearDown?.Invoke(param, iterationResult);
				return new BenchmarkResult(Config, TestMethod, [], TimeSpan.Zero, DateTimeOffset.MinValue);
			}
			return RunInternal(oneIteration, setup, tearDown);
		}

		BenchmarkResult RunInternal(Func<object, object> oneIteration, Func<object> setup, Action<object, object> tearDown)
		{
			var now = Config.Environment.GetLocalTime();

			// Short warm up.
			for (int i = 0; i < Config.WarmUpIterations; i++)
			{
				var param = setup?.Invoke();
				var iterationResult = oneIteration(param);
				if (iterationResult is BenchmarkException ex)
				{
					throw ex;
				}
				tearDown?.Invoke(param, iterationResult);
			}

			var timings = new List<BenchmarkTiming>(65_536);
			int counter = 0;
			var overallRuntime = Stopwatch.StartNew();
			var benchmarkRuntime = TimeSpan.Zero;
			while (counter < Config.MinimumRuns || benchmarkRuntime < Config.TargetTime)
			{
				var param = setup?.Invoke();			// Setup is not timed.

				var sw = new Stopwatch();
				var runBenchmark = RunForSingleInvocation;
				if (Config.ExecutionsPerIteration > 1)
				{
					runBenchmark = RunForManyInvocations;
				}
				var iterationResult = runBenchmark(oneIteration, sw, param);     // Timing is in here.
				benchmarkRuntime += sw.Elapsed;

				tearDown?.Invoke(param, iterationResult);		// Tear down is not timed.

				if (iterationResult is BenchmarkException ex)
				{
					throw ex;
				}

				// Record result
				var highPrecisionTiming = sw.Elapsed.ToHighPrecision();
				if (Config.ExecutionsPerIteration > 1)
				{
					highPrecisionTiming = new HighPrecisionTimeSpan(highPrecisionTiming.Ticks / (ulong)Config.ExecutionsPerIteration);
				}
				timings.Add(new BenchmarkTiming(counter, highPrecisionTiming));
				++counter;
			}
			timings.Sort((x, y) => x.Duration.CompareTo(y.Duration));
			overallRuntime.Stop();

			var result = new BenchmarkResult(Config, TestMethod, timings, overallRuntime.Elapsed, now);

			// Save to temp folder
			if (!Config.TestContext.IsRunningOnDAT)
			{
				result.SaveResultsToTempFolder();
			}
			return result;
		}

		object RunForSingleInvocation(Func<object, object> oneIteration, Stopwatch sw, object param)
		{
			sw.Start();
			var result = oneIteration(param);
			sw.Stop();
			return result;
		}

		object RunForManyInvocations(Func<object, object> oneIteration, Stopwatch sw, object param)
		{
			object result = null;
			var count = Config.ExecutionsPerIteration;
			sw.Start();
			for (int i = 0; i < count; i++)
			{
				result = oneIteration(param);
			}
			sw.Stop();
			return result;
		}
	}
}
