using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AE.Business;
using Enterprise.Edifact.D23A.Elements;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.AE.Manifest.Business;

sealed class PartyFromOrgAddressProvider : IPartyFromOrgAddressProvider
{
	public PartyFromOrgAddressProvider(OrgAddress orgAddress, string functionCode)
	{
		Party = Argument.NotNull(orgAddress, nameof(orgAddress));
		PartyFunctionCode = functionCode;
	}
	OrgAddress Party { get; }

	public string PartyName => partyName ??= Party.Header.OH_FullName;
	string partyName;

	public string StreetAddress => streetAddress ??= GetStreetAddress();
	string streetAddress;

	public string City => city ??= Party.OA_City;
	string city;

	public string Country => country ??= Party.OA_RN_NKCountryCode;
	string country;

	public string PartyFunctionCode { get; }

	public string PartyIdentifier => PartyOrgCusCode?.OK_CustomsRegNo;

	public string CodeListIdentificationCode => string.IsNullOrEmpty(PartyIdentifier) ? null : GetCodeListIdentificationCode();

	OrgCusCode PartyOrgCusCode => partyOrgCusCode ??= GetPartyOrgCusCode();
	OrgCusCode partyOrgCusCode;

	public IPartyContactCommunicationProvider ContactCommunication
		=> CachedValueHelper.GetValue(ref contactCommunication, () => GetContactCommunication());
	CachedValue<IPartyContactCommunicationProvider> contactCommunication;

	string GetStreetAddress()
	{
		var address1 = Party.OA_Address1;
		var address2 = Party.OA_Address2;
		if (!address1.IsEmpty && !address2.IsEmpty)
		{
			return $"{address1} {address2}";
		}
		return address1 + address2;
	}

	OrgCusCode GetPartyOrgCusCode()
	{
		if (PartyFunctionCodeQualifierList.Consignee == PartyFunctionCode)
		{
			return Party.Header?.CustomsCodes.GetAllOrgCusCodesForCountryAndCodes(
				CountryCodes.UnitedArabEmirates,
				new ZString[]
				{
					OrgCusCode.CodeTypes.PassportID,
					OrgCusCode.CodeTypes.TaxFileCode,
					OrgCusCode.UnitedArabEmiratesCodeTypes.IDNumber,
					OrgCusCode.CodeTypes.CorporationCode
				}).FirstOrDefault();
		}
		return null;
	}

	string GetCodeListIdentificationCode()
	{
		return (string)PartyOrgCusCode?.OK_CodeType switch
		{
			OrgCusCode.CodeTypes.TaxFileCode => EstablishmentsIdentificationCode,
			OrgCusCode.CodeTypes.PassportID => IndividualsIdentificationCode,
			OrgCusCode.UnitedArabEmiratesCodeTypes.IDNumber => IndividualsIdentificationCode,
			OrgCusCode.CodeTypes.CorporationCode => EstablishmentsIdentificationCode,
			_ => null
		};
	}

	PartyContactCommunicationProvider GetContactCommunication()
	{
		var phoneNumber = Party.OA_Phone;
		if (!phoneNumber.IsEmpty)
		{
			return new PartyContactCommunicationProvider(CommunicationMeansTypeCodeList.Telephone, phoneNumber);
		}

		var email = Party.OA_Email;
		if (!email.IsEmpty)
		{
			return new PartyContactCommunicationProvider(CommunicationMeansTypeCodeList.ElectronicMail, email);
		}

		var webUrl = Party.Header?.MainWebURL.PU_URL ?? ZString.Empty;
		if (!webUrl.IsEmpty)
		{
			return new PartyContactCommunicationProvider(CommunicationMeansTypeCodeList.UniformResourceLocationUrl, webUrl);
		}

		return null;
	}

	const string EstablishmentsIdentificationCode = "1";
	const string IndividualsIdentificationCode = "2";
}
