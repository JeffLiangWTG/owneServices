using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BE.Business;

public class PartyWithContactProvider : PartyProvider
{
	public PartyWithContactProvider(OrgAddress orgAddress, bool isTransitionPeriodAES30 = false) : base(orgAddress, isTransitionPeriodAES30: isTransitionPeriodAES30) { }

	public override IContactPerson ContactPerson => CachedValueHelper.GetValue(ref contactPerson,
		() => (string.IsNullOrWhiteSpace(IdentificationNumber) && Address is null) ? null
		: ContactPersonProvider.NewOrNull(GlbStaff.CurrentUser.GS_FullName, GlbStaff.CurrentUser.GS_WorkPhone, GlbStaff.CurrentUser.GS_EmailAddress));
	CachedValue<IContactPerson> contactPerson;
}
