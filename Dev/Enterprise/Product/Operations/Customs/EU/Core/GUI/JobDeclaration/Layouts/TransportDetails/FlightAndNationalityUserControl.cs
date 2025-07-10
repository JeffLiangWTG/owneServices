using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class FlightAndNationalityUserControl : ZUserControl
	{
		public FlightAndNationalityUserControl()
		{
			InitializeComponent();
		}

		public static class ControlNames
		{
			public const string FlightNumberTextBox = nameof(FlightNumberTextBox);
			public const string TransportNationalityFindBox = nameof(TransportNationalityFindBox);
		}
	}
}
