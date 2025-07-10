using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Common.AU
{
	public class EntryHeaderStatus : CodeDescriptionPair
	{
		EntryHeaderStatus(object code, MultilingualString description)
			: base(code, description)
		{
		}

		public static readonly EntryHeaderStatus NotSent = new EntryHeaderStatus("", ResString.GetMultilingualString("EntryHeaderStatus|NotSent", "Not Sent"));
		public static readonly EntryHeaderStatus CreateMessageCleared = new EntryHeaderStatus("CRE", ResString.GetMultilingualString("EntryHeaderStatus|CRE", "Create Message Cleared"));
		public static readonly EntryHeaderStatus CPDecMessageCleared = new EntryHeaderStatus("CPD", ResString.GetMultilingualString("EntryHeaderStatus|CPD", "CPDec Message Cleared"));
		public static readonly EntryHeaderStatus LodgeMessageCleared = new EntryHeaderStatus("LDG", ResString.GetMultilingualString("EntryHeaderStatus|LDG", "Lodge Message Cleared"));
		public static readonly EntryHeaderStatus PayMessageCleared = new EntryHeaderStatus("PAY", ResString.GetMultilingualString("EntryHeaderStatus|PAY", "Pay Message Cleared"));
		public static readonly EntryHeaderStatus LodgeImpediment = new EntryHeaderStatus("IMP", ResString.GetMultilingualString("EntryHeaderStatus|IMP", "Lodge Impediment"));
		public static readonly EntryHeaderStatus ReadyForPayment = new EntryHeaderStatus("RPA", ResString.GetMultilingualString("EntryHeaderStatus|RPA", "Ready for Payment"));
		public static readonly EntryHeaderStatus StopPayment = new EntryHeaderStatus("SPA", ResString.GetMultilingualString("EntryHeaderStatus|SPA", "Payment Stopped"));
		public static readonly EntryHeaderStatus GoodsReleased = new EntryHeaderStatus("REL", ResString.GetMultilingualString("EntryHeaderStatus|REL", "Goods Released"));
		public static readonly EntryHeaderStatus DocumentsToBeProduced = new EntryHeaderStatus("DOC", ResString.GetMultilingualString("EntryHeaderStatus|DOC", "Documents to be Produced"));
		public static readonly EntryHeaderStatus GoodsDetained = new EntryHeaderStatus("DET", ResString.GetMultilingualString("EntryHeaderStatus|DET", "Goods Detained"));
		public static readonly EntryHeaderStatus ManualProcedures = new EntryHeaderStatus("MAN", ResString.GetMultilingualString("EntryHeaderStatus|MAN", "Manual Procedures"));

		public static bool IsPostCreateStatus(string status)
		{
			return status == CreateMessageCleared.Code ||
				IsPostCPDecStatus(status);
		}

		public static bool IsPostCPDecStatus(string status)
		{
			return status == CPDecMessageCleared.Code ||
				IsPostLodgeStatus(status);
		}

		public static bool IsPostLodgeStatus(string status)
		{
			return status == LodgeMessageCleared.Code ||
				status == LodgeImpediment.Code ||
				status == ReadyForPayment.Code ||
				status == StopPayment.Code ||
				IsPostPayStatus(status);
		}

		public static bool IsPostPayStatus(string status)
		{
			return status == PayMessageCleared.Code ||
				status == GoodsReleased.Code ||
				status == DocumentsToBeProduced.Code ||
				status == GoodsDetained.Code ||
				status == ManualProcedures.Code;
		}
	}
}
