using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.Messaging.AIS.Testing
{
	class IM462_962ProviderTest : TestCaseWithFactory
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
			AssertEquals("AmendReason001", provider.AmendReason);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new IM462_IM962Provider(new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM962.Im962
			{
				ImportOperation = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MCciOperationType462
				{
					Mrn = "12MRN345CDEFG678R9",
					CaseId = "1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ",
					AmendReason = "AmendReason001",
				}
			});
		}
		IM462_IM962Provider provider;
	}
}
