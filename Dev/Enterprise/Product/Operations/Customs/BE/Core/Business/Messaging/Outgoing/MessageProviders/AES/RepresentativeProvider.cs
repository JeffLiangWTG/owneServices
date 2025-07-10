using CargoWise.Customs.BE.MessageContracts;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BE.Business;

public class RepresentativeProvider : IAESRepresentative
{
	readonly string declarantType;
	readonly OrgAddress representativeAddress;
	readonly bool isTransitionPeriod;

	RepresentativeProvider(OrgAddress representativeAddress, string declarantType, bool isTransitionPeriodAES30)
	{
		this.declarantType = declarantType;
		this.representativeAddress = representativeAddress;
		isTransitionPeriod = isTransitionPeriodAES30;
	}

	public static RepresentativeProvider New(OrgAddress representativeAddress, string declarantType, bool isTransitionPeriodAES30 = false)
	{
		RepresentativeProvider provider = null;
		if (declarantType != RepresentationTypeList.Codes._1Self || !string.IsNullOrEmpty(GetIdentificationNumber(representativeAddress)))
		{
			provider = new RepresentativeProvider(representativeAddress, declarantType, isTransitionPeriodAES30);
		}

		return provider;
	}

	public string Status => CachedValueHelper.GetValue(ref status, () => declarantType == RepresentationTypeList.Codes._2Direct ? "2" : null);
	CachedValue<string> status;

	public string IdentificationNumber => CachedValueHelper.GetValue(ref identificationNumber, () => GetIdentificationNumber(representativeAddress));
	CachedValue<string> identificationNumber;

	public IContactPerson ContactPerson => CachedValueHelper.GetValue(ref contactPerson,
		() => (string.IsNullOrWhiteSpace(IdentificationNumber) ? null
		: ContactPersonProvider.NewOrNull(GlbStaff.CurrentUser.GS_FullName, GlbStaff.CurrentUser.GS_WorkPhone, GlbStaff.CurrentUser.GS_EmailAddress)));
	CachedValue<IContactPerson> contactPerson;

	public string Name => null;

	public IAddress Address => null;

	public int NameMaxlength => isTransitionPeriod ? MessageSchema.PartyNameMaxLengthInTransitionPeriod : MessageSchema.PartyNameMaxLength;

	static string GetIdentificationNumber(OrgAddress representativeAddress)
	{
		return representativeAddress?.Header.GetIdentificationNumber() ?? string.Empty;
	}
}
