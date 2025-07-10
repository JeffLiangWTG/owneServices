using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.KR.Messaging;
using Enterprise.DocumentEngineIntegration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.KR.Business
{
	public class ImportD72Details : NonPersistentBusinessObject, IVisualizerNoteSupporter
	{
		public ImportD72Details(BusinessObjectFactory factory, EDIMessage message) : base(factory)
		{
			this.message = message;
			entryPK = message.EM_LinkUniqueID;
		}
		readonly EDIMessage message;
		readonly ZGuid entryPK;

		[ResourceStringData("ImportD72Details|SequenceNo", Caption = "Version No.")]
		public ZInt SequenceNo => ZInt.ParseSafe(message.EM_ApplicationReference, 0);
		[ResourceStringData("ImportD72Details|MessageOrEntryStatus", Caption = "Message Status")]
		public ZString MessageStatus => message.MessageStatus;
		[ResourceStringData("ImportD72Details|MessageOrEntryStatusDescription", Caption = "Message Status Desc.")]
		public ZString MessageStatusDescription => Factory.GetCachedValue<CustomsMessageStatusTypeList>().GetDescriptionFromCode(MessageStatus);
		[ResourceStringData("ImportD72Details|AcceptedDate", Caption = "Accepted Date")]
		public ZDateTime AcceptedDate => MessageR99?.EM_SystemCreateTimeUtc.ToLocalBranchTime().Date ?? ZDate.Empty;
		[ResourceStringData("ImportD72Details|DecisionDate", Caption = "Review Date")]
		public ZDateTime DecisionDate => MessageDataR43?.AfterReExportScheduledDate ?? ZDate.Empty;
		[ResourceStringData("ImportD72Details|NoticeTypeDescription", Caption = "Review Result Desc.")]
		public ZString NoticeTypeDescription => Factory.GetCachedValue<CustomsEntryStatusTypeList>().GetDescriptionFromCode(MessageR43?.EM_MessageOwner ?? ZString.Empty);
		[ResourceStringData("ImportD72Details|BeforeReExportScheduledDate", Caption = "Before Re-Export Scheduled Date")]
		public ZDateTime BeforeReExportScheduledDate => ZDateTime.TryParseExact(MessageDeclarationD72.PreviousDocument.IssueDateTime, out ZDateTime beforeReExportScheduledDate, Messaging.Constants.DateFormatType.Date) ? beforeReExportScheduledDate : ZDateTime.Empty;
		[ResourceStringData("ImportD72Details|AfterReExportScheduledDate", Caption = "After Re-Export Scheduled Date")]
		public ZDateTime AfterReExportScheduledDate
		{
			get
			{
				ZDateTime result;
				if (MessageDataR43 != null)
				{
					result = MessageDataR43.AfterReExportScheduledDate;
				}
				else
				{
					result = ZDateTime.TryParseExact(MessageDeclarationD72.AdditionalInformation.LimitDateTime, out ZDateTime afterReExportScheduledDate, Messaging.Constants.DateFormatType.Date) ? afterReExportScheduledDate : ZDateTime.Empty;
				}
				return result;
			}
		}
		[ResourceStringData("ImportD72Details|ReasonDescription", Caption = "Reason Description")]
		public ZString ReasonDescription => MessageDeclarationD72.Reason.Value;

		CargoWise.Customs.KR.MessageDefinitions.GOVCBRD72.Declaration MessageDeclarationD72
		{
			get
			{
				if (messageDeclarationD72 == null)
				{
					using (var textReader = message.GetEM_MessageTextReader())
					{
						messageDeclarationD72 = KRXmlObjectSerializer.DeserializeWithoutSchemaValidation<CargoWise.Customs.KR.MessageDefinitions.GOVCBRD72.Declaration>(textReader);
					}
				}
				return messageDeclarationD72;
			}
		}
		CargoWise.Customs.KR.MessageDefinitions.GOVCBRD72.Declaration messageDeclarationD72;

		EDIMessage MessageR99
		{
			get
			{
				if (messageR99 == null)
				{
					messageR99 = entryPK.GetIncomingMessage(Factory, message.EM_MessageNum, ElectronicDocumentTypeList.Codes._R99);
				}
				return messageR99;
			}
		}
		EDIMessage messageR99;

		EDIMessage MessageR43
		{
			get
			{
				if (messageR43 == null)
				{
					messageR43 = entryPK.GetIncomingMessage(Factory, message.EM_MessageNum, ElectronicDocumentTypeList.Codes._R43);
				}
				return messageR43;
			}
		}
		EDIMessage messageR43;

		IGOVCBRR43MessageData MessageDataR43
		{
			get
			{
				if (messageDataR43 == null)
				{
					if (MessageR43 != null)
					{
						using (var textReader = messageR43.GetEM_MessageTextReader())
						{
							messageDataR43 = new GOVCBRR43DataProvider().GetMessageData(Factory, textReader);
						}
					}
				}
				return messageDataR43;
			}
		}
		IGOVCBRR43MessageData messageDataR43;

		ZGuid IVisualizerNoteSupporter.PK => entryPK;
		ZGuid IVisualizerNoteSupporter.ChildBusinessObjectPK => ZGuid.Empty;
		string IVisualizerNoteSupporter.TableCode => CusEntryHeaderSchema.Constants.Prefix;
	}
}
