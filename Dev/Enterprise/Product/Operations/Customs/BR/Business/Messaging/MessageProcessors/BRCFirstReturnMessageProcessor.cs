using System.Collections.Generic;
using CargoWise.Customs.BR.MessageDefinitions.Export.Incoming;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.BR;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.BR.Business
{
	public class BRCFirstReturnMessageProcessor : BRCResponseMessageProcessor
	{
		public BRCFirstReturnMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("14F0E543-77EB-4660-8ED4-4B24C5223BE9", "First return");

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { MessageTypeList.Codes.CDC };

		protected override BusinessObject GetLinkedObject(EDIMessage message) => GetLinkedObjectFromOutgoingMessage(message);

		protected override void ProcessResponseMessage(EDIMessage message)
		{
			if (message.EM_LinkedObject is CusEntryHeader entryHeader)
			{
				var json = message.EM_MessageText;

				var firstReturn = BRMessageHelper.DeserializeObject<FirstReturn>(json);
				if (firstReturn != null)
				{
					if (firstReturn.severity == "ERROR")
					{
						if (entryHeader.CH_Status == BRMessageStatusList.Codes.AwaitingResponse)
						{
							entryHeader.CH_Status = BRMessageStatusList.Codes.Rejected;
						}
						else
						{
							Logger.Log($"Message #{message.EM_MessageNum}: First return error message received, message status not updated, because current status is '{entryHeader.CH_Status}'");
						}
					}
					else if (string.IsNullOrEmpty(firstReturn.severity))
					{
						Logger.Log($"Message #{message.EM_MessageNum}: First return success message received, keep current status '{entryHeader.CH_Status}'");
					}
					else
					{
						Logger.LogError($"Message #{message.EM_MessageNum}: First return message with unknown severity '{firstReturn.severity}' received.");
					}
				}
				else
				{
					message.EM_Status = EDIMessageStatusList.Codes.Failed;

					Logger.LogError($"Message #{message.EM_MessageNum}: Message deserialization was failed.");
				}
			}
		}
	}
}
