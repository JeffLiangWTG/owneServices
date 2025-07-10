using System.Windows.Forms;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.EU.Module
{
	public class JobDeclarationModuleStrip : Customs.Module.JobDeclarationModuleStrip
	{
		protected override Control[] GetCurrentFilterControls(ModuleFilter currentModuleFilter)
		{
			if (currentModuleFilter is CustomsOfficeFilter)
			{
				return CustomsOfficeFilterGUIProvider.GetCustomsOfficeFilterControls(this, FilterControlBindingSource);
			}
			else
			{
				return base.GetCurrentFilterControls(currentModuleFilter);
			}
		}
	}
}
