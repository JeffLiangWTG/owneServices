using System.Collections.Generic;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Messaging
{
	public interface ICUSSTP : IDataProvider
	{
		ZString ReferenceNumber { get; }

		ZString LocalReferenceNumber { get; }

		string MRN { get; }

		ZDateTime NotificationDateTime { get; }

		IReadOnlyCollection<ICUSSTPGoodsItem> GoodsItems { get; }
	}
}
