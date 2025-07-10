using CargoWise.Types;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class NCTS5CommonCommodityWithCusCodeWrapper : NCTS5CommonCommodityWrapper, INCTSCommonCommodityWithCusCode
	{
		public NCTS5CommonCommodityWithCusCodeWrapper(NctsCommonCargoDesc item)
			: base(item)
		{
		}

		public ZString CusCode => CusCodeCore;
		protected virtual ZString CusCodeCore => item.BY_CusC4Number;
	}
}
