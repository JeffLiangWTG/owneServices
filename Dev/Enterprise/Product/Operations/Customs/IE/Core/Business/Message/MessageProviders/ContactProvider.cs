using CargoWise.Customs.IE.MessageContracts.Interfaces;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business
{
	public class ContactProvider : IContact
	{
		public static ContactProvider New(OrgContact orgContact) => orgContact == null ? null : new ContactProvider(orgContact.OC_ContactName, orgContact.PhoneFallbackToOrganisation, orgContact.EmailFallbackToOrganisation);

		public static ContactProvider New(OrgHeader header, bool fallbackToSingleContact = true)
		{
			ContactProvider result = null;
			if (header != null)
			{
				var contact = header.AllocatedContacts.GetAllocatedContact(OrgConstants.ContactAllocationType.CUS);
				if (contact == null && header.Contacts.Count == 1 && fallbackToSingleContact)
				{
					contact = header.Contacts[0];
				}
				result = New(contact);
			}
			return result;
		}

		public static ContactProvider New(JobDocAddress docAddress) => docAddress == null ? null : new ContactProvider(docAddress.E2_Contact, docAddress.E2_Phone, docAddress.E2_Email);

		public ContactProvider(string name, string phone, string email)
		{
			this.Name = name;
			this.PhoneNumber = phone;
			this.EmailAddress = email;
		}

		public string Name { get; }

		public string PhoneNumber { get; }

		public string EmailAddress { get; }
	}
}
