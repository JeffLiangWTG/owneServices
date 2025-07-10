using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.BR.Business
{
	public class BRCCatalogErrorResponseMessageProcessor : BRCResponseMessageProcessor
	{
		public BRCCatalogErrorResponseMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("5C27EE84-8922-42D3-9B88-7507A522D886", "Goods Catalog Error Response");

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { MessageTypeList.Codes.CAT };

		protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => new ZString[] { EDIMessageSubTypeList.Codes.Error };

		protected override BusinessObject GetLinkedObject(EDIMessage message) => GetLinkedObjectFromOutgoingMessage(message);

		protected override void ProcessResponseMessage(EDIMessage message)
		{
			ProcessMessages(message, BRMessageHelper.GetOutgoingMessages(message));
		}

		public static void ProcessMessages(EDIMessage incomingMessage, IEnumerable<EDIMessage> outgoingMessages)
		{
			outgoingMessages.Select(s => s.EM_LinkedObject).OfType<CusGoodsCatalog>().ForEach(catalog =>
			{
				catalog.SuspendUpdateCustomStatusOnSavingUntilSaved();
				catalog.Logs.AddNew(AutoEvents.MessageRejected);
				catalog.CGC_MessageStatus = EDIMessage.Status.Rejected;

				if (catalog != incomingMessage.EM_LinkedObject)
				{
					var clonedMessage = incomingMessage.Clone() as BREDIMessage;
					clonedMessage.EM_Status = EDIMessage.Status.Received;
					catalog.Messages.Add(clonedMessage);
				}
			});
		}
	}
}
