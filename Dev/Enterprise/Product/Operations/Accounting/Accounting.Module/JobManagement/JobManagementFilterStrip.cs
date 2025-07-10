using System.Windows.Forms;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module
{
	public partial class JobManagementFilterStrip : ZFilterStrip
	{
		protected override Control[] GetCurrentFilterControls(ModuleFilter currentModuleFilter)
		{
			if (currentModuleFilter is JobManagementAmountFilter)
			{
				return new Control[] { new JobManagementAmountFilterControl() };
			}
			else
			{
				return base.GetCurrentFilterControls(currentModuleFilter);
			}
		}
	}
}
