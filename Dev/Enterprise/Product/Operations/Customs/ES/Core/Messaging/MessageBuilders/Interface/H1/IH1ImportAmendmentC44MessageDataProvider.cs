using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders;

public interface IH1ImportAmendmentC44MessageDataProvider : IH1ImportCommonDataProvider
{
	ZString MRN { get; }
	ZString LRN { get; }
	IReadOnlyCollection<IH1ImportAmendmentC44GoodsItem> GoodsItems { get; }
}

public interface IH1ImportAmendmentC44GoodsItem : ICommonH1GoodsShipmentItem
{
	IReadOnlyCollection<IH1CommonLineSupportingDocument> SupportingDocuments { get; }
}
