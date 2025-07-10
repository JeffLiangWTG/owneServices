using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.AccumulativeAmendment;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public class GOVCBR5ASExtendOfPeriodSender : MessageSender<CusEntryHeader, ExportAmendmentHeader>
	{
		public GOVCBR5ASExtendOfPeriodSender(IEnumerable<JobDeclarationMiscMessageSendingObject> sendingObjects, BusinessObjectFactory factory)
			: base(sendingObjects.Select(x => x.Header), factory)
		{
			this.sendingObjects = sendingObjects;
		}

		readonly IEnumerable<JobDeclarationMiscMessageSendingObject> sendingObjects;

		public override ZString MessageType => ElectronicDocumentTypeList.Codes._5AS;

		protected override IEnumerable<EDIMessage> CreateMessages(CusEntryHeader parent)
		{
			var messages = base.CreateMessages(parent);
			var message = messages.First();
			message.EM_MessageSubType = _5ASAmendmentType.Codes.Extension;
			message.RecordVersionNumberFromCH_VersionID(parent);
			return messages;
		}

		protected override ExportAmendmentHeader GetMessageDataProvider(CusEntryHeader parent, ZString messageID)
		{
			var sendingObject = GetSendingObject(parent.EntryNumber);
			var amendedItem = AmendedItemGenerator.Generate(sendingObject);
			return new Export5ASHeaderCreator().Create(parent, new AmendedItem[] { amendedItem });
		}

		protected override IMessageBuilder GetMessageBuilder(ExportAmendmentHeader messageDataProvider)
		{
			return new GOVCBR5ASMessageBuilder(messageDataProvider, GetSendingObject(messageDataProvider.ExportDeclarationNumber), MessageFunctions.MessageFunctionCode.Extend);
		}

		JobDeclarationMiscMessageSendingObject GetSendingObject(ZString entryNumber)
		{
			return sendingObjects.First(x => x.EntryNumber == entryNumber);
		}
		protected override void UpdateMessageStatus(CusEntryHeader entry)
		{
			entry.CH_Status = CustomsMessageStatusTypeList.Codes.AmendmentSent;
		}

		static class AmendedItemGenerator
		{
			public static AmendedItem Generate(JobDeclarationMiscMessageSendingObject sendingObject)
			{
				var result = new AmendedItem();
				result.AmendType = EntityAmendType.Update;
				result.EntityType = nameof(IExportEntryHeader);
				result.BeforeValue = sendingObject.CurrentDate.ToString(DateFormatType.Date);
				result.AfterValue = sendingObject.NewDate.ToString(DateFormatType.Date);
				result.DataItemID = ExportAmendmentDataItemIDList.Codes.A608;
				result.DataItemDescription = ExportAmendmentDataItemIDList.Descriptions.A608;
				return result;
			}
		}
	}
}
