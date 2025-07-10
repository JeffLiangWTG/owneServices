using System;

namespace Enterprise.ZArchitecture.Benchmark.Framework
{
	/// <summary>
	/// A version of TimeSpan where 1 Tick = 1 picosecond. TimeSpan uses 100ns ticks, which is insufficient for some micro benchmarks.
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "100ns isn't good enough for micro benchmarks that complete in sub-nanosecond time!")]
	public readonly struct HighPrecisionTimeSpan : IEquatable<HighPrecisionTimeSpan>, IComparable<HighPrecisionTimeSpan>
	{
		// 🚩🚩🚩
		// This has incidental test coverage only.
		// It is sufficient for BenchmarkRunner / BenchmarkResult, but won't be a full replacement for TimeSpan.
		// 🚩🚩🚩

		const ulong TicksPerPicoSecond = 1;
		const ulong TicksPerNanoSecond = TicksPerPicoSecond * 1000;
		const ulong TicksPerMicroSecond = TicksPerNanoSecond * 1000;
		const ulong TicksPerMilliSecond = TicksPerMicroSecond * 1000;
		const ulong TicksPerSecond = TicksPerMilliSecond * 1000;
		const ulong TicksPerMinute = TicksPerSecond * 60;
		const ulong TicksPerHour = TicksPerMinute * 60;

		const ulong TicksPerTimeSpanTick = TicksPerNanoSecond * 100;  // TimeSpan ticks are 100ns

		public const string FormatTitle = "mm:ss.mmmuuunnnppp";

		public static readonly HighPrecisionTimeSpan Zero = new HighPrecisionTimeSpan(0);

		public readonly ulong Ticks { get; }
		public decimal TicksAsDecimal => Ticks;
		public double TicksAsDouble => Ticks;

		public decimal TotalPicoSeconds => TicksAsDecimal;
		public decimal TotalNanoSeconds => TicksAsDecimal / TicksPerNanoSecond;
		public decimal TotalMicroSeconds => TicksAsDecimal / TicksPerMicroSecond;
		public decimal TotalMilliSeconds => TicksAsDecimal / TicksPerMilliSecond;
		public decimal TotalSeconds => TicksAsDecimal / TicksPerSecond;
		public decimal TotalMinutes => TicksAsDecimal / TicksPerMinute;

		public HighPrecisionTimeSpan(ulong ticks)
		{
			if (ticks > TicksPerHour)
			{
				throw new ArgumentOutOfRangeException(nameof(ticks), "HighPrecisionTimeSpan only supports duration of up to 1 hour.");
			}
			Ticks = ticks;
		}

		public static HighPrecisionTimeSpan FromTimeSpan(TimeSpan timeSpan)
		{
			if (timeSpan.Ticks < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(timeSpan), "Negative durations are not allowed.");
			}
			return new HighPrecisionTimeSpan((ulong)timeSpan.Ticks * TicksPerTimeSpanTick);
		}

		public static HighPrecisionTimeSpan FromMilliseconds(double milliseconds)
		{
			if (milliseconds < 0.0)
			{
				throw new ArgumentOutOfRangeException(nameof(milliseconds), "Negative durations are not allowed.");
			}
			return new HighPrecisionTimeSpan((ulong)(milliseconds * TicksPerMilliSecond));
		}

		public static HighPrecisionTimeSpan FromSeconds(double seconds)
		{
			if (seconds < 0.0)
			{
				throw new ArgumentOutOfRangeException(nameof(seconds), "Negative durations are not allowed.");
			}
			return new HighPrecisionTimeSpan((ulong)(seconds * TicksPerSecond));
		}

		public override string ToString()
		{
			var mins = Math.DivRem((long)Ticks, (long)TicksPerMinute, out var remainder);
			var secs = Math.DivRem(remainder, (long)TicksPerSecond, out var residual);
			return $"{mins:00}:{secs:00}.{residual:000000000000}";
		}

		public string ToHumanReadableString()
		{
			if (Ticks < TicksPerNanoSecond)
			{
				return $"{Ticks:000}ps";
			}
			else if (Ticks < TicksPerMicroSecond)
			{
				return $"{TotalNanoSeconds:000.00}ns";
			}
			else if (Ticks < TicksPerMilliSecond)
			{
				return $"{TotalMicroSeconds:000.00}us";
			}
			else if (Ticks < TicksPerSecond)
			{
				return $"{TotalMilliSeconds:000.00}ms";
			}
			else if (Ticks < TicksPerMinute)
			{
				return $"{TotalSeconds:000.00}s";
			}
			else
			{
				var mins = Math.DivRem((long)Ticks, (long)TicksPerMinute, out var remainder);
				var secs = Math.DivRem(remainder, (long)TicksPerSecond, out var _);
				return $"{mins}:{secs}";
			}
		}

		public override bool Equals(object other)
			=> other is HighPrecisionTimeSpan x && Equals(x);

		public bool Equals(HighPrecisionTimeSpan other)
			=> Ticks == other.Ticks;

		public override int GetHashCode()
			=> Ticks.GetHashCode();

		public int CompareTo(HighPrecisionTimeSpan other)
			=> Ticks.CompareTo(other.Ticks);

		public TimeSpan ToTimeSpan()
			=> TimeSpan.FromTicks((long)(Ticks / TicksPerTimeSpanTick));
	}

	static class TimeSpanExtensions
	{
		public static HighPrecisionTimeSpan ToHighPrecision(this TimeSpan timeSpan)
			=> HighPrecisionTimeSpan.FromTimeSpan(timeSpan);
	}
}
