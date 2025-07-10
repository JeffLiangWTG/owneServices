using Enterprise.Customs.CN.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CN.GUI
{
	public partial class ContractNumbersUserControl : ZUserControl
	{
		public ContractNumbersUserControl()
		{
			InitializeComponent();
		}

		void ContractNumbersEditButton_Click(object sender, System.EventArgs e)
		{
			if (CurrentDataItem is JobComInvoiceHeader item)
			{
				ContractNumberCollectionForm.ShowDialog(item.ContractNumbers);
			}
		}
	}
}
