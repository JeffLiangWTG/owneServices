using System.Collections.Generic;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Messaging
{
	public interface ICUSCAN : IDataProvider
	{
		ZString ReferenceNumber { get; }

		string MRN {  get; }

		ZString Reason { get; }

		IReadOnlyCollection<ICUSCANGoodsItem> GoodsItems { get; }
	}
}
