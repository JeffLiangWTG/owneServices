
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Common.AU.CMR
{
	public class CMRDocumentStatus : CodeDescriptionPair
	{
		protected CMRDocumentStatus(object code, MultilingualString description)
			: base(code, description)
		{
		}

		public static readonly CMRDocumentStatus Cancelled = new CMRDocumentStatus("CANCELLED", ResString.GetMultilingualString("CMRDocumentStatus|Cancelled", "The authority to deal has been canceled by Customs. The authority is no longer valid for Export."));
		public static readonly CMRDocumentStatus Clear = new CMRDocumentStatus("CLEAR", ResString.GetMultilingualString("CMRDocumentStatus|CLEAR", "The goods covered by the document are authorized for export and may be dealt with."));
		public static readonly CMRDocumentStatus Error = new CMRDocumentStatus("ERROR", ResString.GetMultilingualString("CMRDocumentStatus|ERROR", "The goods covered by the document cannot be dealt with."));
		public static readonly CMRDocumentStatus Rejected = new CMRDocumentStatus("REJECTED", ResString.GetMultilingualString("CMRDocumentStatus|REJECTED", "The document has been rejected due to errors. Please correct and re-send the message."));
		public static readonly CMRDocumentStatus Revoked = new CMRDocumentStatus("REVOKED", ResString.GetMultilingualString("CMRDocumentStatus|REVOKED", "The goods have not been exported within the allowed period after the notified date of exportation. A new EDN/CRN should be lodged to export the goods."));
		public static readonly CMRDocumentStatus Withdrawn = new CMRDocumentStatus("WITHDRAWN", ResString.GetMultilingualString("CMRDocumentStatus|WITHDRAWN", "The export declaration or cargo report has been withdrawn by the reporting party and may not be dealt with."));
		public static readonly CMRDocumentStatus Lodged = new CMRDocumentStatus("LODGED", ResString.GetMultilingualString("CMRDocumentStatus|LODGED", "The Drawback Claim has been successfully lodged."));
	}
}
