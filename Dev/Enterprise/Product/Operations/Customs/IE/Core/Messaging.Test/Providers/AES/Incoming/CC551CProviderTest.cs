using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC551C;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.AES.Testing
{
	class CC551CProviderTest : TestCaseWithFactory
	{
		public void TestExportOperation()
		{
			AssertEquals("MRN", provider.MovementReferenceNumber);
			AssertEquals("Test Report", provider.OtherThingsToReport);
		}

		public void TestControlResult()
		{
			AssertEquals(ZDateTime.BrettsBirthday, provider.ControlResultDate);
			AssertEquals("Release Rejected", provider.ControlResultText);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new CC551CProvider(new Cc551C
			{
				ExportOperation = new CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.ctypes.ExportOperationType55
				{
					Mrn = "MRN",
					OtherThingsToReport = "Test Report"
				},
				ControlResult = new CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.ctypes.ControlResultType07
				{
					Date = ZDateTime.BrettsBirthday.ToDateTime(),
					Text = "Release Rejected"
				}
			});
		}
		CC551CProvider provider;
	}
}
