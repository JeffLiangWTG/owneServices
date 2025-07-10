using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.BR;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.BR.Business
{
	public class BRCDuimpLinesErrorResponseMessageProcessor : BRCResponseMessageProcessor
	{
		public BRCDuimpLinesErrorResponseMessageProcessor(LoggingInformation logger)
				: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("CC832ED1-9D0E-4AA8-9563-7B5BC685BDA2", "DUIMP Lines Error Response Message");

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { MessageTypeList.Codes.CIL };

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
				entryHeader.CH_Status = BRMessageStatusList.Codes.Rejected;
			}
		}
	}
}
