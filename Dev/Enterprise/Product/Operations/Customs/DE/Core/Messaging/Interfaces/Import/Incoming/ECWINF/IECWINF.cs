using System.Collections.Generic;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Messaging
{
	public interface IECWINF : IDataProvider
	{
		ZString ReferenceNumber { get; }

		string MRN { get; }

		ZString LocalReferenceNumber { get; }

		IReadOnlyCollection<IECWINFGoodsItem> GoodsItems { get; }
	}
}
