using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using CargoWise.Types;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.CH.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.CH.NCTS.Business;

public class HolderOfTransitProcedureDataProvider : IHolderOfTransitProcedure
{
	public static HolderOfTransitProcedureDataProvider New(JobDocAddress jobDocAddress) => jobDocAddress == null || jobDocAddress.IsEmpty ? null : new HolderOfTransitProcedureDataProvider(jobDocAddress);

	HolderOfTransitProcedureDataProvider(JobDocAddress jobDocAddress)
	{
		this.jobDocAddress = jobDocAddress;
		organization = jobDocAddress.Organisation;
	}
	readonly JobDocAddress jobDocAddress;
	readonly OrgHeader organization;

	public string IdentificationNumber => (identificationNumber ?? (identificationNumber = GetIdentificationNumber())).Value.ReturnNullIfEmpty();
	ZString? identificationNumber;

	public string Name => !IsOrgAddressCountryCH ? jobDocAddress.E2_CompanyName.ReturnNullIfEmpty() : null;

	public string TIRHolderIdentificationNumber => (tirHolderIdentificationNumber ?? (tirHolderIdentificationNumber = GetTIRHolderIdentificationNumber())).Value.ReturnNullIfEmpty();
	ZString? tirHolderIdentificationNumber;

	public IAddress Address => address ?? (address = !IsOrgAddressCountryCH ? AddressDataProvider.New(jobDocAddress) : null);
	IAddress address;

	public IContactPerson ContactPerson => contactPerson ??= jobDocAddress.E2_AddressOverride ? ContactPersonDataProvider.New(jobDocAddress) : StaffContactPersonDataProvider.New(GlbStaff.CurrentUser);
	IContactPerson contactPerson;

	bool IsOrgAddressCountryCH => !jobDocAddress.E2_AddressOverride && jobDocAddress.E2_RN_NKCountryCode == CountryCodes.Switzerland;

	ZString GetIdentificationNumber()
	{
		var result = ZString.Empty;
		if (!jobDocAddress.E2_AddressOverride && organization != null)
		{
			var country = jobDocAddress.E2_RN_NKCountryCode;
			switch (country)
			{
				case CountryCodes.Switzerland:
					result = organization.GetIdentificationNumberForCH();
					break;
				case CountryCodes.Norway:
					var customsRegNo = organization.GetCustomsRegNo(OrgCusCode.CodeTypes.OrganizationNumber, country);
					result = customsRegNo.IsEmpty ? string.Empty : CountryCodes.Norway + customsRegNo;
					break;
				case CountryCodes.Turkey:
					result = organization.GetCustomsRegNo(OrgCusCode.CodeTypes.VATCode, country);
					break;
				default:
					if (country == CountryCodes.UnitedKingdom || organization.Factory.IsMemberOfEU(country))
					{
						result = organization.GetIdentificationNumberForEU();
					}
					break;
			}
		}

		return result;
	}

	ZString GetTIRHolderIdentificationNumber()
	{
		var result = ZString.Empty;
		if (!jobDocAddress.E2_AddressOverride && organization != null)
		{
			result = organization.GetCHCustomsRegNo(OrgCusCode.CodeTypes.TIR_TransportsInternationauxRoutiers, true);
		}
		return result;
	}
}
