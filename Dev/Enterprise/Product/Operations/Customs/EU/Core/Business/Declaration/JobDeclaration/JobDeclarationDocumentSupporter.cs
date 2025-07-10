using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class JobDeclarationDocumentSupporter : BaseJobDeclarationDocumentSupporter
	{
		public JobDeclarationDocumentSupporter(JobDeclaration declaration)
			: base(declaration)
		{
			this.Declaration = declaration;
		}
		protected readonly JobDeclaration Declaration;

		public override BusinessContext[] SupportedChildBusinessContexts
			=> [.. base.SupportedChildBusinessContexts, BusinessContext.CusExitHeader];

		public override IDocumentSupportable[] GetChildCollection(IStmMenuItem menu, BusinessContext businessContext, IStmMenuItem childCommandBeingRun)
		{
			var result = base.GetChildCollection(menu, businessContext, childCommandBeingRun);

			switch (businessContext)
			{
				case BusinessContext.CusExitHeader:
					var cusExitHeaders = Declaration.ExitHeaders;
					result = [.. cusExitHeaders.Cast<IDocumentSupportable>()];
					break;
			}

			return result;
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			switch (dataContext)
			{
				case DataContext.SADH:
					List<DocumentWrapper> result = new List<DocumentWrapper>();
					foreach (CusEntryHeader entryHeader in Declaration.CustomsEntryHeaders)
					{
						result.Add(DocumentWrapperFactory.CreateWrapper(dataContext, "Enterprise.DocumentWrappers.Customs.EU", entryHeader));
					}
					return result.ToArray();
			}
			return base.GetDocumentWrappersInternal(dataContext, commandBeingRun);
		}

		public override string GetFilterValue(DocumentFilters filterName)
		{
			switch (filterName)
			{
				case DocumentFilters.CTYEGSADH:
					return Declaration.UseIDDDocument ? ZString.Empty : EconomicGroupList.Codes.EuropeanUnion;
				case DocumentFilters.CTYEGIDD:
					return Declaration.UseIDDDocument ? EconomicGroupList.Codes.EuropeanUnion : ZString.Empty;
				case DocumentFilters.CTYEG:
					return EconomicGroupList.Codes.EuropeanUnion;

				default:
					return base.GetFilterValue(filterName);
			}
		}

		protected override DataContext[] GetSupportedDataContexts()
		{
			List<DataContext> result = new List<DataContext>(base.GetSupportedDataContexts());
			result.Add(DataContext.SADH);
			return result.ToArray();
		}

		public override TitleCopyCountPair GetDocumentTitlesForPivot(ZString parentDocumentMenuName, IDocumentSupportable parentBusinessObject, IStmMenuTemplatePivot pivot)
		{
			if (parentBusinessObject is CusEntryHeader entryHeader)
			{
				var code = entryHeader.MovementReferenceNumber.IsEmpty ? entryHeader.CH_BGMReference : entryHeader.MovementReferenceNumber;
				return new TitleCopyCountPair(parentDocumentMenuName + " - " + code, 1);
			}
			return base.GetDocumentTitlesForPivot(parentDocumentMenuName, parentBusinessObject, pivot);
		}
	}
}
