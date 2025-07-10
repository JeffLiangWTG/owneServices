using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.TP5.Interfaces;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5
{
	public class ContactPersonWrapper : IContactPerson
	{
		ContactPersonWrapper(OrgContact contact)
		{
			this.contact = Argument.NotNull(contact, nameof(contact));
		}

		readonly OrgContact contact;

		public static ContactPersonWrapper New(OrgContact contact) => contact == null ? null : new ContactPersonWrapper(contact);

		public string EmailAddress => emailAddress ?? (emailAddress = contact.OC_Email);
		string emailAddress;

		public string Name => name ?? (name = contact.OC_ContactName);
		string name;

		public string PhoneNumber => phoneNumber ?? (phoneNumber = contact.OC_Phone_Formatted);
		string phoneNumber;
	}
}
