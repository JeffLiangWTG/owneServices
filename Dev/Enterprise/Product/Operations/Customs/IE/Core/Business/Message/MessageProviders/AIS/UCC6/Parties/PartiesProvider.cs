using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class PartiesProvider : IParties
	{
		readonly JobDeclaration declaration;

		public PartiesProvider(JobDeclaration declaration)
		{
			this.declaration = declaration;
		}

		public string Applicant => declaration.Declarant?.GetEORI() ?? ZString.Empty;

		public string RepresentativeIdentification => declaration.Representative?.GetEORI() ?? ZString.Empty;

		public IContactPerson ContactPerson => ContactPersonProvider.New(declaration.Declarant?.Header?.AllocatedContacts.GetAllocatedContact(OrgConstants.ContactAllocationType.CUS));
	}
}
