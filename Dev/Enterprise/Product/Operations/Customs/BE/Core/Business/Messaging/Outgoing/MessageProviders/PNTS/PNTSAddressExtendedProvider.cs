using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;

namespace Enterprise.Customs.BE.Business;

public class PNTSAddressExtendedProvider : IPNTSAddressExtended
{
	public PNTSAddressExtendedProvider(ZString street, ZString additionalLine, ZString country, ZString postcode, ZString city)
	{
		this.street = street;
		this.additionalLine = additionalLine;
		this.country = country;
		this.postcode = postcode;
		this.city = city;
	}
	readonly ZString street;
	readonly ZString additionalLine;
	readonly ZString country;
	readonly ZString postcode;
	readonly ZString city;

	public string Country => country;

	public string PostCode => postcode;

	public string City => city;

	public string Street => street;

	public string StreetAdditionalLine => additionalLine;

	public string Number => ZString.Empty;

	public string PoBox => ZString.Empty;

	public string SubDivision => ZString.Empty;
}
