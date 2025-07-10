using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.IssueManager.Module
{
	public partial class ErrorLogSatusFilterStrip : ZFilterStrip
	{
		protected override Control[] GetCurrentFilterControls(ModuleFilter currentModuleFilter)
		{
			Control[] result;
			if (currentModuleFilter is ErrorLogStatusFilter)
			{
				ErrorLogStatusFilterControl statusControl = new ErrorLogStatusFilterControl();
				PreferredHeight = statusControl.Height + ControlDpiScalingHelper.OnePixel;

				result = new Control[] { statusControl };
			}
			else
			{
				result = base.GetCurrentFilterControls(currentModuleFilter);
			}
			return result;
		}

		internal Control[] InternalGetCurrentFilterControls(ModuleFilter currentModuleFilter)
		{
			return GetCurrentFilterControls(currentModuleFilter);
		}
	}
}
