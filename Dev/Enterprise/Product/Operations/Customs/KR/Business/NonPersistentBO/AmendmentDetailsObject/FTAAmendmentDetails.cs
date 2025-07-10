using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.KR.Messaging;
using Enterprise.DocumentEngineIntegration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.KR.Business
{
	public class FTAAmendmentDetails : NonPersistentBusinessObject, IVisualizerNoteSupporter
	{
		public FTAAmendmentDetails(IImportFTAAmendmentHeader header, BusinessObjectFactory factory, ZString messageNum, ZGuid entryPK, ZString messageStatus, FTAAmendmentMessageDetails amendmentDetails)
			: base(factory)
		{
			this.messageNum = messageNum;
			this.entryPK = entryPK;
			this.amendmentDetails = amendmentDetails;
			MessageStatus = messageStatus;
		}
		readonly ZString messageNum;
		readonly ZGuid entryPK;
		readonly FTAAmendmentMessageDetails amendmentDetails;

		[ResourceStringData("818469BD-B583-4264-AC78-3F0E3F27EEEB", Caption = "Version No.")]
		public ZInt SequenceNo => amendmentDetails.AmendmentVersionNo;
		[ResourceStringData("286ED4B8-8884-4E9B-A972-D94084CFAF97", Caption = "Message Status")]
		public ZString MessageStatus { get; }
		[ResourceStringData("DC6AD1EB-9817-4EDB-A63F-E3F478DA11BF", Caption = "Message Status Description", ShortCaption = "Message Status Desc.")]
		public ZString MessageStatusDescription => Factory.GetCachedValue<CustomsMessageStatusTypeList>().GetDescriptionFromCode(MessageStatus);
		[ResourceStringData("A8E5EF93-106B-4BE3-A593-5776B1F8EEDA", Caption = "Accepted Date")]
		public ZDateTime AcceptedDate => MessageDataR99?.NoticeDateTime ?? ZDateTime.Empty;
		[ResourceStringData("1E00DCC4-B9A4-46CC-B1DB-0D6F8239A68F", Caption = "Review Date")]
		public ZDateTime DecisionDate => MessageData106?.ApprovalDate ?? ZDateTime.Empty;
		[ResourceStringData("BAB2E20B-C012-4F6F-8F13-CC2682164252", Caption = "Review Result Description", ShortCaption = "Review Result Desc.")]
		public ZString NoticeTypeDescription => Factory.GetCachedValue<CustomsEntryStatusTypeList>().GetDescriptionFromCode(Message106?.EM_MessageOwner ?? ZString.Empty);
		[ResourceStringData("4F647032-80AA-434D-81BC-2120A942C780", Caption = "Amendment Reason")]
		public ZString AmendmentReason => amendmentDetails?.AmendReasonDescription ?? ZString.Empty;
		[ResourceStringData("8A7CB9F8-4320-4CB6-B8FD-BA800BEF2505", Caption = "Amendment Type")]
		public ZString AmendmentType => amendmentDetails?.AmendmentType ?? ZString.Empty;

		IGOVCBRR99MessageData MessageDataR99
		{
			get
			{
				if (messageDataR99 == null && entryPK.IsValid)
				{
					var messageR99 = entryPK.GetIncomingMessage(Factory, messageNum, ElectronicDocumentTypeList.Codes._R99);
					if (messageR99 != null)
					{
						using (var textReader = messageR99.GetEM_MessageTextReader())
						{
							messageDataR99 = new GOVCBRR99DataProvider().GetMessageData(textReader);
						}
					}
				}
				return messageDataR99;
			}
		}
		IGOVCBRR99MessageData messageDataR99;

		EDIMessage Message106
		{
			get
			{
				if (message106 == null && entryPK.IsValid)
				{
					message106 = entryPK.GetIncomingMessage(Factory, messageNum, ElectronicDocumentTypeList.Codes._106);
				}
				return message106;
			}
		}
		EDIMessage message106;
		IGOVCBR106MessageData MessageData106
		{
			get
			{
				if (messageData106 == null)
				{
					if (Message106 != null)
					{
						using (var textReader = message106.GetEM_MessageTextReader())
						{
							messageData106 = new GOVCBR106DataProvider().GetMessageData(textReader);
						}
					}
				}
				return messageData106;
			}
		}
		IGOVCBR106MessageData messageData106;

		ZGuid IVisualizerNoteSupporter.PK => entryPK;
		ZGuid IVisualizerNoteSupporter.ChildBusinessObjectPK => ZGuid.Empty;
		string IVisualizerNoteSupporter.TableCode => CusEntryHeaderSchema.Constants.Prefix;
	}
}
