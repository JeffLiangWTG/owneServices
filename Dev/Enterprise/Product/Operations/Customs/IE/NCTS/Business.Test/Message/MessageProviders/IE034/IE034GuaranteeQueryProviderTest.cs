using System;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	class IE034GuaranteeQueryProviderTest : Customs.Business.Testing.DataProviderTestCase<IE034GuaranteeQueryProvider>
	{
		public void TestQueryIdentifier()
		{
			sendingAction.QueryIdentifier = "100";
			AssertEquals("QueryIdentifier", "100", Provider.QueryIdentifier);
		}

		public void TestPeriodToDate()
		{
			CombineAssertions(() =>
			{
				sendingAction.PeriodTo = new ZDate(new ZDateTime(2020, 12, 22));
				AssertEquals("QueryIdentifier", new DateTime(2020, 12, 22), Provider.PeriodToDate);

				sendingAction.PeriodTo = ZDate.Empty;
				AssertEquals("PeriodToDate should be MinValue from Empty", DateTime.MinValue, Provider.PeriodToDate);

				sendingAction.PeriodTo = new ZDate(DateTime.MinValue);
				AssertEquals("PeriodToDate should be MinValue from MinValue", DateTime.MinValue, Provider.PeriodToDate);
			});
		}

		public void TestPeriodFromDate()
		{
			CombineAssertions(() =>
			{
				sendingAction.PeriodFrom = new ZDate(new ZDateTime(2022, 5, 2));
				AssertEquals("PeriodFromDate", new DateTime(2022, 5, 2), Provider.PeriodFromDate);

				sendingAction.PeriodFrom = ZDate.Empty;
				AssertEquals("PeriodFromDate should be null from Empty", DateTime.MinValue, Provider.PeriodFromDate);

				sendingAction.PeriodFrom = new ZDate(DateTime.MinValue);
				AssertEquals("PeriodFromDate should be null from MinValue", DateTime.MinValue, Provider.PeriodFromDate);
			});
		}

		protected override IE034GuaranteeQueryProvider GetProvider() => new IE034GuaranteeQueryProvider(sendingAction);

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			sendingAction = new QueryOnGuaranteeSendingAction(nctsHeader);
		}
		QueryOnGuaranteeSendingAction sendingAction;
		NctsHeader nctsHeader;
	}
}
