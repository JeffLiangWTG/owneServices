using System;
using Enterprise.Customs.FR.Business.NCTS;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5.Testing
{
	class CustomsOfficeOfTransitWrapperTest : Customs.Business.Testing.DataProviderTestCase<CustomsOfficeOfTransitWrapper>
	{
		public void TestReferenceNumber()
		{
			AssertEquals("ReferenceNumber should be mapped to customsOffice CY_Data", "FR000001", Provider.ReferenceNumber);
		}

		public void TestArrivalDateAndTimeEstimated()
		{
			AssertEquals("ArrivalDateAndTimeEstimated should be mapped to customsOffice ", new DateTime(2024, 01, 01), Provider.ArrivalDateAndTimeEstimated);
		}

		protected override CustomsOfficeOfTransitWrapper GetProvider()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var customsOffice = nctsHeader.MovementHeader.CustomsOffices.AddNew();
			customsOffice.CY_Data = "FR000001";
			customsOffice.CY_Date = new DateTime(2024, 01, 01);

			return CustomsOfficeOfTransitWrapper.New(customsOffice);
		}
	}
}
