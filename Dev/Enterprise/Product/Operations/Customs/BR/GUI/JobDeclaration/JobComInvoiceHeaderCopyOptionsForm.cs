using Enterprise.Customs.BR.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI
{
	public partial class JobComInvoiceHeaderCopyOptionsForm : ZChildForm
	{
		public JobComInvoiceHeaderCopyOptionsForm(JobComInvoiceHeaderCopyOptions importLicenseCopyingInvoicesObject) : base(importLicenseCopyingInvoicesObject)
		{
			InitializeComponent();
		}

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

		void ConfirmButton_Click(object sender, System.EventArgs e)
		{
			DialogResult = System.Windows.Forms.DialogResult.OK;
			Close();
		}

		void CancelButton2_Click(object sender, System.EventArgs e)
		{
			DialogResult = System.Windows.Forms.DialogResult.Cancel;
			Close();
		}
	}
}
