using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1
{
	public class CURRELProvider : ICURREL
	{
		public CURRELProvider(GCRELG message)
		{
			this.message = Argument.NotNull(message, nameof(message));
		}
		readonly GCRELG message;

		public string MessageIdentifier => message.MetaData?.MessageIdentifier;

		public string TemporaryReferenceNumber => message.Header?.TemporaryReferenceNumber;

		public string ReferenceNumber => message.Header?.ReferenceNumber;

		public string LocalReferenceNumber => message.Header?.LRN;

		public string MRN => message.Header?.MRN;

		public string PresentationModalitiesNotification => message.Header?.PresentationModalitiesNotification;

		public IReadOnlyCollection<ICURRELGoodsItem> GoodsItems => goodsItems ?? (goodsItems = message.Body.Select(x => new CURRELGoodsItemProvider(x)).ToArray());
		IReadOnlyCollection<ICURRELGoodsItem> goodsItems;
	}
}
