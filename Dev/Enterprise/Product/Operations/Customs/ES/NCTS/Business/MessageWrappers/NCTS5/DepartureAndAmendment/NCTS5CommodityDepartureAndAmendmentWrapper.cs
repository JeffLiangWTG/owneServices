using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class NCTS5CommodityDepartureAndAmendmentWrapper : NCTS5CommonCommodityWithCusCodeWrapper, INCTSCommodityDepartureAndAmendment
	{
		public NCTS5CommodityDepartureAndAmendmentWrapper(NctsDepartureCargoDesc item) : base(item)
		{
			departureItem = item;
		}

		readonly NctsDepartureCargoDesc departureItem;

		public IReadOnlyCollection<ICommonDangerousGoods> DangerousGoods
		{
			get
			{
				if (dangerousGoods == null)
				{
					var dangerousGoodsWrapperList = new List<CommonDangerousGoodsWrapper>();

					var dangerousGoodsList = departureItem.UNDGs;
					ZShort seqNum = 1;
					foreach (var dangerousGood in dangerousGoodsList)
					{
						dangerousGoodsWrapperList.Add(new CommonDangerousGoodsWrapper(seqNum, dangerousGood.Substance.DG_UNNO));
						seqNum++;
					}
					dangerousGoods = dangerousGoodsWrapperList.AsReadOnly();
				}
				return dangerousGoods;
			}
		}
		IReadOnlyCollection<CommonDangerousGoodsWrapper> dangerousGoods;

		public INCTSGoodsMeasureDepartureAndAmendment GoodsMeasure => goodsMeasure ?? (goodsMeasure = new NCTS5GoodsMeasureDepartureAndAmendmentWrapper(departureItem));
		NCTS5GoodsMeasureDepartureAndAmendmentWrapper goodsMeasure;
	}
}
