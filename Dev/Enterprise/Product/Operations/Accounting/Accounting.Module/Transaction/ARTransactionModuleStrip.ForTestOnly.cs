#if DEBUG

using System.Windows.Forms;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module
{
	public partial class ARTransactionModuleStrip
	{
		public void DisplayImportedTransactionForm_ForTestOnly(Business.ARAP.Invoicing.InvoicingBase importedTransaction)
		{
			DisplayImportedTransactionForm(importedTransaction);
		}

		public MenuItem[] GetNewStandardMenuItems_ForTestOnly()
		{
			return GetNewStandardMenuItems();
		}

		public MultilingualString NewBulkReceiptsMenuText_ForTestOnly => NewBulkReceiptsMenuText;

		public MultilingualString BadDebtWriteOffMenuText_ForTestOnly => BadDebtWriteOffMenuText;

		public MenuItem[] GetActionMenuItems_ForTestOnly()
		{
			return GetActionMenuItems();
		}

		public MultilingualString SignInvoiceWithDigitalSignatureMenuText_ForTestOnly => SignInvoiceWithDigitalSignatureMenuText;

		public MultilingualString ReinstateDailyInvoiceDateIncrementingMenuText_ForTestOnly => ReinstateDailyInvoiceDateIncrementingMenuText;

		public MultilingualString NewCollectionBatchMenuText_ForTestOnly => NewCollectionBatchMenuText;

		public MenuItem[] GetOverrideDetailsMenuItems_ForTestOnly()
		{
			return GetOverrideDetailsMenuItems();
		}

		public MultilingualString OverrideInvoicePaymentReferenceCodeText_ForTestOnly => OverrideInvoicePaymentReferenceCodeText;

		public MultilingualString NewPeriodicInvoiceBulkMenuText_ForTestOnly => NewPeriodicInvoiceBulkMenuText;

		public MultilingualString SignElectronicInvoiceMenuText_ForTestOnly => SignElectronicInvoiceMenuText;

		public IZForm FormShownInTest_ForTestOnly
		{
			get { return formShownInTest; }
			set { formShownInTest = value; }
		}
	}
}

#endif
