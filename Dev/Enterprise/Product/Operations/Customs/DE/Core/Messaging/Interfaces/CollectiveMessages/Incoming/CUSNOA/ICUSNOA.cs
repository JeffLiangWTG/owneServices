using System.Collections.Generic;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Messaging
{
	public interface ICUSNOA : IDataProvider
	{
		ZString ReferenceNumber { get; }

		ZString LocalReferenceNumber { get; }

		IReadOnlyCollection<ICUSNOAGoodsItem> GoodsItems { get; }
	}
}
