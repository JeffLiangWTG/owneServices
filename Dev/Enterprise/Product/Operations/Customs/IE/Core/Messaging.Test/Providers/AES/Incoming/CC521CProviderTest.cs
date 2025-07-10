using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC521C;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.Messaging.AES.Testing
{
	class CC521CProviderTest : TestCaseWithFactory
	{
		public void TestMovementReferenceNumber()
		{
			AssertEquals("MRN", provider.MovementReferenceNumber);
		}

		public void TestDiversionRejectionReasonCode()
		{
			AssertEquals("11", provider.DiversionRejectionReasonCode);
		}

		public void TestDiversionRejectionText()
		{
			AssertEquals("Cancelled", provider.DiversionRejectionText);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new CC521CProvider(new Cc521C
			{
				ExportOperation = new CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.ctypes.ExportOperationType11
				{
					Mrn = "MRN",
					DiversionRejectionReasonCode = "11",
					DiversionRejectionText = "Cancelled"
				}
			});
		}
		CC521CProvider provider;
	}
}
