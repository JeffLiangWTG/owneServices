using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module
{
	public partial class OrgCollectionCallsModuleFilterStrip : ZFilterStrip
	{
		#region Filter Controls

		protected override Control[] GetCurrentFilterControls(ModuleFilter currentModuleFilter)
		{
			Control[] result;
			if (currentModuleFilter != null)
			{
				if (currentModuleFilter.GetType() == typeof(DaysAndAmountOverdueModuleFilter))
				{
					var overdueControl = new DaysAndAmountOverdueModuleFilterControl();
					PreferredHeight = overdueControl.Height + ControlDpiScalingHelper.OnePixel;

					result = new Control[] { overdueControl };
				}
				else
				{
					result = base.GetCurrentFilterControls(currentModuleFilter);
				}
			}
			else
			{
				result = base.GetCurrentFilterControls(currentModuleFilter);
			}
			return result;
		}

		#endregion
	}
}

