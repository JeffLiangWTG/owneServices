using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.TR062C;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.trader_ctypes;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging.Testing
{
	class TR062CProviderTest : TestCaseWithFactory
	{
		public void TestEmptyValue()
		{
			var emptyProvider = new TR062CProvider(new Tr062C());
			CombineAssertions(() =>
			{
				AssertEquals("MRN", ZString.Empty, emptyProvider.MRN);
				AssertEquals("CaseID", ZString.Empty, emptyProvider.CaseID);
				AssertEquals("Remarks", ZString.Empty, emptyProvider.Remarks);
			});
		}

		public void TestMRN()
		{
			AssertEquals("MRN", "21IEDU4EX144268149", provider.MRN);
		}

		public void TestCaseID()
		{
			AssertEquals("CaseID", "CaseID", provider.CaseID);
		}

		public void TestRemarks()
		{
			AssertEquals("Remarks", "Remarks", provider.Remarks);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new TR062CProvider(new Tr062C
			{
				Declaration = new DeclarationType102
				{
					Mrn = "21IEDU4EX144268149",
					CaseId = "CaseID",
					Remarks = "Remarks"
				}
			});
		}
		TR062CProvider provider;
	}
}
