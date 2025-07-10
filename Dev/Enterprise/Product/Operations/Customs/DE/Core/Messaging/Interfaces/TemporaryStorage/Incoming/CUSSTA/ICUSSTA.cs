using System.Collections.Generic;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Messaging
{
	public interface ICUSSTA : IDataProvider
	{
		ZString AdditionalReferenceNumber { get; }
		ZString CustodianReferenceNumber { get; }
		ZString CustodianSubsidiaryNumber { get; }
		ZString InterchangeRecipientReferenceNumber { get; }
		ZString InterchangeRecipientSubsidiaryNumber { get; }
		IReadOnlyCollection<ICUSSTAGoodsItem> GoodsItems { get; }
	}
}
