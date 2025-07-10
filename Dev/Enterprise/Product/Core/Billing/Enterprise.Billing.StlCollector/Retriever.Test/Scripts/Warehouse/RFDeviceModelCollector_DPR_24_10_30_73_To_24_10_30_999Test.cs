using System;
using CargoWise.Billing.Collectors.Warehouse;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Warehouse
{
	[TestedType(typeof(RFDeviceModelCollector_DPR_24_10_30_73_To_24_10_30_999))]
	sealed class RFDeviceModelCollector_DPR_24_10_30_73_To_24_10_30_999Test : RFDeviceModelCollectorBaseTest
	{
		public void TestMinimumVersionRequired()
		{
			using (ReleaseInfo.SetTemporaryInstanceForTesting(ReleaseInfo.CreateNewInstanceForTesting("24.10.30.70", DateTime.Now, "ALP")))
			{
				Assert("Should not be active in versions below 24.10.30.73", !ScriptToTest.IsActive);
			}

			using (ReleaseInfo.SetTemporaryInstanceForTesting(ReleaseInfo.CreateNewInstanceForTesting("24.10.30.74", DateTime.Now, "ALP")))
			{
				Assert("Should be active in versions above 24.10.30.73", ScriptToTest.IsActive);
			}
		}
	}
}
