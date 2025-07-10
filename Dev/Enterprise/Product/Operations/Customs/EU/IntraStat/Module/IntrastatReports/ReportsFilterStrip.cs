using System.Windows.Forms;
using Enterprise.Customs.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.Intrastat.Module
{
	public class ReportsFilterStrip : ZFilterStrip
	{
		protected override Control[] GetCurrentFilterControls(ModuleFilter currentModuleFilter)
		{
			return currentModuleFilter is PeriodFilter ? PeriodFilterGUIProvider.GetPeriodFilterControls(this, FilterControlBindingSource) : base.GetCurrentFilterControls(currentModuleFilter);
		}
	}
}
