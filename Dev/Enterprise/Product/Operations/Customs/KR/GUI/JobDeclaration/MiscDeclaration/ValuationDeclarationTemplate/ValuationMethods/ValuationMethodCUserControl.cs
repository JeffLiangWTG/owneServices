using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public partial class ValuationMethodCUserControl : ZUserControl
	{
		public ValuationMethodCUserControl()
		{
			InitializeComponent();
			question5To7GroupBoxUserControl.ChangeBindingTo5SM();
		}
	}
}
