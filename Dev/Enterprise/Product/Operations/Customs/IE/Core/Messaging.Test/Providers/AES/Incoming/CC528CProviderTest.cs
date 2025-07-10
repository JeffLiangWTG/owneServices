using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC528C;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.Messaging.AES.Testing
{
	class CC528CProviderTest : TestCaseWithFactory
	{
		public void TestLocalReferenceNumber()
		{
			AssertEquals("LRN", provider.LocalReferenceNumber);
		}

		public void TestMovementReferenceNumber()
		{
			AssertEquals("MRN", provider.MovementReferenceNumber);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new CC528CProvider(new Cc528C
			{
				ExportOperation = new CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.ctypes.ExportOperationType58
				{
					Lrn = "LRN",
					Mrn = "MRN"
				}
			});
		}
		CC528CProvider provider;
	}
}
