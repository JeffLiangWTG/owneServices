using System;
using System.Linq;
using NUnit.Framework;
using ServiceManager.Host.Abstractions;

namespace Enterprise.ServiceManager.Host.Testing.Helpers.Throttling.Resource
{
	class ResourceThrottlerResultTest
	{
		[Test]
		public void TestCpu()
		{
			Assert.Multiple(() =>
			{
				Test(1);
				Test(50);
				Test(100);
			});

			void Test(decimal value)
			{
				var resourceThrottlerResult = new ResourceThrottlerResult(default, default, default, value, default, default);
				var result = resourceThrottlerResult.Cpu;
				Assert.That(result, Is.EqualTo(value));
			}
		}

		[Test]
		public void TestDiskQueueLength()
		{
			Assert.Multiple(() =>
			{
				Test(1);
				Test(50);
				Test(100);
			});

			void Test(decimal value)
			{
				var resourceThrottlerResult = new ResourceThrottlerResult(default, default, default, default, value, default);
				var result = resourceThrottlerResult.DiskQueueLength;
				Assert.That(result, Is.EqualTo(value));
			}
		}

		[Test]
		public void TestPageFile()
		{
			Assert.Multiple(() =>
			{
				Test(1);
				Test(50);
				Test(100);
			});

			void Test(decimal value)
			{
				var resourceThrottlerResult = new ResourceThrottlerResult(default, default, default, default, default, value);
				var result = resourceThrottlerResult.PageFile;
				Assert.That(result, Is.EqualTo(value));
			}
		}

		[Test]
		public void TestTimedOut()
		{
			Assert.Multiple(() =>
			{
				Test(true);
				Test(false);
			});

			void Test(bool value)
			{
				var resourceThrottlerResult = new ResourceThrottlerResult(value, default, default, default, default, default);
				var result = resourceThrottlerResult.TimedOut;
				Assert.That(result, Is.EqualTo(value));
			}
		}

		[Test]
		public void TestTimeForWait()
		{
			Assert.Multiple(() =>
			{
				Test(TimeSpan.FromSeconds(1));
				Test(TimeSpan.FromSeconds(50));
				Test(TimeSpan.FromSeconds(100));
			});

			void Test(TimeSpan value)
			{
				var resourceThrottlerResult = new ResourceThrottlerResult(default, default, value, default, default, default);
				var result = resourceThrottlerResult.TimeForWait;
				Assert.That(result, Is.EqualTo(value));
			}
		}

		[Test]
		public void TestWaitType()
		{
			Assert.Multiple(() =>
			{
				foreach (var throttlerResultEnum in Enum.GetValues(typeof(ResourceThrottlerResult.ResourceThrottlerResults)).Cast<ResourceThrottlerResult.ResourceThrottlerResults>())
				{
					Test(throttlerResultEnum);
				}
			});

			void Test(ResourceThrottlerResult.ResourceThrottlerResults value)
			{
				var resourceThrottlerResult = new ResourceThrottlerResult(default, value, default, default, default, default);
				var result = resourceThrottlerResult.WaitType;
				Assert.That(result, Is.EqualTo(value));
			}
		}

		[Test]
		public void TestToString()
		{
			Assert.Multiple(() =>
			{
				Test(
					new ResourceThrottlerResult(default, ResourceThrottlerResult.ResourceThrottlerResults.CpuContention, TimeSpan.FromSeconds(1), 12.345m, default, default),
					"Waited for 00:00:01: CPU = 12");
				Test(
					new ResourceThrottlerResult(default, ResourceThrottlerResult.ResourceThrottlerResults.CpuContention, TimeSpan.FromSeconds(15), 34.567m, default, default),
					"Waited for 00:00:15: CPU = 35");
				Test(
					new ResourceThrottlerResult(default, ResourceThrottlerResult.ResourceThrottlerResults.CpuContention, TimeSpan.FromSeconds(30), 100m, default, default),
					"Waited for 00:00:30: CPU = 100");

				Test(
					new ResourceThrottlerResult(default, ResourceThrottlerResult.ResourceThrottlerResults.DiskContention, TimeSpan.FromSeconds(1), default, 12.345m, default),
					"Waited for 00:00:01: Disk Queue Length = 12.35");
				Test(
					new ResourceThrottlerResult(default, ResourceThrottlerResult.ResourceThrottlerResults.DiskContention, TimeSpan.FromSeconds(15), default, 34.567m, default),
					"Waited for 00:00:15: Disk Queue Length = 34.57");
				Test(
					new ResourceThrottlerResult(default, ResourceThrottlerResult.ResourceThrottlerResults.DiskContention, TimeSpan.FromSeconds(30), default, 100m, default),
					"Waited for 00:00:30: Disk Queue Length = 100.00");

				Test(
					new ResourceThrottlerResult(default, ResourceThrottlerResult.ResourceThrottlerResults.PageFileContention, TimeSpan.FromSeconds(1), default, default, 12.345m),
					"Waited for 00:00:01: Page File Size = 12.35 Mb");
				Test(
					new ResourceThrottlerResult(default, ResourceThrottlerResult.ResourceThrottlerResults.PageFileContention, TimeSpan.FromSeconds(15), default, default, 34.567m),
					"Waited for 00:00:15: Page File Size = 34.57 Mb");
				Test(
					new ResourceThrottlerResult(default, ResourceThrottlerResult.ResourceThrottlerResults.PageFileContention, TimeSpan.FromSeconds(30), default, default, 100m),
					"Waited for 00:00:30: Page File Size = 100.00 Mb");

				Test(
					new ResourceThrottlerResult(default, ResourceThrottlerResult.ResourceThrottlerResults.CpuContention | ResourceThrottlerResult.ResourceThrottlerResults.DiskContention | ResourceThrottlerResult.ResourceThrottlerResults.PageFileContention, TimeSpan.FromSeconds(30), 100m, 12.345m, 34.567m),
					"Waited for 00:00:30: CPU = 100, Disk Queue Length = 12.35, Page File Size = 34.57 Mb");
			});

			void Test(ResourceThrottlerResult resourceThrottlerResult, string expected)
			{
				var result = resourceThrottlerResult.ToString();
				Assert.That(result, Is.EqualTo(expected));
			}
		}
	}
}
