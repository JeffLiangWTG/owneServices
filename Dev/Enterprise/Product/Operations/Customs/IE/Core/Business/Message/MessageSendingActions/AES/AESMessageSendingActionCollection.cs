using CargoWise.Types;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.IE.Business.Declaration;
using EntryStatusList = Enterprise.Customs.Common.EU.AESEntryStatusList.Codes;
using MessageTypesList = Enterprise.Customs.IE.Messaging.AESOutgoingMessageTypeList.Codes;

namespace Enterprise.Customs.IE.Business
{
	public class AESMessageSendingActionCollection : CusEntryHeaderMessageSendingActionCollection<AESMessageSendingAction>
	{
		public AESMessageSendingActionCollection(AESMessageSendingActionParent sendingActionParent) : base(sendingActionParent) { }

		protected override AESMessageSendingAction CreateElementCore(CusEntryHeader entryHeader)
		{
			var sendingAction = base.CreateElementCore(entryHeader);

			var defaultMessageType = GetDefaultMessageType(entryHeader.Declaration.JE_MessageType, entryHeader.CH_EntryStatus, entryHeader.MovementReferenceNumber.IsEmpty);
			if (!defaultMessageType.IsEmpty)
			{
				sendingAction.MessageType = defaultMessageType;
			}

			return sendingAction;
		}

		ZString GetDefaultMessageType(string jobType, string entryStatus, bool isMrnEmpty)
		{
			var defaultMessageType = string.Empty;

			if (jobType == IEJobMessageTypeList.Codes.Export)
			{
				if (entryStatus == EntryStatusList.PendingControl || entryStatus == EntryStatusList.Prelodged)
				{
					defaultMessageType = MessageTypesList.ExportPresentation;
				}
				else if (entryStatus == EntryStatusList.AmendmentRequested)
				{
					defaultMessageType = MessageTypesList.ExportAmendment;
				}
				else if (entryStatus == EntryStatusList.MrnAllocated)
				{
					defaultMessageType = MessageTypesList.ExportCancellation;
				}
				else if(entryStatus == EntryStatusList.ReleasedForExport)
				{
					defaultMessageType = MessageTypesList.ReleaseAmendment;
				}
				else if (entryStatus == EntryStatusList.Requested)
				{
					defaultMessageType = MessageTypesList.ExportCancellation;
				}
				else
				{
					defaultMessageType = MessageTypesList.ExportOriginal;
				}
			}
			else if (jobType == IEJobMessageTypeList.Codes.ReExport)
			{
				defaultMessageType = entryStatus == EntryStatusList.CancellationRequestedByCustoms ? MessageTypesList.ExitCancellation : isMrnEmpty ? MessageTypesList.ReExport : MessageTypesList.ReExportAmendment;
			}
			else if (jobType == IEJobMessageTypeList.Codes.ExitSummary)
			{
				defaultMessageType = entryStatus == EntryStatusList.CancellationRequestedByCustoms ? MessageTypesList.ExitCancellation : isMrnEmpty ? MessageTypesList.ExitOriginal : MessageTypesList.ExitAmendment;
			}

			return defaultMessageType;
		}
	}
}
