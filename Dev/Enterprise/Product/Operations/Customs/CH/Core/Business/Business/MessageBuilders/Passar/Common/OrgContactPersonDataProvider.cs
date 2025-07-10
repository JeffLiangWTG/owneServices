using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.Business;

public class OrgContactPersonDataProvider : IContactPerson
{
	public static OrgContactPersonDataProvider New(OrgContact contact) => contact == null ? null : new OrgContactPersonDataProvider(contact);

	OrgContactPersonDataProvider(OrgContact contact)
	{
		this.contact = contact;
	}

	readonly OrgContact contact;

	public string EmailAddress => contact.EmailFallbackToOrganisation.ReturnNullIfEmpty();

	public string Name => contact.OC_ContactName;

	public string PhoneNumber => contact.PhoneFallbackToOrganisation.ReturnNullIfEmpty();
}
