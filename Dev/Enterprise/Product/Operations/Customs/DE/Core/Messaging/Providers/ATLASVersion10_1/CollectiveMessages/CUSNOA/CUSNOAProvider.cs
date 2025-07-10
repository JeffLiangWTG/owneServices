using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1
{
	public class CUSNOAProvider : ICUSNOA
	{
		public CUSNOAProvider(GCNOAD message)
		{
			this.message = Argument.NotNull(message, nameof(message));
		}
		readonly GCNOAD message;

		public string MessageIdentifier => message.MetaData.MessageIdentifier;

		public ZString ReferenceNumber => message.Header.ReferenceNumber;

		public ZString LocalReferenceNumber => message.Header.LocalReferenceNumber;

		public IReadOnlyCollection<ICUSNOAGoodsItem> GoodsItems => goodsItems ?? (goodsItems = message.Body?.Select(x => new CUSNOAGoodsItemProvider(x)).ToArray<ICUSNOAGoodsItem>() ?? Array.Empty<ICUSNOAGoodsItem>());
		IReadOnlyCollection<ICUSNOAGoodsItem> goodsItems;
	}
}
