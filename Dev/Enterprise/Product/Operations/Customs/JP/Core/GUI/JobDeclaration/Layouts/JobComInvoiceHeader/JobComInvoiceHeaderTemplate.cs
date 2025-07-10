using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.GUI
{
	public partial class JobComInvoiceHeaderTemplate : ZUserControl
	{
		public JobComInvoiceHeaderTemplate()
		{
			InitializeComponent();
			SetupComponents();
		}

		void SetupComponents()
		{
			GrossWeightCalcDropEdit.AllowNegative = false;
			NetWeightCalcDropEdit.AllowNegative = false;
			InvoiceAmountConvertToLocalCurrencyControl.AllowNegative = false;
		}
	}
}
