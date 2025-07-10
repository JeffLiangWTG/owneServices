
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Common.AU.CMR
{
	public class CMRMovementStatus : CodeDescriptionPair
	{
		protected CMRMovementStatus(object code, MultilingualString description)
			: base(code, description)
		{
		}

		public static readonly CMRMovementStatus Consolidate = new CMRMovementStatus("CONSOLIDATE", ResString.GetMultilingualString("CMRMovementStatus|CONSOLIDATE", "The reported export shipment may be consolidated."));
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant Code")]
		public static readonly CMRMovementStatus DoNotConsolidate = new CMRMovementStatus("DO NOT CONSOLIDATE", ResString.GetMultilingualString("CMRMovementStatus|DONOTCONSOLIDATE", "The reported export shipment may not to be consolidated. Receival notice may contain incorrect information."));
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant Code")]
		public static readonly CMRMovementStatus DoNotLoad = new CMRMovementStatus("DO NOT LOAD", ResString.GetMultilingualString("CMRMovementStatus|DONOTLOAD", "The reported export shipment may not to be loaded. Receival notice may contain incorrect information."));
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant Code")]
		public static readonly CMRMovementStatus HoldForCustoms = new CMRMovementStatus("HOLD FOR CUSTOMS", ResString.GetMultilingualString("CMRMovementStatus|HOLDFORCUSTOMS", "Do Not Release. The reported export shipment is of interest to Customs."));
		public static readonly CMRMovementStatus Load = new CMRMovementStatus("LOAD", ResString.GetMultilingualString("CMRMovementStatus|LOAD", "The reported export shipment may be loaded."));
		public static readonly CMRMovementStatus Match = new CMRMovementStatus("MATCH", ResString.GetMultilingualString("CMRMovementStatus|MATCH", "The AHECCs and net quantities on the notice match those on the Export Declaration."));
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant Code")]
		public static readonly CMRMovementStatus NoMatch = new CMRMovementStatus("NO MATCH", ResString.GetMultilingualString("CMRMovementStatus|NOMATCH", "A discrepancy exists between the AHECCs or net quantities on the notice and those on the Export Declaration."));
		public static readonly CMRMovementStatus Rejected = new CMRMovementStatus("REJECTED", ResString.GetMultilingualString("CMRMovementStatus|REJECTED", "The transaction has been rejected due to errors. Please correct and re-send the message."));
	}
}
