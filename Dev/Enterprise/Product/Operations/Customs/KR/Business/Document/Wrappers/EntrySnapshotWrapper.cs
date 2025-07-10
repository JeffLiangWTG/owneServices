using CargoWise.EntityFramework;
using Enterprise.Customs.KR.Messaging;
using Enterprise.DocumentEngineCore.DocWrappers;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("Soon to be used")]
	public class EntrySnapshotWrapper : DocumentWrapper
	{
		public EntrySnapshotWrapper(CusEntrySnapshot snapshot, BusinessObjectFactory factory)
			: base(snapshot, factory)
		{
			this.snapshot = snapshot;
		}
		readonly CusEntrySnapshot snapshot;

		public ImportEntryHeaderWrapper ImportEntryWrapper
		{
			get
			{
				if (importEntryWrapper == null)
				{
					if (snapshot != null && snapshot.CES_MessageType == ElectronicDocumentTypeList.Codes._929)
					{
						using (var textReader = snapshot.GetCES_SnapshotXmlReader())
						{
							var header = KRXmlObjectSerializer.DeserializeWithoutSchemaValidation<ImportEntryHeader>(textReader);
							importEntryWrapper = new ImportEntryHeaderWrapper(snapshot.CES_CH_EntryHeader, header, Factory);
							importEntryWrapper.Decorate((CusEntryHeader)snapshot.EntryHeader);
						}
					}
				}
				return importEntryWrapper;
			}
		}
		ImportEntryHeaderWrapper importEntryWrapper;

		public ExportEntryHeaderWrapper ExportEntryWrapper
		{
			get
			{
				if (exportEntryWrapper == null)
				{
					if (snapshot != null && snapshot.CES_MessageType == ElectronicDocumentTypeList.Codes._830)
					{
						using (var textReader = snapshot.GetCES_SnapshotXmlReader())
						{
							var header = KRXmlObjectSerializer.DeserializeWithoutSchemaValidation<ExportEntryHeader>(textReader);
							exportEntryWrapper = new ExportEntryHeaderWrapper(snapshot.CES_CH_EntryHeader, header, Factory);
							exportEntryWrapper.Decorate((CusEntryHeader)snapshot.EntryHeader);
						}
					}
				}
				return exportEntryWrapper;
			}
		}
		ExportEntryHeaderWrapper exportEntryWrapper;

		public Import5ULHeaderWrapper DataProvider5UL
		{
			get
			{
				if (import5ULHeaderWrapper == null)
				{
					if (snapshot != null && snapshot.CES_MessageType == ElectronicDocumentTypeList.Codes._5UL)
					{
						using (var textReader = snapshot.GetCES_SnapshotXmlReader())
						{
							var header = KRXmlObjectSerializer.DeserializeWithoutSchemaValidation<Import5ULHeader>(textReader);
							import5ULHeaderWrapper = new Import5ULHeaderWrapper(snapshot.CES_CH_EntryHeader, header, Factory);
							import5ULHeaderWrapper.Decorate((CusEntryHeader)snapshot.EntryHeader);
						}
					}
				}
				return import5ULHeaderWrapper;
			}
		}
		Import5ULHeaderWrapper import5ULHeaderWrapper;

		public LocalExportEntryHeaderWrapper LocalExportEntryWrapper
		{
			get
			{
				if (localExportEntryWrapper == null)
				{
					if (snapshot != null && (snapshot.CES_MessageType == ElectronicDocumentTypeList.Codes._5DP || snapshot.CES_MessageType == ElectronicDocumentTypeList.Codes._5DQ))
					{
						using (var textReader = snapshot.GetCES_SnapshotXmlReader())
						{
							var header = KRXmlObjectSerializer.DeserializeWithoutSchemaValidation<LocalExportEntryHeader>(textReader);
							localExportEntryWrapper = new LocalExportEntryHeaderWrapper(snapshot.CES_CH_EntryHeader, snapshot.CES_MessageType, header, Factory);
							localExportEntryWrapper.Decorate((CusEntryHeader)snapshot.EntryHeader);
						}
					}
				}
				return localExportEntryWrapper;
			}
		}
		LocalExportEntryHeaderWrapper localExportEntryWrapper;

		public FTAHeaderWrapper FTAHeader
		{
			get
			{
				if (fTAHeader == null)
				{
					if (snapshot != null)
					{
						if (snapshot.CES_MessageType == ElectronicDocumentTypeList.Codes._5SC)
						{
							using (var textReader = snapshot.GetCES_SnapshotXmlReader())
							{
								var header = KRXmlObjectSerializer.DeserializeWithoutSchemaValidation<ImportFTAHeader>(textReader);
								fTAHeader = new FTAHeaderWrapper(header, Factory);
							}
						}
						else if (snapshot.CES_MessageType == ElectronicDocumentTypeList.Codes._DHR)
						{
							using (var textReader = snapshot.GetCES_SnapshotXmlReader())
							{
								var header = KRXmlObjectSerializer.DeserializeWithoutSchemaValidation<ImportDHRHeader>(textReader);
								fTAHeader = new FTAHeaderWrapper(header, Factory);
							}
						}
					}
				}
				return fTAHeader;
			}
		}
		FTAHeaderWrapper fTAHeader;
	}
}
