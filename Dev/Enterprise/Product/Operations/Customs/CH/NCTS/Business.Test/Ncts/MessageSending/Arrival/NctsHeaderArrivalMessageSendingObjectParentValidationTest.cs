using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

class NctsHeaderArrivalMessageSendingObjectParentValidationTest : BusinessObjectValidationTestCase
{
	public void TestDateOfUnloading() => CombineAssertions(() =>
	{
		var messageError = "[NP70025] The unloading date can’t be in the future.";

		SendingObject.DateOfUnloading = ZDateTimeOffset.Empty;
		AssertNoMessageErrorContaining("DateOfUnloading is empty", SendingObject.DateOfUnloadingInfo, messageError);

		SendingObject.DateOfUnloading = ZDateTimeOffset.Today.AddDays(-1);
		AssertNoMessageErrorContaining("DateOfUnloading lies behind the current date", SendingObject.DateOfUnloadingInfo, messageError);

		SendingObject.DateOfUnloading = ZDateTimeOffset.Today;
		AssertNoMessageErrorContaining("DateOfUnloading is equal to the current date", SendingObject.DateOfUnloadingInfo, messageError);

		SendingObject.DateOfUnloading = ZDateTimeOffset.Today.AddDays(1);
		AssertHasMessageErrorContaining("DateOfUnloading lies in the future compared to the current date", SendingObject.DateOfUnloadingInfo, messageError);
	});

	NctsHeader NctsHeader => nctsHeader ?? (nctsHeader = CreateNctsHeader());
	NctsHeader nctsHeader;

	NctsHeader CreateNctsHeader()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
		return nctsHeader;
	}

	NctsHeaderArrivalMessageSendingObjectParent SendingObject => sendingObject ?? (sendingObject = new NctsHeaderArrivalMessageSendingObjectParent(NctsHeader));
	NctsHeaderArrivalMessageSendingObjectParent sendingObject;
}
