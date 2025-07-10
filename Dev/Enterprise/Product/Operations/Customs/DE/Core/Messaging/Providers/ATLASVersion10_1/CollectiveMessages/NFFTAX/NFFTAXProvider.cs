using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1
{
	public class NFFTAXProvider : INFFTAX
	{
		public NFFTAXProvider(GNTAXK message)
		{
			this.message = Argument.NotNull(message, nameof(message));
		}

		public ZString ReferenceNumber => message.Header?.ReferenceNumber ?? ZString.Empty;

		public string MRN => message.Header?.MRN;

		public string MessageIdentifier => message.MetaData?.MessageIdentifier;

		public ZString LocalReferenceNumber => message.Header?.LRN ?? ZString.Empty;

		public IReadOnlyCollection<INFFTAXGoodsItem> GoodsItems => goodsItems ?? (goodsItems = message.Body?.Select(x => new NFFTAXGoodsItemProvider(x)).ToArray() ?? Array.Empty<INFFTAXGoodsItem>());
		IReadOnlyCollection<INFFTAXGoodsItem> goodsItems;

		readonly GNTAXK message;
	}
}
