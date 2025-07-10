using System;
using CargoWise.Billing.Collectors.Warehouse;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Warehouse
{
	[TestedType(typeof(RFDeviceModelCollector_GP2_24_4_19_703_To_24_4_19_999))]
	sealed class RFDeviceModelCollector_GP2_24_4_19_703_To_24_4_19_999Test : RFDeviceModelCollectorBaseTest
	{
		public void TestMinimumVersionRequired()
		{
			using (ReleaseInfo.SetTemporaryInstanceForTesting(ReleaseInfo.CreateNewInstanceForTesting("24.4.19.702", DateTime.Now, "ALP")))
			{
				Assert("Should not be active in versions below 24.4.19.703", !ScriptToTest.IsActive);
			}

			using (ReleaseInfo.SetTemporaryInstanceForTesting(ReleaseInfo.CreateNewInstanceForTesting("24.4.19.705", DateTime.Now, "ALP")))
			{
				Assert("Should be active in versions above 24.4.19.703", ScriptToTest.IsActive);
			}
		}
	}
}
