using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class CC025CConsignmentItemProvider
	{
		public CC025CConsignmentItemProvider(ConsignmentItemType02 consignmentItem)
		{
			consignmentItemType = Argument.NotNull(consignmentItem, nameof(consignmentItem));
		}
		readonly ConsignmentItemType02 consignmentItemType;

		public ZString GoodsItemNumber => consignmentItemType.GoodsItemNumber;

		public ZString DeclarationGoodsItemNumber => consignmentItemType.DeclarationGoodsItemNumber;

		public ZString ReleaseType => consignmentItemType.ReleaseType;
	}
}
