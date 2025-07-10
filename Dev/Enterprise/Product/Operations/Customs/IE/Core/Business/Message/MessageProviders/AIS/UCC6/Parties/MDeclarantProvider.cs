using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class MDeclarantProvider : PartyProvider, IMDeclarant
	{
		MDeclarantProvider(OrgAddress address) : base(address) { }

		public static MDeclarantProvider New(OrgAddress address)
		{
			return address == null ? null : new MDeclarantProvider(address);
		}

		public CargoWise.Customs.IE.MessageContracts.Interfaces.IContact ContactPerson
		{
			get
			{
				if (contactPersonCached == null)
				{
					var contact = address.Header?.AllocatedContacts?.GetAllocatedContact(OrgConstants.ContactAllocationType.CUS);
					if (contact != null)
					{
						contactPersonCached = new ContactProvider(contact.OC_ContactName, contact.OC_Phone, contact.Email);
					}
				}
				return contactPersonCached;
			}
		}
		CargoWise.Customs.IE.MessageContracts.Interfaces.IContact contactPersonCached;

		public IContactDetails ContactDetails => null; // To be implemented in a future WI
	}
}
