using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.Messaging.AIS.Testing
{
	class IM884ProviderTest : TestCaseWithFactory
	{
		public void TestMRN()
		{
			AssertEquals("12MRN345CDEFG678R9", provider.MRN);
		}

		public void TestCaseId()
		{
			AssertEquals("1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ", provider.CaseId);
		}

		public void TestRemarks()
		{
			AssertEquals("Documents Present Request Cancellation Reason", provider.CancellationReason);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new IM884Provider(new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM884.Im884
			{
				ImportOperation = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MCciOperationType884
				{
					Mrn = "12MRN345CDEFG678R9",
					CaseId = "1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ",
					DocumentsPresentRequestCancellationReason = "Documents Present Request Cancellation Reason",
				}
			});
		}
		IM884Provider provider;
	}
}
