using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class TransportIDAndNationalityInlandWaterwayUserControl : ZUserControl
	{
		public static class ControlNames
		{
			public const string TransportIDTextBox = nameof(TransportIDAndNationalityInlandWaterwayUserControl.TransportIDTextBox);
			public const string TransportNationalityFindBox = nameof(TransportIDAndNationalityInlandWaterwayUserControl.TransportNationalityFindBox);
		}

		public TransportIDAndNationalityInlandWaterwayUserControl()
		{
			InitializeComponent();
		}
	}
}
