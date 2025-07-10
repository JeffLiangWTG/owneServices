using Enterprise.Accounting.GUI.ARAP.Invoicing;
using Enterprise.Accounting.GUI.CashBook;

namespace Enterprise.Accounting.GUI.Testing.CashBook
{
	public class DirectCashBookSubAccountsControlTest : SubAccountsControlTest
	{
		protected override string ExceptedSubAccountsGridBindingMemberCore => "Lines.SubAccounts";

		protected override SubAccountsControl GetExceptedSubAcountsControlCore() => new DirectCashBookSubAccountsControl();
	}
}
