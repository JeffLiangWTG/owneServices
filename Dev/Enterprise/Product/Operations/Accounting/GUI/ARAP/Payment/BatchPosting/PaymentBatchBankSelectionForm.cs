using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.ARAP
{
	public partial class PaymentBatchBankSelectionForm : ZChildForm
	{
		public PaymentBatchBankSelectionForm() : base()
		{
		}

		public PaymentBatchBankSelectionForm(BankAccountSelectionObject bankAccountSelectionObject)
			: base(bankAccountSelectionObject)
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		void SaveButton_Click(object sender, EventArgs e)
		{
			FormClose();
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			if (BusinessEntity != null)
			{
				((BankAccountSelectionObject)BusinessEntity).SetNothingSelected();
			}
			FormClose();
		}

		void FormClose()
		{
			this.Close();
		}
	}
}

