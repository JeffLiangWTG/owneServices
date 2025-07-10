using CargoWise.Customs.IE.MessageContracts.Interfaces;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business
{
	public class ContactPersonProvider : IContactPerson
	{
		public static ContactPersonProvider New(OrgContact contact) => contact == null ? null : new ContactPersonProvider(contact.OC_ContactName, contact.PhoneFallbackToOrganisation, contact.FaxFallbackToOrganisation, contact.EmailFallbackToOrganisation);

		ContactPersonProvider(string name, string phone, string fax, string email)
		{
			Name = name;
			PhoneNumber = phone;
			Fax = fax;
			EmailAddress = email;
		}

		public string Name { get; }

		public string PhoneNumber { get; }

		public string Fax { get; }

		public string EmailAddress { get; }
	}
}
