using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class NCTS5CommonCommodityWrapper : INCTSCommonCommodity
	{
		public NCTS5CommonCommodityWrapper(NctsCommonCargoDesc item)
		{
			this.item = Argument.NotNull(item, nameof(item));
		}
		protected readonly NctsCommonCargoDesc item;

		protected const int DescriptionMaxLengthTransitionalPeriod = 280;
		protected const int DescriptionMaxLengthFinalPeriod = 512;
		protected int DescriptionMaxLength => item.IsInPhase5TransitionPeriod ? DescriptionMaxLengthTransitionalPeriod : DescriptionMaxLengthFinalPeriod;

		public ZString DescriptionOfGoods => DescriptionOfGoodsCore;
		protected virtual ZString DescriptionOfGoodsCore => item.BY_Description.SubstringSafe(0, DescriptionMaxLength);

		public INCTSCommonCommodityCode CommodityCode => commodityCode ?? (commodityCode = GetCommodityCode());
		NCTS5CommonCommodityCodeWrapper commodityCode;
		protected virtual NCTS5CommonCommodityCodeWrapper GetCommodityCode() => new NCTS5CommonCommodityCodeWrapper(item);
	}
}
