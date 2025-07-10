#if DEBUG

namespace Enterprise.Accounting.Business.Base.Reversing
{
	public partial class GLJournalReversing
	{
		public void SetReversingDescriptionOnTransactions_ForTestOnly()
		{
			SetReversingDescriptionOnTransactions();
		}

		public void GenerateReverseTransactions_ForTestOnly()
		{
			GenerateReverseTransactions();
		}
	}
}

#endif
