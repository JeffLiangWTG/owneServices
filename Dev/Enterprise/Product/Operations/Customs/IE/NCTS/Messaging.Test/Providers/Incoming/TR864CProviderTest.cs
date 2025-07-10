using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.TR864C;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.trader_ctypes;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging.Testing
{
	class TR864CProviderTest : TestCaseWithFactory
	{
		public void TestEmptyValue()
		{
			var emptyProvider = new TR864CProvider(new Tr864C());
			CombineAssertions(() =>
			{
				AssertEquals("MRN", ZString.Empty, emptyProvider.MRN);
				AssertEquals("CaseID", ZString.Empty, emptyProvider.CaseId);
				AssertEquals("InvalidationRequestCancellationReason", ZString.Empty, emptyProvider.InvalidationRequestCancellationReason);
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

		public void TestInvalidationRequestCancellationReason()
		{
			AssertEquals("InvalidationRequestCancellationReason", "Test Reason", provider.InvalidationRequestCancellationReason);
		}

		TR864CProvider provider;
		protected override void SetUp()
		{
			base.SetUp();
			provider = new TR864CProvider(new Tr864C()
			{
				Declaration = new DeclarationType104
				{
					Mrn = "21IEDU4EX144268149",
					CaseId = "123654",
					InvalidationRequestCancellationReason = "Test Reason"
				}
			});
		}
	}
}
