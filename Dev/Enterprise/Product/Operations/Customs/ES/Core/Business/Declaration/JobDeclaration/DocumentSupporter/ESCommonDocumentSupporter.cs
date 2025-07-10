using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.ES.Business.ESConstants;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class ESCommonDocumentSupporter
	{
		public ESCommonDocumentSupporter(BusinessObject bo)
		{
			bO = bo;
		}

		readonly BusinessObject bO;

		const string C10DataContextValue = ".CusEntryHeaderC10";

		public void AddESSupportedBODataSources(List<DataContextValue> result)
		{
			result.Add(new DataContextValue(C10DataContextValue));
		}

		public IBODocDataProvider[] GetESBODocDataProviders(DataContextValue dataContextValue)
		{
			if (dataContextValue.Equals(new DataContextValue(C10DataContextValue)))
			{
				return GetBODocDataForC10Print();
			}

			return null;
		}

		IBODocDataProvider[] GetBODocDataForC10Print()
		{
			var result = new List<IBODocDataProvider>();

			if (bO is JobDeclaration declaration)
			{
				foreach (CusEntryHeader entryHeader in declaration.ActiveEntryHeaders)
				{
					result.Add(BODocDataProvider.Get(ESDocC10Header.New(entryHeader, bO)));
				}
			}
			else if (bO is CusEntryHeader entryHeader)
			{
				result.Add(BODocDataProvider.Get(ESDocC10Header.New(entryHeader, bO)));
			}

			return result.ToArray();
		}

		public DataContext[] GetESSupportedDataContexts(DataContext[] baseArray)
		{
			var result = new List<DataContext>(baseArray)
			{
				DataContext.ESSADH
			};

			return result.ToArray();
		}

		public DocumentWrapper[] GetESDocumentWrappers(DataContext dataContext, IEnumerable<CusEntryHeader> entryHeaders)
		{
			switch (dataContext)
			{
				case DataContext.ESSADH:
					return GetWrappers(dataContext, entryHeaders);
				case DataContext.SADH:
					return GetWrappers(dataContext, entryHeaders);
			}
			return null;
		}

		DocumentWrapper[] GetWrappers(DataContext dataContext, IEnumerable<CusEntryHeader> entryHeaders)
		{
			var wrapperStrongName = supportedWrappers[dataContext];
			var documentWrappers = new List<DocumentWrapper>();
			foreach (var entry in entryHeaders)
			{
				var wrapper = DocumentWrapperFactory.CreateWrapperWithoutException(wrapperStrongName, entry, DocumentWrapperConstants.DocumentWrappersAssembly);
				if (wrapper != null)
				{
					documentWrappers.Add(wrapper);
				}
			}
			return documentWrappers.ToArray();
		}

		readonly ImmutableDictionary<DataContext, ZString> supportedWrappers = new Dictionary<DataContext, ZString>()
		{
			{ DataContext.ESSADH, DocumentWrapperConstants.SADHExportDocumentWrapperType },
			{ DataContext.SADH, DocumentWrapperConstants.SADHImportDocumentWrapperType },
		}.ToImmutableDictionary();

		public TitleCopyCountPair GetDocumentTitlesForPivot(ZString parentDocumentMenuName, IDocumentSupportable parentBusinessObject, IStmMenuTemplatePivot pivot)
		{
			if (parentBusinessObject is ESDocC10Header doc10Header)
			{
				var entryHeader = doc10Header.entryHeader;
				var code = entryHeader.MovementReferenceNumber.IsEmpty ? entryHeader.CH_BGMReference : entryHeader.MovementReferenceNumber;
				return new TitleCopyCountPair(parentDocumentMenuName + " - " + code, 1);
			}
			return null;
		}
	}
}
