namespace Enterprise.Messaging.Business.Testing
{
	class InboundMessageCreatorForTesting : IInboundMessageCreator
	{
		void IInboundMessageCreator.CreateMessagesForInterchange(EDIInterchange interchange)
		{
			if (interchange.EI_BodyText == "throwexception")
			{
				throw new System.Exception();
			}

			if (interchange.EI_BodyText == "setInError")
			{
				interchange.EI_Status = Integration.EDIInterchangeStatusList.Codes.Error;
			}
			else
			{
				var ediMessage = interchange.ContainedMessages.AddNew(typeof(EDIMessage));
				ediMessage.EM_ApplicationCode = interchange.EI_ApplicationCode;
				ediMessage.EM_MessageType = "XXX";
				ediMessage.EM_Status = EDIMessage.Status.Queued;
				ediMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				ediMessage.EM_MessageNum = "1";
				ediMessage.EM_MessageText = interchange.EI_BodyText;
			}
		}
	}
}
