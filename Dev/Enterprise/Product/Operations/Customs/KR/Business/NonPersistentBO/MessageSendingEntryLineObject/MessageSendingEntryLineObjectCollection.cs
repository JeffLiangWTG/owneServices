using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class MessageSendingEntryLineObjectCollection : NonPersistentBusinessObjectCollection<MessageSendingEntryLineObject>
	{
		public MessageSendingEntryLineObjectCollection(CusEntryHeader header) : base(header.Factory)
		{
			this.header = header;
		}
		readonly CusEntryHeader header;

		public void PopulateElementsFromMergedLines(Func<CusEntryLine, bool> entryLineFilter, ZString messageType)
		{
			foreach (var matchedEntryLine in header.MergedLines.Where(x => entryLineFilter(x)))
			{
				var sendingEntryLineObject = new MessageSendingEntryLineObject(messageType, header);
				sendingEntryLineObject.DecorateFromCusEntryLine(matchedEntryLine, messageType);
				Add(sendingEntryLineObject);
			}
		}

		public void PopulateElementsFrom5FNMessages(ImportEntryHeader importEntryHeader)
		{
			var entryNumbers = header.EntryNumbers.Where(x => x.CE_EntryType == ElectronicDocumentTypeList.Codes._5FN).OrderBy(x => x.CE_EntryLineReference);

			foreach (var entryNum in entryNumbers)
			{
				var message = header.Messages.Cast<EDIMessage>().Where(item => item.EM_MessageType == ElectronicDocumentTypeList.Codes._5FN)
																	.Where(item => item.EM_ApplicationReference == entryNum.CE_EntryLineReference)
																	.OrderBy(x => x.EM_SystemCreateTimeUtc).LastOrDefault();

				var sendingEntryLineObject = new MessageSendingEntryLineObject(Factory);
				sendingEntryLineObject.DecorateFrom5FNCusEntryLine(entryNum, message);

				if (importEntryHeader != null)
				{
					sendingEntryLineObject.DecorateFromImportEntryHeader(importEntryHeader);
				}
				Add(sendingEntryLineObject);
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject() => new MessageSendingEntryLineObject(Factory);
		protected override bool AllowNewCore => false;
	}
}
