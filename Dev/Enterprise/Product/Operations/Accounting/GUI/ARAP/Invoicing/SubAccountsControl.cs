using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.ARAP.Invoicing
{
	public partial class SubAccountsControl : ZUserControl
	{
		public SubAccountsControl()
		{
			InitializeComponent();

			((System.ComponentModel.ISupportInitialize)(BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(SubAccountsGrid)).BeginInit();
			BindingSource.DataSourceType = typeof(Business.Base.Transaction.TransactionHeaderWithLines);
			BindingSource.SetBindingMember(this.SubAccountsGrid, SubAccountsGridBindingMemberCore);
			((System.ComponentModel.ISupportInitialize)(SubAccountsGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(BindingSource)).EndInit();
		}

		protected virtual string SubAccountsGridBindingMemberCore => "FilteredLines.SubAccounts";

		protected ZArchitecture.ZGrid SubAccountsGrid;
	}
}

