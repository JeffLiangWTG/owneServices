using System;
using System.Linq;
using System.Net;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class IPAddressRangeTest : TestCase
	{
		public void TestSingleIp()
		{
			var range = new IPAddressRange("203.62.211.6");
			AssertEquals(true, range.IsInRange(IPAddress.Parse("203.62.211.6")));
			AssertEquals(false, range.IsInRange(IPAddress.Parse("203.62.211.5")));
			AssertEquals(false, range.IsInRange(IPAddress.Parse("203.62.211.7")));
			AssertEquals(false, range.IsInRange(IPAddress.Parse("203.62.210.6")));
			AssertEquals(false, range.IsInRange(IPAddress.Parse("74.125.237.3")));
		}

		public void TestRangeSanity()
		{
			var ranges = IPAddressRanges.Parse("203.62.211.4/30");
			AssertEquals(true, ranges.IsInRange(IPAddress.Parse("203.62.211.6")));
			AssertEquals(true, ranges.IsInRange(IPAddress.Parse("203.62.211.5")));
			AssertEquals(false, ranges.IsInRange(IPAddress.Parse("203.62.210.6")));
			AssertEquals(false, ranges.IsInRange(IPAddress.Parse("74.125.237.3")));
		}

		public void TestMultipeleRanges()
		{
			var ranges = IPAddressRanges.Parse("203.62.211.4/30,74.125.237.0/30");
			AssertEquals(true, ranges.IsInRange(IPAddress.Parse("203.62.211.6")));
			AssertEquals(true, ranges.IsInRange(IPAddress.Parse("74.125.237.3")));
			AssertEquals(false, ranges.IsInRange(IPAddress.Parse("134.170.188.221")));

			ranges = IPAddressRanges.Parse("203.62.211.4/30 , 74.125.237.0/30");
			AssertEquals(true, ranges.IsInRange(IPAddress.Parse("203.62.211.6")));
			AssertEquals(true, ranges.IsInRange(IPAddress.Parse("74.125.237.3")));
			AssertEquals(false, ranges.IsInRange(IPAddress.Parse("134.170.188.221")));

			ranges = IPAddressRanges.Parse("203.62.211.4/30;74.125.237.0/30");
			AssertEquals(true, ranges.IsInRange(IPAddress.Parse("203.62.211.6")));
			AssertEquals(true, ranges.IsInRange(IPAddress.Parse("74.125.237.3")));
			AssertEquals(false, ranges.IsInRange(IPAddress.Parse("134.170.188.221")));
		}

		// test against subnet tables described here: http://www.ietf.org/rfc/rfc1878.txt

		public void TestSubnetTable30()
		{
			TestSubnetTable(30,
@"                           N.N.N.0     N.N.N.1-2        N.N.N.3
                             N.N.N.4     N.N.N.5-6        N.N.N.7
                             N.N.N.8     N.N.N.9-10       N.N.N.11
                             N.N.N.244   N.N.N.245-246    N.N.N.247
                             N.N.N.248   N.N.N.249-250    N.N.N.251
                             N.N.N.252   N.N.N.253-254    N.N.N.255");
		}

		public void TestSubnetTable29()
		{
			TestSubnetTable(29,
@"                           N.N.N.0     N.N.N.1-6        N.N.N.7
                             N.N.N.8     N.N.N.9-14       N.N.N.15
                             N.N.N.16    N.N.N.17-22      N.N.N.23
                             N.N.N.24    N.N.N.25-30      N.N.N.31
                             N.N.N.32    N.N.N.33-38      N.N.N.39
                             N.N.N.40    N.N.N.41-46      N.N.N.47
                             N.N.N.48    N.N.N.49-54      N.N.N.55
                             N.N.N.56    N.N.N.57-62      N.N.N.63
                             N.N.N.64    N.N.N.65-70      N.N.N.71
                             N.N.N.72    N.N.N.73-78      N.N.N.79
                             N.N.N.80    N.N.N.81-86      N.N.N.87
                             N.N.N.88    N.N.N.89-94      N.N.N.95
                             N.N.N.96    N.N.N.97-102     N.N.N.103
                             N.N.N.104   N.N.N.105-110    N.N.N.111
                             N.N.N.112   N.N.N.113-118    N.N.N.119
                             N.N.N.120   N.N.N.121-126    N.N.N.127
                             N.N.N.128   N.N.N.129-134    N.N.N.135
                             N.N.N.136   N.N.N.137-142    N.N.N.143
                             N.N.N.144   N.N.N.145-150    N.N.N.151
                             N.N.N.152   N.N.N.153-158    N.N.N.159
                             N.N.N.160   N.N.N.161-166    N.N.N.167
                             N.N.N.168   N.N.N.169-174    N.N.N.175
                             N.N.N.176   N.N.N.177-182    N.N.N.183
                             N.N.N.184   N.N.N.185-190    N.N.N.191
                             N.N.N.192   N.N.N.193-198    N.N.N.199
                             N.N.N.200   N.N.N.201-206    N.N.N.207
                             N.N.N.208   N.N.N.209-214    N.N.N.215
                             N.N.N.216   N.N.N.217-222    N.N.N.223
                             N.N.N.224   N.N.N.225-230    N.N.N.231
                             N.N.N.232   N.N.N.233-238    N.N.N.239
                             N.N.N.240   N.N.N.241-246    N.N.N.247
                             N.N.N.248   N.N.N.249-254    N.N.N.255");
		}

		public void TestSubnetTable28()
		{
			TestSubnetTable(28,
@"                           N.N.N.0     N.N.N.1-14       N.N.N.15
                             N.N.N.16    N.N.N.17-30      N.N.N.31
                             N.N.N.32    N.N.N.33-46      N.N.N.47
                             N.N.N.48    N.N.N.49-62      N.N.N.63
                             N.N.N.64    N.N.N.65-78      N.N.N.79
                             N.N.N.80    N.N.N.81-94      N.N.N.95
                             N.N.N.96    N.N.N.97-110     N.N.N.111
                             N.N.N.112   N.N.N.113-126    N.N.N.127
                             N.N.N.128   N.N.N.129-142    N.N.N.143
                             N.N.N.144   N.N.N.145-158    N.N.N.159
                             N.N.N.160   N.N.N.161-174    N.N.N.175
                             N.N.N.176   N.N.N.177-190    N.N.N.191
                             N.N.N.192   N.N.N.193-206    N.N.N.207
                             N.N.N.208   N.N.N.209-222    N.N.N.223
                             N.N.N.224   N.N.N.225-238    N.N.N.239
                             N.N.N.240   N.N.N.241-254    N.N.N.255");
		}

		public void TestSubnetTable27()
		{
			TestSubnetTable(27,
@"                           N.N.N.0     N.N.N.1-30       N.N.N.31
                             N.N.N.32    N.N.N.33-62      N.N.N.63
                             N.N.N.64    N.N.N.65-94      N.N.N.95
                             N.N.N.96    N.N.N.97-126     N.N.N.127
                             N.N.N.128   N.N.N.129-158    N.N.N.159
                             N.N.N.160   N.N.N.161-190    N.N.N.191
                             N.N.N.192   N.N.N.193-222    N.N.N.223
                             N.N.N.224   N.N.N.225-254    N.N.N.255");
		}

		public void TestSubnetTable26()
		{
			TestSubnetTable(26,
@"                           N.N.N.0     N.N.N.1-62       N.N.N.63
                             N.N.N.64    N.N.N.65-126     N.N.N.127
                             N.N.N.128   N.N.N.129-190    N.N.N.191
                             N.N.N.192   N.N.N.193-254    N.N.N.255");
		}

		public void TestSubnetTable25()
		{
			TestSubnetTable(25,
@"                           N.N.N.0     N.N.N.1-126      N.N.N.127
                             N.N.N.128   N.N.N.129-254    N.N.N.255");
		}

		public void TestSubnetTable24()
		{
			TestSubnetTable(24,
@"                            N.N.0.0     N.N.0.1-254          N.N.0.255
                              N.N.1.0     N.N.1.1-254          N.N.1.255
                              N.N.252.0   N.N.252.1-254        N.N.252.255
                              N.N.253.0   N.N.253.1-254        N.N.253.255
                              N.N.254.0   N.N.254.1-254        N.N.254.255");
		}

		public void TestSubnetTable23()
		{
			TestSubnetTable(23,
@"                            N.N.0.0     N.N.0-1.10        N.N.1.255
                              N.N.2.0     N.N.2-3.10        N.N.3.255
                              N.N.4.0     N.N.4-5.10        N.N.5.255
                              N.N.250.0   N.N.250-251.10    N.N.251.255
                              N.N.252.0   N.N.252-253.10    N.N.253.255
                              N.N.254.0   N.N.254-254.10    N.N.254.255");
		}

		public void TestSubnetTable22()
		{
			TestSubnetTable(22,
@"                            N.N.0.0     N.N.0-3.10        N.N.3.255
                              N.N.4.0     N.N.4-7.10        N.N.7.255
                              N.N.8.0     N.N.8-11.10       N.N.11.255
                              N.N.12.0    N.N.12-15.10      N.N.15.255
                              N.N.240.0   N.N.240-243.10    N.N.243.255
                              N.N.244.0   N.N.244-247.10    N.N.247.255
                              N.N.248.0   N.N.248-251.10    N.N.251.255
                              N.N.252.0   N.N.252-254.10    N.N.254.255");
		}

		public void TestSubnetTable21()
		{
			TestSubnetTable(21,
@"                            N.N.0.0     N.N.0-7.10        N.N.7.255
                              N.N.8.0     N.N.8-15.10       N.N.15.255
                              N.N.16.0    N.N.16-23.10      N.N.23.255
                              N.N.24.0    N.N.24-31.10      N.N.31.255
                              N.N.32.0    N.N.32-39.10      N.N.39.255
                              N.N.40.0    N.N.40-47.10      N.N.47.255
                              N.N.48.0    N.N.48-55.10      N.N.55.255
                              N.N.56.0    N.N.56-63.10      N.N.63.255
                              N.N.64.0    N.N.64-71.10      N.N.71.255
                              N.N.72.0    N.N.72-79.10      N.N.79.255
                              N.N.80.0    N.N.80-87.10      N.N.87.255
                              N.N.88.0    N.N.88-95.10      N.N.95.255
                              N.N.96.0    N.N.96-103.10     N.N.103.255
                              N.N.104.0   N.N.104-111.10    N.N.111.255
                              N.N.112.0   N.N.112-119.10    N.N.119.255
                              N.N.120.0   N.N.120-127.10    N.N.127.255
                              N.N.128.0   N.N.128-135.10    N.N.135.255
                              N.N.136.0   N.N.136-143.10    N.N.143.255
                              N.N.144.0   N.N.144-151.10    N.N.151.255
                              N.N.152.0   N.N.152-159.10    N.N.159.255
                              N.N.160.0   N.N.160-167.10    N.N.167.255
                              N.N.168.0   N.N.168-175.10    N.N.175.255
                              N.N.176.0   N.N.176-183.10    N.N.183.255
                              N.N.184.0   N.N.184-191.10    N.N.191.255
                              N.N.192.0   N.N.192-199.10    N.N.199.255
                              N.N.200.0   N.N.200-207.10    N.N.207.255
                              N.N.208.0   N.N.208-215.10    N.N.215.255
                              N.N.216.0   N.N.216-223.10    N.N.223.255
                              N.N.224.0   N.N.224-231.10    N.N.231.255
                              N.N.232.0   N.N.232-239.10    N.N.239.255
                              N.N.240.0   N.N.240-247.10    N.N.247.255
                              N.N.248.0   N.N.248-254.10    N.N.254.255");
		}

		public void TestSubnetTable20()
		{
			TestSubnetTable(20,
@"                            N.N.0.0     N.N.0-15.10       N.N.15.255
                              N.N.16.0    N.N.16-31.10      N.N.31.255
                              N.N.32.0    N.N.32-47.10      N.N.47.255
                              N.N.48.0    N.N.48-63.10      N.N.63.255
                              N.N.64.0    N.N.64-79.10      N.N.79.255
                              N.N.80.0    N.N.80-95.10      N.N.95.255
                              N.N.96.0    N.N.96-111.10     N.N.111.255
                              N.N.112.0   N.N.112-127.10    N.N.127.255
                              N.N.128.0   N.N.128-143.10    N.N.143.255
                              N.N.144.0   N.N.144-159.10    N.N.159.255
                              N.N.160.0   N.N.160-175.10    N.N.175.255
                              N.N.176.0   N.N.176-191.10    N.N.191.255
                              N.N.192.0   N.N.192-207.10    N.N.207.255
                              N.N.208.0   N.N.208-223.10    N.N.223.255
                              N.N.224.0   N.N.224-239.10    N.N.239.255
                              N.N.240.0   N.N.240-254.10    N.N.254.255");
		}

		public void TestSubnetTable19()
		{
			TestSubnetTable(19,
@"                            N.N.0.0     N.N.0-31.10       N.N.31.255
                              N.N.32.0    N.N.32-63.10      N.N.63.255
                              N.N.64.0    N.N.64-95.10      N.N.95.255
                              N.N.96.0    N.N.96-127.10     N.N.127.255
                              N.N.128.0   N.N.128-159.10    N.N.159.255
                              N.N.160.0   N.N.160-191.10    N.N.191.255
                              N.N.192.0   N.N.192-223.10    N.N.223.255
                              N.N.224.0   N.N.224-254.10    N.N.254.255");
		}

		public void TestSubnetTable18()
		{
			TestSubnetTable(18,
@"                            N.N.0.0     N.N.0-63.10       N.N.63.255
                              N.N.64.0    N.N.64-127.10     N.N.127.255
                              N.N.128.0   N.N.128-191.10    N.N.191.255
                              N.N.192.0   N.N.192-254.10    N.N.254.255");
		}

		public void TestSubnetTable17()
		{
			TestSubnetTable(17,
@"                            N.N.0.0     N.N.0-127.10      N.N.127.255
                              N.N.128.0   N.N.128-254.10    N.N.254.255");
		}

		void TestSubnetTable(int cidr, string rangeData)
		{
			CombineAssertions(() =>
				{
					foreach (var line in rangeData.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))
					{
						var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
						var ipAddressRange = new IPAddressRange(parts[0].Replace("N", "10") + "/" + cidr.ToString());
						var rangeTableString = parts[1].Split('.').Single(p => p.Contains("-"));
						var rangeTable = rangeTableString.Split('-');
						int from = int.Parse(rangeTable[0]);
						int to = int.Parse(rangeTable[1]);
						AssertInRange(true, ipAddressRange, parts[1].Replace("N", "10").Replace(rangeTableString, from.ToString()));
						AssertInRange(true, ipAddressRange, parts[1].Replace("N", "10").Replace(rangeTableString, to.ToString()));
						int n = from + (to - from) / 2;
						AssertInRange(true, ipAddressRange, parts[1].Replace("N", "10").Replace(rangeTableString, n.ToString()));
						n = from - 3;
						if (n > 0)
						{
							AssertInRange(false, ipAddressRange, parts[1].Replace("N", "10").Replace(rangeTableString, n.ToString()));
						}
						n = to + 3;
						if (n < 255)
						{
							AssertInRange(false, ipAddressRange, parts[1].Replace("N", "10").Replace(rangeTableString, n.ToString()));
						}
						int lastN = parts[1].LastIndexOf("N");
						string ip = parts[1].Substring(0, lastN) + "11" + parts[1].Substring(lastN + 1);
						AssertInRange(false, ipAddressRange, ip.Replace("N", "10").Replace(rangeTableString, from.ToString()));
						ip = parts[1].Substring(0, lastN) + "9" + parts[1].Substring(lastN + 1);
						AssertInRange(false, ipAddressRange, ip.Replace("N", "10").Replace(rangeTableString, from.ToString()));
					}
				});
		}

		void AssertInRange(bool expected, IPAddressRange range, string ip)
		{
			AssertEquals(range.ToString() + " => " + ip, expected, range.IsInRange(IPAddress.Parse(ip)));
		}
	}
}
