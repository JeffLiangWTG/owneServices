using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Customs.KR.Business
{
	public class EntryDocumentWrapper : DocumentWrapper
	{
		public EntryDocumentWrapper(CusEntryHeader entry, BusinessObjectFactory factory)
			: base(entry, factory)
		{
			Argument.NotNull(entry, "entry");
			this.entry = entry;
			declaration = entry.Declaration;
		}

		readonly CusEntryHeader entry;
		readonly JobDeclaration declaration;

		public CusEntryHeader Entry => entry;
		public ZString FormattedEntryNumber => MessageFunctions.DeclarationNumberFormat(entry.EntryNumber);
		public ZString CustomsDepartmentName => MessageFunctions.GetCustomsDepartment(Factory, declaration.JE_CustomsDivision);
		public ZBool IsImportCancellationDeclinedByCustoms => GetImportCancellationDeclinedByCustoms(entry);
		public ZString ImporterTypeOfBusiness
		{
			get
			{
				if (!importerTypeOfBusinessCached.HasValue)
				{
					importerTypeOfBusinessCached = OrgHeaderWrapper.New(entry.RandomHeader.Buyer)?.ZO_TypeOfBusiness ?? ZString.Empty;
				}
				return importerTypeOfBusinessCached.Value;
			}
		}
		ZString? importerTypeOfBusinessCached;

		public EDIMessageWrapperCollection Messages => messages ?? (messages = new EDIMessageWrapperCollection(Entry.Messages.Cast<EDIMessage>(), Factory));
		EDIMessageWrapperCollection messages;

		static ZBool GetImportCancellationDeclinedByCustoms(CusEntryHeader entry)
		{
			var result = false;
			var message5BG = entry.Messages.Cast<EDIMessage>().FirstOrDefault(x => x.EM_MessageType == ElectronicDocumentTypeList.Codes._5BG);
			if (message5BG != null && entry.CusEntryNumber?.CE_ExpiryDate == ZDateTime.Empty)
			{
				result = true;
			}
			return result;
		}

		public ExportEntryHeaderWrapper ExportEntryWrapper
		{
			get
			{
				if (exportEntryWrapper == null)
				{
					exportEntryWrapper = new ExportEntryHeaderWrapper(Entry.PK, new ExportEntryHeaderCreator().Create(Entry), Factory);
					exportEntryWrapper.Decorate(Entry);
				}
				return exportEntryWrapper;
			}
		}
		ExportEntryHeaderWrapper exportEntryWrapper;

		public ImportEntryHeaderWrapper ImportEntryWrapper
		{
			get
			{
				if (importEntryWrapper == null)
				{
					importEntryWrapper = new ImportEntryHeaderWrapper(Entry.PK, new ImportEntryHeaderCreator().Create(Entry), Factory);
					importEntryWrapper.Decorate(Entry);
				}
				return importEntryWrapper;
			}
		}
		ImportEntryHeaderWrapper importEntryWrapper;

		public LocalExportEntryHeaderWrapper LocalExportEntryWrapper
		{
			get
			{
				if (localExportEntryWrapper == null)
				{
					if (LocalExportTransactionNatureCodeList.Is5DP(Entry.Declaration.JE_MessageSubType))
					{
						localExportEntryWrapper = new LocalExportEntryHeaderWrapper(Entry.PK, ElectronicDocumentTypeList.Codes._5DP, new LocalExport5DPEntryHeaderCreator().Create(Entry), Factory);
					}
					else
					{
						localExportEntryWrapper = new LocalExportEntryHeaderWrapper(Entry.PK, ElectronicDocumentTypeList.Codes._5DQ, new LocalExport5DQEntryHeaderCreator().Create(Entry), Factory);
					}
					localExportEntryWrapper.Decorate(Entry);
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
					if (Entry.GetOriginalFTAType() == ElectronicDocumentTypeList.Codes._DHR)
					{
						fTAHeader = new FTAHeaderWrapper(new ImportDHRCreator().Create(Entry), Factory);
					}
					else
					{
						fTAHeader = new FTAHeaderWrapper(new ImportFTACreator().Create(Entry), Factory);
					}
				}
				return fTAHeader;
			}
		}
		FTAHeaderWrapper fTAHeader;
	}
}
