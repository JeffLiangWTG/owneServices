using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Common.AU.CMR
{
	public class CMR3CharDocumentStatus : CodeDescriptionPair
	{
		protected CMR3CharDocumentStatus(object code, MultilingualString description)
			: base(code, description)
		{
		}

		public static readonly CMR3CharDocumentStatus Cancelled = new CMR3CharDocumentStatus("CAN", ResString.GetMultilingualString("CMR3CharDocumentStatus|CAN", "The authority to deal has been canceled by Customs. The authority is no longer valid for Export."));
		public static readonly CMR3CharDocumentStatus Clear = new CMR3CharDocumentStatus("CLR", ResString.GetMultilingualString("CMR3CharDocumentStatus|CLR", "The goods covered by the document are authorized for export and may be dealt with."));
		public static readonly CMR3CharDocumentStatus Error = new CMR3CharDocumentStatus("ERR", ResString.GetMultilingualString("CMR3CharDocumentStatus|ERR", "The goods covered by the document cannot be dealt with."));
		public static readonly CMR3CharDocumentStatus Rejected = new CMR3CharDocumentStatus("REJ", ResString.GetMultilingualString("CMR3CharDocumentStatus|REJ", "The document has been rejected due to errors. Please correct and re-send the message."));
		public static readonly CMR3CharDocumentStatus Revoked = new CMR3CharDocumentStatus("REV", ResString.GetMultilingualString("CMR3CharDocumentStatus|REV", "The goods have not been exported within the allowed period after the notified date of exportation. A new EDN/CRN should be lodged to export the goods."));
		public static readonly CMR3CharDocumentStatus Withdrawn = new CMR3CharDocumentStatus("WDW", ResString.GetMultilingualString("CMR3CharDocumentStatus|WDW", "The export declaration or cargo report has been withdrawn by the reporting party and may not be dealt with."));

		public static CMR3CharDocumentStatus GetFromStatusText(ZString statusText)
		{
			CMR3CharDocumentStatus result = null;
			if (statusText.IndexOf("CANCELLED") != -1)
			{
				result = CMR3CharDocumentStatus.Cancelled;
			}
			else if (statusText.IndexOf("CLEAR") != -1)
			{
				result = CMR3CharDocumentStatus.Clear;
			}
			else if (statusText.IndexOf("ERROR") != -1)
			{
				result = CMR3CharDocumentStatus.Error;
			}
			else if (statusText.IndexOf("REJECTED") != -1)
			{
				result = CMR3CharDocumentStatus.Rejected;
			}
			else if (statusText.IndexOf("REVOKED") != -1)
			{
				result = CMR3CharDocumentStatus.Revoked;
			}
			else if (statusText.IndexOf("WITHDRAWN") != -1)
			{
				result = CMR3CharDocumentStatus.Withdrawn;
			}

			return result;
		}
	}
}
