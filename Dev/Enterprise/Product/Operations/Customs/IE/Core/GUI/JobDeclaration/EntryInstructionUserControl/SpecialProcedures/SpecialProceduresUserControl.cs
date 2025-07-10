using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI
{
	public class SpecialProceduresUserControl : EU.GUI.SpecialProceduresUserControl
	{
		protected override IPanelLayoutProvider GetSpecialProceduresLayout() => new SpecialProceduresLayout();
	}
}
