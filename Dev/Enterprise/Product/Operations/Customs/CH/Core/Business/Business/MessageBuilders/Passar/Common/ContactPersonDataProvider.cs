using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.Business;

public class ContactPersonDataProvider : IContactPerson
{
	public static ContactPersonDataProvider New(JobDocAddress address) => address == null ? null : new ContactPersonDataProvider(address);

	ContactPersonDataProvider(JobDocAddress address)
	{
		this.address = address;
	}
	readonly JobDocAddress address;

	public string Name => address.E2_Contact.ReturnNullIfEmpty();

	public string EmailAddress => address.E2_Email.ReturnNullIfEmpty();

	public string PhoneNumber => address.E2_Phone.ReturnNullIfEmpty();
}
