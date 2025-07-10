#if DEBUG

namespace Enterprise.Accounting.GUI.ARAP.Journal
{
	public partial class BankFeeJournalForm
	{
		public bool IsNewUnsavedObject_ForTestOnly => IsNewUnsavedObject;

		public bool IsCurrentContextMatching_ForTestOnly => IsCurrentContextMatching;
	}
}

#endif
