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
	public class BRCForeignOperatorErrorResponseMessageProcessor : BRCResponseMessageProcessor
	{
		public BRCForeignOperatorErrorResponseMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("3502B802-195B-4540-83C6-EB8BE835EAA3", "Foreign Operator Error Response");

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { MessageTypeList.Codes.OPE };

		protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => new ZString[] { EDIMessageSubTypeList.Codes.Error };

		protected override BusinessObject GetLinkedObject(EDIMessage message) => GetLinkedObjectFromOutgoingMessage(message);

		protected override void ProcessResponseMessage(EDIMessage message)
		{
			ProcessMessages(message, BRMessageHelper.GetOutgoingMessages(message));
		}

		public static void ProcessMessages(EDIMessage incomingMessage, IEnumerable<EDIMessage> outgoingMessages)
		{
			outgoingMessages.Select(s => s.EM_LinkedObject).OfType<CusBRForeignOperator>().ForEach(foreignOperator =>
			{
				foreignOperator.SuspendUpdateMessageStatusOnSavingUntilSaved();
				foreignOperator.Logs.AddNew(AutoEvents.MessageRejected);
				foreignOperator.BFR_MessageStatus = EDIMessage.Status.Rejected;
				if (foreignOperator != incomingMessage.EM_LinkedObject)
				{
					var clonedMessage = incomingMessage.Clone() as BREDIMessage;
					clonedMessage.EM_Status = EDIMessage.Status.Received;
					foreignOperator.Messages.Add(clonedMessage);
				}
			});
		}
	}
}
