using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.MessageWrappers;

public class DeclarationAESExporterWrapper : AESCommonExporterWrapper, IDeclarationAESExporter
{
	public static DeclarationAESExporterWrapper New(JobDocAddress jobDocAddress, JobDeclaration declaration) => jobDocAddress?.Address?.Header == null ? null : new DeclarationAESExporterWrapper(jobDocAddress, declaration);

	DeclarationAESExporterWrapper(JobDocAddress jobDocAddress, JobDeclaration declaration) : base(jobDocAddress.Address.Header)
	{
		orgAddress = jobDocAddress.Address;
		this.declaration = declaration;
	}

	readonly OrgAddress orgAddress;
	readonly JobDeclaration declaration;

	public IPartyAddressProvider Address => address ??= GetAddressForNaturalPerson(orgAddress, AESCommonAddressWrapper.New(orgAddress.OA_Address1, orgAddress.OA_City, orgAddress.OA_PostCode, declaration.GetDefaultTerritory(orgAddress.OA_RN_NKCountryCode)));
	AESCommonAddressWrapper address;
}
