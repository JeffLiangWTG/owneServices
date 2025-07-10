using System;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Messaging;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.Business.Testing
{
	class CusEntryHeaderMessageSendingActionParentForTesting : CusEntryHeaderMessageSendingActionParent<CusEntryHeaderMessageSendingActionForTesting>
	{
		public CusEntryHeaderMessageSendingActionParentForTesting(JobDeclaration declaration) : base(declaration) { }

		protected override Type SendingObjectCollectionType => typeof(CusEntryHeaderMessageSendingActionCollectionForTesting);
	}

	class CusEntryHeaderMessageSendingActionForTesting : CusEntryHeaderMessageSendingAction
	{
		public CusEntryHeaderMessageSendingActionForTesting(CusEntryHeader cusEntryHeader) : base(cusEntryHeader) { }

		protected override Type SenderType => typeof(MessageSenderForTesting);

		protected override CusEntryHeaderMessageSendingActionValidation GetNewValidation() => new CusEntryHeaderMessageSendingActionValidationForTesting(this);

		protected override CusEntryHeaderMessageSendingActionLookups GetNewLookups()
		{
			return new CusEntryHeaderMessageSendingActionLookupsForTesting(this);
		}

		protected override bool Annotation_ReadOnly => MessageType == "ZZZ";
	}

	class CusEntryHeaderMessageSendingActionLookupsForTesting : CusEntryHeaderMessageSendingActionLookups
	{
		public CusEntryHeaderMessageSendingActionLookupsForTesting(CusEntryHeaderMessageSendingActionForTesting parent) : base(parent) { }

		CodeDescriptionPairList fSendingActionTypeList;
		public override CodeDescriptionPairList SendingActionTypeList
		{
			get
			{
				if (fSendingActionTypeList == null)
				{
					fSendingActionTypeList = new CodeDescriptionPairList();
					fSendingActionTypeList.AddPair(AESOutgoingMessageTypeList.Codes.ExportOriginal, AESOutgoingMessageTypeList.Descriptions.ExportOriginal);
					fSendingActionTypeList.AddPair(AESOutgoingMessageTypeList.Codes.ExportAmendment, AESOutgoingMessageTypeList.Descriptions.ExportAmendment);
					fSendingActionTypeList.AddPair(AESOutgoingMessageTypeList.Codes.ExportCancellation, AESOutgoingMessageTypeList.Descriptions.ExportCancellation);
				}
				return fSendingActionTypeList;
			}
		}
	}

	class CusEntryHeaderMessageSendingActionCollectionForTesting : CusEntryHeaderMessageSendingActionCollection<CusEntryHeaderMessageSendingActionForTesting>
	{
		public CusEntryHeaderMessageSendingActionCollectionForTesting(CusEntryHeaderMessageSendingActionParentForTesting sendingParent) : base(sendingParent) { }
	}

	class CusEntryHeaderMessageSendingActionValidationForTesting : CusEntryHeaderMessageSendingActionValidation
	{
		public CusEntryHeaderMessageSendingActionValidationForTesting(CusEntryHeaderMessageSendingActionForTesting parent) : base(parent) { }
	}
}
