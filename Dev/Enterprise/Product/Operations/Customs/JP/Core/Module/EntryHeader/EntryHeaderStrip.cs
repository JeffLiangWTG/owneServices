using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.JP.Module
{
	public class EntryHeaderStrip : WorkflowFilterStrip
	{
		protected override Control[] GetCurrentFilterControls(ModuleFilter currentModuleFilter)
		{
			Control[] result;

			if (currentModuleFilter is InspectionStatusFilter)
			{
				var control = new InspectionStatusFilterControl();
				ControlDpiScalingHelper.SetHeight(control, control.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
				PreferredHeight = control.Height;

				result = new Control[] { control };
			}
			else
			{
				result = base.GetCurrentFilterControls(currentModuleFilter);
			}

			return result;
		}
	}
}
