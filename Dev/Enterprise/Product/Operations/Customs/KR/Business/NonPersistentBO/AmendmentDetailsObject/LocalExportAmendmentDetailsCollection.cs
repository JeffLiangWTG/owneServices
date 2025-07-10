using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class LocalExportAmendmentDetailsCollection : NonPersistentBusinessObjectCollection<LocalExportAmendmentDetails>
	{
		public LocalExportAmendmentDetailsCollection(CusEntryHeader header)
			: base(header.Factory)
		{
			PopulateMessages(header);
		}
		void PopulateMessages(CusEntryHeader header)
		{
			for (var i = 2; i <= header.CH_VersionID + 1; i++)
			{
				EDIMessage lastMessage = (EDIMessage)header.Messages.Where(x => x.EM_MessageType == ElectronicDocumentTypeList.Codes._5DS || x.EM_MessageType == ElectronicDocumentTypeList.Codes._5DR)
																	.Where(x => x.EM_ApplicationReference == i.ToString())
																	.OrderBy(x => x.EM_SystemCreateTimeUtc).LastOrDefault();

				if (lastMessage != null)
				{
					using (var textReader = lastMessage.GetEM_MessageTextReader())
					{
						if (lastMessage.EM_MessageType == ElectronicDocumentTypeList.Codes._5DR)
						{
							var declaration5DR = KRXmlObjectSerializer.DeserializeWithoutSchemaValidation<CargoWise.Customs.KR.MessageDefinitions.GOVCBR5DR.Declaration>(textReader);
							var localExportAmendmentHeader = new LocalExportAmendmentHeaderCreator().Create(declaration5DR);
							var messageDetails = new LocalExportAmendmentMessageDetails(declaration5DR);
							var amendmentDetails = new LocalExportAmendmentDetails(localExportAmendmentHeader, Factory, lastMessage.EM_MessageNum, header.PK, lastMessage.MessageStatus, messageDetails);
							amendmentDetails.Decorate(header, ZShort.ParseSafe(lastMessage.EM_ApplicationReference, 0), ElectronicDocumentTypeList.Codes._5DP);
							this.Add(amendmentDetails);
						}
						else
						{
							var declaration5DS = KRXmlObjectSerializer.DeserializeWithoutSchemaValidation<CargoWise.Customs.KR.MessageDefinitions.GOVCBR5DS.Declaration>(textReader);
							var localExportAmendmentHeader = new LocalExportAmendmentHeaderCreator().Create(declaration5DS);
							var messageDetails = new LocalExportAmendmentMessageDetails(declaration5DS);
							var amendmentDetails = new LocalExportAmendmentDetails(localExportAmendmentHeader, Factory, lastMessage.EM_MessageNum, header.PK, lastMessage.MessageStatus, messageDetails);
							amendmentDetails.Decorate(header, ZShort.ParseSafe(lastMessage.EM_ApplicationReference, 0), ElectronicDocumentTypeList.Codes._5DQ);
							this.Add(amendmentDetails);
						}
					}
				}
			}
		}

		//A functional code should not rely on this. This is used for binding
		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			var amendEntryHeader = new LocalExportAmendEntryHeader();
			amendEntryHeader.AmendedItems = System.Array.Empty<LocalExportAmendItem>();
			return new LocalExportAmendmentDetails(amendEntryHeader, Factory);
		}

		protected override bool AllowNewCore => false;
	}
}
