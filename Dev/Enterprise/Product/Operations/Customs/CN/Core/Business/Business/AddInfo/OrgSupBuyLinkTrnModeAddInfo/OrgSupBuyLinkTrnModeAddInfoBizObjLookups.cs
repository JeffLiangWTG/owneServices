using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.CN.Business
{
	public class OrgSupBuyLinkTrnModeAddInfoBizObjLookups : ZLookups
	{
		public OrgSupBuyLinkTrnModeAddInfoBizObjLookups(OrgSupBuyLinkTrnModeAddInfoBizObj parent) : base(parent) { }

		public ZZRefCusCodeListCombinedCollection CustomsOfficeList => CNRefCusCodeListTypes.GetCustomsOfficeList(Factory, ZDateTime.Today);

		public ZZRefCusCodeListCombinedCollection OfficeOfEntryExitList => CNRefCusCodeListTypes.GetCustomsOfficeList(Factory, ZDateTime.Today);

		public ZZRefCusCodeListCombinedCollection CIQPortList => CNRefCusCodeListTypes.GetCNCIQPorts(Factory, ZDateTime.Today);
	}
}
