using System;
using CargoWise.Billing.Collectors.Warehouse;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Warehouse
{
	[TestedType(typeof(RFDeviceModelCollector_STD_24_10_9_135_To_24_10_9_999))]
	sealed class RFDeviceModelCollector_STD_24_10_9_135_To_24_10_9_999Test : RFDeviceModelCollectorBaseTest
	{
		public void TestMinimumVersionRequired()
		{
			using (ReleaseInfo.SetTemporaryInstanceForTesting(ReleaseInfo.CreateNewInstanceForTesting("24.10.9.134", DateTime.Now, "ALP")))
			{
				Assert("Should not be active in versions below 24.10.9.135", !ScriptToTest.IsActive);
			}

			using (ReleaseInfo.SetTemporaryInstanceForTesting(ReleaseInfo.CreateNewInstanceForTesting("24.10.9.136", DateTime.Now, "ALP")))
			{
				Assert("Should be active in versions above 24.10.9.135", ScriptToTest.IsActive);
			}
		}
	}
}
