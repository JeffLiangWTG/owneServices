using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class ExportAmendmentDetailsCollection : NonPersistentBusinessObjectCollection<ExportAmendmentDetails>
	{
		public ExportAmendmentDetailsCollection(CusEntryHeader header)
			: base(header.Factory)
		{
			PopulateMessages(header);
		}
		void PopulateMessages(CusEntryHeader header)
		{
			for (var i = 2; i <= header.CH_VersionID + 1; i++)
			{
				var lastMessages = header.Messages.Cast<EDIMessage>().Where(item => item.EM_MessageType == ElectronicDocumentTypeList.Codes._5AS)
																	 .Where(item => item.EM_ApplicationReference == i.ToString())
																	 .OrderBy(x => x.EM_SystemCreateTimeUtc).LastOrDefault();

				if (lastMessages != null)
				{
					using (var textReader = lastMessages.GetEM_MessageTextReader())
					{
						var declaration5AS = KRXmlObjectSerializer.DeserializeWithoutSchemaValidation<CargoWise.Customs.KR.MessageDefinitions.GOVCBR5AS.Declaration>(textReader);
						var exportAmendmentHeader = new Export5ASHeaderCreator().Create(declaration5AS);
						var messageDetails = new ExportAmendmentMessageDetails(declaration5AS);
						var amendmentDetails = new ExportAmendmentDetails(exportAmendmentHeader, Factory, lastMessages.EM_MessageNum, header.PK, lastMessages.MessageStatus, lastMessages.MessageOrEntryStatus, messageDetails);
						amendmentDetails.Decorate(header, ZShort.ParseSafe(lastMessages.EM_ApplicationReference, 0));
						this.Add(amendmentDetails);
					}
				}
			}
		}

		//A functional code should not rely on this. This is used for binding
		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			var amendEntryHeader = new ExportAmendmentHeader();
			amendEntryHeader.AmendmentItems = System.Array.Empty<Export5ASItem>();
			return new ExportAmendmentDetails(amendEntryHeader, Factory, ZGuid.Empty);
		}
		protected override bool AllowNewCore => false;
	}
}
