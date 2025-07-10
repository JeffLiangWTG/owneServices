using Enterprise.Customs.GUI;

namespace Enterprise.Customs.AU.GUI
{
	public partial class AUCusUnderbondDetailsUserControl : CusUnderbondDetailsUserControl
	{
		public AUCusUnderbondDetailsUserControl()
		{
			InitializeComponent();
		}

		protected override CusOutturnUserControl GetCusOutturnUserControl()
		{
			return new AUCusOutturnUserControl();
		}
	}
}
