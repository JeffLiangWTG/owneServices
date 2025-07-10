using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Moq;
using Moq.Protected;
using ResponseStatusCodes = Enterprise.Customs.IT.Business.Ucc6AcknowledgementStatusList.Codes;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class ResponseMessageTest : TestCaseWithFactory
{
	public void TestIsElaborationKO()
	{
		CombineAssertions(() =>
		{
			var responseMessage = GetMockResponseMessage(ResponseStatusCodes.ElaborationKoWithoutResult);
			AssertEquals("When Message Status is 197, IsElaborationKO", true, responseMessage.IsElaborationKO);

			responseMessage = GetMockResponseMessage(ResponseStatusCodes.ElaborationKoWithResult);
			AssertEquals("When Message Status is 197, IsElaborationKO", true, responseMessage.IsElaborationKO);

			responseMessage = GetMockResponseMessage(ResponseStatusCodes.MessageAcquired);
			AssertEquals("When Message Status is 20, IsElaborationKO", false, responseMessage.IsElaborationKO);

			responseMessage = GetMockResponseMessage("");
			AssertEquals("When Message Status is Empty, IsElaborationKO", false, responseMessage.IsElaborationKO);
		});
	}

	public void TestIsServiceNotAvailable()
	{
		CombineAssertions(() =>
		{
			var responseMessage = GetMockResponseMessage(ResponseStatusCodes.ServiceNotAvailable);
			AssertEquals("When Message Status is '0', IsServiceNotAvailable", true, responseMessage.IsServiceNotAvailable);

			responseMessage = GetMockResponseMessage(ResponseStatusCodes.ElaborationKoWithResult);
			AssertEquals("When Message Status is 197, IsServiceNotAvailable", false, responseMessage.IsServiceNotAvailable);

			responseMessage = GetMockResponseMessage(ResponseStatusCodes.MessageAcquired);
			AssertEquals("When Message Status is 20, IsServiceNotAvailable", false, responseMessage.IsServiceNotAvailable);

			responseMessage = GetMockResponseMessage("");
			AssertEquals("When Message Status is Empty, IsServiceNotAvailable", false, responseMessage.IsServiceNotAvailable);
		});
	}

	ResponseMessage<DummyBusinessObject> GetMockResponseMessage(string messageStatus)
	{
		var mockResponseMessage = new Mock<ResponseMessage<DummyBusinessObject>>(new ZString("<mock>")) { CallBase = true };

		mockResponseMessage.Protected()
			.Setup<string>("GetMessageStatus")
			.Returns(messageStatus);

		return mockResponseMessage.Object;
	}
}
