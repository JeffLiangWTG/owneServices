using Enterprise.MasterFiles.Business;

namespace Enterprise.ZArchitecture.Core
{
	public class IMDocumentAddressPreferenceCodeDescriptionPairList : CodeDescriptionPairList
	{
		public IMDocumentAddressPreferenceCodeDescriptionPairList()
			: base()
		{
			AddPair(OrgConstants.AddressType.Documentary, OrgDescriptions.AddressType.DocumentaryDescription);
			AddPair(OrgConstants.AddressType.Office, OrgDescriptions.AddressType.OfficeDescription);
			AddPair(OrgConstants.AddressType.Delivery, OrgDescriptions.AddressType.DeliveryDescription);
		}
	}
}
