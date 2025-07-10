using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class ImportAmendmentDetailsCollection : NonPersistentBusinessObjectCollection<ImportAmendmentDetails>
	{
		public ImportAmendmentDetailsCollection(CusEntryHeader header)
			: base(header.Factory)
		{
			PopulateMessages(header);
		}

		void PopulateMessages(CusEntryHeader header)
		{
			var orderedMessages = header.Messages.Cast<EDIMessage>().Where(item => item.EM_MessageType == ElectronicDocumentTypeList.Codes._5FE).OrderBy(x => x.EM_SystemCreateTimeUtc);
			foreach (var message in orderedMessages)
			{
				using (var textReader = message.GetEM_MessageTextReader())
				{
					var declaration5FE = KRXmlObjectSerializer.DeserializeWithoutSchemaValidation<CargoWise.Customs.KR.MessageDefinitions.GOVCBR5FE.Declaration>(textReader);
					var importAmendmentHeader = new Import5FECreator().Create(declaration5FE);
					var messageDetails = new ImportAmendmentMessageDetails(declaration5FE);
					var amendmentDetails = new ImportAmendmentDetails(importAmendmentHeader, Factory, message.EM_MessageNum, header.PK, message.MessageStatus, messageDetails);
					this.Add(amendmentDetails);
				}
			}
		}

		//A functional code should not rely on this. This is used for binding
		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			var amendEntryHeader = new Import5FEHeader();
			amendEntryHeader.AmendedItems = System.Array.Empty<Import5FEItem>();
			return new ImportAmendmentDetails(amendEntryHeader, Factory);
		}

		protected override bool AllowNewCore => false;
	}
}
