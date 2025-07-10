using System;
using Enterprise.ZArchitecture.Benchmark.Framework.TestInterfaces;

namespace Enterprise.ZArchitecture.Benchmark.Framework
{
	public sealed class BenchmarkConfig
	{
		public static BenchmarkConfig Default() => new BenchmarkConfig();

		/// <summary>
		/// Try to run the benchmark for approximately this long.
		/// </summary>
		public TimeSpan TargetTime { get; private set; } = TimeSpan.FromSeconds(15);

		/// <summary>
		/// Must record at least this many iterations. Even if TargetTime is reached.
		/// </summary>
		public int MinimumRuns { get; private set; } = 1024;

		/// <summary>
		/// Number of times the benchmark function is executed per iteration.
		/// Should only be increased for microbenchmarks which take less than 1 tick (100ns) to run.
		/// </summary>
		public int ExecutionsPerIteration { get; private set; } = 1;

		/// <summary>
		/// Number of times the benchmark function is executed before beginning any timings.
		/// To allow caches, JIT, etc to warm up.
		/// </summary>
		public int WarmUpIterations { get; private set; } = 32;

		/// <summary>
		/// When true, the benchmark results are saved to %TEMP%/CargoWiseBenchmarkResults. Otherwise, no results are saved.
		/// </summary>
		public bool SaveResultsToTempFolder { get; private set; } = true;

		/// <summary>
		/// Which implementation of ITestContext to use.
		/// Defaults to NUnit1TestContext, but test results are logged as warnings with NUnit4, which is less ugly.
		/// </summary>
		public ITestContext TestContext { get; private set; } = new NUnit1TestContext();

		/// <summary>
		/// Which implementation of ILocalEnvironment to use.
		/// This is chosen automatically based on if the test is running on DAT or not.
		/// </summary>
		public ILocalEnvironment Environment
		{
			get => EnvironmentInstance ??=
						TestContext.IsRunningOnDAT
							? new NullEnvironment()
							: new LocalEnvironment();
			private set
			{
				EnvironmentInstance = value;
			}
		}
		ILocalEnvironment EnvironmentInstance;

		public BenchmarkConfig WithTargetTime(TimeSpan targetTime)
		{
			TargetTime = targetTime;
			return this;
		}

		public BenchmarkConfig WithMinimumRuns(int minimumRuns)
		{
			MinimumRuns = minimumRuns;
			return this;
		}

		public BenchmarkConfig WithExecutionsPerIteration(int executionsPerIteration)
		{
			ExecutionsPerIteration = executionsPerIteration;
			return this;
		}

		public BenchmarkConfig WithWarmUpIterations(int warmUpIterations)
		{
			WarmUpIterations = warmUpIterations;
			return this;
		}

		public BenchmarkConfig WithSaveResultsToTempFolder(bool saveResultsToTempFolder)
		{
			SaveResultsToTempFolder = saveResultsToTempFolder;
			return this;
		}

		public BenchmarkConfig WithTestContext(ITestContext testContext)
		{
			TestContext = testContext;
			return this;
		}

		public BenchmarkConfig WithEnvironment(ILocalEnvironment environment)
		{
			Environment = environment;
			return this;
		}

		public BenchmarkConfig WithNUnit1TestContext()
			=> WithTestContext(new NUnit1TestContext());

		public BenchmarkConfig WithNUnit4TestContext()
			=> WithTestContext(new NUnit4TestContext());

		public BenchmarkConfig WithNoDatabaseEnvironment()
			=> WithEnvironment(new NoDatabaseLocalEnvironment());
	}
}
