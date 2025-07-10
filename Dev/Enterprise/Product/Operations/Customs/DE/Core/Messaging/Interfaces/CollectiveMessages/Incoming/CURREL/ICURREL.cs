using System.Collections.Generic;
using CargoWise.Customs.DE.MessageContracts;

namespace Enterprise.Customs.DE.Messaging
{
	public interface ICURREL : IDataProvider
	{
		string TemporaryReferenceNumber { get; }

		string ReferenceNumber { get; }

		string LocalReferenceNumber { get; }

		string MRN { get; }

		string PresentationModalitiesNotification { get; }

		IReadOnlyCollection<ICURRELGoodsItem> GoodsItems { get; }
	}
}
