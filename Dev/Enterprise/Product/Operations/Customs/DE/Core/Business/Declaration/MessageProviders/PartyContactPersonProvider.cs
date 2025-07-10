using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business
{
	public class PartyContactPersonProvider : IAESPartyContactPerson
	{
		readonly OrgContact contact;

		public static PartyContactPersonProvider NewOrNull(OrgContact contact) => contact == null ? null : new PartyContactPersonProvider(contact);

		PartyContactPersonProvider(OrgContact contact)
		{
			this.contact = Argument.NotNull(contact, nameof(contact));
		}

		public string Position => contact.OC_Title;

		public string PersonName => contact.OC_ContactName;

		public string PhoneNumber => contact.OC_Phone;

		public string FacsimileNumber => contact.OC_Fax;

		public string MailAddress => contact.OC_Email;
	}
}
