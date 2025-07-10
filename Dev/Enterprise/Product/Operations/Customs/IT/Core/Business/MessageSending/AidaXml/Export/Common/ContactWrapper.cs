using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export;

public sealed class ContactWrapper : IContact
{
	ContactWrapper(ZString contactName, ZString phoneNumber, ZString emailAddress)
	{
		this.contactName = contactName;
		this.phoneNumber = phoneNumber;
		this.emailAddress = emailAddress;
	}

	readonly ZString contactName;
	readonly ZString phoneNumber;
	readonly ZString emailAddress;

	public static IContact NewOrNull(JobDocAddress docAddress)
	{
		Argument.NotNull(docAddress, nameof(docAddress));

		var contactName = docAddress.E2_Contact;
		var phoneNumber = docAddress.E2_Phone;
		var emailAddress = docAddress.E2_Email;

		return contactName.IsEmpty && phoneNumber.IsEmpty && emailAddress.IsEmpty
			? null
			: (IContact)new ContactWrapper(contactName, phoneNumber, emailAddress);
	}

	string IContact.Name => contactName;

	string IContact.PhoneNumber => phoneNumber;

	string IContact.EmailAddress => emailAddress;
}
