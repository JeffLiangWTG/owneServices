using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Integration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module
{
	public partial class NewCashbookExchangeDiffBankAccountFilterControl : ZFilterStripControl, INewCashbookExchangeDiffBankAccountFilterControl
	{
		public NewCashbookExchangeDiffBankAccountFilterControl(IBusinessObjectCollection gridCollection)
			: base(gridCollection, new NewCashbookExchangeDiffBankAccountFilterBusinessObject())
		{
			InitializeComponent();
		}

		protected override int MaxFilterStripPanelHeight => ControlDpiScalingHelper.ScaleToCurrentDpiY(220);
	}
}
