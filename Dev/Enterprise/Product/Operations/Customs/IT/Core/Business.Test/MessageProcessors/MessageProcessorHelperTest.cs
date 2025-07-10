using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class MessageProcessorHelperTest : TestCaseWithFactory
{
	public void TestRetrieveValueOfXmlNode()
	{
		var header = @"<ITMessage>	
	<MessageType>same value as in EI_InterchangeType</MessageType>
	<FileName>200119.ULR</FileName>
	<eHubTrackingIDFromSentInterchange>44730C16-38E6-4258-ABD9-ADF70B58AEC2</eHubTrackingIDFromSentInterchange>
	<Header>first line from the response message</Header>
</ITMessage>";
		AssertEquals("44730C16-38E6-4258-ABD9-ADF70B58AEC2", MessageProcessorHelper.RetrieveValueOfXmlNode(header, "eHubTrackingIDFromSentInterchange"));
		AssertEquals("first line from the response message", MessageProcessorHelper.RetrieveValueOfXmlNode(header, "Header"));

		header = @"<ITMessage xmlns=""http://cargowise.com/ehub/products/ITCustoms"">	
	<MessageType>same value as in EI_InterchangeType</MessageType>
	<FileName>200119.ULR</FileName>
	<eHubTrackingIDFromSentInterchange>44730C16-38E6-4258-ABD9-ADF70B58AEC2</eHubTrackingIDFromSentInterchange>
	<Header>first line from the response message</Header>
</ITMessage>";

		AssertEquals("44730C16-38E6-4258-ABD9-ADF70B58AEC2", MessageProcessorHelper.RetrieveValueOfXmlNode(header, "eHubTrackingIDFromSentInterchange"));
		AssertEquals("first line from the response message", MessageProcessorHelper.RetrieveValueOfXmlNode(header, "Header"));

		header = @"<ITMessage>	
	<MessageType>same value as in EI_InterchangeType</MessageType>
	<FileName>200119.ULR</FileName>
	<eHubTrackingIDFromSentInterchange></eHubTrackingIDFromSentInterchange>
	<Header>first line from the response message</Header>
</ITMessage>";
		AssertEquals("", MessageProcessorHelper.RetrieveValueOfXmlNode(header, "eHubTrackingIDFromSentInterchange"));

		header = "";
		AssertEquals("", MessageProcessorHelper.RetrieveValueOfXmlNode(header, "eHubTrackingIDFromSentInterchange"));

		AssertExceptionThrown<Exception>(() => MessageProcessorHelper.RetrieveValueOfXmlNode(header, "eHubTrackingIDFromSentInterchange", true));
	}

	public void TestGetSentInterchangeWithTrackingId()
	{
		var ediInterchange = Factory.New<EDIInterchange>();
		ediInterchange.EI_InterchangeNum = "1";
		ediInterchange.EI_SessionGUID = new ZGuid("77ABFE3E-177A-4872-9815-4A6C69AD0400");

		var sentEDIInterchange = Factory.New<EDIInterchange>();
		sentEDIInterchange.EI_InterchangeNum = "2";
		sentEDIInterchange.EI_SessionGUID = new ZGuid("27ABFE3E-177A-4872-9815-4A6C69AD0400");
		sentEDIInterchange.EI_ReceiveTransmit = "TRX";
		Factory.Save();

		var retrievedInterchange = MessageProcessorHelper.GetSentInterchangeWithTrackingId(new ZGuid("77ABFE3E-177A-4872-9815-4A6C69AD0400"), Factory);
		AssertNull(retrievedInterchange);
		retrievedInterchange = MessageProcessorHelper.GetSentInterchangeWithTrackingId(new ZGuid("17ABFE3E-177A-4872-9815-4A6C69AD0400"), Factory);
		AssertNull(retrievedInterchange);
		retrievedInterchange = MessageProcessorHelper.GetSentInterchangeWithTrackingId(new ZGuid("27ABFE3E-177A-4872-9815-4A6C69AD0400"), Factory);
		AssertNotNull(retrievedInterchange);
		AssertType<ITEDIInterchange>(retrievedInterchange);
	}
}
