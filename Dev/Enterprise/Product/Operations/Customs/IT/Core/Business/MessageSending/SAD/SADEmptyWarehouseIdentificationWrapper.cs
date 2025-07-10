using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.Business;

public class SADEmptyWarehouseIdentificationWrapper : IWarehouseIdentification
{
	public ZString Type => ZString.Empty;

	public ZString Identification => ZString.Empty;

	public ZString CinIdentification => ZString.Empty;

	public ZString AuthorizingCountry => ZString.Empty;
}
