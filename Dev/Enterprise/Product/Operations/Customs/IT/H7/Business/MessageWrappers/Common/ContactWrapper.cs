using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.H7.Business;

public sealed class ContactWrapper : IContact
{
	public ContactWrapper(OrgContact orgContact)
	{
		this.orgContact = Argument.NotNull(orgContact, nameof(orgContact));
	}

	readonly OrgContact orgContact;

	public string Name => orgContact.OC_ContactName;

	public string PhoneNumber => orgContact.OC_Mobile;

	public string EmailAddress => orgContact.OC_Email;
}
