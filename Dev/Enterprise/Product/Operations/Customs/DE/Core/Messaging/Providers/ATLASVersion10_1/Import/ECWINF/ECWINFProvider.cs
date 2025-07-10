using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1
{
	public class ECWINFProvider : IECWINF
	{
		public ECWINFProvider(LECWIF message)
		{
			this.message = Argument.NotNull(message, nameof(message));
		}
		readonly LECWIF message;

		public string MessageIdentifier => message.MetaData?.MessageIdentifier;

		public ZString ReferenceNumber => message.CustomsWarehouse?.AdditionalRegistrationNumber ?? ZString.Empty;

		public string MRN => message.CustomsWarehouse?.MRN;

		public ZString LocalReferenceNumber => message.CustomsWarehouse?.LRN ?? ZString.Empty;

		public IReadOnlyCollection<IECWINFGoodsItem> GoodsItems => goodsItems ?? (goodsItems = message.CustomsWarehouse?.GoodsItem.Select(x => new ECWINFGoodsItemProvider(x)).ToArray() ?? Array.Empty<IECWINFGoodsItem>());

		IReadOnlyCollection<IECWINFGoodsItem> goodsItems;
	}
}
