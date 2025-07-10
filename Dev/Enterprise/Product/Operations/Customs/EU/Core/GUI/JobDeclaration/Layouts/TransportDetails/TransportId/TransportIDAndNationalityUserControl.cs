using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class TransportIDAndNationalityUserControl : ZUserControl
	{
		public static class ControlNames
		{
			public const string TransportIDTextBox = nameof(TransportIDAndNationalityUserControl.TransportIDTextBox);
			public const string TransportNationalityFindBox = nameof(TransportIDAndNationalityUserControl.TransportNationalityFindBox);
		}

		public TransportIDAndNationalityUserControl()
		{
			InitializeComponent();
		}
	}
}
