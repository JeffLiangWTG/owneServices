using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.Messaging.AIS.Testing
{
	class IM464ProviderTest : TestCaseWithFactory
	{
		public void TestMovementReferenceNumber()
		{
			AssertEquals("12MRN345CDEFG678R9", provider.MovementReferenceNumber);
		}

		public void TestCaseId()
		{
			AssertEquals("1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ", provider.CaseId);
		}

		public void TestRemarks()
		{
			AssertEquals("Remarks001", provider.Remarks);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new IM464Provider(new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM464.Im464
			{
				ImportOperation = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MCciOperationType464
				{
					Mrn = "12MRN345CDEFG678R9",
					CaseId = "1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ",
					Remarks = "Remarks001",
				}
			});
		}
		IM464Provider provider;
	}
}
