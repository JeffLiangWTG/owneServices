using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI.GDM
{
	public class SummaryControl : EU.GUI.SummaryControl
	{
		protected override IPanelLayoutProvider GetGDMBasicLayoutCore() => new GDMBasicLayout();
	}
}
