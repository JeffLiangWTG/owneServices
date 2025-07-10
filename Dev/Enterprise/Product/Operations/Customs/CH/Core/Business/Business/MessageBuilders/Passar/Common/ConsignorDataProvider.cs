using System.Linq;
using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.Business;

public class ConsignorDataProvider : BaseParticipentDataProvider, IConsignor
{
	public static ConsignorDataProvider New(JobDocAddress jobDocAddress, string contactType = null, bool getContactByName = false) => jobDocAddress == null || jobDocAddress.IsEmpty ? null : new ConsignorDataProvider(jobDocAddress, contactType, getContactByName);

	ConsignorDataProvider(JobDocAddress jobDocAddress, string contactType, bool getContactByName) : base(jobDocAddress, contactType)
	{
		this.getContactByName = getContactByName;
	}

	readonly bool getContactByName;

	public string ReferenceNumber => null;

	public bool? PrivatePerson => orgHeader?.IsPrivatePerson();

	protected override IContactPerson GetContactPerson()
	{
		if (getContactByName)
		{
			if (docAddress.E2_Contact.IsEmpty)
			{
				return null;
			}
			else
			{
				var contact = orgHeader?.Contacts?.Where(c => c.Name == docAddress.E2_Contact).FirstOrDefault();
				return contact == null ? ContactPersonDataProvider.New(docAddress) : OrgContactPersonDataProvider.New(contact);
			}
		}
		else
		{
			return base.GetContactPerson();
		}
	}
}
