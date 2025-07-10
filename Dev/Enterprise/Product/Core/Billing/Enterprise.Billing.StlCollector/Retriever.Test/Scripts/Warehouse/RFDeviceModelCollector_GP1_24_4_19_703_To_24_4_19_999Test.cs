using System;
using CargoWise.Billing.Collectors.Warehouse;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Warehouse
{
	[TestedType(typeof(RFDeviceModelCollector_GP1_24_7_12_724_To_24_7_12_999))]
	sealed class RFDeviceModelCollector_GP1_24_7_12_724_To_24_7_12_999Test : RFDeviceModelCollectorBaseTest
	{
		public void TestMinimumVersionRequired()
		{
			using (ReleaseInfo.SetTemporaryInstanceForTesting(ReleaseInfo.CreateNewInstanceForTesting("24.7.12.723", DateTime.Now, "ALP")))
			{
				Assert("Should not be active in versions below 24.7.12.724", !ScriptToTest.IsActive);
			}

			using (ReleaseInfo.SetTemporaryInstanceForTesting(ReleaseInfo.CreateNewInstanceForTesting("24.7.12.725", DateTime.Now, "ALP")))
			{
				Assert("Should be active in versions above 24.7.12.725", ScriptToTest.IsActive);
			}
		}
	}
}
