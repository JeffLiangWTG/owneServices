using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public partial class CusEntrySnapshot : Customs.Business.CusEntrySnapshot
	{
		public CusEntrySnapshot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public IMessageDataProvider DataProviderObject
		{
			get
			{
				if (dataProviderObject == null)
				{
					using (var textReader = GetCES_SnapshotXmlReader())
					{
						if (textReader.Peek() >= 0)
						{
							switch (CES_MessageType)
							{
								case ElectronicDocumentTypeList.Codes._830:
									dataProviderObject = KRXmlObjectSerializer.DeserializeWithoutSchemaValidation<ExportEntryHeader>(textReader);
									break;
								case ElectronicDocumentTypeList.Codes._929:
									dataProviderObject = KRXmlObjectSerializer.DeserializeWithoutSchemaValidation<ImportEntryHeader>(textReader);
									break;
								case ElectronicDocumentTypeList.Codes._5DP:
								case ElectronicDocumentTypeList.Codes._5DQ:
									dataProviderObject = KRXmlObjectSerializer.DeserializeWithoutSchemaValidation<LocalExportEntryHeader>(textReader);
									break;
								case ElectronicDocumentTypeList.Codes._DHR:
									dataProviderObject = KRXmlObjectSerializer.DeserializeWithoutSchemaValidation<ImportDHRHeader>(textReader);
									break;
								case ElectronicDocumentTypeList.Codes._5SC:
									dataProviderObject = KRXmlObjectSerializer.DeserializeWithoutSchemaValidation<ImportFTAHeader>(textReader);
									break;
								case ElectronicDocumentTypeList.Codes._5BA:
									dataProviderObject = KRXmlObjectSerializer.DeserializeWithoutSchemaValidation<Import5BAHeader>(textReader);
									break;
							}
						}
					}
				}
				return dataProviderObject;
			}
		}

		IMessageDataProvider dataProviderObject;
	}
}
