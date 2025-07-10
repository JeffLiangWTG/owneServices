using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public partial class DeclarationDetailsUserControl : ZUserControl
	{
		public DeclarationDetailsUserControl()
		{
			InitializeComponent();
			AcceptanceDateEdit.ReadOnly = true;
		}
	}
}
