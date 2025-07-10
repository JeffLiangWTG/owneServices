using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EdiOrgMembershipImportInfo : ImportCollectionInfoImpl
	{
		public EdiOrgMembershipImportInfo(EdiOrgMembershipFlattenedCollection collection)
			: base(collection)
		{
			Add(new ImportPropertyInfoImpl<EdiOrgMembershipFlattened>(EdiOrgMembershipFlattened.Schema.OrgCode) { CharacterCasing = ZCharacterCasing.Upper });
			Add(new ImportPropertyInfoImpl<EdiOrgMembershipFlattened>(EdiOrgMembershipFlattened.Schema.MembershipType) { CharacterCasing = ZCharacterCasing.Upper });
			Add(new ImportPropertyInfoImpl<EdiOrgMembershipFlattened>(EdiOrgMembershipFlattened.Schema.ValidFrom));
			Add(new ImportPropertyInfoImpl<EdiOrgMembershipFlattened>(EdiOrgMembershipFlattened.Schema.ValidTo));
		}
	}
}
