using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.IL;
using Enterprise.Customs.IL.Business;
using Enterprise.Customs.IL.Business.MessageProcessors;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.IL.Business.Constants;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public class Message828Processor : ILBranchCustomsApplicationTypeMessageProcessorBase
	{
		public Message828Processor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("3EF9D946-0B36-423C-AB29-381071F23EB5", "IL Supporting Document Request Decision Response Message");

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { ILMessageTypeList.Codes.DOC };

		protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => new[] { (ZString)ILEDIMessageSubTypeList.Codes.SupportingDocumentsRqDecisionResponse };

		protected override void ProcessMessageCore(EDIMessage message)
		{
			if (message is not ILDOC828ResponseMessage ilDocMessage
				|| ilDocMessage.MessageDataObject is not ILDOC828ResponseMessageDataObject messageDataObject)
			{
				return;
			}

			var referenceNumber2 = messageDataObject.MessageData.GeneralDetails?.DocumentId;
			if (referenceNumber2 == null)
			{
				UpdateMessageStatus(ilDocMessage, EDIMessage.Status.Discarded, noteText: Res.GetString("89290BA5-D416-4BF6-984B-4636F0179C09", "DocumentId is null in this message"));
				return;
			}

			var supportingDocuments = GetLinkedDocument(messageDataObject.Factory, referenceNumber2.ToString(), message.EM_GC);

			if (supportingDocuments == null || supportingDocuments.Length == 0)
			{
				UpdateMessageStatus(ilDocMessage, EDIMessage.Status.Discarded, noteText: Res.GetString("867ED162-161F-41F0-BF43-7375ACB4870E", "Couldn't locate document with reference ") + $"{referenceNumber2}");
				return;
			}

			if (supportingDocuments.Length > 1)
			{
				UpdateMessageStatus(ilDocMessage, EDIMessage.Status.Discarded, noteText: Res.GetString("3E17FF76-51B8-41CD-8694-2A9734BB909E", "Find more than 1 supporting document with reference ") + $"{referenceNumber2}");
				return;
			}

			var supportingDocument = supportingDocuments[0];
			var status = messageDataObject.MessageData.VerificationDecision?.VerificationDecisionType ?? 1;
			UpdateSupportingDocumentStatus(supportingDocument, status);
			UpdateMessageStatus(ilDocMessage, EDIMessage.Status.ProcessedOK);
		}

		protected override (ZGuid BranchPK, BusinessObject LinkedObject, MultilingualString DiscardReason) TryFindLinkedObjectCore(EDIMessage message)
		{
			var iLDoc828ResponseMessage = message as ILDOC828ResponseMessage;
			if (iLDoc828ResponseMessage == null)
			{
				return (message.EM_GB, null, MessageProcessors.UnexpectedMessage);
			}

			var messageDataObject = iLDoc828ResponseMessage.MessageDataObject as ILDOC828ResponseMessageDataObject;
			var referenceNumber2 = messageDataObject.MessageData.GeneralDetails?.DocumentId;
			if (referenceNumber2 == null)
			{
				return (message.EM_GB, null, Constants.Message828Processor.DocumentIDIsNull);
			}

			var supportingDocuments = GetLinkedDocument(messageDataObject.Factory, referenceNumber2.ToString(), message.EM_GC);
			if (supportingDocuments.Length == 0)
			{
				return (message.EM_GB, null, Constants.Message828Processor.GetSupportingDocumentNotFound(referenceNumber2.ToString()));
			}

			var supportingDocument = supportingDocuments[0];
			var header = ((AsycudaBill)supportingDocument.Parent).Header;

			return (header.Branch.PK, header, (NoResString)ZString.Empty);
		}

		SupportingDocument[] GetLinkedDocument(BusinessObjectFactory factory, string referenceNumber2, ZGuid companyPK)
		{
			var branchSubQuery = new ZDBOnlySubQuery(typeof(GlbBranch), GlbBranchSchema.PK);
			branchSubQuery.AddToFilter(GlbBranchSchema.GB_GC, companyPK);

			var headerQuery = new ZDBOnlySubQuery(typeof(AsycudaManifestHeader), AsycudaManifestHeaderSchema.AMA_ClusterKey);
			headerQuery.AddSubQuery(AsycudaManifestHeaderSchema.AMA_GB, branchSubQuery, JoinCondition.And);

			var arrivalAsycudaBillQuery = new ZDBOnlySubQuery(typeof(AsycudaBill), CusSupportingInfoSchema.CSI_ParentID);
			arrivalAsycudaBillQuery.AddToFilter(AsycudaBillSchema.ABL_BolType, SQLComparisonOperator.Equal, AsycudaBillKindList.Codes.HWB);
			arrivalAsycudaBillQuery.AddSubQuery(AsycudaBillSchema.ABL_ClusterKey, headerQuery, JoinCondition.And);

			var query = new ZDBOnlyQuery(typeof(CusSupportingInfo));
			query.AddToFilter(CusSupportingInfoSchema.CSI_Type, CusSupportingInfoTypeList.Codes.SupportingDocument);
			query.AddToFilter(CusSupportingInfoSchema.CSI_ParentTableCode, AsycudaBillSchema.Constants.Prefix);
			query.AddToFilter(CusSupportingInfoSchema.CSI_ReferenceNumber2, referenceNumber2);
			query.AddSubQuery(arrivalAsycudaBillQuery, JoinCondition.And);

			var supportingDocuments = factory.Load<SupportingDocument>(query);
			return supportingDocuments;
		}

		void UpdateSupportingDocumentStatus(SupportingDocument supportingDocument, int status)
		{
			if (supportingDocument != null)
			{
				switch (status.ToString())
				{
					case Constants.AsycudaSupportingDocument.StatusCode.Verified:
					case Constants.AsycudaSupportingDocument.StatusCode.VerifiedWithClient:
					case Constants.AsycudaSupportingDocument.StatusCode.AutoVerified:
						supportingDocument.CSI_Status = RequestedSupportingStatusList.Codes.VAL;
						break;
					case Constants.AsycudaSupportingDocument.StatusCode.Reject:
						supportingDocument.CSI_Status = RequestedSupportingStatusList.Codes.REJ;
						break;
					default:
						break;
				}
			}
		}
	}
}
