using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1
{
	public sealed class CUSCANProvider : ICUSCAN
	{
		public CUSCANProvider(SCCANE message)
		{
			this.message = Argument.NotNull(message, nameof(message));
		}
		readonly SCCANE message;

		public string MessageIdentifier => message.MetaData.MessageIdentifier;

		public ZString ReferenceNumber => message.Header.ReferenceNumber;

		public string MRN => message.Header.MRN;

		public ZString Reason => message.Header.Reason;

		public IReadOnlyCollection<ICUSCANGoodsItem> GoodsItems => goodsItems ?? (goodsItems = message.Body.Select(x => new CUSCANGoodsItemProvider(x)).ToArray());
		IReadOnlyCollection<ICUSCANGoodsItem> goodsItems;
	}
}
