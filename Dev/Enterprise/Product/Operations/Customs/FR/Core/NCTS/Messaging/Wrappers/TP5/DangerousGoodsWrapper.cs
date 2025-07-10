using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.TP5.Interfaces;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5
{
	public class DangerousGoodsWrapper : IDangerousGoods
	{
		DangerousGoodsWrapper(UNDGDataItem item)
		{
			this.item = Argument.NotNull(item, nameof(item));
		}

		readonly UNDGDataItem item;

		public static DangerousGoodsWrapper New(UNDGDataItem item) => item == null ? null : new DangerousGoodsWrapper(item);

		public string UNNumber => unNumber ?? (unNumber = item.SubstanceCode);
		string unNumber;
	}
}
