using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders;

public interface ICommonDeliveryTerms
{
	ZString Incoterm { get; }
	ZString UNLCode { get; }
	ZString IncotermLocation { get; }
	ZString DeliveryCountry { get; }
	ZString DeliveryText { get; }
}
