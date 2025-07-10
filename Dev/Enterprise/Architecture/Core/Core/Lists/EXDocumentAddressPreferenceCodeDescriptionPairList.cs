using Enterprise.MasterFiles.Business;

namespace Enterprise.ZArchitecture.Core
{
	public class EXDocumentAddressPreferenceCodeDescriptionPairList : CodeDescriptionPairList
	{
		public EXDocumentAddressPreferenceCodeDescriptionPairList()
			: base()
		{
			AddPair(OrgConstants.AddressType.Documentary, OrgDescriptions.AddressType.DocumentaryDescription);
			AddPair(OrgConstants.AddressType.Office, OrgDescriptions.AddressType.OfficeDescription);
			AddPair(OrgConstants.AddressType.Pickup, OrgDescriptions.AddressType.PickupDescription);
		}
	}
}
