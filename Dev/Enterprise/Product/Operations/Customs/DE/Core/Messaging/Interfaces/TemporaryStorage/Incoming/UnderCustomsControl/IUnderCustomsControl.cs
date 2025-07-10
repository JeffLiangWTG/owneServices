using System.Collections.Generic;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Messaging
{
	public interface IUnderCustomsControl : IDataProvider
	{
		ZDate ArrivalDate { get; }
		ZDate PresentationDate { get; }
		ZString ReferenceNumber { get; }
		ZString LocalReferenceNumber { get; }
		string MRN { get; }
		ZString PreviousReferenceType { get; }
		ZString PreviousReferenceNumber { get; }
		ZString CustomsOfficeReferenceNumber { get; }
		ZString ReferencedMessageIdentifier { get; }
		IReadOnlyCollection<IUnderCustomsControlGoodsItem> GoodsItems { get; }
	}
}
