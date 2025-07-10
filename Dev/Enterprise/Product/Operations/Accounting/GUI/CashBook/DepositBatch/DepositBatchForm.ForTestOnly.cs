#if DEBUG

using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.CashBook.DepositBatch
{
	public partial class DepositBatchForm
	{
		public void InitialiseForm_ForTestOnly()
		{
			InitialiseForm();
		}

		public ZLabel CancelReasonLabel_ForTestOnly
		{
			get { return CancelReasonLabel; }
			set { CancelReasonLabel = value; }
		}

		public ZButton BankUnSelectAllButton_ForTestOnly
		{
			get { return BankUnSelectAllButton; }
			set { BankUnSelectAllButton = value; }
		}

		public ZButton BankSelectAllButton_ForTestOnly
		{
			get { return BankSelectAllButton; }
			set { BankSelectAllButton = value; }
		}

		public ZLabel BankCodeLabel_ForTestOnly
		{
			get { return BankCodeLabel; }
			set { BankCodeLabel = value; }
		}

		public ZButton UnSelectAllButton_ForTestOnly
		{
			get { return UnSelectAllButton; }
			set { UnSelectAllButton = value; }
		}

		public ZButton SelectAllButton_ForTestOnly
		{
			get { return SelectAllButton; }
			set { SelectAllButton = value; }
		}

		public void PrintDepositSlip_ForTestOnly()
		{
			PrintDepositSlip();
		}
	}
}

#endif
