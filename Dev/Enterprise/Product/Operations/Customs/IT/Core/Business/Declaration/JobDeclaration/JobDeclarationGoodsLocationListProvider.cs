using CargoWise.EntityFramework;

namespace Enterprise.Customs.IT.Business.Declaration;

public class JobDeclarationGoodsLocationListProvider : GoodsLocationListProvider<GoodsLocationList>
{
	public JobDeclarationGoodsLocationListProvider(IAutHeaderWithCusOfficeProvider authorisationWithCustomsOfficeProvider, BusinessObjectFactory factory) : base(authorisationWithCustomsOfficeProvider, factory)
	{
	}
}
