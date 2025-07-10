using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers;

public class CommonPostcodeAddressWrapper : ICommonPostcodeAddress
{
	public CommonPostcodeAddressWrapper(ZString houseNumber, ZString postCode, ZString country)
	{
		HouseNumber = houseNumber;
		PostCode = postCode;
		Country = country;
	}

	public ZString HouseNumber { get; }

	public ZString PostCode { get; }

	public ZString Country { get; }
}
