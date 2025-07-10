using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class GOVCBR5UASender : MessageSender<CusEntryHeader, Import5UAHeader>
	{
		public GOVCBR5UASender(IEnumerable<PenaltyExemptionRequestMessageSendingObject> sendingObjects, BusinessObjectFactory factory)
			: base(sendingObjects.Select(x => x.Header).Distinct(), factory)
		{
			this.sendingObjects = sendingObjects;
			var declaration = sendingObjects.Select(x => x.Header.Declaration).First();
			declaration.SetValidationModeOnElectronicMessaging(MessageType);
		}
		readonly IEnumerable<PenaltyExemptionRequestMessageSendingObject> sendingObjects;

		public override ZString MessageType => ElectronicDocumentTypeList.Codes._5UA;

		protected override IMessageBuilder GetMessageBuilder(Import5UAHeader messageDataProvider)
		{
			return new GOVCBR5UAMessageBuilder(messageDataProvider);
		}

		protected override void OnSentCore(CusEntryHeader parent)
		{
			base.OnSentCore(parent);
			parent.Declaration.RemoveValidationModeOnElectronicMessaging(MessageType);
		}

		protected override Import5UAHeader GetMessageDataProvider(CusEntryHeader parent, ZString messageID)
		{
			var sendingObject = GetSendingObject(parent, messageID);
			return new Import5UAHeaderCreator().Create(parent, sendingObject, sendingObject.AmendmentDeclarationDate.ToDateTime(), sendingObject.AmendmentVersion, sendingObject.PenaltyType);
		}

		PenaltyExemptionRequestMessageSendingObject GetSendingObject(CusEntryHeader parent, string versionNumber5UA)
		{
			ZShort.TryParse(versionNumber5UA, out ZShort versionNumber5UANumeric);
			return sendingObjects.First(x => x.EntryNumber == parent.EntryNumber && versionNumber5UANumeric == x.DutyPenaltyExemption5UASequenceNumber);
		}

		protected override ZString GetMessageID(EDIMessage message) => message.EM_ApplicationReference;

		protected override IEnumerable<EDIMessage> CreateMessages(CusEntryHeader parent)
		{
			var messages = new List<EDIMessage>();
			foreach (var sendingObject in sendingObjects.Where(x => x.Header.PK == parent.PK))
			{
				var message = Factory.New<EDIMessage>();
				message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				message.EM_MessageType = MessageType;
				message.EM_ApplicationReference = (sendingObject.DutyPenaltyExemption5UASequenceNumber).ToString();
				message.EM_MessageOwner = sendingObject.Message5FE.EM_MessageNum.Left(EDIMessage.Schema.EM_MessageOwnerMaxLength);
				messages.Add(message);
			}
			return messages;
		}

		protected override void UpdateMessageStatus(CusEntryHeader parent)
		{
			foreach (var sendingObject in sendingObjects)
			{
				var entryNum = parent.EntryNumbers.AddNew();
				entryNum.CE_EntryType = MessageType;
				entryNum.CE_EntryLineReference = (sendingObject.DutyPenaltyExemption5UASequenceNumber).ToString();
				entryNum.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalSent;
			}
		}
	}
}
