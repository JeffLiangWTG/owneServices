using System.Collections.Generic;
using CargoWise.Customs.IT.MessageContracts;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;

sealed class AddressEqualityComparer : IEqualityComparer<IAddress>
{
	public bool Equals(IAddress address1, IAddress address2)
	{
		if (ReferenceEquals(address1, address2))
		{
			return true;
		}

		if (address1 == null || address2 == null)
		{
			return false;
		}

		return address1?.StreetAndNumber == address2.StreetAndNumber
				&& address1?.City == address2.City
				&& address1?.Country == address2.Country
				&& address1?.Name == address2.Name
				&& address1?.ZipCode == address2.ZipCode;
	}

	public int GetHashCode(IAddress address)
	{
		if (address is null)
		{
			return 0;
		}

		var hashStreetAndNumber = address.StreetAndNumber.GetHashCode();
		var hashCity = address.City.GetHashCode();
		var hashCountry = address.Country.GetHashCode();
		var hashName = address.Name.GetHashCode();
		var hashZipCode = address.ZipCode.GetHashCode();

		return hashStreetAndNumber
				^ hashCity
				^ hashCountry
				^ hashName
				^ hashZipCode;
	}
}
