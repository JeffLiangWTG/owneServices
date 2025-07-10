using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.AirCargo.GUI
{
	public partial class AirCargoDeclarationUserControl : ZUserControl
	{
		public AirCargoDeclarationUserControl()
		{
			InitializeComponent();
			InitializeShipmentUserControl();
		}

		protected void InitializeShipmentUserControl()
		{
			ShipmentUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			airCargoDeclarationTabPage.Controls.Add(ShipmentUserControl);
		}

		internal BaseAirCargoHouseUserControl ShipmentUserControl
		{
			get
			{
				if (fShipmentUserControl == null)
				{
					fShipmentUserControl = new CMRAirCargoHouseUserControl();
				}

				return fShipmentUserControl;
			}
		}
		BaseAirCargoHouseUserControl fShipmentUserControl;

		public void SetupPlugins()
		{
			if (!pluginsSetup)
			{
				ShipmentUserControl.SetupPlugins();
				pluginsSetup = true;
			}
		}

		bool pluginsSetup;
	}
}
