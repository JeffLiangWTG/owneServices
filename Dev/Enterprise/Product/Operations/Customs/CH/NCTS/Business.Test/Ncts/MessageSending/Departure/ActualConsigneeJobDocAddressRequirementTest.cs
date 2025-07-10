using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

class ActualConsigneeJobDocAddressRequirementTest : BusinessObjectValidationTestCase
{
	public void TestValidateNotEmpty() => CombineAssertions(() =>
	{
		var message = PassarValidationMessages.MessageNS30034;

		AssertError(true);
		AssertError(false, reasonText: string.Empty);
		AssertError(false, actualCustmosOffice: "CH123456");
		AssertError(false, addressPK: ZGuid.BrettsGuid);

		void AssertError(bool errorExpected, string reasonText = "reason", string actualCustmosOffice = "", ZGuid? addressPK = null)
		{
			sendingObject.ReasonText = reasonText;
			sendingObject.ActualDestinationCustomsOffice = actualCustmosOffice;
			sendingObject.ActualConsignee.E2_OA_Address = addressPK ?? ZGuid.Empty;
			sendingObject.ActualConsignee.Validation.ValidateAll();
			var assertionMessage = $"ReasonText={sendingObject.ReasonText} ActualDestinationCustomsOffice={sendingObject.ActualDestinationCustomsOffice} E2_OA_Address={sendingObject.ActualConsignee.E2_OA_Address}";
			if (errorExpected)
			{
				AssertHasError(assertionMessage, sendingObject.ActualConsignee.E2_OA_AddressInfo, message);
			}
			else
			{
				AssertNoError(assertionMessage, sendingObject.ActualConsignee.E2_OA_AddressInfo, message);
			}
		}
	});

	protected override void SetUp()
	{
		base.SetUp();
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		sendingObject = new NctsHeaderDepartureMessageSendingObject(nctsHeader);
		sendingObject.MessageType = PassarMessageTypeList.Codes.NT141;
	}
	NctsHeaderDepartureMessageSendingObject sendingObject;
}
