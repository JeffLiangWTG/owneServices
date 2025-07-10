using CargoWise.EntityFramework;
using Enterprise.Customs.NZ.Business.Documents.MAFCoverSheet;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers.Customs.NZ.MAFCoverSheet
{
	public class DocMAFCoverSheetPageCollection : DocBaseWrapperCollection<DocMAFCoverSheetPage>
	{
		public DocMAFCoverSheetPageCollection(BusinessObjectFactory factory, NZDocsMAFCoverSheet coverSheet)
			: base(factory)
		{
			Add(
				new DocMAFCoverSheetPage(factory
					, new DocMAFCoverSheetCommodityCollection(coverSheet.Commodities, 0, 5)
					, new DocMAFCoverSheetContainerCollection(coverSheet.Containers, 0, 5))
				);
			int nextCommodityIndex = 5;
			int nextContainerIndex = 5;
			int totalCommoditiesCount = coverSheet.Commodities.Count;
			int totalContainersCount = coverSheet.Containers.Count;
			while (nextCommodityIndex < totalCommoditiesCount
				 || nextContainerIndex < totalContainersCount)
			{
				Add(
					new DocMAFCoverSheetPage(factory
						, new DocMAFCoverSheetCommodityCollection(coverSheet.Commodities, nextCommodityIndex, 10)
						, new DocMAFCoverSheetContainerCollection(coverSheet.Containers, nextContainerIndex, 28))
					);
				nextCommodityIndex += 10;
				nextContainerIndex += 28;
			}
		}

		public DocMAFCoverSheetPageCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override DocumentWrapper WrapObject(object objectToWrap)
		{
			return new DocMAFCoverSheetPage(Factory
				, new DocMAFCoverSheetCommodityCollection(Factory)
				, new DocMAFCoverSheetContainerCollection(new NZDocsMAFCSContainerCollection(Factory), 0, 0));
		}
	}
}
