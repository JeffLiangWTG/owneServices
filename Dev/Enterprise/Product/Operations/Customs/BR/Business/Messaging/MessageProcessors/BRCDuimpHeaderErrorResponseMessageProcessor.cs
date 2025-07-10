using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.BR;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.BR.Business
{
	public class BRCDuimpHeaderErrorResponseMessageProcessor : BRCResponseMessageProcessor
	{
		public BRCDuimpHeaderErrorResponseMessageProcessor(LoggingInformation logger)
				: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("0543F1D1-FBA1-43AD-B29A-EC33E4486D65", "DUIMP Error Response Message");

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { MessageTypeList.Codes.CIH };

		protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => new ZString[] { EDIMessageSubTypeList.Codes.Error };

		protected override BusinessObject GetLinkedObject(EDIMessage message) => GetLinkedObjectFromOutgoingMessage(message);

		protected override void ProcessResponseMessage(EDIMessage message)
		{
			var outgoingMessage = BRMessageHelper.GetOutgoingMessage(message);
			if (outgoingMessage != null)
			{
				ProcessMessage(message, outgoingMessage, Logger);
			}
		}

		public static void ProcessMessage(EDIMessage incomingMessage, EDIMessage outgoingMessage, LoggingInformation logger)
		{
			if (outgoingMessage.EM_LinkedObject is CusEntryHeader entryHeader)
			{
				if (outgoingMessage.EM_MessageSubType == EDIMessageSubTypeList.Codes.Update)
				{
					BRCDuimpHeaderSuccessResponseMessageProcessor.PostponeIfAnyLineMessageInQueue(incomingMessage, entryHeader, logger);
				}

				entryHeader.CH_Status = BRMessageStatusList.Codes.Rejected;
			}
		}
	}
}
