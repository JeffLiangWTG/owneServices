using CargoWise.Customs.CH.MessageContracts.Edec;
using CargoWise.Customs.CH.MessageContracts.Edec.GoodsDeclarations;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.CH.Business;

public class EdecAddressDataProvider : IEdecAddress
{
	public static EdecAddressDataProvider New(OrgAddress orgAddress, bool returnFakeAddressIfNull = false) => orgAddress == null && !returnFakeAddressIfNull ? null : new EdecAddressDataProvider(orgAddress);

	public static EdecAddressDataProvider New(OrgHeader organization, bool returnFakeAddressIfNull = false) => New(organization?.MainAddress, returnFakeAddressIfNull);

	public static EdecAddressDataProvider New(JobDocAddress docAddress, bool returnFakeAddressIfEmpty = false)
	{
		return (docAddress != null && (docAddress.E2_AddressOverride || docAddress.HasRealAddress))
			? new EdecAddressDataProvider(docAddress) : New((OrgAddress)null, returnFakeAddressIfEmpty);
	}

	EdecAddressDataProvider(IDocAddress docAddress)
	{
		this.docAddress = docAddress;
		organization = docAddress?.Organisation as OrgHeader;
	}
	readonly IDocAddress docAddress;
	readonly OrgHeader organization;

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
	const string UndefinedCompanyName = "-";
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
	const string UndefinedPostalCode = ".";
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
	const string UndefinedCityCode = "-";
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
	const string UndefinedCountryCode = "xx";

	public string Name => (docAddress?.E2_CompanyName).ReturnDefaultValueIfNullOrEmpty(UndefinedCompanyName);

	public string AddressSupplement1 => (docAddress?.E2_AdditionalAddressInformation.SubstringSafe(0, EdecMessageSchema.AddressSupplement1MaxLength)).ReturnDefaultValueIfNullOrEmpty(null);

	public string AddressSupplement2 => (docAddress?.E2_AdditionalAddressInformation.SubstringSafe(EdecMessageSchema.AddressSupplement1MaxLength, EdecMessageSchema.AddressSupplement2MaxLength)).ReturnDefaultValueIfNullOrEmpty(null);

	public string AddressSupplement3 => null;

	public string Street => (docAddress?.E2_Address1).ReturnDefaultValueIfNullOrEmpty(null);

	public string PostalCode
	{
		get
		{
			string postCode = docAddress?.E2_Postcode ?? UndefinedPostalCode;
			if (string.IsNullOrEmpty(postCode))
			{
				var country = RefCountry.LoadFromCountryCode(organization.Factory, docAddress.CountryCode);
				if (country != null && country.RN_PostcodeValidationRule == CountryAddressValidationRuleList.Codes.NoValidationRule)
				{
					postCode = UndefinedPostalCode;
				}
			}
			return postCode;
		}
	}

	public string City => (docAddress?.E2_City).ReturnDefaultValueIfNullOrEmpty(UndefinedCityCode);

	public string Country => (docAddress?.CountryCode).ReturnDefaultValueIfNullOrEmpty(UndefinedCountryCode);

	public string TraderIdentificationNumber
	{
		get
		{
			string traderIdentificationNumber = null;
			if (Country == Core.Constants.CountryCodes.Switzerland)
			{
				traderIdentificationNumber = organization?.GetUIDNumber().Left(12);
			}
			return traderIdentificationNumber;
		}
	}

	public string Reference => (docAddress?.E2_AdditionalAddressInformation).ReturnDefaultValueIfNullOrEmpty(null);
}
