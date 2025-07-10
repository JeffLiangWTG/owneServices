using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.ES.Business
{
	public abstract class ESBranchCustomsApplicationTypeMessageProcessor<TResponseProvider> : BranchCustomsApplicationTypeMessageProcessor
	{
		protected ESBranchCustomsApplicationTypeMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override sealed string ApplicationCodeCore => ApplicationCodeList.Codes.ESCustomsMessage;

		protected sealed override IReadOnlyList<ZString> MessageTypesToIncludeCore => MessageTypesToIncludeCoreES;
		protected abstract IReadOnlyList<ZString> MessageTypesToIncludeCoreES { get; }

		protected void CheckMessageTextNotEmpty(EDIMessage message)
		{
			if (message.EM_MessageText.IsEmpty)
			{
				throw new InvalidOperationException(Res.GetString("AFDC11C0-C9CB-409F-A8CC-4F87B111C81B", "Message Text is empty so can't continue with processing"));
			}
		}

		public TResponseProvider GetMessageProvider(EDIMessage message) => GetMessageProviderCore(message);
		protected abstract TResponseProvider GetMessageProviderCore(EDIMessage message);

		protected void SetMessageStatusAsFailed(EDIMessage message) => message.EM_Status = EDIMessageStatusList.Codes.Failed;

		protected virtual void SetMessageStatusAsReceived(EDIMessage message) => message.EM_Status = EDIMessageStatusList.Codes.Received;

		protected void SetMessageSubTypeAsAccepted(EDIMessage message) => message.EM_MessageSubType = Messaging.DeclarationMessageSubTypeList.Codes.AcceptedResponse;

		protected ZString GetEDIMessageFailedLogDescription(EDIMessage message) => Res.GetString("A1FDA572-8173-4AF8-B76A-1938799E30DD", "Unable to read message text from message {0}", GetEDIMessageStatusLogDescription(message));

		protected ZString GetExceptionLogDescription(Exception ex) => Res.GetString("8802E688-8811-4BCC-982E-94E885A67049", "Exception was {0}", ex.ToString());

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Html String")]
		protected ZString GetFailureMessageDetails(ZString exceptionString, EDIMessage message)
		{
			var messageDetails = "<H3>Processor Failure</H3><br>";
			messageDetails += string.Format("<H4>Failure: {0}</H4>", GetEDIMessageFailedLogDescription(message));
			messageDetails += string.Format("<H4>Exception: {0}</H4>", exceptionString);

			return messageDetails;
		}

		protected ZString GetEDIMessageStatusLogDescription(EDIMessage message) => Res.GetString("840AA00A-8348-4F5E-9952-1EEB81C1FCC5", "(Number:{0}, Type:{1}, Sub:{2}, Ref:{3}); message status set to {4}.", message.EM_MessageNum, message.EM_MessageType, message.EM_MessageSubType, message.EM_ApplicationReference, new EDIMessageStatusList().GetDescriptionFromCode(message.EM_Status));
	}
}
