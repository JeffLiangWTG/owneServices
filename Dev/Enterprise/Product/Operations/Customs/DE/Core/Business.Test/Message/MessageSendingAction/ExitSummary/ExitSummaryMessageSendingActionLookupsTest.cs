using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.DE.Business.Testing
{
	class ExitSummaryMessageSendingActionLookupsTest : TestCaseWithFactory
	{
		public void TestMessageTypeList()
		{
			var list = lookups.MessageTypeList;
			CombineAssertions(() =>
			{
				AssertEquals("CodesAsString", "ANT, PRE", list.CodesAsString);
				AssertSame("Cached", list, lookups.MessageTypeList);
			});
		}

		public void TestMessageTypeList_Status301()
		{
			exitDetail.CED_Status = UniversalReferenceConstants.CusExitDetailStatus._301;
			var list = lookups.MessageTypeList;
			CombineAssertions(() =>
			{
				AssertEquals("CodesAsString", "PRE", list.CodesAsString);
				AssertSame("Cached", list, lookups.MessageTypeList);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			var exitHeader = Factory.New<CusExitControlHeader>();
			exitDetail = exitHeader.CusExitDetails.AddNew();
			var messageSendingAction = new ExitSummaryMessageSendingAction(exitDetail);
			lookups = messageSendingAction.Lookups;
		}
		CusExitDetail exitDetail;
		ExitSummaryMessageSendingActionLookups lookups;
	}
}
