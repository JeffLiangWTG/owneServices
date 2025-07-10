using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Common.AU.CMR
{
	public class CMRNoticeStatus : CodeDescriptionPair
	{
		protected CMRNoticeStatus(object code, MultilingualString description)
			: base(code, description)
		{
		}

		public static readonly CMRNoticeStatus Clear = new CMRNoticeStatus("CLEAR", ResString.GetMultilingualString("CMRNoticeStatus|CLEAR", "The Notice has been accepted by Customs."));
		public static readonly CMRNoticeStatus Error = new CMRNoticeStatus("ERROR", ResString.GetMultilingualString("CMRNoticeStatus|ERROR", "The Notice has been placed into error. Please amend the error data."));
		public static readonly CMRNoticeStatus Rejected = new CMRNoticeStatus("REJECTED", ResString.GetMultilingualString("CMRNoticeStatus|REJECTED", "The Notice has been rejected due to errors. Please correct and re-send the message."));
		public static readonly CMRNoticeStatus Withdrawn = new CMRNoticeStatus("WITHDRAWN", ResString.GetMultilingualString("CMRNoticeStatus|WITHDRAWN", "The Notice has been successfully withdrawn."));
	}
}
