using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders;

public interface IH1ImportAmendmentC40MessageDataProvider : IH1ImportCommonDataProvider
{
	ZString LRN { get; }
	ZString CustomsRegistrationNumber { get; }
	IReadOnlyCollection<IH1ImportAmendmentC40GoodsItem> GoodsItems { get; }
}

public interface IH1ImportAmendmentC40GoodsItem : ICommonH1GoodsShipmentItem
{
	IReadOnlyCollection<IH1CommonPreviousDocument> PreviousDocuments { get; }
}
