using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.BR;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.BR.Business
{
	public class BRCLPCOErrorResponseMessageProcessor : BRCResponseMessageProcessor
	{
		public BRCLPCOErrorResponseMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("684C37C3-A16C-45A0-90D8-4A2846A07746", "LPCO Error Response");

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { MessageTypeList.Codes.LPC };

		protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => new ZString[] { EDIMessageSubTypeList.Codes.Error };

		protected override BusinessObject GetLinkedObject(EDIMessage message) => GetLinkedObjectFromOutgoingMessage(message);

		protected override void ProcessResponseMessage(EDIMessage message)
		{
			var outgoingMessages = BRMessageHelper.GetOutgoingMessages(message);
			if (outgoingMessages.Any())
			{
				ProcessMessages(message, outgoingMessages);
			}
		}

		public static void ProcessMessages(EDIMessage incomingMessage, IEnumerable<EDIMessage> outgoingMessages)
		{
			outgoingMessages.Select(s => s.EM_LinkedObject).OfType<CusLPCOHeader>().ForEach(lpcoHeader =>
			{
				lpcoHeader.CPH_MessageStatus = BRMessageStatusList.Codes.Rejected;

				if (lpcoHeader != incomingMessage.EM_LinkedObject)
				{
					var clonedMessage = incomingMessage.Clone() as BREDIMessage;
					clonedMessage.EM_Status = EDIMessage.Status.Received;
					lpcoHeader.Messages.Add(clonedMessage);
				}
			});
		}
	}
}
