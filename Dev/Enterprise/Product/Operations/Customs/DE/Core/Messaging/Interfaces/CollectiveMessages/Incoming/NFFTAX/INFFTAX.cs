using System.Collections.Generic;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Messaging
{
	public interface INFFTAX : IDataProvider
	{
		ZString ReferenceNumber { get; }

		string MRN { get; }

		ZString LocalReferenceNumber { get; }

		IReadOnlyCollection<INFFTAXGoodsItem> GoodsItems { get; }
	}
}
