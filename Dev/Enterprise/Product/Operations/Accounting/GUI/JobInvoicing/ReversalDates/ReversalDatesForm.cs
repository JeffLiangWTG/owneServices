using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	public partial class ReversalDatesForm : ZChildForm
	{
		public ReversalDatesForm()
		{
			InitializeComponent();
		}

		public ReversalDatesForm(TransactionHeaderCollection reversedInvoices, ChangeTransactionDatesBusinessObject changeTransactionDatesBusinessObject)
			: base(reversedInvoices)
		{
			InitializeComponent();

			ReversedInvoices = reversedInvoices;
			ReversedInvoices.Factory.SetContext(BusinessContext.ReverseDateForm);
			ChangeTransactionDatesBusinessObject = changeTransactionDatesBusinessObject;

			MakeRequiredFieldsEditableForReversing();
		}

		public readonly TransactionHeaderCollection ReversedInvoices;
		readonly ChangeTransactionDatesBusinessObject ChangeTransactionDatesBusinessObject;

		public override string FormCaption => Res.GetString("0a860db8-6455-4db8-ba18-3ca1525a84ab", "Back Date AR Transaction Reversals");

		void YesButtonOnYesNoCancelPanel_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Yes;
			Close();
		}

		void NoButtonOnYesNoCancelPanel_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.No;
			Close();
		}

		void CancelButtonOnYesNoCancelPanel_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
		{
			base.OnClosing(e);

			switch (DialogResult)
			{
				case DialogResult.None:
					e.Cancel = true;
					Globals.Message.ShowInformation(Res.GetString("90f7c14a-9c42-4420-85cf-1ee4767f0522", "You must choose one of the answers by pressing the buttons at the bottom of the window."));
					break;
				case DialogResult.Yes:
				case DialogResult.No:
					foreach (TransactionHeader invoice in ReversedInvoices)
					{
						JobInvoicingReverserValidationHelper.Validate(invoice);
					}

					if (BusinessEntity.HasErrors())
					{
						ShowErrorsDialog();
						e.Cancel = true;
					}
					break;
			}
		}

		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed(e);
			ReversedInvoices.Factory.RemoveContext(BusinessContext.ReverseDateForm);
		}

		protected void MakeRequiredFieldsEditableForReversing()
		{
			foreach (InvoicingBase invoice in ReversedInvoices)
			{
				MakeRequiredFieldsEditableForReversing(invoice);
			}
		}

		protected void MakeRequiredFieldsEditableForReversing(InvoicingBase invoice)
		{
			List<string> editableFields = new List<string>();

			if (AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.Value && invoice.UserAllowedToBackPost && !PostDate_ReadOnly)
			{
				editableFields.Add(TransactionHeader.Schema.AH_PostDate);
			}

			if (!InvoiceDate_ReadOnly)
			{
				editableFields.Add(TransactionHeader.Schema.AH_InvoiceDate);
			}

			invoice.ReadOnly = false;
			invoice.AddWritableProperties(editableFields.ToArray());
		}

		protected bool InvoiceDate_ReadOnly => ChangeTransactionDatesBusinessObject.InvoiceDateInfo.ReadOnly;

		protected bool PostDate_ReadOnly => ChangeTransactionDatesBusinessObject.PostDateInfo.ReadOnly;
	}
}
