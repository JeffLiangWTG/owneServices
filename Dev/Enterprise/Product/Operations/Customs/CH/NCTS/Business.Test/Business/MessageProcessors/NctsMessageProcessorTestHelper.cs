using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.CH.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using static NUnit.Framework.Assertion;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

public class NctsMessageProcessorTestHelper
{
	public NctsMessageProcessorTestHelper(BusinessObjectFactory factory)
	{
		this.factory = factory;
	}
	readonly BusinessObjectFactory factory;

	public (NctsHeader nctsHeader, CHEDIMessage sentEdiMessage) CreateNctsHeaderAndSentMessage(string applicationReference = null, string movementType = NctsMovementType.Codes.Departure, string mrn = null, Action<NctsHeader> prepareNctsHeader = null)
	{
		var nctsHeader = factory.New<NctsHeader>();
		nctsHeader.SetMovementType(movementType);
		if (mrn != null)
		{
			nctsHeader.MovementReferenceNumberSetter(mrn);
		}
		prepareNctsHeader?.Invoke(nctsHeader);

		var sentEdiMessage = MessageProcessorTestHelper.CreateSentEdiMessage(factory, applicationReference);

		var parentCollection = nctsHeader.IsDepartureMovement ? nctsHeader.MovementHeader.Messages : nctsHeader.Messages;
		parentCollection.Add(sentEdiMessage);

		factory.Save();

		return (nctsHeader, sentEdiMessage);
	}

	public CHEDIMessage CreateReceivedMessage(ZGuid sessionGuid)
	{
		var receivedEdiInterchange = factory.New<EDIInterchange>();
		receivedEdiInterchange.EI_From = "Customs";
		receivedEdiInterchange.EI_To = "CW1";
		receivedEdiInterchange.EI_SessionGUID = sessionGuid;
		receivedEdiInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;

		var receivedEdiMessage = factory.New<CHEDIMessage>();
		receivedEdiMessage.EM_Status = EDIMessage.Status.Received;
		receivedEdiInterchange.ContainedMessages.Add(receivedEdiMessage);

		factory.Save();
		return receivedEdiMessage;
	}

	public void AssertEvent(string assertionMessage, Logs logs, Event expectedEvent, string expectedReference = null)
	{
		var actualEvent = logs.MostRecentLogByEventTime(expectedEvent);
		AssertNotNull($"{assertionMessage} '{expectedEvent.CodeAndDescription}' added", actualEvent);
		if (actualEvent != null)
		{
			if (expectedReference != null)
			{
				AssertEquals($"{assertionMessage} SL_Reference", expectedReference, actualEvent.SL_Reference);
			}
		}
	}
}
