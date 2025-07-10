using System.Windows.Forms;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.AU.Module
{
	public class AUCustomsAirCargoModuleStrip : WorkflowFilterStrip
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
