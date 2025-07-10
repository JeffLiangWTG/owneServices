using Enterprise.Accounting.GUI.ARAP.Invoicing;

namespace Enterprise.Accounting.GUI.GeneralLedger.GLJournals
{
	public partial class GLJournalSubAccountsControl : SubAccountsControl
	{
		public GLJournalSubAccountsControl() : base()
		{
			InitializeComponent();
		}

		protected override string SubAccountsGridBindingMemberCore => "GLJournalLines.SubAccounts";
	}
}

