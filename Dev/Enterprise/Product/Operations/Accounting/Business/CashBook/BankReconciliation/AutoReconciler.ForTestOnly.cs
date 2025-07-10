#if DEBUG

using CargoWise.Types;

namespace Enterprise.Accounting.Business.CashBook
{
	public partial class AutoReconciler
	{
		public bool DoesRefMatchCore_ForTestOnly(ZString ref1, ZString ref2)
		{
			return DoesRefMatchCore(ref1, ref2);
		}

		public void MatchStatement_ForTestOnly(Base.AccStatement.Statement statementToMatch, Base.AccStatement.StatementCollection bankStatementsToMatch, BankReconTransCollection transactionsToMatch, bool ignoreRef)
		{
			MatchStatement(statementToMatch, bankStatementsToMatch, transactionsToMatch, ignoreRef);
		}
	}
}

#endif
