using CargoWise.Customs.IT.MessageContracts;

namespace Enterprise.Customs.IT.H7.Business;

public sealed class AddressWrapper : IAddress
{
	public AddressWrapper(string name, string streetAndNumber, string country, string zipCode, string city)
	{
		this.name = name;
		this.streetAndNumber = streetAndNumber;
		this.country = country;
		this.zipCode = zipCode;
		this.city = city;
	}

	readonly string name;
	readonly string streetAndNumber;
	readonly string country;
	readonly string zipCode;
	readonly string city;

	public string Name => name;

	public string StreetAndNumber => streetAndNumber;

	public string Country => country;

	public string ZipCode => zipCode;

	public string City => city;
}
