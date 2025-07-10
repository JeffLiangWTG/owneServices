using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.NCTS.Departure;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml;

sealed class ContactPersonWrapper : ContactPersonTypeDataProviderAbstractClass
{
	public ContactPersonWrapper(IContact contact)
	{
		this.contact = Argument.NotNull(contact, nameof(contact));
	}
	readonly IContact contact;

	public override string Name => contact.Name;

	public override string PhoneNumber => contact.PhoneNumber;

	public override string EmailAddress => contact.EmailAddress;
}
