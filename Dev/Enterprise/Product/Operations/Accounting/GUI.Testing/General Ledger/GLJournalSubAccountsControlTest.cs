using Enterprise.Accounting.GUI.ARAP.Invoicing;
using Enterprise.Accounting.GUI.Testing;

namespace Enterprise.Accounting.GUI.GeneralLedger.GLJournals.Testing
{
	public class GLJournalSubAccountsControlTest : SubAccountsControlTest
	{
		protected override string ExceptedSubAccountsGridBindingMemberCore => "GLJournalLines.SubAccounts";

		protected override SubAccountsControl GetExceptedSubAcountsControlCore() => new GLJournalSubAccountsControl();
	}
}
