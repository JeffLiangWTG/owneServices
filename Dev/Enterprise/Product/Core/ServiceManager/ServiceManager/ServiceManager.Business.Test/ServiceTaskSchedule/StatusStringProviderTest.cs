using System.Collections.Generic;
using Enterprise.ServiceManager.Shared;
using NUnit.Framework;

namespace Enterprise.ServiceManager.Business.Testing
{
	class StatusStringProviderTest : TestCase
	{
		public void TestStatusString()
		{
			var testResults = new Dictionary<int, string>()
			{
				{ -1, ServiceManagerHelper.StatusUnknown },
				{ 0x0, "" },
				{ 0x1, "" },
				{ 0x2, ServiceManagerHelper.StatusIdle },
				{ 0x3, ServiceManagerHelper.StatusIdle },
				{ 0x4, ServiceManagerHelper.StatusRunning },
				{ 0x5, ServiceManagerHelper.StatusRunning },
				{ 0x6, ServiceManagerHelper.StatusRunning },
				{ 0x7, ServiceManagerHelper.StatusRunning },
				{ 0x10, ServiceManagerHelper.StatusLastRunFailed },
				{ 0x11, ServiceManagerHelper.StatusLastRunFailed },
				{ 0x12, ServiceManagerHelper.StatusLastRunFailed },
				{ 0x13, ServiceManagerHelper.StatusLastRunFailed },
				{ 0x14, ServiceManagerHelper.StatusRunning },
				{ 0x15, ServiceManagerHelper.StatusRunning },
				{ 0x16, ServiceManagerHelper.StatusRunning },
				{ 0x17, ServiceManagerHelper.StatusRunning },
				{ 0x18, ServiceManagerHelper.StatusBlocked },
				{ 0x19, ServiceManagerHelper.StatusBlocked },
				{ 0x1A, ServiceManagerHelper.StatusBlocked },
				{ 0x1B, ServiceManagerHelper.StatusBlocked },
			};

			var statusProvider = new StatusStringProvider();

			CombineAssertions(() =>
			{
				foreach (var test in testResults)
				{
					AssertEquals($"{test.Key} should match status {test.Value}", test.Value, statusProvider.GetStatusString(test.Key));
				}
			});
		}
	}
}
