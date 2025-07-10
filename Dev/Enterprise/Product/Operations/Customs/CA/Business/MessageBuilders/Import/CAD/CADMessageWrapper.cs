using CargoWise.Customs.CA.MessageContracts.CAD;
using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005

#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.MessageBuilders
{
	public class CADMessageWrapper : B3ImportMessageWrapper
	{
		public CADMessageWrapper(CusEntryHeader entryHeader, bool suppressPageLineSetting = false)
			: this(entryHeader, MessageSubTypes.Undefined, suppressPageLineSetting)
		{
		}

		public CADMessageWrapper(CusEntryHeader entryHeader, CADCorrectionMessageSendingActionCollection amendmentActions)
			: this(entryHeader, MessageSubTypes.Change)
		{
			this.amendmentActions = amendmentActions;
		}

		public CADMessageWrapper(CusEntryHeader entryHeader, MessageSubTypes messageSubType, bool suppressPageLineSetting = false)
			: base(entryHeader, suppressPageLineSetting)
		{
			this.messageSubType = messageSubType == MessageSubTypes.Undefined ? GetMessageSubTypeByStatus(entryHeader.CH_Status) : messageSubType;
			this.declaration = entryHeader.Declaration;
			if (messageSubType == MessageSubTypes.Create)
			{
				VersionID = new ZString("00001");
			}
			else
			{
				VersionID = (entryHeader.CH_VersionID + 1).ToString("00000");
			}
		}

		public void ClearSendingActions()
		{
			if (IsAmendment)
			{
				amendmentActions.RemoveAndDeleteAll();
			}
		}

		public ZBool IsAmendment => amendmentActions != null;

		ZString VersionID { get; }
		readonly CADCorrectionMessageSendingActionCollection amendmentActions;
		readonly JobDeclaration declaration;
		public readonly MessageSubTypes messageSubType;

		public CADCorrectionMessageSendingActionCollection AmendmentActions => amendmentActions;

		public ICADMessageMetaData CADDocumentMetaData
		{
			get
			{
				if (cadDocumentMetaData == null)
				{
					cadDocumentMetaData = new CADDocumentMetaData(declaration, messageSubType, VersionID, amendmentActions);
				}
				return cadDocumentMetaData;
			}
		}
		CADDocumentMetaData cadDocumentMetaData;

		MessageSubTypes GetMessageSubTypeByStatus(ZString status)
		{
			if (status == MessageStatusList.Codes.NotSent || status == MessageStatusList.Codes.AwaitingOriginal || status == MessageStatusList.Codes.ErrorOriginal || status == MessageStatusList.Codes.AcknowledgedOriginal)
			{
				return MessageSubTypes.Create;
			}
			return MessageSubTypes.Change;
		}
	}
}
