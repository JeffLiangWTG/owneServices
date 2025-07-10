using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public partial class ArrivalDeclarationDetailsUserControl : ZUserControl
	{
		public ArrivalDeclarationDetailsUserControl()
		{
			InitializeComponent();
			AcceptanceDateEdit.ReadOnly = true;
		}
	}
}
