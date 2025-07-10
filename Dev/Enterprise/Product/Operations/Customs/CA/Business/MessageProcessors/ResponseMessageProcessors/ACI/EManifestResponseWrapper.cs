namespace Enterprise.Customs.CA.Business.MessageProcessors
{
	using System.Collections.Generic;
	using System.Linq;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Customs.Common.MessageBuilders;
	using Enterprise.ZArchitecture.Schema;
	using static Enterprise.Customs.CA.Business.GOVCBRMessageWrapper;

	public class EManifestResponseWrapper
	{
		public EManifestResponseWrapper(EDIMessage message)
		{
			this.WrappedMessage = message;
			this.gOVCBRMessageWrapper = GOVCBRMessageProvider.GetMessageWrapper(message);
		}
		readonly GOVCBRMessageWrapper gOVCBRMessageWrapper;

		internal ZBool HasGOVCBRMessage => gOVCBRMessageWrapper.HasGOVCBRMessage;

		internal ZString DocumentName => gOVCBRMessageWrapper.DocumentName;

		internal ZString DocumentReference => gOVCBRMessageWrapper.DocumentReference;

		internal ZDateTime ProcessingDate => gOVCBRMessageWrapper.ProcessingDate;

		internal ZString OriginalMessageReference => gOVCBRMessageWrapper.OriginalMessageReference;

		internal IEnumerable<string> ErrorComments => gOVCBRMessageWrapper.ErrorComments;

		internal IEnumerable<Notification> Notifications => gOVCBRMessageWrapper.Notifications;

		internal virtual ZBool IsMessageReceived => gOVCBRMessageWrapper.IsMessageReceived;

		internal bool IsSyntaxError => DocumentName == ServiceOptions.Codes.GenResponse && gOVCBRMessageWrapper.IsErrorMessage;

		internal bool IsAccepted => gOVCBRMessageWrapper.IsMessageContentAccepted || gOVCBRMessageWrapper.IsMessageContentAcceptedWithComments;

		internal bool IsRejected => gOVCBRMessageWrapper.IsErrorMessage || gOVCBRMessageWrapper.IsMessageContentRejectedWithComment;

		internal bool IsBatchOrDataError => ErrorComments.Count() == 1 && (ErrorComments.First().StartsWith("28-") || ErrorComments.First().StartsWith("29-"));

		internal bool IsMatchedNotice => NoticeStatusCode == "0001";

		internal bool IsNOTMatchedNotice => NoticeStatusCode == "0002";

		internal ZString NoticeStatusCode => gOVCBRMessageWrapper.NoticeStatusCode;

		public IEDIFACTMessageAttachee LinkedObject => linkedObject ?? (linkedObject = GetLinkedObject());
		IEDIFACTMessageAttachee linkedObject;

		IEDIFACTMessageAttachee GetLinkedObject()
		{
			var factory = WrappedMessage.Factory;
			if (OriginalMessageReference.Length > 4)
			{
				ZString prefix = OriginalMessageReference.Left(4);
				ZString number = OriginalMessageReference.Substring(4);
				if (prefix == CusCAeMHHouse.JobIdentificationPrefix)
				{
					return factory.LoadTop1<CusCAeMHHouse>(new ZQuery(CusCAeMHHouseSchema.BW_MessageReference, number));
				}
				else if (prefix == CusCAeMHMaster.JobIdentificationPrefix)
				{
					return factory.LoadTop1<CusCAeMHMaster>(new ZQuery(CusCAeMHMasterSchema.BP_MessageReference, number));
				}
			}
			if (!DocumentReference.IsEmpty)
			{
				return ImportLinkedObjectManager.GetCusEntryHeaderByTransactionNumber(factory, DocumentReference, new[] { MessageTypeList.Codes.EDIRelease }) ?? ImportLinkedObjectManager.GetCusEntryHeaderByCargoControlNumber(factory, DocumentReference, MessageTypeList.Codes.EDIRelease);
			}
			return null;
		}

		public EDIMessage WrappedMessage { get; private set; }
	}
}
