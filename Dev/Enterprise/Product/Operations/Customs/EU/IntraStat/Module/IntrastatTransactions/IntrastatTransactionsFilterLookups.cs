using CargoWise.Integration;
using Enterprise.Customs.EU.Intrastat.Business;
using Enterprise.Customs.Module;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.EU.Intrastat.Module
{
	public class IntrastatTransactionsFilterLookups : CommonFilterLookups
	{
		public IntrastatTransactionsFilterLookups(FilterStripBusinessObject filterBizObj) : base(filterBizObj)
		{
		}

		public ICodeDescriptionPairList TransportModes => IntrastatHeader.Lookups.ModeOfTransportList;

		CusIntrastatHeader IntrastatHeader => intrastatHeader ?? (intrastatHeader = Factory.GetNull<CusIntrastatHeader>());
		CusIntrastatHeader intrastatHeader;
	}
}
