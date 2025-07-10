using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface WarehouseItemWrapper
	{
		ZString AHECCCode
		{
			get;
		}
		ZDecimal NetQuantity
		{
			get;
		}
		ZString NetQuantityUnit
		{
			get;
		}
		ZString GoodsDescription
		{
			get;
		}
	}
}
