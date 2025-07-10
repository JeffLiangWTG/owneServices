using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Integration.Accounting
{
	public interface IGlobalEInvoicingObjectFactory
	{
		ICountryEInvoicingObjectFactorySettings GetCountryEInvoicingObjectFactorySettings(ZString countryCode);
		ICountryEInvoicingRegistryInformationProvider GetCountryEInvoicingRegistryInformationProvider(ZString countryCode);

		/// <summary>
		/// A list of all countries which are supported for Electronic Invoicing in CargoWise One
		/// </summary>
		IReadOnlyCollection<ZString> SupportedCountryCodes { get; }

		/// <summary>
		/// Check if the country is on the list of countries that support Electronic Invoicing in CargoWise one.
		/// </summary>
		/// <param name="countryCode">Enterprise.Core.Constants.CountryCodes</param>
		/// <returns>Returns true if countryCode is into the list of countries wich are supported for electronic invoice or false if not.</returns>
		ZBool DoesCountrySupportElectronicInvoicing(ZString countryCode);

		IBatchCreatorStrategy GetCountryEInvoicingBatchCreatorStrategy(ZString countryCode);
	}
}
