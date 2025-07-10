using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.KR.Messaging;
using Enterprise.DocumentEngineIntegration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.KR.Business
{
	public class Import5BBDetails : NonPersistentBusinessObject, IVisualizerNoteSupporter
	{
		public Import5BBDetails(EDIMessage message5BB) : base(message5BB.Factory)
		{
			this.message5BB = message5BB;
			entryPK = message5BB.EM_LinkUniqueID;
		}
		readonly EDIMessage message5BB;
		readonly ZGuid entryPK;

		public ZInt SequenceNo => ZInt.ParseSafe(message5BB.EM_ApplicationReference, 0);
		public ZString MessageOrEntryStatus => message5BB.MessageOrEntryStatus;
		public ZString MesageOrEntryStatusDescription => Factory.GetCachedValue<CustomsMessageStatusTypeList>().GetDescriptionFromCode(MessageOrEntryStatus) ?? Factory.GetCachedValue<CustomsEntryStatusTypeList>().GetDescriptionFromCode(MessageOrEntryStatus);
		[ResourceStringData("8DC222A4-2EE4-4112-94F4-8AF8AC42C4A5", Caption = "Accepted Date")]
		public ZDate AcceptedDate => MessageR99?.EM_SystemCreateTimeUtc.ToLocalBranchTime().Date ?? ZDate.Empty;
		[ResourceStringData("4052B4BF-E41A-4D67-8777-12387416D2AB", Caption = "Review Date")]
		public ZDate ReviewDate => MessageData5BC?.ApprovalDate ?? ZDate.Empty;
		[ResourceStringData("1CAF80B6-7C03-4125-86D9-307BED489DF2", Caption = "Review Result Description", ShortCaption = "Review Result Desc.")]
		public ZString ReviewResultDescription => Factory.GetCachedValue<CustomsEntryStatusTypeList>().GetDescriptionFromCode(Message5BC?.EM_MessageOwner ?? ZString.Empty);
		[ResourceStringData("787A97CB-5CB9-4DE7-98C0-827CB680B816", Caption = "Amend Reason Description")]
		public ZString AmendReasonDescription => MessageDeclaration5BB.Reason.Value;

		CargoWise.Customs.KR.MessageDefinitions.GOVCBR5BB.Declaration MessageDeclaration5BB
		{
			get
			{
				if (messageDeclaration5BB == null)
				{
					using (var textReader = message5BB.GetEM_MessageTextReader())
					{
						messageDeclaration5BB = KRXmlObjectSerializer.DeserializeWithoutSchemaValidation<CargoWise.Customs.KR.MessageDefinitions.GOVCBR5BB.Declaration>(textReader);
					}
				}
				return messageDeclaration5BB;
			}
		}
		CargoWise.Customs.KR.MessageDefinitions.GOVCBR5BB.Declaration messageDeclaration5BB;

		EDIMessage MessageR99
		{
			get
			{
				if (messageR99 == null)
				{
					messageR99 = entryPK.GetIncomingMessage(Factory, message5BB.EM_MessageNum, ElectronicDocumentTypeList.Codes._R99);
				}
				return messageR99;
			}
		}
		EDIMessage messageR99;

		EDIMessage Message5BC
		{
			get
			{
				if (message5BC == null)
				{
					message5BC = entryPK.GetIncomingMessage(Factory, message5BB.EM_MessageNum, ElectronicDocumentTypeList.Codes._5BC);
				}
				return message5BC;
			}
		}
		EDIMessage message5BC;

		IGOVCBR5BCMessageData MessageData5BC
		{
			get
			{
				if (messageData5BC == null)
				{
					if (Message5BC != null)
					{
						using (var textReader = message5BC.GetEM_MessageTextReader())
						{
							messageData5BC = new GOVCBR5BCDataProvider().GetMessageData(textReader);
						}
					}
				}
				return messageData5BC;
			}
		}
		IGOVCBR5BCMessageData messageData5BC;

		ZGuid IVisualizerNoteSupporter.PK => entryPK;
		ZGuid IVisualizerNoteSupporter.ChildBusinessObjectPK => ZGuid.Empty;
		string IVisualizerNoteSupporter.TableCode => CusEntryHeaderSchema.Constants.Prefix;
	}
}
