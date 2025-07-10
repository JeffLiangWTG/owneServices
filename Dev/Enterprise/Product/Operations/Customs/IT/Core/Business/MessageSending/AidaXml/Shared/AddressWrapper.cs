using System;
using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;

public sealed class AddressWrapper : IAddress
{
	public AddressWrapper(IDocAddress docAddress)
	{
		Argument.NotNull(docAddress, nameof(docAddress));

		lazyAddress = new Lazy<string>(() => docAddress.GetAddressAsASingleLineForCustomsMessage());
		lazyCity = new Lazy<string>(() => docAddress.E2_City);
		lazyCountry = new Lazy<string>(() => docAddress.E2_RN_NKCountryCode);
		lazyName = new Lazy<string>(() => docAddress.E2_CompanyName);
		lazyZipCode = new Lazy<string>(() => docAddress.E2_Postcode);
	}

	string IAddress.StreetAndNumber => lazyAddress.Value;
	readonly Lazy<string> lazyAddress;

	string IAddress.City => lazyCity.Value;
	readonly Lazy<string> lazyCity;

	string IAddress.Country => lazyCountry.Value;
	readonly Lazy<string> lazyCountry;

	string IAddress.Name => lazyName.Value;
	readonly Lazy<string> lazyName;

	string IAddress.ZipCode => lazyZipCode.Value;
	readonly Lazy<string> lazyZipCode;
}
