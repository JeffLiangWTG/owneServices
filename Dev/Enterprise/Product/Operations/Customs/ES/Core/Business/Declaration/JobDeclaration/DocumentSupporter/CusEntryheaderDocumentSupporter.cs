using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class CusEntryHeaderDocumentSupporter : EU.Business.Declaration.CusEntryHeaderDocumentSupporter
	{
		public CusEntryHeaderDocumentSupporter(CusEntryHeader entryHeader)
			: base(entryHeader)
		{
			eSCommonDocSupporter = new ESCommonDocumentSupporter(entryHeader);
		}

		readonly ESCommonDocumentSupporter eSCommonDocSupporter;

		protected new CusEntryHeader EntryHeader => (CusEntryHeader)base.EntryHeader;

		protected override List<DataContextValue> GetSupportedBODataSources()
		{
			List<DataContextValue> result = GetSupportedBODataSourcesFor(BusinessObject.GetType());
			eSCommonDocSupporter.AddESSupportedBODataSources(result);
			return result;
		}

		protected override IBODocDataProvider[] GetBODocDataProvidersInternal(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			var esBODocDataProviders = eSCommonDocSupporter.GetESBODocDataProviders(dataContextValue);
			return esBODocDataProviders ?? base.GetBODocDataProvidersInternal(dataContextValue, commandBeingRun);
		}

		protected override DataContext[] GetSupportedDataContexts() => eSCommonDocSupporter.GetESSupportedDataContexts(base.GetSupportedDataContexts());

		protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			var esDocumentWrappers = eSCommonDocSupporter.GetESDocumentWrappers(dataContext, new List<CusEntryHeader>() { EntryHeader });
			return esDocumentWrappers ?? base.GetDocumentWrappersInternal(dataContext, commandBeingRun);
		}

		public override TitleCopyCountPair GetDocumentTitlesForPivot(ZString parentDocumentMenuName, IDocumentSupportable parentBusinessObject, IStmMenuTemplatePivot pivot)
		{
			return eSCommonDocSupporter.GetDocumentTitlesForPivot(parentDocumentMenuName, parentBusinessObject, pivot) ?? base.GetDocumentTitlesForPivot(parentDocumentMenuName, parentBusinessObject, pivot);
		}
	}
}
