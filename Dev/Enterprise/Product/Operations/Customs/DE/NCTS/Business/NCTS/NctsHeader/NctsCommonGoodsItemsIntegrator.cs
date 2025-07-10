using Enterprise.Customs.Business.CommonGoodsItemsIntegration;

namespace Enterprise.Customs.DE.NCTS.Business
{
	sealed class NctsCommonGoodsItemsIntegrator : EU.NCTS.Business.NctsCommonGoodsItemsIntegrator
	{
		public NctsCommonGoodsItemsIntegrator(NctsHeader header) : base(header)
		{
		}

		protected override void CopySupplementaryQuantityFromCommonGoodsItem(ICommonGoodsItem source, EU.NCTS.Business.NctsDepartureCargoDesc target)
		{
		}
	}
}
