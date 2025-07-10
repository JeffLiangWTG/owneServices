using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Documents.MAFCoverSheet;

namespace Enterprise.DocumentWrappers.Customs.NZ.MAFCoverSheet
{
	[AllowPublicConstructor]
	[AllowNoStaticNew]
	public class DocMAFCoverSheetCommodity : DocBaseWrapper
	{
		public DocMAFCoverSheetCommodity(NZDocsMAFCSCommodity commodity, BusinessObjectFactory factory)
			: base(commodity, factory)
		{
			if (commodity != null)
			{
				name = commodity.D1_CommodityOrSpecies;
				qty = commodity.D1_QuantityWithUnit;
				measure = commodity.D1_MeasureWithUnit;
			}
		}

		readonly ZString name;
		readonly ZString qty;
		readonly ZString measure;

		public ZString Name
		{
			get { return name; }
		}

		public ZString Qty
		{
			get { return qty; }
		}

		public ZString Measure
		{
			get { return measure; }
		}
	}
}
