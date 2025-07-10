using System;
using System.ComponentModel;
using Enterprise.Accounting.Business.AccountingVoucherPrint;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.AccountingVoucherPrinting
{
	public partial class ChinaJournalListingPrintForm : ZChildForm
	{
		ZButton GenerateButton;
		ZButton CloseButton;
		ZPanel MainPanel;
		ZDateEdit FromDateEdit;
		ZDateEdit EndDateEdit;

		ZPeriodEdit VoucherPeriodEdit;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		readonly Container components;
		protected ZGuidFindBox BranchGuidFindBox;

		public ChinaJournalListingPrintForm(ChinaJournalListingPrintWrapper printWrapper)
			: base(printWrapper)
		{ }

		public override string FormVerb
		{
			get { return ""; }
		}

		#region Event Handler

		void GenerateButton_Click(object sender, EventArgs e)
		{
			if (HasWrapperValidationError)
			{
				ShowErrorsDialog();
			}
			else if (CanPrintChinaJournalListingProceed())
			{
				PrintChinaJournalListingProceed();
			}
		}

#if DEBUG
		virtual
#endif
 protected void PrintChinaJournalListingProceed()
		{
			PrintWrapper.PrintChinaJournalListingDocument();
		}

		protected virtual bool HasWrapperValidationError
		{
			get
			{
				PrintWrapper.RunPreSaveValidation();
				return PrintWrapper.HasErrors;
			}
		}
		#endregion

		#region Implementation

		public ChinaJournalListingPrintWrapper PrintWrapper
		{
			get { return (ChinaJournalListingPrintWrapper)BusinessEntity; }
		}

#if DEBUG
		virtual
#endif
 protected bool CanPrintChinaJournalListingProceed()
		{
			if (PrintWrapper.GetWrapperCount() == 0)
			{
				Globals.Message.ShowInformation(Res.GetString("9C628CF7-D482-4806-84A4-6C98370444D7", "There are no accounting journal within given selection criteria."));
				return false;
			}
			return true;
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			BusinessEntity.HasChanges = false;
			Close();
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

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		protected override void ZForm_Closing(object sender, CancelEventArgs e) { }

		#endregion
	}
}

