using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Documents.MAFCoverSheet;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers.Customs.NZ.MAFCoverSheet
{
	public class DocMAFCoverSheetCommodityCollection : DocBaseWrapperCollection<DocMAFCoverSheetCommodity>
	{
		public DocMAFCoverSheetCommodityCollection(NZDocsMAFCSCommodityCollection commodities, ZInt firstCommodity, ZInt numberOfRows)
			: base(commodities.Factory)
		{
			for (int i = firstCommodity; i <= (firstCommodity + numberOfRows - 1); i++)
			{
				if (commodities.Count > i && commodities[i] != null)
				{
					Add(new DocMAFCoverSheetCommodity(commodities[i], Factory));
				}
				else
				{
					Add(new DocMAFCoverSheetCommodity(null, Factory));
				}
			}
		}

		public DocMAFCoverSheetCommodityCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override DocumentWrapper WrapObject(object objectToWrap)
		{
			return new DocMAFCoverSheetCommodity(null, Factory);
		}
	}
}
