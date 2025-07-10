using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using CargoWise.Common;
using CargoWise.IO;
using Enterprise.ZArchitecture.Core;
using ServiceManager.Host.Abstractions;

namespace ServiceManager.Host.CW.Resource
{
	class ResourceThrottler : IResourceThrottler
	{
		public ResourceThrottler(IHostRegistrySettings hostRegistry)
		{
			this.hostRegistry = hostRegistry ?? throw new ArgumentNullException(nameof(hostRegistry));

			try
			{
				var openQueryResult = PdhApi.OpenQuery(null, IntPtr.Zero, out processorQueryHandle);
				if (openQueryResult == PdhStatus.CStatusValidData)
				{
					PdhApi.AddCounter(processorQueryHandle, @"\Processor(_Total)\% Processor Time", IntPtr.Zero, out processorCounterHandle);
				}
				openQueryResult = PdhApi.OpenQuery(null, IntPtr.Zero, out diskQueryHandle);
				if (openQueryResult == PdhStatus.CStatusValidData)
				{
					PdhApi.AddCounter(diskQueryHandle, @"\PhysicalDisk(_Total)\Avg. Disk Queue Length", IntPtr.Zero, out diskCounterHandle);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				// just ignore throttler if not working
			}
			lastWaitForResourceStopwatch = Stopwatch.StartNew();
		}

		void IDisposable.Dispose()
		{
			if (processorQueryHandle != null && !processorQueryHandle.IsClosed)
			{
				processorQueryHandle.Close();
			}
			if (diskQueryHandle != null && !diskQueryHandle.IsClosed)
			{
				diskQueryHandle.Close();
			}
			if (processorCounterHandle != null && !processorCounterHandle.IsClosed)
			{
				processorCounterHandle.Close();
			}
			if (diskCounterHandle != null && !diskCounterHandle.IsClosed)
			{
				diskCounterHandle.Close();
			}
		}

		public ResourceThrottlerResult WaitForResource()
		{
			if (processorCounterHandle == null
				|| processorQueryHandle == null
				|| diskCounterHandle == null
				|| diskQueryHandle == null
				|| lastWaitForResourceStopwatch.Elapsed < minimumTimeSpanBetweenNextValueCalls)
			{
				return new ResourceThrottlerResult(false, ResourceThrottlerResult.ResourceThrottlerResults.NoResourceContention, TimeSpan.Zero, -1, -1, -1);
			}

			var stopwatch = Stopwatch.StartNew();

			double maximumCpuValue = 0;
			double maximumDiskValue = 0;
			var minimumPageFileValue = ulong.MaxValue;

			var maxTimeToWait = hostRegistry.ServiceTaskMaxWaitForResourceAvailability;
			bool isTimeOut;
			ResourceThrottlerResult.ResourceThrottlerResults result;
			var worstResult = ResourceThrottlerResult.ResourceThrottlerResults.NoResourceContention;
			do
			{
				lastWaitForResourceStopwatch.Restart();
				result = ResourceThrottlerResult.ResourceThrottlerResults.NoResourceContention;

				try
				{
					var cpuValue = CpuCounter;
					maximumCpuValue = Math.Max(maximumCpuValue, cpuValue);
					if (cpuValue > hostRegistry.ServiceTaskMaximumCpuLoadBeforeThrottlingTasks)
					{
						result |= ResourceThrottlerResult.ResourceThrottlerResults.CpuContention;
					}

					var diskValue = DiskCounter;
					maximumDiskValue = Math.Max(maximumDiskValue, diskValue);
					if (diskValue > (float)hostRegistry.ServiceTaskMaximumDiskQueueLengthBeforeThrottlingTasks)
					{
						result |= ResourceThrottlerResult.ResourceThrottlerResults.DiskContention;
					}

					minimumPageFileValue = Math.Min(minimumPageFileValue, PageFile);
					if (minimumPageFileValue < ZSystemInformation.MinimumRequirements.AvailablePageFileInMB)
					{
						result |= ResourceThrottlerResult.ResourceThrottlerResults.PageFileContention;
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					// this is happening about once a month - Issue 00846859
					return new ResourceThrottlerResult(false, ResourceThrottlerResult.ResourceThrottlerResults.NoResourceContention, TimeSpan.Zero, -1, -1, -1);
				}

				if (result != ResourceThrottlerResult.ResourceThrottlerResults.NoResourceContention)
				{
					Thread.Sleep(minimumTimeSpanBetweenNextValueCalls);
				}

				isTimeOut = stopwatch.Elapsed > maxTimeToWait;
				worstResult |= result;
			} while (!isTimeOut && result != ResourceThrottlerResult.ResourceThrottlerResults.NoResourceContention);
			return new ResourceThrottlerResult(isTimeOut, worstResult, stopwatch.Elapsed, (decimal)maximumCpuValue, (decimal)maximumDiskValue, minimumPageFileValue);
		}

		readonly PdhQueryHandle processorQueryHandle, diskQueryHandle;
		readonly PdhCounterHandle processorCounterHandle, diskCounterHandle;
		readonly Stopwatch lastWaitForResourceStopwatch;
		readonly TimeSpan minimumTimeSpanBetweenNextValueCalls = TimeSpan.FromMilliseconds(1500);
		readonly IHostRegistrySettings hostRegistry;

		static double GetPerformanceCounterValue(PdhQueryHandle queryHandle, PdhCounterHandle counterHandle)
		{
			while (true)
			{
				var collectResult = PdhApi.CollectQueryData(queryHandle);
				if (collectResult != PdhStatus.CStatusValidData)
				{
					throw new Win32Exception($"The runtime performance counter returns no data.\r\nError Code: {collectResult}");
				}

				if (PdhApi.GetFormattedCounterValue(counterHandle, PdhCounterFormat.Double | PdhCounterFormat.NoCap100 | PdhCounterFormat.NoScale, IntPtr.Zero, out var counterValue) == PdhStatus.CStatusValidData
					&& counterValue.CStatus == PdhCounterStatus.CStatusValidData)
				{
					return counterValue.DoubleValue;
				}

				Thread.Sleep(1000);
			}
		}

		double CpuCounter => GetPerformanceCounterValue(processorQueryHandle, processorCounterHandle);
		double DiskCounter => GetPerformanceCounterValue(diskQueryHandle, diskCounterHandle);
		ulong PageFile => ZSystemInformation.Instance.AvailablePageFileSize;
	}
}
