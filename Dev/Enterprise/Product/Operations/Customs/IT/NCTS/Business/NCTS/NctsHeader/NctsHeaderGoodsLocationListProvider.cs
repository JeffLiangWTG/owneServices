using CargoWise.EntityFramework;
using Enterprise.Customs.IT.Business;

namespace Enterprise.Customs.IT.NCTS.Business;

public class NctsHeaderGoodsLocationListProvider : GoodsLocationListProvider<NctsGoodsLocationList>
{
	public NctsHeaderGoodsLocationListProvider(IAutHeaderWithCusOfficeProvider authorisationWithCustomsOfficeProvider, BusinessObjectFactory factory) : base(authorisationWithCustomsOfficeProvider, factory)
	{
	}
}
