using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.H7.Business
{
	public class PartiesProvider : IParties
	{
		public PartiesProvider(AsycudaManifestHeader header)
		{
			this.header = header;
		}

		readonly AsycudaManifestHeader header;

		public string Applicant => header.Declarant?.GetEORI() ?? null;

		public string RepresentativeIdentification => header.Representative?.GetEORI() ?? null;

		public IContactPerson ContactPerson => ContactPersonProvider.New(header.Declarant?.Header?.AllocatedContacts?.GetAllocatedContact(OrgConstants.ContactAllocationType.CUS));
	}
}
