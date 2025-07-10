using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI
{
	public sealed partial class ReportsGridUserControl : ZUserControl, IReportsGridUserControl
	{
		public ReportsGridUserControl()
		{
			InitializeComponent();
		}

		ZGrid IReportsGridUserControl.ReportsGrid => ReportsGrid;
	}
}
