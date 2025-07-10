using CargoWise.EntityFramework;

namespace Enterprise.DocumentWrappers.Customs.NZ.MAFCoverSheet
{
	[AllowPublicConstructor]
	[AllowNoStaticNew]
	public class DocMAFCoverSheetPage : DocBaseWrapper
	{
		public DocMAFCoverSheetPage(BusinessObjectFactory factory
			, DocMAFCoverSheetCommodityCollection commodities
			, DocMAFCoverSheetContainerCollection containers)

			: base(null, factory)
		{
			this.commodities = commodities;
			this.containers = containers;
		}

		public DocMAFCoverSheetCommodityCollection Commodities
		{
			get { return commodities; }
		}
		readonly DocMAFCoverSheetCommodityCollection commodities;

		public DocMAFCoverSheetContainerCollection Containers
		{
			get { return containers; }
		}
		readonly DocMAFCoverSheetContainerCollection containers;
	}
}
