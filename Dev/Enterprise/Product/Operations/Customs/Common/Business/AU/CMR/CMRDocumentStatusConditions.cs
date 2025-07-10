
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Common.AU.CMR
{
	public class CMRDocumentStatusConditions : CodeDescriptionPair
	{
		protected CMRDocumentStatusConditions(object code, MultilingualString description)
			: base(code, description)
		{
		}
		public static readonly CMRDocumentStatusConditions Validation = new CMRDocumentStatusConditions("VALIDATION", ResString.GetMultilingualString("CMRDocumentStatusConditions|VALIDATION", "The lodged or amended information has not passed the required edits."));
		public static readonly CMRDocumentStatusConditions Suspended = new CMRDocumentStatusConditions("SUSPENDED", ResString.GetMultilingualString("CMRDocumentStatusConditions|SUSPENDED", "Customs has temporarily stopped the authority to deal."));
		public static readonly CMRDocumentStatusConditions Embargoed = new CMRDocumentStatusConditions("EMBARGOED", ResString.GetMultilingualString("CMRDocumentStatusConditions|EMBARGOED", "A country code subject to UN Sanctions has been detected by Customs. Please contact Customs."));
		public static readonly CMRDocumentStatusConditions Expired = new CMRDocumentStatusConditions("EXPIRED", ResString.GetMultilingualString("CMRDocumentStatusConditions|EXPIRED", "Customs does not expect further quotation of the related CAN."));
	}
}
