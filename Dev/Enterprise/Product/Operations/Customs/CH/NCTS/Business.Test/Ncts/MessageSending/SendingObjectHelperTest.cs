using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

class SendingObjectHelperTest : TestCaseWithFactory
{
	public void TestCheckHeaderNotNullAndHeaderType()
	{
		AssertExceptionThrown<ArgumentNullException>("Null", () => new NctsHeaderDepartureMessageSendingObjectParent(null));

		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
		AssertExceptionThrown<ArgumentException>("Not Departure", () => new NctsHeaderDepartureMessageSendingObject(nctsHeader));

		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		AssertNoExceptionThrown("Departure", () => new NctsHeaderDepartureMessageSendingObject(nctsHeader));
	}
}
