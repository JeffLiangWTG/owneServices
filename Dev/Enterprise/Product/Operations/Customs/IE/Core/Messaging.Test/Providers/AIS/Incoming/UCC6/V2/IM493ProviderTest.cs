using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.Messaging.AIS.Testing
{
	class IM493ProviderTest : TestCaseWithFactory
	{
		public void TestMovementReferenceNumber()
		{
			AssertEquals("12MRN345CDEFG678R9", provider.MovementReferenceNumber);
		}

		public void TestLRN()
		{
			AssertEquals("12LRN323666666", provider.LocalReferenceNumber);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new IM493Provider(new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM493.Im493
			{
				ImportOperation = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MCciOperationType493
				{
					Mrn = "12MRN345CDEFG678R9",
					Lrn = "12LRN323666666",
				}
			});
		}
		IM493Provider provider;
	}
}
