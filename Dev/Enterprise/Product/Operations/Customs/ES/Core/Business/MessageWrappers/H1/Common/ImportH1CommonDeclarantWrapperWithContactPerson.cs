using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.MessageWrappers;

public class ImportH1CommonDeclarantWrapperWithContactPerson : PartyIdWrapper, IPartyIdProviderWithContactPerson
{
	public static ImportH1CommonDeclarantWrapperWithContactPerson New(JobDeclaration jobDeclaration)
	{
		OrgAddress orgAddress = null;
		if (jobDeclaration != null)
		{
			var importerAddress = jobDeclaration?.ImporterDocumentaryAddress.Address;
			var orgAddressDeclarantOrImporter = jobDeclaration?.DeclarantOrgAddress?.Header != null ? jobDeclaration.DeclarantOrgAddress : importerAddress;
			if (jobDeclaration.JE_DeclarantType == (ZString)ESRepresentationTypeList.Codes._1Auto)
			{
				orgAddress = orgAddressDeclarantOrImporter;
			}
			else
			{
				orgAddress = GetDeclarantOrgAddress(jobDeclaration, orgAddressDeclarantOrImporter, importerAddress);
			}
		}
		return orgAddress?.Header == null ? null : new ImportH1CommonDeclarantWrapperWithContactPerson(jobDeclaration, orgAddress);
	}

	protected ImportH1CommonDeclarantWrapperWithContactPerson(JobDeclaration jobDeclaration, OrgAddress orgA) : base(orgA.Header)
	{
		declaration = Argument.NotNull(jobDeclaration, nameof(jobDeclaration));
		orgAddress = orgA;
		declarantTypeIsNot2Or5 = DeclarantTypeIsNot2Or5(declaration);
	}
	readonly JobDeclaration declaration;
	readonly OrgAddress orgAddress;
	readonly bool declarantTypeIsNot2Or5;

	public IPartyContactProvider ContactPerson => contactPerson ?? (contactPerson = declarantTypeIsNot2Or5
	? (contactPerson = orgAddress?.Header != null ? PartyContactWrapper.New(orgAddress.Header.OH_FullName, ContactEmail, orgAddress.OA_Phone) : null)
		: null);
	PartyContactWrapper contactPerson;

	ZString ContactEmail => GetContactEmailForImportH1(declaration);
}
