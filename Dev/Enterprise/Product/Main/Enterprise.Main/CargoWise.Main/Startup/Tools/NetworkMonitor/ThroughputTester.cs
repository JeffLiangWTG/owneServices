using System.Diagnostics;
using Enterprise.RemoteDesktopServices;
using Enterprise.RemoteDesktopServices.Server;

namespace Enterprise.Startup.Tools
{
	public static class ThroughputTester
	{
		public static ThroughputTestResult TestThroughput(int dataSizeInKB)
		{
			var stopwatch = new Stopwatch();

			stopwatch.Start();
			EnterpriseChannel.Instance.SendMessage<bool>(EnterpriseChannelMessageTypes.CheckNetworkRoundLoopDelay, new byte[dataSizeInKB * 1024]);
			stopwatch.Stop();
			var downlinkTimeConsumedInMilliseconds = stopwatch.Elapsed.TotalMilliseconds;

			stopwatch.Restart();
			EnterpriseChannel.Instance.SendMessage<int, byte[]>(EnterpriseChannelMessageTypes.UpLinkThroughputTest, dataSizeInKB);
			stopwatch.Stop();
			var uplinkTimeConsumedInMilliseconds = stopwatch.Elapsed.TotalMilliseconds;

			return new ThroughputTestResult(dataSizeInKB, uplinkTimeConsumedInMilliseconds, downlinkTimeConsumedInMilliseconds);
		}
	}

	public class ThroughputTestResult
	{
		public ThroughputTestResult(int dataSize, double uplinkTimeConsumed, double downlinkTimeConsumed)
		{
			FileSizeInKB = dataSize;
			UplinkTimeConsumed = uplinkTimeConsumed;
			DownlinkTimeConsumed = downlinkTimeConsumed;
		}

		public double UplinkSpeed => FileSizeInKB / UplinkTimeConsumed;
		public double UplinkTimeConsumed { get; }
		public double DownlinkSpeed => FileSizeInKB / DownlinkTimeConsumed;
		public double DownlinkTimeConsumed { get; }
		public int FileSizeInKB { get; }
	}
}
