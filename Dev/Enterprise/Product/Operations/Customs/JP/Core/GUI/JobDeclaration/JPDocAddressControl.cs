using Enterprise.MasterFiles.GUI;

namespace Enterprise.Customs.JP.GUI
{
	public class JPDocAddressControl : ZDocAddressControl
	{
		public bool ContactTabPageTabVisible
		{
			get => ContactTabPage.TabVisible;
			set => ContactTabPage.TabVisible = value;
		}
	}
}
