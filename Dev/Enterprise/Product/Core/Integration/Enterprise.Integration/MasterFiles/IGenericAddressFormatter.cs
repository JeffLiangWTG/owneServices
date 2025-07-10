using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Integration.MasterFiles
{
	public interface IGenericAddressFormatter
	{
		string PostalAddress(BusinessObjectFactory factory, string name, string additionalAddressInformation, string address1, string address2, string city, string state, string postCode, IMultilingualString countryName, bool includeCountryEvenIfSame, string countryCode);
	}
}
