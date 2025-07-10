using System.Collections.Generic;
using CargoWise.Customs.BR.MessageDefinitions.Export.Incoming;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.BR;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.BR.Business
{
	public class BRCSecondReturnErrorMessageProcessor : BRCResponseMessageProcessor
	{
		public BRCSecondReturnErrorMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("69E1D77F-92D3-48A3-AED4-970F67154003", "Second Return Error");

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { MessageTypeList.Codes.CDE };

		protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => new ZString[] { EDIMessageSubTypeList.Codes.Error };

		protected override BusinessObject GetLinkedObject(EDIMessage message) => GetLinkedObjectFromOutgoingMessage(message);

		protected override void ProcessResponseMessage(EDIMessage message)
		{
			if (message.EM_LinkedObject is CusEntryHeader entryHeader)
			{
				var errorObject = XmlObjectSerializer.Deserialize<error>(message.EM_MessageText);

				if (errorObject == null || errorObject.detail == null || errorObject.detail == null && errorObject.message == null && errorObject.status == 0)
				{
					message.EM_Status = EDIMessageStatusList.Codes.Failed;

					Logger.LogError($"Message #{message.EM_MessageNum}: Message deserialization was failed.");
				}
				else
				{
					message.EM_MessageInterpretation = new ExportErrorMessagePrettyFormatter(entryHeader, errorObject).GetFormattedMessageText();
					entryHeader.CH_Status = BRMessageStatusList.Codes.Rejected;
				}
			}
		}
	}
}
