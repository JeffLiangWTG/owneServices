using CargoWise.Data.SqlServer;
using CargoWise.Data.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Diagnostics.Testing
{
	class CdcLatencyProviderTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestGetCdcLatency()
		{
			var latencyProvider = new CdcLatencyProviderForTest();
			var backLog = latencyProvider.GetCurrentBacklog();
			Assert(backLog.BacklogResult.BacklogSize > latencyProvider.AcceptableBacklog);
			Assert("Cdc latency provider return backlog succeed.", backLog.Success);
			AssertEquals("Cdc Failure Reason.", string.Empty, backLog.FailureReason);
			AssertEquals("Cdc backlog description contains latency.", $"Cdc transactions queued:{cdcLatency[0]}", backLog.BacklogResult.BacklogDescription);

			backLog = latencyProvider.GetCurrentBacklog();
			Assert(backLog.BacklogResult.BacklogSize <= latencyProvider.AcceptableBacklog);
			Assert("Cdc latency provider return backlog succeed.", backLog.Success);
			AssertEquals("Cdc Failure Reason.", string.Empty, backLog.FailureReason);
			AssertEquals("Cdc backlog description contains latency.", $"Cdc transactions queued:{cdcLatency[1]}", backLog.BacklogResult.BacklogDescription);
		}

		static readonly int[] cdcLatency = new int[] { 100000, 3000, };

		class CdcLatencyProviderForTest : CdcLatencyProvider
		{
			int times;

			internal override int Duration =>
				times >= cdcLatency.Length ?
					cdcLatency[cdcLatency.Length - 1] : cdcLatency[times++];
		}
	}
}
