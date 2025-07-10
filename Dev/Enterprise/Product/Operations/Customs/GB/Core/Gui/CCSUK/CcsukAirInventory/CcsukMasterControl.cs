using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	public partial class CcsukMasterControl : ZUserControl
	{
		public CcsukMasterControl()
		{
			InitializeComponent();
		}

		internal void ChangeParentFromHawbToMawb()
		{
			new ControlRebinder().Rebind(this, "MAWB.", "");
		}
	}
}
