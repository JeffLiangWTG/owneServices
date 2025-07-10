using System;
using System.Linq;
using Enterprise.Accounting.Business.ARAP.CashAdvance;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	/// <summary>
	/// Summary description for AccrualPopupForm.
	/// </summary>
	public partial class JobChargesPopupForm : ZChildForm
	{
		public ZDisplayGrid JobChargesGrid;
		ZArchitecture.ZLabel SelectJobChargesLabel;
		ZButton ImportButton;
		ZButton CancelImportButton;
		ZGroupBox OptionsGroupBox;
		ZCheckBox IncludeChargesForCreditorsWithTheSameAPSettlementGroupCheckBox;
		ZCheckBox IncludeChargesForAllOtherCreditorsCheckBox;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		readonly System.ComponentModel.Container components;

		public JobChargesPopupForm(JobChargesImporter importer)
			: base(importer)
		{
			this.Importer = importer;
		}

		readonly JobChargesImporter Importer;

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		public override string FormVerb
		{
			get { return ""; }
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!AccountingMasterFilesUtils.IsTaxBranchApplicable)
			{
				JobChargesGrid.RemoveFromAvailableColumns(AutoJobCharge.Schema.JR_GB_CostTaxBranch);
			}
		}

		void CancelImportButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		void ImportButton_Click(object sender, EventArgs e)
		{
			try
			{
				Importer.SetJobChargesToImport(JobChargesGrid.SelectedElements.Cast<Charge>());
				Close();
			}
			catch (CannotGenerateCashAdvanceJournalException ex)
			{
				Globals.Message.ShowError(ex.Message);
			}
		}
	}
}

