using CargoWise.Customs.BE.MessageContracts.Interfaces;

namespace Enterprise.Customs.BE.Business;

public class ContactPersonProvider : IContactPerson
{
	ContactPersonProvider(string name, string phone, string email)
	{
		this.Name = name;
		this.PhoneNumber = phone;
		this.EMailAddress = email;
	}

	public string Name { get; private set; }

	public string PhoneNumber { get; private set; }

	public string EMailAddress { get; private set; }

	public static ContactPersonProvider NewOrNull(string name, string phone, string email) => string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(phone) ? null : new ContactPersonProvider(name, phone, email);
}
