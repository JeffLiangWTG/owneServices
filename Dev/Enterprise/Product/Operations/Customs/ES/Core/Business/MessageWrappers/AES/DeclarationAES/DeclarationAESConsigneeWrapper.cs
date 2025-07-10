using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.MessageWrappers;

public class DeclarationAESConsigneeWrapper : PartyNameWrapper, IDeclarationAESConsignee
{
	public static DeclarationAESConsigneeWrapper New(OrgHeader orgHeader, OrgAddress orgAddress, ZBool isProvisionalPeriod, ZBool dontSendImporterId, JobDeclaration declaration) => orgHeader == null ? null : new DeclarationAESConsigneeWrapper(orgHeader, orgAddress, isProvisionalPeriod, dontSendImporterId, declaration);

	DeclarationAESConsigneeWrapper(OrgHeader orgH, OrgAddress orgA, ZBool isProvisionalPeriod, ZBool dontSendImporterId, JobDeclaration declaration) : base(orgH)
	{
		this.isProvisionalPeriod = isProvisionalPeriod;
		this.dontSendImporterId = dontSendImporterId;
		orgAddress = orgA ?? orgH.MainAddress;
		jobDeclaration = declaration;
	}

	readonly ZBool isProvisionalPeriod;
	readonly ZBool dontSendImporterId;
	readonly OrgAddress orgAddress;
	readonly JobDeclaration jobDeclaration;

	protected override ZString IdCore => dontSendImporterId ? ZString.Empty : base.IdCore;

	protected override ZString NameCore => Id.IsEmpty ? orgHeader.OH_FullName : ZString.Empty;

	public IPartyAddressProvider Address => address ?? (address = Id.IsEmpty ? AESCommonAddressWrapper.New(orgAddress.OA_Address1, orgAddress.OA_City, orgAddress.OA_PostCode, OrgAddressCountryCode, isProvisionalPeriod) : null);
	AESCommonAddressWrapper address;

	ZString OrgAddressCountryCode => jobDeclaration.GetDefaultTerritory(orgAddress.OA_RN_NKCountryCode);
}
