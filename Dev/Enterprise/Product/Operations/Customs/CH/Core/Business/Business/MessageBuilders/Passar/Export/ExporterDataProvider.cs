using System.Linq;
using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.Business;

public class ExporterDataProvider : BaseParticipentDataProvider, IExporter
{
	public static ExporterDataProvider New(JobDocAddress address) => address == null || address.IsEmpty ? null : new ExporterDataProvider(address);

	ExporterDataProvider(JobDocAddress address) : base(address) { }

	public bool PrivatePerson => orgHeader?.IsPrivatePerson() ?? false;

	protected override IContactPerson GetContactPerson()
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
}
