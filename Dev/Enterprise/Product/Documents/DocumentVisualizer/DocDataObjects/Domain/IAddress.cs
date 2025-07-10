using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.DocumentVisualizer.DocDataObjects
{
	public interface IAddress
	{
		ZGuid AddressIdentifier { get; set; }
		ZGuid HeaderIdentifier { get; set; }
		ZString CompanyName { get; set; }
		ZString AddressLine1 { get; set; }
		ZString AddressLine2 { get; set; }
		ZString AdditionalAddressInformation { get; set; }
		ZString City { get; set; }
		ZString State { get; set; }
		ZString Postcode { get; set; }
		ZString Phone { get; set; }
		ZString Fax { get; set; }
		ZString Email { get; set; }
		ZString Contact { get; set; }

		ZString AddressFormatted { get; set; }

		ZString TaxNumber { get; set; }
		ICodeDescription TaxNumberType { get; }

		ICountry Country { get; }
		IUnloco Unloco { get; }

		IReadOnlyCollection<IRegistrationNumber> RegistrationNumbers { get; }
	}
}
