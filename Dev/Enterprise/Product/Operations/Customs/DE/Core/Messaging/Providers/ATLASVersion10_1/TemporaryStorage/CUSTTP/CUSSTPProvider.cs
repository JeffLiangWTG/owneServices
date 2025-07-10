using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1
{
	public sealed class CUSSTPProvider : ICUSSTP
	{
		public CUSSTPProvider(SCSTPC message)
		{
			this.message = Argument.NotNull(message, nameof(message));
		}
		readonly SCSTPC message;

		public string MessageIdentifier => message.MetaData.MessageIdentifier;

		public ZString ReferenceNumber => message.Header.ReferenceNumber;

		public ZString LocalReferenceNumber => message.Header.LRN;

		public string MRN => message.Header.MRN;

		public ZDateTime NotificationDateTime => message.Header.NotificationDateTime;

		public IReadOnlyCollection<ICUSSTPGoodsItem> GoodsItems => goodsItems ?? (goodsItems = message.Body.Select(x => new CUSSTPGoodsItemProvider(x)).ToArray());
		IReadOnlyCollection<ICUSSTPGoodsItem> goodsItems;
	}
}
