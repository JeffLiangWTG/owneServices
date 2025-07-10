using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Common.AU.CMR
{
	// CH_EntryStatus
	public class CMRImportEntryAdvice : CodeDescriptionPair
	{
		protected CMRImportEntryAdvice(object code, MultilingualString description)
			: base(code, description)
		{
		}

		public static readonly CMRImportEntryAdvice Processing = new CMRImportEntryAdvice("PLV", ResString.GetMultilingualString("CMRImportEntryAdvice|PLV", "Pre-lodged"));
		public static readonly CMRImportEntryAdvice Held = new CMRImportEntryAdvice("HLD", ResString.GetMultilingualString("CMRImportEntryAdvice|HLD", "Held - Quarantine and/or Customs Impediment"));
		public static readonly CMRImportEntryAdvice Clear = new CMRImportEntryAdvice("CLR", ResString.GetMultilingualString("CMRImportEntryAdvice|CLR", "Clear - For Payment, refer to 'Entries'"));
		public static readonly CMRImportEntryAdvice Finalised = new CMRImportEntryAdvice("FIN", ResString.GetMultilingualString("CMRImportEntryAdvice|FIN", "Finalized - Paid"));
		public static readonly CMRImportEntryAdvice Withdrawn = new CMRImportEntryAdvice("WTH", ResString.GetMultilingualString("CMRImportEntryAdvice|WTH", "Withdrawn"));
		public static readonly CMRImportEntryAdvice Rejected = new CMRImportEntryAdvice("REJ", ResString.GetMultilingualString("CMRImportEntryAdvice|REJ", "Refund (Amendment) rejected"));
		public static readonly CMRImportEntryAdvice MultiStatus = new CMRImportEntryAdvice("PRT", ResString.GetMultilingualString("CMRImportEntryAdvice|PRT", "Multiple Entries / Status"));
		public static readonly CMRImportEntryAdvice DeclarationWorkComplete = new CMRImportEntryAdvice("DWC", ResString.GetMultilingualString("CMRImportEntryAdvice|DWC", "Declaration Work Complete"));
		public static readonly CMRImportEntryAdvice ATDReceived = new CMRImportEntryAdvice("ATD", ResString.GetMultilingualString("CMRImportEntryAdvice|ATD", "Authority to Deal"));
	}
}
