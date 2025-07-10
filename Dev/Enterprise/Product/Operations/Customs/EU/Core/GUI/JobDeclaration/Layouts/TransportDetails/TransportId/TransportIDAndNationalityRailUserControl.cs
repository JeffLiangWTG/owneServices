using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class TransportIDAndNationalityRailUserControl : ZUserControl
	{
		public static class ControlNames
		{
			public const string TransportIDTextBox = nameof(TransportIDAndNationalityRailUserControl.TransportIDTextBox);
			public const string TransportNationalityFindBox = nameof(TransportIDAndNationalityRailUserControl.TransportNationalityFindBox);
		}

		public TransportIDAndNationalityRailUserControl()
		{
			InitializeComponent();
		}
	}
}
