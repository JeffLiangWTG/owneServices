using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BE.Business;

public class PartyProvider : IParty
{
	public PartyProvider(JobDocAddress docAddress, bool isTransitionPeriodAES30 = false)
		: this(Argument.NotNull(docAddress, nameof(docAddress)).Address, isTransitionPeriodAES30: isTransitionPeriodAES30)
	{
	}

	public PartyProvider(OrgAddress orgAddress, bool isTransitionPeriodAES30 = false)
	{
		this.orgAddress = orgAddress;
		orgHeader = orgAddress?.Header;
		isTransitionPeriod = isTransitionPeriodAES30;
	}

	protected readonly OrgAddress orgAddress;
	protected readonly OrgHeader orgHeader;
	protected readonly bool isTransitionPeriod;

	public string IdentificationNumber => CachedValueHelper.GetValue(ref identificationNumber, orgHeader.GetIdentificationNumber);
	CachedValue<string> identificationNumber;

	public string Name => CachedValueHelper.GetValue(ref name, GetNameCore) ;
	CachedValue<string> name;

	string GetNameCore() => string.IsNullOrWhiteSpace(IdentificationNumber) ? orgHeader?.OH_FullName : null;

	public IAddress Address => CachedValueHelper.GetValue(ref address, GetAddressCore);
	CachedValue<IAddress> address;

	IAddress GetAddressCore()
	{
		return string.IsNullOrWhiteSpace(IdentificationNumber) && !string.IsNullOrWhiteSpace(Name)
			? new AddressProvider(orgAddress, isTransitionPeriodAES30: isTransitionPeriod)
			: null;
	}

	public virtual IContactPerson ContactPerson => null;

	public int NameMaxlength => isTransitionPeriod ? MessageSchema.PartyNameMaxLengthInTransitionPeriod : MessageSchema.PartyNameMaxLength;
}
