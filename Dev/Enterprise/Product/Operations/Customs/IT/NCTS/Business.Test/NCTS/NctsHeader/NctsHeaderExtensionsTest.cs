using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.NCTS.Business;
using Enterprise.Customs.IT.NCTS.Business.Testing;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class NctsHeaderExtensionsTest : TestCaseWithFactory
{
	public void TestCheckNotNullAndDepartureType()
	{
		AssertExceptionThrown<ArgumentNullException>("When nctsHeader is null", () => NctsHeaderExtensions.CheckNotNullAndDepartureType(null));

		var arrivalNctsHeader = Factory.New<NctsHeader>();
		arrivalNctsHeader.BH_HeaderType = EU.NCTS.Business.NctsMovementType.Codes.Arrival;
		AssertExceptionThrown<ArgumentException>("When nctsHeader is not a departure job", expectedExceptionMessage: "nctsHeader is not a departure movement", () => NctsHeaderExtensions.CheckNotNullAndDepartureType(arrivalNctsHeader));

		var departureNctsHeader = Factory.NewDepartureNctsHeader();
		AssertNoExceptionThrown("When nctsHeader is a departure job", () => NctsHeaderExtensions.CheckNotNullAndDepartureType(departureNctsHeader));
		AssertSame("Parameter and returned objects", departureNctsHeader, NctsHeaderExtensions.CheckNotNullAndDepartureType(departureNctsHeader));
	}
}
