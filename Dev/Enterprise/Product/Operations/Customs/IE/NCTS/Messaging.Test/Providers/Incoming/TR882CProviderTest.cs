using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.TR882C;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.trader_ctypes;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging.Testing
{
	class TR882CProviderTest : TestCaseWithFactory
	{
		public void TestEmptyValue()
		{
			var emptyProvider = new TR882CProvider(new Tr882C());
			CombineAssertions(() =>
			{
				AssertEquals("MRN", ZString.Empty, emptyProvider.MRN);
				AssertEquals("CaseID", ZString.Empty, emptyProvider.CaseId);
				AssertEquals("UploadRequestCancellationReason", ZString.Empty, emptyProvider.UploadRequestCancellationReason);
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
			AssertEquals("UploadRequestCancellationReason", "Test Reason", provider.UploadRequestCancellationReason);
		}

		TR882CProvider provider;
		protected override void SetUp()
		{
			base.SetUp();
			provider = new TR882CProvider(new Tr882C
			{
				Declaration = new DeclarationType106
				{
					Mrn = "21IEDU4EX144268149",
					CaseId = "123654",
					UploadRequestCancellationReason = "Test Reason"
				},
			});
		}
	}
}
