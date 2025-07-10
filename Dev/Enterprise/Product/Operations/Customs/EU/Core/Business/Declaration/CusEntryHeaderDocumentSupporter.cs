using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class CusEntryHeaderDocumentSupporter : Customs.Business.CusEntryHeaderDocumentSupporter
	{
		public CusEntryHeaderDocumentSupporter(CusEntryHeader entryHeader)
			: base(entryHeader)
		{
		}

		public override string GetFilterValue(DocumentFilters filterName)
		{
			switch (filterName)
			{
				case DocumentFilters.CTYEG:
					return EntryHeader?.Declaration?.Country?.RN_EconomicGrouping ?? GlbCompany.CurrentCompany.Country.RN_EconomicGrouping;
				case DocumentFilters.CTYEGSADH:
					return (EntryHeader?.Declaration as JobDeclaration)?.UseIDDDocument ?? false ? ZString.Empty : EntryHeader?.Declaration?.Country?.RN_EconomicGrouping ?? GlbCompany.CurrentCompany.Country.RN_EconomicGrouping;
				case DocumentFilters.CTYEGIDD:
					return (EntryHeader?.Declaration as JobDeclaration)?.UseIDDDocument ?? false ? EntryHeader?.Declaration?.Country?.RN_EconomicGrouping ?? GlbCompany.CurrentCompany.Country.RN_EconomicGrouping : ZString.Empty;
				default:
					return base.GetFilterValue(filterName);
			}
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			var result = new List<DocumentWrapper>();
			switch (dataContext)
			{
				case DataContext.SADH:
					var dtiAdviceWrapper = DocumentWrapperFactory.CreateWrapper(dataContext, "Enterprise.DocumentWrappers.Customs.EU", EntryHeader);
					if (dtiAdviceWrapper != null)
					{
						result.Add(dtiAdviceWrapper);
					}
					break;
			}
			return result.Count > 0 ? result.ToArray() : base.GetDocumentWrappersInternal(dataContext, commandBeingRun);
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
