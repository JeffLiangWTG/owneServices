using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.TP5.Interfaces;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5
{
	public class CommodityWrapper : ICommodity
	{
		protected CommodityWrapper(NctsCommonCargoDesc item)
		{
			this.item = Argument.NotNull(item, nameof(item));
		}

		protected readonly NctsCommonCargoDesc item;

		public static CommodityWrapper New(NctsCommonCargoDesc item) => item == null ? null : new CommodityWrapper(item);

		public virtual string DescriptionOfGoods => descriptionOfGoods ?? (descriptionOfGoods = item.BY_Description);
		string descriptionOfGoods;

		public string CusCode => cusCode ?? (cusCode = item.BY_CusC4Number);
		string cusCode;

		public virtual ICommodityCode CommodityCode => commodityCode ?? (commodityCode = CommodityCodeWrapper.New(item.BY_HarmonisedTariff));
		ICommodityCode commodityCode;

		public ICollection<IDangerousGoods> DangerousGoods => dangerousGoods ?? (dangerousGoods = GetDangerousGoods());
		ICollection<IDangerousGoods> dangerousGoods;

		ICollection<IDangerousGoods> GetDangerousGoods()
		{
			if (item is NctsDepartureCargoDesc departureItem)
			{
				var result = new Collection<IDangerousGoods>();
				departureItem.UNDGs.Cast<UNDGDataItem>().ForEach(dangerousGoods => result.Add(DangerousGoodsWrapper.New(dangerousGoods)));
				return result;
			}
			else
			{
				return null;
			}
		}

		public virtual IGoodsMeasure GoodsMeasure => goodsMeasure ?? (goodsMeasure = GoodsMeasureWrapper.New(item));
		IGoodsMeasure goodsMeasure;
	}
}
