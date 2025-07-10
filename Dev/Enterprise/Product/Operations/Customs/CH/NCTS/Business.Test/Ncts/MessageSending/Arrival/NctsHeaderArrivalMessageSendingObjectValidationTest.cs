using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

internal class NctsHeaderArrivalMessageSendingObjectValidationTest : BusinessObjectValidationTestCase
{
	public void TestMessageType()
	{
		CombineAssertions(() =>
		{
			ValidationTestHelper.AssertErrorIfNotEntered(SendingObject.MessageTypeInfo);
			ValidationTestHelper.AssertErrorIfInvalidCode(SendingObject.MessageTypeInfo, "XXX", PassarMessageTypeList.Codes.NT007);
		});
	}

	NctsHeader NctsHeader => nctsHeader ?? (nctsHeader = CreateNctsHeader());
	NctsHeader nctsHeader;

	NctsHeader CreateNctsHeader()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
		sendingObject = new NctsHeaderArrivalMessageSendingObject(nctsHeader);
		return nctsHeader;
	}

	NctsHeaderArrivalMessageSendingObject SendingObject => sendingObject ?? (sendingObject = new NctsHeaderArrivalMessageSendingObject(NctsHeader));
	NctsHeaderArrivalMessageSendingObject sendingObject;
}
