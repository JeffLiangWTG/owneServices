using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Enterprise.ZArchitecture.Benchmark.Framework
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Only visible to developers; no translation required.")]
	public sealed class BenchmarkResult
	(
		BenchmarkConfig config,
		MethodInfo testMethod,
		IReadOnlyList<BenchmarkTiming> timings,
		TimeSpan totalRuntime,
		DateTimeOffset runAt
	)
	{
		public readonly Uri WikiLink = new Uri("https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki/16081/Micro-Benchmark-Framework");

		public BenchmarkConfig Config { get; } = config;
		public MethodInfo TestMethod { get; } = testMethod;
		public IReadOnlyList<BenchmarkTiming> Timings { get; } = timings;
		public TimeSpan TotalRuntime { get; } = totalRuntime;
		public DateTimeOffset RunAt { get; } = runAt;

		string Summary;
		string SummarySavedToPath;
		string AggregateSavedToPath;

		const string SpreadsheetHeaderLine = "Class\tTest\tDate Run\tDateTime Run\tWho Ran\tMean (ms)\tMedian (ms)\tOps/sec\tRuntime";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "EDI011:Temp Path Rule", Justification = "For use on developer machines and unit tests only.")]
		internal void SaveResultsToTempFolder()
		{
			var dir = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "CargoWiseBenchmarkResults"));
			var summaryFilename = $"{TestMethod.DeclaringType.Name}.{TestMethod.Name}.{Config.Environment.RuntimeVersion}.txt";
			SummarySavedToPath = Path.Combine(dir.FullName, summaryFilename);
			AggregateSavedToPath = Path.Combine(dir.FullName, "!TestRuns.txt");
			var aggregatedFile = new FileInfo(AggregateSavedToPath);
			if (!aggregatedFile.Exists)
			{
				// Yes, there is a time-of-check time-of-use condition here. But we never run tests in parallel for CargoWise.
				File.WriteAllText(AggregateSavedToPath, SpreadsheetHeaderLine + Environment.NewLine);
			}
			File.AppendAllText(AggregateSavedToPath, GetSpreadsheetLine() + Environment.NewLine);

			var summary = GetSummary();  // Summary uses the paths; must create summary after setting SummarySavedToPath and AggregateSavedToPath.
			File.WriteAllText(SummarySavedToPath, summary);

			var v = Environment.Version;
		}

		public string GetSummary()
		{
			if (Summary != null)
			{
				return Summary;
			}

			var localEnvironment = Config.Environment;
			var cpu = localEnvironment.GetCPUDetails();

			Summary =
				$"""
				** Summary **
				{TestMethod.DeclaringType.Namespace}.{TestMethod.DeclaringType.Name}.{TestMethod.Name}
				Run At:    {RunAt:yyyy-MM-dd HH:mm:ss zzz}
				Run By:    {localEnvironment.Username}
				Saved To:  {SummarySavedToPath ?? "Was not saved; set BenchmarkConfig.SaveResultsToTempFolder to save summary to temp folder."}
				Aggregate: {AggregateSavedToPath ?? "Was not saved; set BenchmarkConfig.SaveResultsToTempFolder to save aggregate data to temp folder."}
				See also:  {WikiLink}

				         {HighPrecisionTimeSpan.FormatTitle}
				Median:  {Percentile(0.5d)} | {Percentile(0.5d).ToHumanReadableString()}
				Mean:    {Mean()} | {Mean().ToHumanReadableString()}
				Ops/Sec: {AverageBenchmarkRunsPerSecond():N2}

				** For Spreadsheets **
				{SpreadsheetHeaderLine}
				{GetSpreadsheetLine()}

				** General Stats **
				Time Running Benchmark Code: {TotalDuration().ToTimeSpan()}
				Target Run Time:             {Config.TargetTime}
				Total Run Time:              {TotalRuntime}
				Overhead Time:               {TotalRuntime - TotalDuration().ToTimeSpan()}
				Total Iterations:            {Timings.Count:N0}
				Batch Size:                  {Config.ExecutionsPerIteration}
				Total Benchmark Executions:  {TotalBenchmarkMethodRuns():N0}

				** Percentiles **
				       {HighPrecisionTimeSpan.FormatTitle}
				Max:   {Max()} | {Max().ToHumanReadableString()}
				P99.9: {Percentile(0.999)} | {Percentile(0.999).ToHumanReadableString()}
				P99:   {Percentile(0.99)} | {Percentile(0.99).ToHumanReadableString()}
				P95:   {Percentile(0.95)} | {Percentile(0.95).ToHumanReadableString()}
				P90:   {Percentile(0.90)} | {Percentile(0.90).ToHumanReadableString()}
				P75:   {Percentile(0.75)} | {Percentile(0.75).ToHumanReadableString()}
				P50:   {Percentile(0.50)} | {Percentile(0.50).ToHumanReadableString()}
				P25:   {Percentile(0.25)} | {Percentile(0.25).ToHumanReadableString()}
				P05:   {Percentile(0.05)} | {Percentile(0.05).ToHumanReadableString()}
				P01:   {Percentile(0.01)} | {Percentile(0.01).ToHumanReadableString()}
				Min:   {Min()} | {Min().ToHumanReadableString()}

				** Environment **
				Computer Name: {localEnvironment.MachineName}
				CPU Name:      {cpu.name}
				CPU Sockets:   {cpu.physicalCPUs}
				CPU Cores:     {cpu.physicalCores}
				Logical CPUs:  {cpu.logicalCores}
				Pointer Size:  {IntPtr.Size} bytes
				Power Profile: {localEnvironment.PowerProfileName}
				Runtime:       {localEnvironment.RuntimeVersion}
				SQL Server:    {localEnvironment.DatabaseServerName}
				SQL Version:
				  {localEnvironment.DatabaseFullVersionText?.Replace("\n", "\n  ")?.Trim()}
				""";

			return Summary;
		}

		string GetSpreadsheetLine()
			=> $"{TestMethod.DeclaringType.Name}\t{TestMethod.Name}\t{RunAt:yyyy-MM-dd}\t{RunAt:yyyy-MM-dd HH:mm:ss zzz}\t{Config.Environment.Username}\t{Mean().TotalMilliSeconds:N12}\t{Percentile(0.5d).TotalMilliSeconds:N12}\t{AverageBenchmarkRunsPerSecond():N2}\t{Config.Environment.RuntimeVersion}";

		HighPrecisionTimeSpan Percentile(double percentialRatio)
		{
			if (Timings.Count == 0)
			{
				return HighPrecisionTimeSpan.Zero;
			}

			// https://stackoverflow.com/a/8137455
			var n = (Timings.Count - 1) * percentialRatio + 1;
			if (n == 1d)
			{
				return Timings[0].Duration;
			}
			else if (n == Timings.Count)
			{
				return Timings[Timings.Count - 1].Duration;
			}
			else
			{
				var k = (int)n;
				var distance = n - k;
				var interpolatedDuration =
					Timings[k - 1].Duration.Ticks +
					(ulong)(distance * (Timings[k].Duration.Ticks - Timings[k - 1].Duration.Ticks));
				return new HighPrecisionTimeSpan(interpolatedDuration);
			}
		}

		HighPrecisionTimeSpan TotalDuration()
			=> new HighPrecisionTimeSpan((ulong)Timings.Sum(t => t.Duration.TicksAsDecimal * Config.ExecutionsPerIteration));

		long TotalBenchmarkMethodRuns()
			=> (long)Timings.Count * Config.ExecutionsPerIteration;

		decimal AverageBenchmarkRunsPerSecond()
			=> TotalDuration().TotalSeconds == 0m
				? 0m
				: TotalBenchmarkMethodRuns() / TotalDuration().TotalSeconds;

		HighPrecisionTimeSpan Mean()
			=> Timings.Count == 0
				? HighPrecisionTimeSpan.Zero
				: new HighPrecisionTimeSpan((ulong)Timings.Average(t => t.Duration.TicksAsDecimal));

		HighPrecisionTimeSpan Max()
			=> Timings.Count == 0
				? HighPrecisionTimeSpan.Zero
				: new HighPrecisionTimeSpan((ulong)Timings.Max(t => t.Duration.TicksAsDecimal));

		HighPrecisionTimeSpan Min()
			=> Timings.Count == 0
				? HighPrecisionTimeSpan.Zero
				: new HighPrecisionTimeSpan((ulong)Timings.Min(t => t.Duration.TicksAsDecimal));
	}

	public sealed class BenchmarkTiming(int ordinal, HighPrecisionTimeSpan duration)
	{
		public int Ordinal { get; } = ordinal;
		public HighPrecisionTimeSpan Duration { get; } = duration;
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Will only be run in unit tests. No translation is required.")]
	public static class BenchmarkResultExtensions
	{
		public static void ReportSummaryOnLocal(this BenchmarkResult result)
		{
			var testContext = result.Config.TestContext;
			if (testContext.IsRunningOnDAT)
			{
				testContext.PassWithMessage("Performance tests only run on DAT to ensure correctness; benchmark figures are not gathered or asserted.");
				return;
			}

			var message = $"""
				ℹ️ Important: this test never fails on DAT, and always fails locally (so you can see the numbers below).

				-----

				{result.GetSummary()}
				""";
			testContext.ReportTestResults(message);
		}
	}
}
