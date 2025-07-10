using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM862;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.Messaging.AIS.Testing
{
	class IM862ProviderTest : TestCaseWithFactory
	{
		public void TestLocalReferenceNumber()
		{
			AssertEquals("MRN001", provider.MovementReferenceNumber);
		}

		public void TestCaseID()
		{
			AssertEquals("123456789123456789123456789456123456", provider.CaseID);
		}

		public void TestReason()
		{
			AssertEquals("Reason", provider.AmendmentRequestCancellationReason);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new IM862Provider(new Im862()
			{
				ImportOperation = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MCciOperationType862
				{
					AmendmentRequestCancellationReason = "Reason",
					CaseId = "123456789123456789123456789456123456",
					Mrn = "MRN001",
				}
			});
		}
		IM862Provider provider;
	}
}
