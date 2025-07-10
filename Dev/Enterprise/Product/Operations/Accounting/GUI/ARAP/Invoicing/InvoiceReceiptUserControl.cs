using System.ComponentModel;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	/// <summary>
	/// Summary description for InvoicePaymentUserControl.
	/// </summary>
	public partial class InvoiceReceiptUserControl : ZUserControl
	{
		public InvoiceReceiptUserControl()
		{
			fReadOnly = false;
			InitializeComponent();
			SetDataSourceBinding(ReceiptPaymentAH_ChequeOrReferenceTextbox.GetExtension<LabelCaptionRenderer>(), "Caption", "ReceiptPaymentAH_ChequeReferenceLabel", true);
		}

		bool fReadOnly;
		public ZGroupBox ReceiptPaymentDetailsGroupbox;
		public ZTextBox ReceiptPaymentAH_ChequeOrReferenceTextbox;
		public ZTextBox ReceiptPaymentAH_DrawerBranchTextbox;
		public ZTextBox ReceiptPaymentAH_DrawerBankTextbox;
		public ZTextBox ReceiptPaymentAH_ChequeDrawerTextbox;
		public ZGuidFindBox ReceiptPaymentAH_ABFindbox;
		public ZDateEdit ReceiptPaymentAH_InvoiceDateEdit;
		public ZDateEdit ReceiptPaymentAH_PostDateEdit;
		public ZDropEdit ReceiptPaymentAH_ReceiptTypeDropEdit;
		public ZTextBox ReceiptPaymentAH_DescTextbox;

		[Browsable(true), Category(ZGUIConstants.DesignerCategory)]
		public bool ReadOnly
		{
			get { return fReadOnly; }
			set
			{
				if (fReadOnly != value)
				{
					fReadOnly = value;
				}
			}
		}
	}
}

