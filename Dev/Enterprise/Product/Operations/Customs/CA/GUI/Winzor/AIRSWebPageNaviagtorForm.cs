using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI
{
	public partial class AIRSWebPageNaviagtorForm : ZChildForm
	{
		public AIRSWebPageNaviagtorForm(AIRSWebpageNavigator navigator)
			: base(navigator)
		{
			this.CaptionRenderingEnabled = true;
		}
	}
}
