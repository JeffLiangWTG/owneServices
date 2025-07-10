namespace Enterprise.Customs.AU.AirCargo.GUI
{
	public partial class BaseACAMAWBUserControl : AirCargoHAWBProviderContainerControl
	{
		public BaseACAMAWBUserControl()
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
	}
}
