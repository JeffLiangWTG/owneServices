using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class GOVCBR934Sender : CusEntryHeaderOriginalMessageSender<Import934Header>
	{
		public GOVCBR934Sender(IEnumerable<JobDeclarationMiscMessageSendingObject> sendingObjects, BusinessObjectFactory factory)
			: base(sendingObjects.Select(x => x.Header), factory)
		{
		}

		public override ZString MessageType => ElectronicDocumentTypeList.Codes._934;

		protected override IMessageBuilder GetMessageBuilder(Import934Header messageDataProvider) => new GOVCBR934MessageBuilder(messageDataProvider, MessageFunctions.MessageFunctionCode.Original);

		protected override Import934Header GetMessageDataProvider(CusEntryHeader parent, ZString messageID) => new Import934HeaderCreator().Create(parent);

		protected override void UpdateMessageStatus(CusEntryHeader parent)
		{
			var entryNum = parent.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._934);
			entryNum.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalSent;
		}
	}
}
