using CargoWise.Common;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.MessageWrappers;

public class AESCommonDeclarantWrapperWithContactPerson : AESCommonDeclarantWrapper, IPartyIdProviderWithContactPerson
{
	public static new AESCommonDeclarantWrapperWithContactPerson New(JobDeclaration jobDeclaration)
	{
		OrgAddress orgAddress = null;
		if (jobDeclaration != null)
		{
			var exporterAddress = jobDeclaration?.SupplierDocumentaryAddress.Address;
			var orgAddressDeclarantOrExporter = jobDeclaration?.DeclarantOrgAddress?.Header != null ? jobDeclaration.DeclarantOrgAddress : exporterAddress;
			orgAddress = GetDeclarantOrgAddress(jobDeclaration, orgAddressDeclarantOrExporter, exporterAddress);
		}
		return orgAddress?.Header == null ? null : new AESCommonDeclarantWrapperWithContactPerson(jobDeclaration, orgAddress);
	}

	AESCommonDeclarantWrapperWithContactPerson(JobDeclaration jobDeclaration, OrgAddress orgA) : base(orgA.Header)
	{
		declaration = Argument.NotNull(jobDeclaration, nameof(jobDeclaration));
		orgAddress = orgA;
		declarantTypeIsNot2Or5 = DeclarantTypeIsNot2Or5(declaration);
	}

	readonly JobDeclaration declaration;
	readonly OrgAddress orgAddress;
	readonly bool declarantTypeIsNot2Or5;

	public IPartyContactProvider ContactPerson => contactPerson ?? (contactPerson = declarantTypeIsNot2Or5
		? (contactPerson = orgAddress?.Header != null ? PartyContactWrapper.New(orgAddress.Header.OH_FullName, GetEmailFromAddress(orgAddress, declaration), GetPhoneFromAddress(orgAddress)) : null)
		: null);
	PartyContactWrapper contactPerson;
}
