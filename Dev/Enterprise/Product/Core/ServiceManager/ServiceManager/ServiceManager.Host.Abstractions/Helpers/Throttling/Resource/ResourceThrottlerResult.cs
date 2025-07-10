using System;
using System.Collections.Generic;

namespace ServiceManager.Host.Abstractions
{
	public readonly struct ResourceThrottlerResult
	{
		[Flags]
		public enum ResourceThrottlerResults
		{
			NoResourceContention = 0,
			CpuContention = 1,
			DiskContention = 2,
			PageFileContention = 4,
		}

		public ResourceThrottlerResult(bool timedOut, ResourceThrottlerResults waitType, TimeSpan timeForWait, decimal cpu, decimal diskQueueLength, decimal pageFile)
		{
			TimedOut = timedOut;
			Cpu = cpu;
			DiskQueueLength = diskQueueLength;
			WaitType = waitType;
			TimeForWait = timeForWait;
			PageFile = pageFile;
		}

		public decimal Cpu { get; }
		public decimal DiskQueueLength { get; }
		public decimal PageFile { get; }
		public bool TimedOut { get; }
		public TimeSpan TimeForWait { get; }
		public ResourceThrottlerResults WaitType { get; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1068:Do Not Use Math.Round", Justification = "PRC is CW1 independent")]
		public override string ToString()
		{
			var list = new List<string>();
			if ((WaitType & ResourceThrottlerResults.CpuContention) == ResourceThrottlerResults.CpuContention)
			{
				list.Add($"CPU = {Math.Round(Cpu, 0, MidpointRounding.AwayFromZero):F0}");
			}

			if ((WaitType & ResourceThrottlerResults.DiskContention) == ResourceThrottlerResults.DiskContention)
			{
				list.Add($"Disk Queue Length = {Math.Round(DiskQueueLength, 2, MidpointRounding.AwayFromZero):F2}");
			}

			if ((WaitType & ResourceThrottlerResults.PageFileContention) == ResourceThrottlerResults.PageFileContention)
			{
				list.Add($"Page File Size = {Math.Round(PageFile, 2, MidpointRounding.AwayFromZero):F2} Mb");
			}

			return $"Waited for {TimeForWait}: {string.Join(", ", list)}";
		}
	}
}
