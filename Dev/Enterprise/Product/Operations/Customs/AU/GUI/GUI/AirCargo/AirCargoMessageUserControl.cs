using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.AirCargo.GUI
{
	public partial class AirCargoMessageUserControl : ZUserControl
	{
		public AirCargoMessageUserControl()
		{
			InitializeComponent();
			OrderedMessagesBoundGrid.ReadOnly = true;
		}
	}
}
