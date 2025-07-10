using System;
using Enterprise.Accounting.Business.CashBook.DirectReceipt;

namespace Enterprise.Accounting.GUI.CashBook
{
	public partial class DirectReceiptForm : DirectCashBookBaseForm
	{
		public DirectReceiptForm(DirectReceipt directReceiptBizO)
			: base(directReceiptBizO)
		{
		}

		#region Override

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			ChequeBookFindBox.Visible = false;
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();

			CashBookTransactionTabPage1.RunWhenBindingOrFirstShown(delegate
			{
				CashBookLineBoundGrid.ColumnLayoutContext = CashBookLineGridContext.Receipt;
			});
		}

		#endregion
	}
}

