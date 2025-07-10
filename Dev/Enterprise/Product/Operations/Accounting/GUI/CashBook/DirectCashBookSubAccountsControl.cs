using Enterprise.Accounting.GUI.ARAP.Invoicing;

namespace Enterprise.Accounting.GUI.CashBook
{
	public partial class DirectCashBookSubAccountsControl : SubAccountsControl
	{
		public DirectCashBookSubAccountsControl()
		{
			InitializeComponent();
		}

		protected override string SubAccountsGridBindingMemberCore => "Lines.SubAccounts";
	}
}

