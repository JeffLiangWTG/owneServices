using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.TR884C;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.trader_ctypes;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging.Testing
{
	class TR884CProviderTest : TestCaseWithFactory
	{
		public void TestEmptyValue()
		{
			var emptyProvider = new TR884CProvider(new Tr884C());
			CombineAssertions(() =>
			{
				AssertEquals("MRN", ZString.Empty, emptyProvider.MRN);
				AssertEquals("CaseID", ZString.Empty, emptyProvider.CaseId);
				AssertEquals("PresentationRequestCancellationReason", ZString.Empty, emptyProvider.PresentationRequestCancellationReason);
			});
		}

		public void TestMRN()
		{
			AssertEquals("MRN", "21IEDU4EX144268149", provider.MRN);
		}

		public void TestCaseID()
		{
			AssertEquals("CaseID", "123654", provider.CaseId);
		}

		public void TestUploadRequestCancellationReason()
		{
			AssertEquals("PresentationRequestCancellationReason", "Test Reason", provider.PresentationRequestCancellationReason);
		}

		TR884CProvider provider;
		protected override void SetUp()
		{
			base.SetUp();
			provider = new TR884CProvider(new Tr884C
			{
				Declaration = new DeclarationType107
				{
					Mrn = "21IEDU4EX144268149",
					CaseId = "123654",
					PresentationRequestCancellationReason = "Test Reason"
				},
			});
		}
	}
}
