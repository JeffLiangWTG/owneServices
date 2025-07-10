using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC522C;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.ctypes;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.Messaging.AES.Testing
{
	class CC522CProviderTest : TestCaseWithFactory
	{
		public void TestMovementReferenceNumber()
		{
			AssertEquals("MRN", provider.MovementReferenceNumber);
		}

		public void TestExitRejectionMotivationCode()
		{
			AssertEquals("1", provider.ExitRejectionMotivationCode);
		}

		public void TestExitRejectionMotivation()
		{
			AssertEquals("Motivation 1", provider.ExitRejectionMotivation);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new CC522CProvider(new Cc522C
			{
				ExportOperation = new ExportOperationType12
				{
					Mrn = "MRN",
					ExitRejectionMotivationCode = "1",
					ExitRejectionMotivation = "Motivation 1"
				}
			});
		}
		CC522CProvider provider;
	}
}
