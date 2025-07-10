using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.IL.Business;
using Enterprise.Customs.IL.Business.MessageProcessors;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.IL.Business.Constants;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public class Message2716Processor : ILBranchCustomsApplicationTypeMessageProcessorBase
	{
		public Message2716Processor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("DC405B3B-4351-405E-9356-132DBA0A1CB1", "IL Supporting Document Response Message");

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { ILMessageTypeList.Codes.DOC };

		protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => new[] { (ZString)ILEDIMessageSubTypeList.Codes.SupportingDocumentsResponse };

		protected override void ProcessMessageCore(EDIMessage message)
		{
			if(message is ILDOC276ResponseMessage ilDocMessage
				&& ilDocMessage.MessageDataObject is ILDOC276ResponseMessageDataObject messageDataObject)
			{
				var externalAttachmentId = messageDataObject.MessageData.ExternalAttachmentId;
				var supportingDocument = GetLinkedDocument(messageDataObject.Factory, externalAttachmentId);
				if (supportingDocument != null)
				{
					supportingDocument.CSI_Status = messageDataObject.MessageData.ResponseContentHeader.ApplicationId == 0 ? RequestedSupportingStatusList.Codes.FAL : RequestedSupportingStatusList.Codes.SNT;
					if (supportingDocument.CSI_ReferenceNumber2.IsEmpty && messageDataObject.MessageData.ResponseContentHeader.ApplicationId > 0)
					{
						supportingDocument.CSI_ReferenceNumber2 = messageDataObject.MessageData.ResponseContentHeader.ApplicationId.ToString();
					}

					UpdateMessageStatus(ilDocMessage, EDIMessage.Status.ProcessedOK);
				}
			}
		}

		SupportingDocument GetLinkedDocument(BusinessObjectFactory factory, string externalAttachmentId)
		{
			var key = ZGuid.ParseSafe(externalAttachmentId);

			var csiSubQuery = new ZDBOnlySubQuery(typeof(CusSupportingInfo), CusSupportingInfoSchema.CSI_ParentID);
			csiSubQuery.AddToFilter(CusSupportingInfoSchema.CSI_ParentTableCode, AsycudaBillSchema.Constants.Prefix);
			csiSubQuery.AddToFilter(CusSupportingInfoSchema.PK, key);

			var billSubQuery = new ZDBOnlySubQuery(typeof(AsycudaBill), AsycudaBillSchema.ABL_AMA);
			billSubQuery.AddSubQuery(csiSubQuery, JoinCondition.And);

			var headerQuery = new ZDBOnlyQuery(typeof(AsycudaManifestHeader));
			headerQuery.AddSubQuery(billSubQuery, JoinCondition.And);

			var header = factory.LoadTop1<AsycudaManifestHeader>(headerQuery);

			var bill = header?.Bills.Cast<AsycudaBill>().FirstOrDefault(s => s.SupportingDocuments.Cast<SupportingDocument>().Any(d => d.PK == key));
			var supportingDocument = bill?.SupportingDocuments.Cast<SupportingDocument>().FirstOrDefault(s => s.PK == key);
			return supportingDocument;
		}

		protected override (ZGuid BranchPK, BusinessObject LinkedObject, MultilingualString DiscardReason) TryFindLinkedObjectCore(EDIMessage message)
		{
			var ilDocMessage = message as ILDOC276ResponseMessage;
			if (ilDocMessage == null)
			{
				return (message.EM_GB, null, MessageProcessors.UnexpectedMessage);
			}

			var messageDataObject = (ILDOC276ResponseMessageDataObject)ilDocMessage.MessageDataObject;
			var externalAttachmentId = messageDataObject.MessageData.ExternalAttachmentId;
			var supportingDocument = GetLinkedDocument(messageDataObject.Factory, externalAttachmentId);
			if (supportingDocument == null)
			{
				return (message.Branch.PK, null, Constants.Message2716Processor.GetSupportingDocumentNotFound(externalAttachmentId));
			}

			var header = ((AsycudaBill)supportingDocument.Parent).Header;

			return (header.Branch.PK, header, (NoResString)ZString.Empty);
		}
	}
}
