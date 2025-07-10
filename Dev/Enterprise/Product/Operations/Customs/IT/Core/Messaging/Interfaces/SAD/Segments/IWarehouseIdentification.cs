using CargoWise.Types;

namespace Enterprise.Customs.IT.Messaging.SAD;

public interface IWarehouseIdentification
{
	ZString Type { get; }
	ZString Identification { get; }
	ZString CinIdentification { get; }
	ZString AuthorizingCountry { get; }
}
