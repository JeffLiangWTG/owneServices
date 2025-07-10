using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class NctsBillAdditionalDocumentLookups : EU.NCTS.Business.NctsBillAdditionalDocumentLookups
	{
		public NctsBillAdditionalDocumentLookups(NctsBillAdditionalDocument parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList SubTypeList
		{
			get
			{
				return Factory.GetCachedValue("NctsAdditionalInfoPhase5Lookups.SubTypeList", () =>
				{
					var list = new AdditionalInfoSubTypeList();
					if (Parent.IsArrivalMovement)
					{
						list.RemoveCode(AdditionalInfoSubTypeList.Codes.AdditionalInformation);
						list.RemoveCode(AdditionalInfoSubTypeList.Codes.TransportDocument);
					}
					return list;
				});
			}
		}
	}
}
