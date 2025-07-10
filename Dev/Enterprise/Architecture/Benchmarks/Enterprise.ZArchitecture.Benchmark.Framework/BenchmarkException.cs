using System;

namespace Enterprise.ZArchitecture.Benchmark.Framework
{
	/// <summary>
	/// Represents a pre or post condition failure in a benchmark.
	/// Avoid throwing this from the benchmark method, because it will affect the JITs optimizations.
	/// </summary>
	public sealed class BenchmarkException : Exception
	{
		public BenchmarkException() : base()
		{
		}
		public BenchmarkException(string message) : base(message)
		{
		}
		public BenchmarkException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}
