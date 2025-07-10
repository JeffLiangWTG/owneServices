using CargoWise.Types;

namespace Enterprise.Customs.IT.Messaging.EMCS;

public interface ITrader
{
	ZString TraderName { get; }

	ZString StreetName { get; }

	ZString StreetNumber { get; }

	ZString Postcode { get; }

	ZString City { get; }

	ZString LanguageDescriptions { get; }
}

public interface IConsignorTrader : ITrader
{
	ZString TraderExciseNumber { get; }
}

public interface IPlaceOfDispatchTrader : ITrader
{
	ZString ReferenceOfTaxWarehouse { get; }
}

public interface IConsigneeTrader : ITrader
{
	ZString TraderId { get; }

	ZString EoriNumber { get; }
}

public interface IDeliveryPlaceTrader : ITrader
{
	ZString TraderId { get; }
}

public interface ITransportTrader : ITrader
{
	ZString VatNumber { get; }
}

public interface IGuarantorTrader : ITrader
{
	ZString TraderExciseNumber { get; }

	ZString VatNumber { get; }
}
