using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.Business;

public abstract class BaseParticipentDataProvider
{
	protected BaseParticipentDataProvider(JobDocAddress docAddress, string contactType = null, bool checkCountryForAeoReferenceNumber = true)
	{
		this.docAddress = docAddress;
		this.orgHeader = docAddress.Organisation;
		this.contactType = contactType;
		this.checkCountryForAeoReferenceNumber = checkCountryForAeoReferenceNumber;
	}
	protected readonly JobDocAddress docAddress;
	protected readonly OrgHeader orgHeader;
	readonly bool checkCountryForAeoReferenceNumber;
	readonly string contactType;

	public string IdentificationNumber => identificationNumber ??= GetIdentificationNumber();
	string identificationNumber;

	protected virtual string GetIdentificationNumber() => orgHeader.GetIdentificationNumberForCH(addressPK: docAddress.E2_OA_Address).ReturnNullIfEmpty();

	public string Name => IdentificationNumber != null ? null : docAddress.E2_CompanyName.ReturnNullIfEmpty();

	public IAddress Address => address ??= IdentificationNumber != null ? null : AddressDataProvider.New(docAddress);
	IAddress address;

	public string AeoReferenceNumber => CachedValueHelper.GetValue(ref aeoReferenceNumber, () =>
	checkCountryForAeoReferenceNumber && !(IsJobDocAddressCountryCNorGBorNO || IsJobDocAddressCountryInNCL010) ? null : docAddress.Organisation?.GetAEONumber().ReturnNullIfEmpty());
	CachedValue<string> aeoReferenceNumber;

	public IContactPerson ContactPerson => contactPerson ??= GetContactPerson();
	IContactPerson contactPerson;

	protected virtual IContactPerson GetContactPerson() => contactType == null || docAddress.E2_AddressOverride ? ContactPersonDataProvider.New(docAddress) : OrgContactPersonDataProvider.New(AllocatedContact);

	protected OrgContact AllocatedContact => orgHeader?.AllocatedContacts.GetAllocatedContact(contactType);

	bool IsJobDocAddressCountryInNCL010 => RefCusCodeListTypes.GetCachedList(docAddress.Factory,
		Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, UniversalReferenceConstants.RefCusCodeList.PassarTypes.EUCountries, ZDateTime.Today).ContainsCode(docAddress.E2_RN_NKCountryCode);

	bool IsJobDocAddressCountryCNorGBorNO
	{
		get
		{
			switch (docAddress.E2_RN_NKCountryCode)
			{
				case Core.Constants.CountryCodes.UnitedKingdom:
				case Core.Constants.CountryCodes.Norway:
				case Core.Constants.CountryCodes.China:
					return true;
				default:
					return false;
			}
		}
	}
}
