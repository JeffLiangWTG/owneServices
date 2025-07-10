using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GUI.ARAP.Invoicing
{
	public partial class APInvoiceRequisitionForm : ZChildForm
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public APInvoiceRequisitionForm()
		{
			InitializeComponent();
		}

		public APInvoiceRequisitionForm(APTransactionHeaderCollectionHolder holder)
			: base(holder)
		{
			InitializeComponent();

			foreach (APInvoice invoice in Collection)
			{
				invoice.IsInRequisitionContext = true;
				invoice.AddWritableProperties(new string[] { AccTransactionHeaderSchema.AH_RequisitionDate.Name, AccTransactionHeaderSchema.AH_RequisitionStatus.Name });
				invoice.Validation.ValidateAH_RequisitionDate();
				invoice.Validation.ValidateAH_RequisitionStatus();
			}
		}

		public override string FormVerb
		{
			get { return string.Empty; }
		}

		protected APTransactionHeaderCollection Collection
		{
			get { return (BusinessEntity as APTransactionHeaderCollectionHolder).Collection; }
		}

#if DEBUG
		internal
#endif
		void OKButton_Click(object sender, EventArgs e)
		{
			bool hasErrors = false;
			foreach (APInvoice invoice in Collection)
			{
				hasErrors |= invoice.HasErrors;
			}
			if (hasErrors)
			{
				Globals.Message.ShowError(Res.GetString("184d6ddf-8b79-4568-b351-fbd0cafc5f37", "Please correct the errors before continuing."));
			}
			else
			{
				APTransactionHeaderCollectionHolder holder = BusinessEntity as APTransactionHeaderCollectionHolder;
				foreach (APInvoice invoice in Collection)
				{
					try
					{
						invoice.Factory.Save();
					}
					catch (ZSaveConcurrencyException)
					{
						Globals.Message.Show(Res.GetString("674F7E9B-7D18-424A-ADCA-043C38B9EE67", "The invoice whose transaction number is {0} cannot be saved because of another user has updated it beforehand. Please try this again later.", invoice.AH_TransactionNum));
					}
				}
				Close();
			}
		}

		void Cancel_Button_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
