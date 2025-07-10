using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.CommissionManagement.Business.Testing
{
	public class CreateCommissionContextTest : TestCaseWithFactory
	{
		public void TestGetCommissionHeaderStreamsFilter()
		{
			var commissionHeaderForDefaultStream = Factory.New<AccCommissionHeader>();
			commissionHeaderForDefaultStream.CH0_CommissionStream = "";
			var commissionHeaderForAAAStream = Factory.New<AccCommissionHeader>();
			commissionHeaderForAAAStream.CH0_CommissionStream = "AAA";
			var commissionHeaderForBBBStream = Factory.New<AccCommissionHeader>();
			commissionHeaderForBBBStream.CH0_CommissionStream = "BBB";

			var context = new CreateCommissionContext();
			context.AgreementAndRatesOverride = new Dictionary<ZString, ICommissionAgreementAndRates>()
			{
				{ "", null },
				{ "AAA", null }
			};

			context.OnlyCreateForAgreementAndRatesOverrideStreams = false;
			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					commissionHeaderForDefaultStream,
					commissionHeaderForAAAStream,
					commissionHeaderForBBBStream
				},
				Factory.Load<AccCommissionHeader>(context.GetCommissionHeaderStreamsFilter()));

			context.OnlyCreateForAgreementAndRatesOverrideStreams = true;
			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					commissionHeaderForDefaultStream,
					commissionHeaderForAAAStream
				},
				Factory.Load<AccCommissionHeader>(context.GetCommissionHeaderStreamsFilter()));
		}

		public void TestGetLogMessage()
		{
			var context = new CreateCommissionContext();

			context.FromDate = ZDateTime.MinSmallDateTimeValue;
			context.OverwriteOldValues = true;

			AssertEquals("From Date = ALL, Rework Old Commissions = Y", context.GetLogMessage());

			context.FromDate = new ZDateTime(2021, 04, 14);
			context.OverwriteOldValues = false;

			AssertEquals("From Date = 14-Apr-21, Rework Old Commissions = N", context.GetLogMessage());
		}
	}
}
