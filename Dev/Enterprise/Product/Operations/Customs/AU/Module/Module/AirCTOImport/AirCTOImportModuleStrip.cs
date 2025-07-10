using System.Windows.Forms;
using Enterprise.Accounting.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Module
{
	public class AirCTOImportModuleStrip : ZFilterStrip
	{
		protected override Control[] GetCurrentFilterControls(ModuleFilter currentModuleFilter)
		{
			Control[] result;

			if (currentModuleFilter is IJobManagementAmountFilter)
			{
				result = new Control[] { (Control)((IJobManagementAmountFilter)currentModuleFilter).GetFilterControl() };
			}
			else
			{
				result = base.GetCurrentFilterControls(currentModuleFilter);
			}

			return result;
		}
	}
}
