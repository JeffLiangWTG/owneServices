using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.BR.MessageDefinitions.Duimp;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.BR;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.BR.Business
{
	public class BRCDuimpLinesSuccessResponseMessageProcessor : BRCResponseMessageProcessor
	{
		public BRCDuimpLinesSuccessResponseMessageProcessor(LoggingInformation logger)
				: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("7DBE3798-A732-4691-AC17-27BB07775459", "DUIMP Lines Success Response Message");

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { MessageTypeList.Codes.CIL };

		protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => new ZString[] { EDIMessageSubTypeList.Codes.Success };

		protected override BusinessObject GetLinkedObject(EDIMessage message) => GetLinkedObjectFromOutgoingMessage(message);

		protected override void ProcessResponseMessage(EDIMessage message)
		{
			if (message.EM_LinkedObject is CusEntryHeader entryHeader)
			{
				var messageReturn = BRMessageHelper.DeserializeObject<RespostaApiItens>(message.EM_MessageText);
				var statues = messageReturn?.multiStatus;
				if (statues == null)
				{
					message.EM_Status = EDIMessageStatusList.Codes.Failed;
					Logger.LogError($"Message #{message.EM_MessageNum}: Message deserialization was failed.");
				}
				else if (statues.Count == 0)
				{
					message.EM_Status = EDIMessageStatusList.Codes.Failed;
					Logger.LogError($"Message #{message.EM_MessageNum}: multiStatus tag is empty.");
				}
				else
				{
					ProcessLineResponses(statues, entryHeader.AllEntryLines);
					if (entryHeader.CH_CustomsPostedStatus == CustomsPostedStatusList.Codes.Accepted)
					{
						if (entryHeader.AllEntryLines.All(line => line.CL_CustomsPostedStatus.IsAccepted() || line.CL_CustomsPostedStatus.IsDeleted()))
						{
							entryHeader.CH_Status = BRMessageStatusList.Codes.Accepted;
						}
						else
						{
							entryHeader.CH_Status = BRMessageStatusList.Codes.Rejected;
						}
					}
				}
			}
		}

		void ProcessLineResponses(IEnumerable<RespostaApiMultiStatus> multiStatusList, IEnumerable<CusEntryLine> entryLines)
		{
			foreach (var multiStatus in multiStatusList)
			{
				switch (multiStatus.code)
				{
					case Constants.ResponseCodes.Update:
					case Constants.ResponseCodes.Success:
						if (ZShort.TryParse(multiStatus.identificacao?.numeroItem, out var itemNumber) && entryLines.FirstOrDefault(w => w.CL_LineNumber == itemNumber && w.CL_CustomsPostedStatus != CustomsPostedStatusList.Codes.Deleted) is CusEntryLine entryLine)
						{
							entryLine.CL_CustomsPostedStatus = entryLine.CL_CustomsPostedStatus.IsDeletePending() ? CustomsPostedStatusList.Codes.Deleted : CustomsPostedStatusList.Codes.Accepted;
						}
						break;
				}
			}
		}
	}
}
