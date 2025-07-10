using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class TransportIDAndNationalityInlandWaterwayENIUserControl : ZUserControl
	{
		public static class ControlNames
		{
			public const string TransportIDTextBox = nameof(TransportIDAndNationalityInlandWaterwayENIUserControl.TransportIDTextBox);
			public const string TransportNationalityFindBox = nameof(TransportIDAndNationalityInlandWaterwayENIUserControl.TransportNationalityFindBox);
		}

		public TransportIDAndNationalityInlandWaterwayENIUserControl()
		{
			InitializeComponent();
		}
	}
}
