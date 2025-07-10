using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class GoodsInformationOtherTypeCommodityCodeProvider : IGoodsInformationOtherTypeCommodityCode
	{
		public GoodsInformationOtherTypeCommodityCodeProvider(ZString tariff)
		{
			this.tariff = tariff;
		}
		readonly ZString tariff;

		public string CombinedNomenclatureCode => tariff.Length < 8 ? null : tariff.Left(8).ToString();

		public string TaricCode => tariff.Length < 10 ? null : tariff.SubstringSafe(8, 2);
	}
}
