using Enterprise.Customs.EU.ExitControl.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.ExitControl.GUI
{
	public sealed class ExitControlLayoutProvider : ExitControlUcc6LayoutProvider
	{
		protected override IPanelLayoutProvider HeaderDetailsPanelLayoutCore => new HeaderDetailsLayout();

		protected override  IReportsGridUserControl CreateReportsGridUserControlCore() => new ReportsGridUserControl();
	}
}
