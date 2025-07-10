using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class JobDeclarationDocumentSupporter : EU.Business.Declaration.JobDeclarationDocumentSupporter
	{
		public JobDeclarationDocumentSupporter(JobDeclaration declaration)
			: base(declaration)
		{
			eSCommonDocSupporter = new ESCommonDocumentSupporter(declaration);
		}

		readonly ESCommonDocumentSupporter eSCommonDocSupporter;

		protected new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

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
			var esDocumentWrappers = eSCommonDocSupporter.GetESDocumentWrappers(dataContext, EntryHeaders);
			return esDocumentWrappers ?? base.GetDocumentWrappersInternal(dataContext, commandBeingRun);
		}

		public override TitleCopyCountPair GetDocumentTitlesForPivot(ZString parentDocumentMenuName, IDocumentSupportable parentBusinessObject, IStmMenuTemplatePivot pivot)
		{
			return eSCommonDocSupporter.GetDocumentTitlesForPivot(parentDocumentMenuName, parentBusinessObject, pivot) ?? base.GetDocumentTitlesForPivot(parentDocumentMenuName, parentBusinessObject, pivot);
		}

		IEnumerable<CusEntryHeader> EntryHeaders => Declaration.CustomsEntryHeaders;
	}
}
