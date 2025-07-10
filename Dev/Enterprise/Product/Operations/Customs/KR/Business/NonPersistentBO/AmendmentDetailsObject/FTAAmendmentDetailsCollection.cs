using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class FTAAmendmentDetailsCollection : NonPersistentBusinessObjectCollection<FTAAmendmentDetails>
	{
		public FTAAmendmentDetailsCollection(CusEntryHeader header)
			: base(header.Factory)
		{
			var originalFTAType = header.GetOriginalFTAType();
			if (originalFTAType == ElectronicDocumentTypeList.Codes._DHR)
			{
				AddCollectionForMessageData<CargoWise.Customs.KR.MessageDefinitions.GOVCBRDHS.Declaration>(header, originalFTAType, ElectronicDocumentTypeList.Codes._DHS);
			}
			else
			{
				AddCollectionForMessageData<CargoWise.Customs.KR.MessageDefinitions.GOVCBR105.Declaration>(header, originalFTAType, ElectronicDocumentTypeList.Codes._105);
			}
		}

		void AddCollectionForMessageData<T>(CusEntryHeader entry, ZString entryType, ZString messageType)
		{
			var entryNum = entry.EntryNumbers.Cast<CusEntryNumber>().FirstOrDefault(x => x.CE_EntryType == entryType);
			if (entryNum != null)
			{
				for (var i = 2; i <= ZInt.ParseSafe(entryNum.CE_EntryLineReference, 0) + 1; i++)
				{
					var lastMessages = entry.Messages.Cast<EDIMessage>().Where(item => item.EM_MessageType == messageType)
																	 .Where(item => item.EM_ApplicationReference == i.ToString())
																	 .OrderBy(x => x.EM_SystemCreateTimeUtc).LastOrDefault();
					if (lastMessages != null)
					{
						using (var textReader = lastMessages.GetEM_MessageTextReader())
						{
							if (lastMessages.EM_MessageType == ElectronicDocumentTypeList.Codes._DHS)
							{
								var declaration = KRXmlObjectSerializer.DeserializeWithoutSchemaValidation<T>(textReader) as CargoWise.Customs.KR.MessageDefinitions.GOVCBRDHS.Declaration;
								var ftaAmendmentHeader = new ImportFTAAmendmentHeaderCreator().Create(declaration);
								var messageDetails = new FTAAmendmentMessageDetails(declaration);
								var amendmentDetails = new FTAAmendmentDetails(ftaAmendmentHeader, Factory, lastMessages.EM_MessageNum, entry.PK, lastMessages.MessageStatus, messageDetails);
								this.Add(amendmentDetails);
							}
							else
							{
								var declaration = KRXmlObjectSerializer.DeserializeWithoutSchemaValidation<T>(textReader) as CargoWise.Customs.KR.MessageDefinitions.GOVCBR105.Declaration;
								var ftaAmendmentHeader = new ImportFTAAmendmentHeaderCreator().Create(declaration);
								var messageDetails = new FTAAmendmentMessageDetails(declaration);
								var amendmentDetails = new FTAAmendmentDetails(ftaAmendmentHeader, Factory, lastMessages.EM_MessageNum, entry.PK, lastMessages.MessageStatus, messageDetails);
								this.Add(amendmentDetails);
							}
						}
					}
				}
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject() => throw new System.NotImplementedException();

		protected override bool AllowNewCore => false;
	}
}
