using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.NCTS.Departure;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml;

sealed class AddressTypeWrapper : AddressTypeDataProviderAbstractClass
{
	public AddressTypeWrapper(IAddress address)
	{
		this.address = Argument.NotNull(address, nameof(address));
	}
	readonly IAddress address;

	public override string StreetAndNumber => address.StreetAndNumber;

	public override string Country => address.Country;

	public override string Postcode => address.ZipCode;

	public override string City => address.City.Trim();
}
