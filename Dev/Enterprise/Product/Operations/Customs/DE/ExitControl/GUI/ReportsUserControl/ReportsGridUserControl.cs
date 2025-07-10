using Enterprise.Customs.EU.ExitControl.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.ExitControl.GUI
{
	public partial class ReportsGridUserControl : ZUserControl, IReportsGridUserControl
	{
		public ReportsGridUserControl()
		{
			InitializeComponent();
		}

		ZGrid IReportsGridUserControl.ReportsGrid => ReportsGrid;
	}
}
