#if DEBUG

using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public partial class BaseInvoicingForm
	{
		public Business.ARAP.Invoicing.InvoicingBase Invoice_ForTestOnly => Invoice;

		public bool HasClosedJob_ForTestOnly => HasClosedJob;

		public ContinueWithSave ShowPreSaveDialogs_ForTestOnly()
		{
			return ShowPreSaveDialogs();
		}

		public void SetInvoiceHeaderDefaults_ForTestOnly()
		{
			SetInvoiceHeaderDefaults();
		}

		public void EditCostForm_FormClosed_ForTestOnly(object sender, System.Windows.Forms.FormClosedEventArgs e)
		{
			EditCostForm_FormClosed(sender, e);
		}

		public void ViewJobDetails_ForTestOnly(object sender, EventArgs e)
		{
			ViewJobDetails(sender, e);
		}

		public bool ShouldEnableApportionChargesButton_ForTestOnly => ShouldEnableApportionChargesButton;

		public string CaptionForInsertingIntoLabels_ForTestOnly => CaptionForInsertingIntoLabels;

		public void SaveAsIncomplete_ForTestOnly()
		{
			SaveAsIncomplete();
		}

		public void PreviewInvoice_Click_ForTestOnly(object sender, EventArgs e)
		{
			PreviewInvoice_Click(sender, e);
		}

		public bool IsSaveAsIncompletePossible_ForTestOnly => IsSaveAsIncompletePossible;

		public void SetSaveAsIncompleteAccessibility_ForTestOnly()
		{
			SetSaveAsIncompleteAccessibility();
		}

		public ZTabPage RelatedInvoicesTabPage_ForTestOnly
		{
			get { return RelatedInvoicesTabPage; }
			set { RelatedInvoicesTabPage = value; }
		}

		public ZGrid RelatedInvoicesGrid_ForTestOnly
		{
			get { return RelatedInvoicesGrid; }
			set { RelatedInvoicesGrid = value; }
		}

		public void RelatedInvoicesGridContextMenu_Popup_ForTestOnly(object sender, EventArgs e)
		{
			RelatedInvoicesGridContextMenu_Popup(sender, e);
		}

		public bool ShouldEnableBulkChargeImportButton_ForTestOnly => ShouldEnableBulkChargeImportButton;

		public bool ReOpenClosedJob_ForTestOnly()
		{
			return ReOpenClosedJob();
		}

		public void InvoiceForm_ShowJobChargesForImportEvent_ForTestOnly(object sender, EventArgs e)
		{
			InvoiceForm_ShowJobChargesForImportEvent(sender, e);
		}

		public ZCalcFindBox AH_OSExtraTaxAmountCalcEdit_ForTestOnly
		{
			get { return AH_OSExtraTaxAmountCalcEdit; }
			set { AH_OSExtraTaxAmountCalcEdit = value; }
		}

		public MultilingualString EditApportionmentMenuItemName_ForTestOnly => EditApportionmentMenuItemName;

		public ZTabPage SubAccountsTabPage_ForTestOnly
		{
			get { return SubAccountsTabPage; }
			set { SubAccountsTabPage = value; }
		}

		public void TransactionLinesGridContextMenu_Popup_ForTestOnly(object sender, EventArgs e)
		{
			TransactionLinesGridContextMenu_Popup(sender, e);
		}

		public ZLabel RestrictedLineChargesLabel_ForTestOnly
		{
			get { return RestrictedLineChargesLabel; }
			set { RestrictedLineChargesLabel = value; }
		}
	}
}

#endif
