using System;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class NCTSTransitOfficeProviderTest : Customs.Business.Testing.DataProviderTestCase<NCTSTransitOfficeProvider>
	{
		public void TestNewOrNull()
		{
			AssertNull(NCTSTransitOfficeProvider.NewOrNull(null));
		}

		public void TestReferenceNumber()
		{
			AssertEquals("DE1234567", Provider.ReferenceNumber);
		}

		[TestDate(2023, 01, 16, 12, 20, 00)]
		public void TestArrivalDateTime()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Valid", new DateTime(2023, 01, 16, 11, 20, 00, DateTimeKind.Unspecified), Provider.ArrivalDateTime);

				officeCode.CY_Date = ZDateTime.Empty;
				AssertNull("Invalid", Provider.ArrivalDateTime);
			});
		}

		protected override NCTSTransitOfficeProvider GetProvider() => NCTSTransitOfficeProvider.NewOrNull(officeCode);

		protected override void SetUp()
		{
			base.SetUp();

			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			officeCode = header.MovementHeader.CustomsOffices.AddNew();
			officeCode.CY_Data = "DE1234567";
			officeCode.CY_Date = ZDateTime.Now;
		}
		NctsEuOfficeCode officeCode;
	}
}
