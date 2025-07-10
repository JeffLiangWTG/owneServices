using System.Linq;
using Enterprise.Customs.BR.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI
{
	public partial class DuimpTaxRegimeUserControl : ZUserControl
	{
		public DuimpTaxRegimeUserControl()
		{
			InitializeComponent();
		}

		protected JobComInvoiceLine InvoiceLine => CurrentDataItem as JobComInvoiceLine;

		void AddLegalBaseButton_Click(object sender, System.EventArgs e)
		{
			if (InvoiceLine is JobComInvoiceLine invoiceLine)
			{
				var legalCode = invoiceLine.DuimpLegalBase;
				if (legalCode.IsEmpty)
				{
					Globals.Message.ShowError(Res.GetString("806A0BC6-1E7A-48C5-B685-AA1F76275F98", "No Legal Basis selected to add to grid, please select a Legal Basis."));
				}
				else if (invoiceLine.DuimpTaxRegimes.FindByLegalCode(legalCode).Any())
				{
					Globals.Message.Show(Res.GetString("FD07DC00-CC2C-46BD-BDF1-F87A512AD42E", "Legal Basis already added, please select other Legal Basis to be added."));
				}
				else
				{
					invoiceLine.AddDuimpTaxRegimes();
				}
			}
		}

		void LegalBaseGrid_RowsDeleting(object sender, RowsDeletingEventArgs e)
		{
			InvoiceLine?.RemoveDuimpTaxRegimes(e.Objects.Cast<DuimpTaxRegime>());
		}
	}
}
