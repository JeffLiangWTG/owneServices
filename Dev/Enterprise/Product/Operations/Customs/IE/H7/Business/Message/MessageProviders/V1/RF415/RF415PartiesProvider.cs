using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.Customs.IE.MessageContracts.Interfaces.AIS.H7V1;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.H7.Business.Messaging.V1
{
	public class RF415PartiesProvider : IRF415PartiesType
	{
		readonly AsycudaManifestHeader header;

		public RF415PartiesProvider(AsycudaManifestHeader header)
		{
			this.header = header;
		}

		public string Applicant => header.Declarant?.GetEORI() ?? ZString.Empty;

		public string RepresentativeIdentification => header.Representative?.GetEORI() ?? ZString.Empty;

		public IContactPerson ContactPerson => ContactPersonProvider.New(header.Declarant?.Header?.AllocatedContacts?.GetAllocatedContact(OrgConstants.ContactAllocationType.CUS));
	}
}
