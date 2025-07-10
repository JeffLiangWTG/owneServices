using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.COMPLEX;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.UCC6.V1.Testing
{
	sealed class ControlResultV1ProviderTest : TestCaseWithFactory
	{
		public void TestProps()
		{
			var controlsType = new ControlsType
			{
				ControlResultCode = "ABC123",
				ControlDate = "20220101",
				Remarks = "Test remarks"
			};
			var provider = new ControlResultV1Provider(controlsType);

			AssertEquals("ABC123", provider.ControlResultCode);
			AssertEquals("Test remarks", provider.Remarks);
			AssertEquals(new ZDateTime(2022, 1, 1), provider.ControlDate);
		}
	}
}
