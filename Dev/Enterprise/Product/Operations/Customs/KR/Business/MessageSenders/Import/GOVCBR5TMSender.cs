using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class GOVCBR5TMSender : MessageSender<CusEntryHeader, Import5TMHeader>
	{
		public GOVCBR5TMSender(IEnumerable<JobDeclarationMiscMessageSendingObject> sendingObjects, BusinessObjectFactory factory)
			: base(sendingObjects.Select(x => x.Header), factory)
		{
			this.sendingObjects = sendingObjects;
		}
		readonly IEnumerable<JobDeclarationMiscMessageSendingObject> sendingObjects;

		public override ZString MessageType => ElectronicDocumentTypeList.Codes._5TM;
		protected override IMessageBuilder GetMessageBuilder(Import5TMHeader dataProvider) => new GOVCBR5TMMessageBuilder(dataProvider);
		protected override Import5TMHeader GetMessageDataProvider(CusEntryHeader parent, ZString messageID) => new Import5TMHeaderCreator().Create(parent, GetSendingObject(parent.EntryNumber));
		JobDeclarationMiscMessageSendingObject GetSendingObject(ZString entryNumber) => sendingObjects.First(x => x.EntryNumber == entryNumber);

		protected override void UpdateMessageStatus(CusEntryHeader entry)
		{
			var cusEntryNum = entry.EntryNumbers.GetOrCreateCusEntryNum(MessageType);
			cusEntryNum.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalSent;
		}

		protected override void OnSentCore(CusEntryHeader parent)
		{
			base.OnSentCore(parent);
			foreach (var entryLine in parent.MergedLines)
			{
				entryLine.CL_IsGoldOrItsProduct = false;
			}

			foreach (MessageSendingEntryLineObject lineObject in GetSendingObject(parent.EntryNumber).MessageSendingEntryLines)
			{
				var entryLine = parent.MergedLines.FirstOrDefault(x => x.CL_LineNumber == lineObject.EntryLineNo);
				if (entryLine != null)
				{
					entryLine.CL_IsGoldOrItsProduct = lineObject.IsGoldOrItsProduct;
				}
			}
		}
	}
}
