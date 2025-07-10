using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Customs.Universal.GUI;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.ASYCUDA.Module
{
	public class ASYCUDAManifestBillModuleStrip : WorkflowFilterStrip
	{
		protected override Control[] GetCurrentFilterControls(ModuleFilter currentModuleFilter)
		{
			Control filterControl = null;

			if (currentModuleFilter is DataGroupingRelatedFilter)
			{
				filterControl = new DataGroupingRelatedFilterControl();
			}
			else if (currentModuleFilter is CountryRelatedFilter)
			{
				filterControl = new CountryRelatedFilterControl();
			}

			if (filterControl != null)
			{
				ControlDpiScalingHelper.SetHeight(filterControl, filterControl.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
				PreferredHeight = filterControl.Height;

				return new[] { filterControl };
			}
			return base.GetCurrentFilterControls(currentModuleFilter);
		}
	}
}
