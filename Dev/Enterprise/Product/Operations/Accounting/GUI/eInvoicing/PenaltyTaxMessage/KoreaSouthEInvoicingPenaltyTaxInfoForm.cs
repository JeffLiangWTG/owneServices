using System.Drawing;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.EInvoicing.PenaltyTaxMessage
{
	public partial class KoreaSouthEInvoicingPenaltyTaxInfoForm : ZChildForm
	{
		public KoreaSouthEInvoicingPenaltyTaxInfoForm(KoreaSouthEInvoicingPenaltyTaxInfo koreaSouthEInvoicingPenaltyTaxInfo) : base(koreaSouthEInvoicingPenaltyTaxInfo)
		{
			InitializeComponent();
			PictureBox.Image = SystemIcons.Information.ToBitmap();
		}

		public override string FormVerb => "";

		void CloseButton_Click(object sender, System.EventArgs e)
		{
			Close();
		}
	}
}
