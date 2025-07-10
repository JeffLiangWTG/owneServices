using System;
using CargoWise.Billing.Collectors.Warehouse;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Warehouse
{
	[TestedType(typeof(RFDeviceModelCollector))]
	sealed class RFDeviceModelCollectorTest : RFDeviceModelCollectorBaseTest
	{
		public void TestMinimumVersionRequired()
		{
			using (ReleaseInfo.SetTemporaryInstanceForTesting(ReleaseInfo.CreateNewInstanceForTesting("24.10.31.110", DateTime.Now, "ALP")))
			{
				Assert("Should not be active in versions below 24.10.31.119", !ScriptToTest.IsActive);
			}

			using (ReleaseInfo.SetTemporaryInstanceForTesting(ReleaseInfo.CreateNewInstanceForTesting("24.10.31.120", DateTime.Now, "ALP")))
			{
				Assert("Should be active in versions above 24.10.31.119", ScriptToTest.IsActive);
			}
		}
	}
}
