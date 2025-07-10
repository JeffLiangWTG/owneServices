using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class CustomsMessageUpdatesRequesterTest : TestCaseWithFactory
{
	public void TestParameters()
	{
		var updatesRequester = new CustomsMessageUpdatesRequester();

		AssertExceptionThrown<ArgumentNullException>("message is required", () => updatesRequester.RequestUpdates(null));

		var message = Factory.New<ITEDIMessage>();
		AssertExceptionThrown<InvalidOperationException>("message.EI_InterchangeType must be 'R'", () => updatesRequester.RequestUpdates(message));

		message.EM_MessageType = SADConstants.CustomsInterchangeType.IdocR;
		AssertExceptionThrown<ArgumentNullException>("parent interchange is required", () => updatesRequester.RequestUpdates(message));

		var interchange = Factory.New<ITEDIInterchange>();
		interchange.ContainedMessages.Add(message);
		AssertNoExceptionThrown(() => updatesRequester.RequestUpdates(message));
	}

	public void TestRequestUpdates()
	{
		var sentMessage = GetValidMessageLinkedToAnInterchange("845A0108.R01");

		var updatesRequester = new CustomsMessageUpdatesRequester();
		var filename = updatesRequester.RequestUpdates(sentMessage);
		AssertEquals("845A0108.R01", filename);

		var query = new ZDBOnlyQuery(typeof(EDIInterchange));
		query.AddToFilter(EDIInterchangeSchema.EI_InterchangeType, EDIInterchangeTypeList.Codes.ITCustomsRequestResponse);
		var requestInterchange = Factory.LoadTop1<EDIInterchange>(query);
		AssertNotNull(requestInterchange);
		CombineAssertions(() =>
		{
			AssertEquals("HUB", requestInterchange.EI_ApplicationCode);
			AssertEquals("ITR", requestInterchange.EI_InterchangeType);
			AssertEquals("TRX", requestInterchange.EI_ReceiveTransmit);
			AssertEquals("eHub", requestInterchange.EI_To);
			AssertEquals("HQU", requestInterchange.EI_Status);
			AssertEquals("<ITCustoms xmlns=\"http://www.wisetechglobal.com/eServices/Schemas/ITCustoms/RequestResponse\"><Files><File><Name>845A0108.R01</Name></File></Files></ITCustoms>", requestInterchange.EI_BodyText);
			AssertEquals("HUB", requestInterchange.EI_TransportType);
		});
	}

	public void TestUsesSeparateFactory()
	{
		int localSaveCount = 0;
		Factory.Saved += (factory, savedSuccessfully) => { localSaveCount++; };

		AssertEquals("PRE-CONDITION", 0, localSaveCount);
		var sentMessage = GetValidMessageLinkedToAnInterchange("845A0108.R01");
		new CustomsMessageUpdatesRequester().RequestUpdates(sentMessage);
		AssertEquals("POST-CONDITION", 0, localSaveCount);
	}

	ITEDIMessage GetValidMessageLinkedToAnInterchange(ZString filename)
	{
		var sentInterchange = Factory.New<ITEDIInterchange>();
		sentInterchange.IsTransmitInterchange = true;
		sentInterchange.EI_From = "XXX";
		sentInterchange.EI_To = "YYY";
		sentInterchange.EI_HeaderText = $@"
	<ITMessage>
		<Staff>BOB</Staff>
		<Node>845A</Node>
		<MessageType>R</MessageType>
		<AccountNumber>13149600150-003</AccountNumber>
		<Header>845A            {filename}            137100    13149600150     003 00003</Header>
	</ITMessage>";

		var sentMessage = Factory.New<ITEDIMessage>();
		sentMessage.MessageNumberStrategy = new FixedMessageNumberStrategy("000001");
		sentMessage.EM_EI = sentInterchange.PK;
		sentInterchange.ContainedMessages.Add(sentMessage);
		sentMessage.EM_MessageType = SADConstants.CustomsInterchangeType.IdocR;
		return sentMessage;
	}
}
