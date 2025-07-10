using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.ARAP
{
	public partial class TransactionsPendingAllocationForm : ZForm
	{
		public TransactionsPendingAllocationForm()
		{
		}

		public TransactionsPendingAllocationForm(TransactionsPendingAllocation bizO) : base(bizO)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtons);
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}
	}
}
