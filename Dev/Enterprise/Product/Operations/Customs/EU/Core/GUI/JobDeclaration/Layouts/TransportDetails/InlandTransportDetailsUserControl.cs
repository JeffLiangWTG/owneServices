using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class InlandTransportDetailsUserControl : ZUserControl
	{
		public InlandTransportDetailsUserControl()
		{
			InitializeComponent();

			TransportNationalityCodeFindBox.AllowOutsideOfParent();
		}
	}
}
