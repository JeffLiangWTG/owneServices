#if DEBUG

namespace Enterprise.Accounting.Business.JobInvoicing.Posting
{
	static partial class ChargeSplitterByChargeCountAndValue
	{
		public static bool ShouldChargesBeSplit_ForTestOnly(IReceivablesPostingChargeCollection charges) => ShouldChargesBeSplit(charges);
	}
}

#endif
