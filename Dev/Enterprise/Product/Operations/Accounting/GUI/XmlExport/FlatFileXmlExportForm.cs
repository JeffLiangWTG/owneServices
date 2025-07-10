using System;

namespace Enterprise.Accounting.GUI.XmlExport
{
	public partial class FlatFileXmlExportForm : XmlExportForm
	{
		protected FlatFileXmlExportForm()
		{
		}

		public FlatFileXmlExportForm(XmlExportGUIWrapper wrapper) : base(wrapper)
		{
			DisableAllUserInterfaceOptions();
			ExportButton.Click += new EventHandler(ExportButtonClicked);
		}

		public event EventHandler ExportButtonClick;

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		#region DisableAllUserInterfaceOptions

		protected virtual void DisableAllUserInterfaceOptions()
		{
			ARInvoiceCheckBox.Enabled = false;
			ARCreditNoteCheckBox.Enabled = false;
			ARAdjustmentNoteCheckBox.Enabled = false;
			APInvoiceCheckBox.Enabled = false;
			APCreditNoteCheckBox.Enabled = false;
			APAdjustmentNoteCheckBox.Enabled = false;
			OrganisationModuleButtonGrid.Enabled = false;
			NewExportBatchGroupBox.Enabled = false;
			WipPostingCheckBox.Enabled = false;
			WipReversalCheckBox.Enabled = false;
			AccrualPostingCheckBox.Enabled = false;
			AccrualReversingCheckBox.Enabled = false;
			ExcludeARJobRelatedCheckBox.Enabled = false;
			ExcludeARNonJobRelatedCheckBox.Enabled = false;
			ExcludeAPJobRelatedCheckBox.Enabled = false;
			ExcludeAPNonJobRelatedCheckBox.Enabled = false;
			FromZDateEdit.Enabled = false;
			ToZDateEdit.Enabled = false;
			DatesGroupBox.Enabled = false;

			TransactionNumbersGroupBox.Enabled = false;
			TransactionTypesGroupBox.Enabled = false;
			ExcludeTransactionsGroupBox.Enabled = false;
			DepartmentModuleButtonGrid.Enabled = false;
			BranchModuleButtonGrid.Enabled = false;

			JobModuleButtonGrid.Enabled = false;
			PeriodFromPeriodEdit.Enabled = false;
			PeriodToPeriodEdit.Enabled = false;

			ExportBatchNumberCalcEdit.Enabled = true;
		}

		#endregion

		protected virtual void ExportButtonClicked(object sender, EventArgs e)
		{
			if (ExportButtonClick != null)
			{
				ExportButtonClick(sender, e);
			}
		}

		protected override void Dispose(bool disposing)
		{
			ExportButton.Click -= new EventHandler(ExportButtonClicked);
			base.Dispose(disposing);
		}
	}
}

