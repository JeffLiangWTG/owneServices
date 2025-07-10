using System;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.GB.Business.NCTS.Testing
{
	class CustomsOfficesOfTransitProviderTest : DataProviderTestCase<CustomsOfficesOfTransitProvider>
	{
		public void TestSequenceNumber()
		{
			AssertEquals(1, provider.SequenceNumber);
		}

		public void TestArrivalDateAndTimeEstimated()
		{
			AssertEquals(System.DateTime.FromOADate(1234), provider.ArrivalDateAndTimeEstimated);
			AssertEquals("No milliseconds", 0, ((DateTime)provider.ArrivalDateAndTimeEstimated).Millisecond);
		}

		public void TestReferenceNumber()
		{
			AssertEquals(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit, provider.ReferenceNumber);
		}

		public void TestConstructor_WithDateTimeMinValue()
		{
			CombineAssertions(() =>
			{
				CustomsOfficesOfTransitProvider testProvider = null;

				AssertNoExceptionThrown("No error expected when setting arrivalTime to Empty", () =>
				{
					testProvider = new CustomsOfficesOfTransitProvider(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit, ZDateTime.Empty, 1);
				});

				AssertNull("ArrivalDateAndTimeEstimated should be null from Empty", testProvider.ArrivalDateAndTimeEstimated);

				AssertNoExceptionThrown("No error expected when setting arrivalTime to MinValue", () =>
				{
					testProvider = new CustomsOfficesOfTransitProvider(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit, new ZDateTime(System.DateTime.MinValue), 1);
				});

				AssertNull("ArrivalDateAndTimeEstimated should be null from MinValue", testProvider.ArrivalDateAndTimeEstimated);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);

			provider = new CustomsOfficesOfTransitProvider(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit, System.DateTime.FromOADate(1234), 1);
		}

		CustomsOfficesOfTransitProvider provider;

		protected override CustomsOfficesOfTransitProvider GetProvider() => provider;
	}
}
