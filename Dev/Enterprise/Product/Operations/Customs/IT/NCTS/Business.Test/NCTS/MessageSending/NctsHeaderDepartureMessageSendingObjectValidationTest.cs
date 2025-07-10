using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsHeaderDepartureMessageSendingObjectValidationTest : BusinessObjectValidationTestCase
{
	public void TestValidateShouldSend()
	{
		var nctsHeader = Factory.NewDepartureNctsHeader();
		var messageSendingObject = new NctsHeaderDepartureMessageSendingObjectOnlyForTestingPurposes(nctsHeader);

		nctsHeader.BH_JobReference = "A00001";
		nctsHeader.BH_MessageStatus = NctsMessageStatusList.Codes.DepartureDeclarationNotSent;
		messageSendingObject.ShouldSend = true;
		AssertNoWarnings(messageSendingObject.ShouldSendInfo);

		nctsHeader.BH_MessageStatus = NctsMessageStatusList.Codes.Ok;
		messageSendingObject.ShouldSend = true;
		var expectedWarningMessage = $"The Entry {nctsHeader.BH_JobReference} was already sent and is already registered or waiting for a message from Customs. Resending this entry could result in a duplicated declaration.";
		AssertHasWarningContaining(messageSendingObject.ShouldSendInfo, expectedWarningMessage);

		messageSendingObject.ShouldSend = false;
		AssertNoWarnings(messageSendingObject.ShouldSendInfo);
	}
}
