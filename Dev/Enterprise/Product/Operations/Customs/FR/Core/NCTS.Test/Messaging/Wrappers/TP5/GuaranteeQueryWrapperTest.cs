using CargoWise.Types;
using Enterprise.Customs.FR.Business.NCTS;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5.Testing
{
	class GuaranteeQueryWrapperTest : Customs.Business.Testing.DataProviderTestCase<GuaranteeQueryWrapper>
	{
		public void TestQueryIdentifier()
		{
			AssertEquals("QueryIdentifier should be mapped to sendingObject QueryIdentifier.", "Identifier1", Provider.QueryIdentifier);
		}

		public void TestPeriodFromDate()
		{
			AssertEquals("PeriodFromDate should be mapped to sendingObject PeriodFrom.", new ZDateTime(2025, 1, 5), Provider.PeriodFromDate);
		}

		public void TestPeriodToDate()
		{
			AssertEquals("PeriodToDate should be mapped to sendingObject PeriodTo.", new ZDateTime(2025, 1, 6), Provider.PeriodToDate);
		}

		protected override GuaranteeQueryWrapper GetProvider()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);

			var sendingObject = new TP5MessageSendingObject(nctsHeader);
			sendingObject.QueryIdentifier = "Identifier1";
			sendingObject.PeriodFrom = new ZDateTime(2025, 1, 5);
			sendingObject.PeriodTo = new ZDateTime(2025, 1, 6);

			return GuaranteeQueryWrapper.New(sendingObject);
		}
	}
}
