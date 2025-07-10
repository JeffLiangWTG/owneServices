#if DEBUG

using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.CashBook.Transfer
{
	public partial class BankTransferForm
	{
		public void OnShown_ForTestOnly(EventArgs e)
		{
			OnShown(e);
		}

		public ZPanel TaxPanel_ForTestOnly
		{
			get { return TaxPanel; }
			set { TaxPanel = value; }
		}

		public ZArchitecture.ZTextBox BankChargeDescriptionTextBox_ForTestOnly
		{
			get { return BankChargeDescriptionTextBox; }
			set { BankChargeDescriptionTextBox = value; }
		}

		public ZDateEdit TaxDateEdit_ForTestOnly
		{
			get { return TaxDateEdit; }
			set { TaxDateEdit = value; }
		}
	}
}

#endif
