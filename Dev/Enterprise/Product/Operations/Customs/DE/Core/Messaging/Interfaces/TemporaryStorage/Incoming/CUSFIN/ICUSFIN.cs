using System.Collections.Generic;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Messaging
{
	public interface ICUSFIN : IDataProvider
	{
		ZString CustodianReferenceNumber { get; }
		ZString CustodianSubsidiaryNumber { get; }
		ZString InterchangeRecipientReferenceNumber { get; }
		ZString InterchangeRecipientSubsidiaryNumber { get; }
		ZString AdditionalRegistrationNumber { get; }
		ZString AdditionalReferenceNumber { get; }
		string MRN {  get; }
		ZString CompletionType { get; }
		IReadOnlyCollection<ICUSFINGoodsItem> GoodsItems { get; }
	}
}
