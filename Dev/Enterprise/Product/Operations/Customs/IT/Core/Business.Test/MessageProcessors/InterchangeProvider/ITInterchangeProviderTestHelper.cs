using CargoWise.Types;
using Enterprise.Messaging.Business;
using static NUnit.Framework.Assertion;

namespace Enterprise.Customs.IT.Business.Testing;

public static class ITInterchangeProviderTestHelper
{
	public static void AssertCreatedInterchange(EDIMessage message)
	{
		AssertCreatedInterchange(message, EDIInterchange.Status.eHubQueued);
	}

	public static void AssertCreatedInterchange(EDIMessage message, ZString expectedInterchangeStatus)
	{
		var createdInterchange = message.Interchange;

		AssertEquals("Message [" + message.EM_MessageNum + "] Interchange PK", message.EM_EI, createdInterchange.PK);
		AssertEquals("Message [" + message.EM_MessageNum + "] Interchange EI_ApplicationCode", EDIInterchange.ApplicationCodes.ITCustoms, createdInterchange.EI_ApplicationCode);
		AssertEquals("Message [" + message.EM_MessageNum + "] Interchange EI_InterchangeType", message.EM_MessageType, createdInterchange.EI_InterchangeType);
		AssertEquals("Message [" + message.EM_MessageNum + "] Interchange EI_FooterText", ZString.Empty, createdInterchange.EI_FooterText);
		AssertEquals("Message [" + message.EM_MessageNum + "] Interchange EI_InterchangeText", createdInterchange.EI_HeaderText + message.EM_MessageText + createdInterchange.EI_FooterText, createdInterchange.EI_InterchangeText);
		AssertEquals("Message [" + message.EM_MessageNum + "] Interchange EI_HeaderText starts with <ITMessage>", true, createdInterchange.EI_HeaderText.StartsWith("<ITMessage>"));
		AssertEquals("Message [" + message.EM_MessageNum + "] Interchange EI_HeaderText ends with </ITMessage>", true, createdInterchange.EI_HeaderText.EndsWith("</ITMessage>"));
		AssertEquals("Message [" + message.EM_MessageNum + "] Interchange EI_ReceiveTransmit", EDIInterchange.Direction.Transmit, createdInterchange.EI_ReceiveTransmit);
		AssertEquals("Message [" + message.EM_MessageNum + "] Interchange EI_Status", expectedInterchangeStatus, createdInterchange.EI_Status);
		AssertNotEquals("Message [" + message.EM_MessageNum + "] Interchange EI_SessionGUID", ZGuid.Empty, createdInterchange.EI_SessionGUID);
	}
}
