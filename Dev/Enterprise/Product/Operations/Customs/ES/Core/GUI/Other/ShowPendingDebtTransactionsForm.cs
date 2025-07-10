using System;
using Enterprise.Customs.ES.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI
{
	public partial class ShowPendingDebtTransactionsForm : ZChildForm
	{
		[Obsolete("This constructor is just for the designer")]
		ShowPendingDebtTransactionsForm()
			: base()
		{
			InitializeComponent();
		}

		public ShowPendingDebtTransactionsForm(CusGuaranteeLineTransactionCollection transactions)
			: base(transactions)
		{
			InitializeComponent();
		}
	}
}
